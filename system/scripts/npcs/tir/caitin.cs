//--- Aura Script -----------------------------------------------------------
// Caitin, the Grocery Store Owner in Tir Chonaill
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class CaitinBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_caitin");
		SetRace(10001);
		SetBody(height: 1.03f);
		SetFace(skinColor: 15, eyeType: 82, eyeColor: 27, mouthType: 43);
		SetStand("human/female/anim/female_natural_stand_npc_Caitin_new");
		SetLocation(5, 1831, 1801, 59);

		EquipItem(Pocket.Face, 3900, 0x00F3B14E, 0x00FBB8AC, 0x00BF921E);
		EquipItem(Pocket.Hair, 3142, 0x00723A2B, 0x00723A2B, 0x00723A2B);
		EquipItem(Pocket.Armor, 15654, 0x006A9050, 0x00F4D6A9, 0x002A2A2A);
		EquipItem(Pocket.Shoe, 17284, 0x002A2A2A, 0x00000000, 0x00000000);

		AddPhrase("*Yawn*");
		AddPhrase("Hmm... Sales are low today... That isn't good.");
		AddPhrase("I am a little tired.");
		AddPhrase("I have to finish these bills... I'm already behind schedule.");
		AddPhrase("I must have had a bad dream.");
		AddPhrase("It's about time for customers to start coming in.");
		AddPhrase("My body feels stiff all over.");
		AddPhrase("These vegetables are spoiling...");
	}
	
	protected override async Task Talk()
	{
		await Intro(
			"A young lady pouring flour into a bowl smiles at you as you enter.",
			"Her round face is adorably plump and her eyes shine brightly.",
			"As she wipes her hands and walks toward you, you detect the faint scent of cookie dough and flowers."
		);
		
		Msg("What can I do for you?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));
		
		switch (await Select())
		{
			case "@talk":
				Msg("I think this is the first time we've met. Nice to meet you!");
				await StartConversation();
				break;
				
			case "@shop":
				Msg("Welcome to the Grocery Store.<br/>There is a variety of fresh food and ingredients for you to choose from.");
				OpenShop("GroceryShop");
				return;
		}
		
		End();
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("shop_grocery");
				Msg("My grandmother named me.<br/>I work here at the Grocery Store, so I know one important thing.<br/>You have to eat to survive!<br/>Food helps you regain your Stamina.");
				Msg("That doesn't mean you can eat just anything.<br/>You shouldn't have too much greasy food<br/>because you could gain a lot of weight.");
				Msg("Huh? You have food with you but don't know how to eat it?<br/>Okay, open the Inventory and right-click on the food.<br/>Then, click \"Use\" to eat.<br/>If you have bread in your Inventory, and your Stamina is low,<br/>try eating it now.");
				ModifyRelation(Random(2), 0, Random(2));
				break;
			
			case "rumor":
				GiveKeyword("brook");
				Msg("Do you know anything about the Adelia Stream?<br/>The river near the Windmill is the Adelia Stream.<br/>");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "about_skill":
				Msg("I want to bake some bread for Duncan but I'm out of wheat flour.<br/>Could you lend me a hand? In return, I'll tell you how to make wheat flour.<br/>Wait outside and an owl will deliver a message with details on making wheat flour.");
				break;
				
			case "about_arbeit":
				Msg("I'm sorry... This isn't the right time for a part-time job.<br/>Please come back later.");
				break;
				
			case "shop_misc":
				Msg("The General Shop? It's the building in front of here. You must have missed the shop's sign.<br/>Hehe.... Nothing to be embarrassed about. Happens all the time.");
				break;
			
			case "shop_grocery":
				Msg("Yes, this is the Grocery Store. How many times are you going to ask?<br/>Are you implying that I look like a Grocery Store owner?  That I'm...fat?");
				Msg("...<br/>I'm sorry... That wasn't fair.<br/>Please. Come by anytime you need something to eat.<br/>Open the Trade window to take a look around.");
				break;
				
			case "shop_healing":
				Msg("Eat well. Stay healthy. Then you won't need the Healer's help.<br/>A nutritious, home-cooked meal beats medicine any day!<br/>Would Dilys be upset about what I just said? Hehe.");
				break;
			
			case "shop_bank":
				Msg("Sometimes people forget what they deposited at the Bank.<br/>Please keep that in mind!<br/>I say this because It often happens to me... Hehe.<br/>Bebhinn never reminds you of the items either... Sigh...");
				break;
				
			case "shop_smith":
				Msg("Ferghus at the Blacksmith's Shop loves to drink.<br/>He often hangs out with Ranald from the school for a drink.");
				break;
				
			case "skill_instrument":
				Msg("Playing an instrument?<br/>I saw Priestess Endelyon play an organ at the Church before.<br/>Why don't you go and talk to her?");
				break;
				
			case "skill_counter_attack":
				Msg("Melee Counterattack? Sounds like a combat skill to me.<br/>Why don't you ask Ranald at the School?<br/>He teaches combat skills.");
				break;
				
			case "square":
				Msg("The Square? It's right in front of you!<br/>...<br/>...<br/>Um, do you have something else to say?");
				break;
				
			case "brook":
				Msg("The Adelia Stream?<br/>Go straight down the road just out front.<br/>It's easy to find because it's near the Inn.<br/>I used to take baths there when I was young.<br/>Hey!  What kind of thoughts are you having?!");
				break;
				
			case "shop_headman":
				Msg("Chief Duncan's house is just over the hill.<br/>Please give him my best regards.");
				Msg("You should go visit him<br/>especially if you are a newcomer.");
				break;
				
			case "temple":
				Msg("You can get to the Church by taking the downhill road on the right.<br/>You will find Priest Meven and Priestess Endelyon there.<br/>Priestess Endelyon is looking for people who are willing to work at the Church.<br/>If you need a job, please go and ask her.");
				break;
				
			case "school":
				Msg("You'll find the School a bit further down from the Church.<br/>You can't miss it because the building is quite big<br/>and there is a strange plant growing near it.");
				break;
				
			case "shop_armory":
				Msg("A Weapons Shop?<br/>I don't think there is a shop just for weapons...<br/>Oh yea!  Go to Ferghus's Blacksmith.<br/>I saw him making all sorts of swords and hammers recently.");
				break;
				
			case "shop_cloth":
				Msg("I usually make my own clothes<br/>but sometimes I purchase them at Malcolm's General Shop.<br/>Do you need new clothes?");
				break;
				
			case "shop_bookstore":
				Msg("Everyone is so busy with work,<br/>that it's almost impossible to sit down and just read.<br/>The only books you can buy around here are probably<br/>the magic books that Lassar sells at the School.");
				Msg("Some villagers give books as gifts to travelers passing through this village.<br/>Could be tempting if you're a bookworm.");
				break;
				
			case "shop_goverment_office":
				Msg("The Town Office? Huh?<br/>Er, if you are looking for someone who takes care of town affairs,<br/>go and see the Chief.");
				break;
				
			case "lute":
				Msg("Lute...? Do you mean that small stringed instrument?<br/>I saw Malcolm selling them at the General Shop.<br/>If you plan to buy one, the General Shop is the place to go.");
				break;
				
			default:
				RndMsg(
					"Can we change the subject?",
					"I don't have much to say about that.",
					"Never heard of that before.",
					"Well, I really don't know.",
					"Did you ask everyone else the same question?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}
