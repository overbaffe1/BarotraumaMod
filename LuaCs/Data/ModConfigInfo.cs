using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200057D RID: 1405
	public class ModConfigInfo : IModConfigInfo, IAssembliesResourcesInfo, ILuaScriptsResourcesInfo, IConfigsResourcesInfo, IEquatable<ModConfigInfo>
	{
		// Token: 0x1700154B RID: 5451
		// (get) Token: 0x06005632 RID: 22066 RVA: 0x002D19F4 File Offset: 0x002CFBF4
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

		// Token: 0x1700154C RID: 5452
		// (get) Token: 0x06005633 RID: 22067 RVA: 0x002D1A00 File Offset: 0x002CFC00
		// (set) Token: 0x06005634 RID: 22068 RVA: 0x002D1A08 File Offset: 0x002CFC08
		public ImmutableArray<IStylesResourceInfo> Styles { get; set; }

		// Token: 0x1700154D RID: 5453
		// (get) Token: 0x06005635 RID: 22069 RVA: 0x002D1A11 File Offset: 0x002CFC11
		// (set) Token: 0x06005636 RID: 22070 RVA: 0x002D1A19 File Offset: 0x002CFC19
		public ContentPackage Package { get; set; }

		// Token: 0x1700154E RID: 5454
		// (get) Token: 0x06005637 RID: 22071 RVA: 0x002D1A22 File Offset: 0x002CFC22
		// (set) Token: 0x06005638 RID: 22072 RVA: 0x002D1A2A File Offset: 0x002CFC2A
		public ImmutableArray<IAssemblyResourceInfo> Assemblies { get; set; }

		// Token: 0x1700154F RID: 5455
		// (get) Token: 0x06005639 RID: 22073 RVA: 0x002D1A33 File Offset: 0x002CFC33
		// (set) Token: 0x0600563A RID: 22074 RVA: 0x002D1A3B File Offset: 0x002CFC3B
		public ImmutableArray<ILuaScriptResourceInfo> LuaScripts { get; set; }

		// Token: 0x17001550 RID: 5456
		// (get) Token: 0x0600563B RID: 22075 RVA: 0x002D1A44 File Offset: 0x002CFC44
		// (set) Token: 0x0600563C RID: 22076 RVA: 0x002D1A4C File Offset: 0x002CFC4C
		public ImmutableArray<IConfigResourceInfo> Configs { get; set; }

		// Token: 0x0600563D RID: 22077 RVA: 0x002D1A58 File Offset: 0x002CFC58
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

		// Token: 0x0600563E RID: 22078 RVA: 0x002D1AA4 File Offset: 0x002CFCA4
		[NullableContext(1)]
		[CompilerGenerated]
		protected virtual bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Styles = ");
			builder.Append(this.Styles.ToString());
			builder.Append(", Package = ");
			builder.Append(this.Package);
			builder.Append(", Assemblies = ");
			builder.Append(this.Assemblies.ToString());
			builder.Append(", LuaScripts = ");
			builder.Append(this.LuaScripts.ToString());
			builder.Append(", Configs = ");
			builder.Append(this.Configs.ToString());
			return true;
		}

		// Token: 0x0600563F RID: 22079 RVA: 0x002D1B6C File Offset: 0x002CFD6C
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator !=(ModConfigInfo left, ModConfigInfo right)
		{
			return !(left == right);
		}

		// Token: 0x06005640 RID: 22080 RVA: 0x002D1B78 File Offset: 0x002CFD78
		[NullableContext(2)]
		[CompilerGenerated]
		public static bool operator ==(ModConfigInfo left, ModConfigInfo right)
		{
			return left == right || (left != null && left.Equals(right));
		}

		// Token: 0x06005641 RID: 22081 RVA: 0x002D1B8C File Offset: 0x002CFD8C
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return ((((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<ImmutableArray<IStylesResourceInfo>>.Default.GetHashCode(this.<Styles>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<Package>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<IAssemblyResourceInfo>>.Default.GetHashCode(this.<Assemblies>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<ILuaScriptResourceInfo>>.Default.GetHashCode(this.<LuaScripts>k__BackingField)) * -1521134295 + EqualityComparer<ImmutableArray<IConfigResourceInfo>>.Default.GetHashCode(this.<Configs>k__BackingField);
		}

		// Token: 0x06005642 RID: 22082 RVA: 0x002D1C1C File Offset: 0x002CFE1C
		[NullableContext(2)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ModConfigInfo);
		}

		// Token: 0x06005643 RID: 22083 RVA: 0x002D1C2C File Offset: 0x002CFE2C
		[NullableContext(2)]
		[CompilerGenerated]
		public virtual bool Equals(ModConfigInfo other)
		{
			return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<ImmutableArray<IStylesResourceInfo>>.Default.Equals(this.<Styles>k__BackingField, other.<Styles>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<Package>k__BackingField, other.<Package>k__BackingField) && EqualityComparer<ImmutableArray<IAssemblyResourceInfo>>.Default.Equals(this.<Assemblies>k__BackingField, other.<Assemblies>k__BackingField) && EqualityComparer<ImmutableArray<ILuaScriptResourceInfo>>.Default.Equals(this.<LuaScripts>k__BackingField, other.<LuaScripts>k__BackingField) && EqualityComparer<ImmutableArray<IConfigResourceInfo>>.Default.Equals(this.<Configs>k__BackingField, other.<Configs>k__BackingField));
		}

		// Token: 0x06005645 RID: 22085 RVA: 0x002D1CDC File Offset: 0x002CFEDC
		[CompilerGenerated]
		protected ModConfigInfo([Nullable(1)] ModConfigInfo original)
		{
			this.Styles = original.<Styles>k__BackingField;
			this.Package = original.<Package>k__BackingField;
			this.Assemblies = original.<Assemblies>k__BackingField;
			this.LuaScripts = original.<LuaScripts>k__BackingField;
			this.Configs = original.<Configs>k__BackingField;
		}

		// Token: 0x06005646 RID: 22086 RVA: 0x002D1D2B File Offset: 0x002CFF2B
		public ModConfigInfo()
		{
		}
	}
}
