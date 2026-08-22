using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200037D RID: 893
	[NullableContext(1)]
	[Nullable(0)]
	internal class TraitorEventPrefab : EventPrefab
	{
		// Token: 0x170011C0 RID: 4544
		// (get) Token: 0x060043F2 RID: 17394 RVA: 0x002552F3 File Offset: 0x002534F3
		public bool HasReputationRequirements
		{
			get
			{
				return this.reputationRequirements.Any<TraitorEventPrefab.ReputationRequirement>();
			}
		}

		// Token: 0x170011C1 RID: 4545
		// (get) Token: 0x060043F3 RID: 17395 RVA: 0x00255300 File Offset: 0x00253500
		public bool HasMissionRequirements
		{
			get
			{
				return this.missionRequirements.Any<TraitorEventPrefab.MissionRequirement>();
			}
		}

		// Token: 0x170011C2 RID: 4546
		// (get) Token: 0x060043F4 RID: 17396 RVA: 0x0025530D File Offset: 0x0025350D
		public bool HasLevelRequirements
		{
			get
			{
				return this.levelRequirements.Any<TraitorEventPrefab.LevelRequirement>();
			}
		}

		// Token: 0x060043F5 RID: 17397 RVA: 0x0025531C File Offset: 0x0025351C
		public TraitorEventPrefab(ContentXElement element, RandomEventsFile file, Identifier fallbackIdentifier = default(Identifier)) : base(element, file, fallbackIdentifier)
		{
			this.DangerLevel = MathHelper.Clamp(element.GetAttributeInt("DangerLevel", 1), 1, 3);
			this.RequiredPreviousDangerLevel = MathHelper.Clamp(element.GetAttributeInt("RequiredPreviousDangerLevel", this.DangerLevel - 1), 0, 2);
			this.RequirePreviousDangerLevelCompleted = element.GetAttributeBool("RequirePreviousDangerLevelCompleted", false);
			this.MinPlayerCount = element.GetAttributeInt("MinPlayerCount", 0);
			this.SecondaryTraitorAmount = element.GetAttributeInt("SecondaryTraitorAmount", 0);
			this.SecondaryTraitorPercentage = element.GetAttributeFloat("SecondaryTraitorPercentage", 0f);
			this.AllowAccusingSecondaryTraitor = element.GetAttributeBool("AllowAccusingSecondaryTraitor", true);
			this.MoneyPenaltyForUnfoundedTraitorAccusation = element.GetAttributeInt("MoneyPenaltyForUnfoundedTraitorAccusation", 100);
			this.RequiredCompletedTags = element.GetAttributeIdentifierImmutableHashSet("RequiredCompletedTags", ImmutableHashSet<Identifier>.Empty, true);
			this.StealPercentageOfExperience = element.GetAttributeFloat("StealPercentageOfExperience", 0f);
			this.IsChainable = element.GetAttributeBool("IsChainable", true);
			List<TraitorEventPrefab.ReputationRequirement> reputationRequirements = new List<TraitorEventPrefab.ReputationRequirement>();
			List<TraitorEventPrefab.LevelRequirement> levelRequirements = new List<TraitorEventPrefab.LevelRequirement>();
			List<TraitorEventPrefab.MissionRequirement> missionRequirements = new List<TraitorEventPrefab.MissionRequirement>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "reputationrequirement"))
				{
					if (!(a == "missionrequirement"))
					{
						if (!(a == "levelrequirement"))
						{
							if (a == "icon")
							{
								this.Icon = new Sprite(subElement, "", "", false, 1f);
								ContentXElement contentXElement = subElement;
								string key = "color";
								Color white = Color.White;
								this.IconColor = contentXElement.GetAttributeColor(key, white);
							}
						}
						else
						{
							levelRequirements.Add(new TraitorEventPrefab.LevelRequirement(subElement, this));
						}
					}
					else
					{
						missionRequirements.Add(new TraitorEventPrefab.MissionRequirement(subElement, this));
					}
				}
				else
				{
					reputationRequirements.Add(new TraitorEventPrefab.ReputationRequirement(subElement, this));
				}
			}
			this.reputationRequirements = reputationRequirements.ToImmutableArray<TraitorEventPrefab.ReputationRequirement>();
			this.levelRequirements = levelRequirements.ToImmutableArray<TraitorEventPrefab.LevelRequirement>();
			this.missionRequirements = missionRequirements.ToImmutableArray<TraitorEventPrefab.MissionRequirement>();
		}

		// Token: 0x060043F6 RID: 17398 RVA: 0x00255550 File Offset: 0x00253750
		[NullableContext(2)]
		public bool ReputationRequirementsMet(CampaignMode campaign)
		{
			if (campaign == null)
			{
				return true;
			}
			foreach (TraitorEventPrefab.ReputationRequirement requirement in this.reputationRequirements)
			{
				if (!requirement.Match(campaign))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060043F7 RID: 17399 RVA: 0x00255590 File Offset: 0x00253790
		[NullableContext(2)]
		public bool MissionRequirementsMet(GameSession gameSession)
		{
			if (gameSession == null)
			{
				return false;
			}
			ImmutableArray<TraitorEventPrefab.MissionRequirement>.Enumerator enumerator = this.missionRequirements.GetEnumerator();
			while (enumerator.MoveNext())
			{
				TraitorEventPrefab.MissionRequirement requirement = enumerator.Current;
				if (gameSession.Missions.None((Mission m) => requirement.Match(m)))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060043F8 RID: 17400 RVA: 0x002555E8 File Offset: 0x002537E8
		[NullableContext(2)]
		public bool LevelRequirementsMet(Level level)
		{
			if (level == null)
			{
				return false;
			}
			if (this.levelRequirements.None(null) && level.Type != LevelData.LevelType.LocationConnection)
			{
				return false;
			}
			foreach (TraitorEventPrefab.LevelRequirement requirement in this.levelRequirements)
			{
				if (!requirement.Match(level))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060043F9 RID: 17401 RVA: 0x00255643 File Offset: 0x00253843
		public override void Dispose()
		{
			Sprite icon = this.Icon;
			if (icon == null)
			{
				return;
			}
			icon.Remove();
		}

		// Token: 0x060043FA RID: 17402 RVA: 0x00255658 File Offset: 0x00253858
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted("TraitorEventPrefab");
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04002396 RID: 9110
		[Nullable(2)]
		public readonly Sprite Icon;

		// Token: 0x04002397 RID: 9111
		public readonly Color IconColor;

		// Token: 0x04002398 RID: 9112
		public const int MinDangerLevel = 1;

		// Token: 0x04002399 RID: 9113
		public const int MaxDangerLevel = 3;

		// Token: 0x0400239A RID: 9114
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<TraitorEventPrefab.ReputationRequirement> reputationRequirements;

		// Token: 0x0400239B RID: 9115
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<TraitorEventPrefab.MissionRequirement> missionRequirements;

		// Token: 0x0400239C RID: 9116
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<TraitorEventPrefab.LevelRequirement> levelRequirements;

		// Token: 0x0400239D RID: 9117
		public ImmutableHashSet<Identifier> RequiredCompletedTags;

		// Token: 0x0400239E RID: 9118
		public readonly int DangerLevel;

		// Token: 0x0400239F RID: 9119
		public readonly int RequiredPreviousDangerLevel;

		// Token: 0x040023A0 RID: 9120
		public readonly bool RequirePreviousDangerLevelCompleted;

		// Token: 0x040023A1 RID: 9121
		public readonly int MinPlayerCount;

		// Token: 0x040023A2 RID: 9122
		public readonly int SecondaryTraitorAmount;

		// Token: 0x040023A3 RID: 9123
		public readonly float SecondaryTraitorPercentage;

		// Token: 0x040023A4 RID: 9124
		public readonly bool AllowAccusingSecondaryTraitor;

		// Token: 0x040023A5 RID: 9125
		public readonly int MoneyPenaltyForUnfoundedTraitorAccusation;

		// Token: 0x040023A6 RID: 9126
		public readonly bool IsChainable;

		// Token: 0x040023A7 RID: 9127
		public readonly float StealPercentageOfExperience;

		// Token: 0x0200109D RID: 4253
		[Nullable(0)]
		private class MissionRequirement
		{
			// Token: 0x06008D4E RID: 36174 RVA: 0x003B1920 File Offset: 0x003AFB20
			public MissionRequirement(XElement element, TraitorEventPrefab prefab)
			{
				this.MissionIdentifier = element.GetAttributeIdentifier("MissionIdentifier", Identifier.Empty);
				this.MissionTag = element.GetAttributeIdentifier("MissionTag", Identifier.Empty);
				this.MissionType = element.GetAttributeIdentifier("MissionType", Identifier.Empty);
				if (this.MissionIdentifier.IsEmpty && this.MissionTag.IsEmpty && this.MissionType == Identifier.Empty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Error in traitor event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". Mission requirement with no ");
					defaultInterpolatedStringHandler.AppendFormatted("MissionIdentifier");
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted("MissionTag");
					defaultInterpolatedStringHandler.AppendLiteral(" or ");
					defaultInterpolatedStringHandler.AppendFormatted("MissionType");
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, prefab.ContentPackage, false, false);
				}
			}

			// Token: 0x06008D4F RID: 36175 RVA: 0x003B1A38 File Offset: 0x003AFC38
			public bool Match(Mission mission)
			{
				if (mission == null)
				{
					return this.MissionIdentifier.IsEmpty && this.MissionTag.IsEmpty && this.MissionType == Identifier.Empty;
				}
				if (!this.MissionIdentifier.IsEmpty)
				{
					return mission.Prefab.Identifier == this.MissionIdentifier;
				}
				if (!this.MissionTag.IsEmpty)
				{
					return mission.Prefab.Tags.Contains(this.MissionTag);
				}
				if (!this.MissionType.IsEmpty)
				{
					Identifier type = mission.Prefab.Type;
					return type == this.MissionType;
				}
				return false;
			}

			// Token: 0x04005940 RID: 22848
			public Identifier MissionIdentifier;

			// Token: 0x04005941 RID: 22849
			public Identifier MissionTag;

			// Token: 0x04005942 RID: 22850
			public Identifier MissionType;
		}

		// Token: 0x0200109E RID: 4254
		[NullableContext(0)]
		private class LevelRequirement
		{
			// Token: 0x17001C7D RID: 7293
			// (get) Token: 0x06008D50 RID: 36176 RVA: 0x003B1AE5 File Offset: 0x003AFCE5
			public ImmutableArray<Identifier> LocationTypes { get; }

			// Token: 0x06008D51 RID: 36177 RVA: 0x003B1AF0 File Offset: 0x003AFCF0
			[NullableContext(1)]
			public LevelRequirement(ContentXElement element, TraitorEventPrefab prefab)
			{
				string key = "LevelType";
				TraitorEventPrefab.LevelRequirement.LevelType levelType = TraitorEventPrefab.LevelRequirement.LevelType.Any;
				this.levelType = element.GetAttributeEnum<TraitorEventPrefab.LevelRequirement.LevelType>(key, levelType);
				this.LocationTypes = element.GetAttributeIdentifierArray("locationtype", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
				this.minDifficulty = element.GetAttributeFloat("minDifficulty", 0f);
				this.minDifficultyInCampaign = element.GetAttributeFloat("minDifficultyInCampaign", Math.Max(this.minDifficulty, 5f));
				List<PropertyConditional> requiredItemConditionals = new List<PropertyConditional>();
				foreach (ContentXElement subElement in element.Elements())
				{
					Identifier identifier = subElement.NameAsIdentifier();
					if (identifier == "itemconditional")
					{
						requiredItemConditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
					}
				}
				this.RequiredItemConditionals = requiredItemConditionals.ToImmutableArray<PropertyConditional>();
			}

			// Token: 0x06008D52 RID: 36178 RVA: 0x003B1BE0 File Offset: 0x003AFDE0
			[NullableContext(1)]
			public bool Match(Level level)
			{
				if (((level != null) ? level.LevelData : null) == null)
				{
					return false;
				}
				TraitorEventPrefab.LevelRequirement.LevelType levelType = this.levelType;
				if (levelType != TraitorEventPrefab.LevelRequirement.LevelType.LocationConnection)
				{
					if (levelType == TraitorEventPrefab.LevelRequirement.LevelType.Outpost)
					{
						if (level.LevelData.Type != LevelData.LevelType.Outpost)
						{
							return false;
						}
					}
				}
				else if (level.LevelData.Type != LevelData.LevelType.LocationConnection)
				{
					return false;
				}
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) != null)
				{
					if (level.Difficulty < this.minDifficultyInCampaign)
					{
						return false;
					}
				}
				else if (level.Difficulty < this.minDifficulty)
				{
					return false;
				}
				if (level.StartLocation == null)
				{
					if (this.LocationTypes.Any<Identifier>())
					{
						return false;
					}
				}
				else if (this.LocationTypes.Any<Identifier>() && !this.LocationTypes.Contains(level.StartLocation.Type.Identifier))
				{
					return false;
				}
				if (this.RequiredItemConditionals.Any<PropertyConditional>())
				{
					bool matchFound = false;
					using (List<Item>.Enumerator enumerator = Item.ItemList.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Item item = enumerator.Current;
							if (this.RequiredItemConditionals.All((PropertyConditional c) => item.ConditionalMatches(c)))
							{
								matchFound = true;
								break;
							}
						}
					}
					if (!matchFound)
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x04005943 RID: 22851
			private readonly TraitorEventPrefab.LevelRequirement.LevelType levelType;

			// Token: 0x04005945 RID: 22853
			private readonly float minDifficulty;

			// Token: 0x04005946 RID: 22854
			private readonly float minDifficultyInCampaign;

			// Token: 0x04005947 RID: 22855
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public ImmutableArray<PropertyConditional> RequiredItemConditionals;

			// Token: 0x02001587 RID: 5511
			private enum LevelType
			{
				// Token: 0x040068C9 RID: 26825
				LocationConnection,
				// Token: 0x040068CA RID: 26826
				Outpost,
				// Token: 0x040068CB RID: 26827
				Any
			}
		}

		// Token: 0x0200109F RID: 4255
		[Nullable(0)]
		private class ReputationRequirement
		{
			// Token: 0x06008D53 RID: 36179 RVA: 0x003B1D24 File Offset: 0x003AFF24
			public ReputationRequirement(XElement element, TraitorEventPrefab prefab)
			{
				this.Faction = element.GetAttributeIdentifier("Faction", Identifier.Empty);
				string conditionStr = element.GetAttributeString("reputation", string.Empty);
				string[] splitString = conditionStr.Split(' ', StringSplitOptions.None);
				if (splitString.Length == 0)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 2);
					defaultInterpolatedStringHandler.AppendFormatted(conditionStr);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(" is too short.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + "It should start with an operator followed by a faction identifier or a floating point value.", null, null, false, false);
					return;
				}
				string value = string.Join(" ", splitString.Skip(1));
				this.Operator = PropertyConditional.GetComparisonOperatorType(splitString[0]);
				float floatVal;
				if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out floatVal))
				{
					this.CompareToValue = floatVal;
					return;
				}
				this.CompareToFaction = value.ToIdentifier();
			}

			// Token: 0x06008D54 RID: 36180 RVA: 0x003B1E08 File Offset: 0x003B0008
			public bool Match(CampaignMode campaign)
			{
				Faction faction = campaign.GetFaction(this.Faction);
				if (faction == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find the faction ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Faction);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return false;
				}
				if (this.CompareToFaction.IsEmpty)
				{
					return PropertyConditional.CompareFloat(faction.Reputation.Value, this.CompareToValue, this.Operator);
				}
				Faction faction2 = campaign.GetFaction(this.Faction);
				if (faction2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Could not find the faction ");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.CompareToFaction);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					return false;
				}
				return PropertyConditional.CompareFloat(faction.Reputation.Value, faction2.Reputation.Value, this.Operator);
			}

			// Token: 0x04005948 RID: 22856
			public Identifier Faction;

			// Token: 0x04005949 RID: 22857
			public Identifier CompareToFaction;

			// Token: 0x0400594A RID: 22858
			public float CompareToValue;

			// Token: 0x0400594B RID: 22859
			public readonly PropertyConditional.ComparisonOperatorType Operator;
		}
	}
}
