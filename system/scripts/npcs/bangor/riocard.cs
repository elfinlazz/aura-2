//--- Aura Script -----------------------------------------------------------
// Riocard
//--- Description -----------------------------------------------------------
// Waiter
//---------------------------------------------------------------------------

public class RiocardScript : NpcScript
{
	public override void Load()
	{
		SetName("_riocard");
		SetRace(10002);
		SetBody(height: 0.7f);
		SetFace(skinColor: 20, eyeType: 2, eyeColor: 60, mouthType: 1);
		SetStand("human/male/anim/male_natural_stand_npc_riocard");
		SetLocation(31, 15286, 8898, 136);

		EquipItem(Pocket.Face, 4900, 0x009BD3A3, 0x00FFC43A, 0x00496C7C);
		EquipItem(Pocket.Hair, 4001, 0x00AF7B34, 0x00AF7B34, 0x00AF7B34);
		EquipItem(Pocket.Armor, 15040, 0x00EFCE4B, 0x00B5C27E, 0x0037553F);
		EquipItem(Pocket.Shoe, 17010, 0x00512522, 0x009E0075, 0x00B80075);
		EquipItem(Pocket.Head, 18007, 0x00EFCE4B, 0x006CB4E4, 0x0001A890);

		AddGreeting(0, "Welcome to the Bangor Pub. Did you come alone?");
		AddGreeting(1, "You've been here before, haven't you? I recognize your face.");

		AddPhrase("I could use a good story right about now.");
		AddPhrase("I guess taking it easy every now and then isn't such a  bad idea.");
		AddPhrase("I'm getting bored...");
		AddPhrase("It's been a while since I started working here.");
		AddPhrase("Let's see. What should I do now?");
		AddPhrase("Phew... It's dusty already.");
		AddPhrase("There is no end to this mess.");
		AddPhrase("Well, work is work.");
		AddPhrase("Why should I clean up everyone's mess?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Riocard.mp3");

		await Intro(
			"He wears a yellow beret backwards at a slight angle as his hair sticks out on the sides.",
			"The yellow shirt that he seemed to have put on in a rush matches his hat.",
			"Between the narrow shoulders, his face is still full of boyish charm.",
			"Every time he blinks, his eyelid casts a slight shadow over his innocent, light green eyes."
		);

		Msg("Mmm? What is it?", Button("Start Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Player.Titles.SelectedTitle == 11002)
					Msg("Wow, <username/>, your fame reaches the heavens.<br/>But, I bet the burden on your shoulders<br/>is quite heavy as well...");
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
				Msg(FavorExpression(), "My name is Riocard. Rio. Card.<br/>I work here.");
				Msg("If you ever need anything,<br/>please don't hesitate to call me.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "That lady over there by the bar?<br/>That's my boss, Jennifer.");
				Msg("She's still single because<br/>her nasty personality keeps all men away. Haha...<br/>Don't tell her I said that!");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_arbeit":
				Msg("...Haha, you're going to work for me?<br/>Are you sure you want to do that?");
				break;

			case "shop_misc":
				Msg("If you mean Gilmore's General Shop,<br/>it's just over there.");
				Msg("Do you have a strange hobby...?");
				break;

			case "shop_grocery":
				Msg("We have drinks as well as food, so<br/>feel free to ask for anything you need.");
				Msg("Oh, not me. I meant ask Jennifer over there.<br/>Hehe.");
				break;

			case "shop_healing":
				Msg("For some reason, there's no Healer's House in this town.<br/>If I had known that early enough, I would have learned some Healer skills.");
				Msg("Working under Jennifer's tyranny is not<br/>something I want to do forever... Hehe...");
				break;

			case "shop_inn":
				Msg("Jennifer asks me every day,<br/>\"What are your thoughts on starting an inn?\"");
				Msg("I don't understand why she needs to ask me.<br/>If she wants to do it, then she should.");
				Msg("But I guess it's nice of her to ask me what I think...");
				break;

			case "shop_bank":
				Msg("The Bank is right next to the General Shop.");
				Msg("You might have simply passed it because there is no building for it.<br/>There is a man named Bryce over there who can help you.");
				Msg("He takes care of banking business<br/>so talk to him if you have any banking needs.");
				Msg("I don't understand what Jennifer finds so attractive about him.");
				break;

			case "shop_smith":
				Msg("Are you looking for Elen?");
				Msg("You don't hear the hammering or feel the heat that comes from there?<br/>...Wait, you're not asking me because you don't know where it is... Are you?");
				Msg("...Is it because... of Elen?");
				Msg("Or... because of Edern?<br/>Puhaha!");
				break;

			case "skill_rest":
				Msg("Now, this is something Jennifer and I agreed on.");
				Msg("Resting should be done in the Pub!<br/>Hahaha...");
				break;

			case "skill_range":
				Msg("Hmm. Once you have a bow,<br/>long range attack is not a problem,<br/>'cause you can learn the Ranged Attack skill.");
				Msg("Oh! Come to think of it, you'll need arrows, too.");
				Msg("If that's too much trouble, there's always magic attacks.<br/>Learning magic is not so simple, though.");
				Msg("I know what you're thinking... \"How does this pub worker boy know all this?\"<br/>Haha...When you work here long enough, you hear all sorts of things...");
				break;

			case "skill_instrument":
				Msg("Many people say that musical talent is something<br/>you have to be born with,<br/>but that's not true.");
				Msg("If you practice...");
				Msg("And practice some more....");
				Msg("Anyone can learn to play!");
				Msg("If you think about how many kids,<br/>who never have a chance to own an instrument, are discouraged to try<br/>in the name of 'natural talent'");
				Msg("you would understand just how cruel it is to say that.<br/>...<br/>And that goes for you too...");
				break;

			case "skill_composing":
				Msg("Do you want to make music?<br/>The most important thing in composing a song<br/>is expressing the imagination of the heart.");
				Msg("If you just obsess over mastering the skills<br/>and loose the passion in your heart,");
				Msg("you have to wonder whether<br/>a song that comes from<br/>such extrinsic motivation could truly be beautiful.");
				break;

			case "skill_tailoring":
				Msg("Haha... Are you seriously expecting a good answer<br/>from a guy about tailoring?");
				Msg("First, you need to go to the General Shop, and<br/>umm... you have to talk to Gilmore.<br/>Well, anyway, buy a Tailoring Kit.");
				Msg("Once you have it in your hands,<br/>you should be able to learn the skill without anyone explaining to you.");
				Msg("If you are not sure, trying asking Jennifer...");
				Msg("...<br/>Well, it might be better to just ask me.");
				break;

			case "skill_magnum_shot":
				Msg("I heard it's a skill that utilizes the resilience of the arrow<br/>to the max and makes the arrow much more powerful");
				Msg("That's just what I heard... Hehe.");
				Msg("It's a good skill to have if you<br/>practice hard and use it in preemptive strikes.<br/>What do you think?");
				break;

			case "skill_counter_attack":
				Msg("That's a skill that allows you to reverse the enemy's attack.<br/>If you just keep in mind that it continually<br/>uses Stamina and you can't move, the skill should come in handy.");
				Msg("If you are about to be attacked by a Smash and not a regular attack,<br/>your counterattack will apparently be much stronger.  From what I know, it's a pretty popular skill...");
				Msg("I overheard someone say that it came in handy in some dungeons.");
				Msg("...");
				Msg("Haha... You probably already know all this...");
				break;

			case "skill_smash":
				Msg("Smash is a skill that can penetrate an enemy's defense<br/>and inflict a heavy damage.");
				Msg("Some customers<br/>claimed to have defeated a bear just by using the Smash.");
				Msg("But be careful. Smash may seem more powerful,<br/>but it does have its drawbacks<br/>compared to quick regular attacks.");
				Msg("...These are all the things I just heard... So take 'em with a grain of salt, hehe.");
				break;

			case "skill_gathering":
				Msg("Gathering?  The only thing worth gathering in this town is...<br/>Maybe some minerals inside Barri dungeon...?");
				Msg("Sometimes...<br/>I see people going in there<br/>without even a Pickaxe. They must be so brave...");
				Msg("It's more amazing when they actually bring back minerals....");
				break;

			case "farmland":
				Msg("Strange, isn't it? We have no farmers here.<br/>I have to wonder if mining brings in that much revenue...");
				Msg("There were plenty of farmlands where I used to live.");
				break;

			case "brook":
				Msg("I've heard about it from a customer once.<br/>It's a small stream in the region of Ulaid, right?");
				Msg("I heard the water is so clean and clear because it comes from melted snow...<br/>So it makes you feel like your soul is being cleansed just being near it.");
				Msg("Haha... Well, I haven't seen it with my own eyes<br/>so I can't really explain it any further.");
				break;

			case "shop_headman":
				Msg("Haha. There is no such person here.<br/>By the way, seeing how so many people ask for him,<br/>he must be a popular figure in other towns?");
				Msg("I wonder how a popular old man looks like.");
				Msg("Well, it's not that easy imagining someone like that<br/>when you've lived your whole life watching<br/>old folks like Gilmore or Edern, you know.");
				break;

			case "temple":
				Msg("To be honest,<br/>I don't really understand why<br/>the Lymilark Church sent<br/>Comgan to this town.");
				Msg("I'm pretty sure Comgan is younger than I am.<br/>I don't see how a kid like him could testify to God<br/>and comfort the souls of others.");
				Msg("Whenever I see him, I feel kinda bad...<br/>He doesn't even have a lot of friends...");
				Msg("Poor kid.");
				Msg("I think it's more of the Church's fault than his though.");
				break;

			case "school":
				Msg("This town doesn't have a school. Haha...<br/>Hey, hey. Don't look at me like that.<br/>I've learned everything I need to know<br/>and know just about anything and everything.");
				Msg("Tell me, have you met anyone else who could<br/>answer all your questions as well as I do?");
				Msg("Haha... Am I boasting too much now?");
				break;

			case "skill_windmill":
				Msg("Yeah. I've heard about that. The Windmill skill.<br/>It's a combat skill that attacks multiple opponents at once.");
				Msg("It's quite effective when you're surrounded...");
				Msg("Some people seem to think that it's a dance move,<br/>but that's an insult to the art of combat!");
				Msg("Oh, of course, this is just what I've heard...");
				break;

			case "skill_campfire":
				Msg("I don't know if you can learn the Campfire skill in this town.<br/>Everybody's really sensitive about handling fire.");
				Msg("You might not have noticed, but all<br/>of the furnaces over there have someone monitoring them.");
				break;

			case "shop_restaurant":
				Msg("Whether you are looking for a restaurant or a grocery shop,<br/>you can buy your food here.");
				Msg("I did suggest to Jennifer that we should put up a 'Grocery Shop' sign,<br/>but Jennifer is too...");
				break;

			case "shop_armory":
				Msg("You can buy those from Elen at the Blacksmith's Shop.<br/>She's kind of cute, but<br/>doesn't seem like an easy person to be friends with.");
				Msg("People who are extremely nice<br/>generally tend to carry more pain and secrets...<br/>*Hehehe*");
				break;

			case "shop_cloth":
				Msg("Don't expect too much in this town.<br/>No matter how pretty they are,<br/>you have to go through that grumpy Gilmore to buy clothes.");
				break;

			case "shop_bookstore":
				Msg("You know, Jennifer is quite a bookworm too....<br/>I wonder if she'd get mad if I suggest that she collect all her books and open up a bookstore?");
				Msg("Nowadays, she gets mad at anything I say.<br/>I scared to talk to her... Geez.");
				break;

			case "shop_goverment_office":
				Msg("It would be cool if we had a town office.<br/>...");
				Msg("But then, we would have to pay taxes...");
				Msg("That reduces profit...");
				Msg("And that reduces... my wage?");
				Msg("Hey! That's no fun!");
				break;

			default:
				RndFavorMsg(
					"Why do you keep bringing that up...?  Are you teasing me...?",
					"I wish I had an answer for you but, unfortunately, I don't know anything about that.",
					"You somehow seem to enjoy talking about things that I don't know. That's not very nice.",
					"Did Jennifer send you to ask me these questions...? Jennifer always gives me a hard time.",
					"Hmm. I don't really know anything about that topic. I think I could learn a lot from you."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}