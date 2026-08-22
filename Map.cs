using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x020000E5 RID: 229
	internal class Map
	{
		// Token: 0x17000871 RID: 2161
		// (get) Token: 0x06001F62 RID: 8034 RVA: 0x0013689C File Offset: 0x00134A9C
		// (set) Token: 0x06001F63 RID: 8035 RVA: 0x001368A4 File Offset: 0x00134AA4
		public Location HighlightedLocation { get; private set; }

		// Token: 0x06001F64 RID: 8036 RVA: 0x001368B0 File Offset: 0x00134AB0
		private void LocationChanged(Location prevLocation, Location newLocation)
		{
			if (prevLocation == newLocation)
			{
				return;
			}
			if (prevLocation != null)
			{
				this.mapAnimQueue.Enqueue(new Map.MapAnim
				{
					EndZoom = new float?(1f),
					EndLocation = prevLocation,
					Duration = MathHelper.Clamp(Vector2.Distance(-this.DrawOffset, prevLocation.MapPosition) / 1000f, 0.1f, 0.5f)
				});
				this.mapAnimQueue.Enqueue(new Map.MapAnim
				{
					EndZoom = new float?(0.5f),
					StartLocation = prevLocation,
					EndLocation = newLocation,
					Duration = 2f,
					StartDelay = 0.5f
				});
			}
			else
			{
				this.currLocationIndicatorPos = this.CurrentLocation.MapPosition;
			}
			if (newLocation.Visited)
			{
				this.RemoveFogOfWar(newLocation, true);
			}
		}

		// Token: 0x06001F65 RID: 8037 RVA: 0x00136988 File Offset: 0x00134B88
		private void RemoveFogOfWar(Location location, bool removeFromAdjacentLocations = true)
		{
			if (this.mapTiles == null)
			{
				return;
			}
			if (location == null)
			{
				return;
			}
			Sprite mapTile = this.generationParams.MapTiles.Values.FirstOrDefault<ImmutableArray<Sprite>>().FirstOrDefault<Sprite>();
			if (mapTile == null)
			{
				return;
			}
			Vector2 mapTileSize = mapTile.size * this.generationParams.MapTileScale;
			int startX = (int)Math.Max(Math.Floor((double)(location.MapPosition.X / mapTileSize.X - 0.25f)), 0.0);
			int startY = (int)Math.Max(Math.Floor((double)(location.MapPosition.Y / mapTileSize.Y - 0.25f)), 0.0);
			int endX = (int)Math.Min(Math.Floor((double)(location.MapPosition.X / mapTileSize.X + 0.25f)), (double)(this.mapTiles.GetLength(0) - 1));
			int endY = (int)Math.Min(Math.Floor((double)(location.MapPosition.Y / mapTileSize.Y + 0.25f)), (double)(this.mapTiles.GetLength(1) - 1));
			for (int x = startX; x <= endX; x++)
			{
				for (int y = startY; y <= endY; y++)
				{
					this.tileDiscovered[x, y] = true;
				}
			}
			if (removeFromAdjacentLocations)
			{
				foreach (LocationConnection c in location.Connections)
				{
					Location otherLocation = c.OtherLocation(location);
					this.RemoveFogOfWar(otherLocation, false);
				}
			}
		}

		// Token: 0x06001F66 RID: 8038 RVA: 0x00136B28 File Offset: 0x00134D28
		private bool IsInFogOfWar(Location location)
		{
			if (GameMain.DebugDraw)
			{
				return false;
			}
			Vector2 mapTileSize = this.mapTiles[0, 0].size * this.generationParams.MapTileScale;
			int x = (int)Math.Floor((double)(location.MapPosition.X / mapTileSize.X));
			int y = (int)Math.Floor((double)(location.MapPosition.Y / mapTileSize.Y));
			return !this.tileDiscovered[MathHelper.Clamp(x, 0, this.tileDiscovered.Length), MathHelper.Clamp(y, 0, this.tileDiscovered.Length)];
		}

		// Token: 0x06001F67 RID: 8039 RVA: 0x00136BC8 File Offset: 0x00134DC8
		public void DrawNotifications(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			Vector2 pos = new Vector2((float)container.Rect.Right, (float)container.Rect.Center.Y);
			foreach (Map.MapNotification notification in this.mapNotifications)
			{
				Vector2 textPos = pos + new Vector2(notification.Offset, -notification.TextSize.Y / 2f);
				GUIFont font = notification.Font;
				string sanitizedValue = notification.Text.SanitizedValue;
				Vector2 position = textPos;
				Color white = Color.White;
				float rotation = 0f;
				Vector2 zero = Vector2.Zero;
				float scale = 1f;
				SpriteEffects spriteEffects = SpriteEffects.None;
				float layerDepth = 0f;
				ImmutableArray<RichTextData>? richTextData = notification.Text.RichTextData;
				font.DrawStringWithColors(spriteBatch, sanitizedValue, position, white, rotation, zero, scale, spriteEffects, layerDepth, richTextData, 0, Alignment.TopLeft, ForceUpperCase.Inherit);
				int margin = container.Rect.Width / 5;
				notification.IsCurrentlyVisible = (textPos.X < (float)(container.Rect.Right - margin) && textPos.X + notification.TextSize.X > (float)(container.Rect.X + margin));
			}
		}

		// Token: 0x06001F68 RID: 8040 RVA: 0x00136D08 File Offset: 0x00134F08
		private void UpdateNotifications(float deltaTime, GUICustomComponent mapContainer)
		{
			if (this.mapNotifications.Count < 5)
			{
				int maxIndex = 1;
				while (TextManager.ContainsTag("randomnews" + maxIndex.ToString()))
				{
					maxIndex++;
				}
				string textTag = "randomnews" + Rand.Range(0, maxIndex, Rand.RandSync.Unsynced).ToString();
				if (TextManager.ContainsTag(textTag))
				{
					this.mapNotifications.Add(new Map.MapNotification(TextManager.Get(textTag).Value, GUIStyle.SubHeadingFont, this.mapNotifications, null));
				}
			}
			for (int i = this.mapNotifications.Count - 1; i >= 0; i--)
			{
				Map.MapNotification notification = this.mapNotifications[i];
				notification.Offset -= deltaTime * 75f;
				if (notification.Offset < -notification.TextSize.X - (float)mapContainer.Rect.Width)
				{
					notification.Offset = Math.Max(this.mapNotifications.Max((Map.MapNotification n) => n.Offset + n.TextSize.X) + (float)GUI.IntScale(60f), 0f);
					notification.TimesShown++;
					if (this.mapNotifications.Count > 5)
					{
						this.mapNotifications.RemoveAt(i);
					}
					else if (this.mapNotifications.Count > 3 && notification.TimesShown > 2)
					{
						this.mapNotifications.RemoveAt(i);
					}
				}
			}
		}

		// Token: 0x06001F69 RID: 8041 RVA: 0x00136E8C File Offset: 0x0013508C
		private void CreateLocationInfoOverlay(Location location)
		{
			Map.<>c__DisplayClass28_0 CS$<>8__locals1 = new Map.<>c__DisplayClass28_0();
			CS$<>8__locals1.location = location;
			this.locationInfoOverlay = new GUIFrame(new RectTransform(new Point(GUI.IntScale(350f), GUI.IntScale(350f)), GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false), "GUIToolTip", null)
			{
				UserData = CS$<>8__locals1.location
			};
			this.locationInfoOverlay.Color *= 0.8f;
			CS$<>8__locals1.content = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.85f), this.locationInfoOverlay.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			bool flag = this.hudVisibility > 0f && CS$<>8__locals1.location.Type.HasOutpost && CS$<>8__locals1.location.Reputation != null;
			Identifier overrideDescriptionIdentifier;
			LocationType locationTypeToDisplay = CS$<>8__locals1.location.GetLocationTypeToDisplay(out overrideDescriptionIdentifier);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = CS$<>8__locals1.location.DisplayName;
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null).Padding = Vector4.Zero;
			if (!CS$<>8__locals1.location.Type.Name.IsNullOrEmpty())
			{
				RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = locationTypeToDisplay.Name;
				font = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null).Padding = Vector4.Zero;
			}
			CS$<>8__locals1.<CreateLocationInfoOverlay>g__CreateSpacing|1(10);
			LocalizedString description = locationTypeToDisplay.Description;
			if (!overrideDescriptionIdentifier.IsEmpty)
			{
				description = TextManager.Get(overrideDescriptionIdentifier);
			}
			if (!description.IsNullOrEmpty())
			{
				CS$<>8__locals1.<CreateLocationInfoOverlay>g__CreateTextWithIcon|0(description, locationTypeToDisplay.Sprite, null);
			}
			int highestSubTier = CS$<>8__locals1.location.HighestSubmarineTierAvailable(SubmarineClass.Undefined);
			List<ValueTuple<SubmarineClass, int>> overrideTiers = null;
			if (CS$<>8__locals1.location.CanHaveSubsForSale())
			{
				overrideTiers = new List<ValueTuple<SubmarineClass, int>>();
				foreach (object obj in Enum.GetValues(typeof(SubmarineClass)))
				{
					SubmarineClass subClass = (SubmarineClass)obj;
					if (subClass != SubmarineClass.Undefined)
					{
						int highestClassTier = CS$<>8__locals1.location.HighestSubmarineTierAvailable(subClass);
						if (highestClassTier > 0 && highestClassTier > highestSubTier)
						{
							overrideTiers.Add(new ValueTuple<SubmarineClass, int>(subClass, highestClassTier));
						}
					}
				}
			}
			if (highestSubTier > 0)
			{
				CS$<>8__locals1.<CreateLocationInfoOverlay>g__CreateTextWithIcon|0(TextManager.GetWithVariable("advancedsub.all", "[tiernumber]", highestSubTier.ToString(), FormatCapitals.No), null, "LocationOverlaySubmarineIcon");
			}
			if (overrideTiers != null)
			{
				foreach (ValueTuple<SubmarineClass, int> valueTuple in overrideTiers)
				{
					SubmarineClass subClass2 = valueTuple.Item1;
					int tier = valueTuple.Item2;
					Map.<>c__DisplayClass28_0 CS$<>8__locals2 = CS$<>8__locals1;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("advancedsub.");
					defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(subClass2);
					CS$<>8__locals2.<CreateLocationInfoOverlay>g__CreateTextWithIcon|0(TextManager.GetWithVariable(defaultInterpolatedStringHandler.ToStringAndClear(), "[tiernumber]", tier.ToString(), FormatCapitals.No), null, "LocationOverlaySubmarineIcon");
				}
			}
			CS$<>8__locals1.<CreateLocationInfoOverlay>g__CreateSpacing|1(10);
			if (CS$<>8__locals1.location.Faction != null)
			{
				RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				string tag = "reputationgainnotification";
				ValueTuple<string, string>[] array = new ValueTuple<string, string>[2];
				array[0] = new ValueTuple<string, string>("[value]", string.Empty);
				int num = 1;
				string item = "[reputationname]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler2.AppendFormatted(CS$<>8__locals1.location.Faction.Prefab.IconColor.ToStringHex());
				defaultInterpolatedStringHandler2.AppendLiteral("‖");
				defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(CS$<>8__locals1.location.Faction.Prefab.Name);
				defaultInterpolatedStringHandler2.AppendLiteral("‖end‖");
				array[num] = new ValueTuple<string, string>(item, defaultInterpolatedStringHandler2.ToStringAndClear());
				new GUITextBlock(rectT3, RichString.Rich(TextManager.GetWithVariables(tag, array), null), null, null, Alignment.Left, false, "", null).Padding = Vector4.Zero;
				CS$<>8__locals1.<CreateLocationInfoOverlay>g__CreateSpacing|1(10);
				GUILayoutGroup repBarHolder = new GUILayoutGroup(new RectTransform(new Point(CS$<>8__locals1.content.Rect.Width, GUI.IntScale(25f)), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.05f
				};
				new GUICustomComponent(new RectTransform(new Vector2(0.6f, 1f), repBarHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent component)
				{
					if (CS$<>8__locals1.location.Reputation == null)
					{
						return;
					}
					RoundSummary.DrawReputationBar(sb, component.Rect, CS$<>8__locals1.location.Reputation.NormalizedValue, (float)CS$<>8__locals1.location.Reputation.MinReputation, (float)CS$<>8__locals1.location.Reputation.MaxReputation);
				}, null);
				new GUITextBlock(new RectTransform(new Vector2(0.4f, 1f), repBarHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), CS$<>8__locals1.location.Reputation.GetFormattedReputationText(false), null, null, Alignment.CenterRight, false, "", null);
				new GUIImage(new RectTransform(new Vector2(0.25f, 0.5f), this.locationInfoOverlay.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0.05f)
				}, CS$<>8__locals1.location.Faction.Prefab.Icon, true, null).Color = CS$<>8__locals1.location.Faction.Prefab.IconColor * 0.5f;
				CS$<>8__locals1.<CreateLocationInfoOverlay>g__CreateSpacing|1(20);
			}
			float childWidth = Math.Max((float)this.locationInfoOverlay.Rect.Width, CS$<>8__locals1.content.Children.Max(delegate(GUIComponent c)
			{
				GUITextBlock textBlock = c as GUITextBlock;
				if (textBlock == null)
				{
					return (float)c.RectTransform.MinSize.X;
				}
				return textBlock.TextSize.X + textBlock.Padding.X + textBlock.Padding.Z;
			}));
			childWidth = Math.Max((float)this.locationInfoOverlay.Rect.Width, childWidth);
			float childHeight = (float)CS$<>8__locals1.content.Children.Sum((GUIComponent c) => c.Rect.Height);
			Vector2 childSize = new Vector2(childWidth, childHeight) / CS$<>8__locals1.content.RectTransform.RelativeSize;
			this.locationInfoOverlay.RectTransform.NonScaledSize = childSize.ToPoint();
		}

		// Token: 0x06001F6A RID: 8042 RVA: 0x0013762C File Offset: 0x0013582C
		public void Update(CampaignMode campaign, float deltaTime, GUICustomComponent mapContainer)
		{
			Rectangle rect = mapContainer.Rect;
			this.UpdateNotifications(deltaTime, mapContainer);
			Location currentDisplayLocation = (campaign != null) ? campaign.GetCurrentDisplayLocation() : null;
			if (currentDisplayLocation != null && !currentDisplayLocation.Discovered)
			{
				this.RemoveFogOfWar(currentDisplayLocation, true);
				this.Discover(currentDisplayLocation, true);
				if (currentDisplayLocation.MapPosition.X > this.furthestDiscoveredLocation.MapPosition.X)
				{
					this.furthestDiscoveredLocation = currentDisplayLocation;
				}
			}
			Vector2 currentPosition = currentDisplayLocation.MapPosition;
			Level loaded = Level.Loaded;
			if (loaded != null && loaded.Type == LevelData.LevelType.LocationConnection && Level.Loaded.StartLocation != null && Level.Loaded.EndLocation != null)
			{
				Vector2 startPos = (currentDisplayLocation == Level.Loaded.StartLocation) ? Level.Loaded.StartLocation.MapPosition : Level.Loaded.EndLocation.MapPosition;
				int moveDir = (currentDisplayLocation == Level.Loaded.StartLocation) ? 1 : -1;
				Vector2 diff = Level.Loaded.EndLocation.MapPosition - Level.Loaded.StartLocation.MapPosition;
				currentPosition = startPos + Vector2.Normalize(diff) * Math.Min(100f, diff.Length() * 0.2f) * (float)moveDir;
			}
			else
			{
				currentPosition += Vector2.UnitY * 35f;
			}
			this.currLocationIndicatorPos = Vector2.Lerp(this.currLocationIndicatorPos, currentPosition, deltaTime);
			Radiation radiation = this.Radiation;
			if (radiation != null)
			{
				radiation.MapUpdate(deltaTime);
			}
			if (this.mapAnimQueue.Count > 0)
			{
				this.hudVisibility = Math.Max(this.hudVisibility - deltaTime, 0f);
				this.UpdateMapAnim(this.mapAnimQueue.Peek(), deltaTime);
				if (this.mapAnimQueue.Peek().Finished)
				{
					this.mapAnimQueue.Dequeue();
				}
				return;
			}
			this.hudVisibility = Math.Min(this.hudVisibility + deltaTime, 0.75f + (float)Math.Sin(Timing.TotalTime * 3.0) * 0.25f);
			Vector2 rectCenter = new Vector2((float)rect.Center.X, (float)rect.Center.Y);
			Vector2 viewOffset = this.DrawOffset + this.drawOffsetNoise;
			if (this.HighlightedLocation != null)
			{
				Vector2 highlightedLocationDrawPos = rectCenter + (this.HighlightedLocation.MapPosition + viewOffset) * this.zoom;
				if (this.locationInfoOverlay == null || this.locationInfoOverlay.UserData != this.HighlightedLocation)
				{
					this.CreateLocationInfoOverlay(this.HighlightedLocation);
				}
				Point offsetFromLocationIcon = new Point(GUI.IntScale(25f));
				RectTransform locationInfoRt = this.locationInfoOverlay.RectTransform;
				if (locationInfoRt.Pivot == Pivot.BottomLeft || locationInfoRt.Pivot == Pivot.BottomRight)
				{
					offsetFromLocationIcon.Y = -offsetFromLocationIcon.Y;
				}
				if (locationInfoRt.Pivot == Pivot.TopRight || locationInfoRt.Pivot == Pivot.BottomRight)
				{
					offsetFromLocationIcon.X = -offsetFromLocationIcon.X;
				}
				locationInfoRt.ScreenSpaceOffset = highlightedLocationDrawPos.ToPoint() + offsetFromLocationIcon;
				if (this.locationInfoOverlay.Rect.Bottom > rect.Bottom)
				{
					locationInfoRt.Pivot = Pivot.BottomLeft;
				}
				if (this.locationInfoOverlay.Rect.Right > rect.Right)
				{
					locationInfoRt.Pivot = ((locationInfoRt.Pivot == Pivot.TopLeft) ? Pivot.TopRight : Pivot.BottomRight);
				}
				GUIComponent guicomponent = this.locationInfoOverlay;
				if (guicomponent != null)
				{
					guicomponent.AddToGUIUpdateList(false, 1);
				}
			}
			float closestDist = 0f;
			this.HighlightedLocation = null;
			if (GUI.MouseOn == null || GUI.MouseOn == mapContainer)
			{
				int i = 0;
				while (i < this.Locations.Count)
				{
					Location location = this.Locations[i];
					if (!this.IsInFogOfWar(location))
					{
						goto IL_429;
					}
					Location currentDisplayLocation2 = currentDisplayLocation;
					if ((currentDisplayLocation2 != null && currentDisplayLocation2.Connections.Any((LocationConnection c) => c.Locations.Contains(location))) || GameMain.DebugDraw)
					{
						goto IL_429;
					}
					IL_58F:
					i++;
					continue;
					IL_429:
					Vector2 pos = rectCenter + (location.MapPosition + viewOffset) * this.zoom;
					if (!rect.Contains(pos))
					{
						goto IL_58F;
					}
					Sprite locationSprite = location.IsCriticallyRadiated() ? (location.Type.RadiationSprite ?? location.Type.Sprite) : location.Type.Sprite;
					float iconScale = this.generationParams.LocationIconSize / locationSprite.size.X;
					if (location == currentDisplayLocation)
					{
						iconScale *= 1.2f;
					}
					Rectangle drawRect = locationSprite.SourceRect;
					drawRect.Width = (int)((float)drawRect.Width * iconScale * this.zoom * 1.4f);
					drawRect.Height = (int)((float)drawRect.Height * iconScale * this.zoom * 1.4f);
					drawRect.X = (int)pos.X - drawRect.Width / 2;
					drawRect.Y = (int)pos.Y - drawRect.Width / 2;
					if (!drawRect.Contains(PlayerInput.MousePosition))
					{
						goto IL_58F;
					}
					float dist = Vector2.Distance(PlayerInput.MousePosition, pos);
					if (this.HighlightedLocation == null || dist < closestDist)
					{
						closestDist = dist;
						this.HighlightedLocation = location;
						goto IL_58F;
					}
					goto IL_58F;
				}
			}
			if (this.SelectedConnection != null)
			{
				this.connectionHighlightState = Math.Min(this.connectionHighlightState + deltaTime, 1f);
			}
			else
			{
				this.connectionHighlightState = 0f;
			}
			if (GUI.KeyboardDispatcher.Subscriber == null)
			{
				float moveSpeed = 1000f;
				Vector2 moveAmount = Vector2.Zero;
				if (PlayerInput.KeyDown(InputType.Left))
				{
					moveAmount += Vector2.UnitX;
				}
				if (PlayerInput.KeyDown(InputType.Right))
				{
					moveAmount -= Vector2.UnitX;
				}
				if (PlayerInput.KeyDown(InputType.Up))
				{
					moveAmount += Vector2.UnitY;
				}
				if (PlayerInput.KeyDown(InputType.Down))
				{
					moveAmount -= Vector2.UnitY;
				}
				this.DrawOffset += moveAmount * moveSpeed / this.zoom * deltaTime;
			}
			this.targetZoom = MathHelper.Clamp(this.targetZoom, this.generationParams.MinZoom, this.generationParams.MaxZoom);
			this.zoom = MathHelper.Lerp(this.zoom, this.targetZoom * GUI.Scale, 0.1f);
			if (GUI.MouseOn == mapContainer)
			{
				foreach (LocationConnection connection in this.Connections)
				{
					if (this.HighlightedLocation != currentDisplayLocation && connection.Locations.Contains(this.HighlightedLocation) && connection.Locations.Contains(currentDisplayLocation) && PlayerInput.PrimaryMouseButtonClicked() && this.SelectedLocation != this.HighlightedLocation && this.HighlightedLocation != null)
					{
						if (connection.Locked)
						{
							new GUIMessageBox(string.Empty, TextManager.Get("LockedPathTooltip"), null, null, GUIMessageBox.Type.Default);
						}
						else if (CampaignMode.AllowedToManageCampaign(ClientPermissions.ManageMap))
						{
							this.connectionHighlightState = 0f;
							this.SelectedConnection = connection;
							this.SelectedLocation = this.HighlightedLocation;
							Action<Location, LocationConnection> onLocationSelected = this.OnLocationSelected;
							if (onLocationSelected != null)
							{
								onLocationSelected(this.SelectedLocation, this.SelectedConnection);
							}
							GameClient client = GameMain.Client;
							if (client != null)
							{
								client.SendCampaignState();
							}
						}
					}
				}
				this.targetZoom += (float)PlayerInput.ScrollWheelSpeed / 500f;
				if (PlayerInput.MidButtonHeld() || (this.HighlightedLocation == null && PlayerInput.PrimaryMouseButtonHeld()))
				{
					this.DrawOffset += PlayerInput.MouseSpeed / this.zoom;
				}
				if (this.AllowDebugTeleport)
				{
					if (PlayerInput.DoubleClicked() && this.HighlightedLocation != null)
					{
						LocationConnection passedConnection = currentDisplayLocation.Connections.Find((LocationConnection c) => c.OtherLocation(currentDisplayLocation) == this.HighlightedLocation);
						if (passedConnection != null)
						{
							passedConnection.Passed = true;
						}
						Location prevLocation = currentDisplayLocation;
						this.CurrentLocation = this.HighlightedLocation;
						Level.Loaded.DebugSetStartLocation(this.CurrentLocation);
						Level.Loaded.DebugSetEndLocation(null);
						this.Discover(this.CurrentLocation, true);
						this.Visit(this.CurrentLocation, true);
						NamedEvent<Map.LocationChangeInfo> onLocationChanged = this.OnLocationChanged;
						if (onLocationChanged != null)
						{
							onLocationChanged.Invoke(new Map.LocationChangeInfo(prevLocation, this.CurrentLocation));
						}
						this.SelectLocation(-1);
						if (GameMain.Client == null)
						{
							this.CurrentLocation.CreateStores(false);
							this.ProgressWorld(campaign);
							Radiation radiation2 = this.Radiation;
							if (radiation2 != null)
							{
								radiation2.OnStep(1f);
							}
							this.mapAnimQueue.Clear();
						}
						else
						{
							GameMain.Client.SendCampaignState();
						}
					}
					if (PlayerInput.PrimaryMouseButtonClicked() && this.HighlightedLocation == null)
					{
						this.SelectLocation(-1);
					}
				}
			}
		}

		// Token: 0x06001F6B RID: 8043 RVA: 0x00137FB0 File Offset: 0x001361B0
		public void Draw(CampaignMode campaign, SpriteBatch spriteBatch, GUICustomComponent mapContainer)
		{
			this.tooltip = null;
			Location currentDisplayLocation = (campaign != null) ? campaign.GetCurrentDisplayLocation() : null;
			Rectangle rect = mapContainer.Rect;
			Vector2 viewSize = new Vector2((float)rect.Width / this.zoom, (float)rect.Height / this.zoom);
			Vector2 edgeBuffer = new Vector2((float)rect.Width * 0.05f);
			this.DrawOffset.X = MathHelper.Clamp(this.DrawOffset.X, (float)(-(float)this.Width) - edgeBuffer.X + viewSize.X / 2f, edgeBuffer.X - viewSize.X / 2f);
			this.DrawOffset.Y = MathHelper.Clamp(this.DrawOffset.Y, (float)(-(float)this.Height) - edgeBuffer.Y + viewSize.Y / 2f, edgeBuffer.Y - viewSize.Y / 2f);
			this.drawOffsetNoise = new Vector2((float)PerlinNoise.CalculatePerlin(Timing.TotalTime * 0.10000000149011612 % 255.0, Timing.TotalTime * 0.10000000149011612 % 255.0, 0.0) - 0.5f, (float)PerlinNoise.CalculatePerlin(Timing.TotalTime * 0.20000000298023224 % 255.0, Timing.TotalTime * 0.20000000298023224 % 255.0, 0.5) - 0.5f) * 10f;
			Vector2 viewOffset = this.DrawOffset + this.drawOffsetNoise;
			Vector2 rectCenter = new Vector2((float)rect.Center.X, (float)rect.Center.Y);
			float missionIconScale = (this.generationParams.MissionIcon != null) ? (18f / (float)this.generationParams.MissionIcon.SourceRect.Width) : 1f;
			Rectangle prevScissorRect = GameMain.Instance.GraphicsDevice.ScissorRectangle;
			spriteBatch.End();
			spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, rect);
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			Vector2 topLeft = rectCenter + viewOffset - rect.Location.ToVector2();
			Vector2 bottomRight = topLeft + new Vector2((float)this.Width, (float)this.Height);
			Vector2 mapTileSize = this.mapTiles[0, 0].size * this.generationParams.MapTileScale;
			int startX = (int)Math.Floor((double)(-(double)topLeft.X / mapTileSize.X)) - 1;
			int startY = (int)Math.Floor((double)(-(double)topLeft.Y / mapTileSize.Y)) - 1;
			int endX = (int)Math.Ceiling((double)((-(double)topLeft.X + (float)rect.Width) / mapTileSize.X));
			int endY = (int)Math.Ceiling((double)((-(double)topLeft.Y + (float)rect.Height) / mapTileSize.Y));
			float noiseT = (float)(Timing.TotalTime * 0.009999999776482582);
			this.cameraNoiseStrength = (float)PerlinNoise.CalculatePerlin((double)noiseT, (double)(noiseT * 0.5f), (double)(noiseT * 0.2f));
			float noiseScale = (float)PerlinNoise.CalculatePerlin((double)(noiseT * 5f), (double)(noiseT * 2f), 0.0) * 5f;
			for (int x = startX; x <= endX; x++)
			{
				for (int y = startY; y <= endY; y++)
				{
					int tileX = Math.Abs(x) % this.mapTiles.GetLength(0);
					int tileY = Math.Abs(y) % this.mapTiles.GetLength(1);
					Vector2 tilePos = rectCenter + (viewOffset + new Vector2((float)x, (float)y) * mapTileSize) * this.zoom;
					this.mapTiles[tileX, tileY].Draw(spriteBatch, tilePos, Color.White, Vector2.Zero, 0f, this.generationParams.MapTileScale * this.zoom, SpriteEffects.None, null);
					if (!GameMain.DebugDraw && (!this.tileDiscovered[tileX, tileY] || x < 0 || y < 0 || x >= this.tileDiscovered.GetLength(0) || y >= this.tileDiscovered.GetLength(1)))
					{
						Sprite fogOfWarSprite = this.generationParams.FogOfWarSprite;
						if (fogOfWarSprite != null)
						{
							fogOfWarSprite.Draw(spriteBatch, tilePos, Color.White * this.cameraNoiseStrength, Vector2.Zero, 0f, this.generationParams.MapTileScale * this.zoom, SpriteEffects.None, null);
						}
						Sprite sprite = Map.noiseOverlay;
						Vector2 position = tilePos;
						Vector2 targetSize = mapTileSize * this.zoom;
						float rotation = 0f;
						Vector2? startOffset = new Vector2?(new Vector2(Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Width, Rand.RandSync.Unsynced), Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Height, Rand.RandSync.Unsynced)));
						Color? color2 = new Color?(Color.White * this.cameraNoiseStrength * 0.2f);
						Vector2? textureScale = new Vector2?(Vector2.One * noiseScale);
						sprite.DrawTiled(spriteBatch, position, targetSize, rotation, null, color2, startOffset, textureScale, null);
					}
				}
			}
			if (GameMain.DebugDraw)
			{
				if (topLeft.X > (float)rect.X)
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle(rect.X, rect.Y, (int)(topLeft.X - (float)rect.X), rect.Height), Color.Black * 0.5f, true, 0f, 1f);
				}
				if (topLeft.Y > (float)rect.Y)
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)topLeft.X, rect.Y, (int)(bottomRight.X - topLeft.X), (int)(topLeft.Y - (float)rect.Y)), Color.Black * 0.5f, true, 0f, 1f);
				}
				if (bottomRight.X < (float)rect.Right)
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)bottomRight.X, rect.Y, (int)((float)rect.Right - bottomRight.X), rect.Height), Color.Black * 0.5f, true, 0f, 1f);
				}
				if (bottomRight.Y < (float)rect.Bottom)
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)topLeft.X, (int)bottomRight.Y, (int)(bottomRight.X - topLeft.X), (int)((float)rect.Bottom - bottomRight.Y)), Color.Black * 0.5f, true, 0f, 1f);
				}
			}
			float rawNoiseScale = 1f + PerlinNoise.GetPerlin((float)((int)(Timing.TotalTime * 1.0 - 1.0)), (float)((int)(Timing.TotalTime * 1.0 - 1.0)));
			Map.DrawNoise(spriteBatch, rect, rawNoiseScale);
			Radiation radiation = this.Radiation;
			if (radiation != null)
			{
				radiation.Draw(spriteBatch, rect, this.zoom);
			}
			if (this.generationParams.ShowLocations)
			{
				foreach (LocationConnection connection in this.Connections)
				{
					if (!this.IsInFogOfWar(connection.Locations[0]) || !this.IsInFogOfWar(connection.Locations[1]))
					{
						LocationConnection connection2 = connection;
						Rectangle viewArea = rect;
						Vector2 viewOffset2 = viewOffset;
						Location currentDisplayLocation2 = currentDisplayLocation;
						Color? color2 = null;
						this.DrawConnection(spriteBatch, connection2, viewArea, viewOffset2, currentDisplayLocation2, color2);
					}
				}
				Predicate<LocationConnection> <>9__1;
				for (int i = 0; i < this.Locations.Count; i++)
				{
					Location location = this.Locations[i];
					if (location.Discovered || !this.IsInFogOfWar(location))
					{
						bool isEndLocation = this.endLocations.Contains(location);
						if (GameMain.DebugDraw || !isEndLocation || location == this.endLocations.First<Location>())
						{
							Vector2 pos = rectCenter + (location.MapPosition + viewOffset) * this.zoom;
							Sprite locationSprite = location.IsCriticallyRadiated() ? (location.Type.RadiationSprite ?? location.Type.Sprite) : location.Type.Sprite;
							Rectangle drawRect = locationSprite.SourceRect;
							drawRect.X = (int)pos.X - drawRect.Width / 2;
							drawRect.Y = (int)pos.Y - drawRect.Width / 2;
							if (drawRect.X > rect.Right - GUI.IntScale(100f) && this.generationParams.MissionIcon != null)
							{
								if (location.AvailableAndVisibleMissions.Any((Mission m) => m.Prefab.ShowInMenus))
								{
									Vector2 offScreenMissionIconPos = new Vector2((float)(rect.Right - GUI.IntScale(50f)), (float)drawRect.Center.Y);
									this.generationParams.MissionIcon.Draw(spriteBatch, offScreenMissionIconPos, this.generationParams.IndicatorColor, 0f, missionIconScale * this.zoom, SpriteEffects.None, null);
									GUI.Arrow.Draw(spriteBatch, offScreenMissionIconPos + Vector2.UnitX * this.generationParams.MissionIcon.size.X * missionIconScale * this.zoom, this.generationParams.IndicatorColor, 1.5707964f, 0.5f, SpriteEffects.None, null);
								}
							}
							if (rect.Intersects(drawRect))
							{
								Color? color2 = location.OverrideIconColor;
								Color color = color2 ?? location.Type.SpriteColor;
								if (!location.Visited)
								{
									color = Color.White;
								}
								List<LocationConnection> connections = location.Connections;
								Predicate<LocationConnection> match;
								if ((match = <>9__1) == null)
								{
									match = (<>9__1 = ((LocationConnection c) => c.Locations.Contains(currentDisplayLocation)));
								}
								if (connections.Find(match) == null)
								{
									color *= 0.5f;
								}
								float iconScale = (location == currentDisplayLocation) ? 1.2f : 1f;
								if (location == this.HighlightedLocation)
								{
									iconScale *= 1.2f;
								}
								if (isEndLocation)
								{
									iconScale *= 2f;
								}
								float notificationPulseAmount = 1f;
								float notificationColorLerp = 0f;
								if (this.mapNotifications.Any((Map.MapNotification n) => n.RelatedLocation == location && n.IsCurrentlyVisible))
								{
									float sin = MathF.Sin((float)Timing.TotalTime * 2f);
									notificationPulseAmount = Math.Max(sin + 0.5f, 1f);
									notificationColorLerp = (notificationPulseAmount - 1f) * 4f;
									color = Color.Lerp(color, GUIStyle.Yellow, notificationColorLerp);
									iconScale *= notificationPulseAmount;
								}
								locationSprite.Draw(spriteBatch, pos, color, 0f, this.generationParams.LocationIconSize / locationSprite.size.X * iconScale * this.zoom, SpriteEffects.None, null);
								if (location.Faction != null)
								{
									float factionIconScale = iconScale * 0.7f;
									Sprite factionIcon = location.Faction.Prefab.IconSmall ?? location.Faction.Prefab.Icon;
									Color factionIconColor = Color.Lerp(color, location.Faction.Prefab.IconColor, notificationColorLerp);
									factionIcon.Draw(spriteBatch, pos + new Vector2(-15f, 15f) * this.zoom, factionIconColor, 0f, this.generationParams.LocationIconSize / factionIcon.size.X * factionIconScale * this.zoom, SpriteEffects.None, null);
								}
								if (location == currentDisplayLocation)
								{
									if (this.SelectedLocation != null)
									{
										Vector2 dir = Vector2.Normalize(this.SelectedLocation.MapPosition - this.currLocationIndicatorPos);
										GUI.Arrow.Draw(spriteBatch, rectCenter + (this.currLocationIndicatorPos + viewOffset) * this.zoom + dir * this.generationParams.LocationIconSize * 0.6f * this.zoom, this.generationParams.IndicatorColor, GUI.Arrow.Origin, MathUtils.VectorToAngle(dir) + 1.5707964f, new Vector2(0.5f, 1f) * this.zoom, SpriteEffects.None, null);
									}
									this.generationParams.CurrentLocationIndicator.Draw(spriteBatch, rectCenter + (this.currLocationIndicatorPos + viewOffset) * this.zoom, this.generationParams.IndicatorColor, this.generationParams.CurrentLocationIndicator.Origin, 0f, Vector2.One * (this.generationParams.LocationIconSize / this.generationParams.CurrentLocationIndicator.size.X) * 0.8f * this.zoom, SpriteEffects.None, null);
								}
								if (location == this.SelectedLocation)
								{
									this.generationParams.SelectedLocationIndicator.Draw(spriteBatch, rectCenter + (location.MapPosition + viewOffset) * this.zoom, this.generationParams.IndicatorColor, this.generationParams.SelectedLocationIndicator.Origin, 0f, Vector2.One * (this.generationParams.LocationIconSize / this.generationParams.SelectedLocationIndicator.size.X) * 1.7f * this.zoom, SpriteEffects.None, null);
								}
								if (location.TimeSinceLastTypeChange < 1 && !string.IsNullOrEmpty(location.LastTypeChangeMessage) && this.generationParams.TypeChangeIcon != null)
								{
									Vector2 typeChangeIconPos = pos + new Vector2(1.35f, -0.35f) * this.generationParams.LocationIconSize * 0.5f * this.zoom;
									float typeChangeIconScale = 18f / (float)this.generationParams.TypeChangeIcon.SourceRect.Width;
									Color iconColor = GUIStyle.Red;
									color = Color.Lerp(color, GUIStyle.Yellow, notificationColorLerp);
									iconScale *= notificationPulseAmount;
									this.generationParams.TypeChangeIcon.Draw(spriteBatch, typeChangeIconPos, iconColor, 0f, typeChangeIconScale * this.zoom, SpriteEffects.None, null);
									if (Vector2.Distance(PlayerInput.MousePosition, typeChangeIconPos) < (float)this.generationParams.TypeChangeIcon.SourceRect.Width * this.zoom && (this.tooltip == null || this.IsPreferredTooltip(typeChangeIconPos)))
									{
										this.tooltip = new ValueTuple<Rectangle, RichString>?(new ValueTuple<Rectangle, RichString>(new Rectangle(typeChangeIconPos.ToPoint(), new Point(30)), RichString.Rich(location.LastTypeChangeMessage, null)));
									}
								}
								if (location != this.CurrentLocation && this.generationParams.MissionIcon != null)
								{
									IEnumerable<Mission> currentLocationVisibleMissions = this.CurrentLocation.AvailableAndVisibleMissions;
									if (this.CurrentLocation != currentDisplayLocation || !currentLocationVisibleMissions.Any((Mission m) => m.Locations.Contains(location)))
									{
										if (!location.AvailableAndVisibleMissions.Any((Mission m) => m.Locations[0] == m.Locations[1]))
										{
											goto IL_1228;
										}
									}
									Vector2 missionIconPos = pos + new Vector2(1.35f, 0.35f) * this.generationParams.LocationIconSize * 0.5f * this.zoom;
									this.generationParams.MissionIcon.Draw(spriteBatch, missionIconPos, this.generationParams.IndicatorColor, 0f, missionIconScale * this.zoom, SpriteEffects.None, null);
									if (Vector2.Distance(PlayerInput.MousePosition, missionIconPos) < (float)this.generationParams.MissionIcon.SourceRect.Width * this.zoom && this.IsPreferredTooltip(missionIconPos))
									{
										IEnumerable<Mission> allVisibleMissions = (from m in currentLocationVisibleMissions
										where m.Locations.Contains(location)
										select m).Concat(from m in location.AvailableAndVisibleMissions
										where m.Locations[0] == m.Locations[1]
										select m).Distinct<Mission>();
										this.tooltip = new ValueTuple<Rectangle, RichString>?(new ValueTuple<Rectangle, RichString>(new Rectangle(missionIconPos.ToPoint(), new Point(30)), TextManager.Get("mission") + '\n' + string.Join<LocalizedString>('\n', from m in allVisibleMissions
										select "- " + m.Name)));
									}
								}
								IL_1228:
								if (GameMain.DebugDraw)
								{
									Vector2 dPos = pos + new Vector2(15f, -100f);
									if (location == this.HighlightedLocation)
									{
										Vector2 pos2 = dPos;
										LocalizedString left = "Faction: ";
										Faction faction = location.Faction;
										GUI.DrawString(spriteBatch, pos2, left + (((faction != null) ? faction.Prefab.Name : null) ?? "none"), Color.White, new Color?(Color.Black), 0, GUIStyle.SubHeadingFont, ForceUpperCase.Inherit);
										Vector2 pos3 = dPos + new Vector2(0f, 18f);
										LocalizedString left2 = "Secondary Faction: ";
										Faction secondaryFaction = location.SecondaryFaction;
										GUI.DrawString(spriteBatch, pos3, left2 + (((secondaryFaction != null) ? secondaryFaction.Prefab.Name : null) ?? "none"), Color.White, new Color?(Color.Black), 0, GUIStyle.SubHeadingFont, ForceUpperCase.Inherit);
										dPos.Y += 50f;
										if (PlayerInput.KeyDown(Keys.LeftShift))
										{
											GUI.DrawString(spriteBatch, new Vector2(150f, 150f), "Dist: " + Map.GetDistanceToClosestLocationOrConnection(this.CurrentLocation, int.MaxValue, (Location loc) => loc == location, null).ToString(), Color.White, new Color?(Color.Black), 0, GUIStyle.SubHeadingFont, ForceUpperCase.Inherit);
										}
										GUI.DrawString(spriteBatch, dPos, "Difficulty: " + location.LevelData.Difficulty.FormatSingleDecimal(), ToolBox.GradientLerp(location.LevelData.Difficulty / 100f, new Color[]
										{
											GUIStyle.Blue,
											GUIStyle.Yellow,
											GUIStyle.Red
										}), new Color?(Color.Black * 0.8f), 4, GUIStyle.SmallFont, ForceUpperCase.Inherit);
										dPos.Y += 25f;
										Vector2 pos4 = dPos;
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 2);
										defaultInterpolatedStringHandler.AppendLiteral("Biome: ");
										defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(location.LevelData.Biome.DisplayName);
										defaultInterpolatedStringHandler.AppendLiteral(" (");
										defaultInterpolatedStringHandler.AppendFormatted<Identifier>(location.LevelData.GenerationParams.Identifier);
										defaultInterpolatedStringHandler.AppendLiteral(")");
										GUI.DrawString(spriteBatch, pos4, defaultInterpolatedStringHandler.ToStringAndClear(), Color.White, new Color?(Color.Black), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
									}
								}
							}
						}
					}
				}
			}
			this.DrawDecorativeHUD(spriteBatch, rect);
			bool drawRadiationTooltip = this.HighlightedLocation == null;
			if (this.tooltip != null)
			{
				GUIComponent.DrawToolTip(spriteBatch, this.tooltip.Value.Item2, this.tooltip.Value.Item1, Anchor.BottomCenter, Pivot.TopLeft);
				drawRadiationTooltip = false;
			}
			if (drawRadiationTooltip)
			{
				Radiation radiation2 = this.Radiation;
				if (radiation2 != null)
				{
					radiation2.DrawFront(spriteBatch);
				}
			}
			spriteBatch.End();
			GameMain.Instance.GraphicsDevice.ScissorRectangle = prevScissorRect;
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
		}

		// Token: 0x06001F6C RID: 8044 RVA: 0x00139548 File Offset: 0x00137748
		public static void DrawNoise(SpriteBatch spriteBatch, Rectangle rect, float strength)
		{
			if (Map.noiseOverlay == null)
			{
				Map.noiseOverlay = new Sprite("Content/UI/noise.png", Vector2.Zero);
			}
			float noiseT = (float)(Timing.TotalTime * 0.009999999776482582);
			float noiseScale = (float)PerlinNoise.CalculatePerlin((double)(noiseT * 5f), (double)(noiseT * 2f), 0.0) * 5f;
			float rawNoiseScale = 1f + Map.GetPerlinNoise();
			Sprite sprite = Map.noiseOverlay;
			Vector2 position = rect.Location.ToVector2();
			Vector2 targetSize = rect.Size.ToVector2();
			float rotation = 0f;
			Vector2? vector = new Vector2?(new Vector2(Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Width, Rand.RandSync.Unsynced), Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Height, Rand.RandSync.Unsynced)));
			Color? color = new Color?(Color.White * strength * 0.1f);
			Vector2? vector2 = new Vector2?(Vector2.One * rawNoiseScale);
			sprite.DrawTiled(spriteBatch, position, targetSize, rotation, null, color, vector, vector2, null);
			Sprite sprite2 = Map.noiseOverlay;
			Vector2 position2 = rect.Location.ToVector2();
			Vector2 targetSize2 = rect.Size.ToVector2();
			float rotation2 = 0f;
			vector2 = new Vector2?(new Vector2(Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Width, Rand.RandSync.Unsynced), Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Height, Rand.RandSync.Unsynced)));
			color = new Color?(new Color(20, 20, 20, 50));
			vector = new Vector2?(Vector2.One * rawNoiseScale * 2f);
			sprite2.DrawTiled(spriteBatch, position2, targetSize2, rotation2, null, color, vector2, vector, null);
			Sprite sprite3 = Map.noiseOverlay;
			Vector2 zero = Vector2.Zero;
			Vector2 targetSize3 = new Vector2((float)GameMain.GraphicsWidth, (float)GameMain.GraphicsHeight);
			float rotation3 = 0f;
			vector = new Vector2?(new Vector2(Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Width, Rand.RandSync.Unsynced), Rand.Range(0f, (float)Map.noiseOverlay.SourceRect.Height, Rand.RandSync.Unsynced)));
			color = new Color?(Color.White * strength * 0.1f);
			vector2 = new Vector2?(Vector2.One * noiseScale);
			sprite3.DrawTiled(spriteBatch, zero, targetSize3, rotation3, null, color, vector, vector2, null);
		}

		// Token: 0x06001F6D RID: 8045 RVA: 0x001397D6 File Offset: 0x001379D6
		private static float GetPerlinNoise()
		{
			return PerlinNoise.GetPerlin((float)((int)(Timing.TotalTime * 1.0 - 1.0)), (float)((int)(Timing.TotalTime * 1.0 - 1.0)));
		}

		// Token: 0x06001F6E RID: 8046 RVA: 0x00139814 File Offset: 0x00137A14
		private void DrawConnection(SpriteBatch spriteBatch, LocationConnection connection, Rectangle viewArea, Vector2 viewOffset, Location currentDisplayLocation, Color? overrideColor = null)
		{
			Map.<>c__DisplayClass33_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			Color connectionColor;
			if (GameMain.DebugDraw)
			{
				float sizeFactor = MathUtils.InverseLerp(this.generationParams.SmallLevelConnectionLength, this.generationParams.LargeLevelConnectionLength, connection.Length);
				connectionColor = ToolBox.GradientLerp(sizeFactor, new Color[]
				{
					Color.LightGreen,
					GUIStyle.Orange,
					GUIStyle.Red
				});
			}
			else if (overrideColor != null)
			{
				connectionColor = overrideColor.Value;
			}
			else
			{
				connectionColor = (connection.Passed ? this.generationParams.ConnectionColor : this.generationParams.UnvisitedConnectionColor);
			}
			int width = (int)(this.generationParams.LocationConnectionWidth * this.zoom);
			Level loaded = Level.Loaded;
			if (((loaded != null) ? loaded.LevelData : null) == connection.LevelData)
			{
				connectionColor = this.generationParams.HighlightedConnectionColor;
				width = (int)((float)width * 1.5f);
			}
			if (this.SelectedLocation != currentDisplayLocation && connection.Locations.Contains(this.SelectedLocation) && connection.Locations.Contains(currentDisplayLocation))
			{
				connectionColor = this.generationParams.HighlightedConnectionColor;
				width *= 2;
			}
			else if (this.HighlightedLocation != currentDisplayLocation && connection.Locations.Contains(this.HighlightedLocation) && connection.Locations.Contains(currentDisplayLocation))
			{
				connectionColor = this.generationParams.HighlightedConnectionColor;
				width *= 2;
			}
			Vector2 rectCenter = viewArea.Center.ToVector2();
			int startIndex = (connection.CrackSegments.Count > 2) ? 1 : 0;
			int endIndex = (connection.CrackSegments.Count > 2) ? (connection.CrackSegments.Count - 1) : connection.CrackSegments.Count;
			CS$<>8__locals1.connectionStart = null;
			CS$<>8__locals1.connectionEnd = null;
			for (int i = startIndex; i < endIndex; i++)
			{
				Vector2[] segment = connection.CrackSegments[i];
				Vector2 start = rectCenter + (segment[0] + viewOffset) * this.zoom;
				if (CS$<>8__locals1.connectionStart == null)
				{
					CS$<>8__locals1.connectionStart = new Vector2?(start);
				}
				Vector2 end = rectCenter + (segment[1] + viewOffset) * this.zoom;
				CS$<>8__locals1.connectionEnd = new Vector2?(end);
				if (viewArea.Contains(start) || viewArea.Contains(end))
				{
					Vector2 intersection;
					if (MathUtils.GetLineWorldRectangleIntersection(start, end, new Rectangle(viewArea.X, viewArea.Y + viewArea.Height, viewArea.Width, viewArea.Height), out intersection))
					{
						if (!viewArea.Contains(start))
						{
							start = intersection;
						}
						else
						{
							end = intersection;
						}
					}
					float a = 1f;
					if (!connection.Locations[0].Visited && !connection.Locations[1].Visited)
					{
						if (this.IsInFogOfWar(connection.Locations[0]))
						{
							a = (float)i / (float)connection.CrackSegments.Count;
						}
						else if (this.IsInFogOfWar(connection.Locations[1]))
						{
							a = 1f - (float)i / (float)connection.CrackSegments.Count;
						}
					}
					float dist = Vector2.Distance(start, end);
					Sprite connectionSprite = connection.Passed ? this.generationParams.PassedConnectionSprite : this.generationParams.ConnectionSprite;
					if (((connectionSprite != null) ? connectionSprite.Texture : null) != null)
					{
						Color segmentColor = connectionColor;
						int segmentWidth = width;
						if (connection == this.SelectedConnection)
						{
							float t = (float)(i - startIndex) / (float)(endIndex - startIndex - 1);
							if (currentDisplayLocation == connection.Locations[1])
							{
								t = 1f - t;
							}
							if (t > this.connectionHighlightState)
							{
								segmentWidth /= 2;
								segmentColor = (connection.Passed ? this.generationParams.ConnectionColor : this.generationParams.UnvisitedConnectionColor);
							}
						}
						CS$<>8__locals1.spriteBatch.Draw(connectionSprite.Texture, new Rectangle((int)start.X, (int)start.Y, (int)(dist - 1f * this.zoom), segmentWidth), new Rectangle?(connectionSprite.SourceRect), segmentColor * a, MathUtils.VectorToAngle(end - start), new Vector2(0f, connectionSprite.size.Y / 2f), SpriteEffects.None, 0.01f);
					}
				}
			}
			CS$<>8__locals1.iconCount = 0;
			CS$<>8__locals1.iconIndex = 0;
			if (CS$<>8__locals1.connectionStart != null && CS$<>8__locals1.connectionEnd != null)
			{
				if (connection.LevelData.HasBeaconStation)
				{
					int iconCount = CS$<>8__locals1.iconCount;
					CS$<>8__locals1.iconCount = iconCount + 1;
				}
				if (connection.LevelData.HasHuntingGrounds)
				{
					int iconCount = CS$<>8__locals1.iconCount;
					CS$<>8__locals1.iconCount = iconCount + 1;
				}
				if (connection.Locked)
				{
					int iconCount = CS$<>8__locals1.iconCount;
					CS$<>8__locals1.iconCount = iconCount + 1;
				}
				string tooltip = null;
				float subCrushDepth = SubmarineInfo.GetSubCrushDepth(SubmarineSelection.CurrentOrPendingSubmarine(), ref this.pendingSubInfo);
				string crushDepthWarningIconStyle = null;
				LevelData levelData = connection.LevelData;
				float spawnDepth = (float)levelData.InitialDepth + (float)levelData.Size.Y * Math.Max(levelData.GenerationParams.StartPosition.Y, levelData.GenerationParams.EndPosition.Y);
				if (spawnDepth * Physics.DisplayToRealWorldRatio > subCrushDepth)
				{
					int iconCount = CS$<>8__locals1.iconCount;
					CS$<>8__locals1.iconCount = iconCount + 1;
					crushDepthWarningIconStyle = "CrushDepthWarningHighIcon";
					tooltip = "crushdepthwarninghigh";
				}
				else if ((spawnDepth + (float)connection.LevelData.Size.Y) * Physics.DisplayToRealWorldRatio > subCrushDepth)
				{
					int iconCount = CS$<>8__locals1.iconCount;
					CS$<>8__locals1.iconCount = iconCount + 1;
					crushDepthWarningIconStyle = "CrushDepthWarningLowIcon";
					tooltip = "crushdepthwarninglow";
				}
				if (connection.LevelData.HasBeaconStation)
				{
					bool flag;
					if (!connection.LevelData.IsBeaconActive)
					{
						Level loaded2 = Level.Loaded;
						flag = (((loaded2 != null) ? loaded2.LevelData : null) == connection.LevelData && Level.Loaded.CheckBeaconActive());
					}
					else
					{
						flag = true;
					}
					bool beaconActive = flag;
					string beaconStationIconStyle = beaconActive ? "BeaconStationActive" : "BeaconStationInactive";
					this.<DrawConnection>g__DrawIcon|33_0(beaconStationIconStyle, (int)(28f * this.zoom), beaconActive ? this.beaconStationActiveText : this.beaconStationInactiveText, ref CS$<>8__locals1);
				}
				if (connection.Locked)
				{
					Location gateLocation = connection.Locations[0].IsGateBetweenBiomes ? connection.Locations[0] : connection.Locations[1];
					EventPrefab unlockEvent = EventPrefab.GetUnlockPathEvent(gateLocation.LevelData.Biome.Identifier, gateLocation.Faction);
					if (unlockEvent != null)
					{
						Reputation unlockReputation = this.CurrentLocation.Reputation;
						if (!unlockEvent.Faction.IsEmpty)
						{
							Faction unlockFaction = GameMain.GameSession.Campaign.Factions.Find((Faction f) => f.Prefab.Identifier == unlockEvent.Faction);
							unlockReputation = ((unlockFaction != null) ? unlockFaction.Reputation : null);
						}
						if (unlockReputation != null)
						{
							this.<DrawConnection>g__DrawIcon|33_0("LockedLocationConnection", (int)(28f * this.zoom), RichString.Rich(TextManager.GetWithVariables(unlockEvent.UnlockPathTooltip ?? "LockedPathTooltip", new ValueTuple<string, LocalizedString>[]
							{
								new ValueTuple<string, LocalizedString>("[requiredreputation]", Reputation.GetFormattedReputationText(MathUtils.InverseLerp((float)unlockReputation.MinReputation, (float)unlockReputation.MaxReputation, (float)unlockEvent.UnlockPathReputation), (float)unlockEvent.UnlockPathReputation, true)),
								new ValueTuple<string, LocalizedString>("[currentreputation]", unlockReputation.GetFormattedReputationText(true))
							}), null), ref CS$<>8__locals1);
						}
					}
					else
					{
						this.<DrawConnection>g__DrawIcon|33_0("LockedLocationConnection", (int)(28f * this.zoom), TextManager.Get("LockedPathTooltip"), ref CS$<>8__locals1);
					}
				}
				if (connection.LevelData.HasHuntingGrounds)
				{
					this.<DrawConnection>g__DrawIcon|33_0("HuntingGrounds", (int)(28f * this.zoom), RichString.Rich(TextManager.Get("HuntingGroundsTooltip"), null), ref CS$<>8__locals1);
				}
				if (crushDepthWarningIconStyle != null)
				{
					string iconStyle = crushDepthWarningIconStyle;
					int iconSize = (int)(32f * this.zoom);
					string tag = tooltip;
					ValueTuple<string, string>[] array = new ValueTuple<string, string>[2];
					int num = 0;
					string item = "[initialdepth]";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
					defaultInterpolatedStringHandler.AppendLiteral("‖color:gui.orange‖");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)((float)connection.LevelData.InitialDepth * Physics.DisplayToRealWorldRatio));
					defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
					array[num] = new ValueTuple<string, string>(item, defaultInterpolatedStringHandler.ToStringAndClear());
					int num2 = 1;
					string item2 = "[submarinecrushdepth]";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("‖color:gui.orange‖");
					defaultInterpolatedStringHandler2.AppendFormatted<int>((int)subCrushDepth);
					defaultInterpolatedStringHandler2.AppendLiteral("‖end‖");
					array[num2] = new ValueTuple<string, string>(item2, defaultInterpolatedStringHandler2.ToStringAndClear());
					this.<DrawConnection>g__DrawIcon|33_0(iconStyle, iconSize, RichString.Rich(TextManager.GetWithVariables(tag, array), null), ref CS$<>8__locals1);
				}
			}
			if (GameMain.DebugDraw && this.zoom > 1f * GUI.Scale && this.generationParams.ShowLevelTypeNames)
			{
				Vector2 center = rectCenter + (connection.CenterPos + viewOffset) * this.zoom;
				if (viewArea.Contains(center) && connection.Biome != null)
				{
					SpriteBatch spriteBatch2 = CS$<>8__locals1.spriteBatch;
					Vector2 pos = center - Vector2.UnitX * 50f;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(3, 2);
					LevelData levelData2 = connection.LevelData;
					Identifier? identifier;
					if (levelData2 == null)
					{
						identifier = null;
					}
					else
					{
						LevelGenerationParams levelGenerationParams = levelData2.GenerationParams;
						identifier = ((levelGenerationParams != null) ? new Identifier?(levelGenerationParams.Identifier) : null);
					}
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(identifier ?? connection.Biome.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral(" (");
					defaultInterpolatedStringHandler3.AppendFormatted(connection.Difficulty.FormatSingleDecimal());
					defaultInterpolatedStringHandler3.AppendLiteral(")");
					GUI.DrawString(spriteBatch2, pos, defaultInterpolatedStringHandler3.ToStringAndClear(), ToolBox.GradientLerp(connection.Difficulty / 100f, new Color[]
					{
						GUIStyle.Blue,
						GUIStyle.Yellow,
						GUIStyle.Red
					}), new Color?(Color.Black * 0.7f), 0, GUIStyle.SmallFont, ForceUpperCase.Inherit);
				}
			}
		}

		// Token: 0x06001F6F RID: 8047 RVA: 0x0013A268 File Offset: 0x00138468
		private bool IsPreferredTooltip(Vector2 tooltipPos)
		{
			return this.tooltip == null || Vector2.DistanceSquared(tooltipPos, PlayerInput.MousePosition) < Vector2.DistanceSquared(this.tooltip.Value.Item1.Center.ToVector2(), PlayerInput.MousePosition);
		}

		// Token: 0x06001F70 RID: 8048 RVA: 0x0013A2BC File Offset: 0x001384BC
		private void DrawDecorativeHUD(SpriteBatch spriteBatch, Rectangle rect)
		{
			this.generationParams.DecorativeGraphSprite.Draw(spriteBatch, (int)(Timing.TotalTime * 5.0 % (double)this.generationParams.DecorativeGraphSprite.FrameCount), new Vector2((float)rect.X, (float)rect.Bottom - (float)(this.generationParams.DecorativeGraphSprite.FrameSize.Y + 30) * GUI.Scale), Color.White, Vector2.Zero, 0f, Vector2.One * GUI.Scale, SpriteEffects.FlipVertically, null);
			Vector2 pos = new Vector2((float)(rect.Right - GUI.IntScale(170f)), (float)(rect.Y + GUI.IntScale(5f)));
			string text = "JOVIAN FLUX " + ((this.cameraNoiseStrength + Rand.Range(-0.02f, 0.02f, Rand.RandSync.Unsynced)) * 500f).ToString();
			Color color = this.generationParams.IndicatorColor * this.hudVisibility;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUI.DrawString(spriteBatch, pos, text, color, null, 0, smallFont, ForceUpperCase.Inherit);
			Vector2 pos2 = new Vector2((float)(rect.X + GUI.IntScale(5f)), (float)(rect.Y + GUI.IntScale(5f)));
			string text2 = "LAT " + (-this.DrawOffset.Y / 100f).ToString() + "   LON " + (-this.DrawOffset.X / 100f).ToString();
			Color color2 = this.generationParams.IndicatorColor * this.hudVisibility;
			smallFont = GUIStyle.SmallFont;
			GUI.DrawString(spriteBatch, pos2, text2, color2, null, 0, smallFont, ForceUpperCase.Inherit);
		}

		// Token: 0x06001F71 RID: 8049 RVA: 0x0013A47C File Offset: 0x0013867C
		private void UpdateMapAnim(Map.MapAnim anim, float deltaTime)
		{
			if (GUIMessageBox.MessageBoxes.Count(delegate(GUIComponent c)
			{
				GUIMessageBox mb = c as GUIMessageBox;
				return mb == null || mb.MessageBoxType != GUIMessageBox.Type.Hint;
			}) > 0)
			{
				return;
			}
			if (!string.IsNullOrEmpty(anim.StartMessage))
			{
				new GUIMessageBox("", anim.StartMessage, null, null, GUIMessageBox.Type.Default);
				anim.StartMessage = null;
				return;
			}
			float unscaledZoom = this.zoom / GUI.Scale;
			if (anim.StartZoom == null)
			{
				anim.StartZoom = new float?(MathUtils.InverseLerp(this.generationParams.MinZoom, this.generationParams.MaxZoom, unscaledZoom));
			}
			if (anim.EndZoom == null)
			{
				anim.EndZoom = new float?(MathUtils.InverseLerp(this.generationParams.MinZoom, this.generationParams.MaxZoom, unscaledZoom));
			}
			anim.StartPos = new Vector2?((anim.StartLocation == null) ? (-this.DrawOffset) : anim.StartLocation.MapPosition);
			anim.Timer = Math.Min(anim.Timer + deltaTime, anim.Duration);
			float t = (anim.Duration <= 0f) ? 1f : Math.Max(anim.Timer / anim.Duration, 0f);
			this.DrawOffset = -Vector2.SmoothStep(anim.StartPos.Value, anim.EndLocation.MapPosition, t);
			this.DrawOffset += new Vector2((float)PerlinNoise.CalculatePerlin(Timing.TotalTime * 0.30000001192092896 % 255.0, Timing.TotalTime * 0.4000000059604645 % 255.0, 0.0) - 0.5f, (float)PerlinNoise.CalculatePerlin(Timing.TotalTime * 0.4000000059604645 % 255.0, Timing.TotalTime * 0.30000001192092896 % 255.0, 0.5) - 0.5f) * 50f * (float)Math.Sin((double)(t * 3.1415927f));
			this.zoom = MathHelper.Lerp(this.generationParams.MinZoom, this.generationParams.MaxZoom, MathHelper.SmoothStep(anim.StartZoom.Value, anim.EndZoom.Value, t)) * GUI.Scale;
			if (anim.Timer >= anim.Duration)
			{
				if (!string.IsNullOrEmpty(anim.EndMessage))
				{
					new GUIMessageBox("", anim.EndMessage, null, null, GUIMessageBox.Type.Default);
					anim.EndMessage = null;
					return;
				}
				anim.Finished = true;
			}
		}

		// Token: 0x06001F72 RID: 8050 RVA: 0x0013A761 File Offset: 0x00138961
		public void ResetPendingSub()
		{
			this.pendingSubInfo = default(SubmarineInfo.PendingSubInfo);
		}

		// Token: 0x17000872 RID: 2162
		// (get) Token: 0x06001F73 RID: 8051 RVA: 0x0013A76F File Offset: 0x0013896F
		// (set) Token: 0x06001F74 RID: 8052 RVA: 0x0013A777 File Offset: 0x00138977
		public int Width { get; private set; }

		// Token: 0x17000873 RID: 2163
		// (get) Token: 0x06001F75 RID: 8053 RVA: 0x0013A780 File Offset: 0x00138980
		// (set) Token: 0x06001F76 RID: 8054 RVA: 0x0013A788 File Offset: 0x00138988
		public int Height { get; private set; }

		// Token: 0x17000874 RID: 2164
		// (get) Token: 0x06001F77 RID: 8055 RVA: 0x0013A791 File Offset: 0x00138991
		public IReadOnlyList<Location> EndLocations
		{
			get
			{
				return this.endLocations;
			}
		}

		// Token: 0x17000875 RID: 2165
		// (get) Token: 0x06001F78 RID: 8056 RVA: 0x0013A799 File Offset: 0x00138999
		// (set) Token: 0x06001F79 RID: 8057 RVA: 0x0013A7A1 File Offset: 0x001389A1
		public Location StartLocation { get; private set; }

		// Token: 0x17000876 RID: 2166
		// (get) Token: 0x06001F7A RID: 8058 RVA: 0x0013A7AA File Offset: 0x001389AA
		// (set) Token: 0x06001F7B RID: 8059 RVA: 0x0013A7B2 File Offset: 0x001389B2
		public Location CurrentLocation { get; private set; }

		// Token: 0x17000877 RID: 2167
		// (get) Token: 0x06001F7C RID: 8060 RVA: 0x0013A7BB File Offset: 0x001389BB
		public int CurrentLocationIndex
		{
			get
			{
				return this.Locations.IndexOf(this.CurrentLocation);
			}
		}

		// Token: 0x17000878 RID: 2168
		// (get) Token: 0x06001F7D RID: 8061 RVA: 0x0013A7CE File Offset: 0x001389CE
		// (set) Token: 0x06001F7E RID: 8062 RVA: 0x0013A7D6 File Offset: 0x001389D6
		public Location SelectedLocation { get; private set; }

		// Token: 0x17000879 RID: 2169
		// (get) Token: 0x06001F7F RID: 8063 RVA: 0x0013A7DF File Offset: 0x001389DF
		public int SelectedLocationIndex
		{
			get
			{
				return this.Locations.IndexOf(this.SelectedLocation);
			}
		}

		// Token: 0x06001F80 RID: 8064 RVA: 0x0013A7F4 File Offset: 0x001389F4
		public IEnumerable<int> GetSelectedMissionIndices()
		{
			if (this.SelectedConnection != null)
			{
				return this.CurrentLocation.GetSelectedMissionIndices();
			}
			return Enumerable.Empty<int>();
		}

		// Token: 0x1700087A RID: 2170
		// (get) Token: 0x06001F81 RID: 8065 RVA: 0x0013A81C File Offset: 0x00138A1C
		// (set) Token: 0x06001F82 RID: 8066 RVA: 0x0013A824 File Offset: 0x00138A24
		public LocationConnection SelectedConnection { get; private set; }

		// Token: 0x1700087B RID: 2171
		// (get) Token: 0x06001F83 RID: 8067 RVA: 0x0013A82D File Offset: 0x00138A2D
		// (set) Token: 0x06001F84 RID: 8068 RVA: 0x0013A835 File Offset: 0x00138A35
		public string Seed { get; private set; }

		// Token: 0x1700087C RID: 2172
		// (get) Token: 0x06001F85 RID: 8069 RVA: 0x0013A83E File Offset: 0x00138A3E
		// (set) Token: 0x06001F86 RID: 8070 RVA: 0x0013A846 File Offset: 0x00138A46
		public List<Location> Locations { get; private set; }

		// Token: 0x1700087D RID: 2173
		// (get) Token: 0x06001F87 RID: 8071 RVA: 0x0013A84F File Offset: 0x00138A4F
		// (set) Token: 0x06001F88 RID: 8072 RVA: 0x0013A857 File Offset: 0x00138A57
		public List<LocationConnection> Connections { get; private set; }

		// Token: 0x1700087E RID: 2174
		// (get) Token: 0x06001F89 RID: 8073 RVA: 0x0013A860 File Offset: 0x00138A60
		public IOrderedEnumerable<Biome> OrderedBiomes
		{
			get
			{
				IOrderedEnumerable<Biome> result;
				if ((result = this._orderedBiomes) == null)
				{
					IOrderedEnumerable<Biome> orderedEnumerable = this._orderedBiomes = Biome.Prefabs.GetOrdered();
					result = orderedEnumerable;
				}
				return result;
			}
		}

		// Token: 0x06001F8A RID: 8074 RVA: 0x0013A88C File Offset: 0x00138A8C
		public Map(CampaignSettings settings)
		{
			this.generationParams = MapGenerationParams.Instance;
			this.Width = this.generationParams.Width;
			this.Height = this.generationParams.Height;
			this.Locations = new List<Location>();
			this.Connections = new List<LocationConnection>();
			if (this.generationParams.RadiationParams != null)
			{
				this.Radiation = new Radiation(this, this.generationParams.RadiationParams, null)
				{
					Enabled = settings.RadiationEnabled
				};
			}
		}

		// Token: 0x06001F8B RID: 8075 RVA: 0x0013A968 File Offset: 0x00138B68
		private Map(CampaignMode campaign, XElement element) : this(campaign.Settings)
		{
			this.Seed = element.GetAttributeString("seed", "a");
			Rand.SetSyncedSeed(ToolBox.StringToInt(this.Seed));
			this.Width = element.GetAttributeInt("width", this.Width);
			this.Height = element.GetAttributeInt("height", this.Height);
			bool lairsFound = false;
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "location"))
				{
					if (a == "radiation")
					{
						this.Radiation = new Radiation(this, this.generationParams.RadiationParams, subElement)
						{
							Enabled = campaign.Settings.RadiationEnabled
						};
					}
				}
				else
				{
					int i = subElement.GetAttributeInt("i", 0);
					while (this.Locations.Count <= i)
					{
						this.Locations.Add(null);
					}
					lairsFound |= subElement.GetAttributeString("type", "").Equals("lair", StringComparison.OrdinalIgnoreCase);
					this.Locations[i] = new Location(campaign, subElement);
				}
			}
			List<XElement> connectionElements = new List<XElement>();
			foreach (XElement subElement2 in element.Elements())
			{
				string a2 = subElement2.Name.ToString().ToLowerInvariant();
				if (a2 == "connection")
				{
					Point locationIndices = subElement2.GetAttributePoint("locations", new Point(0, 1));
					if (locationIndices.X != locationIndices.Y)
					{
						LocationConnection connection = new LocationConnection(this.Locations[locationIndices.X], this.Locations[locationIndices.Y])
						{
							Passed = subElement2.GetAttributeBool("passed", false),
							Locked = subElement2.GetAttributeBool("locked", false),
							Difficulty = subElement2.GetAttributeFloat("difficulty", 0f)
						};
						this.Locations[locationIndices.X].Connections.Add(connection);
						this.Locations[locationIndices.Y].Connections.Add(connection);
						string biomeId = subElement2.GetAttributeString("biome", "");
						LocationConnection locationConnection = connection;
						Biome biome;
						if ((biome = Biome.Prefabs.FirstOrDefault((Biome b) => b.Identifier == biomeId)) == null)
						{
							biome = (Biome.Prefabs.FirstOrDefault((Biome b) => !b.OldIdentifier.IsEmpty && b.OldIdentifier == biomeId) ?? Biome.Prefabs.First<Biome>());
						}
						locationConnection.Biome = biome;
						connection.Difficulty = MathHelper.Clamp(connection.Difficulty, connection.Biome.MinDifficulty, connection.Biome.AdjustedMaxDifficulty);
						connection.LevelData = new LevelData(subElement2.Element("Level"), new float?(connection.Difficulty), false);
						this.Connections.Add(connection);
						connectionElements.Add(subElement2);
					}
				}
			}
			Random rand = new MTRandom(ToolBox.StringToInt(this.Seed));
			if (this.Locations.First<Location>().Biome == null)
			{
				this.AssignBiomes(rand);
			}
			int startLocationindex = element.GetAttributeInt("startlocation", -1);
			if (startLocationindex >= 0 && startLocationindex < this.Locations.Count)
			{
				this.StartLocation = this.Locations[startLocationindex];
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error while loading the map. Start location index out of bounds (index: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(startLocationindex);
				defaultInterpolatedStringHandler.AppendLiteral(", location count: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Locations.Count);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				foreach (Location location in this.Locations)
				{
					if (location.Type.HasOutpost && (this.StartLocation == null || location.MapPosition.X < this.StartLocation.MapPosition.X))
					{
						this.StartLocation = location;
					}
				}
			}
			if (element.GetAttribute("endlocation", StringComparison.OrdinalIgnoreCase) != null)
			{
				int endLocationIndex = element.GetAttributeInt("endlocation", -1);
				if (endLocationIndex >= 0 && endLocationIndex < this.Locations.Count)
				{
					this.endLocations.Add(this.Locations[endLocationIndex]);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(90, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Error while loading the map. End location index out of bounds (index: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(endLocationIndex);
					defaultInterpolatedStringHandler2.AppendLiteral(", location count: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(this.Locations.Count);
					defaultInterpolatedStringHandler2.AppendLiteral(").");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				}
			}
			else
			{
				int[] endLocationindices = element.GetAttributeIntArray("endlocations", Array.Empty<int>());
				foreach (int endLocationIndex2 in endLocationindices)
				{
					if (endLocationIndex2 >= 0 && endLocationIndex2 < this.Locations.Count)
					{
						this.endLocations.Add(this.Locations[endLocationIndex2]);
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(90, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Error while loading the map. End location index out of bounds (index: ");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(endLocationIndex2);
						defaultInterpolatedStringHandler3.AppendLiteral(", location count: ");
						defaultInterpolatedStringHandler3.AppendFormatted<int>(this.Locations.Count);
						defaultInterpolatedStringHandler3.AppendLiteral(").");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
					}
				}
			}
			if (!this.endLocations.Any<Location>())
			{
				DebugConsole.AddWarning("Error while loading the map. No end location(s) found. Choosing the rightmost location as the end location...", null);
				Location endLocation = null;
				foreach (Location location2 in this.Locations)
				{
					if (endLocation == null || location2.MapPosition.X > endLocation.MapPosition.X)
					{
						endLocation = location2;
					}
				}
				this.endLocations.Add(endLocation);
			}
			Location firstEndLocation = this.EndLocations[0];
			Biome endBiome = firstEndLocation.Biome;
			int missingOutpostCount = endBiome.EndBiomeLocationCount - this.endLocations.Count;
			for (int j = 0; j < missingOutpostCount; j++)
			{
				Vector2 mapPos = new Vector2(MathHelper.Lerp(firstEndLocation.MapPosition.X, (float)this.Width, MathHelper.Lerp(0.2f, 0.8f, (float)j / (float)missingOutpostCount)), (float)this.Height * MathHelper.Lerp(0.2f, 1f, (float)rand.NextDouble()));
				Location newEndLocation = new Location(mapPos, new int?(this.generationParams.DifficultyZones), new Identifier?(endBiome.Identifier), rand, false, firstEndLocation.Type, this.Locations);
				newEndLocation.Biome = endBiome;
				newEndLocation.LevelData = new LevelData(newEndLocation, this, 100f);
				this.Locations.Add(newEndLocation);
				this.endLocations.Add(newEndLocation);
			}
			if (lairsFound)
			{
				if (!this.Connections.Any((LocationConnection c) => c.LevelData.HasHuntingGrounds))
				{
					for (int k = 0; k < this.Connections.Count; k++)
					{
						this.Connections[k].LevelData.HasHuntingGrounds = (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < this.Connections[k].Difficulty / 100f * 0.3f);
						connectionElements[k].SetAttributeValue("hashuntinggrounds", true);
					}
				}
			}
			this.AssignEndLocationLevelData(campaign);
			float maxX = (from l in this.Locations
			select l.MapPosition.X).Max();
			if (maxX > (float)this.Width)
			{
				this.Width = (int)(maxX + 10f);
			}
			float maxY = (from l in this.Locations
			select l.MapPosition.Y).Max();
			if (maxY > (float)this.Height)
			{
				this.Height = (int)(maxY + 10f);
			}
			this.InitProjectSpecific();
		}

		// Token: 0x06001F8C RID: 8076 RVA: 0x0013B284 File Offset: 0x00139484
		public Map(CampaignMode campaign, string seed) : this(campaign.Settings)
		{
			this.Seed = seed;
			Rand.SetSyncedSeed(ToolBox.StringToInt(this.Seed));
			this.Generate(campaign);
			if (this.Locations.Count == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Generating a campaign map failed (no locations created). Width: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Width);
				defaultInterpolatedStringHandler.AppendLiteral(", height: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Height);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.<.ctor>g__FindStartLocation|100_2((Location l) => l.Type.Identifier == "outpost");
			if (this.CurrentLocation == null)
			{
				this.<.ctor>g__FindStartLocation|100_2((Location l) => l.Type.HasOutpost && l.Type.OutpostTeam == CharacterTeamType.FriendlyNPC);
			}
			this.StartLocation.SecondaryFaction = null;
			Faction faction;
			if (campaign == null)
			{
				faction = null;
			}
			else
			{
				faction = campaign.Factions.FirstOrDefault((Faction f) => f.Prefab.StartOutpost);
			}
			Faction startOutpostFaction = faction;
			if (startOutpostFaction != null)
			{
				this.StartLocation.Faction = startOutpostFaction;
			}
			foreach (LocationConnection connection in this.StartLocation.Connections)
			{
				Location otherLocation = connection.OtherLocation(this.StartLocation);
				LocationType outpostLocationType;
				if (!otherLocation.HasOutpost() && LocationType.Prefabs.TryGet("outpost".ToIdentifier(), out outpostLocationType))
				{
					otherLocation.ChangeType(campaign, outpostLocationType, false, true);
				}
				if (otherLocation.HasOutpost() && otherLocation.Type.OutpostTeam == CharacterTeamType.FriendlyNPC && otherLocation.Type.Faction.IsEmpty)
				{
					otherLocation.Faction = startOutpostFaction;
				}
			}
			if (campaign.CampaignMetadata.GetInt("campaign.endings".ToIdentifier(), new int?(0)) == 0 && (campaign.Settings.WorldHostility == WorldHostilityOption.Low || campaign.Settings.WorldHostility == WorldHostilityOption.Medium))
			{
				if (this.StartLocation != null)
				{
					this.StartLocation.LevelData = new LevelData(this.StartLocation, this, 0f);
				}
				foreach (LocationConnection locationConnection in this.StartLocation.Connections)
				{
					if (locationConnection.Difficulty > 0f)
					{
						locationConnection.Difficulty = 0f;
						locationConnection.LevelData = new LevelData(locationConnection);
					}
				}
			}
			LocationType tutorialOutpost;
			if (campaign.IsSinglePlayer && campaign.Settings.TutorialEnabled && LocationType.Prefabs.TryGet("tutorialoutpost", out tutorialOutpost))
			{
				this.CurrentLocation.ChangeType(campaign, tutorialOutpost, true, true);
			}
			else
			{
				LocationType forceStartOutpostType = (from lt in LocationType.Prefabs
				where lt.ForceAsStartOutpost
				select lt).GetRandom(Rand.RandSync.ServerAndClient);
				if (forceStartOutpostType != null)
				{
					this.CurrentLocation.ChangeType(campaign, forceStartOutpostType, true, true);
				}
			}
			this.Discover(this.CurrentLocation, true);
			this.Visit(this.CurrentLocation, true);
			this.CurrentLocation.CreateStores(false);
			foreach (Location location in this.Locations)
			{
				location.UnlockInitialMissions(Rand.RandSync.ServerAndClient);
			}
			this.InitProjectSpecific();
		}

		// Token: 0x06001F8D RID: 8077 RVA: 0x0013B628 File Offset: 0x00139828
		private void InitProjectSpecific()
		{
			if (Map.noiseOverlay == null)
			{
				Map.noiseOverlay = new Sprite("Content/UI/noise.png", Vector2.Zero);
			}
			this.OnLocationChanged.RegisterOverwriteExisting("Map.InitProjSpecific".ToIdentifier(), delegate(Map.LocationChangeInfo locationChangeInfo)
			{
				this.LocationChanged(locationChangeInfo.PrevLocation, locationChangeInfo.NewLocation);
			});
			this.borders = new Rectangle((int)this.Locations.Min((Location l) => l.MapPosition.X), (int)this.Locations.Min((Location l) => l.MapPosition.Y), (int)this.Locations.Max((Location l) => l.MapPosition.X), (int)this.Locations.Max((Location l) => l.MapPosition.Y));
			this.borders.Width = this.borders.Width - this.borders.X;
			this.borders.Height = this.borders.Height - this.borders.Y;
			if (this.CurrentLocation != null)
			{
				this.DrawOffset = -this.CurrentLocation.MapPosition;
			}
			Vector2 tileSize = this.generationParams.MapTiles.Values.First<ImmutableArray<Sprite>>().First<Sprite>().size * this.generationParams.MapTileScale;
			int tilesX = (int)Math.Ceiling((double)((float)this.Width / tileSize.X));
			int tilesY = (int)Math.Ceiling((double)((float)this.Height / tileSize.Y));
			this.mapTiles = new Sprite[tilesX, tilesY];
			this.tileDiscovered = new bool[tilesX, tilesY];
			HashSet<Biome> missingBiomes = new HashSet<Biome>();
			for (int x = 0; x < tilesX; x++)
			{
				for (int y = 0; y < tilesY; y++)
				{
					Biome biome = this.GetBiome((float)x * tileSize.X);
					ImmutableArray<Sprite> tileList;
					if (this.generationParams.MapTiles.ContainsKey(biome.Identifier))
					{
						tileList = this.generationParams.MapTiles[biome.Identifier];
					}
					else
					{
						tileList = this.generationParams.MapTiles.Values.First<ImmutableArray<Sprite>>();
						missingBiomes.Add(biome);
					}
					this.mapTiles[x, y] = tileList[x % tileList.Length];
				}
			}
			foreach (Biome missingBiome in missingBiomes)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(101, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find campaign map sprites for the biome \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(missingBiome.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". Using the sprites of the first biome instead...");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			this.beaconStationActiveText = RichString.Rich(TextManager.Get("BeaconStationActiveTooltip"), null);
			this.beaconStationInactiveText = RichString.Rich(TextManager.Get("BeaconStationInactiveTooltip"), null);
			this.RemoveFogOfWar(this.StartLocation, true);
			this.GenerateAllLocationConnectionVisuals();
		}

		// Token: 0x06001F8E RID: 8078 RVA: 0x0013B964 File Offset: 0x00139B64
		private void Generate(CampaignMode campaign)
		{
			Map.<>c__DisplayClass102_0 CS$<>8__locals1 = new Map.<>c__DisplayClass102_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.campaign = campaign;
			this.Connections.Clear();
			this.Locations.Clear();
			List<Vector2> voronoiSites = new List<Vector2>();
			for (float x = 10f; x < (float)this.Width - 10f; x += (float)this.generationParams.VoronoiSiteInterval.X)
			{
				for (float y = 10f; y < (float)this.Height - 10f; y += (float)this.generationParams.VoronoiSiteInterval.Y)
				{
					voronoiSites.Add(new Vector2(x + (float)this.generationParams.VoronoiSiteVariance.X * Rand.Range(-0.5f, 0.5f, Rand.RandSync.ServerAndClient), y + (float)this.generationParams.VoronoiSiteVariance.Y * Rand.Range(-0.5f, 0.5f, Rand.RandSync.ServerAndClient)));
				}
			}
			MapLocationTypeGenerator mapLocationTypeGenerator = new MapLocationTypeGenerator(CS$<>8__locals1.campaign, this);
			Voronoi voronoi = new Voronoi(0.5);
			List<GraphEdge> edges = voronoi.MakeVoronoiGraph(voronoiSites, this.Width, this.Height);
			Vector2 margin = new Vector2(Math.Min(10f, (float)this.Width * 0.1f), Math.Min(10f, (float)this.Height * 0.2f));
			float startX = margin.X;
			float endX = (float)this.Width - margin.X;
			float startY = margin.Y;
			float endY = (float)this.Height - margin.Y;
			if (!edges.Any<GraphEdge>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(93, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Generating a campaign map failed (no edges in the voronoi graph). Width: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Width);
				defaultInterpolatedStringHandler.AppendLiteral(", height: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Height);
				defaultInterpolatedStringHandler.AppendLiteral(", margin: ");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(margin);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			voronoiSites.Clear();
			using (List<GraphEdge>.Enumerator enumerator = edges.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Map.<>c__DisplayClass102_1 CS$<>8__locals2 = new Map.<>c__DisplayClass102_1();
					CS$<>8__locals2.edge = enumerator.Current;
					if (!(CS$<>8__locals2.edge.Point1 == CS$<>8__locals2.edge.Point2) && CS$<>8__locals2.edge.Point1.X >= margin.X && CS$<>8__locals2.edge.Point1.X <= (float)this.Width - margin.X && CS$<>8__locals2.edge.Point1.Y >= startY && CS$<>8__locals2.edge.Point1.Y <= endY && CS$<>8__locals2.edge.Point2.X >= margin.X && CS$<>8__locals2.edge.Point2.X <= (float)this.Width - margin.X && CS$<>8__locals2.edge.Point2.Y >= startY && CS$<>8__locals2.edge.Point2.Y <= endY)
					{
						Location[] newLocations = new Location[2];
						newLocations[0] = this.Locations.Find((Location l) => l.MapPosition == CS$<>8__locals2.edge.Point1 || l.MapPosition == CS$<>8__locals2.edge.Point2);
						newLocations[1] = this.Locations.Find((Location l) => l != newLocations[0] && (l.MapPosition == CS$<>8__locals2.edge.Point1 || l.MapPosition == CS$<>8__locals2.edge.Point2));
						for (int i4 = 0; i4 < 2; i4++)
						{
							if (newLocations[i4] == null)
							{
								Vector2[] points = new Vector2[]
								{
									CS$<>8__locals2.edge.Point1,
									CS$<>8__locals2.edge.Point2
								};
								int positionIndex = Rand.Int(1, Rand.RandSync.ServerAndClient);
								Vector2 position = points[positionIndex];
								if (newLocations[1 - i4] != null && newLocations[1 - i4].MapPosition == position)
								{
									position = points[1 - positionIndex];
								}
								int zone = this.GetZoneIndex(position.X);
								Location[] newLocations2 = newLocations;
								int num = i4;
								Vector2 position2 = position;
								int? zone6 = new int?(zone);
								Biome biome = this.GetBiome(position.X);
								newLocations2[num] = Location.CreateRandom(position2, zone6, (biome != null) ? new Identifier?(biome.Identifier) : null, Rand.GetRNG(Rand.RandSync.ServerAndClient), false, null, this.Locations);
								mapLocationTypeGenerator.AddToLocationsPerZone(zone, newLocations[i4]);
								this.Locations.Add(newLocations[i4]);
							}
						}
						LocationConnection newConnection = new LocationConnection(newLocations[0], newLocations[1]);
						this.Connections.Add(newConnection);
					}
				}
			}
			float minConnectionDistanceSqr = this.generationParams.MinConnectionDistance * this.generationParams.MinConnectionDistance;
			for (int j2 = this.Connections.Count - 1; j2 >= 0; j2--)
			{
				LocationConnection connection9 = this.Connections[j2];
				if (Vector2.DistanceSquared(connection9.Locations[0].MapPosition, connection9.Locations[1].MapPosition) <= minConnectionDistanceSqr)
				{
					this.Connections.Remove(connection9);
					foreach (LocationConnection connection2 in this.Connections)
					{
						if (connection2.Locations[0] == connection9.Locations[0])
						{
							connection2.Locations[0] = connection9.Locations[1];
						}
						if (connection2.Locations[1] == connection9.Locations[0])
						{
							connection2.Locations[1] = connection9.Locations[1];
						}
					}
				}
			}
			foreach (LocationConnection connection3 in this.Connections)
			{
				connection3.Locations[0].Connections.Add(connection3);
				connection3.Locations[1].Connections.Add(connection3);
			}
			float minLocationDistanceSqr = this.generationParams.MinLocationDistance * this.generationParams.MinLocationDistance;
			int i;
			int num2;
			for (i = this.Locations.Count - 1; i >= 0; i = num2 - 1)
			{
				int j;
				for (j = this.Locations.Count - 1; j > i; j = num2 - 1)
				{
					float dist = Vector2.DistanceSquared(this.Locations[i].MapPosition, this.Locations[j].MapPosition);
					if (dist <= minLocationDistanceSqr)
					{
						foreach (LocationConnection connection4 in this.Locations[j].Connections)
						{
							if (connection4.Locations[0] == this.Locations[j])
							{
								connection4.Locations[0] = this.Locations[i];
							}
							else
							{
								connection4.Locations[1] = this.Locations[i];
							}
							if (connection4.Locations[0] != connection4.Locations[1])
							{
								this.Locations[i].Connections.Add(connection4);
							}
							else
							{
								this.Connections.Remove(connection4);
							}
						}
						this.Locations[i].Connections.RemoveAll((LocationConnection c) => c.OtherLocation(CS$<>8__locals1.<>4__this.Locations[i]) == CS$<>8__locals1.<>4__this.Locations[j]);
						this.Locations.RemoveAt(j);
					}
					num2 = j;
				}
				num2 = i;
			}
			foreach (Location location in this.Locations)
			{
				List<LocationConnection> connections = location.Connections;
				Comparison<LocationConnection> comparison;
				if ((comparison = CS$<>8__locals1.<>9__9) == null)
				{
					comparison = (CS$<>8__locals1.<>9__9 = ((LocationConnection c1, LocationConnection c2) => CS$<>8__locals1.<>4__this.Connections.IndexOf(c1).CompareTo(CS$<>8__locals1.<>4__this.Connections.IndexOf(c2))));
				}
				connections.Sort(comparison);
			}
			for (int k = this.Connections.Count - 1; k >= 0; k--)
			{
				k = Math.Min(k, this.Connections.Count - 1);
				LocationConnection connection5 = this.Connections[k];
				for (int l2 = Math.Min(k - 1, this.Connections.Count - 1); l2 >= 0; l2--)
				{
					if (connection5.Locations.Contains(this.Connections[l2].Locations[0]) && connection5.Locations.Contains(this.Connections[l2].Locations[1]))
					{
						this.Connections.RemoveAt(l2);
					}
				}
			}
			List<LocationConnection>[] connectionsBetweenZones = new List<LocationConnection>[this.generationParams.DifficultyZones];
			for (int m = 0; m < this.generationParams.DifficultyZones; m++)
			{
				connectionsBetweenZones[m] = new List<LocationConnection>();
			}
			List<LocationConnection> shuffledConnections = this.Connections.ToList<LocationConnection>();
			shuffledConnections.Shuffle(Rand.RandSync.ServerAndClient);
			using (List<LocationConnection>.Enumerator enumerator6 = shuffledConnections.GetEnumerator())
			{
				while (enumerator6.MoveNext())
				{
					LocationConnection connection = enumerator6.Current;
					int zone2 = this.GetZoneIndex(connection.Locations[0].MapPosition.X);
					int zone3 = this.GetZoneIndex(connection.Locations[1].MapPosition.X);
					if (zone2 != zone3)
					{
						if (zone2 > zone3)
						{
							int num3 = zone3;
							zone3 = zone2;
							zone2 = num3;
						}
						if (this.generationParams.GateCount[zone2] != 0)
						{
							if (!connectionsBetweenZones[zone2].Any<LocationConnection>())
							{
								connectionsBetweenZones[zone2].Add(connection);
							}
							else if (this.generationParams.GateCount[zone2] == 1)
							{
								if (Math.Abs(connection.CenterPos.Y - (float)(this.Height / 2)) < Math.Abs(connectionsBetweenZones[zone2].First<LocationConnection>().CenterPos.Y - (float)(this.Height / 2)))
								{
									connectionsBetweenZones[zone2].Clear();
									connectionsBetweenZones[zone2].Add(connection);
								}
							}
							else if (connectionsBetweenZones[zone2].Count<LocationConnection>() < this.generationParams.GateCount[zone2] && connectionsBetweenZones[zone2].None((LocationConnection c) => c.Locations.Contains(connection.Locations[0]) || c.Locations.Contains(connection.Locations[1])))
							{
								connectionsBetweenZones[zone2].Add(connection);
							}
							if (connectionsBetweenZones[zone2].None(null))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(140, 2);
								defaultInterpolatedStringHandler2.AppendLiteral("Potential error during map generation: no connections between zones ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(zone2);
								defaultInterpolatedStringHandler2.AppendLiteral(" and ");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(zone3);
								defaultInterpolatedStringHandler2.AppendLiteral(" found. Traversing through to the end of the map may be impossible.");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
							}
						}
					}
				}
			}
			IOrderedEnumerable<LocationType> orderedPrefabs = LocationType.Prefabs.GetOrdered();
			List<Location> forciblyReassignedGateLocations = new List<Location>();
			List<Faction> gateFactions = (from f in CS$<>8__locals1.campaign.Factions
			where f.Prefab.ControlledOutpostPercentage > 0f
			orderby f.Prefab.Identifier
			select f).ToList<Faction>();
			for (int n = this.Connections.Count - 1; n >= 0; n--)
			{
				int zone4 = this.GetZoneIndex(this.Connections[n].Locations[0].MapPosition.X);
				int zone5 = this.GetZoneIndex(this.Connections[n].Locations[1].MapPosition.X);
				if (zone4 != zone5 && zone4 != this.generationParams.DifficultyZones && zone5 != this.generationParams.DifficultyZones)
				{
					int leftZone = Math.Min(zone4, zone5);
					if (this.generationParams.GateCount[leftZone] != 0)
					{
						if (!connectionsBetweenZones[leftZone].Contains(this.Connections[n]))
						{
							this.Connections.RemoveAt(n);
						}
						else
						{
							Location leftMostLocation = (this.Connections[n].Locations[0].MapPosition.X < this.Connections[n].Locations[1].MapPosition.X) ? this.Connections[n].Locations[0] : this.Connections[n].Locations[1];
							if (!Map.<Generate>g__AllowAsBiomeGate|102_11(leftMostLocation.Type))
							{
								IEnumerable<LocationType> source = orderedPrefabs;
								Func<LocationType, bool> predicate;
								if ((predicate = Map.<>O.<0>__AllowAsBiomeGate) == null)
								{
									predicate = (Map.<>O.<0>__AllowAsBiomeGate = new Func<LocationType, bool>(Map.<Generate>g__AllowAsBiomeGate|102_11));
								}
								IEnumerable<LocationType> potentialGateLocationTypes = source.Where(predicate);
								LocationType random;
								Func<LocationType.AreaSettingData, bool> <>9__15;
								Func<LocationType.AreaSettingData, bool> <>9__16;
								if ((random = potentialGateLocationTypes.Where(delegate(LocationType lt)
								{
									IEnumerable<LocationType.AreaSettingData> areaSettings2 = lt.AreaSettings;
									Func<LocationType.AreaSettingData, bool> predicate2;
									if ((predicate2 = <>9__15) == null)
									{
										predicate2 = (<>9__15 = ((LocationType.AreaSettingData areaSettings) => areaSettings.MatchesLocation(CS$<>8__locals1.<>4__this, leftMostLocation) && areaSettings.Commonness > 0f));
									}
									return areaSettings2.Any(predicate2);
								}).GetRandom(Rand.RandSync.ServerAndClient)) == null && (random = potentialGateLocationTypes.Where(delegate(LocationType lt)
								{
									IEnumerable<LocationType.AreaSettingData> areaSettings2 = lt.AreaSettings;
									Func<LocationType.AreaSettingData, bool> predicate2;
									if ((predicate2 = <>9__16) == null)
									{
										predicate2 = (<>9__16 = delegate(LocationType.AreaSettingData areaSettings)
										{
											if (areaSettings.MatchesLocation(CS$<>8__locals1.<>4__this, leftMostLocation))
											{
												int? minCount = areaSettings.MinCount;
												int num4 = 0;
												return minCount.GetValueOrDefault() > num4 & minCount != null;
											}
											return false;
										});
									}
									return areaSettings2.Any(predicate2);
								}).GetRandom(Rand.RandSync.ServerAndClient)) == null)
								{
									random = (from lt in potentialGateLocationTypes
									where lt.AreaSettings.None((LocationType.AreaSettingData areaSettings) => areaSettings.Commonness > 0f || areaSettings.HasCounts)
									select lt).GetRandom(Rand.RandSync.ServerAndClient);
								}
								LocationType gateLocationType = random;
								if (gateLocationType == null)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(80, 2);
									defaultInterpolatedStringHandler3.AppendLiteral("Failed to find a suitable location type for a gate location between zones ");
									defaultInterpolatedStringHandler3.AppendFormatted<int>(zone4);
									defaultInterpolatedStringHandler3.AppendLiteral(" and ");
									defaultInterpolatedStringHandler3.AppendFormatted<int>(zone5);
									defaultInterpolatedStringHandler3.AppendLiteral(".");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
									goto IL_F59;
								}
								leftMostLocation.ChangeType(CS$<>8__locals1.campaign, gateLocationType, false, false);
								forciblyReassignedGateLocations.Add(leftMostLocation);
							}
							leftMostLocation.IsGateBetweenBiomes = true;
							this.Connections[n].Locked = true;
							if (leftMostLocation.Type.HasOutpost && CS$<>8__locals1.campaign != null && gateFactions.Any<Faction>())
							{
								leftMostLocation.Faction = gateFactions[connectionsBetweenZones[leftZone].IndexOf(this.Connections[n]) % gateFactions.Count];
							}
						}
					}
				}
				IL_F59:;
			}
			foreach (Location location2 in this.Locations)
			{
				for (int i2 = location2.Connections.Count - 1; i2 >= 0; i2--)
				{
					if (!this.Connections.Contains(location2.Connections[i2]))
					{
						location2.Connections.RemoveAt(i2);
					}
				}
			}
			for (int i3 = 0; i3 < this.Connections.Count; i3++)
			{
				LocationConnection connection6 = this.Connections[i3];
				if (connection6.Locked)
				{
					Location rightMostLocation = (connection6.Locations[0].MapPosition.X > connection6.Locations[1].MapPosition.X) ? connection6.Locations[0] : connection6.Locations[1];
					if (rightMostLocation.Connections.All((LocationConnection c) => c.OtherLocation(rightMostLocation).MapPosition.X < rightMostLocation.MapPosition.X))
					{
						Location closestLocation = null;
						float closestDist = float.PositiveInfinity;
						foreach (Location otherLocation in this.Locations)
						{
							if (otherLocation != rightMostLocation && otherLocation.MapPosition.X >= rightMostLocation.MapPosition.X)
							{
								float dist2 = Vector2.DistanceSquared(rightMostLocation.MapPosition, otherLocation.MapPosition);
								if (dist2 < closestDist || closestLocation == null)
								{
									closestLocation = otherLocation;
									closestDist = dist2;
								}
							}
						}
						LocationConnection newConnection2 = new LocationConnection(rightMostLocation, closestLocation);
						rightMostLocation.Connections.Add(newConnection2);
						closestLocation.Connections.Add(newConnection2);
						this.Connections.Add(newConnection2);
						this.GenerateLocationConnectionVisuals(newConnection2);
					}
				}
			}
			this.Locations.RemoveAll((Location l) => !CS$<>8__locals1.<>4__this.Connections.Any((LocationConnection c) => c.Locations.Contains(l)));
			this.AssignBiomes(Rand.GetRNG(Rand.RandSync.ServerAndClient));
			IEnumerable<Location> gateLocations = from l in this.Locations
			where l.IsGateBetweenBiomes
			select l;
			foreach (Location gateLocation in forciblyReassignedGateLocations)
			{
				mapLocationTypeGenerator.RemoveOneFromTotals(gateLocation.Type, gateLocation);
			}
			mapLocationTypeGenerator.AssignForcedBiomeGateTypes(gateLocations);
			foreach (LocationConnection connection7 in this.Connections)
			{
				if (connection7.Locations.Any((Location l) => l.IsGateBetweenBiomes))
				{
					connection7.Difficulty = Math.Min(connection7.Locations.Min((Location l) => l.Biome.ActualMaxDifficulty), connection7.Biome.AdjustedMaxDifficulty);
				}
				else
				{
					connection7.Difficulty = CS$<>8__locals1.<Generate>g__CalculateDifficulty|5(connection7.CenterPos.X, connection7.Biome);
				}
			}
			Location startLocation = this.Locations.MinBy((Location l) => l.MapPosition.X);
			LocationType startLocationType;
			if (LocationType.Prefabs.TryGet("outpost", out startLocationType))
			{
				mapLocationTypeGenerator.ChangeLocationTypeAndName(CS$<>8__locals1.campaign, startLocation, startLocationType);
				mapLocationTypeGenerator.AddToFilled(startLocation);
			}
			mapLocationTypeGenerator.AssignLocationTypesBasedOnDesiredPosition(gateLocations);
			foreach (Location location3 in this.Locations)
			{
				location3.LevelData = new LevelData(location3, this, CS$<>8__locals1.<Generate>g__CalculateDifficulty|5(location3.MapPosition.X, location3.Biome));
				location3.TryAssignFactionBasedOnLocationType(CS$<>8__locals1.campaign);
				if (location3.Type.HasOutpost && CS$<>8__locals1.campaign != null && location3.Type.OutpostTeam == CharacterTeamType.FriendlyNPC)
				{
					if (location3.Type.Faction.IsEmpty)
					{
						Location location4 = location3;
						if (location4.Faction == null)
						{
							location4.Faction = CS$<>8__locals1.campaign.GetRandomFaction(Rand.RandSync.ServerAndClient, true);
						}
					}
					if (location3.Type.SecondaryFaction.IsEmpty)
					{
						Location location4 = location3;
						if (location4.SecondaryFaction == null)
						{
							location4.SecondaryFaction = CS$<>8__locals1.campaign.GetRandomSecondaryFaction(Rand.RandSync.ServerAndClient, true);
						}
					}
				}
			}
			foreach (Location gateLocation2 in forciblyReassignedGateLocations)
			{
				gateLocation2.UnlockInitialMissions(Rand.RandSync.ServerAndClient);
			}
			List<Location> locationsToAssign = this.Locations.ToList<Location>();
			locationsToAssign.Remove(this.GetPreviousToEndLocation());
			mapLocationTypeGenerator.AssignLocationTypesBasedOnCount(gateLocations, locationsToAssign);
			foreach (LocationConnection connection8 in this.Connections)
			{
				connection8.LevelData = new LevelData(connection8);
			}
			this.CreateEndLocation(CS$<>8__locals1.campaign);
		}

		// Token: 0x06001F8F RID: 8079 RVA: 0x0013CF78 File Offset: 0x0013B178
		private void GenerateAllLocationConnectionVisuals()
		{
			foreach (LocationConnection connection in this.Connections)
			{
				this.GenerateLocationConnectionVisuals(connection);
			}
		}

		// Token: 0x06001F90 RID: 8080 RVA: 0x0013CFCC File Offset: 0x0013B1CC
		private void GenerateLocationConnectionVisuals(LocationConnection connection)
		{
			Vector2 connectionStart = connection.Locations[0].MapPosition;
			Vector2 connectionEnd = connection.Locations[1].MapPosition;
			float connectionLength = Vector2.Distance(connectionStart, connectionEnd);
			int iterations = Math.Min((int)Math.Sqrt((double)(connectionLength * this.generationParams.ConnectionIndicatorIterationMultiplier)), 5);
			connection.CrackSegments.Clear();
			connection.CrackSegments.AddRange(MathUtils.GenerateJaggedLine(connectionStart, connectionEnd, iterations, connectionLength * this.generationParams.ConnectionIndicatorDisplacementMultiplier, Rand.GetRNG(Rand.RandSync.ServerAndClient), null));
		}

		// Token: 0x06001F91 RID: 8081 RVA: 0x0013D054 File Offset: 0x0013B254
		public int GetZoneIndex(float xPos)
		{
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			return MathHelper.Clamp((int)Math.Floor((double)(xPos / zoneWidth)) + 1, 1, this.generationParams.DifficultyZones);
		}

		// Token: 0x06001F92 RID: 8082 RVA: 0x0013D092 File Offset: 0x0013B292
		public Biome GetBiome(Vector2 mapPos)
		{
			return this.GetBiome(mapPos.X);
		}

		// Token: 0x06001F93 RID: 8083 RVA: 0x0013D0A0 File Offset: 0x0013B2A0
		public Biome GetBiome(float xPos)
		{
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			int zoneIndex = (int)Math.Floor((double)(xPos / zoneWidth)) + 1;
			zoneIndex = Math.Clamp(zoneIndex, 1, this.generationParams.DifficultyZones - 1);
			return this.OrderedBiomes.FirstOrDefault((Biome b) => b.AllowedZones.Contains(zoneIndex));
		}

		// Token: 0x06001F94 RID: 8084 RVA: 0x0013D110 File Offset: 0x0013B310
		private void AssignBiomes(Random rand)
		{
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			List<Biome> allowedBiomes = new List<Biome>(10);
			for (int i = 0; i < this.generationParams.DifficultyZones; i++)
			{
				int zoneIndex = i + 1;
				allowedBiomes.Clear();
				allowedBiomes.AddRange(from b in this.OrderedBiomes
				where b.AllowedZones.Contains(zoneIndex)
				select b);
				float zoneX = zoneWidth * (float)zoneIndex;
				foreach (Location location in this.Locations)
				{
					if (location.Biome == null && location.MapPosition.X < zoneX)
					{
						location.Biome = allowedBiomes[rand.Next() % allowedBiomes.Count];
					}
				}
			}
			foreach (LocationConnection connection in this.Connections)
			{
				if (connection.Biome == null)
				{
					connection.Biome = ((connection.Locations[0].MapPosition.X > connection.Locations[1].MapPosition.X) ? connection.Locations[0].Biome : connection.Locations[1].Biome);
				}
			}
		}

		// Token: 0x06001F95 RID: 8085 RVA: 0x0013D29C File Offset: 0x0013B49C
		private Location GetPreviousToEndLocation()
		{
			Location previousToEndLocation = null;
			foreach (Location location in this.Locations)
			{
				if (!location.Biome.IsEndBiome && (previousToEndLocation == null || location.MapPosition.X > previousToEndLocation.MapPosition.X))
				{
					previousToEndLocation = location;
				}
			}
			return previousToEndLocation;
		}

		// Token: 0x06001F96 RID: 8086 RVA: 0x0013D318 File Offset: 0x0013B518
		private void ForceLocationTypeToNone(CampaignMode campaign, Location location)
		{
			LocationType locationType;
			if (LocationType.Prefabs.TryGet("none", out locationType))
			{
				location.ChangeType(campaign, locationType, false, true);
			}
			location.DisallowLocationTypeChanges = true;
		}

		// Token: 0x06001F97 RID: 8087 RVA: 0x0013D34C File Offset: 0x0013B54C
		private void CreateEndLocation(CampaignMode campaign)
		{
			Map.<>c__DisplayClass111_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			float zoneWidth = (float)(this.Width / this.generationParams.DifficultyZones);
			Vector2 endPos = new Vector2((float)this.Width - zoneWidth * 0.7f, (float)(this.Height / 2));
			float closestDist = float.MaxValue;
			CS$<>8__locals1.endLocation = this.Locations.First<Location>();
			foreach (Location location in this.Locations)
			{
				float dist = Vector2.DistanceSquared(endPos, location.MapPosition);
				if (location.Biome.IsEndBiome && dist < closestDist)
				{
					CS$<>8__locals1.endLocation = location;
					closestDist = dist;
				}
			}
			Location previousToEndLocation = this.GetPreviousToEndLocation();
			if (CS$<>8__locals1.endLocation == null || previousToEndLocation == null)
			{
				return;
			}
			this.endLocations = new List<Location>
			{
				CS$<>8__locals1.endLocation
			};
			if (CS$<>8__locals1.endLocation.Biome.EndBiomeLocationCount > 1)
			{
				this.<CreateEndLocation>g__FindConnectedEndLocations|111_0(CS$<>8__locals1.endLocation, ref CS$<>8__locals1);
			}
			this.ForceLocationTypeToNone(campaign, previousToEndLocation);
			for (int i = this.Locations.Count - 1; i >= 0; i--)
			{
				if (this.Locations[i].Biome.IsEndBiome)
				{
					for (int j = this.Locations[i].Connections.Count - 1; j >= 0; j--)
					{
						if (j < this.Locations[i].Connections.Count)
						{
							LocationConnection connection = this.Locations[i].Connections[j];
							Location otherLocation = connection.OtherLocation(this.Locations[i]);
							this.Locations[i].Connections.RemoveAt(j);
							if (otherLocation != null)
							{
								otherLocation.Connections.Remove(connection);
							}
							this.Connections.Remove(connection);
						}
					}
					if (!this.endLocations.Contains(this.Locations[i]))
					{
						this.Locations.RemoveAt(i);
					}
				}
			}
			if (previousToEndLocation.Connections.None(null))
			{
				Location connectTo = this.Locations.First<Location>();
				foreach (Location location2 in this.Locations)
				{
					if (!location2.Biome.IsEndBiome && location2 != previousToEndLocation && location2.MapPosition.X > connectTo.MapPosition.X)
					{
						connectTo = location2;
					}
				}
				LocationConnection newConnection = new LocationConnection(previousToEndLocation, connectTo)
				{
					Biome = CS$<>8__locals1.endLocation.Biome,
					Difficulty = 100f
				};
				newConnection.LevelData = new LevelData(newConnection);
				this.Connections.Add(newConnection);
				previousToEndLocation.Connections.Add(newConnection);
				connectTo.Connections.Add(newConnection);
			}
			LocationConnection endConnection = new LocationConnection(previousToEndLocation, CS$<>8__locals1.endLocation)
			{
				Biome = CS$<>8__locals1.endLocation.Biome,
				Difficulty = 100f
			};
			endConnection.LevelData = new LevelData(endConnection);
			this.Connections.Add(endConnection);
			previousToEndLocation.Connections.Add(endConnection);
			CS$<>8__locals1.endLocation.Connections.Add(endConnection);
			this.AssignEndLocationLevelData(campaign);
		}

		// Token: 0x06001F98 RID: 8088 RVA: 0x0013D6E4 File Offset: 0x0013B8E4
		private void AssignEndLocationLevelData(CampaignMode campaign)
		{
			Map.<>c__DisplayClass112_0 CS$<>8__locals1 = new Map.<>c__DisplayClass112_0();
			CS$<>8__locals1.<>4__this = this;
			Map.<>c__DisplayClass112_0 CS$<>8__locals2 = CS$<>8__locals1;
			Biome biome = (from p in Biome.Prefabs
			orderby p.UintIdentifier
			select p).FirstOrDefault((Biome b) => b.IsEndBiome);
			if (biome == null)
			{
				throw new InvalidOperationException("Could not find an end biome to assign to the end locations.");
			}
			CS$<>8__locals2.endBiome = biome;
			LocationType locationType = (from p in LocationType.Prefabs
			orderby p.UintIdentifier
			select p).FirstOrDefault(new Func<LocationType, bool>(CS$<>8__locals1.<AssignEndLocationLevelData>g__IsSuitableEndLocationType|3));
			if (locationType == null)
			{
				throw new InvalidOperationException("Could not find an a location type to assign to the end locations.");
			}
			LocationType endLocationType = locationType;
			int i;
			int j;
			for (i = 0; i < this.endLocations.Count; i = j + 1)
			{
				if (this.endLocations[i].Biome != CS$<>8__locals1.endBiome)
				{
					this.endLocations[i].Biome = CS$<>8__locals1.endBiome;
					this.endLocations[i].LevelData = new LevelData(this.endLocations[i], this, this.endLocations[i].LevelData.Difficulty);
				}
				this.endLocations[i].ChangeType(campaign, endLocationType, true, true);
				if (!endLocationType.ForceLocationName.IsEmpty)
				{
					this.endLocations[i].ForceName(endLocationType.ForceLocationName);
				}
				this.endLocations[i].LevelData.ReassignGenerationParams(this.Seed);
				OutpostGenerationParams outpostParams = OutpostGenerationParams.OutpostParams.FirstOrDefault((OutpostGenerationParams p) => p.ForceToEndLocationIndex == i);
				if (outpostParams != null)
				{
					this.endLocations[i].LevelData.ForceOutpostGenerationParams = outpostParams;
				}
				j = i;
			}
		}

		// Token: 0x06001F99 RID: 8089 RVA: 0x0013D904 File Offset: 0x0013BB04
		private void ExpandBiomes(List<LocationConnection> seeds)
		{
			List<LocationConnection> nextSeeds = new List<LocationConnection>();
			foreach (LocationConnection connection in seeds)
			{
				foreach (Location location in connection.Locations)
				{
					foreach (LocationConnection otherConnection in location.Connections)
					{
						if (otherConnection != connection && otherConnection.Biome == null)
						{
							otherConnection.Biome = connection.Biome;
							nextSeeds.Add(otherConnection);
						}
					}
				}
			}
			if (nextSeeds.Count > 0)
			{
				this.ExpandBiomes(nextSeeds);
			}
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x0013D9E4 File Offset: 0x0013BBE4
		public void MoveToNextLocation()
		{
			if (this.SelectedLocation == null)
			{
				Level loaded = Level.Loaded;
				if (((loaded != null) ? loaded.EndLocation : null) != null)
				{
					this.SelectLocation(Level.Loaded.EndLocation);
				}
			}
			if (this.SelectedConnection == null && !this.endLocations.Contains(this.CurrentLocation))
			{
				DebugConsole.ThrowError("Could not move to the next location (no connection selected).\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			if (this.SelectedLocation == null)
			{
				if (!this.endLocations.Contains(this.CurrentLocation))
				{
					DebugConsole.ThrowError("Could not move to the next location (no connection selected).\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
					return;
				}
				int currentEndLocationIndex = this.endLocations.IndexOf(this.CurrentLocation);
				if (currentEndLocationIndex < this.endLocations.Count - 1)
				{
					this.SelectedLocation = this.endLocations[currentEndLocationIndex + 1];
				}
				else
				{
					this.SelectedLocation = this.StartLocation;
				}
			}
			Location prevLocation = this.CurrentLocation;
			if (this.SelectedConnection != null)
			{
				this.SelectedConnection.Passed = true;
			}
			this.CurrentLocation = this.SelectedLocation;
			this.CurrentLocation.CreateStores(false);
			this.Discover(this.CurrentLocation, true);
			this.Visit(this.CurrentLocation, true);
			this.SelectedLocation = null;
			NamedEvent<Map.LocationChangeInfo> onLocationChanged = this.OnLocationChanged;
			if (onLocationChanged != null)
			{
				onLocationChanged.Invoke(new Map.LocationChangeInfo(prevLocation, this.CurrentLocation));
			}
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign != null)
				{
					CampaignMetadata metadata = campaign.CampaignMetadata;
					if (metadata != null)
					{
						metadata.SetValue("campaign.location.id".ToIdentifier(), this.CurrentLocationIndex);
						metadata.SetValue("campaign.location.name".ToIdentifier(), this.CurrentLocation.NameIdentifier.Value);
						CampaignMetadata campaignMetadata = metadata;
						Identifier identifier = "campaign.location.biome".ToIdentifier();
						Biome biome = this.CurrentLocation.Biome;
						campaignMetadata.SetValue(identifier, (biome != null) ? biome.Identifier : "null".ToIdentifier());
						CampaignMetadata campaignMetadata2 = metadata;
						Identifier identifier2 = "campaign.location.type".ToIdentifier();
						LocationType type = this.CurrentLocation.Type;
						campaignMetadata2.SetValue(identifier2, (type != null) ? type.Identifier : "null".ToIdentifier());
					}
				}
			}
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x0013DC18 File Offset: 0x0013BE18
		public void SetLocation(int index)
		{
			if (index == -1)
			{
				this.CurrentLocation = null;
				return;
			}
			if (index < 0 || index >= this.Locations.Count)
			{
				DebugConsole.ThrowError("Location index out of bounds", null, null, false, false);
				return;
			}
			Location prevLocation = this.CurrentLocation;
			this.CurrentLocation = this.Locations[index];
			this.Discover(this.CurrentLocation, true);
			this.CurrentLocation.CreateStores(false);
			if (prevLocation != this.CurrentLocation)
			{
				LocationConnection connection = this.CurrentLocation.Connections.Find((LocationConnection c) => c.Locations.Contains(prevLocation));
				if (connection != null)
				{
					connection.Passed = true;
				}
				NamedEvent<Map.LocationChangeInfo> onLocationChanged = this.OnLocationChanged;
				if (onLocationChanged == null)
				{
					return;
				}
				onLocationChanged.Invoke(new Map.LocationChangeInfo(prevLocation, this.CurrentLocation));
			}
		}

		// Token: 0x06001F9C RID: 8092 RVA: 0x0013DCE8 File Offset: 0x0013BEE8
		public void SelectLocation(int index)
		{
			Map.<>c__DisplayClass116_0 CS$<>8__locals1 = new Map.<>c__DisplayClass116_0();
			CS$<>8__locals1.<>4__this = this;
			if (index == -1)
			{
				this.SelectedLocation = null;
				this.SelectedConnection = null;
				Action<Location, LocationConnection> onLocationSelected = this.OnLocationSelected;
				if (onLocationSelected == null)
				{
					return;
				}
				onLocationSelected(null, null);
				return;
			}
			else
			{
				if (index < 0 || index >= this.Locations.Count)
				{
					DebugConsole.ThrowError("Location index out of bounds", null, null, false, false);
					return;
				}
				Location prevSelected = this.SelectedLocation;
				this.SelectedLocation = this.Locations[index];
				Map.<>c__DisplayClass116_0 CS$<>8__locals2 = CS$<>8__locals1;
				GameSession gameSession = GameMain.GameSession;
				Location currentDisplayLocation;
				if (gameSession == null)
				{
					currentDisplayLocation = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					currentDisplayLocation = ((campaign != null) ? campaign.GetCurrentDisplayLocation() : null);
				}
				CS$<>8__locals2.currentDisplayLocation = currentDisplayLocation;
				if (CS$<>8__locals1.currentDisplayLocation == this.SelectedLocation)
				{
					this.SelectedConnection = this.Connections.Find((LocationConnection c) => c.Locations.Contains(CS$<>8__locals1.<>4__this.CurrentLocation) && c.Locations.Contains(CS$<>8__locals1.<>4__this.SelectedLocation));
				}
				else
				{
					this.SelectedConnection = (this.Connections.Find((LocationConnection c) => c.Locations.Contains(CS$<>8__locals1.currentDisplayLocation) && c.Locations.Contains(CS$<>8__locals1.<>4__this.SelectedLocation)) ?? this.Connections.Find((LocationConnection c) => c.Locations.Contains(CS$<>8__locals1.<>4__this.CurrentLocation) && c.Locations.Contains(CS$<>8__locals1.<>4__this.SelectedLocation)));
				}
				LocationConnection selectedConnection = this.SelectedConnection;
				if (selectedConnection != null && selectedConnection.Locked)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 4);
					defaultInterpolatedStringHandler.AppendLiteral("A locked connection was selected (");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.SelectedConnection.Locations[0].DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral(" -> ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.SelectedConnection.Locations[1].DisplayName);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendLiteral(" Current location: ");
					defaultInterpolatedStringHandler.AppendFormatted<Location>(this.CurrentLocation);
					defaultInterpolatedStringHandler.AppendLiteral(", current display location: ");
					defaultInterpolatedStringHandler.AppendFormatted<Location>(CS$<>8__locals1.currentDisplayLocation);
					defaultInterpolatedStringHandler.AppendLiteral(").\n");
					string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace.CleanupStackTrace();
					GameAnalyticsManager.AddErrorEventOnce("MapSelectLocation:LockedConnectionSelected", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
				}
				if (prevSelected != this.SelectedLocation)
				{
					Action<Location, LocationConnection> onLocationSelected2 = this.OnLocationSelected;
					if (onLocationSelected2 == null)
					{
						return;
					}
					onLocationSelected2(this.SelectedLocation, this.SelectedConnection);
				}
				return;
			}
		}

		// Token: 0x06001F9D RID: 8093 RVA: 0x0013DEF0 File Offset: 0x0013C0F0
		public void SelectLocation(Location location)
		{
			if (!this.Locations.Contains(location))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to select a location. ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(((location != null) ? location.DisplayName : null) ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral(" not found in the map.");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Map.SelectLocation:LocationNotFound", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			Location prevSelected = this.SelectedLocation;
			this.SelectedLocation = location;
			this.SelectedConnection = this.Connections.Find((LocationConnection c) => c.Locations.Contains(this.CurrentLocation) && c.Locations.Contains(this.SelectedLocation));
			LocationConnection selectedConnection = this.SelectedConnection;
			if (selectedConnection != null && selectedConnection.Locked)
			{
				DebugConsole.ThrowError("A locked connection was selected - this should not be possible.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
			}
			if (prevSelected != this.SelectedLocation)
			{
				Action<Location, LocationConnection> onLocationSelected = this.OnLocationSelected;
				if (onLocationSelected == null)
				{
					return;
				}
				onLocationSelected(this.SelectedLocation, this.SelectedConnection);
			}
		}

		// Token: 0x06001F9E RID: 8094 RVA: 0x0013DFF0 File Offset: 0x0013C1F0
		public void SelectMission(IEnumerable<int> missionIndices)
		{
			if (this.CurrentLocation == null)
			{
				string errorMsg = "Failed to select a mission (current location not set).";
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Map.SelectMission:CurrentLocationNotSet", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return;
			}
			if (!missionIndices.SequenceEqual(this.GetSelectedMissionIndices()))
			{
				this.CurrentLocation.SetSelectedMissionIndices(missionIndices);
				foreach (Mission selectedMission in this.CurrentLocation.SelectedMissions.ToList<Mission>())
				{
					if (selectedMission.Locations[0] != this.CurrentLocation || selectedMission.Locations[1] != this.CurrentLocation)
					{
						if (this.SelectedConnection == null)
						{
							return;
						}
						if (selectedMission.Locations[1] != this.SelectedLocation)
						{
							this.CurrentLocation.DeselectMission(selectedMission);
						}
					}
				}
				Action<LocationConnection, IEnumerable<Mission>> onMissionsSelected = this.OnMissionsSelected;
				if (onMissionsSelected == null)
				{
					return;
				}
				onMissionsSelected(this.SelectedConnection, this.CurrentLocation.SelectedMissions);
			}
		}

		// Token: 0x06001F9F RID: 8095 RVA: 0x0013E0F0 File Offset: 0x0013C2F0
		public void SelectRandomLocation(bool preferUndiscovered)
		{
			List<Location> nextLocations = (from c in this.CurrentLocation.Connections
			where !c.Locked
			select c.OtherLocation(this.CurrentLocation)).ToList<Location>();
			List<Location> undiscoveredLocations = nextLocations.FindAll((Location l) => !l.Discovered);
			if (undiscoveredLocations.Count > 0 && preferUndiscovered)
			{
				this.SelectLocation(undiscoveredLocations[Rand.Int(undiscoveredLocations.Count, Rand.RandSync.Unsynced)]);
				return;
			}
			this.SelectLocation(nextLocations[Rand.Int(nextLocations.Count, Rand.RandSync.Unsynced)]);
		}

		// Token: 0x06001FA0 RID: 8096 RVA: 0x0013E1A8 File Offset: 0x0013C3A8
		public void ProgressWorld(CampaignMode campaign, CampaignMode.TransitionType transitionType, float roundDuration)
		{
			int steps = (int)Math.Floor((double)(roundDuration / 600f));
			if (transitionType == CampaignMode.TransitionType.ProgressToNextLocation || transitionType == CampaignMode.TransitionType.ProgressToNextEmptyLocation)
			{
				steps = Math.Max(1, steps);
			}
			steps = Math.Min(steps, 5);
			for (int i = 0; i < steps; i++)
			{
				this.ProgressWorld(campaign);
			}
			for (int j = 0; j < Math.Max(1, steps); j++)
			{
				foreach (Location location in this.Locations)
				{
					if (location.Discovered)
					{
						location.UpdateSpecials();
					}
				}
			}
			Radiation radiation = this.Radiation;
			if (radiation == null)
			{
				return;
			}
			radiation.OnStep((float)steps);
		}

		// Token: 0x06001FA1 RID: 8097 RVA: 0x0013E264 File Offset: 0x0013C464
		private void ProgressWorld(CampaignMode campaign)
		{
			foreach (Location location in this.Locations)
			{
				if (location.Visited)
				{
					location.WorldStepsSinceVisited++;
					if (location.WorldStepsSinceVisited > 10)
					{
						location.ClearStores();
					}
				}
				else
				{
					location.ClearStores();
				}
				location.LevelData.ResetExhaustedEventSets();
				if (location.Discovered && (this.furthestDiscoveredLocation == null || location.MapPosition.X > this.furthestDiscoveredLocation.MapPosition.X))
				{
					this.furthestDiscoveredLocation = location;
				}
			}
			foreach (LocationConnection connection in this.Connections)
			{
				connection.LevelData.ResetExhaustedEventSets();
			}
			foreach (Location location2 in this.Locations)
			{
				if (location2.MapPosition.X <= this.furthestDiscoveredLocation.MapPosition.X)
				{
					bool shouldUpdateStores = location2.Discovered;
					bool shouldProcessLocationTypeChanges = location2 != this.CurrentLocation && location2 != this.SelectedLocation && !location2.IsGateBetweenBiomes;
					if (shouldProcessLocationTypeChanges && this.ProgressLocationTypeChanges(campaign, location2))
					{
						shouldUpdateStores = false;
					}
					if (shouldUpdateStores)
					{
						location2.UpdateStores(false);
					}
				}
			}
			if (this.CurrentLocation != null)
			{
				this.CurrentLocation.UpdateStores(true);
				this.CurrentLocation.WorldStepsSinceVisited = 0;
			}
		}

		// Token: 0x06001FA2 RID: 8098 RVA: 0x0013E428 File Offset: 0x0013C628
		private bool ProgressLocationTypeChanges(CampaignMode campaign, Location location)
		{
			location.TimeSinceLastTypeChange++;
			location.LocationTypeChangeCooldown--;
			if (location.PendingLocationTypeChange != null)
			{
				if (location.PendingLocationTypeChange.Value.Item1.DetermineProbability(location) <= 0f)
				{
					location.PendingLocationTypeChange = null;
				}
				else
				{
					location.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(location.PendingLocationTypeChange.Value.Item1, location.PendingLocationTypeChange.Value.Item2 - 1, location.PendingLocationTypeChange.Value.Item3));
					if (location.PendingLocationTypeChange.Value.Item2 <= 0)
					{
						return this.ChangeLocationType(campaign, location, location.PendingLocationTypeChange.Value.Item1);
					}
				}
			}
			Dictionary<LocationTypeChange, float> allowedTypeChanges = new Dictionary<LocationTypeChange, float>();
			foreach (LocationTypeChange typeChange in location.Type.CanChangeTo)
			{
				float probability = typeChange.DetermineProbability(location);
				if (probability > 0f)
				{
					allowedTypeChanges.Add(typeChange, probability);
				}
			}
			if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < allowedTypeChanges.Sum((KeyValuePair<LocationTypeChange, float> change) => change.Value))
			{
				LocationTypeChange selectedTypeChange = ToolBox.SelectWeightedRandom<LocationTypeChange>(allowedTypeChanges.Keys.ToList<LocationTypeChange>(), allowedTypeChanges.Values.ToList<float>(), Rand.RandSync.Unsynced);
				if (selectedTypeChange != null)
				{
					if (selectedTypeChange.RequiredDurationRange.X > 0)
					{
						location.PendingLocationTypeChange = new ValueTuple<LocationTypeChange, int, MissionPrefab>?(new ValueTuple<LocationTypeChange, int, MissionPrefab>(selectedTypeChange, Rand.Range(selectedTypeChange.RequiredDurationRange.X, selectedTypeChange.RequiredDurationRange.Y, Rand.RandSync.Unsynced), null));
						return false;
					}
					return this.ChangeLocationType(campaign, location, selectedTypeChange);
				}
			}
			foreach (LocationTypeChange typeChange2 in location.Type.CanChangeTo)
			{
				foreach (LocationTypeChange.Requirement requirement in typeChange2.Requirements)
				{
					if (requirement.AnyWithinDistance(location, requirement.RequiredProximityForProbabilityIncrease))
					{
						if (!location.ProximityTimer.ContainsKey(requirement))
						{
							location.ProximityTimer[requirement] = 0;
						}
						Dictionary<LocationTypeChange.Requirement, int> proximityTimer = location.ProximityTimer;
						LocationTypeChange.Requirement key = requirement;
						proximityTimer[key]++;
					}
					else
					{
						location.ProximityTimer.Remove(requirement);
					}
				}
			}
			return false;
		}

		// Token: 0x06001FA3 RID: 8099 RVA: 0x0013E6EC File Offset: 0x0013C8EC
		private bool ChangeLocationType(CampaignMode campaign, Location location, LocationTypeChange change)
		{
			LocalizedString prevName = location.DisplayName;
			LocationType newType;
			if (!LocationType.Prefabs.TryGet(change.ChangeToType, out newType))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to change the type of the location \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(location.DisplayName);
				defaultInterpolatedStringHandler.AppendLiteral("\". Location type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(change.ChangeToType);
				defaultInterpolatedStringHandler.AppendLiteral("\" not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return false;
			}
			if (location.LocationTypeChangesBlocked)
			{
				return false;
			}
			if (newType.OutpostTeam != location.Type.OutpostTeam || newType.HasOutpost != location.Type.HasOutpost)
			{
				location.ClearMissions();
			}
			location.ChangeType(campaign, newType, false, true);
			this.ChangeLocationTypeProjSpecific(location, prevName, change);
			foreach (LocationTypeChange.Requirement requirement in change.Requirements)
			{
				location.ProximityTimer.Remove(requirement);
			}
			location.TimeSinceLastTypeChange = 0;
			location.LocationTypeChangeCooldown = change.CooldownAfterChange;
			location.PendingLocationTypeChange = null;
			return true;
		}

		// Token: 0x06001FA4 RID: 8100 RVA: 0x0013E828 File Offset: 0x0013CA28
		public static bool LocationOrConnectionWithinDistance(Location startLocation, int maxDistance, Func<Location, bool> criteria, Func<LocationConnection, bool> connectionCriteria = null)
		{
			return Map.GetDistanceToClosestLocationOrConnection(startLocation, maxDistance, criteria, connectionCriteria) <= maxDistance;
		}

		// Token: 0x06001FA5 RID: 8101 RVA: 0x0013E83C File Offset: 0x0013CA3C
		public static int GetDistanceToClosestLocationOrConnection(Location startLocation, int maxDistance, Func<Location, bool> criteria, Func<LocationConnection, bool> connectionCriteria = null)
		{
			int distance = 0;
			List<Location> locationsToTest = new List<Location>
			{
				startLocation
			};
			HashSet<Location> nextBatchToTest = new HashSet<Location>();
			HashSet<Location> checkedLocations = new HashSet<Location>();
			while (locationsToTest.Any<Location>())
			{
				foreach (Location location in locationsToTest)
				{
					checkedLocations.Add(location);
					if (criteria(location))
					{
						return distance;
					}
					foreach (LocationConnection connection in location.Connections)
					{
						if (connectionCriteria != null && connectionCriteria(connection))
						{
							return distance;
						}
						Location otherLocation = connection.OtherLocation(location);
						if (!checkedLocations.Contains(otherLocation))
						{
							nextBatchToTest.Add(otherLocation);
						}
					}
					if (distance > maxDistance)
					{
						return int.MaxValue;
					}
				}
				distance++;
				locationsToTest.Clear();
				locationsToTest.AddRange(nextBatchToTest);
				nextBatchToTest.Clear();
			}
			return int.MaxValue;
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x0013E970 File Offset: 0x0013CB70
		private void ChangeLocationTypeProjSpecific(Location location, LocalizedString prevName, LocationTypeChange change)
		{
			IReadOnlyList<string> messages = change.GetMessages(location.Faction);
			if (!messages.Any<string>())
			{
				return;
			}
			string random = messages.GetRandom(Rand.RandSync.Unsynced);
			string oldValue = "[previousname]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
			defaultInterpolatedStringHandler.AppendLiteral("‖color:gui.yellow‖");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(prevName);
			defaultInterpolatedStringHandler.AppendLiteral("‖end‖");
			string text = random.Replace(oldValue, defaultInterpolatedStringHandler.ToStringAndClear());
			string oldValue2 = "[name]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("‖color:gui.yellow‖");
			defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(location.DisplayName);
			defaultInterpolatedStringHandler2.AppendLiteral("‖end‖");
			string msg = text.Replace(oldValue2, defaultInterpolatedStringHandler2.ToStringAndClear());
			location.LastTypeChangeMessage = msg;
			this.mapNotifications.Add(new Map.MapNotification(msg, GUIStyle.SubHeadingFont, this.mapNotifications, location));
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x0013EA3A File Offset: 0x0013CC3A
		private void ClearAnimQueue()
		{
			this.mapAnimQueue.Clear();
		}

		// Token: 0x06001FA8 RID: 8104 RVA: 0x0013EA48 File Offset: 0x0013CC48
		public void Discover(Location location, bool checkTalents = true)
		{
			if (location == null)
			{
				return;
			}
			if (this.locationsDiscovered.Contains(location))
			{
				return;
			}
			this.locationsDiscovered.Add(location);
			if (checkTalents)
			{
				GameSession.GetSessionCrewCharacters(CharacterType.Both).ForEach(delegate(Character c)
				{
					c.CheckTalents(AbilityEffectType.OnLocationDiscovered, new Location.AbilityLocation(location));
				});
			}
		}

		// Token: 0x06001FA9 RID: 8105 RVA: 0x0013EAAA File Offset: 0x0013CCAA
		public void Visit(Location location, bool resetTimeSinceVisited = true)
		{
			if (location == null)
			{
				return;
			}
			if (resetTimeSinceVisited)
			{
				location.WorldStepsSinceVisited = 0;
			}
			if (this.locationsVisited.Contains(location))
			{
				return;
			}
			this.locationsVisited.Add(location);
			this.RemoveFogOfWarProjSpecific(location);
		}

		// Token: 0x06001FAA RID: 8106 RVA: 0x0013EADC File Offset: 0x0013CCDC
		public void ClearLocationHistory()
		{
			this.locationsDiscovered.Clear();
			this.locationsVisited.Clear();
		}

		// Token: 0x06001FAB RID: 8107 RVA: 0x0013EAF4 File Offset: 0x0013CCF4
		public int? GetDiscoveryIndex(Location location)
		{
			if (!this.trackedLocationDiscoveryAndVisitOrder)
			{
				return null;
			}
			if (location == null)
			{
				return new int?(-1);
			}
			return new int?(this.locationsDiscovered.IndexOf(location));
		}

		// Token: 0x06001FAC RID: 8108 RVA: 0x0013EB30 File Offset: 0x0013CD30
		public int? GetVisitIndex(Location location, bool includeLocationsWithoutOutpost = false)
		{
			if (!this.trackedLocationDiscoveryAndVisitOrder)
			{
				return null;
			}
			if (location == null)
			{
				return new int?(-1);
			}
			int index = this.locationsVisited.IndexOf(location);
			if (includeLocationsWithoutOutpost)
			{
				return new int?(index);
			}
			int noOutpostLocations = 0;
			for (int i = 0; i < index; i++)
			{
				Location j = this.locationsVisited[i];
				if (j != null && !j.HasOutpost())
				{
					noOutpostLocations++;
				}
			}
			return new int?(index - noOutpostLocations);
		}

		// Token: 0x06001FAD RID: 8109 RVA: 0x0013EBA5 File Offset: 0x0013CDA5
		public bool IsDiscovered(Location location)
		{
			return location != null && this.locationsDiscovered.Contains(location);
		}

		// Token: 0x06001FAE RID: 8110 RVA: 0x0013EBB8 File Offset: 0x0013CDB8
		public bool IsVisited(Location location)
		{
			return location != null && this.locationsVisited.Contains(location);
		}

		// Token: 0x06001FAF RID: 8111 RVA: 0x0013EBCB File Offset: 0x0013CDCB
		private void RemoveFogOfWarProjSpecific(Location location)
		{
			this.RemoveFogOfWar(location, true);
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x0013EBD8 File Offset: 0x0013CDD8
		public static Map Load(CampaignMode campaign, XElement element)
		{
			Map map = new Map(campaign, element);
			map.LoadState(campaign, element, false);
			map.DrawOffset = -map.CurrentLocation.MapPosition;
			return map;
		}

		// Token: 0x06001FB1 RID: 8113 RVA: 0x0013EC10 File Offset: 0x0013CE10
		public void LoadState(CampaignMode campaign, XElement element, bool showNotifications)
		{
			this.ClearAnimQueue();
			this.SetLocation(element.GetAttributeInt("currentlocation", 0));
			Version version;
			if (!Version.TryParse(element.GetAttributeString("version", ""), out version))
			{
				DebugConsole.ThrowError("Incompatible map save file, loading the game failed.", null, null, false, false);
				return;
			}
			this.ClearLocationHistory();
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "location"))
				{
					if (!(a == "connection"))
					{
						if (!(a == "radiation"))
						{
							if (!(a == "discovered"))
							{
								if (!(a == "visited"))
								{
									continue;
								}
							}
							else
							{
								bool trackedVisitedEmptyLocations = subElement.GetAttributeBool("trackedvisitedemptylocations", false);
								int[] discoveredIndices = subElement.GetAttributeIntArray("indices", Array.Empty<int>());
								foreach (int discoveredIndex in discoveredIndices)
								{
									this.<LoadState>g__Discover|137_0(this.Locations[discoveredIndex]);
								}
								using (IEnumerator<XElement> enumerator2 = subElement.GetChildElements("location", StringComparison.OrdinalIgnoreCase).GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										XElement childElement = enumerator2.Current;
										Location i = this.<LoadState>g__GetLocation|137_2(childElement);
										if (i != null)
										{
											this.<LoadState>g__Discover|137_0(i);
											if (!trackedVisitedEmptyLocations)
											{
												if (!i.HasOutpost())
												{
													this.Visit(i, false);
												}
												this.trackedLocationDiscoveryAndVisitOrder = false;
											}
										}
									}
									continue;
								}
							}
							int[] visitedIndices = subElement.GetAttributeIntArray("indices", Array.Empty<int>());
							foreach (int visitedIndex in visitedIndices)
							{
								this.Visit(this.Locations[visitedIndex], false);
							}
							foreach (XElement childElement2 in subElement.GetChildElements("location", StringComparison.OrdinalIgnoreCase))
							{
								Location j = this.<LoadState>g__GetLocation|137_2(childElement2);
								if (j != null)
								{
									this.Visit(j, false);
								}
							}
						}
						else
						{
							this.Radiation = new Radiation(this, this.generationParams.RadiationParams, subElement);
						}
					}
					else if (subElement.Attribute("i") != null)
					{
						int connectionIndex = subElement.GetAttributeInt("i", -1);
						if (connectionIndex < 0 || connectionIndex >= this.Connections.Count)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Error while loading the campaign map: connection index out of bounds (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(connectionIndex);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						}
						else
						{
							this.Connections[connectionIndex].Passed = subElement.GetAttributeBool("passed", false);
							this.Connections[connectionIndex].Locked = subElement.GetAttributeBool("locked", false);
						}
					}
				}
				else
				{
					int locationIndex = subElement.GetAttributeInt("i", -1);
					if (locationIndex < 0 || locationIndex >= this.Locations.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(69, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Error while loading the campaign map: location index out of bounds (");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(locationIndex);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
					}
					else
					{
						Location location = this.Locations[locationIndex];
						location.ProximityTimer.Clear();
						for (int k = 0; k < location.Type.CanChangeTo.Count; k++)
						{
							for (int l2 = 0; l2 < location.Type.CanChangeTo[k].Requirements.Count; l2++)
							{
								location.ProximityTimer.Add(location.Type.CanChangeTo[k].Requirements[l2], subElement.GetAttributeInt("changetimer" + k.ToString() + "-" + l2.ToString(), 0));
							}
						}
						location.LoadLocationTypeChange(subElement);
						location.LoadChangingProperties(subElement, campaign);
						if (subElement.GetAttributeBool("discovered", false))
						{
							this.<LoadState>g__Discover|137_0(location);
							this.Visit(location, false);
							this.trackedLocationDiscoveryAndVisitOrder = false;
						}
						Identifier locationType = subElement.GetAttributeIdentifier("type", Identifier.Empty);
						LocalizedString prevLocationName = location.DisplayName;
						LocationType prevLocationType = location.Type;
						LocationType newLocationType = LocationType.Prefabs.Find((LocationType lt) => lt.Identifier == locationType) ?? LocationType.Prefabs.GetOrdered().First<LocationType>();
						location.ChangeType(campaign, newLocationType, true, true);
						if (showNotifications && prevLocationType != location.Type)
						{
							LocationTypeChange change = prevLocationType.CanChangeTo.Find((LocationTypeChange c) => c.ChangeToType == location.Type.Identifier);
							if (change != null)
							{
								this.ChangeLocationTypeProjSpecific(location, prevLocationName, change);
								location.TimeSinceLastTypeChange = 0;
							}
						}
						location.LoadStores(subElement);
						location.LoadMissions(subElement);
					}
				}
			}
			foreach (Location location3 in this.Locations)
			{
				if (location3 != null)
				{
					location3.InstantiateLoadedMissions(this);
				}
			}
			if (version < new Version(1, 0))
			{
				if (this.Locations.None((Location l) => l.Faction != null || l.SecondaryFaction != null))
				{
					Rand.SetSyncedSeed(ToolBox.StringToInt(this.Seed));
					foreach (Location location2 in this.Locations)
					{
						if (location2.Type.HasOutpost && campaign != null && location2.Type.OutpostTeam == CharacterTeamType.FriendlyNPC)
						{
							location2.Faction = campaign.GetRandomFaction(Rand.RandSync.ServerAndClient, true);
							if (location2 != this.StartLocation)
							{
								location2.SecondaryFaction = campaign.GetRandomSecondaryFaction(Rand.RandSync.ServerAndClient, true);
							}
						}
					}
				}
			}
			int currentLocationConnection = element.GetAttributeInt("currentlocationconnection", -1);
			if (currentLocationConnection >= 0)
			{
				this.Connections[currentLocationConnection].Locked = false;
				this.SelectLocation(this.Connections[currentLocationConnection].OtherLocation(this.CurrentLocation));
			}
			else if (this.CurrentLocation != null && !this.CurrentLocation.Type.HasOutpost && this.SelectedConnection == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(124, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Error while loading campaign map state. Submarine in a location with no outpost (");
				defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(this.CurrentLocation.DisplayName);
				defaultInterpolatedStringHandler3.AppendLiteral("). Loading the first adjacent connection...");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
				this.SelectLocation(this.CurrentLocation.Connections[0].OtherLocation(this.CurrentLocation));
			}
			Location previousToEndLocation = this.GetPreviousToEndLocation();
			if (previousToEndLocation != null)
			{
				this.ForceLocationTypeToNone(campaign, previousToEndLocation);
			}
		}

		// Token: 0x06001FB2 RID: 8114 RVA: 0x0013F3FC File Offset: 0x0013D5FC
		public void Save(XElement element)
		{
			XElement mapElement = new XElement("map");
			mapElement.Add(new XAttribute("version", GameMain.Version.ToString()));
			mapElement.Add(new XAttribute("currentlocation", this.CurrentLocationIndex));
			GameMode gameMode = GameMain.GameSession.GameMode;
			CampaignMode campaign = gameMode as CampaignMode;
			if (campaign != null)
			{
				if (campaign.NextLevel != null && campaign.NextLevel.Type == LevelData.LevelType.LocationConnection)
				{
					mapElement.Add(new XAttribute("currentlocationconnection", this.Connections.IndexOf(this.CurrentLocation.Connections.Find((LocationConnection c) => c.LevelData == campaign.NextLevel))));
				}
				else if (Level.Loaded != null && Level.Loaded.Type == LevelData.LevelType.LocationConnection && !this.CurrentLocation.Type.HasOutpost)
				{
					mapElement.Add(new XAttribute("currentlocationconnection", this.Connections.IndexOf(this.Connections.Find((LocationConnection c) => c.LevelData == Level.Loaded.LevelData))));
				}
			}
			mapElement.Add(new XAttribute("width", this.Width));
			mapElement.Add(new XAttribute("height", this.Height));
			mapElement.Add(new XAttribute("selectedlocation", this.SelectedLocationIndex));
			mapElement.Add(new XAttribute("startlocation", this.Locations.IndexOf(this.StartLocation)));
			mapElement.Add(new XAttribute("endlocations", string.Join<int>(',', from e in this.EndLocations
			select this.Locations.IndexOf(e))));
			mapElement.Add(new XAttribute("seed", this.Seed));
			for (int i = 0; i < this.Locations.Count; i++)
			{
				Location location = this.Locations[i];
				XElement locationElement = location.Save(this, mapElement);
				locationElement.Add(new XAttribute("i", i));
			}
			for (int j = 0; j < this.Connections.Count; j++)
			{
				LocationConnection connection = this.Connections[j];
				XElement connectionElement = new XElement("connection", new object[]
				{
					new XAttribute("passed", connection.Passed),
					new XAttribute("locked", connection.Locked),
					new XAttribute("difficulty", connection.Difficulty),
					new XAttribute("biome", connection.Biome.Identifier),
					new XAttribute("i", j),
					new XAttribute("locations", this.Locations.IndexOf(connection.Locations[0]).ToString() + "," + this.Locations.IndexOf(connection.Locations[1]).ToString())
				});
				connection.LevelData.Save(connectionElement);
				mapElement.Add(connectionElement);
			}
			if (this.Radiation != null)
			{
				mapElement.Add(this.Radiation.Save());
			}
			if (this.locationsDiscovered.Any<Location>())
			{
				XElement discoveryElement = new XElement("discovered", new object[]
				{
					new XAttribute("trackedvisitedemptylocations", true),
					new XAttribute("indices", string.Join<int>(',', from l in this.locationsDiscovered
					select this.Locations.IndexOf(l)))
				});
				mapElement.Add(discoveryElement);
			}
			if (this.locationsVisited.Any<Location>())
			{
				XElement visitElement = new XElement("visited", new XAttribute("indices", string.Join<int>(',', from l in this.locationsVisited
				select this.Locations.IndexOf(l))));
				mapElement.Add(visitElement);
			}
			element.Add(mapElement);
		}

		// Token: 0x06001FB3 RID: 8115 RVA: 0x0013F8B4 File Offset: 0x0013DAB4
		public void Remove()
		{
			foreach (Location location in this.Locations)
			{
				location.Remove();
			}
			this.RemoveProjSpecific();
		}

		// Token: 0x06001FB4 RID: 8116 RVA: 0x0013F90C File Offset: 0x0013DB0C
		private void RemoveProjSpecific()
		{
			Sprite sprite = Map.noiseOverlay;
			if (sprite != null)
			{
				sprite.Remove();
			}
			Map.noiseOverlay = null;
		}

		// Token: 0x06001FB5 RID: 8117 RVA: 0x0013F924 File Offset: 0x0013DB24
		[CompilerGenerated]
		private void <DrawConnection>g__DrawIcon|33_0(string iconStyle, int iconSize, RichString tooltipText, ref Map.<>c__DisplayClass33_0 A_4)
		{
			Vector2 iconPos = (A_4.connectionStart.Value + A_4.connectionEnd.Value) / 2f;
			Vector2 iconDiff = Vector2.Normalize(A_4.connectionEnd.Value - A_4.connectionStart.Value) * (float)iconSize;
			iconPos += iconDiff * (float)(-(float)(A_4.iconCount - 1)) / 2f + iconDiff * (float)A_4.iconIndex;
			GUIComponentStyle style = GUIStyle.GetComponentStyle(iconStyle);
			bool mouseOn = Vector2.DistanceSquared(iconPos, PlayerInput.MousePosition) < (float)(iconSize * iconSize) && this.IsPreferredTooltip(iconPos);
			Sprite iconSprite = style.GetDefaultSprite();
			iconSprite.Draw(A_4.spriteBatch, iconPos, (mouseOn ? style.HoverColor : style.Color) * 0.7f, 0f, (float)iconSize / iconSprite.size.X, SpriteEffects.None, null);
			if (mouseOn)
			{
				this.tooltip = new ValueTuple<Rectangle, RichString>?(new ValueTuple<Rectangle, RichString>(new Rectangle((iconPos - Vector2.One * (float)iconSize / 2f).ToPoint(), new Point(iconSize)), tooltipText));
			}
			int iconIndex = A_4.iconIndex;
			A_4.iconIndex = iconIndex + 1;
		}

		// Token: 0x06001FB6 RID: 8118 RVA: 0x0013FA84 File Offset: 0x0013DC84
		[CompilerGenerated]
		private void <.ctor>g__FindStartLocation|100_2(Func<Location, bool> predicate)
		{
			foreach (Location location in this.Locations)
			{
				if (predicate(location) && (this.CurrentLocation == null || location.MapPosition.X < this.CurrentLocation.MapPosition.X))
				{
					this.CurrentLocation = (this.StartLocation = (this.furthestDiscoveredLocation = location));
				}
			}
		}

		// Token: 0x06001FB8 RID: 8120 RVA: 0x0013FB2C File Offset: 0x0013DD2C
		[CompilerGenerated]
		internal static bool <Generate>g__AllowAsBiomeGate|102_11(LocationType lt)
		{
			return lt.HasOutpost && lt.Identifier != "abandoned" && lt.BiomeGate != LocationType.BiomeGateSetting.Deny;
		}

		// Token: 0x06001FB9 RID: 8121 RVA: 0x0013FB58 File Offset: 0x0013DD58
		[CompilerGenerated]
		private void <CreateEndLocation>g__FindConnectedEndLocations|111_0(Location currLocation, ref Map.<>c__DisplayClass111_0 A_2)
		{
			if (this.endLocations.Count >= A_2.endLocation.Biome.EndBiomeLocationCount)
			{
				return;
			}
			foreach (LocationConnection connection in currLocation.Connections)
			{
				if (connection.Biome == A_2.endLocation.Biome)
				{
					Location otherLocation = connection.OtherLocation(currLocation);
					if (otherLocation != null && !this.endLocations.Contains(otherLocation))
					{
						if (this.endLocations.Count >= A_2.endLocation.Biome.EndBiomeLocationCount)
						{
							break;
						}
						this.endLocations.Add(otherLocation);
						this.<CreateEndLocation>g__FindConnectedEndLocations|111_0(otherLocation, ref A_2);
					}
				}
			}
		}

		// Token: 0x06001FBC RID: 8124 RVA: 0x0013FC5C File Offset: 0x0013DE5C
		[CompilerGenerated]
		private Location <LoadState>g__GetLocation|137_2(XElement element)
		{
			int index = element.GetAttributeInt("i", -1);
			if (index < 0)
			{
				return null;
			}
			return this.Locations[index];
		}

		// Token: 0x06001FBD RID: 8125 RVA: 0x0013FC88 File Offset: 0x0013DE88
		[CompilerGenerated]
		private void <LoadState>g__Discover|137_0(Location location)
		{
			this.Discover(location, false);
			if (this.furthestDiscoveredLocation == null || location.MapPosition.X > this.furthestDiscoveredLocation.MapPosition.X)
			{
				this.furthestDiscoveredLocation = location;
			}
		}

		// Token: 0x04001021 RID: 4129
		private readonly Queue<Map.MapAnim> mapAnimQueue = new Queue<Map.MapAnim>();

		// Token: 0x04001023 RID: 4131
		private static Sprite noiseOverlay;

		// Token: 0x04001024 RID: 4132
		public Vector2 DrawOffset;

		// Token: 0x04001025 RID: 4133
		private Vector2 drawOffsetNoise;

		// Token: 0x04001026 RID: 4134
		private Vector2 currLocationIndicatorPos;

		// Token: 0x04001027 RID: 4135
		private float zoom = 3f;

		// Token: 0x04001028 RID: 4136
		private float targetZoom;

		// Token: 0x04001029 RID: 4137
		private Rectangle borders;

		// Token: 0x0400102A RID: 4138
		private Sprite[,] mapTiles;

		// Token: 0x0400102B RID: 4139
		private bool[,] tileDiscovered;

		// Token: 0x0400102C RID: 4140
		private float connectionHighlightState;

		// Token: 0x0400102D RID: 4141
		[TupleElementNames(new string[]
		{
			"targetArea",
			"tip"
		})]
		private ValueTuple<Rectangle, RichString>? tooltip;

		// Token: 0x0400102E RID: 4142
		private SubmarineInfo.PendingSubInfo pendingSubInfo;

		// Token: 0x0400102F RID: 4143
		private RichString beaconStationActiveText;

		// Token: 0x04001030 RID: 4144
		private RichString beaconStationInactiveText;

		// Token: 0x04001031 RID: 4145
		private GUIComponent locationInfoOverlay;

		// Token: 0x04001032 RID: 4146
		private readonly List<Map.MapNotification> mapNotifications = new List<Map.MapNotification>();

		// Token: 0x04001033 RID: 4147
		private float hudVisibility;

		// Token: 0x04001034 RID: 4148
		private float cameraNoiseStrength;

		// Token: 0x04001035 RID: 4149
		public bool AllowDebugTeleport;

		// Token: 0x04001036 RID: 4150
		private readonly MapGenerationParams generationParams;

		// Token: 0x04001037 RID: 4151
		private Location furthestDiscoveredLocation;

		// Token: 0x0400103A RID: 4154
		public Action<Location, LocationConnection> OnLocationSelected;

		// Token: 0x0400103B RID: 4155
		public Action<LocationConnection, IEnumerable<Mission>> OnMissionsSelected;

		// Token: 0x0400103C RID: 4156
		public readonly NamedEvent<Map.LocationChangeInfo> OnLocationChanged = new NamedEvent<Map.LocationChangeInfo>();

		// Token: 0x0400103D RID: 4157
		private List<Location> endLocations = new List<Location>();

		// Token: 0x04001044 RID: 4164
		private readonly List<Location> locationsDiscovered = new List<Location>();

		// Token: 0x04001045 RID: 4165
		private readonly List<Location> locationsVisited = new List<Location>();

		// Token: 0x04001047 RID: 4167
		public Radiation Radiation;

		// Token: 0x04001048 RID: 4168
		private bool trackedLocationDiscoveryAndVisitOrder = true;

		// Token: 0x04001049 RID: 4169
		private IOrderedEnumerable<Biome> _orderedBiomes;

		// Token: 0x02000B56 RID: 2902
		private class MapAnim
		{
			// Token: 0x17001AB9 RID: 6841
			// (get) Token: 0x06007838 RID: 30776 RVA: 0x0037C8D5 File Offset: 0x0037AAD5
			// (set) Token: 0x06007839 RID: 30777 RVA: 0x0037C8DD File Offset: 0x0037AADD
			public float StartDelay
			{
				get
				{
					return this.startDelay;
				}
				set
				{
					this.startDelay = value;
					this.Timer = -this.startDelay;
				}
			}

			// Token: 0x04004754 RID: 18260
			public Location StartLocation;

			// Token: 0x04004755 RID: 18261
			public Location EndLocation;

			// Token: 0x04004756 RID: 18262
			public string StartMessage;

			// Token: 0x04004757 RID: 18263
			public string EndMessage;

			// Token: 0x04004758 RID: 18264
			public float? StartZoom;

			// Token: 0x04004759 RID: 18265
			public float? EndZoom;

			// Token: 0x0400475A RID: 18266
			private float startDelay;

			// Token: 0x0400475B RID: 18267
			public Vector2? StartPos;

			// Token: 0x0400475C RID: 18268
			public float Duration;

			// Token: 0x0400475D RID: 18269
			public float Timer;

			// Token: 0x0400475E RID: 18270
			public bool Finished;
		}

		// Token: 0x02000B57 RID: 2903
		private class MapNotification
		{
			// Token: 0x0600783B RID: 30779 RVA: 0x0037C8FC File Offset: 0x0037AAFC
			public MapNotification(string text, GUIFont font, List<Map.MapNotification> existingNotifications, Location relatedLocation)
			{
				this.Text = RichString.Rich(text, null);
				this.Font = font;
				this.TextSize = this.Font.MeasureString(this.Font.ForceUpperCase ? this.Text.SanitizedValue.ToUpper() : this.Text.SanitizedValue, false);
				if (existingNotifications.Any<Map.MapNotification>())
				{
					this.Offset = existingNotifications.Max((Map.MapNotification n) => n.Offset + n.TextSize.X + (float)GUI.IntScale(60f));
				}
				this.RelatedLocation = relatedLocation;
			}

			// Token: 0x0400475F RID: 18271
			public readonly RichString Text;

			// Token: 0x04004760 RID: 18272
			public readonly GUIFont Font;

			// Token: 0x04004761 RID: 18273
			public readonly Vector2 TextSize;

			// Token: 0x04004762 RID: 18274
			public int TimesShown;

			// Token: 0x04004763 RID: 18275
			public float Offset;

			// Token: 0x04004764 RID: 18276
			public readonly Location RelatedLocation;

			// Token: 0x04004765 RID: 18277
			public bool IsCurrentlyVisible;
		}

		// Token: 0x02000B58 RID: 2904
		public readonly struct LocationChangeInfo
		{
			// Token: 0x0600783C RID: 30780 RVA: 0x0037C9A4 File Offset: 0x0037ABA4
			public LocationChangeInfo(Location prevLocation, Location newLocation)
			{
				this.PrevLocation = prevLocation;
				this.NewLocation = newLocation;
			}

			// Token: 0x04004766 RID: 18278
			public readonly Location PrevLocation;

			// Token: 0x04004767 RID: 18279
			public readonly Location NewLocation;
		}

		// Token: 0x02000B59 RID: 2905
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004768 RID: 18280
			public static Func<LocationType, bool> <0>__AllowAsBiomeGate;
		}
	}
}
