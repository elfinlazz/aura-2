// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Runtime.InteropServices;

namespace Aura.Shared.Mabi.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BodyProportions
	{
		public float Height;
		public float Weight;
		public float Upper;
		public float Lower;
	}
}
