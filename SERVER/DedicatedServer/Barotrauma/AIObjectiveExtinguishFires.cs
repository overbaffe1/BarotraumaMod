using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.MapCreatures.Behavior;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200006F RID: 111
	internal class AIObjectiveExtinguishFires : AIObjectiveLoop<Hull>
	{
		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x06000F33 RID: 3891 RVA: 0x0009055D File Offset: 0x0008E75D
		// (set) Token: 0x06000F34 RID: 3892 RVA: 0x00090565 File Offset: 0x0008E765
		public override Identifier Identifier { get; set; } = "extinguish fires".ToIdentifier();

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x06000F35 RID: 3893 RVA: 0x0009056E File Offset: 0x0008E76E
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x06000F36 RID: 3894 RVA: 0x00090571 File Offset: 0x0008E771
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x06000F37 RID: 3895 RVA: 0x00090574 File Offset: 0x0008E774
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x06000F38 RID: 3896 RVA: 0x0009057C File Offset: 0x0008E77C
		public AIObjectiveExtinguishFires(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06000F39 RID: 3897 RVA: 0x000905AB File Offset: 0x0008E7AB
		protected override bool IsValidTarget(Hull hull)
		{
			return AIObjectiveExtinguishFires.IsValidTarget(hull, this.character);
		}

		// Token: 0x06000F3A RID: 3898 RVA: 0x000905BC File Offset: 0x0008E7BC
		protected override float GetTargetPriority()
		{
			if (!base.Targets.Any((Hull t) => t == this.character.CurrentHull || base.HumanAIController.VisibleHulls.Contains(t)))
			{
				return base.Targets.Sum((Hull t) => AIObjectiveExtinguishFires.GetFireSeverity(t) * 100f);
			}
			return 100f;
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x00090614 File Offset: 0x0008E814
		public static float GetFireSeverity(Hull hull)
		{
			return MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 500f, hull.FireSources.Sum((FireSource fs) => fs.Size.X)));
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x00090669 File Offset: 0x0008E869
		protected override IEnumerable<Hull> GetList()
		{
			return Hull.HullList;
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x00090670 File Offset: 0x0008E870
		protected override AIObjective ObjectiveConstructor(Hull target)
		{
			return new AIObjectiveExtinguishFire(this.character, target, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x0009068A File Offset: 0x0008E88A
		protected override void OnObjectiveCompleted(AIObjective objective, Hull target)
		{
			HumanAIController.RemoveTargets<AIObjectiveExtinguishFires, Hull>(this.character, target);
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x00090698 File Offset: 0x0008E898
		public static bool IsValidTarget(Hull hull, Character character)
		{
			if (hull == null)
			{
				return false;
			}
			if (hull.FireSources.None(null))
			{
				return false;
			}
			if (hull.Submarine == null)
			{
				return false;
			}
			if (character.Submarine == null)
			{
				return false;
			}
			if (!character.Submarine.IsEntityFoundOnThisSub(hull, true, false, false))
			{
				return false;
			}
			if (hull.BallastFlora != null)
			{
				return false;
			}
			Func<BallastFloraBranch, bool> <>9__0;
			foreach (BallastFloraBehavior ballastFlora in BallastFloraBehavior.EntityList)
			{
				Hull parent = ballastFlora.Parent;
				if (((parent != null) ? parent.Submarine : null) == character.Submarine)
				{
					IEnumerable<BallastFloraBranch> branches = ballastFlora.Branches;
					Func<BallastFloraBranch, bool> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = ((BallastFloraBranch b) => !b.Removed && b.Health > 0f && b.CurrentHull == hull));
					}
					if (branches.Any(predicate))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
