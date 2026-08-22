using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000070 RID: 112
	internal class AIObjectiveFightIntruders : AIObjectiveLoop<Character>
	{
		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x06000F41 RID: 3905 RVA: 0x000907BB File Offset: 0x0008E9BB
		// (set) Token: 0x06000F42 RID: 3906 RVA: 0x000907C3 File Offset: 0x0008E9C3
		public override Identifier Identifier { get; set; } = "fight intruders".ToIdentifier();

		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x06000F43 RID: 3907 RVA: 0x000907CC File Offset: 0x0008E9CC
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x06000F44 RID: 3908 RVA: 0x000907D3 File Offset: 0x0008E9D3
		public override bool IgnoreUnsafeHulls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x06000F45 RID: 3909 RVA: 0x000907D6 File Offset: 0x0008E9D6
		protected override float TargetUpdateTimeMultiplier
		{
			get
			{
				return 0.2f;
			}
		}

		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x06000F46 RID: 3910 RVA: 0x000907DD File Offset: 0x0008E9DD
		// (set) Token: 0x06000F47 RID: 3911 RVA: 0x000907E5 File Offset: 0x0008E9E5
		public bool TargetCharactersInOtherSubs { get; set; }

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x06000F48 RID: 3912 RVA: 0x000907EE File Offset: 0x0008E9EE
		protected override bool AllowInAnySub
		{
			get
			{
				return this.TargetCharactersInOtherSubs;
			}
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x000907F8 File Offset: 0x0008E9F8
		public AIObjectiveFightIntruders(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x00090827 File Offset: 0x0008EA27
		protected override bool IsValidTarget(Character target)
		{
			return AIObjectiveFightIntruders.IsValidTarget(target, this.character, this.TargetCharactersInOtherSubs);
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x0009083B File Offset: 0x0008EA3B
		protected override IEnumerable<Character> GetList()
		{
			return Character.CharacterList;
		}

		// Token: 0x06000F4C RID: 3916 RVA: 0x00090844 File Offset: 0x0008EA44
		protected override float GetTargetPriority()
		{
			if (base.Targets.None(null))
			{
				return 0f;
			}
			if (!this.character.IsOnPlayerTeam && !this.character.IsOriginallyOnPlayerTeam)
			{
				return 100f;
			}
			if (this.character.IsSecurity)
			{
				return 100f;
			}
			if (this.objectiveManager.IsOrder(this))
			{
				return 100f;
			}
			return (float)(base.HumanAIController.IsTrueForAnyCrewMember((Character c) => c.IsSecurity, true, true) ? 0 : 100);
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x000908E0 File Offset: 0x0008EAE0
		protected override AIObjective ObjectiveConstructor(Character target)
		{
			AIObjectiveCombat.CombatMode combatMode = AIObjectiveCombat.CombatMode.Offensive;
			if (this.character.IsOnPlayerTeam && target != null && target.IsEscorted)
			{
				combatMode = AIObjectiveCombat.CombatMode.Arrest;
			}
			AIObjectiveCombat combatObjective = new AIObjectiveCombat(this.character, target, combatMode, this.objectiveManager, base.PriorityModifier, 10f);
			if (this.character.TeamID == CharacterTeamType.FriendlyNPC && target.TeamID == CharacterTeamType.Team1)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaignMode = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaignMode != null)
				{
					Location currentLocation = campaignMode.CurrentLocation;
					if (currentLocation != null && currentLocation.IsFactionHostile)
					{
						combatObjective.holdFireCondition = delegate()
						{
							if (this.character.GetDamageDoneByAttacker(target) > 0f)
							{
								return false;
							}
							if (target.CurrentHull != null)
							{
								return target.CurrentHull.OutpostModuleTags.Any((Identifier t) => t == "airlock");
							}
							return true;
						};
						this.character.Speak(TextManager.Get("dialogenteroutpostwarning").Value, null, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced), "leaveoutpostwarning".ToIdentifier(), 30f);
					}
				}
			}
			return combatObjective;
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x000909F2 File Offset: 0x0008EBF2
		protected override void OnObjectiveCompleted(AIObjective objective, Character target)
		{
			HumanAIController.RemoveTargets<AIObjectiveFightIntruders, Character>(this.character, target);
		}

		// Token: 0x06000F4F RID: 3919 RVA: 0x00090A00 File Offset: 0x0008EC00
		public static bool IsValidTarget(Character target, Character character, bool targetCharactersInOtherSubs)
		{
			return target != null && !target.Removed && !target.IsDead && !target.InDetectable && (!target.IsUnconscious || target.Params.Health.ConstantHealthRegeneration > 0f) && target != character && target.Submarine != null && character.Submarine != null && target.CurrentHull != null && !HumanAIController.IsFriendly(character, target, false, false) && character.Submarine.IsConnectedTo(target.Submarine) && (targetCharactersInOtherSubs || character.Submarine.TeamID == target.Submarine.TeamID || character.OriginalTeamID == target.Submarine.TeamID) && !target.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI) && !target.IsHandcuffed && !EnemyAIController.IsLatchedToSomeoneElse(target, character);
		}
	}
}
