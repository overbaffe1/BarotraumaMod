using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200008A RID: 138
	public class GUILayoutGroup : GUIComponent
	{
		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x060012DB RID: 4827 RVA: 0x000B7DFC File Offset: 0x000B5FFC
		// (set) Token: 0x060012DC RID: 4828 RVA: 0x000B7E04 File Offset: 0x000B6004
		public bool IsHorizontal
		{
			get
			{
				return this.isHorizontal;
			}
			set
			{
				this.isHorizontal = value;
				this.needsToRecalculate = true;
			}
		}

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x060012DD RID: 4829 RVA: 0x000B7E14 File Offset: 0x000B6014
		// (set) Token: 0x060012DE RID: 4830 RVA: 0x000B7E1C File Offset: 0x000B601C
		public bool Stretch
		{
			get
			{
				return this.stretch;
			}
			set
			{
				this.stretch = value;
				this.needsToRecalculate = true;
			}
		}

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x060012DF RID: 4831 RVA: 0x000B7E2C File Offset: 0x000B602C
		// (set) Token: 0x060012E0 RID: 4832 RVA: 0x000B7E34 File Offset: 0x000B6034
		public int AbsoluteSpacing
		{
			get
			{
				return this.absoluteSpacing;
			}
			set
			{
				this.absoluteSpacing = MathHelper.Clamp(value, 0, int.MaxValue);
				this.needsToRecalculate = true;
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x060012E1 RID: 4833 RVA: 0x000B7E4F File Offset: 0x000B604F
		// (set) Token: 0x060012E2 RID: 4834 RVA: 0x000B7E57 File Offset: 0x000B6057
		public float RelativeSpacing
		{
			get
			{
				return this.relativeSpacing;
			}
			set
			{
				this.relativeSpacing = MathHelper.Clamp(value, -1f, 1f);
				this.needsToRecalculate = true;
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x060012E3 RID: 4835 RVA: 0x000B7E76 File Offset: 0x000B6076
		// (set) Token: 0x060012E4 RID: 4836 RVA: 0x000B7E7E File Offset: 0x000B607E
		public Anchor ChildAnchor
		{
			get
			{
				return this.childAnchor;
			}
			set
			{
				this.childAnchor = value;
				this.needsToRecalculate = true;
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x060012E5 RID: 4837 RVA: 0x000B7E8E File Offset: 0x000B608E
		// (set) Token: 0x060012E6 RID: 4838 RVA: 0x000B7E96 File Offset: 0x000B6096
		public bool NeedsToRecalculate
		{
			get
			{
				return this.needsToRecalculate;
			}
			set
			{
				if (value)
				{
					this.needsToRecalculate = true;
				}
			}
		}

		// Token: 0x060012E7 RID: 4839 RVA: 0x000B7EA4 File Offset: 0x000B60A4
		public GUILayoutGroup(RectTransform rectT, bool isHorizontal = false, Anchor childAnchor = Anchor.TopLeft) : base(null, rectT)
		{
			this.CanBeFocused = false;
			this.isHorizontal = isHorizontal;
			this.childAnchor = childAnchor;
			rectT.ChildrenChanged += delegate(RectTransform child)
			{
				this.needsToRecalculate = true;
			};
			rectT.ScaleChanged += delegate()
			{
				this.needsToRecalculate = true;
			};
			rectT.SizeChanged += delegate()
			{
				this.needsToRecalculate = true;
			};
		}

		// Token: 0x060012E8 RID: 4840 RVA: 0x000B7F04 File Offset: 0x000B6104
		public void Recalculate()
		{
			GUILayoutGroup.<>c__DisplayClass25_0 CS$<>8__locals1 = new GUILayoutGroup.<>c__DisplayClass25_0();
			CS$<>8__locals1.<>4__this = this;
			float stretchFactor = 1f;
			if (this.stretch && base.RectTransform.Children.Count<RectTransform>() > 0)
			{
				foreach (RectTransform child in base.RectTransform.Children)
				{
					if (!child.GUIComponent.IgnoreLayoutGroups)
					{
						switch (child.ScaleBasis)
						{
						case ScaleBasis.BothWidth:
							goto IL_132;
						case ScaleBasis.BothHeight:
							break;
						case ScaleBasis.Smallest:
							if (this.Rect.Height > this.Rect.Width)
							{
								if (this.Rect.Width > this.Rect.Height)
								{
									continue;
								}
								goto IL_132;
							}
							break;
						case ScaleBasis.Largest:
							if (this.Rect.Height <= this.Rect.Width)
							{
								if (this.Rect.Width > this.Rect.Height)
								{
									goto IL_132;
								}
								continue;
							}
							break;
						default:
							continue;
						}
						child.MinSize = new Point((int)((float)child.Rect.Height * child.RelativeSize.X / child.RelativeSize.Y), child.MinSize.Y);
						continue;
						IL_132:
						child.MinSize = new Point(child.MinSize.X, (int)((float)child.Rect.Width * child.RelativeSize.Y / child.RelativeSize.X));
					}
				}
				float minSize = (float)(from c in base.RectTransform.Children
				where !c.GUIComponent.IgnoreLayoutGroups
				select c).Sum(delegate(RectTransform c)
				{
					if (!CS$<>8__locals1.<>4__this.isHorizontal)
					{
						if (!c.IsFixedSize)
						{
							return c.MinSize.Y;
						}
						return c.NonScaledSize.Y;
					}
					else
					{
						if (!c.IsFixedSize)
						{
							return c.MinSize.X;
						}
						return c.NonScaledSize.X;
					}
				});
				float totalSize = (float)(from c in base.RectTransform.Children
				where !c.GUIComponent.IgnoreLayoutGroups
				select c).Sum(delegate(RectTransform c)
				{
					if (!CS$<>8__locals1.<>4__this.isHorizontal)
					{
						if (!c.IsFixedSize)
						{
							return MathHelper.Clamp(c.Rect.Height, c.MinSize.Y, c.MaxSize.Y);
						}
						return c.Rect.Height;
					}
					else
					{
						if (!c.IsFixedSize)
						{
							return MathHelper.Clamp(c.Rect.Width, c.MinSize.X, c.MaxSize.X);
						}
						return c.Rect.Width;
					}
				});
				float thisSize = (float)(this.isHorizontal ? this.Rect.Width : this.Rect.Height);
				totalSize += (float)(base.RectTransform.Children.Count((RectTransform c) => !c.GUIComponent.IgnoreLayoutGroups) - 1) * ((float)this.absoluteSpacing + this.relativeSpacing * thisSize);
				stretchFactor = ((totalSize <= 0f || minSize >= thisSize || totalSize == minSize) ? 1f : ((thisSize - minSize) / (totalSize - minSize)));
			}
			CS$<>8__locals1.absPos = 0;
			float relPos = 0f;
			foreach (RectTransform child2 in base.RectTransform.Children)
			{
				GUILayoutGroup.<>c__DisplayClass25_1 CS$<>8__locals2;
				CS$<>8__locals2.child = child2;
				if (!CS$<>8__locals2.child.GUIComponent.IgnoreLayoutGroups)
				{
					GUILayoutGroup.<>c__DisplayClass25_2 CS$<>8__locals3;
					CS$<>8__locals3.currentStretchFactor = ((CS$<>8__locals2.child.ScaleBasis == ScaleBasis.Normal) ? stretchFactor : 1f);
					CS$<>8__locals2.child.SetPosition(this.childAnchor, null);
					Point childNonScaledSize = CS$<>8__locals2.child.NonScaledSize;
					Vector2 childRelativeSize = CS$<>8__locals2.child.RelativeSize;
					if (this.isHorizontal)
					{
						CS$<>8__locals2.child.RelativeOffset = new Vector2(relPos, CS$<>8__locals2.child.RelativeOffset.Y);
						CS$<>8__locals2.child.AbsoluteOffset = new Point(CS$<>8__locals1.absPos, CS$<>8__locals2.child.AbsoluteOffset.Y);
						CS$<>8__locals1.<Recalculate>g__advancePositionsAndCalculateChildSizes|5(ref childNonScaledSize.X, ref childRelativeSize.X, CS$<>8__locals2.child.MinSize.X, CS$<>8__locals2.child.MaxSize.X, CS$<>8__locals2.child.Rect.Width, this.Rect.Width, ref CS$<>8__locals2, ref CS$<>8__locals3);
					}
					else
					{
						CS$<>8__locals2.child.RelativeOffset = new Vector2(CS$<>8__locals2.child.RelativeOffset.X, relPos);
						CS$<>8__locals2.child.AbsoluteOffset = new Point(CS$<>8__locals2.child.AbsoluteOffset.X, CS$<>8__locals1.absPos);
						CS$<>8__locals1.<Recalculate>g__advancePositionsAndCalculateChildSizes|5(ref childNonScaledSize.Y, ref childRelativeSize.Y, CS$<>8__locals2.child.MinSize.Y, CS$<>8__locals2.child.MaxSize.Y, CS$<>8__locals2.child.Rect.Height, this.Rect.Height, ref CS$<>8__locals2, ref CS$<>8__locals3);
					}
					CS$<>8__locals2.child.NonScaledSize = childNonScaledSize;
					CS$<>8__locals2.child.RelativeSize = childRelativeSize;
					relPos += this.relativeSpacing * stretchFactor;
					if (this.isHorizontal)
					{
						relPos = MathF.Round(relPos * (float)this.Rect.Width) / (float)this.Rect.Width;
					}
					else
					{
						relPos = MathF.Round(relPos * (float)this.Rect.Height) / (float)this.Rect.Height;
					}
				}
			}
			this.needsToRecalculate = false;
		}

		// Token: 0x060012E9 RID: 4841 RVA: 0x000B846C File Offset: 0x000B666C
		protected override void Update(float deltaTime)
		{
			base.Update(deltaTime);
			if (this.needsToRecalculate)
			{
				this.Recalculate();
			}
		}

		// Token: 0x060012EA RID: 4842 RVA: 0x000B8483 File Offset: 0x000B6683
		public override void ForceLayoutRecalculation()
		{
			this.Recalculate();
			base.ForceLayoutRecalculation();
		}

		// Token: 0x04000972 RID: 2418
		private bool isHorizontal;

		// Token: 0x04000973 RID: 2419
		private bool stretch;

		// Token: 0x04000974 RID: 2420
		private int absoluteSpacing;

		// Token: 0x04000975 RID: 2421
		private float relativeSpacing;

		// Token: 0x04000976 RID: 2422
		private Anchor childAnchor;

		// Token: 0x04000977 RID: 2423
		private bool needsToRecalculate;
	}
}
