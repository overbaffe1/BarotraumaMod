using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200016C RID: 364
	[NullableContext(1)]
	[Nullable(0)]
	internal class AIObjectiveCheckStolenItems : AIObjective
	{
		// Token: 0x17000ADE RID: 2782
		// (get) Token: 0x06002B39 RID: 11065 RVA: 0x001DD0FC File Offset: 0x001DB2FC
		// (set) Token: 0x06002B3A RID: 11066 RVA: 0x001DD104 File Offset: 0x001DB304
		public override Identifier Identifier { get; set; } = "check stolen items".ToIdentifier();

		// Token: 0x17000ADF RID: 2783
		// (get) Token: 0x06002B3B RID: 11067 RVA: 0x001DD10D File Offset: 0x001DB30D
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000AE0 RID: 2784
		// (get) Token: 0x06002B3C RID: 11068 RVA: 0x001DD110 File Offset: 0x001DB310
		protected override bool AllowInAnySub
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002B3D RID: 11069 RVA: 0x001DD114 File Offset: 0x001DB314
		public AIObjectiveCheckStolenItems(Character character, Character target, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Target = target;
			this.InitTimers();
		}

		// Token: 0x06002B3E RID: 11070 RVA: 0x001DD167 File Offset: 0x001DB367
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x06002B3F RID: 11071 RVA: 0x001DD16C File Offset: 0x001DB36C
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

		// Token: 0x06002B40 RID: 11072 RVA: 0x001DD1ED File Offset: 0x001DB3ED
		public void ForceComplete()
		{
			base.IsCompleted = true;
		}

		// Token: 0x06002B41 RID: 11073 RVA: 0x001DD1F8 File Offset: 0x001DB3F8
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

		// Token: 0x06002B42 RID: 11074 RVA: 0x001DD26C File Offset: 0x001DB46C
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

		// Token: 0x06002B43 RID: 11075 RVA: 0x001DD4F0 File Offset: 0x001DB6F0
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

		// Token: 0x06002B44 RID: 11076 RVA: 0x001DD710 File Offset: 0x001DB910
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

		// Token: 0x06002B45 RID: 11077 RVA: 0x001DD76A File Offset: 0x001DB96A
		public override void OnDeselected()
		{
			base.OnDeselected();
			this.character.DeselectCharacter();
		}

		// Token: 0x06002B46 RID: 11078 RVA: 0x001DD77D File Offset: 0x001DB97D
		public override void Reset()
		{
			base.Reset();
			this.currentState = AIObjectiveCheckStolenItems.State.GotoTarget;
			this.InitTimers();
		}

		// Token: 0x06002B47 RID: 11079 RVA: 0x001DD792 File Offset: 0x001DB992
		private void InitTimers()
		{
			this.inspectTimer = 5f;
			this.currentWarnDelay = (this.Target.IsCriminal ? 3f : 5f);
			this.warnTimer = this.currentWarnDelay;
		}

		// Token: 0x06002B48 RID: 11080 RVA: 0x001DD7CC File Offset: 0x001DB9CC
		public static bool IsItemIllegitimate(Character owner, Item item)
		{
			return item.Illegitimate && (!item.HasTag(Tags.HandLockerItem) || !owner.HasEquippedItem(item, null, null));
		}

		// Token: 0x0400169C RID: 5788
		public float FindStolenItemsProbability = 1f;

		// Token: 0x0400169D RID: 5789
		private const float InspectTime = 5f;

		// Token: 0x0400169E RID: 5790
		private const float NormalWarnDelay = 5f;

		// Token: 0x0400169F RID: 5791
		private const float CriminalWarnDelay = 3f;

		// Token: 0x040016A0 RID: 5792
		private float inspectTimer;

		// Token: 0x040016A1 RID: 5793
		private float warnTimer;

		// Token: 0x040016A2 RID: 5794
		private float currentWarnDelay;

		// Token: 0x040016A3 RID: 5795
		private AIObjectiveCheckStolenItems.State currentState;

		// Token: 0x040016A4 RID: 5796
		public readonly Character Target;

		// Token: 0x040016A5 RID: 5797
		[Nullable(2)]
		private AIObjectiveGoTo goToObjective;

		// Token: 0x040016A6 RID: 5798
		private readonly List<Item> stolenItems = new List<Item>();

		// Token: 0x02000DF9 RID: 3577
		[NullableContext(0)]
		private enum State
		{
			// Token: 0x04005115 RID: 20757
			GotoTarget,
			// Token: 0x04005116 RID: 20758
			Inspect,
			// Token: 0x04005117 RID: 20759
			Warn,
			// Token: 0x04005118 RID: 20760
			Done
		}
	}
}
