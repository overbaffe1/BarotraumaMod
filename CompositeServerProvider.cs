using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020000F3 RID: 243
	[NullableContext(1)]
	[Nullable(0)]
	internal class CompositeServerProvider : ServerProvider
	{
		// Token: 0x0600232E RID: 9006 RVA: 0x001637D4 File Offset: 0x001619D4
		public CompositeServerProvider(params ServerProvider[] providers)
		{
			this.providers = providers.ToImmutableArray<ServerProvider>();
		}

		// Token: 0x0600232F RID: 9007 RVA: 0x001637F0 File Offset: 0x001619F0
		protected override void RetrieveServersImpl(Action<ServerInfo, ServerProvider> onServerDataReceived, Action onQueryCompleted)
		{
			int providersFinished = 0;
			this.providers.ForEach(delegate(ServerProvider p)
			{
				p.RetrieveServers(onServerDataReceived, new Action(base.<RetrieveServersImpl>g__ackFinishedProvider|0));
			});
		}

		// Token: 0x06002330 RID: 9008 RVA: 0x0016383B File Offset: 0x00161A3B
		public override void Cancel()
		{
			this.providers.ForEach(delegate(ServerProvider p)
			{
				p.Cancel();
			});
		}

		// Token: 0x040011AD RID: 4525
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<ServerProvider> providers;
	}
}
