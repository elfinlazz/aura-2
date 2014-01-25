// Copyright (c) Aura development team - Licensed nder GNU GPL
// For more information, see license file in the main folder

using System.Runtime.InteropServices;
using Aura.Shared.Mabi.Const;

namespace Aura.Shared.Mabi.Structs
{
	/// <summary>
	/// Public item info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ItemInfo
	{
		public Pocket Pocket;
		private byte __unknown2;
		private byte __unknown3;
		private byte __unknown4;
		public int Id;
		public uint Color1;
		public uint Color2;
		public uint Color3;
		public ushort Amount;
		private short __unknown7;
		public int Region;
		public int X;
		public int Y;

		/// <summary>
		/// State of the item? (eg. hoods and helmets)
		/// </summary>
		public byte State; // FigureA

		public byte uFigureB; // related to giant's beards
		public byte uFigureC;
		public byte uFigureD;
		public byte KnockCount;
		private byte __unknown12;
		private byte __unknown13;
		private byte __unknown14;
	}

	/// <summary>
	/// Private item info.
	/// </summary>
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
		public ushort AttackMin;
		public ushort AttackMax;
		public ushort WAttackMin;
		public ushort WAttackMax;
		public byte Balance;
		public byte Critical;
		private byte __unknown24;
		private byte __unknown25;
		public int Defense;
		public short Protection;
		public short EffectiveRange;
		public AttackSpeed AttackSpeed;
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
		public int StarPrice;
		public int PonsPrice;
		private int __unknown3;
	}
}
