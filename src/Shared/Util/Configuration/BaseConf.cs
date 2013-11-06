// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration.Files;

namespace Aura.Shared.Util.Configuration
{
	public abstract class BaseConf : ConfFile
	{
		/// <summary>
		/// log.conf
		/// </summary>
		public LogConfFile Log { get; protected set; }

		/// <summary>
		/// database.conf
		/// </summary>
		public DatabaseConfFile Database { get; protected set; }

		/// <summary>
		/// localization.conf
		/// </summary>
		public LocalizationConfFile Localization { get; protected set; }

		public BaseConf()
		{
			this.Log = new LogConfFile();
			this.Database = new DatabaseConfFile();
			this.Localization = new LocalizationConfFile();
		}

		/// <summary>
		/// Loads several conf files that are generally required,
		/// like log, database, etc.
		/// </summary>
		public void LoadDefault()
		{
			this.Log.Load();
			this.Database.Load();
			this.Localization.Load();
		}

		public abstract void Load();
	}
}
