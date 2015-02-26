// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aura.Data.Database;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureDrops
	{
		/// <summary>
		/// Circle pattern for dropping 21 stacks of gold.
		/// </summary>
		public static int[,] MaxGoldPattern = new int[,] { 
			            {-50,100},  {0,100},  {50,100},
			{-100,50},  {-50,50},   {0,50},   {50,50},   {100,50},
			{-100,0},   {-50,0},    {0,0},    {50,0},    {100,0},
			{-100,-50}, {-50,-50},  {0,-50},  {50,-50},  {100,-50},
			            {-50,-100}, {0,-100}, {50,-100},
		};

		private Creature _creature;
		private Dictionary<int, DropData> _drops;

		public int GoldMin { get; set; }
		public int GoldMax { get; set; }
		public ICollection<DropData> Drops { get { lock (_drops) return _drops.Values.ToArray(); } }

		public CreatureDrops(Creature creature)
		{
			_creature = creature;
			_drops = new Dictionary<int, DropData>();
		}

		/// <summary>
		/// Adds drop
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="chance"></param>
		public void Add(int itemId, float chance)
		{
			lock (_drops)
				_drops[itemId] = new DropData(itemId, chance);
		}

		/// <summary>
		///  Adds all drops
		/// </summary>
		/// <param name="drops"></param>
		public void Add(ICollection<DropData> drops)
		{
			lock (_drops)
			{
				foreach (var drop in drops)
					_drops[drop.ItemId] = drop.Copy();
			}
		}
	}
}
