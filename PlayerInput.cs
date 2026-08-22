using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000FC RID: 252
	public class PlayerInput
	{
		// Token: 0x170009BE RID: 2494
		// (get) Token: 0x060023CD RID: 9165 RVA: 0x00167545 File Offset: 0x00165745
		public static float MaxDoubleClickDistance
		{
			get
			{
				return Math.Max(15f * Math.Max((float)GameMain.GraphicsHeight / 1920f, (float)GameMain.GraphicsHeight / 1080f), 10f);
			}
		}

		// Token: 0x060023CE RID: 9166
		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int smIndex);

		// Token: 0x060023CF RID: 9167 RVA: 0x00167574 File Offset: 0x00165774
		public static bool MouseButtonsSwapped()
		{
			return PlayerInput.GetSystemMetrics(23) != 0;
		}

		// Token: 0x170009BF RID: 2495
		// (get) Token: 0x060023D0 RID: 9168 RVA: 0x00167580 File Offset: 0x00165780
		public static Vector2 MousePosition
		{
			get
			{
				return new Vector2((float)PlayerInput.mouseState.Position.X, (float)PlayerInput.mouseState.Position.Y);
			}
		}

		// Token: 0x170009C0 RID: 2496
		// (get) Token: 0x060023D1 RID: 9169 RVA: 0x001675A7 File Offset: 0x001657A7
		public static Vector2 LatestMousePosition
		{
			get
			{
				return new Vector2((float)PlayerInput.latestMouseState.Position.X, (float)PlayerInput.latestMouseState.Position.Y);
			}
		}

		// Token: 0x170009C1 RID: 2497
		// (get) Token: 0x060023D2 RID: 9170 RVA: 0x001675D0 File Offset: 0x001657D0
		public static bool MouseInsideWindow
		{
			get
			{
				return new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight).Contains(PlayerInput.MousePosition);
			}
		}

		// Token: 0x170009C2 RID: 2498
		// (get) Token: 0x060023D3 RID: 9171 RVA: 0x001675FB File Offset: 0x001657FB
		public static Vector2 MouseSpeed
		{
			get
			{
				if (!PlayerInput.AllowInput)
				{
					return Vector2.Zero;
				}
				return PlayerInput.MousePosition - new Vector2((float)PlayerInput.oldMouseState.X, (float)PlayerInput.oldMouseState.Y);
			}
		}

		// Token: 0x170009C3 RID: 2499
		// (get) Token: 0x060023D4 RID: 9172 RVA: 0x0016762F File Offset: 0x0016582F
		private static bool AllowInput
		{
			get
			{
				return GameMain.WindowActive && PlayerInput.allowInput;
			}
		}

		// Token: 0x170009C4 RID: 2500
		// (get) Token: 0x060023D5 RID: 9173 RVA: 0x0016763F File Offset: 0x0016583F
		// (set) Token: 0x060023D6 RID: 9174 RVA: 0x00167646 File Offset: 0x00165846
		public static Vector2 MouseSpeedPerSecond { get; private set; }

		// Token: 0x170009C5 RID: 2501
		// (get) Token: 0x060023D7 RID: 9175 RVA: 0x0016764E File Offset: 0x0016584E
		public static KeyboardState GetKeyboardState
		{
			get
			{
				return PlayerInput.keyboardState;
			}
		}

		// Token: 0x170009C6 RID: 2502
		// (get) Token: 0x060023D8 RID: 9176 RVA: 0x00167655 File Offset: 0x00165855
		public static KeyboardState GetOldKeyboardState
		{
			get
			{
				return PlayerInput.oldKeyboardState;
			}
		}

		// Token: 0x170009C7 RID: 2503
		// (get) Token: 0x060023D9 RID: 9177 RVA: 0x0016765C File Offset: 0x0016585C
		public static int ScrollWheelSpeed
		{
			get
			{
				if (!PlayerInput.AllowInput)
				{
					return 0;
				}
				return PlayerInput.mouseState.ScrollWheelValue - PlayerInput.oldMouseState.ScrollWheelValue;
			}
		}

		// Token: 0x060023DA RID: 9178 RVA: 0x0016767C File Offset: 0x0016587C
		public static bool PrimaryMouseButtonHeld()
		{
			return PlayerInput.AllowInput && PlayerInput.mouseState.LeftButton == ButtonState.Pressed;
		}

		// Token: 0x060023DB RID: 9179 RVA: 0x00167694 File Offset: 0x00165894
		public static bool PrimaryMouseButtonDown()
		{
			return PlayerInput.AllowInput && PlayerInput.oldMouseState.LeftButton == ButtonState.Released && PlayerInput.mouseState.LeftButton == ButtonState.Pressed;
		}

		// Token: 0x060023DC RID: 9180 RVA: 0x001676B8 File Offset: 0x001658B8
		public static bool PrimaryMouseButtonReleased()
		{
			return PlayerInput.AllowInput && PlayerInput.mouseState.LeftButton == ButtonState.Released;
		}

		// Token: 0x060023DD RID: 9181 RVA: 0x001676D0 File Offset: 0x001658D0
		public static bool PrimaryMouseButtonClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.oldMouseState.LeftButton == ButtonState.Pressed && PlayerInput.mouseState.LeftButton == ButtonState.Released;
		}

		// Token: 0x060023DE RID: 9182 RVA: 0x001676F5 File Offset: 0x001658F5
		public static bool SecondaryMouseButtonHeld()
		{
			return PlayerInput.AllowInput && PlayerInput.mouseState.RightButton == ButtonState.Pressed;
		}

		// Token: 0x060023DF RID: 9183 RVA: 0x0016770D File Offset: 0x0016590D
		public static bool SecondaryMouseButtonDown()
		{
			return PlayerInput.AllowInput && PlayerInput.oldMouseState.RightButton == ButtonState.Released && PlayerInput.mouseState.RightButton == ButtonState.Pressed;
		}

		// Token: 0x060023E0 RID: 9184 RVA: 0x00167731 File Offset: 0x00165931
		public static bool SecondaryMouseButtonReleased()
		{
			return PlayerInput.AllowInput && PlayerInput.mouseState.RightButton == ButtonState.Released;
		}

		// Token: 0x060023E1 RID: 9185 RVA: 0x00167749 File Offset: 0x00165949
		public static bool SecondaryMouseButtonClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.oldMouseState.RightButton == ButtonState.Pressed && PlayerInput.mouseState.RightButton == ButtonState.Released;
		}

		// Token: 0x060023E2 RID: 9186 RVA: 0x0016776E File Offset: 0x0016596E
		public static bool MidButtonClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.oldMouseState.MiddleButton == ButtonState.Pressed && PlayerInput.mouseState.MiddleButton == ButtonState.Released;
		}

		// Token: 0x060023E3 RID: 9187 RVA: 0x00167793 File Offset: 0x00165993
		public static bool MidButtonHeld()
		{
			return PlayerInput.AllowInput && PlayerInput.mouseState.MiddleButton == ButtonState.Pressed;
		}

		// Token: 0x060023E4 RID: 9188 RVA: 0x001677AB File Offset: 0x001659AB
		public static bool Mouse4ButtonClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.oldMouseState.XButton1 == ButtonState.Pressed && PlayerInput.mouseState.XButton1 == ButtonState.Released;
		}

		// Token: 0x060023E5 RID: 9189 RVA: 0x001677D0 File Offset: 0x001659D0
		public static bool Mouse4ButtonHeld()
		{
			return PlayerInput.AllowInput && PlayerInput.mouseState.XButton1 == ButtonState.Pressed;
		}

		// Token: 0x060023E6 RID: 9190 RVA: 0x001677E8 File Offset: 0x001659E8
		public static bool Mouse5ButtonClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.oldMouseState.XButton2 == ButtonState.Pressed && PlayerInput.mouseState.XButton2 == ButtonState.Released;
		}

		// Token: 0x060023E7 RID: 9191 RVA: 0x0016780D File Offset: 0x00165A0D
		public static bool Mouse5ButtonHeld()
		{
			return PlayerInput.AllowInput && PlayerInput.mouseState.XButton2 == ButtonState.Pressed;
		}

		// Token: 0x060023E8 RID: 9192 RVA: 0x00167825 File Offset: 0x00165A25
		public static bool MouseWheelUpClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.ScrollWheelSpeed > 0;
		}

		// Token: 0x060023E9 RID: 9193 RVA: 0x00167838 File Offset: 0x00165A38
		public static bool MouseWheelDownClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.ScrollWheelSpeed < 0;
		}

		// Token: 0x060023EA RID: 9194 RVA: 0x0016784B File Offset: 0x00165A4B
		public static bool DoubleClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.primaryDoubleClicked;
		}

		// Token: 0x060023EB RID: 9195 RVA: 0x0016785B File Offset: 0x00165A5B
		public static bool SecondaryDoubleClicked()
		{
			return PlayerInput.AllowInput && PlayerInput.secondaryDoubleClicked;
		}

		// Token: 0x060023EC RID: 9196 RVA: 0x0016786B File Offset: 0x00165A6B
		public static bool KeyHit(InputType inputType)
		{
			return PlayerInput.AllowInput && GameSettings.CurrentConfig.KeyMap.Bindings[inputType].IsHit();
		}

		// Token: 0x060023ED RID: 9197 RVA: 0x00167890 File Offset: 0x00165A90
		public static bool KeyDown(InputType inputType)
		{
			return PlayerInput.AllowInput && GameSettings.CurrentConfig.KeyMap.Bindings[inputType].IsDown();
		}

		// Token: 0x060023EE RID: 9198 RVA: 0x001678B5 File Offset: 0x00165AB5
		public static bool KeyUp(InputType inputType)
		{
			return PlayerInput.AllowInput && !GameSettings.CurrentConfig.KeyMap.Bindings[inputType].IsDown();
		}

		// Token: 0x060023EF RID: 9199 RVA: 0x001678DD File Offset: 0x00165ADD
		public static bool KeyHit(Keys button)
		{
			return PlayerInput.AllowInput && PlayerInput.oldKeyboardState.IsKeyUp(button) && PlayerInput.keyboardState.IsKeyDown(button);
		}

		// Token: 0x060023F0 RID: 9200 RVA: 0x00167900 File Offset: 0x00165B00
		public static bool InventoryKeyHit(int index)
		{
			return index != -1 && PlayerInput.AllowInput && GameSettings.CurrentConfig.InventoryKeyMap.Bindings[index].IsHit();
		}

		// Token: 0x060023F1 RID: 9201 RVA: 0x0016792B File Offset: 0x00165B2B
		public static bool KeyDown(Keys button)
		{
			return PlayerInput.AllowInput && PlayerInput.keyboardState.IsKeyDown(button);
		}

		// Token: 0x060023F2 RID: 9202 RVA: 0x00167941 File Offset: 0x00165B41
		public static bool KeyUp(Keys button)
		{
			return PlayerInput.AllowInput && PlayerInput.keyboardState.IsKeyUp(button);
		}

		// Token: 0x060023F3 RID: 9203 RVA: 0x00167957 File Offset: 0x00165B57
		public static bool IsShiftDown()
		{
			return PlayerInput.KeyDown(Keys.LeftShift) || PlayerInput.KeyDown(Keys.RightShift);
		}

		// Token: 0x060023F4 RID: 9204 RVA: 0x00167971 File Offset: 0x00165B71
		public static bool IsCtrlDown()
		{
			return PlayerInput.KeyDown(Keys.LeftControl) || PlayerInput.KeyDown(Keys.RightControl);
		}

		// Token: 0x060023F5 RID: 9205 RVA: 0x0016798B File Offset: 0x00165B8B
		public static bool IsAltDown()
		{
			return PlayerInput.KeyDown(Keys.LeftAlt) || PlayerInput.KeyDown(Keys.RightAlt);
		}

		// Token: 0x060023F6 RID: 9206 RVA: 0x001679A8 File Offset: 0x00165BA8
		public static void Update(double deltaTime)
		{
			PlayerInput.timeSincePrimaryClick += deltaTime;
			PlayerInput.timeSinceSecondaryClick += deltaTime;
			if (!GameMain.WindowActive)
			{
				PlayerInput.wasWindowActive = false;
				return;
			}
			if (!PlayerInput.wasWindowActive)
			{
				PlayerInput.wasWindowActive = true;
				PlayerInput.allowInput = false;
			}
			else
			{
				PlayerInput.allowInput = true;
			}
			PlayerInput.oldMouseState = PlayerInput.mouseState;
			PlayerInput.mouseState = PlayerInput.latestMouseState;
			PlayerInput.UpdateVariable();
			PlayerInput.oldKeyboardState = PlayerInput.keyboardState;
			PlayerInput.keyboardState = Keyboard.GetState();
			PlayerInput.MouseSpeedPerSecond = PlayerInput.MouseSpeed / (float)deltaTime;
			PlayerInput.primaryDoubleClicked = false;
			if (PlayerInput.PrimaryMouseButtonClicked())
			{
				PlayerInput.primaryDoubleClicked = PlayerInput.<Update>g__UpdateDoubleClicking|70_0(ref PlayerInput.lastPrimaryClickPosition, ref PlayerInput.timeSincePrimaryClick);
			}
			if (PlayerInput.PrimaryMouseButtonDown())
			{
				PlayerInput.lastPrimaryClickPosition = PlayerInput.mouseState.Position;
			}
			PlayerInput.secondaryDoubleClicked = false;
			if (PlayerInput.SecondaryMouseButtonClicked())
			{
				PlayerInput.secondaryDoubleClicked = PlayerInput.<Update>g__UpdateDoubleClicking|70_0(ref PlayerInput.lastSecondaryClickPosition, ref PlayerInput.timeSinceSecondaryClick);
			}
			if (PlayerInput.SecondaryMouseButtonDown())
			{
				PlayerInput.lastSecondaryClickPosition = PlayerInput.mouseState.Position;
			}
		}

		// Token: 0x060023F7 RID: 9207 RVA: 0x00167AA2 File Offset: 0x00165CA2
		public static void UpdateVariable()
		{
			PlayerInput.latestMouseState = Mouse.GetState();
		}

		// Token: 0x060023FA RID: 9210 RVA: 0x00167B78 File Offset: 0x00165D78
		[CompilerGenerated]
		internal static bool <Update>g__UpdateDoubleClicking|70_0(ref Point lastClickPosition, ref double timeSinceClick)
		{
			bool doubleClicked = false;
			float dist = (PlayerInput.mouseState.Position - lastClickPosition).ToVector2().Length();
			if (timeSinceClick < 0.4000000059604645 && dist < PlayerInput.MaxDoubleClickDistance)
			{
				doubleClicked = true;
				timeSinceClick = 0.4000000059604645;
			}
			else if (timeSinceClick < 0.4000000059604645)
			{
				lastClickPosition = PlayerInput.mouseState.Position;
			}
			if (!doubleClicked && dist < PlayerInput.MaxDoubleClickDistance)
			{
				timeSinceClick = 0.0;
			}
			return doubleClicked;
		}

		// Token: 0x040011DE RID: 4574
		private static MouseState mouseState;

		// Token: 0x040011DF RID: 4575
		private static MouseState oldMouseState;

		// Token: 0x040011E0 RID: 4576
		private static MouseState latestMouseState;

		// Token: 0x040011E1 RID: 4577
		private static KeyboardState keyboardState;

		// Token: 0x040011E2 RID: 4578
		private static KeyboardState oldKeyboardState;

		// Token: 0x040011E3 RID: 4579
		private static double timeSincePrimaryClick;

		// Token: 0x040011E4 RID: 4580
		private static Point lastPrimaryClickPosition;

		// Token: 0x040011E5 RID: 4581
		private static double timeSinceSecondaryClick;

		// Token: 0x040011E6 RID: 4582
		private static Point lastSecondaryClickPosition;

		// Token: 0x040011E7 RID: 4583
		private const float DoubleClickDelay = 0.4f;

		// Token: 0x040011E8 RID: 4584
		private static bool primaryDoubleClicked;

		// Token: 0x040011E9 RID: 4585
		private static bool secondaryDoubleClicked;

		// Token: 0x040011EA RID: 4586
		private static bool allowInput;

		// Token: 0x040011EB RID: 4587
		private static bool wasWindowActive;

		// Token: 0x040011EC RID: 4588
		public static readonly List<Keys> NumberKeys = new List<Keys>
		{
			Keys.D0,
			Keys.D1,
			Keys.D2,
			Keys.D3,
			Keys.D4,
			Keys.D5,
			Keys.D6,
			Keys.D7,
			Keys.D8,
			Keys.D9
		};

		// Token: 0x040011ED RID: 4589
		private const int SM_SWAPBUTTON = 23;

		// Token: 0x040011EE RID: 4590
		public static readonly LocalizedString PrimaryMouseLabel = TextManager.Get("Input." + ((!PlayerInput.MouseButtonsSwapped()) ? "Left" : "Right") + "Mouse");

		// Token: 0x040011EF RID: 4591
		public static readonly LocalizedString SecondaryMouseLabel = TextManager.Get("Input." + ((!PlayerInput.MouseButtonsSwapped()) ? "Right" : "Left") + "Mouse");
	}
}
