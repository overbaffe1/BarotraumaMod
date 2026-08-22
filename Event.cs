using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000275 RID: 629
	internal class Event
	{
		// Token: 0x1400001F RID: 31
		// (add) Token: 0x06003868 RID: 14440 RVA: 0x00217FB0 File Offset: 0x002161B0
		// (remove) Token: 0x06003869 RID: 14441 RVA: 0x00217FE8 File Offset: 0x002161E8
		public event Action Finished;

		// Token: 0x17000EC9 RID: 3785
		// (get) Token: 0x0600386A RID: 14442 RVA: 0x0021801D File Offset: 0x0021621D
		public EventPrefab Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x17000ECA RID: 3786
		// (get) Token: 0x0600386B RID: 14443 RVA: 0x00218025 File Offset: 0x00216225
		// (set) Token: 0x0600386C RID: 14444 RVA: 0x0021802D File Offset: 0x0021622D
		public EventSet ParentSet { get; private set; }

		// Token: 0x17000ECB RID: 3787
		// (get) Token: 0x0600386D RID: 14445 RVA: 0x00218036 File Offset: 0x00216236
		// (set) Token: 0x0600386E RID: 14446 RVA: 0x0021803E File Offset: 0x0021623E
		public bool Initialized { get; private set; }

		// Token: 0x17000ECC RID: 3788
		// (get) Token: 0x0600386F RID: 14447 RVA: 0x00218047 File Offset: 0x00216247
		public bool IsFinished
		{
			get
			{
				return this.isFinished;
			}
		}

		// Token: 0x06003870 RID: 14448 RVA: 0x0021804F File Offset: 0x0021624F
		public override string ToString()
		{
			return "Event (" + this.prefab.EventType.ToString() + ")";
		}

		// Token: 0x17000ECD RID: 3789
		// (get) Token: 0x06003871 RID: 14449 RVA: 0x00218070 File Offset: 0x00216270
		public virtual Vector2 DebugDrawPos
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06003872 RID: 14450 RVA: 0x00218077 File Offset: 0x00216277
		public Event(EventPrefab prefab, int seed)
		{
			this.RandomSeed = seed;
			if (prefab == null)
			{
				throw new ArgumentNullException("prefab");
			}
			this.prefab = prefab;
		}

		// Token: 0x06003873 RID: 14451 RVA: 0x0021809C File Offset: 0x0021629C
		public virtual IEnumerable<ContentFile> GetFilesToPreload()
		{
			return new Event.<GetFilesToPreload>d__24(-2);
		}

		// Token: 0x06003874 RID: 14452 RVA: 0x002180A5 File Offset: 0x002162A5
		public void Init(EventSet parentSet = null)
		{
			this.Initialized = true;
			this.ParentSet = parentSet;
			this.InitEventSpecific(parentSet);
		}

		// Token: 0x06003875 RID: 14453 RVA: 0x002180BC File Offset: 0x002162BC
		protected virtual void InitEventSpecific(EventSet parentSet = null)
		{
		}

		// Token: 0x06003876 RID: 14454 RVA: 0x002180BE File Offset: 0x002162BE
		public virtual string GetDebugInfo()
		{
			return "Finished: " + this.IsFinished.ColorizeObject();
		}

		// Token: 0x06003877 RID: 14455 RVA: 0x002180DA File Offset: 0x002162DA
		public virtual void Update(float deltaTime)
		{
		}

		// Token: 0x06003878 RID: 14456 RVA: 0x002180DC File Offset: 0x002162DC
		public virtual void Finish()
		{
			this.isFinished = true;
			Action finished = this.Finished;
			if (finished == null)
			{
				return;
			}
			finished();
		}

		// Token: 0x06003879 RID: 14457 RVA: 0x002180F5 File Offset: 0x002162F5
		public virtual bool LevelMeetsRequirements()
		{
			return true;
		}

		// Token: 0x04001D46 RID: 7494
		protected bool isFinished;

		// Token: 0x04001D47 RID: 7495
		public readonly int RandomSeed;

		// Token: 0x04001D48 RID: 7496
		protected readonly EventPrefab prefab;

		// Token: 0x04001D49 RID: 7497
		[Nullable(2)]
		public Mission TriggeringMission;

		// Token: 0x04001D4C RID: 7500
		public Func<Level.InterestingPosition, bool> SpawnPosFilter;
	}
}
