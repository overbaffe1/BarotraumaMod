using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000075 RID: 117
	internal class AIObjectiveFixLeaks : AIObjectiveLoop<Gap>
	{
		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x06000FA1 RID: 4001 RVA: 0x0009303B File Offset: 0x0009123B
		// (set) Token: 0x06000FA2 RID: 4002 RVA: 0x00093043 File Offset: 0x00091243
		public override Identifier Identifier { get; set; } = "fix leaks".ToIdentifier();

		// Token: 0x1700042E RID: 1070
		// (get) Token: 0x06000FA3 RID: 4003 RVA: 0x0009304C File Offset: 0x0009124C
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700042F RID: 1071
		// (get) Token: 0x06000FA4 RID: 4004 RVA: 0x0009304F File Offset: 0x0009124F
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000430 RID: 1072
		// (get) Token: 0x06000FA5 RID: 4005 RVA: 0x00093052 File Offset: 0x00091252
		protected override bool AllowInFriendlySubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x06000FA6 RID: 4006 RVA: 0x00093055 File Offset: 0x00091255
		// (set) Token: 0x06000FA7 RID: 4007 RVA: 0x0009305D File Offset: 0x0009125D
		private Hull PrioritizedHull { get; set; }

		// Token: 0x06000FA8 RID: 4008 RVA: 0x00093068 File Offset: 0x00091268
		public AIObjectiveFixLeaks(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f, Hull prioritizedHull = null) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.PrioritizedHull = prioritizedHull;
		}

		// Token: 0x06000FA9 RID: 4009 RVA: 0x0009309F File Offset: 0x0009129F
		protected override bool IsValidTarget(Gap gap)
		{
			return AIObjectiveFixLeaks.IsValidTarget(gap, this.character);
		}

		// Token: 0x06000FAA RID: 4010 RVA: 0x000930B0 File Offset: 0x000912B0
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

		// Token: 0x06000FAB RID: 4011 RVA: 0x00093120 File Offset: 0x00091320
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

		// Token: 0x06000FAC RID: 4012 RVA: 0x00093253 File Offset: 0x00091453
		protected override IEnumerable<Gap> GetList()
		{
			return Gap.GapList;
		}

		// Token: 0x06000FAD RID: 4013 RVA: 0x0009325A File Offset: 0x0009145A
		protected override AIObjective ObjectiveConstructor(Gap gap)
		{
			return new AIObjectiveFixLeak(gap, this.character, this.objectiveManager, base.PriorityModifier, gap.FlowTargetHull == this.PrioritizedHull);
		}

		// Token: 0x06000FAE RID: 4014 RVA: 0x00093282 File Offset: 0x00091482
		protected override void OnObjectiveCompleted(AIObjective objective, Gap target)
		{
			HumanAIController.RemoveTargets<AIObjectiveFixLeaks, Gap>(this.character, target);
		}

		// Token: 0x06000FAF RID: 4015 RVA: 0x00093290 File Offset: 0x00091490
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
