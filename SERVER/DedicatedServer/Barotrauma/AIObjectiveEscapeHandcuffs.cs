using System;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200006D RID: 109
	internal class AIObjectiveEscapeHandcuffs : AIObjective
	{
		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06000F11 RID: 3857 RVA: 0x0008F9C4 File Offset: 0x0008DBC4
		// (set) Token: 0x06000F12 RID: 3858 RVA: 0x0008F9CC File Offset: 0x0008DBCC
		public override Identifier Identifier { get; set; } = "escape handcuffs".ToIdentifier();

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x06000F13 RID: 3859 RVA: 0x0008F9D5 File Offset: 0x0008DBD5
		public override bool AllowAutomaticItemUnequipping
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x06000F14 RID: 3860 RVA: 0x0008F9D8 File Offset: 0x0008DBD8
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x06000F15 RID: 3861 RVA: 0x0008F9DB File Offset: 0x0008DBDB
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000F16 RID: 3862 RVA: 0x0008F9E0 File Offset: 0x0008DBE0
		public AIObjectiveEscapeHandcuffs(Character character, AIObjectiveManager objectiveManager, bool shouldSwitchTeams = true, bool beginInstantly = false, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.shouldSwitchTeams = shouldSwitchTeams;
			if (beginInstantly)
			{
				this.escapeTimer = 7.5f;
			}
		}

		// Token: 0x17000401 RID: 1025
		// (get) Token: 0x06000F17 RID: 3863 RVA: 0x0008FA31 File Offset: 0x0008DC31
		public override bool CanBeCompleted
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000F18 RID: 3864 RVA: 0x0008FA34 File Offset: 0x0008DC34
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x0008FA37 File Offset: 0x0008DC37
		protected override float GetPriority()
		{
			base.Priority = ((!this.isBeingWatched && this.character.LockHands) ? 59f : 0f);
			return base.Priority;
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x0008FA68 File Offset: 0x0008DC68
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

		// Token: 0x06000F1B RID: 3867 RVA: 0x0008FB80 File Offset: 0x0008DD80
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

		// Token: 0x06000F1C RID: 3868 RVA: 0x0008FC58 File Offset: 0x0008DE58
		public override void Reset()
		{
			base.Reset();
			this.escapeProgress = 0;
		}

		// Token: 0x04000711 RID: 1809
		private int escapeProgress;

		// Token: 0x04000712 RID: 1810
		private bool isBeingWatched;

		// Token: 0x04000713 RID: 1811
		private readonly bool shouldSwitchTeams;

		// Token: 0x04000714 RID: 1812
		private const string EscapeTeamChangeIdentifier = "escape";

		// Token: 0x04000715 RID: 1813
		private float escapeTimer = 60f;

		// Token: 0x04000716 RID: 1814
		private const float EscapeIntervalTimer = 7.5f;

		// Token: 0x04000717 RID: 1815
		private float updateTimer;

		// Token: 0x04000718 RID: 1816
		private const float UpdateIntervalTimer = 4f;
	}
}
