using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000084 RID: 132
	internal class AIObjectiveRepairItems : AIObjectiveLoop<Item>
	{
		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x06001175 RID: 4469 RVA: 0x0009D5EE File Offset: 0x0009B7EE
		// (set) Token: 0x06001176 RID: 4470 RVA: 0x0009D5F6 File Offset: 0x0009B7F6
		public override Identifier Identifier { get; set; } = "repair items".ToIdentifier();

		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x06001177 RID: 4471 RVA: 0x0009D5FF File Offset: 0x0009B7FF
		// (set) Token: 0x06001178 RID: 4472 RVA: 0x0009D607 File Offset: 0x0009B807
		public Item PrioritizedItem { get; private set; }

		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x06001179 RID: 4473 RVA: 0x0009D610 File Offset: 0x0009B810
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x0600117A RID: 4474 RVA: 0x0009D613 File Offset: 0x0009B813
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600117B RID: 4475 RVA: 0x0009D618 File Offset: 0x0009B818
		public override bool IsDuplicate<T>(T otherObjective)
		{
			AIObjectiveRepairItems repairObjective = otherObjective as AIObjectiveRepairItems;
			return repairObjective != null && this.objectiveManager.IsOrder(repairObjective) == this.objectiveManager.IsOrder(this);
		}

		// Token: 0x0600117C RID: 4476 RVA: 0x0009D650 File Offset: 0x0009B850
		public AIObjectiveRepairItems(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f, Item prioritizedItem = null) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.PrioritizedItem = prioritizedItem;
		}

		// Token: 0x0600117D RID: 4477 RVA: 0x0009D688 File Offset: 0x0009B888
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

		// Token: 0x0600117E RID: 4478 RVA: 0x0009D834 File Offset: 0x0009BA34
		protected override bool IsValidTarget(Item item)
		{
			return AIObjectiveRepairItems.ViableForRepair(item, this.character, base.HumanAIController) && (base.Objectives.ContainsKey(item) || item == this.character.SelectedItem || !AIObjectiveRepairItems.NearlyFullCondition(item)) && (this.RelevantSkill.IsEmpty || !item.Repairables.None((Repairable r) => r.RequiredSkills.Any((Skill s) => s.Identifier == this.RelevantSkill))) && !AIObjectiveRepairItems.IsItemRepairedByAnother(this.character, item);
		}

		// Token: 0x0600117F RID: 4479 RVA: 0x0009D8B4 File Offset: 0x0009BAB4
		public static bool ViableForRepair(Item item, Character character, HumanAIController humanAIController)
		{
			return AIObjectiveRepairItems.IsValidTarget(item, character) && (item.CurrentHull == null || (item.CurrentHull.FireSources.Count <= 0 && !Character.CharacterList.Any((Character c) => c.CurrentHull == item.CurrentHull && !humanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c))));
		}

		// Token: 0x06001180 RID: 4480 RVA: 0x0009D929 File Offset: 0x0009BB29
		public static bool NearlyFullCondition(Item item)
		{
			return item.Repairables.All((Repairable r) => !r.IsBelowRepairThreshold);
		}

		// Token: 0x06001181 RID: 4481 RVA: 0x0009D958 File Offset: 0x0009BB58
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

		// Token: 0x06001182 RID: 4482 RVA: 0x0009DA58 File Offset: 0x0009BC58
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

		// Token: 0x06001183 RID: 4483 RVA: 0x0009DAE1 File Offset: 0x0009BCE1
		protected override IEnumerable<Item> GetList()
		{
			return Item.RepairableItems;
		}

		// Token: 0x06001184 RID: 4484 RVA: 0x0009DAE8 File Offset: 0x0009BCE8
		protected override AIObjective ObjectiveConstructor(Item item)
		{
			return new AIObjectiveRepairItem(this.character, item, this.objectiveManager, base.PriorityModifier, item == this.PrioritizedItem);
		}

		// Token: 0x06001185 RID: 4485 RVA: 0x0009DB0B File Offset: 0x0009BD0B
		protected override void OnObjectiveCompleted(AIObjective objective, Item target)
		{
			HumanAIController.RemoveTargets<AIObjectiveRepairItems, Item>(this.character, target);
		}

		// Token: 0x06001186 RID: 4486 RVA: 0x0009DB1C File Offset: 0x0009BD1C
		public static bool IsValidTarget(Item item, Character character)
		{
			return item != null && !item.IgnoreByAI(character) && item.IsInteractable(character) && !item.IsFullCondition && item.Submarine != null && character.Submarine != null && !item.IsClaimedByBallastFlora && (!character.IsOnPlayerTeam || !item.Submarine.Info.IsOutpost) && character.Submarine.IsEntityFoundOnThisSub(item, true, false, false) && !item.Repairables.None(null);
		}

		// Token: 0x06001187 RID: 4487 RVA: 0x0009DBAC File Offset: 0x0009BDAC
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

		// Token: 0x04000845 RID: 2117
		public Identifier RelevantSkill;

		// Token: 0x04000847 RID: 2119
		public const float RequiredSuccessFactor = 0.4f;
	}
}
