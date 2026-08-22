using System;
using System.Collections.Generic;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using FluentResults;

namespace Barotrauma
{
	// Token: 0x02000216 RID: 534
	public class LuaCsTimer : ILuaCsTimer, IReusableService, IService, IDisposable, ILuaCsShim, IEventUpdate, IEvent<IEventUpdate>, IEvent
	{
		// Token: 0x17000ACB RID: 2763
		// (get) Token: 0x06002555 RID: 9557 RVA: 0x000F5557 File Offset: 0x000F3757
		public static double Time
		{
			get
			{
				return Timing.TotalTime;
			}
		}

		// Token: 0x06002556 RID: 9558 RVA: 0x000F555E File Offset: 0x000F375E
		public static double GetTime()
		{
			return LuaCsTimer.Time;
		}

		// Token: 0x17000ACC RID: 2764
		// (get) Token: 0x06002557 RID: 9559 RVA: 0x000F5565 File Offset: 0x000F3765
		// (set) Token: 0x06002558 RID: 9560 RVA: 0x000F556C File Offset: 0x000F376C
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

		// Token: 0x06002559 RID: 9561 RVA: 0x000F5574 File Offset: 0x000F3774
		public LuaCsTimer(IEventService eventService, ILoggerService loggerService)
		{
			this._eventService = eventService;
			this._loggerService = loggerService;
			this.SubscribeToEvents();
		}

		// Token: 0x0600255A RID: 9562 RVA: 0x000F559C File Offset: 0x000F379C
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

		// Token: 0x0600255B RID: 9563 RVA: 0x000F560C File Offset: 0x000F380C
		public void Clear()
		{
			this.timedActions = new List<LuaCsTimer.TimedAction>();
		}

		// Token: 0x0600255C RID: 9564 RVA: 0x000F561C File Offset: 0x000F381C
		public void Wait(LuaCsAction action, int millisecondDelay)
		{
			LuaCsTimer.TimedAction timedAction = new LuaCsTimer.TimedAction(action, millisecondDelay);
			this.AddTimer(timedAction);
		}

		// Token: 0x0600255D RID: 9565 RVA: 0x000F5638 File Offset: 0x000F3838
		public void NextFrame(LuaCsAction action)
		{
			LuaCsTimer.TimedAction timedAction = new LuaCsTimer.TimedAction(action, 0);
			this.AddTimer(timedAction);
		}

		// Token: 0x0600255E RID: 9566 RVA: 0x000F5654 File Offset: 0x000F3854
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

		// Token: 0x0600255F RID: 9567 RVA: 0x000F56FC File Offset: 0x000F38FC
		private void SubscribeToEvents()
		{
			this._eventService.Subscribe<IEventUpdate>(this);
		}

		// Token: 0x06002560 RID: 9568 RVA: 0x000F570B File Offset: 0x000F390B
		public Result Reset()
		{
			this.SubscribeToEvents();
			return Result.Ok();
		}

		// Token: 0x06002561 RID: 9569 RVA: 0x000F5718 File Offset: 0x000F3918
		public void Dispose()
		{
			this._eventService.Unsubscribe<IEventUpdate>(this);
		}

		// Token: 0x17000ACD RID: 2765
		// (get) Token: 0x06002562 RID: 9570 RVA: 0x000F5726 File Offset: 0x000F3926
		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04001253 RID: 4691
		private List<LuaCsTimer.TimedAction> timedActions = new List<LuaCsTimer.TimedAction>();

		// Token: 0x04001254 RID: 4692
		private readonly IEventService _eventService;

		// Token: 0x04001255 RID: 4693
		private readonly ILoggerService _loggerService;

		// Token: 0x020009F4 RID: 2548
		private class TimerComparer : IComparer<LuaCsTimer.TimedAction>
		{
			// Token: 0x06005B66 RID: 23398 RVA: 0x001FE93E File Offset: 0x001FCB3E
			public int Compare(LuaCsTimer.TimedAction timedAction1, LuaCsTimer.TimedAction timedAction2)
			{
				if (timedAction1 == null || timedAction2 == null)
				{
					return 0;
				}
				return -Math.Sign(timedAction2.ExecutionTime - timedAction1.ExecutionTime);
			}
		}

		// Token: 0x020009F5 RID: 2549
		private class TimedAction
		{
			// Token: 0x17001576 RID: 5494
			// (get) Token: 0x06005B68 RID: 23400 RVA: 0x001FE963 File Offset: 0x001FCB63
			// (set) Token: 0x06005B69 RID: 23401 RVA: 0x001FE96B File Offset: 0x001FCB6B
			public LuaCsAction Action { get; private set; }

			// Token: 0x17001577 RID: 5495
			// (get) Token: 0x06005B6A RID: 23402 RVA: 0x001FE974 File Offset: 0x001FCB74
			// (set) Token: 0x06005B6B RID: 23403 RVA: 0x001FE97C File Offset: 0x001FCB7C
			public double ExecutionTime { get; private set; }

			// Token: 0x06005B6C RID: 23404 RVA: 0x001FE985 File Offset: 0x001FCB85
			public TimedAction(LuaCsAction action, int delayMs)
			{
				this.Action = action;
				this.ExecutionTime = LuaCsTimer.Time + (double)((float)delayMs / 1000f);
			}
		}
	}
}
