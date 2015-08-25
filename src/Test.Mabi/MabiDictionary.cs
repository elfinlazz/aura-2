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
	public class MabiDictionaryTests
	{
		[Fact]
		public void Serialize()
		{
			var test = new MabiDictionary();
			test.SetBool("a bool", true);
			test.SetByte("a byte", 1);
			test.SetShort("a short", 2);
			test.SetInt("an int", 3);
			test.SetFloat("a float", 4);
			test.SetString("a string", "five, special chars :;");

			var test2 = test.ToString();

			Assert.Equal("a bool:b:1;a byte:1:1;a short:2:2;an int:4:3;a float:f:4;a string:s:five, special chars %C%S;", test2);
		}

		[Fact]
		public void Deserialize()
		{
			var test = new MabiDictionary();
			test.Parse("a bool:b:1;a byte:1:1;a short:2:2;an int:4:3;a float:f:4;a string:s:five, special chars %C%S;");

			Assert.Equal(true, test.GetBool("a bool"));
			Assert.Equal(1, test.GetByte("a byte"));
			Assert.Equal(2, test.GetShort("a short"));
			Assert.Equal(3, test.GetInt("an int"));
			Assert.Equal(4, test.GetFloat("a float"));
			Assert.Equal("five, special chars :;", test.GetString("a string"));
		}
	}
}
