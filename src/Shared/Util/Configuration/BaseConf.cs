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
		public DatabaseConfFile Database { get; private set; }

		/// <summary>
		/// localization.conf
		/// </summary>
		public LocalizationConfFile Localization { get; private set; }

		/// <summary>
		/// internal.conf
		/// </summary>
		public InterConfFile Internal { get; private set; }

		protected BaseConf()
		{
			this.Log = new LogConfFile();
			this.Database = new DatabaseConfFile();
			this.Localization = new LocalizationConfFile();
			this.Internal = new InterConfFile();
		}

		/// <summary>
		/// Loads several conf files that are generally required,
		/// like log, database, etc.
		/// </summary>
		protected void LoadDefault()
		{
			this.Log.Load();
			this.Database.Load();
			this.Localization.Load();
			this.Internal.Load();

			if (this.Internal.Password == "change_me")
				Aura.Shared.Util.Log.Warning("Using the default inter server password is risky, please change it in 'inter.conf'.");
		}

		public abstract void Load();
	}
}
