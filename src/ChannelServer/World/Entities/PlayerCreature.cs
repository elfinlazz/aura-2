// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Base class for creatures controlled by players.
	/// </summary>
	public abstract class PlayerCreature : Creature
	{
		private List<Entity> _visibleEntities = new List<Entity>();

		public override EntityType EntityType { get { return EntityType.Character; } }

		/// <summary>
		/// Creature id, for creature database.
		/// </summary>
		public long CreatureId { get; set; }

		/// <summary>
		/// Server this creature exists on.
		/// </summary>
		public string Server { get; set; }

		/// <summary>
		/// Time at which the creature can be deleted.
		/// </summary>
		public DateTime DeletionTime { get; set; }

		/// <summary>
		/// Time at which the creature was created.
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// Time of last rebirth.
		/// </summary>
		public DateTime LastRebirth { get; set; }

		/// <summary>
		/// Set to true if creature is supposed to be saved.
		/// </summary>
		public bool Save { get; set; }

		public override void Warp(int region, int x, int y)
		{
			this.SetLocation(region, x, y);
			Send.EnterRegion(this);
		}

		/// <summary>
		/// Updates visible creatures, sends Entities(Dis)Appear.
		/// </summary>
		public void LookAround()
		{
			var currentlyVisible = this.Region.GetVisibleEntities(this);

			var appear = currentlyVisible.Except(_visibleEntities);
			var disappear = _visibleEntities.Except(currentlyVisible);

			Send.EntitiesAppear(this.Client, appear);
			Send.EntitiesDisappear(this.Client, disappear);

			_visibleEntities = currentlyVisible.ToList();
		}
	}
}
