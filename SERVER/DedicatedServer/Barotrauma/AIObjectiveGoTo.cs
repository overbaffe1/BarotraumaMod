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
	// Token: 0x02000078 RID: 120
	internal class AIObjectiveGoTo : AIObjective
	{
		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x06001016 RID: 4118 RVA: 0x00094CCC File Offset: 0x00092ECC
		// (set) Token: 0x06001017 RID: 4119 RVA: 0x00094CD4 File Offset: 0x00092ED4
		public override Identifier Identifier { get; set; } = "go to".ToIdentifier();

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x06001018 RID: 4120 RVA: 0x00094CE0 File Offset: 0x00092EE0
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

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x06001019 RID: 4121 RVA: 0x00094D44 File Offset: 0x00092F44
		public override bool KeepDivingGearOn
		{
			get
			{
				return this.GetTargetHull() == null;
			}
		}

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x0600101A RID: 4122 RVA: 0x00094D4F File Offset: 0x00092F4F
		// (set) Token: 0x0600101B RID: 4123 RVA: 0x00094D57 File Offset: 0x00092F57
		public bool SpeakIfFails { get; set; } = true;

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x0600101C RID: 4124 RVA: 0x00094D60 File Offset: 0x00092F60
		// (set) Token: 0x0600101D RID: 4125 RVA: 0x00094D68 File Offset: 0x00092F68
		public bool DebugLogWhenFails { get; set; } = true;

		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x0600101E RID: 4126 RVA: 0x00094D71 File Offset: 0x00092F71
		// (set) Token: 0x0600101F RID: 4127 RVA: 0x00094D79 File Offset: 0x00092F79
		public bool UsePathingOutside { get; set; } = true;

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x06001020 RID: 4128 RVA: 0x00094D82 File Offset: 0x00092F82
		// (set) Token: 0x06001021 RID: 4129 RVA: 0x00094D8A File Offset: 0x00092F8A
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

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x06001022 RID: 4130 RVA: 0x00094DA0 File Offset: 0x00092FA0
		// (set) Token: 0x06001023 RID: 4131 RVA: 0x00094E44 File Offset: 0x00093044
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

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x06001024 RID: 4132 RVA: 0x00094E58 File Offset: 0x00093058
		// (set) Token: 0x06001025 RID: 4133 RVA: 0x00094E60 File Offset: 0x00093060
		public bool IgnoreIfTargetDead { get; set; }

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x06001026 RID: 4134 RVA: 0x00094E69 File Offset: 0x00093069
		// (set) Token: 0x06001027 RID: 4135 RVA: 0x00094E71 File Offset: 0x00093071
		public bool AllowGoingOutside { get; set; }

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x06001028 RID: 4136 RVA: 0x00094E7A File Offset: 0x0009307A
		// (set) Token: 0x06001029 RID: 4137 RVA: 0x00094E82 File Offset: 0x00093082
		public bool FaceTargetOnCompleted { get; set; } = true;

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x0600102A RID: 4138 RVA: 0x00094E8B File Offset: 0x0009308B
		// (set) Token: 0x0600102B RID: 4139 RVA: 0x00094E93 File Offset: 0x00093093
		public bool AlwaysUseEuclideanDistance { get; set; } = true;

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x0600102C RID: 4140 RVA: 0x00094E9C File Offset: 0x0009309C
		// (set) Token: 0x0600102D RID: 4141 RVA: 0x00094EA4 File Offset: 0x000930A4
		public bool UseDistanceRelativeToAimSourcePos { get; set; }

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x0600102E RID: 4142 RVA: 0x00094EAD File Offset: 0x000930AD
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x0600102F RID: 4143 RVA: 0x00094EB0 File Offset: 0x000930B0
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return this.AllowGoingOutside;
			}
		}

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x06001030 RID: 4144 RVA: 0x00094EB8 File Offset: 0x000930B8
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x06001031 RID: 4145 RVA: 0x00094EBB File Offset: 0x000930BB
		// (set) Token: 0x06001032 RID: 4146 RVA: 0x00094EC3 File Offset: 0x000930C3
		public Identifier DialogueIdentifier { get; set; } = AIObjectiveGoTo.DialogCannotReachPlace;

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x06001033 RID: 4147 RVA: 0x00094ECC File Offset: 0x000930CC
		// (set) Token: 0x06001034 RID: 4148 RVA: 0x00094ED4 File Offset: 0x000930D4
		public LocalizedString TargetName { get; set; }

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x06001035 RID: 4149 RVA: 0x00094EDD File Offset: 0x000930DD
		// (set) Token: 0x06001036 RID: 4150 RVA: 0x00094EE5 File Offset: 0x000930E5
		public ISpatialEntity Target { get; private set; }

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x06001037 RID: 4151 RVA: 0x00094EEE File Offset: 0x000930EE
		// (set) Token: 0x06001038 RID: 4152 RVA: 0x00094EF6 File Offset: 0x000930F6
		public Func<bool> SpeakCannotReachCondition { get; set; }

		// Token: 0x06001039 RID: 4153 RVA: 0x00094F00 File Offset: 0x00093100
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

		// Token: 0x0600103A RID: 4154 RVA: 0x00095014 File Offset: 0x00093214
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

		// Token: 0x0600103B RID: 4155 RVA: 0x00095180 File Offset: 0x00093380
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

		// Token: 0x0600103C RID: 4156 RVA: 0x00095298 File Offset: 0x00093498
		public void ForceAct(float deltaTime)
		{
			this.Act(deltaTime);
		}

		// Token: 0x0600103D RID: 4157 RVA: 0x000952A4 File Offset: 0x000934A4
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

		// Token: 0x0600103E RID: 4158 RVA: 0x00096234 File Offset: 0x00094434
		public Hull GetTargetHull()
		{
			return AIObjectiveGoTo.GetTargetHull(this.Target);
		}

		// Token: 0x0600103F RID: 4159 RVA: 0x00096244 File Offset: 0x00094444
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

		// Token: 0x1700046E RID: 1134
		// (get) Token: 0x06001040 RID: 4160 RVA: 0x000962F8 File Offset: 0x000944F8
		// (set) Token: 0x06001041 RID: 4161 RVA: 0x00096300 File Offset: 0x00094500
		public Gap TargetGap { get; private set; }

		// Token: 0x06001042 RID: 4162 RVA: 0x0009630C File Offset: 0x0009450C
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

		// Token: 0x1700046F RID: 1135
		// (get) Token: 0x06001043 RID: 4163 RVA: 0x00096418 File Offset: 0x00094618
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

		// Token: 0x06001044 RID: 4164 RVA: 0x000965A0 File Offset: 0x000947A0
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

		// Token: 0x06001045 RID: 4165 RVA: 0x00096662 File Offset: 0x00094862
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

		// Token: 0x06001046 RID: 4166 RVA: 0x00096690 File Offset: 0x00094890
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

		// Token: 0x06001047 RID: 4167 RVA: 0x000966E0 File Offset: 0x000948E0
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

		// Token: 0x06001048 RID: 4168 RVA: 0x0009673C File Offset: 0x0009493C
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

		// Token: 0x06001049 RID: 4169 RVA: 0x00096780 File Offset: 0x00094980
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

		// Token: 0x06001050 RID: 4176 RVA: 0x00096970 File Offset: 0x00094B70
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

		// Token: 0x06001051 RID: 4177 RVA: 0x00096AEC File Offset: 0x00094CEC
		[CompilerGenerated]
		private bool <Act>g__IsDoneFollowing|101_1()
		{
			return this.repeat && this.IsCloseEnough && (this.requiredCondition == null || this.requiredCondition()) && this.character.CanSeeTarget(this.Target, null, false, false) && (!this.character.IsClimbing || this.IsFollowOrder);
		}

		// Token: 0x04000790 RID: 1936
		public bool IsFindDivingGearSubObjective;

		// Token: 0x04000791 RID: 1937
		private AIObjectiveFindDivingGear findDivingGear;

		// Token: 0x04000792 RID: 1938
		private readonly bool repeat;

		// Token: 0x04000793 RID: 1939
		private float waitUntilPathUnreachable;

		// Token: 0x04000794 RID: 1940
		private readonly bool getDivingGearIfNeeded;

		// Token: 0x04000795 RID: 1941
		public Func<bool> requiredCondition;

		// Token: 0x04000796 RID: 1942
		public Func<PathNode, bool> endNodeFilter;

		// Token: 0x04000797 RID: 1943
		public Func<float> PriorityGetter;

		// Token: 0x04000798 RID: 1944
		public bool IsFollowOrder;

		// Token: 0x04000799 RID: 1945
		public bool IsWaitOrder;

		// Token: 0x0400079A RID: 1946
		public bool Mimic;

		// Token: 0x0400079E RID: 1950
		public float ExtraDistanceWhileSwimming;

		// Token: 0x0400079F RID: 1951
		public float ExtraDistanceOutsideSub;

		// Token: 0x040007A0 RID: 1952
		private float _closeEnoughMultiplier = 1f;

		// Token: 0x040007A1 RID: 1953
		private float _closeEnough = 50f;

		// Token: 0x040007A2 RID: 1954
		private readonly float minDistance = 50f;

		// Token: 0x040007A3 RID: 1955
		private readonly float seekGapsInterval = 1f;

		// Token: 0x040007A4 RID: 1956
		private float seekGapsTimer;

		// Token: 0x040007A5 RID: 1957
		private bool cantFindDivingGear;

		// Token: 0x040007AB RID: 1963
		public static readonly Identifier DialogCannotReachTarget = "dialogcannotreachtarget".ToIdentifier();

		// Token: 0x040007AC RID: 1964
		public static readonly Identifier DialogCannotReachPlace = "dialogcannotreachplace".ToIdentifier();

		// Token: 0x040007AD RID: 1965
		public static readonly Identifier DialogCannotReachPatient = "dialogcannotreachpatient".ToIdentifier();

		// Token: 0x040007AE RID: 1966
		public static readonly Identifier DialogCannotReachFire = "dialogcannotreachfire".ToIdentifier();

		// Token: 0x040007AF RID: 1967
		public static readonly Identifier DialogCannotReachLeak = "dialogcannotreachleak".ToIdentifier();

		// Token: 0x040007B1 RID: 1969
		private readonly Identifier ExoSuitRefuel = "dialog.exosuit.refuel".ToIdentifier();

		// Token: 0x040007B2 RID: 1970
		private readonly Identifier ExoSuitOutOfFuel = "dialog.exosuit.outoffuel".ToIdentifier();

		// Token: 0x040007B5 RID: 1973
		public float? OverridePriority;

		// Token: 0x040007B7 RID: 1975
		private readonly float avoidLookAheadDistance = 5f;

		// Token: 0x040007B8 RID: 1976
		private readonly float pathWaitingTime = 3f;

		// Token: 0x040007B9 RID: 1977
		private bool useScooter;

		// Token: 0x040007BA RID: 1978
		private float checkScooterTimer;

		// Token: 0x040007BB RID: 1979
		private const float CheckScooterTime = 0.5f;

		// Token: 0x040007BC RID: 1980
		private float checkExoSuitTimer;

		// Token: 0x040007BD RID: 1981
		private const float CheckExoSuitTime = 2f;
	}
}
