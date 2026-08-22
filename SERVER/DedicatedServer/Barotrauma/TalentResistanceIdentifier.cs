using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020000B0 RID: 176
	public readonly struct TalentResistanceIdentifier : IEquatable<TalentResistanceIdentifier>
	{
		// Token: 0x0600153B RID: 5435 RVA: 0x000B89A1 File Offset: 0x000B6BA1
		public TalentResistanceIdentifier(Identifier ResistanceIdentifier, Identifier TalentIdentifier)
		{
			this.ResistanceIdentifier = ResistanceIdentifier;
			this.TalentIdentifier = TalentIdentifier;
		}

		// Token: 0x17000629 RID: 1577
		// (get) Token: 0x0600153C RID: 5436 RVA: 0x000B89B1 File Offset: 0x000B6BB1
		// (set) Token: 0x0600153D RID: 5437 RVA: 0x000B89B9 File Offset: 0x000B6BB9
		public Identifier ResistanceIdentifier { get; set; }

		// Token: 0x1700062A RID: 1578
		// (get) Token: 0x0600153E RID: 5438 RVA: 0x000B89C2 File Offset: 0x000B6BC2
		// (set) Token: 0x0600153F RID: 5439 RVA: 0x000B89CA File Offset: 0x000B6BCA
		public Identifier TalentIdentifier { get; set; }

		// Token: 0x06001540 RID: 5440 RVA: 0x000B89D4 File Offset: 0x000B6BD4
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

		// Token: 0x06001541 RID: 5441 RVA: 0x000B8A20 File Offset: 0x000B6C20
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("ResistanceIdentifier = ");
			builder.Append(this.ResistanceIdentifier.ToString());
			builder.Append(", TalentIdentifier = ");
			builder.Append(this.TalentIdentifier.ToString());
			return true;
		}

		// Token: 0x06001542 RID: 5442 RVA: 0x000B8A7C File Offset: 0x000B6C7C
		[CompilerGenerated]
		public static bool operator !=(TalentResistanceIdentifier left, TalentResistanceIdentifier right)
		{
			return !(left == right);
		}

		// Token: 0x06001543 RID: 5443 RVA: 0x000B8A88 File Offset: 0x000B6C88
		[CompilerGenerated]
		public static bool operator ==(TalentResistanceIdentifier left, TalentResistanceIdentifier right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001544 RID: 5444 RVA: 0x000B8A92 File Offset: 0x000B6C92
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Identifier>.Default.GetHashCode(this.<ResistanceIdentifier>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<TalentIdentifier>k__BackingField);
		}

		// Token: 0x06001545 RID: 5445 RVA: 0x000B8ABB File Offset: 0x000B6CBB
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is TalentResistanceIdentifier && this.Equals((TalentResistanceIdentifier)obj);
		}

		// Token: 0x06001546 RID: 5446 RVA: 0x000B8AD3 File Offset: 0x000B6CD3
		[CompilerGenerated]
		public bool Equals(TalentResistanceIdentifier other)
		{
			return EqualityComparer<Identifier>.Default.Equals(this.<ResistanceIdentifier>k__BackingField, other.<ResistanceIdentifier>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<TalentIdentifier>k__BackingField, other.<TalentIdentifier>k__BackingField);
		}

		// Token: 0x06001547 RID: 5447 RVA: 0x000B8B05 File Offset: 0x000B6D05
		[CompilerGenerated]
		public void Deconstruct(out Identifier ResistanceIdentifier, out Identifier TalentIdentifier)
		{
			ResistanceIdentifier = this.ResistanceIdentifier;
			TalentIdentifier = this.TalentIdentifier;
		}
	}
}
