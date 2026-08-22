using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020004B8 RID: 1208
	[NetworkSerialize(45)]
	internal readonly struct P2PServerToOwnerHeader : INetSerializableStruct, IEquatable<P2PServerToOwnerHeader>
	{
		// Token: 0x06004F71 RID: 20337 RVA: 0x002AEB04 File Offset: 0x002ACD04
		[NullableContext(2)]
		public P2PServerToOwnerHeader(string EndpointStr)
		{
			this.EndpointStr = EndpointStr;
		}

		// Token: 0x1700143D RID: 5181
		// (get) Token: 0x06004F72 RID: 20338 RVA: 0x002AEB0D File Offset: 0x002ACD0D
		// (set) Token: 0x06004F73 RID: 20339 RVA: 0x002AEB15 File Offset: 0x002ACD15
		[Nullable(2)]
		public string EndpointStr { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x1700143E RID: 5182
		// (get) Token: 0x06004F74 RID: 20340 RVA: 0x002AEB1E File Offset: 0x002ACD1E
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

		// Token: 0x06004F75 RID: 20341 RVA: 0x002AEB34 File Offset: 0x002ACD34
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

		// Token: 0x06004F76 RID: 20342 RVA: 0x002AEB80 File Offset: 0x002ACD80
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("EndpointStr = ");
			builder.Append(this.EndpointStr);
			builder.Append(", Endpoint = ");
			builder.Append(this.Endpoint.ToString());
			return true;
		}

		// Token: 0x06004F77 RID: 20343 RVA: 0x002AEBCE File Offset: 0x002ACDCE
		[CompilerGenerated]
		public static bool operator !=(P2PServerToOwnerHeader left, P2PServerToOwnerHeader right)
		{
			return !(left == right);
		}

		// Token: 0x06004F78 RID: 20344 RVA: 0x002AEBDA File Offset: 0x002ACDDA
		[CompilerGenerated]
		public static bool operator ==(P2PServerToOwnerHeader left, P2PServerToOwnerHeader right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004F79 RID: 20345 RVA: 0x002AEBE4 File Offset: 0x002ACDE4
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<EndpointStr>k__BackingField);
		}

		// Token: 0x06004F7A RID: 20346 RVA: 0x002AEBF6 File Offset: 0x002ACDF6
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is P2PServerToOwnerHeader && this.Equals((P2PServerToOwnerHeader)obj);
		}

		// Token: 0x06004F7B RID: 20347 RVA: 0x002AEC0E File Offset: 0x002ACE0E
		[CompilerGenerated]
		public bool Equals(P2PServerToOwnerHeader other)
		{
			return EqualityComparer<string>.Default.Equals(this.<EndpointStr>k__BackingField, other.<EndpointStr>k__BackingField);
		}

		// Token: 0x06004F7C RID: 20348 RVA: 0x002AEC26 File Offset: 0x002ACE26
		[NullableContext(2)]
		[CompilerGenerated]
		public void Deconstruct(out string EndpointStr)
		{
			EndpointStr = this.EndpointStr;
		}
	}
}
