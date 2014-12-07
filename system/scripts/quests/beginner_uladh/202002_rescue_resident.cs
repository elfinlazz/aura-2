//--- Aura Script -----------------------------------------------------------
// Rescue Resident
//--- Description -----------------------------------------------------------
// Second quest in the Uladh beginner quest series, started automatically
// after talking to Duncan.
//--- Notes -----------------------------------------------------------------
// Since we don't have dungeons yet the clear Alby objective can be done by
// simply talking to Trefor again.
//---------------------------------------------------------------------------

public class RescueResidentQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202002);
		SetName("Rescue Resident");
		SetDescription("I'm Trefor, serving as a guard in the north part of the town, past the Healer's House. One of the residents of this town went to Alby Dungeon and has not come back yet. I'm worried about it, so I need you to help me search for the lost resident. - Trefor -");
		
		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202001));

		AddObjective("talk_trefor", "Talk with Trefor", 1, 8692, 52637, Talk("trefor"));
		AddObjective("kill_foxes", "Hunt 5 Young Brown Foxes", 1, 9100, 52000, Kill(5, "/brownfox/"));
		AddObjective("talk_trefor2", "Talk with Trefor", 1, 8692, 52637, Talk("trefor"));
		AddObjective("clear_alby", "Rescue a town resident from Alby Dungeon", 13, 3200, 3200, Talk("trefor"));
		
		AddReward(Exp(300));
		AddReward(Gold(1800));
		AddReward(AP(3));
		
		AddHook("_trefor", "after_intro", TalkTrefor);
	}
	
	public async Task<HookResult> TalkTrefor(NpcScript npc, params object[] args)
	{
		if(npc.QuestActive(this.Id, "talk_trefor"))
		{
			npc.FinishQuest(this.Id, "talk_trefor");
			
			npc.Player.Skills.Give(SkillId.Smash, SkillRank.Novice);
			
			npc.Msg("Welcome, I am Trefor, the guard.<br/>Someone from the town went into Alby Dungeon a while ago, but hasn't returned yet.<br/>I wish I could go there myself, but I can't leave my post. I'd really appreciate it if you can go and look for in Alby Dungeon.");
			npc.Msg("Since the dungeon is a dangerous place to be in, I'll teach you a skill that will help you in an emergency situation.<br/>It's called the Smash skill. If you use it, you can knock down a monster with a single blow!<br/>It is also highly effective when you sneak up on a target and deliver the blow without warning.");
			npc.Msg("Against monsters that are using the Defense skill,<br/>Smash will be the only way to penetrate that skill and deliver a killer blow.");
			npc.Msg("However... looking at the way you're holding your sword, I'm not sure if you are up to the task.<br/>Let me test your skills first. Do you see those brown foxes wandering in front of me?<br/>They're quite a nuisance, praying on those roosters in town.<br/>I want you to go and hunt 5 Young Brown Foxes right now.");
			npc.Msg("Foxes use the Defense Skill a lot, and as I told you before, regular attacks do not work against defending targets.<br/>That's then the Smash skill comes in handy.<br/><br/>Watch how I do it, and try picking up the important parts so you can use it too.<br/>You don't need to overstrain yourself by going for the Brown Foxes. Young Brown Foxes will do just fine.", npc.Movie("skillbar_guide_us.wmv", 500, 300), npc.Button("Continue"));
			await npc.Select();
			npc.Close2();

			var scene = new Cutscene("tuto_smash", npc.Player);
			scene.AddActor("me", npc.Player);
			scene.AddActor("#trefor", npc.NPC);
			scene.AddActor("#brownfox", 50001);
			scene.Play();
			
			return HookResult.End;
		}
		else if(npc.QuestActive(this.Id, "talk_trefor2"))
		{
			npc.FinishQuest(this.Id, "talk_trefor2");
			
			npc.Msg("Good, I see that you're getting the hang of it.<br/>Well, I was able to do that when I was 8, but whatever...<br/>It is now time for you to go and search for the missing Villager.");
			npc.Msg("Follow the road up and turn right and you'll find the Alby Dungeon.<br/>You can enter the dungeon by dropping this item on the altar.<br/>If you either lose it or fail to rescue her, come back to me so I can give you another one. Please be careful.", npc.Image("dungeonpass", 128, 128));
			
			npc.GiveItem(63140, 1);
			
			return HookResult.Break;
		}
		else if(npc.QuestActive(this.Id, "clear_alby"))
		{
			npc.FinishQuest(this.Id, "clear_alby");
			
			npc.Msg("You did it! Good job.<br/>Good thing I asked for your help.<br/>For your great work, I will now teach you how to properly use the Smash skill.<br/>If you open your Skill window and press the 'LEARN' button, you will be able to use a more powerful Smash skill.<br/>I can always use some help here, so drop by often, okay?");
			
			if(npc.Player.Skills.Is(SkillId.Smash, SkillRank.Novice))
				npc.Player.Skills.Train(SkillId.Smash, 1);
			
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
}
