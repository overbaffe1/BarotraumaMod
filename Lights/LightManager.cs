using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Lights
{
	// Token: 0x020004D3 RID: 1235
	internal class LightManager
	{
		// Token: 0x1700146E RID: 5230
		// (get) Token: 0x0600503D RID: 20541 RVA: 0x002B28E9 File Offset: 0x002B0AE9
		// (set) Token: 0x0600503E RID: 20542 RVA: 0x002B28F0 File Offset: 0x002B0AF0
		public static Entity ViewTarget { get; set; }

		// Token: 0x1700146F RID: 5231
		// (get) Token: 0x0600503F RID: 20543 RVA: 0x002B28F8 File Offset: 0x002B0AF8
		// (set) Token: 0x06005040 RID: 20544 RVA: 0x002B2900 File Offset: 0x002B0B00
		public RenderTarget2D LightMap { get; private set; }

		// Token: 0x17001470 RID: 5232
		// (get) Token: 0x06005041 RID: 20545 RVA: 0x002B2909 File Offset: 0x002B0B09
		// (set) Token: 0x06005042 RID: 20546 RVA: 0x002B2911 File Offset: 0x002B0B11
		public RenderTarget2D LimbLightMap { get; private set; }

		// Token: 0x17001471 RID: 5233
		// (get) Token: 0x06005043 RID: 20547 RVA: 0x002B291A File Offset: 0x002B0B1A
		// (set) Token: 0x06005044 RID: 20548 RVA: 0x002B2922 File Offset: 0x002B0B22
		public RenderTarget2D LosTexture { get; private set; }

		// Token: 0x17001472 RID: 5234
		// (get) Token: 0x06005045 RID: 20549 RVA: 0x002B292B File Offset: 0x002B0B2B
		// (set) Token: 0x06005046 RID: 20550 RVA: 0x002B2933 File Offset: 0x002B0B33
		public RenderTarget2D HighlightMap { get; private set; }

		// Token: 0x17001473 RID: 5235
		// (get) Token: 0x06005047 RID: 20551 RVA: 0x002B293C File Offset: 0x002B0B3C
		// (set) Token: 0x06005048 RID: 20552 RVA: 0x002B2944 File Offset: 0x002B0B44
		public Effect LosEffect { get; private set; }

		// Token: 0x17001474 RID: 5236
		// (get) Token: 0x06005049 RID: 20553 RVA: 0x002B294D File Offset: 0x002B0B4D
		// (set) Token: 0x0600504A RID: 20554 RVA: 0x002B2955 File Offset: 0x002B0B55
		public Effect SolidColorEffect { get; private set; }

		// Token: 0x17001475 RID: 5237
		// (get) Token: 0x0600504B RID: 20555 RVA: 0x002B295E File Offset: 0x002B0B5E
		public IEnumerable<LightSource> Lights
		{
			get
			{
				return this.lights;
			}
		}

		// Token: 0x0600504C RID: 20556 RVA: 0x002B2968 File Offset: 0x002B0B68
		public LightManager(GraphicsDevice graphics)
		{
			LightManager <>4__this = this;
			this.lights = new List<LightSource>(100);
			this.AmbientLight = new Color(20, 20, 20, 255);
			this.rayCastThread = new Thread(new ThreadStart(this.UpdateRayCasts))
			{
				Name = "LightManager Raycast thread",
				IsBackground = true
			};
			this.rayCastThread.Start();
			this.visionCircle = Sprite.LoadTexture("Content/Lights/visioncircle.png", true, null);
			this.highlightRaster = Sprite.LoadTexture("Content/UI/HighlightRaster.png", true, null);
			this.gapGlowTexture = Sprite.LoadTexture("Content/Lights/pointlight_rays.png", true, null);
			GameMain.Instance.ResolutionChanged += delegate()
			{
				<>4__this.CreateRenderTargets(graphics);
			};
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				<>4__this.CreateRenderTargets(graphics);
				<>4__this.LosEffect = EffectLoader.Load("Effects/losshader");
				<>4__this.SolidColorEffect = EffectLoader.Load("Effects/solidcolor");
				if (<>4__this.lightEffect == null)
				{
					<>4__this.lightEffect = new BasicEffect(GameMain.Instance.GraphicsDevice)
					{
						VertexColorEnabled = true,
						TextureEnabled = true,
						Texture = LightSource.LightTexture
					};
				}
			});
		}

		// Token: 0x0600504D RID: 20557 RVA: 0x002B2AA0 File Offset: 0x002B0CA0
		private void CreateRenderTargets(GraphicsDevice graphics)
		{
			LightManager.<>c__DisplayClass51_0 CS$<>8__locals1;
			CS$<>8__locals1.graphics = graphics;
			CS$<>8__locals1.pp = CS$<>8__locals1.graphics.PresentationParameters;
			this.currLightMapScale = GameSettings.CurrentConfig.Graphics.LightMapScale;
			RenderTarget2D lightMap = this.LightMap;
			if (lightMap != null)
			{
				lightMap.Dispose();
			}
			this.LightMap = LightManager.<CreateRenderTargets>g__CreateRenderTarget|51_0(ref CS$<>8__locals1);
			RenderTarget2D limbLightMap = this.LimbLightMap;
			if (limbLightMap != null)
			{
				limbLightMap.Dispose();
			}
			this.LimbLightMap = LightManager.<CreateRenderTargets>g__CreateRenderTarget|51_0(ref CS$<>8__locals1);
			RenderTarget2D highlightMap = this.HighlightMap;
			if (highlightMap != null)
			{
				highlightMap.Dispose();
			}
			this.HighlightMap = LightManager.<CreateRenderTargets>g__CreateRenderTarget|51_0(ref CS$<>8__locals1);
			RenderTarget2D losTexture = this.LosTexture;
			if (losTexture != null)
			{
				losTexture.Dispose();
			}
			this.LosTexture = new RenderTarget2D(CS$<>8__locals1.graphics, (int)((float)GameMain.GraphicsWidth * GameSettings.CurrentConfig.Graphics.LightMapScale), (int)((float)GameMain.GraphicsHeight * GameSettings.CurrentConfig.Graphics.LightMapScale), false, SurfaceFormat.Color, DepthFormat.None);
		}

		// Token: 0x0600504E RID: 20558 RVA: 0x002B2B89 File Offset: 0x002B0D89
		public void AddLight(LightSource light)
		{
			if (!this.lights.Contains(light))
			{
				this.lights.Add(light);
			}
		}

		// Token: 0x0600504F RID: 20559 RVA: 0x002B2BA5 File Offset: 0x002B0DA5
		public void RemoveLight(LightSource light)
		{
			this.lights.Remove(light);
		}

		// Token: 0x06005050 RID: 20560 RVA: 0x002B2BB4 File Offset: 0x002B0DB4
		public void OnMapLoaded()
		{
			foreach (LightSource light in this.lights)
			{
				light.HullsUpToDate.Clear();
				light.NeedsRecalculation = true;
			}
		}

		// Token: 0x17001476 RID: 5238
		// (get) Token: 0x06005051 RID: 20561 RVA: 0x002B2C14 File Offset: 0x002B0E14
		// (set) Token: 0x06005052 RID: 20562 RVA: 0x002B2C1B File Offset: 0x002B0E1B
		public static int ActiveLightCount { get; private set; }

		// Token: 0x06005053 RID: 20563 RVA: 0x002B2C24 File Offset: 0x002B0E24
		public void Update(float deltaTime)
		{
			this.time = (this.time + deltaTime) % 100000f;
			foreach (LightSource light in this.activeLights)
			{
				if (light.Enabled)
				{
					light.Update(this.time);
				}
			}
		}

		// Token: 0x06005054 RID: 20564 RVA: 0x002B2C98 File Offset: 0x002B0E98
		public void AddRayCastTask(LightSource lightSource, Vector2 drawPos, float rotation)
		{
			object obj = LightManager.mutex;
			lock (obj)
			{
				if (!this.pendingRayCasts.Any((LightManager.RayCastTask p) => p.LightSource == lightSource))
				{
					this.pendingRayCasts.Enqueue(new LightManager.RayCastTask(lightSource, drawPos, rotation));
				}
			}
		}

		// Token: 0x06005055 RID: 20565 RVA: 0x002B2D14 File Offset: 0x002B0F14
		private void UpdateRayCasts()
		{
			for (;;)
			{
				object obj = LightManager.mutex;
				lock (obj)
				{
					while (this.pendingRayCasts.Count > 0)
					{
						this.pendingRayCasts.Dequeue().Calculate();
					}
				}
				Thread.Sleep(10);
			}
		}

		// Token: 0x06005056 RID: 20566 RVA: 0x002B2D78 File Offset: 0x002B0F78
		public void DebugDrawVertices(SpriteBatch spriteBatch)
		{
			foreach (LightSource light in this.lights)
			{
				if (light.Enabled)
				{
					light.DebugDrawVertices(spriteBatch);
				}
			}
		}

		// Token: 0x06005057 RID: 20567 RVA: 0x002B2DD4 File Offset: 0x002B0FD4
		public void RenderLightMap(GraphicsDevice graphics, SpriteBatch spriteBatch, Camera cam, RenderTarget2D backgroundObstructor = null)
		{
			LightManager.<>c__DisplayClass67_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			if (!this.LightingEnabled)
			{
				return;
			}
			if (Math.Abs(this.currLightMapScale - GameSettings.CurrentConfig.Graphics.LightMapScale) > 0.01f)
			{
				this.CreateRenderTargets(graphics);
			}
			Matrix spriteBatchTransform = cam.Transform * Matrix.CreateScale(new Vector3(GameSettings.CurrentConfig.Graphics.LightMapScale, GameSettings.CurrentConfig.Graphics.LightMapScale, 1f));
			Matrix transform = cam.ShaderTransform * Matrix.CreateOrthographic((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight, -1f, 1f) * 0.5f;
			bool highlightsVisible = this.UpdateHighlights(graphics, CS$<>8__locals1.spriteBatch, spriteBatchTransform, cam);
			Rectangle viewRect = cam.WorldView;
			viewRect.Y -= cam.WorldView.Height;
			this.recalculationCount = 0;
			this.activeLights.Clear();
			foreach (LightSource light in this.lights)
			{
				if (light.Enabled && ((light.Color.A >= 1 && light.Range >= 1f) || light.LightSourceParams.OverrideLightSpriteAlpha != null))
				{
					if (light.ParentBody != null)
					{
						light.ParentBody.UpdateDrawPosition(true);
						Vector2 pos = light.ParentBody.DrawPosition + light.OffsetFromBody;
						if (light.ParentSub != null)
						{
							pos -= light.ParentSub.DrawPosition;
						}
						light.Position = pos;
					}
					if (!Level.IsPositionAboveLevel(light.WorldPosition))
					{
						float range = light.LightSourceParams.TextureRange;
						if (light.LightSprite != null)
						{
							float spriteRange = Math.Max(light.LightSprite.size.X * light.SpriteScale.X * (0.5f + Math.Abs(light.LightSprite.RelativeOrigin.X - 0.5f)), light.LightSprite.size.Y * light.SpriteScale.Y * (0.5f + Math.Abs(light.LightSprite.RelativeOrigin.Y - 0.5f)));
							float targetSize = Math.Max(light.LightTextureTargetSize.X, light.LightTextureTargetSize.Y);
							range = Math.Max(Math.Max(spriteRange, targetSize), range);
						}
						if (MathUtils.CircleIntersectsRectangle(light.WorldPosition, range, viewRect))
						{
							light.Priority = LightManager.<RenderLightMap>g__lightPriority|67_1(range, light);
							this.activeLights.Add(light);
						}
					}
				}
			}
			this.activeLights.Sort((LightSource a, LightSource b) => b.Priority.CompareTo(a.Priority));
			LightManager.ActiveLightCount = this.activeLights.Count;
			this.activeShadowCastingLights.Clear();
			foreach (LightSource activeLight in this.activeLights)
			{
				if (activeLight.CastShadows && activeLight.Range >= 1f && activeLight.Color.A >= 1 && activeLight.CurrentBrightness > 0f)
				{
					this.activeShadowCastingLights.Add(activeLight);
				}
			}
			if (this.activeShadowCastingLights.Count > GameSettings.CurrentConfig.Graphics.VisibleLightLimit)
			{
				Screen selected = Screen.Selected;
				if (selected != null && !selected.IsEditor)
				{
					for (int i = GameSettings.CurrentConfig.Graphics.VisibleLightLimit; i < this.activeShadowCastingLights.Count; i++)
					{
						this.activeLights.Remove(this.activeShadowCastingLights[i]);
					}
				}
			}
			this.activeLights.Sort((LightSource l1, LightSource l2) => l1.LastRecalculationTime.CompareTo(l2.LastRecalculationTime));
			graphics.SetRenderTarget(this.LimbLightMap);
			graphics.Clear(Color.Black);
			graphics.BlendState = BlendState.NonPremultiplied;
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(spriteBatchTransform));
			foreach (LightSource light2 in this.activeLights)
			{
				if (!light2.IsBackground && light2.CurrentBrightness > 0f)
				{
					PhysicsBody parentBody = light2.ParentBody;
					Limb limb = ((parentBody != null) ? parentBody.UserData : null) as Limb;
					if (limb != null && !limb.Hide)
					{
						light2.DrawSprite(CS$<>8__locals1.spriteBatch, cam);
					}
				}
			}
			CS$<>8__locals1.spriteBatch.End();
			graphics.SetRenderTarget(this.LightMap);
			graphics.Clear(this.AmbientLight);
			graphics.BlendState = BlendState.Additive;
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, new Matrix?(spriteBatchTransform));
			Level loaded = Level.Loaded;
			if (loaded != null)
			{
				BackgroundCreatureManager backgroundCreatureManager = loaded.BackgroundCreatureManager;
				if (backgroundCreatureManager != null)
				{
					backgroundCreatureManager.DrawLights(CS$<>8__locals1.spriteBatch, cam);
				}
			}
			foreach (LightSource light3 in this.activeLights)
			{
				if (light3.IsBackground && light3.CurrentBrightness > 0f)
				{
					light3.DrawLightVolume(CS$<>8__locals1.spriteBatch, this.lightEffect, transform, this.recalculationCount < 5, ref this.recalculationCount);
					light3.DrawSprite(CS$<>8__locals1.spriteBatch, cam);
				}
			}
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, true, null, ParticleBlendState.Additive, new bool?(false));
			CS$<>8__locals1.spriteBatch.End();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, null, new Matrix?(spriteBatchTransform));
			Dictionary<Hull, Rectangle> visibleHulls = this.GetVisibleHulls(cam);
			foreach (KeyValuePair<Hull, Rectangle> hull in visibleHulls)
			{
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Vector2((float)hull.Value.X, (float)(-(float)hull.Value.Y)), new Vector2((float)hull.Value.Width, (float)hull.Value.Height), (hull.Key.AmbientLight == Color.TransparentBlack) ? Color.Black : hull.Key.AmbientLight.Multiply((float)hull.Key.AmbientLight.A / 255f, false), true, 0f, 1f);
			}
			CS$<>8__locals1.spriteBatch.End();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, new Matrix?(spriteBatchTransform));
			Vector3 glowColorHSV = ToolBox.RGBToHSV(this.AmbientLight);
			glowColorHSV.Z = Math.Max(glowColorHSV.Z, 0.4f);
			Color glowColor = ToolBoxCore.HSVToRGB(glowColorHSV.X, glowColorHSV.Y, glowColorHSV.Z);
			Vector2 glowSpriteSize = new Vector2((float)this.gapGlowTexture.Width, (float)this.gapGlowTexture.Height);
			foreach (Gap gap in Gap.GapList)
			{
				if (!gap.IsRoomToRoom && gap.Open > 0f && gap.ConnectedWall != null)
				{
					float a2 = MathHelper.Lerp(0.5f, 1f, PerlinNoise.GetPerlin((float)Timing.TotalTime * 0.05f, gap.GlowEffectT));
					float scale = MathHelper.Lerp(0.5f, 2f, PerlinNoise.GetPerlin((float)Timing.TotalTime * 0.01f, gap.GlowEffectT));
					float rot = PerlinNoise.GetPerlin((float)Timing.TotalTime * 0.001f, gap.GlowEffectT) * 6.2831855f;
					Vector2 spriteScale = new Vector2((float)gap.Rect.Width, (float)gap.Rect.Height) / glowSpriteSize;
					Vector2 drawPos = new Vector2(gap.DrawPosition.X, -gap.DrawPosition.Y);
					CS$<>8__locals1.spriteBatch.Draw(this.gapGlowTexture, drawPos, null, glowColor * a2, rot, glowSpriteSize / 2f, Math.Max(spriteScale.X, spriteScale.Y) * scale, SpriteEffects.None, 0f);
				}
			}
			CS$<>8__locals1.spriteBatch.End();
			if (backgroundObstructor != null)
			{
				SpriteBatch spriteBatch2 = CS$<>8__locals1.spriteBatch;
				SpriteSortMode sortMode = SpriteSortMode.Immediate;
				BlendState nonPremultiplied = BlendState.NonPremultiplied;
				SamplerState linearWrap = SamplerState.LinearWrap;
				DepthStencilState depthStencilState = null;
				RasterizerState rasterizerState = null;
				Matrix? transformMatrix = new Matrix?(Matrix.Identity);
				spriteBatch2.Begin(sortMode, nonPremultiplied, linearWrap, depthStencilState, rasterizerState, GameMain.GameScreen.DamageEffect, transformMatrix);
				CS$<>8__locals1.spriteBatch.Draw(backgroundObstructor, new Rectangle(0, 0, (int)((float)GameMain.GraphicsWidth * this.currLightMapScale), (int)((float)GameMain.GraphicsHeight * this.currLightMapScale)), Color.Black);
				CS$<>8__locals1.spriteBatch.End();
			}
			else
			{
				GameMain.GameScreen.DamageEffect.CurrentTechnique = GameMain.GameScreen.DamageEffect.Techniques["StencilShaderSolidColor"];
				GameMain.GameScreen.DamageEffect.Parameters["solidColor"].SetValue(Color.Black.ToVector4());
				SpriteBatch spriteBatch3 = CS$<>8__locals1.spriteBatch;
				SpriteSortMode sortMode2 = SpriteSortMode.Immediate;
				BlendState nonPremultiplied2 = BlendState.NonPremultiplied;
				SamplerState linearWrap2 = SamplerState.LinearWrap;
				DepthStencilState depthStencilState2 = null;
				RasterizerState rasterizerState2 = null;
				Matrix? transformMatrix = new Matrix?(spriteBatchTransform);
				spriteBatch3.Begin(sortMode2, nonPremultiplied2, linearWrap2, depthStencilState2, rasterizerState2, GameMain.GameScreen.DamageEffect, transformMatrix);
				Submarine.DrawDamageable(CS$<>8__locals1.spriteBatch, GameMain.GameScreen.DamageEffect, false, null);
				CS$<>8__locals1.spriteBatch.End();
			}
			graphics.BlendState = BlendState.Additive;
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, new Matrix?(spriteBatchTransform));
			foreach (LightSource light4 in this.activeLights)
			{
				if (!light4.IsBackground)
				{
					PhysicsBody parentBody2 = light4.ParentBody;
					if (!(((parentBody2 != null) ? parentBody2.UserData : null) is Limb) && light4.CurrentBrightness > 0f)
					{
						light4.DrawSprite(CS$<>8__locals1.spriteBatch, cam);
					}
				}
			}
			CS$<>8__locals1.spriteBatch.End();
			if (highlightsVisible)
			{
				SpriteBatch spriteBatch4 = CS$<>8__locals1.spriteBatch;
				SpriteSortMode sortMode3 = SpriteSortMode.Deferred;
				BlendState additive = BlendState.Additive;
				SamplerState samplerState = null;
				DepthStencilState depthStencilState3 = null;
				RasterizerState rasterizerState3 = null;
				Effect effect = null;
				Matrix? transformMatrix = null;
				spriteBatch4.Begin(sortMode3, additive, samplerState, depthStencilState3, rasterizerState3, effect, transformMatrix);
				CS$<>8__locals1.spriteBatch.Draw(this.HighlightMap, Vector2.Zero, Color.White);
				CS$<>8__locals1.spriteBatch.End();
			}
			if (cam.Zoom > 0.5f)
			{
				this.SolidColorEffect.CurrentTechnique = this.SolidColorEffect.Techniques["SolidVertexColor"];
				CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, this.SolidColorEffect, new Matrix?(spriteBatchTransform));
				LightManager.<RenderLightMap>g__DrawCharacters|67_3(CS$<>8__locals1.spriteBatch, cam, false);
				CS$<>8__locals1.spriteBatch.End();
				DeformableSprite.Effect.CurrentTechnique = DeformableSprite.Effect.Techniques["DeformShaderSolidVertexColor"];
				DeformableSprite.Effect.CurrentTechnique.Passes[0].Apply();
				CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(spriteBatchTransform));
				LightManager.<RenderLightMap>g__DrawCharacters|67_3(CS$<>8__locals1.spriteBatch, cam, true);
				CS$<>8__locals1.spriteBatch.End();
			}
			DeformableSprite.Effect.CurrentTechnique = DeformableSprite.Effect.Techniques["DeformShader"];
			graphics.BlendState = BlendState.Additive;
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, new Matrix?(spriteBatchTransform));
			CS$<>8__locals1.spriteBatch.Draw(this.LimbLightMap, new Rectangle(cam.WorldView.X, -cam.WorldView.Y, cam.WorldView.Width, cam.WorldView.Height), Color.White);
			foreach (ElectricalDischarger discharger in ElectricalDischarger.List)
			{
				discharger.DrawElectricity(CS$<>8__locals1.spriteBatch);
			}
			foreach (LightSource light5 in this.activeLights)
			{
				if (!light5.IsBackground && light5.CurrentBrightness > 0f)
				{
					light5.DrawLightVolume(CS$<>8__locals1.spriteBatch, this.lightEffect, transform, this.recalculationCount < 5, ref this.recalculationCount);
				}
			}
			if (ConnectionPanel.ShouldDebugDrawWiring)
			{
				foreach (MapEntity e in (Submarine.VisibleEntities ?? MapEntity.MapEntityList))
				{
					Item item = e as Item;
					if (item != null && !item.IsHidden)
					{
						Wire wire = item.GetComponent<Wire>();
						if (wire != null)
						{
							wire.DebugDraw(CS$<>8__locals1.spriteBatch, 0.4f);
						}
					}
				}
			}
			this.lightEffect.World = transform;
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, false, null, ParticleBlendState.Additive, new bool?(false));
			if (Character.Controlled != null)
			{
				this.<RenderLightMap>g__DrawHalo|67_4(Character.Controlled, ref CS$<>8__locals1);
			}
			else
			{
				foreach (Character character in Character.CharacterList)
				{
					if (character.Submarine != null && !character.IsDead && character.IsHuman)
					{
						this.<RenderLightMap>g__DrawHalo|67_4(character, ref CS$<>8__locals1);
					}
				}
			}
			CS$<>8__locals1.spriteBatch.End();
			graphics.SetRenderTarget(null);
			graphics.BlendState = BlendState.NonPremultiplied;
		}

		// Token: 0x06005058 RID: 20568 RVA: 0x002B3D04 File Offset: 0x002B1F04
		private bool UpdateHighlights(GraphicsDevice graphics, SpriteBatch spriteBatch, Matrix spriteBatchTransform, Camera cam)
		{
			if (GUI.DisableItemHighlights)
			{
				return false;
			}
			this.highlightedEntities.Clear();
			if (Character.Controlled != null)
			{
				if (Character.Controlled.IsKeyDown(InputType.Aim))
				{
					if (!Character.Controlled.HeldItems.Any((Item it) => it.GetComponent<Sprayer>() == null))
					{
						goto IL_EF;
					}
				}
				if (Character.Controlled.FocusedItem != null)
				{
					this.highlightedEntities.Add(Character.Controlled.FocusedItem);
				}
				if (Character.Controlled.FocusedCharacter != null)
				{
					this.highlightedEntities.Add(Character.Controlled.FocusedCharacter);
				}
				foreach (MapEntity me in MapEntity.HighlightedEntities)
				{
					Item item = me as Item;
					if (item != null && item != Character.Controlled.FocusedItem)
					{
						this.highlightedEntities.Add(item);
					}
				}
			}
			IL_EF:
			if (this.highlightedEntities.Count == 0)
			{
				return false;
			}
			graphics.SetRenderTarget(this.HighlightMap);
			this.SolidColorEffect.CurrentTechnique = this.SolidColorEffect.Techniques["SolidColor"];
			this.SolidColorEffect.Parameters["color"].SetValue(Color.LightBlue.ToVector4());
			this.SolidColorEffect.CurrentTechnique.Passes[0].Apply();
			DeformableSprite.Effect.CurrentTechnique = DeformableSprite.Effect.Techniques["DeformShaderSolidColor"];
			DeformableSprite.Effect.Parameters["solidColor"].SetValue(Color.LightBlue.ToVector4());
			DeformableSprite.Effect.CurrentTechnique.Passes[0].Apply();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, null, null, this.SolidColorEffect, new Matrix?(spriteBatchTransform));
			foreach (Entity highlighted in this.highlightedEntities)
			{
				Item item2 = highlighted as Item;
				if (item2 != null)
				{
					if (item2.IconStyle == null || (item2 == Character.Controlled.FocusedItem && Character.Controlled.FocusedItem != null))
					{
						item2.Draw(spriteBatch, false, true, null, null);
					}
				}
				else
				{
					Character character = highlighted as Character;
					if (character != null)
					{
						character.Draw(spriteBatch, cam);
					}
				}
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, null, null, this.SolidColorEffect, new Matrix?(spriteBatchTransform));
			foreach (Entity highlighted2 in this.highlightedEntities)
			{
				Item item3 = highlighted2 as Item;
				if (item3 != null && item3.IconStyle != null && (item3 != Character.Controlled.FocusedItem || Character.Controlled.FocusedItem == null))
				{
					this.SolidColorEffect.Parameters["color"].SetValue(item3.IconStyle.Color.ToVector4());
					this.SolidColorEffect.CurrentTechnique.Passes[0].Apply();
					item3.Draw(spriteBatch, false, true, null, null);
				}
			}
			spriteBatch.End();
			float phase = (float)(Math.Sin(Timing.TotalTime * 3.0) + 1.0) / 2f;
			Vector4 overlayColor = Color.Black.ToVector4() * MathHelper.Lerp(0.5f, 0.9f, phase);
			this.SolidColorEffect.Parameters["color"].SetValue(overlayColor);
			this.SolidColorEffect.CurrentTechnique = this.SolidColorEffect.Techniques["SolidColorBlur"];
			this.SolidColorEffect.CurrentTechnique.Passes[0].Apply();
			DeformableSprite.Effect.Parameters["solidColor"].SetValue(overlayColor);
			DeformableSprite.Effect.CurrentTechnique.Passes[0].Apply();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, this.SolidColorEffect, new Matrix?(spriteBatchTransform));
			foreach (Entity highlighted3 in this.highlightedEntities)
			{
				Item item4 = highlighted3 as Item;
				if (item4 != null)
				{
					this.SolidColorEffect.Parameters["blurDistance"].SetValue(0.02f);
					item4.Draw(spriteBatch, false, true, null, null);
				}
				else
				{
					Character character2 = highlighted3 as Character;
					if (character2 != null)
					{
						this.SolidColorEffect.Parameters["blurDistance"].SetValue(0.05f);
						character2.Draw(spriteBatch, cam);
					}
				}
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null, null);
			spriteBatch.Draw(this.highlightRaster, new Rectangle(0, 0, this.HighlightMap.Width, this.HighlightMap.Height), new Rectangle?(new Rectangle(0, 0, (int)((float)this.HighlightMap.Width / this.currLightMapScale * 0.5f), (int)((float)this.HighlightMap.Height / this.currLightMapScale * 0.5f))), Color.White * 0.5f);
			spriteBatch.End();
			DeformableSprite.Effect.CurrentTechnique = DeformableSprite.Effect.Techniques["DeformShader"];
			return true;
		}

		// Token: 0x06005059 RID: 20569 RVA: 0x002B4338 File Offset: 0x002B2538
		private Dictionary<Hull, Rectangle> GetVisibleHulls(Camera cam)
		{
			this.visibleHulls.Clear();
			foreach (Hull hull in Hull.HullList)
			{
				if (!hull.IsHidden)
				{
					Rectangle drawRect = (hull.Submarine == null) ? hull.Rect : new Rectangle((int)(hull.Submarine.DrawPosition.X + (float)hull.Rect.X), (int)(hull.Submarine.DrawPosition.Y + (float)hull.Rect.Y), hull.Rect.Width, hull.Rect.Height);
					if (drawRect.Right >= cam.WorldView.X && drawRect.X <= cam.WorldView.Right && drawRect.Y - drawRect.Height <= cam.WorldView.Y && drawRect.Y >= cam.WorldView.Y - cam.WorldView.Height)
					{
						this.visibleHulls.Add(hull, drawRect);
					}
				}
			}
			return this.visibleHulls;
		}

		// Token: 0x0600505A RID: 20570 RVA: 0x002B4480 File Offset: 0x002B2680
		public void UpdateObstructVision(GraphicsDevice graphics, SpriteBatch spriteBatch, Camera cam, Vector2 lookAtPosition)
		{
			if ((!this.LosEnabled || this.LosMode == LosMode.None) && this.ObstructVisionAmount <= 0f)
			{
				return;
			}
			if (LightManager.ViewTarget == null)
			{
				return;
			}
			graphics.SetRenderTarget(this.LosTexture);
			if (this.ObstructVisionAmount > 0f)
			{
				graphics.Clear(Color.Black);
				Vector2 diff = lookAtPosition - LightManager.ViewTarget.WorldPosition;
				diff.Y = -diff.Y;
				if (diff.LengthSquared() > 400f)
				{
					this.losOffset = diff;
				}
				float rotation = MathUtils.VectorToAngle(this.losOffset);
				float MinHorizontalScale = MathHelper.Lerp(3.5f, 1.5f, this.ObstructVisionAmount);
				float MaxHorizontalScale = 10f;
				float VerticalScale = MathHelper.Lerp(4f, 1.25f, this.ObstructVisionAmount);
				Vector2 scale = new Vector2(MathHelper.Clamp(this.losOffset.Length() / 256f, MinHorizontalScale, MaxHorizontalScale), VerticalScale);
				float relativeOriginStartPosition = 0.2f;
				float originStartPosition = (float)this.visionCircle.Width * relativeOriginStartPosition / scale.X;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, new Matrix?(cam.Transform * Matrix.CreateScale(new Vector3(GameSettings.CurrentConfig.Graphics.LightMapScale, GameSettings.CurrentConfig.Graphics.LightMapScale, 1f))));
				spriteBatch.Draw(this.visionCircle, new Vector2(LightManager.ViewTarget.WorldPosition.X, -LightManager.ViewTarget.WorldPosition.Y), null, Color.White, rotation, new Vector2(originStartPosition, (float)(this.visionCircle.Height / 2)), scale, SpriteEffects.None, 0f);
				spriteBatch.End();
			}
			else
			{
				graphics.Clear(Color.White);
			}
			if (this.LosEnabled && this.LosMode != LosMode.None && LightManager.ViewTarget != null)
			{
				Vector2 pos = LightManager.ViewTarget.DrawPosition;
				bool centeredOnHead = false;
				Character character = LightManager.ViewTarget as Character;
				if (character != null)
				{
					AnimController animController = character.AnimController;
					Limb head = (animController != null) ? animController.GetLimb(LimbType.Head, true, false, false) : null;
					if (head != null && !head.IsSevered && !head.Removed)
					{
						pos = head.body.DrawPosition;
						centeredOnHead = true;
					}
				}
				Rectangle camView = new Rectangle(cam.WorldView.X, cam.WorldView.Y - cam.WorldView.Height, cam.WorldView.Width, cam.WorldView.Height);
				Matrix shadowTransform = cam.ShaderTransform * Matrix.CreateOrthographic((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight, -1f, 1f) * 0.5f;
				List<ConvexHull> convexHulls = ConvexHull.GetHullsInRange(LightManager.ViewTarget.Position, (float)cam.WorldView.Width * 0.75f, LightManager.ViewTarget.Submarine);
				if (centeredOnHead)
				{
					foreach (ConvexHull ch in convexHulls)
					{
						if (ch.Enabled)
						{
							Vector2 currentViewPos = pos;
							Vector2 defaultViewPos = LightManager.ViewTarget.DrawPosition;
							MapEntity parentEntity = ch.ParentEntity;
							if (((parentEntity != null) ? parentEntity.Submarine : null) != null)
							{
								defaultViewPos -= ch.ParentEntity.Submarine.DrawPosition;
								currentViewPos -= ch.ParentEntity.Submarine.DrawPosition;
							}
							if (ch.LosIntersects(defaultViewPos, currentViewPos))
							{
								pos = LightManager.ViewTarget.DrawPosition;
							}
						}
					}
				}
				if (convexHulls != null)
				{
					LightManager.ShadowVertices.Clear();
					LightManager.PenumbraVertices.Clear();
					foreach (ConvexHull convexHull in convexHulls)
					{
						if (convexHull.Intersects(camView))
						{
							Vector2 relativeViewPos = pos;
							MapEntity parentEntity2 = convexHull.ParentEntity;
							if (((parentEntity2 != null) ? parentEntity2.Submarine : null) != null)
							{
								relativeViewPos -= convexHull.ParentEntity.Submarine.DrawPosition;
							}
							convexHull.CalculateLosVertices(relativeViewPos);
							for (int i = 0; i < convexHull.ShadowVertexCount; i++)
							{
								LightManager.ShadowVertices.Add(convexHull.ShadowVertices[i]);
							}
							for (int j = 0; j < convexHull.PenumbraVertexCount; j++)
							{
								LightManager.PenumbraVertices.Add(convexHull.PenumbraVertices[j]);
							}
						}
					}
					if (LightManager.ShadowVertices.Count > 0)
					{
						ConvexHull.shadowEffect.World = shadowTransform;
						ConvexHull.shadowEffect.CurrentTechnique.Passes[0].Apply();
						graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, LightManager.ShadowVertices.ToArray(), 0, LightManager.ShadowVertices.Count / 3, VertexPositionColor.VertexDeclaration);
						if (LightManager.PenumbraVertices.Count > 0)
						{
							ConvexHull.penumbraEffect.World = shadowTransform;
							ConvexHull.penumbraEffect.CurrentTechnique.Passes[0].Apply();
							graphics.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, LightManager.PenumbraVertices.ToArray(), 0, LightManager.PenumbraVertices.Count / 3, VertexPositionTexture.VertexDeclaration);
						}
					}
				}
			}
			graphics.SetRenderTarget(null);
		}

		// Token: 0x0600505B RID: 20571 RVA: 0x002B49E4 File Offset: 0x002B2BE4
		public void DebugDrawLos(SpriteBatch spriteBatch, Camera cam)
		{
			Entity viewTarget = LightManager.ViewTarget;
			Vector2 pos = (viewTarget != null) ? viewTarget.Position : cam.Position;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, new Matrix?(cam.Transform));
			Vector2 position = pos;
			float range = (float)cam.WorldView.Width * 0.75f;
			Entity viewTarget2 = LightManager.ViewTarget;
			List<ConvexHull> convexHulls = ConvexHull.GetHullsInRange(position, range, (viewTarget2 != null) ? viewTarget2.Submarine : null);
			Rectangle camView = new Rectangle(cam.WorldView.X, cam.WorldView.Y - cam.WorldView.Height, cam.WorldView.Width, cam.WorldView.Height);
			foreach (ConvexHull convexHull in convexHulls)
			{
				if (convexHull.Enabled && convexHull.Intersects(camView))
				{
					Structure structure = convexHull.ParentEntity as Structure;
					if (structure == null || structure.CastShadow)
					{
						convexHull.DebugDraw(spriteBatch);
					}
				}
			}
			spriteBatch.End();
		}

		// Token: 0x0600505C RID: 20572 RVA: 0x002B4B00 File Offset: 0x002B2D00
		public void ClearLights()
		{
			this.activeLights.Clear();
			this.activeShadowCastingLights.Clear();
			this.lights.Clear();
		}

		// Token: 0x0600505E RID: 20574 RVA: 0x002B4B50 File Offset: 0x002B2D50
		[CompilerGenerated]
		internal static RenderTarget2D <CreateRenderTargets>g__CreateRenderTarget|51_0(ref LightManager.<>c__DisplayClass51_0 A_0)
		{
			return new RenderTarget2D(A_0.graphics, (int)((float)GameMain.GraphicsWidth * GameSettings.CurrentConfig.Graphics.LightMapScale), (int)((float)GameMain.GraphicsHeight * GameSettings.CurrentConfig.Graphics.LightMapScale), false, A_0.pp.BackBufferFormat, A_0.pp.DepthStencilFormat, A_0.pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
		}

		// Token: 0x0600505F RID: 20575 RVA: 0x002B4BBC File Offset: 0x002B2DBC
		[CompilerGenerated]
		internal static float <RenderLightMap>g__lightPriority|67_1(float range, LightSource light)
		{
			Character controlled = Character.Controlled;
			float num;
			if (((controlled != null) ? controlled.Submarine : null) != null)
			{
				Submarine parentSub = light.ParentSub;
				Character controlled2 = Character.Controlled;
				if (parentSub == ((controlled2 != null) ? controlled2.Submarine : null))
				{
					num = 2f;
					goto IL_39;
				}
			}
			num = 1f;
			IL_39:
			return range * num * (light.CastShadows ? 10f : 1f) * (light.LightSourceParams.OverrideLightSpriteAlpha ?? ((float)light.Color.A / 255f)) * light.PriorityMultiplier;
		}

		// Token: 0x06005060 RID: 20576 RVA: 0x002B4C54 File Offset: 0x002B2E54
		[CompilerGenerated]
		internal static void <RenderLightMap>g__DrawCharacters|67_3(SpriteBatch spriteBatch, Camera cam, bool drawDeformSprites)
		{
			foreach (Character character in Character.CharacterList)
			{
				if (character.CurrentHull != null && character.Enabled && character.IsVisible && character.InvisibleTimer <= 0f)
				{
					Character controlled = Character.Controlled;
					if (((controlled != null) ? controlled.FocusedCharacter : null) != character)
					{
						Color lightColor = (character.CurrentHull.AmbientLight == Color.TransparentBlack) ? Color.Black : character.CurrentHull.AmbientLight.Multiply((float)character.CurrentHull.AmbientLight.A / 255f, false).Opaque();
						foreach (Limb limb in character.AnimController.Limbs)
						{
							if (drawDeformSprites != (limb.DeformSprite == null))
							{
								limb.Draw(spriteBatch, cam, new Color?(lightColor), false);
							}
						}
						foreach (Item heldItem in character.HeldItems)
						{
							heldItem.Draw(spriteBatch, false, true, new Color?(Color.Black), null);
						}
					}
				}
			}
		}

		// Token: 0x06005061 RID: 20577 RVA: 0x002B4DF4 File Offset: 0x002B2FF4
		[CompilerGenerated]
		private void <RenderLightMap>g__DrawHalo|67_4(Character character, ref LightManager.<>c__DisplayClass67_0 A_2)
		{
			if (character == null || character.Removed)
			{
				return;
			}
			Vector2 haloDrawPos = character.DrawPosition;
			haloDrawPos.Y = -haloDrawPos.Y;
			float ambientBrightness = (float)(this.AmbientLight.R + this.AmbientLight.B + this.AmbientLight.G) / 255f / 3f;
			Color haloColor = Color.White.Multiply(0.3f - ambientBrightness, false);
			if (haloColor.A > 0)
			{
				float scale = 512f / (float)LightSource.LightTexture.Width;
				A_2.spriteBatch.Draw(LightSource.LightTexture, haloDrawPos, null, haloColor, 0f, new Vector2((float)LightSource.LightTexture.Width, (float)LightSource.LightTexture.Height) / 2f, scale, SpriteEffects.None, 0f);
			}
		}

		// Token: 0x04002A52 RID: 10834
		private const int MaxLightVolumeRecalculationsPerFrame = 5;

		// Token: 0x04002A53 RID: 10835
		private const float ObstructLightsBehindCharactersZoomThreshold = 0.5f;

		// Token: 0x04002A54 RID: 10836
		private Thread rayCastThread;

		// Token: 0x04002A55 RID: 10837
		private Queue<LightManager.RayCastTask> pendingRayCasts = new Queue<LightManager.RayCastTask>();

		// Token: 0x04002A57 RID: 10839
		private float currLightMapScale;

		// Token: 0x04002A58 RID: 10840
		public Color AmbientLight;

		// Token: 0x04002A5D RID: 10845
		private readonly Texture2D highlightRaster;

		// Token: 0x04002A5E RID: 10846
		private BasicEffect lightEffect;

		// Token: 0x04002A61 RID: 10849
		private readonly List<LightSource> lights;

		// Token: 0x04002A62 RID: 10850
		public bool DebugLos;

		// Token: 0x04002A63 RID: 10851
		public bool LosEnabled = true;

		// Token: 0x04002A64 RID: 10852
		public float LosAlpha = 1f;

		// Token: 0x04002A65 RID: 10853
		public LosMode LosMode = LosMode.Transparent;

		// Token: 0x04002A66 RID: 10854
		public bool LightingEnabled = true;

		// Token: 0x04002A67 RID: 10855
		public float ObstructVisionAmount;

		// Token: 0x04002A68 RID: 10856
		private readonly Texture2D visionCircle;

		// Token: 0x04002A69 RID: 10857
		private readonly Texture2D gapGlowTexture;

		// Token: 0x04002A6A RID: 10858
		private Vector2 losOffset;

		// Token: 0x04002A6B RID: 10859
		private int recalculationCount;

		// Token: 0x04002A6C RID: 10860
		private float time;

		// Token: 0x04002A6D RID: 10861
		private readonly List<LightSource> activeLights = new List<LightSource>(100);

		// Token: 0x04002A6E RID: 10862
		private readonly List<LightSource> activeShadowCastingLights = new List<LightSource>(100);

		// Token: 0x04002A70 RID: 10864
		private static readonly object mutex = new object();

		// Token: 0x04002A71 RID: 10865
		private readonly List<Entity> highlightedEntities = new List<Entity>();

		// Token: 0x04002A72 RID: 10866
		private readonly Dictionary<Hull, Rectangle> visibleHulls = new Dictionary<Hull, Rectangle>();

		// Token: 0x04002A73 RID: 10867
		private static readonly List<VertexPositionColor> ShadowVertices = new List<VertexPositionColor>(500);

		// Token: 0x04002A74 RID: 10868
		private static readonly List<VertexPositionTexture> PenumbraVertices = new List<VertexPositionTexture>(500);

		// Token: 0x0200125D RID: 4701
		private sealed class RayCastTask
		{
			// Token: 0x06009417 RID: 37911 RVA: 0x003CE3C1 File Offset: 0x003CC5C1
			public RayCastTask(LightSource lightSource, Vector2 drawPos, float rotation)
			{
				this.LightSource = lightSource;
				this.DrawPos = drawPos;
				this.Rotation = rotation;
			}

			// Token: 0x06009418 RID: 37912 RVA: 0x003CE3DE File Offset: 0x003CC5DE
			public void Calculate()
			{
				this.LightSource.RayCastTask(this.DrawPos, this.Rotation);
			}

			// Token: 0x04005EEC RID: 24300
			public LightSource LightSource;

			// Token: 0x04005EED RID: 24301
			public Vector2 DrawPos;

			// Token: 0x04005EEE RID: 24302
			public float Rotation;
		}
	}
}
