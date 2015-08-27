//--- Aura Script -----------------------------------------------------------
// Jenifer
//--- Description -----------------------------------------------------------
// Bartender
//---------------------------------------------------------------------------

public class JeniferScript : NpcScript
{
	public override void Load()
	{
		SetName("_jenifer");
		SetRace(10001);
		SetBody(height: 1.1f, weight: 1.1f, lower: 1.1f);
		SetFace(skinColor: 17, eyeType: 4, eyeColor: 119, mouthType: 1);
		SetStand("human/female/anim/female_natural_stand_npc_lassar");
		SetLocation(31, 14628, 8056, 26);

		EquipItem(Pocket.Face, 3901, 0x00D9E9F7, 0x00930B6A, 0x00474C00);
		EquipItem(Pocket.Hair, 3001, 0x00240C1A, 0x00240C1A, 0x00240C1A);
		EquipItem(Pocket.Armor, 15020, 0x00F98C84, 0x00FBDDD7, 0x00351311);
		EquipItem(Pocket.Shoe, 17013, 0x00000000, 0x00366961, 0x00DAD6EB);

		AddGreeting(0, "Welcome to the Bangor Pub. Are you a first-time visitor?");
		AddGreeting(0, "I think I've met you once before... You name is... <username/>, am I right?");

		AddPhrase("Ah, I'm so bored...");
		AddPhrase("Ah. What an unbelievably beautiful weather...");
		AddPhrase("I could never keep this place clean... It always gets dirty.");
		AddPhrase("I thought there was something else that needed to be done...");
		AddPhrase("I wish it would rain so I could take a day off.");
		AddPhrase("I'm gonna get drunk if I drink too much...");
		AddPhrase("I'm so tired...");
		AddPhrase("It would be nice if Riocard drank...");
		AddPhrase("Perhaps I should lose some weight...");
		AddPhrase("Riocard! Did you finish everything I asked you to do?");
		AddPhrase("Riocard. Come play with me.");
		AddPhrase("Today's fortune is... no profit?");
		AddPhrase("Wait a minute...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Jenifer.mp3");

		await Intro(
			"Well-groomed purple hair, a face as smooth as flawless porcelain,",
			"and brown eyes with thick mascara complemented by a mole that adds beauty to her oval face.",
			"The jasmine scent fills the air every time her light sepia healer dress moves,",
			"and her red cross earrings dangle and shine as her smile spreads across her lips."
		);

		Msg("Mmm? How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Player.Titles.SelectedTitle == 11002)
					Msg("<username/>, protecting Erinn is a noble thing,<br/>but how about protecting someone near you first?<br/>You won't regret taking my advice.");
				await Conversation();
				break;

			case "@shop":
				Msg("What do you need?<br/>Right now, we're just selling a few food items.");
				OpenShop("JeniferShop");
				return;

			case "@repair":
				Msg("I can fix accessories.<br/>I know this sounds funny coming from me, but it's not very good to repair accessories.<br/>First off, it costs too much.<br/>You might be better off buying a new one than repairing it. But if you still want to repair it...");
				Msg("Unimplemented");
				Msg("It must be very precious to you if you want to repair an accessory.<br/>I totally understand. But it takes on another kind of charm as it tarnishes, you know?<br/>Well, see you again.");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "My name? It's Jennifer. Ha ha.<br/>Welcome to my Pub.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "If you're curious, try talking to Riocard over there.<br/>He may not look it, but he's smart and has a good memory.<br/>And he knows a lot of rumors from various places. Ha ha.");
				Msg("Yeah, the one over there in yellow wearing a hat.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("Even the gentlest of the Bards can be downright scary if they get angry.<br/>When they fight, they throw their instruments like weapons!");
				break;

			case "about_arbeit":
				Msg("Well...<br/>Riocard is enough help for now...");
				Msg("Let me talk to Riocard first and I'll get back to you.");
				break;

			case "shop_misc":
				Msg("Ha ha... You're looking for Gilmore's shop.<br/>It's just over there. You might have a hard time finding it though, 'cause you have to go through an alley...");
				break;

			case "shop_grocery":
				Msg("Yes, you've come to the right place. We carry food items, too.<br/>Would you like to see if there's something you need?");
				break;

			case "shop_healing":
				Msg("Unfortunately we don't have a Healer's House in this town.<br/>Although, I thought I saw<br/>Comgan carrying some potions and bandages.");
				Msg("You should go there and check it out.");
				break;

			case "shop_inn":
				Msg("Ha ha... A lot of people ask me that.<br/>Maybe I should start an inn, too.");
				Msg("Hey, Riocard! What do you think?");
				Msg("...");
				Msg("He's not answering... jerk...");
				break;

			case "shop_bank":
				Msg("Hmm.<br/>You should go talk to Bryce for that.");
				Msg("Yeah, you know. That dandy gentleman<br/>by the General Shop. Tee hee.");
				break;

			case "shop_smith":
				Msg("You mean Elen's Blacksmith's Shop?<br/>It's just over there, hehehe...");
				break;

			case "skill_rest":
				Msg("...Why don't you use it at this Pub? Tee hee.");
				break;

			case "skill_instrument":
				Msg("It's a romantic skill.");
				Msg("You can easily pick it up<br/>as long as you have an instrument...");
				break;

			case "skill_composing":
				Msg("...Many people misunderstand.<br/>Composition is not merely a transcription<br/>but a way to express the melody inside your heart.");
				Msg("Listening to some people play nowadays,<br/>it seems that only a very few people actually play what they truly feel inside.<br/>Others seem to just repeat what they heard.");
				Msg("...If being a bard was so easy,<br/>I wouldn't have given up that path...");
				break;

			case "skill_tailoring":
				Msg("...'I hear you need a Tailoring Kit for that.<br/>The General Shop probably carries it.");
				break;

			case "skill_gathering":
				Msg("I don't know... I'll tell you if I need it. Ha ha.");
				break;

			case "temple":
				Msg("Now that you mention it, Comgan does want to build a church in this town.");
				Msg("The way I look at it, I doubt he'll be able to build one even if he grows up<br/>to be as old as priests from other villages.<br/>If you'd like, why don't you go lend him a hand?");
				break;

			case "school":
				Msg("No, this town doesn't have a school.<br/>Business is pretty slow during the day, so it would nice to learn something...");
				break;

			case "skill_campfire":
				Msg("If you're going to make fire, please go to a safe place.<br/>I can't have my Pub set on fire.");
				break;

			case "shop_restaurant":
				Msg("Ha ha... You're quite picky aren't you...?<br/>You can just buy food here.<br/>Who needs a restaurants?");
				break;

			case "shop_armory":
				Msg("Hmm... You want to buy weapons?");
				Msg("Then go over to the Blacksmith and<br/>speak with Elen.");
				Msg("Elen seems to be selling the items<br/>that Edern makes. Hehe...");
				break;

			case "shop_cloth":
				Msg("I'm not happy about not having a clothing shop here either.<br/>I have nowhere to go if I feel like getting new clothes.");
				Msg("Gilmore carries some clothes at his shop,<br/>but he doesn't have anything beautiful and I only end up getting nagged there.");
				Msg("Well, I guess on the bright side. I save money that way... Hehehe.");
				break;

			case "shop_bookstore":
				Msg("So you like books, huh?<br/>Oh, me too.");
				Msg("I've read all kinds of books since I was a child.<br/>Tales of Dungeon Explorations, The White Doe Story, The Story of Sealstones...<br/>The White Doe Story is particularly touching, don't you think...?");
				Msg("But this town doesn't really have a bookstore where you can buy decent books.<br/>Maybe I should even start a bookshop?");
				Msg("Hey, Riocard! What do you think?");
				Msg("...");
				Msg("He's ignoring me again. I should just fire him.");
				break;

			case "shop_goverment_office":
				Msg("A town office? In this town?<br/>Then I would have to pay taxes. No way!");
				break;

			case "graveyard":
				Msg("You seem to enjoy scary stories.");
				Msg("You should probably go talk to Riocard then.<br/>I don't think it's something you should talk about with a frail lady like me.");
				break;

			default:
				RndFavorMsg(
					"Hang on... Riocard! Where are you? I've got something to ask you.",
					"Hmm, it sounds like an interesting story, but can you tell other ones too?",
					"Hmm. It seems like an interesting topic, but I don't know too much about it.<br/>Can we talk about something else?",
					"Trying to capture a woman's heart with a story like that simply doesn't work. Ha ha.<br/>Ha ha. Just for you, <username/>, I'll let this one go."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class JeniferShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Food", 50001);    // Big Lump of Cheese
		Add("Food", 50002);    // Slice of Cheese
		Add("Food", 50004);    // Bread
		Add("Food", 50005);    // Large Meat
		Add("Food", 50006, 5); // Sliced Meat x5

		Add("Gift", 52010); // Ramen
		Add("Gift", 52019); // Heart Cake
		Add("Gift", 52021); // Slice of Cake
		Add("Gift", 52022); // Wine
		Add("Gift", 52023); // Wild Ginseng

		Add("Cooking Tools", 40042); // Cooking Knife
		Add("Cooking Tools", 40043); // Rolling Pin
		Add("Cooking Tools", 40044); // Ladle
		Add("Cooking Tools", 46004); // Cooking Pot
		Add("Cooking Tools", 46005); // Cooking Table

		Add("Event"); // Empty
	}
}