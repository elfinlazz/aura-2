//--- Aura Script -----------------------------------------------------------
// Lassar, the Magic Instructor in Tir Chonaill
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class LassarBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_lassar");
		SetRace(10001);
		SetBody(height: 1.1f);
		SetFace(skinColor: 15, eyeType: 153, eyeColor: 25, mouthType: 2);
		SetStand("human/female/anim/female_natural_stand_npc_lassar02");
		SetLocation(9, 2020, 1537, 202);

		EquipItem(Pocket.Face, 3900, 0x0087C5EC, 0x00D5E029, 0x0001945D);
		EquipItem(Pocket.Hair, 3144, 0x00D25D5D, 0x00D25D5D, 0x00D25D5D);
		EquipItem(Pocket.Armor, 15657, 0x00394254, 0x00394254, 0x00574747);
		EquipItem(Pocket.Shoe, 17285, 0x00394254, 0x00000000, 0x00000000);
		EquipItem(Pocket.RightHand1, 40418, 0x00808080, 0x00000000, 0x00000000);
		EquipItem(Pocket.LeftHand1, 46023, 0x00808080, 0x00000000, 0x00000000);
		
		AddPhrase("....");
		AddPhrase("And I have to supervise an advancement test.");
		AddPhrase("Come to think of it, I have to come up with questions for the test.");
		AddPhrase("Funny, I had never thought I would do this kind of work when I was young.");
		AddPhrase("I hope I can go gather some herbs soon...");
		AddPhrase("I should put more clothes on. It's kind of cold.");
		AddPhrase("I think I'll wait a little longer...");
		AddPhrase("Is there any problem in my teaching method?");
		AddPhrase("Perhaps I could take today off...");
		AddPhrase("The weather will be fine for some time, it seems.");
		AddPhrase("Will things get better tomorrow, I wonder?");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Lassar.mp3");
		
		await Intro(
			"Waves of her red hair come down to her shoulders.",
			"Judging by her somewhat small stature, well-proportioned body, and a neat two-piece school uniform, it isn't hard to tell that she is a teacher.",
			"The intelligent look in her eyes, the clear lip line and eyebrows present her as a charming lady."
		);
		
		Msg("Is there anything I can help you with?", Button("Start a Conversation", "@talk"), Button("Shop" , "@shop"), Button("Repair Item" , "@repair"), Button("Upgrade item" , "@upgrade"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("We met before, right?<br/>Yes, I am Lassar.");
				await StartConversation();
				break;
				
			case "@shop":
				Msg("If you want to learn magic, you've come to the right place.");
				OpenShop("LassarShop");
				return;
				
			case "@repair":
				Msg("You want to repair a magic weapon?<br/>Don't ask Ferghus to repair magic weapons. Although he won't even do it..<br/>I can't imagine what would happen...if you tried to repair it like a regular weapon.<br/>");
				Msg("Unimplemented");
				break;
				
			case "@upgrade":
				Msg("You're looking to upgrade something?<br/>Hehe, how smart of you to come to a magic school teacher.<br/>Let me see what you're trying to upgrade.<br/>You know that the amount and type of upgrade available differs with each item, right?");
				Msg("Unimplemented");
				break;
		}
		
		End("Goodbye, Lassar. I'll see you later!");
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch(keyword)
		{
			case "personal_info":
				Msg("Lassar means 'flame'.<br/>My mother gave birth to me after having dreamed about a wildfire burning the field.<br/>");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "rumor":
				Msg("Farmland is just to the south of the School.<br/>They mainly grow wheat or barley, and the crop yields are enough<br/>for the people in Tir Chonaill.<br/>But I think there will be a shortage if travelers stay longer.");
				Msg("That means no stealing crops for you!");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "about_skill":
				Msg("You must be <username/> the so-called Elemental Master.<br/>How is your magic training going?<br/><title name='NONE'/>(You tell her about all the skills you've learned so far.)");
				Msg("Aha. You know the Smash skill.<br/>It seems to me that you have a great interest in combat magic...<br/>Is that why you're here?");
				Msg("Hmm. I am afraid I can't do that...<br/>Magic doesn't exist solely to kill.<br/>If you really are interested in such magic, then<br/>why don't you talk to Stewart?");
				Msg("Wait wait wait. Even if you can prove that you are an Elemental Master,<br/>you can't simply show up and tell him that you are interested in advanced magic.<br/>He probably wouldn't give you the time of day!");
				Msg("I'm only telling you this because it's you...<br/>But first, you must show Stewart the title of the Elemental Master.<br/>If you are really interested in magic-based combat, then<br/>you shouldn't have any trouble, eh?");
				Msg("Good luck...ha ha ha.");
				break;
				
			case "about_study":
				//Msg("So, you already know Icebolt, do you? Did you have any trouble using it?<br/>Since you know the basics already, I'll give you a tuition discount.<br/>Tuition is a lump sum that includes three days of lessons including today.<br/>This tuition covers up to the end of Basic Sorcery Chapter One.");
				//Msg("Class has started long ago.<br/>You should come back later.");
				Msg("Sorry, my classes aren't ready to be studied.");
				break;
				
			case "shop_misc":
				Msg("You're going to have to walk a long distance.<br/>Follow this road to the left, and you'll see the Square.<br/>The General Shop is around there.<br/>If you can't find it, ask other people near the Square.<br/>");
				break;
				
			case "shop_grocery":
				Msg("When the smell of Caitin's cooking reaches here,<br/>I sometimes wonder if it's a restaurant rather than a grocery store.<br/>The Grocery Store is located over the hill and just to your right.<br/>If you cannot find it, look at the Minimap.");
				Msg("Hee hee. Don't worry. It is nearby so you can't miss it.");
				break;
				
			case "shop_healing":
				Msg("Healer's House? You must be talking about Dilys' house.<br/>Pass the Square and go straight down the road.<br/>The healer, Dilys, and I have known each other for a long time.<br/>You can trust her.");
				Msg("But she seems like a quack sometimes, so be careful. Tee hee...");
				break;
				
			case "shop_bank":
				Msg("The Bank? Go up this way just a little and it's right there.<br/>It is not going to be easy talking with Bebhinn, but try to be patient with her... Hee hee.");
				Msg("By the way, I think you are supposed to pay interest to your deposit.<br/>It's weird that the Bank charges fees for deposits...<br/>What do you think?");
				break;
				
			case "shop_smith":
				Msg("If your equipment is destroyed<br/>or broken, you should pay a visit.<br/>Ferghus will help you.");
				break;
				
			case "skill_instrument":
				Msg("Have you forgotten?<br/>Endelyon knows about it.<br/>She's a priestess at Church.<br/>");
				break;
				
			case "skill_counter_attack":
				Msg("With that, Ranald over there is an expert.<br/>Go and talk with him,<br/>and you will get some useful information. Hmm...");
				Msg("(Why are so many people asking me<br/>about Martial Arts these days?)");
				break;
				
			case "square":
				Msg("No way! You must be kidding!<br/>Everyone knows where the Square is.<br/>Hee hee. Or are you flirting with me<br/>by trying to talk about anything?");
				break;
				
			case "farmland":
				Msg("If you go down the road a little, it will be all farmlands.<br/>Be careful not to hurt the crops.<br/>");
				break;
				
			case "windmill":
				Msg("The mill is near the Adelia Stream.<br/>My sister Alissa works there.");
				Msg("She is kind of stubborn, but she's about that age, you know? Hee hee.");
				Msg("Working at the mill can be deceptively stressful...<br/>The mill draws water from<br/>Adelia Stream to the reservoir.");
				Msg("Sometimes it grinds crops.<br/>It might be fun to go sometime<br/>and watch it in action.");
				break;
				
			case "brook":
				Msg("It is said that Adelia Stream is named<br/>after a priestess who lived here long ago...<br/>I wonder what kind of great work she's accomplished<br/>to have the stream named after her.");
				Msg("Well, what I mean is,<br/>even with the help of Lymilark,<br/>I cannot fully understand how the priestess,<br/>as a woman, managed to fight monsters.");
				Msg("Think about it. There were probably many healthy and robust warriors.<br/>Why would a delicate pacifist priestess<br/>have fought against such monsters?");
				break;
				
			case "shop_headman":
				Msg("You are looking for the Chief's House?<br/>The Chief's House is right over the hill from the Square.<br/>So you haven't been there but came here right away?");
				Msg("The Chief might be disappointed.<br/>You should go now!");
				break;
				
			case "temple":
				Msg("Hey! Church is just over there.<br/>You knew that already, didn't you?");
				Msg("I understand you want to<br/>keep flirting with me as long as you can, but...<br/>Hehehe...");
				break;
				
			case "school":
				Msg("*Laugh* You're right. This is the School.<br/>If you want to take a class,<br/>start a conversation<br/>by using the keyword [Class and Training].");
				break;
			
			case "shop_armory":
				Msg("Ranald has asked you to buy a weapon, right?<br/>But, to buy weapons in this town,<br/>you need to go to the Blacksmith's Shop.");
				break;
				
			case "shop_cloth":
				Msg("Hmm... You seem more clothing-savvy than you look.<br/>We just might get along really well... Hehehe...<br/>But this town is so rural<br/>that you need to go to the General Shop to buy clothing.");
				Msg("Hmm... It would be really nice if someone opened a boutique...");
				break;
				
			case "shop_bookstore":
				Msg("Someone like you looking for books? Now, that's unexpected.");
				Msg("Hahaha! I'm just kidding. Don't make that kind of face at me.<br/>There is no bookstore in this town.<br/>Not too many people here like reading.");
				Msg("How do I know?<br/>Actually, I sell magic books.<br/>And few people ever bought any.");
				break;
				
			case "shop_goverment_office":
				Msg("Huh? Did you say town office?<br/>Oh, you must have been to a city before.");
				Msg("Which one have you been in? Emain Macha? Tara?<br/>Or Dunbarton?<br/>Oh. Nothing, really.<br/>It's just that I myself studied in a city.");
				Msg("You know Emain Macha, don't you? Hee hee.");
				Msg("Dilys up there<br/>studied in the same city.<br/>You should go see her sometime. She is at the Healer's House.");
				break;
				
			case "lute":
				Msg("A lute?<br/>Malcolm at the General Shop sells lutes.<br/>I guess Priestess Endelyon talked to you,<br/>but you came the wrong way.");
				Msg("Go in the opposite direction and go toward the Square.<br/>A lute will be useful in many ways.");
				break;
				
			case "jewel":
				Msg("I love Topaz the most among all the gems<br/>because it feels cool to the touch,<br/>like the air during the Eweca hours.");
				break;
			
			default:
				RndMsg(					
					"Being a teacher doesn't necessarily mean knowing everything.",
					"Hmm... I don't know.",
					"Honestly, I don't know much.",
					"I thought I knew. But it is more difficult to actually explain it than I thought.",
					"Why don't you ask other people? I am afraid I would be of little help."				
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class LassarShop : NpcShopScript
{
	public override void Setup()
	{
		//--- Magic Items ----------------------------
		//--------------------------------------------
		Add("Magic Items", 63000);     //Phoenix Feather
		Add("Magic Items", 63000, 10); //Phoenix Feather
		Add("Magic Items", 63001);     //Wings of a Goddess
		Add("Magic Items", 63001, 5);  //Wings of a Goddess
		Add("Magic Items", 62012);     //Elemental Remover
		Add("Magic Items", 62003);     //Blessed Magic Powder
		Add("Magic Items", 62003, 10); //Blessed Magic Powder
		Add("Magic Items", 62001);     //Elite Magic Powder
		Add("Magic Items", 62014);     //Spirit Weapon Restoration Potion
		Add("Magic Items", 62001, 10); //Elite Magic Powder
		
		//--- Magic Book ----------------------------
		//-------------------------------------------
		Add("Magic Book", 1009);       //A Guidebook on Firebolt
		Add("Magic Book", 1008);       //Icebolt Spell: Origin and Training
		Add("Magic Book", 1007);       //Healing: The Basics of Magic
		
		//--- Magic Weapon --------------------------
		//-------------------------------------------
		Add("Magic Weapon", 40038);    //Lightning Wand
		Add("Magic Weapon", 40039);    //Ice Wand
		Add("Magic Weapon", 40040);    //Fire Wand
		Add("Magic Weapon", 40041);    //Combat Wand
		Add("Magic Weapon", 40090);    //Healing Wand
		Add("Magic Weapon", 40231);    //Crystal Lightning Wand
		Add("Magic Weapon", 40232);    //Crown Ice Wand
		Add("Magic Weapon", 40233);    //Phoenix Fire Wand
		Add("Magic Weapon", 40234);    //Tikka Wood Healing Wand
	}
}
