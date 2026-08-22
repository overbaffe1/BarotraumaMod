using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200002C RID: 44
	internal class ScanMission : Mission
	{
		// Token: 0x06000536 RID: 1334 RVA: 0x000309F8 File Offset: 0x0002EBF8
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			msg.WriteUInt16((ushort)this.startingItems.Count);
			foreach (Item item in this.startingItems)
			{
				item.WriteSpawnData(msg, item.ID, this.parentInventoryIDs.GetValueOrDefault(item, 0), this.parentItemContainerIndices.GetValueOrDefault(item, 0), this.inventorySlotIndices.GetValueOrDefault(item, -1));
			}
			this.ServerWriteScanTargetStatus(msg);
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x00030A9C File Offset: 0x0002EC9C
		public override void ServerWrite(IWriteMessage msg)
		{
			base.ServerWrite(msg);
			this.ServerWriteScanTargetStatus(msg);
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x00030AAC File Offset: 0x0002ECAC
		private void ServerWriteScanTargetStatus(IWriteMessage msg)
		{
			msg.WriteByte((byte)this.scanTargets.Count);
			foreach (KeyValuePair<WayPoint, bool> kvp in this.scanTargets)
			{
				WayPoint key = kvp.Key;
				msg.WriteUInt16((key != null) ? key.ID : 0);
				msg.WriteBoolean(kvp.Value);
			}
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000539 RID: 1337 RVA: 0x00030B30 File Offset: 0x0002ED30
		// (set) Token: 0x0600053A RID: 1338 RVA: 0x00030B38 File Offset: 0x0002ED38
		private Ruin TargetRuin { get; set; }

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x0600053B RID: 1339 RVA: 0x00030B44 File Offset: 0x0002ED44
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public override IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				if (this.AllTargetsScanned())
				{
					return Enumerable.Empty<ValueTuple<LocalizedString, Vector2>>();
				}
				return from kvp in this.scanTargets
				where !kvp.Value
				select new ValueTuple<LocalizedString, Vector2>(this.Prefab.SonarLabel, kvp.Key.WorldPosition);
			}
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x00030B9C File Offset: 0x0002ED9C
		public ScanMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.itemConfig = prefab.ConfigElement.GetChildElement("Items");
			this.totalTargetsToScan = prefab.ConfigElement.GetAttributeInt("targets", 1);
			this.minTargetDistance = prefab.ConfigElement.GetAttributeFloat("mintargetdistance", 0f);
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x00030C48 File Offset: 0x0002EE48
		protected override void StartMissionSpecific(Level level)
		{
			this.Reset();
			if (Mission.IsClient)
			{
				return;
			}
			ContentXElement contentXElement = null;
			if (this.itemConfig == contentXElement)
			{
				DebugConsole.ThrowError("Failed to initialize a Scan mission: item config is not set", null, this.Prefab.ContentPackage, false, false);
				return;
			}
			foreach (ContentXElement element in this.itemConfig.Elements())
			{
				this.LoadItem(element, null);
			}
			this.GetScanners();
			Level loaded = Level.Loaded;
			Ruin targetRuin;
			if (loaded == null)
			{
				targetRuin = null;
			}
			else
			{
				List<Ruin> ruins = loaded.Ruins;
				targetRuin = ((ruins != null) ? ruins.GetRandom(Rand.RandSync.ServerAndClient) : null);
			}
			this.TargetRuin = targetRuin;
			if (this.TargetRuin == null)
			{
				DebugConsole.ThrowError("Failed to initialize a Scan mission: level contains no alien ruins", null, this.Prefab.ContentPackage, false, false);
				return;
			}
			List<WayPoint> ruinWaypoints = this.TargetRuin.Submarine.GetWaypoints(false);
			ruinWaypoints.RemoveAll((WayPoint wp) => wp.CurrentHull == null);
			if (ruinWaypoints.Count < this.totalTargetsToScan)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(103, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to initialize a Scan mission: target ruin has less waypoints than required as scan targets (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(ruinWaypoints.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" < ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.totalTargetsToScan);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				return;
			}
			float guaranteedDistance = (float)(Math.Min(this.TargetRuin.Area.Width, this.TargetRuin.Area.Height) / (this.totalTargetsToScan + 1));
			List<WayPoint> availableWaypoints = new List<WayPoint>();
			for (int tries = 0; tries < 15; tries++)
			{
				float triesNormalized = (float)tries / 14f;
				float desperationFactor = MathF.Pow(triesNormalized, 2f);
				float currentMinDistance = MathHelper.Lerp(this.minTargetDistance, guaranteedDistance, desperationFactor);
				float currentMinDistanceSquared = currentMinDistance * currentMinDistance;
				this.scanTargets.Clear();
				availableWaypoints.Clear();
				availableWaypoints.AddRange(ruinWaypoints);
				for (int i = 0; i < this.totalTargetsToScan; i++)
				{
					WayPoint selectedWaypoint = availableWaypoints.GetRandom(Rand.RandSync.ServerAndClient);
					this.scanTargets.Add(selectedWaypoint, false);
					availableWaypoints.Remove(selectedWaypoint);
					if (i < this.totalTargetsToScan - 1)
					{
						availableWaypoints.RemoveAll((WayPoint wp) => wp.CurrentHull == selectedWaypoint.CurrentHull);
						availableWaypoints.RemoveAll((WayPoint wp) => Vector2.DistanceSquared(wp.WorldPosition, selectedWaypoint.WorldPosition) < currentMinDistanceSquared);
						if (availableWaypoints.None(null))
						{
							break;
						}
					}
				}
				if (this.scanTargets.Count >= this.totalTargetsToScan)
				{
					break;
				}
			}
			if (this.scanTargets.Count < this.totalTargetsToScan)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(93, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Error initializing a Scan mission: not enough targets (current targets: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.scanTargets.Count);
				defaultInterpolatedStringHandler2.AppendLiteral(", required targets: ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.totalTargetsToScan);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
			}
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x00030F98 File Offset: 0x0002F198
		private void Reset()
		{
			this.startingItems.Clear();
			this.parentInventoryIDs.Clear();
			this.inventorySlotIndices.Clear();
			this.parentItemContainerIndices.Clear();
			this.scanners.Clear();
			this.TargetRuin = null;
			this.scanTargets.Clear();
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x00030FF0 File Offset: 0x0002F1F0
		private void LoadItem(XElement element, Item parent)
		{
			ItemPrefab itemPrefab = base.FindItemPrefab(element);
			Submarine cargoRoomSub;
			Vector2? position = base.GetCargoSpawnPosition(itemPrefab, out cargoRoomSub);
			if (position == null)
			{
				return;
			}
			Item item = new Item(itemPrefab, position.Value, cargoRoomSub, 0, true);
			item.FindHull();
			this.startingItems.Add(item);
			ItemContainer itemContainer = (parent != null) ? parent.GetComponent<ItemContainer>() : null;
			if (itemContainer != null)
			{
				this.parentInventoryIDs.Add(item, parent.ID);
				this.parentItemContainerIndices.Add(item, (byte)parent.GetComponentIndex(itemContainer));
				parent.Combine(item, null);
				Dictionary<Item, int> dictionary = this.inventorySlotIndices;
				Item key = item;
				Inventory parentInventory = item.ParentInventory;
				dictionary.Add(key, (parentInventory != null) ? parentInventory.FindIndex(item) : -1);
			}
			foreach (XElement subElement in element.Elements())
			{
				int amount = subElement.GetAttributeInt("amount", 1);
				for (int i = 0; i < amount; i++)
				{
					this.LoadItem(subElement, item);
				}
			}
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x00031108 File Offset: 0x0002F308
		protected override void MissionStateChanged(int previousState)
		{
			int state = this.State;
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x00031114 File Offset: 0x0002F314
		private void GetScanners()
		{
			foreach (Item startingItem in this.startingItems)
			{
				Scanner scanner = startingItem.GetComponent<Scanner>();
				if (scanner != null)
				{
					Scanner scanner2 = scanner;
					scanner2.OnScanStarted = (Action<Scanner>)Delegate.Combine(scanner2.OnScanStarted, new Action<Scanner>(this.OnScanStarted));
					if (!Mission.IsClient)
					{
						Scanner scanner3 = scanner;
						scanner3.OnScanCompleted = (Action<Scanner>)Delegate.Combine(scanner3.OnScanCompleted, new Action<Scanner>(this.OnScanCompleted));
					}
					this.scanners.Add(scanner);
				}
			}
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x000311C4 File Offset: 0x0002F3C4
		private void OnScanStarted(Scanner scanner)
		{
			float scanRadiusSquared = scanner.ScanRadius * scanner.ScanRadius;
			foreach (KeyValuePair<WayPoint, bool> kvp in this.scanTargets)
			{
				if (ScanMission.IsValidScanPosition(scanner, kvp, scanRadiusSquared))
				{
					scanner.DisplayProgressBar = true;
					break;
				}
			}
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x00031234 File Offset: 0x0002F434
		private void OnScanCompleted(Scanner scanner)
		{
			if (Mission.IsClient)
			{
				return;
			}
			this.newTargetsScanned.Clear();
			float scanRadiusSquared = scanner.ScanRadius * scanner.ScanRadius;
			foreach (KeyValuePair<WayPoint, bool> kvp in this.scanTargets)
			{
				if (ScanMission.IsValidScanPosition(scanner, kvp, scanRadiusSquared))
				{
					this.newTargetsScanned.Add(kvp.Key);
				}
			}
			foreach (WayPoint wp in this.newTargetsScanned)
			{
				this.scanTargets[wp] = true;
			}
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.UpdateMissionState(this);
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00031318 File Offset: 0x0002F518
		private static bool IsValidScanPosition(Scanner scanner, KeyValuePair<WayPoint, bool> scanStatus, float scanRadiusSquared)
		{
			return !scanStatus.Value && scanStatus.Key.Submarine == scanner.Item.Submarine && Vector2.DistanceSquared(scanStatus.Key.WorldPosition, scanner.Item.WorldPosition) <= scanRadiusSquared;
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x00031370 File Offset: 0x0002F570
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (Mission.IsClient)
			{
				return;
			}
			this.State = Math.Max(this.State, this.scanTargets.Count((KeyValuePair<WayPoint, bool> kvp) => kvp.Value));
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x000313C0 File Offset: 0x0002F5C0
		private bool AllTargetsScanned()
		{
			return this.State >= this.totalTargetsToScan;
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x000313D3 File Offset: 0x0002F5D3
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.AllTargetsScanned();
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x000313DC File Offset: 0x0002F5DC
		protected override void EndMissionSpecific(bool completed)
		{
			foreach (Scanner scanner in this.scanners)
			{
				Item item = scanner.Item;
				if (item != null && !item.Removed)
				{
					Scanner scanner2 = scanner;
					scanner2.OnScanStarted = (Action<Scanner>)Delegate.Remove(scanner2.OnScanStarted, new Action<Scanner>(this.OnScanStarted));
					Scanner scanner3 = scanner;
					scanner3.OnScanCompleted = (Action<Scanner>)Delegate.Remove(scanner3.OnScanCompleted, new Action<Scanner>(this.OnScanCompleted));
					scanner.Item.Remove();
				}
			}
			this.Reset();
			this.failed = !completed;
		}

		// Token: 0x040002A8 RID: 680
		private readonly ContentXElement itemConfig;

		// Token: 0x040002A9 RID: 681
		private readonly List<Item> startingItems = new List<Item>();

		// Token: 0x040002AA RID: 682
		private readonly List<Scanner> scanners = new List<Scanner>();

		// Token: 0x040002AB RID: 683
		private readonly Dictionary<Item, ushort> parentInventoryIDs = new Dictionary<Item, ushort>();

		// Token: 0x040002AC RID: 684
		private readonly Dictionary<Item, int> inventorySlotIndices = new Dictionary<Item, int>();

		// Token: 0x040002AD RID: 685
		private readonly Dictionary<Item, byte> parentItemContainerIndices = new Dictionary<Item, byte>();

		// Token: 0x040002AE RID: 686
		private readonly int totalTargetsToScan;

		// Token: 0x040002AF RID: 687
		private readonly Dictionary<WayPoint, bool> scanTargets = new Dictionary<WayPoint, bool>();

		// Token: 0x040002B0 RID: 688
		private readonly HashSet<WayPoint> newTargetsScanned = new HashSet<WayPoint>();

		// Token: 0x040002B1 RID: 689
		private readonly float minTargetDistance;
	}
}
