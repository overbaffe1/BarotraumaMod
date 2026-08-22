using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000104 RID: 260
	internal class CreditsPlayer : GUIComponent
	{
		// Token: 0x170009D6 RID: 2518
		// (get) Token: 0x0600246B RID: 9323 RVA: 0x00170FD9 File Offset: 0x0016F1D9
		public bool Finished
		{
			get
			{
				return this.listBox.BarScroll >= 1f;
			}
		}

		// Token: 0x170009D7 RID: 2519
		// (get) Token: 0x0600246C RID: 9324 RVA: 0x00170FF0 File Offset: 0x0016F1F0
		// (set) Token: 0x0600246D RID: 9325 RVA: 0x00170FFD File Offset: 0x0016F1FD
		public bool ScrollBarEnabled
		{
			get
			{
				return this.listBox.ScrollBarEnabled;
			}
			set
			{
				this.listBox.ScrollBarEnabled = value;
			}
		}

		// Token: 0x170009D8 RID: 2520
		// (get) Token: 0x0600246E RID: 9326 RVA: 0x0017100B File Offset: 0x0016F20B
		// (set) Token: 0x0600246F RID: 9327 RVA: 0x00171018 File Offset: 0x0016F218
		public bool AllowMouseWheelScroll
		{
			get
			{
				return this.listBox.AllowMouseWheelScroll;
			}
			set
			{
				this.listBox.AllowMouseWheelScroll = value;
			}
		}

		// Token: 0x170009D9 RID: 2521
		// (get) Token: 0x06002470 RID: 9328 RVA: 0x00171026 File Offset: 0x0016F226
		// (set) Token: 0x06002471 RID: 9329 RVA: 0x00171033 File Offset: 0x0016F233
		public float Scroll
		{
			get
			{
				return this.listBox.BarScroll;
			}
			set
			{
				this.listBox.BarScroll = value;
			}
		}

		// Token: 0x06002472 RID: 9330 RVA: 0x00171044 File Offset: 0x0016F244
		public CreditsPlayer(RectTransform rectT, string configFile) : base(null, rectT)
		{
			GameMain.Instance.ResolutionChanged += delegate()
			{
				this.ClearChildren();
				this.Load();
			};
			XDocument doc = XMLExtensions.TryLoadXml(configFile);
			if (doc == null)
			{
				return;
			}
			this.configElement = doc.Root.FromPackage(ContentPackageManager.VanillaCorePackage);
			this.Load();
			Vector2 relativeSize = new Vector2(0.1f);
			RectTransform rectTransform = base.RectTransform;
			Anchor anchor = Anchor.BottomRight;
			Point? maxSize = new Point?(new Point(GUI.IntScale(300f), GUI.IntScale(50f)));
			this.CloseButton = new GUIButton(new RectTransform(relativeSize, rectTransform, anchor, null, null, maxSize, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(GUI.IntScale(20f), GUI.IntScale(20f) + (this.Rect.Bottom - GameMain.GraphicsHeight))
			}, TextManager.Get("close"), Alignment.Center, "", null);
		}

		// Token: 0x06002473 RID: 9331 RVA: 0x00171144 File Offset: 0x0016F344
		private void Load()
		{
			this.scrollSpeed = this.configElement.GetAttributeFloat("scrollspeed", 100f);
			int spacing = this.configElement.GetAttributeInt("spacing", 0);
			this.listBox = new GUIListBox(new RectTransform(Vector2.One, base.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				Spacing = spacing
			};
			foreach (ContentXElement subElement in this.configElement.Elements())
			{
				GUIComponent.FromXML(subElement, this.listBox.Content.RectTransform);
			}
			foreach (GUIComponent child in this.listBox.Content.Children)
			{
				child.CanBeFocused = false;
			}
			this.listBox.RecalculateChildren();
			this.listBox.UpdateScrollBarSize();
		}

		// Token: 0x06002474 RID: 9332 RVA: 0x0017128C File Offset: 0x0016F48C
		public void Restart()
		{
			this.listBox.BarScroll = 0f;
		}

		// Token: 0x06002475 RID: 9333 RVA: 0x001712A0 File Offset: 0x0016F4A0
		protected override void Update(float deltaTime)
		{
			if (!base.Visible)
			{
				return;
			}
			this.listBox.BarScroll += this.scrollSpeed / this.listBox.TotalSize * deltaTime;
			if (this.AutoRestart && this.listBox.BarScroll >= 1f)
			{
				this.listBox.BarScroll = 0f;
			}
		}

		// Token: 0x04001225 RID: 4645
		private GUIListBox listBox;

		// Token: 0x04001226 RID: 4646
		private readonly ContentXElement configElement;

		// Token: 0x04001227 RID: 4647
		private float scrollSpeed;

		// Token: 0x04001228 RID: 4648
		public bool AutoRestart = true;

		// Token: 0x04001229 RID: 4649
		public readonly GUIButton CloseButton;
	}
}
