// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;
using System.Collections.Generic;

namespace Aura.Login.Util
{
	public class LoginConf : BaseConf
	{
		/// <summary>
		/// login.conf
		/// </summary>
		public LoginConfFile Login { get; protected set; }

		public LoginConf()
		{
			this.Login = new LoginConfFile();
		}

		public override void Load()
		{
			this.LoadDefault();
			this.Login.Load();
		}
	}

	/// <summary>
	/// Represents login.conf
	/// </summary>
	public class LoginConfFile : ConfFile
	{
		public int Port { get; protected set; }
		public bool NewAccounts { get; protected set; }
		public bool EnableSecondaryPassword { get; protected set; }

		public bool ConsumeCharacterCards { get; protected set; }
		public bool ConsumePetCards { get; protected set; }
		public bool ConsumePartnerCards { get; protected set; }

		public int DeletionWait { get; protected set; }

		public int WebPort { get; protected set; }
		public HashSet<string> TrustedSources { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/login.conf");

			this.Port = this.GetInt("port", 11000);
			this.NewAccounts = this.GetBool("new_accounts", true);
			this.EnableSecondaryPassword = this.GetBool("enable_secondary", false);

			this.ConsumeCharacterCards = this.GetBool("consume_character_cards", true);
			this.ConsumePetCards = this.GetBool("consume_pet_cards", true);
			this.ConsumePartnerCards = this.GetBool("consume_partner_cards", true);

			this.DeletionWait = this.GetInt("deletion_wait", 107);

			this.WebPort = this.GetInt("web_port", 10999);

			this.TrustedSources = new HashSet<string>();
			var trusted = this.GetString("trusted_sources", "127.0.0.1").Split(',');
			foreach (var source in trusted)
				this.TrustedSources.Add(source.Trim());
		}

		/// <summary>
		/// Returns true if source is listed in trusted sources.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public bool IsTrustedSource(string source)
		{
			return this.TrustedSources.Contains(source);
		}
	}
}
