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
	// Token: 0x020004FF RID: 1279
	public class EventService : IEventService, IReusableService, IService, IDisposable, ILuaEventService, ILuaSafeEventService, ILuaService, ILuaCsHook, ILuaPatcher, ILuaCsShim
	{
		// Token: 0x060052B0 RID: 21168 RVA: 0x002C4864 File Offset: 0x002C2A64
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

		// Token: 0x060052B1 RID: 21169 RVA: 0x002C48F4 File Offset: 0x002C2AF4
		public EventService(ILoggerService loggerService, ILuaPatcher luaPatcher)
		{
			this._loggerService = loggerService;
			this._luaPatcher = luaPatcher;
		}

		// Token: 0x170014EF RID: 5359
		// (get) Token: 0x060052B2 RID: 21170 RVA: 0x002C494C File Offset: 0x002C2B4C
		// (set) Token: 0x060052B3 RID: 21171 RVA: 0x002C4959 File Offset: 0x002C2B59
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

		// Token: 0x060052B4 RID: 21172 RVA: 0x002C4968 File Offset: 0x002C2B68
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

		// Token: 0x060052B5 RID: 21173 RVA: 0x002C49F8 File Offset: 0x002C2BF8
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

		// Token: 0x060052B6 RID: 21174 RVA: 0x002C4B18 File Offset: 0x002C2D18
		public void Add(string eventName, LuaCsFunc callback, object owner = null)
		{
			this.Add(eventName, Random.Shared.NextInt64().ToString(), callback, null);
		}

		// Token: 0x060052B7 RID: 21175 RVA: 0x002C4B40 File Offset: 0x002C2D40
		public object Call(string eventName, params object[] args)
		{
			return this.Call<object>(eventName, args);
		}

		// Token: 0x060052B8 RID: 21176 RVA: 0x002C4B4C File Offset: 0x002C2D4C
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

		// Token: 0x060052B9 RID: 21177 RVA: 0x002C4CF4 File Offset: 0x002C2EF4
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

		// Token: 0x060052BA RID: 21178 RVA: 0x002C4DC8 File Offset: 0x002C2FC8
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

		// Token: 0x060052BB RID: 21179 RVA: 0x002C4E9C File Offset: 0x002C309C
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

		// Token: 0x060052BC RID: 21180 RVA: 0x002C4F30 File Offset: 0x002C3130
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

		// Token: 0x060052BD RID: 21181 RVA: 0x002C4F60 File Offset: 0x002C3160
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

		// Token: 0x060052BE RID: 21182 RVA: 0x002C508C File Offset: 0x002C328C
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

		// Token: 0x060052BF RID: 21183 RVA: 0x002C5178 File Offset: 0x002C3378
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

		// Token: 0x060052C0 RID: 21184 RVA: 0x002C5214 File Offset: 0x002C3414
		public void ClearAllEventSubscribers<T>() where T : class, IEvent
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				ConcurrentDictionary<OneOf<IEvent, string>, IEvent> concurrentDictionary;
				this._subscribers.TryRemove(typeof(T), out concurrentDictionary);
			}
		}

		// Token: 0x060052C1 RID: 21185 RVA: 0x002C528C File Offset: 0x002C348C
		public void ClearAllSubscribers()
		{
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._subscribers.Clear();
			}
		}

		// Token: 0x060052C2 RID: 21186 RVA: 0x002C52F4 File Offset: 0x002C34F4
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

		// Token: 0x060052C3 RID: 21187 RVA: 0x002C544C File Offset: 0x002C364C
		public void AddDispatcherEventService(IEventService eventService)
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				this._subscribedEventDispatchers.TryAdd(eventService, eventService);
			}
		}

		// Token: 0x060052C4 RID: 21188 RVA: 0x002C54B4 File Offset: 0x002C36B4
		public void RemoveDispatcherEventService(IEventService eventService)
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				IService.CheckDisposed(this);
				IEventService eventService2;
				this._subscribedEventDispatchers.TryRemove(eventService, out eventService2);
			}
		}

		// Token: 0x060052C5 RID: 21189 RVA: 0x002C5520 File Offset: 0x002C3720
		public string Patch(string identifier, string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(identifier, className, methodName, parameterTypes, patch, hookType);
		}

		// Token: 0x060052C6 RID: 21190 RVA: 0x002C5536 File Offset: 0x002C3736
		public string Patch(string identifier, string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(identifier, className, methodName, patch, hookType);
		}

		// Token: 0x060052C7 RID: 21191 RVA: 0x002C554A File Offset: 0x002C374A
		public string Patch(string className, string methodName, string[] parameterTypes, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(className, methodName, parameterTypes, patch, hookType);
		}

		// Token: 0x060052C8 RID: 21192 RVA: 0x002C555E File Offset: 0x002C375E
		public string Patch(string className, string methodName, LuaCsPatchFunc patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before)
		{
			return this._luaPatcher.Patch(className, methodName, patch, hookType);
		}

		// Token: 0x060052C9 RID: 21193 RVA: 0x002C5570 File Offset: 0x002C3770
		public bool RemovePatch(string identifier, string className, string methodName, string[] parameterTypes, ILuaCsHook.HookMethodType hookType)
		{
			return this._luaPatcher.RemovePatch(className, className, methodName, parameterTypes, hookType);
		}

		// Token: 0x060052CA RID: 21194 RVA: 0x002C5584 File Offset: 0x002C3784
		public bool RemovePatch(string identifier, string className, string methodName, ILuaCsHook.HookMethodType hookType)
		{
			return this._luaPatcher.RemovePatch(className, className, methodName, hookType);
		}

		// Token: 0x060052CB RID: 21195 RVA: 0x002C5596 File Offset: 0x002C3796
		public void HookMethod(string identifier, MethodBase method, LuaCsPatch patch, ILuaCsHook.HookMethodType hookType = ILuaCsHook.HookMethodType.Before, IAssemblyPlugin owner = null)
		{
			this._luaPatcher.HookMethod(identifier, method, patch, hookType, owner);
		}

		// Token: 0x060052CC RID: 21196 RVA: 0x002C55AA File Offset: 0x002C37AA
		public void HookMethod(string identifier, string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(identifier, className, methodName, parameterNames, patch, hookMethodType);
		}

		// Token: 0x060052CD RID: 21197 RVA: 0x002C55C0 File Offset: 0x002C37C0
		public void HookMethod(string identifier, string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(identifier, className, methodName, patch, hookMethodType);
		}

		// Token: 0x060052CE RID: 21198 RVA: 0x002C55D4 File Offset: 0x002C37D4
		public void HookMethod(string className, string methodName, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(className, methodName, patch, hookMethodType);
		}

		// Token: 0x060052CF RID: 21199 RVA: 0x002C55E6 File Offset: 0x002C37E6
		public void HookMethod(string className, string methodName, string[] parameterNames, LuaCsPatch patch, ILuaCsHook.HookMethodType hookMethodType = ILuaCsHook.HookMethodType.Before)
		{
			this._luaPatcher.HookMethod(className, methodName, parameterNames, patch, hookMethodType);
		}

		// Token: 0x04002BCA RID: 11210
		private readonly ILoggerService _loggerService;

		// Token: 0x04002BCB RID: 11211
		private readonly ILuaPatcher _luaPatcher;

		// Token: 0x04002BCC RID: 11212
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();

		// Token: 0x04002BCD RID: 11213
		private readonly ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<OneOf<IEvent, string>, IEvent>> _subscribers = new ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<OneOf<IEvent, string>, IEvent>>();

		// Token: 0x04002BCE RID: 11214
		[TupleElementNames(new string[]
		{
			"Event",
			"RunnerFactory"
		})]
		private readonly ConcurrentDictionary<EventService.TypeStringKey, ValueTuple<EventService.TypeStringKey, Func<LuaCsFunc, IEvent>>> _luaAliasEventFactory = new ConcurrentDictionary<EventService.TypeStringKey, ValueTuple<EventService.TypeStringKey, Func<LuaCsFunc, IEvent>>>();

		// Token: 0x04002BCF RID: 11215
		private readonly ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc>> _luaLegacyEventsSubscribers = new ConcurrentDictionary<EventService.TypeStringKey, ConcurrentDictionary<EventService.TypeStringKey, LuaCsFunc>>();

		// Token: 0x04002BD0 RID: 11216
		private readonly ConcurrentDictionary<IEventService, IEventService> _subscribedEventDispatchers = new ConcurrentDictionary<IEventService, IEventService>();

		// Token: 0x04002BD1 RID: 11217
		private int _isDisposed;

		// Token: 0x020012C5 RID: 4805
		private readonly struct TypeStringKey : IEqualityComparer<EventService.TypeStringKey>, IEquatable<EventService.TypeStringKey>
		{
			// Token: 0x17001D05 RID: 7429
			// (get) Token: 0x0600956D RID: 38253 RVA: 0x003D3897 File Offset: 0x003D1A97
			// (set) Token: 0x0600956E RID: 38254 RVA: 0x003D389F File Offset: 0x003D1A9F
			public Type Type { get; set; }

			// Token: 0x17001D06 RID: 7430
			// (get) Token: 0x0600956F RID: 38255 RVA: 0x003D38A8 File Offset: 0x003D1AA8
			// (set) Token: 0x06009570 RID: 38256 RVA: 0x003D38B0 File Offset: 0x003D1AB0
			public string TypeName { get; set; }

			// Token: 0x06009571 RID: 38257 RVA: 0x003D38B9 File Offset: 0x003D1AB9
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

			// Token: 0x06009572 RID: 38258 RVA: 0x003D38F3 File Offset: 0x003D1AF3
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

			// Token: 0x06009573 RID: 38259 RVA: 0x003D392E File Offset: 0x003D1B2E
			public bool Equals(EventService.TypeStringKey x, EventService.TypeStringKey y)
			{
				if (x.Type != null && y.Type != null)
				{
					return x.Type == y.Type;
				}
				return x.TypeName == y.TypeName;
			}

			// Token: 0x06009574 RID: 38260 RVA: 0x003D3969 File Offset: 0x003D1B69
			public int GetHashCode(EventService.TypeStringKey obj)
			{
				return obj.HashCode;
			}

			// Token: 0x06009575 RID: 38261 RVA: 0x003D3971 File Offset: 0x003D1B71
			public static implicit operator EventService.TypeStringKey(Type type)
			{
				return new EventService.TypeStringKey(type);
			}

			// Token: 0x06009576 RID: 38262 RVA: 0x003D3979 File Offset: 0x003D1B79
			public static implicit operator EventService.TypeStringKey(string typeName)
			{
				return new EventService.TypeStringKey(typeName);
			}

			// Token: 0x06009577 RID: 38263 RVA: 0x003D3984 File Offset: 0x003D1B84
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

			// Token: 0x06009578 RID: 38264 RVA: 0x003D39D0 File Offset: 0x003D1BD0
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

			// Token: 0x06009579 RID: 38265 RVA: 0x003D3A34 File Offset: 0x003D1C34
			[CompilerGenerated]
			public static bool operator !=(EventService.TypeStringKey left, EventService.TypeStringKey right)
			{
				return !(left == right);
			}

			// Token: 0x0600957A RID: 38266 RVA: 0x003D3A40 File Offset: 0x003D1C40
			[CompilerGenerated]
			public static bool operator ==(EventService.TypeStringKey left, EventService.TypeStringKey right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600957B RID: 38267 RVA: 0x003D3A4A File Offset: 0x003D1C4A
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Type>.Default.GetHashCode(this.<Type>k__BackingField) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.<TypeName>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.HashCode);
			}

			// Token: 0x0600957C RID: 38268 RVA: 0x003D3A8A File Offset: 0x003D1C8A
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is EventService.TypeStringKey && this.Equals((EventService.TypeStringKey)obj);
			}

			// Token: 0x0600957D RID: 38269 RVA: 0x003D3AA4 File Offset: 0x003D1CA4
			[CompilerGenerated]
			public bool Equals(EventService.TypeStringKey other)
			{
				return EqualityComparer<Type>.Default.Equals(this.<Type>k__BackingField, other.<Type>k__BackingField) && EqualityComparer<string>.Default.Equals(this.<TypeName>k__BackingField, other.<TypeName>k__BackingField) && EqualityComparer<int>.Default.Equals(this.HashCode, other.HashCode);
			}

			// Token: 0x04006044 RID: 24644
			public readonly int HashCode;
		}
	}
}
