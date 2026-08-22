using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Barotrauma.Eos
{
	// Token: 0x0200062A RID: 1578
	[NullableContext(1)]
	[Nullable(0)]
	internal static class EosEpicPrimaryLogin
	{
		// Token: 0x060064D7 RID: 25815 RVA: 0x0034291A File Offset: 0x00340B1A
		public static void Start(string exchangeCode)
		{
			TaskPool.Add("Eos.Core.LoginEpic", EosEpicPrimaryLogin.Initialize(exchangeCode), delegate(Task t)
			{
				Action action;
				if (!t.TryGetResult(out action))
				{
					return;
				}
				action();
			});
		}

		// Token: 0x060064D8 RID: 25816 RVA: 0x0034294C File Offset: 0x00340B4C
		private static void Success()
		{
			EosAccount.CloseMessageBox();
			EosAccount.RefreshSelfAccountIds(null);
			EosAccount.OnLoginSuccess();
		}

		// Token: 0x060064D9 RID: 25817 RVA: 0x00342960 File Offset: 0x00340B60
		private static Task<Action> Initialize(string exchangeCode)
		{
			EosEpicPrimaryLogin.<Initialize>d__2 <Initialize>d__;
			<Initialize>d__.<>t__builder = AsyncTaskMethodBuilder<Action>.Create();
			<Initialize>d__.exchangeCode = exchangeCode;
			<Initialize>d__.<>1__state = -1;
			<Initialize>d__.<>t__builder.Start<EosEpicPrimaryLogin.<Initialize>d__2>(ref <Initialize>d__);
			return <Initialize>d__.<>t__builder.Task;
		}

		// Token: 0x060064DA RID: 25818 RVA: 0x003429A3 File Offset: 0x00340BA3
		[CompilerGenerated]
		internal static void <Initialize>g__cancel|2_1()
		{
			EosInterface.Core.CleanupAndQuit();
		}

		// Token: 0x020014B0 RID: 5296
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040066A6 RID: 26278
			[Nullable(0)]
			public static Action <0>__cancel;

			// Token: 0x040066A7 RID: 26279
			[Nullable(0)]
			public static Action <1>__Success;
		}
	}
}
