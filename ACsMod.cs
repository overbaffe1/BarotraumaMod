using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Events;

namespace Barotrauma
{
	// Token: 0x020002F7 RID: 759
	[Obsolete("Make your class implement IAssemblyPlugin instead.")]
	public abstract class ACsMod : IAssemblyPlugin, IDisposable, IEventPluginPreInitialize, IEvent<IEventPluginPreInitialize>, IEvent, IEventPluginInitialize, IEvent<IEventPluginInitialize>, IEventPluginLoadCompleted, IEvent<IEventPluginLoadCompleted>
	{
		// Token: 0x17001055 RID: 4181
		// (get) Token: 0x06003DD8 RID: 15832 RVA: 0x002315D5 File Offset: 0x0022F7D5
		[Obsolete("$This does nothing. Stop using it!")]
		public static List<ACsMod> LoadedMods
		{
			get
			{
				return ACsMod.mods;
			}
		}

		// Token: 0x06003DD9 RID: 15833 RVA: 0x002315DC File Offset: 0x0022F7DC
		[Obsolete("$This does nothing. Stop using it!")]
		public static string GetStoreFolder<T>() where T : ACsMod
		{
			if (!Directory.Exists("LocalMods/.modstore"))
			{
				Directory.CreateDirectory("LocalMods/.modstore");
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted("LocalMods/.modstore");
			defaultInterpolatedStringHandler.AppendLiteral("/");
			defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(T));
			string modFolder = defaultInterpolatedStringHandler.ToStringAndClear();
			if (!Directory.Exists(modFolder))
			{
				Directory.CreateDirectory(modFolder);
			}
			return modFolder;
		}

		// Token: 0x17001056 RID: 4182
		// (get) Token: 0x06003DDA RID: 15834 RVA: 0x0023164A File Offset: 0x0022F84A
		// (set) Token: 0x06003DDB RID: 15835 RVA: 0x00231652 File Offset: 0x0022F852
		public bool IsDisposed { get; private set; }

		// Token: 0x06003DDC RID: 15836 RVA: 0x0023165B File Offset: 0x0022F85B
		public virtual void Initialize()
		{
		}

		// Token: 0x06003DDD RID: 15837 RVA: 0x0023165D File Offset: 0x0022F85D
		public virtual void OnLoadCompleted()
		{
		}

		// Token: 0x06003DDE RID: 15838 RVA: 0x0023165F File Offset: 0x0022F85F
		public void PreInitPatching()
		{
		}

		// Token: 0x06003DDF RID: 15839 RVA: 0x00231664 File Offset: 0x0022F864
		public virtual void Dispose()
		{
			try
			{
				this.Stop();
			}
			catch (Exception e)
			{
				LuaCsSetup.Instance.Logger.HandleException(e, null);
			}
			this.IsDisposed = true;
		}

		// Token: 0x06003DE0 RID: 15840
		public abstract void Stop();

		// Token: 0x0400206E RID: 8302
		private static List<ACsMod> mods = new List<ACsMod>();

		// Token: 0x0400206F RID: 8303
		private const string MOD_STORE = "LocalMods/.modstore";
	}
}
