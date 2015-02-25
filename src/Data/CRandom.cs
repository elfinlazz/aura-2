/*
 * Altered Mersenne Twister
 * Based on the pseudocode in https://en.wikipedia.org/wiki/Mersenne_Twister.
 * Generates uniformly distributed 32-bit integers in the range [0, 232 − 1] with the MT19937 algorithm
 * Yaşar Arabacı <yasar11732 et gmail nokta com>
 * C# implementaion and modifications by tachiorz
*/

using System;
using System.Runtime.Serialization;

namespace Aura.Data.Database
{
	[Serializable]
	public class CRandomException : Exception
	{
		public CRandomException()
		{
		}

		public CRandomException(string message)
			: base(message)
		{
		}

		public CRandomException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected CRandomException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	public class CRandom
	{
		private const int N = 624;
		private uint[] _mt;
		private int _mti;
		private uint _mt_ptr;

		public CRandom(uint seed)
		{
			_mti = 0;
			_mt = new uint[N];

			var x = seed;
			for (var i = 0; i < N; i++)
			{
				_mt[i] = x & 0xFFFF0000;
				x = (69069 * x) & 0xFFFFFFFF;
				_mt[i] |= x >> 16;
				x = 69069 * x + 69070;
			}
			this.Reload();
		}

		private void Reload()
		{
			for (var i = 0; i < N; i++)
			{
				var y = (_mt[i] & 0x80000000) + (_mt[(i + 1) % N] & 0x7FFFFFFF);
				_mt[i] = _mt[(i + 397) % N] ^ (y >> 1);
				if (y % 2 != 0)
					_mt[i] ^= 2567483615;
			}
			_mti = N;
			_mt_ptr = 0;
		}

		public uint RandomU32()
		{
			var temp_i = _mti--;
			if (temp_i == 0)
				Reload();

			uint y = 0;
			if (_mt_ptr < N)
				y = _mt[_mt_ptr++];
			else
			{
				++_mt_ptr;
				throw new CRandomException();
			}

			y ^= (y >> 11);
			y ^= (y << 7) & 0x9D2C5680U;
			y ^= (y << 15) & 0xEFC60000U;
			y ^= (y >> 18);
			return y;
		}

		public float RandomF32()
		{
			return (float)((this.RandomU32() & 0x7FFFFFFF) * 4.656612873077393e-10);
		}

		public float RandomF32(float start, float end)
		{
			return this.RandomF32() * (end - start) + start;
		}
	}
}