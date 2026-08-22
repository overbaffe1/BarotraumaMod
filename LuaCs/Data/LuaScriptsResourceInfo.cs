using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000581 RID: 1409
	public class LuaScriptsResourceInfo : BaseResourceInfo, ILuaScriptResourceInfo, IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<LuaScriptsResourceInfo>
	{
		// Token: 0x17001561 RID: 5473
		// (get) Token: 0x06005684 RID: 22148 RVA: 0x002D25C8 File Offset: 0x002D07C8
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

		// Token: 0x17001562 RID: 5474
		// (get) Token: 0x06005685 RID: 22149 RVA: 0x002D25D4 File Offset: 0x002D07D4
		// (set) Token: 0x06005686 RID: 22150 RVA: 0x002D25DC File Offset: 0x002D07DC
		public bool IsAutorun { get; set; }

		// Token: 0x17001563 RID: 5475
		// (get) Token: 0x06005687 RID: 22151 RVA: 0x002D25E5 File Offset: 0x002D07E5
		// (set) Token: 0x06005688 RID: 22152 RVA: 0x002D25ED File Offset: 0x002D07ED
		public bool RunUnrestricted { get; set; }

		// Token: 0x06005689 RID: 22153 RVA: 0x002D25F8 File Offset: 0x002D07F8
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

		// Token: 0x0600568A RID: 22154 RVA: 0x002D2644 File Offset: 0x002D0844
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

		// Token: 0x0600568B RID: 22155 RVA: 0x002D26B5 File Offset: 0x002D08B5
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(LuaScriptsResourceInfo left, LuaScriptsResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x0600568C RID: 22156 RVA: 0x002D26C1 File Offset: 0x002D08C1
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(LuaScriptsResourceInfo left, LuaScriptsResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x0600568D RID: 22157 RVA: 0x002D26D5 File Offset: 0x002D08D5
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (base.GetHashCode() * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsAutorun>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<RunUnrestricted>k__BackingField);
		}

		// Token: 0x0600568E RID: 22158 RVA: 0x002D270B File Offset: 0x002D090B
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as LuaScriptsResourceInfo);
		}

		// Token: 0x0600568F RID: 22159 RVA: 0x002D2719 File Offset: 0x002D0919
		[NullableContext(2)]
		[CompilerGenerated]
		public sealed override bool Equals(BaseResourceInfo other)
		{
			return this.Equals(other);
		}

		// Token: 0x06005690 RID: 22160 RVA: 0x002D2724 File Offset: 0x002D0924
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(LuaScriptsResourceInfo other)
		{
			return this == other || (base.Equals(other) && EqualityComparer<bool>.Default.Equals(this.<IsAutorun>k__BackingField, other.<IsAutorun>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<RunUnrestricted>k__BackingField, other.<RunUnrestricted>k__BackingField));
		}

		// Token: 0x06005692 RID: 22162 RVA: 0x002D2778 File Offset: 0x002D0978
		[CompilerGenerated]
		protected LuaScriptsResourceInfo([Nullable(1)] LuaScriptsResourceInfo original) : base(original)
		{
			this.IsAutorun = original.<IsAutorun>k__BackingField;
			this.RunUnrestricted = original.<RunUnrestricted>k__BackingField;
		}

		// Token: 0x06005693 RID: 22163 RVA: 0x002D2799 File Offset: 0x002D0999
		public LuaScriptsResourceInfo()
		{
		}
	}
}
