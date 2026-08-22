using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001A0 RID: 416
	internal class GoTo : EventAction
	{
		// Token: 0x170008E9 RID: 2281
		// (get) Token: 0x06001F24 RID: 7972 RVA: 0x000D8269 File Offset: 0x000D6469
		// (set) Token: 0x06001F25 RID: 7973 RVA: 0x000D8271 File Offset: 0x000D6471
		[Serialize("", IsPropertySaveable.Yes, "Name of the label to jump to.", "", false)]
		public string Name { get; set; }

		// Token: 0x170008EA RID: 2282
		// (get) Token: 0x06001F26 RID: 7974 RVA: 0x000D827A File Offset: 0x000D647A
		// (set) Token: 0x06001F27 RID: 7975 RVA: 0x000D8282 File Offset: 0x000D6482
		[Serialize(-1, IsPropertySaveable.Yes, "How many times can this GoTo action be repeated? Can be used to make some parts of an event repeat a limited number of times. If negative or zero, there's no limit.", "", false)]
		public int MaxTimes { get; set; }

		// Token: 0x170008EB RID: 2283
		// (get) Token: 0x06001F28 RID: 7976 RVA: 0x000D828B File Offset: 0x000D648B
		// (set) Token: 0x06001F29 RID: 7977 RVA: 0x000D8293 File Offset: 0x000D6493
		[Serialize(true, IsPropertySaveable.Yes, "By default, jumping to another part in the event closes the active conversation prompt. Use this if if you want to keep it open instead.", "", false)]
		public bool EndConversation { get; set; }

		// Token: 0x06001F2A RID: 7978 RVA: 0x000D829C File Offset: 0x000D649C
		public GoTo(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001F2B RID: 7979 RVA: 0x000D82A6 File Offset: 0x000D64A6
		public override bool IsFinished(ref string goTo)
		{
			if (this.counter < this.MaxTimes || this.MaxTimes <= 0)
			{
				goTo = this.Name;
				this.counter++;
			}
			return true;
		}

		// Token: 0x06001F2C RID: 7980 RVA: 0x000D82D8 File Offset: 0x000D64D8
		public override string ToDebugString()
		{
			string msg = "[-] Go to label \"" + this.Name + "\"";
			if (this.MaxTimes > 0)
			{
				string str = msg;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 2);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.counter);
				defaultInterpolatedStringHandler.AppendLiteral("/");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.MaxTimes);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				msg = str + defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return msg;
		}

		// Token: 0x06001F2D RID: 7981 RVA: 0x000D835A File Offset: 0x000D655A
		public override void Reset()
		{
		}

		// Token: 0x04000EE0 RID: 3808
		private int counter;
	}
}
