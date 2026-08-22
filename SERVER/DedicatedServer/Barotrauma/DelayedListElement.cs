using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000291 RID: 657
	internal class DelayedListElement
	{
		// Token: 0x06002E16 RID: 11798 RVA: 0x00130D75 File Offset: 0x0012EF75
		public DelayedListElement(DelayedEffect parentEffect, Entity parentEntity, IEnumerable<ISerializableEntity> targets, float delay, Vector2? worldPosition, Vector2? startPosition)
		{
			this.Parent = parentEffect;
			this.Entity = parentEntity;
			this.Targets = new List<ISerializableEntity>(targets);
			this.Delay = delay;
			this.WorldPosition = worldPosition;
			this.StartPosition = startPosition;
		}

		// Token: 0x04001688 RID: 5768
		public readonly DelayedEffect Parent;

		// Token: 0x04001689 RID: 5769
		public readonly Entity Entity;

		// Token: 0x0400168A RID: 5770
		public Vector2? WorldPosition;

		// Token: 0x0400168B RID: 5771
		public bool GetPositionBasedOnTargets;

		// Token: 0x0400168C RID: 5772
		public readonly Vector2? StartPosition;

		// Token: 0x0400168D RID: 5773
		public readonly List<ISerializableEntity> Targets;

		// Token: 0x0400168E RID: 5774
		public float Delay;
	}
}
