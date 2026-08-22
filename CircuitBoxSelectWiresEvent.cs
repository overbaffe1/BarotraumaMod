using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x0200020F RID: 527
	[NetworkSerialize(145)]
	internal readonly struct CircuitBoxSelectWiresEvent : INetSerializableStruct, IEquatable<CircuitBoxSelectWiresEvent>
	{
		// Token: 0x060035EC RID: 13804 RVA: 0x00211687 File Offset: 0x0020F887
		public CircuitBoxSelectWiresEvent(ImmutableArray<ushort> TargetIDs, bool Overwrite, ushort CharacterID)
		{
			this.TargetIDs = TargetIDs;
			this.Overwrite = Overwrite;
			this.CharacterID = CharacterID;
		}

		// Token: 0x17000E61 RID: 3681
		// (get) Token: 0x060035ED RID: 13805 RVA: 0x0021169E File Offset: 0x0020F89E
		// (set) Token: 0x060035EE RID: 13806 RVA: 0x002116A6 File Offset: 0x0020F8A6
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x17000E62 RID: 3682
		// (get) Token: 0x060035EF RID: 13807 RVA: 0x002116AF File Offset: 0x0020F8AF
		// (set) Token: 0x060035F0 RID: 13808 RVA: 0x002116B7 File Offset: 0x0020F8B7
		public bool Overwrite { get; set; }

		// Token: 0x17000E63 RID: 3683
		// (get) Token: 0x060035F1 RID: 13809 RVA: 0x002116C0 File Offset: 0x0020F8C0
		// (set) Token: 0x060035F2 RID: 13810 RVA: 0x002116C8 File Offset: 0x0020F8C8
		public ushort CharacterID { get; set; }

		// Token: 0x060035F3 RID: 13811 RVA: 0x002116D4 File Offset: 0x0020F8D4
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxSelectWiresEvent");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060035F4 RID: 13812 RVA: 0x00211720 File Offset: 0x0020F920
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("TargetIDs = ");
			builder.Append(this.TargetIDs.ToString());
			builder.Append(", Overwrite = ");
			builder.Append(this.Overwrite.ToString());
			builder.Append(", CharacterID = ");
			builder.Append(this.CharacterID.ToString());
			return true;
		}

		// Token: 0x060035F5 RID: 13813 RVA: 0x002117A3 File Offset: 0x0020F9A3
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxSelectWiresEvent left, CircuitBoxSelectWiresEvent right)
		{
			return !(left == right);
		}

		// Token: 0x060035F6 RID: 13814 RVA: 0x002117AF File Offset: 0x0020F9AF
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxSelectWiresEvent left, CircuitBoxSelectWiresEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x060035F7 RID: 13815 RVA: 0x002117B9 File Offset: 0x0020F9B9
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Overwrite>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<CharacterID>k__BackingField);
		}

		// Token: 0x060035F8 RID: 13816 RVA: 0x002117F9 File Offset: 0x0020F9F9
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxSelectWiresEvent && this.Equals((CircuitBoxSelectWiresEvent)obj);
		}

		// Token: 0x060035F9 RID: 13817 RVA: 0x00211814 File Offset: 0x0020FA14
		[CompilerGenerated]
		public bool Equals(CircuitBoxSelectWiresEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Overwrite>k__BackingField, other.<Overwrite>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<CharacterID>k__BackingField, other.<CharacterID>k__BackingField);
		}

		// Token: 0x060035FA RID: 13818 RVA: 0x00211869 File Offset: 0x0020FA69
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs, out bool Overwrite, out ushort CharacterID)
		{
			TargetIDs = this.TargetIDs;
			Overwrite = this.Overwrite;
			CharacterID = this.CharacterID;
		}
	}
}
