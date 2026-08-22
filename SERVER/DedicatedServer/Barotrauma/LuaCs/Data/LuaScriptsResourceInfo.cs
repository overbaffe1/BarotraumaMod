using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000462 RID: 1122
	public class LuaScriptsResourceInfo : BaseResourceInfo, ILuaScriptResourceInfo, IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<LuaScriptsResourceInfo>
	{
		// Token: 0x17000FFD RID: 4093
		// (get) Token: 0x06003D1C RID: 15644 RVA: 0x0018D898 File Offset: 0x0018BA98
		[Nullable(1)]
		[CompilerGenerated]
		protected override Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(LuaScriptsResourceInfo);
			}
		}

		// Token: 0x17000FFE RID: 4094
		// (get) Token: 0x06003D1D RID: 15645 RVA: 0x0018D8A4 File Offset: 0x0018BAA4
		// (set) Token: 0x06003D1E RID: 15646 RVA: 0x0018D8AC File Offset: 0x0018BAAC
		public bool IsAutorun { get; set; }

		// Token: 0x17000FFF RID: 4095
		// (get) Token: 0x06003D1F RID: 15647 RVA: 0x0018D8B5 File Offset: 0x0018BAB5
		// (set) Token: 0x06003D20 RID: 15648 RVA: 0x0018D8BD File Offset: 0x0018BABD
		public bool RunUnrestricted { get; set; }

		// Token: 0x06003D21 RID: 15649 RVA: 0x0018D8C8 File Offset: 0x0018BAC8
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("LuaScriptsResourceInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003D22 RID: 15650 RVA: 0x0018D914 File Offset: 0x0018BB14
		[NullableContext(1)]
		[CompilerGenerated]
		protected override bool PrintMembers(StringBuilder builder)
		{
			if (base.PrintMembers(builder))
			{
				builder.Append(", ");
			}
			builder.Append("IsAutorun = ");
			builder.Append(this.IsAutorun.ToString());
			builder.Append(", RunUnrestricted = ");
			builder.Append(this.RunUnrestricted.ToString());
			return true;
		}

		// Token: 0x06003D23 RID: 15651 RVA: 0x0018D985 File Offset: 0x0018BB85
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(LuaScriptsResourceInfo left, LuaScriptsResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003D24 RID: 15652 RVA: 0x0018D991 File Offset: 0x0018BB91
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(LuaScriptsResourceInfo left, LuaScriptsResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003D25 RID: 15653 RVA: 0x0018D9A5 File Offset: 0x0018BBA5
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (base.GetHashCode() * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsAutorun>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<RunUnrestricted>k__BackingField);
		}

		// Token: 0x06003D26 RID: 15654 RVA: 0x0018D9DB File Offset: 0x0018BBDB
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as LuaScriptsResourceInfo);
		}

		// Token: 0x06003D27 RID: 15655 RVA: 0x0018D9E9 File Offset: 0x0018BBE9
		[NullableContext(2)]
		[CompilerGenerated]
		public sealed override bool Equals(BaseResourceInfo other)
		{
			return this.Equals(other);
		}

		// Token: 0x06003D28 RID: 15656 RVA: 0x0018D9F4 File Offset: 0x0018BBF4
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(LuaScriptsResourceInfo other)
		{
			return this == other || (base.Equals(other) && EqualityComparer<bool>.Default.Equals(this.<IsAutorun>k__BackingField, other.<IsAutorun>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<RunUnrestricted>k__BackingField, other.<RunUnrestricted>k__BackingField));
		}

		// Token: 0x06003D2A RID: 15658 RVA: 0x0018DA48 File Offset: 0x0018BC48
		[CompilerGenerated]
		protected LuaScriptsResourceInfo([Nullable(1)] LuaScriptsResourceInfo original) : base(original)
		{
			this.IsAutorun = original.<IsAutorun>k__BackingField;
			this.RunUnrestricted = original.<RunUnrestricted>k__BackingField;
		}

		// Token: 0x06003D2B RID: 15659 RVA: 0x0018DA69 File Offset: 0x0018BC69
		public LuaScriptsResourceInfo()
		{
		}
	}
}
