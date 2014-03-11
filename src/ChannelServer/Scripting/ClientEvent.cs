// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aura.Channel.World.Entities;
using Aura.Data.Database;

namespace Aura.Channel.Scripting
{
	public class ClientEvent
	{
		public long Id { get; private set; }
		public SignalType Signal { get; private set; }
		public Action<Creature, EventData> OnTriggered { get; private set; }

		public ClientEvent(long id, SignalType signal, Action<Creature, EventData> onTriggered)
		{
			this.Id = id;
			this.Signal = signal;
			this.OnTriggered = onTriggered;
		}
	}
}
