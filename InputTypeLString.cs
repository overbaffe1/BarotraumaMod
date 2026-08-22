using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000368 RID: 872
	[NullableContext(1)]
	[Nullable(0)]
	public class InputTypeLString : LocalizedString
	{
		// Token: 0x06004321 RID: 17185 RVA: 0x002525CC File Offset: 0x002507CC
		public InputTypeLString(LocalizedString nStr, bool useColorHighlight = false)
		{
			this.nestedStr = nStr;
			this.useColorHighlight = useColorHighlight;
		}

		// Token: 0x06004322 RID: 17186 RVA: 0x002525E2 File Offset: 0x002507E2
		protected override bool MustRetrieveValue()
		{
			return base.MustRetrieveValue();
		}

		// Token: 0x1700119D RID: 4509
		// (get) Token: 0x06004323 RID: 17187 RVA: 0x002525EA File Offset: 0x002507EA
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06004324 RID: 17188 RVA: 0x002525F8 File Offset: 0x002507F8
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value;
			foreach (object obj in Enum.GetValues(typeof(InputType)))
			{
				InputType? inputType = (InputType?)obj;
				if (inputType != null)
				{
					GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
					string keyBindText = keyMap.KeyBindText(inputType.Value).Value;
					if (this.useColorHighlight)
					{
						keyBindText = "‖color:gui.orange‖" + keyBindText + "‖end‖";
					}
					string cachedValue = this.cachedValue;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
					defaultInterpolatedStringHandler.AppendLiteral("[");
					defaultInterpolatedStringHandler.AppendFormatted<InputType?>(inputType);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					this.cachedValue = cachedValue.Replace(defaultInterpolatedStringHandler.ToStringAndClear(), keyBindText, StringComparison.OrdinalIgnoreCase);
					string cachedValue2 = this.cachedValue;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("[InputType.");
					defaultInterpolatedStringHandler2.AppendFormatted<InputType?>(inputType);
					defaultInterpolatedStringHandler2.AppendLiteral("]");
					this.cachedValue = cachedValue2.Replace(defaultInterpolatedStringHandler2.ToStringAndClear(), keyBindText, StringComparison.OrdinalIgnoreCase);
				}
			}
			base.UpdateLanguage();
		}

		// Token: 0x04002343 RID: 9027
		private readonly LocalizedString nestedStr;

		// Token: 0x04002344 RID: 9028
		private bool useColorHighlight;
	}
}
