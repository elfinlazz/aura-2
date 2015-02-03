//--- Aura Script -----------------------------------------------------------
// Nora, the inn helper in Tir Chonaill
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class NoraBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_nora");
		SetRace(10001);
		SetBody(height: 0.85f);
		SetFace(skinColor: 17);
		SetStand("human/female/anim/female_natural_stand_npc_nora02");
		SetLocation(1, 15933, 33363, 186);

		EquipItem(Pocket.Face, 3900, 0xDED7EA, 0xA2C034, 0x004A18);
		EquipItem(Pocket.Hair, 3025, 0xD39A81, 0xD39A81, 0xD39A81);
		EquipItem(Pocket.Armor, 15010, 0x34696E, 0xFDEEEA, 0xC6D8EA);
		EquipItem(Pocket.Shoe, 17006, 0x34696E, 0x9C558F, 0x901D55);
		
		AddPhrase("I hope the clothes dry quickly.");
		AddPhrase("I would love to listen to some music, but I don't see any musicians around.");
		AddPhrase("No way! There's no such thing as a huge spider.");
		AddPhrase("Oh no! Rats!");
		AddPhrase("Perhaps I should consider taking a day off.");
		AddPhrase("Please wait.");
		AddPhrase("Wait a second.");
		AddPhrase("Wow! Look at that owl! Beautiful!");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Nora.mp3");
	
		await Intro(
			"A girl wearing a well-ironed green apron leans forward, gazing cheerfully at her sorroundings.",
			"Her bright eyes are azure blue and a faint smile plays on her lips.",
			"Cross-shaped earrings dangle from her ears, dancing playfully between her honey-blonde hair.",
			"Her hands are always busy, as she engages in some chore or another, though she often looks into the distance as if deep in thought."
		);
		
		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("Welcome!");
				//Msg("We've met before, right? I remember you!");
				//Msg("Nice to see you, <username/>.");
				await StartConversation();
				break;
				
			case "@shop":
				Msg("Are you looking for a Tailoring Kit and materials?<br/>If so, you've come to the right place.");
				OpenShop("NoraShop");
				return;
				
			case "@repair":
				Msg("Do you want to repair your clothes?<br/>Well I can't say I'm perfect at it,<br/>but I'll do my best.<br/>Just in case, when in doubt, you can always go to a professional tailor.");
				Msg("(Unimplemented)");
				break;
		}
		
		End();
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg("My name is Nora. Please don't forget it.");
				ModifyRelation(Random(2), 0, Random(2));
				break;
			
			case "rumor":
				Msg("The Square is right up the little hill next to us.<br/>It's worth a visit if you have some time.");
				ModifyRelation(Random(2), 0, Random(2));
				break;
			
			case "about_skill":
				Msg("Are you making good use of the Rest skill?<br/>Here's a tip. Only for you, <username/>.<br/>If you continue to rank up your Rest skill,<br/>your HP will increase steadily.");
				break;
			
			case "about_arbeit":
				Msg("Are you interested in the Inn business?<br/>If so, why don't you ask Uncle Piaras?<br/>He is in the Inn.");
				break;
			
			case "shop_misc":
				Msg("Go up the hill.<br/>It's just the next building.<p/>Petty Malcolm will probably be at his General Shop.<br/>Don't pay any attention to what he says!");
				break;
			
			case "shop_bank":
				Msg("Bebhinn is the clerk at the Bank.<br/>She loves interesting stories.<br/>But be careful.<br/>I learned the hard way that it's easy to become the subject of her gossip...");
				break;
			
			case "skill_counter_attack":
				Msg("Have you talked to Ranald?<br/>If you haven't, go to the School to find him.");
				break;
			
			case "square":
				Msg("The Square is just up there. Walk up the slope to reach it.");
				break;
			
			case "farmland":
				Msg("Why are you looking for farmland?<br/>You don't look like a farmer to me.");
				break;
			
			case "shop_headman":
				Msg("The Chief's House? It's right up there.<br/>You already met him, I suppose?<br/>He has a lot of experience from the old days.");
				break;
			
			case "temple":
				Msg("To get to the Church,<br/>walk up the hill and follow the path down from the Bank.<br/>It's not far.<p/>Can you say hello to Priestess Endelyon for me?<br/>If she's not in,<br/>ask Priest Meven to do it on my behalf.");
				break;
			
			case "skill_campfire":
				Msg("No way! I can't tell you about that skill.<br/>We would run out of business!<br/>You don't know how hard it was to get permission to open our Inn!<br/>Hey, business is business.");
				break;
			
			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("Are you looking for a place to have a nice meal?<br/>Many people buy food at the Grocery Store<br/>and come here to eat with others.<br/>Didn't Caitin at the Grocery Store<br/>tell you?");
				break;
			
			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("Are you looking for a Weapons Shop?<br/>Hmm... I can't remember...<br/>Oh, right! Head to the Blacksmith's Shop.<br/>Ferghus is good at making things.<br/>I'm sure he can make all sorts of weapons.");
				break;
			
			case "shop_cloth":
				Msg("Sorry.<br/>There are no Clothing Shops in this town.<br/>Malcolm at the General Shop sells some clothes,<br/>but I wouldn't call them fashionable.");
				break;
			
			case "shop_goverment_office":
				Msg("A town office in this small town?<br/>Yeah, right!<br/>The Chief's House is probably the closest thing, though.<p/>It's right on the hill<br/>near the Square.");
				break;
			
			default:
				RndMsg(
					"Can we change the subject?",
					"Huh?",
					"I don't know much about that.",
					"What are you talking about?",
					"I can't understand what you're asking.",
					"I don't... I don't know."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class NoraShop : TailorShop
{
	public override void Setup()
	{
		base.Setup();
		
		Add("Skill Book", (c, o) => o.GetFavor(c) >= 50); // Allow access with >= 50 favor
		Add("Skill Book", 1082); // Resting Guide
	}
}
