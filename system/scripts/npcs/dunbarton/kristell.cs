//--- Aura Script -----------------------------------------------------------
// Kristell
//--- Description -----------------------------------------------------------
// Priestess
//---------------------------------------------------------------------------

public class KristellScript : NpcScript
{
	public override void Load()
	{
		SetName("_kristell");
		SetRace(10001);
		SetBody(height: 0.97f);
		SetFace(skinColor: 15, eyeType: 3, eyeColor: 191);
		SetStand("human/female/anim/female_natural_stand_npc_Kristell");
		SetLocation(14, 34657, 42808, 0);

		EquipItem(Pocket.Face, 3900, 0x00F8958F, 0x005A4862, 0x00714B4B);
		EquipItem(Pocket.Hair, 3017, 0x00EE937E, 0x00EE937E, 0x00EE937E);
		EquipItem(Pocket.Armor, 15009, 0x00303133, 0x00C6D8EA, 0x00DBC741);
		EquipItem(Pocket.Shoe, 17015, 0x00303133, 0x007BCDB7, 0x006E6565);

		AddGreeting(0, "I am Priestess <npcname/>. Nice to meet you.");
		AddGreeting(1, "Welcome to the Dunbarton church, <username/>.");
        
		AddPhrase("...");
		AddPhrase("I wish there was someone who could ring the bell on time...");
		AddPhrase("In the name of Lymilark...");
		AddPhrase("It's too much to go up and down these stairs to get here...");
		AddPhrase("The Church duties just keep coming. What should I do?");
		AddPhrase("The donations have decreased a little...");
		AddPhrase("There should be a message from the Pontiff's Office any day now.");
		AddPhrase("Why do these villagers obsess so much over their current lives?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Kristell.mp3");

		await Intro(
			"This priestess, in her neat Lymilark priestess robe, has eyes and hair the color of red wine.",
			"Gazing into the distance, she wears the tilted cross, a symbol of Lymilark, on her neck.",
			"She wears dangling earrings made of the same material which emanate a gentle glow."
		);

		Msg("Welcome to the Church of Lymilark.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				GiveKeyword("temple");
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("Guardian of Erinn... There's nothing wrong with someone like you<br/>being called that, <username/>.<br/>Thank you... For saving Erinn...");
				await Conversation();
				break;

			case "@shop":
				Msg("What is it that you are looking for?");
				OpenShop("KristellShop");
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
					Msg("Nice to meet you, <username/>.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "I am the priestess at this Church.<br/>Have you ever heard about Lord Lymilark's deep love and compassion towards humans?<br/>You probably have.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				GiveKeyword("shop_restaurant");
				Msg(FavorExpression(), "You can satisfy the hunger of the soul at the Church.<br/>For the hunger of the body, you should visit the Restaurant.<br/>Glenis' Restaurant is popular around here,<br/>so you should be able to find it easily.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_arbeit":
				Msg("Unimplemented");
				break;

			case "shop_misc":
				GiveKeyword("musicsheet");
				Msg("Looking for the General Shop?<br/>The General Shop is down this way.<br/>Go down to the Square from here<br/>and look for Walter.");
				Msg("It might be useful for you to know that<br/>the General Shop also carries music scores and instruments.");
				break;

			case "shop_grocery":
				Msg("A grocery store? We usually buy our ingredients at the Restaurant.<br/>Did you get on Glenis' bad side or something?");
				Msg("If not,<br/>why don't you just go there for the ingredients?");
				break;

			case "shop_healing":
				Msg("A Healer's House...<br/>You must be looking for Manus.<br/>Manus lives down south in the town.<br/>Go look for the healer sign around there.");
				Msg("Hmm. Consulting your Minimap<br/>or asking around would be<br/>a good way, too.");
				break;

			case "shop_inn":
				Msg("An inn? There is no inn in this town.<br/>Now that you mention it, it is rather strange.");
				break;

			case "shop_bank":
				Msg("If you are looking for a bank, go talk to Austeyn.<br/>To see him, walk down to the right end of the Square.<br/>He may seem easygoing, but he also catches everything..");
				Msg("That is to say, you can talk with him for only a short while and<br/>he will be able to read you like a book.");
				Msg("Lately, I have been thinking that<br/>it's something that comes from life experience.");
				break;

			case "shop_smith":
				Msg("It's the first time I've heard anyone asking about a blacksmith's shop in this town.<br/>If you are looking for weapons or armor,<br/>why don't you stop by Nerys' Weapons Shop?");
				break;

			case "skill_range":
				Msg("Lymilark once taught the following.");
				Msg("'Fighting begetteth fighting,<br/>and that fighting begetteth more fighting...<br/>Therefore, in harmony and understanding ye shall endeavor to live instead of fighting,<br/>embracing and loving one another instead of jealousy and envy.");
				Msg("Repeated fighting in the end shall be the ruin of all...'");
				Msg("Self-defense is important, but<br/>I hope that you do not focus so much on combat.");
				break;

			case "skill_instrument":
				Msg("Hmm. There is an instrument at the Church but,<br/>unfortunately, I am not very proficient at it yet so<br/>many people are helping me on that.");
				break;

			case "skill_composing":
				Msg("I have enough trouble simply reading the score and playing.<br/>Only those who are blessed can do such amazing things.");
				break;

			case "skill_tailoring":
				Msg("Why don't you go ask Simon at the Clothing Shop?<br/>I have yet to see someone<br/>who is as skilled as Simon.");
				break;

			case "skill_magnum_shot":
			case "skill_counter_attack":
			case "skill_smash":
				Msg("Lymilark our Lord gave us many teachings.<br/>Would you like to listen to one of them?");
				Msg("'Thou shalt not attack others.<br/>Ask thyself if anyone suffered wounds inflicted by thee<br/>without knowing and repent thy sins.");
				Msg("The wounded are also our Lord's beloved.<br/>How dare a mere being like thee<br/>attack thine brother and hurt him.");
				Msg("Reckon ye as a father the heart of God.'");
				Msg("Right now, you are interested only in fighting skills, but...<br/>I believe the time will come when you will ask yourself why,<br/>and repent your sins.");
				break;

			case "square":
				Msg("The Square is just over there.<br/>Go down the stairs and you will be right there.");
				break;

			case "farmland":
				Msg("The farmlands are just outside the town.<br/>Would you like to look around?");
				Msg("But please, do not go gleaning without permission.");
				break;

			case "shop_headman":
				Msg("We don't have a chief in our town.");
				break;

			case "temple":
				Msg("Yes, this is the Church<br/>where we worship Lymilark.");
				break;

			case "school":
				Msg("They teach combat skills and magic at the School.<br/>You will see it over the Square.");
				Msg("But I find it unfortunate that they neglect to<br/>instruct the teachings of Lymilark.");
				Msg("'Love is made possible only when your hearts<br/>are emptied and someone else fills them.<br/>Remember ye this word and endeavor to love.'");
				Msg("That is all I can say to you.");
				break;

			case "skill_campfire":
				Msg("Hmm. Discussing with someone<br/>who understands you about the love of the lord and the principles of this world<br/>around the campfire sounds like an interesting experience.");
				break;

			case "shop_restaurant":
				Msg("The Restaurant is just down there.<br/>It's near the General Shop, so you should be able to find it easily.<br/>Glenis owns the place so<br/>talk to her if you need anything.");
				Msg("She also sells cooking ingredients.<br/>And... it may be a Restaurant, but it is rather small<br/>so you may not be able to enter inside.");
				break;

			case "shop_armory":
				Msg("Nerys' Weapons Shop is near the<br/>south entrance.");
				Msg("Don't take it too personally if Nerys seems a little too aloof.<br/>She is just very shy around strangers.<br/>You will be fine once you get to know her a little.");
				break;

			case "shop_cloth":
				Msg("The Clothing Shop...<br/>You must be looking for Simon's shop.<br/>It is near the Square.");
				Msg("If you need clothes, make sure you pay a visit there.<br/>There are lots of decent clothes there.");
				Msg("Simon has made this robe for me for free.<br/>It is really light and comfortable.");
				Msg("If you get to go there,<br/>would you mind telling him I said hi?");
				break;

			case "shop_bookstore":
				Msg("A bookstore... You must be speaking of<br/>Aeira's Bookstore by that School over there.");
				Msg("Aeira may be young, but she is a deep thinker.<br/>From reading a lot of books, perhaps...?");
				Msg("Heehee. Still, she is a kid.<br/>She can't be compared to an adult.");
				break;

			case "shop_goverment_office":
				Msg("You are looking for Eavan, are you not?<br/>The Town Office is that large building you see down there.");
				Msg("It is fairly close by<br/>so you can get there in a few.");
				Msg("By the way... Eavan is the most popular girl in our town. Hehehe.");
				break;

			default:
				RndFavorMsg(
					"Well, it is news to me.",
					"I do not know such things very well. Ha ha.",
					"Hmm. I'm not sure.<br/>Why don't you ask someone else?",
					"You are very knowledgeable. I don't know much about that.",
					"I am sorry. I don't know much about it, so it's pointless to ask me.",
					"Oh... I thought it was a topic I knew about, but I suppose not. Pardon me.",
					"I don't really know... But if you find out more, will you please let me know?",
					"Asking about a topic like that to an<br/>ordinary Restaurant owner is not very proper, you know. Ha ha."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class KristellShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Gift", 52012); // Candlestick
		Add("Gift", 52024); // Bouquet
		Add("Gift", 52013); // Flowerpot
		Add("Gift", 52020); // Flowerpot
	}
}