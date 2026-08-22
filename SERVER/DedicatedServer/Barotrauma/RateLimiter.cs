using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200004E RID: 78
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class RateLimiter
	{
		// Token: 0x06000BFC RID: 3068 RVA: 0x00071B54 File Offset: 0x0006FD54
		public RateLimiter(int maxRequests, int expiryInSeconds, [TupleElementNames(new string[]
		{
			"Action",
			"Punishment"
		})] [Nullable(new byte[]
		{
			1,
			0
		})] params ValueTuple<RateLimitAction, RateLimitPunishment>[] punishmentRules)
		{
			this.maxRequests = maxRequests;
			this.expiryInSeconds = expiryInSeconds;
			this.punishments = punishmentRules.ToImmutableDictionary(([TupleElementNames(new string[]
			{
				"Action",
				"Punishment"
			})] ValueTuple<RateLimitAction, RateLimitPunishment> pair) => pair.Item1, ([TupleElementNames(new string[]
			{
				"Action",
				"Punishment"
			})] ValueTuple<RateLimitAction, RateLimitPunishment> pair) => pair.Item2);
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x00071BE0 File Offset: 0x0006FDE0
		public bool IsLimitReached(Client client)
		{
			if (RateLimiter.IsExempt(client))
			{
				return false;
			}
			this.expiredRateLimits.Clear();
			foreach (KeyValuePair<Client, RateLimiter.RateLimit> keyValuePair in this.rateLimits)
			{
				Client client2;
				RateLimiter.RateLimit rateLimit2;
				keyValuePair.Deconstruct(out client2, out rateLimit2);
				Client c = client2;
				RateLimiter.RateLimit limit = rateLimit2;
				if (limit.Expiry < DateTimeOffset.Now)
				{
					this.expiredRateLimits.Add(c);
				}
			}
			foreach (Client c2 in this.expiredRateLimits)
			{
				this.rateLimits.Remove(c2);
			}
			RateLimiter.RateLimit rateLimit;
			if (!this.rateLimits.TryGetValue(client, out rateLimit))
			{
				rateLimit = new RateLimiter.RateLimit(DateTimeOffset.Now.AddSeconds((double)this.expiryInSeconds));
				this.rateLimits.Add(client, rateLimit);
			}
			rateLimit.RequestAmount++;
			if (rateLimit.RequestAmount > this.maxRequests)
			{
				this.ProcessPunishment(client, rateLimit.RequestAmount);
				return true;
			}
			return false;
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x00071D24 File Offset: 0x0006FF24
		private void ProcessPunishment(Client client, int requests)
		{
			GameServer server = GameMain.Server;
			bool flag;
			if (server != null)
			{
				ServerSettings serverSettings = server.ServerSettings;
				if (serverSettings != null)
				{
					flag = serverSettings.EnableDoSProtection;
					goto IL_1C;
				}
			}
			flag = false;
			IL_1C:
			bool isDosProtectionEnabled = flag;
			foreach (KeyValuePair<RateLimitAction, RateLimitPunishment> keyValuePair in this.punishments)
			{
				RateLimitAction rateLimitAction;
				RateLimitPunishment rateLimitPunishment;
				keyValuePair.Deconstruct(out rateLimitAction, out rateLimitPunishment);
				RateLimitAction action = rateLimitAction;
				RateLimitPunishment punishment = rateLimitPunishment;
				switch (action)
				{
				case RateLimitAction.Invalid:
					continue;
				case RateLimitAction.OnLimitReached:
					if (requests < this.maxRequests)
					{
						continue;
					}
					break;
				case RateLimitAction.OnLimitDoubled:
					if (requests < this.maxRequests * 2)
					{
						continue;
					}
					break;
				default:
					continue;
				}
				switch (punishment)
				{
				case RateLimitPunishment.Announce:
					this.AnnounceOffender(client);
					break;
				case RateLimitPunishment.Kick:
					if (isDosProtectionEnabled)
					{
						GameServer server2 = GameMain.Server;
						if (server2 != null)
						{
							server2.KickClient(client, TextManager.Get("SpamFilterKicked").Value, false);
						}
					}
					break;
				case RateLimitPunishment.Ban:
					if (isDosProtectionEnabled)
					{
						GameServer server3 = GameMain.Server;
						if (server3 != null)
						{
							server3.BanClient(client, TextManager.Get("SpamFilterKicked").Value, null);
						}
					}
					break;
				}
			}
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x00071E58 File Offset: 0x00070058
		private void AnnounceOffender(Client client)
		{
			DateTimeOffset expiry;
			if (this.recentlyAnnouncedOffenders.TryGetValue(client, out expiry))
			{
				if (expiry > DateTimeOffset.Now)
				{
					return;
				}
				this.recentlyAnnouncedOffenders.Remove(client);
			}
			GameServer.Log(NetworkMember.ClientLogName(client, null) + " is sending too many packets!", ServerLog.MessageType.DoSProtection);
			this.recentlyAnnouncedOffenders.Add(client, DateTimeOffset.Now.AddSeconds((double)this.expiryInSeconds));
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x00071EC8 File Offset: 0x000700C8
		public static bool IsExempt(Client client)
		{
			return (GameMain.Server.OwnerConnection != null && client.Connection == GameMain.Server.OwnerConnection) || client.HasPermission(ClientPermissions.SpamImmunity);
		}

		// Token: 0x0400052E RID: 1326
		private readonly Dictionary<Client, RateLimiter.RateLimit> rateLimits = new Dictionary<Client, RateLimiter.RateLimit>();

		// Token: 0x0400052F RID: 1327
		private readonly HashSet<Client> expiredRateLimits = new HashSet<Client>();

		// Token: 0x04000530 RID: 1328
		private readonly Dictionary<Client, DateTimeOffset> recentlyAnnouncedOffenders = new Dictionary<Client, DateTimeOffset>();

		// Token: 0x04000531 RID: 1329
		private readonly int maxRequests;

		// Token: 0x04000532 RID: 1330
		private readonly int expiryInSeconds;

		// Token: 0x04000533 RID: 1331
		private readonly ImmutableDictionary<RateLimitAction, RateLimitPunishment> punishments;

		// Token: 0x0200075D RID: 1885
		[Nullable(0)]
		private sealed class RateLimit : IEquatable<RateLimiter.RateLimit>
		{
			// Token: 0x060051D5 RID: 20949 RVA: 0x001E9E04 File Offset: 0x001E8004
			public RateLimit(DateTimeOffset Expiry)
			{
				this.Expiry = Expiry;
				base..ctor();
			}

			// Token: 0x17001438 RID: 5176
			// (get) Token: 0x060051D6 RID: 20950 RVA: 0x001E9E13 File Offset: 0x001E8013
			[CompilerGenerated]
			private Type EqualityContract
			{
				[CompilerGenerated]
				get
				{
					return typeof(RateLimiter.RateLimit);
				}
			}

			// Token: 0x17001439 RID: 5177
			// (get) Token: 0x060051D7 RID: 20951 RVA: 0x001E9E1F File Offset: 0x001E801F
			// (set) Token: 0x060051D8 RID: 20952 RVA: 0x001E9E27 File Offset: 0x001E8027
			public DateTimeOffset Expiry { get; set; }

			// Token: 0x060051D9 RID: 20953 RVA: 0x001E9E30 File Offset: 0x001E8030
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("RateLimit");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060051DA RID: 20954 RVA: 0x001E9E7C File Offset: 0x001E807C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
				builder.Append("Expiry = ");
				builder.Append(this.Expiry.ToString());
				builder.Append(", RequestAmount = ");
				builder.Append(this.RequestAmount.ToString());
				return true;
			}

			// Token: 0x060051DB RID: 20955 RVA: 0x001E9EDA File Offset: 0x001E80DA
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator !=(RateLimiter.RateLimit left, RateLimiter.RateLimit right)
			{
				return !(left == right);
			}

			// Token: 0x060051DC RID: 20956 RVA: 0x001E9EE6 File Offset: 0x001E80E6
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator ==(RateLimiter.RateLimit left, RateLimiter.RateLimit right)
			{
				return left == right || (left != null && left.Equals(right));
			}

			// Token: 0x060051DD RID: 20957 RVA: 0x001E9EFA File Offset: 0x001E80FA
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<DateTimeOffset>.Default.GetHashCode(this.<Expiry>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.RequestAmount);
			}

			// Token: 0x060051DE RID: 20958 RVA: 0x001E9F3A File Offset: 0x001E813A
			[NullableContext(2)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return this.Equals(obj as RateLimiter.RateLimit);
			}

			// Token: 0x060051DF RID: 20959 RVA: 0x001E9F48 File Offset: 0x001E8148
			[NullableContext(2)]
			[CompilerGenerated]
			public bool Equals(RateLimiter.RateLimit other)
			{
				return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<DateTimeOffset>.Default.Equals(this.<Expiry>k__BackingField, other.<Expiry>k__BackingField) && EqualityComparer<int>.Default.Equals(this.RequestAmount, other.RequestAmount));
			}

			// Token: 0x060051E1 RID: 20961 RVA: 0x001E9FA9 File Offset: 0x001E81A9
			[CompilerGenerated]
			private RateLimit(RateLimiter.RateLimit original)
			{
				this.Expiry = original.<Expiry>k__BackingField;
				this.RequestAmount = original.RequestAmount;
			}

			// Token: 0x060051E2 RID: 20962 RVA: 0x001E9FC9 File Offset: 0x001E81C9
			[CompilerGenerated]
			public void Deconstruct(out DateTimeOffset Expiry)
			{
				Expiry = this.Expiry;
			}

			// Token: 0x04002CC6 RID: 11462
			public int RequestAmount;
		}
	}
}
