// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Shared.Util;
using Aura.Shared.Util.Commands;
using Aura.Channel.Util.Configuration.Files;
using Aura.Data.Database;
using System.Globalization;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Util
{
	public class GmCommandManager : CommandManager<GmCommand, GmCommandFunc>
	{
		public GmCommandManager()
		{
			// Players
			Add(00, 50, "where", "", HandleWhere);

			// VIPs
			Add(01, 50, "go", "<location>", HandleGo);

			// GMs
			Add(50, 50, "warp", "<region> [x] [y]", HandleWarp);
			Add(50, 50, "jump", "[x] [y]", HandleWarp);
			Add(50, 50, "item", "<id|name> [amount|color1 [color2 [color 3]]]", HandleItem);

			// Admins
			Add(99, 99, "test", "", HandleTest);

			// Aliases
			AddAlias("item", "drop");
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
			if (message.Length < 2 || !message.StartsWith(ChannelServer.Instance.Conf.Commands.Prefix.ToString()))
				return false;

			// Parse arguments
			var args = this.ParseLine(message);
			args[0].TrimStart(ChannelServer.Instance.Conf.Commands.Prefix);

			// Handle char commands
			var sender = creature;
			var target = creature;
			var charCommand = message.StartsWith(ChannelServer.Instance.Conf.Commands.Prefix2);
			if (charCommand)
			{
				// Get target player
				if (args.Length < 2 || (target = WorldManager.Instance.GetPlayer(args[1])) == null)
				{
					Send.ServerMessage(creature, "Target not found.");
					return true;
				}

				// Any better way to remove the target? =/
				var tmp = new List<string>(args);
				tmp.RemoveAt(1);
				args = tmp.ToArray();
			}

			// Get command
			var command = this.GetCommand(args[0]);
			var commandConf = ChannelServer.Instance.Conf.Commands.GetAuth(args[0], command.Auth, command.CharAuth);

			// Check command
			if (command == null || ((!charCommand && client.Account.Authority < commandConf.Auth) || (charCommand && client.Account.Authority < commandConf.CharAuth)))
			{
				Send.ServerMessage(creature, "Unknown command '{0}'.", args[0]);
				return true;
			}

			// Run
			var result = command.Func(client, sender, target, message, args);

			// Handle result
			if (result == CommandResult.InvalidArgument)
			{
				Send.ServerMessage(creature, "Usage: {0} {1}", command.Name, command.Usage);
				if (command.CharAuth <= client.Account.Authority)
					Send.ServerMessage(creature, "Usage: {0} <target> {1}", command.Name, command.Usage);

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

		public CommandResult HandleTest(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			//for (int i = 0; i < args.Length; ++i)
			//{
			//    Send.ServerMessage(sender, "Arg{0}: {1}", i, args[i]);
			//}

			return CommandResult.Okay;
		}

		public CommandResult HandleWhere(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			var pos = target.GetPosition();
			var msg = Localization.Get(sender == target ? "gm.where_you" : "gm.where_target"); // (You're|{3} is) here: {0} @ {1}, {2}

			Send.ServerMessage(sender, msg, target.RegionId, pos.X, pos.Y, target.Name);

			return CommandResult.Okay;
		}

		public CommandResult HandleWarp(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			// Handles both warp and jump

			var warp = (args[0] == "warp");
			var offset = (warp ? 1 : 0);

			if (warp && args.Length < 2)
				return CommandResult.InvalidArgument;

			// Get region id
			int regionId = 0;
			if (warp)
			{
				if (!int.TryParse(args[1], out regionId))
				{
					Send.ServerMessage(sender, Localization.Get("gm.warp_invalid_id")); // Invalid region id.
					return CommandResult.InvalidArgument;
				}
			}
			else
				regionId = target.RegionId;

			int x = -1, y = -1;

			// Parse X
			if (args.Length > 1 + offset && !int.TryParse(args[1 + offset], out x))
			{
				Send.ServerMessage(sender, Localization.Get("gm.warp_invalid_x")); // Invalid X coordinate.
				return CommandResult.InvalidArgument;
			}

			// Parse Y
			if (args.Length > 2 + offset && !int.TryParse(args[2 + offset], out y))
			{
				Send.ServerMessage(sender, Localization.Get("gm.warp_invalid_y")); // Invalid Y coordinate.
				return CommandResult.InvalidArgument;
			}

			// Random coordinates if none were specified
			if (x == -1 && y == -1)
			{
				var rndc = AuraData.RegionInfoDb.RandomCoord(regionId);
				if (x < 0) x = rndc.X;
				if (y < 0) y = rndc.Y;
			}

			target.Warp(regionId, x, y);

			if (sender != target)
				Send.ServerMessage(target, Localization.Get("gm.warp_target"), sender.Name); // You've been warped by '{0}'.

			return CommandResult.Okay;
		}

		public CommandResult HandleGo(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			if (args.Length < 2)
			{
				Send.ServerMessage(sender,
					Localization.Get("gm.go_dest") + // Destinations:
					" Tir Chonaill, Dugald Isle, Dunbarton, Gairech, Bangor, Emain Macha, Taillteann, Nekojima, GM Island"
				);
				return CommandResult.InvalidArgument;
			}

			int regionId = -1, x = -1, y = -1;
			var destination = message.Substring(args[0].Length + 1).Trim();

			if (destination.StartsWith("tir")) { regionId = 1; x = 12801; y = 38397; }
			else if (destination.StartsWith("dugald")) { regionId = 16; x = 23017; y = 61244; }
			else if (destination.StartsWith("dun")) { regionId = 14; x = 38001; y = 38802; }
			else if (destination.StartsWith("gairech")) { regionId = 30; x = 39295; y = 53279; }
			else if (destination.StartsWith("bangor")) { regionId = 31; x = 12904; y = 12200; }
			else if (destination.StartsWith("emain")) { regionId = 52; x = 39818; y = 41621; }
			else if (destination.StartsWith("tail")) { regionId = 300; x = 212749; y = 192720; }
			else if (destination.StartsWith("neko")) { regionId = 600; x = 114430; y = 79085; }
			else if (destination.StartsWith("gm")) { regionId = 22; x = 2500; y = 2500; }
			else
			{
				Send.ServerMessage(sender, Localization.Get("gm.go_unk"), args[1]);
				return CommandResult.InvalidArgument;
			}

			if (regionId == -1 || x == -1 || y == -1)
			{
				Send.ServerMessage(sender, "Error while choosing destination.");
				Log.Error("HandleGo: Incomplete destination '{0}'.", args[1]);
				return CommandResult.Fail;
			}

			target.Warp(regionId, x, y);

			if (sender != target)
				Send.ServerMessage(target, Localization.Get("gm.warp_target"), sender.Name); // You've been warped by '{0}'.

			return CommandResult.Okay;
		}

		public CommandResult HandleItem(ChannelClient client, Creature sender, Creature target, string message, string[] args)
		{
			if (args.Length < 2)
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
						Send.ServerMessage(sender, Localization.Get("gm.item_mures"), args[1], itemData.Name, perc.ToString("0.0", CultureInfo.InvariantCulture)); // No exact match found for '{0}', using best result, '{1}' ({2}%).
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
				Send.ServerMessage(sender, Localization.Get("gm.item_nores"), args[1]); // Item '{0}' not found in database.
				return CommandResult.Fail;
			}

			var item = new Item(itemData.Id);

			// Check amount for stackable items
			if (itemData.StackType == StackType.Stackable && args.Length > 2)
			{
				int amount;

				// Get amount
				if (!int.TryParse(args[2], out amount) || amount <= 0)
				{
					Send.ServerMessage(sender, Localization.Get("gm.item_amount")); // Invalid amount.
					return CommandResult.Fail;
				}

				item.Amount = amount;
			}
			// Parse colors
			else if (itemData.StackType != StackType.Stackable && args.Length > 2)
			{
				for (int i = 0; i < 3; ++i)
				{
					if (args.Length < 3 + i)
						break;

					var sColor = args[2 + i];
					uint color = 0;

					// Hex color
					if (sColor.StartsWith("0x"))
					{
						if (!uint.TryParse(sColor.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out color))
						{
							Send.ServerMessage(sender, Localization.Get("gm.item_hex"), sColor); // Invalid hex color '{0}'.
							return CommandResult.Fail;
						}
					}
					else
					{
						switch (sColor)
						{
							case "black": color = 0x00000000; break;
							case "white": color = 0xFFFFFFFF; break;
							default:
								Send.ServerMessage(sender, Localization.Get("gm.item_color"), sColor); // Unknown color '{0}'.
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

			// Spawn item
			var success = true;
			if (!drop)
				success = target.Inventory.Add(item, Pocket.Temporary);
			else
				item.Drop(target.Region, target.GetPosition());

			if (success)
			{
				if (sender != target)
					Send.ServerMessage(target, Localization.Get("gm.item_target"), itemData.Name, sender.Name); // Item '{0}' has been spawned by '{1}'.
				Send.ServerMessage(sender, Localization.Get("gm.item_spawned"), itemData.Name); // Item '{0}' has been spawned.
				return CommandResult.Okay;
			}
			else
			{
				Send.ServerMessage(sender, Localization.Get("gm.item_fail")); // Failed to spawn item.
				return CommandResult.Fail;
			}
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

	public delegate CommandResult GmCommandFunc(ChannelClient client, Creature sender, Creature target, string message, string[] args);
}
