using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020003BB RID: 955
	[NetworkSerialize(45)]
	internal readonly struct P2PServerToOwnerHeader : INetSerializableStruct, IEquatable<P2PServerToOwnerHeader>
	{
		// Token: 0x0600379C RID: 14236 RVA: 0x00175FC8 File Offset: 0x001741C8
		[NullableContext(2)]
		public P2PServerToOwnerHeader(string EndpointStr)
		{
			this.EndpointStr = EndpointStr;
		}

		// Token: 0x17000F42 RID: 3906
		// (get) Token: 0x0600379D RID: 14237 RVA: 0x00175FD1 File Offset: 0x001741D1
		// (set) Token: 0x0600379E RID: 14238 RVA: 0x00175FD9 File Offset: 0x001741D9
		[Nullable(2)]
		public string EndpointStr { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000F43 RID: 3907
		// (get) Token: 0x0600379F RID: 14239 RVA: 0x00175FE2 File Offset: 0x001741E2
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<P2PEndpoint> Endpoint
		{
			[return: Nullable(new byte[]
			{
				0,
				1
			})]
			get
			{
				return P2PEndpoint.Parse(this.EndpointStr ?? "");
			}
		}

		// Token: 0x060037A0 RID: 14240 RVA: 0x00175FF8 File Offset: 0x001741F8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("P2PServerToOwnerHeader");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060037A1 RID: 14241 RVA: 0x00176044 File Offset: 0x00174244
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("EndpointStr = ");
			builder.Append(this.EndpointStr);
			builder.Append(", Endpoint = ");
			builder.Append(this.Endpoint.ToString());
			return true;
		}

		// Token: 0x060037A2 RID: 14242 RVA: 0x00176092 File Offset: 0x00174292
		[CompilerGenerated]
		public static bool operator !=(P2PServerToOwnerHeader left, P2PServerToOwnerHeader right)
		{
			return !(left == right);
		}

		// Token: 0x060037A3 RID: 14243 RVA: 0x0017609E File Offset: 0x0017429E
		[CompilerGenerated]
		public static bool operator ==(P2PServerToOwnerHeader left, P2PServerToOwnerHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x060037A4 RID: 14244 RVA: 0x001760A8 File Offset: 0x001742A8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<EndpointStr>k__BackingField);
		}

		// Token: 0x060037A5 RID: 14245 RVA: 0x001760BA File Offset: 0x001742BA
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is P2PServerToOwnerHeader && this.Equals((P2PServerToOwnerHeader)obj);
		}

		// Token: 0x060037A6 RID: 14246 RVA: 0x001760D2 File Offset: 0x001742D2
		[CompilerGenerated]
		public bool Equals(P2PServerToOwnerHeader other)
		{
			return EqualityComparer<string>.Default.Equals(this.<EndpointStr>k__BackingField, other.<EndpointStr>k__BackingField);
		}

		// Token: 0x060037A7 RID: 14247 RVA: 0x001760EA File Offset: 0x001742EA
		[NullableContext(2)]
		[CompilerGenerated]
		public void Deconstruct(out string EndpointStr)
		{
			EndpointStr = this.EndpointStr;
		}
	}
}
