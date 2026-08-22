using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001CC RID: 460
	internal class HumanPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000D2B RID: 3371
		// (get) Token: 0x0600321D RID: 12829 RVA: 0x002084E0 File Offset: 0x002066E0
		// (set) Token: 0x0600321E RID: 12830 RVA: 0x002084E8 File Offset: 0x002066E8
		[Serialize("any", IsPropertySaveable.No, "", "", false)]
		public Identifier Job { get; protected set; }

		// Token: 0x17000D2C RID: 3372
		// (get) Token: 0x0600321F RID: 12831 RVA: 0x002084F1 File Offset: 0x002066F1
		// (set) Token: 0x06003220 RID: 12832 RVA: 0x002084F9 File Offset: 0x002066F9
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float Commonness { get; protected set; }

		// Token: 0x17000D2D RID: 3373
		// (get) Token: 0x06003221 RID: 12833 RVA: 0x00208502 File Offset: 0x00206702
		// (set) Token: 0x06003222 RID: 12834 RVA: 0x0020850A File Offset: 0x0020670A
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float HealthMultiplier { get; protected set; }

		// Token: 0x17000D2E RID: 3374
		// (get) Token: 0x06003223 RID: 12835 RVA: 0x00208513 File Offset: 0x00206713
		// (set) Token: 0x06003224 RID: 12836 RVA: 0x0020851B File Offset: 0x0020671B
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float HealthMultiplierInMultiplayer { get; protected set; }

		// Token: 0x17000D2F RID: 3375
		// (get) Token: 0x06003225 RID: 12837 RVA: 0x00208524 File Offset: 0x00206724
		// (set) Token: 0x06003226 RID: 12838 RVA: 0x0020852C File Offset: 0x0020672C
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AimSpeed { get; protected set; }

		// Token: 0x17000D30 RID: 3376
		// (get) Token: 0x06003227 RID: 12839 RVA: 0x00208535 File Offset: 0x00206735
		// (set) Token: 0x06003228 RID: 12840 RVA: 0x0020853D File Offset: 0x0020673D
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float AimAccuracy { get; protected set; }

		// Token: 0x17000D31 RID: 3377
		// (get) Token: 0x06003229 RID: 12841 RVA: 0x00208546 File Offset: 0x00206746
		// (set) Token: 0x0600322A RID: 12842 RVA: 0x0020854E File Offset: 0x0020674E
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SkillMultiplier { get; protected set; }

		// Token: 0x17000D32 RID: 3378
		// (get) Token: 0x0600322B RID: 12843 RVA: 0x00208557 File Offset: 0x00206757
		// (set) Token: 0x0600322C RID: 12844 RVA: 0x0020855F File Offset: 0x0020675F
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int ExperiencePoints { get; private set; }

		// Token: 0x17000D33 RID: 3379
		// (get) Token: 0x0600322D RID: 12845 RVA: 0x00208568 File Offset: 0x00206768
		// (set) Token: 0x0600322E RID: 12846 RVA: 0x00208570 File Offset: 0x00206770
		[Serialize(0, IsPropertySaveable.No, "", "", false)]
		public int BaseSalary { get; private set; }

		// Token: 0x17000D34 RID: 3380
		// (get) Token: 0x0600322F RID: 12847 RVA: 0x00208579 File Offset: 0x00206779
		// (set) Token: 0x06003230 RID: 12848 RVA: 0x00208581 File Offset: 0x00206781
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float SalaryMultiplier { get; private set; }

		// Token: 0x17000D35 RID: 3381
		// (get) Token: 0x06003231 RID: 12849 RVA: 0x0020858A File Offset: 0x0020678A
		// (set) Token: 0x06003232 RID: 12850 RVA: 0x0020859C File Offset: 0x0020679C
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

		// Token: 0x17000D36 RID: 3382
		// (get) Token: 0x06003233 RID: 12851 RVA: 0x002085EC File Offset: 0x002067EC
		// (set) Token: 0x06003234 RID: 12852 RVA: 0x00208600 File Offset: 0x00206800
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

		// Token: 0x17000D37 RID: 3383
		// (get) Token: 0x06003235 RID: 12853 RVA: 0x00208650 File Offset: 0x00206850
		// (set) Token: 0x06003236 RID: 12854 RVA: 0x00208664 File Offset: 0x00206864
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

		// Token: 0x17000D38 RID: 3384
		// (get) Token: 0x06003237 RID: 12855 RVA: 0x002086B4 File Offset: 0x002068B4
		// (set) Token: 0x06003238 RID: 12856 RVA: 0x002086BC File Offset: 0x002068BC
		[Serialize(false, IsPropertySaveable.No, "If enabled, the NPC will not spawn if the specified spawn point tags can't be found.", "", false)]
		public bool RequireSpawnPointTag { get; protected set; }

		// Token: 0x17000D39 RID: 3385
		// (get) Token: 0x06003239 RID: 12857 RVA: 0x002086C5 File Offset: 0x002068C5
		// (set) Token: 0x0600323A RID: 12858 RVA: 0x002086CD File Offset: 0x002068CD
		[Serialize(CampaignMode.InteractionType.None, IsPropertySaveable.No, "", "", false)]
		public CampaignMode.InteractionType CampaignInteractionType { get; protected set; }

		// Token: 0x17000D3A RID: 3386
		// (get) Token: 0x0600323B RID: 12859 RVA: 0x002086D6 File Offset: 0x002068D6
		// (set) Token: 0x0600323C RID: 12860 RVA: 0x002086DE File Offset: 0x002068DE
		[Serialize(AIObjectiveIdle.BehaviorType.Passive, IsPropertySaveable.No, "", "", false)]
		public AIObjectiveIdle.BehaviorType Behavior { get; protected set; }

		// Token: 0x17000D3B RID: 3387
		// (get) Token: 0x0600323D RID: 12861 RVA: 0x002086E7 File Offset: 0x002068E7
		// (set) Token: 0x0600323E RID: 12862 RVA: 0x002086EF File Offset: 0x002068EF
		[Serialize(1f, IsPropertySaveable.No, "Affects how far the character can hear sounds created by AI targets with the tag ProvocativeToHumanAI. Used as a multiplier on the sound range of the target, e.g. a value of 0.5 would mean a target with a sound range of 1000 would need to be within 500 units for this character to hear it. Only affects the \"fight intruders\" objective, which makes the character go and inspect noises.", "", false)]
		public float Hearing { get; set; } = 1f;

		// Token: 0x17000D3C RID: 3388
		// (get) Token: 0x0600323F RID: 12863 RVA: 0x002086F8 File Offset: 0x002068F8
		// (set) Token: 0x06003240 RID: 12864 RVA: 0x00208700 File Offset: 0x00206900
		[Serialize(float.PositiveInfinity, IsPropertySaveable.No, "", "", false)]
		public float ReportRange { get; protected set; }

		// Token: 0x17000D3D RID: 3389
		// (get) Token: 0x06003241 RID: 12865 RVA: 0x00208709 File Offset: 0x00206909
		// (set) Token: 0x06003242 RID: 12866 RVA: 0x00208711 File Offset: 0x00206911
		[Serialize(float.PositiveInfinity, IsPropertySaveable.No, "", "", false)]
		public float FindWeaponsRange { get; protected set; }

		// Token: 0x17000D3E RID: 3390
		// (get) Token: 0x06003243 RID: 12867 RVA: 0x0020871A File Offset: 0x0020691A
		// (set) Token: 0x06003244 RID: 12868 RVA: 0x00208722 File Offset: 0x00206922
		public Identifier[] PreferredOutpostModuleTypes { get; protected set; }

		// Token: 0x17000D3F RID: 3391
		// (get) Token: 0x06003245 RID: 12869 RVA: 0x0020872B File Offset: 0x0020692B
		// (set) Token: 0x06003246 RID: 12870 RVA: 0x00208733 File Offset: 0x00206933
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier Faction { get; set; }

		// Token: 0x17000D40 RID: 3392
		// (get) Token: 0x06003247 RID: 12871 RVA: 0x0020873C File Offset: 0x0020693C
		// (set) Token: 0x06003248 RID: 12872 RVA: 0x00208744 File Offset: 0x00206944
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		public Identifier Group { get; set; }

		// Token: 0x17000D41 RID: 3393
		// (get) Token: 0x06003249 RID: 12873 RVA: 0x0020874D File Offset: 0x0020694D
		// (set) Token: 0x0600324A RID: 12874 RVA: 0x00208755 File Offset: 0x00206955
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowDraggingIndefinitely { get; set; }

		// Token: 0x17000D42 RID: 3394
		// (get) Token: 0x0600324B RID: 12875 RVA: 0x0020875E File Offset: 0x0020695E
		// (set) Token: 0x0600324C RID: 12876 RVA: 0x00208766 File Offset: 0x00206966
		public XElement Element { get; protected set; }

		// Token: 0x0600324D RID: 12877 RVA: 0x00208770 File Offset: 0x00206970
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

		// Token: 0x0600324E RID: 12878 RVA: 0x00208850 File Offset: 0x00206A50
		public IEnumerable<Identifier> GetTags()
		{
			return this.tags;
		}

		// Token: 0x0600324F RID: 12879 RVA: 0x00208858 File Offset: 0x00206A58
		public IEnumerable<Identifier> GetModuleFlags()
		{
			return this.moduleFlags;
		}

		// Token: 0x06003250 RID: 12880 RVA: 0x00208860 File Offset: 0x00206A60
		public IEnumerable<Identifier> GetSpawnPointTags()
		{
			return this.spawnPointTags;
		}

		// Token: 0x06003251 RID: 12881 RVA: 0x00208868 File Offset: 0x00206A68
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

		// Token: 0x06003252 RID: 12882 RVA: 0x002088B0 File Offset: 0x00206AB0
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

		// Token: 0x06003253 RID: 12883 RVA: 0x002089E4 File Offset: 0x00206BE4
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

		// Token: 0x06003254 RID: 12884 RVA: 0x00208AB4 File Offset: 0x00206CB4
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

		// Token: 0x17000D43 RID: 3395
		// (get) Token: 0x06003255 RID: 12885 RVA: 0x00208BFC File Offset: 0x00206DFC
		public IReadOnlyCollection<ItemPrefab> InfiniteItems
		{
			get
			{
				return this.infiniteItems.Values;
			}
		}

		// Token: 0x06003256 RID: 12886 RVA: 0x00208C0C File Offset: 0x00206E0C
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

		// Token: 0x06003257 RID: 12887 RVA: 0x00208EA0 File Offset: 0x002070A0
		public override void Dispose()
		{
		}

		// Token: 0x04001A59 RID: 6745
		private readonly HashSet<Identifier> tags = new HashSet<Identifier>();

		// Token: 0x04001A5A RID: 6746
		private readonly HashSet<Identifier> moduleFlags = new HashSet<Identifier>();

		// Token: 0x04001A5B RID: 6747
		private readonly HashSet<Identifier> spawnPointTags = new HashSet<Identifier>();

		// Token: 0x04001A67 RID: 6759
		[TupleElementNames(new string[]
		{
			"element",
			"commonness"
		})]
		public readonly List<ValueTuple<ContentXElement, float>> ItemSets = new List<ValueTuple<ContentXElement, float>>();

		// Token: 0x04001A68 RID: 6760
		[TupleElementNames(new string[]
		{
			"element",
			"commonness"
		})]
		public readonly List<ValueTuple<ContentXElement, float>> CustomCharacterInfos = new List<ValueTuple<ContentXElement, float>>();

		// Token: 0x04001A69 RID: 6761
		public readonly Identifier NpcSetIdentifier;

		// Token: 0x04001A6A RID: 6762
		private readonly Dictionary<Identifier, ItemPrefab> infiniteItems = new Dictionary<Identifier, ItemPrefab>();
	}
}
