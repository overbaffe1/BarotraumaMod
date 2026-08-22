using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma
{
	// Token: 0x02000210 RID: 528
	public class LuaConverters
	{
		// Token: 0x0600251E RID: 9502 RVA: 0x000F43F1 File Offset: 0x000F25F1
		public LuaConverters(ILuaScriptManagementService luaScriptManagementService)
		{
			this._luaScriptManagementService = luaScriptManagementService;
		}

		// Token: 0x0600251F RID: 9503 RVA: 0x000F4400 File Offset: 0x000F2600
		private DynValue Call(object function, params object[] arguments)
		{
			return this._luaScriptManagementService.CallFunctionSafe(function, arguments);
		}

		// Token: 0x06002520 RID: 9504 RVA: 0x000F4410 File Offset: 0x000F2610
		public void RegisterLuaConverters()
		{
			this.RegisterAction<Item>();
			this.RegisterAction<Character>();
			this.RegisterAction<Character, Character>();
			this.RegisterAction<Entity>();
			this.RegisterAction<float>();
			this.RegisterAction();
			this.RegisterFunc<Fixture, Vector2, Vector2, float, float>();
			this.RegisterFunc<AIObjective, bool>();
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(LuaCsAction), (DynValue v) => new LuaCsAction(delegate(object[] args)
			{
				if (v.Function.OwnerScript == this._luaScriptManagementService.InternalScript)
				{
					this.Call(v.Function, args);
				}
			}), null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(LuaCsFunc), (DynValue v) => new LuaCsFunc(delegate(object[] args)
			{
				if (v.Function.OwnerScript == this._luaScriptManagementService.InternalScript)
				{
					return this.Call(v.Function, args);
				}
				return null;
			}), null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(LuaCsPatch), (DynValue v) => new LuaCsPatch(delegate(object self, Dictionary<string, object> args)
			{
				if (v.Function.OwnerScript == this._luaScriptManagementService.InternalScript)
				{
					return this.Call(v.Function, new object[]
					{
						self,
						args
					});
				}
				return null;
			}), null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(LuaCsPatchFunc), (DynValue v) => new LuaCsPatchFunc(delegate(object self, LuaPatcherService.ParameterTable args)
			{
				if (v.Function.OwnerScript == this._luaScriptManagementService.InternalScript)
				{
					return this.Call(v.Function, new object[]
					{
						self,
						args
					});
				}
				return null;
			}), null);
			LuaConverters.<RegisterLuaConverters>g__RegisterHandler|3_4<Character.OnDeathHandler>((MoonSharp.Interpreter.Closure f) => delegate(Character a1, CauseOfDeath a2)
			{
				this.Call(f, new object[]
				{
					a1,
					a2
				});
			});
			LuaConverters.<RegisterLuaConverters>g__RegisterHandler|3_4<Character.OnAttackedHandler>((MoonSharp.Interpreter.Closure f) => delegate(Character a1, AttackResult a2)
			{
				this.Call(f, new object[]
				{
					a1,
					a2
				});
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(NetMessageReceived), (DynValue v) => new NetMessageReceived(delegate(IReadMessage arg1, Client arg2)
			{
				if (v.Function.OwnerScript == this._luaScriptManagementService.InternalScript)
				{
					this.Call(v.Function, new object[]
					{
						arg1,
						arg2
					});
				}
			}), null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(Pair<JobPrefab, int>), (DynValue v) => new Pair<JobPrefab, int>((JobPrefab)v.Table.Get(1).ToObject(), (int)v.Table.Get(2).CastToNumber().Value), null);
			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<ulong>((Script script, ulong v) => DynValue.NewString(v.ToString()));
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.String, typeof(ulong), (DynValue v) => ulong.Parse(v.String), null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(sbyte), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaSByte)
				{
					LuaSByte v = (LuaSByte)@object;
					return v;
				}
				throw new ScriptRuntimeException("use SByte(value) to pass primitive type 'sbyte' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaSByte;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(byte), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaByte)
				{
					LuaByte v = (LuaByte)@object;
					return v;
				}
				throw new ScriptRuntimeException("use Byte(value) to pass primitive type 'byte' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaByte;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(short), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaInt16)
				{
					LuaInt16 v = (LuaInt16)@object;
					return v;
				}
				throw new ScriptRuntimeException("use Int16(value) to pass primitive type 'short' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaInt16;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(ushort), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaUInt16)
				{
					LuaUInt16 v = (LuaUInt16)@object;
					return v;
				}
				throw new ScriptRuntimeException("use UInt16(value) to pass primitive type 'ushort' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaUInt16;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(int), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaInt32)
				{
					LuaInt32 v = (LuaInt32)@object;
					return v;
				}
				throw new ScriptRuntimeException("use Int32(value) to pass primitive type 'int' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaInt32;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(uint), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaUInt32)
				{
					LuaUInt32 v = (LuaUInt32)@object;
					return v;
				}
				throw new ScriptRuntimeException("use UInt32(value) to pass primitive type 'uint' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaUInt32;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(long), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaInt64)
				{
					LuaInt64 v = (LuaInt64)@object;
					return v;
				}
				throw new ScriptRuntimeException("use Int64(value) to pass primitive type 'long' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaInt64;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(ulong), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaUInt64)
				{
					LuaUInt64 v = (LuaUInt64)@object;
					return v;
				}
				throw new ScriptRuntimeException("use UInt64(value) to pass primitive type 'ulong' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaUInt64;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(float), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaSingle)
				{
					LuaSingle v = (LuaSingle)@object;
					return v;
				}
				throw new ScriptRuntimeException("use Single(value) to pass primitive type 'float' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaSingle;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.UserData, typeof(double), delegate(DynValue luaValue)
			{
				object @object = luaValue.UserData.Object;
				if (@object is LuaDouble)
				{
					LuaDouble v = (LuaDouble)@object;
					return v;
				}
				throw new ScriptRuntimeException("use Double(value) to pass primitive type 'double' to C#");
			}, delegate(DynValue luaValue)
			{
				UserData userData = luaValue.UserData;
				return ((userData != null) ? userData.Object : null) is LuaDouble;
			});
			LuaConverters.RegisterOption<Character>(DataType.UserData);
			LuaConverters.RegisterOption<AccountId>(DataType.UserData);
			LuaConverters.RegisterOption<ContentPackageId>(DataType.UserData);
			LuaConverters.RegisterOption<SteamId>(DataType.UserData);
			LuaConverters.RegisterOption<DateTime>(DataType.UserData);
			LuaConverters.RegisterOption<BannedPlayer>(DataType.UserData);
			LuaConverters.RegisterOption<Address>(DataType.UserData);
			LuaConverters.RegisterOption<int>(DataType.Number);
			LuaConverters.RegisterEither<Address, AccountId>();
			LuaConverters.RegisterImmutableArray<FactionPrefab.HireableCharacter>();
		}

		// Token: 0x06002521 RID: 9505 RVA: 0x000F497E File Offset: 0x000F2B7E
		private static void RegisterImmutableArray<T>()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(ImmutableArray<T>), (DynValue v) => v.ToObject<T[]>().ToImmutableArray<T>(), null);
		}

		// Token: 0x06002522 RID: 9506 RVA: 0x000F49BC File Offset: 0x000F2BBC
		private static void RegisterEither<T1, T2>()
		{
			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(typeof(EitherT<T1, T2>), delegate(Script v, object obj)
			{
				EitherT<T1, T2> either = obj as EitherT<T1, T2>;
				if (either != null)
				{
					return LuaConverters.<RegisterEither>g__convertEitherIntoDynValue|5_0<T1, T2>(either);
				}
				return null;
			});
			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(typeof(EitherU<T1, T2>), delegate(Script v, object obj)
			{
				EitherU<T1, T2> either = obj as EitherU<T1, T2>;
				if (either != null)
				{
					return LuaConverters.<RegisterEither>g__convertEitherIntoDynValue|5_0<T1, T2>(either);
				}
				return null;
			});
		}

		// Token: 0x06002523 RID: 9507 RVA: 0x000F4A3C File Offset: 0x000F2C3C
		private static void RegisterOption<T>(DataType dataType)
		{
			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(typeof(Option<T>), delegate(Script v, object obj)
			{
				T outValue;
				if (obj is Option<T> && ((Option<T>)obj).TryUnwrap(out outValue))
				{
					return UserData.Create(outValue);
				}
				return DynValue.Nil;
			});
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(dataType, typeof(Option<T>), (DynValue v) => Option<T>.Some(v.ToObject<T>()), null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Nil, typeof(Option<T>), (DynValue v) => Option<T>.None(), null);
		}

		// Token: 0x06002524 RID: 9508 RVA: 0x000F4AF8 File Offset: 0x000F2CF8
		private void RegisterAction<T>()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action<T>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Action<T>(delegate(T p)
				{
					this.Call(function, new object[]
					{
						p
					});
				});
			}, null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Action<T>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Action<T>(delegate(T p)
				{
					this.Call(function, new object[]
					{
						p
					});
				});
			}, null);
		}

		// Token: 0x06002525 RID: 9509 RVA: 0x000F4B54 File Offset: 0x000F2D54
		private void RegisterAction<T1, T2>()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action<T1, T2>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Action<T1, T2>(delegate(T1 a1, T2 a2)
				{
					this.Call(function, new object[]
					{
						a1,
						a2
					});
				});
			}, null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Action<T1, T2>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Action<T1, T2>(delegate(T1 a1, T2 a2)
				{
					this.Call(function, new object[]
					{
						a1,
						a2
					});
				});
			}, null);
		}

		// Token: 0x06002526 RID: 9510 RVA: 0x000F4BB0 File Offset: 0x000F2DB0
		private void RegisterAction()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Action), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Action(delegate()
				{
					this.Call(function, Array.Empty<object>());
				});
			}, null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Action), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Action(delegate()
				{
					this.Call(function, Array.Empty<object>());
				});
			}, null);
		}

		// Token: 0x06002527 RID: 9511 RVA: 0x000F4C0C File Offset: 0x000F2E0C
		private void RegisterFunc<T1>()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Func<T1>(() => function.Call().ToObject<T1>());
			}, null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Func<T1>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Func<T1>(() => function.Call().ToObject<T1>());
			}, null);
		}

		// Token: 0x06002528 RID: 9512 RVA: 0x000F4C90 File Offset: 0x000F2E90
		private void RegisterFunc<T1, T2>()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1, T2>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Func<T1, T2>((T1 a) => function.Call(new object[]
				{
					a
				}).ToObject<T2>());
			}, null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.ClrFunction, typeof(Func<T1, T2>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Func<T1, T2>((T1 a) => function.Call(new object[]
				{
					a
				}).ToObject<T2>());
			}, null);
		}

		// Token: 0x06002529 RID: 9513 RVA: 0x000F4D14 File Offset: 0x000F2F14
		private void RegisterFunc<T1, T2, T3, T4, T5>()
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1, T2, T3, T4, T5>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Func<T1, T2, T3, T4, T5>((T1 a, T2 b, T3 c, T4 d) => function.Call(new object[]
				{
					a,
					b,
					c,
					d
				}).ToObject<T5>());
			}, null);
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(Func<T1, T2, T3, T4, T5>), delegate(DynValue v)
			{
				MoonSharp.Interpreter.Closure function = v.Function;
				return new Func<T1, T2, T3, T4, T5>((T1 a, T2 b, T3 c, T4 d) => function.Call(new object[]
				{
					a,
					b,
					c,
					d
				}).ToObject<T5>());
			}, null);
		}

		// Token: 0x0600252E RID: 9518 RVA: 0x000F4E58 File Offset: 0x000F3058
		[CompilerGenerated]
		internal static void <RegisterLuaConverters>g__RegisterHandler|3_4<T>(Func<MoonSharp.Interpreter.Closure, T> converter)
		{
			Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(T), (DynValue v) => converter(v.Function), null);
		}

		// Token: 0x06002532 RID: 9522 RVA: 0x000F4F2C File Offset: 0x000F312C
		[CompilerGenerated]
		internal static DynValue <RegisterEither>g__convertEitherIntoDynValue|5_0<T1, T2>(Either<T1, T2> either)
		{
			T1 value;
			if (either.TryGet(out value))
			{
				return UserData.Create(value);
			}
			T2 value2;
			if (either.TryGet(out value2))
			{
				return UserData.Create(value2);
			}
			return null;
		}

		// Token: 0x0400124B RID: 4683
		private readonly ILuaScriptManagementService _luaScriptManagementService;
	}
}
