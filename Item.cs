using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000C5 RID: 197
	internal class Item : MapEntity, IDamageable, ISerializableEntity, IServerSerializable, INetSerializable, IClientSerializable, IIgnorable, ISpatialEntity, IServerPositionSync
	{
		// Token: 0x170005FA RID: 1530
		// (get) Token: 0x06001878 RID: 6264 RVA: 0x000F5FAE File Offset: 0x000F41AE
		// (set) Token: 0x06001879 RID: 6265 RVA: 0x000F5FB6 File Offset: 0x000F41B6
		public GUIComponentStyle IconStyle
		{
			get
			{
				return this.iconStyle;
			}
			private set
			{
				if (this.IconStyle != value)
				{
					this.iconStyle = value;
					this.CheckIsHighlighted();
				}
			}
		}

		// Token: 0x170005FB RID: 1531
		// (get) Token: 0x0600187A RID: 6266 RVA: 0x000F5FCE File Offset: 0x000F41CE
		public IEnumerable<ItemComponent> ActiveHUDs
		{
			get
			{
				return this.activeHUDs;
			}
		}

		// Token: 0x170005FC RID: 1532
		// (get) Token: 0x0600187B RID: 6267 RVA: 0x000F5FD6 File Offset: 0x000F41D6
		// (set) Token: 0x0600187C RID: 6268 RVA: 0x000F5FDE File Offset: 0x000F41DE
		public bool FakeBroken
		{
			get
			{
				return this.fakeBroken;
			}
			set
			{
				if (value != this.fakeBroken)
				{
					this.fakeBroken = value;
					this.SetActiveSprite();
				}
			}
		}

		// Token: 0x170005FD RID: 1533
		// (get) Token: 0x0600187D RID: 6269 RVA: 0x000F5FF6 File Offset: 0x000F41F6
		public override Sprite Sprite
		{
			get
			{
				return this.activeSprite;
			}
		}

		// Token: 0x170005FE RID: 1534
		// (get) Token: 0x0600187E RID: 6270 RVA: 0x000F5FFE File Offset: 0x000F41FE
		// (set) Token: 0x0600187F RID: 6271 RVA: 0x000F6006 File Offset: 0x000F4206
		public override Rectangle Rect
		{
			get
			{
				return base.Rect;
			}
			set
			{
				this.cachedVisibleExtents = null;
				base.Rect = value;
			}
		}

		// Token: 0x170005FF RID: 1535
		// (get) Token: 0x06001880 RID: 6272 RVA: 0x000F601C File Offset: 0x000F421C
		public override bool DrawBelowWater
		{
			get
			{
				SubEditorScreen editor = Screen.Selected as SubEditorScreen;
				return (editor == null || !editor.WiringMode || !this.isWire || !this.isLogic) && (base.DrawBelowWater || this.ParentInventory is CharacterInventory);
			}
		}

		// Token: 0x17000600 RID: 1536
		// (get) Token: 0x06001881 RID: 6273 RVA: 0x000F606C File Offset: 0x000F426C
		public override bool DrawOverWater
		{
			get
			{
				if (!base.DrawOverWater)
				{
					if (!base.IsSelected)
					{
						SubEditorScreen editor = Screen.Selected as SubEditorScreen;
						if (editor == null || !editor.WiringMode)
						{
							return false;
						}
					}
					return this.isWire || this.isLogic;
				}
				return true;
			}
		}

		// Token: 0x17000601 RID: 1537
		// (get) Token: 0x06001882 RID: 6274 RVA: 0x000F60B4 File Offset: 0x000F42B4
		private GUITextBlock ItemInUseWarning
		{
			get
			{
				if (this.itemInUseWarning == null)
				{
					this.itemInUseWarning = new GUITextBlock(new RectTransform(new Point(10), GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", new Color?(GUIStyle.Orange), null, Alignment.Center, false, "OuterGlow", new Color?(Color.Black));
				}
				return this.itemInUseWarning;
			}
		}

		// Token: 0x17000602 RID: 1538
		// (get) Token: 0x06001883 RID: 6275 RVA: 0x000F6124 File Offset: 0x000F4324
		public override bool SelectableInEditor
		{
			get
			{
				return !GameMain.SubEditorScreen.IsSubcategoryHidden(this.Prefab.Subcategory) && SubEditorScreen.IsLayerVisible(this) && (this.parentInventory == null && (this.body == null || this.body.Enabled)) && Item.ShowItems;
			}
		}

		// Token: 0x06001884 RID: 6276 RVA: 0x000F6178 File Offset: 0x000F4378
		public override float GetDrawDepth()
		{
			return base.GetDrawDepth(base.SpriteDepth + this.DrawDepthOffset, this.Sprite);
		}

		// Token: 0x06001885 RID: 6277 RVA: 0x000F6194 File Offset: 0x000F4394
		public Color GetSpriteColor(Color? defaultColor = null, bool withHighlight = false)
		{
			Color color = defaultColor ?? this.spriteColor;
			if (this.Prefab.UseContainedSpriteColor && this.ownInventory != null)
			{
				using (IEnumerator<Item> enumerator = this.ContainedItems.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Item item = enumerator.Current;
						color = item.ContainerColor;
					}
				}
			}
			if (withHighlight)
			{
				if (base.IsHighlighted && !GUI.DisableItemHighlights && Screen.Selected != GameMain.GameScreen)
				{
					color = GUIStyle.Orange * Math.Max((float)this.GetSpriteColor(null, false).A / 255f, 0.1f);
				}
				else if (base.IsHighlighted && this.HighlightColor != null)
				{
					color = Color.Lerp(color, this.HighlightColor.Value, (MathF.Sin((float)Timing.TotalTime * 3f) + 1f) / 2f);
				}
			}
			return color;
		}

		// Token: 0x06001886 RID: 6278 RVA: 0x000F62B0 File Offset: 0x000F44B0
		protected override void CheckIsHighlighted()
		{
			if (base.IsHighlighted || base.ExternalHighlight || this.IconStyle != null)
			{
				MapEntity.highlightedEntities.Add(this);
				return;
			}
			MapEntity.highlightedEntities.Remove(this);
		}

		// Token: 0x06001887 RID: 6279 RVA: 0x000F62E4 File Offset: 0x000F44E4
		public Color GetInventoryIconColor()
		{
			Color color = this.InventoryIconColor;
			if (this.Prefab.UseContainedInventoryIconColor && this.ownInventory != null)
			{
				using (IEnumerator<Item> enumerator = this.ContainedItems.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Item item = enumerator.Current;
						color = item.ContainerColor;
					}
				}
			}
			return color;
		}

		// Token: 0x06001888 RID: 6280 RVA: 0x000F6350 File Offset: 0x000F4550
		public void InitSpriteStates()
		{
			Sprite sprite = this.Prefab.Sprite;
			if (sprite != null)
			{
				sprite.EnsureLazyLoaded(false);
			}
			Sprite inventoryIcon = this.Prefab.InventoryIcon;
			if (inventoryIcon != null)
			{
				inventoryIcon.EnsureLazyLoaded(false);
			}
			foreach (BrokenItemSprite brokenSprite in this.Prefab.BrokenSprites)
			{
				brokenSprite.Sprite.EnsureLazyLoaded(false);
			}
			foreach (DecorativeSprite decorativeSprite in this.Prefab.DecorativeSprites)
			{
				decorativeSprite.Sprite.EnsureLazyLoaded(false);
				this.spriteAnimState.Add(decorativeSprite, new DecorativeSprite.State());
			}
			this.SetActiveSprite();
			this.UpdateSpriteStates(0f);
		}

		// Token: 0x06001889 RID: 6281 RVA: 0x000F6417 File Offset: 0x000F4617
		public void ResetCachedVisibleSize()
		{
			this.cachedVisibleExtents = null;
		}

		// Token: 0x0600188A RID: 6282 RVA: 0x000F6428 File Offset: 0x000F4628
		public override bool IsVisible(Rectangle worldView)
		{
			if (this.container != null)
			{
				return false;
			}
			if (!this.hasComponentsToDraw && this.body != null && !this.body.Enabled)
			{
				return false;
			}
			Inventory inventory = this.parentInventory;
			Character character = ((inventory != null) ? inventory.Owner : null) as Character;
			if (character != null && character.InvisibleTimer > 0f)
			{
				return false;
			}
			Rectangle extents;
			if (this.cachedVisibleExtents != null)
			{
				extents = this.cachedVisibleExtents.Value;
			}
			else
			{
				int padding = 0;
				RectangleF boundingBox = this.GetTransformedQuad().BoundingAxisAlignedRectangle;
				Vector2 min = new Vector2(-boundingBox.Width / 2f - (float)padding, -boundingBox.Height / 2f - (float)padding);
				Vector2 max = -min;
				foreach (IDrawableComponent drawable in this.drawableComponents)
				{
					min.X = Math.Min(min.X, -drawable.DrawSize.X / 2f);
					min.Y = Math.Min(min.Y, -drawable.DrawSize.Y / 2f);
					max.X = Math.Max(max.X, drawable.DrawSize.X / 2f);
					max.Y = Math.Max(max.Y, drawable.DrawSize.Y / 2f);
				}
				foreach (DecorativeSprite decorativeSprite in this.Prefab.DecorativeSprites)
				{
					Vector2 scale = decorativeSprite.GetScale(ref this.spriteAnimState[decorativeSprite].ScaleState, this.spriteAnimState[decorativeSprite].RandomScaleFactor) * this.Scale;
					min.X = Math.Min(-decorativeSprite.Sprite.size.X * decorativeSprite.Sprite.RelativeOrigin.X * scale.X, min.X);
					min.Y = Math.Min(-decorativeSprite.Sprite.size.Y * (1f - decorativeSprite.Sprite.RelativeOrigin.Y) * scale.Y, min.Y);
					max.X = Math.Max(decorativeSprite.Sprite.size.X * (1f - decorativeSprite.Sprite.RelativeOrigin.X) * scale.X, max.X);
					max.Y = Math.Max(decorativeSprite.Sprite.size.Y * decorativeSprite.Sprite.RelativeOrigin.Y * scale.Y, max.Y);
				}
				extents = new Rectangle(min.ToPoint(), max.ToPoint());
				this.cachedVisibleExtents = new Rectangle?(extents);
			}
			Vector2 worldPosition = this.WorldPosition + base.GetCollapseEffectOffset();
			return worldPosition.X + (float)extents.X <= (float)worldView.Right && worldPosition.X + (float)extents.Width >= (float)worldView.X && worldPosition.Y + (float)extents.Height >= (float)(worldView.Y - worldView.Height) && worldPosition.Y + (float)extents.Y <= (float)worldView.Y;
		}

		// Token: 0x0600188B RID: 6283 RVA: 0x000F67D4 File Offset: 0x000F49D4
		public override void Draw(SpriteBatch spriteBatch, bool editing, bool back = true)
		{
			this.Draw(spriteBatch, editing, back, null, null);
		}

		// Token: 0x0600188C RID: 6284 RVA: 0x000F67FC File Offset: 0x000F49FC
		public void Draw(SpriteBatch spriteBatch, bool editing, bool back = true, Color? overrideColor = null, float? overrideDepth = null)
		{
			Item.<>c__DisplayClass45_0 CS$<>8__locals1;
			CS$<>8__locals1.overrideColor = overrideColor;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.editing = editing;
			if (!this.Visible || (!CS$<>8__locals1.editing && base.IsHidden) || !SubEditorScreen.IsLayerVisible(this))
			{
				return;
			}
			if (CS$<>8__locals1.editing)
			{
				if (this.isWire)
				{
					if (!Item.ShowWires)
					{
						return;
					}
				}
				else if (!Item.ShowItems)
				{
					return;
				}
			}
			Color color = this.<Draw>g__GetSpriteColor|45_1(this.spriteColor, ref CS$<>8__locals1);
			bool isWiringMode = CS$<>8__locals1.editing && SubEditorScreen.TransparentWiringMode && SubEditorScreen.IsWiringMode() && !this.isWire && this.parentInventory == null;
			bool renderTransparent = isWiringMode && this.GetComponent<ConnectionPanel>() == null;
			if (renderTransparent)
			{
				color *= 0.15f;
			}
			if (Character.Controlled != null && Character.DebugDrawInteract)
			{
				color = Color.Red;
				foreach (ItemComponent ic in this.components)
				{
					Item.InteractionVisibility interactionType = Item.GetComponentInteractionVisibility(Character.Controlled, ic);
					if (interactionType == Item.InteractionVisibility.MissingRequirement)
					{
						color = Color.Orange;
					}
					else if (interactionType == Item.InteractionVisibility.Visible)
					{
						color = Color.LightGreen;
						break;
					}
				}
			}
			BrokenItemSprite fadeInBrokenSprite = null;
			float fadeInBrokenSpriteAlpha = 0f;
			float displayCondition = this.FakeBroken ? 0f : this.ConditionPercentageRelativeToDefaultMaxCondition;
			Vector2 drawOffset = base.GetCollapseEffectOffset();
			drawOffset.Y = -drawOffset.Y;
			if (displayCondition < this.MaxCondition)
			{
				for (int i = 0; i < this.Prefab.BrokenSprites.Length; i++)
				{
					if (this.Prefab.BrokenSprites[i].FadeIn)
					{
						float min = (i > 0) ? this.Prefab.BrokenSprites[i - i].MaxConditionPercentage : 0f;
						float max = this.Prefab.BrokenSprites[i].MaxConditionPercentage;
						fadeInBrokenSpriteAlpha = 1f - (displayCondition - min) / (max - min);
						if (fadeInBrokenSpriteAlpha > 0f && fadeInBrokenSpriteAlpha <= 1f)
						{
							fadeInBrokenSprite = this.Prefab.BrokenSprites[i];
						}
					}
					else if (displayCondition <= this.Prefab.BrokenSprites[i].MaxConditionPercentage)
					{
						this.activeSprite = this.Prefab.BrokenSprites[i].Sprite;
						drawOffset = this.Prefab.BrokenSprites[i].Offset.ToVector2() * this.Scale;
						break;
					}
				}
			}
			float? num = overrideDepth;
			float depth = num ?? this.GetDrawDepth();
			if (isWiringMode && this.isLogic && !PlayerInput.IsShiftDown())
			{
				depth = 0.01f;
			}
			if (this.activeSprite != null)
			{
				SpriteEffects oldEffects = this.activeSprite.effects;
				this.activeSprite.effects ^= this.SpriteEffects;
				SpriteEffects oldBrokenSpriteEffects = SpriteEffects.None;
				if (fadeInBrokenSprite != null && fadeInBrokenSprite.Sprite != this.activeSprite)
				{
					oldBrokenSpriteEffects = fadeInBrokenSprite.Sprite.effects;
					fadeInBrokenSprite.Sprite.effects ^= this.SpriteEffects;
				}
				if (this.body == null || this.body.BodyType == BodyType.Static)
				{
					if (this.Prefab.ResizeHorizontal || this.Prefab.ResizeVertical)
					{
						if (color.A > 0)
						{
							Vector2 size = new Vector2((float)this.rect.Width, (float)this.rect.Height);
							Sprite sprite = this.activeSprite;
							Vector2 position = new Vector2(this.DrawPosition.X - (float)(this.rect.Width / 2), -(this.DrawPosition.Y + (float)(this.rect.Height / 2))) + drawOffset;
							Vector2 targetSize = size;
							float rotation2 = 0f;
							Color? color2 = new Color?(color);
							Vector2? vector = new Vector2?(Vector2.One * this.Scale);
							num = new float?(depth);
							sprite.DrawTiled(spriteBatch, position, targetSize, rotation2, null, color2, null, vector, num);
							if (fadeInBrokenSprite != null)
							{
								float d = MathHelper.Clamp(depth + (fadeInBrokenSprite.Sprite.Depth - this.activeSprite.Depth - 1E-06f), 0f, 0.999f);
								Sprite sprite2 = fadeInBrokenSprite.Sprite;
								Vector2 position2 = new Vector2(this.DrawPosition.X - (float)(this.rect.Width / 2), -(this.DrawPosition.Y + (float)(this.rect.Height / 2))) + fadeInBrokenSprite.Offset.ToVector2() * this.Scale;
								Vector2 targetSize2 = size;
								float rotation3 = 0f;
								color2 = new Color?(color * fadeInBrokenSpriteAlpha);
								vector = new Vector2?(Vector2.One * this.Scale);
								num = new float?(d);
								sprite2.DrawTiled(spriteBatch, position2, targetSize2, rotation3, null, color2, null, vector, num);
							}
							this.DrawDecorativeSprites(spriteBatch, this.DrawPosition, base.FlippedX && this.Prefab.CanSpriteFlipX, base.FlippedY && this.Prefab.CanSpriteFlipY, 0f, depth, CS$<>8__locals1.overrideColor);
						}
					}
					else
					{
						Vector2 origin = Item.<Draw>g__GetSpriteOrigin|45_0(this.activeSprite);
						if (color.A > 0)
						{
							this.activeSprite.Draw(spriteBatch, new Vector2(this.DrawPosition.X, -this.DrawPosition.Y) + drawOffset, color, origin, this.RotationRad, this.Scale, this.activeSprite.effects, new float?(depth));
							if (fadeInBrokenSprite != null)
							{
								float d2 = MathHelper.Clamp(depth + (fadeInBrokenSprite.Sprite.Depth - this.activeSprite.Depth - 1E-06f), 0f, 0.999f);
								fadeInBrokenSprite.Sprite.Draw(spriteBatch, new Vector2(this.DrawPosition.X, -this.DrawPosition.Y) + fadeInBrokenSprite.Offset.ToVector2() * this.Scale, color * fadeInBrokenSpriteAlpha, origin, this.RotationRad, this.Scale, this.activeSprite.effects, new float?(d2));
							}
						}
						if (this.Infector != null && (this.Infector.ParentBallastFlora.HasBrokenThrough || BallastFloraBehavior.AlwaysShowBallastFloraSprite))
						{
							Sprite infectedSprite = this.Prefab.InfectedSprite;
							if (infectedSprite != null)
							{
								infectedSprite.Draw(spriteBatch, new Vector2(this.DrawPosition.X, -this.DrawPosition.Y) + drawOffset, color, this.Prefab.InfectedSprite.Origin, this.RotationRad, this.Scale, this.activeSprite.effects, new float?(depth - 0.001f));
							}
							Sprite damagedInfectedSprite = this.Prefab.DamagedInfectedSprite;
							if (damagedInfectedSprite != null)
							{
								damagedInfectedSprite.Draw(spriteBatch, new Vector2(this.DrawPosition.X, -this.DrawPosition.Y) + drawOffset, this.Infector.HealthColor, this.Prefab.DamagedInfectedSprite.Origin, this.RotationRad, this.Scale, this.activeSprite.effects, new float?(depth - 0.002f));
							}
						}
						this.DrawDecorativeSprites(spriteBatch, this.DrawPosition, base.FlippedX && this.Prefab.CanSpriteFlipX, base.FlippedY && this.Prefab.CanSpriteFlipY, -this.RotationRad, depth, CS$<>8__locals1.overrideColor);
					}
				}
				else if (this.body.Enabled)
				{
					Holdable holdable = this.GetComponent<Holdable>();
					if (holdable != null)
					{
						Character picker = holdable.Picker;
						if (((picker != null) ? picker.AnimController : null) != null)
						{
							Wearable component = this.GetComponent<Wearable>();
							if (component != null && component.IsActive)
							{
								return;
							}
							if (!back)
							{
								return;
							}
							CharacterInventory inventory = holdable.Picker.Inventory;
							if (((inventory != null) ? inventory.GetItemInLimbSlot(InvSlotType.RightHand) : null) == this)
							{
								depth = Item.<Draw>g__GetHeldItemDepth|45_2(LimbType.RightHand, holdable, depth);
							}
							else
							{
								CharacterInventory inventory2 = holdable.Picker.Inventory;
								if (((inventory2 != null) ? inventory2.GetItemInLimbSlot(InvSlotType.LeftHand) : null) == this)
								{
									depth = Item.<Draw>g__GetHeldItemDepth|45_2(LimbType.LeftHand, holdable, depth);
								}
							}
						}
					}
					Vector2 origin2 = Item.<Draw>g__GetSpriteOrigin|45_0(this.activeSprite);
					this.body.Draw(spriteBatch, this.activeSprite, color, new float?(depth), this.Scale, false, false, new Vector2?(origin2));
					if (fadeInBrokenSprite != null)
					{
						float d3 = Math.Min(depth + (fadeInBrokenSprite.Sprite.Depth - this.activeSprite.Depth - 1E-06f), 0.999f);
						PhysicsBody physicsBody = this.body;
						Sprite sprite3 = fadeInBrokenSprite.Sprite;
						Color color3 = color * fadeInBrokenSpriteAlpha;
						float? depth2 = new float?(d3);
						float num2 = this.Scale;
						bool mirrorX = false;
						bool mirrorY = false;
						Vector2? vector = null;
						physicsBody.Draw(spriteBatch, sprite3, color3, depth2, num2, mirrorX, mirrorY, vector);
					}
					this.DrawDecorativeSprites(spriteBatch, this.body.DrawPosition, this.body.Dir < 0f, false, this.body.Rotation, depth, CS$<>8__locals1.overrideColor);
				}
				foreach (Upgrade upgrade in this.Upgrades)
				{
					foreach (DecorativeSprite decorativeSprite in this.GetUpgradeSprites(upgrade))
					{
						if (this.spriteAnimState[decorativeSprite].IsActive)
						{
							float rotation = decorativeSprite.GetRotation(ref this.spriteAnimState[decorativeSprite].RotationState, this.spriteAnimState[decorativeSprite].RandomRotationFactor);
							Vector2 offset = decorativeSprite.GetOffset(ref this.spriteAnimState[decorativeSprite].OffsetState, this.spriteAnimState[decorativeSprite].RandomOffsetMultiplier, -this.RotationRad) * this.Scale;
							if (base.FlippedX && this.Prefab.CanSpriteFlipX)
							{
								offset.X = -offset.X;
							}
							if (base.FlippedY && this.Prefab.CanSpriteFlipY)
							{
								offset.Y = -offset.Y;
							}
							decorativeSprite.Sprite.Draw(spriteBatch, new Vector2(this.DrawPosition.X + offset.X, -(this.DrawPosition.Y + offset.Y)), color, decorativeSprite.Sprite.Origin, rotation, decorativeSprite.GetScale(ref this.spriteAnimState[decorativeSprite].ScaleState, this.spriteAnimState[decorativeSprite].RandomScaleFactor) * this.Scale, this.activeSprite.effects, new float?(depth + (decorativeSprite.Sprite.Depth - this.activeSprite.Depth)));
						}
					}
				}
				this.activeSprite.effects = oldEffects;
				if (fadeInBrokenSprite != null && fadeInBrokenSprite.Sprite != this.activeSprite)
				{
					fadeInBrokenSprite.Sprite.effects = oldBrokenSpriteEffects;
				}
			}
			for (int j = this.drawableComponents.Count - 1; j >= 0; j--)
			{
				this.drawableComponents[j].Draw(spriteBatch, CS$<>8__locals1.editing && !GameMain.SubEditorScreen.TransformWidgetSelected, depth, CS$<>8__locals1.overrideColor);
			}
			if (GameMain.DebugDraw)
			{
				PhysicsBody physicsBody2 = this.body;
				if (physicsBody2 != null)
				{
					physicsBody2.DebugDraw(spriteBatch, Color.White, false);
				}
				TriggerComponent component2 = this.GetComponent<TriggerComponent>();
				PhysicsBody triggerBody = (component2 != null) ? component2.PhysicsBody : null;
				if (triggerBody != null)
				{
					triggerBody.UpdateDrawPosition(true);
					triggerBody.DebugDraw(spriteBatch, Color.White, false);
				}
			}
			if (CS$<>8__locals1.editing && base.IsSelected && PlayerInput.KeyDown(Keys.Space))
			{
				ElectricalDischarger discharger = this.GetComponent<ElectricalDischarger>();
				if (discharger != null)
				{
					discharger.DrawElectricity(spriteBatch);
				}
			}
			if (!CS$<>8__locals1.editing || (this.body != null && !this.body.Enabled))
			{
				return;
			}
			if (base.IsSelected || base.IsHighlighted)
			{
				Vector2 drawPos = new Vector2(this.DrawPosition.X - (float)(this.rect.Width / 2), -(this.DrawPosition.Y + (float)(this.rect.Height / 2)));
				Vector2 drawSize = new Vector2(MathF.Ceiling((float)this.rect.Width + Math.Abs(drawPos.X - (float)((int)drawPos.X))), MathF.Ceiling((float)this.rect.Height + Math.Abs(drawPos.Y - (float)((int)drawPos.Y))));
				drawPos = new Vector2(MathF.Floor(drawPos.X), MathF.Floor(drawPos.Y));
				GUI.DrawRectangle(spriteBatch, drawPos + drawSize * 0.5f, drawSize.X, drawSize.Y, this.RotationRad, Color.White, 0f, Math.Max(2f / Screen.Selected.Cam.Zoom, 1f));
				foreach (Rectangle t in this.Prefab.Triggers)
				{
					Rectangle transformedTrigger = this.TransformTrigger(t, false);
					Vector2 rectWorldPos = new Vector2((float)transformedTrigger.X, (float)transformedTrigger.Y);
					if (base.Submarine != null)
					{
						rectWorldPos += base.Submarine.Position;
					}
					rectWorldPos.Y = -rectWorldPos.Y;
					GUI.DrawRectangle(spriteBatch, rectWorldPos, new Vector2((float)transformedTrigger.Width, (float)transformedTrigger.Height), GUIStyle.Green, false, 0f, (float)((int)Math.Max(1.5f / Screen.Selected.Cam.Zoom, 1f)));
				}
			}
			if (!Item.ShowLinks || GUI.DisableHUD)
			{
				return;
			}
			foreach (MapEntity e in this.linkedTo)
			{
				bool isLinkAllowed = this.Prefab.IsLinkAllowed(e.Prefab);
				Color lineColor = GUIStyle.Red * 0.5f;
				if (isLinkAllowed)
				{
					Item k = e as Item;
					lineColor = ((k != null && (this.DisplaySideBySideWhenLinked || k.DisplaySideBySideWhenLinked)) ? (Color.Purple * 0.5f) : (Color.LightGreen * 0.5f));
				}
				Vector2 from = new Vector2(this.WorldPosition.X, -this.WorldPosition.Y);
				Vector2 to = new Vector2(e.WorldPosition.X, -e.WorldPosition.Y);
				GUI.DrawLine(spriteBatch, from, to, lineColor * 0.25f, 0f, 3f);
				GUI.DrawLine(spriteBatch, from, to, lineColor, 0f, 1f);
			}
		}

		// Token: 0x0600188D RID: 6285 RVA: 0x000F77B4 File Offset: 0x000F59B4
		public void DrawDecorativeSprites(SpriteBatch spriteBatch, Vector2 drawPos, bool flipX, bool flipY, float rotation, float depth, Color? overrideColor = null)
		{
			foreach (DecorativeSprite decorativeSprite in this.Prefab.DecorativeSprites)
			{
				Color? color = overrideColor;
				Color decorativeSpriteColor = color ?? this.GetSpriteColor(new Color?(decorativeSprite.Color), false).Multiply(this.GetSpriteColor(new Color?(this.spriteColor), false));
				if (this.spriteAnimState[decorativeSprite].IsActive)
				{
					Vector2 offset = decorativeSprite.GetOffset(ref this.spriteAnimState[decorativeSprite].OffsetState, this.spriteAnimState[decorativeSprite].RandomOffsetMultiplier, (flipX ^ flipY) ? (-rotation) : rotation) * this.Scale;
					if (base.ResizeHorizontal || base.ResizeVertical)
					{
						Sprite sprite = decorativeSprite.Sprite;
						Vector2 position = new Vector2(this.DrawPosition.X + offset.X - (float)(this.rect.Width / 2), -(this.DrawPosition.Y + offset.Y + (float)(this.rect.Height / 2)));
						Vector2 targetSize = new Vector2((float)this.rect.Width, (float)this.rect.Height);
						float rotation2 = 0f;
						color = new Color?(decorativeSpriteColor);
						Vector2? textureScale = new Vector2?(Vector2.One * this.Scale);
						float? depth2 = new float?(Math.Min(depth + (decorativeSprite.Sprite.Depth - this.activeSprite.Depth), 0.999f));
						sprite.DrawTiled(spriteBatch, position, targetSize, rotation2, null, color, null, textureScale, depth2);
					}
					else
					{
						float spriteRotation = decorativeSprite.GetRotation(ref this.spriteAnimState[decorativeSprite].RotationState, this.spriteAnimState[decorativeSprite].RandomRotationFactor);
						Vector2 origin = decorativeSprite.Sprite.Origin;
						SpriteEffects spriteEffects = SpriteEffects.None;
						if (flipX && this.Prefab.CanSpriteFlipX)
						{
							offset.X = -offset.X;
							origin.X = -origin.X + decorativeSprite.Sprite.size.X;
							spriteEffects = SpriteEffects.FlipHorizontally;
						}
						if (flipY && this.Prefab.CanSpriteFlipY)
						{
							offset.Y = -offset.Y;
							origin.Y = -origin.Y + decorativeSprite.Sprite.size.Y;
							spriteEffects |= SpriteEffects.FlipVertically;
						}
						decorativeSprite.Sprite.Draw(spriteBatch, new Vector2(drawPos.X + offset.X, -(drawPos.Y + offset.Y)), decorativeSpriteColor, origin, -rotation + spriteRotation, decorativeSprite.GetScale(ref this.spriteAnimState[decorativeSprite].ScaleState, this.spriteAnimState[decorativeSprite].RandomScaleFactor) * this.Scale, spriteEffects, new float?(depth + (decorativeSprite.Sprite.Depth - this.activeSprite.Depth)));
					}
				}
			}
		}

		// Token: 0x0600188E RID: 6286 RVA: 0x000F7AC7 File Offset: 0x000F5CC7
		public void CheckNeedsSoundUpdate(ItemComponent ic)
		{
			if (ic.NeedsSoundUpdate())
			{
				if (!this.updateableComponents.Contains(ic))
				{
					this.updateableComponents.Add(ic);
				}
				this.IsActive = true;
			}
		}

		// Token: 0x0600188F RID: 6287 RVA: 0x000F7AF4 File Offset: 0x000F5CF4
		public void UpdateSpriteStates(float deltaTime)
		{
			if (this.activeContainedSprite != null)
			{
				if (this.activeContainedSprite.DecorativeSpriteBehavior == ContainedItemSprite.DecorativeSpriteBehaviorType.HideWhenVisible)
				{
					foreach (DecorativeSprite decorativeSprite in this.Prefab.DecorativeSprites)
					{
						DecorativeSprite.State spriteState = this.spriteAnimState[decorativeSprite];
						spriteState.IsActive = false;
					}
					return;
				}
			}
			else
			{
				foreach (ContainedItemSprite containedSprite in this.Prefab.ContainedSprites)
				{
					if (containedSprite.Sprite != this.activeSprite && containedSprite.DecorativeSpriteBehavior == ContainedItemSprite.DecorativeSpriteBehaviorType.HideWhenNotVisible)
					{
						foreach (DecorativeSprite decorativeSprite2 in this.Prefab.DecorativeSprites)
						{
							DecorativeSprite.State spriteState2 = this.spriteAnimState[decorativeSprite2];
							spriteState2.IsActive = false;
						}
						return;
					}
				}
			}
			if (this.Prefab.DecorativeSpriteGroups.Count > 0)
			{
				DecorativeSprite.UpdateSpriteStates(this.Prefab.DecorativeSpriteGroups, this.spriteAnimState, (int)this.ID, deltaTime, new Func<PropertyConditional, bool>(this.ConditionalMatches));
			}
			foreach (Upgrade upgrade in this.Upgrades)
			{
				foreach (DecorativeSprite decorativeSprite3 in this.GetUpgradeSprites(upgrade))
				{
					DecorativeSprite.State spriteState3 = this.spriteAnimState[decorativeSprite3];
					spriteState3.IsActive = true;
					foreach (PropertyConditional conditional in decorativeSprite3.IsActiveConditionals)
					{
						if (!this.ConditionalMatches(conditional))
						{
							spriteState3.IsActive = false;
							break;
						}
					}
				}
			}
			foreach (Item containedItem in this.ContainedItems)
			{
				containedItem.UpdateSpriteStates(deltaTime);
			}
		}

		// Token: 0x06001890 RID: 6288 RVA: 0x000F7D30 File Offset: 0x000F5F30
		public override void UpdateEditing(Camera cam, float deltaTime)
		{
			if (MapEntity.editingHUD == null || MapEntity.editingHUD.UserData as Item != this)
			{
				MapEntity.editingHUD = this.CreateEditingHUD(Screen.Selected != GameMain.SubEditorScreen);
				this.editingHUDRefreshTimer = 1f;
			}
			if (this.editingHUDRefreshTimer <= 0f)
			{
				this.activeEditors.ForEach(delegate(SerializableEntityEditor e)
				{
					if (e != null)
					{
						e.RefreshValues();
					}
				});
				this.editingHUDRefreshTimer = 1f;
			}
			if (Screen.Selected != GameMain.SubEditorScreen)
			{
				return;
			}
			if (Character.Controlled == null)
			{
				this.activeHUDs.Clear();
			}
			if (GameMain.SubEditorScreen.TransformWidgetSelected)
			{
				return;
			}
			ElectricalDischarger discharger = this.GetComponent<ElectricalDischarger>();
			if (discharger != null)
			{
				if (PlayerInput.KeyDown(Keys.Space))
				{
					discharger.FindNodes(this.WorldPosition, discharger.Range);
				}
				else
				{
					discharger.IsActive = false;
				}
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.UpdateEditing(deltaTime);
			}
			if (!this.Linkable)
			{
				return;
			}
			if (!PlayerInput.KeyDown(Keys.Space))
			{
				return;
			}
			bool lClick = PlayerInput.PrimaryMouseButtonClicked();
			bool rClick = PlayerInput.SecondaryMouseButtonClicked();
			if (!lClick && !rClick)
			{
				return;
			}
			Vector2 position = cam.ScreenToWorld(PlayerInput.MousePosition);
			MapEntity otherEntity = MapEntity.highlightedEntities.FirstOrDefault((MapEntity e) => e != this && e.IsMouseOn(position));
			if (otherEntity != null)
			{
				if (this.linkedTo.Contains(otherEntity))
				{
					this.linkedTo.Remove(otherEntity);
					if (otherEntity.linkedTo != null && otherEntity.linkedTo.Contains(this))
					{
						otherEntity.linkedTo.Remove(this);
						return;
					}
				}
				else
				{
					this.linkedTo.Add(otherEntity);
					if (otherEntity.Linkable && otherEntity.linkedTo != null)
					{
						otherEntity.linkedTo.Add(this);
					}
				}
			}
		}

		// Token: 0x06001891 RID: 6289 RVA: 0x000F7F30 File Offset: 0x000F6130
		public override bool IsMouseOn(Vector2 position)
		{
			Vector2 rectSize = this.rect.Size.ToVector2();
			Vector2 bodyPos = this.WorldPosition;
			Vector2 transformedMousePos = MathUtils.RotatePointAroundTarget(position, bodyPos, this.RotationRad, true);
			return Math.Abs(transformedMousePos.X - bodyPos.X) < rectSize.X / 2f && Math.Abs(transformedMousePos.Y - bodyPos.Y) < rectSize.Y / 2f;
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x000F7FAC File Offset: 0x000F61AC
		public GUIComponent CreateEditingHUD(bool inGame = false)
		{
			this.activeEditors.Clear();
			int heightScaled = (int)(20f * GUI.Scale);
			MapEntity.editingHUD = new GUIFrame(new RectTransform(new Vector2(0.3f, 0.25f), GUI.Canvas, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(400, 0)
			}, "", null)
			{
				UserData = this
			};
			GUIListBox listBox = new GUIListBox(new RectTransform(new Vector2(0.95f, 0.8f), MapEntity.editingHUD.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, null, true, false)
			{
				CanTakeKeyBoardFocus = false,
				Spacing = (int)(25f * GUI.Scale)
			};
			SerializableEntityEditor itemEditor = new SerializableEntityEditor(listBox.Content.RectTransform, this, inGame, true, "", 24, GUIStyle.LargeFont, false)
			{
				UserData = this
			};
			this.activeEditors.Add(itemEditor);
			itemEditor.Children.First<GUIComponent>().Color = Color.Black * 0.7f;
			if (!inGame)
			{
				if (this.Linkable)
				{
					RectTransform rectT = new RectTransform(new Point(MapEntity.editingHUD.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, true);
					RichString text5 = TextManager.Get("HoldToLink");
					GUIFont smallFont = GUIStyle.SmallFont;
					GUITextBlock linkText = new GUITextBlock(rectT, text5, null, smallFont, Alignment.Left, false, "", null);
					RectTransform rectT2 = new RectTransform(new Point(MapEntity.editingHUD.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, true);
					RichString text2 = TextManager.Get("AllowedLinks");
					smallFont = GUIStyle.SmallFont;
					GUITextBlock itemsText = new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Left, false, "", null);
					LocalizedString allowedItems = base.AllowedLinks.None(null) ? TextManager.Get("None") : string.Join<Identifier>(", ", base.AllowedLinks);
					itemsText.Text = TextManager.AddPunctuation(':', new LocalizedString[]
					{
						itemsText.Text,
						allowedItems
					});
					itemEditor.AddCustomContent(linkText, 1);
					itemEditor.AddCustomContent(itemsText, 2);
					linkText.TextColor = GUIStyle.Orange;
					itemsText.TextColor = GUIStyle.Orange;
				}
				ItemContainer itemContainer = this.GetComponent<ItemContainer>();
				if (itemContainer != null)
				{
					GUITextBox tagBox = itemEditor.Fields["Tags".ToIdentifier()].First<GUIComponent>() as GUITextBox;
					GUITextBox tagBox2 = tagBox;
					GUIComponent guicomponent = (tagBox2 != null) ? tagBox2.Parent : null;
					GUILayoutGroup containerTagLayout = new GUILayoutGroup(new RectTransform(new Point(MapEntity.editingHUD.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, true), true, Anchor.TopLeft);
					GUILayoutGroup containerTagButtonLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.25f, 1f), containerTagLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight);
					GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.95f, 1f), containerTagButtonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("containertaguibutton"), Alignment.Center, "GUIButtonSmall", null);
					guibutton.OnClicked = delegate(GUIButton _, object _)
					{
						this.CreateContainerTagPicker(tagBox);
						return true;
					};
					guibutton.TextBlock.AutoScaleHorizontal = true;
					RectTransform rectT3 = new RectTransform(new Vector2(0.8f, 1f), containerTagLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					RichString text3 = TextManager.Get("containertaguibuttondescription");
					GUIFont smallFont = GUIStyle.SmallFont;
					GUITextBlock containerTagText = new GUITextBlock(rectT3, text3, null, smallFont, Alignment.Left, false, "", null)
					{
						TextColor = GUIStyle.Orange
					};
					LocalizedString limitedString = ToolBox.LimitString(containerTagText.Text, containerTagText.Font, itemEditor.Rect.Width - containerTagButtonLayout.Rect.Width);
					if (limitedString != containerTagText.Text)
					{
						containerTagText.ToolTip = containerTagText.Text;
						containerTagText.Text = limitedString;
					}
					itemEditor.AddCustomContent(containerTagLayout, 3);
				}
				GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Point(listBox.Content.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.02f,
					CanBeFocused = true
				};
				GUIComponent[] rotationFieldComponents;
				GUINumberInput rotationField = itemEditor.Fields.TryGetValue("Rotation".ToIdentifier(), out rotationFieldComponents) ? rotationFieldComponents.OfType<GUINumberInput>().FirstOrDefault<GUINumberInput>() : null;
				GUIButton mirrorX = new GUIButton(new RectTransform(new Vector2(0.23f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MirrorEntityX"), Alignment.Center, "GUIButtonSmall", null)
				{
					ToolTip = TextManager.Get("MirrorEntityXToolTip"),
					Enabled = this.Prefab.CanFlipX,
					OnClicked = delegate(GUIButton button, object data)
					{
						foreach (MapEntity me in MapEntity.SelectedList)
						{
							me.FlipX(false, false);
						}
						if (!MapEntity.SelectedList.Contains(this))
						{
							this.FlipX(false, false);
						}
						MapEntity.ColorFlipButton(button, this.FlippedX);
						if (rotationField != null)
						{
							rotationField.FloatValue = this.Rotation;
						}
						return true;
					}
				};
				MapEntity.ColorFlipButton(mirrorX, base.FlippedX);
				GUIButton mirrorY = new GUIButton(new RectTransform(new Vector2(0.23f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MirrorEntityY"), Alignment.Center, "GUIButtonSmall", null)
				{
					ToolTip = TextManager.Get("MirrorEntityYToolTip"),
					Enabled = this.Prefab.CanFlipY,
					OnClicked = delegate(GUIButton button, object data)
					{
						foreach (MapEntity me in MapEntity.SelectedList)
						{
							me.FlipY(false, false);
						}
						if (!MapEntity.SelectedList.Contains(this))
						{
							this.FlipY(false, false);
						}
						MapEntity.ColorFlipButton(button, this.FlippedY);
						if (rotationField != null)
						{
							rotationField.FloatValue = this.Rotation;
						}
						return true;
					}
				};
				MapEntity.ColorFlipButton(mirrorY, base.FlippedY);
				if (this.Sprite != null)
				{
					GUIButton reloadTextureButton = new GUIButton(new RectTransform(new Vector2(0.23f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ReloadSprite"), Alignment.Center, "GUIButtonSmall", null);
					GUIButton guibutton2 = reloadTextureButton;
					guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object data)
					{
						this.Sprite.ReloadXML();
						this.Sprite.ReloadTexture();
						return true;
					}));
				}
				new GUIButton(new RectTransform(new Vector2(0.23f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ResetToPrefab"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object data)
				{
					foreach (MapEntity me in MapEntity.SelectedList)
					{
						Item item = me as Item;
						if (item != null)
						{
							item.Reset();
						}
						Structure structure = me as Structure;
						if (structure != null)
						{
							structure.Reset();
						}
					}
					if (!MapEntity.SelectedList.Contains(this))
					{
						this.Reset();
					}
					this.CreateEditingHUD(false);
					return true;
				};
				buttonContainer.RectTransform.MinSize = new Point(0, buttonContainer.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
				buttonContainer.RectTransform.IsFixedSize = true;
				itemEditor.AddCustomContent(buttonContainer, itemEditor.ContentCount);
				GUITextBlock.AutoScaleAndNormalize(from b in buttonContainer.Children
				select ((GUIButton)b).TextBlock, true, false, null);
				Submarine mainSub = Submarine.MainSub;
				bool flag;
				if (mainSub == null)
				{
					flag = false;
				}
				else
				{
					SubmarineInfo info = mainSub.Info;
					flag = (((info != null) ? new SubmarineType?(info.Type) : null).GetValueOrDefault() == SubmarineType.OutpostModule);
				}
				if (flag)
				{
					GUITickBox tickBox2 = new GUITickBox(new RectTransform(new Point(listBox.Content.Rect.Width, 10), null, Anchor.TopLeft, null, ScaleBasis.Normal, false), TextManager.Get("sp.structure.removeiflinkedoutpostdoorinuse.name"), null, "")
					{
						Font = GUIStyle.SmallFont,
						Selected = base.RemoveIfLinkedOutpostDoorInUse,
						ToolTip = TextManager.Get("sp.structure.removeiflinkedoutpostdoorinuse.description"),
						OnSelected = delegate(GUITickBox tickBox)
						{
							base.RemoveIfLinkedOutpostDoorInUse = tickBox.Selected;
							return true;
						}
					};
					itemEditor.AddCustomContent(tickBox2, 1);
				}
				if (!base.Layer.IsNullOrEmpty())
				{
					GUITextBlock layerText = new GUITextBlock(new RectTransform(new Point(listBox.Content.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, false)
					{
						MinSize = new Point(0, heightScaled)
					}, TextManager.AddPunctuation(':', new LocalizedString[]
					{
						TextManager.Get("editor.layer"),
						base.Layer
					}), null, null, Alignment.Left, false, "", null);
					itemEditor.AddCustomContent(layerText, 1);
				}
			}
			using (List<ItemComponent>.Enumerator enumerator = this.components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemComponent ic = enumerator.Current;
					if (inGame)
					{
						if (!ic.AllowInGameEditing)
						{
							continue;
						}
						if (SerializableProperty.GetProperties<InGameEditable>(ic).Count == 0 && !SerializableProperty.GetProperties<ConditionallyEditable>(ic).Any((SerializableProperty p) => p.GetAttribute<ConditionallyEditable>().IsEditable(ic)))
						{
							continue;
						}
					}
					else if (ic.RequiredItems.Count == 0 && ic.DisabledRequiredItems.Count == 0 && SerializableProperty.GetProperties<Editable>(ic).Count == 0)
					{
						continue;
					}
					new GUIFrame(new RectTransform(new Vector2(1f, 0.02f), listBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
					SerializableEntityEditor componentEditor = new SerializableEntityEditor(listBox.Content.RectTransform, ic, inGame, !inGame, "", 24, GUIStyle.SubHeadingFont, false)
					{
						UserData = ic
					};
					componentEditor.Children.First<GUIComponent>().Color = Color.Black * 0.7f;
					this.activeEditors.Add(componentEditor);
					if (inGame)
					{
						ic.CreateEditingHUD(componentEditor);
						componentEditor.Recalculate();
					}
					else
					{
						List<RelatedItem> requiredItems = new List<RelatedItem>();
						foreach (KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> kvp in ic.RequiredItems)
						{
							foreach (RelatedItem relatedItem2 in kvp.Value)
							{
								requiredItems.Add(relatedItem2);
							}
						}
						if (ic.RequiredItems.None(null))
						{
							requiredItems.AddRange(ic.DisabledRequiredItems);
						}
						using (List<RelatedItem>.Enumerator enumerator4 = requiredItems.GetEnumerator())
						{
							while (enumerator4.MoveNext())
							{
								RelatedItem relatedItem = enumerator4.Current;
								RectTransform rectT4 = new RectTransform(new Point(listBox.Content.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, false);
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
								defaultInterpolatedStringHandler.AppendFormatted<RelatedItem.RelationType>(relatedItem.Type);
								defaultInterpolatedStringHandler.AppendLiteral(".required");
								LocalizedString localizedString = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(9, 1);
								defaultInterpolatedStringHandler2.AppendFormatted<RelatedItem.RelationType>(relatedItem.Type);
								defaultInterpolatedStringHandler2.AppendLiteral(" required");
								RichString text4 = localizedString.Fallback(defaultInterpolatedStringHandler2.ToStringAndClear(), true);
								GUIFont smallFont = GUIStyle.SmallFont;
								GUITextBlock textBlock = new GUITextBlock(rectT4, text4, null, smallFont, Alignment.Left, false, "", null)
								{
									Padding = new Vector4(10f, 0f, 10f, 0f)
								};
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(17, 1);
								defaultInterpolatedStringHandler3.AppendFormatted<RelatedItem.RelationType>(relatedItem.Type);
								defaultInterpolatedStringHandler3.AppendLiteral(".required.tooltip");
								LocalizedString tooltip = TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()).Fallback(LocalizedString.EmptyString, true);
								if (!tooltip.IsNullOrWhiteSpace())
								{
									textBlock.ToolTip = tooltip;
								}
								textBlock.RectTransform.IsFixedSize = true;
								componentEditor.AddCustomContent(textBlock, 1);
								GUITextBox namesBox = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
								{
									Font = GUIStyle.SmallFont,
									Text = relatedItem.JoinedIdentifiers,
									OverflowClip = true
								};
								textBlock.RectTransform.Resize(new Point(textBlock.Rect.Width, namesBox.RectTransform.MinSize.Y), true);
								namesBox.OnDeselected += delegate(GUITextBox textBox, Keys key)
								{
									relatedItem.JoinedIdentifiers = textBox.Text;
									textBox.Text = relatedItem.JoinedIdentifiers;
								};
								GUITextBox guitextBox = namesBox;
								guitextBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(guitextBox.OnEnterPressed, new GUITextBox.OnEnterHandler(delegate(GUITextBox textBox, string text)
								{
									relatedItem.JoinedIdentifiers = text;
									textBox.Text = relatedItem.JoinedIdentifiers;
									return true;
								}));
							}
						}
						ic.CreateEditingHUD(componentEditor);
						componentEditor.Recalculate();
					}
				}
			}
			MapEntity.PositionEditingHUD();
			this.SetHUDLayout(false);
			return MapEntity.editingHUD;
		}

		// Token: 0x06001893 RID: 6291 RVA: 0x000F8E94 File Offset: 0x000F7094
		private ImmutableArray<DecorativeSprite> GetUpgradeSprites(Upgrade upgrade)
		{
			ImmutableArray<DecorativeSprite> upgradeSprites = upgrade.Prefab.DecorativeSprites;
			if (this.Prefab.UpgradeOverrideSprites.ContainsKey(upgrade.Prefab.Identifier))
			{
				upgradeSprites = this.Prefab.UpgradeOverrideSprites[upgrade.Prefab.Identifier];
			}
			return upgradeSprites;
		}

		// Token: 0x06001894 RID: 6292 RVA: 0x000F8EE8 File Offset: 0x000F70E8
		public override bool AddUpgrade(Upgrade upgrade, bool createNetworkEvent = false)
		{
			if (upgrade.Prefab.IsWallUpgrade)
			{
				return false;
			}
			bool result = base.AddUpgrade(upgrade, createNetworkEvent);
			if (result && !upgrade.Disposed)
			{
				ImmutableArray<DecorativeSprite> upgradeSprites = this.GetUpgradeSprites(upgrade);
				if (upgradeSprites.Any<DecorativeSprite>())
				{
					foreach (DecorativeSprite decorativeSprite in upgradeSprites)
					{
						decorativeSprite.Sprite.EnsureLazyLoaded(false);
						this.spriteAnimState.Add(decorativeSprite, new DecorativeSprite.State());
					}
					this.UpdateSpriteStates(0f);
				}
			}
			return result;
		}

		// Token: 0x06001895 RID: 6293 RVA: 0x000F8F6C File Offset: 0x000F716C
		public void CreateContainerTagPicker([MaybeNull] GUITextBox tagTextBox)
		{
			Item.<>c__DisplayClass54_0 CS$<>8__locals1 = new Item.<>c__DisplayClass54_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.tagTextBox = tagTextBox;
			GUIMessageBox msgBox = new GUIMessageBox(string.Empty, string.Empty, new LocalizedString[]
			{
				TextManager.Get("Ok")
			}, new Vector2?(new Vector2(0.35f, 0.6f)), new Point?(new Point(400, 400)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
			GUIImage guiimage = new GUIImage(new RectTransform(new Vector2(0.066f), msgBox.InnerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.015f)
			}, "GUIButtonInfo", GUIImage.ScalingMode.None);
			guiimage.ToolTip = TextManager.Get("containertagui.tutorial");
			guiimage.IgnoreLayoutGroups = true;
			CS$<>8__locals1.layout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.85f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			CS$<>8__locals1.list = new GUIListBox(new RectTransform(new Vector2(1f, 1f), CS$<>8__locals1.layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			GUILayoutGroup headerLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.075f), CS$<>8__locals1.list.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tagheader.tag"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			guibutton.ForceUpperCase = ForceUpperCase.Yes;
			guibutton.CanBeFocused = false;
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tagheader.items"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			guibutton2.ForceUpperCase = ForceUpperCase.Yes;
			guibutton2.CanBeFocused = false;
			GUIButton guibutton3 = new GUIButton(new RectTransform(new Vector2(0.1f, 1f), headerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tagheader.count"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			guibutton3.ForceUpperCase = ForceUpperCase.Yes;
			guibutton3.CanBeFocused = false;
			ImmutableDictionary<ContainerTagPrefab, ImmutableArray<ContainerTagPrefab.ItemAndProbability>> itemsByTag = ContainerTagPrefab.Prefabs.ToImmutableDictionary((ContainerTagPrefab ct) => ct, (ContainerTagPrefab ct) => ct.GetItemsAndSpawnProbabilities());
			ImmutableDictionary<Identifier, ImmutableArray<Identifier>> tagCategories = (from ct in ContainerTagPrefab.Prefabs
			group ct by ct.Category).ToImmutableDictionary((IGrouping<Identifier, ContainerTagPrefab> g) => g.Key, (IGrouping<Identifier, ContainerTagPrefab> g) => (from ct in g
			select ct.Identifier).ToImmutableArray<Identifier>());
			using (ImmutableDictionary<Identifier, ImmutableArray<Identifier>>.Enumerator enumerator = tagCategories.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item.<>c__DisplayClass54_1 CS$<>8__locals2 = new Item.<>c__DisplayClass54_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					KeyValuePair<Identifier, ImmutableArray<Identifier>> keyValuePair = enumerator.Current;
					Identifier category;
					ImmutableArray<Identifier> immutableArray;
					keyValuePair.Deconstruct(out category, out immutableArray);
					CS$<>8__locals2.category = category;
					ImmutableArray<Identifier> categoryTags = immutableArray;
					GUIButton categoryButton = new GUIButton(new RectTransform(new Vector2(1f, 0.075f), CS$<>8__locals2.CS$<>8__locals1.list.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "GUIButtonSmallFreeScale", null);
					categoryButton.Color *= 0.66f;
					GUILayoutGroup categoryLayout = new GUILayoutGroup(new RectTransform(Vector2.One, categoryButton.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
					{
						Stretch = true
					};
					RectTransform rectT = new RectTransform(Vector2.One, categoryLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("tagcategory.");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals2.category);
					RichString text = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
					GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
					GUITextBlock categoryText = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
					CS$<>8__locals2.arrowImage = new GUIImage(new RectTransform(new Vector2(1f, 0.5f), categoryLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "GUIButtonVerticalArrowFreeScale", GUIImage.ScalingMode.None);
					GUIFrame arrowPadding = new GUIFrame(new RectTransform(new Vector2(0.025f, 1f), categoryLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
					bool hasHiddenCategories = false;
					using (IEnumerator<Identifier> enumerator2 = (from t in categoryTags
					orderby t.Value
					select t).GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Item.<>c__DisplayClass54_2 CS$<>8__locals3 = new Item.<>c__DisplayClass54_2();
							CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals2;
							CS$<>8__locals3.categoryTag = enumerator2.Current;
							Item.<>c__DisplayClass54_3 CS$<>8__locals4 = new Item.<>c__DisplayClass54_3();
							CS$<>8__locals4.CS$<>8__locals3 = CS$<>8__locals3;
							KeyValuePair<ContainerTagPrefab, ImmutableArray<ContainerTagPrefab.ItemAndProbability>>? found = itemsByTag.FirstOrNull((KeyValuePair<ContainerTagPrefab, ImmutableArray<ContainerTagPrefab.ItemAndProbability>> kvp) => kvp.Key.Identifier == CS$<>8__locals4.CS$<>8__locals3.categoryTag);
							if (found == null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(49, 1);
								defaultInterpolatedStringHandler2.AppendLiteral("Failed to find tag with identifier ");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(CS$<>8__locals4.CS$<>8__locals3.categoryTag);
								defaultInterpolatedStringHandler2.AppendLiteral(" in itemsByTag");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
							}
							else
							{
								ContainerTagPrefab tag;
								ImmutableArray<ContainerTagPrefab.ItemAndProbability> prefabsAndProbabilities;
								found.Value.Deconstruct(out tag, out prefabsAndProbabilities);
								CS$<>8__locals4.tag = tag;
								CS$<>8__locals4.prefabsAndProbabilities = prefabsAndProbabilities;
								CS$<>8__locals4.isCorrectSubType = CS$<>8__locals4.tag.IsRecommendedForSub(base.Submarine);
								GUILayoutGroup tagLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.list.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
								{
									UserData = CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.category,
									Visible = CS$<>8__locals4.isCorrectSubType
								};
								if (!CS$<>8__locals4.isCorrectSubType)
								{
									hasHiddenCategories = true;
								}
								GUILayoutGroup checkBoxLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.4f, 1f), tagLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
								GUITickBox enabledCheckBox = new GUITickBox(new RectTransform(Vector2.One, checkBoxLayout.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), CS$<>8__locals4.tag.Name, GUIStyle.SmallFont, "")
								{
									Selected = this.tags.Contains(CS$<>8__locals4.tag.Identifier),
									ToolTip = CS$<>8__locals4.tag.Description
								};
								GUITextBlock tickBoxText = enabledCheckBox.TextBlock;
								tickBoxText.Text = ToolBox.LimitString(tickBoxText.Text, tickBoxText.Font, tickBoxText.Rect.Width);
								GUILayoutGroup itemLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), tagLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
								CS$<>8__locals4.itemLayoutScissor = new GUIScissorComponent(new RectTransform(new Vector2(0.8f, 1f), itemLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal))
								{
									CanBeFocused = false
								};
								GUILayoutGroup itemLayoutButtonLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.2f, 1f), itemLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.Center);
								GUIButton itemLayoutButton = new GUIButton(new RectTransform(new Vector2(0.8f), itemLayoutButtonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "...", Alignment.Center, "GUICharacterInfoButton", null)
								{
									UserData = CS$<>8__locals4.tag,
									ToolTip = TextManager.Get("containertagui.viewprobabilities")
								};
								itemLayoutButtonLayout.Recalculate();
								CS$<>8__locals4.scroll = 0f;
								CS$<>8__locals4.localScroll = 0f;
								CS$<>8__locals4.lastSkippedItems = 0;
								CS$<>8__locals4.skippedItems = 0;
								GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(new Vector2(1f, 0.9f), CS$<>8__locals4.itemLayoutScissor.Content.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch spriteBatch, GUICustomComponent component)
								{
									component.ToolTip = string.Empty;
									float offset = 0f;
									float size = (float)component.Rect.Height;
									int start = (int)Math.Floor((double)CS$<>8__locals4.scroll);
									int amountToDraw = (int)Math.Ceiling((double)((float)component.Rect.Width / size)) + 1;
									bool shouldIncrementOnSkip = true;
									float toDrawWidth = (float)CS$<>8__locals4.prefabsAndProbabilities.Length * (size + 8f);
									if (toDrawWidth < (float)component.Rect.Width)
									{
										shouldIncrementOnSkip = false;
										amountToDraw = CS$<>8__locals4.prefabsAndProbabilities.Length;
									}
									for (int i = start; i < start + amountToDraw; i++)
									{
										ItemPrefab itemPrefab;
										float num;
										float num2;
										CS$<>8__locals4.prefabsAndProbabilities[i % CS$<>8__locals4.prefabsAndProbabilities.Length].Deconstruct(out itemPrefab, out num, out num2);
										ItemPrefab ip = itemPrefab;
										float probability = num;
										Sprite sprite = ip.InventoryIcon ?? ip.Sprite;
										if (sprite == null)
										{
											if (shouldIncrementOnSkip)
											{
												amountToDraw++;
												int skippedItems = CS$<>8__locals4.skippedItems;
												CS$<>8__locals4.skippedItems = skippedItems + 1;
											}
										}
										else if (Item.ShouldHideItemPrefab(ip, probability))
										{
											if (shouldIncrementOnSkip)
											{
												int skippedItems = CS$<>8__locals4.skippedItems;
												CS$<>8__locals4.skippedItems = skippedItems + 1;
												amountToDraw++;
											}
										}
										else
										{
											float partialScroll = CS$<>8__locals4.localScroll * (size + 8f);
											RectangleF drawRect = new RectangleF((float)CS$<>8__locals4.itemLayoutScissor.Rect.X + offset - partialScroll, (float)component.Rect.Y, size, size);
											bool isMouseOver = drawRect.Contains(PlayerInput.MousePosition);
											if (isMouseOver)
											{
												component.ToolTip = ip.CreateTooltipText();
											}
											Sprite slotSprite = Inventory.SlotSpriteSmall;
											if (slotSprite != null)
											{
												slotSprite.Draw(spriteBatch, drawRect.Location, Color.White, Vector2.Zero, 0f, size / slotSprite.size.X * 0.575f, SpriteEffects.None, null);
											}
											float iconScale = Math.Min(drawRect.Width / sprite.size.X, drawRect.Height / sprite.size.Y) * 0.9f;
											Color drawColor = ip.InventoryIconColor;
											sprite.Draw(spriteBatch, drawRect.Center, drawColor, sprite.Origin, 0f, iconScale, SpriteEffects.None, null);
											offset += size + 8f;
										}
									}
									if (CS$<>8__locals4.skippedItems < CS$<>8__locals4.lastSkippedItems)
									{
										CS$<>8__locals4.scroll += (float)(CS$<>8__locals4.lastSkippedItems - CS$<>8__locals4.skippedItems);
									}
									CS$<>8__locals4.lastSkippedItems = CS$<>8__locals4.skippedItems;
									CS$<>8__locals4.skippedItems = 0;
								}, delegate(float deltaTime, GUICustomComponent component)
								{
									if (GUI.MouseOn != component && MathUtils.NearlyEqual(CS$<>8__locals4.localScroll, 0f, deltaTime * 2f))
									{
										CS$<>8__locals4.localScroll = 0f;
										return;
									}
									float totalWidth = (float)CS$<>8__locals4.prefabsAndProbabilities.Length * ((float)component.Rect.Height + 8f);
									if (totalWidth < (float)component.Rect.Width)
									{
										return;
									}
									CS$<>8__locals4.scroll += deltaTime;
									CS$<>8__locals4.localScroll = CS$<>8__locals4.scroll % 1f;
								});
								guicustomComponent.HoverCursor = CursorState.Default;
								guicustomComponent.AlwaysOverrideCursor = true;
								CS$<>8__locals4.tooltip = TextManager.Get(CS$<>8__locals4.tag.WarnIfLess ? "ContainerTagUI.RecommendedAmount" : "ContainerTagUI.SuggestedAmount");
								CS$<>8__locals4.countBlock = new GUITextBlock(new RectTransform(new Vector2(0.1f, 1f), tagLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Center, false, "", null)
								{
									ToolTip = CS$<>8__locals4.tooltip
								};
								CS$<>8__locals4.<CreateContainerTagPicker>g__UpdateCountBlock|10(CS$<>8__locals4.countBlock, CS$<>8__locals4.tag);
								GUITickBox guitickBox = enabledCheckBox;
								guitickBox.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(guitickBox.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox tickBox)
								{
									if (tickBox.Selected)
									{
										CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>4__this.AddTag(CS$<>8__locals4.tag.Identifier);
									}
									else
									{
										CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>4__this.RemoveTag(CS$<>8__locals4.tag.Identifier);
									}
									if (CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.tagTextBox != null)
									{
										GUITextBox tagTextBox2 = CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.tagTextBox;
										char separator = ',';
										IEnumerable<Identifier> source = CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>4__this.tags;
										Func<Identifier, bool> predicate;
										if ((predicate = CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>9__14) == null)
										{
											predicate = (CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>9__14 = ((Identifier t) => !CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.<>4__this.Prefab.Tags.Contains(t)));
										}
										tagTextBox2.Text = string.Join<Identifier>(separator, source.Where(predicate));
									}
									base.<CreateContainerTagPicker>g__UpdateCountBlock|10(CS$<>8__locals4.countBlock, CS$<>8__locals4.tag);
									return true;
								}));
								itemLayoutButton.OnClicked = delegate(GUIButton button, object _)
								{
									Item.CreateContainerTagItemListPopup(CS$<>8__locals4.tag, button.Rect.Center, CS$<>8__locals4.CS$<>8__locals3.CS$<>8__locals2.CS$<>8__locals1.layout, CS$<>8__locals4.prefabsAndProbabilities);
									return true;
								};
							}
						}
					}
					CS$<>8__locals2.arrowImage.SpriteEffects = (hasHiddenCategories ? SpriteEffects.None : SpriteEffects.FlipVertically);
					categoryButton.OnClicked = delegate(GUIButton _, object _)
					{
						CS$<>8__locals2.arrowImage.SpriteEffects ^= SpriteEffects.FlipVertically;
						foreach (GUIComponent child in CS$<>8__locals2.CS$<>8__locals1.list.Content.Children)
						{
							object userData = child.UserData;
							if (userData is Identifier)
							{
								Identifier id = (Identifier)userData;
								if (id == CS$<>8__locals2.category)
								{
									child.Visible = !child.Visible;
								}
							}
						}
						return true;
					};
				}
			}
		}

		// Token: 0x06001896 RID: 6294 RVA: 0x000F9BA4 File Offset: 0x000F7DA4
		private static void CreateContainerTagItemListPopup(ContainerTagPrefab tag, Point location, GUIComponent popupParent, ImmutableArray<ContainerTagPrefab.ItemAndProbability> prefabAndProbabilities)
		{
			GUIComponent existingTooltip = popupParent.GetChildByUserData("tooltip");
			if (existingTooltip != null)
			{
				popupParent.RemoveChild(existingTooltip);
			}
			GUIFrame tooltip = new GUIFrame(new RectTransform(new Point(popupParent.Rect.Height), popupParent.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = location - popupParent.Rect.Location
			}, "", null)
			{
				UserData = "tooltip",
				IgnoreLayoutGroups = true
			};
			if (tooltip.Rect.Bottom > GameMain.GraphicsHeight)
			{
				int diffY = tooltip.Rect.Bottom - GameMain.GraphicsHeight;
				tooltip.RectTransform.AbsoluteOffset -= new Point(0, diffY);
			}
			if (tooltip.Rect.Right > GameMain.GraphicsWidth)
			{
				int diffX = tooltip.Rect.Right - GameMain.GraphicsWidth;
				tooltip.RectTransform.AbsoluteOffset -= new Point(diffX, 0);
			}
			GUILayoutGroup tooltipLayout = new GUILayoutGroup(new RectTransform(ToolBox.PaddingSizeParentRelative(tooltip.RectTransform, 0.9f), tooltip.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.1f), tooltipLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = tag.Name;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock tooltipHeader = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Center, false, "", null);
			GUIListBox tooltipList = new GUIListBox(new RectTransform(new Vector2(1f, 0.7f), tooltipLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			GUILayoutGroup tooltipHeaderLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), tooltipList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.66f, 1f), tooltipHeaderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tagheader.item"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			guibutton.ForceUpperCase = ForceUpperCase.Yes;
			guibutton.CanBeFocused = false;
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.33f, 1f), tooltipHeaderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("tagheader.probability"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			guibutton2.ForceUpperCase = ForceUpperCase.Yes;
			guibutton2.CanBeFocused = false;
			foreach (ContainerTagPrefab.ItemAndProbability itemAndProbability in from p in prefabAndProbabilities
			orderby p.Probability descending
			select p)
			{
				ContainerTagPrefab.ItemAndProbability itemAndProbability2 = itemAndProbability;
				ItemPrefab itemPrefab;
				float num;
				float num2;
				itemAndProbability2.Deconstruct(out itemPrefab, out num, out num2);
				ItemPrefab ip = itemPrefab;
				float probability = num;
				float campaignOnlyProbability = num2;
				if (!Item.ShouldHideItemPrefab(ip, probability))
				{
					GUILayoutGroup itemLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), tooltipList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
					{
						UserData = itemAndProbability
					};
					GUILayoutGroup itemNameLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.66f, 1f), itemLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
					{
						Stretch = true
					};
					new GUIImage(new RectTransform(Vector2.One, itemNameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), ip.InventoryIcon ?? ip.Sprite, true, null).Color = ip.InventoryIconColor;
					GUITextBlock itemName = new GUITextBlock(new RectTransform(Vector2.One, itemNameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), ip.Name, null, null, Alignment.Left, false, "", null);
					itemName.Text = ToolBox.LimitString(ip.Name, itemName.Font, itemName.Rect.Width);
					GUIFrame guiframe = new GUIFrame(new RectTransform(Vector2.One, itemNameLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
					guiframe.IgnoreLayoutGroups = true;
					guiframe.ToolTip = ip.CreateTooltipText();
					GUITextBlock probabilityText = new GUITextBlock(new RectTransform(new Vector2(0.33f, 1f), itemLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Item.<CreateContainerTagItemListPopup>g__ProbabilityToPercentage|55_0(campaignOnlyProbability), null, null, Alignment.Right, false, "", null)
					{
						UserData = "probability"
					};
					if (MathUtils.NearlyEqual(campaignOnlyProbability, 0f, 0.0001f))
					{
						probabilityText.TextColor = GUIStyle.Red;
					}
				}
			}
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), tooltipLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("containertagui.campaignonly"), null, "");
			guitickBox.ToolTip = TextManager.Get("containertagui.campaignonlytooltip");
			guitickBox.Selected = true;
			guitickBox.OnSelected = delegate(GUITickBox box)
			{
				foreach (GUIComponent child in tooltipList.Content.Children)
				{
					object userData = child.UserData;
					if (userData is ContainerTagPrefab.ItemAndProbability)
					{
						ContainerTagPrefab.ItemAndProbability data = (ContainerTagPrefab.ItemAndProbability)userData;
						GUITextBlock text2 = child.GetChildByUserData("probability") as GUITextBlock;
						if (text2 != null)
						{
							float probability2 = box.Selected ? data.CampaignProbability : data.Probability;
							text2.Text = Item.<CreateContainerTagItemListPopup>g__ProbabilityToPercentage|55_0(probability2);
							text2.TextColor = (MathUtils.NearlyEqual(probability2, 0f, 0.0001f) ? GUIStyle.Red : GUIStyle.TextColorNormal);
						}
					}
				}
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(1f, 0.1f), tooltipLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Close"), Alignment.Center, "", null).OnClicked = delegate(GUIButton _, object _)
			{
				popupParent.RemoveChild(tooltip);
				return true;
			};
		}

		// Token: 0x06001897 RID: 6295 RVA: 0x000FA350 File Offset: 0x000F8550
		private static bool ShouldHideItemPrefab(ItemPrefab ip, float probability)
		{
			return ip.HideInMenus && MathUtils.NearlyEqual(probability, 0f, 0.0001f);
		}

		// Token: 0x06001898 RID: 6296 RVA: 0x000FA36C File Offset: 0x000F856C
		private void SetHUDLayout(bool ignoreLocking = false)
		{
			List<GUIComponent> elementsToMove = new List<GUIComponent>();
			if (MapEntity.editingHUD != null && MapEntity.editingHUD.UserData == this)
			{
				if (this.HasInGameEditableProperties)
				{
					Character controlled = Character.Controlled;
					if (((controlled != null) ? controlled.SelectedItem : null) == this)
					{
						goto IL_42;
					}
				}
				if (Screen.Selected != GameMain.SubEditorScreen)
				{
					goto IL_4D;
				}
				IL_42:
				elementsToMove.Add(MapEntity.editingHUD);
			}
			IL_4D:
			this.debugInitialHudPositions.Clear();
			foreach (ItemComponent ic in this.activeHUDs)
			{
				if (ic.GuiFrame != null && ic.GetLinkUIToComponent() == null)
				{
					bool nearlyCoversScreen = (float)ic.GuiFrame.Rect.Width >= (float)GameMain.GraphicsWidth * 0.9f && (float)ic.GuiFrame.Rect.Height >= (float)GameMain.GraphicsHeight * 0.9f;
					if (ic.AllowUIOverlap || (!ignoreLocking && ic.LockGuiFramePosition) || nearlyCoversScreen)
					{
						ic.GuiFrame.ClampToArea(new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight));
					}
					else
					{
						ic.GuiFrame.RectTransform.ScreenSpaceOffset = ic.GuiFrameOffset;
						elementsToMove.Add(ic.GuiFrame);
						this.debugInitialHudPositions.Add(ic.GuiFrame.Rect);
					}
				}
			}
			List<Rectangle> disallowedAreas = new List<Rectangle>();
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.CrewManager : null) != null && Screen.Selected == GameMain.GameScreen)
			{
				int disallowedPadding = (int)(50f * GUI.Scale);
				disallowedAreas.Add(GameMain.GameSession.CrewManager.GetActiveCrewArea());
				disallowedAreas.Add(new Rectangle(HUDLayoutSettings.ChatBoxArea.X - disallowedPadding, HUDLayoutSettings.ChatBoxArea.Y, HUDLayoutSettings.ChatBoxArea.Width + disallowedPadding, HUDLayoutSettings.ChatBoxArea.Height));
			}
			SubEditorScreen editor = Screen.Selected as SubEditorScreen;
			if (editor != null)
			{
				disallowedAreas.Add(editor.EntityMenu.Rect);
				disallowedAreas.Add(editor.TopPanel.Rect);
				disallowedAreas.Add(editor.ToggleEntityMenuButton.Rect);
			}
			GUI.PreventElementOverlap(elementsToMove, disallowedAreas, new Rectangle?(HUDLayoutSettings.ItemHUDArea));
			foreach (ItemComponent ic2 in this.activeHUDs)
			{
				if (ic2.GuiFrame != null)
				{
					ItemComponent linkUIToComponent = ic2.GetLinkUIToComponent();
					if (linkUIToComponent != null)
					{
						ic2.GuiFrame.RectTransform.ScreenSpaceOffset = linkUIToComponent.GuiFrame.RectTransform.ScreenSpaceOffset;
					}
				}
			}
		}

		// Token: 0x06001899 RID: 6297 RVA: 0x000FA638 File Offset: 0x000F8838
		public void UpdateHUD(Camera cam, Character character, float deltaTime)
		{
			Item.<>c__DisplayClass62_0 CS$<>8__locals1;
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			bool editingHUDCreated = false;
			if ((this.HasInGameEditableProperties && (CS$<>8__locals1.character.SelectedItem == this || this.EditableWhenEquipped)) || Screen.Selected == GameMain.SubEditorScreen)
			{
				GUIComponent prevEditingHUD = MapEntity.editingHUD;
				this.UpdateEditing(cam, deltaTime);
				editingHUDCreated = (MapEntity.editingHUD != null && MapEntity.editingHUD != prevEditingHUD);
			}
			if (MapEntity.editingHUD != null)
			{
				GUITextBox textBox = GUI.KeyboardDispatcher.Subscriber as GUITextBox;
				if (textBox != null && MapEntity.editingHUD.IsParentOf(textBox, true))
				{
					goto IL_98;
				}
			}
			this.editingHUDRefreshTimer -= deltaTime;
			IL_98:
			this.prevActiveHUDs.Clear();
			this.prevActiveHUDs.AddRange(this.activeHUDs);
			this.activeComponents.Clear();
			this.activeComponents.AddRange(this.components);
			Controller controller = this.GetComponent<Controller>();
			if (controller == null || controller.User != Character.Controlled || !controller.HideAllItemComponentHUDs)
			{
				foreach (MapEntity entity in this.linkedTo)
				{
					if (this.Prefab.IsLinkAllowed(entity.Prefab))
					{
						Item i = entity as Item;
						if (i != null && i.DisplaySideBySideWhenLinked)
						{
							this.activeComponents.AddRange(i.components);
						}
					}
				}
			}
			this.activeHUDs.Clear();
			this.maxPriorityHUDs.Clear();
			foreach (ItemComponent ic in this.activeComponents)
			{
				if (ic.HudPriority > 0 && this.<UpdateHUD>g__DrawHud|62_0(ic, ref CS$<>8__locals1) && (this.maxPriorityHUDs.Count == 0 || ic.HudPriority >= this.maxPriorityHUDs[0].HudPriority))
				{
					if (this.maxPriorityHUDs.Count > 0 && ic.HudPriority > this.maxPriorityHUDs[0].HudPriority)
					{
						this.maxPriorityHUDs.Clear();
					}
					this.maxPriorityHUDs.Add(ic);
				}
			}
			if (this.maxPriorityHUDs.Count > 0)
			{
				this.activeHUDs.AddRange(this.maxPriorityHUDs);
			}
			else
			{
				foreach (ItemComponent ic2 in this.activeComponents)
				{
					if (this.<UpdateHUD>g__DrawHud|62_0(ic2, ref CS$<>8__locals1))
					{
						this.activeHUDs.Add(ic2);
					}
				}
			}
			this.activeHUDs.Sort((ItemComponent h1, ItemComponent h2) => h2.HudLayer.CompareTo(h1.HudLayer));
			if (!this.prevActiveHUDs.SequenceEqual(this.activeHUDs) || editingHUDCreated)
			{
				this.SetHUDLayout(false);
			}
			Rectangle mergedHUDRect = Rectangle.Empty;
			foreach (ItemComponent ic3 in this.activeHUDs)
			{
				ic3.UpdateHUD(CS$<>8__locals1.character, deltaTime, cam);
				if (ic3.GuiFrame != null && ic3.GuiFrame.Rect.Height < GameMain.GraphicsHeight)
				{
					mergedHUDRect = ((mergedHUDRect == Rectangle.Empty) ? ic3.GuiFrame.Rect : Rectangle.Union(mergedHUDRect, ic3.GuiFrame.Rect));
				}
			}
			if (mergedHUDRect != Rectangle.Empty)
			{
				if (this.itemInUseWarning != null)
				{
					this.itemInUseWarning.Visible = false;
				}
				foreach (Character otherCharacter in Character.CharacterList)
				{
					if (otherCharacter != CS$<>8__locals1.character && otherCharacter.SelectedItem == this && !otherCharacter.IsAttachedToController())
					{
						this.ItemInUseWarning.Visible = true;
						if (mergedHUDRect.Width > GameMain.GraphicsWidth / 2)
						{
							mergedHUDRect.Inflate(-GameMain.GraphicsWidth / 4, 0);
						}
						this.itemInUseWarning.RectTransform.ScreenSpaceOffset = new Point(mergedHUDRect.X, mergedHUDRect.Bottom);
						this.itemInUseWarning.RectTransform.NonScaledSize = new Point(mergedHUDRect.Width, (int)(50f * GUI.Scale));
						if (this.itemInUseWarning.UserData != otherCharacter)
						{
							this.itemInUseWarning.Text = TextManager.GetWithVariable("ItemInUse", "[character]", otherCharacter.Name, FormatCapitals.No);
							this.itemInUseWarning.UserData = otherCharacter;
							break;
						}
						break;
					}
				}
			}
		}

		// Token: 0x0600189A RID: 6298 RVA: 0x000FAB78 File Offset: 0x000F8D78
		public void DrawHUD(SpriteBatch spriteBatch, Camera cam, Character character)
		{
			if (this.HasInGameEditableProperties && (character.SelectedItem == this || this.EditableWhenEquipped))
			{
				this.DrawEditing(spriteBatch, cam);
			}
			foreach (ItemComponent ic in this.activeHUDs)
			{
				if (ic.CanBeSelected)
				{
					ic.DrawHUD(spriteBatch, character);
				}
			}
			if (GameMain.DebugDraw)
			{
				int i = 0;
				foreach (ItemComponent ic2 in this.activeHUDs)
				{
					if (i >= this.debugInitialHudPositions.Count)
					{
						break;
					}
					if (this.activeHUDs[i].GuiFrame != null && ic2.GuiFrame != null && !ic2.AllowUIOverlap && ic2.GetLinkUIToComponent() == null)
					{
						GUI.DrawRectangle(spriteBatch, this.debugInitialHudPositions[i], Color.Orange, false, 0f, 1f);
						GUI.DrawRectangle(spriteBatch, ic2.GuiFrame.Rect, Color.LightGreen, false, 0f, 1f);
						GUI.DrawLine(spriteBatch, this.debugInitialHudPositions[i].Location.ToVector2(), ic2.GuiFrame.Rect.Location.ToVector2(), Color.Orange, 0f, 1f);
						i++;
					}
				}
			}
		}

		// Token: 0x0600189B RID: 6299 RVA: 0x000FAD44 File Offset: 0x000F8F44
		public void ClearActiveHUDs()
		{
			this.activeHUDs.Clear();
		}

		// Token: 0x0600189C RID: 6300 RVA: 0x000FAD54 File Offset: 0x000F8F54
		public List<ColoredText> GetHUDTexts(Character character, bool recreateHudTexts = true)
		{
			if (this.texts.Any<ColoredText>() && !recreateHudTexts)
			{
				return this.texts;
			}
			this.texts.Clear();
			string nameText = RichString.Rich(this.Prefab.Name, null).SanitizedValue;
			if (this.Prefab.Tags.Contains("identitycard") || this.Tags.Contains("despawncontainer"))
			{
				string[] readTags = this.Tags.Split(',', StringSplitOptions.None);
				string idName = null;
				foreach (string tag in readTags)
				{
					string[] s = tag.Split(':', StringSplitOptions.None);
					if (s[0] == "name")
					{
						idName = s[1];
						break;
					}
				}
				if (idName != null)
				{
					nameText = nameText + " (" + idName + ")";
				}
			}
			if (this.DroppedStack.Any<Item>())
			{
				string str = nameText;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendLiteral(" x");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.DroppedStack.Count<Item>());
				nameText = str + defaultInterpolatedStringHandler.ToStringAndClear();
			}
			this.texts.Add(new ColoredText(nameText, GUIStyle.TextColorNormal, false, false));
			if (CampaignMode.BlocksInteraction(this.CampaignInteractionType))
			{
				List<ColoredText> list = this.texts;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(20, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("CampaignInteraction.");
				defaultInterpolatedStringHandler2.AppendFormatted<CampaignMode.InteractionType>(this.CampaignInteractionType);
				string tag2 = defaultInterpolatedStringHandler2.ToStringAndClear();
				string varName = "[key]";
				GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
				list.Add(new ColoredText(TextManager.GetWithVariable(tag2, varName, keyMap.KeyBindText(InputType.Use), FormatCapitals.No).Value, Color.Cyan, false, false));
			}
			else
			{
				foreach (ItemComponent itemComponent in this.components)
				{
					Item.InteractionVisibility interactionVisibility = Item.GetComponentInteractionVisibility(character, itemComponent);
					if (interactionVisibility != Item.InteractionVisibility.None && !itemComponent.DisplayMsg.IsNullOrEmpty())
					{
						Color color = (interactionVisibility == Item.InteractionVisibility.MissingRequirement) ? Color.Gray : Color.Cyan;
						this.texts.Add(new ColoredText(itemComponent.DisplayMsg.Value, color, false, false));
					}
				}
			}
			if (PlayerInput.KeyDown(InputType.ContextualCommand))
			{
				this.texts.Add(new ColoredText(TextManager.ParseInputTypes(TextManager.Get("itemmsgcontextualorders"), false).Value, Color.Cyan, false, false));
			}
			else
			{
				this.texts.Add(new ColoredText(TextManager.Get("itemmsg.morreoptionsavailable").Value, Color.LightGray * 0.7f, false, false));
			}
			return this.texts;
		}

		// Token: 0x0600189D RID: 6301 RVA: 0x000FB000 File Offset: 0x000F9200
		private static Item.InteractionVisibility GetComponentInteractionVisibility(Character character, ItemComponent itemComponent)
		{
			if (!itemComponent.CanBePicked && !itemComponent.CanBeSelected)
			{
				return Item.InteractionVisibility.None;
			}
			Holdable holdable = itemComponent as Holdable;
			if (holdable != null && !holdable.CanBeDeattached())
			{
				return Item.InteractionVisibility.None;
			}
			ConnectionPanel connectionPanel = itemComponent as ConnectionPanel;
			if (connectionPanel != null && !connectionPanel.CanRewire())
			{
				return Item.InteractionVisibility.None;
			}
			Item.InteractionVisibility interactionVisibility = Item.InteractionVisibility.MissingRequirement;
			if (itemComponent.HasRequiredItems(character, false, null))
			{
				Repairable repairable = itemComponent as Repairable;
				if (repairable != null)
				{
					if (repairable.IsBelowRepairThreshold)
					{
						interactionVisibility = Item.InteractionVisibility.Visible;
					}
				}
				else
				{
					interactionVisibility = Item.InteractionVisibility.Visible;
				}
			}
			return interactionVisibility;
		}

		// Token: 0x0600189E RID: 6302 RVA: 0x000FB070 File Offset: 0x000F9270
		public bool HasVisibleInteraction(Character character)
		{
			foreach (ItemComponent component in this.components)
			{
				if (Item.GetComponentInteractionVisibility(character, component) == Item.InteractionVisibility.Visible)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600189F RID: 6303 RVA: 0x000FB0D0 File Offset: 0x000F92D0
		public void ForceHUDLayoutUpdate(bool ignoreLocking = false)
		{
			foreach (ItemComponent ic in this.activeHUDs)
			{
				if (ic.GuiFrame != null && (ic.CanBeSelected || ic.DrawHudWhenEquipped))
				{
					ic.GuiFrame.RectTransform.ScreenSpaceOffset = Point.Zero;
					if (ic.UseAlternativeLayout)
					{
						ItemComponent.GUILayoutSettings alternativeLayout = ic.AlternativeLayout;
						if (alternativeLayout != null)
						{
							alternativeLayout.ApplyTo(ic.GuiFrame.RectTransform);
						}
					}
					else
					{
						ItemComponent.GUILayoutSettings defaultLayout = ic.DefaultLayout;
						if (defaultLayout != null)
						{
							defaultLayout.ApplyTo(ic.GuiFrame.RectTransform);
						}
					}
				}
			}
			this.SetHUDLayout(ignoreLocking);
		}

		// Token: 0x060018A0 RID: 6304 RVA: 0x000FB194 File Offset: 0x000F9394
		public override void AddToGUIUpdateList(int order = 0)
		{
			if (Screen.Selected is SubEditorScreen)
			{
				if (MapEntity.editingHUD != null && MapEntity.editingHUD.UserData == this)
				{
					MapEntity.editingHUD.AddToGUIUpdateList(false, 0);
				}
			}
			else if (this.HasInGameEditableProperties && Character.Controlled != null && (Character.Controlled.SelectedItem == this || this.EditableWhenEquipped) && MapEntity.editingHUD != null && MapEntity.editingHUD.UserData == this)
			{
				MapEntity.editingHUD.AddToGUIUpdateList(false, 0);
			}
			Character character = Character.Controlled;
			Character controlled = Character.Controlled;
			Item selectedItem = (controlled != null) ? controlled.SelectedItem : null;
			if (character != null && selectedItem != this && this.GetComponent<RemoteController>() == null && (((selectedItem != null) ? selectedItem.GetComponent<CircuitBox>() : null) == null || !selectedItem.ContainedItems.Contains(this)))
			{
				Item item;
				if (selectedItem == null)
				{
					item = null;
				}
				else
				{
					RemoteController component = selectedItem.GetComponent<RemoteController>();
					item = ((component != null) ? component.TargetItem : null);
				}
				if (item != this && !character.HeldItems.Any(delegate(Item it)
				{
					RemoteController component2 = it.GetComponent<RemoteController>();
					return ((component2 != null) ? component2.TargetItem : null) == this;
				}))
				{
					return;
				}
			}
			bool needsLayoutUpdate = false;
			foreach (ItemComponent ic in this.activeHUDs)
			{
				if (ic.CanBeSelected)
				{
					bool useAlternativeLayout = this.activeHUDs.Count > 1;
					bool wasUsingAlternativeLayout = ic.UseAlternativeLayout;
					ic.UseAlternativeLayout = useAlternativeLayout;
					needsLayoutUpdate |= (ic.UseAlternativeLayout != wasUsingAlternativeLayout);
					ic.AddToGUIUpdateList(order);
				}
			}
			if (this.itemInUseWarning != null && this.itemInUseWarning.Visible)
			{
				this.itemInUseWarning.AddToGUIUpdateList(false, 0);
			}
			if (needsLayoutUpdate)
			{
				this.SetHUDLayout(false);
			}
		}

		// Token: 0x060018A1 RID: 6305 RVA: 0x000FB344 File Offset: 0x000F9544
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			Item.EventType eventType = (Item.EventType)msg.ReadRangedInteger(0, 12);
			switch (eventType)
			{
			case Item.EventType.ComponentState:
			{
				int componentIndex = msg.ReadRangedInteger(0, this.components.Count - 1);
				IServerSerializable serverSerializable = this.components[componentIndex] as IServerSerializable;
				if (serverSerializable != null)
				{
					serverSerializable.ClientEventRead(msg, sendingTime);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to read component state - ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(this.components[componentIndex].GetType());
				defaultInterpolatedStringHandler.AppendLiteral(" in item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" is not IServerSerializable.");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			case Item.EventType.InventoryState:
			{
				int containerIndex = msg.ReadRangedInteger(0, this.components.Count - 1);
				ItemContainer container = this.components[containerIndex] as ItemContainer;
				if (container != null)
				{
					container.Inventory.ClientEventRead(msg);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(70, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Failed to read inventory state - ");
				defaultInterpolatedStringHandler2.AppendFormatted<Type>(this.components[containerIndex].GetType());
				defaultInterpolatedStringHandler2.AppendLiteral(" in item \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\"  is not an ItemContainer.");
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			case Item.EventType.ChangeProperty:
				this.ReadPropertyChange(msg, false, null);
				return;
			case Item.EventType.Status:
			{
				bool loadingRound = msg.ReadBoolean();
				float newCondition = msg.ReadSingle();
				this.SetCondition(newCondition, true, !loadingRound);
				return;
			}
			case Item.EventType.AssignCampaignInteraction:
			{
				bool isVisible = msg.ReadBoolean();
				if (isVisible)
				{
					CampaignMode.InteractionType interactionType = (CampaignMode.InteractionType)msg.ReadByte();
					this.AssignCampaignInteractionType(interactionType, null);
					return;
				}
				return;
			}
			case Item.EventType.ApplyStatusEffect:
			{
				ActionType actionType = (ActionType)msg.ReadRangedInteger(0, Enum.GetValues(typeof(ActionType)).Length - 1);
				byte componentIndex2 = msg.ReadByte();
				ushort targetCharacterID = msg.ReadUInt16();
				byte targetLimbID = msg.ReadByte();
				ushort useTargetID = msg.ReadUInt16();
				Vector2? worldPosition = null;
				bool hasPosition = msg.ReadBoolean();
				if (hasPosition)
				{
					worldPosition = new Vector2?(new Vector2(msg.ReadSingle(), msg.ReadSingle()));
				}
				ItemComponent targetComponent = ((int)componentIndex2 < this.components.Count) ? this.components[(int)componentIndex2] : null;
				Character targetCharacter = Entity.FindEntityByID(targetCharacterID) as Character;
				Limb targetLimb = (targetCharacter != null && (int)targetLimbID < targetCharacter.AnimController.Limbs.Length) ? targetCharacter.AnimController.Limbs[(int)targetLimbID] : null;
				Entity useTarget = Entity.FindEntityByID(useTargetID);
				if (targetComponent == null)
				{
					this.ApplyStatusEffects(actionType, 1f, targetCharacter, targetLimb, useTarget, true, worldPosition);
					return;
				}
				targetComponent.ApplyStatusEffects(actionType, 1f, targetCharacter, targetLimb, useTarget, null, worldPosition, 1f);
				return;
			}
			case Item.EventType.Upgrade:
			{
				Identifier identifier = msg.ReadIdentifier();
				byte level = msg.ReadByte();
				UpgradePrefab upgradePrefab = UpgradePrefab.Find(identifier);
				if (upgradePrefab != null)
				{
					Upgrade upgrade = new Upgrade(this, upgradePrefab, (int)level, null);
					byte targetCount = msg.ReadByte();
					for (int i = 0; i < (int)targetCount; i++)
					{
						byte propertyCount = msg.ReadByte();
						for (int j = 0; j < (int)propertyCount; j++)
						{
							float value = msg.ReadSingle();
							upgrade.TargetComponents.ElementAt(i).Value[j].SetOriginalValue(value);
						}
					}
					this.AddUpgrade(upgrade, false);
					return;
				}
				return;
			}
			case Item.EventType.ItemStat:
			{
				byte length = msg.ReadByte();
				for (int k = 0; k < (int)length; k++)
				{
					TalentStatIdentifier statIdentifier = INetSerializableStruct.Read<TalentStatIdentifier>(msg);
					float statValue = msg.ReadSingle();
					this.StatManager.ApplyStatDirect(statIdentifier, statValue);
				}
				return;
			}
			case Item.EventType.DroppedStack:
			{
				int itemCount = msg.ReadRangedInteger(0, 63);
				if (itemCount > 0)
				{
					List<Item> droppedStack = new List<Item>();
					for (int l = 0; l < itemCount; l++)
					{
						ushort id = msg.ReadUInt16();
						Item droppedItem = Entity.FindEntityByID(id) as Item;
						if (droppedItem == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(66, 2);
							defaultInterpolatedStringHandler3.AppendLiteral("Error while reading ");
							defaultInterpolatedStringHandler3.AppendFormatted<Item.EventType>(Item.EventType.DroppedStack);
							defaultInterpolatedStringHandler3.AppendLiteral(" message: could not find an item with the ID ");
							defaultInterpolatedStringHandler3.AppendFormatted<ushort>(id);
							defaultInterpolatedStringHandler3.AppendLiteral(".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
						}
						else
						{
							droppedStack.Add(droppedItem);
						}
					}
					this.CreateDroppedStack(droppedStack, true);
					return;
				}
				this.RemoveFromDroppedStack(true);
				return;
			}
			case Item.EventType.SetHighlight:
			{
				bool isTargetedForThisClient = msg.ReadBoolean();
				if (!isTargetedForThisClient)
				{
					return;
				}
				bool highlight = msg.ReadBoolean();
				base.ExternalHighlight = highlight;
				if (highlight)
				{
					Color highlightColor = msg.ReadColorR8G8B8A8();
					this.HighlightColor = new Color?(highlightColor);
					return;
				}
				this.HighlightColor = null;
				return;
			}
			case Item.EventType.SwapItem:
			{
				ushort newId = msg.ReadUInt16();
				uint prefabUintId = msg.ReadUInt32();
				ItemPrefab newPrefab = ItemPrefab.Prefabs.FirstOrDefault((ItemPrefab p) => p.UintIdentifier == prefabUintId);
				if (newPrefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(75, 2);
					defaultInterpolatedStringHandler4.AppendLiteral("Error while reading ");
					defaultInterpolatedStringHandler4.AppendFormatted<Item.EventType>(Item.EventType.SwapItem);
					defaultInterpolatedStringHandler4.AppendLiteral(" message: could not find an item prefab with the hash ");
					defaultInterpolatedStringHandler4.AppendFormatted<uint>(prefabUintId);
					defaultInterpolatedStringHandler4.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
					return;
				}
				this.ReplaceFromNetwork(newPrefab, newId);
				return;
			}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(54, 1);
			defaultInterpolatedStringHandler5.AppendLiteral("Malformed incoming item event: unsupported event type ");
			defaultInterpolatedStringHandler5.AppendFormatted<Item.EventType>(eventType);
			throw new Exception(defaultInterpolatedStringHandler5.ToStringAndClear());
		}

		// Token: 0x060018A2 RID: 6306 RVA: 0x000FB8B4 File Offset: 0x000F9AB4
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			if (extraData == null)
			{
				throw this.<ClientEventWrite>g__error|73_0("event data was null");
			}
			Item.IEventData eventData = extraData as Item.IEventData;
			if (eventData == null)
			{
				throw this.<ClientEventWrite>g__error|73_0("event data was of the wrong type (\"" + extraData.GetType().Name + "\")");
			}
			Item.EventType eventType = eventData.EventType;
			msg.WriteRangedInteger((int)eventType, 0, 12);
			if (eventData is Item.ComponentStateEventData)
			{
				Item.ComponentStateEventData componentStateEventData = (Item.ComponentStateEventData)eventData;
				ItemComponent component = componentStateEventData.Component;
				if (component == null)
				{
					throw this.<ClientEventWrite>g__error|73_0("component was null");
				}
				IClientSerializable clientSerializable = component as IClientSerializable;
				if (clientSerializable == null)
				{
					throw this.<ClientEventWrite>g__error|73_0("component was not IClientSerializable");
				}
				int componentIndex = this.components.IndexOf(component);
				if (componentIndex < 0)
				{
					throw this.<ClientEventWrite>g__error|73_0("component did not belong to item");
				}
				msg.WriteRangedInteger(componentIndex, 0, this.components.Count - 1);
				clientSerializable.ClientEventWrite(msg, extraData);
				return;
			}
			else if (eventData is Item.InventoryStateEventData)
			{
				Item.InventoryStateEventData inventoryStateEventData = (Item.InventoryStateEventData)eventData;
				ItemContainer container = inventoryStateEventData.Component;
				if (container == null)
				{
					throw this.<ClientEventWrite>g__error|73_0("container was null");
				}
				int containerIndex = this.components.IndexOf(container);
				if (containerIndex < 0)
				{
					throw this.<ClientEventWrite>g__error|73_0("container did not belong to item");
				}
				msg.WriteRangedInteger(containerIndex, 0, this.components.Count - 1);
				container.Inventory.ClientEventWrite(msg, inventoryStateEventData);
				return;
			}
			else
			{
				if (eventData is Item.TreatmentEventData)
				{
					Item.TreatmentEventData treatmentEventData = (Item.TreatmentEventData)eventData;
					Character targetCharacter = treatmentEventData.TargetCharacter;
					msg.WriteUInt16(targetCharacter.ID);
					msg.WriteByte(treatmentEventData.LimbIndex);
					return;
				}
				if (eventData is Item.ChangePropertyEventData)
				{
					Item.ChangePropertyEventData changePropertyEventData = (Item.ChangePropertyEventData)eventData;
					this.WritePropertyChange(msg, changePropertyEventData, true);
					this.editingHUDRefreshTimer = 1f;
					return;
				}
				if (eventData is Item.CombineEventData)
				{
					Item.CombineEventData combineEventData = (Item.CombineEventData)eventData;
					Item combineTarget = combineEventData.CombineTarget;
					msg.WriteUInt16(combineTarget.ID);
					return;
				}
				throw this.<ClientEventWrite>g__error|73_0("Unsupported event type " + eventData.GetType().Name);
			}
		}

		// Token: 0x060018A3 RID: 6307 RVA: 0x000FBAB0 File Offset: 0x000F9CB0
		public void ClientReadPosition(IReadMessage msg, float sendingTime)
		{
			if (this.body == null)
			{
				string errorMsg = "Received a position update for an item with no physics body (" + this.Name + ")";
				if (GameSettings.CurrentConfig.VerboseLogging)
				{
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				GameAnalyticsManager.AddErrorEventOnce("Item.ClientReadPosition:nophysicsbody", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			PosInfo posInfo = this.body.ClientRead(msg, sendingTime, this.Name);
			msg.ReadPadBits();
			Projectile component = this.GetComponent<Projectile>();
			if (component != null && component.IsStuckToTarget)
			{
				return;
			}
			if (posInfo != null)
			{
				int index = 0;
				while (index < this.positionBuffer.Count && sendingTime > this.positionBuffer[index].Timestamp)
				{
					index++;
				}
				this.positionBuffer.Insert(index, posInfo);
			}
		}

		// Token: 0x060018A4 RID: 6308 RVA: 0x000FBB65 File Offset: 0x000F9D65
		public void CreateClientEvent<T>(T ic) where T : ItemComponent, IClientSerializable
		{
			this.CreateClientEvent<T>(ic, null);
		}

		// Token: 0x060018A5 RID: 6309 RVA: 0x000FBB70 File Offset: 0x000F9D70
		public void CreateClientEvent<T>(T ic, ItemComponent.IEventData extraData) where T : ItemComponent, IClientSerializable
		{
			if (GameMain.Client == null)
			{
				return;
			}
			if (!this.components.Contains(ic))
			{
				return;
			}
			Item.ComponentStateEventData eventData = new Item.ComponentStateEventData(ic, extraData);
			if (!ic.ValidateEventData(eventData))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Client-side component event creation for the item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" failed: ");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted("ValidateEventData");
				defaultInterpolatedStringHandler.AppendLiteral(" returned false. ");
				defaultInterpolatedStringHandler.AppendLiteral("Data: ");
				defaultInterpolatedStringHandler.AppendFormatted(((extraData != null) ? extraData.GetType().ToString() : null) ?? "null");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(41, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Item.CreateClientEvent:ValidateEventData:");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				GameAnalyticsManager.AddErrorEventOnce(defaultInterpolatedStringHandler2.ToStringAndClear(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				throw new Exception(errorMsg);
			}
			GameMain.Client.CreateEntityEvent(this, eventData);
		}

		// Token: 0x060018A6 RID: 6310 RVA: 0x000FBCB0 File Offset: 0x000F9EB0
		public static Item ReadSpawnData(IReadMessage msg, bool spawn = true)
		{
			string itemName = msg.ReadString();
			string itemIdentifier = msg.ReadString();
			bool descriptionChanged = msg.ReadBoolean();
			string itemDesc = "";
			if (descriptionChanged)
			{
				itemDesc = msg.ReadString();
			}
			ushort itemId = msg.ReadUInt16();
			ushort inventoryId = msg.ReadUInt16();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Received entity spawn message for item \"");
			defaultInterpolatedStringHandler.AppendFormatted(itemName);
			defaultInterpolatedStringHandler.AppendLiteral("\" (identifier: ");
			defaultInterpolatedStringHandler.AppendFormatted(itemIdentifier);
			defaultInterpolatedStringHandler.AppendLiteral(", id: ");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(itemId);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			ItemPrefab itemPrefab = string.IsNullOrEmpty(itemIdentifier) ? ItemPrefab.Find(itemName, Identifier.Empty) : ItemPrefab.Find(itemName, itemIdentifier.ToIdentifier());
			Vector2 pos = Vector2.Zero;
			Submarine sub = null;
			int itemContainerIndex = -1;
			int inventorySlotIndex = -1;
			if (inventoryId > 0)
			{
				itemContainerIndex = (int)msg.ReadByte();
				inventorySlotIndex = (int)msg.ReadByte();
			}
			else
			{
				pos = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				float rotation = msg.ReadRangedSingle(0f, 6.2831855f, 8);
				ushort subID = msg.ReadUInt16();
				if (subID > 0)
				{
					sub = Submarine.Loaded.Find((Submarine s) => s.ID == subID);
				}
			}
			bool onInsertedEffectsAppliedOnPreviousRound = msg.ReadBoolean();
			byte bodyType = msg.ReadByte();
			bool spawnedInOutpost = msg.ReadBoolean();
			bool allowStealing = msg.ReadBoolean();
			int quality = msg.ReadRangedInteger(0, 3);
			byte teamID = msg.ReadByte();
			bool hasIdCard = msg.ReadBoolean();
			string ownerName = "";
			string ownerTags = "";
			int ownerBeardIndex = -1;
			int ownerHairIndex = -1;
			int ownerMoustacheIndex = -1;
			int ownerFaceAttachmentIndex = -1;
			Color ownerHairColor = Color.White;
			Color ownerFacialHairColor = Color.White;
			Color ownerSkinColor = Color.White;
			Identifier ownerJobId = Identifier.Empty;
			Vector2 ownerSheetIndex = Vector2.Zero;
			int submarineSpecificId = 0;
			if (hasIdCard)
			{
				submarineSpecificId = msg.ReadInt32();
				ownerName = msg.ReadString();
				ownerTags = msg.ReadString();
				ownerBeardIndex = (int)(msg.ReadByte() - 1);
				ownerHairIndex = (int)(msg.ReadByte() - 1);
				ownerMoustacheIndex = (int)(msg.ReadByte() - 1);
				ownerFaceAttachmentIndex = (int)(msg.ReadByte() - 1);
				ownerHairColor = msg.ReadColorR8G8B8();
				ownerFacialHairColor = msg.ReadColorR8G8B8();
				ownerSkinColor = msg.ReadColorR8G8B8();
				ownerJobId = msg.ReadIdentifier();
				int x = (int)msg.ReadByte();
				int y = (int)msg.ReadByte();
				ownerSheetIndex = new ValueTuple<float, float>((float)x, (float)y);
			}
			bool tagsChanged = msg.ReadBoolean();
			string tags = "";
			if (tagsChanged)
			{
				HashSet<Identifier> addedTags = msg.ReadString().ToIdentifiers(",").ToHashSet<Identifier>();
				HashSet<Identifier> removedTags = msg.ReadString().ToIdentifiers(",").ToHashSet<Identifier>();
				if (itemPrefab != null)
				{
					tags = string.Join<Identifier>(',', (from t in itemPrefab.Tags
					where !removedTags.Contains(t)
					select t).Union(addedTags));
				}
			}
			bool isNameTag = msg.ReadBoolean();
			string writtenName = "";
			if (isNameTag)
			{
				writtenName = msg.ReadString();
			}
			if (!spawn)
			{
				return null;
			}
			if (itemPrefab == null)
			{
				string errorMsg = string.Concat(new string[]
				{
					"Failed to spawn item, prefab not found (name: ",
					itemName ?? "null",
					", identifier: ",
					itemIdentifier ?? "null",
					")"
				});
				errorMsg = errorMsg + "\n" + string.Join(", ", from cp in ContentPackageManager.EnabledPackages.All
				select cp.Name);
				GameAnalyticsManager.AddErrorEventOnce("Item.ReadSpawnData:PrefabNotFound" + (itemName ?? "null") + (itemIdentifier ?? "null"), GameAnalyticsManager.ErrorSeverity.Critical, errorMsg);
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				return null;
			}
			Inventory inventory = null;
			if (inventoryId > 0)
			{
				Entity inventoryOwner = Entity.FindEntityByID(inventoryId);
				Character character = inventoryOwner as Character;
				if (character != null)
				{
					inventory = character.Inventory;
				}
				else
				{
					Item parentItem = inventoryOwner as Item;
					if (parentItem != null)
					{
						if (itemContainerIndex < 0 || itemContainerIndex >= parentItem.components.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(104, 5);
							defaultInterpolatedStringHandler2.AppendLiteral("Failed to spawn item \"");
							defaultInterpolatedStringHandler2.AppendFormatted(itemIdentifier ?? "null");
							defaultInterpolatedStringHandler2.AppendLiteral("\" in the inventory of \"");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(parentItem.Prefab.Identifier);
							defaultInterpolatedStringHandler2.AppendLiteral(" (");
							defaultInterpolatedStringHandler2.AppendFormatted<ushort>(parentItem.ID);
							defaultInterpolatedStringHandler2.AppendLiteral(")\" (component index out of range). Index: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(itemContainerIndex);
							defaultInterpolatedStringHandler2.AppendLiteral(", components: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(parentItem.components.Count);
							defaultInterpolatedStringHandler2.AppendLiteral(".");
							string errorMsg2 = defaultInterpolatedStringHandler2.ToStringAndClear();
							GameAnalyticsManager.AddErrorEventOnce("Item.ReadSpawnData:ContainerIndexOutOfRange" + (itemName ?? "null") + (itemIdentifier ?? "null"), GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
							DebugConsole.ThrowError(errorMsg2, null, null, false, false);
							ItemContainer component = parentItem.GetComponent<ItemContainer>();
							inventory = ((component != null) ? component.Inventory : null);
						}
						else
						{
							ItemContainer container = parentItem.components[itemContainerIndex] as ItemContainer;
							if (container != null)
							{
								inventory = container.Inventory;
							}
						}
					}
					else if (inventoryOwner == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(85, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Failed to spawn item \"");
						defaultInterpolatedStringHandler3.AppendFormatted(itemIdentifier ?? "null");
						defaultInterpolatedStringHandler3.AppendLiteral("\" in the inventory of an entity with the ID ");
						defaultInterpolatedStringHandler3.AppendFormatted<ushort>(inventoryId);
						defaultInterpolatedStringHandler3.AppendLiteral(" (entity not found)");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(100, 3);
						defaultInterpolatedStringHandler4.AppendLiteral("Failed to spawn item \"");
						defaultInterpolatedStringHandler4.AppendFormatted(itemIdentifier ?? "null");
						defaultInterpolatedStringHandler4.AppendLiteral("\" in the inventory of \"");
						defaultInterpolatedStringHandler4.AppendFormatted<Entity>(inventoryOwner);
						defaultInterpolatedStringHandler4.AppendLiteral(" (");
						defaultInterpolatedStringHandler4.AppendFormatted<ushort>(inventoryOwner.ID);
						defaultInterpolatedStringHandler4.AppendLiteral(")\" (invalid entity, should be an item or a character)");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
					}
				}
			}
			Item item = null;
			try
			{
				item = new Item(itemPrefab, pos, sub, itemId, true)
				{
					SpawnedInCurrentOutpost = spawnedInOutpost,
					AllowStealing = allowStealing,
					Quality = quality
				};
				if (onInsertedEffectsAppliedOnPreviousRound)
				{
					item.OnInsertedEffectsApplied = (item.OnInsertedEffectsAppliedOnPreviousRound = true);
				}
			}
			catch (Exception e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(21, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("Failed to spawn item ");
				defaultInterpolatedStringHandler5.AppendFormatted<LocalizedString>(itemPrefab.Name);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), e, null, false, false);
				throw;
			}
			if (item.body != null)
			{
				item.body.BodyType = (BodyType)bodyType;
			}
			foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
			{
				wifiComponent.TeamID = (CharacterTeamType)teamID;
			}
			foreach (IdCard idCard in item.GetComponents<IdCard>())
			{
				idCard.SubmarineSpecificID = submarineSpecificId;
				idCard.TeamID = (CharacterTeamType)teamID;
				idCard.OwnerName = ownerName;
				idCard.OwnerTags = ownerTags;
				idCard.OwnerBeardIndex = ownerBeardIndex;
				idCard.OwnerHairIndex = ownerHairIndex;
				idCard.OwnerMoustacheIndex = ownerMoustacheIndex;
				idCard.OwnerFaceAttachmentIndex = ownerFaceAttachmentIndex;
				idCard.OwnerHairColor = ownerHairColor;
				idCard.OwnerFacialHairColor = ownerFacialHairColor;
				idCard.OwnerSkinColor = ownerSkinColor;
				idCard.OwnerJobId = ownerJobId;
				idCard.OwnerSheetIndex = ownerSheetIndex;
			}
			if (descriptionChanged)
			{
				item.Description = itemDesc;
			}
			if (tagsChanged)
			{
				item.Tags = tags;
			}
			NameTag nameTag = item.GetComponent<NameTag>();
			if (nameTag != null)
			{
				nameTag.WrittenName = writtenName;
			}
			if (sub != null)
			{
				item.CurrentHull = Hull.FindHull(pos + sub.Position, null, true, true);
				Entity entity = item;
				Hull hull = item.CurrentHull;
				entity.Submarine = ((hull != null) ? hull.Submarine : null);
			}
			if (inventory != null)
			{
				if (inventorySlotIndex >= 0 && inventorySlotIndex < 255 && !inventory.TryPutItem(item, inventorySlotIndex, false, false, null, false, true, true) && inventory.IsSlotEmpty(inventorySlotIndex))
				{
					inventory.ForceToSlot(item, inventorySlotIndex);
				}
				else
				{
					inventory.TryPutItem(item, null, item.AllowedSlots, false, false, true);
				}
				item.SetTransform(inventory.Owner.SimPosition, 0f, true, true, inventory.Owner.Submarine);
				Character character2 = inventory.Owner as Character;
				if (character2 != null && !character2.Enabled && item.body != null)
				{
					item.body.Enabled = false;
				}
			}
			return item;
		}

		// Token: 0x060018A7 RID: 6311 RVA: 0x000FC550 File Offset: 0x000FA750
		public void OnPlayerSkillsChanged()
		{
			foreach (ItemComponent ic in this.components)
			{
				ic.OnPlayerSkillsChanged();
			}
		}

		// Token: 0x17000603 RID: 1539
		// (get) Token: 0x060018A8 RID: 6312 RVA: 0x000FC5A4 File Offset: 0x000FA7A4
		public static IReadOnlyCollection<Item> DangerousItems
		{
			get
			{
				return Item._dangerousItems;
			}
		}

		// Token: 0x17000604 RID: 1540
		// (get) Token: 0x060018A9 RID: 6313 RVA: 0x000FC5AB File Offset: 0x000FA7AB
		public static IReadOnlyCollection<Item> RepairableItems
		{
			get
			{
				return Item._repairableItems;
			}
		}

		// Token: 0x17000605 RID: 1541
		// (get) Token: 0x060018AA RID: 6314 RVA: 0x000FC5B2 File Offset: 0x000FA7B2
		public static IReadOnlyCollection<Item> CleanableItems
		{
			get
			{
				return Item._cleanableItems;
			}
		}

		// Token: 0x17000606 RID: 1542
		// (get) Token: 0x060018AB RID: 6315 RVA: 0x000FC5B9 File Offset: 0x000FA7B9
		public static HashSet<Item> DeconstructItems
		{
			get
			{
				return Item._deconstructItems;
			}
		}

		// Token: 0x17000607 RID: 1543
		// (get) Token: 0x060018AC RID: 6316 RVA: 0x000FC5C0 File Offset: 0x000FA7C0
		public static IReadOnlyCollection<Item> SonarVisibleItems
		{
			get
			{
				return Item._sonarVisibleItems;
			}
		}

		// Token: 0x17000608 RID: 1544
		// (get) Token: 0x060018AD RID: 6317 RVA: 0x000FC5C7 File Offset: 0x000FA7C7
		public static IReadOnlyCollection<Item> TurretTargetItems
		{
			get
			{
				return Item._turretTargetItems;
			}
		}

		// Token: 0x17000609 RID: 1545
		// (get) Token: 0x060018AE RID: 6318 RVA: 0x000FC5CE File Offset: 0x000FA7CE
		public static IReadOnlyCollection<Item> ChairItems
		{
			get
			{
				return Item._chairItems;
			}
		}

		// Token: 0x1700060A RID: 1546
		// (get) Token: 0x060018AF RID: 6319 RVA: 0x000FC5D5 File Offset: 0x000FA7D5
		public new ItemPrefab Prefab
		{
			get
			{
				return this.Prefab as ItemPrefab;
			}
		}

		// Token: 0x1700060B RID: 1547
		// (get) Token: 0x060018B0 RID: 6320 RVA: 0x000FC5E2 File Offset: 0x000FA7E2
		public override ContentPackage ContentPackage
		{
			get
			{
				ItemPrefab prefab = this.Prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.ContentPackage;
			}
		}

		// Token: 0x1700060C RID: 1548
		// (get) Token: 0x060018B1 RID: 6321 RVA: 0x000FC5F5 File Offset: 0x000FA7F5
		// (set) Token: 0x060018B2 RID: 6322 RVA: 0x000FC5FD File Offset: 0x000FA7FD
		public Hull CurrentHull
		{
			get
			{
				return this.currentHull;
			}
			set
			{
				this.currentHull = value;
			}
		}

		// Token: 0x1700060D RID: 1549
		// (get) Token: 0x060018B3 RID: 6323 RVA: 0x000FC606 File Offset: 0x000FA806
		public float HullOxygenPercentage
		{
			get
			{
				Hull hull = this.CurrentHull;
				if (hull == null)
				{
					return 0f;
				}
				return hull.OxygenPercentage;
			}
		}

		// Token: 0x1700060E RID: 1550
		// (get) Token: 0x060018B4 RID: 6324 RVA: 0x000FC61D File Offset: 0x000FA81D
		public CampaignMode.InteractionType CampaignInteractionType
		{
			get
			{
				return this.campaignInteractionType;
			}
		}

		// Token: 0x060018B5 RID: 6325 RVA: 0x000FC625 File Offset: 0x000FA825
		public void AssignCampaignInteractionType(CampaignMode.InteractionType interactionType, IEnumerable<Client> targetClients = null)
		{
			if (this.campaignInteractionType == interactionType)
			{
				return;
			}
			this.campaignInteractionType = interactionType;
			this.AssignCampaignInteractionTypeProjSpecific(this.campaignInteractionType, targetClients);
		}

		// Token: 0x060018B6 RID: 6326 RVA: 0x000FC648 File Offset: 0x000FA848
		private void AssignCampaignInteractionTypeProjSpecific(CampaignMode.InteractionType interactionType, IEnumerable<Client> targetClients)
		{
			if (interactionType == CampaignMode.InteractionType.None)
			{
				this.IconStyle = null;
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
			defaultInterpolatedStringHandler.AppendLiteral("CampaignInteractionIcon.");
			defaultInterpolatedStringHandler.AppendFormatted<CampaignMode.InteractionType>(interactionType);
			this.IconStyle = GUIStyle.GetComponentStyle(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x1700060F RID: 1551
		// (get) Token: 0x060018B7 RID: 6327 RVA: 0x000FC690 File Offset: 0x000FA890
		// (set) Token: 0x060018B8 RID: 6328 RVA: 0x000FC698 File Offset: 0x000FA898
		public bool FullyInitialized { get; private set; }

		// Token: 0x17000610 RID: 1552
		// (get) Token: 0x060018B9 RID: 6329 RVA: 0x000FC6A4 File Offset: 0x000FA8A4
		// (set) Token: 0x060018BA RID: 6330 RVA: 0x000FC6CF File Offset: 0x000FA8CF
		public float WaterDragCoefficient
		{
			get
			{
				float? num = this.overrideWaterDragCoefficient;
				if (num == null)
				{
					return this.originalWaterDragCoefficient;
				}
				return num.GetValueOrDefault();
			}
			set
			{
				this.overrideWaterDragCoefficient = new float?(value);
			}
		}

		// Token: 0x17000611 RID: 1553
		// (get) Token: 0x060018BB RID: 6331 RVA: 0x000FC6DD File Offset: 0x000FA8DD
		// (set) Token: 0x060018BC RID: 6332 RVA: 0x000FC6F0 File Offset: 0x000FA8F0
		public BodyType BodyType
		{
			get
			{
				PhysicsBody physicsBody = this.body;
				if (physicsBody == null)
				{
					return BodyType.Dynamic;
				}
				return physicsBody.BodyType;
			}
			set
			{
				if (this.body != null)
				{
					this.body.BodyType = value;
				}
			}
		}

		// Token: 0x060018BD RID: 6333 RVA: 0x000FC706 File Offset: 0x000FA906
		public void ResetWaterDragCoefficient()
		{
			this.overrideWaterDragCoefficient = null;
		}

		// Token: 0x17000612 RID: 1554
		// (get) Token: 0x060018BE RID: 6334 RVA: 0x000FC714 File Offset: 0x000FA914
		// (set) Token: 0x060018BF RID: 6335 RVA: 0x000FC71C File Offset: 0x000FA91C
		public Rectangle DefaultRect
		{
			get
			{
				return this.defaultRect;
			}
			set
			{
				this.defaultRect = value;
			}
		}

		// Token: 0x17000613 RID: 1555
		// (get) Token: 0x060018C0 RID: 6336 RVA: 0x000FC725 File Offset: 0x000FA925
		// (set) Token: 0x060018C1 RID: 6337 RVA: 0x000FC72D File Offset: 0x000FA92D
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; protected set; }

		// Token: 0x17000614 RID: 1556
		// (get) Token: 0x060018C2 RID: 6338 RVA: 0x000FC738 File Offset: 0x000FA938
		private bool HasInGameEditableProperties
		{
			get
			{
				if (this.hasInGameEditableProperties == null)
				{
					this.hasInGameEditableProperties = new bool?(false);
					if (this.SerializableProperties.Values.Any((SerializableProperty p) => p.Attributes.OfType<InGameEditable>().Any<InGameEditable>()))
					{
						this.hasInGameEditableProperties = new bool?(true);
					}
					else
					{
						foreach (ItemComponent component in this.components)
						{
							if (component.AllowInGameEditing)
							{
								if (component.SerializableProperties.Values.Any((SerializableProperty p) => p.Attributes.OfType<InGameEditable>().Any<InGameEditable>()) || component.SerializableProperties.Values.Any((SerializableProperty p) => p.Attributes.OfType<ConditionallyEditable>().Any((ConditionallyEditable a) => a.IsEditable(this))))
								{
									this.hasInGameEditableProperties = new bool?(true);
									break;
								}
							}
						}
					}
				}
				return this.hasInGameEditableProperties.Value;
			}
		}

		// Token: 0x17000615 RID: 1557
		// (get) Token: 0x060018C3 RID: 6339 RVA: 0x000FC854 File Offset: 0x000FAA54
		// (set) Token: 0x060018C4 RID: 6340 RVA: 0x000FC85C File Offset: 0x000FAA5C
		public bool EditableWhenEquipped { get; set; }

		// Token: 0x17000616 RID: 1558
		// (get) Token: 0x060018C5 RID: 6341 RVA: 0x000FC865 File Offset: 0x000FAA65
		// (set) Token: 0x060018C6 RID: 6342 RVA: 0x000FC86D File Offset: 0x000FAA6D
		public Inventory PreviousParentInventory { get; set; }

		// Token: 0x17000617 RID: 1559
		// (get) Token: 0x060018C7 RID: 6343 RVA: 0x000FC876 File Offset: 0x000FAA76
		// (set) Token: 0x060018C8 RID: 6344 RVA: 0x000FC87E File Offset: 0x000FAA7E
		public Inventory ParentInventory
		{
			get
			{
				return this.parentInventory;
			}
			set
			{
				this.parentInventory = value;
				if (this.parentInventory != null)
				{
					this.Container = (this.parentInventory.Owner as Item);
					this.RemoveFromDroppedStack(false);
				}
				this.PreviousParentInventory = value;
			}
		}

		// Token: 0x17000618 RID: 1560
		// (get) Token: 0x060018C9 RID: 6345 RVA: 0x000FC8B3 File Offset: 0x000FAAB3
		// (set) Token: 0x060018CA RID: 6346 RVA: 0x000FC8BC File Offset: 0x000FAABC
		public Item RootContainer
		{
			get
			{
				return this.rootContainer;
			}
			private set
			{
				if (value == this)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Attempted to set the item \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" as it's own root container!\n");
					defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					this.rootContainer = null;
					return;
				}
				this.rootContainer = value;
			}
		}

		// Token: 0x17000619 RID: 1561
		// (get) Token: 0x060018CB RID: 6347 RVA: 0x000FC931 File Offset: 0x000FAB31
		// (set) Token: 0x060018CC RID: 6348 RVA: 0x000FC939 File Offset: 0x000FAB39
		public Item Container
		{
			get
			{
				return this.container;
			}
			private set
			{
				if (value != this.container)
				{
					this.container = value;
					this.CheckCleanable();
					this.SetActiveSprite();
					this.RefreshRootContainer();
				}
			}
		}

		// Token: 0x1700061A RID: 1562
		// (get) Token: 0x060018CD RID: 6349 RVA: 0x000FC95D File Offset: 0x000FAB5D
		public override string Name
		{
			get
			{
				return this.Prefab.Name.Value;
			}
		}

		// Token: 0x1700061B RID: 1563
		// (get) Token: 0x060018CE RID: 6350 RVA: 0x000FC96F File Offset: 0x000FAB6F
		// (set) Token: 0x060018CF RID: 6351 RVA: 0x000FC98B File Offset: 0x000FAB8B
		public string Description
		{
			get
			{
				return this.description ?? this.Prefab.Description.Value;
			}
			set
			{
				this.description = value;
			}
		}

		// Token: 0x1700061C RID: 1564
		// (get) Token: 0x060018D0 RID: 6352 RVA: 0x000FC994 File Offset: 0x000FAB94
		// (set) Token: 0x060018D1 RID: 6353 RVA: 0x000FC99C File Offset: 0x000FAB9C
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.OnlyByStatusEffectsAndNetwork, true)]
		public string DescriptionTag
		{
			get
			{
				return this.descriptionTag;
			}
			set
			{
				if (value == this.descriptionTag)
				{
					return;
				}
				if (value.IsNullOrEmpty())
				{
					this.descriptionTag = null;
					this.description = null;
				}
				else
				{
					this.description = TextManager.Get(value).Value;
					this.descriptionTag = value;
				}
				SerializableProperty property;
				if (this.FullyInitialized && this.SerializableProperties != null && this.SerializableProperties.TryGetValue("DescriptionTag".ToIdentifier(), out property))
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember == null)
					{
						return;
					}
					networkMember.CreateEntityEvent(this, new Item.ChangePropertyEventData(property, this));
				}
			}
		}

		// Token: 0x1700061D RID: 1565
		// (get) Token: 0x060018D2 RID: 6354 RVA: 0x000FCA2D File Offset: 0x000FAC2D
		// (set) Token: 0x060018D3 RID: 6355 RVA: 0x000FCA35 File Offset: 0x000FAC35
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool NonInteractable { get; set; }

		// Token: 0x1700061E RID: 1566
		// (get) Token: 0x060018D4 RID: 6356 RVA: 0x000FCA3E File Offset: 0x000FAC3E
		// (set) Token: 0x060018D5 RID: 6357 RVA: 0x000FCA46 File Offset: 0x000FAC46
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "When enabled, item is interactable only for characters on non-player teams.", "", true)]
		public bool NonPlayerTeamInteractable { get; set; }

		// Token: 0x1700061F RID: 1567
		// (get) Token: 0x060018D6 RID: 6358 RVA: 0x000FCA4F File Offset: 0x000FAC4F
		// (set) Token: 0x060018D7 RID: 6359 RVA: 0x000FCA57 File Offset: 0x000FAC57
		[ConditionallyEditable(ConditionallyEditable.ConditionType.IsSwappableItem, true)]
		[Serialize(true, IsPropertySaveable.Yes, "", "", true)]
		public bool AllowSwapping { get; set; }

		// Token: 0x17000620 RID: 1568
		// (get) Token: 0x060018D8 RID: 6360 RVA: 0x000FCA60 File Offset: 0x000FAC60
		// (set) Token: 0x060018D9 RID: 6361 RVA: 0x000FCA68 File Offset: 0x000FAC68
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PurchasedNewSwap { get; set; }

		// Token: 0x17000621 RID: 1569
		// (get) Token: 0x060018DA RID: 6362 RVA: 0x000FCA71 File Offset: 0x000FAC71
		public bool IsPlayerTeamInteractable
		{
			get
			{
				return !this.NonInteractable && !this.NonPlayerTeamInteractable;
			}
		}

		// Token: 0x060018DB RID: 6363 RVA: 0x000FCA86 File Offset: 0x000FAC86
		public bool IsInteractable(Character character)
		{
			if (Screen.Selected is EditorScreen)
			{
				return true;
			}
			if (base.IsHidden)
			{
				return false;
			}
			if (character != null && character.IsOnPlayerTeam)
			{
				return this.IsPlayerTeamInteractable;
			}
			return !this.NonInteractable;
		}

		// Token: 0x17000622 RID: 1570
		// (get) Token: 0x060018DC RID: 6364 RVA: 0x000FCABB File Offset: 0x000FACBB
		// (set) Token: 0x060018DD RID: 6365 RVA: 0x000FCAC8 File Offset: 0x000FACC8
		[ConditionallyEditable(ConditionallyEditable.ConditionType.AllowRotating, true, DecimalCount = 3, ForceShowPlusMinusButtons = true, ValueStep = 0.1f)]
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Rotation
		{
			get
			{
				return MathHelper.ToDegrees(this.RotationRad);
			}
			set
			{
				if (!this.Prefab.AllowRotatingInEditor)
				{
					return;
				}
				this.RotationRad = MathUtils.WrapAnglePi(MathHelper.ToRadians(value));
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					this.SetContainedItemPositions();
					foreach (LightComponent light in this.GetComponents<LightComponent>())
					{
						light.SetLightSourceTransform();
					}
					foreach (Turret turret in this.GetComponents<Turret>())
					{
						turret.UpdateLightComponents();
					}
					foreach (TriggerComponent triggerComponent in this.GetComponents<TriggerComponent>())
					{
						triggerComponent.SetPhysicsBodyPosition(true);
					}
				}
			}
		}

		// Token: 0x17000623 RID: 1571
		// (get) Token: 0x060018DE RID: 6366 RVA: 0x000FCBC8 File Offset: 0x000FADC8
		// (set) Token: 0x060018DF RID: 6367 RVA: 0x000FCBD0 File Offset: 0x000FADD0
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.ReceivesSubmarineImpacts, true, MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float ImpactTolerance
		{
			get
			{
				return this.impactTolerance;
			}
			set
			{
				this.impactTolerance = Math.Max(value, 0f);
			}
		}

		// Token: 0x17000624 RID: 1572
		// (get) Token: 0x060018E0 RID: 6368 RVA: 0x000FCBE3 File Offset: 0x000FADE3
		// (set) Token: 0x060018E1 RID: 6369 RVA: 0x000FCBEB File Offset: 0x000FADEB
		[Serialize(0f, IsPropertySaveable.Yes, "The amount of damage the item takes from impacts. Acts as a multiplier on the strength of the impact. Note that ImpactTolerance must be set for impacts to register.", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.ReceivesSubmarineImpacts, true, MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float ImpactDamage { get; set; }

		// Token: 0x17000625 RID: 1573
		// (get) Token: 0x060018E2 RID: 6370 RVA: 0x000FCBF4 File Offset: 0x000FADF4
		// (set) Token: 0x060018E3 RID: 6371 RVA: 0x000FCBFC File Offset: 0x000FADFC
		[Serialize(1f, IsPropertySaveable.Yes, "Probability for impacts to register. Defaults to 1. Note that ImpactTolerance must also be set for impacts to register.", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.ReceivesSubmarineImpacts, true, MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float ImpactDamageProbability { get; set; }

		// Token: 0x17000626 RID: 1574
		// (get) Token: 0x060018E4 RID: 6372 RVA: 0x000FCC05 File Offset: 0x000FAE05
		public float InteractDistance
		{
			get
			{
				return this.Prefab.InteractDistance;
			}
		}

		// Token: 0x17000627 RID: 1575
		// (get) Token: 0x060018E5 RID: 6373 RVA: 0x000FCC12 File Offset: 0x000FAE12
		public float InteractPriority
		{
			get
			{
				return this.Prefab.InteractPriority;
			}
		}

		// Token: 0x17000628 RID: 1576
		// (get) Token: 0x060018E6 RID: 6374 RVA: 0x000FCC1F File Offset: 0x000FAE1F
		public override Vector2 Position
		{
			get
			{
				if (this.body != null)
				{
					return this.body.Position;
				}
				return base.Position;
			}
		}

		// Token: 0x17000629 RID: 1577
		// (get) Token: 0x060018E7 RID: 6375 RVA: 0x000FCC3B File Offset: 0x000FAE3B
		public override Vector2 SimPosition
		{
			get
			{
				if (this.body != null)
				{
					return this.body.SimPosition;
				}
				return ConvertUnits.ToSimUnits(base.Position);
			}
		}

		// Token: 0x1700062A RID: 1578
		// (get) Token: 0x060018E8 RID: 6376 RVA: 0x000FCC5C File Offset: 0x000FAE5C
		public Rectangle InteractionRect
		{
			get
			{
				return base.WorldRect;
			}
		}

		// Token: 0x1700062B RID: 1579
		// (get) Token: 0x060018E9 RID: 6377 RVA: 0x000FCC64 File Offset: 0x000FAE64
		// (set) Token: 0x060018EA RID: 6378 RVA: 0x000FCC6C File Offset: 0x000FAE6C
		public override float Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				if (this.scale == value)
				{
					return;
				}
				this.scale = MathHelper.Clamp(value, this.Prefab.MinScale, this.Prefab.MaxScale);
				float relativeScale = this.scale / this.Prefab.Scale;
				if (!base.ResizeHorizontal || !base.ResizeVertical)
				{
					int newWidth = base.ResizeHorizontal ? this.rect.Width : ((int)((float)this.defaultRect.Width * relativeScale));
					int newHeight = base.ResizeVertical ? this.rect.Height : ((int)((float)this.defaultRect.Height * relativeScale));
					this.Rect = new Rectangle(this.rect.X, this.rect.Y, newWidth, newHeight);
				}
				if (this.body != null)
				{
					if (this.FullyInitialized)
					{
						Screen selected = Screen.Selected;
						if (selected != null && selected.IsEditor)
						{
							this.UpdateTransform();
						}
					}
					else
					{
						this.body.SetTransformIgnoreContacts(ConvertUnits.ToSimUnits(base.Position), this.body.Rotation, true);
					}
				}
				if (this.components != null)
				{
					foreach (ItemComponent component in this.components)
					{
						component.OnScaleChanged();
					}
				}
			}
		}

		// Token: 0x1700062C RID: 1580
		// (get) Token: 0x060018EB RID: 6379 RVA: 0x000FCDD4 File Offset: 0x000FAFD4
		// (set) Token: 0x060018EC RID: 6380 RVA: 0x000FCDDC File Offset: 0x000FAFDC
		public float PositionUpdateInterval { get; set; } = float.PositiveInfinity;

		// Token: 0x1700062D RID: 1581
		// (get) Token: 0x060018ED RID: 6381 RVA: 0x000FCDE5 File Offset: 0x000FAFE5
		// (set) Token: 0x060018EE RID: 6382 RVA: 0x000FCDED File Offset: 0x000FAFED
		public Sprite OverrideInventorySprite { get; set; }

		// Token: 0x1700062E RID: 1582
		// (get) Token: 0x060018EF RID: 6383 RVA: 0x000FCDF6 File Offset: 0x000FAFF6
		// (set) Token: 0x060018F0 RID: 6384 RVA: 0x000FCDFE File Offset: 0x000FAFFE
		[Editable]
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		public Color SpriteColor
		{
			get
			{
				return this.spriteColor;
			}
			set
			{
				this.spriteColor = value;
			}
		}

		// Token: 0x1700062F RID: 1583
		// (get) Token: 0x060018F1 RID: 6385 RVA: 0x000FCE07 File Offset: 0x000FB007
		// (set) Token: 0x060018F2 RID: 6386 RVA: 0x000FCE0F File Offset: 0x000FB00F
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.Pickable, true)]
		public Color InventoryIconColor { get; protected set; }

		// Token: 0x17000630 RID: 1584
		// (get) Token: 0x060018F3 RID: 6387 RVA: 0x000FCE18 File Offset: 0x000FB018
		// (set) Token: 0x060018F4 RID: 6388 RVA: 0x000FCE20 File Offset: 0x000FB020
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "Changes the color of the item this item is contained inside. Only has an effect if either of the UseContainedSpriteColor or UseContainedInventoryIconColor property of the container is set to true.", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.Pickable, true)]
		public Color ContainerColor { get; protected set; }

		// Token: 0x17000631 RID: 1585
		// (get) Token: 0x060018F5 RID: 6389 RVA: 0x000FCE2C File Offset: 0x000FB02C
		public Identifier ContainerIdentifier
		{
			get
			{
				Item item = this.Container;
				if (item != null)
				{
					return item.Prefab.Identifier;
				}
				Inventory inventory = this.ParentInventory;
				Identifier? identifier;
				if (inventory == null)
				{
					identifier = null;
				}
				else
				{
					Entity owner = inventory.Owner;
					identifier = ((owner != null) ? new Identifier?(owner.ToIdentifier<Entity>()) : null);
				}
				Identifier? identifier2 = identifier;
				if (identifier2 == null)
				{
					return Identifier.Empty;
				}
				return identifier2.GetValueOrDefault();
			}
		}

		// Token: 0x17000632 RID: 1586
		// (get) Token: 0x060018F6 RID: 6390 RVA: 0x000FCE98 File Offset: 0x000FB098
		public bool IsContained
		{
			get
			{
				return this.parentInventory != null;
			}
		}

		// Token: 0x17000633 RID: 1587
		// (get) Token: 0x060018F7 RID: 6391 RVA: 0x000FCEA4 File Offset: 0x000FB0A4
		public float Speed
		{
			get
			{
				if (this.body != null && this.body.PhysEnabled)
				{
					return this.body.LinearVelocity.Length();
				}
				Inventory inventory = this.ParentInventory;
				Character character = ((inventory != null) ? inventory.Owner : null) as Character;
				if (character != null)
				{
					return character.AnimController.MainLimb.LinearVelocity.Length();
				}
				if (this.container != null)
				{
					return this.container.Speed;
				}
				return 0f;
			}
		}

		// Token: 0x17000634 RID: 1588
		// (get) Token: 0x060018F8 RID: 6392 RVA: 0x000FCF27 File Offset: 0x000FB127
		// (set) Token: 0x060018F9 RID: 6393 RVA: 0x000FCF50 File Offset: 0x000FB150
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SonarLabel
		{
			get
			{
				AITarget aiTarget = base.AiTarget;
				string text;
				if (aiTarget == null)
				{
					text = null;
				}
				else
				{
					LocalizedString sonarLabel = aiTarget.SonarLabel;
					text = ((sonarLabel != null) ? sonarLabel.Value : null);
				}
				return text ?? "";
			}
			set
			{
				if (base.AiTarget != null)
				{
					string trimmedStr = (!string.IsNullOrEmpty(value) && value.Length > 250) ? value.Substring(250) : value;
					base.AiTarget.SonarLabel = TextManager.Get(trimmedStr).Fallback(trimmedStr, true);
				}
			}
		}

		// Token: 0x17000635 RID: 1589
		// (get) Token: 0x060018FA RID: 6394 RVA: 0x000FCFA6 File Offset: 0x000FB1A6
		public bool PhysicsBodyActive
		{
			get
			{
				return this.body != null && this.body.Enabled;
			}
		}

		// Token: 0x17000636 RID: 1590
		// (get) Token: 0x060018FB RID: 6395 RVA: 0x000FCFBD File Offset: 0x000FB1BD
		// (set) Token: 0x060018FC RID: 6396 RVA: 0x000FCFD8 File Offset: 0x000FB1D8
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public new float SoundRange
		{
			get
			{
				if (this.aiTarget != null)
				{
					return this.aiTarget.SoundRange;
				}
				return 0f;
			}
			set
			{
				if (this.aiTarget != null)
				{
					this.aiTarget.SoundRange = Math.Max(0f, value);
				}
			}
		}

		// Token: 0x17000637 RID: 1591
		// (get) Token: 0x060018FD RID: 6397 RVA: 0x000FCFF8 File Offset: 0x000FB1F8
		// (set) Token: 0x060018FE RID: 6398 RVA: 0x000FD013 File Offset: 0x000FB213
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public new float SightRange
		{
			get
			{
				if (this.aiTarget != null)
				{
					return this.aiTarget.SightRange;
				}
				return 0f;
			}
			set
			{
				if (this.aiTarget != null)
				{
					this.aiTarget.SightRange = Math.Max(0f, value);
				}
			}
		}

		// Token: 0x17000638 RID: 1592
		// (get) Token: 0x060018FF RID: 6399 RVA: 0x000FD033 File Offset: 0x000FB233
		// (set) Token: 0x06001900 RID: 6400 RVA: 0x000FD03B File Offset: 0x000FB23B
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool IsShootable { get; set; }

		// Token: 0x17000639 RID: 1593
		// (get) Token: 0x06001901 RID: 6401 RVA: 0x000FD044 File Offset: 0x000FB244
		// (set) Token: 0x06001902 RID: 6402 RVA: 0x000FD04C File Offset: 0x000FB24C
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RequireAimToUse { get; set; }

		// Token: 0x1700063A RID: 1594
		// (get) Token: 0x06001903 RID: 6403 RVA: 0x000FD055 File Offset: 0x000FB255
		// (set) Token: 0x06001904 RID: 6404 RVA: 0x000FD05D File Offset: 0x000FB25D
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool RequireAimToSecondaryUse { get; set; }

		// Token: 0x1700063B RID: 1595
		// (get) Token: 0x06001905 RID: 6405 RVA: 0x000FD066 File Offset: 0x000FB266
		// (set) Token: 0x06001906 RID: 6406 RVA: 0x000FD06E File Offset: 0x000FB26E
		public bool DontCleanUp { get; set; }

		// Token: 0x1700063C RID: 1596
		// (get) Token: 0x06001907 RID: 6407 RVA: 0x000FD077 File Offset: 0x000FB277
		// (set) Token: 0x06001908 RID: 6408 RVA: 0x000FD07F File Offset: 0x000FB27F
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool OnInsertedEffectsApplied { get; set; }

		// Token: 0x1700063D RID: 1597
		// (get) Token: 0x06001909 RID: 6409 RVA: 0x000FD088 File Offset: 0x000FB288
		public Color Color
		{
			get
			{
				return this.spriteColor;
			}
		}

		// Token: 0x1700063E RID: 1598
		// (get) Token: 0x0600190A RID: 6410 RVA: 0x000FD090 File Offset: 0x000FB290
		// (set) Token: 0x0600190B RID: 6411 RVA: 0x000FD098 File Offset: 0x000FB298
		public bool IsFullCondition { get; private set; }

		// Token: 0x1700063F RID: 1599
		// (get) Token: 0x0600190C RID: 6412 RVA: 0x000FD0A1 File Offset: 0x000FB2A1
		// (set) Token: 0x0600190D RID: 6413 RVA: 0x000FD0A9 File Offset: 0x000FB2A9
		public float MaxCondition { get; private set; }

		// Token: 0x17000640 RID: 1600
		// (get) Token: 0x0600190E RID: 6414 RVA: 0x000FD0B2 File Offset: 0x000FB2B2
		// (set) Token: 0x0600190F RID: 6415 RVA: 0x000FD0BA File Offset: 0x000FB2BA
		public float ConditionPercentage { get; private set; }

		// Token: 0x17000641 RID: 1601
		// (get) Token: 0x06001910 RID: 6416 RVA: 0x000FD0C4 File Offset: 0x000FB2C4
		public float ConditionPercentageRelativeToDefaultMaxCondition
		{
			get
			{
				float defaultMaxCondition = this.MaxCondition / this.MaxRepairConditionMultiplier;
				return MathUtils.Percentage(this.Condition, defaultMaxCondition);
			}
		}

		// Token: 0x17000642 RID: 1602
		// (get) Token: 0x06001911 RID: 6417 RVA: 0x000FD0EB File Offset: 0x000FB2EB
		// (set) Token: 0x06001912 RID: 6418 RVA: 0x000FD0F3 File Offset: 0x000FB2F3
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float OffsetOnSelectedMultiplier
		{
			get
			{
				return this.offsetOnSelectedMultiplier;
			}
			set
			{
				this.offsetOnSelectedMultiplier = value;
			}
		}

		// Token: 0x17000643 RID: 1603
		// (get) Token: 0x06001913 RID: 6419 RVA: 0x000FD0FC File Offset: 0x000FB2FC
		// (set) Token: 0x06001914 RID: 6420 RVA: 0x000FD104 File Offset: 0x000FB304
		[Serialize(1f, IsPropertySaveable.Yes, "Multiply the maximum condition by this value", "", false)]
		public float HealthMultiplier
		{
			get
			{
				return this.healthMultiplier;
			}
			set
			{
				float prevConditionPercentage = this.ConditionPercentage;
				this.healthMultiplier = MathHelper.Clamp(value, 0f, float.PositiveInfinity);
				this.RecalculateConditionValues();
				this.condition = this.MaxCondition * prevConditionPercentage / 100f;
				this.RecalculateConditionValues();
			}
		}

		// Token: 0x17000644 RID: 1604
		// (get) Token: 0x06001915 RID: 6421 RVA: 0x000FD14E File Offset: 0x000FB34E
		// (set) Token: 0x06001916 RID: 6422 RVA: 0x000FD156 File Offset: 0x000FB356
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float MaxRepairConditionMultiplier
		{
			get
			{
				return this.maxRepairConditionMultiplier;
			}
			set
			{
				this.maxRepairConditionMultiplier = MathHelper.Clamp(value, 0f, float.PositiveInfinity);
				this.RecalculateConditionValues();
			}
		}

		// Token: 0x17000645 RID: 1605
		// (get) Token: 0x06001917 RID: 6423 RVA: 0x000FD174 File Offset: 0x000FB374
		// (set) Token: 0x06001918 RID: 6424 RVA: 0x000FD17C File Offset: 0x000FB37C
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool HasBeenInstantiatedOnce { get; set; }

		// Token: 0x17000646 RID: 1606
		// (get) Token: 0x06001919 RID: 6425 RVA: 0x000FD185 File Offset: 0x000FB385
		// (set) Token: 0x0600191A RID: 6426 RVA: 0x000FD18D File Offset: 0x000FB38D
		[Serialize(float.NaN, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public float Condition
		{
			get
			{
				return this.condition;
			}
			set
			{
				this.SetCondition(value, false, true);
			}
		}

		// Token: 0x17000647 RID: 1607
		// (get) Token: 0x0600191B RID: 6427 RVA: 0x000FD198 File Offset: 0x000FB398
		// (set) Token: 0x0600191C RID: 6428 RVA: 0x000FD1A0 File Offset: 0x000FB3A0
		private double ConditionLastUpdated { get; set; }

		// Token: 0x17000648 RID: 1608
		// (get) Token: 0x0600191D RID: 6429 RVA: 0x000FD1A9 File Offset: 0x000FB3A9
		// (set) Token: 0x0600191E RID: 6430 RVA: 0x000FD1B1 File Offset: 0x000FB3B1
		private float LastConditionChange { get; set; }

		// Token: 0x17000649 RID: 1609
		// (get) Token: 0x0600191F RID: 6431 RVA: 0x000FD1BA File Offset: 0x000FB3BA
		public bool ConditionIncreasedRecently
		{
			get
			{
				return Timing.TotalTime < this.ConditionLastUpdated + 1.0 && this.LastConditionChange > 0f;
			}
		}

		// Token: 0x1700064A RID: 1610
		// (get) Token: 0x06001920 RID: 6432 RVA: 0x000FD1E2 File Offset: 0x000FB3E2
		public float Health
		{
			get
			{
				return this.condition;
			}
		}

		// Token: 0x1700064B RID: 1611
		// (get) Token: 0x06001921 RID: 6433 RVA: 0x000FD1EC File Offset: 0x000FB3EC
		// (set) Token: 0x06001922 RID: 6434 RVA: 0x000FD21C File Offset: 0x000FB41C
		public bool Indestructible
		{
			get
			{
				bool? flag = this.indestructible;
				if (flag == null)
				{
					return this.Prefab.Indestructible;
				}
				return flag.GetValueOrDefault();
			}
			set
			{
				this.indestructible = new bool?(value);
			}
		}

		// Token: 0x1700064C RID: 1612
		// (get) Token: 0x06001923 RID: 6435 RVA: 0x000FD22A File Offset: 0x000FB42A
		// (set) Token: 0x06001924 RID: 6436 RVA: 0x000FD232 File Offset: 0x000FB432
		public bool AllowDeconstruct { get; set; }

		// Token: 0x1700064D RID: 1613
		// (get) Token: 0x06001925 RID: 6437 RVA: 0x000FD23C File Offset: 0x000FB43C
		// (set) Token: 0x06001926 RID: 6438 RVA: 0x000FD26C File Offset: 0x000FB46C
		public bool IsDangerous
		{
			get
			{
				bool? flag = this.isDangerous;
				if (flag == null)
				{
					return this.Prefab.IsDangerous;
				}
				return flag.GetValueOrDefault();
			}
			set
			{
				this.isDangerous = new bool?(value);
				if (!value)
				{
					Item._dangerousItems.Remove(this);
					return;
				}
				Item._dangerousItems.Add(this);
			}
		}

		// Token: 0x1700064E RID: 1614
		// (get) Token: 0x06001927 RID: 6439 RVA: 0x000FD296 File Offset: 0x000FB496
		// (set) Token: 0x06001928 RID: 6440 RVA: 0x000FD29E File Offset: 0x000FB49E
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "When enabled will prevent the item from taking damage from all sources", "", false)]
		public bool InvulnerableToDamage { get; set; }

		// Token: 0x1700064F RID: 1615
		// (get) Token: 0x06001929 RID: 6441 RVA: 0x000FD2A7 File Offset: 0x000FB4A7
		public bool Illegitimate
		{
			get
			{
				return !this.AllowStealing && this.SpawnedInCurrentOutpost;
			}
		}

		// Token: 0x17000650 RID: 1616
		// (get) Token: 0x0600192A RID: 6442 RVA: 0x000FD2B9 File Offset: 0x000FB4B9
		// (set) Token: 0x0600192B RID: 6443 RVA: 0x000FD2C1 File Offset: 0x000FB4C1
		public bool SpawnedInCurrentOutpost
		{
			get
			{
				return this.spawnedInCurrentOutpost;
			}
			set
			{
				if (!this.spawnedInCurrentOutpost && value)
				{
					GameSession gameSession = GameMain.GameSession;
					string text;
					if (gameSession == null)
					{
						text = null;
					}
					else
					{
						LevelData levelData = gameSession.LevelData;
						text = ((levelData != null) ? levelData.Seed : null);
					}
					this.OriginalOutpost = text;
				}
				this.spawnedInCurrentOutpost = value;
			}
		}

		// Token: 0x17000651 RID: 1617
		// (get) Token: 0x0600192C RID: 6444 RVA: 0x000FD2FA File Offset: 0x000FB4FA
		// (set) Token: 0x0600192D RID: 6445 RVA: 0x000FD311 File Offset: 0x000FB511
		[Serialize(true, IsPropertySaveable.Yes, "Determined by where/how the item originally spawned. If ItemPrefab.AllowStealing is true, stealing the item is always allowed.", "", true)]
		public bool AllowStealing
		{
			get
			{
				return this.allowStealing || this.Prefab.AllowStealingAlways;
			}
			set
			{
				this.allowStealing = value;
			}
		}

		// Token: 0x17000652 RID: 1618
		// (get) Token: 0x0600192E RID: 6446 RVA: 0x000FD31A File Offset: 0x000FB51A
		// (set) Token: 0x0600192F RID: 6447 RVA: 0x000FD324 File Offset: 0x000FB524
		[Serialize("", IsPropertySaveable.Yes, "", "", true)]
		public string OriginalOutpost
		{
			get
			{
				return this.originalOutpost;
			}
			set
			{
				this.originalOutpost = value;
				if (!string.IsNullOrEmpty(value))
				{
					GameSession gameSession = GameMain.GameSession;
					bool flag;
					if (gameSession == null)
					{
						flag = false;
					}
					else
					{
						LevelData levelData = gameSession.LevelData;
						flag = (((levelData != null) ? new LevelData.LevelType?(levelData.Type) : null).GetValueOrDefault() == LevelData.LevelType.Outpost);
					}
					if (flag)
					{
						GameSession gameSession2 = GameMain.GameSession;
						string a;
						if (gameSession2 == null)
						{
							a = null;
						}
						else
						{
							LevelData levelData2 = gameSession2.LevelData;
							a = ((levelData2 != null) ? levelData2.Seed : null);
						}
						if (a == value)
						{
							this.spawnedInCurrentOutpost = true;
						}
					}
				}
			}
		}

		// Token: 0x17000653 RID: 1619
		// (get) Token: 0x06001930 RID: 6448 RVA: 0x000FD3A3 File Offset: 0x000FB5A3
		// (set) Token: 0x06001931 RID: 6449 RVA: 0x000FD3B8 File Offset: 0x000FB5B8
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string Tags
		{
			get
			{
				return this.tags.ConvertToString(",");
			}
			set
			{
				this.tags.Clear();
				this.Prefab.Tags.ForEach(delegate(Identifier t)
				{
					this.tags.Add(t);
				});
				this.tags = this.tags.Union(value.ToIdentifiers(",")).ToHashSet<Identifier>();
			}
		}

		// Token: 0x17000654 RID: 1620
		// (get) Token: 0x06001932 RID: 6450 RVA: 0x000FD40D File Offset: 0x000FB60D
		// (set) Token: 0x06001933 RID: 6451 RVA: 0x000FD415 File Offset: 0x000FB615
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool FireProof { get; private set; }

		// Token: 0x17000655 RID: 1621
		// (get) Token: 0x06001934 RID: 6452 RVA: 0x000FD41E File Offset: 0x000FB61E
		// (set) Token: 0x06001935 RID: 6453 RVA: 0x000FD428 File Offset: 0x000FB628
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool WaterProof
		{
			get
			{
				return this.waterProof;
			}
			private set
			{
				if (this.waterProof == value)
				{
					return;
				}
				this.waterProof = value;
				foreach (Item containedItem in this.ContainedItems)
				{
					containedItem.RefreshInWaterProofContainer();
				}
			}
		}

		// Token: 0x17000656 RID: 1622
		// (get) Token: 0x06001936 RID: 6454 RVA: 0x000FD488 File Offset: 0x000FB688
		public bool UseInHealthInterface
		{
			get
			{
				return this.Prefab.UseInHealthInterface;
			}
		}

		// Token: 0x17000657 RID: 1623
		// (get) Token: 0x06001937 RID: 6455 RVA: 0x000FD495 File Offset: 0x000FB695
		// (set) Token: 0x06001938 RID: 6456 RVA: 0x000FD4A8 File Offset: 0x000FB6A8
		public int Quality
		{
			get
			{
				Quality quality = this.qualityComponent;
				if (quality == null)
				{
					return 0;
				}
				return quality.QualityLevel;
			}
			set
			{
				if (this.qualityComponent != null)
				{
					this.qualityComponent.QualityLevel = value;
				}
			}
		}

		// Token: 0x17000658 RID: 1624
		// (get) Token: 0x06001939 RID: 6457 RVA: 0x000FD4BE File Offset: 0x000FB6BE
		public bool InWater
		{
			get
			{
				if (this.body != null && this.body.Enabled)
				{
					return this.inWater;
				}
				if (this.hasInWaterStatusEffects)
				{
					return this.inWater;
				}
				return this.IsInWater();
			}
		}

		// Token: 0x17000659 RID: 1625
		// (get) Token: 0x0600193A RID: 6458 RVA: 0x000FD4F1 File Offset: 0x000FB6F1
		// (set) Token: 0x0600193B RID: 6459 RVA: 0x000FD4F9 File Offset: 0x000FB6F9
		public List<Connection> LastSentSignalRecipients { get; private set; } = new List<Connection>(20);

		// Token: 0x1700065A RID: 1626
		// (get) Token: 0x0600193C RID: 6460 RVA: 0x000FD502 File Offset: 0x000FB702
		public ContentPath ConfigFilePath
		{
			get
			{
				return this.Prefab.ContentFile.Path;
			}
		}

		// Token: 0x1700065B RID: 1627
		// (get) Token: 0x0600193D RID: 6461 RVA: 0x000FD514 File Offset: 0x000FB714
		public IEnumerable<InvSlotType> AllowedSlots
		{
			get
			{
				return this.allowedSlots;
			}
		}

		// Token: 0x1700065C RID: 1628
		// (get) Token: 0x0600193E RID: 6462 RVA: 0x000FD51C File Offset: 0x000FB71C
		public List<Connection> Connections
		{
			get
			{
				ConnectionPanel panel = this.GetComponent<ConnectionPanel>();
				if (panel == null)
				{
					return null;
				}
				return panel.Connections;
			}
		}

		// Token: 0x1700065D RID: 1629
		// (get) Token: 0x0600193F RID: 6463 RVA: 0x000FD53C File Offset: 0x000FB73C
		public IEnumerable<Item> ContainedItems
		{
			get
			{
				Item.<get_ContainedItems>d__425 <get_ContainedItems>d__ = new Item.<get_ContainedItems>d__425(-2);
				<get_ContainedItems>d__.<>4__this = this;
				return <get_ContainedItems>d__;
			}
		}

		// Token: 0x1700065E RID: 1630
		// (get) Token: 0x06001940 RID: 6464 RVA: 0x000FD559 File Offset: 0x000FB759
		public ItemInventory OwnInventory
		{
			get
			{
				return this.ownInventory;
			}
		}

		// Token: 0x1700065F RID: 1631
		// (get) Token: 0x06001941 RID: 6465 RVA: 0x000FD561 File Offset: 0x000FB761
		// (set) Token: 0x06001942 RID: 6466 RVA: 0x000FD569 File Offset: 0x000FB769
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Enable if you want to display the item HUD side by side with another item's HUD, when linked together. Disclaimer: It's possible or even likely that the views block each other, if they were not designed to be viewed together!", "", false)]
		public bool DisplaySideBySideWhenLinked { get; set; }

		// Token: 0x17000660 RID: 1632
		// (get) Token: 0x06001943 RID: 6467 RVA: 0x000FD572 File Offset: 0x000FB772
		public List<Repairable> Repairables
		{
			get
			{
				return this.repairables;
			}
		}

		// Token: 0x17000661 RID: 1633
		// (get) Token: 0x06001944 RID: 6468 RVA: 0x000FD57A File Offset: 0x000FB77A
		public List<ItemComponent> Components
		{
			get
			{
				return this.components;
			}
		}

		// Token: 0x17000662 RID: 1634
		// (get) Token: 0x06001945 RID: 6469 RVA: 0x000FD582 File Offset: 0x000FB782
		public override bool Linkable
		{
			get
			{
				return this.Prefab.Linkable;
			}
		}

		// Token: 0x17000663 RID: 1635
		// (get) Token: 0x06001946 RID: 6470 RVA: 0x000FD58F File Offset: 0x000FB78F
		public float WorldPositionX
		{
			get
			{
				return this.WorldPosition.X;
			}
		}

		// Token: 0x17000664 RID: 1636
		// (get) Token: 0x06001947 RID: 6471 RVA: 0x000FD59C File Offset: 0x000FB79C
		public float WorldPositionY
		{
			get
			{
				return this.WorldPosition.Y;
			}
		}

		// Token: 0x17000665 RID: 1637
		// (get) Token: 0x06001948 RID: 6472 RVA: 0x000FD5A9 File Offset: 0x000FB7A9
		// (set) Token: 0x06001949 RID: 6473 RVA: 0x000FD5B6 File Offset: 0x000FB7B6
		public float PositionX
		{
			get
			{
				return this.Position.X;
			}
			private set
			{
				this.Move(new Vector2(value * this.Scale, 0f), true);
			}
		}

		// Token: 0x17000666 RID: 1638
		// (get) Token: 0x0600194A RID: 6474 RVA: 0x000FD5D1 File Offset: 0x000FB7D1
		// (set) Token: 0x0600194B RID: 6475 RVA: 0x000FD5DE File Offset: 0x000FB7DE
		public float PositionY
		{
			get
			{
				return this.Position.Y;
			}
			private set
			{
				this.Move(new Vector2(0f, value * this.Scale), true);
			}
		}

		// Token: 0x17000667 RID: 1639
		// (get) Token: 0x0600194C RID: 6476 RVA: 0x000FD5F9 File Offset: 0x000FB7F9
		// (set) Token: 0x0600194D RID: 6477 RVA: 0x000FD601 File Offset: 0x000FB801
		public BallastFloraBranch Infector { get; set; }

		// Token: 0x17000668 RID: 1640
		// (get) Token: 0x0600194E RID: 6478 RVA: 0x000FD60A File Offset: 0x000FB80A
		// (set) Token: 0x0600194F RID: 6479 RVA: 0x000FD612 File Offset: 0x000FB812
		public ItemPrefab PendingItemSwap { get; set; }

		// Token: 0x06001950 RID: 6480 RVA: 0x000FD61C File Offset: 0x000FB81C
		public override string ToString()
		{
			return (this.Name.IsNullOrEmpty() ? this.Prefab.Identifier : this.Name).ToString() + " (ID: " + this.ID.ToString() + ")";
		}

		// Token: 0x17000669 RID: 1641
		// (get) Token: 0x06001951 RID: 6481 RVA: 0x000FD676 File Offset: 0x000FB876
		public IReadOnlyList<ISerializableEntity> AllPropertyObjects
		{
			get
			{
				return this.allPropertyObjects;
			}
		}

		// Token: 0x06001952 RID: 6482 RVA: 0x000FD67E File Offset: 0x000FB87E
		public bool IgnoreByAI(Character character)
		{
			return this.HasTag(Barotrauma.Tags.IgnoredByAI) || (this.OrderedToBeIgnored && character.IsOnPlayerTeam);
		}

		// Token: 0x1700066A RID: 1642
		// (get) Token: 0x06001953 RID: 6483 RVA: 0x000FD69F File Offset: 0x000FB89F
		// (set) Token: 0x06001954 RID: 6484 RVA: 0x000FD6A7 File Offset: 0x000FB8A7
		public bool OrderedToBeIgnored { get; set; }

		// Token: 0x1700066B RID: 1643
		// (get) Token: 0x06001955 RID: 6485 RVA: 0x000FD6B0 File Offset: 0x000FB8B0
		public bool HasBallastFloraInHull
		{
			get
			{
				Hull hull = this.CurrentHull;
				return ((hull != null) ? hull.BallastFlora : null) != null;
			}
		}

		// Token: 0x1700066C RID: 1644
		// (get) Token: 0x06001956 RID: 6486 RVA: 0x000FD6C7 File Offset: 0x000FB8C7
		public bool IsClaimedByBallastFlora
		{
			get
			{
				Hull hull = this.CurrentHull;
				return ((hull != null) ? hull.BallastFlora : null) != null && this.CurrentHull.BallastFlora.ClaimedTargets.Contains(this);
			}
		}

		// Token: 0x1700066D RID: 1645
		// (get) Token: 0x06001957 RID: 6487 RVA: 0x000FD6F8 File Offset: 0x000FB8F8
		public bool InPlayerSubmarine
		{
			get
			{
				Submarine submarine = base.Submarine;
				SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
				return submarineInfo != null && submarineInfo.IsPlayer;
			}
		}

		// Token: 0x1700066E RID: 1646
		// (get) Token: 0x06001958 RID: 6488 RVA: 0x000FD724 File Offset: 0x000FB924
		public bool InBeaconStation
		{
			get
			{
				Submarine submarine = base.Submarine;
				SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
				return submarineInfo != null && submarineInfo.Type == SubmarineType.BeaconStation;
			}
		}

		// Token: 0x1700066F RID: 1647
		// (get) Token: 0x06001959 RID: 6489 RVA: 0x000FD752 File Offset: 0x000FB952
		public bool IsLadder { get; }

		// Token: 0x17000670 RID: 1648
		// (get) Token: 0x0600195A RID: 6490 RVA: 0x000FD75A File Offset: 0x000FB95A
		public bool IsSecondaryItem { get; }

		// Token: 0x17000671 RID: 1649
		// (get) Token: 0x0600195B RID: 6491 RVA: 0x000FD762 File Offset: 0x000FB962
		public ItemStatManager StatManager
		{
			get
			{
				if (this.statManager == null)
				{
					this.statManager = new ItemStatManager(this);
				}
				return this.statManager;
			}
		}

		// Token: 0x17000672 RID: 1650
		// (get) Token: 0x0600195C RID: 6492 RVA: 0x000FD77E File Offset: 0x000FB97E
		// (set) Token: 0x0600195D RID: 6493 RVA: 0x000FD786 File Offset: 0x000FB986
		public float LastEatenTime { get; set; }

		// Token: 0x0600195E RID: 6494 RVA: 0x000FD790 File Offset: 0x000FB990
		public Item(ItemPrefab itemPrefab, Vector2 position, Submarine submarine, ushort id = 0, bool callOnItemLoaded = true) : this(new Rectangle((int)(position.X - itemPrefab.Sprite.size.X / 2f * itemPrefab.Scale), (int)(position.Y + itemPrefab.Sprite.size.Y / 2f * itemPrefab.Scale), (int)(itemPrefab.Sprite.size.X * itemPrefab.Scale), (int)(itemPrefab.Sprite.size.Y * itemPrefab.Scale)), itemPrefab, submarine, callOnItemLoaded, id)
		{
		}

		// Token: 0x0600195F RID: 6495 RVA: 0x000FD828 File Offset: 0x000FBA28
		public Item(Rectangle newRect, ItemPrefab itemPrefab, Submarine submarine, bool callOnItemLoaded = true, ushort id = 0) : base(itemPrefab, submarine, id)
		{
			this.spriteColor = this.Prefab.SpriteColor;
			this.components = new List<ItemComponent>();
			this.drawableComponents = new List<IDrawableComponent>();
			this.hasComponentsToDraw = false;
			this.tags = new HashSet<Identifier>();
			this.repairables = new List<Repairable>();
			this.defaultRect = newRect;
			this.rect = newRect;
			this.condition = (this.MaxCondition = (this.prevCondition = this.Prefab.Health));
			this.ConditionPercentage = 100f;
			this.lastSentCondition = this.condition;
			this.AllowDeconstruct = itemPrefab.AllowDeconstruct;
			this.allPropertyObjects.Add(this);
			ContentXElement element = itemPrefab.ConfigElement;
			ContentXElement contentXElement = null;
			if (element == contentXElement)
			{
				return;
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (submarine == null || !submarine.Loading)
			{
				this.FindHull();
			}
			this.SetActiveSprite();
			ContentXElement bodyElement = null;
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 4:
						if (text == "body")
						{
							bodyElement = subElement;
							float density = subElement.GetAttributeFloat("density", 10f);
							float minDensity = subElement.GetAttributeFloat("mindensity", density);
							float maxDensity = subElement.GetAttributeFloat("maxdensity", density);
							if (minDensity < maxDensity)
							{
								Random rand = new Random((int)this.ID);
								density = MathHelper.Lerp(minDensity, maxDensity, (float)rand.NextDouble());
							}
							string collisionCategoryStr = subElement.GetAttributeString("collisioncategory", null);
							Category collisionCategory = Category.Cat5;
							Category collidesWith = Category.Cat1 | Category.Cat3 | Category.Cat8 | Category.Cat9;
							if ((this.Prefab.DamagedByProjectiles || this.Prefab.DamagedByMeleeWeapons || this.Prefab.DamagedByRepairTools) && this.Condition > 0f)
							{
								collisionCategory = Category.Cat2;
								collidesWith |= Category.Cat7;
							}
							if (collisionCategoryStr != null)
							{
								Category cat;
								if (!Physics.TryParseCollisionCategory(collisionCategoryStr, out cat))
								{
									DebugConsole.ThrowError(string.Concat(new string[]
									{
										"Invalid collision category in item \"",
										this.Name,
										"\" (",
										collisionCategoryStr,
										")"
									}), null, element.ContentPackage, false, false);
								}
								else
								{
									collisionCategory = cat;
									if (cat.HasFlag(Category.Cat2))
									{
										collisionCategory |= Category.Cat7;
									}
								}
							}
							this.body = new PhysicsBody(subElement, ConvertUnits.ToSimUnits(this.Position), this.Scale, new float?(density), collisionCategory, collidesWith, false);
							this.body.FarseerBody.AngularDamping = subElement.GetAttributeFloat("angulardamping", 0.2f);
							this.body.FarseerBody.LinearDamping = subElement.GetAttributeFloat("lineardamping", 0.1f);
							this.body.UserData = this;
							continue;
						}
						break;
					case 5:
						if (text == "price")
						{
							continue;
						}
						break;
					case 6:
						if (text == "sprite")
						{
							continue;
						}
						break;
					case 7:
					{
						char c2 = text[0];
						if (c2 != 't')
						{
							if (c2 == 'u')
							{
								if (text == "upgrade")
								{
									continue;
								}
							}
						}
						else if (text == "trigger")
						{
							continue;
						}
						break;
					}
					case 8:
						if (text == "aitarget")
						{
							this.aiTarget = new AITarget(this, subElement);
							continue;
						}
						break;
					case 9:
						if (text == "fabricate")
						{
							continue;
						}
						break;
					case 10:
					{
						char c2 = text[0];
						if (c2 != 'f')
						{
							if (c2 == 's')
							{
								if (text == "staticbody")
								{
									this.StaticBodyConfig = subElement;
									continue;
								}
							}
						}
						else if (text == "fabricable")
						{
							continue;
						}
						break;
					}
					case 11:
					{
						char c2 = text[0];
						if (c2 != 'd')
						{
							if (c2 == 'm')
							{
								if (text == "minimapicon")
								{
									continue;
								}
							}
						}
						else if (text == "deconstruct")
						{
							continue;
						}
						break;
					}
					case 12:
						if (text == "brokensprite")
						{
							continue;
						}
						break;
					case 13:
					{
						char c2 = text[0];
						if (c2 != 'i')
						{
							if (c2 != 's')
							{
								if (c2 == 'u')
								{
									if (text == "upgrademodule")
									{
										continue;
									}
								}
							}
							else if (text == "swappableitem")
							{
								continue;
							}
						}
						else if (text == "inventoryicon")
						{
							continue;
						}
						break;
					}
					case 14:
					{
						char c2 = text[0];
						if (c2 != 'f')
						{
							if (c2 == 'i')
							{
								if (text == "infectedsprite")
								{
									continue;
								}
							}
						}
						else if (text == "fabricableitem")
						{
							continue;
						}
						break;
					}
					case 15:
					{
						char c2 = text[0];
						if (c2 != 'c')
						{
							if (c2 != 'l')
							{
								if (c2 == 'u')
								{
									if (text == "upgradeoverride")
									{
										continue;
									}
								}
							}
							else if (text == "levelcommonness")
							{
								continue;
							}
						}
						else if (text == "containedsprite")
						{
							continue;
						}
						break;
					}
					case 16:
						if (text == "decorativesprite")
						{
							continue;
						}
						break;
					case 17:
						if (text == "suitabletreatment")
						{
							continue;
						}
						break;
					case 18:
						if (text == "preferredcontainer")
						{
							continue;
						}
						break;
					case 20:
					{
						char c2 = text[0];
						if (c2 != 's')
						{
							if (c2 == 'u')
							{
								if (text == "upgradepreviewsprite")
								{
									continue;
								}
							}
						}
						else if (text == "skillrequirementhint")
						{
							continue;
						}
						break;
					}
					case 21:
						if (text == "damagedinfectedsprite")
						{
							continue;
						}
						break;
					}
				}
				ItemComponent ic4 = ItemComponent.Load(subElement, this, true);
				if (ic4 != null)
				{
					this.AddComponent(ic4);
				}
			}
			foreach (ItemComponent ic2 in this.components)
			{
				Pickable pickable = ic2 as Pickable;
				if (pickable != null)
				{
					foreach (InvSlotType allowedSlot in pickable.AllowedSlots)
					{
						this.allowedSlots.Add(allowedSlot);
					}
				}
				Repairable repairable = ic2 as Repairable;
				if (repairable != null)
				{
					this.repairables.Add(repairable);
				}
				if (ic2 is IDrawableComponent && ic2.Drawable)
				{
					this.drawableComponents.Add(ic2 as IDrawableComponent);
					this.hasComponentsToDraw = true;
				}
				if (ic2.statusEffectLists != null && !ic2.InheritStatusEffects)
				{
					if (this.statusEffectLists == null)
					{
						this.statusEffectLists = new Dictionary<ActionType, List<StatusEffect>>();
					}
					foreach (List<StatusEffect> componentEffectList in ic2.statusEffectLists.Values)
					{
						ActionType actionType = componentEffectList.First<StatusEffect>().type;
						List<StatusEffect> statusEffectList;
						if (!this.statusEffectLists.TryGetValue(actionType, out statusEffectList))
						{
							statusEffectList = new List<StatusEffect>();
							this.statusEffectLists.Add(actionType, statusEffectList);
							this.hasStatusEffectsOfType[(int)actionType] = true;
						}
						foreach (StatusEffect effect in componentEffectList)
						{
							statusEffectList.Add(effect);
						}
					}
				}
			}
			this.hasInWaterStatusEffects = this.hasStatusEffectsOfType[12];
			this.hasNotInWaterStatusEffects = this.hasStatusEffectsOfType[13];
			if (this.body != null)
			{
				this.body.Submarine = submarine;
				this.originalWaterDragCoefficient = bodyElement.GetAttributeFloat("waterdragcoefficient", 5f);
			}
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel != null)
			{
				this.connections = new Dictionary<string, Connection>();
				foreach (Connection c3 in connectionPanel.Connections)
				{
					if (!this.connections.ContainsKey(c3.Name))
					{
						this.connections.Add(c3.Name, c3);
					}
				}
			}
			if (this.body != null)
			{
				this.body.FarseerBody.OnCollision += this.OnCollision;
			}
			ItemContainer itemContainer = this.GetComponent<ItemContainer>();
			if (itemContainer != null)
			{
				this.ownInventory = itemContainer.Inventory;
			}
			this.OwnInventories = (from ic in this.GetComponents<ItemContainer>()
			select ic.Inventory).ToImmutableArray<ItemInventory>();
			this.qualityComponent = this.GetComponent<Quality>();
			this.IsLadder = (this.GetComponent<Ladder>() != null);
			int num;
			if (!this.IsLadder)
			{
				Controller component = this.GetComponent<Controller>();
				num = ((component != null && component.IsSecondaryItem) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			this.IsSecondaryItem = num;
			this.InitProjSpecific();
			if (callOnItemLoaded)
			{
				foreach (ItemComponent ic3 in this.components)
				{
					ic3.OnItemLoaded();
				}
			}
			IEnumerable<ItemComponent> holdables = from c in this.components
			where c is Holdable
			select c;
			if (holdables.Count<ItemComponent>() > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Item ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(" has multiple ");
				defaultInterpolatedStringHandler.AppendFormatted("Holdable");
				defaultInterpolatedStringHandler.AppendLiteral(" components (");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(", ", from h in holdables
				select h.GetType().Name));
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), this.Prefab.ContentPackage);
			}
			base.InsertToList();
			Item.ItemList.Add(this);
			if (this.Prefab.IsDangerous)
			{
				Item._dangerousItems.Add(this);
			}
			if (this.Repairables.Any<Repairable>())
			{
				Item._repairableItems.Add(this);
			}
			if (this.Prefab.SonarSize > 0f)
			{
				Item._sonarVisibleItems.Add(this);
			}
			if (this.Prefab.IsAITurretTarget)
			{
				Item._turretTargetItems.Add(this);
			}
			if (this.Prefab.Tags.Contains(Barotrauma.Tags.ChairItem))
			{
				Item._chairItems.Add(this);
			}
			this.CheckCleanable();
			DebugConsole.Log(string.Concat(new string[]
			{
				"Created ",
				this.Name,
				" (",
				this.ID.ToString(),
				")"
			}));
			if (this.Components.Any((ItemComponent ic) => ic is Wire))
			{
				if (this.Components.All((ItemComponent ic) => ic is Wire || ic is Holdable))
				{
					this.isWire = true;
				}
			}
			if (this.HasTag(Barotrauma.Tags.LogicItem))
			{
				this.isLogic = true;
			}
			this.ApplyStatusEffects(ActionType.OnSpawn, 1f, null, null, null, false, null);
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign != null)
			{
				if (this.HasTag(Barotrauma.Tags.OxygenSource))
				{
					this.conditionMultiplierCampaign *= campaign.Settings.OxygenMultiplier;
				}
				if (this.HasTag(Barotrauma.Tags.ReactorFuel))
				{
					this.conditionMultiplierCampaign *= campaign.Settings.FuelMultiplier;
				}
			}
			this.condition *= this.conditionMultiplierCampaign;
			this.RecalculateConditionValues();
			if (callOnItemLoaded)
			{
				this.FullyInitialized = true;
			}
			Submarine.ForceVisibilityRecheck();
			this.HasBeenInstantiatedOnce = true;
		}

		// Token: 0x06001960 RID: 6496 RVA: 0x000FE710 File Offset: 0x000FC910
		private void InitProjSpecific()
		{
			this.InitSpriteStates();
		}

		// Token: 0x06001961 RID: 6497 RVA: 0x000FE718 File Offset: 0x000FC918
		public bool IsContainerPreferred(ItemContainer container, out bool isPreferencesDefined, out bool isSecondary, bool requireConditionRestriction = false)
		{
			return this.Prefab.IsContainerPreferred(this, container, out isPreferencesDefined, out isSecondary, requireConditionRestriction, false);
		}

		// Token: 0x06001962 RID: 6498 RVA: 0x000FE72C File Offset: 0x000FC92C
		public override MapEntity Clone()
		{
			Item clone = new Item(this.rect, this.Prefab, base.Submarine, false, 0)
			{
				defaultRect = this.defaultRect
			};
			foreach (KeyValuePair<Identifier, SerializableProperty> property in this.SerializableProperties)
			{
				if (!property.Value.Attributes.OfType<Serialize>().None(null))
				{
					clone.SerializableProperties[property.Key].TrySetValue(clone, property.Value.GetValue(this));
				}
			}
			if (this.components.Count != clone.components.Count)
			{
				string errorMsg = "Error while cloning item \"" + this.Name + "\" - clone does not have the same number of components. ";
				errorMsg = errorMsg + "Original components: " + string.Join(", ", from c in this.components
				select c.GetType().ToString());
				errorMsg = errorMsg + ", cloned components: " + string.Join(", ", from c in clone.components
				select c.GetType().ToString());
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Item.Clone:" + this.Name, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			}
			int i = 0;
			while (i < this.components.Count && i < clone.components.Count)
			{
				foreach (KeyValuePair<Identifier, SerializableProperty> property2 in from s in this.components[i].SerializableProperties
				orderby s.Key
				select s)
				{
					if (!property2.Value.Attributes.OfType<Serialize>().None(null))
					{
						clone.components[i].SerializableProperties[property2.Key].TrySetValue(clone.components[i], property2.Value.GetValue(this.components[i]));
					}
				}
				foreach (KeyValuePair<RelatedItem.RelationType, List<RelatedItem>> kvp in this.components[i].RequiredItems)
				{
					for (int j = 0; j < kvp.Value.Count; j++)
					{
						if (clone.components[i].RequiredItems.ContainsKey(kvp.Key) && clone.components[i].RequiredItems[kvp.Key].Count > j)
						{
							clone.components[i].RequiredItems[kvp.Key][j].JoinedIdentifiers = kvp.Value[j].JoinedIdentifiers;
						}
					}
				}
				i++;
			}
			if (base.FlippedX)
			{
				clone.FlipX(false, false);
			}
			if (base.FlippedY)
			{
				clone.FlipY(false, false);
			}
			clone.Rotation = this.Rotation;
			foreach (ItemComponent component in clone.components)
			{
				component.OnItemLoaded();
			}
			Dictionary<ushort, Item> clonedContainedItems = new Dictionary<ushort, Item>();
			int k = 0;
			while (k < this.components.Count && k < clone.components.Count)
			{
				ItemComponent component2 = this.components[k];
				ItemComponent cloneComp = clone.components[k];
				ItemContainer origInv = component2 as ItemContainer;
				if (origInv != null)
				{
					ItemContainer cloneInv = cloneComp as ItemContainer;
					if (cloneInv != null)
					{
						foreach (Item containedItem in origInv.Inventory.AllItems)
						{
							Item containedClone = (Item)containedItem.Clone();
							cloneInv.Inventory.TryPutItem(containedClone, null, null, true, false, true);
							clonedContainedItems.Add(containedItem.ID, containedClone);
						}
					}
				}
				k++;
			}
			int l = 0;
			while (l < this.components.Count && l < clone.components.Count)
			{
				ItemComponent component3 = this.components[l];
				ItemComponent cloneComp2 = clone.components[l];
				if (component3.GetType() == cloneComp2.GetType())
				{
					cloneComp2.Clone(component3);
				}
				CircuitBox origBox = component3 as CircuitBox;
				if (origBox != null)
				{
					CircuitBox cloneBox = cloneComp2 as CircuitBox;
					if (cloneBox != null)
					{
						cloneBox.CloneFrom(origBox, clonedContainedItems);
					}
				}
				l++;
			}
			clone.FullyInitialized = true;
			return clone;
		}

		// Token: 0x06001963 RID: 6499 RVA: 0x000FEC8C File Offset: 0x000FCE8C
		public void AddComponent(ItemComponent component)
		{
			Item.<>c__DisplayClass494_0 CS$<>8__locals1 = new Item.<>c__DisplayClass494_0();
			CS$<>8__locals1.component = component;
			CS$<>8__locals1.<>4__this = this;
			this.allPropertyObjects.Add(CS$<>8__locals1.component);
			this.components.Add(CS$<>8__locals1.component);
			if (CS$<>8__locals1.component.IsActive || CS$<>8__locals1.component.UpdateWhenInactive || CS$<>8__locals1.component.Parent != null || (CS$<>8__locals1.component.IsActiveConditionals != null && CS$<>8__locals1.component.IsActiveConditionals.Any<PropertyConditional>()))
			{
				this.updateableComponents.Add(CS$<>8__locals1.component);
			}
			ItemComponent component2 = CS$<>8__locals1.component;
			component2.OnActiveStateChanged = (Action<bool>)Delegate.Combine(component2.OnActiveStateChanged, new Action<bool>(delegate(bool isActive)
			{
				bool needsSoundUpdate = CS$<>8__locals1.component.NeedsSoundUpdate();
				if (!isActive && !CS$<>8__locals1.component.UpdateWhenInactive && !needsSoundUpdate && CS$<>8__locals1.component.Parent == null && (CS$<>8__locals1.component.IsActiveConditionals == null || !CS$<>8__locals1.component.IsActiveConditionals.Any<PropertyConditional>()))
				{
					if (CS$<>8__locals1.<>4__this.updateableComponents.Contains(CS$<>8__locals1.component))
					{
						CS$<>8__locals1.<>4__this.updateableComponents.Remove(CS$<>8__locals1.component);
						return;
					}
				}
				else if (!CS$<>8__locals1.<>4__this.updateableComponents.Contains(CS$<>8__locals1.component))
				{
					CS$<>8__locals1.<>4__this.updateableComponents.Add(CS$<>8__locals1.component);
					CS$<>8__locals1.<>4__this.IsActive = true;
				}
			}));
			Type type = CS$<>8__locals1.component.GetType();
			CS$<>8__locals1.<AddComponent>g__CacheComponent|0(type);
			Type baseType = type.BaseType;
			while (baseType != null)
			{
				CS$<>8__locals1.<AddComponent>g__CacheComponent|0(baseType);
				baseType = baseType.BaseType;
			}
		}

		// Token: 0x06001964 RID: 6500 RVA: 0x000FED80 File Offset: 0x000FCF80
		public void EnableDrawableComponent(IDrawableComponent drawable)
		{
			if (!this.drawableComponents.Contains(drawable))
			{
				this.drawableComponents.Add(drawable);
				this.hasComponentsToDraw = true;
				Submarine.ForceVisibilityRecheck();
				this.cachedVisibleExtents = null;
			}
		}

		// Token: 0x06001965 RID: 6501 RVA: 0x000FEDB4 File Offset: 0x000FCFB4
		public void DisableDrawableComponent(IDrawableComponent drawable)
		{
			if (this.drawableComponents.Contains(drawable))
			{
				this.drawableComponents.Remove(drawable);
				this.hasComponentsToDraw = (this.drawableComponents.Count > 0);
				this.cachedVisibleExtents = null;
			}
		}

		// Token: 0x06001966 RID: 6502 RVA: 0x000FEDF1 File Offset: 0x000FCFF1
		public int GetComponentIndex(ItemComponent component)
		{
			return this.components.IndexOf(component);
		}

		// Token: 0x06001967 RID: 6503 RVA: 0x000FEE00 File Offset: 0x000FD000
		public T GetComponent<T>() where T : ItemComponent
		{
			List<ItemComponent> matchingComponents;
			if (this.componentsByType.TryGetValue(typeof(T), out matchingComponents))
			{
				return (T)((object)matchingComponents.First<ItemComponent>());
			}
			return default(T);
		}

		// Token: 0x06001968 RID: 6504 RVA: 0x000FEE3C File Offset: 0x000FD03C
		public IEnumerable<T> GetComponents<T>()
		{
			if (typeof(T) == typeof(ItemComponent))
			{
				return this.components.Cast<T>();
			}
			List<ItemComponent> matchingComponents;
			if (this.componentsByType.TryGetValue(typeof(T), out matchingComponents))
			{
				return matchingComponents.Cast<T>();
			}
			return Enumerable.Empty<T>();
		}

		// Token: 0x06001969 RID: 6505 RVA: 0x000FEE95 File Offset: 0x000FD095
		public float GetQualityModifier(Quality.StatType statType)
		{
			Quality component = this.GetComponent<Quality>();
			if (component == null)
			{
				return 0f;
			}
			return component.GetValue(statType);
		}

		// Token: 0x0600196A RID: 6506 RVA: 0x000FEEAD File Offset: 0x000FD0AD
		public void RemoveContained(Item contained)
		{
			ItemInventory itemInventory = this.ownInventory;
			if (itemInventory != null)
			{
				itemInventory.RemoveItem(contained);
			}
			contained.Container = null;
		}

		// Token: 0x0600196B RID: 6507 RVA: 0x000FEEC8 File Offset: 0x000FD0C8
		public void SetTransform(Vector2 simPosition, float rotation, bool findNewHull = true, bool setPrevTransform = true, Submarine forceSubmarine = null)
		{
			if (!MathUtils.IsValid(simPosition))
			{
				string[] array = new string[6];
				array[0] = "Attempted to move the item ";
				array[1] = this.Name;
				array[2] = " to an invalid position (";
				int num = 3;
				Vector2 vector = simPosition;
				array[num] = vector.ToString();
				array[4] = ")\n";
				array[5] = Environment.StackTrace.CleanupStackTrace();
				string errorMsg = string.Concat(array);
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Item.SetPosition:InvalidPosition" + this.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (this.body != null)
			{
				this.body.SetTransformIgnoreContacts(simPosition, rotation, setPrevTransform);
			}
			Vector2 displayPos = ConvertUnits.ToDisplayUnits(simPosition);
			this.rect.X = (int)MathF.Round(displayPos.X - (float)this.rect.Width / 2f);
			this.rect.Y = (int)MathF.Round(displayPos.Y + (float)this.rect.Height / 2f);
			if (findNewHull)
			{
				this.FindHull();
			}
			if (forceSubmarine != null)
			{
				base.Submarine = forceSubmarine;
			}
		}

		// Token: 0x0600196C RID: 6508 RVA: 0x000FEFDC File Offset: 0x000FD1DC
		public bool AllowDroppingOnSwapWith(Item otherItem)
		{
			if (!this.Prefab.AllowDroppingOnSwap || otherItem == null)
			{
				return false;
			}
			if (this.Prefab.AllowDroppingOnSwapWith.Any<Identifier>())
			{
				foreach (Identifier tagOrIdentifier in this.Prefab.AllowDroppingOnSwapWith)
				{
					if (otherItem.Prefab.Identifier == tagOrIdentifier)
					{
						return true;
					}
					if (otherItem.HasTag(tagOrIdentifier))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		// Token: 0x0600196D RID: 6509 RVA: 0x000FF07C File Offset: 0x000FD27C
		public void SetActiveSprite()
		{
			this.SetActiveSpriteProjSpecific();
		}

		// Token: 0x0600196E RID: 6510 RVA: 0x000FF084 File Offset: 0x000FD284
		private void SetActiveSpriteProjSpecific()
		{
			this.activeSprite = this.Prefab.Sprite;
			this.activeContainedSprite = null;
			Holdable holdable = this.GetComponent<Holdable>();
			if (holdable != null && holdable.Attached)
			{
				foreach (ContainedItemSprite containedSprite in this.Prefab.ContainedSprites)
				{
					if (containedSprite.UseWhenAttached)
					{
						this.activeContainedSprite = containedSprite;
						this.activeSprite = containedSprite.Sprite;
						this.UpdateSpriteStates(0f);
						return;
					}
				}
			}
			if (this.Container != null)
			{
				foreach (ContainedItemSprite containedSprite2 in this.Prefab.ContainedSprites)
				{
					if (containedSprite2.MatchesContainer(this.Container))
					{
						this.activeContainedSprite = containedSprite2;
						this.activeSprite = containedSprite2.Sprite;
						this.UpdateSpriteStates(0f);
						return;
					}
				}
			}
			float displayCondition = this.FakeBroken ? 0f : this.ConditionPercentageRelativeToDefaultMaxCondition;
			for (int i = 0; i < this.Prefab.BrokenSprites.Length; i++)
			{
				if (!this.Prefab.BrokenSprites[i].FadeIn)
				{
					float minCondition = (i > 0) ? this.Prefab.BrokenSprites[i - i].MaxConditionPercentage : 0f;
					if (displayCondition <= minCondition || displayCondition <= this.Prefab.BrokenSprites[i].MaxConditionPercentage)
					{
						this.activeSprite = this.Prefab.BrokenSprites[i].Sprite;
						return;
					}
				}
			}
		}

		// Token: 0x0600196F RID: 6511 RVA: 0x000FF23C File Offset: 0x000FD43C
		public void CheckCleanable()
		{
			Pickable pickable = this.GetComponent<Pickable>();
			if (pickable != null && !pickable.IsAttached && this.Prefab.PreferredContainers.Any<PreferredContainer>() && (this.container == null || this.container.HasTag(Barotrauma.Tags.AllowCleanup)))
			{
				if (!Item._cleanableItems.Contains(this))
				{
					Item._cleanableItems.Add(this);
					return;
				}
			}
			else
			{
				Item._cleanableItems.Remove(this);
			}
		}

		// Token: 0x06001970 RID: 6512 RVA: 0x000FF2AC File Offset: 0x000FD4AC
		public override void Move(Vector2 amount, bool ignoreContacts = true)
		{
			if (!MathUtils.IsValid(amount))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to move an item by an invalid amount (");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(amount);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			base.Move(amount, ignoreContacts);
			if (Item.ItemList != null && this.body != null)
			{
				if (ignoreContacts)
				{
					this.body.SetTransformIgnoreContacts(this.body.SimPosition + ConvertUnits.ToSimUnits(amount), this.body.Rotation, true);
				}
				else
				{
					this.body.SetTransform(this.body.SimPosition + ConvertUnits.ToSimUnits(amount), this.body.Rotation, true);
				}
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Move(amount, ignoreContacts);
			}
			if (this.body == null)
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					return;
				}
			}
			Submarine submarine = base.Submarine;
			if (submarine == null || !submarine.Loading)
			{
				this.FindHull();
			}
		}

		// Token: 0x06001971 RID: 6513 RVA: 0x000FF404 File Offset: 0x000FD604
		public Rectangle TransformTrigger(Rectangle trigger, bool world = false)
		{
			Rectangle baseRect = world ? base.WorldRect : this.Rect;
			Rectangle transformedRect = new Rectangle((int)((float)baseRect.X + (float)trigger.X * this.Scale), (int)((float)baseRect.Y + (float)trigger.Y * this.Scale), (trigger.Width == 0) ? this.Rect.Width : ((int)((float)trigger.Width * this.Scale)), (trigger.Height == 0) ? this.Rect.Height : ((int)((float)trigger.Height * this.Scale)));
			if (base.FlippedX)
			{
				transformedRect.X = baseRect.X + (baseRect.Right - transformedRect.Right);
			}
			if (base.FlippedY)
			{
				transformedRect.Y = baseRect.Y + (baseRect.Y - baseRect.Height - (transformedRect.Y - transformedRect.Height));
			}
			return transformedRect;
		}

		// Token: 0x06001972 RID: 6514 RVA: 0x000FF4F8 File Offset: 0x000FD6F8
		public override Quad2D GetTransformedQuad()
		{
			return Quad2D.FromSubmarineRectangle(this.rect).Rotated(-this.RotationRad);
		}

		// Token: 0x06001973 RID: 6515 RVA: 0x000FF524 File Offset: 0x000FD724
		public static void UpdateHulls()
		{
			foreach (Item item in Item.ItemList)
			{
				item.FindHull();
			}
		}

		// Token: 0x06001974 RID: 6516 RVA: 0x000FF578 File Offset: 0x000FD778
		public Hull FindHull()
		{
			if (this.parentInventory != null && this.parentInventory.Owner != null)
			{
				Character character = this.parentInventory.Owner as Character;
				if (character != null)
				{
					this.CurrentHull = character.AnimController.CurrentHull;
				}
				else
				{
					Item item = this.parentInventory.Owner as Item;
					if (item != null)
					{
						this.CurrentHull = item.CurrentHull;
					}
				}
				base.Submarine = this.parentInventory.Owner.Submarine;
				if (this.body != null)
				{
					this.body.Submarine = base.Submarine;
				}
				return this.CurrentHull;
			}
			this.CurrentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
			if (this.body != null && this.body.Enabled && (this.body.BodyType == BodyType.Dynamic || base.Submarine == null))
			{
				Hull hull = this.CurrentHull;
				base.Submarine = ((hull != null) ? hull.Submarine : null);
				this.body.Submarine = base.Submarine;
			}
			return this.CurrentHull;
		}

		// Token: 0x06001975 RID: 6517 RVA: 0x000FF690 File Offset: 0x000FD890
		private void RefreshRootContainer()
		{
			Item newRootContainer = null;
			this.inWaterProofContainer = false;
			if (this.Container != null)
			{
				Item rootContainer = this.Container;
				this.inWaterProofContainer |= this.Container.WaterProof;
				while (rootContainer.Container != null)
				{
					rootContainer = rootContainer.Container;
					if (rootContainer == this)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(61, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Invalid container hierarchy: \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\" was contained inside itself!\n");
						defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						rootContainer = null;
						break;
					}
					this.inWaterProofContainer |= rootContainer.WaterProof;
				}
				newRootContainer = rootContainer;
			}
			if (newRootContainer != this.RootContainer)
			{
				this.RootContainer = newRootContainer;
				this.IsActive = true;
				foreach (Item containedItem in this.ContainedItems)
				{
					containedItem.RefreshRootContainer();
				}
			}
		}

		// Token: 0x06001976 RID: 6518 RVA: 0x000FF7AC File Offset: 0x000FD9AC
		private void RefreshInWaterProofContainer()
		{
			this.inWaterProofContainer = false;
			if (this.container == null)
			{
				return;
			}
			if (this.container.WaterProof || this.container.inWaterProofContainer)
			{
				this.inWaterProofContainer = true;
			}
			foreach (Item containedItem in this.ContainedItems)
			{
				containedItem.RefreshInWaterProofContainer();
			}
		}

		// Token: 0x06001977 RID: 6519 RVA: 0x000FF82C File Offset: 0x000FDA2C
		public bool HasAccess(Character character)
		{
			if (character.IsBot && this.IgnoreByAI(character))
			{
				return false;
			}
			if (!this.IsInteractable(character))
			{
				return false;
			}
			ItemContainer itemContainer = this.GetComponent<ItemContainer>();
			if (itemContainer != null && !itemContainer.HasAccess(character))
			{
				return false;
			}
			if (this.Container != null && !this.Container.HasAccess(character))
			{
				return false;
			}
			Pickable component = this.GetComponent<Pickable>();
			return component == null || component.CanBePicked;
		}

		// Token: 0x06001978 RID: 6520 RVA: 0x000FF89C File Offset: 0x000FDA9C
		public bool IsOwnedBy(Entity entity)
		{
			return this.FindParentInventory((Inventory i) => i.Owner == entity) != null;
		}

		// Token: 0x06001979 RID: 6521 RVA: 0x000FF8CC File Offset: 0x000FDACC
		public Entity GetRootInventoryOwner()
		{
			if (this.ParentInventory == null)
			{
				return this;
			}
			if (this.ParentInventory.Owner is Character)
			{
				return this.ParentInventory.Owner;
			}
			Item item = this.RootContainer;
			object obj;
			if (item == null)
			{
				obj = null;
			}
			else
			{
				Inventory inventory = item.ParentInventory;
				obj = ((inventory != null) ? inventory.Owner : null);
			}
			if (obj is Character)
			{
				return this.RootContainer.ParentInventory.Owner;
			}
			return this.RootContainer ?? this;
		}

		// Token: 0x0600197A RID: 6522 RVA: 0x000FF944 File Offset: 0x000FDB44
		public Inventory FindParentInventory(Func<Inventory, bool> predicate)
		{
			if (this.parentInventory != null)
			{
				if (predicate(this.parentInventory))
				{
					return this.parentInventory;
				}
				Item owner = this.parentInventory.Owner as Item;
				if (owner != null)
				{
					return owner.FindParentInventory(predicate);
				}
			}
			return null;
		}

		// Token: 0x0600197B RID: 6523 RVA: 0x000FF98C File Offset: 0x000FDB8C
		public void SetContainedItemPositions()
		{
			foreach (ItemInventory ownInventory in this.OwnInventories)
			{
				ownInventory.Container.SetContainedItemPositions();
			}
		}

		// Token: 0x0600197C RID: 6524 RVA: 0x000FF9C3 File Offset: 0x000FDBC3
		public void AddTag(string tag)
		{
			this.AddTag(tag.ToIdentifier());
		}

		// Token: 0x0600197D RID: 6525 RVA: 0x000FF9D1 File Offset: 0x000FDBD1
		public void AddTag(Identifier tag)
		{
			this.tags.Add(tag);
		}

		// Token: 0x0600197E RID: 6526 RVA: 0x000FF9E0 File Offset: 0x000FDBE0
		public void RemoveTag(Identifier tag)
		{
			if (!this.tags.Contains(tag))
			{
				return;
			}
			this.tags.Remove(tag);
		}

		// Token: 0x0600197F RID: 6527 RVA: 0x000FF9FE File Offset: 0x000FDBFE
		public bool HasTag(Identifier tag)
		{
			return this.tags.Contains(tag) || this.Prefab.Tags.Contains(tag);
		}

		// Token: 0x06001980 RID: 6528 RVA: 0x000FFA21 File Offset: 0x000FDC21
		public bool HasIdentifierOrTags(IEnumerable<Identifier> identifiersOrTags)
		{
			return identifiersOrTags.Contains(this.Prefab.Identifier) || this.HasTag(identifiersOrTags);
		}

		// Token: 0x06001981 RID: 6529 RVA: 0x000FFA3F File Offset: 0x000FDC3F
		public void ReplaceTag(string tag, string newTag)
		{
			this.ReplaceTag(tag.ToIdentifier(), newTag.ToIdentifier());
		}

		// Token: 0x06001982 RID: 6530 RVA: 0x000FFA53 File Offset: 0x000FDC53
		public void ReplaceTag(Identifier tag, Identifier newTag)
		{
			if (!this.tags.Contains(tag))
			{
				return;
			}
			this.tags.Remove(tag);
			this.tags.Add(newTag);
		}

		// Token: 0x06001983 RID: 6531 RVA: 0x000FFA7E File Offset: 0x000FDC7E
		public IReadOnlyCollection<Identifier> GetTags()
		{
			return this.tags;
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x000FFA88 File Offset: 0x000FDC88
		public bool HasTag(IEnumerable<Identifier> allowedTags)
		{
			foreach (Identifier tag in allowedTags)
			{
				if (this.HasTag(tag))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x000FFADC File Offset: 0x000FDCDC
		public bool ConditionalMatches(PropertyConditional conditional)
		{
			return this.ConditionalMatches(conditional, true);
		}

		// Token: 0x06001986 RID: 6534 RVA: 0x000FFAE8 File Offset: 0x000FDCE8
		public bool ConditionalMatches(PropertyConditional conditional, bool checkContainer)
		{
			if (checkContainer && conditional.TargetContainer)
			{
				if (conditional.TargetGrandParent)
				{
					Item item = this.container;
					return ((item != null) ? item.container : null) != null && this.container.container.ConditionalMatches(conditional, false);
				}
				return this.container != null && this.container.ConditionalMatches(conditional, false);
			}
			else
			{
				if (string.IsNullOrEmpty(conditional.TargetItemComponent))
				{
					return conditional.Matches(this);
				}
				PropertyConditional.LogicalOperatorType itemComponentComparison = conditional.ItemComponentComparison;
				if (itemComponentComparison == PropertyConditional.LogicalOperatorType.And)
				{
					bool matchingComponentFound = false;
					foreach (ItemComponent c in this.components)
					{
						if (Item.<ConditionalMatches>g__MatchesComponent|529_0(c, conditional))
						{
							matchingComponentFound = true;
							if (!conditional.Matches(c))
							{
								return false;
							}
						}
					}
					return matchingComponentFound;
				}
				if (itemComponentComparison == PropertyConditional.LogicalOperatorType.Or)
				{
					foreach (ItemComponent c2 in this.components)
					{
						if (Item.<ConditionalMatches>g__MatchesComponent|529_0(c2, conditional) && conditional.Matches(c2))
						{
							return true;
						}
					}
					return false;
				}
				throw new NotSupportedException();
			}
		}

		// Token: 0x06001987 RID: 6535 RVA: 0x000FFC2C File Offset: 0x000FDE2C
		public IEnumerable<StatusEffect> GetStatusEffectsOfType(ActionType type)
		{
			if (!this.hasStatusEffectsOfType[(int)type])
			{
				return Enumerable.Empty<StatusEffect>();
			}
			return this.statusEffectLists[type];
		}

		// Token: 0x06001988 RID: 6536 RVA: 0x000FFC4C File Offset: 0x000FDE4C
		public void ApplyStatusEffects(ActionType type, float deltaTime, Character character = null, Limb limb = null, Entity useTarget = null, bool isNetworkEvent = false, Vector2? worldPosition = null)
		{
			if (!this.hasStatusEffectsOfType[(int)type])
			{
				return;
			}
			foreach (StatusEffect effect in this.statusEffectLists[type])
			{
				this.ApplyStatusEffect(effect, type, deltaTime, character, limb, useTarget, isNetworkEvent, false, worldPosition);
			}
		}

		// Token: 0x06001989 RID: 6537 RVA: 0x000FFCBC File Offset: 0x000FDEBC
		public void ApplyStatusEffect(StatusEffect effect, ActionType type, float deltaTime, Character character = null, Limb limb = null, Entity useTarget = null, bool isNetworkEvent = false, bool checkCondition = true, Vector2? worldPosition = null)
		{
			if (effect.ShouldWaitForInterval(this, deltaTime))
			{
				return;
			}
			if (!isNetworkEvent && checkCondition && this.condition == 0f && !effect.AllowWhenBroken && effect.type != ActionType.OnBroken)
			{
				return;
			}
			if (effect.type != type)
			{
				return;
			}
			bool hasTargets = effect.TargetIdentifiers == null;
			this.targets.Clear();
			if (effect.HasTargetType(StatusEffect.TargetType.Contained))
			{
				using (IEnumerator<Item> enumerator = this.ContainedItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Item containedItem = enumerator.Current;
						if ((effect.TargetIdentifiers == null || effect.TargetIdentifiers.Contains(containedItem.Prefab.Identifier) || effect.TargetIdentifiers.Any((Identifier id) => containedItem.HasTag(id))) && (effect.TargetSlot <= -1 || this.OwnInventory.GetItemsAt(effect.TargetSlot).Contains(containedItem)))
						{
							hasTargets = true;
							this.targets.AddRange(containedItem.AllPropertyObjects);
						}
					}
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters) || effect.HasTargetType(StatusEffect.TargetType.NearbyItems))
			{
				effect.AddNearbyTargets(this.WorldPosition, this.targets);
				if (this.targets.Count > 0)
				{
					hasTargets = true;
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.UseTarget))
			{
				ISerializableEntity serializableTarget = useTarget as ISerializableEntity;
				if (serializableTarget != null)
				{
					hasTargets = true;
					this.targets.Add(serializableTarget);
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.LinkedEntities))
			{
				foreach (MapEntity linkedEntity in this.linkedTo)
				{
					Item linkedItem = linkedEntity as Item;
					if (linkedItem != null)
					{
						this.targets.AddRange(linkedItem.AllPropertyObjects);
					}
					else
					{
						ISerializableEntity serializableEntity = linkedEntity as ISerializableEntity;
						if (serializableEntity != null)
						{
							this.targets.Add(serializableEntity);
						}
					}
				}
			}
			if (!hasTargets)
			{
				return;
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Hull) && this.CurrentHull != null)
			{
				this.targets.Add(this.CurrentHull);
			}
			if (effect.HasTargetType(StatusEffect.TargetType.This))
			{
				foreach (ISerializableEntity pobject in this.AllPropertyObjects)
				{
					this.targets.Add(pobject);
				}
			}
			if (character != null)
			{
				if (effect.HasTargetType(StatusEffect.TargetType.Character))
				{
					if (type == ActionType.OnContained)
					{
						CharacterInventory characterInventory = this.ParentInventory as CharacterInventory;
						if (characterInventory != null)
						{
							this.targets.Add(characterInventory.Owner as ISerializableEntity);
							goto IL_2A3;
						}
					}
					this.targets.Add(character);
				}
				IL_2A3:
				if (effect.HasTargetType(StatusEffect.TargetType.AllLimbs))
				{
					this.targets.AddRange(character.AnimController.Limbs);
				}
				if (effect.HasTargetType(StatusEffect.TargetType.Limb) && limb == null && effect.targetLimbs != null)
				{
					foreach (Limb characterLimb in character.AnimController.Limbs)
					{
						if (effect.targetLimbs.Contains(characterLimb.type))
						{
							this.targets.Add(characterLimb);
						}
					}
				}
			}
			if (effect.HasTargetType(StatusEffect.TargetType.Limb) && limb != null)
			{
				this.targets.Add(limb);
			}
			if (this.Container != null && effect.HasTargetType(StatusEffect.TargetType.Parent))
			{
				this.targets.AddRange(this.Container.AllPropertyObjects);
			}
			effect.Apply(type, deltaTime, this, this.targets, worldPosition);
		}

		// Token: 0x0600198A RID: 6538 RVA: 0x00100070 File Offset: 0x000FE270
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = true)
		{
			if (this.Indestructible || this.InvulnerableToDamage)
			{
				return default(AttackResult);
			}
			float damageAmount = attack.GetItemDamage(deltaTime, this.Prefab.ItemDamageMultiplier);
			this.Condition -= damageAmount;
			if (damageAmount >= this.Prefab.OnDamagedThreshold)
			{
				this.ApplyStatusEffects(ActionType.OnDamaged, 1f, null, null, null, false, null);
			}
			return new AttackResult(damageAmount, null);
		}

		// Token: 0x0600198B RID: 6539 RVA: 0x001000E8 File Offset: 0x000FE2E8
		private void SetCondition(float value, bool isNetworkEvent, bool executeEffects = true)
		{
			Item.<>c__DisplayClass535_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (!isNetworkEvent && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (!MathUtils.IsValid(value))
			{
				return;
			}
			if (this.Indestructible)
			{
				return;
			}
			if (this.InvulnerableToDamage && value <= this.condition)
			{
				return;
			}
			bool wasInFullCondition = this.IsFullCondition;
			float diff = value - this.condition;
			Door door = this.GetComponent<Door>();
			if (door != null && door.IsStuck && diff < 0f)
			{
				float dmg = -diff;
				float prevStuck = door.Stuck;
				door.Stuck -= dmg;
				if (door.IsStuck)
				{
					return;
				}
				float damageReduction = dmg - prevStuck;
				if (damageReduction < 0f)
				{
					return;
				}
				value -= damageReduction;
			}
			this.condition = MathHelper.Clamp(value, 0f, this.MaxCondition);
			if (MathUtils.NearlyEqual(this.prevCondition, value, 1E-06f))
			{
				return;
			}
			this.RecalculateConditionValues();
			CS$<>8__locals1.wasPreviousConditionChanged = false;
			if (this.condition == 0f && this.prevCondition > 0f)
			{
				Item.<SetCondition>g__flagChangedConnections|535_1(this.connections);
				if (executeEffects)
				{
					foreach (ItemComponent ic in this.components)
					{
						ic.PlaySound(ActionType.OnBroken, null);
						ic.StopSounds(ActionType.OnActive);
					}
				}
				if (Screen.Selected == GameMain.SubEditorScreen)
				{
					return;
				}
				this.<SetCondition>g__SetPreviousCondition|535_0(ref CS$<>8__locals1);
				if (executeEffects)
				{
					this.ApplyStatusEffects(ActionType.OnBroken, 1f, null, null, null, false, null);
				}
			}
			else if (this.condition > 0f && this.prevCondition <= 0f)
			{
				Item.<SetCondition>g__flagChangedConnections|535_1(this.connections);
			}
			this.SetActiveSprite();
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				bool needsConditionUpdate = false;
				if (!MathUtils.NearlyEqual(this.lastSentCondition, this.condition, 0.0001f) && (this.condition <= 0f || this.condition >= this.MaxCondition))
				{
					this.sendConditionUpdateTimer = 0f;
					needsConditionUpdate = true;
				}
				else if (Math.Abs(this.lastSentCondition - this.condition) > 1f || wasInFullCondition != this.IsFullCondition)
				{
					needsConditionUpdate = true;
				}
				if (needsConditionUpdate && !Item.itemsWithPendingConditionUpdates.Contains(this))
				{
					Item.itemsWithPendingConditionUpdates.Add(this);
				}
			}
			if (!CS$<>8__locals1.wasPreviousConditionChanged)
			{
				this.<SetCondition>g__SetPreviousCondition|535_0(ref CS$<>8__locals1);
			}
		}

		// Token: 0x0600198C RID: 6540 RVA: 0x00100368 File Offset: 0x000FE568
		public void RecalculateConditionValues()
		{
			this.MaxCondition = this.Prefab.Health * this.healthMultiplier * this.conditionMultiplierCampaign * this.maxRepairConditionMultiplier * (1f + this.GetQualityModifier(Barotrauma.Items.Components.Quality.StatType.Condition));
			this.IsFullCondition = MathUtils.NearlyEqual(this.Condition, this.MaxCondition, 0.0001f);
			this.ConditionPercentage = MathUtils.Percentage(this.Condition, this.MaxCondition);
		}

		// Token: 0x0600198D RID: 6541 RVA: 0x001003DC File Offset: 0x000FE5DC
		private bool IsInWater()
		{
			if (this.CurrentHull == null)
			{
				return true;
			}
			float surfaceY = this.CurrentHull.Surface;
			return this.CurrentHull.WaterVolume > 0f && this.Position.Y < surfaceY;
		}

		// Token: 0x0600198E RID: 6542 RVA: 0x00100424 File Offset: 0x000FE624
		public void SendPendingNetworkUpdates()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsServer)
			{
				return;
			}
			if (!Item.itemsWithPendingConditionUpdates.Contains(this))
			{
				return;
			}
			this.SendPendingNetworkUpdatesInternal();
			Item.itemsWithPendingConditionUpdates.Remove(this);
		}

		// Token: 0x0600198F RID: 6543 RVA: 0x00100463 File Offset: 0x000FE663
		private void SendPendingNetworkUpdatesInternal()
		{
			this.CreateStatusEvent(false);
			this.lastSentCondition = this.condition;
			this.sendConditionUpdateTimer = NetConfig.ItemConditionUpdateInterval;
		}

		// Token: 0x06001990 RID: 6544 RVA: 0x00100484 File Offset: 0x000FE684
		public void CreateStatusEvent(bool loadingRound)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer && this.condition <= 0f && StatusEffect.DurationList.Any((DurationListElement d) => d.Targets.Contains(this) && d.Parent.HasTag(Barotrauma.Tags.OnFireStatusEffectTag)))
			{
				GameMain.NetworkMember.CreateEntityEvent(this, new Item.ApplyStatusEffectEventData(ActionType.OnFire, null, null, null, null, null));
			}
			GameMain.NetworkMember.CreateEntityEvent(this, new Item.ItemStatusEventData(loadingRound));
		}

		// Token: 0x06001991 RID: 6545 RVA: 0x00100504 File Offset: 0x000FE704
		public static void UpdatePendingConditionUpdates(float deltaTime)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember == null || !networkMember.IsServer)
			{
				return;
			}
			for (int i = 0; i < Item.itemsWithPendingConditionUpdates.Count; i++)
			{
				Item item = Item.itemsWithPendingConditionUpdates[i];
				if (item == null || item.Removed)
				{
					Item.itemsWithPendingConditionUpdates.RemoveAt(i--);
				}
				else
				{
					Submarine submarine = item.Submarine;
					if (submarine == null || !submarine.Loading)
					{
						item.sendConditionUpdateTimer -= deltaTime;
						if (item.sendConditionUpdateTimer <= 0f)
						{
							item.SendPendingNetworkUpdatesInternal();
							Item.itemsWithPendingConditionUpdates.RemoveAt(i--);
						}
					}
				}
			}
		}

		// Token: 0x06001992 RID: 6546 RVA: 0x001005A4 File Offset: 0x000FE7A4
		public override void Update(float deltaTime, Camera cam)
		{
			if (!this.IsActive || base.IsLayerHidden || this.IsInRemoveQueue)
			{
				return;
			}
			if (this.impactQueue != null)
			{
				float impact;
				while (this.impactQueue.TryDequeue(out impact))
				{
					this.ReceiveImpact(impact, true);
				}
			}
			if (this.isDroppedStackOwner && this.body != null)
			{
				foreach (Item item in this.droppedStack)
				{
					if (item != this)
					{
						item.body.Enabled = false;
						item.body.SetTransformIgnoreContacts(this.SimPosition, this.body.Rotation, true);
					}
				}
			}
			if (this.aiTarget != null && this.aiTarget.NeedsUpdate)
			{
				this.aiTarget.Update(deltaTime);
			}
			ActionType containedEffectType = (this.parentInventory == null) ? ActionType.OnNotContained : ActionType.OnContained;
			ActionType type = ActionType.Always;
			CharacterInventory characterInventory = this.parentInventory as CharacterInventory;
			this.ApplyStatusEffects(type, deltaTime, ((characterInventory != null) ? characterInventory.Owner : null) as Character, null, null, false, null);
			ActionType type2 = containedEffectType;
			CharacterInventory characterInventory2 = this.parentInventory as CharacterInventory;
			this.ApplyStatusEffects(type2, deltaTime, ((characterInventory2 != null) ? characterInventory2.Owner : null) as Character, null, null, false, null);
			for (int i = 0; i < this.updateableComponents.Count; i++)
			{
				ItemComponent ic = this.updateableComponents[i];
				bool flag;
				if (ic.InheritParentIsActive)
				{
					ItemComponent parent = ic.Parent;
					flag = (parent != null && !parent.IsActive);
				}
				else
				{
					flag = false;
				}
				bool isParentInActive = flag;
				if (ic.IsActiveConditionals != null && !isParentInActive)
				{
					if (ic.IsActiveConditionalComparison == PropertyConditional.LogicalOperatorType.And)
					{
						bool shouldBeActive = true;
						foreach (PropertyConditional conditional in ic.IsActiveConditionals)
						{
							if (!this.ConditionalMatches(conditional))
							{
								shouldBeActive = false;
								break;
							}
						}
						ic.IsActive = shouldBeActive;
					}
					else
					{
						bool shouldBeActive2 = false;
						foreach (PropertyConditional conditional2 in ic.IsActiveConditionals)
						{
							if (this.ConditionalMatches(conditional2))
							{
								shouldBeActive2 = true;
								break;
							}
						}
						ic.IsActive = shouldBeActive2;
					}
				}
				if (ic.HasSounds)
				{
					ic.PlaySound(ActionType.Always, null);
					ic.UpdateSounds();
					if (!ic.WasUsed)
					{
						ic.StopSounds(ActionType.OnUse);
					}
					if (!ic.WasSecondaryUsed)
					{
						ic.StopSounds(ActionType.OnSecondaryUse);
					}
				}
				ic.WasUsed = false;
				ic.WasSecondaryUsed = false;
				if (ic.IsActive || ic.UpdateWhenInactive)
				{
					if (!ic.UpdateWhenBroken && this.condition <= 0f)
					{
						ic.UpdateBroken(deltaTime, cam);
					}
					else
					{
						ic.Update(deltaTime, cam);
						if (ic.IsActive)
						{
							if (ic.IsActiveTimer > 0.02f)
							{
								ic.PlaySound(ActionType.OnActive, null);
							}
							ic.IsActiveTimer += deltaTime;
						}
					}
				}
			}
			if (base.Removed)
			{
				return;
			}
			bool needsWaterCheck = this.hasInWaterStatusEffects || this.hasNotInWaterStatusEffects;
			if (this.body != null && this.body.Enabled)
			{
				if (Math.Abs(this.body.LinearVelocity.X) > 0.01f || Math.Abs(this.body.LinearVelocity.Y) > 0.01f || this.transformDirty)
				{
					if (this.body.CollisionCategories != Category.None)
					{
						this.UpdateTransform();
					}
					if (this.CurrentHull == null && Level.Loaded != null && this.body.SimPosition.Y < ConvertUnits.ToSimUnits(-1000000))
					{
						EntitySpawner spawner = Entity.Spawner;
						if (spawner == null)
						{
							return;
						}
						spawner.AddItemToRemoveQueue(this);
						return;
					}
				}
				needsWaterCheck = true;
				this.UpdateNetPosition(deltaTime);
				if (this.inWater)
				{
					this.ApplyWaterForces();
					Hull hull = this.CurrentHull;
					if (hull != null)
					{
						hull.ApplyFlowForces(deltaTime, this);
					}
				}
			}
			if (needsWaterCheck)
			{
				bool wasInWater = this.inWater;
				this.inWater = (!this.inWaterProofContainer && this.IsInWater());
				if (this.inWater && !wasInWater && this.CurrentHull != null && this.body != null && this.body.LinearVelocity.Y < -1f)
				{
					this.Splash();
					Projectile component = this.GetComponent<Projectile>();
					if (component == null || !component.IsActive)
					{
						this.body.LinearVelocity *= 0.2f;
					}
				}
				if ((this.hasInWaterStatusEffects || this.hasNotInWaterStatusEffects) && this.condition > 0f)
				{
					this.ApplyStatusEffects(this.inWater ? ActionType.InWater : ActionType.NotInWater, deltaTime, null, null, null, false, null);
				}
				if (this.inWaterProofContainer && !this.hasNotInWaterStatusEffects)
				{
					needsWaterCheck = false;
				}
			}
			if (!needsWaterCheck && this.updateableComponents.Count == 0 && (this.aiTarget == null || !this.aiTarget.NeedsUpdate) && !this.hasStatusEffectsOfType[0] && !this.hasStatusEffectsOfType[(int)containedEffectType] && (this.body == null || !this.body.Enabled))
			{
				this.positionBuffer.Clear();
				this.IsActive = false;
			}
		}

		// Token: 0x06001993 RID: 6547 RVA: 0x00100B08 File Offset: 0x000FED08
		private void Splash()
		{
			if (this.body == null || this.CurrentHull == null)
			{
				return;
			}
			float massFactor = MathHelper.Clamp(this.body.Mass, 0.5f, 20f);
			int i = 0;
			while ((float)i < MathHelper.Clamp(Math.Abs(this.body.LinearVelocity.Y), 1f, 10f))
			{
				Particle splash = GameMain.ParticleManager.CreateParticle("watersplash", new Vector2(this.WorldPosition.X, this.CurrentHull.WorldSurface), new Vector2(0f, Math.Abs(-this.body.LinearVelocity.Y * massFactor)) + Rand.Vector(Math.Abs(this.body.LinearVelocity.Y * 10f), Rand.RandSync.Unsynced), Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced), this.CurrentHull, 0f, null);
				if (splash != null)
				{
					splash.Size *= MathHelper.Clamp(Math.Abs(this.body.LinearVelocity.Y) * 0.1f * massFactor, 1f, 4f);
				}
				i++;
			}
			GameMain.ParticleManager.CreateParticle("bubbles", new Vector2(this.WorldPosition.X, this.CurrentHull.WorldSurface), this.body.LinearVelocity * massFactor, 0f, this.CurrentHull, 0f, null);
			if (this.body.LinearVelocity.Y < 0f)
			{
				int j = (int)((this.Position.X - (float)this.CurrentHull.Rect.X) / 32f);
				if (j >= 0 && j < this.currentHull.WaveVel.Length)
				{
					this.CurrentHull.WaveVel[j] += MathHelper.Clamp(this.body.LinearVelocity.Y * massFactor, -5f, 5f);
				}
			}
			SoundPlayer.PlaySplashSound(this.WorldPosition, Math.Abs(this.body.LinearVelocity.Y) + Rand.Range(-10f, -5f, Rand.RandSync.Unsynced));
		}

		// Token: 0x06001994 RID: 6548 RVA: 0x00100D4C File Offset: 0x000FEF4C
		public void UpdateTransform()
		{
			if (this.body == null)
			{
				return;
			}
			Submarine prevSub = base.Submarine;
			Projectile projectile = this.GetComponent<Projectile>();
			if (((projectile != null) ? projectile.StickTarget : null) != null)
			{
				Limb limb = ((projectile != null) ? projectile.StickTarget.UserData : null) as Limb;
				if (limb != null && limb.character != null)
				{
					base.Submarine = (this.body.Submarine = limb.character.Submarine);
					this.currentHull = limb.character.CurrentHull;
				}
				else
				{
					Structure structure = projectile.StickTarget.UserData as Structure;
					if (structure != null)
					{
						base.Submarine = (this.body.Submarine = structure.Submarine);
						this.currentHull = Hull.FindHull(this.WorldPosition, this.CurrentHull, true, true);
					}
					else
					{
						Item targetItem = projectile.StickTarget.UserData as Item;
						if (targetItem != null)
						{
							base.Submarine = (this.body.Submarine = targetItem.Submarine);
							this.currentHull = targetItem.CurrentHull;
						}
						else if (projectile.StickTarget.UserData is Submarine)
						{
							base.Submarine = (this.body.Submarine = null);
							this.currentHull = null;
						}
					}
				}
			}
			else
			{
				this.FindHull();
			}
			if (base.Submarine == null && prevSub != null)
			{
				this.body.SetTransformIgnoreContacts(this.body.SimPosition + prevSub.SimPosition, this.body.Rotation, true);
			}
			else if (base.Submarine != null && prevSub == null)
			{
				this.body.SetTransformIgnoreContacts(this.body.SimPosition - base.Submarine.SimPosition, this.body.Rotation, true);
			}
			else if (base.Submarine != null && prevSub != null && base.Submarine != prevSub)
			{
				this.body.SetTransformIgnoreContacts(this.body.SimPosition + prevSub.SimPosition - base.Submarine.SimPosition, this.body.Rotation, true);
			}
			if (base.Submarine != prevSub)
			{
				foreach (Item containedItem in this.ContainedItems)
				{
					if (containedItem != null)
					{
						containedItem.Submarine = base.Submarine;
					}
				}
			}
			Vector2 displayPos = ConvertUnits.ToDisplayUnits(this.body.SimPosition);
			this.rect.X = (int)(displayPos.X - (float)this.rect.Width / 2f);
			this.rect.Y = (int)(displayPos.Y + (float)this.rect.Height / 2f);
			if (Math.Abs(this.body.LinearVelocity.X) > 64f || Math.Abs(this.body.LinearVelocity.Y) > 64f)
			{
				this.body.LinearVelocity = new Vector2(MathHelper.Clamp(this.body.LinearVelocity.X, -64f, 64f), MathHelper.Clamp(this.body.LinearVelocity.Y, -64f, 64f));
			}
			this.transformDirty = false;
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x001010B4 File Offset: 0x000FF2B4
		private void ApplyWaterForces()
		{
			if (this.body.Mass <= 0f || this.body.Density <= 0f || this.body.BodyType != BodyType.Dynamic)
			{
				return;
			}
			float forceFactor = 1f;
			if (this.CurrentHull != null)
			{
				float floor = (float)(this.CurrentHull.Rect.Y - this.CurrentHull.Rect.Height);
				float waterLevel = floor + this.CurrentHull.WaterVolume / (float)this.CurrentHull.Rect.Width;
				forceFactor = Math.Min((waterLevel - this.Position.Y) / (float)this.rect.Height, 1f);
				if (forceFactor <= 0f)
				{
					return;
				}
			}
			bool moving = this.body.LinearVelocity.LengthSquared() > 0.001f;
			float volume = this.body.Mass / this.body.Density;
			if (moving)
			{
				Vector2 localFront = this.body.GetLocalFront(null);
				Vector2 frontVel = this.body.FarseerBody.GetLinearVelocityFromLocalPoint(localFront);
				float speed = frontVel.Length();
				float drag = speed * speed * this.WaterDragCoefficient * volume * 10f;
				if (this.body.FarseerBody.IsBullet)
				{
					drag *= 0.1f;
				}
				Vector2 dragVec = -frontVel / speed * drag;
				Vector2 back = this.body.FarseerBody.GetWorldPoint(-localFront * 0.01f);
				this.body.ApplyForce(dragVec, back);
			}
			if (moving || this.body.Density <= 10f)
			{
				Vector2 buoyancy = -GameMain.World.Gravity * this.body.FarseerBody.GravityScale * forceFactor * volume * 10f;
				this.body.ApplyForce(buoyancy, 64f);
			}
			if (Math.Abs(this.body.AngularVelocity) > 0.0001f)
			{
				this.body.ApplyTorque(this.body.AngularVelocity * volume * -0.1f);
			}
		}

		// Token: 0x06001996 RID: 6550 RVA: 0x001012F8 File Offset: 0x000FF4F8
		private bool OnCollision(Fixture f1, Fixture f2, Contact contact)
		{
			if (this.transformDirty)
			{
				return false;
			}
			Projectile projectile = this.GetComponent<Projectile>();
			if (projectile != null)
			{
				if (f2.CollisionCategories == Category.Cat2)
				{
					return false;
				}
				if (projectile.IgnoredBodies != null && projectile.IgnoredBodies.Contains(f2.Body))
				{
					return false;
				}
				if (projectile.ShouldIgnoreSubmarineCollision(f2, contact))
				{
					return false;
				}
			}
			if (GameMain.GameSession == null || GameMain.GameSession.RoundDuration > 1f)
			{
				Vector2 normal;
				FixedArray2<Vector2> fixedArray;
				contact.GetWorldManifold(out normal, out fixedArray);
				if (contact.FixtureA.Body == f1.Body)
				{
					normal = -normal;
				}
				float impact = Vector2.Dot(f1.Body.LinearVelocity, -normal);
				if (this.impactQueue == null)
				{
					this.impactQueue = new ConcurrentQueue<float>();
				}
				this.impactQueue.Enqueue(impact);
			}
			this.IsActive = true;
			return true;
		}

		// Token: 0x06001997 RID: 6551 RVA: 0x001013C8 File Offset: 0x000FF5C8
		public void ReceiveImpact(float impactStrength, bool recursive = true)
		{
			this.OnCollisionProjSpecific(impactStrength);
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			if (this.ImpactTolerance > 0f && Math.Abs(impactStrength) > this.ImpactTolerance && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.ImpactDamageProbability)
			{
				if (this.ImpactDamage != 0f)
				{
					this.Condition -= impactStrength * this.ImpactDamage;
				}
				if (this.hasStatusEffectsOfType[14])
				{
					foreach (StatusEffect effect in this.statusEffectLists[ActionType.OnImpact])
					{
						this.ApplyStatusEffect(effect, ActionType.OnImpact, 1f, null, null, null, false, true, null);
					}
				}
			}
			if (!recursive)
			{
				return;
			}
			foreach (Item contained in this.ContainedItems)
			{
				if (contained.body != null)
				{
					contained.ReceiveImpact(impactStrength, true);
				}
			}
		}

		// Token: 0x06001998 RID: 6552 RVA: 0x00101508 File Offset: 0x000FF708
		private void OnCollisionProjSpecific(float impact)
		{
			if (impact > 1f && this.Container == null && !string.IsNullOrEmpty(this.Prefab.ImpactSoundTag) && Timing.TotalTime > (double)(this.LastImpactSoundTime + 0.2f))
			{
				this.LastImpactSoundTime = (float)Timing.TotalTime;
				string impactSoundTag = this.Prefab.ImpactSoundTag;
				Vector2 worldPosition = this.WorldPosition;
				Hull hullGuess = this.CurrentHull;
				SoundPlayer.PlaySound(impactSoundTag, worldPosition, null, null, hullGuess);
			}
		}

		// Token: 0x06001999 RID: 6553 RVA: 0x00101588 File Offset: 0x000FF788
		public override void FlipX(bool relativeToSub, bool force = false)
		{
			base.FlipX(relativeToSub, false);
			if (!this.Prefab.CanFlipX && !force)
			{
				base.FlippedX = false;
				return;
			}
			if (this.Prefab.AllowRotatingInEditor)
			{
				this.RotationRad = MathUtils.WrapAnglePi(-this.RotationRad);
			}
			if (this.Prefab.CanSpriteFlipX)
			{
				this.SpriteEffects ^= SpriteEffects.FlipHorizontally;
			}
			foreach (ItemComponent component in this.components)
			{
				component.FlipX(relativeToSub);
			}
			this.SetContainedItemPositions();
		}

		// Token: 0x0600199A RID: 6554 RVA: 0x0010163C File Offset: 0x000FF83C
		public override void FlipY(bool relativeToSub, bool force = false)
		{
			base.FlipY(relativeToSub, false);
			if (!this.Prefab.CanFlipY && !force)
			{
				base.FlippedY = false;
				return;
			}
			if (this.Prefab.AllowRotatingInEditor)
			{
				this.RotationRad = MathUtils.WrapAngleTwoPi(-this.RotationRad);
			}
			if (this.Prefab.CanSpriteFlipY)
			{
				this.SpriteEffects ^= SpriteEffects.FlipVertically;
			}
			foreach (ItemComponent component in this.components)
			{
				component.FlipY(relativeToSub);
			}
			this.SetContainedItemPositions();
		}

		// Token: 0x0600199B RID: 6555 RVA: 0x001016F0 File Offset: 0x000FF8F0
		public T GetDirectlyConnectedComponent<T>(Func<Connection, bool> connectionFilter = null) where T : ItemComponent
		{
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel == null)
			{
				return default(T);
			}
			foreach (Connection c in connectionPanel.Connections)
			{
				if (connectionFilter == null || connectionFilter(c))
				{
					foreach (Connection recipient in c.Recipients)
					{
						T component = recipient.Item.GetComponent<T>();
						if (component != null)
						{
							return component;
						}
					}
				}
			}
			return default(T);
		}

		// Token: 0x0600199C RID: 6556 RVA: 0x001017C4 File Offset: 0x000FF9C4
		public List<T> GetConnectedComponents<T>(bool recursive = false, bool allowTraversingBackwards = true, Func<Connection, bool> connectionFilter = null) where T : ItemComponent
		{
			List<T> connectedComponents = new List<T>();
			if (recursive)
			{
				HashSet<Connection> alreadySearched = new HashSet<Connection>();
				this.GetConnectedComponentsRecursive<T>(alreadySearched, connectedComponents, false, allowTraversingBackwards);
				return connectedComponents;
			}
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel == null)
			{
				return connectedComponents;
			}
			foreach (Connection c in connectionPanel.Connections)
			{
				if (connectionFilter == null || connectionFilter(c))
				{
					foreach (Connection recipient in c.Recipients)
					{
						T component = recipient.Item.GetComponent<T>();
						if (component != null && !connectedComponents.Contains(component))
						{
							connectedComponents.Add(component);
						}
					}
				}
			}
			return connectedComponents;
		}

		// Token: 0x0600199D RID: 6557 RVA: 0x001018B0 File Offset: 0x000FFAB0
		private void GetConnectedComponentsRecursive<T>(HashSet<Connection> alreadySearched, List<T> connectedComponents, bool ignoreInactiveRelays = false, bool allowTraversingBackwards = true) where T : ItemComponent
		{
			ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
			if (connectionPanel == null)
			{
				return;
			}
			foreach (Connection c in connectionPanel.Connections)
			{
				if (alreadySearched.Add(c))
				{
					this.GetConnectedComponentsRecursive<T>(c, alreadySearched, connectedComponents, ignoreInactiveRelays, allowTraversingBackwards);
				}
			}
		}

		// Token: 0x0600199E RID: 6558 RVA: 0x0010191C File Offset: 0x000FFB1C
		public List<T> GetConnectedComponentsRecursive<T>(Connection c, bool ignoreInactiveRelays = false, bool allowTraversingBackwards = true) where T : ItemComponent
		{
			List<T> connectedComponents = new List<T>();
			HashSet<Connection> alreadySearched = new HashSet<Connection>();
			this.GetConnectedComponentsRecursive<T>(c, alreadySearched, connectedComponents, ignoreInactiveRelays, allowTraversingBackwards);
			return connectedComponents;
		}

		// Token: 0x0600199F RID: 6559 RVA: 0x00101944 File Offset: 0x000FFB44
		private void GetConnectedComponentsRecursive<T>(Connection c, HashSet<Connection> alreadySearched, List<T> connectedComponents, bool ignoreInactiveRelays, bool allowTraversingBackwards = true) where T : ItemComponent
		{
			Item.<>c__DisplayClass558_0<T> CS$<>8__locals1;
			CS$<>8__locals1.alreadySearched = alreadySearched;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.connectedComponents = connectedComponents;
			CS$<>8__locals1.ignoreInactiveRelays = ignoreInactiveRelays;
			CS$<>8__locals1.allowTraversingBackwards = allowTraversingBackwards;
			CS$<>8__locals1.c = c;
			CS$<>8__locals1.alreadySearched.Add(CS$<>8__locals1.c);
			foreach (Connection recipient in Item.<GetConnectedComponentsRecursive>g__GetRecipients|558_0<T>(CS$<>8__locals1.c))
			{
				if (!CS$<>8__locals1.alreadySearched.Contains(recipient))
				{
					T component = recipient.Item.GetComponent<T>();
					if (component != null && !CS$<>8__locals1.connectedComponents.Contains(component))
					{
						CS$<>8__locals1.connectedComponents.Add(component);
					}
					CircuitBox circuitBox = recipient.Item.GetComponent<CircuitBox>();
					CircuitBoxConnection cbConnection;
					if (circuitBox != null && circuitBox.FindInputOutputConnection(recipient).TryUnwrap(out cbConnection))
					{
						CircuitBoxInputConnection inputConnection = cbConnection as CircuitBoxInputConnection;
						if (inputConnection != null)
						{
							using (List<CircuitBoxConnection>.Enumerator enumerator2 = inputConnection.ExternallyConnectedTo.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									CircuitBoxConnection connectedTo = enumerator2.Current;
									if (!CS$<>8__locals1.alreadySearched.Contains(connectedTo.Connection))
									{
										this.<GetConnectedComponentsRecursive>g__CheckRecipient|558_1<T>(connectedTo.Connection, ref CS$<>8__locals1);
									}
								}
								goto IL_18B;
							}
						}
						foreach (CircuitBoxConnection connectedFrom in cbConnection.ExternallyConnectedFrom)
						{
							if (!CS$<>8__locals1.alreadySearched.Contains(connectedFrom.Connection) && CS$<>8__locals1.allowTraversingBackwards)
							{
								this.<GetConnectedComponentsRecursive>g__CheckRecipient|558_1<T>(connectedFrom.Connection, ref CS$<>8__locals1);
							}
						}
					}
					IL_18B:
					this.<GetConnectedComponentsRecursive>g__CheckRecipient|558_1<T>(recipient, ref CS$<>8__locals1);
				}
			}
			if (CS$<>8__locals1.ignoreInactiveRelays)
			{
				RelayComponent relay = this.GetComponent<RelayComponent>();
				if (relay != null && !relay.IsOn)
				{
					return;
				}
			}
			foreach (ValueTuple<Identifier, Identifier> valueTuple in Item.connectionPairs)
			{
				Identifier input = valueTuple.Item1;
				Identifier output = valueTuple.Item2;
				this.<GetConnectedComponentsRecursive>g__searchFromAToB|558_2<T>(input, output, ref CS$<>8__locals1);
				if (CS$<>8__locals1.allowTraversingBackwards)
				{
					this.<GetConnectedComponentsRecursive>g__searchFromAToB|558_2<T>(output, input, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x060019A0 RID: 6560 RVA: 0x00101BB4 File Offset: 0x000FFDB4
		public Controller FindController(ImmutableArray<Identifier>? tags = null)
		{
			List<Controller> controllers = this.GetConnectedComponents<Controller>(false, true, null);
			bool needsTag = tags != null && tags.Value.Length > 0;
			if (controllers.None(null) || (needsTag && controllers.None((Controller c) => c.Item.HasTag(tags))))
			{
				controllers = this.GetConnectedComponents<Controller>(true, true, null);
			}
			if (needsTag)
			{
				controllers.RemoveAll((Controller c) => !c.Item.HasTag(tags));
			}
			Controller result;
			if (controllers.Count >= 2)
			{
				if ((result = controllers.FirstOrDefault((Controller c) => c.GetFocusTarget() == this)) == null)
				{
					return controllers.FirstOrDefault<Controller>();
				}
			}
			else
			{
				result = controllers.FirstOrDefault<Controller>();
			}
			return result;
		}

		// Token: 0x060019A1 RID: 6561 RVA: 0x00101C7C File Offset: 0x000FFE7C
		public bool TryFindController(out Controller controller, ImmutableArray<Identifier>? tags = null)
		{
			controller = this.FindController(tags);
			return controller != null;
		}

		// Token: 0x060019A2 RID: 6562 RVA: 0x00101C8C File Offset: 0x000FFE8C
		public void SendSignal(string signal, string connectionName)
		{
			this.SendSignal(new Signal(signal, 0, null, null, 0f, 1f), connectionName);
		}

		// Token: 0x060019A3 RID: 6563 RVA: 0x00101CA8 File Offset: 0x000FFEA8
		public void SendSignal(Signal signal, string connectionName)
		{
			if (this.connections == null)
			{
				return;
			}
			Connection connection;
			if (!this.connections.TryGetValue(connectionName, out connection))
			{
				return;
			}
			ref Item ptr = ref signal.source;
			if (ptr == null)
			{
				ptr = this;
			}
			this.SendSignal(signal, connection);
		}

		// Token: 0x060019A4 RID: 6564 RVA: 0x00101CE8 File Offset: 0x000FFEE8
		public void SendSignal(Signal signal, Connection connection)
		{
			this.LastSentSignalRecipients.Clear();
			if (this.connections == null || connection == null)
			{
				return;
			}
			signal.stepsTaken++;
			if (signal.stepsTaken > 5 && signal.source != null)
			{
				int duplicateRecipients = 0;
				foreach (Connection recipient in signal.source.LastSentSignalRecipients)
				{
					if (recipient == connection)
					{
						duplicateRecipients++;
						if (duplicateRecipients > 2)
						{
							return;
						}
					}
				}
			}
			if (signal.stepsTaken > 10)
			{
				signal.stepsTaken = 0;
				bool duplicateFound = false;
				foreach (ValueTuple<Signal, Connection> s in this.delayedSignals)
				{
					if (s.Item2 == connection && s.Item1.source == signal.source && s.Item1.value == signal.value && s.Item1.sender == signal.sender)
					{
						duplicateFound = true;
						break;
					}
				}
				if (!duplicateFound)
				{
					this.delayedSignals.Add(new ValueTuple<Signal, Connection>(signal, connection));
					CoroutineManager.StartCoroutine(this.DelaySignal(signal, connection), "");
					return;
				}
			}
			else
			{
				if (connection.Effects != null && signal.value != "0" && !string.IsNullOrEmpty(signal.value))
				{
					foreach (StatusEffect effect in connection.Effects)
					{
						if (this.condition > 0f || effect.type == ActionType.OnBroken)
						{
							this.ApplyStatusEffect(effect, ActionType.OnUse, 0.016666668f, null, null, null, false, true, null);
						}
					}
				}
				ref Item ptr = ref signal.source;
				if (ptr == null)
				{
					ptr = this;
				}
				connection.SendSignal(signal);
			}
		}

		// Token: 0x060019A5 RID: 6565 RVA: 0x00101F08 File Offset: 0x00100108
		private IEnumerable<CoroutineStatus> DelaySignal(Signal signal, Connection connection)
		{
			Item.<DelaySignal>d__565 <DelaySignal>d__ = new Item.<DelaySignal>d__565(-2);
			<DelaySignal>d__.<>4__this = this;
			<DelaySignal>d__.<>3__signal = signal;
			<DelaySignal>d__.<>3__connection = connection;
			return <DelaySignal>d__;
		}

		// Token: 0x060019A6 RID: 6566 RVA: 0x00101F28 File Offset: 0x00100128
		public bool IsInsideTrigger(Vector2 worldPosition)
		{
			Rectangle rectangle;
			return this.IsInsideTrigger(worldPosition, out rectangle);
		}

		// Token: 0x060019A7 RID: 6567 RVA: 0x00101F40 File Offset: 0x00100140
		public bool IsInsideTrigger(Vector2 worldPosition, out Rectangle transformedTrigger)
		{
			foreach (Rectangle trigger in this.Prefab.Triggers)
			{
				transformedTrigger = this.TransformTrigger(trigger, true);
				if (Submarine.RectContains(transformedTrigger, worldPosition, false))
				{
					return true;
				}
			}
			transformedTrigger = Rectangle.Empty;
			return false;
		}

		// Token: 0x060019A8 RID: 6568 RVA: 0x00101F9F File Offset: 0x0010019F
		public bool CanClientAccess(Client c)
		{
			return c != null && c.Character != null && c.Character.CanInteractWith(this, true);
		}

		// Token: 0x060019A9 RID: 6569 RVA: 0x00101FBC File Offset: 0x001001BC
		public bool TryInteract(Character user, bool ignoreRequiredItems = false, bool forceSelectKey = false, bool forceUseKey = false)
		{
			CampaignMode.InteractionType campaignInteractionType = this.CampaignInteractionType;
			if (CampaignMode.BlocksInteraction(campaignInteractionType))
			{
				return false;
			}
			bool picked = false;
			bool selected = false;
			bool hasRequiredSkills = true;
			Skill requiredSkill = null;
			float skillMultiplier = 1f;
			if (!this.IsInteractable(user))
			{
				return false;
			}
			foreach (ItemComponent ic in this.components)
			{
				bool pickHit = false;
				bool selectHit = false;
				if (!(ic is Ladder) && user.IsKeyDown(InputType.Aim))
				{
					pickHit = false;
					selectHit = false;
				}
				else if (forceSelectKey)
				{
					if (ic.PickKey == InputType.Select)
					{
						pickHit = true;
					}
					if (ic.SelectKey == InputType.Select)
					{
						selectHit = true;
					}
				}
				else if (forceUseKey)
				{
					if (ic.PickKey == InputType.Use)
					{
						pickHit = true;
					}
					if (ic.SelectKey == InputType.Use)
					{
						selectHit = true;
					}
				}
				else
				{
					pickHit = user.IsKeyHit(ic.PickKey);
					selectHit = user.IsKeyHit(ic.SelectKey);
					if (user == Character.Controlled && GUI.MouseOn != null)
					{
						if (GameSettings.CurrentConfig.KeyMap.Bindings[ic.PickKey].MouseButton == MouseButton.PrimaryMouse)
						{
							pickHit = false;
						}
						if (GameSettings.CurrentConfig.KeyMap.Bindings[ic.SelectKey].MouseButton == MouseButton.PrimaryMouse)
						{
							selectHit = false;
						}
					}
				}
				if (Screen.Selected == GameMain.SubEditorScreen && GameMain.SubEditorScreen.WiringMode)
				{
					selectHit = (pickHit = ((GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Use].MouseButton == MouseButton.None) ? user.IsKeyHit(InputType.Use) : user.IsKeyHit(InputType.Select)));
				}
				if (pickHit || selectHit)
				{
					Skill tempRequiredSkill;
					if (!ic.HasRequiredSkills(user, out tempRequiredSkill))
					{
						hasRequiredSkills = false;
						skillMultiplier = ic.GetSkillMultiplier();
					}
					bool showUiMsg = user == Character.Controlled && Screen.Selected != GameMain.SubEditorScreen && ((pickHit && ic.CanBePicked) || (selectHit && ic.CanBeSelected));
					if ((ignoreRequiredItems || ic.HasRequiredItems(user, showUiMsg, null)) && ((ic.CanBePicked && pickHit && ic.Pick(user)) || (ic.CanBeSelected && selectHit && ic.Select(user))))
					{
						picked = true;
						ic.ApplyStatusEffects(ActionType.OnPicked, 1f, user, null, null, null, null, 1f);
						if (user == Character.Controlled)
						{
							GUI.ForceMouseOn(null);
						}
						if (tempRequiredSkill != null)
						{
							requiredSkill = tempRequiredSkill;
						}
						if (ic.CanBeSelected && !(ic is Door))
						{
							selected = true;
						}
					}
				}
			}
			Inventory inventory = this.ParentInventory;
			if (((inventory != null) ? inventory.Owner : null) == user && this.GetComponent<ItemContainer>() != null)
			{
				selected = false;
			}
			if (!picked)
			{
				return false;
			}
			Action onInteract = this.OnInteract;
			if (onInteract != null)
			{
				onInteract();
			}
			if (user != null)
			{
				if (user.SelectedItem == this)
				{
					if (user.IsKeyHit(InputType.Select) || forceSelectKey)
					{
						user.SelectedItem = null;
					}
				}
				else if (user.SelectedSecondaryItem == this)
				{
					if (user.IsKeyHit(InputType.Select) || forceSelectKey)
					{
						user.SelectedSecondaryItem = null;
					}
				}
				else if (selected)
				{
					if (this.IsSecondaryItem)
					{
						user.SelectedSecondaryItem = this;
					}
					else
					{
						user.SelectedItem = this;
					}
				}
			}
			if (!hasRequiredSkills && Character.Controlled == user && Screen.Selected != GameMain.SubEditorScreen && requiredSkill != null)
			{
				GUI.AddMessage(TextManager.GetWithVariables("InsufficientSkills", new ValueTuple<string, LocalizedString, FormatCapitals>[]
				{
					new ValueTuple<string, LocalizedString, FormatCapitals>("[requiredskill]", TextManager.Get("SkillName." + requiredSkill.Identifier.ToString()), FormatCapitals.Yes),
					new ValueTuple<string, LocalizedString, FormatCapitals>("[requiredlevel]", ((int)(requiredSkill.Level * skillMultiplier)).ToString(), FormatCapitals.No)
				}), GUIStyle.Red, null, true, null);
			}
			if (this.Container != null)
			{
				this.Container.RemoveContained(this);
			}
			return true;
		}

		// Token: 0x060019AA RID: 6570 RVA: 0x001023A8 File Offset: 0x001005A8
		public float GetContainedItemConditionPercentage()
		{
			if (this.ownInventory == null)
			{
				return -1f;
			}
			float condition = 0f;
			float maxCondition = 0f;
			foreach (Item item in this.ContainedItems)
			{
				condition += item.condition;
				maxCondition += item.MaxCondition;
			}
			if (maxCondition > 0f)
			{
				return condition / maxCondition;
			}
			return -1f;
		}

		// Token: 0x060019AB RID: 6571 RVA: 0x0010242C File Offset: 0x0010062C
		public void Use(float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
		{
			if (this.RequireAimToUse && (user == null || !user.IsKeyDown(InputType.Aim)))
			{
				return;
			}
			if (this.condition <= 0f)
			{
				return;
			}
			bool remove = false;
			foreach (ItemComponent ic in this.components)
			{
				bool isControlled = user == Character.Controlled;
				if (ic.HasRequiredContainedItems(user, isControlled, null) && ic.Use(deltaTime, user))
				{
					ic.WasUsed = true;
					ic.PlaySound(ActionType.OnUse, user);
					ic.ApplyStatusEffects(ActionType.OnUse, deltaTime, user, targetLimb, useTarget, user, null, 1f);
					ic.OnUsed.Invoke(new ItemComponent.ItemUseInfo(this, user ?? userForOnUsedEvent));
					if (ic.DeleteOnUse)
					{
						remove = true;
					}
				}
			}
			if (remove)
			{
				Entity.Spawner.AddItemToRemoveQueue(this);
			}
		}

		// Token: 0x060019AC RID: 6572 RVA: 0x00102518 File Offset: 0x00100718
		public void SecondaryUse(float deltaTime, Character character = null)
		{
			if (this.condition <= 0f)
			{
				return;
			}
			bool remove = false;
			foreach (ItemComponent ic in this.components)
			{
				bool isControlled = character == Character.Controlled;
				if (ic.HasRequiredContainedItems(character, isControlled, null) && ic.SecondaryUse(deltaTime, character))
				{
					ic.WasSecondaryUsed = true;
					ic.PlaySound(ActionType.OnSecondaryUse, character);
					ic.ApplyStatusEffects(ActionType.OnSecondaryUse, deltaTime, character, null, character, character, null, 1f);
					if (ic.DeleteOnUse)
					{
						remove = true;
					}
				}
			}
			if (remove)
			{
				Entity.Spawner.AddItemToRemoveQueue(this);
			}
		}

		// Token: 0x060019AD RID: 6573 RVA: 0x001025DC File Offset: 0x001007DC
		public void ApplyTreatment(Character user, Character character, Limb targetLimb)
		{
			if (character.IsDead)
			{
				return;
			}
			if (!this.UseInHealthInterface)
			{
				return;
			}
			if (this.Prefab.ContentPackage == ContentPackageManager.VanillaCorePackage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f)
			{
				GameAnalyticsManager.AddDesignEvent("ApplyTreatment:" + this.Prefab.Identifier.ToString());
			}
			if (user == Character.Controlled)
			{
				if (HealingCooldown.IsOnCooldown)
				{
					return;
				}
				HealingCooldown.PutOnCooldown();
			}
			if (GameMain.Client != null)
			{
				GameMain.Client.CreateEntityEvent(this, new Item.TreatmentEventData(character, targetLimb));
				return;
			}
			bool remove = false;
			foreach (ItemComponent ic in this.components)
			{
				if (ic.HasRequiredContainedItems(user, user == Character.Controlled, null))
				{
					ActionType conditionalActionType = (Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) < ic.DegreeOfSuccess(user)) ? ActionType.OnSuccess : ActionType.OnFailure;
					ic.PlaySound(conditionalActionType, user);
					ic.PlaySound(ActionType.OnUse, user);
					ic.WasUsed = true;
					ic.ApplyStatusEffects(conditionalActionType, 1f, character, targetLimb, character, user, null, 1f);
					ic.ApplyStatusEffects(ActionType.OnUse, 1f, character, targetLimb, character, user, null, 1f);
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember != null && networkMember.IsServer)
					{
						GameMain.NetworkMember.CreateEntityEvent(this, new Item.ApplyStatusEffectEventData(conditionalActionType, ic, character, targetLimb, character, null));
						GameMain.NetworkMember.CreateEntityEvent(this, new Item.ApplyStatusEffectEventData(ActionType.OnUse, ic, character, targetLimb, character, null));
					}
					if (ic.DeleteOnUse)
					{
						remove = true;
					}
				}
			}
			if (user != null)
			{
				AbilityApplyTreatment abilityItem = new AbilityApplyTreatment(user, character, this, targetLimb);
				user.CheckTalents(AbilityEffectType.OnApplyTreatment, abilityItem);
			}
			if (remove)
			{
				EntitySpawner spawner = Entity.Spawner;
				if (spawner == null)
				{
					return;
				}
				spawner.AddItemToRemoveQueue(this);
			}
		}

		// Token: 0x060019AE RID: 6574 RVA: 0x001027F4 File Offset: 0x001009F4
		public bool Combine(Item item, Character user)
		{
			if (item == this)
			{
				return false;
			}
			bool isCombined = false;
			foreach (ItemComponent ic in this.components)
			{
				if (ic.Combine(item, user))
				{
					isCombined = true;
				}
			}
			if (isCombined)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.CreateEntityEvent(this, new Item.CombineEventData(item));
				}
			}
			return isCombined;
		}

		// Token: 0x060019AF RID: 6575 RVA: 0x00102874 File Offset: 0x00100A74
		public void Drop(Character dropper, bool createNetworkEvent = true, bool setTransform = true)
		{
			if (createNetworkEvent && this.parentInventory != null && !this.parentInventory.Owner.Removed && !base.Removed && GameMain.NetworkMember != null && (GameMain.NetworkMember.IsServer || Character.Controlled == dropper))
			{
				this.parentInventory.CreateNetworkEvent();
				this.PositionUpdateInterval = 0f;
			}
			if (this.body != null)
			{
				this.IsActive = true;
				this.body.Enabled = true;
				this.body.PhysEnabled = true;
				this.body.ResetDynamics();
				if (dropper != null)
				{
					if (this.body.Removed)
					{
						DebugConsole.ThrowError("Failed to drop the item \"" + this.Name + "\" (body has been removed" + (base.Removed ? ", item has been removed)" : ")"), null, null, false, false);
					}
					else if (setTransform)
					{
						this.body.SetTransformIgnoreContacts(dropper.SimPosition, 0f, true);
					}
				}
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Drop(dropper, setTransform);
			}
			if (this.Container != null)
			{
				if (setTransform)
				{
					this.SetTransform(this.Container.SimPosition, 0f, true, true, null);
				}
				this.Container.RemoveContained(this);
				this.Container = null;
			}
			if (this.ParentInventory != null)
			{
				this.ParentInventory.RemoveItem(this);
				this.ParentInventory = null;
			}
			this.transformDirty = true;
			this.SetContainedItemPositions();
			Submarine.ForceVisibilityRecheck();
		}

		// Token: 0x17000673 RID: 1651
		// (get) Token: 0x060019B0 RID: 6576 RVA: 0x00102A18 File Offset: 0x00100C18
		public IEnumerable<Item> DroppedStack
		{
			get
			{
				IEnumerable<Item> enumerable = this.droppedStack;
				return enumerable ?? Enumerable.Empty<Item>();
			}
		}

		// Token: 0x060019B1 RID: 6577 RVA: 0x00102A38 File Offset: 0x00100C38
		public void CreateDroppedStack(IEnumerable<Item> items, bool allowClientExecute)
		{
			if (!allowClientExecute)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			int itemCount = items.Count<Item>();
			if (itemCount == 1)
			{
				return;
			}
			if (items.DistinctBy((Item it) => it.Prefab).Count<Item>() > 1)
			{
				DebugConsole.ThrowError("Attempted to create a dropped stack of multiple different items (" + string.Join<Item>(", ", items.DistinctBy((Item it) => it.Prefab)) + ")\n" + Environment.StackTrace, null, null, false, false);
				return;
			}
			if (items.Any((Item it) => it.body == null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to create a dropped stack for an item with no body (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(items.First<Item>().Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			if (items.None(null))
			{
				DebugConsole.ThrowError("Attempted to create a dropped stack of an empty list of items.\n" + Environment.StackTrace, null, null, false, false);
				return;
			}
			int maxStackSize = items.First<Item>().Prefab.MaxStackSize;
			if (itemCount > maxStackSize)
			{
				int i = 0;
				while ((float)i < MathF.Ceiling((float)(itemCount / maxStackSize)))
				{
					int startIndex = i * maxStackSize;
					items.ElementAt(startIndex).CreateDroppedStack(items.Skip(startIndex).Take(maxStackSize), allowClientExecute);
					i++;
				}
				return;
			}
			if (this.droppedStack == null)
			{
				this.droppedStack = new List<Item>();
			}
			foreach (Item item in items)
			{
				if (!this.droppedStack.Contains(item))
				{
					this.droppedStack.Add(item);
				}
			}
			this.SetDroppedStackItemStates();
		}

		// Token: 0x060019B2 RID: 6578 RVA: 0x00102C40 File Offset: 0x00100E40
		private void RemoveFromDroppedStack(bool allowClientExecute)
		{
			if (!allowClientExecute)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return;
				}
			}
			if (this.droppedStack == null)
			{
				return;
			}
			this.body.Enabled = (this.ParentInventory == null);
			this.isDroppedStackOwner = false;
			this.droppedStack.Remove(this);
			this.SetDroppedStackItemStates();
			this.droppedStack = null;
		}

		// Token: 0x060019B3 RID: 6579 RVA: 0x00102CA0 File Offset: 0x00100EA0
		private void SetDroppedStackItemStates()
		{
			if (this.droppedStack == null)
			{
				return;
			}
			bool isFirst = true;
			foreach (Item item in this.droppedStack)
			{
				item.droppedStack = this.droppedStack;
				item.isDroppedStackOwner = isFirst;
				if (item.body != null)
				{
					item.body.Enabled = (item.body.PhysEnabled = isFirst);
					if (isFirst)
					{
						item.IsActive = true;
						item.body.ResetDynamics();
					}
				}
				isFirst = false;
			}
		}

		// Token: 0x060019B4 RID: 6580 RVA: 0x00102D44 File Offset: 0x00100F44
		public IEnumerable<Item> GetStackedItems()
		{
			Item.<GetStackedItems>d__583 <GetStackedItems>d__ = new Item.<GetStackedItems>d__583(-2);
			<GetStackedItems>d__.<>4__this = this;
			return <GetStackedItems>d__;
		}

		// Token: 0x060019B5 RID: 6581 RVA: 0x00102D54 File Offset: 0x00100F54
		public void Equip(Character character)
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Tried to equip a removed item (" + this.Name + ").\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Equip(character);
			}
			CharacterHUD.RecreateHudTextsIfControlling(character);
		}

		// Token: 0x060019B6 RID: 6582 RVA: 0x00102DE0 File Offset: 0x00100FE0
		public void Unequip(Character character)
		{
			foreach (ItemComponent ic in this.components)
			{
				ic.Unequip(character);
			}
			CharacterHUD.RecreateHudTextsIfControlling(character);
		}

		// Token: 0x060019B7 RID: 6583 RVA: 0x00102E3C File Offset: 0x0010103C
		[return: TupleElementNames(new string[]
		{
			"obj",
			"property"
		})]
		public List<ValueTuple<object, SerializableProperty>> GetProperties<T>()
		{
			List<ValueTuple<object, SerializableProperty>> allProperties = new List<ValueTuple<object, SerializableProperty>>();
			List<SerializableProperty> itemProperties = SerializableProperty.GetProperties<T>(this);
			foreach (SerializableProperty itemProperty in itemProperties)
			{
				allProperties.Add(new ValueTuple<object, SerializableProperty>(this, itemProperty));
			}
			foreach (ItemComponent ic in this.components)
			{
				List<SerializableProperty> componentProperties = SerializableProperty.GetProperties<T>(ic);
				foreach (SerializableProperty componentProperty in componentProperties)
				{
					allProperties.Add(new ValueTuple<object, SerializableProperty>(ic, componentProperty));
				}
			}
			return allProperties;
		}

		// Token: 0x060019B8 RID: 6584 RVA: 0x00102F2C File Offset: 0x0010112C
		private void WritePropertyChange(IWriteMessage msg, Item.ChangePropertyEventData extraData, bool inGameEditableOnly)
		{
			List<ValueTuple<object, SerializableProperty>> allProperties = inGameEditableOnly ? this.GetInGameEditableProperties(true) : this.GetProperties<Editable>();
			SerializableProperty property = extraData.SerializableProperty;
			ISerializableEntity entity = extraData.Entity;
			if (property == null)
			{
				throw new ArgumentException("Failed to write propery value - property \"" + ((property == null) ? "null" : property.Name) + "\" is not serializable.");
			}
			if (allProperties.Count > 1)
			{
				if (allProperties.None(([TupleElementNames(new string[]
				{
					"obj",
					"property"
				})] ValueTuple<object, SerializableProperty> p) => p.Item2 == property && p.Item1 == entity))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find the property \"");
					defaultInterpolatedStringHandler.AppendFormatted(property.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" in \"");
					defaultInterpolatedStringHandler.AppendFormatted(entity.Name ?? "null");
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				msg.WriteIdentifier(property.Name.ToIdentifier());
			}
			object value = property.GetValue(entity);
			string stringVal = value as string;
			if (stringVal != null)
			{
				msg.WriteString(stringVal);
				return;
			}
			if (value is Identifier)
			{
				Identifier idValue = (Identifier)value;
				msg.WriteIdentifier(idValue);
				return;
			}
			if (value is float)
			{
				float floatVal = (float)value;
				msg.WriteSingle(floatVal);
				return;
			}
			if (value is int)
			{
				int intVal = (int)value;
				msg.WriteInt32(intVal);
				return;
			}
			if (value is bool)
			{
				bool boolVal = (bool)value;
				msg.WriteBoolean(boolVal);
				return;
			}
			if (value is Color)
			{
				Color color = (Color)value;
				msg.WriteByte(color.R);
				msg.WriteByte(color.G);
				msg.WriteByte(color.B);
				msg.WriteByte(color.A);
				return;
			}
			if (value is Vector2)
			{
				Vector2 vector2 = (Vector2)value;
				msg.WriteSingle(vector2.X);
				msg.WriteSingle(vector2.Y);
				return;
			}
			if (value is Vector3)
			{
				Vector3 vector3 = (Vector3)value;
				msg.WriteSingle(vector3.X);
				msg.WriteSingle(vector3.Y);
				msg.WriteSingle(vector3.Z);
				return;
			}
			if (value is Vector4)
			{
				Vector4 vector4 = (Vector4)value;
				msg.WriteSingle(vector4.X);
				msg.WriteSingle(vector4.Y);
				msg.WriteSingle(vector4.Z);
				msg.WriteSingle(vector4.W);
				return;
			}
			if (value is Point)
			{
				Point point = (Point)value;
				msg.WriteInt32(point.X);
				msg.WriteInt32(point.Y);
				return;
			}
			if (value is Rectangle)
			{
				Rectangle rect = (Rectangle)value;
				msg.WriteInt32(rect.X);
				msg.WriteInt32(rect.Y);
				msg.WriteInt32(rect.Width);
				msg.WriteInt32(rect.Height);
				return;
			}
			if (value is Enum)
			{
				msg.WriteInt32((int)value);
				return;
			}
			string[] a = value as string[];
			if (a != null)
			{
				msg.WriteInt32(a.Length);
				for (int i = 0; i < a.Length; i++)
				{
					msg.WriteString(a[i] ?? "");
				}
				return;
			}
			string str = "Serializing item properties of the type \"";
			Type type = value.GetType();
			throw new NotImplementedException(str + ((type != null) ? type.ToString() : null) + "\" not supported");
		}

		// Token: 0x060019B9 RID: 6585 RVA: 0x001032A0 File Offset: 0x001014A0
		[return: TupleElementNames(new string[]
		{
			"obj",
			"property"
		})]
		private List<ValueTuple<object, SerializableProperty>> GetInGameEditableProperties(bool ignoreConditions = false)
		{
			if (ignoreConditions)
			{
				return this.GetProperties<ConditionallyEditable>().Union(this.GetProperties<InGameEditable>()).ToList<ValueTuple<object, SerializableProperty>>();
			}
			return (from ce in this.GetProperties<ConditionallyEditable>()
			where ce.Item2.GetAttribute<ConditionallyEditable>().IsEditable(this)
			select ce).Union(this.GetProperties<InGameEditable>()).ToList<ValueTuple<object, SerializableProperty>>();
		}

		// Token: 0x060019BA RID: 6586 RVA: 0x001032F0 File Offset: 0x001014F0
		private void ReadPropertyChange(IReadMessage msg, bool inGameEditableOnly, Client sender = null)
		{
			List<ValueTuple<object, SerializableProperty>> allProperties = inGameEditableOnly ? this.GetInGameEditableProperties(true) : this.GetProperties<Editable>();
			if (allProperties.Count == 0)
			{
				return;
			}
			Identifier propertyIdentifier = msg.ReadIdentifier();
			int propertyIndex = allProperties.IndexOf(([TupleElementNames(new string[]
			{
				"obj",
				"property"
			})] ValueTuple<object, SerializableProperty> p) => p.Item2.Name == propertyIdentifier);
			if (propertyIndex < 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(96, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ");
				defaultInterpolatedStringHandler.AppendFormatted("ReadPropertyChange");
				defaultInterpolatedStringHandler.AppendLiteral(". Could not find the property \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(propertyIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" in item \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" (property count: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(allProperties.Count);
				defaultInterpolatedStringHandler.AppendLiteral(", in-game editable only: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(inGameEditableOnly);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			bool allowEditing = true;
			object parentObject = allProperties[propertyIndex].Item1;
			SerializableProperty property = allProperties[propertyIndex].Item2;
			if (inGameEditableOnly)
			{
				ItemComponent ic = parentObject as ItemComponent;
				if (ic != null && !ic.AllowInGameEditing)
				{
					allowEditing = false;
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				bool conditionAllowsEditing = true;
				ConditionallyEditable condition = property.GetAttribute<ConditionallyEditable>();
				if (condition != null)
				{
					conditionAllowsEditing = condition.IsEditable(this);
				}
				Item item = this.Container;
				CircuitBox cb = (item != null) ? item.GetComponent<CircuitBox>() : null;
				bool canAccess;
				if (cb != null && this.Container.CanClientAccess(sender))
				{
					canAccess = !cb.IsLocked();
				}
				else
				{
					canAccess = this.CanClientAccess(sender);
				}
				if (!canAccess || !conditionAllowsEditing)
				{
					allowEditing = false;
				}
			}
			bool? should = null;
			LuaCsSetup.Instance.EventService.PublishEvent<IEventItemReadPropertyChange>(delegate(IEventItemReadPropertyChange x)
			{
				bool? flag = x.OnItemReadPropertyChange(this, property, parentObject, allowEditing, sender);
				should = ((flag != null) ? flag : should);
			});
			if (should != null && should.Value)
			{
				return;
			}
			Type type = property.PropertyType;
			if (type == typeof(string))
			{
				string val = msg.ReadString();
				Editable editableAttribute = property.GetAttribute<Editable>();
				if (editableAttribute != null && editableAttribute.MaxLength > 0 && val.Length > editableAttribute.MaxLength)
				{
					val = val.Substring(0, editableAttribute.MaxLength);
				}
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val);
				}
			}
			else if (type == typeof(Identifier))
			{
				Identifier val2 = msg.ReadIdentifier();
				string logValue = val2.Value;
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val2);
				}
			}
			else if (type == typeof(float))
			{
				float val3 = msg.ReadSingle();
				string logValue = val3.ToString("G", CultureInfo.InvariantCulture);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val3);
				}
			}
			else if (type == typeof(int))
			{
				int val4 = msg.ReadInt32();
				string logValue = val4.ToString();
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val4);
				}
			}
			else if (type == typeof(bool))
			{
				bool val5 = msg.ReadBoolean();
				string logValue = val5.ToString();
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val5);
				}
			}
			else if (type == typeof(Color))
			{
				Color val6 = new Color(msg.ReadByte(), msg.ReadByte(), msg.ReadByte(), msg.ReadByte());
				string logValue = XMLExtensions.ColorToString(val6);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val6);
				}
			}
			else if (type == typeof(Vector2))
			{
				Vector2 val7 = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				string logValue = XMLExtensions.Vector2ToString(val7);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val7);
				}
			}
			else if (type == typeof(Vector3))
			{
				Vector3 val8 = new Vector3(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
				string logValue = XMLExtensions.Vector3ToString(val8, "G");
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val8);
				}
			}
			else if (type == typeof(Vector4))
			{
				Vector4 val9 = new Vector4(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
				string logValue = XMLExtensions.Vector4ToString(val9, "G");
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val9);
				}
			}
			else if (type == typeof(Point))
			{
				Point val10 = new Point(msg.ReadInt32(), msg.ReadInt32());
				string logValue = XMLExtensions.PointToString(val10);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val10);
				}
			}
			else if (type == typeof(Rectangle))
			{
				Rectangle val11 = new Rectangle(msg.ReadInt32(), msg.ReadInt32(), msg.ReadInt32(), msg.ReadInt32());
				string logValue = XMLExtensions.RectToString(val11);
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val11);
				}
			}
			else
			{
				if (!(type == typeof(string[])))
				{
					if (typeof(Enum).IsAssignableFrom(type))
					{
						int intVal = msg.ReadInt32();
						try
						{
							if (allowEditing)
							{
								property.TrySetValue(parentObject, Enum.ToObject(type, intVal));
								string logValue = property.GetValue(parentObject).ToString();
							}
							goto IL_753;
						}
						catch (Exception e)
						{
							string str = "Item.ReadPropertyChange:";
							string name = this.Name;
							string str2 = ":";
							Type type2 = type;
							string identifier = str + name + str2 + ((type2 != null) ? type2.ToString() : null);
							GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Warning;
							string[] array = new string[7];
							array[0] = "Failed to convert the int value \"";
							array[1] = intVal.ToString();
							array[2] = "\" to ";
							int num = 3;
							Type type3 = type;
							array[num] = ((type3 != null) ? type3.ToString() : null);
							array[4] = " (item ";
							array[5] = this.Name;
							array[6] = ")";
							GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, string.Concat(array));
							goto IL_753;
						}
					}
					return;
				}
				int arrayLength = msg.ReadInt32();
				string[] val12 = new string[arrayLength];
				for (int i = 0; i < arrayLength; i++)
				{
					val12[i] = msg.ReadString();
				}
				if (allowEditing)
				{
					property.TrySetValue(parentObject, val12);
				}
			}
			IL_753:
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				ISerializableEntity entity = parentObject as ISerializableEntity;
				if (entity != null)
				{
					GameMain.NetworkMember.CreateEntityEvent(this, new Item.ChangePropertyEventData(property, entity));
				}
			}
		}

		// Token: 0x060019BB RID: 6587 RVA: 0x00103AA4 File Offset: 0x00101CA4
		private void UpdateNetPosition(float deltaTime)
		{
			if (GameMain.Client == null)
			{
				return;
			}
			if (this.parentInventory == null && this.body != null && this.body.Enabled && !base.Removed)
			{
				Projectile component = this.GetComponent<Projectile>();
				if (component == null || !component.IsStuckToTarget)
				{
					this.IsActive = true;
					if (this.positionBuffer.Count > 0)
					{
						this.transformDirty = true;
					}
					Vector2 newPosition;
					Vector2 newVelocity;
					float newRotation;
					float newAngularVelocity;
					this.body.CorrectPosition<PosInfo>(this.positionBuffer, out newPosition, out newVelocity, out newRotation, out newAngularVelocity);
					this.body.LinearVelocity = newVelocity;
					this.body.AngularVelocity = newAngularVelocity;
					float distSqr = Vector2.DistanceSquared(newPosition, this.body.SimPosition);
					if (distSqr > 0.0001f || Math.Abs(newRotation - this.body.Rotation) > 0.01f)
					{
						this.body.TargetPosition = new Vector2?(newPosition);
						this.body.TargetRotation = new float?(newRotation);
						this.body.MoveToTargetPosition(true);
						if (distSqr > 100f)
						{
							base.Submarine = null;
							this.UpdateTransform();
						}
					}
					if (Level.IsPositionAboveLevel(this.WorldPosition) && base.Submarine == null)
					{
						Submarine newSub = Submarine.FindContainingInLocalCoordinates(ConvertUnits.ToDisplayUnits(this.body.SimPosition), 0f);
						if (newSub != null)
						{
							base.Submarine = newSub;
							this.FindHull();
						}
					}
					Vector2 displayPos = ConvertUnits.ToDisplayUnits(this.body.SimPosition);
					this.rect.X = (int)(displayPos.X - (float)this.rect.Width / 2f);
					this.rect.Y = (int)(displayPos.Y + (float)this.rect.Height / 2f);
					return;
				}
			}
			this.positionBuffer.Clear();
		}

		// Token: 0x060019BC RID: 6588 RVA: 0x00103C67 File Offset: 0x00101E67
		public static Item Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			return Item.Load(element, submarine, false, idRemap);
		}

		// Token: 0x060019BD RID: 6589 RVA: 0x00103C74 File Offset: 0x00101E74
		public static Item Load(ContentXElement element, Submarine submarine, bool createNetworkEvent, IdRemap idRemap)
		{
			string name = element.GetAttribute("name").Value;
			Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (string.IsNullOrWhiteSpace(name) && identifier.IsEmpty)
			{
				string errorMessage = "Failed to load an item (both name and identifier were null):\n" + element.ToString();
				DebugConsole.ThrowError(errorMessage, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Item.Load:NameAndIdentifierNull", GameAnalyticsManager.ErrorSeverity.Error, errorMessage);
				return null;
			}
			Identifier pendingSwap = element.GetAttributeIdentifier("pendingswap", Identifier.Empty);
			ItemPrefab appliedSwap = null;
			ItemPrefab oldPrefab = null;
			if (!pendingSwap.IsEmpty)
			{
				Level loaded = Level.Loaded;
				if (loaded == null || loaded.Type != LevelData.LevelType.Outpost)
				{
					oldPrefab = ItemPrefab.Find(name, identifier);
					appliedSwap = ItemPrefab.Find(string.Empty, pendingSwap);
					identifier = pendingSwap;
					pendingSwap = Identifier.Empty;
				}
			}
			ItemPrefab prefab = ItemPrefab.Find(name, identifier);
			if (prefab == null)
			{
				return null;
			}
			string key = "rect";
			Rectangle empty = Rectangle.Empty;
			Rectangle rect = element.GetAttributeRect(key, empty);
			Vector2 centerPos = new Vector2((float)(rect.X + rect.Width / 2), (float)(rect.Y - rect.Height / 2));
			if (appliedSwap != null)
			{
				rect.Width = (int)(prefab.Sprite.size.X * prefab.Scale);
				rect.Height = (int)(prefab.Sprite.size.Y * prefab.Scale);
			}
			else if (rect.Width == 0 && rect.Height == 0)
			{
				rect.Width = (int)(prefab.Size.X * prefab.Scale);
				rect.Height = (int)(prefab.Size.Y * prefab.Scale);
			}
			Item item = new Item(rect, prefab, submarine, false, idRemap.GetOffsetId(element))
			{
				Submarine = submarine,
				linkedToID = new List<ushort>(),
				PendingItemSwap = (pendingSwap.IsEmpty ? null : (MapEntityPrefab.Find(pendingSwap.Value, null, true) as ItemPrefab))
			};
			foreach (XAttribute attribute in (((appliedSwap != null) ? appliedSwap.ConfigElement : null) ?? element).Attributes())
			{
				SerializableProperty property;
				if (item.SerializableProperties.TryGetValue(attribute.NameAsIdentifier(), out property))
				{
					bool shouldBeLoaded = false;
					foreach (Serialize propertyAttribute in property.Attributes.OfType<Serialize>())
					{
						if (propertyAttribute.IsSaveable == IsPropertySaveable.Yes)
						{
							shouldBeLoaded = true;
							break;
						}
					}
					if (shouldBeLoaded)
					{
						object prevValue = property.GetValue(item);
						property.TrySetValue(item, attribute.Value);
						if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer && property.Attributes.OfType<Editable>().Any<Editable>() && (submarine == null || !submarine.Loading) && !(property.Name == "Tags") && !(property.Name == "Condition") && !(property.Name == "Description"))
						{
							object value = property.GetValue(item);
							if (value != null && !value.Equals(prevValue))
							{
								GameMain.NetworkMember.CreateEntityEvent(item, new Item.ChangePropertyEventData(property, item));
							}
						}
					}
				}
			}
			item.OnInsertedEffectsAppliedOnPreviousRound = item.OnInsertedEffectsApplied;
			item.ParseLinks(element, idRemap);
			bool thisIsOverride = element.GetAttributeBool("isoverride", false);
			bool isItemSwap = appliedSwap != null;
			bool usePrefabValues = thisIsOverride != ItemPrefab.Prefabs.IsOverride(prefab) || isItemSwap;
			List<ItemComponent> unloadedComponents = new List<ItemComponent>(item.components);
			using (IEnumerator<ContentXElement> enumerator3 = element.Elements().GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					ContentXElement subElement = enumerator3.Current;
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (!(a == "upgrade"))
					{
						if (!(a == "itemstats"))
						{
							ItemComponent component = unloadedComponents.Find((ItemComponent x) => x.Name == subElement.Name.ToString());
							if (component != null)
							{
								component.Load(subElement, usePrefabValues, idRemap, isItemSwap);
								unloadedComponents.Remove(component);
							}
						}
						else
						{
							item.StatManager.Load(subElement);
						}
					}
					else
					{
						Identifier upgradeIdentifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						UpgradePrefab upgradePrefab = UpgradePrefab.Find(upgradeIdentifier);
						int level = subElement.GetAttributeInt("level", 1);
						if (upgradePrefab != null)
						{
							item.AddUpgrade(new Upgrade(item, upgradePrefab, level, isItemSwap ? null : subElement), false);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
							defaultInterpolatedStringHandler.AppendLiteral("An upgrade with identifier \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(upgradeIdentifier);
							defaultInterpolatedStringHandler.AppendLiteral("\" on ");
							defaultInterpolatedStringHandler.AppendFormatted(item.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" was not found. ");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "It's effect will not be applied and won't be saved after the round ends.", null);
						}
					}
				}
			}
			if (usePrefabValues && !isItemSwap)
			{
				item.Scale = prefab.ConfigElement.GetAttributeFloat(item.scale, new string[]
				{
					"scale",
					"Scale"
				});
			}
			item.Upgrades.ForEach(delegate(Upgrade upgrade)
			{
				upgrade.ApplyUpgrade();
			});
			Identifier[] availableSwapIds = element.GetAttributeIdentifierArray("availableswaps", Array.Empty<Identifier>(), true);
			foreach (Identifier swapId in availableSwapIds)
			{
				ItemPrefab swapPrefab = ItemPrefab.Find(string.Empty, swapId);
				if (swapPrefab != null)
				{
					item.AvailableSwaps.Add(swapPrefab);
				}
			}
			if (element.GetAttributeBool("markedfordeconstruction", false))
			{
				Item._deconstructItems.Add(item);
			}
			float prevRotation = item.Rotation;
			if (element.GetAttributeBool("flippedx", false))
			{
				item.FlipX(false, true);
			}
			if (element.GetAttributeBool("flippedy", false))
			{
				item.FlipY(false, true);
			}
			item.Rotation = prevRotation;
			if (appliedSwap != null)
			{
				item.SpriteDepth = element.GetAttributeFloat("spritedepth", item.SpriteDepth);
				Item item2 = item;
				string key2 = "spritecolor";
				Color color = item.SpriteColor;
				item2.SpriteColor = element.GetAttributeColor(key2, color);
				item.Rotation = element.GetAttributeFloat("rotation", item.Rotation);
				item.PurchasedNewSwap = element.GetAttributeBool("purchasednewswap", false);
				float scaleRelativeToPrefab = element.GetAttributeFloat(item.scale, new string[]
				{
					"scale",
					"Scale"
				}) / oldPrefab.Scale;
				item.Scale *= scaleRelativeToPrefab;
				if (oldPrefab.SwappableItem != null && prefab.SwappableItem != null)
				{
					Vector2 oldRelativeOrigin = (oldPrefab.SwappableItem.SwapOrigin - oldPrefab.Size / 2f) * element.GetAttributeFloat(item.scale, new string[]
					{
						"scale",
						"Scale"
					});
					oldRelativeOrigin.Y = -oldRelativeOrigin.Y;
					oldRelativeOrigin = MathUtils.RotatePoint(oldRelativeOrigin, -item.RotationRad);
					Vector2 oldOrigin = centerPos + oldRelativeOrigin;
					Vector2 relativeOrigin = (prefab.SwappableItem.SwapOrigin - prefab.Size / 2f) * item.Scale;
					relativeOrigin.Y = -relativeOrigin.Y;
					relativeOrigin = MathUtils.RotatePoint(relativeOrigin, -item.RotationRad);
					Vector2 origin = new Vector2((float)(rect.X + rect.Width / 2), (float)(rect.Y - rect.Height / 2)) + relativeOrigin;
					Item item3 = item;
					item3.rect.Location = item3.rect.Location - (origin - oldOrigin).ToPoint();
				}
				if (item.PurchasedNewSwap)
				{
					SwappableItem swappableItem = appliedSwap.SwappableItem;
					if (!string.IsNullOrEmpty((swappableItem != null) ? swappableItem.SpawnWithId : null))
					{
						ItemContainer container = item.GetComponent<ItemContainer>();
						if (container != null)
						{
							container.SpawnWithId = appliedSwap.SwappableItem.SpawnWithId;
						}
					}
				}
				item.PurchasedNewSwap = false;
			}
			Version savedVersion = (submarine != null) ? submarine.Info.GameVersion : null;
			XDocument document = element.Document;
			if (((document != null) ? document.Root : null) != null && element.Document.Root.Name.ToString().Equals("gamesession", StringComparison.OrdinalIgnoreCase))
			{
				savedVersion = new Version(element.Document.Root.GetAttributeString("version", "0.0.0.0"));
			}
			float prevCondition = item.condition;
			if (savedVersion != null)
			{
				SerializableProperty.UpgradeGameVersion(item, item.Prefab.ConfigElement, savedVersion);
			}
			if (element.GetAttribute("conditionpercentage") != null)
			{
				item.condition = element.GetAttributeFloat("conditionpercentage", 100f) / 100f * item.MaxCondition;
			}
			else
			{
				item.condition = element.GetAttributeFloat("condition", item.condition);
				if (item.condition > 0f)
				{
					bool wasFullCondition = prevCondition >= item.Prefab.Health;
					if (wasFullCondition)
					{
						item.condition = item.MaxCondition;
					}
					item.condition = MathHelper.Clamp(item.condition, 0f, item.MaxCondition);
				}
			}
			item.lastSentCondition = (item.prevCondition = item.condition);
			item.RecalculateConditionValues();
			item.SetActiveSprite();
			foreach (ItemComponent component2 in item.components)
			{
				if (component2.Parent != null && component2.InheritParentIsActive)
				{
					component2.IsActive = component2.Parent.IsActive;
				}
				component2.OnItemLoaded();
			}
			item.FullyInitialized = true;
			return item;
		}

		// Token: 0x060019BE RID: 6590 RVA: 0x00104724 File Offset: 0x00102924
		private void ReplaceFromNetwork(ItemPrefab replacement, ushort newId)
		{
			this.Replace(replacement, Option.Some<ushort>(newId), false);
		}

		// Token: 0x060019BF RID: 6591 RVA: 0x00104734 File Offset: 0x00102934
		public void ReplaceWithLinkedItems(ItemPrefab replacement)
		{
			Option.UnspecifiedNone none = Option.None;
			this.Replace(replacement, none, true);
		}

		// Token: 0x060019C0 RID: 6592 RVA: 0x00104758 File Offset: 0x00102958
		private void Replace(ItemPrefab replacement, Option<ushort> newId, bool createEntityEvent)
		{
			Vector2 centerPos = this.Position;
			Item newItem = new Item(replacement, this.Position, base.Submarine, newId.Fallback(0), true)
			{
				SpriteDepth = base.SpriteDepth,
				SpriteColor = this.SpriteColor,
				Rotation = this.Rotation
			};
			if (base.FlippedX)
			{
				newItem.FlipX(false, false);
			}
			if (base.FlippedY)
			{
				newItem.FlipY(false, false);
			}
			float scaleRelativeToPrefab = this.Scale / this.Prefab.Scale;
			newItem.Scale *= scaleRelativeToPrefab;
			if (this.Prefab.SwappableItem != null && replacement.SwappableItem != null)
			{
				Vector2 oldRelativeOrigin = (this.Prefab.SwappableItem.SwapOrigin - this.Prefab.Size / 2f) * this.scale;
				oldRelativeOrigin.Y = -oldRelativeOrigin.Y;
				oldRelativeOrigin = MathUtils.RotatePoint(oldRelativeOrigin, -this.RotationRad);
				Vector2 oldOrigin = centerPos + oldRelativeOrigin;
				Vector2 relativeOrigin = (this.Prefab.SwappableItem.SwapOrigin - this.Prefab.Size / 2f) * this.Scale;
				relativeOrigin.Y = -relativeOrigin.Y;
				relativeOrigin = MathUtils.RotatePoint(relativeOrigin, -this.RotationRad);
				Vector2 origin = new Vector2((float)this.rect.X + (float)this.rect.Width / 2f, (float)this.rect.Y - (float)this.rect.Height / 2f) + relativeOrigin;
				Item item = newItem;
				item.rect.Location = item.rect.Location - (origin - oldOrigin).ToPoint();
			}
			SwappableItem swappableItem = this.Prefab.SwappableItem;
			if (!string.IsNullOrEmpty((swappableItem != null) ? swappableItem.SpawnWithId : null))
			{
				ItemContainer newContainer = newItem.GetComponent<ItemContainer>();
				if (newContainer != null)
				{
					newContainer.SpawnWithId = this.Prefab.SwappableItem.SpawnWithId;
				}
			}
			using (List<ItemComponent>.Enumerator enumerator = this.components.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemComponent originalComponent = enumerator.Current;
					List<ItemComponent> originalComponents = (from c in this.components
					where c.GetType() == originalComponent.GetType()
					select c).ToList<ItemComponent>();
					List<ItemComponent> newComponents = (from c in newItem.components
					where c.GetType() == originalComponent.GetType()
					select c).ToList<ItemComponent>();
					int originalIndex = originalComponents.IndexOf(originalComponent);
					if (originalIndex < newComponents.Count)
					{
						ItemComponent newComponent = newComponents[originalIndex];
						foreach (KeyValuePair<Identifier, SerializableProperty> originalProperty in originalComponent.SerializableProperties)
						{
							if (!originalProperty.Value.OverridePrefabValues)
							{
								Editable attribute = originalProperty.Value.GetAttribute<Editable>();
								if (attribute == null || !attribute.TransferToSwappedItem)
								{
									continue;
								}
							}
							newComponent.SerializableProperties[originalProperty.Key].TrySetValue(newComponent, originalProperty.Value.GetValue(originalComponent));
						}
					}
				}
			}
			foreach (MapEntity linked in this.linkedTo)
			{
				newItem.linkedTo.Add(linked);
				if (linked.linkedTo.Contains(this))
				{
					linked.linkedTo.Add(newItem);
				}
			}
			ConnectionPanel thisConnectionPanel = this.GetComponent<ConnectionPanel>();
			ConnectionPanel newConnectionPanel = newItem.GetComponent<ConnectionPanel>();
			if (thisConnectionPanel != null && newConnectionPanel != null)
			{
				foreach (Connection connection in thisConnectionPanel.Connections)
				{
					foreach (Wire wire in connection.Wires)
					{
						int wireConnectionIndex = wire.Connections.IndexOf(connection);
						wire.RemoveConnection(this);
						int thisConnectionIndex = connection.ConnectionPanel.Connections.IndexOf(connection);
						if (thisConnectionIndex < 0 || thisConnectionIndex >= newConnectionPanel.Connections.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(161, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to move a wire from the connection ");
							defaultInterpolatedStringHandler.AppendFormatted(connection.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" when swapping the item ");
							defaultInterpolatedStringHandler.AppendFormatted(this.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" with ");
							defaultInterpolatedStringHandler.AppendFormatted(newItem.Name);
							defaultInterpolatedStringHandler.AppendLiteral(". The new item probably does not have the same number of connections as the previous one.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							Connection newConnection = newConnectionPanel.Connections[thisConnectionIndex];
							wire.Connect(newConnection, wireConnectionIndex, false, false);
							newConnection.ConnectWire(wire);
						}
					}
				}
			}
			if (newId.IsNone() && replacement.SwappableItem != null)
			{
				Dictionary<Item, ItemPrefab> connectedItemsToSwap = newItem.GetConnectedItemsToSwap(replacement.SwappableItem);
				foreach (KeyValuePair<Item, ItemPrefab> kvp in connectedItemsToSwap)
				{
					Item itemToSwap = kvp.Key;
					ItemPrefab swapTo = kvp.Value;
					Item item2 = itemToSwap;
					ItemPrefab replacement2 = swapTo;
					Option.UnspecifiedNone none = Option.None;
					item2.Replace(replacement2, none, createEntityEvent);
				}
			}
			this.Remove();
		}

		// Token: 0x060019C1 RID: 6593 RVA: 0x00104D7C File Offset: 0x00102F7C
		public Dictionary<Item, ItemPrefab> GetConnectedItemsToSwap(SwappableItem swappingTo)
		{
			Dictionary<Item, ItemPrefab> itemsToSwap = new Dictionary<Item, ItemPrefab>();
			foreach (ValueTuple<Identifier, Identifier> valueTuple in swappingTo.ConnectedItemsToSwap)
			{
				Identifier requiredTag = valueTuple.Item1;
				Identifier swapTo = valueTuple.Item2;
				ItemPrefab replacement = MapEntityPrefab.FindByIdentifier(swapTo) as ItemPrefab;
				if (replacement != null)
				{
					foreach (MapEntity linked in this.linkedTo)
					{
						Item linkedItem = linked as Item;
						if (linkedItem != null && linkedItem.HasTag(requiredTag))
						{
							itemsToSwap.Add(linkedItem, replacement);
						}
					}
					ConnectionPanel connectionPanel = this.GetComponent<ConnectionPanel>();
					if (connectionPanel != null)
					{
						foreach (Connection c in connectionPanel.Connections)
						{
							foreach (ItemComponent connectedComponent in this.GetConnectedComponentsRecursive<ItemComponent>(c, false, true))
							{
								if (!itemsToSwap.ContainsKey(connectedComponent.Item) && connectedComponent.Item.HasTag(requiredTag))
								{
									itemsToSwap.Add(connectedComponent.Item, replacement);
								}
							}
						}
					}
				}
			}
			return itemsToSwap;
		}

		// Token: 0x060019C2 RID: 6594 RVA: 0x00104F40 File Offset: 0x00103140
		public override XElement Save(XElement parentElement)
		{
			XElement element = new XElement("Item");
			element.Add(new object[]
			{
				new XAttribute("name", this.Prefab.OriginalName),
				new XAttribute("identifier", this.Prefab.Identifier),
				new XAttribute("ID", this.ID),
				new XAttribute("markedfordeconstruction", Item._deconstructItems.Contains(this))
			});
			if (this.PendingItemSwap != null)
			{
				element.Add(new XAttribute("pendingswap", this.PendingItemSwap.Identifier));
			}
			if (this.Rotation != 0f)
			{
				element.Add(new XAttribute("rotation", this.Rotation));
			}
			if (ItemPrefab.Prefabs.IsOverride(this.Prefab))
			{
				element.Add(new XAttribute("isoverride", "true"));
			}
			if (base.FlippedX)
			{
				element.Add(new XAttribute("flippedx", true));
			}
			if (base.FlippedY)
			{
				element.Add(new XAttribute("flippedy", true));
			}
			if (this.AvailableSwaps.Any<ItemPrefab>())
			{
				element.Add(new XAttribute("availableswaps", string.Join<Identifier>(',', from s in this.AvailableSwaps
				select s.Identifier)));
			}
			if (!MathUtils.NearlyEqual(this.healthMultiplier, 1f, 0.0001f))
			{
				element.Add(new XAttribute("healthmultiplier", this.HealthMultiplier.ToString("G", CultureInfo.InvariantCulture)));
			}
			Item item = this.RootContainer ?? this;
			Vector2 subPosition = (base.Submarine == null) ? Vector2.Zero : base.Submarine.HiddenSubPosition;
			int width = base.ResizeHorizontal ? this.rect.Width : this.defaultRect.Width;
			int height = base.ResizeVertical ? this.rect.Height : this.defaultRect.Height;
			element.Add(new XAttribute("rect", string.Concat(new string[]
			{
				((int)((float)this.rect.X - subPosition.X)).ToString(),
				",",
				((int)((float)this.rect.Y - subPosition.Y)).ToString(),
				",",
				width.ToString(),
				",",
				height.ToString()
			})));
			if (this.linkedTo != null && this.linkedTo.Count > 0)
			{
				bool isOutpost = base.Submarine != null && base.Submarine.Info.IsOutpost;
				IEnumerable<MapEntity> saveableLinked = from l in this.linkedTo
				where l.ShouldBeSaved && l.Removed == this.Removed && (l.Submarine == null || l.Submarine.Info.IsOutpost == isOutpost)
				select l;
				element.Add(new XAttribute("linked", string.Join(",", from l in saveableLinked
				select l.ID.ToString())));
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
			foreach (ItemComponent ic in this.components)
			{
				ic.Save(element);
			}
			foreach (Upgrade upgrade in this.Upgrades)
			{
				upgrade.Save(element);
			}
			ItemStatManager itemStatManager = this.statManager;
			if (itemStatManager != null)
			{
				itemStatManager.Save(element);
			}
			element.Add(new XAttribute("conditionpercentage", this.ConditionPercentage.ToString("G", CultureInfo.InvariantCulture)));
			XAttribute conditionAttribute = element.GetAttribute("condition", StringComparison.OrdinalIgnoreCase);
			if (conditionAttribute != null)
			{
				conditionAttribute.Remove();
			}
			parentElement.Add(element);
			return element;
		}

		// Token: 0x060019C3 RID: 6595 RVA: 0x001053E0 File Offset: 0x001035E0
		public virtual void Reset()
		{
			Holdable holdable = this.GetComponent<Holdable>();
			bool wasAttached = holdable != null && holdable.Attached;
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, this.Prefab.ConfigElement);
			this.Sprite.ReloadXML();
			base.SpriteDepth = this.Sprite.Depth;
			this.condition = this.MaxCondition;
			this.components.ForEach(delegate(ItemComponent c)
			{
				c.Reset();
			});
			if (wasAttached)
			{
				holdable.AttachToWall();
			}
		}

		// Token: 0x060019C4 RID: 6596 RVA: 0x00105478 File Offset: 0x00103678
		public override void OnMapLoaded()
		{
			this.FindHull();
			foreach (ItemComponent ic in this.components)
			{
				ic.OnMapLoaded();
			}
		}

		// Token: 0x060019C5 RID: 6597 RVA: 0x001054D4 File Offset: 0x001036D4
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			foreach (ItemComponent ic in this.components)
			{
				ic.ShallowRemove();
			}
			this.RemoveFromLists();
			if (this.body != null)
			{
				this.body.Remove();
				this.body = null;
			}
		}

		// Token: 0x060019C6 RID: 6598 RVA: 0x0010554C File Offset: 0x0010374C
		public override void Remove()
		{
			if (base.Removed)
			{
				DebugConsole.ThrowError("Attempting to remove an already removed item (" + this.Name + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			DebugConsole.Log(string.Concat(new string[]
			{
				"Removing item ",
				this.Name,
				" (ID: ",
				this.ID.ToString(),
				")"
			}));
			base.Remove();
			foreach (Character character in Character.CharacterList)
			{
				if (character.SelectedItem == this)
				{
					character.SelectedItem = null;
				}
				if (character.SelectedSecondaryItem == this)
				{
					character.SelectedSecondaryItem = null;
				}
			}
			Door door = this.GetComponent<Door>();
			Ladder ladder = this.GetComponent<Ladder>();
			if (door != null || ladder != null)
			{
				foreach (WayPoint wp in WayPoint.WayPointList)
				{
					if (door != null && wp.ConnectedDoor == door)
					{
						wp.ConnectedGap = null;
					}
					if (ladder != null && wp.Ladders == ladder)
					{
						wp.Ladders = null;
					}
				}
			}
			Dictionary<string, Connection> dictionary = this.connections;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			if (this.parentInventory != null)
			{
				CharacterInventory characterInventory = this.parentInventory as CharacterInventory;
				if (characterInventory != null)
				{
					characterInventory.RemoveItem(this, true);
				}
				else
				{
					this.parentInventory.RemoveItem(this);
				}
				this.parentInventory = null;
			}
			foreach (ItemComponent ic in this.components)
			{
				ic.Remove();
				ic.GuiFrame = null;
			}
			this.RemoveFromLists();
			if (this.body != null)
			{
				this.body.Remove();
				this.body = null;
			}
			this.CurrentHull = null;
			if (this.StaticFixtures != null)
			{
				foreach (Fixture fixture in this.StaticFixtures)
				{
					Body body = fixture.Body;
					if (((body != null) ? body.World : null) != null)
					{
						fixture.Body.Remove(fixture);
					}
				}
				this.StaticFixtures.Clear();
			}
			foreach (Item it in Item.ItemList)
			{
				if (it.linkedTo.Contains(this))
				{
					it.linkedTo.Remove(this);
				}
			}
			this.RemoveProjSpecific();
		}

		// Token: 0x060019C7 RID: 6599 RVA: 0x00105834 File Offset: 0x00103A34
		private void RemoveFromLists()
		{
			Item.ItemList.Remove(this);
			Item._dangerousItems.Remove(this);
			Item._repairableItems.Remove(this);
			Item._sonarVisibleItems.Remove(this);
			Item._cleanableItems.Remove(this);
			Item._deconstructItems.Remove(this);
			Item._turretTargetItems.Remove(this);
			Item._chairItems.Remove(this);
			this.RemoveFromDroppedStack(true);
		}

		// Token: 0x060019C8 RID: 6600 RVA: 0x001058A8 File Offset: 0x00103AA8
		private void RemoveProjSpecific()
		{
			if (Inventory.DraggingItems.Contains(this))
			{
				Inventory.DraggingItems.Clear();
				Inventory.DraggingSlot = null;
			}
		}

		// Token: 0x060019C9 RID: 6601 RVA: 0x001058C8 File Offset: 0x00103AC8
		public static void RemoveByPrefab(ItemPrefab prefab)
		{
			if (Item.ItemList == null)
			{
				return;
			}
			List<Item> list = new List<Item>(Item.ItemList);
			foreach (Item item in list)
			{
				if (item.Prefab == prefab)
				{
					item.Remove();
				}
			}
		}

		// Token: 0x060019CB RID: 6603 RVA: 0x00105AC4 File Offset: 0x00103CC4
		[CompilerGenerated]
		internal static float <Draw>g__GetHeldItemDepth|45_2(LimbType limb, Holdable holdable, float depth)
		{
			bool flag;
			if (holdable == null)
			{
				flag = (null != null);
			}
			else
			{
				Character picker = holdable.Picker;
				flag = (((picker != null) ? picker.AnimController : null) != null);
			}
			if (!flag)
			{
				return depth;
			}
			float limbDepthOffset = 1E-06f;
			float depthOffset = holdable.Picker.AnimController.GetDepthOffset();
			Limb holdLimb = holdable.Picker.AnimController.GetLimb((limb == LimbType.RightHand) ? LimbType.RightArm : LimbType.LeftArm, true, false, false);
			if (((holdLimb != null) ? holdLimb.ActiveSprite : null) != null)
			{
				depth = holdLimb.ActiveSprite.Depth + depthOffset + limbDepthOffset * 2f * (float)((limb == LimbType.RightHand) ? 1 : -1);
				foreach (WearableSprite wearableSprite in holdLimb.WearingItems)
				{
					if (!wearableSprite.InheritLimbDepth && wearableSprite.Sprite != null)
					{
						depth = ((limb == LimbType.RightHand) ? Math.Max(wearableSprite.Sprite.Depth + limbDepthOffset, depth) : Math.Min(wearableSprite.Sprite.Depth - limbDepthOffset, depth));
					}
				}
				Limb head = holdable.Picker.AnimController.GetLimb(LimbType.Head, true, false, false);
				if (((head != null) ? head.Sprite : null) != null)
				{
					depth = ((limb == LimbType.RightHand) ? Math.Min(head.Sprite.Depth + depthOffset - limbDepthOffset, depth) : Math.Max(head.Sprite.Depth + depthOffset + limbDepthOffset, depth));
				}
			}
			return depth;
		}

		// Token: 0x060019CC RID: 6604 RVA: 0x00105C2C File Offset: 0x00103E2C
		[CompilerGenerated]
		internal static Vector2 <Draw>g__GetSpriteOrigin|45_0(Sprite sprite)
		{
			Vector2 origin = sprite.Origin;
			if ((sprite.effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally)
			{
				origin.X = (float)sprite.SourceRect.Width - origin.X;
			}
			if ((sprite.effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically)
			{
				origin.Y = (float)sprite.SourceRect.Height - origin.Y;
			}
			return origin;
		}

		// Token: 0x060019CD RID: 6605 RVA: 0x00105C8C File Offset: 0x00103E8C
		[CompilerGenerated]
		private Color <Draw>g__GetSpriteColor|45_1(Color defaultColor, ref Item.<>c__DisplayClass45_0 A_2)
		{
			Color? overrideColor = A_2.overrideColor;
			if (overrideColor != null)
			{
				return overrideColor.GetValueOrDefault();
			}
			if (!(base.IsIncludedInSelection & A_2.editing))
			{
				return this.GetSpriteColor(new Color?(defaultColor), true);
			}
			return GUIStyle.Blue;
		}

		// Token: 0x060019D1 RID: 6609 RVA: 0x00105D90 File Offset: 0x00103F90
		[CompilerGenerated]
		internal static LocalizedString <CreateContainerTagItemListPopup>g__ProbabilityToPercentage|55_0(float probability)
		{
			return TextManager.GetWithVariable("percentageformat", "[value]", MathF.Round(probability * 100f, 1).ToString(CultureInfo.InvariantCulture), FormatCapitals.No);
		}

		// Token: 0x060019D2 RID: 6610 RVA: 0x00105DCC File Offset: 0x00103FCC
		[CompilerGenerated]
		private bool <UpdateHUD>g__DrawHud|62_0(ItemComponent ic, ref Item.<>c__DisplayClass62_0 A_2)
		{
			if (!ic.ShouldDrawHUD(A_2.character))
			{
				return false;
			}
			if (A_2.character.HasEquippedItem(this, null, null))
			{
				return ic.DrawHudWhenEquipped;
			}
			return ic.CanBeSelected && ic.HasRequiredItems(A_2.character, false, null);
		}

		// Token: 0x060019D4 RID: 6612 RVA: 0x00105E38 File Offset: 0x00104038
		[CompilerGenerated]
		private Exception <ClientEventWrite>g__error|73_0(string reason)
		{
			string errorMsg = "Failed to write a network event for the item \"" + this.Name + "\" - " + reason;
			GameAnalyticsManager.AddErrorEventOnce("Item.ClientWrite:" + this.Name, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			return new Exception(errorMsg);
		}

		// Token: 0x060019D8 RID: 6616 RVA: 0x00105EAF File Offset: 0x001040AF
		[CompilerGenerated]
		internal static bool <ConditionalMatches>g__MatchesComponent|529_0(ItemComponent comp, PropertyConditional cond)
		{
			return comp.Name == cond.TargetItemComponent;
		}

		// Token: 0x060019D9 RID: 6617 RVA: 0x00105EC2 File Offset: 0x001040C2
		[CompilerGenerated]
		private void <SetCondition>g__SetPreviousCondition|535_0(ref Item.<>c__DisplayClass535_0 A_1)
		{
			this.LastConditionChange = this.condition - this.prevCondition;
			this.ConditionLastUpdated = Timing.TotalTime;
			this.prevCondition = this.condition;
			A_1.wasPreviousConditionChanged = true;
		}

		// Token: 0x060019DA RID: 6618 RVA: 0x00105EF8 File Offset: 0x001040F8
		[CompilerGenerated]
		internal static void <SetCondition>g__flagChangedConnections|535_1(Dictionary<string, Connection> connections)
		{
			if (connections == null)
			{
				return;
			}
			foreach (Connection c in connections.Values)
			{
				if (c.IsPower)
				{
					Powered.ChangedConnections.Add(c);
					foreach (Connection conn in c.Recipients)
					{
						Powered.ChangedConnections.Add(conn);
					}
				}
			}
		}

		// Token: 0x060019DC RID: 6620 RVA: 0x00105FC8 File Offset: 0x001041C8
		[CompilerGenerated]
		internal static IEnumerable<Connection> <GetConnectedComponentsRecursive>g__GetRecipients|558_0<T>(Connection c) where T : ItemComponent
		{
			Item.<<GetConnectedComponentsRecursive>g__GetRecipients|558_0>d<T> <<GetConnectedComponentsRecursive>g__GetRecipients|558_0>d = new Item.<<GetConnectedComponentsRecursive>g__GetRecipients|558_0>d<T>(-2);
			<<GetConnectedComponentsRecursive>g__GetRecipients|558_0>d.<>3__c = c;
			return <<GetConnectedComponentsRecursive>g__GetRecipients|558_0>d;
		}

		// Token: 0x060019DD RID: 6621 RVA: 0x00105FE8 File Offset: 0x001041E8
		[CompilerGenerated]
		private void <GetConnectedComponentsRecursive>g__CheckRecipient|558_1<T>(Connection recipient, ref Item.<>c__DisplayClass558_0<T> A_2) where T : ItemComponent
		{
			WifiComponent wifiComponent = recipient.Item.GetComponent<WifiComponent>();
			if (wifiComponent != null && wifiComponent.CanTransmit(false))
			{
				foreach (WifiComponent wifiReceiver in wifiComponent.GetTransmittersInRange())
				{
					List<Connection> receiverConnections = wifiReceiver.Item.Connections;
					if (receiverConnections != null)
					{
						foreach (Connection wifiOutput in receiverConnections)
						{
							if (wifiOutput.IsOutput != recipient.IsOutput && !A_2.alreadySearched.Contains(wifiOutput))
							{
								this.GetConnectedComponentsRecursive<T>(wifiOutput, A_2.alreadySearched, A_2.connectedComponents, A_2.ignoreInactiveRelays, A_2.allowTraversingBackwards);
							}
						}
					}
				}
			}
			recipient.Item.GetConnectedComponentsRecursive<T>(recipient, A_2.alreadySearched, A_2.connectedComponents, A_2.ignoreInactiveRelays, A_2.allowTraversingBackwards);
		}

		// Token: 0x060019DE RID: 6622 RVA: 0x001060FC File Offset: 0x001042FC
		[CompilerGenerated]
		private void <GetConnectedComponentsRecursive>g__searchFromAToB|558_2<T>(Identifier connectionEndA, Identifier connectionEndB, ref Item.<>c__DisplayClass558_0<T> A_3) where T : ItemComponent
		{
			if (connectionEndA == A_3.c.Name)
			{
				Connection pairedConnection = A_3.c.Item.Connections.FirstOrDefault((Connection c2) => c2.Name == connectionEndB);
				if (pairedConnection != null)
				{
					if (A_3.alreadySearched.Contains(pairedConnection))
					{
						return;
					}
					this.GetConnectedComponentsRecursive<T>(pairedConnection, A_3.alreadySearched, A_3.connectedComponents, A_3.ignoreInactiveRelays, A_3.allowTraversingBackwards);
				}
			}
		}

		// Token: 0x04000CB7 RID: 3255
		public static bool ShowItems = true;

		// Token: 0x04000CB8 RID: 3256
		public static bool ShowWires = true;

		// Token: 0x04000CB9 RID: 3257
		private readonly List<PosInfo> positionBuffer = new List<PosInfo>();

		// Token: 0x04000CBA RID: 3258
		private readonly List<ItemComponent> activeHUDs = new List<ItemComponent>();

		// Token: 0x04000CBB RID: 3259
		private readonly List<SerializableEntityEditor> activeEditors = new List<SerializableEntityEditor>();

		// Token: 0x04000CBC RID: 3260
		private GUIComponentStyle iconStyle;

		// Token: 0x04000CBD RID: 3261
		public float LastImpactSoundTime;

		// Token: 0x04000CBE RID: 3262
		public const float ImpactSoundInterval = 0.2f;

		// Token: 0x04000CBF RID: 3263
		private float editingHUDRefreshTimer;

		// Token: 0x04000CC0 RID: 3264
		private ContainedItemSprite activeContainedSprite;

		// Token: 0x04000CC1 RID: 3265
		private readonly Dictionary<DecorativeSprite, DecorativeSprite.State> spriteAnimState = new Dictionary<DecorativeSprite, DecorativeSprite.State>();

		// Token: 0x04000CC2 RID: 3266
		public float DrawDepthOffset;

		// Token: 0x04000CC3 RID: 3267
		private bool fakeBroken;

		// Token: 0x04000CC4 RID: 3268
		private Sprite activeSprite;

		// Token: 0x04000CC5 RID: 3269
		private GUITextBlock itemInUseWarning;

		// Token: 0x04000CC6 RID: 3270
		private Rectangle? cachedVisibleExtents;

		// Token: 0x04000CC7 RID: 3271
		private readonly List<Rectangle> debugInitialHudPositions = new List<Rectangle>();

		// Token: 0x04000CC8 RID: 3272
		private readonly List<ItemComponent> prevActiveHUDs = new List<ItemComponent>();

		// Token: 0x04000CC9 RID: 3273
		private readonly List<ItemComponent> activeComponents = new List<ItemComponent>();

		// Token: 0x04000CCA RID: 3274
		private readonly List<ItemComponent> maxPriorityHUDs = new List<ItemComponent>();

		// Token: 0x04000CCB RID: 3275
		private readonly List<ColoredText> texts = new List<ColoredText>();

		// Token: 0x04000CCC RID: 3276
		public static readonly List<Item> ItemList = new List<Item>();

		// Token: 0x04000CCD RID: 3277
		private static readonly HashSet<Item> _dangerousItems = new HashSet<Item>();

		// Token: 0x04000CCE RID: 3278
		private static readonly List<Item> _repairableItems = new List<Item>();

		// Token: 0x04000CCF RID: 3279
		private static readonly List<Item> _cleanableItems = new List<Item>();

		// Token: 0x04000CD0 RID: 3280
		private static readonly HashSet<Item> _deconstructItems = new HashSet<Item>();

		// Token: 0x04000CD1 RID: 3281
		private static readonly List<Item> _sonarVisibleItems = new List<Item>();

		// Token: 0x04000CD2 RID: 3282
		private static readonly List<Item> _turretTargetItems = new List<Item>();

		// Token: 0x04000CD3 RID: 3283
		private static readonly List<Item> _chairItems = new List<Item>();

		// Token: 0x04000CD4 RID: 3284
		public static bool ShowLinks = true;

		// Token: 0x04000CD5 RID: 3285
		private HashSet<Identifier> tags;

		// Token: 0x04000CD6 RID: 3286
		private readonly bool isWire;

		// Token: 0x04000CD7 RID: 3287
		private readonly bool isLogic;

		// Token: 0x04000CD8 RID: 3288
		private Hull currentHull;

		// Token: 0x04000CD9 RID: 3289
		private CampaignMode.InteractionType campaignInteractionType;

		// Token: 0x04000CDA RID: 3290
		public bool Visible = true;

		// Token: 0x04000CDB RID: 3291
		public SpriteEffects SpriteEffects;

		// Token: 0x04000CDC RID: 3292
		private readonly Dictionary<Type, List<ItemComponent>> componentsByType = new Dictionary<Type, List<ItemComponent>>();

		// Token: 0x04000CDD RID: 3293
		private readonly List<ItemComponent> components;

		// Token: 0x04000CDE RID: 3294
		private readonly List<ItemComponent> updateableComponents = new List<ItemComponent>();

		// Token: 0x04000CDF RID: 3295
		private readonly List<IDrawableComponent> drawableComponents;

		// Token: 0x04000CE0 RID: 3296
		private bool hasComponentsToDraw;

		// Token: 0x04000CE2 RID: 3298
		public PhysicsBody body;

		// Token: 0x04000CE3 RID: 3299
		private readonly float originalWaterDragCoefficient;

		// Token: 0x04000CE4 RID: 3300
		private float? overrideWaterDragCoefficient;

		// Token: 0x04000CE5 RID: 3301
		public readonly XElement StaticBodyConfig;

		// Token: 0x04000CE6 RID: 3302
		public List<Fixture> StaticFixtures = new List<Fixture>();

		// Token: 0x04000CE7 RID: 3303
		private bool transformDirty = true;

		// Token: 0x04000CE8 RID: 3304
		private static readonly List<Item> itemsWithPendingConditionUpdates = new List<Item>();

		// Token: 0x04000CE9 RID: 3305
		private float lastSentCondition;

		// Token: 0x04000CEA RID: 3306
		private float sendConditionUpdateTimer;

		// Token: 0x04000CEB RID: 3307
		private float prevCondition;

		// Token: 0x04000CEC RID: 3308
		private float condition;

		// Token: 0x04000CED RID: 3309
		private bool inWater;

		// Token: 0x04000CEE RID: 3310
		private readonly bool hasInWaterStatusEffects;

		// Token: 0x04000CEF RID: 3311
		private readonly bool hasNotInWaterStatusEffects;

		// Token: 0x04000CF0 RID: 3312
		private Inventory parentInventory;

		// Token: 0x04000CF1 RID: 3313
		private readonly ItemInventory ownInventory;

		// Token: 0x04000CF2 RID: 3314
		private Rectangle defaultRect;

		// Token: 0x04000CF3 RID: 3315
		private readonly Dictionary<string, Connection> connections;

		// Token: 0x04000CF4 RID: 3316
		private readonly List<Repairable> repairables;

		// Token: 0x04000CF5 RID: 3317
		private readonly Quality qualityComponent;

		// Token: 0x04000CF6 RID: 3318
		private ConcurrentQueue<float> impactQueue;

		// Token: 0x04000CF7 RID: 3319
		private readonly bool[] hasStatusEffectsOfType = new bool[Enum.GetValues(typeof(ActionType)).Length];

		// Token: 0x04000CF8 RID: 3320
		private readonly Dictionary<ActionType, List<StatusEffect>> statusEffectLists;

		// Token: 0x04000CF9 RID: 3321
		private readonly float conditionMultiplierCampaign = 1f;

		// Token: 0x04000CFA RID: 3322
		public Action OnInteract;

		// Token: 0x04000CFC RID: 3324
		private bool? hasInGameEditableProperties;

		// Token: 0x04000CFE RID: 3326
		public Character Equipper;

		// Token: 0x04000D00 RID: 3328
		private Item rootContainer;

		// Token: 0x04000D01 RID: 3329
		private bool inWaterProofContainer;

		// Token: 0x04000D02 RID: 3330
		private Item container;

		// Token: 0x04000D03 RID: 3331
		private string description;

		// Token: 0x04000D04 RID: 3332
		private string descriptionTag;

		// Token: 0x04000D09 RID: 3337
		private float impactTolerance;

		// Token: 0x04000D0C RID: 3340
		public const float SubmarineImpactCooldown = 0.1f;

		// Token: 0x04000D0D RID: 3341
		public double LastSubmarineImpactTime;

		// Token: 0x04000D0E RID: 3342
		private float scale = 1f;

		// Token: 0x04000D11 RID: 3345
		protected Color spriteColor;

		// Token: 0x04000D14 RID: 3348
		public Color? HighlightColor;

		// Token: 0x04000D1A RID: 3354
		public bool OnInsertedEffectsAppliedOnPreviousRound;

		// Token: 0x04000D1E RID: 3358
		private float offsetOnSelectedMultiplier = 1f;

		// Token: 0x04000D1F RID: 3359
		private float healthMultiplier = 1f;

		// Token: 0x04000D20 RID: 3360
		private float maxRepairConditionMultiplier = 1f;

		// Token: 0x04000D24 RID: 3364
		private bool? indestructible;

		// Token: 0x04000D26 RID: 3366
		private bool? isDangerous;

		// Token: 0x04000D28 RID: 3368
		public bool UnequipAutomatically = true;

		// Token: 0x04000D29 RID: 3369
		public bool StolenDuringRound;

		// Token: 0x04000D2A RID: 3370
		private bool spawnedInCurrentOutpost;

		// Token: 0x04000D2B RID: 3371
		private bool allowStealing;

		// Token: 0x04000D2C RID: 3372
		public bool IsSalvageMissionItem;

		// Token: 0x04000D2D RID: 3373
		private string originalOutpost;

		// Token: 0x04000D2F RID: 3375
		private bool waterProof;

		// Token: 0x04000D31 RID: 3377
		private readonly HashSet<InvSlotType> allowedSlots = new HashSet<InvSlotType>();

		// Token: 0x04000D32 RID: 3378
		public readonly ImmutableArray<ItemInventory> OwnInventories = ImmutableArray<ItemInventory>.Empty;

		// Token: 0x04000D36 RID: 3382
		public readonly HashSet<ItemPrefab> AvailableSwaps = new HashSet<ItemPrefab>();

		// Token: 0x04000D37 RID: 3383
		private readonly List<ISerializableEntity> allPropertyObjects = new List<ISerializableEntity>();

		// Token: 0x04000D3B RID: 3387
		private ItemStatManager statManager;

		// Token: 0x04000D3D RID: 3389
		public Action<Character> OnDeselect;

		// Token: 0x04000D3E RID: 3390
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x04000D3F RID: 3391
		public bool IsActive = true;

		// Token: 0x04000D40 RID: 3392
		public bool IsInRemoveQueue;

		// Token: 0x04000D41 RID: 3393
		[TupleElementNames(new string[]
		{
			"Input",
			"Output"
		})]
		public static readonly ImmutableArray<ValueTuple<Identifier, Identifier>> connectionPairs = new ValueTuple<Identifier, Identifier>[]
		{
			new ValueTuple<Identifier, Identifier>("power_in".ToIdentifier(), "power_out".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in1".ToIdentifier(), "signal_out1".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in2".ToIdentifier(), "signal_out2".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in3".ToIdentifier(), "signal_out3".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in4".ToIdentifier(), "signal_out4".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in".ToIdentifier(), "signal_out".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in1".ToIdentifier(), "signal_out".ToIdentifier()),
			new ValueTuple<Identifier, Identifier>("signal_in2".ToIdentifier(), "signal_out".ToIdentifier())
		}.ToImmutableArray<ValueTuple<Identifier, Identifier>>();

		// Token: 0x04000D42 RID: 3394
		[TupleElementNames(new string[]
		{
			"Signal",
			"Connection"
		})]
		private readonly HashSet<ValueTuple<Signal, Connection>> delayedSignals = new HashSet<ValueTuple<Signal, Connection>>();

		// Token: 0x04000D43 RID: 3395
		private List<Item> droppedStack;

		// Token: 0x04000D44 RID: 3396
		private bool isDroppedStackOwner;

		// Token: 0x02000A71 RID: 2673
		private enum InteractionVisibility
		{
			// Token: 0x04004469 RID: 17513
			None,
			// Token: 0x0400446A RID: 17514
			MissingRequirement,
			// Token: 0x0400446B RID: 17515
			Visible
		}

		// Token: 0x02000A72 RID: 2674
		private readonly struct CombineEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A83 RID: 6787
			// (get) Token: 0x06007551 RID: 30033 RVA: 0x00375923 File Offset: 0x00373B23
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.Combine;
				}
			}

			// Token: 0x06007552 RID: 30034 RVA: 0x00375926 File Offset: 0x00373B26
			public CombineEventData(Item combineTarget)
			{
				this.CombineTarget = combineTarget;
			}

			// Token: 0x0400446C RID: 17516
			public readonly Item CombineTarget;
		}

		// Token: 0x02000A73 RID: 2675
		private readonly struct TreatmentEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A84 RID: 6788
			// (get) Token: 0x06007553 RID: 30035 RVA: 0x0037592F File Offset: 0x00373B2F
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.Treatment;
				}
			}

			// Token: 0x17001A85 RID: 6789
			// (get) Token: 0x06007554 RID: 30036 RVA: 0x00375934 File Offset: 0x00373B34
			public byte LimbIndex
			{
				get
				{
					Character targetCharacter = this.TargetCharacter;
					Limb[] array;
					if (targetCharacter == null)
					{
						array = null;
					}
					else
					{
						AnimController animController = targetCharacter.AnimController;
						array = ((animController != null) ? animController.Limbs : null);
					}
					Limb[] limbs = array;
					if (limbs == null)
					{
						return byte.MaxValue;
					}
					return (byte)Array.IndexOf<Limb>(limbs, this.TargetLimb);
				}
			}

			// Token: 0x06007555 RID: 30037 RVA: 0x00375976 File Offset: 0x00373B76
			public TreatmentEventData(Character targetCharacter, Limb targetLimb)
			{
				this.TargetCharacter = targetCharacter;
				this.TargetLimb = targetLimb;
			}

			// Token: 0x0400446D RID: 17517
			public readonly Character TargetCharacter;

			// Token: 0x0400446E RID: 17518
			public readonly Limb TargetLimb;
		}

		// Token: 0x02000A74 RID: 2676
		public enum EventType
		{
			// Token: 0x04004470 RID: 17520
			ComponentState,
			// Token: 0x04004471 RID: 17521
			InventoryState,
			// Token: 0x04004472 RID: 17522
			Treatment,
			// Token: 0x04004473 RID: 17523
			ChangeProperty,
			// Token: 0x04004474 RID: 17524
			Combine,
			// Token: 0x04004475 RID: 17525
			Status,
			// Token: 0x04004476 RID: 17526
			AssignCampaignInteraction,
			// Token: 0x04004477 RID: 17527
			ApplyStatusEffect,
			// Token: 0x04004478 RID: 17528
			Upgrade,
			// Token: 0x04004479 RID: 17529
			ItemStat,
			// Token: 0x0400447A RID: 17530
			DroppedStack,
			// Token: 0x0400447B RID: 17531
			SetHighlight,
			// Token: 0x0400447C RID: 17532
			SwapItem,
			// Token: 0x0400447D RID: 17533
			MinValue = 0,
			// Token: 0x0400447E RID: 17534
			MaxValue = 12
		}

		// Token: 0x02000A75 RID: 2677
		public interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x17001A86 RID: 6790
			// (get) Token: 0x06007556 RID: 30038
			Item.EventType EventType { get; }
		}

		// Token: 0x02000A76 RID: 2678
		public readonly struct ComponentStateEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A87 RID: 6791
			// (get) Token: 0x06007557 RID: 30039 RVA: 0x00375986 File Offset: 0x00373B86
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ComponentState;
				}
			}

			// Token: 0x06007558 RID: 30040 RVA: 0x00375989 File Offset: 0x00373B89
			public ComponentStateEventData(ItemComponent component, ItemComponent.IEventData componentData)
			{
				this.Component = component;
				this.ComponentData = componentData;
			}

			// Token: 0x0400447F RID: 17535
			public readonly ItemComponent Component;

			// Token: 0x04004480 RID: 17536
			public readonly ItemComponent.IEventData ComponentData;
		}

		// Token: 0x02000A77 RID: 2679
		public readonly struct InventoryStateEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A88 RID: 6792
			// (get) Token: 0x06007559 RID: 30041 RVA: 0x00375999 File Offset: 0x00373B99
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.InventoryState;
				}
			}

			// Token: 0x0600755A RID: 30042 RVA: 0x0037599C File Offset: 0x00373B9C
			public InventoryStateEventData(ItemContainer component, Range slotRange)
			{
				this.Component = component;
				this.SlotRange = slotRange;
			}

			// Token: 0x04004481 RID: 17537
			public readonly ItemContainer Component;

			// Token: 0x04004482 RID: 17538
			public readonly Range SlotRange;
		}

		// Token: 0x02000A78 RID: 2680
		public readonly struct ChangePropertyEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A89 RID: 6793
			// (get) Token: 0x0600755B RID: 30043 RVA: 0x003759AC File Offset: 0x00373BAC
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ChangeProperty;
				}
			}

			// Token: 0x0600755C RID: 30044 RVA: 0x003759B0 File Offset: 0x00373BB0
			public ChangePropertyEventData(SerializableProperty serializableProperty, ISerializableEntity entity)
			{
				if (serializableProperty.GetAttribute<Editable>() == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Attempted to create ");
					defaultInterpolatedStringHandler.AppendFormatted("ChangePropertyEventData");
					defaultInterpolatedStringHandler.AppendLiteral(" for the non-editable property ");
					defaultInterpolatedStringHandler.AppendFormatted(serializableProperty.Name);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				this.SerializableProperty = serializableProperty;
				this.Entity = entity;
			}

			// Token: 0x04004483 RID: 17539
			public readonly SerializableProperty SerializableProperty;

			// Token: 0x04004484 RID: 17540
			public readonly ISerializableEntity Entity;
		}

		// Token: 0x02000A79 RID: 2681
		public readonly struct SetItemStatEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A8A RID: 6794
			// (get) Token: 0x0600755D RID: 30045 RVA: 0x00375A2A File Offset: 0x00373C2A
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ItemStat;
				}
			}

			// Token: 0x0600755E RID: 30046 RVA: 0x00375A2E File Offset: 0x00373C2E
			public SetItemStatEventData(Dictionary<TalentStatIdentifier, float> stats)
			{
				this.Stats = stats;
			}

			// Token: 0x04004485 RID: 17541
			public readonly Dictionary<TalentStatIdentifier, float> Stats;
		}

		// Token: 0x02000A7A RID: 2682
		private readonly struct ItemStatusEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A8B RID: 6795
			// (get) Token: 0x0600755F RID: 30047 RVA: 0x00375A37 File Offset: 0x00373C37
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.Status;
				}
			}

			// Token: 0x06007560 RID: 30048 RVA: 0x00375A3A File Offset: 0x00373C3A
			public ItemStatusEventData(bool loadingRound)
			{
				this.LoadingRound = loadingRound;
			}

			// Token: 0x04004486 RID: 17542
			public readonly bool LoadingRound;
		}

		// Token: 0x02000A7B RID: 2683
		private readonly struct AssignCampaignInteractionEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A8C RID: 6796
			// (get) Token: 0x06007561 RID: 30049 RVA: 0x00375A43 File Offset: 0x00373C43
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.AssignCampaignInteraction;
				}
			}

			// Token: 0x06007562 RID: 30050 RVA: 0x00375A46 File Offset: 0x00373C46
			public AssignCampaignInteractionEventData(IEnumerable<Client> targetClients)
			{
				this.TargetClients = (targetClients ?? Enumerable.Empty<Client>()).ToImmutableArray<Client>();
			}

			// Token: 0x04004487 RID: 17543
			public readonly ImmutableArray<Client> TargetClients;
		}

		// Token: 0x02000A7C RID: 2684
		public readonly struct ApplyStatusEffectEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A8D RID: 6797
			// (get) Token: 0x06007563 RID: 30051 RVA: 0x00375A5D File Offset: 0x00373C5D
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.ApplyStatusEffect;
				}
			}

			// Token: 0x06007564 RID: 30052 RVA: 0x00375A60 File Offset: 0x00373C60
			public ApplyStatusEffectEventData(ActionType actionType, ItemComponent targetItemComponent = null, Character targetCharacter = null, Limb targetLimb = null, Entity useTarget = null, Vector2? worldPosition = null)
			{
				this.ActionType = actionType;
				this.TargetItemComponent = targetItemComponent;
				this.TargetCharacter = targetCharacter;
				this.TargetLimb = targetLimb;
				this.UseTarget = useTarget;
				this.WorldPosition = worldPosition;
			}

			// Token: 0x04004488 RID: 17544
			public readonly ActionType ActionType;

			// Token: 0x04004489 RID: 17545
			public readonly ItemComponent TargetItemComponent;

			// Token: 0x0400448A RID: 17546
			public readonly Character TargetCharacter;

			// Token: 0x0400448B RID: 17547
			public readonly Limb TargetLimb;

			// Token: 0x0400448C RID: 17548
			public readonly Entity UseTarget;

			// Token: 0x0400448D RID: 17549
			public readonly Vector2? WorldPosition;
		}

		// Token: 0x02000A7D RID: 2685
		private readonly struct UpgradeEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A8E RID: 6798
			// (get) Token: 0x06007565 RID: 30053 RVA: 0x00375A8F File Offset: 0x00373C8F
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.Upgrade;
				}
			}

			// Token: 0x06007566 RID: 30054 RVA: 0x00375A92 File Offset: 0x00373C92
			public UpgradeEventData(Upgrade upgrade)
			{
				this.Upgrade = upgrade;
			}

			// Token: 0x0400448E RID: 17550
			public readonly Upgrade Upgrade;
		}

		// Token: 0x02000A7E RID: 2686
		private readonly struct SwapItemEventData : Item.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001A8F RID: 6799
			// (get) Token: 0x06007567 RID: 30055 RVA: 0x00375A9B File Offset: 0x00373C9B
			public Item.EventType EventType
			{
				get
				{
					return Item.EventType.SwapItem;
				}
			}

			// Token: 0x06007568 RID: 30056 RVA: 0x00375A9F File Offset: 0x00373C9F
			public SwapItemEventData(ItemPrefab newItem, ushort newId)
			{
				this.NewItem = newItem;
				this.NewId = newId;
			}

			// Token: 0x0400448F RID: 17551
			public readonly ItemPrefab NewItem;

			// Token: 0x04004490 RID: 17552
			public readonly ushort NewId;
		}
	}
}
