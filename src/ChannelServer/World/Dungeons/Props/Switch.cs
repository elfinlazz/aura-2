// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Mabi;

namespace Aura.Channel.World.Dungeons.Props
{
	public class Switch : DungeonProp
	{
		public bool IsTurnedOn { get { return this.State == "on"; } }

		public Switch(string name, uint color)
			: this(10202, name, color)
		{
		}

		public Switch(int propId, string name, uint color)
			: base(propId, name)
		{
			this.InternalName = name;
			this.State = "off";
			this.Info.Color2 = color;

			this.Behavior = this.DefaultBehavior;
		}

		private void DefaultBehavior(Creature creature, Prop prop)
		{
			if (!this.IsTurnedOn)
				this.TurnOn();
		}

		public void TurnOn()
		{
			this.SetState("on");
		}

		public void TurnOff()
		{
			this.SetState("off");
		}
	}
}
