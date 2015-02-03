//--- Aura Script -----------------------------------------------------------
// Nerys
//--- Description -----------------------------------------------------------
// Weapon and Armor Store Owner
//---------------------------------------------------------------------------

public class NerysScript : NpcScript
{
	public override void Load()
	{
		SetName("_nerys");
		SetRace(10001);
		SetBody(height: 0.9f);
		SetFace(skinColor: 16, eyeType: 4, eyeColor: 31);
		SetStand("human/female/anim/female_natural_stand_npc_Nerys");
		SetLocation(14, 44229, 35842, 139);

		EquipItem(Pocket.Face, 3900, 0x00EBAE4E, 0x00005A33, 0x00F36246);
		EquipItem(Pocket.Hair, 3023, 0x00994433, 0x00994433, 0x00994433);
		EquipItem(Pocket.Armor, 15043, 0x0094C1C5, 0x006C9D9A, 0x00BE8C92);
		EquipItem(Pocket.Glove, 16008, 0x00818775, 0x00117C7D, 0x0000A3DC);
		EquipItem(Pocket.Shoe, 17001, 0x00823021, 0x0082C991, 0x00F2597B);

		AddGreeting(0, "What are you looking for?");
		AddGreeting(1, "Wait... I think I've met you once before...<br/>If not, sorry.");

		AddPhrase("There are so many weapon repair requests this month.");
		AddPhrase("That fellow... I like the look in his eyes.");
		AddPhrase("Wait, I shouldn't be doing this right now.");
		AddPhrase("I should have gone on the trip myself...");
		AddPhrase("Do you need something else?");
		AddPhrase("Wow. Time flies...");
		AddPhrase("At this rate, I won't have enough arrows next month...");
		AddPhrase("Manus is showing off his muscles again...");
		AddPhrase("This way, people. This way.");
		AddPhrase("See something you like?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Nerys.mp3");

		await Intro(
			"This lady has a slender build and wears comfortable clothing.",
			"The subtle softness of her short red hair is brought out by being tightly combed back.",
			"Thick ruby earrings matching her hair dangle from her ears and",
			"slightly waver and glitter every time she looks up."
		);

		Msg("Tell me if you need anything.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"), Button("Modify Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("...Guardian of Erinn...?<br/>Well, as long as you didn't break anything, I guess that's a good thing.<br/>Anyway, good job.");
				await Conversation();
				break;

			case "@shop":
				Msg("You brought your money with you, right?<br/>");
				OpenShop("NerysShop");
				return;

			case "@repair":
				Msg("You can repair weapons, armor, and equipment here.<br/>I use expensive repair tools, so the fee is fairly high. Is that okay with you?<br/>I do make fewer mistakes because of that, though."); // <repair rate='95' stringid='(*/smith_repairable/*)' />");
				Msg("Unimplemented");
				Msg("If the repair fee is too much for you,<br/>try using some Holy Water of Lymilark.<br/>It should be a big help.<br/>Now, come again if there's anything that needs to be repaired."); // <repair hide='true'/>
				break;

			case "@upgrade":
				Msg("Modification? Pick an item.<br/>I don't have to explain to you about<br/>the number of possible modification and the types, do I?"); // <upgrade />
				Msg("Unimplemented");
				Msg("Is that all for today?<br/>Well, come back anytime you need me."); // <br/><upgrade hide='true'/>
				break;
		}
		
		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("shop_armory");
				if (Memory == 1)
				{
					Msg("I'm <npcname/>, the owner of this Weapons Shop. Nice to meet you.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "Aren't you here to buy weapons?<br/>Well, I guess having a chat buddy doesn't hurt.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				Msg(FavorExpression(), "When going to a nearby dungeon, you must be adequately equipped with weapons.<br/>If not, you'll face a world of trouble.<br/>Try visiting Walter's General Shop or that idiot healer, Manus' place.");
				Msg("Oh, and take your time talking to Aranwen at the School.<br/>She's rather irritable and high-flown,<br/>so she doesn't like to teach skills to those who don't have the basics down.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("Yeah, <username/>? You have something to ask me about skills?");
				Msg("The only skill I know has to do with repairing items.<br/>It's not an easy skill to teach, though.<br/>But, if you're asking, <username/>, I suppose I could tell you.");
				Msg("Okay, are you interested at all in the advanced skills that archers use?<br/>I have heard that Aranwen at the School teaches<br/>a skill that allows you to quickly load your arrows and shoot successively.<br/>If you're interested, it would probably do you good.");
				Msg("But Aranwen tends to be a little irritable.<br/>She probably won't teach it to you simply by asking about skills.<br/>Knowing her, if you are to learn the advanced archery skills,<br/>you'll have to be fairly proficient at the basic archery skills first.");
				Msg("And showing the appeal is a different matter altogether.<br/>Let's see... How could you do that without making it awkward?");
				Msg("....Right. Try putting on the Fire Arrow title.");
				Msg("It's inside information I told you,<br/>so make sure you do as I say.<br/>Fire Arrows and bow proficiency. Don't forget.");
				break;

			case "about_arbeit":
				Msg("Unimplemented");
				break;

			case "shop_misc":
				Msg("If you mean Walter's General Shop, go to the Square.<br/>This way is by the south entrance.<br/>I don't need to explain to you how to use the Minimap, do I?");
				break;

			case "shop_grocery":
				Msg("The Grocery Store? You mean the Restaurant.");
				break;

			case "shop_healing":
				Msg("You are in the right place. It's just over there.<br/>When you go in there, tell that<br/>muscle head Manus that I directed you there.");
				break;

			case "shop_inn":
				Msg("An inn? I've never heard of an inn here before.<br/>Are you sure you know what you're talking about?");
				break;

			case "shop_bank":
				Msg("Austeyn is in the building southeast of the Square.<br/>Erskin Banks all look similar everywhere so it shouldn't be too hard to find it.");
				break;

			case "shop_smith":
				Msg("Hmm... This town doesn't have a Blacksmith's Shop.<br/>But we do simple repairs here<br/>so you can leave your items with me.");
				break;

			case "skill_range":
				Msg("There is a variety of long range attacks.<br/>You can use a bow,<br/>cast a spell,<br/>or throw spears.");
				Msg("So, is that what you're into?<br/>Why don't you buy a bow for now?");
				break;

			case "skill_instrument":
				Msg("You are more romantic that I thought,<br/>asking me a question like that.<br/>Unfortunately, I'm not interested in it.");
				break;

			case "skill_composing":
				Msg("Ha ha.<br/>I think Aeira just brought in some books that covers it.<br/>If you want one, why don't you get a copy?");
				break;

			case "skill_tailoring":
				Msg("I don't know. I often buy clothes from Simon's Clothing Shop<br/>rather than making them on my own.<br/>Perhaps Simon knows more about this?");
				break;

			case "skill_magnum_shot":
				Msg("Don't you think you need to use the bow a little first?");
				break;

			case "skill_counter_attack":
			case "skill_smash":
				Msg("Aranwen should know skills like that.<br/>She's rather irritable, though, so I'm not sure if she'll teach you.<br/>Anyhow, go see her at the School.");
				break;

			case "square":
				Msg("Follow this road into the town.<br/>The town center is the Square, so you should be able to find it easily.");
				break;

			case "farmland":
				Msg("Everything around here is all farm.<br/>I guess you haven't been outside yet.<br/>People are concerned because there have been<br/>an abnormally huge group of large rats attacking the area.");
				break;

			case "shop_headman":
				Msg("Ha ha. A chief in this kind town?<br/>Don't you find that a little funny?");
				break;

			case "temple":
				Msg("If you want to see that holier-than-thou Kristell, you have to go straight northwest.<br/>She's on the exact opposite side of the town from here.<br/>You may find it rough going up there, so just make sure you're prepared.");
				break;

			case "school":
				Msg("Follow this road up north just a little bit.");
				break;

			case "skill_windmill":
				Msg("Is that a type of combat?<br/>You should go speak to Aranwen at the School first.<br/>She might not teach you, so don't get your hopes up too high.");
				break;

			case "skill_campfire":
				Msg("Hmm... I'm not sure there's anyone in this town<br/>who knows such a skill.");
				break;

			case "shop_restaurant":
				Msg("You mean Glenis' Restaurant, right?<br/>That's around the corner of the alley, northwest of the Square.<br/>Well, they sell cooking ingredients, too.");
				break;

			case "shop_armory":
				Msg("This is the Weapons Shop.<br/>What? Do you want to go somewhere else?");
				break;

			case "shop_cloth":
				Msg("It's between the Bank and the General Shop.<br/>There are some pretty decent clothes there, so stop by.");
				break;

			case "shop_bookstore":
				Msg("Follow this road up north<br/>and go into the alley just before you reach the north entrance.<br/>You'll see a girl wearing glasses.<br/>That's the Bookstore.");
				break;

			case "shop_goverment_office":
				Msg("Hmm, the Town Office...<br/>It's the building made out of stone located at the north of the Square.<br/>You probably can't go inside the Town Office...");
				Msg("But Eavan works at the registration.<br/>If you have any business there, she's the one to talk to.");
				break;

			default:
				RndFavorMsg(
					"I'm not sure.",
					"You are rather grumpy.",
					"Someone else probably knows.",
					"I don't know. It's news to me.",
					"Don't ask me about stuff like that.",
					"Hmm. You like to ask strange questions, don't you?",
					"Well... I think I might have heard something about that.",
					"People ask me about that a lot...<br/>But I'm not interested in that at all."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class NerysShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Weapon", 45001, 20);  // Arrow (20)
		Add("Weapon", 45001, 100); // Arrow (100)
		Add("Weapon", 40023);      // Gathering Knife
		Add("Weapon", 40022);      // Gathering Axe
		Add("Weapon", 45002, 50);  // Bolt (20)
		Add("Weapon", 45002, 100); // Bolt (100)
		Add("Weapon", 40027);      // Weeding Hoe
		Add("Weapon", 40003);      // Shortbow
		Add("Weapon", 40006);      // Dagger
		Add("Weapon", 40179);      // Spiked Knuckle
		Add("Weapon", 40243);      // Battle Short Sword
		Add("Weapon", 40013);      // Longbow
		Add("Weapon", 40015);      // Fluted Short Sword
		Add("Weapon", 40014);      // Composite Bow
		Add("Weapon", 40010);      // Longsword
		Add("Weapon", 40016);      // Warhammer
		Add("Weapon", 40244);      // Bear Knuckle
		Add("Weapon", 40011);      // Broadsword
		Add("Weapon", 40180);      // Hobnail Knuckle
		Add("Weapon", 40424);      // Battle Sword
		Add("Weapon", 40031);      // Crossbow
		Add("Weapon", 40745);      // Basic Control Bar
		Add("Weapon", 40841);      // Shuriken
		Add("Weapon", 40404);      // Physis Wooden Lance
		Add("Weapon", 46001);      // Round Shield
		Add("Weapon", 46006);      // Kite Shield

		Add("Shoes && Gloves", 16009); // Tork's Hunter Gloves
		Add("Shoes && Gloves", 16005); // Wood Plate Cannon
		Add("Shoes && Gloves", 16017); // Standard Gloves
		Add("Shoes && Gloves", 17506); // Long Greaves
		Add("Shoes && Gloves", 16501); // Leather Protector
		Add("Shoes && Gloves", 17501); // Solleret Shoes
		Add("Shoes && Gloves", 16500); // Ulna Protector Gloves
		Add("Shoes && Gloves", 17500); // High Polean Plate Boots
		Add("Shoes && Gloves", 16504); // Counter Gauntlet
		Add("Shoes && Gloves", 16505); // Fluted Gauntlet

		Add("Helmet", 18513); // Spiked Cap
		Add("Helmet", 18500); // Ring Mail Helm
		Add("Helmet", 18504); // Cross Full Helm
		Add("Helmet", 18502); // Bone Helm
		Add("Helmet", 18501); // Guardian Helm
		Add("Helmet", 18506); // Wing Half Helm
		Add("Helmet", 18508); // Slit Full Helm
		Add("Helmet", 18505); // Spiked Helm
		Add("Helmet", 18509); // Bascinet
		Add("Helmet", 18525); // Waterdrop Cap
		Add("Helmet", 18522); // Pelican Protector
		Add("Helmet", 18520); // Steel Headgear
		Add("Helmet", 18521); // European Comb
		Add("Helmet", 18515); // Twin Horn Cap
		Add("Helmet", 18524); // Four Wings Cap
		Add("Helmet", 18519); // Panache Head Protector

		Add("Armor", 14006); // Linen Cuirass (F)
		Add("Armor", 14009); // Linen Cuirass (M)
		Add("Armor", 14007); // Padded Armor with Breastplate
		Add("Armor", 14013); // Lorica Segmentata
		Add("Armor", 14005); // Drandos Leather Mail (F)
		Add("Armor", 14011); // Drandos Leather Mail (M)
		Add("Armor", 14017); // Three-Belt Leather Mail
		Add("Armor", 14016); // Cross Belt Leather Coat
		Add("Armor", 13017); // Surcoat Chain Mail
		Add("Armor", 13001); // Melka Chain Mail
		Add("Armor", 13010); // Round Pauldron Chainmail

		Add("Event"); // Empty

		Add("Arrowheads", 64011); // Bundle of Arrowheads
		Add("Arrowheads", 64015); // Bundle of Bolt Heads
		Add("Arrowheads", 64013); // Bundle of Fine Arrowheads
		Add("Arrowheads", 64016); // Bundle of Fine Bolt Heads
		Add("Arrowheads", 64014); // Bundle of the Finest Arrowheads
		Add("Arrowheads", 64017); // Bundle of the Finest Bolt Heads
	}
}