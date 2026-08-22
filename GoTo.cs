using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000293 RID: 659
	internal class GoTo : EventAction
	{
		// Token: 0x17000F3D RID: 3901
		// (get) Token: 0x060039EE RID: 14830 RVA: 0x0021D71D File Offset: 0x0021B91D
		// (set) Token: 0x060039EF RID: 14831 RVA: 0x0021D725 File Offset: 0x0021B925
		[Serialize("", IsPropertySaveable.Yes, "Name of the label to jump to.", "", false)]
		public string Name { get; set; }

		// Token: 0x17000F3E RID: 3902
		// (get) Token: 0x060039F0 RID: 14832 RVA: 0x0021D72E File Offset: 0x0021B92E
		// (set) Token: 0x060039F1 RID: 14833 RVA: 0x0021D736 File Offset: 0x0021B936
		[Serialize(-1, IsPropertySaveable.Yes, "How many times can this GoTo action be repeated? Can be used to make some parts of an event repeat a limited number of times. If negative or zero, there's no limit.", "", false)]
		public int MaxTimes { get; set; }

		// Token: 0x17000F3F RID: 3903
		// (get) Token: 0x060039F2 RID: 14834 RVA: 0x0021D73F File Offset: 0x0021B93F
		// (set) Token: 0x060039F3 RID: 14835 RVA: 0x0021D747 File Offset: 0x0021B947
		[Serialize(true, IsPropertySaveable.Yes, "By default, jumping to another part in the event closes the active conversation prompt. Use this if if you want to keep it open instead.", "", false)]
		public bool EndConversation { get; set; }

		// Token: 0x060039F4 RID: 14836 RVA: 0x0021D750 File Offset: 0x0021B950
		public GoTo(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060039F5 RID: 14837 RVA: 0x0021D75A File Offset: 0x0021B95A
		public override bool IsFinished(ref string goTo)
		{
			if (this.counter < this.MaxTimes || this.MaxTimes <= 0)
			{
				goTo = this.Name;
				this.counter++;
			}
			return true;
		}

		// Token: 0x060039F6 RID: 14838 RVA: 0x0021D78C File Offset: 0x0021B98C
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

		// Token: 0x060039F7 RID: 14839 RVA: 0x0021D80E File Offset: 0x0021BA0E
		public override void Reset()
		{
		}

		// Token: 0x04001DD7 RID: 7639
		private int counter;
	}
}
