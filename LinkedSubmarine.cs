using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000E4 RID: 228
	internal class LinkedSubmarine : MapEntity
	{
		// Token: 0x06001F4A RID: 8010 RVA: 0x00134F00 File Offset: 0x00133100
		public override void Draw(SpriteBatch spriteBatch, bool editing, bool back = true)
		{
			if (!editing || this.wallVertices == null)
			{
				return;
			}
			this.Draw(spriteBatch, this.Position, 1f);
			if (!Item.ShowLinks)
			{
				return;
			}
			foreach (MapEntity e in this.linkedTo)
			{
				Item item = e as Item;
				bool isLinkAllowed = item != null && item.HasTag(Tags.DockingPort);
				GUI.DrawLine(spriteBatch, new Vector2(this.WorldPosition.X, -this.WorldPosition.Y), new Vector2(e.WorldPosition.X, -e.WorldPosition.Y), isLinkAllowed ? (GUIStyle.Green * 0.5f) : (GUIStyle.Red * 0.5f), 0f, 3f);
			}
		}

		// Token: 0x06001F4B RID: 8011 RVA: 0x00134FFC File Offset: 0x001331FC
		public void Draw(SpriteBatch spriteBatch, Vector2 drawPos, float alpha = 1f)
		{
			Color color = base.IsHighlighted ? GUIStyle.Orange : GUIStyle.Green;
			if (base.IsSelected)
			{
				color = GUIStyle.Red;
			}
			Vector2 pos = drawPos;
			for (int i = 0; i < this.wallVertices.Count; i++)
			{
				Vector2 startPos = this.wallVertices[i] + pos;
				startPos.Y = -startPos.Y;
				Vector2 endPos = this.wallVertices[(i + 1) % this.wallVertices.Count] + pos;
				endPos.Y = -endPos.Y;
				GUI.DrawLine(spriteBatch, startPos, endPos, color * alpha, 0f, 5f);
			}
			pos.Y = -pos.Y;
			GUI.DrawLine(spriteBatch, pos + Vector2.UnitY * 50f, pos - Vector2.UnitY * 50f, color * alpha, 0f, 5f);
			GUI.DrawLine(spriteBatch, pos + Vector2.UnitX * 50f, pos - Vector2.UnitX * 50f, color * alpha, 0f, 5f);
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x0013514C File Offset: 0x0013334C
		public override void UpdateEditing(Camera cam, float deltaTime)
		{
			if (MapEntity.editingHUD == null || MapEntity.editingHUD.UserData as LinkedSubmarine != this)
			{
				MapEntity.editingHUD = this.CreateEditingHUD(false);
			}
			MapEntity.editingHUD.UpdateManually(deltaTime, false, true);
			if (!PlayerInput.PrimaryMouseButtonClicked() || !PlayerInput.KeyDown(Keys.Space))
			{
				return;
			}
			Vector2 position = cam.ScreenToWorld(PlayerInput.MousePosition);
			foreach (MapEntity entity in MapEntity.HighlightedEntities)
			{
				if (entity != this && entity is Item && entity.IsMouseOn(position) && ((Item)entity).GetComponent<DockingPort>() != null)
				{
					if (this.linkedTo.Contains(entity))
					{
						this.linkedTo.Remove(entity);
						entity.linkedTo.Remove(this);
					}
					else
					{
						this.linkedTo.Add(entity);
						if (!entity.linkedTo.Contains(this))
						{
							entity.linkedTo.Add(this);
						}
					}
				}
			}
		}

		// Token: 0x06001F4D RID: 8013 RVA: 0x00135258 File Offset: 0x00133458
		private GUIComponent CreateEditingHUD(bool inGame = false)
		{
			MapEntity.editingHUD = new GUIFrame(new RectTransform(new Vector2(0.3f, 0.25f), GUI.Canvas, Anchor.CenterRight, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(400, 0)
			}, "", null)
			{
				UserData = this
			};
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.8f), MapEntity.editingHUD.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = (int)(GUI.Scale * 5f)
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("LinkedSub");
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
			if (!inGame)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("LinkLinkedSub"), new Color?(GUIStyle.Orange), GUIStyle.SmallFont, Alignment.Left, false, "", null);
			}
			GUILayoutGroup pathContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			string filePath = this.filePath;
			if (filePath.StartsWith("Submarines"))
			{
				string subName = Path.GetFileNameWithoutExtension(filePath);
				SubmarineFile submarineFile = ContentPackageManager.LocalPackages.Concat(ContentPackageManager.VanillaCorePackage.ToEnumerable<CorePackage>()).SelectMany((ContentPackage p) => p.GetFiles<SubmarineFile>()).FirstOrDefault((SubmarineFile f) => Path.GetFileNameWithoutExtension(f.Path.Value).Equals(subName, StringComparison.OrdinalIgnoreCase));
				string foundPath = (submarineFile != null) ? submarineFile.Path.Value : null;
				if (foundPath.IsNullOrEmpty())
				{
					foundPath = Path.Combine(new string[]
					{
						"LocalMods",
						subName,
						subName + ".sub"
					});
				}
				filePath = foundPath;
			}
			RectTransform rectT2 = new RectTransform(new Vector2(0.75f, 1f), pathContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text2 = filePath;
			font = GUIStyle.SmallFont;
			GUITextBox pathBox = new GUITextBox(rectT2, text2, null, font, Alignment.Left, false, "", null, false, true);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.25f / pathBox.RectTransform.RelativeSize.X, 1f), pathBox.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), TextManager.Get("ReloadLinkedSub"), Alignment.Center, "GUIButtonSmall", null);
			guibutton.OnClicked = new GUIButton.OnClickedHandler(this.Reload);
			guibutton.UserData = pathBox;
			guibutton.ToolTip = TextManager.Get("ReloadLinkedSubTooltip");
			MapEntity.editingHUD.RectTransform.Resize(new Point(MapEntity.editingHUD.Rect.Width, (int)((float)paddedFrame.Children.Sum((GUIComponent c) => c.Rect.Height + paddedFrame.AbsoluteSpacing) / paddedFrame.RectTransform.RelativeSize.Y)), true);
			MapEntity.PositionEditingHUD();
			return MapEntity.editingHUD;
		}

		// Token: 0x06001F4E RID: 8014 RVA: 0x0013569C File Offset: 0x0013389C
		private bool Reload(GUIButton button, object obj)
		{
			GUITextBox pathBox = obj as GUITextBox;
			if (!File.Exists(pathBox.Text))
			{
				new GUIMessageBox(TextManager.Get("Error"), TextManager.GetWithVariable("ReloadLinkedSubError", "[file]", pathBox.Text, FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
				pathBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
				pathBox.Text = this.filePath;
				return false;
			}
			XDocument doc = SubmarineInfo.OpenFile(pathBox.Text);
			if (doc == null || doc.Root == null)
			{
				return false;
			}
			doc.Root.SetAttributeValue("filepath", pathBox.Text);
			pathBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
			this.GenerateWallVertices(doc.Root);
			this.saveElement = doc.Root;
			this.saveElement.Name = "LinkedSubmarine";
			this.CargoCapacity = doc.Root.GetAttributeInt("cargocapacity", 0);
			this.filePath = pathBox.Text;
			return true;
		}

		// Token: 0x1700086C RID: 2156
		// (get) Token: 0x06001F4F RID: 8015 RVA: 0x001357D8 File Offset: 0x001339D8
		public bool LoadSub
		{
			get
			{
				return this.loadSub;
			}
		}

		// Token: 0x1700086D RID: 2157
		// (get) Token: 0x06001F50 RID: 8016 RVA: 0x001357E0 File Offset: 0x001339E0
		public ushort OriginalLinkedToID
		{
			get
			{
				return this.originalLinkedToID;
			}
		}

		// Token: 0x1700086E RID: 2158
		// (get) Token: 0x06001F51 RID: 8017 RVA: 0x001357E8 File Offset: 0x001339E8
		public Submarine Sub
		{
			get
			{
				return this.sub;
			}
		}

		// Token: 0x1700086F RID: 2159
		// (get) Token: 0x06001F52 RID: 8018 RVA: 0x001357F0 File Offset: 0x001339F0
		public override bool Linkable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000870 RID: 2160
		// (get) Token: 0x06001F53 RID: 8019 RVA: 0x001357F3 File Offset: 0x001339F3
		// (set) Token: 0x06001F54 RID: 8020 RVA: 0x001357FB File Offset: 0x001339FB
		public int CargoCapacity { get; private set; }

		// Token: 0x06001F55 RID: 8021 RVA: 0x00135804 File Offset: 0x00133A04
		public LinkedSubmarine(Submarine submarine, ushort id = 0) : base(null, submarine, id)
		{
			this.linkedToID = new List<ushort>();
			base.InsertToList();
			DebugConsole.Log("Created linked submarine (" + this.ID.ToString() + ")");
		}

		// Token: 0x06001F56 RID: 8022 RVA: 0x00135840 File Offset: 0x00133A40
		public static LinkedSubmarine CreateDummy(Submarine mainSub, Submarine linkedSub)
		{
			return new LinkedSubmarine(mainSub, 0)
			{
				sub = linkedSub
			};
		}

		// Token: 0x06001F57 RID: 8023 RVA: 0x00135860 File Offset: 0x00133A60
		public static LinkedSubmarine CreateDummy(Submarine mainSub, string filePath, Vector2 position)
		{
			XDocument doc = SubmarineInfo.OpenFile(filePath);
			if (doc == null || doc.Root == null)
			{
				return null;
			}
			LinkedSubmarine sl = LinkedSubmarine.CreateDummy(mainSub, doc.Root, position, 0);
			sl.filePath = filePath;
			sl.saveElement = doc.Root;
			sl.saveElement.Name = "LinkedSubmarine";
			sl.saveElement.SetAttributeValue("filepath", filePath);
			return sl;
		}

		// Token: 0x06001F58 RID: 8024 RVA: 0x001358D0 File Offset: 0x00133AD0
		public static LinkedSubmarine CreateDummy(Submarine mainSub, XElement element, Vector2 position, ushort id = 0)
		{
			LinkedSubmarine sl = new LinkedSubmarine(mainSub, id);
			sl.GenerateWallVertices(element);
			sl.CargoCapacity = element.GetAttributeInt("cargocapacity", 0);
			if (sl.wallVertices.Any<Vector2>())
			{
				sl.Rect = new Rectangle((int)sl.wallVertices.Min((Vector2 v) => v.X + position.X), (int)sl.wallVertices.Max((Vector2 v) => v.Y + position.Y), (int)sl.wallVertices.Max((Vector2 v) => v.X + position.X), (int)sl.wallVertices.Min((Vector2 v) => v.Y + position.Y));
				int width = sl.rect.Width - sl.rect.X;
				int height = sl.rect.Y - sl.rect.Height;
				sl.Rect = new Rectangle((int)(position.X - (float)(width / 2)), (int)(position.Y + (float)(height / 2)), width, height);
			}
			else
			{
				sl.Rect = new Rectangle((int)position.X, (int)position.Y, 10, 10);
			}
			return sl;
		}

		// Token: 0x06001F59 RID: 8025 RVA: 0x00135A0F File Offset: 0x00133C0F
		public override bool IsMouseOn(Vector2 position)
		{
			return Vector2.Distance(position, this.WorldPosition) < 50f;
		}

		// Token: 0x06001F5A RID: 8026 RVA: 0x00135A24 File Offset: 0x00133C24
		public override MapEntity Clone()
		{
			XElement cloneElement = new XElement(this.saveElement);
			LinkedSubmarine sl = LinkedSubmarine.CreateDummy(base.Submarine, cloneElement, this.Position, 0);
			sl.saveElement = cloneElement;
			sl.filePath = this.filePath;
			return sl;
		}

		// Token: 0x06001F5B RID: 8027 RVA: 0x00135A68 File Offset: 0x00133C68
		private void GenerateWallVertices(XElement rootElement)
		{
			List<Vector2> points = new List<Vector2>();
			foreach (XElement element in rootElement.Elements())
			{
				if (!(element.Name != "Structure"))
				{
					string name = element.GetAttributeString("name", "");
					Identifier identifier = element.GetAttributeIdentifier("identifier", "");
					StructurePrefab prefab = Structure.FindPrefab(name, identifier);
					if (prefab != null)
					{
						float scale = element.GetAttributeFloat("scale", prefab.Scale);
						Vector4 rect = element.GetAttributeVector4("rect", Vector4.Zero);
						if (!prefab.ResizeHorizontal)
						{
							rect.Z *= scale / prefab.Scale;
						}
						if (!prefab.ResizeVertical)
						{
							rect.W *= scale / prefab.Scale;
						}
						points.Add(new Vector2(rect.X, rect.Y));
						points.Add(new Vector2(rect.X + rect.Z, rect.Y));
						points.Add(new Vector2(rect.X, rect.Y - rect.W));
						points.Add(new Vector2(rect.X + rect.Z, rect.Y - rect.W));
					}
				}
			}
			this.wallVertices = MathUtils.GiftWrap(points);
		}

		// Token: 0x06001F5C RID: 8028 RVA: 0x00135C08 File Offset: 0x00133E08
		public static LinkedSubmarine Load(ContentXElement element, Submarine submarine, IdRemap idRemap)
		{
			string key = "pos";
			Vector2 zero = Vector2.Zero;
			Vector2 pos = element.GetAttributeVector2(key, zero);
			ushort id;
			idRemap.AssignMaxId(out id);
			LinkedSubmarine linkedSub;
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				linkedSub = LinkedSubmarine.CreateDummy(submarine, element, pos, id);
				linkedSub.saveElement = new XElement(element);
				linkedSub.purchasedLostShuttles = false;
			}
			else
			{
				string levelSeed = element.GetAttributeString("location", "");
				GameSession gameSession = GameMain.GameSession;
				LevelData levelData2;
				if (gameSession == null)
				{
					levelData2 = null;
				}
				else
				{
					CampaignMode campaign2 = gameSession.Campaign;
					levelData2 = ((campaign2 != null) ? campaign2.NextLevel : null);
				}
				LevelData levelData3;
				if ((levelData3 = levelData2) == null)
				{
					GameSession gameSession2 = GameMain.GameSession;
					levelData3 = ((gameSession2 != null) ? gameSession2.LevelData : null);
				}
				LevelData levelData = levelData3;
				LinkedSubmarine linkedSubmarine = new LinkedSubmarine(submarine, id);
				LinkedSubmarine linkedSubmarine2 = linkedSubmarine;
				GameSession gameSession3 = GameMain.GameSession;
				CampaignMode campaign = ((gameSession3 != null) ? gameSession3.GameMode : null) as CampaignMode;
				linkedSubmarine2.purchasedLostShuttles = ((campaign != null && campaign.PurchasedLostShuttles) || element.GetAttributeBool("purchasedlostshuttle", false));
				linkedSubmarine.saveElement = new XElement(element);
				linkedSub = linkedSubmarine;
				if (string.IsNullOrWhiteSpace(levelSeed) || levelData == null || levelData.Seed == levelSeed || linkedSub.purchasedLostShuttles)
				{
					GameSession gameSession4 = GameMain.GameSession;
					bool flag;
					if (gameSession4 == null)
					{
						flag = (null != null);
					}
					else
					{
						CampaignMode campaign3 = gameSession4.Campaign;
						flag = (((campaign3 != null) ? campaign3.PendingSubmarineSwitch : null) != null);
					}
					if (!flag)
					{
						linkedSub.loadSub = true;
						linkedSub.rect.Location = MathUtils.ToPoint(pos);
						goto IL_160;
					}
				}
				linkedSub.loadSub = false;
			}
			IL_160:
			LinkedSubmarine linkedSubmarine3 = linkedSub;
			ContentPath attributeContentPath = element.GetAttributeContentPath("filepath");
			linkedSubmarine3.filePath = (((attributeContentPath != null) ? attributeContentPath.Value : null) ?? string.Empty);
			int[] linkedToIds = element.GetAttributeIntArray("linkedto", Array.Empty<int>());
			for (int i = 0; i < linkedToIds.Length; i++)
			{
				linkedSub.linkedToID.Add(idRemap.GetOffsetId(linkedToIds[i]));
			}
			linkedSub.originalLinkedToID = idRemap.GetOffsetId(element.GetAttributeInt("originallinkedto", 0));
			linkedSub.originalMyPortID = (ushort)element.GetAttributeInt("originalmyport", 0);
			linkedSub.CargoCapacity = element.GetAttributeInt("cargocapacity", 0);
			if (!linkedSub.loadSub)
			{
				return null;
			}
			return linkedSub;
		}

		// Token: 0x06001F5D RID: 8029 RVA: 0x00135E1C File Offset: 0x0013401C
		public void LinkDummyToMainSubmarine()
		{
			if (Screen.Selected != GameMain.SubEditorScreen)
			{
				return;
			}
			for (int i = 0; i < this.linkedToID.Count; i++)
			{
				MapEntity linked = Entity.FindEntityByID(this.linkedToID[i]) as MapEntity;
				if (linked != null)
				{
					this.linkedTo.Add(linked);
				}
			}
		}

		// Token: 0x06001F5E RID: 8030 RVA: 0x00135E74 File Offset: 0x00134074
		public void SetPositionRelativeToMainSub()
		{
			if (this.positionRelativeToMainSub != null)
			{
				this.Sub.SetPosition(base.Submarine.WorldPosition + this.positionRelativeToMainSub.Value, null, true);
			}
			this.positionRelativeToMainSub = null;
		}

		// Token: 0x06001F5F RID: 8031 RVA: 0x00135EC4 File Offset: 0x001340C4
		public override void OnMapLoaded()
		{
			if (!this.loadSub)
			{
				return;
			}
			SubmarineInfo info = new SubmarineInfo(base.Submarine.Info.FilePath, "", this.saveElement, true, false);
			if (!info.SubmarineElement.HasElements)
			{
				DebugConsole.ThrowError("Failed to load a linked submarine (empty XML element). The save file may be corrupted.", null, null, false, false);
				return;
			}
			if (!info.SubmarineElement.Elements().Any((XElement e) => e.Name.ToString().Equals("hull", StringComparison.OrdinalIgnoreCase)))
			{
				DebugConsole.ThrowError("Failed to load a linked submarine (the submarine contains no hulls).", null, null, false, false);
				return;
			}
			XAttribute xattribute = this.saveElement.Attribute("purchasedlostshuttle");
			if (xattribute != null)
			{
				xattribute.Remove();
			}
			IdRemap parentRemap = new IdRemap(base.Submarine.Info.SubmarineElement, (int)base.Submarine.IdOffset);
			this.sub = Submarine.Load(info, false, parentRemap);
			this.sub.Info.SubmarineClass = base.Submarine.Info.SubmarineClass;
			if (base.Submarine.Info.IsOutpost && base.Submarine.TeamID == CharacterTeamType.FriendlyNPC)
			{
				this.sub.TeamID = CharacterTeamType.FriendlyNPC;
			}
			IdRemap childRemap = new IdRemap(this.saveElement, (int)this.sub.IdOffset);
			Vector2 worldPos = this.saveElement.GetAttributeVector2("worldpos", Vector2.Zero);
			if (worldPos != Vector2.Zero)
			{
				if (GameMain.GameSession != null && GameMain.GameSession.MirrorLevel)
				{
					worldPos.X = (float)GameMain.GameSession.LevelData.Size.X - worldPos.X;
				}
				this.sub.SetPosition(worldPos, null, true);
			}
			else
			{
				this.sub.SetPosition(this.WorldPosition, null, true);
			}
			DockingPort linkedPort = null;
			DockingPort myPort = null;
			MapEntity linkedItem = this.linkedTo.FirstOrDefault(delegate(MapEntity lt)
			{
				Item item3 = lt as Item;
				return ((item3 != null) ? item3.GetComponent<DockingPort>() : null) != null;
			});
			if (linkedItem == null)
			{
				linkedPort = DockingPort.List.FirstOrDefault((DockingPort dp) => dp.DockingTarget != null && dp.DockingTarget.Item.Submarine == this.sub);
			}
			else
			{
				linkedPort = ((Item)linkedItem).GetComponent<DockingPort>();
			}
			if (linkedPort == null && this.purchasedLostShuttles)
			{
				Item item = Entity.FindEntityByID(this.originalLinkedToID) as Item;
				linkedPort = ((item != null) ? item.GetComponent<DockingPort>() : null);
			}
			if (linkedPort != null)
			{
				this.originalLinkedPort = linkedPort;
				ushort originalMyId = childRemap.GetOffsetId((int)this.originalMyPortID);
				Item item2 = Entity.FindEntityByID(originalMyId) as Item;
				myPort = ((item2 != null) ? item2.GetComponent<DockingPort>() : null);
				if (myPort == null)
				{
					float closestDistance = 0f;
					foreach (DockingPort port in DockingPort.List)
					{
						if (port.Item.Submarine == this.sub && port.IsHorizontal == linkedPort.IsHorizontal && (port.ForceDockingDirection == DockingPort.DirectionType.None || port.ForceDockingDirection != linkedPort.ForceDockingDirection))
						{
							float dist = Vector2.Distance(port.Item.WorldPosition, linkedPort.Item.WorldPosition);
							if (myPort == null || dist < closestDistance)
							{
								myPort = port;
								closestDistance = dist;
							}
						}
					}
				}
				if (myPort != null)
				{
					this.originalMyPortID = myPort.Item.ID;
					myPort.Undock(false);
					myPort.DockingDir = 0;
					if (linkedPort.Docked && linkedPort.DockingTarget != null && linkedPort.DockingTarget != myPort)
					{
						this.sub.SetPosition(linkedPort.Item.Submarine.WorldPosition - new Vector2(0f, (float)(linkedPort.Item.Submarine.GetDockedBorders(true).Height / 2 + this.sub.GetDockedBorders(true).Height / 2)), null, true);
					}
					else
					{
						Vector2 portDiff = myPort.Item.WorldPosition - this.sub.WorldPosition;
						Vector2 offset = myPort.IsHorizontal ? (Vector2.UnitX * (float)myPort.GetDir(linkedPort)) : (Vector2.UnitY * (float)myPort.GetDir(linkedPort));
						offset *= myPort.DockedDistance;
						this.sub.SetPosition(linkedPort.Item.WorldPosition - portDiff - offset, null, true);
						myPort.Dock(linkedPort);
						myPort.Lock(true, false, true);
					}
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null && (campaign.PurchasedLostShuttles || campaign.PurchasedLostShuttlesInLatestSave))
				{
					foreach (Structure wall in Structure.WallList)
					{
						if (wall.Submarine == this.sub)
						{
							for (int i = 0; i < wall.SectionCount; i++)
							{
								wall.SetDamage(i, 0f, null, false, true, false, false);
							}
						}
					}
					foreach (Hull hull in Hull.HullList)
					{
						if (hull.Submarine == this.sub)
						{
							hull.WaterVolume = 0f;
							hull.OxygenPercentage = 100f;
							BallastFloraBehavior ballastFlora = hull.BallastFlora;
							if (ballastFlora != null)
							{
								ballastFlora.Kill();
							}
						}
					}
				}
				this.sub.SetPosition(this.sub.WorldPosition - base.Submarine.WorldPosition, null, false);
				this.sub.Submarine = base.Submarine;
				return;
			}
			if (!(worldPos == Vector2.Zero))
			{
				this.sub.Submarine = base.Submarine;
				return;
			}
			Vector2 relativePos = this.saveElement.GetAttributeVector2("posrelativetomainsub", Vector2.Zero);
			if (relativePos != Vector2.Zero)
			{
				this.positionRelativeToMainSub = new Vector2?(relativePos);
				return;
			}
			DebugConsole.ThrowError("Something went wrong when loading a linked submarine - the save didn't include a world position, a linked port or position relative to the main sub.", null, null, false, false);
		}

		// Token: 0x06001F60 RID: 8032 RVA: 0x001364F8 File Offset: 0x001346F8
		public override XElement Save(XElement parentElement)
		{
			XElement saveElement;
			if (this.sub == null)
			{
				if (this.saveElement == null)
				{
					XDocument doc = SubmarineInfo.OpenFile(this.filePath);
					saveElement = doc.Root;
					saveElement.Add(new XAttribute("filepath", this.filePath));
				}
				else
				{
					saveElement = this.saveElement;
				}
				saveElement.Name = "LinkedSubmarine";
				if (saveElement.Attribute("previewimage") != null)
				{
					saveElement.Attribute("previewimage").Remove();
				}
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
				if (campaign != null && campaign.PurchasedLostShuttles)
				{
					saveElement.SetAttributeValue("purchasedlostshuttle", true);
				}
				saveElement.SetAttributeValue("pos", XMLExtensions.Vector2ToString(this.Position - base.Submarine.HiddenSubPosition));
			}
			else
			{
				saveElement = new XElement("LinkedSubmarine");
				this.sub.SaveToXElement(saveElement);
			}
			if (this.linkedTo.Any<MapEntity>() || this.linkedToID.Any<ushort>())
			{
				MapEntity linkedPort = this.linkedTo.FirstOrDefault(delegate(MapEntity lt)
				{
					Item item = lt as Item;
					return item != null && item.GetComponent<DockingPort>() != null;
				}) ?? (Entity.FindEntityByID(this.linkedToID.First<ushort>()) as MapEntity);
				if (linkedPort != null)
				{
					saveElement.SetAttributeValue("linkedto", linkedPort.ID);
				}
			}
			saveElement.SetAttributeValue("originallinkedto", (this.originalLinkedPort != null) ? this.originalLinkedPort.Item.ID : this.originalLinkedToID);
			saveElement.SetAttributeValue("originalmyport", this.originalMyPortID);
			if (this.sub != null)
			{
				bool leaveBehind = false;
				if (this.sub.Submarine != null && !this.sub.DockedTo.Contains(this.sub.Submarine))
				{
					if (Submarine.MainSub.AtEndExit)
					{
						leaveBehind = (this.sub.AtEndExit != Submarine.MainSub.AtEndExit);
					}
					else
					{
						leaveBehind = (this.sub.AtStartExit != Submarine.MainSub.AtStartExit);
					}
				}
				if (leaveBehind)
				{
					saveElement.SetAttributeValue("location", Level.Loaded.Seed);
					Vector2 position = this.sub.SubBody.Position;
					if (Level.Loaded.Mirrored)
					{
						position.X = (float)Level.Loaded.Size.X - position.X;
					}
					saveElement.SetAttributeValue("worldpos", XMLExtensions.Vector2ToString(position));
				}
				else
				{
					if (saveElement.Attribute("location") != null)
					{
						saveElement.Attribute("location").Remove();
					}
					if (saveElement.Attribute("worldpos") != null)
					{
						saveElement.Attribute("worldpos").Remove();
					}
					saveElement.SetAttributeValue("posrelativetomainsub", XMLExtensions.Vector2ToString(this.sub.WorldPosition - base.Submarine.WorldPosition));
				}
				saveElement.SetAttributeValue("pos", XMLExtensions.Vector2ToString(this.Position - base.Submarine.HiddenSubPosition));
			}
			parentElement.Add(saveElement);
			return saveElement;
		}

		// Token: 0x04001016 RID: 4118
		private List<Vector2> wallVertices;

		// Token: 0x04001017 RID: 4119
		private string filePath;

		// Token: 0x04001018 RID: 4120
		private bool loadSub;

		// Token: 0x04001019 RID: 4121
		private Submarine sub;

		// Token: 0x0400101A RID: 4122
		private ushort originalMyPortID;

		// Token: 0x0400101B RID: 4123
		private ushort originalLinkedToID;

		// Token: 0x0400101C RID: 4124
		private DockingPort originalLinkedPort;

		// Token: 0x0400101D RID: 4125
		private bool purchasedLostShuttles;

		// Token: 0x0400101E RID: 4126
		private XElement saveElement;

		// Token: 0x0400101F RID: 4127
		private Vector2? positionRelativeToMainSub;
	}
}
