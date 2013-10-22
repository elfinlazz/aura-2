// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Aura.Shared.Network
{
	public enum PacketElementType : byte
	{
		None = 0,
		Byte = 1,
		Short = 2,
		Int = 3,
		Long = 4,
		Float = 5,
		String = 6,
		Bin = 7,
	}

	public class MabiPacket
	{
		private const int DefaultSize = 8192;
		private const int AddSize = 512;

		protected byte[] _buffer;
		protected int _ptr;
		protected bool _built;
		protected int _bodyStart;

		private int _elements, _bodyLen;

		public int Op { get; set; }
		public long Id { get; set; }

		public MabiPacket(int op, long id)
		{
			this.Op = op;
			this.Id = id;

			_buffer = new byte[DefaultSize];
		}

		public MabiPacket(byte[] buffer)
		{
			_built = true;
			_buffer = buffer;
			_ptr = 6;

			var length = buffer.Length;

			this.Op = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_buffer, _ptr));
			this.Id = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(_buffer, _ptr + sizeof(int)));
			_ptr += 12;

			while (_ptr < length)
			{ if (_buffer[++_ptr - 1] == 0) break; }

			_bodyStart = _ptr;
		}

		public PacketElementType Peek()
		{
			if (_ptr + 2 > _buffer.Length)
				return 0;
			return (PacketElementType)_buffer[_ptr];
		}

		// Write
		// ------------------------------------------------------------------

		protected MabiPacket PutSimple(PacketElementType type, params byte[] val)
		{
			this.EnsureEditable();

			var len = 1 + val.Length;
			this.EnsureSize(len);

			_buffer[_ptr++] = (byte)type;
			Buffer.BlockCopy(val, 0, _buffer, _ptr, val.Length);
			_ptr += val.Length;

			_elements++;
			_bodyLen += len;

			return this;
		}

		protected MabiPacket PutWithLength(PacketElementType type, byte[] val)
		{
			this.EnsureEditable();

			var len = 1 + sizeof(short) + val.Length;
			this.EnsureSize(len);

			_buffer[_ptr++] = (byte)type;
			Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)val.Length)), 0, _buffer, _ptr, sizeof(short));
			_ptr += 2;
			Buffer.BlockCopy(val, 0, _buffer, _ptr, val.Length);
			_ptr += val.Length;

			_elements++;
			_bodyLen += len;

			return this;
		}

		public MabiPacket PutByte(byte val) { return this.PutSimple(PacketElementType.Byte, val); }
		public MabiPacket PutByte(bool val) { return this.PutByte(val ? (byte)1 : (byte)0); }
		public MabiPacket PutShort(short val) { return this.PutSimple(PacketElementType.Short, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val))); }
		public MabiPacket PutInt(int val) { return this.PutSimple(PacketElementType.Int, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val))); }
		public MabiPacket PutLong(long val) { return this.PutSimple(PacketElementType.Long, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(val))); }
		public MabiPacket PutLong(DateTime val) { return this.PutLong((long)(val.Ticks / 10000)); }
		public MabiPacket PutFloat(float val) { return this.PutSimple(PacketElementType.Float, BitConverter.GetBytes(val)); }
		public MabiPacket PutFloat(double val) { return this.PutFloat((float)val); }

		public MabiPacket PutString(string val) { return this.PutWithLength(PacketElementType.String, Encoding.UTF8.GetBytes(val + "\0")); }
		public MabiPacket PutString(string format, params object[] args) { return this.PutString(string.Format((format != null ? format : string.Empty), args)); }

		public MabiPacket PutBin(byte[] val) { return this.PutWithLength(PacketElementType.Bin, val); }
		public MabiPacket PutBin(object val)
		{
			var type = val.GetType();
			if (!type.IsValueType || type.IsPrimitive)
				throw new Exception("PutBin only takes byte[] and structs.");

			var size = Marshal.SizeOf(val);
			var arr = new byte[size];
			var ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(val, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);

			return this.PutBin(arr);
		}

		protected void EnsureSize(int required)
		{
			if (_ptr + required >= _buffer.Length)
				Array.Resize(ref _buffer, _buffer.Length + AddSize);
		}

		protected void EnsureEditable()
		{
			if (_built)
				throw new Exception("Packet can't be modified once it was built.");
		}

		// Read
		// ------------------------------------------------------------------

		public byte GetByte()
		{
			if (this.Peek() != PacketElementType.Byte)
				throw new Exception("Expected Byte, got " + this.Peek() + ".");

			_ptr += 1;
			return _buffer[_ptr++];
		}
		public bool GetBool() { return (this.GetByte() != 0); }

		public short GetShort()
		{
			if (this.Peek() != PacketElementType.Short)
				throw new Exception("Expected Short, got " + this.Peek() + ".");

			_ptr += 1;
			var val = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(_buffer, _ptr));
			_ptr += sizeof(short);

			return val;
		}

		public int GetInt()
		{
			if (this.Peek() != PacketElementType.Int)
				throw new Exception("Expected Int, got " + this.Peek() + ".");

			_ptr += 1;
			var val = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(_buffer, _ptr));
			_ptr += sizeof(int);

			return val;
		}

		public long GetLong()
		{
			if (this.Peek() != PacketElementType.Long)
				throw new Exception("Expected Long, got " + this.Peek() + ".");

			_ptr += 1;
			var val = IPAddress.HostToNetworkOrder(BitConverter.ToInt64(_buffer, _ptr));
			_ptr += sizeof(long);

			return val;
		}

		public float GetFloat()
		{
			if (this.Peek() != PacketElementType.Float)
				throw new Exception("Expected Float, got " + this.Peek() + ".");

			_ptr += 1;
			var val = BitConverter.ToSingle(_buffer, _ptr);
			_ptr += 4;

			return val;
		}

		public string GetString()
		{
			if (this.Peek() != PacketElementType.String)
				throw new ArgumentException("Expected String, got " + this.Peek() + ".");

			_ptr += 1;
			var len = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(_buffer, _ptr));
			_ptr += 2;

			var val = Encoding.UTF8.GetString(_buffer, _ptr, len - 1);
			_ptr += len;

			return val;
		}

		public byte[] GetBin()
		{
			if (this.Peek() != PacketElementType.Bin)
				throw new ArgumentException("Expected Bin, got " + this.Peek() + ".");

			_ptr += 1;
			var len = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(_buffer, _ptr));
			_ptr += 2;

			var val = new byte[len];
			Buffer.BlockCopy(_buffer, _ptr, val, 0, len);
			_ptr += len;

			return val;
		}

		// ------------------------------------------------------------------

		public byte[] Build(bool includeProtocolHeader = true)
		{
			if (_built)
				return _buffer;

			var ptr = includeProtocolHeader ? 6 : 0;
			var result = new byte[ptr + 30 + _bodyLen]; // protocol+header+body
			var length = _bodyLen;

			// Packet header
			{
				// Op + Id
				Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.Op)), 0, result, ptr, sizeof(int));
				Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.Id)), 0, result, ptr + sizeof(int), sizeof(long));
				ptr += 12;

				// Body len
				int n = _bodyLen;
				do
				{
					result[ptr++] = (byte)(n > 0x7F ? (0x80 | (n & 0xFF)) : n & 0xFF);
					n >>= 7;
				}
				while (n != 0);

				// Element amount
				n = _elements;
				do
				{
					result[ptr++] = (byte)(n > 0x7F ? (0x80 | (n & 0xFF)) : n & 0xFF);
					n >>= 7;
				}
				while (n != 0);

				result[ptr++] = 0;

				length += ptr;
			}

			// Body
			_bodyStart = ptr;
			Buffer.BlockCopy(_buffer, 0, result, ptr, _bodyLen);

			// Append dummy space for checksum... not really needed, is it?
			length += 4;

			// Protocol header
			if (includeProtocolHeader)
			{
				result[0] = 0x88;
				Buffer.BlockCopy(BitConverter.GetBytes(length), 0, result, 1, sizeof(int));
			}

			// Cut off remaining bytes
			Array.Resize(ref result, length);

			_buffer = result;
			_built = true;

			return result;
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			var prevPtr = _ptr;
			_ptr = _bodyStart;

			result.AppendFormat("Op: {0:X08}, Id: {1:X016}" + Environment.NewLine, this.Op, this.Id);

			PacketElementType type;
			for (int i = 1; ((type = this.Peek()) != PacketElementType.None && _ptr < _buffer.Length); ++i)
			{
				if (type == PacketElementType.Byte)
				{
					var data = this.GetByte();
					result.AppendFormat("{0:000} [{1}] Byte   : {2}", i, data.ToString("X2").PadLeft(16, '.'), data);
				}
				else if (type == PacketElementType.Short)
				{
					var data = this.GetShort();
					result.AppendFormat("{0:000} [{1}] Short  : {2}", i, data.ToString("X4").PadLeft(16, '.'), data);
				}
				else if (type == PacketElementType.Int)
				{
					var data = this.GetInt();
					result.AppendFormat("{0:000} [{1}] Int    : {2}", i, data.ToString("X8").PadLeft(16, '.'), data);
				}
				else if (type == PacketElementType.Long)
				{
					var data = this.GetLong();
					result.AppendFormat("{0:000} [{1}] Long   : {2}", i, data.ToString("X8").PadLeft(16, '.'), data);
				}
				else if (type == PacketElementType.Float)
				{
					var data = this.GetFloat();
					result.AppendFormat("{0:000} [{1}] Float  : {2}", i, (BitConverter.DoubleToInt64Bits(data) >> 32).ToString("X8").PadLeft(16, '.'), data.ToString("0.0####", CultureInfo.InvariantCulture));
				}
				else if (type == PacketElementType.String)
				{
					var data = this.GetString();
					result.AppendFormat("{0:000} [................] String : {1}", i, data);
				}
				else if (type == PacketElementType.Bin)
				{
					var data = BitConverter.ToString(this.GetBin());
					var splitted = data.Split('-');

					result.AppendFormat("{0:000} [................] Bin    : ", i);
					for (var j = 1; j <= splitted.Length; ++j)
					{
						result.Append(splitted[j - 1]);
						if (j < splitted.Length)
							if (j % 16 == 0)
								result.Append(Environment.NewLine.PadRight(33, ' '));
							else
								result.Append(' ');
					}
				}

				result.AppendLine();
			}

			_ptr = prevPtr;

			return result.ToString();
		}
	}
}
