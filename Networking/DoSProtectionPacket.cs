using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020004BF RID: 1215
	[NetworkSerialize(97)]
	internal readonly struct DoSProtectionPacket : INetSerializableStruct, IEquatable<DoSProtectionPacket>
	{
		// Token: 0x06004F8D RID: 20365 RVA: 0x002AEDB7 File Offset: 0x002ACFB7
		[NullableContext(1)]
		public DoSProtectionPacket(string EndpointStr, bool ShouldBan)
		{
			this.EndpointStr = EndpointStr;
			this.ShouldBan = ShouldBan;
		}

		// Token: 0x17001442 RID: 5186
		// (get) Token: 0x06004F8E RID: 20366 RVA: 0x002AEDC7 File Offset: 0x002ACFC7
		// (set) Token: 0x06004F8F RID: 20367 RVA: 0x002AEDCF File Offset: 0x002ACFCF
		[Nullable(1)]
		public string EndpointStr { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x17001443 RID: 5187
		// (get) Token: 0x06004F90 RID: 20368 RVA: 0x002AEDD8 File Offset: 0x002ACFD8
		// (set) Token: 0x06004F91 RID: 20369 RVA: 0x002AEDE0 File Offset: 0x002ACFE0
		public bool ShouldBan { get; set; }

		// Token: 0x17001444 RID: 5188
		// (get) Token: 0x06004F92 RID: 20370 RVA: 0x002AEDE9 File Offset: 0x002ACFE9
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
				return P2PEndpoint.Parse(this.EndpointStr);
			}
		}

		// Token: 0x06004F93 RID: 20371 RVA: 0x002AEDF8 File Offset: 0x002ACFF8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("DoSProtectionPacket");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06004F94 RID: 20372 RVA: 0x002AEE44 File Offset: 0x002AD044
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("EndpointStr = ");
			builder.Append(this.EndpointStr);
			builder.Append(", ShouldBan = ");
			builder.Append(this.ShouldBan.ToString());
			builder.Append(", Endpoint = ");
			builder.Append(this.Endpoint.ToString());
			return true;
		}

		// Token: 0x06004F95 RID: 20373 RVA: 0x002AEEB9 File Offset: 0x002AD0B9
		[CompilerGenerated]
		public static bool operator !=(DoSProtectionPacket left, DoSProtectionPacket right)
		{
			return !(left == right);
		}

		// Token: 0x06004F96 RID: 20374 RVA: 0x002AEEC5 File Offset: 0x002AD0C5
		[CompilerGenerated]
		public static bool operator ==(DoSProtectionPacket left, DoSProtectionPacket right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004F97 RID: 20375 RVA: 0x002AEECF File Offset: 0x002AD0CF
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<EndpointStr>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<ShouldBan>k__BackingField);
		}

		// Token: 0x06004F98 RID: 20376 RVA: 0x002AEEF8 File Offset: 0x002AD0F8
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is DoSProtectionPacket && this.Equals((DoSProtectionPacket)obj);
		}

		// Token: 0x06004F99 RID: 20377 RVA: 0x002AEF10 File Offset: 0x002AD110
		[CompilerGenerated]
		public bool Equals(DoSProtectionPacket other)
		{
			return EqualityComparer<string>.Default.Equals(this.<EndpointStr>k__BackingField, other.<EndpointStr>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<ShouldBan>k__BackingField, other.<ShouldBan>k__BackingField);
		}

		// Token: 0x06004F9A RID: 20378 RVA: 0x002AEF42 File Offset: 0x002AD142
		[NullableContext(1)]
		[CompilerGenerated]
		public void Deconstruct(out string EndpointStr, out bool ShouldBan)
		{
			EndpointStr = this.EndpointStr;
			ShouldBan = this.ShouldBan;
		}
	}
}
