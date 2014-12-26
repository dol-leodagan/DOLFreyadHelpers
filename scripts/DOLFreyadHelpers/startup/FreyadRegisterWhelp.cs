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
		[ServerProperty("freyadhelpers", "helper_register_whelp_enable", "Enable or disable the Register Whelp Helper to lead Player for registration", false)]
		public static bool helper_register_whelp_enable;

		/// <summary>
		/// Delay for Register Whelp Spawn
		/// </summary>
		[ServerProperty("freyadhelpers", "helper_register_whelp_spawndelay", "Delay in Millisecond after player Zoning for Register Whelp Spawning (should take zoning time into account)", 30000)]
		public static int helper_register_whelp_spawntime;

		/// <summary>
		/// Delay for Register Whelp Unspawn (Lifetime)
		/// </summary>
		[ServerProperty("freyadhelpers", "helper_register_whelp_unspawndelay", "Lifetime in Millisecond for Register Whelp Duration (Idle Delay)", 600000)]
		public static int helper_register_whelp_unspawndelay;
		#endregion

		private const string REGISTER_WHELP_ACCOUNT_TAG = "REGISTER_WHELP_ACCOUNT_TAG";
		
		/// <summary>
		/// On Script Loading Listen to World Events.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[ScriptLoadedEvent]
		public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRegionEntering));
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnLoaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(GamePlayerEvent.RegionChanged, new DOLEventHandler(OnRegionEntering));
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
						GameServer.Database.SaveObject(acc);
					}
					
					player.TempProperties.setProperty(REGISTER_WHELP_ACCOUNT_TAG, acc);
				}
				
				if (acc.Validated == false)
				{
					// Our player isn't fully validated, spawn the registration "Helper"
					new RegionTimerAction<GamePlayer>(player, 10000, pl => { SpawnRegisterHelper(pl, acc); return 0; });
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
			
		}
		
	}
}
