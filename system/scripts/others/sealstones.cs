//--- Aura Script -----------------------------------------------------------
// Seal Stones
//--- Description -----------------------------------------------------------
// Restrict access to specific areas through placement of shapely rocks.
//---------------------------------------------------------------------------

// Dugald
// --------------------------------------------------------------------------

public class DugaldSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Dugald Aisle", "_sealstone_dugald");
		SetLocation(16, 19798, 4456, 1.48f);
		SetHelp("The Seal of Dugald Aisle\n\nGet 20 ranks in skills.");
	}

	public override bool Check(Creature creature, Prop prop)
	{
		// Number Total Skill Ranks over 20 
		int ranks = creature.Skills.CountRanks();
		return (ranks >= 20);
	}

	public override void OnBreak(Creature cr)
	{
		cr.Titles.Enable(10002); // the Dugald Aisle Seal Breaker
	}
}

// Ciar
// --------------------------------------------------------------------------

public class CiarSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Ciar Dungeon", "_sealstone_ciar");
		SetLocation(1, 28003, 30528, 0.16f);
		SetHelp("The Seal of Ciar Dungeon\n\nGet over 35 Strength.");
	}

	public override bool Check(Creature creature, Prop prop)
	{
		return (creature.Str >= 35);
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10003); // the Ciar Seal Breaker
	}
}

// Rabbie
// --------------------------------------------------------------------------

public class RabbieSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Rabbie Dungeon", "_sealstone_rabbie");
		SetLocation(14, 16801, 58978, 4.71f);
		SetHelp("The Seal of Rabbie Dungeon\n\nReach level 35+.");
	}

	public override bool Check(Creature creature, Prop prop)
	{
		return (creature.Level >= 35);
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10004); // the Rabbie Seal Breaker
	}
}

// Math
// --------------------------------------------------------------------------

public class MathSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Math Dungeon", "_sealstone_math");
		SetLocation(14, 58409, 58185, 4.71f);
		SetHelp("The Seal of Math Dungeon\n\nBe a good little bard.");
	}

	public override bool Check(Creature creature, Prop prop)
	{
		// Must have rank D Playing Instrument, Composing, and Musical Knowledge
		return (
			(creature.Skills.Has(SkillId.PlayingInstrument) && creature.Skills.Get(SkillId.PlayingInstrument).Info.Rank >= SkillRank.RD) &&
			(creature.Skills.Has(SkillId.Composing) && creature.Skills.Get(SkillId.Composing).Info.Rank >= SkillRank.RD) &&
			(creature.Skills.Has(SkillId.MusicalKnowledge) && creature.Skills.Get(SkillId.MusicalKnowledge).Info.Rank >= SkillRank.RD)
		);
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10005); // the Math Seal Breaker
	}
}

// Bangor
// --------------------------------------------------------------------------

public class BangorSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Bangor", "_sealstone_bangor");
		SetLocation(30, 39189, 17014, 1.54f);
		SetHelp("The Seal of Bangor\n\nBangor needs archers! Eh...");
	}

	public override bool Check(Creature creature, Prop prop)
	{
		// Must have 13+ ranks of Archery Skills
		var ranks = creature.Skills.CountRanks(SkillId.RangedAttack, SkillId.MagnumShot, SkillId.ArrowRevolver, SkillId.ArrowRevolver2, SkillId.SupportShot, SkillId.MirageMissile);
		return (ranks >= 13);
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10006); // the Bangor Breaker
	}
}

// Fiodh
// --------------------------------------------------------------------------

public class FiodhSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Fiodh Dungeon", "_sealstone_fiodh");
		SetLocation(30, 10696, 83099, 4.7f);
		SetHelp("The Seal of Fiodh Dungeon\n\nGot titles?");
	}

	public override bool Check(Creature creature, Prop prop)
	{
		return (creature.Titles.Count >= 18);
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10008); // the Fiodh Breaker
	}
}

// North Emain Macha
// --------------------------------------------------------------------------

