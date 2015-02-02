//--- Aura Script -----------------------------------------------------------
// Aeira
//--- Description -----------------------------------------------------------
// Bookstore Owner
//---------------------------------------------------------------------------

public class AeiraScript : NpcScript
{
	public override void Load()
	{
		SetName("_aeira");
		SetRace(10001);
		SetBody(height: 0.8f);
		SetFace(skinColor: 16, eyeType: 2, eyeColor: 27, mouthType: 1);
		SetStand("human/female/anim/female_natural_stand_npc_Aeira");
		SetLocation(14, 44978, 43143, 158);

		EquipItem(Pocket.Face, 3900, 0x0090CEF1, 0x00006B55, 0x006E6162);
		EquipItem(Pocket.Hair, 3022, 0x00664444, 0x00664444, 0x00664444);
		EquipItem(Pocket.Armor, 15042, 0x00EBAE98, 0x00354E34, 0x00E3E4EE);
		EquipItem(Pocket.Shoe, 17024, 0x00A0505E, 0x00F8784F, 0x00006E41);
		EquipItem(Pocket.Head, 18028, 0x00746C54, 0x00C0C0C0, 0x00007C8C);

		AddGreeting(0, "I'm sorry, but your name is...?<br/>Mmm? <username/>? Nice to meet you.");
		AddGreeting(1, "Hahaha. I... Umm... I think I've met you before...<br/>Your name was...<br/>Oh, I'm sorry, <username/>. My mind went blank for a second. Hehehe.");

		AddPhrase("*cough* The books are too dusty...");
		AddPhrase("*Whistle*");
		AddPhrase("Hahaha.");
		AddPhrase("Hmm. I can't really see...");
		AddPhrase("Hmm. The Bookstore is kind of small.");
		AddPhrase("I wonder if this book would sell?");
		AddPhrase("I wonder what Stewart is up to?");
		AddPhrase("Kristell... She's unfair.");
		AddPhrase("Oh, hello!");
		AddPhrase("Umm... So...");
		AddPhrase("Whew... I should just finish up the transcription.");
	}

