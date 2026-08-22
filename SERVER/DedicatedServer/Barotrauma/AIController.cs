using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000054 RID: 84
	internal abstract class AIController : ISteerable
	{
		// Token: 0x17000338 RID: 824
		// (get) Token: 0x06000C3A RID: 3130 RVA: 0x0007435F File Offset: 0x0007255F
		// (set) Token: 0x06000C3B RID: 3131 RVA: 0x00074368 File Offset: 0x00072568
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

		// Token: 0x17000339 RID: 825
		// (get) Token: 0x06000C3C RID: 3132 RVA: 0x0007442B File Offset: 0x0007262B
		public SteeringManager SteeringManager
		{
			get
			{
				return this.steeringManager;
			}
		}

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x06000C3D RID: 3133 RVA: 0x00074433 File Offset: 0x00072633
		// (set) Token: 0x06000C3E RID: 3134 RVA: 0x00074445 File Offset: 0x00072645
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

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x06000C3F RID: 3135 RVA: 0x00074458 File Offset: 0x00072658
		public Vector2 SimPosition
		{
			get
			{
				return this.Character.SimPosition;
			}
		}

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x06000C40 RID: 3136 RVA: 0x00074465 File Offset: 0x00072665
		public Vector2 WorldPosition
		{
			get
			{
				return this.Character.WorldPosition;
			}
		}

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x06000C41 RID: 3137 RVA: 0x00074472 File Offset: 0x00072672
		public Vector2 Velocity
		{
			get
			{
				return this.Character.AnimController.Collider.LinearVelocity;
			}
		}

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x06000C42 RID: 3138 RVA: 0x00074489 File Offset: 0x00072689
		public virtual CanEnterSubmarine CanEnterSubmarine
		{
			get
			{
				return this.Character.AnimController.CanEnterSubmarine;
			}
		}

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x06000C43 RID: 3139 RVA: 0x0007449B File Offset: 0x0007269B
		public virtual bool CanFlip
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x06000C44 RID: 3140 RVA: 0x0007449E File Offset: 0x0007269E
		public virtual bool IsMentallyUnstable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x06000C45 RID: 3141 RVA: 0x000744A1 File Offset: 0x000726A1
		// (set) Token: 0x06000C46 RID: 3142 RVA: 0x000744C2 File Offset: 0x000726C2
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

		// Token: 0x06000C47 RID: 3143 RVA: 0x000744CC File Offset: 0x000726CC
		public bool HasValidPath(bool requireNonDirty = true, bool requireUnfinished = true, Func<WayPoint, bool> nodePredicate = null)
		{
			IndoorsSteeringManager pathSteering = this.SteeringManager as IndoorsSteeringManager;
			return pathSteering != null && pathSteering.CurrentPath != null && !pathSteering.CurrentPath.Unreachable && (!requireUnfinished || !pathSteering.CurrentPath.Finished) && (!requireNonDirty || !pathSteering.IsPathDirty) && (nodePredicate == null || pathSteering.CurrentPath.Nodes.All((WayPoint n) => nodePredicate(n)));
		}

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x06000C48 RID: 3144 RVA: 0x00074558 File Offset: 0x00072758
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

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x06000C49 RID: 3145 RVA: 0x0007458C File Offset: 0x0007278C
		public bool IsCurrentPathUnreachable
		{
			get
			{
				IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
				return pathSteering != null && !pathSteering.IsPathDirty && pathSteering.CurrentPath != null && pathSteering.CurrentPath.Unreachable;
			}
		}

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x06000C4A RID: 3146 RVA: 0x000745C8 File Offset: 0x000727C8
		public bool IsCurrentPathFinished
		{
			get
			{
				IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
				return pathSteering != null && !pathSteering.IsPathDirty && pathSteering.CurrentPath != null && pathSteering.CurrentPath.Finished;
			}
		}

		// Token: 0x06000C4B RID: 3147 RVA: 0x00074604 File Offset: 0x00072804
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

		// Token: 0x06000C4C RID: 3148 RVA: 0x000746CE File Offset: 0x000728CE
		public virtual void OnHealed(Character healer, float healAmount)
		{
		}

		// Token: 0x06000C4D RID: 3149 RVA: 0x000746D0 File Offset: 0x000728D0
		public virtual void OnAttacked(Character attacker, AttackResult attackResult)
		{
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x000746D2 File Offset: 0x000728D2
		public virtual void SelectTarget(AITarget target)
		{
		}

		// Token: 0x06000C4F RID: 3151 RVA: 0x000746D4 File Offset: 0x000728D4
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

		// Token: 0x06000C50 RID: 3152 RVA: 0x00074712 File Offset: 0x00072912
		public virtual void Reset()
		{
			this.ResetAITarget();
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x0007471A File Offset: 0x0007291A
		protected void ResetAITarget()
		{
			this._lastAiTarget = null;
			this._selectedAiTarget = null;
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x0007472A File Offset: 0x0007292A
		public void FaceTarget(ISpatialEntity target)
		{
			this.Character.AnimController.TargetDir = ((target.WorldPosition.X > this.Character.WorldPosition.X) ? Direction.Right : Direction.Left);
		}

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x06000C53 RID: 3155 RVA: 0x0007475D File Offset: 0x0007295D
		// (set) Token: 0x06000C54 RID: 3156 RVA: 0x00074765 File Offset: 0x00072965
		public bool IsSteeringThroughGap { get; protected set; }

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x06000C55 RID: 3157 RVA: 0x0007476E File Offset: 0x0007296E
		// (set) Token: 0x06000C56 RID: 3158 RVA: 0x00074776 File Offset: 0x00072976
		public bool IsTryingToSteerThroughGap { get; protected set; }

		// Token: 0x06000C57 RID: 3159 RVA: 0x00074780 File Offset: 0x00072980
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

		// Token: 0x06000C58 RID: 3160 RVA: 0x0007483C File Offset: 0x00072A3C
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

		// Token: 0x06000C59 RID: 3161 RVA: 0x000748B8 File Offset: 0x00072AB8
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

		// Token: 0x06000C5A RID: 3162 RVA: 0x00074914 File Offset: 0x00072B14
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

		// Token: 0x06000C5B RID: 3163 RVA: 0x00074944 File Offset: 0x00072B44
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

		// Token: 0x06000C5C RID: 3164 RVA: 0x00074BD8 File Offset: 0x00072DD8
		public void UnequipEmptyItems(Item parentItem, bool avoidDroppingInSea = true, bool allowDestroying = false)
		{
			AIController.UnequipEmptyItems(this.Character, parentItem, avoidDroppingInSea, allowDestroying);
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x00074BE8 File Offset: 0x00072DE8
		public void UnequipContainedItems(Item parentItem, Func<Item, bool> predicate = null, bool avoidDroppingInSea = true, bool allowDestroying = false, int? unequipMax = null)
		{
			AIController.UnequipContainedItems(this.Character, parentItem, predicate, avoidDroppingInSea, allowDestroying, unequipMax);
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x00074BFC File Offset: 0x00072DFC
		public static void UnequipEmptyItems(Character character, Item parentItem, bool avoidDroppingInSea = true, bool allowDestroying = false)
		{
			AIController.UnequipContainedItems(character, parentItem, (Item it) => it.Condition <= 0f, avoidDroppingInSea, allowDestroying, null);
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x00074C3C File Offset: 0x00072E3C
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

		// Token: 0x06000C60 RID: 3168 RVA: 0x00074D90 File Offset: 0x00072F90
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

		// Token: 0x06000C61 RID: 3169 RVA: 0x00074E2C File Offset: 0x0007302C
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

		// Token: 0x06000C62 RID: 3170 RVA: 0x00074EC8 File Offset: 0x000730C8
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

		// Token: 0x06000C63 RID: 3171
		public abstract bool Escape(float deltaTime);

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x06000C64 RID: 3172 RVA: 0x00074F5C File Offset: 0x0007315C
		// (set) Token: 0x06000C65 RID: 3173 RVA: 0x00074F64 File Offset: 0x00073164
		public Gap EscapeTarget { get; private set; }

		// Token: 0x06000C66 RID: 3174 RVA: 0x00074F70 File Offset: 0x00073170
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

		// Token: 0x06000C67 RID: 3175 RVA: 0x000752EC File Offset: 0x000734EC
		public void ResetEscape()
		{
			this.EscapeTarget = null;
			this.allGapsSearched = false;
			this.unreachableGaps.Clear();
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x00075307 File Offset: 0x00073507
		protected virtual void OnStateChanged(AIState from, AIState to)
		{
		}

		// Token: 0x06000C69 RID: 3177 RVA: 0x00075309 File Offset: 0x00073509
		protected virtual void OnTargetChanged(AITarget previousTarget, AITarget newTarget)
		{
		}

		// Token: 0x04000545 RID: 1349
		public bool Enabled;

		// Token: 0x04000546 RID: 1350
		public readonly Character Character;

		// Token: 0x04000547 RID: 1351
		protected AITarget _lastAiTarget;

		// Token: 0x04000548 RID: 1352
		protected AITarget _previousAiTarget;

		// Token: 0x04000549 RID: 1353
		protected AITarget _selectedAiTarget;

		// Token: 0x0400054A RID: 1354
		protected SteeringManager steeringManager;

		// Token: 0x0400054B RID: 1355
		private IEnumerable<Hull> visibleHulls;

		// Token: 0x0400054C RID: 1356
		private float hullVisibilityTimer;

		// Token: 0x0400054D RID: 1357
		private const float hullVisibilityInterval = 0.5f;

		// Token: 0x0400054E RID: 1358
		protected readonly float colliderWidth;

		// Token: 0x0400054F RID: 1359
		protected readonly float minGapSize;

		// Token: 0x04000550 RID: 1360
		protected readonly float colliderLength;

		// Token: 0x04000551 RID: 1361
		protected readonly float avoidLookAheadDistance;

		// Token: 0x04000554 RID: 1364
		private readonly HashSet<Item> unequippedItems = new HashSet<Item>();

		// Token: 0x04000556 RID: 1366
		private readonly float escapeTargetSeekInterval = 2f;

		// Token: 0x04000557 RID: 1367
		private float escapeTimer;

		// Token: 0x04000558 RID: 1368
		protected bool allGapsSearched;

		// Token: 0x04000559 RID: 1369
		protected readonly HashSet<Gap> unreachableGaps = new HashSet<Gap>();
	}
}
