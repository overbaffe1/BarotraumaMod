using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000499 RID: 1177
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class LidgrenEndpoint : Endpoint
	{
		// Token: 0x1700140E RID: 5134
		// (get) Token: 0x06004E49 RID: 20041 RVA: 0x002ACAAE File Offset: 0x002AACAE
		public int Port
		{
			get
			{
				return this.NetEndpoint.Port;
			}
		}

		// Token: 0x1700140F RID: 5135
		// (get) Token: 0x06004E4A RID: 20042 RVA: 0x002ACABB File Offset: 0x002AACBB
		public override string StringRepresentation
		{
			get
			{
				return this.NetEndpoint.ToString();
			}
		}

		// Token: 0x17001410 RID: 5136
		// (get) Token: 0x06004E4B RID: 20043 RVA: 0x002ACAC8 File Offset: 0x002AACC8
		public override LocalizedString ServerTypeString { get; } = TextManager.Get("DedicatedServer");

		// Token: 0x06004E4C RID: 20044 RVA: 0x002ACAD0 File Offset: 0x002AACD0
		public LidgrenEndpoint(IPAddress address, int port) : this(new IPEndPoint(address, port))
		{
		}

		// Token: 0x06004E4D RID: 20045 RVA: 0x002ACAE0 File Offset: 0x002AACE0
		public LidgrenEndpoint(IPEndPoint netEndpoint) : base(new LidgrenAddress(netEndpoint.Address))
		{
			this.NetEndpoint = new IPEndPoint((this.Address as LidgrenAddress).NetAddress, netEndpoint.Port);
		}

		// Token: 0x06004E4E RID: 20046 RVA: 0x002ACB2F File Offset: 0x002AAD2F
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public new static Option<LidgrenEndpoint> Parse(string endpointStr)
		{
			return LidgrenEndpoint.ParseFromWithHostNameCheck(endpointStr, false);
		}

		// Token: 0x06004E4F RID: 20047 RVA: 0x002ACB38 File Offset: 0x002AAD38
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<LidgrenEndpoint> ParseFromWithHostNameCheck(string endpointStr, bool tryParseHostName)
		{
			string hostName = endpointStr;
			int port = 27015;
			if (endpointStr.Count((char c) => c == ':') == 1)
			{
				string[] split = endpointStr.Split(':', StringSplitOptions.None);
				hostName = split[0];
				int tmpPort;
				port = (int.TryParse(split[1], out tmpPort) ? tmpPort : port);
			}
			LidgrenAddress adr;
			if (LidgrenAddress.Parse(hostName).TryUnwrap(out adr) || (tryParseHostName && LidgrenAddress.ParseHostName(hostName).TryUnwrap(out adr)))
			{
				return Option<LidgrenEndpoint>.Some(new LidgrenEndpoint(adr.NetAddress, port));
			}
			IPEndPoint netEndpoint;
			if (!IPEndPoint.TryParse(endpointStr, out netEndpoint))
			{
				return Option<LidgrenEndpoint>.None();
			}
			return Option<LidgrenEndpoint>.Some(new LidgrenEndpoint(netEndpoint));
		}

		// Token: 0x06004E50 RID: 20048 RVA: 0x002ACBF0 File Offset: 0x002AADF0
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			LidgrenEndpoint otherEndpoint = obj as LidgrenEndpoint;
			return otherEndpoint != null && this == otherEndpoint;
		}

		// Token: 0x06004E51 RID: 20049 RVA: 0x002ACC14 File Offset: 0x002AAE14
		public override int GetHashCode()
		{
			return this.NetEndpoint.GetHashCode();
		}

		// Token: 0x06004E52 RID: 20050 RVA: 0x002ACC21 File Offset: 0x002AAE21
		public static bool operator ==(LidgrenEndpoint a, LidgrenEndpoint b)
		{
			return a.NetEndpoint.EquivalentTo(b.NetEndpoint);
		}

		// Token: 0x06004E53 RID: 20051 RVA: 0x002ACC34 File Offset: 0x002AAE34
		public static bool operator !=(LidgrenEndpoint a, LidgrenEndpoint b)
		{
			return !(a == b);
		}

		// Token: 0x040029B1 RID: 10673
		public readonly IPEndPoint NetEndpoint;
	}
}
