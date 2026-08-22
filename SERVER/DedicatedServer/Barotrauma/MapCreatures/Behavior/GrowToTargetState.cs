using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020003D7 RID: 983
	internal class GrowToTargetState : GrowIdleState
	{
		// Token: 0x060038BF RID: 14527 RVA: 0x0017B3F2 File Offset: 0x001795F2
		public GrowToTargetState(BallastFloraBehavior behavior, BallastFloraBranch starter, Item target) : base(behavior)
		{
			this.Target = target;
			this.TargetBranches.Add(starter);
		}

		// Token: 0x060038C0 RID: 14528 RVA: 0x0017B419 File Offset: 0x00179619
		public override void Enter()
		{
		}

		// Token: 0x060038C1 RID: 14529 RVA: 0x0017B41B File Offset: 0x0017961B
		public override ExitState GetState()
		{
			if (!this.isFinished)
			{
				return ExitState.Running;
			}
			return ExitState.Terminate;
		}

		// Token: 0x060038C2 RID: 14530 RVA: 0x0017B428 File Offset: 0x00179628
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

		// Token: 0x060038C3 RID: 14531 RVA: 0x0017B4BC File Offset: 0x001796BC
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

		// Token: 0x060038C4 RID: 14532 RVA: 0x0017B674 File Offset: 0x00179874
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

		// Token: 0x04001C9A RID: 7322
		public readonly List<BallastFloraBranch> TargetBranches = new List<BallastFloraBranch>();

		// Token: 0x04001C9B RID: 7323
		public readonly Item Target;

		// Token: 0x04001C9C RID: 7324
		private bool isFinished;
	}
}
