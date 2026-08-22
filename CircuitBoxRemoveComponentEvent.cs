using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000209 RID: 521
	[NetworkSerialize(127)]
	internal readonly struct CircuitBoxRemoveComponentEvent : INetSerializableStruct, IEquatable<CircuitBoxRemoveComponentEvent>
	{
		// Token: 0x06003592 RID: 13714 RVA: 0x00210A74 File Offset: 0x0020EC74
		public CircuitBoxRemoveComponentEvent(ImmutableArray<ushort> TargetIDs)
		{
			this.TargetIDs = TargetIDs;
		}

		// Token: 0x17000E4F RID: 3663
		// (get) Token: 0x06003593 RID: 13715 RVA: 0x00210A7D File Offset: 0x0020EC7D
		// (set) Token: 0x06003594 RID: 13716 RVA: 0x00210A85 File Offset: 0x0020EC85
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x06003595 RID: 13717 RVA: 0x00210A90 File Offset: 0x0020EC90
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxRemoveComponentEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003596 RID: 13718 RVA: 0x00210ADC File Offset: 0x0020ECDC
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			return true;
		}

		// Token: 0x06003597 RID: 13719 RVA: 0x00210B11 File Offset: 0x0020ED11
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxRemoveComponentEvent left, CircuitBoxRemoveComponentEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06003598 RID: 13720 RVA: 0x00210B1D File Offset: 0x0020ED1D
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxRemoveComponentEvent left, CircuitBoxRemoveComponentEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003599 RID: 13721 RVA: 0x00210B27 File Offset: 0x0020ED27
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField);
		}

		// Token: 0x0600359A RID: 13722 RVA: 0x00210B39 File Offset: 0x0020ED39
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxRemoveComponentEvent && this.Equals((CircuitBoxRemoveComponentEvent)obj);
		}

		// Token: 0x0600359B RID: 13723 RVA: 0x00210B51 File Offset: 0x0020ED51
		[CompilerGenerated]
		public bool Equals(CircuitBoxRemoveComponentEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField);
		}

		// Token: 0x0600359C RID: 13724 RVA: 0x00210B69 File Offset: 0x0020ED69
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs)
		{
			TargetIDs = this.TargetIDs;
		}
	}
}
