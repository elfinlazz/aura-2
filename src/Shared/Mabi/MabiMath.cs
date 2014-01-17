// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Mabi
{
	public static class MabiMath
	{
		/// <summary>
		/// Converts Mabi's byte direction into a radian.
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static float ByteToRadian(byte direction)
		{
			return (float)Math.PI * 2 / 255 * direction;
		}

		/// <summary>
		/// Converts direction into Mabi's byte direction.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static byte DirectionToByte(double x, double y)
		{
			return (byte)(Math.Floor(Math.Atan2(x, y) / 0.02454369260617026));
		}
	}
}
