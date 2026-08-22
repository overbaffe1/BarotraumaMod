using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200019A RID: 410
	internal abstract class EventAction
	{
		// Token: 0x06001EE4 RID: 7908 RVA: 0x000D75C4 File Offset: 0x000D57C4
		public EventAction(ScriptedEvent parentEvent, ContentXElement element)
		{
			this.ParentEvent = parentEvent;
			SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x06001EE5 RID: 7909
		public abstract bool IsFinished(ref string goToLabel);

		// Token: 0x06001EE6 RID: 7910 RVA: 0x000D75E0 File Offset: 0x000D57E0
		public virtual bool SetGoToTarget(string goTo)
		{
			return false;
		}

		// Token: 0x06001EE7 RID: 7911
		public abstract void Reset();

		// Token: 0x06001EE8 RID: 7912 RVA: 0x000D75E3 File Offset: 0x000D57E3
		public virtual bool CanBeFinished()
		{
			return true;
		}

		// Token: 0x06001EE9 RID: 7913 RVA: 0x000D75E6 File Offset: 0x000D57E6
		public virtual IEnumerable<EventAction> GetSubActions()
		{
			return Enumerable.Empty<EventAction>();
		}

		// Token: 0x06001EEA RID: 7914 RVA: 0x000D75ED File Offset: 0x000D57ED
		public virtual void Update(float deltaTime)
		{
		}

		// Token: 0x06001EEB RID: 7915 RVA: 0x000D75F0 File Offset: 0x000D57F0
		public static EventAction Instantiate(ScriptedEvent scriptedEvent, ContentXElement element)
		{
			Type actionType;
			try
			{
				Identifier typeName = element.Name.ToString().ToIdentifier();
				if (typeName == "TutorialSegmentAction")
				{
					typeName = "EventObjectiveAction".ToIdentifier();
				}
				else if (typeName == "TutorialHighlightAction")
				{
					typeName = "HighlightAction".ToIdentifier();
				}
				actionType = Type.GetType("Barotrauma." + typeName.ToString(), true, true);
				if (actionType == null)
				{
					throw new NullReferenceException();
				}
			}
			catch
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find an ");
				defaultInterpolatedStringHandler.AppendFormatted("EventAction");
				defaultInterpolatedStringHandler.AppendLiteral(" class of the type \"");
				defaultInterpolatedStringHandler.AppendFormatted<XName>(element.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				return null;
			}
			ConstructorInfo constructor = actionType.GetConstructor(new Type[]
			{
				typeof(ScriptedEvent),
				typeof(ContentXElement)
			});
			EventAction result;
			try
			{
				if (constructor == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(81, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in scripted event \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(scriptedEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral("\" - could not find a constructor for the EventAction \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Type>(actionType);
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				result = (constructor.Invoke(new object[]
				{
					scriptedEvent,
					element
				}) as EventAction);
			}
			catch (Exception ex)
			{
				DebugConsole.ThrowError((ex.InnerException != null) ? ex.InnerException.ToString() : ex.ToString(), null, element.ContentPackage, false, false);
				result = null;
			}
			return result;
		}

		// Token: 0x06001EEC RID: 7916 RVA: 0x000D77CC File Offset: 0x000D59CC
		protected void ApplyTagsToHulls(Entity entity, Identifier hullTag, Identifier linkedHullTag)
		{
			Item item = entity as Item;
			Hull hull;
			if (item == null)
			{
				Character character = entity as Character;
				if (character == null)
				{
					hull = null;
				}
				else
				{
					hull = character.CurrentHull;
				}
			}
			else
			{
				hull = item.CurrentHull;
			}
			Hull currentHull = hull;
			if (currentHull == null)
			{
				return;
			}
			if (!hullTag.IsEmpty)
			{
				this.ParentEvent.AddTarget(hullTag, currentHull);
			}
			if (!linkedHullTag.IsEmpty)
			{
				this.ParentEvent.AddTarget(linkedHullTag, currentHull);
				foreach (Hull linkedHull in currentHull.GetLinkedEntities<Hull>(null, null, null))
				{
					this.ParentEvent.AddTarget(linkedHullTag, linkedHull);
				}
			}
		}

		// Token: 0x06001EED RID: 7917 RVA: 0x000D7890 File Offset: 0x000D5A90
		protected string GetEventDebugName()
		{
			ScriptedEvent parentEvent = this.ParentEvent;
			Identifier? identifier2;
			if (parentEvent == null)
			{
				identifier2 = null;
			}
			else
			{
				EventPrefab prefab = parentEvent.Prefab;
				identifier2 = ((prefab != null) ? new Identifier?(prefab.Identifier) : null);
			}
			Identifier? identifier3 = identifier2;
			if (identifier3 != null)
			{
				Identifier identifier = identifier3.GetValueOrDefault();
				if (!identifier.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("the event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			}
			return "an unknown event";
		}

		// Token: 0x06001EEE RID: 7918 RVA: 0x000D7922 File Offset: 0x000D5B22
		public virtual string ToDebugString()
		{
			return "[?] " + base.GetType().Name;
		}

		// Token: 0x04000EC9 RID: 3785
		public readonly ScriptedEvent ParentEvent;

		// Token: 0x02000913 RID: 2323
		public class SubactionGroup
		{
			// Token: 0x1700154B RID: 5451
			// (get) Token: 0x060058A2 RID: 22690 RVA: 0x001F75C4 File Offset: 0x001F57C4
			public EventAction CurrentSubAction
			{
				get
				{
					if (this.currentSubAction >= 0 && this.Actions.Count > this.currentSubAction)
					{
						return this.Actions[this.currentSubAction];
					}
					return null;
				}
			}

			// Token: 0x060058A3 RID: 22691 RVA: 0x001F75F8 File Offset: 0x001F57F8
			public SubactionGroup(ScriptedEvent scriptedEvent, ContentXElement element)
			{
				SerializableProperty.DeserializeProperties(this, element);
				this.Actions = new List<EventAction>();
				foreach (ContentXElement e in element.Elements())
				{
					if (e.NameAsIdentifier().Equals("statuseffect"))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(142, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error in event prefab \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(scriptedEvent.Prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\". Status effect configured as a sub action. Please configure status effects as child elements of a StatusEffectAction.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					}
					else
					{
						if (e.NameAsIdentifier().Equals("OnRoundEndAction"))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(112, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Error in event prefab \"");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(scriptedEvent.Prefab.Identifier);
							defaultInterpolatedStringHandler2.AppendLiteral("\". ");
							defaultInterpolatedStringHandler2.AppendFormatted("OnRoundEndAction");
							defaultInterpolatedStringHandler2.AppendLiteral(" configured as a sub action. Please configure it as an action at the end of the event.");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
						}
						EventAction action = EventAction.Instantiate(scriptedEvent, e);
						if (action != null)
						{
							this.Actions.Add(action);
						}
					}
				}
			}

			// Token: 0x060058A4 RID: 22692 RVA: 0x001F776C File Offset: 0x001F596C
			public bool IsFinished(ref string goTo)
			{
				if (this.currentSubAction < this.Actions.Count)
				{
					string innerGoTo = null;
					if (this.Actions[this.currentSubAction].IsFinished(ref innerGoTo))
					{
						if (!string.IsNullOrEmpty(innerGoTo))
						{
							goTo = innerGoTo;
							return true;
						}
						this.currentSubAction++;
					}
				}
				return this.currentSubAction >= this.Actions.Count;
			}

			// Token: 0x060058A5 RID: 22693 RVA: 0x001F77DC File Offset: 0x001F59DC
			public bool SetGoToTarget(string goTo)
			{
				this.currentSubAction = 0;
				for (int i = 0; i < this.Actions.Count; i++)
				{
					if (this.Actions[i].SetGoToTarget(goTo))
					{
						this.currentSubAction = i;
						return true;
					}
				}
				return false;
			}

			// Token: 0x060058A6 RID: 22694 RVA: 0x001F7824 File Offset: 0x001F5A24
			public void Reset()
			{
				this.Actions.ForEach(delegate(EventAction a)
				{
					a.Reset();
				});
				this.currentSubAction = 0;
			}

			// Token: 0x060058A7 RID: 22695 RVA: 0x001F7857 File Offset: 0x001F5A57
			public void Update(float deltaTime)
			{
				if (this.currentSubAction < this.Actions.Count)
				{
					this.Actions[this.currentSubAction].Update(deltaTime);
				}
			}

			// Token: 0x040031ED RID: 12781
			public List<EventAction> Actions;

			// Token: 0x040031EE RID: 12782
			private int currentSubAction;
		}
	}
}
