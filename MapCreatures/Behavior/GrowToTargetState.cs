using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020004DF RID: 1247
	internal class GrowToTargetState : GrowIdleState
	{
		// Token: 0x0600516B RID: 20843 RVA: 0x002BD07A File Offset: 0x002BB27A
		public GrowToTargetState(BallastFloraBehavior behavior, BallastFloraBranch starter, Item target) : base(behavior)
		{
			this.Target = target;
			this.TargetBranches.Add(starter);
		}

		// Token: 0x0600516C RID: 20844 RVA: 0x002BD0A1 File Offset: 0x002BB2A1
		public override void Enter()
		{
		}

		// Token: 0x0600516D RID: 20845 RVA: 0x002BD0A3 File Offset: 0x002BB2A3
		public override ExitState GetState()
		{
			if (!this.isFinished)
			{
				return ExitState.Running;
			}
			return ExitState.Terminate;
		}

		// Token: 0x0600516E RID: 20846 RVA: 0x002BD0B0 File Offset: 0x002BB2B0
		protected override void Grow()
		{
			if (this.TargetBranches.Any((BallastFloraBranch b) => b.Removed))
			{
				if (!this.Behavior.IgnoredTargets.ContainsKey(this.Target))
				{
					this.Behavior.IgnoredTargets.Add(this.Target, 10);
				}
				this.isFinished = true;
				return;
			}
			if (this.Target == null || this.Target.Removed)
			{
				this.isFinished = true;
				return;
			}
			this.GrowTowardsTarget();
		}

		// Token: 0x0600516F RID: 20847 RVA: 0x002BD144 File Offset: 0x002BB344
		private void GrowTowardsTarget()
		{
			bool succeeded = false;
			List<BallastFloraBranch> newList = new List<BallastFloraBranch>(this.TargetBranches);
			foreach (BallastFloraBranch branch in newList)
			{
				if (branch.FailedGrowthAttempts <= 8 && !branch.DisconnectedFromRoot && branch.CanGrowMore())
				{
					TileSide side = this.GetClosestSide(branch, this.Target.WorldPosition);
					if (!branch.IsSideBlocked(side))
					{
						List<BallastFloraBranch> newBranches;
						succeeded |= this.Behavior.TryGrowBranch(branch, side, out newBranches, false, null);
						this.TargetBranches.AddRange(newBranches);
						foreach (BallastFloraBranch newBranch in newBranches)
						{
							Rectangle worldRect = newBranch.Rect;
							worldRect.Location = this.Behavior.GetWorldPosition().ToPoint() + worldRect.Location;
							if (this.Behavior.BranchContainsTarget(newBranch, this.Target))
							{
								this.Behavior.ClaimTarget(this.Target, newBranch, false);
								this.isFinished = true;
								return;
							}
						}
					}
				}
			}
			if (!succeeded)
			{
				if (!this.Behavior.IgnoredTargets.ContainsKey(this.Target))
				{
					this.Behavior.IgnoredTargets.Add(this.Target, 1);
				}
				this.isFinished = true;
			}
		}

		// Token: 0x06005170 RID: 20848 RVA: 0x002BD2FC File Offset: 0x002BB4FC
		private TileSide GetClosestSide(VineTile tile, Vector2 targetPos)
		{
			float num;
			float num2;
			(tile.Position + this.Behavior.GetWorldPosition() - targetPos).Deconstruct(out num, out num2);
			float distX = num;
			float distY = num2;
			int absDistX = (int)Math.Abs(distX);
			int absDistY = (int)Math.Abs(distY);
			if (absDistX <= absDistY)
			{
				if (distY <= 0f)
				{
					return TileSide.Top;
				}
				return TileSide.Bottom;
			}
			else
			{
				if (distX <= 0f)
				{
					return TileSide.Right;
				}
				return TileSide.Left;
			}
		}

		// Token: 0x04002B33 RID: 11059
		public readonly List<BallastFloraBranch> TargetBranches = new List<BallastFloraBranch>();

		// Token: 0x04002B34 RID: 11060
		public readonly Item Target;

		// Token: 0x04002B35 RID: 11061
		private bool isFinished;
	}
}
