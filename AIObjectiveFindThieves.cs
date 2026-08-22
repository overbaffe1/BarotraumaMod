using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000179 RID: 377
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal class AIObjectiveFindThieves : AIObjectiveLoop<Character>
	{
		// Token: 0x17000B3B RID: 2875
		// (get) Token: 0x06002C87 RID: 11399 RVA: 0x001E53BE File Offset: 0x001E35BE
		// (set) Token: 0x06002C88 RID: 11400 RVA: 0x001E53C6 File Offset: 0x001E35C6
		public override Identifier Identifier { get; set; } = "find thieves".ToIdentifier();

		// Token: 0x17000B3C RID: 2876
		// (get) Token: 0x06002C89 RID: 11401 RVA: 0x001E53CF File Offset: 0x001E35CF
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x17000B3D RID: 2877
		// (get) Token: 0x06002C8A RID: 11402 RVA: 0x001E53D6 File Offset: 0x001E35D6
		public override bool IgnoreUnsafeHulls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B3E RID: 2878
		// (get) Token: 0x06002C8B RID: 11403 RVA: 0x001E53D9 File Offset: 0x001E35D9
		protected override float TargetUpdateTimeMultiplier
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x17000B3F RID: 2879
		// (get) Token: 0x06002C8C RID: 11404 RVA: 0x001E53E0 File Offset: 0x001E35E0
		public float InspectProbability
		{
			get
			{
				if (this.overrideInspectProbability != null)
				{
					return this.overrideInspectProbability.Value;
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
				if (campaign != null)
				{
					Map map = campaign.Map;
					Reputation reputation2;
					if (map == null)
					{
						reputation2 = null;
					}
					else
					{
						Location currentLocation = map.CurrentLocation;
						reputation2 = ((currentLocation != null) ? currentLocation.Reputation : null);
					}
					Reputation reputation = reputation2;
					if (reputation != null)
					{
						return MathHelper.Lerp(campaign.Settings.PatdownProbabilityMax, campaign.Settings.PatdownProbabilityMin, reputation.NormalizedValue);
					}
				}
				return 0.2f;
			}
		}

		// Token: 0x06002C8D RID: 11405 RVA: 0x001E5464 File Offset: 0x001E3664
		public AIObjectiveFindThieves(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06002C8E RID: 11406 RVA: 0x001E54A0 File Offset: 0x001E36A0
		protected override bool IsValidTarget(Character target)
		{
			GameSession gameSession = GameMain.GameSession;
			if (gameSession == null || gameSession.RoundDuration <= 30f)
			{
				return false;
			}
			if (!this.CheckTarget(target))
			{
				return false;
			}
			float inspectDist = target.IsCriminal ? 500f : this.inspectDistance;
			if (Vector2.DistanceSquared(target.WorldPosition, this.character.WorldPosition) > inspectDist * inspectDist)
			{
				return false;
			}
			double lastInspectionTime;
			if (AIObjectiveFindThieves.lastInspectionTimes.TryGetValue(target, out lastInspectionTime))
			{
				float inspectionInterval = target.IsCriminal ? 30f : 120f;
				if (Timing.TotalTime < lastInspectionTime + (double)inspectionInterval)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002C8F RID: 11407 RVA: 0x001E5535 File Offset: 0x001E3735
		protected override IEnumerable<Character> GetList()
		{
			return Character.CharacterList;
		}

		// Token: 0x06002C90 RID: 11408 RVA: 0x001E553C File Offset: 0x001E373C
		protected override float GetTargetPriority()
		{
			if (this.character.IsClimbing)
			{
				return 0f;
			}
			return (float)(this.subObjectives.Any<AIObjective>() ? 50 : 0);
		}

		// Token: 0x06002C91 RID: 11409 RVA: 0x001E5564 File Offset: 0x001E3764
		public void InspectEveryone()
		{
			AIObjectiveFindThieves.lastInspectionTimes.Clear();
			this.overrideInspectProbability = new float?(1f);
			this.inspectDistance = 400f;
		}

		// Token: 0x06002C92 RID: 11410 RVA: 0x001E558C File Offset: 0x001E378C
		protected override AIObjective ObjectiveConstructor(Character target)
		{
			AIObjectiveCheckStolenItems checkStolenItemsObjective = new AIObjectiveCheckStolenItems(this.character, target, this.objectiveManager, 1f);
			float probabity = target.IsCriminal ? 1f : this.InspectProbability;
			if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) >= probabity)
			{
				checkStolenItemsObjective.ForceComplete();
				AIObjectiveFindThieves.lastInspectionTimes[target] = Timing.TotalTime;
			}
			return checkStolenItemsObjective;
		}

		// Token: 0x06002C93 RID: 11411 RVA: 0x001E55F4 File Offset: 0x001E37F4
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.checkVisibleStolenItemsTimer > 0f || this.character.IsClimbing)
			{
				this.checkVisibleStolenItemsTimer -= deltaTime;
				return;
			}
			Item selectedSecondaryItem = this.character.SelectedSecondaryItem;
			if (((selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Controller>() : null) != null)
			{
				this.character.SelectedSecondaryItem = null;
			}
			if (base.HumanAIController.CurrentHullSafety >= 40f)
			{
				using (List<Character>.Enumerator enumerator = Character.CharacterList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Character target = enumerator.Current;
						if (this.CheckTarget(target) && target.Inventory.AllItems.Any((Item it) => target.HasEquippedItem(it, null, null) && AIObjectiveCheckStolenItems.IsItemIllegitimate(target, it)) && this.character.CanSeeTarget(target, null, true, false) && HumanAIController.CalculateObjectiveHullSafety(target) >= 40f && this.objectiveManager.GetActiveObjectives<AIObjectiveCheckStolenItems>().FirstOrDefault((AIObjectiveCheckStolenItems o) => o.Target == target) == null)
						{
							this.objectiveManager.AddObjective<AIObjectiveCheckStolenItems>(new AIObjectiveCheckStolenItems(this.character, target, this.objectiveManager, 1f));
							AIObjectiveFindThieves.lastInspectionTimes[target] = Timing.TotalTime;
						}
					}
				}
			}
			this.checkVisibleStolenItemsTimer = 5f;
		}

		// Token: 0x06002C94 RID: 11412 RVA: 0x001E5780 File Offset: 0x001E3980
		private bool CheckTarget(Character target)
		{
			return target != null && !target.Removed && !target.IsIncapacitated && target != this.character && target.Submarine != null && this.character.Submarine != null && target.CurrentHull != null && target.Submarine == this.character.Submarine && target.IsOnPlayerTeam && !target.IsHandcuffed && this.character.OriginalTeamID != target.TeamID && this.character.TeamID != target.TeamID && !target.IsClimbing && !base.HumanAIController.IsTrueForAnyBotInTheCrew(delegate(HumanAIController bot)
			{
				if (bot == this.HumanAIController)
				{
					return false;
				}
				AIObjectiveCheckStolenItems checkObj = bot.ObjectiveManager.GetActiveObjective() as AIObjectiveCheckStolenItems;
				if (checkObj == null || checkObj.Target != target)
				{
					AIObjectiveCombat combatObj = bot.ObjectiveManager.GetActiveObjective() as AIObjectiveCombat;
					return combatObj != null && combatObj.Enemy == target;
				}
				return true;
			});
		}

		// Token: 0x06002C95 RID: 11413 RVA: 0x001E5899 File Offset: 0x001E3A99
		protected override void OnObjectiveCompleted(AIObjective objective, Character target)
		{
			AIObjectiveFindThieves.MarkTargetAsInspected(target);
		}

		// Token: 0x06002C96 RID: 11414 RVA: 0x001E58A1 File Offset: 0x001E3AA1
		public static void MarkTargetAsInspected(Character target)
		{
			AIObjectiveFindThieves.lastInspectionTimes[target] = Timing.TotalTime;
		}

		// Token: 0x06002C97 RID: 11415 RVA: 0x001E58B3 File Offset: 0x001E3AB3
		public override void OnDeselected()
		{
			base.OnDeselected();
			this.character.DeselectCharacter();
		}

		// Token: 0x04001739 RID: 5945
		private const float DelayOnRoundStart = 30f;

		// Token: 0x0400173A RID: 5946
		private const float DefaultInspectDistance = 200f;

		// Token: 0x0400173B RID: 5947
		private const float ExtendedInspectDistance = 400f;

		// Token: 0x0400173C RID: 5948
		private const float CriminalInspectDistance = 500f;

		// Token: 0x0400173D RID: 5949
		private const float CriminalInspectProbability = 1f;

		// Token: 0x0400173E RID: 5950
		private float inspectDistance = 200f;

		// Token: 0x0400173F RID: 5951
		private float? overrideInspectProbability;

		// Token: 0x04001740 RID: 5952
		private static readonly Dictionary<Character, double> lastInspectionTimes = new Dictionary<Character, double>();

		// Token: 0x04001741 RID: 5953
		private const float NormalInspectionInterval = 120f;

		// Token: 0x04001742 RID: 5954
		private const float CriminalInspectionInterval = 30f;

		// Token: 0x04001743 RID: 5955
		private float checkVisibleStolenItemsTimer;

		// Token: 0x04001744 RID: 5956
		private const float CheckVisibleStolenItemsInterval = 5f;
	}
}
