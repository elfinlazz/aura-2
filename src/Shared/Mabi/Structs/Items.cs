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

		/// <summary>
		/// - Ego aura level (0-21)
		/// - Related to giant's beards
		/// </summary>
		public byte FigureB;

		public byte FigureC;
		public byte FigureD;
		public byte KnockCount;
		private byte __unknown12;
		private byte __unknown13;
		private byte __unknown14;
	}

	/// <summary>
	/// Private item info.
	/// </summary>
	/// <remarks>
	/// Explicit structure, to write LinkedPocketId as an int.
	/// </remarks>
	[StructLayout(LayoutKind.Explicit)]
	public struct ItemOptionInfo
	{
		[FieldOffset(0)]
		public ItemFlags Flags;
		[FieldOffset(1)]
		public byte __unknown15;
		[FieldOffset(2)]
		public byte __unknown16;
		[FieldOffset(3)]
		public byte __unknown17;

		[FieldOffset(4)]
		public int Price;
		[FieldOffset(8)]
		public int SellingPrice;

		[FieldOffset(12)]
		public Pocket LinkedPocketId;

		[FieldOffset(16)]
		public int Durability;
		[FieldOffset(20)]
		public int DurabilityMax;
		[FieldOffset(24)]
		public int DurabilityOriginal;

		[FieldOffset(28)]
		public ushort AttackMin;
		[FieldOffset(30)]
		public ushort AttackMax;

		[FieldOffset(32)]
		public ushort InjuryMin;
		[FieldOffset(34)]
		public ushort InjuryMax;

		[FieldOffset(36)]
		public byte Balance;
		[FieldOffset(37)]
		public sbyte Critical;
		[FieldOffset(38)]
		public byte __unknown24;
		[FieldOffset(39)]
		public byte __unknown25;

		[FieldOffset(40)]
		public int Defense;

		[FieldOffset(44)]
		public short Protection;
		[FieldOffset(46)]
		public short EffectiveRange;

		[FieldOffset(48)]
		public AttackSpeed AttackSpeed;
		[FieldOffset(49)]
		public byte KnockCount;
		[FieldOffset(50)]
		public short Experience;

		[FieldOffset(52)]
		public byte EP;
		[FieldOffset(53)]
		public byte Upgraded;
		[FieldOffset(54)]
		public byte UpgradeMax;
		[FieldOffset(55)]
		public byte WeaponType;

		[FieldOffset(56)]
		public int Grade;

		[FieldOffset(60)]
		public ushort Prefix;
		[FieldOffset(62)]
		public ushort Suffix;

		[FieldOffset(64)]
		public short Elemental;
		[FieldOffset(66)]
		public short __unknown31;

		[FieldOffset(68)]
		public int ExpireTime;
		[FieldOffset(72)]
		public int StackRemainingTime;
		[FieldOffset(76)]
		public int JoustPointPrice;
		[FieldOffset(80)]
		public int DucatPrice;
		[FieldOffset(84)]
		public int StarPrice;
		[FieldOffset(88)]
		public int PonsPrice;
		[FieldOffset(92)]
		public int __unknown3;
	}

	/// <summary>
	/// Positions of the pickers when using regular dye.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DyePickers
	{
		public DyePicker Picker1;
		public DyePicker Picker2;
		public DyePicker Picker3;
		public DyePicker Picker4;
		public DyePicker Picker5;
	}

	/// <summary>
	/// Position of a picker when using regular dye.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DyePicker
	{
		public short X, Y;
	}
}
