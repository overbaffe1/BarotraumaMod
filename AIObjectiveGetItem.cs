using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200017C RID: 380
	internal class AIObjectiveGetItem : AIObjective
	{
		// Token: 0x17000B4C RID: 2892
		// (get) Token: 0x06002CB9 RID: 11449 RVA: 0x001E6265 File Offset: 0x001E4465
		// (set) Token: 0x06002CBA RID: 11450 RVA: 0x001E626D File Offset: 0x001E446D
		public override Identifier Identifier { get; set; } = "get item".ToIdentifier();

		// Token: 0x17000B4D RID: 2893
		// (get) Token: 0x06002CBB RID: 11451 RVA: 0x001E6278 File Offset: 0x001E4478
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted((this.IdentifiersOrTags == null || this.IdentifiersOrTags.None(null)) ? "none" : string.Join<Identifier>(", ", this.IdentifiersOrTags));
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17000B4E RID: 2894
		// (get) Token: 0x06002CBC RID: 11452 RVA: 0x001E62EE File Offset: 0x001E44EE
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B4F RID: 2895
		// (get) Token: 0x06002CBD RID: 11453 RVA: 0x001E62F1 File Offset: 0x001E44F1
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B50 RID: 2896
		// (get) Token: 0x06002CBE RID: 11454 RVA: 0x001E62F4 File Offset: 0x001E44F4
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B51 RID: 2897
		// (get) Token: 0x06002CBF RID: 11455 RVA: 0x001E62F7 File Offset: 0x001E44F7
		// (set) Token: 0x06002CC0 RID: 11456 RVA: 0x001E62FF File Offset: 0x001E44FF
		public float TargetCondition { get; set; } = 1f;

		// Token: 0x17000B52 RID: 2898
		// (get) Token: 0x06002CC1 RID: 11457 RVA: 0x001E6308 File Offset: 0x001E4508
		// (set) Token: 0x06002CC2 RID: 11458 RVA: 0x001E6310 File Offset: 0x001E4510
		public bool AllowDangerousPressure { get; set; }

		// Token: 0x17000B53 RID: 2899
		// (get) Token: 0x06002CC3 RID: 11459 RVA: 0x001E6319 File Offset: 0x001E4519
		public Item TargetItem
		{
			get
			{
				return this.targetItem;
			}
		}

		// Token: 0x17000B54 RID: 2900
		// (get) Token: 0x06002CC4 RID: 11460 RVA: 0x001E6321 File Offset: 0x001E4521
		// (set) Token: 0x06002CC5 RID: 11461 RVA: 0x001E6329 File Offset: 0x001E4529
		public bool AllowToFindDivingGear { get; set; } = true;

		// Token: 0x17000B55 RID: 2901
		// (get) Token: 0x06002CC6 RID: 11462 RVA: 0x001E6332 File Offset: 0x001E4532
		// (set) Token: 0x06002CC7 RID: 11463 RVA: 0x001E633A File Offset: 0x001E453A
		public bool MustBeSpecificItem { get; set; }

		// Token: 0x17000B56 RID: 2902
		// (get) Token: 0x06002CC8 RID: 11464 RVA: 0x001E6343 File Offset: 0x001E4543
		// (set) Token: 0x06002CC9 RID: 11465 RVA: 0x001E634B File Offset: 0x001E454B
		public bool AllowStealing { get; set; }

		// Token: 0x17000B57 RID: 2903
		// (get) Token: 0x06002CCA RID: 11466 RVA: 0x001E6354 File Offset: 0x001E4554
		// (set) Token: 0x06002CCB RID: 11467 RVA: 0x001E635C File Offset: 0x001E455C
		public bool TakeWholeStack { get; set; }

		// Token: 0x17000B58 RID: 2904
		// (get) Token: 0x06002CCC RID: 11468 RVA: 0x001E6365 File Offset: 0x001E4565
		// (set) Token: 0x06002CCD RID: 11469 RVA: 0x001E636D File Offset: 0x001E456D
		public bool AllowVariants { get; set; }

		// Token: 0x17000B59 RID: 2905
		// (get) Token: 0x06002CCE RID: 11470 RVA: 0x001E6376 File Offset: 0x001E4576
		// (set) Token: 0x06002CCF RID: 11471 RVA: 0x001E637E File Offset: 0x001E457E
		public bool Equip { get; set; }

		// Token: 0x17000B5A RID: 2906
		// (get) Token: 0x06002CD0 RID: 11472 RVA: 0x001E6387 File Offset: 0x001E4587
		// (set) Token: 0x06002CD1 RID: 11473 RVA: 0x001E638F File Offset: 0x001E458F
		public bool Wear { get; set; }

		// Token: 0x17000B5B RID: 2907
		// (get) Token: 0x06002CD2 RID: 11474 RVA: 0x001E6398 File Offset: 0x001E4598
		// (set) Token: 0x06002CD3 RID: 11475 RVA: 0x001E63A0 File Offset: 0x001E45A0
		public bool RequireNonEmpty { get; set; }

		// Token: 0x17000B5C RID: 2908
		// (get) Token: 0x06002CD4 RID: 11476 RVA: 0x001E63A9 File Offset: 0x001E45A9
		// (set) Token: 0x06002CD5 RID: 11477 RVA: 0x001E63B1 File Offset: 0x001E45B1
		public bool EvaluateCombatPriority { get; set; }

		// Token: 0x17000B5D RID: 2909
		// (get) Token: 0x06002CD6 RID: 11478 RVA: 0x001E63BA File Offset: 0x001E45BA
		// (set) Token: 0x06002CD7 RID: 11479 RVA: 0x001E63C2 File Offset: 0x001E45C2
		public bool CheckPathForEachItem { get; set; }

		// Token: 0x17000B5E RID: 2910
		// (get) Token: 0x06002CD8 RID: 11480 RVA: 0x001E63CB File Offset: 0x001E45CB
		// (set) Token: 0x06002CD9 RID: 11481 RVA: 0x001E63D3 File Offset: 0x001E45D3
		public bool SpeakIfFails { get; set; }

		// Token: 0x17000B5F RID: 2911
		// (get) Token: 0x06002CDA RID: 11482 RVA: 0x001E63DC File Offset: 0x001E45DC
		// (set) Token: 0x06002CDB RID: 11483 RVA: 0x001E63E4 File Offset: 0x001E45E4
		public string CannotFindDialogueIdentifierOverride { get; set; }

		// Token: 0x17000B60 RID: 2912
		// (get) Token: 0x06002CDC RID: 11484 RVA: 0x001E63ED File Offset: 0x001E45ED
		// (set) Token: 0x06002CDD RID: 11485 RVA: 0x001E63F5 File Offset: 0x001E45F5
		public Func<bool> CannotFindDialogueCondition { get; set; }

		// Token: 0x17000B61 RID: 2913
		// (get) Token: 0x06002CDE RID: 11486 RVA: 0x001E63FE File Offset: 0x001E45FE
		// (set) Token: 0x06002CDF RID: 11487 RVA: 0x001E6406 File Offset: 0x001E4606
		public int ItemCount
		{
			get
			{
				return this._itemCount;
			}
			set
			{
				this._itemCount = Math.Max(value, 1);
			}
		}

		// Token: 0x17000B62 RID: 2914
		// (get) Token: 0x06002CE0 RID: 11488 RVA: 0x001E6415 File Offset: 0x001E4615
		// (set) Token: 0x06002CE1 RID: 11489 RVA: 0x001E641D File Offset: 0x001E461D
		public InvSlotType? EquipSlotType { get; set; }

		// Token: 0x06002CE2 RID: 11490 RVA: 0x001E6428 File Offset: 0x001E4628
		public AIObjectiveGetItem(Character character, Item targetItem, AIObjectiveManager objectiveManager, bool equip = true, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.currentSearchIndex = 0;
			this.Equip = equip;
			this.originalTarget = targetItem;
			this.targetItem = targetItem;
			this.moveToTarget = ((targetItem != null) ? targetItem.GetRootInventoryOwner() : null);
		}

		// Token: 0x06002CE3 RID: 11491 RVA: 0x001E64C1 File Offset: 0x001E46C1
		public AIObjectiveGetItem(Character character, Identifier identifierOrTag, AIObjectiveManager objectiveManager, bool equip = true, bool checkInventory = true, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : this(character, new Identifier[]
		{
			identifierOrTag
		}, objectiveManager, equip, checkInventory, priorityModifier, spawnItemIfNotFound)
		{
		}

		// Token: 0x06002CE4 RID: 11492 RVA: 0x001E64E4 File Offset: 0x001E46E4
		public AIObjectiveGetItem(Character character, IEnumerable<Identifier> identifiersOrTags, AIObjectiveManager objectiveManager, bool equip = true, bool checkInventory = true, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.currentSearchIndex = 0;
			this.Equip = equip;
			this.spawnItemIfNotFound = spawnItemIfNotFound;
			this.checkInventory = checkInventory;
			this.IdentifiersOrTags = AIObjectiveGetItem.ParseGearTags(identifiersOrTags).ToImmutableHashSet<Identifier>();
			this.ignoredIdentifiersOrTags = AIObjectiveGetItem.ParseIgnoredTags(identifiersOrTags).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06002CE5 RID: 11493 RVA: 0x001E6590 File Offset: 0x001E4790
		public static IEnumerable<Identifier> ParseGearTags(IEnumerable<Identifier> identifiersOrTags)
		{
			List<Identifier> tags = new List<Identifier>();
			foreach (Identifier tag in identifiersOrTags)
			{
				if (!tag.Contains("!"))
				{
					tags.Add(tag);
				}
			}
			return tags;
		}

		// Token: 0x06002CE6 RID: 11494 RVA: 0x001E65F0 File Offset: 0x001E47F0
		public static IEnumerable<Identifier> ParseIgnoredTags(IEnumerable<Identifier> identifiersOrTags)
		{
			List<Identifier> ignoredTags = new List<Identifier>();
			foreach (Identifier tag in identifiersOrTags)
			{
				if (tag.Contains("!"))
				{
					ignoredTags.Add(tag.Remove("!"));
				}
			}
			return ignoredTags;
		}

		// Token: 0x06002CE7 RID: 11495 RVA: 0x001E6658 File Offset: 0x001E4858
		public static Func<PathNode, bool> CreateEndNodeFilter(ISpatialEntity targetEntity)
		{
			return (PathNode n) => (n.Waypoint.Ladders == null || n.Waypoint.IsInWater) && Vector2.DistanceSquared(n.Waypoint.WorldPosition, targetEntity.WorldPosition) <= MathUtils.Pow2(150f);
		}

		// Token: 0x06002CE8 RID: 11496 RVA: 0x001E6680 File Offset: 0x001E4880
		private bool CheckInventory()
		{
			if (this.IdentifiersOrTags == null)
			{
				return false;
			}
			Item item = this.character.Inventory.FindItem((Item i) => this.CheckItem(i), true);
			if (item != null)
			{
				this.targetItem = item;
				this.moveToTarget = item.GetRootInventoryOwner();
			}
			return item != null;
		}

		// Token: 0x06002CE9 RID: 11497 RVA: 0x001E66D0 File Offset: 0x001E48D0
		private bool CountItems()
		{
			int itemCount = 0;
			foreach (Item it in this.character.Inventory.AllItems)
			{
				if (this.CheckItem(it))
				{
					itemCount++;
				}
			}
			return itemCount >= this.ItemCount;
		}

		// Token: 0x06002CEA RID: 11498 RVA: 0x001E673C File Offset: 0x001E493C
		protected override void Act(float deltaTime)
		{
			if (this.IdentifiersOrTags != null)
			{
				if (this.checkInventory && this.CheckInventory())
				{
					this.isDoneSeeking = true;
					this.itemCandidates.Clear();
				}
				if (!this.isDoneSeeking)
				{
					if (this.character.Submarine == null)
					{
						base.Abandon = true;
						return;
					}
					if (!this.AllowDangerousPressure)
					{
						bool dangerousPressure = !this.character.IsProtectedFromPressure && (this.character.CurrentHull == null || this.character.CurrentHull.LethalPressure > 0f);
						if (dangerousPressure)
						{
							base.Abandon = true;
							return;
						}
					}
					this.FindTargetItem();
				}
				if (this.targetItem == null)
				{
					if (this.isDoneSeeking)
					{
						this.HandlePotentialItems(deltaTime);
					}
					if (!(this.objectiveManager.CurrentOrder is AIObjectiveGoTo))
					{
						this.objectiveManager.GetObjective<AIObjectiveIdle>().Wander(deltaTime);
					}
					return;
				}
			}
			else if (this.character.Submarine == null)
			{
				base.Abandon = true;
				return;
			}
			Item item3 = this.targetItem;
			bool flag;
			if (item3 != null)
			{
				bool removed = item3.Removed;
				if (!removed)
				{
					flag = false;
					goto IL_109;
				}
			}
			flag = true;
			IL_109:
			if (flag)
			{
				if (this.<Act>g__ShouldAbort|111_0())
				{
					base.Abandon = true;
				}
				return;
			}
			if (this.moveToTarget == null)
			{
				if (this.<Act>g__ShouldAbort|111_0())
				{
					base.Abandon = true;
					return;
				}
				return;
			}
			else
			{
				if (!this.character.IsItemTakenBySomeoneElse(this.targetItem))
				{
					bool canInteract = false;
					Character c = this.moveToTarget as Character;
					if (c != null)
					{
						if (this.character == c)
						{
							canInteract = true;
							this.moveToTarget = null;
						}
						else
						{
							this.character.SelectCharacter(c);
							canInteract = this.character.CanInteractWith(c, 200f, true, false);
							this.character.DeselectCharacter();
						}
					}
					else
					{
						Item parentItem = this.moveToTarget as Item;
						if (parentItem != null)
						{
							canInteract = this.character.CanInteractWith(parentItem, false);
						}
					}
					if (canInteract)
					{
						if (this.targetItem.GetComponent<Pickable>() == null)
						{
							base.Abandon = true;
							return;
						}
						Inventory itemInventory = this.targetItem.ParentInventory;
						List<int> slots = (itemInventory != null) ? itemInventory.FindIndices(this.targetItem) : null;
						List<Item> droppedStack = this.TargetItem.DroppedStack.ToList<Item>();
						if (base.HumanAIController.TakeItem(this.targetItem, this.character.Inventory, this.Equip, this.Wear, true, false, true, this.IdentifiersOrTags))
						{
							if (this.TakeWholeStack)
							{
								int maxStackSize = 0;
								int takenItemCount = 1;
								for (int i = 0; i < this.character.Inventory.Capacity; i++)
								{
									maxStackSize = Math.Max(maxStackSize, this.character.Inventory.HowManyCanBePut(this.targetItem.Prefab, i, null, false));
								}
								if (slots != null)
								{
									foreach (int slot in slots)
									{
										foreach (Item item in itemInventory.GetItemsAt(slot).ToList<Item>())
										{
											if (!base.HumanAIController.TakeItem(item, this.character.Inventory, false, false, true, false, true, null))
											{
												break;
											}
											takenItemCount++;
											if (takenItemCount >= maxStackSize)
											{
												break;
											}
										}
									}
								}
								foreach (Item item2 in droppedStack)
								{
									if (item2 != this.TargetItem)
									{
										if (!base.HumanAIController.TakeItem(item2, this.character.Inventory, false, false, true, false, true, null))
										{
											break;
										}
										takenItemCount++;
										if (takenItemCount >= maxStackSize)
										{
											break;
										}
									}
								}
							}
							if (this.IdentifiersOrTags == null)
							{
								base.IsCompleted = true;
								return;
							}
							base.IsCompleted = this.CountItems();
							if (!base.IsCompleted)
							{
								this.ResetInternal();
								return;
							}
						}
						else
						{
							if (!this.Equip)
							{
								this.Equip = true;
								if (!this.objectiveManager.HasActiveObjective<AIObjectiveCleanupItem>() && !this.objectiveManager.HasActiveObjective<AIObjectiveLoadItem>())
								{
									this.Wear = true;
								}
								return;
							}
							base.Abandon = true;
							return;
						}
					}
					else if (this.moveToTarget != null)
					{
						base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(this.moveToTarget, this.character, this.objectiveManager, false, this.AllowToFindDivingGear, 1f, 100f)
						{
							IsFindDivingGearSubObjective = this.IsFindDivingGearSubObjective,
							AbortCondition = delegate(AIObjective obj)
							{
								if (this.targetItem != null)
								{
									Entity owner = this.targetItem.GetRootInventoryOwner();
									return owner != null && owner != this.moveToTarget && owner != this.character;
								}
								return true;
							},
							SpeakIfFails = false,
							ForceWalkTemporarily = base.ForceWalkTemporarily,
							ForceWalkPermanently = base.ForceWalkPermanently,
							endNodeFilter = AIObjectiveGetItem.CreateEndNodeFilter(this.moveToTarget)
						}, delegate
						{
							base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
						}, delegate
						{
							if (this.originalTarget == null)
							{
								this.ignoredItems.Add(this.targetItem);
								if (this.targetItem != this.moveToTarget)
								{
									Item item4 = this.moveToTarget as Item;
									if (item4 != null)
									{
										this.ignoredItems.Add(item4);
									}
								}
								this.ResetInternal();
								return;
							}
							base.Abandon = true;
						});
					}
					return;
				}
				if (this.originalTarget == null)
				{
					this.ignoredItems.Add(this.targetItem);
					this.ResetInternal();
					return;
				}
				base.Abandon = true;
				return;
			}
		}

		// Token: 0x17000B63 RID: 2915
		// (get) Token: 0x06002CEB RID: 11499 RVA: 0x001E6BE0 File Offset: 0x001E4DE0
		private Stopwatch StopWatch
		{
			get
			{
				Stopwatch result;
				if ((result = this.sw) == null)
				{
					result = (this.sw = new Stopwatch());
				}
				return result;
			}
		}

		// Token: 0x06002CEC RID: 11500 RVA: 0x001E6C08 File Offset: 0x001E4E08
		private void FindTargetItem()
		{
			if (this.IdentifiersOrTags == null)
			{
				if (this.targetItem == null)
				{
					base.Abandon = true;
				}
				return;
			}
			if (HumanAIController.DebugAI)
			{
				this.StopWatch.Restart();
			}
			float priority = this.objectiveManager.GetCurrentPriority();
			bool checkPath = this.CheckPathForEachItem || priority >= 50f || this.ItemCount > 1;
			if (this.itemList != null && !this.character.Submarine.IsEntityFoundOnThisSub(this.itemList.FirstOrDefault<Item>(), true, false, false))
			{
				this.currentSearchIndex = 0;
			}
			if (this.currentSearchIndex == 0)
			{
				this.itemCandidates.Clear();
				this.itemList = this.character.Submarine.GetItems(true);
			}
			int itemsPerFrame = (int)MathHelper.Lerp(30f, 300f, MathUtils.InverseLerp(10f, 100f, priority));
			int checkedItems = 0;
			int i = 0;
			while (i < itemsPerFrame && this.currentSearchIndex < this.itemList.Count)
			{
				checkedItems++;
				Item item = this.itemList[this.currentSearchIndex];
				Submarine submarine;
				if ((submarine = item.Submarine) == null)
				{
					Inventory parentInventory = item.ParentInventory;
					if (parentInventory == null)
					{
						submarine = null;
					}
					else
					{
						Entity owner = parentInventory.Owner;
						submarine = ((owner != null) ? owner.Submarine : null);
					}
				}
				Submarine itemSub = submarine;
				if (itemSub != null)
				{
					Submarine mySub = this.character.Submarine;
					if (mySub != null && (this.checkInventory || !item.IsOwnedBy(this.character)) && (this.AllowStealing || !this.character.IsOnPlayerTeam || !item.Illegitimate) && this.CheckItem(item) && (item.Container == null || (!item.Container.HasTag(Tags.DontTakeItems) && !this.ignoredItems.Contains(item.Container) && (this.ignoredContainerIdentifiers == null || !this.ignoredContainerIdentifiers.Contains(item.ContainerIdentifier)))) && !this.character.IsItemTakenBySomeoneElse(item))
					{
						ItemInventory itemInventory = item.ParentInventory as ItemInventory;
						if (itemInventory == null || itemInventory.Container.HasRequiredItems(this.character, false, null))
						{
							float itemPriority = item.Prefab.BotPriority;
							if (this.GetItemPriority != null)
							{
								itemPriority *= this.GetItemPriority(item);
							}
							if (itemPriority > 0f)
							{
								Entity rootInventoryOwner = item.GetRootInventoryOwner();
								Item ownerItem = rootInventoryOwner as Item;
								if (ownerItem != null)
								{
									if (!ownerItem.IsInteractable(this.character))
									{
										goto IL_475;
									}
									if (ownerItem != item)
									{
										ItemContainer component = ownerItem.GetComponent<ItemContainer>();
										if (component != null && !component.HasRequiredItems(this.character, false, null))
										{
											goto IL_475;
										}
										if (ownerItem != item.Container)
										{
											if (this.ContainTarget != null && this.ContainTarget.Item.Prefab.Identifier == item.Container.Prefab.Identifier)
											{
												itemPriority = 0.95f;
											}
											else
											{
												itemPriority *= 0.1f;
											}
										}
									}
								}
								Vector2 itemPos = (rootInventoryOwner ?? item).WorldPosition;
								float distanceFactor = base.GetDistanceFactor(itemPos, this.EvaluateCombatPriority ? 0.1f : 0f, 5f, 10000f, 1f);
								itemPriority *= distanceFactor;
								if (this.EvaluateCombatPriority)
								{
									MeleeWeapon mw = item.GetComponent<MeleeWeapon>();
									RangedWeapon rw = item.GetComponent<RangedWeapon>();
									float combatFactor;
									if (mw != null)
									{
										if (mw.CombatPriority > 0f)
										{
											combatFactor = mw.CombatPriority / 100f;
										}
										else
										{
											combatFactor = Math.Min(AIObjectiveCombat.GetLethalDamage(mw) / 1000f, 0.1f);
										}
									}
									else if (rw != null)
									{
										if (rw.CombatPriority > 0f)
										{
											combatFactor = rw.CombatPriority / 100f;
										}
										else
										{
											combatFactor = Math.Min(AIObjectiveCombat.GetLethalDamage(rw) / 1000f, 0.1f);
										}
									}
									else
									{
										IEnumerable<ItemComponent> components = item.Components;
										Func<ItemComponent, float> selector;
										if ((selector = AIObjectiveGetItem.<>O.<0>__GetLethalDamage) == null)
										{
											selector = (AIObjectiveGetItem.<>O.<0>__GetLethalDamage = new Func<ItemComponent, float>(AIObjectiveCombat.GetLethalDamage));
										}
										combatFactor = Math.Min(components.Sum(selector) / 1000f, 0.1f);
									}
									itemPriority *= combatFactor;
								}
								else
								{
									itemPriority *= item.Condition / item.MaxCondition;
								}
								if (itemPriority >= this.currItemPriority && (!this.EvaluateCombatPriority || itemPriority > 0f))
								{
									if (checkPath)
									{
										this.itemCandidates.Add(new ValueTuple<Item, float>(item, itemPriority));
									}
									else
									{
										this.currItemPriority = itemPriority;
										this.targetItem = item;
										this.moveToTarget = (rootInventoryOwner ?? item);
									}
								}
							}
						}
					}
				}
				IL_475:
				i++;
				this.currentSearchIndex++;
			}
			if (this.currentSearchIndex >= this.itemList.Count - 1)
			{
				this.isDoneSeeking = true;
				if (this.itemCandidates.Any<ValueTuple<Item, float>>())
				{
					this.itemCandidates.Sort(([TupleElementNames(new string[]
					{
						"item",
						"priority"
					})] ValueTuple<Item, float> x, [TupleElementNames(new string[]
					{
						"item",
						"priority"
					})] ValueTuple<Item, float> y) => y.Item2.CompareTo(x.Item2));
				}
				if (HumanAIController.DebugAI && this.StopWatch.ElapsedMilliseconds > 2L)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 5);
					defaultInterpolatedStringHandler.AppendLiteral("Went through ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(checkedItems);
					defaultInterpolatedStringHandler.AppendLiteral(" of total ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.itemList.Count);
					defaultInterpolatedStringHandler.AppendLiteral(" items. Found item ");
					Item item2 = this.targetItem;
					defaultInterpolatedStringHandler.AppendFormatted(((item2 != null) ? item2.Name : null) ?? "NULL");
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted<long>(this.StopWatch.ElapsedMilliseconds);
					defaultInterpolatedStringHandler.AppendLiteral(" ms. Completed: ");
					defaultInterpolatedStringHandler.AppendFormatted<bool>(this.isDoneSeeking);
					string msg = defaultInterpolatedStringHandler.ToStringAndClear();
					if (this.StopWatch.ElapsedMilliseconds > 5L)
					{
						DebugConsole.ThrowError(msg, null, null, false, false);
						return;
					}
					DebugConsole.AddWarning(msg, null);
				}
			}
		}

		// Token: 0x06002CED RID: 11501 RVA: 0x001E71F8 File Offset: 0x001E53F8
		private void HandlePotentialItems(float deltaTime)
		{
			if (this.itemCandidates.Any<ValueTuple<Item, float>>())
			{
				if (base.PathSteering == null)
				{
					this.itemCandidates.Clear();
					base.Abandon = true;
					return;
				}
				ValueTuple<Item, float> itemCandidate = this.itemCandidates.FirstOrDefault<ValueTuple<Item, float>>();
				SteeringPath path = base.PathSteering.PathFinder.FindPath(this.character.SimPosition, this.character.GetRelativeSimPosition(itemCandidate.Item1, null), this.character.Submarine, "AIObjectiveGetItem " + this.character.DisplayName, 0f, null, null, (PathNode node) => node.Waypoint.CurrentHull != null, true, 0f);
				if (path.Unreachable)
				{
					this.itemCandidates.Remove(itemCandidate);
				}
				else
				{
					this.itemCandidates.Clear();
					this.targetItem = itemCandidate.Item1;
					this.moveToTarget = (this.targetItem.GetRootInventoryOwner() ?? this.targetItem);
				}
			}
			if (this.targetItem == null)
			{
				if (this.spawnItemIfNotFound)
				{
					ItemPrefab prefab = this.FindItemToSpawn();
					if (prefab == null)
					{
						base.Abandon = true;
						return;
					}
					Entity.Spawner.AddItemToSpawnQueue(prefab, this.character.Inventory, null, null, delegate(Item spawnedItem)
					{
						this.targetItem = spawnedItem;
						if (this.character.TeamID == CharacterTeamType.FriendlyNPC)
						{
							Submarine submarine = this.character.Submarine;
							if (submarine != null && submarine.Info.IsOutpost)
							{
								spawnedItem.SpawnedInCurrentOutpost = true;
							}
						}
					}, true, false, InvSlotType.None);
					return;
				}
				else
				{
					this.abandonDelayIfItemNotFound -= deltaTime;
					if (this.abandonDelayIfItemNotFound <= 0f)
					{
						base.Abandon = true;
					}
				}
			}
		}

		// Token: 0x06002CEE RID: 11502 RVA: 0x001E7388 File Offset: 0x001E5588
		private ItemPrefab FindItemToSpawn()
		{
			ItemPrefab bestItem = null;
			float lowestCost = float.MaxValue;
			using (IEnumerator<MapEntityPrefab> enumerator = MapEntityPrefab.List.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MapEntityPrefab prefab = enumerator.Current;
					ItemPrefab itemPrefab = prefab as ItemPrefab;
					if (itemPrefab != null && this.IdentifiersOrTags.Any((Identifier id) => id == prefab.Identifier || prefab.Tags.Contains(id)))
					{
						if (this.character.AIController.HasInfiniteItemSpawns(prefab.Identifier))
						{
							return itemPrefab;
						}
						float cost = (itemPrefab.DefaultPrice != null && itemPrefab.CanBeBought) ? ((float)itemPrefab.DefaultPrice.Price) : float.MaxValue;
						if (cost < lowestCost || bestItem == null)
						{
							bestItem = itemPrefab;
							lowestCost = cost;
						}
					}
				}
			}
			return bestItem;
		}

		// Token: 0x06002CEF RID: 11503 RVA: 0x001E7470 File Offset: 0x001E5670
		protected override bool CheckObjectiveState()
		{
			if (this.targetItem == null)
			{
				return false;
			}
			if (this.IdentifiersOrTags != null && this.ItemCount > 1)
			{
				return this.CountItems();
			}
			if (this.Equip && this.EquipSlotType != null)
			{
				return this.character.HasEquippedItem(this.targetItem, new InvSlotType?(this.EquipSlotType.Value), null);
			}
			return this.character.HasItem(this.targetItem, this.Equip, null);
		}

		// Token: 0x06002CF0 RID: 11504 RVA: 0x001E7500 File Offset: 0x001E5700
		private bool CheckItem(Item item)
		{
			return (item.HasIdentifierOrTags(this.IdentifiersOrTags) || (this.AllowVariants && !item.Prefab.VariantOf.IsEmpty && this.IdentifiersOrTags.Contains(item.Prefab.VariantOf))) && item.HasAccess(this.character) && !this.ignoredItems.Contains(item) && (this.ignoredIdentifiersOrTags == null || !item.HasIdentifierOrTags(this.ignoredIdentifiersOrTags)) && item.Condition >= this.TargetCondition && (this.ItemFilter == null || this.ItemFilter(item)) && (!this.RequireNonEmpty || !item.Components.Any((ItemComponent i) => i.IsEmpty(this.character)));
		}

		// Token: 0x06002CF1 RID: 11505 RVA: 0x001E75DE File Offset: 0x001E57DE
		public override void Reset()
		{
			base.Reset();
			this.ResetInternal();
		}

		// Token: 0x06002CF2 RID: 11506 RVA: 0x001E75EC File Offset: 0x001E57EC
		private void ResetInternal()
		{
			base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
			this.targetItem = this.originalTarget;
			Item item = this.targetItem;
			this.moveToTarget = ((item != null) ? item.GetRootInventoryOwner() : null);
			this.isDoneSeeking = false;
			this.currentSearchIndex = 0;
			this.currItemPriority = 0f;
		}

		// Token: 0x06002CF3 RID: 11507 RVA: 0x001E7642 File Offset: 0x001E5842
		protected override void OnAbandon()
		{
			base.OnAbandon();
			ISpatialEntity spatialEntity = this.moveToTarget;
			this.SpeakCannotFind();
		}

		// Token: 0x06002CF4 RID: 11508 RVA: 0x001E7658 File Offset: 0x001E5858
		private void SpeakCannotFind()
		{
			if (!this.SpeakIfFails)
			{
				return;
			}
			if (!this.character.IsOnPlayerTeam)
			{
				return;
			}
			if (this.objectiveManager.CurrentOrder != this.objectiveManager.CurrentObjective)
			{
				return;
			}
			if (this.CannotFindDialogueCondition != null && !this.CannotFindDialogueCondition())
			{
				return;
			}
			LocalizedString msg = TextManager.Get(new string[]
			{
				this.CannotFindDialogueIdentifierOverride,
				"dialogcannotfinditem"
			});
			if (msg.IsNullOrEmpty() || !msg.Loaded)
			{
				return;
			}
			Character character = this.character;
			string value = msg.Value;
			Identifier identifier = "dialogcannotfinditem".ToIdentifier();
			character.Speak(value, null, 0f, identifier, 20f);
		}

		// Token: 0x06002CF7 RID: 11511 RVA: 0x001E774F File Offset: 0x001E594F
		[CompilerGenerated]
		private bool <Act>g__ShouldAbort|111_0()
		{
			return this.IdentifiersOrTags == null || (this.isDoneSeeking && this.itemCandidates.None(null));
		}

		// Token: 0x0400174F RID: 5967
		public HashSet<Item> ignoredItems = new HashSet<Item>();

		// Token: 0x04001750 RID: 5968
		public Func<Item, float> GetItemPriority;

		// Token: 0x04001751 RID: 5969
		public Func<Item, bool> ItemFilter;

		// Token: 0x04001754 RID: 5972
		public readonly ImmutableHashSet<Identifier> IdentifiersOrTags;

		// Token: 0x04001755 RID: 5973
		private readonly bool spawnItemIfNotFound;

		// Token: 0x04001756 RID: 5974
		private Item targetItem;

		// Token: 0x04001757 RID: 5975
		private readonly Item originalTarget;

		// Token: 0x04001758 RID: 5976
		public ItemContainer ContainTarget;

		// Token: 0x04001759 RID: 5977
		private ISpatialEntity moveToTarget;

		// Token: 0x0400175A RID: 5978
		private bool isDoneSeeking;

		// Token: 0x0400175B RID: 5979
		private int currentSearchIndex;

		// Token: 0x0400175C RID: 5980
		public ImmutableHashSet<Identifier> ignoredContainerIdentifiers;

		// Token: 0x0400175D RID: 5981
		public ImmutableHashSet<Identifier> ignoredIdentifiersOrTags;

		// Token: 0x0400175E RID: 5982
		private AIObjectiveGoTo goToObjective;

		// Token: 0x0400175F RID: 5983
		private float currItemPriority;

		// Token: 0x04001760 RID: 5984
		private readonly bool checkInventory;

		// Token: 0x04001761 RID: 5985
		public const float DefaultReach = 100f;

		// Token: 0x04001762 RID: 5986
		public const float MaxReach = 150f;

		// Token: 0x04001763 RID: 5987
		private float abandonDelayIfItemNotFound = 5f;

		// Token: 0x04001764 RID: 5988
		public bool IsFindDivingGearSubObjective;

		// Token: 0x04001772 RID: 6002
		private int _itemCount = 1;

		// Token: 0x04001774 RID: 6004
		public static readonly Identifier[] AllowedItemsToTake = new Identifier[]
		{
			Tags.OxygenSource,
			Tags.FireExtinguisher,
			Tags.LightDivingGear,
			Tags.HeavyDivingGear
		};

		// Token: 0x04001775 RID: 6005
		private Stopwatch sw;

		// Token: 0x04001776 RID: 6006
		[TupleElementNames(new string[]
		{
			"item",
			"priority"
		})]
		private readonly List<ValueTuple<Item, float>> itemCandidates = new List<ValueTuple<Item, float>>();

		// Token: 0x04001777 RID: 6007
		private List<Item> itemList;

		// Token: 0x02000E20 RID: 3616
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005181 RID: 20865
			public static Func<ItemComponent, float> <0>__GetLethalDamage;
		}
	}
}