	protected override async Task Talk() 
	{
		SetBgm("NPC_Aeira.mp3");

		await Intro(
			"This girl seems to be in her late teens with big thick glasses resting at the tip of her nose.",
			"Behind the glasses are two large brown eyes shining brilliantly.",
			"Wearing a loose-fitting dress, she has a ribbon made of soft and thin material around her neck."
		);

		Msg("So, what can I help you with?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("Wow... <username/>, you really<br/>rescued Erinn?<br/>I wasn't sure before, but you really are an amazing person.<br/>Please continue to watch over my Bookstore!");
				await Conversation();
				break;

			case "@shop":
				Msg("Welcome to the Bookstore.");
				OpenShop("AeiraShop");
				return;
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
					Msg("My name? It's <npcname/>. We've never met before, have we?");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					GiveKeyword("shop_bookstore");
					Msg(FavorExpression(), "Hehehe... I may not look the part, but I own this Bookstore.<br/>It's okay to be casual, but<br/>at least give me some respect as a store owner.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				GiveKeyword("school");
				Msg(FavorExpression(), "If you want to properly train the stuff that's written on the book,<br/>why don't you first read the book in detail, then visit the school?<br/>Oh, and don't forget to talk to Stewart when you're there.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("I've talked a lot with other people regarding skills, but<br/>you seem be very knowledgeable about music,<username/>.<br/>I'm impressed. Hahaha.");
				break;

			case "about_arbeit":
				Msg("Unimplemented");
				break;

			case "shop_misc":
				Msg("Hmm. The General Shop?<br/>That's where my father works at!<br/>To get to the General Shop,<br/>keep going straight towards the Square.");
				Msg("The shop is reasonably priced, with plenty of quality items...<br/>And there's just about everything you may be looking for,<br/>so don't forget to pay a visit! Hehe.");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("A grocery store?<br/>In this town, you can find the food ingredients at the Restaurant, too,<br/>so try going there instead.");
				break;

			case "shop_healing":
				Msg("A healer? That would be Manus! You're looking for Manus' place!<br/>His house is down there.<br/>Head south along the main road to the Square.");
				break;

			case "shop_inn":
				GiveKeyword("skill_campfire");
				Msg("Oh, no. There is no inn in our town.<br/>There's really no good place to rest either.<br/>Why don't you stay with people who can use the Campfire skill for now?");
				break;

			case "shop_bank":
				Msg("Are you looking for Austeyn?<br/>The Bank is on the west end of the Square.<br/>You should be able to spot the sign easily.");
				Msg("Go there right now!");
				break;

			case "shop_smith":
				Msg("Do we have a blacksmith's shop in this town? I think Nerys might know.<br/>It's just that I've never seen Nerys hammering<br/>or using the bellow.");
				Msg("You might want to go visit the Weapons Shop first.");
				break;

			case "skill_range":
				Msg("Hmm. A long range attack is...!");
				Msg("It's a way of attacking a hostile<br/>enemy at a certain distance by using<br/>devices such as a bow, spear, spells, etc...");
				Msg("Oh, pardon me.<br/>I got excited whenever I hear something I know.");
				break;

			case "skill_instrument":
				Msg("Hehe... I guess I'm not very good at something like that.");
				Msg("But those who bought instruments at my father's General Shop<br/>quickly picked it up after playing it only a few times.");
				break;

			case "skill_tailoring":
				Msg("Why don't you go ask Simon?<br/>It would also help to buy a Tailoring Kit from my father's shop.");
				break;

			case "skill_gathering":
				Msg("There was a time when I followed Stewart around<br/>to pick herbs all over the town.<br/>It was so much fun!");
				break;

			case "square":
				Msg("The Square is right over there<br/>through the alley.");
				break;

			case "farmland":
				Msg("The farmland is near here but everyone is stressed out over<br/>all the rats that are showing up there. Yuck!<br/>If you see them, please get them out of there.");
				break;

			case "brook":
				Msg("Adelia Stream?<br/>Yes, I've heard of it.");
				Msg("It's supposed to be a stream that flows near a town called Tir Chonaill,<br/>which is located just up north.");
				Msg("I don't think it flows<br/>all the way down here, though.");
				break;

			case "shop_headman":
				Msg("Hmm. Our town Chief?<br/>Never heard of one.<br/>Umm... Do we have one?");
				break;

			case "temple":
				Msg("Oh...<br/>Church is located in the<br/>northwest part of town.");
				break;

			case "school":
				Msg("The School is up there! Head straight up and you'll see it.<br/>If you have a good set of eyes, you could see it from here, too. Hehehe.");
				Msg("Oh, if you get to go there,<br/>please see how Stewart is doing! Please.");
				break;

			case "skill_campfire":
				Msg("Hmm.<br/>Sometimes there are novels<br/>that feature adventurers building a campfire.");
				Msg("In the middle of the field, gazing upon twinkling stars,<br/>sitting around a bonfire that illuminates the darkness,<br/>enjoying the moment with others...<br/>It's all so romantic!");
				Msg("Wait a minute... Did I order the books on campfire?");
				break;

			case "shop_restaurant":
				Msg("The Restaurant? You must be talking about Glenis' place.<br/>All you need to do is go straight to the Square.");
				Msg("While you are on the way, make sure to<br/>visit the General Shop, too. Tee hee.");
				break;

			case "shop_armory":
				Msg("The Weapons Shop is over there by the south entrance.<br/>Nerys is usually outside the store so ask her.");
				Msg("By the way, weapons or armor here might be<br/>really expensive...");
				break;

			case "shop_cloth":
				Msg("The Clothing Shop? You can find it at the Square.<br/>How do you like what I'm wearing? I bought this there, too.");
				Msg("I like the clothes there so much that I bought several more from them!");
				break;

			case "shop_bookstore":
				Msg("Yes, this is the Bookstore! Ta-da!<br/>Feel free to look around, and press the 'Shop' button when you're ready.");
				Msg("I know this place isn't that big so you may not find the book you like,<br/>but we often bring in new shipments so don't be too disappointed<br/>if you can't find what you like today, okay?");
				break;

			case "shop_goverment_office":
				Msg("The Town Office? Are you looking for Eavan?<br/>Are you close friends with her?<br/>What's the relationship between you two?");
				Msg("Oh, nothing. It's just strange to see someone<br/>who's looking for someone who's as cold as ice. Hehe.<br/>...");
				break;

			default:
				RndFavorMsg(
					"...?",
					"Oh... Umm... That... I don't know.",
					"I'm not sure I know. Maybe Stewart knows.",
					"I don't know too much about that. Sorry...",
					"I don't really understand what you just said...",
					"Yeah, but... I don't really know anything about that.",
					"Hahaha. Well, it's not really my area of expertise...",
					"I don't know much about it, but let me know if you find out more.",
					"I'm not sure exactly what that is but it seems important,<br/>seeing how so many people inquire about it..."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class AeiraShop : NpcShopScript 
{
	public override void Setup() 
	{
		Add("Skill Book", 1006); // Introduction to Music Composition
		Add("Skill Book", 1012); // Campfire Manual
		Add("Skill Book", 1505); // The World of Handicrafts
		Add("Skill Book", 1302); // Your first Glass of Wine Vol. 1
		Add("Skill Book", 1303); // Your first Glass of Wine Vol. 2
		Add("Skill Book", 1011); // Improving Your Composing Skill
		Add("Skill Book", 1304); // Wine for the Everyman
		Add("Skill Book", 1018); // The History of Music in Erinn (1)
		Add("Skill Book", 1305); // Tin's Liquor Drop
		Add("Skill Book", 1083); // Campfire Skill: Beyond the Kit
		Add("Skill Book", 1064); // Master Chef's Cooking Class: Baking
		Add("Skill Book", 1065); // Master Chef's Cooking Class: Simmering
		Add("Skill Book", 1019); // The History of Music in Erinn (2)
		Add("Skill Book", 1066); // About Kneading
		Add("Skill Book", 1020); // Composition Lessons with Helene (1)
		Add("Skill Book", 1123); // The Great Camping Companion: Camp Kit
		Add("Skill Book", 1007); // Healing: The Basics of Magic
		Add("Skill Book", 1029); // A Campfire Memory
		Add("Skill Book", 1114); // The History of Music in Erinn (3)
		Add("Skill Book", 1111); // The Path of Composing
		Add("Skill Book", 1013); // Music Theory

		Add("Life Skill Book", 1055); // The Road to Becoming a Magic Warrior
		Add("Life Skill Book", 1056); // How to Enjoy Field Hunting
		Add("Life Skill Book", 1092); // Enchant, Another Mysterious Magic
		Add("Life Skill Book", 1124); // An Easy Guide to Taking Up Residence in a Home
		Add("Life Skill Book", 1102); // Your Pet
		Add("Life Skill Book", 1052); // How to milk a Cow
		Add("Life Skill Book", 1050); // An Unempolyed Man's Memoir of Clothes
		Add("Life Skill Book", 1040); // Facial Expressions Require Practice too
		Add("Life Skill Book", 1046); // Fire Arrow, The Ultimate Archery
		Add("Life Skill Book", 1021); // The Tir Chonaill Environs
		Add("Life Skill Book", 1022); // The Dunbarton Environs
		Add("Life Skill Book", 1043); // Wizards Love the Dark
		Add("Life Skill Book", 1057); // Introduction to Field Bosses
		Add("Life Skill Book", 1058); // Understanding Whisps
		Add("Life Skill Book", 1015); // Seal Stone Research Almanac: Rabbie Dungeon
		Add("Life Skill Book", 1016); // Seal Stone Research Almanac: Ciar Dungeon
		Add("Life Skill Book", 1017); // Seal Stone Research Almanac: Dugald Aisle
		Add("Life Skill Book", 1033); // Guidebook for Dungeon Exploration - Theory
		Add("Life Skill Book", 1034); // Guidebook for Dungeon Exploration - Practicum
		Add("Life Skill Book", 1035); // An Adventurer's Memoir
		Add("Life Skill Book", 1077); // Wanderer of the Fiodh Forest
		Add("Life Skill Book", 1090); // How Am I Going to Survive Like This?
		Add("Life Skill Book", 1031); // Understanding Elementals
		Add("Life Skill Book", 1036); // Records of the Bangorr Seal Stone Investigation
		Add("Life Skill Book", 1072); // Cooking on Your Own Vol. 1
		Add("Life Skill Book", 1073); // Cooking on Your Own Vol. 2

		Add("Literature", 1023);  // The Story of Spiral Hill
		Add("Literature", 1025);  // Mystery of the Dungeon
		Add("Literature", 1026);  // A Report on Astralium
		Add("Literature", 1027);  // I Hate Cuteness
		Add("Literature", 1028);  // Tracy's Secret
		Add("Literature", 1032);  // The Shadow Mystery
		Add("Literature", 1140);  // It's a 'paper airplane' that flies.
		Add("Literature", 1001);  // The Story of a White Doe
		Add("Literature", 1059);  // A Campfire Story
		Add("Literature", 1060);  // Imp's Diary
		Add("Literature", 1061);  // The Tale of Ifan the Rich
		Add("Literature", 1042);  // Animal-loving Healer
		Add("Literature", 1103);  // The Story of a Lizard
		Add("Literature", 1104);  // The Origin of Moon Gates
		Add("Literature", 74028); // The Forgotten Legend of Fiodh Forest
		Add("Literature", 74029); // The Tragedy of Emain Macha
		Add("Literature", 74027); // The Knight of Light Lugh, The Hero of Mag Tuireadh
	}
}