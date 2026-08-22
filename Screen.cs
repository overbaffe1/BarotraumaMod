using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200011A RID: 282
	internal abstract class Screen
	{
		// Token: 0x06002689 RID: 9865 RVA: 0x0019929C File Offset: 0x0019749C
		protected Screen()
		{
			this.Frame = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
		}

		// Token: 0x0600268A RID: 9866 RVA: 0x001992F7 File Offset: 0x001974F7
		public virtual void AddToGUIUpdateList()
		{
			this.Frame.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x0600268B RID: 9867 RVA: 0x00199306 File Offset: 0x00197506
		public virtual void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
		}

		// Token: 0x0600268C RID: 9868 RVA: 0x00199308 File Offset: 0x00197508
		public void ColorFade(Color from, Color to, float duration)
		{
			if (duration <= 0f)
			{
				return;
			}
			CoroutineManager.StartCoroutine(this.UpdateColorFade(from, to, duration), "");
		}

		// Token: 0x0600268D RID: 9869 RVA: 0x00199327 File Offset: 0x00197527
		private IEnumerable<CoroutineStatus> UpdateColorFade(Color from, Color to, float duration)
		{
			Screen.<UpdateColorFade>d__5 <UpdateColorFade>d__ = new Screen.<UpdateColorFade>d__5(-2);
			<UpdateColorFade>d__.<>4__this = this;
			<UpdateColorFade>d__.<>3__from = from;
			<UpdateColorFade>d__.<>3__to = to;
			<UpdateColorFade>d__.<>3__duration = duration;
			return <UpdateColorFade>d__;
		}

		// Token: 0x0600268E RID: 9870 RVA: 0x0019934C File Offset: 0x0019754C
		public virtual void OnFileDropped(string filePath, string extension)
		{
		}

		// Token: 0x0600268F RID: 9871 RVA: 0x0019934E File Offset: 0x0019754E
		public virtual void Release()
		{
			this.Frame.RectTransform.Parent = null;
		}

		// Token: 0x17000A32 RID: 2610
		// (get) Token: 0x06002690 RID: 9872 RVA: 0x00199361 File Offset: 0x00197561
		// (set) Token: 0x06002691 RID: 9873 RVA: 0x00199368 File Offset: 0x00197568
		public static Screen Selected { get; private set; }

		// Token: 0x06002692 RID: 9874 RVA: 0x00199370 File Offset: 0x00197570
		public static void SelectNull()
		{
			Screen.Selected = null;
		}

		// Token: 0x06002693 RID: 9875 RVA: 0x00199378 File Offset: 0x00197578
		public virtual void Deselect()
		{
		}

		// Token: 0x06002694 RID: 9876 RVA: 0x0019937C File Offset: 0x0019757C
		public virtual void Select()
		{
			if (Screen.Selected != null && Screen.Selected != this)
			{
				Screen.Selected.Deselect();
				GameMain.ParticleManager.ClearParticles();
				GUIContextMenu.CurrentContextMenu = null;
				GUI.ClearCursorWait();
				if (GUI.KeyboardDispatcher.Subscriber != DebugConsole.TextBox)
				{
					GUI.KeyboardDispatcher.Subscriber = null;
					GUI.ScreenChanged = true;
				}
				SubmarinePreview.Close();
				if (this == GameMain.MainMenuScreen || this == GameMain.NetLobbyScreen)
				{
					GUI.DisableSavingIndicatorDelayed(3f);
				}
				GameMain.ResetIMEWorkaround();
			}
			GUI.SettingsMenuOpen = false;
			Screen.Selected = this;
		}

		// Token: 0x17000A33 RID: 2611
		// (get) Token: 0x06002695 RID: 9877 RVA: 0x00199409 File Offset: 0x00197609
		public virtual Camera Cam
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000A34 RID: 2612
		// (get) Token: 0x06002696 RID: 9878 RVA: 0x0019940C File Offset: 0x0019760C
		public virtual bool IsEditor
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002697 RID: 9879 RVA: 0x0019940F File Offset: 0x0019760F
		public virtual void Update(double deltaTime)
		{
		}

		// Token: 0x0400136E RID: 4974
		public readonly GUIFrame Frame;
	}
}
