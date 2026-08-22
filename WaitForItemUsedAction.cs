using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020002B1 RID: 689
	[NullableContext(1)]
	[Nullable(0)]
	internal class WaitForItemUsedAction : EventAction
	{
		// Token: 0x17000FBE RID: 4030
		// (get) Token: 0x06003BD0 RID: 15312 RVA: 0x00224ABB File Offset: 0x00222CBB
		// (set) Token: 0x06003BD1 RID: 15313 RVA: 0x00224AC3 File Offset: 0x00222CC3
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item that must be used. Note that the item needs to have been tagged by the event - this does not refer to the tags that can be set per-item in the sub editor.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000FBF RID: 4031
		// (get) Token: 0x06003BD2 RID: 15314 RVA: 0x00224ACC File Offset: 0x00222CCC
		// (set) Token: 0x06003BD3 RID: 15315 RVA: 0x00224AD4 File Offset: 0x00222CD4
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character that must use the item. If there's multiple matching characters, it's enough if any of them use the item. If empty, it doesn't matter who uses the item.", "", false)]
		public Identifier UserTag { get; set; }

		// Token: 0x17000FC0 RID: 4032
		// (get) Token: 0x06003BD4 RID: 15316 RVA: 0x00224ADD File Offset: 0x00222CDD
		// (set) Token: 0x06003BD5 RID: 15317 RVA: 0x00224AE5 File Offset: 0x00222CE5
		[Serialize("", IsPropertySaveable.Yes, "Name of the ItemComponent that the character must use. If empty, the character attempts to use all of them.", "", false)]
		public Identifier TargetItemComponent { get; set; }

		// Token: 0x17000FC1 RID: 4033
		// (get) Token: 0x06003BD6 RID: 15318 RVA: 0x00224AEE File Offset: 0x00222CEE
		// (set) Token: 0x06003BD7 RID: 15319 RVA: 0x00224AF6 File Offset: 0x00222CF6
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the target item when it's used.", "", false)]
		public Identifier ApplyTagToItem { get; set; }

		// Token: 0x17000FC2 RID: 4034
		// (get) Token: 0x06003BD8 RID: 15320 RVA: 0x00224AFF File Offset: 0x00222CFF
		// (set) Token: 0x06003BD9 RID: 15321 RVA: 0x00224B07 File Offset: 0x00222D07
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the user when the target item is used.", "", false)]
		public Identifier ApplyTagToUser { get; set; }

		// Token: 0x17000FC3 RID: 4035
		// (get) Token: 0x06003BDA RID: 15322 RVA: 0x00224B10 File Offset: 0x00222D10
		// (set) Token: 0x06003BDB RID: 15323 RVA: 0x00224B18 File Offset: 0x00222D18
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the hull the target item is inside when the item is used.", "", false)]
		public Identifier ApplyTagToHull { get; set; }

		// Token: 0x17000FC4 RID: 4036
		// (get) Token: 0x06003BDC RID: 15324 RVA: 0x00224B21 File Offset: 0x00222D21
		// (set) Token: 0x06003BDD RID: 15325 RVA: 0x00224B29 File Offset: 0x00222D29
		[Serialize("", IsPropertySaveable.Yes, "Optional tag to apply to the hull the target item is inside, and all the hulls it's linked to, when the item is used.", "", false)]
		public Identifier ApplyTagToLinkedHulls { get; set; }

		// Token: 0x17000FC5 RID: 4037
		// (get) Token: 0x06003BDE RID: 15326 RVA: 0x00224B32 File Offset: 0x00222D32
		// (set) Token: 0x06003BDF RID: 15327 RVA: 0x00224B3A File Offset: 0x00222D3A
		[Serialize(1, IsPropertySaveable.Yes, "How many times does the item need to be used. Defaults to 1.", "", false)]
		public int RequiredUseCount { get; set; }

		// Token: 0x17000FC6 RID: 4038
		// (get) Token: 0x06003BE0 RID: 15328 RVA: 0x00224B44 File Offset: 0x00222D44
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

		// Token: 0x06003BE1 RID: 15329 RVA: 0x00224BC0 File Offset: 0x00222DC0
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

		// Token: 0x06003BE2 RID: 15330 RVA: 0x00224C74 File Offset: 0x00222E74
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

		// Token: 0x06003BE3 RID: 15331 RVA: 0x00224D2F File Offset: 0x00222F2F
		public override void Update(float deltaTime)
		{
			this.TryRegisterTargets();
		}

		// Token: 0x06003BE4 RID: 15332 RVA: 0x00224D38 File Offset: 0x00222F38
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

		// Token: 0x06003BE5 RID: 15333 RVA: 0x00224DF0 File Offset: 0x00222FF0
		private void DeregisterTargets()
		{
			foreach (ItemComponent ic in this.targetComponents)
			{
				ic.OnUsed.Deregister(this.OnUseEventIdentifier);
			}
			this.targetComponents.Clear();
			this.targets.Clear();
		}

		// Token: 0x06003BE6 RID: 15334 RVA: 0x00224E64 File Offset: 0x00223064
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003BE7 RID: 15335 RVA: 0x00224E6C File Offset: 0x0022306C
		public override void Reset()
		{
			this.isFinished = false;
			this.useCount = 0;
			this.DeregisterTargets();
		}

		// Token: 0x06003BE8 RID: 15336 RVA: 0x00224E84 File Offset: 0x00223084
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

		// Token: 0x06003BEB RID: 15339 RVA: 0x00224F1D File Offset: 0x0022311D
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

		// Token: 0x04001E88 RID: 7816
		private static int IdCounter;

		// Token: 0x04001E91 RID: 7825
		private bool isFinished;

		// Token: 0x04001E92 RID: 7826
		private readonly HashSet<Entity> targets = new HashSet<Entity>();

		// Token: 0x04001E93 RID: 7827
		private readonly HashSet<ItemComponent> targetComponents = new HashSet<ItemComponent>();

		// Token: 0x04001E94 RID: 7828
		private int useCount;

		// Token: 0x04001E95 RID: 7829
		private Identifier onUseEventIdentifier;
	}
}
