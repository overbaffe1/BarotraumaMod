using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000AD RID: 173
	internal class ParamsEditor
	{
		// Token: 0x1700058E RID: 1422
		// (get) Token: 0x060015B7 RID: 5559 RVA: 0x000CDF38 File Offset: 0x000CC138
		public static ParamsEditor Instance
		{
			get
			{
				if (ParamsEditor._instance == null)
				{
					ParamsEditor._instance = new ParamsEditor(null);
				}
				return ParamsEditor._instance;
			}
		}

		// Token: 0x1700058F RID: 1423
		// (get) Token: 0x060015B8 RID: 5560 RVA: 0x000CDF51 File Offset: 0x000CC151
		// (set) Token: 0x060015B9 RID: 5561 RVA: 0x000CDF59 File Offset: 0x000CC159
		public GUIComponent Parent { get; private set; }

		// Token: 0x17000590 RID: 1424
		// (get) Token: 0x060015BA RID: 5562 RVA: 0x000CDF62 File Offset: 0x000CC162
		// (set) Token: 0x060015BB RID: 5563 RVA: 0x000CDF6A File Offset: 0x000CC16A
		public GUIListBox EditorBox { get; private set; }

		// Token: 0x060015BC RID: 5564 RVA: 0x000CDF74 File Offset: 0x000CC174
		public IEnumerable<SerializableEntityEditor> FindEntityEditors()
		{
			return from c in this.EditorBox.Content.RectTransform.Children
			select c.GUIComponent as SerializableEntityEditor into c
			where c != null
			select c;
		}

		// Token: 0x060015BD RID: 5565 RVA: 0x000CDFE0 File Offset: 0x000CC1E0
		public GUIListBox CreateEditorBox(RectTransform rectT = null)
		{
			RectTransform rectTransform;
			if ((rectTransform = rectT) == null)
			{
				(rectTransform = new RectTransform(new Vector2(0.25f, 1f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)).MinSize = new Point(340, GameMain.GraphicsHeight);
			}
			rectT = rectTransform;
			rectT.SetPosition(Anchor.TopRight, null);
			this.Parent = new GUIFrame(rectT, null, new Color?(ParamsEditor.Color));
			this.EditorBox = new GUIListBox(new RectTransform(Vector2.One * 0.98f, rectT, Anchor.Center, null, null, null, ScaleBasis.Normal), false, new Color?(Color.Black), null, true, false)
			{
				Spacing = 10,
				AutoHideScrollBar = true,
				KeepSpaceForScrollBar = true
			};
			return this.EditorBox;
		}

		// Token: 0x060015BE RID: 5566 RVA: 0x000CE0CD File Offset: 0x000CC2CD
		public void Clear()
		{
			this.EditorBox.ClearChildren();
		}

		// Token: 0x060015BF RID: 5567 RVA: 0x000CE0DA File Offset: 0x000CC2DA
		public ParamsEditor(RectTransform rectT = null)
		{
			this.EditorBox = this.CreateEditorBox(null);
		}

		// Token: 0x04000AF0 RID: 2800
		private static ParamsEditor _instance;

		// Token: 0x04000AF3 RID: 2803
		public static Color Color = new Color(20, 20, 20, 255);
	}
}
