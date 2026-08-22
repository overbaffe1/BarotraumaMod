using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002C4 RID: 708
	internal class Reputation
	{
		// Token: 0x17000FF4 RID: 4084
		// (get) Token: 0x06003C96 RID: 15510 RVA: 0x0022B739 File Offset: 0x00229939
		public Identifier Identifier { get; }

		// Token: 0x17000FF5 RID: 4085
		// (get) Token: 0x06003C97 RID: 15511 RVA: 0x0022B741 File Offset: 0x00229941
		public int MinReputation { get; }

		// Token: 0x17000FF6 RID: 4086
		// (get) Token: 0x06003C98 RID: 15512 RVA: 0x0022B749 File Offset: 0x00229949
		public int MaxReputation { get; }

		// Token: 0x17000FF7 RID: 4087
		// (get) Token: 0x06003C99 RID: 15513 RVA: 0x0022B751 File Offset: 0x00229951
		public int InitialReputation { get; }

		// Token: 0x17000FF8 RID: 4088
		// (get) Token: 0x06003C9A RID: 15514 RVA: 0x0022B759 File Offset: 0x00229959
		public CampaignMetadata Metadata { get; }

		// Token: 0x17000FF9 RID: 4089
		// (get) Token: 0x06003C9B RID: 15515 RVA: 0x0022B761 File Offset: 0x00229961
		// (set) Token: 0x06003C9C RID: 15516 RVA: 0x0022B769 File Offset: 0x00229969
		public float ReputationAtRoundStart { get; set; }

		// Token: 0x17000FFA RID: 4090
		// (get) Token: 0x06003C9D RID: 15517 RVA: 0x0022B772 File Offset: 0x00229972
		public float NormalizedValue
		{
			get
			{
				return MathUtils.InverseLerp((float)this.MinReputation, (float)this.MaxReputation, this.Value);
			}
		}

		// Token: 0x17000FFB RID: 4091
		// (get) Token: 0x06003C9E RID: 15518 RVA: 0x0022B78D File Offset: 0x0022998D
		// (set) Token: 0x06003C9F RID: 15519 RVA: 0x0022B7C8 File Offset: 0x002299C8
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
				if (onAnyReputationValueChanged != null)
				{
					onAnyReputationValueChanged.Invoke(this);
				}
				int increase = (int)this.Value - (int)prevValue;
				if (increase != 0 && Character.Controlled != null)
				{
					Character controlled = Character.Controlled;
					string tag = "reputationgainnotification";
					string varName = "[reputationname]";
					Location location = this.Location;
					controlled.AddMessage(TextManager.GetWithVariable(tag, varName, ((location != null) ? location.DisplayName : null) ?? this.Faction.Prefab.Name, FormatCapitals.No).Value, (increase > 0) ? GUIStyle.Green : GUIStyle.Red, true, this.Identifier, new int?(increase), 5f);
				}
			}
		}

		// Token: 0x06003CA0 RID: 15520 RVA: 0x0022B8C6 File Offset: 0x00229AC6
		public void SetReputation(float newReputation)
		{
			this.Value = newReputation;
		}

		// Token: 0x06003CA1 RID: 15521 RVA: 0x0022B8D0 File Offset: 0x00229AD0
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

		// Token: 0x06003CA2 RID: 15522 RVA: 0x0022B918 File Offset: 0x00229B18
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

		// Token: 0x06003CA3 RID: 15523 RVA: 0x0022B985 File Offset: 0x00229B85
		public Reputation(CampaignMetadata metadata, Location location, Identifier identifier, int minReputation, int maxReputation, int initialReputation) : this(metadata, null, location, identifier, minReputation, maxReputation, initialReputation)
		{
		}

		// Token: 0x06003CA4 RID: 15524 RVA: 0x0022B998 File Offset: 0x00229B98
		public Reputation(CampaignMetadata metadata, Faction faction, int minReputation, int maxReputation, int initialReputation)
		{
			Location location = null;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
			defaultInterpolatedStringHandler.AppendLiteral("faction.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(faction.Prefab.Identifier);
			this..ctor(metadata, faction, location, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), minReputation, maxReputation, initialReputation);
		}

		// Token: 0x06003CA5 RID: 15525 RVA: 0x0022B9E8 File Offset: 0x00229BE8
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

		// Token: 0x06003CA6 RID: 15526 RVA: 0x0022BA79 File Offset: 0x00229C79
		public LocalizedString GetReputationName()
		{
			return Reputation.GetReputationName(this.NormalizedValue);
		}

		// Token: 0x06003CA7 RID: 15527 RVA: 0x0022BA88 File Offset: 0x00229C88
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

		// Token: 0x06003CA8 RID: 15528 RVA: 0x0022BAEC File Offset: 0x00229CEC
		public static Color GetReputationColor(float normalizedValue)
		{
			if (normalizedValue < 0.2f)
			{
				return GUIStyle.ColorReputationVeryLow;
			}
			if (normalizedValue < 0.4f)
			{
				return GUIStyle.ColorReputationLow;
			}
			if (normalizedValue < 0.6f)
			{
				return GUIStyle.ColorReputationNeutral;
			}
			if (normalizedValue < 0.8f)
			{
				return GUIStyle.ColorReputationHigh;
			}
			return GUIStyle.ColorReputationVeryHigh;
		}

		// Token: 0x06003CA9 RID: 15529 RVA: 0x0022BB4F File Offset: 0x00229D4F
		public LocalizedString GetFormattedReputationText(bool addColorTags = false)
		{
			return Reputation.GetFormattedReputationText(this.NormalizedValue, this.Value, addColorTags);
		}

		// Token: 0x06003CAA RID: 15530 RVA: 0x0022BB64 File Offset: 0x00229D64
		public static LocalizedString GetFormattedReputationText(float normalizedValue, float value, bool addColorTags = false)
		{
			LocalizedString reputationName = Reputation.GetReputationName(normalizedValue);
			LocalizedString formattedReputation = TextManager.GetWithVariables("reputationformat", new ValueTuple<string, LocalizedString>[]
			{
				new ValueTuple<string, LocalizedString>("[reputationname]", reputationName),
				new ValueTuple<string, LocalizedString>("[reputationvalue]", ((int)value).ToString())
			});
			if (addColorTags)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(Reputation.GetReputationColor(normalizedValue).ToStringHex());
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(formattedReputation);
				defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
				formattedReputation = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return formattedReputation;
		}

		// Token: 0x06003CAC RID: 15532 RVA: 0x0022BC20 File Offset: 0x00229E20
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

		// Token: 0x06003CAD RID: 15533 RVA: 0x0022BC90 File Offset: 0x00229E90
		[CompilerGenerated]
		private bool <AddReputation>g__doesReputationChangeGoOverLimit|35_0(float newValue, float change, ref Reputation.<>c__DisplayClass35_0 A_3)
		{
			float totalReputationChange = newValue - this.ReputationAtRoundStart;
			return Math.Abs(totalReputationChange) > A_3.maxReputationChangePerRound && Math.Sign(totalReputationChange) == Math.Sign(change);
		}

		// Token: 0x04001F2E RID: 7982
		public const float HostileThreshold = 0.2f;

		// Token: 0x04001F2F RID: 7983
		public const float ReputationLossPerNPCDamage = 0.025f;

		// Token: 0x04001F30 RID: 7984
		public const float ReputationLossPerWallDamage = 0.025f;

		// Token: 0x04001F31 RID: 7985
		public const float ReputationLossPerStolenItemPrice = 0.0025f;

		// Token: 0x04001F32 RID: 7986
		public const float MinReputationLossPerStolenItem = 0.025f;

		// Token: 0x04001F33 RID: 7987
		public const float MaxReputationLossPerStolenItem = 0.5f;

		// Token: 0x04001F34 RID: 7988
		public const float MaxReputationLossFromNPCDamage = 20f;

		// Token: 0x04001F35 RID: 7989
		public const float MaxReputationLossFromWallDamage = 10f;

		// Token: 0x04001F3C RID: 7996
		private readonly Identifier metaDataIdentifier;

		// Token: 0x04001F3D RID: 7997
		public readonly NamedEvent<Reputation> OnReputationValueChanged;

		// Token: 0x04001F3E RID: 7998
		public static readonly NamedEvent<Reputation> OnAnyReputationValueChanged = new NamedEvent<Reputation>();

		// Token: 0x04001F3F RID: 7999
		public readonly Faction Faction;

		// Token: 0x04001F40 RID: 8000
		public readonly Location Location;
	}
}
