//--- Aura Script -----------------------------------------------------------
// Deian in Tir Chonaill
//--- Description -----------------------------------------------------------
// Shepard Boy.
//---------------------------------------------------------------------------

public class DeianScript : NpcScript
{
	public override void Load()
	{
		SetName("_deian");
		SetRace(10002);
		SetBody(height: 0.85f);
		SetFace(skinColor: 23, eyeType: 19, eyeColor: 0, mouthType: 0);
		SetStand("human/male/anim/male_natural_stand_npc_deian");
		SetLocation(1, 27953, 42287, 158);

		EquipItem(Pocket.Face, 4900, 0x00FFDC53, 0x00FFB682, 0x00A8DDD3);
		EquipItem(Pocket.Hair, 4156, 0x00E7CB60, 0x00E7CB60, 0x00E7CB60);
		EquipItem(Pocket.Armor, 15656, 0x00E2EDC7, 0x004F5E44, 0x00000000);
		EquipItem(Pocket.Glove, 16099, 0x00343F2D, 0x00000000, 0x00000000);
		EquipItem(Pocket.Shoe, 17287, 0x004C392A, 0x00000000, 0x00000000);
		EquipItem(Pocket.Head, 18407, 0x00343F2D, 0x00000000, 0x00000000);
		EquipItem(Pocket.RightHand1, 40001, 0x00755748, 0x005E9A49, 0x005E9A49);

		AddPhrase("Another day... another boring day in the countryside.");
		AddPhrase("Baa! Baa!");
		AddPhrase("Geez, these sheep are a pain in the neck.");
		AddPhrase("Hey, this way!");
		AddPhrase("I don't understand. I have one extra...");
		AddPhrase("I'm so bored. There's just nothing exciting around here.");
		AddPhrase("It's amazing how fast they grow feeding on grass.");
		AddPhrase("I wonder if I could buy a house with my savings yet...");
		AddPhrase("What the... Now there's one missing!");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Deian.mp3");
	
		await Intro(
			"An adolescent boy carrying a shepherd's staff watches over a flock of sheep.",
			"Now and then, he hollers at some sheep that've wandered too far, and his voice cracks every time.",
			"His skin is tanned and his muscles are strong from his daily work.",
			"Though he's young, he peers at you with so much confidence it almost seems like arrogance."
		);

		Msg("What can I do for you?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Msg("Nice to meet you, I am Deian.<br/>You don't look that old, maybe a couple of years older than I am?<br/>Let's just say we're the same age. You don't mind do ya?<br/>");
				//Msg("Welcome, <username />");
				//Msg("Nice to meet you again.");
				await StartConversation();
				break;

			case "@shop":
				Msg("I got nothing much, except for some quest scrolls. Are you interested?");
				OpenShop("DeianShop");
				return;

			case "@upgrade":
				Msg("Upgrades! Who else would know more about that than the great Deian? Hehe...<br/>Now, what do you want to upgrade?<br/>Don't forget to check how many times you can upgrade that tiem and what type of upgrade it is before you give it to me... <upgrade />",
					Button("End Conversation", "@end"));

				switch (await Select())
				{
					case "@end":
					case "@reject":
						Msg("Come and see me again.<br/>I just discovered I have a new talent. Thanks to you!<br/><upgrade hide='true'/>");
						break;
				}
				break;
		}
		
		End();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg("Yeah, yeah. I'm a mere shepherd...for now.<br/>But I will soon be a mighty warrior!<br/>");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				GiveKeyword("pool");
				Msg("Some people should have been born as fish.<br/>They can't pass water without diving right in.<br/>I wish they'd stop.");
				Msg("Not long ago, someone jumped into the reservoir<br/>and made a huge mess.<br/>Guess who got stuck cleaning it up?<br/>Sooo not my job.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				GiveKeyword("skill_instrument");
				Msg("Know anything about the Instrument Playing skill?<br/>Only introspective guys like me<br/>can handle instruments.<br/>I wonder how well you would do...");
				Msg("Priestess Endelyon knows all about this skill.<br/>You should talk to her.<br/>");
				break;

			case "about_arbeit":
				Msg("It's not time to start work yet.<br/>Can you come back and ask for a job later?");
				//Msg("Do you want a part-time job? I'm always in need of help.<br/>Have you ever sheared a sheep before?<br/>If you keep doing a good job, I'll raise your pay.<br/>Want to give it a try?");
				break;

			case "skill_instrument":
				GiveKeyword("temple");
				Msg("You really are something.<br/>I just told you,<br/>talk to Priestess Endelyon at the Church<br/>about that.");
				Msg("I know your type...<br/>You like to use everything single<br/>keyword you get... Bug off!");
				break;

			case "pool":
				Msg("It's right behind chubby ol' Caitin's place.<br/>You know where her Grocery Store is, right?");
				Msg("By the way, what are you going to do there?<br/>You're not going to jump in, are you?<br/>I'm just teasing. Calm down.");
				break;

			case "temple":
				Msg("The Church... Hm, the Church....<br/>That... Er... Hmm...");
				Msg("Well, I don't know! Go into town and ask someone there!<br/>Or just look at your Minimap, geez!");
				break;

			case "shop_misc":
				Msg("You know the guy at the General Shop? His name is Malcolm.<br/>Everyone knows he's a hermit.<br/>He does nothing but work, all day long.<br/>What a dull life!");
				break;

			case "shop_grocery":
				Msg("Every time I go there, I smell fresh baked bread. Yum.<br/>Boy, I miss that fatty, Caitin.");
				Msg("You know what? Caitin has a pretty face,<br/>but her legs are so chunky! Like tree trunks! Hahahaha!<br/>There's a reason she wears long skirts.<br/>Hehe...");
				break;

			case "shop_healing":
				Msg("Oh, you are talking about Dilys' place.<br/>Sometimes, even when I bring a sick lamb, she still treats it with extra care.<br/>I guess lambs and humans aren't that much different when they're sick...");
				break;

			case "shop_bank":
				Msg("Darn, I wish I had enough items to deposit at the Bank.<br/>Did you talk to Bebhinn?<br/>Bebhinn loves to talk about other people.<br/>You'd better be careful when you talk to her.");
				break;

			case "shop_smith":
				Msg("The Blacksmith's Shop is too hot. I just hate the heat.<br/>I'd rather be under the shade of a nice tree...<br/>");
				break;

			case "skill_counter_attack":
				Msg("I heard somewhere, you can learn that<br/>by getting beat up...<br/> It's not worth it for me.<br/>A method like that just seems stupid...");
				break;

			case "skill_smash":
				Msg("Well, I learned that before.");
				Msg("But I forgot.");
				break;

			case "skill_gathering":
				Msg("Here's the rundown.<br/>Think about what you want to gather first, then, find out where you can get it.<br/>You'll need the right tool.<br/>More importantly, you need time, hard work, and money.");
				Msg("But you won't get paid much.<br/>You want to make an easy living by picking up stuff from the ground, right?<br/>But trust me, it's not that easy. I've tried.");
				break;

			case "square":
				Msg("The Square? Are you serious?<br/>You haven't been there yet?<br/>You are such a bad liar!<br/>I saw you walking out from the Square<br/>just a moment ago!");
				break;

			case "farmland":
				Msg("Are you really interested in that?<br/>Don't ask unless you are really interested!<br/>What? How am I suppose to know if you are interested or not?<br/>If you are interested in the farmland, what are you doing here?");
				break;

			case "windmill":
				Msg("You must be talking about the Windmill down there.<br/>Well, you won't find anything interesting there.<br/>You'll see a little kid.<br/>Even if she acts rude, just let her be...");
				break;

			case "brook":
				Msg("It's the stream right over there!<br/>Didn't you cross the bridge on your way here?<br/>Ha... Your memory is a bit...poor.");
				Msg("Sometimes, if you stay here long enough,<br/>you see people peeing in it.  Gross.");
				break;

			case "shop_headman":
				Msg("If you're going to the Chief's House,<br/>go to the Square first.<br/>You'll find a hill with a drawing on it.");
				Msg("Yeah, where the big tree is.<br/>There's a house over that hill.<br/>That's where our Chief lives.");
				break;

			case "school":
				Msg("Where's the School?<br/>Wow, you are way lost.");
				Msg("Okay, cross the stream first, alright?<br/>Then run along, with the stream on your left<br/>and you will see the farmland.<br/>Once you see it, you know you're almost there.");
				Msg("It's really close to the farmland, so you'll see it right away.");
				Msg("Hey, wait a minute. Why am I telling you all this?<br/>I'm a busy guy!");
				break;

			case "graveyard":
				Msg("The graveyard? That place is creepy.");
				Msg("You know it's on your Minimap...<br/>Asking all these foolish questions...<br/>What's your problem?");
				break;

			case "lute":
				Msg("Oh... I want a red lute.<br/>Why don't you buy me one when you get rich, yea?");
				break;

			case "complicity":
				Msg("Welcome to the real world...");
				break;

			default:
				RndMsg(
					"Ask all you want, I'm not telling you.",
					"Hold up, I feel like I'm being interrogated.",
					"Meh, I don't want to tell you.",
					"Pry all you like. You'll get nothing from me.",
					"So many questions, at least give me a small gift...",
					"Sometimes, I'm just not in the mood to answer questions."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class DeianShop : NpcShopScript
{
	public override void Setup()
	{
		//Party Quest tab
		//Add("Party Quest", ?);        // Party Quests.
		Add("Gathering Tools", 40023);        // Gathering Knife
	}
}
