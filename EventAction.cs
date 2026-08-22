using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200028D RID: 653
	internal abstract class EventAction
	{
		// Token: 0x060039AE RID: 14766 RVA: 0x0021CA78 File Offset: 0x0021AC78
		public EventAction(ScriptedEvent parentEvent, ContentXElement element)
		{
			this.ParentEvent = parentEvent;
			SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060039AF RID: 14767
		public abstract bool IsFinished(ref string goToLabel);

		// Token: 0x060039B0 RID: 14768 RVA: 0x0021CA94 File Offset: 0x0021AC94
		public virtual bool SetGoToTarget(string goTo)
		{
			return false;
		}

		// Token: 0x060039B1 RID: 14769
		public abstract void Reset();

		// Token: 0x060039B2 RID: 14770 RVA: 0x0021CA97 File Offset: 0x0021AC97
		public virtual bool CanBeFinished()
		{
			return true;
		}

		// Token: 0x060039B3 RID: 14771 RVA: 0x0021CA9A File Offset: 0x0021AC9A
		public virtual IEnumerable<EventAction> GetSubActions()
		{
			return Enumerable.Empty<EventAction>();
		}

		// Token: 0x060039B4 RID: 14772 RVA: 0x0021CAA1 File Offset: 0x0021ACA1
		public virtual void Update(float deltaTime)
		{
		}

		// Token: 0x060039B5 RID: 14773 RVA: 0x0021CAA4 File Offset: 0x0021ACA4
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

		// Token: 0x060039B6 RID: 14774 RVA: 0x0021CC80 File Offset: 0x0021AE80
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

		// Token: 0x060039B7 RID: 14775 RVA: 0x0021CD44 File Offset: 0x0021AF44
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

		// Token: 0x060039B8 RID: 14776 RVA: 0x0021CDD6 File Offset: 0x0021AFD6
		public virtual string ToDebugString()
		{
			return "[?] " + base.GetType().Name;
		}

		// Token: 0x04001DC0 RID: 7616
		public readonly ScriptedEvent ParentEvent;

		// Token: 0x02000F23 RID: 3875
		public class SubactionGroup
		{
			// Token: 0x17001C1A RID: 7194
			// (get) Token: 0x06008838 RID: 34872 RVA: 0x003A5558 File Offset: 0x003A3758
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

			// Token: 0x06008839 RID: 34873 RVA: 0x003A558C File Offset: 0x003A378C
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

			// Token: 0x0600883A RID: 34874 RVA: 0x003A5700 File Offset: 0x003A3900
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

			// Token: 0x0600883B RID: 34875 RVA: 0x003A5770 File Offset: 0x003A3970
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

			// Token: 0x0600883C RID: 34876 RVA: 0x003A57B8 File Offset: 0x003A39B8
			public void Reset()
			{
				this.Actions.ForEach(delegate(EventAction a)
				{
					a.Reset();
				});
				this.currentSubAction = 0;
			}

			// Token: 0x0600883D RID: 34877 RVA: 0x003A57EB File Offset: 0x003A39EB
			public void Update(float deltaTime)
			{
				if (this.currentSubAction < this.Actions.Count)
				{
					this.Actions[this.currentSubAction].Update(deltaTime);
				}
			}

			// Token: 0x040054B9 RID: 21689
			public List<EventAction> Actions;

			// Token: 0x040054BA RID: 21690
			private int currentSubAction;
		}
	}
}
