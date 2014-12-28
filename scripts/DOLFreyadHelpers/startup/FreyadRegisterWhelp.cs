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

using DOL.Events;
using DOL.GS;
using DOL.Database;
using DOL.GS.ServerProperties;
using DOL.AI.Brain;

namespace DOLFreyadHelpers
{
	/// <summary>
	/// Register Whelp Script, Started to listen on server entering game and "asking" them to register character.
	/// </summary>
	public static class FreyadRegisterWhelp
	{
		#region ServerProperties
		/// <summary>
		/// Enable or Disable the Freyad Helpers Register Scripts
		/// </summary>
		[ServerProperty("freyadhelpers", "helper_register_account_enable", "Enable or disable the Register Whelp Helper to lead Player for registration", false)]
		public static bool HELPER_REGISTER_ACCOUNT_ENABLE;

		/// <summary>
		/// Enable or Disable the Freyad Helpers Register Whelp Spawning
		/// </summary>
		[ServerProperty("freyadhelpers", "helper_register_whelp_enable", "Enable or disable the Register Whelp Helper to lead Player for registration", true)]
		public static bool HELPER_REGISTER_WHELP_ENABLE;

		/// <summary>
		/// Delay for Register Whelp Spawn
		/// </summary>
		[ServerProperty("freyadhelpers", "helper_register_whelp_spawndelay", "Delay in Millisecond after player Zoning for Register Whelp Spawning (should take zoning time into account)", 30000)]
		public static int HELPER_REGISTER_WHELP_SPAWNTIME;

		/// <summary>
		/// Delay for Register Whelp Unspawn (Lifetime)
		/// </summary>
		[ServerProperty("freyadhelpers", "helper_register_whelp_unspawndelay", "Lifetime in Millisecond for Register Whelp Duration (Idle Delay)", 300000)]
		public static int HELPER_REGISTER_WHELP_UNSPAWNDELAY;
		#endregion

		public const string REGISTER_WHELP_ACCOUNT_TAG = "REGISTER_WHELP_ACCOUNT_TAG";
		public const string REGISTER_WHELP_TIMER_TAG = "REGISTER_WHELP_TIMER_TAG";
		public const string REGISTER_WHELP_SPAWN_TAG = "REGISTER_WHELP_SPAWN_TAG";
		
		/// <summary>
		/// On Script Loading Listen to World Events.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			if (HELPER_REGISTER_ACCOUNT_ENABLE)
			{
				GameEventMgr.AddHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(OnRegionEntering));
				GameEventMgr.AddHandler(GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRegionEntering));
				GameEventMgr.AddHandler(GamePlayerEvent.LevelUp, new DOLEventHandler(OnRegionEntering));
			}
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnLoaded(DOLEvent e, object sender, EventArgs args)
		{
			try
			{
				GameEventMgr.RemoveHandler(GamePlayerEvent.GameEntered, new DOLEventHandler(OnRegionEntering));
				GameEventMgr.RemoveHandler(GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRegionEntering));
				GameEventMgr.RemoveHandler(GamePlayerEvent.LevelUp, new DOLEventHandler(OnRegionEntering));
			}
			catch
			{
			}
		}
		
		/// <summary>
		/// Trigger Spawn on Player Entering World
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OnRegionEntering(DOLEvent e, object sender, EventArgs args)
		{
			// Get Player Changing Region
			if (sender is GamePlayer)
			{
				GamePlayer player = (GamePlayer)sender;
				
				if (player == null)
					return;
				
				// Check it's Game Account External Registration or make a default object.
				AccountXHelperRegister acc = player.TempProperties.getProperty<AccountXHelperRegister>(REGISTER_WHELP_ACCOUNT_TAG);
				
				if (acc == null)
				{
					acc = GameServer.Database.FindObjectByKey<AccountXHelperRegister>(player.Client.Account.Name);
					
					if (acc == null)
					{
						acc = new AccountXHelperRegister();
						acc.AccountName = player.Client.Account.Name;
						acc.Validated = false;
						acc.Token = string.Empty;
						acc.ExternalAccount = string.Empty;
						GameServer.Database.AddObject(acc);
					}
					
					player.TempProperties.setProperty(REGISTER_WHELP_ACCOUNT_TAG, acc);
				}
				
				if (acc.Validated == false)
				{
					var currentRegion = player.CurrentRegion;
					
					// Check if the Timer isn't running
					var timer = player.TempProperties.getProperty<RegionTimerAction<GamePlayer>>(REGISTER_WHELP_TIMER_TAG, null);
					
					if (timer != null && timer.IsAlive)
						return;
					
					// Our player isn't fully validated, spawn the registration "Helper"
					player.TempProperties.setProperty(REGISTER_WHELP_TIMER_TAG,
					                                  new RegionTimerAction<GamePlayer>(player, HELPER_REGISTER_WHELP_SPAWNTIME, pl =>
					                                                                    {
					                                                                    	SpawnRegisterHelper(pl, acc);
					                                                                    	pl.TempProperties.removeProperty(REGISTER_WHELP_TIMER_TAG);
					                                                                    }));
				}
			}
		}
		
		/// <summary>
		/// Spawn a Register Helper on Targeted Player with current Registration Status...
		/// </summary>
		/// <param name="player"></param>
		/// <param name="account"></param>
		public static void SpawnRegisterHelper(GamePlayer player, AccountXHelperRegister account)
		{
			if (HELPER_REGISTER_WHELP_ENABLE == false)
				return;
			
			// Still have a pet
			if (player.TempProperties.getProperty<RegisterWhelpNPC>(REGISTER_WHELP_SPAWN_TAG, null) != null)
				return;
						
			RegisterWhelpNPC npc = new RegisterWhelpNPC(player, account);
			FollowOwnerBrain brain = new FollowOwnerBrain(player);
			Point2D spawnloc = player.GetPointFromHeading(player.Heading, 64);
			
			brain.IsMainPet = false;

			npc.Name = "Register Whelp";
			npc.GuildName = "Please Register !";
			npc.Model = 678;
			npc.X = spawnloc.X;
			npc.Y = spawnloc.Y;
			npc.Z = player.Z;
			npc.CurrentRegion = player.CurrentRegion;
			npc.Heading = (ushort)((player.Heading + 2048) % 4096);
			npc.CurrentSpeed = 0;
			npc.Level = 99;
			npc.Size = 50;

			npc.SetOwnBrain(brain);
			npc.AddToWorld();
			
			// Register Pet Death for Owner
			player.TempProperties.setProperty(REGISTER_WHELP_SPAWN_TAG, npc);
			GameEventMgr.AddHandler(npc, GameLivingEvent.RemoveFromWorld, new DOLEventHandler(PetOwnerUnregister));
			// Start Death Timer
			new RegionTimerAction<GameNPC>(npc, HELPER_REGISTER_WHELP_UNSPAWNDELAY, obj => obj.Die(null));
		}
		
		/// <summary>
		/// Method for Unregistering Pet Owner on Whelp Unspawning
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void PetOwnerUnregister(DOLEvent e, object sender, EventArgs args)
		{
			if (sender is RegisterWhelpNPC)
			{
				try
				{
					((RegisterWhelpNPC)sender).Player.TempProperties.removeProperty(REGISTER_WHELP_SPAWN_TAG);
				}
				finally
				{
					GameEventMgr.RemoveHandler(sender, GameLivingEvent.RemoveFromWorld, new DOLEventHandler(PetOwnerUnregister));
				}
			}
		}
	}
}
