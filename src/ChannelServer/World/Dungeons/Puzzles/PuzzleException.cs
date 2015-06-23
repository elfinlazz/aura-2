// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Runtime.Serialization;

namespace Aura.Channel.World.Dungeons.Puzzles
{
	[Serializable]
	public class PuzzleException : Exception
	{
		public PuzzleException()
		{
		}

		public PuzzleException(string message)
			: base(message)
		{
		}

		public PuzzleException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected PuzzleException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
