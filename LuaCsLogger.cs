using System;
using Barotrauma.LuaCs;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000CB RID: 203
	public class LuaCsLogger
	{
		// Token: 0x06001ADE RID: 6878 RVA: 0x00109F10 File Offset: 0x00108110
		private static void CreateOverlay(string message)
		{
			LuaCsLogger.overlayFrame = new GUIFrame(new RectTransform(new Vector2(0.4f, 0.03f), null, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, new Color?(new Color(50, 50, 50, 100)))
			{
				CanBeFocused = false
			};
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.8f), LuaCsLogger.overlayFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			LuaCsLogger.textBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), message, null, null, Alignment.Left, false, "", null);
			LuaCsLogger.overlayFrame.RectTransform.MinSize = new Point((int)((double)LuaCsLogger.textBlock.TextSize.X * 1.2), 0);
			layout.Recalculate();
		}

		// Token: 0x06001ADF RID: 6879 RVA: 0x0010A04D File Offset: 0x0010824D
		public static void AddToGUIUpdateList()
		{
			if (LuaCsLogger.overlayFrame != null && Timing.TotalTime <= LuaCsLogger.showTimer)
			{
				LuaCsLogger.overlayFrame.AddToGUIUpdateList(false, 0);
			}
		}

		// Token: 0x06001AE0 RID: 6880 RVA: 0x0010A070 File Offset: 0x00108270
		public static void ShowErrorOverlay(string message, float time = 5f, float duration = 1.5f)
		{
			if (Timing.TotalTime <= LuaCsLogger.showTimer)
			{
				return;
			}
			LuaCsLogger.CreateOverlay(message);
			LuaCsLogger.overlayFrame.Flash(new Color?(Color.Red), duration, true, false, null);
			LuaCsLogger.showTimer = Timing.TotalTime + (double)time;
		}

		// Token: 0x06001AE1 RID: 6881 RVA: 0x0010A0BD File Offset: 0x001082BD
		public static void HandleException(Exception ex, LuaCsMessageOrigin origin)
		{
			LuaCsSetup.Instance.Logger.HandleException(ex, null);
		}

		// Token: 0x06001AE2 RID: 6882 RVA: 0x0010A0D0 File Offset: 0x001082D0
		public static void LogError(string message, LuaCsMessageOrigin origin)
		{
			LuaCsSetup.Instance.Logger.LogError(message);
		}

		// Token: 0x06001AE3 RID: 6883 RVA: 0x0010A0E2 File Offset: 0x001082E2
		public static void LogError(string message)
		{
			LuaCsSetup.Instance.Logger.LogError(message);
		}

		// Token: 0x06001AE4 RID: 6884 RVA: 0x0010A0F4 File Offset: 0x001082F4
		public static void LogMessage(string message, Color? serverColor = null, Color? clientColor = null)
		{
			LuaCsSetup.Instance.Logger.LogMessage(message, serverColor, clientColor);
		}

		// Token: 0x06001AE5 RID: 6885 RVA: 0x0010A108 File Offset: 0x00108308
		public static void Log(string message, Color? color = null, ServerLog.MessageType messageType = ServerLog.MessageType.ServerMessage)
		{
			LuaCsSetup.Instance.Logger.Log(message, color, messageType);
		}

		// Token: 0x04000DBD RID: 3517
		private static GUIFrame overlayFrame;

		// Token: 0x04000DBE RID: 3518
		private static GUITextBlock textBlock;

		// Token: 0x04000DBF RID: 3519
		private static double showTimer;
	}
}
