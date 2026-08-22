using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000064 RID: 100
	internal abstract class AIObjective
	{
		// Token: 0x1700039D RID: 925
		// (get) Token: 0x06000DD1 RID: 3537 RVA: 0x0008915C File Offset: 0x0008735C
		public virtual float Devotion
		{
			get
			{
				return 5f;
			}
		}

		// Token: 0x1700039E RID: 926
		// (get) Token: 0x06000DD2 RID: 3538
		// (set) Token: 0x06000DD3 RID: 3539
		public abstract Identifier Identifier { get; set; }

		// Token: 0x1700039F RID: 927
		// (get) Token: 0x06000DD4 RID: 3540 RVA: 0x00089164 File Offset: 0x00087364
		public virtual string DebugTag
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x06000DD5 RID: 3541 RVA: 0x0008917F File Offset: 0x0008737F
		public virtual bool ForceRun
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x06000DD6 RID: 3542 RVA: 0x00089182 File Offset: 0x00087382
		public virtual bool IgnoreUnsafeHulls
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x06000DD7 RID: 3543 RVA: 0x00089185 File Offset: 0x00087385
		public virtual bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x06000DD8 RID: 3544 RVA: 0x00089188 File Offset: 0x00087388
		public virtual bool AllowSubObjectiveSorting
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x06000DD9 RID: 3545 RVA: 0x0008918B File Offset: 0x0008738B
		public virtual bool PrioritizeIfSubObjectivesActive
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x06000DDA RID: 3546 RVA: 0x0008918E File Offset: 0x0008738E
		public virtual bool AllowMultipleInstances
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x06000DDB RID: 3547 RVA: 0x00089191 File Offset: 0x00087391
		protected virtual bool ConcurrentObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06000DDC RID: 3548 RVA: 0x00089194 File Offset: 0x00087394
		public virtual bool KeepDivingGearOn
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x06000DDD RID: 3549 RVA: 0x00089197 File Offset: 0x00087397
		public virtual bool KeepDivingGearOnAlsoWhenInactive
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x06000DDE RID: 3550 RVA: 0x0008919A File Offset: 0x0008739A
		public virtual bool AllowAutomaticItemUnequipping
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x06000DDF RID: 3551 RVA: 0x0008919D File Offset: 0x0008739D
		protected virtual bool AllowOutsideSubmarine
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x06000DE0 RID: 3552 RVA: 0x000891A0 File Offset: 0x000873A0
		protected virtual bool AllowInFriendlySubs
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x06000DE1 RID: 3553 RVA: 0x000891A3 File Offset: 0x000873A3
		protected virtual bool AllowInAnySub
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x06000DE2 RID: 3554 RVA: 0x000891A6 File Offset: 0x000873A6
		protected virtual bool AllowWhileHandcuffed
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x06000DE3 RID: 3555 RVA: 0x000891A9 File Offset: 0x000873A9
		protected virtual bool AbandonIfDisallowed
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x06000DE4 RID: 3556 RVA: 0x000891AC File Offset: 0x000873AC
		public virtual bool CanBeCompleted
		{
			get
			{
				return !this.Abandon;
			}
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x06000DE5 RID: 3557 RVA: 0x000891B7 File Offset: 0x000873B7
		protected virtual float MaxDevotion
		{
			get
			{
				return 10f;
			}
		}

		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x06000DE6 RID: 3558 RVA: 0x000891BE File Offset: 0x000873BE
		// (set) Token: 0x06000DE7 RID: 3559 RVA: 0x000891C6 File Offset: 0x000873C6
		protected float CumulatedDevotion
		{
			get
			{
				return this._cumulatedDevotion;
			}
			set
			{
				this._cumulatedDevotion = MathHelper.Clamp(value, 0f, this.MaxDevotion);
			}
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x06000DE8 RID: 3560 RVA: 0x000891DF File Offset: 0x000873DF
		// (set) Token: 0x06000DE9 RID: 3561 RVA: 0x000891E7 File Offset: 0x000873E7
		public float Priority { get; set; }

		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x06000DEA RID: 3562 RVA: 0x000891F0 File Offset: 0x000873F0
		// (set) Token: 0x06000DEB RID: 3563 RVA: 0x000891F8 File Offset: 0x000873F8
		public float BasePriority { get; set; }

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x06000DEC RID: 3564 RVA: 0x00089201 File Offset: 0x00087401
		// (set) Token: 0x06000DED RID: 3565 RVA: 0x00089209 File Offset: 0x00087409
		public float PriorityModifier { get; private set; } = 1f;

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x06000DEE RID: 3566 RVA: 0x00089212 File Offset: 0x00087412
		// (set) Token: 0x06000DEF RID: 3567 RVA: 0x0008921A File Offset: 0x0008741A
		public bool ForceHighestPriority
		{
			get
			{
				return this._forceHighestPriority;
			}
			set
			{
				if (this._forceHighestPriority == value)
				{
					return;
				}
				this._forceHighestPriority = value;
				if (this._forceHighestPriority)
				{
					this.resetPriorityTimer = this.resetPriorityTime;
				}
			}
		}

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x06000DF0 RID: 3568 RVA: 0x00089241 File Offset: 0x00087441
		// (set) Token: 0x06000DF1 RID: 3569 RVA: 0x00089249 File Offset: 0x00087449
		public bool ForceWalkTemporarily { get; set; }

		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x06000DF2 RID: 3570 RVA: 0x00089252 File Offset: 0x00087452
		// (set) Token: 0x06000DF3 RID: 3571 RVA: 0x0008925A File Offset: 0x0008745A
		public bool ForceWalkPermanently { get; set; }

		// Token: 0x170003B8 RID: 952
		// (get) Token: 0x06000DF4 RID: 3572 RVA: 0x00089263 File Offset: 0x00087463
		public bool ForceWalk
		{
			get
			{
				return this.ForceWalkTemporarily || this.ForceWalkPermanently;
			}
		}

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x06000DF5 RID: 3573 RVA: 0x00089275 File Offset: 0x00087475
		// (set) Token: 0x06000DF6 RID: 3574 RVA: 0x0008927D File Offset: 0x0008747D
		public bool IgnoreAtOutpost { get; set; }

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x06000DF7 RID: 3575 RVA: 0x00089286 File Offset: 0x00087486
		// (set) Token: 0x06000DF8 RID: 3576 RVA: 0x0008928E File Offset: 0x0008748E
		public bool Abandon
		{
			get
			{
				return this._abandon;
			}
			set
			{
				this._abandon = value;
				if (this._abandon)
				{
					this.OnAbandon();
				}
			}
		}

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x06000DF9 RID: 3577 RVA: 0x000892A5 File Offset: 0x000874A5
		public IEnumerable<AIObjective> SubObjectives
		{
			get
			{
				return this.subObjectives;
			}
		}

		// Token: 0x170003BC RID: 956
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x000892AD File Offset: 0x000874AD
		public AIObjective CurrentSubObjective
		{
			get
			{
				return this.subObjectives.FirstOrDefault<AIObjective>();
			}
		}

		// Token: 0x06000DFB RID: 3579 RVA: 0x000892BC File Offset: 0x000874BC
		public IEnumerable<AIObjective> GetSubObjectivesRecursive(bool includingSelf = false)
		{
			this.all.Clear();
			if (includingSelf)
			{
				this.all.Add(this);
			}
			foreach (AIObjective subObjective in this.subObjectives)
			{
				this.all.AddRange(subObjective.GetSubObjectivesRecursive(true));
			}
			return this.all;
		}

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000DFC RID: 3580 RVA: 0x0008933C File Offset: 0x0008753C
		// (remove) Token: 0x06000DFD RID: 3581 RVA: 0x00089374 File Offset: 0x00087574
		public event Action Completed;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000DFE RID: 3582 RVA: 0x000893AC File Offset: 0x000875AC
		// (remove) Token: 0x06000DFF RID: 3583 RVA: 0x000893E4 File Offset: 0x000875E4
		public event Action Abandoned;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000E00 RID: 3584 RVA: 0x0008941C File Offset: 0x0008761C
		// (remove) Token: 0x06000E01 RID: 3585 RVA: 0x00089454 File Offset: 0x00087654
		public event Action Selected;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000E02 RID: 3586 RVA: 0x0008948C File Offset: 0x0008768C
		// (remove) Token: 0x06000E03 RID: 3587 RVA: 0x000894C4 File Offset: 0x000876C4
		public event Action Deselected;

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x06000E04 RID: 3588 RVA: 0x000894F9 File Offset: 0x000876F9
		protected HumanAIController HumanAIController
		{
			get
			{
				return this.character.AIController as HumanAIController;
			}
		}

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x06000E05 RID: 3589 RVA: 0x0008950B File Offset: 0x0008770B
		protected IndoorsSteeringManager PathSteering
		{
			get
			{
				return this.HumanAIController.PathSteering;
			}
		}

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x06000E06 RID: 3590 RVA: 0x00089518 File Offset: 0x00087718
		protected SteeringManager SteeringManager
		{
			get
			{
				return this.HumanAIController.SteeringManager;
			}
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x00089528 File Offset: 0x00087728
		public AIObjective GetActiveObjective()
		{
			AIObjective subObjective = this.CurrentSubObjective;
			if (subObjective != null)
			{
				return subObjective.GetActiveObjective();
			}
			return this;
		}

		// Token: 0x06000E08 RID: 3592 RVA: 0x00089548 File Offset: 0x00087748
		public AIObjective(Character character, AIObjectiveManager objectiveManager, float priorityModifier, Identifier option = default(Identifier))
		{
			this.objectiveManager = objectiveManager;
			this.character = character;
			this.Option = option;
			this.PriorityModifier = priorityModifier;
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x000895A4 File Offset: 0x000877A4
		public void TryComplete(float deltaTime)
		{
			if (this.isCompleted)
			{
				return;
			}
			if (this.CheckState())
			{
				return;
			}
			for (int i = 0; i < this.subObjectives.Count; i++)
			{
				this.subObjectives[i].TryComplete(deltaTime);
				if (!this.ConcurrentObjectives)
				{
					return;
				}
			}
			this.Act(deltaTime);
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x000895FC File Offset: 0x000877FC
		public void AddSubObjective(AIObjective objective, bool addFirst = false)
		{
			Type type = objective.GetType();
			objective.SourceObjective = this;
			this.subObjectives.RemoveAll((AIObjective o) => o.GetType() == type);
			if (addFirst)
			{
				this.subObjectives.Insert(0, objective);
				return;
			}
			this.subObjectives.Add(objective);
		}

		// Token: 0x06000E0B RID: 3595 RVA: 0x00089658 File Offset: 0x00087858
		public void RemoveSubObjective<T>(ref T objective) where T : AIObjective
		{
			if (objective != null)
			{
				if (this.subObjectives.Contains(objective))
				{
					this.subObjectives.Remove(objective);
				}
				objective = default(T);
			}
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x000896A8 File Offset: 0x000878A8
		public void SortSubObjectives()
		{
			if (!this.AllowSubObjectiveSorting)
			{
				return;
			}
			if (this.subObjectives.None(null))
			{
				return;
			}
			AIObjective previousSubObjective = this.subObjectives.First<AIObjective>();
			this.subObjectives.ForEach(delegate(AIObjective so)
			{
				so.GetPriority();
			});
			this.subObjectives.Sort((AIObjective x, AIObjective y) => y.Priority.CompareTo(x.Priority));
			if (this.ConcurrentObjectives)
			{
				this.subObjectives.ForEach(delegate(AIObjective so)
				{
					so.SortSubObjectives();
				});
				return;
			}
			AIObjective currentSubObjective = this.subObjectives.First<AIObjective>();
			if (previousSubObjective != currentSubObjective)
			{
				previousSubObjective.OnDeselected();
				currentSubObjective.OnSelected();
			}
			currentSubObjective.SortSubObjectives();
		}

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x06000E0D RID: 3597 RVA: 0x00089784 File Offset: 0x00087984
		public bool IsAllowed
		{
			get
			{
				return (this.AllowWhileHandcuffed || !this.character.LockHands) && (this.AllowOutsideSubmarine || this.character.Submarine != null) && !this.IsIgnoredAtOutpost() && (this.AllowInAnySub || ((this.AllowInFriendlySubs && this.character.Submarine.TeamID == CharacterTeamType.FriendlyNPC) || this.character.IsEscorted) || this.character.Submarine.TeamID == this.character.TeamID || this.character.Submarine.TeamID == this.character.OriginalTeamID);
			}
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x0008983C File Offset: 0x00087A3C
		public bool IsIgnoredAtOutpost()
		{
			if (!this.IgnoreAtOutpost)
			{
				return false;
			}
			if (!Level.IsLoadedFriendlyOutpost && !(GameMain.GameSession.GameMode is TestGameMode))
			{
				return false;
			}
			if (!this.character.IsOnPlayerTeam || this.character.IsFriendlyNPCTurnedHostile)
			{
				return false;
			}
			Submarine submarine = this.character.Submarine;
			return ((submarine != null) ? submarine.Info : null) != null && this.character.Submarine.Info.IsOutpost && this.character.Submarine.TeamID == CharacterTeamType.FriendlyNPC;
		}

		// Token: 0x06000E0F RID: 3599 RVA: 0x000898D0 File Offset: 0x00087AD0
		protected void HandleDisallowed()
		{
			this.Priority = 0f;
			if (this.AbandonIfDisallowed && !this.IsIgnoredAtOutpost())
			{
				this.Abandon = true;
			}
		}

		// Token: 0x06000E10 RID: 3600 RVA: 0x000898F4 File Offset: 0x00087AF4
		protected virtual float GetPriority()
		{
			if (!this.IsAllowed)
			{
				this.HandleDisallowed();
				return this.Priority;
			}
			if (this.objectiveManager.IsOrder(this))
			{
				this.Priority = this.objectiveManager.GetOrderPriority(this);
			}
			else
			{
				this.Priority = this.BasePriority + this.CumulatedDevotion;
			}
			return this.Priority;
		}

		// Token: 0x06000E11 RID: 3601 RVA: 0x00089951 File Offset: 0x00087B51
		public float CalculatePriority()
		{
			this.ForceWalkTemporarily = false;
			this.Priority = this.GetPriority();
			this.ForceHighestPriority = false;
			return this.Priority;
		}

		// Token: 0x06000E12 RID: 3602 RVA: 0x00089974 File Offset: 0x00087B74
		public static float GetDistanceFactor(Vector2 selfPos, Vector2 targetWorldPos, float factorAtMaxDistance, float verticalDistanceMultiplier = 3f, float maxDistance = 10000f, float factorAtMinDistance = 1f)
		{
			float yDist = Math.Abs(selfPos.Y - targetWorldPos.Y);
			yDist = ((yDist > 100f) ? (yDist * verticalDistanceMultiplier) : 0f);
			float distance = Math.Abs(selfPos.X - targetWorldPos.X) + yDist;
			float distanceFactor = MathHelper.Lerp(factorAtMinDistance, factorAtMaxDistance, MathUtils.InverseLerp(0f, maxDistance, distance));
			if (factorAtMinDistance <= factorAtMaxDistance)
			{
				return MathHelper.Clamp(distanceFactor, factorAtMinDistance, factorAtMaxDistance);
			}
			return MathHelper.Clamp(distanceFactor, factorAtMaxDistance, factorAtMinDistance);
		}

		// Token: 0x06000E13 RID: 3603 RVA: 0x000899EA File Offset: 0x00087BEA
		protected float GetDistanceFactor(Vector2 targetWorldPos, float factorAtMaxDistance, float verticalDistanceMultiplier = 3f, float maxDistance = 10000f, float factorAtMinDistance = 1f)
		{
			return AIObjective.GetDistanceFactor(this.character.WorldPosition, targetWorldPos, factorAtMaxDistance, verticalDistanceMultiplier, maxDistance, factorAtMinDistance);
		}

		// Token: 0x06000E14 RID: 3604 RVA: 0x00089A04 File Offset: 0x00087C04
		private void UpdateDevotion(float deltaTime)
		{
			AIObjective currentObjective = this.objectiveManager.CurrentObjective;
			if (currentObjective != null && (currentObjective == this || currentObjective.subObjectives.FirstOrDefault<AIObjective>() == this))
			{
				this.CumulatedDevotion += this.Devotion * deltaTime;
			}
		}

		// Token: 0x06000E15 RID: 3605 RVA: 0x00089A47 File Offset: 0x00087C47
		public virtual bool IsDuplicate<T>(T otherObjective) where T : AIObjective
		{
			return otherObjective.Option == this.Option;
		}

		// Token: 0x06000E16 RID: 3606 RVA: 0x00089A60 File Offset: 0x00087C60
		public virtual void Update(float deltaTime)
		{
			if (this.resetPriorityTimer > 0f)
			{
				this.resetPriorityTimer -= deltaTime;
			}
			else
			{
				this.ForceHighestPriority = false;
			}
			if (!this.objectiveManager.IsOrder(this) && this.objectiveManager.WaitTimer <= 0f)
			{
				this.UpdateDevotion(deltaTime);
			}
			this.subObjectives.ForEach(delegate(AIObjective so)
			{
				so.Update(deltaTime);
			});
		}

		// Token: 0x06000E17 RID: 3607 RVA: 0x00089AE8 File Offset: 0x00087CE8
		protected virtual void SyncRemovedObjectives<T1, T2>(Dictionary<T1, T2> dictionary, IEnumerable<T1> collection) where T2 : AIObjective
		{
			foreach (T1 key in collection)
			{
				T2 objective;
				if (dictionary.TryGetValue(key, out objective) && !this.subObjectives.Contains(objective))
				{
					dictionary.Remove(key);
				}
			}
		}

		// Token: 0x06000E18 RID: 3608 RVA: 0x00089B50 File Offset: 0x00087D50
		protected bool TryAddSubObjective<T>(ref T objective, Func<T> constructor, Action onCompleted = null, Action onAbandon = null) where T : AIObjective
		{
			if (objective != null)
			{
				if (!this.subObjectives.Contains(objective))
				{
					objective = default(T);
				}
				return false;
			}
			objective = constructor();
			if (!this.subObjectives.Contains(objective))
			{
				if (objective.AllowMultipleInstances)
				{
					objective.SourceObjective = this;
					this.subObjectives.Add(objective);
				}
				else
				{
					this.AddSubObjective(objective, false);
				}
				if (onCompleted != null)
				{
					objective.Completed += onCompleted;
				}
				if (onAbandon != null)
				{
					objective.Abandoned += onAbandon;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000E19 RID: 3609 RVA: 0x00089C1D File Offset: 0x00087E1D
		public virtual void OnSelected()
		{
			this.Reset();
			Action selected = this.Selected;
			if (selected != null)
			{
				selected();
			}
			this.Selected = null;
		}

		// Token: 0x06000E1A RID: 3610 RVA: 0x00089C3D File Offset: 0x00087E3D
		public virtual void OnDeselected()
		{
			this.CumulatedDevotion = 0f;
			Action deselected = this.Deselected;
			if (deselected != null)
			{
				deselected();
			}
			this.Deselected = null;
		}

		// Token: 0x06000E1B RID: 3611 RVA: 0x00089C62 File Offset: 0x00087E62
		protected virtual void OnCompleted()
		{
			Action completed = this.Completed;
			if (completed != null)
			{
				completed();
			}
			this.Completed = null;
		}

		// Token: 0x06000E1C RID: 3612 RVA: 0x00089C7C File Offset: 0x00087E7C
		protected virtual void OnAbandon()
		{
			Action abandoned = this.Abandoned;
			if (abandoned != null)
			{
				abandoned();
			}
			this.Abandoned = null;
		}

		// Token: 0x06000E1D RID: 3613 RVA: 0x00089C96 File Offset: 0x00087E96
		public virtual void Reset()
		{
			this.subObjectives.Clear();
			this.isCompleted = false;
			this.hasBeenChecked = false;
			this._abandon = false;
			this.CumulatedDevotion = 0f;
		}

		// Token: 0x06000E1E RID: 3614
		protected abstract void Act(float deltaTime);

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x06000E1F RID: 3615 RVA: 0x00089CC3 File Offset: 0x00087EC3
		// (set) Token: 0x06000E20 RID: 3616 RVA: 0x00089CDA File Offset: 0x00087EDA
		public bool IsCompleted
		{
			get
			{
				if (!this.hasBeenChecked)
				{
					this.CheckState();
				}
				return this.isCompleted;
			}
			protected set
			{
				if (this.isCompleted == value)
				{
					return;
				}
				this.isCompleted = value;
				if (this.isCompleted)
				{
					this.OnCompleted();
				}
			}
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x00089CFB File Offset: 0x00087EFB
		private bool Check()
		{
			if (this.isCompleted)
			{
				return true;
			}
			if (this.AbortCondition != null && this.AbortCondition(this))
			{
				this.Abandon = true;
				return false;
			}
			return this.CheckObjectiveState();
		}

		// Token: 0x06000E22 RID: 3618
		protected abstract bool CheckObjectiveState();

		// Token: 0x06000E23 RID: 3619 RVA: 0x00089D2C File Offset: 0x00087F2C
		private bool CheckState()
		{
			this.hasBeenChecked = true;
			this.CheckSubObjectives();
			if ((this.subObjectives.None(null) || this.ConcurrentObjectives) && this.Check())
			{
				this.IsCompleted = true;
			}
			return this.isCompleted;
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x00089D68 File Offset: 0x00087F68
		private void CheckSubObjectives()
		{
			for (int i = 0; i < this.subObjectives.Count; i++)
			{
				AIObjective subObjective = this.subObjectives[i];
				subObjective.CheckState();
				if (subObjective.IsCompleted)
				{
					this.subObjectives.Remove(subObjective);
				}
				else if (!subObjective.CanBeCompleted)
				{
					this.subObjectives.Remove(subObjective);
					if (this.AbandonWhenCannotCompleteSubObjectives)
					{
						if (this.objectiveManager.IsOrder(this))
						{
							this.Reset();
						}
						else
						{
							this.Abandon = true;
						}
					}
				}
			}
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x00089DF0 File Offset: 0x00087FF0
		protected static bool CanPutInInventory(Character character, Item item, bool allowWearing)
		{
			if (item == null)
			{
				return false;
			}
			bool canEquip = false;
			if (item.AllowedSlots.Contains(InvSlotType.Any) && character.Inventory.IsAnySlotAvailable(item))
			{
				canEquip = true;
			}
			if (!canEquip)
			{
				CharacterInventory inv = character.Inventory;
				foreach (InvSlotType allowedSlot in item.AllowedSlots)
				{
					if (allowWearing || allowedSlot.HasFlag(InvSlotType.RightHand) || allowedSlot.HasFlag(InvSlotType.LeftHand))
					{
						foreach (InvSlotType slotType in inv.SlotTypes)
						{
							if (allowedSlot.HasFlag(slotType))
							{
								for (int i = 0; i < inv.Capacity; i++)
								{
									canEquip = true;
									if (allowedSlot.HasFlag(inv.SlotTypes[i]) && inv.GetItemAt(i) != null)
									{
										canEquip = false;
										break;
									}
								}
							}
						}
					}
				}
			}
			return canEquip && character.Inventory.CanBePut(item);
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x00089F20 File Offset: 0x00088120
		protected bool CanEquip(Item item, bool allowWearing)
		{
			return AIObjective.CanPutInInventory(this.character, item, allowWearing);
		}

		// Token: 0x04000686 RID: 1670
		public EventAction SourceEventAction;

		// Token: 0x04000687 RID: 1671
		public AIObjective SourceObjective;

		// Token: 0x04000688 RID: 1672
		protected readonly List<AIObjective> subObjectives = new List<AIObjective>();

		// Token: 0x04000689 RID: 1673
		private float _cumulatedDevotion;

		// Token: 0x0400068D RID: 1677
		private float resetPriorityTimer;

		// Token: 0x0400068E RID: 1678
		private readonly float resetPriorityTime = 1f;

		// Token: 0x0400068F RID: 1679
		private bool _forceHighestPriority;

		// Token: 0x04000693 RID: 1683
		public readonly Character character;

		// Token: 0x04000694 RID: 1684
		public readonly AIObjectiveManager objectiveManager;

		// Token: 0x04000695 RID: 1685
		public readonly Identifier Option;

		// Token: 0x04000696 RID: 1686
		private bool _abandon;

		// Token: 0x04000697 RID: 1687
		private readonly List<AIObjective> all = new List<AIObjective>();

		// Token: 0x04000698 RID: 1688
		public Func<AIObjective, bool> AbortCondition;

		// Token: 0x0400069D RID: 1693
		private bool isCompleted;

		// Token: 0x0400069E RID: 1694
		private bool hasBeenChecked;
	}
}
