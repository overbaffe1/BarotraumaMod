using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000FB RID: 251
	public class KeyOrMouse
	{
		// Token: 0x170009BC RID: 2492
		// (get) Token: 0x060023B6 RID: 9142 RVA: 0x001671A6 File Offset: 0x001653A6
		public LocalizedString Name
		{
			get
			{
				if (this.name == null)
				{
					this.name = this.GetName();
				}
				return this.name;
			}
		}

		// Token: 0x170009BD RID: 2493
		// (get) Token: 0x060023B7 RID: 9143 RVA: 0x001671C8 File Offset: 0x001653C8
		// (set) Token: 0x060023B8 RID: 9144 RVA: 0x001671D0 File Offset: 0x001653D0
		public MouseButton MouseButton { get; private set; }

		// Token: 0x060023B9 RID: 9145 RVA: 0x001671D9 File Offset: 0x001653D9
		public static implicit operator KeyOrMouse(Keys key)
		{
			return new KeyOrMouse(key);
		}

		// Token: 0x060023BA RID: 9146 RVA: 0x001671E1 File Offset: 0x001653E1
		public static implicit operator KeyOrMouse(MouseButton mouseButton)
		{
			return new KeyOrMouse(mouseButton);
		}

		// Token: 0x060023BB RID: 9147 RVA: 0x001671E9 File Offset: 0x001653E9
		public KeyOrMouse(Keys keyBinding)
		{
			this.Key = keyBinding;
			this.MouseButton = MouseButton.None;
		}

		// Token: 0x060023BC RID: 9148 RVA: 0x001671FF File Offset: 0x001653FF
		public KeyOrMouse(MouseButton mouseButton)
		{
			this.Key = Keys.None;
			this.MouseButton = mouseButton;
		}

		// Token: 0x060023BD RID: 9149 RVA: 0x00167218 File Offset: 0x00165418
		public bool IsDown()
		{
			switch (this.MouseButton)
			{
			case MouseButton.None:
				return this.Key != Keys.None && PlayerInput.KeyDown(this.Key);
			case MouseButton.PrimaryMouse:
				return PlayerInput.PrimaryMouseButtonHeld();
			case MouseButton.SecondaryMouse:
				return PlayerInput.SecondaryMouseButtonHeld();
			case MouseButton.MiddleMouse:
				return PlayerInput.MidButtonHeld();
			case MouseButton.MouseButton4:
				return PlayerInput.Mouse4ButtonHeld();
			case MouseButton.MouseButton5:
				return PlayerInput.Mouse5ButtonHeld();
			case MouseButton.MouseWheelUp:
				return PlayerInput.MouseWheelUpClicked();
			case MouseButton.MouseWheelDown:
				return PlayerInput.MouseWheelDownClicked();
			default:
				return false;
			}
		}

		// Token: 0x060023BE RID: 9150 RVA: 0x00167298 File Offset: 0x00165498
		public bool IsHit()
		{
			switch (this.MouseButton)
			{
			case MouseButton.None:
				return this.Key != Keys.None && PlayerInput.KeyHit(this.Key);
			case MouseButton.PrimaryMouse:
				return PlayerInput.PrimaryMouseButtonClicked();
			case MouseButton.SecondaryMouse:
				return PlayerInput.SecondaryMouseButtonClicked();
			case MouseButton.MiddleMouse:
				return PlayerInput.MidButtonClicked();
			case MouseButton.MouseButton4:
				return PlayerInput.Mouse4ButtonClicked();
			case MouseButton.MouseButton5:
				return PlayerInput.Mouse5ButtonClicked();
			case MouseButton.MouseWheelUp:
				return PlayerInput.MouseWheelUpClicked();
			case MouseButton.MouseWheelDown:
				return PlayerInput.MouseWheelDownClicked();
			default:
				return false;
			}
		}

		// Token: 0x060023BF RID: 9151 RVA: 0x00167318 File Offset: 0x00165518
		public override bool Equals(object obj)
		{
			KeyOrMouse keyOrMouse = obj as KeyOrMouse;
			return keyOrMouse != null && this == keyOrMouse;
		}

		// Token: 0x060023C0 RID: 9152 RVA: 0x00167338 File Offset: 0x00165538
		public static bool operator ==(KeyOrMouse a, KeyOrMouse b)
		{
			if (a == null)
			{
				return b == null;
			}
			if (a.MouseButton != MouseButton.None)
			{
				return b != null && a.MouseButton == b.MouseButton;
			}
			return b != null && a.Key.Equals(b.Key);
		}

		// Token: 0x060023C1 RID: 9153 RVA: 0x0016738E File Offset: 0x0016558E
		public static bool operator !=(KeyOrMouse a, KeyOrMouse b)
		{
			return !(a == b);
		}

		// Token: 0x060023C2 RID: 9154 RVA: 0x0016739A File Offset: 0x0016559A
		public static bool operator ==(KeyOrMouse keyOrMouse, Keys key)
		{
			return keyOrMouse.MouseButton == MouseButton.None && keyOrMouse.Key == key;
		}

		// Token: 0x060023C3 RID: 9155 RVA: 0x001673B0 File Offset: 0x001655B0
		public static bool operator !=(KeyOrMouse keyOrMouse, Keys key)
		{
			return !(keyOrMouse == key);
		}

		// Token: 0x060023C4 RID: 9156 RVA: 0x001673BC File Offset: 0x001655BC
		public static bool operator ==(Keys key, KeyOrMouse keyOrMouse)
		{
			return keyOrMouse == key;
		}

		// Token: 0x060023C5 RID: 9157 RVA: 0x001673C5 File Offset: 0x001655C5
		public static bool operator !=(Keys key, KeyOrMouse keyOrMouse)
		{
			return keyOrMouse != key;
		}

		// Token: 0x060023C6 RID: 9158 RVA: 0x001673CE File Offset: 0x001655CE
		public static bool operator ==(KeyOrMouse keyOrMouse, MouseButton mb)
		{
			return keyOrMouse.MouseButton == mb && keyOrMouse.Key == Keys.None;
		}

		// Token: 0x060023C7 RID: 9159 RVA: 0x001673E4 File Offset: 0x001655E4
		public static bool operator !=(KeyOrMouse keyOrMouse, MouseButton mb)
		{
			return !(keyOrMouse == mb);
		}

		// Token: 0x060023C8 RID: 9160 RVA: 0x001673F0 File Offset: 0x001655F0
		public static bool operator ==(MouseButton mb, KeyOrMouse keyOrMouse)
		{
			return keyOrMouse == mb;
		}

		// Token: 0x060023C9 RID: 9161 RVA: 0x001673F9 File Offset: 0x001655F9
		public static bool operator !=(MouseButton mb, KeyOrMouse keyOrMouse)
		{
			return keyOrMouse != mb;
		}

		// Token: 0x060023CA RID: 9162 RVA: 0x00167404 File Offset: 0x00165604
		public override string ToString()
		{
			MouseButton mouseButton = this.MouseButton;
			if (mouseButton == MouseButton.None)
			{
				return this.Key.ToString();
			}
			return this.MouseButton.ToString();
		}

		// Token: 0x060023CB RID: 9163 RVA: 0x00167448 File Offset: 0x00165648
		public override int GetHashCode()
		{
			int hashCode = int.MinValue;
			hashCode = hashCode * -1521134295 + this.Key.GetHashCode();
			return hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(new int?((int)this.MouseButton));
		}

		// Token: 0x060023CC RID: 9164 RVA: 0x00167498 File Offset: 0x00165698
		public LocalizedString GetName()
		{
			if (PlayerInput.NumberKeys.Contains(this.Key))
			{
				return this.Key.ToString().Substring(1, 1);
			}
			if (this.MouseButton == MouseButton.None)
			{
				return this.Key.ToString();
			}
			MouseButton mouseButton = this.MouseButton;
			if (mouseButton == MouseButton.PrimaryMouse)
			{
				return PlayerInput.PrimaryMouseLabel;
			}
			if (mouseButton != MouseButton.SecondaryMouse)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Input.");
				defaultInterpolatedStringHandler.AppendFormatted<MouseButton>(this.MouseButton);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return PlayerInput.SecondaryMouseLabel;
		}

		// Token: 0x040011DB RID: 4571
		public readonly Keys Key;

		// Token: 0x040011DC RID: 4572
		private LocalizedString name;
	}
}
