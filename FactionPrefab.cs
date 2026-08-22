using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002C3 RID: 707
	[NullableContext(1)]
	[Nullable(0)]
	internal class FactionPrefab : Prefab
	{
		// Token: 0x17000FE5 RID: 4069
		// (get) Token: 0x06003C80 RID: 15488 RVA: 0x0022B2C1 File Offset: 0x002294C1
		public LocalizedString Name { get; }

		// Token: 0x17000FE6 RID: 4070
		// (get) Token: 0x06003C81 RID: 15489 RVA: 0x0022B2C9 File Offset: 0x002294C9
		public LocalizedString Description { get; }

		// Token: 0x17000FE7 RID: 4071
		// (get) Token: 0x06003C82 RID: 15490 RVA: 0x0022B2D1 File Offset: 0x002294D1
		public LocalizedString ShortDescription { get; }

		// Token: 0x17000FE8 RID: 4072
		// (get) Token: 0x06003C83 RID: 15491 RVA: 0x0022B2D9 File Offset: 0x002294D9
		public Identifier OpposingFaction { get; }

		// Token: 0x17000FE9 RID: 4073
		// (get) Token: 0x06003C84 RID: 15492 RVA: 0x0022B2E1 File Offset: 0x002294E1
		public bool StartOutpost { get; }

		// Token: 0x17000FEA RID: 4074
		// (get) Token: 0x06003C85 RID: 15493 RVA: 0x0022B2E9 File Offset: 0x002294E9
		public int MenuOrder { get; }

		// Token: 0x17000FEB RID: 4075
		// (get) Token: 0x06003C86 RID: 15494 RVA: 0x0022B2F1 File Offset: 0x002294F1
		public int MinReputation { get; }

		// Token: 0x17000FEC RID: 4076
		// (get) Token: 0x06003C87 RID: 15495 RVA: 0x0022B2F9 File Offset: 0x002294F9
		public int MaxReputation { get; }

		// Token: 0x17000FED RID: 4077
		// (get) Token: 0x06003C88 RID: 15496 RVA: 0x0022B301 File Offset: 0x00229501
		public int InitialReputation { get; }

		// Token: 0x17000FEE RID: 4078
		// (get) Token: 0x06003C89 RID: 15497 RVA: 0x0022B309 File Offset: 0x00229509
		public float ControlledOutpostPercentage { get; }

		// Token: 0x17000FEF RID: 4079
		// (get) Token: 0x06003C8A RID: 15498 RVA: 0x0022B311 File Offset: 0x00229511
		public float SecondaryControlledOutpostPercentage { get; }

		// Token: 0x17000FF0 RID: 4080
		// (get) Token: 0x06003C8B RID: 15499 RVA: 0x0022B319 File Offset: 0x00229519
		// (set) Token: 0x06003C8C RID: 15500 RVA: 0x0022B321 File Offset: 0x00229521
		[Nullable(2)]
		public Sprite Icon { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000FF1 RID: 4081
		// (get) Token: 0x06003C8D RID: 15501 RVA: 0x0022B32A File Offset: 0x0022952A
		// (set) Token: 0x06003C8E RID: 15502 RVA: 0x0022B332 File Offset: 0x00229532
		[Nullable(2)]
		public Sprite IconSmall { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000FF2 RID: 4082
		// (get) Token: 0x06003C8F RID: 15503 RVA: 0x0022B33B File Offset: 0x0022953B
		// (set) Token: 0x06003C90 RID: 15504 RVA: 0x0022B343 File Offset: 0x00229543
		[Nullable(2)]
		public Sprite BackgroundPortrait { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17000FF3 RID: 4083
		// (get) Token: 0x06003C91 RID: 15505 RVA: 0x0022B34C File Offset: 0x0022954C
		public Color IconColor { get; }

		// Token: 0x06003C92 RID: 15506 RVA: 0x0022B354 File Offset: 0x00229554
		public FactionPrefab(ContentXElement element, FactionsFile file) : base(file, element.GetAttributeIdentifier("identifier", string.Empty))
		{
			this.MenuOrder = element.GetAttributeInt("menuorder", 0);
			this.StartOutpost = element.GetAttributeBool("startoutpost", false);
			this.MinReputation = element.GetAttributeInt("minreputation", -100);
			this.MaxReputation = element.GetAttributeInt("maxreputation", 100);
			this.InitialReputation = element.GetAttributeInt("initialreputation", 0);
			this.ControlledOutpostPercentage = element.GetAttributeFloat("controlledoutpostpercentage", 0f);
			this.SecondaryControlledOutpostPercentage = element.GetAttributeFloat("secondarycontrolledoutpostpercentage", 0f);
			string attributeString = element.GetAttributeString("name", null);
			LocalizedString localizedString;
			if (attributeString == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
				defaultInterpolatedStringHandler.AppendLiteral("faction.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				localizedString = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback("Unnamed", true);
			}
			else
			{
				localizedString = attributeString;
			}
			this.Name = localizedString;
			attributeString = element.GetAttributeString("description", null);
			LocalizedString localizedString2;
			if (attributeString == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("faction.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral(".description");
				localizedString2 = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()).Fallback("", true);
			}
			else
			{
				localizedString2 = attributeString;
			}
			this.Description = localizedString2;
			attributeString = element.GetAttributeString("shortdescription", null);
			LocalizedString localizedString3;
			if (attributeString == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(25, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("faction.");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler3.AppendLiteral(".shortdescription");
				localizedString3 = TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()).Fallback("", true);
			}
			else
			{
				localizedString3 = attributeString;
			}
			this.ShortDescription = localizedString3;
			this.OpposingFaction = element.GetAttributeIdentifier("OpposingFaction", Identifier.Empty);
			List<FactionPrefab.HireableCharacter> hireableCharacters = new List<FactionPrefab.HireableCharacter>();
			List<FactionPrefab.AutomaticMission> automaticMissions = new List<FactionPrefab.AutomaticMission>();
			foreach (ContentXElement subElement in element.Elements())
			{
				Identifier subElementId = subElement.NameAsIdentifier();
				if (subElementId == "icon")
				{
					ContentXElement contentXElement = subElement;
					string key = "color";
					Color white = Color.White;
					this.IconColor = contentXElement.GetAttributeColor(key, white);
					this.Icon = new Sprite(subElement, "", "", false, 1f);
				}
				else if (subElementId == "iconsmall")
				{
					this.IconSmall = new Sprite(subElement, "", "", false, 1f);
				}
				else if (subElementId == "portrait")
				{
					this.BackgroundPortrait = new Sprite(subElement, "", "", false, 1f);
				}
				else if (subElementId == "hireable")
				{
					hireableCharacters.Add(new FactionPrefab.HireableCharacter(subElement));
				}
				else if (subElementId == "mission" || subElementId == "automaticmission")
				{
					automaticMissions.Add(new FactionPrefab.AutomaticMission(subElement, this.Identifier.ToString()));
				}
			}
			this.HireableCharacters = hireableCharacters.ToImmutableArray<FactionPrefab.HireableCharacter>();
			this.AutomaticMissions = automaticMissions.ToImmutableArray<FactionPrefab.AutomaticMission>();
		}

		// Token: 0x06003C93 RID: 15507 RVA: 0x0022B6C4 File Offset: 0x002298C4
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003C94 RID: 15508 RVA: 0x0022B713 File Offset: 0x00229913
		public override void Dispose()
		{
			Sprite icon = this.Icon;
			if (icon != null)
			{
				icon.Remove();
			}
			this.Icon = null;
		}

		// Token: 0x04001F1C RID: 7964
		public static readonly PrefabCollection<FactionPrefab> Prefabs = new PrefabCollection<FactionPrefab>();

		// Token: 0x04001F21 RID: 7969
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<FactionPrefab.HireableCharacter> HireableCharacters;

		// Token: 0x04001F22 RID: 7970
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<FactionPrefab.AutomaticMission> AutomaticMissions;

		// Token: 0x02000F76 RID: 3958
		[NullableContext(0)]
		public class HireableCharacter
		{
			// Token: 0x06008929 RID: 35113 RVA: 0x003A786C File Offset: 0x003A5A6C
			[NullableContext(1)]
			public HireableCharacter(ContentXElement element)
			{
				this.NPCSetIdentifier = element.GetAttributeIdentifier("from", element.GetAttributeIdentifier("npcsetidentifier", Identifier.Empty));
				this.NPCIdentifier = element.GetAttributeIdentifier("identifier", element.GetAttributeIdentifier("npcidentifier", Identifier.Empty));
				this.MinReputation = element.GetAttributeFloat("minreputation", 0f);
			}

			// Token: 0x040055B1 RID: 21937
			public readonly Identifier NPCSetIdentifier;

			// Token: 0x040055B2 RID: 21938
			public readonly Identifier NPCIdentifier;

			// Token: 0x040055B3 RID: 21939
			public readonly float MinReputation;
		}

		// Token: 0x02000F77 RID: 3959
		[NullableContext(0)]
		public class AutomaticMission
		{
			// Token: 0x0600892A RID: 35114 RVA: 0x003A78D8 File Offset: 0x003A5AD8
			[NullableContext(1)]
			public AutomaticMission(ContentXElement element, string parentDebugName)
			{
				this.MissionTag = element.GetAttributeIdentifier("missiontag", Identifier.Empty);
				string key = "leveltype";
				LevelData.LevelType levelType = LevelData.LevelType.LocationConnection;
				this.LevelType = element.GetAttributeEnum<LevelData.LevelType>(key, levelType);
				this.MinReputation = element.GetAttributeFloat("minreputation", 0f);
				this.MaxReputation = element.GetAttributeFloat("maxreputation", 0f);
				if (this.MinReputation > this.MaxReputation)
				{
					DebugConsole.ThrowError("Error in faction prefab \"" + parentDebugName + "\": MinReputation cannot be larger than MaxReputation.", null, null, false, false);
				}
				float probability = element.GetAttributeFloat("probability", 0f);
				this.MinProbability = element.GetAttributeFloat("minprobability", probability);
				this.MaxProbability = element.GetAttributeFloat("maxprobability", probability);
				this.MaxDistanceFromFactionOutpost = element.GetAttributeInt("MaxDistanceFromFactionOutpost", int.MaxValue);
				this.DisallowBetweenOtherFactionOutposts = element.GetAttributeBool("DisallowBetweenOtherFactionOutposts", false);
			}

			// Token: 0x040055B4 RID: 21940
			public readonly Identifier MissionTag;

			// Token: 0x040055B5 RID: 21941
			public readonly LevelData.LevelType LevelType;

			// Token: 0x040055B6 RID: 21942
			public readonly float MinReputation;

			// Token: 0x040055B7 RID: 21943
			public readonly float MaxReputation;

			// Token: 0x040055B8 RID: 21944
			public readonly float MinProbability;

			// Token: 0x040055B9 RID: 21945
			public readonly float MaxProbability;

			// Token: 0x040055BA RID: 21946
			public readonly int MaxDistanceFromFactionOutpost;

			// Token: 0x040055BB RID: 21947
			public readonly bool DisallowBetweenOtherFactionOutposts;
		}
	}
}
