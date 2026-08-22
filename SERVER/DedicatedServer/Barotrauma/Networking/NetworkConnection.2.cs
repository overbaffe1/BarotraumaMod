using System;

namespace Barotrauma.Networking
{
	// Token: 0x020003AF RID: 943
	public abstract class NetworkConnection
	{
		// Token: 0x17000F38 RID: 3896
		// (get) Token: 0x0600376A RID: 14186 RVA: 0x00175B38 File Offset: 0x00173D38
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

		// Token: 0x17000F39 RID: 3897
		// (get) Token: 0x0600376B RID: 14187 RVA: 0x00175B94 File Offset: 0x00173D94
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

		// Token: 0x17000F3A RID: 3898
		// (get) Token: 0x0600376C RID: 14188 RVA: 0x00175BEF File Offset: 0x00173DEF
		// (set) Token: 0x0600376D RID: 14189 RVA: 0x00175BF7 File Offset: 0x00173DF7
		public AccountInfo AccountInfo { get; private set; } = AccountInfo.None;

		// Token: 0x17000F3B RID: 3899
		// (get) Token: 0x0600376E RID: 14190 RVA: 0x00175C00 File Offset: 0x00173E00
		// (set) Token: 0x0600376F RID: 14191 RVA: 0x00175C08 File Offset: 0x00173E08
		[Obsolete("TODO: this doesn't belong in layer 1")]
		public LanguageIdentifier Language { get; set; }

		// Token: 0x06003770 RID: 14192 RVA: 0x00175C11 File Offset: 0x00173E11
		protected NetworkConnection(Endpoint endpoint)
		{
			this.Endpoint = endpoint;
		}

		// Token: 0x06003771 RID: 14193 RVA: 0x00175C32 File Offset: 0x00173E32
		public bool EndpointMatches(Endpoint endPoint)
		{
			return this.Endpoint == endPoint;
		}

		// Token: 0x06003772 RID: 14194
		public abstract bool AddressMatches(NetworkConnection other);

		// Token: 0x06003773 RID: 14195 RVA: 0x00175C40 File Offset: 0x00173E40
		public void SetAccountInfo(AccountInfo newInfo)
		{
			if (this.AccountInfo.IsNone)
			{
				this.AccountInfo = newInfo;
			}
		}

		// Token: 0x06003774 RID: 14196 RVA: 0x00175C64 File Offset: 0x00173E64
		public sealed override string ToString()
		{
			return this.Endpoint.StringRepresentation;
		}

		// Token: 0x04001BD8 RID: 7128
		public readonly Endpoint Endpoint;

		// Token: 0x04001BDA RID: 7130
		public NetworkConnectionStatus Status = NetworkConnectionStatus.Disconnected;
	}
}
