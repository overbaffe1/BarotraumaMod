using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200017A RID: 378
	internal class AIObjectiveFixLeak : AIObjective
	{
		// Token: 0x17000B40 RID: 2880
		// (get) Token: 0x06002C99 RID: 11417 RVA: 0x001E58D2 File Offset: 0x001E3AD2
		// (set) Token: 0x06002C9A RID: 11418 RVA: 0x001E58DA File Offset: 0x001E3ADA
		public override Identifier Identifier { get; set; } = "fix leak".ToIdentifier();

		// Token: 0x17000B41 RID: 2881
		// (get) Token: 0x06002C9B RID: 11419 RVA: 0x001E58E3 File Offset: 0x001E3AE3
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B42 RID: 2882
		// (get) Token: 0x06002C9C RID: 11420 RVA: 0x001E58E6 File Offset: 0x001E3AE6
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B43 RID: 2883
		// (get) Token: 0x06002C9D RID: 11421 RVA: 0x001E58E9 File Offset: 0x001E3AE9
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B44 RID: 2884
		// (get) Token: 0x06002C9E RID: 11422 RVA: 0x001E58EC File Offset: 0x001E3AEC
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B45 RID: 2885
		// (get) Token: 0x06002C9F RID: 11423 RVA: 0x001E58EF File Offset: 0x001E3AEF
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B46 RID: 2886
		// (get) Token: 0x06002CA0 RID: 11424 RVA: 0x001E58F2 File Offset: 0x001E3AF2
		// (set) Token: 0x06002CA1 RID: 11425 RVA: 0x001E58FA File Offset: 0x001E3AFA
		public Gap Leak { get; private set; }

		// Token: 0x06002CA2 RID: 11426 RVA: 0x001E5904 File Offset: 0x001E3B04
		public AIObjectiveFixLeak(Gap leak, Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool isPriority = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Leak = leak;
			this.isPriority = isPriority;
		}

		// Token: 0x06002CA3 RID: 11427 RVA: 0x001E5943 File Offset: 0x001E3B43
		protected override bool CheckObjectiveState()
		{
			return this.Leak.Open <= 0f || this.Leak.Removed;
		}

		// Token: 0x06002CA4 RID: 11428 RVA: 0x001E5964 File Offset: 0x001E3B64
		protected override float GetPriority()
		{
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
				return base.Priority;
			}
			float coopMultiplier = 1f;
			foreach (Character c in Character.CharacterList)
			{
				if (HumanAIController.IsActive(c) && c.TeamID == this.character.TeamID && c != this.character && !c.IsPlayer)
				{
					HumanAIController otherAI = c.AIController as HumanAIController;
					if (otherAI != null)
					{
						AIObjectiveFixLeak fixLeak = otherAI.ObjectiveManager.GetFirstActiveObjective<AIObjectiveFixLeak>();
						if (fixLeak != null)
						{
							if (fixLeak.Leak == this.Leak)
							{
								base.Priority = 0f;
								return base.Priority;
							}
							if (fixLeak.Leak.FlowTargetHull == this.Leak.FlowTargetHull)
							{
								coopMultiplier = 0.1f;
								break;
							}
						}
					}
				}
			}
			float reduction = (float)(this.isPriority ? 1 : 2);
			float maxPriority = 60f - reduction;
			if (this.operateObjective != null)
			{
				AIObjectiveFixLeaks fixLeaks = this.objectiveManager.GetFirstActiveObjective<AIObjectiveFixLeaks>();
				if (fixLeaks != null && fixLeaks.CurrentSubObjective == this)
				{
					base.Priority = maxPriority;
					goto IL_257;
				}
			}
			float xDist = Math.Abs(this.character.WorldPosition.X - this.Leak.WorldPosition.X);
			float yDist = Math.Abs(this.character.WorldPosition.Y - this.Leak.WorldPosition.Y);
			float distanceFactor = (this.isPriority || (xDist < 200f && yDist < 100f)) ? 1f : MathHelper.Lerp(1f, 0.1f, MathUtils.InverseLerp(0f, 3000f, xDist + yDist * 3f));
			if (this.Leak.linkedTo.Any(delegate(MapEntity e)
			{
				Hull h = e as Hull;
				return h != null && h == this.character.CurrentHull;
			}))
			{
				distanceFactor *= 2f;
			}
			float severity = this.isPriority ? 1f : (AIObjectiveFixLeaks.GetLeakSeverity(this.Leak) / 100f);
			float devotion = base.CumulatedDevotion / 100f;
			base.Priority = MathHelper.Lerp(0f, maxPriority, MathHelper.Clamp(devotion + severity * distanceFactor * base.PriorityModifier * coopMultiplier, 0f, 1f));
			IL_257:
			return base.Priority;
		}

		// Token: 0x06002CA5 RID: 11429 RVA: 0x001E5BE4 File Offset: 0x001E3DE4
		protected override void Act(float deltaTime)
		{
			AIObjectiveFixLeak.<>c__DisplayClass26_0 CS$<>8__locals1 = new AIObjectiveFixLeak.<>c__DisplayClass26_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.weldingTool = this.character.Inventory.FindItemByTag("weldingequipment".ToIdentifier(), true);
			AIObjectiveFixLeak.<>c__DisplayClass26_0 CS$<>8__locals2 = CS$<>8__locals1;
			Item weldingTool = CS$<>8__locals1.weldingTool;
			CS$<>8__locals2.repairTool = ((weldingTool != null) ? weldingTool.GetComponent<RepairTool>() : null);
			if (CS$<>8__locals1.weldingTool == null)
			{
				base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getWeldingTool, () => new AIObjectiveGetItem(CS$<>8__locals1.<>4__this.character, "weldingequipment".ToIdentifier(), CS$<>8__locals1.<>4__this.objectiveManager, true, true, 1f, CS$<>8__locals1.<>4__this.character.TeamID == CharacterTeamType.FriendlyNPC), delegate
				{
					CS$<>8__locals1.<>4__this.RemoveSubObjective<AIObjectiveGetItem>(ref CS$<>8__locals1.<>4__this.getWeldingTool);
				}, delegate
				{
					if (CS$<>8__locals1.<>4__this.character.IsOnPlayerTeam && CS$<>8__locals1.<>4__this.objectiveManager.IsCurrentOrder<AIObjectiveFixLeaks>())
					{
						CS$<>8__locals1.<>4__this.character.Speak(TextManager.Get("dialogcannotfindweldingequipment").Value, null, 0f, "dialogcannotfindweldingequipment".ToIdentifier(), 10f);
					}
					CS$<>8__locals1.<>4__this.Abandon = true;
				});
				return;
			}
			if (CS$<>8__locals1.repairTool == null)
			{
				base.Abandon = true;
				return;
			}
			if (CS$<>8__locals1.weldingTool.OwnInventory == null)
			{
				if (CS$<>8__locals1.repairTool.RequiredItems.Any((KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> r) => r.Key == RelatedItem.RelationType.Contained))
				{
					base.Abandon = true;
					return;
				}
			}
			if (CS$<>8__locals1.weldingTool.OwnInventory != null)
			{
				if (CS$<>8__locals1.weldingTool.OwnInventory.AllItems.None((Item i) => i.HasTag(Tags.WeldingFuel) && i.Condition > 0f))
				{
					base.TryAddSubObjective<AIObjectiveContainItem>(ref this.refuelObjective, () => new AIObjectiveContainItem(CS$<>8__locals1.<>4__this.character, Tags.WeldingFuel, CS$<>8__locals1.weldingTool.GetComponent<ItemContainer>(), CS$<>8__locals1.<>4__this.objectiveManager, 1f, CS$<>8__locals1.<>4__this.character.TeamID == CharacterTeamType.FriendlyNPC)
					{
						RemoveExisting = true
					}, delegate
					{
						CS$<>8__locals1.<>4__this.RemoveSubObjective<AIObjectiveContainItem>(ref CS$<>8__locals1.<>4__this.refuelObjective);
						base.<Act>g__ReportWeldingFuelTankCount|8();
					}, delegate
					{
						CS$<>8__locals1.<>4__this.Abandon = true;
						base.<Act>g__ReportWeldingFuelTankCount|8();
					});
					return;
				}
			}
			if (this.subObjectives.Any<AIObjective>())
			{
				return;
			}
			Vector2 toLeak = this.Leak.WorldPosition - this.character.AnimController.AimSourceWorldPos;
			if (!this.character.AnimController.InWater && Math.Abs(toLeak.X) < 100f && toLeak.Y < 0f && toLeak.Y > -150f)
			{
				base.HumanAIController.AnimController.Crouch();
			}
			CS$<>8__locals1.reach = AIObjectiveFixLeak.CalculateReach(CS$<>8__locals1.repairTool, this.character);
			bool canOperate = toLeak.LengthSquared() < CS$<>8__locals1.reach * CS$<>8__locals1.reach;
			if (canOperate)
			{
				base.TryAddSubObjective<AIObjectiveOperateItem>(ref this.operateObjective, delegate
				{
					AIObjectiveOperateItem aiobjectiveOperateItem = new AIObjectiveOperateItem(CS$<>8__locals1.repairTool, CS$<>8__locals1.<>4__this.character, CS$<>8__locals1.<>4__this.objectiveManager, Identifier.Empty, true, CS$<>8__locals1.<>4__this.Leak, false, null, 1f);
					aiobjectiveOperateItem.EndNodeFilter = ((PathNode n) => true);
					return aiobjectiveOperateItem;
				}, delegate
				{
					if (CS$<>8__locals1.<>4__this.CheckObjectiveState())
					{
						CS$<>8__locals1.<>4__this.IsCompleted = true;
						return;
					}
					CS$<>8__locals1.<>4__this.Abandon = true;
				}, delegate
				{
					CS$<>8__locals1.<>4__this.Abandon = true;
				});
				return;
			}
			base.TryAddSubObjective<AIObjectiveGoTo>(ref this.gotoObjective, delegate
			{
				AIObjectiveGoTo aiobjectiveGoTo = new AIObjectiveGoTo(CS$<>8__locals1.<>4__this.Leak, CS$<>8__locals1.<>4__this.character, CS$<>8__locals1.<>4__this.objectiveManager, false, true, 1f, 0f);
				aiobjectiveGoTo.UseDistanceRelativeToAimSourcePos = true;
				aiobjectiveGoTo.CloseEnough = CS$<>8__locals1.reach;
				aiobjectiveGoTo.DialogueIdentifier = ((CS$<>8__locals1.<>4__this.Leak.FlowTargetHull != null) ? AIObjectiveGoTo.DialogCannotReachLeak : Identifier.Empty);
				Hull flowTargetHull = CS$<>8__locals1.<>4__this.Leak.FlowTargetHull;
				aiobjectiveGoTo.TargetName = ((flowTargetHull != null) ? flowTargetHull.DisplayName : null);
				Func<bool> requiredCondition;
				if ((requiredCondition = CS$<>8__locals1.<>9__18) == null)
				{
					requiredCondition = (CS$<>8__locals1.<>9__18 = delegate()
					{
						if (CS$<>8__locals1.<>4__this.Leak.Submarine == CS$<>8__locals1.<>4__this.character.Submarine)
						{
							IEnumerable<MapEntity> linkedTo = CS$<>8__locals1.<>4__this.Leak.linkedTo;
							Func<MapEntity, bool> predicate;
							if ((predicate = CS$<>8__locals1.<>9__20) == null)
							{
								predicate = (CS$<>8__locals1.<>9__20 = delegate(MapEntity e)
								{
									Hull h = e as Hull;
									return h != null && (CS$<>8__locals1.<>4__this.character.CurrentHull == h || h.linkedTo.Contains(CS$<>8__locals1.<>4__this.character.CurrentHull));
								});
							}
							return linkedTo.Any(predicate);
						}
						return false;
					});
				}
				aiobjectiveGoTo.requiredCondition = requiredCondition;
				aiobjectiveGoTo.endNodeFilter = new Func<PathNode, bool>(base.<Act>g__IsSuitableEndNode|15);
				Func<bool> speakCannotReachCondition;
				if ((speakCannotReachCondition = CS$<>8__locals1.<>9__19) == null)
				{
					speakCannotReachCondition = (CS$<>8__locals1.<>9__19 = (() => CS$<>8__locals1.<>4__this.isPriority && !CS$<>8__locals1.<>4__this.CheckObjectiveState()));
				}
				aiobjectiveGoTo.SpeakCannotReachCondition = speakCannotReachCondition;
				return aiobjectiveGoTo;
			}, delegate
			{
				CS$<>8__locals1.<>4__this.RemoveSubObjective<AIObjectiveGoTo>(ref CS$<>8__locals1.<>4__this.gotoObjective);
			}, delegate
			{
				if (CS$<>8__locals1.<>4__this.CheckObjectiveState())
				{
					CS$<>8__locals1.<>4__this.IsCompleted = true;
					return;
				}
				if ((CS$<>8__locals1.<>4__this.Leak.WorldPosition - CS$<>8__locals1.<>4__this.character.AnimController.AimSourceWorldPos).LengthSquared() > MathUtils.Pow(CS$<>8__locals1.reach * 2f, 2f))
				{
					CS$<>8__locals1.<>4__this.Abandon = true;
					return;
				}
				CS$<>8__locals1.<>4__this.RemoveSubObjective<AIObjectiveGoTo>(ref CS$<>8__locals1.<>4__this.gotoObjective);
			});
		}

		// Token: 0x06002CA6 RID: 11430 RVA: 0x001E5E4D File Offset: 0x001E404D
		public override void Reset()
		{
			base.Reset();
			this.getWeldingTool = null;
			this.refuelObjective = null;
			this.gotoObjective = null;
			this.operateObjective = null;
		}

		// Token: 0x06002CA7 RID: 11431 RVA: 0x001E5E74 File Offset: 0x001E4074
		public static float CalculateReach(RepairTool repairTool, Character character)
		{
			float armLength = ConvertUnits.ToDisplayUnits(((HumanoidAnimController)character.AnimController).ArmLength);
			return repairTool.Range + armLength * 2f;
		}

		// Token: 0x04001747 RID: 5959
		private AIObjectiveGetItem getWeldingTool;

		// Token: 0x04001748 RID: 5960
		private AIObjectiveContainItem refuelObjective;

		// Token: 0x04001749 RID: 5961
		private AIObjectiveGoTo gotoObjective;

		// Token: 0x0400174A RID: 5962
		private AIObjectiveOperateItem operateObjective;

		// Token: 0x0400174B RID: 5963
		public readonly bool isPriority;
	}
}
