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
				await StartConversation();
				return;
				
			case "@bank":
				Msg("(Unimplemented)");
				return;
				
			case "@redeem":
				Msg("Are you here to redeem your coupon?<br/>Please enter the coupon number you wish to redeem.", Input("Exchange Coupon", "Enter your coupon number"));
				var input = await Select();
				
				if(input == "@cancel")
					return;
				
				if(!RedeemCoupon(input))
				{
					Msg("I checked the number at our Head Office, and they say this coupon does not exist.<br/>Please double check the coupon number.");
					return;
				}
				
				// Unofficial response.
				Msg("There you go, have a nice day.");
				return;
				
			case "@shop":
				Msg("So, does that mean you're looking for a Personal Shop License then?<br/>You must have something you want to sell around here!<br/>Hahaha...");
				OpenShop("BebhinnShop");
				return;
				
			default:
				Msg("...");
				return;
		}
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			default: Msg("Can we change the subject?"); break;
		}
	}
}
