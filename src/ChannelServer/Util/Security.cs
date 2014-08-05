// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aura.Channel.Database;
using Aura.Channel.Network;
using Aura.Shared.Util;
using MySql.Data.MySqlClient.Memcached;

namespace Aura.Channel.Util
{
	/// <summary>
	/// The base for the exceptions to throw when someone does something bad
	/// </summary>
	public abstract class SecurityViolationException : Exception
	{
		private const int _stackDepth = 5;

		private readonly string _message;

		/// <summary>
		/// Details on what happened
		/// </summary>
		public override string Message { get { return _message; } }

		/// <summary>
		/// A short stack trace to identify where the incident occured.
		/// </summary>
		public string StackReport { get; private set; }
		
		/// <summary>
		/// How bad it was
		/// </summary>
		public IncidentSeverityLevel Level { get; private set; }

		protected SecurityViolationException(IncidentSeverityLevel lvl, string report)
		{
			this.Level = lvl;

			var stacktrace = new System.Diagnostics.StackTrace(2); // Skip 2 frames for this and calling ctor

			this.StackReport = string.Join(" --> ",
				stacktrace.GetFrames()
				.Take(_stackDepth)
				.Reverse()
				.Select(n => n.GetMethod().DeclaringType.Name + "." + n.GetMethod().Name));

			_message = string.Format("{0}: {1}", stacktrace.GetFrame(0).GetMethod().Name, report);
		}
	}

	/// <summary>
	/// Used to indicate something suspicious happened, but it
	/// *could* be nothing. This setting should be rarely used...
	/// </summary>
	public sealed class MildViolation : SecurityViolationException
	{
		public MildViolation(string report, params object[] args)
			: base(IncidentSeverityLevel.Mild, string.Format(report, args))
		{
		}
	}

	/// <summary>
	/// Something that is a strong indicator for a hack, but not certain.
	/// </summary>
	public sealed class ModerateViolation : SecurityViolationException
	{
		public ModerateViolation(string report, params object[] args)
			: base(IncidentSeverityLevel.Moderate, string.Format(report, args))
		{
		}
	}

	/// <summary>
	/// Something happened that could really only be caused by a hack tool
	/// </summary>
	public sealed class SevereViolation : SecurityViolationException
	{
		public SevereViolation(string report, params object[] args)
			: base(IncidentSeverityLevel.Severe, string.Format(report, args))
		{
		}
	}

	public enum IncidentSeverityLevel
	{
		Mild = 1,
		Moderate,
		Severe
	}

	public enum AutobanLengthIncrease
	{
		None,
		Linear,
		Exponential
	}

	public sealed class Autoban
	{
		private readonly ChannelClient _client;

		public int Score
		{
			get
			{
				return _client.Account.AutobanScore;
			}

			private set
			{
				_client.Account.AutobanScore = value;
			}
		}
	
		public int BanCount
		{
			get
			{
				return _client.Account.AutobanCount;
			}

			private set
			{
				_client.Account.AutobanCount = value;
			}
		}

		public DateTime LastAutobanReduction
		{
			get
			{
				return _client.Account.LastAutobanReduction;
			}
			private set
			{
				_client.Account.LastAutobanReduction = value;
			}
		}

		public Autoban(ChannelClient client)
		{
			_client = client;
		}

		public void Incident(IncidentSeverityLevel level, string report, string stacktrace = null)
		{
			switch (level)
			{
				case IncidentSeverityLevel.Mild: this.Score += ChannelServer.Instance.Conf.Autoban.MildAmount; break;
				case IncidentSeverityLevel.Moderate: this.Score += ChannelServer.Instance.Conf.Autoban.ModerateAmount; break;
				case IncidentSeverityLevel.Severe: this.Score += ChannelServer.Instance.Conf.Autoban.SevereAmount; break;
				default:
					Log.Warning("Unknown severity level {0}", level);
					goto case IncidentSeverityLevel.Mild;
			}

			Log.Warning(
				"Account '{0}' (Controlling {1}) just committed a {2} offense. Total ban score: {3}. Incident report: {4}",
				_client.Account.Id, _client.Controlling == null ? "NULL" : "'" + _client.Controlling.Name + "'", level, this.Score,
				report);

			ChannelDb.Instance.LogSecurityIncident(_client, level, report, stacktrace);

			if (!ChannelServer.Instance.Conf.Autoban.Enabled)
				return;

			this.LastAutobanReduction = DateTime.Now;

			if (this.Score >= ChannelServer.Instance.Conf.Autoban.BanAt)
				this.Ban();

			_client.Kill();
		}

		private void Ban()
		{
			this.BanCount++;

			TimeSpan banLength;

			switch (ChannelServer.Instance.Conf.Autoban.LengthIncrease)
			{
				case AutobanLengthIncrease.None:
					banLength = ChannelServer.Instance.Conf.Autoban.InitialBanTime;
					break;

				case AutobanLengthIncrease.Linear:
					banLength = TimeSpan.FromMinutes((long)(ChannelServer.Instance.Conf.Autoban.InitialBanTime.TotalMinutes * this.BanCount));
					break;

				case AutobanLengthIncrease.Exponential:
					banLength = TimeSpan.FromMinutes(
						 ChannelServer.Instance.Conf.Autoban.InitialBanTime.TotalMinutes * (long)Math.Pow(this.BanCount, 2));
					break;

				default:
					Log.Warning("Unknown AutobanLengthIncrease: {0}", ChannelServer.Instance.Conf.Autoban.LengthIncrease);
					goto case AutobanLengthIncrease.Exponential;
			}

			Log.Info("Autobanning account '{0}'. Total times they've been autobanned: {1}. Length of this ban: {2}",
				_client.Account.Id, this.BanCount, banLength);

			_client.Account.BanExpiration = DateTime.Now + banLength;

			_client.Account.BanReason = "Automatic ban triggered.";

			// So their score doesn't decrease while they're banned.
			this.LastAutobanReduction = _client.Account.BanExpiration;

			if (ChannelServer.Instance.Conf.Autoban.ResetScoreOnBan)
				this.Score = 0;
		}
	}
}
