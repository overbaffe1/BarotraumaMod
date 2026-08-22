using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200017B RID: 379
	internal class AIObjectiveFixLeaks : AIObjectiveLoop<Gap>
	{
		// Token: 0x17000B47 RID: 2887
		// (get) Token: 0x06002CA9 RID: 11433 RVA: 0x001E5ECF File Offset: 0x001E40CF
		// (set) Token: 0x06002CAA RID: 11434 RVA: 0x001E5ED7 File Offset: 0x001E40D7
		public override Identifier Identifier { get; set; } = "fix leaks".ToIdentifier();

		// Token: 0x17000B48 RID: 2888
		// (get) Token: 0x06002CAB RID: 11435 RVA: 0x001E5EE0 File Offset: 0x001E40E0
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B49 RID: 2889
		// (get) Token: 0x06002CAC RID: 11436 RVA: 0x001E5EE3 File Offset: 0x001E40E3
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B4A RID: 2890
		// (get) Token: 0x06002CAD RID: 11437 RVA: 0x001E5EE6 File Offset: 0x001E40E6
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B4B RID: 2891
		// (get) Token: 0x06002CAE RID: 11438 RVA: 0x001E5EE9 File Offset: 0x001E40E9
		// (set) Token: 0x06002CAF RID: 11439 RVA: 0x001E5EF1 File Offset: 0x001E40F1
		private Hull PrioritizedHull { get; set; }

		// Token: 0x06002CB0 RID: 11440 RVA: 0x001E5EFC File Offset: 0x001E40FC
		public AIObjectiveFixLeaks(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f, Hull prioritizedHull = null) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.PrioritizedHull = prioritizedHull;
		}

		// Token: 0x06002CB1 RID: 11441 RVA: 0x001E5F33 File Offset: 0x001E4133
		protected override bool IsValidTarget(Gap gap)
		{
			return AIObjectiveFixLeaks.IsValidTarget(gap, this.character);
		}

		// Token: 0x06002CB2 RID: 11442 RVA: 0x001E5F44 File Offset: 0x001E4144
		public static float GetLeakSeverity(Gap leak)
		{
			if (leak == null)
			{
				return 0f;
			}
			float sizeFactor = MathHelper.Lerp(1f, 10f, MathUtils.InverseLerp(0f, 200f, leak.Size));
			float severity = sizeFactor * leak.Open;
			if (!leak.IsRoomToRoom)
			{
				severity *= 10f;
				return MathHelper.Clamp(severity, 10f, 100f);
			}
			return MathHelper.Min(severity, 100f);
		}

		// Token: 0x06002CB3 RID: 11443 RVA: 0x001E5FB4 File Offset: 0x001E41B4
		protected override float GetTargetPriority()
		{
			int totalLeaks = base.Targets.Count;
			if (totalLeaks == 0)
			{
				return 0f;
			}
			int otherFixers = base.HumanAIController.CountBotsInTheCrew((HumanAIController c) => c != base.HumanAIController && c.ObjectiveManager.IsCurrentObjective<AIObjectiveFixLeaks>() && c.Character.Submarine == this.character.Submarine);
			bool anyFixers = otherFixers > 0;
			if (this.objectiveManager.IsOrder(this))
			{
				float ratio = anyFixers ? ((float)totalLeaks / (float)otherFixers) : 1f;
				return base.Targets.Sum((Gap t) => AIObjectiveFixLeaks.GetLeakSeverity(t)) * ratio;
			}
			int secondaryLeaks = base.Targets.Count((Gap l) => l.IsRoomToRoom);
			int leaks = totalLeaks - secondaryLeaks;
			float ratio2 = (leaks == 0) ? 1f : (anyFixers ? ((float)leaks / (float)otherFixers) : 1f);
			if (anyFixers && (ratio2 <= 1f || otherFixers > 5 || (float)otherFixers / (float)base.HumanAIController.CountBotsInTheCrew(null) > 0.75f))
			{
				return 0f;
			}
			return base.Targets.Sum((Gap t) => AIObjectiveFixLeaks.GetLeakSeverity(t)) * ratio2;
		}

		// Token: 0x06002CB4 RID: 11444 RVA: 0x001E60E7 File Offset: 0x001E42E7
		protected override IEnumerable<Gap> GetList()
		{
			return Gap.GapList;
		}

		// Token: 0x06002CB5 RID: 11445 RVA: 0x001E60EE File Offset: 0x001E42EE
		protected override AIObjective ObjectiveConstructor(Gap gap)
		{
			return new AIObjectiveFixLeak(gap, this.character, this.objectiveManager, base.PriorityModifier, gap.FlowTargetHull == this.PrioritizedHull);
		}

		// Token: 0x06002CB6 RID: 11446 RVA: 0x001E6116 File Offset: 0x001E4316
		protected override void OnObjectiveCompleted(AIObjective objective, Gap target)
		{
			HumanAIController.RemoveTargets<AIObjectiveFixLeaks, Gap>(this.character, target);
		}

		// Token: 0x06002CB7 RID: 11447 RVA: 0x001E6124 File Offset: 0x001E4324
		public static bool IsValidTarget(Gap gap, Character character)
		{
			if (gap == null)
			{
				return false;
			}
			if (gap.ConnectedWall != null)
			{
				if (gap.ConnectedWall.Sections.Any((WallSection s) => s.gap == gap && s.IgnoreByAI(character)))
				{
					return false;
				}
				if (gap.ConnectedWall.MaxHealth <= 0f)
				{
					return false;
				}
			}
			if (gap.ConnectedWall != null && gap.ConnectedDoor == null && gap.Open > 0f)
			{
				if (!gap.linkedTo.All((MapEntity l) => l == null))
				{
					return gap.Submarine != null && character.Submarine != null && character.Submarine.IsEntityFoundOnThisSub(gap, true, false, false);
				}
			}
			return false;
		}
	}
}
