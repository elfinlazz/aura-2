/* 
   A C-program for MT19937, with initialization improved 2002/1/26.
   Coded by Takuji Nishimura and Makoto Matsumoto.

   Before using, initialize the state by using init_genrand(seed)  
   or init_by_array(init_key, key_length).

   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.                          

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

	 1. Redistributions of source code must retain the above copyright
		notice, this list of conditions and the following disclaimer.

	 2. Redistributions in binary form must reproduce the above copyright
		notice, this list of conditions and the following disclaimer in the
		documentation and/or other materials provided with the distribution.

	 3. The names of its contributors may not be used to endorse or promote 
		products derived from this software without specific prior written 
		permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


   Any feedback is very welcome.
   http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html
   email: m-mat @ math.sci.hiroshima-u.ac.jp (remove space)
*/

namespace Aura.Data
{
	// Implementation by Yiting
	public class MTRandom
	{
		private const int N = 624;
		private const int M = 397;
		private const uint MATRIX_A = 0x9908B0DF;

		private uint[] _mt;
		private int _mti;

		public MTRandom(int seed = 5498)
			: this((uint)seed)
		{ }

		public MTRandom(uint seed = 5498)
		{
			_mti = N;
			_mt = new uint[N];

			uint x = seed;
			_mt[0] = x;
			for (int i = 1; i < N; i++)
			{
				x = 1812433253U * (x ^ (x >> 30)) + (uint)i;
				_mt[i] = x;
			}
		}

		/// <summary>
		/// Generates random number between 0 and uint max.
		/// </summary>
		/// <returns></returns>
		public uint GetUInt32()
		{
			if (_mti >= N)
			{
				for (int i = 0; i < N - M; i++)
				{
					uint x = _mt[i];
					x ^= (x ^ _mt[i + 1]) & 0x7FFFFFFF;
					_mt[i] = _mt[i + M] ^ (x >> 1) ^ ((x & 1) == 0 ? 0 : MATRIX_A);
				}
				for (int i = N - M; i < N - 1; i++)
				{
					uint x = _mt[i];
					x ^= (x ^ _mt[i + 1]) & 0x7FFFFFFF;
					_mt[i] = _mt[i + M - N] ^ (x >> 1) ^ ((x & 1) == 0 ? 0 : MATRIX_A);
				}
				{
					uint x = _mt[N - 1];
					x ^= (x ^ _mt[0]) & 0x7FFFFFFF;
					_mt[N - 1] = _mt[M - 1] ^ (x >> 1) ^ ((x & 1) == 0 ? 0 : MATRIX_A);
				}
				_mti = 0;
			}

			uint y = _mt[_mti++];
			y ^= (y >> 11);
			y ^= (y << 7) & 0x9D2C5680U;
			y ^= (y << 15) & 0xEFC60000U;
			y ^= (y >> 18);
			return y;
		}

		/// <summary>
		/// Generates random number between 0 and max - 1.
		/// Example: max = 10 returns a number from 0 to 9.
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public uint GetUInt32(uint max)
		{
			return this.GetUInt32(0, max - 1);
		}

		/// <summary>
		/// Generates random number between min and max.
		/// Example: min = 1 and max = 10 returns a number from 1 to 10.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public uint GetUInt32(uint min, uint max)
		{
			return (uint)(this.GetDouble() * (max + 1 - min) + min);
		}

		/// <summary>
		/// Generates a random number between 0.0 and 1.0 (not including 1.0).
		/// </summary>
		/// <returns></returns>
		public double GetDouble()
		{
			return ((double)this.GetUInt32() / 0xFFFFFFFFU);
		}
	}
}
