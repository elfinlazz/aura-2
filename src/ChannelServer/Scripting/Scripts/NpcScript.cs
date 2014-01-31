// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using System.Collections;
using System.Web;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Shops;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using Aura.Data;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class NpcScript : BaseScript
	{
		public NPC NPC { get; protected set; }
		public NpcShop Shop { get; protected set; }

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
		public virtual IEnumerable Talk(Creature creature)
		{
			Msg(creature, "...");
			yield break;
		}

		public virtual void EndConversation(Creature creature)
		{
			Close(creature, "(You ended your conversation with <npcname/>.)");
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
		protected void SetStand(string stand)
		{
			this.NPC.StandStyle = stand;
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
		protected void EquipItem(Pocket pocket, int itemId, uint color1, uint color2, uint color3, ItemState state = ItemState.Up)
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
		/// Adds item to NPC's inventory.
		/// </summary>
		/// <param name="pocket"></param>
		/// <param name="itemId"></param>
		/// <param name="color1"></param>
		/// <param name="state">For robes and helmets</param>
		protected void EquipItem(Pocket pocket, int itemId, uint color1, ItemState state = ItemState.Up)
		{
			EquipItem(pocket, itemId, color1, 0, 0, state);
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

		// Functions
		// ------------------------------------------------------------------

		/// <summary>
		/// Sends Msg with Bgm element.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="fileName"></param>
		protected void Bgm(Creature creature, string fileName)
		{
			this.Msg(creature, new DialogBgm(fileName));
		}

		/// <summary>
		/// Opens NPC shop for creature.
		/// </summary>
		/// <param name="creature"></param>
		protected void OpenShop(Creature creature)
		{
			if (this.Shop == null)
			{
				this.Close(creature, "(Missing shop.)");
				return;
			}

			Send.OpenNpcShop(creature, this.Shop);
		}

		protected void Intro(Creature creature, params object[] lines)
		{
			this.Msg(creature, Hide.Both, string.Join("<br/>", lines));
		}

		// Dialog
		// ------------------------------------------------------------------

		/// <summary>
		/// Sends dialog to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="message"></param>
		/// <param name="elements"></param>
		protected void Msg(Creature creature, Hide hide, string message, params DialogElement[] elements)
		{
			var mes = new DialogElement();

			if (hide == Hide.Face || hide == Hide.Both)
				mes.Add(new DialogFace(null));
			if (hide == Hide.Name || hide == Hide.Both)
				mes.Add(new DialogTitle(null));

			mes.Add(new DialogText(message));
			mes.Add(elements);

			this.Msg(creature, mes);
		}

		/// <summary>
		/// Sends dialog to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="message"></param>
		/// <param name="elements"></param>
		protected void Msg(Creature creature, string message, params DialogElement[] elements)
		{
			var mes = new DialogElement();
			mes.Add(new DialogText(message));
			mes.Add(elements);

			this.Msg(creature, mes);
		}

		/// <summary>
		/// Sends dialog to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="elements"></param>
		protected void Msg(Creature creature, params DialogElement[] elements)
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
			creature.EntityId, HttpUtility.HtmlEncode(new DialogElement(elements).ToString()));

			Send.NpcTalk(creature, xml);
		}

		/// <summary>
		/// Closes dialog box, by sending NpcTalkEndR.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="message">Dialog closes immediately if null.</param>
		protected void Close(Creature creature, string message = null)
		{
			Send.NpcTalkEndR(creature, this.NPC.EntityId, message);
			creature.Client.NpcSession.Clear();
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
		public string Select(Creature creature)
		{
			var script = string.Format(
				"<call convention='thiscall' syncmode='sync' session='{1}'>" +
					"<this type='character'>{0}</this>" +
					"<function>" +
						"<prototype>string character::SelectInTalk(string)</prototype>" +
						"<arguments><argument type='string'>&#60;keyword&#62;&#60;gift&#62;</argument></arguments>" +
					"</function>" +
				"</call>"
			, creature.EntityId, creature.Client.NpcSession.Id);

			Send.NpcTalk(creature, script);

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
		protected void ShowKeywords(Creature creature)
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
			, creature.EntityId);

			Send.NpcTalk(creature, script);
		}

		// Dialog factory
		// ------------------------------------------------------------------

		protected DialogButton Button(string text, string keyword = null, string onFrame = null)
		{
			return new DialogButton(text, keyword, onFrame);
		}

		protected DialogBgm Bgm(string file)
		{
			return new DialogBgm(file);
		}

		protected DialogImage Image(string name, bool localize = false, int width = 0, int height = 0)
		{
			return new DialogImage(name, localize, width, height);
		}

		protected DialogList List(string text, int height, string cancelKeyword, params DialogButton[] elements)
		{
			return new DialogList(text, height, cancelKeyword, elements);
		}

		protected DialogList List(string text, params DialogButton[] elements)
		{
			return this.List(text, (int)elements.Length, elements);
		}

		protected DialogList List(string text, int height, params DialogButton[] elements)
		{
			return this.List(text, height, "@end", elements);
		}

		protected DialogInput Input(string title = "Input", string text = "", byte maxLength = 20, bool cancelable = true)
		{
			return new DialogInput(title, text, maxLength, cancelable);
		}

		protected DialogAutoContinue AutoContinue(int duration)
		{
			return new DialogAutoContinue(duration);
		}

		protected DialogFace Face(string expression)
		{
			return new DialogFace(expression);
		}

		protected DialogMovie Movie(string file, int width, int height, bool loop = true)
		{
			return new DialogMovie(file, width, height, loop);
		}

		// ------------------------------------------------------------------

		protected enum ItemState : byte { Up = 0, Down = 1 }
		protected enum Hide { Face, Name, Both }
	}
}
