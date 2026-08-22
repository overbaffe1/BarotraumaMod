using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020003BA RID: 954
	[NetworkSerialize(38)]
	internal readonly struct P2POwnerToServerHeader : INetSerializableStruct, IEquatable<P2POwnerToServerHeader>
	{
		// Token: 0x0600378E RID: 14222 RVA: 0x00175E1E File Offset: 0x0017401E
		[NullableContext(2)]
		public P2POwnerToServerHeader(string EndpointStr, AccountInfo AccountInfo)
		{
			this.EndpointStr = EndpointStr;
			this.AccountInfo = AccountInfo;
		}

		// Token: 0x17000F3F RID: 3903
		// (get) Token: 0x0600378F RID: 14223 RVA: 0x00175E2E File Offset: 0x0017402E
		// (set) Token: 0x06003790 RID: 14224 RVA: 0x00175E36 File Offset: 0x00174036
		[Nullable(2)]
		public string EndpointStr { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x17000F40 RID: 3904
		// (get) Token: 0x06003791 RID: 14225 RVA: 0x00175E3F File Offset: 0x0017403F
		// (set) Token: 0x06003792 RID: 14226 RVA: 0x00175E47 File Offset: 0x00174047
		public AccountInfo AccountInfo { get; set; }

		// Token: 0x17000F41 RID: 3905
		// (get) Token: 0x06003793 RID: 14227 RVA: 0x00175E50 File Offset: 0x00174050
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

		// Token: 0x06003794 RID: 14228 RVA: 0x00175E68 File Offset: 0x00174068
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("P2POwnerToServerHeader");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003795 RID: 14229 RVA: 0x00175EB4 File Offset: 0x001740B4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("EndpointStr = ");
			builder.Append(this.EndpointStr);
			builder.Append(", AccountInfo = ");
			builder.Append(this.AccountInfo.ToString());
			builder.Append(", Endpoint = ");
			builder.Append(this.Endpoint.ToString());
			return true;
		}

		// Token: 0x06003796 RID: 14230 RVA: 0x00175F29 File Offset: 0x00174129
		[CompilerGenerated]
		public static bool operator !=(P2POwnerToServerHeader left, P2POwnerToServerHeader right)
		{
			return !(left == right);
		}

		// Token: 0x06003797 RID: 14231 RVA: 0x00175F35 File Offset: 0x00174135
		[CompilerGenerated]
		public static bool operator ==(P2POwnerToServerHeader left, P2POwnerToServerHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003798 RID: 14232 RVA: 0x00175F3F File Offset: 0x0017413F
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<EndpointStr>k__BackingField) * -1521134295 + EqualityComparer<AccountInfo>.Default.GetHashCode(this.<AccountInfo>k__BackingField);
		}

		// Token: 0x06003799 RID: 14233 RVA: 0x00175F68 File Offset: 0x00174168
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is P2POwnerToServerHeader && this.Equals((P2POwnerToServerHeader)obj);
		}

		// Token: 0x0600379A RID: 14234 RVA: 0x00175F80 File Offset: 0x00174180
		[CompilerGenerated]
		public bool Equals(P2POwnerToServerHeader other)
		{
			return EqualityComparer<string>.Default.Equals(this.<EndpointStr>k__BackingField, other.<EndpointStr>k__BackingField) && EqualityComparer<AccountInfo>.Default.Equals(this.<AccountInfo>k__BackingField, other.<AccountInfo>k__BackingField);
		}

		// Token: 0x0600379B RID: 14235 RVA: 0x00175FB2 File Offset: 0x001741B2
		[NullableContext(2)]
		[CompilerGenerated]
		public void Deconstruct(out string EndpointStr, out AccountInfo AccountInfo)
		{
			EndpointStr = this.EndpointStr;
			AccountInfo = this.AccountInfo;
		}
	}
}
