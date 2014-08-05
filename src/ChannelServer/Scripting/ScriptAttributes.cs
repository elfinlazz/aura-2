// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.Scripting
{
	/// <summary>
	/// Defines a type that's not to load.
	/// </summary>
	/// <remarks>
	/// Use when overriding an existing NPC, to only load one version of it.
	/// </remarks>
	public class OverrideAttribute : Attribute
	{
		public string TypeName { get; private set; }

		public OverrideAttribute(string typeName)
		{
			this.TypeName = typeName;
		}
	}

	/// <summary>
	/// Defines types to remove from loading list.
	/// </summary>
	/// <remarks>
	/// List types that are to be removed from the loading list.
	/// Similar to Override in functionality.
	/// </remarks>
	public class RemoveAttribute : Attribute
	{
		public string[] TypeNames { get; private set; }

		public RemoveAttribute(params string[] typeNames)
		{
			this.TypeNames = typeNames;
		}
	}
}
