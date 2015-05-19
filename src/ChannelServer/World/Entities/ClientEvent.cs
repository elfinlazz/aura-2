// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// A client in an area of a region.
	/// </summary>
	/// <remarks>
	/// Is this considered to be an entity by the client?
	/// </remarks>
	public class ClientEvent
	{
		/// <summary>
		/// Handler that is called if this event is triggered
		/// </summary>
		public Collection<SignalType, Action<Creature, EventData>> Handlers { get; private set; }

		/// <summary>
		/// Event's id
		/// </summary>
		public long EntityId { get; private set; }

		/// <summary>
		/// Data for this event
		/// </summary>
		public EventData Data { get; private set; }

		/// <summary>
		/// Creates new client event
		/// </summary>
		/// <param name="eventData"></param>
		public ClientEvent(long id, EventData eventData)
		{
			this.EntityId = id;
			this.Data = eventData;

			this.Handlers = new Collection<SignalType, Action<Creature, EventData>>();
		}
	}
}
