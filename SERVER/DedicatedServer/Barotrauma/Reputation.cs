using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001D7 RID: 471
	internal class Reputation
	{
		// Token: 0x170009E8 RID: 2536
		// (get) Token: 0x06002297 RID: 8855 RVA: 0x000E8881 File Offset: 0x000E6A81
		public Identifier Identifier { get; }

		// Token: 0x170009E9 RID: 2537
		// (get) Token: 0x06002298 RID: 8856 RVA: 0x000E8889 File Offset: 0x000E6A89
		public int MinReputation { get; }

		// Token: 0x170009EA RID: 2538
		// (get) Token: 0x06002299 RID: 8857 RVA: 0x000E8891 File Offset: 0x000E6A91
		public int MaxReputation { get; }

		// Token: 0x170009EB RID: 2539
		// (get) Token: 0x0600229A RID: 8858 RVA: 0x000E8899 File Offset: 0x000E6A99
		public int InitialReputation { get; }

		// Token: 0x170009EC RID: 2540
		// (get) Token: 0x0600229B RID: 8859 RVA: 0x000E88A1 File Offset: 0x000E6AA1
		public CampaignMetadata Metadata { get; }

		// Token: 0x170009ED RID: 2541
		// (get) Token: 0x0600229C RID: 8860 RVA: 0x000E88A9 File Offset: 0x000E6AA9
		// (set) Token: 0x0600229D RID: 8861 RVA: 0x000E88B1 File Offset: 0x000E6AB1
		public float ReputationAtRoundStart { get; set; }

		// Token: 0x170009EE RID: 2542
		// (get) Token: 0x0600229E RID: 8862 RVA: 0x000E88BA File Offset: 0x000E6ABA
		public float NormalizedValue
		{
			get
			{
				return MathUtils.InverseLerp((float)this.MinReputation, (float)this.MaxReputation, this.Value);
			}
		}

		// Token: 0x170009EF RID: 2543
		// (get) Token: 0x0600229F RID: 8863 RVA: 0x000E88D5 File Offset: 0x000E6AD5
		// (set) Token: 0x060022A0 RID: 8864 RVA: 0x000E8910 File Offset: 0x000E6B10
		public float Value
		{
			get
			{
				if (this.Metadata != null)
				{
					return Math.Min((float)this.MaxReputation, this.Metadata.GetFloat(this.metaDataIdentifier, new float?((float)this.InitialReputation)));
				}
				return 0f;
			}
			private set
			{
				if (MathUtils.NearlyEqual(this.Value, value, 0.0001f) || this.Metadata == null)
				{
					return;
				}
				float prevValue = this.Value;
				this.Metadata.SetValue(this.metaDataIdentifier, Math.Clamp(value, (float)this.MinReputation, (float)this.MaxReputation));
				NamedEvent<Reputation> onReputationValueChanged = this.OnReputationValueChanged;
				if (onReputationValueChanged != null)
				{
					onReputationValueChanged.Invoke(this);
				}
				NamedEvent<Reputation> onAnyReputationValueChanged = Reputation.OnAnyReputationValueChanged;
				if (onAnyReputationValueChanged == null)
				{
					return;
				}
				onAnyReputationValueChanged.Invoke(this);
			}
		}

		// Token: 0x060022A1 RID: 8865 RVA: 0x000E898C File Offset: 0x000E6B8C
		public void SetReputation(float newReputation)
		{
			this.Value = newReputation;
		}

		// Token: 0x060022A2 RID: 8866 RVA: 0x000E8998 File Offset: 0x000E6B98
		public float GetReputationChangeMultiplier(float reputationChange)
		{
			float result;
			if (reputationChange <= 0f)
			{
				if (reputationChange >= 0f)
				{
					result = 1f;
				}
				else
				{
					result = Reputation.<GetReputationChangeMultiplier>g__GetMultiplierForStatType|34_0(StatTypes.ReputationLossMultiplier, this.Identifier);
				}
			}
			else
			{
				result = Reputation.<GetReputationChangeMultiplier>g__GetMultiplierForStatType|34_0(StatTypes.ReputationGainMultiplier, this.Identifier);
			}
			return result;
		}

		// Token: 0x060022A3 RID: 8867 RVA: 0x000E89E0 File Offset: 0x000E6BE0
		public void AddReputation(float reputationChange, float maxReputationChangePerRound = 3.4028235E+38f)
		{
			Reputation.<>c__DisplayClass35_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.maxReputationChangePerRound = maxReputationChangePerRound;
			float prevValue = this.Value;
			if (this.<AddReputation>g__doesReputationChangeGoOverLimit|35_0(prevValue, reputationChange, ref CS$<>8__locals1))
			{
				return;
			}
			float newValue = this.Value + reputationChange * this.GetReputationChangeMultiplier(reputationChange);
			if (this.<AddReputation>g__doesReputationChangeGoOverLimit|35_0(newValue, newValue - prevValue, ref CS$<>8__locals1))
			{
				newValue = this.ReputationAtRoundStart + CS$<>8__locals1.maxReputationChangePerRound * (float)Math.Sign(reputationChange);
			}
			this.Value = newValue;
		}

		// Token: 0x060022A4 RID: 8868 RVA: 0x000E8A4D File Offset: 0x000E6C4D
		public Reputation(CampaignMetadata metadata, Location location, Identifier identifier, int minReputation, int maxReputation, int initialReputation) : this(metadata, null, location, identifier, minReputation, maxReputation, initialReputation)
		{
		}

		// Token: 0x060022A5 RID: 8869 RVA: 0x000E8A60 File Offset: 0x000E6C60
		public Reputation(CampaignMetadata metadata, Faction faction, int minReputation, int maxReputation, int initialReputation)
		{
			Location location = null;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
			defaultInterpolatedStringHandler.AppendLiteral("faction.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(faction.Prefab.Identifier);
			this..ctor(metadata, faction, location, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), minReputation, maxReputation, initialReputation);
		}

		// Token: 0x060022A6 RID: 8870 RVA: 0x000E8AB0 File Offset: 0x000E6CB0
		private Reputation(CampaignMetadata metadata, Faction faction, Location location, Identifier identifier, int minReputation, int maxReputation, int initialReputation)
		{
			this.OnReputationValueChanged = new NamedEvent<Reputation>();
			base..ctor();
			this.Metadata = metadata;
			this.Identifier = identifier;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler.AppendLiteral("reputation.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			this.metaDataIdentifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			this.MinReputation = minReputation;
			this.MaxReputation = maxReputation;
			this.ReputationAtRoundStart = (float)initialReputation;
			this.InitialReputation = initialReputation;
			this.Faction = faction;
			this.Location = location;
		}

		// Token: 0x060022A7 RID: 8871 RVA: 0x000E8B41 File Offset: 0x000E6D41
		public LocalizedString GetReputationName()
		{
			return Reputation.GetReputationName(this.NormalizedValue);
		}

		// Token: 0x060022A8 RID: 8872 RVA: 0x000E8B50 File Offset: 0x000E6D50
		public static LocalizedString GetReputationName(float normalizedValue)
		{
			if (normalizedValue < 0.2f)
			{
				return TextManager.Get("reputationverylow");
			}
			if (normalizedValue < 0.4f)
			{
				return TextManager.Get("reputationlow");
			}
			if (normalizedValue < 0.6f)
			{
				return TextManager.Get("reputationneutral");
			}
			if (normalizedValue < 0.8f)
			{
				return TextManager.Get("reputationhigh");
			}
			return TextManager.Get("reputationveryhigh");
		}

		// Token: 0x060022AA RID: 8874 RVA: 0x000E8BC0 File Offset: 0x000E6DC0
		[CompilerGenerated]
		internal static float <GetReputationChangeMultiplier>g__GetMultiplierForStatType|34_0(StatTypes statTypes, Identifier identifier)
		{
			float multiplier = 1f;
			ImmutableHashSet<Character> crew = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			if (crew != null && crew.Any<Character>())
			{
				multiplier *= 1f + crew.Max((Character c) => c.GetStatValue(statTypes, false));
				multiplier *= 1f + crew.Max(delegate(Character c)
				{
					CharacterInfo info = c.Info;
					if (info == null)
					{
						return 0f;
					}
					return info.GetSavedStatValue(statTypes, identifier);
				});
			}
			return multiplier;
		}

		// Token: 0x060022AB RID: 8875 RVA: 0x000E8C30 File Offset: 0x000E6E30
		[CompilerGenerated]
		private bool <AddReputation>g__doesReputationChangeGoOverLimit|35_0(float newValue, float change, ref Reputation.<>c__DisplayClass35_0 A_3)
		{
			float totalReputationChange = newValue - this.ReputationAtRoundStart;
			return Math.Abs(totalReputationChange) > A_3.maxReputationChangePerRound && Math.Sign(totalReputationChange) == Math.Sign(change);
		}

		// Token: 0x04001092 RID: 4242
		public const float HostileThreshold = 0.2f;

		// Token: 0x04001093 RID: 4243
		public const float ReputationLossPerNPCDamage = 0.025f;

		// Token: 0x04001094 RID: 4244
		public const float ReputationLossPerWallDamage = 0.025f;

		// Token: 0x04001095 RID: 4245
		public const float ReputationLossPerStolenItemPrice = 0.0025f;

		// Token: 0x04001096 RID: 4246
		public const float MinReputationLossPerStolenItem = 0.025f;

		// Token: 0x04001097 RID: 4247
		public const float MaxReputationLossPerStolenItem = 0.5f;

		// Token: 0x04001098 RID: 4248
		public const float MaxReputationLossFromNPCDamage = 20f;

		// Token: 0x04001099 RID: 4249
		public const float MaxReputationLossFromWallDamage = 10f;

		// Token: 0x040010A0 RID: 4256
		private readonly Identifier metaDataIdentifier;

		// Token: 0x040010A1 RID: 4257
		public readonly NamedEvent<Reputation> OnReputationValueChanged;

		// Token: 0x040010A2 RID: 4258
		public static readonly NamedEvent<Reputation> OnAnyReputationValueChanged = new NamedEvent<Reputation>();

		// Token: 0x040010A3 RID: 4259
		public readonly Faction Faction;

		// Token: 0x040010A4 RID: 4260
		public readonly Location Location;
	}
}
