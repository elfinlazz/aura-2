// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Mabi;

namespace Aura.Channel.World.Dungeons.Props
{
	/// <summary>
	/// Switch prop, as found in dungeons.
	/// </summary>
	public class Switch : DungeonProp
	{
		/// <summary>
		/// Returns true if the switch is currently on.
		/// </summary>
		public bool IsTurnedOn { get { return this.State == "on"; } }

		/// <summary>
		/// Creates new switch prop with a default prop id.
		/// </summary>
		/// <param name="name">Name of the prop.</param>
		/// <param name="color">Color of the orb.</param>
		public Switch(string name, uint color)
			: this(10202, name, color)
		{
		}

		/// <summary>
		/// Creates new switch prop.
		/// </summary>
		/// <param name="propId">Id of the switch prop.</param>
		/// <param name="name">Name of the prop.</param>
		/// <param name="color">Color of the orb.</param>
		public Switch(int propId, string name, uint color)
			: base(propId, name)
		{
			this.Name = name;
			this.State = "off";
			this.Info.Color2 = color;

			this.Behavior = this.DefaultBehavior;
		}

		/// <summary>
		/// Default behavior of the switch, turning it on.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		private void DefaultBehavior(Creature creature, Prop prop)
		{
			if (!this.IsTurnedOn)
				this.TurnOn();
		}

		/// <summary>
		/// Turns switch on.
		/// </summary>
		public void TurnOn()
		{
			this.SetState("on");
		}

		/// <summary>
		/// Turns switch off.
		/// </summary>
		public void TurnOff()
		{
			this.SetState("off");
		}
	}
}
