// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Puzzles
{
	public interface IMonsterGroup
	{
		void AddKeyForLock(IPuzzlePlace lockPlace);
	}

	public class MonsterGroup : IMonsterGroup
	{
		public void AddKeyForLock(IPuzzlePlace lockPlace)
		{
			throw new System.NotImplementedException();
		}
	}
}
