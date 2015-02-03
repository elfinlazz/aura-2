//--- Aura Script -----------------------------------------------------------
// Eavan
//--- Description -----------------------------------------------------------
// Public Servant
//---------------------------------------------------------------------------

public class EavanScript : NpcScript
{
	public override void Load()
	{
		SetName("_eavan");
		SetRace(10001);
		SetBody(weight: 0.7f, upper: 0.7f, lower: 0.7f);
		SetFace(skinColor: 15, eyeType: 3, eyeColor: 3);
		SetStand("human/female/anim/female_natural_stand_npc_Eavan");
		SetLocation(14, 40024, 41041, 192);

		EquipItem(Pocket.Face, 3900, 0x003B9C3F, 0x000896D4, 0x0093A6D4);
		EquipItem(Pocket.Hair, 3022, 0x00FFEEAA, 0x00FFEEAA, 0x00FFEEAA);
		EquipItem(Pocket.Armor, 15041, 0x00FFCCCC, 0x0080C5D3, 0x00A7ACB4);
		EquipItem(Pocket.Glove, 16015, 0x00FFFFFF, 0x00E6F2E2, 0x006161AC);
		EquipItem(Pocket.Shoe, 17008, 0x00DDAACC, 0x00F79B2F, 0x00E10175);

		AddGreeting(0, "Welcome to Dunbarton.<br/>My name is <npcname/>, the Town Office worker who takes care of all the business related to the Adventurers' Association.");
		AddGreeting(1, "Hmm. I've seen someone that looks like you before.");

		AddPhrase("*Sigh* Back to work.");
		AddPhrase("Hmm. This letter is fairly well done. B+.");
		AddPhrase("Next person please!");
		AddPhrase("Next, please!");
		AddPhrase("Registration is this way!");
		AddPhrase("Teehee... Another love letter.");
		AddPhrase("The Adventurers' Association is this way!");
		AddPhrase("Ugh. I wish I could take a breather...");
		AddPhrase("What's with this letter? How unpleasant!");
		AddPhrase("Whew. I want to take a break...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Eavan.mp3");

		await Intro(
			"Wearing a rosy pink blouse, her shoulders are gently covered by her blonde hair that seems to wave in the breeze.",
			"An oval face, a pair of calm eyes with depth, and a slightly small nose with a rounded tip...",
			"Beneath are the lips that shine in the same color as her blouse."
		);

		Msg("This is the Adventurers' Association.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Retrieve Lost Items", "@lostandfound"), Button("About Daily Events", "@daily_quest") /*, Button("Daily Dungeon Quest", "@daily_dungeon_quest")*/);

