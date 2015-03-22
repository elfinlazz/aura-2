// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
	public enum NoticeType
	{
		Top = 1,
		TopRed,
		MiddleTop,
		Middle,
		Left,
		TopGreen,
		MiddleSystem,
		System,
		MiddleLower,
	}

	public enum MsgBoxTitle
	{
		Notice,
		Info,
		Warning,
		Confirm,
	}

	public enum MsgBoxButtons : byte
	{
		None,
		Close,
		OkCancel,
		YesNoCancel,
	}

	public enum MsgBoxAlign : byte
	{
		Left,
		Center,
	}
}
