using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020001CA RID: 458
	internal sealed class MissionPrefab : PrefabWithUintIdentifier, IImplementsVariants<MissionPrefab>
	{
		// Token: 0x17000993 RID: 2451
		// (get) Token: 0x06002195 RID: 8597 RVA: 0x000E1959 File Offset: 0x000DFB59
		// (set) Token: 0x06002196 RID: 8598 RVA: 0x000E1961 File Offset: 0x000DFB61
		public Identifier Type { get; private set; }

		// Token: 0x17000994 RID: 2452
		// (get) Token: 0x06002197 RID: 8599 RVA: 0x000E196A File Offset: 0x000DFB6A
		// (set) Token: 0x06002198 RID: 8600 RVA: 0x000E1972 File Offset: 0x000DFB72
		public Type MissionClass { get; private set; }

		// Token: 0x17000995 RID: 2453
		// (get) Token: 0x06002199 RID: 8601 RVA: 0x000E197B File Offset: 0x000DFB7B
		// (set) Token: 0x0600219A RID: 8602 RVA: 0x000E1983 File Offset: 0x000DFB83
		public bool CampaignOnly { get; private set; }

		// Token: 0x17000996 RID: 2454
		// (get) Token: 0x0600219B RID: 8603 RVA: 0x000E198C File Offset: 0x000DFB8C
		// (set) Token: 0x0600219C RID: 8604 RVA: 0x000E1994 File Offset: 0x000DFB94
		public bool MultiplayerOnly { get; private set; }

		// Token: 0x17000997 RID: 2455
		// (get) Token: 0x0600219D RID: 8605 RVA: 0x000E199D File Offset: 0x000DFB9D
		// (set) Token: 0x0600219E RID: 8606 RVA: 0x000E19A5 File Offset: 0x000DFBA5
		public bool SingleplayerOnly { get; private set; }

		// Token: 0x17000998 RID: 2456
		// (get) Token: 0x0600219F RID: 8607 RVA: 0x000E19AE File Offset: 0x000DFBAE
		// (set) Token: 0x060021A0 RID: 8608 RVA: 0x000E19B6 File Offset: 0x000DFBB6
		public Identifier TextIdentifier { get; private set; }

		// Token: 0x17000999 RID: 2457
		// (get) Token: 0x060021A1 RID: 8609 RVA: 0x000E19BF File Offset: 0x000DFBBF
		// (set) Token: 0x060021A2 RID: 8610 RVA: 0x000E19C7 File Offset: 0x000DFBC7
		public ImmutableHashSet<Identifier> Tags { get; private set; }

		// Token: 0x1700099A RID: 2458
		// (get) Token: 0x060021A3 RID: 8611 RVA: 0x000E19D0 File Offset: 0x000DFBD0
		// (set) Token: 0x060021A4 RID: 8612 RVA: 0x000E19D8 File Offset: 0x000DFBD8
		public LocalizedString Name { get; private set; }

		// Token: 0x1700099B RID: 2459
		// (get) Token: 0x060021A5 RID: 8613 RVA: 0x000E19E1 File Offset: 0x000DFBE1
		// (set) Token: 0x060021A6 RID: 8614 RVA: 0x000E19E9 File Offset: 0x000DFBE9
		public LocalizedString Description { get; private set; }

		// Token: 0x1700099C RID: 2460
		// (get) Token: 0x060021A7 RID: 8615 RVA: 0x000E19F2 File Offset: 0x000DFBF2
		// (set) Token: 0x060021A8 RID: 8616 RVA: 0x000E19FA File Offset: 0x000DFBFA
		public LocalizedString SuccessMessage { get; private set; }

		// Token: 0x1700099D RID: 2461
		// (get) Token: 0x060021A9 RID: 8617 RVA: 0x000E1A03 File Offset: 0x000DFC03
		// (set) Token: 0x060021AA RID: 8618 RVA: 0x000E1A0B File Offset: 0x000DFC0B
		public LocalizedString FailureMessage { get; private set; }

		// Token: 0x1700099E RID: 2462
		// (get) Token: 0x060021AB RID: 8619 RVA: 0x000E1A14 File Offset: 0x000DFC14
		// (set) Token: 0x060021AC RID: 8620 RVA: 0x000E1A1C File Offset: 0x000DFC1C
		public LocalizedString SonarLabel { get; private set; }

		// Token: 0x1700099F RID: 2463
		// (get) Token: 0x060021AD RID: 8621 RVA: 0x000E1A25 File Offset: 0x000DFC25
		// (set) Token: 0x060021AE RID: 8622 RVA: 0x000E1A2D File Offset: 0x000DFC2D
		public Identifier SonarIconIdentifier { get; private set; }

		// Token: 0x170009A0 RID: 2464
		// (get) Token: 0x060021AF RID: 8623 RVA: 0x000E1A36 File Offset: 0x000DFC36
		// (set) Token: 0x060021B0 RID: 8624 RVA: 0x000E1A3E File Offset: 0x000DFC3E
		public Identifier AchievementIdentifier { get; private set; }

		// Token: 0x170009A1 RID: 2465
		// (get) Token: 0x060021B1 RID: 8625 RVA: 0x000E1A47 File Offset: 0x000DFC47
		// (set) Token: 0x060021B2 RID: 8626 RVA: 0x000E1A4F File Offset: 0x000DFC4F
		public ImmutableList<MissionPrefab.ReputationReward> ReputationRewards { get; private set; }

		// Token: 0x170009A2 RID: 2466
		// (get) Token: 0x060021B3 RID: 8627 RVA: 0x000E1A58 File Offset: 0x000DFC58
		// (set) Token: 0x060021B4 RID: 8628 RVA: 0x000E1A60 File Offset: 0x000DFC60
		public int Commonness { get; private set; }

		// Token: 0x170009A3 RID: 2467
		// (get) Token: 0x060021B5 RID: 8629 RVA: 0x000E1A69 File Offset: 0x000DFC69
		// (set) Token: 0x060021B6 RID: 8630 RVA: 0x000E1A71 File Offset: 0x000DFC71
		public int? Difficulty { get; private set; }

		// Token: 0x170009A4 RID: 2468
		// (get) Token: 0x060021B7 RID: 8631 RVA: 0x000E1A7A File Offset: 0x000DFC7A
		// (set) Token: 0x060021B8 RID: 8632 RVA: 0x000E1A82 File Offset: 0x000DFC82
		public int MinLevelDifficulty { get; private set; }

		// Token: 0x170009A5 RID: 2469
		// (get) Token: 0x060021B9 RID: 8633 RVA: 0x000E1A8B File Offset: 0x000DFC8B
		// (set) Token: 0x060021BA RID: 8634 RVA: 0x000E1A93 File Offset: 0x000DFC93
		public int MaxLevelDifficulty { get; private set; } = 100;

		// Token: 0x170009A6 RID: 2470
		// (get) Token: 0x060021BB RID: 8635 RVA: 0x000E1A9C File Offset: 0x000DFC9C
		// (set) Token: 0x060021BC RID: 8636 RVA: 0x000E1AA4 File Offset: 0x000DFCA4
		public int Reward { get; private set; }

		// Token: 0x170009A7 RID: 2471
		// (get) Token: 0x060021BD RID: 8637 RVA: 0x000E1AAD File Offset: 0x000DFCAD
		// (set) Token: 0x060021BE RID: 8638 RVA: 0x000E1AB5 File Offset: 0x000DFCB5
		public float ExperienceMultiplier { get; private set; }

		// Token: 0x170009A8 RID: 2472
		// (get) Token: 0x060021BF RID: 8639 RVA: 0x000E1ABE File Offset: 0x000DFCBE
		// (set) Token: 0x060021C0 RID: 8640 RVA: 0x000E1AC6 File Offset: 0x000DFCC6
		public ImmutableArray<LocalizedString> Headers { get; private set; }

		// Token: 0x170009A9 RID: 2473
		// (get) Token: 0x060021C1 RID: 8641 RVA: 0x000E1ACF File Offset: 0x000DFCCF
		// (set) Token: 0x060021C2 RID: 8642 RVA: 0x000E1AD7 File Offset: 0x000DFCD7
		public ImmutableArray<LocalizedString> Messages { get; private set; }

		// Token: 0x170009AA RID: 2474
		// (get) Token: 0x060021C3 RID: 8643 RVA: 0x000E1AE0 File Offset: 0x000DFCE0
		// (set) Token: 0x060021C4 RID: 8644 RVA: 0x000E1AE8 File Offset: 0x000DFCE8
		public bool AllowRetry { get; private set; }

		// Token: 0x170009AB RID: 2475
		// (get) Token: 0x060021C5 RID: 8645 RVA: 0x000E1AF1 File Offset: 0x000DFCF1
		// (set) Token: 0x060021C6 RID: 8646 RVA: 0x000E1AF9 File Offset: 0x000DFCF9
		public bool ShowSonarLabels { get; private set; }

		// Token: 0x170009AC RID: 2476
		// (get) Token: 0x060021C7 RID: 8647 RVA: 0x000E1B02 File Offset: 0x000DFD02
		// (set) Token: 0x060021C8 RID: 8648 RVA: 0x000E1B0A File Offset: 0x000DFD0A
		public bool ShowInMenus { get; private set; }

		// Token: 0x170009AD RID: 2477
		// (get) Token: 0x060021C9 RID: 8649 RVA: 0x000E1B13 File Offset: 0x000DFD13
		// (set) Token: 0x060021CA RID: 8650 RVA: 0x000E1B1B File Offset: 0x000DFD1B
		public bool ShowStartMessage { get; private set; }

		// Token: 0x170009AE RID: 2478
		// (get) Token: 0x060021CB RID: 8651 RVA: 0x000E1B24 File Offset: 0x000DFD24
		// (set) Token: 0x060021CC RID: 8652 RVA: 0x000E1B2C File Offset: 0x000DFD2C
		public bool IsSideObjective { get; private set; }

		// Token: 0x170009AF RID: 2479
		// (get) Token: 0x060021CD RID: 8653 RVA: 0x000E1B35 File Offset: 0x000DFD35
		// (set) Token: 0x060021CE RID: 8654 RVA: 0x000E1B3D File Offset: 0x000DFD3D
		public bool AllowOtherMissionsInLevel { get; private set; }

		// Token: 0x170009B0 RID: 2480
		// (get) Token: 0x060021CF RID: 8655 RVA: 0x000E1B46 File Offset: 0x000DFD46
		// (set) Token: 0x060021D0 RID: 8656 RVA: 0x000E1B4E File Offset: 0x000DFD4E
		public bool RequireWreck { get; private set; }

		// Token: 0x170009B1 RID: 2481
		// (get) Token: 0x060021D1 RID: 8657 RVA: 0x000E1B57 File Offset: 0x000DFD57
		// (set) Token: 0x060021D2 RID: 8658 RVA: 0x000E1B5F File Offset: 0x000DFD5F
		public bool RequireRuin { get; private set; }

		// Token: 0x170009B2 RID: 2482
		// (get) Token: 0x060021D3 RID: 8659 RVA: 0x000E1B68 File Offset: 0x000DFD68
		// (set) Token: 0x060021D4 RID: 8660 RVA: 0x000E1B70 File Offset: 0x000DFD70
		public bool RequireBeaconStation { get; private set; }

		// Token: 0x170009B3 RID: 2483
		// (get) Token: 0x060021D5 RID: 8661 RVA: 0x000E1B79 File Offset: 0x000DFD79
		// (set) Token: 0x060021D6 RID: 8662 RVA: 0x000E1B81 File Offset: 0x000DFD81
		public bool RequireThalamusWreck { get; private set; }

		// Token: 0x170009B4 RID: 2484
		// (get) Token: 0x060021D7 RID: 8663 RVA: 0x000E1B8A File Offset: 0x000DFD8A
		// (set) Token: 0x060021D8 RID: 8664 RVA: 0x000E1B92 File Offset: 0x000DFD92
		public bool SpawnBeaconStationInMiddle { get; private set; }

		// Token: 0x170009B5 RID: 2485
		// (get) Token: 0x060021D9 RID: 8665 RVA: 0x000E1B9B File Offset: 0x000DFD9B
		// (set) Token: 0x060021DA RID: 8666 RVA: 0x000E1BA3 File Offset: 0x000DFDA3
		public bool AllowOutpostNPCs { get; private set; }

		// Token: 0x170009B6 RID: 2486
		// (get) Token: 0x060021DB RID: 8667 RVA: 0x000E1BAC File Offset: 0x000DFDAC
		// (set) Token: 0x060021DC RID: 8668 RVA: 0x000E1BB4 File Offset: 0x000DFDB4
		public Identifier ForceOutpostGenerationParameters { get; private set; }

		// Token: 0x170009B7 RID: 2487
		// (get) Token: 0x060021DD RID: 8669 RVA: 0x000E1BBD File Offset: 0x000DFDBD
		// (set) Token: 0x060021DE RID: 8670 RVA: 0x000E1BC5 File Offset: 0x000DFDC5
		public RespawnMode? ForceRespawnMode { get; private set; }

		// Token: 0x170009B8 RID: 2488
		// (get) Token: 0x060021DF RID: 8671 RVA: 0x000E1BCE File Offset: 0x000DFDCE
		// (set) Token: 0x060021E0 RID: 8672 RVA: 0x000E1BD6 File Offset: 0x000DFDD6
		public Identifier AllowOutpostSelectionFromTag { get; private set; }

		// Token: 0x170009B9 RID: 2489
		// (get) Token: 0x060021E1 RID: 8673 RVA: 0x000E1BDF File Offset: 0x000DFDDF
		// (set) Token: 0x060021E2 RID: 8674 RVA: 0x000E1BE7 File Offset: 0x000DFDE7
		public bool LoadSubmarines { get; private set; } = true;

		// Token: 0x170009BA RID: 2490
		// (get) Token: 0x060021E3 RID: 8675 RVA: 0x000E1BF0 File Offset: 0x000DFDF0
		// (set) Token: 0x060021E4 RID: 8676 RVA: 0x000E1BF8 File Offset: 0x000DFDF8
		public bool BlockLocationTypeChanges { get; private set; }

		// Token: 0x170009BB RID: 2491
		// (get) Token: 0x060021E5 RID: 8677 RVA: 0x000E1C01 File Offset: 0x000DFE01
		// (set) Token: 0x060021E6 RID: 8678 RVA: 0x000E1C09 File Offset: 0x000DFE09
		public bool ShowProgressBar { get; private set; }

		// Token: 0x170009BC RID: 2492
		// (get) Token: 0x060021E7 RID: 8679 RVA: 0x000E1C12 File Offset: 0x000DFE12
		// (set) Token: 0x060021E8 RID: 8680 RVA: 0x000E1C1A File Offset: 0x000DFE1A
		public bool ShowProgressInNumbers { get; private set; }

		// Token: 0x170009BD RID: 2493
		// (get) Token: 0x060021E9 RID: 8681 RVA: 0x000E1C23 File Offset: 0x000DFE23
		// (set) Token: 0x060021EA RID: 8682 RVA: 0x000E1C2B File Offset: 0x000DFE2B
		public int MaxProgressState { get; private set; }

		// Token: 0x170009BE RID: 2494
		// (get) Token: 0x060021EB RID: 8683 RVA: 0x000E1C34 File Offset: 0x000DFE34
		// (set) Token: 0x060021EC RID: 8684 RVA: 0x000E1C3C File Offset: 0x000DFE3C
		public LocalizedString ProgressBarLabel { get; private set; }

		// Token: 0x170009BF RID: 2495
		// (get) Token: 0x060021ED RID: 8685 RVA: 0x000E1C45 File Offset: 0x000DFE45
		// (set) Token: 0x060021EE RID: 8686 RVA: 0x000E1C4D File Offset: 0x000DFE4D
		[TupleElementNames(new string[]
		{
			"from",
			"to"
		})]
		public List<ValueTuple<Identifier, Identifier>> AllowedConnectionTypes { [return: TupleElementNames(new string[]
		{
			"from",
			"to"
		})] get; [param: TupleElementNames(new string[]
		{
			"from",
			"to"
		})] private set; }

		// Token: 0x170009C0 RID: 2496
		// (get) Token: 0x060021EF RID: 8687 RVA: 0x000E1C56 File Offset: 0x000DFE56
		// (set) Token: 0x060021F0 RID: 8688 RVA: 0x000E1C5E File Offset: 0x000DFE5E
		public Identifier RequiredLocationFaction { get; private set; }

		// Token: 0x170009C1 RID: 2497
		// (get) Token: 0x060021F1 RID: 8689 RVA: 0x000E1C67 File Offset: 0x000DFE67
		// (set) Token: 0x060021F2 RID: 8690 RVA: 0x000E1C6F File Offset: 0x000DFE6F
		public List<string> UnhideEntitySubCategories { get; private set; }

		// Token: 0x170009C2 RID: 2498
		// (get) Token: 0x060021F3 RID: 8691 RVA: 0x000E1C78 File Offset: 0x000DFE78
		// (set) Token: 0x060021F4 RID: 8692 RVA: 0x000E1C80 File Offset: 0x000DFE80
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x170009C3 RID: 2499
		// (get) Token: 0x060021F5 RID: 8693 RVA: 0x000E1C89 File Offset: 0x000DFE89
		public Identifier VariantOf { get; }

		// Token: 0x170009C4 RID: 2500
		// (get) Token: 0x060021F6 RID: 8694 RVA: 0x000E1C91 File Offset: 0x000DFE91
		// (set) Token: 0x060021F7 RID: 8695 RVA: 0x000E1C99 File Offset: 0x000DFE99
		public MissionPrefab ParentPrefab { get; set; }

		// Token: 0x060021F8 RID: 8696 RVA: 0x000E1CA4 File Offset: 0x000DFEA4
		public MissionPrefab(ContentXElement element, MissionsFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.originalElement = element;
			this.ConfigElement = element;
			this.VariantOf = element.VariantOf();
			if (!this.VariantOf.IsEmpty)
			{
				return;
			}
			this.ParseConfigElement(null);
		}

		// Token: 0x060021F9 RID: 8697 RVA: 0x000E1D2C File Offset: 0x000DFF2C
		public void InheritFrom(MissionPrefab parent)
		{
			this.ConfigElement = this.originalElement.CreateVariantXML(parent.ConfigElement, null);
			this.ParseConfigElement(parent);
		}

		// Token: 0x060021FA RID: 8698 RVA: 0x000E1D50 File Offset: 0x000DFF50
		private void ParseConfigElement(MissionPrefab variantOf = null)
		{
			this.TextIdentifier = this.ConfigElement.GetAttributeIdentifier("textidentifier", this.Identifier);
			this.Tags = ImmutableHashSet.Create<Identifier>(new ReadOnlySpan<Identifier>(this.ConfigElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToArray<Identifier>()));
			this.Name = this.<ParseConfigElement>g__GetText|215_0(this.ConfigElement.GetAttributeString("name", ""), "MissionName");
			this.Description = this.<ParseConfigElement>g__GetText|215_0(this.ConfigElement.GetAttributeString("description", ""), "MissionDescription");
			this.Reward = this.ConfigElement.GetAttributeInt("Reward", 1);
			this.ExperienceMultiplier = this.ConfigElement.GetAttributeFloat("ExperienceMultiplier", 1f);
			this.AllowRetry = this.ConfigElement.GetAttributeBool("AllowRetry", false);
			this.ShowSonarLabels = this.ConfigElement.GetAttributeBool("ShowSonarLabels", true);
			this.ShowInMenus = this.ConfigElement.GetAttributeBool("ShowInMenus", true);
			this.ShowStartMessage = this.ConfigElement.GetAttributeBool("ShowStartMessage", true);
			this.IsSideObjective = this.ConfigElement.GetAttributeBool("sideobjective", false);
			this.RequireWreck = this.ConfigElement.GetAttributeBool("RequireWreck", false);
			this.RequireThalamusWreck = this.ConfigElement.GetAttributeBool("RequireThalamusWreck", false);
			this.RequireRuin = this.ConfigElement.GetAttributeBool("RequireRuin", false);
			this.RequireBeaconStation = this.ConfigElement.GetAttributeBool("RequireBeaconStation", false);
			this.SpawnBeaconStationInMiddle = this.ConfigElement.GetAttributeBool("SpawnBeaconStationInMiddle", false);
			this.RequireWreck |= this.RequireThalamusWreck;
			this.LoadSubmarines = this.ConfigElement.GetAttributeBool("LoadSubmarines", true);
			this.BlockLocationTypeChanges = this.ConfigElement.GetAttributeBool("BlockLocationTypeChanges", false);
			this.RequiredLocationFaction = this.ConfigElement.GetAttributeIdentifier("RequiredLocationFaction", Identifier.Empty);
			this.Commonness = this.ConfigElement.GetAttributeInt("Commonness", 1);
			this.AllowOtherMissionsInLevel = this.ConfigElement.GetAttributeBool("AllowOtherMissionsInLevel", true);
			if (this.ConfigElement.GetAttribute("difficulty") != null)
			{
				int difficulty = this.ConfigElement.GetAttributeInt("Difficulty", 1);
				this.Difficulty = new int?(Math.Clamp(difficulty, 1, 4));
			}
			this.MinLevelDifficulty = this.ConfigElement.GetAttributeInt("MinLevelDifficulty", this.MinLevelDifficulty);
			this.MaxLevelDifficulty = this.ConfigElement.GetAttributeInt("MaxLevelDifficulty", this.MaxLevelDifficulty);
			this.MinLevelDifficulty = Math.Clamp(this.MinLevelDifficulty, 0, Math.Min(this.MaxLevelDifficulty, 100));
			this.MaxLevelDifficulty = Math.Clamp(this.MaxLevelDifficulty, Math.Max(this.MinLevelDifficulty, 0), 100);
			this.AllowOutpostNPCs = this.ConfigElement.GetAttributeBool("AllowOutpostNPCs", true);
			this.ForceOutpostGenerationParameters = this.ConfigElement.GetAttributeIdentifier("ForceOutpostGenerationParameters", Identifier.Empty);
			this.AllowOutpostSelectionFromTag = this.ConfigElement.GetAttributeIdentifier("AllowOutpostSelectionFromTag", Identifier.Empty);
			if (this.ConfigElement.GetAttribute("ForceRespawnMode") != null)
			{
				ContentXElement configElement = this.ConfigElement;
				string key = "ForceRespawnMode";
				RespawnMode respawnMode = RespawnMode.MidRound;
				this.ForceRespawnMode = new RespawnMode?(configElement.GetAttributeEnum<RespawnMode>(key, respawnMode));
			}
			this.ShowProgressBar = this.ConfigElement.GetAttributeBool("ShowProgressBar", false);
			this.ShowProgressInNumbers = this.ConfigElement.GetAttributeBool("ShowProgressInNumbers", false);
			this.MaxProgressState = this.ConfigElement.GetAttributeInt("MaxProgressState", 1);
			string progressBarLabel = this.ConfigElement.GetAttributeString("ProgressBarLabel", "");
			this.ProgressBarLabel = TextManager.Get(progressBarLabel).Fallback(progressBarLabel, true);
			string successMessageTag = this.ConfigElement.GetAttributeString("successmessage", "");
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("MissionSuccess.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TextIdentifier);
			this.SuccessMessage = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			if (!string.IsNullOrEmpty(successMessageTag))
			{
				this.SuccessMessage = this.SuccessMessage.Fallback(TextManager.Get(successMessageTag), true).Fallback(successMessageTag, true);
			}
			this.SuccessMessage = this.SuccessMessage.Fallback(TextManager.Get("missioncompleted"), true);
			string failureMessageTag = this.ConfigElement.GetAttributeString("failuremessage", "");
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("MissionFailure.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.TextIdentifier);
			this.FailureMessage = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			if (!string.IsNullOrEmpty(failureMessageTag))
			{
				this.FailureMessage = this.FailureMessage.Fallback(TextManager.Get(failureMessageTag), true).Fallback(failureMessageTag, true);
			}
			this.FailureMessage = this.FailureMessage.Fallback(TextManager.Get("missionfailed"), true);
			string sonarLabelTag = this.ConfigElement.GetAttributeString("sonarlabel", "");
			LocalizedString localizedString = TextManager.Get("MissionSonarLabel." + sonarLabelTag).Fallback(TextManager.Get(sonarLabelTag), true);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("MissionSonarLabel.");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.TextIdentifier);
			this.SonarLabel = localizedString.Fallback(TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()), true);
			if (!string.IsNullOrEmpty(sonarLabelTag))
			{
				this.SonarLabel = this.SonarLabel.Fallback(sonarLabelTag, true);
			}
			this.SonarIconIdentifier = this.ConfigElement.GetAttributeIdentifier("sonaricon", "");
			this.CampaignOnly = this.ConfigElement.GetAttributeBool("CampaignOnly", false);
			this.MultiplayerOnly = this.ConfigElement.GetAttributeBool("MultiplayerOnly", false);
			this.SingleplayerOnly = this.ConfigElement.GetAttributeBool("SingleplayerOnly", false);
			this.AchievementIdentifier = this.ConfigElement.GetAttributeIdentifier("achievementidentifier", "");
			this.UnhideEntitySubCategories = this.ConfigElement.GetAttributeStringArray("unhideentitysubcategories", Array.Empty<string>(), false).ToList<string>();
			List<LocalizedString> headers = new List<LocalizedString>();
			List<LocalizedString> messages = new List<LocalizedString>();
			this.AllowedConnectionTypes = new List<ValueTuple<Identifier, Identifier>>();
			for (int i = 0; i < 100; i++)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("MissionHeader");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler4.AppendLiteral(".");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.TextIdentifier);
				LocalizedString header = TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(15, 2);
				defaultInterpolatedStringHandler5.AppendLiteral("MissionMessage");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler5.AppendLiteral(".");
				defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(this.TextIdentifier);
				LocalizedString message = TextManager.Get(defaultInterpolatedStringHandler5.ToStringAndClear());
				if (!message.IsNullOrEmpty())
				{
					headers.Add(header);
					messages.Add(message);
				}
			}
			List<MissionPrefab.ReputationReward> reputationRewards = new List<MissionPrefab.ReputationReward>();
			int messageIndex = 0;
			foreach (ContentXElement subElement in this.ConfigElement.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 7:
					{
						if (!(text == "message"))
						{
							continue;
						}
						if (messageIndex >= headers.Count)
						{
							headers.Add(string.Empty);
							messages.Add(string.Empty);
						}
						List<LocalizedString> list = headers;
						int index = messageIndex;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(14, 2);
						defaultInterpolatedStringHandler6.AppendLiteral("MissionHeader");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(messageIndex);
						defaultInterpolatedStringHandler6.AppendLiteral(".");
						defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(this.TextIdentifier);
						list[index] = TextManager.Get(defaultInterpolatedStringHandler6.ToStringAndClear()).Fallback(TextManager.Get(subElement.GetAttributeString("header", "")), true).Fallback(subElement.GetAttributeString("header", ""), true);
						List<LocalizedString> list2 = messages;
						int index2 = messageIndex;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(15, 2);
						defaultInterpolatedStringHandler7.AppendLiteral("MissionMessage");
						defaultInterpolatedStringHandler7.AppendFormatted<int>(messageIndex);
						defaultInterpolatedStringHandler7.AppendLiteral(".");
						defaultInterpolatedStringHandler7.AppendFormatted<Identifier>(this.TextIdentifier);
						list2[index2] = TextManager.Get(defaultInterpolatedStringHandler7.ToStringAndClear()).Fallback(TextManager.Get(subElement.GetAttributeString("text", "")), true).Fallback(subElement.GetAttributeString("text", ""), true);
						messageIndex++;
						continue;
					}
					case 8:
					{
						if (!(text == "metadata"))
						{
							continue;
						}
						Identifier identifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						string stringValue = subElement.GetAttributeString("value", string.Empty);
						if (!string.IsNullOrWhiteSpace(stringValue) && !identifier.IsEmpty)
						{
							object value = SetDataAction.ConvertXMLValue(stringValue);
							SetDataAction.OperationType operation = SetDataAction.OperationType.Set;
							string operatingString = subElement.GetAttributeString("operation", string.Empty);
							if (!string.IsNullOrWhiteSpace(operatingString))
							{
								operation = (SetDataAction.OperationType)Enum.Parse(typeof(SetDataAction.OperationType), operatingString);
							}
							this.DataRewards.Add(new ValueTuple<Identifier, object, SetDataAction.OperationType>(identifier, value, operation));
							continue;
						}
						continue;
					}
					case 9:
					case 11:
					case 13:
					case 15:
					case 17:
						continue;
					case 10:
						if (!(text == "reputation"))
						{
							continue;
						}
						goto IL_A23;
					case 12:
					{
						char c = text[0];
						if (c != 'l')
						{
							if (c != 't')
							{
								continue;
							}
							if (!(text == "triggerevent"))
							{
								continue;
							}
							this.TriggerEvents.Add(new MissionPrefab.TriggerEvent(subElement));
							continue;
						}
						else if (!(text == "locationtype"))
						{
							continue;
						}
						break;
					}
					case 14:
						if (!(text == "connectiontype"))
						{
							continue;
						}
						break;
					case 16:
						if (!(text == "reputationreward"))
						{
							continue;
						}
						goto IL_A23;
					case 18:
						if (!(text == "locationtypechange"))
						{
							continue;
						}
						this.LocationTypeChangeOnCompleted = new LocationTypeChange(subElement.GetAttributeIdentifier("from", ""), subElement, false, 1f);
						continue;
					default:
						continue;
					}
					if (subElement.GetAttribute("identifier") != null)
					{
						this.AllowedLocationTypes.Add(subElement.GetAttributeIdentifier("identifier", ""));
						continue;
					}
					this.AllowedConnectionTypes.Add(new ValueTuple<Identifier, Identifier>(subElement.GetAttributeIdentifier("from", ""), subElement.GetAttributeIdentifier("to", "")));
					continue;
					IL_A23:
					reputationRewards.Add(new MissionPrefab.ReputationReward(subElement));
				}
			}
			this.Headers = ImmutableCollectionsMarshal.AsImmutableArray<LocalizedString>(headers.ToArray());
			this.Messages = ImmutableCollectionsMarshal.AsImmutableArray<LocalizedString>(messages.ToArray());
			this.ReputationRewards = ImmutableList.Create<MissionPrefab.ReputationReward>(new ReadOnlySpan<MissionPrefab.ReputationReward>(reputationRewards.ToArray()));
			this.MissionClass = this.FindMissionClass(this.ConfigElement);
			this.Type = this.ConfigElement.GetAttributeIdentifier("Type", Identifier.Empty);
			if (!this.LoadSubmarines && this.MissionClass != typeof(CombatMission))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(165, 1);
				defaultInterpolatedStringHandler8.AppendLiteral("Potential error in mission ");
				defaultInterpolatedStringHandler8.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler8.AppendLiteral(": Disabling submarines is only intended for combat missions taking place in an outpost, and may lead to issues in other types of missions.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler8.ToStringAndClear(), this.ConfigElement.ContentPackage);
			}
			this.constructor = this.FindMissionConstructor(this.ConfigElement, this.MissionClass);
			if (this.constructor == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(53, 1);
				defaultInterpolatedStringHandler9.AppendLiteral("Failed to find a constructor for the mission type \"");
				defaultInterpolatedStringHandler9.AppendFormatted<Identifier>(this.Type);
				defaultInterpolatedStringHandler9.AppendLiteral("\"!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler9.ToStringAndClear(), null, this.ConfigElement.ContentPackage, false, false);
			}
		}

		// Token: 0x060021FB RID: 8699 RVA: 0x000E29B4 File Offset: 0x000E0BB4
		private Type FindMissionClass(ContentXElement element)
		{
			Type type = MissionPrefab.<FindMissionClass>g__TryGetClass|216_0(element.NameAsIdentifier().RemoveFromEnd("Mission"));
			if (type == null)
			{
				Identifier typeNameLegacy = element.GetAttributeIdentifier("type", Identifier.Empty).ToIdentifier<Identifier>();
				if (typeNameLegacy == "OutpostDestroy" || typeNameLegacy == "OutpostRescue")
				{
					typeNameLegacy = "AbandonedOutpost".ToIdentifier();
				}
				else if (typeNameLegacy == "clearalienruins")
				{
					typeNameLegacy = "EliminateTargets".ToIdentifier();
				}
				type = (MissionPrefab.<FindMissionClass>g__TryGetClass|216_0(typeNameLegacy) ?? MissionPrefab.<FindMissionClass>g__TryGetClass|216_0(typeNameLegacy.AppendIfMissing("Mission")));
				if (type == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to find the mission type \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(typeNameLegacy);
					defaultInterpolatedStringHandler.AppendLiteral("\" for the mission ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					return null;
				}
			}
			return type;
		}

		// Token: 0x060021FC RID: 8700 RVA: 0x000E2AC0 File Offset: 0x000E0CC0
		private ConstructorInfo FindMissionConstructor(ContentXElement element, Type missionClass)
		{
			if (missionClass == null)
			{
				return null;
			}
			if (missionClass != typeof(Mission) && !missionClass.IsSubclassOf(typeof(Mission)))
			{
				return null;
			}
			ConstructorInfo constructor = missionClass.GetConstructor(new Type[]
			{
				typeof(MissionPrefab),
				typeof(Location[]),
				typeof(Submarine)
			});
			if (constructor == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find the constructor of the mission type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(missionClass);
				defaultInterpolatedStringHandler.AppendLiteral("\" for the mission ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				return null;
			}
			return constructor;
		}

		// Token: 0x060021FD RID: 8701 RVA: 0x000E2B8C File Offset: 0x000E0D8C
		public bool IsAllowed(Location from, Location to)
		{
			if (from == to)
			{
				if (!this.RequiredLocationFaction.IsEmpty)
				{
					Faction faction = from.Faction;
					Identifier? identifier;
					Identifier? identifier2;
					if (faction == null)
					{
						identifier = null;
						identifier2 = identifier;
					}
					else
					{
						identifier2 = new Identifier?(faction.Prefab.Identifier);
					}
					identifier = identifier2;
					Identifier? identifier3 = new Identifier?(this.RequiredLocationFaction);
					if (identifier != identifier3)
					{
						return false;
					}
				}
				return this.AllowedLocationTypes.Any((Identifier lt) => lt == "any") || this.AllowedLocationTypes.Any((Identifier lt) => lt == Barotrauma.Tags.AnyOutpost && from.HasOutpost() && from.Type.IsAnyOutpost) || this.AllowedLocationTypes.Any((Identifier lt) => lt == from.Type.Identifier);
			}
			foreach (ValueTuple<Identifier, Identifier> valueTuple in this.AllowedConnectionTypes)
			{
				Identifier fromType = valueTuple.Item1;
				Identifier toType = valueTuple.Item2;
				if ((fromType == "any" || fromType == from.Type.Identifier || (fromType == Barotrauma.Tags.AnyOutpost && from.HasOutpost() && from.Type.IsAnyOutpost && from.Type.Identifier != "abandoned")) && (toType == "any" || toType == to.Type.Identifier || (toType == Barotrauma.Tags.AnyOutpost && to.HasOutpost() && to.Type.IsAnyOutpost && to.Type.Identifier != "abandoned")))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060021FE RID: 8702 RVA: 0x000E2DA0 File Offset: 0x000E0FA0
		public bool IsAllowedDifficulty(float difficulty)
		{
			return difficulty >= (float)this.MinLevelDifficulty && difficulty <= (float)this.MaxLevelDifficulty;
		}

		// Token: 0x060021FF RID: 8703 RVA: 0x000E2DBB File Offset: 0x000E0FBB
		public Mission Instantiate(Location[] locations, Submarine sub)
		{
			ConstructorInfo constructorInfo = this.constructor;
			return ((constructorInfo != null) ? constructorInfo.Invoke(new object[]
			{
				this,
				locations,
				sub
			}) : null) as Mission;
		}

		// Token: 0x06002200 RID: 8704 RVA: 0x000E2DE6 File Offset: 0x000E0FE6
		public override void Dispose()
		{
		}

		// Token: 0x06002201 RID: 8705 RVA: 0x000E2DE8 File Offset: 0x000E0FE8
		public static IEnumerable<Identifier> GetAllMultiplayerSelectableMissionTypes()
		{
			List<Identifier> missionTypes = new List<Identifier>();
			foreach (MissionPrefab missionPrefab in MissionPrefab.Prefabs)
			{
				if ((float)missionPrefab.Commonness > 0f && !missionPrefab.CampaignOnly && !missionPrefab.SingleplayerOnly && !MissionPrefab.HiddenMissionTypes.Contains(missionPrefab.Type) && !missionTypes.Contains(missionPrefab.Type))
				{
					missionTypes.Add(missionPrefab.Type);
				}
			}
			return from t in missionTypes
			orderby t.Value
			select t;
		}

		// Token: 0x06002203 RID: 8707 RVA: 0x000E3088 File Offset: 0x000E1288
		[CompilerGenerated]
		private LocalizedString <ParseConfigElement>g__GetText|215_0(string textTag, string textTagPrefix)
		{
			if (string.IsNullOrEmpty(textTag))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted(textTagPrefix);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TextIdentifier);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			LocalizedString localizedString = TextManager.Get(textTag);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler2.AppendFormatted(textTagPrefix);
			defaultInterpolatedStringHandler2.AppendLiteral(".");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.TextIdentifier);
			return localizedString.Fallback(TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()), true).Fallback(textTag, true);
		}

		// Token: 0x06002204 RID: 8708 RVA: 0x000E3124 File Offset: 0x000E1324
		[CompilerGenerated]
		internal static Type <FindMissionClass>g__TryGetClass|216_0(Identifier typeName)
		{
			Type coOpMissionClass;
			if (MissionPrefab.CoOpMissionClasses.TryGetValue(typeName, out coOpMissionClass))
			{
				return coOpMissionClass;
			}
			Type pvpMissionClass;
			if (MissionPrefab.PvPMissionClasses.TryGetValue(typeName, out pvpMissionClass))
			{
				return pvpMissionClass;
			}
			return null;
		}

		// Token: 0x04001007 RID: 4103
		public static readonly PrefabCollection<MissionPrefab> Prefabs = new PrefabCollection<MissionPrefab>();

		// Token: 0x04001008 RID: 4104
		public static readonly Dictionary<Identifier, Type> CoOpMissionClasses = new Dictionary<Identifier, Type>
		{
			{
				"Salvage".ToIdentifier(),
				typeof(SalvageMission)
			},
			{
				"Monster".ToIdentifier(),
				typeof(MonsterMission)
			},
			{
				"Cargo".ToIdentifier(),
				typeof(CargoMission)
			},
			{
				"Beacon".ToIdentifier(),
				typeof(BeaconMission)
			},
			{
				"Nest".ToIdentifier(),
				typeof(NestMission)
			},
			{
				"Mineral".ToIdentifier(),
				typeof(MineralMission)
			},
			{
				"AbandonedOutpost".ToIdentifier(),
				typeof(AbandonedOutpostMission)
			},
			{
				"Escort".ToIdentifier(),
				typeof(EscortMission)
			},
			{
				"Pirate".ToIdentifier(),
				typeof(PirateMission)
			},
			{
				"GoTo".ToIdentifier(),
				typeof(GoToMission)
			},
			{
				"ScanAlienRuins".ToIdentifier(),
				typeof(ScanMission)
			},
			{
				"EliminateTargets".ToIdentifier(),
				typeof(EliminateTargetsMission)
			},
			{
				"End".ToIdentifier(),
				typeof(EndMission)
			},
			{
				"Custom".ToIdentifier(),
				typeof(CustomMission)
			}
		};

		// Token: 0x04001009 RID: 4105
		public static readonly Dictionary<Identifier, Type> PvPMissionClasses = new Dictionary<Identifier, Type>
		{
			{
				"Combat".ToIdentifier(),
				typeof(CombatMission)
			}
		};

		// Token: 0x0400100A RID: 4106
		public static readonly HashSet<Identifier> HiddenMissionTypes = new HashSet<Identifier>
		{
			"GoTo".ToIdentifier(),
			"End".ToIdentifier()
		};

		// Token: 0x0400100B RID: 4107
		private ConstructorInfo constructor;

		// Token: 0x0400101B RID: 4123
		[TupleElementNames(new string[]
		{
			"Identifier",
			"Value",
			"OperationType"
		})]
		public readonly List<ValueTuple<Identifier, object, SetDataAction.OperationType>> DataRewards = new List<ValueTuple<Identifier, object, SetDataAction.OperationType>>();

		// Token: 0x0400101E RID: 4126
		public const int MinDifficulty = 1;

		// Token: 0x0400101F RID: 4127
		public const int MaxDifficulty = 4;

		// Token: 0x0400103C RID: 4156
		public readonly List<Identifier> AllowedLocationTypes = new List<Identifier>();

		// Token: 0x0400103F RID: 4159
		public readonly List<MissionPrefab.TriggerEvent> TriggerEvents = new List<MissionPrefab.TriggerEvent>();

		// Token: 0x04001040 RID: 4160
		public LocationTypeChange LocationTypeChangeOnCompleted;

		// Token: 0x04001041 RID: 4161
		private readonly ContentXElement originalElement;

		// Token: 0x02000952 RID: 2386
		public class ReputationReward
		{
			// Token: 0x0600594A RID: 22858 RVA: 0x001F8B94 File Offset: 0x001F6D94
			public ReputationReward(XElement element)
			{
				this.FactionIdentifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
				this.Amount = element.GetAttributeFloat("Amount", 0f);
				this.AmountForOpposingFaction = element.GetAttributeFloat("AmountForOpposingFaction", 0f);
			}

			// Token: 0x040032CB RID: 13003
			public readonly Identifier FactionIdentifier;

			// Token: 0x040032CC RID: 13004
			public readonly float Amount;

			// Token: 0x040032CD RID: 13005
			public readonly float AmountForOpposingFaction;
		}

		// Token: 0x02000953 RID: 2387
		public class TriggerEvent
		{
			// Token: 0x17001551 RID: 5457
			// (get) Token: 0x0600594B RID: 22859 RVA: 0x001F8BE9 File Offset: 0x001F6DE9
			// (set) Token: 0x0600594C RID: 22860 RVA: 0x001F8BF1 File Offset: 0x001F6DF1
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			public Identifier EventIdentifier { get; private set; }

			// Token: 0x17001552 RID: 5458
			// (get) Token: 0x0600594D RID: 22861 RVA: 0x001F8BFA File Offset: 0x001F6DFA
			// (set) Token: 0x0600594E RID: 22862 RVA: 0x001F8C02 File Offset: 0x001F6E02
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			public Identifier EventTag { get; private set; }

			// Token: 0x17001553 RID: 5459
			// (get) Token: 0x0600594F RID: 22863 RVA: 0x001F8C0B File Offset: 0x001F6E0B
			// (set) Token: 0x06005950 RID: 22864 RVA: 0x001F8C13 File Offset: 0x001F6E13
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			public int State { get; private set; }

			// Token: 0x17001554 RID: 5460
			// (get) Token: 0x06005951 RID: 22865 RVA: 0x001F8C1C File Offset: 0x001F6E1C
			// (set) Token: 0x06005952 RID: 22866 RVA: 0x001F8C24 File Offset: 0x001F6E24
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			public float Delay { get; private set; }

			// Token: 0x17001555 RID: 5461
			// (get) Token: 0x06005953 RID: 22867 RVA: 0x001F8C2D File Offset: 0x001F6E2D
			// (set) Token: 0x06005954 RID: 22868 RVA: 0x001F8C35 File Offset: 0x001F6E35
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			public bool CampaignOnly { get; private set; }

			// Token: 0x06005955 RID: 22869 RVA: 0x001F8C3E File Offset: 0x001F6E3E
			public TriggerEvent(XElement element)
			{
				SerializableProperty.DeserializeProperties(this, element);
			}
		}
	}
}
