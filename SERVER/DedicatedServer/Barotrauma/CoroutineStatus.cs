using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000166 RID: 358
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class CoroutineStatus
	{
		// Token: 0x17000856 RID: 2134
		// (get) Token: 0x06001D3E RID: 7486 RVA: 0x000D12FB File Offset: 0x000CF4FB
		public static CoroutineStatus Running
		{
			get
			{
				return EnumCoroutineStatus.Running;
			}
		}

		// Token: 0x17000857 RID: 2135
		// (get) Token: 0x06001D3F RID: 7487 RVA: 0x000D1302 File Offset: 0x000CF502
		public static CoroutineStatus Success
		{
			get
			{
				return EnumCoroutineStatus.Success;
			}
		}

		// Token: 0x17000858 RID: 2136
		// (get) Token: 0x06001D40 RID: 7488 RVA: 0x000D1309 File Offset: 0x000CF509
		public static CoroutineStatus Failure
		{
			get
			{
				return EnumCoroutineStatus.Failure;
			}
		}

		// Token: 0x06001D41 RID: 7489
		public abstract bool CheckFinished(float deltaTime);

		// Token: 0x06001D42 RID: 7490
		public abstract bool EndsCoroutine(CoroutineHandle handle);
	}
}
