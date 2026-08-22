using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using FluentResults;
using Microsoft.Toolkit.Diagnostics;
using MoonSharp.Interpreter;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003E9 RID: 1001
	public class EventService : IEventService, IReusableService, IService, IDisposable, ILuaEventService, ILuaSafeEventService, ILuaService, ILuaCsHook, ILuaPatcher, ILuaCsShim
	{
		// Token: 0x06003971 RID: 14705 RVA: 0x0017F94C File Offset: 0x0017DB4C
		public void Dispose()
		{
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
				{
					this._luaLegacyEventsSubscribers.Clear();
					this._luaAliasEventFactory.Clear();
					this._subscribers.Clear();
					this._luaPatcher.Dispose();
				}
			}
		}

		// Token: 0x06003972 RID: 14706 RVA: 0x0017F9DC File Offset: 0x0017DBDC
		public EventService(ILoggerService loggerService, ILuaPatcher luaPatcher)
		{
			this._loggerService = loggerService;
			this._luaPatcher = luaPatcher;
		}

		// Token: 0x17000FA6 RID: 4006
		// (get) Token: 0x06003973 RID: 14707 RVA: 0x0017FA34 File Offset: 0x0017DC34
		// (set) Token: 0x06003974 RID: 14708 RVA: 0x0017FA41 File Offset: 0x0017DC41
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

		// Token: 0x06003975 RID: 14709 RVA: 0x0017FA50 File Offset: 0x0017DC50
		public Result Reset()
		{
			Result result;
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._luaLegacyEventsSubscribers.Clear();
				this._luaAliasEventFactory.Clear();
				this._subscribers.Clear();
				this._luaPatcher.Reset();
				result = Result.Ok();
			}
			return result;
		}

		// Token: 0x06003976 RID: 14710 RVA: 0x0017FAE0 File Offset: 0x0017DCE0
		public void Add(string eventName, string identifier, LuaCsFunc callback, object owner = null)
		{
			Guard.IsNotNullOrWhiteSpace(eventName, "eventName");
			Guard.IsNotNullOrWhiteSpace(identifier, "identifier");
			Guard.IsNotNull<LuaCsFunc>(callback, "callback");
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ValueTuple<EventService.TypeStringKey, Func<LuaCsFunc, IEvent>> eventFunc;
				if (this._luaAliasEventFactory.TryGetValue(eventName, out eventFunc))
				{
					ConcurrentDictionary<OneOf<IEvent, string>, IEvent> eventSubs = this._subscribers.GetOrAdd(eventFunc.Item1, (EventService.TypeStringKey key) => new ConcurrentDictionary<OneOf<IEvent, string>, IEvent>());
					eventSubs[identifier] = eventFunc.Item2(callback);
				}
				else
				{
					ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc> eventSubs2 = this._luaLegacyEventsSubscribers.GetOrAdd(eventName, (EventService.TypeStringKey key) => new ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc>());
					eventSubs2[identifier] = callback;
				}
			}
		}

		// Token: 0x06003977 RID: 14711 RVA: 0x0017FC00 File Offset: 0x0017DE00
		public void Add(string eventName, LuaCsFunc callback, object owner = null)
		{
			this.Add(eventName, Random.Shared.NextInt64().ToString(), callback, null);
		}

		// Token: 0x06003978 RID: 14712 RVA: 0x0017FC28 File Offset: 0x0017DE28
		public object Call(string eventName, params object[] args)
		{
			return this.Call<object>(eventName, args);
		}

		// Token: 0x06003979 RID: 14713 RVA: 0x0017FC34 File Offset: 0x0017DE34
		[MoonSharpHidden]
		public T Call<T>(string eventName, params object[] args)
		{
			Guard.IsNotNullOrWhiteSpace(eventName, "eventName");
			T t;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc> eventSubscribers;
				if (!this._luaLegacyEventsSubscribers.TryGetValue(eventName, out eventSubscribers) || eventSubscribers.IsEmpty)
				{
					t = default(T);
					t = t;
				}
				else
				{
					T returnValue = default(T);
					foreach (KeyValuePair<EventService.TypeStringKey, LuaCsFunc> subscriber in eventSubscribers)
					{
						try
						{
							object result = subscriber.Value(args);
							DynValue luaResult = result as DynValue;
							if (luaResult != null)
							{
								if (luaResult.Type == DataType.Tuple)
								{
									bool replaceNil = luaResult.Tuple.Length > 1 && luaResult.Tuple[1].CastToBool();
									if (!luaResult.Tuple[0].IsNil() || replaceNil)
									{
										returnValue = luaResult.ToObject<T>();
									}
								}
								else if (!luaResult.IsNil())
								{
									returnValue = luaResult.ToObject<T>();
								}
							}
							else
							{
								returnValue = (T)((object)result);
							}
						}
						catch (Exception e)
						{
							this._loggerService.LogError(e.Message);
						}
					}
					t = returnValue;
				}
			}
			return t;
		}

		// Token: 0x0600397A RID: 14714 RVA: 0x0017FDDC File Offset: 0x0017DFDC
		public void Subscribe<T>(string identifier, IDictionary<string, LuaCsFunc> callbacks) where T : class, IEvent<T>
		{
			Guard.IsNotNullOrWhiteSpace(identifier, "identifier");
			Guard.IsNotNull<IDictionary<string, LuaCsFunc>>(callbacks, "callbacks");
			Guard.IsNotEmpty<KeyValuePair<string, LuaCsFunc>>(callbacks, "callbacks");
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentDictionary<OneOf<IEvent, string>, IEvent> eventSubs = this._subscribers.GetOrAdd(typeof(T), (EventService.TypeStringKey key) => new ConcurrentDictionary<OneOf<IEvent, string>, IEvent>());
				eventSubs[identifier] = IEvent<T>.GetLuaRunner(callbacks);
			}
		}

		// Token: 0x0600397B RID: 14715 RVA: 0x0017FEB0 File Offset: 0x0017E0B0
		public void Remove(string eventName, string identifier)
		{
			Guard.IsNotNullOrWhiteSpace(eventName, "eventName");
			Guard.IsNotNullOrWhiteSpace(identifier, "identifier");
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ValueTuple<EventService.TypeStringKey, Func<LuaCsFunc, IEvent>> eventFunc;
				ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc> eventSubs2;
				if (this._luaAliasEventFactory.TryGetValue(eventName, out eventFunc))
				{
					ConcurrentDictionary<OneOf<IEvent, string>, IEvent> eventSubs;
					if (this._subscribers.TryGetValue(eventFunc.Item1, out eventSubs))
					{
						IEvent @event;
						eventSubs.TryRemove(identifier, out @event);
					}
				}
				else if (this._luaLegacyEventsSubscribers.TryGetValue(eventName, out eventSubs2))
				{
					LuaCsFunc luaCsFunc;
					eventSubs2.TryRemove(identifier, out luaCsFunc);
				}
			}
		}

		// Token: 0x0600397C RID: 14716 RVA: 0x0017FF84 File Offset: 0x0017E184
		public void Unsubscribe(string eventName, string identifier)
		{
			Guard.IsNotNullOrWhiteSpace(eventName, "eventName");
			Guard.IsNotNullOrWhiteSpace(identifier, "identifier");
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				ConcurrentDictionary<OneOf<IEvent, string>, IEvent> evtSubscribers;
				if (this._subscribers.TryGetValue(eventName, out evtSubscribers))
				{
					IEvent @event;
					evtSubscribers.TryRemove(identifier, out @event);
				}
			}
		}

		// Token: 0x0600397D RID: 14717 RVA: 0x00180018 File Offset: 0x0017E218
		public void PublishLuaEvent<T>(LuaCsFunc subscriberRunner) where T : class, IEvent<T>
		{
			this.PublishEvent<T>(delegate(T sub)
			{
				subscriberRunner(new object[]
				{
					sub
				});
			});
		}

		// Token: 0x0600397E RID: 14718 RVA: 0x00180048 File Offset: 0x0017E248
		public Result RegisterLuaEventAlias<T>(string luaEventName, string targetMethod) where T : class, IEvent<T>
		{
			Guard.IsNotNullOrWhiteSpace(luaEventName, "luaEventName");
			Guard.IsNotNullOrWhiteSpace(targetMethod, "targetMethod");
			Result result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				if (this._luaAliasEventFactory.ContainsKey(luaEventName))
				{
					result = Result.Fail("RegisterLuaEventAlias: An alias already exists for the event of " + luaEventName + ".");
				}
				else
				{
					Func<LuaCsFunc, IEvent> eventRunnerFactory = (LuaCsFunc function) => IEvent<T>.GetLuaRunner(new Dictionary<string, LuaCsFunc>
					{
						{
							targetMethod,
							function
						}
					});
					this._luaAliasEventFactory[luaEventName] = new ValueTuple<EventService.TypeStringKey, Func<LuaCsFunc, IEvent>>(typeof(T), eventRunnerFactory);
					this._subscribers.GetOrAdd(typeof(T), (EventService.TypeStringKey key) => new ConcurrentDictionary<OneOf<IEvent, string>, IEvent>());
					result = Result.Ok();
				}
			}
			return result;
		}

		// Token: 0x0600397F RID: 14719 RVA: 0x00180174 File Offset: 0x0017E374
		public Result Subscribe<T>(T subscriber) where T : class, IEvent<T>
		{
			Guard.IsNotNull<T>(subscriber, "subscriber");
			Result result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentDictionary<OneOf<IEvent, string>, IEvent> eventSubs = this._subscribers.GetOrAdd(typeof(T), (EventService.TypeStringKey type) => new ConcurrentDictionary<OneOf<IEvent, string>, IEvent>());
				if (eventSubs.ContainsKey(subscriber))
				{
					ThrowHelper.ThrowInvalidOperationException("Subscribe: The instance is already registered!");
				}
				result = (eventSubs.TryAdd(subscriber, subscriber) ? Result.Ok() : Result.Fail("Subscribe: Failed to add subscriber."));
			}
			return result;
		}

		// Token: 0x06003980 RID: 14720 RVA: 0x00180260 File Offset: 0x0017E460
		public void Unsubscribe<T>(T subscriber) where T : class, IEvent
		{
			Guard.IsNotNull<T>(subscriber, "subscriber");
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentDictionary<OneOf<IEvent, string>, IEvent> evtSubscribers;
				if (this._subscribers.TryGetValue(typeof(T), out evtSubscribers))
				{
					IEvent @event;
					evtSubscribers.TryRemove(subscriber, out @event);
				}
			}
		}

		// Token: 0x06003981 RID: 14721 RVA: 0x001802FC File Offset: 0x0017E4FC
		public void ClearAllEventSubscribers<T>() where T : class, IEvent
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentDictionary<OneOf<IEvent, string>, IEvent> concurrentDictionary;
				this._subscribers.TryRemove(typeof(T), out concurrentDictionary);
			}
		}

		// Token: 0x06003982 RID: 14722 RVA: 0x00180374 File Offset: 0x0017E574
		public void ClearAllSubscribers()
		{
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._subscribers.Clear();
			}
		}

		// Token: 0x06003983 RID: 14723 RVA: 0x001803DC File Offset: 0x0017E5DC
		public Result PublishEvent<T>(Action<T> action) where T : class, IEvent<T>
		{
			Guard.IsNotNull<Action<T>>(action, "action");
			Result result;
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentDictionary<OneOf<IEvent, string>, IEvent> subs;
				if (!this._subscribers.TryGetValue(typeof(T), out subs) || subs.IsEmpty)
				{
					result = Result.Ok();
				}
				else
				{
					Result results = new Result();
					foreach (KeyValuePair<OneOf<IEvent, string>, IEvent> sub in subs)
					{
						try
						{
							action(Unsafe.As<T>(sub.Value));
						}
						catch (Exception e)
						{
							results.WithError(new ExceptionalError(e));
							this._loggerService.LogError(e.Message);
						}
					}
					foreach (KeyValuePair<IEventService, IEventService> dispatchers in this._subscribedEventDispatchers.ToImmutableArray<KeyValuePair<IEventService, IEventService>>())
					{
						dispatchers.Value.PublishEvent<T>(action);
					}
					result = results;
				}
			}
			return result;
		}

		// Token: 0x06003984 RID: 14724 RVA: 0x00180534 File Offset: 0x0017E734
		public void AddDispatcherEventService(IEventService eventService)
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._subscribedEventDispatchers.TryAdd(eventService, eventService);
			}
		}

		// Token: 0x06003985 RID: 14725 RVA: 0x0018059C File Offset: 0x0017E79C
		public void RemoveDispatcherEventService(IEventService eventService)
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				IEventService eventService2;
				this._subscribedEventDispatchers.TryRemove(eventService, out eventService2);
			}
		}

		// Token: 0x06003986 RID: 14726 RVA: 0x00180608 File Offset: 0x0017E808
		public string Patch(string identifier, string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(identifier, className, methodName, parameterTypes, patch, hookType);
		}

		// Token: 0x06003987 RID: 14727 RVA: 0x0018061E File Offset: 0x0017E81E
		public string Patch(string identifier, string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(identifier, className, methodName, patch, hookType);
		}

		// Token: 0x06003988 RID: 14728 RVA: 0x00180632 File Offset: 0x0017E832
		public string Patch(string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(className, methodName, parameterTypes, patch, hookType);
		}

		// Token: 0x06003989 RID: 14729 RVA: 0x00180646 File Offset: 0x0017E846
		public string Patch(string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(className, methodName, patch, hookType);
		}

		// Token: 0x0600398A RID: 14730 RVA: 0x00180658 File Offset: 0x0017E858
		public bool RemovePatch(string identifier, string className, string methodName, string[] parameterTypes, ILuaCsHook.HookMethodType hookType)
		{
			return this._luaPatcher.RemovePatch(className, className, methodName, parameterTypes, hookType);
		}

		// Token: 0x0600398B RID: 14731 RVA: 0x0018066C File Offset: 0x0017E86C
		public bool RemovePatch(string identifier, string className, string methodName, ILuaCsHook.HookMethodType hookType)
		{
			return this._luaPatcher.RemovePatch(className, className, methodName, hookType);
		}

		// Token: 0x0600398C RID: 14732 RVA: 0x0018067E File Offset: 0x0017E87E
		public void HookMethod(string identifier, MethodBase method, LuaCsPatch patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before, IAssemblyPlugin owner = null)
		{
			this._luaPatcher.HookMethod(identifier, method, patch, hookType, owner);
		}

		// Token: 0x0600398D RID: 14733 RVA: 0x00180692 File Offset: 0x0017E892
		public void HookMethod(string identifier, string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(identifier, className, methodName, parameterNames, patch, hookMethodType);
		}

		// Token: 0x0600398E RID: 14734 RVA: 0x001806A8 File Offset: 0x0017E8A8
		public void HookMethod(string identifier, string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(identifier, className, methodName, patch, hookMethodType);
		}

		// Token: 0x0600398F RID: 14735 RVA: 0x001806BC File Offset: 0x0017E8BC
		public void HookMethod(string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(className, methodName, patch, hookMethodType);
		}

		// Token: 0x06003990 RID: 14736 RVA: 0x001806CE File Offset: 0x0017E8CE
		public void HookMethod(string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(className, methodName, parameterNames, patch, hookMethodType);
		}

		// Token: 0x04001CDE RID: 7390
		private readonly ILoggerService _loggerService;

		// Token: 0x04001CDF RID: 7391
		private readonly ILuaPatcher _luaPatcher;

		// Token: 0x04001CE0 RID: 7392
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04001CE1 RID: 7393
		private readonly ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<OneOf<IEvent, string>, IEvent>> _subscribers = new ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<OneOf<IEvent, string>, IEvent>>();

		// Token: 0x04001CE2 RID: 7394
		[TupleElementNames(new string[]
		{
			"Event",
			"RunnerFactory"
		})]
		private readonly ConcurrentDictionary<EventService.TypeStringKey, ValueTuple<EventService.TypeStringKey, Func<LuaCsFunc, IEvent>>> _luaAliasEventFactory = new ConcurrentDictionary<EventService.TypeStringKey, ValueTuple<EventService.TypeStringKey, Func<LuaCsFunc, IEvent>>>();

		// Token: 0x04001CE3 RID: 7395
		private readonly ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc>> _luaLegacyEventsSubscribers = new ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc>>();

		// Token: 0x04001CE4 RID: 7396
		private readonly ConcurrentDictionary<IEventService, IEventService> _subscribedEventDispatchers = new ConcurrentDictionary<IEventService, IEventService>();

		// Token: 0x04001CE5 RID: 7397
		private int _isDisposed;

		// Token: 0x02000C99 RID: 3225
		private readonly struct TypeStringKey : IEqualityComparer<EventService.TypeStringKey>, IEquatable<EventService.TypeStringKey>
		{
			// Token: 0x17001645 RID: 5701
			// (get) Token: 0x060064C3 RID: 25795 RVA: 0x002161BE File Offset: 0x002143BE
			// (set) Token: 0x060064C4 RID: 25796 RVA: 0x002161C6 File Offset: 0x002143C6
			public Type Type { get; set; }

			// Token: 0x17001646 RID: 5702
			// (get) Token: 0x060064C5 RID: 25797 RVA: 0x002161CF File Offset: 0x002143CF
			// (set) Token: 0x060064C6 RID: 25798 RVA: 0x002161D7 File Offset: 0x002143D7
			public string TypeName { get; set; }

			// Token: 0x060064C7 RID: 25799 RVA: 0x002161E0 File Offset: 0x002143E0
			public TypeStringKey(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				this.Type = type;
				this.TypeName = type.Name.ToLowerInvariant();
				this.HashCode = this.TypeName.GetHashCode();
			}

			// Token: 0x060064C8 RID: 25800 RVA: 0x0021621A File Offset: 0x0021441A
			public TypeStringKey(string typeName)
			{
				this.Type = null;
				string text = (typeName != null) ? typeName.ToLowerInvariant() : null;
				if (text == null)
				{
					throw new ArgumentNullException("typeName");
				}
				this.TypeName = text;
				this.HashCode = this.TypeName.GetHashCode();
			}

			// Token: 0x060064C9 RID: 25801 RVA: 0x00216255 File Offset: 0x00214455
			public bool Equals(EventService.TypeStringKey x, EventService.TypeStringKey y)
			{
				if (x.Type != null && y.Type != null)
				{
					return x.Type == y.Type;
				}
				return x.TypeName == y.TypeName;
			}

			// Token: 0x060064CA RID: 25802 RVA: 0x00216290 File Offset: 0x00214490
			public int GetHashCode(EventService.TypeStringKey obj)
			{
				return obj.HashCode;
			}

			// Token: 0x060064CB RID: 25803 RVA: 0x00216298 File Offset: 0x00214498
			public static implicit operator EventService.TypeStringKey(Type type)
			{
				return new EventService.TypeStringKey(type);
			}

			// Token: 0x060064CC RID: 25804 RVA: 0x002162A0 File Offset: 0x002144A0
			public static implicit operator EventService.TypeStringKey(string typeName)
			{
				return new EventService.TypeStringKey(typeName);
			}

			// Token: 0x060064CD RID: 25805 RVA: 0x002162A8 File Offset: 0x002144A8
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("TypeStringKey");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060064CE RID: 25806 RVA: 0x002162F4 File Offset: 0x002144F4
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Type = ");
				builder.Append(this.Type);
				builder.Append(", TypeName = ");
				builder.Append(this.TypeName);
				builder.Append(", HashCode = ");
				builder.Append(this.HashCode.ToString());
				return true;
			}

			// Token: 0x060064CF RID: 25807 RVA: 0x00216358 File Offset: 0x00214558
			[CompilerGenerated]
			public static bool operator !=(EventService.TypeStringKey left, EventService.TypeStringKey right)
			{
				return !(left == right);
			}

			// Token: 0x060064D0 RID: 25808 RVA: 0x00216364 File Offset: 0x00214564
			[CompilerGenerated]
			public static bool operator ==(EventService.TypeStringKey left, EventService.TypeStringKey right)
			{
				return left.Equals(right);
			}

			// Token: 0x060064D1 RID: 25809 RVA: 0x0021636E File Offset: 0x0021456E
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Type>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<TypeName>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.HashCode);
			}

			// Token: 0x060064D2 RID: 25810 RVA: 0x002163AE File Offset: 0x002145AE
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is EventService.TypeStringKey && this.Equals((EventService.TypeStringKey)obj);
			}

			// Token: 0x060064D3 RID: 25811 RVA: 0x002163C8 File Offset: 0x002145C8
			[CompilerGenerated]
			public bool Equals(EventService.TypeStringKey other)
			{
				return EqualityComparer<Type>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<TypeName>k__BackingField, other.<TypeName>k__BackingField) && EqualityComparer<int>.Default.Equals(this.HashCode, other.HashCode);
			}

			// Token: 0x04003CEB RID: 15595
			public readonly int HashCode;
		}
	}
}
