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
			"A beautiful girl stands before you in an elegant black dress.",
			"Her deep azure eyes remind everyone of an endless blue sea full of mystique.",
			"Her pale skin and flowing white hair appear almost otherworldly."
		);

		await Introduction();
		await Questions();
		// Destiny/Talent...
		await End();
	}

	private async Task Introduction()
	{
		Msg("Hello, there. You are <username/>, right?<br/>I have been waiting for you.");
		Msg("My name is Nao.<br/>It is my duty to lead pure souls<br/>like yours to the land of Erinn.");
	}

	private async Task Questions()
	{
		Msg("You must be disoriented from your trip through<br/>the soul stream. Is there anything I can help you understand?", Button("No"), Button("Yes"));
		if (await Select() != "@yes")
			return;

		while (true)
		{
			Msg(RandomPhrase(),
				Button("End Conversation", "@endconv"),
				List("Talk to Nao", 4, "@endconv",
					Button("Where am I?", "@where"),
					Button("What is there to do in Erinn?", "@erinn"),
                    Button("What kind of adventures will I have?", "@adventures"),
					Button("Who are you?", "@who")
				)
			);

			switch (await Select())
			{
				case "@where":
                    //NOTE: In the official version there is a double quote around Milletians. However, escaping this with a backslash (e.g. \"Milletians\") caused the client to crash... How can we properly escape double quotes?
					Msg("You're in the Soul Stream. Whenever a soul from another world<br/>wishes to enter Erinn, they must pass through here first.<p/>We call those who come from other worlds 'Milletians'. Soon,<br/>Milletian, I will send you to Erinn to begin your adventures.");
					break;

				case "@erinn":
					Msg("In Erinn, you can do whatever you wish.<br/>You can battle giant dragons, explore dungeons,<br/>grow strawberries, even participate in fashion shows...<p/>You can fish, fight wyverns aboard hot-air balloons,<br/>joust, visit hot springs, search for ancient relics,<br/>and so much more...<p/>It makes me happy just thinking about all the<br/>different things you'll discover in Erinn.");
                    Msg("Some time ago, adventurers discovered a land called Iria,<br/>and others even conquered Belvast Island, between the continents.<br/>Now, these places have become home to adventurers like yourself, <username/>.<p/>You can go to Tir Chonaill of Uladh now,<br/>but you should try catching a boat from Uladh and<br/>crossing the ocean to Iria or Belvast Island.");
					break;

				case "@adventures":
					Msg("That depends entirely on you, <username/>.<p/>You could create an idyllic life... fishing, crafting,<br/>farming, and sitting in the town square playing music<br/>you've composed or selling goods you've crafted.<p/>You could charge head-first into danger, helping<br/>residents around Erinn as they struggle against<br/>dangers much bigger than themselves.<p/>In between, you could practice in limited time<br/>events that celebrate holidays from your world<br/>and other special occassions.<p/>But it's really up to you. You have the freedom to<br/>pursue anything that interests you!");
					break;

				case "@who":
					Msg("M-me? My name is Nao Mariota Pryderi. It's my duty<br/>to guide souls like yours safely to Erinn. There was once a time when...<p/>(Nao smiles.)<br/>No, never mind. Forget I said anything. It wasn't<br/>important anyway. Was there anything else you<br/>wanted to ask?");
					break;

				default:
					return;
			}
		}
	}

	private async Task End()
	{
        //Talent selection isn't implemented yet... Use old dialogue
        //Msg("(Nao smiles gently.)<br/>You're ready to journey into the world, <username/>.<p/>Before you start your adventure, you must choose a Talent.<br/>Don't worry, you can change your talent as many times as<br/>you want, without losing any of the skills or stats you've earned.");
        //SELECT TALENT
        //Msg("You've selected <talent> as your active Talent.");
        //Msg("Take the time to test out your new skills when you reach Erinn.<br/>There are many Talents to choose from, if you don't like the one<br/>you've chosen. You can try a new Talent once you learn how to Rebirth.<p/>Look for Chief Duncan when you arrive in Tir Chonaill. I've also gotten you<br/>some supplies to help you on your way.");
        //Msg(Hide.Both, "(Nao hands you Bread, a Traveler's Guide, and some Soul Stones.)", Image("novice_item_g9korea"));
        //Msg("Use the Traveler's Guide if you ever find yourself lost or confused.<br/>And good luck on your adventure. The world is waiting for you<br/>to make your mark.", Button("End Conversation"));

		
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
}
