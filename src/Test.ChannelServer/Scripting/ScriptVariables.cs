using Aura.Channel.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aura.Tests.Channel.Scripting
{
	public class ScriptVariables
	{
		[Fact]
		public void GettingAndSettingFields()
		{
			dynamic mgr = new VariableManager();
			mgr.Test1 = 1;
			mgr.Test2 = "two";
			mgr.Test3 = mgr.Test1 + 2;
			mgr.Test4 = mgr.Test2 + 2;

			Assert.Equal(1, mgr.Test1);
			Assert.Equal("two", mgr.Test2);
			Assert.Equal(3, mgr.Test3);
			Assert.Equal("two2", mgr.Test4);
		}

		[Fact]
		public void GettingAndSettingIndex()
		{
			var mgr = new VariableManager();
			mgr["Test1"] = 1;
			mgr["Test2"] = "two";
			mgr["Test3"] = mgr["Test1"] + 2;
			mgr["Test4"] = mgr["Test2"] + 2;

			Assert.Equal(1, mgr["Test1"]);
			Assert.Equal("two", mgr["Test2"]);
			Assert.Equal(3, mgr["Test3"]);
			Assert.Equal("two2", mgr["Test4"]);
		}

		[Fact]
		public void InitializingVariables()
		{
			dynamic mgr = new VariableManager();
			mgr.Test1 = 1;
			mgr.Test2 = "two";
			mgr.Test3 = mgr.Test1 + 2;
			mgr.Test4 = mgr.Test2 + 2;

			var mgr2 = new VariableManager(mgr.GetList());

			Assert.Equal(1, mgr["Test1"]);
			Assert.Equal("two", mgr["Test2"]);
			Assert.Equal(3, mgr["Test3"]);
			Assert.Equal("two2", mgr["Test4"]);
		}
	}
}
