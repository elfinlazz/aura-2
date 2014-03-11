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

		await Introduction();
		await Questions();
		// Destiny/Talent...
		await End();
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

	private async Task End()
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

		GiveItem(1000, 1); // Traveler's Guide
		GiveItem(50004, 1); // Bread
		GiveItem(85539, 3); // Nao's Soul Stone for Beginners

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
