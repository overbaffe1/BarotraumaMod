using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Items.Components;
using Barotrauma.MapCreatures.Behavior;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000242 RID: 578
	internal class LinkedSubmarine : MapEntity
	{
		// Token: 0x17000BE9 RID: 3049
		// (get) Token: 0x0600288E RID: 10382 RVA: 0x001067A6 File Offset: 0x001049A6
		public bool LoadSub
		{
			get
			{
				return this.loadSub;
			}
		}

		// Token: 0x17000BEA RID: 3050
		// (get) Token: 0x0600288F RID: 10383 RVA: 0x001067AE File Offset: 0x001049AE
		public ushort OriginalLinkedToID
		{
			get
			{
				return this.originalLinkedToID;
			}
		}

		// Token: 0x17000BEB RID: 3051
		// (get) Token: 0x06002890 RID: 10384 RVA: 0x001067B6 File Offset: 0x001049B6
		public Submarine Sub
		{
			get
			{
				return this.sub;
			}
		}

		// Token: 0x17000BEC RID: 3052
		// (get) Token: 0x06002891 RID: 10385 RVA: 0x001067BE File Offset: 0x001049BE
		public override bool Linkable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000BED RID: 3053
		// (get) Token: 0x06002892 RID: 10386 RVA: 0x001067C1 File Offset: 0x001049C1
		// (set) Token: 0x06002893 RID: 10387 RVA: 0x001067C9 File Offset: 0x001049C9
		public int CargoCapacity { get; private set; }

		// Token: 0x06002894 RID: 10388 RVA: 0x001067D2 File Offset: 0x001049D2
		public LinkedSubmarine(Submarine submarine, ushort id = 0) : base(null, submarine, id)
		{
			this.linkedToID = new List<ushort>();
			base.InsertToList();
			DebugConsole.Log("Created linked submarine (" + this.ID.ToString() + ")");
		}

		// Token: 0x06002895 RID: 10389 RVA: 0x00106810 File Offset: 0x00104A10
		public static LinkedSubmarine CreateDummy(Submarine mainSub, Submarine linkedSub)
		{
			return new LinkedSubmarine(mainSub, 0)
			{
				sub = linkedSub
			};
		}

		// Token: 0x06002896 RID: 10390 RVA: 0x00106830 File Offset: 0x00104A30
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

		// Token: 0x06002897 RID: 10391 RVA: 0x001068A0 File Offset: 0x00104AA0
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

		// Token: 0x06002898 RID: 10392 RVA: 0x001069DF File Offset: 0x00104BDF
		public override bool IsMouseOn(Vector2 position)
		{
			return Vector2.Distance(position, this.WorldPosition) < 50f;
		}

		// Token: 0x06002899 RID: 10393 RVA: 0x001069F4 File Offset: 0x00104BF4
		public override MapEntity Clone()
		{
			XElement cloneElement = new XElement(this.saveElement);
			LinkedSubmarine sl = LinkedSubmarine.CreateDummy(base.Submarine, cloneElement, this.Position, 0);
			sl.saveElement = cloneElement;
			sl.filePath = this.filePath;
			return sl;
		}

		// Token: 0x0600289A RID: 10394 RVA: 0x00106A38 File Offset: 0x00104C38
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

		// Token: 0x0600289B RID: 10395 RVA: 0x00106BD8 File Offset: 0x00104DD8
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

		// Token: 0x0600289C RID: 10396 RVA: 0x00106DEC File Offset: 0x00104FEC
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

		// Token: 0x0600289D RID: 10397 RVA: 0x00106E44 File Offset: 0x00105044
		public void SetPositionRelativeToMainSub()
		{
			if (this.positionRelativeToMainSub != null)
			{
				this.Sub.SetPosition(base.Submarine.WorldPosition + this.positionRelativeToMainSub.Value, null, true);
			}
			this.positionRelativeToMainSub = null;
		}

		// Token: 0x0600289E RID: 10398 RVA: 0x00106E94 File Offset: 0x00105094
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

		// Token: 0x0600289F RID: 10399 RVA: 0x001074C8 File Offset: 0x001056C8
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

		// Token: 0x040013F0 RID: 5104
		private List<Vector2> wallVertices;

		// Token: 0x040013F1 RID: 5105
		private string filePath;

		// Token: 0x040013F2 RID: 5106
		private bool loadSub;

		// Token: 0x040013F3 RID: 5107
		private Submarine sub;

		// Token: 0x040013F4 RID: 5108
		private ushort originalMyPortID;

		// Token: 0x040013F5 RID: 5109
		private ushort originalLinkedToID;

		// Token: 0x040013F6 RID: 5110
		private DockingPort originalLinkedPort;

		// Token: 0x040013F7 RID: 5111
		private bool purchasedLostShuttles;

		// Token: 0x040013F8 RID: 5112
		private XElement saveElement;

		// Token: 0x040013F9 RID: 5113
		private Vector2? positionRelativeToMainSub;
	}
}
