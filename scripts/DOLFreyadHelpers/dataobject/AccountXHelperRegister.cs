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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOLFreyadHelpers
{
	/// <summary>
	/// AccountXHelperRegister Table Holding Helper Registration Records
	/// </summary>
	[DataTable(TableName="AccountXHelperRegister")]
	public class AccountXHelperRegister : DataObject
	{
		protected string m_accountName;
		
		/// <summary>
		/// DOL Account Name Link
		/// </summary>
		[PrimaryKey]
		public string AccountName {
			get { return m_accountName; }
			set { Dirty = true; m_accountName = value; }
		}
		
		protected string m_externalAccount;
		
		/// <summary>
		/// External Account Identifier
		/// </summary>
		[DataElement(Varchar = 255, AllowDbNull = false, Index = true)]
		public string ExternalAccount {
			get { return m_externalAccount; }
			set { Dirty = true; m_externalAccount = value; }
		}
		
		protected bool m_validated;
		
		/// <summary>
		/// Validation Flag
		/// </summary>
		[DataElement(Varchar = 255, AllowDbNull = false, Index = true)]
		public bool Validated {
			get { return m_validated; }
			set { Dirty = true; m_validated = value; }
		}
		
		protected string token;
		
		/// <summary>
		/// Validation Token
		/// </summary>
		[DataElement(Varchar = 255, AllowDbNull = false, Index = false)]
		public string Token {
			get { return token; }
			set { Dirty = true; token = value; }
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public AccountXHelperRegister()
		{
		}
	}
}
