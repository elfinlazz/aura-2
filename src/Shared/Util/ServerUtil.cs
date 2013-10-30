// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Database;
using Aura.Data;
using System.IO;

namespace Aura.Shared.Util
{
	public static class ServerUtil
	{
		/// <summary>
		/// Tries to call conf's load method, exits on error.
		/// </summary>
		public static void LoadConf(BaseConf conf)
		{
			try
			{
				conf.Load();
				Log.Info("Read configuration.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to read configuration. ({0})", ex.Message);
				CmdUtil.Exit(1);
			}
		}

		/// <summary>
		/// Tries to initialize database, with the information from conf,
		/// exits on error.
		/// </summary>
		public static void InitDatabase(BaseConf conf)
		{
			try
			{
				AuraDb.Instance.Init(conf.Host, conf.User, conf.Pass, conf.Db);

				Log.Info("Initialized database.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to open database connection. ({0})", ex.Message);
				CmdUtil.Exit(1);
			}
		}

		/// <summary>
		/// (Re-)Loads the data with the data path as base, called on server
		/// start and with some reload commands. Should only load required data,
		/// e.g. Msgr Server doesn't need race data.
		/// Calls Exit if there are any problems.
		/// </summary>
		public static void LoadData(DataLoad toLoad, bool reload)
		{
			try
			{
				if ((toLoad & DataLoad.Races) != 0)
				{
					LoadDb(MabiData.AncientDropDb, "db/ancient_drops.txt", reload);
					LoadDb(MabiData.RaceSkillDb, "db/race_skills.txt", reload);
					LoadDb(MabiData.SpeedDb, "db/speed.txt", reload, false);
					LoadDb(MabiData.RaceDb, "db/races.txt", reload);
				}

				if ((toLoad & DataLoad.StatsBase) != 0)
				{
					LoadDb(MabiData.StatsBaseDb, "db/stats_base.txt", reload);
				}

				if ((toLoad & DataLoad.StatsLevel) != 0)
				{
					LoadDb(MabiData.StatsLevelUpDb, "db/stats_levelup.txt", reload);
				}

				if ((toLoad & DataLoad.Motions) != 0)
				{
					LoadDb(MabiData.MotionDb, "db/motions.txt", reload);
				}

				if ((toLoad & DataLoad.Cards) != 0)
				{
					LoadDb(MabiData.CharCardSetDb, "db/charcardsets.txt", reload, false);
					LoadDb(MabiData.CharCardDb, "db/charcards.txt", reload);
				}

				if ((toLoad & DataLoad.Colors) != 0)
				{
					LoadDb(MabiData.ColorMapDb, "db/colormap.dat", reload);
				}

				if ((toLoad & DataLoad.Items) != 0)
				{
					LoadDb(MabiData.ItemDb, "db/items.txt", reload);
					LoadDb(MabiData.ChairDb, "db/chairs.txt", reload);
				}

				if ((toLoad & DataLoad.Skills) != 0)
				{
					LoadDb(MabiData.SkillRankDb, "db/skill_ranks.txt", reload, false);
					LoadDb(MabiData.SkillDb, "db/skills.txt", reload);
				}

				if ((toLoad & DataLoad.Regions) != 0)
				{
					LoadDb(MabiData.RegionDb, "db/regions.txt", reload);
					LoadDb(MabiData.RegionInfoDb, "db/regioninfo.dat", reload);
				}

				if ((toLoad & DataLoad.Shamala) != 0)
				{
					LoadDb(MabiData.ShamalaDb, "db/shamala.txt", reload);
				}

				if ((toLoad & DataLoad.PropDrops) != 0)
				{
					LoadDb(MabiData.PropDropDb, "db/prop_drops.txt", reload);
				}

				if ((toLoad & DataLoad.Exp) != 0)
				{
					LoadDb(MabiData.ExpDb, "db/exp.txt", reload);
				}

				if ((toLoad & DataLoad.Pets) != 0)
				{
					LoadDb(MabiData.PetDb, "db/pets.txt", reload);
				}

				if ((toLoad & DataLoad.Weather) != 0)
				{
					LoadDb(MabiData.WeatherDb, "db/weather.txt", reload);
				}
			}
			catch (FileNotFoundException ex)
			{
				Log.Error(ex.Message);
				CmdUtil.Exit(1);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				CmdUtil.Exit(1);
			}
		}

		/// <summary>
		/// Loads db, first from system, then from user.
		/// Logs problems as warnings.
		/// </summary>
		private static void LoadDb(IDatabase db, string path, bool reload, bool log = true)
		{
			var systemPath = Path.Combine("../..", "system", path);
			var userPath = Path.Combine("../..", "user", path);

			// System
			{
				db.Load(systemPath, reload);

				foreach (var ex in db.Warnings)
					Log.Warning(ex.ToString());
			}

			// User
			{
				// It's okay if user dbs don't exist.
				if (File.Exists(userPath))
				{
					db.Load(path, false);

					foreach (var ex in db.Warnings)
						Log.Warning(ex.ToString());
				}
			}

			if (log)
				Log.Info("Done loading {0} entries from {1}.", db.Count, Path.GetFileName(path));
		}
	}

	/// <summary>
	/// Used in LoadData, to specify which db files should be loaded.
	/// </summary>
	public enum DataLoad
	{
		//Spawns = 0x01,
		Skills = 0x02,
		Races = 0x04,
		StatsBase = 0x08,
		StatsLevel = 0x10,
		Motions = 0x20,
		Cards = 0x40,
		Colors = 0x80,
		Items = 0x100,
		Regions = 0x200,
		Shamala = 0x400,
		PropDrops = 0x800,
		Exp = 0x1000,
		Pets = 0x2000,
		Weather = 0x4000,

		All = 0xFFFF,

		LoginServer = Races | StatsBase | Cards | Colors | Items | Pets,
		WorldServer = All,
		Npcs = Races,
	}
}
