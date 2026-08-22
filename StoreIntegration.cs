using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Barotrauma.Steam;

namespace Barotrauma
{
	// Token: 0x02000142 RID: 322
	internal static class StoreIntegration
	{
		// Token: 0x17000AA6 RID: 2726
		// (get) Token: 0x060029B6 RID: 10678 RVA: 0x001CEDC1 File Offset: 0x001CCFC1
		// (set) Token: 0x060029B7 RID: 10679 RVA: 0x001CEDC8 File Offset: 0x001CCFC8
		public static StoreIntegration.Store CurrentStore { get; private set; }

		// Token: 0x060029B8 RID: 10680 RVA: 0x001CEDD0 File Offset: 0x001CCFD0
		public static void Init(ref string[] programArgs)
		{
			if (EosInterface.Login.ParseEgsExchangeCode(programArgs).IsNone() && SteamManager.SteamworksLibExists)
			{
				SteamManager.Initialize();
				StoreIntegration.CurrentStore = StoreIntegration.Store.Steam;
				return;
			}
			EosInterface.Core.InitError initError;
			if (EosInterface.Core.Init(EosInterface.ApplicationCredentials.Client, RuntimeInformation.IsOSPlatform(OSPlatform.Windows)).TryUnwrapFailure(out initError))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
				defaultInterpolatedStringHandler.AppendLiteral("EOS failed to initialize: ");
				defaultInterpolatedStringHandler.AppendFormatted<EosInterface.Core.InitError>(initError);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			StoreIntegration.CurrentStore = StoreIntegration.Store.Epic;
		}

		// Token: 0x02000DB1 RID: 3505
		public enum Store
		{
			// Token: 0x04005043 RID: 20547
			None,
			// Token: 0x04005044 RID: 20548
			Steam,
			// Token: 0x04005045 RID: 20549
			Epic
		}
	}
}
