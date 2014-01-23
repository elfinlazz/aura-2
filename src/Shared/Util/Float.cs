// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Aura.Shared.Util
{
	public static class FloatExtensions
	{
		public static string ToInvariant(this float f, string format = "g")
		{
			return f.ToString(format, CultureInfo.InvariantCulture);
		}

		public static string ToInvariant(this double f, string format = "g")
		{
			return f.ToString(format, CultureInfo.InvariantCulture);
		}
	}
}
