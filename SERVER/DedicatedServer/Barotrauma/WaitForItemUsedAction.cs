using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001BF RID: 447
	[NullableContext(1)]
	[Nullable(0)]
	internal class WaitForItemUsedAction : EventAction
	{
		// Token: 0x1700097F RID: 2431
		// (get) Token: 0x06002132 RID: 8498 RVA: 0x000DEC27 File Offset: 0x000DCE27
		// (set) Token: 0x06002133 RID: 8499 RVA: 0x000DEC2F File Offset: 0x000DCE2F
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item that must be used. Note that the item needs to have been tagged by the event - this does not refer to the tags that can be set per-item in the sub editor.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000980 RID: 2432
		// (get) Token: 0x06002134 RID: 8500 RVA: 0x000DEC38 File Offset: 0x000DCE38
		// (set) Token: 0x06002135 RID: 8501 RVA: 0x000DEC40 File Offset: 0x000DCE40
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character that must use the item. If there's multiple matching characters, it's enough if any of them use the item. If empty, it doesn't matter who uses the item.", "", false)]
		public Identifier UserTag { get; set; }

		// Token: 0x17000981 RID: 2433
		// (get) Token: 0x06002136 RID: 8502 RVA: 0x000DEC49 File Offset: 0x000DCE49
		// (set) Token: 0x06002137 RID: 8503 RVA: 0x000DEC51 File Offset: 0x000DCE51
		[Serialize("", IsPropertySaveable.Yes, "Name of the ItemComponent that the character must use. If empty, the character attempts to use all of them.", "", false)]
		public Identifier TargetItemComponent { get; set; }

		// Token: 0x17000982 RID: 2434
		// (get) Token: 0x06002138 RID: 8504 RVA: 0x000DEC5A File Offset: 0x000DCE5A
		// (set) Token: 0x06002139 RID: 8505 RVA: 0x000DEC62 File Offset: 0x000DCE62
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the target item when it's used.", "", false)]
		public Identifier ApplyTagToItem { get; set; }

		// Token: 0x17000983 RID: 2435
		// (get) Token: 0x0600213A RID: 8506 RVA: 0x000DEC6B File Offset: 0x000DCE6B
		// (set) Token: 0x0600213B RID: 8507 RVA: 0x000DEC73 File Offset: 0x000DCE73
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the user when the target item is used.", "", false)]
		public Identifier ApplyTagToUser { get; set; }

		// Token: 0x17000984 RID: 2436
		// (get) Token: 0x0600213C RID: 8508 RVA: 0x000DEC7C File Offset: 0x000DCE7C
		// (set) Token: 0x0600213D RID: 8509 RVA: 0x000DEC84 File Offset: 0x000DCE84
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the hull the target item is inside when the item is used.", "", false)]
		public Identifier ApplyTagToHull { get; set; }

		// Token: 0x17000985 RID: 2437
		// (get) Token: 0x0600213E RID: 8510 RVA: 0x000DEC8D File Offset: 0x000DCE8D
		// (set) Token: 0x0600213F RID: 8511 RVA: 0x000DEC95 File Offset: 0x000DCE95
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the hull the target item is inside, and all the hulls it's linked to, when the item is used.", "", false)]
		public Identifier ApplyTagToLinkedHulls { get; set; }

		// Token: 0x17000986 RID: 2438
		// (get) Token: 0x06002140 RID: 8512 RVA: 0x000DEC9E File Offset: 0x000DCE9E
		// (set) Token: 0x06002141 RID: 8513 RVA: 0x000DECA6 File Offset: 0x000DCEA6
		[Serialize(1, IsPropertySaveable.Yes, "How many times does the item need to be used. Defaults to 1.", "", false)]
		public int RequiredUseCount { get; set; }

		// Token: 0x17000987 RID: 2439
		// (get) Token: 0x06002142 RID: 8514 RVA: 0x000DECB0 File Offset: 0x000DCEB0
		private Identifier OnUseEventIdentifier
		{
			get
			{
				if (this.onUseEventIdentifier.IsEmpty)
				{
					this.onUseEventIdentifier = (this.ParentEvent.Prefab.Identifier.ToString() + this.ParentEvent.Actions.IndexOf(this).ToString() + WaitForItemUsedAction.IdCounter.ToString()).ToIdentifier();
					WaitForItemUsedAction.IdCounter++;
				}
				return this.onUseEventIdentifier;
			}
		}

		// Token: 0x06002143 RID: 8515 RVA: 0x000DED2C File Offset: 0x000DCF2C
		public WaitForItemUsedAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.ItemTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ParentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\". ");
				defaultInterpolatedStringHandler.AppendFormatted("ItemTag");
				defaultInterpolatedStringHandler.AppendLiteral(" not set in ");
				defaultInterpolatedStringHandler.AppendFormatted("WaitForItemUsedAction");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
		}

		// Token: 0x06002144 RID: 8516 RVA: 0x000DEDE0 File Offset: 0x000DCFE0
		private void OnItemUsed(Item item, Character user)
		{
			if (!this.UserTag.IsEmpty && !this.ParentEvent.GetTargets(this.UserTag).Contains(user))
			{
				return;
			}
			this.useCount++;
			if (this.useCount < this.RequiredUseCount)
			{
				return;
			}
			if (!this.ApplyTagToItem.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.ApplyTagToItem, item);
			}
			if (!this.ApplyTagToUser.IsEmpty && user != null)
			{
				this.ParentEvent.AddTarget(this.ApplyTagToUser, user);
			}
			base.ApplyTagsToHulls(item, this.ApplyTagToHull, this.ApplyTagToLinkedHulls);
			this.DeregisterTargets();
			this.isFinished = true;
		}

		// Token: 0x06002145 RID: 8517 RVA: 0x000DEE9B File Offset: 0x000DD09B
		public override void Update(float deltaTime)
		{
			this.TryRegisterTargets();
		}

		// Token: 0x06002146 RID: 8518 RVA: 0x000DEEA4 File Offset: 0x000DD0A4
		private void TryRegisterTargets()
		{
			foreach (Entity target in this.ParentEvent.GetTargets(this.ItemTag))
			{
				if (!this.targets.Contains(target))
				{
					Item item = target as Item;
					if (item != null)
					{
						if (this.TargetItemComponent.IsEmpty)
						{
							item.GetComponents<ItemComponent>().ForEach(delegate(ItemComponent ic)
							{
								this.<TryRegisterTargets>g__Register|43_0(ic);
							});
						}
						else
						{
							ItemComponent targetItemComponent = item.Components.FirstOrDefault(delegate(ItemComponent ic)
							{
								string name = ic.Name;
								Identifier targetItemComponent2 = this.TargetItemComponent;
								return name == targetItemComponent2;
							});
							if (targetItemComponent != null)
							{
								this.<TryRegisterTargets>g__Register|43_0(targetItemComponent);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002147 RID: 8519 RVA: 0x000DEF5C File Offset: 0x000DD15C
		private void DeregisterTargets()
		{
			foreach (ItemComponent ic in this.targetComponents)
			{
				ic.OnUsed.Deregister(this.OnUseEventIdentifier);
			}
			this.targetComponents.Clear();
			this.targets.Clear();
		}

		// Token: 0x06002148 RID: 8520 RVA: 0x000DEFD0 File Offset: 0x000DD1D0
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06002149 RID: 8521 RVA: 0x000DEFD8 File Offset: 0x000DD1D8
		public override void Reset()
		{
			this.isFinished = false;
			this.useCount = 0;
			this.DeregisterTargets();
		}

		// Token: 0x0600214A RID: 8522 RVA: 0x000DEFF0 File Offset: 0x000DD1F0
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("WaitForItemUsedAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ItemTag);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600214D RID: 8525 RVA: 0x000DF089 File Offset: 0x000DD289
		[CompilerGenerated]
		private void <TryRegisterTargets>g__Register|43_0(ItemComponent ic)
		{
			this.targets.Add(ic.Item);
			this.targetComponents.Add(ic);
			ic.OnUsed.RegisterOverwriteExisting(this.OnUseEventIdentifier, delegate(ItemComponent.ItemUseInfo i)
			{
				this.OnItemUsed(i.Item, i.User);
			});
		}

		// Token: 0x04000FA4 RID: 4004
		private static int IdCounter;

		// Token: 0x04000FAD RID: 4013
		private bool isFinished;

		// Token: 0x04000FAE RID: 4014
		private readonly HashSet<Entity> targets = new HashSet<Entity>();

		// Token: 0x04000FAF RID: 4015
		private readonly HashSet<ItemComponent> targetComponents = new HashSet<ItemComponent>();

		// Token: 0x04000FB0 RID: 4016
		private int useCount;

		// Token: 0x04000FB1 RID: 4017
		private Identifier onUseEventIdentifier;
	}
}
