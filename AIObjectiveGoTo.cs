using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200017E RID: 382
	internal class AIObjectiveGoTo : AIObjective
	{
		// Token: 0x17000B74 RID: 2932
		// (get) Token: 0x06002D1E RID: 11550 RVA: 0x001E7B60 File Offset: 0x001E5D60
		// (set) Token: 0x06002D1F RID: 11551 RVA: 0x001E7B68 File Offset: 0x001E5D68
		public override Identifier Identifier { get; set; } = "go to".ToIdentifier();

		// Token: 0x17000B75 RID: 2933
		// (get) Token: 0x06002D20 RID: 11552 RVA: 0x001E7B74 File Offset: 0x001E5D74
		public override string DebugTag
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				ISpatialEntity target = this.Target;
				defaultInterpolatedStringHandler.AppendFormatted(((target != null) ? target.ToString() : null) ?? "none");
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17000B76 RID: 2934
		// (get) Token: 0x06002D21 RID: 11553 RVA: 0x001E7BD8 File Offset: 0x001E5DD8
		public override bool KeepDivingGearOn
		{
			get
			{
				return this.GetTargetHull() == null;
			}
		}

		// Token: 0x17000B77 RID: 2935
		// (get) Token: 0x06002D22 RID: 11554 RVA: 0x001E7BE3 File Offset: 0x001E5DE3
		// (set) Token: 0x06002D23 RID: 11555 RVA: 0x001E7BEB File Offset: 0x001E5DEB
		public bool SpeakIfFails { get; set; } = true;

		// Token: 0x17000B78 RID: 2936
		// (get) Token: 0x06002D24 RID: 11556 RVA: 0x001E7BF4 File Offset: 0x001E5DF4
		// (set) Token: 0x06002D25 RID: 11557 RVA: 0x001E7BFC File Offset: 0x001E5DFC
		public bool DebugLogWhenFails { get; set; } = true;

		// Token: 0x17000B79 RID: 2937
		// (get) Token: 0x06002D26 RID: 11558 RVA: 0x001E7C05 File Offset: 0x001E5E05
		// (set) Token: 0x06002D27 RID: 11559 RVA: 0x001E7C0D File Offset: 0x001E5E0D
		public bool UsePathingOutside { get; set; } = true;

		// Token: 0x17000B7A RID: 2938
		// (get) Token: 0x06002D28 RID: 11560 RVA: 0x001E7C16 File Offset: 0x001E5E16
		// (set) Token: 0x06002D29 RID: 11561 RVA: 0x001E7C1E File Offset: 0x001E5E1E
		public float CloseEnoughMultiplier
		{
			get
			{
				return this._closeEnoughMultiplier;
			}
			set
			{
				this._closeEnoughMultiplier = Math.Max(value, 1f);
			}
		}

		// Token: 0x17000B7B RID: 2939
		// (get) Token: 0x06002D2A RID: 11562 RVA: 0x001E7C34 File Offset: 0x001E5E34
		// (set) Token: 0x06002D2B RID: 11563 RVA: 0x001E7CD8 File Offset: 0x001E5ED8
		public float CloseEnough
		{
			get
			{
				if (this.IsFollowOrder)
				{
					Character targetCharacter = this.Target as Character;
					if (targetCharacter != null && targetCharacter.CurrentHull == null != (this.character.CurrentHull == null))
					{
						return this.minDistance;
					}
				}
				float dist = this._closeEnough * this.CloseEnoughMultiplier;
				float extraMultiplier = Math.Clamp(this.CloseEnoughMultiplier * 0.6f, 1f, 3f);
				if (this.character.AnimController.InWater)
				{
					dist += this.ExtraDistanceWhileSwimming * extraMultiplier;
				}
				if (this.character.CurrentHull == null)
				{
					dist += this.ExtraDistanceOutsideSub * extraMultiplier;
				}
				return dist;
			}
			set
			{
				this._closeEnough = Math.Max(this.minDistance, value);
			}
		}

		// Token: 0x17000B7C RID: 2940
		// (get) Token: 0x06002D2C RID: 11564 RVA: 0x001E7CEC File Offset: 0x001E5EEC
		// (set) Token: 0x06002D2D RID: 11565 RVA: 0x001E7CF4 File Offset: 0x001E5EF4
		public bool IgnoreIfTargetDead { get; set; }

		// Token: 0x17000B7D RID: 2941
		// (get) Token: 0x06002D2E RID: 11566 RVA: 0x001E7CFD File Offset: 0x001E5EFD
		// (set) Token: 0x06002D2F RID: 11567 RVA: 0x001E7D05 File Offset: 0x001E5F05
		public bool AllowGoingOutside { get; set; }

		// Token: 0x17000B7E RID: 2942
		// (get) Token: 0x06002D30 RID: 11568 RVA: 0x001E7D0E File Offset: 0x001E5F0E
		// (set) Token: 0x06002D31 RID: 11569 RVA: 0x001E7D16 File Offset: 0x001E5F16
		public bool FaceTargetOnCompleted { get; set; } = true;

		// Token: 0x17000B7F RID: 2943
		// (get) Token: 0x06002D32 RID: 11570 RVA: 0x001E7D1F File Offset: 0x001E5F1F
		// (set) Token: 0x06002D33 RID: 11571 RVA: 0x001E7D27 File Offset: 0x001E5F27
		public bool AlwaysUseEuclideanDistance { get; set; } = true;

		// Token: 0x17000B80 RID: 2944
		// (get) Token: 0x06002D34 RID: 11572 RVA: 0x001E7D30 File Offset: 0x001E5F30
		// (set) Token: 0x06002D35 RID: 11573 RVA: 0x001E7D38 File Offset: 0x001E5F38
		public bool UseDistanceRelativeToAimSourcePos { get; set; }

		// Token: 0x17000B81 RID: 2945
		// (get) Token: 0x06002D36 RID: 11574 RVA: 0x001E7D41 File Offset: 0x001E5F41
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000B82 RID: 2946
		// (get) Token: 0x06002D37 RID: 11575 RVA: 0x001E7D44 File Offset: 0x001E5F44
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return this.AllowGoingOutside;
			}
		}

		// Token: 0x17000B83 RID: 2947
		// (get) Token: 0x06002D38 RID: 11576 RVA: 0x001E7D4C File Offset: 0x001E5F4C
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B84 RID: 2948
		// (get) Token: 0x06002D39 RID: 11577 RVA: 0x001E7D4F File Offset: 0x001E5F4F
		// (set) Token: 0x06002D3A RID: 11578 RVA: 0x001E7D57 File Offset: 0x001E5F57
		public Identifier DialogueIdentifier { get; set; } = AIObjectiveGoTo.DialogCannotReachPlace;

		// Token: 0x17000B85 RID: 2949
		// (get) Token: 0x06002D3B RID: 11579 RVA: 0x001E7D60 File Offset: 0x001E5F60
		// (set) Token: 0x06002D3C RID: 11580 RVA: 0x001E7D68 File Offset: 0x001E5F68
		public LocalizedString TargetName { get; set; }

		// Token: 0x17000B86 RID: 2950
		// (get) Token: 0x06002D3D RID: 11581 RVA: 0x001E7D71 File Offset: 0x001E5F71
		// (set) Token: 0x06002D3E RID: 11582 RVA: 0x001E7D79 File Offset: 0x001E5F79
		public ISpatialEntity Target { get; private set; }

		// Token: 0x17000B87 RID: 2951
		// (get) Token: 0x06002D3F RID: 11583 RVA: 0x001E7D82 File Offset: 0x001E5F82
		// (set) Token: 0x06002D40 RID: 11584 RVA: 0x001E7D8A File Offset: 0x001E5F8A
		public Func<bool> SpeakCannotReachCondition { get; set; }

		// Token: 0x06002D41 RID: 11585 RVA: 0x001E7D94 File Offset: 0x001E5F94
		protected override float GetPriority()
		{
			bool isOrder = this.objectiveManager.IsOrder(this);
			if (!base.IsAllowed)
			{
				base.Priority = 0f;
				base.Abandon = !isOrder;
				return base.Priority;
			}
			ISpatialEntity target = this.Target;
			bool flag;
			if (target != null)
			{
				Entity entity = target as Entity;
				if (entity != null)
				{
					bool removed = entity.Removed;
					if (removed)
					{
						goto IL_4F;
					}
				}
				flag = false;
				goto IL_57;
			}
			IL_4F:
			flag = true;
			IL_57:
			if (flag)
			{
				base.Priority = 0f;
				base.Abandon = !isOrder;
			}
			if (this.IgnoreIfTargetDead)
			{
				Character character = this.Target as Character;
				if (character != null && character.IsDead)
				{
					base.Priority = 0f;
					base.Abandon = !isOrder;
					goto IL_100;
				}
			}
			if (this.PriorityGetter != null)
			{
				base.Priority = this.PriorityGetter();
			}
			else if (this.OverridePriority != null)
			{
				base.Priority = this.OverridePriority.Value;
			}
			else
			{
				base.Priority = (isOrder ? this.objectiveManager.GetOrderPriority(this) : 10f);
			}
			IL_100:
			return base.Priority;
		}

		// Token: 0x06002D42 RID: 11586 RVA: 0x001E7EA8 File Offset: 0x001E60A8
		public AIObjectiveGoTo(ISpatialEntity target, Character character, AIObjectiveManager objectiveManager, bool repeat = false, bool getDivingGearIfNeeded = true, float priorityModifier = 1f, float closeEnough = 0f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.Target = target;
			this.repeat = repeat;
			this.waitUntilPathUnreachable = this.pathWaitingTime;
			this.getDivingGearIfNeeded = getDivingGearIfNeeded;
			Item i = this.Target as Item;
			if (i != null)
			{
				this.CloseEnough = Math.Max(this.CloseEnough, i.InteractDistance + (float)(Math.Max(i.Rect.Width, i.Rect.Height) / 2));
				return;
			}
			if (this.Target is Character)
			{
				this.CloseEnough = Math.Max(closeEnough, MathUtils.NearlyEqual(closeEnough, 0f, 0.0001f) ? 100f : this.minDistance);
				return;
			}
			this.CloseEnough = closeEnough;
		}

		// Token: 0x06002D43 RID: 11587 RVA: 0x001E8014 File Offset: 0x001E6214
		private void SpeakCannotReach()
		{
			if (!this.character.IsOnPlayerTeam)
			{
				return;
			}
			if (this.objectiveManager.CurrentOrder != this.objectiveManager.CurrentObjective)
			{
				return;
			}
			Identifier dialogueIdentifier = this.DialogueIdentifier;
			if (dialogueIdentifier == null)
			{
				return;
			}
			if (!this.SpeakIfFails)
			{
				return;
			}
			if (this.SpeakCannotReachCondition != null && !this.SpeakCannotReachCondition())
			{
				return;
			}
			if (this.TargetName == null)
			{
				dialogueIdentifier = this.DialogueIdentifier;
				if (dialogueIdentifier == AIObjectiveGoTo.DialogCannotReachTarget)
				{
					this.DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachPlace;
				}
			}
			LocalizedString msg = (this.TargetName == null) ? TextManager.Get(this.DialogueIdentifier) : TextManager.GetWithVariable(this.DialogueIdentifier, "[name]".ToIdentifier(), this.TargetName, (this.Target is Character) ? FormatCapitals.No : FormatCapitals.Yes);
			if (msg.IsNullOrEmpty() || !msg.Loaded)
			{
				return;
			}
			Character character = this.character;
			string value = msg.Value;
			dialogueIdentifier = this.DialogueIdentifier;
			character.Speak(value, null, 0f, dialogueIdentifier, 20f);
		}

		// Token: 0x06002D44 RID: 11588 RVA: 0x001E812C File Offset: 0x001E632C
		public void ForceAct(float deltaTime)
		{
			this.Act(deltaTime);
		}

		// Token: 0x06002D45 RID: 11589 RVA: 0x001E8138 File Offset: 0x001E6338
		protected override void Act(float deltaTime)
		{
			if (this.Target == null)
			{
				base.Abandon = true;
				return;
			}
			if (this.checkExoSuitTimer <= 0f)
			{
				this.checkExoSuitTimer = 2f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
				Item exoSuit = this.character.GetEquippedItem(Tags.PoweredDivingSuit, new InvSlotType?(InvSlotType.OuterClothes));
				if (exoSuit != null)
				{
					Inventory exoSuitInventory = exoSuit.OwnInventory;
					if (exoSuitInventory != null)
					{
						Powered component = exoSuit.GetComponent<Powered>();
						if (component == null || !component.HasPower)
						{
							IEnumerable<Item> fuelRods;
							if (HumanAIController.HasItem(this.character, Tags.DivingSuitFuel, out fuelRods, default(Identifier), 1f, false, true, null))
							{
								if (this.character.IsOnPlayerTeam)
								{
									Character character = this.character;
									string value = TextManager.Get(this.ExoSuitRefuel).Value;
									Identifier identifier = this.ExoSuitRefuel;
									character.Speak(value, null, 0f, identifier, 10f);
								}
								foreach (Item containedItem in exoSuit.ContainedItems.ToList<Item>())
								{
									if (containedItem.HasTag(Tags.DivingSuitFuel) && containedItem.Condition <= 0f)
									{
										this.character.Unequip(containedItem);
									}
								}
								Item fuelRod = fuelRods.MaxBy((Item b) => b.Condition);
								exoSuitInventory.TryPutItem(fuelRod, 1, true, true, this.character, true, false, true);
							}
							else if (this.character.IsOnPlayerTeam)
							{
								Character character2 = this.character;
								string value2 = TextManager.Get(this.ExoSuitOutOfFuel).Value;
								Identifier identifier = this.ExoSuitOutOfFuel;
								character2.Speak(value2, null, 0f, identifier, 30f);
							}
						}
					}
				}
			}
			else
			{
				this.checkExoSuitTimer -= deltaTime;
			}
			if (this.Target == this.character || (this.character.SelectedBy != null && base.HumanAIController.IsFriendly(this.character.SelectedBy, false)))
			{
				this.character.AIController.SteeringManager.Reset();
				return;
			}
			this.character.SelectedItem = null;
			if (this.character.SelectedSecondaryItem != null && !this.character.SelectedSecondaryItem.IsLadder)
			{
				this.character.SelectedSecondaryItem = null;
			}
			Entity e = this.Target as Entity;
			if (e != null)
			{
				if (e.Removed)
				{
					base.Abandon = true;
					return;
				}
				this.character.AIController.SelectTarget(e.AiTarget);
			}
			Hull targetHull = this.GetTargetHull();
			if (!this.IsFollowOrder)
			{
				bool isUnreachable = base.HumanAIController.UnreachableHulls.Contains(targetHull);
				if (!this.objectiveManager.CurrentObjective.IgnoreUnsafeHulls && (this.IsWaitOrder || !this.objectiveManager.HasOrders()))
				{
					if (base.HumanAIController.UnsafeHulls.Contains(targetHull))
					{
						isUnreachable = true;
						base.HumanAIController.AskToRecalculateHullSafety(targetHull);
					}
					else
					{
						IndoorsSteeringManager pathSteering = base.PathSteering;
						if (((pathSteering != null) ? pathSteering.CurrentPath : null) != null)
						{
							foreach (WayPoint wp in base.PathSteering.CurrentPath.Nodes)
							{
								if (wp.CurrentHull != null && base.HumanAIController.UnsafeHulls.Contains(wp.CurrentHull))
								{
									isUnreachable = true;
									base.HumanAIController.AskToRecalculateHullSafety(wp.CurrentHull);
								}
							}
						}
					}
				}
				if (isUnreachable)
				{
					base.SteeringManager.Reset();
					IndoorsSteeringManager pathSteering2 = base.PathSteering;
					if (((pathSteering2 != null) ? pathSteering2.CurrentPath : null) != null)
					{
						base.PathSteering.CurrentPath.Unreachable = true;
					}
					if (this.repeat)
					{
						this.SpeakCannotReach();
						return;
					}
					base.Abandon = true;
					return;
				}
			}
			bool insideSteering = base.SteeringManager == base.PathSteering && base.PathSteering.CurrentPath != null && !base.PathSteering.IsPathDirty;
			bool isInside = this.character.CurrentHull != null;
			bool hasOutdoorNodes = insideSteering && base.PathSteering.CurrentPath.HasOutdoorsNodes;
			if (isInside && hasOutdoorNodes && !this.AllowGoingOutside)
			{
				base.Abandon = true;
			}
			else if (base.HumanAIController.SteeringManager == base.PathSteering)
			{
				this.waitUntilPathUnreachable -= deltaTime;
				if (base.HumanAIController.IsCurrentPathNullOrUnreachable)
				{
					base.SteeringManager.Reset();
					if (this.waitUntilPathUnreachable < 0f)
					{
						this.waitUntilPathUnreachable = this.pathWaitingTime;
						if (this.repeat && !base.IsCompleted)
						{
							if (!this.<Act>g__IsDoneFollowing|101_1())
							{
								this.SpeakCannotReach();
							}
						}
						else
						{
							base.Abandon = true;
						}
					}
				}
				else if (base.HumanAIController.HasValidPath(true, false, null))
				{
					this.waitUntilPathUnreachable = this.pathWaitingTime;
				}
			}
			if (base.Abandon)
			{
				return;
			}
			if (!this.IsFindDivingGearSubObjective)
			{
				bool needsDivingSuit = (!isInside || hasOutdoorNodes) && !this.character.IsImmuneToPressure;
				bool tryToGetDivingGear = needsDivingSuit || base.HumanAIController.NeedsDivingGear(targetHull, out needsDivingSuit, null);
				bool tryToGetDivingSuit = needsDivingSuit;
				Character followTarget = this.Target as Character;
				if (this.Mimic && !this.character.IsImmuneToPressure)
				{
					if (HumanAIController.HasDivingSuit(followTarget, 0f, true, true))
					{
						tryToGetDivingGear = true;
						tryToGetDivingSuit = true;
					}
					else if (HumanAIController.HasDivingMask(followTarget, 0f, true) && this.character.CharacterHealth.OxygenLowResistance < 1f)
					{
						tryToGetDivingGear = true;
					}
				}
				bool needsEquipment = false;
				float minOxygen = AIObjectiveFindDivingGear.GetMinOxygen(this.character);
				if (tryToGetDivingSuit)
				{
					needsEquipment = !HumanAIController.HasDivingSuit(this.character, minOxygen, true, !this.objectiveManager.FailedToFindDivingGearForDepth);
				}
				else if (tryToGetDivingGear)
				{
					needsEquipment = !HumanAIController.HasDivingGear(this.character, minOxygen, true);
				}
				if (!this.getDivingGearIfNeeded)
				{
					if (needsEquipment)
					{
						base.Abandon = true;
						return;
					}
				}
				else
				{
					if (this.character.LockHands)
					{
						this.cantFindDivingGear = true;
					}
					if (this.cantFindDivingGear & needsDivingSuit)
					{
						base.Abandon = true;
						return;
					}
					if (needsEquipment && !this.cantFindDivingGear)
					{
						base.SteeringManager.Reset();
						Func<AIObjectiveFindDivingGear> <>9__6;
						base.TryAddSubObjective<AIObjectiveFindDivingGear>(ref this.findDivingGear, () => new AIObjectiveFindDivingGear(this.character, tryToGetDivingSuit, this.objectiveManager, 1f), delegate
						{
							base.RemoveSubObjective<AIObjectiveFindDivingGear>(ref this.findDivingGear);
						}, delegate
						{
							this.cantFindDivingGear = true;
							if (needsDivingSuit)
							{
								this.Abandon = true;
								return;
							}
							this.RemoveSubObjective<AIObjectiveFindDivingGear>(ref this.findDivingGear);
							AIObjective <>4__this = this;
							AIObjectiveGoTo <>4__this2 = this;
							Func<AIObjectiveFindDivingGear> constructor;
							if ((constructor = <>9__6) == null)
							{
								constructor = (<>9__6 = (() => new AIObjectiveFindDivingGear(this.character, !tryToGetDivingSuit, this.objectiveManager, 1f)));
							}
							<>4__this.TryAddSubObjective<AIObjectiveFindDivingGear>(ref <>4__this2.findDivingGear, constructor, delegate
							{
								base.RemoveSubObjective<AIObjectiveFindDivingGear>(ref this.findDivingGear);
							}, delegate
							{
								base.Abandon = (this.character.CurrentHull != null && (this.objectiveManager.CurrentOrder != this || this.Target.Submarine == null));
								base.RemoveSubObjective<AIObjectiveFindDivingGear>(ref this.findDivingGear);
							});
						});
						return;
					}
				}
			}
			if (this.<Act>g__IsDoneFollowing|101_1())
			{
				this.OnCompleted();
				return;
			}
			float maxGapDistance = 500f;
			Character targetCharacter = this.Target as Character;
			if (this.character.AnimController.InWater)
			{
				if (this.character.CurrentHull == null || (this.IsFollowOrder && targetCharacter != null && targetCharacter.CurrentHull == null != (this.character.CurrentHull == null) && Vector2.DistanceSquared(this.character.WorldPosition, this.Target.WorldPosition) < maxGapDistance * maxGapDistance))
				{
					if (this.seekGapsTimer > 0f)
					{
						this.seekGapsTimer -= deltaTime;
					}
					else
					{
						Submarine submarine = this.character.Submarine;
						bool flag;
						if (submarine == null)
						{
							flag = false;
						}
						else
						{
							bool isRuin = submarine.Info.IsRuin;
							flag = true;
						}
						bool flag2;
						if (!flag)
						{
							Submarine submarine2 = this.Target.Submarine;
							if (submarine2 == null)
							{
								flag2 = false;
							}
							else
							{
								bool isRuin2 = submarine2.Info.IsRuin;
								flag2 = true;
							}
						}
						else
						{
							flag2 = true;
						}
						bool isRuins = flag2;
						bool isEitherOneInside = isInside || this.Target.Submarine != null;
						if (isEitherOneInside && (!isRuins || !base.HumanAIController.HasValidPath(true, true, null)))
						{
							this.SeekGaps(maxGapDistance);
							this.seekGapsTimer = this.seekGapsInterval * Rand.Range(0.1f, 1.1f, Rand.RandSync.Unsynced);
							if (this.TargetGap != null)
							{
								Vector2 rayStart = this.character.SimPosition;
								Vector2 rayEnd = this.TargetGap.SimPosition;
								if (this.TargetGap.Submarine != null && this.character.Submarine == null)
								{
									rayStart -= this.TargetGap.Submarine.SimPosition;
								}
								else if (this.TargetGap.Submarine == null && this.character.Submarine != null)
								{
									rayEnd -= this.character.Submarine.SimPosition;
								}
								Body closestBody = Submarine.CheckVisibility(rayStart, rayEnd, false, true, true, true, true, null);
								if (closestBody != null)
								{
									this.TargetGap = null;
								}
							}
						}
						else
						{
							this.TargetGap = null;
						}
					}
				}
				else
				{
					this.TargetGap = null;
				}
				if (this.TargetGap != null)
				{
					if (this.TargetGap.FlowTargetHull != null && base.HumanAIController.SteerThroughGap(this.TargetGap, this.IsFollowOrder ? this.Target.WorldPosition : this.TargetGap.FlowTargetHull.WorldPosition, deltaTime, -1f))
					{
						base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 1f);
						return;
					}
					this.TargetGap = null;
				}
				if (this.checkScooterTimer <= 0f)
				{
					this.useScooter = false;
					this.checkScooterTimer = 0.5f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
					Item scooter = null;
					bool shouldUseScooter = this.Mimic && targetCharacter != null && targetCharacter.HasEquippedItem(Tags.Scooter, false, null);
					if (!shouldUseScooter)
					{
						float threshold = 500f;
						if (isInside)
						{
							Vector2 diff = this.Target.WorldPosition - this.character.WorldPosition;
							shouldUseScooter = (Math.Abs(diff.X) > threshold || Math.Abs(diff.Y) > 150f);
						}
						else
						{
							shouldUseScooter = (Vector2.DistanceSquared(this.character.WorldPosition, this.Target.WorldPosition) > threshold * threshold);
						}
					}
					IEnumerable<Item> equippedScooters;
					if (HumanAIController.HasItem(this.character, Tags.Scooter, out equippedScooters, default(Identifier), 0f, true, false, null))
					{
						scooter = equippedScooters.FirstOrDefault<Item>();
					}
					else if (shouldUseScooter)
					{
						ValueTuple<Item, Item> items;
						bool hasHandsFull = this.character.HasHandsFull(out items);
						if (hasHandsFull)
						{
							hasHandsFull = (!this.character.TryPutItemInAnySlot(items.Item1) && !this.character.TryPutItemInAnySlot(items.Item2) && !this.character.TryPutItemInBag(items.Item1) && !this.character.TryPutItemInBag(items.Item2));
						}
						if (!hasHandsFull)
						{
							bool hasBattery = false;
							IEnumerable<Item> nonEquippedScootersWithBattery;
							IEnumerable<Item> nonEquippedScootersWithoutBattery;
							if (HumanAIController.HasItem(this.character, Tags.Scooter, out nonEquippedScootersWithBattery, Tags.MobileBattery, 1f, false, true, null))
							{
								scooter = nonEquippedScootersWithBattery.FirstOrDefault<Item>();
								hasBattery = true;
							}
							else if (HumanAIController.HasItem(this.character, Tags.Scooter, out nonEquippedScootersWithoutBattery, default(Identifier), 0f, false, true, null))
							{
								scooter = nonEquippedScootersWithoutBattery.FirstOrDefault<Item>();
								IEnumerable<Item> enumerable;
								hasBattery = HumanAIController.HasItem(this.character, Tags.MobileBattery, out enumerable, default(Identifier), 1f, false, false, null);
							}
							if (scooter != null && hasBattery)
							{
								base.HumanAIController.TakeItem(scooter, this.character.Inventory, true, false, false, true, false, null);
							}
						}
					}
					if (scooter != null && this.character.HasEquippedItem(scooter, null, null))
					{
						if (shouldUseScooter)
						{
							this.useScooter = true;
							if (scooter.ContainedItems.None((Item i) => i.Condition > 0f))
							{
								IEnumerable<Item> batteries;
								if (HumanAIController.HasItem(this.character, Tags.MobileBattery, out batteries, default(Identifier), 1f, false, false, null))
								{
									scooter.ContainedItems.ForEachMod(delegate(Item emptyBattery)
									{
										this.character.Inventory.TryPutItem(emptyBattery, this.character, CharacterInventory.AnySlot, true, false, true);
									});
									if (!scooter.Combine((from b in batteries
									orderby b.Condition descending
									select b).First<Item>(), this.character))
									{
										this.useScooter = false;
									}
								}
								else
								{
									this.useScooter = false;
								}
							}
						}
						if (!this.useScooter)
						{
							this.character.TryPutItemInAnySlot(scooter);
						}
					}
				}
				else
				{
					this.checkScooterTimer -= deltaTime;
				}
			}
			else
			{
				this.TargetGap = null;
				this.useScooter = false;
				this.checkScooterTimer = 0f;
			}
			if (base.SteeringManager == base.PathSteering)
			{
				Vector2 targetPos = this.character.GetRelativeSimPosition(this.Target, null);
				Func<PathNode, bool> nodeFilter = null;
				if (isInside && !this.AllowGoingOutside)
				{
					nodeFilter = ((PathNode n) => n.Waypoint.CurrentHull != null);
				}
				else if (!isInside)
				{
					if (base.HumanAIController.UseOutsideWaypoints)
					{
						nodeFilter = ((PathNode n) => n.Waypoint.Submarine == null);
					}
					else
					{
						nodeFilter = ((PathNode n) => n.Waypoint.Submarine != null || n.Waypoint.Ruin != null);
					}
				}
				if (!isInside && !this.UsePathingOutside)
				{
					this.character.ReleaseSecondaryItem();
					base.PathSteering.SteeringSeekSimple(this.character.GetRelativeSimPosition(this.Target, null), 10f);
					if (this.character.AnimController.InWater)
					{
						base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
					}
				}
				else
				{
					base.PathSteering.SteeringSeek(targetPos, 1f, 0f, (PathNode n) => n.Waypoint.CurrentHull == null == (this.character.CurrentHull == null), this.endNodeFilter, nodeFilter, this.Target is Item || this.Target is Character, 0f);
				}
				if (!isInside && (base.PathSteering.CurrentPath == null || base.PathSteering.IsPathDirty || base.PathSteering.CurrentPath.Unreachable))
				{
					if (this.useScooter)
					{
						this.<Act>g__UseScooter|101_0(this.Target.WorldPosition);
						return;
					}
					this.character.ReleaseSecondaryItem();
					base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(this.Target.WorldPosition - this.character.WorldPosition));
					if (this.character.AnimController.InWater)
					{
						base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 2f);
						return;
					}
				}
				else if (this.useScooter)
				{
					SteeringPath currentPath = base.PathSteering.CurrentPath;
					if (((currentPath != null) ? currentPath.CurrentNode : null) != null)
					{
						this.<Act>g__UseScooter|101_0(base.PathSteering.CurrentPath.CurrentNode.WorldPosition);
						return;
					}
				}
			}
			else
			{
				if (this.useScooter)
				{
					this.<Act>g__UseScooter|101_0(this.Target.WorldPosition);
					return;
				}
				this.character.ReleaseSecondaryItem();
				base.SteeringManager.SteeringSeek(this.character.GetRelativeSimPosition(this.Target, null), 10f);
				if (this.character.AnimController.InWater)
				{
					base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
				}
			}
		}

		// Token: 0x06002D46 RID: 11590 RVA: 0x001E90C8 File Offset: 0x001E72C8
		public Hull GetTargetHull()
		{
			return AIObjectiveGoTo.GetTargetHull(this.Target);
		}

		// Token: 0x06002D47 RID: 11591 RVA: 0x001E90D8 File Offset: 0x001E72D8
		public static Hull GetTargetHull(ISpatialEntity target)
		{
			Hull h = target as Hull;
			if (h != null)
			{
				return h;
			}
			Item i = target as Item;
			if (i != null)
			{
				return i.CurrentHull;
			}
			Character c = target as Character;
			if (c != null)
			{
				return c.CurrentHull ?? c.AnimController.CurrentHull;
			}
			Structure structure = target as Structure;
			if (structure != null)
			{
				return Hull.FindHull(structure.Position, null, false, true);
			}
			Gap g = target as Gap;
			if (g != null)
			{
				return g.FlowTargetHull;
			}
			WayPoint wp = target as WayPoint;
			if (wp != null)
			{
				return wp.CurrentHull;
			}
			FireSource fs = target as FireSource;
			if (fs != null)
			{
				return fs.Hull;
			}
			OrderTarget ot = target as OrderTarget;
			if (ot != null)
			{
				return ot.Hull;
			}
			return null;
		}

		// Token: 0x17000B88 RID: 2952
		// (get) Token: 0x06002D48 RID: 11592 RVA: 0x001E918C File Offset: 0x001E738C
		// (set) Token: 0x06002D49 RID: 11593 RVA: 0x001E9194 File Offset: 0x001E7394
		public Gap TargetGap { get; private set; }

		// Token: 0x06002D4A RID: 11594 RVA: 0x001E91A0 File Offset: 0x001E73A0
		private void SeekGaps(float maxDistance)
		{
			Gap selectedGap = null;
			float selectedDistance = -1f;
			Vector2 toTargetNormalized = Vector2.Normalize(this.Target.WorldPosition - this.character.WorldPosition);
			foreach (Gap gap in Gap.GapList)
			{
				if (gap.Open >= 1f && gap.Submarine != null && (this.IsFollowOrder || (gap.FlowTargetHull != null && gap.Submarine == this.Target.Submarine)))
				{
					Vector2 toGap = gap.WorldPosition - this.character.WorldPosition;
					if (Vector2.Dot(Vector2.Normalize(toGap), toTargetNormalized) >= 0f)
					{
						float squaredDistance = toGap.LengthSquared();
						if (squaredDistance <= maxDistance * maxDistance && (selectedGap == null || squaredDistance < selectedDistance))
						{
							selectedGap = gap;
							selectedDistance = squaredDistance;
						}
					}
				}
			}
			this.TargetGap = selectedGap;
		}

		// Token: 0x17000B89 RID: 2953
		// (get) Token: 0x06002D4B RID: 11595 RVA: 0x001E92AC File Offset: 0x001E74AC
		public bool IsCloseEnough
		{
			get
			{
				if (this.character.IsClimbing && base.SteeringManager == base.PathSteering && base.PathSteering.CurrentPath != null && !base.PathSteering.CurrentPath.Finished && base.PathSteering.IsCurrentNodeLadder && !base.PathSteering.CurrentPath.IsAtEndNode)
				{
					if (this.Target.WorldPosition.Y > this.character.WorldPosition.Y)
					{
						return false;
					}
					if (!this.character.AnimController.IsAboveFloor)
					{
						return false;
					}
					Item targetItem = this.Target as Item;
					if (targetItem != null && targetItem.GetComponent<Pickable>() == null)
					{
						return false;
					}
				}
				if (this.AlwaysUseEuclideanDistance || this.character.AnimController.InWater)
				{
					Vector2 sourcePos = this.UseDistanceRelativeToAimSourcePos ? this.character.AnimController.AimSourceWorldPos : this.character.WorldPosition;
					return Vector2.DistanceSquared(this.Target.WorldPosition, sourcePos) < this.CloseEnough * this.CloseEnough;
				}
				float yDist = Math.Abs(this.Target.WorldPosition.Y - this.character.WorldPosition.Y);
				if (yDist > this.CloseEnough)
				{
					return false;
				}
				float xDist = Math.Abs(this.Target.WorldPosition.X - this.character.WorldPosition.X);
				return xDist <= this.CloseEnough;
			}
		}

		// Token: 0x06002D4C RID: 11596 RVA: 0x001E9434 File Offset: 0x001E7634
		protected override bool CheckObjectiveState()
		{
			if (this.Target == null)
			{
				base.Abandon = true;
				return false;
			}
			if (this.repeat)
			{
				return false;
			}
			if (this.IsCloseEnough && (this.requiredCondition == null || this.requiredCondition()))
			{
				Item item = this.Target as Item;
				if (item != null)
				{
					float num;
					if (this.character.CanInteractWith(item, out num, false))
					{
						base.IsCompleted = true;
					}
				}
				else
				{
					Character targetCharacter = this.Target as Character;
					if (targetCharacter != null)
					{
						this.character.SelectCharacter(targetCharacter);
						if (this.character.CanInteractWith(targetCharacter, 200f, true, true))
						{
							base.IsCompleted = true;
						}
						this.character.DeselectCharacter();
					}
					else
					{
						base.IsCompleted = true;
					}
				}
			}
			return base.IsCompleted;
		}

		// Token: 0x06002D4D RID: 11597 RVA: 0x001E94F6 File Offset: 0x001E76F6
		protected override void OnAbandon()
		{
			this.StopMovement();
			if (base.SteeringManager == base.PathSteering)
			{
				base.PathSteering.ResetPath();
			}
			this.SpeakCannotReach();
			base.OnAbandon();
		}

		// Token: 0x06002D4E RID: 11598 RVA: 0x001E9524 File Offset: 0x001E7724
		private void StopMovement()
		{
			SteeringManager steeringManager = base.SteeringManager;
			if (steeringManager != null)
			{
				steeringManager.Reset();
			}
			if (this.FaceTargetOnCompleted)
			{
				Entity entity = this.Target as Entity;
				if (entity != null && !entity.Removed)
				{
					base.HumanAIController.FaceTarget(this.Target);
				}
			}
		}

		// Token: 0x06002D4F RID: 11599 RVA: 0x001E9574 File Offset: 0x001E7774
		protected override void OnCompleted()
		{
			this.StopMovement();
			WayPoint wayPoint = this.Target as WayPoint;
			if (wayPoint != null && wayPoint.Ladders == null && this.character.IsClimbing && this.character.AnimController.IsAboveFloor)
			{
				this.character.StopClimbing();
			}
			base.OnCompleted();
		}

		// Token: 0x06002D50 RID: 11600 RVA: 0x001E95D0 File Offset: 0x001E77D0
		public override void Reset()
		{
			base.Reset();
			this.findDivingGear = null;
			this.seekGapsTimer = 0f;
			this.TargetGap = null;
			IndoorsSteeringManager pathSteering = base.SteeringManager as IndoorsSteeringManager;
			if (pathSteering != null)
			{
				pathSteering.ResetPath();
			}
		}

		// Token: 0x06002D51 RID: 11601 RVA: 0x001E9614 File Offset: 0x001E7814
		public bool ShouldRun(bool run)
		{
			if (base.ForceWalk)
			{
				return false;
			}
			if (run && this.objectiveManager.ForcedOrder == this && this.IsWaitOrder && !this.character.IsOnPlayerTeam)
			{
				run = false;
			}
			else if (this.Target != null)
			{
				if (this.character.CurrentHull == null)
				{
					run = (Vector2.DistanceSquared(this.character.WorldPosition, this.Target.WorldPosition) > 90000f);
				}
				else
				{
					float yDiff = this.Target.WorldPosition.Y - this.character.WorldPosition.Y;
					if (Math.Abs(yDiff) > 100f)
					{
						run = true;
					}
					else
					{
						float xDiff = this.Target.WorldPosition.X - this.character.WorldPosition.X;
						run = (Math.Abs(xDiff) > 500f);
					}
				}
			}
			return run;
		}

		// Token: 0x06002D58 RID: 11608 RVA: 0x001E9804 File Offset: 0x001E7A04
		[CompilerGenerated]
		private void <Act>g__UseScooter|101_0(Vector2 targetWorldPos)
		{
			if (!this.character.HasEquippedItem("scooter".ToIdentifier(), true, null))
			{
				return;
			}
			base.SteeringManager.Reset();
			this.character.ReleaseSecondaryItem();
			this.character.CursorPosition = targetWorldPos;
			if (this.character.Submarine != null)
			{
				this.character.CursorPosition -= this.character.Submarine.Position;
			}
			Vector2 diff = this.character.CursorPosition - this.character.Position;
			Vector2 dir = Vector2.Normalize(diff);
			if (this.character.CurrentHull == null && this.IsFollowOrder)
			{
				float sqrDist = diff.LengthSquared();
				if (sqrDist > MathUtils.Pow2(this.CloseEnough * 1.5f))
				{
					base.SteeringManager.SteeringManual(1f, dir);
				}
				else
				{
					float dot = Vector2.Dot(dir, VectorExtensions.Forward(this.character.AnimController.Collider.Rotation + 1.5707964f, 1f));
					if (dot <= 0.9f && sqrDist > MathUtils.Pow2(this.CloseEnough))
					{
						base.SteeringManager.SteeringManual(1f, dir);
					}
				}
			}
			else
			{
				base.SteeringManager.SteeringManual(1f, dir);
			}
			this.character.SetInput(InputType.Aim, false, true);
			this.character.SetInput(InputType.Shoot, false, true);
		}

		// Token: 0x06002D59 RID: 11609 RVA: 0x001E9980 File Offset: 0x001E7B80
		[CompilerGenerated]
		private bool <Act>g__IsDoneFollowing|101_1()
		{
			return this.repeat && this.IsCloseEnough && (this.requiredCondition == null || this.requiredCondition()) && this.character.CanSeeTarget(this.Target, null, false, false) && (!this.character.IsClimbing || this.IsFollowOrder);
		}

		// Token: 0x0400178A RID: 6026
		public bool IsFindDivingGearSubObjective;

		// Token: 0x0400178B RID: 6027
		private AIObjectiveFindDivingGear findDivingGear;

		// Token: 0x0400178C RID: 6028
		private readonly bool repeat;

		// Token: 0x0400178D RID: 6029
		private float waitUntilPathUnreachable;

		// Token: 0x0400178E RID: 6030
		private readonly bool getDivingGearIfNeeded;

		// Token: 0x0400178F RID: 6031
		public Func<bool> requiredCondition;

		// Token: 0x04001790 RID: 6032
		public Func<PathNode, bool> endNodeFilter;

		// Token: 0x04001791 RID: 6033
		public Func<float> PriorityGetter;

		// Token: 0x04001792 RID: 6034
		public bool IsFollowOrder;

		// Token: 0x04001793 RID: 6035
		public bool IsWaitOrder;

		// Token: 0x04001794 RID: 6036
		public bool Mimic;

		// Token: 0x04001798 RID: 6040
		public float ExtraDistanceWhileSwimming;

		// Token: 0x04001799 RID: 6041
		public float ExtraDistanceOutsideSub;

		// Token: 0x0400179A RID: 6042
		private float _closeEnoughMultiplier = 1f;

		// Token: 0x0400179B RID: 6043
		private float _closeEnough = 50f;

		// Token: 0x0400179C RID: 6044
		private readonly float minDistance = 50f;

		// Token: 0x0400179D RID: 6045
		private readonly float seekGapsInterval = 1f;

		// Token: 0x0400179E RID: 6046
		private float seekGapsTimer;

		// Token: 0x0400179F RID: 6047
		private bool cantFindDivingGear;

		// Token: 0x040017A5 RID: 6053
		public static readonly Identifier DialogCannotReachTarget = "dialogcannotreachtarget".ToIdentifier();

		// Token: 0x040017A6 RID: 6054
		public static readonly Identifier DialogCannotReachPlace = "dialogcannotreachplace".ToIdentifier();

		// Token: 0x040017A7 RID: 6055
		public static readonly Identifier DialogCannotReachPatient = "dialogcannotreachpatient".ToIdentifier();

		// Token: 0x040017A8 RID: 6056
		public static readonly Identifier DialogCannotReachFire = "dialogcannotreachfire".ToIdentifier();

		// Token: 0x040017A9 RID: 6057
		public static readonly Identifier DialogCannotReachLeak = "dialogcannotreachleak".ToIdentifier();

		// Token: 0x040017AB RID: 6059
		private readonly Identifier ExoSuitRefuel = "dialog.exosuit.refuel".ToIdentifier();

		// Token: 0x040017AC RID: 6060
		private readonly Identifier ExoSuitOutOfFuel = "dialog.exosuit.outoffuel".ToIdentifier();

		// Token: 0x040017AF RID: 6063
		public float? OverridePriority;

		// Token: 0x040017B1 RID: 6065
		private readonly float avoidLookAheadDistance = 5f;

		// Token: 0x040017B2 RID: 6066
		private readonly float pathWaitingTime = 3f;

		// Token: 0x040017B3 RID: 6067
		private bool useScooter;

		// Token: 0x040017B4 RID: 6068
		private float checkScooterTimer;

		// Token: 0x040017B5 RID: 6069
		private const float CheckScooterTime = 0.5f;

		// Token: 0x040017B6 RID: 6070
		private float checkExoSuitTimer;

		// Token: 0x040017B7 RID: 6071
		private const float CheckExoSuitTime = 2f;
	}
}
