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
	// Token: 0x020003E4 RID: 996
	public sealed class AssemblyLoader : AssemblyLoadContext, IAssemblyLoaderService, IService, IDisposable
	{
		// Token: 0x17000F99 RID: 3993
		// (get) Token: 0x06003928 RID: 14632 RVA: 0x0017D1DA File Offset: 0x0017B3DA
		// (set) Token: 0x06003929 RID: 14633 RVA: 0x0017D1E2 File Offset: 0x0017B3E2
		public Guid Id { get; set; }

		// Token: 0x17000F9A RID: 3994
		// (get) Token: 0x0600392A RID: 14634 RVA: 0x0017D1EB File Offset: 0x0017B3EB
		// (set) Token: 0x0600392B RID: 14635 RVA: 0x0017D1F3 File Offset: 0x0017B3F3
		public ContentPackage OwnerPackage { get; private set; }

		// Token: 0x17000F9B RID: 3995
		// (get) Token: 0x0600392C RID: 14636 RVA: 0x0017D1FC File Offset: 0x0017B3FC
		// (set) Token: 0x0600392D RID: 14637 RVA: 0x0017D204 File Offset: 0x0017B404
		public bool IsReferenceOnlyMode { get; set; }

		// Token: 0x17000F9C RID: 3996
		// (get) Token: 0x0600392E RID: 14638 RVA: 0x0017D20D File Offset: 0x0017B40D
		// (set) Token: 0x0600392F RID: 14639 RVA: 0x0017D21A File Offset: 0x0017B41A
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

		// Token: 0x17000F9D RID: 3997
		// (get) Token: 0x06003930 RID: 14640 RVA: 0x0017D228 File Offset: 0x0017B428
		// (set) Token: 0x06003931 RID: 14641 RVA: 0x0017D23A File Offset: 0x0017B43A
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

		// Token: 0x06003932 RID: 14642 RVA: 0x0017D25C File Offset: 0x0017B45C
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

		// Token: 0x06003933 RID: 14643 RVA: 0x0017D360 File Offset: 0x0017B560
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

		// Token: 0x06003934 RID: 14644 RVA: 0x0017D460 File Offset: 0x0017B660
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

		// Token: 0x17000F9E RID: 3998
		// (get) Token: 0x06003935 RID: 14645 RVA: 0x0017D564 File Offset: 0x0017B764
		public IEnumerable<MetadataReference> AssemblyReferences
		{
			get
			{
				AssemblyLoader.<get_AssemblyReferences>d__32 <get_AssemblyReferences>d__ = new AssemblyLoader.<get_AssemblyReferences>d__32(-2);
				<get_AssemblyReferences>d__.<>4__this = this;
				return <get_AssemblyReferences>d__;
			}
		}

		// Token: 0x06003936 RID: 14646 RVA: 0x0017D584 File Offset: 0x0017B784
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

		// Token: 0x06003937 RID: 14647 RVA: 0x0017D678 File Offset: 0x0017B878
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

		// Token: 0x06003938 RID: 14648 RVA: 0x0017D9F0 File Offset: 0x0017BBF0
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

		// Token: 0x06003939 RID: 14649 RVA: 0x0017DD14 File Offset: 0x0017BF14
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

		// Token: 0x0600393A RID: 14650 RVA: 0x0017DE94 File Offset: 0x0017C094
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

		// Token: 0x0600393B RID: 14651 RVA: 0x0017DF3C File Offset: 0x0017C13C
		[MethodImpl(MethodImplOptions.NoInlining)]
		public IEnumerable<Type> UnsafeGetTypesInAssemblies()
		{
			AssemblyLoader.<UnsafeGetTypesInAssemblies>d__38 <UnsafeGetTypesInAssemblies>d__ = new AssemblyLoader.<UnsafeGetTypesInAssemblies>d__38(-2);
			<UnsafeGetTypesInAssemblies>d__.<>4__this = this;
			return <UnsafeGetTypesInAssemblies>d__;
		}

		// Token: 0x0600393C RID: 14652 RVA: 0x0017DF4C File Offset: 0x0017C14C
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

		// Token: 0x0600393D RID: 14653 RVA: 0x0017E044 File Offset: 0x0017C244
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

		// Token: 0x0600393E RID: 14654 RVA: 0x0017E064 File Offset: 0x0017C264
		~AssemblyLoader()
		{
			base.Unload();
		}

		// Token: 0x0600393F RID: 14655 RVA: 0x0017E090 File Offset: 0x0017C290
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

		// Token: 0x06003940 RID: 14656 RVA: 0x0017E0EC File Offset: 0x0017C2EC
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

		// Token: 0x06003941 RID: 14657 RVA: 0x0017E164 File Offset: 0x0017C364
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

		// Token: 0x06003942 RID: 14658 RVA: 0x0017E1D4 File Offset: 0x0017C3D4
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

		// Token: 0x06003943 RID: 14659 RVA: 0x0017E290 File Offset: 0x0017C490
		IEnumerable<Assembly> IAssemblyLoaderService.get_Assemblies()
		{
			return base.Assemblies;
		}

		// Token: 0x06003944 RID: 14660 RVA: 0x0017E298 File Offset: 0x0017C498
		[CompilerGenerated]
		private Result<Assembly> <LoadAssemblyFromFile>g__GenerateExceptionReturn|35_0<T>(T exception, ref AssemblyLoader.<>c__DisplayClass35_0 A_2) where T : Exception
		{
			return Result.Fail<Assembly>(new ExceptionalError(exception).WithMetadata(MetadataType.ExceptionObject, this).WithMetadata(MetadataType.RootObject, A_2.assemblyFilePath).WithMetadata(MetadataType.ExceptionDetails, exception.Message).WithMetadata(MetadataType.StackTrace, exception.StackTrace));
		}

		// Token: 0x04001CC2 RID: 7362
		private int _isDisposed;

		// Token: 0x04001CC3 RID: 7363
		private int _operationsRunning;

		// Token: 0x04001CC4 RID: 7364
		private readonly Action<IAssemblyLoaderService> _onUnload;

		// Token: 0x04001CC5 RID: 7365
		private readonly Func<IAssemblyLoaderService, AssemblyName, Assembly> _onResolvingManaged;

		// Token: 0x04001CC6 RID: 7366
		private readonly Func<Assembly, string, IntPtr> _onResolvingUnmanagedDll;

		// Token: 0x04001CC7 RID: 7367
		private readonly ConcurrentDictionary<string, AssemblyDependencyResolver> _dependencyResolvers = new ConcurrentDictionary<string, AssemblyDependencyResolver>();

		// Token: 0x04001CC8 RID: 7368
		private readonly ConcurrentDictionary<AssemblyLoader.AssemblyOrStringKey, AssemblyLoader.AssemblyData> _loadedAssemblyData = new ConcurrentDictionary<AssemblyLoader.AssemblyOrStringKey, AssemblyLoader.AssemblyData>();

		// Token: 0x04001CC9 RID: 7369
		private readonly ThreadLocal<bool> _isResolving = new ThreadLocal<bool>(() => false);

		// Token: 0x04001CCA RID: 7370
		private readonly ThreadLocal<bool> _isResolvingNative = new ThreadLocal<bool>(() => false);

		// Token: 0x02000C7E RID: 3198
		public class Factory : IAssemblyLoaderService.IFactory, IService, IDisposable
		{
			// Token: 0x0600643C RID: 25660 RVA: 0x002143B8 File Offset: 0x002125B8
			public IAssemblyLoaderService CreateInstance(IAssemblyLoaderService.LoaderInitData initData)
			{
				return new AssemblyLoader(initData);
			}

			// Token: 0x0600643D RID: 25661 RVA: 0x002143C0 File Offset: 0x002125C0
			public void Dispose()
			{
			}

			// Token: 0x17001636 RID: 5686
			// (get) Token: 0x0600643E RID: 25662 RVA: 0x002143C2 File Offset: 0x002125C2
			public bool IsDisposed
			{
				get
				{
					return false;
				}
			}
		}

		// Token: 0x02000C7F RID: 3199
		private readonly struct AssemblyData : IEquatable<AssemblyLoader.AssemblyData>
		{
			// Token: 0x06006440 RID: 25664 RVA: 0x002143D0 File Offset: 0x002125D0
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

			// Token: 0x06006441 RID: 25665 RVA: 0x0021448C File Offset: 0x0021268C
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

			// Token: 0x06006442 RID: 25666 RVA: 0x00214548 File Offset: 0x00212748
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

			// Token: 0x06006443 RID: 25667 RVA: 0x00214594 File Offset: 0x00212794
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

			// Token: 0x06006444 RID: 25668 RVA: 0x00214635 File Offset: 0x00212835
			[CompilerGenerated]
			public static bool operator !=(AssemblyLoader.AssemblyData left, AssemblyLoader.AssemblyData right)
			{
				return !(left == right);
			}

			// Token: 0x06006445 RID: 25669 RVA: 0x00214641 File Offset: 0x00212841
			[CompilerGenerated]
			public static bool operator ==(AssemblyLoader.AssemblyData left, AssemblyLoader.AssemblyData right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006446 RID: 25670 RVA: 0x0021464C File Offset: 0x0021284C
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (((EqualityComparer<Assembly>.Default.GetHashCode(this.Assembly) * -1521134295 + EqualityComparer<OneOf<byte[], string>>.Default.GetHashCode(this.AssemblyImageOrPath)) * -1521134295 + EqualityComparer<MetadataReference>.Default.GetHashCode(this.AssemblyReference)) * -1521134295 + EqualityComparer<ImmutableArray<Type>>.Default.GetHashCode(this.Types)) * -1521134295 + EqualityComparer<ImmutableDictionary<string, Type>>.Default.GetHashCode(this.TypesByName);
			}

			// Token: 0x06006447 RID: 25671 RVA: 0x002146C5 File Offset: 0x002128C5
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is AssemblyLoader.AssemblyData && this.Equals((AssemblyLoader.AssemblyData)obj);
			}

			// Token: 0x06006448 RID: 25672 RVA: 0x002146E0 File Offset: 0x002128E0
			[CompilerGenerated]
			public bool Equals(AssemblyLoader.AssemblyData other)
			{
				return EqualityComparer<Assembly>.Default.Equals(this.Assembly, other.Assembly) && EqualityComparer<OneOf<byte[], string>>.Default.Equals(this.AssemblyImageOrPath, other.AssemblyImageOrPath) && EqualityComparer<MetadataReference>.Default.Equals(this.AssemblyReference, other.AssemblyReference) && EqualityComparer<ImmutableArray<Type>>.Default.Equals(this.Types, other.Types) && EqualityComparer<ImmutableDictionary<string, Type>>.Default.Equals(this.TypesByName, other.TypesByName);
			}

			// Token: 0x04003C8F RID: 15503
			public readonly Assembly Assembly;

			// Token: 0x04003C90 RID: 15504
			public readonly OneOf<byte[], string> AssemblyImageOrPath;

			// Token: 0x04003C91 RID: 15505
			public readonly MetadataReference AssemblyReference;

			// Token: 0x04003C92 RID: 15506
			public readonly ImmutableArray<Type> Types;

			// Token: 0x04003C93 RID: 15507
			public readonly ImmutableDictionary<string, Type> TypesByName;
		}

		// Token: 0x02000C80 RID: 3200
		private readonly struct AssemblyOrStringKey : IEquatable<AssemblyLoader.AssemblyOrStringKey>, IEqualityComparer<AssemblyLoader.AssemblyOrStringKey>
		{
			// Token: 0x17001637 RID: 5687
			// (get) Token: 0x06006449 RID: 25673 RVA: 0x00214765 File Offset: 0x00212965
			// (set) Token: 0x0600644A RID: 25674 RVA: 0x0021476D File Offset: 0x0021296D
			public Assembly Assembly { get; set; }

			// Token: 0x17001638 RID: 5688
			// (get) Token: 0x0600644B RID: 25675 RVA: 0x00214776 File Offset: 0x00212976
			// (set) Token: 0x0600644C RID: 25676 RVA: 0x0021477E File Offset: 0x0021297E
			public string AssemblyName { get; set; }

			// Token: 0x0600644D RID: 25677 RVA: 0x00214788 File Offset: 0x00212988
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

			// Token: 0x0600644E RID: 25678 RVA: 0x002147E5 File Offset: 0x002129E5
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

			// Token: 0x0600644F RID: 25679 RVA: 0x00214819 File Offset: 0x00212A19
			public bool Equals(AssemblyLoader.AssemblyOrStringKey x, AssemblyLoader.AssemblyOrStringKey y)
			{
				if (x.Assembly != null && y.Assembly != null)
				{
					return x.Assembly == y.Assembly;
				}
				return x.AssemblyName == y.AssemblyName;
			}

			// Token: 0x06006450 RID: 25680 RVA: 0x00214854 File Offset: 0x00212A54
			public int GetHashCode(AssemblyLoader.AssemblyOrStringKey obj)
			{
				return this.HashCode;
			}

			// Token: 0x06006451 RID: 25681 RVA: 0x0021485C File Offset: 0x00212A5C
			public static implicit operator AssemblyLoader.AssemblyOrStringKey(Assembly assembly)
			{
				return new AssemblyLoader.AssemblyOrStringKey(assembly);
			}

			// Token: 0x06006452 RID: 25682 RVA: 0x00214864 File Offset: 0x00212A64
			public static implicit operator AssemblyLoader.AssemblyOrStringKey(string name)
			{
				return new AssemblyLoader.AssemblyOrStringKey(name);
			}

			// Token: 0x06006453 RID: 25683 RVA: 0x0021486C File Offset: 0x00212A6C
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

			// Token: 0x06006454 RID: 25684 RVA: 0x002148B8 File Offset: 0x00212AB8
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

			// Token: 0x06006455 RID: 25685 RVA: 0x0021491C File Offset: 0x00212B1C
			[CompilerGenerated]
			public static bool operator !=(AssemblyLoader.AssemblyOrStringKey left, AssemblyLoader.AssemblyOrStringKey right)
			{
				return !(left == right);
			}

			// Token: 0x06006456 RID: 25686 RVA: 0x00214928 File Offset: 0x00212B28
			[CompilerGenerated]
			public static bool operator ==(AssemblyLoader.AssemblyOrStringKey left, AssemblyLoader.AssemblyOrStringKey right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006457 RID: 25687 RVA: 0x00214932 File Offset: 0x00212B32
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Assembly>.Default.GetHashCode(this.<Assembly>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<AssemblyName>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.HashCode);
			}

			// Token: 0x06006458 RID: 25688 RVA: 0x00214972 File Offset: 0x00212B72
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is AssemblyLoader.AssemblyOrStringKey && this.Equals((AssemblyLoader.AssemblyOrStringKey)obj);
			}

			// Token: 0x06006459 RID: 25689 RVA: 0x0021498C File Offset: 0x00212B8C
			[CompilerGenerated]
			public bool Equals(AssemblyLoader.AssemblyOrStringKey other)
			{
				return EqualityComparer<Assembly>.Default.Equals(this.<Assembly>k__BackingField, other.<Assembly>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<AssemblyName>k__BackingField, other.<AssemblyName>k__BackingField) && EqualityComparer<int>.Default.Equals(this.HashCode, other.HashCode);
			}

			// Token: 0x04003C96 RID: 15510
			public readonly int HashCode;
		}
	}
}
