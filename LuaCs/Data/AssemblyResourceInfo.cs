using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200057F RID: 1407
	public class AssemblyResourceInfo : BaseResourceInfo, IAssemblyResourceInfo, IBaseResourceInfo, IResourceInfo, IPlatformInfo, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IDependencyInfo, IEquatable<AssemblyResourceInfo>
	{
		// Token: 0x1700155B RID: 5467
		// (get) Token: 0x06005664 RID: 22116 RVA: 0x002D223F File Offset: 0x002D043F
		[Nullable(1)]
		[CompilerGenerated]
		protected override Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(AssemblyResourceInfo);
			}
		}

		// Token: 0x1700155C RID: 5468
		// (get) Token: 0x06005665 RID: 22117 RVA: 0x002D224B File Offset: 0x002D044B
		// (set) Token: 0x06005666 RID: 22118 RVA: 0x002D2253 File Offset: 0x002D0453
		public string FriendlyName { get; set; }

		// Token: 0x1700155D RID: 5469
		// (get) Token: 0x06005667 RID: 22119 RVA: 0x002D225C File Offset: 0x002D045C
		// (set) Token: 0x06005668 RID: 22120 RVA: 0x002D2264 File Offset: 0x002D0464
		public bool IsScript { get; set; }

		// Token: 0x1700155E RID: 5470
		// (get) Token: 0x06005669 RID: 22121 RVA: 0x002D226D File Offset: 0x002D046D
		// (set) Token: 0x0600566A RID: 22122 RVA: 0x002D2275 File Offset: 0x002D0475
		public bool UseInternalAccessName { get; set; }

		// Token: 0x1700155F RID: 5471
		// (get) Token: 0x0600566B RID: 22123 RVA: 0x002D227E File Offset: 0x002D047E
		// (set) Token: 0x0600566C RID: 22124 RVA: 0x002D2286 File Offset: 0x002D0486
		public bool IsReferenceModeOnly { get; set; }

		// Token: 0x0600566D RID: 22125 RVA: 0x002D2290 File Offset: 0x002D0490
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("AssemblyResourceInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x0600566E RID: 22126 RVA: 0x002D22DC File Offset: 0x002D04DC
		[NullableContext(1)]
		[CompilerGenerated]
		protected override bool PrintMembers(StringBuilder builder)
		{
			if (base.PrintMembers(builder))
			{
				builder.Append(", ");
			}
			builder.Append("FriendlyName = ");
			builder.Append(this.FriendlyName);
			builder.Append(", IsScript = ");
			builder.Append(this.IsScript.ToString());
			builder.Append(", UseInternalAccessName = ");
			builder.Append(this.UseInternalAccessName.ToString());
			builder.Append(", IsReferenceModeOnly = ");
			builder.Append(this.IsReferenceModeOnly.ToString());
			return true;
		}

		// Token: 0x0600566F RID: 22127 RVA: 0x002D238D File Offset: 0x002D058D
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(AssemblyResourceInfo left, AssemblyResourceInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06005670 RID: 22128 RVA: 0x002D2399 File Offset: 0x002D0599
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(AssemblyResourceInfo left, AssemblyResourceInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06005671 RID: 22129 RVA: 0x002D23B0 File Offset: 0x002D05B0
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((base.GetHashCode() * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<FriendlyName>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsScript>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<UseInternalAccessName>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsReferenceModeOnly>k__BackingField);
		}

		// Token: 0x06005672 RID: 22130 RVA: 0x002D241F File Offset: 0x002D061F
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as AssemblyResourceInfo);
		}

		// Token: 0x06005673 RID: 22131 RVA: 0x002D242D File Offset: 0x002D062D
		[NullableContext(2)]
		[CompilerGenerated]
		public sealed override bool Equals(BaseResourceInfo other)
		{
			return this.Equals(other);
		}

		// Token: 0x06005674 RID: 22132 RVA: 0x002D2438 File Offset: 0x002D0638
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(AssemblyResourceInfo other)
		{
			return this == other || (base.Equals(other) && EqualityComparer<string>.Default.Equals(this.<FriendlyName>k__BackingField, other.<FriendlyName>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsScript>k__BackingField, other.<IsScript>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<UseInternalAccessName>k__BackingField, other.<UseInternalAccessName>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsReferenceModeOnly>k__BackingField, other.<IsReferenceModeOnly>k__BackingField));
		}

		// Token: 0x06005676 RID: 22134 RVA: 0x002D24BC File Offset: 0x002D06BC
		[CompilerGenerated]
		protected AssemblyResourceInfo([Nullable(1)] AssemblyResourceInfo original) : base(original)
		{
			this.FriendlyName = original.<FriendlyName>k__BackingField;
			this.IsScript = original.<IsScript>k__BackingField;
			this.UseInternalAccessName = original.<UseInternalAccessName>k__BackingField;
			this.IsReferenceModeOnly = original.<IsReferenceModeOnly>k__BackingField;
		}

		// Token: 0x06005677 RID: 22135 RVA: 0x002D24F5 File Offset: 0x002D06F5
		public AssemblyResourceInfo()
		{
		}
	}
}
