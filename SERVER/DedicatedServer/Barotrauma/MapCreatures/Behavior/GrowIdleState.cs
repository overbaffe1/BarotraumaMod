using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020003D6 RID: 982
	[NullableContext(1)]
	[Nullable(0)]
	internal class GrowIdleState : IBallastFloraState
	{
		// Token: 0x060038B3 RID: 14515 RVA: 0x0017AE54 File Offset: 0x00179054
		public GrowIdleState(BallastFloraBehavior behavior)
		{
			this.Behavior = behavior;
		}

		// Token: 0x060038B4 RID: 14516 RVA: 0x0017AE63 File Offset: 0x00179063
		public virtual ExitState GetState()
		{
			return ExitState.Running;
		}

		// Token: 0x060038B5 RID: 14517 RVA: 0x0017AE68 File Offset: 0x00179068
		public virtual void Enter()
		{
			foreach (BallastFloraBranch branch in from b in this.Behavior.Branches
			where b.CanGrowMore()
			select b)
			{
				if (this.TryScanTargets(branch))
				{
					break;
				}
			}
		}

		// Token: 0x060038B6 RID: 14518 RVA: 0x0017AEE4 File Offset: 0x001790E4
		public void Exit()
		{
		}

		// Token: 0x060038B7 RID: 14519 RVA: 0x0017AEE8 File Offset: 0x001790E8
		private bool TryScanTargets(BallastFloraBranch branch)
		{
			Item newTarget = this.ScanForTargets(branch);
			if (newTarget != null)
			{
				this.Behavior.StateMachine.EnterState(new GrowToTargetState(this.Behavior, branch, newTarget));
				return true;
			}
			return false;
		}

		// Token: 0x060038B8 RID: 14520 RVA: 0x0017AF20 File Offset: 0x00179120
		public void Update(float deltaTime)
		{
			if (this.growthTimer > 0f)
			{
				this.growthTimer -= this.Behavior.GetGrowthSpeed(deltaTime);
				return;
			}
			this.Grow();
			this.UpdateIgnoredTargets();
			this.growthTimer = ((this.Behavior.GrowthWarps > 0) ? 0f : 5f);
		}

		// Token: 0x060038B9 RID: 14521 RVA: 0x0017AF80 File Offset: 0x00179180
		protected virtual void Grow()
		{
			List<BallastFloraBranch> newBranches = this.GrowRandomly();
			foreach (BallastFloraBranch branch in newBranches)
			{
				this.TryScanTargets(branch);
			}
		}

		// Token: 0x060038BA RID: 14522 RVA: 0x0017AFD8 File Offset: 0x001791D8
		public void UpdateIgnoredTargets()
		{
			this.Behavior.IgnoredTargets.ForEachMod(delegate(KeyValuePair<Item, int> pair)
			{
				KeyValuePair<Item, int> keyValuePair = pair;
				Item item2;
				int num;
				keyValuePair.Deconstruct(out item2, out num);
				Item item = item2;
				int delay = num;
				if (delay <= 0)
				{
					this.Behavior.IgnoredTargets.Remove(item);
					return;
				}
				this.Behavior.IgnoredTargets[item] = delay - 1;
			});
		}

		// Token: 0x060038BB RID: 14523 RVA: 0x0017AFF8 File Offset: 0x001791F8
		private List<BallastFloraBranch> GrowRandomly()
		{
			List<BallastFloraBranch> availableBranches = (from b in this.Behavior.Branches
			where !b.DisconnectedFromRoot && b.FailedGrowthAttempts <= 8 && b.CanGrowMore()
			select b).ToList<BallastFloraBranch>();
			if (availableBranches.Count == 0)
			{
				return availableBranches;
			}
			BallastFloraBranch branch = ToolBox.SelectWeightedRandom<BallastFloraBranch>(availableBranches, (BallastFloraBranch b) => (float)b.BranchDepth, Rand.RandSync.Unsynced);
			TileSide side = branch.GetRandomFreeSide(null);
			if (side == TileSide.None)
			{
				return availableBranches;
			}
			List<BallastFloraBranch> result;
			this.Behavior.TryGrowBranch(branch, side, out result, false, null);
			availableBranches.Clear();
			availableBranches.Add(branch);
			return availableBranches;
		}

		// Token: 0x060038BC RID: 14524 RVA: 0x0017B0A4 File Offset: 0x001792A4
		[return: Nullable(2)]
		private Item ScanForTargets(VineTile branch)
		{
			Hull parent = this.Behavior.Parent;
			Vector2 worldPos = this.Behavior.GetWorldPosition() + branch.Position;
			Vector2 pos = parent.Position + this.Behavior.Offset + branch.Position;
			Vector2 diameter = ConvertUnits.ToSimUnits(new Vector2((float)branch.Rect.Width / 2f, (float)branch.Rect.Height / 2f));
			Vector2 topLeft = ConvertUnits.ToSimUnits(pos) - diameter;
			Vector2 bottomRight = ConvertUnits.ToSimUnits(pos) + diameter;
			int highestPriority = 0;
			Item currentItem = null;
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine == parent.Submarine && Vector2.DistanceSquared(worldPos, item.WorldPosition) <= (float)(this.Behavior.Sight * this.Behavior.Sight) && !this.Behavior.ClaimedTargets.Contains(item) && !this.Behavior.IgnoredTargets.ContainsKey(item))
				{
					int priority = 0;
					foreach (BallastFloraBehavior.AITarget target in this.Behavior.Targets)
					{
						if (target.Priority > highestPriority && target.Matches(item))
						{
							priority = target.Priority;
							break;
						}
					}
					if (priority != 0)
					{
						Vector2 itemSimPos = ConvertUnits.ToSimUnits(item.Position);
						Body body = Submarine.CheckVisibility(itemSimPos - diameter, topLeft, false, false, true, true, true, null);
						if (!GrowIdleState.<ScanForTargets>g__Blocks|11_0(body, item))
						{
							Body body2 = Submarine.CheckVisibility(itemSimPos + diameter, bottomRight, false, false, true, true, true, null);
							if (!GrowIdleState.<ScanForTargets>g__Blocks|11_0(body2, item))
							{
								highestPriority = priority;
								currentItem = item;
							}
						}
					}
				}
			}
			if (currentItem != null)
			{
				foreach (BallastFloraBranch existingBranch in this.Behavior.Branches)
				{
					if (existingBranch.Health > 0f && !existingBranch.IsRootGrowth && this.Behavior.BranchContainsTarget(existingBranch, currentItem))
					{
						this.Behavior.ClaimTarget(currentItem, existingBranch, false);
						return null;
					}
				}
				return currentItem;
			}
			return null;
		}

		// Token: 0x060038BE RID: 14526 RVA: 0x0017B3B8 File Offset: 0x001795B8
		[CompilerGenerated]
		internal static bool <ScanForTargets>g__Blocks|11_0(Body body, Item target)
		{
			if (body == null)
			{
				return false;
			}
			object userData = body.UserData;
			if (!(userData is Submarine) && !(userData is Structure))
			{
				Item it = userData as Item;
				if (it == null || it == target)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x04001C98 RID: 7320
		public readonly BallastFloraBehavior Behavior;

		// Token: 0x04001C99 RID: 7321
		private float growthTimer;
	}
}
