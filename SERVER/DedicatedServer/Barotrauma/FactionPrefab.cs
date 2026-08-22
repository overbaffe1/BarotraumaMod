using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001D6 RID: 470
	[NullableContext(1)]
	[Nullable(0)]
	internal class FactionPrefab : Prefab
	{
		// Token: 0x170009DC RID: 2524
		// (get) Token: 0x06002287 RID: 8839 RVA: 0x000E84C1 File Offset: 0x000E66C1
		public LocalizedString Name { get; }

		// Token: 0x170009DD RID: 2525
		// (get) Token: 0x06002288 RID: 8840 RVA: 0x000E84C9 File Offset: 0x000E66C9
		public LocalizedString Description { get; }

		// Token: 0x170009DE RID: 2526
		// (get) Token: 0x06002289 RID: 8841 RVA: 0x000E84D1 File Offset: 0x000E66D1
		public LocalizedString ShortDescription { get; }

		// Token: 0x170009DF RID: 2527
		// (get) Token: 0x0600228A RID: 8842 RVA: 0x000E84D9 File Offset: 0x000E66D9
		public Identifier OpposingFaction { get; }

		// Token: 0x170009E0 RID: 2528
		// (get) Token: 0x0600228B RID: 8843 RVA: 0x000E84E1 File Offset: 0x000E66E1
		public bool StartOutpost { get; }

		// Token: 0x170009E1 RID: 2529
		// (get) Token: 0x0600228C RID: 8844 RVA: 0x000E84E9 File Offset: 0x000E66E9
		public int MenuOrder { get; }

		// Token: 0x170009E2 RID: 2530
		// (get) Token: 0x0600228D RID: 8845 RVA: 0x000E84F1 File Offset: 0x000E66F1
		public int MinReputation { get; }

		// Token: 0x170009E3 RID: 2531
		// (get) Token: 0x0600228E RID: 8846 RVA: 0x000E84F9 File Offset: 0x000E66F9
		public int MaxReputation { get; }

		// Token: 0x170009E4 RID: 2532
		// (get) Token: 0x0600228F RID: 8847 RVA: 0x000E8501 File Offset: 0x000E6701
		public int InitialReputation { get; }

		// Token: 0x170009E5 RID: 2533
		// (get) Token: 0x06002290 RID: 8848 RVA: 0x000E8509 File Offset: 0x000E6709
		public float ControlledOutpostPercentage { get; }

		// Token: 0x170009E6 RID: 2534
		// (get) Token: 0x06002291 RID: 8849 RVA: 0x000E8511 File Offset: 0x000E6711
		public float SecondaryControlledOutpostPercentage { get; }

		// Token: 0x170009E7 RID: 2535
		// (get) Token: 0x06002292 RID: 8850 RVA: 0x000E8519 File Offset: 0x000E6719
		public Color IconColor { get; }

		// Token: 0x06002293 RID: 8851 RVA: 0x000E8524 File Offset: 0x000E6724
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
				}
				else if (!(subElementId == "iconsmall") && !(subElementId == "portrait"))
				{
					if (subElementId == "hireable")
					{
						hireableCharacters.Add(new FactionPrefab.HireableCharacter(subElement));
					}
					else if (subElementId == "mission" || subElementId == "automaticmission")
					{
						automaticMissions.Add(new FactionPrefab.AutomaticMission(subElement, this.Identifier.ToString()));
					}
				}
			}
			this.HireableCharacters = hireableCharacters.ToImmutableArray<FactionPrefab.HireableCharacter>();
			this.AutomaticMissions = automaticMissions.ToImmutableArray<FactionPrefab.AutomaticMission>();
		}

		// Token: 0x06002294 RID: 8852 RVA: 0x000E8824 File Offset: 0x000E6A24
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06002295 RID: 8853 RVA: 0x000E8873 File Offset: 0x000E6A73
		public override void Dispose()
		{
		}

		// Token: 0x04001083 RID: 4227
		public static readonly PrefabCollection<FactionPrefab> Prefabs = new PrefabCollection<FactionPrefab>();

		// Token: 0x04001088 RID: 4232
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<FactionPrefab.HireableCharacter> HireableCharacters;

		// Token: 0x04001089 RID: 4233
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<FactionPrefab.AutomaticMission> AutomaticMissions;

		// Token: 0x02000986 RID: 2438
		[NullableContext(0)]
		public class HireableCharacter
		{
			// Token: 0x060059EE RID: 23022 RVA: 0x001FB130 File Offset: 0x001F9330
			[NullableContext(1)]
			public HireableCharacter(ContentXElement element)
			{
				this.NPCSetIdentifier = element.GetAttributeIdentifier("from", element.GetAttributeIdentifier("npcsetidentifier", Identifier.Empty));
				this.NPCIdentifier = element.GetAttributeIdentifier("identifier", element.GetAttributeIdentifier("npcidentifier", Identifier.Empty));
				this.MinReputation = element.GetAttributeFloat("minreputation", 0f);
			}

			// Token: 0x0400339B RID: 13211
			public readonly Identifier NPCSetIdentifier;

			// Token: 0x0400339C RID: 13212
			public readonly Identifier NPCIdentifier;

			// Token: 0x0400339D RID: 13213
			public readonly float MinReputation;
		}

		// Token: 0x02000987 RID: 2439
		[NullableContext(0)]
		public class AutomaticMission
		{
			// Token: 0x060059EF RID: 23023 RVA: 0x001FB19C File Offset: 0x001F939C
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

			// Token: 0x0400339E RID: 13214
			public readonly Identifier MissionTag;

			// Token: 0x0400339F RID: 13215
			public readonly LevelData.LevelType LevelType;

			// Token: 0x040033A0 RID: 13216
			public readonly float MinReputation;

			// Token: 0x040033A1 RID: 13217
			public readonly float MaxReputation;

			// Token: 0x040033A2 RID: 13218
			public readonly float MinProbability;

			// Token: 0x040033A3 RID: 13219
			public readonly float MaxProbability;

			// Token: 0x040033A4 RID: 13220
			public readonly int MaxDistanceFromFactionOutpost;

			// Token: 0x040033A5 RID: 13221
			public readonly bool DisallowBetweenOtherFactionOutposts;
		}
	}
}
