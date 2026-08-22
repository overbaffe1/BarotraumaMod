using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200005C RID: 92
	internal class AITargetMemory
	{
		// Token: 0x17000374 RID: 884
		// (get) Token: 0x06000D11 RID: 3345 RVA: 0x0007F6D8 File Offset: 0x0007D8D8
		// (set) Token: 0x06000D12 RID: 3346 RVA: 0x0007F6E0 File Offset: 0x0007D8E0
		public Vector2 Location { get; set; }

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x06000D13 RID: 3347 RVA: 0x0007F6E9 File Offset: 0x0007D8E9
		// (set) Token: 0x06000D14 RID: 3348 RVA: 0x0007F6F1 File Offset: 0x0007D8F1
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

		// Token: 0x06000D15 RID: 3349 RVA: 0x0007F709 File Offset: 0x0007D909
		public AITargetMemory(AITarget target, float priority)
		{
			this.Target = target;
			this.Location = target.WorldPosition;
			this.priority = priority;
		}

		// Token: 0x04000601 RID: 1537
		public readonly AITarget Target;

		// Token: 0x04000603 RID: 1539
		private float priority;
	}
}
