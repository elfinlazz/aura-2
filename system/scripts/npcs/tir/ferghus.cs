//--- Aura Script -----------------------------------------------------------
// Ferghus, the blacksmith of Tir Chonaill
//--- Description -----------------------------------------------------------
// Weapon Breaker
//---------------------------------------------------------------------------

public class FerghusBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_ferghus");
		SetRace(10002);
		SetBody(height: 1.1f, upper: 1.4f, lower: 1.1f);
		SetFace(skinColor: 23, eyeType: 3, eyeColor: 112, mouthType: 4);
		SetStand("human/male/anim/male_natural_stand_npc_Ferghus_retake");
		SetLocation(1, 18075, 29960, 80);

		EquipItem(Pocket.Face, 4950, 0x00C6C561, 0x00E1C210, 0x00E8A14A);
		EquipItem(Pocket.Hair, 4153, 0x002E303F, 0x002E303F, 0x002E303F);
		EquipItem(Pocket.Armor, 15650, 0x001F2340, 0x00988486, 0x009E9FAC);
		EquipItem(Pocket.Shoe, 17283, 0x0077564A, 0x00F2A03A, 0x008A243D);
		EquipItem(Pocket.RightHand1, 40024, 0x00808080, 0x00212121, 0x00808080);

		AddPhrase("(Spits out a loogie)");
		AddPhrase("Beard! Oh, beard! A true man never forgets how to grow a beard, yeah!");
		AddPhrase("How come they are so late? I've been expecting armor customers for hours now.");
		AddPhrase("Hrrrm");
		AddPhrase("I am running out of Iron Ore. I guess I should wait for more.");
		AddPhrase("I feel like working while singing songs.");
		AddPhrase("I probably did too much hammering yesterday. Now my arm is sore.");
		AddPhrase("I really need a pair of bellows... The sooner the better.");
		AddPhrase("Ouch, I yawned too big. I nearly ripped my mouth open!");
		AddPhrase("Scratching");
		AddPhrase("What am I going to make today?");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Ferghus.mp3");
		
		await Intro(
			"His bronze complexion shines with the glow of vitality. His distinctive facial outline ends with a strong jaw line covered with dark beard.",
			"The first impression clearly shows he is a seasoned blacksmith with years of experience.",
			"The wide-shouldered man keeps humming with a deep voice while his muscular torso swings gently to the rhythm of the tune."
		);
		
		Msg("Welcome to my Blacksmith's Shop.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"), Button("Modify Item", "@upgrade"));
		
		switch (await Select())
		{
			case "@talk":
				Msg("Are you new here? Good to see you.");
				await StartConversation();
				break;
				
			case "@shop":
				Msg("Looking for a weapon?<br/>Or armor?");
				OpenShop("FerghusShop");
				return;
				
			case "@repair":
				Msg("If you want to have armor, kits or weapons repaired, you've come to the right place.<br/>I sometimes make mistakes, but I offer the best deal for repair work.<br/>For rare and expensive items, I think you should go to a big city. I can't guarantee anything.<br/><repair rate='90' stringid='(*/smith_repairable/*)' />");
				
				while(true)
				{
					var repair = await Select();
				
					if(!repair.StartsWith("@repair"))
						break;
					
					var result = Repair(repair, 90, "/smith_repairable/");
					if(!result.HadGold)
					{
						RndMsg(
							"Haha. You don't have enough Gold to repair that.",
							"Well, you have to bring more money to have it fixed.",
							"Do you have the Gold?"
						);
					}
					else if(result.Points == 1)
					{
						if(result.Fails == 0)
							RndMsg(
								"Alright! 1 Point repaired!",
								"Durability rose 1 point.",
								"Finished 1 point repair."
							);
						else
							Msg("Hmm... The repair didn't go well. Sorry...");
					}
					else if(result.Points > 1)
					{
						if(result.Fails == 0)
							Msg("Alright! It's perfectly repaired.");
						else
							// TODO: Use string format once we have XML dialogues.
							Msg("Repair is over.<br/>Unfortunately, I made " + result.Fails + " mistake(s).<br/>Only " + result.Successes + " point(s) got repaired.");
					}
				}
				
				Msg("<repair hide='true'/>By the way, do you know you can bless your items with the Holy Water of Lymilark?<br/>I don't know why but I make fewer mistakes<br/>while repairing blessed items. Haha.");
				Msg("Well, come again when you have items to fix.");
				break;
				
			case "@upgrade":
				Msg("Will you select items to be modified?<br/>The number and types of modifications are different depending on the items.<br/>When I modify them, my hands never slip or make mistakes. So don't worry. Trust me.<upgrade />");
				
				while(true)
				{
					var reply = await Select();
					
					if(!reply.StartsWith("@upgrade:"))
						break;
						
					var result = Upgrade(reply);
					if(result.Success)
						Msg("The modification you've asked for has been done.<br/>Is there anything you want to modify?");
					else
						Msg("(Error)");
				}
				Msg("If you have something to modify, let me know anytime.<upgrade hide='true'/>");
				break;
		}
		
		End("Goodbye, Ferghus. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg("I'm the blacksmith of Tir Chonaill. We'll see each other often, <username/>.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				GiveKeyword("windmill");
				Msg("The wind around Tir Chonaill is very strong. It even breaks the windmill blades.<br/>And I'm the one to fix them.<br/>Malcolm's got some skills,<br/>but I'm the one who deals with iron.");
				Msg("I made those extra blades out there just in case.<br/>When the Windmill stops working, it's really inconvenient around here.<br/>It's always better to be prepared, isn't it?<br/>");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "about_skill":
				GiveKeyword("skill_fishing");
				Msg("Hmm... Well, <username/>, since you ask,<br/>I might as well answer you. Let's see.<br/>Fishing. Do you know about the Fishing skill?");
				Msg("I'm not sure about the details, but<br/>I've seen a lot of people fishing up there.<br/>I'm not sure if fishing would be considered a skill, though.");
				Msg("From what I've seen, all you need is<br/>a Fishing Rod and a Bait Tin.");
				break;
				
			case "about_arbeit":
				Msg("What? Part-time job?<br/>There's nothing. You can come back later.");
				break;
				
			case "shop_misc":
				Msg("This is the Blacksmith's Shop. Surprisingly, many people think they are at the General Shop.");
				Msg("Let me tell you the biggest difference between Malcolm and me.<br/>He sells all kinds of stuff for your everyday life,<br/>but I, the best smithy in town, make metal stuff, you know.<br/>Like weapons, for example.");
				Msg("If you insist, I'll show you the way to the General Shop.<br/>Walk across the bridge, and go up the hill to the Square.");
				break;
				
			case "shop_grocery":
				Msg("The Grocery Store?<br/>The cute and lovely Caitin works there. Have you met her?<br/>She looks busy these days,<br/>but she cooks food for the people in town when she's got some time.");
				Msg("What? You want to go there? Go up to the Square.");
				break;
				
			case "shop_healing":
				Msg("I once hurt my hand while making a piece of armor.<br/>It really hurt and I went to Dilys. She cured it in a second!<br/>If you're not feeling well, you should go see her.<br/>Walk past the Square and you'll find her.");
				break;
			
			case "shop_bank":
				Msg("The Bank can be quite handy. They store your items, not just Gold.<br/>You'll probably use it a lot.<br/>The clerk at the Bank, Bebhinn, can help you in many ways if you get to know her.<br/>I'm not lying.");
				Msg("The Bank is near the Square. So you have to go up there first...<br/>Do you follow me?");
				break;
				
			case "shop_smith":
				Msg("Yes, this is the Blacksmith's Shop.");
				Msg("Alright, alright. You can't wait to equip yourself...<br/>But no need to rush.<br/>When you get an item, select it in your inventory, and then a slot for that item will blink.<br/>Then you just equip it. There's nothing no reason to rush.");
				Msg("What? You want to buy an item? Then you should have pressed 'Shop' instead<br/>of having this chat with me.");
				break;
				
			case "skill_instrument":
				Msg("Looks like you like music a lot,<br/>but I don't think I can help you with that.<br/>You know, I'm a blacksmith. I've never played any instruments before.");
				break;
			
			case "skill_counter_attack":
				Msg("Melee Counterattack skill?<br/>Have you talked to Ranald? He's at the School.<br/>What about Trefor?");
				Msg("When it comes to combat skills,<br/>you'd better talk with them. They will tell you useful stories.<br/>Chief Duncan was once a warrior.<br/>Perhaps he can give you some tips from his experience.");
				break;
				
			case "square":
				Msg("Haha. You must have missed it.<br/>It's nearly impossible to miss the Square.<br/>I think you need to keep your eyes open.");
				Msg("The Square farther within the town,<br/>right next to the big tree.<br/>You can see it even from that hill.");
				break;
				
			case "windmill":
				Msg("It's near the entrance of the town. Little lady Alissa works there.<br/>The Windmill pulls water from the stream to the reservoir.<br/>It's also used to grind crops.");
				Msg("Better be careful near the mill. It can be dangerous.");
				break;
				
			case "brook":
				Msg("Adelia Stream?<br/>That's flowing right there. Right in front of my shop.<br/>Have you seen any stream<br/>other than that one in this town?");
				break;
				
			case "shop_headman":
				Msg("You are looking for the Chief's House?<br/>His house is near the Square.<br/>Did you come straight down here<br/>without dropping by his place?");
				Msg("Then you didn't read the Quest Scroll?<br/>No, no. You should've read that.");
				Msg("Someone worked hard to create that scroll.<br/>Read it and do what it says. You've got nothing to lose.<br/>haha.");
				break;
				
			case "temple":
				Msg("Church is a bit far from here.<br/>Can you see it in your Minimap?<br/>No? Then I'll explain.<br/>First, go to the Square.");
				Msg("And then, look at your Minimap again.");
				Msg("Hmm... I think that's better than going through a long explanation.<br/>You're not upset, right?");
				break;
				
			case "school":
				GiveKeyword("farmland");
				Msg("Did you ask because you want to know the location of the School?<br/>Then I will give you an answer.<br/>Cross the bridge first,<br/>and there's a road. Just go to the left until you see the farmland.");
				Msg("If you pass the farmland, the School is very near you.<br/>The School gate is pretty big so you can't miss it.");
				Msg("When you get there, can you tell Ranald<br/>we should get a drink together?<br/>Lassar must not find out about it, alright?");
				break;
				
			case "farmland":
				Msg("The farmland? Then you're on the wrong side.<br/>Cross the Adelia Stream,<br/>and follow the path to the left.");
				break;
				
			case "shop_armory":
				Msg("...");
				Msg("Haha,<br/>are you joking around with me?<br/>I'm sure my Blacksmith's Shop comes before the Weapons Shop<br/>in the Information Memo.");
				break;
				
			case "shop_cloth":
				Msg("Oh, you want to get new clothes?<br/>There's no clothing shop or anything like that here,<br/>but you can buy some at the General Shop.");
				Msg("You know what? Many people in Erinn spend their fortune on clothes, accessories and stuff.<br/>I think it's because they can wear whatever they buy here<br/>without worrying about their age or size.");
				break;
				
			case "shop_bookstore":
				Msg("You must be interested in books.<br/>Sorry to say this, but... we don't have a bookstore in this town.<br/>If you want to buy books,<br/>go to Lassar. I think she sells some books.");
				Msg("But don't expect too much.<br/>She probably sells expensive spellbooks. What else could she have?");
				break;
				
			case "shop_goverment_office":
				Msg("Town Office?<br/>I think you expect too much<br/>from a small town like Tir Chonaill.");
				Msg("If you need some help,<br/>go see the Chief.");
				break;
				
			case "skill_fishing":
				Msg("Based on what I've seen, all you need<br/>is a Fishing Rod and a Bait Tin in each hand.");
				break;
				
			case "lute":
				Msg("Malcolm's General Shop sells lutes.<br/>He also sells... Um...<br/>What was that called? Ukul... something.");
				break;			
				
			default:
				RndMsg(
					"?",
					"*Yawn* I don't know.",
					"That's not my concern.",
					"I don't know, man. That's just out of my league.",
					"Haha. I have no idea."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class FerghusShop : NpcShopScript
{
	public override void Setup()
	{
		//--- Weapon tab -----------------------------
		//--------------------------------------------
		Add("Weapon", 40023);       //Gathering Knife
		Add("Weapon", 45001, 20);   //Arrow x20
		Add("Weapon", 45001, 100);  //Arrow x100
		Add("Weapon", 40022);       //Gathering Axe
		Add("Weapon", 45002, 50);   //Bolt x50
		Add("Weapon", 45002, 200);  //Bolt x200
		Add("Weapon", 40027);       //Weeding Hoe
		Add("Weapon", 40003);       //Short Bow
		Add("Weapon", 40026);       //Sickle
		Add("Weapon", 40006);       //Dagger
		Add("Weapon", 40005);       //Short Sword
		Add("Weapon", 40025);       //Pickaxe
		Add("Weapon", 40179);       //Spiked Knuckle
		Add("Weapon", 40007);       //Hatchet
		Add("Weapon", 40024);       //Blacksmith Hammer
		Add("Weapon", 40244);       //Bear Knuckle
		Add("Weapon", 40180);       //Hobnail Knuckle
		Add("Weapon", 40745);       //Basic Control Bar
		Add("Weapon", 40841);       //Spiral Shuriken
		Add("Weapon", 46001);       //Round Shield
		
		//--- Shoes and Gloves tab -------------------
		//--------------------------------------------			
		Add("Shoes Gloves", 16004); //Studded Bracelet
		Add("Shoes Gloves", 16008); //Cores' Thief Gloves
		Add("Shoes Gloves", 16000); //Leather Gloves
		Add("Shoes Gloves", 17021); //Lorica Sandles
		Add("Shoes Gloves", 17014); //Leather Shoes
		Add("Shoes Gloves", 17001); //Ladies Leather Boots
		Add("Shoes Gloves", 17005); //Hunter Boots
		Add("Shoes Gloves", 17015); //Combat Shoes
		Add("Shoes Gloves", 17016); //Field Combat Shoes
		Add("Shoes Gloves", 17020); //Thief Shoes
		Add("Shoes Gloves", 16014); //Lorica Gloves
		
		//--- Helmet tab -----------------------------
		//--------------------------------------------
		Add("Helmet", 18503);       //Cuirassier Helm
		
		//--- Armor tab ------------------------------
		//--------------------------------------------
		Add("Armor", 14001);        //Light Leather Mail (F)
		Add("Armor", 14010);        //Light Leather Mail (M)
		Add("Armor", 14004);        //Cloth Mail
		Add("Armor", 14008);        //Full Leather Armor Set (F)
		Add("Armor", 14003);        //Studded Cuirassier
	}
}
