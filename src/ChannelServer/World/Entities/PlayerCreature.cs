// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Const;
using Aura.Data.Database;
using Aura.Data;
using Aura.Channel.World.Entities.Creatures;

namespace Aura.Channel.World.Entities
{
	public abstract class Entity
	{
		public long EntityId { get; set; }
		public int Region { get; set; }

		public abstract EntityType EntityType { get; }

		public abstract Position GetPosition();
	}

	public enum EntityType { Undefined, Character, Pet, Item, NPC, Prop }

	public abstract class Creature : Entity
	{
		// General
		// ------------------------------------------------------------------

		public string Name { get; set; }

		public CreatureStates State { get; set; }
		public CreatureStatesEx StateEx { get; set; }

		public CreatureCondition Conditions { get; set; }

		public int Race { get; set; }
		public RaceInfo RaceInfo { get; protected set; }

		public Creature Owner { get; set; }

		public CreatureTemp Temp { get; protected set; }

		// Look
		// ------------------------------------------------------------------

		public byte SkinColor { get; set; }
		public byte EyeType { get; set; }
		public byte EyeColor { get; set; }
		public byte MouthType { get; set; }

		public float Height { get; set; }
		public float Weight { get; set; }
		public float Upper { get; set; }
		public float Lower { get; set; }

		public string StandStyle { get; set; }
		public string StandStyleTalking { get; set; }

		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }

		// Movement
		// ------------------------------------------------------------------

		public byte Direction { get; set; }
		public bool IsMoving { get { return false; } }
		public bool IsWalking { get; protected set; }
		public Position Destination { get; protected set; }

		// Misc
		// ------------------------------------------------------------------

		public int BattleState { get; set; }
		public byte WeaponSet { get; set; }

		public List<short> Keywords { get; protected set; }

		// Title
		// ------------------------------------------------------------------

		public short Title { get; set; }
		public DateTime TitleApplied { get; set; }

		public Dictionary<ushort, bool> Titles { get; protected set; }

		public short OptionTitle { get; set; }

		// Stats
		// ------------------------------------------------------------------

		public short Level { get; set; }
		public int LevelTotal { get; set; }
		public long Exp { get; set; }

		public short Age { get; set; }
		public short AbilityPoints { get; set; }

		public float CombatPower { get; set; }

		public float Life { get; set; }
		public float LifeInjured { get; set; }
		public float LifeMaxBaseTotal { get; set; }
		public float LifeMaxMod { get; set; }

		public float Mana { get; set; }
		public float ManaMaxBaseTotal { get; set; }
		public float ManaMaxMod { get; set; }

		public float Stamina { get; set; }
		public float StaminaMaxBaseTotal { get; set; }
		public float StaminaMaxMod { get; set; }
		public float StaminaHunger { get; set; }

		public float StrBaseTotal { get; set; }
		public float StrMod { get; set; }
		public float DexBaseTotal { get; set; }
		public float DexMod { get; set; }
		public float IntBaseTotal { get; set; }
		public float IntMod { get; set; }
		public float WillBaseTotal { get; set; }
		public float WillMod { get; set; }
		public float LuckBaseTotal { get; set; }
		public float LuckMod { get; set; }

		public float ProtectionMod { get; set; }
		public float ProtectionPassive { get; set; }
		public short DefenseMod { get; set; }
		public short DefensePassive { get; set; }

		// ------------------------------------------------------------------

		public Creature()
		{
			this.Temp = new CreatureTemp();
			this.Titles = new Dictionary<ushort, bool>();
			this.Keywords = new List<short>();

			this.RaceInfo = AuraData.RaceDb.Find(10002);
		}

		private Position _position;

		public override Position GetPosition()
		{
			return _position;
		}

		public Position SetPosition(int x, int y)
		{
			return _position = new Position(x, y);
		}
	}

	public class PlayerCreature : Creature
	{
		public override EntityType EntityType { get { return EntityType.Character; } }

		public long CreatureId { get; set; }
		public string Server { get; set; }
		public DateTime DeletionTime { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime LastRebirth { get; set; }
	}

	public class Character : PlayerCreature
	{
	}

	public class Pet : PlayerCreature
	{
	}

	public struct Position
	{
		public int X;
		public int Y;

		public Position(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		public Position(Position pos)
		{
			this.X = pos.X;
			this.Y = pos.Y;
		}
	}
}
