using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200011C RID: 284
	[NetworkSerialize(164)]
	internal readonly struct CircuitBoxErrorEvent : INetSerializableStruct, IEquatable<CircuitBoxErrorEvent>
	{
		// Token: 0x06001B55 RID: 6997 RVA: 0x000CC047 File Offset: 0x000CA247
		[NullableContext(1)]
		public CircuitBoxErrorEvent(string Message)
		{
			this.Message = Message;
		}

		// Token: 0x1700080E RID: 2062
		// (get) Token: 0x06001B56 RID: 6998 RVA: 0x000CC050 File Offset: 0x000CA250
		// (set) Token: 0x06001B57 RID: 6999 RVA: 0x000CC058 File Offset: 0x000CA258
		[Nullable(1)]
		public string Message { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x06001B58 RID: 7000 RVA: 0x000CC064 File Offset: 0x000CA264
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

		// Token: 0x06001B59 RID: 7001 RVA: 0x000CC0B0 File Offset: 0x000CA2B0
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Message = ");
			builder.Append(this.Message);
			return true;
		}

		// Token: 0x06001B5A RID: 7002 RVA: 0x000CC0CC File Offset: 0x000CA2CC
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxErrorEvent left, CircuitBoxErrorEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B5B RID: 7003 RVA: 0x000CC0D8 File Offset: 0x000CA2D8
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxErrorEvent left, CircuitBoxErrorEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B5C RID: 7004 RVA: 0x000CC0E2 File Offset: 0x000CA2E2
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<string>.Default.GetHashCode(this.<Message>k__BackingField);
		}

		// Token: 0x06001B5D RID: 7005 RVA: 0x000CC0F4 File Offset: 0x000CA2F4
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxErrorEvent && this.Equals((CircuitBoxErrorEvent)obj);
		}

		// Token: 0x06001B5E RID: 7006 RVA: 0x000CC10C File Offset: 0x000CA30C
		[CompilerGenerated]
		public bool Equals(CircuitBoxErrorEvent other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Message>k__BackingField, other.<Message>k__BackingField);
		}

		// Token: 0x06001B5F RID: 7007 RVA: 0x000CC124 File Offset: 0x000CA324
		[NullableContext(1)]
		[CompilerGenerated]
		public void Deconstruct(out string Message)
		{
			Message = this.Message;
		}
	}
}
