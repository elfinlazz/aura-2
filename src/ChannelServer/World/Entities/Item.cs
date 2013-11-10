// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Structs;
using Aura.Data.Database;
using Aura.Data;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Entities
{
	public class Item : Entity
	{
		public override EntityType EntityType { get { return EntityType.Item; } }
		public override DataType DataType { get { return DataType.Item; } }
		public override int RegionId
		{
			get
			{
				return this.Info.Region;
			}
			set
			{
				this.Info.Region = value;
			}
		}

		public ItemInfo Info;
		public ItemOptionInfo OptionInfo;
		public ItemData Data { get; set; }

		private bool _firstTimeAppear = true;
		/// <summary>
		/// Returns true once, and false afterwards, until it's set true again.
		/// Used to decide whether the appearing item should bounce.
		/// </summary>
		public bool FirstTimeAppear
		{
			get
			{
				var result = _firstTimeAppear;
				_firstTimeAppear = false;
				return result;
			}
			set { _firstTimeAppear = true; }
		}

		public long QuestId { get; set; }

		public Item(int itemId)
		{
			this.Info.ItemId = itemId;

			this.Data = AuraData.ItemDb.Find(itemId);
		}

		public Item(Item baseItem)
		{
			//this.Info.Class = itemId;
		}

		public override Position GetPosition()
		{
			return new Position(this.Info.X, this.Info.Y);
		}

		public void Move(Pocket pocket, int x, int y)
		{
			this.Info.Pocket = pocket;
			this.Info.X = x;
			this.Info.Y = y;
		}

		public void Move(int region, int x, int y)
		{
			this.Info.Pocket = Pocket.None;
			this.Info.Region = this.RegionId = region;
			this.Info.X = x;
			this.Info.Y = y;
		}
	}
}
