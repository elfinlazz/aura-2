//--- Aura Script -----------------------------------------------------------
// Dilys in Tir Chonaill
//--- Description -----------------------------------------------------------
// Healer
//---------------------------------------------------------------------------

public class DilysScript : NpcScript
{
	public override void Load()
	{
		SetName("_dilys");
		SetRace(10001);
		SetBody(height: 0.9f, upper: 1.2f);
		SetFace(skinColor: 17, eyeType: 3, eyeColor: 27, mouthType: 48);
		SetStand("human/female/anim/female_natural_stand_npc_Dilys_retake");
		SetLocation(6, 1107, 1050, 195);

		EquipItem(Pocket.Face, 3908, 0x0058B49E, 0x00365C72, 0x00D6EEF5);
		EquipItem(Pocket.Hair, 3141, 0x00633C31, 0x00633C31, 0x00633C31);
		EquipItem(Pocket.Armor, 15653, 0x00FFFFFF, 0x0061854B, 0x00FFFFFF);
		EquipItem(Pocket.Glove, 16098, 0x0061854B, 0x00000000, 0x00000000);
		EquipItem(Pocket.Shoe, 17285, 0x00E8E8E8, 0x00000000, 0x00000000);

		AddPhrase("It's such a hassle to get all those ingrediants for just one meal.");
		AddPhrase("Men are all the same.");
		AddPhrase("Perhaps I should order a safe this month.");
		AddPhrase("Should I go to the market?");
		AddPhrase("I wish I could see the stars.");
		AddPhrase("Should I go to the market?");
	}
	
	protected override async Task Talk()
	{
		await Intro(
			"A tall, slim lady tinkers with various ointments, herbs, and bandages.",
			"She looks wise beyond her years, but it might just be the healer's dress",
			"and neatly combed hair."
		);

		Msg("Welcome to the Healer's House.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Get Treatment", "@healerscare"), Button("Heal Pet", "@petheal"));
		
		switch(await Select())
		{
			
			case "@talk":
				Msg("Welcome to the Healer's House.");
				//Msg("Have you been here before?<br/>You look familiar.");
				//Msg("You're back.<br/>Nice to see you again, <username/>.<br/>");
				await StartConversation();
				break;
				
			case "@shop":
				Msg("What potion do you need?");
				OpenShop("DilysShop");
				return;
			
			case "@healerscare":
				if (Player.Life == Player.LifeMax)
				{
					Msg("You don't have a mark on you! You really shouldn't pretend to be sick... There are people out there that need my help!");
				}
				else
				{
					Msg("Goodness, <username/>! Are you hurt? I must treat your wounds immediately.<br/>I can't understand why everyone gets injured so much around here...<br/>The fee is 90 Gold but don't think about money right now. What's important is that you get treated.", Button("Receive Treatment", "@gethealing"), Button("Decline", "@end"));
					if(await Select() == "@gethealing")
					{
						if (Gold >= 90)
						{
							Gold -= 90;
							Player.FullLifeHeal();
							Player.Mana = Player.ManaMax;
							Msg("Good, I've put on some bandages and your treatment is done.<br/>If you get injured again, don't hesitate to visit me.");
							if (!Player.Skills.Has(SkillId.FirstAid))
							{
								Player.Skills.Give(SkillId.FirstAid, SkillRank.Novice);
								Msg("I see you haven't learned the First Aid skill yet.<br/>Since you can't come to me every time you get hurt,<br/>you should learn how to apply a bandage to yourself.<p/>I will teach you the First Aid skill.<br/>This skill requires bandages<br/>so always keep them handy in your inventory.");
							}
						}
						else
						{
							Msg("Oh, hm...you're short on money.<br/>I need the gold to pay for the bandages and medince you need...<br/>Why don't you go do some part-time jobs and then come back?");
						}
					}
				}
				break;
			
			case "@petheal":
				Msg("You may want to summon your animal friend first.<br/>If you don't have a pet, then please don't waste my time.");
				break;
		}
		
		End();
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("shop_healing");
				Msg("A healer's job is to treat sick people.<br/>Don't hesitate to come to me if you ever feel sick.");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "rumor":
				GiveKeyword("graveyard");
				Msg("It was hard for you to get here, wasn't it? I bet if I were a little closer to the Square<br/>you would've come earlier. Hehe...<br/>Truthfully, it is kind of scary being next to the graveyard.<p/>At first I thought about opening the Healer's House near the Square<br/>but Duncan advised me that this place would be better for business.<br/>Actually, I haven't had many patients.<br/>Only people who come to hunt spiders and...Trefor, who stores his goods here...");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "about_skill":
				GiveKeyword("skill_counter_attack");
				Msg("Skills...<br/>Oh! A while ago, Ranald defeated a fox<br/>that had appeared in town using some skill or other... What was that called...?<br/>I think it's called  Melee... Counter... Counterattack? Something like that...");
				break;
				
			case "about_arbeit":
				Msg("It's not time to start work yet.<br/>Can you come back and ask for a job later?");
				//Msg("Do you need some work to do?<br/>If you want, you can help me here.<br/>The pay is not great, but I will definitely pay you for your work.<br/>The pay also depends on how long you've worked for me.<br/>Would you like to try?);
				break;
				
			case "shop_misc":
				Msg("Follow the road all the way down until you see the Square.<br/>The building on the left is the General Shop.<br/>You can't miss it.<br/>And please say hello to Malcolm for me.");
				break;
				
