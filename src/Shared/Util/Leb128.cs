using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Shared.Util
{
	/// <summary>
	/// Provides methods for dealing with Leb128
	/// encoded values
	/// 
	/// http://en.wikipedia.org/wiki/LEB128
	/// </summary>
	public static class Leb128
	{
		private const int _low7Bits = 0x7F;
		private const int _highBit = 0x80;

		/// <summary>
		/// Encodes an integer in LEB128 unsigned
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static byte[] Encode(int value)
		{
			// Allocate buffer sized to the max number this value will take up
			var buff = new byte[value / (1 << 7) + 1];
			
			var i = 0;

			do
			{
				// Store low 7 bits in output
				buff[i] = (byte) (value & _low7Bits);
				value >>= 7;
				i++;
			} while (value != 0);

			// Set the last byte flag
			buff[i-1] |= _highBit;

			var ret = new byte[i];
			Buffer.BlockCopy(buff, 0, ret, 0, ret.Length);

			return ret;
		}

		/// <summary>
		/// Decodes an integer stored as LEB128 unsigned
		/// </summary>
		/// <param name="raw"></param>
		/// <returns></returns>
		public static int Decode(byte[] raw)
		{
			var num = 0;


			for (var i = 0; ; i++)
			{
				// Restore bits
				num |= (raw[i] & _low7Bits) << 7 * i;

				// If this was the last byte, we're done
				if ((raw[i] & _highBit) != 0)
					break;
			}

			return num;
		}
	}
}
