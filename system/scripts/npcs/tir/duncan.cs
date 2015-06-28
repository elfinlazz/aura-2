//--- Aura Script -----------------------------------------------------------
// Duncan in Tir Chonaill
//--- Description -----------------------------------------------------------
// Good ol' Duncan
//---------------------------------------------------------------------------

public class DuncanBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_duncan");
		SetRace(10002);
		SetBody(height: 1.3f);
		SetFace(skinColor: 20, eyeType: 17);
		SetStand("human/male/anim/male_natural_stand_npc_duncan_new", "male_natural_stand_npc_Duncan_talk");
		SetLocation(1, 15409, 38310, 122);
		SetGiftWeights(beauty: 0, individuality: 0, luxury: 0, toughness: 1, utility: 2, rarity: 0, meaning: 2, adult: 1, maniac: -1, anime: -1, sexy: 0);

		EquipItem(Pocket.Face, 4950, 0x93005C);
		EquipItem(Pocket.Hair, 4083, 0xBAAD9A);
		EquipItem(Pocket.Armor, 15004, 0x5E3E48, 0xD4975C, 0x3D3645);
		EquipItem(Pocket.Shoe, 17021, 0xCBBBAD);

		AddGreeting(0, "Welcome to Tir Chonaill.");
		AddGreeting(1, "What did you say your name was again...?<br/>Anyway, welcome.");
		AddGreeting(2, "<username/>, I could recognize you from afar.");
		AddGreeting(6, "I was just thinking... <username/> should be visiting right about now.");
		AddGreeting(7, "Hoho, I will definitely remember your face, <username/>!");

		AddPhrase("Ah, that bird in the tree is still sleeping.");
		AddPhrase("Ah, who knows how many days are left in these old bones?");
		AddPhrase("Everything appears to be fine, but something feels off.");
		AddPhrase("Hmm....");
		AddPhrase("It's quite warm today.");
		AddPhrase("Sometimes, my memories sneak up on me and steal my breath away.");
		AddPhrase("That tree has been there for quite a long time, now that I think about it.");
		AddPhrase("The graveyard has been left unattended far too long.");
		AddPhrase("Watch your language.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Duncan.mp3");
		
		await Intro(
			"An elderly man gazes softly at the world around him with a calm air of confidence.",
			"Although his face appears weather-beaten, and his hair and beard are gray, his large beaming eyes make him look youthful somehow.",
			"As he speaks, his voice resonates with a kind of gentle authority."
		);
		
		Msg("Please let me know if you need anything.", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Retrive Lost Items", "@lostandfound"));
		
		switch(await Select())
		{
			case "@talk":
				Greet();
				await StartConversation();
				break;
				
			case "@shop":
				Msg("Choose a quest you would like to do.");
				OpenShop("DuncanShop");
				return;
				
			case "@lostandfound":
				Msg("If you are knocked unconcious in a dungeon or field, any item you've dropped will be lost unless you get resurrected right at the spot.<br/>Lost items can usually be recovered from a Town Office or a Lost-and-Found.");
				Msg("Unfortunatly, Tir Chonaill does not have a Town Office, so I run the Lost-and-Found myself.<br/>The lost items are recovered with magic,<br/>so unless you've dropped them on purpose, you can recover those items with their blessings intact.<br/>You will, however, need to pay a fee.");
				Msg("As you can see, I have limited space in my home. So I can only keep 20 items for you.<br/>If there are more than 20 lost items, I'll have to throw out the oldest items to make room.<br/>I strongly suggest you retrieve any lost items you don't want to lose as soon as possible.");
				break;
		}
		
		End();
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Favor > 40)
					Msg("See that bird on the tree over there? When I was young, he used to help me on the battlefield.<br/>Now he's as old as I am and sleeps all the time.<br/>Perhaps he has closed his heart in disappointment at my present appearance, so old and changed...");
				else
					Msg("Once again, welcome to Tir Chonaill.");
					
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "rumor":
				if (Favor > 40)
					Msg("The weather here changes unpredictably because Tir Chonaill is located high up in the mountains.<br/>There are instances where bridges collapse and roads are destroyed after a heavy rainfall,<br/>and people lose all contact with the outside world.<br/>Despite that, I think you've done quite well here.");
				else
					Msg("I heard a rumor that this is just a copy of the world of Erin. Trippy, huh?");
				//Msg("Talk to the good people in Tir Chonaill as much as you can, and pay close attention to what they say.<br/>Once you become friends with them, they will help you in many ways.<br/>Why don't you start off by visiting the buildings around the Square?");
				
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "about_skill":
				if(HasSkill(SkillId.RangedAttack) && !HasSkill(SkillId.MagnumShot))
				{
					// Unofficial
					Msg("You know about the Ranged Attack skill? I've heard about another skill called Magnum Shot.<br/>Supposedly it's very strong.");
					GiveKeyword("skill_magnum_shot");
					break;
				}
				
				Msg("I don't know of any skills... Why don't you ask Malcom?");
				//Msg("You know about the Combat Mastery skill?<br/>It's one of the basic skills needed to protect yourself in combat.<br/>It may look simple, but never underestimate its efficiency.<br/>Continue training the skill diligently and you will soon reap the rewards. That's a promise.");
				break;
			
			case "about_arbeit":
				Msg("I don't have any jobs for you, but you can get a part time job in town.");
				//Msg("Are you interested in a part-time job?<br/>It's great to see young people eager to work!<br/>To get one, talk to the people in town with the 'Part-Time Jobs' keyword.<br/>If you go at the right time, you'll be offered a job.<p/>If you do a good job, you will be duly rewarded.<br/>Just make sure to return to the person who gave you the job and report the results before the deadline.<br/>If you miss the deadline, you will not be rewarded regardless of how hard you worked.<p/>Part-time jobs aren't available 24 hours a day.<br/>You have to get there at the right time.<p/>The sign-up period usually begins between 7:00 am and 9:00 am.<br/>Since there are only a limited number of jobs available,<br/>others may take them all if you're too late.<br/>Also, you can do only one part-time job per day.<p/>It looks like Nora and Caitin could use your help,<br/>so head to the Grocery Store or the Inn and talk to them.<br/>Start the conversation with them with the keyword 'Part-Time Jobs' and make sure it's between 7 and 9 am.<br/>Good luck!");
				break;
			
			case "about_study":
				Msg("You can study different magic down at the school!");
				break;
			
			case "shop_misc":
				Msg("If you look down at the Square, you can see a building with a dark roof.<br/>That's the General Shop,<br/>where Malcolm sells homemade products.<br/>The quality of his products are quite good.");
				break;
			
			case "shop_bank":
				Msg("It's been a while since the Erskin Bank first opened its doors...<br/>It's that big building with a tiled roof below in the Square.<br/>There, you'll find Bebhinn, the teller.<br/>She knows a lot of gossip, so talk to her if you're curious.");
				break;
			
			case "skill_counter_attack":
				Msg("Haha, I am just the Chief of a small town. I don't know the details of that...<br/>I'm too old to give demonstrations, don't you think?<br/>Why don't you go ask Trefor to teach you?<br/>Go farther up the hill past the Healer's House, and you will see him.");
				break;
				
			default:
				RndFavorMsg(
					"I don't know anything about that...",
					"I think it'd be better for you to ask someone else.",
					"Hmm, I wonder who might know about that...",
					"I have no idea...",
					"I don't really know about that... "
				);
				
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
	
	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				Msg("Oh! How did you know I like this?<br/>Thank you very much.");
				break;
				
			case GiftReaction.Dislike:
				RndMsg(
					"Hmm. Not exactly to my taste...",
					"Hmm. I'll keep it safe for someone who may need it."
				);
				break;
			
			default:
				RndMsg(
					"Is that for me?",
					"You didn't need to do this..."
				);
				break;
		}
	}
}

public class DuncanShop : NpcShopScript
{
	public override void Setup()
	{
		//Quest tab
		
		//Party Quest tab

		Add("Etc", 1045);		// Hit What You See
	}
}
