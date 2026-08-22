using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using FluentResults;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003E5 RID: 997
	public interface IAssemblyLoaderService : IService, IDisposable
	{
		// Token: 0x17000F9F RID: 3999
		// (get) Token: 0x06003946 RID: 14662
		Guid Id { get; }

		// Token: 0x17000FA0 RID: 4000
		// (get) Token: 0x06003947 RID: 14663
		ContentPackage OwnerPackage { get; }

		// Token: 0x17000FA1 RID: 4001
		// (get) Token: 0x06003948 RID: 14664
		bool IsReferenceOnlyMode { get; }

		// Token: 0x06003949 RID: 14665
		Result AddDependencyPaths(ImmutableArray<string> paths);

		// Token: 0x0600394A RID: 14666
		Result<Assembly> CompileScriptAssembly([NotNull] string assemblyName, bool compileWithInternalAccess, ImmutableArray<SyntaxTree> syntaxTrees, ImmutableArray<MetadataReference> metadataReferences, CSharpCompilationOptions compilationOptions = null);

		// Token: 0x0600394B RID: 14667
		Result<Assembly> LoadAssemblyFromFile(string assemblyFilePath, ImmutableArray<string> additionalDependencyPaths);

		// Token: 0x0600394C RID: 14668
		Result<Assembly> GetAssemblyByName(string assemblyName);

		// Token: 0x0600394D RID: 14669
		Result<ImmutableArray<Type>> GetTypesInAssemblies();

		// Token: 0x0600394E RID: 14670
		IEnumerable<Type> UnsafeGetTypesInAssemblies();

		// Token: 0x0600394F RID: 14671
		Result<Type> GetTypeInAssemblies(string typeName);

		// Token: 0x17000FA2 RID: 4002
		// (get) Token: 0x06003950 RID: 14672
		IEnumerable<Assembly> Assemblies { get; }

		// Token: 0x17000FA3 RID: 4003
		// (get) Token: 0x06003951 RID: 14673
		IEnumerable<MetadataReference> AssemblyReferences { get; }

		// Token: 0x04001CCB RID: 7371
		public static readonly string InternalsAccessAssemblyName = "InternalsAwareAssembly";

		// Token: 0x04001CCC RID: 7372
		public const string InternalsAwareAssemblyName = "InternalsAwareAssembly";

		// Token: 0x02000C85 RID: 3205
		public interface IFactory : IService, IDisposable
		{
			// Token: 0x06006473 RID: 25715
			IAssemblyLoaderService CreateInstance(IAssemblyLoaderService.LoaderInitData initData);
		}

		// Token: 0x02000C86 RID: 3206
		public class LoaderInitData : IEquatable<IAssemblyLoaderService.LoaderInitData>
		{
			// Token: 0x06006474 RID: 25716 RVA: 0x00214EA7 File Offset: 0x002130A7
			public LoaderInitData([Required] Guid InstanceId, [Required] [NotNull] string Name, [Required] bool IsReferenceMode, ContentPackage OwnerPackage, Action<IAssemblyLoaderService> OnUnload, Func<IAssemblyLoaderService, AssemblyName, Assembly> OnResolvingManaged, Func<Assembly, string, IntPtr> OnResolvingUnmanagedDll)
			{
				this.InstanceId = InstanceId;
				this.Name = Name;
				this.IsReferenceMode = IsReferenceMode;
				this.OwnerPackage = OwnerPackage;
				this.OnUnload = OnUnload;
				this.OnResolvingManaged = OnResolvingManaged;
				this.OnResolvingUnmanagedDll = OnResolvingUnmanagedDll;
				base..ctor();
			}

			// Token: 0x1700163D RID: 5693
			// (get) Token: 0x06006475 RID: 25717 RVA: 0x00214EE4 File Offset: 0x002130E4
			[Nullable(1)]
			[CompilerGenerated]
			protected virtual Type EqualityContract
			{
				[NullableContext(1)]
				[CompilerGenerated]
				get
				{
					return typeof(IAssemblyLoaderService.LoaderInitData);
				}
			}

			// Token: 0x1700163E RID: 5694
			// (get) Token: 0x06006476 RID: 25718 RVA: 0x00214EF0 File Offset: 0x002130F0
			// (set) Token: 0x06006477 RID: 25719 RVA: 0x00214EF8 File Offset: 0x002130F8
			public Guid InstanceId { get; set; }

			// Token: 0x1700163F RID: 5695
			// (get) Token: 0x06006478 RID: 25720 RVA: 0x00214F01 File Offset: 0x00213101
			// (set) Token: 0x06006479 RID: 25721 RVA: 0x00214F09 File Offset: 0x00213109
			public string Name { get; set; }

			// Token: 0x17001640 RID: 5696
			// (get) Token: 0x0600647A RID: 25722 RVA: 0x00214F12 File Offset: 0x00213112
			// (set) Token: 0x0600647B RID: 25723 RVA: 0x00214F1A File Offset: 0x0021311A
			public bool IsReferenceMode { get; set; }

			// Token: 0x17001641 RID: 5697
			// (get) Token: 0x0600647C RID: 25724 RVA: 0x00214F23 File Offset: 0x00213123
			// (set) Token: 0x0600647D RID: 25725 RVA: 0x00214F2B File Offset: 0x0021312B
			public ContentPackage OwnerPackage { get; set; }

			// Token: 0x17001642 RID: 5698
			// (get) Token: 0x0600647E RID: 25726 RVA: 0x00214F34 File Offset: 0x00213134
			// (set) Token: 0x0600647F RID: 25727 RVA: 0x00214F3C File Offset: 0x0021313C
			public Action<IAssemblyLoaderService> OnUnload { get; set; }

			// Token: 0x17001643 RID: 5699
			// (get) Token: 0x06006480 RID: 25728 RVA: 0x00214F45 File Offset: 0x00213145
			// (set) Token: 0x06006481 RID: 25729 RVA: 0x00214F4D File Offset: 0x0021314D
			public Func<IAssemblyLoaderService, AssemblyName, Assembly> OnResolvingManaged { get; set; }

			// Token: 0x17001644 RID: 5700
			// (get) Token: 0x06006482 RID: 25730 RVA: 0x00214F56 File Offset: 0x00213156
			// (set) Token: 0x06006483 RID: 25731 RVA: 0x00214F5E File Offset: 0x0021315E
			public Func<Assembly, string, IntPtr> OnResolvingUnmanagedDll { get; set; }

			// Token: 0x06006484 RID: 25732 RVA: 0x00214F68 File Offset: 0x00213168
			[NullableContext(1)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("LoaderInitData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06006485 RID: 25733 RVA: 0x00214FB4 File Offset: 0x002131B4
			[NullableContext(1)]
			[CompilerGenerated]
			protected virtual bool PrintMembers(StringBuilder builder)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
				builder.Append("InstanceId = ");
				builder.Append(this.InstanceId.ToString());
				builder.Append(", Name = ");
				builder.Append(this.Name);
				builder.Append(", IsReferenceMode = ");
				builder.Append(this.IsReferenceMode.ToString());
				builder.Append(", OwnerPackage = ");
				builder.Append(this.OwnerPackage);
				builder.Append(", OnUnload = ");
				builder.Append(this.OnUnload);
				builder.Append(", OnResolvingManaged = ");
				builder.Append(this.OnResolvingManaged);
				builder.Append(", OnResolvingUnmanagedDll = ");
				builder.Append(this.OnResolvingUnmanagedDll);
				return true;
			}

			// Token: 0x06006486 RID: 25734 RVA: 0x00215092 File Offset: 0x00213292
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator !=(IAssemblyLoaderService.LoaderInitData left, IAssemblyLoaderService.LoaderInitData right)
			{
				return !(left == right);
			}

			// Token: 0x06006487 RID: 25735 RVA: 0x0021509E File Offset: 0x0021329E
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator ==(IAssemblyLoaderService.LoaderInitData left, IAssemblyLoaderService.LoaderInitData right)
			{
				return left == right || (left != null && left.Equals(right));
			}

			// Token: 0x06006488 RID: 25736 RVA: 0x002150B4 File Offset: 0x002132B4
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((((((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(this.<InstanceId>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Name>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsReferenceMode>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<Action<IAssemblyLoaderService>>.Default.GetHashCode(this.<OnUnload>k__BackingField)) * -1521134295 + EqualityComparer<Func<IAssemblyLoaderService, AssemblyName, Assembly>>.Default.GetHashCode(this.<OnResolvingManaged>k__BackingField)) * -1521134295 + EqualityComparer<Func<Assembly, string, IntPtr>>.Default.GetHashCode(this.<OnResolvingUnmanagedDll>k__BackingField);
			}

			// Token: 0x06006489 RID: 25737 RVA: 0x00215172 File Offset: 0x00213372
			[NullableContext(2)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return this.Equals(obj as IAssemblyLoaderService.LoaderInitData);
			}

			// Token: 0x0600648A RID: 25738 RVA: 0x00215180 File Offset: 0x00213380
			[NullableContext(2)]
			[CompilerGenerated]
			public virtual bool Equals(IAssemblyLoaderService.LoaderInitData other)
			{
				return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<Guid>.Default.Equals(this.<InstanceId>k__BackingField, other.<InstanceId>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsReferenceMode>k__BackingField, other.<IsReferenceMode>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<Action<IAssemblyLoaderService>>.Default.Equals(this.<OnUnload>k__BackingField, other.<OnUnload>k__BackingField) && EqualityComparer<Func<IAssemblyLoaderService, AssemblyName, Assembly>>.Default.Equals(this.<OnResolvingManaged>k__BackingField, other.<OnResolvingManaged>k__BackingField) && EqualityComparer<Func<Assembly, string, IntPtr>>.Default.Equals(this.<OnResolvingUnmanagedDll>k__BackingField, other.<OnResolvingUnmanagedDll>k__BackingField));
			}

			// Token: 0x0600648C RID: 25740 RVA: 0x00215268 File Offset: 0x00213468
			[CompilerGenerated]
			protected LoaderInitData([Nullable(1)] IAssemblyLoaderService.LoaderInitData original)
			{
				this.InstanceId = original.<InstanceId>k__BackingField;
				this.Name = original.<Name>k__BackingField;
				this.IsReferenceMode = original.<IsReferenceMode>k__BackingField;
				this.OwnerPackage = original.<OwnerPackage>k__BackingField;
				this.OnUnload = original.<OnUnload>k__BackingField;
				this.OnResolvingManaged = original.<OnResolvingManaged>k__BackingField;
				this.OnResolvingUnmanagedDll = original.<OnResolvingUnmanagedDll>k__BackingField;
			}

			// Token: 0x0600648D RID: 25741 RVA: 0x002152D0 File Offset: 0x002134D0
			[CompilerGenerated]
			public void Deconstruct(out Guid InstanceId, out string Name, out bool IsReferenceMode, out ContentPackage OwnerPackage, out Action<IAssemblyLoaderService> OnUnload, out Func<IAssemblyLoaderService, AssemblyName, Assembly> OnResolvingManaged, out Func<Assembly, string, IntPtr> OnResolvingUnmanagedDll)
			{
				InstanceId = this.InstanceId;
				Name = this.Name;
				IsReferenceMode = this.IsReferenceMode;
				OwnerPackage = this.OwnerPackage;
				OnUnload = this.OnUnload;
				OnResolvingManaged = this.OnResolvingManaged;
				OnResolvingUnmanagedDll = this.OnResolvingUnmanagedDll;
			}
		}
	}
}
