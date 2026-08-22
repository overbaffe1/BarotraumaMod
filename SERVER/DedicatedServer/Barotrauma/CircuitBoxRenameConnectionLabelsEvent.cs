using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200011B RID: 283
	[NetworkSerialize(160)]
	internal readonly struct CircuitBoxRenameConnectionLabelsEvent : INetSerializableStruct, IEquatable<CircuitBoxRenameConnectionLabelsEvent>
	{
		// Token: 0x06001B48 RID: 6984 RVA: 0x000CBECC File Offset: 0x000CA0CC
		public CircuitBoxRenameConnectionLabelsEvent(CircuitBoxInputOutputNode.Type Type, [Nullable(new byte[]
		{
			0,
			1,
			1
		})] NetDictionary<string, string> Override)
		{
			this.Type = Type;
			this.Override = Override;
		}

		// Token: 0x1700080C RID: 2060
		// (get) Token: 0x06001B49 RID: 6985 RVA: 0x000CBEDC File Offset: 0x000CA0DC
		// (set) Token: 0x06001B4A RID: 6986 RVA: 0x000CBEE4 File Offset: 0x000CA0E4
		public CircuitBoxInputOutputNode.Type Type { get; set; }

		// Token: 0x1700080D RID: 2061
		// (get) Token: 0x06001B4B RID: 6987 RVA: 0x000CBEED File Offset: 0x000CA0ED
		// (set) Token: 0x06001B4C RID: 6988 RVA: 0x000CBEF5 File Offset: 0x000CA0F5
		[Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public NetDictionary<string, string> Override { [return: Nullable(new byte[]
		{
			0,
			1,
			1
		})] get; [param: Nullable(new byte[]
		{
			0,
			1,
			1
		})] set; }

		// Token: 0x06001B4D RID: 6989 RVA: 0x000CBF00 File Offset: 0x000CA100
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRenameConnectionLabelsEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001B4E RID: 6990 RVA: 0x000CBF4C File Offset: 0x000CA14C
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Type = ");
			builder.Append(this.Type.ToString());
			builder.Append(", Override = ");
			builder.Append(this.Override.ToString());
			return true;
		}

		// Token: 0x06001B4F RID: 6991 RVA: 0x000CBFA8 File Offset: 0x000CA1A8
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRenameConnectionLabelsEvent left, CircuitBoxRenameConnectionLabelsEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B50 RID: 6992 RVA: 0x000CBFB4 File Offset: 0x000CA1B4
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRenameConnectionLabelsEvent left, CircuitBoxRenameConnectionLabelsEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B51 RID: 6993 RVA: 0x000CBFBE File Offset: 0x000CA1BE
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<NetDictionary<string, string>>.Default.GetHashCode(this.<Override>k__BackingField);
		}

		// Token: 0x06001B52 RID: 6994 RVA: 0x000CBFE7 File Offset: 0x000CA1E7
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRenameConnectionLabelsEvent && this.Equals((CircuitBoxRenameConnectionLabelsEvent)obj);
		}

		// Token: 0x06001B53 RID: 6995 RVA: 0x000CBFFF File Offset: 0x000CA1FF
		[CompilerGenerated]
		public bool Equals(CircuitBoxRenameConnectionLabelsEvent other)
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<NetDictionary<string, string>>.Default.Equals(this.<Override>k__BackingField, other.<Override>k__BackingField);
		}

		// Token: 0x06001B54 RID: 6996 RVA: 0x000CC031 File Offset: 0x000CA231
		[CompilerGenerated]
		public void Deconstruct(out CircuitBoxInputOutputNode.Type Type, [Nullable(new byte[]
		{
			0,
			1,
			1
		})] out NetDictionary<string, string> Override)
		{
			Type = this.Type;
			Override = this.Override;
		}
	}
}
