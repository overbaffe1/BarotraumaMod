using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200025C RID: 604
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class EnumCoroutineStatus : CoroutineStatus
	{
		// Token: 0x06003827 RID: 14375 RVA: 0x00216F30 File Offset: 0x00215130
		private EnumCoroutineStatus(EnumCoroutineStatus.StatusValue value)
		{
			this.value = value;
		}

		// Token: 0x06003828 RID: 14376 RVA: 0x00216F3F File Offset: 0x0021513F
		public override bool CheckFinished(float deltaTime)
		{
			return true;
		}

		// Token: 0x06003829 RID: 14377 RVA: 0x00216F42 File Offset: 0x00215142
		public override bool EndsCoroutine(CoroutineHandle handle)
		{
			if (this.value == EnumCoroutineStatus.StatusValue.Failure)
			{
				DebugConsole.ThrowError("Coroutine \"" + handle.Name + "\" has failed", null, null, false, false);
			}
			return this.value > EnumCoroutineStatus.StatusValue.Running;
		}

		// Token: 0x0600382A RID: 14378 RVA: 0x00216F74 File Offset: 0x00215174
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			EnumCoroutineStatus other = obj as EnumCoroutineStatus;
			return other != null && this.value == other.value;
		}

		// Token: 0x0600382B RID: 14379 RVA: 0x00216F9C File Offset: 0x0021519C
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		// Token: 0x0600382C RID: 14380 RVA: 0x00216FC0 File Offset: 0x002151C0
		public override string ToString()
		{
			return this.value.ToString();
		}

		// Token: 0x04001C33 RID: 7219
		private readonly EnumCoroutineStatus.StatusValue value;

		// Token: 0x04001C34 RID: 7220
		public new static readonly EnumCoroutineStatus Running = new EnumCoroutineStatus(EnumCoroutineStatus.StatusValue.Running);

		// Token: 0x04001C35 RID: 7221
		public new static readonly EnumCoroutineStatus Success = new EnumCoroutineStatus(EnumCoroutineStatus.StatusValue.Success);

		// Token: 0x04001C36 RID: 7222
		public new static readonly EnumCoroutineStatus Failure = new EnumCoroutineStatus(EnumCoroutineStatus.StatusValue.Failure);

		// Token: 0x02000F0B RID: 3851
		[NullableContext(0)]
		private enum StatusValue
		{
			// Token: 0x04005485 RID: 21637
			Running,
			// Token: 0x04005486 RID: 21638
			Success,
			// Token: 0x04005487 RID: 21639
			Failure
		}
	}
}
