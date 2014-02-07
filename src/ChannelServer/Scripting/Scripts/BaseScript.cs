// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.Scripting.Scripts
{
	public abstract partial class BaseScript
	{
		public string ScriptFilePath { get; set; }

		protected ScriptVariables GlobalVars { get { return ChannelServer.Instance.ScriptManager.GlobalVars; } }

		public virtual void Load()
		{
		}

		protected void CreatureSpawn(int raceId, int amount, int regionId, params int[] coordinates)
		{
			ChannelServer.Instance.ScriptManager.AddCreatureSpawn(new CreatureSpawn(raceId, amount, regionId, coordinates));
		}
	}
}