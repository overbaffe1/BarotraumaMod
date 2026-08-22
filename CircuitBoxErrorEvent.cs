using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000215 RID: 533
	[NetworkSerialize(164)]
	internal readonly struct CircuitBoxErrorEvent : INetSerializableStruct, IEquatable<CircuitBoxErrorEvent>
	{
		// Token: 0x06003644 RID: 13892 RVA: 0x0021221B File Offset: 0x0021041B
		[NullableContext(1)]
		public CircuitBoxErrorEvent(string Message)
		{
			this.Message = Message;
		}

		// Token: 0x17000E72 RID: 3698
		// (get) Token: 0x06003645 RID: 13893 RVA: 0x00212224 File Offset: 0x00210424
		// (set) Token: 0x06003646 RID: 13894 RVA: 0x0021222C File Offset: 0x0021042C
		[Nullable(1)]
		public string Message { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x06003647 RID: 13895 RVA: 0x00212238 File Offset: 0x00210438
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxErrorEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003648 RID: 13896 RVA: 0x00212284 File Offset: 0x00210484
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Message = ");
			builder.Append(this.Message);
			return true;
		}

		// Token: 0x06003649 RID: 13897 RVA: 0x002122A0 File Offset: 0x002104A0
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxErrorEvent left, CircuitBoxErrorEvent right)
		{
			return !(left == right);
		}

		// Token: 0x0600364A RID: 13898 RVA: 0x002122AC File Offset: 0x002104AC
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxErrorEvent left, CircuitBoxErrorEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600364B RID: 13899 RVA: 0x002122B6 File Offset: 0x002104B6
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<Message>k__BackingField);
		}

		// Token: 0x0600364C RID: 13900 RVA: 0x002122C8 File Offset: 0x002104C8
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxErrorEvent && this.Equals((CircuitBoxErrorEvent)obj);
		}

		// Token: 0x0600364D RID: 13901 RVA: 0x002122E0 File Offset: 0x002104E0
		[CompilerGenerated]
		public bool Equals(CircuitBoxErrorEvent other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Message>k__BackingField, other.<Message>k__BackingField);
		}

		// Token: 0x0600364E RID: 13902 RVA: 0x002122F8 File Offset: 0x002104F8
		[NullableContext(1)]
		[CompilerGenerated]
		public void Deconstruct(out string Message)
		{
			Message = this.Message;
		}
	}
}
