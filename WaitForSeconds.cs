using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200025D RID: 605
	internal sealed class WaitForSeconds : CoroutineStatus
	{
		// Token: 0x0600382E RID: 14382 RVA: 0x00217004 File Offset: 0x00215204
		public WaitForSeconds(float time, bool ignorePause = true)
		{
			this.timer = time;
			this.TotalTime = time;
			this.ignorePause = ignorePause;
		}

		// Token: 0x0600382F RID: 14383 RVA: 0x00217021 File Offset: 0x00215221
		public override bool CheckFinished(float deltaTime)
		{
			if (this.ignorePause || !CoroutineManager.Paused)
			{
				this.timer -= deltaTime;
			}
			return this.timer <= 0f;
		}

		// Token: 0x06003830 RID: 14384 RVA: 0x00217050 File Offset: 0x00215250
		[NullableContext(1)]
		public override bool EndsCoroutine(CoroutineHandle handle)
		{
			return false;
		}

		// Token: 0x04001C37 RID: 7223
		public readonly float TotalTime;

		// Token: 0x04001C38 RID: 7224
		private float timer;

		// Token: 0x04001C39 RID: 7225
		private readonly bool ignorePause;
	}
}
