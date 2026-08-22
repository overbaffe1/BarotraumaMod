using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x02000361 RID: 865
	internal class DurationListElement
	{
		// Token: 0x17001195 RID: 4501
		// (get) Token: 0x06004307 RID: 17159 RVA: 0x00251ABB File Offset: 0x0024FCBB
		// (set) Token: 0x06004308 RID: 17160 RVA: 0x00251AC3 File Offset: 0x0024FCC3
		public float Duration { get; private set; }

		// Token: 0x17001196 RID: 4502
		// (get) Token: 0x06004309 RID: 17161 RVA: 0x00251ACC File Offset: 0x0024FCCC
		// (set) Token: 0x0600430A RID: 17162 RVA: 0x00251AD4 File Offset: 0x0024FCD4
		public Character User { get; private set; }

		// Token: 0x0600430B RID: 17163 RVA: 0x00251AE0 File Offset: 0x0024FCE0
		public DurationListElement(StatusEffect parentEffect, Entity parentEntity, IEnumerable<ISerializableEntity> targets, float duration, Character user)
		{
			this.Parent = parentEffect;
			this.Entity = parentEntity;
			this.Targets = new List<ISerializableEntity>(targets);
			this.Duration = duration;
			this.Timer = duration;
			this.User = user;
		}

		// Token: 0x0600430C RID: 17164 RVA: 0x00251B28 File Offset: 0x0024FD28
		public void Reset(float duration, Character newUser)
		{
			this.Duration = duration;
			this.Timer = duration;
			this.User = newUser;
		}

		// Token: 0x040022CB RID: 8907
		public readonly StatusEffect Parent;

		// Token: 0x040022CC RID: 8908
		public readonly Entity Entity;

		// Token: 0x040022CE RID: 8910
		public readonly List<ISerializableEntity> Targets;

		// Token: 0x040022D0 RID: 8912
		public float Timer;
	}
}
