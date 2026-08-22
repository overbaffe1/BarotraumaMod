using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000151 RID: 337
	[NullableContext(1)]
	[Nullable(0)]
	internal class LeakyBucket
	{
		// Token: 0x06002A13 RID: 10771 RVA: 0x001D168A File Offset: 0x001CF88A
		public LeakyBucket(float cooldownInSeconds, int capacity)
		{
			this.cooldownInSeconds = cooldownInSeconds;
			this.capacity = capacity;
			this.queue = new Queue<Action>(capacity);
		}

		// Token: 0x06002A14 RID: 10772 RVA: 0x001D16AC File Offset: 0x001CF8AC
		public void Update(float deltaTime)
		{
			if (this.timer > 0f)
			{
				this.timer -= deltaTime;
				return;
			}
			if (this.queue.Count == 0)
			{
				return;
			}
			this.TryDequeue();
		}

		// Token: 0x06002A15 RID: 10773 RVA: 0x001D16E0 File Offset: 0x001CF8E0
		private void TryDequeue()
		{
			this.timer = this.cooldownInSeconds;
			Action action;
			if (this.queue.TryDequeue(out action))
			{
				action();
			}
		}

		// Token: 0x06002A16 RID: 10774 RVA: 0x001D170E File Offset: 0x001CF90E
		public bool TryEnqueue(Action item)
		{
			if (this.queue.Count >= this.capacity)
			{
				return false;
			}
			this.queue.Enqueue(item);
			return true;
		}

		// Token: 0x04001602 RID: 5634
		private readonly Queue<Action> queue;

		// Token: 0x04001603 RID: 5635
		private readonly int capacity;

		// Token: 0x04001604 RID: 5636
		private readonly float cooldownInSeconds;

		// Token: 0x04001605 RID: 5637
		private float timer;
	}
}
