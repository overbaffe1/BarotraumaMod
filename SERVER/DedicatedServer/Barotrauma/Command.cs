using System;

namespace Barotrauma
{
	// Token: 0x02000125 RID: 293
	internal abstract class Command
	{
		// Token: 0x06001BA8 RID: 7080
		public abstract void Execute();

		// Token: 0x06001BA9 RID: 7081
		public abstract void UnExecute();

		// Token: 0x06001BAA RID: 7082
		public abstract void Cleanup();
	}
}
