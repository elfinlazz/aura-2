// Copyright (c) Aura development team - Licensed nder GNU GPL
// For more information, see licence file in the main folder

using System.Runtime.InteropServices;

namespace Aura.Shared.Mabi.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ItemInfo
	{
		public byte Pocket;
		private byte __unknown2;
		private byte __unknown3;
		private byte __unknown4;
		public int Class;
		public uint Color1;
		public uint Color2;
		public uint Color3;
		public short Amount;
		private short __unknown7;
		public int Region;
		public int X;
		public int Y;

		/// <summary>
		/// State of the item? (eg. hoods and helmets)
		/// </summary>
		public byte State; // FigureA

		public byte uFigureB;
		public byte uFigureC;
		public byte uFigureD;
		public byte KnockCount;
		private byte __unknown12;
		private byte __unknown13;
		private byte __unknown14;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ItemOptionInfo
	{
		public byte Flag;
		private byte __unknown15;
		private byte __unknown16;
		private byte __unknown17;
		public int Price;
		public int SellingPrice;
		public int LinkedPocketId;
		public int Durability;
		public int DurabilityMax;
		public int DurabilityOriginal;
		public short AttackMin;
		public short AttackMax;
		public short WAttackMin;
		public short WAttackMax;
		public byte Balance;
		public byte Critical;
		private byte __unknown24;
		private byte __unknown25;
		public int Defense;
		public short Protection;
		public short EffectiveRange;
		public byte AttackSpeed;
		public byte KnockCount;
		public short Experience;
		public byte EP;
		public byte Upgraded;
		public byte UpgradeMax;
		public byte WeaponType;
		public int Grade;
		public short Prefix;
		public short Suffix;
		public short Elemental;
		private short __unknown31;
		public int ExpireTime;
		public int StackRemainingTime;
		public int JoustPointPrice;
		public int DucatPrice;
	}
}
