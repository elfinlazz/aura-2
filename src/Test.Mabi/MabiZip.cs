// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Aura.Tests.Mabi
{
	public class MabiZipTests
	{
		[Fact]
		public void Compress()
		{
			var test = MabiZip.Compress("Copyright (c) Aura development team - Licensed under GNU GPL");

			Assert.Equal("122;789c1d8cbb0d80300c44df2829a16008444183100d03440902247e0a0912db73a4b0eff97c76c3c9c54b6065662162287094d29a24df8a3c138f6acbe95d74e4641459cd864ad5e987cbbb5bddcb49e2ff36885b7ac6ac8392f001358114a0", test);
		}

		[Fact]
		public void Decompress()
		{
			var test = MabiZip.Decompress("122;789c1d8cbb0d80300c44df2829a16008444183100d03440902247e0a0912db73a4b0eff97c76c3c9c54b6065662162287094d29a24df8a3c138f6acbe95d74e4641459cd864ad5e987cbbb5bddcb49e2ff36885b7ac6ac8392f001358114a0");

			Assert.Equal("Copyright (c) Aura development team - Licensed under GNU GPL", test);
		}
	}
}
