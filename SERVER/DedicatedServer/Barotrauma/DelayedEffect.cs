using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000292 RID: 658
	internal class DelayedEffect : StatusEffect
	{
		// Token: 0x06002E17 RID: 11799 RVA: 0x00130DB0 File Offset: 0x0012EFB0
		public DelayedEffect(ContentXElement element, string parentDebugName) : base(element, parentDebugName)
		{
			string key = "delaytype";
			DelayedEffect.DelayTypes delayTypes = DelayedEffect.DelayTypes.Timer;
			this.delayType = element.GetAttributeEnum<DelayedEffect.DelayTypes>(key, delayTypes);
			if (this.delayType == DelayedEffect.DelayTypes.ReachCursor)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Potential error in ");
				defaultInterpolatedStringHandler.AppendFormatted(parentDebugName);
				defaultInterpolatedStringHandler.AppendLiteral(": the delay type ");
				defaultInterpolatedStringHandler.AppendFormatted<DelayedEffect.DelayTypes>(DelayedEffect.DelayTypes.ReachCursor);
				defaultInterpolatedStringHandler.AppendLiteral(" is not supported.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), element.ContentPackage);
			}
			if (this.delayType == DelayedEffect.DelayTypes.Timer)
			{
				this.delay = element.GetAttributeFloat("delay", 1f);
			}
		}

		// Token: 0x06002E18 RID: 11800 RVA: 0x00130E54 File Offset: 0x0012F054
		public override void Apply(ActionType type, float deltaTime, Entity entity, ISerializableEntity target, Vector2? worldPosition = null)
		{
			if (this.type != type || !this.HasRequiredItems(entity))
			{
				return;
			}
			if (!this.Stackable)
			{
				foreach (DelayedListElement existingEffect in DelayedEffect.DelayList)
				{
					if (existingEffect.Parent == this && existingEffect.Targets.FirstOrDefault<ISerializableEntity>() == target)
					{
						return;
					}
				}
			}
			if (!base.IsValidTarget(target))
			{
				return;
			}
			this.currentTargets.Clear();
			this.currentTargets.Add(target);
			if (!base.HasRequiredConditions(this.currentTargets))
			{
				return;
			}
			DelayedEffect.DelayTypes delayTypes = this.delayType;
			if (delayTypes == DelayedEffect.DelayTypes.Timer)
			{
				DelayedListElement newDelayListElement = new DelayedListElement(this, entity, this.currentTargets, this.delay, new Vector2?(worldPosition ?? base.GetPosition(entity, this.currentTargets, worldPosition)), null)
				{
					GetPositionBasedOnTargets = (worldPosition == null)
				};
				DelayedEffect.DelayList.Add(newDelayListElement);
				return;
			}
			if (delayTypes != DelayedEffect.DelayTypes.ReachCursor)
			{
				return;
			}
			Item item = entity as Item;
			Projectile projectile = (item != null) ? item.GetComponent<Projectile>() : null;
			if (projectile == null)
			{
				DebugConsole.LogError("Non-projectile using a delaytype of reachcursor", null, null);
				return;
			}
			Character character;
			if ((character = projectile.User) == null && (character = projectile.Attacker) == null)
			{
				Item launcher = projectile.Launcher;
				character = (((launcher != null) ? launcher.GetRootInventoryOwner() : null) as Character);
			}
			if (character == null)
			{
				return;
			}
			DelayedEffect.DelayList.Add(new DelayedListElement(this, entity, this.currentTargets, Vector2.Distance(entity.WorldPosition, projectile.User.CursorWorldPosition), worldPosition, new Vector2?(entity.WorldPosition)));
		}

		// Token: 0x06002E19 RID: 11801 RVA: 0x00131018 File Offset: 0x0012F218
		public override void Apply(ActionType type, float deltaTime, Entity entity, IReadOnlyList<ISerializableEntity> targets, Vector2? worldPosition = null)
		{
			if (this.type != type)
			{
				return;
			}
			if (base.Disabled)
			{
				return;
			}
			if (base.ShouldWaitForInterval(entity, deltaTime))
			{
				return;
			}
			if (!this.HasRequiredItems(entity))
			{
				return;
			}
			if (this.delayType == DelayedEffect.DelayTypes.ReachCursor && Character.Controlled == null)
			{
				return;
			}
			if (!this.Stackable)
			{
				foreach (DelayedListElement existingEffect in DelayedEffect.DelayList)
				{
					if (existingEffect.Parent == this && existingEffect.Targets.SequenceEqual(targets))
					{
						return;
					}
				}
			}
			this.currentTargets.Clear();
			foreach (ISerializableEntity target in targets)
			{
				if (base.IsValidTarget(target))
				{
					this.currentTargets.Add(target);
				}
			}
			if (!base.HasRequiredConditions(this.currentTargets))
			{
				return;
			}
			DelayedEffect.DelayTypes delayTypes = this.delayType;
			if (delayTypes == DelayedEffect.DelayTypes.Timer)
			{
				DelayedEffect.DelayList.Add(new DelayedListElement(this, entity, this.currentTargets, this.delay, worldPosition, null));
				return;
			}
			if (delayTypes != DelayedEffect.DelayTypes.ReachCursor)
			{
				return;
			}
			Item item = entity as Item;
			Projectile projectile = (item != null) ? item.GetComponent<Projectile>() : null;
			if (projectile == null)
			{
				return;
			}
			Character character;
			if ((character = projectile.User) == null && (character = projectile.Attacker) == null)
			{
				Item launcher = projectile.Launcher;
				character = (((launcher != null) ? launcher.GetRootInventoryOwner() : null) as Character);
			}
			Character user = character;
			if (user == null)
			{
				return;
			}
			DelayedEffect.DelayList.Add(new DelayedListElement(this, entity, this.currentTargets, Vector2.Distance(entity.WorldPosition, user.CursorWorldPosition), worldPosition, new Vector2?(entity.WorldPosition)));
		}

		// Token: 0x06002E1A RID: 11802 RVA: 0x001311E4 File Offset: 0x0012F3E4
		public static void Update(float deltaTime)
		{
			for (int i = DelayedEffect.DelayList.Count - 1; i >= 0; i--)
			{
				DelayedListElement element = DelayedEffect.DelayList[i];
				if (element.Parent.CheckConditionalAlways && !element.Parent.HasRequiredConditions(element.Targets))
				{
					DelayedEffect.DelayList.Remove(element);
				}
				else
				{
					DelayedEffect.DelayTypes delayTypes = element.Parent.delayType;
					if (delayTypes != DelayedEffect.DelayTypes.Timer)
					{
						if (delayTypes == DelayedEffect.DelayTypes.ReachCursor)
						{
							if (Vector2.Distance(element.Entity.WorldPosition, element.StartPosition.Value) < element.Delay)
							{
								goto IL_125;
							}
						}
					}
					else
					{
						element.Delay -= deltaTime;
						if (element.Delay > 0f)
						{
							if (!element.GetPositionBasedOnTargets)
							{
								goto IL_125;
							}
							Entity entity = element.Entity;
							if (entity != null && !entity.Removed)
							{
								element.WorldPosition = new Vector2?(element.Parent.GetPosition(element.Entity, element.Parent.currentTargets, null));
								goto IL_125;
							}
							goto IL_125;
						}
					}
					element.Parent.Apply(deltaTime, element.Entity, element.Targets, element.WorldPosition);
					DelayedEffect.DelayList.Remove(element);
				}
				IL_125:;
			}
		}

		// Token: 0x0400168F RID: 5775
		public static readonly List<DelayedListElement> DelayList = new List<DelayedListElement>();

		// Token: 0x04001690 RID: 5776
		private readonly DelayedEffect.DelayTypes delayType;

		// Token: 0x04001691 RID: 5777
		private readonly float delay;

		// Token: 0x02000B01 RID: 2817
		private enum DelayTypes
		{
			// Token: 0x0400380B RID: 14347
			Timer,
			// Token: 0x0400380C RID: 14348
			[Obsolete("The delay type is unsupported.")]
			ReachCursor
		}
	}
}
