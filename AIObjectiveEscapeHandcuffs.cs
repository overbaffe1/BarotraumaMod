using System;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000173 RID: 371
	internal class AIObjectiveEscapeHandcuffs : AIObjective
	{
		// Token: 0x17000B17 RID: 2839
		// (get) Token: 0x06002C19 RID: 11289 RVA: 0x001E2858 File Offset: 0x001E0A58
		// (set) Token: 0x06002C1A RID: 11290 RVA: 0x001E2860 File Offset: 0x001E0A60
		public override Identifier Identifier { get; set; } = "escape handcuffs".ToIdentifier();

		// Token: 0x17000B18 RID: 2840
		// (get) Token: 0x06002C1B RID: 11291 RVA: 0x001E2869 File Offset: 0x001E0A69
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B19 RID: 2841
		// (get) Token: 0x06002C1C RID: 11292 RVA: 0x001E286C File Offset: 0x001E0A6C
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B1A RID: 2842
		// (get) Token: 0x06002C1D RID: 11293 RVA: 0x001E286F File Offset: 0x001E0A6F
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002C1E RID: 11294 RVA: 0x001E2874 File Offset: 0x001E0A74
		public AIObjectiveEscapeHandcuffs(Character character, AIObjectiveManager objectiveManager, bool shouldSwitchTeams = true, bool beginInstantly = false, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.shouldSwitchTeams = shouldSwitchTeams;
			if (beginInstantly)
			{
				this.escapeTimer = 7.5f;
			}
		}

		// Token: 0x17000B1B RID: 2843
		// (get) Token: 0x06002C1F RID: 11295 RVA: 0x001E28C5 File Offset: 0x001E0AC5
		public override bool CanBeCompleted
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002C20 RID: 11296 RVA: 0x001E28C8 File Offset: 0x001E0AC8
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x06002C21 RID: 11297 RVA: 0x001E28CB File Offset: 0x001E0ACB
		protected override float GetPriority()
		{
			base.Priority = ((!this.isBeingWatched && this.character.LockHands) ? 59f : 0f);
			return base.Priority;
		}

		// Token: 0x06002C22 RID: 11298 RVA: 0x001E28FC File Offset: 0x001E0AFC
		public override void Update(float deltaTime)
		{
			this.updateTimer -= deltaTime;
			if (this.updateTimer <= 0f)
			{
				if (this.shouldSwitchTeams)
				{
					if (!this.character.LockHands)
					{
						if (!this.character.HasTeamChange("escape"))
						{
							this.character.TryAddNewTeamChange("escape", new ActiveTeamChange(CharacterTeamType.None, ActiveTeamChange.TeamChangePriorities.Willful, false));
						}
					}
					else
					{
						this.character.TryRemoveTeamChange("escape");
					}
				}
				this.isBeingWatched = false;
				foreach (Character otherCharacter in Character.CharacterList)
				{
					if (HumanAIController.IsActive(otherCharacter) && otherCharacter.TeamID == CharacterTeamType.Team1 && base.HumanAIController.VisibleHulls.Contains(otherCharacter.CurrentHull))
					{
						this.isBeingWatched = true;
						this.escapeProgress = 0;
						break;
					}
				}
				this.updateTimer = 4f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x06002C23 RID: 11299 RVA: 0x001E2A14 File Offset: 0x001E0C14
		protected override void Act(float deltaTime)
		{
			base.SteeringManager.Reset();
			this.escapeTimer -= deltaTime;
			if (this.escapeTimer <= 0f)
			{
				this.escapeProgress += Rand.Range(2, 5, Rand.RandSync.Unsynced);
				if (this.escapeProgress > 15)
				{
					foreach (Item it in this.character.HeldItems)
					{
						if (it.HasTag(Tags.HandLockerItem) && it.IsInteractable(this.character))
						{
							it.Drop(this.character, true, true);
						}
					}
				}
				this.escapeTimer = 7.5f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x06002C24 RID: 11300 RVA: 0x001E2AEC File Offset: 0x001E0CEC
		public override void Reset()
		{
			base.Reset();
			this.escapeProgress = 0;
		}

		// Token: 0x0400170B RID: 5899
		private int escapeProgress;

		// Token: 0x0400170C RID: 5900
		private bool isBeingWatched;

		// Token: 0x0400170D RID: 5901
		private readonly bool shouldSwitchTeams;

		// Token: 0x0400170E RID: 5902
		private const string EscapeTeamChangeIdentifier = "escape";

		// Token: 0x0400170F RID: 5903
		private float escapeTimer = 60f;

		// Token: 0x04001710 RID: 5904
		private const float EscapeIntervalTimer = 7.5f;

		// Token: 0x04001711 RID: 5905
		private float updateTimer;

		// Token: 0x04001712 RID: 5906
		private const float UpdateIntervalTimer = 4f;
	}
}
