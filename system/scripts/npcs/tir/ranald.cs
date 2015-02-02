//--- Aura Script -----------------------------------------------------------
// Ranald, Combat Instructor
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class RanaldBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_ranald");
		SetRace(10002);
		SetBody(upper: 1.1f);
		SetFace(skinColor: 20);
		SetStand("human/male/anim/male_natural_stand_npc_ranald02", "human/male/anim/male_natural_stand_npc_ranald_talk");
		SetLocation(1, 4651, 32166, 195);

		EquipItem(Pocket.Face, 4900, 0xF88B4A);
		EquipItem(Pocket.Hair, 4154, 0x4D4B53);
		EquipItem(Pocket.Armor, 15652, 0xAC9271, 0x4D4F48, 0x7C6144);
		EquipItem(Pocket.Shoe, 17012, 0x9C7D6C, 0xFFC9A3, 0xF7941D);
		EquipItem(Pocket.LeftHand1, 40012, 0xDCDCDC, 0xC08B48, 0x808080);
		
		AddPhrase("I need a drink...");
		AddPhrase("I guess I drank too much last night...");
		AddPhrase("I need a nap...");
		AddPhrase("I should drink in moderation...");
		AddPhrase("I should sharpen my blade later.");
		AddPhrase("It's really dusty here.");
		AddPhrase("What's with the hair styles of today's kids?");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Ranald.mp3");
	
		await Intro(
			"From his appearance and posture, there is no doubt that he is well into middle age, but he is surprisingly well-built and in good shape.",
			"Long fringes of hair cover half of his forehead and right cheek. A strong nose bridge stands high between his shining hawkish eyes.",
			"His deep, low voice has the power to command other people's attention."
		);
		
		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"), Button("Get Ciar Beginner Dungeon Pass", "@ciarpass"));
		
		switch(await Select())
		{
			case "@talk":
				//Msg("Hmm...<br/>Nice to meet you.");
				Msg("My name is Ranald."); // "<br/>I assume you remember my name?"
				await StartConversation();
				break;
				
			case "@shop":
				Msg("Tell me if you need a Quest Scroll.<br/>Working on these quests can also be a good way to train yourself.");
				Msg("(Unimplemented)");
				break;
				
			case "@upgrade":
				Msg("Hmm... You want me to modify your item? You got some nerve!<br/>Ha ha. Just joking. Do you need to modify an item? Count on Ranald.<br/>Pick an item to modify.<br/>Oh, before that. Types or numbers of modifications are different depending on what item you want to modify. Always remember that.");
				Msg("(Unimplemented)");
				break;
				
			case "@ciarpass":
				GiveItem(63139); // Ciar Beginner Dungeon Pass
				Notice("Recieved Ciar Beginner Dungeon Pass from Ranald.");
				Msg("Ok, here's the pass.<br/>You can ask for it again if you need it.<br/>That doesn't mean you can fill up the iventory with a pile of passes.");
				break;
		}
		
		End("Goodbye, Ranald. I'll see you later!");
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("school");
				Msg("Hello, there. I teach combat skills at the School in Tir Chonaill.<br/>If you're interested, talk to me with the 'Classes and Training' keyword.");
				Msg("Hey, hey... This is not free. You'll need to pay tuition for my classes...<br/>");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				GiveKeyword("complicity");
				Msg("Hmm... Did you hear the news?<br/>Ferghus can't stop smiling these days.<br/>I heard his arrow sales have jumped up lately.");
				Msg("It seems like Trefor received a huge gift from Ferghus.<br/>People are assuming that Trefor is helping Ferghus with something.");
				//Msg("A dinner with Ferghus usually leads to a bit of drinking at the end.<br/>You know he loves to drink, right? As a matter of fact, I like to drink, too. Hahaha...");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				//If player knows Windmill
				Msg("How was the Windmill skill? Was it of any help?<br/>You will one day become a great warrior<br/>as long as you remain an ardent student eager for training just like you are now.");
				break;

			case "about_study":
				Msg("This is not the time for class. Come back tomorrow morning.");
				//Msg("Are you interested in the combat class?<br/>If you're sick and tired of battles run by simple mouse clicks,<br/>my class is definitely worth spending some time and money on.");
				break;

			case "shop_misc":
				GiveKeyword("shop_armory");
				Msg("You're not going to the General Shop for weapons, are you?<br/>For weapons, you would need to go to the Blacksmith's Shop.<br/>");
				break;

			case "shop_grocery":
				Msg("The Grocery Store is located near the Square. Right now, you're at the School.");
				break;

			case "shop_healing":
				Msg("There's no way to avoid getting wounded while battling, and some of the wounds can't be healed by magic or basic treatment.<br/>If the injuries keep piling up, your body will get noticeably weaker.");
				Msg("That's why it's important to receive proper treatments for your wounds.");
				break;

			case "shop_inn":
				Msg("In my time, inns were not a daily sight, so I had to pitch a tent and make the best of it.<br/>Compared to that, you're having it way too easy here.");
				break;

			case "shop_bank":
				Msg("It's a good habit to deposit your items at the Bank whenever possible.<br/>If you run out of inventory space,<br/>you won't be able to pick up or carry additional items.<br/>If that happens, you would be doing all the work and someone else would be reaping the rewards.<br/>You don't want that, do you?");
				break;

			case "shop_smith":
				Msg("The Blacksmith's Shop is on the bank of the Adelia Stream, located close to the town entrance.<br/>Tell Ferghus I sent you, and he'll take good care of you.");
				break;

			case "skill_instrument":
				GiveKeyword("church");
				Msg("You want to learn the Instrument Playing skill?<br/>Hmm... Did you forget that I am a combat instructor?<br/>I can't believe you are asking me for that. I am a little disappointed");
				Msg("Ha ha... Don't be so disheartened about what I said, though.<br/>I didn't know you would be easily affected by my words. Hahaha.<br/>You're just like Malcolm when he was a kid.");
				Msg("OK, about the Instrument Playing skill...<br/>Go ask Priestess Endelyon at the Church about it.<br/>I'm sure she will teach you well.");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_grocery");
				Msg("You want to make clothes for yourself?<br/>Hmm... Why would you come to the School to ask me, a combat instructor,<br/>how to make clothes?<br/>I just don't get it. But, anyway...");
				Msg("You have asked me a question, and as a teacher, I feel obligated to answer it.<br/>Talk to Caitin at the Grocery Store.<br/>She knows more about that skill than anyone else.");
				Msg("From what I've heard, most of the clothes at Malcolm's shop<br/>were either designed or tailored by her...");
				break;

			case "skill_smash":
				Msg("Smash skill...<br/>More than anything, balancing your power is the most important thing for this skill.<br/>In that respect, Ferghus knows a lot more than I do.<br/>Go learn from him.");
				break;

			case "skill_counter_attack":
				Msg("Counterattack skill...<br/>Simply knowing the skill won't help you.<br/>It's all about timing.");
				Msg("This skill cannot be mastered unless you practice it in a real combat situation, which means...<br/>You'll need to get hit first.");
				Msg("I could show you a demonstration right now and teach you this skill, but... I'll probably break you in half.<br/>You should go to Trefor instead and ask him to show you the Counterattack skill.<br/>It would be a lot safer for you. Go now.<br/>");
				break;

			case "square":
				Msg("Follow the path up in front of the School<br/>and you will see the Square.");
				Msg("The Square is usually the place to meet for many people,<br/>talking about various topics and exchanging information.");
				Msg("So, if you see any travelers there, don't hesitate to say hello.<br/>It never hurts to make friends.");
				break;

			case "pool":
				Msg("The reservoir? Go up north a little bit, and that's where you'll find it.<br/>The water in the reservoir comes from the Adelia Stream.<br/>We use the Windmill to get water up from the stream to fill the reservoir.<br/>It's critical for irrigating the farmland.");
				break;

			case "farmland":
				GiveKeyword("shop_grocery");
				Msg("The farmland?<br/>Well, it's right in front of the School. You didn't see it?<br/>If you're talking about another farmland,<br/>there's a small one next to Caitin's Grocery Store, but...");
				Msg("Don't step on those crops!<br/>Let the scarecrow take care of it.");
				break;

			case "windmill":
				GiveKeyword("shop_inn");
				Msg("Hmm... are you looking for the windmill?<br/>I see... The windmill here is well worth a visit. It's picturesque.");
				Msg("If you want to see the windmill up close, go near the Inn.");
				break;

			case "brook":
				Msg("Hmm... you want to know how to reach the Adelia Stream?<br/>It will take some time to get there...<br/>It's not easy to explain where it is...");
				Msg("Walk outside the School and follow along the farmland.<br/>After a while, you will see a small stream and a bridge.<br/>Well, that's the Adelia Stream.");
				break;

			case "shop_headman":
				Msg("The Chief's House is at the top of the hill with the big tree,<br/>north of the Square.<br/>You can see the whole town from up there.");
				break;

			case "temple":
				Msg("The Church?<br/>It's very close from here.<br/>Just take a few steps up north and you'll see it.");
				Msg("Hmm... Hey,<br/>can you do me a favor?<br/>Can you go to the Church and see what Priestess Endelyon is doing?<br/>It's nothing, really. I'm just wondering what she's doing, you know...");
				Msg("But don't let Priest Meven see you, okay?");
				break;

			case "school":
				Msg("Tir Chonaill is such a small town that establishing a school was almost unnecessary.<br/>But I thought it was necessary that we establish a place for our children to learn the traditions and wisdom of our forefathers.<br/>Otherwise, our proud heritage would be been lost after only a few generations.<br/>We could not let that happen.");
				Msg("Fortunately, Lassar came back from her studies around that time, and decided to help out.<br/>That's how this School came to be.");
				break;

			case "skill_windmill":
				Msg("Do you want to know more about the Windmill skill?<br/>You should go to Dunbarton then.");
				Msg("The combat instructor in Dunbarton is really beautiful... I mean, ummm, she's a really good teacher.<br/>Anyway, go to Dunbarton and look for her.");
				Msg("Just go all the way down south from Tir Chonaill. That's where Dunbarton is.<br/>One thing before you leave. You'd better stay away from that man Tracy, hanging around the Logging Camp.<br/>He's just not a good person to be with.");
				break;

			case "skill_campfire":
				Msg("Hmm... A warrior who also knows the Campfire skill.<br/>I guess that could be helpful.<br/>However, you can end up spending precious time chopping wood<br/>instead of training for your combat skills...");
				Msg("You may end up as a lumberjack, not as a warrior...");
				Msg("...<br/>You don't want to be like Tracy when you grow up, right?");
				Msg("A warrior must always practice self-discipline.<br/>Don't stay around a campfire too long and waste precious training time.");
				break;

			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("Hmm... As far as I know, there are no restaurants in Tir Chonaill.<br/>If you're looking for something to eat, then talk to Caitin.");
				Msg("She spends most of her time working at the Grocery Store,<br/>so why don't you go there and say hi to her?");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("You can't find the Weapons Shop?<br/>Hmm... Well, this is very heartbreaking to say, but there is no Weapons Shop in this town.<br/>You will need to go to the Blacksmith's Shop if you need a weapon.");
				Msg("All weapons and armor are made at the Blacksmith's Shop by Ferghus himself.");
				Msg("There's not much of a variety there,<br/>but talk to Ferghus if you need something.");
				Msg("Perhaps it's better this way. You know that weapons and armor won't make you a real warrior, don't you?<br/>If you depend on them too much, all your training may end up in vain.");
				break;

			case "shop_bookstore":
				Msg("A bookstore? You won't find one in this town.<br/>If you really want to buy some books,<br/>it would be better for you to go to another town.");
				Msg("A friend of mine who's also a combat instructor told me<br/>that Dunbarton has a bookstore.");
				Msg("Hmm... reading books is certainly a good thing,<br/>but spending too much time on it won't help you get your job done.<br/>I suggest you read in moderation.");
				Msg("If you spend too much time on books,<br/>you can end up a scrawny nerd like that Malcolm guy.");
				break;

			case "shop_government_office":
				Msg("Hmm.... Probably a big town or city may have it,<br/>since they are usually under the lordship of Aliech.<br/>But there's no town office around here.");
				Msg("When the descendants of Ulaid built Tir Chonaill and its neighborhood,<br/>they wanted no one to govern us but ourselves.");
				Msg("If you'd like to know more about our town,<br/>why don't you visit the Chief's House?<br/>You can go and see him in case you've lost something.<br/>The Chief usually keeps it for a while.");
				break;

			case "graveyard":
				GiveKeyword("shop_headman");
				Msg("The graveyard? Hmm...<br/>There is one located over the hill behind the Chief's House but...");
				Msg("If I were you,<br/>I would stay away from that place.<br/>It's not right to cause a commotion literally on top of the dead.<br/>It is simply not the right thing to do.");
				Msg("I want you to respect the dead,<br/>and let them rest in peace.");
				break;

			case "complicity":
				Msg("Hmm... as a strong advocate of ethics and education,<br/>I don't think it's the right thing to do...");
				Msg("But I think sometimes it's necessary to open our minds<br/>and be tolerant of such things.");
				break;

			case "lute":
				GiveKeyword("shop_misc");
				Msg("Do you want to know where to buy a lute?<br/>You must have seen some people carrying them around, didn't you?");
				Msg("You know, you could have just gone over to one of them and ask where they bought it.");
				Msg("Go to Malcolm at the General Shop if you want one of those.<br/>lute is the cheapest instrument you can buy,<br/>so I think you can easily afford it.");
				Msg("Don't feel sorry for yourself for not having enough money.<br/>You need to start doing some part-time jobs or party quests, so you can earn some money to buy something you like.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog is the land of paradise everyone in Erinn dreams of.<br/>People say there is no pain or suffering in Tir Na Nog, but only eternal life and happiness.");
				Msg("Personally, I have never been there,<br/>and I don't really know what it's like...");
				Msg("Still,<br/>I don't believe Endelyon would say anything false.<br/>She is not the type of person that lies.");
				break;

			case "mabinogi":
				Msg("Hmm... Mabinogi is a song written and sung to praise mighty warriors.<br/>It's a song about the story of fearless warriors who fought against<br/>the evil Fomors in endless battles a long time ago.");
				Msg("You know, Mabinogi is like a living, breathing creature.<br/>It keeps growing and evolving,<br/>its contents revised, extended, sung and heard through endless generations.");
				Msg("So, who knows? If you become a great warrior yourself,<br/>you may someday have a song dedicated to your heroic efforts.<br/>People will write and sing about you...");
				break;

			case "musicsheet":
				Msg("Music Score?");
				Msg("Why... you want to write a song?<br/>Ask Malcolm if your creativity extends to composition.<br/>He was drawing on some sheets of paper to write some scores the last time I saw him...");
				Msg("How he keeps his concentration while doodling on a sheet of paper is a complete mystery to me.");
				break;

			default:
				RndMsg(
					"Hmm... Actually, I forgot my lines... Haha.",
					"I am not very interested in that.",
					"I haven't paid much attention to it, especially on a topic like that.",
					"Well, I don't really know...",
					"You know I've been busy..."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}
