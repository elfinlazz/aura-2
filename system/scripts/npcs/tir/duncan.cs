//--- Aura Script -----------------------------------------------------------
// Duncan in Tir Chonaill
//--- Description -----------------------------------------------------------
// Good ol' Duncan
//---------------------------------------------------------------------------

public class DuncanBaseScript : NpcScript
{
    public override void Load()
    {
        SetName("_duncan");
        SetRace(10002);
        SetBody(height: 1.3f);
        SetFace(skinColor: 20, eyeType: 17);
        SetStand("human/male/anim/male_natural_stand_npc_duncan_new", "male_natural_stand_npc_Duncan_talk");
        SetLocation(1, 15409, 38310, 122);

        EquipItem(Pocket.Face, 4950, 0x93005C);
        EquipItem(Pocket.Hair, 4083, 0xBAAD9A);
        EquipItem(Pocket.Armor, 15004, 0x5E3E48, 0xD4975C, 0x3D3645);
        EquipItem(Pocket.Shoe, 17021, 0xCBBBAD);

        AddPhrase("Ah, that bird in the tree is still sleeping.");
        AddPhrase("Ah, who knows how many days are left in these old bones?");
        AddPhrase("Everything appears to be fine, but something feels off.");
        AddPhrase("Hmm....");
        AddPhrase("It's quite warm today.");
        AddPhrase("Sometimes, my memories sneak up on me and steal my breath away.");
        AddPhrase("That tree has been there for quite a long time, now that I think about it.");
        AddPhrase("The graveyard has been left unattended far too long.");
        AddPhrase("Watch your language.");
    }