			case "shop_grocery":
				Msg("You know Caitin at the Grocery Store? Her mother can't see well.<br/>I want to help, but there is nothing I can do.<br/>At times like this, I get so frustrated that I wonder<br/>if I made the right choice by becoming a healer.");
				break;

			case "shop_healing":
				Msg("I must not fit your image of a healer.  Everyone says that.<br/>This is my house...<br/>But Trefor uses it as his own storehouse.<br/>I just don't understand men...");
				break;

			case "shop_inn":
				Msg("The Inn is near the town entrance.<br/>When you get to the door of the Inn<br/>you will see Nora greeting guests.<br/>But If you don't feel better after resting at the Inn, then come back to me.");
				break;
				
			case "shop_bank":
				Msg("Bebhinn, the bank clerk, is a pleasant lady.  She is such a free spirit, and funny too...<br/>Something about her... She's so diligent and sharp.<br/>You act like you don't believe me.");
				break;
				
			case "shop_smith":
				Msg("The Blacksmith's Shop is on the other side of the town.<br/>Just across the bridge.<br/>If you go there, please check to see if Ferghus is okay.<p/>I am worried about him since he likes to drink so much.");
				break;

			case "skill_range":
				GiveKeyword("school");
				Msg("You should not ask such questions to a healer...<br/>Why don't you go to the School and talk to Ranald?");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("Come on, stop teasing.<br/>How can a fragile lady like me use such a powerful skill?<br/>Maybe, if I use it on Lassar... Haha!<br/>Teacher Ranald is at the School.<br/>Try talking to him about it.");
				break;
				
			case "square":
				Msg("Go straight down the road to get to the Square.<br/>The Square is the most crowded place in town.<br/>It's lively but a little too hectic and noisy for me...");
				break;

			case "pool":
				GiveKeyword("shop_misc");
				Msg("You can get water from the reservoir,<br/>but you will need a bottle or a bowl.<br/>You could probably buy something at Malcolm's General Shop.");
				break;
				
			case "farmland":
				GiveKeyword("school");
				Msg("The farmland is near the School.<br/>Farmers grow crops like wheat or barley,<br/>but I don't usually go there.<br/>You will need to check it out for yourself.");
				break;

			case "windmill":
				GiveKeyword("shop_smith");
				Msg("The Windmill is near the Blacksmith's Shop.<br/>If you want to grind wheat or grain, talk to Alissa first.<br/>She's in front of the Windmill.<br/>Be careful not to get hurt by the mill.");
				break;

			case "brook":
				Msg("You must be talking about the creek at the town's entrance.<br/>The creek has a legend about a priestess<br/>who protected this town against evil creatures.<br/>How did that story go? I can't remember...");
				break;
				
			case "shop_headman":
				Msg("The Chief's House is not far from here. It's a short walk.<br/>Follow the path around the hill near the Square,<br/>and you will find it.<br/>");
				break;
				
			case "temple":
				Msg("The Church? It's a place where they worship Lymilark.<br/>I didn't realize it when I was a kid,<br/>but the fact that there's a church in a small town like this<br/>must mean that people's faith in Lymilark is fervent...<br/>It amazes me sometimes.");
				break;

			case "school":
				GiveKeyword("bank");
				Msg("The School is nearby.<br/>See that road by the Bank? Follow it down the hill.<br/>Ah, I forgot Lassar is at the School.<br/>She claims that she is a teacher of magic,<br/>but actually, she can't even do half of what she teaches.");
				break;
				
			case "skill_campfire":
				GiveKeyword("shop_inn");
				Msg("Seems like a lot of people are using the Campfire skill lately...<br/>You can go ask Piaras at the Inn. He traveled around before settling in this town.<br/>He probably knows all sorts of skills.<p/>Oh? Piaras already told you everything he knows?<br/>Hmm... Then, er, perhaps Shepherd Deian might know something?");
				break;

			case "graveyard":
				Msg("The graveyard? Just go up the hill. <be/>But, really? Asking about the graveyard at the Healer's House?<br/>Haha, you're quite strange...");
				break;
				
			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("Restaurant?<br/>There are no restaurants in this town. But you could go to the Grocery Store.<br/>Go down the road and you will see a shop with a chef sign.<br/>You will find Caitin there.");
				break;
				
			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("Hmm... We don't have a shop that sells weapons here...<br/>But you could go and talk to Ferghus.<br/>His Blacksmith's Shop is past the Inn, just across the bridge.");
				break;

			case "shop_bookstore":
				Msg("Yes, I am also frustrated that there are no bookstores in this town...<br/>When I need to books I always have to ask someone who is leaving town<br/>to purchase them for me.<p/>Since I don't know when patients might need my help,<br/>I cannot leave this place...");
				break;
				
			case "shop_goverment_office":
				Msg("Huh? A Town Office? Here?<br/>I don't think so...<br/>In fact, this town is not under the control of the Kingdom of Aliech...<p/>Are you looking for an item you lost during your adventures?<br/>The Chief takes care of all town-related issue. Why don't you go see him?");
				break;
				
			default:
				RndMsg(
					"I don't know... I'm sorry.",
					"Did you ask others about this as well?",
					"What are you talking about?",
					"Did they say they didn't know about it either?<br/>Well...",
					"Eh?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}
