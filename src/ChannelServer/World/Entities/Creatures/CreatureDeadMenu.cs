// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Text;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureDeadMenu
	{
		public ReviveOptions Options { get; set; }

		public void Add(ReviveOptions option)
		{
			this.Options |= option;
		}

		public bool Has(ReviveOptions option)
		{
			return ((this.Options & option) != 0);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (this.Has(ReviveOptions.ArenaLobby))
				sb.Append("arena_lobby;");
			if (this.Has(ReviveOptions.ArenaSide))
				sb.Append("arena_side;");
			if (this.Has(ReviveOptions.ArenaWaitingRoom))
				sb.Append("arena_waiting;");
			if (this.Has(ReviveOptions.BarriLobby))
				sb.Append("barri_lobby;");
			if (this.Has(ReviveOptions.NaoStone))
				sb.Append("naocoupon;");
			if (this.Has(ReviveOptions.DungeonEntrance))
				sb.Append("dungeon_lobby;");
			if (this.Has(ReviveOptions.Here))
				sb.Append("here;");
			if (this.Has(ReviveOptions.HereNoPenalty))
				sb.Append("trnsfrm_pvp_here;");
			if (this.Has(ReviveOptions.HerePvP))
				sb.Append("showdown_pvp_here;");
			if (this.Has(ReviveOptions.InCamp))
				sb.Append("camp;");
			if (this.Has(ReviveOptions.StatueOfGoddess))
				sb.Append("dungeon_statue;");
			if (this.Has(ReviveOptions.TirChonaill))
				sb.Append("tirchonaill;");
			if (this.Has(ReviveOptions.Town))
				sb.Append("town;");
			if (this.Has(ReviveOptions.WaitForRescue))
				sb.Append("stay;");

			return sb.ToString().Trim(';');
		}
	}
}
