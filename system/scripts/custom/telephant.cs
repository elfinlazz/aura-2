// --- Aura Script ----------------------------------------------------------
//  Telephant
// --- Description ----------------------------------------------------------
//  Warper NPC
// --- By -------------------------------------------------------------------
//  Miro, exec
// --------------------------------------------------------------------------

public class _TelephantBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("Telephant");
		SetRace(550001);
	}

	protected override async Task Talk()
	{
		Msg("Hello! I am Telephant, and I'm here to carry you all over the world!");

	L_Selection:
		Msg("Where would you like to go?",
			Button("Never Mind", "@nvm"),
			List("Categories", 10, "@nvm",
				Button("Events / Customized", "@custom"),
				Button("Uladh", "@uladh"),
				Button("Iria", "@iria"),
				Button("Another World", "@a_world"),
				Button("Others", "@others")
			)
		);

		var category = await Select();

		if (category == "@nvm")
			Close("Come back any time!");

		var list = List("Locations", 10, "@back");

		switch (category)
		{
			case "@custom":
				list.Add(Button("Nekojima", "@neko"));
				list.Add(Button("The Moon", "@moon"));
				list.Add(Button("Soul Stream", "@soul"));
				break;

			case "@uladh":
				list.Add(Button("Tir Chonaill", "@tir"));
				list.Add(Button("Dunbarton", "@dun"));
				list.Add(Button("Bangor", "@bangor"));
				list.Add(Button("Emain Macha", "@emain"));
				list.Add(Button("Taillteann", "@tail"));
				list.Add(Button("Tara", "@tara"));
				list.Add(Button("Port Cobh", "@cobh"));
				list.Add(Button("Ceo Island", "@ceo"));
				break;

			case "@iria":
				list.Add(Button("Quilla Base Camp", "@quilla"));
				list.Add(Button("Filia", "@filia"));
				list.Add(Button("Vales", "@vales"));
				list.Add(Button("Cor", "@cor"));
				list.Add(Button("Calida", "@calida"));
				break;

			case "@a_world":
				list.Add(Button("Crossroads", "@cross"));
				list.Add(Button("Bangor (A)", "@bangor_a"));
				list.Add(Button("Gairech Hills (A)", "@gairech_a"));
				list.Add(Button("Tir Chonaill (A)", "@tir_a"));
				break;

			case "@others":
				list.Add(Button("Belvast", "@belvast"));
				list.Add(Button("Avon", "@avon"));
				list.Add(Button("Falias", "@falias"));
				break;
		}

		Msg("What location would you like to go to?", list);
		var location = await Select();

		if (location == "@back")
			goto L_Selection;

		await Warp(location);
	}

	private async Task Warp(string location)
	{
		int region = 0, x = 0, y = 0;

		switch (location)
		{
			case "@neko": region = 600; x = 93757; y = 88234; break;
			case "@moon": region = 1003; x = 7058; y = 6724; break;
			case "@soul": region = 1000; x = 6368; y = 7150; break;

			case "@tir": region = 1; x = 12991; y = 38549; break;
			case "@dun": region = 14; x = 38001; y = 38802; break;
			case "@bangor": region = 31; x = 12904; y = 12200; break;
			case "@emain": region = 52; x = 39818; y = 41621; break;
			case "@tail": region = 300; x = 212749; y = 192720; break;
			case "@tara": region = 401; x = 99793; y = 91209; break;
			case "@cobh": region = 23; x = 28559; y = 37693; break;
			case "@ceo": region = 56; x = 8743; y = 9299; break;

			case "@quilla": region = 3001; x = 166562; y = 168930; break;
			case "@filia": region = 3100; x = 373654; y = 424901; break;
			case "@vales": region = 3200; x = 289556; y = 211936; break;
			case "@cor": region = 3300; x = 254233; y = 186929; break;
			case "@calida": region = 3400; x = 328825; y = 176094; break;

			case "@cross": region = 51; x = 10410; y = 10371; break;
			case "@tir_a": region = 35; x = 12801; y = 38380; break;
			case "@bangor_a": region = 84; x = 12888; y = 7986; break;
			case "@gairech_a": region = 83; x = 38405; y = 47366; break;

			case "@belvast": region = 4005; x = 63373; y = 26475; break;
			case "@avon": region = 501; x = 64195; y = 63211; break;
			case "@falias": region = 500; x = 11839; y = 23832; break;

			// Unknown location, just cancel.
			default: Close(); break;
		}

		Msg("You will be teleported now, see you on the other side!", Button("Okay"));
		await Select();

		Player.Warp(region, x, y);

		Close();
	}
}

public class TelephantAvonScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(501, 65355, 63105, 125); } }
public class TelephantBangorScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(31, 11634, 12285, 0); } }
public class TelephantBangorAScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(84, 12887, 7657, 60); } }
public class TelephantBelvastScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(4005, 63661, 25973, 80); } }
public class TelephantCalidaScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(3400, 328767, 176263, 210); } }
public class TelephantCeoScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(56, 8250, 9340, 0); } }
public class TelephantCobhScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(23, 28773, 37705, 130); } }
public class TelephantCorScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(3300, 254100, 187111, 215); } }
public class TelephantCrossroadScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(51, 10040, 10317, 0); } }
public class TelephantDunScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(14, 38000, 39629, 190); } }
public class TelephantEmainScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(52, 39948, 41431, 80); } }
public class TelephantFaliasScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(500, 11770, 23458, 50); } }
public class TelephantFiliaScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(3100, 374015, 424585, 95); } }
public class TelephantGairechAScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(83, 38366, 47743, 190); } }
public class TelephantMoonScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(1003, 6868, 7008, 220); } }
public class TelephantMoon2Script : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(1003, 20843, 20083, 100); } }
public class TelephantNekoScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(600, 93654, 88089, 30); } }
public class TelephantQuillaScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(3001, 166788, 168716, 95); } }
public class TelephantSoulStreamScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(1000, 5678, 7193, 0); } }
public class TelephantTailScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(300, 212336, 194269, 200); } }
public class TelephantTaraScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(401, 100092, 91201, 125); } }
public class TelephantTirScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(1, 13189, 38776, 160); } }
public class TelephantTirAScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(35, 13585, 38385, 125); } }
public class TelephantValesScript : _TelephantBaseScript { public override void Load() { base.Load(); SetLocation(3200, 289848, 212069, 140); } }
