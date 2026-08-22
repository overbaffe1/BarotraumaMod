using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200001E RID: 30
	internal abstract class AIController : ISteerable
	{
		// Token: 0x06000146 RID: 326 RVA: 0x00007AFB File Offset: 0x00005CFB
		public virtual void DebugDraw(SpriteBatch spriteBatch)
		{
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000147 RID: 327 RVA: 0x00007AFD File Offset: 0x00005CFD
		// (set) Token: 0x06000148 RID: 328 RVA: 0x00007B08 File Offset: 0x00005D08
		public AITarget SelectedAiTarget
		{
			get
			{
				return this._selectedAiTarget;
			}
			protected set
			{
				this._previousAiTarget = this._selectedAiTarget;
				this._selectedAiTarget = value;
				if (this._selectedAiTarget != this._previousAiTarget)
				{
					if (this._previousAiTarget != null)
					{
						this._lastAiTarget = this._previousAiTarget;
						if (this._selectedAiTarget != null)
						{
							Item i = this._selectedAiTarget.Entity as Item;
							if (i != null)
							{
								Character c = this._previousAiTarget.Entity as Character;
								if (c != null)
								{
									if (i.IsOwnedBy(c))
									{
										return;
									}
									goto IL_A4;
								}
							}
							Item it = this._previousAiTarget.Entity as Item;
							if (it != null)
							{
								Character ch = this._selectedAiTarget.Entity as Character;
								if (ch != null && it.IsOwnedBy(ch))
								{
									return;
								}
							}
						}
					}
					IL_A4:
					this.OnTargetChanged(this._previousAiTarget, this._selectedAiTarget);
				}
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000149 RID: 329 RVA: 0x00007BCB File Offset: 0x00005DCB
		public SteeringManager SteeringManager
		{
			get
			{
				return this.steeringManager;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600014A RID: 330 RVA: 0x00007BD3 File Offset: 0x00005DD3
		// (set) Token: 0x0600014B RID: 331 RVA: 0x00007BE5 File Offset: 0x00005DE5
		public Vector2 Steering
		{
			get
			{
				return this.Character.AnimController.TargetMovement;
			}
			set
			{
				this.Character.AnimController.TargetMovement = value;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600014C RID: 332 RVA: 0x00007BF8 File Offset: 0x00005DF8
		public Vector2 SimPosition
		{
			get
			{
				return this.Character.SimPosition;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600014D RID: 333 RVA: 0x00007C05 File Offset: 0x00005E05
		public Vector2 WorldPosition
		{
			get
			{
				return this.Character.WorldPosition;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600014E RID: 334 RVA: 0x00007C12 File Offset: 0x00005E12
		public Vector2 Velocity
		{
			get
			{
				return this.Character.AnimController.Collider.LinearVelocity;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600014F RID: 335 RVA: 0x00007C29 File Offset: 0x00005E29
		public virtual CanEnterSubmarine CanEnterSubmarine
		{
			get
			{
				return this.Character.AnimController.CanEnterSubmarine;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000150 RID: 336 RVA: 0x00007C3B File Offset: 0x00005E3B
		public virtual bool CanFlip
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000151 RID: 337 RVA: 0x00007C3E File Offset: 0x00005E3E
		public virtual bool IsMentallyUnstable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000152 RID: 338 RVA: 0x00007C41 File Offset: 0x00005E41
		// (set) Token: 0x06000153 RID: 339 RVA: 0x00007C62 File Offset: 0x00005E62
		public IEnumerable<Hull> VisibleHulls
		{
			get
			{
				if (this.visibleHulls == null)
				{
					this.visibleHulls = this.Character.GetVisibleHulls();
				}
				return this.visibleHulls;
			}
			private set
			{
				this.visibleHulls = value;
			}
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00007C6C File Offset: 0x00005E6C
		public bool HasValidPath(bool requireNonDirty = true, bool requireUnfinished = true, Func<WayPoint, bool> nodePredicate = null)
		{
			IndoorsSteeringManager pathSteering = this.SteeringManager as IndoorsSteeringManager;
			return pathSteering != null && pathSteering.CurrentPath != null && !pathSteering.CurrentPath.Unreachable && (!requireUnfinished || !pathSteering.CurrentPath.Finished) && (!requireNonDirty || !pathSteering.IsPathDirty) && (nodePredicate == null || pathSteering.CurrentPath.Nodes.All((WayPoint n) => nodePredicate(n)));
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000155 RID: 341 RVA: 0x00007CF8 File Offset: 0x00005EF8
		public bool IsCurrentPathNullOrUnreachable
		{
			get
			{
				if (!this.IsCurrentPathUnreachable)
				{
					IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
					return pathSteering != null && pathSteering.CurrentPath == null;
				}
				return true;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000156 RID: 342 RVA: 0x00007D2C File Offset: 0x00005F2C
		public bool IsCurrentPathUnreachable
		{
			get
			{
				IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
				return pathSteering != null && !pathSteering.IsPathDirty && pathSteering.CurrentPath != null && pathSteering.CurrentPath.Unreachable;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000157 RID: 343 RVA: 0x00007D68 File Offset: 0x00005F68
		public bool IsCurrentPathFinished
		{
			get
			{
				IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
				return pathSteering != null && !pathSteering.IsPathDirty && pathSteering.CurrentPath != null && pathSteering.CurrentPath.Finished;
			}
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00007DA4 File Offset: 0x00005FA4
		public AIController(Character c)
		{
			this.Character = c;
			this.hullVisibilityTimer = Rand.Range(0f, this.hullVisibilityTimer, Rand.RandSync.Unsynced);
			this.Enabled = true;
			Vector2 size = this.Character.AnimController.Collider.GetSize();
			this.colliderWidth = size.X;
			this.colliderLength = size.Y;
			this.avoidLookAheadDistance = Math.Max(Math.Max(this.colliderWidth, this.colliderLength) * 3f, 1.5f);
			this.minGapSize = ConvertUnits.ToDisplayUnits(Math.Min(this.colliderWidth, this.colliderLength));
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00007E6E File Offset: 0x0000606E
		public virtual void OnHealed(Character healer, float healAmount)
		{
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00007E70 File Offset: 0x00006070
		public virtual void OnAttacked(Character attacker, AttackResult attackResult)
		{
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00007E72 File Offset: 0x00006072
		public virtual void SelectTarget(AITarget target)
		{
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00007E74 File Offset: 0x00006074
		public virtual void Update(float deltaTime)
		{
			if (this.hullVisibilityTimer > 0f)
			{
				this.hullVisibilityTimer -= 1f;
				return;
			}
			this.hullVisibilityTimer = 0.5f;
			this.VisibleHulls = this.Character.GetVisibleHulls();
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00007EB2 File Offset: 0x000060B2
		public virtual void Reset()
		{
			this.ResetAITarget();
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00007EBA File Offset: 0x000060BA
		protected void ResetAITarget()
		{
			this._lastAiTarget = null;
			this._selectedAiTarget = null;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00007ECA File Offset: 0x000060CA
		public void FaceTarget(ISpatialEntity target)
		{
			this.Character.AnimController.TargetDir = ((target.WorldPosition.X > this.Character.WorldPosition.X) ? Direction.Right : Direction.Left);
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000160 RID: 352 RVA: 0x00007EFD File Offset: 0x000060FD
		// (set) Token: 0x06000161 RID: 353 RVA: 0x00007F05 File Offset: 0x00006105
		public bool IsSteeringThroughGap { get; protected set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000162 RID: 354 RVA: 0x00007F0E File Offset: 0x0000610E
		// (set) Token: 0x06000163 RID: 355 RVA: 0x00007F16 File Offset: 0x00006116
		public bool IsTryingToSteerThroughGap { get; protected set; }

		// Token: 0x06000164 RID: 356 RVA: 0x00007F20 File Offset: 0x00006120
		public virtual bool SteerThroughGap(Structure wall, WallSection section, Vector2 targetWorldPos, float deltaTime)
		{
			if (wall == null)
			{
				return false;
			}
			if (section == null)
			{
				return false;
			}
			Gap gap = section.gap;
			if (gap == null)
			{
				return false;
			}
			float maxDistance = (float)Math.Min(wall.Rect.Width, wall.Rect.Height);
			if (Vector2.DistanceSquared(this.Character.WorldPosition, targetWorldPos) > maxDistance * maxDistance)
			{
				return false;
			}
			Hull targetHull = gap.FlowTargetHull;
			if (targetHull == null)
			{
				return false;
			}
			if (wall.IsHorizontal)
			{
				targetWorldPos.Y = (float)(targetHull.WorldRect.Y - targetHull.Rect.Height / 2);
			}
			else
			{
				targetWorldPos.X = (float)targetHull.WorldRect.Center.X;
			}
			return this.SteerThroughGap(gap, targetWorldPos, deltaTime, -1f);
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00007FDC File Offset: 0x000061DC
		public virtual bool SteerThroughGap(Gap gap, Vector2 targetWorldPos, float deltaTime, float maxDistance = -1f)
		{
			if (gap.FlowTargetHull == null)
			{
				return false;
			}
			if (maxDistance > 0f && Vector2.DistanceSquared(this.Character.WorldPosition, gap.WorldPosition) > maxDistance * maxDistance)
			{
				return false;
			}
			IndoorsSteeringManager pathSteering = this.SteeringManager as IndoorsSteeringManager;
			if (pathSteering != null)
			{
				pathSteering.ResetPath();
			}
			this.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(targetWorldPos - this.Character.WorldPosition));
			return true;
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00008058 File Offset: 0x00006258
		public bool CanPassThroughHole(Structure wall, int sectionIndex, int requiredHoleCount)
		{
			if (!wall.SectionBodyDisabled(sectionIndex))
			{
				return false;
			}
			int holeCount = 1;
			int i = sectionIndex - 1;
			while (i > sectionIndex - requiredHoleCount && wall.SectionBodyDisabled(i))
			{
				holeCount++;
				i--;
			}
			int j = sectionIndex + 1;
			while (j < sectionIndex + requiredHoleCount && wall.SectionBodyDisabled(j))
			{
				holeCount++;
				j++;
			}
			return holeCount >= requiredHoleCount;
		}

		// Token: 0x06000167 RID: 359 RVA: 0x000080B4 File Offset: 0x000062B4
		protected bool IsWallDisabled(Structure wall)
		{
			bool isDisabled = true;
			for (int i = 0; i < wall.Sections.Length; i++)
			{
				if (!wall.SectionBodyDisabled(i))
				{
					isDisabled = false;
					break;
				}
			}
			return isDisabled;
		}

		// Token: 0x06000168 RID: 360 RVA: 0x000080E4 File Offset: 0x000062E4
		public bool TakeItem(Item item, CharacterInventory targetInventory, bool equip, bool wear = false, bool dropOtherIfCannotMove = true, bool allowSwapping = false, bool storeUnequipped = false, IEnumerable<Identifier> targetTags = null)
		{
			Pickable pickable = item.GetComponent<Pickable>();
			if (pickable == null)
			{
				return false;
			}
			if (wear)
			{
				Wearable wearable = item.GetComponent<Wearable>();
				if (wearable != null)
				{
					pickable = wearable;
				}
			}
			else
			{
				pickable = item.GetComponent<Holdable>();
			}
			ItemInventory itemInventory = item.ParentInventory as ItemInventory;
			if (itemInventory != null && !itemInventory.Container.HasRequiredItems(this.Character, false, null))
			{
				return false;
			}
			if (equip && pickable != null)
			{
				int targetSlot = -1;
				foreach (InvSlotType slots in pickable.AllowedSlots)
				{
					if (!slots.HasFlag(InvSlotType.Any) && (wear || slots == InvSlotType.RightHand || slots == InvSlotType.LeftHand || slots == (InvSlotType.RightHand | InvSlotType.LeftHand)))
					{
						for (int i = 0; i < targetInventory.Capacity; i++)
						{
							if (targetInventory == null || slots.HasFlag(targetInventory.SlotTypes[i]))
							{
								targetSlot = i;
								Item otherItem = targetInventory.GetItemAt(i);
								if (otherItem != null)
								{
									if (!otherItem.IsInteractable(this.Character))
									{
										return false;
									}
									if (otherItem.AllowedSlots.Contains(InvSlotType.Any) && targetInventory.TryPutItem(otherItem, this.Character, CharacterInventory.AnySlot, true, false, true))
									{
										if (storeUnequipped && targetInventory.Owner == this.Character)
										{
											this.unequippedItems.Add(otherItem);
										}
									}
									else if (dropOtherIfCannotMove)
									{
										if (otherItem.Prefab.Identifier == item.Prefab.Identifier || (targetTags != null && otherItem.HasIdentifierOrTags(targetTags)))
										{
											if (targetTags == null)
											{
												goto IL_1C5;
											}
											Identifier identifier = targetTags.FirstOrDefault<Identifier>();
											if (!(identifier == Tags.HeavyDivingGear) || !AIObjectiveFindDivingGear.IsSuitablePressureProtection(item, Tags.HeavyDivingGear, this.Character))
											{
												goto IL_1C5;
											}
											bool flag = !AIObjectiveFindDivingGear.IsSuitablePressureProtection(otherItem, Tags.HeavyDivingGear, this.Character);
											IL_1C6:
											if (!flag)
											{
												return false;
											}
											goto IL_1D4;
											IL_1C5:
											flag = false;
											goto IL_1C6;
										}
										IL_1D4:
										otherItem.Drop(this.Character, true, true);
									}
								}
							}
						}
					}
				}
				if (targetSlot < 0)
				{
					return false;
				}
				if (pickable.AllowedSlots.Contains(InvSlotType.Any))
				{
					targetInventory.TryPutItem(item, this.Character, CharacterInventory.AnySlot, true, false, true);
				}
				return targetInventory.TryPutItem(item, targetSlot, allowSwapping, false, this.Character, true, false, true);
			}
			return targetInventory.TryPutItem(item, this.Character, CharacterInventory.AnySlot, true, false, true);
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00008378 File Offset: 0x00006578
		public void UnequipEmptyItems(Item parentItem, bool avoidDroppingInSea = true, bool allowDestroying = false)
		{
			AIController.UnequipEmptyItems(this.Character, parentItem, avoidDroppingInSea, allowDestroying);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00008388 File Offset: 0x00006588
		public void UnequipContainedItems(Item parentItem, Func<Item, bool> predicate = null, bool avoidDroppingInSea = true, bool allowDestroying = false, int? unequipMax = null)
		{
			AIController.UnequipContainedItems(this.Character, parentItem, predicate, avoidDroppingInSea, allowDestroying, unequipMax);
		}

		// Token: 0x0600016B RID: 363 RVA: 0x0000839C File Offset: 0x0000659C
		public static void UnequipEmptyItems(Character character, Item parentItem, bool avoidDroppingInSea = true, bool allowDestroying = false)
		{
			AIController.UnequipContainedItems(character, parentItem, (Item it) => it.Condition <= 0f, avoidDroppingInSea, allowDestroying, null);
		}

		// Token: 0x0600016C RID: 364 RVA: 0x000083DC File Offset: 0x000065DC
		public static void UnequipContainedItems(Character character, Item parentItem, Func<Item, bool> predicate, bool avoidDroppingInSea = true, bool allowDestroying = false, int? unequipMax = null)
		{
			ItemInventory inventory = parentItem.OwnInventory;
			if (inventory == null || !inventory.Container.DrawInventory)
			{
				return;
			}
			int removed = 0;
			if (predicate == null || inventory.AllItems.Any(predicate))
			{
				foreach (Item containedItem in inventory.AllItemsMod)
				{
					if (containedItem != null && (predicate == null || predicate(containedItem)))
					{
						if (allowDestroying)
						{
							NetworkMember networkMember = GameMain.NetworkMember;
							if ((networkMember == null || !networkMember.IsClient) && character.AIController.HasInfiniteItemSpawns(containedItem.Prefab.Identifier))
							{
								EntitySpawner spawner = Entity.Spawner;
								if (spawner == null)
								{
									goto IL_F3;
								}
								spawner.AddItemToRemoveQueue(containedItem);
								goto IL_F3;
							}
						}
						if (avoidDroppingInSea && !character.IsInFriendlySub && character.Inventory.TryPutItem(containedItem, character, CharacterInventory.AnySlot, true, false, true))
						{
							if (unequipMax == null)
							{
								continue;
							}
							int num = ++removed;
							int? num2 = unequipMax;
							if (num >= num2.GetValueOrDefault() & num2 != null)
							{
								break;
							}
							continue;
						}
						else
						{
							containedItem.Drop(character, true, true);
						}
						IL_F3:
						if (unequipMax != null)
						{
							int num3 = ++removed;
							int? num2 = unequipMax;
							if (num3 >= num2.GetValueOrDefault() & num2 != null)
							{
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00008530 File Offset: 0x00006730
		public bool HasInfiniteItemSpawns(IEnumerable<Identifier> itemIdentifiers)
		{
			HumanPrefab humanPrefab = this.Character.HumanPrefab;
			if (humanPrefab == null || !humanPrefab.InfiniteItems.Any((ItemPrefab it) => itemIdentifiers.Contains(it.Identifier) || it.Tags.Any(new Func<Identifier, bool>(itemIdentifiers.Contains<Identifier>))))
			{
				CharacterInfo info = this.Character.Info;
				bool? flag;
				if (info == null)
				{
					flag = null;
				}
				else
				{
					Job job = info.Job;
					flag = ((job != null) ? new bool?(job.HasJobItem((JobPrefab.JobItem jobItem) => jobItem.Infinite && itemIdentifiers.Contains(jobItem.GetItemIdentifier(this.Character.TeamID, GameMain.GameSession.GameMode is PvPMode)))) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
			return true;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x000085CC File Offset: 0x000067CC
		public bool HasInfiniteItemSpawns(Identifier itemIdentifier)
		{
			HumanPrefab humanPrefab = this.Character.HumanPrefab;
			if (humanPrefab == null || !humanPrefab.InfiniteItems.Any((ItemPrefab it) => it.Identifier == itemIdentifier || it.Tags.Contains(itemIdentifier)))
			{
				CharacterInfo info = this.Character.Info;
				bool? flag;
				if (info == null)
				{
					flag = null;
				}
				else
				{
					Job job = info.Job;
					flag = ((job != null) ? new bool?(job.HasJobItem(delegate(JobPrefab.JobItem jobItem)
					{
						if (jobItem.Infinite)
						{
							Identifier itemIdentifier2 = jobItem.GetItemIdentifier(this.Character.TeamID, GameMain.GameSession.GameMode is PvPMode);
							return itemIdentifier2 == itemIdentifier;
						}
						return false;
					})) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
			return true;
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00008668 File Offset: 0x00006868
		public void ReequipUnequipped()
		{
			foreach (Item item in this.unequippedItems)
			{
				if (item != null && !item.Removed && this.Character.HasItem(item, false, null))
				{
					this.TakeItem(item, this.Character.Inventory, true, true, true, true, false, null);
				}
			}
			this.unequippedItems.Clear();
		}

		// Token: 0x06000170 RID: 368
		public abstract bool Escape(float deltaTime);

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000171 RID: 369 RVA: 0x000086FC File Offset: 0x000068FC
		// (set) Token: 0x06000172 RID: 370 RVA: 0x00008704 File Offset: 0x00006904
		public Gap EscapeTarget { get; private set; }

		// Token: 0x06000173 RID: 371 RVA: 0x00008710 File Offset: 0x00006910
		protected bool UpdateEscape(float deltaTime, bool canAttackDoors)
		{
			IndoorsSteeringManager pathSteering = this.SteeringManager as IndoorsSteeringManager;
			if (this.allGapsSearched)
			{
				this.escapeTimer -= deltaTime;
				if (this.escapeTimer <= 0f)
				{
					this.allGapsSearched = false;
				}
			}
			if (this.Character.CurrentHull != null && pathSteering != null)
			{
				if (!this.allGapsSearched)
				{
					float closestDistance = 0f;
					foreach (Gap gap in Gap.GapList)
					{
						if (gap != null && !gap.Removed && this.EscapeTarget != gap && !this.unreachableGaps.Contains(gap) && gap.Submarine == this.Character.Submarine && !gap.IsRoomToRoom)
						{
							float multiplier = 1f;
							Door door = gap.ConnectedDoor;
							if (door != null)
							{
								if (!pathSteering.CanAccessDoor(door, null))
								{
									continue;
								}
							}
							else if (gap.Open < 1f || gap.Size < this.minGapSize)
							{
								continue;
							}
							if (gap.FlowTargetHull == this.Character.CurrentHull)
							{
								this.EscapeTarget = gap;
								break;
							}
							float distance = Vector2.DistanceSquared(this.Character.WorldPosition, gap.WorldPosition) * multiplier;
							if (this.EscapeTarget == null || distance < closestDistance)
							{
								this.EscapeTarget = gap;
								closestDistance = distance;
							}
						}
					}
					this.allGapsSearched = true;
					this.escapeTimer = this.escapeTargetSeekInterval;
				}
				else if (this.EscapeTarget != null && this.EscapeTarget.FlowTargetHull != this.Character.CurrentHull && this.IsCurrentPathUnreachable)
				{
					this.unreachableGaps.Add(this.EscapeTarget);
					this.EscapeTarget = null;
					this.allGapsSearched = false;
				}
			}
			if (this.EscapeTarget != null)
			{
				Door door2 = this.EscapeTarget.ConnectedDoor;
				bool isClosedDoor = door2 != null && door2.IsClosed;
				Vector2 diff = this.EscapeTarget.WorldPosition - this.Character.WorldPosition;
				float sqrDist = diff.LengthSquared();
				bool isClose = sqrDist < MathUtils.Pow2(100f);
				if (this.Character.CurrentHull == null || (isClose && !isClosedDoor) || pathSteering == null || this.IsCurrentPathUnreachable || this.IsCurrentPathFinished)
				{
					this.Character.ReleaseSecondaryItem();
					this.SteeringManager.Reset();
					if (pathSteering != null)
					{
						pathSteering.ResetPath();
					}
					Vector2 dir = Vector2.Normalize(diff);
					if (this.Character.CurrentHull == null || isClose)
					{
						if (this.EscapeTarget.FlowTargetHull != null)
						{
							this.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(this.EscapeTarget.WorldPosition - this.EscapeTarget.FlowTargetHull.WorldPosition));
						}
						else
						{
							this.SteeringManager.SteeringManual(deltaTime, -dir);
						}
					}
					else
					{
						this.SteeringManager.SteeringManual(deltaTime, dir);
					}
					return sqrDist < MathUtils.Pow2(250f);
				}
				if (pathSteering != null)
				{
					pathSteering.SteeringSeek(this.EscapeTarget.SimPosition, 1f, this.minGapSize, null, null, null, true, 0f);
				}
				else
				{
					this.SteeringManager.SteeringSeek(this.EscapeTarget.SimPosition, 10f);
				}
			}
			else
			{
				this.EscapeTarget = null;
				this.allGapsSearched = false;
				this.unreachableGaps.Clear();
			}
			return false;
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00008A8C File Offset: 0x00006C8C
		public void ResetEscape()
		{
			this.EscapeTarget = null;
			this.allGapsSearched = false;
			this.unreachableGaps.Clear();
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00008AA7 File Offset: 0x00006CA7
		protected virtual void OnStateChanged(AIState from, AIState to)
		{
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00008AA9 File Offset: 0x00006CA9
		protected virtual void OnTargetChanged(AITarget previousTarget, AITarget newTarget)
		{
		}

		// Token: 0x040000FC RID: 252
		public bool Enabled;

		// Token: 0x040000FD RID: 253
		public readonly Character Character;

		// Token: 0x040000FE RID: 254
		protected AITarget _lastAiTarget;

		// Token: 0x040000FF RID: 255
		protected AITarget _previousAiTarget;

		// Token: 0x04000100 RID: 256
		protected AITarget _selectedAiTarget;

		// Token: 0x04000101 RID: 257
		protected SteeringManager steeringManager;

		// Token: 0x04000102 RID: 258
		private IEnumerable<Hull> visibleHulls;

		// Token: 0x04000103 RID: 259
		private float hullVisibilityTimer;

		// Token: 0x04000104 RID: 260
		private const float hullVisibilityInterval = 0.5f;

		// Token: 0x04000105 RID: 261
		protected readonly float colliderWidth;

		// Token: 0x04000106 RID: 262
		protected readonly float minGapSize;

		// Token: 0x04000107 RID: 263
		protected readonly float colliderLength;

		// Token: 0x04000108 RID: 264
		protected readonly float avoidLookAheadDistance;

		// Token: 0x0400010B RID: 267
		private readonly HashSet<Item> unequippedItems = new HashSet<Item>();

		// Token: 0x0400010D RID: 269
		private readonly float escapeTargetSeekInterval = 2f;

		// Token: 0x0400010E RID: 270
		private float escapeTimer;

		// Token: 0x0400010F RID: 271
		protected bool allGapsSearched;

		// Token: 0x04000110 RID: 272
		protected readonly HashSet<Gap> unreachableGaps = new HashSet<Gap>();
	}
}
