using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace EventInput
{
	// Token: 0x02000015 RID: 21
	public readonly struct KeyEventArgs : IEquatable<KeyEventArgs>
	{
		// Token: 0x060000DD RID: 221 RVA: 0x00006666 File Offset: 0x00004866
		public KeyEventArgs(Keys KeyCode, char Character)
		{
			this.KeyCode = KeyCode;
			this.Character = Character;
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060000DE RID: 222 RVA: 0x00006676 File Offset: 0x00004876
		// (set) Token: 0x060000DF RID: 223 RVA: 0x0000667E File Offset: 0x0000487E
		public Keys KeyCode { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00006687 File Offset: 0x00004887
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x0000668F File Offset: 0x0000488F
		public char Character { get; set; }

		// Token: 0x060000E2 RID: 226 RVA: 0x00006698 File Offset: 0x00004898
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("KeyEventArgs");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x000066E4 File Offset: 0x000048E4
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("KeyCode = ");
			builder.Append(this.KeyCode.ToString());
			builder.Append(", Character = ");
			builder.Append(this.Character.ToString());
			return true;
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00006740 File Offset: 0x00004940
		[CompilerGenerated]
		public static bool operator !=(KeyEventArgs left, KeyEventArgs right)
		{
			return !(left == right);
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x0000674C File Offset: 0x0000494C
		[CompilerGenerated]
		public static bool operator ==(KeyEventArgs left, KeyEventArgs right)
		{
			return left.Equals(right);
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00006756 File Offset: 0x00004956
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Keys>.Default.GetHashCode(this.<KeyCode>k__BackingField) * -1521134295 + EqualityComparer<char>.Default.GetHashCode(this.<Character>k__BackingField);
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x0000677F File Offset: 0x0000497F
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is KeyEventArgs && this.Equals((KeyEventArgs)obj);
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00006797 File Offset: 0x00004997
		[CompilerGenerated]
		public bool Equals(KeyEventArgs other)
		{
			return EqualityComparer<Keys>.Default.Equals(this.<KeyCode>k__BackingField, other.<KeyCode>k__BackingField) && EqualityComparer<char>.Default.Equals(this.<Character>k__BackingField, other.<Character>k__BackingField);
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x000067C9 File Offset: 0x000049C9
		[CompilerGenerated]
		public void Deconstruct(out Keys KeyCode, out char Character)
		{
			KeyCode = this.KeyCode;
			Character = this.Character;
		}
	}
}
