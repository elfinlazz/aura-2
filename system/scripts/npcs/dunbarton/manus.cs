//--- Aura Script -----------------------------------------------------------
// Manus
//--- Description -----------------------------------------------------------
// Healer
//---------------------------------------------------------------------------

public class ManusScript : NpcScript
{
	public override void Load()
	{
		SetName("_manus");
		SetRace(25);
		SetFace(skinColor: 27, eyeType: 12, eyeColor: 27, mouthType: 18);
		SetLocation(19, 881, 1194, 0);

		EquipItem(Pocket.Face, 4900, 0x003C3161, 0x00737F39, 0x00856430);
		EquipItem(Pocket.Hair, 4096, 0x002B2822, 0x002B2822, 0x002B2822);
		EquipItem(Pocket.Armor, 15030, 0x00CFD0B5, 0x00006600, 0x00006600);
		EquipItem(Pocket.Shoe, 17035, 0x00223846, 0x00574662, 0x00808080);

		AddGreeting(0, "You look familiar. Haven't we met before?");
		AddGreeting(1, "I seem to be seeing you often.");
        
		AddPhrase("A healthy body for a healthy mind!");
		AddPhrase("Alright! Here we go! Woo-hoo!");
		AddPhrase("Come! A special potion concocted by Manus for sale now!");
		AddPhrase("Here, let's have a look.");
		AddPhrase("I wish there was something I could spend this extra energy on...");
		AddPhrase("Perhaps Stewart could tell me about this...");
		AddPhrase("There's nothing like a massage for relief when your muscles are tight! Hahaha!");
		AddPhrase("Why did you let it go this bad?!");
		AddPhrase("You should exercise more. You're so thin.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Manus.mp3");

		await Intro(
			"This man is wearing a green and white healer's dress.",
			"His thick, dark hair is immaculately combed and reaches down to his neck,",
			"his straight bangs accentuating a strong jaw and prominent cheekbones."
		);

		Msg("Ha! Tell me everything you need!", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Get Treatment", "@healerscare"), Button("Heal Pet", "@petheal"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("Wow, what a title!<br/><username/>, I feel like<br/>I need to treat you differently. Haha!");
				await Conversation();
				break;

			case "@shop":
				Msg("Is there something I can help you with?");
				OpenShop("ManusShop");
				return;

			case "@healerscare":
				if (Player.Life == Player.LifeMax)
				{
					Msg("Huh? What's this? You are fine. What do you mean you need treatment?<br/>Foot fungus, by any chance? Hahaha!");
				}
				else
				{
					Msg("My, how did you manage to get hurt this badly?! We'll have to bandage it right now!<br/>Oh, but don't forget that you have to pay the fee. It's 90 Gold.", Button("Receive Treatment", "@gethealing"), Button("Decline", "@cancel"));
					if(await Select() == "@gethealing")
					{
						if (Gold >= 90)
						{
							Gold -= 90;
							Player.FullLifeHeal();
							Msg("There, how do you like my skills?! Don't be so careless with your body.");
						}
						else
						{
							Msg("Gee, that's not enough money.<br/>I may be generous, but I can't do this for free.");
						}
					}
				}
				break;

			case "@petheal":
				if (Player.Pet == null)
				{
					Msg("You'll need to show me your pet first before I can diagnose it.<br/>Don't you think so?");
				}
				else if (Player.Pet.IsDead)
				{
					Msg("Your pet is already knocked unconscious! Revive it first, immediately.");
				}
				else if (Player.Pet.Life == Player.Pet.LifeMax)
				{
					Msg("What? Your pet seems perfectly fine. Why would it need to be treated?");
				}
				else
				{
					Msg("How did you get your pet to be hurt this badly?! I'll treat it right now!<br/>By the way, it will cost you 180 Gold. Don't forget that.", Button("Recieve Treatment", "@recieveheal"), Button("Decline the Treatment", "@end"));
					if(await Select() == "@recieveheal")
					{
						if (Gold < 180)
						{
							Msg("Hmmm...I think you are short.<br/>I may be a generous person, but I can't do business like this... for free...");
						}
						else
						{
							Gold -= 180;
							Player.Pet.FullLifeHeal();
							Msg("Your pet is fixed, and ready to go! Take care of your pet as much as you'd take care of yourself.");
						}
					}
				}
				break;
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
					Msg("My name is <npcname/>.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					GiveKeyword("shop_healing");
					Msg(FavorExpression(), "I am the healer in this town. I'm good at what I do,<br/>so feel free to come by if you get sick.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				GiveKeyword("shop_restaurant");
				Msg(FavorExpression(), "Have you been to Glenis' Restaurant yet?<br/>Make sure you pay a visit and order something.<br/>Eating well is the most important thing in maintaining good health. Hahaha!");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				// If player has resting skill...
				Msg("Oh, I see that you know the Resting skill. Do you find it handy?<br/>If you want, I could raise the skill level for you by one.");
				Msg("Ah! Ah! Of course, it comes at a price.<br/>Hahah... Let's see...");
				Msg("I know! I always need more help, so why don't you do some work around here?<br/>It doesn't have to be with me. You can help any healer in any town.<br/>I'll raise the skill level for you if you do some healer part-time work. Any questions?");
				break;

			case "about_arbeit":
				Msg("Unimplemented");
				break;

			case "shop_misc":
				Msg("Hmm. Don't tell me you really don't know.<br/>I saw it on your Minimap a while ago. You will find Walter there.<br/>I don't know what else to tell you.");
				Msg("Yes, it's the General Shop, that's why Walter sells general goods.<br/>I know it's obvious. Haha.");
				break;

			case "shop_grocery":
				Msg("The Grocery Store...? You mean the Restaurant.");
				break;

			case "shop_healing":
				Msg("You're right there.");
				Msg("Looking to go somewhere else?");
				break;

			case "shop_inn":
				Msg("I've never heard that there is an inn in this town.");
				Msg("Who told you that?");
				break;

			case "shop_bank":
				Msg("You have to go to the Square for the Bank.<br/>Look for a sign with a winged chest.");
				Msg("Don't tell me you don't know what a Bank sign looks like?");
				break;

			case "shop_smith":
				Msg("There is a Blacksmith's Shop in our town?<br/>Well Nerys would know.<br/>Go find her at the Weapons Shop.");
				Msg("If she doesn't know,<br/>then I think it would be safe to assume that there isn't one here.");
				break;

			case "skill_composing":
				Msg("Anyone can compose these days...<br/>Everything sounds the same.<br/>This is the age of the copycats.");
				break;

			case "skill_tailoring":
				Msg("Simon knows clothing-related skills well.<br/>Although, knowing him, I doubt he would pass on anything.");
				Msg("Why don't you go see him anyway?<br/>He's at the Clothing Shop.");
				break;

			case "skill_magnum_shot":
				Msg("That's something you should ask that<br/>tomboy teacher, Aranwen.");
				Msg("You would probably find her at the School, eh?");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("Go to the School and ask Aranwen there.");
				break;

			case "skill_smash":
				Msg("Hmm. I'd like to show off and teach it to you myself,<br/>but if you develop bad habits or anything,<br/>you would curse me for the rest of your life. Ha ha.");
				Msg("Go talk to Aranwen.<br/>She teaches combat skills, so she should be at the School.");
				break;

			case "skill_gathering":
				Msg("Sometimes Stewart and I go<br/>gather herbs together.");
				Msg("It's no fun to do it by yourself.<br/>By the way, rumor has it that<br/>people sometimes fight over who gets to gather more.");
				Msg("But it's not my place to tell people what to do...<br/>Just come see me if you get hurt while fighting!");
				break;

			case "square":
				Msg("Go outside and follow the road and you'll see it.<br/>When you see the bell tower, that's the Square.<br/>I guess you missed it on your way.");
				Msg("(What a careless kid.)");
				break;

			case "pool":
				Msg("Haha. This town mostly uses wells,<br/>so there's no need for a reservoir.");
				break;

			case "farmland":
				Msg("Right. I hear there are lots of<br/>abnormally large rats eating away the crops.<br/>If you happen to see them,<br/>do us a favor and take them out.");
				break;

			case "shop_headman":
				Msg("Ha ha. The town is ruled by a Lord.<br/>A chief? A chief... Hahaha!");
				break;

			case "temple":
				Msg("Want to see Kristell?<br/>Look at the Minimap! This would be a very good time to use your eyes for once!<br/>...But then again, I suppose she could be hard to locate on the Minimap.");
				Msg("Oh, what the heck! I'll be nice and tell you where she is.<br/>Leave here and follow the road. You'll hit the Square.");
				Msg("From there, go to an alley near Glenis' Restaurant.<br/>Go up the steps there and turn left, and the Church will be right there.");
				Msg("If you can't remember all this, take a look at your Minimap<br/>when you get to the Square.<br/>If you're still not sure, ask other people.");
				break;

			case "school":
				Msg("The School? Follow this road all the way.<br/>You should be able to see it on your Minimap.<br/>It's on the right side of the road,<br/>so it shouldn't be too difficult to find.");
				break;

			case "skill_windmill":
				Msg("So I look like someone who would know about<br/>such a barbaric skill, do I?");
				Msg("Go talk to Aranwen. She's at the School.");
				break;

			case "shop_restaurant":
				Msg("The Restaurant! The Restaurant! Yes, the Restaurant!");
				Msg("Take a look at your Minimap. It should be on there.");
				break;

			case "shop_armory":
				Msg("The Weapons Shop is just over there.<br/>Nerys is often outside.<br/>If you see a slender girl with sharp eyes and red hair, that's Nerys.");
				Msg("By the way... what are you buying?");
				break;

			case "shop_cloth":
				Msg("The Clothing Shop? Oh, you mean Simon's.<br/>It's between the Bank and the General Shop.<br/>Look around the Square and you'll find it.");
				Msg("Say hello to Simon for me.<br/>Also, tell him that I'm very much enjoying this robe.");
				break;

			case "shop_bookstore":
				Msg("You don't look like the book-ish type.<br/>Just play the game. What good are books?");
				Msg("Ha ha. I'm just kidding. Did I offend you?<br/>I'll tell you, so quit looking at me like that.");
				Msg("Exit here and follow the road to the north.<br/>There is an alley in one corner of the north entrance.<br/>Go down there and you'll find the Bookstore.");
				Msg("If you see a little girl with a pair of glasses,<br/>that's where the Bookstore is.");
				break;

			case "shop_goverment_office":
				Msg("That's where the beautiful Eavan works.<br/>The Lord and... whatchama call it...<br/>the Captain of the Royal Guards are also there,<br/>but they hardly leave the Town Office.");
				Msg("If you have any business there, talk to Eavan.<br/>Not only is she pretty, but she's kind, too.<br/>She'll kindly answer any of your questions.");
				break;

			default:
				RndFavorMsg(
					"Well... I don't know.",
					"Don't ask me about that.",
					"I don't know what that is.",
					"Hmm. You have strange interests.",
					"Hmm. I don't know anything about that.",
					"You sure like to ask about strange things...",
					"I think that's enough. It's not even a topic I'm interested in.",
					"Talking about unfamiliar subjects makes us both tired. Let's talk about something else."
				);
				break;
		}
	}
}

public class ManusShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Potions", 51037);     // Base Potion
		Add("Potions", 51001);     // HP 10 Potion
		Add("Potions", 51011);     // Stamina 10 Potion
		Add("Potions", 51000);     // Potion Conconction Kit
		Add("Potions", 51201);     // Marionette 30 x1
		Add("Potions", 51201, 10); // Marionette 30 x10
		Add("Potions", 51201, 20); // Marionette 30 x20
		Add("Potions", 51002);     // HP 30 Potion x1
		Add("Potions", 51002, 10); // HP 30 Potion x10
		Add("Potions", 51002, 20); // HP 30 Potion x20
		Add("Potions", 51012);     // Stamina 30 Potion x1
		Add("Potions", 51012, 10); // Stamina 30 Potion x10
		Add("Potions", 51012, 20); // Stamina 30 Potion x20

		Add("First Aid Kits", 60005, 10); // Bandage x10
		Add("First Aid Kits", 60005, 20); // Bandage x20
		Add("First Aid Kits", 63000, 10); // Phoenix Feather x10
		Add("First Aid Kits", 63000, 20); // Phoenix Feather x20
		Add("First Aid Kits", 63032);     // Pet First-Aid Kit
		Add("First Aid Kits", 63716, 10); // Marionette Repair Set x10
		Add("First Aid Kits", 63716, 20); // Marionette Repair Set x20

		Add("Ect", 1044);     // Reshaping Your Body
		Add("Ect", 1047);     // On Effective Treatment of Wounds
		Add("Ect", 91563);    // Hot Spring Ticket x1
		Add("Ect", 91563, 5); // Hot Spring Ticket x5
	}
}