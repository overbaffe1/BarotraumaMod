using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x020000E5 RID: 229
	internal interface IFishAnimation
	{
		// Token: 0x17000729 RID: 1833
		// (get) Token: 0x06001840 RID: 6208
		// (set) Token: 0x06001841 RID: 6209
		string FootAngles { get; set; }

		// Token: 0x1700072A RID: 1834
		// (get) Token: 0x06001842 RID: 6210
		// (set) Token: 0x06001843 RID: 6211
		Dictionary<int, float> FootAnglesInRadians { get; set; }

		// Token: 0x1700072B RID: 1835
		// (get) Token: 0x06001844 RID: 6212
		// (set) Token: 0x06001845 RID: 6213
		float TailAngle { get; set; }

		// Token: 0x1700072C RID: 1836
		// (get) Token: 0x06001846 RID: 6214
		float TailAngleInRadians { get; }

		// Token: 0x1700072D RID: 1837
		// (get) Token: 0x06001847 RID: 6215
		// (set) Token: 0x06001848 RID: 6216
		float TailTorque { get; set; }

		// Token: 0x1700072E RID: 1838
		// (get) Token: 0x06001849 RID: 6217
		// (set) Token: 0x0600184A RID: 6218
		bool Flip { get; set; }

		// Token: 0x1700072F RID: 1839
		// (get) Token: 0x0600184B RID: 6219
		// (set) Token: 0x0600184C RID: 6220
		float FlipCooldown { get; set; }

		// Token: 0x17000730 RID: 1840
		// (get) Token: 0x0600184D RID: 6221
		// (set) Token: 0x0600184E RID: 6222
		float FlipDelay { get; set; }
	}
}
