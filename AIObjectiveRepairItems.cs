using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200018A RID: 394
	internal class AIObjectiveRepairItems : AIObjectiveLoop<Item>
	{
		// Token: 0x17000BE7 RID: 3047
		// (get) Token: 0x06002E7D RID: 11901 RVA: 0x001F0486 File Offset: 0x001EE686
		// (set) Token: 0x06002E7E RID: 11902 RVA: 0x001F048E File Offset: 0x001EE68E
		public override Identifier Identifier { get; set; } = "repair items".ToIdentifier();

		// Token: 0x17000BE8 RID: 3048
		// (get) Token: 0x06002E7F RID: 11903 RVA: 0x001F0497 File Offset: 0x001EE697
		// (set) Token: 0x06002E80 RID: 11904 RVA: 0x001F049F File Offset: 0x001EE69F
		public Item PrioritizedItem { get; private set; }

		// Token: 0x17000BE9 RID: 3049
		// (get) Token: 0x06002E81 RID: 11905 RVA: 0x001F04A8 File Offset: 0x001EE6A8
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BEA RID: 3050
		// (get) Token: 0x06002E82 RID: 11906 RVA: 0x001F04AB File Offset: 0x001EE6AB
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002E83 RID: 11907 RVA: 0x001F04B0 File Offset: 0x001EE6B0
		public override bool IsDuplicate<T>(T otherObjective)
		{
			AIObjectiveRepairItems repairObjective = otherObjective as AIObjectiveRepairItems;
			return repairObjective != null && this.objectiveManager.IsOrder(repairObjective) == this.objectiveManager.IsOrder(this);
		}

		// Token: 0x06002E84 RID: 11908 RVA: 0x001F04E8 File Offset: 0x001EE6E8
		public AIObjectiveRepairItems(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f, Item prioritizedItem = null) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.PrioritizedItem = prioritizedItem;
		}

		// Token: 0x06002E85 RID: 11909 RVA: 0x001F0520 File Offset: 0x001EE720
		protected override void CreateObjectives()
		{
			using (HashSet<Item>.Enumerator enumerator = base.Targets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					using (List<Repairable>.Enumerator enumerator2 = item.Repairables.GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							Repairable repairable = enumerator2.Current;
							AIObjective objective;
							if (!base.Objectives.TryGetValue(item, out objective))
							{
								objective = this.ObjectiveConstructor(item);
								base.Objectives.Add(item, objective);
								if (!this.subObjectives.Contains(objective))
								{
									this.subObjectives.Add(objective);
								}
								objective.Completed += delegate()
								{
									this.Objectives.Remove(item);
									this.OnObjectiveCompleted(objective, item);
								};
								AIObjective objective2 = objective;
								Action value;
								Action <>9__1;
								if ((value = <>9__1) == null)
								{
									value = (<>9__1 = delegate()
									{
										this.Objectives.Remove(item);
										this.ignoreList.Add(item);
										this.targetUpdateTimer = Math.Min(0.1f, this.targetUpdateTimer);
									});
								}
								objective2.Abandoned += value;
							}
						}
					}
				}
			}
		}

		// Token: 0x06002E86 RID: 11910 RVA: 0x001F06CC File Offset: 0x001EE8CC
		protected override bool IsValidTarget(Item item)
		{
			return AIObjectiveRepairItems.ViableForRepair(item, this.character, base.HumanAIController) && (base.Objectives.ContainsKey(item) || item == this.character.SelectedItem || !AIObjectiveRepairItems.NearlyFullCondition(item)) && (this.RelevantSkill.IsEmpty || !item.Repairables.None((Repairable r) => r.RequiredSkills.Any((Skill s) => s.Identifier == this.RelevantSkill))) && !AIObjectiveRepairItems.IsItemRepairedByAnother(this.character, item);
		}

		// Token: 0x06002E87 RID: 11911 RVA: 0x001F074C File Offset: 0x001EE94C
		public static bool ViableForRepair(Item item, Character character, HumanAIController humanAIController)
		{
			return AIObjectiveRepairItems.IsValidTarget(item, character) && (item.CurrentHull == null || (item.CurrentHull.FireSources.Count <= 0 && !Character.CharacterList.Any((Character c) => c.CurrentHull == item.CurrentHull && !humanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c))));
		}

		// Token: 0x06002E88 RID: 11912 RVA: 0x001F07C1 File Offset: 0x001EE9C1
		public static bool NearlyFullCondition(Item item)
		{
			return item.Repairables.All((Repairable r) => !r.IsBelowRepairThreshold);
		}

		// Token: 0x06002E89 RID: 11913 RVA: 0x001F07F0 File Offset: 0x001EE9F0
		protected override float GetTargetPriority()
		{
			Item selectedItem = this.character.SelectedItem;
			if (selectedItem != null && AIObjectiveRepairItem.IsRepairing(this.character, selectedItem) && selectedItem.ConditionPercentage < 100f)
			{
				return 100f;
			}
			int otherFixers = base.HumanAIController.CountBotsInTheCrew((HumanAIController c) => c != base.HumanAIController && c.ObjectiveManager.IsCurrentObjective<AIObjectiveRepairItems>());
			int items = base.Targets.Count;
			if (items == 0)
			{
				return 0f;
			}
			bool anyFixers = otherFixers > 0;
			float ratio = anyFixers ? ((float)items / (float)otherFixers) : 1f;
			if (this.objectiveManager.IsOrder(this))
			{
				return base.Targets.Sum((Item t) => 100f - t.ConditionPercentage);
			}
			if (anyFixers && (ratio <= 1f || otherFixers > 5 || (float)otherFixers / (float)base.HumanAIController.CountBotsInTheCrew(null) > 0.75f))
			{
				return 0f;
			}
			return base.Targets.Sum((Item t) => AIObjectiveRepairItems.GetTargetPriority(t, this.character, 0.4f)) * ratio;
		}

		// Token: 0x06002E8A RID: 11914 RVA: 0x001F08F0 File Offset: 0x001EEAF0
		public static float GetTargetPriority(Item item, Character character, float requiredSuccessFactor = 0f)
		{
			float damagePriority = MathHelper.Lerp(1f, 0f, item.Condition / item.MaxCondition);
			float successFactor = MathHelper.Lerp(0f, 1f, item.Repairables.Average((Repairable r) => r.DegreeOfSuccess(character)));
			if (successFactor < requiredSuccessFactor)
			{
				return 0f;
			}
			return MathHelper.Lerp(0f, 100f, MathHelper.Clamp(damagePriority * successFactor, 0f, 1f));
		}

		// Token: 0x06002E8B RID: 11915 RVA: 0x001F0979 File Offset: 0x001EEB79
		protected override IEnumerable<Item> GetList()
		{
			return Item.RepairableItems;
		}

		// Token: 0x06002E8C RID: 11916 RVA: 0x001F0980 File Offset: 0x001EEB80
		protected override AIObjective ObjectiveConstructor(Item item)
		{
			return new AIObjectiveRepairItem(this.character, item, this.objectiveManager, base.PriorityModifier, item == this.PrioritizedItem);
		}

		// Token: 0x06002E8D RID: 11917 RVA: 0x001F09A3 File Offset: 0x001EEBA3
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveRepairItems, Item>(this.character, target);
		}

		// Token: 0x06002E8E RID: 11918 RVA: 0x001F09B4 File Offset: 0x001EEBB4
		public static bool IsValidTarget(Item item, Character character)
		{
			return item != null && !item.IgnoreByAI(character) && item.IsInteractable(character) && !item.IsFullCondition && item.Submarine != null && character.Submarine != null && !item.IsClaimedByBallastFlora && (!character.IsOnPlayerTeam || !item.Submarine.Info.IsOutpost) && character.Submarine.IsEntityFoundOnThisSub(item, true, false, false) && !item.Repairables.None(null);
		}

		// Token: 0x06002E8F RID: 11919 RVA: 0x001F0A44 File Offset: 0x001EEC44
		public static bool IsItemRepairedByAnother(Character character, Item target)
		{
			AIObjectiveRepairItems.<>c__DisplayClass26_0 CS$<>8__locals1 = new AIObjectiveRepairItems.<>c__DisplayClass26_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.target = target;
			if (CS$<>8__locals1.target == null)
			{
				return false;
			}
			bool isOrder = CS$<>8__locals1.<IsItemRepairedByAnother>g__IsOrderedToPrioritizeTarget|0(CS$<>8__locals1.character.AIController as HumanAIController);
			using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Character c = enumerator.Current;
					if (HumanAIController.IsActive(c) && c != CS$<>8__locals1.character && c.TeamID == CS$<>8__locals1.character.TeamID)
					{
						if (c.IsPlayer)
						{
							if (CS$<>8__locals1.target.Repairables.Any((Repairable r) => r.CurrentFixer == c))
							{
								return true;
							}
						}
						else
						{
							HumanAIController otherAI = c.AIController as HumanAIController;
							if (otherAI != null)
							{
								AIObjectiveRepairItems repairItemsObjective = otherAI.ObjectiveManager.GetObjective<AIObjectiveRepairItems>();
								if (repairItemsObjective != null)
								{
									AIObjectiveRepairItem activeObjective = repairItemsObjective.SubObjectives.FirstOrDefault((AIObjective o) => o is AIObjectiveRepairItem) as AIObjectiveRepairItem;
									if (activeObjective != null && activeObjective.Item == CS$<>8__locals1.target)
									{
										bool isTargetOrdered = CS$<>8__locals1.<IsItemRepairedByAnother>g__IsOrderedToPrioritizeTarget|0(otherAI);
										if (!isOrder)
										{
											if (isTargetOrdered)
											{
												return true;
											}
										}
										else if (!isTargetOrdered)
										{
											continue;
										}
										if (otherAI.ObjectiveManager.CurrentObjective is AIObjectiveRepairItems)
										{
											IEnumerable<Repairable> repairables = CS$<>8__locals1.target.Repairables;
											Func<Repairable, float> selector;
											if ((selector = CS$<>8__locals1.<>9__3) == null)
											{
												selector = (CS$<>8__locals1.<>9__3 = ((Repairable r) => r.DegreeOfSuccess(CS$<>8__locals1.character)));
											}
											return repairables.Max(selector) <= CS$<>8__locals1.target.Repairables.Max((Repairable r) => r.DegreeOfSuccess(c));
										}
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x0400183F RID: 6207
		public Identifier RelevantSkill;

		// Token: 0x04001841 RID: 6209
		public const float RequiredSuccessFactor = 0.4f;
	}
}
