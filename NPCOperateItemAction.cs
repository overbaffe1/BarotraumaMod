using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200029C RID: 668
	internal class NPCOperateItemAction : EventAction
	{
		// Token: 0x17000F61 RID: 3937
		// (get) Token: 0x06003A6A RID: 14954 RVA: 0x0021F386 File Offset: 0x0021D586
		// (set) Token: 0x06003A6B RID: 14955 RVA: 0x0021F38E File Offset: 0x0021D58E
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) that should operate the item.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x17000F62 RID: 3938
		// (get) Token: 0x06003A6C RID: 14956 RVA: 0x0021F397 File Offset: 0x0021D597
		// (set) Token: 0x06003A6D RID: 14957 RVA: 0x0021F39F File Offset: 0x0021D59F
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item to operate. If it's not something AI characters can or know how to operate, such as a cabinet or an engine, the NPC will just select it.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000F63 RID: 3939
		// (get) Token: 0x06003A6E RID: 14958 RVA: 0x0021F3A8 File Offset: 0x0021D5A8
		// (set) Token: 0x06003A6F RID: 14959 RVA: 0x0021F3B0 File Offset: 0x0021D5B0
		[Serialize("Controller", IsPropertySaveable.Yes, "Name of the component to operate. For example, the Controller component of a periscope or the Reactor component of a nuclear reactor.", "", false)]
		public Identifier ItemComponentName { get; set; }

		// Token: 0x17000F64 RID: 3940
		// (get) Token: 0x06003A70 RID: 14960 RVA: 0x0021F3B9 File Offset: 0x0021D5B9
		// (set) Token: 0x06003A71 RID: 14961 RVA: 0x0021F3C1 File Offset: 0x0021D5C1
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the option, if there are several ways the item can be operated. For example, \"powerup\" or \"shutdown\" when operating a reactor.", "", false)]
		public Identifier OrderOption { get; set; }

		// Token: 0x17000F65 RID: 3941
		// (get) Token: 0x06003A72 RID: 14962 RVA: 0x0021F3CA File Offset: 0x0021D5CA
		// (set) Token: 0x06003A73 RID: 14963 RVA: 0x0021F3D2 File Offset: 0x0021D5D2
		[Serialize(false, IsPropertySaveable.Yes, "Should the character equip the item before attempting to operate it (only valid if the item is equippable).", "", false)]
		public bool RequireEquip { get; set; }

		// Token: 0x17000F66 RID: 3942
		// (get) Token: 0x06003A74 RID: 14964 RVA: 0x0021F3DB File Offset: 0x0021D5DB
		// (set) Token: 0x06003A75 RID: 14965 RVA: 0x0021F3E3 File Offset: 0x0021D5E3
		[Serialize(true, IsPropertySaveable.Yes, "Should the character start or stop operating the item.", "", false)]
		public bool Operate { get; set; }

		// Token: 0x17000F67 RID: 3943
		// (get) Token: 0x06003A76 RID: 14966 RVA: 0x0021F3EC File Offset: 0x0021D5EC
		// (set) Token: 0x06003A77 RID: 14967 RVA: 0x0021F3F4 File Offset: 0x0021D5F4
		[Serialize(-1, IsPropertySaveable.Yes, "Maximum number of NPCs the action can target. For example, you could only make a specific number of security officers man a periscope.", "", false)]
		public int MaxTargets { get; set; }

		// Token: 0x17000F68 RID: 3944
		// (get) Token: 0x06003A78 RID: 14968 RVA: 0x0021F3FD File Offset: 0x0021D5FD
		// (set) Token: 0x06003A79 RID: 14969 RVA: 0x0021F405 File Offset: 0x0021D605
		[Serialize(100f, IsPropertySaveable.Yes, "AI priority for the action. Uses 100 by default, which is the absolute maximum for any objectives, meaning nothing can be prioritized over it, including the emergency objectives, such as find safety and combat.Setting the priority to 70 would function like a regular order, but with the highest priority.A priority of 60 would make the objective work like a lowest priority order.So, if we'll want the character to operate the item, but still be able to find safety, defend themselves when attacked, or flee from dangers,it's better to use e.g. 70 instead of 100.", "", false)]
		public float Priority
		{
			get
			{
				return this._priority;
			}
			set
			{
				this._priority = Math.Clamp(value, 60f, 100f);
			}
		}

		// Token: 0x17000F69 RID: 3945
		// (get) Token: 0x06003A7A RID: 14970 RVA: 0x0021F41D File Offset: 0x0021D61D
		// (set) Token: 0x06003A7B RID: 14971 RVA: 0x0021F425 File Offset: 0x0021D625
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC stop operating the item when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x06003A7C RID: 14972 RVA: 0x0021F42E File Offset: 0x0021D62E
		public NPCOperateItemAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003A7D RID: 14973 RVA: 0x0021F438 File Offset: 0x0021D638
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Item> potentialTargets = this.ParentEvent.GetTargets(this.TargetTag).OfType<Item>();
			IEnumerable<Item> nonSelectedItems = potentialTargets.Where(delegate(Item it)
			{
				Controller component = it.GetComponent<Controller>();
				return ((component != null) ? component.User : null) == null;
			});
			this.target = (nonSelectedItems.Any<Item>() ? nonSelectedItems.GetRandomUnsynced<Item>() : potentialTargets.GetRandomUnsynced<Item>());
			if (this.target == null)
			{
				return;
			}
			int targetCount = 0;
			this.affectedNpcs = (from c in this.ParentEvent.GetTargets(this.NPCTag)
			where c is Character
			select c as Character).ToList<Character>();
			foreach (Character npc in this.affectedNpcs)
			{
				if (!npc.Removed)
				{
					HumanAIController humanAiController = npc.AIController as HumanAIController;
					if (humanAiController != null)
					{
						if (this.Operate)
						{
							ItemComponent itemComponent = this.target.Components.FirstOrDefault(delegate(ItemComponent ic)
							{
								Identifier itemComponentName = this.ItemComponentName;
								return itemComponentName == ic.Name;
							});
							if (itemComponent == null)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Error in NPCOperateItemAction: could not find the component \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ItemComponentName);
								defaultInterpolatedStringHandler.AppendLiteral("\" in item \"");
								defaultInterpolatedStringHandler.AppendFormatted(this.target.Name);
								defaultInterpolatedStringHandler.AppendLiteral("\".");
								DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							}
							else
							{
								AIObjectiveOperateItem newObjective = new AIObjectiveOperateItem(itemComponent, npc, humanAiController.ObjectiveManager, this.OrderOption, this.RequireEquip, null, false, null, 1f)
								{
									OverridePriority = new float?(this.Priority)
								};
								humanAiController.ObjectiveManager.AddObjective<AIObjectiveOperateItem>(newObjective);
								humanAiController.ObjectiveManager.WaitTimer = 0f;
								humanAiController.ObjectiveManager.Objectives.RemoveAll(delegate(AIObjective o)
								{
									AIObjectiveGoTo gotoOjective = o as AIObjectiveGoTo;
									return gotoOjective != null;
								});
							}
						}
						else
						{
							foreach (AIObjective objective in humanAiController.ObjectiveManager.Objectives)
							{
								AIObjectiveOperateItem operateItemObjective = objective as AIObjectiveOperateItem;
								if (operateItemObjective != null && operateItemObjective.Component.Item == this.target)
								{
									objective.Abandon = true;
								}
							}
						}
						targetCount++;
						if (this.MaxTargets > -1 && targetCount >= this.MaxTargets)
						{
							break;
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003A7E RID: 14974 RVA: 0x0021F738 File Offset: 0x0021D938
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A7F RID: 14975 RVA: 0x0021F740 File Offset: 0x0021D940
		public override void Reset()
		{
			if (this.affectedNpcs != null && this.target != null && this.AbandonOnReset)
			{
				foreach (Character npc in this.affectedNpcs)
				{
					if (!npc.Removed)
					{
						HumanAIController humanAiController = npc.AIController as HumanAIController;
						if (humanAiController != null)
						{
							foreach (AIObjectiveOperateItem operateItemObjective in humanAiController.ObjectiveManager.GetActiveObjectives<AIObjectiveOperateItem>())
							{
								if (operateItemObjective.Component.Item == this.target)
								{
									operateItemObjective.Abandon = true;
								}
							}
						}
					}
				}
				this.target = null;
				this.affectedNpcs = null;
			}
			this.isFinished = false;
		}

		// Token: 0x06003A80 RID: 14976 RVA: 0x0021F830 File Offset: 0x0021DA30
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("AIObjectiveOperateItem");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (NPCTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.NPCTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Operate: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Operate.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001E0B RID: 7691
		private float _priority;

		// Token: 0x04001E0D RID: 7693
		private bool isFinished;

		// Token: 0x04001E0E RID: 7694
		private List<Character> affectedNpcs;

		// Token: 0x04001E0F RID: 7695
		private Item target;
	}
}
