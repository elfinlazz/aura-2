//--- Aura Script -----------------------------------------------------------
// Edern
//--- Description -----------------------------------------------------------
// Blacksmith
//---------------------------------------------------------------------------

public class EdernScript : NpcScript
{
	public override void Load()
	{
		SetName("_edern");
		SetRace(10002);
		SetBody(height: 1.3f, weight: 1.4f, upper: 2f, lower: 1.4f);
		SetFace(skinColor: 25, eyeType: 9, eyeColor: 38, mouthType: 2);
		SetStand("human/male/anim/male_natural_stand_npc_edern");
		SetLocation(31, 10972, 13373, 76);

		EquipItem(Pocket.Face, 4904, 0x00F99D8B, 0x00F9E0EC, 0x009A1561);
		EquipItem(Pocket.Hair, 4028, 0x00C0BC92, 0x00C0BC92, 0x00C0BC92);
		EquipItem(Pocket.Armor, 15039, 0x009B5033, 0x009A835F, 0x00321007);
		EquipItem(Pocket.Glove, 16510, 0x00EABE7D, 0x00808080, 0x0057685E);
		EquipItem(Pocket.Shoe, 17504, 0x002B1C09, 0x00857756, 0x00321007);
		EquipItem(Pocket.RightHand1, 40024, 0x00FACB5F, 0x004F3C26, 0x00FAB052);

		AddGreeting(0, "Welcome! You look familiar.");

		AddPhrase("A true blacksmith never complains.");
		AddPhrase("Hahaha...");
		AddPhrase("Hey! Don't just stand there and make me nervous. If you've got something to say, say it!");
		AddPhrase("Hey, you! You there! Don't just snoop around. Come in!");
		AddPhrase("How I wish for a hard-working young man or woman to help...");
		AddPhrase("I hope Elen learns all my trade skills soon...");
		AddPhrase("I'll have to have some food first.");
		AddPhrase("Kids nowadays don't want to do hard work... Grrr...");
		AddPhrase("Let's see... Elen's mom was supposed to come in sometime.");
		AddPhrase("So many lazy kids. That's a problem.");
		AddPhrase("So many people underestimate blacksmith work.");
		AddPhrase("The town is lively as usual.");
		AddPhrase("Yes... This is the true scent of metal.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Edern.mp3");

		await Intro(
			"Between the long strands of white hair, you can see the wrinkles on his face and neck that show his old age.",
			"But his confidence and well-built torso with copper skin reveal that this man is anything but fragile.",
			"His eyes encompass both the passion of youth and the wisdom of old age.",
			"The thick brows that shoot upward with wrinkles add a fierce look, but his eyes are of soft amber tone."
		);

		Msg("You must have something to say to me.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"), Button("Modify Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Player.Titles.SelectedTitle == 11002)
					Msg("You got quite a name there...<br/>But you can't be satisfied with being the guardian of Erinn!<br/>It's good to think big.");
				await Conversation();
				break;

			case "@shop":
				Msg("Are you looking for something?<br/>If you're looking for normal equipment, talk to Elen.<br/>I only deal with special equipment that you can't find anywhere else.");
				OpenShop("EdernShop");
				return;

			case "@repair":
				Msg("If it's not urgent, would you mind talking to Elen?<br/>If it's something you particularly treasure, I can repair it myself.");
				Msg("Unimplemented");
				Msg("You can figure out a person by looking at his equipment.");
				break;

			case "@upgrade":
				Msg("Then give me the item to be modified.<br/>I ask this for your own good, but, while the weapons are not affected,<br/>armor that has been modified will be yours only. You know that, right?<br/>It won't fit anyone else.");
				Msg("Unimplemented");
				Msg("Then come back to me when you have something you want to modify.");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "My name is Edern. I am the blacksmith in this town.<br/>I own the Bangor Blacksmith's Shop.<br/>And you, who ask such obvious questions, are <username/>.");
				Msg("There are plenty of spaces to handle metal, so feel free to use the available space.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "Oh, that girl over there?<br/>That's Elen, my granddaughter.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				if (!HasSkill(SkillId.Blacksmithing, SkillRank.RF))
				{
					Msg("Seeing how you're somewhat familiar with the Blacksmith skill,<br/>I suppose I can tell you this much.");
					Msg("Being a blacksmith is more than hammering metal.<br/>The cardinal point is to learn how the metal<br/>reacts at what temperature.");
					Msg("Melting metal at the proper temperature,<br/>knowing when to hammer it and to put it in water,<br/>and understanding the nature of the metal.<br/>That's the Blacksmith skill.");
					Msg("Of course you wouldn't understand now.<br/>You have a more lot to learn to even understand<br/>the knowledge I have gained for so many years.");
					Msg("Still, it will do you good to remember what I've told you. Haha.");
				}
				else
				{
					Msg("Hmm... So you are getting the hang of it now? Blacksmith skill?");
					Msg("You seem rather cocky. Well, let me tell you something.");
					Msg("You are not even a hatchling in the world of blacksmiths.");
					Msg("An egg...<br/>To force the analogy,<br/>you are about the level of an egg.");
					Msg("Don't be too proud now with that little skill you have.<br/>It's only the beginning.");
					Msg("If you don't devote yourself, you will only amount to a half-baked blacksmith.");
				}
				break;

			case "about_arbeit":
				Msg("Unimplemented");
				break;

			case "shop_misc":
				Msg("A General Shop?<br/>Grrr! You mean that senile Gilmore's place, right?<br/>That old loser gets worse by the year.");
				break;

			case "shop_grocery":
				Msg("Well, you do have to have some food in your stomach to work.");
				Msg("Try the Pub. You can find many different kinds of food there along with drinks.");
				Msg("You are looking at me funny.<br/>That place was originally a grocery store.<br/>It only turned into a pub because people kept<br/>looking for drinks and ate there.");
				Msg("...");
				Msg("You are going to be in big trouble<br/>if you dare come here drunk without having eaten anything.");
				break;

			case "shop_healing":
				Msg("Are you alright?<br/>You are weaker than you look, friend.");
				Msg("There is no Healer's House in this town<br/>so it's best not to get hurt.");
				Msg("If you really need help, you have to travel to...<br/>Whatchama call it... Dunbarton.");
				break;

			case "shop_inn":
				Msg("We don't have an inn in this town.");
				break;

			case "shop_bank":
				Msg("Go talk to Bryce over there.<br/>You'll easily find him near the General Shop.");
				break;

			case "shop_smith":
				Msg("You are interested in the Refine skill?<br/>And that gives you the right to bother me with such basic questions, right?<br/>Wrong! Not only are you stupid, but greedy, too.<br/>Now, leave me alone and go ask Elen for basic skills.");
				Msg("Elen may be young, but she can hold her own as an apprentice blacksmith.");
				break;

			case "skill_rest":
				Msg("You must first learn to work,<br/>not to rest.");
				Msg("Rest is only as valuable as what you sweat.");
				break;

			case "skill_range":
				Msg("I only craft bows, so I don't know too much about that.<br/>Creating a powerful weapon that the user can comfortably use and<br/>effectively attacking a long range target are two different things.");
				Msg("When you get to my age, you'll learn not to speak<br/>rashly about something outisde of your expertise.<br/>It will do you good to remember that.");
				break;

			case "skill_instrument":
				Msg("If you're going to practice, please go to the dungeon.<br/>The work of a blacksmith has a flow and you're disrupting it.");
				break;

			case "skill_composing":
				Msg("What?");
				Msg("What a strange fellow.");
				Msg("If you're interested in Composition,<br/>you shouldn't be asking me.");
				break;

			case "skill_tailoring":
				Msg("You've got the wrong person for that.<br/>I make armor, not clothes.");
				Msg("It's a pity that a young fellow like you can lack so much sense.");
				break;

			case "skill_magnum_shot":
				Msg("I'm not interested in combat skills.");
				break;

			case "skill_counter_attack":
				Msg("I said, I'm not interested in martial arts.");
				break;

			case "skill_smash":
				Msg("I. Said. I'm. Not. Interested. In. Martial. Arts.");
				break;

			case "skill_gathering":
				Msg("What else would you need at the Blacksmith's Shop?");
				Msg("Mining Iron Ore from Barri Dungeon would<br/>help you make iron products.");
				Msg("Hey, friend. You aren't thinking of going bare-handed, are you?<br/>Talk to Elen and at least bring a Pickaxe with you.");
				break;

			case "square":
				Msg("So you like open spaces, do you?");
				Msg("Well, young men and women should learn to embrace nature.");
				break;

			case "pool":
				Msg("Seeing how you ask such questions, I assume you don't know this town's circumstances very well.<br/>In this town, water is a precious commodity.");
				Msg("Why else would we reuse the water that was drawn from<br/>a pit in a mine to refine metal?");
				Msg("Don't be so careless...<br/>Be a little more aware of your surroundings...");
				Msg("Think a little, you know?<br/>Young folks should know how to use their brains.");
				break;

			case "farmland":
				Msg("Your questions seem to be all about what this<br/>town is lacking. I don't find it pleasant.");
				break;

			case "brook":
				Msg("What makes you go about asking for the name of a stream in Tir Chonaill?");
				Msg("Hmm... Perhaps you...");
				Msg("Ha. Is it because of Ferghus?<br/>He did study under my tutelage years ago.");
				Msg("I heard on several occasions that he had started a Blacksmith's Shop in front of the Adelia Stream.<br/>But that clumsy kid probably only ruined the equipment of his customers.");
				break;

			case "shop_headman":
				Msg("We don't have such a thing here.");
				break;

			case "temple":
				Msg("It's been a long time since the church burned down.");
				break;

			case "school":
				Msg("We don't have that many kids.");
				break;

			case "skill_campfire":
				Msg("I know this town is close to fire, but<br/>you shouldn't go about building fire just anywhere.");
				Msg("The Church and the Bank both went down in some careless play with fires.");
				break;

			case "shop_restaurant":
				Msg("You must need food.<br/>This town has food at the Pub. Why don't you go there?");
				Msg("It's right over there.");
				break;

			case "shop_armory":
				Msg("If you want to purchase anything,<br/>talk to Elen next to me.");
				Msg("I'm busier than I look.");
				break;

			case "shop_cloth":
				Msg("I see that you are into decorating yourself, but<br/>the only useful clothing in this town is armor.");
				break;

			case "shop_bookstore":
				Msg("Books? Ha!");
				Msg("What good would they do?");
				Msg("The heart of metalworking cannot be gained by reading mere books.");
				break;

			case "shop_goverment_office":
				Msg("What are you talking about?");
				break;

			default:
				RndFavorMsg(
					"And why are you asking ME?",
					"You have strange interests.",
					"What are you even talking about?",
					"Why would you want to know about something like that?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class EdernShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Weapon", 40078); // Bipennis
		Add("Weapon", 40079); // Mace
		Add("Weapon", 40080); // Trudy Hunting Suit
		Add("Weapon", 40081); // Leather Long Bow
		Add("Weapon", 40404); // Physis Wooden Lance

		Add("Advanced Weapon"); // Empty?

		Add("Armor", 13043); // Leminia's Holy Moon Armor (M)
		Add("Armor", 13044); // Leminia's Holy Moon Armor (F)
		Add("Armor", 13045); // Arish Ashuvain Armor (M)
		Add("Armor", 13046); // Arish Ashuvain Armor (F)
		Add("Armor", 13047); // Kirinusjin's Half-plate Armor (M)
		Add("Armor", 13048); // Kirinusjin's Half-plate Armor (F)
		Add("Armor", 16525); // Arish Ashuvain Gauntlet
		Add("Armor", 17518); // Arish Ashuvain Boots (M)
		Add("Armor", 17519); // Arish Ashuvain Boots (F)
	}
}
