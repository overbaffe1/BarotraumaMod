using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000154 RID: 340
	internal sealed class P2POwnerDoSProtection
	{
		// Token: 0x06002A1C RID: 10780 RVA: 0x001D17FA File Offset: 0x001CF9FA
		public P2POwnerDoSProtection(P2POwnerDoSProtection.ExcessivePacketDelegate onExcessivePackets)
		{
			this.onExcessivePackets = onExcessivePackets;
			this.nextCheckTime = Timing.TotalTime + 10.0;
		}

		// Token: 0x17000AB8 RID: 2744
		// (get) Token: 0x06002A1D RID: 10781 RVA: 0x001D1834 File Offset: 0x001CFA34
		private static int MaxPacketCount
		{
			get
			{
				GameClient client = GameMain.Client;
				ServerSettings serverSettings = (client != null) ? client.ServerSettings : null;
				if (serverSettings == null)
				{
					return (int)MathF.Ceiling(800.00006f);
				}
				return (int)MathF.Ceiling((float)serverSettings.MaxPacketAmount * MathF.Max((float)serverSettings.TickRate / 20f, 1f) * 0.20000002f);
			}
		}

		// Token: 0x06002A1E RID: 10782 RVA: 0x001D1890 File Offset: 0x001CFA90
		private static bool ShouldCheck()
		{
			GameClient client = GameMain.Client;
			ServerSettings serverSettings = (client != null) ? client.ServerSettings : null;
			return serverSettings != null && serverSettings.EnableDoSProtection && serverSettings.MaxPacketAmount > 1200;
		}

		// Token: 0x06002A1F RID: 10783 RVA: 0x001D18CC File Offset: 0x001CFACC
		public void OnPacket(P2PEndpoint endpoint)
		{
			if (!P2POwnerDoSProtection.ShouldCheck())
			{
				return;
			}
			int count;
			this.packetCounts.TryGetValue(endpoint, out count);
			count = (this.packetCounts[endpoint] = count + 1);
			if (Timing.TotalTime > this.nextCheckTime)
			{
				foreach (P2PEndpoint e in this.packetCounts.Keys.ToArray<P2PEndpoint>())
				{
					this.CheckForExcessivePackets(e, count);
				}
				this.packetCounts.Clear();
				this.nextCheckTime = Timing.TotalTime + 10.0;
			}
		}

		// Token: 0x06002A20 RID: 10784 RVA: 0x001D195C File Offset: 0x001CFB5C
		private void CheckForExcessivePackets(P2PEndpoint endpoint, int count)
		{
			if (count > P2POwnerDoSProtection.MaxPacketCount)
			{
				int kickCount;
				this.kicksByEndpoint.TryGetValue(endpoint, out kickCount);
				kickCount = (this.kicksByEndpoint[endpoint] = kickCount + 1);
				this.onExcessivePackets(endpoint, kickCount > 3);
				this.packetCounts.Remove(endpoint);
			}
		}

		// Token: 0x04001608 RID: 5640
		private readonly Dictionary<P2PEndpoint, int> packetCounts = new Dictionary<P2PEndpoint, int>();

		// Token: 0x04001609 RID: 5641
		private readonly Dictionary<P2PEndpoint, int> kicksByEndpoint = new Dictionary<P2PEndpoint, int>();

		// Token: 0x0400160A RID: 5642
		private readonly P2POwnerDoSProtection.ExcessivePacketDelegate onExcessivePackets;

		// Token: 0x0400160B RID: 5643
		private double nextCheckTime;

		// Token: 0x0400160C RID: 5644
		private const int PacketCheckTimer = 10;

		// Token: 0x02000DC4 RID: 3524
		// (Invoke) Token: 0x06008247 RID: 33351
		public delegate void ExcessivePacketDelegate(P2PEndpoint endpoint, bool shouldBan);
	}
}