public class NorthEmainSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of North Emain Macha", "_sealstone_osnasail");
		SetLocation(70, 7844, 13621, 0);
		SetHelp("The Seal of North Emain Macha\n\nExperience before Age.");
		
		if(!IsEnabled("EmainMacha"))
			SetLock(true);
	}

	public override bool Check(Creature creature, Prop prop)
	{
		return (creature.Level >= (creature.Age * 4));
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10025); // the North Emain Macha Seal Breaker
	}
}

// South Emain Macha
// --------------------------------------------------------------------------

public class SouthEmainSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of South Emain Macha", "_sealstone_south_emainmacha");
		SetLocation(53, 67830, 107710, 0);
		SetHelp("The Seal of South Emain Macha\n\nExperience before Age.");
		
		if(!IsEnabled("EmainMacha"))
			SetLock(true);
	}

	public override bool Check(Creature creature, Prop prop)
	{
		return (creature.Level >= (creature.Age * 4));
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10009); // the South Emain Macha Seal Breaker
	}
}

// Abb Neagh
// --------------------------------------------------------------------------

public class AbbSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Abb Neagh", "_sealstone_south_taillteann");
		SetLocation(14, 14023, 56756, 0);
		SetHelp("The Seal of Abb Neagh\n\nBlah, Wand, blah, Mage.");
		SetLock(true);
	}

	public override bool Check(Creature creature, Prop prop)
	{
		// Wand
		if (creature.RightHand != null && creature.RightHand.Info.Id >= 40038 && creature.RightHand.Info.Id <= 40041)
			return true;

		return false;
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10068); // the Abb Neagh Seal Breaker
	}
}

// Sliab Cuilin
// --------------------------------------------------------------------------

public class SliabSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Sliab Cuilin", "_sealstone_east_taillteann");
		SetLocation(16, 6336, 62882, 0);
		SetHelp("The Seal of Sliab Cuilin\n\nUtilize Tracy's Secret.");
		SetLock(true);
	}

	public override bool Check(Creature creature, Prop prop)
	{
		return (creature.LeftHand != null && creature.LeftHand.Info.Id == 1028); // Tracy's Secret
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10067); // the Sliab Cuilin Seal Breaker
	}
}

// Tara
// --------------------------------------------------------------------------

public class TaraSealStoneScript : SealStoneScript
{
	public override void Setup()
	{
		SetName("Seal Stone of Tara", "_sealstone_tara");
		SetLocation(400, 56799, 33820, 2.23f);
		SetHelp("The Seal of Tara\n\nAlchemists only!!!");
		SetLock(true);
	}

	public override bool Check(Creature creature, Prop prop)
	{
		// Have alchemist clothes, shoes, a Cylinder, and Beginner Alchemist title equipped ?

		if (creature.Titles.SelectedTitle != 26)
			return false;

		// Shoes
		var item = creature.Inventory.GetItemAt(Pocket.Shoe, 0, 0);
		if (item == null || (item.Info.Id != 17138))
			return false;

		// Clothes
		item = creature.Inventory.GetItemAt(Pocket.Armor, 0, 0);
		if (item == null || (item.Info.Id != 15351))
			return false;

		// Cylinder
		if (creature.RightHand != null)
		{
			if (creature.RightHand.Info.Id == 40258) return true;
			if (creature.RightHand.Info.Id == 40270) return true;
			if (creature.RightHand.Info.Id == 40284) return true;
			if (creature.RightHand.Info.Id == 40285) return true;
			if (creature.RightHand.Info.Id == 40286) return true;
			if (creature.RightHand.Info.Id == 40287) return true;
			if (creature.RightHand.Info.Id == 40296) return true;
		}

		return false;
	}

	public override void OnBreak(Creature creature)
	{
		creature.Titles.Enable(10077); // the Tara Seal Breaker
	}
}

// Base Script
// --------------------------------------------------------------------------

public abstract class SealStoneScript : GeneralScript
{
	protected const bool AllowMultiple = false;

