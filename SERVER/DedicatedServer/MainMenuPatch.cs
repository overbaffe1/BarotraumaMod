using System;
using Barotrauma;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Events;
using FluentResults;
using HarmonyLib;

// Token: 0x02000005 RID: 5
[HarmonyPatch]
internal class MainMenuPatch : ISystem, IReusableService, IService, IDisposable, IEventScreenSelected, IEvent<IEventScreenSelected>, IEvent
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x0600000F RID: 15 RVA: 0x0000227A File Offset: 0x0000047A
	// (set) Token: 0x06000010 RID: 16 RVA: 0x00002282 File Offset: 0x00000482
	public bool IsDisposed { get; private set; }

	// Token: 0x06000011 RID: 17 RVA: 0x0000228B File Offset: 0x0000048B
	public MainMenuPatch(IEventService eventService)
	{
		this._eventService = eventService;
		this.RegisterEvents();
	}

	// Token: 0x06000012 RID: 18 RVA: 0x000022A0 File Offset: 0x000004A0
	public void OnScreenSelected(Screen screen)
	{
	}

	// Token: 0x06000013 RID: 19 RVA: 0x000022A2 File Offset: 0x000004A2
	private void RegisterEvents()
	{
		this._eventService.Subscribe<IEventScreenSelected>(this);
	}

	// Token: 0x06000014 RID: 20 RVA: 0x000022B1 File Offset: 0x000004B1
	public void Dispose()
	{
		this._eventService.Unsubscribe<IEventScreenSelected>(this);
		this.IsDisposed = true;
	}

	// Token: 0x06000015 RID: 21 RVA: 0x000022C6 File Offset: 0x000004C6
	public FluentResults.Result Reset()
	{
		this.RegisterEvents();
		return FluentResults.Result.Ok();
	}

	// Token: 0x04000006 RID: 6
	private readonly IEventService _eventService;

	// Token: 0x04000007 RID: 7
	private bool mainMenuUIAdded;
}
