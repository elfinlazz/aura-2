// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Creatures;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared;
using Aura.Shared.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Shared.Util.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Aura.Channel.Util
{
	public class GmCommandManager : CommandManager<GmCommand, GmCommandFunc>
	{
		public GmCommandManager()
		{
			// Players
			Add(00, 50, "where", "", HandleWhere);
			Add(00, 50, "cp", "", HandleCp);
			Add(00, 50, "distance", "", HandleDistance);

			// VIPs
			Add(01, 50, "go", "<location>", HandleGo);
			Add(01, 50, "iteminfo", "<name>", HandleItemInfo);
			Add(01, 50, "skillinfo", "<name>", HandleSkillInfo);
			Add(01, 50, "raceinfo", "<name>", HandleRaceInfo);
			Add(01, 50, "height", "<height>", HandleBody);
			Add(01, 50, "weight", "<weight>", HandleBody);
			Add(01, 50, "upper", "<upper>", HandleBody);
			Add(01, 50, "lower", "<lower>", HandleBody);
			Add(01, 50, "haircolor", "<hex color>", HandleHairColor);
			Add(01, 50, "die", "", HandleDie);
			Add(01, 50, "who", "", HandleWho);
			Add(01, 50, "motion", "<category> <motion>", HandleMotion);
			Add(01, 50, "gesture", "<gesture>", HandleGesture);

			// GMs
			Add(50, 50, "warp", "<region> [x] [y]", HandleWarp);
			Add(50, 50, "jump", "[x] [y]", HandleWarp);
			Add(50, 50, "item", "<id|name> [amount|color1 [color2 [color 3]]]", HandleItem);
			Add(50, 50, "skill", "<id> [rank]", HandleSkill);
			Add(50, 50, "title", "<id>", HandleTitle);
			Add(50, 50, "speed", "[increase]", HandleSpeed);
			Add(50, 50, "spawn", "<race> [amount]", HandleSpawn);
			Add(50, 50, "ap", "<amount>", HandleAp);
			Add(50, -1, "gmcp", "", HandleGmcp);
			Add(50, 50, "card", "<id>", HandleCard);
			Add(50, 50, "petcard", "<race>", HandleCard);
			Add(50, 50, "heal", "", HandleHeal);
			Add(50, 50, "clean", "", HandleClean);
			Add(50, 50, "condition", "[a] [b] [c] [d] [e]", HandleCondition);
			Add(50, 50, "effect", "<id> [(b|i|s:parameter)|me]", HandleEffect);
			Add(50, 50, "prop", "<id>", HandleProp);
			Add(50, 50, "msg", "<message>", HandleMsg);
			Add(50, 50, "broadcast", "<message>", HandleBroadcast);
			Add(50, 50, "allskills", "", HandleAllSkills);
			Add(50, 50, "alltitles", "", HandleAllTitles);

			// Admins
			Add(99, 99, "variant", "<xml_file>", HandleVariant);
			Add(99, -1, "reloaddata", "", HandleReloadData);
			Add(99, -1, "reloadscripts", "", HandleReloadScripts);
			Add(99, -1, "reloadconf", "", HandleReloadConf);
			Add(99, 99, "closenpc", "", HandleCloseNpc);

			// Aliases
			AddAlias("item", "drop");
			AddAlias("iteminfo", "ii");
			AddAlias("skillinfo", "si");
			AddAlias("raceinfo", "ri");
			AddAlias("msg", "m");
			AddAlias("broadcast", "bc");
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Adds new command.
		/// </summary>
		/// <param name="auth"></param>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="func"></param>
		public void Add(int auth, string name, string usage, GmCommandFunc func)
		{
			this.Add(auth, 100, name, usage, func);
		}

		/// <summary>
		/// Adds new command.
		/// </summary>
		/// <param name="auth"></param>
		/// <param name="charAuth"></param>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="func"></param>
		public void Add(int auth, int charAuth, string name, string usage, GmCommandFunc func)
		{
			this.Add(new GmCommand(auth, charAuth, name, usage, func));
		}

		/// <summary>
		/// Adds alias for command.
		/// </summary>
		/// <param name="commandName"></param>
		/// <param name="alias"></param>
		public void AddAlias(string commandName, string alias)
		{
			var command = this.GetCommand(commandName);
			if (command == null)
				throw new Exception("Aliasing: Command '" + commandName + "' not found");

			_commands[alias] = command;
		}

		/// <summary>
		/// Tries to run command, based on message.
		/// Returns false if the message was of no interest to the
		/// command handler.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="creature"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool Process(ChannelClient client, Creature creature, string message)
		{
			if (message.Length < 2 || !message.StartsWith(ChannelServer.Instance.Conf.Commands.Prefix.ToString(CultureInfo.InvariantCulture)))
				return false;

			// Parse arguments
			var args = this.ParseLine(message);

			var sender = creature;
			var target = creature;
			var isCharCommand = message.StartsWith(ChannelServer.Instance.Conf.Commands.Prefix2);

			// Handle char commands
			if (isCharCommand)
			{
				// Get target player
				if (args.Count < 2 || (target = ChannelServer.Instance.World.GetPlayer(args[1])) == null)
				{
					Send.ServerMessage(creature, Localization.Get("Target not found."));
					return true;
				}

				// Remove target name from the args
				args.RemoveAt(1);
			}

			// Get command
			var command = this.GetCommand(args[0]);
			if (command == null)
			{
				// Don't send invalid command message because it'll interfere with
				// 4chan-greentext style ">lol"

				//Send.ServerMessage(creature, Localization.Get("Unknown command '{0}'."), args[0]);
				return false;
			}

			var commandConf = ChannelServer.Instance.Conf.Commands.GetAuth(command.Name, command.Auth, command.CharAuth);

			// Check auth
			if ((!isCharCommand && client.Account.Authority < commandConf.Auth) || (isCharCommand && client.Account.Authority < commandConf.CharAuth))
			{
				Send.ServerMessage(creature, Localization.Get("You're not authorized to use '{0}'."), args[0]);
				return true;
			}

			if (isCharCommand && commandConf.CharAuth < 0)
			{
				Send.ServerMessage(creature, Localization.Get("Command '{0}' cannot be used on another character."), args[0]);
				return true;
			}

			// Run
			var result = command.Func(client, sender, target, message, args);

			// Handle result
			if (result == CommandResult.InvalidArgument)
			{
				Send.ServerMessage(creature, Localization.Get("Usage: {0} {1}"), command.Name, command.Usage);
				if (command.CharAuth <= client.Account.Authority && command.CharAuth > 0)
					Send.ServerMessage(creature, Localization.Get("Usage: {0} <target> {1}"), command.Name, command.Usage);

				return true;
			}

			if (result == CommandResult.Fail)
			{
				Send.ServerMessage(creature, "Failed to process command.");
				return true;
			}

			return true;
		}

		// ------------------------------------------------------------------

		private CommandResult HandleWhere(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			var pos = target.GetPosition();
			var msg = sender == target
				? Localization.Get("You're here: Region: {0} @ {1}/{2}, Area: {5}, Dir: {4} (Radian: {6})")
				: Localization.Get("{3} is here: Region: {0} @ {1}/{2}, Area: {5}, Dir: {4} (Radian: {6})");

			Send.ServerMessage(sender, msg, target.RegionId, pos.X, pos.Y, target.Name, target.Direction, AuraData.RegionInfoDb.GetAreaId(target.RegionId, pos.X, pos.Y), MabiMath.ByteToRadian(target.Direction).ToInvariant("#.###"));

			return CommandResult.Okay;
		}

		private CommandResult HandleWarp(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			// Handles both warp and jump

			var warp = (args[0] == "warp");
			var offset = (warp ? 1 : 0);

			if (warp && args.Count < 2)
				return CommandResult.InvalidArgument;

			// Get region id
			int regionId = 0;
			if (warp)
			{
				if (!int.TryParse(args[1].Replace("r:", ""), out regionId))
				{
					Send.ServerMessage(sender, Localization.Get("Invalid region id."));
					return CommandResult.InvalidArgument;
				}
			}
			else
				regionId = target.RegionId;

			// Check region
			if (warp && !ChannelServer.Instance.World.HasRegion(regionId))
			{
				Send.ServerMessage(sender, Localization.Get("Region doesn't exist."));
				return CommandResult.Fail;
			}

			int x = -1, y = -1;

			// Parse X
			if (args.Count > 1 + offset && !int.TryParse(args[1 + offset].Replace("x:", ""), out x))
			{
				Send.ServerMessage(sender, Localization.Get("Invalid X coordinate."));
				return CommandResult.InvalidArgument;
			}

			// Parse Y
			if (args.Count > 2 + offset && !int.TryParse(args[2 + offset].Replace("y:", ""), out y))
			{
				Send.ServerMessage(sender, Localization.Get("Invalid Y coordinate."));
				return CommandResult.InvalidArgument;
			}

			// Random coordinates if none were specified
			if (x == -1 || y == -1)
			{
				var rndc = AuraData.RegionInfoDb.RandomCoord(regionId);
				if (x < 0) x = rndc.X;
				if (y < 0) y = rndc.Y;
			}

			target.Warp(regionId, x, y);

			if (sender != target)
				Send.ServerMessage(target, Localization.Get("You've been warped by '{0}'."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleGo(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
			{
				Send.ServerMessage(sender,
					Localization.Get("Destinations:") +
					" Tir Chonaill, Dugald Isle, Dunbarton, Gairech, Bangor, Emain Macha, Taillteann, Tara, Cobh, Ceo Island, Nekojima, GM Island"
				);
				return CommandResult.InvalidArgument;
			}

			int regionId = -1, x = -1, y = -1;
			var destination = args[1].ToLower();

			if (destination.StartsWith("tir")) { regionId = 1; x = 12801; y = 38397; }
			else if (destination.StartsWith("dugald")) { regionId = 16; x = 23017; y = 61244; }
			else if (destination.StartsWith("dun")) { regionId = 14; x = 38001; y = 38802; }
			else if (destination.StartsWith("gairech")) { regionId = 30; x = 39295; y = 53279; }
			else if (destination.StartsWith("bangor")) { regionId = 31; x = 12904; y = 12200; }
			else if (destination.StartsWith("emain")) { regionId = 52; x = 39818; y = 41621; }
			else if (destination.StartsWith("tail")) { regionId = 300; x = 212749; y = 192720; }
			else if (destination.StartsWith("tara")) { regionId = 401; x = 99793; y = 91209; }
			else if (destination.StartsWith("cobh")) { regionId = 23; x = 28559; y = 37693; }
			else if (destination.StartsWith("ceo")) { regionId = 56; x = 8987; y = 9611; }
			else if (destination.StartsWith("neko")) { regionId = 600; x = 114430; y = 79085; }
			else if (destination.StartsWith("gm")) { regionId = 22; x = 2500; y = 2500; }
			else
			{
				Send.ServerMessage(sender, Localization.Get("Unkown destination"), args[1]);
				return CommandResult.InvalidArgument;
			}

			if (regionId == -1 || x == -1 || y == -1)
			{
				Send.ServerMessage(sender, Localization.Get("Error while choosing destination."));
				Log.Error("HandleGo: Incomplete destination '{0}'.", args[1]);
				return CommandResult.Fail;
			}

			target.Warp(regionId, x, y);

			if (sender != target)
				Send.ServerMessage(target, Localization.Get("You've been warped by '{0}'."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleItem(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var drop = (args[0] == "drop");
			int itemId = 0;
			ItemData itemData = null;

			// Get item data
			if (!int.TryParse(args[1], out itemId))
			{
				var all = AuraData.ItemDb.FindAll(args[1]);

				// One or multiple items found
				if (all.Count > 0)
				{
					// Find best result
					var score = 10000;
					foreach (var data in all)
					{
						var curScore = data.Name.LevenshteinDistance(args[1], false);
						if (curScore < score)
						{
							score = curScore;
							itemData = data;
						}

						if (score == 0)
							break;
					}

					if (all.Count > 1 && score != 0)
					{
						var perc = 100 - (100f / itemData.Name.Length * score);
						Send.ServerMessage(sender, Localization.Get("No exact match found for '{0}', using best result, '{1}' ({2}%)."), args[1], itemData.Name, perc.ToString("0.0", CultureInfo.InvariantCulture));
					}
				}
			}
			else
			{
				itemData = AuraData.ItemDb.Find(itemId);
			}

			// Check item data
			if (itemData == null)
			{
				Send.ServerMessage(sender, Localization.Get("Item '{0}' not found in database."), args[1]);
				return CommandResult.Fail;
			}

			var item = new Item(itemData.Id);

			// Check amount for stackable items
			if (itemData.StackType == StackType.Stackable && args.Count > 2)
			{
				int amount;

				// Get amount
				if (!int.TryParse(args[2], out amount) || amount <= 0)
				{
					Send.ServerMessage(sender, Localization.Get("Invalid amount."));
					return CommandResult.Fail;
				}

				item.Amount = amount;
			}
			// Parse colors
			else if (itemData.StackType != StackType.Stackable && args.Count > 2)
			{
				for (int i = 0; i < 3; ++i)
				{
					if (args.Count < 3 + i)
						break;

					var sColor = args[2 + i];
					uint color = 0;

					// Hex color
					if (sColor.StartsWith("0x"))
					{
						if (!uint.TryParse(sColor.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color))
						{
							Send.ServerMessage(sender, Localization.Get("Invalid hex color '{0}'."), sColor);
							return CommandResult.Fail;
						}
					}
					else
					{
						switch (sColor)
						{
							case "0":
							case "black": color = 0x00000000; break;
							case "f":
							case "white": color = 0xFFFFFFFF; break;
							default:
								Send.ServerMessage(sender, Localization.Get("Unknown color '{0}'."), sColor);
								return CommandResult.Fail;
						}
					}

					switch (i)
					{
						case 0: item.Info.Color1 = color; break;
						case 1: item.Info.Color2 = color; break;
						case 2: item.Info.Color3 = color; break;
					}
				}
			}

			// Create new pockets for bags
			if (item.Data.HasTag("/pouch/bag/") && !drop)
			{
				if (item.Data.BagWidth == 0)
				{
					Send.ServerMessage(sender, Localization.Get("Beware, shaped bags aren't supported yet."));
				}
				else if (!target.Inventory.AddBagPocket(item))
				{
					// TODO: Handle somehow? Without linked pocket the bag
					//   won't open.
				}
			}

			// Spawn item
			var success = true;
			if (!drop)
				success = target.Inventory.Add(item, Pocket.Temporary);
			else
				item.Drop(target.Region, target.GetPosition());

			if (success)
			{
				if (sender != target)
					Send.ServerMessage(target, Localization.Get("Item '{0}' has been spawned by '{1}'."), itemData.Name, sender.Name);
				Send.ServerMessage(sender, Localization.Get("Item '{0}' has been spawned."), itemData.Name);
				return CommandResult.Okay;
			}
			else
			{
				Send.ServerMessage(sender, Localization.Get("Failed to spawn item."));
				return CommandResult.Fail;
			}
		}

		private CommandResult HandleVariant(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var actualId = 1;
			var dynamicId = 35001;
			var x = 12800;
			var y = 38100;

			ChannelServer.Instance.World.AddRegion(35001);
			sender.SetLocation(dynamicId, x, y);
			sender.Warping = true;

			var pp = new Packet(0x0000A97E, MabiId.Broadcast);
			pp.PutLong(sender.EntityId);
			pp.PutInt(actualId);
			pp.PutInt(dynamicId);
			pp.PutInt(x);
			pp.PutInt(y);
			pp.PutInt(0);
			pp.PutInt(1);
			pp.PutInt(dynamicId);
			pp.PutString("DynamicRegion" + dynamicId);
			pp.PutUInt(0x80000001);
			pp.PutInt(1);
			pp.PutString("uladh_main");
			pp.PutInt(200);
			pp.PutByte(0);
			pp.PutString("data/world/uladh_main/" + args[1] + ".xml"); // region_variation_797208

			client.Send(pp);

			return CommandResult.Okay;
		}

		private CommandResult HandleItemInfo(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var search = message.Substring(message.IndexOf(" ")).Trim();
			var items = AuraData.ItemDb.FindAll(search);
			if (items.Count == 0)
			{
				Send.ServerMessage(target, Localization.Get("No items found for '{0}'."), search);
				return CommandResult.Okay;
			}

			var eItems = items.OrderBy(a => a.Name.LevenshteinDistance(search)).ThenBy(a => a.Id).GetEnumerator();
			var max = 20;
			for (int i = 0; eItems.MoveNext() && i < max; ++i)
			{
				var item = eItems.Current;
				Send.ServerMessage(target, Localization.Get("{0}: {1}, Type: {2}"), item.Id, item.Name, item.Type);
			}

			Send.ServerMessage(target, Localization.Get("Results: {0} (Max. {1} shown)"), items.Count, max);

			return CommandResult.Okay;
		}

		private CommandResult HandleSkillInfo(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var search = message.Substring(message.IndexOf(" ")).Trim();
			var items = AuraData.SkillDb.FindAll(search);
			if (items.Count == 0)
			{
				Send.ServerMessage(target, Localization.Get("No skills found for '{0}'."), search);
				return CommandResult.Okay;
			}

			var eItems = items.OrderBy(a => a.Name.LevenshteinDistance(search)).ThenBy(a => a.Id).GetEnumerator();
			var max = 20;
			for (int i = 0; eItems.MoveNext() && i < max; ++i)
			{
				var item = eItems.Current;
				Send.ServerMessage(target, Localization.Get("{0}: {1}"), item.Id, item.Name);
			}

			Send.ServerMessage(target, Localization.Get("Results: {0} (Max. {1} shown)"), items.Count, max);

			return CommandResult.Okay;
		}

		private CommandResult HandleRaceInfo(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var search = message.Substring(message.IndexOf(" ")).Trim();
			var items = AuraData.RaceDb.FindAll(search);
			if (items.Count == 0)
			{
				Send.ServerMessage(target, Localization.Get("No races found for '{0}'."), search);
				return CommandResult.Okay;
			}

			var eItems = items.OrderBy(a => a.Name.LevenshteinDistance(search)).ThenBy(a => a.Id).GetEnumerator();
			var max = 20;
			for (int i = 0; eItems.MoveNext() && i < max; ++i)
			{
				var item = eItems.Current;
				Send.ServerMessage(target, Localization.Get("{0}: {1}"), item.Id, item.Name);
			}

			Send.ServerMessage(target, Localization.Get("Results: {0} (Max. {1} shown)"), items.Count, max);

			return CommandResult.Okay;
		}

		private CommandResult HandleSkill(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			int skillId;
			if (!int.TryParse(args[1], out skillId))
				return CommandResult.InvalidArgument;

			var skillData = AuraData.SkillDb.Find(skillId);
			if (skillData == null)
			{
				Send.ServerMessage(sender, Localization.Get("Skill '{0}' not found in database."), args[1]);
				return CommandResult.Fail;
			}

			int rank = 0;
			if (args.Count > 2 && args[2] != "novice" && !int.TryParse(args[2], NumberStyles.HexNumber, null, out rank))
				return CommandResult.InvalidArgument;

			if (rank > 0)
				rank = Math2.Clamp(0, 18, 16 - rank);

			var rankData = skillData.GetRankData(rank, target.Race);
			if (rankData == null)
			{
				Send.ServerMessage(sender, Localization.Get("Skill '{0}' doesn't have rank '{1}'."), args[1], (SkillRank)rank);
				return CommandResult.Fail;
			}

			target.Skills.Give((SkillId)skillId, (SkillRank)rank);

			Send.ServerMessage(sender, Localization.Get("Skill added."));
			if (target != sender)
				Send.ServerMessage(sender, Localization.Get("Skill '{0}' added by '{1}'."), (SkillId)skillId, sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleBody(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			float val;
			if (!float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out val))
				return CommandResult.InvalidArgument;

			switch (args[0])
			{
				case "height": target.Height = val; val = target.Height; break;
				case "weight": target.Weight = val; val = target.Weight; break;
				case "upper": target.Upper = val; val = target.Upper; break;
				case "lower": target.Lower = val; val = target.Lower; break;
			}

			Send.CreatureBodyUpdate(target);

			Send.ServerMessage(sender, Localization.Get("Change successful, new value: {0}"), val.ToInvariant("0.0"));
			if (sender != target)
				Send.ServerMessage(target, Localization.Get("Your appearance has been changed by {0}."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleCp(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (sender == target)
				Send.ServerMessage(sender, Localization.Get("Your combat power: {0}"), target.CombatPower.ToInvariant("0.0"));
			else
				Send.ServerMessage(sender, Localization.Get("{0}'s combat power: {1}"), target.Name, target.CombatPower.ToInvariant("0.0"));

			return CommandResult.Okay;
		}

		private CommandResult HandleHairColor(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			uint color;
			// Hex color
			if (args[1].StartsWith("0x"))
			{
				if (!uint.TryParse(args[1].Replace("0x", ""), NumberStyles.HexNumber, null, out color))
					return CommandResult.InvalidArgument;
			}
			// Mabi color
			else if (uint.TryParse(args[1], out color) && color <= 0xFF)
			{
				color += 0x10000000;
			}
			else
			{
				switch (args[1])
				{
					case "saiyan": color = 0x60001312; break;
					default:
						return CommandResult.InvalidArgument;
				}
			}

			var hair = target.Inventory.GetItemAt(Pocket.Hair, 0, 0);
			if (hair == null)
				return CommandResult.Fail;

			hair.Info.Color1 = color;
			Send.EquipmentChanged(target, hair);

			Send.ServerMessage(sender, Localization.Get("Change successful, new value: {0}"), "0x" + color.ToString("X8"));
			if (sender != target)
				Send.ServerMessage(target, Localization.Get("Your appearance has been changed by {0}."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleTitle(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			ushort titleId;
			if (!ushort.TryParse(args[1], out titleId))
				return CommandResult.InvalidArgument;

			target.Titles.Enable(titleId);

			Send.ServerMessage(sender, Localization.Get("Added title."));
			if (sender != target)
				Send.ServerMessage(target, Localization.Get("{0} enabled a title for you."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleSpeed(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			short speed = 0;
			if (args.Count > 1 && !short.TryParse(args[1], out speed))
				return CommandResult.InvalidArgument;

			speed = (short)Math2.Clamp(0, 1000, speed);

			if (speed == 0)
				target.Conditions.Deactivate(ConditionsC.Hurry);
			else
				target.Conditions.Activate(ConditionsC.Hurry, speed);

			Send.ServerMessage(sender, Localization.Get("Speed changed to +{0}%."), speed);
			if (sender != target)
				Send.ServerMessage(target, Localization.Get("Your speed has been changed to +{0}% by {1}."), speed, sender.Name);


			return CommandResult.Okay;
		}

		private CommandResult HandleSpawn(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			int raceId;
			if (!int.TryParse(args[1], out raceId))
				return CommandResult.InvalidArgument;

			if (!AuraData.RaceDb.Exists(raceId))
			{
				Send.ServerMessage(sender, Localization.Get("Race '{0}' doesn't exist."), raceId);
				return CommandResult.Fail;
			}

			int amount = 1;
			if (args.Count > 2 && !int.TryParse(args[2], out amount))
				return CommandResult.InvalidArgument;

			var targetPos = target.GetPosition();
			for (int i = 0; i < amount; ++i)
			{
				var x = (int)(targetPos.X + Math.Sin(i) * i * 20);
				var y = (int)(targetPos.Y + Math.Cos(i) * i * 20);

				var creature = ChannelServer.Instance.ScriptManager.Spawn(raceId, target.RegionId, x, y, -1, true, true);
			}

			Send.ServerMessage(sender, Localization.Get("Creatures spawned."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("{0} spawned creatures around you."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleDie(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			target.Kill(sender);

			//Send.PlayDead(target);

			if (target != sender)
				Send.ServerMessage(target, Localization.Get("You've been killed by {0}."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleReloadData(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			Send.ServerMessage(sender, Localization.Get("Reloading, this might take a moment."));
			ChannelServer.Instance.LoadData(DataLoad.ChannelServer, true);
			Send.ServerMessage(sender, Localization.Get("Reload complete."));

			return CommandResult.Okay;
		}

		private CommandResult HandleReloadScripts(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			Send.ServerMessage(sender, Localization.Get("Beware, reloading should only be used during development, it's not guaranteed to be safe."));
			Send.ServerMessage(sender, Localization.Get("Reloading, this might take a moment."));
			ChannelServer.Instance.ScriptManager.Reload();
			Send.ServerMessage(sender, Localization.Get("Reload complete."));

			return CommandResult.Okay;
		}

		private CommandResult HandleReloadConf(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			Send.ServerMessage(sender, Localization.Get("Beware, reloading should only be used during development, it's not guaranteed to be safe."));
			Send.ServerMessage(sender, Localization.Get("Reloading, this might take a moment."));
			ChannelServer.Instance.Conf.Load();
			Send.ServerMessage(sender, Localization.Get("Reload complete."));

			return CommandResult.Okay;
		}

		private CommandResult HandleAp(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			short amount;
			if (!short.TryParse(args[1], out amount))
				return CommandResult.InvalidArgument;

			target.GiveAp(amount);

			return CommandResult.Okay;
		}

		private CommandResult HandleCloseNpc(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (!client.NpcSession.IsValid())
				return CommandResult.Fail;

			Send.NpcTalkEndR(client.NpcSession.Script.Player, client.NpcSession.Script.NPC.EntityId, "Ended by closenpc command.");

			return CommandResult.Okay;
		}

		private CommandResult HandleGmcp(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (client.Account.Authority < ChannelServer.Instance.Conf.World.GmcpMinAuth)
			{
				Send.ServerMessage(sender, Localization.Get("You're not authorized to use the GMCP."));
				return CommandResult.Fail;
			}

			Send.GmcpOpen(sender);

			return CommandResult.Okay;
		}

		private CommandResult HandleCard(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			int type, race;
			if (args[0] == "card")
			{
				race = 0;
				if (!int.TryParse(args[1], out type))
					return CommandResult.InvalidArgument;
			}
			else
			{
				type = MabiId.PetCardType;
				if (!int.TryParse(args[1], out race))
					return CommandResult.InvalidArgument;
			}

			ChannelServer.Instance.Database.AddCard(target.Client.Account.Id, type, race);

			Send.ServerMessage(sender, Localization.Get("Added card."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("You've received a card from '{0}'."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleHeal(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			target.FullHeal();

			Send.ServerMessage(sender, Localization.Get("Healed."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("You've been healed by '{0}'."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleClean(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			var items = target.Region.GetAllItems();
			foreach (var item in items)
				item.DisappearTime = DateTime.Now;

			Send.ServerMessage(sender, Localization.Get("Marked all items on the floor to disappear now."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("{0} removed all items in your region."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleCondition(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			var conditions = new ulong[5];

			// Read arguments
			for (int i = 1; i < args.Count; ++i)
			{
				if (!ulong.TryParse(args[i].Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out conditions[i - 1]))
				{
					Send.ServerMessage(sender, Localization.Get("Invalid condition number."));
					return CommandResult.InvalidArgument;
				}
			}

			// Apply conditions
			target.Conditions.Deactivate(ConditionsA.All); target.Conditions.Activate((ConditionsA)conditions[0]);
			target.Conditions.Deactivate(ConditionsB.All); target.Conditions.Activate((ConditionsB)conditions[1]);
			target.Conditions.Deactivate(ConditionsC.All); target.Conditions.Activate((ConditionsC)conditions[2]);
			target.Conditions.Deactivate(ConditionsD.All); target.Conditions.Activate((ConditionsD)conditions[3]);
			target.Conditions.Deactivate(ConditionsE.All); target.Conditions.Activate((ConditionsE)conditions[4]);

			if (args.Count > 1)
				Send.ServerMessage(sender, Localization.Get("Applied condition."));
			else
				Send.ServerMessage(sender, Localization.Get("Cleared condition."));

			if (target != sender)
				Send.ServerMessage(target, Localization.Get("Your condition has been changed by {0}."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleEffect(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			// Requirement: command + effect id
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var packet = new Packet(Op.Effect, target.EntityId);

			// Get effect id
			uint effectId;
			if (!uint.TryParse(args[1], out effectId))
				return CommandResult.InvalidArgument;

			packet.PutUInt(effectId);

			// Parse arguments
			for (int i = 2; i < args.Count; ++i)
			{
				// type:value
				var splitted = args[i].Split(':');

				// "me" = target's entity id (long)
				if (splitted[0] == "me")
				{
					packet.PutLong(target.EntityId);
					continue;
				}

				// Everything but the above arguments require
				// a type and a value.
				if (splitted.Length < 2)
					continue;

				splitted[0] = splitted[0].Trim();
				splitted[1] = splitted[1].Trim();

				switch (splitted[0])
				{
					// Byte
					case "b":
						{
							byte val;
							if (!byte.TryParse(splitted[1], out val))
								return CommandResult.InvalidArgument;
							packet.PutByte(val);
							break;
						}
					// Int
					case "i":
						{
							uint val;
							if (!uint.TryParse(splitted[1], out val))
								return CommandResult.InvalidArgument;
							packet.PutUInt(val);
							break;
						}
					// String
					case "s":
						{
							packet.PutString(splitted[1]);
							break;
						}
				}
			}

			// Broadcast effect
			target.Region.Broadcast(packet, target);

			Send.ServerMessage(sender, Localization.Get("Applied effect."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("{0} has applied an effect to you."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleProp(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			int propId;
			if (!int.TryParse(args[1], out propId))
				return CommandResult.InvalidArgument;

			var pos = target.GetPosition();
			var prop = new Prop(propId, target.RegionId, pos.X, pos.Y, MabiMath.ByteToRadian(target.Direction));

			target.Region.AddProp(prop);

			Send.ServerMessage(sender, Localization.Get("Spawned prop."));

			return CommandResult.Okay;
		}

		private CommandResult HandleWho(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			int regionId = 0;
			if (args.Count > 1 && !int.TryParse(args[1], out regionId))
				return CommandResult.InvalidArgument;

			List<Creature> players;
			if (regionId != 0)
			{
				var region = ChannelServer.Instance.World.GetRegion(regionId);
				if (region == null)
				{
					Send.ServerMessage(sender, Localization.Get("Unknown region."));
					return CommandResult.Fail;
				}

				players = region.GetAllPlayers();

				Send.ServerMessage(sender, Localization.Get("Players online in region {0} ({1}):"), regionId, players.Count);
			}
			else
			{
				players = ChannelServer.Instance.World.GetAllPlayers();

				Send.ServerMessage(sender, Localization.Get("Players online ({0}):"), players.Count);
			}

			Send.ServerMessage(sender,
				players.Count == 0
				? Localization.Get("None")
				: string.Join(", ", players.Select(a => a.Name))
			);

			return CommandResult.Okay;
		}

		private CommandResult HandleMotion(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 3)
				return CommandResult.InvalidArgument;

			int category, motion;
			if (!int.TryParse(args[1], out category) || !int.TryParse(args[2], out motion))
				return CommandResult.InvalidArgument;

			Send.UseMotion(target, category, motion, false, true);

			Send.ServerMessage(sender, Localization.Get("Applied motion."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("{0} has applied a motion to you."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleGesture(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var gesture = AuraData.MotionDb.Find(args[1]);
			if (gesture == null)
			{
				Send.ServerMessage(sender, Localization.Get("Unknown gesture."));
				return CommandResult.Fail;
			}

			Send.UseMotion(target, gesture.Category, gesture.Type, gesture.Loop, true);

			Send.ServerMessage(sender, Localization.Get("Gestured '{0}'."), args[1]);
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("{0} made you gesture '{1}'."), sender.Name, args[1]);

			return CommandResult.Okay;
		}

		private CommandResult HandleBroadcast(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			var notice = sender.Name + ": " + message.Substring(message.IndexOf(" "));

			Send.Internal_Broadcast(notice);

			return CommandResult.Okay;
		}

		private CommandResult HandleMsg(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			if (args.Count < 2)
				return CommandResult.InvalidArgument;

			Send.System_Broadcast(target.Name, message.Substring(message.IndexOf(" ")));

			return CommandResult.Okay;
		}

		private CommandResult HandleAllSkills(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			// List of "working" skills
			var listOfSkills = new SkillId[] {
				SkillId.Smash, SkillId.Defense,
				SkillId.Rest,
				SkillId.ManaShield, 
				SkillId.Composing, SkillId.PlayingInstrument, SkillId.Song,
			};

			// Add all skills
			foreach (var sid in listOfSkills)
			{
				var skill = AuraData.SkillDb.Find((int)sid);
				if (skill == null) continue;

				target.Skills.Give(sid, (SkillRank)skill.MaxRank);
			}

			// Success
			Send.ServerMessage(sender, Localization.Get("Added all skills the server supports on their max rank."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("{0} gave you all skills the server supports on their max rank."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleAllTitles(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			// Add all titles. Using Enable to send an enable packet for
			// every title crashes the client.
			foreach (var title in AuraData.TitleDb.Entries.Values)
				target.Titles.Add(title.Id, TitleState.Usable);

			// Success
			Send.ServerMessage(sender, Localization.Get("Enabled all available titles, please relog to use them."));
			if (target != sender)
				Send.ServerMessage(target, Localization.Get("{0} enabled all available titles for you, please relog to use them."), sender.Name);

			return CommandResult.Okay;
		}

		private CommandResult HandleDistance(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
		{
			var distancePos = sender.Vars.Temp.DistanceCommandPos;

			if (distancePos == null)
			{
				sender.Vars.Temp.DistanceCommandPos = sender.GetPosition();
				Send.ServerMessage(sender, Localization.Get("Position 1 saved, use command again to calculate distance."));
			}
			else
			{
				var pos2 = sender.GetPosition();
				var distance = pos2.GetDistance(distancePos);

				Send.ServerMessage(sender, Localization.Get("Distance between '{0}' and '{1}': {2}"), distancePos, pos2, distance);

				sender.Vars.Temp.DistanceCommandPos = null;
			}

			return CommandResult.Okay;
		}
	}

	public class GmCommand : Command<GmCommandFunc>
	{
		public int Auth { get; private set; }
		public int CharAuth { get; private set; }

		public GmCommand(int auth, int charAuth, string name, string usage, GmCommandFunc func)
			: base(name, usage, "", func)
		{
			this.Auth = auth;
			this.CharAuth = charAuth;
		}
	}

	public delegate CommandResult GmCommandFunc(ChannelClient client, Creature sender, Creature target, string message, IList<string> args);
}
