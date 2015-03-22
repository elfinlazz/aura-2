// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Mabi.Const
{
	public enum LoginType
	{
		/// <summary>
		/// Only seen in KR
		/// </summary>
		KR = 0,

		/// <summary>
		/// Used to request disconnect when you're already logged in.
		/// </summary>
		RequestDisconnect = 1,

		/// <summary>
		/// Coming from channel (session key)
		/// </summary>
		FromChannel = 2,

		/// <summary>
		/// NX auth hash
		/// </summary>
		NewHash = 5,

		/// <summary>
		/// Default, hashed password
		/// </summary>
		Normal = 12,

		/// <summary>
		/// ? o.o
		/// </summary>
		CmdLogin = 16,

		/// <summary>
		/// Last seen in EU (no hashed password)
		/// </summary>
		EU = 18,

		/// <summary>
		/// Password + Secondary password
		/// </summary>
		SecondaryPassword = 20,

		/// <summary>
		/// RSA password, used by CH
		/// </summary>
		CH = 23,
	}

	public enum LoginResult
	{
		Fail = 0,
		Success = 1,
		Empty = 2,
		IdOrPassIncorrect = 3,
		/* IdOrPassIncorrect = 4, */
		TooManyConnections = 6,
		AlreadyLoggedIn = 7,
		UnderAge = 33,
		Message = 51,
		SecondaryReq = 90,
		SecondaryFail = 91,
		Banned = 101,
	}

	public enum PetCreationOptionsListType : byte
	{
		BlackList = 0, WhiteList = 1
	}

	public enum DeletionFlag { Normal, Recover, Ready, Delete }
}
