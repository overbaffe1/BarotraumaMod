using System;
using System.Collections.Generic;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using FluentResults;

namespace Barotrauma
{
	// Token: 0x020002FE RID: 766
	public class LuaCsTimer : ILuaCsTimer, IReusableService, IService, IDisposable, ILuaCsShim, IEventUpdate, IEvent<IEventUpdate>, IEvent
	{
		// Token: 0x17001063 RID: 4195
		// (get) Token: 0x06003E3C RID: 15932 RVA: 0x00232EE7 File Offset: 0x002310E7
		public static double Time
		{
			get
			{
				return Timing.TotalTime;
			}
		}

		// Token: 0x06003E3D RID: 15933 RVA: 0x00232EEE File Offset: 0x002310EE
		public static double GetTime()
		{
			return LuaCsTimer.Time;
		}

		// Token: 0x17001064 RID: 4196
		// (get) Token: 0x06003E3E RID: 15934 RVA: 0x00232EF5 File Offset: 0x002310F5
		// (set) Token: 0x06003E3F RID: 15935 RVA: 0x00232EFC File Offset: 0x002310FC
		public static double AccumulatorMax
		{
			get
			{
				return Timing.AccumulatorMax;
			}
			set
			{
				Timing.AccumulatorMax = value;
			}
		}

		// Token: 0x06003E40 RID: 15936 RVA: 0x00232F04 File Offset: 0x00231104
		public LuaCsTimer(IEventService eventService, ILoggerService loggerService)
		{
			this._eventService = eventService;
			this._loggerService = loggerService;
			this.SubscribeToEvents();
		}

		// Token: 0x06003E41 RID: 15937 RVA: 0x00232F2C File Offset: 0x0023112C
		private void AddTimer(LuaCsTimer.TimedAction timedAction)
		{
			if (timedAction == null)
			{
				throw new ArgumentNullException("timedAction");
			}
			List<LuaCsTimer.TimedAction> obj = this.timedActions;
			lock (obj)
			{
				int insertionPoint = this.timedActions.BinarySearch(timedAction, new LuaCsTimer.TimerComparer());
				if (insertionPoint < 0)
				{
					insertionPoint = ~insertionPoint;
				}
				this.timedActions.Insert(insertionPoint, timedAction);
			}
		}

		// Token: 0x06003E42 RID: 15938 RVA: 0x00232F9C File Offset: 0x0023119C
		public void Clear()
		{
			this.timedActions = new List<LuaCsTimer.TimedAction>();
		}

		// Token: 0x06003E43 RID: 15939 RVA: 0x00232FAC File Offset: 0x002311AC
		public void Wait(LuaCsAction action, int millisecondDelay)
		{
			LuaCsTimer.TimedAction timedAction = new LuaCsTimer.TimedAction(action, millisecondDelay);
			this.AddTimer(timedAction);
		}

		// Token: 0x06003E44 RID: 15940 RVA: 0x00232FC8 File Offset: 0x002311C8
		public void NextFrame(LuaCsAction action)
		{
			LuaCsTimer.TimedAction timedAction = new LuaCsTimer.TimedAction(action, 0);
			this.AddTimer(timedAction);
		}

		// Token: 0x06003E45 RID: 15941 RVA: 0x00232FE4 File Offset: 0x002311E4
		public void OnUpdate(double fixedDeltaTime)
		{
			List<LuaCsTimer.TimedAction> obj = this.timedActions;
			lock (obj)
			{
				foreach (LuaCsTimer.TimedAction timedAction in this.timedActions.ToArray())
				{
					if (LuaCsTimer.Time < timedAction.ExecutionTime)
					{
						break;
					}
					try
					{
						timedAction.Action(Array.Empty<object>());
					}
					catch (Exception e)
					{
						this._loggerService.HandleException(e, null);
					}
					this.timedActions.Remove(timedAction);
				}
			}
		}

		// Token: 0x06003E46 RID: 15942 RVA: 0x0023308C File Offset: 0x0023128C
		private void SubscribeToEvents()
		{
			this._eventService.Subscribe<IEventUpdate>(this);
		}

		// Token: 0x06003E47 RID: 15943 RVA: 0x0023309B File Offset: 0x0023129B
		public Result Reset()
		{
			this.SubscribeToEvents();
			return Result.Ok();
		}

		// Token: 0x06003E48 RID: 15944 RVA: 0x002330A8 File Offset: 0x002312A8
		public void Dispose()
		{
			this._eventService.Unsubscribe<IEventUpdate>(this);
		}

		// Token: 0x17001065 RID: 4197
		// (get) Token: 0x06003E49 RID: 15945 RVA: 0x002330B6 File Offset: 0x002312B6
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0400207F RID: 8319
		private List<LuaCsTimer.TimedAction> timedActions = new List<LuaCsTimer.TimedAction>();

		// Token: 0x04002080 RID: 8320
		private readonly IEventService _eventService;

		// Token: 0x04002081 RID: 8321
		private readonly ILoggerService _loggerService;

		// Token: 0x02000FD8 RID: 4056
		private class TimerComparer : IComparer<LuaCsTimer.TimedAction>
		{
			// Token: 0x06008A5F RID: 35423 RVA: 0x003AA516 File Offset: 0x003A8716
			public int Compare(LuaCsTimer.TimedAction timedAction1, LuaCsTimer.TimedAction timedAction2)
			{
				if (timedAction1 == null || timedAction2 == null)
				{
					return 0;
				}
				return -Math.Sign(timedAction2.ExecutionTime - timedAction1.ExecutionTime);
			}
		}

		// Token: 0x02000FD9 RID: 4057
		private class TimedAction
		{
			// Token: 0x17001C3A RID: 7226
			// (get) Token: 0x06008A61 RID: 35425 RVA: 0x003AA53B File Offset: 0x003A873B
			// (set) Token: 0x06008A62 RID: 35426 RVA: 0x003AA543 File Offset: 0x003A8743
			public LuaCsAction Action { get; private set; }

			// Token: 0x17001C3B RID: 7227
			// (get) Token: 0x06008A63 RID: 35427 RVA: 0x003AA54C File Offset: 0x003A874C
			// (set) Token: 0x06008A64 RID: 35428 RVA: 0x003AA554 File Offset: 0x003A8754
			public double ExecutionTime { get; private set; }

			// Token: 0x06008A65 RID: 35429 RVA: 0x003AA55D File Offset: 0x003A875D
			public TimedAction(LuaCsAction action, int delayMs)
			{
				this.Action = action;
				this.ExecutionTime = LuaCsTimer.Time + (double)((float)delayMs / 1000f);
			}
		}
	}
}
