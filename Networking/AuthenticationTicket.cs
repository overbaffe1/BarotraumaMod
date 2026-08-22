using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000492 RID: 1170
	[NetworkSerialize(19, ArrayMaxSize = 65535)]
	internal readonly struct AuthenticationTicket : INetSerializableStruct, IEquatable<AuthenticationTicket>
	{
		// Token: 0x06004E1A RID: 19994 RVA: 0x002AC560 File Offset: 0x002AA760
		public AuthenticationTicket(AuthenticationTicketKind Kind, ImmutableArray<byte> Data)
		{
			this.Kind = Kind;
			this.Data = Data;
		}

		// Token: 0x17001407 RID: 5127
		// (get) Token: 0x06004E1B RID: 19995 RVA: 0x002AC570 File Offset: 0x002AA770
		// (set) Token: 0x06004E1C RID: 19996 RVA: 0x002AC578 File Offset: 0x002AA778
		public AuthenticationTicketKind Kind { get; set; }

		// Token: 0x17001408 RID: 5128
		// (get) Token: 0x06004E1D RID: 19997 RVA: 0x002AC581 File Offset: 0x002AA781
		// (set) Token: 0x06004E1E RID: 19998 RVA: 0x002AC589 File Offset: 0x002AA789
		public ImmutableArray<byte> Data { get; set; }

		// Token: 0x06004E1F RID: 19999 RVA: 0x002AC594 File Offset: 0x002AA794
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

		// Token: 0x06004E20 RID: 20000 RVA: 0x002AC5D8 File Offset: 0x002AA7D8
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

		// Token: 0x06004E21 RID: 20001 RVA: 0x002AC624 File Offset: 0x002AA824
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Kind = ");
			builder.Append(this.Kind.ToString());
			builder.Append(", Data = ");
			builder.Append(this.Data.ToString());
			return true;
		}

		// Token: 0x06004E22 RID: 20002 RVA: 0x002AC680 File Offset: 0x002AA880
		[CompilerGenerated]
		public static bool operator !=(AuthenticationTicket left, AuthenticationTicket right)
		{
			return !(left == right);
		}

		// Token: 0x06004E23 RID: 20003 RVA: 0x002AC68C File Offset: 0x002AA88C
		[CompilerGenerated]
		public static bool operator ==(AuthenticationTicket left, AuthenticationTicket right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004E24 RID: 20004 RVA: 0x002AC696 File Offset: 0x002AA896
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<AuthenticationTicketKind>.Default.GetHashCode(this.<Kind>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<byte>>.Default.GetHashCode(this.<Data>k__BackingField);
		}

		// Token: 0x06004E25 RID: 20005 RVA: 0x002AC6BF File Offset: 0x002AA8BF
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is AuthenticationTicket && this.Equals((AuthenticationTicket)obj);
		}

		// Token: 0x06004E26 RID: 20006 RVA: 0x002AC6D7 File Offset: 0x002AA8D7
		[CompilerGenerated]
		public bool Equals(AuthenticationTicket other)
		{
			return EqualityComparer<AuthenticationTicketKind>.Default.Equals(this.<Kind>k__BackingField, other.<Kind>k__BackingField) && EqualityComparer<ImmutableArray<byte>>.Default.Equals(this.<Data>k__BackingField, other.<Data>k__BackingField);
		}

		// Token: 0x06004E27 RID: 20007 RVA: 0x002AC709 File Offset: 0x002AA909
		[CompilerGenerated]
		public void Deconstruct(out AuthenticationTicketKind Kind, out ImmutableArray<byte> Data)
		{
			Kind = this.Kind;
			Data = this.Data;
		}
	}
}
