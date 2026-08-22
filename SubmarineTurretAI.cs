using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001A4 RID: 420
	internal class SubmarineTurretAI
	{
		// Token: 0x17000C68 RID: 3176
		// (get) Token: 0x06003005 RID: 12293 RVA: 0x001F9028 File Offset: 0x001F7228
		// (set) Token: 0x06003006 RID: 12294 RVA: 0x001F9030 File Offset: 0x001F7230
		public Submarine Submarine { get; protected set; }

		// Token: 0x06003007 RID: 12295 RVA: 0x001F903C File Offset: 0x001F723C
		public SubmarineTurretAI(Submarine submarine, Identifier friendlyTag = default(Identifier))
		{
			this.FriendlyTag = friendlyTag;
			this.Submarine = submarine;
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine == this.Submarine)
				{
					Turret turret = item.GetComponent<Turret>();
					if (turret != null)
					{
						this.turrets.Add(turret);
						turret.AutoOperate = false;
						turret.Item.Condition = turret.Item.MaxCondition;
						foreach (MapEntity linkedEntity in turret.Item.linkedTo)
						{
							Item linkedItem = linkedEntity as Item;
							if (linkedItem != null)
							{
								linkedItem.Condition = linkedItem.MaxCondition;
							}
						}
					}
				}
			}
			this.LoadAllTurrets();
		}

		// Token: 0x06003008 RID: 12296 RVA: 0x001F9154 File Offset: 0x001F7354
		public virtual void Update(float deltaTime)
		{
			if (this.Submarine == null || this.Submarine.Removed)
			{
				return;
			}
			this.OperateTurrets(deltaTime, this.FriendlyTag);
		}

		// Token: 0x06003009 RID: 12297 RVA: 0x001F917C File Offset: 0x001F737C
		protected virtual void LoadAllTurrets()
		{
			foreach (Turret turret in this.turrets)
			{
				this.LoadTurret(turret, null);
			}
		}

		// Token: 0x0600300A RID: 12298 RVA: 0x001F91D0 File Offset: 0x001F73D0
		protected void LoadTurret(Turret turret, Func<ItemPrefab, bool> ammoFilter = null)
		{
			foreach (Item linkedItem in turret.Item.GetLinkedEntities<Item>(null, null, null))
			{
				ItemContainer container = linkedItem.GetComponent<ItemContainer>();
				if (container != null)
				{
					int j;
					int i;
					Func<MapEntityPrefab, bool> <>9__0;
					for (i = 0; i < container.Inventory.Capacity; i = j + 1)
					{
						if (container.Inventory.GetItemAt(i) == null)
						{
							IEnumerable<MapEntityPrefab> list = MapEntityPrefab.List;
							Func<MapEntityPrefab, bool> predicate;
							if ((predicate = <>9__0) == null)
							{
								predicate = (<>9__0 = delegate(MapEntityPrefab e)
								{
									ItemPrefab ip = e as ItemPrefab;
									return ip != null && container.CanBeContained(ip, i) && (ammoFilter == null || ammoFilter(ip));
								});
							}
							ItemPrefab ammoPrefab = list.GetRandom(predicate, Rand.RandSync.ServerAndClient) as ItemPrefab;
							if (ammoPrefab != null)
							{
								Item ammo = new Item(ammoPrefab, container.Item.WorldPosition, this.Submarine, 0, true);
								if (!container.Inventory.TryPutItem(ammo, i, false, false, null, false, false, true))
								{
									turret.Item.Remove();
								}
							}
						}
						j = i;
					}
				}
			}
		}

		// Token: 0x0600300B RID: 12299 RVA: 0x001F9358 File Offset: 0x001F7558
		protected void OperateTurrets(float deltaTime, Identifier friendlyTag)
		{
			foreach (Turret turret in this.turrets)
			{
				turret.UpdateAutoOperate(deltaTime, true, friendlyTag);
			}
		}

		// Token: 0x04001904 RID: 6404
		protected readonly List<Turret> turrets = new List<Turret>();

		// Token: 0x04001905 RID: 6405
		public Identifier FriendlyTag;
	}
}
