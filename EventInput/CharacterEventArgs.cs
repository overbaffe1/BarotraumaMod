using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EventInput
{
	// Token: 0x02000014 RID: 20
	public readonly struct CharacterEventArgs : IEquatable<CharacterEventArgs>
	{
		// Token: 0x060000CB RID: 203 RVA: 0x000063D3 File Offset: 0x000045D3
		public CharacterEventArgs(char Character, long Param)
		{
			this.Character = Character;
			this.Param = Param;
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x060000CC RID: 204 RVA: 0x000063E3 File Offset: 0x000045E3
		// (set) Token: 0x060000CD RID: 205 RVA: 0x000063EB File Offset: 0x000045EB
		public char Character { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x060000CE RID: 206 RVA: 0x000063F4 File Offset: 0x000045F4
		// (set) Token: 0x060000CF RID: 207 RVA: 0x000063FC File Offset: 0x000045FC
		public long Param { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x060000D0 RID: 208 RVA: 0x00006405 File Offset: 0x00004605
		public long RepeatCount
		{
			get
			{
				return this.Param & 65535L;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x00006414 File Offset: 0x00004614
		public bool ExtendedKey
		{
			get
			{
				return (this.Param & 16777216L) > 0L;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x00006427 File Offset: 0x00004627
		public bool AltPressed
		{
			get
			{
				return (this.Param & 536870912L) > 0L;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000D3 RID: 211 RVA: 0x0000643A File Offset: 0x0000463A
		public bool PreviousState
		{
			get
			{
				return (this.Param & 1073741824L) > 0L;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x0000644D File Offset: 0x0000464D
		public bool TransitionState
		{
			get
			{
				return (this.Param & -2147483648L) > 0L;
			}
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00006460 File Offset: 0x00004660
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CharacterEventArgs");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x000064AC File Offset: 0x000046AC
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Character = ");
			builder.Append(this.Character.ToString());
			builder.Append(", Param = ");
			builder.Append(this.Param.ToString());
			builder.Append(", RepeatCount = ");
			builder.Append(this.RepeatCount.ToString());
			builder.Append(", ExtendedKey = ");
			builder.Append(this.ExtendedKey.ToString());
			builder.Append(", AltPressed = ");
			builder.Append(this.AltPressed.ToString());
			builder.Append(", PreviousState = ");
			builder.Append(this.PreviousState.ToString());
			builder.Append(", TransitionState = ");
			builder.Append(this.TransitionState.ToString());
			return true;
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x000065CB File Offset: 0x000047CB
		[CompilerGenerated]
		public static bool operator !=(CharacterEventArgs left, CharacterEventArgs right)
		{
			return !(left == right);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x000065D7 File Offset: 0x000047D7
		[CompilerGenerated]
		public static bool operator ==(CharacterEventArgs left, CharacterEventArgs right)
		{
			return left.Equals(right);
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x000065E1 File Offset: 0x000047E1
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<char>.Default.GetHashCode(this.<Character>k__BackingField) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(this.<Param>k__BackingField);
		}

		// Token: 0x060000DA RID: 218 RVA: 0x0000660A File Offset: 0x0000480A
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CharacterEventArgs && this.Equals((CharacterEventArgs)obj);
		}

		// Token: 0x060000DB RID: 219 RVA: 0x00006622 File Offset: 0x00004822
		[CompilerGenerated]
		public bool Equals(CharacterEventArgs other)
		{
			return EqualityComparer<char>.Default.Equals(this.<Character>k__BackingField, other.<Character>k__BackingField) && EqualityComparer<long>.Default.Equals(this.<Param>k__BackingField, other.<Param>k__BackingField);
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00006654 File Offset: 0x00004854
		[CompilerGenerated]
		public void Deconstruct(out char Character, out long Param)
		{
			Character = this.Character;
			Param = this.Param;
		}
	}
}
