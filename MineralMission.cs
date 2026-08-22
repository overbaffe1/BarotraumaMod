using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000057 RID: 87
	internal class MineralMission : Mission
	{
		// Token: 0x17000339 RID: 825
		// (get) Token: 0x06000BAE RID: 2990 RVA: 0x0006D76E File Offset: 0x0006B96E
		public override bool DisplayAsCompleted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x06000BAF RID: 2991 RVA: 0x0006D771 File Offset: 0x0006B971
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x06000BB0 RID: 2992 RVA: 0x0006D774 File Offset: 0x0006B974
		// (set) Token: 0x06000BB1 RID: 2993 RVA: 0x0006D77C File Offset: 0x0006B97C
		public override int State
		{
			get
			{
				return base.State;
			}
			set
			{
				base.State = value;
				if (base.State > 0)
				{
					this.caves.ForEach(delegate(Level.Cave c)
					{
						c.MissionsToDisplayOnSonar.Remove(this);
					});
				}
			}
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x0006D7A8 File Offset: 0x0006B9A8
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			byte caveCount = msg.ReadByte();
			for (int i = 0; i < (int)caveCount; i++)
			{
				byte selectedCaveIndex = msg.ReadByte();
				if (selectedCaveIndex < 255 && Level.Loaded != null)
				{
					if ((int)selectedCaveIndex < Level.Loaded.Caves.Count)
					{
						Level.Cave selectedCave = Level.Loaded.Caves[(int)selectedCaveIndex];
						selectedCave.MissionsToDisplayOnSonar.Add(this);
						this.caves.Add(selectedCave);
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(83, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Cave index out of bounds when reading nest mission data. Index: ");
						defaultInterpolatedStringHandler.AppendFormatted<byte>(selectedCaveIndex);
						defaultInterpolatedStringHandler.AppendLiteral(", number of caves: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(Level.Loaded.Caves.Count);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
				}
			}
			for (int j = 0; j < this.resourceAmounts.Count; j++)
			{
				byte amount = msg.ReadByte();
				float rotation = msg.ReadSingle();
				for (int k = 0; k < (int)amount; k++)
				{
					Item item = Item.ReadSpawnData(msg, true);
					Holdable h = item.GetComponent<Holdable>();
					if (h != null)
					{
						h.AttachToWall();
						item.Rotation = rotation;
					}
					List<Item> resources;
					if (this.spawnedResources.TryGetValue(item.Prefab.Identifier, out resources))
					{
						resources.Add(item);
					}
					else
					{
						this.spawnedResources.Add(item.Prefab.Identifier, new List<Item>
						{
							item
						});
					}
				}
			}
			this.CalculateMissionClusterPositions();
			for (int l = 0; l < this.resourceAmounts.Count; l++)
			{
				Identifier identifier = msg.ReadIdentifier();
				byte count = msg.ReadByte();
				Item[] resources2 = new Item[(int)count];
				for (int m = 0; m < (int)count; m++)
				{
					ushort id = msg.ReadUInt16();
					Entity entity = Entity.FindEntityByID(id);
					Item item2 = entity as Item;
					if (item2 != null)
					{
						resources2[m] = item2;
					}
				}
				this.relevantLevelResources.Add(identifier, resources2);
			}
		}

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x06000BB3 RID: 2995 RVA: 0x0006D9AF File Offset: 0x0006BBAF
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
				return from p in this.missionClusterPositions
				where this.spawnedResources.ContainsKey(p.Item1) && this.AnyAreUncollected(this.spawnedResources[p.Item1])
				select new ValueTuple<LocalizedString, Vector2>(this.ModifyMessage(this.Prefab.SonarLabel, false), p.Item2);
			}
		}

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x06000BB4 RID: 2996 RVA: 0x0006D9D9 File Offset: 0x0006BBD9
		public IEnumerable<List<Item>> SpawnedResources
		{
			get
			{
				return this.spawnedResources.Values;
			}
		}

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x06000BB5 RID: 2997 RVA: 0x0006D9E6 File Offset: 0x0006BBE6
		public override LocalizedString SuccessMessage
		{
			get
			{
				return this.ModifyMessage(base.SuccessMessage, true);
			}
		}

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x06000BB6 RID: 2998 RVA: 0x0006D9F5 File Offset: 0x0006BBF5
		public override LocalizedString FailureMessage
		{
			get
			{
				return this.ModifyMessage(base.FailureMessage, true);
			}
		}

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x06000BB7 RID: 2999 RVA: 0x0006DA04 File Offset: 0x0006BC04
		public override LocalizedString Description
		{
			get
			{
				return this.ModifyMessage(this.description, true);
			}
		}

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x06000BB8 RID: 3000 RVA: 0x0006DA13 File Offset: 0x0006BC13
		public override LocalizedString Name
		{
			get
			{
				return this.ModifyMessage(base.Name, false);
			}
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x0006DA24 File Offset: 0x0006BC24
		public MineralMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			Level.PositionType positionType = prefab.ConfigElement.GetAttributeEnum<Level.PositionType>("PositionType", this.positionType);
			if (MineralMission.ValidPositionTypes.Contains(positionType))
			{
				this.positionType = positionType;
			}
			float handoverAmount = prefab.ConfigElement.GetAttributeFloat("ResourceHandoverAmount", 0f);
			this.resourceHandoverAmount = Math.Clamp(handoverAmount, 0f, 1f);
			ContentXElement configElement = prefab.ConfigElement.GetChildElement("Items");
			foreach (ContentXElement c in configElement.GetChildElements("Item"))
			{
				Identifier identifier = c.GetAttributeIdentifier("identifier", Identifier.Empty);
				if (!identifier.IsEmpty)
				{
					if (this.resourceAmounts.ContainsKey(identifier))
					{
						Dictionary<Identifier, int> dictionary = this.resourceAmounts;
						Identifier key = identifier;
						int num = dictionary[key];
						dictionary[key] = num + 1;
					}
					else
					{
						this.resourceAmounts.Add(identifier, 1);
					}
				}
			}
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x0006DB7C File Offset: 0x0006BD7C
		protected override void StartMissionSpecific(Level level)
		{
			if (this.spawnedResources.Any<KeyValuePair<Identifier, List<Item>>>())
			{
				DebugConsole.AddWarning("Spawned resources list was not empty at the start of a mineral mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.spawnedResources.Clear();
			}
			if (this.relevantLevelResources.Any<KeyValuePair<Identifier, Item[]>>())
			{
				DebugConsole.AddWarning("Relevant level resources list was not empty at the start of a mineral mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.relevantLevelResources.Clear();
			}
			if (this.missionClusterPositions.Any<ValueTuple<Identifier, Vector2>>())
			{
				DebugConsole.AddWarning("Mission cluster positions list was not empty at the start of a mineral mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.missionClusterPositions.Clear();
			}
			this.caves.Clear();
			if (Mission.IsClient)
			{
				return;
			}
			foreach (KeyValuePair<Identifier, int> keyValuePair in this.resourceAmounts)
			{
				Identifier identifier2;
				int num;
				keyValuePair.Deconstruct(out identifier2, out num);
				Identifier identifier = identifier2;
				int amount = num;
				ItemPrefab prefab = MapEntityPrefab.FindByIdentifier(identifier) as ItemPrefab;
				if (prefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Error in MineralMission: couldn't find an item prefab (identifier: \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\")");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
				}
				else
				{
					List<Item> spawnedResources = level.GenerateMissionResources(prefab, amount, this.positionType, this.caves);
					if (spawnedResources.Count < amount)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(43, 3);
						defaultInterpolatedStringHandler2.AppendLiteral("Error in MineralMission: spawned only ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(spawnedResources.Count);
						defaultInterpolatedStringHandler2.AppendLiteral("/");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(amount);
						defaultInterpolatedStringHandler2.AppendLiteral(" of ");
						defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(prefab.Name);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					}
					if (!spawnedResources.None(null))
					{
						this.spawnedResources.Add(identifier, spawnedResources);
						foreach (Level.Cave cave in Level.Loaded.Caves)
						{
							foreach (Item spawnedResource in spawnedResources)
							{
								if (cave.Area.Contains(spawnedResource.WorldPosition))
								{
									cave.MissionsToDisplayOnSonar.Add(this);
									this.caves.Add(cave);
									break;
								}
							}
						}
					}
				}
			}
			this.CalculateMissionClusterPositions();
			this.FindRelevantLevelResources();
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x0006DE3C File Offset: 0x0006C03C
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (Mission.IsClient)
			{
				return;
			}
			int state = this.State;
			if (state != 0)
			{
				if (state != 1)
				{
					return;
				}
				if (!Submarine.MainSub.AtEitherExit)
				{
					return;
				}
				this.State = 2;
				return;
			}
			else
			{
				if (!this.EnoughHaveBeenCollected())
				{
					return;
				}
				this.State = 1;
				return;
			}
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x0006DE85 File Offset: 0x0006C085
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.EnoughHaveBeenCollected();
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x0006DE90 File Offset: 0x0006C090
		protected override void EndMissionSpecific(bool completed)
		{
			this.failed = (!completed && this.state > 0);
			if (completed && !Mission.IsClient)
			{
				List<Item> handoverResources = new List<Item>();
				foreach (Identifier identifier in this.resourceAmounts.Keys)
				{
					Item[] availableResources;
					if (this.relevantLevelResources.TryGetValue(identifier, out availableResources))
					{
						IEnumerable<Item> collectedResources = availableResources.Where(new Func<Item, bool>(this.HasBeenCollected));
						if (collectedResources.Any<Item>())
						{
							int handoverCount = (int)MathF.Round(this.resourceHandoverAmount * (float)collectedResources.Count<Item>());
							for (int i = 0; i < handoverCount; i++)
							{
								handoverResources.Add(collectedResources.ElementAt(i));
							}
						}
					}
				}
				foreach (Item resource in handoverResources)
				{
					resource.Remove();
				}
			}
			foreach (KeyValuePair<Identifier, List<Item>> kvp in this.spawnedResources)
			{
				foreach (Item j in kvp.Value)
				{
					if (j != null && !j.Removed && !this.HasBeenCollected(j))
					{
						j.Remove();
					}
				}
			}
			this.spawnedResources.Clear();
			this.relevantLevelResources.Clear();
			this.missionClusterPositions.Clear();
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x0006E06C File Offset: 0x0006C26C
		private void FindRelevantLevelResources()
		{
			this.relevantLevelResources.Clear();
			using (Dictionary<Identifier, int>.KeyCollection.Enumerator enumerator = this.resourceAmounts.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Identifier identifier = enumerator.Current;
					Item[] items = Item.ItemList.Where(delegate(Item i)
					{
						if (i.Prefab.Identifier == identifier && i.Submarine == null && i.ParentInventory == null)
						{
							Holdable h = i.GetComponent<Holdable>();
							return h == null || (h.Attachable && h.Attached);
						}
						return false;
					}).ToArray<Item>();
					this.relevantLevelResources.Add(identifier, items);
				}
			}
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x0006E104 File Offset: 0x0006C304
		private bool EnoughHaveBeenCollected()
		{
			foreach (KeyValuePair<Identifier, int> kvp in this.resourceAmounts)
			{
				Item[] availableResources;
				if (!this.relevantLevelResources.TryGetValue(kvp.Key, out availableResources))
				{
					return false;
				}
				int collected = availableResources.Count(new Func<Item, bool>(this.HasBeenCollected));
				int needed = kvp.Value;
				if (collected < needed)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x0006E198 File Offset: 0x0006C398
		private bool HasBeenCollected(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (item.Removed)
			{
				return false;
			}
			Entity owner = item.GetRootInventoryOwner();
			if (owner.Submarine != null && owner.Submarine.Info.Type == SubmarineType.Player)
			{
				return true;
			}
			Character c = owner as Character;
			return c != null && c.Info != null && GameMain.GameSession.CrewManager.GetCharacterInfos(false).Contains(c.Info);
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x0006E208 File Offset: 0x0006C408
		private bool AnyAreUncollected(IEnumerable<Item> items)
		{
			return items.Any((Item i) => !this.HasBeenCollected(i));
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x0006E21C File Offset: 0x0006C41C
		private void CalculateMissionClusterPositions()
		{
			this.missionClusterPositions.Clear();
			foreach (KeyValuePair<Identifier, List<Item>> kvp in this.spawnedResources)
			{
				if (!kvp.Value.None(null))
				{
					Vector2 pos = Vector2.Zero;
					int itemCount = 0;
					foreach (Item j in from i in kvp.Value
					where i != null && !i.Removed
					select i)
					{
						pos += j.WorldPosition;
						itemCount++;
					}
					pos /= (float)itemCount;
					this.missionClusterPositions.Add(new ValueTuple<Identifier, Vector2>(kvp.Key, pos));
				}
			}
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x0006E328 File Offset: 0x0006C528
		protected override LocalizedString ModifyMessage(LocalizedString message, bool color = true)
		{
			MineralMission.<>c__DisplayClass38_0 CS$<>8__locals1;
			CS$<>8__locals1.color = color;
			CS$<>8__locals1.message = message;
			int i = 1;
			foreach (KeyValuePair<Identifier, int> keyValuePair in this.resourceAmounts)
			{
				Identifier identifier2;
				int num;
				keyValuePair.Deconstruct(out identifier2, out num);
				Identifier identifier = identifier2;
				int amount = num;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("[resourcename");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				string find = defaultInterpolatedStringHandler.ToStringAndClear();
				MapEntityPrefab mapEntityPrefab = MapEntityPrefab.FindByIdentifier(identifier);
				MineralMission.<ModifyMessage>g__Replace|38_0(find, ((mapEntityPrefab != null) ? mapEntityPrefab.Name.Value : null) ?? "", ref CS$<>8__locals1);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("[resourcequantity");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler2.AppendLiteral("]");
				MineralMission.<ModifyMessage>g__Replace|38_0(defaultInterpolatedStringHandler2.ToStringAndClear(), amount.ToString(), ref CS$<>8__locals1);
				i++;
			}
			MineralMission.<ModifyMessage>g__Replace|38_0("[handoverpercentage]", ToolBox.GetFormattedPercentage(this.resourceHandoverAmount), ref CS$<>8__locals1);
			return CS$<>8__locals1.message;
		}

		// Token: 0x06000BC9 RID: 3017 RVA: 0x0006E4D6 File Offset: 0x0006C6D6
		[CompilerGenerated]
		internal static void <ModifyMessage>g__Replace|38_0(string find, string replace, ref MineralMission.<>c__DisplayClass38_0 A_2)
		{
			if (A_2.color)
			{
				replace = "‖color:gui.orange‖" + replace + "‖end‖";
			}
			A_2.message = A_2.message.Replace(find, replace, StringComparison.Ordinal);
		}

		// Token: 0x0400061A RID: 1562
		private readonly Dictionary<Identifier, int> resourceAmounts = new Dictionary<Identifier, int>();

		// Token: 0x0400061B RID: 1563
		private readonly Dictionary<Identifier, List<Item>> spawnedResources = new Dictionary<Identifier, List<Item>>();

		// Token: 0x0400061C RID: 1564
		private readonly Dictionary<Identifier, Item[]> relevantLevelResources = new Dictionary<Identifier, Item[]>();

		// Token: 0x0400061D RID: 1565
		[TupleElementNames(new string[]
		{
			"Identifier",
			"Position"
		})]
		private readonly List<ValueTuple<Identifier, Vector2>> missionClusterPositions = new List<ValueTuple<Identifier, Vector2>>();

		// Token: 0x0400061E RID: 1566
		private readonly HashSet<Level.Cave> caves = new HashSet<Level.Cave>();

		// Token: 0x0400061F RID: 1567
		private readonly Level.PositionType positionType = Level.PositionType.Cave;

		// Token: 0x04000620 RID: 1568
		public static readonly ImmutableArray<Level.PositionType> ValidPositionTypes = RuntimeHelpers.CreateSpan<Level.PositionType>(fieldof(<PrivateImplementationDetails>.B04AE73851D6938965D1525A1E5CCFA868786B644211DB890187DC5F331947C24).FieldHandle).ToImmutableArray<Level.PositionType>();

		// Token: 0x04000621 RID: 1569
		private readonly float resourceHandoverAmount;
	}
}
