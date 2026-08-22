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
	// Token: 0x02000026 RID: 38
	internal class MineralMission : Mission
	{
		// Token: 0x060004A4 RID: 1188 RVA: 0x000294B8 File Offset: 0x000276B8
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			msg.WriteByte((byte)this.caves.Count);
			foreach (Level.Cave cave in this.caves)
			{
				msg.WriteByte((byte)((Level.Loaded == null || !Level.Loaded.Caves.Contains(cave)) ? 255 : Level.Loaded.Caves.IndexOf(cave)));
			}
			foreach (KeyValuePair<Identifier, List<Item>> kvp in this.spawnedResources)
			{
				msg.WriteByte((byte)kvp.Value.Count);
				Item item3 = kvp.Value.FirstOrDefault<Item>();
				msg.WriteSingle((item3 != null) ? item3.Rotation : 0f);
				foreach (Item item in kvp.Value)
				{
					item.WriteSpawnData(msg, item.ID, 0, 0, -1);
				}
			}
			foreach (KeyValuePair<Identifier, Item[]> kvp2 in this.relevantLevelResources)
			{
				msg.WriteIdentifier(kvp2.Key);
				msg.WriteByte((byte)kvp2.Value.Length);
				foreach (Item item2 in kvp2.Value)
				{
					msg.WriteUInt16(item2.ID);
				}
			}
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x060004A5 RID: 1189 RVA: 0x000296A8 File Offset: 0x000278A8
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

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x060004A6 RID: 1190 RVA: 0x000296D2 File Offset: 0x000278D2
		public IEnumerable<List<Item>> SpawnedResources
		{
			get
			{
				return this.spawnedResources.Values;
			}
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x060004A7 RID: 1191 RVA: 0x000296DF File Offset: 0x000278DF
		public override LocalizedString SuccessMessage
		{
			get
			{
				return this.ModifyMessage(base.SuccessMessage, true);
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x060004A8 RID: 1192 RVA: 0x000296EE File Offset: 0x000278EE
		public override LocalizedString FailureMessage
		{
			get
			{
				return this.ModifyMessage(base.FailureMessage, true);
			}
		}

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x060004A9 RID: 1193 RVA: 0x000296FD File Offset: 0x000278FD
		public override LocalizedString Description
		{
			get
			{
				return this.ModifyMessage(this.description, true);
			}
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x060004AA RID: 1194 RVA: 0x0002970C File Offset: 0x0002790C
		public override LocalizedString Name
		{
			get
			{
				return this.ModifyMessage(base.Name, false);
			}
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x0002971C File Offset: 0x0002791C
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

		// Token: 0x060004AC RID: 1196 RVA: 0x00029874 File Offset: 0x00027A74
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

		// Token: 0x060004AD RID: 1197 RVA: 0x00029B34 File Offset: 0x00027D34
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

		// Token: 0x060004AE RID: 1198 RVA: 0x00029B7D File Offset: 0x00027D7D
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.EnoughHaveBeenCollected();
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x00029B88 File Offset: 0x00027D88
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

		// Token: 0x060004B0 RID: 1200 RVA: 0x00029D64 File Offset: 0x00027F64
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

		// Token: 0x060004B1 RID: 1201 RVA: 0x00029DFC File Offset: 0x00027FFC
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

		// Token: 0x060004B2 RID: 1202 RVA: 0x00029E90 File Offset: 0x00028090
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

		// Token: 0x060004B3 RID: 1203 RVA: 0x00029F00 File Offset: 0x00028100
		private bool AnyAreUncollected(IEnumerable<Item> items)
		{
			return items.Any((Item i) => !this.HasBeenCollected(i));
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00029F14 File Offset: 0x00028114
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

		// Token: 0x060004B5 RID: 1205 RVA: 0x0002A020 File Offset: 0x00028220
		protected override LocalizedString ModifyMessage(LocalizedString message, bool color = true)
		{
			MineralMission.<>c__DisplayClass31_0 CS$<>8__locals1;
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
				MineralMission.<ModifyMessage>g__Replace|31_0(find, ((mapEntityPrefab != null) ? mapEntityPrefab.Name.Value : null) ?? "", ref CS$<>8__locals1);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("[resourcequantity");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler2.AppendLiteral("]");
				MineralMission.<ModifyMessage>g__Replace|31_0(defaultInterpolatedStringHandler2.ToStringAndClear(), amount.ToString(), ref CS$<>8__locals1);
				i++;
			}
			MineralMission.<ModifyMessage>g__Replace|31_0("[handoverpercentage]", ToolBox.GetFormattedPercentage(this.resourceHandoverAmount), ref CS$<>8__locals1);
			return CS$<>8__locals1.message;
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x0002A1BF File Offset: 0x000283BF
		[CompilerGenerated]
		internal static void <ModifyMessage>g__Replace|31_0(string find, string replace, ref MineralMission.<>c__DisplayClass31_0 A_2)
		{
			if (A_2.color)
			{
				replace = "‖color:gui.orange‖" + replace + "‖end‖";
			}
			A_2.message = A_2.message.Replace(find, replace, StringComparison.Ordinal);
		}

		// Token: 0x0400025C RID: 604
		private readonly Dictionary<Identifier, int> resourceAmounts = new Dictionary<Identifier, int>();

		// Token: 0x0400025D RID: 605
		private readonly Dictionary<Identifier, List<Item>> spawnedResources = new Dictionary<Identifier, List<Item>>();

		// Token: 0x0400025E RID: 606
		private readonly Dictionary<Identifier, Item[]> relevantLevelResources = new Dictionary<Identifier, Item[]>();

		// Token: 0x0400025F RID: 607
		[TupleElementNames(new string[]
		{
			"Identifier",
			"Position"
		})]
		private readonly List<ValueTuple<Identifier, Vector2>> missionClusterPositions = new List<ValueTuple<Identifier, Vector2>>();

		// Token: 0x04000260 RID: 608
		private readonly HashSet<Level.Cave> caves = new HashSet<Level.Cave>();

		// Token: 0x04000261 RID: 609
		private readonly Level.PositionType positionType = Level.PositionType.Cave;

		// Token: 0x04000262 RID: 610
		public static readonly ImmutableArray<Level.PositionType> ValidPositionTypes = RuntimeHelpers.CreateSpan<Level.PositionType>(fieldof(<PrivateImplementationDetails>.B04AE73851D6938965D1525A1E5CCFA868786B644211DB890187DC5F331947C24).FieldHandle).ToImmutableArray<Level.PositionType>();

		// Token: 0x04000263 RID: 611
		private readonly float resourceHandoverAmount;
	}
}
