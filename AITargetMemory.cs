using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000164 RID: 356
	internal class AITargetMemory
	{
		// Token: 0x17000AC1 RID: 2753
		// (get) Token: 0x06002ACF RID: 10959 RVA: 0x001D875B File Offset: 0x001D695B
		// (set) Token: 0x06002AD0 RID: 10960 RVA: 0x001D8763 File Offset: 0x001D6963
		public Vector2 Location { get; set; }

		// Token: 0x17000AC2 RID: 2754
		// (get) Token: 0x06002AD1 RID: 10961 RVA: 0x001D876C File Offset: 0x001D696C
		// (set) Token: 0x06002AD2 RID: 10962 RVA: 0x001D8774 File Offset: 0x001D6974
		public float Priority
		{
			get
			{
				return this.priority;
			}
			set
			{
				this.priority = MathHelper.Clamp(value, 1f, 100f);
			}
		}

		// Token: 0x06002AD3 RID: 10963 RVA: 0x001D878C File Offset: 0x001D698C
		public AITargetMemory(AITarget target, float priority)
		{
			this.Target = target;
			this.Location = target.WorldPosition;
			this.priority = priority;
		}

		// Token: 0x04001654 RID: 5716
		public readonly AITarget Target;

		// Token: 0x04001656 RID: 5718
		private float priority;
	}
}
