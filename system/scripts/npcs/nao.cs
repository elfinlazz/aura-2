//--- Aura Script -----------------------------------------------------------
// Nao
//--- Description -----------------------------------------------------------
// First NPC players encounter. Also met on every rebirth.
// Located on an unaccessible map, can be talked to from anywhere,
// given the permission.
//---------------------------------------------------------------------------

public class NaoScript : NpcScript
{
	public override void Load()
	{
		SetId(MabiId.Nao);
		SetName("_nao");
		SetRace(1);
		SetLocation(22, 6013, 5712);
	}

	protected override async Task Talk()
	{
		SetBgm("Nao_talk.mp3");

		await Intro(
			"A beautiful girl in a black dress with intricate patterns.",
			"Her deep azure eyes remind everyone of an endless blue sea full of mystique.",
			"With her pale skin and her distinctively sublime silhouette, she seems like she belongs in another world."
		);
		
		if(!Player.Has(CreatureStates.EverEnteredWorld))
			await FirstTime();
		else
			await Rebirth();
	}
	
	private async Task FirstTime()
	{
		await Introduction();
		await Questions();
		// Destiny/Talent...
		await EndIntroduction();
	}

	private async Task Introduction()
	{
		Msg("Hello, there... You are <username/>, right?<br/>I have been waiting for you.<br/>It's good to see a " + (Player.IsMale ? "gentleman" : "lady") + " like you here.");
		Msg("My name is Nao.<br/>It is my duty to lead pure souls like yours to Erinn.");
	}

	private async Task Questions()
	{
		Msg("<username/>, we have some time before I guide you to Erinn.<br/>Do you have any questions for me?", Button("No"), Button("Yes"));
		if (await Select() != "@yes")
			return;

		while (true)
		{
			Msg(RandomPhrase(),
				Button("End Conversation", "@endconv"),
				List("Talk to Nao", 4, "@endconv",
					Button("About Mabinogi", "@mabinogi"),
					Button("About Erinn", "@erinn"),
					Button("What to do?", "@what"),
					Button("About Adventures", "@adventures")
				)
			);

			switch (await Select())
			{
				case "@mabinogi":
					Msg("Mabinogi can be defined as the songs of bards, although in some cases, the bards themselves are referred to as Mabinogi.<br/>To the residents at Erinn, music is a big part of their lives and nothing brings joy to them quite like music and Mabinogi.<br/>Once you get there, I highly recommend joining them in composing songs and playing musical instruments.");
					break;

				case "@erinn":
					Msg("Erinn is the name of the place you will be going to, <username/>.<br/>The place commonly known as the world of Mabinogi is called Erinn.<br/>It has become so lively since outsiders such as yourself began to come.");
					Msg("Some time ago, adventurers discovered a land called Iria,<br/>and others even conquered Belvast Island, between the continents.<br/>Now, these places have become home to adventurers like yourself, <username/>.<p/>You can go to Tir Chonaill of Uladh now,<br/>but you should try catching a boat from Uladh and<br/>crossing the ocean to Iria or Belvast Island.");
					break;

				case "@what":
					Msg("That purely depends on what you wish to do.<br/>You are not obligated to do anything, <username/>.<br/>You set your own goals in life, and pursue them during your adventures in Erinn.<p/>Sure, it may be nice to be recognized as one of the best, be it the most powerful, most resourceful, etc., but <br/>I don't believe your goal in life should necessarily have to be becoming 'the best' at everything.<br/>Isn't happiness a much better goal to pursue?<p/>I think you should experience what Erinn has to offer <br/>before deciding what you really want to do there.");
					break;

				case "@adventures":
					Msg("There are so many things to do and adventures to go on in Erinn.<br/>Hunting and exploring dungeons in Uladh...<br/>Exploring the ruins of Iria...<br/>Learning the stories of the Fomors in Belvast...<p/>Explore all three regions to experience brand new adventures!<br/>Whatever you wish to do, <username/>, if you follow your heart,<br/>I know you will become a great adventurer before you know it!");
					break;

				default:
					return;
			}
		}
	}

	private async Task EndIntroduction()
	{
		Msg("Are you ready to take the next step?");
		Msg("You will be headed to Erinn right now.<br/>Don't worry, once you get there, someone else is there to take care of you, my little friend by the name of Tin.<br/>After you receive some pointers from Tin, head Northeast and you will see a town.");
		Msg("It's a small town called Tir Chonaill.<br/>I have already talked to Chef Duncan about you, so all you need to do is show him the letter of introduction I wrote right here.", Image("tir_chonaill"));
		Msg("You can find Chief Duncan on the east side of the Square.<br/>When you get there, try to find a sign that says 'Chief's House'.", Image("npc_duncan"));
		Msg("I will give you some bread I have personally baked, and a book with some information you may find useful.<br/>To see those items, open your inventory once you get to Erinn.");
		Msg(Hide.Both, "(Received a Bread and a Traveler's Guide from Nao.)", Image("novice_items"));
		Msg("I wish you the best of luck in Erinn.<br/>See you around.", Button("End Conversation"));
		await Select();

		// Move to Uladh Beginner Area
		Player.SetLocation(125, 21489, 76421);
		Player.Direction = 233;

		GiveItem(1000,  1); // Traveler's Guide
		GiveItem(50004, 1); // Bread

		Close();
	}

	private string RandomPhrase()
	{
		switch (Random(3))
		{
			default:
			case 0: return "If there is something you'd like to know more of, please ask me now.";
			case 1: return "Do not hesitate to ask questions. I am more than happy to answer them for you.";
			case 2: return "If you have any questions before heading off to Erinn, please feel free to ask.";
		}
	}
	
