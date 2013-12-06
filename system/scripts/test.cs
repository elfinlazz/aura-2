using System.Collections;
using Aura.Channel.Network;
using Aura.Channel.Scripting;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

public class TestScript : NpcScript
{
	public override void Load()
	{
		SetName("_nao");
		SetLocation(1, 12750, 38219, 0);
	}

	public override IEnumerable Talk(Creature c)
	{
		Msg(c, "test");
		ShowKeywords(c);
		var r = Select(c);

		Msg(c, "Response: " + r, Bgm("npc_owen.mp3"));

		Call(Test(c));

		Msg(c, "test end");
		Return();
	}

	public IEnumerable Test(Creature c)
	{
		Msg(c, "test from test");
		Return();
	}
}
