using System;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x0200048F RID: 1167
	internal interface ILuaCsTimer : IReusableService, IService, IDisposable, ILuaCsShim
	{
		// Token: 0x17001064 RID: 4196
		// (get) Token: 0x06003E38 RID: 15928 RVA: 0x0018FBC7 File Offset: 0x0018DDC7
		double Time
		{
			get
			{
				return Timing.TotalTime;
			}
		}

		// Token: 0x06003E39 RID: 15929 RVA: 0x0018FBCE File Offset: 0x0018DDCE
		public static double GetTime()
		{
			return ILuaCsTimer.Time;
		}

		// Token: 0x17001065 RID: 4197
		// (get) Token: 0x06003E3A RID: 15930 RVA: 0x0018FBD5 File Offset: 0x0018DDD5
		// (set) Token: 0x06003E3B RID: 15931 RVA: 0x0018FBDC File Offset: 0x0018DDDC
		double AccumulatorMax { get; set; }

		// Token: 0x06003E3C RID: 15932
		void Clear();

		// Token: 0x06003E3D RID: 15933
		void Wait(LuaCsAction action, int millisecondDelay);

		// Token: 0x06003E3E RID: 15934
		void NextFrame(LuaCsAction action);
	}
}
