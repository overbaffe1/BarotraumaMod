using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020004B7 RID: 1207
	[NetworkSerialize(38)]
	internal readonly struct P2POwnerToServerHeader : INetSerializableStruct, IEquatable<P2POwnerToServerHeader>
	{
		// Token: 0x06004F63 RID: 20323 RVA: 0x002AE95A File Offset: 0x002ACB5A
		[NullableContext(2)]
		public P2POwnerToServerHeader(string EndpointStr, AccountInfo AccountInfo)
		{
			this.EndpointStr = EndpointStr;
			this.AccountInfo = AccountInfo;
		}

		// Token: 0x1700143A RID: 5178
		// (get) Token: 0x06004F64 RID: 20324 RVA: 0x002AE96A File Offset: 0x002ACB6A
		// (set) Token: 0x06004F65 RID: 20325 RVA: 0x002AE972 File Offset: 0x002ACB72
		[Nullable(2)]
		public string EndpointStr { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x1700143B RID: 5179
		// (get) Token: 0x06004F66 RID: 20326 RVA: 0x002AE97B File Offset: 0x002ACB7B
		// (set) Token: 0x06004F67 RID: 20327 RVA: 0x002AE983 File Offset: 0x002ACB83
		public AccountInfo AccountInfo { get; set; }

		// Token: 0x1700143C RID: 5180
		// (get) Token: 0x06004F68 RID: 20328 RVA: 0x002AE98C File Offset: 0x002ACB8C
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

		// Token: 0x06004F69 RID: 20329 RVA: 0x002AE9A4 File Offset: 0x002ACBA4
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

		// Token: 0x06004F6A RID: 20330 RVA: 0x002AE9F0 File Offset: 0x002ACBF0
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

		// Token: 0x06004F6B RID: 20331 RVA: 0x002AEA65 File Offset: 0x002ACC65
		[CompilerGenerated]
		public static bool operator !=(P2POwnerToServerHeader left, P2POwnerToServerHeader right)
		{
			return !(left == right);
		}

		// Token: 0x06004F6C RID: 20332 RVA: 0x002AEA71 File Offset: 0x002ACC71
		[CompilerGenerated]
		public static bool operator ==(P2POwnerToServerHeader left, P2POwnerToServerHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004F6D RID: 20333 RVA: 0x002AEA7B File Offset: 0x002ACC7B
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<EndpointStr>k__BackingField) * -1521134295 + EqualityComparer<AccountInfo>.Default.GetHashCode(this.<AccountInfo>k__BackingField);
		}

		// Token: 0x06004F6E RID: 20334 RVA: 0x002AEAA4 File Offset: 0x002ACCA4
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is P2POwnerToServerHeader && this.Equals((P2POwnerToServerHeader)obj);
		}

		// Token: 0x06004F6F RID: 20335 RVA: 0x002AEABC File Offset: 0x002ACCBC
		[CompilerGenerated]
		public bool Equals(P2POwnerToServerHeader other)
		{
			return EqualityComparer<string>.Default.Equals(this.<EndpointStr>k__BackingField, other.<EndpointStr>k__BackingField) && EqualityComparer<AccountInfo>.Default.Equals(this.<AccountInfo>k__BackingField, other.<AccountInfo>k__BackingField);
		}

		// Token: 0x06004F70 RID: 20336 RVA: 0x002AEAEE File Offset: 0x002ACCEE
		[NullableContext(2)]
		[CompilerGenerated]
		public void Deconstruct(out string EndpointStr, out AccountInfo AccountInfo)
		{
			EndpointStr = this.EndpointStr;
			AccountInfo = this.AccountInfo;
		}
	}
}
