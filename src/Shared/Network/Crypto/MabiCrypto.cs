// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Network.Crypto
{
	public sealed class MabiCrypto
	{
		public bool ForServer { get; private set; }
		public uint Seed { get; private set; }

		private readonly MabiCipherEngine _mabiCipher;
		private readonly MabiAesEngine _aesEngine;

		private static readonly Random _rnd = new Random();

		/// <summary>
		/// Initializes a new instance of Crypto with a random seed
		/// </summary>
		/// <param name="forServer">True if this instance will be responsible for packets
		/// sent by the server</param>
		public MabiCrypto(bool forServer)
			: this(GetRandomSeed(), forServer)
		{

		}

		/// <summary>
		/// Initializes a new instance of Crypto with the specified seed
		/// </summary>
		/// <param name="forServer">True if this instance will be responsible for packets
		/// sent by the server</param>
		public MabiCrypto(uint seed, bool forServer)
		{
			this.Seed = seed;
			this.ForServer = forServer;

			var keyGen = new MabiKeystreamGenerator(seed);
			_aesEngine = new MabiAesEngine(forServer, keyGen.AesKey);
			_mabiCipher = new MabiCipherEngine(keyGen);
		}

		/// <summary>
		/// Returns a random seed
		/// </summary>
		/// <returns></returns>
		private static uint GetRandomSeed()
		{
			return (uint)_rnd.Next(int.MinValue, int.MaxValue);
		}

		/// <summary>
		/// Applies the appropriate transformation to a packet that travels
		/// from the server to client
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public void FromServer(byte[] packet, int offset, int count)
		{
			_aesEngine.ProcessPacket(packet, offset, count);
		}

		/// <summary>
		/// Applies the appropriate transformation to a packet that travels
		/// from the server to client
		/// 
		/// This version uses the defaults for a standard Mabi packet
		/// </summary>
		/// <param name="packet"></param>
		public void FromServer(byte[] packet)
		{
			FromServer(packet, 6, packet.Length-6);
		}

		/// <summary>
		/// Applies the appropriate transformation to a packet that travels
		/// from the client to server.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public void FromClient(byte[] packet, int offset, int count)
		{
			_mabiCipher.ProcessPacket(packet, offset, count);
		}

		/// <summary>
		/// Applies the appropriate transformation to a packet that travels
		/// from the client to server.
		/// 
		/// This version uses the defaults for a standard Mabi packet
		/// </summary>
		/// <param name="packet"></param>
		public void FromClient(byte[] packet)
		{
			FromClient(packet, 6, packet.Length);
		}
	}
}
