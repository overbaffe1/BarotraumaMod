using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200035E RID: 862
	internal class DelayedListElement
	{
		// Token: 0x060042ED RID: 17133 RVA: 0x002505A5 File Offset: 0x0024E7A5
		public DelayedListElement(DelayedEffect parentEffect, Entity parentEntity, IEnumerable<ISerializableEntity> targets, float delay, Vector2? worldPosition, Vector2? startPosition)
		{
			this.Parent = parentEffect;
			this.Entity = parentEntity;
			this.Targets = new List<ISerializableEntity>(targets);
			this.Delay = delay;
			this.WorldPosition = worldPosition;
			this.StartPosition = startPosition;
		}

		// Token: 0x040022B4 RID: 8884
		public readonly DelayedEffect Parent;

		// Token: 0x040022B5 RID: 8885
		public readonly Entity Entity;

		// Token: 0x040022B6 RID: 8886
		public Vector2? WorldPosition;

		// Token: 0x040022B7 RID: 8887
		public bool GetPositionBasedOnTargets;

		// Token: 0x040022B8 RID: 8888
		public readonly Vector2? StartPosition;

		// Token: 0x040022B9 RID: 8889
		public readonly List<ISerializableEntity> Targets;

		// Token: 0x040022BA RID: 8890
		public float Delay;
	}
}
