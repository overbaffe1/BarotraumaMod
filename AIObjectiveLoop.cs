using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000183 RID: 387
	internal abstract class AIObjectiveLoop<T> : AIObjective
	{
		// Token: 0x17000B9F RID: 2975
		// (get) Token: 0x06002DAA RID: 11690 RVA: 0x001EBC7A File Offset: 0x001E9E7A
		// (set) Token: 0x06002DAB RID: 11691 RVA: 0x001EBC82 File Offset: 0x001E9E82
		public HashSet<T> Targets { get; private set; } = new HashSet<T>();

		// Token: 0x17000BA0 RID: 2976
		// (get) Token: 0x06002DAC RID: 11692 RVA: 0x001EBC8B File Offset: 0x001E9E8B
		// (set) Token: 0x06002DAD RID: 11693 RVA: 0x001EBC93 File Offset: 0x001E9E93
		public Dictionary<T, AIObjective> Objectives { get; private set; } = new Dictionary<T, AIObjective>();

		// Token: 0x17000BA1 RID: 2977
		// (get) Token: 0x06002DAE RID: 11694 RVA: 0x001EBC9C File Offset: 0x001E9E9C
		protected virtual float TargetUpdateTimeMultiplier { get; } = 1f;

		// Token: 0x17000BA2 RID: 2978
		// (get) Token: 0x06002DAF RID: 11695 RVA: 0x001EBCA4 File Offset: 0x001E9EA4
		protected virtual float IgnoreListClearInterval
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000BA3 RID: 2979
		// (get) Token: 0x06002DB0 RID: 11696 RVA: 0x001EBCAB File Offset: 0x001E9EAB
		// (set) Token: 0x06002DB1 RID: 11697 RVA: 0x001EBCB3 File Offset: 0x001E9EB3
		public HashSet<T> ReportedTargets { get; private set; } = new HashSet<T>();

		// Token: 0x06002DB2 RID: 11698 RVA: 0x001EBCBC File Offset: 0x001E9EBC
		public bool AddTarget(T target)
		{
			if (this.character.IsDead)
			{
				return false;
			}
			if (this.ReportedTargets.Contains(target))
			{
				return false;
			}
			if (this.IsValidTarget(target))
			{
				this.ReportedTargets.Add(target);
				return true;
			}
			return false;
		}

		// Token: 0x06002DB3 RID: 11699 RVA: 0x001EBCF8 File Offset: 0x001E9EF8
		public AIObjectiveLoop(Character character, AIObjectiveManager objectiveManager, float priorityModifier, Identifier option = default(Identifier)) : base(character, objectiveManager, priorityModifier, option)
		{
		}

		// Token: 0x06002DB4 RID: 11700 RVA: 0x001EBD52 File Offset: 0x001E9F52
		protected override void Act(float deltaTime)
		{
		}

		// Token: 0x06002DB5 RID: 11701 RVA: 0x001EBD54 File Offset: 0x001E9F54
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x17000BA4 RID: 2980
		// (get) Token: 0x06002DB6 RID: 11702 RVA: 0x001EBD57 File Offset: 0x001E9F57
		public override bool CanBeCompleted
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BA5 RID: 2981
		// (get) Token: 0x06002DB7 RID: 11703 RVA: 0x001EBD5A File Offset: 0x001E9F5A
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BA6 RID: 2982
		// (get) Token: 0x06002DB8 RID: 11704 RVA: 0x001EBD5D File Offset: 0x001E9F5D
		public override bool AllowSubObjectiveSorting
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BA7 RID: 2983
		// (get) Token: 0x06002DB9 RID: 11705 RVA: 0x001EBD60 File Offset: 0x001E9F60
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BA8 RID: 2984
		// (get) Token: 0x06002DBA RID: 11706 RVA: 0x001EBD63 File Offset: 0x001E9F63
		protected override bool AbandonIfDisallowed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BA9 RID: 2985
		// (get) Token: 0x06002DBB RID: 11707 RVA: 0x001EBD66 File Offset: 0x001E9F66
		public virtual bool InverseTargetPriority
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000BAA RID: 2986
		// (get) Token: 0x06002DBC RID: 11708 RVA: 0x001EBD69 File Offset: 0x001E9F69
		protected virtual bool ResetWhenClearingIgnoreList
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BAB RID: 2987
		// (get) Token: 0x06002DBD RID: 11709 RVA: 0x001EBD6C File Offset: 0x001E9F6C
		protected virtual bool ForceOrderPriority
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BAC RID: 2988
		// (get) Token: 0x06002DBE RID: 11710 RVA: 0x001EBD6F File Offset: 0x001E9F6F
		protected virtual int MaxTargets
		{
			get
			{
				return int.MaxValue;
			}
		}

		// Token: 0x06002DBF RID: 11711 RVA: 0x001EBD78 File Offset: 0x001E9F78
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.IgnoreListClearInterval > 0f)
			{
				if (this.ignoreListClearTimer > this.IgnoreListClearInterval)
				{
					if (this.ResetWhenClearingIgnoreList)
					{
						this.Reset();
					}
					else
					{
						this.ignoreList.Clear();
						this.ignoreListClearTimer = 0f;
					}
				}
				else
				{
					this.ignoreListClearTimer += deltaTime;
				}
			}
			if (this.targetUpdateTimer <= 0f)
			{
				this.UpdateTargets();
			}
			else
			{
				this.targetUpdateTimer -= deltaTime;
			}
			if (this.syncTimer <= 0f)
			{
				this.syncTimer = Math.Min(this.syncTime * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced), this.targetUpdateTimer);
				foreach (KeyValuePair<T, AIObjective> objective in this.Objectives)
				{
					T target = objective.Key;
					if (!this.Targets.Contains(target))
					{
						this.subObjectives.Remove(objective.Value);
					}
				}
				this.SyncRemovedObjectives<T, AIObjective>(this.Objectives, this.GetList());
				return;
			}
			this.syncTimer -= deltaTime;
		}

		// Token: 0x06002DC0 RID: 11712 RVA: 0x001EBEC4 File Offset: 0x001EA0C4
		private float CalculateTargetUpdateTimer()
		{
			return this.targetUpdateTimer = 1f / MathHelper.Clamp(base.PriorityModifier * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced), 0.1f, 1f) * this.TargetUpdateTimeMultiplier;
		}

		// Token: 0x06002DC1 RID: 11713 RVA: 0x001EBF0D File Offset: 0x001EA10D
		public override void Reset()
		{
			base.Reset();
			this.ignoreList.Clear();
			this.ignoreListClearTimer = 0f;
			this.UpdateTargets();
		}

		// Token: 0x06002DC2 RID: 11714 RVA: 0x001EBF34 File Offset: 0x001EA134
		protected override float GetPriority()
		{
			if (!base.IsAllowed)
			{
				base.HandleDisallowed();
				return base.Priority;
			}
			float targetPriority = this.GetTargetPriority();
			if (this.InverseTargetPriority)
			{
				targetPriority = 100f - targetPriority;
			}
			AIObjective currentSubObjective = base.CurrentSubObjective;
			if (currentSubObjective != null && currentSubObjective.Priority > targetPriority)
			{
				targetPriority = currentSubObjective.Priority;
			}
			if (targetPriority < 1f)
			{
				base.Priority = 0f;
			}
			else if (this.objectiveManager.IsOrder(this))
			{
				base.Priority = (this.ForceOrderPriority ? this.objectiveManager.GetOrderPriority(this) : targetPriority);
			}
			else
			{
				float max = 59f;
				AIObjectiveRescueAll rescueObjective = this as AIObjectiveRescueAll;
				if (rescueObjective != null && rescueObjective.Targets.Contains(this.character))
				{
					max = 90f;
				}
				float value = MathHelper.Clamp((base.CumulatedDevotion + targetPriority * base.PriorityModifier) / 100f, 0f, 1f);
				base.Priority = MathHelper.Lerp(0f, max, value);
			}
			return base.Priority;
		}

		// Token: 0x06002DC3 RID: 11715 RVA: 0x001EC034 File Offset: 0x001EA234
		protected void UpdateTargets()
		{
			this.CalculateTargetUpdateTimer();
			this.Targets.Clear();
			this.FindTargets();
			this.CreateObjectives();
		}

		// Token: 0x06002DC4 RID: 11716 RVA: 0x001EC054 File Offset: 0x001EA254
		protected virtual void FindTargets()
		{
			foreach (T target in this.GetList())
			{
				if ((this.objectiveManager.IsOrder(this) || (this is AIObjectiveChargeBatteries || this is AIObjectivePumpWater || this is AIObjectiveFindThieves) || this.ReportedTargets.Contains(target)) && this.IsValidTarget(target) && !this.ignoreList.Contains(target))
				{
					this.Targets.Add(target);
					if (this.Targets.Count > this.MaxTargets)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06002DC5 RID: 11717 RVA: 0x001EC114 File Offset: 0x001EA314
		protected virtual void CreateObjectives()
		{
			using (HashSet<T>.Enumerator enumerator = this.Targets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AIObjectiveLoop<T>.<>c__DisplayClass50_0 CS$<>8__locals1 = new AIObjectiveLoop<T>.<>c__DisplayClass50_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.target = enumerator.Current;
					AIObjective objective;
					if (!this.ignoreList.Contains(CS$<>8__locals1.target) && !this.Objectives.TryGetValue(CS$<>8__locals1.target, out objective))
					{
						objective = this.ObjectiveConstructor(CS$<>8__locals1.target);
						this.Objectives.Add(CS$<>8__locals1.target, objective);
						if (!this.subObjectives.Contains(objective))
						{
							this.subObjectives.Add(objective);
						}
						objective.Completed += delegate()
						{
							CS$<>8__locals1.<>4__this.Objectives.Remove(CS$<>8__locals1.target);
							CS$<>8__locals1.<>4__this.OnObjectiveCompleted(objective, CS$<>8__locals1.target);
						};
						objective.Abandoned += delegate()
						{
							CS$<>8__locals1.<>4__this.Objectives.Remove(CS$<>8__locals1.target);
							CS$<>8__locals1.<>4__this.ignoreList.Add(CS$<>8__locals1.target);
							CS$<>8__locals1.<>4__this.targetUpdateTimer = Math.Min(0.1f, CS$<>8__locals1.<>4__this.targetUpdateTimer);
						};
					}
				}
			}
		}

		// Token: 0x06002DC6 RID: 11718
		protected abstract void OnObjectiveCompleted(AIObjective objective, T target);

		// Token: 0x06002DC7 RID: 11719
		protected abstract IEnumerable<T> GetList();

		// Token: 0x06002DC8 RID: 11720
		protected abstract float GetTargetPriority();

		// Token: 0x06002DC9 RID: 11721
		protected abstract AIObjective ObjectiveConstructor(T target);

		// Token: 0x06002DCA RID: 11722
		protected abstract bool IsValidTarget(T target);

		// Token: 0x040017ED RID: 6125
		protected HashSet<T> ignoreList = new HashSet<T>();

		// Token: 0x040017EE RID: 6126
		private float ignoreListClearTimer;

		// Token: 0x040017EF RID: 6127
		protected float targetUpdateTimer;

		// Token: 0x040017F1 RID: 6129
		private float syncTimer;

		// Token: 0x040017F2 RID: 6130
		private readonly float syncTime = 1f;
	}
}
