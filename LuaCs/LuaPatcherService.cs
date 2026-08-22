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
	// Token: 0x02000535 RID: 1333
	public class LuaPatcherService : ILuaPatcher, IReusableService, IService, IDisposable
	{
		// Token: 0x060054F8 RID: 21752 RVA: 0x002CEEF0 File Offset: 0x002CD0F0
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

		// Token: 0x060054F9 RID: 21753 RVA: 0x002CF090 File Offset: 0x002CD290
		private static bool HookLuaCsPatchPrefix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			object result;
			LuaPatcherService._hookLuaCsPatch(__originalMethod, __args, __instance, out result, ILuaCsHook.HookMethodType.Before);
			return result == null;
		}

		// Token: 0x060054FA RID: 21754 RVA: 0x002CF0AC File Offset: 0x002CD2AC
		private static void HookLuaCsPatchPostfix(MethodBase __originalMethod, object[] __args, object __instance)
		{
			object obj;
			LuaPatcherService._hookLuaCsPatch(__originalMethod, __args, __instance, out obj, ILuaCsHook.HookMethodType.After);
		}

		// Token: 0x060054FB RID: 21755 RVA: 0x002CF0C4 File Offset: 0x002CD2C4
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

		// Token: 0x060054FC RID: 21756 RVA: 0x002CF0E8 File Offset: 0x002CD2E8
		private static void HookLuaCsPatchRetPostfix(MethodBase __originalMethod, object[] __args, ref object __result, object __instance)
		{
			object result;
			LuaPatcherService._hookLuaCsPatch(__originalMethod, __args, __instance, out result, ILuaCsHook.HookMethodType.After);
			if (result != null)
			{
				__result = result;
			}
		}

		// Token: 0x060054FD RID: 21757 RVA: 0x002CF108 File Offset: 0x002CD308
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

		// Token: 0x060054FE RID: 21758 RVA: 0x002CF3F0 File Offset: 0x002CD5F0
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

		// Token: 0x060054FF RID: 21759 RVA: 0x002CF455 File Offset: 0x002CD655
		public void HookMethod(string identifier, string className, string methodName, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this.HookMethod(identifier, className, methodName, null, patch, hookMethodType);
		}

		// Token: 0x06005500 RID: 21760 RVA: 0x002CF465 File Offset: 0x002CD665
		public void HookMethod(string className, string methodName, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this.HookMethod("", className, methodName, null, patch, hookMethodType);
		}

		// Token: 0x06005501 RID: 21761 RVA: 0x002CF478 File Offset: 0x002CD678
		public void HookMethod(string className, string methodName, string[] parameterNames, Barotrauma.LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this.HookMethod("", className, methodName, parameterNames, patch, hookMethodType);
		}

		// Token: 0x06005502 RID: 21762 RVA: 0x002CF48C File Offset: 0x002CD68C
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

		// Token: 0x06005503 RID: 21763 RVA: 0x002CF4F8 File Offset: 0x002CD6F8
		protected void UnhookMethod(string identifier, string className, string methodName, string[] parameterNames, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterNames);
			if (method == null)
			{
				return;
			}
			this.UnhookMethod(identifier, method, hookType);
		}

		// Token: 0x06005504 RID: 21764 RVA: 0x002CF524 File Offset: 0x002CD724
		public LuaPatcherService()
		{
			LuaPatcherService.instance = this;
			this.harmony = new Harmony("LuaCsForBarotrauma");
			this.patchModuleBuilder = new Lazy<ModuleBuilder>(new Func<ModuleBuilder>(this.CreateModuleBuilder));
			UserData.RegisterType<LuaPatcherService.ParameterTable>(InteropAccessMode.Default, null);
		}

		// Token: 0x06005505 RID: 21765 RVA: 0x002CF590 File Offset: 0x002CD790
		private static void ValidatePatchTarget(MethodBase method)
		{
			if (LuaPatcherService.prohibitedHooks.Any((string h) => method.DeclaringType.FullName.StartsWith(h)))
			{
				throw new ArgumentException("Hooks into the modding environment are prohibited.");
			}
		}

		// Token: 0x06005506 RID: 21766 RVA: 0x002CF5CD File Offset: 0x002CD7CD
		private static string NormalizeIdentifier(string identifier)
		{
			if (identifier == null)
			{
				return null;
			}
			return identifier.Trim().ToLowerInvariant();
		}

		// Token: 0x06005507 RID: 21767 RVA: 0x002CF5E0 File Offset: 0x002CD7E0
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

		// Token: 0x06005508 RID: 21768 RVA: 0x002CF830 File Offset: 0x002CDA30
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

		// Token: 0x17001530 RID: 5424
		// (get) Token: 0x06005509 RID: 21769 RVA: 0x002CFA60 File Offset: 0x002CDC60
		// (set) Token: 0x0600550A RID: 21770 RVA: 0x002CFA68 File Offset: 0x002CDC68
		public bool IsDisposed { get; private set; }

		// Token: 0x0600550B RID: 21771 RVA: 0x002CFA74 File Offset: 0x002CDC74
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

		// Token: 0x0600550C RID: 21772 RVA: 0x002D02A0 File Offset: 0x002CE4A0
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

		// Token: 0x0600550D RID: 21773 RVA: 0x002D0408 File Offset: 0x002CE608
		public string Patch(string identifier, string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterTypes);
			return this.Patch(identifier, method, patch, hookType);
		}

		// Token: 0x0600550E RID: 21774 RVA: 0x002D042C File Offset: 0x002CE62C
		public string Patch(string identifier, string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, null);
			return this.Patch(identifier, method, patch, hookType);
		}

		// Token: 0x0600550F RID: 21775 RVA: 0x002D0450 File Offset: 0x002CE650
		public string Patch(string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterTypes);
			return this.Patch(null, method, patch, hookType);
		}

		// Token: 0x06005510 RID: 21776 RVA: 0x002D0474 File Offset: 0x002CE674
		public string Patch(string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, null);
			return this.Patch(null, method, patch, hookType);
		}

		// Token: 0x06005511 RID: 21777 RVA: 0x002D0498 File Offset: 0x002CE698
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

		// Token: 0x06005512 RID: 21778 RVA: 0x002D050C File Offset: 0x002CE70C
		public bool RemovePatch(string identifier, string className, string methodName, string[] parameterTypes, ILuaCsHook.HookMethodType hookType)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, parameterTypes);
			return this.RemovePatch(identifier, method, hookType);
		}

		// Token: 0x06005513 RID: 21779 RVA: 0x002D0530 File Offset: 0x002CE730
		public bool RemovePatch(string identifier, string className, string methodName, ILuaCsHook.HookMethodType hookType)
		{
			MethodBase method = LuaPatcherService.ResolveMethod(className, methodName, null);
			return this.RemovePatch(identifier, method, hookType);
		}

		// Token: 0x06005514 RID: 21780 RVA: 0x002D0550 File Offset: 0x002CE750
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

		// Token: 0x06005515 RID: 21781 RVA: 0x002D0618 File Offset: 0x002CE818
		public void Dispose()
		{
			this.IsDisposed = true;
			this.ClearAll();
		}

		// Token: 0x06005516 RID: 21782 RVA: 0x002D0627 File Offset: 0x002CE827
		public Result Reset()
		{
			this.ClearAll();
			return Result.Ok();
		}

		// Token: 0x06005518 RID: 21784 RVA: 0x002D06E0 File Offset: 0x002CE8E0
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

		// Token: 0x06005519 RID: 21785 RVA: 0x002D0739 File Offset: 0x002CE939
		[CompilerGenerated]
		internal static string <CreateDynamicHarmonyPatch>g__MangleName|40_0(object o)
		{
			return LuaPatcherService.InvalidIdentifierCharsRegex.Replace((o != null) ? o.ToString() : null, "_");
		}

		// Token: 0x04002C57 RID: 11351
		private static LuaPatcherService instance;

		// Token: 0x04002C58 RID: 11352
		private Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>> compatHookPrefixMethods = new Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>>();

		// Token: 0x04002C59 RID: 11353
		private Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>> compatHookPostfixMethods = new Dictionary<long, HashSet<ValueTuple<string, Barotrauma.LuaCsPatch>>>();

		// Token: 0x04002C5A RID: 11354
		private static MethodInfo _miHookLuaCsPatchPrefix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchPrefix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04002C5B RID: 11355
		private static MethodInfo _miHookLuaCsPatchPostfix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchPostfix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04002C5C RID: 11356
		private static MethodInfo _miHookLuaCsPatchRetPrefix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchRetPrefix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04002C5D RID: 11357
		private static MethodInfo _miHookLuaCsPatchRetPostfix = typeof(LuaPatcherService).GetMethod("HookLuaCsPatchRetPostfix", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04002C5E RID: 11358
		private static readonly string[] prohibitedHooks = new string[]
		{
			"Barotrauma.Lua",
			"Barotrauma.Cs",
			"Barotrauma.ContentPackageManager"
		};

		// Token: 0x04002C5F RID: 11359
		private Harmony harmony;

		// Token: 0x04002C60 RID: 11360
		private Lazy<ModuleBuilder> patchModuleBuilder;

		// Token: 0x04002C61 RID: 11361
		private readonly Dictionary<LuaPatcherService.MethodKey, LuaPatcherService.PatchedMethod> registeredPatches = new Dictionary<LuaPatcherService.MethodKey, LuaPatcherService.PatchedMethod>();

		// Token: 0x04002C62 RID: 11362
		private static readonly Regex InvalidIdentifierCharsRegex = new Regex("[^\\w\\d]", RegexOptions.Compiled);

		// Token: 0x04002C63 RID: 11363
		private const string FIELD_LUACS = "LuaCs";

		// Token: 0x02001347 RID: 4935
		private class LuaCsHookCallback
		{
			// Token: 0x06009700 RID: 38656 RVA: 0x003DA955 File Offset: 0x003D8B55
			public LuaCsHookCallback(string name, string hookName, LuaCsFunc func)
			{
				this.name = name;
				this.hookName = hookName;
				this.func = func;
			}

			// Token: 0x04006297 RID: 25239
			public string name;

			// Token: 0x04006298 RID: 25240
			public string hookName;

			// Token: 0x04006299 RID: 25241
			public LuaCsFunc func;
		}

		// Token: 0x02001348 RID: 4936
		private class LuaCsPatch
		{
			// Token: 0x17001D0A RID: 7434
			// (get) Token: 0x06009701 RID: 38657 RVA: 0x003DA972 File Offset: 0x003D8B72
			// (set) Token: 0x06009702 RID: 38658 RVA: 0x003DA97A File Offset: 0x003D8B7A
			public string Identifier { get; set; }

			// Token: 0x17001D0B RID: 7435
			// (get) Token: 0x06009703 RID: 38659 RVA: 0x003DA983 File Offset: 0x003D8B83
			// (set) Token: 0x06009704 RID: 38660 RVA: 0x003DA98B File Offset: 0x003D8B8B
			public LuaCsPatchFunc PatchFunc { get; set; }
		}

		// Token: 0x02001349 RID: 4937
		private class PatchedMethod
		{
			// Token: 0x06009706 RID: 38662 RVA: 0x003DA99C File Offset: 0x003D8B9C
			public PatchedMethod(MethodInfo harmonyPrefix, MethodInfo harmonyPostfix)
			{
				this.HarmonyPrefixMethod = harmonyPrefix;
				this.HarmonyPostfixMethod = harmonyPostfix;
				this.Prefixes = new Dictionary<string, LuaPatcherService.LuaCsPatch>();
				this.Postfixes = new Dictionary<string, LuaPatcherService.LuaCsPatch>();
			}

			// Token: 0x17001D0C RID: 7436
			// (get) Token: 0x06009707 RID: 38663 RVA: 0x003DA9C8 File Offset: 0x003D8BC8
			public MethodInfo HarmonyPrefixMethod { get; }

			// Token: 0x17001D0D RID: 7437
			// (get) Token: 0x06009708 RID: 38664 RVA: 0x003DA9D0 File Offset: 0x003D8BD0
			public MethodInfo HarmonyPostfixMethod { get; }

			// Token: 0x06009709 RID: 38665 RVA: 0x003DA9D8 File Offset: 0x003D8BD8
			public IEnumerator<LuaPatcherService.LuaCsPatch> GetPrefixEnumerator()
			{
				return this.Prefixes.Values.GetEnumerator();
			}

			// Token: 0x0600970A RID: 38666 RVA: 0x003DA9EF File Offset: 0x003D8BEF
			public IEnumerator<LuaPatcherService.LuaCsPatch> GetPostfixEnumerator()
			{
				return this.Postfixes.Values.GetEnumerator();
			}

			// Token: 0x17001D0E RID: 7438
			// (get) Token: 0x0600970B RID: 38667 RVA: 0x003DAA06 File Offset: 0x003D8C06
			public Dictionary<string, LuaPatcherService.LuaCsPatch> Prefixes { get; }

			// Token: 0x17001D0F RID: 7439
			// (get) Token: 0x0600970C RID: 38668 RVA: 0x003DAA0E File Offset: 0x003D8C0E
			public Dictionary<string, LuaPatcherService.LuaCsPatch> Postfixes { get; }
		}

		// Token: 0x0200134A RID: 4938
		public class ParameterTable
		{
			// Token: 0x0600970D RID: 38669 RVA: 0x003DAA16 File Offset: 0x003D8C16
			public ParameterTable(Dictionary<string, object> dict)
			{
				this.parameters = dict;
			}

			// Token: 0x17001D10 RID: 7440
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

			// Token: 0x17001D11 RID: 7441
			// (get) Token: 0x06009710 RID: 38672 RVA: 0x003DAA6A File Offset: 0x003D8C6A
			// (set) Token: 0x06009711 RID: 38673 RVA: 0x003DAA72 File Offset: 0x003D8C72
			public object OriginalReturnValue { get; private set; }

			// Token: 0x17001D12 RID: 7442
			// (get) Token: 0x06009712 RID: 38674 RVA: 0x003DAA7B File Offset: 0x003D8C7B
			// (set) Token: 0x06009713 RID: 38675 RVA: 0x003DAA92 File Offset: 0x003D8C92
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

			// Token: 0x17001D13 RID: 7443
			// (get) Token: 0x06009714 RID: 38676 RVA: 0x003DAAA2 File Offset: 0x003D8CA2
			// (set) Token: 0x06009715 RID: 38677 RVA: 0x003DAAAA File Offset: 0x003D8CAA
			public bool PreventExecution { get; set; }

			// Token: 0x17001D14 RID: 7444
			// (get) Token: 0x06009716 RID: 38678 RVA: 0x003DAAB3 File Offset: 0x003D8CB3
			public Dictionary<string, object> OriginalParameters
			{
				get
				{
					return this.parameters;
				}
			}

			// Token: 0x17001D15 RID: 7445
			// (get) Token: 0x06009717 RID: 38679 RVA: 0x003DAABB File Offset: 0x003D8CBB
			[MoonSharpHidden]
			public Dictionary<string, object> ModifiedParameters { get; } = new Dictionary<string, object>();

			// Token: 0x040062A0 RID: 25248
			private readonly Dictionary<string, object> parameters;

			// Token: 0x040062A1 RID: 25249
			private bool returnValueModified;

			// Token: 0x040062A2 RID: 25250
			private object returnValue;
		}

		// Token: 0x0200134B RID: 4939
		private struct MethodKey : IEquatable<LuaPatcherService.MethodKey>
		{
			// Token: 0x17001D16 RID: 7446
			// (get) Token: 0x06009718 RID: 38680 RVA: 0x003DAAC3 File Offset: 0x003D8CC3
			// (set) Token: 0x06009719 RID: 38681 RVA: 0x003DAACB File Offset: 0x003D8CCB
			public ModuleHandle ModuleHandle { readonly get; set; }

			// Token: 0x17001D17 RID: 7447
			// (get) Token: 0x0600971A RID: 38682 RVA: 0x003DAAD4 File Offset: 0x003D8CD4
			// (set) Token: 0x0600971B RID: 38683 RVA: 0x003DAADC File Offset: 0x003D8CDC
			public int MetadataToken { readonly get; set; }

			// Token: 0x0600971C RID: 38684 RVA: 0x003DAAE8 File Offset: 0x003D8CE8
			public override bool Equals(object obj)
			{
				if (obj is LuaPatcherService.MethodKey)
				{
					LuaPatcherService.MethodKey key = (LuaPatcherService.MethodKey)obj;
					return this.Equals(key);
				}
				return false;
			}

			// Token: 0x0600971D RID: 38685 RVA: 0x003DAB10 File Offset: 0x003D8D10
			public bool Equals(LuaPatcherService.MethodKey other)
			{
				return this.ModuleHandle.Equals(other.ModuleHandle) && this.MetadataToken == other.MetadataToken;
			}

			// Token: 0x0600971E RID: 38686 RVA: 0x003DAB45 File Offset: 0x003D8D45
			public override int GetHashCode()
			{
				return HashCode.Combine<ModuleHandle, int>(this.ModuleHandle, this.MetadataToken);
			}

			// Token: 0x0600971F RID: 38687 RVA: 0x003DAB58 File Offset: 0x003D8D58
			public static bool operator ==(LuaPatcherService.MethodKey left, LuaPatcherService.MethodKey right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009720 RID: 38688 RVA: 0x003DAB62 File Offset: 0x003D8D62
			public static bool operator !=(LuaPatcherService.MethodKey left, LuaPatcherService.MethodKey right)
			{
				return !(left == right);
			}

			// Token: 0x06009721 RID: 38689 RVA: 0x003DAB70 File Offset: 0x003D8D70
			public static LuaPatcherService.MethodKey Create(MethodBase method)
			{
				return new LuaPatcherService.MethodKey
				{
					ModuleHandle = method.Module.ModuleHandle,
					MetadataToken = method.MetadataToken
				};
			}
		}

		// Token: 0x0200134C RID: 4940
		private class DynamicParameterMapping
		{
			// Token: 0x06009722 RID: 38690 RVA: 0x003DABA5 File Offset: 0x003D8DA5
			public DynamicParameterMapping(string name, Type originalMethodParamType, Type harmonyPatchParamType)
			{
				this.ParameterName = name;
				this.OriginalMethodParamType = originalMethodParamType;
				this.HarmonyPatchParamType = harmonyPatchParamType;
			}

			// Token: 0x17001D18 RID: 7448
			// (get) Token: 0x06009723 RID: 38691 RVA: 0x003DABC2 File Offset: 0x003D8DC2
			// (set) Token: 0x06009724 RID: 38692 RVA: 0x003DABCA File Offset: 0x003D8DCA
			public string ParameterName { get; set; }

			// Token: 0x17001D19 RID: 7449
			// (get) Token: 0x06009725 RID: 38693 RVA: 0x003DABD3 File Offset: 0x003D8DD3
			// (set) Token: 0x06009726 RID: 38694 RVA: 0x003DABDB File Offset: 0x003D8DDB
			public Type OriginalMethodParamType { get; set; }

			// Token: 0x17001D1A RID: 7450
			// (get) Token: 0x06009727 RID: 38695 RVA: 0x003DABE4 File Offset: 0x003D8DE4
			// (set) Token: 0x06009728 RID: 38696 RVA: 0x003DABEC File Offset: 0x003D8DEC
			public Type HarmonyPatchParamType { get; set; }
		}
	}
}
