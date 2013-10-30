// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using Aura.Shared.Mabi.Structs;

namespace Aura.Login.Database
{
	public class Item
	{
		public long Id { get; set; }
		public ItemInfo Info;

		/// <summary>
		/// Returns whether the item is in an equipment pocket (Head, Equip, Style).
		/// </summary>
		public bool IsVisible
		{
			get
			{
				// Head
				if (this.Info.Pocket >= 3 && this.Info.Pocket <= 4)
					return true;

				// Equipment
				if (this.Info.Pocket >= 5 && this.Info.Pocket <= 15)
					return true;

				// Style
				if (this.Info.Pocket >= 43 && this.Info.Pocket <= 47)
					return true;

				return false;
			}
		}
	}
}
