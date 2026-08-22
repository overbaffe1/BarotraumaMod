using System;

namespace Barotrauma
{
	// Token: 0x02000049 RID: 73
	internal class UnimplementedScreen : Screen
	{
		// Token: 0x06000BD7 RID: 3031 RVA: 0x0007074A File Offset: 0x0006E94A
		public override void Select()
		{
			throw new Exception("Tried to select unimplemented screen");
		}

		// Token: 0x04000511 RID: 1297
		public static readonly UnimplementedScreen Instance = new UnimplementedScreen();
	}
}
