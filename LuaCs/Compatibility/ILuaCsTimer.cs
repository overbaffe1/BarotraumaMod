using System;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x02000572 RID: 1394
	internal interface ILuaCsTimer : IReusableService, IService, IDisposable, ILuaCsShim
	{
		// Token: 0x17001539 RID: 5433
		// (get) Token: 0x060055F0 RID: 22000 RVA: 0x002D14EE File Offset: 0x002CF6EE
		double Time
		{
			get
			{
				return Timing.TotalTime;
			}
		}

		// Token: 0x060055F1 RID: 22001 RVA: 0x002D14F5 File Offset: 0x002CF6F5
		public static double GetTime()
		{
			return ILuaCsTimer.Time;
		}

		// Token: 0x1700153A RID: 5434
		// (get) Token: 0x060055F2 RID: 22002 RVA: 0x002D14FC File Offset: 0x002CF6FC
		// (set) Token: 0x060055F3 RID: 22003 RVA: 0x002D1503 File Offset: 0x002CF703
		double AccumulatorMax { get; set; }

		// Token: 0x060055F4 RID: 22004
		void Clear();

		// Token: 0x060055F5 RID: 22005
		void Wait(LuaCsAction action, int millisecondDelay);

		// Token: 0x060055F6 RID: 22006
		void NextFrame(LuaCsAction action);
	}
}
