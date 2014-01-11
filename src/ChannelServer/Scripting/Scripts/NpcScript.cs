// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections;
using System.Web;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Shops;

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

		// Built-in
		// ------------------------------------------------------------------

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
	}
}
