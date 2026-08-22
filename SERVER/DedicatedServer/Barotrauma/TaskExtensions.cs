using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Barotrauma
{
	// Token: 0x020002D2 RID: 722
	public static class TaskExtensions
	{
		// Token: 0x060030B0 RID: 12464 RVA: 0x0014E470 File Offset: 0x0014C670
		[NullableContext(1)]
		public static Task<T> WaitForLoadingScreen<[Nullable(2)] T>(this Task<T> task)
		{
			TaskExtensions.<WaitForLoadingScreen>d__0<T> <WaitForLoadingScreen>d__;
			<WaitForLoadingScreen>d__.<>t__builder = AsyncTaskMethodBuilder<T>.Create();
			<WaitForLoadingScreen>d__.task = task;
			<WaitForLoadingScreen>d__.<>1__state = -1;
			<WaitForLoadingScreen>d__.<>t__builder.Start<TaskExtensions.<WaitForLoadingScreen>d__0<T>>(ref <WaitForLoadingScreen>d__);
			return <WaitForLoadingScreen>d__.<>t__builder.Task;
		}
	}
}
