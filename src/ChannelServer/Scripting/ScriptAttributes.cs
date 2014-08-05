// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.Scripting
{
	public class OverrideAttribute : Attribute
	{
		public string TypeName { get; private set; }

		public OverrideAttribute(string typeName)
		{
			this.TypeName = typeName;
		}
	}

	public class RemoveAttribute : Attribute
	{
		public string[] TypeNames { get; private set; }

		public RemoveAttribute(params string[] typeNames)
		{
			this.TypeNames = typeNames;
		}
	}
}
