using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200039C RID: 924
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class LidgrenEndpoint : Endpoint
	{
		// Token: 0x17000F13 RID: 3859
		// (get) Token: 0x06003674 RID: 13940 RVA: 0x00173F72 File Offset: 0x00172172
		public int Port
		{
			get
			{
				return this.NetEndpoint.Port;
			}
		}

		// Token: 0x17000F14 RID: 3860
		// (get) Token: 0x06003675 RID: 13941 RVA: 0x00173F7F File Offset: 0x0017217F
		public override string StringRepresentation
		{
			get
			{
				return this.NetEndpoint.ToString();
			}
		}

		// Token: 0x17000F15 RID: 3861
		// (get) Token: 0x06003676 RID: 13942 RVA: 0x00173F8C File Offset: 0x0017218C
		public override LocalizedString ServerTypeString { get; } = TextManager.Get("DedicatedServer");

		// Token: 0x06003677 RID: 13943 RVA: 0x00173F94 File Offset: 0x00172194
		public LidgrenEndpoint(IPAddress address, int port) : this(new IPEndPoint(address, port))
		{
		}

		// Token: 0x06003678 RID: 13944 RVA: 0x00173FA4 File Offset: 0x001721A4
		public LidgrenEndpoint(IPEndPoint netEndpoint) : base(new LidgrenAddress(netEndpoint.Address))
		{
			this.NetEndpoint = new IPEndPoint((this.Address as LidgrenAddress).NetAddress, netEndpoint.Port);
		}

		// Token: 0x06003679 RID: 13945 RVA: 0x00173FF3 File Offset: 0x001721F3
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public new static Option<LidgrenEndpoint> Parse(string endpointStr)
		{
			return LidgrenEndpoint.ParseFromWithHostNameCheck(endpointStr, false);
		}

		// Token: 0x0600367A RID: 13946 RVA: 0x00173FFC File Offset: 0x001721FC
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

		// Token: 0x0600367B RID: 13947 RVA: 0x001740B4 File Offset: 0x001722B4
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			LidgrenEndpoint otherEndpoint = obj as LidgrenEndpoint;
			return otherEndpoint != null && this == otherEndpoint;
		}

		// Token: 0x0600367C RID: 13948 RVA: 0x001740D8 File Offset: 0x001722D8
		public override int GetHashCode()
		{
			return this.NetEndpoint.GetHashCode();
		}

		// Token: 0x0600367D RID: 13949 RVA: 0x001740E5 File Offset: 0x001722E5
		public static bool operator ==(LidgrenEndpoint a, LidgrenEndpoint b)
		{
			return a.NetEndpoint.EquivalentTo(b.NetEndpoint);
		}

		// Token: 0x0600367E RID: 13950 RVA: 0x001740F8 File Offset: 0x001722F8
		public static bool operator !=(LidgrenEndpoint a, LidgrenEndpoint b)
		{
			return !(a == b);
		}

		// Token: 0x04001BBA RID: 7098
		public readonly IPEndPoint NetEndpoint;
	}
}
