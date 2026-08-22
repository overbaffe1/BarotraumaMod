using System;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200001C RID: 28
	internal class Camera
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000110 RID: 272 RVA: 0x00006B9C File Offset: 0x00004D9C
		// (set) Token: 0x06000111 RID: 273 RVA: 0x00006BD8 File Offset: 0x00004DD8
		public float DefaultZoom
		{
			get
			{
				float? num = this.defaultZoom;
				if (num != null)
				{
					return num.GetValueOrDefault();
				}
				if (!GameSettings.CurrentConfig.EnableMouseLook)
				{
					return 1f;
				}
				return 1.3f;
			}
			set
			{
				this.defaultZoom = new float?(MathHelper.Clamp(value, 0.5f, 2f));
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00006BF5 File Offset: 0x00004DF5
		// (set) Token: 0x06000113 RID: 275 RVA: 0x00006BFD File Offset: 0x00004DFD
		public float ZoomSmoothness
		{
			get
			{
				return this.zoomSmoothness;
			}
			set
			{
				this.zoomSmoothness = Math.Max(value, 0.01f);
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00006C10 File Offset: 0x00004E10
		// (set) Token: 0x06000115 RID: 277 RVA: 0x00006C18 File Offset: 0x00004E18
		public float MoveSmoothness
		{
			get
			{
				return this.moveSmoothness;
			}
			set
			{
				this.moveSmoothness = Math.Max(value, 0.01f);
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000116 RID: 278 RVA: 0x00006C2B File Offset: 0x00004E2B
		// (set) Token: 0x06000117 RID: 279 RVA: 0x00006C33 File Offset: 0x00004E33
		public float MinZoom
		{
			get
			{
				return this.minZoom;
			}
			set
			{
				this.minZoom = MathHelper.Clamp(value, 0.001f, 10f);
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000118 RID: 280 RVA: 0x00006C4B File Offset: 0x00004E4B
		// (set) Token: 0x06000119 RID: 281 RVA: 0x00006C53 File Offset: 0x00004E53
		public float MaxZoom
		{
			get
			{
				return this.maxZoom;
			}
			set
			{
				this.maxZoom = MathHelper.Clamp(value, 1f, 10f);
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00006C6B File Offset: 0x00004E6B
		// (set) Token: 0x0600011B RID: 283 RVA: 0x00006C73 File Offset: 0x00004E73
		public Vector2 ShakePosition { get; private set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600011C RID: 284 RVA: 0x00006C7C File Offset: 0x00004E7C
		// (set) Token: 0x0600011D RID: 285 RVA: 0x00006C84 File Offset: 0x00004E84
		public float Zoom
		{
			get
			{
				return this.zoom;
			}
			set
			{
				this.zoom = MathHelper.Clamp(value, GameMain.DebugDraw ? 0.01f : this.MinZoom, this.MaxZoom);
				Vector2 center = this.WorldViewCenter;
				float newWidth = (float)this.Resolution.X / this.zoom;
				float newHeight = (float)this.Resolution.Y / this.zoom;
				this.WorldView = new Rectangle((int)(center.X - newWidth / 2f), (int)(center.Y + newHeight / 2f), (int)newWidth, (int)newHeight);
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600011E RID: 286 RVA: 0x00006D13 File Offset: 0x00004F13
		// (set) Token: 0x0600011F RID: 287 RVA: 0x00006D1B File Offset: 0x00004F1B
		public float Rotation
		{
			get
			{
				return this.rotation;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.rotation = value;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000120 RID: 288 RVA: 0x00006D2D File Offset: 0x00004F2D
		// (set) Token: 0x06000121 RID: 289 RVA: 0x00006D35 File Offset: 0x00004F35
		public float AngularVelocity
		{
			get
			{
				return this.angularVelocity;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.angularVelocity = value;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000122 RID: 290 RVA: 0x00006D47 File Offset: 0x00004F47
		// (set) Token: 0x06000123 RID: 291 RVA: 0x00006D4F File Offset: 0x00004F4F
		public float OffsetAmount { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000124 RID: 292 RVA: 0x00006D58 File Offset: 0x00004F58
		// (set) Token: 0x06000125 RID: 293 RVA: 0x00006D60 File Offset: 0x00004F60
		public Point Resolution { get; private set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000126 RID: 294 RVA: 0x00006D69 File Offset: 0x00004F69
		// (set) Token: 0x06000127 RID: 295 RVA: 0x00006D71 File Offset: 0x00004F71
		public Rectangle WorldView { get; private set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000128 RID: 296 RVA: 0x00006D7C File Offset: 0x00004F7C
		public Vector2 WorldViewCenter
		{
			get
			{
				return new Vector2((float)this.WorldView.X + (float)this.WorldView.Width / 2f, (float)this.WorldView.Y - (float)this.WorldView.Height / 2f);
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000129 RID: 297 RVA: 0x00006DCC File Offset: 0x00004FCC
		public Matrix Transform
		{
			get
			{
				return this.transform;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600012A RID: 298 RVA: 0x00006DD4 File Offset: 0x00004FD4
		public Matrix ShaderTransform
		{
			get
			{
				return this.shaderTransform;
			}
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00006DDC File Offset: 0x00004FDC
		public Camera()
		{
			this.zoom = (this.prevZoom = (this.targetZoom = 1f));
			this.rotation = 0f;
			this.position = Vector2.Zero;
			this.CreateMatrices();
			this.UpdateTransform(false, true);
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x0600012C RID: 300 RVA: 0x00006E79 File Offset: 0x00005079
		// (set) Token: 0x0600012D RID: 301 RVA: 0x00006E81 File Offset: 0x00005081
		public Vector2 TargetPos { get; set; }

		// Token: 0x0600012E RID: 302 RVA: 0x00006E8A File Offset: 0x0000508A
		public Vector2 GetPosition()
		{
			return this.position;
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00006E92 File Offset: 0x00005092
		public void Translate(Vector2 amount)
		{
			this.position += amount;
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00006EA6 File Offset: 0x000050A6
		public void ClientWrite(in SegmentTableWriter<ClientNetSegment> segmentTableWriter, IWriteMessage msg)
		{
			if (Character.Controlled != null && !Character.Controlled.IsDead)
			{
				return;
			}
			segmentTableWriter.StartNewSegment(ClientNetSegment.SpectatingPos);
			msg.WriteSingle(this.position.X);
			msg.WriteSingle(this.position.Y);
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00006EE5 File Offset: 0x000050E5
		private void CreateMatrices()
		{
			this.SetResolution(new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight));
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00006EFC File Offset: 0x000050FC
		public void SetResolution(Point res)
		{
			this.Resolution = res;
			this.WorldView = new Rectangle(0, 0, res.X, res.Y);
			this.viewMatrix = Matrix.CreateTranslation(new Vector3((float)res.X / 2f, (float)res.Y / 2f, 0f));
			float newGlobalZoomScale = new Vector2((float)GUI.UIWidth, (float)this.Resolution.Y).Length() / GUI.ReferenceResolution.Length();
			if (this.globalZoomScale > 0f)
			{
				this.Zoom *= newGlobalZoomScale / this.globalZoomScale;
				this.targetZoom *= newGlobalZoomScale / this.globalZoomScale;
				this.prevZoom *= newGlobalZoomScale / this.globalZoomScale;
			}
			this.globalZoomScale = newGlobalZoomScale;
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00006FDC File Offset: 0x000051DC
		public void UpdateTransform(bool interpolate = true, bool updateListener = true)
		{
			if (this.AutoUpdateToScreenResolution && (GameMain.GraphicsWidth != this.Resolution.X || GameMain.GraphicsHeight != this.Resolution.Y))
			{
				this.CreateMatrices();
			}
			Vector2 interpolatedPosition = interpolate ? Timing.Interpolate(this.prevPosition, this.position) : this.position;
			float interpolatedZoom = interpolate ? Timing.Interpolate(this.prevZoom, this.zoom) : this.zoom;
			this.WorldView = new Rectangle((int)((double)interpolatedPosition.X - (double)this.WorldView.Width / 2.0), (int)((double)interpolatedPosition.Y + (double)this.WorldView.Height / 2.0), this.WorldView.Width, this.WorldView.Height);
			this.transform = Matrix.CreateTranslation(new Vector3(-interpolatedPosition.X, interpolatedPosition.Y, 0f)) * Matrix.CreateScale(new Vector3(interpolatedZoom, interpolatedZoom, 1f)) * Matrix.CreateRotationZ(this.rotation) * this.viewMatrix;
			this.shaderTransform = Matrix.CreateTranslation(new Vector3(-interpolatedPosition.X - (float)this.Resolution.X / interpolatedZoom / 2f, -interpolatedPosition.Y - (float)this.Resolution.Y / interpolatedZoom / 2f, 0f)) * Matrix.CreateScale(new Vector3(interpolatedZoom, interpolatedZoom, 1f)) * this.viewMatrix * Matrix.CreateRotationZ(-this.rotation);
			if (updateListener)
			{
				if (Character.Controlled == null)
				{
					GameMain.SoundManager.ListenerPosition = new Vector3(this.WorldViewCenter.X, this.WorldViewCenter.Y, -(100f / this.zoom));
				}
				else
				{
					GameMain.SoundManager.ListenerPosition = new Vector3(Character.Controlled.WorldPosition.X, Character.Controlled.WorldPosition.Y, -(100f / this.zoom));
				}
			}
			if (!interpolate)
			{
				this.prevPosition = this.position;
				this.prevZoom = this.zoom;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000134 RID: 308 RVA: 0x00007219 File Offset: 0x00005419
		// (set) Token: 0x06000135 RID: 309 RVA: 0x00007221 File Offset: 0x00005421
		public bool Freeze { get; set; }

		// Token: 0x06000136 RID: 310 RVA: 0x0000722C File Offset: 0x0000542C
		public void MoveCamera(float deltaTime, bool allowMove = true, bool allowZoom = true, bool allowInput = true, bool? followSub = null)
		{
			this.prevPosition = this.position;
			this.prevZoom = this.zoom;
			float moveSpeed = 20f / this.zoom;
			this.MovementLockTimer -= deltaTime;
			Vector2 moveCam = Vector2.Zero;
			if (this.TargetPos == Vector2.Zero)
			{
				Vector2 moveInput = Vector2.Zero;
				if (allowMove && !this.Freeze)
				{
					if (GUI.KeyboardDispatcher.Subscriber == null && allowInput && this.MovementLockTimer <= 0f)
					{
						if (PlayerInput.KeyDown(Keys.LeftShift))
						{
							moveSpeed *= 2f;
						}
						if (PlayerInput.KeyDown(Keys.LeftControl))
						{
							moveSpeed *= 0.5f;
						}
						if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Left].IsDown())
						{
							moveInput.X -= 1f;
						}
						if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Right].IsDown())
						{
							moveInput.X += 1f;
						}
						if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Down].IsDown())
						{
							moveInput.Y -= 1f;
						}
						if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Up].IsDown())
						{
							moveInput.Y += 1f;
						}
					}
					this.velocity = Vector2.Lerp(this.velocity, moveInput, deltaTime * 10f);
					moveCam = this.velocity * moveSpeed * deltaTime * this.FreeCamMoveSpeed * 60f;
					if (Screen.Selected == GameMain.GameScreen && (followSub ?? Camera.FollowSub))
					{
						GameMain instance = GameMain.Instance;
						if (instance == null || !instance.Paused)
						{
							Submarine closestSub = Submarine.FindClosest(this.WorldViewCenter, false, true, false, null);
							if (closestSub != null)
							{
								moveCam += ConvertUnits.ToDisplayUnits(closestSub.Velocity * deltaTime);
							}
						}
					}
				}
				if (allowZoom)
				{
					Vector2 mouseInWorld = this.ScreenToWorld(PlayerInput.MousePosition);
					Vector2 diffViewCenter = (mouseInWorld - this.Position) * this.Zoom;
					this.targetZoom = MathHelper.Clamp(this.targetZoom + (float)PlayerInput.ScrollWheelSpeed / 1000f * this.zoom, GameMain.DebugDraw ? (this.MinZoom * 0.1f) : this.MinZoom, this.MaxZoom);
					if (PlayerInput.KeyDown(Keys.LeftControl))
					{
						this.Zoom += (this.targetZoom - this.zoom) / (this.ZoomSmoothness * 10f);
					}
					else
					{
						this.Zoom = MathHelper.Lerp(this.Zoom, this.targetZoom, deltaTime * 10f);
					}
					if (!PlayerInput.KeyDown(Keys.F))
					{
						this.Position = mouseInWorld - diffViewCenter / this.Zoom;
					}
				}
			}
			else if (allowMove)
			{
				Vector2 mousePos = PlayerInput.MousePosition;
				Vector2 offset = mousePos - this.Resolution.ToVector2() / 2f;
				offset.X /= (float)this.Resolution.X * 0.4f;
				offset.Y = -offset.Y / ((float)this.Resolution.Y * 0.3f);
				if (offset.LengthSquared() > 1f)
				{
					offset.Normalize();
				}
				float offsetUnscaledLen = offset.Length();
				offset *= this.OffsetAmount;
				if (GUI.MouseOn != null && this.OffsetAmount > 0f)
				{
					this.Freeze = true;
				}
				if (CharacterHealth.OpenHealthWindow != null || CrewManager.IsCommandInterfaceOpen || ConversationAction.IsDialogOpen)
				{
					offset *= 0f;
					this.Freeze = false;
				}
				if (this.Freeze)
				{
					if (offset.LengthSquared() > 0.001f)
					{
						offset = this.previousOffset;
					}
				}
				else
				{
					this.previousOffset = offset;
				}
				if (allowZoom)
				{
					float zoomOutAmount = this.GetZoomAmount(offset);
					float scaledZoom = MathHelper.Lerp(this.DefaultZoom, this.MinZoom, zoomOutAmount) * this.globalZoomScale;
					float newZoom = scaledZoom * (MathHelper.Lerp(0.3f * (1f - Math.Min(this.globalZoomScale, 1f)), 0f, GameSettings.CurrentConfig.EnableMouseLook ? ((float)Math.Sqrt((double)offsetUnscaledLen)) : 0.3f) + 1f);
					this.Zoom += (newZoom - this.zoom) / this.ZoomSmoothness;
				}
				this.targetZoom = this.Zoom;
				Vector2 diff = this.TargetPos + offset - this.position;
				moveCam = diff / this.MoveSmoothness;
			}
			this.rotation += this.angularVelocity * deltaTime;
			this.angularVelocity *= 1f - this.angularDamping;
			this.angularVelocity += -this.rotation * this.angularSpring;
			this.angularDamping = 0.05f;
			this.angularSpring = 0.2f;
			if (this.Shake < 0.01f)
			{
				this.ShakePosition = Vector2.Zero;
				this.shakeTimer = 0f;
			}
			else
			{
				this.shakeTimer += deltaTime * 5f;
				Vector2 noisePos = new Vector2((float)PerlinNoise.CalculatePerlin((double)this.shakeTimer, (double)this.shakeTimer, 0.0) - 0.5f, (float)PerlinNoise.CalculatePerlin((double)this.shakeTimer, (double)this.shakeTimer, 0.5) - 0.5f);
				this.ShakePosition = noisePos * this.Shake * 2f;
				this.Shake = MathHelper.Lerp(this.Shake, 0f, deltaTime * 2f);
			}
			this.Translate(moveCam + this.ShakePosition);
			this.Freeze = false;
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00007851 File Offset: 0x00005A51
		public void StopMovement()
		{
			this.targetZoom = this.zoom;
			this.velocity = Vector2.Zero;
			this.angularVelocity = 0f;
			this.rotation = 0f;
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000138 RID: 312 RVA: 0x00007880 File Offset: 0x00005A80
		// (set) Token: 0x06000139 RID: 313 RVA: 0x00007888 File Offset: 0x00005A88
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

		// Token: 0x0600013A RID: 314 RVA: 0x0000789C File Offset: 0x00005A9C
		public Vector2 ScreenToWorld(Vector2 coords)
		{
			Vector2 worldCoords = Vector2.Transform(coords, Matrix.Invert(this.transform));
			return new Vector2(worldCoords.X, -worldCoords.Y);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x000078CD File Offset: 0x00005ACD
		public Vector2 WorldToScreen(Vector2 coords)
		{
			coords.Y = -coords.Y;
			return Vector2.Transform(coords, this.transform);
		}

		// Token: 0x0600013C RID: 316 RVA: 0x000078E9 File Offset: 0x00005AE9
		private float GetZoomAmount(Vector2 offset)
		{
			return Math.Min(offset.Length() / 1000f, 1f);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00007902 File Offset: 0x00005B02
		public float GetZoomAmountFromPrevious()
		{
			return this.GetZoomAmount(this.previousOffset);
		}

		// Token: 0x040000CB RID: 203
		public static bool FollowSub = true;

		// Token: 0x040000CC RID: 204
		private float? defaultZoom;

		// Token: 0x040000CD RID: 205
		private float zoomSmoothness = 8f;

		// Token: 0x040000CE RID: 206
		private float moveSmoothness = 8f;

		// Token: 0x040000CF RID: 207
		private float minZoom = 0.1f;

		// Token: 0x040000D0 RID: 208
		private float maxZoom = 2f;

		// Token: 0x040000D1 RID: 209
		public float FreeCamMoveSpeed = 1f;

		// Token: 0x040000D2 RID: 210
		private float zoom;

		// Token: 0x040000D3 RID: 211
		private Matrix transform;

		// Token: 0x040000D4 RID: 212
		private Matrix shaderTransform;

		// Token: 0x040000D5 RID: 213
		private Matrix viewMatrix;

		// Token: 0x040000D6 RID: 214
		private Vector2 position;

		// Token: 0x040000D7 RID: 215
		private float rotation;

		// Token: 0x040000D8 RID: 216
		private float angularVelocity;

		// Token: 0x040000D9 RID: 217
		private float angularDamping;

		// Token: 0x040000DA RID: 218
		private float angularSpring;

		// Token: 0x040000DB RID: 219
		private Vector2 prevPosition;

		// Token: 0x040000DC RID: 220
		private float prevZoom;

		// Token: 0x040000DD RID: 221
		public float Shake;

		// Token: 0x040000DE RID: 222
		public bool AutoUpdateToScreenResolution = true;

		// Token: 0x040000E0 RID: 224
		private float shakeTimer;

		// Token: 0x040000E1 RID: 225
		public float MovementLockTimer;

		// Token: 0x040000E2 RID: 226
		private float globalZoomScale = 1f;

		// Token: 0x040000E3 RID: 227
		private float targetZoom;

		// Token: 0x040000E4 RID: 228
		private Vector2 velocity;

		// Token: 0x040000E9 RID: 233
		private Vector2 previousOffset;
	}
}
