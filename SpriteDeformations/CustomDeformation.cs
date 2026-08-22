using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.SpriteDeformations
{
	// Token: 0x02000430 RID: 1072
	internal class CustomDeformation : SpriteDeformation
	{
		// Token: 0x17001246 RID: 4678
		// (get) Token: 0x060047F0 RID: 18416 RVA: 0x00278879 File Offset: 0x00276A79
		private CustomDeformationParams CustomDeformationParams
		{
			get
			{
				return base.Params as CustomDeformationParams;
			}
		}

		// Token: 0x17001247 RID: 4679
		// (get) Token: 0x060047F1 RID: 18417 RVA: 0x00278886 File Offset: 0x00276A86
		// (set) Token: 0x060047F2 RID: 18418 RVA: 0x0027888E File Offset: 0x00276A8E
		public override float Phase
		{
			get
			{
				return this.phase;
			}
			set
			{
				this.phase = value;
			}
		}

		// Token: 0x060047F3 RID: 18419 RVA: 0x00278898 File Offset: 0x00276A98
		public CustomDeformation(XElement element) : base(element, new CustomDeformationParams(element))
		{
			this.phase = Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced);
			if (element == null)
			{
				this.deformRows.Add(new Vector2[]
				{
					Vector2.Zero,
					Vector2.Zero
				});
				this.deformRows.Add(new Vector2[]
				{
					Vector2.Zero,
					Vector2.Zero
				});
			}
			else
			{
				int i = 0;
				for (;;)
				{
					string row = element.GetAttributeString("row" + i.ToString(), "");
					if (string.IsNullOrWhiteSpace(row))
					{
						break;
					}
					string[] splitRow = row.Split(' ', StringSplitOptions.None);
					Vector2[] rowVectors = new Vector2[splitRow.Length];
					for (int j = 0; j < splitRow.Length; j++)
					{
						rowVectors[j] = XMLExtensions.ParseVector2(splitRow[j], true);
					}
					this.deformRows.Add(rowVectors);
					i++;
				}
			}
			if (this.deformRows.Count<Vector2[]>() == 0 || this.deformRows.First<Vector2[]>() == null || this.deformRows.First<Vector2[]>().Length == 0)
			{
				return;
			}
			Vector2[,] configDeformation = new Vector2[this.deformRows.First<Vector2[]>().Length, this.deformRows.Count];
			for (int x = 0; x < configDeformation.GetLength(0); x++)
			{
				for (int y = 0; y < configDeformation.GetLength(1); y++)
				{
					configDeformation[x, y] = this.deformRows[y][x];
				}
			}
			int newWidth = base.Resolution.X;
			int newHeight = base.Resolution.Y;
			base.Deformation = MathUtils.ResizeVector2Array(configDeformation, newWidth, newHeight);
			this.flippedDeformation = new Vector2[base.Resolution.X, base.Resolution.Y];
			for (int x2 = 0; x2 < base.Resolution.X; x2++)
			{
				for (int y2 = 0; y2 < base.Resolution.Y; y2++)
				{
					this.flippedDeformation[x2, y2] = base.Deformation[base.Resolution.X - x2 - 1, y2];
				}
			}
		}

		// Token: 0x060047F4 RID: 18420 RVA: 0x00278ADC File Offset: 0x00276CDC
		protected override void GetDeformation(out Vector2[,] deformation, out float multiplier, bool flippedHorizontally, bool inverseY)
		{
			deformation = (flippedHorizontally ? this.flippedDeformation : base.Deformation);
			multiplier = ((this.CustomDeformationParams.Frequency <= 0f) ? this.CustomDeformationParams.Amplitude : ((float)Math.Sin((double)(inverseY ? (-(double)this.phase) : this.phase)) * this.CustomDeformationParams.Amplitude));
			multiplier *= base.Params.Strength;
		}

		// Token: 0x060047F5 RID: 18421 RVA: 0x00278B53 File Offset: 0x00276D53
		public override void Update(float deltaTime)
		{
			if (!base.Params.UseMovementSine)
			{
				this.phase += deltaTime * this.CustomDeformationParams.Frequency;
				this.phase %= 6.2831855f;
			}
		}

		// Token: 0x060047F6 RID: 18422 RVA: 0x00278B90 File Offset: 0x00276D90
		public override void Save(XElement element)
		{
			base.Save(element);
			for (int i = 0; i < this.deformRows.Count; i++)
			{
				element.Add(new XAttribute("row" + i.ToString(), string.Join(" ", from r in this.deformRows[i]
				select XMLExtensions.Vector2ToString(r))));
			}
		}

		// Token: 0x0400254E RID: 9550
		private readonly List<Vector2[]> deformRows = new List<Vector2[]>();

		// Token: 0x0400254F RID: 9551
		private readonly Vector2[,] flippedDeformation;

		// Token: 0x04002550 RID: 9552
		private float phase;
	}
}
