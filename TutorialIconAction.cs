using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Tutorials;

namespace Barotrauma
{
	// Token: 0x020002AD RID: 685
	internal class TutorialIconAction : EventAction
	{
		// Token: 0x17000FB5 RID: 4021
		// (get) Token: 0x06003BA8 RID: 15272 RVA: 0x0022438D File Offset: 0x0022258D
		// (set) Token: 0x06003BA9 RID: 15273 RVA: 0x00224395 File Offset: 0x00222595
		[Serialize(TutorialIconAction.ActionType.Add, IsPropertySaveable.Yes, "What to do with the icon. Add = add an icon, Remove = remove the icon that has the specific target and style, RemoveTarget = remove all icons assigned to the specific target, RemoveIcon = remove all icons with the specific style, Remove = remove all icons.", "", false)]
		public TutorialIconAction.ActionType Type { get; set; }

		// Token: 0x17000FB6 RID: 4022
		// (get) Token: 0x06003BAA RID: 15274 RVA: 0x0022439E File Offset: 0x0022259E
		// (set) Token: 0x06003BAB RID: 15275 RVA: 0x002243A6 File Offset: 0x002225A6
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target to assign the icon to.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000FB7 RID: 4023
		// (get) Token: 0x06003BAC RID: 15276 RVA: 0x002243AF File Offset: 0x002225AF
		// (set) Token: 0x06003BAD RID: 15277 RVA: 0x002243B7 File Offset: 0x002225B7
		[Serialize("", IsPropertySaveable.Yes, "Style of the icon.", "", false)]
		public Identifier IconStyle { get; set; }

		// Token: 0x06003BAE RID: 15278 RVA: 0x002243C0 File Offset: 0x002225C0
		public TutorialIconAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003BAF RID: 15279 RVA: 0x002243CC File Offset: 0x002225CC
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			TutorialMode tutorialMode = ((gameSession != null) ? gameSession.GameMode : null) as TutorialMode;
			if (tutorialMode != null)
			{
				Entity target = this.ParentEvent.GetTargets(this.TargetTag).FirstOrDefault<Entity>();
				if (target != null)
				{
					if (this.Type == TutorialIconAction.ActionType.Add)
					{
						Tutorial tutorial = tutorialMode.Tutorial;
						if (tutorial != null)
						{
							tutorial.Icons.Add(new ValueTuple<Entity, Identifier>(target, this.IconStyle));
						}
					}
					else if (this.Type == TutorialIconAction.ActionType.Remove)
					{
						Tutorial tutorial2 = tutorialMode.Tutorial;
						if (tutorial2 != null)
						{
							tutorial2.Icons.RemoveAll(delegate([TupleElementNames(new string[]
							{
								"entity",
								"iconStyle"
							})] ValueTuple<Entity, Identifier> i)
							{
								if (i.Item1 == target)
								{
									Identifier iconStyle = this.IconStyle;
									return i.Item2 == iconStyle;
								}
								return false;
							});
						}
					}
					else if (this.Type == TutorialIconAction.ActionType.RemoveTarget)
					{
						Tutorial tutorial3 = tutorialMode.Tutorial;
						if (tutorial3 != null)
						{
							tutorial3.Icons.RemoveAll(([TupleElementNames(new string[]
							{
								"entity",
								"iconStyle"
							})] ValueTuple<Entity, Identifier> i) => i.Item1 == target);
						}
					}
					else if (this.Type == TutorialIconAction.ActionType.RemoveIcon)
					{
						Tutorial tutorial4 = tutorialMode.Tutorial;
						if (tutorial4 != null)
						{
							tutorial4.Icons.RemoveAll(delegate([TupleElementNames(new string[]
							{
								"entity",
								"iconStyle"
							})] ValueTuple<Entity, Identifier> i)
							{
								Identifier iconStyle = this.IconStyle;
								return i.Item2 == iconStyle;
							});
						}
					}
					else if (this.Type == TutorialIconAction.ActionType.Clear)
					{
						Tutorial tutorial5 = tutorialMode.Tutorial;
						if (tutorial5 != null)
						{
							tutorial5.Icons.Clear();
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003BB0 RID: 15280 RVA: 0x0022451D File Offset: 0x0022271D
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06003BB1 RID: 15281 RVA: 0x00224525 File Offset: 0x00222725
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04001E7D RID: 7805
		private bool isFinished;

		// Token: 0x02000F4E RID: 3918
		public enum ActionType
		{
			// Token: 0x04005545 RID: 21829
			Add,
			// Token: 0x04005546 RID: 21830
			Remove,
			// Token: 0x04005547 RID: 21831
			RemoveTarget,
			// Token: 0x04005548 RID: 21832
			RemoveIcon,
			// Token: 0x04005549 RID: 21833
			Clear
		}
	}
}
