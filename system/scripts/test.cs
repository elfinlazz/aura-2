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
	L_Keywords:
		ShowKeywords(c);
		var r = Select(c);

		switch(r)
		{
			default:
				Msg(c, "Hm... no idea. How about a talk about breasts?");
				c.Keywords.Give("breast");
				goto L_Keywords;
			case "rumor":
				Msg(c, "Titles anyone?");
				if(!c.Titles.Knows(8))
					c.Titles.Show(8);
				else
					c.Titles.Enable(8);
				break;
			case "breast":
				Msg(c, "=DD");
				break;
		}
		
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

public class PropSpawnTest : BaseScript
{
	public override void Load()
	{
		SpawnProp(1, 1, 12000, 38000, 0, PropDrop(2));
	}
}
