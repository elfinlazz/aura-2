using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Network.Sending;

public class TirPortals : BaseScript
{
	public override void Load()
	{
		//SetPropBehavior(45036533145010804, PropWarp(1, 15388, 38706));

		// Bank
		SetPropBehavior(45036000569263146, PropWarp(2, 2102, 1184));
		SetPropBehavior(45036004863705089, PropWarp(1, 11475, 39606));

		// Duncan
		SetPropBehavior(45036000569262524, PropWarp(3, 2351, 1880));
		SetPropBehavior(45036009158672385, PropWarp(1, 16061, 38154));

		// Church
		SetPropBehavior(45036000569262795, PropWarp(4, 1693, 687));
		SetPropBehavior(45036013453639682, PropWarp(1, 5888, 37471));

		// Grocery Store
		SetPropBehavior(45036000569262904, PropWarp(5, 2310, 1712));
		SetPropBehavior(45036017748606977, PropWarp(1, 11482, 37274));
		
		// Healer
		SetPropBehavior(45036000569262443, PropWarp(6, 664, 854));
		SetPropBehavior(45036022043574273, PropWarp(1, 13575, 44611));

		// Inn
		SetPropBehavior(45036000569262537, PropWarp(7, 1419, 685));
		SetPropBehavior(45036026338541569, PropWarp(1, 15814, 33691));
		
		// General Shop
		SetPropBehavior(45036000569262320, PropWarp(8, 845, 2440));
		SetPropBehavior(45036030633508867, PropWarp(1, 13113, 36454));

		// Shool
		SetPropBehavior(45036000569262997, PropWarp(9, 2348, 886));
		SetPropBehavior(45036034928476161, PropWarp(1, 4093, 32909));

		// Ciar
		SetPropBehavior(45036000568999945, PropWarp(11, 3886, 3297));
		SetPropBehavior(45036043518410754, PropWarp(1, 28761, 30725));

		// Alby
		SetPropBehavior(45036000569196606, PropWarp(13, 3197, 2518));
		SetPropBehavior(45036052108345350, PropWarp(1, 9756, 59227));

		// Dugald Aisle
		SetPropBehavior(45036000568868967, PropWarp(16, 28545, 96881));
		SetPropBehavior(45036064993312808, PropWarp(1, 5067, 17156));

		// Sidhe
		SetPropBehavior(45036000569196647, PropWarp(47, 9985, 6522));
		SetPropBehavior(45036198137233609, PropWarp(1, 1748, 59187));

		// Beginner Tutorial
		SetPropBehavior(45040308420935876, PropWarp(1, 12785, 38383));
		SetPropBehavior(45040308420936241, PropWarp(1, 12785, 38383));
		SetPropBehavior(45040308420935963, PropWarp(1004, 18517, 12543));
		SetPropBehavior(45040308420935964, PropWarp(1004, 17654, 14772));
		
		SetPropBehavior(45036515964813322, PropWarp(11, 3193, 4319));
		SetPropBehavior(45036043518410789, (cr, pr) =>
		{
			if(cr.LevelTotal >= 250)
				cr.Warp(121, 3206, 2085);
			else
				Send.Notice(cr, "You need a cumulative level of at least 250.");
		});

		// TNN indoor -> Tir ...?
		//SetPropBehavior(45036150892593154, PropWarp(1, 16061, 38154));
		//SetPropBehavior(45036155187560450, PropWarp(1, 5888, 37471));
		//SetPropBehavior(45036159482527745, PropWarp(1, 11482, 37274));
		//SetPropBehavior(45036163777495042, PropWarp(1, 13575, 44611));
		//SetPropBehavior(45036168072462338, PropWarp(1, 15814, 33691));
		//SetPropBehavior(45036172367429633, PropWarp(1, 13113, 36454));
		//SetPropBehavior(45036176662396930, PropWarp(1, 4093, 32909));
		//SetPropBehavior(45036180957364226, PropWarp(1, 11475, 39606));
	}
}
