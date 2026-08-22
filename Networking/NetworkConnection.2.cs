using System;

namespace Barotrauma.Networking
{
	// Token: 0x020004AC RID: 1196
	public abstract class NetworkConnection
	{
		// Token: 0x17001433 RID: 5171
		// (get) Token: 0x06004F3F RID: 20287 RVA: 0x002AE674 File Offset: 0x002AC874
		public static double TimeoutThresholdNotInGame
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				float? num;
				if (networkMember == null)
				{
					num = null;
				}
				else
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					num = ((serverSettings != null) ? new float?(serverSettings.TimeoutThresholdNotInGame) : null);
				}
				float? num2 = num;
				if (num2 == null)
				{
					return 60.0;
				}
				return (double)num2.GetValueOrDefault();
			}
		}

		// Token: 0x17001434 RID: 5172
		// (get) Token: 0x06004F40 RID: 20288 RVA: 0x002AE6D0 File Offset: 0x002AC8D0
		public static double TimeoutThresholdInGame
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				float? num;
				if (networkMember == null)
				{
					num = null;
				}
				else
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					num = ((serverSettings != null) ? new float?(serverSettings.TimeoutThresholdInGame) : null);
				}
				float? num2 = num;
				if (num2 == null)
				{
					return 10.0;
				}
				return (double)num2.GetValueOrDefault();
			}
		}

		// Token: 0x17001435 RID: 5173
		// (get) Token: 0x06004F41 RID: 20289 RVA: 0x002AE72B File Offset: 0x002AC92B
		// (set) Token: 0x06004F42 RID: 20290 RVA: 0x002AE733 File Offset: 0x002AC933
		public AccountInfo AccountInfo { get; private set; } = AccountInfo.None;

		// Token: 0x17001436 RID: 5174
		// (get) Token: 0x06004F43 RID: 20291 RVA: 0x002AE73C File Offset: 0x002AC93C
		// (set) Token: 0x06004F44 RID: 20292 RVA: 0x002AE744 File Offset: 0x002AC944
		[Obsolete("TODO: this doesn't belong in layer 1")]
		public LanguageIdentifier Language { get; set; }

		// Token: 0x06004F45 RID: 20293 RVA: 0x002AE74D File Offset: 0x002AC94D
		protected NetworkConnection(Endpoint endpoint)
		{
			this.Endpoint = endpoint;
		}

		// Token: 0x06004F46 RID: 20294 RVA: 0x002AE76E File Offset: 0x002AC96E
		public bool EndpointMatches(Endpoint endPoint)
		{
			return this.Endpoint == endPoint;
		}

		// Token: 0x06004F47 RID: 20295
		public abstract bool AddressMatches(NetworkConnection other);

		// Token: 0x06004F48 RID: 20296 RVA: 0x002AE77C File Offset: 0x002AC97C
		public void SetAccountInfo(AccountInfo newInfo)
		{
			if (this.AccountInfo.IsNone)
			{
				this.AccountInfo = newInfo;
			}
		}

		// Token: 0x06004F49 RID: 20297 RVA: 0x002AE7A0 File Offset: 0x002AC9A0
		public sealed override string ToString()
		{
			return this.Endpoint.StringRepresentation;
		}

		// Token: 0x040029CF RID: 10703
		public readonly Endpoint Endpoint;

		// Token: 0x040029D1 RID: 10705
		public NetworkConnectionStatus Status = NetworkConnectionStatus.Disconnected;
	}
}
