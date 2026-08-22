using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000EA RID: 234
	internal class Structure : MapEntity, IDamageable, IServerSerializable, INetSerializable, ISerializableEntity
	{
		// Token: 0x170008CA RID: 2250
		// (get) Token: 0x0600209F RID: 8351 RVA: 0x0014682E File Offset: 0x00144A2E
		public override bool SelectableInEditor
		{
			get
			{
				if (GameMain.SubEditorScreen.IsSubcategoryHidden(this.Prefab.Subcategory))
				{
					return false;
				}
				if (!SubEditorScreen.IsLayerVisible(this))
				{
					return false;
				}
				if (!this.HasBody)
				{
					return Structure.ShowStructures;
				}
				return Structure.ShowWalls;
			}
		}

		// Token: 0x060020A0 RID: 8352 RVA: 0x00146868 File Offset: 0x00144A68
		public static Vector2 UpgradeTextureOffset(Vector2 targetSize, Vector2 originalTextureOffset, SubmarineInfo submarineInfo, Rectangle sourceRect, Vector2 scale, bool flippedX, bool flippedY)
		{
			if (submarineInfo.GameVersion <= Sprite.LastBrokenTiledSpriteGameVersion)
			{
				Vector2 flipper = new ValueTuple<float, float>(flippedX ? -1f : 1f, flippedY ? -1f : 1f);
				Vector2 textureOffset = originalTextureOffset * flipper;
				textureOffset = new Vector2((float)MathUtils.PositiveModulo((int)(-(int)textureOffset.X), sourceRect.Width), (float)MathUtils.PositiveModulo((int)(-(int)textureOffset.Y), sourceRect.Height));
				textureOffset.X = textureOffset.X / scale.X % (float)sourceRect.Width;
				textureOffset.Y = textureOffset.Y / scale.Y % (float)sourceRect.Height;
				Vector2 flippedDrawOffset = Vector2.Zero;
				if (flippedX)
				{
					float diff = targetSize.X % ((float)sourceRect.Width * scale.X);
					flippedDrawOffset.X = ((float)sourceRect.Width * scale.X - diff) / scale.X;
					flippedDrawOffset.X = (MathUtils.NearlyEqual(flippedDrawOffset.X, MathF.Round(flippedDrawOffset.X), 0.0001f) ? MathF.Round(flippedDrawOffset.X) : flippedDrawOffset.X);
				}
				if (flippedY)
				{
					float diff2 = targetSize.Y % ((float)sourceRect.Height * scale.Y);
					flippedDrawOffset.Y = ((float)sourceRect.Height * scale.Y - diff2) / scale.Y;
					flippedDrawOffset.Y = (MathUtils.NearlyEqual(flippedDrawOffset.Y, MathF.Round(flippedDrawOffset.Y), 0.0001f) ? MathF.Round(flippedDrawOffset.Y) : flippedDrawOffset.Y);
				}
				Vector2 textureOffsetPlusFlipBs = textureOffset + flippedDrawOffset;
				if (textureOffsetPlusFlipBs.X > (float)sourceRect.Width)
				{
					float diff3 = textureOffsetPlusFlipBs.X - (float)sourceRect.Width;
					textureOffset.X = (textureOffset.X + diff3 * (scale.X - 1f)) % (float)sourceRect.Width;
				}
				if (textureOffsetPlusFlipBs.Y > (float)sourceRect.Height)
				{
					float diff4 = textureOffsetPlusFlipBs.Y - (float)sourceRect.Height;
					textureOffset.Y = (textureOffset.Y + diff4 * (scale.Y - 1f)) % (float)sourceRect.Height;
				}
				textureOffset *= scale * flipper;
				return -textureOffset;
			}
			return originalTextureOffset;
		}

		// Token: 0x060020A1 RID: 8353 RVA: 0x00146ABD File Offset: 0x00144CBD
		public override void UpdateEditing(Camera cam, float deltaTime)
		{
			if (MapEntity.editingHUD == null || MapEntity.editingHUD.UserData as Structure != this)
			{
				MapEntity.editingHUD = this.CreateEditingHUD(Screen.Selected != GameMain.SubEditorScreen);
			}
		}

		// Token: 0x060020A2 RID: 8354 RVA: 0x00146AF4 File Offset: 0x00144CF4
		private void SetLightTextureOffset()
		{
			Vector2 textOffset = this.textureOffset;
			if (base.FlippedX)
			{
				textOffset.X = -textOffset.X;
			}
			if (base.FlippedY)
			{
				textOffset.Y = -textOffset.Y;
			}
			foreach (LightSource light in this.Lights)
			{
				Vector2 bgOffset = new Vector2(MathUtils.PositiveModulo(-textOffset.X, (float)light.texture.Width), MathUtils.PositiveModulo(-textOffset.Y, (float)light.texture.Height));
				light.LightTextureOffset = bgOffset;
			}
		}

		// Token: 0x060020A3 RID: 8355 RVA: 0x00146BB4 File Offset: 0x00144DB4
		public GUIComponent CreateEditingHUD(bool inGame = false)
		{
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
				CanTakeKeyBoardFocus = false
			};
			SerializableEntityEditor editor = new SerializableEntityEditor(listBox.Content.RectTransform, this, inGame, true, "", 24, GUIStyle.LargeFont, false)
			{
				UserData = this
			};
			GUIComponent[] scaleFields;
			if (editor.Fields.TryGetValue("Scale".ToIdentifier(), out scaleFields))
			{
				GUINumberInput scaleInput = scaleFields.FirstOrDefault<GUIComponent>() as GUINumberInput;
				if (scaleInput != null)
				{
					GUINumberInput guinumberInput = scaleInput;
					guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numberInput)
					{
						this.TextureOffset *= this.Scale / this.ScaleWhenTextureOffsetSet;
					}));
				}
			}
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
				editor.AddCustomContent(tickBox2, 1);
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
				editor.AddCustomContent(layerText, 1);
			}
			GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Point(listBox.Content.Rect.Width, heightScaled), null, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			GUIButton mirrorX = new GUIButton(new RectTransform(new Vector2(0.23f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MirrorEntityX"), Alignment.Center, "GUIButtonSmall", null)
			{
				ToolTip = TextManager.Get("MirrorEntityXToolTip"),
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
					MapEntity.ColorFlipButton(button, base.FlippedX);
					return true;
				}
			};
			MapEntity.ColorFlipButton(mirrorX, base.FlippedX);
			GUIButton mirrorY = new GUIButton(new RectTransform(new Vector2(0.23f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MirrorEntityY"), Alignment.Center, "GUIButtonSmall", null)
			{
				ToolTip = TextManager.Get("MirrorEntityYToolTip"),
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
					MapEntity.ColorFlipButton(button, base.FlippedY);
					return true;
				}
			};
			MapEntity.ColorFlipButton(mirrorY, base.FlippedY);
			new GUIButton(new RectTransform(new Vector2(0.23f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ReloadSprite"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object data)
			{
				this.Sprite.ReloadXML();
				this.Sprite.ReloadTexture();
				return true;
			};
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
			buttonContainer.RectTransform.Resize(new Point(buttonContainer.Rect.Width, buttonContainer.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y)), true);
			buttonContainer.RectTransform.IsFixedSize = true;
			GUITextBlock.AutoScaleAndNormalize(from c in buttonContainer.Children
			where c is GUIButton
			select c into b
			select ((GUIButton)b).TextBlock, true, false, null);
			editor.AddCustomContent(buttonContainer, editor.ContentCount);
			MapEntity.PositionEditingHUD();
			return MapEntity.editingHUD;
		}

		// Token: 0x060020A4 RID: 8356 RVA: 0x0014717C File Offset: 0x0014537C
		public override bool IsVisible(Rectangle worldView)
		{
			Quad2D quad2D = Quad2D.FromSubmarineRectangle(base.WorldRect);
			quad2D = quad2D.Rotated((base.FlippedX != base.FlippedY) ? this.RotationRad : (-this.RotationRad));
			RectangleF worldRect = quad2D.BoundingAxisAlignedRectangle;
			Vector2 worldPos = this.WorldPosition;
			Vector2 min = new Vector2(worldRect.X, worldRect.Y);
			Vector2 max = new Vector2(worldRect.Right, worldRect.Y + worldRect.Height);
			foreach (DecorativeSprite decorativeSprite in this.Prefab.DecorativeSprites)
			{
				Vector2 scale = decorativeSprite.GetScale(ref this.spriteAnimState[decorativeSprite].ScaleState, this.spriteAnimState[decorativeSprite].RandomScaleFactor) * this.Scale;
				min.X = Math.Min(worldPos.X - decorativeSprite.Sprite.size.X * decorativeSprite.Sprite.RelativeOrigin.X * scale.X, min.X);
				max.X = Math.Max(worldPos.X + decorativeSprite.Sprite.size.X * (1f - decorativeSprite.Sprite.RelativeOrigin.X) * scale.X, max.X);
				min.Y = Math.Min(worldPos.Y - decorativeSprite.Sprite.size.Y * (1f - decorativeSprite.Sprite.RelativeOrigin.Y) * scale.Y, min.Y);
				max.Y = Math.Max(worldPos.Y + decorativeSprite.Sprite.size.Y * decorativeSprite.Sprite.RelativeOrigin.Y * scale.Y, max.Y);
			}
			Vector2 offset = base.GetCollapseEffectOffset();
			min += offset;
			max += offset;
			if (min.X > (float)worldView.Right || max.X < (float)worldView.X)
			{
				return false;
			}
			if (min.Y > (float)worldView.Y || max.Y < (float)(worldView.Y - worldView.Height))
			{
				return false;
			}
			Vector2 extents = max - min;
			return extents.X * Screen.Selected.Cam.Zoom >= 1f && extents.Y * Screen.Selected.Cam.Zoom >= 1f;
		}

		// Token: 0x060020A5 RID: 8357 RVA: 0x0014742C File Offset: 0x0014562C
		public override void Draw(SpriteBatch spriteBatch, bool editing, bool back = true)
		{
			if (this.Prefab.Sprite == null)
			{
				return;
			}
			if (editing)
			{
				if (!SubEditorScreen.IsLayerVisible(this))
				{
					return;
				}
				if (!this.HasBody && !Structure.ShowStructures)
				{
					return;
				}
				if (this.HasBody && !Structure.ShowWalls)
				{
					return;
				}
			}
			this.Draw(spriteBatch, editing, back, null);
		}

		// Token: 0x060020A6 RID: 8358 RVA: 0x0014747D File Offset: 0x0014567D
		public void DrawDamage(SpriteBatch spriteBatch, Effect damageEffect, bool editing)
		{
			this.Draw(spriteBatch, editing, false, damageEffect);
		}

		// Token: 0x060020A7 RID: 8359 RVA: 0x00147489 File Offset: 0x00145689
		private float GetRealDepth()
		{
			if (!base.SpriteDepthOverrideIsSet)
			{
				return this.Prefab.Sprite.Depth;
			}
			return base.SpriteOverrideDepth;
		}

		// Token: 0x060020A8 RID: 8360 RVA: 0x001474AA File Offset: 0x001456AA
		public override float GetDrawDepth()
		{
			return base.GetDrawDepth(this.GetRealDepth(), this.Prefab.Sprite);
		}

		// Token: 0x060020A9 RID: 8361 RVA: 0x001474C4 File Offset: 0x001456C4
		private void Draw(SpriteBatch spriteBatch, bool editing, bool back = true, Effect damageEffect = null)
		{
			if (this.Prefab.Sprite == null)
			{
				return;
			}
			if (editing)
			{
				if (!SubEditorScreen.IsLayerVisible(this))
				{
					return;
				}
				if (!this.HasBody && !Structure.ShowStructures)
				{
					return;
				}
				if (this.HasBody && !Structure.ShowWalls)
				{
					return;
				}
			}
			else if (base.IsHidden)
			{
				return;
			}
			Color color = (base.IsIncludedInSelection && editing) ? GUIStyle.Blue : (base.IsHighlighted ? (GUIStyle.Orange * Math.Max((float)this.spriteColor.A / 255f, 0.1f)) : this.spriteColor);
			if (base.IsSelected && editing)
			{
				color = this.spriteColor;
				Vector2 rectSize = this.rect.Size.ToVector2();
				if (this.BodyWidth > 0f)
				{
					rectSize.X = this.BodyWidth;
				}
				if (this.BodyHeight > 0f)
				{
					rectSize.Y = this.BodyHeight;
				}
				Vector2 bodyPos = this.WorldPosition + this.BodyOffset * this.Scale;
				GUI.DrawRectangle(spriteBatch, new Vector2(bodyPos.X, -bodyPos.Y), rectSize.X, rectSize.Y, this.BodyRotation, Color.White, 0f, (float)Math.Max(1, (int)(2f / Screen.Selected.Cam.Zoom)));
			}
			bool isWiringMode = editing && SubEditorScreen.TransparentWiringMode && SubEditorScreen.IsWiringMode();
			if (isWiringMode)
			{
				color *= 0.15f;
			}
			Vector2 drawOffset = (base.Submarine == null) ? Vector2.Zero : base.Submarine.DrawPosition;
			drawOffset += base.GetCollapseEffectOffset();
			float depth = this.GetDrawDepth();
			Vector2 textureOffset = this.textureOffset;
			if (back && damageEffect == null && !isWiringMode && this.Prefab.BackgroundSprite != null)
			{
				Vector2 dropShadowOffset = Vector2.Zero;
				if (this.UseDropShadow)
				{
					dropShadowOffset = this.DropShadowOffset;
					if (dropShadowOffset == Vector2.Zero)
					{
						if (base.Submarine == null)
						{
							dropShadowOffset = Vector2.UnitY * 10f;
						}
						else
						{
							dropShadowOffset = (this.IsHorizontal ? new Vector2(0f, (float)Math.Sign(base.Submarine.HiddenSubPosition.Y - this.Position.Y) * 10f) : new Vector2((float)Math.Sign(base.Submarine.HiddenSubPosition.X - this.Position.X) * 10f, 0f));
						}
					}
					dropShadowOffset.Y = -dropShadowOffset.Y;
				}
				Vector2 backGroundOffset = new Vector2(MathUtils.PositiveModulo(-textureOffset.X, (float)this.Prefab.BackgroundSprite.SourceRect.Width * this.TextureScale.X * this.Scale), MathUtils.PositiveModulo(-textureOffset.Y, (float)this.Prefab.BackgroundSprite.SourceRect.Height * this.TextureScale.Y * this.Scale));
				float rotationRad = Structure.<Draw>g__GetRotationForSprite|16_0(this.RotationRad, this.Prefab.BackgroundSprite);
				Sprite backgroundSprite = this.Prefab.BackgroundSprite;
				Vector2 position = new Vector2((float)(this.rect.X + this.rect.Width / 2) + drawOffset.X, -((float)(this.rect.Y - this.rect.Height / 2) + drawOffset.Y));
				Vector2 targetSize = new Vector2((float)this.rect.Width, (float)this.rect.Height);
				float num = rotationRad;
				Vector2? vector = new Vector2?(this.rect.Size.ToVector2() * new Vector2(0.5f, 0.5f));
				Color? color2 = new Color?(this.Prefab.BackgroundSpriteColor);
				Vector2? startOffset = new Vector2?(this.TextureScale * this.Scale);
				Vector2? vector2 = new Vector2?(backGroundOffset);
				float? depth2 = new float?(Math.Max(base.GetDrawDepth(this.Prefab.BackgroundSprite.Depth, this.Prefab.BackgroundSprite), depth + 1E-06f));
				backgroundSprite.DrawTiled(spriteBatch, position, targetSize, this.Prefab.BackgroundSprite.effects ^ this.SpriteEffects, num, vector, color2, vector2, startOffset, depth2);
				if (this.UseDropShadow)
				{
					Sprite backgroundSprite2 = this.Prefab.BackgroundSprite;
					Vector2 position2 = new Vector2((float)(this.rect.X + this.rect.Width / 2) + drawOffset.X, -((float)(this.rect.Y - this.rect.Height / 2) + drawOffset.Y)) + dropShadowOffset;
					Vector2 targetSize2 = new Vector2((float)this.rect.Width, (float)this.rect.Height);
					num = rotationRad;
					vector2 = new Vector2?(this.rect.Size.ToVector2() * new Vector2(0.5f, 0.5f));
					color2 = new Color?(Color.Black * 0.5f);
					startOffset = new Vector2?(this.TextureScale * this.Scale);
					vector = new Vector2?(backGroundOffset);
					depth2 = new float?((depth + this.Prefab.BackgroundSprite.Depth) / 2f);
					backgroundSprite2.DrawTiled(spriteBatch, position2, targetSize2, this.Prefab.BackgroundSprite.effects ^ this.SpriteEffects, num, vector2, color2, vector, startOffset, depth2);
				}
			}
			if (back == this.GetRealDepth() > 0.5f)
			{
				Vector2 advanceX = MathUtils.RotatedUnitXRadians(this.RotationRad).FlipY();
				Vector2 advanceY = advanceX.YX().FlipX();
				if (base.FlippedX != base.FlippedY)
				{
					advanceX = advanceX.FlipY();
					advanceY = advanceY.FlipX();
				}
				float sectionSpriteRotationRad = Structure.<Draw>g__GetRotationForSprite|16_0(this.RotationRad, this.Prefab.Sprite);
				for (int i = 0; i < this.Sections.Length; i++)
				{
					Rectangle drawSection = this.Sections[i].rect;
					if (damageEffect != null)
					{
						float newCutoff = MathHelper.Lerp(0f, 0.65f, this.Sections[i].damage / this.MaxHealth);
						if (Math.Abs(newCutoff - Submarine.DamageEffectCutoff) > 0.01f || MathUtils.NearlyEqual(newCutoff, 0f, 0.0001f) != MathUtils.NearlyEqual(Submarine.DamageEffectCutoff, 0f, 0.0001f))
						{
							spriteBatch.End();
							spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, damageEffect, new Matrix?(Screen.Selected.Cam.Transform));
							damageEffect.Parameters["aCutoff"].SetValue(newCutoff);
							damageEffect.Parameters["cCutoff"].SetValue(newCutoff * 1.2f);
							damageEffect.CurrentTechnique.Passes[0].Apply();
							Submarine.DamageEffectCutoff = newCutoff;
						}
					}
					if (!this.HasDamage && i == 0)
					{
						drawSection = new Rectangle(drawSection.X, drawSection.Y, this.Sections[this.Sections.Length - 1].rect.Right - drawSection.X, drawSection.Y - (this.Sections[this.Sections.Length - 1].rect.Y - this.Sections[this.Sections.Length - 1].rect.Height));
						i = this.Sections.Length;
					}
					Vector2 sectionOffset = new Vector2((float)Math.Abs(this.rect.Location.X - drawSection.Location.X), (float)Math.Abs(this.rect.Location.Y - drawSection.Location.Y));
					if (base.FlippedX && this.IsHorizontal)
					{
						sectionOffset.X = (float)(this.rect.Right - drawSection.Right);
					}
					if (base.FlippedY && !this.IsHorizontal)
					{
						sectionOffset.Y = (float)(drawSection.Y - drawSection.Height - (this.rect.Y - this.rect.Height));
					}
					sectionOffset.X += MathUtils.PositiveModulo(-textureOffset.X, (float)this.Prefab.Sprite.SourceRect.Width * this.TextureScale.X * this.Scale);
					sectionOffset.Y += MathUtils.PositiveModulo(-textureOffset.Y, (float)this.Prefab.Sprite.SourceRect.Height * this.TextureScale.Y * this.Scale);
					Vector2 pos = new Vector2((float)drawSection.X, (float)drawSection.Y);
					pos -= this.rect.Location.ToVector2();
					pos = advanceX * pos.X + advanceY * pos.Y;
					pos += this.rect.Location.ToVector2();
					pos = new Vector2(pos.X + (float)(this.rect.Width / 2) + drawOffset.X, -(pos.Y - (float)(this.rect.Height / 2) + drawOffset.Y));
					Sprite sprite = this.Prefab.Sprite;
					Vector2 position3 = pos;
					Vector2 targetSize3 = new Vector2((float)drawSection.Width, (float)drawSection.Height);
					float num = sectionSpriteRotationRad;
					Vector2? vector = new Vector2?(this.rect.Size.ToVector2() * new Vector2(0.5f, 0.5f));
					Color? color2 = new Color?(color);
					Vector2? startOffset = new Vector2?(sectionOffset);
					float? depth2 = new float?(depth);
					Vector2? vector2 = new Vector2?(this.TextureScale * this.Scale);
					sprite.DrawTiled(spriteBatch, position3, targetSize3, this.Prefab.Sprite.effects ^ this.SpriteEffects, num, vector, color2, startOffset, vector2, depth2);
				}
				foreach (DecorativeSprite decorativeSprite in this.Prefab.DecorativeSprites)
				{
					if (this.spriteAnimState[decorativeSprite].IsActive)
					{
						float rotation = decorativeSprite.GetRotation(ref this.spriteAnimState[decorativeSprite].RotationState, this.spriteAnimState[decorativeSprite].RandomRotationFactor) + this.RotationRad;
						Vector2 offset = decorativeSprite.GetOffset(ref this.spriteAnimState[decorativeSprite].OffsetState, this.spriteAnimState[decorativeSprite].RandomOffsetMultiplier, 0f) * this.Scale;
						if (base.FlippedX && this.Prefab.CanSpriteFlipX)
						{
							offset.X = -offset.X;
						}
						if (base.FlippedY && this.Prefab.CanSpriteFlipY)
						{
							offset.Y = -offset.Y;
						}
						Vector2 drawPos = this.DrawPosition + MathUtils.RotatePoint(offset, -this.RotationRad);
						Sprite sprite2 = decorativeSprite.Sprite;
						Vector2 pos3 = drawPos.FlipY();
						Color color3 = color;
						float num = rotation;
						sprite2.Draw(spriteBatch, pos3, color3, decorativeSprite.Sprite.Origin, num, decorativeSprite.GetScale(ref this.spriteAnimState[decorativeSprite].ScaleState, this.spriteAnimState[decorativeSprite].RandomScaleFactor) * this.Scale, this.Prefab.Sprite.effects ^ this.SpriteEffects, new float?(Math.Min(depth + (decorativeSprite.Sprite.Depth - this.Prefab.Sprite.Depth), 0.999f)));
					}
				}
			}
			if (GameMain.DebugDraw && Screen.Selected.Cam.Zoom > 0.5f)
			{
				if (this.Bodies != null)
				{
					foreach (Body body in this.Bodies)
					{
						Vector2 pos2 = ConvertUnits.ToDisplayUnits(body.Position);
						if (base.Submarine != null)
						{
							pos2 += base.Submarine.DrawPosition;
						}
						pos2.Y = -pos2.Y;
						Vector2 dimensions = this.bodyDimensions[body];
						GUI.DrawRectangle(spriteBatch, pos2, ConvertUnits.ToDisplayUnits(dimensions.X), ConvertUnits.ToDisplayUnits(dimensions.Y), -body.Rotation, Color.White, 0f, 1f);
					}
				}
				if (this.SectionCount > 0 && this.HasBody)
				{
					for (int j = 0; j < this.SectionCount; j++)
					{
						if (this.GetSection(j).damage > 0f)
						{
							Vector2 textPos = this.SectionPosition(j, true);
							if (base.Submarine != null)
							{
								textPos += base.Submarine.DrawPosition - base.Submarine.Position;
							}
							textPos.Y = -textPos.Y;
							Vector2 pos4 = textPos;
							string text = "Damage: " + ((int)(this.GetSection(j).damage / this.MaxHealth * 100f)).ToString() + "%";
							Color yellow = Color.Yellow;
							Color? color2 = null;
							GUI.DrawString(spriteBatch, pos4, text, yellow, color2, 0, null, ForceUpperCase.Inherit);
						}
					}
				}
			}
		}

		// Token: 0x060020AA RID: 8362 RVA: 0x001482A8 File Offset: 0x001464A8
		public void UpdateSpriteStates(float deltaTime)
		{
			if (this.Prefab.DecorativeSpriteGroups.Count == 0)
			{
				return;
			}
			DecorativeSprite.UpdateSpriteStates(this.Prefab.DecorativeSpriteGroups, this.spriteAnimState, (int)this.ID, deltaTime, new Func<PropertyConditional, bool>(this.ConditionalMatches));
			foreach (int spriteGroup in this.Prefab.DecorativeSpriteGroups.Keys)
			{
				for (int i = 0; i < this.Prefab.DecorativeSpriteGroups[spriteGroup].Length; i++)
				{
					DecorativeSprite decorativeSprite = this.Prefab.DecorativeSpriteGroups[spriteGroup][i];
					if (decorativeSprite != null)
					{
						if (spriteGroup > 0)
						{
							int activeSpriteIndex = (int)this.ID % this.Prefab.DecorativeSpriteGroups[spriteGroup].Length;
							if (i != activeSpriteIndex)
							{
								this.spriteAnimState[decorativeSprite].IsActive = false;
								goto IL_193;
							}
						}
						DecorativeSprite.State spriteState = this.spriteAnimState[decorativeSprite];
						spriteState.IsActive = true;
						foreach (PropertyConditional conditional in decorativeSprite.IsActiveConditionals)
						{
							if (!this.ConditionalMatches(conditional))
							{
								spriteState.IsActive = false;
								break;
							}
						}
						if (spriteState.IsActive)
						{
							bool animate = true;
							foreach (PropertyConditional conditional2 in decorativeSprite.AnimationConditionals)
							{
								if (!this.ConditionalMatches(conditional2))
								{
									animate = false;
									break;
								}
							}
							if (animate)
							{
								spriteState.OffsetState += deltaTime;
								spriteState.RotationState += deltaTime;
							}
						}
					}
					IL_193:;
				}
			}
		}

		// Token: 0x060020AB RID: 8363 RVA: 0x001484D0 File Offset: 0x001466D0
		private bool ConditionalMatches(PropertyConditional conditional)
		{
			return string.IsNullOrEmpty(conditional.TargetItemComponent) && conditional.Matches(this);
		}

		// Token: 0x060020AC RID: 8364 RVA: 0x001484F0 File Offset: 0x001466F0
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			byte sectionCount = msg.ReadByte();
			bool invalidMessage = false;
			if ((int)sectionCount != this.Sections.Length)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(109, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Error while reading a network event for the structure \"");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler.AppendLiteral(")\". Section count does not match (server: ");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(sectionCount);
				defaultInterpolatedStringHandler.AppendLiteral(" client: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Sections.Length);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				GameAnalyticsManager.AddErrorEventOnce("Structure.ClientRead:SectionCountMismatch", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				throw new Exception(errorMsg);
			}
			for (int i = 0; i < (int)sectionCount; i++)
			{
				float damage = msg.ReadRangedSingle(0f, 1f, 8) * this.MaxHealth;
				if (!invalidMessage && i < this.Sections.Length)
				{
					this.SetDamage(i, damage, null, true, true, true, false);
				}
			}
		}

		// Token: 0x170008CB RID: 2251
		// (get) Token: 0x060020AD RID: 8365 RVA: 0x001485F0 File Offset: 0x001467F0
		public override ContentPackage ContentPackage
		{
			get
			{
				StructurePrefab prefab = this.Prefab;
				if (prefab == null)
				{
					return null;
				}
				return prefab.ContentPackage;
			}
		}

		// Token: 0x170008CC RID: 2252
		// (get) Token: 0x060020AE RID: 8366 RVA: 0x00148603 File Offset: 0x00146803
		// (set) Token: 0x060020AF RID: 8367 RVA: 0x0014860B File Offset: 0x0014680B
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		public bool Indestructible { get; set; }

		// Token: 0x170008CD RID: 2253
		// (get) Token: 0x060020B0 RID: 8368 RVA: 0x00148614 File Offset: 0x00146814
		// (set) Token: 0x060020B1 RID: 8369 RVA: 0x0014861C File Offset: 0x0014681C
		public WallSection[] Sections { get; private set; }

		// Token: 0x170008CE RID: 2254
		// (get) Token: 0x060020B2 RID: 8370 RVA: 0x00148625 File Offset: 0x00146825
		public override Sprite Sprite
		{
			get
			{
				return this.Prefab.Sprite;
			}
		}

		// Token: 0x170008CF RID: 2255
		// (get) Token: 0x060020B3 RID: 8371 RVA: 0x00148632 File Offset: 0x00146832
		public bool IsPlatform
		{
			get
			{
				return this.Prefab.Platform;
			}
		}

		// Token: 0x170008D0 RID: 2256
		// (get) Token: 0x060020B4 RID: 8372 RVA: 0x0014863F File Offset: 0x0014683F
		// (set) Token: 0x060020B5 RID: 8373 RVA: 0x00148647 File Offset: 0x00146847
		public Direction StairDirection { get; private set; }

		// Token: 0x170008D1 RID: 2257
		// (get) Token: 0x060020B6 RID: 8374 RVA: 0x00148650 File Offset: 0x00146850
		public override string Name
		{
			get
			{
				return this.Prefab.Name.Value;
			}
		}

		// Token: 0x170008D2 RID: 2258
		// (get) Token: 0x060020B7 RID: 8375 RVA: 0x00148662 File Offset: 0x00146862
		public bool HasBody
		{
			get
			{
				return this.Prefab.Body && !this.DisableCollision;
			}
		}

		// Token: 0x170008D3 RID: 2259
		// (get) Token: 0x060020B8 RID: 8376 RVA: 0x0014867C File Offset: 0x0014687C
		// (set) Token: 0x060020B9 RID: 8377 RVA: 0x00148684 File Offset: 0x00146884
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBodyByDefault, true)]
		public bool DisableCollision { get; set; }

		// Token: 0x170008D4 RID: 2260
		// (get) Token: 0x060020BA RID: 8378 RVA: 0x0014868D File Offset: 0x0014688D
		// (set) Token: 0x060020BB RID: 8379 RVA: 0x00148695 File Offset: 0x00146895
		public List<Body> Bodies { get; private set; }

		// Token: 0x170008D5 RID: 2261
		// (get) Token: 0x060020BC RID: 8380 RVA: 0x0014869E File Offset: 0x0014689E
		// (set) Token: 0x060020BD RID: 8381 RVA: 0x001486A6 File Offset: 0x001468A6
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		public bool CastShadow { get; set; }

		// Token: 0x170008D6 RID: 2262
		// (get) Token: 0x060020BE RID: 8382 RVA: 0x001486AF File Offset: 0x001468AF
		public bool IsHorizontal { get; }

		// Token: 0x170008D7 RID: 2263
		// (get) Token: 0x060020BF RID: 8383 RVA: 0x001486B7 File Offset: 0x001468B7
		public int SectionCount
		{
			get
			{
				return this.Sections.Length;
			}
		}

		// Token: 0x170008D8 RID: 2264
		// (get) Token: 0x060020C0 RID: 8384 RVA: 0x001486C4 File Offset: 0x001468C4
		// (set) Token: 0x060020C1 RID: 8385 RVA: 0x001486F4 File Offset: 0x001468F4
		[Serialize(100f, IsPropertySaveable.Yes, "", "", false)]
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true, MinValueFloat = 0f)]
		public float MaxHealth
		{
			get
			{
				float? num = this.maxHealth;
				if (num == null)
				{
					return this.Prefab.Health;
				}
				return num.GetValueOrDefault();
			}
			set
			{
				this.maxHealth = new float?(value);
			}
		}

		// Token: 0x170008D9 RID: 2265
		// (get) Token: 0x060020C2 RID: 8386 RVA: 0x00148702 File Offset: 0x00146902
		// (set) Token: 0x060020C3 RID: 8387 RVA: 0x0014870A File Offset: 0x0014690A
		[Serialize(3500f, IsPropertySaveable.Yes, "", "", false)]
		public float CrushDepth
		{
			get
			{
				return this.crushDepth;
			}
			set
			{
				this.crushDepth = Math.Max(value, 3500f);
			}
		}

		// Token: 0x170008DA RID: 2266
		// (get) Token: 0x060020C4 RID: 8388 RVA: 0x0014871D File Offset: 0x0014691D
		public float Health
		{
			get
			{
				return this.MaxHealth;
			}
		}

		// Token: 0x170008DB RID: 2267
		// (get) Token: 0x060020C5 RID: 8389 RVA: 0x00148725 File Offset: 0x00146925
		public override bool DrawBelowWater
		{
			get
			{
				return base.DrawBelowWater || this.Prefab.BackgroundSprite != null;
			}
		}

		// Token: 0x170008DC RID: 2268
		// (get) Token: 0x060020C6 RID: 8390 RVA: 0x0014873F File Offset: 0x0014693F
		public override bool DrawOverWater
		{
			get
			{
				return (this.Sprite == null || base.SpriteDepth <= 0.5f) && !this.DrawDamageEffect;
			}
		}

		// Token: 0x170008DD RID: 2269
		// (get) Token: 0x060020C7 RID: 8391 RVA: 0x00148761 File Offset: 0x00146961
		public bool DrawDamageEffect
		{
			get
			{
				return this.Prefab.Body && !this.IsPlatform;
			}
		}

		// Token: 0x170008DE RID: 2270
		// (get) Token: 0x060020C8 RID: 8392 RVA: 0x0014877B File Offset: 0x0014697B
		// (set) Token: 0x060020C9 RID: 8393 RVA: 0x00148783 File Offset: 0x00146983
		public bool HasDamage { get; private set; }

		// Token: 0x170008DF RID: 2271
		// (get) Token: 0x060020CA RID: 8394 RVA: 0x0014878C File Offset: 0x0014698C
		public new StructurePrefab Prefab
		{
			get
			{
				return this.Prefab as StructurePrefab;
			}
		}

		// Token: 0x170008E0 RID: 2272
		// (get) Token: 0x060020CB RID: 8395 RVA: 0x00148799 File Offset: 0x00146999
		public ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return this.Prefab.Tags;
			}
		}

		// Token: 0x170008E1 RID: 2273
		// (get) Token: 0x060020CC RID: 8396 RVA: 0x001487A6 File Offset: 0x001469A6
		// (set) Token: 0x060020CD RID: 8397 RVA: 0x001487AE File Offset: 0x001469AE
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string SpecialTag { get; set; }

		// Token: 0x170008E2 RID: 2274
		// (get) Token: 0x060020CE RID: 8398 RVA: 0x001487B7 File Offset: 0x001469B7
		// (set) Token: 0x060020CF RID: 8399 RVA: 0x001487BF File Offset: 0x001469BF
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

		// Token: 0x170008E3 RID: 2275
		// (get) Token: 0x060020D0 RID: 8400 RVA: 0x001487C8 File Offset: 0x001469C8
		// (set) Token: 0x060020D1 RID: 8401 RVA: 0x001487D0 File Offset: 0x001469D0
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool UseDropShadow { get; private set; }

		// Token: 0x170008E4 RID: 2276
		// (get) Token: 0x060020D2 RID: 8402 RVA: 0x001487D9 File Offset: 0x001469D9
		// (set) Token: 0x060020D3 RID: 8403 RVA: 0x001487E1 File Offset: 0x001469E1
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasBody, true)]
		[Serialize("0,0", IsPropertySaveable.Yes, "The position of the drop shadow relative to the structure. If set to zero, the shadow is positioned automatically so that it points towards the sub's center of mass.", "", false)]
		public Vector2 DropShadowOffset { get; private set; }

		// Token: 0x170008E5 RID: 2277
		// (get) Token: 0x060020D4 RID: 8404 RVA: 0x001487EA File Offset: 0x001469EA
		// (set) Token: 0x060020D5 RID: 8405 RVA: 0x001487F4 File Offset: 0x001469F4
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
				this.scale = MathHelper.Clamp(value, 0.1f, 10f);
				float relativeScale = this.scale / this.Prefab.Scale;
				if (!base.ResizeHorizontal || !base.ResizeVertical)
				{
					int newWidth = Math.Max(base.ResizeHorizontal ? this.rect.Width : ((int)((float)this.defaultRect.Width * relativeScale)), 1);
					int newHeight = Math.Max(base.ResizeVertical ? this.rect.Height : ((int)((float)this.defaultRect.Height * relativeScale)), 1);
					this.Rect = new Rectangle(this.rect.X, this.rect.Y, newWidth, newHeight);
					if (this.StairDirection != Direction.None)
					{
						this.CreateStairBodies();
					}
					else if (this.Sections != null)
					{
						this.UpdateSections();
					}
				}
				foreach (LightSource light in this.Lights)
				{
					light.SpriteScale = this.scale * this.textureScale;
				}
			}
		}

		// Token: 0x170008E6 RID: 2278
		// (get) Token: 0x060020D6 RID: 8406 RVA: 0x00148938 File Offset: 0x00146B38
		// (set) Token: 0x060020D7 RID: 8407 RVA: 0x00148945 File Offset: 0x00146B45
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
				this.RotationRad = MathHelper.WrapAngle(MathHelper.ToRadians(value));
				if (this.StairDirection != Direction.None)
				{
					this.CreateStairBodies();
					return;
				}
				if (this.Prefab.Body)
				{
					this.CreateSections();
					this.UpdateSections();
				}
			}
		}

		// Token: 0x170008E7 RID: 2279
		// (get) Token: 0x060020D8 RID: 8408 RVA: 0x00148980 File Offset: 0x00146B80
		// (set) Token: 0x060020D9 RID: 8409 RVA: 0x00148988 File Offset: 0x00146B88
		[Editable(DecimalCount = 3, MinValueFloat = 0.01f, MaxValueFloat = 10f, ValueStep = 0.1f)]
		[Serialize("1.0, 1.0", IsPropertySaveable.No, "", "", false)]
		public Vector2 TextureScale
		{
			get
			{
				return this.textureScale;
			}
			set
			{
				this.textureScale = new Vector2(MathHelper.Clamp(value.X, 0.01f, 10f), MathHelper.Clamp(value.Y, 0.01f, 10f));
				foreach (LightSource light in this.Lights)
				{
					light.LightTextureScale = this.textureScale * this.scale;
				}
			}
		}

		// Token: 0x170008E8 RID: 2280
		// (get) Token: 0x060020DA RID: 8410 RVA: 0x00148A20 File Offset: 0x00146C20
		// (set) Token: 0x060020DB RID: 8411 RVA: 0x00148A28 File Offset: 0x00146C28
		public float ScaleWhenTextureOffsetSet { get; private set; } = 1f;

		// Token: 0x170008E9 RID: 2281
		// (get) Token: 0x060020DC RID: 8412 RVA: 0x00148A31 File Offset: 0x00146C31
		// (set) Token: 0x060020DD RID: 8413 RVA: 0x00148A3C File Offset: 0x00146C3C
		[Editable(ForceShowPlusMinusButtons = true, ValueStep = 1f)]
		[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "", "", false)]
		public Vector2 TextureOffset
		{
			get
			{
				return this.textureOffset;
			}
			set
			{
				this.textureOffset = value;
				this.textureOffset.X = MathUtils.PositiveModulo(this.textureOffset.X, (float)this.Sprite.SourceRect.Width * this.TextureScale.X * this.Scale);
				this.textureOffset.Y = MathUtils.PositiveModulo(this.textureOffset.Y, (float)this.Sprite.SourceRect.Height * this.TextureScale.Y * this.Scale);
				this.ScaleWhenTextureOffsetSet = this.Scale;
				this.SetLightTextureOffset();
			}
		}

		// Token: 0x170008EA RID: 2282
		// (get) Token: 0x060020DE RID: 8414 RVA: 0x00148AE0 File Offset: 0x00146CE0
		// (set) Token: 0x060020DF RID: 8415 RVA: 0x00148AE8 File Offset: 0x00146CE8
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

		// Token: 0x170008EB RID: 2283
		// (get) Token: 0x060020E0 RID: 8416 RVA: 0x00148AF1 File Offset: 0x00146CF1
		// (set) Token: 0x060020E1 RID: 8417 RVA: 0x00148AFC File Offset: 0x00146CFC
		public override Rectangle Rect
		{
			get
			{
				return base.Rect;
			}
			set
			{
				Rectangle oldRect = this.Rect;
				base.Rect = value;
				if (this.Prefab.Body)
				{
					this.CreateSections();
					this.UpdateSections();
					return;
				}
				if (this.Sections == null)
				{
					return;
				}
				foreach (WallSection sec in this.Sections)
				{
					Rectangle secRect = sec.rect;
					secRect.X -= oldRect.X;
					secRect.Y -= oldRect.Y;
					secRect.X *= value.Width;
					secRect.X /= oldRect.Width;
					secRect.Y *= value.Height;
					secRect.Y /= oldRect.Height;
					secRect.Width *= value.Width;
					secRect.Width /= oldRect.Width;
					secRect.Height *= value.Height;
					secRect.Height /= oldRect.Height;
					secRect.X += value.X;
					secRect.Y += value.Y;
					sec.rect = secRect;
				}
			}
		}

		// Token: 0x170008EC RID: 2284
		// (get) Token: 0x060020E2 RID: 8418 RVA: 0x00148C35 File Offset: 0x00146E35
		public float BodyWidth
		{
			get
			{
				if (this.Prefab.BodyWidth <= 0f)
				{
					return (float)this.rect.Width;
				}
				return this.Prefab.BodyWidth * this.scale;
			}
		}

		// Token: 0x170008ED RID: 2285
		// (get) Token: 0x060020E3 RID: 8419 RVA: 0x00148C68 File Offset: 0x00146E68
		public float BodyHeight
		{
			get
			{
				if (this.Prefab.BodyHeight <= 0f)
				{
					return (float)this.rect.Height;
				}
				return this.Prefab.BodyHeight * this.scale;
			}
		}

		// Token: 0x170008EE RID: 2286
		// (get) Token: 0x060020E4 RID: 8420 RVA: 0x00148C9C File Offset: 0x00146E9C
		public float BodyRotation
		{
			get
			{
				float rotation = MathHelper.ToRadians(this.Prefab.BodyRotation) + this.RotationRad;
				if (this.IsHorizontal)
				{
					if (base.FlippedX)
					{
						rotation = -3.1415927f - rotation;
					}
					if (base.FlippedY)
					{
						rotation = -rotation;
					}
				}
				else
				{
					if (base.FlippedX)
					{
						rotation = -rotation;
					}
					if (base.FlippedY)
					{
						rotation = -3.1415927f - rotation;
					}
				}
				return MathHelper.WrapAngle(rotation);
			}
		}

		// Token: 0x170008EF RID: 2287
		// (get) Token: 0x060020E5 RID: 8421 RVA: 0x00148D0C File Offset: 0x00146F0C
		public Vector2 BodyOffset
		{
			get
			{
				Vector2 bodyOffset = this.Prefab.BodyOffset;
				if (this.RotationRad != 0f)
				{
					bodyOffset = MathUtils.RotatePoint(bodyOffset, -this.RotationRad);
				}
				if (base.FlippedX)
				{
					bodyOffset.X = -bodyOffset.X;
				}
				if (base.FlippedY)
				{
					bodyOffset.Y = -bodyOffset.Y;
				}
				return bodyOffset;
			}
		}

		// Token: 0x170008F0 RID: 2288
		// (get) Token: 0x060020E6 RID: 8422 RVA: 0x00148D6D File Offset: 0x00146F6D
		// (set) Token: 0x060020E7 RID: 8423 RVA: 0x00148D75 File Offset: 0x00146F75
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool NoAITarget { get; private set; }

		// Token: 0x170008F1 RID: 2289
		// (get) Token: 0x060020E8 RID: 8424 RVA: 0x00148D7E File Offset: 0x00146F7E
		// (set) Token: 0x060020E9 RID: 8425 RVA: 0x00148D86 File Offset: 0x00146F86
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x060020EA RID: 8426 RVA: 0x00148D90 File Offset: 0x00146F90
		public override void Move(Vector2 amount, bool ignoreContacts = true)
		{
			if (!MathUtils.IsValid(amount))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to move a structure by an invalid amount (");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(amount);
				defaultInterpolatedStringHandler.AppendLiteral(")\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			base.Move(amount, ignoreContacts);
			for (int i = 0; i < this.Sections.Length; i++)
			{
				Rectangle r = this.Sections[i].rect;
				r.X += (int)amount.X;
				r.Y += (int)amount.Y;
				this.Sections[i].rect = r;
			}
			if (this.Bodies != null)
			{
				Vector2 simAmount = ConvertUnits.ToSimUnits(amount);
				foreach (Body b in this.Bodies)
				{
					Vector2 pos = b.Position + simAmount;
					if (ignoreContacts)
					{
						b.SetTransformIgnoreContacts(ref pos, b.Rotation);
					}
					else
					{
						b.SetTransform(pos, b.Rotation);
					}
				}
			}
			List<ConvexHull> list = this.convexHulls;
			if (list != null)
			{
				list.ForEach(delegate(ConvexHull x)
				{
					x.Move(amount);
				});
			}
			foreach (LightSource light in this.Lights)
			{
				light.LightTextureTargetSize = this.rect.Size.ToVector2();
				light.Position = this.rect.Location.ToVector2();
			}
		}

		// Token: 0x060020EB RID: 8427 RVA: 0x00148F90 File Offset: 0x00147190
		public Structure(Rectangle rectangle, StructurePrefab sp, Submarine submarine, ushort id = 0, XElement element = null) : base(sp, submarine, id)
		{
			if (rectangle.Width == 0 || rectangle.Height == 0)
			{
				return;
			}
			this.defaultRect = rectangle;
			this.maxHealth = new float?(sp.Health);
			this.rect = rectangle;
			this.TextureScale = sp.TextureScale;
			this.spriteColor = this.Prefab.SpriteColor;
			if (sp.IsHorizontal != null)
			{
				this.IsHorizontal = sp.IsHorizontal.Value;
			}
			else if (base.ResizeHorizontal && !base.ResizeVertical)
			{
				this.IsHorizontal = 1;
			}
			else if (base.ResizeVertical && !base.ResizeHorizontal)
			{
				this.IsHorizontal = 0;
			}
			else
			{
				float width = (this.BodyWidth > 0f) ? this.BodyWidth : ((float)this.rect.Width);
				float height = (this.BodyHeight > 0f) ? this.BodyHeight : ((float)this.rect.Height);
				if (this.BodyWidth > 0f && this.BodyHeight > 0f)
				{
					this.IsHorizontal = (width > height);
				}
			}
			this.StairDirection = this.Prefab.StairDirection;
			this.NoAITarget = this.Prefab.NoAITarget;
			this.InitProjSpecific();
			this.SerializableProperties = ((element != null) ? SerializableProperty.DeserializeProperties(this, element) : SerializableProperty.GetProperties(this));
			if (((element != null) ? element.GetAttribute("CastShadow", StringComparison.OrdinalIgnoreCase) : null) == null)
			{
				this.CastShadow = this.Prefab.CastShadow;
			}
			if (((element != null) ? element.GetAttribute("Indestructible", StringComparison.OrdinalIgnoreCase) : null) == null)
			{
				this.Indestructible = this.Prefab.ConfigElement.GetAttributeBool("Indestructible", false);
			}
			if (this.Prefab.Body)
			{
				Structure.WallList.Add(this);
			}
			if (this.HasBody)
			{
				this.Bodies = new List<Body>();
				this.CreateSections();
				this.UpdateSections();
			}
			else if (this.StairDirection != Direction.None)
			{
				this.CreateStairBodies();
			}
			if (this.Sections == null)
			{
				this.Sections = new WallSection[1];
				this.Sections[0] = new WallSection(this.rect, this, 0f);
			}
			foreach (ContentXElement subElement in sp.ConfigElement.Elements())
			{
				if (subElement.Name.ToString().Equals("light", StringComparison.OrdinalIgnoreCase))
				{
					this.rect.Location.ToVector2().Y += (float)this.rect.Height;
					LightSource lightSource = new LightSource(subElement, null);
					lightSource.ParentSub = base.Submarine;
					lightSource.Position = this.rect.Location.ToVector2();
					lightSource.CastShadows = false;
					lightSource.IsBackground = false;
					ContentXElement contentXElement = subElement;
					string key = "lightcolor";
					Color white = Color.White;
					lightSource.Color = contentXElement.GetAttributeColor(key, white);
					lightSource.SpriteScale = Vector2.One;
					lightSource.Range = 0f;
					lightSource.LightTextureTargetSize = this.rect.Size.ToVector2();
					lightSource.LightTextureScale = this.textureScale * this.scale;
					lightSource.LightSourceParams.Flicker = subElement.GetAttributeFloat("flicker", 0f);
					lightSource.LightSourceParams.FlickerSpeed = subElement.GetAttributeFloat("flickerspeed", 0f);
					lightSource.LightSourceParams.PulseAmount = subElement.GetAttributeFloat("pulseamount", 0f);
					lightSource.LightSourceParams.PulseFrequency = subElement.GetAttributeFloat("pulsefrequency", 0f);
					lightSource.LightSourceParams.BlinkFrequency = subElement.GetAttributeFloat("blinkfrequency", 0f);
					LightSource light = lightSource;
					this.Lights.Add(light);
					this.SetLightTextureOffset();
				}
			}
			if (this.aiTarget == null && this.HasBody && this.Tags.Contains("wall") && submarine != null && !submarine.Info.IsWreck && !this.NoAITarget)
			{
				this.aiTarget = new AITarget(this)
				{
					MinSightRange = 1000f,
					MaxSightRange = 4000f,
					MaxSoundRange = 0f
				};
			}
			base.InsertToList();
			DebugConsole.Log(string.Concat(new string[]
			{
				"Created ",
				this.Name,
				" (",
				this.ID.ToString(),
				")"
			}));
		}

		// Token: 0x060020EC RID: 8428 RVA: 0x00149494 File Offset: 0x00147694
		private void InitProjSpecific()
		{
			Sprite sprite = this.Prefab.Sprite;
			if (sprite != null)
			{
				sprite.EnsureLazyLoaded(false);
			}
			Sprite backgroundSprite = this.Prefab.BackgroundSprite;
			if (backgroundSprite != null)
			{
				backgroundSprite.EnsureLazyLoaded(false);
			}
			foreach (DecorativeSprite decorativeSprite in this.Prefab.DecorativeSprites)
			{
				decorativeSprite.Sprite.EnsureLazyLoaded(false);
				this.spriteAnimState.Add(decorativeSprite, new DecorativeSprite.State());
			}
			this.UpdateSpriteStates(0f);
		}

		// Token: 0x060020ED RID: 8429 RVA: 0x0014951B File Offset: 0x0014771B
		public override string ToString()
		{
			return this.Name;
		}

		// Token: 0x060020EE RID: 8430 RVA: 0x00149524 File Offset: 0x00147724
		public override MapEntity Clone()
		{
			Structure clone = new Structure(this.rect, this.Prefab, base.Submarine, 0, null)
			{
				defaultRect = this.defaultRect
			};
			foreach (KeyValuePair<Identifier, SerializableProperty> property in this.SerializableProperties)
			{
				if (property.Value.Attributes.OfType<Serialize>().Any<Serialize>())
				{
					clone.SerializableProperties[property.Key].TrySetValue(clone, property.Value.GetValue(this));
				}
			}
			if (base.FlippedX)
			{
				clone.FlipX(false, false);
			}
			if (base.FlippedY)
			{
				clone.FlipY(false, false);
			}
			return clone;
		}

		// Token: 0x060020EF RID: 8431 RVA: 0x001495F8 File Offset: 0x001477F8
		private void CreateStairBodies()
		{
			this.Bodies = new List<Body>();
			this.bodyDimensions.Clear();
			float stairAngle = MathHelper.ToRadians(Math.Min(this.Prefab.StairAngle, 75f));
			float bodyWidth = ConvertUnits.ToSimUnits((double)this.rect.Width / Math.Cos((double)stairAngle));
			float bodyHeight = ConvertUnits.ToSimUnits(10);
			float stairHeight = (float)this.rect.Width * (float)Math.Tan((double)stairAngle);
			Body newBody = GameMain.World.CreateRectangle(bodyWidth, bodyHeight, 1.5f, default(Vector2), 0f, BodyType.Static, Category.Cat1, Category.All, true);
			float rotationWithFlip = base.RotationRadWithFlipping;
			newBody.BodyType = BodyType.Static;
			Vector2 stairRectHeightDiff = new Vector2(0f, stairHeight / 2f - (float)this.rect.Height / 2f);
			stairRectHeightDiff = MathUtils.RotatePoint(stairRectHeightDiff, -rotationWithFlip);
			if (base.FlippedY)
			{
				stairRectHeightDiff = -stairRectHeightDiff;
			}
			Vector2 stairPos = new Vector2(this.Position.X, (float)this.rect.Y - (float)this.rect.Height / 2f) + stairRectHeightDiff;
			newBody.Rotation = ((this.StairDirection == Direction.Right) ? stairAngle : (-stairAngle)) - rotationWithFlip;
			newBody.CollisionCategories = Category.Cat4;
			newBody.Friction = 0.8f;
			newBody.UserData = this;
			newBody.Position = ConvertUnits.ToSimUnits(stairPos) + ConvertUnits.ToSimUnits(this.BodyOffset) * this.Scale;
			this.bodyDimensions.Add(newBody, new Vector2(bodyWidth, bodyHeight));
			this.Bodies.Add(newBody);
		}

		// Token: 0x060020F0 RID: 8432 RVA: 0x001497A4 File Offset: 0x001479A4
		private void CreateSections()
		{
			int xsections = 1;
			int ysections = 1;
			int width = this.rect.Width;
			int height = this.rect.Height;
			WallSection[] prevSections = null;
			if (this.Sections != null)
			{
				prevSections = this.Sections.ToArray<WallSection>();
			}
			if (!this.Prefab.Body)
			{
				if (base.FlippedX && this.IsHorizontal)
				{
					xsections = (int)Math.Ceiling((double)((float)this.rect.Width / (float)this.Prefab.Sprite.SourceRect.Width));
					width = this.Prefab.Sprite.SourceRect.Width;
				}
				else if (base.FlippedY && !this.IsHorizontal)
				{
					ysections = (int)Math.Ceiling((double)((float)this.rect.Height / (float)this.Prefab.Sprite.SourceRect.Height));
					width = this.Prefab.Sprite.SourceRect.Height;
				}
				else
				{
					xsections = 1;
					ysections = 1;
				}
				this.Sections = new WallSection[Math.Max(xsections, ysections)];
			}
			else if (this.IsHorizontal)
			{
				xsections = (this.rect.Width + 96 - 1) / 96;
				this.Sections = new WallSection[xsections];
				width = 96;
			}
			else
			{
				ysections = (this.rect.Height + 96 - 1) / 96;
				this.Sections = new WallSection[ysections];
				height = 96;
			}
			for (int x = 0; x < xsections; x++)
			{
				for (int y = 0; y < ysections; y++)
				{
					if (base.FlippedX || base.FlippedY)
					{
						Rectangle sectionRect = new Rectangle(base.FlippedX ? (this.rect.Right - (x + 1) * width) : (this.rect.X + x * width), base.FlippedY ? (this.rect.Y - this.rect.Height + (y + 1) * height) : (this.rect.Y - y * height), width, height);
						if (base.FlippedX)
						{
							int over = Math.Max(this.rect.X - sectionRect.X, 0);
							sectionRect.X += over;
							sectionRect.Width -= over;
						}
						else
						{
							sectionRect.Width -= (int)Math.Max((float)(sectionRect.Right - this.rect.Right), 0f);
						}
						if (base.FlippedY)
						{
							int over2 = Math.Max(sectionRect.Y - this.rect.Y, 0);
							sectionRect.Y -= over2;
							sectionRect.Height -= over2;
						}
						else
						{
							sectionRect.Height -= (int)Math.Max((float)(this.rect.Y - this.rect.Height - (sectionRect.Y - sectionRect.Height)), 0f);
						}
						int xIndex = (base.FlippedX && this.IsHorizontal) ? (xsections - 1 - x) : x;
						int yIndex = (base.FlippedY && !this.IsHorizontal) ? (ysections - 1 - y) : y;
						this.Sections[xIndex + yIndex] = new WallSection(sectionRect, this, 0f);
					}
					else
					{
						Rectangle sectionRect2 = new Rectangle(this.rect.X + x * width, this.rect.Y - y * height, width, height);
						sectionRect2.Width -= (int)Math.Max((float)(sectionRect2.Right - this.rect.Right), 0f);
						sectionRect2.Height -= (int)Math.Max((float)(this.rect.Y - this.rect.Height - (sectionRect2.Y - sectionRect2.Height)), 0f);
						this.Sections[x + y] = new WallSection(sectionRect2, this, 0f);
					}
				}
			}
			if (prevSections != null && this.Sections.Length == prevSections.Length)
			{
				for (int i = 0; i < this.Sections.Length; i++)
				{
					this.Sections[i].damage = prevSections[i].damage;
				}
			}
		}

		// Token: 0x060020F1 RID: 8433 RVA: 0x00149BD8 File Offset: 0x00147DD8
		private Rectangle GenerateMergedRect(List<WallSection> mergedSections)
		{
			if (this.IsHorizontal)
			{
				return new Rectangle(mergedSections.Min((WallSection x) => x.rect.Left), mergedSections.Max((WallSection x) => x.rect.Top), mergedSections.Sum((WallSection x) => x.rect.Width), mergedSections.First<WallSection>().rect.Height);
			}
			return new Rectangle(mergedSections.Min((WallSection x) => x.rect.Left), mergedSections.Max((WallSection x) => x.rect.Top), mergedSections.First<WallSection>().rect.Width, mergedSections.Sum((WallSection x) => x.rect.Height));
		}

		// Token: 0x060020F2 RID: 8434 RVA: 0x00149CFC File Offset: 0x00147EFC
		public override Quad2D GetTransformedQuad()
		{
			return Quad2D.FromSubmarineRectangle(this.rect).Rotated((base.FlippedX != base.FlippedY) ? this.RotationRad : (-this.RotationRad));
		}

		// Token: 0x060020F3 RID: 8435 RVA: 0x00149D40 File Offset: 0x00147F40
		public static Structure GetAttachTarget(Vector2 worldPosition)
		{
			foreach (MapEntity mapEntity in MapEntity.MapEntityList)
			{
				Structure structure = mapEntity as Structure;
				if (structure != null && structure.Prefab.AllowAttachItems && (structure.Bodies == null || structure.Bodies.Count <= 0))
				{
					Rectangle worldRect = mapEntity.WorldRect;
					if (worldPosition.X >= (float)worldRect.X && worldPosition.X <= (float)worldRect.Right && worldPosition.Y <= (float)worldRect.Y && worldPosition.Y >= (float)(worldRect.Y - worldRect.Height))
					{
						return structure;
					}
				}
			}
			return null;
		}

		// Token: 0x060020F4 RID: 8436 RVA: 0x00149E14 File Offset: 0x00148014
		public override bool IsMouseOn(Vector2 position)
		{
			if (this.StairDirection == Direction.None)
			{
				Vector2 rectSize = this.rect.Size.ToVector2();
				if (this.BodyWidth > 0f)
				{
					rectSize.X = this.BodyWidth;
				}
				if (this.BodyHeight > 0f)
				{
					rectSize.Y = this.BodyHeight;
				}
				Vector2 bodyPos = this.WorldPosition + this.BodyOffset * this.Scale;
				Vector2 transformedMousePos = MathUtils.RotatePointAroundTarget(position, bodyPos, this.BodyRotation, true);
				return Math.Abs(transformedMousePos.X - bodyPos.X) < rectSize.X / 2f && Math.Abs(transformedMousePos.Y - bodyPos.Y) < rectSize.Y / 2f;
			}
			Vector2 transformedMousePos2 = MathUtils.RotatePointAroundTarget(position, base.WorldRect.Location.ToVector2() + base.WorldRect.Size.ToVector2().FlipY() * 0.5f, this.BodyRotation, true);
			if (!Submarine.RectContains(base.WorldRect, position, false))
			{
				return false;
			}
			if (this.StairDirection == Direction.Left)
			{
				return MathUtils.LineToPointDistanceSquared(new Vector2((float)base.WorldRect.X, (float)base.WorldRect.Y), new Vector2((float)base.WorldRect.Right, (float)(base.WorldRect.Y - base.WorldRect.Height)), transformedMousePos2) < 1600f;
			}
			return MathUtils.LineToPointDistanceSquared(new Vector2((float)base.WorldRect.X, (float)(base.WorldRect.Y - this.rect.Height)), new Vector2((float)base.WorldRect.Right, (float)base.WorldRect.Y), transformedMousePos2) < 1600f;
		}

		// Token: 0x060020F5 RID: 8437 RVA: 0x0014A000 File Offset: 0x00148200
		public override void ShallowRemove()
		{
			base.ShallowRemove();
			if (Structure.WallList.Contains(this))
			{
				Structure.WallList.Remove(this);
			}
			if (this.Bodies != null)
			{
				foreach (Body b in this.Bodies)
				{
					GameMain.World.Remove(b);
				}
				this.Bodies.Clear();
			}
			if (this.Sections != null)
			{
				foreach (WallSection s in this.Sections)
				{
					if (s.gap != null)
					{
						s.gap.Remove();
						s.gap = null;
					}
				}
			}
			if (this.convexHulls != null)
			{
				this.convexHulls.ForEach(delegate(ConvexHull x)
				{
					x.Remove();
				});
			}
			foreach (LightSource light in this.Lights)
			{
				light.Remove();
			}
		}

		// Token: 0x060020F6 RID: 8438 RVA: 0x0014A140 File Offset: 0x00148340
		public override void Remove()
		{
			base.Remove();
			if (Structure.WallList.Contains(this))
			{
				Structure.WallList.Remove(this);
			}
			if (this.Bodies != null)
			{
				foreach (Body b in this.Bodies)
				{
					GameMain.World.Remove(b);
				}
				this.Bodies.Clear();
			}
			if (this.Sections != null)
			{
				foreach (WallSection s in this.Sections)
				{
					if (s.gap != null)
					{
						s.gap.Remove();
						s.gap = null;
					}
				}
			}
			if (this.convexHulls != null)
			{
				this.convexHulls.ForEach(delegate(ConvexHull x)
				{
					x.Remove();
				});
			}
			foreach (LightSource light in this.Lights)
			{
				light.Remove();
			}
		}

		// Token: 0x060020F7 RID: 8439 RVA: 0x0014A280 File Offset: 0x00148480
		private bool OnWallCollision(Fixture f1, Fixture f2, Contact contact)
		{
			if (this.Prefab.Platform)
			{
				Limb limb = f2.Body.UserData as Limb;
				if (limb != null && limb.character.AnimController.IgnorePlatforms)
				{
					return false;
				}
			}
			if (f2.Body.UserData is Limb)
			{
				Character character = ((Limb)f2.Body.UserData).character;
				if (character.DisableImpactDamageTimer > 0f || ((Limb)f2.Body.UserData).Mass < 100f)
				{
					return true;
				}
			}
			this.OnImpactProjSpecific(f1, f2, contact);
			return true;
		}

		// Token: 0x060020F8 RID: 8440 RVA: 0x0014A320 File Offset: 0x00148520
		private void OnImpactProjSpecific(Fixture f1, Fixture f2, Contact contact)
		{
			if (!this.Prefab.Platform && this.Prefab.StairDirection == Direction.None)
			{
				Vector2 pos = ConvertUnits.ToDisplayUnits(f2.Body.Position);
				int section = this.FindSectionIndex(pos, false, false);
				if (section > -1)
				{
					Vector2 normal = contact.Manifold.LocalNormal;
					float impact = Vector2.Dot(f2.Body.LinearVelocity, -normal) * f2.Body.Mass * 0.1f;
					if (impact > 10f)
					{
						SoundPlayer.PlayDamageSound("StructureBlunt", impact, this.SectionPosition(section, true), 2000f, this.Tags, 1f);
					}
				}
			}
		}

		// Token: 0x060020F9 RID: 8441 RVA: 0x0014A3C9 File Offset: 0x001485C9
		public WallSection GetSection(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return null;
			}
			return this.Sections[sectionIndex];
		}

		// Token: 0x060020FA RID: 8442 RVA: 0x0014A3E4 File Offset: 0x001485E4
		public bool SectionBodyDisabled(int sectionIndex)
		{
			return sectionIndex >= 0 && sectionIndex < this.Sections.Length && this.Sections[sectionIndex].damage >= this.MaxHealth;
		}

		// Token: 0x060020FB RID: 8443 RVA: 0x0014A410 File Offset: 0x00148610
		public bool AllSectionBodiesDisabled()
		{
			for (int i = 0; i < this.Sections.Length; i++)
			{
				if (this.Sections[i].damage < this.MaxHealth)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060020FC RID: 8444 RVA: 0x0014A448 File Offset: 0x00148648
		public bool SectionIsLeaking(int sectionIndex)
		{
			return sectionIndex >= 0 && sectionIndex < this.Sections.Length && this.Sections[sectionIndex].damage >= this.MaxHealth * 0.1f;
		}

		// Token: 0x060020FD RID: 8445 RVA: 0x0014A47C File Offset: 0x0014867C
		public bool SectionIsLeakingFromOutside(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return false;
			}
			if (this.SectionIsLeaking(sectionIndex))
			{
				Gap gap = this.Sections[sectionIndex].gap;
				return gap != null && !gap.IsRoomToRoom;
			}
			return false;
		}

		// Token: 0x060020FE RID: 8446 RVA: 0x0014A4C1 File Offset: 0x001486C1
		public int SectionLength(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return 0;
			}
			if (!this.IsHorizontal)
			{
				return this.Sections[sectionIndex].rect.Height;
			}
			return this.Sections[sectionIndex].rect.Width;
		}

		// Token: 0x060020FF RID: 8447 RVA: 0x0014A504 File Offset: 0x00148704
		public override bool AddUpgrade(Upgrade upgrade, bool createNetworkEvent = false)
		{
			if (!upgrade.Prefab.IsWallUpgrade)
			{
				return false;
			}
			Upgrade existingUpgrade = base.GetUpgrade(upgrade.Identifier);
			if (existingUpgrade != null)
			{
				existingUpgrade.Level += upgrade.Level;
				existingUpgrade.ApplyUpgrade();
				upgrade.Dispose();
			}
			else
			{
				this.Upgrades.Add(upgrade);
				upgrade.ApplyUpgrade();
			}
			this.UpdateSections();
			return true;
		}

		// Token: 0x06002100 RID: 8448 RVA: 0x0014A56C File Offset: 0x0014876C
		public void AddDamage(int sectionIndex, float damage, Character attacker = null, bool emitParticles = true, bool createWallDamageProjectiles = false)
		{
			if (!this.HasBody || this.Prefab.Platform || this.Indestructible)
			{
				return;
			}
			if (sectionIndex < 0 || sectionIndex > this.Sections.Length - 1)
			{
				return;
			}
			WallSection section = this.Sections[sectionIndex];
			float prevDamage = section.damage;
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				this.SetDamage(sectionIndex, section.damage + damage, attacker, true, true, true, createWallDamageProjectiles);
			}
			if (damage > 0f && emitParticles)
			{
				float dmg = Math.Min(section.damage - prevDamage, damage);
				float particleAmount = MathHelper.Lerp(0f, 25f, MathUtils.InverseLerp(0f, 100f, dmg * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced)));
				if (particleAmount < 1f && Rand.Value(Rand.RandSync.Unsynced) < 0.1f)
				{
					particleAmount = 1f;
				}
				int i = 1;
				while ((float)i <= particleAmount)
				{
					Rectangle worldRect = section.WorldRect;
					Vector2 directionUnitX = MathUtils.RotatedUnitXRadians(this.BodyRotation);
					Vector2 directionUnitY = directionUnitX.YX().FlipX();
					Vector2 particlePos = new Vector2((float)Rand.Range(0, worldRect.Width + 1, Rand.RandSync.Unsynced), (float)Rand.Range(-worldRect.Height, 1, Rand.RandSync.Unsynced));
					particlePos -= worldRect.Size.ToVector2().FlipY() * 0.5f;
					Vector2 particlePosFinal = this.SectionPosition(sectionIndex, true);
					particlePosFinal += particlePos.X * directionUnitX + particlePos.Y * directionUnitY;
					Particle particle = GameMain.ParticleManager.CreateParticle(this.Prefab.DamageParticle, particlePosFinal, Rand.Vector(Rand.Range(1f, 50f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced), 0f, null, 1f, null);
					if (particle == null)
					{
						break;
					}
					i++;
				}
			}
		}

		// Token: 0x06002101 RID: 8449 RVA: 0x0014A74C File Offset: 0x0014894C
		public int FindSectionIndex(Vector2 displayPos, bool world = false, bool clamp = false)
		{
			if (this.Sections.None(null))
			{
				return -1;
			}
			if (world && base.Submarine != null)
			{
				displayPos -= base.Submarine.Position;
			}
			if (this.IsHorizontal)
			{
				if (this.Sections[0].rect.Width < 96)
				{
					displayPos += this.DirectionUnit * (float)(96 - this.Sections[0].rect.Width);
				}
			}
			else if (this.Sections[0].rect.Height < 96)
			{
				displayPos += this.DirectionUnit * (float)(96 - this.Sections[0].rect.Height);
			}
			Vector2 leftmostPos = this.Position - this.DirectionUnit * (float)(this.IsHorizontal ? this.Rect.Width : this.Rect.Height) * 0.5f;
			int index = (int)Math.Floor((double)(Vector2.Dot(this.DirectionUnit, displayPos - leftmostPos) / 96f));
			if (clamp)
			{
				index = MathHelper.Clamp(index, 0, this.Sections.Length - 1);
			}
			else if (index < 0 || index > this.Sections.Length - 1)
			{
				return -1;
			}
			return index;
		}

		// Token: 0x06002102 RID: 8450 RVA: 0x0014A89C File Offset: 0x00148A9C
		public float SectionDamage(int sectionIndex)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return 0f;
			}
			return this.Sections[sectionIndex].damage;
		}

		// Token: 0x170008F2 RID: 2290
		// (get) Token: 0x06002103 RID: 8451 RVA: 0x0014A8C0 File Offset: 0x00148AC0
		protected Vector2 DirectionUnit
		{
			get
			{
				float rotation = this.IsHorizontal ? (-this.BodyRotation) : (-1.5707964f - this.BodyRotation);
				if (this.IsHorizontal && base.FlippedX)
				{
					rotation += 3.1415927f;
				}
				if (!this.IsHorizontal && base.FlippedY)
				{
					rotation += 3.1415927f;
				}
				return MathUtils.RotatedUnitXRadians(rotation);
			}
		}

		// Token: 0x06002104 RID: 8452 RVA: 0x0014A924 File Offset: 0x00148B24
		public Vector2 SectionPosition(int sectionIndex, bool world = false)
		{
			if (sectionIndex < 0 || sectionIndex >= this.Sections.Length)
			{
				return Vector2.Zero;
			}
			if (MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
			{
				Vector2 sectionPos = new Vector2((float)this.Sections[sectionIndex].rect.X + (float)this.Sections[sectionIndex].rect.Width / 2f, (float)this.Sections[sectionIndex].rect.Y - (float)this.Sections[sectionIndex].rect.Height / 2f);
				if (world && base.Submarine != null)
				{
					sectionPos += base.Submarine.Position;
				}
				return sectionPos;
			}
			Rectangle sectionRect = this.Sections[sectionIndex].rect;
			float diffFromCenter;
			if (this.IsHorizontal)
			{
				diffFromCenter = (float)(sectionRect.Center.X - this.rect.Center.X) / (float)this.rect.Width * this.BodyWidth;
			}
			else
			{
				diffFromCenter = (float)(sectionRect.Y - sectionRect.Height / 2 - (this.rect.Y - this.rect.Height / 2)) / (float)this.rect.Height * this.BodyHeight;
				diffFromCenter = -diffFromCenter;
			}
			Vector2 sectionPos2 = this.Position + this.DirectionUnit * diffFromCenter;
			if (world && base.Submarine != null)
			{
				sectionPos2 += base.Submarine.Position;
			}
			return sectionPos2;
		}

		// Token: 0x06002105 RID: 8453 RVA: 0x0014AAA4 File Offset: 0x00148CA4
		public AttackResult AddDamage(Character attacker, Vector2 worldPosition, Attack attack, Vector2 impulseDirection, float deltaTime, bool playSound = false)
		{
			if (base.Submarine != null && base.Submarine.GodMode)
			{
				return new AttackResult(0f, null);
			}
			if (!this.HasBody || this.Prefab.Platform || this.Indestructible)
			{
				return new AttackResult(0f, null);
			}
			Vector2 transformedPos = worldPosition;
			if (base.Submarine != null)
			{
				transformedPos -= base.Submarine.Position;
			}
			if (!MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
			{
				Vector2 center = this.Rect.Location.ToVector2() + this.Rect.Size.ToVector2().FlipY() * 0.5f;
				float rotation = this.BodyRotation;
				if (this.IsHorizontal && base.FlippedX)
				{
					rotation += 3.1415927f;
				}
				if (!this.IsHorizontal && base.FlippedY)
				{
					rotation += 3.1415927f;
				}
				transformedPos = MathUtils.RotatePointAroundTarget(transformedPos, center, rotation, true);
			}
			float damageAmount = 0f;
			for (int i = 0; i < this.SectionCount; i++)
			{
				Rectangle sectionRect = this.Sections[i].rect;
				sectionRect.Y -= this.Sections[i].rect.Height;
				if (MathUtils.CircleIntersectsRectangle(transformedPos, attack.DamageRange, sectionRect))
				{
					damageAmount = attack.GetStructureDamage(deltaTime);
					this.AddDamage(i, damageAmount, attacker, true, attack.CreateWallDamageProjectiles);
					if (attack.EmitStructureDamageParticles)
					{
						GameMain.ParticleManager.CreateParticle("dustcloud", this.SectionPosition(i, false), 0f, 0f, null, 0f, null);
					}
				}
			}
			if (playSound && damageAmount > 0f)
			{
				string damageSound = this.Prefab.DamageSound;
				if (string.IsNullOrWhiteSpace(damageSound))
				{
					damageSound = attack.StructureSoundType;
				}
				SoundPlayer.PlayDamageSound(damageSound, damageAmount, worldPosition, 2000f, this.Tags, 1f);
			}
			if (base.Submarine != null && damageAmount > 0f && attacker != null)
			{
				AbilityAttackerSubmarine abilityAttackerSubmarine = new AbilityAttackerSubmarine(attacker, base.Submarine);
				foreach (Character character in Character.CharacterList)
				{
					character.CheckTalents(AbilityEffectType.AfterSubmarineAttacked, abilityAttackerSubmarine);
				}
			}
			return new AttackResult(damageAmount, null);
		}

		// Token: 0x06002106 RID: 8454 RVA: 0x0014AD18 File Offset: 0x00148F18
		public void SetDamage(int sectionIndex, float damage, Character attacker = null, bool createNetworkEvent = true, bool isNetworkEvent = true, bool createExplosionEffect = true, bool createWallDamageProjectiles = false)
		{
			if ((base.Submarine != null && base.Submarine.GodMode) || (this.Indestructible && !isNetworkEvent))
			{
				return;
			}
			if (!this.HasBody)
			{
				return;
			}
			if (!MathUtils.IsValid(damage))
			{
				return;
			}
			damage = MathHelper.Clamp(damage, 0f, this.MaxHealth - this.Prefab.MinHealth);
			if (this.Sections[sectionIndex].NoPhysicsBody)
			{
				return;
			}
			if (damage < this.MaxHealth * 0.1f)
			{
				if (this.Sections[sectionIndex].gap != null)
				{
					DebugConsole.Log(string.Concat(new string[]
					{
						"Removing gap (ID ",
						this.Sections[sectionIndex].gap.ID.ToString(),
						", section: ",
						sectionIndex.ToString(),
						") from wall ",
						this.ID.ToString()
					}));
					this.Sections[sectionIndex].gap.Open = 0f;
					this.Sections[sectionIndex].gap.Remove();
					this.Sections[sectionIndex].gap = null;
				}
			}
			else
			{
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					Gap gap2 = this.Sections[sectionIndex].gap;
					float prevGapOpenState = (gap2 != null) ? gap2.Open : 0f;
					if (this.Sections[sectionIndex].gap == null)
					{
						Rectangle gapRect = this.Sections[sectionIndex].rect;
						float diffFromCenter;
						if (this.IsHorizontal)
						{
							diffFromCenter = (float)(gapRect.Center.X - this.rect.Center.X) / (float)this.rect.Width * this.BodyWidth;
							if (this.BodyWidth > 0f)
							{
								gapRect.Width = (int)(this.BodyWidth * ((float)gapRect.Width / (float)this.rect.Width));
							}
							if (this.BodyHeight > 0f)
							{
								gapRect.Y = gapRect.Y - gapRect.Height / 2 + (int)(this.BodyHeight / 2f + this.BodyOffset.Y * this.scale);
								gapRect.Height = (int)this.BodyHeight;
							}
							if (base.FlippedX)
							{
								diffFromCenter = -diffFromCenter;
							}
						}
						else
						{
							diffFromCenter = (float)(gapRect.Y - gapRect.Height / 2 - (this.rect.Y - this.rect.Height / 2)) / (float)this.rect.Height * this.BodyHeight;
							if (this.BodyWidth > 0f)
							{
								gapRect.X = gapRect.Center.X + (int)(-this.BodyWidth / 2f + this.BodyOffset.X * this.scale);
								gapRect.Width = (int)this.BodyWidth;
							}
							if (this.BodyHeight > 0f)
							{
								gapRect.Height = (int)(this.BodyHeight * ((float)gapRect.Height / (float)this.rect.Height));
							}
							if (base.FlippedY)
							{
								diffFromCenter = -diffFromCenter;
							}
						}
						if (Math.Abs(this.BodyRotation) > 0.01f)
						{
							Vector2 structureCenter = this.Position;
							Vector2 gapPos = structureCenter + new Vector2((float)Math.Cos((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation))), (float)Math.Sin((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation)))) * diffFromCenter + this.BodyOffset * this.scale;
							gapRect = new Rectangle((int)(gapPos.X - (float)(gapRect.Width / 2)), (int)(gapPos.Y + (float)(gapRect.Height / 2)), gapRect.Width, gapRect.Height);
						}
						gapRect.X -= 10;
						gapRect.Y += 10;
						gapRect.Width += 20;
						gapRect.Height += 20;
						bool rotatedEnoughToChangeOrientation = MathUtils.WrapAngleTwoPi(this.RotationRad - 0.7853982f) % 3.1415927f < 1.5707964f;
						if (rotatedEnoughToChangeOrientation)
						{
							Point center = gapRect.Location + gapRect.Size.FlipY() / new Point(2);
							Point topLeft = gapRect.Location;
							Point diff = topLeft - center;
							diff = diff.FlipY().YX().FlipY();
							Point newTopLeft = diff + center;
							gapRect = new Rectangle(newTopLeft, gapRect.Size.YX());
						}
						bool horizontalGap = rotatedEnoughToChangeOrientation ? this.IsHorizontal : (!this.IsHorizontal);
						bool diagonalGap = false;
						if (!MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
						{
							float sectorizedRotation = MathUtils.WrapAngleTwoPi(this.BodyRotation) % 1.5707964f;
							diagonalGap = (sectorizedRotation > 0.5235988f && sectorizedRotation < 1.0471976f);
							if (diagonalGap)
							{
								horizontalGap = ((float)(gapRect.Y - gapRect.Height / 2) < this.Position.Y);
								if (base.FlippedY)
								{
									horizontalGap = !horizontalGap;
								}
							}
						}
						this.Sections[sectionIndex].gap = new Gap(gapRect, horizontalGap, base.Submarine, diagonalGap, 0);
						this.Sections[sectionIndex].gap.FreeID();
						this.Sections[sectionIndex].gap.ShouldBeSaved = false;
						this.Sections[sectionIndex].gap.ConnectedWall = this;
						DebugConsole.Log(string.Concat(new string[]
						{
							"Created gap (ID ",
							this.Sections[sectionIndex].gap.ID.ToString(),
							", section: ",
							sectionIndex.ToString(),
							") on wall ",
							this.ID.ToString()
						}));
					}
					Gap gap = this.Sections[sectionIndex].gap;
					float damageRatio = (this.MaxHealth <= 0f) ? 0f : (damage / this.MaxHealth);
					float gapOpen = 0f;
					if (damageRatio > 0.7f)
					{
						gapOpen = MathHelper.Lerp(0.35f, 0.75f, MathUtils.InverseLerp(0.7f, 1f, damageRatio));
					}
					else if (damageRatio > 0.1f)
					{
						gapOpen = MathHelper.Lerp(0f, 0.35f, MathUtils.InverseLerp(0.1f, 0.7f, damageRatio));
					}
					gap.Open = gapOpen;
					if (gapOpen - prevGapOpenState > 0.25f && createExplosionEffect && !gap.IsRoomToRoom)
					{
						Structure.CreateWallDamageExplosion(gap, attacker, createWallDamageProjectiles);
						SteamTimelineManager.OnHullBreached(this);
					}
				}
			}
			float damageDiff = damage - this.Sections[sectionIndex].damage;
			bool hadHole = this.SectionBodyDisabled(sectionIndex);
			this.Sections[sectionIndex].damage = MathHelper.Clamp(damage, 0f, this.MaxHealth);
			this.HasDamage = this.Sections.Any((WallSection s) => s.damage > 0f);
			if (damageDiff != 0f)
			{
				Structure.OnHealthChangedHandler onHealthChanged = this.OnHealthChanged;
				if (onHealthChanged != null)
				{
					onHealthChanged(attacker, damageDiff);
				}
				if (attacker != null)
				{
					HumanAIController.StructureDamaged(this, damageDiff, attacker);
					if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && damageDiff < 0f)
					{
						CharacterInfo info = attacker.Info;
						if (info != null)
						{
							info.ApplySkillGain(Barotrauma.Tags.MechanicalSkill, -damageDiff * SkillSettings.Current.SkillIncreasePerRepairedStructureDamage, false, 2f, false);
						}
					}
				}
			}
			bool hasHole = this.SectionBodyDisabled(sectionIndex);
			if (hadHole == hasHole)
			{
				return;
			}
			this.UpdateSections();
		}

		// Token: 0x06002107 RID: 8455 RVA: 0x0014B4B4 File Offset: 0x001496B4
		private static void CreateWallDamageExplosion(Gap gap, Character attacker, bool createProjectiles)
		{
			float explosionStrength = gap.Open;
			Hull linkedHull = gap.linkedTo.FirstOrDefault<MapEntity>() as Hull;
			if (linkedHull != null)
			{
				foreach (Gap otherGap in linkedHull.ConnectedGaps)
				{
					if (otherGap != gap && !otherGap.IsRoomToRoom && otherGap.Open >= 0.25f)
					{
						explosionStrength -= Math.Max(0f, 500f - Vector2.Distance(otherGap.WorldPosition, gap.WorldPosition)) / 500f;
						if (explosionStrength <= 0f)
						{
							return;
						}
					}
				}
			}
			if (Structure.explosionOnBroken == null)
			{
				Structure.explosionOnBroken = new Explosion(500f, 5f, 0f, 0f, 0f, 0f, 0f);
				AfflictionPrefab lacerations;
				if (AfflictionPrefab.Prefabs.TryGet("lacerations".ToIdentifier(), out lacerations))
				{
					Structure.explosionOnBroken.Attack.Afflictions.Add(lacerations.Instantiate(5f, null), null);
				}
				else
				{
					Structure.explosionOnBroken.Attack.Afflictions.Add(AfflictionPrefab.InternalDamage.Instantiate(5f, null), null);
				}
				Structure.explosionOnBroken.CameraShake = 5f;
				Structure.explosionOnBroken.IgnoreCover = false;
				Structure.explosionOnBroken.OnlyInside = true;
				Structure.explosionOnBroken.DistanceFalloff = false;
				Structure.explosionOnBroken.PlayDamageSounds = true;
				Structure.explosionOnBroken.DisableParticles();
			}
			Structure.explosionOnBroken.CameraShake = 25f;
			Explosion explosion = Structure.explosionOnBroken;
			Structure connectedWall = gap.ConnectedWall;
			explosion.IgnoredCover = ((connectedWall != null) ? connectedWall.ToEnumerable<Structure>() : null);
			Structure.explosionOnBroken.Attack.Range = (Structure.explosionOnBroken.CameraShakeRange = 500f * gap.Open);
			Structure.explosionOnBroken.Attack.DamageMultiplier = explosionStrength;
			Structure.explosionOnBroken.Attack.Stun = MathHelper.Clamp(explosionStrength, 0.5f, 1f);
			Structure.explosionOnBroken.IgnoredCharacters.Clear();
			if (((attacker != null) ? attacker.AIController : null) is EnemyAIController)
			{
				Structure.explosionOnBroken.IgnoredCharacters.Add(attacker);
			}
			Explosion explosion2 = Structure.explosionOnBroken;
			if (explosion2 != null)
			{
				explosion2.Explode(gap.WorldPosition, null, attacker);
			}
			ItemPrefab projectilePrefab;
			if (createProjectiles && ItemPrefab.Prefabs.TryGet("walldamageprojectile", out projectilePrefab) && linkedHull != null)
			{
				float angle = gap.IsHorizontal ? ((linkedHull.WorldPosition.X < gap.WorldPosition.X) ? 3.1415927f : 0f) : ((linkedHull.WorldPosition.Y < gap.WorldPosition.Y) ? -1.5707964f : 1.5707964f);
				Entity.Spawner.AddItemToSpawnQueue(projectilePrefab, gap.WorldPosition, null, null, delegate(Item item)
				{
					item.body.SetTransformIgnoreContacts(item.body.SimPosition, angle, true);
					Projectile projectile = item.GetComponent<Projectile>();
					if (projectile != null)
					{
						projectile.Use(null, 0f);
					}
				});
			}
			SoundPlayer.PlaySound("Ricochet", gap.WorldPosition, null, null, null);
			if (linkedHull != null)
			{
				for (int i = 0; i <= 50; i++)
				{
					Vector2 emitDirection = gap.IsHorizontal ? ((gap.linkedTo[0].WorldPosition.X < gap.WorldPosition.X) ? (-Vector2.UnitX) : Vector2.UnitX) : ((gap.linkedTo[0].WorldPosition.Y < gap.WorldPosition.Y) ? (-Vector2.UnitY) : Vector2.UnitY);
					Vector2 particlePos = new Vector2((float)Rand.Range(gap.WorldRect.X, gap.WorldRect.Right, Rand.RandSync.Unsynced), (float)Rand.Range(gap.WorldRect.Y - gap.WorldRect.Height, gap.WorldRect.Y, Rand.RandSync.Unsynced));
					emitDirection = new Vector2(emitDirection.X + Rand.Range(-0.2f, 0.2f, Rand.RandSync.Unsynced), emitDirection.Y + Rand.Range(-0.2f, 0.2f, Rand.RandSync.Unsynced));
					Particle shrapnelParticle = GameMain.ParticleManager.CreateParticle("shrapnel", particlePos, emitDirection * Rand.Range(100f, 3000f, Rand.RandSync.Unsynced), 0f, linkedHull, 0.1f, null);
					Particle sparkParticle = GameMain.ParticleManager.CreateParticle("whitespark", particlePos, emitDirection * Rand.Range(1000f, 3000f, Rand.RandSync.Unsynced), 0f, linkedHull, 0.05f, null);
					if (shrapnelParticle == null || sparkParticle == null)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06002108 RID: 8456 RVA: 0x0014B980 File Offset: 0x00149B80
		public void SetCollisionCategory(Category collisionCategory)
		{
			if (this.Bodies == null)
			{
				return;
			}
			foreach (Body body in this.Bodies)
			{
				body.CollisionCategories = collisionCategory;
			}
		}

		// Token: 0x06002109 RID: 8457 RVA: 0x0014B9DC File Offset: 0x00149BDC
		private void UpdateSections()
		{
			if (this.Bodies == null)
			{
				return;
			}
			foreach (Body b in this.Bodies)
			{
				GameMain.World.Remove(b);
			}
			this.Bodies.Clear();
			this.bodyDimensions.Clear();
			List<ConvexHull> list = this.convexHulls;
			if (list != null)
			{
				list.ForEach(delegate(ConvexHull ch)
				{
					ch.Remove();
				});
			}
			List<ConvexHull> list2 = this.convexHulls;
			if (list2 != null)
			{
				list2.Clear();
			}
			bool hasHoles = false;
			List<WallSection> mergedSections = new List<WallSection>();
			for (int i = 0; i < this.Sections.Length; i++)
			{
				if (this.SectionBodyDisabled(i))
				{
					hasHoles = true;
					if (mergedSections.Any<WallSection>())
					{
						Rectangle mergedRect = this.GenerateMergedRect(mergedSections);
						mergedSections.Clear();
						this.CreateRectBody(mergedRect, true);
					}
				}
				else
				{
					mergedSections.Add(this.Sections[i]);
				}
			}
			if (mergedSections.Count > 0)
			{
				Rectangle mergedRect2 = this.GenerateMergedRect(mergedSections);
				this.CreateRectBody(mergedRect2, true);
			}
			if (hasHoles || !this.Bodies.Any<Body>())
			{
				Body sensorBody = this.CreateRectBody(this.rect, false);
				sensorBody.CollisionCategories = Category.Cat9;
			}
			foreach (WallSection section in this.Sections)
			{
				bool intersectsWithBody = false;
				foreach (Body body in this.Bodies)
				{
					Rectangle bodyRect = new Rectangle(ConvertUnits.ToDisplayUnits(body.Position - this.bodyDimensions[body] / 2f).ToPoint(), ConvertUnits.ToDisplayUnits(this.bodyDimensions[body]).ToPoint());
					Rectangle sectionRect = section.rect;
					sectionRect.Y -= section.rect.Height;
					if (bodyRect.Intersects(sectionRect))
					{
						intersectsWithBody = true;
						break;
					}
				}
				section.NoPhysicsBody = !intersectsWithBody;
			}
		}

		// Token: 0x0600210A RID: 8458 RVA: 0x0014BC34 File Offset: 0x00149E34
		private Body CreateRectBody(Rectangle rect, bool createConvexHull)
		{
			float diffFromCenter;
			if (this.IsHorizontal)
			{
				diffFromCenter = (float)(rect.Center.X - this.rect.Center.X) / (float)this.rect.Width * this.BodyWidth;
				if (this.BodyWidth > 0f)
				{
					rect.Width = Math.Max((int)Math.Round((double)(this.BodyWidth * ((float)rect.Width / (float)this.rect.Width))), 1);
				}
				if (this.BodyHeight > 0f)
				{
					rect.Height = (int)this.BodyHeight;
				}
				if (base.FlippedX)
				{
					diffFromCenter = -diffFromCenter;
				}
			}
			else
			{
				diffFromCenter = (float)(rect.Y - rect.Height / 2 - (this.rect.Y - this.rect.Height / 2)) / (float)this.rect.Height * this.BodyHeight;
				if (this.BodyWidth > 0f)
				{
					rect.Width = (int)this.BodyWidth;
				}
				if (this.BodyHeight > 0f)
				{
					rect.Height = Math.Max((int)Math.Round((double)(this.BodyHeight * ((float)rect.Height / (float)this.rect.Height))), 1);
				}
				if (base.FlippedY)
				{
					diffFromCenter = -diffFromCenter;
				}
			}
			Vector2 bodyOffset = ConvertUnits.ToSimUnits(this.BodyOffset) * this.scale;
			Body newBody = GameMain.World.CreateRectangle(ConvertUnits.ToSimUnits(rect.Width), ConvertUnits.ToSimUnits(rect.Height), 1.5f, default(Vector2), 0f, BodyType.Static, Category.Cat1, Category.All, false);
			newBody.Friction = 0.5f;
			newBody.OnCollision += this.OnWallCollision;
			newBody.CollisionCategories = (this.Prefab.Platform ? Category.Cat3 : Category.Cat1);
			newBody.UserData = this;
			Vector2 structureCenter = ConvertUnits.ToSimUnits(this.Position);
			if (!MathUtils.NearlyEqual(this.BodyRotation, 0f, 0.0001f))
			{
				Vector2 pos = structureCenter + bodyOffset + new Vector2((float)Math.Cos((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation))), (float)Math.Sin((double)(this.IsHorizontal ? (-(double)this.BodyRotation) : (1.5707964f - this.BodyRotation)))) * ConvertUnits.ToSimUnits(diffFromCenter);
				newBody.SetTransformIgnoreContacts(ref pos, -this.BodyRotation);
			}
			else
			{
				Vector2 pos2 = structureCenter + (this.IsHorizontal ? Vector2.UnitX : Vector2.UnitY) * ConvertUnits.ToSimUnits(diffFromCenter) + bodyOffset;
				newBody.SetTransformIgnoreContacts(ref pos2, newBody.Rotation);
			}
			if (createConvexHull)
			{
				this.CreateConvexHull(ConvertUnits.ToDisplayUnits(newBody.Position), rect.Size.ToVector2(), newBody.Rotation);
			}
			this.Bodies.Add(newBody);
			this.bodyDimensions.Add(newBody, new Vector2(ConvertUnits.ToSimUnits(rect.Width), ConvertUnits.ToSimUnits(rect.Height)));
			return newBody;
		}

		// Token: 0x0600210B RID: 8459 RVA: 0x0014BF4C File Offset: 0x0014A14C
		private void CreateConvexHull(Vector2 position, Vector2 size, float rotation)
		{
			if (!this.CastShadow)
			{
				return;
			}
			if (this.convexHulls == null)
			{
				this.convexHulls = new List<ConvexHull>();
			}
			float length = this.IsHorizontal ? size.X : size.Y;
			int convexHullCount = (int)Math.Max(1.0, Math.Ceiling((double)(length / 1024f)));
			Vector2 sectionSize = size;
			if (convexHullCount > 1)
			{
				if (this.IsHorizontal)
				{
					sectionSize.X = length / (float)convexHullCount;
				}
				else
				{
					sectionSize.Y = length / (float)convexHullCount;
				}
			}
			for (int i = 0; i < convexHullCount; i++)
			{
				Vector2 offset = (this.IsHorizontal ? Vector2.UnitX : Vector2.UnitY) * ((float)i * length / (float)convexHullCount);
				ConvexHull h = new ConvexHull(new Rectangle((position - size / 2f + offset).ToPoint(), sectionSize.ToPoint()), this.IsHorizontal, this);
				if (Math.Abs(rotation) > 0.001f)
				{
					h.Rotate(position, rotation);
				}
				this.convexHulls.Add(h);
			}
		}

		// Token: 0x0600210C RID: 8460 RVA: 0x0014C064 File Offset: 0x0014A264
		public override void FlipX(bool relativeToSub, bool force = false)
		{
			base.FlipX(relativeToSub, false);
			if (this.Prefab.CanSpriteFlipX)
			{
				this.SpriteEffects ^= SpriteEffects.FlipHorizontally;
			}
			if (this.StairDirection != Direction.None)
			{
				this.StairDirection = ((this.StairDirection == Direction.Left) ? Direction.Right : Direction.Left);
				this.Bodies.ForEach(delegate(Body b)
				{
					GameMain.World.Remove(b);
				});
				this.Bodies.Clear();
				this.bodyDimensions.Clear();
				this.CreateStairBodies();
			}
			if (this.Prefab.Body)
			{
				this.CreateSections();
				this.UpdateSections();
			}
		}

		// Token: 0x0600210D RID: 8461 RVA: 0x0014C110 File Offset: 0x0014A310
		public override void FlipY(bool relativeToSub, bool force = false)
		{
			base.FlipY(relativeToSub, false);
			if (this.Prefab.CanSpriteFlipY)
			{
				this.SpriteEffects ^= SpriteEffects.FlipVertically;
			}
			if (this.StairDirection != Direction.None)
			{
				this.StairDirection = ((this.StairDirection == Direction.Left) ? Direction.Right : Direction.Left);
				this.Bodies.ForEach(delegate(Body b)
				{
					GameMain.World.Remove(b);
				});
				this.Bodies.Clear();
				this.bodyDimensions.Clear();
				this.CreateStairBodies();
			}
			if (this.Prefab.Body)
			{
				this.CreateSections();
				this.UpdateSections();
			}
		}

		// Token: 0x0600210E RID: 8462 RVA: 0x0014C1BC File Offset: 0x0014A3BC
		public static Structure Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			string name = element.GetAttribute("name").Value;
			Identifier identifier = element.GetAttributeIdentifier("identifier", "");
			StructurePrefab prefab = Structure.FindPrefab(name, identifier);
			if (prefab == null)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Error loading structure - structure prefab \"",
					name,
					"\" (identifier \"",
					identifier.ToString(),
					"\") not found."
				}), null, null, false, false);
				return null;
			}
			string key = "rect";
			Rectangle empty = Rectangle.Empty;
			Rectangle rect = element.GetAttributeRect(key, empty);
			Structure s = new Structure(rect, prefab, submarine, idRemap.GetOffsetId(element), element)
			{
				Submarine = submarine
			};
			bool flippedX = element.GetAttributeBool("FlippedX", false);
			bool flippedY = element.GetAttributeBool("FlippedY", false);
			if (((submarine != null) ? submarine.Info.GameVersion : null) != null)
			{
				SerializableProperty.UpgradeGameVersion(s, s.Prefab.ConfigElement, submarine.Info.GameVersion);
				if (submarine.Info.GameVersion < new Version(0, 19, 10))
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.LevelData : null) != null)
					{
						s.CrushDepth = Math.Max(s.CrushDepth, (float)GameMain.GameSession.LevelData.InitialDepth * Physics.DisplayToRealWorldRatio + 500f);
					}
				}
				Structure structure = s;
				Vector2 targetSize = rect.Size.ToVector2();
				string key2 = "TextureOffset";
				Vector2 zero = Vector2.Zero;
				structure.TextureOffset = Structure.UpgradeTextureOffset(targetSize, element.GetAttributeVector2(key2, zero), submarine.Info, s.Sprite.SourceRect, s.Scale * s.TextureScale, flippedX, flippedY);
			}
			bool hasDamage = false;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "section"))
				{
					if (a == "upgrade")
					{
						Identifier upgradeIdentifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						UpgradePrefab upgradePrefab = UpgradePrefab.Find(upgradeIdentifier);
						int level = subElement.GetAttributeInt("level", 1);
						if (upgradePrefab != null)
						{
							s.AddUpgrade(new Upgrade(s, upgradePrefab, level, subElement), false);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
							defaultInterpolatedStringHandler.AppendLiteral("An upgrade with identifier \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(upgradeIdentifier);
							defaultInterpolatedStringHandler.AppendLiteral("\" on ");
							defaultInterpolatedStringHandler.AppendFormatted(s.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" was not found. ");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + "It's effect will not be applied and won't be saved after the round ends.", null, null, false, false);
						}
					}
				}
				else
				{
					int index = subElement.GetAttributeInt("i", -1);
					if (index != -1)
					{
						if (index < 0 || index >= s.SectionCount)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(95, 3);
							defaultInterpolatedStringHandler2.AppendLiteral("Error while loading structure \"");
							defaultInterpolatedStringHandler2.AppendFormatted(s.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("\". Section damage index out of bounds. Index: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(index);
							defaultInterpolatedStringHandler2.AppendLiteral(", section count: ");
							defaultInterpolatedStringHandler2.AppendFormatted<int>(s.SectionCount);
							defaultInterpolatedStringHandler2.AppendLiteral(".");
							string errorMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
							DebugConsole.ThrowError(errorMsg, null, null, false, false);
							GameAnalyticsManager.AddErrorEventOnce("Structure.Load:SectionIndexOutOfBounds", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
						}
						else
						{
							float damage = subElement.GetAttributeFloat("damage", 0f);
							s.Sections[index].damage = damage;
							hasDamage |= (damage > 0f);
						}
					}
				}
			}
			if (flippedX)
			{
				s.FlipX(false, false);
			}
			if (flippedY)
			{
				s.FlipY(false, false);
			}
			if (element.GetAttribute("UseDropShadow") == null)
			{
				s.UseDropShadow = s.HasBody;
			}
			if (element.GetAttribute("NoAITarget") == null)
			{
				s.NoAITarget = prefab.NoAITarget;
			}
			if (hasDamage)
			{
				s.UpdateSections();
			}
			return s;
		}

		// Token: 0x0600210F RID: 8463 RVA: 0x0014C5F8 File Offset: 0x0014A7F8
		public static StructurePrefab FindPrefab(string name, Identifier identifier)
		{
			StructurePrefab prefab = null;
			StructurePrefab structurePrefab;
			if (identifier.IsEmpty)
			{
				prefab = (MapEntityPrefab.Find(name, "", true) as StructurePrefab);
				if (prefab == null)
				{
					prefab = (MapEntityPrefab.Find(name, null, true) as StructurePrefab);
				}
				if (prefab == null)
				{
					prefab = (MapEntityPrefab.Find(null, name, true) as StructurePrefab);
				}
			}
			else if (StructurePrefab.Prefabs.TryGet(identifier, out structurePrefab))
			{
				prefab = structurePrefab;
			}
			return prefab;
		}

		// Token: 0x06002110 RID: 8464 RVA: 0x0014C658 File Offset: 0x0014A858
		public override XElement Save(XElement parentElement)
		{
			XElement element = new XElement("Structure");
			int width = base.ResizeHorizontal ? this.rect.Width : this.defaultRect.Width;
			int height = base.ResizeVertical ? this.rect.Height : this.defaultRect.Height;
			element.Add(new object[]
			{
				new XAttribute("name", this.Prefab.Name),
				new XAttribute("identifier", this.Prefab.Identifier),
				new XAttribute("ID", this.ID),
				new XAttribute("rect", string.Concat(new string[]
				{
					((int)((float)this.rect.X - base.Submarine.HiddenSubPosition.X)).ToString(),
					",",
					((int)((float)this.rect.Y - base.Submarine.HiddenSubPosition.Y)).ToString(),
					",",
					width.ToString(),
					",",
					height.ToString()
				}))
			});
			if (base.FlippedX)
			{
				element.Add(new XAttribute("flippedx", true));
			}
			if (base.FlippedY)
			{
				element.Add(new XAttribute("flippedy", true));
			}
			for (int i = 0; i < this.Sections.Length; i++)
			{
				if (this.Sections[i].damage != 0f)
				{
					XElement sectionElement = new XElement("section", new object[]
					{
						new XAttribute("i", i),
						new XAttribute("damage", this.Sections[i].damage)
					});
					element.Add(sectionElement);
				}
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
			if (this.CastShadow == this.Prefab.CastShadow)
			{
				XAttribute attribute = element.GetAttribute("CastShadow", StringComparison.OrdinalIgnoreCase);
				if (attribute != null)
				{
					attribute.Remove();
				}
			}
			foreach (Upgrade upgrade in this.Upgrades)
			{
				upgrade.Save(element);
			}
			parentElement.Add(element);
			return element;
		}

		// Token: 0x06002111 RID: 8465 RVA: 0x0014C910 File Offset: 0x0014AB10
		public override void OnMapLoaded()
		{
			for (int i = 0; i < this.Sections.Length; i++)
			{
				this.SetDamage(i, this.Sections[i].damage, null, false, true, false, false);
			}
		}

		// Token: 0x06002112 RID: 8466 RVA: 0x0014C94C File Offset: 0x0014AB4C
		public virtual void Reset()
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, this.Prefab.ConfigElement);
			this.MaxHealth = this.Prefab.Health;
			this.Sprite.ReloadXML();
			base.SpriteDepth = this.Sprite.Depth;
			this.NoAITarget = this.Prefab.NoAITarget;
		}

		// Token: 0x06002113 RID: 8467 RVA: 0x0014C9B4 File Offset: 0x0014ABB4
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.aiTarget != null)
			{
				this.aiTarget.SightRange = ((base.Submarine == null) ? this.aiTarget.MinSightRange : MathHelper.Lerp(this.aiTarget.MinSightRange, this.aiTarget.MaxSightRange, base.Submarine.Velocity.Length() / 10f));
			}
		}

		// Token: 0x0600211B RID: 8475 RVA: 0x0014CBFC File Offset: 0x0014ADFC
		[CompilerGenerated]
		internal static float <Draw>g__GetRotationForSprite|16_0(float rotationRad, Sprite sprite)
		{
			bool flipHorizontally = (sprite.effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
			bool flipVertically = (sprite.effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			if (flipHorizontally != flipVertically)
			{
				rotationRad = -rotationRad;
			}
			return rotationRad;
		}

		// Token: 0x040010A4 RID: 4260
		public static bool ShowWalls = true;

		// Token: 0x040010A5 RID: 4261
		public static bool ShowStructures = true;

		// Token: 0x040010A6 RID: 4262
		private List<ConvexHull> convexHulls;

		// Token: 0x040010A7 RID: 4263
		private readonly Dictionary<DecorativeSprite, DecorativeSprite.State> spriteAnimState = new Dictionary<DecorativeSprite, DecorativeSprite.State>();

		// Token: 0x040010A8 RID: 4264
		public readonly List<LightSource> Lights = new List<LightSource>();

		// Token: 0x040010A9 RID: 4265
		public const int WallSectionSize = 96;

		// Token: 0x040010AA RID: 4266
		public static List<Structure> WallList = new List<Structure>();

		// Token: 0x040010AB RID: 4267
		private const float LeakThreshold = 0.1f;

		// Token: 0x040010AC RID: 4268
		private const float BigGapThreshold = 0.7f;

		// Token: 0x040010AD RID: 4269
		public const float SmallGapOpenness = 0.35f;

		// Token: 0x040010AE RID: 4270
		public const float LargeGapOpenness = 0.75f;

		// Token: 0x040010AF RID: 4271
		public SpriteEffects SpriteEffects;

		// Token: 0x040010B0 RID: 4272
		private readonly Dictionary<Body, Vector2> bodyDimensions = new Dictionary<Body, Vector2>();

		// Token: 0x040010B1 RID: 4273
		private static Explosion explosionOnBroken;

		// Token: 0x040010B2 RID: 4274
		public Structure.OnHealthChangedHandler OnHealthChanged;

		// Token: 0x040010BA RID: 4282
		private float? maxHealth;

		// Token: 0x040010BB RID: 4283
		private float crushDepth;

		// Token: 0x040010BE RID: 4286
		protected Color spriteColor;

		// Token: 0x040010C1 RID: 4289
		private float scale = 1f;

		// Token: 0x040010C2 RID: 4290
		protected Vector2 textureScale = Vector2.One;

		// Token: 0x040010C4 RID: 4292
		protected Vector2 textureOffset = Vector2.Zero;

		// Token: 0x040010C5 RID: 4293
		private Rectangle defaultRect;

		// Token: 0x02000B8C RID: 2956
		// (Invoke) Token: 0x060078FC RID: 30972
		public delegate void OnHealthChangedHandler(Character attacker, float damage);
	}
}
