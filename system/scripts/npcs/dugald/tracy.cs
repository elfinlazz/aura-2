//--- Aura Script -----------------------------------------------------------
// Tracy in Ulaid Logging Camp
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class TracyScript : NpcScript
{
	public override void Load()
	{
		SetName("_tracy");
		SetRace(10002);
		SetBody(height: 1.2f, weight: 1.5f, upper: 2f);
		SetFace(skinColor: 19, eyeType: 9, eyeColor: 27);
		SetLocation(16, 22900, 59500, 56);

		EquipItem(Pocket.Face, 4904, 0x004B4C64, 0x007C1B83, 0x00CE8970);
		EquipItem(Pocket.Hair, 4025, 0x00754C2A, 0x00754C2A, 0x00754C2A);
		EquipItem(Pocket.Armor, 15005, 0x00744D3C, 0x00DDB372, 0x00D6BDA3);
		EquipItem(Pocket.Glove, 16010, 0x00755744, 0x00005B40, 0x009E086C);
		EquipItem(Pocket.Shoe, 17010, 0x00371E00, 0x00000047, 0x00747374);
		EquipItem(Pocket.Head, 18017, 0x00744D3C, 0x00F79622, 0x00BE7781);
		EquipItem(Pocket.RightHand1, 40007, 0x00A7A894, 0x00625F44, 0x00872F92);

		AddPhrase("Gee, it's hot.");
		AddPhrase("I tire out so easily these days...");
		AddPhrase("It's so dull here...");
		AddPhrase("Man, I'm so sweaty...");
		AddPhrase("Oh, my arm...");
		AddPhrase("Oh, my leg...");
		AddPhrase("Oww, my muscles are sore all over.");
		AddPhrase("Phew. Alright. Time to rest!");
		AddPhrase("Should I take a break now?");
		AddPhrase("*Yawn*");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Tracy.mp3");
	
		await Intro(
			"This broad-shouldered man holding a wood-cutting axe in his right hand must have gone through a lot of rough times.",
			"He's wearing a cap backwards and his bronzed face is covered with a heavy beard.",
			"Between the wavy strands of his bushy dark brown hair are a pair of bright, playful eyes full of benevolent mischief."
		);
		
		Msg("What is it? Come on, spit it out!<br/>I'm a busy man!", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("What was your name?<br/>I think... you were the one snickering at my face before...");
				await StartConversation();
				break;
				
			case "@shop":
				Msg("You need something?");
				OpenShop("TracyShop");
				return;
				
			case "@upgrade":
				Msg("Somebody told you that modified items are good, right?<br/>Well, if <username/> needs a favor, I guess I must help.<br/>Show me what you want to modify.");
				Msg("(Unimplemented)");
				// @end: Msg("Just ask me if you want something modified, man! Anytime, haha!");
				break;
		}
		
		End("Goodbye, <npcname/>. I'll see you later!");
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if(Memory == 1)
				{
					Msg("Hey, hey. You're thinking about my name again?<br/>I don't like it myself.");
					Msg("Stop grinning. Don't give me that look any more. It's really disturbing.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "Yes, Tracy is my name. The lumberjack...<br/>Hey! Why are you giggling while you ask?");
					Msg("...Ha! <username/>... Your name sounds no better than mine.<br/>Actually, yours is even worse!");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				Msg("Looks like the people who talked to me before<br/>don't really like my speaking style...<br/>Let me say something. These people just show up and start pestering me with all these silly questions,<br/>and I'm supposed to be polite to them all the time?");
				Msg("What do you think? What?<br/>You don't like how I speak either?");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				// Learn skill Carpentry Mastery
				// Start quest 20113
				Msg("Interested in Carpentry? It's a skill that lets you<br/>make lumber and bows.<br/>Buy a Lumber Axe, chop a piece of Average Firewood<br/>using that chopping block over there, and talk to me.<br/>I'll teach the secrets to carpentry then.");
				break;

			case "about_arbeit":
				Msg("You want a logging job?<br/>This is not the right time. Come back later.<br/>When the shadow's in the northwest... I think 7 o'clock in the morning will do.");
				//Msg("Good. You want a logging job?<br/>I was actually a little bored working alone.<br/>I can use some help. If you're good enough, I can pay you more.");
				//Msg("Want to give it a try?");
				break;

			case "shop_misc":
				Msg("Haha... Looking for a general shop in a place like this...<br/>What is it? An emergency?");
				Msg("I guess you need to go to a town to find one.<br/>That gentlemanly Malcolm's to the north,<br/>and the General Shop in Dunbarton is to the south...");
				Msg("It will probably take the same time either way...<br/>If I were you, I'd walk down the slope to the south.");
				break;

			case "shop_healing":
				Msg("You can find a Healer's House in any decent town.");
				Msg("But, you know what? If I got hurt,<br/>I wouldn't mind walking a little more<br/>to go to Tir Chonaill to get treated.");
				Msg("The healer girl there, she's really pretty, isn't she? Hehe...");
				break;

			case "shop_inn":
				Msg("Want to go to the Inn?<br/>You don't look that tired...<br/>You've got some business with Nora or Piaras?");
				Msg("Tir Chonaill is in the north. Follow the road up.");
				break;

			case "shop_smith":
				Msg("Blacksmith's Shop?<br/>Walk along the path up to Tir Chonaill.<br/>And say hi to Ferghus for me.");
				Msg("Be careful of the wolves around here...<br/>Ah... nah, forget what I said. You're ugly enough to scare off any monsters.");
				break;

			case "skill_instrument":
				Msg("haha... You are not a complete idiot, after all!<br/>Now I chop down trees here,<br/>but I once used to dream of becoming a musician.");
				Msg("What? You want me to play music with this axe or something?<br/>You have to at least show me an instrument before asking me.");
				break;

			case "skill_tailoring":
				Msg("Tailoring skill?<br/>It's a good skill.");
				Msg("...");
				Msg("What more did you expect?");
				break;

			case "skill_smash":
				Msg("Smash? Go and ask Ranald about it...<br/>Ranald, that creep...<br/>Thinks he's some kind of a famous warrior...<br/>Wearing the same clothes as mine... Man, I don't like him...");
				break;

			case "pool":
				Msg("You want to jump into the water?<br/>I see... You are quite dirty and stained. A quick wash won't hurt you at all.");
				Msg("Ah, are you challenging me with your scowl?");
				break;

			case "farmland":
				Msg("Are you interested in farming? More so than in trees?<br/>I won't stop you if you insist...");
				Msg("Yes...<br/>Most of the towns in Erinn<br/>have farmlands nearby.<br/>I guess helping farmers would be good too.");
				break;

			case "windmill":
				Msg("Hehe...<br/>The Windmill up in Tir Chonaill, you know.<br/>Malcolm didn't listen to me<br/>and paid the price.");
				Msg("He made the Windmill frame against the woodgrain<br/>and the frame just broke in half....<br/>The broken Windmill blades flew so far away that<br/>one of them nearly fell here.");
				Msg("Do you get the lesson of my story?<br/>You might be really good at making something,<br/>but you should always listen to the one providing the materials.");
				Msg("I wonder how that cocky kid is doing... Hehe.");
				break;

			case "brook":
				Msg("Curious about Adelia Stream?");
				Msg("You want to play in the water or something?");
				Msg("Nah, first you need... a bath... Man, why don't you go clean yourself first.");
				break;

			case "shop_headman":
				Msg("The Chief's House?<br/>Ah, that stubborn old man.<br/>You mean Duncan, right?<br/>He lives at the highest place in Tir Chonaill... just like a monkey.");
				Msg("...");
				Msg("You came all the way out here just to ask me that?");
				break;

			case "temple":
				Msg("Hmm... You're looking for a church?<br/>Do you have anything to confess?<br/>Let me give you some comfort. Cheer up.");
				Msg("What? What's up with that look? I'm trying to help you, you know?");
				Msg("Humans tend to look for God when they are in trouble,<br/>but once they are OK, they don't even look back, man.<br/>Am I right or wrong?");
				break;

			case "school":
				Msg("School? Well...<br/>It all depends, you know...");
				Msg("Tir Chonaill's School is in the north,<br/>and the one in Dunbarton is in the south.<br/>Just follow the path.");
				Msg("Which reminds me... It's been some time since I last saw that man-beater Ranald...");
				break;

			case "skill_windmill":
				Msg("The Windmill skill is useful<br/>when you are surrounded by enemies,<br/>but it's really distracting...");
				Msg("Once you use it, you don't even know where you are...");
				break;

			case "skill_campfire":
				Msg("Haha...<br/>Most of the people coming to Ulaid Logging Camp<br/>actually show up here to build a campfire,<br/>not to get wood.");
				Msg("After spending a day or two here,<br/>even I get confused about where I am. A logging camp or a holiday campsite?");
				Msg("Haha... I don't mind it as long as there are some good views...");
				break;

			case "shop_restaurant":
				Msg("Restaurant? Are you hungry?<br/>Well, I'm sorry but... it's far from here...<br/>You need to go to a town to find one.");
				Msg("If you need some bread,<br/>I can give you mine for free.<br/>Want to hear about it?", Button("Sure", "@yes"), Button("Not interested", "@no"));
				switch (await Select())
				{
					case "@yes":
						Msg("Awesome! Repeat after me!<br/>[Tough Guy Tracy!]<br/>Say it 100 times and I'll give you a piece of bread.", Button("Tough Guy Tracy!", "@reply1"), Button("Crazy Dude", "@reply2"));
						switch (await Select())
						{
							case "reply1":
								// NOTE: He asks you to say it 100 times, but I received this message after 1 time
								Msg("Wow, did you actually do that?<br/>Your pride is worth... a measly piece of bread? Hahahahaha...");
								break;

							case "reply2":
								Msg("...<p/>Right back at ya!");
								break;
						}
						break;

					case "@no":
						Msg("Haha... Don't bother if you don't like it.<br/>Ah, this bread looks great!");
						break;
				}
				break;

			case "shop_armory":
				Msg("You need to go to the nearest town to find a Weapons Shop...<br/>Who would want to buy a weapon here, so far away from town?");
				Msg("Hmm... Now that you say so,<br/>it could be a good idea to open one here.");
				break;
				
			case "shop_bookstore":
				Msg("A bookstore?<br/>haha... You are interested in books?<br/>But you really don't look like one of those bookworms I know...");
				Msg("The nearest bookstore from here...<br/>Go to Dunbarton.<br/>Check out where it is for yourself.");
				break;

			case "shop_government_office":
				Msg("The Town Office? You are asking about the Town Office in Dunbarton, right?<br/>Then you should ask about it in Dunbarton!");
				Msg("You know how to get to Dunbarton, don't you?<br/>You can't read a map?<br/>Then what's the point of working so hard to make a Minimap?<br/>Just walk straight to the south.");
				Msg("Eavan at the Town Office is pretty...<br/>But she's so cold, man. Too cold...");
				break;

			case "graveyard":
				Msg("The graveyard... I believe other people have told you<br/>about it in detail...<br/>I'll tell you what, there's an interesting story about the graveyard...");
				Msg("You saw the dead tree in the back of the graveyard?<br/>You know why it died?");
				Msg("A lumberjack once walked close to the tree, you know.<br/>Very slowly, just one little step at a time.<br/>And the tree got so scared watching him that it shivered to death.");
				Msg("...");
				Msg("You're not entertained?");
				break;

			case "lute":
				Msg("You know a Lute is made of wood, right?<br/>You must carefully carve the wood thin<br/>and bend it slowly to adjust the sound.");
				Msg("That's why you need the outer rim of a tree<br/>to make a Lute.<br/>This part is soft and easy to bend.");
				Msg("But it can also change and break very easily.<br/>You must make sure to take care of it...");
				Msg("I saw some kids using Lutes as weapons.<br/>Man, that will definitely ruin the instrument.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog? Hahaha...<br/>You can tell it's full of baloney, can't you?");
				Msg("...<br/>It may not be a complete waste though.<br/>Kids can listen to that<br/>and have all the colorful dreams they wish.");
				break;

			case "musicsheet":
				GiveKeyword("shop_misc");
				Msg("Music Scores are sold at the General Shop.<br/>If you have one with you,<br/>you can play the music written on it.");
				Msg("But what's wrong with those morons<br/>complaining they can't play music with Music Scores<br/>when they don't have any instruments!");
				break;

			default:
				RndMsg(
					"I don't know anything about that. Try to make some sense.",
					"I don't think other people know about that...",
					"Man! You want me to talk about it?",
					"That's what you want to tell me? Childish....",
					"What? You want to be a know-it-all?",
					"Why do you care about all that weird stuff?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class TracyShop : NpcShopScript
{
	public override void Setup()
	{
		// Hunting quests aren't implemented yet
		//Add("Quest", 70095);			// Hunting Quest (Brown Fox)
		//Add("Quest", 70099);			// Hunting Quest (Brown Bear)
		//Add("Quest", 70100);			// Hunting Quest (Red Bear)
		//Add("Quest", 70089);			// Hunting Quest (Black Dire Wolf)
		//Add("Quest", 70090);			// Hunting Quest (White Dire Wolf)
		//Add("Quest", 70117);			// Hunting Quest (Raccoon)
		//Add("Quest", 70119);			// Hunting Quest (Wisp)

		// Party Quest tab

		Add("Food", 50004);				// Bread

		Add("Carpentry Tool", 40022);	// Gathering Axe
		Add("Carpentry Tool", 63223);	// Woodworking Plane
		Add("Carpentry Tool", 63222);	// Lumber Axe
	}
}
