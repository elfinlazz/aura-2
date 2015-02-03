//--- Aura Script -----------------------------------------------------------
// Piaras, the Inn Keeper in Tir Chonaill
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class PiarasBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_piaras");
		SetRace(10002);
		SetBody(height: 1.28f, weight: 0.9f, upper: 1.2f);
		SetFace(skinColor: 22, eyeType: 1);
		SetStand("human/male/anim/male_natural_stand_npc_Piaras");
		SetLocation(7, 1344, 1225, 182);

		EquipItem(Pocket.Face, 4900, 0x00BDAF73, 0x000AB0D4, 0x00E50072);
		EquipItem(Pocket.Hair, 4004, 0x003F4959, 0x003F4959, 0x003F4959);
		EquipItem(Pocket.Armor, 15003, 0x00355047, 0x00F6E2B1, 0x00FBFBF3);
		EquipItem(Pocket.Shoe, 17012, 0x009C936F, 0x00724548, 0x0050685C);
		
		AddPhrase("Ah... The weather is just right to go on a journey.");
		AddPhrase("Do you ever wonder who lives up that mountain?");
		AddPhrase("Hey, you need to take your part-time job more seriously!");
		AddPhrase("I haven't seen Malcolm around here today. He used to come by every day.");
		AddPhrase("Nora, where are you? Nora?");
		AddPhrase("The Inn is always bustling with people.");
	}
	
	protected override async Task Talk()
	{
		await Intro(
			"His straight posture gives him a strong, resolute impression even though he's only slightly taller than average height.",
			"Clean shaven, well groomed hair, spotless appearance and dark green vest make him look like a dandy.",
			"His neat looks, dark and thick eyebrows and the strong jaw line harmonized with the deep baritone voice complete the impression of an affable gentleman."
		);
		
		Msg("Welcome to my Inn.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("Nice to meet you.");
				await StartConversation();
				break;
				
			case "@shop":
				Msg("May I ask what you are looking for?");
				OpenShop("PiarasShop");
				return;
		}
		
		End("Goodbye, Piaras. I'll see you later!");
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch(keyword)
		{
			case "personal_info":
				GiveKeyword("shop_inn");
				Msg("I might sound too proud,<br/>but I put a lot of effort into making this place as comfortable for my guests as possible.<br/>Please visit us when you need a place to stay.<br/>");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "rumor":
				Msg("Why don't you talk to others in town? There's a good spot to meet people. The Town Square is right up this way. I suggest you try there first.");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "about_skill":
				if (Player.Skills.Has(SkillId.Campfire))
				{
					Msg("Ha ha. Now you know how to use the Campfire skill.<br/>It's something I didn't want to teach you, to be honest,<br/>but I am impressed that you have mastered it so well.<br/>With this, another young adventurer is born today, ha ha.");
				}
				else
				{
					Msg("Do you by chance know about the Campfire Skill?");
					Msg("If you start a fire using the Campfire Skill,<br/>people would come by one at a time after seeing the bright fire from afar...");
					Msg("People share what they have in their inventory<br/>and spend long summer nights sharing stories about their adventures.");
					Msg("But really, if all travelers knew the Campfire Skill,<br/>inn owners like myself would have to pack up and find a different profession.");
				}
				break;
				
			case "about_arbeit":
				Msg("(Unimplemented)");
				break;
				
			case "shop_misc":
				Msg("So, you are looking for Malcolm's General Shop? It's over this hill.<br/>You'll get there in no time if you follow this road.<br/>");
				break;
				
			case "shop_grocery":
				Msg("Ah, I just remembered Caitin said she'd bring some food ingredients to my Inn.<br/>I keep forgetting these days.<br/>");
				break;
				
			case "shop_healing":
				Msg("Are you ill? Is anything bothering you?<br/>It gets cold at night. Would you like to have more heat in your room?");
				Msg("If that's not the problem, you might need a check-up from healer Dilys.");
				break;
				
			case "shop_inn":
				Msg("Are you looking for another Inn?<br/>I'm afraid you probably suspected you're being overcharged here? Ha ha.<br/>");
				break;
			
			case "shop_bank":
				Msg("Are you interested in the Bank?<br/>The Erskin Bank is a huge franchise. There are branch offices in other cities, too.<br/>You can deposit your items or money there<br/>and easily retrieve them from another office.");
				Msg("Wherever you are, you'll find a bank that can retrieve your items,<br/>so you can deposit your belongings and not worry.<br/>");
				break;
			
			case "shop_smith":
				Msg("The Blacksmith's Shop is literally right around the corner. You just need to cross the bridge.<br/>");
				break;
				
			case "skill_instrument":
				Msg("What a music lover you are.<br/>The romance of a traveler...<br/>Its pinnacle can be reached<br/>only through the performance of music.");
				Msg("If you want to be good at playing an instrument,<br/>I'd suggest you buy any musical instrument you can get<br/>and keep practicing.<br/>");
				break;
				
			case "skill_counter_attack":
				Msg("Melee Counterattack skill?<br/>Hmm... It's very difficult to explain with words.<br/>You'd better learn it at the School.");
				Msg("Hey, don't give me that look.<br/>I really don't know that skill.");
				Msg("How about talking to Ranald at the School<br/>or Trefor guarding this town?<br/>I'm sure they can help you better.");
				break;
				
			case "square":
				Msg("Well, Nora will be more than happy to explain to you about the Square.<br/>Frankly, I talked so much that my throat is getting sore.");
				break;
				
			case "farmland":
				Msg("You are looking for the farmland?<br/>There were only a few people who wanted to go there.");
				Msg("Um... No offense, but you have a very unique interest.<br/>What do you think?<br/>Don't you hear that a lot?");
				break;
				
			case "windmill":
				Msg("The Windmill is right out there.<br/>The wind blowing down the valley moves the blades and draws water to the reservoir,<br/>and also grinds the crops.");
				Msg("I noticed more and more people going to the Windmill<br/>with wool these days.<br/>Nora asked them what they're up to,<br/>and they said they're trying to spin yarn from wool.");
				Msg("There is a spinning wheel at Malcolm's General Shop,<br/>but I guess that doesn't satisfy their needs.<br/>Perhaps they think they can spin yarn faster<br/>with a bigger wheel.");
				Msg("By the way, can it actually spin out yarn?<br/>I don't think it can.");
				break;
				
			case "brook":
				Msg("Adelia Stream?<br/>The small stream in front of my shop is the Adelia Stream.<br/>Yes, the one near the Windmill.<br/>You must have missed it. Hahaha.");
				Msg("A lot of people missed that just like you.<br/>Perhaps I should talk with Ferghus<br/>and put a sign there.");
				break;
				
			case "shop_headman":
				Msg("Are you looking for the Chief's House?<br/>Hm, it's very close.");
				Msg("Go up the hill with the big tree from the Square<br/>and you'll find it right there.");
				Msg("If you happen to go there,<br/>please say hello for me and<br/>try not to do anything inappropriate.");
				break;
				
			case "temple":
				Msg("Looking for the Church?<br/>Walk up to the Square first,<br/>and go down along the narrow path toward the Bank<br/>and you'll find it.");
				Msg("When you get there, will you please<br/>tell the generous Priest Meven and the beautiful Priestess Endelyon that<br/>Piaras from the Inn<br/>gives his fullest regards?");
				break;
				
			case "school":
				Msg("You are looking for the School?<br/>It's near the Church.<br/>It's not that far from here.");
				Msg("There are teachers teaching magic and swordsmanship in the School.<br/>So you can ask them if you need anything from them.<br/>They will kindly explain to you about many things.");
				Msg("If you can afford it,<br/>perhaps it's worthwhile to pay the tuition fee and take a class.");
				break;
				
			case "shop_armory":
				Msg("Weapons Shop?<br/>There isn't one in this town, but...<br/>If you are in need of some weapons,<br/>you might want to visit the Blacksmith's Shop right over there.");
				Msg("Tell Ferghus I sent you<br/>and he'll take care of you.");
				break;
				
			case "shop_cloth":
				Msg("Are you looking for something to wear?<br/>Hmm... What you are wearing right now seems good enough.");
				Msg("You must be interested in fashion.<br/>It would be quite hard to find a better outfit than what you have.<br/>Nevertheless, you can go talk to Malcolm at the General Shop.");
				break;
				
			case "shop_bookstore":
				Msg("Are you looking for a book?<br/>I saw Lassar<br/>selling some spell books.");
				Msg("However,<br/>most people don't actually read them.<br/>They just carry them around.");
				Msg("It makes me sad to see books becoming<br/>nothing more than fashion items.");
				break;
				
			case "shop_goverment_office":
				Msg("A town office?<br/>Surely you are new to this town.<br/>There is no town office here.");
				Msg("But if you want to get some help<br/>or talk to the town elders,<br/>you'd want to pay a visit to<br/>Chief Duncan.");
				break;
				
			case "lute":
				Msg("Do you need a lute?<br/>I would really like to give you one,<br/>but so many people are asking these days.<br/>So, I can't make an exception... Even if it's you, hahaha.");
				Msg("If you visit the General Shop up there,<br/>you'll be able to find a few instruments.<br/>They are decent enough to play<br/>even though you may not find the lute you're looking for.");
				break;
				
			case "g3_DarkKnight":
				Msg("...I heard about them long ago<br/>when I used to travel around.");
				Msg("Dark Knights are the ones who<br/>betrayed humans and attacked their own brothers and sisters<br/>alongside Fomors during the war at Mag Tuireadh in the past...");
				Msg("...The previous tyrant Breath is said to have been a Dark Knight as well...");
				break;
			
			default:
				RndMsg(
					"?",
					"I don't know about that.",
					"I'd love to listen to you, but about something else.",
					"I'm afraid this conversation isn't very interesting to me.",				
					"To be honest, I don't know."					
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}		
	}
}

