using System;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000180 RID: 384
	internal class ArtifactEvent : Event
	{
		// Token: 0x17000870 RID: 2160
		// (get) Token: 0x06001D91 RID: 7569 RVA: 0x000D258E File Offset: 0x000D078E
		public bool SpawnPending
		{
			get
			{
				return this.spawnPending;
			}
		}

		// Token: 0x17000871 RID: 2161
		// (get) Token: 0x06001D92 RID: 7570 RVA: 0x000D2596 File Offset: 0x000D0796
		public int State
		{
			get
			{
				return this.state;
			}
		}

		// Token: 0x17000872 RID: 2162
		// (get) Token: 0x06001D93 RID: 7571 RVA: 0x000D259E File Offset: 0x000D079E
		public Item Item
		{
			get
			{
				return this.item;
			}
		}

		// Token: 0x17000873 RID: 2163
		// (get) Token: 0x06001D94 RID: 7572 RVA: 0x000D25A6 File Offset: 0x000D07A6
		public Vector2 SpawnPos
		{
			get
			{
				return this.spawnPos;
			}
		}

		// Token: 0x17000874 RID: 2164
		// (get) Token: 0x06001D95 RID: 7573 RVA: 0x000D25AE File Offset: 0x000D07AE
		public override Vector2 DebugDrawPos
		{
			get
			{
				return this.spawnPos;
			}
		}

		// Token: 0x06001D96 RID: 7574 RVA: 0x000D25B8 File Offset: 0x000D07B8
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
			defaultInterpolatedStringHandler.AppendLiteral("ArtifactEvent (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>((this.itemPrefab == null) ? "null" : this.itemPrefab.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001D97 RID: 7575 RVA: 0x000D2614 File Offset: 0x000D0814
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

		// Token: 0x06001D98 RID: 7576 RVA: 0x000D26F4 File Offset: 0x000D08F4
		protected override void InitEventSpecific(EventSet parentSet)
		{
			this.spawnPos = Level.Loaded.GetRandomItemPos((Rand.Value(Rand.RandSync.ServerAndClient) < 0.5f) ? (Level.PositionType.MainPath | Level.PositionType.SidePath) : (Level.PositionType.Cave | Level.PositionType.Ruin), 500f, 10000f, 30f, this.SpawnPosFilter);
			this.spawnPending = true;
		}

		// Token: 0x06001D99 RID: 7577 RVA: 0x000D2734 File Offset: 0x000D0934
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

		// Token: 0x06001D9A RID: 7578 RVA: 0x000D2800 File Offset: 0x000D0A00
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
			if (GameMain.Server != null)
			{
				Entity.Spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(this.item));
			}
		}

		// Token: 0x06001D9B RID: 7579 RVA: 0x000D2900 File Offset: 0x000D0B00
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

		// Token: 0x04000E49 RID: 3657
		private ItemPrefab itemPrefab;

		// Token: 0x04000E4A RID: 3658
		private Item item;

		// Token: 0x04000E4B RID: 3659
		private int state;

		// Token: 0x04000E4C RID: 3660
		private Vector2 spawnPos;

		// Token: 0x04000E4D RID: 3661
		private bool spawnPending;
	}
}
