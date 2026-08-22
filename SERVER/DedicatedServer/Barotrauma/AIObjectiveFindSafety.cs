using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000072 RID: 114
	internal class AIObjectiveFindSafety : AIObjective
	{
		// Token: 0x17000418 RID: 1048
		// (get) Token: 0x06000F6A RID: 3946 RVA: 0x00091251 File Offset: 0x0008F451
		// (set) Token: 0x06000F6B RID: 3947 RVA: 0x00091259 File Offset: 0x0008F459
		public override Identifier Identifier { get; set; } = "find safety".ToIdentifier();

		// Token: 0x17000419 RID: 1049
		// (get) Token: 0x06000F6C RID: 3948 RVA: 0x00091262 File Offset: 0x0008F462
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x06000F6D RID: 3949 RVA: 0x00091265 File Offset: 0x0008F465
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x06000F6E RID: 3950 RVA: 0x00091268 File Offset: 0x0008F468
		public override bool IgnoreUnsafeHulls
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06000F6F RID: 3951 RVA: 0x0009126B File Offset: 0x0008F46B
		protected override bool ConcurrentObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x06000F70 RID: 3952 RVA: 0x0009126E File Offset: 0x0008F46E
		protected override bool AllowOutsideSubmarine
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06000F71 RID: 3953 RVA: 0x00091271 File Offset: 0x0008F471
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06000F72 RID: 3954 RVA: 0x00091274 File Offset: 0x0008F474
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x00091278 File Offset: 0x0008F478
		public AIObjectiveFindSafety(Character character, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
		}

		// Token: 0x06000F74 RID: 3956 RVA: 0x000912CB File Offset: 0x0008F4CB
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x06000F75 RID: 3957 RVA: 0x000912CE File Offset: 0x0008F4CE
		public override bool CanBeCompleted
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000F76 RID: 3958 RVA: 0x000912D4 File Offset: 0x0008F4D4
		protected override float GetPriority()
		{
			if (this.character.CurrentHull == null)
			{
				if (!(this.objectiveManager.CurrentOrder is AIObjectiveGoTo) && !this.objectiveManager.HasActiveObjective<AIObjectiveRescue>())
				{
					if (!this.objectiveManager.Objectives.Any(delegate(AIObjective o)
					{
						bool flag = o is AIObjectiveCombat || o is AIObjectiveReturn;
						return flag && o.Priority > 0f;
					}))
					{
						goto IL_8F;
					}
				}
				float priority;
				if ((!this.character.IsLowInOxygen && this.character.IsImmuneToPressure) || HumanAIController.HasDivingSuit(this.character, 0f, true, true))
				{
					priority = 0f;
					goto IL_9B;
				}
				IL_8F:
				priority = 80f;
				IL_9B:
				base.Priority = priority;
			}
			else
			{
				if ((this.character.IsLowInOxygen && !this.character.AnimController.HeadInWater && HumanAIController.HasDivingSuit(this.character, 0f, false, true)) || (!this.objectiveManager.HasActiveObjective<AIObjectiveFindDivingGear>() && AIObjectiveFindSafety.<GetPriority>g__IsSuffocatingWithoutDivingGear|30_1(this.character)))
				{
					base.Priority = 100f;
				}
				else if (this.NeedMoreDivingGear(this.character.CurrentHull, AIObjectiveFindDivingGear.GetMinOxygen(this.character)))
				{
					if (this.objectiveManager.FailedToFindDivingGearForDepth && HumanAIController.HasDivingSuit(this.character, 0f, true, false))
					{
						base.Priority = 0f;
					}
					else
					{
						base.Priority = 100f;
					}
				}
				else
				{
					AIObjectiveGoTo aiobjectiveGoTo = this.objectiveManager.CurrentOrder as AIObjectiveGoTo;
					if (aiobjectiveGoTo != null && aiobjectiveGoTo.IsFollowOrder)
					{
						base.Priority = 0f;
					}
					else if ((this.objectiveManager.IsCurrentOrder<AIObjectiveGoTo>() || this.objectiveManager.IsCurrentOrder<AIObjectiveReturn>()) && this.character.Submarine != null && !this.character.IsOnFriendlyTeam(this.character.Submarine.TeamID))
					{
						base.Priority = 0f;
					}
					else if (this.objectiveManager.Objectives.Any((AIObjective o) => o is AIObjectiveCombat && o.Priority > 0f))
					{
						base.Priority = 0f;
					}
				}
				base.Priority = MathHelper.Clamp(base.Priority, 0f, 100f);
				AIObjectiveFindDivingGear aiobjectiveFindDivingGear = this.divingGearObjective;
				if (aiobjectiveFindDivingGear != null && !aiobjectiveFindDivingGear.IsCompleted && aiobjectiveFindDivingGear.CanBeCompleted && aiobjectiveFindDivingGear.Priority > 0f)
				{
					base.Priority = Math.Max(base.Priority, Math.Min(89f, 100f));
				}
			}
			return base.Priority;
		}

		// Token: 0x06000F77 RID: 3959 RVA: 0x00091568 File Offset: 0x0008F768
		public override void Update(float deltaTime)
		{
			if (this.retryTimer > 0f)
			{
				this.retryTimer -= deltaTime;
				if (this.retryTimer <= 0f)
				{
					this.retryCounter = 0;
				}
			}
			if (this.resetPriority)
			{
				base.Priority = 0f;
				this.resetPriority = false;
				return;
			}
			if (this.character.CurrentHull == null)
			{
				this.currentHullSafety = 0f;
				return;
			}
			this.currentHullSafety = base.HumanAIController.CurrentHullSafety;
			if (this.currentHullSafety > 40f)
			{
				base.Priority -= 10f * deltaTime;
				if (this.currentHullSafety >= 100f && !this.character.IsLowInOxygen)
				{
					base.Priority = 0f;
				}
			}
			else
			{
				float dangerFactor = (100f - this.currentHullSafety) / 100f;
				base.Priority += dangerFactor * 100f * deltaTime;
			}
			base.Priority = MathHelper.Clamp(base.Priority, 0f, 100f);
		}

		// Token: 0x06000F78 RID: 3960 RVA: 0x00091678 File Offset: 0x0008F878
		protected override void Act(float deltaTime)
		{
			if (this.resetPriority)
			{
				return;
			}
			Hull currentHull = this.character.CurrentHull;
			bool dangerousPressure = (currentHull == null || currentHull.LethalPressure > 0f) && !this.character.IsProtectedFromPressure;
			bool shouldActOnSuffocation = this.character.IsLowInOxygen && !this.character.AnimController.HeadInWater && HumanAIController.HasDivingSuit(this.character, 0f, false, true);
			if (!this.character.LockHands && (!dangerousPressure || shouldActOnSuffocation || this.cannotFindSafeHull))
			{
				bool needsDivingSuit;
				bool needsDivingGear = base.HumanAIController.NeedsDivingGear(currentHull, out needsDivingSuit, this.objectiveManager);
				if (this.character.TeamID == CharacterTeamType.FriendlyNPC)
				{
					Submarine submarine = this.character.Submarine;
					SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
					if (submarineInfo != null && submarineInfo.IsOutpost)
					{
						needsDivingSuit = false;
					}
				}
				bool needsEquipment = shouldActOnSuffocation;
				if (needsDivingSuit)
				{
					needsEquipment = !HumanAIController.HasDivingSuit(this.character, AIObjectiveFindDivingGear.GetMinOxygen(this.character), true, true);
				}
				else if (needsDivingGear)
				{
					needsEquipment = !HumanAIController.HasDivingGear(this.character, AIObjectiveFindDivingGear.GetMinOxygen(this.character), true);
				}
				if (needsEquipment)
				{
					if (this.cannotFindDivingGear && this.retryCounter < this.findDivingGearAttempts)
					{
						this.retryTimer = this.retryResetTime;
						this.retryCounter++;
						needsDivingSuit = !needsDivingSuit;
						base.RemoveSubObjective<AIObjectiveFindDivingGear>(ref this.divingGearObjective);
					}
					if (this.divingGearObjective == null)
					{
						this.cannotFindDivingGear = false;
						base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
						base.TryAddSubObjective<AIObjectiveFindDivingGear>(ref this.divingGearObjective, () => new AIObjectiveFindDivingGear(this.character, needsDivingSuit, this.objectiveManager, 1f), delegate
						{
							this.resetPriority = true;
							this.searchHullTimer = Math.Min(1f, this.searchHullTimer);
							this.RemoveSubObjective<AIObjectiveFindDivingGear>(ref this.divingGearObjective);
						}, delegate
						{
							this.searchHullTimer = Math.Min(1f, this.searchHullTimer);
							this.cannotFindDivingGear = true;
						});
					}
				}
			}
			if (this.divingGearObjective == null || !this.divingGearObjective.CanBeCompleted)
			{
				if (this.currentHullSafety < 40f)
				{
					this.searchHullTimer = Math.Min(1f, this.searchHullTimer);
				}
				if (this.searchHullTimer > 0f)
				{
					this.searchHullTimer -= deltaTime;
				}
				else
				{
					Hull potentialSafeHull;
					AIObjectiveFindSafety.HullSearchStatus hullSearchStatus = this.FindBestHull(out potentialSafeHull, null, this.character.TeamID != CharacterTeamType.FriendlyNPC);
					if (hullSearchStatus != AIObjectiveFindSafety.HullSearchStatus.Finished)
					{
						this.UpdateSimpleEscape(deltaTime);
						return;
					}
					this.searchHullTimer = 3f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
					this.previousSafeHull = this.currentSafeHull;
					this.currentSafeHull = potentialSafeHull;
					this.cannotFindSafeHull = (this.currentSafeHull == null || this.NeedMoreDivingGear(this.currentSafeHull, 0f));
					if (this.currentSafeHull == null)
					{
						this.currentSafeHull = this.previousSafeHull;
					}
					if (this.currentSafeHull != null && this.currentSafeHull != currentHull)
					{
						AIObjectiveGoTo aiobjectiveGoTo = this.goToObjective;
						if (((aiobjectiveGoTo != null) ? aiobjectiveGoTo.Target : null) != this.currentSafeHull)
						{
							base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
						}
						base.TryAddSubObjective<AIObjectiveGoTo>(ref this.goToObjective, () => new AIObjectiveGoTo(this.currentSafeHull, this.character, this.objectiveManager, false, true, 1f, 0f)
						{
							SpeakIfFails = false,
							AllowGoingOutside = (this.character.IsProtectedFromPressure || this.character.CurrentHull == null || this.character.CurrentHull.IsAirlock || this.character.CurrentHull.LeadsOutside(this.character))
						}, delegate
						{
							bool needsSuit;
							if (this.currentHullSafety > 40f || (this.HumanAIController.NeedsDivingGear(currentHull, out needsSuit, null) && (needsSuit ? HumanAIController.HasDivingSuit(this.character, 0f, true, true) : HumanAIController.HasDivingMask(this.character, 0f, true))))
							{
								this.resetPriority = true;
								this.searchHullTimer = Math.Min(1f, this.searchHullTimer);
							}
							this.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
							if (this.cannotFindDivingGear)
							{
								this.RemoveSubObjective<AIObjectiveFindDivingGear>(ref this.divingGearObjective);
							}
						}, delegate
						{
							if (this.goToObjective != null)
							{
								Hull hull = this.goToObjective.Target as Hull;
								if (hull != null && (currentHull != null || !Submarine.MainSubs.Contains(hull.Submarine)))
								{
									this.HumanAIController.UnreachableHulls.Add(hull);
								}
							}
							this.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
						});
					}
					else
					{
						base.RemoveSubObjective<AIObjectiveGoTo>(ref this.goToObjective);
					}
				}
				if (this.subObjectives.Any((AIObjective so) => so.CanBeCompleted))
				{
					return;
				}
				this.UpdateSimpleEscape(deltaTime);
				bool inFriendlySub = this.character.IsInFriendlySub || (this.character.IsEscorted && this.character.IsInPlayerSub);
				if (this.cannotFindSafeHull && !inFriendlySub && this.character.IsOnPlayerTeam)
				{
					OrderPrefab orderPrefab;
					if (this.objectiveManager.Objectives.None((AIObjective o) => o is AIObjectiveReturn) && OrderPrefab.Prefabs.TryGet("return".ToIdentifier(), out orderPrefab))
					{
						this.objectiveManager.AddObjective<AIObjectiveReturn>(new AIObjectiveReturn(this.character, this.character, this.objectiveManager, 1f));
					}
				}
			}
		}

		// Token: 0x06000F79 RID: 3961 RVA: 0x00091AE0 File Offset: 0x0008FCE0
		public void UpdateSimpleEscape(float deltaTime)
		{
			Vector2 escapeVel = Vector2.Zero;
			if (this.character.CurrentHull != null)
			{
				foreach (Hull hull in base.HumanAIController.VisibleHulls)
				{
					foreach (FireSource fireSource in hull.FireSources)
					{
						Vector2 dir = this.character.Position - fireSource.Position;
						float distMultiplier = MathHelper.Clamp(100f / Vector2.Distance(fireSource.Position, this.character.Position), 0.1f, 10f);
						escapeVel += new Vector2((float)Math.Sign(dir.X) * distMultiplier, (!this.character.IsClimbing) ? 0f : ((float)Math.Sign(dir.Y) * distMultiplier));
					}
				}
				foreach (Character enemy in Character.CharacterList)
				{
					if (HumanAIController.IsActive(enemy) && !base.HumanAIController.IsFriendly(enemy, false) && !enemy.IsHandcuffed && base.HumanAIController.VisibleHulls.Contains(enemy.CurrentHull))
					{
						Vector2 dir2 = this.character.Position - enemy.Position;
						float distMultiplier2 = MathHelper.Clamp(100f / Vector2.Distance(enemy.Position, this.character.Position), 0.1f, 10f);
						escapeVel += new Vector2((float)Math.Sign(dir2.X) * distMultiplier2, (!this.character.IsClimbing) ? 0f : ((float)Math.Sign(dir2.Y) * distMultiplier2));
					}
				}
			}
			if (escapeVel != Vector2.Zero)
			{
				Hull currentHull = this.character.CurrentHull;
				if (currentHull != null)
				{
					float left = (float)(currentHull.Rect.X + 50);
					float right = (float)(currentHull.Rect.Right - 50);
					if ((escapeVel.X < 0f && this.character.Position.X > left) || (escapeVel.X > 0f && this.character.Position.X < right))
					{
						this.character.ReleaseSecondaryItem();
						this.character.AIController.SteeringManager.SteeringManual(deltaTime, escapeVel);
						return;
					}
					this.character.AnimController.TargetDir = ((escapeVel.X < 0f) ? Direction.Right : Direction.Left);
					this.character.AIController.SteeringManager.Reset();
					return;
				}
			}
			this.objectiveManager.GetObjective<AIObjectiveIdle>().Wander(deltaTime);
		}

		// Token: 0x06000F7A RID: 3962 RVA: 0x00091E18 File Offset: 0x00090018
		public AIObjectiveFindSafety.HullSearchStatus FindBestHull(out Hull bestHull, IEnumerable<Hull> ignoredHulls = null, bool allowChangingSubmarine = true)
		{
			if (this.hullSearchIndex == -1)
			{
				this.bestHullValue = 0f;
				this.potentialBestHull = null;
				this.bestHullIsAirlock = false;
				this.hulls.Clear();
				Submarine submarine = this.character.Submarine;
				IEnumerable<Submarine> connectedSubs = (submarine != null) ? submarine.GetConnectedSubs() : null;
				foreach (Hull hull in Hull.HullList)
				{
					if (hull.Submarine != null && !hull.Submarine.Info.IsRuin && (allowChangingSubmarine || hull.Submarine == this.character.Submarine) && (float)hull.Rect.Height >= ConvertUnits.ToDisplayUnits(this.character.AnimController.ColliderHeightFromFloor) * 2f && (ignoredHulls == null || !ignoredHulls.Contains(hull)) && !base.HumanAIController.UnreachableHulls.Contains(hull) && (connectedSubs == null || connectedSubs.Contains(hull.Submarine)))
					{
						if (this.hulls.None(null))
						{
							this.hulls.Add(hull);
						}
						else
						{
							bool addLast = true;
							float hullSuitability = this.<FindBestHull>g__EstimateHullSuitability|48_0(hull);
							for (int i = 0; i < this.hulls.Count; i++)
							{
								Hull otherHull = this.hulls[i];
								float otherHullSuitability = this.<FindBestHull>g__EstimateHullSuitability|48_0(otherHull);
								if (hullSuitability > otherHullSuitability)
								{
									this.hulls.Insert(i, hull);
									addLast = false;
									break;
								}
							}
							if (addLast)
							{
								this.hulls.Add(hull);
							}
						}
					}
				}
				if (this.hulls.None(null))
				{
					bestHull = null;
					return AIObjectiveFindSafety.HullSearchStatus.Finished;
				}
				this.hullSearchIndex = 0;
			}
			Hull potentialHull = this.hulls[this.hullSearchIndex];
			float hullSafety = 0f;
			bool hullIsAirlock = false;
			bool isCharacterInside = this.character.CurrentHull != null && this.character.Submarine != null;
			if (isCharacterInside)
			{
				hullSafety = HumanAIController.GetHullSafety(potentialHull, potentialHull.GetConnectedHulls(true, new int?(1), false), this.character, false, false, false, false);
				float distanceFactor = base.GetDistanceFactor(potentialHull.WorldPosition, 0.9f, 3f, 10000f, 1f);
				hullSafety *= distanceFactor;
				if (hullSafety > this.bestHullValue)
				{
					if (!allowChangingSubmarine)
					{
						if (!potentialHull.OutpostModuleTags.All((Identifier t) => t != "airlock"))
						{
							hullSafety = 0f;
							goto IL_4E8;
						}
					}
					SteeringPath path = base.PathSteering.PathFinder.FindPath(this.character.SimPosition, this.character.GetRelativeSimPosition(potentialHull, null), this.character.Submarine, null, 0f, null, null, (PathNode node) => node.Waypoint.CurrentHull != null, true, 0f);
					if (path.Unreachable)
					{
						hullSafety = 0f;
						base.HumanAIController.UnreachableHulls.Add(potentialHull);
					}
					else
					{
						Hull previousHull = null;
						foreach (WayPoint node2 in path.Nodes)
						{
							Hull hull2 = node2.CurrentHull;
							if (hull2 != previousHull)
							{
								previousHull = hull2;
								if (hull2 != this.character.CurrentHull && base.HumanAIController.UnsafeHulls.Contains(hull2))
								{
									float nodeHullSafety = HumanAIController.GetHullSafety(hull2, hull2.GetConnectedHulls(true, new int?(1), false), this.character, false, false, false, false);
									if (nodeHullSafety < 40f && nodeHullSafety < base.HumanAIController.CurrentHullSafety)
									{
										hullSafety = 0f;
										break;
									}
									float hullThreat = 100f - nodeHullSafety;
									hullSafety -= hullThreat / 2f;
									if (hullSafety <= 0f)
									{
										break;
									}
								}
							}
						}
						if (!this.character.Submarine.IsEntityFoundOnThisSub(potentialHull, true, false, false))
						{
							hullSafety /= 10f;
						}
					}
				}
			}
			else
			{
				if (potentialHull.IsAirlock)
				{
					hullSafety = 100f;
					hullIsAirlock = true;
				}
				else if (!this.bestHullIsAirlock && potentialHull.LeadsOutside(this.character))
				{
					hullSafety = 100f;
				}
				Hull currentHull = this.character.CurrentHull;
				float characterY = (currentHull != null) ? currentHull.WorldPosition.Y : this.character.WorldPosition.Y;
				float distanceFactor2 = AIObjective.GetDistanceFactor(new Vector2(this.character.WorldPosition.X, characterY), potentialHull.WorldPosition, 0.2f, 3f, 10000f, 1f);
				hullSafety *= distanceFactor2;
				if (potentialHull.Submarine.TeamID != this.character.TeamID && potentialHull.Submarine.TeamID != CharacterTeamType.FriendlyNPC)
				{
					hullSafety /= 10f;
				}
			}
			IL_4E8:
			if (hullSafety > this.bestHullValue || (!isCharacterInside && hullIsAirlock && !this.bestHullIsAirlock))
			{
				this.potentialBestHull = potentialHull;
				this.bestHullValue = hullSafety;
				this.bestHullIsAirlock = hullIsAirlock;
			}
			bestHull = this.potentialBestHull;
			this.hullSearchIndex++;
			if (this.hullSearchIndex >= this.hulls.Count)
			{
				this.hullSearchIndex = -1;
				return AIObjectiveFindSafety.HullSearchStatus.Finished;
			}
			return AIObjectiveFindSafety.HullSearchStatus.Running;
		}

		// Token: 0x06000F7B RID: 3963 RVA: 0x000923A4 File Offset: 0x000905A4
		public override void Reset()
		{
			base.Reset();
			this.goToObjective = null;
			this.divingGearObjective = null;
			this.currentSafeHull = null;
			this.previousSafeHull = null;
			this.retryCounter = 0;
			this.cannotFindDivingGear = false;
			this.cannotFindSafeHull = false;
		}

		// Token: 0x06000F7C RID: 3964 RVA: 0x000923E0 File Offset: 0x000905E0
		private bool NeedMoreDivingGear(Hull targetHull, float minOxygen = 0f)
		{
			bool needsSuit;
			if (!base.HumanAIController.NeedsDivingGear(targetHull, out needsSuit, null))
			{
				return false;
			}
			if (needsSuit)
			{
				return !HumanAIController.HasDivingSuit(this.character, minOxygen, true, true);
			}
			return !HumanAIController.HasDivingGear(this.character, minOxygen, true);
		}

		// Token: 0x06000F7D RID: 3965 RVA: 0x00092425 File Offset: 0x00090625
		[CompilerGenerated]
		internal static bool <GetPriority>g__IsSuffocatingWithoutDivingGear|30_1(Character c)
		{
			return c.IsLowInOxygen && c.AnimController.HeadInWater && !HumanAIController.HasDivingGear(c, 0f, true);
		}

		// Token: 0x06000F7E RID: 3966 RVA: 0x00092450 File Offset: 0x00090650
		[CompilerGenerated]
		private float <FindBestHull>g__EstimateHullSuitability|48_0(Hull h)
		{
			float distX = Math.Abs(h.WorldPosition.X - this.character.WorldPosition.X);
			float distY = Math.Abs(h.WorldPosition.Y - this.character.WorldPosition.Y);
			if (this.character.CurrentHull != null)
			{
				distY *= 3f;
			}
			float dist = distX + distY;
			float suitability = -dist;
			if (h.Submarine != this.character.Submarine)
			{
				suitability -= 10000f;
			}
			if (this.character.CurrentHull != null)
			{
				if (h.AvoidStaying)
				{
					suitability -= 10000f;
				}
				if (base.HumanAIController.UnsafeHulls.Contains(h))
				{
					suitability -= 10000f;
				}
				bool flag;
				if (base.HumanAIController.NeedsDivingGear(h, out flag, null))
				{
					suitability -= 10000f;
				}
			}
			return suitability;
		}

		// Token: 0x04000729 RID: 1833
		private const float PriorityIncrease = 100f;

		// Token: 0x0400072A RID: 1834
		private const float PriorityDecrease = 10f;

		// Token: 0x0400072B RID: 1835
		private const float SearchHullInterval = 3f;

		// Token: 0x0400072C RID: 1836
		private float currentHullSafety;

		// Token: 0x0400072D RID: 1837
		private float searchHullTimer;

		// Token: 0x0400072E RID: 1838
		private AIObjectiveGoTo goToObjective;

		// Token: 0x0400072F RID: 1839
		private AIObjectiveFindDivingGear divingGearObjective;

		// Token: 0x04000730 RID: 1840
		private bool resetPriority;

		// Token: 0x04000731 RID: 1841
		private Hull currentSafeHull;

		// Token: 0x04000732 RID: 1842
		private Hull previousSafeHull;

		// Token: 0x04000733 RID: 1843
		private bool cannotFindSafeHull;

		// Token: 0x04000734 RID: 1844
		private bool cannotFindDivingGear;

		// Token: 0x04000735 RID: 1845
		private readonly int findDivingGearAttempts = 2;

		// Token: 0x04000736 RID: 1846
		private int retryCounter;

		// Token: 0x04000737 RID: 1847
		private readonly float retryResetTime = 5f;

		// Token: 0x04000738 RID: 1848
		private float retryTimer;

		// Token: 0x04000739 RID: 1849
		private readonly List<Hull> hulls = new List<Hull>();

		// Token: 0x0400073A RID: 1850
		private int hullSearchIndex = -1;

		// Token: 0x0400073B RID: 1851
		private float bestHullValue;

		// Token: 0x0400073C RID: 1852
		private bool bestHullIsAirlock;

		// Token: 0x0400073D RID: 1853
		private Hull potentialBestHull;

		// Token: 0x020007CD RID: 1997
		public enum HullSearchStatus
		{
			// Token: 0x04002DFD RID: 11773
			Running,
			// Token: 0x04002DFE RID: 11774
			Finished
		}
	}
}
