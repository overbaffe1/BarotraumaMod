using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000011 RID: 17
	public class Camera
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000054 RID: 84 RVA: 0x000040BC File Offset: 0x000022BC
		// (set) Token: 0x06000055 RID: 85 RVA: 0x000040C4 File Offset: 0x000022C4
		public float Zoom
		{
			get
			{
				return this.zoom;
			}
			set
			{
				this.zoom = value;
				Vector2 center = this.WorldViewCenter;
				float newWidth = (float)this.resolution.X / this.zoom;
				float newHeight = (float)this.resolution.Y / this.zoom;
				this.worldView = new Rectangle((int)(center.X - newWidth / 2f), (int)(center.Y + newHeight / 2f), (int)newWidth, (int)newHeight);
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000056 RID: 86 RVA: 0x00004134 File Offset: 0x00002334
		// (set) Token: 0x06000057 RID: 87 RVA: 0x0000413C File Offset: 0x0000233C
		public float Rotation
		{
			get
			{
				return this.rotation;
			}
			set
			{
				this.rotation = value;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000058 RID: 88 RVA: 0x00004145 File Offset: 0x00002345
		// (set) Token: 0x06000059 RID: 89 RVA: 0x0000414D File Offset: 0x0000234D
		public float OffsetAmount
		{
			get
			{
				return this.offsetAmount;
			}
			set
			{
				this.offsetAmount = value;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600005A RID: 90 RVA: 0x00004156 File Offset: 0x00002356
		public Point Resolution
		{
			get
			{
				return this.resolution;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600005B RID: 91 RVA: 0x0000415E File Offset: 0x0000235E
		public Rectangle WorldView
		{
			get
			{
				return this.worldView;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600005C RID: 92 RVA: 0x00004168 File Offset: 0x00002368
		public Vector2 WorldViewCenter
		{
			get
			{
				return new Vector2((float)this.worldView.X + (float)this.worldView.Width / 2f, (float)this.worldView.Y - (float)this.worldView.Height / 2f);
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600005D RID: 93 RVA: 0x000041B8 File Offset: 0x000023B8
		public Matrix Transform
		{
			get
			{
				return this.transform;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600005E RID: 94 RVA: 0x000041C0 File Offset: 0x000023C0
		public Matrix ShaderTransform
		{
			get
			{
				return this.shaderTransform;
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000041C8 File Offset: 0x000023C8
		public Camera()
		{
			this.zoom = 1f;
			this.rotation = 0f;
			this.position = Vector2.Zero;
			this.worldView = new Rectangle(0, 0, 1, 1);
			this.resolution = new Point(1, 1);
			this.viewMatrix = Matrix.CreateTranslation(new Vector3(0.5f, 0.5f, 0f));
			this.UpdateTransform(true, false);
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000060 RID: 96 RVA: 0x0000423F File Offset: 0x0000243F
		// (set) Token: 0x06000061 RID: 97 RVA: 0x00004247 File Offset: 0x00002447
		public Vector2 TargetPos
		{
			get
			{
				return this.targetPos;
			}
			set
			{
				this.targetPos = value;
			}
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004250 File Offset: 0x00002450
		public Vector2 GetPosition()
		{
			return this.position;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00004258 File Offset: 0x00002458
		public void Translate(Vector2 amount)
		{
			this.position += amount;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x0000426C File Offset: 0x0000246C
		public void UpdateTransform(bool interpolate = true, bool clampPos = false)
		{
			Vector2 interpolatedPosition = interpolate ? Timing.Interpolate(this.prevPosition, this.position) : this.position;
			float interpolatedZoom = interpolate ? Timing.Interpolate(this.prevZoom, this.zoom) : this.zoom;
			this.worldView.X = (int)((double)interpolatedPosition.X - (double)this.worldView.Width / 2.0);
			this.worldView.Y = (int)((double)interpolatedPosition.Y + (double)this.worldView.Height / 2.0);
			if (Level.Loaded != null && clampPos)
			{
				this.position.Y = this.position.Y - Math.Max((float)(this.worldView.Y - Level.Loaded.Size.Y), 0f);
				interpolatedPosition.Y -= Math.Max((float)(this.worldView.Y - Level.Loaded.Size.Y), 0f);
				this.worldView.Y = Math.Min(Level.Loaded.Size.Y, this.worldView.Y);
			}
			this.transform = Matrix.CreateTranslation(new Vector3(-interpolatedPosition.X, interpolatedPosition.Y, 0f)) * Matrix.CreateScale(new Vector3(interpolatedZoom, interpolatedZoom, 1f)) * this.viewMatrix;
			this.shaderTransform = Matrix.CreateTranslation(new Vector3(-interpolatedPosition.X - (float)this.resolution.X / interpolatedZoom / 2f, -interpolatedPosition.Y - (float)this.resolution.Y / interpolatedZoom / 2f, 0f)) * Matrix.CreateScale(new Vector3(interpolatedZoom, interpolatedZoom, 1f)) * this.viewMatrix;
			if (!interpolate)
			{
				this.prevPosition = this.position;
				this.prevZoom = this.zoom;
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00004472 File Offset: 0x00002672
		public void MoveCamera(float deltaTime, bool allowMove = true, bool allowZoom = true)
		{
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00004474 File Offset: 0x00002674
		// (set) Token: 0x06000067 RID: 103 RVA: 0x0000447C File Offset: 0x0000267C
		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.position = value;
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00004490 File Offset: 0x00002690
		public Vector2 ScreenToWorld(Vector2 coords)
		{
			Vector2 worldCoords = Vector2.Transform(coords, Matrix.Invert(this.transform));
			return new Vector2(worldCoords.X, -worldCoords.Y);
		}

		// Token: 0x06000069 RID: 105 RVA: 0x000044C1 File Offset: 0x000026C1
		public Vector2 WorldToScreen(Vector2 coords)
		{
			coords.Y = -coords.Y;
			return Vector2.Transform(coords, this.transform);
		}

		// Token: 0x0400005C RID: 92
		public static Camera Instance = new Camera();

		// Token: 0x0400005D RID: 93
		public static bool FollowSub = true;

		// Token: 0x0400005E RID: 94
		private const float DefaultZoom = 1f;

		// Token: 0x0400005F RID: 95
		private const float ZoomSmoothness = 8f;

		// Token: 0x04000060 RID: 96
		private const float MoveSmoothness = 8f;

		// Token: 0x04000061 RID: 97
		private float zoom;

		// Token: 0x04000062 RID: 98
		private float offsetAmount;

		// Token: 0x04000063 RID: 99
		private Matrix transform;

		// Token: 0x04000064 RID: 100
		private Matrix shaderTransform;

		// Token: 0x04000065 RID: 101
		private Matrix viewMatrix;

		// Token: 0x04000066 RID: 102
		private Vector2 position;

		// Token: 0x04000067 RID: 103
		private float rotation;

		// Token: 0x04000068 RID: 104
		private Vector2 prevPosition;

		// Token: 0x04000069 RID: 105
		private float prevZoom;

		// Token: 0x0400006A RID: 106
		public float Shake;

		// Token: 0x0400006B RID: 107
		private Vector2 shakePosition;

		// Token: 0x0400006C RID: 108
		private Vector2 shakeTargetPosition;

		// Token: 0x0400006D RID: 109
		private Rectangle worldView;

		// Token: 0x0400006E RID: 110
		private Point resolution;

		// Token: 0x0400006F RID: 111
		private Vector2 targetPos;
	}
}
