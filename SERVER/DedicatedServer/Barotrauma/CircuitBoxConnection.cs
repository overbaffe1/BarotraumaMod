using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000016 RID: 22
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class CircuitBoxConnection
	{
		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06000321 RID: 801 RVA: 0x0001877E File Offset: 0x0001697E
		public string Name
		{
			get
			{
				return this.Connection.Name;
			}
		}

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06000322 RID: 802
		public abstract bool IsOutput { get; }

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06000323 RID: 803 RVA: 0x0001878B File Offset: 0x0001698B
		// (set) Token: 0x06000324 RID: 804 RVA: 0x00018794 File Offset: 0x00016994
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.Rect.X = value.X - this.Rect.Width / 2f;
				this.Rect.Y = value.Y - this.Rect.Height / 2f;
				this.position = value;
			}
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06000325 RID: 805 RVA: 0x000187EE File Offset: 0x000169EE
		// (set) Token: 0x06000326 RID: 806 RVA: 0x000187F6 File Offset: 0x000169F6
		public float Length { get; private set; }

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000327 RID: 807 RVA: 0x00018800 File Offset: 0x00016A00
		public Vector2 AnchorPoint
		{
			get
			{
				return new Vector2(this.IsOutput ? (this.Rect.Right + 24f) : (this.Rect.Left - 24f), this.Rect.Center.Y);
			}
		}

		// Token: 0x06000328 RID: 808 RVA: 0x00018850 File Offset: 0x00016A50
		protected CircuitBoxConnection(Vector2 position, Connection connection, CircuitBox circuitBox)
		{
			this.Connection = connection;
			this.Rect.Width = (this.Rect.Height = CircuitBoxConnection.Size);
			this.Position = position;
			this.CircuitBox = circuitBox;
			this.InitProjSpecific(circuitBox);
		}

		// Token: 0x06000329 RID: 809 RVA: 0x000188A8 File Offset: 0x00016AA8
		private void InitProjSpecific(CircuitBox circuitBox)
		{
			this.Length = 100f;
		}

		// Token: 0x0600032A RID: 810
		public abstract void ReceiveSignal(Signal signal);

		// Token: 0x0600032B RID: 811 RVA: 0x000188B8 File Offset: 0x00016AB8
		public bool Contains(Vector2 pos)
		{
			float x = this.Rect.X;
			float y = -(this.Rect.Y + this.Rect.Height);
			float width = this.Rect.Width;
			float height = this.Rect.Height;
			RectangleF rect = new RectangleF(x, y, width, height);
			return rect.Contains(pos);
		}

		// Token: 0x04000179 RID: 377
		public readonly Connection Connection;

		// Token: 0x0400017A RID: 378
		public RectangleF Rect;

		// Token: 0x0400017B RID: 379
		private Vector2 position;

		// Token: 0x0400017C RID: 380
		public readonly List<CircuitBoxConnection> ExternallyConnectedFrom = new List<CircuitBoxConnection>();

		// Token: 0x0400017D RID: 381
		public static readonly float Size = 32f;

		// Token: 0x0400017F RID: 383
		public readonly CircuitBox CircuitBox;
	}
}
