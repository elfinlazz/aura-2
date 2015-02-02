//--- Aura Script -----------------------------------------------------------
// Aranwen
//--- Description -----------------------------------------------------------
// Teacher
//---------------------------------------------------------------------------

public class AranwenScript : NpcScript
{
	public override void Load()
	{
		SetName("_aranwen");
		SetRace(10001);
		SetBody(height: 1.15f, weight: 0.9f, upper: 1.1f, lower: 0.8f);
		SetFace(skinColor: 15, eyeType: 3, eyeColor: 192);
		SetLocation(14, 43378, 40048, 125);

		EquipItem(Pocket.Face, 3900, 0x00344300, 0x0000163E, 0x008B0021);
		EquipItem(Pocket.Hair, 3026, 0x00BDC2E5, 0x00BDC2E5, 0x00BDC2E5);
		EquipItem(Pocket.Armor, 13008, 0x00C6D8EA, 0x00C6D8EA, 0x00635985);
		EquipItem(Pocket.Glove, 16503, 0x00C6D8EA, 0x00B20859, 0x00A7131C);
		EquipItem(Pocket.Shoe, 17504, 0x00C6D8EA, 0x00C6D8EA, 0x003F6577);
		EquipItem(Pocket.RightHand1, 40012, 0x00C0C0C0, 0x008C84A4, 0x00403C47);

		AddGreeting(0, "Yes? Please don't block my view.");
		AddGreeting(1, "Hmm. <username/>, right?<br/>Of course.");
        
		AddPhrase("...");
		AddPhrase("A sword does not betray its own will.");
		AddPhrase("A sword is not a stick. I don't feel any tension from you!");
		AddPhrase("Aren't you well?");
		AddPhrase("Focus when you're practicing.");
		AddPhrase("Hahaha.");
		AddPhrase("If you're done resting, let's keep practicing!");
		AddPhrase("It's those people who really need to learn swordsmanship.");
		AddPhrase("Put more into the wrists!");
		AddPhrase("That student may need to rest a while.");
	}
    
	protected override async Task Talk()
	{
		SetBgm("NPC_Aranwen.mp3");

		await Intro(
			"A lady decked out in shining armor is confidently training students in swordsmanship in front of the school.",
			"Unlike a typical swordswoman, her moves seem delicate and elegant.",
			"Her long, braided silver hair falls down her back, leaving her eyes sternly fixed on me."
		);

		Msg("What brings you here?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"));

		switch (await Select()) 
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
				{
					Msg("Guardian of Erinn...<br/>If it were anyone else,<br/>I would tell them to stop being so arrogant...");
					Msg("But with you, <username/>, you are definitely qualified.<br/>Good job.");
				}
				await Conversation();
				break;

			case "@shop":
				Msg("Are you looking for a party quest scroll?");
				OpenShop("AranwenShop");
				return;

