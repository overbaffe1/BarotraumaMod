using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000278 RID: 632
	internal abstract class BinaryOptionAction : EventAction
	{
		// Token: 0x06003894 RID: 14484 RVA: 0x00218700 File Offset: 0x00216900
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

		// Token: 0x06003895 RID: 14485 RVA: 0x002187AC File Offset: 0x002169AC
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

		// Token: 0x06003896 RID: 14486 RVA: 0x002187FC File Offset: 0x002169FC
		public override bool IsFinished(ref string goTo)
		{
			return this.DetermineFinished(ref goTo);
		}

		// Token: 0x06003897 RID: 14487 RVA: 0x00218808 File Offset: 0x00216A08
		protected bool DetermineFinished()
		{
			string throwaway = null;
			return this.DetermineFinished(ref throwaway);
		}

		// Token: 0x06003898 RID: 14488 RVA: 0x00218820 File Offset: 0x00216A20
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

		// Token: 0x06003899 RID: 14489 RVA: 0x00218878 File Offset: 0x00216A78
		protected bool HasBeenDetermined()
		{
			return this.succeeded != null;
		}

		// Token: 0x0600389A RID: 14490 RVA: 0x00218888 File Offset: 0x00216A88
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

		// Token: 0x0600389B RID: 14491 RVA: 0x002188DE File Offset: 0x00216ADE
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

		// Token: 0x0600389C RID: 14492 RVA: 0x00218910 File Offset: 0x00216B10
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

		// Token: 0x0600389D RID: 14493
		protected abstract bool? DetermineSuccess();

		// Token: 0x04001D57 RID: 7511
		public EventAction.SubactionGroup Success;

		// Token: 0x04001D58 RID: 7512
		public EventAction.SubactionGroup Failure;

		// Token: 0x04001D59 RID: 7513
		protected bool? succeeded;
	}
}
