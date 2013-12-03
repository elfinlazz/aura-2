// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Mabi.Structs;
using Aura.Shared.Mabi.Const;

namespace Aura.Login.Database
{
	public class Item
	{
		public long Id { get; set; }
		public ItemInfo Info;

		public Item()
		{
		}

		public Item(int itemId, Pocket pocket, uint color1, uint color2, uint color3)
		{
			this.Info.Id = itemId;
			this.Info.Pocket = pocket;
			this.Info.Color1 = color1;
			this.Info.Color2 = color2;
			this.Info.Color3 = color3;
		}

		/// <summary>
		/// Returns whether the item is in an equipment pocket (Head, Equip, Style).
		/// </summary>
		public bool IsVisible
		{
			get
			{
				// Head
				if (this.Info.Pocket >= Pocket.Face && this.Info.Pocket <= Pocket.Hair)
					return true;

				// Equipment
				if (this.Info.Pocket >= Pocket.Armor && this.Info.Pocket <= Pocket.Magazine2)
					return true;

				// Style
				if (this.Info.Pocket >= Pocket.ArmorStyle && this.Info.Pocket <= Pocket.RobeStyle)
					return true;

				return false;
			}
		}
	}
}
