using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000181 RID: 385
	internal class Event
	{
		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06001D9C RID: 7580 RVA: 0x000D2994 File Offset: 0x000D0B94
		// (remove) Token: 0x06001D9D RID: 7581 RVA: 0x000D29CC File Offset: 0x000D0BCC
		public event Action Finished;

		// Token: 0x17000875 RID: 2165
		// (get) Token: 0x06001D9E RID: 7582 RVA: 0x000D2A01 File Offset: 0x000D0C01
		public EventPrefab Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x17000876 RID: 2166
		// (get) Token: 0x06001D9F RID: 7583 RVA: 0x000D2A09 File Offset: 0x000D0C09
		// (set) Token: 0x06001DA0 RID: 7584 RVA: 0x000D2A11 File Offset: 0x000D0C11
		public EventSet ParentSet { get; private set; }

		// Token: 0x17000877 RID: 2167
		// (get) Token: 0x06001DA1 RID: 7585 RVA: 0x000D2A1A File Offset: 0x000D0C1A
		// (set) Token: 0x06001DA2 RID: 7586 RVA: 0x000D2A22 File Offset: 0x000D0C22
		public bool Initialized { get; private set; }

		// Token: 0x17000878 RID: 2168
		// (get) Token: 0x06001DA3 RID: 7587 RVA: 0x000D2A2B File Offset: 0x000D0C2B
		public bool IsFinished
		{
			get
			{
				return this.isFinished;
			}
		}

		// Token: 0x06001DA4 RID: 7588 RVA: 0x000D2A33 File Offset: 0x000D0C33
		public override string ToString()
		{
			return "Event (" + this.prefab.EventType.ToString() + ")";
		}

		// Token: 0x17000879 RID: 2169
		// (get) Token: 0x06001DA5 RID: 7589 RVA: 0x000D2A54 File Offset: 0x000D0C54
		public virtual Vector2 DebugDrawPos
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06001DA6 RID: 7590 RVA: 0x000D2A5B File Offset: 0x000D0C5B
		public Event(EventPrefab prefab, int seed)
		{
			this.RandomSeed = seed;
			if (prefab == null)
			{
				throw new ArgumentNullException("prefab");
			}
			this.prefab = prefab;
		}

		// Token: 0x06001DA7 RID: 7591 RVA: 0x000D2A80 File Offset: 0x000D0C80
		public virtual IEnumerable<ContentFile> GetFilesToPreload()
		{
			return new Event.<GetFilesToPreload>d__24(-2);
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x000D2A89 File Offset: 0x000D0C89
		public void Init(EventSet parentSet = null)
		{
			this.Initialized = true;
			this.ParentSet = parentSet;
			this.InitEventSpecific(parentSet);
		}

		// Token: 0x06001DA9 RID: 7593 RVA: 0x000D2AA0 File Offset: 0x000D0CA0
		protected virtual void InitEventSpecific(EventSet parentSet = null)
		{
		}

		// Token: 0x06001DAA RID: 7594 RVA: 0x000D2AA2 File Offset: 0x000D0CA2
		public virtual string GetDebugInfo()
		{
			return "Finished: " + this.IsFinished.ColorizeObject();
		}

		// Token: 0x06001DAB RID: 7595 RVA: 0x000D2ABE File Offset: 0x000D0CBE
		public virtual void Update(float deltaTime)
		{
		}

		// Token: 0x06001DAC RID: 7596 RVA: 0x000D2AC0 File Offset: 0x000D0CC0
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

		// Token: 0x06001DAD RID: 7597 RVA: 0x000D2AD9 File Offset: 0x000D0CD9
		public virtual bool LevelMeetsRequirements()
		{
			return true;
		}

		// Token: 0x04000E4F RID: 3663
		protected bool isFinished;

		// Token: 0x04000E50 RID: 3664
		public readonly int RandomSeed;

		// Token: 0x04000E51 RID: 3665
		protected readonly EventPrefab prefab;

		// Token: 0x04000E52 RID: 3666
		[Nullable(2)]
		public Mission TriggeringMission;

		// Token: 0x04000E55 RID: 3669
		public Func<Level.InterestingPosition, bool> SpawnPosFilter;
	}
}
