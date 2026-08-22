using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SharpDX;

namespace Barotrauma
{
	// Token: 0x0200025F RID: 607
	[NullableContext(1)]
	[Nullable(0)]
	internal static class CoroutineManager
	{
		// Token: 0x17000EB7 RID: 3767
		// (get) Token: 0x06003832 RID: 14386 RVA: 0x00217089 File Offset: 0x00215289
		// (set) Token: 0x06003833 RID: 14387 RVA: 0x00217090 File Offset: 0x00215290
		public static float DeltaTime { get; private set; }

		// Token: 0x17000EB8 RID: 3768
		// (get) Token: 0x06003834 RID: 14388 RVA: 0x00217098 File Offset: 0x00215298
		// (set) Token: 0x06003835 RID: 14389 RVA: 0x0021709F File Offset: 0x0021529F
		public static bool Paused { get; private set; }

		// Token: 0x06003836 RID: 14390 RVA: 0x002170A8 File Offset: 0x002152A8
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

		// Token: 0x06003837 RID: 14391 RVA: 0x002170FC File Offset: 0x002152FC
		public static CoroutineHandle Invoke(Action action, float delay = 0f)
		{
			return CoroutineManager.StartCoroutine(CoroutineManager.DoInvokeAfter(action, delay), "");
		}

		// Token: 0x06003838 RID: 14392 RVA: 0x0021710F File Offset: 0x0021530F
		private static IEnumerable<CoroutineStatus> DoInvokeAfter([Nullable(2)] Action action, float delay)
		{
			CoroutineManager.<DoInvokeAfter>d__11 <DoInvokeAfter>d__ = new CoroutineManager.<DoInvokeAfter>d__11(-2);
			<DoInvokeAfter>d__.<>3__action = action;
			<DoInvokeAfter>d__.<>3__delay = delay;
			return <DoInvokeAfter>d__;
		}

		// Token: 0x06003839 RID: 14393 RVA: 0x00217128 File Offset: 0x00215328
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

		// Token: 0x0600383A RID: 14394 RVA: 0x00217188 File Offset: 0x00215388
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

		// Token: 0x0600383B RID: 14395 RVA: 0x002171D0 File Offset: 0x002153D0
		public static void StopCoroutines(string name)
		{
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			lock (coroutines)
			{
				CoroutineManager.HandleCoroutineStopping((CoroutineHandle c) => c.Name == name);
				CoroutineManager.Coroutines.RemoveAll((CoroutineHandle c) => c.Name == name);
			}
		}

		// Token: 0x0600383C RID: 14396 RVA: 0x00217240 File Offset: 0x00215440
		public static void StopCoroutines(CoroutineHandle handle)
		{
			List<CoroutineHandle> coroutines = CoroutineManager.Coroutines;
			lock (coroutines)
			{
				CoroutineManager.HandleCoroutineStopping((CoroutineHandle c) => c == handle);
				CoroutineManager.Coroutines.RemoveAll((CoroutineHandle c) => c == handle);
			}
		}

		// Token: 0x0600383D RID: 14397 RVA: 0x002172B0 File Offset: 0x002154B0
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

		// Token: 0x0600383E RID: 14398 RVA: 0x0021730C File Offset: 0x0021550C
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

		// Token: 0x0600383F RID: 14399 RVA: 0x0021735C File Offset: 0x0021555C
		private static bool IsDone(CoroutineHandle handle)
		{
			bool result;
			try
			{
				result = CoroutineManager.PerformCoroutineStep(handle);
			}
			catch (Exception e)
			{
				if (e is SharpDXException)
				{
					throw;
				}
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

		// Token: 0x06003840 RID: 14400 RVA: 0x002173E8 File Offset: 0x002155E8
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

		// Token: 0x06003841 RID: 14401 RVA: 0x002174C4 File Offset: 0x002156C4
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

		// Token: 0x04001C3E RID: 7230
		private static readonly List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

		// Token: 0x04001C41 RID: 7233
		private static readonly List<CoroutineHandle> coroutinePass = new List<CoroutineHandle>();
	}
}
