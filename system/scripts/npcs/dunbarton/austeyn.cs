//--- Aura Script -----------------------------------------------------------
// Austeyn
//--- Description -----------------------------------------------------------
// Bank Manager
//---------------------------------------------------------------------------

public class AusteynScript : NpcScript
{
	public override void Load()
	{
		SetName("_austeyn");
		SetRace(10002);
		SetBody(upper: 1.2f);
		SetFace(skinColor: 16, eyeType: 8, eyeColor: 84, mouthType: 1);
		SetLocation(20, 660, 770, 251);

		EquipItem(Pocket.Face, 4904, 0x00784C3D, 0x00D58877, 0x00FFCB9C);
		EquipItem(Pocket.Hair, 4027, 0x00D1D9E3, 0x00D1D9E3, 0x00D1D9E3);
		EquipItem(Pocket.Armor, 15003, 0x0036485A, 0x00BDC2B1, 0x00626C76);
		EquipItem(Pocket.Shoe, 17009, 0x0036485A, 0x00FFE1B9, 0x009A004E);

		AddGreeting(0, "Welcome. It must have been a long journey for you. Your legs must be hurting.");
		AddGreeting(1, "You seem familiar. Have we met before?");
        
		AddPhrase("*Doze off*");
		AddPhrase("*Yawn*");
		AddPhrase("Ah... How boring...");
		AddPhrase("Come to think of it, it's been a while since my last hair cut.");
		AddPhrase("It's boring during the day with everyone attending school.");
		AddPhrase("Let's see... That fellow should be coming to the Bank soon.");
		AddPhrase("Mmm... I must have dozed off.");
		AddPhrase("Mmm... I'm tired...");
		AddPhrase("My body's not like it used be... Hahaha.");
		AddPhrase("Oops. The mistakes have been getting more frequent lately.");
		AddPhrase("Perhaps I should hire a cute office assistant. Who knows? Maybe that will bring in more business.");
		AddPhrase("That fellow looks like he might have some Gold on him...");
	}

