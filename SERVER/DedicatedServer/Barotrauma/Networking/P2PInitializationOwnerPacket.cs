using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020003BD RID: 957
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(59)]
	internal readonly struct P2PInitializationOwnerPacket : INetSerializableStruct, IEquatable<P2PInitializationOwnerPacket>
	{
		// Token: 0x060037A8 RID: 14248 RVA: 0x001760F4 File Offset: 0x001742F4
		public P2PInitializationOwnerPacket(string Name, AccountId AccountId)
		{
			this.Name = Name;
			this.AccountId = AccountId;
		}

		// Token: 0x17000F44 RID: 3908
		// (get) Token: 0x060037A9 RID: 14249 RVA: 0x00176104 File Offset: 0x00174304
		// (set) Token: 0x060037AA RID: 14250 RVA: 0x0017610C File Offset: 0x0017430C
		public string Name { get; set; }

		// Token: 0x17000F45 RID: 3909
		// (get) Token: 0x060037AB RID: 14251 RVA: 0x00176115 File Offset: 0x00174315
		// (set) Token: 0x060037AC RID: 14252 RVA: 0x0017611D File Offset: 0x0017431D
		public AccountId AccountId { get; set; }

		// Token: 0x060037AD RID: 14253 RVA: 0x00176128 File Offset: 0x00174328
		[NullableContext(0)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("P2PInitializationOwnerPacket");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060037AE RID: 14254 RVA: 0x00176174 File Offset: 0x00174374
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Name = ");
			builder.Append(this.Name);
			builder.Append(", AccountId = ");
			builder.Append(this.AccountId);
			return true;
		}

		// Token: 0x060037AF RID: 14255 RVA: 0x001761A9 File Offset: 0x001743A9
		[CompilerGenerated]
		public static bool operator !=(P2PInitializationOwnerPacket left, P2PInitializationOwnerPacket right)
		{
			return !(left == right);
		}

		// Token: 0x060037B0 RID: 14256 RVA: 0x001761B5 File Offset: 0x001743B5
		[CompilerGenerated]
		public static bool operator ==(P2PInitializationOwnerPacket left, P2PInitializationOwnerPacket right)
		{
			return left.Equals(right);
		}

		// Token: 0x060037B1 RID: 14257 RVA: 0x001761BF File Offset: 0x001743BF
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<Name>k__BackingField) * -1521134295 + EqualityComparer<AccountId>.Default.GetHashCode(this.<AccountId>k__BackingField);
		}

		// Token: 0x060037B2 RID: 14258 RVA: 0x001761E8 File Offset: 0x001743E8
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is P2PInitializationOwnerPacket && this.Equals((P2PInitializationOwnerPacket)obj);
		}

		// Token: 0x060037B3 RID: 14259 RVA: 0x00176200 File Offset: 0x00174400
		[CompilerGenerated]
		public bool Equals(P2PInitializationOwnerPacket other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<AccountId>.Default.Equals(this.<AccountId>k__BackingField, other.<AccountId>k__BackingField);
		}

		// Token: 0x060037B4 RID: 14260 RVA: 0x00176232 File Offset: 0x00174432
		[CompilerGenerated]
		public void Deconstruct(out string Name, out AccountId AccountId)
		{
			Name = this.Name;
			AccountId = this.AccountId;
		}
	}
}