public class PiarasShop : NpcShopScript
{
	public override void Setup()
	{
        //--- Book ---------------------------------
        //------------------------------------------
        Add("Book", 1055);     //The Road to Becoming a Magic Warrior
        Add("Book", 1056);     //How to Enjoy Field Hunting
        Add("Book", 1124);     //An Easy Guide to Taking Up Residence in a Home
        Add("Book", 1037);     //Experiencing the Miracle of Resurrection with 100 Gold
        Add("Book", 1041);     //A Story About Eggs
        Add("Book", 1038);     //Nora Talks about the Tailoring Skill
        Add("Book", 1039);     //Easy Part-Time Jobs
        Add("Book", 1062);     //The Greedy Snow Imp
        Add("Book", 1048);     //My Fluffy Life with Wool
        Add("Book", 1049);     //The Holy Water of Lymilark
        Add("Book", 1057);     //Introduction to Field Bosses
        Add("Book", 1054);     //Behold the Dungeon - Advice for Young Generations
        Add("Book", 1058);     //Understanding Wisps
        Add("Book", 1015);     //Seal Stone Research Almanac : Rabbie Dungeon
        Add("Book", 1016);     //Seal Stone Research Almanac : Ciar Dungeon
        Add("Book", 1017);     //Seal Stone Research Almanac : Dugald Aisle
        Add("Book", 1505);     //The World of Handicrafts

        //--- Gift ---------------------------------
        //------------------------------------------
        Add("Gift", 52011);     //Socks
        Add("Gift", 52018);     //Hammer
        Add("Gift", 52008);     //Anthology
        Add("Gift", 52009);     //Cubic Puzzle
        Add("Gift", 52017);     //Underwear Set
	}
}
