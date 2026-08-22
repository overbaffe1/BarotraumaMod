using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barotrauma
{
	// Token: 0x020002EF RID: 751
	[Obsolete("Deprecated.")]
	public class LuaCsPerformanceCounter
	{
		// Token: 0x17001054 RID: 4180
		// (get) Token: 0x06003DC6 RID: 15814 RVA: 0x00231454 File Offset: 0x0022F654
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

		// Token: 0x06003DC7 RID: 15815 RVA: 0x00231483 File Offset: 0x0022F683
		public void SetHookElapsedTicks(string eventName, string hookName, long ticks)
		{
			if (!this.HookElapsedTime.ContainsKey(eventName))
			{
				this.HookElapsedTime[eventName] = new Dictionary<string, double>();
			}
			this.HookElapsedTime[eventName][hookName] = (double)ticks / (double)Stopwatch.Frequency;
		}

		// Token: 0x04002067 RID: 8295
		public bool EnablePerformanceCounter;

		// Token: 0x04002068 RID: 8296
		public double UpdateElapsedTime;

		// Token: 0x04002069 RID: 8297
		public Dictionary<string, Dictionary<string, double>> HookElapsedTime = new Dictionary<string, Dictionary<string, double>>();
	}
}
