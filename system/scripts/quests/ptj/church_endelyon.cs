//--- Aura Script -----------------------------------------------------------
// Endelyon's Church Part-Time Job
//--- Description -----------------------------------------------------------
// Handles Endelyon's PTJs via hooks.
//---------------------------------------------------------------------------

public class EndelyonPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Church;
	
	const int Start = 12;
	const int Report = 16;
	const int Deadline = 21;
	const int PerDay = 20;
	
	int remaining = PerDay;
	
	readonly int[] QuestIds = new int[]
	{
		502102, // Basic - 10 Barley
		502103, // Basic - 10 Wheat
		502105, // Basic - 15 Eggs
		502132, // Int - 15 Barley
		502133, // Int - 15 Wheat
		502135, // Int - 20 Eggs
		502162, // Adv - 20 Barley
		502163, // Adv - 20 Wheat
		502165, // Adv - 30 Eggs
	};

	public override void Load()
	{
		AddHook("_endelyon", "after_intro", AfterIntro);
		AddHook("_endelyon", "before_keywords", BeforeKeywords);
	}
	
	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if(npc.DoingPtjForNpc() && npc.ErinnHour(Report, Deadline))
		{
			await AboutArbeit(npc);
			return HookResult.Break;
		}
		
		return HookResult.Continue;
	}
	
	[On("ErinnMidnightTick")]
	private void OnErinnMidnightTick(ErinnTime time)
	{
		remaining = PerDay;
	}
	
	public async Task<HookResult> BeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		
		if(keyword == "about_arbeit")
		{
			await AboutArbeit(npc);
			await npc.Conversation();
			npc.End();
			
			return HookResult.End;
		}
		
		return HookResult.Continue;
	}
	
	public async Task AboutArbeit(NpcScript npc)
	{
		if(npc.DoingPtjForOtherNpc())
		{
			npc.Msg("You have other things to do, right?<br/>If you need the Holy Water of Lymilark, can you come back after you are finished with your work?");
			return;
		}
		
		if(npc.DoingPtjForNpc())
		{
			var result = npc.GetPtjResult();
			
			if(!npc.ErinnHour(Report, Deadline))
			{
				if(result == QuestResult.Perfect)
					npc.Msg("It seems you took care of your end of the bargain.<br/>I'm a little busy right now, but come back later so I can compensate you for your work.");
				else
					npc.Msg("How are you doing with the part-time job for the Church today?<br/>I have the utmost faith in you, <username/>.");
				return;
			}
			
			npc.Msg("Did you finish the part-time job I gave you?<br/>If you are done, you can report the results to me. Do you want to do so now?<button title='Report Now' keyword='@report' /><button title='Report Later' keyword='@later' />");
			
			if(await npc.Select() != "@report")
			{
				npc.Msg("You don't want to report yet?<br/>Please make sure to come back and report before the deadline.");
				return;
			}
			
			if(result == QuestResult.None)
			{
				npc.GiveUpPtj();
				
				npc.Msg(npc.FavorExpression(), "I'm sorry,<br/>but I cannot give you the Holy Water of Lymilark unless you complete the task I've asked you to take care of.<br/>Please work harder next time.");
				npc.ModifyRelation(0, -Random(3), 0);
			}
			else
			{
				npc.Msg("Well done, <username/>. I feel very relieved thanks to you.<br/>In appreciation of all the hard work you've put in for the Church,<br/>I prepared some things for you.<br/>I'd love to give you all these if I could, but I can't. Please pick one.<button title='Report Later' keyword='@later' />" + npc.GetPtjReportXml(result));
				var reply = await npc.Select();
				
				if(!reply.StartsWith("@reward:"))
				{
					npc.Msg("You don't want to report yet?<br/>Please make sure to come back and report before the deadline.");
					return;
				}
				
				npc.CompletePtj(reply);
				remaining--;
				
				if(result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), "Thanks. You took care of everything I've asked for.<br/>As promised, I will give you the Holy Water of Lymilark.");
					npc.ModifyRelation(0, Random(3), 0); 
				}
				else if(result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), "It's a bit short of what I asked for,<br/>but I appreciate your help.<br/>I will give you the Holy Water of Lymilark in return.");
					npc.ModifyRelation(0, Random(1), 0); 
				}
				else if(result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), "Did you run out of time?<br/>You completed only a portion of what I asked for.<br/>I'm sorry, but that's not good enough for me to give you the Holy Water of Lymilark.");
					npc.ModifyRelation(0, -Random(2), 0); 
				}
			}
			return;
		}
		
		if(!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg("Are you willing to help the Church?<br/>It's a bit early, though. Please come back at a later time.");
			return;
		}
		
		if(!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg("Today's part-time jobs are all taken.<br/>If you need some Holy Water of Lymilark, please come back tomorrow.");
			return;
		}
		
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var ptjXml = npc.GetPtjXml(randomPtj, "Endelyon's Church Part-Time Job", "Looking for help with delivering goods to Church.", PerDay, remaining);
		var msg = "";
		
		if(npc.GetPtjDoneCount(JobType) == 0)
			msg = "Our Church is looking for a kind soul to help take care of our crops.<br/>The main job is to harvest wheat or barley from the farmland located south of the Church.<br/>One thing to note: because of our tight budget, we cannot afford to pay in gold.<p/>Instead, anyone who completes the job will receive some Holy Water of Lymilark,<br/>which can be used to bless items.<br/>Blessed items do not fall to the ground<br/>when its owner is knocked unconscious.<br/>Now, what do you say?";
		else
			msg = "Are you here for the Holy Water of Lymilark again?<br/>Please take a look at today's part-time job and tell me if you want it.";
		
		npc.Msg(msg + ptjXml);
		
		if(await npc.Select() == "@accept")
		{
			npc.Msg("Thank you.<br/>Please take care of this on time.");
			npc.StartPtj(randomPtj);
		}
		else
		{
			npc.Msg("If you don't want to do it, then I guess that's that.");
		}
	}
}
