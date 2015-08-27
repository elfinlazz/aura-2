//--- Aura Script -----------------------------------------------------------
// Sion
//--- Description -----------------------------------------------------------
// Furnace/Watermill Operator
//---------------------------------------------------------------------------

public class SionScript : NpcScript
{
	public override void Load()
	{
		SetName("_sion");
		SetRace(10002);
		SetBody(height: 0.1f, upper: 1.3f, lower: 1.3f);
		SetFace(skinColor: 17, eyeType: 2, eyeColor: 27, mouthType: 3);
		SetStand("human/anim/tool/Rhand_A/female_tool_Rhand_A02_stand_friendly");
		SetLocation(31, 12093, 15062, 184);

		EquipItem(Pocket.Face, 4900, 0x00531B77, 0x007C4D8F, 0x00804A9D);
		EquipItem(Pocket.Hair, 4008, 0x002E4830, 0x002E4830, 0x002E4830);
		EquipItem(Pocket.Armor, 15044, 0x0054697A, 0x00CAD98C, 0x001F2E26);
		EquipItem(Pocket.Glove, 16000, 0x00AFA992, 0x00B60659, 0x00E1D5E9);
		EquipItem(Pocket.Shoe, 17012, 0x00676149, 0x0071485B, 0x00F78A3D);
		EquipItem(Pocket.Head, 18024, 0x00808000, 0x00FFFFFF, 0x00AA89C0);
		EquipItem(Pocket.RightHand1, 40025, 0x00C0C6BB, 0x008E6D59, 0x00C7B0D5);

		AddGreeting(0, "You're not from this town, are you? I don't think I've seen you before.");
		AddGreeting(1, "Oh! I remember you. We talked before, remember?");

		AddPhrase("Dad should be coming any minute now...");
		AddPhrase("I want to grow up quickly and be an adult soon.");
		AddPhrase("I wonder what's for dinner. *Gulp*");
		AddPhrase("Ibbie... I miss you...");
		AddPhrase("If you want to activate the switch by the Watermill, let me know!");
		AddPhrase("If you want to make an ingot, talk to me first!");
		AddPhrase("If you want to refine ore, you have to come talk to me!");
		AddPhrase("The Watermill never gets boring...");
		AddPhrase("The way Gilmore talks is too hard to understand.");
		AddPhrase("To fire up the furnace, come talk to me!");
		AddPhrase("Why does Bryce not like me?");
		AddPhrase("You have to pay. You have to pay to activate the switch!");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Sion.mp3");

		await Intro(
			"Wearing a sturdy overall over his pale yellow shirt, this boy has soot and dust all over his face, hands, and clothes.",
			"His short and stubby fingers are quite calloused, and he repeatedly rubs his hands on the bulging pocket of his pants.",
			"His dark green hair is so coarse that even his hair band can't keep it neat. But between his messy hair, his brown sparkly eyes shine bright with curiosity."
		);

		Msg("What's up?", Button("Start Conversation", "@talk"), Button("Use a Furnace", "@watermill"), Button("Upgrade Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Player.Titles.SelectedTitle == 11002)
					Msg("You're incredible...<br/>but, I'll be happy just being Ibbie's guardian.");
				await Conversation();
				break;

			case "@watermill":
				Msg("Unimplemented");

				/*Msg("Do you want to use the furnace?<br/>You can use it for 1 minute with 100 Gold,<br/>and for 5 minutes with 450 Gold.");
				Msg("Hehe... It uses firewood, water, and other things...<br/>so I'm sorry but I have to charge you or I lose money.<br/>However, anyone can use it when it's running.", Button("1 Minute", "@1minute"), Button("5 Minutes", "@5minute"), Button("Forget It", "@quit"));
				
				var response = await Select();
				if (response == "@1minute")
				{
					Msg("There, I turned on the switch. Now anyone can use the furnace for 1 minute.");
				}
				else if (response == "@5minute")
				{
					Msg("There, I turned on the switch. Now anyone can use the furnace for 5 minutes.");
				}
				else if (response == "@quit")
				{
					Msg("You're not going to pay? Then you can't make ingots.<br/>You need fire to refine ore...");
				}

				// Already in Use
				// Msg("I started the furnace...<br/>You better hurry up and use it before time runs out.");

				// If you don't have enough money
				// Msg("You don't have enough to pay for it.<br/>...I'm sorry, but you need more money...");*/
				break;

			case "@upgrade":
				Msg("The pickaxe?<br/>Well, I used to play with it quite a bit as a kid...<br/>Do you think it needs to be upgraded? Leave it up to me.");
				Msg("Unimplemented");
				Msg("Come see me anytime, especially if you need anything upgraded.");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "My name is Sion, hehe.<br/>I manage this awesome water mill.");
				Msg("If you want to refine ore, make sure you come talk to me.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "I heard from my dad that the mine over here<br/>was originally a dungeon.");
				Msg("So he said the inside still looks like a dungeon.");
				Msg("What happened was, they found metal ore there<br/>while building a dungeon.<br/>So they stopped the construction<br/>and started to develop the mine.");
				Msg("Then, they said they kept running into water,<br/>so they built a water mill here...");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "shop_misc":
				Msg("Hehe...You mean Gilmore's place?<br/>Head southwest and go through the alley.<br/>Let me know if he's got anything decent... Hehe.");
				break;

			case "shop_grocery":
				Msg("Are you hungry? ...Hehe");
				Msg("Don't eat too much now. You're gonna get fat.");
				break;

			case "shop_healing":
				Msg("...What's that?");
				Msg("I think Ibbie said she needs one too...");
				break;

			case "shop_bank":
				Msg("Oh, there?  That's where Ibbie's dad works.<br/>But he doesn't seem to like me too much...");
				Msg("He's going to be my future father-in-law...teehee...<br/>I hope he starts liking me soon...");
				Msg("So anyway...<br/>do you think you can find out<br/>what Bryce likes for me...?");
				Msg("I gotta do something<br/>to make him like me more.");
				break;

			case "shop_smith":
				Msg("Hmm. Do you know if a blacksmith has anything to do with being a Chief?");
				Msg("Edern sure looks like a Chief.");
				break;

			case "skill_rest":
				Msg("Hmm... I want to go sit next to Ibbie, too...");
				break;

			case "skill_range":
				Msg("Are you throwing a stone or something?");
				break;

			case "skill_instrument":
				Msg("Hmm... Ibbie should try playing an instrument.");
				break;

			case "skill_tailoring":
				Msg("I tried that once before,<br/>but I kept on poking myself with the needle...<br/>I guess I'm too clumsy, hehe...");
				break;

			case "skill_magnum_shot":
				Msg("Riocard would be the one to talk to about something like that.<br/>Have you talked to him yet...?");
				break;

			case "skill_counter_attack":
				Msg("Riocard would be the one to talk to about something like that.<br/>He should be over at the Pub.");
				Msg("Oh, I'm sorry if you already knew that...");
				break;

			case "skill_smash":
				Msg("Riocard would be the one to talk to about that.");
				Msg("I wonder if you can use the Smash skill with a Pickaxe... That would be cool.");
				break;

			case "skill_gathering":
				Msg("A Pickaxe is your best bet in this town.<br/>I even talked to Gilmore, and<br/>he told me that it's the most popular weapon.");
				break;

			case "pool":
				Msg("No, you're lying.<br/>How can you have so much water in one place?");
				Msg("I know I'm a country boy, but I'm not that dumb. Hehe...");
				break;

			case "farmland":
				Msg("My dad used to tell me<br/>this town doesn't have enough land for farming,<br/>and that mining is the only way to make a living.");
				Msg("What is farming again...?<br/>Planting trees and eating stuff that grow from it?<br/>That sounds kind of fun.");
				break;

			case "windmill":
				Msg("Yeah. I've only heard about it, but never seen it myself.<br/>They say it's like a watermill.");
				break;

			case "shop_headman":
				Msg("Huh? Do we have something like that in our town?");
				Msg("I don't know... Hehe...");
				break;

			case "temple":
				Msg("We don't have a church here.<br/>Comgan is working hard to build one, though.");
				Msg("He's an awesome person.<br/>I don't think I could ever think about doing something like that.<br/>He's basically trying to do<br/>something that takes several adults to do all by himself.");
				Msg("That's awesome!");
				break;

			case "school":
				Msg("Ibbie told me about it.<br/>I heard it's a place where friends my age get together.");
				Msg("Well, I don't like the idea of studying, but<br/>I just might be able to put up with it I make a lot of friends...");
				break;

			case "skill_windmill":
				Msg("Hehe...<br/>What is that?");
				break;

			case "skill_campfire":
				Msg("The grown-ups in this town don't really like making fire...");
				Msg("But still, fire looks fun.<br/>The grown-ups use it whenever they want<br/>but won't let the kids use it.");
				Msg("that seems kind of unfair... Don't you think?");
				Msg("I can't wait to be an adult.");
				break;

			case "shop_restaurant":
				Msg("Heh...<br/>I haven't had anything to eat in a while.<br/>Would you mind getting me something? I... I don't have money...");
				break;

			case "shop_armory":
				Msg("Elen sells weapons.<br/>Yep. The tanned girl at the Blacksmith.");
				Msg("I wish she would put that hammer of hers away when she talks to me.<br/>She's too wild sometimes...");
				break;

			case "shop_cloth":
				Msg("A place that sells clothes, right?<br/>Then, you can go to Gilmore's General Shop!<br/>");
				break;

			case "shop_bookstore":
				Msg("...Hehe... Books put me to sleep...");
				break;

			case "shop_goverment_office":
				Msg("Yeah, Ibbie told me.<br/>She said there's always one in big cities.");
				break;

			case "graveyard":
				Msg("That's a place where people are buried when they die, right?<br/>Hmm... I'm not really scared but<br/>it does creep me out a little bit...");
				break;

			default:
				RndFavorMsg(
					"Hmm... I don't really know...",
					"Mmm? Tell me. What is it about?",
					"I don't know anything about that... Do you really need to know...?",
					"I don't really know anything about that... But please don't tell Ibbie...",
					"I'm not sure...<br/>I feel really ignorant talking with you, <username/>..."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}