using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200009E RID: 158
	internal class SubmarineTurretAI
	{
		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x060012FD RID: 4861 RVA: 0x000A6148 File Offset: 0x000A4348
		// (set) Token: 0x060012FE RID: 4862 RVA: 0x000A6150 File Offset: 0x000A4350
		public Submarine Submarine { get; protected set; }

		// Token: 0x060012FF RID: 4863 RVA: 0x000A615C File Offset: 0x000A435C
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

		// Token: 0x06001300 RID: 4864 RVA: 0x000A6274 File Offset: 0x000A4474
		public virtual void Update(float deltaTime)
		{
			if (this.Submarine == null || this.Submarine.Removed)
			{
				return;
			}
			this.OperateTurrets(deltaTime, this.FriendlyTag);
		}

		// Token: 0x06001301 RID: 4865 RVA: 0x000A629C File Offset: 0x000A449C
		protected virtual void LoadAllTurrets()
		{
			foreach (Turret turret in this.turrets)
			{
				this.LoadTurret(turret, null);
			}
		}

		// Token: 0x06001302 RID: 4866 RVA: 0x000A62F0 File Offset: 0x000A44F0
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

		// Token: 0x06001303 RID: 4867 RVA: 0x000A6478 File Offset: 0x000A4678
		protected void OperateTurrets(float deltaTime, Identifier friendlyTag)
		{
			foreach (Turret turret in this.turrets)
			{
				turret.UpdateAutoOperate(deltaTime, true, friendlyTag);
			}
		}

		// Token: 0x0400090A RID: 2314
		protected readonly List<Turret> turrets = new List<Turret>();

		// Token: 0x0400090B RID: 2315
		public Identifier FriendlyTag;
	}
}
