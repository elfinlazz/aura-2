// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Channel.Network.Sending;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World
{
	public class Cutscene
	{
		public Action<Cutscene> _callback;

		/// <summary>
		/// Name of the cutscene file.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// Creature that created the cutscene.
		/// </summary>
		public Creature Leader { get; protected set; }

		/// <summary>
		/// Actors of the cutscene.
		/// </summary>
		public Dictionary<string, Creature> Actors { get; protected set; }

		public Cutscene(string name, Creature leader)
		{
			this.Name = name;
			this.Leader = leader;

			this.Actors = new Dictionary<string, Creature>();
		}

		/// <summary>
		/// Adds creature as actor.
		/// </summary>
		/// <remarks>
		/// Officials apparently create copies of the creatures, getting rid
		/// of the name (replaced by the actor name), stand styles, etc.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="creature"></param>
		public void AddActor(string name, Creature creature)
		{
			if (creature == null)
			{
				creature = new NPC();
				creature.Name = name;
			}

			this.Actors[name] = creature;
		}

		/// <summary>
		/// Adds new creature of race as actor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="raceId"></param>
		public void AddActor(string name, int raceId)
		{
			var creature = new NPC();
			creature.Race = raceId;
			creature.LoadDefault();
			creature.Name = creature.RaceData.Name;
			creature.Color1 = creature.RaceData.Color1;
			creature.Color2 = creature.RaceData.Color2;
			creature.Color3 = creature.RaceData.Color3;
			creature.Height = creature.RaceData.Size;

			this.Actors[name] = creature;
		}

		/// <summary>
		/// Plays cutscene for everybody.
		/// </summary>
		public void Play()
		{
			this.Leader.Temp.CurrentCutscene = this;

			// TODO: All viewers
			Send.CharacterLock(this.Leader, Locks.Default);
			Send.PlayCutscene(this.Leader, this);
		}

		/// <summary>
		/// Plays cutscene for everybody.
		/// </summary>
		public void Play(Action<Cutscene> onFinish)
		{
			this.Play();
			_callback = onFinish;
		}

		/// <summary>
		/// Ends cutscene for everybody.
		/// </summary>
		public void Finish()
		{
			Send.CutsceneEnd(this);
			Send.CharacterUnlock(this.Leader, Locks.Default);
			Send.CutsceneUnk(this);

			this.Leader.Temp.CurrentCutscene = null;

			if (_callback != null)
				_callback(this);
		}
	}
}
