using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200025B RID: 603
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class CoroutineStatus
	{
		// Token: 0x17000EB4 RID: 3764
		// (get) Token: 0x06003821 RID: 14369 RVA: 0x00216F13 File Offset: 0x00215113
		public static CoroutineStatus Running
		{
			get
			{
				return EnumCoroutineStatus.Running;
			}
		}

		// Token: 0x17000EB5 RID: 3765
		// (get) Token: 0x06003822 RID: 14370 RVA: 0x00216F1A File Offset: 0x0021511A
		public static CoroutineStatus Success
		{
			get
			{
				return EnumCoroutineStatus.Success;
			}
		}

		// Token: 0x17000EB6 RID: 3766
		// (get) Token: 0x06003823 RID: 14371 RVA: 0x00216F21 File Offset: 0x00215121
		public static CoroutineStatus Failure
		{
			get
			{
				return EnumCoroutineStatus.Failure;
			}
		}

		// Token: 0x06003824 RID: 14372
		public abstract bool CheckFinished(float deltaTime);

		// Token: 0x06003825 RID: 14373
		public abstract bool EndsCoroutine(CoroutineHandle handle);
	}
}
