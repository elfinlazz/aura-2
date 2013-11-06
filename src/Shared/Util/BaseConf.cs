// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Reflection;

namespace Aura.Shared.Util
{
	public abstract class BaseConf : ConfFile
	{
		// Log
		public bool Archive;
		public LogLevel Hide;

		// Database
		public string Host;
		public string User;
		public string Pass;
		public string Db;

		// Localization
		public string Language;

		public BaseConf()
		{
		}

		/// <summary>
		/// Loads several conf files generally required,
		/// like log, database, etc.
		/// </summary>
		public void LoadDefault()
		{
			this.LoadLog();
			this.LoadDatabase();
			this.LoadLocalization();
		}

		public abstract void Load();

		/// <summary>
		/// Loads log.conf from system and user.
		/// </summary>
		protected void LoadLog()
		{
			this.RequireAndInclude("{0}/conf/log.conf", "system", "user");

			this.Archive = this.GetBool("log.archive", true);
			this.Hide = (LogLevel)this.GetShort("log.cmd_hide", (short)(LogLevel.Debug));

			if (this.Archive)
				Log.Archive = "log/archive/";
			Log.LogFile = string.Format("log/{0}.txt", System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "").Replace(".vshost", ""));
			Log.Hide |= this.Hide;
		}

		/// <summary>
		/// Loads database.conf from system and user.
		/// </summary>
		protected void LoadDatabase()
		{
			this.RequireAndInclude("{0}/conf/database.conf", "system", "user");

			this.Host = this.GetString("database.host", "127.0.0.1");
			this.User = this.GetString("database.user", "root");
			this.Pass = this.GetString("database.pass", "");
			this.Db = this.GetString("database.db", "aura");
		}

		/// <summary>
		/// Loads localization.conf from system and user.
		/// </summary>
		protected void LoadLocalization()
		{
			this.RequireAndInclude("{0}/conf/localization.conf", "system", "user");

			this.Language = this.GetString("localization.language", "us");
		}
	}
}