    protected override async Task Talk()
    {
        SetBgm("NPC_Duncan.mp3");

        await Intro(
            "An elderly man watches over the town with a soft, confident gaze.",
            "While his leathery skin and gray hair are indicative of his long life,",
            "his eyes retain a youthful twinkle. When he speaks, his voice",
            "resonates with gentle authority."
        );

        Msg("Please let me know if you need anything.", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Retrive Lost Items", "@lostandfound"), Button("Reset Beginner Skills", "@resetskills"), Button("View Controls", "@controls"));

        switch (await Select())
        {
            case "@talk":
                Msg("What did you say your name was?<br/>Anyway, welcome.");
                await StartConversation();
                return;

            case "@shop":
                Msg("Choose a quest you would like to do.");
                OpenShop("DuncanShop");
                return;

            case "@lostandfound":
                Msg("If you are knocked unconcious in a dungeon or field, any item you've dropped will be lost unless you get resurrected right at the spot.<br/>Lost items can usually be recovered from a Town Office or a Lost-and-Found.");
                Msg("Unfortunatly, Tir Chonaill does not have a Town Office, so I run the Lost-and-Found myself.<br/>The lost items are recovered with magic,<br/>so unless you've dropped them on purpose, you can recover those items with their blessings intact.<br/>You will, however, need to pay a fee.");
                Msg("As you can see, I have limited space in my home. So I can only keep 20 items for you.<br/>If there are more than 20 lost items, I'll have to throw out the oldest items to make room.<br/>I strongly suggest you retrieve any lost items you don't want to lose as soon as possible.");
                return;

            case "@resetskills":
                Msg("(Unimplemented)");
                //if (Player.levelTotal < 1000)
                //{
                    //Msg("I see you have yet to reach a cumulative level of 1000.<br/>As such, I would be happy to offer you the ability to reset<br/>all of your skills to their base level so that you could rebuild them as you like.");
                    //Msg("Would you be interested in reseting your skills?"); //YES or NO response
                    //YES: reset skills... I'm too scared to try this now.
                    //NO: Msg("Come see me any time. So long as your cumulative level is<br/>under 1000, it would be no trouble to give you a reset.");
                //}
                //else
                //{
                    //I don't know... Does this button even show up?
                //}
                return;

            case "@controls":
                Msg("First, let me show you the basic mouse controls.<br/>Take your time and get to know them well.", Movie("movement_camera_guide_us.wmv", 500, 300, true));
                Msg("The keyboard controls are a little harder to grasp, but you'll<br/>get it down. The most important thing to take away is that<br/>you can change your Hotkey settings in the Options menu.", Image("us/guide_keybord", 744, 344));
                Msg("Now that you know the basic controls, let me teach you<br/>a few other things.", Image("us/guide_keybord", 744, 344));
                Msg("This is an example of a keyboard shortcut. You'll find them<br/>very useful in accomplishing common tasks quickly.<br/>Get to know your controls, and let me know if you have any questions.", Image("us/guide_keylook", 744, 344));
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
            case "personal_info":
                Msg("I'm the chief of this town...");
                //Msg("Once again, welcome to Tir Chonaill.");
                // MoodChange
                break;

            case "rumor":
                Msg("I heard a rumor that this is just a copy of the world of Erin. Trippy, huh?");
                //Msg("Talk to the good people in Tir Chonaill as much as you can, and pay close attention to what they say.<br/>Once you become friends with them, they will help you in many ways.<br/>Why don't you start off by visiting the buildings around the Square?");
                //Msg("Have you heard of field bosses?<br/>They are very powerful monsters that appear randomly in places outside dungeons, like open fields.<br/>Field bosses are either a Fomor or an animal affected by the forces of evil and transformed into a huge, savage creature.<p/>Field bosses usually show up with several monsters with them,<br/>so they pose a big threat to travelers.<br/>If you want to face a field boss, the people in town will tell you about them<br/>if you ask about nearby rumors a few times.");
                // MoodChange
                break;

            case "about_skill":
                Msg("I don't know of any skills... Why don't you ask Malcom?");
                //Msg("You know about the Combat Mastery skill?<br/>It's one of the basic skills needed to protect yourself in combat.<br/>It may look simple, but never underestimate its efficiency.<br/>Continue training the skill diligently and you will soon reap the rewards. That's a promise.");
                break;

            case "about_arbeit":
                Msg("I don't have any jobs for you, but you can get a part time job in town.");
                //Msg("Are you interested in a part-time job?<br/>It's great to see young people eager to work!<br/>To get one, talk to the people in town with the 'Part-Time Jobs' keyword.<br/>If you go at the right time, you'll be offered a job.<p/>If you do a good job, you will be duly rewarded.<br/>Just make sure to return to the person who gave you the job and report the results before the deadline.<br/>If you miss the deadline, you will not be rewarded regardless of how hard you worked.<p/>Part-time jobs aren't available 24 hours a day.<br/>You have to get there at the right time.<p/>The sign-up period usually begins between 7:00 am and 9:00 am.<br/>Since there are only a limited number of jobs available,<br/>others may take them all if you're too late.<br/>Also, you can do only one part-time job per day.<p/>It looks like Nora and Caitin could use your help,<br/>so head to the Grocery Store or the Inn and talk to them.<br/>Start the conversation with them with the keyword 'Part-Time Jobs' and make sure it's between 7 and 9 am.<br/>Good luck!");
                break;

            case "about_study":
                Msg("You can study different magic down at the school!");
                //Msg("Ah, you'll need to go to the School for that.<br/>Talk to one of the teachers with that keyword.<br/>That should get you started with classes.<p/>Find the guidepost near the Bank down the street.<br/>Once you do, it should be easy to locate the School.<br/>Keep in mind that the guideposts around town are there to help you out.");
                break;

            case "shop_misc":
                Msg("If you look down at the Square, you can see a building with a dark roof.<br/>That's the General Shop,<br/>where Malcolm sells homemade products.<br/>The quality of his products are quite good.");
                break;

            case "shop_grocery":
                Msg("Caitin from the Grocery Store is a diligent girl.<br/>Growing all those vegetables, cooking all that food, and running the business all by herself,<br/>it's certainly not as easy as it sounds.<br/>To find her, you can check the Minimap at the upper right corner of your screen.<br/>Press 'M' to toggle the Minimap.");
                break;

            case "shop_bank":
                Msg("It's been a while since the Erskin Bank first opened its doors...<br/>It's that big building with a tiled roof below in the Square.<br/>There, you'll find Bebhinn, the teller.<br/>She knows a lot of gossip, so talk to her if you're curious.");
                break;

            case "shop_smith":
                Msg("Anyone without a weapon should head there immediately.<br/>The shop does not carry a wide variety of weapons,<br/>since they're short-handed and can only make few weapons at a time.<br/>The quality, however, is good, and they last a while...<br/>Best of all, they are quite affordable...");
                break;

            case "skill_counter_attack":
                Msg("Haha, I am just the Chief of a small town. I don't know the details of that...<br/>I'm too old to give demonstrations, don't you think?<br/>Why don't you go ask Trefor to teach you?<br/>Go farther up the hill past the Healer's House, and you will see him.");
                break;

            case "square":
                Msg("The Square is perhaps the liveliest area in town.<br/>Many people use it as a meeting place.");
                break;

            case "farmland":
                Msg("Head south, and you'll find farmland spread across the fields.<br/>I'm concerned, though. Travelers walk through the fields as they please,<br/>and end up damaging our crops.<p/>Now I don't think they have bad intentions...<br/>But it's so hard to maintain viable farmland<br/>in such an inhospitable location at the foot of a mountain.<br/>And that farmland is essential to the well-being of my town.<p/>Hey, I'm not pointing any fingers, I'm just saying you should be careful, okay?<br/>Knowledge is power, right? And now you know...");
                break;

            case "shop_headman":
                Msg("That's right. This is my house.<br/>A strange cat appeared one day and made himself at home.<br/>I think it might even have some special powers.<br/>If you're curious, try talking to it.");
                break;

            case "temple":
                Msg("Have you talked to Priest Meven at the Church?<br/>Walk down near the School and you should easily find the Church.");
                break;

            case "skill_campfire":
                Msg("As a matter of fact, I have a book about campfires at home.<br/>Or rather, had. The shephard boy Dejan snuck it out some time ago.<br/>Haha. Why don't you go and talk to him?<br/>But don't mention that I know he took my book.");
                break;

            case "shop_restaurant":
                Msg("You're looking for a restaurant? You must be starving...<br/>Then head to the Grocery Store.<br/>Caitin's an amazing cook, and her food should fill you up.");
                break;

            case "shop_armory":
                Msg("Are you looking for a better weapon?<br/>If so, head south and you'll find the Blacksmith's Shop where Ferghus works.<br/>Tir Chonaill is known for its fearsome warriors, but interestingly enough, there isn't a single Weapon Shop in town.<p/>Why? Because only beginners and cowards entrust their lives to weapons!<br/>If you want to become a true warrior, never mistake your weapon's power for your own.");
                break;

            case "shop_goverment_office":
                Msg("The Town Office...<br/>I don't know if you're interested in the Town Office building itself, or in the services a Town Office provides.<p/>This is Tir Chonaill in the Ulaid province.<br/>We are not under the control of the Aliech Kingdom, which is located far to the south.<br/>Most Town Offices are governed by the Aliech Kingdom.<br/>The nearest one is in Dunbarton, which is located just south of here.<br/>If you're just looking to recover lost items, I can help you with that.");
                break;

            default:
                RndMsg(
                    "I don't know anything about that...",
                    "I think it'd be better for you to ask someone else.",
                    "Hmm, I wonder who might know about that...",
                    "I have no idea...",
                    "I don't really know about that... "
                );
                // MoodChange
                break;
        }
    }
}
