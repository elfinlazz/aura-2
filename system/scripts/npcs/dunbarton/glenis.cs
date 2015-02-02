//--- Aura Script -----------------------------------------------------------
// Glenis
//--- Description -----------------------------------------------------------
// Grocery Store Owner
//---------------------------------------------------------------------------

public class GlenisScript : NpcScript
{
	public override void Load()
	{
		SetName("_glenis");
		SetRace(10001);
		SetBody(height: 0.8f, weight: 0.3f, upper: 1.4f, lower: 1.2f);
		SetFace(skinColor: 15, eyeType: 7, eyeColor: 119, mouthType: 1);
		SetStand("human/female/anim/female_natural_stand_npc_Glenis");
		SetLocation(14, 37566, 41605, 129);

		EquipItem(Pocket.Face, 3902, 0x00918B00, 0x000082C9, 0x00018CCB);
		EquipItem(Pocket.Hair, 3020, 0x00BC756C, 0x00BC756C, 0x00BC756C);
		EquipItem(Pocket.Armor, 15010, 0x00764E63, 0x00CCD8ED, 0x00E7957A);
		EquipItem(Pocket.Shoe, 17012, 0x00764E63, 0x00FC9C5F, 0x00D2CCE5);

		AddGreeting(0, "Are you looking for the Restaurant? This is it.");
		AddGreeting(1, "What... was your name again?<br/>Bah, my memory is not like it used to be in my old age.");
        
		AddPhrase("Come buy your food here.");
		AddPhrase("Flora! Are the ingredients ready?");
		AddPhrase("Have a nice day today!");
		AddPhrase("Please come again!");
		AddPhrase("Thank you for coming!");
		AddPhrase("This is Glenis' Restaurant.");
		AddPhrase("This is today's special! Mushroom soup!");
		AddPhrase("We are serving breakfast now.");
		AddPhrase("Welcome!");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Glenis.mp3");

		await Intro(
			"With her round face and large, sparkling eyes, this middle aged woman appears to have a big heart.",
			"Her face, devoid of makeup, is dominated by her large eyes and a playful smile.",
			"Over her lace collar she wears an old but well-polished locket."
		);

		Msg("Welcome!<br/>This is Glenis' Restaurant.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select()) 
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("Erinn's Guardian, huh...?<br/>Sounds like my husband when he was young... Hehe.<br/>If you need anything, just let me know.");
				await Conversation();
				break;

			case "@shop":
				Msg("Are you looking for any kind of food in particular?<br/>Take your pick.");
				OpenShop("GlenisShop");
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
					Msg("Oh, my! Don't be so formal but just call me <npcname/>!");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					GiveKeyword("shop_restaurant");
					Msg(FavorExpression(), "What do I do for a living, you ask? Ha ha. I own this Restaurant, and my name is <npcname/>. I thought I told you all of this.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				GiveKeyword("shop_healing");
				Msg(FavorExpression(), "Rumors around here? Well...<br/>Do I look like I would be keen on other people's affairs? Ha ha.<br/>You seem pretty tired.<br/>Why don't you go to the Healer's House down there and talk to Manus.");
				Msg("If you have the Resting skill handy, he'll give you some helpful tips.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("Ha ha. You've got the look of<br/>someone who can officially use the Cooking skill now.<br/>It makes me happy to know that there's one more person who's going down the same path as I am.<br/>Are you interested in making Assorted Fruits, by any chance?", Button("Of course!", "@fruit01"), Button("I will pass", "@fruit02"));
				switch(await Select())
				{
					case "@fruit01":
						Msg("Ah-ha! You certainly have a burning passion for cooking!<br/>Now, calm yourself down,<br/>and pick up the cooking knife and the cooking table.<br/>Make sure you're mixing here.");
						Msg("First, you have to put the ingredients in.<br/>Apples, strawberries, and berries.<br/>The amount doesn't matter. What's important is the ratio.<br/>Press the 'Use Ingredient' button to determine the ratio.", Image("cook_making01", true));
						Msg("Now, remember this.<br/>1/3 of strawberries...<br/>1/3 of apples...<br/>1/3 of berries...", Image("cook_making01", true));
						Msg("When you put in all the ingredients and the ratio bar turns yellow,<br/>press the 'Start' button to start cooking.", Image("cook_making02", true));
						Msg("You'll have a tasty Assorted Fruits dish.<br/>Ha ha. What do you think?<br/>Make sure you get some ingredients and try it.");
						Msg("Hmm.<br/>If you want to get more cooking recipes,<br/>stop by Aeira's Bookstore.<br/>There are recipe books there. Sometimes I go there to quickly look through them.");
						Msg("Don't tell Aeira, though. Ha ha.");
						break;

					case "@fruit02":
						Msg("Ha ha. I guess my intention's been revealed...");
						Msg("I've been craving fruit lately.");
						break;
				}
				break;

			case "about_arbeit":
				Msg("Unimplemented");
				break;

			case "shop_misc":
				Msg("The General Shop?<br/>Oh, Walter over there<br/>is the owner of that General Shop!<br/>He's right behind you!");
				break;

			case "shop_grocery":
				Msg("Ah, you are looking for my Restaurant.<br/>Next time, ask for the Restaurant.<br/>A lot of people tend to get confused.");
				break;

			case "shop_healing":
				Msg("Oh, you mean Manus' place!<br/>You should see it if you go straight towards the south entrance.<br/>Why... are you not feeling well?");
				break;

			case "shop_inn":
				Msg("Hmmm...<br/>Is there an inn in this town..?");
				break;

			case "shop_bank":
				Msg("Go across from here<br/>and keep following along to the end of the Square.<br/>It should be past the General Shop and the Clothing Shop.<br/>Say hello to Austeyn for me.");
				break;

			case "shop_smith":
				Msg("A Blacksmith's Shop? Hahaha.<br/>I never heard that there was a Blacksmith's Shop in this town...<br/>Are you talking about the Weapons Shop, by any chance?");
				break;

			case "skill_range":
				Msg("You speak of it with such ease.<br/>Ha ha. I don't really know.");
				break;

			case "skill_instrument":
				Msg("Speaking of which, my late husband<br/>played music really well...");
				break;

			case "skill_tailoring":
				Msg("My Flora seems to do it as a hobby,<br/>but she not very good at it...<br/>And she's busy, too.");
				break;

			case "skill_gathering":
				Msg("Hmm. It's not easy to explain.<br/>Why don't you do some part-time work at our place?<br/>If not, you can try Manus.");
				break;

			case "square":
				Msg("The Square? It's right here.<br/>Maybe it was so big that the thought escaped you.<br/>Ha ha.");
				break;

			case "pool":
				Msg("Well... There isn't anything like that around here.<br/>Why? Do you want to go for a swim?");
				break;

			case "farmland":
				Msg("It's all farmlands around here.<br/>They seem to be having trouble with the number of rats these days.<br/>Would you go and drive some out, if you have the time?");
				break;

			case "shop_headman":
				Msg("What?<br/>You mean the Town Office, right?<br/>That pretty girl over there is Eavan, who works there.<br/>Go ask her.");
				break;

			case "temple":
				Msg("Go down this alley<br/>and you'll see Priestess Kristell.<br/>That's where the Church is.");
				break;

			case "school":
				Msg("The School? Are you looking for Stewart? Or Aranwen?<br/>A lot of people have been asking for the School recently.<br/>To get there, go east down any alley.");
				break;

			case "shop_restaurant":
				Msg("That's right. This is the Restaurant.<br/>Keep in mind - we not only sell food<br/>but also carry cooking ingredients.");
				break;

			case "shop_armory":
				Msg("If you want to go to the Weapons Shop, go see Nerys.<br/>From the Square, follow the path to the south<br/>and go near the south entrance of Dunbarton.<br/>It's right along the path, so you should easily see it.");
				break;

			case "shop_cloth":
				Msg("Simon's Clothing Shop is just down there,<br/>right next to the General Shop.<br/>It's not too far at all.");
				break;

			case "shop_bookstore":
				Msg("You mean Aeira's Bookstore.<br/>It's just down the alley near the northern town entrance.<br/>It is a bit far from here, but you are young. It won't take you too long by walking. Heeheehee...");
				break;

			case "shop_goverment_office":
				Msg("The Town office? Go to the Square<br/>and find a large building to the north.");
				break;

			default:
				RndFavorMsg(
					"You should go ask someone else.",
					"I don't know much about things like that. Ha ha.",
					"Oh, no. I don't know what to tell you about that.",
					"Ha ha. It's no use asking me about something like that.",
					"Oh, that... Umm... No, I don't think I know anything about it.",
					"Well, people do ask me about it every now and then.<br/>It must be an important topic.",
					"That's not a topic I'm very familiar with. Perhaps Walter over there knows more about it than I do.",
					"Asking about a topic like that to an<br/>ordinary Restaurant owner is not very proper, you know. Ha ha."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class GlenisShop : NpcShopScript
{
	public override void Setup() 
	{
		Add("Food", 50004);     // Bread
		Add("Food", 50002);     // Slice of Cheese
		Add("Food", 50220);     // Corn Powder
		Add("Food", 50206);     // Chocolate
		Add("Food", 50217);     // Celery
		Add("Food", 50217);     // Basil x1
		Add("Food", 50217, 5);  // Basil x5
		Add("Food", 50217, 10); // Basil x10
		Add("Food", 50217, 20); // Basil x20
		Add("Food", 50218);     // Tomato x1
		Add("Food", 50218);     // Tomato x5
		Add("Food", 50218);     // Tomato x10
		Add("Food", 50218);     // Tomato x20
		Add("Food", 50045);     // Pine Nut
		Add("Food", 50047);     // Camellia Seeds
		Add("Food", 50111);     // Carrot x1
		Add("Food", 50111);     // Carrot x5
		Add("Food", 50111);     // Carrot x10
		Add("Food", 50111);     // Carrot x20
		Add("Food", 50018);     // Baking Chocolate x1
		Add("Food", 50018, 5);  // Baking Chocolate x5
		Add("Food", 50018, 10); // Baking Chocolate x10
		Add("Food", 50018, 20); // Baking Chocolate x20
		Add("Food", 50114);     // Garlic x1
		Add("Food", 50114, 5);  // Garlic x5
		Add("Food", 50114, 10); // Garlic x10
		Add("Food", 50114, 20); // Garlic x20
		Add("Food", 50127);     // Shrimp x1
		Add("Food", 50127, 5);  // Shrimp x5
		Add("Food", 50127, 10); // Shrimp x10
		Add("Food", 50127, 20); // Shrimp x20
		Add("Food", 50131);     // Sugar x1
		Add("Food", 50131, 5);  // Sugar x5
		Add("Food", 50131, 10); // Sugar x10
		Add("Food", 50131, 20); // Sugar x20
		Add("Food", 50132);     // Salt x1
		Add("Food", 50132, 5);  // Salt x5
		Add("Food", 50132, 10); // Salt x10
		Add("Food", 50132, 20); // Salt x20
		Add("Food", 50148);     // Yeast x1
		Add("Food", 50148, 5);  // Yeast x5
		Add("Food", 50148, 10); // Yeast x10
		Add("Food", 50148, 20); // Yeast x20
		Add("Food", 50153);     // Deep Fry Batter x1
		Add("Food", 50153, 5);  // Deep Fry Batter x5
		Add("Food", 50153, 10); // Deep Fry Batter x10
		Add("Food", 50153, 20); // Deep Fry Batter x20
		Add("Food", 50156);     // Pepper x1
		Add("Food", 50156, 5);  // Pepper x5
		Add("Food", 50156, 10); // Pepper x10
		Add("Food", 50156, 20); // Pepper x20
		Add("Food", 50046);     // Juniper Berry
		Add("Food", 50112);     // Strawberry x1
		Add("Food", 50112, 5);  // Strawberry x5
		Add("Food", 50112, 10); // Strawberry x10
		Add("Food", 50112, 20); // Strawberry x20
		Add("Food", 50121);     // Butter x1
		Add("Food", 50121, 5);  // Butter x5
		Add("Food", 50121, 10); // Butter x10
		Add("Food", 50121, 20); // Butter x20
		Add("Food", 50142);     // Onion x1
		Add("Food", 50142, 5);  // Onion x5
		Add("Food", 50142, 10); // Onion x10
		Add("Food", 50142, 20); // Onion x20
		Add("Food", 50108);     // Chicken Wings x1
		Add("Food", 50108, 5);  // Chicken Wings x5
		Add("Food", 50108, 10); // Chicken Wings x10
		Add("Food", 50108, 20); // Chicken Wings x20
		Add("Food", 50130);     // Whipped Cream x1
		Add("Food", 50130, 5);  // Whipped Cream x5
		Add("Food", 50130, 10); // Whipped Cream x10
		Add("Food", 50130, 20); // Whipped Cream x20
		Add("Food", 50186);     // Red Pepper Powder x1
		Add("Food", 50186, 5);  // Red Pepper Powder x5
		Add("Food", 50186, 10); // Red Pepper Powder x10
		Add("Food", 50186, 20); // Red Pepper Powder x20
		Add("Food", 50005);     // Large Meat
		Add("Food", 50001);     // Big Lump of Cheese
		Add("Food", 50135);     // Rice x1
		Add("Food", 50135, 5);  // Rice x5
		Add("Food", 50135, 10); // Rice x10
		Add("Food", 50135, 20); // Rice x20
		Add("Food", 50138);     // Cabbage x1
		Add("Food", 50138, 5);  // Cabbage x5
		Add("Food", 50138, 10); // Cabbage x10
		Add("Food", 50138, 20); // Cabbage x20
		Add("Food", 50139);     // Button Mushroom x1
		Add("Food", 50139, 5);  // Button Mushroom x5
		Add("Food", 50139, 10); // Button Mushroom x10
		Add("Food", 50139, 20); // Button Mushroom x20
		Add("Food", 50145);     // Olive Oil x1
		Add("Food", 50145, 5);  // Olive Oil x5
		Add("Food", 50145, 10); // Olive Oil x10
		Add("Food", 50145, 20); // Olive Oil x20
		Add("Food", 50187);     // Lemon x1
		Add("Food", 50187, 5);  // Lemon x5
		Add("Food", 50187, 10); // Lemon x10
		Add("Food", 50187, 20); // Lemon x20
		Add("Food", 50118);     // Orange x1
		Add("Food", 50118, 5);  // Orange x5
		Add("Food", 50118, 10); // Orange x10
		Add("Food", 50118, 20); // Orange x20
		Add("Food", 50118);     // Thyme x1
		Add("Food", 50118, 5);  // Thyme x5
		Add("Food", 50118, 10); // Thyme x10
		Add("Food", 50118, 20); // Thyme x20
		Add("Food", 50421);     // Pecan x1
		Add("Food", 50421, 5);  // Pecan x5
		Add("Food", 50421, 10); // Pecan x10
		Add("Food", 50421, 20); // Pecan x20
		Add("Food", 50426);     // Peanuts x1
		Add("Food", 50426, 5);  // Peanuts x5
		Add("Food", 50426, 10); // Peanuts x10
		Add("Food", 50426, 20); // Peanuts x20
		Add("Food", 50123);     // Roasted Bacon
		Add("Food", 50134);     // Sliced Bread
		Add("Food", 50133);     // Beef
		Add("Food", 50122);     // Bacon x1
		Add("Food", 50122, 5);  // Bacon x5
		Add("Food", 50122, 10); // Bacon x10
		Add("Food", 50122, 20); // Bacon x20
		Add("Food", 50430);     // Grapes x1
		Add("Food", 50430, 5);  // Grapes x5
		Add("Food", 50430, 10); // Grapes x10
		Add("Food", 50430, 20); // Grapes x20
		Add("Food", 50120);     // Steamed Rice
		Add("Food", 50102);     // Potato Salad
		Add("Food", 50431);     // Ripe Pumpkin x1
		Add("Food", 50431, 5);  // Ripe Pumpkin x5
		Add("Food", 50431, 10); // Ripe Pumpkin x10
		Add("Food", 50431, 20); // Ripe Pumpkin x20
		Add("Food", 50006);     // Sliced Meat x1
		Add("Food", 50006, 5);  // Sliced Meat x5
		Add("Food", 50006, 10); // Sliced Meat x10
		Add("Food", 50006, 20); // Sliced Meat x2
		Add("Food", 50104);     // Egg Salad
		Add("Food", 50101);     // Potato Egg Salad

		Add("Gift", 52010); // Ramen
		Add("Gift", 52021); // Slice of Cake
		Add("Gift", 52019); // Heart Cake
		Add("Gift", 52022); // Wine
		Add("Gift", 52023); // Wild Ginseng

		Add("Quest", 70070); // Cooking Quest - Curry Paste
		Add("Quest", 70070); // Cooking Quest - Fry Batter
		Add("Quest", 70070); // Cooking Quest - Flour Dough
		Add("Quest", 70070); // Cooking Quest - Mayonnaise
		Add("Quest", 70070); // Cooking Quest - Steamed Potato
		Add("Quest", 70070); // Cooking Quest - Hard-Boiled Egg
		Add("Quest", 70070); // Cooking Quest - Assorted Fruits
		Add("Quest", 70070); // Cooking Quest - Rice

		Add("Event"); // Empty
	}
}