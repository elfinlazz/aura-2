//--- Aura Script -----------------------------------------------------------
// Ibbie
//--- Description -----------------------------------------------------------
// Daughter of Bryce (Citizen)
//---------------------------------------------------------------------------

public class IbbieScript : NpcScript
{
	public override void Load()
	{
		SetName("_ibbie");
		SetRace(10001);
		SetBody(height: 0f);
		SetFace(skinColor: 15, eyeColor: 90);
		SetStand("human/anim/female_natural_sit_02");
		SetLocation(31, 10774, 15796, 197);

		EquipItem(Pocket.Face, 3900, 0x00005046, 0x00F10370, 0x00690A6D);
		EquipItem(Pocket.Hair, 3024, 0x00B78B68, 0x00B78B68, 0x00B78B68);
		EquipItem(Pocket.Armor, 15042, 0x00FFC3BF, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.Glove, 16011, 0x00FFFFFF, 0x006D696C, 0x00D9EEE5);
		EquipItem(Pocket.Shoe, 17007, 0x00702639, 0x009F5B0D, 0x005F6069);
		EquipItem(Pocket.Head, 18014, 0x00FFDBC5, 0x009B7685, 0x00736A4B);

		AddGreeting(0, "You must be a visitor in this town, aren't you?</p><username/>...?</p>Me... I'm Ibbie.");
		AddGreeting(1, "Your name... Tell me your name again...? I'm not used to it yet...");

		AddPhrase("*Cough* *Cough*");
		AddPhrase("Are you... lonely, too?");
		AddPhrase("Daddy...");
		AddPhrase("I think I have a fever...");
		AddPhrase("I wish I can gain some weight, too...");
		AddPhrase("I'm lonely...");
		AddPhrase("I'm tired of being sick...");
		AddPhrase("Maybe I messed up on this one...");
		AddPhrase("Mom...");
		AddPhrase("Sigh...");
		AddPhrase("There are so many people with such mysterious items...");
		AddPhrase("What does the afterlife look like...? I wish I had a friend...");
		AddPhrase("Where is Sion?");

	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Ibbie.mp3");

		await Intro(
			"Under the wide brim of the Mongo hat, her lovely blonde hair dances in the gentle breeze.",
			"Her delicate neck stretches out of the lace collar of her intricately tailored rosy-pink dress.",
			"Her big, bright jade eyes twinkle",
			"and her round face, like porcelain, is so fair that it looks pale."
		);

		Msg("Do you have something to say?", Button("Start Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Player.Titles.SelectedTitle == 11002)
					Msg("Guardian... of Erinn...?<br/>Then will this person watch over Ibbie, too...?");
				await Conversation();
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "My name...?<br/>Didn't I tell you?");
				Msg("Ibbie... Just call me Ibbie.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "Jennifer once told me<br/>that I should eat lots to gain weight and get healthy.");
				Msg("But...<br/>I don't really want to eat anything.");
				Msg("Ibbie wishes she could live without eating.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_arbeit":
				Msg("I want to  try it too...");
				break;

			case "shop_misc":
				Msg("General shop?<br/>You mean Gilmore's place<br/>next to the Blacksmith's Shop?");
				Msg("The General Shop is at the end of that building down there.<br/>I would take you there myself but...");
				Msg("My legs hurt... I'm sorry.");
				break;

			case "shop_grocery":
				Msg("We don't have a Grocery Store here.<br/>But if you want to buy some food...<br/>try Jennifer's Pub.");
				Msg("It's a pub, but they carry more than just drinks.<br/>If Jennifer seems busy, you can talk to Riocard.");
				Msg("You have to go through the town to get there.<br/>You'll see it on the right side of the Minimap.");
				break;

			case "shop_healing":
				Msg("This town doesn't have a Healer's House.");
				Msg("So if you get sick,<br/>you have to go all the way to Dunbarton.");
				Msg("They told me last time when I was sick<br/>that daddy had to carry me on his back and run through Gairech Hill, all the way to Dunbarton. *sniff*");
				Msg("It's not like I'm light either... *sniff*");
				break;

			case "shop_inn":
				Msg("We don't have that in this town.");
				break;

			case "shop_bank":
				Msg("A Bank?<br/>You're looking for my daddy then.");
				Msg("But this town doesn't have a bank building.<br/>So daddy works  in front of the dusty storage room...");
				Msg("It's all my fault...");
				break;

			case "shop_smith":
				Msg("It's the building just down there... *cough* *cough*");
				Msg("Can't see it?");
				Msg("You must have pretty bad eyes, too.");
				Msg("You should be careful when you enter the town.");
				Msg("I fell going into the town once...");
				break;

			case "skill_rest":
				Msg("It's good to use the Resting skill, but<br/>don't sit close to me...");
				break;

			case "skill_range":
				Msg("Do you HAVE to talk about fighting... With me...?");
				break;

			case "skill_instrument":
				Msg("It would be wonderful if someone played some music for me...");
				Msg("You think that's too much...?");
				break;

			case "skill_composing":
				Msg("I don't think there's anyone in town who can do that...");
				Msg("Everyone is busy making ends meet...");
				break;

			case "skill_gathering":
				Msg("Ibbie doesn't know how to do something like that...");
				break;

			case "square":
				Msg("Sometimes I think about<br/>how nice it would be to have a playground in the middle of town where kids can run around and play...<br/>There would be flowers and grass everywhere...");
				Msg("Am I asking too much...?<br/>Probably not in this town...");
				break;

			case "pool":
				Msg("Water is precious in this town...<br/>Sometimes there isn't even enough water for me to shower...");
				Msg("I hate feeling dirty...");
				break;

			case "farmland":
				Msg("Now that you mention it, I've never seen anyone farming around here.<br/>We buy all our food from Jennifer...");
				break;

			case "brook":
				Msg("What are you talking about?");
				break;

			case "shop_headman":
				Msg("Come to think of it, I've never heard about<br/>a chief in this town.");
				break;

			case "temple":
				Msg("There is no church here.");
				Msg("But we do have a priest...");
				Msg("Why don't you go talk to Comgan over there?");
				break;

			case "school":
				Msg("This town doesn't have a school.");
				Msg("It's nice having not to go to school, but...");
				Msg("It gets boring...");
				break;

			case "skill_campfire":
				Msg("You mean playing around a campfire?<br/>It seems like a lot of fun, but...<br/>Daddy gets upset if I go near a campfire...");
				Msg("So if you are gonna do it, don't do it around here...");
				break;

			case "shop_restaurant":
				Msg("You can buy food from Jennifer...");
				Msg("So try visiting the Pub...");
				Msg("A pub that sells food...<br/>I know it seems funny, but it's true...");
				break;

			case "shop_armory":
				Msg("Well... I've seen something like that at the Blacksmith's Shop<br/>but I'm not really sure...");
				break;

			case "shop_cloth":
				Msg("We don't have that in this town.<br/>Dunbarton seems to have it, though.");
				Msg("Last time I was there, I saw this one beautiful dress...");
				Msg("I really wanted to try it on...");
				Msg("...Am I asking for too much?");
				break;

			case "shop_bookstore":
				Msg("It's probably not in this town,<br/>but I heard that there is<br/>a place like that in Dunbarton.");
				break;

			case "graveyard":
				Msg("This town doesn't have one,<br/>but I heard about it.");
				Msg("It's a place where you get buried after you die, right?<br/>");
				break;

			default:
				RndFavorMsg(
					"I don't really like it...",
					"Is that something you have to know?",
					"Ibbie doesn't know too much about that.",
					"...Sorry...I just don't want to talk about it...",
					"..I'm not really interested in things like that...",
					"Please don't talk about stuff like that with me...",
					"I don't think it matters whether you know it or not..."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}
