// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Runtime.InteropServices;

namespace Aura.Shared.Mabi.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PropInfo
	{
		public int Id;
		public int Region;
		public float X;
		public float Altitude;
		public float Y;
		public float Direction;
		public float Scale;
		public uint Color1;
		public uint Color2;
		public uint Color3;
		public uint Color4;
		public uint Color5;
		public uint Color6;
		public uint Color7;
		public uint Color8;
		public uint Color9;
		public byte FixedAltitude;
		private byte __unknown65;
		private byte __unknown66;
		private byte __unknown67;
	}
}
