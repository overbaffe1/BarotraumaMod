using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Events;

namespace Barotrauma
{
	// Token: 0x0200020E RID: 526
	[Obsolete("Make your class implement IAssemblyPlugin instead.")]
	public abstract class ACsMod : IAssemblyPlugin, IDisposable, IEventPluginPreInitialize, IEvent<IEventPluginPreInitialize>, IEvent, IEventPluginInitialize, IEvent<IEventPluginInitialize>, IEventPluginLoadCompleted, IEvent<IEventPluginLoadCompleted>
	{
		// Token: 0x17000ABD RID: 2749
		// (get) Token: 0x06002500 RID: 9472 RVA: 0x000F4149 File Offset: 0x000F2349
		[Obsolete("$This does nothing. Stop using it!")]
		public static List<ACsMod> LoadedMods
		{
			get
			{
				return ACsMod.mods;
			}
		}

		// Token: 0x06002501 RID: 9473 RVA: 0x000F4150 File Offset: 0x000F2350
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

		// Token: 0x17000ABE RID: 2750
		// (get) Token: 0x06002502 RID: 9474 RVA: 0x000F41BE File Offset: 0x000F23BE
		// (set) Token: 0x06002503 RID: 9475 RVA: 0x000F41C6 File Offset: 0x000F23C6
		public bool IsDisposed { get; private set; }

		// Token: 0x06002504 RID: 9476 RVA: 0x000F41CF File Offset: 0x000F23CF
		public virtual void Initialize()
		{
		}

		// Token: 0x06002505 RID: 9477 RVA: 0x000F41D1 File Offset: 0x000F23D1
		public virtual void OnLoadCompleted()
		{
		}

		// Token: 0x06002506 RID: 9478 RVA: 0x000F41D3 File Offset: 0x000F23D3
		public void PreInitPatching()
		{
		}

		// Token: 0x06002507 RID: 9479 RVA: 0x000F41D8 File Offset: 0x000F23D8
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

		// Token: 0x06002508 RID: 9480
		public abstract void Stop();

		// Token: 0x04001242 RID: 4674
		private static List<ACsMod> mods = new List<ACsMod>();

		// Token: 0x04001243 RID: 4675
		private const string MOD_STORE = "LocalMods/.modstore";
	}
}
