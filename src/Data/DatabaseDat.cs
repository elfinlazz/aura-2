// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System.IO;
using System.IO.Compression;

namespace Aura.Data
{
	public abstract class DatabaseDatIndexed<TIndex, TInfo> : DatabaseIndexed<TIndex, TInfo> where TInfo : class, new()
	{
		public override int Load(string path, bool clear)
		{
			if (clear)
				this.Clear();

			var data = File.ReadAllBytes(path);

			using (var min = new MemoryStream(data))
			using (var mout = new MemoryStream())
			{
				using (var gzip = new GZipStream(min, CompressionMode.Decompress))
				{
					gzip.CopyTo(mout);
				}

				using (var br = new BinaryReader(mout))
				{
					br.BaseStream.Position = 0;
					while (br.BaseStream.Position < br.BaseStream.Length)
					{
						try
						{
							this.Read(br);
						}
						catch (DatabaseWarningException ex)
						{
							ex.Source = Path.GetFileName(path);
							this.Warnings.Add(ex);
							continue;
						}
					}
				}
			}

			return this.Count;
		}

		protected abstract void Read(BinaryReader br);
	}
}
