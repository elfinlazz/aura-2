// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Aura.Shared.Mabi
{
	/// <summary>
	/// Wrapper around zip working with strings.
	/// </summary>
	/// <remarks>
	/// Compression used for MML code. Little more than basic zlib,
	/// includes a header with the length of the uncompressed data.
	/// </remarks>
	public static class MabiZip
	{
		/// <summary>
		/// Returns compressed version of given string.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Compress(string str)
		{
			var barr = Encoding.Unicode.GetBytes(str + '\0');
			using (var mout = new MemoryStream())
			{
				// Deflate should use optimal compression level by default (0, as defined by .NET 4.5).
				using (var df = new DeflateStream(mout, CompressionMode.Compress))
				{
					// Write compressed data to memory stream.
					df.Write(barr, 0, barr.Length);
				}

				// Compression method
				int cmf = 8; // cm
				cmf += 7 << 4; // cinfo

				// Flags
				int flg = 2 << 6; // Compression level
				int n = ((cmf * 256) + flg) % 31;
				if (n != 0)
					flg += 31 - n; // Check bits

				// <length>;<cmf><flg><data><checksum>
				return string.Format("{0};{1:x02}{2:x02}{3}{4:x08}", barr.Length, cmf, flg, BitConverter.ToString(mout.ToArray()).Replace("-", "").ToLower(), ComputeAdler32(barr));
			}
		}

		/// <summary>
		/// Returns decompressed version of given string.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string Decompress(string str)
		{
			if (str.Length < 12) // zlib header + checksum
				throw new InvalidDataException("Compressed data seems too short.");

			// Strip length and zlib header
			var pos = str.IndexOf(';');
			if (pos == -1)
				pos = str.IndexOf("S");
			str = str.Substring((pos > -1 ? 4 + pos + 1 : 4));

			// Hex string to byte array
			int len = str.Length;
			var barr = new byte[len >> 1];
			for (int i = 0; i < len; i += 2)
				barr[i >> 1] = Convert.ToByte(str.Substring(i, 2), 16);

			// Decompress and return
			using (var mout = new MemoryStream())
			using (var min = new MemoryStream(barr))
			using (var df = new DeflateStream(min, CompressionMode.Decompress))
			{
				df.CopyTo(mout);
				return Encoding.Unicode.GetString(mout.ToArray());
			}
		}

		/// <summary>
		/// Returns Adler32 hash for the given byte array.
		/// </summary>
		/// <param name="bar"></param>
		/// <returns></returns>
		public static uint ComputeAdler32(byte[] bar)
		{
			ushort sum1 = 1, sum2 = 0;

			for (int i = 0; i < bar.Length; i++)
			{
				sum1 = (ushort)((sum1 + bar[i]) % 65521);
				sum2 = (ushort)((sum1 + sum2) % 65521);
			}

			return (uint)((sum2 << 16) | sum1);
		}
	}
}
