using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020003C2 RID: 962
	[NetworkSerialize(97)]
	internal readonly struct DoSProtectionPacket : INetSerializableStruct, IEquatable<DoSProtectionPacket>
	{
		// Token: 0x060037B8 RID: 14264 RVA: 0x0017627B File Offset: 0x0017447B
		[NullableContext(1)]
		public DoSProtectionPacket(string EndpointStr, bool ShouldBan)
		{
			this.EndpointStr = EndpointStr;
			this.ShouldBan = ShouldBan;
		}

		// Token: 0x17000F47 RID: 3911
		// (get) Token: 0x060037B9 RID: 14265 RVA: 0x0017628B File Offset: 0x0017448B
		// (set) Token: 0x060037BA RID: 14266 RVA: 0x00176293 File Offset: 0x00174493
		[Nullable(1)]
		public string EndpointStr { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x17000F48 RID: 3912
		// (get) Token: 0x060037BB RID: 14267 RVA: 0x0017629C File Offset: 0x0017449C
		// (set) Token: 0x060037BC RID: 14268 RVA: 0x001762A4 File Offset: 0x001744A4
		public bool ShouldBan { get; set; }

		// Token: 0x17000F49 RID: 3913
		// (get) Token: 0x060037BD RID: 14269 RVA: 0x001762AD File Offset: 0x001744AD
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

		// Token: 0x060037BE RID: 14270 RVA: 0x001762BC File Offset: 0x001744BC
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

		// Token: 0x060037BF RID: 14271 RVA: 0x00176308 File Offset: 0x00174508
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

		// Token: 0x060037C0 RID: 14272 RVA: 0x0017637D File Offset: 0x0017457D
		[CompilerGenerated]
		public static bool operator !=(DoSProtectionPacket left, DoSProtectionPacket right)
		{
			return !(left == right);
		}

		// Token: 0x060037C1 RID: 14273 RVA: 0x00176389 File Offset: 0x00174589
		[CompilerGenerated]
		public static bool operator ==(DoSProtectionPacket left, DoSProtectionPacket right)
		{
			return left.Equals(right);
		}

		// Token: 0x060037C2 RID: 14274 RVA: 0x00176393 File Offset: 0x00174593
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<EndpointStr>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<ShouldBan>k__BackingField);
		}

		// Token: 0x060037C3 RID: 14275 RVA: 0x001763BC File Offset: 0x001745BC
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is DoSProtectionPacket && this.Equals((DoSProtectionPacket)obj);
		}

		// Token: 0x060037C4 RID: 14276 RVA: 0x001763D4 File Offset: 0x001745D4
		[CompilerGenerated]
		public bool Equals(DoSProtectionPacket other)
		{
			return EqualityComparer<string>.Default.Equals(this.<EndpointStr>k__BackingField, other.<EndpointStr>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<ShouldBan>k__BackingField, other.<ShouldBan>k__BackingField);
		}

		// Token: 0x060037C5 RID: 14277 RVA: 0x00176406 File Offset: 0x00174606
		[NullableContext(1)]
		[CompilerGenerated]
		public void Deconstruct(out string EndpointStr, out bool ShouldBan)
		{
			EndpointStr = this.EndpointStr;
			ShouldBan = this.ShouldBan;
		}
	}
}
