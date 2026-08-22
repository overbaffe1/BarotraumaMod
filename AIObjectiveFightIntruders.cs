using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000176 RID: 374
	internal class AIObjectiveFightIntruders : AIObjectiveLoop<Character>
	{
		// Token: 0x17000B26 RID: 2854
		// (get) Token: 0x06002C49 RID: 11337 RVA: 0x001E364F File Offset: 0x001E184F
		// (set) Token: 0x06002C4A RID: 11338 RVA: 0x001E3657 File Offset: 0x001E1857
		public override Identifier Identifier { get; set; } = "fight intruders".ToIdentifier();

		// Token: 0x17000B27 RID: 2855
		// (get) Token: 0x06002C4B RID: 11339 RVA: 0x001E3660 File Offset: 0x001E1860
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x17000B28 RID: 2856
		// (get) Token: 0x06002C4C RID: 11340 RVA: 0x001E3667 File Offset: 0x001E1867
		public override bool IgnoreUnsafeHulls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B29 RID: 2857
		// (get) Token: 0x06002C4D RID: 11341 RVA: 0x001E366A File Offset: 0x001E186A
		protected override float TargetUpdateTimeMultiplier
		{
			get
			{
				return 0.2f;
			}
		}

		// Token: 0x17000B2A RID: 2858
		// (get) Token: 0x06002C4E RID: 11342 RVA: 0x001E3671 File Offset: 0x001E1871
		// (set) Token: 0x06002C4F RID: 11343 RVA: 0x001E3679 File Offset: 0x001E1879
		public bool TargetCharactersInOtherSubs { get; set; }

		// Token: 0x17000B2B RID: 2859
		// (get) Token: 0x06002C50 RID: 11344 RVA: 0x001E3682 File Offset: 0x001E1882
		protected override bool AllowInAnySub
		{
			get
			{
				return this.TargetCharactersInOtherSubs;
			}
		}

		// Token: 0x06002C51 RID: 11345 RVA: 0x001E368C File Offset: 0x001E188C
		public AIObjectiveFightIntruders(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06002C52 RID: 11346 RVA: 0x001E36BB File Offset: 0x001E18BB
		protected override bool IsValidTarget(Character target)
		{
			return AIObjectiveFightIntruders.IsValidTarget(target, this.character, this.TargetCharactersInOtherSubs);
		}

		// Token: 0x06002C53 RID: 11347 RVA: 0x001E36CF File Offset: 0x001E18CF
		protected override IEnumerable<Character> GetList()
		{
			return Character.CharacterList;
		}

		// Token: 0x06002C54 RID: 11348 RVA: 0x001E36D8 File Offset: 0x001E18D8
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

		// Token: 0x06002C55 RID: 11349 RVA: 0x001E3774 File Offset: 0x001E1974
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

		// Token: 0x06002C56 RID: 11350 RVA: 0x001E3886 File Offset: 0x001E1A86
		protected override void OnObjectiveCompleted(AIObjective objective, Character target)
		{
			HumanAIController.RemoveTargets<AIObjectiveFightIntruders, Character>(this.character, target);
		}

		// Token: 0x06002C57 RID: 11351 RVA: 0x001E3894 File Offset: 0x001E1A94
		public static bool IsValidTarget(Character target, Character character, bool targetCharactersInOtherSubs)
		{
			return target != null && !target.Removed && !target.IsDead && !target.InDetectable && (!target.IsUnconscious || target.Params.Health.ConstantHealthRegeneration > 0f) && target != character && target.Submarine != null && character.Submarine != null && target.CurrentHull != null && !HumanAIController.IsFriendly(character, target, false, false) && character.Submarine.IsConnectedTo(target.Submarine) && (targetCharactersInOtherSubs || character.Submarine.TeamID == target.Submarine.TeamID || character.OriginalTeamID == target.Submarine.TeamID) && !target.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI) && !target.IsHandcuffed && !EnemyAIController.IsLatchedToSomeoneElse(target, character);
		}
	}
}
