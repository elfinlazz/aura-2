// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Network.Crypto
{
	/// <summary>
	/// Implements a Mabinogi-style AES cipher
	/// </summary>
	public sealed class MabiAesEngine
	{
		/// <summary>
		/// Any data left over after processing all multiples of 16
		/// is XORed with this array.
		/// </summary>
		private static readonly byte[] _remainderMask = { 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10, 0xf, 0x1e, 0x2d, 0x3c, 0x4b, 0x5a, 0x69, 0x78 };

		private readonly AesFastEngine _aesEngine;

		/// <summary>
		/// Creates a new instance of the mabi cipher
		/// </summary>
		/// <param name="forEncryption">True if the cipher will be used to encrypt data, false otherwise</param>
		/// <param name="key">The key, in little endian form</param>
		public MabiAesEngine(bool forEncryption, byte[] key)
		{
			_aesEngine = new AesFastEngine();

			_aesEngine.Init(forEncryption, key);
		}

		/// <summary>
		/// Transforms a packet
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public unsafe void ProcessPacket(byte[] packet, int offset, int count)
		{
			int ptr = offset, ctr = count;

			var temp = new byte[16];

			fixed (byte* tmpPtr = temp, packetPtr = packet)
			{
				// Read and encrypt 16 byte chunks with the provided cipher
				// Write the processed values right back to the packet
				while (ctr >= 16)
				{
					// Mabi's cipher treats blocks as Big Endian
					// But standard AES treats them as Little, so we need to swap our data
					for (var i = 0; i < 16; i += 4)
					{
						*(tmpPtr + i + 0) = *(packetPtr + ptr + i + 3);
						*(tmpPtr + i + 1) = *(packetPtr + ptr + i + 2);
						*(tmpPtr + i + 2) = *(packetPtr + ptr + i + 1);
						*(tmpPtr + i + 3) = *(packetPtr + ptr + i + 0);
					}

					_aesEngine.ProcessBlock(temp, 0, temp, 0);

					for (var i = 0; i < 16; i += 4)
					{
						*(packetPtr + ptr + i + 0) = *(tmpPtr + i + 3);
						*(packetPtr + ptr + i + 1) = *(tmpPtr + i + 2);
						*(packetPtr + ptr + i + 2) = *(tmpPtr + i + 1);
						*(packetPtr + ptr + i + 3) = *(tmpPtr + i + 0);
					}

					ptr += 16;
					ctr -= 16;
				}

				if (ctr != 0) // If we have leftovers, we'd better eat - err, mask - them
				{
					for (var i = 0; i < ctr; ++i)
					{
						*(packetPtr + ptr) ^= _remainderMask[i];
						ptr++;
					}
				}				
			}
		}
	}
}
