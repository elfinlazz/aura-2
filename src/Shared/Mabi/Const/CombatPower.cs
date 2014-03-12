// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Shared.Mabi.Const
{
	/// <summary>
	/// Power of one creature compared to another.
	/// </summary>
	/// <remarks>
	///      < 0.8x  Weakest
	/// 0.8x - 1.0x  Weak
	/// 1.0x - 1.4x  Normal
	/// 1.4x - 2.0x  Strong
	/// 2.0x - 3.0x  Awful
	///      > 3.0x  Boss
	/// </remarks>
	public enum PowerRating
	{
		Weakest,
		Weak,
		Normal,
		Strong,
		Awful,
		Boss,
	}
}
