// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Channel.Network;
using Aura.Channel.World.Entities.Creatures;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Mabi;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Base class for all "creaturly" entities.
	/// </summary>
	public abstract class Creature : Entity
	{
		public override DataType DataType { get { return DataType.Creature; } }

		// General
		// ------------------------------------------------------------------

		public ChannelClient Client { get; set; }

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

		public virtual float CombatPower { get { return (this.RaceInfo != null ? this.RaceInfo.CombatPower : 1); } }

		public float ProtectionMod { get; set; }
		public float ProtectionPassive { get; set; }
		public short DefenseMod { get; set; }
		public short DefensePassive { get; set; }

		public float StrBaseSkill { get; set; }
		public float DexBaseSkill { get; set; }
		public float IntBaseSkill { get; set; }
		public float WillBaseSkill { get; set; }
		public float LuckBaseSkill { get; set; }

		public float StrMod { get; set; }
		public float DexMod { get; set; }
		public float IntMod { get; set; }
		public float WillMod { get; set; }
		public float LuckMod { get; set; }

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
		public float Str { get { return this.StrBaseTotal + this.StrMod; } }
		public float Dex { get { return this.DexBaseTotal + this.DexMod; } }
		public float Int { get { return this.IntBaseTotal + this.IntMod; } }
		public float Will { get { return this.WillBaseTotal + this.WillMod; } }
		public float Luck { get { return this.LuckBaseTotal + this.LuckMod; } }

		// Life
		// ------------------------------------------------------------------

		public float LifeMaxBaseSkill { get; set; }
		public float ManaMaxBaseSkill { get; set; }
		public float StaminaMaxBaseSkill { get; set; }

		public float LifeMaxMod { get; set; }
		public float ManaMaxMod { get; set; }
		public float StaminaMaxMod { get; set; }

		private float _life, _injuries;
		public float Life
		{
			get { return _life; }
			set
			{
				if (value > this.LifeInjured)
					_life = this.LifeInjured;
				else if (value < -this.LifeMax)
					_life = -this.LifeMax;
				else
					_life = value;

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
			set { _injuries = Math.Max(0, Math.Min(this.LifeMax, value)); }
		}
		public float LifeMaxBase { get; set; }
		public float LifeMaxBaseTotal { get { return this.LifeMaxBase + this.LifeMaxBaseSkill; } }
		public float LifeMax { get { return this.LifeMaxBaseTotal + this.LifeMaxMod; } }
		public float LifeInjured { get { return this.LifeMax - _injuries; } }

		// Mana
		// ------------------------------------------------------------------

		private float _mana;
		public float Mana
		{
			get { return _mana; }
			set { _mana = Math.Max(0, Math.Min(this.ManaMax, value)); }
		}

		public float ManaMaxBase { get; set; }
		public float ManaMaxBaseTotal { get { return this.ManaMaxBase + this.ManaMaxBaseSkill; } }
		public float ManaMax { get { return ManaMaxBaseTotal + this.ManaMaxMod; } }

		// Stamina
		// ------------------------------------------------------------------

		private float _stamina, _hunger;
		public float Stamina
		{
			get { return _stamina; }
			set { _stamina = Math.Max(0, Math.Min(this.StaminaHunger, value)); }
		}
		public float Hunger
		{
			get { return _hunger; }
			set { _hunger = Math.Max(0, Math.Min(this.StaminaMax / 2, value)); }
		}
		public float StaminaMaxBase { get; set; }
		public float StaminaMaxBaseTotal { get { return this.StaminaMaxBase + this.StaminaMaxBaseSkill; } }
		public float StaminaMax { get { return this.StaminaMaxBaseTotal + this.StaminaMaxMod; } }
		public float StaminaHunger { get { return this.StaminaMax - _hunger; } }

		// ------------------------------------------------------------------

		public Creature()
		{
			this.Client = new DummyClient();

			this.Temp = new CreatureTemp();
			this.Titles = new Dictionary<ushort, bool>();
			this.Keywords = new List<short>();

			this.RaceInfo = AuraData.RaceDb.Find(10002);

			this.Inventory = new CreatureInventory(this);
			this.Inventory.AddMainInventory();
		}

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
		/// Returns current movement speed.
		/// </summary>
		/// <returns></returns>
		public float GetSpeed()
		{
			return (!this.IsWalking ? this.RaceInfo.RunningSpeed : this.RaceInfo.WalkingSpeed);
		}

		/// <summary>
		/// Warps creature to target location.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public abstract void Warp(int regionId, int x, int y);
	}
}
