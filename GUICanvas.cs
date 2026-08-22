using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200007F RID: 127
	public class GUICanvas : RectTransform
	{
		// Token: 0x060011EE RID: 4590 RVA: 0x000B1DD4 File Offset: 0x000AFFD4
		protected GUICanvas() : base(GUICanvas.Size, null, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
		{
		}

		// Token: 0x1700046B RID: 1131
		// (get) Token: 0x060011EF RID: 4591 RVA: 0x000B1E18 File Offset: 0x000B0018
		public static GUICanvas Instance
		{
			get
			{
				if (GUICanvas._instance == null)
				{
					GUICanvas._instance = new GUICanvas();
					if (GameMain.Instance != null)
					{
						GameMain instance = GameMain.Instance;
						Action value;
						if ((value = GUICanvas.<>O.<0>__RecalculateSize) == null)
						{
							value = (GUICanvas.<>O.<0>__RecalculateSize = new Action(GUICanvas.RecalculateSize));
						}
						instance.ResolutionChanged += value;
					}
					RectTransform instance2 = GUICanvas._instance;
					Action<RectTransform> value2;
					if ((value2 = GUICanvas.<>O.<1>__OnChildrenChanged) == null)
					{
						value2 = (GUICanvas.<>O.<1>__OnChildrenChanged = new Action<RectTransform>(GUICanvas.OnChildrenChanged));
					}
					instance2.ChildrenChanged += value2;
				}
				return GUICanvas._instance;
			}
		}

		// Token: 0x1700046C RID: 1132
		// (get) Token: 0x060011F0 RID: 4592 RVA: 0x000B1E8C File Offset: 0x000B008C
		private static Vector2 Size
		{
			get
			{
				return new Vector2((float)GameMain.GraphicsWidth / (float)GUI.UIWidth, 1f);
			}
		}

		// Token: 0x1700046D RID: 1133
		// (get) Token: 0x060011F1 RID: 4593 RVA: 0x000B1EA5 File Offset: 0x000B00A5
		protected override Rectangle NonScaledUIRect
		{
			get
			{
				return base.UIRect;
			}
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x000B1EAD File Offset: 0x000B00AD
		private static void OnChildrenChanged(RectTransform _)
		{
			CrossThread.TaskDelegate deleg;
			if ((deleg = GUICanvas.<>O.<2>__RefreshChildren) == null)
			{
				deleg = (GUICanvas.<>O.<2>__RefreshChildren = new CrossThread.TaskDelegate(GUICanvas.RefreshChildren));
			}
			CrossThread.RequestExecutionOnMainThread(deleg);
		}

		// Token: 0x060011F3 RID: 4595 RVA: 0x000B1ED0 File Offset: 0x000B00D0
		private static void RefreshChildren()
		{
			using (IEnumerator<RectTransform> enumerator = GUICanvas._instance.Children.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					RectTransform child = enumerator.Current;
					if (!GUICanvas._instance.childrenWeakRef.Any(delegate(WeakReference<RectTransform> c)
					{
						RectTransform existingChild;
						return c.TryGetTarget(out existingChild) && existingChild == child;
					}))
					{
						GUICanvas._instance.childrenWeakRef.Add(new WeakReference<RectTransform>(child));
					}
				}
			}
			GUICanvas._instance.children.Clear();
			for (int i = GUICanvas._instance.childrenWeakRef.Count - 1; i >= 0; i--)
			{
				RectTransform child2;
				if (!GUICanvas._instance.childrenWeakRef[i].TryGetTarget(out child2) || child2.Parent != GUICanvas._instance)
				{
					GUICanvas._instance.childrenWeakRef.RemoveAt(i);
				}
			}
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x000B1FC0 File Offset: 0x000B01C0
		private static void RecalculateSize()
		{
			Vector2 recalculatedSize = GUICanvas.Size;
			for (int i = 0; i < GUICanvas.Instance.childrenWeakRef.Count; i++)
			{
				RectTransform target;
				if (GUICanvas._instance.childrenWeakRef[i].TryGetTarget(out target) && target != null)
				{
					GUICanvas._instance.children.Add(target);
					if (target.RelativeSize.X >= 1f || target.RelativeSize.Y >= 1f)
					{
						GUICanvas.ResizeAxis axis;
						if (target.RelativeSize.X >= 1f && target.RelativeSize.Y >= 1f)
						{
							axis = GUICanvas.ResizeAxis.Both;
						}
						else if (target.RelativeSize.X >= 1f)
						{
							axis = GUICanvas.ResizeAxis.X;
						}
						else
						{
							axis = GUICanvas.ResizeAxis.Y;
						}
						switch (axis)
						{
						case GUICanvas.ResizeAxis.Both:
							target.RelativeSize = recalculatedSize;
							break;
						case GUICanvas.ResizeAxis.X:
							target.RelativeSize = new Vector2(recalculatedSize.X, target.RelativeSize.Y);
							break;
						case GUICanvas.ResizeAxis.Y:
							target.RelativeSize = new Vector2(target.RelativeSize.X, recalculatedSize.Y);
							break;
						}
					}
				}
			}
			GUICanvas.Instance.Resize(GUICanvas.Size, true);
			(from c in GUICanvas.Instance.GetAllChildren()
			select c.GUIComponent as GUITextBlock).ForEach(delegate(GUITextBlock t)
			{
				if (t != null)
				{
					t.SetTextPos();
				}
			});
			GUICanvas._instance.children.Clear();
		}

		// Token: 0x040008F4 RID: 2292
		private static GUICanvas _instance;

		// Token: 0x040008F5 RID: 2293
		private readonly List<WeakReference<RectTransform>> childrenWeakRef = new List<WeakReference<RectTransform>>();

		// Token: 0x02000934 RID: 2356
		private enum ResizeAxis
		{
			// Token: 0x040040A5 RID: 16549
			Both,
			// Token: 0x040040A6 RID: 16550
			X,
			// Token: 0x040040A7 RID: 16551
			Y
		}

		// Token: 0x02000935 RID: 2357
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040040A8 RID: 16552
			public static Action <0>__RecalculateSize;

			// Token: 0x040040A9 RID: 16553
			public static Action<RectTransform> <1>__OnChildrenChanged;

			// Token: 0x040040AA RID: 16554
			public static CrossThread.TaskDelegate <2>__RefreshChildren;
		}
	}
}
