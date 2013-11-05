// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Util;

namespace Aura.Channel.Util
{
	public class ChannelConf : BaseConf
	{
		public static readonly ChannelConf Instance = new ChannelConf();

		// Login
		public int Port;
		//public bool NewAccounts;
		//public bool EnableSecondaryPassword;

		//public bool ConsumeCharacterCards;
		//public bool ConsumePetCards;
		//public bool ConsumePartnerCards;

		//public int DeletionWait;

		private ChannelConf()
		{
		}

		public override void Load()
		{
			ChannelConf.Instance.RequireAndInclude("{0}/conf/log.conf", "system", "user");
			ChannelConf.Instance.RequireAndInclude("{0}/conf/database.conf", "system", "user");
			ChannelConf.Instance.RequireAndInclude("{0}/conf/login.conf", "system", "user");

			this.LoadLog("login");
			this.LoadDatabase();
			this.LoadLocalization();
			this.LoadChannel();
		}

		public void LoadChannel()
		{
			//this.Port = this.GetInt("login.port", 11000);
			//this.NewAccounts = this.GetBool("login.new_accounts", true);
			//this.EnableSecondaryPassword = this.GetBool("login.enable_secondary", false);

			//this.ConsumeCharacterCards = this.GetBool("login.consume_character_cards", true);
			//this.ConsumePetCards = this.GetBool("login.consume_pet_cards", true);
			//this.ConsumePartnerCards = this.GetBool("login.consume_partner_cards", true);

			//this.DeletionWait = this.GetInt("login.deletion_wait", 107);
		}
	}
}
