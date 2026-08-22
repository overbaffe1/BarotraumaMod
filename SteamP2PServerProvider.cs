using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma
{
	// Token: 0x020000F7 RID: 247
	internal sealed class SteamP2PServerProvider : ServerProvider
	{
		// Token: 0x0600233E RID: 9022 RVA: 0x00163BA0 File Offset: 0x00161DA0
		[NullableContext(1)]
		protected override void RetrieveServersImpl(Action<Barotrauma.Networking.ServerInfo, ServerProvider> onServerDataReceived, Action onQueryCompleted)
		{
			SteamP2PServerProvider.<>c__DisplayClass2_0 CS$<>8__locals1 = new SteamP2PServerProvider.<>c__DisplayClass2_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.onQueryCompleted = onQueryCompleted;
			CS$<>8__locals1.onServerDataReceived = onServerDataReceived;
			if (!SteamManager.IsInitialized)
			{
				CS$<>8__locals1.onQueryCompleted();
				return;
			}
			CS$<>8__locals1.selfQueryRef = new object();
			this.queryRef = CS$<>8__locals1.selfQueryRef;
			CS$<>8__locals1.lobbyQuery = SteamMatchmaking.CreateLobbyQuery().FilterDistanceWorldwide().WithMaxResults(50);
			CS$<>8__locals1.requestCount = 0;
			CS$<>8__locals1.retrieved = new HashSet<Barotrauma.Networking.SteamId>();
			CS$<>8__locals1.<RetrieveServersImpl>g__startQuery|0();
		}

		// Token: 0x0600233F RID: 9023 RVA: 0x00163C27 File Offset: 0x00161E27
		public override void Cancel()
		{
			this.queryRef = null;
		}

		// Token: 0x040011B0 RID: 4528
		[Nullable(2)]
		private object queryRef;

		// Token: 0x02000BE0 RID: 3040
		public class DataSource : Barotrauma.Networking.ServerInfo.DataSource
		{
			// Token: 0x06007A39 RID: 31289 RVA: 0x00381028 File Offset: 0x0037F228
			[NullableContext(1)]
			public override void Write(XElement element)
			{
			}

			// Token: 0x06007A3A RID: 31290 RVA: 0x0038102A File Offset: 0x0037F22A
			public DataSource(Lobby lobby)
			{
				this.Lobby = lobby;
			}

			// Token: 0x04004932 RID: 18738
			public readonly Lobby Lobby;
		}
	}
}