	protected string _name, _ident;
	protected int _region, _x, _y;
	protected float _direction;
	protected int _hits, _required = 10;
	protected string _help;
	protected bool _locked;

	public override void Load()
	{
		Setup();

		var stone = new Prop(40000, _region, _x, _y, _direction, 1, 0, "state1", _ident);

		if (GlobalVars.Perm["SealStoneId" + _ident] != null)
		{
			var id = (long)GlobalVars.Perm["SealStoneId" + _ident];
			var name = (string)GlobalVars.Perm["SealStoneName" + _ident];
			SetBreaker(stone, id, name);
		}

		SpawnProp(stone, OnHit);
	}

	public void OnHit(Creature creature, Prop pr)
	{
		lock (_ident)
		{
			if (_hits > _required)
				return;

			if (_locked)
			{
				Send.Notice(creature, "This seal stone cannot be broken yet.");
				return;
			}

			// You can only become breaker once officially.
			if (IsBreaker(creature) && !AllowMultiple && creature.Titles.SelectedTitle != 60001)
			{
				Send.Notice(creature, "Unable to break the Seal.\nYou already hold the title of a Seal Breaker.");
				return;
			}

			// Fulfilling the requirements?
			if (!Check(creature, pr))
			{
				Send.Notice(creature, _help);
				return;
			}

			_hits++;

			// Done
			if (_hits == _required)
			{
				SetBreaker(pr, creature.EntityId, creature.Name);

				GlobalVars.Perm["SealStoneId" + _ident] = creature.EntityId;
				GlobalVars.Perm["SealStoneName" + _ident] = creature.Name;

				OnBreak(creature);

				Send.PropUpdate(pr);
				Send.Notice(creature.Region, "{0} successfully broke {1} apart.", creature.Name, _name);
			}
			// Cracks after half.
			else if (_hits == Math.Floor(_required / 2f))
			{
				pr.State = "state2";

				Send.PropUpdate(pr);
				Send.Notice(creature.Region, "{0} has started breaking {1} apart.", creature.Name, _name);
			}
		}
	}

	private void SetBreaker(Prop prop, long entityId, string characterName)
	{
		prop.State = "state3";
		prop.Xml.SetAttributeValue("breaker_id", entityId);
		prop.Xml.SetAttributeValue("breaker_name", characterName);
		_hits = _required;
	}

	public void SetName(string name, string ident) { _name = name; _ident = ident; }
	public void SetLocation(int region, int x, int y, float direction) { _region = region; _x = x; _y = y; _direction = direction; }
	public void SetHelp(string help) { _help = help; }
	public void SetLock(bool locked) { _locked = locked; }

	public bool IsBreaker(Creature creature)
	{
		if (creature.Titles.IsUsable(10002)) return true; // the Dugald Aisle Seal Breaker
		if (creature.Titles.IsUsable(10003)) return true; // the Ciar Seal Breaker
		if (creature.Titles.IsUsable(10004)) return true; // the Rabbie Seal Breaker
		if (creature.Titles.IsUsable(10005)) return true; // the Math Seal Breaker
		if (creature.Titles.IsUsable(10006)) return true; // the Bangor Seal Breaker
		if (creature.Titles.IsUsable(10008)) return true; // the Fiodh Seal Breaker
		if (creature.Titles.IsUsable(10009)) return true; // the South Emain Macha Seal Breaker
		if (creature.Titles.IsUsable(10025)) return true; // the North Emain Macha Seal Breaker
		if (creature.Titles.IsUsable(10067)) return true; // the Sliab Cuilin Seal Breaker
		if (creature.Titles.IsUsable(10068)) return true; // the Abb Neagh Seal Breaker
		if (creature.Titles.IsUsable(10077)) return true; // the Tara Seal Breaker

		return false;
	}

	public virtual void OnBreak(Creature creature)
	{
	}

	public abstract void Setup();
	public abstract bool Check(Creature creature, Prop prop);
}
