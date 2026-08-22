using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Barotrauma
{
	// Token: 0x0200039D RID: 925
	public static class TaskExtensions
	{
		// Token: 0x0600452E RID: 17710 RVA: 0x00267ECC File Offset: 0x002660CC
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
