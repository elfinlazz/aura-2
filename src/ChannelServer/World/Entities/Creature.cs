// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Text;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities.Creatures;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Mabi.Structs;
using Aura.Shared.Util;
using Aura.Channel.Scripting;
using Aura.Channel.World.Inventory;
using Aura.Channel.Skills.Life;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Base class for all "creaturly" entities.
	/// </summary>
	public abstract class Creature : Entity, IDisposable
	{
		public const float HandBalance = 0.3f;
		public const float HandCritical = 0.1f;
		public const int HandAttackMin = 0;
		public const int HandAttackMax = 8;
		public const float MaxKnockBack = 120;

		private const float MinWeight = 0.7f, MaxWeight = 1.5f;
		private const float MaxFoodStatBonus = 100;

		public override DataType DataType { get { return DataType.Creature; } }

		// General
		// ------------------------------------------------------------------

		public ChannelClient Client { get; set; }

		public string Name { get; set; }

		public CreatureStates State { get; set; }
		public CreatureStatesEx StateEx { get; set; }

		public int Race { get; set; }
		public RaceData RaceData { get; protected set; }

		public Creature Master { get; set; }
		public Creature Pet { get; set; }

		public CreatureTemp Temp { get; protected set; }
		public CreatureKeywords Keywords { get; protected set; }
		public CreatureTitles Titles { get; protected set; }
		public CreatureSkills Skills { get; protected set; }
		public CreatureRegen Regens { get; protected set; }
		public CreatureStatMods StatMods { get; protected set; }
		public CreatureConditions Conditions { get; protected set; }
		public CreatureQuests Quests { get; protected set; }
		public CreatureDrops Drops { get; protected set; }

		public ScriptVariables Vars { get; protected set; }

		public bool IsPlayer { get { return (this.IsCharacter || this.IsPet); } }
		public bool IsCharacter { get { return (this is Character); } }
		public bool IsPet { get { return (this is Pet); } }
		public bool IsPartner { get { return (this.IsPet && this.EntityId >= MabiId.Partners); } }

		public bool IsHuman { get { return (this.Race == 10001 || this.Race == 10002); } }
		public bool IsElf { get { return (this.Race == 9001 || this.Race == 9002); } }
		public bool IsGiant { get { return (this.Race == 8001 || this.Race == 8002); } }

		public bool IsMale { get { return (this.RaceData != null && this.RaceData.Gender == Gender.Male); } }
		public bool IsFemale { get { return (this.RaceData != null && this.RaceData.Gender == Gender.Female); } }

		public override int RegionId { get; set; }

		/// <summary>
		/// Returns whether creature is able to receive exp and level up.
		/// </summary>
		public virtual bool LevelingEnabled { get { return false; } }

		// Look
		// ------------------------------------------------------------------

		public byte SkinColor { get; set; }
		public short EyeType { get; set; }
		public byte EyeColor { get; set; }
		public byte MouthType { get; set; }

		private float _weight, _upper, _lower;
		public float Height { get; set; }
		public float Weight { get { return _weight; } set { _weight = Math2.Clamp(MinWeight, MaxWeight, value); } }
		public float Upper { get { return _upper; } set { _upper = Math2.Clamp(MinWeight, MaxWeight, value); } }
		public float Lower { get { return _lower; } set { _lower = Math2.Clamp(MinWeight, MaxWeight, value); } }

		public string StandStyle { get; set; }
		public string StandStyleTalking { get; set; }

		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }

		/// <summary>
		/// Returns body proportions
		/// </summary>
		public BodyProportions Body
		{
			get
			{
				BodyProportions result;
				result.Height = this.Height;
				result.Weight = this.Weight;
				result.Upper = this.Upper;
				result.Lower = this.Lower;
				return result;
			}
		}

		// Inventory
		// ------------------------------------------------------------------

		public CreatureInventory Inventory { get; protected set; }
		public Item RightHand { get { return this.Inventory.RightHand; } }
		public Item LeftHand { get { return this.Inventory.LeftHand; } }
		public Item Magazine { get { return this.Inventory.Magazine; } }

		// Movement
		// ------------------------------------------------------------------

		private Position _position, _destination;
		private DateTime _moveStartTime;
		private double _moveDuration, _movementX, _movementY;

		public byte Direction { get; set; }
		public bool IsMoving { get { return (_position != _destination); } }
		public bool IsWalking { get; protected set; }
		public double MoveDuration { get { return _moveDuration; } }

		/// <summary>
		/// Location if the creature before the warp.
		/// </summary>
		public Location LastLocation { get; set; }

		/// <summary>
		/// Location to fall back to, when saving in a temp region.
		/// </summary>
		public Location FallbackLocation { get; set; }

		/// <summary>
		/// True while character is warping somewhere.
		/// </summary>
		public bool Warping { get; set; }

		// Combat
		// ------------------------------------------------------------------

		protected bool _battleStance;
		/// <summary>
		/// Changes stance and broadcasts update.
		/// </summary>
		public bool IsInBattleStance { get { return _battleStance; } set { _battleStance = value; Send.ChangeStance(this); } }
		public Creature Target { get; set; }

		private int _stun;
		private DateTime _stunChange;
		/// <summary>
		/// Amount of ms before creature can do something again.
		/// </summary>
		/// <remarks>
		/// Max stun animation duration for monsters seems to be about 3s.
		/// </remarks>
		public int Stun
		{
			get
			{
				if (_stun <= 0)
					return 0;

				var diff = (DateTime.Now - _stunChange).TotalMilliseconds;
				if (diff < _stun)
					return (int)(_stun - diff);

				return (_stun = 0);
			}
			set
			{
				_stun = Math2.Clamp(0, short.MaxValue, value);
				_stunChange = DateTime.Now;
			}
		}

		private float _knockBack;
		private DateTime _knockBackChange;
		/// <summary>
		/// "Force" applied to the creature.
		/// </summary>
		/// <remarks>
		/// The more you hit a creature with heavy weapons,
		/// the higher this value. If it goes above 100 the creature
		/// is knocked back/down.
		/// This is also used for the knock down gauge.
		/// </remarks>
		public float KnockBack
		{
			get
			{
				if (_knockBack <= 0)
					return 0;

				var result = _knockBack - ((DateTime.Now - _knockBackChange).TotalMilliseconds / 60f);
				if (result <= 0)
					result = _knockBack = 0;

				return (float)result;
			}
			set
			{
				_knockBack = Math.Min(MaxKnockBack, value);
				_knockBackChange = DateTime.Now;
			}
		}

		public bool IsDead { get { return this.Has(CreatureStates.Dead); } }
		public bool IsStunned { get { return (this.Stun > 0); } }

		// Stats
		// ------------------------------------------------------------------

		public short Level { get; set; }
		public int LevelTotal { get; set; }
		public long Exp { get; set; }

		public short Age { get; set; }

		private short _ap;
		public short AbilityPoints { get { return _ap; } set { _ap = Math.Max((short)0, value); } }

		public virtual float CombatPower { get { return (this.RaceData != null ? this.RaceData.CombatPower : 1); } }

		public float StrBaseSkill { get; set; }
		public float DexBaseSkill { get; set; }
		public float IntBaseSkill { get; set; }
		public float WillBaseSkill { get; set; }
		public float LuckBaseSkill { get; set; }

		public float StrMod { get { return this.StatMods.Get(Stat.StrMod); } }
		public float DexMod { get { return this.StatMods.Get(Stat.DexMod); } }
		public float IntMod { get { return this.StatMods.Get(Stat.IntMod); } }
		public float WillMod { get { return this.StatMods.Get(Stat.WillMod); } }
		public float LuckMod { get { return this.StatMods.Get(Stat.LuckMod); } }

		public float StrBase { get; set; }
		public float DexBase { get; set; }
		public float IntBase { get; set; }
		public float WillBase { get; set; }
		public float LuckBase { get; set; }
		public float StrBaseTotal { get { return this.StrBase + this.StrBaseSkill; } }
		public float DexBaseTotal { get { return this.DexBase + this.DexBaseSkill; } }
		public float IntBaseTotal { get { return this.IntBase + this.IntBaseSkill; } }
		public float WillBaseTotal { get { return this.WillBase + this.WillBaseSkill; } }
		public float LuckBaseTotal { get { return this.LuckBase + this.LuckBaseSkill; } }
		public float Str { get { return this.StrBaseTotal + this.StrMod + this.StrFoodMod; } }
		public float Dex { get { return this.DexBaseTotal + this.DexMod + this.DexFoodMod; } }
		public float Int { get { return this.IntBaseTotal + this.IntMod + this.IntFoodMod; } }
		public float Will { get { return this.WillBaseTotal + this.WillMod + this.WillFoodMod; } }
		public float Luck { get { return this.LuckBaseTotal + this.LuckMod + this.LuckFoodMod; } }

		// TODO: Make equipment actually modify mods?

		public float BalanceBase
		{
			get
			{
				var result = HandBalance;

				if (this.RightHand != null)
				{
					result = this.RightHand.Balance;
					if (this.LeftHand != null)
					{
						result += this.LeftHand.Balance;
						result /= 2f; // average
					}
				}

				return result;
			}
		}

		public float CriticalBase
		{
			get
			{
				var result = HandCritical;

				if (this.RightHand != null)
				{
					result = this.RightHand.Critical;
					if (this.LeftHand != null)
					{
						result += this.LeftHand.Critical;
						result /= 2f; // average
					}
				}

				// TODO: Stat bonuses?

				return result;
			}
		}

		public int AttackMinBaseMod
		{
			get
			{
				var result = HandAttackMin;

				if (this.RightHand != null)
				{
					result = this.RightHand.OptionInfo.AttackMin;
					if (this.LeftHand != null)
					{
						result += this.LeftHand.OptionInfo.AttackMin;
						result /= 2; // average
					}
				}

				return result;
			}
		}

		public int AttackMaxBaseMod
		{
			get
			{
				var result = HandAttackMax;

				if (this.RightHand != null)
				{
					result = this.RightHand.OptionInfo.AttackMax;
					if (this.LeftHand != null)
					{
						result += this.LeftHand.OptionInfo.AttackMax;
						result /= 2; // average
					}
				}

				return result;
			}
		}

		public int WAttackMinBase
		{
			get
			{
				var result = 0;

				if (this.RightHand != null)
				{
					result = this.RightHand.OptionInfo.WAttackMin;
					if (this.LeftHand != null)
					{
						result += this.LeftHand.OptionInfo.WAttackMin;
						result /= 2; // average
					}
				}

				return result;
			}
		}

		public int WAttackMaxBase
		{
			get
			{
				var result = 0;

				if (this.RightHand != null)
				{
					result = this.RightHand.OptionInfo.WAttackMax;
					if (this.LeftHand != null)
					{
						result += this.LeftHand.OptionInfo.WAttackMax;
						result /= 2; // average
					}
				}

				return result;
			}
		}

		public int AttackMinMod { get { return (int)this.StatMods.Get(Stat.AttackMinMod); } }
		public int AttackMaxMod { get { return (int)this.StatMods.Get(Stat.AttackMaxMod); } }

		// Food Mods
		// ------------------------------------------------------------------

		private float _lifeFoodMod, _manaFoodMod, _staminaFoodMod;
		private float _strFoodMod, _intFoodMod, _dexFoodMod, _willFoodMod, _luckFoodMod;

		public float LifeFoodMod { get { return _lifeFoodMod; } set { _lifeFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float ManaFoodMod { get { return _manaFoodMod; } set { _manaFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float StaminaFoodMod { get { return _staminaFoodMod; } set { _staminaFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float StrFoodMod { get { return _strFoodMod; } set { _strFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float IntFoodMod { get { return _intFoodMod; } set { _intFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float DexFoodMod { get { return _dexFoodMod; } set { _dexFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float WillFoodMod { get { return _willFoodMod; } set { _willFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float LuckFoodMod { get { return _luckFoodMod; } set { _luckFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }

		// Defense/Protection
		// ------------------------------------------------------------------

		public int DefenseBase { get; set; } // Race
		public int DefenseBaseMod { get { return (int)this.StatMods.Get(Stat.DefenseBaseMod) + this.Inventory.GetEquipmentDefense(); } } // Skills, Titles, etc?
		public int DefenseMod { get { return (int)this.StatMods.Get(Stat.DefenseMod); } } // eg Reforging? (yellow)
		public int Defense
		{
			get
			{
				var result = this.DefenseBase + this.DefenseBaseMod + this.DefenseMod;

				// Str defense is displayed automatically on the client side
				result += (int)Math.Max(0, (this.Str - 10f) / 10f);

				return result;
			}
		}

		public float ProtectionBase { get; set; }
		public float ProtectionBaseMod { get { return this.StatMods.Get(Stat.ProtectionBaseMod) + this.Inventory.GetEquipmentProtection(); } }
		public float ProtectionMod { get { return this.StatMods.Get(Stat.ProtectionMod); } }
		public float Protection
		{
			get
			{
				var result = this.ProtectionBase + this.ProtectionBaseMod + this.ProtectionMod;

				return (result / 100f);
			}
		}

		public short ArmorPierce { get { return (short)Math.Max(0, ((this.Dex - 10) / 15)); } }

		// Life
		// ------------------------------------------------------------------

		private float _life, _injuries;
		public float Life
		{
			get { return _life; }
			set
			{
				_life = Math2.Clamp(-this.LifeMax, this.LifeInjured, value);

				//if (_life < 0 && !this.Has(CreatureConditionA.Deadly))
				//{
				//    this.Activate(CreatureConditionA.Deadly);
				//}
				//else if (_life >= 0 && this.Has(CreatureConditionA.Deadly))
				//{
				//    this.Deactivate(CreatureConditionA.Deadly);
				//}

			}
		}
		public float Injuries
		{
			get { return _injuries; }
			set { _injuries = Math2.Clamp(0, this.LifeMax, value); }
		}
		public float LifeMaxBase { get; set; }
		public float LifeMaxBaseSkill { get; set; }
		public float LifeMaxBaseTotal { get { return this.LifeMaxBase + this.LifeMaxBaseSkill; } }
		public float LifeMaxMod { get { return this.StatMods.Get(Stat.LifeMaxMod); } }
		public float LifeMax { get { return Math.Max(1, this.LifeMaxBaseTotal + this.LifeMaxMod + this.LifeFoodMod); } }
		public float LifeInjured { get { return this.LifeMax - _injuries; } }

		// Mana
		// ------------------------------------------------------------------

		private float _mana;
		public float Mana
		{
			get { return _mana; }
			set { _mana = Math2.Clamp(0, this.ManaMax, value); }
		}
		public float ManaMaxBase { get; set; }
		public float ManaMaxBaseSkill { get; set; }
		public float ManaMaxBaseTotal { get { return this.ManaMaxBase + this.ManaMaxBaseSkill; } }
		public float ManaMaxMod { get { return this.StatMods.Get(Stat.ManaMaxMod); } }
		public float ManaMax { get { return Math.Max(1, ManaMaxBaseTotal + this.ManaMaxMod + this.ManaFoodMod); } }

		// Stamina
		// ------------------------------------------------------------------

		private float _stamina, _hunger;
		public float Stamina
		{
			get { return _stamina; }
			set { _stamina = Math2.Clamp(0, this.StaminaMax, value); }
		}
		/// <summary>
		/// The amount of stamina that's not usable because of hunger.
		/// </summary>
		/// <remarks>
		/// While regen is limited to 50%, hunger can actually go higher.
		/// </remarks>
		public float Hunger
		{
			get { return _hunger; }
			set { _hunger = Math2.Clamp(0, this.StaminaMax, value); }
		}
		public float StaminaMaxBase { get; set; }
		public float StaminaMaxBaseSkill { get; set; }
		public float StaminaMaxBaseTotal { get { return this.StaminaMaxBase + this.StaminaMaxBaseSkill; } }
		public float StaminaMaxMod { get { return this.StatMods.Get(Stat.StaminaMaxMod); } }
		public float StaminaMax { get { return Math.Max(1, this.StaminaMaxBaseTotal + this.StaminaMaxMod + this.StaminaFoodMod); } }
		public float StaminaHunger { get { return this.StaminaMax - _hunger; } }

		/// <summary>
		/// Returns multiplicator to be used when regenerating stamina.
		/// </summary>
		public float StaminaRegenMultiplicator { get { return (this.Stamina < this.StaminaHunger ? 1f : 0.2f); } }

		// Events
		// ------------------------------------------------------------------

		/// <summary>
		/// Raised when creature dies.
		/// </summary>
		public event Action<Creature, Creature> Death;

		// ------------------------------------------------------------------

		protected Creature()
		{
			this.Client = new DummyClient();

			this.Temp = new CreatureTemp();
			this.Titles = new CreatureTitles(this);
			this.Keywords = new CreatureKeywords(this);
			this.Inventory = new CreatureInventory(this);
			this.Regens = new CreatureRegen(this);
			this.Skills = new CreatureSkills(this);
			this.StatMods = new CreatureStatMods(this);
			this.Conditions = new CreatureConditions(this);
			this.Quests = new CreatureQuests(this);
			this.Drops = new CreatureDrops(this);

			this.Vars = new ScriptVariables();
		}

		/// <summary>
		/// Loads race and handles some basic stuff, like adding regens.
		/// </summary>
		/// <param name="fullyFunctional">Fully functional creatures have an inv, regens, etc.</param>
		public virtual void LoadDefault(bool fullyFunctional = true)
		{
			if (this.Race == 0)
				throw new Exception("Set race before calling LoadDefault.");

			this.RaceData = AuraData.RaceDb.Find(this.Race);
			if (this.RaceData == null)
			{
				// Try to default to Human
				this.RaceData = AuraData.RaceDb.Find(10000);
				if (this.RaceData == null)
					throw new Exception("Unable to load race data, race '" + this.Race.ToString() + "' not found.");

				Log.Warning("Race '{0}' not found, using human instead.", this.Race);
			}

			if (fullyFunctional)
			{
				this.DefenseBase = this.RaceData.Defense;
				this.ProtectionBase = this.RaceData.Protection;

				this.Inventory.AddMainInventory();

				// The wiki says it's 0.125 life, but the packets have 0.12.
				this.Regens.Add(Stat.Life, 0.12f, this.LifeMax);
				this.Regens.Add(Stat.Mana, 0.05f, this.ManaMax);
				this.Regens.Add(Stat.Stamina, 0.4f, this.StaminaMax);
				if (ChannelServer.Instance.Conf.World.EnableHunger)
					this.Regens.Add(Stat.Hunger, 0.01f, this.StaminaMax);
				this.Regens.OnErinnDaytimeTick(ErinnTime.Now);

				ChannelServer.Instance.Events.MabiTick += this.OnMabiTick;
			}
		}

		/// <summary>
		/// Called when creature is removed from the server.
		/// (Killed NPC, disconnect, etc)
		/// </summary>
		public virtual void Dispose()
		{
			this.Regens.Dispose();
			ChannelServer.Instance.Events.MabiTick -= this.OnMabiTick;

			// Stop rest, so character doesn't appear sitting anymore
			// and chair props are removed.
			// Do this in dispose because we can't expect a clean logout.
			if (this.Has(CreatureStates.SitDown))
			{
				var restHandler = ChannelServer.Instance.SkillManager.GetHandler<RestSkillHandler>(SkillId.Rest);
				if (restHandler != null)
					restHandler.Stop(this, this.Skills.Get(SkillId.Rest));
			}
		}

		public void Activate(CreatureStates state) { this.State |= state; }
		public void Activate(CreatureStatesEx state) { this.StateEx |= state; }
		public void Deactivate(CreatureStates state) { this.State &= ~state; }
		public void Deactivate(CreatureStatesEx state) { this.StateEx &= ~state; }
		public bool Has(CreatureStates state) { return ((this.State & state) != 0); }
		public bool Is(RaceStands stand) { return ((this.RaceData.Stand & stand) != 0); }

		/// <summary>
		/// Returns current position.
		/// </summary>
		/// <returns></returns>
		public override Position GetPosition()
		{
			if (!this.IsMoving)
				return _position;

			var passed = (DateTime.Now - _moveStartTime).TotalSeconds;
			if (passed >= _moveDuration)
				return this.SetPosition(_destination.X, _destination.Y);

			var xt = _position.X + (_movementX * passed);
			var yt = _position.Y + (_movementY * passed);

			return new Position((int)xt, (int)yt);
		}

		/// <summary>
		/// Sets region, x, and y, to be near entity.
		/// Also randomizes direction.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="range"></param>
		public void SetLocationNear(Entity entity, int range)
		{
			var rnd = RandomProvider.Get();
			var pos = entity.GetPosition();
			var target = pos.GetRandomInRange(range, rnd);
			var dir = (byte)rnd.Next(255);

			this.SetLocation(entity.RegionId, target.X, target.Y);
			this.Direction = dir;
		}

		/// <summary>
		/// Returns current destination.
		/// </summary>
		/// <returns></returns>
		public Position GetDestination()
		{
			return _destination;
		}

		/// <summary>
		/// Sets position and destination.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Position SetPosition(int x, int y)
		{
			return _position = _destination = new Position(x, y);
		}

		/// <summary>
		/// Sets RegionId and position.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetLocation(int region, int x, int y)
		{
			this.RegionId = region;
			this.SetPosition(x, y);
		}

		/// <summary>
		/// Starts movement from current position to destination.
		/// Sends Running|Walking.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="walking"></param>
		public void Move(Position destination, bool walking)
		{
			_position = this.GetPosition();
			_destination = destination;
			_moveStartTime = DateTime.Now;
			this.IsWalking = walking;

			var diffX = _destination.X - _position.X;
			var diffY = _destination.Y - _position.Y;
			_moveDuration = Math.Sqrt(diffX * diffX + diffY * diffY) / this.GetSpeed();
			_movementX = diffX / _moveDuration;
			_movementY = diffY / _moveDuration;

			this.Direction = MabiMath.DirectionToByte(_movementX, _movementY);

			Send.Move(this, _position, _destination, walking);
		}

		/// <summary>
		/// Returns current movement speed (x/s).
		/// </summary>
		/// <returns></returns>
		public float GetSpeed()
		{
			var speed = (!this.IsWalking ? this.RaceData.RunningSpeed : this.RaceData.WalkingSpeed);

			// RaceSpeedFactor
			if (!this.IsWalking)
				speed *= this.RaceData.RunSpeedFactor;

			// Hurry condition
			var hurry = this.Conditions.GetExtraVal(169);
			speed *= 1 + (hurry / 100f);

			return speed;
		}

		/// <summary>
		/// Stops movement, returning new current position.
		/// Sends Force(Walk|Run)To.
		/// </summary>
		/// <returns></returns>
		public Position StopMove()
		{
			if (!this.IsMoving)
				return _position;

			var pos = this.GetPosition();
			this.SetPosition(pos.X, pos.Y);

			if (this.IsWalking)
				Send.ForceWalkTo(this, pos);
			else
				Send.ForceRunTo(this, pos);

			return pos;
		}

		/// <summary>
		/// Warps creature to target location,
		/// returns false if warp is unsuccessful.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public abstract bool Warp(int regionId, int x, int y);

		/// <summary>
		/// Called every 5 minutes, checks changes through food.
		/// </summary>
		/// <param name="time"></param>
		public void OnMabiTick(ErinnTime time)
		{
			var weight = this.Temp.WeightFoodChange;
			var upper = this.Temp.UpperFoodChange;
			var lower = this.Temp.LowerFoodChange;
			var life = this.Temp.LifeFoodChange;
			var mana = this.Temp.ManaFoodChange;
			var stm = this.Temp.StaminaFoodChange;
			var str = this.Temp.StrFoodChange;
			var int_ = this.Temp.IntFoodChange;
			var dex = this.Temp.DexFoodChange;
			var will = this.Temp.WillFoodChange;
			var luck = this.Temp.LuckFoodChange;
			var changes = false;

			var sb = new StringBuilder();

			if (ChannelServer.Instance.Conf.World.YouAreWhatYouEat)
			{
				if (weight != 0)
				{
					changes = true;
					this.Weight += weight;
					sb.Append(weight > 0 ? Localization.Get("You gained some weight.") : Localization.Get("You lost some weight.") + "\r\n");
				}

				if (upper != 0)
				{
					changes = true;
					this.Upper += upper;
					sb.Append(upper > 0 ? Localization.Get("Your upper body got bigger.") : Localization.Get("Your upper body got slimmer.") + "\r\n");
				}

				if (lower != 0)
				{
					changes = true;
					this.Lower += lower;
					sb.Append(lower > 0 ? Localization.Get("Your legs got bigger.") : Localization.Get("Your legs got slimmer.") + "\r\n");
				}
			}

			if (life != 0)
			{
				changes = true;
				this.LifeFoodMod += life;
			}

			if (mana != 0)
			{
				changes = true;
				this.ManaFoodMod += mana;
			}

			if (stm != 0)
			{
				changes = true;
				this.StaminaFoodMod += stm;
			}

			if (str != 0)
			{
				changes = true;
				this.StrFoodMod += str;
			}

			if (int_ != 0)
			{
				changes = true;
				this.IntFoodMod += int_;
			}

			if (dex != 0)
			{
				changes = true;
				this.DexFoodMod += dex;
			}

			if (will != 0)
			{
				changes = true;
				this.WillFoodMod += will;
			}

			if (luck != 0)
			{
				changes = true;
				this.LuckFoodMod += luck;
			}

			if (!changes)
				return;

			this.Temp.WeightFoodChange = 0;
			this.Temp.UpperFoodChange = 0;
			this.Temp.LowerFoodChange = 0;
			this.Temp.LifeFoodChange = 0;
			this.Temp.ManaFoodChange = 0;
			this.Temp.StaminaFoodChange = 0;
			this.Temp.StrFoodChange = 0;
			this.Temp.IntFoodChange = 0;
			this.Temp.DexFoodChange = 0;
			this.Temp.WillFoodChange = 0;
			this.Temp.LuckFoodChange = 0;

			Send.StatUpdate(this, StatUpdateType.Private, Stat.LifeMaxFoodMod, Stat.ManaMaxFoodMod, Stat.StaminaMaxFoodMod, Stat.StrFoodMod, Stat.IntFoodMod, Stat.DexFoodMod, Stat.WillFoodMod, Stat.LuckFoodMod);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.LifeMaxFoodMod);
			Send.CreatureBodyUpdate(this);

			if (sb.Length > 0)
				Send.Notice(this, sb.ToString());
		}

		/// <summary>
		/// Returns true if creature is able to attack this creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public virtual bool CanTarget(Creature creature)
		{
			if (this.IsDead || creature.IsDead)
				return false;

			return true;
		}

		/// <summary>
		/// Returns the max distance the creature can have to attack.
		/// </summary>
		/// <remarks>
		/// http://dev.mabinoger.com/forum/index.php/topic/767-attack-range/
		/// 
		/// This might not be the 100% accurate formula, but it should be
		/// good enough to work with for now.
		/// </remarks>
		/// <param name="target"></param>
		/// <returns></returns>
		public int AttackRangeFor(Creature target)
		{
			var attackerRange = this.RaceData.AttackRange;
			var targetRange = target.RaceData.AttackRange;

			var result = 156; // Default found in the client (for reference)

			if ((attackerRange < 300 && targetRange < 300) || (attackerRange >= 300 && attackerRange > targetRange))
				result = ((attackerRange + targetRange) / 2);
			else
				result = targetRange;

			// A little something extra
			result += 25;

			return result;
		}

		/// <summary>
		/// Calculates random damage using the given item.
		/// </summary>
		/// <param name="weapon">null for hands</param>
		/// <param name="balance">NaN for individual balance calculation</param>
		/// <returns></returns>
		public virtual float GetRndDamage(Item weapon, float balance = float.NaN)
		{
			float min = 0, max = 0;

			if (float.IsNaN(balance))
				balance = this.GetRndBalance(weapon);

			if (weapon != null)
			{
				min += weapon.OptionInfo.AttackMin;
				max += weapon.OptionInfo.AttackMax;
			}
			else
			{
				min = this.RaceData.AttackMin;
				max = this.RaceData.AttackMax;
			}

			min += (Math.Max(0, this.Str - 10) / 3.0f);
			max += (Math.Max(0, this.Str - 10) / 2.5f);

			min += this.AttackMinMod;
			max += this.AttackMaxMod;

			if (min > max)
				min = max;

			return (min + ((max - min) * balance));
		}

		/// <summary>
		/// Calculates the damage of left-and-right slots together
		/// </summary>
		/// <returns></returns>
		public float GetRndTotalDamage()
		{
			var balance = this.GetRndAverageBalance();

			var dmg = this.GetRndDamage(this.RightHand, balance);
			if (this.LeftHand != null)
				dmg += this.GetRndDamage(this.LeftHand, balance);

			return dmg;
		}

		/// <summary>
		/// Calculates random balance using the given base balance (eg 0.3 for hands).
		/// </summary>
		/// <param name="baseBalance"></param>
		/// <returns></returns>
		protected float GetRndBalance(float baseBalance)
		{
			var rnd = RandomProvider.Get();
			var balance = baseBalance;

			// Dex
			balance += (Math.Max(0, this.Dex - 10) / 4) / 100f;

			// Randomization, balance+-(100-balance), eg 80 = 60~100
			var diff = 1.0f - balance;
			balance += ((diff - (diff * 2 * (float)rnd.NextDouble())) * (float)rnd.NextDouble());
			balance = (float)Math.Max(0f, Math.Round(balance, 2));

			return balance;
		}

		/// <summary>
		/// Returns randomized average balance, taking both weapons into consideration.
		/// </summary>
		/// <returns></returns>
		public float GetRndAverageBalance()
		{
			return this.GetRndBalance(this.BalanceBase);
		}

		/// <summary>
		/// Calculates random balance for the given weapon.
		/// </summary>
		/// <param name="weapon">null for hands</param>
		/// <returns></returns>
		public float GetRndBalance(Item weapon)
		{
			return this.GetRndBalance(weapon != null ? weapon.Balance : HandBalance);
		}

		/// <summary>
		/// Applies damage to Life, kills creature if necessary.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="from"></param>
		public void TakeDamage(float damage, Creature from)
		{
			var lifeBefore = this.Life;

			this.Life -= damage;

			if (this.Life < 0 && !this.ShouldSurvive(damage, from, lifeBefore))
				this.Kill(from);
		}

		/// <summary>
		/// Returns true if creature should go into deadly by the attack.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="from"></param>
		/// <param name="lifeBefore"></param>
		/// <returns></returns>
		protected abstract bool ShouldSurvive(float damage, Creature from, float lifeBefore);

		/// <summary>
		/// Kills creature.
		/// </summary>
		/// <param name="killer"></param>
		public virtual void Kill(Creature killer)
		{
			if (this.Conditions.Has(ConditionsA.Deadly))
				this.Conditions.Deactivate(ConditionsA.Deadly);
			this.Activate(CreatureStates.Dead);

			//Send.SetFinisher(this, killer.EntityId);
			//Send.SetFinisher2(this);
			Send.IsNowDead(this);
			Send.SetFinisher(this, 0);

			ChannelServer.Instance.Events.OnCreatureKilled(this, killer);
			if (killer != null && killer.IsPlayer)
				ChannelServer.Instance.Events.OnCreatureKilledByPlayer(this, killer);
			this.Death.Raise(this, killer);

			if (this.Skills.ActiveSkill != null)
				this.Skills.CancelActiveSkill();

			var rnd = RandomProvider.Get();
			var pos = this.GetPosition();

			// Gold
			if (rnd.NextDouble() < ChannelServer.Instance.Conf.World.GoldDropChance)
			{
				// Random base amount
				var amount = rnd.Next(this.Drops.GoldMin, this.Drops.GoldMax + 1);

				if (amount > 0)
				{
					// Lucky Finish
					var luckyChance = rnd.NextDouble();
					if (luckyChance < ChannelServer.Instance.Conf.World.HugeLuckyFinishChance)
					{
						amount *= 100;

						if (amount >= 2000) killer.Titles.Enable(23); // the Lucky

						Send.CombatMessage(killer, Localization.Get("Huge Lucky Finish!!"));
						Send.Notice(killer, Localization.Get("Huge Lucky Finish!!"));
					}
					else if (luckyChance < ChannelServer.Instance.Conf.World.BigLuckyFinishChance)
					{
						amount *= 5;

						if (amount >= 2000) killer.Titles.Enable(23); // the Lucky

						Send.CombatMessage(killer, Localization.Get("Big Lucky Finish!!"));
						Send.Notice(killer, Localization.Get("Big Lucky Finish!!"));
					}
					else if (luckyChance < ChannelServer.Instance.Conf.World.LuckyFinishChance)
					{
						amount *= 2;

						if (amount >= 2000) killer.Titles.Enable(23); // the Lucky

						Send.CombatMessage(killer, Localization.Get("Lucky Finish!!"));
						Send.Notice(killer, Localization.Get("Lucky Finish!!"));
					}

					// Drop rate muliplicator
					amount = Math.Min(21000, (int)(amount * ChannelServer.Instance.Conf.World.GoldDropRate));

					// Drop stack for stack
					var i = 0;
					var pattern = (amount == 21000);
					do
					{
						Position dropPos;
						if (!pattern)
						{
							dropPos = pos.GetRandomInRange(50, rnd);
						}
						else
						{
							dropPos = new Position(pos.X + CreatureDrops.MaxGoldPattern[i, 0], pos.Y + CreatureDrops.MaxGoldPattern[i, 1]);
							i++;
						}

						var gold = new Item(2000);
						gold.Info.Amount = (ushort)Math.Min(1000, amount);
						gold.Info.Region = this.RegionId;
						gold.Info.X = dropPos.X;
						gold.Info.Y = dropPos.Y;
						gold.DisappearTime = DateTime.Now.AddSeconds(60);

						this.Region.AddItem(gold);

						amount -= gold.Info.Amount;
					}
					while (amount > 0);
				}
			}

			// Drops
			foreach (var drop in this.Drops.Drops)
			{
				if (rnd.NextDouble() < drop.Chance * ChannelServer.Instance.Conf.World.DropRate)
				{
					var dropPos = pos.GetRandomInRange(50, rnd);

					var item = new Item(drop.ItemId);
					item.Info.Amount = 1;
					item.Info.Region = this.RegionId;
					item.Info.X = dropPos.X;
					item.Info.Y = dropPos.Y;
					item.DisappearTime = DateTime.Now.AddSeconds(60);

					this.Region.AddItem(item);
				}
			}
		}

		/// <summary>
		/// Increases exp and levels up creature if appropriate.
		/// </summary>
		/// <param name="val"></param>
		public void GiveExp(long val)
		{
			if (!this.LevelingEnabled) return;

			this.Exp += val;

			var levelStats = AuraData.StatsLevelUpDb.Find(this.Race, this.Age);

			var prevLevel = this.Level;
			float ap = this.AbilityPoints;
			float life = this.LifeMaxBase;
			float mana = this.ManaMaxBase;
			float stamina = this.StaminaMaxBase;
			float str = this.StrBase;
			float int_ = this.IntBase;
			float dex = this.DexBase;
			float will = this.WillBase;
			float luck = this.LuckBase;

			while (this.Level < AuraData.ExpDb.MaxLevel && this.Exp >= AuraData.ExpDb.GetTotalForNextLevel(this.Level))
			{
				this.Level++;

				if (levelStats == null)
					continue;

				this.AbilityPoints += levelStats.AP;
				this.LifeMaxBase += levelStats.Life;
				this.ManaMaxBase += levelStats.Mana;
				this.StaminaMaxBase += levelStats.Stamina;
				this.StrBase += levelStats.Str;
				this.IntBase += levelStats.Int;
				this.DexBase += levelStats.Dex;
				this.WillBase += levelStats.Will;
				this.LuckBase += levelStats.Luck;
			}

			if (prevLevel < this.Level)
			{
				// Only notify on level up
				if (levelStats == null)
					Log.Unimplemented("GiveExp: Level up stats missing for race '{0}'.", this.Race);

				this.FullHeal();

				Send.StatUpdateDefault(this);
				Send.LevelUp(this);

				// Only send aquire if stat crosses the X.0 border.
				// Eg, 50.9 -> 50.1
				float diff = 0;
				if ((diff = (this.AbilityPoints - (int)ap)) >= 1) Send.SimpleAcquireInfo(this, "ap", diff);
				if ((diff = (this.LifeMaxBase - (int)life)) >= 1) Send.SimpleAcquireInfo(this, "life", diff);
				if ((diff = (this.ManaMaxBase - (int)mana)) >= 1) Send.SimpleAcquireInfo(this, "mana", diff);
				if ((diff = (this.StaminaMaxBase - (int)stamina)) >= 1) Send.SimpleAcquireInfo(this, "stamina", diff);
				if ((diff = (this.StrBase - (int)str)) >= 1) Send.SimpleAcquireInfo(this, "str", diff);
				if ((diff = (this.IntBase - (int)int_)) >= 1) Send.SimpleAcquireInfo(this, "int", diff);
				if ((diff = (this.DexBase - (int)dex)) >= 1) Send.SimpleAcquireInfo(this, "dex", diff);
				if ((diff = (this.WillBase - (int)will)) >= 1) Send.SimpleAcquireInfo(this, "will", diff);
				if ((diff = (this.LuckBase - (int)luck)) >= 1) Send.SimpleAcquireInfo(this, "luck", diff);

				//EventManager.CreatureEvents.OnCreatureLevelsUp(this);
			}
			else
				Send.StatUpdate(this, StatUpdateType.Private, Stat.Experience);
		}

		/// <summary>
		/// Heals all life, mana, stamina, hunger, and wounds and updates client.
		/// </summary>
		public void FullHeal()
		{
			this.Injuries = 0;
			this.Hunger = 0;
			this.Life = this.LifeMax;
			this.Mana = this.ManaMax;
			this.Stamina = this.StaminaMax;

			Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.Stamina, Stat.Hunger, Stat.Mana);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
		}

		/// <summary>
		/// Fully heals life and updates client.
		/// </summary>
		public void FullLifeHeal()
		{
			this.Injuries = 0;
			this.Life = this.LifeMax;

			Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
		}

		/// <summary>
		/// Increases AP and updates client.
		/// </summary>
		/// <param name="amount"></param>
		public void GiveAp(int amount)
		{
			this.AbilityPoints += (short)Math2.Clamp(short.MinValue, short.MaxValue, amount);
			Send.StatUpdate(this, StatUpdateType.Private, Stat.AbilityPoints);
		}

		/// <summary>
		/// Revives creature
		/// </summary>
		public void Revive()
		{
			if (!this.IsDead)
				return;

			if (this.Life <= 1)
				this.Life = 1;

			this.Deactivate(CreatureStates.Dead);

			Send.RemoveDeathScreen(this);
			Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod);
			Send.RiseFromTheDead(this);
			//Send.DeadFeather(creature);
			Send.Revived(this);
		}

		/// <summary>
		/// Returns the power rating (Weak, Boss, etc) of
		/// compareCreature towards creature.
		/// </summary>
		/// <param name="compareCreature">Creature to compare to</param>
		/// <returns></returns>
		public PowerRating GetPowerRating(Creature compareCreature)
		{
			var cp = this.CombatPower;
			var otherCp = compareCreature.CombatPower;

			if (otherCp < cp * 0.8f) return PowerRating.Weakest;
			if (otherCp < cp * 1.0f) return PowerRating.Weak;
			if (otherCp < cp * 1.4f) return PowerRating.Normal;
			if (otherCp < cp * 2.0f) return PowerRating.Strong;
			if (otherCp < cp * 3.0f) return PowerRating.Awful;
			return PowerRating.Boss;
		}

		/// <summary>
		/// Returns CriticalBase - target protection, the base
		/// critical hit chance.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public float GetCritChanceFor(Creature target)
		{
			return (this.CriticalBase - target.Protection);
		}
	}
}
