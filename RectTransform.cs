using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000B1 RID: 177
	public class RectTransform
	{
		// Token: 0x17000591 RID: 1425
		// (get) Token: 0x060015C1 RID: 5569 RVA: 0x000CE106 File Offset: 0x000CC306
		// (set) Token: 0x060015C2 RID: 5570 RVA: 0x000CE10E File Offset: 0x000CC30E
		public GUIComponent GUIComponent { get; set; }

		// Token: 0x17000592 RID: 1426
		// (get) Token: 0x060015C3 RID: 5571 RVA: 0x000CE117 File Offset: 0x000CC317
		// (set) Token: 0x060015C4 RID: 5572 RVA: 0x000CE120 File Offset: 0x000CC320
		public RectTransform Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				if (this.parent == value || value == this)
				{
					return;
				}
				this.RemoveFromHierarchy(false);
				this.parent = value;
				if (this.parent != null && !this.parent.children.Contains(this))
				{
					this.parent.children.Add(this);
					this.RecalculateAll(false, true, true);
					Action<RectTransform> childrenChanged = this.Parent.ChildrenChanged;
					if (childrenChanged != null)
					{
						childrenChanged(this);
					}
				}
				Action<RectTransform> parentChanged = this.ParentChanged;
				if (parentChanged == null)
				{
					return;
				}
				parentChanged(this.parent);
			}
		}

		// Token: 0x17000593 RID: 1427
		// (get) Token: 0x060015C5 RID: 5573 RVA: 0x000CE1AC File Offset: 0x000CC3AC
		public IEnumerable<RectTransform> Children
		{
			get
			{
				return this.children;
			}
		}

		// Token: 0x17000594 RID: 1428
		// (get) Token: 0x060015C6 RID: 5574 RVA: 0x000CE1B4 File Offset: 0x000CC3B4
		public int CountChildren
		{
			get
			{
				return this.children.Count;
			}
		}

		// Token: 0x17000595 RID: 1429
		// (get) Token: 0x060015C7 RID: 5575 RVA: 0x000CE1C1 File Offset: 0x000CC3C1
		// (set) Token: 0x060015C8 RID: 5576 RVA: 0x000CE1C9 File Offset: 0x000CC3C9
		public Vector2 RelativeSize
		{
			get
			{
				return this.relativeSize;
			}
			set
			{
				if (this.relativeSize.NearlyEquals(value))
				{
					return;
				}
				this.relativeSize = value;
				this.RecalculateAll(true, false, true);
			}
		}

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x060015C9 RID: 5577 RVA: 0x000CE1EC File Offset: 0x000CC3EC
		// (set) Token: 0x060015CA RID: 5578 RVA: 0x000CE218 File Offset: 0x000CC418
		public Point MinSize
		{
			get
			{
				Point? point = this.minSize;
				if (point == null)
				{
					return Point.Zero;
				}
				return point.GetValueOrDefault();
			}
			set
			{
				if (this.minSize == value)
				{
					return;
				}
				this.minSize = new Point?(value);
				this.RecalculateAll(true, false, true);
			}
		}

		// Token: 0x17000597 RID: 1431
		// (get) Token: 0x060015CB RID: 5579 RVA: 0x000CE260 File Offset: 0x000CC460
		// (set) Token: 0x060015CC RID: 5580 RVA: 0x000CE28C File Offset: 0x000CC48C
		public Point MaxSize
		{
			get
			{
				Point? point = this.maxSize;
				if (point == null)
				{
					return RectTransform.MaxPoint;
				}
				return point.GetValueOrDefault();
			}
			set
			{
				if (this.maxSize == value)
				{
					return;
				}
				this.maxSize = new Point?(value);
				this.RecalculateAll(true, false, true);
			}
		}

		// Token: 0x17000598 RID: 1432
		// (get) Token: 0x060015CD RID: 5581 RVA: 0x000CE2D3 File Offset: 0x000CC4D3
		// (set) Token: 0x060015CE RID: 5582 RVA: 0x000CE2DC File Offset: 0x000CC4DC
		public Point NonScaledSize
		{
			get
			{
				return this.nonScaledSize;
			}
			set
			{
				if (this.nonScaledSize == value)
				{
					return;
				}
				this.nonScaledSize = value.Clamp(this.MinSize, this.MaxSize);
				this.RecalculateRelativeSize();
				this.RecalculateAnchorPoint();
				this.RecalculatePivotOffset();
				this.RecalculateChildren(true, false);
			}
		}

		// Token: 0x17000599 RID: 1433
		// (get) Token: 0x060015CF RID: 5583 RVA: 0x000CE32A File Offset: 0x000CC52A
		public Point ScaledSize
		{
			get
			{
				return this.NonScaledSize.Multiply(this.Scale);
			}
		}

		// Token: 0x1700059A RID: 1434
		// (get) Token: 0x060015D0 RID: 5584 RVA: 0x000CE33D File Offset: 0x000CC53D
		// (set) Token: 0x060015D1 RID: 5585 RVA: 0x000CE345 File Offset: 0x000CC545
		public Vector2 LocalScale
		{
			get
			{
				return this.localScale;
			}
			set
			{
				if (this.localScale.NearlyEquals(value))
				{
					return;
				}
				this.localScale = value;
				this.RecalculateAll(false, true, true);
				Action scaleChanged = this.ScaleChanged;
				if (scaleChanged == null)
				{
					return;
				}
				scaleChanged();
			}
		}

		// Token: 0x1700059B RID: 1435
		// (get) Token: 0x060015D2 RID: 5586 RVA: 0x000CE376 File Offset: 0x000CC576
		// (set) Token: 0x060015D3 RID: 5587 RVA: 0x000CE37E File Offset: 0x000CC57E
		public Vector2 Scale { get; private set; }

		// Token: 0x1700059C RID: 1436
		// (get) Token: 0x060015D4 RID: 5588 RVA: 0x000CE387 File Offset: 0x000CC587
		// (set) Token: 0x060015D5 RID: 5589 RVA: 0x000CE38F File Offset: 0x000CC58F
		public Vector2 RelativeOffset
		{
			get
			{
				return this.relativeOffset;
			}
			set
			{
				if (this.relativeOffset.NearlyEquals(value))
				{
					return;
				}
				this.relativeOffset = value;
				this.recalculateRect = true;
				this.RecalculateChildren(false, false);
			}
		}

		// Token: 0x1700059D RID: 1437
		// (get) Token: 0x060015D6 RID: 5590 RVA: 0x000CE3B6 File Offset: 0x000CC5B6
		// (set) Token: 0x060015D7 RID: 5591 RVA: 0x000CE3BE File Offset: 0x000CC5BE
		public Point AbsoluteOffset
		{
			get
			{
				return this.absoluteOffset;
			}
			set
			{
				if (this.absoluteOffset == value)
				{
					return;
				}
				this.absoluteOffset = value;
				this.recalculateRect = true;
				this.RecalculateChildren(false, false);
			}
		}

		// Token: 0x1700059E RID: 1438
		// (get) Token: 0x060015D8 RID: 5592 RVA: 0x000CE3E5 File Offset: 0x000CC5E5
		// (set) Token: 0x060015D9 RID: 5593 RVA: 0x000CE3ED File Offset: 0x000CC5ED
		public Point ScreenSpaceOffset
		{
			get
			{
				return this.screenSpaceOffset;
			}
			set
			{
				if (this.screenSpaceOffset == value)
				{
					return;
				}
				this.screenSpaceOffset = value;
				this.recalculateRect = true;
				this.RecalculateChildren(false, false);
			}
		}

		// Token: 0x1700059F RID: 1439
		// (get) Token: 0x060015DA RID: 5594 RVA: 0x000CE414 File Offset: 0x000CC614
		// (set) Token: 0x060015DB RID: 5595 RVA: 0x000CE41C File Offset: 0x000CC61C
		public Point PivotOffset { get; private set; }

		// Token: 0x170005A0 RID: 1440
		// (get) Token: 0x060015DC RID: 5596 RVA: 0x000CE425 File Offset: 0x000CC625
		// (set) Token: 0x060015DD RID: 5597 RVA: 0x000CE42D File Offset: 0x000CC62D
		public Point AnchorPoint { get; private set; }

		// Token: 0x170005A1 RID: 1441
		// (get) Token: 0x060015DE RID: 5598 RVA: 0x000CE438 File Offset: 0x000CC638
		public Point TopLeft
		{
			get
			{
				Point absoluteOffset = RectTransform.ConvertOffsetRelativeToAnchor(this.AbsoluteOffset, this.Anchor);
				Point relativeOffset = this.ParentRect.MultiplySize(this.RelativeOffset);
				relativeOffset = RectTransform.ConvertOffsetRelativeToAnchor(relativeOffset, this.Anchor);
				return this.AnchorPoint + this.PivotOffset + absoluteOffset + relativeOffset + this.ScreenSpaceOffset;
			}
		}

		// Token: 0x170005A2 RID: 1442
		// (get) Token: 0x060015DF RID: 5599 RVA: 0x000CE4A0 File Offset: 0x000CC6A0
		protected Point NonScaledTopLeft
		{
			get
			{
				Point absoluteOffset = RectTransform.ConvertOffsetRelativeToAnchor(this.AbsoluteOffset, this.Anchor);
				Point relativeOffset = new Point((int)((float)this.NonScaledParentSize.X * this.RelativeOffset.X), (int)((float)this.NonScaledParentSize.Y * this.RelativeOffset.Y));
				relativeOffset = RectTransform.ConvertOffsetRelativeToAnchor(relativeOffset, this.Anchor);
				return this.AnchorPoint + this.PivotOffset + absoluteOffset + relativeOffset + this.ScreenSpaceOffset;
			}
		}

		// Token: 0x170005A3 RID: 1443
		// (get) Token: 0x060015E0 RID: 5600 RVA: 0x000CE52D File Offset: 0x000CC72D
		public Rectangle Rect
		{
			get
			{
				if (this.recalculateRect)
				{
					this._rect = new Rectangle(this.TopLeft, this.ScaledSize);
					this.recalculateRect = false;
				}
				return this._rect;
			}
		}

		// Token: 0x170005A4 RID: 1444
		// (get) Token: 0x060015E1 RID: 5601 RVA: 0x000CE55B File Offset: 0x000CC75B
		public Rectangle ParentRect
		{
			get
			{
				if (this.Parent == null)
				{
					return this.UIRect;
				}
				return this.Parent.Rect;
			}
		}

		// Token: 0x170005A5 RID: 1445
		// (get) Token: 0x060015E2 RID: 5602 RVA: 0x000CE577 File Offset: 0x000CC777
		protected Rectangle NonScaledRect
		{
			get
			{
				return new Rectangle(this.NonScaledTopLeft, this.NonScaledSize);
			}
		}

		// Token: 0x170005A6 RID: 1446
		// (get) Token: 0x060015E3 RID: 5603 RVA: 0x000CE58A File Offset: 0x000CC78A
		protected virtual Rectangle NonScaledUIRect
		{
			get
			{
				return this.NonScaledRect;
			}
		}

		// Token: 0x170005A7 RID: 1447
		// (get) Token: 0x060015E4 RID: 5604 RVA: 0x000CE592 File Offset: 0x000CC792
		protected Point NonScaledParentSize
		{
			get
			{
				RectTransform rectTransform = this.parent;
				if (rectTransform == null)
				{
					return new Point(GUI.UIWidth, GameMain.GraphicsHeight);
				}
				return rectTransform.NonScaledSize;
			}
		}

		// Token: 0x170005A8 RID: 1448
		// (get) Token: 0x060015E5 RID: 5605 RVA: 0x000CE5B3 File Offset: 0x000CC7B3
		protected Rectangle NonScaledParentRect
		{
			get
			{
				if (this.parent == null)
				{
					return this.UIRect;
				}
				return this.Parent.NonScaledRect;
			}
		}

		// Token: 0x170005A9 RID: 1449
		// (get) Token: 0x060015E6 RID: 5606 RVA: 0x000CE5CF File Offset: 0x000CC7CF
		protected Rectangle NonScaledParentUIRect
		{
			get
			{
				if (this.parent == null)
				{
					return this.UIRect;
				}
				return this.Parent.NonScaledUIRect;
			}
		}

		// Token: 0x170005AA RID: 1450
		// (get) Token: 0x060015E7 RID: 5607 RVA: 0x000CE5EB File Offset: 0x000CC7EB
		protected Rectangle UIRect
		{
			get
			{
				return new Rectangle(0, 0, GUI.UIWidth, GameMain.GraphicsHeight);
			}
		}

		// Token: 0x170005AB RID: 1451
		// (get) Token: 0x060015E8 RID: 5608 RVA: 0x000CE5FE File Offset: 0x000CC7FE
		// (set) Token: 0x060015E9 RID: 5609 RVA: 0x000CE606 File Offset: 0x000CC806
		public Pivot Pivot
		{
			get
			{
				return this.pivot;
			}
			set
			{
				if (this.pivot == value)
				{
					return;
				}
				this.pivot = value;
				this.RecalculatePivotOffset();
			}
		}

		// Token: 0x170005AC RID: 1452
		// (get) Token: 0x060015EA RID: 5610 RVA: 0x000CE61F File Offset: 0x000CC81F
		// (set) Token: 0x060015EB RID: 5611 RVA: 0x000CE627 File Offset: 0x000CC827
		public Anchor Anchor
		{
			get
			{
				return this.anchor;
			}
			set
			{
				if (this.anchor == value)
				{
					return;
				}
				this.anchor = value;
				this.RecalculateAnchorPoint();
			}
		}

		// Token: 0x170005AD RID: 1453
		// (get) Token: 0x060015EC RID: 5612 RVA: 0x000CE640 File Offset: 0x000CC840
		// (set) Token: 0x060015ED RID: 5613 RVA: 0x000CE648 File Offset: 0x000CC848
		public ScaleBasis ScaleBasis
		{
			get
			{
				return this._scaleBasis;
			}
			set
			{
				this._scaleBasis = value;
				this.RecalculateAbsoluteSize();
				this.RecalculateAnchorPoint();
				this.RecalculatePivotOffset();
			}
		}

		// Token: 0x170005AE RID: 1454
		// (get) Token: 0x060015EE RID: 5614 RVA: 0x000CE664 File Offset: 0x000CC864
		public bool IsLastChild
		{
			get
			{
				if (this.Parent == null)
				{
					return false;
				}
				RectTransform last = this.Parent.Children.LastOrDefault<RectTransform>();
				return last != null && last == this;
			}
		}

		// Token: 0x170005AF RID: 1455
		// (get) Token: 0x060015EF RID: 5615 RVA: 0x000CE698 File Offset: 0x000CC898
		public bool IsFirstChild
		{
			get
			{
				if (this.Parent == null)
				{
					return false;
				}
				RectTransform first = this.Parent.Children.FirstOrDefault<RectTransform>();
				return first != null && first == this;
			}
		}

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x060015F0 RID: 5616 RVA: 0x000CE6CC File Offset: 0x000CC8CC
		// (remove) Token: 0x060015F1 RID: 5617 RVA: 0x000CE704 File Offset: 0x000CC904
		public event Action<RectTransform> ParentChanged;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x060015F2 RID: 5618 RVA: 0x000CE73C File Offset: 0x000CC93C
		// (remove) Token: 0x060015F3 RID: 5619 RVA: 0x000CE774 File Offset: 0x000CC974
		public event Action<RectTransform> ChildrenChanged;

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x060015F4 RID: 5620 RVA: 0x000CE7AC File Offset: 0x000CC9AC
		// (remove) Token: 0x060015F5 RID: 5621 RVA: 0x000CE7E4 File Offset: 0x000CC9E4
		public event Action ScaleChanged;

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x060015F6 RID: 5622 RVA: 0x000CE81C File Offset: 0x000CCA1C
		// (remove) Token: 0x060015F7 RID: 5623 RVA: 0x000CE854 File Offset: 0x000CCA54
		public event Action SizeChanged;

		// Token: 0x060015F8 RID: 5624 RVA: 0x000CE889 File Offset: 0x000CCA89
		public void ResetSizeChanged()
		{
			this.SizeChanged = null;
		}

		// Token: 0x060015F9 RID: 5625 RVA: 0x000CE894 File Offset: 0x000CCA94
		public RectTransform(Vector2 relativeSize, RectTransform parent, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null, Point? minSize = null, Point? maxSize = null, ScaleBasis scaleBasis = ScaleBasis.Normal)
		{
			this.Init(parent, anchor, pivot);
			this._scaleBasis = scaleBasis;
			this.relativeSize = relativeSize;
			this.minSize = minSize;
			this.maxSize = maxSize;
			this.RecalculateScale();
			this.RecalculateAbsoluteSize();
			this.RecalculateAnchorPoint();
			this.RecalculatePivotOffset();
			if (parent != null)
			{
				Action<RectTransform> childrenChanged = parent.ChildrenChanged;
				if (childrenChanged == null)
				{
					return;
				}
				childrenChanged(this);
			}
		}

		// Token: 0x060015FA RID: 5626 RVA: 0x000CE948 File Offset: 0x000CCB48
		public RectTransform(Point absoluteSize, RectTransform parent = null, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null, ScaleBasis scaleBasis = ScaleBasis.Normal, bool isFixedSize = false)
		{
			this.Init(parent, anchor, pivot);
			this._scaleBasis = scaleBasis;
			this.nonScaledSize = absoluteSize;
			this.RecalculateScale();
			this.RecalculateRelativeSize();
			if (scaleBasis != ScaleBasis.Normal)
			{
				this.RecalculateAbsoluteSize();
			}
			this.RecalculateAnchorPoint();
			this.RecalculatePivotOffset();
			this.IsFixedSize = isFixedSize;
			if (parent != null)
			{
				Action<RectTransform> childrenChanged = parent.ChildrenChanged;
				if (childrenChanged == null)
				{
					return;
				}
				childrenChanged(this);
			}
		}

		// Token: 0x060015FB RID: 5627 RVA: 0x000CE9FC File Offset: 0x000CCBFC
		public static RectTransform Load(XElement element, RectTransform parent, Anchor defaultAnchor = Anchor.TopLeft)
		{
			Anchor anchor;
			Enum.TryParse<Anchor>(element.GetAttributeString("anchor", defaultAnchor.ToString()), out anchor);
			Pivot pivot;
			Enum.TryParse<Pivot>(element.GetAttributeString("pivot", anchor.ToString()), out pivot);
			Point? minSize = null;
			Point? maxSize = null;
			ScaleBasis scaleBasis = ScaleBasis.Normal;
			if (element.Attribute("minsize") != null)
			{
				minSize = new Point?(element.GetAttributePoint("minsize", Point.Zero));
			}
			if (element.Attribute("maxsize") != null)
			{
				maxSize = new Point?(element.GetAttributePoint("maxsize", new Point(1000, 1000)));
			}
			string sb = element.GetAttributeString("scalebasis", null);
			if (sb != null)
			{
				Enum.TryParse<ScaleBasis>(sb, true, out scaleBasis);
			}
			RectTransform rectTransform;
			if (element.Attribute("absolutesize") != null)
			{
				rectTransform = new RectTransform(element.GetAttributePoint("absolutesize", new Point(1000, 1000)), parent, anchor, new Pivot?(pivot), scaleBasis, false)
				{
					minSize = minSize,
					maxSize = maxSize
				};
			}
			else
			{
				rectTransform = new RectTransform(element.GetAttributeVector2("relativesize", Vector2.One), parent, anchor, new Pivot?(pivot), minSize, maxSize, scaleBasis);
			}
			rectTransform.RelativeOffset = element.GetAttributeVector2("relativeoffset", Vector2.Zero);
			rectTransform.AbsoluteOffset = element.GetAttributePoint("absoluteoffset", Point.Zero);
			return rectTransform;
		}

		// Token: 0x060015FC RID: 5628 RVA: 0x000CEB78 File Offset: 0x000CCD78
		private void Init(RectTransform parent = null, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null)
		{
			this.parent = parent;
			if (parent != null)
			{
				parent.children.Add(this);
			}
			this.Anchor = anchor;
			this.Pivot = (pivot ?? RectTransform.MatchPivotToAnchor(this.Anchor));
		}

		// Token: 0x060015FD RID: 5629 RVA: 0x000CEBC8 File Offset: 0x000CCDC8
		protected void RecalculateScale()
		{
			Vector2 scale = this.LocalScale * RectTransform.globalScale;
			IEnumerable<RectTransform> parents = this.GetParents();
			Vector2 scale2;
			if (!parents.Any<RectTransform>())
			{
				scale2 = scale;
			}
			else
			{
				scale2 = (from rt in parents
				select rt.LocalScale).Aggregate((Vector2 parent, Vector2 child) => parent * child) * scale;
			}
			this.Scale = scale2;
			this.recalculateRect = true;
			Action scaleChanged = this.ScaleChanged;
			if (scaleChanged == null)
			{
				return;
			}
			scaleChanged();
		}

		// Token: 0x060015FE RID: 5630 RVA: 0x000CEC64 File Offset: 0x000CCE64
		protected void RecalculatePivotOffset()
		{
			this.PivotOffset = RectTransform.CalculatePivotOffset(this.Pivot, this.ScaledSize);
			this.recalculateRect = true;
		}

		// Token: 0x060015FF RID: 5631 RVA: 0x000CEC84 File Offset: 0x000CCE84
		protected void RecalculateAnchorPoint()
		{
			this.AnchorPoint = RectTransform.CalculateAnchorPoint(this.Anchor, this.ParentRect);
			this.recalculateRect = true;
		}

		// Token: 0x06001600 RID: 5632 RVA: 0x000CECA4 File Offset: 0x000CCEA4
		protected void RecalculateRelativeSize()
		{
			this.relativeSize = new Vector2((float)this.NonScaledSize.X, (float)this.NonScaledSize.Y) / new Vector2((float)this.NonScaledParentUIRect.Width, (float)this.NonScaledParentUIRect.Height);
			this.recalculateRect = true;
			Action sizeChanged = this.SizeChanged;
			if (sizeChanged == null)
			{
				return;
			}
			sizeChanged();
		}

		// Token: 0x06001601 RID: 5633 RVA: 0x000CED10 File Offset: 0x000CCF10
		protected void RecalculateAbsoluteSize()
		{
			Point size = this.NonScaledParentUIRect.Size;
			switch (this.ScaleBasis)
			{
			case ScaleBasis.BothWidth:
				size.Y = size.X;
				break;
			case ScaleBasis.BothHeight:
				size.X = size.Y;
				break;
			case ScaleBasis.Smallest:
				if (size.X < size.Y)
				{
					size.Y = size.X;
				}
				else
				{
					size.X = size.Y;
				}
				break;
			case ScaleBasis.Largest:
				if (size.X > size.Y)
				{
					size.Y = size.X;
				}
				else
				{
					size.X = size.Y;
				}
				break;
			}
			size = size.Multiply(this.RelativeSize);
			this.nonScaledSize = size.Clamp(this.MinSize, this.MaxSize);
			this.recalculateRect = true;
			Action sizeChanged = this.SizeChanged;
			if (sizeChanged == null)
			{
				return;
			}
			sizeChanged();
		}

		// Token: 0x170005B0 RID: 1456
		// (get) Token: 0x06001602 RID: 5634 RVA: 0x000CEDFD File Offset: 0x000CCFFD
		// (set) Token: 0x06001603 RID: 5635 RVA: 0x000CEE05 File Offset: 0x000CD005
		public bool IsFixedSize { get; set; }

		// Token: 0x06001604 RID: 5636 RVA: 0x000CEE0E File Offset: 0x000CD00E
		protected void RecalculateAll(bool resize, bool scale = true, bool withChildren = true)
		{
			if (scale)
			{
				this.RecalculateScale();
			}
			if (resize && !this.IsFixedSize)
			{
				this.RecalculateAbsoluteSize();
			}
			this.RecalculateAnchorPoint();
			this.RecalculatePivotOffset();
			if (withChildren)
			{
				this.RecalculateChildren(resize, scale);
			}
		}

		// Token: 0x06001605 RID: 5637 RVA: 0x000CEE44 File Offset: 0x000CD044
		private bool RemoveFromHierarchy(bool displayErrors = true)
		{
			if (this.Parent == null)
			{
				if (displayErrors)
				{
					DebugConsole.ThrowError("Parent null" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				}
				return false;
			}
			if (!this.Parent.children.Contains(this))
			{
				if (displayErrors)
				{
					DebugConsole.ThrowError("The children of the parent does not contain this child. This should not be possible! " + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				}
				return false;
			}
			if (!this.Parent.children.Remove(this))
			{
				if (displayErrors)
				{
					DebugConsole.ThrowError("Unable to remove the child from the parent. " + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				}
				return false;
			}
			return true;
		}

		// Token: 0x06001606 RID: 5638 RVA: 0x000CEEE8 File Offset: 0x000CD0E8
		public void SetPosition(Anchor anchor, Pivot? pivot = null)
		{
			this.Anchor = anchor;
			this.Pivot = (pivot ?? RectTransform.MatchPivotToAnchor(anchor));
			this.ScreenSpaceOffset = Point.Zero;
			this.recalculateRect = true;
			this.RecalculateChildren(false, false);
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x000CEF36 File Offset: 0x000CD136
		public void Resize(Point absoluteSize, bool resizeChildren = true)
		{
			this.nonScaledSize = absoluteSize.Clamp(this.MinSize, this.MaxSize);
			this.RecalculateRelativeSize();
			this.RecalculateAll(false, false, false);
			this.RecalculateChildren(resizeChildren, false);
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x000CEF67 File Offset: 0x000CD167
		public void Resize(Vector2 relativeSize, bool resizeChildren = true)
		{
			this.relativeSize = relativeSize;
			this.RecalculateAll(true, false, false);
			this.RecalculateChildren(resizeChildren, false);
		}

		// Token: 0x06001609 RID: 5641 RVA: 0x000CEF81 File Offset: 0x000CD181
		public void ChangeScale(Vector2 newScale)
		{
			this.LocalScale = newScale;
		}

		// Token: 0x0600160A RID: 5642 RVA: 0x000CEF8A File Offset: 0x000CD18A
		public void ResetScale()
		{
			this.ChangeScale(Vector2.One);
		}

		// Token: 0x0600160B RID: 5643 RVA: 0x000CEF97 File Offset: 0x000CD197
		public void RecalculateScale(bool withChildren)
		{
			this.RecalculateScale();
			if (withChildren)
			{
				this.RecalculateChildren(false, true);
			}
		}

		// Token: 0x0600160C RID: 5644 RVA: 0x000CEFAA File Offset: 0x000CD1AA
		public void Translate(Point translation)
		{
			this.ScreenSpaceOffset += translation;
		}

		// Token: 0x0600160D RID: 5645 RVA: 0x000CEFC0 File Offset: 0x000CD1C0
		public IEnumerable<RectTransform> GetParents()
		{
			List<RectTransform> parents = new List<RectTransform>();
			if (this.Parent != null)
			{
				parents.Add(this.Parent);
				return parents.Concat(this.Parent.GetParents());
			}
			return parents;
		}

		// Token: 0x0600160E RID: 5646 RVA: 0x000CEFFA File Offset: 0x000CD1FA
		public IEnumerable<RectTransform> GetAllChildren()
		{
			return this.children.Concat(this.children.SelectManyRecursive((RectTransform c) => c.children));
		}

		// Token: 0x0600160F RID: 5647 RVA: 0x000CF031 File Offset: 0x000CD231
		public int GetChildIndex(RectTransform rectT)
		{
			return this.children.IndexOf(rectT);
		}

		// Token: 0x06001610 RID: 5648 RVA: 0x000CF03F File Offset: 0x000CD23F
		public RectTransform GetChild(int index)
		{
			return this.children[index];
		}

		// Token: 0x06001611 RID: 5649 RVA: 0x000CF050 File Offset: 0x000CD250
		public bool IsParentOf(RectTransform rectT, bool recursive = true)
		{
			if (this.children.Contains(rectT))
			{
				return true;
			}
			if (recursive)
			{
				foreach (RectTransform child in this.children)
				{
					if (child.IsParentOf(rectT, true))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06001612 RID: 5650 RVA: 0x000CF0C0 File Offset: 0x000CD2C0
		public bool IsChildOf(RectTransform rectT, bool recursive = true)
		{
			return this.Parent != null && (this.Parent == rectT || (recursive && this.Parent.IsChildOf(rectT, true)));
		}

		// Token: 0x06001613 RID: 5651 RVA: 0x000CF0E9 File Offset: 0x000CD2E9
		public void ClearChildren()
		{
			this.children.ForEachMod(delegate(RectTransform c)
			{
				c.Parent = null;
			});
		}

		// Token: 0x06001614 RID: 5652 RVA: 0x000CF115 File Offset: 0x000CD315
		public void SortChildren(Comparison<RectTransform> comparison)
		{
			this.children.Sort(comparison);
			this.RecalculateAll(false, false, true);
			Action<RectTransform> childrenChanged = this.Parent.ChildrenChanged;
			if (childrenChanged == null)
			{
				return;
			}
			childrenChanged(this);
		}

		// Token: 0x06001615 RID: 5653 RVA: 0x000CF142 File Offset: 0x000CD342
		public void ReverseChildren()
		{
			this.children.Reverse();
			this.RecalculateAll(false, false, true);
			Action<RectTransform> childrenChanged = this.Parent.ChildrenChanged;
			if (childrenChanged == null)
			{
				return;
			}
			childrenChanged(this);
		}

		// Token: 0x06001616 RID: 5654 RVA: 0x000CF170 File Offset: 0x000CD370
		public void SetAsLastChild()
		{
			if (this.IsLastChild)
			{
				return;
			}
			if (!this.RemoveFromHierarchy(true))
			{
				return;
			}
			this.parent.children.Add(this);
			this.RecalculateAll(false, true, true);
			Action<RectTransform> childrenChanged = this.parent.ChildrenChanged;
			if (childrenChanged == null)
			{
				return;
			}
			childrenChanged(this);
		}

		// Token: 0x06001617 RID: 5655 RVA: 0x000CF1C0 File Offset: 0x000CD3C0
		public void SetAsFirstChild()
		{
			if (this.IsFirstChild)
			{
				return;
			}
			this.RepositionChildInHierarchy(0);
		}

		// Token: 0x06001618 RID: 5656 RVA: 0x000CF1D4 File Offset: 0x000CD3D4
		public bool RepositionChildInHierarchy(int index)
		{
			if (!this.RemoveFromHierarchy(true))
			{
				return false;
			}
			try
			{
				this.Parent.children.Insert(index, this);
			}
			catch (ArgumentOutOfRangeException e)
			{
				DebugConsole.ThrowError(e.ToString(), null, null, false, false);
				return false;
			}
			this.RecalculateAll(false, true, true);
			Action<RectTransform> childrenChanged = this.Parent.ChildrenChanged;
			if (childrenChanged != null)
			{
				childrenChanged(this);
			}
			return true;
		}

		// Token: 0x06001619 RID: 5657 RVA: 0x000CF248 File Offset: 0x000CD448
		public void RecalculateChildren(bool resize, bool scale = true)
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].RecalculateAll(resize, scale, true);
			}
		}

		// Token: 0x0600161A RID: 5658 RVA: 0x000CF280 File Offset: 0x000CD480
		public void AddChildrenToGUIUpdateList(bool ignoreChildren = false, int order = 0)
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].GUIComponent.AddToGUIUpdateList(ignoreChildren, order);
			}
		}

		// Token: 0x0600161B RID: 5659 RVA: 0x000CF2BB File Offset: 0x000CD4BB
		public void MatchPivotToAnchor()
		{
			RectTransform.MatchPivotToAnchor(this.Anchor);
		}

		// Token: 0x170005B1 RID: 1457
		// (get) Token: 0x0600161C RID: 5660 RVA: 0x000CF2CC File Offset: 0x000CD4CC
		public Point AnimTargetPos
		{
			get
			{
				Point? point = this.animTargetPos;
				if (point == null)
				{
					return this.AbsoluteOffset;
				}
				return point.GetValueOrDefault();
			}
		}

		// Token: 0x0600161D RID: 5661 RVA: 0x000CF2F7 File Offset: 0x000CD4F7
		public void MoveOverTime(Point targetPos, float duration, Action onDoneMoving = null)
		{
			this.animTargetPos = new Point?(targetPos);
			CoroutineManager.StartCoroutine(this.DoMoveAnimation(targetPos, duration, onDoneMoving), "");
		}

		// Token: 0x0600161E RID: 5662 RVA: 0x000CF319 File Offset: 0x000CD519
		public void ScaleOverTime(Point targetSize, float duration)
		{
			CoroutineManager.StartCoroutine(this.DoScaleAnimation(targetSize, duration), "");
		}

		// Token: 0x0600161F RID: 5663 RVA: 0x000CF32E File Offset: 0x000CD52E
		private IEnumerable<CoroutineStatus> DoMoveAnimation(Point targetPos, float duration, Action onDoneMoving = null)
		{
			RectTransform.<DoMoveAnimation>d__154 <DoMoveAnimation>d__ = new RectTransform.<DoMoveAnimation>d__154(-2);
			<DoMoveAnimation>d__.<>4__this = this;
			<DoMoveAnimation>d__.<>3__targetPos = targetPos;
			<DoMoveAnimation>d__.<>3__duration = duration;
			<DoMoveAnimation>d__.<>3__onDoneMoving = onDoneMoving;
			return <DoMoveAnimation>d__;
		}

		// Token: 0x06001620 RID: 5664 RVA: 0x000CF353 File Offset: 0x000CD553
		private IEnumerable<CoroutineStatus> DoScaleAnimation(Point targetSize, float duration)
		{
			RectTransform.<DoScaleAnimation>d__155 <DoScaleAnimation>d__ = new RectTransform.<DoScaleAnimation>d__155(-2);
			<DoScaleAnimation>d__.<>4__this = this;
			<DoScaleAnimation>d__.<>3__targetSize = targetSize;
			<DoScaleAnimation>d__.<>3__duration = duration;
			return <DoScaleAnimation>d__;
		}

		// Token: 0x06001621 RID: 5665 RVA: 0x000CF374 File Offset: 0x000CD574
		public void InheritTotalChildrenMinHeight()
		{
			this.MinSize = new Point(this.MinSize.X, this.children.Sum((RectTransform c) => c.MinSize.Y));
		}

		// Token: 0x06001622 RID: 5666 RVA: 0x000CF3C4 File Offset: 0x000CD5C4
		public void InheritTotalChildrenHeight()
		{
			this.MinSize = new Point(this.MinSize.X, this.children.Sum((RectTransform c) => c.Rect.Height));
		}

		// Token: 0x06001623 RID: 5667 RVA: 0x000CF411 File Offset: 0x000CD611
		public static Pivot MatchPivotToAnchor(Anchor anchor)
		{
			return (Pivot)anchor;
		}

		// Token: 0x06001624 RID: 5668 RVA: 0x000CF414 File Offset: 0x000CD614
		public static Anchor MatchAnchorToPivot(Pivot pivot)
		{
			return (Anchor)pivot;
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x000CF417 File Offset: 0x000CD617
		public static Anchor MoveAnchorLeft(Anchor anchor)
		{
			switch (anchor)
			{
			case Anchor.TopCenter:
			case Anchor.TopRight:
				return Anchor.TopLeft;
			case Anchor.Center:
			case Anchor.CenterRight:
				return Anchor.CenterLeft;
			case Anchor.BottomCenter:
			case Anchor.BottomRight:
				return Anchor.BottomLeft;
			}
			return anchor;
		}

		// Token: 0x06001626 RID: 5670 RVA: 0x000CF44A File Offset: 0x000CD64A
		public static Anchor MoveAnchorRight(Anchor anchor)
		{
			switch (anchor)
			{
			case Anchor.TopLeft:
			case Anchor.TopCenter:
				return Anchor.TopRight;
			case Anchor.CenterLeft:
			case Anchor.Center:
				return Anchor.CenterRight;
			case Anchor.BottomLeft:
			case Anchor.BottomCenter:
				return Anchor.BottomRight;
			}
			return anchor;
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x000CF47B File Offset: 0x000CD67B
		public static Anchor MoveAnchorTop(Anchor anchor)
		{
			switch (anchor)
			{
			case Anchor.CenterLeft:
			case Anchor.BottomLeft:
				return Anchor.TopLeft;
			case Anchor.Center:
			case Anchor.BottomCenter:
				return Anchor.TopCenter;
			case Anchor.CenterRight:
			case Anchor.BottomRight:
				return Anchor.TopRight;
			default:
				return anchor;
			}
		}

		// Token: 0x06001628 RID: 5672 RVA: 0x000CF4A6 File Offset: 0x000CD6A6
		public static Anchor MoveAnchorBottom(Anchor anchor)
		{
			switch (anchor)
			{
			case Anchor.TopLeft:
			case Anchor.CenterLeft:
				return Anchor.BottomLeft;
			case Anchor.TopCenter:
			case Anchor.Center:
				return Anchor.BottomCenter;
			case Anchor.TopRight:
			case Anchor.CenterRight:
				return Anchor.BottomRight;
			default:
				return anchor;
			}
		}

		// Token: 0x06001629 RID: 5673 RVA: 0x000CF4D0 File Offset: 0x000CD6D0
		public static Point ConvertOffsetRelativeToAnchor(Point offset, Anchor anchor)
		{
			switch (anchor)
			{
			case Anchor.TopRight:
			case Anchor.CenterRight:
				return new Point(-offset.X, offset.Y);
			case Anchor.BottomLeft:
			case Anchor.BottomCenter:
				return new Point(offset.X, -offset.Y);
			case Anchor.BottomRight:
				return offset.Inverse();
			}
			return offset;
		}

		// Token: 0x0600162A RID: 5674 RVA: 0x000CF534 File Offset: 0x000CD734
		public static Point CalculatePivotOffset(Pivot anchor, Point size)
		{
			int width = size.X;
			int height = size.Y;
			switch (anchor)
			{
			case Pivot.TopLeft:
				return Point.Zero;
			case Pivot.TopCenter:
				return new Point(-width / 2, 0);
			case Pivot.TopRight:
				return new Point(-width, 0);
			case Pivot.CenterLeft:
				return new Point(0, -height / 2);
			case Pivot.Center:
				return size.Divide(2).Inverse();
			case Pivot.CenterRight:
				return new Point(-width, -height / 2);
			case Pivot.BottomLeft:
				return new Point(0, -height);
			case Pivot.BottomCenter:
				return new Point(-width / 2, -height);
			case Pivot.BottomRight:
				return new Point(-width, -height);
			default:
				throw new NotImplementedException(anchor.ToString());
			}
		}

		// Token: 0x0600162B RID: 5675 RVA: 0x000CF5EC File Offset: 0x000CD7EC
		public static Point CalculateAnchorPoint(Anchor anchor, Rectangle parent)
		{
			switch (anchor)
			{
			case Anchor.TopLeft:
				return parent.Location;
			case Anchor.TopCenter:
				return new Point(parent.Center.X, parent.Top);
			case Anchor.TopRight:
				return new Point(parent.Right, parent.Top);
			case Anchor.CenterLeft:
				return new Point(parent.Left, parent.Center.Y);
			case Anchor.Center:
				return parent.Center;
			case Anchor.CenterRight:
				return new Point(parent.Right, parent.Center.Y);
			case Anchor.BottomLeft:
				return new Point(parent.Left, parent.Bottom);
			case Anchor.BottomCenter:
				return new Point(parent.Center.X, parent.Bottom);
			case Anchor.BottomRight:
				return new Point(parent.Right, parent.Bottom);
			default:
				throw new NotImplementedException(anchor.ToString());
			}
		}

		// Token: 0x0600162C RID: 5676 RVA: 0x000CF6EA File Offset: 0x000CD8EA
		public static void ResetGlobalScale()
		{
			RectTransform.globalScale = Vector2.One;
		}

		// Token: 0x04000B0F RID: 2831
		private RectTransform parent;

		// Token: 0x04000B10 RID: 2832
		protected readonly List<RectTransform> children = new List<RectTransform>();

		// Token: 0x04000B11 RID: 2833
		private Vector2 relativeSize = Vector2.One;

		// Token: 0x04000B12 RID: 2834
		private Point? minSize;

		// Token: 0x04000B13 RID: 2835
		public static readonly Point MaxPoint = new Point(int.MaxValue, int.MaxValue);

		// Token: 0x04000B14 RID: 2836
		private Point? maxSize;

		// Token: 0x04000B15 RID: 2837
		private Point nonScaledSize;

		// Token: 0x04000B16 RID: 2838
		public static Vector2 globalScale = Vector2.One;

		// Token: 0x04000B17 RID: 2839
		private Vector2 localScale = Vector2.One;

		// Token: 0x04000B19 RID: 2841
		private Vector2 relativeOffset = Vector2.Zero;

		// Token: 0x04000B1A RID: 2842
		private Point absoluteOffset = Point.Zero;

		// Token: 0x04000B1B RID: 2843
		private Point screenSpaceOffset = Point.Zero;

		// Token: 0x04000B1E RID: 2846
		private bool recalculateRect = true;

		// Token: 0x04000B1F RID: 2847
		private Rectangle _rect;

		// Token: 0x04000B20 RID: 2848
		private Pivot pivot;

		// Token: 0x04000B21 RID: 2849
		private Anchor anchor;

		// Token: 0x04000B22 RID: 2850
		private ScaleBasis _scaleBasis;

		// Token: 0x04000B28 RID: 2856
		private Point? animTargetPos;
	}
}
