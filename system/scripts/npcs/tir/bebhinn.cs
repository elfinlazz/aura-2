//--- Aura Script -----------------------------------------------------------
// Bebhinn
//--- Description -----------------------------------------------------------
// Bank manager of Tir
//---------------------------------------------------------------------------

public class BebhinnBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_bebhinn");
		SetRace(10001);
		SetStand("human/female/anim/female_natural_stand_npc_Bebhinn");
		SetLocation(2, 1364, 1785, 228);

		SetFace(skinColor: 27, eyeType: 59, eyeColor: 55, mouthType: 1);
		EquipItem(Pocket.Face, 3900, 0xF78042);
		EquipItem(Pocket.Hair, 3100, 0x201C1A);
		EquipItem(Pocket.Armor, 90106, 0xFFE4BF, 0x1E649D, 0x175884);
		EquipItem(Pocket.Shoe, 17040, 0x996633, 0x6175AD, 0x808080);
		
		AddPhrase("Any city would be better than here, right?");
		AddPhrase("I prefer rainy days over clear days.");
		AddPhrase("It's soooo boring.");
		AddPhrase("No matter what, I am going hiking this weekend.");
		AddPhrase("Should I move out to the city?");
		AddPhrase("So many good-looking men stopped by today...");
		AddPhrase("There's nothing worse than a man who makes a woman wait.");
		AddPhrase("Where would be a good place to spend my vacation?");
		AddPhrase("Wow... I'm so pretty... Hehe.");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Bebhinn.mp3");
	
		await Intro(
			"A young lady is admiring her nails as you enter.",
			"When she notices you, she looks up expectantly, as if waiting for you to liven things up.",
			"Her big, blue eyes sparkle with charm and fun, and her subtle smile creates irresistable dimples."
		);
		
		Msg("May I help you?", Button("Start Conversation", "@talk"), Button("Open My Account", "@bank"), Button("Redeem Coupon", "@redeem"), Button("Shop", "@shop"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("Is this your first time here? Nice to meet you.");
				//Msg("I think we've met before... nice to see you again.");
				await StartConversation();
				break;
				
			case "@bank":
				OpenBank();
				return;
				
			case "@redeem":
				Msg("Are you here to redeem your coupon?<br/>Please enter the coupon number you wish to redeem.", Input("Exchange Coupon", "Enter your coupon number"));
				var input = await Select();
				
				if(input == "@cancel")
					return;
				
				if(!RedeemCoupon(input))
				{
					Msg("I checked the number at our Head Office, and they say this coupon does not exist.<br/>Please double check the coupon number.");
				}
				else
				{
					// Unofficial response.
					Msg("There you go, have a nice day.");
				}
				break;
				
			case "@shop":
				Msg("So, does that mean you're looking for a Personal Shop License then?<br/>You must have something you want to sell around here!<br/>Hahaha...");
				OpenShop("BebhinnShop");
				return;
		}
		
		End();
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg("My name is Bebhinn. Don't forget it!");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "rumor":
				Msg("Oh, you know what?<br/>Some people were hitting the scarecrow at the School to practice their skills,<br/>and they wandered off and ruined the crops in the farmland.<br/>The owner got pretty upset about it.");
				ModifyRelation(Random(2), 0, Random(2));
				break;
				
			case "about_skill":
				if(Player.Skills.Has(SkillId.Composing))
				{
					Msg("Wow! You know about the Composition skill?<br/>Can you write me a song someday? Hehe...");
					break;
				}
				goto default;
				
			case "shop_misc":
				Msg("Are you looking for the General Shop? It's just across the road.<br/>You know who's there, right? Just ask for Malcolm.");
				break;
				
			case "shop_grocery":
				GiveKeyword("skill_gathering");
				Msg("The Grocery Store is next to the Bank.<br/>There's a big chef sign next to it.<br/>And chat with Caitin while you're there.<p/>Her food is fresh because she uses ingredients harvested directly<br/>from the farm next to the shop.");
				break;
				
			case "shop_bank":
				Msg("Do you have something for me?<br/>The Bank charges you for making deposits,<br/>but I accept things for free!<br/>...<br/>However, I never give them back...");
				break;
				
			case "shop_smith":
				Msg("If you have anything that's broken and needs to be repaired, get it fixed at the General Shop or the Blacksmith's Shop<br/>before depositing it at the Bank.<br/>Once it starts to rust, there's just no stopping it!<p/>Oh, and send my regards to Ferghus if you go to the Blacksmith's Shop.");
				break;
				
			case "skill_gathering":
				Msg("Gathering?<br/>I wouldn't know anything about that...I grew up never getting my hands dirty...<br/>Hehe...");
				break;
				
			case "square":
				Msg("The Square is just in front of here.<br/>Walk out and it's right there.<br/>I guess it is a bit small for a square, huh?");
				break;
				
			case "farmland":
				Msg("The farmland is located south of here, just past the Church.<br/>There is nothing to see there...<br/>That doesn't mean you can harvest anything you want.<br/>Even boring farms have owners.");
				break;
				
			case "shop_headman":
				Msg("The Chief's House is just over there.<br/>Take the stairs from the Square<br/>and go up the hill.<p/>You know his name is Duncan, right?");
				break;
				
			case "temple":
				Msg("The Church?<br/>It's downhill from here.<br/>When you go out, go downhill towards the right.<br/>It will come up shortly.<p/>Can you send my regards to Priest Meven<br/>and Priestess Endelyon?");
				break;
				
			case "shop_restaurant":
				Msg("You mean, a restaurant?<br/>If you are looking for something to eat,<br/>you can go see Caitin.<br/>The grocery store sells food ingredients,<br/>as well as homemade food prepared  by Caitin herself.<br/>Why don't you go and ask her?");
				break;
				
			case "shop_armory":
				Msg("We don't have a dedicated Weapons Shop,<br/>but I guess you could find some weapons at the Blacksmith's Shop.<br/>Ferghus is the owner of the Blacksmith's Shop,<br/>so tell him what you want<br/>and he might be able to get it for you.");
				break;
				
			case "shop_cloth":
				Msg("There are no clothing shops in this town.<br/>Malcolm does sell some clothes though.<br/>If you're looking for something to wear,<br/>just go to the General Shop.<br/>But you might not find anything you like...");
				break;
				
			case "shop_goverment_office":
				Msg("Haha! You're joking, right? We have the Chief's House and that's it.<br/>A town office? In this small town? Please!<br/>Since this town is in the Ulaid region,<br/>we are not governed directly by the king.<p/>Instead, Duncan represents our town.<br/>If you are looking for an elder,<br/>go up the hill to Duncan's House.");
				break;
			
			default:
				RndFavorMsg(
					"Can we change the subject?",
					"Hmm... You know a story I've never heard of... How could that be?",
					"Hehe... I don't know what you're talking about...",
					"I said I don't know! Why do you keep rubbing it in my face? That's mean... Hehe.",
					"I have no idea... Why don't you ask someone else?",
					"What's that?",
					"Well...what do you mean?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}
