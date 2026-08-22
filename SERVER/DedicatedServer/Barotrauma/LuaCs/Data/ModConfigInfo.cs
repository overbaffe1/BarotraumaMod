using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200045E RID: 1118
	public class ModConfigInfo : IModConfigInfo, IAssembliesResourcesInfo, ILuaScriptsResourcesInfo, IConfigsResourcesInfo, IEquatable<ModConfigInfo>
	{
		// Token: 0x17000FE8 RID: 4072
		// (get) Token: 0x06003CCC RID: 15564 RVA: 0x0018CD46 File Offset: 0x0018AF46
		[Nullable(1)]
		[CompilerGenerated]
		protected virtual Type EqualityContract
		{
			[NullableContext(1)]
			[CompilerGenerated]
			get
			{
				return typeof(ModConfigInfo);
			}
		}

		// Token: 0x17000FE9 RID: 4073
		// (get) Token: 0x06003CCD RID: 15565 RVA: 0x0018CD52 File Offset: 0x0018AF52
		// (set) Token: 0x06003CCE RID: 15566 RVA: 0x0018CD5A File Offset: 0x0018AF5A
		public ContentPackage Package { get; set; }

		// Token: 0x17000FEA RID: 4074
		// (get) Token: 0x06003CCF RID: 15567 RVA: 0x0018CD63 File Offset: 0x0018AF63
		// (set) Token: 0x06003CD0 RID: 15568 RVA: 0x0018CD6B File Offset: 0x0018AF6B
		public ImmutableArray<IAssemblyResourceInfo> Assemblies { get; set; }

		// Token: 0x17000FEB RID: 4075
		// (get) Token: 0x06003CD1 RID: 15569 RVA: 0x0018CD74 File Offset: 0x0018AF74
		// (set) Token: 0x06003CD2 RID: 15570 RVA: 0x0018CD7C File Offset: 0x0018AF7C
		public ImmutableArray<ILuaScriptResourceInfo> LuaScripts { get; set; }

		// Token: 0x17000FEC RID: 4076
		// (get) Token: 0x06003CD3 RID: 15571 RVA: 0x0018CD85 File Offset: 0x0018AF85
		// (set) Token: 0x06003CD4 RID: 15572 RVA: 0x0018CD8D File Offset: 0x0018AF8D
		public ImmutableArray<IConfigResourceInfo> Configs { get; set; }

		// Token: 0x06003CD5 RID: 15573 RVA: 0x0018CD98 File Offset: 0x0018AF98
		[NullableContext(1)]
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ModConfigInfo");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06003CD6 RID: 15574 RVA: 0x0018CDE4 File Offset: 0x0018AFE4
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Package = ");
			builder.Append(this.Package);
			builder.Append(", Assemblies = ");
			builder.Append(this.Assemblies.ToString());
			builder.Append(", LuaScripts = ");
			builder.Append(this.LuaScripts.ToString());
			builder.Append(", Configs = ");
			builder.Append(this.Configs.ToString());
			return true;
		}

		// Token: 0x06003CD7 RID: 15575 RVA: 0x0018CE85 File Offset: 0x0018B085
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ModConfigInfo left, ModConfigInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06003CD8 RID: 15576 RVA: 0x0018CE91 File Offset: 0x0018B091
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ModConfigInfo left, ModConfigInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06003CD9 RID: 15577 RVA: 0x0018CEA8 File Offset: 0x0018B0A8
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<Package>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<IAssemblyResourceInfo>>.Default.GetHashCode(this.<Assemblies>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ILuaScriptResourceInfo>>.Default.GetHashCode(this.<LuaScripts>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<IConfigResourceInfo>>.Default.GetHashCode(this.<Configs>k__BackingField);
		}

		// Token: 0x06003CDA RID: 15578 RVA: 0x0018CF21 File Offset: 0x0018B121
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ModConfigInfo);
		}

		// Token: 0x06003CDB RID: 15579 RVA: 0x0018CF30 File Offset: 0x0018B130
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ModConfigInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<ContentPackage>.Default.Equals(this.<Package>k__BackingField, other.<Package>k__BackingField) && EqualityComparer<ImmutableArray<IAssemblyResourceInfo>>.Default.Equals(this.<Assemblies>k__BackingField, other.<Assemblies>k__BackingField) && EqualityComparer<ImmutableArray<ILuaScriptResourceInfo>>.Default.Equals(this.<LuaScripts>k__BackingField, other.<LuaScripts>k__BackingField) && EqualityComparer<ImmutableArray<IConfigResourceInfo>>.Default.Equals(this.<Configs>k__BackingField, other.<Configs>k__BackingField));
		}

		// Token: 0x06003CDD RID: 15581 RVA: 0x0018CFC1 File Offset: 0x0018B1C1
		[CompilerGenerated]
		protected ModConfigInfo([Nullable(1)] ModConfigInfo original)
		{
			this.Package = original.<Package>k__BackingField;
			this.Assemblies = original.<Assemblies>k__BackingField;
			this.LuaScripts = original.<LuaScripts>k__BackingField;
			this.Configs = original.<Configs>k__BackingField;
		}

		// Token: 0x06003CDE RID: 15582 RVA: 0x0018CFF9 File Offset: 0x0018B1F9
		public ModConfigInfo()
		{
		}
	}
}
