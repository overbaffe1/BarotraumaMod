using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000073 RID: 115
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal class AIObjectiveFindThieves : AIObjectiveLoop<Character>
	{
		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x06000F7F RID: 3967 RVA: 0x0009252A File Offset: 0x0009072A
		// (set) Token: 0x06000F80 RID: 3968 RVA: 0x00092532 File Offset: 0x00090732
		public override Identifier Identifier { get; set; } = "find thieves".ToIdentifier();

		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06000F81 RID: 3969 RVA: 0x0009253B File Offset: 0x0009073B
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06000F82 RID: 3970 RVA: 0x00092542 File Offset: 0x00090742
		public override bool IgnoreUnsafeHulls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06000F83 RID: 3971 RVA: 0x00092545 File Offset: 0x00090745
		protected override float TargetUpdateTimeMultiplier
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06000F84 RID: 3972 RVA: 0x0009254C File Offset: 0x0009074C
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

		// Token: 0x06000F85 RID: 3973 RVA: 0x000925D0 File Offset: 0x000907D0
		public AIObjectiveFindThieves(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x0009260C File Offset: 0x0009080C
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

		// Token: 0x06000F87 RID: 3975 RVA: 0x000926A1 File Offset: 0x000908A1
		protected override IEnumerable<Character> GetList()
		{
			return Character.CharacterList;
		}

		// Token: 0x06000F88 RID: 3976 RVA: 0x000926A8 File Offset: 0x000908A8
		protected override float GetTargetPriority()
		{
			if (this.character.IsClimbing)
			{
				return 0f;
			}
			return (float)(this.subObjectives.Any<AIObjective>() ? 50 : 0);
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x000926D0 File Offset: 0x000908D0
		public void InspectEveryone()
		{
			AIObjectiveFindThieves.lastInspectionTimes.Clear();
			this.overrideInspectProbability = new float?(1f);
			this.inspectDistance = 400f;
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x000926F8 File Offset: 0x000908F8
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

		// Token: 0x06000F8B RID: 3979 RVA: 0x00092760 File Offset: 0x00090960
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

		// Token: 0x06000F8C RID: 3980 RVA: 0x000928EC File Offset: 0x00090AEC
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

		// Token: 0x06000F8D RID: 3981 RVA: 0x00092A05 File Offset: 0x00090C05
		protected override void OnObjectiveCompleted(AIObjective objective, Character target)
		{
			AIObjectiveFindThieves.MarkTargetAsInspected(target);
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x00092A0D File Offset: 0x00090C0D
		public static void MarkTargetAsInspected(Character target)
		{
			AIObjectiveFindThieves.lastInspectionTimes[target] = Timing.TotalTime;
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x00092A1F File Offset: 0x00090C1F
		public override void OnDeselected()
		{
			base.OnDeselected();
			this.character.DeselectCharacter();
		}

		// Token: 0x0400073F RID: 1855
		private const float DelayOnRoundStart = 30f;

		// Token: 0x04000740 RID: 1856
		private const float DefaultInspectDistance = 200f;

		// Token: 0x04000741 RID: 1857
		private const float ExtendedInspectDistance = 400f;

		// Token: 0x04000742 RID: 1858
		private const float CriminalInspectDistance = 500f;

		// Token: 0x04000743 RID: 1859
		private const float CriminalInspectProbability = 1f;

		// Token: 0x04000744 RID: 1860
		private float inspectDistance = 200f;

		// Token: 0x04000745 RID: 1861
		private float? overrideInspectProbability;

		// Token: 0x04000746 RID: 1862
		private static readonly Dictionary<Character, double> lastInspectionTimes = new Dictionary<Character, double>();

		// Token: 0x04000747 RID: 1863
		private const float NormalInspectionInterval = 120f;

		// Token: 0x04000748 RID: 1864
		private const float CriminalInspectionInterval = 30f;

		// Token: 0x04000749 RID: 1865
		private float checkVisibleStolenItemsTimer;

		// Token: 0x0400074A RID: 1866
		private const float CheckVisibleStolenItemsInterval = 5f;
	}
}
