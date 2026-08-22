using System;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000274 RID: 628
	internal class ArtifactEvent : Event
	{
		// Token: 0x17000EC4 RID: 3780
		// (get) Token: 0x0600385D RID: 14429 RVA: 0x00217BC6 File Offset: 0x00215DC6
		public bool SpawnPending
		{
			get
			{
				return this.spawnPending;
			}
		}

		// Token: 0x17000EC5 RID: 3781
		// (get) Token: 0x0600385E RID: 14430 RVA: 0x00217BCE File Offset: 0x00215DCE
		public int State
		{
			get
			{
				return this.state;
			}
		}

		// Token: 0x17000EC6 RID: 3782
		// (get) Token: 0x0600385F RID: 14431 RVA: 0x00217BD6 File Offset: 0x00215DD6
		public Item Item
		{
			get
			{
				return this.item;
			}
		}

		// Token: 0x17000EC7 RID: 3783
		// (get) Token: 0x06003860 RID: 14432 RVA: 0x00217BDE File Offset: 0x00215DDE
		public Vector2 SpawnPos
		{
			get
			{
				return this.spawnPos;
			}
		}

		// Token: 0x17000EC8 RID: 3784
		// (get) Token: 0x06003861 RID: 14433 RVA: 0x00217BE6 File Offset: 0x00215DE6
		public override Vector2 DebugDrawPos
		{
			get
			{
				return this.spawnPos;
			}
		}

		// Token: 0x06003862 RID: 14434 RVA: 0x00217BF0 File Offset: 0x00215DF0
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("ArtifactEvent (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>((this.itemPrefab == null) ? "null" : this.itemPrefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003863 RID: 14435 RVA: 0x00217C4C File Offset: 0x00215E4C
		public ArtifactEvent(EventPrefab prefab, int seed) : base(prefab, seed)
		{
			if (prefab.ConfigElement.GetAttribute("itemname") != null)
			{
				DebugConsole.ThrowError("Error in ArtifactEvent - use item identifier instead of the name of the item.", null, (prefab != null) ? prefab.ContentPackage : null, false, false);
				string itemName = prefab.ConfigElement.GetAttributeString("itemname", "");
				this.itemPrefab = (MapEntityPrefab.Find(itemName, null, true) as ItemPrefab);
				if (this.itemPrefab == null)
				{
					DebugConsole.ThrowError("Error in SalvageMission: couldn't find an item prefab with the name " + itemName, null, null, false, false);
					return;
				}
			}
			else
			{
				Identifier itemIdentifier = prefab.ConfigElement.GetAttributeIdentifier("itemidentifier", Identifier.Empty);
				this.itemPrefab = (MapEntityPrefab.FindByIdentifier(itemIdentifier) as ItemPrefab);
				if (this.itemPrefab == null)
				{
					DebugConsole.ThrowError("Error in ArtifactEvent - couldn't find an item prefab with the identifier " + itemIdentifier.ToString(), null, (prefab != null) ? prefab.ContentPackage : null, false, false);
				}
			}
		}

		// Token: 0x06003864 RID: 14436 RVA: 0x00217D2C File Offset: 0x00215F2C
		protected override void InitEventSpecific(EventSet parentSet)
		{
			this.spawnPos = Level.Loaded.GetRandomItemPos((Rand.Value(Rand.RandSync.ServerAndClient) < 0.5f) ? (Level.PositionType.MainPath | Level.PositionType.SidePath) : (Level.PositionType.Cave | Level.PositionType.Ruin), 500f, 10000f, 30f, this.SpawnPosFilter);
			this.spawnPending = true;
		}

		// Token: 0x06003865 RID: 14437 RVA: 0x00217D6C File Offset: 0x00215F6C
		public override string GetDebugInfo()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 4);
			defaultInterpolatedStringHandler.AppendLiteral("Finished: ");
			defaultInterpolatedStringHandler.AppendFormatted(base.IsFinished.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Item: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Item.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Spawn pending: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.SpawnPending.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Spawn position: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.SpawnPos.ColorizeObject());
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003866 RID: 14438 RVA: 0x00217E38 File Offset: 0x00216038
		private void SpawnItem()
		{
			this.item = new Item(this.itemPrefab, this.spawnPos, null, 0, true);
			this.item.body.FarseerBody.BodyType = BodyType.Kinematic;
			foreach (Item it in Item.ItemList)
			{
				if (it.Submarine == null && it.HasTag(Tags.ArtifactHolder))
				{
					ItemContainer itemContainer = it.GetComponent<ItemContainer>();
					if (itemContainer != null && itemContainer.Combine(this.item, null))
					{
						break;
					}
				}
			}
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage("Initialized ArtifactEvent (" + this.item.Name + ")", new Color?(Color.White), false);
			}
		}

		// Token: 0x06003867 RID: 14439 RVA: 0x00217F1C File Offset: 0x0021611C
		public override void Update(float deltaTime)
		{
			if (this.spawnPending)
			{
				if (this.itemPrefab == null)
				{
					this.isFinished = true;
					return;
				}
				this.SpawnItem();
				this.spawnPending = false;
			}
			int num = this.state;
			if (num != 0)
			{
				if (num != 1)
				{
					return;
				}
				if (!Submarine.MainSub.AtEitherExit)
				{
					return;
				}
				this.Finish();
				this.state = 2;
				return;
			}
			else
			{
				if (this.item.ParentInventory != null)
				{
					this.item.body.FarseerBody.BodyType = BodyType.Dynamic;
				}
				if (this.item.CurrentHull == null)
				{
					return;
				}
				this.state = 1;
				return;
			}
		}

		// Token: 0x04001D40 RID: 7488
		private ItemPrefab itemPrefab;

		// Token: 0x04001D41 RID: 7489
		private Item item;

		// Token: 0x04001D42 RID: 7490
		private int state;

		// Token: 0x04001D43 RID: 7491
		private Vector2 spawnPos;

		// Token: 0x04001D44 RID: 7492
		private bool spawnPending;
	}
}
