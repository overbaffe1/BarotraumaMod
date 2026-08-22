using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.Networking
{
	// Token: 0x020004BA RID: 1210
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(59)]
	internal readonly struct P2PInitializationOwnerPacket : INetSerializableStruct, IEquatable<P2PInitializationOwnerPacket>
	{
		// Token: 0x06004F7D RID: 20349 RVA: 0x002AEC30 File Offset: 0x002ACE30
		public P2PInitializationOwnerPacket(string Name, AccountId AccountId)
		{
			this.Name = Name;
			this.AccountId = AccountId;
		}

		// Token: 0x1700143F RID: 5183
		// (get) Token: 0x06004F7E RID: 20350 RVA: 0x002AEC40 File Offset: 0x002ACE40
		// (set) Token: 0x06004F7F RID: 20351 RVA: 0x002AEC48 File Offset: 0x002ACE48
		public string Name { get; set; }

		// Token: 0x17001440 RID: 5184
		// (get) Token: 0x06004F80 RID: 20352 RVA: 0x002AEC51 File Offset: 0x002ACE51
		// (set) Token: 0x06004F81 RID: 20353 RVA: 0x002AEC59 File Offset: 0x002ACE59
		public AccountId AccountId { get; set; }

		// Token: 0x06004F82 RID: 20354 RVA: 0x002AEC64 File Offset: 0x002ACE64
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

		// Token: 0x06004F83 RID: 20355 RVA: 0x002AECB0 File Offset: 0x002ACEB0
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

		// Token: 0x06004F84 RID: 20356 RVA: 0x002AECE5 File Offset: 0x002ACEE5
		[CompilerGenerated]
		public static bool operator !=(P2PInitializationOwnerPacket left, P2PInitializationOwnerPacket right)
		{
			return !(left == right);
		}

		// Token: 0x06004F85 RID: 20357 RVA: 0x002AECF1 File Offset: 0x002ACEF1
		[CompilerGenerated]
		public static bool operator ==(P2PInitializationOwnerPacket left, P2PInitializationOwnerPacket right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004F86 RID: 20358 RVA: 0x002AECFB File Offset: 0x002ACEFB
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<Name>k__BackingField) * -1521134295 + EqualityComparer<AccountId>.Default.GetHashCode(this.<AccountId>k__BackingField);
		}

		// Token: 0x06004F87 RID: 20359 RVA: 0x002AED24 File Offset: 0x002ACF24
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is P2PInitializationOwnerPacket && this.Equals((P2PInitializationOwnerPacket)obj);
		}

		// Token: 0x06004F88 RID: 20360 RVA: 0x002AED3C File Offset: 0x002ACF3C
		[CompilerGenerated]
		public bool Equals(P2PInitializationOwnerPacket other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<AccountId>.Default.Equals(this.<AccountId>k__BackingField, other.<AccountId>k__BackingField);
		}

		// Token: 0x06004F89 RID: 20361 RVA: 0x002AED6E File Offset: 0x002ACF6E
		[CompilerGenerated]
		public void Deconstruct(out string Name, out AccountId AccountId)
		{
			Name = this.Name;
			AccountId = this.AccountId;
		}
	}
}
