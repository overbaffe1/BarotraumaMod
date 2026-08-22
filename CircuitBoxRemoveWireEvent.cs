using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000212 RID: 530
	[NetworkSerialize(154)]
	internal readonly struct CircuitBoxRemoveWireEvent : INetSerializableStruct, IEquatable<CircuitBoxRemoveWireEvent>
	{
		// Token: 0x0600361B RID: 13851 RVA: 0x00211D13 File Offset: 0x0020FF13
		public CircuitBoxRemoveWireEvent(ImmutableArray<ushort> TargetIDs)
		{
			this.TargetIDs = TargetIDs;
		}

		// Token: 0x17000E6B RID: 3691
		// (get) Token: 0x0600361C RID: 13852 RVA: 0x00211D1C File Offset: 0x0020FF1C
		// (set) Token: 0x0600361D RID: 13853 RVA: 0x00211D24 File Offset: 0x0020FF24
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x0600361E RID: 13854 RVA: 0x00211D30 File Offset: 0x0020FF30
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRemoveWireEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600361F RID: 13855 RVA: 0x00211D7C File Offset: 0x0020FF7C
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			return true;
		}

		// Token: 0x06003620 RID: 13856 RVA: 0x00211DB1 File Offset: 0x0020FFB1
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRemoveWireEvent left, CircuitBoxRemoveWireEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003621 RID: 13857 RVA: 0x00211DBD File Offset: 0x0020FFBD
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRemoveWireEvent left, CircuitBoxRemoveWireEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003622 RID: 13858 RVA: 0x00211DC7 File Offset: 0x0020FFC7
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField);
		}

		// Token: 0x06003623 RID: 13859 RVA: 0x00211DD9 File Offset: 0x0020FFD9
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRemoveWireEvent && this.Equals((CircuitBoxRemoveWireEvent)obj);
		}

		// Token: 0x06003624 RID: 13860 RVA: 0x00211DF1 File Offset: 0x0020FFF1
		[CompilerGenerated]
		public bool Equals(CircuitBoxRemoveWireEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField);
		}

		// Token: 0x06003625 RID: 13861 RVA: 0x00211E09 File Offset: 0x00210009
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs)
		{
			TargetIDs = this.TargetIDs;
		}
	}
}
