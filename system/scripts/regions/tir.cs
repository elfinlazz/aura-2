//--- Aura Script -----------------------------------------------------------
// Tir Chonaill
//--- Description -----------------------------------------------------------
// Warp, prop, and spawn definitions for Tir.
//---------------------------------------------------------------------------

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Network.Sending;

public class TirRegionScript : BaseScript
{
	public override void Load()
	{
		LoadWarps();
		LoadPropDrops();
	}
	
	public void LoadWarps()
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
	
	public void LoadPropDrops()
	{
		SetPropBehavior(45036000568737807, PropDrop(2));
		SetPropBehavior(45036000568737810, PropDrop(2));
		SetPropBehavior(45036000568737814, PropDrop(2));
		SetPropBehavior(45036000568737817, PropDrop(2));
		SetPropBehavior(45036000568737819, PropDrop(2));
		SetPropBehavior(45036000568868868, PropDrop(2));
		SetPropBehavior(45036000568868869, PropDrop(2));
		SetPropBehavior(45036000568868874, PropDrop(2));
		SetPropBehavior(45036000568868876, PropDrop(2));
		SetPropBehavior(45036000568868896, PropDrop(1));
		SetPropBehavior(45036000568868898, PropDrop(1));
		SetPropBehavior(45036000568868904, PropDrop(2));
		SetPropBehavior(45036000568868913, PropDrop(2));
		SetPropBehavior(45036000568868920, PropDrop(1));
		SetPropBehavior(45036000568868929, PropDrop(2));
		SetPropBehavior(45036000568868934, PropDrop(2));
		SetPropBehavior(45036000568868941, PropDrop(2));
		SetPropBehavior(45036000568868971, PropDrop(1));
		SetPropBehavior(45036000568934412, PropDrop(2));
		SetPropBehavior(45036000568934415, PropDrop(2));
		SetPropBehavior(45036000568934419, PropDrop(1));
		SetPropBehavior(45036000568934421, PropDrop(2));
		SetPropBehavior(45036000568999942, PropDrop(2));
		SetPropBehavior(45036000568999946, PropDrop(1));
		SetPropBehavior(45036000568999958, PropDrop(1));
		SetPropBehavior(45036000568999972, PropDrop(1));
		SetPropBehavior(45036000568999980, PropDrop(1));
		SetPropBehavior(45036000568999988, PropDrop(1));
		SetPropBehavior(45036000568999989, PropDrop(1));
		SetPropBehavior(45036000569000005, PropDrop(2));
		SetPropBehavior(45036000569000010, PropDrop(2));
		SetPropBehavior(45036000569065489, PropDrop(2));
		SetPropBehavior(45036000569065491, PropDrop(1));
		SetPropBehavior(45036000569065497, PropDrop(2));
		SetPropBehavior(45036000569065503, PropDrop(2));
		SetPropBehavior(45036000569065521, PropDrop(2));
		SetPropBehavior(45036000569065522, PropDrop(1));
		SetPropBehavior(45036000569131010, PropDrop(1));
		SetPropBehavior(45036000569131020, PropDrop(2));
		SetPropBehavior(45036000569131023, PropDrop(2));
		SetPropBehavior(45036000569131028, PropDrop(2));
		SetPropBehavior(45036000569131041, PropDrop(1));
		SetPropBehavior(45036000569131043, PropDrop(2));
		SetPropBehavior(45036000569131044, PropDrop(54));
		SetPropBehavior(45036000569131050, PropDrop(2));
		SetPropBehavior(45036000569131051, PropDrop(2));
		SetPropBehavior(45036000569131052, PropDrop(1));
		SetPropBehavior(45036000569196553, PropDrop(1));
		SetPropBehavior(45036000569196554, PropDrop(2));
		SetPropBehavior(45036000569196556, PropDrop(2));
		SetPropBehavior(45036000569196572, PropDrop(2));
		SetPropBehavior(45036000569196576, PropDrop(1));
		SetPropBehavior(45036000569196580, PropDrop(2));
		SetPropBehavior(45036000569196583, PropDrop(1));
		SetPropBehavior(45036000569196584, PropDrop(1));
		SetPropBehavior(45036000569196589, PropDrop(1));
		SetPropBehavior(45036000569196604, PropDrop(2));
		SetPropBehavior(45036000569196616, PropDrop(1));
		SetPropBehavior(45036000569196683, PropDrop(1));
		SetPropBehavior(45036000569196684, PropDrop(1));
		SetPropBehavior(45036000569196685, PropDrop(1));
		SetPropBehavior(45036000569196686, PropDrop(1));
		SetPropBehavior(45036000569196687, PropDrop(1));
		SetPropBehavior(45036000569196688, PropDrop(1));
		SetPropBehavior(45036000569196689, PropDrop(1));
		SetPropBehavior(45036000569196690, PropDrop(1));
		SetPropBehavior(45036000569196691, PropDrop(1));
		SetPropBehavior(45036000569196698, PropDrop(1));
		SetPropBehavior(45036000569262081, PropDrop(1));
		SetPropBehavior(45036000569262083, PropDrop(2));
		SetPropBehavior(45036000569262085, PropDrop(2));
		SetPropBehavior(45036000569262093, PropDrop(1));
		SetPropBehavior(45036000569262095, PropDrop(2));
		SetPropBehavior(45036000569262101, PropDrop(2));
		SetPropBehavior(45036000569262109, PropDrop(1));
		SetPropBehavior(45036000569262112, PropDrop(1));
		SetPropBehavior(45036000569262116, PropDrop(1));
		SetPropBehavior(45036000569262119, PropDrop(2));
		SetPropBehavior(45036000569262124, PropDrop(2));
		SetPropBehavior(45036000569262127, PropDrop(1));
		SetPropBehavior(45036000569262130, PropDrop(1));
		SetPropBehavior(45036000569262132, PropDrop(1));
		SetPropBehavior(45036000569262135, PropDrop(1));
		SetPropBehavior(45036000569262138, PropDrop(1));
		SetPropBehavior(45036000569262139, PropDrop(1));
		SetPropBehavior(45036000569262142, PropDrop(2));
		SetPropBehavior(45036000569262148, PropDrop(2));
		SetPropBehavior(45036000569262149, PropDrop(2));
		SetPropBehavior(45036000569262169, PropDrop(2));
		SetPropBehavior(45036000569262170, PropDrop(2));
		SetPropBehavior(45036000569262176, PropDrop(1));
		SetPropBehavior(45036000569262177, PropDrop(2));
		SetPropBehavior(45036000569262184, PropDrop(1));
		SetPropBehavior(45036000569262194, PropDrop(2));
		SetPropBehavior(45036000569262197, PropDrop(2));
		SetPropBehavior(45036000569262202, PropDrop(2));
		SetPropBehavior(45036000569262207, PropDrop(1));
		SetPropBehavior(45036000569262213, PropDrop(2));
		SetPropBehavior(45036000569262220, PropDrop(2));
		SetPropBehavior(45036000569262238, PropDrop(1));
		SetPropBehavior(45036000569262241, PropDrop(1));
		SetPropBehavior(45036000569262243, PropDrop(2));
		SetPropBehavior(45036000569262245, PropDrop(2));
		SetPropBehavior(45036000569262246, PropDrop(1));
		SetPropBehavior(45036000569262248, PropDrop(1));
		SetPropBehavior(45036000569262249, PropDrop(2));
		SetPropBehavior(45036000569262250, PropDrop(1));
		SetPropBehavior(45036000569262252, PropDrop(1));
		SetPropBehavior(45036000569262259, PropDrop(1));
		SetPropBehavior(45036000569262269, PropDrop(2));
		SetPropBehavior(45036000569262271, PropDrop(2));
		SetPropBehavior(45036000569262274, PropDrop(2));
		SetPropBehavior(45036000569262279, PropDrop(2));
		SetPropBehavior(45036000569262280, PropDrop(2));
		SetPropBehavior(45036000569262282, PropDrop(2));
		SetPropBehavior(45036000569262298, PropDrop(1));
		SetPropBehavior(45036000569262302, PropDrop(1));
		SetPropBehavior(45036000569262308, PropDrop(2));
		SetPropBehavior(45036000569262316, PropDrop(2));
		SetPropBehavior(45036000569262319, PropDrop(1));
		SetPropBehavior(45036000569262323, PropDrop(2));
		SetPropBehavior(45036000569262335, PropDrop(2));
		SetPropBehavior(45036000569262358, PropDrop(2));
		SetPropBehavior(45036000569262363, PropDrop(2));
		SetPropBehavior(45036000569262365, PropDrop(1));
		SetPropBehavior(45036000569262373, PropDrop(2));
		SetPropBehavior(45036000569262381, PropDrop(1));
		SetPropBehavior(45036000569262387, PropDrop(2));
		SetPropBehavior(45036000569262389, PropDrop(2));
		SetPropBehavior(45036000569262392, PropDrop(1));
		SetPropBehavior(45036000569262395, PropDrop(2));
		SetPropBehavior(45036000569262402, PropDrop(2));
		SetPropBehavior(45036000569262406, PropDrop(2));
		SetPropBehavior(45036000569262407, PropDrop(2));
		SetPropBehavior(45036000569262409, PropDrop(2));
		SetPropBehavior(45036000569262417, PropDrop(2));
		SetPropBehavior(45036000569262429, PropDrop(2));
		SetPropBehavior(45036000569262431, PropDrop(2));
		SetPropBehavior(45036000569262444, PropDrop(2));
		SetPropBehavior(45036000569262448, PropDrop(2));
		SetPropBehavior(45036000569262451, PropDrop(1));
		SetPropBehavior(45036000569262463, PropDrop(2));
		SetPropBehavior(45036000569262481, PropDrop(1));
		SetPropBehavior(45036000569262482, PropDrop(2));
		SetPropBehavior(45036000569262484, PropDrop(1));
		SetPropBehavior(45036000569262486, PropDrop(2));
		SetPropBehavior(45036000569262501, PropDrop(2));
		SetPropBehavior(45036000569262506, PropDrop(2));
		SetPropBehavior(45036000569262523, PropDrop(1));
		SetPropBehavior(45036000569262526, PropDrop(2));
		SetPropBehavior(45036000569262533, PropDrop(2));
		SetPropBehavior(45036000569262540, PropDrop(1));
		SetPropBehavior(45036000569262541, PropDrop(2));
		SetPropBehavior(45036000569262562, PropDrop(2));
		SetPropBehavior(45036000569262571, PropDrop(2));
		SetPropBehavior(45036000569262574, PropDrop(1));
		SetPropBehavior(45036000569262576, PropDrop(1));
		SetPropBehavior(45036000569262586, PropDrop(2));
		SetPropBehavior(45036000569262595, PropDrop(1));
		SetPropBehavior(45036000569262596, PropDrop(2));
		SetPropBehavior(45036000569262599, PropDrop(2));
		SetPropBehavior(45036000569262606, PropDrop(1));
		SetPropBehavior(45036000569262610, PropDrop(2));
		SetPropBehavior(45036000569262620, PropDrop(1));
		SetPropBehavior(45036000569262628, PropDrop(2));
		SetPropBehavior(45036000569262629, PropDrop(2));
		SetPropBehavior(45036000569262644, PropDrop(1));
		SetPropBehavior(45036000569262645, PropDrop(2));
		SetPropBehavior(45036000569262646, PropDrop(1));
		SetPropBehavior(45036000569262647, PropDrop(2));
		SetPropBehavior(45036000569262660, PropDrop(1));
		SetPropBehavior(45036000569262687, PropDrop(2));
		SetPropBehavior(45036000569262690, PropDrop(2));
		SetPropBehavior(45036000569262705, PropDrop(2));
		SetPropBehavior(45036000569262713, PropDrop(1));
		SetPropBehavior(45036000569262714, PropDrop(2));
		SetPropBehavior(45036000569262724, PropDrop(1));
		SetPropBehavior(45036000569262726, PropDrop(1));
		SetPropBehavior(45036000569262729, PropDrop(1));
		SetPropBehavior(45036000569262733, PropDrop(1));
		SetPropBehavior(45036000569262739, PropDrop(1));
		SetPropBehavior(45036000569262763, PropDrop(2));
		SetPropBehavior(45036000569262765, PropDrop(2));
		SetPropBehavior(45036000569262766, PropDrop(2));
		SetPropBehavior(45036000569262769, PropDrop(1));
		SetPropBehavior(45036000569262774, PropDrop(1));
		SetPropBehavior(45036000569262777, PropDrop(1));
		SetPropBehavior(45036000569262782, PropDrop(2));
		SetPropBehavior(45036000569262783, PropDrop(2));
		SetPropBehavior(45036000569262789, PropDrop(1));
		SetPropBehavior(45036000569262802, PropDrop(2));
		SetPropBehavior(45036000569262803, PropDrop(1));
		SetPropBehavior(45036000569262804, PropDrop(2));
		SetPropBehavior(45036000569262805, PropDrop(1));
		SetPropBehavior(45036000569262819, PropDrop(2));
		SetPropBehavior(45036000569262823, PropDrop(2));
		SetPropBehavior(45036000569262829, PropDrop(2));
		SetPropBehavior(45036000569262832, PropDrop(2));
		SetPropBehavior(45036000569262840, PropDrop(2));
		SetPropBehavior(45036000569262846, PropDrop(2));
		SetPropBehavior(45036000569262853, PropDrop(1));
		SetPropBehavior(45036000569262858, PropDrop(2));
		SetPropBehavior(45036000569262862, PropDrop(1));
		SetPropBehavior(45036000569262914, PropDrop(1));
		SetPropBehavior(45036000569262925, PropDrop(1));
		SetPropBehavior(45036000569262937, PropDrop(2));
		SetPropBehavior(45036000569262938, PropDrop(2));
		SetPropBehavior(45036000569262939, PropDrop(2));
		SetPropBehavior(45036000569262946, PropDrop(2));
		SetPropBehavior(45036000569262956, PropDrop(2));
		SetPropBehavior(45036000569262973, PropDrop(1));
		SetPropBehavior(45036000569262987, PropDrop(2));
		SetPropBehavior(45036000569262991, PropDrop(2));
		SetPropBehavior(45036000569263001, PropDrop(2));
		SetPropBehavior(45036000569263004, PropDrop(1));
		SetPropBehavior(45036000569263009, PropDrop(2));
		SetPropBehavior(45036000569263018, PropDrop(2));
		SetPropBehavior(45036000569263031, PropDrop(2));
		SetPropBehavior(45036000569263039, PropDrop(2));
		SetPropBehavior(45036000569263042, PropDrop(2));
		SetPropBehavior(45036000569263061, PropDrop(1));
		SetPropBehavior(45036000569263062, PropDrop(2));
		SetPropBehavior(45036000569263067, PropDrop(2));
		SetPropBehavior(45036000569263073, PropDrop(2));
		SetPropBehavior(45036000569263076, PropDrop(2));
		SetPropBehavior(45036000569263079, PropDrop(2));
		SetPropBehavior(45036000569263083, PropDrop(1));
		SetPropBehavior(45036000569263088, PropDrop(2));
		SetPropBehavior(45036000569263091, PropDrop(1));
		SetPropBehavior(45036000569263092, PropDrop(2));
		SetPropBehavior(45036000569263117, PropDrop(1));
		SetPropBehavior(45036000569263121, PropDrop(2));
		SetPropBehavior(45036000569263123, PropDrop(2));
		SetPropBehavior(45036000569263134, PropDrop(2));
		SetPropBehavior(45036000569263154, PropDrop(2));
		SetPropBehavior(45036000569263155, PropDrop(1));
		SetPropBehavior(45036000569263162, PropDrop(2));
		SetPropBehavior(45036000569263169, PropDrop(1));
		SetPropBehavior(45036000569263173, PropDrop(2));
		SetPropBehavior(45036000569263178, PropDrop(1));
		SetPropBehavior(45036000569263186, PropDrop(2));
		SetPropBehavior(45036000569263197, PropDrop(1));
		SetPropBehavior(45036000569263206, PropDrop(1));
		SetPropBehavior(45036000569263208, PropDrop(1));
		SetPropBehavior(45036000569263220, PropDrop(2));
		SetPropBehavior(45036000569263223, PropDrop(1));
		SetPropBehavior(45036000569263225, PropDrop(2));
		SetPropBehavior(45036000569263236, PropDrop(2));
		SetPropBehavior(45036000569263243, PropDrop(2));
		SetPropBehavior(45036000569263253, PropDrop(1));
		SetPropBehavior(45036000569263255, PropDrop(2));
		SetPropBehavior(45036000569263256, PropDrop(2));
		SetPropBehavior(45036000569263257, PropDrop(2));
		SetPropBehavior(45036000569263262, PropDrop(1));
		SetPropBehavior(45036000569263263, PropDrop(2));
		SetPropBehavior(45036000569263267, PropDrop(2));
		SetPropBehavior(45036000569263283, PropDrop(2));
		SetPropBehavior(45036000569263293, PropDrop(2));
		SetPropBehavior(45036000569263295, PropDrop(2));
		SetPropBehavior(45036000569263297, PropDrop(2));
		SetPropBehavior(45036000569263300, PropDrop(1));
		SetPropBehavior(45036000569263310, PropDrop(2));
		SetPropBehavior(45036000569263311, PropDrop(2));
		SetPropBehavior(45036000569263322, PropDrop(1));
		SetPropBehavior(45036000569263323, PropDrop(1));
		SetPropBehavior(45036000569263327, PropDrop(2));
		SetPropBehavior(45036000569263328, PropDrop(2));
		SetPropBehavior(45036000569263330, PropDrop(1));
		SetPropBehavior(45036000569263364, PropDrop(841));
	}
}
