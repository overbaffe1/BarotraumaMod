using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200006E RID: 110
	internal class AIObjectiveExtinguishFire : AIObjective
	{
		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x06000F1D RID: 3869 RVA: 0x0008FC67 File Offset: 0x0008DE67
		// (set) Token: 0x06000F1E RID: 3870 RVA: 0x0008FC6F File Offset: 0x0008DE6F
		public override Identifier Identifier { get; set; } = "extinguish fire".ToIdentifier();

		// Token: 0x17000403 RID: 1027
		// (get) Token: 0x06000F1F RID: 3871 RVA: 0x0008FC78 File Offset: 0x0008DE78
		public override bool ForceRun
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000404 RID: 1028
		// (get) Token: 0x06000F20 RID: 3872 RVA: 0x0008FC7B File Offset: 0x0008DE7B
		protected override bool ConcurrentObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000405 RID: 1029
		// (get) Token: 0x06000F21 RID: 3873 RVA: 0x0008FC7E File Offset: 0x0008DE7E
		public override bool KeepDivingGearOn
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x06000F22 RID: 3874 RVA: 0x0008FC81 File Offset: 0x0008DE81
		protected override bool AllowInAnySub
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x06000F23 RID: 3875 RVA: 0x0008FC84 File Offset: 0x0008DE84
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x0008FC88 File Offset: 0x0008DE88
		public AIObjectiveExtinguishFire(Character character, Hull targetHull, AIObjectiveManager objectiveManager, float priorityModifier = 1f) : base(character, objectiveManager, priorityModifier, default(Identifier))
		{
			this.targetHull = targetHull;
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x0008FCC0 File Offset: 0x0008DEC0
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

		// Token: 0x06000F26 RID: 3878 RVA: 0x0008FEE5 File Offset: 0x0008E0E5
		protected override bool CheckObjectiveState()
		{
			return this.targetHull.FireSources.None(null);
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x0008FEF8 File Offset: 0x0008E0F8
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

		// Token: 0x06000F28 RID: 3880 RVA: 0x00090368 File Offset: 0x0008E568
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

		// Token: 0x06000F29 RID: 3881 RVA: 0x00090399 File Offset: 0x0008E599
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

		// Token: 0x06000F2A RID: 3882 RVA: 0x000903B1 File Offset: 0x0008E5B1
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

		// Token: 0x0400071A RID: 1818
		private readonly Hull targetHull;

		// Token: 0x0400071B RID: 1819
		private AIObjectiveGetItem getExtinguisherObjective;

		// Token: 0x0400071C RID: 1820
		private AIObjectiveGoTo gotoObjective;

		// Token: 0x0400071D RID: 1821
		private float sinTime;
	}
}
