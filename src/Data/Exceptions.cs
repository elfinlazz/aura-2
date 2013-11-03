// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Data
{
	public class DatabaseWarningException : Exception
	{
		public int Line { get; set; }

		public DatabaseWarningException(string source, int line, string msg, params object[] args)
			: base(string.Format(msg, args))
		{
			this.Source = source;
			this.Line = line;
		}

		public DatabaseWarningException(string msg, params object[] args)
			: base(string.Format(msg, args))
		{ }

		public override string ToString()
		{
			return string.Format("{0} on line {1}: {2}", this.Source, this.Line, this.Message);
		}
	}

	public class FieldCountException : DatabaseWarningException
	{
		public FieldCountException(int expectedAmount)
			: base("Expected at least {0} fields.", expectedAmount)
		{ }
	}
}
