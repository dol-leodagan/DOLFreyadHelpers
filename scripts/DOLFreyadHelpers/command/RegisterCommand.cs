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
using System.Linq;
using System.Text.RegularExpressions;

using DOL.GS;
using DOL.GS.Commands;

namespace DOLFreyadHelpers
{
	/// <summary>
	/// Registration Command for pairing Game Account with Other Software Account.
	/// Handle Token Query upon pairing, and Token Registration upon validation.
	/// </summary>
	[CmdAttribute("&register", //command to handle
	              ePrivLevel.Player, //minimum privelege level
	              "Register your Game Account and bind it to your Website Account", //command description
	              "/register \"Website Account Name\" | /register \"Validation Token\"")] //usage
	public class RegisterCommand : AbstractCommandHandler, ICommandHandler
	{
		private const string REGISTER_COMMAND_ARG_PASS = "REGISTER_COMMAND_ARG_PASS";
		
		/// <summary>
		/// On Register Command Handle Registration and Validation
		/// </summary>
		/// <param name="client"></param>
		/// <param name="args"></param>
		public void OnCommand(GameClient client, string[] args)
		{
			// Filter Invalid Clients
			if(client == null || client.IsPlaying == false || client.Player == null || client.Account == null)
				return;
			
			if (FreyadRegisterWhelp.HELPER_REGISTER_ACCOUNT_ENABLE == false)
			{
				DisplayMessage(client.Player, "Registration is actually Disabled !");
				return;
			}
			
			// Need At Least One Arg
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}
			
			// Join args
			string arg = string.Join(" ", args.Skip(1)).Trim();
			if (string.IsNullOrEmpty(arg))
			{
				DisplaySyntax(client);
				return;
			}
			
			var acc = client.Player.TempProperties.getProperty<AccountXHelperRegister>(FreyadRegisterWhelp.REGISTER_WHELP_ACCOUNT_TAG);
			if (acc == null)
			{
				DisplayMessage(client.Player, "Cannot Register Actually... Try Zoning or Reloading!");
				return;
			}
			
			if (acc.Validated)
			{
				DisplayMessage(client.Player, string.Format("You are already Registered to \"{0}\", and cannot Register again...", acc.ExternalAccount));
				return;
			}
			
			var tokenRegEx = new Regex("^[0-9]{10}$");
			if (tokenRegEx.IsMatch(arg))
				arg = string.Format("#{0}", arg);
						
			if (arg.StartsWith("#"))
			{
				if (string.IsNullOrEmpty(acc.ExternalAccount))
				{
					DisplayMessage(client.Player, "You need to register a Web Account Name before entering Validation Token");
					return;
				}
				
				// Handle Registration Token
				if (arg.Equals(acc.Token))
				{
					client.Player.TempProperties.setProperty(REGISTER_COMMAND_ARG_PASS, arg);
					client.Player.Out.SendCustomDialog(string.Format("Confirm Registration Token to Account: {0}", acc.ExternalAccount), ConfirmValidationToken);
				}
				else
				{
					DisplayMessage(client.Player, "Wrong Validation Token ! Please double check your spelling before trying again...");
				}
			}
			else
			{
				// Handle Registration Account
				client.Player.TempProperties.setProperty(REGISTER_COMMAND_ARG_PASS, arg);
				client.Player.Out.SendCustomDialog(string.Format("Confirm Registration to Account: {0}", arg), ConfirmRegisterAccount);
			}
		}
		
		/// <summary>
		/// Confirm Account Registration
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		private void ConfirmRegisterAccount(GamePlayer player, byte response)
		{
			var arg = player.TempProperties.getProperty<string>(REGISTER_COMMAND_ARG_PASS, null);
			player.TempProperties.removeProperty(REGISTER_COMMAND_ARG_PASS);
			var acc = player.TempProperties.getProperty<AccountXHelperRegister>(FreyadRegisterWhelp.REGISTER_WHELP_ACCOUNT_TAG);
			
			if (arg == null || acc == null || arg.StartsWith("#"))
			{
				DisplayMessage(player, "Error while handling your Account Registration Confirm, please try again...");
				return;
			}
			
			if (response != 0x00)
			{
				acc.ExternalAccount = arg;
				acc.Token = CreateValidationToken();
				GameServer.Database.SaveObject(acc);
				DisplayMessage(player, string.Format("You registered account {0}, please visit https://daoc.freyad.net/account to get your validation Token!", arg));
				return;
			}

			DisplayMessage(player, "Account Registration Canceled!");
		}

		/// <summary>
		/// Confirm Token Validation
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		private void ConfirmValidationToken(GamePlayer player, byte response)
		{
			var arg = player.TempProperties.getProperty<string>(REGISTER_COMMAND_ARG_PASS, null);
			player.TempProperties.removeProperty(REGISTER_COMMAND_ARG_PASS);
			var acc = player.TempProperties.getProperty<AccountXHelperRegister>(FreyadRegisterWhelp.REGISTER_WHELP_ACCOUNT_TAG);
			
			if (arg == null || acc == null || !arg.StartsWith("#"))
			{
				DisplayMessage(player, "Error while handling your Account Validation Confirm, please try again...");
				return;
			}
			
			if (response != 0x00)
			{
				if (!arg.Equals(acc.Token))
				{
					DisplayMessage(player, "Error your Registration Token is Invalid, please try again...");
					return;
				}
				
				acc.Validated = true;
				GameServer.Database.SaveObject(acc);
				DisplayMessage(player, "Thank you, your account is successfully validated!");
				return;
			}
	
			DisplayMessage(player, "Registration Token Validation Canceled!");
			
		}
		
		/// <summary>
		/// Create a Random Token
		/// </summary>
		/// <returns></returns>
		private string CreateValidationToken()
		{
			string result = "#";
			for (int i = 0 ; i < 10 ; i++)
			{
				result += Util.Random(9);
			}
			
			return result;
		}
	}
}
