using System;
using System.Runtime.CompilerServices;
using Barotrauma;
using Barotrauma.LuaCs;
using Barotrauma.LuaCs.Events;
using FluentResults;
using HarmonyLib;
using Microsoft.Xna.Framework;

// Token: 0x02000005 RID: 5
[HarmonyPatch]
internal class MainMenuPatch : ISystem, IReusableService, IService, IDisposable, IEventScreenSelected, IEvent<IEventScreenSelected>, IEvent
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x0600000F RID: 15 RVA: 0x0000227A File Offset: 0x0000047A
	// (set) Token: 0x06000010 RID: 16 RVA: 0x00002282 File Offset: 0x00000482
	public bool IsDisposed { get; private set; }

	// Token: 0x06000011 RID: 17 RVA: 0x0000228C File Offset: 0x0000048C
	public MainMenuPatch(IEventService eventService)
	{
		this._eventService = eventService;
		this.RegisterEvents();
		MainMenuScreen mainMenuScreen = Screen.Selected as MainMenuScreen;
		if (mainMenuScreen != null)
		{
			this.AddToMainMenu(mainMenuScreen);
		}
	}

	// Token: 0x06000012 RID: 18 RVA: 0x000022C4 File Offset: 0x000004C4
	public void OnScreenSelected(Screen screen)
	{
		MainMenuScreen mainMenuScreen = screen as MainMenuScreen;
		if (mainMenuScreen != null)
		{
			this.AddToMainMenu(mainMenuScreen);
		}
	}

	// Token: 0x06000013 RID: 19 RVA: 0x000022E4 File Offset: 0x000004E4
	private void AddToMainMenu(MainMenuScreen screen)
	{
		if (this.mainMenuUIAdded)
		{
			return;
		}
		GUITextBlock textBlock = new GUITextBlock(new RectTransform(new Point(300, 30), screen.Frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
		{
			AbsoluteOffset = new Point(10, 10)
		}, "", new Color?(Color.Red), null, Alignment.TopLeft, false, "", null)
		{
			IgnoreLayoutGroups = false
		};
		textBlock.OnAddedToGUIUpdateList = delegate(GUIComponent component)
		{
			string mode = LuaCsSetup.Instance.CsRunPolicyValue;
			if (mode == "Prompt")
			{
				string sessionState = LuaCsSetup.Instance.IsCsEnabledForSession ? "yes" : "no";
				mode = "enabled (prompt mode, allowed for this session: " + sessionState + ")";
			}
			else if (mode == "Enabled")
			{
				mode = "always enabled";
			}
			else
			{
				mode = "disabled";
			}
			GUITextBlock textBlock = textBlock;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(105, 2);
			defaultInterpolatedStringHandler.AppendLiteral("LuaCsForBarotrauma active (revision ");
			defaultInterpolatedStringHandler.AppendFormatted(AssemblyInfo.GitRevision);
			defaultInterpolatedStringHandler.AppendLiteral("), C# is currently ");
			defaultInterpolatedStringHandler.AppendFormatted(mode);
			defaultInterpolatedStringHandler.AppendLiteral("\nNew settings available in the game settings menu.");
			textBlock.Text = defaultInterpolatedStringHandler.ToStringAndClear();
		};
		this.mainMenuUIAdded = true;
	}

	// Token: 0x06000014 RID: 20 RVA: 0x0000238D File Offset: 0x0000058D
	private void RegisterEvents()
	{
		this._eventService.Subscribe<IEventScreenSelected>(this);
	}

	// Token: 0x06000015 RID: 21 RVA: 0x0000239C File Offset: 0x0000059C
	public void Dispose()
	{
		this._eventService.Unsubscribe<IEventScreenSelected>(this);
		this.IsDisposed = true;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x000023B1 File Offset: 0x000005B1
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