		switch (await Select())
		{
			case "@talk":
				GiveKeyword("shop_goverment_office");
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("The Guardian of Erinn, <username/>...<br/>You are always welcomed here.");
				await Conversation();
				break;

			case "@shop":
				Msg("Welcome to the Adventurers' Association.");
				OpenShop("EavanShop");
				return;

			case "@lostandfound":
				Msg("At the Town Office, you can retrieve the items you've lost during your adventure.<br/>Unless you dropped an item on purpose while using Magical powers,<br/>you can usually retrieve it here with the blessing still cast on it.");
				Msg("You have to pay a small fee though, and only up to 20 items are stored.<br/>Any more than that, and the items will be lost starting with the oldest lost items first.");
				Msg("Unimplemented");
				break;

			case "@daily_quest":
				Msg("Did you receive today's Daily Event quest?<br/>Every day, you'll get a mission for each region.<br/>For instance, you can complete one mission each<br/>at Tara and at Taillteann.");
				Msg("Once you have completed an event quest from one region,<br/>you will automatically receive the next region's event quest.");
				Msg("Expired daily event quests will automatically disappear, so<br/>don't forget to do them!");
				break;

			/*case "@daily_dungeon_quest":
				Msg("Would you like to take on the once-a-day challenge of clearing Uladh Dungeon?", Button("Accept", "@ok"), Button("Refuse", "@no"));

				switch(await Select())
				{
					case "@ok":
						if (!QuestActive(70079))
						{
							StartQuest(70079); //[Daily Quest] Uladh Dungeon
							Msg("Good luck.");
						}
						else
						{
							Msg("You already received the Daily Dungeon Quest. Come back tomorrow.");
						}
						break;

					case "@no":
						Msg("Guess you're too busy.");
						break;
				}
				break;*/
		}
		
		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Memory == 1)
				{
					Msg("You said your name was... <username/>, right? Tell me what's going on.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "I've been kind of busy today.<br/>There are lots of people looking for work.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor": 
				Msg(FavorExpression(), "Dunbarton is a city located near the border of the Kingdom of Aliech.<br/>It attracts a lot of travelers who are looking for adventure.<br/>If you'd like to improve your skills, how about going to the school?");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_arbeit":
				Msg("Unimplemented"); 
				break;

			case "shop_misc":
				Msg("Looking for the General Shop?<br/>It's to the west of the Town Office.<br/>It's just over there, so you should be able to find it easily.<br/>Once you're there, talk to Walter.");
				break;

			case "shop_grocery":
				Msg("A grocery store? The Restaurant carries cooking ingredients too,<br/>so why don't you just go there?<br/>It's close from here.");
				break;

			case "shop_healing":
				Msg("Hmm... You mean Manus' place.<br/>Follow the path to the right of the Square and go straight south.<br/>It's the L-shaped building right next to the bend in the road.<br/>Watch for the sign and you'll easily spot it.");
				break;

			case "shop_inn":
				Msg("This town doesn't have an inn yet.");
				break;

			case "shop_bank":
				Msg("The Bank is just near the General Shop.<br/>Talk to Austeyn.");
				break;

			case "shop_smith":
				Msg("Hmm. We don't have a Blacksmith's Shop here.<br/>If it is weapons or armor you are looking for,<br/>why don't you check out Nerys' Weapons Shop?");
				break;

			case "skill_range":
				Msg("Huh? What is that?");
				break;

			case "skill_instrument":
				Msg("If you know how to play music,<br/>would you play a song for me?");
				break;

			case "skill_composing":
				Msg("I'm not really good at it.<br/>Most townsfolk here are also tone deaf<br/>so I'm not sure you'll find someone who knows how to do it.");
				break;

			case "skill_tailoring":
				Msg("Hmm. Do I look like a housewife<br/>who stays home all day working?");
				break;

			case "square":
				Msg("Hmm. Do I look like a housewife<br/>who stays home all day working?");
				break;

			case "pool":
				Msg("I don't know if we have one around here...<br/>All we have is a well. Nothing big like that.");
				break;

			case "farmland":
				Msg("It's just outside from here.<br/>Are you looking for anything in particular?");
				break;

			case "brook":
				Msg("Well...<br/>I heard that there is a stream by that name<br/>somewhere far up north.");
				break;

			case "shop_headman":
				Msg("The leader of this town is a Lord.<br/>It's not proper to call him a chief.");
				break;

			case "temple":
				Msg("To get to the church, follow along the alley next to the Restaurant west of here.<br/>You'll see the Lymilark cross tower as well as Priestess Kristell,<br/>so you should be able to find it easily.");
				break;

			case "school":
				Msg("A school? It's not over here.<br/>You should go east. Turn right at the Bookstore<br/>and go up from there.");
				break;

			case "skill_campfire":
				Msg("I'm sure it's a graceful skill,<br/>but I don't think it's for a frail lady.");
				break;

			case "shop_restaurant":
				Msg("The Restaurant is right next to us.<br/>You might have missed it because of the alley...<br/>Talk to Glenis.");
				break;

			case "shop_armory":
				Msg("If you mean Nerys' Weapons Shop,<br/>follow the road down south and you'll see it.");
				break;

			case "shop_cloth":
				Msg("Do you want to buy clothes?<br/>Go straight west.<br/>Don't leave the Square, though.");
				break;

			case "shop_bookstore":
				Msg("Head east.<br/>Go down the alley when you see it.<br/>The Minimap should help you.");
				break;

			case "shop_goverment_office":
				Msg("Mmm? This is the Town Office.<br/>Oh... Sorry, but you are not allowed to enter.");
				break;

			default:
				RndFavorMsg(
					"I don't know much about all that. <username/>, you do understand, don't you?",
					"You know, something's just come up and I'm a bit busy right now.<br/>Do you mind coming back another day?",
					"Must be that you've been exploring for so long now, right?<br/><username/>, you can certainly know a whole lot.",
					"I'm feeling achy all over today.<br/>I think I need to get some rest now. I'm so sorry, <username/>.<br/>I feel like I've heard something like that before... Perhaps I can find some notes I've jotted down in my expedition journal?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class EavanShop : NpcShopScript 
{
	public override void Setup() 
	{
		Add("Quest", 70080); // Hunting Quest - 10 Skeleton's Former Scrolls
		Add("Quest", 70081); // Hunting Quest - 10 Red Skeleton's Former Scrolls
		Add("Quest", 70082); // Hunting Quest - 10 Metal Skeleton's Former Scrolls
		Add("Quest", 70086); // Hunting Quest - 10 Skeletons Wolf's Former Scrolls
		Add("Quest", 70095); // Hunting Quest - 10 Brown Fox Former Scrools
		Add("Quest", 70081); // Hunting Quest - 10 Red Fox Former Scrolls
		Add("Quest", 70097); // Hunting Quest - 10 Gray Fox Former Scrolls
		Add("Quest", 70100); // Hunting Quest - 10 Red Bear Former Scrolls
		Add("Quest", 70102); // Hunting Quest - 10 Brown Grizzly Bear's Former Scrolls
		Add("Quest", 70103); // Hunting Quest - 10 Red Grizzly Bear's Former Scrolls
		Add("Quest", 70104); // Hunting Quest - 10 Black Grizzly Bear's Former Scrolls
		Add("Quest", 70106); // Hunting Quest - 10 Mimic Former Scrolls
		Add("Quest", 70105); // Hunting Quest - 10 Bat Former Scrolls
		Add("Quest", 70108); // Hunting Quest - 10 Brown Town Rat's Former Scrolls
		Add("Quest", 70109); // Hunting Quest - 10 Gray Town Rat Former Scrolls
		Add("Quest", 70114); // Hunting Quest - 10 Kobold's Former Scrolls
		Add("Quest", 70115); // Hunting Quest - 10 Poison Kobold's Former Scrolls
		Add("Quest", 70116); // Hunting Quest - 10 Gold Kobold's Former Scrolls
		Add("Quest", 70118); // Hunting Quest - 10 Imp's Former Scrolls
		Add("Quest", 70139); // Hunting Quest - 10 Flying Sword's Former Scrolls
		Add("Quest", 70119); // Hunting Quest - 10 Whisp Former Scrolls
		Add("Quest", 70099); // Hunting Quest - 10 Brown Bear Former Former Scrolls
		Add("Quest", 70091); // Hunting Quest - 10 White Spider Former Scrolls
		Add("Quest", 70092); // Hunting Quest - 10 Black Spider Former Scrolls
		Add("Quest", 70093); // Hunting Quest - 10 Red Spider Former Scrolls
		Add("Quest", 70137); // Hunting Quest - 10 Ice Sprite's Former Scrolls
		Add("Quest", 70138); // Hunting Quest - 10 Fire Sprite's Former Scrolls
		Add("Quest", 70137); // Hunting Quest - 10 Ice Sprite's Former Scrolls

		Add("Party Quest", 70025); // Party Quest - 10 Gold Goblins
		Add("Party Quest", 70025); // Party Quest - 30 Skeletons
		Add("Party Quest", 70025); // Party Quest - 30 Red Skeletons
		Add("Party Quest", 70025); // Party Quest - 30 Metal Skeletons
		Add("Party Quest", 70025); // Party Quest - 30 Skeleton Wolves
		Add("Party Quest", 70025); // Party Quest - 10 Metal Skeletons and 20 Red Skeletons
		Add("Party Quest", 70025); // Party Quest - 30 Kobolds
		Add("Party Quest", 70025); // Party Quest - 30 Poison Kobolds
		Add("Party Quest", 70025); // Party Quest - 10 Poison Kobolds and 20 Kobold Archers
		Add("Party Quest", 70025); // Party Quest - Lycanthrope in Rabbie Basic
		Add("Party Quest", 70025); // Party Quest - Black Succubus in Rabbie Adv. for 2
		Add("Party Quest", 70025); // Party Quest - Red Succubus in Rabbie Adv. for 3
		Add("Party Quest", 70025); // Party Quest - Red Succubus in Rabbie Adv.

		Add("Guild", 63040); // Guild Formation Permit
		Add("Guild", 63041); // Guild Stone Installation Permit

		Add("Gift", 52014); // Teddy Bear
		Add("Gift", 52016); // Bunny Doll
		Add("Gift", 52015); // Pearl Necklace
		Add("Gift", 52025); // Gift Ring

		Add("Arena", 63050, 10);  // Rabbie Battle Arena Coin x10
		Add("Arena", 63050, 20);  // Rabbie Battle Arena Coin x20
		Add("Arena", 63050, 50);  // Rabbie Battle Arena Coin x50
		Add("Arena", 63050, 100); // Rabbie Battle Arena Coin x100

		Add("Guild Quest", 70152); // Guild Quest - Demi Lich
		Add("Guild Quest", 70152); // Guild Quest - Banshee
		Add("Guild Quest", 70152); // Guild Quest - Goblin Bandits
		Add("Guild Quest", 70152); // Guild Quest - Giant Ogre
		Add("Guild Quest", 70152); // Guild Quest - Gaint Bear

		Add("Guild Robe", 19047); // [Guild Name] Guild Robe
	}
}