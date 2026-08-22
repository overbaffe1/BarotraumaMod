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
	// Token: 0x020002B1 RID: 689
	[NullableContext(1)]
	[Nullable(0)]
	internal class TraitorEventPrefab : EventPrefab
	{
		// Token: 0x17000DBD RID: 3517
		// (get) Token: 0x06002F5A RID: 12122 RVA: 0x0013ADAF File Offset: 0x00138FAF
		public bool HasReputationRequirements
		{
			get
			{
				return this.reputationRequirements.Any<TraitorEventPrefab.ReputationRequirement>();
			}
		}

		// Token: 0x17000DBE RID: 3518
		// (get) Token: 0x06002F5B RID: 12123 RVA: 0x0013ADBC File Offset: 0x00138FBC
		public bool HasMissionRequirements
		{
			get
			{
				return this.missionRequirements.Any<TraitorEventPrefab.MissionRequirement>();
			}
		}

		// Token: 0x17000DBF RID: 3519
		// (get) Token: 0x06002F5C RID: 12124 RVA: 0x0013ADC9 File Offset: 0x00138FC9
		public bool HasLevelRequirements
		{
			get
			{
				return this.levelRequirements.Any<TraitorEventPrefab.LevelRequirement>();
			}
		}

		// Token: 0x06002F5D RID: 12125 RVA: 0x0013ADD8 File Offset: 0x00138FD8
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

		// Token: 0x06002F5E RID: 12126 RVA: 0x0013B00C File Offset: 0x0013920C
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

		// Token: 0x06002F5F RID: 12127 RVA: 0x0013B04C File Offset: 0x0013924C
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

		// Token: 0x06002F60 RID: 12128 RVA: 0x0013B0A4 File Offset: 0x001392A4
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

		// Token: 0x06002F61 RID: 12129 RVA: 0x0013B0FF File Offset: 0x001392FF
		public override void Dispose()
		{
			Sprite icon = this.Icon;
			if (icon == null)
			{
				return;
			}
			icon.Remove();
		}

		// Token: 0x06002F62 RID: 12130 RVA: 0x0013B114 File Offset: 0x00139314
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted("TraitorEventPrefab");
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x040017B3 RID: 6067
		[Nullable(2)]
		public readonly Sprite Icon;

		// Token: 0x040017B4 RID: 6068
		public readonly Color IconColor;

		// Token: 0x040017B5 RID: 6069
		public const int MinDangerLevel = 1;

		// Token: 0x040017B6 RID: 6070
		public const int MaxDangerLevel = 3;

		// Token: 0x040017B7 RID: 6071
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<TraitorEventPrefab.ReputationRequirement> reputationRequirements;

		// Token: 0x040017B8 RID: 6072
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<TraitorEventPrefab.MissionRequirement> missionRequirements;

		// Token: 0x040017B9 RID: 6073
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<TraitorEventPrefab.LevelRequirement> levelRequirements;

		// Token: 0x040017BA RID: 6074
		public ImmutableHashSet<Identifier> RequiredCompletedTags;

		// Token: 0x040017BB RID: 6075
		public readonly int DangerLevel;

		// Token: 0x040017BC RID: 6076
		public readonly int RequiredPreviousDangerLevel;

		// Token: 0x040017BD RID: 6077
		public readonly bool RequirePreviousDangerLevelCompleted;

		// Token: 0x040017BE RID: 6078
		public readonly int MinPlayerCount;

		// Token: 0x040017BF RID: 6079
		public readonly int SecondaryTraitorAmount;

		// Token: 0x040017C0 RID: 6080
		public readonly float SecondaryTraitorPercentage;

		// Token: 0x040017C1 RID: 6081
		public readonly bool AllowAccusingSecondaryTraitor;

		// Token: 0x040017C2 RID: 6082
		public readonly int MoneyPenaltyForUnfoundedTraitorAccusation;

		// Token: 0x040017C3 RID: 6083
		public readonly bool IsChainable;

		// Token: 0x040017C4 RID: 6084
		public readonly float StealPercentageOfExperience;

		// Token: 0x02000B41 RID: 2881
		[Nullable(0)]
		private class MissionRequirement
		{
			// Token: 0x06006046 RID: 24646 RVA: 0x00209B50 File Offset: 0x00207D50
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

			// Token: 0x06006047 RID: 24647 RVA: 0x00209C68 File Offset: 0x00207E68
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

			// Token: 0x0400390C RID: 14604
			public Identifier MissionIdentifier;

			// Token: 0x0400390D RID: 14605
			public Identifier MissionTag;

			// Token: 0x0400390E RID: 14606
			public Identifier MissionType;
		}

		// Token: 0x02000B42 RID: 2882
		[NullableContext(0)]
		private class LevelRequirement
		{
			// Token: 0x170015E3 RID: 5603
			// (get) Token: 0x06006048 RID: 24648 RVA: 0x00209D15 File Offset: 0x00207F15
			public ImmutableArray<Identifier> LocationTypes { get; }

			// Token: 0x06006049 RID: 24649 RVA: 0x00209D20 File Offset: 0x00207F20
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

			// Token: 0x0600604A RID: 24650 RVA: 0x00209E10 File Offset: 0x00208010
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

			// Token: 0x0400390F RID: 14607
			private readonly TraitorEventPrefab.LevelRequirement.LevelType levelType;

			// Token: 0x04003911 RID: 14609
			private readonly float minDifficulty;

			// Token: 0x04003912 RID: 14610
			private readonly float minDifficultyInCampaign;

			// Token: 0x04003913 RID: 14611
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public ImmutableArray<PropertyConditional> RequiredItemConditionals;

			// Token: 0x02000EC9 RID: 3785
			private enum LevelType
			{
				// Token: 0x04004372 RID: 17266
				LocationConnection,
				// Token: 0x04004373 RID: 17267
				Outpost,
				// Token: 0x04004374 RID: 17268
				Any
			}
		}

		// Token: 0x02000B43 RID: 2883
		[Nullable(0)]
		private class ReputationRequirement
		{
			// Token: 0x0600604B RID: 24651 RVA: 0x00209F54 File Offset: 0x00208154
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

			// Token: 0x0600604C RID: 24652 RVA: 0x0020A038 File Offset: 0x00208238
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

			// Token: 0x04003914 RID: 14612
			public Identifier Faction;

			// Token: 0x04003915 RID: 14613
			public Identifier CompareToFaction;

			// Token: 0x04003916 RID: 14614
			public float CompareToValue;

			// Token: 0x04003917 RID: 14615
			public readonly PropertyConditional.ComparisonOperatorType Operator;
		}
	}
}
