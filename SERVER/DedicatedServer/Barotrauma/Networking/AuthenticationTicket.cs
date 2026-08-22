using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000395 RID: 917
	[NetworkSerialize(19, ArrayMaxSize = 65535)]
	internal readonly struct AuthenticationTicket : INetSerializableStruct, IEquatable<AuthenticationTicket>
	{
		// Token: 0x06003645 RID: 13893 RVA: 0x00173A24 File Offset: 0x00171C24
		public AuthenticationTicket(AuthenticationTicketKind Kind, ImmutableArray<byte> Data)
		{
			this.Kind = Kind;
			this.Data = Data;
		}

		// Token: 0x17000F0C RID: 3852
		// (get) Token: 0x06003646 RID: 13894 RVA: 0x00173A34 File Offset: 0x00171C34
		// (set) Token: 0x06003647 RID: 13895 RVA: 0x00173A3C File Offset: 0x00171C3C
		public AuthenticationTicketKind Kind { get; set; }

		// Token: 0x17000F0D RID: 3853
		// (get) Token: 0x06003648 RID: 13896 RVA: 0x00173A45 File Offset: 0x00171C45
		// (set) Token: 0x06003649 RID: 13897 RVA: 0x00173A4D File Offset: 0x00171C4D
		public ImmutableArray<byte> Data { get; set; }

		// Token: 0x0600364A RID: 13898 RVA: 0x00173A58 File Offset: 0x00171C58
		[NullableContext(1)]
		[return: Nullable(new byte[]
		{
			1,
			0
		})]
		public static Task<Option<AuthenticationTicket>> Create(Endpoint serverEndpoint)
		{
			AuthenticationTicket.<Create>d__9 <Create>d__;
			<Create>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AuthenticationTicket>>.Create();
			<Create>d__.serverEndpoint = serverEndpoint;
			<Create>d__.<>1__state = -1;
			<Create>d__.<>t__builder.Start<AuthenticationTicket.<Create>d__9>(ref <Create>d__);
			return <Create>d__.<>t__builder.Task;
		}

		// Token: 0x0600364B RID: 13899 RVA: 0x00173A9C File Offset: 0x00171C9C
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("AuthenticationTicket");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600364C RID: 13900 RVA: 0x00173AE8 File Offset: 0x00171CE8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Kind = ");
			builder.Append(this.Kind.ToString());
			builder.Append(", Data = ");
			builder.Append(this.Data.ToString());
			return true;
		}

		// Token: 0x0600364D RID: 13901 RVA: 0x00173B44 File Offset: 0x00171D44
		[CompilerGenerated]
		public static bool operator !=(AuthenticationTicket left, AuthenticationTicket right)
		{
			return !(left == right);
		}

		// Token: 0x0600364E RID: 13902 RVA: 0x00173B50 File Offset: 0x00171D50
		[CompilerGenerated]
		public static bool operator ==(AuthenticationTicket left, AuthenticationTicket right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600364F RID: 13903 RVA: 0x00173B5A File Offset: 0x00171D5A
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<AuthenticationTicketKind>.Default.GetHashCode(this.<Kind>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<byte>>.Default.GetHashCode(this.<Data>k__BackingField);
		}

		// Token: 0x06003650 RID: 13904 RVA: 0x00173B83 File Offset: 0x00171D83
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is AuthenticationTicket && this.Equals((AuthenticationTicket)obj);
		}

		// Token: 0x06003651 RID: 13905 RVA: 0x00173B9B File Offset: 0x00171D9B
		[CompilerGenerated]
		public bool Equals(AuthenticationTicket other)
		{
			return EqualityComparer<AuthenticationTicketKind>.Default.Equals(this.<Kind>k__BackingField, other.<Kind>k__BackingField) && EqualityComparer<ImmutableArray<byte>>.Default.Equals(this.<Data>k__BackingField, other.<Data>k__BackingField);
		}

		// Token: 0x06003652 RID: 13906 RVA: 0x00173BCD File Offset: 0x00171DCD
		[CompilerGenerated]
		public void Deconstruct(out AuthenticationTicketKind Kind, out ImmutableArray<byte> Data)
		{
			Kind = this.Kind;
			Data = this.Data;
		}
	}
}
