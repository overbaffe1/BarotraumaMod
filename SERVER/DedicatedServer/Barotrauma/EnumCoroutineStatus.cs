using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000167 RID: 359
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class EnumCoroutineStatus : CoroutineStatus
	{
		// Token: 0x06001D44 RID: 7492 RVA: 0x000D1318 File Offset: 0x000CF518
		private EnumCoroutineStatus(EnumCoroutineStatus.StatusValue value)
		{
			this.value = value;
		}

		// Token: 0x06001D45 RID: 7493 RVA: 0x000D1327 File Offset: 0x000CF527
		public override bool CheckFinished(float deltaTime)
		{
			return true;
		}

		// Token: 0x06001D46 RID: 7494 RVA: 0x000D132A File Offset: 0x000CF52A
		public override bool EndsCoroutine(CoroutineHandle handle)
		{
			if (this.value == EnumCoroutineStatus.StatusValue.Failure)
			{
				DebugConsole.ThrowError("Coroutine \"" + handle.Name + "\" has failed", null, null, false, false);
			}
			return this.value > EnumCoroutineStatus.StatusValue.Running;
		}

		// Token: 0x06001D47 RID: 7495 RVA: 0x000D135C File Offset: 0x000CF55C
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			EnumCoroutineStatus other = obj as EnumCoroutineStatus;
			return other != null && this.value == other.value;
		}

		// Token: 0x06001D48 RID: 7496 RVA: 0x000D1384 File Offset: 0x000CF584
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		// Token: 0x06001D49 RID: 7497 RVA: 0x000D13A8 File Offset: 0x000CF5A8
		public override string ToString()
		{
			return this.value.ToString();
		}

		// Token: 0x04000D2E RID: 3374
		private readonly EnumCoroutineStatus.StatusValue value;

		// Token: 0x04000D2F RID: 3375
		public new static readonly EnumCoroutineStatus Running = new EnumCoroutineStatus(EnumCoroutineStatus.StatusValue.Running);

		// Token: 0x04000D30 RID: 3376
		public new static readonly EnumCoroutineStatus Success = new EnumCoroutineStatus(EnumCoroutineStatus.StatusValue.Success);

		// Token: 0x04000D31 RID: 3377
		public new static readonly EnumCoroutineStatus Failure = new EnumCoroutineStatus(EnumCoroutineStatus.StatusValue.Failure);

		// Token: 0x020008F9 RID: 2297
		[NullableContext(0)]
		private enum StatusValue
		{
			// Token: 0x040031B7 RID: 12727
			Running,
			// Token: 0x040031B8 RID: 12728
			Success,
			// Token: 0x040031B9 RID: 12729
			Failure
		}
	}
}
