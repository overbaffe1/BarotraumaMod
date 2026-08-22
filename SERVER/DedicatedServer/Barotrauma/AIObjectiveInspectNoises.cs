using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200007A RID: 122
	internal class AIObjectiveInspectNoises : AIObjective
	{
		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x0600106E RID: 4206 RVA: 0x00097D5D File Offset: 0x00095F5D
		// (set) Token: 0x0600106F RID: 4207 RVA: 0x00097D65 File Offset: 0x00095F65
		public override Identifier Identifier { get; set; } = "inspect noises".ToIdentifier();

		// Token: 0x06001070 RID: 4208 RVA: 0x00097D6E File Offset: 0x00095F6E
		protected override float GetPriority()
		{
			AIObjectiveGoTo aiobjectiveGoTo = this.inspectNoiseObjective;
			if (aiobjectiveGoTo == null)
			{
				return 0f;
			}
			return aiobjectiveGoTo.Priority;
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x00097D88 File Offset: 0x00095F88
		public AIObjectiveInspectNoises(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.inspectNoiseTimer = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x00097DD0 File Offset: 0x00095FD0
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

		// Token: 0x06001073 RID: 4211 RVA: 0x00097E54 File Offset: 0x00096054
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

		// Token: 0x06001074 RID: 4212 RVA: 0x00098068 File Offset: 0x00096268
		protected override void Act(float deltaTime)
		{
		}

		// Token: 0x06001075 RID: 4213 RVA: 0x0009806A File Offset: 0x0009626A
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x00098070 File Offset: 0x00096270
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

		// Token: 0x040007DA RID: 2010
		private AIObjectiveGoTo inspectNoiseObjective;

		// Token: 0x040007DB RID: 2011
		private const float InspectNoisePriority = 10f;

		// Token: 0x040007DC RID: 2012
		private const float InspectNoisePriorityIncrease = 10f;

		// Token: 0x040007DD RID: 2013
		private const float InspectNoiseInterval = 1f;

		// Token: 0x040007DE RID: 2014
		private float inspectNoiseTimer;

		// Token: 0x040007DF RID: 2015
		private const float InspectNoiseExpirationDelay = 60f;

		// Token: 0x040007E0 RID: 2016
		private float inspectNoiseExpirationTimer;
	}
}
