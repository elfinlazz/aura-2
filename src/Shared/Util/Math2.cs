// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Shared.Util
{
	public static class Math2
	{
		public static int MinMax(int min, int max, int val)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;
			return val;
		}
	}
}
