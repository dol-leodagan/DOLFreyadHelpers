/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;

using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOLFreyadHelpers
{
	/// <summary>
	/// Helper Whelp NPC, "gentle" companion pet following player to grant help or ask for certain accomplishment.
	/// </summary>
	public class RegisterWhelpNPC : GameNPC
	{
		private const int AVERAGE_ANIMATION_DELAY = 30000;
		private const int SPELL_EFFECT_ID = 207;
		
		private DOLEventHandler m_playerOutEvent;
		private DOLEventHandler m_helperUnspawnEvent;
		private DOLEventHandler m_helperInteractEvent;
		private GamePlayer m_player;
		
		public GamePlayer Player {
			get { return m_player; }
		}
		private AccountXHelperRegister m_account;
		private RegionTimerAction<RegisterWhelpNPC> m_animationTimer;

		/// <summary>
		/// Default Constructor Needing Player Owner and Account Object for Walkthrough to Registration.
		/// </summary>
		/// <param name="player">Player Followed</param>
		/// <param name="acc">External Account Linker</param>
		public RegisterWhelpNPC(GamePlayer player, AccountXHelperRegister acc)
			: base()
		{
			m_player = player;
			m_account = acc;
			m_playerOutEvent = new DOLEventHandler(PlayerOut);
			m_helperUnspawnEvent = new DOLEventHandler(HelperUnspawn);
			m_helperInteractEvent = new DOLEventHandler(HelperInteract);
			
			// Player missing
			GameEventMgr.AddHandler(m_player, GamePlayerEvent.Quit, m_playerOutEvent);
			GameEventMgr.AddHandler(m_player, GamePlayerEvent.Delete, m_playerOutEvent);
			GameEventMgr.AddHandler(m_player, GamePlayerEvent.Released, m_playerOutEvent);
			GameEventMgr.AddHandler(m_player, GamePlayerEvent.Linkdeath, m_playerOutEvent);
			GameEventMgr.AddHandler(m_player, GamePlayerEvent.RegionChanged, m_playerOutEvent);
			
			// Npc Unspawn
			GameEventMgr.AddHandler(this, GameLivingEvent.RemoveFromWorld, m_helperUnspawnEvent);
			GameEventMgr.AddHandler(this, GameLivingEvent.Delete, m_helperUnspawnEvent);
			GameEventMgr.AddHandler(this, GameLivingEvent.Dying, m_helperUnspawnEvent);
			
			// Npc Interact
			GameEventMgr.AddHandler(this, GamePlayerEvent.Interact, m_helperInteractEvent);
			
			// Npc init
			Flags |= eFlags.PEACE;
		}
		
		/// <summary>
		/// On Player getting Out or Out of Range.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PlayerOut(DOLEvent e, object sender, EventArgs args)
		{
			// Try to respawn on release in same region
			if (e == GamePlayerEvent.Released)
			{
				GamePlayer player = sender as GamePlayer;
				
				if (player != null && player == m_player && m_player.CurrentRegion == CurrentRegion)
					FreyadRegisterWhelp.OnRegionEntering(e, m_player, null);
			}

			Die(null);
		}
		
		/// <summary>
		/// Unspawning Event
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void HelperUnspawn(DOLEvent e, object sender, EventArgs args)
		{
			m_animationTimer.Stop();
			m_animationTimer = null;
			RemoveAllHandlers();
		}
		
		/// <summary>
		/// Tells player how to Register when Interacted With...
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void HelperInteract(DOLEvent e, object sender, EventArgs args)
		{
			// Only reply to Player Owner !
			if (args is InteractEventArgs && ((InteractEventArgs)args).Source != m_player)
				return;
			
			if (m_account.Validated)
				Die(null);
			
			string status;
			// Invite Player to Finish it's registration.
			if (string.IsNullOrEmpty(m_account.ExternalAccount))
			{
				// Need to target an External Account
				status = string.Format("{0}{1}", "You didn't registered any Website Account !",
				                                     "\nPlease create a Website Account as described and use the /register command !");
			}
			else
			{
				// Need to Validate an External Account
				status = string.Format("{0}{1}{2}{3}", "Your registered Website Account isn't validated! Currently set to :\n",
				                                         m_account.ExternalAccount,
				                                         "\n\nPlease use /register command to enter your Validation Token available from Website Gaming Account Page.",
				                                         "\nIf you registered a wrong Website Account Name you can use /register to enter an other Account Name, a new registration Token will be created !");
			}
			
			// Welcome String
			string welcome = string.Format("Hello {0} and Welcome to Freyad !{1}{2}{3}{4}{5}{6}{7}{8}{9}", m_player.Name,
			                               "\nThis server is meant for longterm play and needs you to register on the Website for easier Support and other Features...",
			                               "\n\nBrowse to https://daoc.freyad.net and Create an Account on Forum or Login an existing One.",
			                               "\n\nThen Register your current Playing Account to your Website account using command:",
			                               "\n/register \"Website Account Name\"",
			                               "\n\nA Validation Token will then be displayed in your Gaming Account:",
			                               "\nhttps://daoc.freyad.net/account",
			                               "\n\nYou need to enter your validation token using command:",
			                               "\n/register \"Validation Token\"\n\n", status);
			
			SayTo(m_player, welcome, false);

		}
		
		/// <summary>
		/// Remove All Event Handlers
		/// </summary>
		private void RemoveAllHandlers()
		{
			// Player missing
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Quit, m_playerOutEvent);
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Delete, m_playerOutEvent);
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Released, m_playerOutEvent);
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.Linkdeath, m_playerOutEvent);
			GameEventMgr.RemoveHandler(m_player, GamePlayerEvent.RegionChanged, m_playerOutEvent);
			
			// Npc Unspawn
			GameEventMgr.RemoveHandler(this, GameLivingEvent.RemoveFromWorld, m_helperUnspawnEvent);
			GameEventMgr.RemoveHandler(this, GameLivingEvent.Delete, m_helperUnspawnEvent);
			GameEventMgr.RemoveHandler(this, GameLivingEvent.Dying, m_helperUnspawnEvent);
			
			// Npc Interact
			GameEventMgr.RemoveHandler(this, GamePlayerEvent.Interact, m_helperInteractEvent);
		}
		
		/// <summary>
		/// Start Animation When Added to World !
		/// </summary>
		/// <returns></returns>
		public override bool AddToWorld()
		{
			if (base.AddToWorld())
			{
				// Animation
				if (m_animationTimer != null)
					m_animationTimer.Stop();
				
				m_animationTimer = new RegionTimerAction<RegisterWhelpNPC>(this, obj => AnimationCallback(obj));
				m_animationTimer.Start(1);
				return true;
			}

			return false;
		}
		
		/// <summary>
		/// Animation Timer
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private int AnimationCallback(RegisterWhelpNPC source)
		{
			if (ObjectState != eObjectState.Active)
				return AVERAGE_ANIMATION_DELAY;
			
			if (m_player == null || m_player.ObjectState != eObjectState.Active)
				Die(null);
			
			if (m_account.Validated)
				Die(null);
			
			// Play a Spell or Play an Attack.
			switch (Util.Random(2))
			{
				case 0:
				// Attack Animation
				AttackData ad = new AttackData();
				ad.Attacker = this;
				ad.Target = m_player;
				ad.AttackResult = eAttackResult.Fumbled;
				ad.AnimationId = -1;
				ad.AttackType = AttackData.eAttackType.MeleeOneHand;
				this.ShowAttackAnimation(ad, null);
				break;
				
				case 1:
				// Spell Animation
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player == null)
						continue;
					
					player.Out.SendSpellCastAnimation(this, SPELL_EFFECT_ID, 30);
				}
				break;
				
				default:
				if (string.IsNullOrEmpty(m_account.ExternalAccount))
				{
					// Invite...
					if (Util.RandomBool())
						SayTo(m_player, eChatLoc.CL_ChatWindow, "Hey ! You should definitely Try to register on Freyad !");
					else
						SayTo(m_player, eChatLoc.CL_ChatWindow, "I would stop following you if you Registered on Freyad...");
				}
				else
				{
					// Validate...
					if (Util.RandomBool())
						SayTo(m_player, eChatLoc.CL_ChatWindow, "Don't forget to validate your registered Account !");
					else
						SayTo(m_player, eChatLoc.CL_ChatWindow, "I would stop following you if you Validated your Account...");
				}
				break;
			}
			
			// Randomize some trigger time...
			return AVERAGE_ANIMATION_DELAY - (Util.Random(AVERAGE_ANIMATION_DELAY) >>  1);
		}
	}
}
