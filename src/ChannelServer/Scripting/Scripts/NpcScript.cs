// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Aura.Channel.Network;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending;
using Aura.Channel.World;
using System.Text.RegularExpressions;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class NpcScript : BaseScript
	{
		public NPC NPC { get; protected set; }

		public NpcScript()
		{
			this.NPC = new NPC();
		}

		// ------------------------------------------------------------------

		public virtual IEnumerable Talk(Creature creature)
		{
			Msg(creature, "...");
			yield break;
		}

		// Setup
		// ------------------------------------------------------------------

		protected void SetName(string name)
		{
			this.NPC.Name = name;
		}

		protected void SetLocation(int regionId, int x, int y, byte direction = 0)
		{
			this.NPC.SetLocation(regionId, x, y);
			this.NPC.Direction = direction;
		}

		// Built-in
		// ------------------------------------------------------------------

		protected void Msg(Creature creature, string message, params DialogElement[] elements)
		{
			var mes = new DialogElement();
			mes.Add(new DialogText(message));
			mes.Add(elements);

			this.Msg(creature, mes);
		}

		protected void Msg(Creature creature, params DialogElement[] elements)
		{
			this.SendScript(creature, new DialogElement(elements));
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

		public virtual void Select(Creature creature)
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
		}

		// Dialog factory
		// ------------------------------------------------------------------

		protected DialogButton Button(string text, string keyword = null, string onFrame = null)
		{
			return new DialogButton(text, keyword, onFrame);
		}

		// Building
		// ------------------------------------------------------------------

		protected void SendScript(Creature creature, DialogElement element)
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
			creature.EntityId, HttpUtility.HtmlEncode(element.ToString()));

			Send.NpcTalk(creature, xml);
		}
	}
}
