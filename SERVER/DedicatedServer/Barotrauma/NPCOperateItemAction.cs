using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x020001AA RID: 426
	internal class NPCOperateItemAction : EventAction
	{
		// Token: 0x1700091B RID: 2331
		// (get) Token: 0x06001FBE RID: 8126 RVA: 0x000D986A File Offset: 0x000D7A6A
		// (set) Token: 0x06001FBF RID: 8127 RVA: 0x000D9872 File Offset: 0x000D7A72
		[Serialize("", IsPropertySaveable.Yes, "Tag of the NPC(s) that should operate the item.", "", false)]
		public Identifier NPCTag { get; set; }

		// Token: 0x1700091C RID: 2332
		// (get) Token: 0x06001FC0 RID: 8128 RVA: 0x000D987B File Offset: 0x000D7A7B
		// (set) Token: 0x06001FC1 RID: 8129 RVA: 0x000D9883 File Offset: 0x000D7A83
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item to operate. If it's not something AI characters can or know how to operate, such as a cabinet or an engine, the NPC will just select it.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x1700091D RID: 2333
		// (get) Token: 0x06001FC2 RID: 8130 RVA: 0x000D988C File Offset: 0x000D7A8C
		// (set) Token: 0x06001FC3 RID: 8131 RVA: 0x000D9894 File Offset: 0x000D7A94
		[Serialize("Controller", IsPropertySaveable.Yes, "Name of the component to operate. For example, the Controller component of a periscope or the Reactor component of a nuclear reactor.", "", false)]
		public Identifier ItemComponentName { get; set; }

		// Token: 0x1700091E RID: 2334
		// (get) Token: 0x06001FC4 RID: 8132 RVA: 0x000D989D File Offset: 0x000D7A9D
		// (set) Token: 0x06001FC5 RID: 8133 RVA: 0x000D98A5 File Offset: 0x000D7AA5
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the option, if there are several ways the item can be operated. For example, \"powerup\" or \"shutdown\" when operating a reactor.", "", false)]
		public Identifier OrderOption { get; set; }

		// Token: 0x1700091F RID: 2335
		// (get) Token: 0x06001FC6 RID: 8134 RVA: 0x000D98AE File Offset: 0x000D7AAE
		// (set) Token: 0x06001FC7 RID: 8135 RVA: 0x000D98B6 File Offset: 0x000D7AB6
		[Serialize(false, IsPropertySaveable.Yes, "Should the character equip the item before attempting to operate it (only valid if the item is equippable).", "", false)]
		public bool RequireEquip { get; set; }

		// Token: 0x17000920 RID: 2336
		// (get) Token: 0x06001FC8 RID: 8136 RVA: 0x000D98BF File Offset: 0x000D7ABF
		// (set) Token: 0x06001FC9 RID: 8137 RVA: 0x000D98C7 File Offset: 0x000D7AC7
		[Serialize(true, IsPropertySaveable.Yes, "Should the character start or stop operating the item.", "", false)]
		public bool Operate { get; set; }

		// Token: 0x17000921 RID: 2337
		// (get) Token: 0x06001FCA RID: 8138 RVA: 0x000D98D0 File Offset: 0x000D7AD0
		// (set) Token: 0x06001FCB RID: 8139 RVA: 0x000D98D8 File Offset: 0x000D7AD8
		[Serialize(-1, IsPropertySaveable.Yes, "Maximum number of NPCs the action can target. For example, you could only make a specific number of security officers man a periscope.", "", false)]
		public int MaxTargets { get; set; }

		// Token: 0x17000922 RID: 2338
		// (get) Token: 0x06001FCC RID: 8140 RVA: 0x000D98E1 File Offset: 0x000D7AE1
		// (set) Token: 0x06001FCD RID: 8141 RVA: 0x000D98E9 File Offset: 0x000D7AE9
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

		// Token: 0x17000923 RID: 2339
		// (get) Token: 0x06001FCE RID: 8142 RVA: 0x000D9901 File Offset: 0x000D7B01
		// (set) Token: 0x06001FCF RID: 8143 RVA: 0x000D9909 File Offset: 0x000D7B09
		[Serialize(true, IsPropertySaveable.Yes, "The event actions reset when a GoTo action makes the event jump to a different point. Should the NPC stop operating the item when the event resets?", "", false)]
		public bool AbandonOnReset { get; set; }

		// Token: 0x06001FD0 RID: 8144 RVA: 0x000D9912 File Offset: 0x000D7B12
		public NPCOperateItemAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001FD1 RID: 8145 RVA: 0x000D991C File Offset: 0x000D7B1C
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

		// Token: 0x06001FD2 RID: 8146 RVA: 0x000D9C1C File Offset: 0x000D7E1C
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001FD3 RID: 8147 RVA: 0x000D9C24 File Offset: 0x000D7E24
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

		// Token: 0x06001FD4 RID: 8148 RVA: 0x000D9D14 File Offset: 0x000D7F14
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

		// Token: 0x04000F22 RID: 3874
		private float _priority;

		// Token: 0x04000F24 RID: 3876
		private bool isFinished;

		// Token: 0x04000F25 RID: 3877
		private List<Character> affectedNpcs;

		// Token: 0x04000F26 RID: 3878
		private Item target;
	}
}
