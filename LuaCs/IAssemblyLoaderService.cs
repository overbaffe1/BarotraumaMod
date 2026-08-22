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
	// Token: 0x020004FC RID: 1276
	public interface IAssemblyLoaderService : IService, IDisposable
	{
		// Token: 0x170014E9 RID: 5353
		// (get) Token: 0x0600529B RID: 21147
		Guid Id { get; }

		// Token: 0x170014EA RID: 5354
		// (get) Token: 0x0600529C RID: 21148
		ContentPackage OwnerPackage { get; }

		// Token: 0x170014EB RID: 5355
		// (get) Token: 0x0600529D RID: 21149
		bool IsReferenceOnlyMode { get; }

		// Token: 0x0600529E RID: 21150
		Result AddDependencyPaths(ImmutableArray<string> paths);

		// Token: 0x0600529F RID: 21151
		Result<Assembly> CompileScriptAssembly([NotNull] string assemblyName, bool compileWithInternalAccess, ImmutableArray<SyntaxTree> syntaxTrees, ImmutableArray<MetadataReference> metadataReferences, CSharpCompilationOptions compilationOptions = null);

		// Token: 0x060052A0 RID: 21152
		Result<Assembly> LoadAssemblyFromFile(string assemblyFilePath, ImmutableArray<string> additionalDependencyPaths);

		// Token: 0x060052A1 RID: 21153
		Result<Assembly> GetAssemblyByName(string assemblyName);

		// Token: 0x060052A2 RID: 21154
		Result<ImmutableArray<Type>> GetTypesInAssemblies();

		// Token: 0x060052A3 RID: 21155
		IEnumerable<Type> UnsafeGetTypesInAssemblies();

		// Token: 0x060052A4 RID: 21156
		Result<Type> GetTypeInAssemblies(string typeName);

		// Token: 0x170014EC RID: 5356
		// (get) Token: 0x060052A5 RID: 21157
		IEnumerable<Assembly> Assemblies { get; }

		// Token: 0x170014ED RID: 5357
		// (get) Token: 0x060052A6 RID: 21158
		IEnumerable<MetadataReference> AssemblyReferences { get; }

		// Token: 0x04002BC6 RID: 11206
		public static readonly string InternalsAccessAssemblyName = "InternalsAwareAssembly";

		// Token: 0x04002BC7 RID: 11207
		public const string InternalsAwareAssemblyName = "InternalsAwareAssembly";

		// Token: 0x020012C0 RID: 4800
		public interface IFactory : IService, IDisposable
		{
			// Token: 0x0600954B RID: 38219
			IAssemblyLoaderService CreateInstance(IAssemblyLoaderService.LoaderInitData initData);
		}

		// Token: 0x020012C1 RID: 4801
		public class LoaderInitData : IEquatable<IAssemblyLoaderService.LoaderInitData>
		{
			// Token: 0x0600954C RID: 38220 RVA: 0x003D3397 File Offset: 0x003D1597
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

			// Token: 0x17001CFD RID: 7421
			// (get) Token: 0x0600954D RID: 38221 RVA: 0x003D33D4 File Offset: 0x003D15D4
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

			// Token: 0x17001CFE RID: 7422
			// (get) Token: 0x0600954E RID: 38222 RVA: 0x003D33E0 File Offset: 0x003D15E0
			// (set) Token: 0x0600954F RID: 38223 RVA: 0x003D33E8 File Offset: 0x003D15E8
			public Guid InstanceId { get; set; }

			// Token: 0x17001CFF RID: 7423
			// (get) Token: 0x06009550 RID: 38224 RVA: 0x003D33F1 File Offset: 0x003D15F1
			// (set) Token: 0x06009551 RID: 38225 RVA: 0x003D33F9 File Offset: 0x003D15F9
			public string Name { get; set; }

			// Token: 0x17001D00 RID: 7424
			// (get) Token: 0x06009552 RID: 38226 RVA: 0x003D3402 File Offset: 0x003D1602
			// (set) Token: 0x06009553 RID: 38227 RVA: 0x003D340A File Offset: 0x003D160A
			public bool IsReferenceMode { get; set; }

			// Token: 0x17001D01 RID: 7425
			// (get) Token: 0x06009554 RID: 38228 RVA: 0x003D3413 File Offset: 0x003D1613
			// (set) Token: 0x06009555 RID: 38229 RVA: 0x003D341B File Offset: 0x003D161B
			public ContentPackage OwnerPackage { get; set; }

			// Token: 0x17001D02 RID: 7426
			// (get) Token: 0x06009556 RID: 38230 RVA: 0x003D3424 File Offset: 0x003D1624
			// (set) Token: 0x06009557 RID: 38231 RVA: 0x003D342C File Offset: 0x003D162C
			public Action<IAssemblyLoaderService> OnUnload { get; set; }

			// Token: 0x17001D03 RID: 7427
			// (get) Token: 0x06009558 RID: 38232 RVA: 0x003D3435 File Offset: 0x003D1635
			// (set) Token: 0x06009559 RID: 38233 RVA: 0x003D343D File Offset: 0x003D163D
			public Func<IAssemblyLoaderService, AssemblyName, Assembly> OnResolvingManaged { get; set; }

			// Token: 0x17001D04 RID: 7428
			// (get) Token: 0x0600955A RID: 38234 RVA: 0x003D3446 File Offset: 0x003D1646
			// (set) Token: 0x0600955B RID: 38235 RVA: 0x003D344E File Offset: 0x003D164E
			public Func<Assembly, string, IntPtr> OnResolvingUnmanagedDll { get; set; }

			// Token: 0x0600955C RID: 38236 RVA: 0x003D3458 File Offset: 0x003D1658
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

			// Token: 0x0600955D RID: 38237 RVA: 0x003D34A4 File Offset: 0x003D16A4
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

			// Token: 0x0600955E RID: 38238 RVA: 0x003D3582 File Offset: 0x003D1782
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator !=(IAssemblyLoaderService.LoaderInitData left, IAssemblyLoaderService.LoaderInitData right)
			{
				return !(left == right);
			}

			// Token: 0x0600955F RID: 38239 RVA: 0x003D358E File Offset: 0x003D178E
			[NullableContext(2)]
			[CompilerGenerated]
			public static bool operator ==(IAssemblyLoaderService.LoaderInitData left, IAssemblyLoaderService.LoaderInitData right)
			{
				return left == right || (left != null && left.Equals(right));
			}

			// Token: 0x06009560 RID: 38240 RVA: 0x003D35A4 File Offset: 0x003D17A4
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return ((((((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(this.<InstanceId>k__BackingField)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<Name>k__BackingField)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsReferenceMode>k__BackingField)) * -1521134295 + EqualityComparer<ContentPackage>.Default.GetHashCode(this.<OwnerPackage>k__BackingField)) * -1521134295 + EqualityComparer<Action<IAssemblyLoaderService>>.Default.GetHashCode(this.<OnUnload>k__BackingField)) * -1521134295 + EqualityComparer<Func<IAssemblyLoaderService, AssemblyName, Assembly>>.Default.GetHashCode(this.<OnResolvingManaged>k__BackingField)) * -1521134295 + EqualityComparer<Func<Assembly, string, IntPtr>>.Default.GetHashCode(this.<OnResolvingUnmanagedDll>k__BackingField);
			}

			// Token: 0x06009561 RID: 38241 RVA: 0x003D3662 File Offset: 0x003D1862
			[NullableContext(2)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return this.Equals(obj as IAssemblyLoaderService.LoaderInitData);
			}

			// Token: 0x06009562 RID: 38242 RVA: 0x003D3670 File Offset: 0x003D1870
			[NullableContext(2)]
			[CompilerGenerated]
			public virtual bool Equals(IAssemblyLoaderService.LoaderInitData other)
			{
				return this == other || (other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<Guid>.Default.Equals(this.<InstanceId>k__BackingField, other.<InstanceId>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsReferenceMode>k__BackingField, other.<IsReferenceMode>k__BackingField) && EqualityComparer<ContentPackage>.Default.Equals(this.<OwnerPackage>k__BackingField, other.<OwnerPackage>k__BackingField) && EqualityComparer<Action<IAssemblyLoaderService>>.Default.Equals(this.<OnUnload>k__BackingField, other.<OnUnload>k__BackingField) && EqualityComparer<Func<IAssemblyLoaderService, AssemblyName, Assembly>>.Default.Equals(this.<OnResolvingManaged>k__BackingField, other.<OnResolvingManaged>k__BackingField) && EqualityComparer<Func<Assembly, string, IntPtr>>.Default.Equals(this.<OnResolvingUnmanagedDll>k__BackingField, other.<OnResolvingUnmanagedDll>k__BackingField));
			}

			// Token: 0x06009564 RID: 38244 RVA: 0x003D3758 File Offset: 0x003D1958
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

			// Token: 0x06009565 RID: 38245 RVA: 0x003D37C0 File Offset: 0x003D19C0
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
