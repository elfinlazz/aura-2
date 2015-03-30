//--- Aura Script -----------------------------------------------------------
// Bryce
//--- Description -----------------------------------------------------------
// Banker
//---------------------------------------------------------------------------

public class BryceScript : NpcScript
{
	public override void Load()
	{
		SetName("_bryce");
		SetRace(10002);
		SetBody(height: 1.1f, upper: 1.5f);
		SetFace(skinColor: 20, eyeType: 5, eyeColor: 76, mouthType: 12);
		SetStand("human/male/anim/male_natural_stand_npc_bryce");
		SetLocation(31, 11365, 9372, 0);

		EquipItem(Pocket.Face, 4902, 0x00FCCE4F, 0x00D69559, 0x009DD5AA);
		EquipItem(Pocket.Hair, 4027, 0x005B482B, 0x005B482B, 0x005B482B);
		EquipItem(Pocket.Armor, 15034, 0x00FAF7EB, 0x003C2D22, 0x00100C0A);
		EquipItem(Pocket.Shoe, 17009, 0x00000000, 0x00F69A2B, 0x004B676F);

		AddGreeting(0, "Welcome to the Bangor branch of the Erskin Bank.");
		AddGreeting(1, "Hello, <username/>. I'm pretty good with names.");

		AddPhrase("*Cough* There's just too much dust in here.");
		AddPhrase("Anyway, where did Ibbie go again?");
		AddPhrase("Have my eyes really become this bad?");
		AddPhrase("I don't even have time to read a book these days.");
		AddPhrase("I'll just have to fight through it.");
		AddPhrase("It's about the time Ibbie returned.");
		AddPhrase("It's almost time.");
		AddPhrase("Mmm... Up to where did I calculate?");
		AddPhrase("Sion, you little punk... You'll pay if you bully my Ibbie.");
		AddPhrase("Tomorrow will be better than today.");
		AddPhrase("Well, cheer up!");
		AddPhrase("What should I buy Ibbie today?");
		AddPhrase("When was I supposed to be contacted from Dunbarton?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Bryce.mp3");

		await Intro(
			"He's dressed neatly in a high neck shirt and a brown vest.",
			"His cleft chin is cleanly shaved and his hair has been well groomed and flawlessly brushed back.",
			"He stares at you with shining hazelnut eyes that are deep-set in his pale face."
		);

		Msg("What is it?", Button("Start a Conversation", "@talk"), Button("Open My Account", "@bank"), Button("Redeem Coupon", "@coupon"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
				{
					Msg("Guardian of Erinn...?<br/>You know, listening to over exaggerated rumors<br/>can be dangerous to you.");
					Msg("Although, if anyone, you could<br/>probably fit that title...");
				}
				await Conversation();
				break;

			case "@bank":
				OpenBank();
				return;

			case "@coupon":
				Msg("Would you like to redeem your coupon?<br/>You're a blessed one.<br/>Please input the number of the coupon you wish to redeem.", Input("Redeem Coupon", "Enter Coupon Number"));
				var input = await Select();

				if (input == "@cancel")
					return;

				if (!RedeemCoupon(input))
				{
					Msg("......<br/>I'm not sure what kind of coupon this is.<br/>Please make sure that you have inputted the correct coupon number.");
				}
				else
				{
					// Unofficial response.
					Msg("There you go, have a nice day.");
				}
				break;

			case "@shop":
				Msg("You need a license to open a Personal Shop here.<br/>...I recommend buying one in case you need it.");
				OpenShop("BryceShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "My name is Bryce.<br/>I take care of bank duties here.<br/>Is there anything I can help you with?");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "How is this town?");
				Msg("Judging from your looks,<br/>I'm guessing you want to talk about the dragon ruins found on the way into this town.");
				Msg("Are you interested in hearing an old legend?<br/>If you are, there is an ancient tale I'd like to tell you.");
				Msg("I heard this tale a long time ago.<br/>The ancient humans who lived around here used to worship a dragon.");
				Msg("The dragon made regular appearances in this town,<br/>burning everything to the ground.<br/>Town folks built a gigantic stone statue<br/>and sacrificed virgins of the town to ease the rage of the dragon.");
				Msg("People called that dragon Cromm Cruaich.<br/>A God of Destruction from another world.<br/>Yes, it's the dragon that took the life of the ancient king, Nuadha.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_arbeit":
				Msg("Part-time job?<br/>I don't know. Right now, I'm good by myself here.");
				break;

			case "shop_misc":
				Msg("If you make a turn at the next alley,<br/>you should be able to see it.");
				break;

			case "shop_grocery":
				Msg("You seem to be looking for a good place to eat.<br/>Since this is a small town, the only place you'll find food is at the Pub.");
				Msg("Jennifer is a decent cook,<br/>so unless you're too picky with food, the Pub should be good enough.");
				Msg("It's just that Jennifer's a bit lazy, so... Hm-Hmm...");
				break;

			case "shop_healing":
				Msg("There's no such place around here.<br/>I was really worried when Ibbie was sick as well.");
				Msg("This town may not have much to offer for now, but<br/>I am confident that with time, things will get better.");
				break;

			case "shop_inn":
				Msg("This town doesn't have an inn,<br/>Drifters aren't so common here, you see.<br/>You could always camp outside of town<br/>Rustic, I know, but what can you do? You should bear with it for now.");
				break;

			case "shop_bank":
				Msg("Yes, I take care of the banking needs.<br/>I understand if you feel apprehensive about trusting me,<br/>since I don't work in an actual building.");
				Msg("Even so, just because there isn't a structure<br/>doesn't mean it's impossible to see to your banking needs.<br/>If there's anything you need,<br/>you just let me know.");
				break;

			case "shop_smith":
				Msg("The Blacksmith's Shop is located just to my left.<br/>You should drop by if you wish to repair or purchase a new weapon.");
				Msg("I can even hold onto your weapons as much as the storage space allows.<br/>Make note of that for future reference.");
				break;

			case "skill_rest":
				Msg("Sion once came to me<br/>asking me to teach him the Resting skill.");
				Msg("He should learn such things from his own father.<br/>I don't understand why he asked me instead.");
				break;

			case "skill_range":
				Msg("I'm not accustomed to fighting,<br/>so asking me about it wouldn't be of much help to you.");
				Msg("Do you still want my help?");
				Msg("Really, I know nothing about it.");
				break;

			case "skill_instrument":
				Msg("I'm not quite sure.");
				Msg("Let's see...<br/>I wonder who you should go see for that...");
				break;

			case "skill_composing":
				Msg("I'm not really interested in composing.");
				Msg("Haha... I have nothing to say even if you think<br/>I'm the least romantic person in the world.");
				break;

			case "skill_tailoring":
				Msg("Hmmm... Did someone tell you that<br/>I may know a thing or two about it?");
				Msg("This just seems like a bad joke.");
				break;

			case "skill_magnum_shot":
				Msg("Magnum Shot?<br/>The only thing I know is that it's a bow-related skill.");
				Msg("I'm no good when it comes to fighting.<br/>Honestly, I'm fed up with people telling others that<br/>they used to be great fighters back in the day.<br/>It just sounds cocky.");
				break;

			case "skill_gathering":
				Msg("Gathering?<br/>Iron ores are really all that can be found in this village.<br/>Why don't you grab a Pickaxe and<br/>head on over to the mine in Barri Dungeon?");
				Msg("You can purchase a Pickaxe from the Blacksmith's Shop.");
				break;

			case "pool":
				Msg("I'm not sure if you've already heard this from someone else,<br/>but water is precious in this town.");
				Msg("Emain Macha would be another story,<br/>but a reservoir in a town like this is...");
				break;

			case "farmland":
				Msg("The fields around this town are mostly farmland.<br/>You can see it yourself<br/>if you go along the town walls.");
				break;

			case "windmill":
				Msg("Since water is so rare in this town,<br/>farming is no easy task.");
				Msg("Also, since the land is barren,<br/>regardless of what's planted, it will wither away in no time.");
				Msg("I guess we should be thankful for the few trees that are growing here...");
				break;

			case "brook":
				Msg("This town is located in a valley,<br/>so it's strange that the wind almost never blows here.");
				Msg("A windmill can be built,<br/>but I'm not sure what good it would do in a town like this.");
				break;

			case "shop_headman":
				Msg("I'm not sure.<br/>Now that you mention it, we do not have a chief in this town.");
				Msg("Our town is that small...");
				break;

			case "temple":
				Msg("Priest Comgan seems to be having a hard time.<br/>I'd honestly like to help if I'm in any position to help but...");
				Msg("I'm currently struggling myself so...");
				break;

			case "school":
				Msg("I should send Ibbie to school as well.");
				Msg("She was too sick to attend school at our last town.<br/>I'm worried about her.");
				Msg("I can't let her be out of school for too long.");
				break;

			case "skill_windmill":
				Msg("Windmill skill?<br/>It sounds like a combat skill afterall.");
				Msg("Fighting with fists is a method<br/>used by those who lack the will or ability to carry an intelligent conversation.");
				Msg("Why resort to fists without first making a good use of words?");
				break;

			case "skill_campfire":
				Msg("Please be extra cautious<br/>when building a fire in this town.");
				Msg("There was a big fire in this town not too long ago,<br/>so the residents become annoyed<br/>with people carelessly building fire.");
				Msg("No one will stop you from starting one,<br/>but you should keep this in mind.");
				break;

			case "shop_restaurant":
				Msg("Are you hungry?<br/>Then you should go to the Pub<br/>and order some food from Jennifer.");
				break;

			case "shop_armory":
				Msg("It'll be faster for you to talk to Elen at the Blacksmith's Shop about that<br/>than me.");
				break;

			case "shop_cloth":
				Msg("Clothes? Hmmm...<br/>I, for one, don't concern myself too much with clothes.");
				Msg("Even so, Gilmore's General Shop...<br/>That place is just... Wow...");
				break;

			case "shop_bookstore":
				Msg("There's no bookstore in this town.<br/>We often have to rely on Dunbarton if we want to buy anything.");
				break;

			case "shop_goverment_office":
				Msg("The town is too small<br/>to have a need for our own town office.");
				Msg("If any town office will do for your needs,<br/>why don't you check out the one in Dunbarton?");
				break;

			case "graveyard":
				Msg("There's no graveyard in this town.<br/>I do vaguely remember hearing that people are<br/>buried in a place located in Barri Dungeon.");
				break;

			default:
				RndFavorMsg(
					"You should ask other people.",
					"No such tale exists in my memory.",
					"I don't think I know of that tale.",
					"I don't have anything to say about that.",
					"That's a difficult question for me to answer.",
					"I think it might be better to talk about something else now."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class BryceShop : NpcShopScript
{
	public override void Setup()
	{
		Add("License", 60103); // Bangor Merchant License
		Add("License", 81010); // Purple Personal Shop Brownie Work-For-Hire Contract
		Add("License", 81011); // Pink Personal Shop Brownie Work-For-Hire Contract
		Add("License", 81012); // Green Personal Shop Brownie Work-For-Hire Contract
	}
}