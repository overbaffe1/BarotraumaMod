using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200061C RID: 1564
	internal class RegExFindComponent : ItemComponent
	{
		// Token: 0x17001963 RID: 6499
		// (get) Token: 0x06006455 RID: 25685 RVA: 0x003406FD File Offset: 0x0033E8FD
		// (set) Token: 0x06006456 RID: 25686 RVA: 0x00340705 File Offset: 0x0033E905
		[Editable]
		[Serialize(200, IsPropertySaveable.No, "The maximum length of the output string. Warning: Large values can lead to large memory usage or networking issues.", "", false)]
		public int MaxOutputLength
		{
			get
			{
				return this.maxOutputLength;
			}
			set
			{
				this.maxOutputLength = Math.Max(value, 0);
			}
		}

		// Token: 0x17001964 RID: 6500
		// (get) Token: 0x06006457 RID: 25687 RVA: 0x00340714 File Offset: 0x0033E914
		// (set) Token: 0x06006458 RID: 25688 RVA: 0x0034071C File Offset: 0x0033E91C
		[InGameEditable]
		[Serialize("1", IsPropertySaveable.Yes, "The signal this item outputs when the received signal matches the regular expression.", "", true)]
		public string Output
		{
			get
			{
				return this.output;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.output = value;
				if (this.output.Length > this.MaxOutputLength && (this.item.Submarine == null || !this.item.Submarine.Loading))
				{
					this.output = this.output.Substring(0, this.MaxOutputLength);
				}
			}
		}

		// Token: 0x17001965 RID: 6501
		// (get) Token: 0x06006459 RID: 25689 RVA: 0x0034077E File Offset: 0x0033E97E
		// (set) Token: 0x0600645A RID: 25690 RVA: 0x00340786 File Offset: 0x0033E986
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component output a value of a capture group instead of a constant signal.", "", true)]
		public bool UseCaptureGroup { get; set; }

		// Token: 0x17001966 RID: 6502
		// (get) Token: 0x0600645B RID: 25691 RVA: 0x0034078F File Offset: 0x0033E98F
		// (set) Token: 0x0600645C RID: 25692 RVA: 0x00340797 File Offset: 0x0033E997
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component output the value of a capture group even if it's empty?", "", true)]
		public bool OutputEmptyCaptureGroup { get; set; }

		// Token: 0x17001967 RID: 6503
		// (get) Token: 0x0600645D RID: 25693 RVA: 0x003407A0 File Offset: 0x0033E9A0
		// (set) Token: 0x0600645E RID: 25694 RVA: 0x003407A8 File Offset: 0x0033E9A8
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal this item outputs when the received signal does not match the regular expression.", "", true)]
		public string FalseOutput { get; set; }

		// Token: 0x17001968 RID: 6504
		// (get) Token: 0x0600645F RID: 25695 RVA: 0x003407B1 File Offset: 0x0033E9B1
		// (set) Token: 0x06006460 RID: 25696 RVA: 0x003407B9 File Offset: 0x0033E9B9
		[InGameEditable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the component keep sending the output even after it stops receiving a signal, or only send an output when it receives a signal.", "", true)]
		public bool ContinuousOutput { get; set; }

		// Token: 0x17001969 RID: 6505
		// (get) Token: 0x06006461 RID: 25697 RVA: 0x003407C2 File Offset: 0x0033E9C2
		// (set) Token: 0x06006462 RID: 25698 RVA: 0x003407CC File Offset: 0x0033E9CC
		[InGameEditable]
		[Serialize("", IsPropertySaveable.Yes, "The regular expression used to check the incoming signals.", "", true)]
		public string Expression
		{
			get
			{
				return this.expression;
			}
			set
			{
				if (this.expression == value)
				{
					return;
				}
				this.expression = value;
				this.previousReceivedSignal = "";
				try
				{
					this.regex = new Regex(this.expression, RegexOptions.None, RegExFindComponent.timeout);
				}
				catch
				{
					return;
				}
				this.timedOut = false;
			}
		}

		// Token: 0x06006463 RID: 25699 RVA: 0x00340830 File Offset: 0x0033EA30
		public RegExFindComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.nonContinuousOutputSent = true;
			this.IsActive = true;
		}

		// Token: 0x06006464 RID: 25700 RVA: 0x00340854 File Offset: 0x0033EA54
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.timedOut)
			{
				this.item.SendSignal("TIMEOUT", "signal_out");
				return;
			}
			if (string.IsNullOrWhiteSpace(this.expression) || this.regex == null)
			{
				return;
			}
			if (!this.ContinuousOutput && this.nonContinuousOutputSent)
			{
				return;
			}
			if (this.receivedSignal != this.previousReceivedSignal && this.receivedSignal != null)
			{
				try
				{
					this.stopwatch.Restart();
					Match match = this.regex.Match(this.receivedSignal);
					this.stopwatch.Stop();
					if (this.stopwatch.Elapsed > RegExFindComponent.shortTimeout)
					{
						this.timeOutsInARow++;
						if (this.timeOutsInARow >= 3)
						{
							throw new RegexMatchTimeoutException();
						}
					}
					else
					{
						this.timeOutsInARow = 0;
					}
					this.previousResult = match.Success;
					this.previousGroups = ((this.UseCaptureGroup && this.previousResult) ? match.Groups : null);
					this.previousReceivedSignal = this.receivedSignal;
				}
				catch (Exception e)
				{
					if (e is RegexMatchTimeoutException)
					{
						this.timedOut = true;
					}
					else
					{
						this.item.SendSignal("ERROR", "signal_out");
					}
					this.previousResult = false;
					return;
				}
			}
			bool allowEmptyStringOutput = false;
			string signalOut;
			if (this.previousResult)
			{
				if (this.UseCaptureGroup)
				{
					Group group;
					if (this.previousGroups != null && this.previousGroups.TryGetValue(this.Output, out group))
					{
						signalOut = group.Value;
						allowEmptyStringOutput = this.OutputEmptyCaptureGroup;
					}
					else
					{
						signalOut = this.FalseOutput;
					}
				}
				else
				{
					signalOut = this.Output;
				}
			}
			else
			{
				signalOut = this.FalseOutput;
			}
			if (!string.IsNullOrEmpty(signalOut) || (allowEmptyStringOutput && signalOut == string.Empty))
			{
				this.item.SendSignal(signalOut, "signal_out");
			}
			if (!this.ContinuousOutput)
			{
				this.nonContinuousOutputSent = true;
			}
		}

		// Token: 0x06006465 RID: 25701 RVA: 0x00340A38 File Offset: 0x0033EC38
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_in")
			{
				this.receivedSignal = signal.value;
				this.nonContinuousOutputSent = false;
				return;
			}
			if (!(name == "set_output"))
			{
				return;
			}
			this.Output = signal.value;
		}

		// Token: 0x04003404 RID: 13316
		private static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(50.0);

		// Token: 0x04003405 RID: 13317
		private static readonly TimeSpan shortTimeout = TimeSpan.FromMilliseconds(1.0);

		// Token: 0x04003406 RID: 13318
		private readonly Stopwatch stopwatch = new Stopwatch();

		// Token: 0x04003407 RID: 13319
		private bool timedOut;

		// Token: 0x04003408 RID: 13320
		private int timeOutsInARow;

		// Token: 0x04003409 RID: 13321
		private const int MaxTimeOutsInARow = 3;

		// Token: 0x0400340A RID: 13322
		private string expression;

		// Token: 0x0400340B RID: 13323
		private string receivedSignal;

		// Token: 0x0400340C RID: 13324
		private string previousReceivedSignal;

		// Token: 0x0400340D RID: 13325
		private bool previousResult;

		// Token: 0x0400340E RID: 13326
		private GroupCollection previousGroups;

		// Token: 0x0400340F RID: 13327
		private Regex regex;

		// Token: 0x04003410 RID: 13328
		private bool nonContinuousOutputSent;

		// Token: 0x04003411 RID: 13329
		private int maxOutputLength;

		// Token: 0x04003412 RID: 13330
		private string output;
	}
}
