using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.MapCreatures.Behavior;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000175 RID: 373
	internal class AIObjectiveExtinguishFires : AIObjectiveLoop<Hull>
	{
		// Token: 0x17000B22 RID: 2850
		// (get) Token: 0x06002C3B RID: 11323 RVA: 0x001E33F1 File Offset: 0x001E15F1
		// (set) Token: 0x06002C3C RID: 11324 RVA: 0x001E33F9 File Offset: 0x001E15F9
		public override Identifier Identifier { get; set; } = "extinguish fires".ToIdentifier();

		// Token: 0x17000B23 RID: 2851
		// (get) Token: 0x06002C3D RID: 11325 RVA: 0x001E3402 File Offset: 0x001E1602
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B24 RID: 2852
		// (get) Token: 0x06002C3E RID: 11326 RVA: 0x001E3405 File Offset: 0x001E1605
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B25 RID: 2853
		// (get) Token: 0x06002C3F RID: 11327 RVA: 0x001E3408 File Offset: 0x001E1608
		protected override float IgnoreListClearInterval
		{
			get
			{
				return 30f;
			}
		}

		// Token: 0x06002C40 RID: 11328 RVA: 0x001E3410 File Offset: 0x001E1610
		public AIObjectiveExtinguishFires(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06002C41 RID: 11329 RVA: 0x001E343F File Offset: 0x001E163F
		protected override bool IsValidTarget(Hull hull)
		{
			return AIObjectiveExtinguishFires.IsValidTarget(hull, this.character);
		}

		// Token: 0x06002C42 RID: 11330 RVA: 0x001E3450 File Offset: 0x001E1650
		protected override float GetTargetPriority()
		{
			if (!base.Targets.Any((Hull t) => t == this.character.CurrentHull || base.HumanAIController.VisibleHulls.Contains(t)))
			{
				return base.Targets.Sum((Hull t) => AIObjectiveExtinguishFires.GetFireSeverity(t) * 100f);
			}
			return 100f;
		}

		// Token: 0x06002C43 RID: 11331 RVA: 0x001E34A8 File Offset: 0x001E16A8
		public static float GetFireSeverity(Hull hull)
		{
			return MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 500f, hull.FireSources.Sum((FireSource fs) => fs.Size.X)));
		}

		// Token: 0x06002C44 RID: 11332 RVA: 0x001E34FD File Offset: 0x001E16FD
		protected override IEnumerable<Hull> GetList()
		{
			return Hull.HullList;
		}

		// Token: 0x06002C45 RID: 11333 RVA: 0x001E3504 File Offset: 0x001E1704
		protected override AIObjective ObjectiveConstructor(Hull target)
		{
			return new AIObjectiveExtinguishFire(this.character, target, this.objectiveManager, base.PriorityModifier);
		}

		// Token: 0x06002C46 RID: 11334 RVA: 0x001E351E File Offset: 0x001E171E
		protected override void OnObjectiveCompleted(AIObjective objective, Hull target)
		{
			HumanAIController.RemoveTargets<AIObjectiveExtinguishFires, Hull>(this.character, target);
		}

		// Token: 0x06002C47 RID: 11335 RVA: 0x001E352C File Offset: 0x001E172C
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