	private async Task Rebirth()
	{
		Msg("Hello, <username/>!<br/>Is life here in Erinn pleasant for you?");
		
		if(!RebirthAllowed())
		{
			Msg("Barely any time has passed since your last rebirth.<br/>Why don't you enjoy your current life in Erinn for a bit longer?");
			goto L_End;
		}
		
		Msg("If you wish, you can abandon your current body and be reborn into a new one, <username/>.");
		
		while(true)
		{
			Msg("Feel free to ask me any questions you have about rebirth.<br/>Once you've made up your mind to be reborn, press Rebirth.",
				Button("Rebirth"), Button("About Rebirths"), Button("Cancel"));
			
			switch(await Select())
			{
				case "@rebirth":
					Msg("<rebirth style='-1'/>");
					switch(await Select())
					{
						case "@rebirth":
							// Old:
							//   Msg("Would you like to be reborn with the currently selected features?<br/><button title='Yes' keyword='@rebirthyes' /><button title='No' keyword='@rebirthhelp' />");
							//   Msg("<username/>, you have been reborn with a new appearance.<br/>Did you enjoy having Close Combat as your active Talent?<br/>Would you like to choose a different active Talent for this life?<button title='New Talent' keyword='@yes' /><button title='Keep Old Talent' keyword='@no' />");
							//   Msg("Then I will show you the different Talents available to you.<br/>Please select your new active Talent after you consider everything.<talent_select />")
							//   Msg("You have selected Close Combat.<br/>May your courage and skill grow.<br/>I will be cheering you on from afar.");
							Close(Hide.None, "May your new appearance bring you happiness!<br/>Though you'll be different when next we meet,<br/>but I'll still be able to recognize you, <username/>.<p/>We will meet again, right?<br/>Until then, take care.");
							return;
					
						default:
							goto L_Cancel;
					}
					break;
					
				case "@about_rebirths":
					await RebirthAbout();
					break;
					
				default:
					goto L_Cancel;
			}
		}
		
	L_Cancel:
		Msg("There are plenty more opportunities to be reborn.<br/>Perhaps another time.<rebirth hide='true'/>");
		
	L_End:
		Close(Hide.None, "Until we meet again, then.<br/>I wish you the best of luck in Erinn.<br/>I'll see you around.");
	}
	
	private async Task RebirthAbout()
	{
		while(true)
		{
			Msg("When you rebirth, you will be able to have a new body.<br/>Aside from your looks, you can also change your age and starting location.<br/>Please feel free to ask me more.",
				Button("What is Rebirth?", "@whatis"), Button("What changes after a Rebirth?", "@whatchanges"), Button("What does not change after a Rebirth?", "@whatnot"), Button("Done"));
		
			switch(await Select())
			{
				case "@whatis":
					Msg("You can choose a new body between the age of 10 and 17.<br/>Know that you won't receive the extra 7 AP just for being 17,<br/>as you did at the beginning of your journey.<br/>You will keep the AP that you have right now.");
					Msg("Also, your Level and Exploration Level will reset to 1.<br/>You'll get to keep all of your skills from your previous life, though.");
					Msg("You'll have to<br/>start at a low level for the Exploration Quests,<br/>but I doubt that it will be an issue for you.");
					Msg("If you wish, you can even just change your appearance<br/>without resetting your levels or your age.<br/>Just don't select the 'Reset Levels and Age' button<br/>to remake yourself without losing your levels.", Image("Rebirth_01_c2", true, 200, 200));
					Msg("You can even change your gender<br/>by clicking on 'Change Gender and Look.'<br/>If you want to maintain your current look, then don't select that button.", Image("Rebirth_02_c2", true, 200, 200));
					Msg("You can choose where you would like to rebirth.<br/>Choose between Tir Chonaill, Qilla Base Camp,<br/>or the last location you were at<br/>in your current life.", Image("Rebirth_03", true, 200, 200));
					break;
				
				case "@whatchanges":
					Msg("You can choose a new body between the ages of 10 and 17.<br/>though you won't receive the extra 7 AP just for being 17<br/>as you did at the beginning of your journey.");
					Msg("You'll keep all the AP that you have right now<br/>and your level will reset to 1.<br/>You'll keep all of your skills from your previous life, though.");
					Msg("If you wish, you can even change your appearance without<br/>resetting your levels or your age.<br/>Just don't select the 'Reset Levels and Age' button,<br/>and you'll be able to remake yourself without losing your current levels.", Image("Rebirth_01", true));
					Msg("You can even change your gender by selecting 'Change Gender and Look.'<br/>If you want to keep your current look, just don't select that button.", Image("Rebirth_02", true));
					Msg("Lastly, if you would like to return to your last location,<br/>select 'Move to the Last Location'.<br/>Otherwise, you'll be relocated to the Forest of Souls<br/>near Tir Chonaill.");
					break;
				
				case "@whatnot":
					Msg("First of all, know that you cannot change the<br/>name you chose upon entering Erinn.<br/>Your name is how others know you<br/>even when all else changes.");
					Msg("<username/>, you can also bring all the knowledge you'd earned<br/>in this life into your next one.<br/>Skills, keywords, remaining AP, titles, and guild will all be carried over.<br/>The items you have and your banking information will also remain intact.");
					break;
				
				default:
					return;
			}
		}
	}
	
	private bool RebirthAllowed()
	{
		var player = (PlayerCreature)Player;
		return (player.LastRebirth + ChannelServer.Instance.Conf.World.RebirthTime < DateTime.Now);
	}
}
