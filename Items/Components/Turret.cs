using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E3 RID: 1507
	internal class Turret : Powered, IDrawableComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x170018C8 RID: 6344
		// (get) Token: 0x06006229 RID: 25129 RVA: 0x00330394 File Offset: 0x0032E594
		public int UIElementHeight
		{
			get
			{
				int height = 0;
				if (this.ShowChargeIndicator)
				{
					height += this.powerIndicator.Rect.Height;
				}
				if (this.ShowProjectileIndicator)
				{
					height += (int)(Inventory.SlotSpriteSmall.size.Y * Inventory.UIScale) + 5;
				}
				return height;
			}
		}

		// Token: 0x170018C9 RID: 6345
		// (get) Token: 0x0600622A RID: 25130 RVA: 0x003303E2 File Offset: 0x0032E5E2
		private float RetractionTime
		{
			get
			{
				return Math.Max(this.Reload * this.RetractionDurationMultiplier, this.RecoilTime);
			}
		}

		// Token: 0x170018CA RID: 6346
		// (get) Token: 0x0600622B RID: 25131 RVA: 0x003303FC File Offset: 0x0032E5FC
		// (set) Token: 0x0600622C RID: 25132 RVA: 0x00330404 File Offset: 0x0032E604
		[Serialize(false, IsPropertySaveable.No, "Should the charge of the connected batteries/supercapacitors be shown at the top of the screen when operating the item.", "", false)]
		public bool ShowChargeIndicator { get; private set; }

		// Token: 0x170018CB RID: 6347
		// (get) Token: 0x0600622D RID: 25133 RVA: 0x0033040D File Offset: 0x0032E60D
		// (set) Token: 0x0600622E RID: 25134 RVA: 0x00330415 File Offset: 0x0032E615
		[Serialize(false, IsPropertySaveable.No, "Should the available ammunition be shown at the top of the screen when operating the item.", "", false)]
		public bool ShowProjectileIndicator { get; private set; }

		// Token: 0x170018CC RID: 6348
		// (get) Token: 0x0600622F RID: 25135 RVA: 0x0033041E File Offset: 0x0032E61E
		// (set) Token: 0x06006230 RID: 25136 RVA: 0x00330426 File Offset: 0x0032E626
		[Serialize(0f, IsPropertySaveable.No, "How far the barrel \"recoils back\" when the turret is fired (in pixels).", "", false)]
		public float RecoilDistance { get; private set; }

		// Token: 0x170018CD RID: 6349
		// (get) Token: 0x06006231 RID: 25137 RVA: 0x0033042F File Offset: 0x0032E62F
		// (set) Token: 0x06006232 RID: 25138 RVA: 0x00330437 File Offset: 0x0032E637
		[Serialize(0f, IsPropertySaveable.No, "The distance in which the spinning barrels rotate. Only used if spinning barrels are created.", "", false)]
		public float SpinningBarrelDistance { get; private set; }

		// Token: 0x170018CE RID: 6350
		// (get) Token: 0x06006233 RID: 25139 RVA: 0x00330440 File Offset: 0x0032E640
		public Vector2 DrawSize
		{
			get
			{
				float size = Math.Max(this.transformedBarrelPos.X, this.transformedBarrelPos.Y);
				if (this.railSprite != null && this.barrelSprite != null)
				{
					size += Math.Max(Math.Max(this.barrelSprite.size.X, this.barrelSprite.size.Y), Math.Max(this.railSprite.size.X, this.railSprite.size.Y)) * this.item.Scale;
				}
				else if (this.railSprite != null)
				{
					size += Math.Max(this.railSprite.size.X, this.railSprite.size.Y) * this.item.Scale;
				}
				else if (this.barrelSprite != null)
				{
					size += Math.Max(this.barrelSprite.size.X, this.barrelSprite.size.Y) * this.item.Scale;
				}
				return Vector2.One * size * 2f;
			}
		}

		// Token: 0x170018CF RID: 6351
		// (get) Token: 0x06006234 RID: 25140 RVA: 0x00330568 File Offset: 0x0032E768
		public Sprite BarrelSprite
		{
			get
			{
				return this.barrelSprite;
			}
		}

		// Token: 0x170018D0 RID: 6352
		// (get) Token: 0x06006235 RID: 25141 RVA: 0x00330570 File Offset: 0x0032E770
		// (set) Token: 0x06006236 RID: 25142 RVA: 0x00330578 File Offset: 0x0032E778
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool HideBarrelWhenBroken { get; private set; }

		// Token: 0x170018D1 RID: 6353
		// (get) Token: 0x06006237 RID: 25143 RVA: 0x00330581 File Offset: 0x0032E781
		// (set) Token: 0x06006238 RID: 25144 RVA: 0x00330589 File Offset: 0x0032E789
		[Serialize("0.5, 1.5", IsPropertySaveable.No, "Pitch slides from X to Y over the charge time", "", false)]
		public Vector2 ChargeSoundWindupPitchSlide
		{
			get
			{
				return this._chargeSoundWindupPitchSlide;
			}
			set
			{
				this._chargeSoundWindupPitchSlide = new Vector2(Math.Max(value.X, 0.25f), Math.Min(value.Y, 4f));
			}
		}

		// Token: 0x06006239 RID: 25145 RVA: 0x003305B6 File Offset: 0x0032E7B6
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			this.widgets.Clear();
		}

		// Token: 0x0600623A RID: 25146 RVA: 0x003305C3 File Offset: 0x0032E7C3
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.recoilTimer -= deltaTime;
		}

		// Token: 0x0600623B RID: 25147 RVA: 0x003305DC File Offset: 0x0032E7DC
		public override void UpdateEditing(float deltaTime)
		{
			if (Screen.Selected == GameMain.SubEditorScreen && this.item.IsSelected)
			{
				if (this.widgets.ContainsKey("maxrotation"))
				{
					this.widgets["maxrotation"].Update(deltaTime);
				}
				if (this.widgets.ContainsKey("minrotation"))
				{
					this.widgets["minrotation"].Update(deltaTime);
				}
			}
		}

		// Token: 0x0600623C RID: 25148 RVA: 0x00330654 File Offset: 0x0032E854
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (this.crosshairSprite != null)
			{
				Vector2 itemPos = cam.WorldToScreen(this.item.WorldPosition);
				Vector2 turretDir = new Vector2((float)Math.Cos((double)this.Rotation), (float)Math.Sin((double)this.Rotation));
				Vector2 mouseDiff = itemPos - PlayerInput.MousePosition;
				this.crosshairPos = new Vector2(MathHelper.Clamp(itemPos.X + turretDir.X * mouseDiff.Length(), 0f, (float)GameMain.GraphicsWidth), MathHelper.Clamp(itemPos.Y + turretDir.Y * mouseDiff.Length(), 0f, (float)GameMain.GraphicsHeight));
			}
			this.crosshairPointerPos = PlayerInput.MousePosition;
		}

		// Token: 0x0600623D RID: 25149 RVA: 0x0033070C File Offset: 0x0032E90C
		public Vector2 GetRecoilOffset()
		{
			float recoilOffset = 0f;
			if (Math.Abs(this.RecoilDistance) > 0f && this.recoilTimer > 0f)
			{
				float diff = this.RetractionTime - this.RecoilTime;
				if (this.recoilTimer >= diff)
				{
					recoilOffset = this.RecoilDistance * (1f - (this.recoilTimer - diff) / this.RecoilTime);
				}
				else if (this.recoilTimer <= diff - this.RetractionDelay)
				{
					float t = diff - this.RetractionDelay;
					recoilOffset = this.RecoilDistance * this.recoilTimer / t;
				}
				else
				{
					recoilOffset = this.RecoilDistance;
				}
			}
			return new Vector2((float)Math.Cos((double)this.Rotation), (float)Math.Sin((double)this.Rotation)) * recoilOffset;
		}

		// Token: 0x0600623E RID: 25150 RVA: 0x003307CC File Offset: 0x0032E9CC
		public void Draw(SpriteBatch spriteBatch, bool editing = false, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (!MathUtils.NearlyEqual(this.item.Rotation, this.prevBaseRotation, 0.0001f) || !MathUtils.NearlyEqual(this.item.Scale, this.prevScale, 0.0001f))
			{
				this.UpdateTransformedBarrelPos();
			}
			Vector2 drawPos = this.GetDrawPos();
			if (this.item.Condition > 0f || !this.HideBarrelWhenBroken)
			{
				Sprite currentRailSprite = (this.item.Condition <= 0f && this.railSpriteBroken != null) ? this.railSpriteBroken : this.railSprite;
				Sprite currentBarrelSprite = (this.item.Condition <= 0f && this.barrelSpriteBroken != null) ? this.barrelSpriteBroken : this.barrelSprite;
				if (currentRailSprite != null)
				{
					currentRailSprite.Draw(spriteBatch, drawPos, overrideColor ?? this.item.SpriteColor, this.Rotation + 1.5707964f, this.item.Scale, SpriteEffects.None, new float?(this.item.SpriteDepth + (currentRailSprite.Depth - this.item.Sprite.Depth)));
				}
				if (currentBarrelSprite != null)
				{
					currentBarrelSprite.Draw(spriteBatch, drawPos - this.GetRecoilOffset() * this.item.Scale, overrideColor ?? this.item.SpriteColor, this.Rotation + 1.5707964f, this.item.Scale, SpriteEffects.None, new float?(this.item.SpriteDepth + (currentBarrelSprite.Depth - this.item.Sprite.Depth)));
				}
				float chargeRatio = this.currentChargeTime / this.MaxChargeTime;
				foreach (ValueTuple<Sprite, Vector2> valueTuple in this.chargeSprites)
				{
					Sprite chargeSprite = valueTuple.Item1;
					Vector2 position = valueTuple.Item2;
					if (chargeSprite != null)
					{
						chargeSprite.Draw(spriteBatch, drawPos - MathUtils.RotatePoint(new Vector2(position.X * chargeRatio, position.Y * chargeRatio) * this.item.Scale, this.Rotation + 1.5707964f), this.item.SpriteColor, this.Rotation + 1.5707964f, this.item.Scale, SpriteEffects.None, new float?(this.item.SpriteDepth + (chargeSprite.Depth - this.item.Sprite.Depth)));
					}
				}
				int spinningBarrelCount = this.spinningBarrelSprites.Count;
				for (int i = 0; i < spinningBarrelCount; i++)
				{
					Sprite spinningBarrel = this.spinningBarrelSprites[i];
					float barrelCirclePosition = (360f * (float)i / (float)spinningBarrelCount + this.currentBarrelSpin) % 360f;
					float newDepth = this.item.SpriteDepth + (spinningBarrel.Depth - this.item.Sprite.Depth) + ((barrelCirclePosition > 180f) ? 0f : 0.001f);
					float barrelColorPosition = (barrelCirclePosition + 90f) % 360f;
					float colorOffset = Math.Abs(barrelColorPosition - 180f) / 180f;
					Color newColorModifier = Color.Lerp(Color.Black, Color.Gray, colorOffset);
					float barrelHalfCirclePosition = Math.Abs(barrelCirclePosition - 180f);
					float barrelPositionModifier = MathUtils.SmoothStep(barrelHalfCirclePosition / 180f);
					float newPositionOffset = barrelPositionModifier * this.SpinningBarrelDistance;
					spinningBarrel.Draw(spriteBatch, drawPos - MathUtils.RotatePoint(new Vector2(newPositionOffset, 0f) * this.item.Scale, this.Rotation + 1.5707964f), Color.Lerp(overrideColor ?? this.item.SpriteColor, newColorModifier, 0.8f), this.Rotation + 1.5707964f, this.item.Scale, SpriteEffects.None, new float?(newDepth));
				}
			}
			if (GameMain.DebugDraw)
			{
				Vector2 firingPos = this.GetRelativeFiringPosition(true);
				Vector2 endPos = firingPos + 3500f * this.GetBarrelDir();
				firingPos.Y = -firingPos.Y;
				endPos.Y = -endPos.Y;
				GUI.DrawLine(spriteBatch, firingPos - Vector2.UnitX * 5f, firingPos + Vector2.UnitX * 5f, Color.Red, 0f, 1f);
				GUI.DrawLine(spriteBatch, firingPos - Vector2.UnitY * 5f, firingPos + Vector2.UnitY * 5f, Color.Red, 0f, 1f);
				if (this.debugDrawTargetPos != null)
				{
					Vector2 targetPos = this.debugDrawTargetPos.Value;
					targetPos.Y = -targetPos.Y;
					GUI.DrawLine(spriteBatch, targetPos - Vector2.UnitX * 5f, targetPos + Vector2.UnitX * 5f, Color.Magenta, 0f, 5f);
					GUI.DrawLine(spriteBatch, targetPos - Vector2.UnitY * 5f, targetPos + Vector2.UnitY * 5f, Color.Magenta, 0f, 5f);
					GUI.DrawLine(spriteBatch, firingPos, targetPos, Color.Magenta, 0f, 2f);
				}
				GUI.DrawLine(spriteBatch, firingPos, endPos, Color.LightGray, 0f, 2f);
			}
			if (!editing || GUI.DisableHUD || !this.item.IsSelected)
			{
				return;
			}
			Vector2 center = new Vector2((float)Math.Cos((double)((this.maxRotation + this.minRotation) / 2f)), (float)Math.Sin((double)((this.maxRotation + this.minRotation) / 2f)));
			GUI.DrawLine(spriteBatch, drawPos, drawPos + center * 60f, Color.LightGreen, 0f, 1f);
			float radians = this.maxRotation - this.minRotation;
			float circleRadius = 300f / Screen.Selected.Cam.Zoom * GUI.Scale;
			float lineThickness = 1f / Screen.Selected.Cam.Zoom;
			if (Math.Abs(this.minRotation - this.maxRotation) < 0.02f)
			{
				spriteBatch.DrawLine(drawPos, drawPos + center * circleRadius, GUIStyle.Green, lineThickness);
			}
			else if (radians >= 6.2831855f)
			{
				spriteBatch.DrawCircle(drawPos, circleRadius, 180, GUIStyle.Green, lineThickness);
			}
			else
			{
				spriteBatch.DrawSector(drawPos, circleRadius, radians, (int)Math.Abs(90f * radians), GUIStyle.Green, this.minRotation, lineThickness);
			}
			int baseWidgetScale = GUI.IntScale(16f);
			int widgetSize = (int)Math.Max((float)baseWidgetScale, (float)baseWidgetScale / Screen.Selected.Cam.Zoom);
			float widgetThickness = Math.Max(1f, lineThickness);
			Widget minRotationWidget = this.GetWidget("minrotation", spriteBatch, widgetSize, widgetThickness, delegate(Widget widget)
			{
				widget.Selected += delegate()
				{
					this.oldRotation = this.RotationLimits;
				};
				widget.MouseDown += delegate()
				{
					widget.Color = GUIStyle.Green;
					this.prevAngle = this.minRotation;
				};
				widget.Deselected += delegate()
				{
					widget.Color = Color.Yellow;
					this.item.CreateEditingHUD(false);
					this.RotationLimits = this.RotationLimits;
					if (SubEditorScreen.IsSubEditor())
					{
						SubEditorScreen.StoreCommand(new PropertyCommand(this, "RotationLimits".ToIdentifier(), this.RotationLimits, this.oldRotation));
					}
				};
				widget.MouseHeld += delegate(float deltaTime)
				{
					float newMinRotation = this.GetRotationAngle(this.GetDrawPos());
					Turret.AngleWrapAdjustment(this.minRotation, newMinRotation, ref this.maxRotation);
					this.minRotation = MathHelper.Clamp(newMinRotation, this.maxRotation - 6.2831855f, this.maxRotation);
					this.<Draw>g__UpdateBarrel|65_2();
					MapEntity.DisableSelect = true;
				};
				widget.PreUpdate += delegate(float deltaTime)
				{
					widget.DrawPos = new Vector2(widget.DrawPos.X, -widget.DrawPos.Y);
					widget.DrawPos = Screen.Selected.Cam.WorldToScreen(widget.DrawPos);
				};
				widget.PostUpdate += delegate(float deltaTime)
				{
					widget.DrawPos = Screen.Selected.Cam.ScreenToWorld(widget.DrawPos);
					widget.DrawPos = new Vector2(widget.DrawPos.X, -widget.DrawPos.Y);
				};
				widget.PreDraw += delegate(SpriteBatch sprtBtch, float deltaTime)
				{
					widget.Tooltip = "Min: " + ((int)MathHelper.ToDegrees(this.minRotation)).ToString();
					widget.DrawPos = this.GetDrawPos() + new Vector2((float)Math.Cos((double)this.minRotation), (float)Math.Sin((double)this.minRotation)) * 300f / Screen.Selected.Cam.Zoom * GUI.Scale;
				};
			});
			Widget maxRotationWidget = this.GetWidget("maxrotation", spriteBatch, widgetSize, widgetThickness, delegate(Widget widget)
			{
				widget.Selected += delegate()
				{
					this.oldRotation = this.RotationLimits;
				};
				widget.MouseDown += delegate()
				{
					widget.Color = GUIStyle.Green;
					this.prevAngle = this.maxRotation;
				};
				widget.Deselected += delegate()
				{
					widget.Color = Color.Yellow;
					this.item.CreateEditingHUD(false);
					this.RotationLimits = this.RotationLimits;
					if (SubEditorScreen.IsSubEditor())
					{
						SubEditorScreen.StoreCommand(new PropertyCommand(this, "RotationLimits".ToIdentifier(), this.RotationLimits, this.oldRotation));
					}
				};
				widget.MouseHeld += delegate(float deltaTime)
				{
					float newMaxRotation = this.GetRotationAngle(this.GetDrawPos());
					Turret.AngleWrapAdjustment(this.maxRotation, newMaxRotation, ref this.minRotation);
					this.maxRotation = MathHelper.Clamp(newMaxRotation, this.minRotation, this.minRotation + 6.2831855f);
					this.<Draw>g__UpdateBarrel|65_2();
					MapEntity.DisableSelect = true;
				};
				widget.PreUpdate += delegate(float deltaTime)
				{
					widget.DrawPos = new Vector2(widget.DrawPos.X, -widget.DrawPos.Y);
					widget.DrawPos = Screen.Selected.Cam.WorldToScreen(widget.DrawPos);
				};
				widget.PostUpdate += delegate(float deltaTime)
				{
					widget.DrawPos = Screen.Selected.Cam.ScreenToWorld(widget.DrawPos);
					widget.DrawPos = new Vector2(widget.DrawPos.X, -widget.DrawPos.Y);
				};
				widget.PreDraw += delegate(SpriteBatch sprtBtch, float deltaTime)
				{
					widget.Tooltip = "Max: " + ((int)MathHelper.ToDegrees(this.maxRotation)).ToString();
					widget.DrawPos = this.GetDrawPos() + new Vector2((float)Math.Cos((double)this.maxRotation), (float)Math.Sin((double)this.maxRotation)) * 300f / Screen.Selected.Cam.Zoom * GUI.Scale;
					widget.Update(deltaTime);
				};
			});
			minRotationWidget.Draw(spriteBatch, 0.016666668f);
			maxRotationWidget.Draw(spriteBatch, 0.016666668f);
		}

		// Token: 0x0600623F RID: 25151 RVA: 0x00330F7C File Offset: 0x0032F17C
		private static void AngleWrapAdjustment(float currentRotation, float newRotation, ref float rangeLockedRotation)
		{
			if (Turret.DetectAngleWrapAround(currentRotation, newRotation))
			{
				if (newRotation < currentRotation)
				{
					rangeLockedRotation -= 6.2831855f;
					return;
				}
				rangeLockedRotation += 6.2831855f;
			}
		}

		// Token: 0x06006240 RID: 25152 RVA: 0x00330FA0 File Offset: 0x0032F1A0
		private static bool DetectAngleWrapAround(float rotation, float newRotation)
		{
			float deltaRotation = MathF.Abs(rotation - newRotation);
			return deltaRotation > 5.0265484f;
		}

		// Token: 0x06006241 RID: 25153 RVA: 0x00330FC4 File Offset: 0x0032F1C4
		public Vector2 GetDrawPos()
		{
			Vector2 drawPos = new Vector2((float)this.item.Rect.X + this.transformedBarrelPos.X, (float)this.item.Rect.Y - this.transformedBarrelPos.Y);
			if (this.item.Submarine != null)
			{
				drawPos += this.item.Submarine.DrawPosition;
			}
			drawPos.Y = -drawPos.Y;
			return drawPos;
		}

		// Token: 0x06006242 RID: 25154 RVA: 0x00331048 File Offset: 0x0032F248
		private Widget GetWidget(string id, SpriteBatch spriteBatch, int size = 5, float thickness = 1f, Action<Widget> initMethod = null)
		{
			Vector2 offset = new Vector2((float)(size / 2 + 5), -10f);
			Widget widget;
			if (!this.widgets.TryGetValue(id, out widget))
			{
				widget = new Widget(id, size, WidgetShape.Rectangle)
				{
					Color = Color.Yellow,
					TooltipOffset = new Vector2?(offset),
					InputAreaMargin = 20,
					RequireMouseOn = false
				};
				this.widgets.Add(id, widget);
				if (initMethod != null)
				{
					initMethod(widget);
				}
			}
			widget.Size = size;
			widget.TooltipOffset = new Vector2?(offset);
			widget.Thickness = thickness;
			return widget;
		}

		// Token: 0x06006243 RID: 25155 RVA: 0x003310DC File Offset: 0x0032F2DC
		private void GetAvailablePower(out float availableCharge, out float availableCapacity)
		{
			availableCharge = 0f;
			availableCapacity = 0f;
			if (this.item.Connections == null || this.powerIn == null)
			{
				return;
			}
			List<Connection> recipients = this.powerIn.Recipients;
			foreach (Connection recipient in recipients)
			{
				if (recipient.IsPower && recipient.IsOutput)
				{
					Item item = recipient.Item;
					PowerContainer battery = (item != null) ? item.GetComponent<PowerContainer>() : null;
					if (battery != null && battery.Item.Condition > 0f && !battery.OutputDisabled)
					{
						availableCharge += battery.Charge;
						availableCapacity += battery.GetCapacity();
					}
				}
			}
		}

		// Token: 0x06006244 RID: 25156 RVA: 0x003311AC File Offset: 0x0032F3AC
		private float GetRotationAngle(Vector2 drawPosition)
		{
			Vector2 mouseVector = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);
			mouseVector.Y = -mouseVector.Y;
			Vector2 rotationVector = mouseVector - drawPosition;
			rotationVector.Normalize();
			double angle = Math.Atan2((double)MathHelper.ToRadians(rotationVector.Y), (double)MathHelper.ToRadians(rotationVector.X));
			if (angle < 0.0)
			{
				angle = ((Math.Abs(angle - (double)this.prevAngle) < Math.Abs(angle + 6.283185307179586 - (double)this.prevAngle)) ? angle : (angle + 6.283185307179586));
			}
			else if (angle > 0.0)
			{
				angle = ((Math.Abs(angle - (double)this.prevAngle) < Math.Abs(angle - 6.283185307179586 - (double)this.prevAngle)) ? angle : (angle - 6.283185307179586));
			}
			angle = (double)MathHelper.Clamp((float)angle, -6.2831855f, 6.2831855f);
			this.prevAngle = (float)angle;
			return (float)angle;
		}

		// Token: 0x06006245 RID: 25157 RVA: 0x003312B0 File Offset: 0x0032F4B0
		public override void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			base.DrawHUD(spriteBatch, character);
			if (this.HudTint.A > 0)
			{
				GUI.DrawRectangle(spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), new Color((int)this.HudTint.R, (int)this.HudTint.G, (int)this.HudTint.B) * ((float)this.HudTint.A / 255f), true, 0f, 1f);
			}
			float batteryCharge;
			float batteryCapacity;
			this.GetAvailablePower(out batteryCharge, out batteryCapacity);
			Turret.<>c__DisplayClass72_0 CS$<>8__locals1;
			CS$<>8__locals1.availableAmmo = new List<Item>();
			Turret.<DrawHUD>g__AddAmmoFromContainer|72_0(this.item.GetComponent<ItemContainer>(), ref CS$<>8__locals1);
			foreach (MapEntity e in this.item.linkedTo)
			{
				Item linkedItem = e as Item;
				if (linkedItem != null)
				{
					Turret.<DrawHUD>g__AddAmmoFromContainer|72_0(linkedItem.GetComponent<ItemContainer>(), ref CS$<>8__locals1);
				}
			}
			float chargeRate = (this.powerConsumption <= 0f) ? 1f : ((batteryCapacity > 0f) ? (batteryCharge / batteryCapacity) : 0f);
			bool charged = batteryCharge * 3600f > this.powerConsumption;
			bool flag;
			if (this.reload <= 0f && charged)
			{
				flag = CS$<>8__locals1.availableAmmo.Any((Item p) => p != null);
			}
			else
			{
				flag = false;
			}
			bool readyToFire = flag;
			if (this.ShowChargeIndicator && base.PowerConsumption > 0f)
			{
				this.powerIndicator.Color = (charged ? (this.HasPowerToShoot() ? GUIStyle.Green : GUIStyle.Orange) : GUIStyle.Red);
				if (this.flashLowPower)
				{
					this.powerIndicator.BarSize = 1f;
					this.powerIndicator.Color *= (float)Math.Sin((double)(this.flashTimer * 12f));
					this.powerIndicator.RectTransform.ChangeScale(Vector2.Lerp(Vector2.One, Vector2.One * 1.01f, 2f * (float)Math.Sin((double)(this.flashTimer * 15f))));
				}
				else
				{
					this.powerIndicator.BarSize = chargeRate;
				}
				this.powerIndicator.DrawManually(spriteBatch, true, true);
				Rectangle sliderRect = this.powerIndicator.GetSliderRect(1f);
				int requiredChargeIndicatorPos = (int)(this.powerConsumption / (batteryCapacity * 3600f) * (float)sliderRect.Width);
				GUI.DrawRectangle(spriteBatch, new Rectangle(sliderRect.X + requiredChargeIndicatorPos, sliderRect.Y, 2, sliderRect.Height), Color.White * 0.5f, true, 0f, 1f);
			}
			if (this.ShowProjectileIndicator)
			{
				Point slotSize = (Inventory.SlotSpriteSmall.size * Inventory.UIScale).ToPoint();
				Point spacing = new Point(GUI.IntScale(5f), GUI.IntScale(20f));
				int slotsPerRow = Math.Min(CS$<>8__locals1.availableAmmo.Count, 6);
				int totalWidth = slotSize.X * slotsPerRow + spacing.X * (slotsPerRow - 1);
				int rows = (int)Math.Ceiling((double)((float)CS$<>8__locals1.availableAmmo.Count / (float)slotsPerRow));
				Point invSlotPos = new Point(GameMain.GraphicsWidth / 2 - totalWidth / 2, this.powerIndicator.Rect.Y - (slotSize.Y + spacing.Y) * rows);
				for (int i = 0; i < CS$<>8__locals1.availableAmmo.Count; i++)
				{
					Inventory.DrawSlot(spriteBatch, null, new VisualSlot(new Rectangle(invSlotPos + new Point(i % slotsPerRow * (slotSize.X + spacing.X), (int)Math.Floor((double)((float)i / (float)slotsPerRow)) * (slotSize.Y + spacing.Y)), slotSize)), CS$<>8__locals1.availableAmmo[i], -1, true, InvSlotType.Any);
				}
				Rectangle rect = new Rectangle(invSlotPos.X, invSlotPos.Y, totalWidth, slotSize.Y);
				float inflate = MathHelper.Lerp(3f, 8f, (float)Math.Abs(Math.Sin((double)(this.flashTimer * 5f))));
				rect.Inflate(inflate, inflate);
				Color color = GUIStyle.Red * Math.Max(0.5f, (float)Math.Sin((double)(this.flashTimer * 12f)));
				if (this.flashNoAmmo)
				{
					GUI.DrawRectangle(spriteBatch, rect, color, false, 0f, 3f);
				}
				else if (this.flashLoaderBroken)
				{
					GUI.DrawRectangle(spriteBatch, rect, color, false, 0f, 3f);
					GUIStyle.BrokenIcon.Value.Sprite.Draw(spriteBatch, rect.Center.ToVector2(), color, 0f, (float)rect.Height / GUIStyle.BrokenIcon.Value.Sprite.size.Y, SpriteEffects.None, null);
					GUIComponent.DrawToolTip(spriteBatch, TextManager.Get("turretloaderbroken"), new Rectangle(invSlotPos.X + totalWidth + GUI.IntScale(10f), invSlotPos.Y + slotSize.Y / 2 - GUI.IntScale(9f), 0, 0), Anchor.BottomCenter, Pivot.TopLeft);
				}
			}
			float zoom = (this.cam == null) ? 1f : ((float)Math.Sqrt((double)this.cam.Zoom));
			GUI.HideCursor = ((this.crosshairSprite != null || this.crosshairPointerSprite != null) && GUI.MouseOn == null && !GameMain.Instance.Paused);
			if (GUI.HideCursor)
			{
				Sprite sprite = this.crosshairSprite;
				if (sprite != null)
				{
					sprite.Draw(spriteBatch, this.crosshairPos, readyToFire ? Color.White : (Color.White * 0.2f), 0f, zoom, SpriteEffects.None, null);
				}
				Sprite sprite2 = this.crosshairPointerSprite;
				if (sprite2 == null)
				{
					return;
				}
				sprite2.Draw(spriteBatch, this.crosshairPointerPos, 0f, zoom, SpriteEffects.None);
			}
		}

		// Token: 0x06006246 RID: 25158 RVA: 0x003318F8 File Offset: 0x0032FAF8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			ushort projectileID = msg.ReadUInt16();
			float newTargetRotation = msg.ReadRangedSingle(this.minRotation, this.maxRotation, 16);
			if (Character.Controlled == null || this.user != Character.Controlled)
			{
				this.targetRotation = newTargetRotation;
			}
			if (projectileID == 0)
			{
				return;
			}
			if (projectileID == 65535)
			{
				this.Launch(null, this.user, null, 0f);
				return;
			}
			Item projectile = Entity.FindEntityByID(projectileID) as Item;
			if (projectile == null)
			{
				DebugConsole.ThrowError("Failed to launch a projectile - item with the ID \"" + projectileID.ToString() + " not found", null, null, false, false);
				return;
			}
			this.Launch(projectile, this.user, new float?(newTargetRotation), 0f);
		}

		// Token: 0x170018D2 RID: 6354
		// (get) Token: 0x06006247 RID: 25159 RVA: 0x003319AB File Offset: 0x0032FBAB
		public IEnumerable<Item> ActiveProjectiles
		{
			get
			{
				return this.activeProjectiles;
			}
		}

		// Token: 0x170018D3 RID: 6355
		// (get) Token: 0x06006248 RID: 25160 RVA: 0x003319B3 File Offset: 0x0032FBB3
		// (set) Token: 0x06006249 RID: 25161 RVA: 0x003319BB File Offset: 0x0032FBBB
		public float Rotation { get; private set; }

		// Token: 0x170018D4 RID: 6356
		// (get) Token: 0x0600624A RID: 25162 RVA: 0x003319C4 File Offset: 0x0032FBC4
		// (set) Token: 0x0600624B RID: 25163 RVA: 0x003319CC File Offset: 0x0032FBCC
		[Serialize("0,0", IsPropertySaveable.No, "The position of the barrel relative to the upper left corner of the base sprite (in pixels).", "", false)]
		public Vector2 BarrelPos
		{
			get
			{
				return this.barrelPos;
			}
			set
			{
				this.barrelPos = value;
				this.UpdateTransformedBarrelPos();
			}
		}

		// Token: 0x170018D5 RID: 6357
		// (get) Token: 0x0600624C RID: 25164 RVA: 0x003319DB File Offset: 0x0032FBDB
		// (set) Token: 0x0600624D RID: 25165 RVA: 0x003319E3 File Offset: 0x0032FBE3
		[Serialize("0,0", IsPropertySaveable.No, "The projectile launching location relative to transformed barrel position (in pixels).", "", false)]
		public Vector2 FiringOffset { get; set; }

		// Token: 0x170018D6 RID: 6358
		// (get) Token: 0x0600624E RID: 25166 RVA: 0x003319EC File Offset: 0x0032FBEC
		// (set) Token: 0x0600624F RID: 25167 RVA: 0x003319F4 File Offset: 0x0032FBF4
		[Serialize(false, IsPropertySaveable.No, "If enabled, the firing offset will alternate from left to right (i.e. flipping the x-component of the offset each shot.)", "", false)]
		public bool AlternatingFiringOffset { get; set; }

		// Token: 0x170018D7 RID: 6359
		// (get) Token: 0x06006250 RID: 25168 RVA: 0x003319FD File Offset: 0x0032FBFD
		public Vector2 TransformedBarrelPos
		{
			get
			{
				return this.transformedBarrelPos;
			}
		}

		// Token: 0x170018D8 RID: 6360
		// (get) Token: 0x06006251 RID: 25169 RVA: 0x00331A05 File Offset: 0x0032FC05
		// (set) Token: 0x06006252 RID: 25170 RVA: 0x00331A0D File Offset: 0x0032FC0D
		[Serialize(0f, IsPropertySaveable.No, "The impulse applied to the physics body of the projectile (the higher the impulse, the faster the projectiles are launched).", "", false)]
		public float LaunchImpulse { get; set; }

		// Token: 0x170018D9 RID: 6361
		// (get) Token: 0x06006253 RID: 25171 RVA: 0x00331A16 File Offset: 0x0032FC16
		// (set) Token: 0x06006254 RID: 25172 RVA: 0x00331A1E File Offset: 0x0032FC1E
		[Serialize(1f, IsPropertySaveable.No, "Multiplies the damage the turret deals by this amount.", "", false)]
		public float DamageMultiplier { get; set; }

		// Token: 0x170018DA RID: 6362
		// (get) Token: 0x06006255 RID: 25173 RVA: 0x00331A27 File Offset: 0x0032FC27
		// (set) Token: 0x06006256 RID: 25174 RVA: 0x00331A2F File Offset: 0x0032FC2F
		[Serialize(1, IsPropertySaveable.No, "How many projectiles the weapon launches when fired once.", "", false)]
		public int ProjectileCount { get; set; }

		// Token: 0x170018DB RID: 6363
		// (get) Token: 0x06006257 RID: 25175 RVA: 0x00331A38 File Offset: 0x0032FC38
		// (set) Token: 0x06006258 RID: 25176 RVA: 0x00331A40 File Offset: 0x0032FC40
		[Serialize(false, IsPropertySaveable.No, "Can the turret be fired without projectiles (causing it just to execute the OnUse effects and the firing animation without actually firing anything).", "", false)]
		public bool LaunchWithoutProjectile { get; set; }

		// Token: 0x170018DC RID: 6364
		// (get) Token: 0x06006259 RID: 25177 RVA: 0x00331A49 File Offset: 0x0032FC49
		// (set) Token: 0x0600625A RID: 25178 RVA: 0x00331A51 File Offset: 0x0032FC51
		[Serialize(0f, IsPropertySaveable.No, "Random spread applied to the firing angle of the projectiles (in degrees).", "", false)]
		public float Spread { get; set; }

		// Token: 0x170018DD RID: 6365
		// (get) Token: 0x0600625B RID: 25179 RVA: 0x00331A5A File Offset: 0x0032FC5A
		// (set) Token: 0x0600625C RID: 25180 RVA: 0x00331A62 File Offset: 0x0032FC62
		[Serialize(1f, IsPropertySaveable.No, "How fast the turret can rotate while firing (for charged weapons).", "", false)]
		public float FiringRotationSpeedModifier { get; set; }

		// Token: 0x170018DE RID: 6366
		// (get) Token: 0x0600625D RID: 25181 RVA: 0x00331A6B File Offset: 0x0032FC6B
		// (set) Token: 0x0600625E RID: 25182 RVA: 0x00331A73 File Offset: 0x0032FC73
		[Serialize(false, IsPropertySaveable.Yes, "Whether the turret should always charge-up fully to shoot.", "", false)]
		public bool SingleChargedShot { get; set; }

		// Token: 0x170018DF RID: 6367
		// (get) Token: 0x0600625F RID: 25183 RVA: 0x00331A7C File Offset: 0x0032FC7C
		// (set) Token: 0x06006260 RID: 25184 RVA: 0x00331A89 File Offset: 0x0032FC89
		[Serialize(0f, IsPropertySaveable.Yes, "The angle of the turret's base in degrees.", "", true)]
		public float BaseRotation
		{
			get
			{
				return this.item.Rotation;
			}
			set
			{
				this.item.Rotation = value;
				this.UpdateTransformedBarrelPos();
			}
		}

		// Token: 0x170018E0 RID: 6368
		// (get) Token: 0x06006261 RID: 25185 RVA: 0x00331A9D File Offset: 0x0032FC9D
		// (set) Token: 0x06006262 RID: 25186 RVA: 0x00331AA5 File Offset: 0x0032FCA5
		[Serialize(3500f, IsPropertySaveable.Yes, "How close to a target the turret has to be for an AI character to fire it.", "", false)]
		public float AIRange { get; set; }

		// Token: 0x170018E1 RID: 6369
		// (get) Token: 0x06006263 RID: 25187 RVA: 0x00331AAE File Offset: 0x0032FCAE
		// (set) Token: 0x06006264 RID: 25188 RVA: 0x00331AB6 File Offset: 0x0032FCB6
		[Serialize(10f, IsPropertySaveable.No, "How much off the turret can be from the target for the AI to shoot. In degrees.", "", false)]
		public float MaxAngleOffset
		{
			get
			{
				return this._maxAngleOffset;
			}
			private set
			{
				this._maxAngleOffset = MathHelper.Clamp(value, 0f, 180f);
			}
		}

		// Token: 0x170018E2 RID: 6370
		// (get) Token: 0x06006265 RID: 25189 RVA: 0x00331ACE File Offset: 0x0032FCCE
		// (set) Token: 0x06006266 RID: 25190 RVA: 0x00331AD6 File Offset: 0x0032FCD6
		[Serialize(1.1f, IsPropertySaveable.No, "How much does the AI prefer currently selected targets over new targets closer to the turret.", "", false)]
		public float AICurrentTargetPriorityMultiplier { get; private set; }

		// Token: 0x170018E3 RID: 6371
		// (get) Token: 0x06006267 RID: 25191 RVA: 0x00331ADF File Offset: 0x0032FCDF
		// (set) Token: 0x06006268 RID: 25192 RVA: 0x00331AE7 File Offset: 0x0032FCE7
		[Serialize(-1, IsPropertySaveable.Yes, "The turret won't fire additional projectiles if the number of previously fired, still active projectiles reaches this limit. If set to -1, there is no limit to the number of projectiles.", "", false)]
		public int MaxActiveProjectiles { get; set; }

		// Token: 0x170018E4 RID: 6372
		// (get) Token: 0x06006269 RID: 25193 RVA: 0x00331AF0 File Offset: 0x0032FCF0
		// (set) Token: 0x0600626A RID: 25194 RVA: 0x00331AF8 File Offset: 0x0032FCF8
		[Serialize(0f, IsPropertySaveable.Yes, "The time required for a charge-type turret to charge up before able to fire.", "", false)]
		public float MaxChargeTime { get; private set; }

		// Token: 0x170018E5 RID: 6373
		// (get) Token: 0x0600626B RID: 25195 RVA: 0x00331B01 File Offset: 0x0032FD01
		// (set) Token: 0x0600626C RID: 25196 RVA: 0x00331B09 File Offset: 0x0032FD09
		[Serialize(5f, IsPropertySaveable.No, "The period of time the user has to wait between shots.", "", false)]
		[Editable(0f, 1000f, 3)]
		public float Reload { get; set; }

		// Token: 0x170018E6 RID: 6374
		// (get) Token: 0x0600626D RID: 25197 RVA: 0x00331B12 File Offset: 0x0032FD12
		// (set) Token: 0x0600626E RID: 25198 RVA: 0x00331B1A File Offset: 0x0032FD1A
		[Serialize(1, IsPropertySaveable.No, "How many projectiles needs to be shot before we add an extra break? Think of the double coilgun.", "", false)]
		[Editable(1, 100)]
		public int ShotsPerBurst { get; set; }

		// Token: 0x170018E7 RID: 6375
		// (get) Token: 0x0600626F RID: 25199 RVA: 0x00331B23 File Offset: 0x0032FD23
		// (set) Token: 0x06006270 RID: 25200 RVA: 0x00331B2B File Offset: 0x0032FD2B
		[Serialize(0f, IsPropertySaveable.No, "An extra delay between the bursts. Added to the reload.", "", false)]
		[Editable(0f, 1000f, 3)]
		public float DelayBetweenBursts { get; set; }

		// Token: 0x170018E8 RID: 6376
		// (get) Token: 0x06006271 RID: 25201 RVA: 0x00331B34 File Offset: 0x0032FD34
		// (set) Token: 0x06006272 RID: 25202 RVA: 0x00331B3C File Offset: 0x0032FD3C
		[Serialize(1f, IsPropertySaveable.No, "Modifies the duration of retraction of the barrell after recoil to get back to the original position after shooting. Reload time affects this too.", "", false)]
		[Editable(0.1f, 10f, 1)]
		public float RetractionDurationMultiplier { get; set; }

		// Token: 0x170018E9 RID: 6377
		// (get) Token: 0x06006273 RID: 25203 RVA: 0x00331B45 File Offset: 0x0032FD45
		// (set) Token: 0x06006274 RID: 25204 RVA: 0x00331B4D File Offset: 0x0032FD4D
		[Serialize(0.1f, IsPropertySaveable.No, "How quickly the recoil moves the barrel after launching.", "", false)]
		[Editable(0.1f, 10f, 1)]
		public float RecoilTime { get; set; }

		// Token: 0x170018EA RID: 6378
		// (get) Token: 0x06006275 RID: 25205 RVA: 0x00331B56 File Offset: 0x0032FD56
		// (set) Token: 0x06006276 RID: 25206 RVA: 0x00331B5E File Offset: 0x0032FD5E
		[Serialize(0f, IsPropertySaveable.No, "How long the barrell stays in place after the recoil and before retracting back to the original position.", "", false)]
		[Editable(0f, 1000f, 1)]
		public float RetractionDelay { get; set; }

		// Token: 0x170018EB RID: 6379
		// (get) Token: 0x06006277 RID: 25207 RVA: 0x00331B67 File Offset: 0x0032FD67
		// (set) Token: 0x06006278 RID: 25208 RVA: 0x00331B84 File Offset: 0x0032FD84
		[Editable(VectorComponentLabels = new string[]
		{
			"editable.minvalue",
			"editable.maxvalue"
		})]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "The range at which the barrel can rotate.", "", true)]
		public Vector2 RotationLimits
		{
			get
			{
				return new Vector2(MathHelper.ToDegrees(this.minRotation), MathHelper.ToDegrees(this.maxRotation));
			}
			set
			{
				float newMinRotation = MathHelper.ToRadians(value.X);
				float newMaxRotation = MathHelper.ToRadians(value.Y);
				bool minRotationModified = MathHelper.Distance(newMinRotation, this.minRotation) > 0.02f;
				bool maxRotationModified = MathHelper.Distance(newMaxRotation, this.maxRotation) > 0.02f;
				if (minRotationModified && !maxRotationModified)
				{
					newMinRotation = MathHelper.Clamp(newMinRotation, this.maxRotation - 6.2831855f, this.maxRotation);
				}
				else if (!minRotationModified && maxRotationModified)
				{
					newMaxRotation = MathHelper.Clamp(newMaxRotation, this.minRotation, this.minRotation + 6.2831855f);
				}
				this.maxRotation = newMaxRotation;
				this.minRotation = newMinRotation;
				this.Rotation = (this.minRotation + this.maxRotation) / 2f;
				if (this.lightComponents != null)
				{
					foreach (LightComponent light in this.lightComponents)
					{
						light.Rotation = this.Rotation;
						light.Light.Rotation = -this.Rotation;
					}
				}
			}
		}

		// Token: 0x170018EC RID: 6380
		// (get) Token: 0x06006279 RID: 25209 RVA: 0x00331CA4 File Offset: 0x0032FEA4
		// (set) Token: 0x0600627A RID: 25210 RVA: 0x00331CAC File Offset: 0x0032FEAC
		[Serialize(5f, IsPropertySaveable.No, "How much torque is applied to rotate the barrel when the item is used by a character with insufficient skills to operate it. Higher values make the barrel rotate faster.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringStiffnessLowSkill { get; private set; }

		// Token: 0x170018ED RID: 6381
		// (get) Token: 0x0600627B RID: 25211 RVA: 0x00331CB5 File Offset: 0x0032FEB5
		// (set) Token: 0x0600627C RID: 25212 RVA: 0x00331CBD File Offset: 0x0032FEBD
		[Serialize(2f, IsPropertySaveable.No, "How much torque is applied to rotate the barrel when the item is used by a character with sufficient skills to operate it. Higher values make the barrel rotate faster.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringStiffnessHighSkill { get; private set; }

		// Token: 0x170018EE RID: 6382
		// (get) Token: 0x0600627D RID: 25213 RVA: 0x00331CC6 File Offset: 0x0032FEC6
		// (set) Token: 0x0600627E RID: 25214 RVA: 0x00331CCE File Offset: 0x0032FECE
		[Serialize(50f, IsPropertySaveable.No, "How much torque is applied to resist the movement of the barrel when the item is used by a character with insufficient skills to operate it. Higher values make the aiming more \"snappy\", stopping the barrel from swinging around the direction it's being aimed at.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringDampingLowSkill { get; private set; }

		// Token: 0x170018EF RID: 6383
		// (get) Token: 0x0600627F RID: 25215 RVA: 0x00331CD7 File Offset: 0x0032FED7
		// (set) Token: 0x06006280 RID: 25216 RVA: 0x00331CDF File Offset: 0x0032FEDF
		[Serialize(10f, IsPropertySaveable.No, "How much torque is applied to resist the movement of the barrel when the item is used by a character with sufficient skills to operate it. Higher values make the aiming more \"snappy\", stopping the barrel from swinging around the direction it's being aimed at.", "", false)]
		[Editable(0f, 1000f, 1, DecimalCount = 2)]
		public float SpringDampingHighSkill { get; private set; }

		// Token: 0x170018F0 RID: 6384
		// (get) Token: 0x06006281 RID: 25217 RVA: 0x00331CE8 File Offset: 0x0032FEE8
		// (set) Token: 0x06006282 RID: 25218 RVA: 0x00331CF0 File Offset: 0x0032FEF0
		[Serialize(1f, IsPropertySaveable.No, "Maximum angular velocity of the barrel when used by a character with insufficient skills to operate it.", "", false)]
		[Editable(0f, 100f, 1, DecimalCount = 2)]
		public float RotationSpeedLowSkill { get; private set; }

		// Token: 0x170018F1 RID: 6385
		// (get) Token: 0x06006283 RID: 25219 RVA: 0x00331CF9 File Offset: 0x0032FEF9
		// (set) Token: 0x06006284 RID: 25220 RVA: 0x00331D01 File Offset: 0x0032FF01
		[Serialize(5f, IsPropertySaveable.No, "Maximum angular velocity of the barrel when used by a character with sufficient skills to operate it.", "", false)]
		[Editable(0f, 100f, 1, DecimalCount = 2)]
		public float RotationSpeedHighSkill { get; private set; }

		// Token: 0x170018F2 RID: 6386
		// (get) Token: 0x06006285 RID: 25221 RVA: 0x00331D0A File Offset: 0x0032FF0A
		// (set) Token: 0x06006286 RID: 25222 RVA: 0x00331D12 File Offset: 0x0032FF12
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "Optional screen tint color when the item is being operated (R,G,B,A).", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public Color HudTint { get; set; }

		// Token: 0x170018F3 RID: 6387
		// (get) Token: 0x06006287 RID: 25223 RVA: 0x00331D1B File Offset: 0x0032FF1B
		// (set) Token: 0x06006288 RID: 25224 RVA: 0x00331D23 File Offset: 0x0032FF23
		[Header("", "sp.turret.AutoOperate.propertyheader")]
		[Serialize(false, IsPropertySaveable.Yes, "Should the turret operate automatically using AI targeting? Comes with some optional random movement that can be adjusted below.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool AutoOperate { get; set; }

		// Token: 0x170018F4 RID: 6388
		// (get) Token: 0x06006289 RID: 25225 RVA: 0x00331D2C File Offset: 0x0032FF2C
		// (set) Token: 0x0600628A RID: 25226 RVA: 0x00331D34 File Offset: 0x0032FF34
		[Serialize(false, IsPropertySaveable.Yes, "Can the Auto Operate functionality be enabled using signals to the turret?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool AllowAutoOperateWithWiring { get; set; }

		// Token: 0x170018F5 RID: 6389
		// (get) Token: 0x0600628B RID: 25227 RVA: 0x00331D3D File Offset: 0x0032FF3D
		// (set) Token: 0x0600628C RID: 25228 RVA: 0x00331D45 File Offset: 0x0032FF45
		[Serialize(0f, IsPropertySaveable.Yes, "[Auto Operate] How much the turret should adjust the aim off the target randomly instead of tracking the target perfectly? In Degrees.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public float RandomAimAmount { get; set; }

		// Token: 0x170018F6 RID: 6390
		// (get) Token: 0x0600628D RID: 25229 RVA: 0x00331D4E File Offset: 0x0032FF4E
		// (set) Token: 0x0600628E RID: 25230 RVA: 0x00331D56 File Offset: 0x0032FF56
		[Serialize(0f, IsPropertySaveable.Yes, "[Auto Operate] How often the turret should adjust the aim randomly instead of tracking the target perfectly? Minimum wait time, in seconds.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public float RandomAimMinTime { get; set; }

		// Token: 0x170018F7 RID: 6391
		// (get) Token: 0x0600628F RID: 25231 RVA: 0x00331D5F File Offset: 0x0032FF5F
		// (set) Token: 0x06006290 RID: 25232 RVA: 0x00331D67 File Offset: 0x0032FF67
		[Serialize(0f, IsPropertySaveable.Yes, "[Auto Operate] How often the turret should adjust the aim randomly instead of tracking the target perfectly? Maximum wait time, in seconds.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public float RandomAimMaxTime { get; set; }

		// Token: 0x170018F8 RID: 6392
		// (get) Token: 0x06006291 RID: 25233 RVA: 0x00331D70 File Offset: 0x0032FF70
		// (set) Token: 0x06006292 RID: 25234 RVA: 0x00331D78 File Offset: 0x0032FF78
		[Serialize(false, IsPropertySaveable.Yes, "[Auto Operate] Should the turret move randomly while idle?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool RandomMovement { get; set; }

		// Token: 0x170018F9 RID: 6393
		// (get) Token: 0x06006293 RID: 25235 RVA: 0x00331D81 File Offset: 0x0032FF81
		// (set) Token: 0x06006294 RID: 25236 RVA: 0x00331D89 File Offset: 0x0032FF89
		[Serialize(false, IsPropertySaveable.Yes, "[Auto Operate] Should the turret have a delay while targeting targets or always aim prefectly?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool AimDelay { get; set; }

		// Token: 0x170018FA RID: 6394
		// (get) Token: 0x06006295 RID: 25237 RVA: 0x00331D92 File Offset: 0x0032FF92
		// (set) Token: 0x06006296 RID: 25238 RVA: 0x00331D9A File Offset: 0x0032FF9A
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target characters in general?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetCharacters { get; set; }

		// Token: 0x170018FB RID: 6395
		// (get) Token: 0x06006297 RID: 25239 RVA: 0x00331DA3 File Offset: 0x0032FFA3
		// (set) Token: 0x06006298 RID: 25240 RVA: 0x00331DAB File Offset: 0x0032FFAB
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target all monsters?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetMonsters { get; set; }

		// Token: 0x170018FC RID: 6396
		// (get) Token: 0x06006299 RID: 25241 RVA: 0x00331DB4 File Offset: 0x0032FFB4
		// (set) Token: 0x0600629A RID: 25242 RVA: 0x00331DBC File Offset: 0x0032FFBC
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target all humans (or creatures in the same group, like pets)?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetHumans { get; set; }

		// Token: 0x170018FD RID: 6397
		// (get) Token: 0x0600629B RID: 25243 RVA: 0x00331DC5 File Offset: 0x0032FFC5
		// (set) Token: 0x0600629C RID: 25244 RVA: 0x00331DCD File Offset: 0x0032FFCD
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target other submarines?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetSubmarines { get; set; }

		// Token: 0x170018FE RID: 6398
		// (get) Token: 0x0600629D RID: 25245 RVA: 0x00331DD6 File Offset: 0x0032FFD6
		// (set) Token: 0x0600629E RID: 25246 RVA: 0x00331DDE File Offset: 0x0032FFDE
		[Serialize(true, IsPropertySaveable.Yes, "[Auto Operate] Should the turret target items?", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public bool TargetItems { get; set; }

		// Token: 0x170018FF RID: 6399
		// (get) Token: 0x0600629F RID: 25247 RVA: 0x00331DE7 File Offset: 0x0032FFE7
		// (set) Token: 0x060062A0 RID: 25248 RVA: 0x00331DEF File Offset: 0x0032FFEF
		[Serialize("", IsPropertySaveable.Yes, "[Auto Operate] Group or SpeciesName that the AI ignores when the turret is operated automatically.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public Identifier FriendlyTag { get; private set; }

		// Token: 0x17001900 RID: 6400
		// (get) Token: 0x060062A1 RID: 25249 RVA: 0x00331DF8 File Offset: 0x0032FFF8
		// (set) Token: 0x060062A2 RID: 25250 RVA: 0x00331E00 File Offset: 0x00330000
		[Serialize("OwnSub", IsPropertySaveable.Yes, "[Auto Operate] Team that the turret considers friendly.", "", false)]
		[Editable(TransferToSwappedItem = true)]
		public Turret.TeamType FriendlyTeamType { get; private set; }

		// Token: 0x060062A3 RID: 25251 RVA: 0x00331E0C File Offset: 0x0033000C
		public Turret(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "barrelsprite"))
				{
					if (!(a == "railsprite"))
					{
						if (!(a == "barrelspritebroken"))
						{
							if (!(a == "railspritebroken"))
							{
								if (!(a == "chargesprite"))
								{
									if (a == "spinningbarrelsprite")
									{
										int spriteCount = subElement.GetAttributeInt("spriteamount", 1);
										for (int i = 0; i < spriteCount; i++)
										{
											this.spinningBarrelSprites.Add(new Sprite(subElement, "", "", false, 1f));
										}
									}
								}
								else
								{
									List<ValueTuple<Sprite, Vector2>> list = this.chargeSprites;
									Sprite item2 = new Sprite(subElement, "", "", false, 1f);
									ContentXElement contentXElement = subElement;
									string key = "chargetarget";
									Vector2 zero = Vector2.Zero;
									list.Add(new ValueTuple<Sprite, Vector2>(item2, contentXElement.GetAttributeVector2(key, zero)));
								}
							}
							else
							{
								this.railSpriteBroken = new Sprite(subElement, "", "", false, 1f);
							}
						}
						else
						{
							this.barrelSpriteBroken = new Sprite(subElement, "", "", false, 1f);
						}
					}
					else
					{
						this.railSprite = new Sprite(subElement, "", "", false, 1f);
					}
				}
				else
				{
					this.barrelSprite = new Sprite(subElement, "", "", false, 1f);
				}
			}
			item.IsShootable = true;
			item.RequireAimToUse = false;
			this.isSlowTurret = item.HasTag("slowturret".ToIdentifier());
			this.InitProjSpecific(element);
		}

		// Token: 0x060062A4 RID: 25252 RVA: 0x00332060 File Offset: 0x00330260
		private void InitProjSpecific(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string textureDir = base.GetTextureDirectory(subElement);
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 9:
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c == 'm')
							{
								if (text == "movesound")
								{
									this.moveSound = RoundSound.Load(subElement);
								}
							}
						}
						else if (text == "crosshair")
						{
							this.crosshairSprite = new Sprite(subElement, textureDir, "", false, 1f);
						}
						break;
					}
					case 11:
						if (text == "chargesound")
						{
							this.chargeSound = RoundSound.Load(subElement);
						}
						break;
					case 12:
						if (text == "endmovesound")
						{
							this.endMoveSound = RoundSound.Load(subElement);
						}
						break;
					case 14:
						if (text == "startmovesound")
						{
							this.startMoveSound = RoundSound.Load(subElement);
						}
						break;
					case 15:
					{
						char c = text[0];
						if (c != 'p')
						{
							if (c == 'w')
							{
								if (text == "weaponindicator")
								{
									this.WeaponIndicatorSprite = new Sprite(subElement, textureDir, "", false, 1f);
								}
							}
						}
						else if (text == "particleemitter")
						{
							this.particleEmitters.Add(new ParticleEmitter(subElement));
						}
						break;
					}
					case 16:
						if (text == "crosshairpointer")
						{
							this.crosshairPointerSprite = new Sprite(subElement, textureDir, "", false, 1f);
						}
						break;
					case 21:
						if (text == "particleemittercharge")
						{
							this.particleEmitterCharges.Add(new ParticleEmitter(subElement));
						}
						break;
					}
				}
			}
			this.powerIndicator = new GUIProgressBar(new RectTransform(new Vector2(0.18f, 0.03f), GUI.Canvas, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(100, 20),
				RelativeOffset = new Vector2(0f, 0.01f)
			}, 0f, null, "DeviceProgressBar", true)
			{
				CanBeFocused = false
			};
		}

		// Token: 0x060062A5 RID: 25253 RVA: 0x00332344 File Offset: 0x00330544
		private void UpdateTransformedBarrelPos()
		{
			this.transformedBarrelPos = MathUtils.RotatePointAroundTarget(this.barrelPos * this.item.Scale, new Vector2((float)(this.item.Rect.Width / 2), (float)(this.item.Rect.Height / 2)), MathHelper.ToRadians(this.item.Rotation), true);
			this.item.ResetCachedVisibleSize();
			this.prevBaseRotation = this.item.Rotation;
			this.prevScale = this.item.Scale;
		}

		// Token: 0x060062A6 RID: 25254 RVA: 0x003323DC File Offset: 0x003305DC
		public override void OnMapLoaded()
		{
			base.OnMapLoaded();
			if (this.loadedRotationLimits != null)
			{
				this.RotationLimits = this.loadedRotationLimits.Value;
			}
			if (this.loadedBaseRotation != null)
			{
				this.BaseRotation = this.loadedBaseRotation.Value;
			}
			if (this.loadedFriendlyTeamType != null)
			{
				this.FriendlyTeamType = this.loadedFriendlyTeamType.Value;
			}
			this.targetRotation = this.Rotation;
			this.UpdateTransformedBarrelPos();
			if (!this.AllowAutoOperateWithWiring)
			{
				Screen selected = Screen.Selected;
				if (selected != null && !selected.IsEditor)
				{
					foreach (ConnectionPanel connectionPanel in base.Item.GetComponents<ConnectionPanel>())
					{
						connectionPanel.Connections.RemoveAll(delegate(Connection c)
						{
							string name = c.Name;
							bool flag = name == "toggle_auto_operate" || name == "set_auto_operate";
							return flag && c.Wires.None(null);
						});
					}
				}
			}
		}

		// Token: 0x060062A7 RID: 25255 RVA: 0x003324E0 File Offset: 0x003306E0
		private void FindLightComponents()
		{
			if (this.lightComponents != null)
			{
				return;
			}
			foreach (LightComponent lc in this.item.GetComponents<LightComponent>())
			{
				if (((lc != null) ? lc.Parent : null) == this)
				{
					if (this.lightComponents == null)
					{
						this.lightComponents = new List<LightComponent>();
					}
					this.lightComponents.Add(lc);
				}
			}
			if (this.lightComponents != null)
			{
				foreach (LightComponent light in this.lightComponents)
				{
					light.Parent = null;
					light.Rotation = this.Rotation - this.item.RotationRad;
					light.Light.Rotation = -this.Rotation;
					light.Light.PriorityMultiplier *= 10f;
				}
			}
		}

		// Token: 0x060062A8 RID: 25256 RVA: 0x003325F0 File Offset: 0x003307F0
		public override void Update(float deltaTime, Camera cam)
		{
			this.cam = cam;
			if (this.reload > 0f)
			{
				this.reload -= deltaTime;
			}
			if (!MathUtils.NearlyEqual(this.item.Rotation, this.prevBaseRotation, 0.0001f) || !MathUtils.NearlyEqual(this.item.Scale, this.prevScale, 0.0001f))
			{
				this.UpdateTransformedBarrelPos();
			}
			Character activeUser = this.user;
			if (activeUser != null && activeUser.Removed)
			{
				this.user = null;
			}
			else
			{
				this.resetUserTimer -= deltaTime;
				if (this.resetUserTimer <= 0f)
				{
					this.user = null;
				}
			}
			activeUser = this.ActiveUser;
			if (activeUser != null && activeUser.Removed)
			{
				this.ActiveUser = null;
			}
			else
			{
				this.resetActiveUserTimer -= deltaTime;
				if (this.resetActiveUserTimer <= 0f)
				{
					this.ActiveUser = null;
				}
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			float previousChargeTime = this.currentChargeTime;
			if (this.SingleChargedShot && this.reload > 0f)
			{
				this.currentChargeTime = ((this.Reload > 0f) ? Math.Max(0f, this.MaxChargeTime * (this.reload / this.Reload - 0.5f)) : 0f);
			}
			else
			{
				float chargeDeltaTime = this.tryingToCharge ? deltaTime : (-deltaTime);
				if (chargeDeltaTime > 0f && this.user != null)
				{
					chargeDeltaTime *= 1f + this.user.GetStatValue(StatTypes.TurretChargeSpeed, true);
				}
				this.currentChargeTime = Math.Clamp(this.currentChargeTime + chargeDeltaTime, 0f, this.MaxChargeTime);
			}
			this.tryingToCharge = false;
			if (this.currentChargeTime == 0f)
			{
				this.currentChargingState = Turret.ChargingState.Inactive;
			}
			else if (this.currentChargeTime < previousChargeTime)
			{
				this.currentChargingState = Turret.ChargingState.WindingDown;
			}
			else
			{
				this.currentChargingState = Turret.ChargingState.WindingUp;
			}
			this.UpdateProjSpecific(deltaTime);
			if (MathUtils.NearlyEqual(this.minRotation, this.maxRotation, 0.0001f))
			{
				this.UpdateLightComponents();
				return;
			}
			float targetMidDiff = MathHelper.WrapAngle(this.targetRotation - (this.minRotation + this.maxRotation) / 2f);
			float maxDist = (this.maxRotation - this.minRotation) / 2f;
			if (Math.Abs(targetMidDiff) > maxDist)
			{
				this.targetRotation = ((targetMidDiff < 0f) ? this.minRotation : this.maxRotation);
			}
			float degreeOfSuccess = (this.user == null) ? 0.5f : base.DegreeOfSuccess(this.user);
			if (degreeOfSuccess < 0.5f)
			{
				degreeOfSuccess *= degreeOfSuccess;
			}
			float springStiffness = MathHelper.Lerp(this.SpringStiffnessLowSkill, this.SpringStiffnessHighSkill, degreeOfSuccess);
			float springDamping = MathHelper.Lerp(this.SpringDampingLowSkill, this.SpringDampingHighSkill, degreeOfSuccess);
			float rotationSpeed = MathHelper.Lerp(this.RotationSpeedLowSkill, this.RotationSpeedHighSkill, degreeOfSuccess);
			if (this.MaxChargeTime > 0f)
			{
				rotationSpeed *= MathHelper.Lerp(1f, this.FiringRotationSpeedModifier, MathUtils.EaseIn(this.currentChargeTime / this.MaxChargeTime));
			}
			Character character = this.user;
			if (((character != null) ? character.Info : null) != null)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.Campaign : null) == null || !Level.IsLoadedFriendlyOutpost)
				{
					this.user.Info.ApplySkillGain(Tags.WeaponsSkill, SkillSettings.Current.SkillIncreasePerSecondWhenOperatingTurret * deltaTime, false, 2f, false);
				}
			}
			float rotMidDiff = MathHelper.WrapAngle(this.Rotation - (this.minRotation + this.maxRotation) / 2f);
			float targetRotationDiff = MathHelper.WrapAngle(this.targetRotation - this.Rotation);
			if (this.maxRotation - this.minRotation < 6.2831855f)
			{
				float targetRotationMaxDiff = MathHelper.WrapAngle(this.targetRotation - this.maxRotation);
				float targetRotationMinDiff = MathHelper.WrapAngle(this.targetRotation - this.minRotation);
				if (Math.Abs(targetRotationMaxDiff) < Math.Abs(targetRotationMinDiff) && rotMidDiff < 0f && targetRotationDiff < 0f)
				{
					targetRotationDiff += 6.2831855f;
				}
				else if (Math.Abs(targetRotationMaxDiff) > Math.Abs(targetRotationMinDiff) && rotMidDiff > 0f && targetRotationDiff > 0f)
				{
					targetRotationDiff -= 6.2831855f;
				}
			}
			this.angularVelocity += (targetRotationDiff * springStiffness - this.angularVelocity * springDamping) * deltaTime;
			this.angularVelocity = MathHelper.Clamp(this.angularVelocity, -rotationSpeed, rotationSpeed);
			this.Rotation += this.angularVelocity * deltaTime;
			rotMidDiff = MathHelper.WrapAngle(this.Rotation - (this.minRotation + this.maxRotation) / 2f);
			if (rotMidDiff < -maxDist)
			{
				this.Rotation = this.minRotation;
				this.angularVelocity *= -0.5f;
			}
			else if (rotMidDiff > maxDist)
			{
				this.Rotation = this.maxRotation;
				this.angularVelocity *= -0.5f;
			}
			if (this.aiFindTargetTimer > 0f)
			{
				this.aiFindTargetTimer -= deltaTime;
			}
			this.UpdateLightComponents();
			if (this.AutoOperate && this.ActiveUser == null)
			{
				this.UpdateAutoOperate(deltaTime, false, default(Identifier));
			}
		}

		// Token: 0x060062A9 RID: 25257 RVA: 0x00332B18 File Offset: 0x00330D18
		public void UpdateLightComponents()
		{
			if (this.lightComponents != null)
			{
				foreach (LightComponent light in this.lightComponents)
				{
					light.Rotation = this.Rotation - this.item.RotationRad;
				}
			}
		}

		// Token: 0x060062AA RID: 25258 RVA: 0x00332B84 File Offset: 0x00330D84
		private void UpdateProjSpecific(float deltaTime)
		{
			this.recoilTimer -= deltaTime;
			if (this.crosshairSprite != null)
			{
				Vector2 itemPos = this.cam.WorldToScreen(new Vector2((float)this.item.WorldRect.X + this.transformedBarrelPos.X, (float)this.item.WorldRect.Y - this.transformedBarrelPos.Y));
				Vector2 turretDir = new Vector2((float)Math.Cos((double)this.Rotation), (float)Math.Sin((double)this.Rotation));
				Vector2 mouseDiff = itemPos - PlayerInput.MousePosition;
				this.crosshairPos = new Vector2(MathHelper.Clamp(itemPos.X + turretDir.X * mouseDiff.Length(), 0f, (float)GameMain.GraphicsWidth), MathHelper.Clamp(itemPos.Y + turretDir.Y * mouseDiff.Length(), 0f, (float)GameMain.GraphicsHeight));
			}
			this.crosshairPointerPos = PlayerInput.MousePosition;
			if (Math.Abs(this.angularVelocity) > 0.1f)
			{
				if (this.moveSoundChannel == null && this.startMoveSound != null)
				{
					RoundSound sound = this.startMoveSound;
					Vector2 worldPosition = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					this.moveSoundChannel = SoundPlayer.PlaySound(sound, worldPosition, null, currentHull);
				}
				else if ((this.moveSoundChannel == null || !this.moveSoundChannel.IsPlaying) && this.moveSound != null)
				{
					SoundChannel soundChannel = this.moveSoundChannel;
					if (soundChannel != null)
					{
						soundChannel.FadeOutAndDispose();
					}
					RoundSound sound2 = this.moveSound;
					Vector2 worldPosition2 = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					this.moveSoundChannel = SoundPlayer.PlaySound(sound2, worldPosition2, null, currentHull);
					if (this.moveSoundChannel != null)
					{
						this.moveSoundChannel.Looping = true;
					}
				}
			}
			else if (Math.Abs(this.angularVelocity) < 0.05f && this.moveSoundChannel != null)
			{
				if (this.endMoveSound != null && this.moveSoundChannel.Sound != this.endMoveSound.Sound)
				{
					this.moveSoundChannel.FadeOutAndDispose();
					RoundSound sound3 = this.endMoveSound;
					Vector2 worldPosition3 = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					this.moveSoundChannel = SoundPlayer.PlaySound(sound3, worldPosition3, null, currentHull);
					if (this.moveSoundChannel != null)
					{
						this.moveSoundChannel.Looping = false;
					}
				}
				else if (!this.moveSoundChannel.IsPlaying)
				{
					this.moveSoundChannel.FadeOutAndDispose();
					this.moveSoundChannel = null;
				}
			}
			float chargeRatio = this.currentChargeTime / this.MaxChargeTime;
			this.currentBarrelSpin = (this.currentBarrelSpin + 360f * chargeRatio * deltaTime * 3f) % 360f;
			Turret.ChargingState chargingState = this.currentChargingState;
			if (chargingState == Turret.ChargingState.WindingUp)
			{
				Vector2 particlePos = this.GetRelativeFiringPosition(true);
				float sizeMultiplier = Math.Clamp(chargeRatio, 0.1f, 1f);
				foreach (ParticleEmitter emitter in this.particleEmitterCharges)
				{
					emitter.Emit(deltaTime, particlePos, null, -this.Rotation, this.Rotation, 1f, sizeMultiplier, 1f, new Color?(emitter.Prefab.Properties.ColorMultiplier), null, false, null);
				}
				if (this.chargeSoundChannel == null || !this.chargeSoundChannel.IsPlaying)
				{
					if (this.chargeSound != null)
					{
						RoundSound sound4 = this.chargeSound;
						Vector2 worldPosition4 = this.item.WorldPosition;
						Hull currentHull = this.item.CurrentHull;
						this.chargeSoundChannel = SoundPlayer.PlaySound(sound4, worldPosition4, null, currentHull);
						if (this.chargeSoundChannel != null)
						{
							this.chargeSoundChannel.Looping = true;
						}
					}
				}
				else if (this.chargeSoundChannel != null)
				{
					this.chargeSoundChannel.FrequencyMultiplier = MathHelper.Lerp(this.ChargeSoundWindupPitchSlide.X, this.ChargeSoundWindupPitchSlide.Y, chargeRatio);
					this.chargeSoundChannel.Position = new Vector3?(new Vector3(this.item.WorldPosition, 0f));
				}
			}
			else if (this.chargeSoundChannel != null)
			{
				if (this.chargeSoundChannel.IsPlaying)
				{
					this.chargeSoundChannel.FadeOutAndDispose();
					this.chargeSoundChannel.Looping = false;
				}
				else
				{
					this.chargeSoundChannel = null;
				}
			}
			if (this.moveSoundChannel != null && this.moveSoundChannel.IsPlaying)
			{
				this.moveSoundChannel.Gain = MathHelper.Clamp(Math.Abs(this.angularVelocity), 0.5f, 1f);
			}
			if (this.flashLowPower || this.flashNoAmmo || this.flashLoaderBroken)
			{
				this.flashTimer += deltaTime;
				if (this.flashTimer >= this.flashLength)
				{
					this.flashTimer = 0f;
					this.flashLowPower = false;
					this.flashNoAmmo = false;
					this.flashLoaderBroken = false;
				}
			}
		}

		// Token: 0x060062AB RID: 25259 RVA: 0x00333084 File Offset: 0x00331284
		public override bool Use(float deltaTime, Character character = null)
		{
			if (!this.characterUsable && character != null)
			{
				return false;
			}
			if (this.isUseBeingCalled)
			{
				return false;
			}
			this.isUseBeingCalled = true;
			bool wasSuccessful = this.TryLaunch(deltaTime, character, false);
			this.isUseBeingCalled = false;
			return wasSuccessful;
		}

		// Token: 0x060062AC RID: 25260 RVA: 0x003330C4 File Offset: 0x003312C4
		public float GetPowerRequiredToShoot()
		{
			float powerCost = this.powerConsumption;
			if (this.user != null)
			{
				powerCost /= 1f + this.user.GetStatValue(StatTypes.TurretPowerCostReduction, true);
			}
			return powerCost;
		}

		// Token: 0x060062AD RID: 25261 RVA: 0x003330F8 File Offset: 0x003312F8
		public bool HasPowerToShoot()
		{
			return base.GetAvailableInstantaneousBatteryPower() >= this.GetPowerRequiredToShoot();
		}

		// Token: 0x060062AE RID: 25262 RVA: 0x0033310B File Offset: 0x0033130B
		private Vector2 GetBarrelDir()
		{
			return new Vector2((float)Math.Cos((double)this.Rotation), -(float)Math.Sin((double)this.Rotation));
		}

		// Token: 0x060062AF RID: 25263 RVA: 0x00333130 File Offset: 0x00331330
		private bool TryLaunch(float deltaTime, Character character = null, bool ignorePower = false)
		{
			Turret.<>c__DisplayClass318_0 CS$<>8__locals1;
			CS$<>8__locals1.deltaTime = deltaTime;
			CS$<>8__locals1.<>4__this = this;
			this.tryingToCharge = true;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return false;
			}
			if (this.currentChargeTime < this.MaxChargeTime)
			{
				return false;
			}
			if (this.reload > 0f)
			{
				return false;
			}
			if (this.MaxActiveProjectiles >= 0)
			{
				this.activeProjectiles.RemoveAll((Item it) => it.Removed);
				if (this.activeProjectiles.Count >= this.MaxActiveProjectiles)
				{
					return false;
				}
			}
			if (!ignorePower && !this.HasPowerToShoot())
			{
				if (!this.flashLowPower && character != null && character == Character.Controlled)
				{
					this.flashLowPower = true;
					SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
				}
				return false;
			}
			Projectile launchedProjectile = null;
			bool loaderBroken = false;
			float tinkeringStrength = 0f;
			for (int i = 0; i < this.ProjectileCount; i++)
			{
				Turret.<>c__DisplayClass318_1 CS$<>8__locals2;
				CS$<>8__locals2.projectiles = this.GetLoadedProjectiles();
				if (CS$<>8__locals2.projectiles.Any<Projectile>())
				{
					Item container2 = CS$<>8__locals2.projectiles.First<Projectile>().Item.Container;
					ItemContainer projectileContainer = (container2 != null) ? container2.GetComponent<ItemContainer>() : null;
					if (projectileContainer != null && projectileContainer.Item != this.item && projectileContainer != null)
					{
						projectileContainer.Item.Use(CS$<>8__locals1.deltaTime, null, null, null, this.user);
					}
				}
				else
				{
					for (int j = 0; j < this.item.linkedTo.Count; j++)
					{
						MapEntity e = this.item.linkedTo[(j + this.currentLoaderIndex) % this.item.linkedTo.Count];
						Item linkedItem = e as Item;
						if (linkedItem != null && this.item.Prefab.IsLinkAllowed(e.Prefab))
						{
							if (linkedItem.Condition <= 0f)
							{
								loaderBroken = true;
							}
							else if (this.<TryLaunch>g__tryUseProjectileContainer|318_1(linkedItem, ref CS$<>8__locals1, ref CS$<>8__locals2))
							{
								break;
							}
						}
					}
					this.<TryLaunch>g__tryUseProjectileContainer|318_1(this.item, ref CS$<>8__locals1, ref CS$<>8__locals2);
				}
				if (CS$<>8__locals2.projectiles.Count == 0 && !this.LaunchWithoutProjectile)
				{
					this.failedLaunchAttempts++;
					if (!this.flashNoAmmo && !this.flashLoaderBroken && character != null && character == Character.Controlled && this.failedLaunchAttempts > 20)
					{
						if (loaderBroken)
						{
							this.flashLoaderBroken = true;
						}
						else
						{
							this.flashNoAmmo = true;
						}
						this.failedLaunchAttempts = 0;
						SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
					}
					return false;
				}
				this.failedLaunchAttempts = 0;
				foreach (MapEntity e2 in this.item.linkedTo)
				{
					Item linkedItem2 = e2 as Item;
					if (linkedItem2 != null && this.item.Prefab.IsLinkAllowed(e2.Prefab))
					{
						Repairable repairable = linkedItem2.GetComponent<Repairable>();
						if (repairable != null && repairable.IsTinkering && linkedItem2.HasTag(Tags.TurretAmmoSource))
						{
							tinkeringStrength = repairable.TinkeringStrength;
						}
					}
				}
				if (!ignorePower)
				{
					IEnumerable<PowerContainer> batteries = from b in base.GetDirectlyConnectedBatteries()
					where !b.OutputDisabled && b.Charge > 0.0001f && b.MaxOutPut > 0.0001f
					select b;
					float neededPower = this.GetPowerRequiredToShoot();
					neededPower /= 1f + tinkeringStrength * 0.2f;
					while (neededPower > 0.0001f && batteries.Any<PowerContainer>())
					{
						float takePower = neededPower / (float)batteries.Count<PowerContainer>();
						takePower = Math.Min(takePower, batteries.Min((PowerContainer b) => Math.Min(b.Charge * 3600f, b.MaxOutPut)));
						foreach (PowerContainer battery in batteries)
						{
							neededPower -= takePower;
							battery.Charge -= takePower / 3600f;
						}
					}
				}
				launchedProjectile = CS$<>8__locals2.projectiles.FirstOrDefault<Projectile>();
				Item container = (launchedProjectile != null) ? launchedProjectile.Item.Container : null;
				if (container != null)
				{
					Repairable repairable2 = (launchedProjectile != null) ? launchedProjectile.Item.Container.GetComponent<Repairable>() : null;
					if (repairable2 != null)
					{
						repairable2.LastActiveTime = (float)Timing.TotalTime + 1f;
					}
				}
				if (launchedProjectile != null || this.LaunchWithoutProjectile)
				{
					if (((launchedProjectile != null) ? launchedProjectile.Item.GetComponent<Rope>() : null) != null)
					{
						Projectile projectile2 = this.lastProjectile;
						Rope rope = (projectile2 != null) ? projectile2.Item.GetComponent<Rope>() : null;
						if (rope != null && rope.SnapWhenWeaponFiredAgain)
						{
							rope.Snap();
						}
					}
					float tinkeringStrength2;
					if (CS$<>8__locals2.projectiles.Any<Projectile>())
					{
						using (List<Projectile>.Enumerator enumerator3 = CS$<>8__locals2.projectiles.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								Projectile projectile = enumerator3.Current;
								Item item = projectile.Item;
								tinkeringStrength2 = tinkeringStrength;
								this.Launch(item, character, null, tinkeringStrength2);
							}
							goto IL_4F0;
						}
						goto IL_4D9;
					}
					goto IL_4D9;
					IL_4F0:
					if (this.item.AiTarget != null)
					{
						this.item.AiTarget.SoundRange = this.item.AiTarget.MaxSoundRange;
					}
					if (container != null)
					{
						Turret.ShiftItemsInProjectileContainer(container.GetComponent<ItemContainer>());
					}
					if (this.item.linkedTo.Count > 0)
					{
						this.currentLoaderIndex = (this.currentLoaderIndex + 1) % this.item.linkedTo.Count;
						goto IL_55F;
					}
					goto IL_55F;
					IL_4D9:
					Item projectile3 = null;
					tinkeringStrength2 = tinkeringStrength;
					this.Launch(projectile3, character, null, tinkeringStrength2);
					goto IL_4F0;
				}
				IL_55F:;
			}
			this.lastProjectile = launchedProjectile;
			return true;
		}

		// Token: 0x060062B0 RID: 25264 RVA: 0x003336E0 File Offset: 0x003318E0
		private void Launch(Item projectile, Character user = null, float? launchRotation = null, float tinkeringStrength = 0f)
		{
			this.reload = this.Reload;
			if (this.ShotsPerBurst > 1)
			{
				this.shotCounter++;
				if (this.shotCounter >= this.ShotsPerBurst)
				{
					this.reload += this.DelayBetweenBursts;
					this.shotCounter = 0;
				}
			}
			this.reload /= 1f + tinkeringStrength * 0.2f;
			if (user != null)
			{
				this.reload /= 1f + user.GetStatValue(StatTypes.TurretAttackSpeed, true);
			}
			if (projectile != null)
			{
				if (this.AlternatingFiringOffset)
				{
					this.flipFiringOffset = !this.flipFiringOffset;
				}
				this.activeProjectiles.Add(projectile);
				projectile.Drop(null, true, false);
				if (projectile.body != null)
				{
					projectile.body.Dir = 1f;
					projectile.body.ResetDynamics();
					projectile.body.Enabled = true;
				}
				float spread = MathHelper.ToRadians(this.Spread) * Rand.Range(-0.5f, 0.5f, Rand.RandSync.Unsynced);
				Vector2 launchPos = ConvertUnits.ToSimUnits(this.GetRelativeFiringPosition(true));
				Body pickedBody = Submarine.PickBody(ConvertUnits.ToSimUnits(this.item.WorldPosition), launchPos, null, new Category?(Category.Cat1), true, delegate(Fixture f)
				{
					Submarine sub = f.Body.UserData as Submarine;
					return sub == null || sub != this.item.Submarine;
				}, true);
				if (pickedBody != null)
				{
					launchPos = Submarine.LastPickedPosition;
				}
				projectile.SetTransform(launchPos, -(launchRotation ?? this.Rotation) + spread, true, true, null);
				projectile.UpdateTransform();
				PhysicsBody body = projectile.body;
				projectile.Submarine = ((body != null) ? body.Submarine : null);
				Projectile projectileComponent = projectile.GetComponent<Projectile>();
				if (projectileComponent != null)
				{
					this.TryDetermineProjectileSpeed(projectileComponent);
					projectileComponent.Launcher = this.item;
					Projectile projectile2 = projectileComponent;
					projectileComponent.User = user;
					projectile2.Attacker = user;
					if (projectileComponent.Attack != null)
					{
						projectileComponent.Attack.DamageMultiplier = 1f * this.DamageMultiplier + 0.2f * tinkeringStrength;
					}
					projectileComponent.Use(null, this.LaunchImpulse);
					TriggerComponent trigger = this.item.GetComponent<TriggerComponent>();
					if (trigger != null)
					{
						projectileComponent.IgnoredBodies.Add(trigger.PhysicsBody.FarseerBody);
					}
					Rope component = projectile.GetComponent<Rope>();
					if (component != null)
					{
						component.Attach(this.item, projectile);
					}
					projectileComponent.User = user;
					if (this.item.Submarine != null && projectile.body != null)
					{
						Vector2 velocitySum = this.item.Submarine.PhysicsBody.LinearVelocity + projectile.body.LinearVelocity;
						if (velocitySum.LengthSquared() < 3686.4f)
						{
							projectile.body.LinearVelocity = velocitySum;
						}
					}
				}
				Item container = projectile.Container;
				if (container != null)
				{
					container.RemoveContained(projectile);
				}
			}
			base.ApplyStatusEffects(ActionType.OnUse, 1f, null, null, null, user, null, 1f);
			this.LaunchProjSpecific();
		}

		// Token: 0x060062B1 RID: 25265 RVA: 0x003339B8 File Offset: 0x00331BB8
		private void TryDetermineProjectileSpeed(Projectile projectile)
		{
			if (projectile != null && !projectile.Hitscan)
			{
				this.projectileSpeed = ConvertUnits.ToDisplayUnits(MathHelper.Clamp((projectile.LaunchImpulse + this.LaunchImpulse) / projectile.Item.body.Mass, 20f, 64f));
			}
		}

		// Token: 0x060062B2 RID: 25266 RVA: 0x00333A08 File Offset: 0x00331C08
		private void LaunchProjSpecific()
		{
			this.recoilTimer = this.RetractionTime;
			if (this.user != null)
			{
				this.recoilTimer /= 1f + this.user.GetStatValue(StatTypes.TurretAttackSpeed, true);
			}
			base.PlaySound(ActionType.OnUse, null);
			Vector2 particlePos = this.GetRelativeFiringPosition(true);
			foreach (ParticleEmitter emitter in this.particleEmitters)
			{
				emitter.Emit(1f, particlePos, null, -this.Rotation, this.Rotation, 1f, 1f, 1f, null, null, false, null);
			}
		}

		// Token: 0x060062B3 RID: 25267 RVA: 0x00333AD0 File Offset: 0x00331CD0
		private static void ShiftItemsInProjectileContainer(ItemContainer container)
		{
			if (container == null)
			{
				return;
			}
			bool moved;
			do
			{
				moved = false;
				for (int i = 1; i < container.Capacity; i++)
				{
					Item item = container.Inventory.GetItemAt(i);
					if (item != null && container.Inventory.CanBePutInSlot(item, i - 1, false) && container.Inventory.TryPutItem(item, i - 1, false, false, null, true, false, true))
					{
						moved = true;
					}
				}
			}
			while (moved);
		}

		// Token: 0x060062B4 RID: 25268 RVA: 0x00333B32 File Offset: 0x00331D32
		private float GetTargetPriorityModifier()
		{
			if (this.currentChargingState != Turret.ChargingState.WindingUp)
			{
				return this.AICurrentTargetPriorityMultiplier;
			}
			return 10f;
		}

		// Token: 0x060062B5 RID: 25269 RVA: 0x00333B4C File Offset: 0x00331D4C
		public void UpdateAutoOperate(float deltaTime, bool ignorePower, Identifier friendlyTag = default(Identifier))
		{
			if (!ignorePower && !this.HasPowerToShoot())
			{
				return;
			}
			this.IsActive = true;
			if (friendlyTag.IsEmpty)
			{
				friendlyTag = this.FriendlyTag;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.updatePending)
			{
				if (this.updateTimer < 0f)
				{
					this.prevTargetRotation = this.targetRotation;
					this.updateTimer = 0.25f;
				}
				this.updateTimer -= deltaTime;
			}
			if (this.AimDelay && this.waitTimer > 0f)
			{
				this.waitTimer -= deltaTime;
				return;
			}
			Submarine closestSub = null;
			float maxDistance = 10000f;
			float shootDistance = this.AIRange;
			ISpatialEntity target = null;
			float closestDist = shootDistance * shootDistance;
			if (this.TargetCharacters)
			{
				foreach (Character character in Character.CharacterList)
				{
					if (Turret.IsValidTarget(character))
					{
						float priority = this.isSlowTurret ? character.Params.AISlowTurretPriority : character.Params.AITurretPriority;
						if (priority > 0f && this.IsValidTargetForAutoOperate(character, friendlyTag))
						{
							float dist = Vector2.DistanceSquared(character.WorldPosition, this.item.WorldPosition);
							if (dist <= closestDist && this.IsWithinAimingRadius(character.WorldPosition))
							{
								target = character;
								if (this.currentTarget != null && target == this.currentTarget)
								{
									priority *= this.GetTargetPriorityModifier();
								}
								closestDist = dist / priority;
							}
						}
					}
				}
			}
			if (this.TargetItems)
			{
				foreach (Item targetItem in Item.TurretTargetItems)
				{
					if (Turret.IsValidTarget(targetItem))
					{
						float priority2 = this.isSlowTurret ? targetItem.Prefab.AISlowTurretPriority : targetItem.Prefab.AITurretPriority;
						if (priority2 > 0f)
						{
							float dist2 = Vector2.DistanceSquared(this.item.WorldPosition, targetItem.WorldPosition);
							if (dist2 <= closestDist && dist2 <= shootDistance * shootDistance && this.IsTargetItemCloseEnough(targetItem, dist2) && this.IsWithinAimingRadius(targetItem.WorldPosition))
							{
								target = targetItem;
								if (this.currentTarget != null && target == this.currentTarget)
								{
									priority2 *= this.GetTargetPriorityModifier();
								}
								closestDist = dist2 / priority2;
							}
						}
					}
				}
			}
			if (this.TargetSubmarines && (target == null || target.Submarine != null))
			{
				closestDist = maxDistance * maxDistance;
				foreach (Submarine sub in Submarine.Loaded)
				{
					if (sub != base.Item.Submarine && !sub.IsRespawnShuttle && (this.item.Submarine == null || !Character.IsOnFriendlyTeam(this.item.Submarine.TeamID, sub.TeamID)))
					{
						float dist3 = Vector2.DistanceSquared(sub.WorldPosition, this.item.WorldPosition);
						if (dist3 <= closestDist)
						{
							closestSub = sub;
							closestDist = dist3;
						}
					}
				}
				closestDist = shootDistance * shootDistance;
				if (closestSub != null)
				{
					foreach (Hull hull in Hull.HullList)
					{
						if (closestSub.IsEntityFoundOnThisSub(hull, true, false, false))
						{
							float dist4 = Vector2.DistanceSquared(hull.WorldPosition, this.item.WorldPosition);
							if (dist4 <= closestDist)
							{
								target = hull;
								closestDist = dist4;
							}
						}
					}
				}
			}
			if (target == null && this.RandomMovement)
			{
				this.waitTimer = ((Rand.Value(Rand.RandSync.Unsynced) < 0.98f) ? 0f : Rand.Range(5f, 20f, Rand.RandSync.Unsynced));
				this.targetRotation = Rand.Range(this.minRotation, this.maxRotation, Rand.RandSync.Unsynced);
				this.updatePending = true;
				return;
			}
			if (this.AimDelay && this.RandomAimAmount > 0f)
			{
				if (this.randomAimTimer < 0f)
				{
					this.randomAimTimer = Rand.Range(this.RandomAimMinTime, this.RandomAimMaxTime, Rand.RandSync.Unsynced);
					this.waitTimer = Rand.Range(0.25f, 1f, Rand.RandSync.Unsynced);
					float randomAim = MathHelper.ToRadians(this.RandomAimAmount);
					this.targetRotation = MathUtils.WrapAngleTwoPi(this.targetRotation += Rand.Range(-randomAim, randomAim, Rand.RandSync.Unsynced));
					this.updatePending = true;
					return;
				}
				this.randomAimTimer -= deltaTime;
			}
			if (target == null)
			{
				return;
			}
			this.currentTarget = target;
			float angle = -MathUtils.VectorToAngle(target.WorldPosition - this.item.WorldPosition);
			this.targetRotation = MathUtils.WrapAngleTwoPi(angle);
			if (Math.Abs(this.targetRotation - this.prevTargetRotation) > 0.1f)
			{
				this.updatePending = true;
			}
			Hull targetHull = target as Hull;
			if (targetHull != null)
			{
				Vector2 barrelDir = this.GetBarrelDir();
				Vector2 vector;
				if (!MathUtils.GetLineWorldRectangleIntersection(this.item.WorldPosition, this.item.WorldPosition + barrelDir * this.AIRange, targetHull.WorldRect, out vector))
				{
					return;
				}
			}
			else
			{
				if (!this.IsWithinAimingRadius(angle))
				{
					return;
				}
				if (!this.IsPointingTowards(target.WorldPosition))
				{
					return;
				}
			}
			Vector2 start = ConvertUnits.ToSimUnits(this.item.WorldPosition);
			Vector2 end = ConvertUnits.ToSimUnits(target.WorldPosition);
			bool doLineOfSightCheck = this.lastLineOfSightCheck.Item3 < Timing.TotalTimeUnpaused - 0.5;
			if (doLineOfSightCheck)
			{
				this.lastLineOfSightCheck.Item1 = this.CheckLineOfSight(start, end);
				this.lastLineOfSightCheck.Item3 = Timing.TotalTime;
			}
			Body worldTarget = this.lastLineOfSightCheck.Item1;
			bool shoot;
			if (target.Submarine != null)
			{
				if (doLineOfSightCheck)
				{
					start -= target.Submarine.SimPosition;
					end -= target.Submarine.SimPosition;
					this.lastLineOfSightCheck.Item2 = this.CheckLineOfSight(start, end);
				}
				shoot = ((worldTarget == null || this.CanShoot(worldTarget, null, friendlyTag, this.TargetSubmarines, false)) && this.CanShoot(this.lastLineOfSightCheck.Item2, null, friendlyTag, this.TargetSubmarines, false));
			}
			else
			{
				shoot = this.CanShoot(worldTarget, null, friendlyTag, this.TargetSubmarines, false);
			}
			if (shoot)
			{
				this.TryLaunch(deltaTime, null, ignorePower);
			}
		}

		// Token: 0x060062B6 RID: 25270 RVA: 0x003341DC File Offset: 0x003323DC
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			Turret.<>c__DisplayClass331_0 CS$<>8__locals1 = new Turret.<>c__DisplayClass331_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			AITarget selectedAiTarget = CS$<>8__locals1.character.AIController.SelectedAiTarget;
			Character previousTarget = ((selectedAiTarget != null) ? selectedAiTarget.Entity : null) as Character;
			if (previousTarget != null && previousTarget.IsDead)
			{
				if (previousTarget.LastAttacker == null || previousTarget.LastAttacker == CS$<>8__locals1.character)
				{
					Character character2 = CS$<>8__locals1.character;
					string value = TextManager.Get("DialogTurretTargetDead").Value;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("killedtarget");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(previousTarget.ID);
					Identifier identifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
					character2.Speak(value, null, 0f, identifier, 5f);
				}
				CS$<>8__locals1.character.AIController.SelectTarget(null);
			}
			if (!this.HasPowerToShoot())
			{
				float lowestCharge = 0f;
				PowerContainer batteryToLoad = null;
				foreach (PowerContainer battery in base.GetDirectlyConnectedBatteries())
				{
					if (battery.Item.IsInteractable(CS$<>8__locals1.character) && !battery.OutputDisabled)
					{
						if (batteryToLoad == null || battery.Charge < lowestCharge)
						{
							batteryToLoad = battery;
							lowestCharge = battery.Charge;
						}
						if (battery.Item.ConditionPercentage <= 0f && AIObjectiveRepairItems.IsValidTarget(battery.Item, CS$<>8__locals1.character))
						{
							IEnumerable<Repairable> repairables = battery.Item.Repairables;
							Func<Repairable, float> selector;
							if ((selector = CS$<>8__locals1.<>9__0) == null)
							{
								selector = (CS$<>8__locals1.<>9__0 = ((Repairable r) => r.DegreeOfSuccess(CS$<>8__locals1.character)));
							}
							if (repairables.Average(selector) > 0.4f)
							{
								objective.AddSubObjective(new AIObjectiveRepairItem(CS$<>8__locals1.character, battery.Item, objective.objectiveManager, 1f, true), false);
								return false;
							}
							Character character3 = CS$<>8__locals1.character;
							string value2 = TextManager.Get("DialogSupercapacitorIsBroken").Value;
							Identifier identifier = "supercapacitorisbroken".ToIdentifier();
							character3.Speak(value2, null, 0f, identifier, 30f);
						}
					}
				}
				if (batteryToLoad == null)
				{
					return true;
				}
				if (batteryToLoad.RechargeSpeed < batteryToLoad.MaxRechargeSpeed * 0.4f)
				{
					objective.AddSubObjective(new AIObjectiveOperateItem(batteryToLoad, CS$<>8__locals1.character, objective.objectiveManager, Identifier.Empty, false, null, false, null, 1f), false);
					return false;
				}
				if (lowestCharge <= 0f && batteryToLoad.Item.ConditionPercentage > 0f)
				{
					Character character4 = CS$<>8__locals1.character;
					string value3 = TextManager.Get("DialogTurretHasNoPower").Value;
					Identifier identifier = "turrethasnopower".ToIdentifier();
					character4.Speak(value3, null, 0f, identifier, 30f);
				}
			}
			int usableProjectileCount = 0;
			int maxProjectileCount = 0;
			foreach (MapEntity e in this.item.linkedTo)
			{
				if (this.item.IsInteractable(CS$<>8__locals1.character) && this.item.Prefab.IsLinkAllowed(e.Prefab))
				{
					Item projectileContainer = e as Item;
					if (projectileContainer != null)
					{
						ItemContainer container = projectileContainer.GetComponent<ItemContainer>();
						if (container != null)
						{
							maxProjectileCount += container.Capacity;
							IEnumerable<Item> projectiles = from it in projectileContainer.ContainedItems
							where it.Condition > 0f
							select it;
							Item firstProjectile = projectiles.FirstOrDefault<Item>();
							ItemPrefab itemPrefab = (firstProjectile != null) ? firstProjectile.Prefab : null;
							Item item = this.previousAmmo;
							if (itemPrefab != ((item != null) ? item.Prefab : null))
							{
								this.projectileSpeed = float.PositiveInfinity;
							}
							this.previousAmmo = firstProjectile;
							if (projectiles.Any<Item>())
							{
								Projectile projectile2;
								if ((projectile2 = firstProjectile.GetComponent<Projectile>()) == null)
								{
									Item item2 = firstProjectile.ContainedItems.FirstOrDefault<Item>();
									projectile2 = ((item2 != null) ? item2.GetComponent<Projectile>() : null);
								}
								Projectile projectile = projectile2;
								this.TryDetermineProjectileSpeed(projectile);
								usableProjectileCount += projectiles.Count<Item>();
							}
						}
					}
				}
			}
			if (usableProjectileCount == 0)
			{
				Turret.<>c__DisplayClass331_1 CS$<>8__locals2 = new Turret.<>c__DisplayClass331_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.container = null;
				Item containerItem = null;
				foreach (MapEntity e2 in this.item.linkedTo)
				{
					containerItem = (e2 as Item);
					if (containerItem != null && containerItem.IsInteractable(CS$<>8__locals2.CS$<>8__locals1.character))
					{
						HumanAIController aiController = CS$<>8__locals2.CS$<>8__locals1.character.AIController as HumanAIController;
						if (aiController == null || !aiController.IgnoredItems.Contains(containerItem))
						{
							CS$<>8__locals2.container = containerItem.GetComponent<ItemContainer>();
							if (CS$<>8__locals2.container != null)
							{
								break;
							}
						}
					}
				}
				if (CS$<>8__locals2.container == null || !CS$<>8__locals2.container.ContainableItemIdentifiers.Any<Identifier>())
				{
					if (CS$<>8__locals2.CS$<>8__locals1.character.IsOnPlayerTeam)
					{
						Character character5 = CS$<>8__locals2.CS$<>8__locals1.character;
						string value4 = TextManager.GetWithVariable("DialogCannotLoadTurret", "[itemname]", this.item.Name, FormatCapitals.Yes).Value;
						Identifier identifier = "cannotloadturret".ToIdentifier();
						character5.Speak(value4, null, 0f, identifier, 30f);
					}
					return true;
				}
				if (objective.SubObjectives.None(null))
				{
					AIObjectiveContainItem loadItemsObjective = base.AIContainItems<Turret>(CS$<>8__locals2.container, CS$<>8__locals2.CS$<>8__locals1.character, objective, usableProjectileCount + 1, true, true, false, true);
					loadItemsObjective.ignoredContainerIdentifiers = containerItem.Prefab.Identifier.ToEnumerable<Identifier>().ToImmutableHashSet<Identifier>();
					if (CS$<>8__locals2.CS$<>8__locals1.character.IsOnPlayerTeam)
					{
						Character character6 = CS$<>8__locals2.CS$<>8__locals1.character;
						string value5 = TextManager.GetWithVariable("DialogLoadTurret", "[itemname]", this.item.Name, FormatCapitals.Yes).Value;
						Identifier identifier = "loadturret".ToIdentifier();
						character6.Speak(value5, null, 0f, identifier, 30f);
					}
					loadItemsObjective.Abandoned += CS$<>8__locals2.<CrewAIOperate>g__CheckRemainingAmmo|2;
					loadItemsObjective.Completed += CS$<>8__locals2.<CrewAIOperate>g__CheckRemainingAmmo|2;
					return false;
				}
				if (objective.SubObjectives.Any<AIObjective>())
				{
					return false;
				}
			}
			CS$<>8__locals1.closestEnemy = null;
			CS$<>8__locals1.targetPos = null;
			float maxDistance = 10000f;
			float shootDistance = this.AIRange * this.item.OffsetOnSelectedMultiplier;
			float closestDistance = maxDistance * maxDistance;
			bool hadCurrentTarget = this.currentTarget != null;
			if (hadCurrentTarget)
			{
				bool isValidTarget = Turret.IsValidTarget(this.currentTarget);
				if (isValidTarget)
				{
					float dist = Vector2.DistanceSquared(this.item.WorldPosition, this.currentTarget.WorldPosition);
					if (dist > closestDistance)
					{
						isValidTarget = false;
					}
					else
					{
						Item targetItem = this.currentTarget as Item;
						if (targetItem != null && !this.IsTargetItemCloseEnough(targetItem, dist))
						{
							isValidTarget = false;
						}
					}
				}
				if (!isValidTarget)
				{
					this.currentTarget = null;
					this.aiFindTargetTimer = 0.2f;
				}
			}
			if (this.aiFindTargetTimer <= 0f)
			{
				foreach (Character enemy in Character.CharacterList)
				{
					if (Turret.IsValidTarget(enemy))
					{
						float priority = this.isSlowTurret ? enemy.Params.AISlowTurretPriority : enemy.Params.AITurretPriority;
						if (priority > 0f && (CS$<>8__locals1.character.Submarine == null || (enemy.Submarine != CS$<>8__locals1.character.Submarine && (enemy.Submarine == null || (enemy.Submarine.TeamID != CS$<>8__locals1.character.Submarine.TeamID && !enemy.Submarine.Info.IsOutpost)))) && (enemy.IsHuman || enemy.CurrentHull == null) && !HumanAIController.IsFriendly(CS$<>8__locals1.character, enemy, false, true) && !enemy.LockHands)
						{
							float dist2 = Vector2.DistanceSquared(enemy.WorldPosition, this.item.WorldPosition);
							if (dist2 <= closestDistance && (dist2 >= shootDistance * shootDistance || this.IsWithinAimingRadius(enemy.WorldPosition)))
							{
								if (this.currentTarget != null && enemy == this.currentTarget)
								{
									priority *= this.GetTargetPriorityModifier();
								}
								CS$<>8__locals1.targetPos = new Vector2?(enemy.WorldPosition);
								CS$<>8__locals1.closestEnemy = enemy;
								closestDistance = dist2 / priority;
								this.currentTarget = CS$<>8__locals1.closestEnemy;
							}
						}
					}
				}
				foreach (Item targetItem2 in Item.TurretTargetItems)
				{
					if (Turret.IsValidTarget(targetItem2))
					{
						float priority2 = this.isSlowTurret ? targetItem2.Prefab.AISlowTurretPriority : targetItem2.Prefab.AITurretPriority;
						if (priority2 > 0f)
						{
							float dist3 = Vector2.DistanceSquared(this.item.WorldPosition, targetItem2.WorldPosition);
							if (dist3 <= closestDistance && dist3 <= shootDistance * shootDistance && this.IsTargetItemCloseEnough(targetItem2, dist3) && this.IsWithinAimingRadius(targetItem2.WorldPosition))
							{
								if (this.currentTarget != null && targetItem2 == this.currentTarget)
								{
									priority2 *= this.GetTargetPriorityModifier();
								}
								CS$<>8__locals1.targetPos = new Vector2?(targetItem2.WorldPosition);
								closestDistance = dist3 / priority2;
								CS$<>8__locals1.closestEnemy = null;
								this.currentTarget = targetItem2;
							}
						}
					}
				}
				this.aiFindTargetTimer = ((this.currentTarget == null) ? 1f : 0.2f);
			}
			else if (this.currentTarget != null)
			{
				CS$<>8__locals1.targetPos = new Vector2?(this.currentTarget.WorldPosition);
			}
			bool iceSpireSpotted = false;
			Vector2 targetVelocity = Vector2.Zero;
			Character targetCharacter = this.currentTarget as Character;
			if (targetCharacter != null)
			{
				bool enemyInAnotherSub = targetCharacter.Submarine != null && targetCharacter.CurrentHull != null && targetCharacter.Submarine != this.item.Submarine;
				bool canSeeTarget = true;
				if (enemyInAnotherSub && (this.lastCanSeeTargetCheck.Item3 < Timing.TotalTime - 0.5 || targetCharacter != this.lastCanSeeTargetCheck.Item1))
				{
					canSeeTarget = targetCharacter.CanSeeTarget(base.Item, null, false, false);
					this.lastCanSeeTargetCheck = new ValueTuple<Character, bool, double>(targetCharacter, canSeeTarget, Timing.TotalTime);
				}
				if (enemyInAnotherSub && !canSeeTarget)
				{
					CS$<>8__locals1.targetPos = new Vector2?(targetCharacter.CurrentHull.WorldPosition);
					if (closestDistance > maxDistance * maxDistance)
					{
						CS$<>8__locals1.<CrewAIOperate>g__ResetTarget|4();
					}
				}
				else
				{
					float closestDistSqr = closestDistance;
					foreach (Limb limb in targetCharacter.AnimController.Limbs)
					{
						if (!limb.IsSevered && !limb.Hidden && this.IsWithinAimingRadius(limb.WorldPosition))
						{
							float distSqr = Vector2.DistanceSquared(limb.WorldPosition, this.item.WorldPosition);
							if (distSqr < closestDistSqr)
							{
								closestDistSqr = distSqr;
								if (limb == targetCharacter.AnimController.MainLimb)
								{
									closestDistSqr *= 0.5f;
								}
								CS$<>8__locals1.targetPos = new Vector2?(limb.WorldPosition);
							}
						}
					}
					if (this.projectileSpeed < float.PositiveInfinity && CS$<>8__locals1.targetPos != null)
					{
						float dist4 = MathF.Sqrt(closestDistSqr);
						float projectileMovementTime = dist4 / this.projectileSpeed;
						targetVelocity = targetCharacter.AnimController.Collider.LinearVelocity;
						Vector2 movementAmount = targetVelocity * projectileMovementTime;
						movementAmount = ConvertUnits.ToDisplayUnits(movementAmount.ClampLength(10f));
						Vector2 futurePosition = CS$<>8__locals1.targetPos.Value + movementAmount;
						CS$<>8__locals1.targetPos = new Vector2?(Vector2.Lerp(CS$<>8__locals1.targetPos.Value, futurePosition, base.DegreeOfSuccess(CS$<>8__locals1.character)));
					}
					if (closestDistSqr > shootDistance * shootDistance)
					{
						this.aiFindTargetTimer = 0.2f;
						CS$<>8__locals1.<CrewAIOperate>g__ResetTarget|4();
					}
				}
			}
			else if (CS$<>8__locals1.targetPos == null && this.item.Submarine != null && Level.Loaded != null)
			{
				shootDistance = this.AIRange * this.item.OffsetOnSelectedMultiplier;
				closestDistance = shootDistance;
				foreach (LevelWall wall in Level.Loaded.ExtraWalls)
				{
					DestructibleLevelWall destructibleWall = wall as DestructibleLevelWall;
					if (destructibleWall != null && !destructibleWall.Destroyed)
					{
						foreach (VoronoiCell cell in wall.Cells)
						{
							if (cell.DoesDamage)
							{
								foreach (GraphEdge edge in cell.Edges)
								{
									Vector2 p = edge.Point1 + cell.Translation;
									Vector2 p2 = edge.Point2 + cell.Translation;
									Vector2 closestPoint = MathUtils.GetClosestPointOnLineSegment(p, p2, this.item.WorldPosition);
									if (!this.IsWithinAimingRadius(closestPoint))
									{
										Vector2 barrelDir = new Vector2((float)Math.Cos((double)this.Rotation), -(float)Math.Sin((double)this.Rotation));
										Vector2 intersection;
										if (!MathUtils.GetLineSegmentIntersection(p, p2, this.item.WorldPosition, this.item.WorldPosition + barrelDir * shootDistance, out intersection))
										{
											continue;
										}
										closestPoint = intersection;
										if (!this.IsWithinAimingRadius(closestPoint))
										{
											continue;
										}
									}
									float dist5 = Vector2.Distance(closestPoint, this.item.WorldPosition);
									closestPoint += (closestPoint - this.item.WorldPosition) / Math.Max(dist5, 1f);
									if (dist5 <= this.AIRange + 1000f)
									{
										float dot = 0f;
										if (!MathUtils.NearlyEqual(this.item.Submarine.Velocity, Vector2.Zero, 0.0001f))
										{
											dot = Vector2.Dot(Vector2.Normalize(this.item.Submarine.Velocity), Vector2.Normalize(closestPoint - this.item.Submarine.WorldPosition));
										}
										float minAngle = 0.5f;
										if (dot >= minAngle || dist5 <= 1000f)
										{
											dist5 -= MathHelper.Lerp(0f, 1000f, MathUtils.InverseLerp(minAngle, 1f, dot));
											if (dist5 <= closestDistance)
											{
												CS$<>8__locals1.targetPos = new Vector2?(closestPoint);
												closestDistance = dist5;
												iceSpireSpotted = true;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (CS$<>8__locals1.targetPos == null)
			{
				return false;
			}
			objective.ForceHighestPriority = true;
			this.debugDrawTargetPos = new Vector2?(CS$<>8__locals1.targetPos.Value);
			if (CS$<>8__locals1.closestEnemy != null && CS$<>8__locals1.character.AIController.SelectedAiTarget != CS$<>8__locals1.closestEnemy.AiTarget)
			{
				if (CS$<>8__locals1.character.IsOnPlayerTeam)
				{
					if (CS$<>8__locals1.character.AIController.SelectedAiTarget == null && !hadCurrentTarget)
					{
						if (CreatureMetrics.RecentlyEncountered.Contains(CS$<>8__locals1.closestEnemy.SpeciesName) || CS$<>8__locals1.closestEnemy.IsHuman)
						{
							Character character7 = CS$<>8__locals1.character;
							string value6 = TextManager.Get("DialogNewTargetSpotted").Value;
							Identifier identifier = "newtargetspotted".ToIdentifier();
							character7.Speak(value6, null, 0f, identifier, 30f);
						}
						else if (CreatureMetrics.Encountered.Contains(CS$<>8__locals1.closestEnemy.SpeciesName))
						{
							Character character8 = CS$<>8__locals1.character;
							string value7 = TextManager.GetWithVariable("DialogIdentifiedTargetSpotted", "[speciesname]", CS$<>8__locals1.closestEnemy.DisplayName, FormatCapitals.No).Value;
							Identifier identifier = "identifiedtargetspotted".ToIdentifier();
							character8.Speak(value7, null, 0f, identifier, 30f);
						}
						else
						{
							Character character9 = CS$<>8__locals1.character;
							string value8 = TextManager.Get("DialogUnidentifiedTargetSpotted").Value;
							Identifier identifier = "unidentifiedtargetspotted".ToIdentifier();
							character9.Speak(value8, null, 0f, identifier, 5f);
						}
					}
					else if (!CreatureMetrics.Encountered.Contains(CS$<>8__locals1.closestEnemy.SpeciesName))
					{
						Character character10 = CS$<>8__locals1.character;
						string value9 = TextManager.Get("DialogUnidentifiedTargetSpotted").Value;
						Identifier identifier = "unidentifiedtargetspotted".ToIdentifier();
						character10.Speak(value9, null, 0f, identifier, 5f);
					}
					CreatureMetrics.AddEncounter(CS$<>8__locals1.closestEnemy.SpeciesName);
				}
				CS$<>8__locals1.character.AIController.SelectTarget(CS$<>8__locals1.closestEnemy.AiTarget);
			}
			else if (iceSpireSpotted && CS$<>8__locals1.character.IsOnPlayerTeam)
			{
				Character character11 = CS$<>8__locals1.character;
				string value10 = TextManager.Get("DialogIceSpireSpotted").Value;
				Identifier identifier = "icespirespotted".ToIdentifier();
				character11.Speak(value10, null, 0f, identifier, 60f);
			}
			CS$<>8__locals1.character.CursorPosition = CS$<>8__locals1.targetPos.Value;
			if (CS$<>8__locals1.character.Submarine != null)
			{
				CS$<>8__locals1.character.CursorPosition -= CS$<>8__locals1.character.Submarine.Position;
			}
			if (this.IsPointingTowards(CS$<>8__locals1.targetPos.Value))
			{
				Vector2 barrelDir2 = this.GetBarrelDir();
				Vector2 aimStartPos = this.item.WorldPosition;
				Vector2 aimEndPos = this.item.WorldPosition + barrelDir2 * shootDistance;
				bool allowShootingIfNothingInWay = false;
				if (this.currentTarget != null)
				{
					Vector2 targetStartPos = this.currentTarget.WorldPosition;
					Vector2 targetEndPos = this.currentTarget.WorldPosition + targetVelocity * ConvertUnits.ToDisplayUnits(10f);
					allowShootingIfNothingInWay = (targetVelocity.LengthSquared() > 0.001f && MathUtils.LineSegmentsIntersect(aimStartPos, aimEndPos, targetStartPos, targetEndPos) && Math.Abs(Vector2.Dot(Vector2.Normalize(aimEndPos - aimStartPos), Vector2.Normalize(targetEndPos - targetStartPos))) < 0.5f);
				}
				Vector2 start = ConvertUnits.ToSimUnits(aimStartPos);
				Vector2 end = ConvertUnits.ToSimUnits(aimEndPos);
				Body worldTarget = this.CheckLineOfSight(start, end);
				bool canShoot;
				if (CS$<>8__locals1.closestEnemy != null && CS$<>8__locals1.closestEnemy.Submarine != null)
				{
					start -= CS$<>8__locals1.closestEnemy.Submarine.SimPosition;
					end -= CS$<>8__locals1.closestEnemy.Submarine.SimPosition;
					Body transformedTarget = this.CheckLineOfSight(start, end);
					Body targetBody = transformedTarget;
					Character character12 = CS$<>8__locals1.character;
					bool allowShootingIfNothingInWay2 = allowShootingIfNothingInWay;
					bool flag;
					if (this.CanShoot(targetBody, character12, default(Identifier), true, allowShootingIfNothingInWay2))
					{
						if (worldTarget != null)
						{
							Body targetBody2 = worldTarget;
							Character character13 = CS$<>8__locals1.character;
							allowShootingIfNothingInWay2 = allowShootingIfNothingInWay;
							flag = this.CanShoot(targetBody2, character13, default(Identifier), true, allowShootingIfNothingInWay2);
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = false;
					}
					canShoot = flag;
				}
				else
				{
					Body targetBody3 = worldTarget;
					Character character14 = CS$<>8__locals1.character;
					bool allowShootingIfNothingInWay2 = allowShootingIfNothingInWay;
					canShoot = this.CanShoot(targetBody3, character14, default(Identifier), true, allowShootingIfNothingInWay2);
				}
				if (!canShoot)
				{
					return false;
				}
				if (CS$<>8__locals1.character.IsOnPlayerTeam)
				{
					Character character15 = CS$<>8__locals1.character;
					string value11 = TextManager.Get("DialogFireTurret").Value;
					Identifier identifier = "fireturret".ToIdentifier();
					character15.Speak(value11, null, 0f, identifier, 30f);
				}
				CS$<>8__locals1.character.SetInput(InputType.Shoot, true, true);
			}
			return false;
		}

		// Token: 0x060062B7 RID: 25271 RVA: 0x00335644 File Offset: 0x00333844
		private bool IsPointingTowards(Vector2 targetPos)
		{
			float enemyAngle = MathUtils.VectorToAngle(targetPos - this.item.WorldPosition);
			float turretAngle = -this.Rotation;
			float maxAngleError = MathHelper.ToRadians(this.MaxAngleOffset);
			if (this.MaxChargeTime > 0f && this.currentChargingState == Turret.ChargingState.WindingUp && this.FiringRotationSpeedModifier > 0f)
			{
				maxAngleError *= 2f;
			}
			return Math.Abs(MathUtils.GetShortestAngle(enemyAngle, turretAngle)) <= maxAngleError;
		}

		// Token: 0x060062B8 RID: 25272 RVA: 0x003356B9 File Offset: 0x003338B9
		private bool IsTargetItemCloseEnough(Item target, float sqrDist)
		{
			return float.IsPositiveInfinity(target.Prefab.AITurretTargetingMaxDistance) || sqrDist < MathUtils.Pow2(target.Prefab.AITurretTargetingMaxDistance);
		}

		// Token: 0x060062B9 RID: 25273 RVA: 0x003356E2 File Offset: 0x003338E2
		public override float GetCurrentPowerConsumption(Connection conn = null)
		{
			return 0f;
		}

		// Token: 0x060062BA RID: 25274 RVA: 0x003356EC File Offset: 0x003338EC
		private static bool IsValidTarget(ISpatialEntity target)
		{
			if (target == null)
			{
				return false;
			}
			Character targetCharacter = target as Character;
			if (targetCharacter != null)
			{
				if (!targetCharacter.Enabled || targetCharacter.Removed || targetCharacter.IsDead || targetCharacter.AITurretPriority <= 0f)
				{
					return false;
				}
			}
			else
			{
				Item targetItem = target as Item;
				if (targetItem != null)
				{
					if (targetItem.Removed || targetItem.Condition <= 0f || !targetItem.Prefab.IsAITurretTarget || targetItem.Prefab.AITurretPriority <= 0f || targetItem.IsHidden)
					{
						return false;
					}
					if (targetItem.Submarine != null)
					{
						return false;
					}
					if (targetItem.ParentInventory != null)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060062BB RID: 25275 RVA: 0x0033578C File Offset: 0x0033398C
		private CharacterTeamType GetFriendlyTeam()
		{
			CharacterTeamType result;
			switch (this.FriendlyTeamType)
			{
			case Turret.TeamType.OwnSub:
			{
				Submarine submarine = this.item.Submarine;
				result = ((submarine != null) ? submarine.TeamID : CharacterTeamType.None);
				break;
			}
			case Turret.TeamType.Team1:
				result = CharacterTeamType.Team1;
				break;
			case Turret.TeamType.Team2:
				result = CharacterTeamType.Team2;
				break;
			case Turret.TeamType.FriendlyNPC:
				result = CharacterTeamType.FriendlyNPC;
				break;
			case Turret.TeamType.NoneTeam:
				result = CharacterTeamType.None;
				break;
			default:
				throw new NotImplementedException();
			}
			return result;
		}

		// Token: 0x060062BC RID: 25276 RVA: 0x003357F0 File Offset: 0x003339F0
		private bool IsValidTargetForAutoOperate(Character target, Identifier friendlyTag)
		{
			if (!friendlyTag.IsEmpty)
			{
				Identifier identifier = target.SpeciesName;
				if (!identifier.Equals(friendlyTag))
				{
					identifier = target.Group;
					if (!identifier.Equals(friendlyTag))
					{
						goto IL_2D;
					}
				}
				return false;
			}
			IL_2D:
			CharacterTeamType friendlyTeam = this.GetFriendlyTeam();
			if (target.TeamID == friendlyTeam)
			{
				return false;
			}
			bool flag;
			if (!target.IsHuman)
			{
				Identifier identifier = target.Group;
				flag = (identifier == CharacterPrefab.HumanSpeciesName);
			}
			else
			{
				flag = true;
			}
			bool isHuman = flag;
			if (isHuman)
			{
				return !target.IsOnFriendlyTeam(friendlyTeam) && this.TargetHumans;
			}
			return this.TargetMonsters;
		}

		// Token: 0x060062BD RID: 25277 RVA: 0x00335878 File Offset: 0x00333A78
		private bool CanShoot(Body targetBody, Character user = null, Identifier friendlyTag = default(Identifier), bool targetSubmarines = true, bool allowShootingIfNothingInWay = false)
		{
			if (targetBody == null)
			{
				return allowShootingIfNothingInWay;
			}
			Character targetCharacter = null;
			Character c = targetBody.UserData as Character;
			if (c != null)
			{
				targetCharacter = c;
			}
			else
			{
				Limb limb = targetBody.UserData as Limb;
				if (limb != null)
				{
					targetCharacter = limb.character;
				}
			}
			if (targetCharacter != null && !targetCharacter.Removed)
			{
				if (user != null)
				{
					if (HumanAIController.IsFriendly(user, targetCharacter, false, true))
					{
						return false;
					}
				}
				else if (!this.IsValidTargetForAutoOperate(targetCharacter, friendlyTag))
				{
					return false;
				}
			}
			else
			{
				ISpatialEntity e = targetBody.UserData as ISpatialEntity;
				if (e != null)
				{
					Structure structure = e as Structure;
					if (structure != null && structure.Indestructible)
					{
						return false;
					}
					if (!targetSubmarines && e is Submarine)
					{
						return false;
					}
					Submarine sub = e.Submarine ?? (e as Submarine);
					if (sub == null)
					{
						return true;
					}
					if (sub == base.Item.Submarine)
					{
						return false;
					}
					if (sub.Info.IsOutpost || sub.Info.IsWreck || sub.Info.IsBeacon || sub.Info.IsRuin)
					{
						return false;
					}
					if (this.item.Submarine == null)
					{
						if (sub.TeamID == this.GetFriendlyTeam())
						{
							return false;
						}
					}
					else if (sub.TeamID == base.Item.Submarine.TeamID)
					{
						return false;
					}
				}
				else
				{
					VoronoiCell voronoiCell = targetBody.UserData as VoronoiCell;
					if (voronoiCell == null || !voronoiCell.IsDestructible)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060062BE RID: 25278 RVA: 0x003359D4 File Offset: 0x00333BD4
		private Body CheckLineOfSight(Vector2 start, Vector2 end)
		{
			Category collisionCategories = Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8;
			return Submarine.PickBody(start, end, null, new Category?(collisionCategories), true, delegate(Fixture f)
			{
				Item i = f.UserData as Item;
				return (i == null || i.GetComponent<Turret>() == null) && f.CollidesWith != Category.None && f.Body.UserData != this.item && !(f.UserData is Hull) && !this.item.StaticFixtures.Contains(f);
			}, true);
		}

		// Token: 0x060062BF RID: 25279 RVA: 0x00335A08 File Offset: 0x00333C08
		private Vector2 GetRelativeFiringPosition(bool useOffset = true)
		{
			Vector2 transformedFiringOffset = Vector2.Zero;
			if (useOffset)
			{
				Vector2 currOffSet = this.FiringOffset;
				if (this.flipFiringOffset)
				{
					currOffSet.X = -currOffSet.X;
				}
				transformedFiringOffset = MathUtils.RotatePoint(new Vector2(-currOffSet.Y, -currOffSet.X) * this.item.Scale, -this.Rotation);
			}
			return new Vector2((float)this.item.WorldRect.X + this.transformedBarrelPos.X + transformedFiringOffset.X, (float)this.item.WorldRect.Y - this.transformedBarrelPos.Y + transformedFiringOffset.Y);
		}

		// Token: 0x060062C0 RID: 25280 RVA: 0x00335AB8 File Offset: 0x00333CB8
		private bool IsWithinAimingRadius(float angle)
		{
			float midRotation = (this.minRotation + this.maxRotation) / 2f;
			while (midRotation - angle < -3.1415927f)
			{
				angle -= 6.2831855f;
			}
			while (midRotation - angle > 3.1415927f)
			{
				angle += 6.2831855f;
			}
			return angle >= this.minRotation && angle <= this.maxRotation;
		}

		// Token: 0x060062C1 RID: 25281 RVA: 0x00335B1A File Offset: 0x00333D1A
		public bool IsWithinAimingRadius(Vector2 target)
		{
			return this.IsWithinAimingRadius(-MathUtils.VectorToAngle(target - this.item.WorldPosition));
		}

		// Token: 0x060062C2 RID: 25282 RVA: 0x00335B3C File Offset: 0x00333D3C
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sprite sprite = this.barrelSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.barrelSprite = null;
			Sprite sprite2 = this.railSprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			this.railSprite = null;
			Sprite sprite3 = this.barrelSpriteBroken;
			if (sprite3 != null)
			{
				sprite3.Remove();
			}
			this.barrelSpriteBroken = null;
			Sprite sprite4 = this.railSpriteBroken;
			if (sprite4 != null)
			{
				sprite4.Remove();
			}
			this.railSpriteBroken = null;
			Sprite sprite5 = this.crosshairSprite;
			if (sprite5 != null)
			{
				sprite5.Remove();
			}
			this.crosshairSprite = null;
			Sprite sprite6 = this.crosshairPointerSprite;
			if (sprite6 != null)
			{
				sprite6.Remove();
			}
			this.crosshairPointerSprite = null;
			SoundChannel soundChannel = this.moveSoundChannel;
			if (soundChannel != null)
			{
				soundChannel.Dispose();
			}
			this.moveSoundChannel = null;
			Sprite weaponIndicatorSprite = this.WeaponIndicatorSprite;
			if (weaponIndicatorSprite != null)
			{
				weaponIndicatorSprite.Remove();
			}
			this.WeaponIndicatorSprite = null;
			if (this.powerIndicator != null)
			{
				this.powerIndicator.RectTransform.Parent = null;
				this.powerIndicator = null;
			}
		}

		// Token: 0x060062C3 RID: 25283 RVA: 0x00335C30 File Offset: 0x00333E30
		private List<Projectile> GetLoadedProjectiles()
		{
			List<Projectile> projectiles = new List<Projectile>();
			bool flag;
			Turret.CheckProjectileContainer(this.item, projectiles, out flag);
			for (int i = 0; i < this.item.linkedTo.Count; i++)
			{
				MapEntity e = this.item.linkedTo[(i + this.currentLoaderIndex) % this.item.linkedTo.Count];
				if (this.item.Prefab.IsLinkAllowed(e.Prefab))
				{
					Item projectileContainer = e as Item;
					if (projectileContainer != null)
					{
						bool stopSearching;
						Turret.CheckProjectileContainer(projectileContainer, projectiles, out stopSearching);
						if (projectiles.Any<Projectile>() || stopSearching)
						{
							return projectiles;
						}
					}
				}
			}
			return projectiles;
		}

		// Token: 0x060062C4 RID: 25284 RVA: 0x00335CD4 File Offset: 0x00333ED4
		private static void CheckProjectileContainer(Item projectileContainer, List<Projectile> projectiles, out bool stopSearching)
		{
			stopSearching = false;
			if (projectileContainer.Condition <= 0f)
			{
				return;
			}
			IEnumerable<Item> containedItems = projectileContainer.ContainedItems;
			if (containedItems == null)
			{
				return;
			}
			foreach (Item containedItem in containedItems)
			{
				Projectile projectileComponent = containedItem.GetComponent<Projectile>();
				if (projectileComponent != null && projectileComponent.Item.body != null)
				{
					projectiles.Add(projectileComponent);
					break;
				}
				foreach (Item subContainedItem in containedItem.ContainedItems)
				{
					projectileComponent = subContainedItem.GetComponent<Projectile>();
					if (projectileComponent != null && projectileComponent.Item.body != null)
					{
						projectiles.Add(projectileComponent);
					}
				}
				if (containedItem.Condition > 0f || projectiles.Any<Projectile>())
				{
					stopSearching = true;
					break;
				}
			}
		}

		// Token: 0x060062C5 RID: 25285 RVA: 0x00335DCC File Offset: 0x00333FCC
		public override void FlipX(bool relativeToSub)
		{
			this.minRotation = 3.1415927f - this.minRotation;
			this.maxRotation = 3.1415927f - this.maxRotation;
			float temp = this.minRotation;
			this.minRotation = this.maxRotation;
			this.maxRotation = temp;
			this.barrelPos.X = (float)this.item.Rect.Width / this.item.Scale - this.barrelPos.X;
			while (this.minRotation < 0f)
			{
				this.minRotation += 6.2831855f;
				this.maxRotation += 6.2831855f;
			}
			this.targetRotation = (this.Rotation = (this.minRotation + this.maxRotation) / 2f);
			this.UpdateTransformedBarrelPos();
			this.UpdateLightComponents();
		}

		// Token: 0x060062C6 RID: 25286 RVA: 0x00335EAC File Offset: 0x003340AC
		public override void FlipY(bool relativeToSub)
		{
			this.BaseRotation = MathHelper.ToDegrees(MathUtils.WrapAngleTwoPi(MathHelper.ToRadians(180f - this.BaseRotation)));
			this.minRotation = -this.minRotation;
			this.maxRotation = -this.maxRotation;
			float temp = this.minRotation;
			this.minRotation = this.maxRotation;
			this.maxRotation = temp;
			while (this.minRotation < 0f)
			{
				this.minRotation += 6.2831855f;
				this.maxRotation += 6.2831855f;
			}
			this.targetRotation = (this.Rotation = (this.minRotation + this.maxRotation) / 2f);
			this.UpdateTransformedBarrelPos();
			this.UpdateLightComponents();
		}

		// Token: 0x060062C7 RID: 25287 RVA: 0x00335F70 File Offset: 0x00334170
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			Character sender = signal.sender;
			string name = connection.Name;
			if (!(name == "position_in"))
			{
				if (!(name == "trigger_in"))
				{
					if (!(name == "toggle_light"))
					{
						if (!(name == "set_light"))
						{
							if (!(name == "set_auto_operate"))
							{
								if (!(name == "toggle_auto_operate"))
								{
									return;
								}
								if (!this.AllowAutoOperateWithWiring)
								{
									return;
								}
								if (signal.value != "0")
								{
									this.AutoOperate = !this.AutoOperate;
								}
							}
							else
							{
								if (!this.AllowAutoOperateWithWiring)
								{
									return;
								}
								this.AutoOperate = (signal.value != "0");
								return;
							}
						}
						else if (this.lightComponents != null)
						{
							bool shouldBeOn = signal.value != "0";
							foreach (LightComponent light in this.lightComponents)
							{
								light.IsOn = shouldBeOn;
							}
							this.UpdateLightComponents();
							return;
						}
					}
					else if (this.lightComponents != null && signal.value != "0")
					{
						foreach (LightComponent light2 in this.lightComponents)
						{
							light2.IsOn = !light2.IsOn;
						}
						this.UpdateLightComponents();
						return;
					}
				}
				else
				{
					if (signal.value == "0")
					{
						return;
					}
					this.item.Use(0.016666668f, sender, null, null, null);
					this.user = sender;
					this.ActiveUser = sender;
					this.resetActiveUserTimer = 1f;
					this.resetUserTimer = 10f;
					if (!this.characterUsable && sender != null)
					{
						this.TryLaunch(0.016666668f, sender, false);
						return;
					}
				}
				return;
			}
			float newRotation;
			if (float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newRotation))
			{
				if (!MathUtils.IsValid(newRotation))
				{
					return;
				}
				this.targetRotation = MathHelper.ToRadians(newRotation);
				this.IsActive = true;
			}
			this.user = sender;
			this.ActiveUser = sender;
			this.resetActiveUserTimer = 1f;
			this.resetUserTimer = 10f;
		}

		// Token: 0x060062C8 RID: 25288 RVA: 0x003361DC File Offset: 0x003343DC
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			string key = "rotationlimits";
			Vector2 rotationLimits = this.RotationLimits;
			this.loadedRotationLimits = new Vector2?(componentElement.GetAttributeVector2(key, rotationLimits));
			this.loadedBaseRotation = new float?(componentElement.GetAttributeFloat("baserotation", componentElement.Parent.GetAttributeFloat("rotation", this.BaseRotation)));
			XAttribute friendlyTeamAttribute = componentElement.GetAttribute("FriendlyTeam");
			if (friendlyTeamAttribute != null)
			{
				Turret.TeamType value;
				switch (XMLExtensions.ParseEnumValue<CharacterTeamType>(friendlyTeamAttribute.Value, CharacterTeamType.None, friendlyTeamAttribute))
				{
				case CharacterTeamType.None:
					value = Turret.TeamType.OwnSub;
					break;
				case CharacterTeamType.Team1:
					value = Turret.TeamType.Team1;
					break;
				case CharacterTeamType.Team2:
					value = Turret.TeamType.Team2;
					break;
				case CharacterTeamType.FriendlyNPC:
					value = Turret.TeamType.FriendlyNPC;
					break;
				default:
					throw new NotImplementedException();
				}
				this.loadedFriendlyTeamType = new Turret.TeamType?(value);
			}
		}

		// Token: 0x060062C9 RID: 25289 RVA: 0x00336298 File Offset: 0x00334498
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.FindLightComponents();
			this.targetRotation = this.Rotation;
			if (this.loadedBaseRotation == null)
			{
				if (this.item.FlippedX)
				{
					this.FlipX(false);
				}
				if (this.item.FlippedY)
				{
					this.FlipY(false);
				}
			}
			this.UpdateTransformedBarrelPos();
			this.UpdateLightComponents();
		}

		// Token: 0x060062CA RID: 25290 RVA: 0x00336300 File Offset: 0x00334500
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Turret.EventData eventData;
			if (base.TryExtractEventData<Turret.EventData>(extraData, out eventData))
			{
				Item projectile = eventData.Projectile;
				msg.WriteUInt16((projectile != null) ? projectile.ID : ushort.MaxValue);
				msg.WriteRangedSingle(MathHelper.Clamp(this.<ServerEventWrite>g__wrapAngle|354_0(this.Rotation), this.minRotation, this.maxRotation), this.minRotation, this.maxRotation, 16);
				return;
			}
			msg.WriteUInt16(0);
			msg.WriteRangedSingle(MathHelper.Clamp(this.<ServerEventWrite>g__wrapAngle|354_0(this.targetRotation), this.minRotation, this.maxRotation), this.minRotation, this.maxRotation, 16);
		}

		// Token: 0x060062D1 RID: 25297 RVA: 0x003365EE File Offset: 0x003347EE
		[CompilerGenerated]
		private void <Draw>g__UpdateBarrel|65_2()
		{
			this.Rotation = (this.minRotation + this.maxRotation) / 2f;
		}

		// Token: 0x060062D2 RID: 25298 RVA: 0x0033660C File Offset: 0x0033480C
		[CompilerGenerated]
		internal static void <DrawHUD>g__AddAmmoFromContainer|72_0(ItemContainer itemContainer, ref Turret.<>c__DisplayClass72_0 A_1)
		{
			if (itemContainer == null)
			{
				return;
			}
			A_1.availableAmmo.AddRange(itemContainer.Inventory.AllItems);
			for (int i = 0; i < itemContainer.Inventory.Capacity - itemContainer.Inventory.AllItems.Count<Item>(); i++)
			{
				A_1.availableAmmo.Add(null);
			}
		}

		// Token: 0x060062D3 RID: 25299 RVA: 0x00336668 File Offset: 0x00334868
		[CompilerGenerated]
		private bool <TryLaunch>g__tryUseProjectileContainer|318_1(Item containerItem, ref Turret.<>c__DisplayClass318_0 A_2, ref Turret.<>c__DisplayClass318_1 A_3)
		{
			ItemContainer projectileContainer = containerItem.GetComponent<ItemContainer>();
			if (projectileContainer != null)
			{
				containerItem.Use(A_2.deltaTime, null, null, null, this.user);
				A_3.projectiles = this.GetLoadedProjectiles();
				if (A_3.projectiles.Any<Projectile>())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060062D6 RID: 25302 RVA: 0x0033674C File Offset: 0x0033494C
		[CompilerGenerated]
		private float <ServerEventWrite>g__wrapAngle|354_0(float angle)
		{
			float wrappedAngle;
			for (wrappedAngle = angle; wrappedAngle < this.minRotation; wrappedAngle += 6.2831855f)
			{
				if (!MathUtils.IsValid(wrappedAngle))
				{
					break;
				}
			}
			while (wrappedAngle > this.maxRotation && MathUtils.IsValid(wrappedAngle))
			{
				wrappedAngle -= 6.2831855f;
			}
			return wrappedAngle;
		}

		// Token: 0x04003297 RID: 12951
		private Sprite crosshairSprite;

		// Token: 0x04003298 RID: 12952
		private Sprite crosshairPointerSprite;

		// Token: 0x04003299 RID: 12953
		public Sprite WeaponIndicatorSprite;

		// Token: 0x0400329A RID: 12954
		private GUIProgressBar powerIndicator;

		// Token: 0x0400329B RID: 12955
		private Vector2? debugDrawTargetPos;

		// Token: 0x0400329C RID: 12956
		private float recoilTimer;

		// Token: 0x0400329D RID: 12957
		private RoundSound startMoveSound;

		// Token: 0x0400329E RID: 12958
		private RoundSound endMoveSound;

		// Token: 0x0400329F RID: 12959
		private RoundSound moveSound;

		// Token: 0x040032A0 RID: 12960
		private RoundSound chargeSound;

		// Token: 0x040032A1 RID: 12961
		private SoundChannel moveSoundChannel;

		// Token: 0x040032A2 RID: 12962
		private SoundChannel chargeSoundChannel;

		// Token: 0x040032A3 RID: 12963
		private Vector2 oldRotation = Vector2.Zero;

		// Token: 0x040032A4 RID: 12964
		private Vector2 crosshairPos;

		// Token: 0x040032A5 RID: 12965
		private Vector2 crosshairPointerPos;

		// Token: 0x040032A6 RID: 12966
		private readonly Dictionary<string, Widget> widgets = new Dictionary<string, Widget>();

		// Token: 0x040032A7 RID: 12967
		private float prevAngle;

		// Token: 0x040032A8 RID: 12968
		private float currentBarrelSpin;

		// Token: 0x040032A9 RID: 12969
		private bool flashLowPower;

		// Token: 0x040032AA RID: 12970
		private bool flashNoAmmo;

		// Token: 0x040032AB RID: 12971
		private bool flashLoaderBroken;

		// Token: 0x040032AC RID: 12972
		private float flashTimer;

		// Token: 0x040032AD RID: 12973
		private readonly float flashLength = 1f;

		// Token: 0x040032AE RID: 12974
		private const float MaxCircle = 360f;

		// Token: 0x040032AF RID: 12975
		private const float HalfCircle = 180f;

		// Token: 0x040032B0 RID: 12976
		private const float QuarterCircle = 90f;

		// Token: 0x040032B1 RID: 12977
		private readonly List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();

		// Token: 0x040032B2 RID: 12978
		private readonly List<ParticleEmitter> particleEmitterCharges = new List<ParticleEmitter>();

		// Token: 0x040032B8 RID: 12984
		private Vector2 _chargeSoundWindupPitchSlide;

		// Token: 0x040032B9 RID: 12985
		private Sprite barrelSprite;

		// Token: 0x040032BA RID: 12986
		private Sprite railSprite;

		// Token: 0x040032BB RID: 12987
		private Sprite barrelSpriteBroken;

		// Token: 0x040032BC RID: 12988
		private Sprite railSpriteBroken;

		// Token: 0x040032BD RID: 12989
		[TupleElementNames(new string[]
		{
			"sprite",
			"position"
		})]
		private readonly List<ValueTuple<Sprite, Vector2>> chargeSprites = new List<ValueTuple<Sprite, Vector2>>();

		// Token: 0x040032BE RID: 12990
		private readonly List<Sprite> spinningBarrelSprites = new List<Sprite>();

		// Token: 0x040032BF RID: 12991
		private const ushort LaunchWithoutProjectileId = 65535;

		// Token: 0x040032C0 RID: 12992
		private Vector2 barrelPos;

		// Token: 0x040032C1 RID: 12993
		private Vector2 transformedBarrelPos;

		// Token: 0x040032C2 RID: 12994
		private float targetRotation;

		// Token: 0x040032C3 RID: 12995
		private float reload;

		// Token: 0x040032C4 RID: 12996
		private int shotCounter;

		// Token: 0x040032C5 RID: 12997
		private float minRotation;

		// Token: 0x040032C6 RID: 12998
		private float maxRotation;

		// Token: 0x040032C7 RID: 12999
		private Camera cam;

		// Token: 0x040032C8 RID: 13000
		private float angularVelocity;

		// Token: 0x040032C9 RID: 13001
		private int failedLaunchAttempts;

		// Token: 0x040032CA RID: 13002
		private float currentChargeTime;

		// Token: 0x040032CB RID: 13003
		private bool tryingToCharge;

		// Token: 0x040032CC RID: 13004
		private const float LineOfSightCheckInterval = 0.5f;

		// Token: 0x040032CD RID: 13005
		[TupleElementNames(new string[]
		{
			"WorldTarget",
			"TransformedTarget",
			"Time"
		})]
		private ValueTuple<Body, Body, double> lastLineOfSightCheck;

		// Token: 0x040032CE RID: 13006
		[TupleElementNames(new string[]
		{
			"Target",
			"CanSee",
			"Time"
		})]
		private ValueTuple<Character, bool, double> lastCanSeeTargetCheck;

		// Token: 0x040032CF RID: 13007
		private Turret.ChargingState currentChargingState;

		// Token: 0x040032D0 RID: 13008
		private readonly List<Item> activeProjectiles = new List<Item>();

		// Token: 0x040032D1 RID: 13009
		private Character user;

		// Token: 0x040032D2 RID: 13010
		private float resetUserTimer;

		// Token: 0x040032D3 RID: 13011
		private float aiFindTargetTimer;

		// Token: 0x040032D4 RID: 13012
		private ISpatialEntity currentTarget;

		// Token: 0x040032D5 RID: 13013
		private const float CrewAiFindTargetMaxInterval = 1f;

		// Token: 0x040032D6 RID: 13014
		private const float CrewAIFindTargetMinInverval = 0.2f;

		// Token: 0x040032D7 RID: 13015
		private const float MinimumProjectileVelocityForAimAhead = 20f;

		// Token: 0x040032D8 RID: 13016
		private const float MaximumAimAhead = 10f;

		// Token: 0x040032D9 RID: 13017
		private float projectileSpeed;

		// Token: 0x040032DA RID: 13018
		private Item previousAmmo;

		// Token: 0x040032DB RID: 13019
		private int currentLoaderIndex;

		// Token: 0x040032DC RID: 13020
		private const float TinkeringPowerCostReduction = 0.2f;

		// Token: 0x040032DD RID: 13021
		private const float TinkeringDamageIncrease = 0.2f;

		// Token: 0x040032DE RID: 13022
		private const float TinkeringReloadDecrease = 0.2f;

		// Token: 0x040032DF RID: 13023
		public Character ActiveUser;

		// Token: 0x040032E0 RID: 13024
		private float resetActiveUserTimer;

		// Token: 0x040032E1 RID: 13025
		private List<LightComponent> lightComponents;

		// Token: 0x040032E2 RID: 13026
		private Projectile lastProjectile;

		// Token: 0x040032E3 RID: 13027
		private readonly bool isSlowTurret;

		// Token: 0x040032E6 RID: 13030
		private bool flipFiringOffset;

		// Token: 0x040032EF RID: 13039
		private float prevScale;

		// Token: 0x040032F0 RID: 13040
		private float prevBaseRotation;

		// Token: 0x040032F2 RID: 13042
		private float _maxAngleOffset;

		// Token: 0x04003311 RID: 13073
		private const string SetAutoOperateConnection = "set_auto_operate";

		// Token: 0x04003312 RID: 13074
		private const string ToggleAutoOperateConnection = "toggle_auto_operate";

		// Token: 0x04003313 RID: 13075
		private bool isUseBeingCalled;

		// Token: 0x04003314 RID: 13076
		private float waitTimer;

		// Token: 0x04003315 RID: 13077
		private float randomAimTimer;

		// Token: 0x04003316 RID: 13078
		private float prevTargetRotation;

		// Token: 0x04003317 RID: 13079
		private float updateTimer;

		// Token: 0x04003318 RID: 13080
		private bool updatePending;

		// Token: 0x04003319 RID: 13081
		private Vector2? loadedRotationLimits;

		// Token: 0x0400331A RID: 13082
		private float? loadedBaseRotation;

		// Token: 0x0400331B RID: 13083
		private Turret.TeamType? loadedFriendlyTeamType;

		// Token: 0x0200147D RID: 5245
		private enum ChargingState
		{
			// Token: 0x040065F2 RID: 26098
			Inactive,
			// Token: 0x040065F3 RID: 26099
			WindingUp,
			// Token: 0x040065F4 RID: 26100
			WindingDown
		}

		// Token: 0x0200147E RID: 5246
		public enum TeamType
		{
			// Token: 0x040065F6 RID: 26102
			OwnSub,
			// Token: 0x040065F7 RID: 26103
			Team1,
			// Token: 0x040065F8 RID: 26104
			Team2,
			// Token: 0x040065F9 RID: 26105
			FriendlyNPC,
			// Token: 0x040065FA RID: 26106
			NoneTeam
		}

		// Token: 0x0200147F RID: 5247
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x06009B2A RID: 39722 RVA: 0x003E4775 File Offset: 0x003E2975
			public EventData(Item projectile, Turret turret)
			{
				this.Projectile = projectile;
			}

			// Token: 0x040065FB RID: 26107
			public readonly Item Projectile;
		}
	}
}
