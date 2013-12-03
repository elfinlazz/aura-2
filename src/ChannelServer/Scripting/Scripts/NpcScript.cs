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

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class NpcScript : BaseScript
	{
		public NPC NPC { get; protected set; }

		public NpcScript()
		{
			this.NPC = new NPC();
		}

		public override void Load()
		{
			Log.Debug("!!!!!!!!!!!!!!!!!!!");
		}

		// ------------------------------------------------------------------

		public virtual IEnumerator Talk(Creature creature)
		{
			Msg(creature, "...");
			yield break;
		}

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
