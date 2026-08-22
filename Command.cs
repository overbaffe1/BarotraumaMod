using System;

namespace Barotrauma
{
	// Token: 0x02000144 RID: 324
	internal abstract class Command
	{
		// Token: 0x060029BB RID: 10683
		public abstract LocalizedString GetDescription();

		// Token: 0x060029BC RID: 10684
		public abstract void Execute();

		// Token: 0x060029BD RID: 10685
		public abstract void UnExecute();

		// Token: 0x060029BE RID: 10686
		public abstract void Cleanup();
	}
}
