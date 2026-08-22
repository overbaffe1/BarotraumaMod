using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000074 RID: 116
	internal class AIObjectiveFixLeak : AIObjective
	{
		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06000F91 RID: 3985 RVA: 0x00092A3E File Offset: 0x00090C3E
		// (set) Token: 0x06000F92 RID: 3986 RVA: 0x00092A46 File Offset: 0x00090C46
		public override Identifier Identifier { get; set; } = "fix leak".ToIdentifier();

		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06000F93 RID: 3987 RVA: 0x00092A4F File Offset: 0x00090C4F
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x06000F94 RID: 3988 RVA: 0x00092A52 File Offset: 0x00090C52
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x06000F95 RID: 3989 RVA: 0x00092A55 File Offset: 0x00090C55
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x06000F96 RID: 3990 RVA: 0x00092A58 File Offset: 0x00090C58
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x06000F97 RID: 3991 RVA: 0x00092A5B File Offset: 0x00090C5B
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x06000F98 RID: 3992 RVA: 0x00092A5E File Offset: 0x00090C5E
		// (set) Token: 0x06000F99 RID: 3993 RVA: 0x00092A66 File Offset: 0x00090C66
		public Gap Leak { get; private set; }

		// Token: 0x06000F9A RID: 3994 RVA: 0x00092A70 File Offset: 0x00090C70
		public AIObjectiveFixLeak(Gap leak, Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f, bool isPriority = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Leak = leak;
			this.isPriority = isPriority;
		}

		// Token: 0x06000F9B RID: 3995 RVA: 0x00092AAF File Offset: 0x00090CAF
		protected override bool CheckObjectiveState()
		{
			return this.Leak.Open <= 0f || this.Leak.Removed;
		}

		// Token: 0x06000F9C RID: 3996 RVA: 0x00092AD0 File Offset: 0x00090CD0
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

		// Token: 0x06000F9D RID: 3997 RVA: 0x00092D50 File Offset: 0x00090F50
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

		// Token: 0x06000F9E RID: 3998 RVA: 0x00092FB9 File Offset: 0x000911B9
		public override void Reset()
		{
			base.Reset();
			this.getWeldingTool = null;
			this.refuelObjective = null;
			this.gotoObjective = null;
			this.operateObjective = null;
		}

		// Token: 0x06000F9F RID: 3999 RVA: 0x00092FE0 File Offset: 0x000911E0
		public static float CalculateReach(RepairTool repairTool, Character character)
		{
			float armLength = ConvertUnits.ToDisplayUnits(((HumanoidAnimController)character.AnimController).ArmLength);
			return repairTool.Range + armLength * 2f;
		}

		// Token: 0x0400074D RID: 1869
		private AIObjectiveGetItem getWeldingTool;

		// Token: 0x0400074E RID: 1870
		private AIObjectiveContainItem refuelObjective;

		// Token: 0x0400074F RID: 1871
		private AIObjectiveGoTo gotoObjective;

		// Token: 0x04000750 RID: 1872
		private AIObjectiveOperateItem operateObjective;

		// Token: 0x04000751 RID: 1873
		public readonly bool isPriority;
	}
}
