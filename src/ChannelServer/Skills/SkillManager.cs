// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Const;
using Aura.Channel.Skills.Base;
using System.Reflection;
using Aura.Shared.Util;

namespace Aura.Channel.Skills
{
	public class SkillManager
	{
		private Dictionary<SkillId, ISkillHandler> _handlers;

		public SkillManager()
		{
			_handlers = new Dictionary<SkillId, ISkillHandler>();
		}

		/// <summary>
		/// Loads all classes with skill attributes as handlers.
		/// </summary>
		public void AutoLoad()
		{
			Log.Info("Loading skill handlers...");

			lock (_handlers)
			{
				_handlers.Clear();

				// Search through all loaded types to find skill attributes
				foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
				{
					var attributes = type.GetCustomAttributes(typeof(SkillAttribute), false);
					if (attributes == null || attributes.Length == 0)
						continue;

					var handler = Activator.CreateInstance(type) as ISkillHandler;
					foreach (var skillId in (attributes.First() as SkillAttribute).Ids)
					{
						if (_handlers.ContainsKey(skillId))
							Log.Warning("SkillManager: Duplicate handler for '{0}', using '{1}'.", skillId, type.Name);

						var initHandler = handler as IInitiableSkillHandler;
						if (initHandler != null) initHandler.Init();

						_handlers[skillId] = handler;
					}
				}
			}

			Log.Info("Done loading {0} skill handlers.", _handlers.Count);
		}

		/// <summary>
		/// Returns skill handler for id, or null.
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		public ISkillHandler GetHandler(SkillId skillId)
		{
			ISkillHandler result;
			lock (_handlers)
				_handlers.TryGetValue(skillId, out result);
			return result;
		}

		/// <summary>
		/// Returns skill handler for id, or null.
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		public T GetHandler<T>(SkillId skillId) where T : class, ISkillHandler
		{
			ISkillHandler result;
			lock (_handlers)
				_handlers.TryGetValue(skillId, out result);
			return (result as T);
		}
	}

	public class SkillAttribute : Attribute
	{
		public SkillId[] Ids { get; protected set; }

		public SkillAttribute(params SkillId[] skillIds)
		{
			this.Ids = skillIds;
		}
	}
}
