//--- Aura Script -----------------------------------------------------------
// Spirit Sword (F) 
//--- Description -----------------------------------------------------------
// Female sword ego
//---------------------------------------------------------------------------

public class SpiritSwordFScript : NpcScript
{
	public override void Load()
	{
		SetName("_ego_female_sword");
		SetRace(1);
		SetLocation(22, 5800, 7100, 0);
	}

	protected override async Task Talk()
	{
		while(true)
		{
			Msg("How are you doing?", Button("Talk", "@talk"), Button("Give Item", "@feed_item"), Button("Repair", "@repair"), Button("Finish Conversation", "@endconvo"));
			var reply = await Select();
			
			if(reply == "@endconvo")
				break;
			
			if(reply == "@talk")
			{
				Msg(Expression("normal"), "...Alright, what do you want to know?");
				await Conversation();
				break;
			}
			else
			{
				Msg("(Unimplemented)");
			}
		}
		
		switch(Random(2))
		{
			case 0: Msg(Expression("good"), "See you another time."); break;
			case 1: Msg(Expression("good"), "Then I'll see you later."); break;
		}
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			default:
				// Msg("<face name='sulky4'/>Well... I don't know much about that");
				// Msg("<face name='bad1'/>*sigh*...<br/>That's a depressing topic.");
				// Msg("<face name='despair'/>Aren't you tired of asking these kinds of questions?");
				// Msg("<face name='bad2'/>...You are very good at avoiding tough questions.");
				// Msg("<face name='normal'/>Hey, <username/>! Why do you want to ask me that right now?<br/>What a stupid Human! *tsk*");
				// Msg("<face name='despair'/>Huh? Why are you interested in that?");
				// Msg("<face name='sulky2'/>*tsk*. You don't seem to take my feelings into consideration<br/>when you are talking to me...");
				// Msg("<face name='despair'/>Let's talk about something else.<br/>That was boring.");
				// Msg("<face name='bad2'/>I'm furious!");
				// Msg("<face name='normal'/>Hey, <username/>! Why do you want to ask me that right now?<br/>What a stupid Human! *tsk*");
				
				Msg(Expression("despair"), "Huh? Why are you interested in that?");
				
				break;
		}
	}
}
