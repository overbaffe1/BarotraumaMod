using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200057B RID: 1403
	public class StylesResourceInfo : BaseResourceInfo, IStylesResourceInfo, IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<StylesResourceInfo>
	{
		// Token: 0x17001548 RID: 5448
		// (get) Token: 0x06005624 RID: 22052 RVA: 0x002D192B File Offset: 0x002CFB2B
		[Nullable(1)]
		[CompilerGenerated]
		protected override Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(StylesResourceInfo);
			}
		}

		// Token: 0x06005625 RID: 22053 RVA: 0x002D1938 File Offset: 0x002CFB38
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("StylesResourceInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06005626 RID: 22054 RVA: 0x002D1984 File Offset: 0x002CFB84
		[NullableContext(1)]
		[CompilerGenerated]
		protected override bool PrintMembers(StringBuilder builder)
		{
			return base.PrintMembers(builder);
		}

		// Token: 0x06005627 RID: 22055 RVA: 0x002D198D File Offset: 0x002CFB8D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(StylesResourceInfo left, StylesResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06005628 RID: 22056 RVA: 0x002D1999 File Offset: 0x002CFB99
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(StylesResourceInfo left, StylesResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06005629 RID: 22057 RVA: 0x002D19AD File Offset: 0x002CFBAD
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		// Token: 0x0600562A RID: 22058 RVA: 0x002D19B5 File Offset: 0x002CFBB5
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as StylesResourceInfo);
		}

		// Token: 0x0600562B RID: 22059 RVA: 0x002D19C3 File Offset: 0x002CFBC3
		[NullableContext(2)]
		[CompilerGenerated]
		public sealed override bool Equals(BaseResourceInfo other)
		{
			return this.Equals(other);
		}

		// Token: 0x0600562C RID: 22060 RVA: 0x002D19CC File Offset: 0x002CFBCC
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(StylesResourceInfo other)
		{
			return this == other || base.Equals(other);
		}

		// Token: 0x0600562E RID: 22062 RVA: 0x002D19E3 File Offset: 0x002CFBE3
		[CompilerGenerated]
		protected StylesResourceInfo([Nullable(1)] StylesResourceInfo original) : base(original)
		{
		}

		// Token: 0x0600562F RID: 22063 RVA: 0x002D19EC File Offset: 0x002CFBEC
		public StylesResourceInfo()
		{
		}
	}
}
