// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Network.Crypto
{
	/// <summary>
	/// Implementation of Mabi's custom cipher
	/// </summary>
	public sealed class MabiCipherEngine
	{
		private int _idx;

		private readonly byte[] _keystream;
		private readonly MabiKeystreamGenerator _keyGen;

		public MabiCipherEngine(MabiKeystreamGenerator keyGen)
		{
			_keyGen = keyGen;
			_keystream = new byte[128];

			for (var i = 31; i >= 0; i--)
			{
				BitConverter.GetBytes(_keyGen.GetNextKey()).CopyTo(_keystream, i * 4);
			}

			_idx = 31;
		}

		public void ProcessPacket(byte[] packet, int offset, int len)
		{
			len -= 4;
			int i;

			var tmpIdx = _idx * 4; // Convert idx into a byte index

			for (i = offset; i < len; i += 4)
			{
				var j = tmpIdx & 0x1F; // Convert tmpIdx into a value [0, 32)
				// this is, in effect, the first 32 bytes of the keystream

				tmpIdx += 4; // Advance the counter by 4

				packet[i] ^= _keystream[j];
				packet[i + 1] ^= _keystream[j + 1];
				packet[i + 2] ^= _keystream[j + 2];
				packet[i + 3] ^= _keystream[j + 3];
			}

			len += 4;

			// Process the final block, which may not be a multiple of 4
			while (i < len)
			{
				packet[i] ^= _keystream[tmpIdx & 0x7F]; // Convert the counter into a value [0, 128)
				// this is, in effect, anywhere along the key
				i++;
				tmpIdx++;
			}

			// Replace a subset of the key. Idx will always be on the range [0, 32)
			// So this effectively changes 4 bytes of the "Main" key and doesn't touch the remainder
			BitConverter.GetBytes(_keyGen.GetNextKey()).CopyTo(_keystream, _idx);

			if (_idx == 0)
				_idx = 31;
			else
				_idx--;
		}
	}
}
