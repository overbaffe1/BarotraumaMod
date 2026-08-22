using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000168 RID: 360
	internal sealed class WaitForSeconds : CoroutineStatus
	{
		// Token: 0x06001D4B RID: 7499 RVA: 0x000D13EC File Offset: 0x000CF5EC
		public WaitForSeconds(float time, bool ignorePause = true)
		{
			this.timer = time;
			this.TotalTime = time;
			this.ignorePause = ignorePause;
		}

		// Token: 0x06001D4C RID: 7500 RVA: 0x000D1409 File Offset: 0x000CF609
		public override bool CheckFinished(float deltaTime)
		{
			this.timer -= deltaTime;
			return this.timer <= 0f;
		}

		// Token: 0x06001D4D RID: 7501 RVA: 0x000D1429 File Offset: 0x000CF629
		[NullableContext(1)]
		public override bool EndsCoroutine(CoroutineHandle handle)
		{
			return false;
		}

		// Token: 0x04000D32 RID: 3378
		public readonly float TotalTime;

		// Token: 0x04000D33 RID: 3379
		private float timer;

		// Token: 0x04000D34 RID: 3380
		private readonly bool ignorePause;
	}
}
