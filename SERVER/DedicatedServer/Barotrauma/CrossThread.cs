using System;
using System.Collections.Generic;
using System.Threading;

namespace Barotrauma
{
	// Token: 0x020002BB RID: 699
	public static class CrossThread
	{
		// Token: 0x06002FAD RID: 12205 RVA: 0x0013D294 File Offset: 0x0013B494
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

		// Token: 0x06002FAE RID: 12206 RVA: 0x0013D330 File Offset: 0x0013B530
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

		// Token: 0x06002FAF RID: 12207 RVA: 0x0013D39C File Offset: 0x0013B59C
		public static void AddOnMainThread<T>(this List<T> list, T element)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.Add(element);
			});
		}

		// Token: 0x06002FB0 RID: 12208 RVA: 0x0013D3D0 File Offset: 0x0013B5D0
		public static void AddRangeOnMainThread<T>(this List<T> list, IEnumerable<T> elements)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.AddRange(elements);
			});
		}

		// Token: 0x06002FB1 RID: 12209 RVA: 0x0013D404 File Offset: 0x0013B604
		public static void RemoveOnMainThread<T>(this List<T> list, T element)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.Remove(element);
			});
		}

		// Token: 0x06002FB2 RID: 12210 RVA: 0x0013D438 File Offset: 0x0013B638
		public static void ClearOnMainThread<T>(this List<T> list)
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				list.Clear();
			});
		}

		// Token: 0x040017F5 RID: 6133
		private static readonly List<CrossThread.Task> enqueuedTasks = new List<CrossThread.Task>();

		// Token: 0x02000B5B RID: 2907
		// (Invoke) Token: 0x06006092 RID: 24722
		public delegate void TaskDelegate();

		// Token: 0x02000B5C RID: 2908
		private sealed class Task
		{
			// Token: 0x06006095 RID: 24725 RVA: 0x0020A8CB File Offset: 0x00208ACB
			public Task(CrossThread.TaskDelegate d)
			{
				this.Deleg = d;
				this.Mre = new ManualResetEvent(false);
				this.Done = false;
			}

			// Token: 0x06006096 RID: 24726 RVA: 0x0020A8ED File Offset: 0x00208AED
			public void PerformWait()
			{
				if (!this.Done)
				{
					this.Mre.WaitOne();
				}
			}

			// Token: 0x04003944 RID: 14660
			public CrossThread.TaskDelegate Deleg;

			// Token: 0x04003945 RID: 14661
			public ManualResetEvent Mre;

			// Token: 0x04003946 RID: 14662
			public bool Done;
		}
	}
}
