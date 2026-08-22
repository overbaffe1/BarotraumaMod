using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000180 RID: 384
	internal class AIObjectiveInspectNoises : AIObjective
	{
		// Token: 0x17000B90 RID: 2960
		// (get) Token: 0x06002D76 RID: 11638 RVA: 0x001EABF1 File Offset: 0x001E8DF1
		// (set) Token: 0x06002D77 RID: 11639 RVA: 0x001EABF9 File Offset: 0x001E8DF9
		public override Identifier Identifier { get; set; } = "inspect noises".ToIdentifier();

		// Token: 0x06002D78 RID: 11640 RVA: 0x001EAC02 File Offset: 0x001E8E02
		protected override float GetPriority()
		{
			AIObjectiveGoTo aiobjectiveGoTo = this.inspectNoiseObjective;
			if (aiobjectiveGoTo == null)
			{
				return 0f;
			}
			return aiobjectiveGoTo.Priority;
		}

		// Token: 0x06002D79 RID: 11641 RVA: 0x001EAC1C File Offset: 0x001E8E1C
		public AIObjectiveInspectNoises(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.inspectNoiseTimer = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06002D7A RID: 11642 RVA: 0x001EAC64 File Offset: 0x001E8E64
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			this.inspectNoiseTimer -= deltaTime;
			if (this.inspectNoiseTimer <= 0f)
			{
				this.CheckEnemyNoises();
				this.inspectNoiseTimer = 1f;
			}
			if (this.inspectNoiseObjective != null && this.objectiveManager.GetActiveObjective() != this.inspectNoiseObjective)
			{
				this.inspectNoiseExpirationTimer += deltaTime;
				if (this.inspectNoiseExpirationTimer > 60f)
				{
					this.inspectNoiseObjective.Abandon = true;
				}
			}
		}

		// Token: 0x06002D7B RID: 11643 RVA: 0x001EACE8 File Offset: 0x001E8EE8
		private void CheckEnemyNoises()
		{
			if (this.character.CurrentHull == null)
			{
				return;
			}
			if (this.inspectNoiseObjective != null && base.CurrentSubObjective != this.inspectNoiseObjective)
			{
				this.inspectNoiseObjective.Abandon = true;
			}
			foreach (AITarget aiTarget in AITarget.List)
			{
				if (!aiTarget.ShouldBeIgnored() && aiTarget.IsWithinSector(this.character.WorldPosition))
				{
					Item item = aiTarget.Entity as Item;
					if (item != null && item.HasTag(Tags.ProvocativeToHumanAI))
					{
						Character targetCharacter = item.GetRootInventoryOwner() as Character;
						if (targetCharacter != null && AIObjectiveFightIntruders.IsValidTarget(targetCharacter, this.character, false))
						{
							float range = aiTarget.SoundRange * base.HumanAIController.Hearing;
							float dist = this.character.CurrentHull.GetApproximateDistance(this.character.Position, targetCharacter.Position, targetCharacter.CurrentHull, range, 2f, 0.5f);
							if (dist <= range)
							{
								Character character = this.character;
								string value = TextManager.Get("dialogheardenemy").Value;
								Identifier identifier = "heardenemy".ToIdentifier();
								character.Speak(value, null, 0f, identifier, 30f);
								if (this.inspectNoiseObjective != null && this.subObjectives.Contains(this.inspectNoiseObjective))
								{
									this.inspectNoiseObjective.Priority = Math.Min(this.inspectNoiseObjective.Priority + 10f, 59f);
									if (this.objectiveManager.GetActiveObjective() != this.inspectNoiseObjective && this.inspectNoiseObjective.Target != targetCharacter.CurrentHull)
									{
										this.<CheckEnemyNoises>g__CreateInspectNoiseObjective|14_0(targetCharacter.CurrentHull, this.inspectNoiseObjective.Priority);
									}
								}
								else
								{
									this.<CheckEnemyNoises>g__CreateInspectNoiseObjective|14_0(targetCharacter.CurrentHull, 10f);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06002D7C RID: 11644 RVA: 0x001EAEFC File Offset: 0x001E90FC
		protected override void Act(float deltaTime)
		{
		}

		// Token: 0x06002D7D RID: 11645 RVA: 0x001EAEFE File Offset: 0x001E90FE
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x06002D7E RID: 11646 RVA: 0x001EAF04 File Offset: 0x001E9104
		[CompilerGenerated]
		private void <CheckEnemyNoises>g__CreateInspectNoiseObjective|14_0(ISpatialEntity target, float priority)
		{
			base.RemoveSubObjective<AIObjectiveGoTo>(ref this.inspectNoiseObjective);
			this.inspectNoiseObjective = new AIObjectiveGoTo(target, this.character, this.objectiveManager, false, true, 1f, 0f)
			{
				Priority = priority,
				SourceObjective = this
			};
			this.inspectNoiseObjective.Completed += delegate()
			{
				this.inspectNoiseObjective = null;
				this.inspectNoiseExpirationTimer = 0f;
			};
			this.inspectNoiseObjective.Abandoned += delegate()
			{
				this.inspectNoiseObjective = null;
				this.inspectNoiseExpirationTimer = 0f;
			};
			base.AddSubObjective(this.inspectNoiseObjective, false);
		}

		// Token: 0x040017D4 RID: 6100
		private AIObjectiveGoTo inspectNoiseObjective;

		// Token: 0x040017D5 RID: 6101
		private const float InspectNoisePriority = 10f;

		// Token: 0x040017D6 RID: 6102
		private const float InspectNoisePriorityIncrease = 10f;

		// Token: 0x040017D7 RID: 6103
		private const float InspectNoiseInterval = 1f;

		// Token: 0x040017D8 RID: 6104
		private float inspectNoiseTimer;

		// Token: 0x040017D9 RID: 6105
		private const float InspectNoiseExpirationDelay = 60f;

		// Token: 0x040017DA RID: 6106
		private float inspectNoiseExpirationTimer;
	}
}
