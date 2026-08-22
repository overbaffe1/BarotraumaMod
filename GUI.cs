using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.CharacterEditor;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.Networking;
using Barotrauma.Sounds;
using EventInput;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200007D RID: 125
	internal static class GUI
	{
		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x0600113A RID: 4410 RVA: 0x000AA14F File Offset: 0x000A834F
		public static GUICanvas Canvas
		{
			get
			{
				return GUICanvas.Instance;
			}
		}

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x0600113B RID: 4411 RVA: 0x000AA156 File Offset: 0x000A8356
		public static float Scale
		{
			get
			{
				return ((float)GUI.UIWidth / GUI.ReferenceResolution.X + (float)GameMain.GraphicsHeight / GUI.ReferenceResolution.Y) / 2f * GameSettings.CurrentConfig.Graphics.HUDScale;
			}
		}

		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x0600113C RID: 4412 RVA: 0x000AA191 File Offset: 0x000A8391
		public static float xScale
		{
			get
			{
				return (float)GUI.UIWidth / GUI.ReferenceResolution.X * GameSettings.CurrentConfig.Graphics.HUDScale;
			}
		}

		// Token: 0x1700043D RID: 1085
		// (get) Token: 0x0600113D RID: 4413 RVA: 0x000AA1B4 File Offset: 0x000A83B4
		public static float yScale
		{
			get
			{
				return (float)GameMain.GraphicsHeight / GUI.ReferenceResolution.Y * GameSettings.CurrentConfig.Graphics.HUDScale;
			}
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x000AA1D7 File Offset: 0x000A83D7
		public static int IntScale(float f)
		{
			return (int)(f * GUI.Scale);
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x000AA1E1 File Offset: 0x000A83E1
		public static int IntScaleFloor(float f)
		{
			return (int)Math.Floor((double)(f * GUI.Scale));
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x000AA1F1 File Offset: 0x000A83F1
		public static int IntScaleCeiling(float f)
		{
			return (int)Math.Ceiling((double)(f * GUI.Scale));
		}

		// Token: 0x06001141 RID: 4417 RVA: 0x000AA201 File Offset: 0x000A8401
		public static float AdjustForTextScale(float f)
		{
			return f * GameSettings.CurrentConfig.Graphics.TextScale;
		}

		// Token: 0x1700043E RID: 1086
		// (get) Token: 0x06001142 RID: 4418 RVA: 0x000AA214 File Offset: 0x000A8414
		public static float HorizontalAspectRatio
		{
			get
			{
				return (float)GameMain.GraphicsWidth / (float)GameMain.GraphicsHeight;
			}
		}

		// Token: 0x1700043F RID: 1087
		// (get) Token: 0x06001143 RID: 4419 RVA: 0x000AA223 File Offset: 0x000A8423
		public static float VerticalAspectRatio
		{
			get
			{
				return (float)GameMain.GraphicsHeight / (float)GameMain.GraphicsWidth;
			}
		}

		// Token: 0x17000440 RID: 1088
		// (get) Token: 0x06001144 RID: 4420 RVA: 0x000AA232 File Offset: 0x000A8432
		public static float RelativeHorizontalAspectRatio
		{
			get
			{
				return GUI.HorizontalAspectRatio / (GUI.ReferenceResolution.X / GUI.ReferenceResolution.Y);
			}
		}

		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06001145 RID: 4421 RVA: 0x000AA24F File Offset: 0x000A844F
		public static float RelativeVerticalAspectRatio
		{
			get
			{
				return GUI.VerticalAspectRatio / (GUI.ReferenceResolution.Y / GUI.ReferenceResolution.X);
			}
		}

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x06001146 RID: 4422 RVA: 0x000AA26C File Offset: 0x000A846C
		public static float AspectRatioDifference
		{
			get
			{
				float referenceAspectRatio = GUI.ReferenceResolution.X / GUI.ReferenceResolution.Y;
				float aspectRatioDifference = referenceAspectRatio - GUI.HorizontalAspectRatio;
				if (MathUtils.NearlyEqual(aspectRatioDifference, 0f, 0.0001f))
				{
					return 0f;
				}
				return aspectRatioDifference;
			}
		}

		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x06001147 RID: 4423 RVA: 0x000AA2B0 File Offset: 0x000A84B0
		public static float AspectRatioAdjustment
		{
			get
			{
				if (GUI.HorizontalAspectRatio >= 1.4f)
				{
					return 1f;
				}
				return 1f - (1.4f - GUI.HorizontalAspectRatio);
			}
		}

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x06001148 RID: 4424 RVA: 0x000AA2D5 File Offset: 0x000A84D5
		public static bool IsUltrawide
		{
			get
			{
				return GUI.HorizontalAspectRatio > 2.3f;
			}
		}

		// Token: 0x17000445 RID: 1093
		// (get) Token: 0x06001149 RID: 4425 RVA: 0x000AA2E3 File Offset: 0x000A84E3
		public static bool IsHUDScaled
		{
			get
			{
				return GameSettings.CurrentConfig.Graphics.HUDScale > 1f || GameSettings.CurrentConfig.Graphics.InventoryScale > 1f;
			}
		}

		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x0600114A RID: 4426 RVA: 0x000AA313 File Offset: 0x000A8513
		public static int UIWidth
		{
			get
			{
				if (GUI.IsUltrawide)
				{
					return (int)((float)GameMain.GraphicsHeight * GUI.ReferenceResolution.X / GUI.ReferenceResolution.Y);
				}
				return GameMain.GraphicsWidth;
			}
		}

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x0600114B RID: 4427 RVA: 0x000AA33F File Offset: 0x000A853F
		public static float SlicedSpriteScale
		{
			get
			{
				if (Math.Abs(1f - GUI.Scale) < 0.1f)
				{
					return 1f;
				}
				return GUI.Scale;
			}
		}

		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x0600114C RID: 4428 RVA: 0x000AA363 File Offset: 0x000A8563
		public static Texture2D WhiteTexture
		{
			get
			{
				return GUI.solidWhiteTexture;
			}
		}

		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x0600114D RID: 4429 RVA: 0x000AA36A File Offset: 0x000A856A
		private static GUICursor MouseCursorSprites
		{
			get
			{
				return GUIStyle.CursorSprite;
			}
		}

		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x0600114E RID: 4430 RVA: 0x000AA371 File Offset: 0x000A8571
		public static GraphicsDevice GraphicsDevice
		{
			get
			{
				return GameMain.Instance.GraphicsDevice;
			}
		}

		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x0600114F RID: 4431 RVA: 0x000AA37D File Offset: 0x000A857D
		// (set) Token: 0x06001150 RID: 4432 RVA: 0x000AA384 File Offset: 0x000A8584
		public static GUIFrame PauseMenu { get; private set; }

		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x06001151 RID: 4433 RVA: 0x000AA38C File Offset: 0x000A858C
		// (set) Token: 0x06001152 RID: 4434 RVA: 0x000AA393 File Offset: 0x000A8593
		public static GUIFrame SettingsMenuContainer { get; private set; }

		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x06001153 RID: 4435 RVA: 0x000AA39B File Offset: 0x000A859B
		public static Sprite Arrow
		{
			get
			{
				return GUIStyle.Arrow.Value.Sprite;
			}
		}

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x06001154 RID: 4436 RVA: 0x000AA3AC File Offset: 0x000A85AC
		// (set) Token: 0x06001155 RID: 4437 RVA: 0x000AA3B3 File Offset: 0x000A85B3
		public static KeyboardDispatcher KeyboardDispatcher { get; set; }

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x06001156 RID: 4438 RVA: 0x000AA3BB File Offset: 0x000A85BB
		// (set) Token: 0x06001157 RID: 4439 RVA: 0x000AA3C4 File Offset: 0x000A85C4
		public static bool SettingsMenuOpen
		{
			get
			{
				return GUI.settingsMenuOpen;
			}
			set
			{
				if (value == GUI.SettingsMenuOpen)
				{
					return;
				}
				if (value)
				{
					GUI.SettingsMenuContainer = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
					new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.SettingsMenuContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
					GUIFrame settingsMenuInner = new GUIFrame(new RectTransform(new Vector2(1f, 0.8f), GUI.SettingsMenuContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Smallest)
					{
						MinSize = new Point(640, 480)
					}, "", null);
					SettingsMenu.Create(settingsMenuInner.RectTransform);
				}
				else
				{
					SettingsMenu instance = SettingsMenu.Instance;
					if (instance != null)
					{
						instance.Close();
					}
				}
				GUI.settingsMenuOpen = value;
			}
		}

		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x06001158 RID: 4440 RVA: 0x000AA4F2 File Offset: 0x000A86F2
		// (set) Token: 0x06001159 RID: 4441 RVA: 0x000AA4F9 File Offset: 0x000A86F9
		public static bool PauseMenuOpen { get; private set; }

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x0600115A RID: 4442 RVA: 0x000AA504 File Offset: 0x000A8704
		public static bool InputBlockingMenuOpen
		{
			get
			{
				if (!GUI.PauseMenuOpen && !GUI.SettingsMenuOpen)
				{
					SocialOverlay instance = SocialOverlay.Instance;
					if ((instance == null || !instance.IsOpen) && !DebugConsole.IsOpen && !GameSession.IsTabMenuOpen)
					{
						GameSession gameSession = GameMain.GameSession;
						GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
						if ((gameMode == null || !gameMode.Paused) && !CharacterHUD.IsCampaignInterfaceOpen)
						{
							GameSession gameSession2 = GameMain.GameSession;
							CampaignMode campaignMode = (gameSession2 != null) ? gameSession2.Campaign : null;
							if (campaignMode != null)
							{
								SlideshowPlayer slideshowPlayer = campaignMode.SlideshowPlayer;
								if (slideshowPlayer != null && !slideshowPlayer.Finished)
								{
									return slideshowPlayer.Visible;
								}
							}
							return false;
						}
					}
				}
				return true;
			}
		}

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x0600115B RID: 4443 RVA: 0x000AA593 File Offset: 0x000A8793
		// (set) Token: 0x0600115C RID: 4444 RVA: 0x000AA59A File Offset: 0x000A879A
		public static Color ScreenOverlayColor { get; set; }

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x0600115D RID: 4445 RVA: 0x000AA5A2 File Offset: 0x000A87A2
		private static bool IsSavingIndicatorVisible
		{
			get
			{
				return GUI.savingIndicatorColor.A > 0;
			}
		}

		// Token: 0x0600115E RID: 4446 RVA: 0x000AA5B1 File Offset: 0x000A87B1
		public static void Init()
		{
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				GUI.solidWhiteTexture = new Texture2D(GUI.GraphicsDevice, 1, 1);
				GUI.solidWhiteTexture.SetData<Color>(new Color[]
				{
					Color.White
				});
			});
		}

		// Token: 0x0600115F RID: 4447 RVA: 0x000AA5D8 File Offset: 0x000A87D8
		public static void Draw(Camera cam, SpriteBatch spriteBatch)
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				GUI.usedIndicatorAngles.Clear();
				if (GUI.ScreenChanged)
				{
					GUI.updateList.Clear();
					GUI.updateListSet.Clear();
					Screen selected = Screen.Selected;
					if (selected != null)
					{
						selected.AddToGUIUpdateList();
					}
					GUI.ScreenChanged = false;
				}
				foreach (GUIComponent c3 in GUI.updateList)
				{
					c3.DrawAuto(spriteBatch);
				}
				foreach (GUIComponent c2 in GUI.updateList)
				{
					GUITextBox box = c2 as GUITextBox;
					if (box != null)
					{
						box.DrawIMEPreview(spriteBatch);
					}
				}
				if ((float)GUI.ScreenOverlayColor.A > 0f)
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), GUI.ScreenOverlayColor, true, 0f, 1f);
				}
				if (GUI.DisableHUD)
				{
					GUI.DrawSavingIndicator(spriteBatch);
				}
				else
				{
					float startY = 10f;
					float yStep = GUI.AdjustForTextScale(18f) * GUI.yScale;
					if (GameMain.ShowFPS || GameMain.DebugDraw || GameMain.ShowPerf)
					{
						float y = startY;
						GUI.DrawString(spriteBatch, new Vector2(10f, y), "FPS: " + Math.Round(GameMain.PerformanceCounter.AverageFramesPerSecond).ToString(), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						if (GameMain.GameSession != null && (double)GameMain.GameSession.RoundDuration > 1.0)
						{
							y += yStep;
							Vector2 pos2 = new Vector2(10f, y);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Physics: ");
							defaultInterpolatedStringHandler.AppendFormatted<int>(GameMain.CurrentUpdateRate);
							GUI.DrawString(spriteBatch, pos2, defaultInterpolatedStringHandler.ToStringAndClear(), (GameMain.CurrentUpdateRate < 60) ? Color.Red : Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						}
						if (GameMain.DebugDraw || GameMain.ShowPerf)
						{
							y += yStep;
							GUI.DrawString(spriteBatch, new Vector2(10f, y), "Active lights: " + LightManager.ActiveLightCount.ToString(), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							y += yStep;
							GUI.DrawString(spriteBatch, new Vector2(10f, y), "Physics: " + GameMain.World.UpdateTime.TotalMilliseconds.ToString() + " ms", Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							y += yStep;
							try
							{
								Vector2 pos3 = new Vector2(10f, y);
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 3);
								defaultInterpolatedStringHandler2.AppendLiteral("Bodies: ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(GameMain.World.BodyList.Count);
								defaultInterpolatedStringHandler2.AppendLiteral(" (");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(GameMain.World.BodyList.Count((Body b) => b != null && b.Awake && b.Enabled));
								defaultInterpolatedStringHandler2.AppendLiteral(" awake, ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(GameMain.World.BodyList.Count((Body b) => b != null && b.Awake && b.BodyType == BodyType.Dynamic && b.Enabled));
								defaultInterpolatedStringHandler2.AppendLiteral(" dynamic)");
								GUI.DrawString(spriteBatch, pos3, defaultInterpolatedStringHandler2.ToStringAndClear(), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							}
							catch (InvalidOperationException)
							{
								DebugConsole.AddWarning("Exception while rendering debug info. Physics bodies may have been created or removed while rendering.", null);
							}
							y += yStep;
							GUI.DrawString(spriteBatch, new Vector2(10f, y), "Particle count: " + GameMain.ParticleManager.ParticleCount.ToString() + "/" + GameMain.ParticleManager.MaxParticles.ToString(), Color.Lerp(GUIStyle.Green, GUIStyle.Red, (float)GameMain.ParticleManager.ParticleCount / (float)GameMain.ParticleManager.MaxParticles), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						}
					}
					if (GameMain.ShowPerf)
					{
						float x = 400f;
						float y2 = startY;
						GUI.DrawString(spriteBatch, new Vector2(x, y2), string.Concat(new string[]
						{
							"Draw - Avg: ",
							GameMain.PerformanceCounter.DrawTimeGraph.Average().ToString("0.00"),
							" ms Max: ",
							GameMain.PerformanceCounter.DrawTimeGraph.LargestValue().ToString("0.00"),
							" ms"
						}), GUIStyle.Green, new Color?(Color.Black * 0.8f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						y2 += yStep;
						Graph drawTimeGraph = GameMain.PerformanceCounter.DrawTimeGraph;
						Rectangle rect = new Rectangle((int)x, (int)y2, 170, 50);
						Color? color = new Color?(GUIStyle.Green);
						drawTimeGraph.Draw(spriteBatch, rect, null, 0f, color, null);
						y2 += yStep * 4f;
						GUI.DrawString(spriteBatch, new Vector2(x, y2), string.Concat(new string[]
						{
							"Update - Avg: ",
							GameMain.PerformanceCounter.UpdateTimeGraph.Average().ToString("0.00"),
							" ms Max: ",
							GameMain.PerformanceCounter.UpdateTimeGraph.LargestValue().ToString("0.00"),
							" ms"
						}), Color.LightBlue, new Color?(Color.Black * 0.8f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						y2 += yStep;
						Graph updateTimeGraph = GameMain.PerformanceCounter.UpdateTimeGraph;
						Rectangle rect2 = new Rectangle((int)x, (int)y2, 170, 50);
						color = new Color?(Color.LightBlue);
						updateTimeGraph.Draw(spriteBatch, rect2, null, 0f, color, null);
						y2 += yStep * 4f;
						foreach (string key in from i in GameMain.PerformanceCounter.GetSavedIdentifiers
						orderby i
						select i)
						{
							float elapsedMillisecs = GameMain.PerformanceCounter.GetAverageElapsedMillisecs(key);
							int categoryDepth = key.Count((char c) => c == ':');
							float runningSlowThreshold = 10f / (float)categoryDepth;
							GUI.DrawString(spriteBatch, new Vector2(x + (float)(categoryDepth * 15), y2), key.Split(':', StringSplitOptions.None).Last<string>() + ": " + elapsedMillisecs.ToString("0.00"), ToolBox.GradientLerp(elapsedMillisecs / runningSlowThreshold, new Color[]
							{
								Color.LightGreen,
								GUIStyle.Yellow,
								GUIStyle.Orange,
								GUIStyle.Red,
								Color.Magenta
							}), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							y2 += yStep;
						}
						if (Powered.Grids != null)
						{
							GUI.DrawString(spriteBatch, new Vector2(x, y2), "Grids: " + Powered.Grids.Count.ToString(), Color.LightGreen, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							y2 += yStep;
						}
						x += yStep * 2f;
						GUI.DrawString(spriteBatch, new Vector2(x, y2), "ContinuousPhysicsTime: " + GameMain.World.ContinuousPhysicsTime.TotalMilliseconds.ToString("0.00"), Color.Lerp(Color.LightGreen, GUIStyle.Red, (float)GameMain.World.ContinuousPhysicsTime.TotalMilliseconds / 10f), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						GUI.DrawString(spriteBatch, new Vector2(x, y2 + yStep), "ControllersUpdateTime: " + GameMain.World.ControllersUpdateTime.TotalMilliseconds.ToString("0.00"), Color.Lerp(Color.LightGreen, GUIStyle.Red, (float)GameMain.World.ControllersUpdateTime.TotalMilliseconds / 10f), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						GUI.DrawString(spriteBatch, new Vector2(x, y2 + yStep * 2f), "AddRemoveTime: " + GameMain.World.AddRemoveTime.TotalMilliseconds.ToString("0.00"), Color.Lerp(Color.LightGreen, GUIStyle.Red, (float)GameMain.World.AddRemoveTime.TotalMilliseconds / 10f), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						GUI.DrawString(spriteBatch, new Vector2(x, y2 + yStep * 3f), "NewContactsTime: " + GameMain.World.NewContactsTime.TotalMilliseconds.ToString("0.00"), Color.Lerp(Color.LightGreen, GUIStyle.Red, (float)GameMain.World.NewContactsTime.TotalMilliseconds / 10f), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						GUI.DrawString(spriteBatch, new Vector2(x, y2 + yStep * 4f), "ContactsUpdateTime: " + GameMain.World.ContactsUpdateTime.TotalMilliseconds.ToString("0.00"), Color.Lerp(Color.LightGreen, GUIStyle.Red, (float)GameMain.World.ContactsUpdateTime.TotalMilliseconds / 10f), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						GUI.DrawString(spriteBatch, new Vector2(x, y2 + yStep * 5f), "SolveUpdateTime: " + GameMain.World.SolveUpdateTime.TotalMilliseconds.ToString("0.00"), Color.Lerp(Color.LightGreen, GUIStyle.Red, (float)GameMain.World.SolveUpdateTime.TotalMilliseconds / 10f), new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
					}
					if (GameMain.DebugDraw && !Submarine.Unloading && !(Screen.Selected is RoundSummaryScreen))
					{
						float y3 = startY + yStep * 6f;
						if (Screen.Selected.Cam != null)
						{
							y3 += yStep;
							GUI.DrawString(spriteBatch, new Vector2(10f, y3), "Camera pos: " + Screen.Selected.Cam.Position.ToPoint().ToString() + ", zoom: " + Screen.Selected.Cam.Zoom.ToString(), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						}
						if (Submarine.MainSub != null)
						{
							y3 += yStep;
							GUI.DrawString(spriteBatch, new Vector2(10f, y3), "Sub pos: " + Submarine.MainSub.WorldPosition.ToPoint().ToString(), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						}
						if (GUI.loadedSpritesText == null || DateTime.Now > GUI.loadedSpritesUpdateTime)
						{
							string[] array = new string[5];
							array[0] = "Loaded sprites: ";
							array[1] = Sprite.LoadedSprites.Count<Sprite>().ToString();
							array[2] = "\n(";
							array[3] = (from s in Sprite.LoadedSprites
							select s.FilePath).Distinct<ContentPath>().Count<ContentPath>().ToString();
							array[4] = " unique textures)";
							GUI.loadedSpritesText = string.Concat(array);
							GUI.loadedSpritesUpdateTime = DateTime.Now + new TimeSpan(0, 0, 5);
						}
						y3 += yStep * 2f;
						GUI.DrawString(spriteBatch, new Vector2(10f, y3), GUI.loadedSpritesText, Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						if (GUI.debugDrawSounds)
						{
							float soundTextY = 0f;
							GUI.DrawString(spriteBatch, new Vector2(500f, soundTextY), "Sounds (Ctrl+S to hide): ", Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							soundTextY += yStep;
							GUI.DrawString(spriteBatch, new Vector2(500f, soundTextY), "Current playback amplitude: " + GameMain.SoundManager.PlaybackAmplitude.ToString(), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							soundTextY += yStep;
							GUI.DrawString(spriteBatch, new Vector2(500f, soundTextY), "Compressed dynamic range gain: " + GameMain.SoundManager.CompressionDynamicRangeGain.ToString(), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							soundTextY += yStep;
							GUI.DrawString(spriteBatch, new Vector2(500f, soundTextY), string.Concat(new string[]
							{
								"Loaded sounds: ",
								GameMain.SoundManager.LoadedSoundCount.ToString(),
								" (",
								GameMain.SoundManager.UniqueLoadedSoundCount.ToString(),
								" unique)"
							}), Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							soundTextY += yStep;
							for (int j = 0; j < 32; j++)
							{
								Color clr = Color.White;
								string soundStr = j.ToString() + ": ";
								SoundChannel playingSoundChannel = GameMain.SoundManager.GetSoundChannelFromIndex(SoundManager.SourcePoolIndex.Default, j);
								if (playingSoundChannel == null)
								{
									soundStr += "none";
									clr *= 0.5f;
								}
								else
								{
									soundStr += Path.GetFileNameWithoutExtension(playingSoundChannel.Sound.Filename);
									if (playingSoundChannel.Looping)
									{
										soundStr += " (looping)";
										clr = Color.Yellow;
									}
									if (playingSoundChannel.IsStream)
									{
										soundStr += " (streaming)";
										clr = Color.Lime;
									}
									if (!playingSoundChannel.IsPlaying)
									{
										soundStr += " (stopped)";
										clr *= 0.5f;
									}
									else
									{
										if (playingSoundChannel.Muffled)
										{
											soundStr += " (muffled)";
											clr = Color.Lerp(clr, Color.LightGray, 0.5f);
										}
										if (playingSoundChannel.FadingOutAndDisposing)
										{
											soundStr += ". Fading out...";
											clr = Color.Lerp(clr, Color.Black, 0.15f);
										}
									}
								}
								GUI.DrawString(spriteBatch, new Vector2(500f, soundTextY), soundStr, clr, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
								soundTextY += yStep;
							}
						}
						else
						{
							GUI.DrawString(spriteBatch, new Vector2(500f, 0f), "Ctrl+S to show sound debug info", Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						}
						y3 += 185f * GUI.yScale;
						if (GUI.debugDrawEvents)
						{
							GUI.DrawString(spriteBatch, new Vector2(10f, y3), "Ctrl+E to hide EventManager debug info", Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							GameSession gameSession = GameMain.GameSession;
							if (gameSession != null)
							{
								EventManager eventManager = gameSession.EventManager;
								if (eventManager != null)
								{
									eventManager.DebugDrawHUD(spriteBatch, y3 + 15f * GUI.yScale);
								}
							}
						}
						else
						{
							GUI.DrawString(spriteBatch, new Vector2(10f, y3), "Ctrl+E to show EventManager debug info", Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
						}
						GameSession gameSession2 = GameMain.GameSession;
						CampaignMode campaignMode = ((gameSession2 != null) ? gameSession2.GameMode : null) as CampaignMode;
						if (campaignMode != null)
						{
							if (GUI.debugDrawMetaData.Enabled)
							{
								string text = string.Concat(new string[]
								{
									"Ctrl+M to hide campaign metadata debug info\n\nCtrl+1 to ",
									GUI.debugDrawMetaData.FactionMetadata ? "hide" : "show",
									" faction reputations, \nCtrl+2 to ",
									GUI.debugDrawMetaData.UpgradeLevels ? "hide" : "show",
									" upgrade levels, \nCtrl+3 to ",
									GUI.debugDrawMetaData.UpgradePrices ? "hide" : "show",
									" upgrade prices"
								});
								Vector2 textSize = GUIStyle.SmallFont.MeasureString(text, false);
								Vector2 pos = new Vector2((float)GameMain.GraphicsWidth - (textSize.X + 10f), 300f);
								GUI.DrawString(spriteBatch, pos, text, Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
								pos.Y += textSize.Y + 8f;
								CampaignMetadata campaignMetadata = campaignMode.CampaignMetadata;
								if (campaignMetadata != null)
								{
									campaignMetadata.DebugDraw(spriteBatch, pos, campaignMode, GUI.debugDrawMetaData);
								}
							}
							else
							{
								GUI.DrawString(spriteBatch, new Vector2((float)GameMain.GraphicsWidth - (GUIStyle.SmallFont.MeasureString("Ctrl+M to show campaign metadata debug info", false).X + 10f), 300f), "Ctrl+M to show campaign metadata debug info", Color.White, new Color?(Color.Black * 0.5f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							}
						}
						IEnumerable<string> strings;
						if (GUI.MouseOn != null)
						{
							RectTransform mouseOnRect = GUI.MouseOn.RectTransform;
							bool isAbsoluteOffsetInUse = mouseOnRect.AbsoluteOffset != Point.Zero || mouseOnRect.RelativeOffset == Vector2.Zero;
							string[] array2 = new string[3];
							int num = 0;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(25, 3);
							defaultInterpolatedStringHandler3.AppendLiteral("Selected UI Element: ");
							defaultInterpolatedStringHandler3.AppendFormatted(GUI.MouseOn.GetType().Name);
							defaultInterpolatedStringHandler3.AppendLiteral(" (");
							GUIComponentStyle style = GUI.MouseOn.Style;
							defaultInterpolatedStringHandler3.AppendFormatted(((style != null) ? style.Element.Name.LocalName : null) ?? "no style");
							defaultInterpolatedStringHandler3.AppendLiteral(", ");
							defaultInterpolatedStringHandler3.AppendFormatted<Rectangle>(GUI.MouseOn.Rect);
							array2[num] = defaultInterpolatedStringHandler3.ToStringAndClear();
							int num2 = 1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(37, 3);
							defaultInterpolatedStringHandler4.AppendLiteral("Relative Offset: ");
							defaultInterpolatedStringHandler4.AppendFormatted<Vector2>(mouseOnRect.RelativeOffset);
							defaultInterpolatedStringHandler4.AppendLiteral(" | Absolute Offset: ");
							defaultInterpolatedStringHandler4.AppendFormatted<Point>(isAbsoluteOffsetInUse ? mouseOnRect.AbsoluteOffset : mouseOnRect.ParentRect.MultiplySize(mouseOnRect.RelativeOffset));
							defaultInterpolatedStringHandler4.AppendFormatted(isAbsoluteOffsetInUse ? "" : " (Calculated from RelativeOffset)");
							array2[num2] = defaultInterpolatedStringHandler4.ToStringAndClear();
							int num3 = 2;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(18, 2);
							defaultInterpolatedStringHandler5.AppendLiteral("Anchor: ");
							defaultInterpolatedStringHandler5.AppendFormatted<Anchor>(mouseOnRect.Anchor);
							defaultInterpolatedStringHandler5.AppendLiteral(" | Pivot: ");
							defaultInterpolatedStringHandler5.AppendFormatted<Pivot>(mouseOnRect.Pivot);
							array2[num3] = defaultInterpolatedStringHandler5.ToStringAndClear();
							strings = array2;
						}
						else
						{
							string[] array3 = new string[5];
							int num4 = 0;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(11, 1);
							defaultInterpolatedStringHandler6.AppendLiteral("GUI.Scale: ");
							defaultInterpolatedStringHandler6.AppendFormatted<float>(GUI.Scale);
							array3[num4] = defaultInterpolatedStringHandler6.ToStringAndClear();
							int num5 = 1;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(12, 1);
							defaultInterpolatedStringHandler7.AppendLiteral("GUI.xScale: ");
							defaultInterpolatedStringHandler7.AppendFormatted<float>(GUI.xScale);
							array3[num5] = defaultInterpolatedStringHandler7.ToStringAndClear();
							int num6 = 2;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(12, 1);
							defaultInterpolatedStringHandler8.AppendLiteral("GUI.yScale: ");
							defaultInterpolatedStringHandler8.AppendFormatted<float>(GUI.yScale);
							array3[num6] = defaultInterpolatedStringHandler8.ToStringAndClear();
							int num7 = 3;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(31, 1);
							defaultInterpolatedStringHandler9.AppendLiteral("RelativeHorizontalAspectRatio: ");
							defaultInterpolatedStringHandler9.AppendFormatted<float>(GUI.RelativeHorizontalAspectRatio);
							array3[num7] = defaultInterpolatedStringHandler9.ToStringAndClear();
							int num8 = 4;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler10 = new DefaultInterpolatedStringHandler(29, 1);
							defaultInterpolatedStringHandler10.AppendLiteral("RelativeVerticalAspectRatio: ");
							defaultInterpolatedStringHandler10.AppendFormatted<float>(GUI.RelativeVerticalAspectRatio);
							array3[num8] = defaultInterpolatedStringHandler10.ToStringAndClear();
							strings = array3;
						}
						IEnumerable<string> first = strings;
						string[] array4 = new string[1];
						int num9 = 0;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler11 = new DefaultInterpolatedStringHandler(10, 1);
						defaultInterpolatedStringHandler11.AppendLiteral("Cam.Zoom: ");
						Camera cam2 = Screen.Selected.Cam;
						defaultInterpolatedStringHandler11.AppendFormatted<float>((cam2 != null) ? cam2.Zoom : 0f);
						array4[num9] = defaultInterpolatedStringHandler11.ToStringAndClear();
						strings = first.Concat(array4);
						int padding = GUI.IntScale(10f);
						int yPos = padding;
						foreach (string str in strings)
						{
							Vector2 stringSize = GUIStyle.SmallFont.MeasureString(str, false);
							GUI.DrawString(spriteBatch, new Vector2((float)(GameMain.GraphicsWidth - (int)stringSize.X - padding), (float)yPos), str, Color.LightGreen, new Color?(Color.Black), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
							yPos += (int)stringSize.Y + padding / 2;
						}
					}
					GameSession gameSession3 = GameMain.GameSession;
					if (gameSession3 != null)
					{
						EventManager eventManager2 = gameSession3.EventManager;
						if (eventManager2 != null)
						{
							eventManager2.DrawPinnedEvent(spriteBatch);
						}
					}
					if (HUDLayoutSettings.DebugDraw)
					{
						HUDLayoutSettings.Draw(spriteBatch);
					}
					GameClient client = GameMain.Client;
					if (client != null)
					{
						client.Draw(spriteBatch);
					}
					Character controlled = Character.Controlled;
					if (((controlled != null) ? controlled.Inventory : null) != null && Character.Controlled.Stun < 0.1f && !Character.Controlled.IsDead)
					{
						Inventory.DrawFront(spriteBatch);
					}
					GUI.DrawMessages(spriteBatch, cam);
					if (GUI.MouseOn != null)
					{
						Action<GUIComponent> onDrawToolTip = GUI.MouseOn.OnDrawToolTip;
						if (onDrawToolTip != null)
						{
							onDrawToolTip(GUI.MouseOn);
						}
						if (!GUI.MouseOn.ToolTip.IsNullOrWhiteSpace())
						{
							GUI.MouseOn.DrawToolTip(spriteBatch);
						}
					}
					if (SubEditorScreen.IsSubEditor())
					{
						MapEntityPrefab draggedItemPrefab = SubEditorScreen.DraggedItemPrefab;
						ItemPrefab itemPrefab = draggedItemPrefab as ItemPrefab;
						if (itemPrefab == null)
						{
							ItemAssemblyPrefab itemAssemblyPrefab = draggedItemPrefab as ItemAssemblyPrefab;
							if (itemAssemblyPrefab != null)
							{
								itemAssemblyPrefab.Draw(spriteBatch, PlayerInput.MousePosition.FlipY());
							}
						}
						else
						{
							Sprite sprite = itemPrefab.InventoryIcon ?? itemPrefab.Sprite;
							if (sprite != null)
							{
								sprite.Draw(spriteBatch, PlayerInput.MousePosition, 0f, Math.Min(64f / sprite.size.X, 64f / sprite.size.Y) * GUI.Scale, SpriteEffects.None);
							}
						}
					}
					GUI.DrawSavingIndicator(spriteBatch);
					GUI.DrawCursor(spriteBatch);
					GUI.HideCursor = false;
				}
			}
		}

		// Token: 0x06001160 RID: 4448 RVA: 0x000ABE4C File Offset: 0x000AA04C
		public static void DrawMessageBoxesOnly(SpriteBatch spriteBatch)
		{
			bool anyDrawn = false;
			foreach (GUIComponent component in GUI.updateList)
			{
				component.DrawAuto(spriteBatch);
				anyDrawn = true;
			}
			if (anyDrawn)
			{
				GUI.DrawCursor(spriteBatch);
			}
		}

		// Token: 0x06001161 RID: 4449 RVA: 0x000ABEAC File Offset: 0x000AA0AC
		private static void DrawCursor(SpriteBatch spriteBatch)
		{
			if (GameMain.WindowActive && !GUI.HideCursor && GUI.MouseCursorSprites.Prefabs.Any<GUICursorPrefab>())
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerStateClamp, null, GameMain.ScissorTestEnable, null, null);
				GameSession gameSession = GameMain.GameSession;
				CrewManager crewManager = (gameSession != null) ? gameSession.CrewManager : null;
				if (crewManager != null)
				{
					OrderPrefab draggedOrderPrefab = crewManager.DraggedOrderPrefab;
					if (draggedOrderPrefab != null)
					{
						Sprite orderSprite = draggedOrderPrefab.SymbolSprite;
						if (orderSprite != null)
						{
							Color color = draggedOrderPrefab.Color;
							if (crewManager.DragOrder)
							{
								float spriteSize = Math.Max(orderSprite.size.X, orderSprite.size.Y);
								orderSprite.Draw(spriteBatch, PlayerInput.LatestMousePosition, color, orderSprite.size / 2f, 0f, 32f / spriteSize * GUI.Scale, SpriteEffects.None, null);
							}
						}
					}
				}
				Sprite sprite = GUI.MouseCursorSprites[GUI.MouseCursor] ?? GUI.MouseCursorSprites[CursorState.Default];
				sprite.Draw(spriteBatch, PlayerInput.LatestMousePosition, Color.White, sprite.Origin, 0f, GUI.Scale / 1.5f, SpriteEffects.None, null);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			}
		}

		// Token: 0x06001162 RID: 4450 RVA: 0x000AC014 File Offset: 0x000AA214
		public static void DrawBackgroundSprite(SpriteBatch spriteBatch, Sprite backgroundSprite, Color color, Rectangle? drawArea = null, SpriteEffects spriteEffects = SpriteEffects.None)
		{
			Rectangle area = drawArea ?? new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			float scale = Math.Max((float)area.Width / (float)backgroundSprite.SourceRect.Width, (float)area.Height / (float)backgroundSprite.SourceRect.Height) * 1.1f;
			float paddingX = (float)backgroundSprite.SourceRect.Width * scale - (float)area.Width;
			float paddingY = (float)backgroundSprite.SourceRect.Height * scale - (float)area.Height;
			double noiseT = Timing.TotalTime * 0.019999999552965164;
			Vector2 pos = new Vector2((float)PerlinNoise.CalculatePerlin(noiseT, noiseT, 0.0) - 0.5f, (float)PerlinNoise.CalculatePerlin(noiseT, noiseT, 0.5) - 0.5f);
			pos = new Vector2(pos.X * paddingX, pos.Y * paddingY);
			spriteBatch.Draw(backgroundSprite.Texture, area.Center.ToVector2() + pos, new Rectangle?(backgroundSprite.SourceRect), color, 0f, backgroundSprite.size / 2f, scale, spriteEffects, 0f);
		}

		// Token: 0x06001163 RID: 4451 RVA: 0x000AC15C File Offset: 0x000AA35C
		public static void AddToUpdateList(GUIComponent component)
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				if (component == null)
				{
					DebugConsole.ThrowError("Trying to add a null component on the GUI update list!", null, null, false, false);
				}
				else if (component.Visible)
				{
					if (component.UpdateOrder < 0)
					{
						GUI.firstAdditions.Add(component);
					}
					else if (component.UpdateOrder > 0)
					{
						GUI.lastAdditions.Add(component);
					}
					else
					{
						GUI.additions.Enqueue(component);
					}
				}
			}
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x000AC1EC File Offset: 0x000AA3EC
		public static void RemoveFromUpdateList(GUIComponent component, bool alsoChildren = true)
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				if (GUI.updateListSet.Contains(component))
				{
					GUI.removals.Enqueue(component);
				}
				if (alsoChildren)
				{
					if (component.RectTransform != null)
					{
						component.RectTransform.Children.ForEach(delegate(RectTransform c)
						{
							GUI.RemoveFromUpdateList(c.GUIComponent, true);
						});
					}
					else
					{
						component.Children.ForEach(delegate(GUIComponent c)
						{
							GUI.RemoveFromUpdateList(c, true);
						});
					}
				}
			}
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x000AC2A4 File Offset: 0x000AA4A4
		public static void ClearUpdateList()
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				if (GUI.KeyboardDispatcher.Subscriber is GUIComponent && !GUI.updateList.Contains(GUI.KeyboardDispatcher.Subscriber as GUIComponent))
				{
					GUI.KeyboardDispatcher.Subscriber = null;
				}
				GUI.updateList.Clear();
				GUI.updateListSet.Clear();
			}
		}

		// Token: 0x06001166 RID: 4454 RVA: 0x000AC328 File Offset: 0x000AA528
		private static void RefreshUpdateList()
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				foreach (GUIComponent component in GUI.updateList)
				{
					if (!component.Visible)
					{
						GUI.RemoveFromUpdateList(component, true);
					}
				}
				GUI.ProcessHelperList(GUI.firstAdditions);
				GUI.ProcessAdditions();
				GUI.ProcessHelperList(GUI.lastAdditions);
				GUI.ProcessRemovals();
			}
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x000AC3C8 File Offset: 0x000AA5C8
		private static void ProcessAdditions()
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				while (GUI.additions.Count > 0)
				{
					GUIComponent component = GUI.additions.Dequeue();
					if (!GUI.updateListSet.Contains(component))
					{
						GUI.updateList.Add(component);
						GUI.updateListSet.Add(component);
					}
				}
			}
		}

		// Token: 0x06001168 RID: 4456 RVA: 0x000AC440 File Offset: 0x000AA640
		private static void ProcessRemovals()
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				while (GUI.removals.Count > 0)
				{
					GUIComponent component = GUI.removals.Dequeue();
					GUI.updateList.Remove(component);
					GUI.updateListSet.Remove(component);
					if (component as IKeyboardSubscriber == GUI.KeyboardDispatcher.Subscriber)
					{
						GUI.KeyboardDispatcher.Subscriber = null;
					}
				}
			}
		}

		// Token: 0x06001169 RID: 4457 RVA: 0x000AC4C8 File Offset: 0x000AA6C8
		private static void ProcessHelperList(List<GUIComponent> list)
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				if (list.Count != 0)
				{
					foreach (GUIComponent item in list)
					{
						int index = 0;
						if (GUI.updateList.Count > 0)
						{
							index = GUI.updateList.Count;
							while (index > 0 && GUI.updateList[index - 1].UpdateOrder > item.UpdateOrder)
							{
								index--;
							}
						}
						if (!GUI.updateListSet.Contains(item))
						{
							GUI.updateList.Insert(index, item);
							GUI.updateListSet.Add(item);
						}
					}
					list.Clear();
				}
			}
		}

		// Token: 0x0600116A RID: 4458 RVA: 0x000AC5B4 File Offset: 0x000AA7B4
		private static void HandlePersistingElements(float deltaTime)
		{
			GUIMessageBox guimessageBox = GUIMessageBox.VisibleBox as GUIMessageBox;
			bool currentMessageBoxIsVerificationPrompt = guimessageBox != null && guimessageBox.DrawOnTop;
			if (!currentMessageBoxIsVerificationPrompt)
			{
				GUIMessageBox.AddActiveToGUIUpdateList();
			}
			if (GUI.SettingsMenuOpen)
			{
				GUI.SettingsMenuContainer.AddToGUIUpdateList(false, 0);
			}
			else if (GUI.PauseMenuOpen)
			{
				GUI.PauseMenu.AddToGUIUpdateList(false, 0);
			}
			foreach (GUIComponent openAccordion in GUIComponent.OpenAccordionPopups)
			{
				openAccordion.AddToGUIUpdateList(false, 1);
			}
			if (PlayerInput.PrimaryMouseButtonDown())
			{
				GUIComponent.OpenAccordionPopups.Clear();
			}
			SocialOverlay instance = SocialOverlay.Instance;
			if (instance != null)
			{
				instance.AddToGuiUpdateList();
			}
			GUIContextMenu.AddActiveToGUIUpdateList();
			if (currentMessageBoxIsVerificationPrompt)
			{
				GUIMessageBox.VisibleBox.AddToGUIUpdateList(false, 0);
			}
		}

		// Token: 0x0600116B RID: 4459 RVA: 0x000AC684 File Offset: 0x000AA884
		public static IEnumerable<GUIComponent> GetAdditions()
		{
			return GUI.additions.Union(GUI.firstAdditions).Union(GUI.lastAdditions);
		}

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x0600116C RID: 4460 RVA: 0x000AC69F File Offset: 0x000AA89F
		// (set) Token: 0x0600116D RID: 4461 RVA: 0x000AC6A6 File Offset: 0x000AA8A6
		public static GUIComponent MouseOn { get; private set; }

		// Token: 0x0600116E RID: 4462 RVA: 0x000AC6B0 File Offset: 0x000AA8B0
		public static bool IsMouseOn(GUIComponent target)
		{
			object obj = GUI.mutex;
			bool result;
			lock (obj)
			{
				if (target == null)
				{
					result = false;
				}
				else
				{
					result = (target == GUI.MouseOn || target.IsParentOf(GUI.MouseOn, true));
				}
			}
			return result;
		}

		// Token: 0x0600116F RID: 4463 RVA: 0x000AC70C File Offset: 0x000AA90C
		public static void ForceMouseOn(GUIComponent c)
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				GUI.MouseOn = c;
			}
		}

		// Token: 0x06001170 RID: 4464 RVA: 0x000AC74C File Offset: 0x000AA94C
		public static GUIComponent UpdateMouseOn()
		{
			object obj = GUI.mutex;
			GUIComponent mouseOn;
			lock (obj)
			{
				GUIComponent prevMouseOn = GUI.MouseOn;
				GUI.MouseOn = null;
				int inventoryIndex = -1;
				Inventory.RefreshMouseOnInventory();
				if (Inventory.IsMouseOnInventory)
				{
					inventoryIndex = GUI.updateList.IndexOf(CharacterHUD.HUDFrame);
				}
				if ((!PlayerInput.PrimaryMouseButtonHeld() && !PlayerInput.PrimaryMouseButtonClicked()) || (prevMouseOn == null && !PlayerInput.SecondaryMouseButtonHeld() && !Inventory.DraggingItems.Any<Item>()))
				{
					int i = GUI.updateList.Count - 1;
					while (i > inventoryIndex)
					{
						GUIComponent c = GUI.updateList[i];
						if (c.CanBeFocused && c.MouseRect.Contains(PlayerInput.MousePosition))
						{
							if ((!PlayerInput.PrimaryMouseButtonHeld() && !PlayerInput.PrimaryMouseButtonClicked()) || c == prevMouseOn || prevMouseOn == null)
							{
								GUI.MouseOn = c;
								break;
							}
							break;
						}
						else
						{
							i--;
						}
					}
				}
				else
				{
					GUI.MouseOn = prevMouseOn;
				}
				GUI.MouseCursor = GUI.UpdateMouseCursorState(GUI.MouseOn);
				mouseOn = GUI.MouseOn;
			}
			return mouseOn;
		}

		// Token: 0x06001171 RID: 4465 RVA: 0x000AC85C File Offset: 0x000AAA5C
		private static CursorState UpdateMouseCursorState(GUIComponent c)
		{
			object obj = GUI.mutex;
			CursorState result;
			lock (obj)
			{
				if (GUI.MouseCursor == CursorState.Waiting)
				{
					result = CursorState.Waiting;
				}
				else if (GUIScrollBar.DraggingBar != null)
				{
					result = GUIScrollBar.DraggingBar.Bar.HoverCursor;
				}
				else if (SubEditorScreen.IsSubEditor() && SubEditorScreen.DraggedItemPrefab != null)
				{
					result = CursorState.Hand;
				}
				else
				{
					if (Character.Controlled != null)
					{
						Item selectedItem = Character.Controlled.SelectedItem;
						if (((selectedItem != null) ? selectedItem.GetComponent<ConnectionPanel>() : null) != null)
						{
							if (Connection.DraggingConnected != null)
							{
								return CursorState.Dragging;
							}
							if (ConnectionPanel.HighlightedWire != null)
							{
								return CursorState.Hand;
							}
						}
						if (Wire.DraggingWire != null)
						{
							return CursorState.Dragging;
						}
					}
					if (c == null || c is GUICustomComponent)
					{
						Screen selected = Screen.Selected;
						CharacterEditorScreen editor = selected as CharacterEditorScreen;
						if (editor != null)
						{
							return editor.GetMouseCursorState();
						}
						if (!(selected is GameScreen))
						{
							if (selected is SubEditorScreen)
							{
								if (MapEntity.StartMovingPos != Vector2.Zero || MapEntity.Resizing)
								{
									return CursorState.Dragging;
								}
								if (MapEntity.HighlightedEntities.Any((MapEntity h) => !h.IsSelected))
								{
									return CursorState.Hand;
								}
							}
						}
						else
						{
							Character controlled = Character.Controlled;
							if (controlled != null && !controlled.ShouldLockHud() && (CharacterHUD.MouseOnCharacterPortrait() || CharacterHealth.IsMouseOnHealthBar()))
							{
								return CursorState.Hand;
							}
						}
					}
					if (c != null && c.Visible)
					{
						if (c.AlwaysOverrideCursor)
						{
							return c.HoverCursor;
						}
						Rectangle monitorRect = new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
						GUIComponent parent = GUI.<UpdateMouseCursorState>g__FindInteractParent|126_0(c);
						if (c.Enabled)
						{
							GUIDragHandle dragHandle = (c as GUIDragHandle) ?? (parent as GUIDragHandle);
							if (dragHandle != null)
							{
								return dragHandle.Dragging ? CursorState.Dragging : CursorState.Hand;
							}
							if (c is GUIListBox && (parent == null || parent == c))
							{
								return CursorState.Default;
							}
							if (((!(parent is GUIButton) && !(parent is GUIListBox)) || c is GUIButton || c is GUITickBox) && !c.Rect.Equals(monitorRect))
							{
								if (!(c is GUITickBox))
								{
									return c.HoverCursor;
								}
								GUIComponent.ComponentState state = c.State;
								bool flag2 = state == GUIComponent.ComponentState.Hover || state == GUIComponent.ComponentState.HoverSelected;
								if (flag2)
								{
									return c.HoverCursor;
								}
							}
						}
						GUIListBox listBox = parent as GUIListBox;
						if (listBox != null && c.Parent == listBox.Content)
						{
							if (listBox.DraggedElement != null)
							{
								return CursorState.Dragging;
							}
							if (listBox.CurrentDragMode != GUIListBox.DragMode.NoDragging)
							{
								return CursorState.Move;
							}
							if (listBox.HoverCursor != CursorState.Default)
							{
								GUIComponent hoverParent = c;
								while (hoverParent != parent && hoverParent != null)
								{
									if (hoverParent.State == GUIComponent.ComponentState.Hover)
									{
										return CursorState.Hand;
									}
									hoverParent = hoverParent.Parent;
								}
							}
						}
						if (parent != null && parent.CanBeFocused && !parent.Rect.Equals(monitorRect))
						{
							return parent.HoverCursor;
						}
					}
					if (Inventory.IsMouseOnInventory)
					{
						result = Inventory.GetInventoryMouseCursor();
					}
					else
					{
						Character character = Character.Controlled;
						if (character != null)
						{
							if (character.CharacterHealth.MouseOnElement)
							{
								return CursorState.Hand;
							}
							if (character.SelectedCharacter != null && character.SelectedCharacter.CharacterHealth.MouseOnElement)
							{
								return CursorState.Hand;
							}
							if (character.FocusedItem != null)
							{
								return CursorState.Hand;
							}
						}
						result = CursorState.Default;
					}
				}
			}
			return result;
		}

		// Token: 0x06001172 RID: 4466 RVA: 0x000ACBDC File Offset: 0x000AADDC
		public static void SetCursorWaiting(int waitSeconds = 10, Func<bool> endCondition = null)
		{
			GUI.<>c__DisplayClass127_0 CS$<>8__locals1 = new GUI.<>c__DisplayClass127_0();
			CS$<>8__locals1.waitSeconds = waitSeconds;
			CS$<>8__locals1.endCondition = endCondition;
			CoroutineManager.StartCoroutine(CS$<>8__locals1.<SetCursorWaiting>g__WaitCursorCoroutine|0(), "WaitCursorTimeout");
		}

		// Token: 0x06001173 RID: 4467 RVA: 0x000ACC10 File Offset: 0x000AAE10
		public static void ClearCursorWait()
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				CoroutineManager.StopCoroutines("WaitCursorTimeout");
				GUI.MouseCursor = CursorState.Default;
			}
		}

		// Token: 0x06001174 RID: 4468 RVA: 0x000ACC5C File Offset: 0x000AAE5C
		public static bool HasSizeChanged(Point referenceResolution, float referenceUIScale, float referenceHUDScale)
		{
			return GameMain.GraphicsWidth != referenceResolution.X || GameMain.GraphicsHeight != referenceResolution.Y || referenceUIScale != Inventory.UIScale || referenceHUDScale != GUI.Scale;
		}

		// Token: 0x06001175 RID: 4469 RVA: 0x000ACC90 File Offset: 0x000AAE90
		public static void Update(float deltaTime)
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				if (PlayerInput.KeyDown(Keys.LeftControl) && PlayerInput.KeyHit(Keys.S))
				{
					GUI.debugDrawSounds = !GUI.debugDrawSounds;
				}
				if (PlayerInput.KeyDown(Keys.LeftControl) && PlayerInput.KeyHit(Keys.E))
				{
					GUI.debugDrawEvents = !GUI.debugDrawEvents;
				}
				if (PlayerInput.IsCtrlDown() && PlayerInput.KeyHit(Keys.M))
				{
					GUI.debugDrawMetaData.Enabled = !GUI.debugDrawMetaData.Enabled;
				}
				if (GUI.debugDrawMetaData.Enabled)
				{
					if (PlayerInput.KeyHit(Keys.Up))
					{
						GUI.debugDrawMetaData.Offset = GUI.debugDrawMetaData.Offset - 1;
					}
					if (PlayerInput.KeyHit(Keys.Down))
					{
						GUI.debugDrawMetaData.Offset = GUI.debugDrawMetaData.Offset + 1;
					}
					if (PlayerInput.IsCtrlDown())
					{
						if (PlayerInput.KeyHit(Keys.D1))
						{
							GUI.debugDrawMetaData.FactionMetadata = !GUI.debugDrawMetaData.FactionMetadata;
							GUI.debugDrawMetaData.Offset = 0;
						}
						if (PlayerInput.KeyHit(Keys.D2))
						{
							GUI.debugDrawMetaData.UpgradeLevels = !GUI.debugDrawMetaData.UpgradeLevels;
							GUI.debugDrawMetaData.Offset = 0;
						}
						if (PlayerInput.KeyHit(Keys.D3))
						{
							GUI.debugDrawMetaData.UpgradePrices = !GUI.debugDrawMetaData.UpgradePrices;
							GUI.debugDrawMetaData.Offset = 0;
						}
					}
				}
				GUI.HandlePersistingElements(deltaTime);
				GUI.RefreshUpdateList();
				GUI.UpdateMouseOn();
				foreach (GUIComponent c in GUI.updateList)
				{
					c.UpdateAuto(deltaTime);
				}
				GUI.UpdateMessages(deltaTime);
				GUI.UpdateSavingIndicator(deltaTime);
			}
		}

		// Token: 0x06001176 RID: 4470 RVA: 0x000ACE74 File Offset: 0x000AB074
		public static void UpdateGUIMessageBoxesOnly(float deltaTime)
		{
			GUIMessageBox.AddActiveToGUIUpdateList();
			GUI.RefreshUpdateList();
			GUI.UpdateMouseOn();
			foreach (GUIComponent c in GUI.updateList)
			{
				c.UpdateAuto(deltaTime);
			}
		}

		// Token: 0x06001177 RID: 4471 RVA: 0x000ACED8 File Offset: 0x000AB0D8
		private static void UpdateMessages(float deltaTime)
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				foreach (GUIMessage msg in GUI.messages)
				{
					if (!msg.WorldSpace)
					{
						msg.Timer -= deltaTime;
						if (msg.Size.X > (float)HUDLayoutSettings.MessageAreaTop.Width)
						{
							msg.Pos = Vector2.Lerp(Vector2.Zero, new Vector2((float)(-(float)HUDLayoutSettings.MessageAreaTop.Width) - msg.Size.X, 0f), 1f - msg.Timer / msg.LifeTime);
							break;
						}
						if (msg.Timer > 1f)
						{
							msg.Pos = Vector2.Lerp(msg.Pos, new Vector2((float)(-(float)HUDLayoutSettings.MessageAreaTop.Width / 2) - msg.Size.X / 2f, 0f), Math.Min(deltaTime * 10f, 1f));
							break;
						}
						msg.Pos = Vector2.Lerp(msg.Pos, new Vector2((float)(-(float)HUDLayoutSettings.MessageAreaTop.Width) - msg.Size.X, 0f), deltaTime * 10f);
						break;
					}
				}
				foreach (GUIMessage msg2 in GUI.messages)
				{
					if (msg2.WorldSpace)
					{
						msg2.Timer -= deltaTime;
						msg2.Pos += msg2.Velocity * deltaTime;
					}
				}
				GUI.messages.RemoveAll((GUIMessage m) => m.Timer <= 0f);
			}
		}

		// Token: 0x06001178 RID: 4472 RVA: 0x000AD124 File Offset: 0x000AB324
		private static void UpdateSavingIndicator(float deltaTime)
		{
			if (GUIStyle.SavingIndicator == null)
			{
				return;
			}
			object obj = GUI.mutex;
			lock (obj)
			{
				if (GUI.timeUntilSavingIndicatorDisabled != null)
				{
					GUI.timeUntilSavingIndicatorDisabled -= deltaTime;
					float? num = GUI.timeUntilSavingIndicatorDisabled;
					float num2 = 0f;
					if (num.GetValueOrDefault() <= num2 & num != null)
					{
						GUI.isSavingIndicatorEnabled = false;
						GUI.timeUntilSavingIndicatorDisabled = null;
					}
				}
				if (GUI.isSavingIndicatorEnabled)
				{
					if (GUI.savingIndicatorColor == Color.Transparent)
					{
						GUI.savingIndicatorState = GUI.SavingIndicatorState.FadingIn;
						GUI.savingIndicatorColorLerpAmount = 0f;
					}
					else if (GUI.savingIndicatorColor == Color.White)
					{
						GUI.savingIndicatorState = GUI.SavingIndicatorState.None;
					}
				}
				else if (GUI.savingIndicatorColor == Color.White)
				{
					GUI.savingIndicatorState = GUI.SavingIndicatorState.FadingOut;
					GUI.savingIndicatorColorLerpAmount = 0f;
				}
				else if (GUI.savingIndicatorColor == Color.Transparent)
				{
					GUI.savingIndicatorState = GUI.SavingIndicatorState.None;
				}
				if (GUI.savingIndicatorState != GUI.SavingIndicatorState.None)
				{
					bool isFadingIn = GUI.savingIndicatorState == GUI.SavingIndicatorState.FadingIn;
					Color lerpStartColor = isFadingIn ? Color.Transparent : Color.White;
					Color lerpTargetColor = isFadingIn ? Color.White : Color.Transparent;
					GUI.savingIndicatorColorLerpAmount += (isFadingIn ? 2f : 0.5f) * deltaTime;
					GUI.savingIndicatorColor = Color.Lerp(lerpStartColor, lerpTargetColor, GUI.savingIndicatorColorLerpAmount);
				}
				if (GUI.IsSavingIndicatorVisible)
				{
					GUI.savingIndicatorSpriteIndex = (GUI.savingIndicatorSpriteIndex + 15f * deltaTime) % (float)(GUIStyle.SavingIndicator.FrameCount + 1);
				}
			}
		}

		// Token: 0x06001179 RID: 4473 RVA: 0x000AD2EC File Offset: 0x000AB4EC
		public static void DrawIndicator(SpriteBatch spriteBatch, in Vector2 worldPosition, Camera cam, in Range<float> visibleRange, Sprite sprite, in Color color, bool createOffset = true, float scaleMultiplier = 1f, float? overrideAlpha = null, LocalizedString label = null)
		{
			Vector2 diff = worldPosition - cam.WorldViewCenter;
			float dist = diff.Length();
			float symbolScale = Math.Min(64f / sprite.size.X, 1f) * scaleMultiplier * GUI.Scale;
			if (overrideAlpha != null || visibleRange.Contains(dist))
			{
				float? num = overrideAlpha;
				float num7;
				if (num == null)
				{
					float[] array = new float[3];
					int num2 = 0;
					float num3 = dist;
					Range<float> range = visibleRange;
					array[num2] = (num3 - range.Start) / 100f;
					int num4 = 1;
					float num5 = 1f;
					float num6 = dist;
					range = visibleRange;
					array[num4] = num5 - (num6 - range.End + 100f) / 100f;
					array[2] = 1f;
					num7 = MathUtils.Min(array);
				}
				else
				{
					num7 = num.GetValueOrDefault();
				}
				float alpha = num7;
				Vector2 targetScreenPos = cam.WorldToScreen(worldPosition);
				if (!createOffset)
				{
					sprite.Draw(spriteBatch, targetScreenPos, color * alpha, 0f, symbolScale, SpriteEffects.None, null);
					return;
				}
				float screenDist = Vector2.Distance(cam.WorldToScreen(cam.WorldViewCenter), targetScreenPos);
				float angle = MathUtils.VectorToAngle(diff);
				float originalAngle = angle;
				bool overlapFound = true;
				int iterations = 0;
				while (overlapFound && iterations < 10)
				{
					overlapFound = false;
					foreach (float usedIndicatorAngle in GUI.usedIndicatorAngles)
					{
						float shortestAngle = MathUtils.GetShortestAngle(angle, usedIndicatorAngle);
						if (MathUtils.NearlyEqual(shortestAngle, 0f, 0.0001f))
						{
							shortestAngle = 0.01f;
						}
						if (Math.Abs(shortestAngle) < 0.05f)
						{
							angle -= (float)Math.Sign(shortestAngle) * (0.05f - Math.Abs(shortestAngle));
							overlapFound = true;
							break;
						}
					}
					iterations++;
				}
				GUI.usedIndicatorAngles.Add(angle);
				Vector2 iconDiff = new Vector2((float)Math.Cos((double)angle) * Math.Min((float)GameMain.GraphicsWidth * 0.4f, screenDist + 10f), (float)(-(float)Math.Sin((double)angle)) * Math.Min((float)GameMain.GraphicsHeight * 0.4f, screenDist + 10f));
				angle = MathHelper.Lerp(originalAngle, angle, MathHelper.Clamp((screenDist + 10f - iconDiff.Length()) / 10f, 0f, 1f));
				iconDiff = new Vector2((float)Math.Cos((double)angle) * Math.Min((float)GameMain.GraphicsWidth * 0.4f, screenDist), (float)(-(float)Math.Sin((double)angle)) * Math.Min((float)GameMain.GraphicsHeight * 0.4f, screenDist));
				Vector2 iconPos = cam.WorldToScreen(cam.WorldViewCenter) + iconDiff;
				sprite.Draw(spriteBatch, iconPos, color * alpha, 0f, symbolScale, SpriteEffects.None, null);
				if (label != null)
				{
					float cursorDist = Vector2.Distance(PlayerInput.MousePosition, iconPos);
					if (cursorDist < sprite.size.X * symbolScale)
					{
						Vector2 textSize = GUIStyle.Font.MeasureString(label, false);
						Vector2 textPos = iconPos + new Vector2(sprite.size.X * symbolScale * 0.7f * (float)Math.Sign(-iconDiff.X), -textSize.Y / 2f);
						if (iconDiff.X > 0f)
						{
							textPos.X -= textSize.X;
						}
						GUI.DrawString(spriteBatch, textPos + Vector2.One, label, Color.Black, null, 0, null, ForceUpperCase.Inherit);
						GUI.DrawString(spriteBatch, textPos, label, color, null, 0, null, ForceUpperCase.Inherit);
					}
				}
				if (screenDist - 10f > iconDiff.Length())
				{
					Vector2 normalizedDiff = Vector2.Normalize(targetScreenPos - iconPos);
					Vector2 arrowOffset = normalizedDiff * sprite.size.X * symbolScale * 0.7f;
					GUI.Arrow.Draw(spriteBatch, iconPos + arrowOffset, color * alpha, MathUtils.VectorToAngle(arrowOffset) + 1.5707964f, 0.5f, SpriteEffects.None, null);
				}
			}
		}

		// Token: 0x0600117A RID: 4474 RVA: 0x000AD738 File Offset: 0x000AB938
		public static void DrawIndicator(SpriteBatch spriteBatch, Vector2 worldPosition, Camera cam, float hideDist, Sprite sprite, Color color, bool createOffset = true, float scaleMultiplier = 1f, float? overrideAlpha = null)
		{
			Range<float> range = new Range<float>(hideDist, float.PositiveInfinity);
			GUI.DrawIndicator(spriteBatch, worldPosition, cam, range, sprite, color, createOffset, scaleMultiplier, overrideAlpha, null);
		}

		// Token: 0x0600117B RID: 4475 RVA: 0x000AD767 File Offset: 0x000AB967
		public static void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color clr, float depth = 0f, float width = 1f)
		{
			GUI.DrawLine(sb, GUI.solidWhiteTexture, start, end, clr, depth, (int)width);
		}

		// Token: 0x0600117C RID: 4476 RVA: 0x000AD77C File Offset: 0x000AB97C
		public static void DrawLine(SpriteBatch sb, Sprite sprite, Vector2 start, Vector2 end, Color clr, float depth = 0f, int width = 1)
		{
			Vector2 edge = end - start;
			float angle = (float)Math.Atan2((double)edge.Y, (double)edge.X);
			sb.Draw(sprite.Texture, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), width), new Rectangle?(sprite.SourceRect), clr, angle, new Vector2(0f, (float)(sprite.SourceRect.Height / 2)), SpriteEffects.None, depth);
		}

		// Token: 0x0600117D RID: 4477 RVA: 0x000AD7F8 File Offset: 0x000AB9F8
		public static void DrawLine(SpriteBatch sb, Texture2D texture, Vector2 start, Vector2 end, Color clr, float depth = 0f, int width = 1)
		{
			Vector2 edge = end - start;
			float angle = (float)Math.Atan2((double)edge.Y, (double)edge.X);
			sb.Draw(texture, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), width), null, clr, angle, new Vector2(0f, (float)texture.Height / 2f), SpriteEffects.None, depth);
		}

		// Token: 0x0600117E RID: 4478 RVA: 0x000AD86C File Offset: 0x000ABA6C
		public static void DrawString(SpriteBatch sb, Vector2 pos, LocalizedString text, Color color, Color? backgroundColor = null, int backgroundPadding = 0, GUIFont font = null, ForceUpperCase forceUpperCase = ForceUpperCase.Inherit)
		{
			GUI.DrawString(sb, pos, text.Value, color, backgroundColor, backgroundPadding, font, forceUpperCase);
		}

		// Token: 0x0600117F RID: 4479 RVA: 0x000AD884 File Offset: 0x000ABA84
		public static void DrawString(SpriteBatch sb, Vector2 pos, string text, Color color, Color? backgroundColor = null, int backgroundPadding = 0, GUIFont font = null, ForceUpperCase forceUpperCase = ForceUpperCase.Inherit)
		{
			if (color.A == 0)
			{
				return;
			}
			if (font == null)
			{
				font = GUIStyle.Font;
			}
			if (backgroundColor != null && backgroundColor.Value.A > 0)
			{
				Vector2 textSize = font.MeasureString(text, false);
				GUI.DrawRectangle(sb, pos - Vector2.One * (float)backgroundPadding, textSize + Vector2.One * 2f * (float)backgroundPadding, backgroundColor.Value, true, 0f, 1f);
			}
			font.DrawString(sb, text, pos, color, forceUpperCase, false);
		}

		// Token: 0x06001180 RID: 4480 RVA: 0x000AD928 File Offset: 0x000ABB28
		public static void DrawStringWithColors(SpriteBatch sb, Vector2 pos, string text, Color color, in ImmutableArray<RichTextData>? richTextData, Color? backgroundColor = null, int backgroundPadding = 0, GUIFont font = null, float depth = 0f)
		{
			if (font == null)
			{
				font = GUIStyle.Font;
			}
			if (backgroundColor != null)
			{
				Vector2 textSize = font.MeasureString(text, false);
				GUI.DrawRectangle(sb, pos - Vector2.One * (float)backgroundPadding, textSize + Vector2.One * 2f * (float)backgroundPadding, backgroundColor.Value, true, depth, 5f);
			}
			font.DrawStringWithColors(sb, text, pos, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth, richTextData, 0, Alignment.TopLeft, ForceUpperCase.Inherit);
		}

		// Token: 0x06001181 RID: 4481 RVA: 0x000AD9C0 File Offset: 0x000ABBC0
		public static void DrawDonutSection(SpriteBatch sb, Vector2 center, Range<float> radii, float sectionRad, Color clr, float depth = 0f, float rotationRad = 0f)
		{
			GUI.<>c__DisplayClass146_0 CS$<>8__locals1;
			CS$<>8__locals1.radii = radii;
			CS$<>8__locals1.sectionProportion = sectionRad / 6.2831855f;
			CS$<>8__locals1.maxDirectionIndex = Math.Min(30, (int)MathF.Ceiling(CS$<>8__locals1.sectionProportion * 30f));
			for (int vertexIndex = 0; vertexIndex < CS$<>8__locals1.maxDirectionIndex * 4; vertexIndex++)
			{
				GUI.donutVerts[vertexIndex].Color = clr;
				GUI.donutVerts[vertexIndex].Position = new Vector3(center + Vector2.Transform(GUI.<DrawDonutSection>g__getDirection|146_2(vertexIndex, ref CS$<>8__locals1) * GUI.<DrawDonutSection>g__getRadius|146_0(vertexIndex, ref CS$<>8__locals1), Matrix.CreateRotationZ(rotationRad)), 0f);
			}
			sb.Draw(GUI.solidWhiteTexture, GUI.donutVerts, depth, new int?(CS$<>8__locals1.maxDirectionIndex));
		}

		// Token: 0x06001182 RID: 4482 RVA: 0x000ADA88 File Offset: 0x000ABC88
		public static void DrawRectangle(SpriteBatch sb, Vector2 start, Vector2 size, Color clr, bool isFilled = false, float depth = 0f, float thickness = 1f)
		{
			if (size.X < 0f)
			{
				start.X += size.X;
				size.X = -size.X;
			}
			if (size.Y < 0f)
			{
				start.Y += size.Y;
				size.Y = -size.Y;
			}
			GUI.DrawRectangle(sb, new Rectangle((int)start.X, (int)start.Y, (int)size.X, (int)size.Y), clr, isFilled, depth, thickness);
		}

		// Token: 0x06001183 RID: 4483 RVA: 0x000ADB1C File Offset: 0x000ABD1C
		public static void DrawRectangle(SpriteBatch sb, Rectangle rect, Color clr, bool isFilled = false, float depth = 0f, float thickness = 1f)
		{
			if (isFilled)
			{
				sb.Draw(GUI.solidWhiteTexture, rect, null, clr, 0f, Vector2.Zero, SpriteEffects.None, depth);
				return;
			}
			Rectangle srcRect = new Rectangle(0, 0, 1, 1);
			sb.Draw(GUI.solidWhiteTexture, new Vector2((float)rect.X, (float)rect.Y), new Rectangle?(srcRect), clr, 0f, Vector2.Zero, new Vector2(thickness, (float)rect.Height), SpriteEffects.None, depth);
			sb.Draw(GUI.solidWhiteTexture, new Vector2((float)rect.X + thickness, (float)rect.Y), new Rectangle?(srcRect), clr, 0f, Vector2.Zero, new Vector2((float)rect.Width - thickness, thickness), SpriteEffects.None, depth);
			sb.Draw(GUI.solidWhiteTexture, new Vector2((float)rect.X + thickness, (float)rect.Bottom - thickness), new Rectangle?(srcRect), clr, 0f, Vector2.Zero, new Vector2((float)rect.Width - thickness, thickness), SpriteEffects.None, depth);
			sb.Draw(GUI.solidWhiteTexture, new Vector2((float)rect.Right - thickness, (float)rect.Y + thickness), new Rectangle?(srcRect), clr, 0f, Vector2.Zero, new Vector2(thickness, (float)rect.Height - thickness * 2f), SpriteEffects.None, depth);
		}

		// Token: 0x06001184 RID: 4484 RVA: 0x000ADC7C File Offset: 0x000ABE7C
		public static void DrawRectangle(SpriteBatch sb, Vector2 position, Vector2 size, Vector2 origin, float rotation, Color clr, float depth = 0f, float thickness = 1f, GUI.OutlinePosition outlinePos = GUI.OutlinePosition.Centered)
		{
			Vector2 topLeft = new Vector2(-origin.X, -origin.Y);
			Vector2 topRight = new Vector2(-origin.X + size.X, -origin.Y);
			Vector2 bottomLeft = new Vector2(-origin.X, -origin.Y + size.Y);
			Vector2 actualSize = size;
			switch (outlinePos)
			{
			case GUI.OutlinePosition.Default:
				actualSize += new Vector2(thickness);
				break;
			case GUI.OutlinePosition.Inside:
				topRight -= new Vector2(thickness, 0f);
				bottomLeft -= new Vector2(0f, thickness);
				break;
			case GUI.OutlinePosition.Centered:
				topLeft -= new Vector2(thickness * 0.5f);
				topRight -= new Vector2(thickness * 0.5f);
				bottomLeft -= new Vector2(thickness * 0.5f);
				actualSize += new Vector2(thickness);
				break;
			case GUI.OutlinePosition.Outside:
				topLeft -= new Vector2(thickness);
				topRight -= new Vector2(0f, thickness);
				bottomLeft -= new Vector2(thickness, 0f);
				actualSize += new Vector2(thickness * 2f);
				break;
			}
			Matrix rotate = Matrix.CreateRotationZ(rotation);
			topLeft = Vector2.Transform(topLeft, rotate) + position;
			topRight = Vector2.Transform(topRight, rotate) + position;
			bottomLeft = Vector2.Transform(bottomLeft, rotate) + position;
			Rectangle srcRect = new Rectangle(0, 0, 1, 1);
			sb.Draw(GUI.solidWhiteTexture, topLeft, new Rectangle?(srcRect), clr, rotation, Vector2.Zero, new Vector2(thickness, actualSize.Y), SpriteEffects.None, depth);
			sb.Draw(GUI.solidWhiteTexture, topLeft, new Rectangle?(srcRect), clr, rotation, Vector2.Zero, new Vector2(actualSize.X, thickness), SpriteEffects.None, depth);
			sb.Draw(GUI.solidWhiteTexture, topRight, new Rectangle?(srcRect), clr, rotation, Vector2.Zero, new Vector2(thickness, actualSize.Y), SpriteEffects.None, depth);
			sb.Draw(GUI.solidWhiteTexture, bottomLeft, new Rectangle?(srcRect), clr, rotation, Vector2.Zero, new Vector2(actualSize.X, thickness), SpriteEffects.None, depth);
		}

		// Token: 0x06001185 RID: 4485 RVA: 0x000ADEB4 File Offset: 0x000AC0B4
		public static void DrawFilledRectangle(SpriteBatch sb, Vector2 position, Vector2 size, Vector2 pivot, float rotation, Color clr, float depth = 0f)
		{
			Rectangle srcRect = new Rectangle(0, 0, 1, 1);
			sb.Draw(GUI.solidWhiteTexture, position, new Rectangle?(srcRect), clr, rotation, pivot / size, size, SpriteEffects.None, depth);
		}

		// Token: 0x06001186 RID: 4486 RVA: 0x000ADEED File Offset: 0x000AC0ED
		public static void DrawFilledRectangle(SpriteBatch sb, RectangleF rect, Color clr, float depth = 0f)
		{
			GUI.DrawFilledRectangle(sb, rect.Location, rect.Size, clr, depth);
		}

		// Token: 0x06001187 RID: 4487 RVA: 0x000ADF08 File Offset: 0x000AC108
		public static void DrawFilledRectangle(SpriteBatch sb, Vector2 start, Vector2 size, Color clr, float depth = 0f)
		{
			if (size.X < 0f)
			{
				start.X += size.X;
				size.X = -size.X;
			}
			if (size.Y < 0f)
			{
				start.Y += size.Y;
				size.Y = -size.Y;
			}
			sb.Draw(GUI.solidWhiteTexture, start, null, clr, 0f, Vector2.Zero, size, SpriteEffects.None, depth);
		}

		// Token: 0x06001188 RID: 4488 RVA: 0x000ADF94 File Offset: 0x000AC194
		public static void DrawRectangle(SpriteBatch sb, Vector2 center, float width, float height, float rotation, Color clr, float depth = 0f, float thickness = 1f)
		{
			Matrix rotate = Matrix.CreateRotationZ(rotation);
			width *= 0.5f;
			height *= 0.5f;
			Vector2 topLeft = center + Vector2.Transform(new Vector2(-width, -height), rotate);
			Vector2 topRight = center + Vector2.Transform(new Vector2(width, -height), rotate);
			Vector2 bottomLeft = center + Vector2.Transform(new Vector2(-width, height), rotate);
			Vector2 bottomRight = center + Vector2.Transform(new Vector2(width, height), rotate);
			GUI.DrawLine(sb, topLeft, topRight, clr, depth, thickness);
			GUI.DrawLine(sb, topRight, bottomRight, clr, depth, thickness);
			GUI.DrawLine(sb, bottomRight, bottomLeft, clr, depth, thickness);
			GUI.DrawLine(sb, bottomLeft, topLeft, clr, depth, thickness);
		}

		// Token: 0x06001189 RID: 4489 RVA: 0x000AE04C File Offset: 0x000AC24C
		public static void DrawRectangle(SpriteBatch sb, Vector2[] corners, Color clr, float depth = 0f, float thickness = 1f)
		{
			if (corners.Length != 4)
			{
				throw new Exception("Invalid length of the corners array! Must be 4");
			}
			GUI.DrawLine(sb, corners[0], corners[1], clr, depth, thickness);
			GUI.DrawLine(sb, corners[1], corners[2], clr, depth, thickness);
			GUI.DrawLine(sb, corners[2], corners[3], clr, depth, thickness);
			GUI.DrawLine(sb, corners[3], corners[0], clr, depth, thickness);
		}

		// Token: 0x0600118A RID: 4490 RVA: 0x000AE0CC File Offset: 0x000AC2CC
		public static void DrawProgressBar(SpriteBatch sb, Vector2 start, Vector2 size, float progress, Color clr, float depth = 0f)
		{
			GUI.DrawProgressBar(sb, start, size, progress, clr, new Color(0.5f, 0.57f, 0.6f, 1f), depth);
		}

		// Token: 0x0600118B RID: 4491 RVA: 0x000AE100 File Offset: 0x000AC300
		public static void DrawProgressBar(SpriteBatch sb, Vector2 start, Vector2 size, float progress, Color clr, Color outlineColor, float depth = 0f)
		{
			GUI.DrawRectangle(sb, new Vector2(start.X, -start.Y), size, outlineColor, false, depth, 1f);
			int padding = 2;
			GUI.DrawRectangle(sb, new Rectangle((int)start.X + padding, -(int)(start.Y - (float)padding), (int)((size.X - (float)(padding * 2)) * progress), (int)size.Y - padding * 2), clr, true, depth, 1f);
		}

		// Token: 0x0600118C RID: 4492 RVA: 0x000AE174 File Offset: 0x000AC374
		public static bool DrawButton(SpriteBatch sb, Rectangle rect, string text, Color color, bool isHoldable = false)
		{
			bool clicked = false;
			if (rect.Contains(PlayerInput.MousePosition))
			{
				clicked = PlayerInput.PrimaryMouseButtonHeld();
				color = (clicked ? new Color((int)((float)color.R * 0.8f), (int)((float)color.G * 0.8f), (int)((float)color.B * 0.8f), (int)color.A) : new Color((int)((float)color.R * 1.2f), (int)((float)color.G * 1.2f), (int)((float)color.B * 1.2f), (int)color.A));
				if (!isHoldable)
				{
					clicked = PlayerInput.PrimaryMouseButtonClicked();
				}
			}
			GUI.DrawRectangle(sb, rect, color, true, 0f, 1f);
			Vector2 origin;
			try
			{
				origin = GUIStyle.Font.MeasureString(text, false) / 2f;
			}
			catch
			{
				origin = Vector2.Zero;
			}
			GUIStyle.Font.DrawString(sb, text, new Vector2((float)rect.Center.X, (float)rect.Center.Y), Color.White, 0f, origin, 1f, SpriteEffects.None, 0f, Alignment.TopLeft, ForceUpperCase.Inherit);
			return clicked;
		}

		// Token: 0x0600118D RID: 4493 RVA: 0x000AE2AC File Offset: 0x000AC4AC
		private static void DrawMessages(SpriteBatch spriteBatch, Camera cam)
		{
			if (GUI.messages.Count == 0)
			{
				return;
			}
			bool useScissorRect = GUI.messages.Any((GUIMessage m) => !m.WorldSpace);
			Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			if (useScissorRect)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = HUDLayoutSettings.MessageAreaTop;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, GameMain.ScissorTestEnable, null, null);
			}
			foreach (GUIMessage msg in GUI.messages)
			{
				if (!msg.WorldSpace)
				{
					Vector2 drawPos = new Vector2((float)HUDLayoutSettings.MessageAreaTop.Right, (float)HUDLayoutSettings.MessageAreaTop.Center.Y);
					msg.Font.DrawString(spriteBatch, msg.Text, drawPos + msg.DrawPos + Vector2.One, Color.Black, 0f, msg.Origin, 1f, SpriteEffects.None, 0f, Alignment.TopLeft, ForceUpperCase.Inherit);
					msg.Font.DrawString(spriteBatch, msg.Text, drawPos + msg.DrawPos, msg.Color, 0f, msg.Origin, 1f, SpriteEffects.None, 0f, Alignment.TopLeft, ForceUpperCase.Inherit);
					break;
				}
			}
			if (useScissorRect)
			{
				spriteBatch.End();
				spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
				spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
			}
			foreach (GUIMessage msg2 in GUI.messages)
			{
				if (msg2.WorldSpace && cam != null)
				{
					float alpha = 1f;
					if (msg2.Timer < 1f)
					{
						alpha -= 1f - msg2.Timer;
					}
					Vector2 drawPos2 = cam.WorldToScreen(msg2.DrawPos);
					msg2.Font.DrawString(spriteBatch, msg2.Text, drawPos2 + Vector2.One, Color.Black * alpha, 0f, msg2.Origin, 1f, SpriteEffects.None, 0f, Alignment.TopLeft, ForceUpperCase.Inherit);
					msg2.Font.DrawString(spriteBatch, msg2.Text, drawPos2, msg2.Color * alpha, 0f, msg2.Origin, 1f, SpriteEffects.None, 0f, Alignment.TopLeft, ForceUpperCase.Inherit);
				}
			}
			GUI.messages.RemoveAll((GUIMessage m) => m.Timer <= 0f);
		}

		// Token: 0x0600118E RID: 4494 RVA: 0x000AE59C File Offset: 0x000AC79C
		public static void DrawBezierWithDots(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Vector2 control, int pointCount, Color color, int dotSize = 2)
		{
			for (int i = 0; i < pointCount; i++)
			{
				float t = (float)i / (float)(pointCount - 1);
				Vector2 pos = MathUtils.Bezier(start, control, end, t);
				spriteBatch.DrawPoint(pos, color, (float)dotSize);
			}
		}

		// Token: 0x0600118F RID: 4495 RVA: 0x000AE5D8 File Offset: 0x000AC7D8
		public static void DrawSineWithDots(SpriteBatch spriteBatch, Vector2 from, Vector2 dir, float amplitude, float length, float scale, int pointCount, Color color, int dotSize = 2)
		{
			Vector2 up = dir.Right();
			for (int i = 0; i < pointCount; i++)
			{
				Vector2 pos = from;
				if (i > 0)
				{
					float t = (float)i / (float)(pointCount - 1);
					float sin = (float)Math.Sin((double)(t / length * scale)) * amplitude;
					pos += up * sin + dir * t;
				}
				spriteBatch.DrawPoint(pos, color, (float)dotSize);
			}
		}

		// Token: 0x06001190 RID: 4496 RVA: 0x000AE644 File Offset: 0x000AC844
		private static void DrawSavingIndicator(SpriteBatch spriteBatch)
		{
			if (!GUI.IsSavingIndicatorVisible || GUIStyle.SavingIndicator == null)
			{
				return;
			}
			GUISpriteSheet sheet = GUIStyle.SavingIndicator;
			Vector2 pos = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight) - new Vector2((float)HUDLayoutSettings.Padding) - 2f * GUI.Scale * sheet.FrameSize.ToVector2();
			sheet.Draw(spriteBatch, (int)Math.Floor((double)GUI.savingIndicatorSpriteIndex), pos, GUI.savingIndicatorColor, Vector2.Zero, 0f, new Vector2(GUI.Scale), SpriteEffects.None, null);
		}

		// Token: 0x06001191 RID: 4497 RVA: 0x000AE6E4 File Offset: 0x000AC8E4
		public static void DrawCapsule(SpriteBatch sb, Vector2 origin, float length, float radius, float rotation, Color clr, float depth = 0f, float thickness = 1f)
		{
			GUI.DrawDonutSection(sb, origin + Vector2.Transform(-new Vector2(length / 2f, 0f), Matrix.CreateRotationZ(rotation)), new Range<float>(radius - thickness / 2f, radius + thickness / 2f), 3.1415927f, clr, depth, rotation - 3.1415927f);
			GUI.DrawRectangle(sb, origin, new Vector2(length, radius * 2f), new Vector2(length / 2f, radius), rotation, clr, depth, thickness, GUI.OutlinePosition.Centered);
			GUI.DrawDonutSection(sb, origin + Vector2.Transform(new Vector2(length / 2f, 0f), Matrix.CreateRotationZ(rotation)), new Range<float>(radius - thickness / 2f, radius + thickness / 2f), 3.1415927f, clr, depth, rotation);
		}

		// Token: 0x06001192 RID: 4498 RVA: 0x000AE7C0 File Offset: 0x000AC9C0
		public static Texture2D CreateCircle(int radius, bool filled = false)
		{
			int outerRadius = radius * 2 + 2;
			Color[] data = new Color[outerRadius * outerRadius];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = Color.Transparent;
			}
			if (filled)
			{
				float diameterSqr = (float)(radius * radius);
				for (int x = 0; x < outerRadius; x++)
				{
					for (int y = 0; y < outerRadius; y++)
					{
						Vector2 pos = new Vector2((float)(radius - x), (float)(radius - y));
						if (pos.LengthSquared() <= diameterSqr)
						{
							GUI.TrySetArray(data, y * outerRadius + x + 1, Color.White);
						}
					}
				}
			}
			else
			{
				double angleStep = (double)(1f / (float)radius);
				for (double angle = 0.0; angle < 6.283185307179586; angle += angleStep)
				{
					int x2 = (int)Math.Round((double)radius + (double)radius * Math.Cos(angle));
					int y2 = (int)Math.Round((double)radius + (double)radius * Math.Sin(angle));
					GUI.TrySetArray(data, y2 * outerRadius + x2 + 1, Color.White);
				}
			}
			Texture2D texture = null;
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				texture = new Texture2D(GUI.GraphicsDevice, outerRadius, outerRadius);
				texture.SetData<Color>(data);
			});
			return texture;
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x000AE918 File Offset: 0x000ACB18
		public static Texture2D CreateCapsule(int radius, int height)
		{
			int textureWidth = Math.Max(radius * 2, 1);
			int textureHeight = Math.Max(height + radius * 2, 1);
			Color[] data = new Color[textureWidth * textureHeight];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = Color.Transparent;
			}
			double angleStep = (double)(1f / (float)radius);
			for (int j = 0; j < 2; j++)
			{
				for (double angle = 0.0; angle < 6.283185307179586; angle += angleStep)
				{
					int x = (int)Math.Round((double)radius + (double)radius * Math.Cos(angle));
					int y = (height - 1) * j + (int)Math.Round((double)radius + (double)radius * Math.Sin(angle));
					GUI.TrySetArray(data, y * textureWidth + x, Color.White);
				}
			}
			for (int y2 = radius; y2 < textureHeight - radius; y2++)
			{
				GUI.TrySetArray(data, y2 * textureWidth, Color.White);
				GUI.TrySetArray(data, y2 * textureWidth + (textureWidth - 1), Color.White);
			}
			Texture2D texture = null;
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				texture = new Texture2D(GUI.GraphicsDevice, textureWidth, textureHeight);
				texture.SetData<Color>(data);
			});
			return texture;
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x000AEA7C File Offset: 0x000ACC7C
		public static Texture2D CreateRectangle(int width, int height)
		{
			width = Math.Max(width, 1);
			height = Math.Max(height, 1);
			Color[] data = new Color[width * height];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = Color.Transparent;
			}
			for (int y = 0; y < height; y++)
			{
				GUI.TrySetArray(data, y * width, Color.White);
				GUI.TrySetArray(data, y * width + (width - 1), Color.White);
			}
			for (int x = 0; x < width; x++)
			{
				GUI.TrySetArray(data, x, Color.White);
				GUI.TrySetArray(data, (height - 1) * width + x, Color.White);
			}
			Texture2D texture = null;
			CrossThread.RequestExecutionOnMainThread(delegate
			{
				texture = new Texture2D(GUI.GraphicsDevice, width, height);
				texture.SetData<Color>(data);
			});
			return texture;
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x000AEBAC File Offset: 0x000ACDAC
		private static bool TrySetArray(Color[] data, int index, Color value)
		{
			if (index >= 0 && index < data.Length)
			{
				data[index] = value;
				return true;
			}
			return false;
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x000AEBC4 File Offset: 0x000ACDC4
		public static List<GUIButton> CreateButtons(int count, Vector2 relativeSize, RectTransform parent, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null, Point? minSize = null, Point? maxSize = null, int absoluteSpacing = 0, float relativeSpacing = 0f, Func<int, int> extraSpacing = null, int startOffsetAbsolute = 0, float startOffsetRelative = 0f, bool isHorizontal = false, Alignment textAlignment = Alignment.Center, string style = "")
		{
			Func<RectTransform, GUIButton> constructor = (RectTransform rectT) => new GUIButton(rectT, string.Empty, textAlignment, style, null);
			return GUI.CreateElements<GUIButton>(count, relativeSize, parent, constructor, anchor, pivot, minSize, maxSize, absoluteSpacing, relativeSpacing, extraSpacing, startOffsetAbsolute, startOffsetRelative, isHorizontal);
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x000AEC10 File Offset: 0x000ACE10
		public static List<GUIButton> CreateButtons(int count, Point absoluteSize, RectTransform parent, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null, int absoluteSpacing = 0, float relativeSpacing = 0f, Func<int, int> extraSpacing = null, int startOffsetAbsolute = 0, float startOffsetRelative = 0f, bool isHorizontal = false, Alignment textAlignment = Alignment.Center, string style = "")
		{
			Func<RectTransform, GUIButton> constructor = (RectTransform rectT) => new GUIButton(rectT, string.Empty, textAlignment, style, null);
			return GUI.CreateElements<GUIButton>(count, absoluteSize, parent, constructor, anchor, pivot, absoluteSpacing, relativeSpacing, extraSpacing, startOffsetAbsolute, startOffsetRelative, isHorizontal);
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x000AEC58 File Offset: 0x000ACE58
		public static List<T> CreateElements<T>(int count, Vector2 relativeSize, RectTransform parent, Func<RectTransform, T> constructor, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null, Point? minSize = null, Point? maxSize = null, int absoluteSpacing = 0, float relativeSpacing = 0f, Func<int, int> extraSpacing = null, int startOffsetAbsolute = 0, float startOffsetRelative = 0f, bool isHorizontal = false) where T : GUIComponent
		{
			return GUI.CreateElements<T>(count, parent, constructor, new Vector2?(relativeSize), null, anchor, pivot, minSize, maxSize, absoluteSpacing, relativeSpacing, extraSpacing, startOffsetAbsolute, startOffsetRelative, isHorizontal);
		}

		// Token: 0x06001199 RID: 4505 RVA: 0x000AEC90 File Offset: 0x000ACE90
		public static List<T> CreateElements<T>(int count, Point absoluteSize, RectTransform parent, Func<RectTransform, T> constructor, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null, int absoluteSpacing = 0, float relativeSpacing = 0f, Func<int, int> extraSpacing = null, int startOffsetAbsolute = 0, float startOffsetRelative = 0f, bool isHorizontal = false) where T : GUIComponent
		{
			return GUI.CreateElements<T>(count, parent, constructor, null, new Point?(absoluteSize), anchor, pivot, null, null, absoluteSpacing, relativeSpacing, extraSpacing, startOffsetAbsolute, startOffsetRelative, isHorizontal);
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x000AECD8 File Offset: 0x000ACED8
		public static GUIComponent CreateEnumField(Enum value, int elementHeight, LocalizedString name, RectTransform parent, string toolTip = null, GUIFont font = null)
		{
			font = (font ?? GUIStyle.SmallFont);
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(parent.Rect.Width, elementHeight), parent, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(0.6f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = name;
			GUIFont font2 = font;
			new GUITextBlock(rectT, text, null, font2, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUIDropDown enumDropDown = new GUIDropDown(new RectTransform(new Vector2(0.4f, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), null, Enum.GetValues(value.GetType()).Length, "", false, false, Alignment.CenterLeft, 1f)
			{
				ToolTip = toolTip
			};
			foreach (object enumValue in Enum.GetValues(value.GetType()))
			{
				enumDropDown.AddItem(enumValue.ToString(), enumValue, null, null, null);
			}
			enumDropDown.SelectItem(value);
			return frame;
		}

		// Token: 0x0600119B RID: 4507 RVA: 0x000AEE84 File Offset: 0x000AD084
		public static GUIComponent CreateRectangleField(Rectangle value, int elementHeight, LocalizedString name, RectTransform parent, LocalizedString toolTip = null, GUIFont font = null)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(parent.Rect.Width, Math.Max(elementHeight, 26)), parent, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", new Color?(Color.Transparent));
			font = (font ?? GUIStyle.SmallFont);
			RectTransform rectT = new RectTransform(new Vector2(0.2f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = name;
			GUIFont font2 = font;
			new GUITextBlock(rectT, text, null, font2, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			for (int i = 3; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.22f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(50, 0),
					MaxSize = new Point(150, 50)
				}, null, null);
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = GUI.RectComponentLabels[i];
				font2 = font;
				new GUITextBlock(rectT2, text2, null, font2, Alignment.CenterLeft, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = font
				};
				numberInput.MinValueInt = new int?(0);
				numberInput.MaxValueInt = new int?(9999);
				switch (i)
				{
				case 0:
					numberInput.IntValue = value.X;
					break;
				case 1:
					numberInput.IntValue = value.Y;
					break;
				case 2:
					numberInput.IntValue = value.Width;
					break;
				case 3:
					numberInput.IntValue = value.Height;
					break;
				}
			}
			return frame;
		}

		// Token: 0x0600119C RID: 4508 RVA: 0x000AF168 File Offset: 0x000AD368
		public static GUIComponent CreatePointField(Point value, int elementHeight, LocalizedString displayName, RectTransform parent, LocalizedString toolTip = null)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(parent.Rect.Width, Math.Max(elementHeight, 26)), parent, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(0.4f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.6f, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			for (int i = 1; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.45f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = GUI.VectorComponentLabels[i];
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, smallFont, Alignment.CenterLeft, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				if (i == 0)
				{
					numberInput.IntValue = value.X;
				}
				else
				{
					numberInput.IntValue = value.Y;
				}
			}
			return frame;
		}

		// Token: 0x0600119D RID: 4509 RVA: 0x000AF3D4 File Offset: 0x000AD5D4
		public static GUIComponent CreateVector2Field(Vector2 value, int elementHeight, LocalizedString name, RectTransform parent, LocalizedString toolTip = null, GUIFont font = null, int decimalsToDisplay = 1)
		{
			font = (font ?? GUIStyle.SmallFont);
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(parent.Rect.Width, Math.Max(elementHeight, 26)), parent, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(0.4f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = name;
			GUIFont font2 = font;
			new GUITextBlock(rectT, text, null, font2, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.6f, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			for (int i = 1; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.45f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = GUI.VectorComponentLabels[i];
				font2 = font;
				new GUITextBlock(rectT2, text2, null, font2, Alignment.CenterLeft, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = font
				};
				if (i != 0)
				{
					if (i == 1)
					{
						numberInput.FloatValue = value.Y;
					}
				}
				else
				{
					numberInput.FloatValue = value.X;
				}
				numberInput.DecimalsToDisplay = decimalsToDisplay;
			}
			return frame;
		}

		// Token: 0x0600119E RID: 4510 RVA: 0x000AF654 File Offset: 0x000AD854
		public static GUITextBox CreateTextBoxWithPlaceholder(RectTransform rectT, string text, LocalizedString placeholder)
		{
			GUI.<>c__DisplayClass175_0 CS$<>8__locals1 = new GUI.<>c__DisplayClass175_0();
			GUIFrame holder = new GUIFrame(rectT, null, null);
			GUI.<>c__DisplayClass175_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(Vector2.One, holder.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
			Color? textColor = null;
			GUIFont font = null;
			Alignment textAlignment = Alignment.Left;
			bool wrap = false;
			string style = "";
			Color? color = null;
			CS$<>8__locals2.textBox = new GUITextBox(rectT2, text, textColor, font, textAlignment, wrap, style, color, false, true);
			GUI.<>c__DisplayClass175_0 CS$<>8__locals3 = CS$<>8__locals1;
			RectTransform rectT3 = new RectTransform(Vector2.One, holder.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
			color = new Color?(Color.DarkGray * 0.6f);
			CS$<>8__locals3.placeholderElement = new GUITextBlock(rectT3, placeholder, color, null, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false
			};
			new GUICustomComponent(new RectTransform(Vector2.Zero, holder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float <p0>, GUICustomComponent <p1>)
			{
				CS$<>8__locals1.placeholderElement.RectTransform.NonScaledSize = CS$<>8__locals1.textBox.Frame.RectTransform.NonScaledSize;
			});
			CS$<>8__locals1.textBox.OnSelected += delegate(GUITextBox <p0>, Keys <p1>)
			{
				CS$<>8__locals1.placeholderElement.Visible = false;
			};
			CS$<>8__locals1.textBox.OnDeselected += delegate(GUITextBox <p0>, Keys <p1>)
			{
				CS$<>8__locals1.placeholderElement.Visible = CS$<>8__locals1.textBox.Text.IsNullOrWhiteSpace();
			};
			CS$<>8__locals1.placeholderElement.Visible = string.IsNullOrWhiteSpace(text);
			return CS$<>8__locals1.textBox;
		}

		// Token: 0x0600119F RID: 4511 RVA: 0x000AF7C8 File Offset: 0x000AD9C8
		public static GUITextBox CreateFilterBox(RectTransform rectT)
		{
			GUITextBox guitextBox = new GUITextBox(rectT, "", null, null, Alignment.Left, false, "", null, true, true);
			guitextBox.OnEnterPressed = delegate(GUITextBox tb, string _)
			{
				tb.Deselect();
				return true;
			};
			GUITextBox textBox = guitextBox;
			GUITextBlock label = new GUITextBlock(new RectTransform(Vector2.One, textBox.TextBlock.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("serverlog.filter"), new Color?(GUIStyle.TextColorNormal * 0.75f), null, Alignment.Left, false, "", null);
			textBox.OnSelected += delegate(GUITextBox _, Keys _)
			{
				label.Visible = false;
			};
			textBox.OnDeselected += delegate(GUITextBox tb, Keys _)
			{
				label.Visible = tb.Text.IsNullOrEmpty();
			};
			textBox.OnTextChanged += ((GUITextBox tb, string text) => label.Visible = (!tb.Selected && text.IsNullOrEmpty()));
			return textBox;
		}

		// Token: 0x060011A0 RID: 4512 RVA: 0x000AF8D4 File Offset: 0x000ADAD4
		public static void NotifyPrompt(LocalizedString header, LocalizedString body)
		{
			GUIMessageBox msgBox = new GUIMessageBox(header, body, new LocalizedString[]
			{
				TextManager.Get("Ok")
			}, new Vector2?(new Vector2(0.2f, 0.175f)), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				msgBox.Close();
				return true;
			};
		}

		// Token: 0x060011A1 RID: 4513 RVA: 0x000AF96C File Offset: 0x000ADB6C
		public static GUIMessageBox AskForConfirmation(LocalizedString header, LocalizedString body, Action onConfirm, Action onDeny = null, Vector2? relativeSize = null, Point? minSize = null)
		{
			LocalizedString[] buttons = new LocalizedString[]
			{
				TextManager.Get("Ok"),
				TextManager.Get("Cancel")
			};
			GUIMessageBox msgBox = new GUIMessageBox(header, body, buttons, new Vector2?(relativeSize ?? new Vector2(0.2f, 0.175f)), new Point?(minSize ?? new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				Action onDeny2 = onDeny;
				if (onDeny2 != null)
				{
					onDeny2();
				}
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				onConfirm();
				msgBox.Close();
				return true;
			};
			return msgBox;
		}

		// Token: 0x060011A2 RID: 4514 RVA: 0x000AFA74 File Offset: 0x000ADC74
		public static GUIMessageBox PromptTextInput(LocalizedString header, string body, Action<string> onConfirm)
		{
			LocalizedString[] buttons = new LocalizedString[]
			{
				TextManager.Get("Ok"),
				TextManager.Get("Cancel")
			};
			GUIMessageBox msgBox = new GUIMessageBox(header, string.Empty, buttons, new Vector2?(new Vector2(0.2f, 0.175f)), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUITextBox textBox = new GUITextBox(new RectTransform(Vector2.One, msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), body, null, null, Alignment.Left, false, "", null, false, true)
			{
				OverflowClip = true
			};
			msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				onConfirm(textBox.Text);
				msgBox.Close();
				return true;
			};
			return msgBox;
		}

		// Token: 0x060011A3 RID: 4515 RVA: 0x000AFBB8 File Offset: 0x000ADDB8
		[NullableContext(2)]
		[return: Nullable(1)]
		public static GUITextBlock CreateDigitalDisplay([Nullable(1)] RectTransform rect, [NotNullIfNotNull("leftLabelText")] out GUITextBlock leftLabel, [Nullable(1)] out GUITextBlock rightLabel, LocalizedString leftLabelText = null, LocalizedString rightLabelText = null, LocalizedString tooltip = null, GUIFont leftLabelFont = null)
		{
			GUILayoutGroup textArea = new GUILayoutGroup(rect, true, Anchor.CenterLeft)
			{
				Stretch = true,
				CanBeFocused = true,
				ToolTip = tooltip,
				AbsoluteSpacing = 5
			};
			leftLabel = null;
			if (leftLabelText != null)
			{
				leftLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1f), textArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), leftLabelText, new Color?(GUIStyle.TextColorBright), leftLabelFont ?? GUIStyle.LargeFont, Alignment.CenterRight, false, "", null);
			}
			GUIFrame displayBackground = new GUIFrame(new RectTransform(new Vector2(0.55f, 0.8f), textArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "DigitalFrameDark", null);
			RectTransform rectT = new RectTransform(new Vector2(0.9f, 0.95f), displayBackground.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text = "8888";
			GUIFont digitalFont = GUIStyle.DigitalFont;
			GUITextBlock displayText = new GUITextBlock(rectT, text, new Color?(GUIStyle.TextColorDark), digitalFont, Alignment.CenterRight, false, "", null);
			displayText.TextScale = Math.Max((float)(displayText.Rect.Height - 10) / GUIStyle.DigitalFont.LineHeight, 0.1f);
			rightLabel = new GUITextBlock(new RectTransform(Vector2.Zero, textArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), rightLabelText ?? TextManager.Get("kilowatt"), new Color?(GUIStyle.TextColorNormal), GUIStyle.Font, Alignment.CenterRight, false, "", null)
			{
				Padding = Vector4.Zero
			};
			rightLabel.RectTransform.MinSize = rightLabel.TextSize.ToPoint();
			textArea.GetAllChildren().ForEach(delegate(GUIComponent child)
			{
				child.CanBeFocused = false;
			});
			return displayText;
		}

		// Token: 0x060011A4 RID: 4516 RVA: 0x000AFE1C File Offset: 0x000AE01C
		[NullableContext(1)]
		public static GUITickBox CreateIndicatorLight(RectTransform rect, string style = "", [Nullable(2)] LocalizedString label = null, [Nullable(2)] LocalizedString tooltip = null, [Nullable(2)] GUIFont labelFont = null)
		{
			GUITickBox indicator = new GUITickBox(rect, label, labelFont ?? GUIStyle.SubHeadingFont, style)
			{
				Enabled = false,
				ToolTip = tooltip
			};
			indicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			return indicator;
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x000AFE68 File Offset: 0x000AE068
		private static List<T> CreateElements<T>(int count, RectTransform parent, Func<RectTransform, T> constructor, Vector2? relativeSize = null, Point? absoluteSize = null, Anchor anchor = Anchor.TopLeft, Pivot? pivot = null, Point? minSize = null, Point? maxSize = null, int absoluteSpacing = 0, float relativeSpacing = 0f, Func<int, int> extraSpacing = null, int startOffsetAbsolute = 0, float startOffsetRelative = 0f, bool isHorizontal = false) where T : GUIComponent
		{
			List<T> elements = new List<T>();
			int extraTotal = 0;
			for (int i = 0; i < count; i++)
			{
				if (extraSpacing != null)
				{
					extraTotal += extraSpacing(i);
				}
				if (relativeSize != null)
				{
					Vector2 size = relativeSize.Value;
					Tuple<Vector2, Point> offsets = GUI.CalculateOffsets(size, startOffsetRelative, startOffsetAbsolute, relativeSpacing, absoluteSpacing, i, extraTotal, isHorizontal);
					elements.Add(constructor(new RectTransform(size, parent, anchor, pivot, minSize, maxSize, ScaleBasis.Normal)
					{
						RelativeOffset = offsets.Item1,
						AbsoluteOffset = offsets.Item2
					}));
				}
				else
				{
					Point size2 = absoluteSize.Value;
					Tuple<Vector2, Point> offsets2 = GUI.CalculateOffsets(size2, startOffsetRelative, startOffsetAbsolute, relativeSpacing, absoluteSpacing, i, extraTotal, isHorizontal);
					elements.Add(constructor(new RectTransform(size2, parent, anchor, pivot, ScaleBasis.Normal, false)
					{
						RelativeOffset = offsets2.Item1,
						AbsoluteOffset = offsets2.Item2
					}));
				}
			}
			return elements;
		}

		// Token: 0x060011A6 RID: 4518 RVA: 0x000AFF50 File Offset: 0x000AE150
		private static Tuple<Vector2, Point> CalculateOffsets(Vector2 relativeSize, float startOffsetRelative, int startOffsetAbsolute, float relativeSpacing, int absoluteSpacing, int counter, int extra, bool isHorizontal)
		{
			float relX = 0f;
			float relY = 0f;
			int absX = 0;
			int absY = 0;
			if (isHorizontal)
			{
				relX = GUI.CalculateRelativeOffset(startOffsetRelative, relativeSpacing, relativeSize.X, counter);
				absX = GUI.CalculateAbsoluteOffset(startOffsetAbsolute, absoluteSpacing, counter, extra);
			}
			else
			{
				relY = GUI.CalculateRelativeOffset(startOffsetRelative, relativeSpacing, relativeSize.Y, counter);
				absY = GUI.CalculateAbsoluteOffset(startOffsetAbsolute, absoluteSpacing, counter, extra);
			}
			return Tuple.Create<Vector2, Point>(new Vector2(relX, relY), new Point(absX, absY));
		}

		// Token: 0x060011A7 RID: 4519 RVA: 0x000AFFC0 File Offset: 0x000AE1C0
		private static Tuple<Vector2, Point> CalculateOffsets(Point absoluteSize, float startOffsetRelative, int startOffsetAbsolute, float relativeSpacing, int absoluteSpacing, int counter, int extra, bool isHorizontal)
		{
			float relX = 0f;
			float relY = 0f;
			int absX = 0;
			int absY = 0;
			if (isHorizontal)
			{
				relX = GUI.CalculateRelativeOffset(startOffsetRelative, relativeSpacing, counter);
				absX = GUI.CalculateAbsoluteOffset(startOffsetAbsolute, absoluteSpacing, absoluteSize.X, counter, extra);
			}
			else
			{
				relY = GUI.CalculateRelativeOffset(startOffsetRelative, relativeSpacing, counter);
				absY = GUI.CalculateAbsoluteOffset(startOffsetAbsolute, absoluteSpacing, absoluteSize.Y, counter, extra);
			}
			return Tuple.Create<Vector2, Point>(new Vector2(relX, relY), new Point(absX, absY));
		}

		// Token: 0x060011A8 RID: 4520 RVA: 0x000B0030 File Offset: 0x000AE230
		private static float CalculateRelativeOffset(float startOffset, float spacing, float size, int counter)
		{
			return startOffset + (spacing + size) * (float)counter;
		}

		// Token: 0x060011A9 RID: 4521 RVA: 0x000B003A File Offset: 0x000AE23A
		private static float CalculateRelativeOffset(float startOffset, float spacing, int counter)
		{
			return startOffset + spacing * (float)counter;
		}

		// Token: 0x060011AA RID: 4522 RVA: 0x000B0042 File Offset: 0x000AE242
		private static int CalculateAbsoluteOffset(int startOffset, int spacing, int counter, int extra)
		{
			return startOffset + spacing * counter + extra;
		}

		// Token: 0x060011AB RID: 4523 RVA: 0x000B004B File Offset: 0x000AE24B
		private static int CalculateAbsoluteOffset(int startOffset, int spacing, int size, int counter, int extra)
		{
			return startOffset + (spacing + size) * counter + extra;
		}

		// Token: 0x060011AC RID: 4524 RVA: 0x000B0058 File Offset: 0x000AE258
		public static void PreventElementOverlap(IList<GUIComponent> elements, IList<Rectangle> disallowedAreas = null, Rectangle? clampArea = null)
		{
			List<GUIComponent> sortedElements = (from e in elements
			orderby e.Rect.Width + e.Rect.Height descending
			select e).ToList<GUIComponent>();
			Rectangle area = clampArea ?? new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
			for (int i = 0; i < sortedElements.Count; i++)
			{
				Point moveAmount = Point.Zero;
				Rectangle rect = sortedElements[i].Rect;
				moveAmount.X += Math.Max(area.X - rect.X, 0);
				moveAmount.X -= Math.Max(rect.Right - area.Right, 0);
				moveAmount.Y += Math.Max(area.Y - rect.Y, 0);
				moveAmount.Y -= Math.Max(rect.Bottom - area.Bottom, 0);
				sortedElements[i].RectTransform.ScreenSpaceOffset += moveAmount;
			}
			bool intersections = true;
			int iterations = 0;
			while (intersections && iterations < 100)
			{
				intersections = false;
				for (int j = 0; j < sortedElements.Count; j++)
				{
					Rectangle rect2 = sortedElements[j].Rect;
					for (int k = j + 1; k < sortedElements.Count; k++)
					{
						Rectangle rect3 = sortedElements[k].Rect;
						if (rect2.Intersects(rect3))
						{
							intersections = true;
							Point centerDiff = rect2.Center - rect3.Center;
							Vector2 moveAmount2 = (centerDiff == Point.Zero) ? (Vector2.UnitX + Rand.Vector(0.1f, Rand.RandSync.Unsynced)) : Vector2.Normalize(centerDiff.ToVector2());
							if (Math.Abs(moveAmount2.X) > Math.Abs(moveAmount2.Y) * 8f)
							{
								moveAmount2.Y = 0f;
							}
							else if (Math.Abs(moveAmount2.Y) > Math.Abs(moveAmount2.X) * 8f)
							{
								moveAmount2.X = 0f;
							}
							Vector2 moveAmount3 = GUI.ClampMoveAmount(rect2, area, moveAmount2 * 10f);
							Vector2 moveAmount4 = GUI.ClampMoveAmount(rect3, area, -moveAmount2 * 10f);
							sortedElements[j].RectTransform.ScreenSpaceOffset += moveAmount3.ToPoint();
							sortedElements[k].RectTransform.ScreenSpaceOffset += moveAmount4.ToPoint();
						}
					}
					if (disallowedAreas != null)
					{
						foreach (Rectangle rect4 in disallowedAreas)
						{
							if (rect2.Intersects(rect4))
							{
								intersections = true;
								Point centerDiff2 = rect2.Center - rect4.Center;
								Vector2 moveAmount5 = (centerDiff2 == Point.Zero) ? Rand.Vector(1f, Rand.RandSync.Unsynced) : Vector2.Normalize(centerDiff2.ToVector2());
								Vector2 moveAmount6 = GUI.ClampMoveAmount(rect2, area, moveAmount5 * 10f);
								sortedElements[j].RectTransform.ScreenSpaceOffset += moveAmount6.ToPoint();
							}
						}
					}
				}
				iterations++;
			}
		}

		// Token: 0x060011AD RID: 4525 RVA: 0x000B03EC File Offset: 0x000AE5EC
		private static Vector2 ClampMoveAmount(Rectangle rect, Rectangle clampTo, Vector2 moveAmount)
		{
			if (rect.Y < clampTo.Y)
			{
				moveAmount.Y = Math.Max(moveAmount.Y, 0f);
			}
			else if (rect.Bottom > clampTo.Bottom)
			{
				moveAmount.Y = Math.Min(moveAmount.Y, 0f);
			}
			if (rect.X < clampTo.X)
			{
				moveAmount.X = Math.Max(moveAmount.X, 0f);
			}
			else if (rect.Right > clampTo.Right)
			{
				moveAmount.X = Math.Min(moveAmount.X, 0f);
			}
			return moveAmount;
		}

		// Token: 0x060011AE RID: 4526 RVA: 0x000B0498 File Offset: 0x000AE698
		public static void TogglePauseMenu()
		{
			if (Screen.Selected == GameMain.MainMenuScreen)
			{
				return;
			}
			if (GUI.PreventPauseMenuToggle)
			{
				return;
			}
			GUI.SettingsMenuOpen = false;
			GUI.TogglePauseMenu(null, null);
			if (GUI.PauseMenuOpen)
			{
				Inventory.DraggingItems.Clear();
				Inventory.DraggingInventory = null;
				GUI.PauseMenu = new GUIFrame(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
				new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.PauseMenu.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
				GUIFrame pauseMenuInner = new GUIFrame(new RectTransform(new Vector2(0.13f, 0.3f), GUI.PauseMenu.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(250, 300)
				}, "", null);
				float padding = 0.06f;
				GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 0.8f), pauseMenuInner.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0f, padding)
				}, false, Anchor.TopLeft)
				{
					AbsoluteSpacing = GUI.IntScale(15f)
				};
				GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.1f, 0.07f), pauseMenuInner.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(padding)
				}, "", Alignment.Center, "GUIBugButton", null);
				guibutton.IgnoreLayoutGroups = true;
				LocalizedString left = TextManager.Get("bugreportbutton");
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
				defaultInterpolatedStringHandler.AppendLiteral(" (v");
				defaultInterpolatedStringHandler.AppendFormatted<Version>(GameMain.Version);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				guibutton.ToolTip = left + defaultInterpolatedStringHandler.ToStringAndClear();
				guibutton.OnClicked = delegate(GUIButton btn, object userdata)
				{
					if (GUI.PauseMenuOpen)
					{
						GUI.TogglePauseMenu();
					}
					GameMain.Instance.ShowBugReporter();
					return true;
				};
				GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuResume", buttonContainer, null, null, null);
				GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuSettings", buttonContainer, delegate
				{
					GUI.SettingsMenuOpen = true;
				}, null, null);
				if (Screen.Selected == GameMain.GameScreen && GameMain.GameSession != null)
				{
					SinglePlayerCampaign spMode = GameMain.GameSession.GameMode as SinglePlayerCampaign;
					if (spMode != null)
					{
						GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuRetry", buttonContainer, delegate
						{
							RoundSummary roundSummary = GameMain.GameSession.RoundSummary;
							if (((roundSummary != null) ? roundSummary.Frame : null) != null)
							{
								GUIMessageBox.MessageBoxes.Remove(GameMain.GameSession.RoundSummary.Frame);
							}
							GUIMessageBox.MessageBoxes.RemoveAll((GUIComponent mb) => mb.UserData as string == "ConversationAction");
							GameMain.GameSession.LoadPreviousSave();
						}, "PauseMenuRetryVerification", null);
						if (GUI.<TogglePauseMenu>g__IsFriendlyOutpostLevel|191_2() && !spMode.CrewDead)
						{
							GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuSaveQuit", buttonContainer, delegate
							{
								if (GUI.<TogglePauseMenu>g__IsFriendlyOutpostLevel|191_2())
								{
									GameMain.QuitToMainMenu(true);
								}
							}, "PauseMenuSaveAndReturnToMainMenuVerification", null);
						}
					}
					else if (GameMain.GameSession.GameMode is TestGameMode)
					{
						GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuReturnToEditor", buttonContainer, delegate
						{
							GameSession gameSession = GameMain.GameSession;
							if (gameSession == null)
							{
								return;
							}
							gameSession.EndRound("", CampaignMode.TransitionType.None, null, true);
						}, null, null);
					}
					else if (!GameMain.GameSession.GameMode.IsSinglePlayer && GameMain.Client != null)
					{
						if (!GameMain.Client.IsServerOwner)
						{
							GUI.<TogglePauseMenu>g__CreateButton|191_0("ReturnToServerlobby", buttonContainer, delegate
							{
								GameClient client = GameMain.Client;
								if (client == null)
								{
									return;
								}
								client.EndRoundForSelf();
							}, "PauseMenuReturnToServerLobbyVerificationSelf", null);
						}
						if (GameMain.Client.HasPermission(ClientPermissions.ManageRound))
						{
							bool canSave = GameMain.GameSession.GameMode is CampaignMode && GUI.<TogglePauseMenu>g__IsFriendlyOutpostLevel|191_2();
							if (canSave)
							{
								GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuSaveQuit", buttonContainer, delegate
								{
									GameClient client = GameMain.Client;
									if (client == null)
									{
										return;
									}
									client.RequestEndRound(true, false);
								}, "PauseMenuSaveAndReturnToServerLobbyVerification", new Color?(GUIStyle.Red));
							}
							GUI.<TogglePauseMenu>g__CreateButton|191_0("EndRound", buttonContainer, delegate
							{
								GameClient client = GameMain.Client;
								if (client == null)
								{
									return;
								}
								client.RequestEndRound(false, false);
							}, (GameMain.GameSession.GameMode is CampaignMode) ? "PauseMenuReturnToServerLobbyVerification" : "EndRoundSubNotAtLevelEnd", new Color?(GUIStyle.Red));
						}
					}
				}
				if (GameMain.GameSession != null || Screen.Selected is CharacterEditorScreen || Screen.Selected is SubEditorScreen)
				{
					GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuQuit", buttonContainer, delegate
					{
						GameMain.QuitToMainMenu(false);
					}, (GameMain.GameSession == null) ? "PauseMenuQuitVerificationEditor" : "PauseMenuQuitVerification", null);
				}
				else
				{
					GUI.<TogglePauseMenu>g__CreateButton|191_0("PauseMenuQuit", buttonContainer, delegate
					{
						GameMain.QuitToMainMenu(false);
					}, null, null);
				}
				GUITextBlock.AutoScaleAndNormalize(from c in buttonContainer.Children
				where c is GUIButton
				select ((GUIButton)c).TextBlock, true, false, null);
				pauseMenuInner.RectTransform.MinSize = new Point(pauseMenuInner.RectTransform.MinSize.X, Math.Max((int)((float)buttonContainer.Children.Sum((GUIComponent c) => c.Rect.Height + buttonContainer.AbsoluteSpacing) / buttonContainer.RectTransform.RelativeSize.Y), pauseMenuInner.RectTransform.MinSize.X));
			}
		}

		// Token: 0x060011AF RID: 4527 RVA: 0x000B0B48 File Offset: 0x000AED48
		public static void CreateVerificationPrompt(string textTag, Action confirmAction)
		{
			GUIMessageBox msgBox = new GUIMessageBox("", TextManager.Get(textTag), new LocalizedString[]
			{
				TextManager.Get("Yes"),
				TextManager.Get("No")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
			{
				UserData = "verificationprompt",
				DrawOnTop = true
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton _, object __)
			{
				GUI.PauseMenuOpen = false;
				Action confirmAction2 = confirmAction;
				if (confirmAction2 != null)
				{
					confirmAction2();
				}
				return true;
			};
			GUIButton guibutton = msgBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
			GUIButton guibutton2 = msgBox.Buttons[1];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
		}

		// Token: 0x060011B0 RID: 4528 RVA: 0x000B0C49 File Offset: 0x000AEE49
		private static bool TogglePauseMenu(GUIButton button, object obj)
		{
			GUI.PauseMenuOpen = !GUI.PauseMenuOpen;
			if (!GUI.PauseMenuOpen && GUI.PauseMenu != null)
			{
				GUI.PauseMenu.RectTransform.Parent = null;
				GUI.PauseMenu = null;
			}
			return true;
		}

		// Token: 0x060011B1 RID: 4529 RVA: 0x000B0C7D File Offset: 0x000AEE7D
		public static void AddMessage(LocalizedString message, Color color, float? lifeTime = null, bool playSound = true, GUIFont font = null)
		{
			GUI.AddMessage(message.Value, color, lifeTime, playSound, font);
		}

		// Token: 0x060011B2 RID: 4530 RVA: 0x000B0C8F File Offset: 0x000AEE8F
		public static void AddMessage(LocalizedString message, Color color, Vector2 pos, Vector2 velocity, float lifeTime = 3f, bool playSound = true, GUISoundType soundType = GUISoundType.UIMessage, int subId = -1)
		{
			GUI.AddMessage(message.Value, color, pos, velocity, lifeTime, playSound, soundType, subId);
		}

		// Token: 0x060011B3 RID: 4531 RVA: 0x000B0CA8 File Offset: 0x000AEEA8
		public static void AddMessage(string message, Color color, float? lifeTime = null, bool playSound = true, GUIFont font = null)
		{
			GUIMessage guiMessage = new GUIMessage(message, color, lifeTime ?? MathHelper.Clamp((float)message.Length / 5f, 3f, 10f), font ?? GUIStyle.LargeFont);
			object obj = GUI.mutex;
			lock (obj)
			{
				if (GUI.messages.Any((GUIMessage msg) => msg.Text == message))
				{
					return;
				}
				GUI.messages.Add(guiMessage);
			}
			if (playSound)
			{
				SoundPlayer.PlayUISound(GUISoundType.UIMessage);
			}
		}

		// Token: 0x060011B4 RID: 4532 RVA: 0x000B0D6C File Offset: 0x000AEF6C
		public static void AddMessage(string message, Color color, Vector2 pos, Vector2 velocity, float lifeTime = 3f, bool playSound = true, GUISoundType soundType = GUISoundType.UIMessage, int subId = -1)
		{
			Submarine sub = Submarine.Loaded.FirstOrDefault((Submarine s) => (int)s.ID == subId);
			GUIMessage newMessage = new GUIMessage(message, color, pos, velocity, lifeTime, Alignment.Center, GUIStyle.Font, sub);
			if (playSound)
			{
				SoundPlayer.PlayUISound(soundType);
			}
			object obj = GUI.mutex;
			lock (obj)
			{
				bool overlapFound = true;
				int tries = 0;
				while (overlapFound)
				{
					overlapFound = false;
					foreach (GUIMessage otherMessage in GUI.messages)
					{
						float xDiff = otherMessage.Pos.X - newMessage.Pos.X;
						if (Math.Abs(xDiff) <= (newMessage.Size.X + otherMessage.Size.X) / 2f)
						{
							float yDiff = otherMessage.Pos.Y - newMessage.Pos.Y;
							if (Math.Abs(yDiff) <= (newMessage.Size.Y + otherMessage.Size.Y) / 2f)
							{
								Vector2 moveDir = -(new Vector2(xDiff, yDiff) + Rand.Vector(1f, Rand.RandSync.Unsynced));
								if (moveDir.LengthSquared() > 0.0001f)
								{
									moveDir = Vector2.Normalize(moveDir);
								}
								else
								{
									moveDir = Rand.Vector(1f, Rand.RandSync.Unsynced);
								}
								moveDir.Y = -Math.Abs(moveDir.Y);
								newMessage.Pos -= Vector2.UnitY * 10f;
							}
						}
					}
					tries++;
					if (tries > 20)
					{
						break;
					}
				}
				GUI.messages.Add(newMessage);
			}
		}

		// Token: 0x060011B5 RID: 4533 RVA: 0x000B0F70 File Offset: 0x000AF170
		public static void ClearMessages()
		{
			object obj = GUI.mutex;
			lock (obj)
			{
				GUI.messages.Clear();
			}
		}

		// Token: 0x060011B6 RID: 4534 RVA: 0x000B0FB4 File Offset: 0x000AF1B4
		public static void SetSavingIndicatorState(bool enabled)
		{
			if (enabled)
			{
				GUI.timeUntilSavingIndicatorDisabled = null;
			}
			GUI.isSavingIndicatorEnabled = enabled;
		}

		// Token: 0x060011B7 RID: 4535 RVA: 0x000B0FCA File Offset: 0x000AF1CA
		public static void DisableSavingIndicatorDelayed(float delay = 3f)
		{
			if (!GUI.isSavingIndicatorEnabled)
			{
				return;
			}
			GUI.timeUntilSavingIndicatorDisabled = new float?(delay);
		}

		// Token: 0x060011B9 RID: 4537 RVA: 0x000B11F0 File Offset: 0x000AF3F0
		[CompilerGenerated]
		internal static GUIComponent <UpdateMouseCursorState>g__FindInteractParent|126_0(GUIComponent component)
		{
			for (;;)
			{
				GUIComponent parent = component.Parent;
				if (parent == null)
				{
					break;
				}
				if (!GUI.<UpdateMouseCursorState>g__ContainsMouse|126_1(parent))
				{
					goto IL_65;
				}
				if (parent.Enabled)
				{
					GUIButton button = parent as GUIButton;
					if (button != null)
					{
						return button;
					}
					GUITextBox box = parent as GUITextBox;
					if (box != null)
					{
						return box;
					}
					GUIListBox list = parent as GUIListBox;
					if (list != null)
					{
						return list;
					}
					GUIScrollBar bar = parent as GUIScrollBar;
					if (bar != null)
					{
						return bar;
					}
					GUIDragHandle dragHandle = parent as GUIDragHandle;
					if (dragHandle != null)
					{
						return dragHandle;
					}
				}
				component = parent;
			}
			return null;
			IL_65:
			return null;
		}

		// Token: 0x060011BA RID: 4538 RVA: 0x000B1264 File Offset: 0x000AF464
		[CompilerGenerated]
		internal static bool <UpdateMouseCursorState>g__ContainsMouse|126_1(GUIComponent component)
		{
			if (component.MouseRect.Equals(Rectangle.Empty))
			{
				return component.Rect.Contains(PlayerInput.MousePosition);
			}
			return component.MouseRect.Contains(PlayerInput.MousePosition);
		}

		// Token: 0x060011BB RID: 4539 RVA: 0x000B12B0 File Offset: 0x000AF4B0
		[CompilerGenerated]
		internal static float <DrawDonutSection>g__getRadius|146_0(int vertexIndex, ref GUI.<>c__DisplayClass146_0 A_1)
		{
			float result;
			switch (vertexIndex % 4)
			{
			case 0:
				result = A_1.radii.End;
				break;
			case 1:
				result = A_1.radii.End;
				break;
			case 2:
				result = A_1.radii.Start;
				break;
			case 3:
				result = A_1.radii.Start;
				break;
			default:
				throw new InvalidOperationException();
			}
			return result;
		}

		// Token: 0x060011BC RID: 4540 RVA: 0x000B1318 File Offset: 0x000AF518
		[CompilerGenerated]
		internal static int <DrawDonutSection>g__getDirectionIndex|146_1(int vertexIndex)
		{
			int result;
			switch (vertexIndex % 4)
			{
			case 0:
				result = vertexIndex / 4;
				break;
			case 1:
				result = vertexIndex / 4 + 1;
				break;
			case 2:
				result = vertexIndex / 4;
				break;
			case 3:
				result = vertexIndex / 4 + 1;
				break;
			default:
				throw new InvalidOperationException();
			}
			return result;
		}

		// Token: 0x060011BD RID: 4541 RVA: 0x000B1364 File Offset: 0x000AF564
		[CompilerGenerated]
		internal static Vector2 <DrawDonutSection>g__getDirection|146_2(int vertexIndex, ref GUI.<>c__DisplayClass146_0 A_1)
		{
			int directionIndex = GUI.<DrawDonutSection>g__getDirectionIndex|146_1(vertexIndex);
			Vector2 dir = GUI.canonicalCircle[directionIndex % 30];
			if (A_1.maxDirectionIndex > 0 && directionIndex >= A_1.maxDirectionIndex)
			{
				float maxSectionProportion = (float)A_1.maxDirectionIndex / 30f;
				dir = Vector2.Lerp(GUI.canonicalCircle[A_1.maxDirectionIndex - 1], GUI.canonicalCircle[A_1.maxDirectionIndex % 30], 1f - (maxSectionProportion - A_1.sectionProportion) * 30f);
			}
			return new Vector2(dir.Y, -dir.X);
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x000B13F8 File Offset: 0x000AF5F8
		[CompilerGenerated]
		internal static bool <TogglePauseMenu>g__IsFriendlyOutpostLevel|191_2()
		{
			return GameMain.GameSession != null && Level.IsLoadedFriendlyOutpost;
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x000B1408 File Offset: 0x000AF608
		[CompilerGenerated]
		internal static void <TogglePauseMenu>g__CreateButton|191_0(string textTag, GUIComponent parent, Action action, string verificationTextTag = null, Color? color = null)
		{
			GUIButton button = new GUIButton(new RectTransform(new Vector2(1f, 0.1f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(textTag), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object userData)
				{
					if (string.IsNullOrEmpty(verificationTextTag))
					{
						GUI.PauseMenuOpen = false;
						Action action2 = action;
						if (action2 != null)
						{
							action2();
						}
					}
					else
					{
						GUI.CreateVerificationPrompt(verificationTextTag, action);
					}
					return true;
				}
			};
			if (color != null)
			{
				button.Color = color.Value;
				button.HoverColor = Color.Lerp(color.Value, Color.White, 0.5f);
				button.PressedColor = Color.Lerp(color.Value, Color.Black, 0.5f);
			}
		}

		// Token: 0x040008B7 RID: 2231
		public static CursorState MouseCursor = CursorState.Default;

		// Token: 0x040008B8 RID: 2232
		public static readonly SamplerState SamplerState = new SamplerState
		{
			Filter = TextureFilter.Linear,
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			AddressW = TextureAddressMode.Wrap,
			BorderColor = Color.White,
			MaxAnisotropy = 4,
			MaxMipLevel = 0,
			MipMapLevelOfDetailBias = -0.8f,
			ComparisonFunction = CompareFunction.Never,
			FilterMode = TextureFilterMode.Default
		};

		// Token: 0x040008B9 RID: 2233
		public static readonly SamplerState SamplerStateClamp = new SamplerState
		{
			Filter = TextureFilter.Linear,
			AddressU = TextureAddressMode.Clamp,
			AddressV = TextureAddressMode.Clamp,
			AddressW = TextureAddressMode.Clamp,
			BorderColor = Color.White,
			MaxAnisotropy = 4,
			MaxMipLevel = 0,
			MipMapLevelOfDetailBias = -0.8f,
			ComparisonFunction = CompareFunction.Never,
			FilterMode = TextureFilterMode.Default
		};

		// Token: 0x040008BA RID: 2234
		public static readonly string[] VectorComponentLabels = new string[]
		{
			"X",
			"Y",
			"Z",
			"W"
		};

		// Token: 0x040008BB RID: 2235
		public static readonly string[] RectComponentLabels = new string[]
		{
			"X",
			"Y",
			"W",
			"H"
		};

		// Token: 0x040008BC RID: 2236
		public static readonly string[] ColorComponentLabels = new string[]
		{
			"R",
			"G",
			"B",
			"A"
		};

		// Token: 0x040008BD RID: 2237
		private static readonly object mutex = new object();

		// Token: 0x040008BE RID: 2238
		public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

		// Token: 0x040008BF RID: 2239
		private static Texture2D solidWhiteTexture;

		// Token: 0x040008C0 RID: 2240
		private static bool debugDrawSounds;

		// Token: 0x040008C1 RID: 2241
		private static bool debugDrawEvents;

		// Token: 0x040008C2 RID: 2242
		private static GUI.DebugDrawMetaData debugDrawMetaData;

		// Token: 0x040008C3 RID: 2243
		private static readonly List<GUIMessage> messages = new List<GUIMessage>();

		// Token: 0x040008C6 RID: 2246
		public static bool HideCursor;

		// Token: 0x040008C8 RID: 2248
		public static bool ScreenChanged;

		// Token: 0x040008C9 RID: 2249
		private static bool settingsMenuOpen;

		// Token: 0x040008CB RID: 2251
		public static bool PreventPauseMenuToggle = false;

		// Token: 0x040008CD RID: 2253
		public static bool DisableHUD;

		// Token: 0x040008CE RID: 2254
		public static bool DisableUpperHUD;

		// Token: 0x040008CF RID: 2255
		public static bool DisableItemHighlights;

		// Token: 0x040008D0 RID: 2256
		public static bool DisableCharacterNames;

		// Token: 0x040008D1 RID: 2257
		private static bool isSavingIndicatorEnabled;

		// Token: 0x040008D2 RID: 2258
		private static Color savingIndicatorColor = Color.Transparent;

		// Token: 0x040008D3 RID: 2259
		private static float savingIndicatorSpriteIndex;

		// Token: 0x040008D4 RID: 2260
		private static float savingIndicatorColorLerpAmount;

		// Token: 0x040008D5 RID: 2261
		private static GUI.SavingIndicatorState savingIndicatorState = GUI.SavingIndicatorState.None;

		// Token: 0x040008D6 RID: 2262
		private static float? timeUntilSavingIndicatorDisabled;

		// Token: 0x040008D7 RID: 2263
		private static string loadedSpritesText;

		// Token: 0x040008D8 RID: 2264
		private static DateTime loadedSpritesUpdateTime;

		// Token: 0x040008D9 RID: 2265
		private static readonly List<GUIComponent> updateList = new List<GUIComponent>();

		// Token: 0x040008DA RID: 2266
		private static readonly HashSet<GUIComponent> updateListSet = new HashSet<GUIComponent>();

		// Token: 0x040008DB RID: 2267
		private static readonly Queue<GUIComponent> removals = new Queue<GUIComponent>();

		// Token: 0x040008DC RID: 2268
		private static readonly Queue<GUIComponent> additions = new Queue<GUIComponent>();

		// Token: 0x040008DD RID: 2269
		private static readonly List<GUIComponent> firstAdditions = new List<GUIComponent>();

		// Token: 0x040008DE RID: 2270
		private static readonly List<GUIComponent> lastAdditions = new List<GUIComponent>();

		// Token: 0x040008E0 RID: 2272
		private static readonly List<float> usedIndicatorAngles = new List<float>();

		// Token: 0x040008E1 RID: 2273
		private const int DonutSegments = 30;

		// Token: 0x040008E2 RID: 2274
		private static readonly ImmutableArray<Vector2> canonicalCircle = (from i in Enumerable.Range(0, 30)
		select (float)i * 0.20943952f into angle
		select new Vector2(MathF.Cos(angle), MathF.Sin(angle))).ToImmutableArray<Vector2>();

		// Token: 0x040008E3 RID: 2275
		private static readonly VertexPositionColorTexture[] donutVerts = new VertexPositionColorTexture[120];

		// Token: 0x0200091C RID: 2332
		public enum OutlinePosition
		{
			// Token: 0x04004056 RID: 16470
			Default,
			// Token: 0x04004057 RID: 16471
			Inside,
			// Token: 0x04004058 RID: 16472
			Centered,
			// Token: 0x04004059 RID: 16473
			Outside
		}

		// Token: 0x0200091D RID: 2333
		public struct DebugDrawMetaData
		{
			// Token: 0x0400405A RID: 16474
			public bool Enabled;

			// Token: 0x0400405B RID: 16475
			public bool FactionMetadata;

			// Token: 0x0400405C RID: 16476
			public bool UpgradeLevels;

			// Token: 0x0400405D RID: 16477
			public bool UpgradePrices;

			// Token: 0x0400405E RID: 16478
			public int Offset;
		}

		// Token: 0x0200091E RID: 2334
		private enum SavingIndicatorState
		{
			// Token: 0x04004060 RID: 16480
			None,
			// Token: 0x04004061 RID: 16481
			FadingIn,
			// Token: 0x04004062 RID: 16482
			FadingOut
		}
	}
}
