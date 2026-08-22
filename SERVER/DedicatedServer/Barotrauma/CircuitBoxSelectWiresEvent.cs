using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x02000116 RID: 278
	[NetworkSerialize(145)]
	internal readonly struct CircuitBoxSelectWiresEvent : INetSerializableStruct, IEquatable<CircuitBoxSelectWiresEvent>
	{
		// Token: 0x06001AFD RID: 6909 RVA: 0x000CB4B3 File Offset: 0x000C96B3
		public CircuitBoxSelectWiresEvent(ImmutableArray<ushort> TargetIDs, bool Overwrite, ushort CharacterID)
		{
			this.TargetIDs = TargetIDs;
			this.Overwrite = Overwrite;
			this.CharacterID = CharacterID;
		}

		// Token: 0x170007FD RID: 2045
		// (get) Token: 0x06001AFE RID: 6910 RVA: 0x000CB4CA File Offset: 0x000C96CA
		// (set) Token: 0x06001AFF RID: 6911 RVA: 0x000CB4D2 File Offset: 0x000C96D2
		public ImmutableArray<ushort> TargetIDs { get; set; }

		// Token: 0x170007FE RID: 2046
		// (get) Token: 0x06001B00 RID: 6912 RVA: 0x000CB4DB File Offset: 0x000C96DB
		// (set) Token: 0x06001B01 RID: 6913 RVA: 0x000CB4E3 File Offset: 0x000C96E3
		public bool Overwrite { get; set; }

		// Token: 0x170007FF RID: 2047
		// (get) Token: 0x06001B02 RID: 6914 RVA: 0x000CB4EC File Offset: 0x000C96EC
		// (set) Token: 0x06001B03 RID: 6915 RVA: 0x000CB4F4 File Offset: 0x000C96F4
		public ushort CharacterID { get; set; }

		// Token: 0x06001B04 RID: 6916 RVA: 0x000CB500 File Offset: 0x000C9700
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

		// Token: 0x06001B05 RID: 6917 RVA: 0x000CB54C File Offset: 0x000C974C
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

		// Token: 0x06001B06 RID: 6918 RVA: 0x000CB5CF File Offset: 0x000C97CF
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxSelectWiresEvent left, CircuitBoxSelectWiresEvent right)
		{
			return !(left == right);
		}

		// Token: 0x06001B07 RID: 6919 RVA: 0x000CB5DB File Offset: 0x000C97DB
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxSelectWiresEvent left, CircuitBoxSelectWiresEvent right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B08 RID: 6920 RVA: 0x000CB5E5 File Offset: 0x000C97E5
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<ImmutableArray<ushort>>.Default.GetHashCode(this.<TargetIDs>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<Overwrite>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<CharacterID>k__BackingField);
		}

		// Token: 0x06001B09 RID: 6921 RVA: 0x000CB625 File Offset: 0x000C9825
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxSelectWiresEvent && this.Equals((CircuitBoxSelectWiresEvent)obj);
		}

		// Token: 0x06001B0A RID: 6922 RVA: 0x000CB640 File Offset: 0x000C9840
		[CompilerGenerated]
		public bool Equals(CircuitBoxSelectWiresEvent other)
		{
			return EqualityComparer<ImmutableArray<ushort>>.Default.Equals(this.<TargetIDs>k__BackingField, other.<TargetIDs>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<Overwrite>k__BackingField, other.<Overwrite>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<CharacterID>k__BackingField, other.<CharacterID>k__BackingField);
		}

		// Token: 0x06001B0B RID: 6923 RVA: 0x000CB695 File Offset: 0x000C9895
		[CompilerGenerated]
		public void Deconstruct(out ImmutableArray<ushort> TargetIDs, out bool Overwrite, out ushort CharacterID)
		{
			TargetIDs = this.TargetIDs;
			Overwrite = this.Overwrite;
			CharacterID = this.CharacterID;
		}
	}
}
