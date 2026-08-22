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
	// Token: 0x020004DE RID: 1246
	[NullableContext(1)]
	[Nullable(0)]
	internal class GrowIdleState : IBallastFloraState
	{
		// Token: 0x0600515F RID: 20831 RVA: 0x002BCADC File Offset: 0x002BACDC
		public GrowIdleState(BallastFloraBehavior behavior)
		{
			this.Behavior = behavior;
		}

		// Token: 0x06005160 RID: 20832 RVA: 0x002BCAEB File Offset: 0x002BACEB
		public virtual ExitState GetState()
		{
			return ExitState.Running;
		}

		// Token: 0x06005161 RID: 20833 RVA: 0x002BCAF0 File Offset: 0x002BACF0
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

		// Token: 0x06005162 RID: 20834 RVA: 0x002BCB6C File Offset: 0x002BAD6C
		public void Exit()
		{
		}

		// Token: 0x06005163 RID: 20835 RVA: 0x002BCB70 File Offset: 0x002BAD70
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

		// Token: 0x06005164 RID: 20836 RVA: 0x002BCBA8 File Offset: 0x002BADA8
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

		// Token: 0x06005165 RID: 20837 RVA: 0x002BCC08 File Offset: 0x002BAE08
		protected virtual void Grow()
		{
			List<BallastFloraBranch> newBranches = this.GrowRandomly();
			foreach (BallastFloraBranch branch in newBranches)
			{
				this.TryScanTargets(branch);
			}
		}

		// Token: 0x06005166 RID: 20838 RVA: 0x002BCC60 File Offset: 0x002BAE60
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

		// Token: 0x06005167 RID: 20839 RVA: 0x002BCC80 File Offset: 0x002BAE80
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

		// Token: 0x06005168 RID: 20840 RVA: 0x002BCD2C File Offset: 0x002BAF2C
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

		// Token: 0x0600516A RID: 20842 RVA: 0x002BD040 File Offset: 0x002BB240
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

		// Token: 0x04002B31 RID: 11057
		public readonly BallastFloraBehavior Behavior;

		// Token: 0x04002B32 RID: 11058
		private float growthTimer;
	}
}
