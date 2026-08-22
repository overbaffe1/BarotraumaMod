using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000184 RID: 388
	internal abstract class BinaryOptionAction : EventAction
	{
		// Token: 0x06001DC8 RID: 7624 RVA: 0x000D314C File Offset: 0x000D134C
		public BinaryOptionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			foreach (ContentXElement elem in element.Elements())
			{
				string elemName = elem.Name.LocalName;
				if (elemName.Equals("success", StringComparison.InvariantCultureIgnoreCase))
				{
					if (this.Success == null)
					{
						this.Success = new EventAction.SubactionGroup(this.ParentEvent, elem);
					}
				}
				else if (elemName.Equals("failure", StringComparison.InvariantCultureIgnoreCase) && this.Failure == null)
				{
					this.Failure = new EventAction.SubactionGroup(this.ParentEvent, elem);
				}
			}
		}

		// Token: 0x06001DC9 RID: 7625 RVA: 0x000D31F8 File Offset: 0x000D13F8
		public override IEnumerable<EventAction> GetSubActions()
		{
			EventAction.SubactionGroup success = this.Success;
			IEnumerable<EventAction> enumerable = (success != null) ? success.Actions : null;
			IEnumerable<EventAction> actions = enumerable ?? Enumerable.Empty<EventAction>();
			IEnumerable<EventAction> first = actions;
			EventAction.SubactionGroup failure = this.Failure;
			enumerable = ((failure != null) ? failure.Actions : null);
			return first.Concat(enumerable ?? Enumerable.Empty<EventAction>());
		}

		// Token: 0x06001DCA RID: 7626 RVA: 0x000D3248 File Offset: 0x000D1448
		public override bool IsFinished(ref string goTo)
		{
			return this.DetermineFinished(ref goTo);
		}

		// Token: 0x06001DCB RID: 7627 RVA: 0x000D3254 File Offset: 0x000D1454
		protected bool DetermineFinished()
		{
			string throwaway = null;
			return this.DetermineFinished(ref throwaway);
		}

		// Token: 0x06001DCC RID: 7628 RVA: 0x000D326C File Offset: 0x000D146C
		protected bool DetermineFinished(ref string goTo)
		{
			if (this.succeeded != null)
			{
				if (this.succeeded.Value)
				{
					if (this.Success == null || this.Success.IsFinished(ref goTo))
					{
						return true;
					}
				}
				else if (this.Failure == null || this.Failure.IsFinished(ref goTo))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001DCD RID: 7629 RVA: 0x000D32C4 File Offset: 0x000D14C4
		protected bool HasBeenDetermined()
		{
			return this.succeeded != null;
		}

		// Token: 0x06001DCE RID: 7630 RVA: 0x000D32D4 File Offset: 0x000D14D4
		public override bool SetGoToTarget(string goTo)
		{
			if (this.Success != null && this.Success.SetGoToTarget(goTo))
			{
				this.succeeded = new bool?(true);
				return true;
			}
			if (this.Failure != null && this.Failure.SetGoToTarget(goTo))
			{
				this.succeeded = new bool?(false);
				return true;
			}
			return false;
		}

		// Token: 0x06001DCF RID: 7631 RVA: 0x000D332A File Offset: 0x000D152A
		public override void Reset()
		{
			EventAction.SubactionGroup success = this.Success;
			if (success != null)
			{
				success.Reset();
			}
			EventAction.SubactionGroup failure = this.Failure;
			if (failure != null)
			{
				failure.Reset();
			}
			this.succeeded = null;
		}

		// Token: 0x06001DD0 RID: 7632 RVA: 0x000D335C File Offset: 0x000D155C
		public override void Update(float deltaTime)
		{
			if (this.succeeded == null)
			{
				this.succeeded = this.DetermineSuccess();
				return;
			}
			if (this.succeeded.Value)
			{
				EventAction.SubactionGroup success = this.Success;
				if (success == null)
				{
					return;
				}
				success.Update(deltaTime);
				return;
			}
			else
			{
				EventAction.SubactionGroup failure = this.Failure;
				if (failure == null)
				{
					return;
				}
				failure.Update(deltaTime);
				return;
			}
		}

		// Token: 0x06001DD1 RID: 7633
		protected abstract bool? DetermineSuccess();

		// Token: 0x04000E60 RID: 3680
		public EventAction.SubactionGroup Success;

		// Token: 0x04000E61 RID: 3681
		public EventAction.SubactionGroup Failure;

		// Token: 0x04000E62 RID: 3682
		protected bool? succeeded;
	}
}
