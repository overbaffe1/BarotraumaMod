using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000031 RID: 49
	internal class JobPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x060007E8 RID: 2024 RVA: 0x00048A7C File Offset: 0x00046C7C
		public GUIButton CreateInfoFrame(bool isPvP, out GUIComponent buttonContainer)
		{
			int windowPixelWidth = 500;
			int windowPixelHeight = 400;
			Point absoluteWindowSize = new Point((int)((float)windowPixelWidth * GUI.xScale), (int)((float)windowPixelHeight * GUI.yScale));
			GUIButton frameHolder = new GUIButton(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null);
			new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, frameHolder.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			GUIFrame frame = new GUIFrame(new RectTransform(absoluteWindowSize, frameHolder.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "", null);
			GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = this.Name;
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
			GUIListBox contentList = new GUIListBox(new RectTransform(new Vector2(1f, 0.75f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, false, null, "", true, false)
			{
				ScrollBarVisible = true,
				AutoHideScrollBar = true,
				CurrentSelectMode = GUIListBox.SelectMode.None,
				Padding = new Vector4(0f, GUI.Scale * 10f, 0f, 0f),
				Spacing = (int)(GUI.Scale * 5f)
			};
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), contentList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = this.Description;
			font = GUIStyle.SmallFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.TopLeft, true, "", null).CanBeFocused = false;
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), contentList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("Skills");
			font = GUIStyle.LargeFont;
			new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null).CanBeFocused = false;
			foreach (SkillPrefab skill in this.Skills)
			{
				Range<float> levelRange = skill.GetLevelRange(isPvP);
				string levelStr = (levelRange.End > levelRange.Start) ? (((int)levelRange.Start).ToString() + " - " + ((int)levelRange.End).ToString()) : ((int)levelRange.Start).ToString();
				RectTransform rectT4 = new RectTransform(new Vector2(1f, 0f), contentList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text4 = "   - " + TextManager.AddPunctuation(':', new LocalizedString[]
				{
					TextManager.Get("SkillName." + skill.Identifier.ToString()),
					levelStr
				});
				font = GUIStyle.SmallFont;
				new GUITextBlock(rectT4, text4, null, font, Alignment.Left, true, "", null).CanBeFocused = false;
			}
			buttonContainer = paddedFrame;
			return frameHolder;
		}

		// Token: 0x060007E9 RID: 2025 RVA: 0x00048F50 File Offset: 0x00047150
		public IEnumerable<Sprite> GetJobOutfitSprites(CharacterTeamType team, bool isPvPMode)
		{
			IEnumerable<Identifier> equipIdentifiers = from j in this.JobItems.SelectMany((KeyValuePair<int, ImmutableArray<JobPrefab.JobItem>> kvp) => kvp.Value)
			where j.Outfit
			select j.GetItemIdentifier(team, isPvPMode);
			List<ItemPrefab> outfitPrefabs = new List<ItemPrefab>();
			using (IEnumerator<Identifier> enumerator = equipIdentifiers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier equipIdentifier = enumerator.Current;
					ItemPrefab itemPrefab = ItemPrefab.Prefabs.Find((ItemPrefab ip) => ip.Identifier == equipIdentifier);
					if (itemPrefab != null)
					{
						outfitPrefabs.Add(itemPrefab);
					}
				}
			}
			if (!outfitPrefabs.Any<ItemPrefab>())
			{
				return Enumerable.Empty<Sprite>();
			}
			return from p in outfitPrefabs
			select p.InventoryIcon ?? p.Sprite;
		}

		// Token: 0x060007EA RID: 2026 RVA: 0x00049070 File Offset: 0x00047270
		public override void Dispose()
		{
		}

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x060007EB RID: 2027 RVA: 0x00049072 File Offset: 0x00047272
		public static IReadOnlyDictionary<Identifier, float> ItemRepairPriorities
		{
			get
			{
				return JobPrefab._itemRepairPriorities;
			}
		}

		// Token: 0x060007EC RID: 2028 RVA: 0x00049079 File Offset: 0x00047279
		public static JobPrefab Get(Identifier identifier)
		{
			if (JobPrefab.Prefabs.ContainsKey(identifier))
			{
				return JobPrefab.Prefabs[identifier];
			}
			DebugConsole.ThrowError("Couldn't find a job prefab with the given identifier: " + identifier.ToString(), null, null, false, false);
			return null;
		}

		// Token: 0x060007ED RID: 2029 RVA: 0x000490B8 File Offset: 0x000472B8
		public IEnumerable<JobPrefab.JobItem> GetJobItems(int jobVariant, Func<JobPrefab.JobItem, bool> predicate)
		{
			ImmutableArray<JobPrefab.JobItem> items;
			if (!this.JobItems.TryGetValue(jobVariant, out items))
			{
				return Enumerable.Empty<JobPrefab.JobItem>();
			}
			return items.Where(predicate);
		}

		// Token: 0x060007EE RID: 2030 RVA: 0x000490E4 File Offset: 0x000472E4
		public bool HasJobItem(int jobVariant, Func<JobPrefab.JobItem, bool> predicate)
		{
			ImmutableArray<JobPrefab.JobItem> items;
			return this.JobItems.TryGetValue(jobVariant, out items) && items.Any(predicate);
		}

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x060007EF RID: 2031 RVA: 0x0004910A File Offset: 0x0004730A
		// (set) Token: 0x060007F0 RID: 2032 RVA: 0x00049112 File Offset: 0x00047312
		[Serialize("1,1,1,1", IsPropertySaveable.No, "", "", false)]
		public Color UIColor { get; private set; }

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x060007F1 RID: 2033 RVA: 0x0004911B File Offset: 0x0004731B
		// (set) Token: 0x060007F2 RID: 2034 RVA: 0x00049123 File Offset: 0x00047323
		[Serialize(AIObjectiveIdle.BehaviorType.Passive, IsPropertySaveable.No, "How should the character behave when idling (not doing any particular task)?", "", false)]
		public AIObjectiveIdle.BehaviorType IdleBehavior { get; private set; }

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x060007F3 RID: 2035 RVA: 0x0004912C File Offset: 0x0004732C
		// (set) Token: 0x060007F4 RID: 2036 RVA: 0x00049134 File Offset: 0x00047334
		[Serialize(false, IsPropertySaveable.No, "Can the character speak any random lines, or just ones specifically meant for the job?", "", false)]
		public bool OnlyJobSpecificDialog { get; private set; }

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x060007F5 RID: 2037 RVA: 0x0004913D File Offset: 0x0004733D
		// (set) Token: 0x060007F6 RID: 2038 RVA: 0x00049145 File Offset: 0x00047345
		[Serialize(0, IsPropertySaveable.No, "The number of these characters in the crew the player starts with in the single player campaign.", "", false)]
		public int InitialCount { get; private set; }

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x060007F7 RID: 2039 RVA: 0x0004914E File Offset: 0x0004734E
		// (set) Token: 0x060007F8 RID: 2040 RVA: 0x00049156 File Offset: 0x00047356
		[Serialize(10, IsPropertySaveable.No, "Determines the order of the characters in the campaign setup ui.", "", false)]
		public int CampaignSetupUIOrder { get; private set; }

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x060007F9 RID: 2041 RVA: 0x0004915F File Offset: 0x0004735F
		// (set) Token: 0x060007FA RID: 2042 RVA: 0x00049167 File Offset: 0x00047367
		[Serialize(false, IsPropertySaveable.No, "If set to true, a client that has chosen this as their preferred job will get it regardless of the maximum number or the amount of spawnpoints in the sub.", "", false)]
		public bool AllowAlways { get; private set; }

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x060007FB RID: 2043 RVA: 0x00049170 File Offset: 0x00047370
		// (set) Token: 0x060007FC RID: 2044 RVA: 0x00049178 File Offset: 0x00047378
		[Serialize(100, IsPropertySaveable.No, "How many crew members can have the job (e.g. only one captain etc).", "", false)]
		public int MaxNumber { get; private set; }

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x060007FD RID: 2045 RVA: 0x00049181 File Offset: 0x00047381
		// (set) Token: 0x060007FE RID: 2046 RVA: 0x00049189 File Offset: 0x00047389
		[Serialize(0, IsPropertySaveable.No, "How many crew members are required to have the job. I.e. if one captain is required, one captain is chosen even if all the players have set captain to lowest preference.", "", false)]
		public int MinNumber { get; private set; }

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x060007FF RID: 2047 RVA: 0x00049192 File Offset: 0x00047392
		// (set) Token: 0x06000800 RID: 2048 RVA: 0x0004919A File Offset: 0x0004739A
		[Serialize(0f, IsPropertySaveable.No, "Minimum amount of karma a player must have to get assigned this job.", "", false)]
		public float MinKarma { get; private set; }

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000801 RID: 2049 RVA: 0x000491A3 File Offset: 0x000473A3
		// (set) Token: 0x06000802 RID: 2050 RVA: 0x000491AB File Offset: 0x000473AB
		[Serialize(1f, IsPropertySaveable.No, "Multiplier on the base hiring cost when hiring the character from an outpost.", "", false)]
		public float PriceMultiplier { get; private set; }

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06000803 RID: 2051 RVA: 0x000491B4 File Offset: 0x000473B4
		// (set) Token: 0x06000804 RID: 2052 RVA: 0x000491BC File Offset: 0x000473BC
		[Serialize(0f, IsPropertySaveable.No, "How much the vitality of the character is increased/reduced from the default value (e.g. 10 = 110 total vitality if the default vitality is 100.).", "", false)]
		public float VitalityModifier { get; private set; }

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000805 RID: 2053 RVA: 0x000491C5 File Offset: 0x000473C5
		// (set) Token: 0x06000806 RID: 2054 RVA: 0x000491CD File Offset: 0x000473CD
		[Serialize(false, IsPropertySaveable.No, "Hidden jobs are not selectable by players, but can be used by e.g. outpost NPCs.", "", false)]
		public bool HiddenJob { get; private set; }

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000807 RID: 2055 RVA: 0x000491D6 File Offset: 0x000473D6
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

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000808 RID: 2056 RVA: 0x00049208 File Offset: 0x00047408
		// (set) Token: 0x06000809 RID: 2057 RVA: 0x00049210 File Offset: 0x00047410
		public ContentXElement Element { get; private set; }

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x0600080A RID: 2058 RVA: 0x00049219 File Offset: 0x00047419
		// (set) Token: 0x0600080B RID: 2059 RVA: 0x00049221 File Offset: 0x00047421
		public int Variants { get; private set; }

		// Token: 0x0600080C RID: 2060 RVA: 0x0004922C File Offset: 0x0004742C
		public JobPrefab(ContentXElement element, JobsFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			SerializableProperty.DeserializeProperties(this, element);
			this.Name = TextManager.Get("JobName." + this.Identifier.ToString());
			this.Description = TextManager.Get("JobDescription." + this.Identifier.ToString());
			this.Element = element;
			JobPrefab.<>c__DisplayClass77_0 CS$<>8__locals1;
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
								this.<.ctor>g__loadJobItems|77_0(subElement, variant, null, ref CS$<>8__locals1);
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

		// Token: 0x0600080D RID: 2061 RVA: 0x000495A0 File Offset: 0x000477A0
		public static JobPrefab Random(Rand.RandSync sync, Func<JobPrefab, bool> predicate = null)
		{
			return JobPrefab.Prefabs.GetRandom((JobPrefab p) => !p.HiddenJob && (predicate == null || predicate(p)), sync);
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x0004961C File Offset: 0x0004781C
		[CompilerGenerated]
		private void <.ctor>g__loadJobItems|77_0(ContentXElement parentElement, int variant, JobPrefab.JobItem parentItem, ref JobPrefab.<>c__DisplayClass77_0 A_4)
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
					this.<.ctor>g__loadJobItems|77_0(itemElement, variant, jobItem, ref A_4);
				}
			}
		}

		// Token: 0x04000422 RID: 1058
		public static readonly PrefabCollection<JobPrefab> Prefabs = new PrefabCollection<JobPrefab>();

		// Token: 0x04000423 RID: 1059
		private static readonly Dictionary<Identifier, float> _itemRepairPriorities = new Dictionary<Identifier, float>();

		// Token: 0x04000424 RID: 1060
		public readonly ImmutableDictionary<int, ImmutableArray<JobPrefab.JobItem>> JobItems;

		// Token: 0x04000425 RID: 1061
		public readonly List<SkillPrefab> Skills = new List<SkillPrefab>();

		// Token: 0x04000426 RID: 1062
		public readonly List<AutonomousObjective> AutonomousObjectives = new List<AutonomousObjective>();

		// Token: 0x04000427 RID: 1063
		public readonly List<Identifier> AppropriateOrders = new List<Identifier>();

		// Token: 0x04000429 RID: 1065
		public readonly LocalizedString Name;

		// Token: 0x0400042B RID: 1067
		public readonly LocalizedString Description;

		// Token: 0x04000436 RID: 1078
		public Sprite Icon;

		// Token: 0x04000437 RID: 1079
		public Sprite IconSmall;

		// Token: 0x0200070B RID: 1803
		public class JobItem
		{
			// Token: 0x06006786 RID: 26502 RVA: 0x0034A6F4 File Offset: 0x003488F4
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

			// Token: 0x06006787 RID: 26503 RVA: 0x0034A7B4 File Offset: 0x003489B4
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

			// Token: 0x0400388D RID: 14477
			public readonly Identifier ItemIdentifier;

			// Token: 0x0400388E RID: 14478
			public readonly Identifier ItemIdentifierTeam2;

			// Token: 0x0400388F RID: 14479
			public readonly bool ShowPreview;

			// Token: 0x04003890 RID: 14480
			public readonly bool Equip;

			// Token: 0x04003891 RID: 14481
			public readonly bool Outfit;

			// Token: 0x04003892 RID: 14482
			public readonly int Amount;

			// Token: 0x04003893 RID: 14483
			public readonly bool Infinite;

			// Token: 0x04003894 RID: 14484
			public readonly JobPrefab.JobItem ParentItem;

			// Token: 0x04003895 RID: 14485
			public readonly JobPrefab.JobItem.GameModeType GameMode;

			// Token: 0x0200150D RID: 5389
			public enum GameModeType
			{
				// Token: 0x0400674A RID: 26442
				Any,
				// Token: 0x0400674B RID: 26443
				PvP,
				// Token: 0x0400674C RID: 26444
				PvE
			}
		}
	}
}
