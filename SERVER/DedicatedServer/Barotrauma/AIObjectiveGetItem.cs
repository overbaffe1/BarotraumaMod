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
	// Token: 0x02000076 RID: 118
	internal class AIObjectiveGetItem : AIObjective
	{
		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x06000FB1 RID: 4017 RVA: 0x000933D1 File Offset: 0x000915D1
		// (set) Token: 0x06000FB2 RID: 4018 RVA: 0x000933D9 File Offset: 0x000915D9
		public override Identifier Identifier { get; set; } = "get item".ToIdentifier();

		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x06000FB3 RID: 4019 RVA: 0x000933E4 File Offset: 0x000915E4
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

		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x06000FB4 RID: 4020 RVA: 0x0009345A File Offset: 0x0009165A
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06000FB5 RID: 4021 RVA: 0x0009345D File Offset: 0x0009165D
		public override bool AllowMultipleInstances
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06000FB6 RID: 4022 RVA: 0x00093460 File Offset: 0x00091660
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x06000FB7 RID: 4023 RVA: 0x00093463 File Offset: 0x00091663
		// (set) Token: 0x06000FB8 RID: 4024 RVA: 0x0009346B File Offset: 0x0009166B
		public float TargetCondition { get; set; } = 1f;

		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x06000FB9 RID: 4025 RVA: 0x00093474 File Offset: 0x00091674
		// (set) Token: 0x06000FBA RID: 4026 RVA: 0x0009347C File Offset: 0x0009167C
		public bool AllowDangerousPressure { get; set; }

		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x06000FBB RID: 4027 RVA: 0x00093485 File Offset: 0x00091685
		public Item TargetItem
		{
			get
			{
				return this.targetItem;
			}
		}

		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x06000FBC RID: 4028 RVA: 0x0009348D File Offset: 0x0009168D
		// (set) Token: 0x06000FBD RID: 4029 RVA: 0x00093495 File Offset: 0x00091695
		public bool AllowToFindDivingGear { get; set; } = true;

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x06000FBE RID: 4030 RVA: 0x0009349E File Offset: 0x0009169E
		// (set) Token: 0x06000FBF RID: 4031 RVA: 0x000934A6 File Offset: 0x000916A6
		public bool MustBeSpecificItem { get; set; }

		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x06000FC0 RID: 4032 RVA: 0x000934AF File Offset: 0x000916AF
		// (set) Token: 0x06000FC1 RID: 4033 RVA: 0x000934B7 File Offset: 0x000916B7
		public bool AllowStealing { get; set; }

		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x06000FC2 RID: 4034 RVA: 0x000934C0 File Offset: 0x000916C0
		// (set) Token: 0x06000FC3 RID: 4035 RVA: 0x000934C8 File Offset: 0x000916C8
		public bool TakeWholeStack { get; set; }

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06000FC4 RID: 4036 RVA: 0x000934D1 File Offset: 0x000916D1
		// (set) Token: 0x06000FC5 RID: 4037 RVA: 0x000934D9 File Offset: 0x000916D9
		public bool AllowVariants { get; set; }

		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x06000FC6 RID: 4038 RVA: 0x000934E2 File Offset: 0x000916E2
		// (set) Token: 0x06000FC7 RID: 4039 RVA: 0x000934EA File Offset: 0x000916EA
		public bool Equip { get; set; }

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06000FC8 RID: 4040 RVA: 0x000934F3 File Offset: 0x000916F3
		// (set) Token: 0x06000FC9 RID: 4041 RVA: 0x000934FB File Offset: 0x000916FB
		public bool Wear { get; set; }

		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06000FCA RID: 4042 RVA: 0x00093504 File Offset: 0x00091704
		// (set) Token: 0x06000FCB RID: 4043 RVA: 0x0009350C File Offset: 0x0009170C
		public bool RequireNonEmpty { get; set; }

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x06000FCC RID: 4044 RVA: 0x00093515 File Offset: 0x00091715
		// (set) Token: 0x06000FCD RID: 4045 RVA: 0x0009351D File Offset: 0x0009171D
		public bool EvaluateCombatPriority { get; set; }

		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x06000FCE RID: 4046 RVA: 0x00093526 File Offset: 0x00091726
		// (set) Token: 0x06000FCF RID: 4047 RVA: 0x0009352E File Offset: 0x0009172E
		public bool CheckPathForEachItem { get; set; }

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x06000FD0 RID: 4048 RVA: 0x00093537 File Offset: 0x00091737
		// (set) Token: 0x06000FD1 RID: 4049 RVA: 0x0009353F File Offset: 0x0009173F
		public bool SpeakIfFails { get; set; }

		// Token: 0x17000445 RID: 1093
		// (get) Token: 0x06000FD2 RID: 4050 RVA: 0x00093548 File Offset: 0x00091748
		// (set) Token: 0x06000FD3 RID: 4051 RVA: 0x00093550 File Offset: 0x00091750
		public string CannotFindDialogueIdentifierOverride { get; set; }

		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x06000FD4 RID: 4052 RVA: 0x00093559 File Offset: 0x00091759
		// (set) Token: 0x06000FD5 RID: 4053 RVA: 0x00093561 File Offset: 0x00091761
		public Func<bool> CannotFindDialogueCondition { get; set; }

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x06000FD6 RID: 4054 RVA: 0x0009356A File Offset: 0x0009176A
		// (set) Token: 0x06000FD7 RID: 4055 RVA: 0x00093572 File Offset: 0x00091772
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

		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x06000FD8 RID: 4056 RVA: 0x00093581 File Offset: 0x00091781
		// (set) Token: 0x06000FD9 RID: 4057 RVA: 0x00093589 File Offset: 0x00091789
		public InvSlotType? EquipSlotType { get; set; }

		// Token: 0x06000FDA RID: 4058 RVA: 0x00093594 File Offset: 0x00091794
		public AIObjectiveGetItem(Character character, Item targetItem, AIObjectiveManager objectiveManager, bool equip = true, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.currentSearchIndex = 0;
			this.Equip = equip;
			this.originalTarget = targetItem;
			this.targetItem = targetItem;
			this.moveToTarget = ((targetItem != null) ? targetItem.GetRootInventoryOwner() : null);
		}

		// Token: 0x06000FDB RID: 4059 RVA: 0x0009362D File Offset: 0x0009182D
		public AIObjectiveGetItem(Character character, Identifier identifierOrTag, AIObjectiveManager objectiveManager, bool equip = true, bool checkInventory = true, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : this(character, new Identifier[]
		{
			identifierOrTag
		}, objectiveManager, equip, checkInventory, priorityModifier, spawnItemIfNotFound)
		{
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x00093650 File Offset: 0x00091850
		public AIObjectiveGetItem(Character character, IEnumerable<Identifier> identifiersOrTags, AIObjectiveManager objectiveManager, bool equip = true, bool checkInventory = true, float priorityModifier = 1f, bool spawnItemIfNotFound = false) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.currentSearchIndex = 0;
			this.Equip = equip;
			this.spawnItemIfNotFound = spawnItemIfNotFound;
			this.checkInventory = checkInventory;
			this.IdentifiersOrTags = AIObjectiveGetItem.ParseGearTags(identifiersOrTags).ToImmutableHashSet<Identifier>();
			this.ignoredIdentifiersOrTags = AIObjectiveGetItem.ParseIgnoredTags(identifiersOrTags).ToImmutableHashSet<Identifier>();
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x000936FC File Offset: 0x000918FC
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

		// Token: 0x06000FDE RID: 4062 RVA: 0x0009375C File Offset: 0x0009195C
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

		// Token: 0x06000FDF RID: 4063 RVA: 0x000937C4 File Offset: 0x000919C4
		public static Func<PathNode, bool> CreateEndNodeFilter(ISpatialEntity targetEntity)
		{
			return (PathNode n) => (n.Waypoint.Ladders == null || n.Waypoint.IsInWater) && Vector2.DistanceSquared(n.Waypoint.WorldPosition, targetEntity.WorldPosition) <= MathUtils.Pow2(150f);
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x000937EC File Offset: 0x000919EC
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

		// Token: 0x06000FE1 RID: 4065 RVA: 0x0009383C File Offset: 0x00091A3C
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

		// Token: 0x06000FE2 RID: 4066 RVA: 0x000938A8 File Offset: 0x00091AA8
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

		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x06000FE3 RID: 4067 RVA: 0x00093D4C File Offset: 0x00091F4C
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

		// Token: 0x06000FE4 RID: 4068 RVA: 0x00093D74 File Offset: 0x00091F74
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

		// Token: 0x06000FE5 RID: 4069 RVA: 0x00094364 File Offset: 0x00092564
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

		// Token: 0x06000FE6 RID: 4070 RVA: 0x000944F4 File Offset: 0x000926F4
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

		// Token: 0x06000FE7 RID: 4071 RVA: 0x000945DC File Offset: 0x000927DC
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

		// Token: 0x06000FE8 RID: 4072 RVA: 0x0009466C File Offset: 0x0009286C
		private bool CheckItem(Item item)
		{
			return (item.HasIdentifierOrTags(this.IdentifiersOrTags) || (this.AllowVariants && !item.Prefab.VariantOf.IsEmpty && this.IdentifiersOrTags.Contains(item.Prefab.VariantOf))) && item.HasAccess(this.character) && !this.ignoredItems.Contains(item) && (this.ignoredIdentifiersOrTags == null || !item.HasIdentifierOrTags(this.ignoredIdentifiersOrTags)) && item.Condition >= this.TargetCondition && (this.ItemFilter == null || this.ItemFilter(item)) && (!this.RequireNonEmpty || !item.Components.Any((ItemComponent i) => i.IsEmpty(this.character)));
		}

		// Token: 0x06000FE9 RID: 4073 RVA: 0x0009474A File Offset: 0x0009294A
		public override void Reset()
		{
			base.Reset();
			this.ResetInternal();
		}

		// Token: 0x06000FEA RID: 4074 RVA: 0x00094758 File Offset: 0x00092958
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

		// Token: 0x06000FEB RID: 4075 RVA: 0x000947AE File Offset: 0x000929AE
		protected override void OnAbandon()
		{
			base.OnAbandon();
			ISpatialEntity spatialEntity = this.moveToTarget;
			this.SpeakCannotFind();
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x000947C4 File Offset: 0x000929C4
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

		// Token: 0x06000FEF RID: 4079 RVA: 0x000948BB File Offset: 0x00092ABB
		[CompilerGenerated]
		private bool <Act>g__ShouldAbort|111_0()
		{
			return this.IdentifiersOrTags == null || (this.isDoneSeeking && this.itemCandidates.None(null));
		}

		// Token: 0x04000755 RID: 1877
		public HashSet<Item> ignoredItems = new HashSet<Item>();

		// Token: 0x04000756 RID: 1878
		public Func<Item, float> GetItemPriority;

		// Token: 0x04000757 RID: 1879
		public Func<Item, bool> ItemFilter;

		// Token: 0x0400075A RID: 1882
		public readonly ImmutableHashSet<Identifier> IdentifiersOrTags;

		// Token: 0x0400075B RID: 1883
		private readonly bool spawnItemIfNotFound;

		// Token: 0x0400075C RID: 1884
		private Item targetItem;

		// Token: 0x0400075D RID: 1885
		private readonly Item originalTarget;

		// Token: 0x0400075E RID: 1886
		public ItemContainer ContainTarget;

		// Token: 0x0400075F RID: 1887
		private ISpatialEntity moveToTarget;

		// Token: 0x04000760 RID: 1888
		private bool isDoneSeeking;

		// Token: 0x04000761 RID: 1889
		private int currentSearchIndex;

		// Token: 0x04000762 RID: 1890
		public ImmutableHashSet<Identifier> ignoredContainerIdentifiers;

		// Token: 0x04000763 RID: 1891
		public ImmutableHashSet<Identifier> ignoredIdentifiersOrTags;

		// Token: 0x04000764 RID: 1892
		private AIObjectiveGoTo goToObjective;

		// Token: 0x04000765 RID: 1893
		private float currItemPriority;

		// Token: 0x04000766 RID: 1894
		private readonly bool checkInventory;

		// Token: 0x04000767 RID: 1895
		public const float DefaultReach = 100f;

		// Token: 0x04000768 RID: 1896
		public const float MaxReach = 150f;

		// Token: 0x04000769 RID: 1897
		private float abandonDelayIfItemNotFound = 5f;

		// Token: 0x0400076A RID: 1898
		public bool IsFindDivingGearSubObjective;

		// Token: 0x04000778 RID: 1912
		private int _itemCount = 1;

		// Token: 0x0400077A RID: 1914
		public static readonly Identifier[] AllowedItemsToTake = new Identifier[]
		{
			Tags.OxygenSource,
			Tags.FireExtinguisher,
			Tags.LightDivingGear,
			Tags.HeavyDivingGear
		};

		// Token: 0x0400077B RID: 1915
		private Stopwatch sw;

		// Token: 0x0400077C RID: 1916
		[TupleElementNames(new string[]
		{
			"item",
			"priority"
		})]
		private readonly List<ValueTuple<Item, float>> itemCandidates = new List<ValueTuple<Item, float>>();

		// Token: 0x0400077D RID: 1917
		private List<Item> itemList;

		// Token: 0x020007D8 RID: 2008
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04002E21 RID: 11809
			public static Func<ItemComponent, float> <0>__GetLethalDamage;
		}
	}
}
