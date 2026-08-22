using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000113 RID: 275
	internal class GameScreen : Screen
	{
		// Token: 0x170009FA RID: 2554
		// (get) Token: 0x0600252C RID: 9516 RVA: 0x001795C9 File Offset: 0x001777C9
		// (set) Token: 0x0600252D RID: 9517 RVA: 0x001795D1 File Offset: 0x001777D1
		public Effect PostProcessEffect { get; private set; }

		// Token: 0x170009FB RID: 2555
		// (get) Token: 0x0600252E RID: 9518 RVA: 0x001795DA File Offset: 0x001777DA
		// (set) Token: 0x0600252F RID: 9519 RVA: 0x001795E2 File Offset: 0x001777E2
		public Effect GradientEffect { get; private set; }

		// Token: 0x170009FC RID: 2556
		// (get) Token: 0x06002530 RID: 9520 RVA: 0x001795EB File Offset: 0x001777EB
		// (set) Token: 0x06002531 RID: 9521 RVA: 0x001795F3 File Offset: 0x001777F3
		public Effect GrainEffect { get; private set; }

		// Token: 0x170009FD RID: 2557
		// (get) Token: 0x06002532 RID: 9522 RVA: 0x001795FC File Offset: 0x001777FC
		// (set) Token: 0x06002533 RID: 9523 RVA: 0x00179604 File Offset: 0x00177804
		public Effect ThresholdTintEffect { get; private set; }

		// Token: 0x170009FE RID: 2558
		// (get) Token: 0x06002534 RID: 9524 RVA: 0x0017960D File Offset: 0x0017780D
		// (set) Token: 0x06002535 RID: 9525 RVA: 0x00179615 File Offset: 0x00177815
		public Effect BlueprintEffect { get; set; }

		// Token: 0x06002536 RID: 9526 RVA: 0x00179620 File Offset: 0x00177820
		public GameScreen(GraphicsDevice graphics)
		{
			GameScreen <>4__this = this;
			this.cam = new Camera();
			this.cam.Translate(new Vector2(-10f, 50f));
			this.CreateRenderTargets(graphics);
			GameMain.Instance.ResolutionChanged += delegate()
			{
				<>4__this.CreateRenderTargets(graphics);
			};
			this.DamageEffect = EffectLoader.Load("Effects/damageshader");
			this.PostProcessEffect = EffectLoader.Load("Effects/postprocess");
			this.GradientEffect = EffectLoader.Load("Effects/gradientshader");
			this.GrainEffect = EffectLoader.Load("Effects/grainshader");
			this.ThresholdTintEffect = EffectLoader.Load("Effects/thresholdtint");
			this.BlueprintEffect = EffectLoader.Load("Effects/blueprintshader");
			this.damageStencil = TextureLoader.FromFile("Content/Map/walldamage.png", true, false, null);
			this.DamageEffect.Parameters["xStencil"].SetValue(this.damageStencil);
			this.DamageEffect.Parameters["aMultiplier"].SetValue(50f);
			this.DamageEffect.Parameters["cMultiplier"].SetValue(200f);
			this.distortTexture = TextureLoader.FromFile("Content/Effects/distortnormals.png", true, false, null);
			this.PostProcessEffect.Parameters["xDistortTexture"].SetValue(this.distortTexture);
		}

		// Token: 0x06002537 RID: 9527 RVA: 0x001797A0 File Offset: 0x001779A0
		private void CreateRenderTargets(GraphicsDevice graphics)
		{
			RenderTarget2D renderTarget2D = this.renderTarget;
			if (renderTarget2D != null)
			{
				renderTarget2D.Dispose();
			}
			RenderTarget2D renderTarget2D2 = this.renderTargetBackground;
			if (renderTarget2D2 != null)
			{
				renderTarget2D2.Dispose();
			}
			RenderTarget2D renderTarget2D3 = this.renderTargetWater;
			if (renderTarget2D3 != null)
			{
				renderTarget2D3.Dispose();
			}
			RenderTarget2D renderTarget2D4 = this.renderTargetFinal;
			if (renderTarget2D4 != null)
			{
				renderTarget2D4.Dispose();
			}
			this.renderTarget = new RenderTarget2D(graphics, GameMain.GraphicsWidth, GameMain.GraphicsHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			this.renderTargetBackground = new RenderTarget2D(graphics, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.renderTargetWater = new RenderTarget2D(graphics, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			this.renderTargetFinal = new RenderTarget2D(graphics, GameMain.GraphicsWidth, GameMain.GraphicsHeight, false, SurfaceFormat.Color, DepthFormat.None);
			this.renderTargetDamageable = new RenderTarget2D(graphics, GameMain.GraphicsWidth, GameMain.GraphicsHeight, false, SurfaceFormat.Color, DepthFormat.None);
		}

		// Token: 0x06002538 RID: 9528 RVA: 0x0017986C File Offset: 0x00177A6C
		public override void AddToGUIUpdateList()
		{
			if (Character.Controlled != null)
			{
				Item selectedItem = Character.Controlled.SelectedItem;
				if (selectedItem != null && Character.Controlled.CanInteractWith(selectedItem, true))
				{
					selectedItem.AddToGUIUpdateList(0);
				}
				Item selectedSecondaryItem = Character.Controlled.SelectedSecondaryItem;
				if (selectedSecondaryItem != null && Character.Controlled.CanInteractWith(selectedSecondaryItem, true))
				{
					selectedSecondaryItem.AddToGUIUpdateList(0);
				}
				if (Character.Controlled.Inventory != null)
				{
					foreach (Item item in Character.Controlled.Inventory.AllItems)
					{
						if (Character.Controlled.HasEquippedItem(item, null, null))
						{
							item.AddToGUIUpdateList(0);
						}
					}
				}
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.AddToGUIUpdateList();
			}
			Character.AddAllToGUIUpdateList();
			base.AddToGUIUpdateList();
		}

		// Token: 0x06002539 RID: 9529 RVA: 0x00179954 File Offset: 0x00177B54
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			this.cam.UpdateTransform(true, true);
			Submarine.CullEntities(this.cam);
			foreach (Character c in Character.CharacterList)
			{
				c.AnimController.Limbs.ForEach(delegate(Limb l)
				{
					l.body.UpdateDrawPosition(true);
				});
				bool wasVisible = c.IsVisible;
				c.DoVisibilityCheck(this.cam);
				if (c.IsVisible != wasVisible)
				{
					foreach (Limb limb in c.AnimController.Limbs)
					{
						LightSource light = limb.LightSource;
						if (light != null)
						{
							light.Enabled = c.IsVisible;
						}
					}
				}
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			this.DrawMap(graphics, spriteBatch, deltaTime);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map", sw.ElapsedTicks);
			sw.Restart();
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			if (Character.Controlled != null && this.cam != null)
			{
				Character.Controlled.DrawHUD(spriteBatch, this.cam, true);
			}
			if (GameMain.GameSession != null)
			{
				GameMain.GameSession.Draw(spriteBatch);
			}
			if (Character.Controlled == null && !GUI.DisableHUD)
			{
				this.DrawPositionIndicators(spriteBatch);
			}
			if (!GUI.DisableHUD)
			{
				foreach (Character c2 in Character.CharacterList)
				{
					c2.DrawGUIMessages(spriteBatch, this.cam);
				}
			}
			GUI.Draw(this.cam, spriteBatch);
			spriteBatch.End();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:HUD", sw.ElapsedTicks);
			sw.Restart();
		}

		// Token: 0x0600253A RID: 9530 RVA: 0x00179B6C File Offset: 0x00177D6C
		private void DrawPositionIndicators(SpriteBatch spriteBatch)
		{
			UISprite value = GUIStyle.SubLocationIcon.Value;
			Sprite subLocationSprite = (value != null) ? value.Sprite : null;
			UISprite value2 = GUIStyle.ShuttleIcon.Value;
			Sprite shuttleSprite = (value2 != null) ? value2.Sprite : null;
			UISprite value3 = GUIStyle.WreckIcon.Value;
			Sprite wreckSprite = (value3 != null) ? value3.Sprite : null;
			UISprite value4 = GUIStyle.CaveIcon.Value;
			Sprite caveSprite = (value4 != null) ? value4.Sprite : null;
			UISprite value5 = GUIStyle.OutpostIcon.Value;
			Sprite outpostSprite = (value5 != null) ? value5.Sprite : null;
			UISprite value6 = GUIStyle.RuinIcon.Value;
			Sprite ruinSprite = (value6 != null) ? value6.Sprite : null;
			UISprite value7 = GUIStyle.EnemyIcon.Value;
			Sprite enemySprite = (value7 != null) ? value7.Sprite : null;
			UISprite value8 = GUIStyle.CorpseIcon.Value;
			Sprite corpseSprite = (value8 != null) ? value8.Sprite : null;
			UISprite value9 = GUIStyle.BeaconIcon.Value;
			Sprite beaconSprite = (value9 != null) ? value9.Sprite : null;
			for (int i = 0; i < Submarine.MainSubs.Length; i++)
			{
				if (Submarine.MainSubs[i] != null && (Level.Loaded == null || Submarine.MainSubs[i].WorldPosition.Y >= -1000000f))
				{
					Vector2 position = (Submarine.MainSubs[i].SubBody != null) ? Submarine.MainSubs[i].WorldPosition : Submarine.MainSubs[i].HiddenSubPosition;
					Color indicatorColor = (i == 0) ? (Color.LightBlue * 0.5f) : (GUIStyle.Red * 0.5f);
					Sprite displaySprite = Submarine.MainSubs[i].Info.HasTag(SubmarineTag.Shuttle) ? shuttleSprite : subLocationSprite;
					if (displaySprite != null)
					{
						GUI.DrawIndicator(spriteBatch, position, this.cam, (float)Math.Max(Submarine.MainSubs[i].Borders.Width, Submarine.MainSubs[i].Borders.Height), displaySprite, indicatorColor, true, 1f, null);
					}
				}
			}
			if (!GameMain.DevMode)
			{
				return;
			}
			if (Level.Loaded != null)
			{
				foreach (Level.Cave cave in Level.Loaded.Caves)
				{
					Vector2 position2 = cave.StartPos.ToVector2();
					Color indicatorColor2 = Color.Yellow * 0.5f;
					if (caveSprite != null)
					{
						GUI.DrawIndicator(spriteBatch, position2, this.cam, 3000f, caveSprite, indicatorColor2, true, 1f, null);
					}
				}
			}
			foreach (Submarine submarine in Submarine.Loaded)
			{
				if (!Submarine.MainSubs.Contains(submarine))
				{
					Vector2 position3 = submarine.WorldPosition;
					Color color;
					switch (submarine.TeamID)
					{
					case CharacterTeamType.Team1:
						color = Color.LightBlue * 0.5f;
						break;
					case CharacterTeamType.Team2:
						color = GUIStyle.Red * 0.5f;
						break;
					case CharacterTeamType.FriendlyNPC:
						color = GUIStyle.Yellow * 0.5f;
						break;
					default:
						color = Color.Green * 0.5f;
						break;
					}
					Color teamColorIndicator = color;
					switch (submarine.Info.Type)
					{
					case SubmarineType.Outpost:
						color = Color.LightGreen;
						break;
					case SubmarineType.OutpostModule:
					case SubmarineType.EnemySubmarine:
						goto IL_361;
					case SubmarineType.Wreck:
						color = Color.SaddleBrown;
						break;
					case SubmarineType.BeaconStation:
						color = Color.Azure;
						break;
					case SubmarineType.Ruin:
						color = Color.Purple;
						break;
					default:
						goto IL_361;
					}
					IL_365:
					Color indicatorColor3 = color;
					Sprite sprite;
					switch (submarine.Info.Type)
					{
					case SubmarineType.Outpost:
						sprite = outpostSprite;
						break;
					case SubmarineType.OutpostModule:
					case SubmarineType.EnemySubmarine:
						goto IL_3B1;
					case SubmarineType.Wreck:
						sprite = wreckSprite;
						break;
					case SubmarineType.BeaconStation:
						sprite = beaconSprite;
						break;
					case SubmarineType.Ruin:
						sprite = ruinSprite;
						break;
					default:
						goto IL_3B1;
					}
					IL_3B4:
					Sprite displaySprite2 = sprite;
					if (submarine.Info.SubmarineClass == SubmarineClass.Transport)
					{
						indicatorColor3 *= 0.75f;
					}
					if (displaySprite2 != null)
					{
						GUI.DrawIndicator(spriteBatch, position3, this.cam, (float)Math.Max(submarine.Borders.Width, submarine.Borders.Height), displaySprite2, indicatorColor3, true, 1f, null);
						continue;
					}
					continue;
					IL_3B1:
					sprite = subLocationSprite;
					goto IL_3B4;
					IL_361:
					color = teamColorIndicator;
					goto IL_365;
				}
			}
			foreach (Character character in Character.CharacterList)
			{
				Vector2 position4 = character.WorldPosition;
				Color indicatorColor4 = Color.DarkRed * 0.5f;
				if (character.IsDead)
				{
					indicatorColor4 = Color.DarkGray * 0.5f;
				}
				if (character.TeamID == CharacterTeamType.None)
				{
					Sprite displaySprite3 = character.IsDead ? corpseSprite : enemySprite;
					if (displaySprite3 != null)
					{
						GUI.DrawIndicator(spriteBatch, position4, this.cam, 3000f, displaySprite3, indicatorColor4, true, 1f, null);
					}
				}
			}
		}

		// Token: 0x0600253B RID: 9531 RVA: 0x0017A0AC File Offset: 0x001782AC
		public void DrawMap(GraphicsDevice graphics, SpriteBatch spriteBatch, double deltaTime)
		{
			GameScreen.<>c__DisplayClass34_0 CS$<>8__locals1 = new GameScreen.<>c__DisplayClass34_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			foreach (Submarine sub in Submarine.Loaded)
			{
				sub.UpdateTransform(true);
			}
			GameMain.ParticleManager.UpdateTransforms();
			Stopwatch sw = new Stopwatch();
			sw.Start();
			if (Character.Controlled != null && (Character.Controlled.ViewTarget == Character.Controlled || Character.Controlled.ViewTarget == null))
			{
				GameMain.LightManager.ObstructVisionAmount = Character.Controlled.ObstructVisionAmount;
			}
			else
			{
				GameMain.LightManager.ObstructVisionAmount = 0f;
			}
			LightManager lightManager = GameMain.LightManager;
			SpriteBatch spriteBatch2 = CS$<>8__locals1.spriteBatch;
			Camera camera = this.cam;
			Character controlled = Character.Controlled;
			lightManager.UpdateObstructVision(graphics, spriteBatch2, camera, (controlled != null) ? controlled.CursorWorldPosition : Vector2.Zero);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:LOS", sw.ElapsedTicks);
			sw.Restart();
			graphics.SetRenderTarget(this.renderTarget);
			graphics.Clear(Color.Transparent);
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			Submarine.DrawBack(CS$<>8__locals1.spriteBatch, false, delegate(MapEntity e)
			{
				Structure s = e as Structure;
				return s != null && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && !GameScreen.<DrawMap>g__IsFromOutpostDrawnBehindSubs|34_0(e);
			});
			Submarine.DrawPaintedColors(CS$<>8__locals1.spriteBatch, false, null);
			CS$<>8__locals1.spriteBatch.End();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackStructures", sw.ElapsedTicks);
			sw.Restart();
			graphics.SetRenderTarget(this.renderTargetDamageable);
			graphics.Clear(Color.Transparent);
			this.DamageEffect.CurrentTechnique = this.DamageEffect.Techniques["StencilShader"];
			CS$<>8__locals1.<DrawMap>g__ResetDamageEffect|2();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, this.DamageEffect, new Matrix?(this.cam.Transform));
			Submarine.DrawDamageable(CS$<>8__locals1.spriteBatch, this.DamageEffect, false, null);
			CS$<>8__locals1.spriteBatch.End();
			CS$<>8__locals1.<DrawMap>g__ResetDamageEffect|2();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontDamageable", sw.ElapsedTicks);
			sw.Restart();
			graphics.SetRenderTarget(null);
			GameMain.LightManager.RenderLightMap(graphics, CS$<>8__locals1.spriteBatch, this.cam, this.renderTargetDamageable);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:Lighting", sw.ElapsedTicks);
			sw.Restart();
			graphics.SetRenderTarget(this.renderTargetBackground);
			if (Level.Loaded != null)
			{
				Level.Loaded.DrawBack(graphics, CS$<>8__locals1.spriteBatch, this.cam);
			}
			else
			{
				GameSession gameSession = GameMain.GameSession;
				TestGameMode testMode = ((gameSession != null) ? gameSession.GameMode : null) as TestGameMode;
				if (testMode != null)
				{
					graphics.Clear(testMode.BackgroundParams.BackgroundColor);
					CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null, null);
					testMode.BackgroundParams.DrawBackgrounds(CS$<>8__locals1.spriteBatch, this.cam);
					CS$<>8__locals1.spriteBatch.End();
					CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.DepthRead, null, null, new Matrix?(this.cam.Transform));
					testMode.BackgroundParams.DrawWaterParticles(CS$<>8__locals1.spriteBatch, this.cam, testMode.WaterParticleOffset);
					CS$<>8__locals1.spriteBatch.End();
				}
				else
				{
					graphics.Clear(new Color(11, 18, 26, 255));
				}
			}
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			Submarine.DrawBack(CS$<>8__locals1.spriteBatch, false, delegate(MapEntity e)
			{
				Structure s = e as Structure;
				return s != null && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && GameScreen.<DrawMap>g__IsFromOutpostDrawnBehindSubs|34_0(e);
			});
			CS$<>8__locals1.spriteBatch.End();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, true, new bool?(false), ParticleBlendState.AlphaBlend, new bool?(false));
			CS$<>8__locals1.spriteBatch.End();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, true, new bool?(false), ParticleBlendState.Additive, new bool?(false));
			CS$<>8__locals1.spriteBatch.End();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, null);
			CS$<>8__locals1.spriteBatch.Draw(this.renderTarget, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White);
			CS$<>8__locals1.spriteBatch.End();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackLevel", sw.ElapsedTicks);
			sw.Restart();
			graphics.SetRenderTarget(this.renderTarget);
			graphics.BlendState = BlendState.NonPremultiplied;
			graphics.SamplerStates[0] = SamplerState.LinearWrap;
			GraphicsQuad.UseBasicEffect(this.renderTargetBackground);
			GraphicsQuad.Render();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			Submarine.DrawBack(CS$<>8__locals1.spriteBatch, false, (MapEntity e) => !(e is Structure) || e.SpriteDepth < 0.9f);
			CS$<>8__locals1.<DrawMap>g__DrawCharacters|5(false, true);
			CS$<>8__locals1.spriteBatch.End();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackCharactersItems", sw.ElapsedTicks);
			sw.Restart();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			CS$<>8__locals1.<DrawMap>g__DrawCharacters|5(true, true);
			CS$<>8__locals1.<DrawMap>g__DrawCharacters|5(true, false);
			CS$<>8__locals1.<DrawMap>g__DrawCharacters|5(false, false);
			CS$<>8__locals1.spriteBatch.End();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:DeformableCharacters", sw.ElapsedTicks);
			sw.Restart();
			Level loaded = Level.Loaded;
			if (loaded != null)
			{
				loaded.DrawFront(CS$<>8__locals1.spriteBatch, this.cam);
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontLevel", sw.ElapsedTicks);
			sw.Restart();
			graphics.SetRenderTarget(this.renderTargetWater);
			graphics.BlendState = BlendState.Opaque;
			graphics.SamplerStates[0] = SamplerState.LinearWrap;
			GraphicsQuad.UseBasicEffect(this.renderTarget);
			GraphicsQuad.Render();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, new Matrix?(this.cam.Transform));
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, true, new bool?(true), ParticleBlendState.AlphaBlend, new bool?(false));
			CS$<>8__locals1.spriteBatch.End();
			graphics.SetRenderTarget(this.renderTarget);
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, new Matrix?(this.cam.Transform));
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, false, null, ParticleBlendState.AlphaBlend, new bool?(false));
			CS$<>8__locals1.spriteBatch.End();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, false, null, ParticleBlendState.Additive, new bool?(false));
			CS$<>8__locals1.spriteBatch.End();
			graphics.DepthStencilState = DepthStencilState.DepthRead;
			graphics.SetRenderTarget(this.renderTargetFinal);
			WaterRenderer.Instance.ResetBuffers();
			Hull.UpdateVertices(this.cam, WaterRenderer.Instance);
			WaterRenderer.Instance.RenderWater(CS$<>8__locals1.spriteBatch, this.renderTargetWater, this.cam);
			WaterRenderer.Instance.RenderAir(graphics, this.cam, this.renderTarget, this.Cam.ShaderTransform);
			graphics.DepthStencilState = DepthStencilState.None;
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontParticles", sw.ElapsedTicks);
			sw.Restart();
			GraphicsQuad.UseBasicEffect(this.renderTargetDamageable);
			GraphicsQuad.Render();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			Submarine.DrawFront(CS$<>8__locals1.spriteBatch, false, null);
			CS$<>8__locals1.spriteBatch.End();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontStructuresItems", sw.ElapsedTicks);
			sw.Restart();
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.Default, null, null, new Matrix?(this.cam.Transform));
			GameMain.ParticleManager.Draw(CS$<>8__locals1.spriteBatch, true, new bool?(true), ParticleBlendState.Additive, new bool?(false));
			foreach (ElectricalDischarger discharger in ElectricalDischarger.List)
			{
				discharger.DrawElectricity(CS$<>8__locals1.spriteBatch);
			}
			CS$<>8__locals1.spriteBatch.End();
			if (GameMain.LightManager.LightingEnabled)
			{
				graphics.DepthStencilState = DepthStencilState.None;
				graphics.SamplerStates[0] = SamplerState.LinearWrap;
				graphics.BlendState = CustomBlendStates.Multiplicative;
				GraphicsQuad.UseBasicEffect(GameMain.LightManager.LightMap);
				GraphicsQuad.Render();
			}
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, null, null, new Matrix?(this.cam.Transform));
			foreach (Character c2 in Character.CharacterList)
			{
				c2.DrawFront(CS$<>8__locals1.spriteBatch, this.cam);
			}
			GameMain.LightManager.DebugDrawVertices(CS$<>8__locals1.spriteBatch);
			Level loaded2 = Level.Loaded;
			if (loaded2 != null)
			{
				loaded2.DrawDebugOverlay(CS$<>8__locals1.spriteBatch, this.cam);
			}
			if (GameMain.DebugDraw)
			{
				MapEntity.MapEntityList.ForEach(delegate(MapEntity me)
				{
					AITarget aiTarget = me.AiTarget;
					if (aiTarget == null)
					{
						return;
					}
					aiTarget.Draw(CS$<>8__locals1.spriteBatch);
				});
				Character.CharacterList.ForEach(delegate(Character c)
				{
					AITarget aiTarget = c.AiTarget;
					if (aiTarget == null)
					{
						return;
					}
					aiTarget.Draw(CS$<>8__locals1.spriteBatch);
				});
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.EventManager : null) != null)
				{
					GameMain.GameSession.EventManager.DebugDraw(CS$<>8__locals1.spriteBatch);
				}
			}
			CS$<>8__locals1.spriteBatch.End();
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontMisc", sw.ElapsedTicks);
			sw.Restart();
			if (GameMain.LightManager.LosEnabled && GameMain.LightManager.LosMode != LosMode.None && LightManager.ViewTarget != null)
			{
				GameMain.LightManager.LosEffect.CurrentTechnique = GameMain.LightManager.LosEffect.Techniques["LosShader"];
				GameMain.LightManager.LosEffect.Parameters["blurDistance"].SetValue(0.005f);
				GameMain.LightManager.LosEffect.Parameters["xTexture"].SetValue(this.renderTargetBackground);
				GameMain.LightManager.LosEffect.Parameters["xLosTexture"].SetValue(GameMain.LightManager.LosTexture);
				GameMain.LightManager.LosEffect.Parameters["xLosAlpha"].SetValue(GameMain.LightManager.LosAlpha);
				Color losColor;
				if (GameMain.LightManager.LosMode == LosMode.Transparent)
				{
					Character controlled2 = Character.Controlled;
					float r = (((controlled2 != null) ? controlled2.CharacterHealth : null) == null) ? 0f : Math.Min(Character.Controlled.CharacterHealth.DamageOverlayTimer * 0.5f, 0.5f);
					Vector3 ambientLightHls = GameMain.LightManager.AmbientLight.RgbToHLS();
					Vector3 losColorHls = Color.Lerp(GameMain.LightManager.AmbientLight, Color.Red, r).RgbToHLS();
					losColorHls.Y = ambientLightHls.Y;
					losColor = ToolBox.HLSToRGB(losColorHls);
				}
				else
				{
					losColor = Color.Black;
				}
				GameMain.LightManager.LosEffect.Parameters["xColor"].SetValue(losColor.ToVector4());
				graphics.BlendState = BlendState.NonPremultiplied;
				graphics.SamplerStates[0] = SamplerState.PointClamp;
				graphics.SamplerStates[1] = SamplerState.PointClamp;
				GameMain.LightManager.LosEffect.CurrentTechnique.Passes[0].Apply();
				GraphicsQuad.Render();
				graphics.SamplerStates[0] = SamplerState.LinearWrap;
				graphics.SamplerStates[1] = SamplerState.LinearWrap;
			}
			Character character = Character.Controlled;
			if (character != null)
			{
				float grainStrength = character.GrainStrength;
				Rectangle screenRect = new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
				CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, this.GrainEffect, null);
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, screenRect, Color.White, true, 0f, 1f);
				this.GrainEffect.Parameters["seed"].SetValue(Rand.Range(0f, 1f, Rand.RandSync.Unsynced));
				this.GrainEffect.Parameters["intensity"].SetValue(grainStrength);
				this.GrainEffect.Parameters["grainColor"].SetValue(character.GrainColor.ToVector4());
				CS$<>8__locals1.spriteBatch.End();
			}
			graphics.SetRenderTarget(null);
			Vector3 chromaticAberrationStrength = GameSettings.CurrentConfig.Graphics.ChromaticAberration ? new Vector3(-0.02f, -0.01f, 0f) : Vector3.Zero;
			Level loaded3 = Level.Loaded;
			if (((loaded3 != null) ? loaded3.Renderer : null) != null)
			{
				chromaticAberrationStrength += new Vector3(-0.03f, -0.015f, 0f) * Level.Loaded.Renderer.ChromaticAberrationStrength;
			}
			float BlurStrength;
			float DistortStrength;
			if (Character.Controlled != null)
			{
				BlurStrength = Character.Controlled.BlurStrength * 0.005f;
				DistortStrength = Character.Controlled.DistortStrength;
				if (GameSettings.CurrentConfig.Graphics.RadialDistortion)
				{
					chromaticAberrationStrength -= Vector3.One * Character.Controlled.RadialDistortStrength;
				}
				chromaticAberrationStrength += new Vector3(-0.03f, -0.015f, 0f) * Character.Controlled.ChromaticAberrationStrength;
			}
			else
			{
				BlurStrength = 0f;
				DistortStrength = 0f;
			}
			string postProcessTechnique = "";
			if (BlurStrength > 0f)
			{
				postProcessTechnique += "Blur";
				this.PostProcessEffect.Parameters["blurDistance"].SetValue(BlurStrength);
			}
			if (chromaticAberrationStrength != Vector3.Zero)
			{
				postProcessTechnique += "ChromaticAberration";
				this.PostProcessEffect.Parameters["chromaticAberrationStrength"].SetValue(chromaticAberrationStrength);
			}
			if (DistortStrength > 0f)
			{
				postProcessTechnique += "Distort";
				this.PostProcessEffect.Parameters["distortScale"].SetValue(Vector2.One * DistortStrength);
				this.PostProcessEffect.Parameters["distortUvOffset"].SetValue(WaterRenderer.Instance.WavePos * 0.001f);
			}
			graphics.BlendState = BlendState.Opaque;
			graphics.SamplerStates[0] = SamplerState.LinearClamp;
			graphics.DepthStencilState = DepthStencilState.None;
			if (string.IsNullOrEmpty(postProcessTechnique))
			{
				GraphicsQuad.UseBasicEffect(this.renderTargetFinal);
			}
			else
			{
				this.PostProcessEffect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
				this.PostProcessEffect.Parameters["xTexture"].SetValue(this.renderTargetFinal);
				this.PostProcessEffect.CurrentTechnique = this.PostProcessEffect.Techniques[postProcessTechnique];
				this.PostProcessEffect.CurrentTechnique.Passes[0].Apply();
			}
			GraphicsQuad.Render();
			Character.DrawSpeechBubbles(CS$<>8__locals1.spriteBatch, this.cam);
			if (this.fadeToBlackState > 0f)
			{
				CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.Lerp(Color.TransparentBlack, Color.Black, this.fadeToBlackState), true, 0f, 1f);
				CS$<>8__locals1.spriteBatch.End();
			}
			if (GameMain.LightManager.DebugLos)
			{
				GameMain.LightManager.DebugDrawLos(CS$<>8__locals1.spriteBatch, this.cam);
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:PostProcess", sw.ElapsedTicks);
			sw.Restart();
		}

		// Token: 0x170009FF RID: 2559
		// (get) Token: 0x0600253C RID: 9532 RVA: 0x0017B214 File Offset: 0x00179414
		public override Camera Cam
		{
			get
			{
				return this.cam;
			}
		}

		// Token: 0x17000A00 RID: 2560
		// (get) Token: 0x0600253D RID: 9533 RVA: 0x0017B21C File Offset: 0x0017941C
		// (set) Token: 0x0600253E RID: 9534 RVA: 0x0017B224 File Offset: 0x00179424
		public double GameTime { get; private set; }

		// Token: 0x0600253F RID: 9535 RVA: 0x0017B22D File Offset: 0x0017942D
		public GameScreen()
		{
			this.cam = new Camera();
			this.cam.Translate(new Vector2(-10f, 50f));
		}

		// Token: 0x06002540 RID: 9536 RVA: 0x0017B268 File Offset: 0x00179468
		public override void Select()
		{
			base.Select();
			if (Character.Controlled != null)
			{
				this.cam.Position = Character.Controlled.WorldPosition;
				this.cam.UpdateTransform(true, true);
			}
			else if (Submarine.MainSub != null)
			{
				this.cam.Position = Submarine.MainSub.WorldPosition;
				this.cam.UpdateTransform(true, true);
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				CrewManager crewManager = gameSession.CrewManager;
				if (crewManager != null)
				{
					crewManager.ResetCrewListOpenState();
				}
			}
			ChatBox.ResetChatBoxOpenState();
			MapEntity.ClearHighlightedEntities();
		}

		// Token: 0x06002541 RID: 9537 RVA: 0x0017B2F4 File Offset: 0x001794F4
		public unsafe override void Deselect()
		{
			base.Deselect();
			GameSettings.Config config = *GameSettings.CurrentConfig;
			config.CrewMenuOpen = CrewManager.PreferCrewMenuOpen;
			config.ChatOpen = ChatBox.PreferChatBoxOpen;
			GameSettings.SetCurrentConfig(config);
			GameSettings.SaveCurrentConfig();
			GameMain.SoundManager.SetCategoryMuffle(SoundManager.SoundCategoryDefault, false);
			GUI.ClearMessages();
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is TestGameMode)
			{
				DebugConsole.DeactivateCheats();
			}
		}

		// Token: 0x06002542 RID: 9538 RVA: 0x0017B368 File Offset: 0x00179568
		public override void Update(double deltaTime)
		{
			LightManager lightManager = GameMain.LightManager;
			if (lightManager != null)
			{
				lightManager.Update((float)deltaTime);
			}
			this.GameTime += deltaTime;
			foreach (PhysicsBody body in PhysicsBody.List)
			{
				if ((body.Enabled || body.UserData is Character) && body.BodyType != BodyType.Static)
				{
					body.Update();
				}
			}
			MapEntity.ClearHighlightedEntities();
			Stopwatch sw = new Stopwatch();
			sw.Start();
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.Update((float)deltaTime);
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:GameSession", sw.ElapsedTicks);
			sw.Restart();
			GameMain.ParticleManager.Update((float)deltaTime);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Particles", sw.ElapsedTicks);
			sw.Restart();
			if (Level.Loaded != null)
			{
				Level.Loaded.Update((float)deltaTime, this.cam);
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Level", sw.ElapsedTicks);
			Character controlled = Character.Controlled;
			if (controlled != null)
			{
				if (controlled.SelectedItem != null && controlled.CanInteractWith(controlled.SelectedItem, true))
				{
					controlled.SelectedItem.UpdateHUD(this.cam, controlled, (float)deltaTime);
				}
				if (controlled.Inventory != null)
				{
					foreach (Item item in controlled.Inventory.AllItems)
					{
						if (controlled.HasEquippedItem(item, null, null))
						{
							item.UpdateHUD(this.cam, controlled, (float)deltaTime);
						}
					}
				}
			}
			sw.Restart();
			Character.UpdateAll((float)deltaTime, this.cam);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Character", sw.ElapsedTicks);
			sw.Restart();
			StatusEffect.UpdateAll((float)deltaTime);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:StatusEffects", sw.ElapsedTicks);
			sw.Restart();
			if (Character.Controlled != null && LightManager.ViewTarget != null)
			{
				Vector2 targetPos = LightManager.ViewTarget.WorldPosition;
				if (LightManager.ViewTarget == Character.Controlled)
				{
					targetPos += ConvertUnits.ToDisplayUnits(Character.Controlled.AnimController.Collider.NetworkPositionErrorOffset);
					if (CharacterHealth.OpenHealthWindow != null || CrewManager.IsCommandInterfaceOpen || ConversationAction.IsDialogOpen)
					{
						Vector2 screenTargetPos = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight) * 0.5f;
						if (CharacterHealth.OpenHealthWindow != null)
						{
							screenTargetPos.X = (float)GameMain.GraphicsWidth * ((CharacterHealth.OpenHealthWindow.Alignment == Alignment.Left) ? 0.6f : 0.4f);
						}
						else if (ConversationAction.IsDialogOpen)
						{
							screenTargetPos.Y = (float)GameMain.GraphicsHeight * 0.4f;
						}
						Vector2 screenOffset = screenTargetPos - new Vector2((float)(GameMain.GraphicsWidth / 2), (float)(GameMain.GraphicsHeight / 2));
						screenOffset.Y = -screenOffset.Y;
						targetPos -= screenOffset / this.cam.Zoom;
					}
				}
				this.cam.TargetPos = targetPos;
			}
			this.cam.MoveCamera((float)deltaTime, true, GUI.MouseOn == null && !Inventory.IsMouseOnInventory, true, null);
			Character controlled2 = Character.Controlled;
			if (controlled2 != null)
			{
				controlled2.UpdateLocalCursor(this.cam);
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				sub.SetPrevTransform(sub.Position);
			}
			foreach (PhysicsBody body2 in PhysicsBody.List)
			{
				if (body2.Enabled && body2.BodyType != BodyType.Static)
				{
					body2.SetPrevTransform(body2.SimPosition, body2.Rotation);
				}
			}
			MapEntity.UpdateAll((float)deltaTime, this.cam);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity", sw.ElapsedTicks);
			sw.Restart();
			Character.UpdateAnimAll((float)deltaTime);
			Ragdoll.UpdateAll((float)deltaTime, this.cam);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Ragdolls", sw.ElapsedTicks);
			sw.Restart();
			foreach (Submarine sub2 in Submarine.Loaded)
			{
				sub2.Update((float)deltaTime);
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Submarine", sw.ElapsedTicks);
			sw.Restart();
			try
			{
				GameMain.World.Step(0.016666668f);
			}
			catch (WorldLockedException e)
			{
				string errorMsg = "Attempted to modify the state of the physics simulation while a time step was running.";
				DebugConsole.ThrowError(errorMsg, e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("GameScreen.Update:WorldLockedException" + e.Message, GameAnalyticsManager.ErrorSeverity.Critical, errorMsg);
			}
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Physics", sw.ElapsedTicks);
			this.UpdateProjSpecific(deltaTime);
		}

		// Token: 0x06002543 RID: 9539 RVA: 0x0017B8E8 File Offset: 0x00179AE8
		private void UpdateProjSpecific(double deltaTime)
		{
			if (ConversationAction.FadeScreenToBlack)
			{
				this.fadeToBlackState = Math.Min(this.fadeToBlackState + (float)deltaTime, 1f);
			}
			else
			{
				this.fadeToBlackState = Math.Max(this.fadeToBlackState - (float)deltaTime, 0f);
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				Inventory.DraggingSlot = null;
				Inventory.DraggingItems.Clear();
			}
		}

		// Token: 0x06002544 RID: 9540 RVA: 0x0017B948 File Offset: 0x00179B48
		private void ExecutePhysics()
		{
			for (;;)
			{
				if (this.physicsTime >= 0.016666666666666666)
				{
					object obj = this.updateLock;
					lock (obj)
					{
						GameMain.World.Step(0.016666668f);
						this.physicsTime -= 0.016666666666666666;
					}
				}
			}
		}

		// Token: 0x06002545 RID: 9541 RVA: 0x0017B9BC File Offset: 0x00179BBC
		[CompilerGenerated]
		internal static bool <DrawMap>g__IsFromOutpostDrawnBehindSubs|34_0(Entity e)
		{
			Submarine submarine = e.Submarine;
			if (submarine != null)
			{
				SubmarineInfo info = submarine.Info;
				if (info != null)
				{
					OutpostGenerationParams outpostGenerationParams = info.OutpostGenerationParams;
					if (outpostGenerationParams != null)
					{
						return outpostGenerationParams.DrawBehindSubs;
					}
				}
			}
			return false;
		}

		// Token: 0x04001280 RID: 4736
		private RenderTarget2D renderTargetBackground;

		// Token: 0x04001281 RID: 4737
		private RenderTarget2D renderTarget;

		// Token: 0x04001282 RID: 4738
		private RenderTarget2D renderTargetWater;

		// Token: 0x04001283 RID: 4739
		private RenderTarget2D renderTargetFinal;

		// Token: 0x04001284 RID: 4740
		private RenderTarget2D renderTargetDamageable;

		// Token: 0x04001285 RID: 4741
		public readonly Effect DamageEffect;

		// Token: 0x04001286 RID: 4742
		private readonly Texture2D damageStencil;

		// Token: 0x04001287 RID: 4743
		private readonly Texture2D distortTexture;

		// Token: 0x04001288 RID: 4744
		private float fadeToBlackState;

		// Token: 0x0400128E RID: 4750
		private object updateLock = new object();

		// Token: 0x0400128F RID: 4751
		private double physicsTime;

		// Token: 0x04001290 RID: 4752
		private readonly Camera cam;
	}
}
