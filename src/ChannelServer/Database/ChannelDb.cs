// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using MySql.Data.MySqlClient;

namespace Aura.Channel.Database
{
	public class ChannelDb
	{
		public static readonly ChannelDb Instance = new ChannelDb();

		private ChannelDb()
		{
		}
	}
}
