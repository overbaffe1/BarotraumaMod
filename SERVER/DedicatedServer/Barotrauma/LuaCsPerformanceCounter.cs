using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barotrauma
{
	// Token: 0x02000206 RID: 518
	[Obsolete("Deprecated.")]
	public class LuaCsPerformanceCounter
	{
		// Token: 0x17000ABC RID: 2748
		// (get) Token: 0x060024EE RID: 9454 RVA: 0x000F3FC8 File Offset: 0x000F21C8
		public static float MemoryUsage
		{
			get
			{
				Process proc = Process.GetCurrentProcess();
				float memory = MathF.Round((float)(proc.PrivateMemorySize64 / 1048576L), 2);
				proc.Dispose();
				return memory;
			}
		}

		// Token: 0x060024EF RID: 9455 RVA: 0x000F3FF7 File Offset: 0x000F21F7
		public void SetHookElapsedTicks(string eventName, string hookName, long ticks)
		{
			if (!this.HookElapsedTime.ContainsKey(eventName))
			{
				this.HookElapsedTime[eventName] = new Dictionary<string, double>();
			}
			this.HookElapsedTime[eventName][hookName] = (double)ticks / (double)Stopwatch.Frequency;
		}

		// Token: 0x0400123B RID: 4667
		public bool EnablePerformanceCounter;

		// Token: 0x0400123C RID: 4668
		public double UpdateElapsedTime;

		// Token: 0x0400123D RID: 4669
		public Dictionary<string, Dictionary<string, double>> HookElapsedTime = new Dictionary<string, Dictionary<string, double>>();
	}
}
