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
	// Token: 0x0200005F RID: 95
	internal class ScanMission : Mission
	{
		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x06000CEA RID: 3306 RVA: 0x00076F54 File Offset: 0x00075154
		public override IEnumerable<Entity> HudIconTargets
		{
			get
			{
				return from kvp in this.scanTargets
				where !kvp.Value
				select kvp.Key;
			}
		}

		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x06000CEB RID: 3307 RVA: 0x00076FAF File Offset: 0x000751AF
		public override bool DisplayAsCompleted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06000CEC RID: 3308 RVA: 0x00076FB2 File Offset: 0x000751B2
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000CED RID: 3309 RVA: 0x00076FB8 File Offset: 0x000751B8
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			this.startingItems.Clear();
			ushort itemCount = msg.ReadUInt16();
			for (int i = 0; i < (int)itemCount; i++)
			{
				this.startingItems.Add(Item.ReadSpawnData(msg, true));
			}
			if (this.startingItems.Contains(null))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in ScanMission.ClientReadInitial: item list contains null (mission: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (this.startingItems.Count != (int)itemCount)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(100, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in ScanMission.ClientReadInitial: item count does not match the server count (");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(itemCount);
				defaultInterpolatedStringHandler2.AppendLiteral(" != ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.startingItems.Count);
				defaultInterpolatedStringHandler2.AppendLiteral(", mission: ");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			this.scanners.Clear();
			this.GetScanners();
			this.ClientReadScanTargetStatus(msg);
		}

		// Token: 0x06000CEE RID: 3310 RVA: 0x000770E4 File Offset: 0x000752E4
		public override void ClientRead(IReadMessage msg)
		{
			base.ClientRead(msg);
			this.ClientReadScanTargetStatus(msg);
		}

		// Token: 0x06000CEF RID: 3311 RVA: 0x000770F4 File Offset: 0x000752F4
		private void ClientReadScanTargetStatus(IReadMessage msg)
		{
			this.scanTargets.Clear();
			byte targetsToScan = msg.ReadByte();
			for (int i = 0; i < (int)targetsToScan; i++)
			{
				ushort id = msg.ReadUInt16();
				bool scanned = msg.ReadBoolean();
				Entity entity = Entity.FindEntityByID(id);
				WayPoint wayPoint = entity as WayPoint;
				if (wayPoint == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to find a waypoint in ScanMission.ClientReadScanTargetStatus. Entity ");
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(id);
					defaultInterpolatedStringHandler.AppendLiteral(" was ");
					defaultInterpolatedStringHandler.AppendFormatted(((entity != null) ? entity.ToString() : null) ?? null);
					string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("ScanMission.ClientReadScanTargetStatus", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				}
				else
				{
					this.scanTargets.Add(wayPoint, scanned);
				}
			}
		}

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x06000CF0 RID: 3312 RVA: 0x000771BD File Offset: 0x000753BD
		// (set) Token: 0x06000CF1 RID: 3313 RVA: 0x000771C5 File Offset: 0x000753C5
		private Ruin TargetRuin { get; set; }

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x06000CF2 RID: 3314 RVA: 0x000771D0 File Offset: 0x000753D0
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

		// Token: 0x06000CF3 RID: 3315 RVA: 0x00077228 File Offset: 0x00075428
		public ScanMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.itemConfig = prefab.ConfigElement.GetChildElement("Items");
			this.totalTargetsToScan = prefab.ConfigElement.GetAttributeInt("targets", 1);
			this.minTargetDistance = prefab.ConfigElement.GetAttributeFloat("mintargetdistance", 0f);
		}

		// Token: 0x06000CF4 RID: 3316 RVA: 0x000772D4 File Offset: 0x000754D4
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

		// Token: 0x06000CF5 RID: 3317 RVA: 0x00077624 File Offset: 0x00075824
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

		// Token: 0x06000CF6 RID: 3318 RVA: 0x0007767C File Offset: 0x0007587C
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

		// Token: 0x06000CF7 RID: 3319 RVA: 0x00077794 File Offset: 0x00075994
		protected override void MissionStateChanged(int previousState)
		{
			if (previousState < this.State)
			{
				SteamTimelineManager.OnScanSuccessful(this);
			}
		}

		// Token: 0x06000CF8 RID: 3320 RVA: 0x000777A8 File Offset: 0x000759A8
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

		// Token: 0x06000CF9 RID: 3321 RVA: 0x00077858 File Offset: 0x00075A58
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

		// Token: 0x06000CFA RID: 3322 RVA: 0x000778C8 File Offset: 0x00075AC8
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
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x0007799C File Offset: 0x00075B9C
		private static bool IsValidScanPosition(Scanner scanner, KeyValuePair<WayPoint, bool> scanStatus, float scanRadiusSquared)
		{
			return !scanStatus.Value && scanStatus.Key.Submarine == scanner.Item.Submarine && Vector2.DistanceSquared(scanStatus.Key.WorldPosition, scanner.Item.WorldPosition) <= scanRadiusSquared;
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x000779F4 File Offset: 0x00075BF4
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (Mission.IsClient)
			{
				return;
			}
			this.State = Math.Max(this.State, this.scanTargets.Count((KeyValuePair<WayPoint, bool> kvp) => kvp.Value));
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x00077A44 File Offset: 0x00075C44
		private bool AllTargetsScanned()
		{
			return this.State >= this.totalTargetsToScan;
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x00077A57 File Offset: 0x00075C57
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.AllTargetsScanned();
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x00077A60 File Offset: 0x00075C60
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

		// Token: 0x040006AE RID: 1710
		private readonly ContentXElement itemConfig;

		// Token: 0x040006AF RID: 1711
		private readonly List<Item> startingItems = new List<Item>();

		// Token: 0x040006B0 RID: 1712
		private readonly List<Scanner> scanners = new List<Scanner>();

		// Token: 0x040006B1 RID: 1713
		private readonly Dictionary<Item, ushort> parentInventoryIDs = new Dictionary<Item, ushort>();

		// Token: 0x040006B2 RID: 1714
		private readonly Dictionary<Item, int> inventorySlotIndices = new Dictionary<Item, int>();

		// Token: 0x040006B3 RID: 1715
		private readonly Dictionary<Item, byte> parentItemContainerIndices = new Dictionary<Item, byte>();

		// Token: 0x040006B4 RID: 1716
		private readonly int totalTargetsToScan;

		// Token: 0x040006B5 RID: 1717
		private readonly Dictionary<WayPoint, bool> scanTargets = new Dictionary<WayPoint, bool>();

		// Token: 0x040006B6 RID: 1718
		private readonly HashSet<WayPoint> newTargetsScanned = new HashSet<WayPoint>();

		// Token: 0x040006B7 RID: 1719
		private readonly float minTargetDistance;
	}
}
