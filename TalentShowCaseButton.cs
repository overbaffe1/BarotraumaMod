using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma
{
	// Token: 0x020000B7 RID: 183
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct TalentShowCaseButton : IEquatable<TalentShowCaseButton>
	{
		// Token: 0x06001704 RID: 5892 RVA: 0x000DF3F2 File Offset: 0x000DD5F2
		public TalentShowCaseButton(ImmutableHashSet<TalentButton> Buttons, GUIComponent IconComponent)
		{
			this.Buttons = Buttons;
			this.IconComponent = IconComponent;
		}

		// Token: 0x170005C4 RID: 1476
		// (get) Token: 0x06001705 RID: 5893 RVA: 0x000DF402 File Offset: 0x000DD602
		// (set) Token: 0x06001706 RID: 5894 RVA: 0x000DF40A File Offset: 0x000DD60A
		public ImmutableHashSet<TalentButton> Buttons { get; set; }

		// Token: 0x170005C5 RID: 1477
		// (get) Token: 0x06001707 RID: 5895 RVA: 0x000DF413 File Offset: 0x000DD613
		// (set) Token: 0x06001708 RID: 5896 RVA: 0x000DF41B File Offset: 0x000DD61B
		public GUIComponent IconComponent { get; set; }

		// Token: 0x06001709 RID: 5897 RVA: 0x000DF424 File Offset: 0x000DD624
		[NullableContext(0)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TalentShowCaseButton");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600170A RID: 5898 RVA: 0x000DF470 File Offset: 0x000DD670
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Buttons = ");
			builder.Append(this.Buttons);
			builder.Append(", IconComponent = ");
			builder.Append(this.IconComponent);
			return true;
		}

		// Token: 0x0600170B RID: 5899 RVA: 0x000DF4A5 File Offset: 0x000DD6A5
		[CompilerGenerated]
		public static bool operator !=(TalentShowCaseButton left, TalentShowCaseButton right)
		{
			return !(left == right);
		}

		// Token: 0x0600170C RID: 5900 RVA: 0x000DF4B1 File Offset: 0x000DD6B1
		[CompilerGenerated]
		public static bool operator ==(TalentShowCaseButton left, TalentShowCaseButton right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600170D RID: 5901 RVA: 0x000DF4BB File Offset: 0x000DD6BB
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableHashSet<TalentButton>>.Default.GetHashCode(this.<Buttons>k__BackingField) * -1521134295 + EqualityComparer<GUIComponent>.Default.GetHashCode(this.<IconComponent>k__BackingField);
		}

		// Token: 0x0600170E RID: 5902 RVA: 0x000DF4E4 File Offset: 0x000DD6E4
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is TalentShowCaseButton && this.Equals((TalentShowCaseButton)obj);
		}

		// Token: 0x0600170F RID: 5903 RVA: 0x000DF4FC File Offset: 0x000DD6FC
		[CompilerGenerated]
		public bool Equals(TalentShowCaseButton other)
		{
			return EqualityComparer<ImmutableHashSet<TalentButton>>.Default.Equals(this.<Buttons>k__BackingField, other.<Buttons>k__BackingField) && EqualityComparer<GUIComponent>.Default.Equals(this.<IconComponent>k__BackingField, other.<IconComponent>k__BackingField);
		}

		// Token: 0x06001710 RID: 5904 RVA: 0x000DF52E File Offset: 0x000DD72E
		[CompilerGenerated]
		public void Deconstruct(out ImmutableHashSet<TalentButton> Buttons, out GUIComponent IconComponent)
		{
			Buttons = this.Buttons;
			IconComponent = this.IconComponent;
		}
	}
}
