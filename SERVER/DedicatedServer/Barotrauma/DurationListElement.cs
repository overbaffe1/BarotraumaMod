using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x02000294 RID: 660
	internal class DurationListElement
	{
		// Token: 0x17000D89 RID: 3465
		// (get) Token: 0x06002E30 RID: 11824 RVA: 0x0013228B File Offset: 0x0013048B
		// (set) Token: 0x06002E31 RID: 11825 RVA: 0x00132293 File Offset: 0x00130493
		public float Duration { get; private set; }

		// Token: 0x17000D8A RID: 3466
		// (get) Token: 0x06002E32 RID: 11826 RVA: 0x0013229C File Offset: 0x0013049C
		// (set) Token: 0x06002E33 RID: 11827 RVA: 0x001322A4 File Offset: 0x001304A4
		public Character User { get; private set; }

		// Token: 0x06002E34 RID: 11828 RVA: 0x001322B0 File Offset: 0x001304B0
		public DurationListElement(StatusEffect parentEffect, Entity parentEntity, IEnumerable<ISerializableEntity> targets, float duration, Character user)
		{
			this.Parent = parentEffect;
			this.Entity = parentEntity;
			this.Targets = new List<ISerializableEntity>(targets);
			this.Duration = duration;
			this.Timer = duration;
			this.User = user;
		}

		// Token: 0x06002E35 RID: 11829 RVA: 0x001322F8 File Offset: 0x001304F8
		public void Reset(float duration, Character newUser)
		{
			this.Duration = duration;
			this.Timer = duration;
			this.User = newUser;
		}

		// Token: 0x0400169F RID: 5791
		public readonly StatusEffect Parent;

		// Token: 0x040016A0 RID: 5792
		public readonly Entity Entity;

		// Token: 0x040016A2 RID: 5794
		public readonly List<ISerializableEntity> Targets;

		// Token: 0x040016A4 RID: 5796
		public float Timer;
	}
}
