// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Scripting.Scripts
{
	public interface IScript
	{
		bool Init();
	}

	public interface IAutoLoader
	{
		void AutoLoad();
	}
}
