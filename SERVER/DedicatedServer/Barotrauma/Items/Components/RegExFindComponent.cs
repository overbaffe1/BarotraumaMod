using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F9 RID: 1273
	internal class RegExFindComponent : ItemComponent
	{
		// Token: 0x17001334 RID: 4916
		// (get) Token: 0x06004785 RID: 18309 RVA: 0x001C7A31 File Offset: 0x001C5C31
		// (set) Token: 0x06004786 RID: 18310 RVA: 0x001C7A39 File Offset: 0x001C5C39
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

		// Token: 0x17001335 RID: 4917
		// (get) Token: 0x06004787 RID: 18311 RVA: 0x001C7A48 File Offset: 0x001C5C48
		// (set) Token: 0x06004788 RID: 18312 RVA: 0x001C7A50 File Offset: 0x001C5C50
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

		// Token: 0x17001336 RID: 4918
		// (get) Token: 0x06004789 RID: 18313 RVA: 0x001C7AB2 File Offset: 0x001C5CB2
		// (set) Token: 0x0600478A RID: 18314 RVA: 0x001C7ABA File Offset: 0x001C5CBA
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component output a value of a capture group instead of a constant signal.", "", true)]
		public bool UseCaptureGroup { get; set; }

		// Token: 0x17001337 RID: 4919
		// (get) Token: 0x0600478B RID: 18315 RVA: 0x001C7AC3 File Offset: 0x001C5CC3
		// (set) Token: 0x0600478C RID: 18316 RVA: 0x001C7ACB File Offset: 0x001C5CCB
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the component output the value of a capture group even if it's empty?", "", true)]
		public bool OutputEmptyCaptureGroup { get; set; }

		// Token: 0x17001338 RID: 4920
		// (get) Token: 0x0600478D RID: 18317 RVA: 0x001C7AD4 File Offset: 0x001C5CD4
		// (set) Token: 0x0600478E RID: 18318 RVA: 0x001C7ADC File Offset: 0x001C5CDC
		[InGameEditable]
		[Serialize("0", IsPropertySaveable.Yes, "The signal this item outputs when the received signal does not match the regular expression.", "", true)]
		public string FalseOutput { get; set; }

		// Token: 0x17001339 RID: 4921
		// (get) Token: 0x0600478F RID: 18319 RVA: 0x001C7AE5 File Offset: 0x001C5CE5
		// (set) Token: 0x06004790 RID: 18320 RVA: 0x001C7AED File Offset: 0x001C5CED
		[InGameEditable]
		[Serialize(true, IsPropertySaveable.Yes, "Should the component keep sending the output even after it stops receiving a signal, or only send an output when it receives a signal.", "", true)]
		public bool ContinuousOutput { get; set; }

		// Token: 0x1700133A RID: 4922
		// (get) Token: 0x06004791 RID: 18321 RVA: 0x001C7AF6 File Offset: 0x001C5CF6
		// (set) Token: 0x06004792 RID: 18322 RVA: 0x001C7B00 File Offset: 0x001C5D00
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

		// Token: 0x06004793 RID: 18323 RVA: 0x001C7B64 File Offset: 0x001C5D64
		public RegExFindComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.nonContinuousOutputSent = true;
			this.IsActive = true;
		}

		// Token: 0x06004794 RID: 18324 RVA: 0x001C7B88 File Offset: 0x001C5D88
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

		// Token: 0x06004795 RID: 18325 RVA: 0x001C7D6C File Offset: 0x001C5F6C
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

		// Token: 0x04002280 RID: 8832
		private static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(50.0);

		// Token: 0x04002281 RID: 8833
		private static readonly TimeSpan shortTimeout = TimeSpan.FromMilliseconds(1.0);

		// Token: 0x04002282 RID: 8834
		private readonly Stopwatch stopwatch = new Stopwatch();

		// Token: 0x04002283 RID: 8835
		private bool timedOut;

		// Token: 0x04002284 RID: 8836
		private int timeOutsInARow;

		// Token: 0x04002285 RID: 8837
		private const int MaxTimeOutsInARow = 3;

		// Token: 0x04002286 RID: 8838
		private string expression;

		// Token: 0x04002287 RID: 8839
		private string receivedSignal;

		// Token: 0x04002288 RID: 8840
		private string previousReceivedSignal;

		// Token: 0x04002289 RID: 8841
		private bool previousResult;

		// Token: 0x0400228A RID: 8842
		private GroupCollection previousGroups;

		// Token: 0x0400228B RID: 8843
		private Regex regex;

		// Token: 0x0400228C RID: 8844
		private bool nonContinuousOutputSent;

		// Token: 0x0400228D RID: 8845
		private int maxOutputLength;

		// Token: 0x0400228E RID: 8846
		private string output;
	}
}