			case "@upgrade":
				Msg("Please select the weapon you'd like to modify.<br/>Each weapon can be modified according to its kind.<upgrade />");
				Msg("Unimplemented");
				Msg("A bow is weaker than a crossbow?<br/>That's because you don't know a bow very well.<br/>Crossbows are advanced weapons for sure,<br/>but a weapon that reflects your strength and senses is closer to nature than machinery.");
				break;
		}
		
		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword) 
	{
		switch (keyword) {
			case "personal_info":
				GiveKeyword("school");
				if (Memory == 1)
				{
					Msg("Let me introduce myself.<br/>My name is <npcname/>. I teach combat skills at the Dunbarton School.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "If you are looking to learn combat arts, it's probably better<br/>to talk about classes or training rather than hold personal conversations.<br/>But then, I suppose there is lots to learn in this town other than combat skills.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				GiveKeyword("shop_armory");
				Msg(FavorExpression(), "If you need a weapon for the training,<br/>why don't you go see Nerys in the south side?<br/>She runs the Weapons Shop.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("...I am sorry, but someone that has yet to master the skill<br/>should not be bluntly asking questions about skills like this.");
				Msg("...if you are interested in high-leveled bowman skills, then<br/>you should at least master the Fire Arrow skill first.");
				break;

			case "shop_misc":
				Msg("Hmm. Looking for the General Shop?<br/>You'll find it down there across the Square.");
				Msg("Walter should be standing by the door.<br/>You can buy instruments, music scores, gifts, and tailoring goods such as sewing patterns.");
				break;

			case "shop_grocery":
				Msg("If you are looking to buy cooking ingredients,<br/>the Restaurant will be your best bet.");
				break;

			case "shop_healing":
				Msg("A Healer's House? Are you looking for Manus?<br/>Manus runs a Healer's House near<br/>the Weapons Shop in the southern part of town.");
				Msg("Even if you're not ill<br/>and you're simply looking for things like potions,<br/>that's the place to go.");
				break;

			case "shop_inn":
				Msg("There is no inn in this town.");
				break;

			case "shop_bank":
				Msg("If you're looking for a bank, you can go to<br/>the Erskin Bank in the west end of the Square.<br/>Talk to Austeyn there for anything involving money or items.");
				break;

			case "shop_smith":
				Msg("There is no blacksmith's shop in this town, but<br/>if you are looking for anything like weapons or armor,<br/>why don't you head south and visit the Weapons Shop?");
				break;

			case "skill_range":
				Msg("I suppose I could take my time and verbally explain it to you,<br/>but you should be able to quickly get the hang of it<br/>once you equip and use a bow a few times.");
				break;

			case "skill_tailoring":
				Msg("It would be most logical to get Simon's help<br/>at the Clothing Shop.");
				break;

			case "skill_magnum_shot":
				Msg("Magnum Shot?<br/>Haven't you learned such a basic skill alrerady?<br/>You must seriously lack training.");
				Msg("It may be rather time-consuming, but<br/>please go back to Tir Chonaill.<br/>Ranald will teach you the skill.");
				break;

			case "skill_counter_attack":
				Msg("If you don't know the Counterattack skill yet, that is definitely a problem.<br/>Very well. First, you'll need to fight a powerful monster and get hit by its Counterattack.");
				Msg("Monsters like bears use Counterattack<br/>so watch how they use it and take a hit,<br/>and you should be able to quickly get the hang of it without any particular training.");
				Msg("In fact, if you are not willing to take the hit,<br/>there is no other way to learn that skill.<br/>Simply reading books will not help.");
				break;

			case "skill_smash":
				Msg("Smash...<br/>For the Smash skill, why don't you go to the Bookstore and<br/>look for a book on it?");
				Msg("You should learn it by yourself before bothering<br/>people with questions.<br/>You should be ashamed of yourself.");
				break;

			case "square":
				Msg("The Square is just over here.<br/>Perhaps it totally escaped you<br/>because it's so large.");
				break;

			case "farmland":
				Msg("Strangely, large rats have been seen<br/>in large numbers in the farmlands recently.<br/>This obviously isn't normal.");
				Msg("If you are willing,<br/>would you go and take some out?<br/>You'll be appreciated by many.");
				break;

			case "brook":
				Msg("Adelia Stream...<br/>I believe you're speaking of the<br/>stream in Tir Chonaill...");
				Msg("Shouldn't you be asking<br/>these questions<br/>in Tir Chonaill?");
				break;

			case "shop_headman":
				Msg("A chief?<br/>This town is ruled by a Lord,<br/>so there is no such person as a chief here.");
				break;

			case "temple":
				Msg("You must have something to discuss with Priestess Kristell.<br/>You'll find her at the Church up north.");
				Msg("You can also take the stairs that head<br/>northwest to the Square.<br/>There are other ways to get there, too,<br/>so it shouldn't be too difficult to find it.");
				break;

			case "school":
				Msg("Mmm? This is the only school around here.");
				break;

			case "skill_windmill":
				RemoveKeyword("skill_windmill");
				Msg("Are you curious about the Windmill skill?<br/>It is a useful skill to have when you're surrounded by enemies.<br/>Very well. I will teach you the Windmill skill.");
				break;

			case "shop_restaurant":
				Msg("If you're looking for a restaurant, you are looking for Glenis' place.<br/>She not only sells food, but also a lot of cooking ingredients, so<br/>you should pay a visit if you need something.");
				Msg("The Restaurant is in the north alley of the Square.");
				break;

			case "shop_armory":
				Msg("Nerys is the owner of the Weapons Shop.<br/>Keep following the road that leads down south<br/>and you'll see her mending weapons outside.");
				Msg("She may seem a little aloof,<br/>but don't let that get to you too much<br/>and you'll get used to it.");
				break;

			case "shop_cloth":
				Msg("There is no decent clothing shop in this town...<br/>But, if you must, go visit Simon's place.<br/>You should be able to find something that fits right away.");
				break;

			case "shop_bookstore":
				Msg("You mean Aeira's Bookstore.<br/>It's just around here.<br/>Follow the road in front of the school up north.");
				Msg("Many types of books go through that place,<br/>so even if you don't find what you want right away,<br/>keep visiting and you'll soon get it.");
				break;

			case "shop_goverment_office":
				Msg("Are you looking for Eavan?<br/>The Lord and the Captain of the Royal Guards<br/>are very hard to reach. ");
				Msg("If you're really looking for Eavan,<br/>go over to that large building to the north of the Square.");
				break;

			default:
				RndFavorMsg(
					"Will you tell me about it when you find out more?",
					"Being a teacher doesn't mean that I know everything.",
					"Hey! Asking me about such things is a waste of time.",
					"I don't know anything about it. Why don't you ask others?",
					"I don't know too much about anything other than combat skills.",
					"I don't know anything about it. I'm sorry I can't be much help.",
					"It doesn't seem bad but... I don't think I can help you with it.",
					"If you keep bringing up topics like this, I can't say much to you."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class AranwenShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Party Quest", 70025); // Party Quest Scroll - 10 Red Bears
		Add("Party Quest", 70025); // Party Quest Scroll - 30 Red Bears
		Add("Party Quest", 70025); // Party Quest Scroll - 30 Brown Grizzly Bears
		Add("Party Quest", 70025); // Party Quest Scroll - 30 Red Grizzly Bears
		Add("Party Quest", 70025); // Party Quest Scroll - 30 Black Grizzly Bears
		Add("Party Quest", 70025); // Party Quest Scroll - 10 Black Grizzly Bears and 10 Brown Grizzly Bears
		Add("Party Quest", 70025); // Party Quest Scroll - 15 Black Grizzly Bear Cubs and Brown Grizzly Bear Cubs
		Add("Party Quest", 70025); // Party Quest Scroll - 10 Red Grizzly Bears and 10 Brown Grizzly Bears
		Add("Party Quest", 70025); // Party Quest Scroll - 15 Red Grizzly Bear Cubs and Brown Grizzly Bear Cubs
	}
}