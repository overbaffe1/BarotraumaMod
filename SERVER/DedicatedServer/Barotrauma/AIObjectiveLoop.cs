using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200007D RID: 125
	internal abstract class AIObjectiveLoop<T> : AIObjective
	{
		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x060010A2 RID: 4258 RVA: 0x00098DE6 File Offset: 0x00096FE6
		// (set) Token: 0x060010A3 RID: 4259 RVA: 0x00098DEE File Offset: 0x00096FEE
		public HashSet<T> Targets { get; private set; } = new HashSet<T>();

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x060010A4 RID: 4260 RVA: 0x00098DF7 File Offset: 0x00096FF7
		// (set) Token: 0x060010A5 RID: 4261 RVA: 0x00098DFF File Offset: 0x00096FFF
		public Dictionary<T, AIObjective> Objectives { get; private set; } = new Dictionary<T, AIObjective>();

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x060010A6 RID: 4262 RVA: 0x00098E08 File Offset: 0x00097008
		protected virtual float TargetUpdateTimeMultiplier { get; } = 1f;

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x060010A7 RID: 4263 RVA: 0x00098E10 File Offset: 0x00097010
		protected virtual float IgnoreListClearInterval
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x060010A8 RID: 4264 RVA: 0x00098E17 File Offset: 0x00097017
		// (set) Token: 0x060010A9 RID: 4265 RVA: 0x00098E1F File Offset: 0x0009701F
		public HashSet<T> ReportedTargets { get; private set; } = new HashSet<T>();

		// Token: 0x060010AA RID: 4266 RVA: 0x00098E28 File Offset: 0x00097028
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

		// Token: 0x060010AB RID: 4267 RVA: 0x00098E64 File Offset: 0x00097064
		public AIObjectiveLoop(Character character, AIObjectiveManager objectiveManager, float priorityModifier, Identifier option = default(Identifier)) : base(character, objectiveManager, priorityModifier, option)
		{
		}

		// Token: 0x060010AC RID: 4268 RVA: 0x00098EBE File Offset: 0x000970BE
		protected override void Act(float deltaTime)
		{
		}

		// Token: 0x060010AD RID: 4269 RVA: 0x00098EC0 File Offset: 0x000970C0
		protected override bool CheckObjectiveState()
		{
			return false;
		}

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x060010AE RID: 4270 RVA: 0x00098EC3 File Offset: 0x000970C3
		public override bool CanBeCompleted
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x060010AF RID: 4271 RVA: 0x00098EC6 File Offset: 0x000970C6
		public override bool AbandonWhenCannotCompleteSubObjectives
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x060010B0 RID: 4272 RVA: 0x00098EC9 File Offset: 0x000970C9
		public override bool AllowSubObjectiveSorting
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x060010B1 RID: 4273 RVA: 0x00098ECC File Offset: 0x000970CC
		protected override bool AllowWhileHandcuffed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x060010B2 RID: 4274 RVA: 0x00098ECF File Offset: 0x000970CF
		protected override bool AbandonIfDisallowed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x060010B3 RID: 4275 RVA: 0x00098ED2 File Offset: 0x000970D2
		public virtual bool InverseTargetPriority
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x060010B4 RID: 4276 RVA: 0x00098ED5 File Offset: 0x000970D5
		protected virtual bool ResetWhenClearingIgnoreList
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x060010B5 RID: 4277 RVA: 0x00098ED8 File Offset: 0x000970D8
		protected virtual bool ForceOrderPriority
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x060010B6 RID: 4278 RVA: 0x00098EDB File Offset: 0x000970DB
		protected virtual int MaxTargets
		{
			get
			{
				return int.MaxValue;
			}
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x00098EE4 File Offset: 0x000970E4
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

		// Token: 0x060010B8 RID: 4280 RVA: 0x00099030 File Offset: 0x00097230
		private float CalculateTargetUpdateTimer()
		{
			return this.targetUpdateTimer = 1f / MathHelper.Clamp(base.PriorityModifier * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced), 0.1f, 1f) * this.TargetUpdateTimeMultiplier;
		}

		// Token: 0x060010B9 RID: 4281 RVA: 0x00099079 File Offset: 0x00097279
		public override void Reset()
		{
			base.Reset();
			this.ignoreList.Clear();
			this.ignoreListClearTimer = 0f;
			this.UpdateTargets();
		}

		// Token: 0x060010BA RID: 4282 RVA: 0x000990A0 File Offset: 0x000972A0
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

		// Token: 0x060010BB RID: 4283 RVA: 0x000991A0 File Offset: 0x000973A0
		protected void UpdateTargets()
		{
			this.CalculateTargetUpdateTimer();
			this.Targets.Clear();
			this.FindTargets();
			this.CreateObjectives();
		}

		// Token: 0x060010BC RID: 4284 RVA: 0x000991C0 File Offset: 0x000973C0
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

		// Token: 0x060010BD RID: 4285 RVA: 0x00099280 File Offset: 0x00097480
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

		// Token: 0x060010BE RID: 4286
		protected abstract void OnObjectiveCompleted(AIObjective objective, T target);

		// Token: 0x060010BF RID: 4287
		protected abstract IEnumerable<T> GetList();

		// Token: 0x060010C0 RID: 4288
		protected abstract float GetTargetPriority();

		// Token: 0x060010C1 RID: 4289
		protected abstract AIObjective ObjectiveConstructor(T target);

		// Token: 0x060010C2 RID: 4290
		protected abstract bool IsValidTarget(T target);

		// Token: 0x040007F3 RID: 2035
		protected HashSet<T> ignoreList = new HashSet<T>();

		// Token: 0x040007F4 RID: 2036
		private float ignoreListClearTimer;

		// Token: 0x040007F5 RID: 2037
		protected float targetUpdateTimer;

		// Token: 0x040007F7 RID: 2039
		private float syncTimer;

		// Token: 0x040007F8 RID: 2040
		private readonly float syncTime = 1f;
	}
}
