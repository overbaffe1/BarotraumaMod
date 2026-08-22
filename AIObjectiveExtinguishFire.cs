using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000174 RID: 372
	internal class AIObjectiveExtinguishFire : AIObjective
	{
		// Token: 0x17000B1C RID: 2844
		// (get) Token: 0x06002C25 RID: 11301 RVA: 0x001E2AFB File Offset: 0x001E0CFB
		// (set) Token: 0x06002C26 RID: 11302 RVA: 0x001E2B03 File Offset: 0x001E0D03
		public override Identifier Identifier { get; set; } = "extinguish fire".ToIdentifier();

		// Token: 0x17000B1D RID: 2845
		// (get) Token: 0x06002C27 RID: 11303 RVA: 0x001E2B0C File Offset: 0x001E0D0C
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B1E RID: 2846
		// (get) Token: 0x06002C28 RID: 11304 RVA: 0x001E2B0F File Offset: 0x001E0D0F
		protected override bool ConcurrentObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B1F RID: 2847
		// (get) Token: 0x06002C29 RID: 11305 RVA: 0x001E2B12 File Offset: 0x001E0D12
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B20 RID: 2848
		// (get) Token: 0x06002C2A RID: 11306 RVA: 0x001E2B15 File Offset: 0x001E0D15
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000B21 RID: 2849
		// (get) Token: 0x06002C2B RID: 11307 RVA: 0x001E2B18 File Offset: 0x001E0D18
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002C2C RID: 11308 RVA: 0x001E2B1C File Offset: 0x001E0D1C
		public AIObjectiveExtinguishFire(Character character, Hull targetHull, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.targetHull = targetHull;
		}

		// Token: 0x06002C2D RID: 11309 RVA: 0x001E2B54 File Offset: 0x001E0D54
		protected override float GetPriority()
		{
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
				return base.Priority;
			}
			bool isOrder = this.objectiveManager.HasOrder<AIObjectiveExtinguishFires>(null);
			if (!isOrder && Character.CharacterList.Any((Character c) => c.CurrentHull == this.targetHull && !base.HumanAIController.IsFriendly(c, false) && HumanAIController.IsActive(c)))
			{
				base.Priority = 0f;
				base.Abandon = true;
				return base.Priority;
			}
			bool inDamageRange = this.targetHull.FireSources.Any((FireSource fs) => fs.IsInDamageRange(this.character, fs.DamageRange));
			float severity = inDamageRange ? 1f : AIObjectiveExtinguishFires.GetFireSeverity(this.targetHull);
			Hull currentHull = this.character.CurrentHull;
			float characterY = (currentHull != null) ? currentHull.WorldPosition.Y : this.character.WorldPosition.Y;
			float distanceFactor = (this.targetHull == this.character.CurrentHull) ? 1f : (base.HumanAIController.VisibleHulls.Contains(this.targetHull) ? 0.75f : 0f);
			if (distanceFactor <= 0f)
			{
				distanceFactor = AIObjective.GetDistanceFactor(new Vector2(this.character.WorldPosition.Y, characterY), this.targetHull.WorldPosition, 0.1f, 3f, 5000f, 1f);
			}
			if (!inDamageRange && severity > 0.75f && distanceFactor < 0.75f && !isOrder && this.character.IsOnPlayerTeam && this.targetHull.RoomName != null && !this.targetHull.RoomName.Contains("reactor", StringComparison.OrdinalIgnoreCase) && !this.targetHull.RoomName.Contains("engine", StringComparison.OrdinalIgnoreCase) && !this.targetHull.RoomName.Contains("command", StringComparison.OrdinalIgnoreCase))
			{
				base.Priority = 0f;
				base.Abandon = true;
				return base.Priority;
			}
			float devotion = base.CumulatedDevotion / 100f;
			base.Priority = MathHelper.Lerp(0f, 100f, MathHelper.Clamp(devotion + severity * distanceFactor * base.PriorityModifier, 0f, 1f));
			return base.Priority;
		}

		// Token: 0x06002C2E RID: 11310 RVA: 0x001E2D79 File Offset: 0x001E0F79
		protected override bool CheckObjectiveState()
		{
			return this.targetHull.FireSources.None(null);
		}

		// Token: 0x06002C2F RID: 11311 RVA: 0x001E2D8C File Offset: 0x001E0F8C
		protected override void Act(float deltaTime)
		{
			Item extinguisherItem = this.character.Inventory.FindItemByTag(Tags.FireExtinguisher, false);
			if (extinguisherItem == null || extinguisherItem.Condition <= 0f || !this.character.HasEquippedItem(extinguisherItem, null, null))
			{
				base.TryAddSubObjective<AIObjectiveGetItem>(ref this.getExtinguisherObjective, delegate
				{
					if (this.character.IsOnPlayerTeam && !this.character.HasEquippedItem(Tags.FireExtinguisher, false, null))
					{
						this.character.Speak(TextManager.Get("DialogFindExtinguisher").Value, null, 2f, Tags.FireExtinguisher, 30f);
					}
					AIObjectiveGetItem getItemObjective = new AIObjectiveGetItem(this.character, Tags.FireExtinguisher, this.objectiveManager, true, true, 1f, false)
					{
						AllowStealing = true,
						GetItemPriority = delegate(Item i)
						{
							if (!base.HumanAIController.UnsafeHulls.Contains(i.CurrentHull))
							{
								return 1f;
							}
							return 0.1f;
						}
					};
					if (this.objectiveManager.HasOrder<AIObjectiveExtinguishFires>(null))
					{
						getItemObjective.Abandoned += delegate()
						{
							this.character.Speak(TextManager.Get("dialogcannotfindfireextinguisher").Value, null, 0f, "dialogcannotfindfireextinguisher".ToIdentifier(), 10f);
						};
					}
					return getItemObjective;
				}, null, null);
				return;
			}
			RepairTool extinguisher = extinguisherItem.GetComponent<RepairTool>();
			if (extinguisher == null)
			{
				base.Abandon = true;
				return;
			}
			using (List<FireSource>.Enumerator enumerator = this.targetHull.FireSources.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					FireSource fs = enumerator.Current;
					if (fs != null && !fs.Removed)
					{
						if (this.character.CurrentHull == null)
						{
							base.Abandon = true;
							break;
						}
						float xDist = Math.Abs(this.character.WorldPosition.X - fs.WorldPosition.X);
						float yDist = (!this.character.IsClimbing && MathUtils.NearlyEqual(this.character.CurrentHull.WorldPosition.Y, this.targetHull.WorldPosition.Y, 0.0001f)) ? 0f : Math.Abs(this.character.CurrentHull.WorldPosition.Y - fs.WorldPosition.Y);
						float dist = xDist + yDist;
						bool inRange = dist < extinguisher.Range;
						bool isInDamageRange = fs.IsInDamageRange(this.character, fs.DamageRange) && this.character.CanSeeTarget(this.targetHull, null, false, false);
						bool moveCloser = !isInDamageRange && (!inRange || !this.character.CanSeeTarget(this.targetHull, null, false, false));
						bool operateExtinguisher = !moveCloser || (dist < extinguisher.Range * 1.2f && this.character.CanSeeTarget(this.targetHull, null, false, false));
						if (operateExtinguisher)
						{
							this.character.CursorPosition = ConvertUnits.ToDisplayUnits(Submarine.GetRelativeSimPositionFromWorldPosition(fs.WorldPosition, this.character.Submarine, fs.Submarine));
							Vector2 fromCharacterToFireSource = fs.WorldPosition - this.character.WorldPosition;
							this.character.CursorPosition += VectorExtensions.Forward(extinguisherItem.body.TransformedRotation + (float)Math.Sin((double)this.sinTime) / 2f, fromCharacterToFireSource.Length() / 2f);
							if (extinguisherItem.RequireAimToUse)
							{
								this.character.SetInput(InputType.Aim, false, true);
								this.sinTime += deltaTime * 10f;
							}
							this.character.SetInput(extinguisherItem.IsShootable ? InputType.Shoot : InputType.Use, false, true);
							extinguisher.Use(deltaTime, this.character);
							if (!this.targetHull.FireSources.Contains(fs))
							{
								this.character.Speak(TextManager.GetWithVariable("DialogPutOutFire", "[roomname]", this.targetHull.DisplayName, FormatCapitals.Yes).Value, null, 0f, "putoutfire".ToIdentifier(), 10f);
							}
							this.objectiveManager.CurrentObjective.ForceWalkTemporarily = true;
						}
						if (moveCloser)
						{
							if (base.TryAddSubObjective<AIObjectiveGoTo>(ref this.gotoObjective, () => new AIObjectiveGoTo(fs, this.character, this.objectiveManager, false, true, 1f, extinguisher.Range * 0.8f)
							{
								DialogueIdentifier = AIObjectiveGoTo.DialogCannotReachFire,
								TargetName = fs.Hull.DisplayName
							}, delegate
							{
								base.RemoveSubObjective<AIObjectiveGoTo>(ref this.gotoObjective);
							}, delegate
							{
								base.Abandon = true;
							}))
							{
								this.gotoObjective.requiredCondition = (() => this.character.CanSeeTarget(this.targetHull, null, false, false));
								break;
							}
							break;
						}
						else
						{
							if (!operateExtinguisher || isInDamageRange)
							{
								base.RemoveSubObjective<AIObjectiveGoTo>(ref this.gotoObjective);
								base.SteeringManager.Reset();
								break;
							}
							break;
						}
					}
				}
			}
		}

		// Token: 0x06002C30 RID: 11312 RVA: 0x001E31FC File Offset: 0x001E13FC
		public override void Reset()
		{
			base.Reset();
			this.getExtinguisherObjective = null;
			this.gotoObjective = null;
			this.sinTime = 0f;
			SteeringManager steeringManager = base.SteeringManager;
			if (steeringManager == null)
			{
				return;
			}
			steeringManager.Reset();
		}

		// Token: 0x06002C31 RID: 11313 RVA: 0x001E322D File Offset: 0x001E142D
		protected override void OnCompleted()
		{
			base.OnCompleted();
			SteeringManager steeringManager = base.SteeringManager;
			if (steeringManager == null)
			{
				return;
			}
			steeringManager.Reset();
		}

		// Token: 0x06002C32 RID: 11314 RVA: 0x001E3245 File Offset: 0x001E1445
		protected override void OnAbandon()
		{
			base.OnAbandon();
			SteeringManager steeringManager = base.SteeringManager;
			if (steeringManager == null)
			{
				return;
			}
			steeringManager.Reset();
		}

		// Token: 0x04001714 RID: 5908
		private readonly Hull targetHull;

		// Token: 0x04001715 RID: 5909
		private AIObjectiveGetItem getExtinguisherObjective;

		// Token: 0x04001716 RID: 5910
		private AIObjectiveGoTo gotoObjective;

		// Token: 0x04001717 RID: 5911
		private float sinTime;
	}
}
