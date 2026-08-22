using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020000CE RID: 206
	internal class HumanPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x170006A6 RID: 1702
		// (get) Token: 0x060016E0 RID: 5856 RVA: 0x000C0EB9 File Offset: 0x000BF0B9
		// (set) Token: 0x060016E1 RID: 5857 RVA: 0x000C0EC1 File Offset: 0x000BF0C1
		[Serialize("any", IsPropertySaveable.No, "", "", false)]
		public Identifier Job { get; protected set; }

		// Token: 0x170006A7 RID: 1703
		// (get) Token: 0x060016E2 RID: 5858 RVA: 0x000C0ECA File Offset: 0x000BF0CA
		// (set) Token: 0x060016E3 RID: 5859 RVA: 0x000C0ED2 File Offset: 0x000BF0D2
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float Commonness { get; protected set; }

		// Token: 0x170006A8 RID: 1704
		// (get) Token: 0x060016E4 RID: 5860 RVA: 0x000C0EDB File Offset: 0x000BF0DB
		// (set) Token: 0x060016E5 RID: 5861 RVA: 0x000C0EE3 File Offset: 0x000BF0E3
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float HealthMultiplier { get; protected set; }

		// Token: 0x170006A9 RID: 1705
		// (get) Token: 0x060016E6 RID: 5862 RVA: 0x000C0EEC File Offset: 0x000BF0EC
		// (set) Token: 0x060016E7 RID: 5863 RVA: 0x000C0EF4 File Offset: 0x000BF0F4
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float HealthMultiplierInMultiplayer { get; protected set; }

		// Token: 0x170006AA RID: 1706
		// (get) Token: 0x060016E8 RID: 5864 RVA: 0x000C0EFD File Offset: 0x000BF0FD
		// (set) Token: 0x060016E9 RID: 5865 RVA: 0x000C0F05 File Offset: 0x000BF105
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AimSpeed { get; protected set; }

		// Token: 0x170006AB RID: 1707
		// (get) Token: 0x060016EA RID: 5866 RVA: 0x000C0F0E File Offset: 0x000BF10E
		// (set) Token: 0x060016EB RID: 5867 RVA: 0x000C0F16 File Offset: 0x000BF116
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AimAccuracy { get; protected set; }

		// Token: 0x170006AC RID: 1708
		// (get) Token: 0x060016EC RID: 5868 RVA: 0x000C0F1F File Offset: 0x000BF11F
		// (set) Token: 0x060016ED RID: 5869 RVA: 0x000C0F27 File Offset: 0x000BF127
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SkillMultiplier { get; protected set; }

		// Token: 0x170006AD RID: 1709
		// (get) Token: 0x060016EE RID: 5870 RVA: 0x000C0F30 File Offset: 0x000BF130
		// (set) Token: 0x060016EF RID: 5871 RVA: 0x000C0F38 File Offset: 0x000BF138
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int ExperiencePoints { get; private set; }

		// Token: 0x170006AE RID: 1710
		// (get) Token: 0x060016F0 RID: 5872 RVA: 0x000C0F41 File Offset: 0x000BF141
		// (set) Token: 0x060016F1 RID: 5873 RVA: 0x000C0F49 File Offset: 0x000BF149
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int BaseSalary { get; private set; }

		// Token: 0x170006AF RID: 1711
		// (get) Token: 0x060016F2 RID: 5874 RVA: 0x000C0F52 File Offset: 0x000BF152
		// (set) Token: 0x060016F3 RID: 5875 RVA: 0x000C0F5A File Offset: 0x000BF15A
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SalaryMultiplier { get; private set; }

		// Token: 0x170006B0 RID: 1712
		// (get) Token: 0x060016F4 RID: 5876 RVA: 0x000C0F63 File Offset: 0x000BF163
		// (set) Token: 0x060016F5 RID: 5877 RVA: 0x000C0F78 File Offset: 0x000BF178
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string Tags
		{
			get
			{
				return string.Join<Identifier>(",", this.tags);
			}
			set
			{
				this.tags.Clear();
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] splitTags = value.Split(',', StringSplitOptions.None);
					foreach (string tag in splitTags)
					{
						this.tags.Add(tag.ToIdentifier());
					}
				}
			}
		}

		// Token: 0x170006B1 RID: 1713
		// (get) Token: 0x060016F6 RID: 5878 RVA: 0x000C0FC8 File Offset: 0x000BF1C8
		// (set) Token: 0x060016F7 RID: 5879 RVA: 0x000C0FDC File Offset: 0x000BF1DC
		[Serialize("", IsPropertySaveable.Yes, "What outpost module tags does the NPC prefer to spawn in.", "", false)]
		public string ModuleFlags
		{
			get
			{
				return string.Join<Identifier>(",", this.moduleFlags);
			}
			set
			{
				this.moduleFlags.Clear();
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] splitFlags = value.Split(',', StringSplitOptions.None);
					foreach (string f in splitFlags)
					{
						this.moduleFlags.Add(f.ToIdentifier());
					}
				}
			}
		}

		// Token: 0x170006B2 RID: 1714
		// (get) Token: 0x060016F8 RID: 5880 RVA: 0x000C102C File Offset: 0x000BF22C
		// (set) Token: 0x060016F9 RID: 5881 RVA: 0x000C1040 File Offset: 0x000BF240
		[Serialize("", IsPropertySaveable.Yes, "Tag(s) of the spawnpoints the NPC prefers to spawn at.", "", false)]
		public string SpawnPointTags
		{
			get
			{
				return string.Join<Identifier>(",", this.spawnPointTags);
			}
			set
			{
				this.spawnPointTags.Clear();
				if (!string.IsNullOrWhiteSpace(value))
				{
					string[] splitTags = value.Split(',', StringSplitOptions.None);
					foreach (string tag in splitTags)
					{
						this.spawnPointTags.Add(tag.ToIdentifier());
					}
				}
			}
		}

		// Token: 0x170006B3 RID: 1715
		// (get) Token: 0x060016FA RID: 5882 RVA: 0x000C1090 File Offset: 0x000BF290
		// (set) Token: 0x060016FB RID: 5883 RVA: 0x000C1098 File Offset: 0x000BF298
		[Serialize(false, IsPropertySaveable.No, "If enabled, the NPC will not spawn if the specified spawn point tags can't be found.", "", false)]
		public bool RequireSpawnPointTag { get; protected set; }

		// Token: 0x170006B4 RID: 1716
		// (get) Token: 0x060016FC RID: 5884 RVA: 0x000C10A1 File Offset: 0x000BF2A1
		// (set) Token: 0x060016FD RID: 5885 RVA: 0x000C10A9 File Offset: 0x000BF2A9
		[Serialize(CampaignMode.InteractionType.None, IsPropertySaveable.No, "", "", false)]
		public CampaignMode.InteractionType CampaignInteractionType { get; protected set; }

		// Token: 0x170006B5 RID: 1717
		// (get) Token: 0x060016FE RID: 5886 RVA: 0x000C10B2 File Offset: 0x000BF2B2
		// (set) Token: 0x060016FF RID: 5887 RVA: 0x000C10BA File Offset: 0x000BF2BA
		[Serialize(AIObjectiveIdle.BehaviorType.Passive, IsPropertySaveable.No, "", "", false)]
		public AIObjectiveIdle.BehaviorType Behavior { get; protected set; }

		// Token: 0x170006B6 RID: 1718
		// (get) Token: 0x06001700 RID: 5888 RVA: 0x000C10C3 File Offset: 0x000BF2C3
		// (set) Token: 0x06001701 RID: 5889 RVA: 0x000C10CB File Offset: 0x000BF2CB
		[Serialize(1f, IsPropertySaveable.No, "Affects how far the character can hear sounds created by AI targets with the tag ProvocativeToHumanAI. Used as a multiplier on the sound range of the target, e.g. a value of 0.5 would mean a target with a sound range of 1000 would need to be within 500 units for this character to hear it. Only affects the \"fight intruders\" objective, which makes the character go and inspect noises.", "", false)]
		public float Hearing { get; set; } = 1f;

		// Token: 0x170006B7 RID: 1719
		// (get) Token: 0x06001702 RID: 5890 RVA: 0x000C10D4 File Offset: 0x000BF2D4
		// (set) Token: 0x06001703 RID: 5891 RVA: 0x000C10DC File Offset: 0x000BF2DC
		[Serialize(float.PositiveInfinity, IsPropertySaveable.No, "", "", false)]
		public float ReportRange { get; protected set; }

		// Token: 0x170006B8 RID: 1720
		// (get) Token: 0x06001704 RID: 5892 RVA: 0x000C10E5 File Offset: 0x000BF2E5
		// (set) Token: 0x06001705 RID: 5893 RVA: 0x000C10ED File Offset: 0x000BF2ED
		[Serialize(float.PositiveInfinity, IsPropertySaveable.No, "", "", false)]
		public float FindWeaponsRange { get; protected set; }

		// Token: 0x170006B9 RID: 1721
		// (get) Token: 0x06001706 RID: 5894 RVA: 0x000C10F6 File Offset: 0x000BF2F6
		// (set) Token: 0x06001707 RID: 5895 RVA: 0x000C10FE File Offset: 0x000BF2FE
		public Identifier[] PreferredOutpostModuleTypes { get; protected set; }

		// Token: 0x170006BA RID: 1722
		// (get) Token: 0x06001708 RID: 5896 RVA: 0x000C1107 File Offset: 0x000BF307
		// (set) Token: 0x06001709 RID: 5897 RVA: 0x000C110F File Offset: 0x000BF30F
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier Faction { get; set; }

		// Token: 0x170006BB RID: 1723
		// (get) Token: 0x0600170A RID: 5898 RVA: 0x000C1118 File Offset: 0x000BF318
		// (set) Token: 0x0600170B RID: 5899 RVA: 0x000C1120 File Offset: 0x000BF320
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier Group { get; set; }

		// Token: 0x170006BC RID: 1724
		// (get) Token: 0x0600170C RID: 5900 RVA: 0x000C1129 File Offset: 0x000BF329
		// (set) Token: 0x0600170D RID: 5901 RVA: 0x000C1131 File Offset: 0x000BF331
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowDraggingIndefinitely { get; set; }

		// Token: 0x170006BD RID: 1725
		// (get) Token: 0x0600170E RID: 5902 RVA: 0x000C113A File Offset: 0x000BF33A
		// (set) Token: 0x0600170F RID: 5903 RVA: 0x000C1142 File Offset: 0x000BF342
		public XElement Element { get; protected set; }

		// Token: 0x06001710 RID: 5904 RVA: 0x000C114C File Offset: 0x000BF34C
		public HumanPrefab(ContentXElement element, ContentFile file, Identifier npcSetIdentifier) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			SerializableProperty.DeserializeProperties(this, element);
			this.Element = element;
			element.GetChildElements("itemset").ForEach(delegate(ContentXElement e)
			{
				this.ItemSets.Add(new ValueTuple<ContentXElement, float>(e, e.GetAttributeFloat("commonness", 1f)));
			});
			element.GetChildElements("character").ForEach(delegate(ContentXElement e)
			{
				this.CustomCharacterInfos.Add(new ValueTuple<ContentXElement, float>(e, e.GetAttributeFloat("commonness", 1f)));
			});
			this.PreferredOutpostModuleTypes = element.GetAttributeIdentifierArray("preferredoutpostmoduletypes", Array.Empty<Identifier>(), true);
			this.NpcSetIdentifier = npcSetIdentifier;
		}

		// Token: 0x06001711 RID: 5905 RVA: 0x000C122C File Offset: 0x000BF42C
		public IEnumerable<Identifier> GetTags()
		{
			return this.tags;
		}

		// Token: 0x06001712 RID: 5906 RVA: 0x000C1234 File Offset: 0x000BF434
		public IEnumerable<Identifier> GetModuleFlags()
		{
			return this.moduleFlags;
		}

		// Token: 0x06001713 RID: 5907 RVA: 0x000C123C File Offset: 0x000BF43C
		public IEnumerable<Identifier> GetSpawnPointTags()
		{
			return this.spawnPointTags;
		}

		// Token: 0x06001714 RID: 5908 RVA: 0x000C1244 File Offset: 0x000BF444
		public JobPrefab GetJobPrefab(Rand.RandSync randSync = Rand.RandSync.Unsynced, Func<JobPrefab, bool> predicate = null)
		{
			Identifier job = this.Job;
			if (!job.IsEmpty)
			{
				job = this.Job;
				if (job != "any")
				{
					return JobPrefab.Get(this.Job);
				}
			}
			return JobPrefab.Random(randSync, predicate);
		}

		// Token: 0x06001715 RID: 5909 RVA: 0x000C128C File Offset: 0x000BF48C
		public void InitializeCharacter(Character npc, ISpatialEntity positionToStayIn = null)
		{
			HumanAIController humanAI = npc.AIController as HumanAIController;
			if (humanAI != null)
			{
				AIObjectiveIdle idleObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveIdle>();
				if (positionToStayIn != null && this.Behavior == AIObjectiveIdle.BehaviorType.StayInHull)
				{
					idleObjective.TargetHull = AIObjectiveGoTo.GetTargetHull(positionToStayIn);
					idleObjective.Behavior = AIObjectiveIdle.BehaviorType.StayInHull;
				}
				else
				{
					idleObjective.Behavior = this.Behavior;
					foreach (Identifier moduleType in this.PreferredOutpostModuleTypes)
					{
						idleObjective.PreferredOutpostModuleTypes.Add(moduleType);
					}
				}
				humanAI.Hearing = this.Hearing;
				humanAI.ReportRange = this.ReportRange;
				humanAI.FindWeaponsRange = this.FindWeaponsRange;
				humanAI.AimSpeed = this.AimSpeed;
				humanAI.AimAccuracy = this.AimAccuracy;
			}
			if (this.CampaignInteractionType != CampaignMode.InteractionType.None)
			{
				CampaignMode campaignMode = GameMain.GameSession.GameMode as CampaignMode;
				if (campaignMode != null)
				{
					campaignMode.AssignNPCMenuInteraction(npc, this.CampaignInteractionType);
				}
				if (positionToStayIn != null && humanAI != null)
				{
					humanAI.ObjectiveManager.SetForcedOrder(new AIObjectiveGoTo(positionToStayIn, npc, humanAI.ObjectiveManager, true, false, 1f, 200f)
					{
						FaceTargetOnCompleted = false,
						DebugLogWhenFails = false,
						IsWaitOrder = true,
						CloseEnough = 100f
					});
				}
			}
		}

		// Token: 0x06001716 RID: 5910 RVA: 0x000C13C0 File Offset: 0x000BF5C0
		public bool GiveItems(Character character, Submarine submarine, WayPoint spawnPoint, Rand.RandSync randSync = Rand.RandSync.Unsynced, bool createNetworkEvents = true)
		{
			if (this.ItemSets == null || !this.ItemSets.Any<ValueTuple<ContentXElement, float>>())
			{
				return false;
			}
			ContentXElement spawnItems = ToolBox.SelectWeightedRandom<ValueTuple<ContentXElement, float>>(this.ItemSets, ([TupleElementNames(new string[]
			{
				"element",
				"commonness"
			})] ValueTuple<ContentXElement, float> it) => it.Item2, randSync).Item1;
			ContentXElement contentXElement = null;
			if (spawnItems != contentXElement)
			{
				foreach (ContentXElement itemElement in spawnItems.GetChildElements("item"))
				{
					int amount = itemElement.GetAttributeInt("amount", 1);
					for (int i = 0; i < amount; i++)
					{
						HumanPrefab.InitializeItem(character, itemElement, submarine, this, spawnPoint, null, createNetworkEvents);
					}
				}
			}
			return true;
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x000C1490 File Offset: 0x000BF690
		public CharacterInfo CreateCharacterInfo(Rand.RandSync randSync = Rand.RandSync.Unsynced)
		{
			ContentXElement characterElement = ToolBox.SelectWeightedRandom<ValueTuple<ContentXElement, float>>(this.CustomCharacterInfos, ([TupleElementNames(new string[]
			{
				"element",
				"commonness"
			})] ValueTuple<ContentXElement, float> info) => info.Item2, randSync).Item1;
			ContentXElement contentXElement = null;
			CharacterInfo characterInfo;
			if (characterElement == contentXElement)
			{
				Identifier humanSpeciesName = CharacterPrefab.HumanSpeciesName;
				string name = "";
				string originalName = "";
				Either<Job, JobPrefab> jobOrJobPrefab = this.GetJobPrefab(randSync, null);
				int variant = 0;
				Identifier identifier = this.Identifier;
				characterInfo = new CharacterInfo(humanSpeciesName, name, originalName, jobOrJobPrefab, variant, randSync, identifier);
			}
			else
			{
				characterInfo = new CharacterInfo(characterElement, this.Identifier);
			}
			if (characterInfo.Job != null && !MathUtils.NearlyEqual(this.SkillMultiplier, 1f, 0.0001f))
			{
				foreach (Skill skill in characterInfo.Job.GetSkills())
				{
					float newSkill = skill.Level * this.SkillMultiplier;
					skill.IncreaseSkill(newSkill - skill.Level, false);
				}
			}
			characterInfo.Salary = characterInfo.CalculateSalary(this.BaseSalary, this.SalaryMultiplier);
			characterInfo.HumanPrefabIds = new ValueTuple<Identifier, Identifier>(this.NpcSetIdentifier, this.Identifier);
			characterInfo.GiveExperience(this.ExperiencePoints);
			return characterInfo;
		}

		// Token: 0x170006BE RID: 1726
		// (get) Token: 0x06001718 RID: 5912 RVA: 0x000C15D8 File Offset: 0x000BF7D8
		public IReadOnlyCollection<ItemPrefab> InfiniteItems
		{
			get
			{
				return this.infiniteItems.Values;
			}
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x000C15E8 File Offset: 0x000BF7E8
		public static void InitializeItem(Character character, ContentXElement itemElement, Submarine submarine, HumanPrefab humanPrefab, WayPoint spawnPoint = null, Item parentItem = null, bool createNetworkEvents = true)
		{
			string itemIdentifier = itemElement.GetAttributeString("identifier", "");
			ItemPrefab itemPrefab = MapEntityPrefab.FindByIdentifier(itemIdentifier.ToIdentifier()) as ItemPrefab;
			if (itemPrefab == null)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Tried to spawn \"",
					((humanPrefab != null) ? new Identifier?(humanPrefab.Identifier) : null).ToString(),
					"\" with the item \"",
					itemIdentifier,
					"\". Matching item prefab not found."
				}), null, (itemElement != null) ? itemElement.ContentPackage : null, false, false);
				return;
			}
			Item item = new Item(itemPrefab, character.Position, null, 0, true);
			if (GameMain.Server != null && Entity.Spawner != null && createNetworkEvents)
			{
				if (GameMain.Server.EntityEventManager.UniqueEvents.Any((ServerEntityEvent ev) => ev.Entity == item))
				{
					string errorMsg = "Error while spawning job items. Item " + item.Name + " created network events before the spawn event had been created.";
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("Job.InitializeJobItem:EventsBeforeSpawning", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					GameMain.Server.EntityEventManager.UniqueEvents.RemoveAll((ServerEntityEvent ev) => ev.Entity == item);
					GameMain.Server.EntityEventManager.Events.RemoveAll((ServerEntityEvent ev) => ev.Entity == item);
				}
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item));
			}
			if (itemElement.GetAttributeBool("equip", false))
			{
				List<InvSlotType> list;
				if (item.GetComponents<Pickable>().Count<Pickable>() <= 1)
				{
					list = new List<InvSlotType>(item.AllowedSlots);
				}
				else
				{
					Wearable component = item.GetComponent<Wearable>();
					list = new List<InvSlotType>(((component != null) ? component.AllowedSlots : null) ?? item.GetComponent<Pickable>().AllowedSlots);
				}
				List<InvSlotType> allowedSlots = list;
				allowedSlots.Remove(InvSlotType.Any);
				item.UnequipAutomatically = false;
				character.Inventory.TryPutItem(item, null, allowedSlots, true, false, true);
			}
			else
			{
				character.Inventory.TryPutItem(item, null, item.AllowedSlots, true, false, true);
			}
			IdCard idCardComponent = item.GetComponent<IdCard>();
			if (idCardComponent != null)
			{
				idCardComponent.Initialize(spawnPoint, character);
				if (submarine != null && (submarine.Info.IsWreck || submarine.Info.IsOutpost))
				{
					idCardComponent.SubmarineSpecificID = submarine.SubmarineSpecificIDTag;
				}
				string[] idCardTags = itemElement.GetAttributeStringArray("tags", Array.Empty<string>(), false);
				foreach (string tag in idCardTags)
				{
					item.AddTag(tag);
				}
			}
			foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
			{
				wifiComponent.TeamID = character.TeamID;
			}
			if (parentItem != null)
			{
				parentItem.Combine(item, null);
			}
			if (itemElement.GetAttributeBool("Infinite", false))
			{
				humanPrefab.infiniteItems.TryAdd(itemPrefab.Identifier, itemPrefab);
			}
			foreach (ContentXElement childItemElement in itemElement.Elements())
			{
				int amount = childItemElement.GetAttributeInt("Amount", 1);
				for (int i = 0; i < amount; i++)
				{
					HumanPrefab.InitializeItem(character, childItemElement, submarine, humanPrefab, spawnPoint, item, createNetworkEvents);
				}
			}
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x000C1990 File Offset: 0x000BFB90
		public override void Dispose()
		{
		}

		// Token: 0x04000B15 RID: 2837
		private readonly HashSet<Identifier> tags = new HashSet<Identifier>();

		// Token: 0x04000B16 RID: 2838
		private readonly HashSet<Identifier> moduleFlags = new HashSet<Identifier>();

		// Token: 0x04000B17 RID: 2839
		private readonly HashSet<Identifier> spawnPointTags = new HashSet<Identifier>();

		// Token: 0x04000B23 RID: 2851
		[TupleElementNames(new string[]
		{
			"element",
			"commonness"
		})]
		public readonly List<ValueTuple<ContentXElement, float>> ItemSets = new List<ValueTuple<ContentXElement, float>>();

		// Token: 0x04000B24 RID: 2852
		[TupleElementNames(new string[]
		{
			"element",
			"commonness"
		})]
		public readonly List<ValueTuple<ContentXElement, float>> CustomCharacterInfos = new List<ValueTuple<ContentXElement, float>>();

		// Token: 0x04000B25 RID: 2853
		public readonly Identifier NpcSetIdentifier;

		// Token: 0x04000B26 RID: 2854
		private readonly Dictionary<Identifier, ItemPrefab> infiniteItems = new Dictionary<Identifier, ItemPrefab>();
	}
}
