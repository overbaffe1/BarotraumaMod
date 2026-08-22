using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020001B2 RID: 434
	public readonly struct TalentResistanceIdentifier : IEquatable<TalentResistanceIdentifier>
	{
		// Token: 0x060030FE RID: 12542 RVA: 0x002037B5 File Offset: 0x002019B5
		public TalentResistanceIdentifier(Identifier ResistanceIdentifier, Identifier TalentIdentifier)
		{
			this.ResistanceIdentifier = ResistanceIdentifier;
			this.TalentIdentifier = TalentIdentifier;
		}

		// Token: 0x17000CCF RID: 3279
		// (get) Token: 0x060030FF RID: 12543 RVA: 0x002037C5 File Offset: 0x002019C5
		// (set) Token: 0x06003100 RID: 12544 RVA: 0x002037CD File Offset: 0x002019CD
		public Identifier ResistanceIdentifier { get; set; }

		// Token: 0x17000CD0 RID: 3280
		// (get) Token: 0x06003101 RID: 12545 RVA: 0x002037D6 File Offset: 0x002019D6
		// (set) Token: 0x06003102 RID: 12546 RVA: 0x002037DE File Offset: 0x002019DE
		public Identifier TalentIdentifier { get; set; }

		// Token: 0x06003103 RID: 12547 RVA: 0x002037E8 File Offset: 0x002019E8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TalentResistanceIdentifier");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003104 RID: 12548 RVA: 0x00203834 File Offset: 0x00201A34
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("ResistanceIdentifier = ");
			builder.Append(this.ResistanceIdentifier.ToString());
			builder.Append(", TalentIdentifier = ");
			builder.Append(this.TalentIdentifier.ToString());
			return true;
		}

		// Token: 0x06003105 RID: 12549 RVA: 0x00203890 File Offset: 0x00201A90
		[CompilerGenerated]
		public static bool operator !=(TalentResistanceIdentifier left, TalentResistanceIdentifier right)
		{
			return !(left == right);
		}

		// Token: 0x06003106 RID: 12550 RVA: 0x0020389C File Offset: 0x00201A9C
		[CompilerGenerated]
		public static bool operator ==(TalentResistanceIdentifier left, TalentResistanceIdentifier right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003107 RID: 12551 RVA: 0x002038A6 File Offset: 0x00201AA6
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Identifier>.Default.GetHashCode(this.<ResistanceIdentifier>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<TalentIdentifier>k__BackingField);
		}

		// Token: 0x06003108 RID: 12552 RVA: 0x002038CF File Offset: 0x00201ACF
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is TalentResistanceIdentifier && this.Equals((TalentResistanceIdentifier)obj);
		}

		// Token: 0x06003109 RID: 12553 RVA: 0x002038E7 File Offset: 0x00201AE7
		[CompilerGenerated]
		public bool Equals(TalentResistanceIdentifier other)
		{
			return EqualityComparer<Identifier>.Default.Equals(this.<ResistanceIdentifier>k__BackingField, other.<ResistanceIdentifier>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<TalentIdentifier>k__BackingField, other.<TalentIdentifier>k__BackingField);
		}

		// Token: 0x0600310A RID: 12554 RVA: 0x00203919 File Offset: 0x00201B19
		[CompilerGenerated]
		public void Deconstruct(out Identifier ResistanceIdentifier, out Identifier TalentIdentifier)
		{
			ResistanceIdentifier = this.ResistanceIdentifier;
			TalentIdentifier = this.TalentIdentifier;
		}
	}
}
