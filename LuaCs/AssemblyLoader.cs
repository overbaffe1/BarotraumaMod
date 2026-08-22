using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using FluentResults;
using FluentResults.LuaCs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004FB RID: 1275
	public sealed class AssemblyLoader : AssemblyLoadContext, IAssemblyLoaderService, IService, IDisposable
	{
		// Token: 0x170014E3 RID: 5347
		// (get) Token: 0x0600527D RID: 21117 RVA: 0x002C351A File Offset: 0x002C171A
		// (set) Token: 0x0600527E RID: 21118 RVA: 0x002C3522 File Offset: 0x002C1722
		public Guid Id { get; set; }

		// Token: 0x170014E4 RID: 5348
		// (get) Token: 0x0600527F RID: 21119 RVA: 0x002C352B File Offset: 0x002C172B
		// (set) Token: 0x06005280 RID: 21120 RVA: 0x002C3533 File Offset: 0x002C1733
		public ContentPackage OwnerPackage { get; private set; }

		// Token: 0x170014E5 RID: 5349
		// (get) Token: 0x06005281 RID: 21121 RVA: 0x002C353C File Offset: 0x002C173C
		// (set) Token: 0x06005282 RID: 21122 RVA: 0x002C3544 File Offset: 0x002C1744
		public bool IsReferenceOnlyMode { get; set; }

		// Token: 0x170014E6 RID: 5350
		// (get) Token: 0x06005283 RID: 21123 RVA: 0x002C354D File Offset: 0x002C174D
		// (set) Token: 0x06005284 RID: 21124 RVA: 0x002C355A File Offset: 0x002C175A
		public bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
			private set
			{
				ModUtils.Threading.SetBool(ref this._isDisposed, value);
			}
		}

		// Token: 0x170014E7 RID: 5351
		// (get) Token: 0x06005285 RID: 21125 RVA: 0x002C3568 File Offset: 0x002C1768
		// (set) Token: 0x06005286 RID: 21126 RVA: 0x002C357A File Offset: 0x002C177A
		private bool AreOperationRunning
		{
			get
			{
				return Interlocked.CompareExchange(ref this._operationsRunning, 0, 0) > 0;
			}
			set
			{
				if (value)
				{
					Interlocked.Add(ref this._operationsRunning, 1);
					return;
				}
				Interlocked.Add(ref this._operationsRunning, -1);
			}
		}

		// Token: 0x06005287 RID: 21127 RVA: 0x002C359C File Offset: 0x002C179C
		public AssemblyLoader(IAssemblyLoaderService.LoaderInitData initData) : base(initData.Name, true)
		{
			this.Id = initData.InstanceId;
			this.IsReferenceOnlyMode = initData.IsReferenceMode;
			this._onUnload = initData.OnUnload;
			this._onResolvingManaged = initData.OnResolvingManaged;
			this._onResolvingUnmanagedDll = initData.OnResolvingUnmanagedDll;
			this.OwnerPackage = initData.OwnerPackage;
			base.Unloading += this.OnUnload;
			base.Resolving += this.OnResolvingManagedAssembly;
			base.ResolvingUnmanagedDll += this.OnResolvingUnmanagedDll;
		}

		// Token: 0x06005288 RID: 21128 RVA: 0x002C36A0 File Offset: 0x002C18A0
		private IntPtr OnResolvingUnmanagedDll(Assembly invokingAssembly, string assemblyName)
		{
			if (this.IsDisposed)
			{
				return (IntPtr)0;
			}
			if (this._isResolvingNative.Value)
			{
				return (IntPtr)0;
			}
			this.AreOperationRunning = true;
			this._isResolvingNative.Value = true;
			IntPtr result;
			try
			{
				if (!this._dependencyResolvers.IsEmpty)
				{
					foreach (KeyValuePair<string, AssemblyDependencyResolver> resolver in this._dependencyResolvers)
					{
						try
						{
							string path = resolver.Value.ResolveUnmanagedDllToPath(assemblyName);
							if (!path.IsNullOrWhiteSpace())
							{
								return base.LoadUnmanagedDllFromPath(path);
							}
						}
						catch
						{
						}
					}
				}
				if (this._onResolvingUnmanagedDll != null)
				{
					try
					{
						return this._onResolvingUnmanagedDll(invokingAssembly, assemblyName);
					}
					catch
					{
					}
				}
				result = (IntPtr)0;
			}
			finally
			{
				this.AreOperationRunning = false;
				this._isResolvingNative.Value = false;
			}
			return result;
		}

		// Token: 0x06005289 RID: 21129 RVA: 0x002C37A0 File Offset: 0x002C19A0
		private Assembly OnResolvingManagedAssembly(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
		{
			if (this.IsDisposed)
			{
				return null;
			}
			if (this._isResolving.Value)
			{
				return null;
			}
			if (assemblyLoadContext != this)
			{
				return null;
			}
			this.AreOperationRunning = true;
			this._isResolving.Value = true;
			Assembly result;
			try
			{
				if (!this._dependencyResolvers.IsEmpty)
				{
					foreach (KeyValuePair<string, AssemblyDependencyResolver> resolver in this._dependencyResolvers)
					{
						try
						{
							string path = resolver.Value.ResolveAssemblyToPath(assemblyName);
							if (!path.IsNullOrWhiteSpace())
							{
								return assemblyLoadContext.LoadFromAssemblyPath(path);
							}
						}
						catch
						{
						}
					}
				}
				if (this._onResolvingManaged != null)
				{
					try
					{
						return this._onResolvingManaged(this, assemblyName);
					}
					catch
					{
					}
				}
				result = null;
			}
			finally
			{
				this.AreOperationRunning = false;
				this._isResolving.Value = false;
			}
			return result;
		}

		// Token: 0x170014E8 RID: 5352
		// (get) Token: 0x0600528A RID: 21130 RVA: 0x002C38A4 File Offset: 0x002C1AA4
		public IEnumerable<MetadataReference> AssemblyReferences
		{
			get
			{
				AssemblyLoader.<get_AssemblyReferences>d__32 <get_AssemblyReferences>d__ = new AssemblyLoader.<get_AssemblyReferences>d__32(-2);
				<get_AssemblyReferences>d__.<>4__this = this;
				return <get_AssemblyReferences>d__;
			}
		}

		// Token: 0x0600528B RID: 21131 RVA: 0x002C38C4 File Offset: 0x002C1AC4
		public Result AddDependencyPaths(ImmutableArray<string> paths)
		{
			if (this.IsDisposed)
			{
				return Result.Fail("Loader is disposed!");
			}
			this.AreOperationRunning = true;
			Result result;
			try
			{
				if (paths.Length == 0)
				{
					result = Result.Ok();
				}
				else
				{
					Result res = new Result();
					foreach (string path in paths)
					{
						try
						{
							string p = Path.GetFullPath(path.CleanUpPath());
							if (!this._dependencyResolvers.ContainsKey(p))
							{
								this._dependencyResolvers[p] = new AssemblyDependencyResolver(p);
							}
						}
						catch (Exception ex)
						{
							res = res.WithError(new ExceptionalError(ex).WithMetadata(MetadataType.Sources, path));
						}
					}
					if (res.Errors.Any<IError>())
					{
						result = Result.Fail(res.Errors);
					}
					else
					{
						result = Result.Ok();
					}
				}
			}
			finally
			{
				this.AreOperationRunning = false;
			}
			return result;
		}

		// Token: 0x0600528C RID: 21132 RVA: 0x002C39B8 File Offset: 0x002C1BB8
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Result<Assembly> CompileScriptAssembly([NotNull] string assemblyName, bool compileWithInternalAccess, ImmutableArray<SyntaxTree> syntaxTrees, ImmutableArray<MetadataReference> metadataReferences, CSharpCompilationOptions compilationOptions = null)
		{
			if (this.IsDisposed)
			{
				return Result.Fail("Loader is disposed!");
			}
			this.AreOperationRunning = true;
			Result<Assembly> result2;
			try
			{
				if (assemblyName.IsNullOrWhiteSpace())
				{
					result2 = new Result<Assembly>().WithError(new Error("The name provided is null!").WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, syntaxTrees));
				}
				else if (this._loadedAssemblyData.ContainsKey(assemblyName))
				{
					result2 = new Result<Assembly>().WithError(new Error("The name provided is already assigned to an assembly!").WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, syntaxTrees));
				}
				else
				{
					string compilationAssemblyName = compileWithInternalAccess ? "InternalsAwareAssembly" : assemblyName;
					if (compilationOptions == null)
					{
						compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, null, null, OptimizationLevel.Release, false, true, null, null, default(ImmutableArray<byte>), null, Platform.AnyCpu, ReportDiagnostic.Default, 0, null, true, false, null, null, null, null, null, false, MetadataImportOptions.Public, NullableContextOptions.Disable);
					}
					if (!compileWithInternalAccess)
					{
						PropertyInfo property = typeof(CSharpCompilationOptions).GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic);
						if (property != null)
						{
							property.SetValue(compilationOptions, 37748738U);
						}
					}
					using (MemoryStream asmMemoryStream = new MemoryStream())
					{
						EmitResult result = CSharpCompilation.Create(compilationAssemblyName, syntaxTrees, metadataReferences, compilationOptions).Emit(asmMemoryStream, null, null, null, null, null, null, null, null, null, default(CancellationToken));
						if (!result.Success)
						{
							StringBuilder sb = new StringBuilder();
							foreach (Diagnostic resultDiagnostic in result.Diagnostics)
							{
								if (resultDiagnostic.IsWarningAsError || resultDiagnostic.Severity == DiagnosticSeverity.Error)
								{
									StringBuilder stringBuilder = sb;
									StringBuilder stringBuilder2 = stringBuilder;
									StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
									appendInterpolatedStringHandler.AppendLiteral("\n");
									appendInterpolatedStringHandler.AppendFormatted<Diagnostic>(resultDiagnostic);
									stringBuilder2.AppendLine(ref appendInterpolatedStringHandler);
								}
							}
							ResultBase<Result> resultBase = new Result();
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Package Error: ");
							defaultInterpolatedStringHandler.AppendFormatted(this.OwnerPackage.Name);
							defaultInterpolatedStringHandler.AppendLiteral(": Compilation failed for assembly ");
							defaultInterpolatedStringHandler.AppendFormatted(assemblyName);
							defaultInterpolatedStringHandler.AppendLiteral("!\n ");
							defaultInterpolatedStringHandler.AppendFormatted(sb.ToString());
							defaultInterpolatedStringHandler.AppendLiteral("\n");
							Result res = resultBase.WithError(new Error(defaultInterpolatedStringHandler.ToStringAndClear()).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, syntaxTrees));
							result2 = res;
						}
						else
						{
							asmMemoryStream.Seek(0L, SeekOrigin.Begin);
							AssemblyLoader.AssemblyData data = new AssemblyLoader.AssemblyData(base.LoadFromStream(asmMemoryStream), asmMemoryStream.ToArray());
							this._loadedAssemblyData[data.Assembly] = data;
							result2 = new Result<Assembly>().WithSuccess("Compiled assembly " + assemblyName + " successful.").WithValue(data.Assembly);
						}
					}
				}
			}
			catch (Exception ex)
			{
				result2 = new Result().WithError(new ExceptionalError(ex).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, assemblyName).WithMetadata(MetadataType.Sources, syntaxTrees));
			}
			finally
			{
				this.AreOperationRunning = false;
			}
			return result2;
		}

		// Token: 0x0600528D RID: 21133 RVA: 0x002C3D30 File Offset: 0x002C1F30
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Result<Assembly> LoadAssemblyFromFile(string assemblyFilePath, ImmutableArray<string> additionalDependencyPaths)
		{
			AssemblyLoader.<>c__DisplayClass35_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.assemblyFilePath = assemblyFilePath;
			if (this.IsDisposed)
			{
				return Result.Fail("Loader is disposed!");
			}
			this.AreOperationRunning = true;
			Result<Assembly> result;
			try
			{
				if (CS$<>8__locals1.assemblyFilePath.IsNullOrWhiteSpace())
				{
					result = new Result<Assembly>().WithError(new Error("The path provided is empty."));
				}
				else
				{
					if (additionalDependencyPaths.Any<string>())
					{
						Result r = this.AddDependencyPaths(additionalDependencyPaths);
						if (r.IsFailed)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to load dependency paths for '");
							defaultInterpolatedStringHandler.AppendFormatted(CS$<>8__locals1.assemblyFilePath);
							defaultInterpolatedStringHandler.AppendLiteral("' with paths: ");
							defaultInterpolatedStringHandler.AppendFormatted(additionalDependencyPaths.Aggregate((string s, string ac) => ac + "| P=" + s));
							defaultInterpolatedStringHandler.AppendLiteral(".");
							return Result.Fail(new Error(defaultInterpolatedStringHandler.ToStringAndClear()).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, CS$<>8__locals1.assemblyFilePath)).WithErrors(r.Errors);
						}
					}
					string sanitizedFilePath = Path.GetFullPath(CS$<>8__locals1.assemblyFilePath.CleanUpPath());
					if (Path.GetDirectoryName(sanitizedFilePath) == null)
					{
						result = Result.Fail(new Error("Unable to load assembly: bath file path: " + CS$<>8__locals1.assemblyFilePath).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, sanitizedFilePath));
					}
					else
					{
						try
						{
							Assembly assembly = base.LoadFromAssemblyPath(sanitizedFilePath);
							this._loadedAssemblyData[assembly] = new AssemblyLoader.AssemblyData(assembly, assembly.Location);
							ResultBase<Result<Assembly>> resultBase = new Result<Assembly>();
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
							defaultInterpolatedStringHandler2.AppendLiteral("Loaded assembly '");
							defaultInterpolatedStringHandler2.AppendFormatted<AssemblyName>(assembly.GetName());
							defaultInterpolatedStringHandler2.AppendLiteral("'");
							result = resultBase.WithSuccess(defaultInterpolatedStringHandler2.ToStringAndClear()).WithValue(assembly);
						}
						catch (FileNotFoundException fnfe)
						{
							try
							{
								AssemblyName assemblyName = new AssemblyName(Path.GetFileName(sanitizedFilePath));
								foreach (KeyValuePair<string, AssemblyDependencyResolver> resolver in this._dependencyResolvers)
								{
									try
									{
										string path = resolver.Value.ResolveAssemblyToPath(assemblyName);
										return base.LoadFromAssemblyPath(path);
									}
									catch
									{
									}
								}
								result = this.<LoadAssemblyFromFile>g__GenerateExceptionReturn|35_0<FileNotFoundException>(fnfe, ref CS$<>8__locals1);
							}
							catch (Exception e)
							{
								result = this.<LoadAssemblyFromFile>g__GenerateExceptionReturn|35_0<FileNotFoundException>(fnfe, ref CS$<>8__locals1);
							}
						}
						catch (Exception e2)
						{
							result = this.<LoadAssemblyFromFile>g__GenerateExceptionReturn|35_0<Exception>(e2, ref CS$<>8__locals1);
						}
					}
				}
			}
			finally
			{
				this.AreOperationRunning = false;
			}
			return result;
		}

		// Token: 0x0600528E RID: 21134 RVA: 0x002C4054 File Offset: 0x002C2254
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Result<Assembly> GetAssemblyByName(string assemblyName)
		{
			if (this.IsDisposed)
			{
				return Result.Fail(new Error("Loader is disposed!"));
			}
			if (assemblyName.IsNullOrWhiteSpace())
			{
				return Result.Fail(new Error("Assembly name is empty.").WithMetadata(MetadataType.ExceptionObject, this));
			}
			this.AreOperationRunning = true;
			Result<Assembly> result;
			try
			{
				AssemblyLoader.AssemblyData data;
				if (this._loadedAssemblyData.TryGetValue(assemblyName, out data))
				{
					result = new Result<Assembly>().WithSuccess(new Success("Assembly found.")).WithValue(data.Assembly);
				}
				else
				{
					foreach (Assembly assembly in from a in base.Assemblies
					where !this._loadedAssemblyData.ContainsKey(a)
					select a)
					{
						if (assembly.GetName().FullName == assemblyName)
						{
							try
							{
								if (!assembly.Location.IsNullOrWhiteSpace())
								{
									this._loadedAssemblyData[assembly] = new AssemblyLoader.AssemblyData(assembly, assembly.Location);
								}
							}
							catch (NotSupportedException nse)
							{
							}
							return new Result<Assembly>().WithSuccess(new Success("Assembly found.")).WithValue(assembly);
						}
					}
					result = Result.Fail(new Error("Assembly named '" + assemblyName + "' not found!"));
				}
			}
			finally
			{
				this.AreOperationRunning = false;
			}
			return result;
		}

		// Token: 0x0600528F RID: 21135 RVA: 0x002C41D4 File Offset: 0x002C23D4
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Result<ImmutableArray<Type>> GetTypesInAssemblies()
		{
			if (this.IsDisposed)
			{
				return Result.Fail(new Error("Loader is disposed!"));
			}
			this.AreOperationRunning = true;
			Result<ImmutableArray<Type>> result;
			try
			{
				result = new Result<ImmutableArray<Type>>().WithValue(this._loadedAssemblyData.SelectMany((KeyValuePair<AssemblyLoader.AssemblyOrStringKey, AssemblyLoader.AssemblyData> kvp) => kvp.Value.Types).ToImmutableArray<Type>());
			}
			catch (Exception e)
			{
				result = Result.Fail(new ExceptionalError(e));
			}
			finally
			{
				this.AreOperationRunning = false;
			}
			return result;
		}

		// Token: 0x06005290 RID: 21136 RVA: 0x002C427C File Offset: 0x002C247C
		[MethodImpl(MethodImplOptions.NoInlining)]
		public IEnumerable<Type> UnsafeGetTypesInAssemblies()
		{
			AssemblyLoader.<UnsafeGetTypesInAssemblies>d__38 <UnsafeGetTypesInAssemblies>d__ = new AssemblyLoader.<UnsafeGetTypesInAssemblies>d__38(-2);
			<UnsafeGetTypesInAssemblies>d__.<>4__this = this;
			return <UnsafeGetTypesInAssemblies>d__;
		}

		// Token: 0x06005291 RID: 21137 RVA: 0x002C428C File Offset: 0x002C248C
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Result<Type> GetTypeInAssemblies(string typeName)
		{
			if (this.IsDisposed)
			{
				return Result.Fail(new Error("Loader is disposed!"));
			}
			this.AreOperationRunning = true;
			Result<Type> result;
			try
			{
				if (this._loadedAssemblyData.IsEmpty)
				{
					result = Result.Fail(new Error("No assemblies loaded!"));
				}
				else
				{
					foreach (KeyValuePair<AssemblyLoader.AssemblyOrStringKey, AssemblyLoader.AssemblyData> assemblyData in this._loadedAssemblyData)
					{
						Type type;
						if (assemblyData.Value.TypesByName.TryGetValue(typeName, out type))
						{
							return new Result<Type>().WithSuccess("Found type.").WithValue(type);
						}
					}
					result = Result.Fail(new Error("No matching types found for " + typeName + "!"));
				}
			}
			finally
			{
				this.AreOperationRunning = false;
			}
			return result;
		}

		// Token: 0x06005292 RID: 21138 RVA: 0x002C4384 File Offset: 0x002C2584
		public void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			this.IsDisposed = true;
			base.Unload();
			GC.SuppressFinalize(this);
		}

		// Token: 0x06005293 RID: 21139 RVA: 0x002C43A4 File Offset: 0x002C25A4
		~AssemblyLoader()
		{
			base.Unload();
		}

		// Token: 0x06005294 RID: 21140 RVA: 0x002C43D0 File Offset: 0x002C25D0
		private void OnUnload(AssemblyLoadContext context)
		{
			DateTime timeout = DateTime.Now.AddSeconds(2.0);
			while (timeout > DateTime.Now && this.AreOperationRunning)
			{
				Thread.Sleep(15);
			}
			Action<IAssemblyLoaderService> onUnload = this._onUnload;
			if (onUnload != null)
			{
				onUnload(this);
			}
			this.DisposeInternal();
		}

		// Token: 0x06005295 RID: 21141 RVA: 0x002C442C File Offset: 0x002C262C
		private void DisposeInternal()
		{
			this.IsDisposed = true;
			base.Resolving -= this.OnResolvingManagedAssembly;
			base.ResolvingUnmanagedDll -= this.OnResolvingUnmanagedDll;
			base.Unloading -= this.OnUnload;
			this._dependencyResolvers.Clear();
			this._loadedAssemblyData.Clear();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
			GC.WaitForFullGCComplete(10);
		}

		// Token: 0x06005296 RID: 21142 RVA: 0x002C44A4 File Offset: 0x002C26A4
		protected override Assembly Load(AssemblyName assemblyName)
		{
			if (this.IsDisposed)
			{
				return null;
			}
			this.AreOperationRunning = true;
			Assembly result;
			try
			{
				AssemblyLoader.AssemblyData assembly;
				if (this._loadedAssemblyData.TryGetValue(assemblyName.FullName, out assembly))
				{
					result = assembly.Assembly;
				}
				else
				{
					result = null;
				}
			}
			catch
			{
				result = null;
			}
			finally
			{
				this.AreOperationRunning = false;
			}
			return result;
		}

		// Token: 0x06005297 RID: 21143 RVA: 0x002C4514 File Offset: 0x002C2714
		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
		{
			if (this.IsDisposed)
			{
				return (IntPtr)0;
			}
			GCHandle? handle = null;
			this.AreOperationRunning = true;
			try
			{
				AssemblyLoader.AssemblyData assemblyData;
				if (this._loadedAssemblyData.TryGetValue(unmanagedDllName, out assemblyData))
				{
					handle = new GCHandle?(GCHandle.Alloc(assemblyData.Assembly, GCHandleType.Pinned));
					return GCHandle.ToIntPtr(handle.Value);
				}
			}
			catch
			{
				return (IntPtr)0;
			}
			finally
			{
				this.AreOperationRunning = false;
				try
				{
					if (handle != null)
					{
						handle.Value.Free();
					}
				}
				catch
				{
				}
			}
			return (IntPtr)0;
		}

		// Token: 0x06005298 RID: 21144 RVA: 0x002C45D0 File Offset: 0x002C27D0
		IEnumerable<Assembly> IAssemblyLoaderService.get_Assemblies()
		{
			return base.Assemblies;
		}

		// Token: 0x06005299 RID: 21145 RVA: 0x002C45D8 File Offset: 0x002C27D8
		[CompilerGenerated]
		private Result<Assembly> <LoadAssemblyFromFile>g__GenerateExceptionReturn|35_0<T>(T exception, ref AssemblyLoader.<>c__DisplayClass35_0 A_2) where T : Exception
		{
			return Result.Fail<Assembly>(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, A_2.assemblyFilePath).WithMetadata(MetadataType.ExceptionDetails, exception.Message).WithMetadata(MetadataType.StackTrace, exception.StackTrace));
		}

		// Token: 0x04002BBD RID: 11197
		private int _isDisposed;

		// Token: 0x04002BBE RID: 11198
		private int _operationsRunning;

		// Token: 0x04002BBF RID: 11199
		private readonly Action<IAssemblyLoaderService> _onUnload;

		// Token: 0x04002BC0 RID: 11200
		private readonly Func<IAssemblyLoaderService, AssemblyName, Assembly> _onResolvingManaged;

		// Token: 0x04002BC1 RID: 11201
		private readonly Func<Assembly, string, IntPtr> _onResolvingUnmanagedDll;

		// Token: 0x04002BC2 RID: 11202
		private readonly ConcurrentDictionary<string, AssemblyDependencyResolver> _dependencyResolvers = new ConcurrentDictionary<string, AssemblyDependencyResolver>();

		// Token: 0x04002BC3 RID: 11203
		private readonly ConcurrentDictionary<AssemblyLoader.AssemblyOrStringKey, AssemblyLoader.AssemblyData> _loadedAssemblyData = new ConcurrentDictionary<AssemblyLoader.AssemblyOrStringKey, AssemblyLoader.AssemblyData>();

		// Token: 0x04002BC4 RID: 11204
		private readonly ThreadLocal<bool> _isResolving = new ThreadLocal<bool>(() => false);

		// Token: 0x04002BC5 RID: 11205
		private readonly ThreadLocal<bool> _isResolvingNative = new ThreadLocal<bool>(() => false);

		// Token: 0x020012B9 RID: 4793
		public class Factory : IAssemblyLoaderService.IFactory, IService, IDisposable
		{
			// Token: 0x06009514 RID: 38164 RVA: 0x003D28A8 File Offset: 0x003D0AA8
			public IAssemblyLoaderService CreateInstance(IAssemblyLoaderService.LoaderInitData initData)
			{
				return new AssemblyLoader(initData);
			}

			// Token: 0x06009515 RID: 38165 RVA: 0x003D28B0 File Offset: 0x003D0AB0
			public void Dispose()
			{
			}

			// Token: 0x17001CF6 RID: 7414
			// (get) Token: 0x06009516 RID: 38166 RVA: 0x003D28B2 File Offset: 0x003D0AB2
			public bool IsDisposed
			{
				get
				{
					return false;
				}
			}
		}

		// Token: 0x020012BA RID: 4794
		private readonly struct AssemblyData : IEquatable<AssemblyLoader.AssemblyData>
		{
			// Token: 0x06009518 RID: 38168 RVA: 0x003D28C0 File Offset: 0x003D0AC0
			[MethodImpl(MethodImplOptions.NoOptimization)]
			public AssemblyData(Assembly assembly, byte[] assemblyImage)
			{
				if (assembly == null)
				{
					throw new ArgumentNullException("assembly");
				}
				this.Assembly = assembly;
				if (assemblyImage == null)
				{
					throw new ArgumentNullException("assemblyImage");
				}
				this.AssemblyImageOrPath = assemblyImage;
				this.AssemblyReference = MetadataReference.CreateFromImage(assemblyImage, default(MetadataReferenceProperties), null, null);
				this.Types = assembly.GetSafeTypes().ToImmutableArray<Type>();
				this.TypesByName = this.Types.ToImmutableDictionary((Type type) => type.FullName, (Type type) => type);
			}

			// Token: 0x06009519 RID: 38169 RVA: 0x003D297C File Offset: 0x003D0B7C
			[MethodImpl(MethodImplOptions.NoOptimization)]
			public AssemblyData(Assembly assembly, string path)
			{
				if (assembly == null)
				{
					throw new ArgumentNullException("assembly");
				}
				this.Assembly = assembly;
				if (path == null)
				{
					throw new ArgumentNullException("path");
				}
				this.AssemblyImageOrPath = path;
				this.AssemblyReference = MetadataReference.CreateFromFile(path, default(MetadataReferenceProperties), null);
				this.Types = assembly.GetSafeTypes().ToImmutableArray<Type>();
				this.TypesByName = this.Types.ToImmutableDictionary((Type type) => type.FullName, (Type type) => type);
			}

			// Token: 0x0600951A RID: 38170 RVA: 0x003D2A38 File Offset: 0x003D0C38
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("AssemblyData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600951B RID: 38171 RVA: 0x003D2A84 File Offset: 0x003D0C84
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Assembly = ");
				builder.Append(this.Assembly);
				builder.Append(", AssemblyImageOrPath = ");
				builder.Append(this.AssemblyImageOrPath.ToString());
				builder.Append(", AssemblyReference = ");
				builder.Append(this.AssemblyReference);
				builder.Append(", Types = ");
				builder.Append(this.Types.ToString());
				builder.Append(", TypesByName = ");
				builder.Append(this.TypesByName);
				return true;
			}

			// Token: 0x0600951C RID: 38172 RVA: 0x003D2B25 File Offset: 0x003D0D25
			[CompilerGenerated]
			public static bool operator !=(AssemblyLoader.AssemblyData left, AssemblyLoader.AssemblyData right)
			{
				return !(left == right);
			}

			// Token: 0x0600951D RID: 38173 RVA: 0x003D2B31 File Offset: 0x003D0D31
			[CompilerGenerated]
			public static bool operator ==(AssemblyLoader.AssemblyData left, AssemblyLoader.AssemblyData right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600951E RID: 38174 RVA: 0x003D2B3C File Offset: 0x003D0D3C
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((EqualityComparer<Assembly>.Default.GetHashCode(this.Assembly) * -1521134295 + EqualityComparer<OneOf<byte[], string>>.Default.GetHashCode(this.AssemblyImageOrPath)) * -1521134295 + EqualityComparer<MetadataReference>.Default.GetHashCode(this.AssemblyReference)) * -1521134295 + EqualityComparer<ImmutableArray<Type>>.Default.GetHashCode(this.Types)) * -1521134295 + EqualityComparer<ImmutableDictionary<string, Type>>.Default.GetHashCode(this.TypesByName);
			}

			// Token: 0x0600951F RID: 38175 RVA: 0x003D2BB5 File Offset: 0x003D0DB5
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is AssemblyLoader.AssemblyData && this.Equals((AssemblyLoader.AssemblyData)obj);
			}

			// Token: 0x06009520 RID: 38176 RVA: 0x003D2BD0 File Offset: 0x003D0DD0
			[CompilerGenerated]
			public bool Equals(AssemblyLoader.AssemblyData other)
			{
				return EqualityComparer<Assembly>.Default.Equals(this.Assembly, other.Assembly) && EqualityComparer<OneOf<byte[], string>>.Default.Equals(this.AssemblyImageOrPath, other.AssemblyImageOrPath) && EqualityComparer<MetadataReference>.Default.Equals(this.AssemblyReference, other.AssemblyReference) && EqualityComparer<ImmutableArray<Type>>.Default.Equals(this.Types, other.Types) && EqualityComparer<ImmutableDictionary<string, Type>>.Default.Equals(this.TypesByName, other.TypesByName);
			}

			// Token: 0x0400601E RID: 24606
			public readonly Assembly Assembly;

			// Token: 0x0400601F RID: 24607
			public readonly OneOf<byte[], string> AssemblyImageOrPath;

			// Token: 0x04006020 RID: 24608
			public readonly MetadataReference AssemblyReference;

			// Token: 0x04006021 RID: 24609
			public readonly ImmutableArray<Type> Types;

			// Token: 0x04006022 RID: 24610
			public readonly ImmutableDictionary<string, Type> TypesByName;
		}

		// Token: 0x020012BB RID: 4795
		private readonly struct AssemblyOrStringKey : IEquatable<AssemblyLoader.AssemblyOrStringKey>, IEqualityComparer<AssemblyLoader.AssemblyOrStringKey>
		{
			// Token: 0x17001CF7 RID: 7415
			// (get) Token: 0x06009521 RID: 38177 RVA: 0x003D2C55 File Offset: 0x003D0E55
			// (set) Token: 0x06009522 RID: 38178 RVA: 0x003D2C5D File Offset: 0x003D0E5D
			public Assembly Assembly { get; set; }

			// Token: 0x17001CF8 RID: 7416
			// (get) Token: 0x06009523 RID: 38179 RVA: 0x003D2C66 File Offset: 0x003D0E66
			// (set) Token: 0x06009524 RID: 38180 RVA: 0x003D2C6E File Offset: 0x003D0E6E
			public string AssemblyName { get; set; }

			// Token: 0x06009525 RID: 38181 RVA: 0x003D2C78 File Offset: 0x003D0E78
			public AssemblyOrStringKey(Assembly assembly)
			{
				if (assembly == null)
				{
					throw new ArgumentNullException("assembly");
				}
				this.Assembly = assembly;
				this.AssemblyName = assembly.GetName().FullName;
				if (this.AssemblyName == null)
				{
					throw new ArgumentNullException("AssemblyName");
				}
				this.HashCode = this.AssemblyName.GetHashCode();
			}

			// Token: 0x06009526 RID: 38182 RVA: 0x003D2CD5 File Offset: 0x003D0ED5
			[MethodImpl(MethodImplOptions.NoOptimization)]
			public AssemblyOrStringKey(string assemblyName)
			{
				if (assemblyName.IsNullOrWhiteSpace())
				{
					throw new ArgumentNullException("assemblyName");
				}
				this.Assembly = null;
				this.AssemblyName = assemblyName;
				this.HashCode = this.AssemblyName.GetHashCode();
			}

			// Token: 0x06009527 RID: 38183 RVA: 0x003D2D09 File Offset: 0x003D0F09
			public bool Equals(AssemblyLoader.AssemblyOrStringKey x, AssemblyLoader.AssemblyOrStringKey y)
			{
				if (x.Assembly != null && y.Assembly != null)
				{
					return x.Assembly == y.Assembly;
				}
				return x.AssemblyName == y.AssemblyName;
			}

			// Token: 0x06009528 RID: 38184 RVA: 0x003D2D44 File Offset: 0x003D0F44
			public int GetHashCode(AssemblyLoader.AssemblyOrStringKey obj)
			{
				return this.HashCode;
			}

			// Token: 0x06009529 RID: 38185 RVA: 0x003D2D4C File Offset: 0x003D0F4C
			public static implicit operator AssemblyLoader.AssemblyOrStringKey(Assembly assembly)
			{
				return new AssemblyLoader.AssemblyOrStringKey(assembly);
			}

			// Token: 0x0600952A RID: 38186 RVA: 0x003D2D54 File Offset: 0x003D0F54
			public static implicit operator AssemblyLoader.AssemblyOrStringKey(string name)
			{
				return new AssemblyLoader.AssemblyOrStringKey(name);
			}

			// Token: 0x0600952B RID: 38187 RVA: 0x003D2D5C File Offset: 0x003D0F5C
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("AssemblyOrStringKey");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600952C RID: 38188 RVA: 0x003D2DA8 File Offset: 0x003D0FA8
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Assembly = ");
				builder.Append(this.Assembly);
				builder.Append(", AssemblyName = ");
				builder.Append(this.AssemblyName);
				builder.Append(", HashCode = ");
				builder.Append(this.HashCode.ToString());
				return true;
			}

			// Token: 0x0600952D RID: 38189 RVA: 0x003D2E0C File Offset: 0x003D100C
			[CompilerGenerated]
			public static bool operator !=(AssemblyLoader.AssemblyOrStringKey left, AssemblyLoader.AssemblyOrStringKey right)
			{
				return !(left == right);
			}

			// Token: 0x0600952E RID: 38190 RVA: 0x003D2E18 File Offset: 0x003D1018
			[CompilerGenerated]
			public static bool operator ==(AssemblyLoader.AssemblyOrStringKey left, AssemblyLoader.AssemblyOrStringKey right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600952F RID: 38191 RVA: 0x003D2E22 File Offset: 0x003D1022
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Assembly>.Default.GetHashCode(this.<Assembly>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<AssemblyName>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.HashCode);
			}

			// Token: 0x06009530 RID: 38192 RVA: 0x003D2E62 File Offset: 0x003D1062
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is AssemblyLoader.AssemblyOrStringKey && this.Equals((AssemblyLoader.AssemblyOrStringKey)obj);
			}

			// Token: 0x06009531 RID: 38193 RVA: 0x003D2E7C File Offset: 0x003D107C
			[CompilerGenerated]
			public bool Equals(AssemblyLoader.AssemblyOrStringKey other)
			{
				return EqualityComparer<Assembly>.Default.Equals(this.<Assembly>k__BackingField, other.<Assembly>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<AssemblyName>k__BackingField, other.<AssemblyName>k__BackingField) && EqualityComparer<int>.Default.Equals(this.HashCode, other.HashCode);
			}

			// Token: 0x04006025 RID: 24613
			public readonly int HashCode;
		}
	}
}
