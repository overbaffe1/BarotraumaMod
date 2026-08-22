using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000066 RID: 102
	[NullableContext(1)]
	[Nullable(0)]
	internal class AIObjectiveCheckStolenItems : AIObjective
	{
		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x06000E31 RID: 3633 RVA: 0x0008A268 File Offset: 0x00088468
		// (set) Token: 0x06000E32 RID: 3634 RVA: 0x0008A270 File Offset: 0x00088470
		public override Identifier Identifier { get; set; } = "check stolen items".ToIdentifier();

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x06000E33 RID: 3635 RVA: 0x0008A279 File Offset: 0x00088479
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x06000E34 RID: 3636 RVA: 0x0008A27C File Offset: 0x0008847C
		protected override bool AllowInAnySub
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x0008A280 File Offset: 0x00088480
		public AIObjectiveCheckStolenItems(Character character, Character target, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Target = target;
			this.InitTimers();
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x0008A2D3 File Offset: 0x000884D3
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x0008A2D8 File Offset: 0x000884D8
		protected override float GetPriority()
		{
			if (!base.Abandon && !base.IsCompleted && this.objectiveManager.IsOrder(this))
			{
				base.Priority = this.objectiveManager.GetOrderPriority(this);
			}
			else if (base.HumanAIController.CurrentHullSafety < 40f || HumanAIController.CalculateObjectiveHullSafety(this.Target) < 40f)
			{
				base.Priority = 0f;
			}
			else
			{
				base.Priority = 59f;
			}
			return base.Priority;
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x0008A359 File Offset: 0x00088559
		public void ForceComplete()
		{
			base.IsCompleted = true;
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x0008A364 File Offset: 0x00088564
		protected override void Act(float deltaTime)
		{
			switch (this.currentState)
			{
			case AIObjectiveCheckStolenItems.State.GotoTarget:
				base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(this.Target, this.character, this.objectiveManager, false, true, 1f, 0f)
				{
					SpeakIfFails = false
				}, delegate
				{
					base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
					if (this.character.IsClimbing || base.HumanAIController.CurrentHullSafety < 40f || HumanAIController.CalculateObjectiveHullSafety(this.Target) < 40f)
					{
						base.Abandon = true;
						return;
					}
					this.currentState = AIObjectiveCheckStolenItems.State.Inspect;
					this.stolenItems.Clear();
					this.Target.Inventory.FindAllItems((Item it) => AIObjectiveCheckStolenItems.IsItemIllegitimate(this.Target, it), true, this.stolenItems);
					this.character.Speak(TextManager.Get(this.Target.IsCriminal ? "dialogcheckstolenitems.criminal" : "dialogcheckstolenitems").Value, null, 0f, default(Identifier), 0f);
				}, delegate
				{
					base.Abandon = true;
				});
				return;
			case AIObjectiveCheckStolenItems.State.Inspect:
				this.Inspect(deltaTime);
				return;
			case AIObjectiveCheckStolenItems.State.Warn:
				this.Warn(deltaTime);
				return;
			case AIObjectiveCheckStolenItems.State.Done:
				base.IsCompleted = true;
				return;
			default:
				return;
			}
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x0008A3D8 File Offset: 0x000885D8
		private void Inspect(float deltaTime)
		{
			if (this.inspectTimer > 0f)
			{
				Vector2 diff = this.Target.WorldPosition - this.character.WorldPosition;
				float dist = diff.Length();
				float maxDist = ConvertUnits.ToDisplayUnits(1.4f);
				if (dist <= maxDist)
				{
					if (dist < maxDist * 0.5f)
					{
						this.character.AIController.SteeringManager.Reset();
					}
					this.character.SelectCharacter(this.Target);
					this.inspectTimer -= deltaTime;
					if (this.inspectTimer < 4f && Math.Abs(this.Target.AnimController.TargetMovement.X) > 1f)
					{
						Character character = this.character;
						string value = TextManager.Get("dialogcheckstolenitems.holdstill").Value;
						Identifier identifier = "holdstill".ToIdentifier();
						character.Speak(value, null, 0f, identifier, 3f);
					}
					return;
				}
				if (dist > maxDist * 2f || !this.character.CanSeeTarget(this.Target, null, false, false))
				{
					this.currentState = AIObjectiveCheckStolenItems.State.GotoTarget;
					return;
				}
				if (Math.Abs(diff.X) > Math.Abs(diff.Y) * 2f)
				{
					this.character.AIController.SteeringManager.SteeringManual(deltaTime, new Vector2((float)MathF.Sign(this.Target.WorldPosition.X - this.character.WorldPosition.X), 0f));
					return;
				}
				this.character.AIController.SteeringManager.Reset();
				return;
			}
			else
			{
				if (this.character.SelectedCharacter != this.Target)
				{
					base.Abandon = true;
					return;
				}
				if (this.stolenItems.Any<Item>() && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.FindStolenItemsProbability)
				{
					this.character.Speak(TextManager.Get("dialogcheckstolenitems.warn").Value, null, 0f, default(Identifier), 0f);
					this.currentState = AIObjectiveCheckStolenItems.State.Warn;
				}
				else
				{
					this.character.Speak(TextManager.Get(this.Target.IsCriminal ? "dialogcheckstolenitems.nostolenitems.criminal" : "dialogcheckstolenitems.nostolenitems").Value, null, 0f, default(Identifier), 0f);
					this.currentState = AIObjectiveCheckStolenItems.State.Done;
					base.IsCompleted = true;
				}
				this.character.DeselectCharacter();
				return;
			}
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x0008A65C File Offset: 0x0008885C
		private void Warn(float deltaTime)
		{
			if (this.warnTimer > 0f)
			{
				this.warnTimer -= deltaTime;
				return;
			}
			IEnumerable<Item> stolenItemsOnCharacter = from it in this.stolenItems
			where it.GetRootInventoryOwner() == this.Target
			select it;
			if (stolenItemsOnCharacter.Any<Item>())
			{
				if (this.Target.IsBot)
				{
					foreach (Item item in stolenItemsOnCharacter)
					{
						item.Drop(this.Target, true, true);
					}
					this.character.Speak(TextManager.Get("dialogcheckstolenitems.comply").Value, null, 0f, default(Identifier), 0f);
					goto IL_175;
				}
				this.character.Speak(TextManager.Get(this.character.IsCriminal ? "dialogcheckstolenitems.arrest.criminal" : "dialogcheckstolenitems.arrest").Value, null, 0f, default(Identifier), 0f);
				this.Arrest(true, true);
				using (IEnumerator<Item> enumerator2 = stolenItemsOnCharacter.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Item stolenItem = enumerator2.Current;
						HumanAIController.ApplyStealingReputationLoss(stolenItem);
					}
					goto IL_175;
				}
			}
			this.character.Speak(TextManager.Get("dialogcheckstolenitems.comply").Value, null, 0f, default(Identifier), 0f);
			IL_175:
			foreach (Item item2 in this.stolenItems)
			{
				base.HumanAIController.ObjectiveManager.AddObjective<AIObjectiveGetItem>(new AIObjectiveGetItem(this.character, item2, this.objectiveManager, false, 1f)
				{
					BasePriority = 10f
				});
			}
			this.currentState = AIObjectiveCheckStolenItems.State.Done;
			base.IsCompleted = true;
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x0008A87C File Offset: 0x00088A7C
		private void Arrest(bool abortWhenItemsDropped, bool allowHoldFire)
		{
			bool isCriminal = this.Target.IsCriminal;
			Func<AIObjective, bool> abortCondition = null;
			if (abortWhenItemsDropped && !isCriminal)
			{
				abortCondition = ((AIObjective obj) => this.Target.Inventory.FindItem((Item it) => it.Illegitimate, true) == null);
			}
			HumanAIController humanAIController = base.HumanAIController;
			AIObjectiveCombat.CombatMode mode = AIObjectiveCombat.CombatMode.Arrest;
			Character target = this.Target;
			float delay = 0f;
			bool allowHoldFire2 = allowHoldFire && !isCriminal;
			bool speakWarnings = !isCriminal;
			humanAIController.AddCombatObjective(mode, target, delay, abortCondition, null, null, allowHoldFire2, speakWarnings);
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x0008A8D6 File Offset: 0x00088AD6
		public override void OnDeselected()
		{
			base.OnDeselected();
			this.character.DeselectCharacter();
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x0008A8E9 File Offset: 0x00088AE9
		public override void Reset()
		{
			base.Reset();
			this.currentState = AIObjectiveCheckStolenItems.State.GotoTarget;
			this.InitTimers();
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x0008A8FE File Offset: 0x00088AFE
		private void InitTimers()
		{
			this.inspectTimer = 5f;
			this.currentWarnDelay = (this.Target.IsCriminal ? 3f : 5f);
			this.warnTimer = this.currentWarnDelay;
		}

		// Token: 0x06000E40 RID: 3648 RVA: 0x0008A938 File Offset: 0x00088B38
		public static bool IsItemIllegitimate(Character owner, Item item)
		{
			return item.Illegitimate && (!item.HasTag(Tags.HandLockerItem) || !owner.HasEquippedItem(item, null, null));
		}

		// Token: 0x040006A2 RID: 1698
		public float FindStolenItemsProbability = 1f;

		// Token: 0x040006A3 RID: 1699
		private const float InspectTime = 5f;

		// Token: 0x040006A4 RID: 1700
		private const float NormalWarnDelay = 5f;

		// Token: 0x040006A5 RID: 1701
		private const float CriminalWarnDelay = 3f;

		// Token: 0x040006A6 RID: 1702
		private float inspectTimer;

		// Token: 0x040006A7 RID: 1703
		private float warnTimer;

		// Token: 0x040006A8 RID: 1704
		private float currentWarnDelay;

		// Token: 0x040006A9 RID: 1705
		private AIObjectiveCheckStolenItems.State currentState;

		// Token: 0x040006AA RID: 1706
		public readonly Character Target;

		// Token: 0x040006AB RID: 1707
		[Nullable(2)]
		private AIObjectiveGoTo goToObjective;

		// Token: 0x040006AC RID: 1708
		private readonly List<Item> stolenItems = new List<Item>();

		// Token: 0x020007B1 RID: 1969
		[NullableContext(0)]
		private enum State
		{
			// Token: 0x04002DB5 RID: 11701
			GotoTarget,
			// Token: 0x04002DB6 RID: 11702
			Inspect,
			// Token: 0x04002DB7 RID: 11703
			Warn,
			// Token: 0x04002DB8 RID: 11704
			Done
		}
	}
}
