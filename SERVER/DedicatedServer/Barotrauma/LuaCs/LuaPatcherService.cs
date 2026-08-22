using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Barotrauma.LuaCs.Compatibility;
using FluentResults;
using HarmonyLib;
using MoonSharp.Interpreter;
using Sigil;
using Sigil.NonGeneric;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000422 RID: 1058
	public class LuaPatcherService : ILuaPatcher, IReusableService, IService, IDisposable
	{
		// Token: 0x06003BDD RID: 15325 RVA: 0x0018A730 File Offset: 0x00188930
		private static void _hookLuaCsPatch(MethodBase __originalMethod, object[] __args, object __instance, out object result, ILuaCsHook.HookMethodType hookType)
		{
			result = null;
			try
			{
				long funcAddr = (long)__originalMethod.MethodHandle.GetFunctionPointer();
				HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>> methodSet = null;
				if (hookType != ILuaCsHook.HookMethodType.Before)
				{
					if (hookType != ILuaCsHook.HookMethodType.After)
					{
						throw new ArgumentException("Invalid HookMethodType enum value.", "hookType");
					}
					LuaPatcherService.instance.compatHookPostfixMethods.TryGetValue(funcAddr, out methodSet);
				}
				else
				{
					LuaPatcherService.instance.compatHookPrefixMethods.TryGetValue(funcAddr, out methodSet);
				}
				if (methodSet != null)
				{
					ParameterInfo[] @params = __originalMethod.GetParameters();
					Dictionary<string, object> args = new Dictionary<string, object>();
					for (int i = 0; i < @params.Length; i++)
					{
						args.Add(@params[i].Name, __args[i]);
					}
					foreach (ValueTuple<string, Barotrauma.LuaCsPatch> tuple in methodSet)
					{
						object _result = tuple.Item2(__instance, args);
						if (_result != null)
						{
							DynValue res = _result as DynValue;
							if (res != null)
							{
								if (!res.IsNil())
								{
									MethodInfo mi = __originalMethod as MethodInfo;
									if (mi != null && mi.ReturnType != typeof(void))
									{
										result = res.ToObject(mi.ReturnType);
									}
									else
									{
										result = res.ToObject();
									}
								}
							}
							else
							{
								result = _result;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				LuaCsLogger.LogError("Error in " + __originalMethod.Name + ":", LuaCsMessageOrigin.Unknown);
				LuaCsLogger.HandleException(ex, LuaCsMessageOrigin.Unknown);
			}
		}

		// Token: 0x06003BDE RID: 15326 RVA: 0x0018A8D0 File Offset: 0x00188AD0
		private static bool HookLuaCsPatchPrefix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			object result;
			LuaPatcherService._hookLuaCsPatch(__originalMethod, __args, __instance, out result, ILuaCsHook.HookMethodType.Before);
			return result == null;
		}

		// Token: 0x06003BDF RID: 15327 RVA: 0x0018A8EC File Offset: 0x00188AEC
		private static void HookLuaCsPatchPostfix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			object obj;
			LuaPatcherService._hookLuaCsPatch(__originalMethod, __args, __instance, out obj, ILuaCsHook.HookMethodType.After);
		}

		// Token: 0x06003BE0 RID: 15328 RVA: 0x0018A904 File Offset: 0x00188B04
		private static bool HookLuaCsPatchRetPrefix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			object result;
			LuaPatcherService._hookLuaCsPatch(__originalMethod, __args, __instance, out result, ILuaCsHook.HookMethodType.Before);
			if (result != null)
			{
				__result = result;
				return false;
			}
			return true;
		}

		// Token: 0x06003BE1 RID: 15329 RVA: 0x0018A928 File Offset: 0x00188B28
		private static void HookLuaCsPatchRetPostfix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			object result;
			LuaPatcherService._hookLuaCsPatch(__originalMethod, __args, __instance, out result, ILuaCsHook.HookMethodType.After);
			if (result != null)
			{
				__result = result;
			}
		}

		// Token: 0x06003BE2 RID: 15330 RVA: 0x0018A948 File Offset: 0x00188B48
		public void HookMethod(string identifier, MethodBase method, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before, IAssemblyPlugin owner = null)
		{
			if (identifier == null || method == null || patch2 == null)
			{
				LuaCsLogger.HandleException(new ArgumentNullException("Identifier, Method and Patch arguments must not be null."), LuaCsMessageOrigin.Unknown);
				return;
			}
			LuaPatcherService.ValidatePatchTarget(method);
			long funcAddr = (long)method.MethodHandle.GetFunctionPointer();
			Patches patches = Harmony.GetPatchInfo(method);
			if (hookType == ILuaCsHook.HookMethodType.Before)
			{
				MethodInfo mi = method as MethodInfo;
				if (mi != null && mi.ReturnType != typeof(void))
				{
					if (patches != null && patches.Prefixes != null)
					{
						if (patches.Prefixes.Find((Patch patch) => patch.PatchMethod == LuaPatcherService._miHookLuaCsPatchRetPrefix) != null)
						{
							goto IL_120;
						}
					}
					this.harmony.Patch(method, new HarmonyMethod(LuaPatcherService._miHookLuaCsPatchRetPrefix), null, null, null, null);
				}
				else
				{
					if (patches != null && patches.Prefixes != null)
					{
						if (patches.Prefixes.Find((Patch patch) => patch.PatchMethod == LuaPatcherService._miHookLuaCsPatchPrefix) != null)
						{
							goto IL_120;
						}
					}
					this.harmony.Patch(method, new HarmonyMethod(LuaPatcherService._miHookLuaCsPatchPrefix), null, null, null, null);
				}
				IL_120:
				HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>> methodSet;
				if (this.compatHookPrefixMethods.TryGetValue(funcAddr, out methodSet))
				{
					if (identifier != "")
					{
						methodSet.RemoveWhere((ValueTuple<string, Barotrauma.LuaCsPatch> tuple) => tuple.Item1 == identifier);
					}
					methodSet.Add(new ValueTuple<string, Barotrauma.LuaCsPatch>(identifier, patch2));
					return;
				}
				if (patch2 != null)
				{
					this.compatHookPrefixMethods.Add(funcAddr, new HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>
					{
						new ValueTuple<string, Barotrauma.LuaCsPatch>(identifier, patch2)
					});
					return;
				}
			}
			else if (hookType == ILuaCsHook.HookMethodType.After)
			{
				MethodInfo mi2 = method as MethodInfo;
				if (mi2 != null && mi2.ReturnType != typeof(void))
				{
					if (patches != null && patches.Postfixes != null)
					{
						if (patches.Postfixes.Find((Patch patch) => patch.PatchMethod == LuaPatcherService._miHookLuaCsPatchRetPostfix) != null)
						{
							goto IL_268;
						}
					}
					this.harmony.Patch(method, null, new HarmonyMethod(LuaPatcherService._miHookLuaCsPatchRetPostfix), null, null, null);
				}
				else
				{
					if (patches != null && patches.Postfixes != null)
					{
						if (patches.Postfixes.Find((Patch patch) => patch.PatchMethod == LuaPatcherService._miHookLuaCsPatchPostfix) != null)
						{
							goto IL_268;
						}
					}
					this.harmony.Patch(method, null, new HarmonyMethod(LuaPatcherService._miHookLuaCsPatchPostfix), null, null, null);
				}
				IL_268:
				HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>> methodSet2;
				if (this.compatHookPostfixMethods.TryGetValue(funcAddr, out methodSet2))
				{
					if (identifier != "")
					{
						methodSet2.RemoveWhere((ValueTuple<string, Barotrauma.LuaCsPatch> tuple) => tuple.Item1 == identifier);
					}
					methodSet2.Add(new ValueTuple<string, Barotrauma.LuaCsPatch>(identifier, patch2));
					return;
				}
				if (patch2 != null)
				{
					this.compatHookPostfixMethods.Add(funcAddr, new HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>
					{
						new ValueTuple<string, Barotrauma.LuaCsPatch>(identifier, patch2)
					});
				}
			}
		}

		// Token: 0x06003BE3 RID: 15331 RVA: 0x0018AC30 File Offset: 0x00188E30
		public void HookMethod(string identifier, string className, string methodName, string[] parameterNames, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterNames);
			if (method == null)
			{
				return;
			}
			if (method.GetParameters().Any((ParameterInfo x) => x.ParameterType.IsByRef))
			{
				throw new InvalidOperationException("HookMethod doesn't support ByRef parameters; use Patch instead.");
			}
			this.HookMethod(identifier, method, patch, hookMethodType, null);
		}

		// Token: 0x06003BE4 RID: 15332 RVA: 0x0018AC95 File Offset: 0x00188E95
		public void HookMethod(string identifier, string className, string methodName, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this.HookMethod(identifier, className, methodName, null, patch, hookMethodType);
		}

		// Token: 0x06003BE5 RID: 15333 RVA: 0x0018ACA5 File Offset: 0x00188EA5
		public void HookMethod(string className, string methodName, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this.HookMethod("", className, methodName, null, patch, hookMethodType);
		}

		// Token: 0x06003BE6 RID: 15334 RVA: 0x0018ACB8 File Offset: 0x00188EB8
		public void HookMethod(string className, string methodName, string[] parameterNames, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this.HookMethod("", className, methodName, parameterNames, patch, hookMethodType);
		}

		// Token: 0x06003BE7 RID: 15335 RVA: 0x0018ACCC File Offset: 0x00188ECC
		public void UnhookMethod(string identifier, MethodBase method, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			long funcAddr = (long)method.MethodHandle.GetFunctionPointer();
			Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>> methods;
			if (hookType == ILuaCsHook.HookMethodType.Before)
			{
				methods = this.compatHookPrefixMethods;
			}
			else
			{
				if (hookType != ILuaCsHook.HookMethodType.After)
				{
					throw null;
				}
				methods = this.compatHookPostfixMethods;
			}
			if (methods.ContainsKey(funcAddr))
			{
				HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>> hashSet = methods[funcAddr];
				if (hashSet == null)
				{
					return;
				}
				hashSet.RemoveWhere((ValueTuple<string, Barotrauma.LuaCsPatch> t) => t.Item1 == identifier);
			}
		}

		// Token: 0x06003BE8 RID: 15336 RVA: 0x0018AD38 File Offset: 0x00188F38
		protected void UnhookMethod(string identifier, string className, string methodName, string[] parameterNames, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterNames);
			if (method == null)
			{
				return;
			}
			this.UnhookMethod(identifier, method, hookType);
		}

		// Token: 0x06003BE9 RID: 15337 RVA: 0x0018AD64 File Offset: 0x00188F64
		public LuaPatcherService()
		{
			LuaPatcherService.instance = this;
			this.harmony = new Harmony("LuaCsForBarotrauma");
			this.patchModuleBuilder = new Lazy<ModuleBuilder>(new Func<ModuleBuilder>(this.CreateModuleBuilder));
			UserData.RegisterType<LuaPatcherService.ParameterTable>(InteropAccessMode.Default, null);
		}

		// Token: 0x06003BEA RID: 15338 RVA: 0x0018ADD0 File Offset: 0x00188FD0
		private static void ValidatePatchTarget(MethodBase method)
		{
			if (LuaPatcherService.prohibitedHooks.Any((string h) => method.DeclaringType.FullName.StartsWith(h)))
			{
				throw new ArgumentException("Hooks into the modding environment are prohibited.");
			}
		}

		// Token: 0x06003BEB RID: 15339 RVA: 0x0018AE0D File Offset: 0x0018900D
		private static string NormalizeIdentifier(string identifier)
		{
			if (identifier == null)
			{
				return null;
			}
			return identifier.Trim().ToLowerInvariant();
		}

		// Token: 0x06003BEC RID: 15340 RVA: 0x0018AE20 File Offset: 0x00189020
		private ModuleBuilder CreateModuleBuilder()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("LuaCsHookPatch-");
			defaultInterpolatedStringHandler.AppendFormatted<Guid>(Guid.NewGuid(), "N");
			string assemblyName = defaultInterpolatedStringHandler.ToStringAndClear();
			AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("LuaCsHookPatch");
			TypeBuilder typeBuilder = moduleBuilder.DefineType("System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute", TypeAttributes.Sealed, typeof(Attribute));
			CustomAttributeBuilder attributeUsageAttribute = new CustomAttributeBuilder(typeof(AttributeUsageAttribute).GetConstructor(new Type[]
			{
				typeof(AttributeTargets)
			}), new object[]
			{
				AttributeTargets.Assembly
			}, new PropertyInfo[]
			{
				typeof(AttributeUsageAttribute).GetProperty("AllowMultiple")
			}, new object[]
			{
				true
			});
			typeBuilder.SetCustomAttribute(attributeUsageAttribute);
			FieldBuilder attributeTypeFieldBuilder = typeBuilder.DefineField("assemblyName", typeof(string), FieldAttributes.Private | FieldAttributes.InitOnly);
			Emit ctor = Emit.BuildConstructor(new Type[]
			{
				typeof(string)
			}, typeBuilder, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard | CallingConventions.HasThis, false, true, false);
			ctor.LoadArgument(0);
			ctor.LoadArgument(1);
			ctor.StoreField(attributeTypeFieldBuilder, false, null);
			ctor.Return();
			ctor.CreateConstructor(OptimizationOptions.All);
			Emit attributeNameGetter = Emit.BuildMethod(typeof(string), new Type[0], typeBuilder, "get_AttributeName", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard | CallingConventions.HasThis, false, true, false);
			attributeNameGetter.LoadArgument(0);
			attributeNameGetter.LoadField(attributeTypeFieldBuilder, null, null);
			attributeNameGetter.Return();
			PropertyBuilder attributeName = typeBuilder.DefineProperty("AttributeName", PropertyAttributes.None, typeof(string), null);
			attributeName.SetGetMethod(attributeNameGetter.CreateMethod(OptimizationOptions.All));
			Type type = typeBuilder.CreateTypeInfo().AsType();
			string[] assembliesToExpose = new string[]
			{
				"Barotrauma",
				"DedicatedServer"
			};
			foreach (string name in assembliesToExpose)
			{
				ConstructorInfo constructor = type.GetConstructor(new Type[]
				{
					typeof(string)
				});
				object[] constructorArgs = new string[]
				{
					name
				};
				CustomAttributeBuilder attr = new CustomAttributeBuilder(constructor, constructorArgs);
				assemblyBuilder.SetCustomAttribute(attr);
			}
			return moduleBuilder;
		}

		// Token: 0x06003BED RID: 15341 RVA: 0x0018B070 File Offset: 0x00189270
		private static MethodBase ResolveMethod(string className, string methodName, string[] parameters)
		{
			LuaPatcherService.<>c__DisplayClass32_0 CS$<>8__locals1;
			CS$<>8__locals1.classType = LuaCsSetup.Instance.PluginManagementService.GetType(className, false, false, true);
			if (CS$<>8__locals1.classType == null)
			{
				throw new ScriptRuntimeException("invalid class name '" + className + "'");
			}
			MethodBase method = null;
			try
			{
				if (parameters != null)
				{
					Type[] parameterTypes = new Type[parameters.Length];
					for (int i = 0; i < parameters.Length; i++)
					{
						Type type = LuaCsSetup.Instance.PluginManagementService.GetType(parameters[i], false, false, true);
						if (type == null)
						{
							throw new ScriptRuntimeException("invalid parameter type '" + parameters[i] + "'");
						}
						parameterTypes[i] = type;
					}
					MethodBase methodBase;
					if (!(methodName == ".cctor"))
					{
						if (!(methodName == ".ctor"))
						{
							methodBase = CS$<>8__locals1.classType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
						}
						else
						{
							methodBase = (from x in CS$<>8__locals1.classType.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Except(new ConstructorInfo[]
							{
								CS$<>8__locals1.classType.TypeInitializer
							})
							where (from x in x.GetParameters()
							select x.ParameterType).SequenceEqual(parameterTypes)
							select x).SingleOrDefault<ConstructorInfo>();
						}
					}
					else
					{
						methodBase = CS$<>8__locals1.classType.TypeInitializer;
					}
					method = methodBase;
				}
				else
				{
					if (methodName == ".cctor")
					{
						throw new ScriptRuntimeException("type initializers can't have parameters");
					}
					MethodBase methodBase;
					if (!(methodName == ".ctor"))
					{
						methodBase = CS$<>8__locals1.classType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					}
					else
					{
						methodBase = LuaPatcherService.<ResolveMethod>g__GetCtor|32_0(ref CS$<>8__locals1);
					}
					method = methodBase;
				}
			}
			catch (AmbiguousMatchException)
			{
				throw new ScriptRuntimeException("ambiguous method signature");
			}
			if (method == null)
			{
				string parameterNamesStr = (parameters == null) ? "" : string.Join(", ", parameters);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 3);
				defaultInterpolatedStringHandler.AppendLiteral("method '");
				defaultInterpolatedStringHandler.AppendFormatted(methodName);
				defaultInterpolatedStringHandler.AppendLiteral("(");
				defaultInterpolatedStringHandler.AppendFormatted(parameterNamesStr);
				defaultInterpolatedStringHandler.AppendLiteral(")' not found in class '");
				defaultInterpolatedStringHandler.AppendFormatted(className);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				throw new ScriptRuntimeException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return method;
		}

		// Token: 0x17000FE0 RID: 4064
		// (get) Token: 0x06003BEE RID: 15342 RVA: 0x0018B2A0 File Offset: 0x001894A0
		// (set) Token: 0x06003BEF RID: 15343 RVA: 0x0018B2A8 File Offset: 0x001894A8
		public bool IsDisposed { get; private set; }

		// Token: 0x06003BF0 RID: 15344 RVA: 0x0018B2B4 File Offset: 0x001894B4
		private MethodInfo CreateDynamicHarmonyPatch(string identifier, MethodBase original, ILuaCsHook.HookMethodType hookType)
		{
			LuaPatcherService.<>c__DisplayClass40_0 CS$<>8__locals1 = new LuaPatcherService.<>c__DisplayClass40_0();
			CS$<>8__locals1.original = original;
			CS$<>8__locals1.parameters = new List<LuaPatcherService.DynamicParameterMapping>
			{
				new LuaPatcherService.DynamicParameterMapping("__originalMethod", null, typeof(MethodBase)),
				new LuaPatcherService.DynamicParameterMapping("__instance", null, typeof(object))
			};
			LuaPatcherService.<>c__DisplayClass40_0 CS$<>8__locals2 = CS$<>8__locals1;
			MethodInfo mi = CS$<>8__locals1.original as MethodInfo;
			CS$<>8__locals2.hasReturnType = (mi != null && mi.ReturnType != typeof(void));
			if (CS$<>8__locals1.hasReturnType)
			{
				CS$<>8__locals1.parameters.Add(new LuaPatcherService.DynamicParameterMapping("__result", null, typeof(object).MakeByRefType()));
			}
			foreach (ParameterInfo parameter in CS$<>8__locals1.original.GetParameters())
			{
				string paramName = parameter.Name;
				Type originalMethodParamType = parameter.ParameterType;
				Type harmonyPatchParamType = originalMethodParamType.IsByRef ? originalMethodParamType : originalMethodParamType.MakeByRefType();
				CS$<>8__locals1.parameters.Add(new LuaPatcherService.DynamicParameterMapping(paramName, originalMethodParamType, harmonyPatchParamType));
			}
			ModuleBuilder moduleBuilder = this.patchModuleBuilder.Value;
			string mangledName = (CS$<>8__locals1.original.DeclaringType != null) ? (LuaPatcherService.<CreateDynamicHarmonyPatch>g__MangleName|40_0(CS$<>8__locals1.original.DeclaringType) + "-" + LuaPatcherService.<CreateDynamicHarmonyPatch>g__MangleName|40_0(CS$<>8__locals1.original)) : LuaPatcherService.<CreateDynamicHarmonyPatch>g__MangleName|40_0(CS$<>8__locals1.original);
			ModuleBuilder moduleBuilder2 = moduleBuilder;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Patch_");
			defaultInterpolatedStringHandler.AppendFormatted(identifier);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<Guid>(Guid.NewGuid(), "N");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted(mangledName);
			TypeBuilder typeBuilder = moduleBuilder2.DefineType(defaultInterpolatedStringHandler.ToStringAndClear(), TypeAttributes.Public);
			FieldBuilder luaCsField = typeBuilder.DefineField("LuaCs", typeof(LuaCsSetup), FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static);
			string methodName = (hookType == ILuaCsHook.HookMethodType.Before) ? "HarmonyPrefix" : "HarmonyPostfix";
			Emit il2 = Emit.BuildMethod((hookType == ILuaCsHook.HookMethodType.Before) ? typeof(bool) : typeof(void), (from x in CS$<>8__locals1.parameters
			select x.HarmonyPatchParamType).ToArray<Type>(), typeBuilder, methodName, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static, CallingConventions.Standard, false, true, false);
			CS$<>8__locals1.labelReturn = il2.DefineLabel("endOfFunction");
			Sigil.ExceptionBlock exceptionBlock;
			il2.BeginExceptionBlock(out exceptionBlock);
			CS$<>8__locals1.harmonyReturnValue = il2.DeclareLocal<bool>("harmonyReturnValue");
			il2.LoadConstant(true);
			il2.StoreLocal(CS$<>8__locals1.harmonyReturnValue);
			Local patchKey = il2.DeclareLocal<LuaPatcherService.MethodKey>("patchKey");
			il2.LoadArgument(0);
			il2.CastClass<MethodBase>();
			il2.Call(typeof(LuaPatcherService.MethodKey).GetMethod("Create"), null);
			il2.StoreLocal(patchKey);
			Local patchExists = il2.DeclareLocal<bool>("patchExists");
			Local patches = il2.DeclareLocal<LuaPatcherService.PatchedMethod>("patches");
			il2.LoadField(typeof(LuaPatcherService).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic), null, null);
			il2.LoadField(typeof(LuaPatcherService).GetField("registeredPatches", BindingFlags.Instance | BindingFlags.NonPublic), null, null);
			il2.LoadLocal(patchKey);
			il2.LoadLocalAddress(patches);
			il2.Call(typeof(Dictionary<LuaPatcherService.MethodKey, LuaPatcherService.PatchedMethod>).GetMethod("TryGetValue"), null);
			il2.StoreLocal(patchExists);
			il2.LoadLocal(patchExists);
			il2.IfNot(delegate(Emit il)
			{
				il.Leave(CS$<>8__locals1.labelReturn);
			});
			Local parameterDict = il2.DeclareLocal<Dictionary<string, object>>("parameterDict");
			il2.LoadConstant(CS$<>8__locals1.parameters.Count((LuaPatcherService.DynamicParameterMapping x) => x.OriginalMethodParamType != null));
			il2.NewObject(typeof(Dictionary<string, object>), new Type[]
			{
				typeof(int)
			});
			il2.StoreLocal(parameterDict);
			ushort l = 0;
			while ((int)l < CS$<>8__locals1.parameters.Count)
			{
				if (!(CS$<>8__locals1.parameters[(int)l].OriginalMethodParamType == null))
				{
					il2.LoadLocal(parameterDict);
					il2.LoadConstant(CS$<>8__locals1.parameters[(int)l].ParameterName);
					il2.LoadArgument(l);
					il2.ToObject(CS$<>8__locals1.parameters[(int)l].HarmonyPatchParamType);
					il2.Call(typeof(Dictionary<string, object>).GetMethod("Add"), null);
				}
				l += 1;
			}
			CS$<>8__locals1.ptable = il2.DeclareLocal<LuaPatcherService.ParameterTable>("ptable");
			il2.LoadLocal(parameterDict);
			il2.NewObject(typeof(LuaPatcherService.ParameterTable), new Type[]
			{
				typeof(Dictionary<string, object>)
			});
			il2.StoreLocal(CS$<>8__locals1.ptable);
			if (CS$<>8__locals1.hasReturnType && hookType == ILuaCsHook.HookMethodType.After)
			{
				il2.LoadLocal(CS$<>8__locals1.ptable);
				il2.LoadArgument(2);
				il2.ToObject(CS$<>8__locals1.parameters[2].HarmonyPatchParamType);
				il2.Call(typeof(LuaPatcherService.ParameterTable).GetProperty("OriginalReturnValue").GetSetMethod(true), null);
			}
			Local enumerator = il2.DeclareLocal<IEnumerator<LuaPatcherService.LuaCsPatch>>("enumerator");
			il2.LoadLocal(patches);
			il2.CallVirtual(typeof(LuaPatcherService.PatchedMethod).GetMethod((hookType == ILuaCsHook.HookMethodType.Before) ? "GetPrefixEnumerator" : "GetPostfixEnumerator", BindingFlags.Instance | BindingFlags.Public), null, null);
			il2.StoreLocal(enumerator);
			Label labelUpdateParameters = il2.DefineLabel("updateParameters");
			il2.ForEachEnumerator(enumerator, delegate(Emit il, Local current, Label labelLeave)
			{
				Local luaReturnValue = il.DeclareLocal<DynValue>("luaReturnValue");
				il.LoadLocal(current);
				il.Call(typeof(LuaPatcherService.LuaCsPatch).GetProperty("PatchFunc").GetGetMethod(), null);
				il.LoadArgument(1);
				il.LoadLocal(CS$<>8__locals1.ptable);
				il.CallVirtual(typeof(LuaCsPatchFunc).GetMethod("Invoke"), null, null);
				il.StoreLocal(luaReturnValue);
				if (CS$<>8__locals1.hasReturnType)
				{
					Local ptableReturnValue = il.DeclareLocal<object>("ptableReturnValue");
					il.LoadLocal(CS$<>8__locals1.ptable);
					il.Call(typeof(LuaPatcherService.ParameterTable).GetProperty("ReturnValue").GetGetMethod(), null);
					il.StoreLocal(ptableReturnValue);
					il.LoadLocal(ptableReturnValue);
					il.If(delegate(Emit il)
					{
						il.LoadArgument(2);
						il.LoadLocal(ptableReturnValue);
						il.StoreIndirect(typeof(object), false, null);
						il.Break();
					});
					il.LoadLocal(luaReturnValue);
					Action<Emit> <>9__9;
					il.If(delegate(Emit il)
					{
						il.LoadLocal(luaReturnValue);
						il.Call(typeof(DynValue).GetMethod("IsVoid"), null);
						Action<Emit> action;
						if ((action = <>9__9) == null)
						{
							action = (<>9__9 = delegate(Emit il)
							{
								Local csReturnType = il.DeclareLocal<Type>("csReturnType");
								il.LoadType(((MethodInfo)CS$<>8__locals1.original).ReturnType);
								il.StoreLocal(csReturnType);
								Local csReturnValue = il.DeclareLocal<object>("csReturnValue");
								il.LoadLocal(luaReturnValue);
								il.LoadLocal(csReturnType);
								il.Call(typeof(DynValue).GetMethod("ToObject", BindingFlags.Instance | BindingFlags.Public, null, new Type[]
								{
									typeof(Type)
								}, null), null);
								il.StoreLocal(csReturnValue);
								il.LoadArgument(2);
								il.LoadLocal(csReturnValue);
								il.StoreIndirect(typeof(object), false, null);
							});
						}
						il.IfNot(action);
					});
				}
				il.LoadLocal(CS$<>8__locals1.ptable);
				il.Call(typeof(LuaPatcherService.ParameterTable).GetProperty("PreventExecution").GetGetMethod(), null);
				il.If(delegate(Emit il)
				{
					il.LoadConstant(false);
					il.StoreLocal(CS$<>8__locals1.harmonyReturnValue);
					il.Leave(labelLeave);
				});
			});
			Local modifiedParameters = il2.DeclareLocal<Dictionary<string, object>>("modifiedParameters");
			il2.LoadLocal(CS$<>8__locals1.ptable);
			il2.Call(typeof(LuaPatcherService.ParameterTable).GetProperty("ModifiedParameters").GetGetMethod(), null);
			il2.StoreLocal(modifiedParameters);
			CS$<>8__locals1.modifiedValue = il2.DeclareLocal<object>("modifiedValue");
			ushort i = 0;
			while ((int)i < CS$<>8__locals1.parameters.Count)
			{
				if (!(CS$<>8__locals1.parameters[(int)i].OriginalMethodParamType == null))
				{
					il2.LoadLocal(modifiedParameters);
					il2.LoadConstant(CS$<>8__locals1.parameters[(int)i].ParameterName);
					il2.LoadLocalAddress(CS$<>8__locals1.modifiedValue);
					il2.Call(typeof(Dictionary<string, object>).GetMethod("TryGetValue"), null);
					il2.If(delegate(Emit il)
					{
						Type paramType = CS$<>8__locals1.parameters[(int)i].HarmonyPatchParamType.GetElementType();
						il.LoadArgument(i);
						il.LoadLocalAndCast(CS$<>8__locals1.modifiedValue, paramType);
						if (paramType.IsValueType)
						{
							il.StoreObject(paramType, false, null);
							return;
						}
						il.StoreIndirect(paramType, false, null);
					});
				}
				ushort i2 = i;
				i = i2 + 1;
			}
			il2.MarkLabel(CS$<>8__locals1.labelReturn);
			CatchBlock catchBlock;
			il2.BeginCatchAllBlock(exceptionBlock, out catchBlock);
			CS$<>8__locals1.exception = il2.DeclareLocal<Exception>("exception");
			il2.StoreLocal(CS$<>8__locals1.exception);
			il2.LoadField(luaCsField, null, null);
			il2.If(delegate(Emit il)
			{
				il.LoadLocal(CS$<>8__locals1.exception);
				il.LoadConstant(2);
				il.Call(typeof(LuaCsLogger).GetMethod("HandleException", BindingFlags.Static | BindingFlags.Public), null);
			});
			il2.EndCatchBlock(catchBlock);
			il2.EndExceptionBlock(exceptionBlock);
			if (hookType == ILuaCsHook.HookMethodType.Before)
			{
				il2.LoadLocal(CS$<>8__locals1.harmonyReturnValue);
			}
			il2.Return();
			MethodBuilder method = il2.CreateMethod(OptimizationOptions.All);
			for (int j = 0; j < CS$<>8__locals1.parameters.Count; j++)
			{
				method.DefineParameter(j + 1, ParameterAttributes.None, CS$<>8__locals1.parameters[j].ParameterName);
			}
			Type type = typeBuilder.CreateType();
			type.GetField("LuaCs", BindingFlags.Static | BindingFlags.Public).SetValue(null, LuaCsSetup.Instance);
			return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
		}

		// Token: 0x06003BF1 RID: 15345 RVA: 0x0018BAE0 File Offset: 0x00189CE0
		private string Patch(string identifier, MethodBase method, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (patch == null)
			{
				throw new ArgumentNullException("patch");
			}
			LuaPatcherService.ValidatePatchTarget(method);
			if (identifier == null)
			{
				identifier = Guid.NewGuid().ToString("N");
			}
			identifier = LuaPatcherService.NormalizeIdentifier(identifier);
			LuaPatcherService.MethodKey patchKey = LuaPatcherService.MethodKey.Create(method);
			LuaPatcherService.PatchedMethod methodPatches;
			if (!this.registeredPatches.TryGetValue(patchKey, out methodPatches))
			{
				MethodInfo harmonyPrefix = this.CreateDynamicHarmonyPatch(identifier, method, ILuaCsHook.HookMethodType.Before);
				MethodInfo harmonyPostfix = this.CreateDynamicHarmonyPatch(identifier, method, ILuaCsHook.HookMethodType.After);
				this.harmony.Patch(method, new HarmonyMethod(harmonyPrefix), new HarmonyMethod(harmonyPostfix), null, null, null);
				methodPatches = (this.registeredPatches[patchKey] = new LuaPatcherService.PatchedMethod(harmonyPrefix, harmonyPostfix));
			}
			if (hookType == ILuaCsHook.HookMethodType.Before)
			{
				if (methodPatches.Prefixes.Remove(identifier))
				{
					LuaCsLogger.LogMessage("Replacing existing prefix: " + identifier, null, null);
				}
				methodPatches.Prefixes.Add(identifier, new LuaPatcherService.LuaCsPatch
				{
					Identifier = identifier,
					PatchFunc = patch
				});
			}
			else if (hookType == ILuaCsHook.HookMethodType.After)
			{
				if (methodPatches.Postfixes.Remove(identifier))
				{
					LuaCsLogger.LogMessage("Replacing existing postfix: " + identifier, null, null);
				}
				methodPatches.Postfixes.Add(identifier, new LuaPatcherService.LuaCsPatch
				{
					Identifier = identifier,
					PatchFunc = patch
				});
			}
			return identifier;
		}

		// Token: 0x06003BF2 RID: 15346 RVA: 0x0018BC48 File Offset: 0x00189E48
		public string Patch(string identifier, string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterTypes);
			return this.Patch(identifier, method, patch, hookType);
		}

		// Token: 0x06003BF3 RID: 15347 RVA: 0x0018BC6C File Offset: 0x00189E6C
		public string Patch(string identifier, string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, null);
			return this.Patch(identifier, method, patch, hookType);
		}

		// Token: 0x06003BF4 RID: 15348 RVA: 0x0018BC90 File Offset: 0x00189E90
		public string Patch(string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterTypes);
			return this.Patch(null, method, patch, hookType);
		}

		// Token: 0x06003BF5 RID: 15349 RVA: 0x0018BCB4 File Offset: 0x00189EB4
		public string Patch(string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, null);
			return this.Patch(null, method, patch, hookType);
		}

		// Token: 0x06003BF6 RID: 15350 RVA: 0x0018BCD8 File Offset: 0x00189ED8
		private bool RemovePatch(string identifier, MethodBase method, ILuaCsHook.HookMethodType hookType)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			identifier = LuaPatcherService.NormalizeIdentifier(identifier);
			LuaPatcherService.MethodKey patchKey = LuaPatcherService.MethodKey.Create(method);
			LuaPatcherService.PatchedMethod methodPatches;
			if (!this.registeredPatches.TryGetValue(patchKey, out methodPatches))
			{
				return false;
			}
			bool result;
			if (hookType != ILuaCsHook.HookMethodType.Before)
			{
				if (hookType != ILuaCsHook.HookMethodType.After)
				{
					throw new ArgumentException("Invalid HookMethodType enum value.", "hookType");
				}
				result = methodPatches.Postfixes.Remove(identifier);
			}
			else
			{
				result = methodPatches.Prefixes.Remove(identifier);
			}
			return result;
		}

		// Token: 0x06003BF7 RID: 15351 RVA: 0x0018BD4C File Offset: 0x00189F4C
		public bool RemovePatch(string identifier, string className, string methodName, string[] parameterTypes, ILuaCsHook.HookMethodType hookType)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterTypes);
			return this.RemovePatch(identifier, method, hookType);
		}

		// Token: 0x06003BF8 RID: 15352 RVA: 0x0018BD70 File Offset: 0x00189F70
		public bool RemovePatch(string identifier, string className, string methodName, ILuaCsHook.HookMethodType hookType)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, null);
			return this.RemovePatch(identifier, method, hookType);
		}

		// Token: 0x06003BF9 RID: 15353 RVA: 0x0018BD90 File Offset: 0x00189F90
		private void ClearAll()
		{
			Harmony harmony = this.harmony;
			if (harmony != null)
			{
				harmony.UnpatchSelf();
			}
			foreach (KeyValuePair<LuaPatcherService.MethodKey, LuaPatcherService.PatchedMethod> keyValuePair in this.registeredPatches)
			{
				LuaPatcherService.MethodKey methodKey;
				LuaPatcherService.PatchedMethod patchedMethod;
				keyValuePair.Deconstruct(out methodKey, out patchedMethod);
				LuaPatcherService.PatchedMethod patch = patchedMethod;
				patch.HarmonyPrefixMethod.DeclaringType.GetField("LuaCs", BindingFlags.Static | BindingFlags.Public).SetValue(null, null);
				patch.HarmonyPostfixMethod.DeclaringType.GetField("LuaCs", BindingFlags.Static | BindingFlags.Public).SetValue(null, null);
			}
			this.registeredPatches.Clear();
			this.compatHookPrefixMethods.Clear();
			this.compatHookPostfixMethods.Clear();
		}

		// Token: 0x06003BFA RID: 15354 RVA: 0x0018BE58 File Offset: 0x0018A058
		public void Dispose()
		{
			this.IsDisposed = true;
			this.ClearAll();
		}

		// Token: 0x06003BFB RID: 15355 RVA: 0x0018BE67 File Offset: 0x0018A067
		public Result Reset()
		{
			this.ClearAll();
			return Result.Ok();
		}

		// Token: 0x06003BFD RID: 15357 RVA: 0x0018BF20 File Offset: 0x0018A120
		[CompilerGenerated]
		internal static ConstructorInfo <ResolveMethod>g__GetCtor|32_0(ref LuaPatcherService.<>c__DisplayClass32_0 A_0)
		{
			IEnumerator<ConstructorInfo> ctors = A_0.classType.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Except(new ConstructorInfo[]
			{
				A_0.classType.TypeInitializer
			}).GetEnumerator();
			if (!ctors.MoveNext())
			{
				return null;
			}
			ConstructorInfo ctor = ctors.Current;
			if (ctors.MoveNext())
			{
				throw new AmbiguousMatchException();
			}
			return ctor;
		}

		// Token: 0x06003BFE RID: 15358 RVA: 0x0018BF79 File Offset: 0x0018A179
		[CompilerGenerated]
		internal static string <CreateDynamicHarmonyPatch>g__MangleName|40_0(object o)
		{
			return LuaPatcherService.InvalidIdentifierCharsRegex.Replace((o != null) ? o.ToString() : null, "_");
		}

		// Token: 0x04001D73 RID: 7539
		private static LuaPatcherService instance;

		// Token: 0x04001D74 RID: 7540
		private Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>> compatHookPrefixMethods = new Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>>();

		// Token: 0x04001D75 RID: 7541
		private Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>> compatHookPostfixMethods = new Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>>();

		// Token: 0x04001D76 RID: 7542
		private static MethodInfo _miHookLuaCsPatchPrefix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchPrefix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04001D77 RID: 7543
		private static MethodInfo _miHookLuaCsPatchPostfix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchPostfix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04001D78 RID: 7544
		private static MethodInfo _miHookLuaCsPatchRetPrefix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchRetPrefix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04001D79 RID: 7545
		private static MethodInfo _miHookLuaCsPatchRetPostfix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchRetPostfix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04001D7A RID: 7546
		private static readonly string[] prohibitedHooks = new string[]
		{
			"Barotrauma.Lua",
			"Barotrauma.Cs",
			"Barotrauma.ContentPackageManager"
		};

		// Token: 0x04001D7B RID: 7547
		private Harmony harmony;

		// Token: 0x04001D7C RID: 7548
		private Lazy<ModuleBuilder> patchModuleBuilder;

		// Token: 0x04001D7D RID: 7549
		private readonly Dictionary<LuaPatcherService.MethodKey, LuaPatcherService.PatchedMethod> registeredPatches = new Dictionary<LuaPatcherService.MethodKey, LuaPatcherService.PatchedMethod>();

		// Token: 0x04001D7E RID: 7550
		private static readonly Regex InvalidIdentifierCharsRegex = new Regex("[^\\w\\d]", RegexOptions.Compiled);

		// Token: 0x04001D7F RID: 7551
		private const string FIELD_LUACS = "LuaCs";

		// Token: 0x02000D28 RID: 3368
		private class LuaCsHookCallback
		{
			// Token: 0x0600666E RID: 26222 RVA: 0x0021E2F1 File Offset: 0x0021C4F1
			public LuaCsHookCallback(string name, string hookName, LuaCsFunc func)
			{
				this.name = name;
				this.hookName = hookName;
				this.func = func;
			}

			// Token: 0x04003F74 RID: 16244
			public string name;

			// Token: 0x04003F75 RID: 16245
			public string hookName;

			// Token: 0x04003F76 RID: 16246
			public LuaCsFunc func;
		}

		// Token: 0x02000D29 RID: 3369
		private class LuaCsPatch
		{
			// Token: 0x1700164A RID: 5706
			// (get) Token: 0x0600666F RID: 26223 RVA: 0x0021E30E File Offset: 0x0021C50E
			// (set) Token: 0x06006670 RID: 26224 RVA: 0x0021E316 File Offset: 0x0021C516
			public string Identifier { get; set; }

			// Token: 0x1700164B RID: 5707
			// (get) Token: 0x06006671 RID: 26225 RVA: 0x0021E31F File Offset: 0x0021C51F
			// (set) Token: 0x06006672 RID: 26226 RVA: 0x0021E327 File Offset: 0x0021C527
			public LuaCsPatchFunc PatchFunc { get; set; }
		}

		// Token: 0x02000D2A RID: 3370
		private class PatchedMethod
		{
			// Token: 0x06006674 RID: 26228 RVA: 0x0021E338 File Offset: 0x0021C538
			public PatchedMethod(MethodInfo harmonyPrefix, MethodInfo harmonyPostfix)
			{
				this.HarmonyPrefixMethod = harmonyPrefix;
				this.HarmonyPostfixMethod = harmonyPostfix;
				this.Prefixes = new Dictionary<string, LuaPatcherService.LuaCsPatch>();
				this.Postfixes = new Dictionary<string, LuaPatcherService.LuaCsPatch>();
			}

			// Token: 0x1700164C RID: 5708
			// (get) Token: 0x06006675 RID: 26229 RVA: 0x0021E364 File Offset: 0x0021C564
			public MethodInfo HarmonyPrefixMethod { get; }

			// Token: 0x1700164D RID: 5709
			// (get) Token: 0x06006676 RID: 26230 RVA: 0x0021E36C File Offset: 0x0021C56C
			public MethodInfo HarmonyPostfixMethod { get; }

			// Token: 0x06006677 RID: 26231 RVA: 0x0021E374 File Offset: 0x0021C574
			public IEnumerator<LuaPatcherService.LuaCsPatch> GetPrefixEnumerator()
			{
				return this.Prefixes.Values.GetEnumerator();
			}

			// Token: 0x06006678 RID: 26232 RVA: 0x0021E38B File Offset: 0x0021C58B
			public IEnumerator<LuaPatcherService.LuaCsPatch> GetPostfixEnumerator()
			{
				return this.Postfixes.Values.GetEnumerator();
			}

			// Token: 0x1700164E RID: 5710
			// (get) Token: 0x06006679 RID: 26233 RVA: 0x0021E3A2 File Offset: 0x0021C5A2
			public Dictionary<string, LuaPatcherService.LuaCsPatch> Prefixes { get; }

			// Token: 0x1700164F RID: 5711
			// (get) Token: 0x0600667A RID: 26234 RVA: 0x0021E3AA File Offset: 0x0021C5AA
			public Dictionary<string, LuaPatcherService.LuaCsPatch> Postfixes { get; }
		}

		// Token: 0x02000D2B RID: 3371
		public class ParameterTable
		{
			// Token: 0x0600667B RID: 26235 RVA: 0x0021E3B2 File Offset: 0x0021C5B2
			public ParameterTable(Dictionary<string, object> dict)
			{
				this.parameters = dict;
			}

			// Token: 0x17001650 RID: 5712
			public object this[string paramName]
			{
				get
				{
					object value;
					if (this.ModifiedParameters.TryGetValue(paramName, out value))
					{
						return value;
					}
					return this.OriginalParameters[paramName];
				}
				set
				{
					this.ModifiedParameters[paramName] = value;
				}
			}

			// Token: 0x17001651 RID: 5713
			// (get) Token: 0x0600667E RID: 26238 RVA: 0x0021E406 File Offset: 0x0021C606
			// (set) Token: 0x0600667F RID: 26239 RVA: 0x0021E40E File Offset: 0x0021C60E
			public object OriginalReturnValue { get; private set; }

			// Token: 0x17001652 RID: 5714
			// (get) Token: 0x06006680 RID: 26240 RVA: 0x0021E417 File Offset: 0x0021C617
			// (set) Token: 0x06006681 RID: 26241 RVA: 0x0021E42E File Offset: 0x0021C62E
			public object ReturnValue
			{
				get
				{
					if (this.returnValueModified)
					{
						return this.returnValue;
					}
					return this.OriginalReturnValue;
				}
				set
				{
					this.returnValueModified = true;
					this.returnValue = value;
				}
			}

			// Token: 0x17001653 RID: 5715
			// (get) Token: 0x06006682 RID: 26242 RVA: 0x0021E43E File Offset: 0x0021C63E
			// (set) Token: 0x06006683 RID: 26243 RVA: 0x0021E446 File Offset: 0x0021C646
			public bool PreventExecution { get; set; }

			// Token: 0x17001654 RID: 5716
			// (get) Token: 0x06006684 RID: 26244 RVA: 0x0021E44F File Offset: 0x0021C64F
			public Dictionary<string, object> OriginalParameters
			{
				get
				{
					return this.parameters;
				}
			}

			// Token: 0x17001655 RID: 5717
			// (get) Token: 0x06006685 RID: 26245 RVA: 0x0021E457 File Offset: 0x0021C657
			[MoonSharpHidden]
			public Dictionary<string, object> ModifiedParameters { get; } = new Dictionary<string, object>();

			// Token: 0x04003F7D RID: 16253
			private readonly Dictionary<string, object> parameters;

			// Token: 0x04003F7E RID: 16254
			private bool returnValueModified;

			// Token: 0x04003F7F RID: 16255
			private object returnValue;
		}

		// Token: 0x02000D2C RID: 3372
		private struct MethodKey : IEquatable<LuaPatcherService.MethodKey>
		{
			// Token: 0x17001656 RID: 5718
			// (get) Token: 0x06006686 RID: 26246 RVA: 0x0021E45F File Offset: 0x0021C65F
			// (set) Token: 0x06006687 RID: 26247 RVA: 0x0021E467 File Offset: 0x0021C667
			public ModuleHandle ModuleHandle { readonly get; set; }

			// Token: 0x17001657 RID: 5719
			// (get) Token: 0x06006688 RID: 26248 RVA: 0x0021E470 File Offset: 0x0021C670
			// (set) Token: 0x06006689 RID: 26249 RVA: 0x0021E478 File Offset: 0x0021C678
			public int MetadataToken { readonly get; set; }

			// Token: 0x0600668A RID: 26250 RVA: 0x0021E484 File Offset: 0x0021C684
			public override bool Equals(object obj)
			{
				if (obj is LuaPatcherService.MethodKey)
				{
					LuaPatcherService.MethodKey key = (LuaPatcherService.MethodKey)obj;
					return this.Equals(key);
				}
				return false;
			}

			// Token: 0x0600668B RID: 26251 RVA: 0x0021E4AC File Offset: 0x0021C6AC
			public bool Equals(LuaPatcherService.MethodKey other)
			{
				return this.ModuleHandle.Equals(other.ModuleHandle) && this.MetadataToken == other.MetadataToken;
			}

			// Token: 0x0600668C RID: 26252 RVA: 0x0021E4E1 File Offset: 0x0021C6E1
			public override int GetHashCode()
			{
				return HashCode.Combine<ModuleHandle, int>(this.ModuleHandle, this.MetadataToken);
			}

			// Token: 0x0600668D RID: 26253 RVA: 0x0021E4F4 File Offset: 0x0021C6F4
			public static bool operator ==(LuaPatcherService.MethodKey left, LuaPatcherService.MethodKey right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600668E RID: 26254 RVA: 0x0021E4FE File Offset: 0x0021C6FE
			public static bool operator !=(LuaPatcherService.MethodKey left, LuaPatcherService.MethodKey right)
			{
				return !(left == right);
			}

			// Token: 0x0600668F RID: 26255 RVA: 0x0021E50C File Offset: 0x0021C70C
			public static LuaPatcherService.MethodKey Create(MethodBase method)
			{
				return new LuaPatcherService.MethodKey
				{
					ModuleHandle = method.Module.ModuleHandle,
					MetadataToken = method.MetadataToken
				};
			}
		}

		// Token: 0x02000D2D RID: 3373
		private class DynamicParameterMapping
		{
			// Token: 0x06006690 RID: 26256 RVA: 0x0021E541 File Offset: 0x0021C741
			public DynamicParameterMapping(string name, Type originalMethodParamType, Type harmonyPatchParamType)
			{
				this.ParameterName = name;
				this.OriginalMethodParamType = originalMethodParamType;
				this.HarmonyPatchParamType = harmonyPatchParamType;
			}

			// Token: 0x17001658 RID: 5720
			// (get) Token: 0x06006691 RID: 26257 RVA: 0x0021E55E File Offset: 0x0021C75E
			// (set) Token: 0x06006692 RID: 26258 RVA: 0x0021E566 File Offset: 0x0021C766
			public string ParameterName { get; set; }

			// Token: 0x17001659 RID: 5721
			// (get) Token: 0x06006693 RID: 26259 RVA: 0x0021E56F File Offset: 0x0021C76F
			// (set) Token: 0x06006694 RID: 26260 RVA: 0x0021E577 File Offset: 0x0021C777
			public Type OriginalMethodParamType { get; set; }

			// Token: 0x1700165A RID: 5722
			// (get) Token: 0x06006695 RID: 26261 RVA: 0x0021E580 File Offset: 0x0021C780
			// (set) Token: 0x06006696 RID: 26262 RVA: 0x0021E588 File Offset: 0x0021C788
			public Type HarmonyPatchParamType { get; set; }
		}
	}
}
