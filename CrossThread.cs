using System;
using System.Collections.Generic;
using System.Threading;

namespace Barotrauma
{
	// Token: 0x02000386 RID: 902
	public static class CrossThread
	{
		// Token: 0x0600442B RID: 17451 RVA: 0x00256D38 File Offset: 0x00254F38
		public static void ProcessTasks()
		{
			List<CrossThread.Task> obj = CrossThread.enqueuedTasks;
			lock (obj)
			{
				foreach (CrossThread.Task task in CrossThread.enqueuedTasks)
				{
					task.Deleg();
					task.Mre.Set();
					task.Done = true;
				}
				CrossThread.enqueuedTasks.Clear();
			}
		}

		// Token: 0x0600442C RID: 17452 RVA: 0x00256DD4 File Offset: 0x00254FD4
		public static void RequestExecutionOnMainThread(CrossThread.TaskDelegate deleg)
		{
			if (GameMain.MainThread == null || Thread.CurrentThread == GameMain.MainThread)
			{
				deleg();
				return;
			}
			CrossThread.Task newTask = new CrossThread.Task(deleg);
			List<CrossThread.Task> obj = CrossThread.enqueuedTasks;
			lock (obj)
			{
				CrossThread.enqueuedTasks.Add(newTask);
			}
			newTask.PerformWait();
		}

		// Token: 0x0600442D RID: 17453 RVA: 0x00256E40 File Offset: 0x00255040
		public static void AddOnMainThread<T>(this List<T> list, T element)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.Add(element);
			});
		}

		// Token: 0x0600442E RID: 17454 RVA: 0x00256E74 File Offset: 0x00255074
		public static void AddRangeOnMainThread<T>(this List<T> list, IEnumerable<T> elements)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.AddRange(elements);
			});
		}

		// Token: 0x0600442F RID: 17455 RVA: 0x00256EA8 File Offset: 0x002550A8
		public static void RemoveOnMainThread<T>(this List<T> list, T element)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.Remove(element);
			});
		}

		// Token: 0x06004430 RID: 17456 RVA: 0x00256EDC File Offset: 0x002550DC
		public static void ClearOnMainThread<T>(this List<T> list)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.Clear();
			});
		}

		// Token: 0x040023C7 RID: 9159
		private static readonly List<CrossThread.Task> enqueuedTasks = new List<CrossThread.Task>();

		// Token: 0x020010AC RID: 4268
		// (Invoke) Token: 0x06008D78 RID: 36216
		public delegate void TaskDelegate();

		// Token: 0x020010AD RID: 4269
		private sealed class Task
		{
			// Token: 0x06008D7B RID: 36219 RVA: 0x003B2160 File Offset: 0x003B0360
			public Task(CrossThread.TaskDelegate d)
			{
				this.Deleg = d;
				this.Mre = new ManualResetEvent(false);
				this.Done = false;
			}

			// Token: 0x06008D7C RID: 36220 RVA: 0x003B2182 File Offset: 0x003B0382
			public void PerformWait()
			{
				if (!this.Done)
				{
					this.Mre.WaitOne();
				}
			}

			// Token: 0x04005961 RID: 22881
			public CrossThread.TaskDelegate Deleg;

			// Token: 0x04005962 RID: 22882
			public ManualResetEvent Mre;

			// Token: 0x04005963 RID: 22883
			public bool Done;
		}
	}
}
