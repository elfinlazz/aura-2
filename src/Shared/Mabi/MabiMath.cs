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
		/// <remarks>
		/// While entity packets use a byte from 0-255 for the direction,
		/// props are using radian floats.
		/// </remarks>
		/// <param name="direction"></param>
		/// <returns></returns>
		public static float ByteToRadian(byte direction)
		{
			return (float)Math.PI * 2 / 255 * direction;
		}

		/// <summary>
		/// Converts vector direction into Mabi's byte direction.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static byte DirectionToByte(double x, double y)
		{
			return (byte)(Math.Floor(Math.Atan2(y, x) / 0.02454369260617026));
		}

		/// <summary>
		/// Calculates the stat bonus for eating food.
		/// </summary>
		/// <remarks>
		/// Formula: (Stat Boost * Hunger Filled) / (Hunger Fill * 20 * Current Age of Character)
		/// Reference: http://wiki.mabinogiworld.com/view/Food_List
		/// </remarks>
		/// <param name="boost"></param>
		/// <param name="hunger"></param>
		/// <param name="hungerFilled"></param>
		/// <param name="age"></param>
		/// <returns></returns>
		public static float FoodStatBonus(double boost, double hunger, double hungerFilled, int age)
		{
			return (float)((boost * hungerFilled) / (hunger * 20 * age));
		}
	}
}
