// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi;

namespace Aura.Channel.World.Dungeons.Props
{
	public class Switch : Prop
	{
		public string InternalName;

		public Switch(int id, int regionId, int x, int y, float direction, float scale = 1f, float altitude = 0,
			string state = "off", string name = "", string title = "", string intName = "")
			: base(id, regionId, x, y, direction, scale, altitude, state, name, title)
		{
			this.InternalName = intName;
			this.Behavior = DefaultSwitchBehavior;
		}

		private void DefaultSwitchBehavior(Creature creature, Prop prop)
		{
			if (this.IsTurnedOn())
				return;
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

		public bool IsTurnedOn()
		{
			return this.State == "on";
		}

		public static Switch CreateSwitch(int x, int y, float direction, uint color, int propId = 10202, int regionId = 0, string name = "")
		{
			direction = MabiMath.DegreeToRadian((int)direction);
			var s = new Switch(propId, regionId, x, y, direction, intName: name);
			s.Info.Color2 = color;
			return s;
		}
	}
}
