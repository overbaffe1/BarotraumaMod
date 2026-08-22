using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000165 RID: 357
	internal class IndoorsSteeringManager : SteeringManager
	{
		// Token: 0x17000AC3 RID: 2755
		// (get) Token: 0x06002AD4 RID: 10964 RVA: 0x001D87AE File Offset: 0x001D69AE
		// (set) Token: 0x06002AD5 RID: 10965 RVA: 0x001D87B6 File Offset: 0x001D69B6
		public bool CanBreakDoors { get; set; }

		// Token: 0x06002AD6 RID: 10966 RVA: 0x001D87C0 File Offset: 0x001D69C0
		private bool ShouldBreakDoor(Door door)
		{
			return this.CanBreakDoors && !door.Item.Indestructible && !door.Item.InvulnerableToDamage && (door.Item.Submarine == null || door.Item.Submarine.TeamID != this.character.TeamID);
		}

		// Token: 0x17000AC4 RID: 2756
		// (get) Token: 0x06002AD7 RID: 10967 RVA: 0x001D8820 File Offset: 0x001D6A20
		public SteeringPath CurrentPath
		{
			get
			{
				return this.currentPath;
			}
		}

		// Token: 0x17000AC5 RID: 2757
		// (get) Token: 0x06002AD8 RID: 10968 RVA: 0x001D8828 File Offset: 0x001D6A28
		public PathFinder PathFinder
		{
			get
			{
				return this.pathFinder;
			}
		}

		// Token: 0x17000AC6 RID: 2758
		// (get) Token: 0x06002AD9 RID: 10969 RVA: 0x001D8830 File Offset: 0x001D6A30
		// (set) Token: 0x06002ADA RID: 10970 RVA: 0x001D8838 File Offset: 0x001D6A38
		public bool IsPathDirty { get; private set; }

		// Token: 0x17000AC7 RID: 2759
		// (get) Token: 0x06002ADB RID: 10971 RVA: 0x001D8841 File Offset: 0x001D6A41
		public bool PathHasStairs
		{
			get
			{
				if (this.currentPath != null)
				{
					return this.currentPath.Nodes.Any((WayPoint n) => n.Stairs != null);
				}
				return false;
			}
		}

		// Token: 0x17000AC8 RID: 2760
		// (get) Token: 0x06002ADC RID: 10972 RVA: 0x001D887C File Offset: 0x001D6A7C
		public bool IsCurrentNodeLadder
		{
			get
			{
				return this.GetCurrentLadder() != null;
			}
		}

		// Token: 0x17000AC9 RID: 2761
		// (get) Token: 0x06002ADD RID: 10973 RVA: 0x001D8887 File Offset: 0x001D6A87
		public bool IsNextNodeLadder
		{
			get
			{
				return this.GetNextLadder() != null;
			}
		}

		// Token: 0x17000ACA RID: 2762
		// (get) Token: 0x06002ADE RID: 10974 RVA: 0x001D8894 File Offset: 0x001D6A94
		public bool IsNextLadderSameAsCurrent
		{
			get
			{
				Ladder currentLadder = this.GetCurrentLadder();
				return currentLadder != null && currentLadder == this.GetNextLadder();
			}
		}

		// Token: 0x06002ADF RID: 10975 RVA: 0x001D88B8 File Offset: 0x001D6AB8
		public IndoorsSteeringManager(ISteerable host, bool canOpenDoors, bool canBreakDoors) : base(host)
		{
			this.pathFinder = new PathFinder(WayPoint.WayPointList.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Path), true)
			{
				GetNodePenalty = new PathFinder.GetNodePenaltyHandler(this.GetNodePenalty),
				GetSingleNodePenalty = new PathFinder.GetSingleNodePenaltyHandler(this.GetSingleNodePenalty)
			};
			this.canOpenDoors = canOpenDoors;
			this.CanBreakDoors = canBreakDoors;
			this.character = (host as AIController).Character;
			this.findPathTimer = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06002AE0 RID: 10976 RVA: 0x001D895C File Offset: 0x001D6B5C
		public override void Update(float speed)
		{
			base.Update(speed);
			float step = 0.016666668f;
			this.checkDoorsTimer -= step;
			if (this.lastDoor.Item1 == null || !this.lastDoor.Item2 || this.lastDoor.Item1.IsFullyOpen)
			{
				this.buttonPressTimer = 0f;
			}
			else
			{
				this.buttonPressTimer -= step;
			}
			this.findPathTimer -= step;
		}

		// Token: 0x06002AE1 RID: 10977 RVA: 0x001D89D9 File Offset: 0x001D6BD9
		public void SetPath(Vector2 targetPos, SteeringPath path)
		{
			this.currentTargetPos = targetPos;
			this.currentPath = path;
			this.findPathTimer = Math.Min(this.findPathTimer, 1f);
			this.IsPathDirty = false;
		}

		// Token: 0x06002AE2 RID: 10978 RVA: 0x001D8A06 File Offset: 0x001D6C06
		public void ResetPath()
		{
			this.currentPath = null;
			this.IsPathDirty = true;
		}

		// Token: 0x06002AE3 RID: 10979 RVA: 0x001D8A16 File Offset: 0x001D6C16
		public void SteeringSeekSimple(Vector2 targetSimPos, float weight = 1f)
		{
			this.steering += base.DoSteeringSeek(targetSimPos, weight);
		}

		// Token: 0x06002AE4 RID: 10980 RVA: 0x001D8A34 File Offset: 0x001D6C34
		public void SteeringSeek(Vector2 target, float weight, float minGapWidth = 0f, Func<PathNode, bool> startNodeFilter = null, Func<PathNode, bool> endNodeFilter = null, Func<PathNode, bool> nodeFilter = null, bool checkVisiblity = true, float outsideNodePenalty = 0f)
		{
			Vector2 addition = this.CalculateSteeringSeek(target, weight, minGapWidth, startNodeFilter, endNodeFilter, nodeFilter, checkVisiblity, outsideNodePenalty);
			this.steering += addition;
		}

		// Token: 0x06002AE5 RID: 10981 RVA: 0x001D8A67 File Offset: 0x001D6C67
		public Ladder GetCurrentLadder()
		{
			SteeringPath steeringPath = this.currentPath;
			return this.GetLadder((steeringPath != null) ? steeringPath.CurrentNode : null);
		}

		// Token: 0x06002AE6 RID: 10982 RVA: 0x001D8A81 File Offset: 0x001D6C81
		public Ladder GetNextLadder()
		{
			SteeringPath steeringPath = this.currentPath;
			return this.GetLadder((steeringPath != null) ? steeringPath.NextNode : null);
		}

		// Token: 0x06002AE7 RID: 10983 RVA: 0x001D8A9C File Offset: 0x001D6C9C
		private Ladder GetLadder(WayPoint wp)
		{
			Item item2;
			if (wp == null)
			{
				item2 = null;
			}
			else
			{
				Ladder ladders = wp.Ladders;
				item2 = ((ladders != null) ? ladders.Item : null);
			}
			Item item = item2;
			if (item != null && item.IsInteractable(this.character))
			{
				return wp.Ladders;
			}
			return null;
		}

		// Token: 0x06002AE8 RID: 10984 RVA: 0x001D8ADC File Offset: 0x001D6CDC
		private Vector2 CalculateSteeringSeek(Vector2 target, float weight, float minGapSize = 0f, Func<PathNode, bool> startNodeFilter = null, Func<PathNode, bool> endNodeFilter = null, Func<PathNode, bool> nodeFilter = null, bool checkVisibility = true, float outsideNodePenalty = 0f)
		{
			bool needsNewPath = this.currentPath == null || this.currentPath.Unreachable || this.currentPath.Finished || this.currentPath.IsAtEndNode || this.currentPath.CurrentNode == null;
			if (!needsNewPath && this.character.Submarine != null && this.character.Params.PathFinderPriority > 0.5f && (target - this.currentTargetPos).LengthSquared() > 1f)
			{
				needsNewPath = true;
			}
			if (needsNewPath || this.findPathTimer < -1f)
			{
				this.IsPathDirty = true;
				if (!needsNewPath)
				{
					SteeringPath steeringPath = this.currentPath;
					WayPoint wp = (steeringPath != null) ? steeringPath.CurrentNode : null;
					if (wp != null)
					{
						if (this.character.Submarine != null && wp.Ladders == null && wp.ConnectedDoor == null && Math.Abs(this.character.AnimController.TargetMovement.Combine()) <= 0f)
						{
							needsNewPath = true;
						}
						if (this.character.Submarine == null && wp.CurrentHull != null)
						{
							float maxDist = 200f;
							if (Vector2.DistanceSquared(this.character.WorldPosition, wp.WorldPosition) > maxDist * maxDist)
							{
								needsNewPath = true;
							}
						}
					}
				}
				if (this.findPathTimer < 0f)
				{
					this.<CalculateSteeringSeek>g__SkipCurrentPathNodes|39_1();
					this.currentTargetPos = target;
					Vector2 currentPos = this.host.SimPosition;
					this.pathFinder.InsideSubmarine = (this.character.Submarine != null && !this.character.Submarine.Info.IsRuin);
					this.pathFinder.ApplyPenaltyToOutsideNodes = (this.character.Submarine != null && !this.character.IsProtectedFromPressure);
					IndoorsSteeringManager.<>c__DisplayClass39_0 CS$<>8__locals1;
					CS$<>8__locals1.newPath = this.pathFinder.FindPath(currentPos, target, this.character.Submarine, "(Character: " + this.character.Name + ")", minGapSize, startNodeFilter, endNodeFilter, nodeFilter, checkVisibility, outsideNodePenalty);
					bool useNewPath = needsNewPath;
					if (!useNewPath)
					{
						SteeringPath steeringPath2 = this.currentPath;
						if (((steeringPath2 != null) ? steeringPath2.CurrentNode : null) != null && CS$<>8__locals1.newPath.Nodes.Any<WayPoint>() && !CS$<>8__locals1.newPath.Unreachable)
						{
							if (this.<CalculateSteeringSeek>g__IsIdenticalPath|39_0(ref CS$<>8__locals1))
							{
								useNewPath = false;
							}
							else if (!this.character.IsClimbing)
							{
								float t = (float)this.currentPath.CurrentIndex / (float)(this.currentPath.Nodes.Count - 1);
								useNewPath = (CS$<>8__locals1.newPath.Cost < this.currentPath.Cost * MathHelper.Lerp(0.95f, 0f, t));
								if (!useNewPath && this.character.Submarine != null)
								{
									useNewPath = ((double)Vector2.DistanceSquared(this.character.WorldPosition, this.currentPath.CurrentNode.WorldPosition) > Math.Pow((double)(Vector2.Distance(this.character.WorldPosition, CS$<>8__locals1.newPath.Nodes.First<WayPoint>().WorldPosition) * 3f), 2.0));
								}
							}
							if (!useNewPath && !this.character.CanSeeTarget(this.currentPath.CurrentNode, null, false, false))
							{
								useNewPath = true;
							}
						}
					}
					if (useNewPath)
					{
						if (this.currentPath != null)
						{
							this.CheckDoorsInPath();
						}
						this.currentPath = CS$<>8__locals1.newPath;
					}
					float priority = MathHelper.Lerp(3f, 1f, this.character.Params.PathFinderPriority);
					this.findPathTimer = priority * Rand.Range(1f, 1.2f, Rand.RandSync.Unsynced);
					this.IsPathDirty = false;
				}
			}
			Vector2 diff = this.GetDiffAndAdvance();
			if (diff == Vector2.Zero)
			{
				return Vector2.Zero;
			}
			return Vector2.Normalize(diff) * weight;
		}

		// Token: 0x06002AE9 RID: 10985 RVA: 0x001D8EBC File Offset: 0x001D70BC
		protected override Vector2 DoSteeringSeek(Vector2 target, float weight)
		{
			return this.CalculateSteeringSeek(target, weight, 0f, null, null, null, true, 0f);
		}

		// Token: 0x06002AEA RID: 10986 RVA: 0x001D8EE0 File Offset: 0x001D70E0
		private Vector2 GetDiffAndAdvance()
		{
			IndoorsSteeringManager.<>c__DisplayClass41_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (this.currentPath == null || this.currentPath.Unreachable)
			{
				return Vector2.Zero;
			}
			if (this.currentPath.Finished)
			{
				Vector2 hostPosition = this.host.SimPosition;
				if (this.character != null && this.character.Submarine == null && this.CurrentPath.Nodes.Count > 0 && this.CurrentPath.Nodes.Last<WayPoint>().Submarine != null)
				{
					hostPosition -= this.CurrentPath.Nodes.Last<WayPoint>().Submarine.SimPosition;
				}
				return this.currentTargetPos - hostPosition;
			}
			bool doorsChecked = false;
			this.checkDoorsTimer = Math.Min(this.checkDoorsTimer, this.GetDoorCheckTime());
			if (!this.character.LockHands && this.checkDoorsTimer <= 0f)
			{
				this.CheckDoorsInPath();
				doorsChecked = true;
			}
			if (this.buttonPressTimer > 0f && this.lastDoor.Item1 != null && this.lastDoor.Item2 && !this.lastDoor.Item1.IsFullyOpen)
			{
				base.Reset();
				return Vector2.Zero;
			}
			WayPoint currentNode = this.currentPath.CurrentNode;
			WayPoint nextNode = this.currentPath.NextNode;
			CS$<>8__locals1.diff = currentNode.WorldPosition - this.host.WorldPosition;
			float horizontalDistance = Math.Abs(CS$<>8__locals1.diff.X);
			float verticalDistance = Math.Abs(CS$<>8__locals1.diff.Y);
			bool isDiving = this.character.AnimController.InWater && this.character.AnimController.HeadInWater;
			bool canClimb = this.character.CanClimb;
			Ladder currentLadder = this.GetCurrentLadder();
			Ladder nextLadder = this.GetNextLadder();
			Ladder ladders = currentLadder ?? nextLadder;
			bool useLadders = canClimb && ladders != null;
			PhysicsBody collider = this.character.AnimController.Collider;
			Vector2 colliderSize = ConvertUnits.ToDisplayUnits(collider.GetSize());
			float colliderHeight = colliderSize.Y;
			FishGroundedParams fishGrounded = this.character.AnimController.CurrentAnimationParams as FishGroundedParams;
			if (fishGrounded != null)
			{
				float standAngle = fishGrounded.ColliderStandAngleInRadians * this.character.AnimController.Dir;
				Vector2 transformedColliderSize = PhysicsBody.RotateVector(colliderSize, standAngle);
				colliderHeight = Math.Abs(transformedColliderSize.Y);
			}
			if (useLadders)
			{
				if (this.character.IsClimbing && Math.Abs(CS$<>8__locals1.diff.X) - colliderSize.X > Math.Abs(CS$<>8__locals1.diff.Y))
				{
					useLadders = false;
				}
				else if (!this.character.IsClimbing && nextNode != null && nextLadder == null)
				{
					Vector2 diffToNextNode = nextNode.WorldPosition - this.host.WorldPosition;
					if (Math.Abs(diffToNextNode.X) > Math.Abs(diffToNextNode.Y))
					{
						useLadders = false;
					}
				}
				else if (isDiving && this.steering.Y < 1f)
				{
					useLadders = false;
				}
			}
			if (this.character.IsClimbing && !useLadders)
			{
				if (this.currentPath.IsAtEndNode && canClimb && ladders != null)
				{
					useLadders = true;
				}
				else
				{
					this.character.StopClimbing();
				}
			}
			if (useLadders && this.character.SelectedSecondaryItem != ladders.Item)
			{
				if (this.character.CanInteractWith(ladders.Item, true))
				{
					ladders.Item.TryInteract(this.character, false, true, false);
				}
				else
				{
					WayPoint prevNode = this.currentPath.PrevNode;
					Ladder previousLadders = (prevNode != null) ? prevNode.Ladders : null;
					if (previousLadders != null && previousLadders != ladders && this.character.SelectedSecondaryItem != previousLadders.Item && this.character.CanInteractWith(previousLadders.Item, true) && Math.Abs(previousLadders.Item.WorldPosition.X - ladders.Item.WorldPosition.X) < 5f)
					{
						previousLadders.Item.TryInteract(this.character, false, true, false);
					}
				}
			}
			if (this.character.IsClimbing && useLadders)
			{
				if (currentLadder == null && nextLadder != null && this.character.SelectedSecondaryItem == nextLadder.Item)
				{
					return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
				}
				bool nextLadderSameAsCurrent = currentLadder == nextLadder;
				float distanceMargin = colliderSize.X;
				if (currentLadder != null && nextLadder != null)
				{
					CS$<>8__locals1.diff.X = 0f;
				}
				if (verticalDistance < colliderHeight / 2f * 1.25f)
				{
					if (nextLadder != null && !nextLadderSameAsCurrent && this.character.SelectedSecondaryItem != nextLadder.Item && this.character.CanInteractWith(nextLadder.Item, true) && nextLadder.Item.TryInteract(this.character, false, true, false))
					{
						return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
					}
					bool isAboveFloor;
					if (CS$<>8__locals1.diff.Y < 0f)
					{
						float colliderBottom = this.character.AnimController.Collider.SimPosition.Y;
						float floorY = this.character.AnimController.FloorY;
						isAboveFloor = (colliderBottom > floorY);
					}
					else
					{
						float heightFromFloor = this.character.AnimController.GetHeightFromFloor();
						isAboveFloor = (heightFromFloor > -0.1f);
					}
					if (isAboveFloor)
					{
						if (verticalDistance < distanceMargin)
						{
							return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
						}
						if (!this.currentPath.IsAtEndNode && (nextLadder == null || (currentLadder != null && Math.Abs(currentLadder.Item.WorldPosition.X - nextLadder.Item.WorldPosition.X) > distanceMargin)))
						{
							this.character.StopClimbing();
						}
					}
				}
				else if (currentLadder != null && nextNode != null)
				{
					if (Math.Sign(currentNode.WorldPosition.Y - this.character.WorldPosition.Y) != Math.Sign(nextNode.WorldPosition.Y - this.character.WorldPosition.Y))
					{
						return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
					}
					if (nextLadderSameAsCurrent && CS$<>8__locals1.diff.Y < 0f && nextNode.WorldPosition.Y > currentNode.WorldPosition.Y)
					{
						return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
					}
				}
			}
			else if (this.character.AnimController.InWater)
			{
				Door door = currentNode.ConnectedDoor;
				if (door == null || door.CanBeTraversed)
				{
					float distanceMultiplier = MathHelper.Lerp(1f, 5f, MathHelper.Clamp(collider.LinearVelocity.Length() / 10f, 0f, 1f));
					float targetDistance = Math.Max(Math.Max(colliderSize.X, colliderSize.Y) / 2f * distanceMultiplier, 0.5f);
					float modifiedVerticalDist = verticalDistance;
					if (this.character.CurrentHull != currentNode.CurrentHull)
					{
						modifiedVerticalDist *= 2f;
					}
					float distance = horizontalDistance + modifiedVerticalDist;
					if (distance < targetDistance)
					{
						return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
					}
				}
			}
			else
			{
				Vector2 colliderBottom2 = this.character.AnimController.GetColliderBottom();
				if (this.character.Submarine != currentNode.Submarine)
				{
					colliderBottom2 = Submarine.GetRelativeSimPosition(colliderBottom2, currentNode.Submarine, this.character.Submarine);
				}
				Vector2 velocity = collider.LinearVelocity;
				float minHeight = 1.6125001f;
				float minWidth = 0.3225f;
				float characterHeight = Math.Max(ConvertUnits.ToSimUnits(colliderHeight) + this.character.AnimController.ColliderHeightFromFloor, minHeight);
				bool isTargetTooHigh = currentNode.SimPosition.Y > colliderBottom2.Y + characterHeight;
				bool isTargetTooLow = currentNode.SimPosition.Y < colliderBottom2.Y;
				Door door2 = currentNode.ConnectedDoor;
				float targetDistanceMultiplier = MathHelper.Lerp(1f, 10f, MathHelper.Clamp(Math.Abs(velocity.X) / 5f, 0f, 1f));
				if (currentNode.Stairs == null)
				{
					bool isBelowEnough = (nextNode ?? currentNode).WorldPosition.Y < this.character.WorldPosition.Y - colliderHeight / 2f;
					bool drop = false;
					if (isBelowEnough && !canClimb)
					{
						Door nextDoor = door2 ?? ((nextNode != null) ? nextNode.ConnectedDoor : null);
						if (nextDoor != null && nextDoor.IsHorizontal && nextDoor.CanBeTraversed)
						{
							Door openHatch = nextDoor;
							bool isHatchBelowCharacter = openHatch.LinkedGap.WorldPosition.Y < this.character.WorldPosition.Y;
							if (isHatchBelowCharacter)
							{
								drop = true;
							}
						}
						else if (currentLadder != null && !isTargetTooLow && nextDoor == null)
						{
							drop = true;
						}
					}
					if (drop)
					{
						return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
					}
					if (verticalDistance < colliderHeight / 2f)
					{
						CS$<>8__locals1.diff.Y = 0f;
					}
				}
				else if (((nextNode != null) ? nextNode.Stairs : null) != currentNode.Stairs)
				{
					targetDistanceMultiplier = 1f;
					if (currentNode.SimPosition.Y < colliderBottom2.Y + this.character.AnimController.ColliderHeightFromFloor * 0.25f)
					{
						isTargetTooLow = true;
					}
					Structure nextStairs = (nextNode != null) ? nextNode.Stairs : null;
					if (this.character.AnimController.Stairs != null && nextStairs != null)
					{
						if (this.character.AnimController.Stairs.StairDirection == Direction.Right)
						{
							CS$<>8__locals1.diff = ((nextStairs.WorldPosition.Y > this.character.AnimController.Stairs.WorldPosition.Y) ? Vector2.UnitX : (-Vector2.UnitX));
						}
						else
						{
							CS$<>8__locals1.diff = ((nextStairs.WorldPosition.Y > this.character.AnimController.Stairs.WorldPosition.Y) ? (-Vector2.UnitX) : Vector2.UnitX);
						}
					}
				}
				float targetDistance2 = Math.Max(colliderSize.X / 2f * targetDistanceMultiplier, ConvertUnits.ToDisplayUnits(minWidth / 2f));
				if (!isTargetTooHigh && !isTargetTooLow && horizontalDistance < targetDistance2)
				{
					bool isBlockedByDoor = door2 != null && !door2.CanBeTraversed;
					bool notOnLadders = currentLadder == null || nextLadder == null;
					if (!isBlockedByDoor && notOnLadders)
					{
						return this.<GetDiffAndAdvance>g__NextNode|41_0(!doorsChecked, ref CS$<>8__locals1);
					}
				}
			}
			return this.<GetDiffAndAdvance>g__ReturnDiff|41_1(ref CS$<>8__locals1);
		}

		// Token: 0x06002AEB RID: 10987 RVA: 0x001D9964 File Offset: 0x001D7B64
		public bool CanAccessDoor(Door door, Func<Controller, bool> buttonFilter = null)
		{
			IndoorsSteeringManager.<>c__DisplayClass42_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.buttonFilter = buttonFilter;
			if (door.CanBeTraversed)
			{
				return true;
			}
			if (door.IsClosed)
			{
				if (!door.Item.IsInteractable(this.character))
				{
					return false;
				}
				if (!this.ShouldBreakDoor(door))
				{
					if (door.IsStuck || door.IsJammed)
					{
						return false;
					}
					if (!this.canOpenDoors || this.character.LockHands)
					{
						return false;
					}
				}
			}
			if (door.HasIntegratedButtons)
			{
				return door.IsOpen || door.HasAccess(this.character) || this.ShouldBreakDoor(door);
			}
			bool canAccessButtons = false;
			bool buttonsFound = false;
			foreach (Controller button in door.Item.GetConnectedComponents<Controller>(true, true, delegate(Connection c)
			{
				string name = c.Name;
				return name == "toggle" || name == "set_state";
			}))
			{
				buttonsFound = true;
				if (this.<CanAccessDoor>g__CanAccessButton|42_0(button, ref CS$<>8__locals1))
				{
					canAccessButtons = true;
				}
			}
			if (!canAccessButtons)
			{
				foreach (MapEntity linked in door.Item.linkedTo)
				{
					Item linkedItem = linked as Item;
					if (linkedItem != null)
					{
						Controller button2 = linkedItem.GetComponent<Controller>();
						if (button2 != null)
						{
							buttonsFound = true;
							if (this.<CanAccessDoor>g__CanAccessButton|42_0(button2, ref CS$<>8__locals1))
							{
								canAccessButtons = true;
							}
						}
					}
				}
			}
			if (door.IsOpen || this.ShouldBreakDoor(door))
			{
				return true;
			}
			if (!buttonsFound)
			{
				return door.HasAccess(this.character);
			}
			return canAccessButtons;
		}

		// Token: 0x06002AEC RID: 10988 RVA: 0x001D9B10 File Offset: 0x001D7D10
		private float GetColliderLength()
		{
			Vector2 colliderSize = this.character.AnimController.Collider.GetSize();
			return ConvertUnits.ToDisplayUnits(Math.Max(colliderSize.X, colliderSize.Y));
		}

		// Token: 0x06002AED RID: 10989 RVA: 0x001D9B49 File Offset: 0x001D7D49
		private float GetDoorCheckTime()
		{
			if (this.steering.LengthSquared() <= 0f)
			{
				return float.PositiveInfinity;
			}
			if (!this.character.AnimController.IsMovingFast)
			{
				return 0.3f;
			}
			return 0.1f;
		}

		// Token: 0x06002AEE RID: 10990 RVA: 0x001D9B80 File Offset: 0x001D7D80
		private void CheckDoorsInPath()
		{
			this.checkDoorsTimer = this.GetDoorCheckTime();
			if (!this.canOpenDoors)
			{
				return;
			}
			int i = 0;
			while (i < 5)
			{
				WayPoint nextWaypoint = null;
				Door door = null;
				bool shouldBeOpen = false;
				if (this.currentPath.Nodes.Count == 1)
				{
					door = this.currentPath.Nodes.First<WayPoint>().ConnectedDoor;
					shouldBeOpen = (door != null);
					if (i > 0)
					{
						return;
					}
					goto IL_286;
				}
				else
				{
					WayPoint currentWaypoint;
					if (i == 0)
					{
						currentWaypoint = this.currentPath.CurrentNode;
						nextWaypoint = this.currentPath.NextNode;
					}
					else
					{
						int previousIndex = this.currentPath.CurrentIndex - i;
						if (previousIndex < 0)
						{
							break;
						}
						currentWaypoint = this.currentPath.Nodes[previousIndex];
						nextWaypoint = this.currentPath.CurrentNode;
					}
					if (((currentWaypoint != null) ? currentWaypoint.ConnectedDoor : null) != null)
					{
						if (nextWaypoint == null)
						{
							Gap linkedGap = currentWaypoint.ConnectedDoor.LinkedGap;
							if (linkedGap != null)
							{
								if (currentWaypoint.Submarine != null)
								{
									SubmarineInfo info = currentWaypoint.Submarine.Info;
									if ((info == null || info.IsPlayer) && linkedGap.IsRoomToRoom)
									{
										if (!linkedGap.IsRoomToRoom)
										{
											goto IL_286;
										}
										Hull currentHull = currentWaypoint.CurrentHull;
										if (currentHull == null || currentHull.IsWetRoom)
										{
											goto IL_286;
										}
									}
								}
								shouldBeOpen = true;
								door = currentWaypoint.ConnectedDoor;
								goto IL_286;
							}
							goto IL_286;
						}
						else
						{
							float colliderLength = this.GetColliderLength();
							door = currentWaypoint.ConnectedDoor;
							if (door.LinkedGap.IsHorizontal)
							{
								int dir = Math.Sign(nextWaypoint.WorldPosition.X - door.Item.WorldPosition.X);
								float size = this.character.AnimController.InWater ? colliderLength : ConvertUnits.ToDisplayUnits(this.character.AnimController.Collider.GetSize()).X;
								shouldBeOpen = ((door.Item.WorldPosition.X - this.character.WorldPosition.X) * (float)dir > -size);
								goto IL_286;
							}
							int dir2 = Math.Sign(nextWaypoint.WorldPosition.Y - door.Item.WorldPosition.Y);
							shouldBeOpen = ((door.Item.WorldPosition.Y - this.character.WorldPosition.Y) * (float)dir2 > -colliderLength);
							goto IL_286;
						}
					}
				}
				IL_548:
				i++;
				continue;
				IL_286:
				if (door == null)
				{
					return;
				}
				if (door.BotsShouldKeepOpen)
				{
					shouldBeOpen = true;
				}
				if ((door.IsOpen || door.IsBroken) == shouldBeOpen)
				{
					goto IL_548;
				}
				if (!shouldBeOpen)
				{
					AIController aicontroller = this.character.AIController;
					HumanAIController humanAI = aicontroller as HumanAIController;
					if (humanAI != null)
					{
						if (!this.character.IsBot)
						{
							goto IL_35C;
						}
						Submarine submarine = door.Item.Submarine;
						CharacterTeamType? characterTeamType = (submarine != null) ? new CharacterTeamType?(submarine.TeamID) : null;
						CharacterTeamType teamID = this.character.TeamID;
						if (!(characterTeamType.GetValueOrDefault() == teamID & characterTeamType != null))
						{
							goto IL_35C;
						}
						bool flag = true;
						IL_389:
						if (!flag)
						{
							return;
						}
						Hull currentHull = door.Item.CurrentHull;
						bool flag2;
						if (currentHull == null || !currentHull.IsWetRoom)
						{
							currentHull = this.character.CurrentHull;
							flag2 = (currentHull != null && currentHull.IsWetRoom);
						}
						else
						{
							flag2 = true;
						}
						if (!flag2 && Character.CharacterList.Any((Character c) => c != this.character && humanAI.IsFriendly(c, false) && humanAI.VisibleHulls.Contains(c.CurrentHull) && !c.IsUnconscious))
						{
							return;
						}
						goto IL_3F3;
						IL_35C:
						flag = (this.character.Params.AI != null && this.character.Params.AI.KeepDoorsClosed);
						goto IL_389;
					}
				}
				IL_3F3:
				Controller closestButton = null;
				float closestDist = 0f;
				bool canAccess = this.CanAccessDoor(door, delegate(Controller button)
				{
					if (nextWaypoint != null)
					{
						if (door.LinkedGap.IsHorizontal)
						{
							int dir3 = Math.Sign(nextWaypoint.WorldPosition.X - door.Item.WorldPosition.X);
							if (button.Item.WorldPosition.X * (float)dir3 > door.Item.WorldPosition.X * (float)dir3)
							{
								return false;
							}
						}
						else
						{
							int dir4 = Math.Sign(nextWaypoint.WorldPosition.Y - door.Item.WorldPosition.Y);
							if (button.Item.WorldPosition.Y * (float)dir4 > door.Item.WorldPosition.Y * (float)dir4)
							{
								return false;
							}
						}
					}
					float distance = Vector2.DistanceSquared(button.Item.WorldPosition, this.character.WorldPosition);
					if (door.Item.linkedTo.Contains(button.Item))
					{
						distance *= 0.1f;
					}
					if ((closestButton == null || distance < closestDist) && distance < MathUtils.Pow2(button.Item.InteractDistance + this.GetColliderLength()) && this.character.CanSeeTarget(button.Item, null, false, false))
					{
						closestButton = button;
						closestDist = distance;
					}
					return closestButton != null;
				});
				if (canAccess)
				{
					bool pressButton = this.buttonPressTimer <= 0f || this.lastDoor.Item1 != door || this.lastDoor.Item2 != shouldBeOpen;
					if (door.HasIntegratedButtons)
					{
						if (!pressButton || !this.character.CanSeeTarget(door.Item, null, false, false))
						{
							break;
						}
						if (door.Item.TryInteract(this.character, false, true, false))
						{
							this.lastDoor = new ValueTuple<Door, bool>(door, shouldBeOpen);
							this.buttonPressTimer = (shouldBeOpen ? 1f : 0f);
							return;
						}
						this.buttonPressTimer = 0f;
						return;
					}
					else
					{
						if (closestButton == null)
						{
							goto IL_548;
						}
						if (!pressButton)
						{
							break;
						}
						if (closestButton.Item.TryInteract(this.character, false, true, false))
						{
							this.lastDoor = new ValueTuple<Door, bool>(door, shouldBeOpen);
							this.buttonPressTimer = (shouldBeOpen ? 1f : 0f);
							return;
						}
						this.buttonPressTimer = 0f;
						return;
					}
				}
				else
				{
					if (shouldBeOpen)
					{
						this.currentPath.Unreachable = true;
						return;
					}
					goto IL_548;
				}
			}
		}

		// Token: 0x06002AEF RID: 10991 RVA: 0x001DA0E0 File Offset: 0x001D82E0
		private float? GetNodePenalty(PathNode node, PathNode nextNode)
		{
			if (this.character == null)
			{
				return new float?(0f);
			}
			float? penalty = this.GetSingleNodePenalty(nextNode);
			if (penalty == null)
			{
				return null;
			}
			Vector2 nextNodePosition = nextNode.Position;
			if (nextNode.Waypoint.Submarine != node.Waypoint.Submarine)
			{
				nextNodePosition = Submarine.GetRelativeSimPosition(nextNodePosition, node.Waypoint.Submarine, nextNode.Waypoint.Submarine);
			}
			bool nextNodeAboveWaterLevel = nextNode.Waypoint.CurrentHull != null && nextNode.Waypoint.CurrentHull.Surface < nextNode.Waypoint.Position.Y;
			if (!this.character.CanClimb && node.Waypoint.Stairs == null && nextNode.Waypoint.Stairs == null && ((node.Waypoint.Ladders != null && nextNode.Waypoint.Ladders != null && (!nextNode.Waypoint.Ladders.Item.IsInteractable(this.character) || this.character.LockHands)) || (nextNodePosition.Y - node.Position.Y > 1f && nextNodeAboveWaterLevel)))
			{
				return null;
			}
			if (node.Waypoint.CurrentHull != null)
			{
				Hull hull = node.Waypoint.CurrentHull;
				if (hull.FireSources.Count > 0)
				{
					foreach (FireSource fs in hull.FireSources)
					{
						penalty += fs.Size.X * 10f;
					}
				}
				if (this.character.NeedsAir)
				{
					if (hull.WaterVolume / (float)hull.Rect.Width > 100f && !HumanAIController.HasDivingSuit(this.character, 0f, true, true) && this.character.CharacterHealth.OxygenLowResistance < 1f)
					{
						penalty += 500f;
					}
					if (this.character.PressureProtection < 10f && hull.WaterVolume > hull.Volume)
					{
						penalty += 1000f;
					}
				}
				float yDist = Math.Abs(node.Position.Y - nextNodePosition.Y);
				if (nextNodeAboveWaterLevel && node.Waypoint.Ladders == null && nextNode.Waypoint.Ladders == null && node.Waypoint.Stairs == null && nextNode.Waypoint.Stairs == null)
				{
					penalty += yDist * 10f;
				}
			}
			return penalty;
		}

		// Token: 0x06002AF0 RID: 10992 RVA: 0x001DA420 File Offset: 0x001D8620
		private float? GetSingleNodePenalty(PathNode node)
		{
			if (!node.Waypoint.IsTraversable)
			{
				return null;
			}
			if (node.IsBlocked())
			{
				return null;
			}
			float penalty = 0f;
			Gap connectedGap = node.Waypoint.ConnectedGap;
			if (connectedGap != null && connectedGap.Open < 0.9f)
			{
				Door door = node.Waypoint.ConnectedDoor;
				if (door == null)
				{
					penalty = 100f;
				}
				else if (!this.CanAccessDoor(door, delegate(Controller button)
				{
					if (Vector2.DistanceSquared(door.Item.WorldPosition, button.Item.WorldPosition) > MathUtils.Pow2(button.Item.InteractDistance + this.GetColliderLength()))
					{
						return false;
					}
					if (!ISpatialEntity.IsTargetVisible(button.Item, door.Item, false, false))
					{
						return false;
					}
					if (door.IsHorizontal)
					{
						if (Math.Sign(button.Item.WorldPosition.Y - door.Item.WorldPosition.Y) != Math.Sign(this.character.WorldPosition.Y - door.Item.WorldPosition.Y))
						{
							MotionSensor ms = door.Item.GetDirectlyConnectedComponent<MotionSensor>(null);
							return ms != null && ms.TriggersOn(this.character);
						}
					}
					else if (Math.Sign(button.Item.WorldPosition.X - door.Item.WorldPosition.X) != Math.Sign(this.character.WorldPosition.X - door.Item.WorldPosition.X))
					{
						MotionSensor ms2 = door.Item.GetDirectlyConnectedComponent<MotionSensor>(null);
						return ms2 != null && ms2.TriggersOn(this.character);
					}
					return true;
				}))
				{
					return null;
				}
			}
			return new float?(penalty);
		}

		// Token: 0x06002AF1 RID: 10993 RVA: 0x001DA4D0 File Offset: 0x001D86D0
		public void Wander(float deltaTime, float wallAvoidDistance = 150f, bool stayStillInTightSpace = true)
		{
			bool wander = false;
			bool inWater = this.character.AnimController.InWater;
			Limb limb = this.character.AnimController.GetLimb(LimbType.RightLeg, true, false, false);
			Hull hull2;
			if ((hull2 = ((limb != null) ? limb.Hull : null)) == null)
			{
				Limb limb2 = this.character.AnimController.GetLimb(LimbType.LeftLeg, true, false, false);
				hull2 = (((limb2 != null) ? limb2.Hull : null) ?? this.character.CurrentHull);
			}
			Hull currentHull = hull2;
			if (currentHull != null && !inWater)
			{
				float roomWidth = (float)currentHull.Rect.Width;
				if (stayStillInTightSpace && roomWidth < Math.Max(wallAvoidDistance * 3f, IndoorsSteeringManager.smallRoomSize))
				{
					base.Reset();
				}
				else
				{
					bool isVerySmallRoom = roomWidth < IndoorsSteeringManager.smallRoomSize;
					Hull nextRoom = null;
					if (!stayStillInTightSpace && isVerySmallRoom)
					{
						float closestDistance = 0f;
						foreach (Gap gap in currentHull.ConnectedGaps)
						{
							if (gap.Open >= 1f)
							{
								float feetPos = ConvertUnits.ToDisplayUnits(this.character.AnimController.FloorY);
								float gapTop = (float)gap.Rect.Y;
								float gapBottom = (float)(gap.Rect.Y - gap.Rect.Height);
								if (this.character.Position.Y <= gapTop && feetPos >= gapBottom - 25f)
								{
									Hull room = null;
									foreach (MapEntity entity in gap.linkedTo)
									{
										Hull hull = entity as Hull;
										if (hull != null && hull.Submarine == this.character.Submarine && (float)hull.Rect.Width >= IndoorsSteeringManager.smallRoomSize && hull != currentHull)
										{
											room = hull;
											break;
										}
									}
									if (room != null)
									{
										Vector2 toGap = gap.Position - this.character.Position;
										float distance = Math.Abs(toGap.X);
										if (nextRoom == null || distance < closestDistance)
										{
											closestDistance = distance;
											nextRoom = room;
										}
									}
								}
							}
						}
					}
					if (nextRoom != null)
					{
						float toNextRoom = nextRoom.Position.X - this.character.Position.X;
						base.SteeringManual(deltaTime, Vector2.UnitX * (float)Math.Sign(toNextRoom));
						base.WanderAngle = 0f;
					}
					else
					{
						if (!stayStillInTightSpace && isVerySmallRoom)
						{
							base.Reset();
							return;
						}
						float leftDist = this.character.Position.X - (float)currentHull.Rect.X;
						float rightDist = (float)currentHull.Rect.Right - this.character.Position.X;
						if (leftDist < wallAvoidDistance && rightDist < wallAvoidDistance)
						{
							float diff = rightDist - leftDist;
							if (Math.Abs(diff) > wallAvoidDistance / 2f)
							{
								base.SteeringManual(deltaTime, Vector2.UnitX * (float)Math.Sign(diff));
								return;
							}
							if (stayStillInTightSpace)
							{
								base.Reset();
								return;
							}
						}
						if (leftDist < wallAvoidDistance)
						{
							float speed = (wallAvoidDistance - leftDist) / wallAvoidDistance;
							base.SteeringManual(deltaTime, Vector2.UnitX * MathHelper.Clamp(speed, 0.25f, 1f));
							base.WanderAngle = 0f;
						}
						else if (rightDist < wallAvoidDistance)
						{
							float speed2 = (wallAvoidDistance - rightDist) / wallAvoidDistance;
							base.SteeringManual(deltaTime, -Vector2.UnitX * MathHelper.Clamp(speed2, 0.25f, 1f));
							base.WanderAngle = 3.1415927f;
						}
						else
						{
							wander = true;
						}
					}
				}
			}
			else
			{
				wander = true;
			}
			if (wander)
			{
				base.SteeringWander(1f, false);
				if (inWater)
				{
					base.SteeringAvoid(deltaTime, ConvertUnits.ToSimUnits(wallAvoidDistance), 5f);
				}
			}
			if (!inWater)
			{
				base.ResetY();
			}
		}

		// Token: 0x06002AF3 RID: 10995 RVA: 0x001DA8D4 File Offset: 0x001D8AD4
		[CompilerGenerated]
		private bool <CalculateSteeringSeek>g__IsIdenticalPath|39_0(ref IndoorsSteeringManager.<>c__DisplayClass39_0 A_1)
		{
			int nodeCount = A_1.newPath.Nodes.Count;
			if (nodeCount == this.currentPath.Nodes.Count)
			{
				for (int i = 0; i < nodeCount - 1; i++)
				{
					if (A_1.newPath.Nodes[i] != this.currentPath.Nodes[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002AF4 RID: 10996 RVA: 0x001DA93C File Offset: 0x001D8B3C
		[CompilerGenerated]
		private void <CalculateSteeringSeek>g__SkipCurrentPathNodes|39_1()
		{
			if (!this.character.AnimController.InWater || this.character.Submarine != null)
			{
				return;
			}
			if (this.CurrentPath == null || this.CurrentPath.Unreachable || this.CurrentPath.Finished)
			{
				return;
			}
			if (this.CurrentPath.CurrentIndex < 0 || this.CurrentPath.CurrentIndex >= this.CurrentPath.Nodes.Count - 1)
			{
				return;
			}
			WayPoint lastNode = this.CurrentPath.Nodes.Last<WayPoint>();
			Submarine targetSub = lastNode.Submarine;
			if (targetSub != null)
			{
				float subSize = (float)(Math.Max(targetSub.Borders.Size.X, targetSub.Borders.Size.Y) / 2);
				float margin = 500f;
				if (Vector2.DistanceSquared(this.character.WorldPosition, targetSub.WorldPosition) < MathUtils.Pow2(subSize + margin))
				{
					return;
				}
			}
			float pathDistance = Vector2.Distance(this.character.WorldPosition, this.CurrentPath.CurrentNode.WorldPosition);
			pathDistance += this.CurrentPath.GetLength(new int?(this.CurrentPath.CurrentIndex), null);
			for (int i = this.CurrentPath.Nodes.Count - 1; i > this.CurrentPath.CurrentIndex + 1; i--)
			{
				WayPoint waypoint = this.CurrentPath.Nodes[i];
				float directDistance = Vector2.DistanceSquared(this.character.WorldPosition, waypoint.WorldPosition);
				if (directDistance <= MathUtils.Pow2(pathDistance) && this.character.CanSeeTarget(waypoint, null, false, false))
				{
					this.CurrentPath.SkipToNode(i);
					return;
				}
				pathDistance -= this.CurrentPath.GetLength(new int?(i - 1), new int?(i));
			}
		}

		// Token: 0x06002AF5 RID: 10997 RVA: 0x001DAB1F File Offset: 0x001D8D1F
		[CompilerGenerated]
		private Vector2 <GetDiffAndAdvance>g__NextNode|41_0(bool checkDoors, ref IndoorsSteeringManager.<>c__DisplayClass41_0 A_2)
		{
			if (checkDoors)
			{
				this.CheckDoorsInPath();
			}
			this.currentPath.SkipToNextNode();
			return this.<GetDiffAndAdvance>g__ReturnDiff|41_1(ref A_2);
		}

		// Token: 0x06002AF6 RID: 10998 RVA: 0x001DAB3C File Offset: 0x001D8D3C
		[CompilerGenerated]
		private Vector2 <GetDiffAndAdvance>g__ReturnDiff|41_1(ref IndoorsSteeringManager.<>c__DisplayClass41_0 A_1)
		{
			if (this.currentPath.CurrentNode == null)
			{
				return Vector2.Zero;
			}
			return ConvertUnits.ToSimUnits(A_1.diff);
		}

		// Token: 0x06002AF7 RID: 10999 RVA: 0x001DAB5C File Offset: 0x001D8D5C
		[CompilerGenerated]
		private bool <CanAccessDoor>g__CanAccessButton|42_0(Controller button, ref IndoorsSteeringManager.<>c__DisplayClass42_0 A_2)
		{
			return button.HasAccess(this.character) && (A_2.buttonFilter == null || A_2.buttonFilter(button));
		}

		// Token: 0x04001657 RID: 5719
		private readonly PathFinder pathFinder;

		// Token: 0x04001658 RID: 5720
		private SteeringPath currentPath;

		// Token: 0x04001659 RID: 5721
		private readonly bool canOpenDoors;

		// Token: 0x0400165B RID: 5723
		private readonly Character character;

		// Token: 0x0400165C RID: 5724
		private Vector2 currentTargetPos;

		// Token: 0x0400165D RID: 5725
		private float findPathTimer;

		// Token: 0x0400165E RID: 5726
		private const float ButtonPressCooldown = 1f;

		// Token: 0x0400165F RID: 5727
		private float checkDoorsTimer;

		// Token: 0x04001660 RID: 5728
		private float buttonPressTimer;

		// Token: 0x04001662 RID: 5730
		[TupleElementNames(new string[]
		{
			"door",
			"shouldBeOpen"
		})]
		private ValueTuple<Door, bool> lastDoor;

		// Token: 0x04001663 RID: 5731
		public static float smallRoomSize = 500f;
	}
}
