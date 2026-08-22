using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200016A RID: 362
	[NullableContext(1)]
	[Nullable(0)]
	internal static class CoroutineManager
	{
		// Token: 0x17000859 RID: 2137
		// (get) Token: 0x06001D4F RID: 7503 RVA: 0x000D1462 File Offset: 0x000CF662
		// (set) Token: 0x06001D50 RID: 7504 RVA: 0x000D1469 File Offset: 0x000CF669
		public static float DeltaTime { get; private set; }

		// Token: 0x1700085A RID: 2138
		// (get) Token: 0x06001D51 RID: 7505 RVA: 0x000D1471 File Offset: 0x000CF671
		// (set) Token: 0x06001D52 RID: 7506 RVA: 0x000D1478 File Offset: 0x000CF678
		public static bool Paused { get; private set; }

		// Token: 0x06001D53 RID: 7507 RVA: 0x000D1480 File Offset: 0x000CF680
		public static CoroutineHandle StartCoroutine(IEnumerable<CoroutineStatus> func, string name = "")
		{
			CoroutineHandle handle = new CoroutineHandle(func.GetEnumerator(), name);
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			lock (coroutines)
			{
				CoroutineManager.Coroutines.Add(handle);
			}
			return handle;
		}

		// Token: 0x06001D54 RID: 7508 RVA: 0x000D14D4 File Offset: 0x000CF6D4
		public static CoroutineHandle Invoke(Action action, float delay = 0f)
		{
			return CoroutineManager.StartCoroutine(CoroutineManager.DoInvokeAfter(action, delay), "");
		}

		// Token: 0x06001D55 RID: 7509 RVA: 0x000D14E7 File Offset: 0x000CF6E7
		private static IEnumerable<CoroutineStatus> DoInvokeAfter([Nullable(2)] Action action, float delay)
		{
			CoroutineManager.<DoInvokeAfter>d__11 <DoInvokeAfter>d__ = new CoroutineManager.<DoInvokeAfter>d__11(-2);
			<DoInvokeAfter>d__.<>3__action = action;
			<DoInvokeAfter>d__.<>3__delay = delay;
			return <DoInvokeAfter>d__;
		}

		// Token: 0x06001D56 RID: 7510 RVA: 0x000D1500 File Offset: 0x000CF700
		public static bool IsCoroutineRunning(string name)
		{
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			bool result;
			lock (coroutines)
			{
				result = CoroutineManager.Coroutines.Any((CoroutineHandle c) => c.Name == name);
			}
			return result;
		}

		// Token: 0x06001D57 RID: 7511 RVA: 0x000D1560 File Offset: 0x000CF760
		public static bool IsCoroutineRunning(CoroutineHandle handle)
		{
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			bool result;
			lock (coroutines)
			{
				result = CoroutineManager.Coroutines.Contains(handle);
			}
			return result;
		}

		// Token: 0x06001D58 RID: 7512 RVA: 0x000D15A8 File Offset: 0x000CF7A8
		public static void StopCoroutines(string name)
		{
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			lock (coroutines)
			{
				CoroutineManager.HandleCoroutineStopping((CoroutineHandle c) => c.Name == name);
				CoroutineManager.Coroutines.RemoveAll((CoroutineHandle c) => c.Name == name);
			}
		}

		// Token: 0x06001D59 RID: 7513 RVA: 0x000D1618 File Offset: 0x000CF818
		public static void StopCoroutines(CoroutineHandle handle)
		{
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			lock (coroutines)
			{
				CoroutineManager.HandleCoroutineStopping((CoroutineHandle c) => c == handle);
				CoroutineManager.Coroutines.RemoveAll((CoroutineHandle c) => c == handle);
			}
		}

		// Token: 0x06001D5A RID: 7514 RVA: 0x000D1688 File Offset: 0x000CF888
		private static void HandleCoroutineStopping(Func<CoroutineHandle, bool> filter)
		{
			foreach (CoroutineHandle coroutine in CoroutineManager.Coroutines)
			{
				if (filter(coroutine))
				{
					coroutine.AbortRequested = true;
				}
			}
		}

		// Token: 0x06001D5B RID: 7515 RVA: 0x000D16E4 File Offset: 0x000CF8E4
		private static bool PerformCoroutineStep(CoroutineHandle handle)
		{
			CoroutineStatus current = handle.Coroutine.Current;
			if (current != null)
			{
				if (current.EndsCoroutine(handle) || handle.AbortRequested)
				{
					return true;
				}
				if (!current.CheckFinished(CoroutineManager.DeltaTime))
				{
					return false;
				}
			}
			return !handle.Coroutine.MoveNext();
		}

		// Token: 0x06001D5C RID: 7516 RVA: 0x000D1734 File Offset: 0x000CF934
		private static bool IsDone(CoroutineHandle handle)
		{
			bool result;
			try
			{
				result = CoroutineManager.PerformCoroutineStep(handle);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Coroutine ",
					handle.Name,
					" threw an exception: ",
					e.Message,
					"\n",
					e.StackTrace.CleanupStackTrace()
				}), null, null, false, false);
				handle.Exception = e;
				result = true;
			}
			return result;
		}

		// Token: 0x06001D5D RID: 7517 RVA: 0x000D17B4 File Offset: 0x000CF9B4
		public static void Update(bool paused, float deltaTime)
		{
			CoroutineManager.Paused = paused;
			CoroutineManager.DeltaTime = deltaTime;
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			lock (coroutines)
			{
				CoroutineManager.coroutinePass.AddRange(CoroutineManager.Coroutines);
			}
			foreach (CoroutineHandle coroutine in CoroutineManager.coroutinePass)
			{
				if (CoroutineManager.IsDone(coroutine))
				{
					List<CoroutineHandle> coroutines2 = CoroutineManager.Coroutines;
					lock (coroutines2)
					{
						CoroutineManager.Coroutines.Remove(coroutine);
					}
				}
			}
			CoroutineManager.coroutinePass.Clear();
		}

		// Token: 0x06001D5E RID: 7518 RVA: 0x000D1890 File Offset: 0x000CFA90
		public static void ListCoroutines()
		{
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			lock (coroutines)
			{
				DebugConsole.NewMessage("***********", null, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>(CoroutineManager.Coroutines.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" coroutine(s)");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
				foreach (CoroutineHandle c in CoroutineManager.Coroutines)
				{
					DebugConsole.NewMessage("- " + c.Name, null, false);
				}
			}
		}

		// Token: 0x04000D39 RID: 3385
		private static readonly List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

		// Token: 0x04000D3C RID: 3388
		private static readonly List<CoroutineHandle> coroutinePass = new List<CoroutineHandle>();
	}
}
