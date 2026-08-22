using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020000F4 RID: 244
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class EosServerProvider : ServerProvider
	{
		// Token: 0x06002331 RID: 9009 RVA: 0x0016386C File Offset: 0x00161A6C
		protected override void RetrieveServersImpl(Action<ServerInfo, ServerProvider> onServerDataReceived, Action onQueryCompleted)
		{
			EosServerProvider.<>c__DisplayClass1_0 CS$<>8__locals1 = new EosServerProvider.<>c__DisplayClass1_0();
			CS$<>8__locals1.onQueryCompleted = onQueryCompleted;
			CS$<>8__locals1.onServerDataReceived = onServerDataReceived;
			CS$<>8__locals1.<>4__this = this;
			ImmutableArray<EosInterface.ProductUserId> loggedInPuids = EosInterface.IdQueries.GetLoggedInPuids();
			if (loggedInPuids.Length <= 0)
			{
				return;
			}
			CS$<>8__locals1.finishedTaskCount = 0;
			CS$<>8__locals1.totalTaskCount = 10;
			for (int bucketIndex = 0; bucketIndex <= 9; bucketIndex++)
			{
				EosInterface.Sessions.RemoteSession.Query query = new EosInterface.Sessions.RemoteSession.Query(bucketIndex, loggedInPuids.First<EosInterface.ProductUserId>(), 200U, ImmutableDictionary<Identifier, string>.Empty);
				TaskPool.Add("EosServerProvider.RetrieveServersImpl", query.Run(), new Action<Task>(CS$<>8__locals1.<RetrieveServersImpl>g__onTaskFinished|1));
			}
		}

		// Token: 0x06002332 RID: 9010 RVA: 0x001638F7 File Offset: 0x00161AF7
		public override void Cancel()
		{
		}

		// Token: 0x02000BD8 RID: 3032
		[Nullable(0)]
		public sealed class DataSource : ServerInfo.DataSource
		{
			// Token: 0x06007A23 RID: 31267 RVA: 0x00380A4E File Offset: 0x0037EC4E
			public DataSource(string steamPingLocation)
			{
				this.SteamPingLocation = steamPingLocation;
			}

			// Token: 0x06007A24 RID: 31268 RVA: 0x00380A5D File Offset: 0x0037EC5D
			public override void Write(XElement element)
			{
			}

			// Token: 0x0400491C RID: 18716
			public readonly string SteamPingLocation;
		}
	}
}