	protected override async Task Talk()
	{	
		SetBgm("NPC_Austeyn.mp3");

		await Intro(
			"His gray hair and mustache may show his age,",
			"but his firm build and the smile on his face show a youthful presence.",
			"It's as if he wants to prove that he can smile even with his small eyes."
		);

		Msg("So, what can I help you with?", Button("Start a Conversation", "@talk"), Button("Open My Account", "@bank"), Button("Redeem Coupon", "@coupon"), Button("Coin Event", "@coin"), Button("Trade", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("Oh wow, you're the one who saved Erinn?<br/>While you're at it,<br/>can you take care of the economic state of Dunbarton as well? Hehe...");
				await Conversation();
				break;

			case "@coupon":
				Msg("Do you want to redeem your coupon?<br/>Then please give me the number of the coupon you want to redeem.<br/>Slowly, one digit at a time.", Input("Coupon Exchange", "Enter Coupon Number"));
				var input = await Select();

				if(input == "@cancel")
					return;

				if(!RedeemCoupon(input))
				{
					Msg("Strange coupon number.<br/>Are you sure that's the right number?<br/>Think about it one more time... carefully.");
				}
				else
				{
					// Unofficial response.
					Msg("There you go, have a nice day.");
				}
				break;

			case "@bank":
				OpenBank();
				return;

			case "@coin":
				Msg("During the Coin event, you can collect 4 different kinds of coins");
				Msg("You'll find Event Coins all over Erinn.<br/>The General Shop has a quest scroll that will convert those coins into event coupons.<br/>Convert those coins into coupons, then bring them to me to participate!");
				break;

			case "@shop":
				Msg("Ah, so you need a Personal Shop License?<br/>You must have one if you want to sell<br/>merchandise around here, so keep it with you.");
				OpenShop("AusteynShop");
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
					Msg("My name is <npcname/>. Your name is?<br/><username/>, huh? Ahhh. <username/>... <username/>...");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					GiveKeyword("shop_bank");
					Msg(FavorExpression(), "That's right. I am <npcname/>, the manager of this Dunbarton branch of the Erskin Bank. Nice to meet you.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				Msg(FavorExpression(), "Are you from the north?<br/>Then you must be from the Ulaid region.<br/>Ulaid region is where the folks at Tir Chonaill live.<br/>They are the descendants of Partholon.");
				Msg("I hear they used to be a kingdom, even though it has since turned into a small village.<br/>If you get to go there, say hello to the young lady at the Bank for me. Her name is Bebhinn.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("Hm? I'm not sure.");

				// If user doesn't have gold strike...

				/*Msg("What's sharper than a sword<br/>and more powerful than a hammer?<br/>Gold is! Gold can win battles,<br/>and I'm not talking about hiring goons<br/>to fight on your behalf.");
				Msg("I'm talking about taking shiny pieces of gold<br/>and pummeling your enemies with them.<br/>Nothing smells better than freshly-spilt blood<br/>coating your hard-earned gold pieces.<br/>Interested in trying learning the skill?<button title='Teach Me' keyword='@yes' /><button title='Maybe Later' keyword='@no' />");
				switch(await Select())
				{
					case "@yes":
						StartQuest("20118"); //For the Wealthy Only
						Msg("I can sense your enthusiasm beneath<br/>your deceptively calm facade!<br/>Here, these Wings of a Goddess will take you to Tara.<br/>Find Keith at the Bank. He'll fill you in on the details.");
						Msg("<npcportrait name='NONE'/>(<npcname/> hands you a Wings of a Goddess,<br/>an excited look in his eyes.)");
						break;
					
					case "@no":
						Msg("Really? How disappointing.<br/>Well, let me know if you change your mind.<br/>I really hope you change your mind.");
						break;
					}*/
				break;

			case "shop_misc":
				Msg("Ah, do you need some items?<br/>Then exit here and go to the left.<br/>The shop with a frowning man in front is the General Shop.");
				Msg("While you're there, talk to Walter for me and ask if he needs a loan.");
				break;

			case "shop_grocery":
				Msg("A grocery store?<br/>Just go to the Restaurant and ask for food.<br/>Hahaha.");
				break;

			case "shop_healing":
				Msg("A Healer's House...<br/>You mean Manus' place.<br/>Follow the road down south<br/>and you'll see it.");
				Msg("It's on the right side of the<br/>road that leads to the town entrance.<br/>Look for the sign and you should be able to find it easily.");
				break;

			case "shop_inn":
				Msg("Mmm? I've lived in this town for a while now, but I've never heard of an inn here.<br/>If there is one, make sure to let me know.<br/>I'd like to go see it myself...");
				break;

			case "shop_bank":
				Msg("A Bank? You're standing in it.<br/>Ohh, you already knew this was a Bank?<br/>My, my...");
				break;

			case "shop_smith":
				GiveKeyword("shop_bookstore");
				Msg("The Blacksmith's Shop?");
				Msg("Do you know what a blacksmith's shop is by any chance?<br/>I happened to buy an expensive encyclopedia<br/>at the Bookstore over there the other day, and let's see...");
				Msg("A blacksmith's shop is<br/>'...It's a place with bellows that treats pig iron...'<br/>That's what it says here.");
				Msg("This doesn't tell me anything!");
				Msg("Aeira sells useless books!<br/>Do you understand what this means exactly? Or should I just go to the Bookstore and complain?");
				break;

			case "skill_instrument":
				Msg("I thought I heard someone say that<br/>you can learn that skill once you buy an instrument.<br/>Have you tried it before?");
				Msg("The General Shop happens to be next door.<br/>If you really want to learn it, do as I told you.");
				break;

			case "skill_composing":
				Msg("Composing music?<br/>I'm not sure there is anyone in this town<br/>who could teach you something like that.");
				break;

			case "skill_tailoring":
				Msg("In this town, no one can compare to Simon<br/>when it comes to tailoring.");
				break;

			case "skill_magnum_shot":
				Msg("If you are looking for combat-related skills,<br/>why don't you go talk to Aranwen at the School<br/>instead of me?");
				break;

			case "skill_counter_attack":
				Msg("...<br/>Hmm... Did you get on Aranwen's bad side in class?");
				Msg("Why else would you ask a banker for such things<br/>when there is a combat instructor in town?");
				break;

			case "skill_smash":
				Msg("It's probably better to ask Aranwen at school.<br/>Don't tell me you are too lazy for that.");
				break;

			case "square":
				Msg("The Square? The Square?? The Square???<br/>It's just outside. Hahaha.<br/>You were just kidding, right?");
				break;

			case "farmland":
				Msg("There are lots of fields once you<br/>go out into the plains past the town entrance.");
				Msg("...<br/>Don't tell me you don't think that<br/>those fields are farmlands?");
				Msg("By the way, there have been these<br/>strangely large packs of rats<br/>appearing rather frequently as of late,<br/>and I just can't figure out why...");
				break;

			case "brook":
				Msg("Adelia Stream, huh?<br/>It's probably the stream that flows down<br/>from the northern region of Ulaid.");
				break;

			case "shop_headman":
				Msg("A chief? What, you think this is a tribal village?<br/>Hahaha. A chief? Hahaha.");
				break;

			case "temple":
				Msg("Ah-ha! Got a burden on your heart, do you?<br/>People usually seek God once they have<br/>tasted the bitterness of life,<br/>rather than when they are doing well for themselves.");
				Msg("Anyway, whatever the case,<br/>Priestess Kristell won't<br/>turn you away or anything like that.<br/>Hahaha!");
				break;

			case "school":
				Msg("The School is on the other side of the Square.<br/>You can't go straight up there,<br/>but you have to go around a bit.<br/>Don't forget.");
				Msg("Also, just to be extra sure,<br/>the martial arts instructor, Aranwen, is right in front<br/>of the School, wearing silver armor.");
				break;

			case "shop_restaurant":
				Msg("Oh, no! You haven't eaten yet, have you?<br/>Now, now, the food's available just around the corner, so don't get too antsy.<br/>When you get there, tell the lady that <npcname/> sent you.");
				Msg("She'll take real good care of you.<br/>She may not look it, but she has the heart of an angel. Hahaha!");
				break;

			case "shop_armory":
				Msg("The Weapons Shop is over there by the south entrance.<br/>Nerys is usually outside the store so ask her.");
				Msg("By the way, weapons or armor here might be<br/>really expensive...");
				break;

			case "shop_cloth":
				Msg("Simon's Clothing Shop is right next door.<br/>Have you been there yet?<br/>It's a strange-looking place, but don't be too perplexed.");
				Msg("That's just how Simon is. Hahaha.<br/>Think of it that way and it will give you more peace of mind.");
				break;

			case "shop_bookstore":
				Msg("Hahaha.<br/>You mean that kid Aeira's Bookstore near the northern town entrance.<br/>Don't you think she's cute?");
				Msg("Last time I was there to buy a book, I called her a kid.<br/>She didn't like that, and started pouting and said");
				Msg("\"Excuse me? Kid?<br/>I'm a full-grown lady now!\"");
				Msg("I really wanted to ask what her<br/>idea of a \"full-grown lady\" was,<br/>but I decided not to. Hahaha!");
				break;

			case "shop_goverment_office":
				Msg("Hmm. The Lord and the Captain of the Royal Guards are there.<br/>They don't show their faces too often,<br/>but the office is just over there, so stop by.<br/>You'll also get to see the cute Eavan, too.");
				Msg("Ah, before you get the wrong idea,<br/>Eavan is a girl, not a boy.<br/>Don't go looking for the wrong person.");
				break;

			default:
				RndFavorMsg(
					"Hmm. I don't know.",
					"Hmm? What is it you just said?",
					"Ha ha. Why are you asking me? I don't know.",
					"Shame. I probably can't help you with that.",
					"Oh, I don't think it's a topic I'm familiar with.",
					"You can't keep talking about something like that with me. Hahaha.",
					"Heh. Don't think that I<br/>should know everything you talk about.",
					"Hmm... It's news to me.<br/>I'll ask someone else for you when they come by."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class AusteynShop : NpcShopScript
{
	public override void Setup()
	{
		Add("License", 60102); // Dunbarton Merchant License
		Add("License", 81010); // Purple Personal Shop Brownie Work-For-Hire Contract
		Add("License", 81011); // Pink Personal Shop Brownie Work-For-Hire Contract
		Add("License", 81012); // Green Personal Shop Brownie Work-For-Hire Contract
	}
}