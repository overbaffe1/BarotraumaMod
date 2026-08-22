using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000D3 RID: 211
	internal class JobPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x06001734 RID: 5940 RVA: 0x000C2520 File Offset: 0x000C0720
		public override void Dispose()
		{
		}

		// Token: 0x170006C3 RID: 1731
		// (get) Token: 0x06001735 RID: 5941 RVA: 0x000C2522 File Offset: 0x000C0722
		public static IReadOnlyDictionary<Identifier, float> ItemRepairPriorities
		{
			get
			{
				return JobPrefab._itemRepairPriorities;
			}
		}

		// Token: 0x06001736 RID: 5942 RVA: 0x000C2529 File Offset: 0x000C0729
		public static JobPrefab Get(Identifier identifier)
		{
			if (JobPrefab.Prefabs.ContainsKey(identifier))
			{
				return JobPrefab.Prefabs[identifier];
			}
			DebugConsole.ThrowError("Couldn't find a job prefab with the given identifier: " + identifier.ToString(), null, null, false, false);
			return null;
		}

		// Token: 0x06001737 RID: 5943 RVA: 0x000C2568 File Offset: 0x000C0768
		public IEnumerable<JobPrefab.JobItem> GetJobItems(int jobVariant, Func<JobPrefab.JobItem, bool> predicate)
		{
			ImmutableArray<JobPrefab.JobItem> items;
			if (!this.JobItems.TryGetValue(jobVariant, out items))
			{
				return Enumerable.Empty<JobPrefab.JobItem>();
			}
			return items.Where(predicate);
		}

		// Token: 0x06001738 RID: 5944 RVA: 0x000C2594 File Offset: 0x000C0794
		public bool HasJobItem(int jobVariant, Func<JobPrefab.JobItem, bool> predicate)
		{
			ImmutableArray<JobPrefab.JobItem> items;
			return this.JobItems.TryGetValue(jobVariant, out items) && items.Any(predicate);
		}

		// Token: 0x170006C4 RID: 1732
		// (get) Token: 0x06001739 RID: 5945 RVA: 0x000C25BA File Offset: 0x000C07BA
		// (set) Token: 0x0600173A RID: 5946 RVA: 0x000C25C2 File Offset: 0x000C07C2
		[Serialize("1,1,1,1", IsPropertySaveable.No, "", "", false)]
		public Color UIColor { get; private set; }

		// Token: 0x170006C5 RID: 1733
		// (get) Token: 0x0600173B RID: 5947 RVA: 0x000C25CB File Offset: 0x000C07CB
		// (set) Token: 0x0600173C RID: 5948 RVA: 0x000C25D3 File Offset: 0x000C07D3
		[Serialize(AIObjectiveIdle.BehaviorType.Passive, IsPropertySaveable.No, "How should the character behave when idling (not doing any particular task)?", "", false)]
		public AIObjectiveIdle.BehaviorType IdleBehavior { get; private set; }

		// Token: 0x170006C6 RID: 1734
		// (get) Token: 0x0600173D RID: 5949 RVA: 0x000C25DC File Offset: 0x000C07DC
		// (set) Token: 0x0600173E RID: 5950 RVA: 0x000C25E4 File Offset: 0x000C07E4
		[Serialize(false, IsPropertySaveable.No, "Can the character speak any random lines, or just ones specifically meant for the job?", "", false)]
		public bool OnlyJobSpecificDialog { get; private set; }

		// Token: 0x170006C7 RID: 1735
		// (get) Token: 0x0600173F RID: 5951 RVA: 0x000C25ED File Offset: 0x000C07ED
		// (set) Token: 0x06001740 RID: 5952 RVA: 0x000C25F5 File Offset: 0x000C07F5
		[Serialize(0, IsPropertySaveable.No, "The number of these characters in the crew the player starts with in the single player campaign.", "", false)]
		public int InitialCount { get; private set; }

		// Token: 0x170006C8 RID: 1736
		// (get) Token: 0x06001741 RID: 5953 RVA: 0x000C25FE File Offset: 0x000C07FE
		// (set) Token: 0x06001742 RID: 5954 RVA: 0x000C2606 File Offset: 0x000C0806
		[Serialize(10, IsPropertySaveable.No, "Determines the order of the characters in the campaign setup ui.", "", false)]
		public int CampaignSetupUIOrder { get; private set; }

		// Token: 0x170006C9 RID: 1737
		// (get) Token: 0x06001743 RID: 5955 RVA: 0x000C260F File Offset: 0x000C080F
		// (set) Token: 0x06001744 RID: 5956 RVA: 0x000C2617 File Offset: 0x000C0817
		[Serialize(false, IsPropertySaveable.No, "If set to true, a client that has chosen this as their preferred job will get it regardless of the maximum number or the amount of spawnpoints in the sub.", "", false)]
		public bool AllowAlways { get; private set; }

		// Token: 0x170006CA RID: 1738
		// (get) Token: 0x06001745 RID: 5957 RVA: 0x000C2620 File Offset: 0x000C0820
		// (set) Token: 0x06001746 RID: 5958 RVA: 0x000C2628 File Offset: 0x000C0828
		[Serialize(100, IsPropertySaveable.No, "How many crew members can have the job (e.g. only one captain etc).", "", false)]
		public int MaxNumber { get; private set; }

		// Token: 0x170006CB RID: 1739
		// (get) Token: 0x06001747 RID: 5959 RVA: 0x000C2631 File Offset: 0x000C0831
		// (set) Token: 0x06001748 RID: 5960 RVA: 0x000C2639 File Offset: 0x000C0839
		[Serialize(0, IsPropertySaveable.No, "How many crew members are required to have the job. I.e. if one captain is required, one captain is chosen even if all the players have set captain to lowest preference.", "", false)]
		public int MinNumber { get; private set; }

		// Token: 0x170006CC RID: 1740
		// (get) Token: 0x06001749 RID: 5961 RVA: 0x000C2642 File Offset: 0x000C0842
		// (set) Token: 0x0600174A RID: 5962 RVA: 0x000C264A File Offset: 0x000C084A
		[Serialize(0f, IsPropertySaveable.No, "Minimum amount of karma a player must have to get assigned this job.", "", false)]
		public float MinKarma { get; private set; }

		// Token: 0x170006CD RID: 1741
		// (get) Token: 0x0600174B RID: 5963 RVA: 0x000C2653 File Offset: 0x000C0853
		// (set) Token: 0x0600174C RID: 5964 RVA: 0x000C265B File Offset: 0x000C085B
		[Serialize(1f, IsPropertySaveable.No, "Multiplier on the base hiring cost when hiring the character from an outpost.", "", false)]
		public float PriceMultiplier { get; private set; }

		// Token: 0x170006CE RID: 1742
		// (get) Token: 0x0600174D RID: 5965 RVA: 0x000C2664 File Offset: 0x000C0864
		// (set) Token: 0x0600174E RID: 5966 RVA: 0x000C266C File Offset: 0x000C086C
		[Serialize(0f, IsPropertySaveable.No, "How much the vitality of the character is increased/reduced from the default value (e.g. 10 = 110 total vitality if the default vitality is 100.).", "", false)]
		public float VitalityModifier { get; private set; }

		// Token: 0x170006CF RID: 1743
		// (get) Token: 0x0600174F RID: 5967 RVA: 0x000C2675 File Offset: 0x000C0875
		// (set) Token: 0x06001750 RID: 5968 RVA: 0x000C267D File Offset: 0x000C087D
		[Serialize(false, IsPropertySaveable.No, "Hidden jobs are not selectable by players, but can be used by e.g. outpost NPCs.", "", false)]
		public bool HiddenJob { get; private set; }

		// Token: 0x170006D0 RID: 1744
		// (get) Token: 0x06001751 RID: 5969 RVA: 0x000C2686 File Offset: 0x000C0886
		public SkillPrefab PrimarySkill
		{
			get
			{
				List<SkillPrefab> skills = this.Skills;
				if (skills == null)
				{
					return null;
				}
				return skills.FirstOrDefault((SkillPrefab s) => s.IsPrimarySkill);
			}
		}

		// Token: 0x170006D1 RID: 1745
		// (get) Token: 0x06001752 RID: 5970 RVA: 0x000C26B8 File Offset: 0x000C08B8
		// (set) Token: 0x06001753 RID: 5971 RVA: 0x000C26C0 File Offset: 0x000C08C0
		public ContentXElement Element { get; private set; }

		// Token: 0x170006D2 RID: 1746
		// (get) Token: 0x06001754 RID: 5972 RVA: 0x000C26C9 File Offset: 0x000C08C9
		// (set) Token: 0x06001755 RID: 5973 RVA: 0x000C26D1 File Offset: 0x000C08D1
		public int Variants { get; private set; }

		// Token: 0x06001756 RID: 5974 RVA: 0x000C26DC File Offset: 0x000C08DC
		public JobPrefab(ContentXElement element, JobsFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			SerializableProperty.DeserializeProperties(this, element);
			this.Name = TextManager.Get("JobName." + this.Identifier.ToString());
			this.Description = TextManager.Get("JobDescription." + this.Identifier.ToString());
			this.Element = element;
			JobPrefab.<>c__DisplayClass75_0 CS$<>8__locals1;
			CS$<>8__locals1.jobItems = new Dictionary<int, List<JobPrefab.JobItem>>();
			int variant = 0;
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length <= 7)
					{
						if (length != 6)
						{
							if (length != 7)
							{
								continue;
							}
							char c = text[0];
							if (c != 'i')
							{
								if (c != 'j')
								{
									continue;
								}
								if (!(text == "jobicon"))
								{
									continue;
								}
								this.Icon = new Sprite(subElement.FirstElement(), "", "", false, 1f);
								continue;
							}
							else
							{
								if (!(text == "itemset"))
								{
									continue;
								}
								CS$<>8__locals1.jobItems[variant] = new List<JobPrefab.JobItem>();
								this.<.ctor>g__loadJobItems|75_0(subElement, variant, null, ref CS$<>8__locals1);
								variant++;
								continue;
							}
						}
						else
						{
							if (!(text == "skills"))
							{
								continue;
							}
							using (IEnumerator<ContentXElement> enumerator2 = subElement.Elements().GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									ContentXElement skillElement = enumerator2.Current;
									this.Skills.Add(new SkillPrefab(skillElement));
								}
								continue;
							}
						}
					}
					else
					{
						if (length != 12)
						{
							switch (length)
							{
							case 17:
								if (!(text == "appropriateorders"))
								{
									continue;
								}
								break;
							case 18:
							case 19:
								continue;
							case 20:
								if (!(text == "autonomousobjectives"))
								{
									continue;
								}
								goto IL_23E;
							case 21:
								if (!(text == "appropriateobjectives"))
								{
									continue;
								}
								break;
							default:
								continue;
							}
							subElement.Elements().ForEach(delegate(ContentXElement order)
							{
								this.AppropriateOrders.Add(order.GetAttributeIdentifier("identifier", ""));
							});
							continue;
						}
						if (!(text == "jobiconsmall"))
						{
							continue;
						}
						this.IconSmall = new Sprite(subElement.FirstElement(), "", "", false, 1f);
						continue;
					}
					IL_23E:
					subElement.Elements().ForEach(delegate(ContentXElement order)
					{
						this.AutonomousObjectives.Add(new AutonomousObjective(order));
					});
				}
			}
			this.JobItems = (from kvp in CS$<>8__locals1.jobItems
			select new ValueTuple<int, ImmutableArray<JobPrefab.JobItem>>(kvp.Key, kvp.Value.ToImmutableArray<JobPrefab.JobItem>())).ToImmutableDictionary<int, ImmutableArray<JobPrefab.JobItem>>();
			this.Variants = variant;
			this.Skills.Sort((SkillPrefab x, SkillPrefab y) => y.GetLevelRange(false).Start.CompareTo(x.GetLevelRange(false).Start));
		}

		// Token: 0x06001757 RID: 5975 RVA: 0x000C2A50 File Offset: 0x000C0C50
		public static JobPrefab Random(Rand.RandSync sync, Func<JobPrefab, bool> predicate = null)
		{
			return JobPrefab.Prefabs.GetRandom((JobPrefab p) => !p.HiddenJob && (predicate == null || predicate(p)), sync);
		}

		// Token: 0x0600175B RID: 5979 RVA: 0x000C2ACC File Offset: 0x000C0CCC
		[CompilerGenerated]
		private void <.ctor>g__loadJobItems|75_0(ContentXElement parentElement, int variant, JobPrefab.JobItem parentItem, ref JobPrefab.<>c__DisplayClass75_0 A_4)
		{
			foreach (ContentXElement itemElement in parentElement.GetChildElements("Item"))
			{
				if (itemElement.GetAttribute("name") != null)
				{
					DebugConsole.ThrowErrorLocalized("Error in job config \"" + this.Name + "\" - use identifiers instead of names to configure the items.", null, parentElement.ContentPackage, false, false);
				}
				else
				{
					Identifier itemIdentifier = itemElement.GetAttributeIdentifier("identifier", Identifier.Empty);
					JobPrefab.JobItem jobItem = null;
					if (itemIdentifier.IsEmpty)
					{
						DebugConsole.ThrowErrorLocalized("Error in job config \"" + this.Name + "\" - item with no identifier.", null, parentElement.ContentPackage, false, false);
					}
					else
					{
						jobItem = new JobPrefab.JobItem(itemElement, parentItem);
						A_4.jobItems[variant].Add(jobItem);
					}
					this.<.ctor>g__loadJobItems|75_0(itemElement, variant, jobItem, ref A_4);
				}
			}
		}

		// Token: 0x04000B34 RID: 2868
		public static readonly PrefabCollection<JobPrefab> Prefabs = new PrefabCollection<JobPrefab>();

		// Token: 0x04000B35 RID: 2869
		private static readonly Dictionary<Identifier, float> _itemRepairPriorities = new Dictionary<Identifier, float>();

		// Token: 0x04000B36 RID: 2870
		public readonly ImmutableDictionary<int, ImmutableArray<JobPrefab.JobItem>> JobItems;

		// Token: 0x04000B37 RID: 2871
		public readonly List<SkillPrefab> Skills = new List<SkillPrefab>();

		// Token: 0x04000B38 RID: 2872
		public readonly List<AutonomousObjective> AutonomousObjectives = new List<AutonomousObjective>();

		// Token: 0x04000B39 RID: 2873
		public readonly List<Identifier> AppropriateOrders = new List<Identifier>();

		// Token: 0x04000B3B RID: 2875
		public readonly LocalizedString Name;

		// Token: 0x04000B3D RID: 2877
		public readonly LocalizedString Description;

		// Token: 0x04000B48 RID: 2888
		public Sprite Icon;

		// Token: 0x04000B49 RID: 2889
		public Sprite IconSmall;

		// Token: 0x02000899 RID: 2201
		public class JobItem
		{
			// Token: 0x06005575 RID: 21877 RVA: 0x001F2A3C File Offset: 0x001F0C3C
			public JobItem(ContentXElement element, JobPrefab.JobItem parentItem)
			{
				this.ItemIdentifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
				this.ItemIdentifierTeam2 = element.GetAttributeIdentifier("identifierteam2", Identifier.Empty);
				this.ShowPreview = element.GetAttributeBool("ShowPreview", true);
				string key = "GameMode";
				JobPrefab.JobItem.GameModeType gameModeType = (parentItem != null) ? parentItem.GameMode : JobPrefab.JobItem.GameModeType.Any;
				this.GameMode = element.GetAttributeEnum<JobPrefab.JobItem.GameModeType>(key, gameModeType);
				this.Amount = element.GetAttributeInt("Amount", 1);
				this.Equip = element.GetAttributeBool("Equip", false);
				this.Outfit = element.GetAttributeBool("Outfit", false);
				this.Infinite = element.GetAttributeBool("Infinite", false);
				this.ParentItem = parentItem;
			}

			// Token: 0x06005576 RID: 21878 RVA: 0x001F2AFC File Offset: 0x001F0CFC
			public Identifier GetItemIdentifier(CharacterTeamType team, bool isPvPMode)
			{
				JobPrefab.JobItem.GameModeType gameMode = this.GameMode;
				if (gameMode != JobPrefab.JobItem.GameModeType.PvP)
				{
					if (gameMode == JobPrefab.JobItem.GameModeType.PvE)
					{
						if (isPvPMode)
						{
							return Identifier.Empty;
						}
					}
				}
				else if (!isPvPMode)
				{
					return Identifier.Empty;
				}
				if (team != CharacterTeamType.Team2 || this.ItemIdentifierTeam2.IsEmpty)
				{
					return this.ItemIdentifier;
				}
				return this.ItemIdentifierTeam2;
			}

			// Token: 0x04003039 RID: 12345
			public readonly Identifier ItemIdentifier;

			// Token: 0x0400303A RID: 12346
			public readonly Identifier ItemIdentifierTeam2;

			// Token: 0x0400303B RID: 12347
			public readonly bool ShowPreview;

			// Token: 0x0400303C RID: 12348
			public readonly bool Equip;

			// Token: 0x0400303D RID: 12349
			public readonly bool Outfit;

			// Token: 0x0400303E RID: 12350
			public readonly int Amount;

			// Token: 0x0400303F RID: 12351
			public readonly bool Infinite;

			// Token: 0x04003040 RID: 12352
			public readonly JobPrefab.JobItem ParentItem;

			// Token: 0x04003041 RID: 12353
			public readonly JobPrefab.JobItem.GameModeType GameMode;

			// Token: 0x02000E82 RID: 3714
			public enum GameModeType
			{
				// Token: 0x04004288 RID: 17032
				Any,
				// Token: 0x04004289 RID: 17033
				PvP,
				// Token: 0x0400428A RID: 17034
				PvE
			}
		}
	}
}
