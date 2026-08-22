using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000214 RID: 532
	[NetworkSerialize(160)]
	internal readonly struct CircuitBoxRenameConnectionLabelsEvent : INetSerializableStruct, IEquatable<CircuitBoxRenameConnectionLabelsEvent>
	{
		// Token: 0x06003637 RID: 13879 RVA: 0x002120A0 File Offset: 0x002102A0
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

		// Token: 0x17000E70 RID: 3696
		// (get) Token: 0x06003638 RID: 13880 RVA: 0x002120B0 File Offset: 0x002102B0
		// (set) Token: 0x06003639 RID: 13881 RVA: 0x002120B8 File Offset: 0x002102B8
		public CircuitBoxInputOutputNode.Type Type { get; set; }

		// Token: 0x17000E71 RID: 3697
		// (get) Token: 0x0600363A RID: 13882 RVA: 0x002120C1 File Offset: 0x002102C1
		// (set) Token: 0x0600363B RID: 13883 RVA: 0x002120C9 File Offset: 0x002102C9
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

		// Token: 0x0600363C RID: 13884 RVA: 0x002120D4 File Offset: 0x002102D4
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

		// Token: 0x0600363D RID: 13885 RVA: 0x00212120 File Offset: 0x00210320
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Type = ");
			builder.Append(this.Type.ToString());
			builder.Append(", Override = ");
			builder.Append(this.Override.ToString());
			return true;
		}

		// Token: 0x0600363E RID: 13886 RVA: 0x0021217C File Offset: 0x0021037C
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRenameConnectionLabelsEvent left, CircuitBoxRenameConnectionLabelsEvent right)
		{
			return !(left == right);
		}

		// Token: 0x0600363F RID: 13887 RVA: 0x00212188 File Offset: 0x00210388
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRenameConnectionLabelsEvent left, CircuitBoxRenameConnectionLabelsEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003640 RID: 13888 RVA: 0x00212192 File Offset: 0x00210392
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<NetDictionary<string, string>>.Default.GetHashCode(this.<Override>k__BackingField);
		}

		// Token: 0x06003641 RID: 13889 RVA: 0x002121BB File Offset: 0x002103BB
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRenameConnectionLabelsEvent && this.Equals((CircuitBoxRenameConnectionLabelsEvent)obj);
		}

		// Token: 0x06003642 RID: 13890 RVA: 0x002121D3 File Offset: 0x002103D3
		[CompilerGenerated]
		public bool Equals(CircuitBoxRenameConnectionLabelsEvent other)
		{
			return EqualityComparer<CircuitBoxInputOutputNode.Type>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<NetDictionary<string, string>>.Default.Equals(this.<Override>k__BackingField, other.<Override>k__BackingField);
		}

		// Token: 0x06003643 RID: 13891 RVA: 0x00212205 File Offset: 0x00210405
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
