// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Linq;
using System.Web;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Shops;
using Aura.Data;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class NpcScript : BaseScript
	{
		public NPC NPC { get; set; }
		public NpcShop Shop { get; set; }

		private Creature _player;
		public Creature Player
		{
			get
			{
				if (_player == null)
					throw new Exception("NpcScript: Missing player in " + this.GetType().Name);
				return _player;
			}
			set { _player = value; }
		}

		public NpcScript()
		{
			this.NPC = new NPC();
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Called when a player starts the conversation.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public virtual IEnumerable Talk()
		{
			Msg("...");
			yield break;
		}

		/// <summary>
		/// Sends Close, with the standard ending phrase.
		/// </summary>
		public virtual void EndConversation()
		{
			Close("(You ended your conversation with <npcname/>.)");
		}

		// Setup
		// ------------------------------------------------------------------

		/// <summary>
		/// Sets NPC's name.
		/// </summary>
		/// <param name="name"></param>
		protected void SetName(string name)
		{
			this.NPC.Name = name;
		}

		/// <summary>
		/// Sets NPC's location.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		protected void SetLocation(int regionId, int x, int y, byte direction = 0)
		{
			this.NPC.SetLocation(regionId, x, y);
			this.NPC.Direction = direction;
		}

		/// <summary>
		/// Sets NPC's race.
		/// </summary>
		/// <param name="raceId"></param>
		protected void SetRace(int raceId)
		{
			this.NPC.Race = raceId;
		}

		/// <summary>
		/// Sets NPC's body proportions.
		/// </summary>
		/// <param name="height"></param>
		/// <param name="weight"></param>
		/// <param name="upper"></param>
		/// <param name="lower"></param>
		protected void SetBody(float height = 1, float weight = 1, float upper = 1, float lower = 1)
		{
			this.NPC.Height = height;
			this.NPC.Weight = weight;
			this.NPC.Upper = upper;
			this.NPC.Lower = lower;
		}

		/// <summary>
		/// Sets NPC's face values.
		/// </summary>
		/// <param name="skinColor"></param>
		/// <param name="eyeType"></param>
		/// <param name="eyeColor"></param>
		/// <param name="mouthType"></param>
		protected void SetFace(byte skinColor = 0, byte eyeType = 0, byte eyeColor = 0, byte mouthType = 0)
		{
			this.NPC.SkinColor = skinColor;
			this.NPC.EyeType = eyeType;
			this.NPC.EyeColor = eyeColor;
			this.NPC.MouthType = mouthType;
		}

		/// <summary>
		/// Sets NPC's stand style.
		/// </summary>
		/// <param name="stand"></param>
		/// <param name="talkStand"></param>
		protected void SetStand(string stand, string talkStand = null)
		{
			this.NPC.StandStyle = stand;
			this.NPC.StandStyleTalking = talkStand;
		}

		/// <summary>
		/// Adds item to NPC's inventory.
		/// </summary>
		/// <param name="pocket"></param>
		/// <param name="itemId"></param>
		/// <param name="color1"></param>
		/// <param name="color2"></param>
		/// <param name="color3"></param>
		/// <param name="state">For robes and helmets</param>
		protected void EquipItem(Pocket pocket, int itemId, uint color1 = 0, uint color2 = 0, uint color3 = 0, ItemState state = ItemState.Up)
		{
			if (!pocket.IsEquip())
			{
				Log.Error("Pocket '{0}' is not for equipment ({1})", pocket, this.ScriptFilePath);
				return;
			}

			if (!AuraData.ItemDb.Exists(itemId))
			{
				Log.Error("Unknown item '{0}' ({1})", itemId, this.ScriptFilePath);
				return;
			}

			var item = new Item(itemId);
			item.Info.Pocket = pocket;
			item.Info.Color1 = color1;
			item.Info.Color2 = color2;
			item.Info.Color3 = color3;
			item.Info.State = (byte)state;

			this.NPC.Inventory.InitAdd(item);
		}

		/// <summary>
		/// Adds phrase to AI.
		/// </summary>
		/// <param name="phrase"></param>
		protected void AddPhrase(string phrase)
		{
			if (this.NPC.AI != null)
				this.NPC.AI.Phrases.Add(phrase);
		}

		protected void SetId(long entityId)
		{
			this.NPC.EntityId = entityId;
		}

		// Functions
		// ------------------------------------------------------------------

		/// <summary>
		/// Sends Msg with Bgm element.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="fileName"></param>
		protected void Bgm(string fileName)
		{
			this.Msg(new DialogBgm(fileName));
		}

		/// <summary>
		/// Opens NPC shop for creature.
		/// </summary>
		/// <param name="creature"></param>
		protected void OpenShop()
		{
			if (this.Shop == null)
			{
				this.Close("(Missing shop.)");
				return;
			}

			Send.OpenNpcShop(this.Player, this.Shop);
		}

		/// <summary>
		/// Joins lines and sends them as Msg,
		/// but only once per creature and NPC.
		/// </summary>
		/// <param name="lines"></param>
		protected void Intro(params object[] lines)
		{
			if (this.Player.Vars.Perm["npc_intro:" + this.NPC.Name] != null)
				return;

			this.Msg(Hide.Both, string.Join("<br/>", lines));
			this.Player.Vars.Perm["npc_intro:" + this.NPC.Name] = true;
		}

		/// <summary>
		/// Adds item(s) to player's inventory.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool GiveItem(int itemId, int amount = 1)
		{
			return Player.Inventory.Add(itemId, amount);
		}

		/// <summary>
		/// Execute Hook! Harhar.
		/// </summary>
		/// <remarks>
		/// Runs all hook funcs, one by one.
		/// </remarks>
		/// <param name="hookName"></param>
		/// <returns></returns>
		protected IEnumerable Hook(string hookName, params object[] args)
		{
			foreach (var hook in ChannelServer.Instance.ScriptManager.GetHooks(this.NPC.Name, hookName))
			{
				foreach (var call in hook(this, args))
				{
					var result = call as string;
					if (result != null && result == "break_hook")
						yield break;

					yield return call;
				}
			}
		}

		/// <summary>
		/// Returns true if quest is in progress.
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="objective"></param>
		/// <returns></returns>
		public bool QuestActive(int questId, string objective = null)
		{
			return (this.Player as PlayerCreature).Quests.IsActive(questId, objective);
		}

		/// <summary>
		/// Finishes objective in quest.
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="objective"></param>
		/// <returns></returns>
		public bool FinishQuest(int questId, string objective)
		{
			return (this.Player as PlayerCreature).Quests.Finish(questId, objective);
		}

		/// <summary>
		/// Returns current quest objective.
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="objective"></param>
		/// <returns></returns>
		public string QuestObjective(int questId)
		{
			var quest = (this.Player as PlayerCreature).Quests.Get(questId);
			if (quest == null)
				throw new Exception("NPC.GetQuestObjective: Player doesn't have quest '" + questId.ToString() + "'.");

			var current = quest.CurrentObjective;
			if (current == null)
				return null;

			return current.Ident;
		}

		/// <summary>
		/// (Re)Starts quest.
		/// </summary>
		/// <param name="questId"></param>
		public void StartQuest(int questId)
		{
			(this.Player as PlayerCreature).Quests.Start(questId);
		}

		/// <summary>
		/// Displays notice.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Notice(string format, params object[] args)
		{
			Send.Notice(this.Player, format, args);
		}

		// Dialog
		// ------------------------------------------------------------------

		/// <summary>
		/// Sends dialog to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="message"></param>
		/// <param name="elements"></param>
		public void Msg(Hide hide, string message, params DialogElement[] elements)
		{
			var mes = new DialogElement();

			if (hide == Hide.Face || hide == Hide.Both)
				mes.Add(new DialogFaceExpression(null));
			if (hide == Hide.Name || hide == Hide.Both)
				mes.Add(new DialogTitle(null));

			mes.Add(new DialogText(message));
			mes.Add(elements);

			this.Msg(mes);
		}

		/// <summary>
		/// Sends dialog to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="message"></param>
		/// <param name="elements"></param>
		public void Msg(string message, params DialogElement[] elements)
		{
			var mes = new DialogElement();
			mes.Add(new DialogText(message));
			mes.Add(elements);

			this.Msg(mes);
		}

		/// <summary>
		/// Sends dialog to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="elements"></param>
		public void Msg(params DialogElement[] elements)
		{
			var xml = string.Format(
				"<call convention='thiscall' syncmode='non-sync'>" +
					"<this type='character'>{0}</this>" +
					"<function>" +
						"<prototype>void character::ShowTalkMessage(character, string)</prototype>" +
							"<arguments>" +
								"<argument type='character'>{0}</argument>" +
								"<argument type='string'>{1}</argument>" +
							"</arguments>" +
						"</function>" +
				"</call>",
			this.Player.EntityId, HttpUtility.HtmlEncode(new DialogElement(elements).ToString()));

			Send.NpcTalk(this.Player, xml);
		}

		/// <summary>
		/// Closes dialog box, by sending NpcTalkEndR.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="message">Dialog closes immediately if null.</param>
		public void Close(string message = null)
		{
			Send.NpcTalkEndR(this.Player, this.NPC.EntityId, message);
			this.Player.Client.NpcSession.Clear();
		}

		/// <summary>
		/// Informs the client that something can be selected now.
		/// </summary>
		/// <remarks>
		/// Replaced by the pre-processor, to yield after the call,
		/// to wait for a response.
		/// If there actually is nothing to select, the last auto button
		/// (End Conversation) is gonna come in as a select.
		/// </remarks>
		/// <param name="creature"></param>
		public string Select()
		{
			var script = string.Format(
				"<call convention='thiscall' syncmode='sync' session='{1}'>" +
					"<this type='character'>{0}</this>" +
					"<function>" +
						"<prototype>string character::SelectInTalk(string)</prototype>" +
						"<arguments><argument type='string'>&#60;keyword&#62;&#60;gift&#62;</argument></arguments>" +
					"</function>" +
				"</call>"
			, this.Player.EntityId, this.Player.Client.NpcSession.Id);

			Send.NpcTalk(this.Player, script);

			return "Foo!";
		}

		/// <summary>
		/// Opens keyword window.
		/// </summary>
		/// <remarks>
		/// Select should be sent afterwards...
		/// so you can actually select a keyword.
		/// </remarks>
		/// <param name="creature"></param>
		protected void ShowKeywords()
		{
			var script = string.Format(
				"<call convention='thiscall' syncmode='non-sync'>" +
					"<this type='character'>{0}</this>" +
					"<function>" +
						"<prototype>void character::OpenTravelerMemo(string)</prototype>" +
						"<arguments>" +
							"<argument type='string'>(null)</argument>" +
						"</arguments>" +
					"</function>" +
				"</call>"
			, this.Player.EntityId);

			Send.NpcTalk(this.Player, script);
		}

		// Dialog factory
		// ------------------------------------------------------------------

		public DialogButton Button(string text, string keyword = null, string onFrame = null) { return new DialogButton(text, keyword, onFrame); }

		public DialogBgm SetBgm(string file) { return new DialogBgm(file); }

		public DialogImage Image(string name) { return new DialogImage(name, false, 0, 0); }
		public DialogImage Image(string name, int width, int height) { return new DialogImage(name, false, width, height); }
		public DialogImage Image(string name, bool localize, int width, int height) { return new DialogImage(name, localize, width, height); }

		public DialogList List(string text, int height, string cancelKeyword, params DialogButton[] elements) { return new DialogList(text, height, cancelKeyword, elements); }
		public DialogList List(string text, params DialogButton[] elements) { return this.List(text, (int)elements.Length, elements); }
		public DialogList List(string text, int height, params DialogButton[] elements) { return this.List(text, height, "@end", elements); }

		public DialogInput Input(string title = "Input", string text = "", byte maxLength = 20, bool cancelable = true) { return new DialogInput(title, text, maxLength, cancelable); }

		public DialogAutoContinue AutoContinue(int duration) { return new DialogAutoContinue(duration); }

		public DialogFaceExpression Expression(string expression) { return new DialogFaceExpression(expression); }

		public DialogMovie Movie(string file, int width, int height, bool loop = true) { return new DialogMovie(file, width, height, loop); }

		public DialogText Text(string format, params object[] args) { return new DialogText(format, args); }

		public DialogHotkey Hotkey(string text) { return new DialogHotkey(text); }

		// ------------------------------------------------------------------

		protected enum ItemState : byte { Up = 0, Down = 1 }
	}

	public enum Hide { Face, Name, Both }
}
