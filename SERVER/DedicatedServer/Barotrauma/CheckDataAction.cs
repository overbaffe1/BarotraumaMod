using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000188 RID: 392
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckDataAction : BinaryOptionAction
	{
		// Token: 0x17000894 RID: 2196
		// (get) Token: 0x06001E03 RID: 7683 RVA: 0x000D3D72 File Offset: 0x000D1F72
		// (set) Token: 0x06001E04 RID: 7684 RVA: 0x000D3D7A File Offset: 0x000D1F7A
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the data to check.", "", false)]
		public Identifier Identifier { get; set; } = Identifier.Empty;

		// Token: 0x17000895 RID: 2197
		// (get) Token: 0x06001E05 RID: 7685 RVA: 0x000D3D83 File Offset: 0x000D1F83
		// (set) Token: 0x06001E06 RID: 7686 RVA: 0x000D3D8B File Offset: 0x000D1F8B
		[Serialize("", IsPropertySaveable.Yes, "The condition that must be met for the check to succeed. Uses the same formatting as conditionals (for example, \"gt 5.2\", \"true\", \"lt 10\".)", "", false)]
		public string Condition { get; set; } = "";

		// Token: 0x17000896 RID: 2198
		// (get) Token: 0x06001E07 RID: 7687 RVA: 0x000D3D94 File Offset: 0x000D1F94
		// (set) Token: 0x06001E08 RID: 7688 RVA: 0x000D3D9C File Offset: 0x000D1F9C
		[Serialize(false, IsPropertySaveable.Yes, "Forces the comparison to use string instead of attempting to parse it as a boolean or a float first. Use this if you know the value is a string.", "", false)]
		public bool ForceString { get; set; }

		// Token: 0x17000897 RID: 2199
		// (get) Token: 0x06001E09 RID: 7689 RVA: 0x000D3DA5 File Offset: 0x000D1FA5
		// (set) Token: 0x06001E0A RID: 7690 RVA: 0x000D3DAD File Offset: 0x000D1FAD
		[Serialize(false, IsPropertySaveable.Yes, "Performs the comparison against a metadata by identifier instead of a constant value. Meaning that you could for example check whether the value of \"progress_of_some_event\" is larger than \"progress_of_some_other_event\".", "", false)]
		public bool CheckAgainstMetadata { get; set; }

		// Token: 0x17000898 RID: 2200
		// (get) Token: 0x06001E0B RID: 7691 RVA: 0x000D3DB6 File Offset: 0x000D1FB6
		// (set) Token: 0x06001E0C RID: 7692 RVA: 0x000D3DBE File Offset: 0x000D1FBE
		protected PropertyConditional.ComparisonOperatorType Operator { get; set; }

		// Token: 0x06001E0D RID: 7693 RVA: 0x000D3DC8 File Offset: 0x000D1FC8
		public CheckDataAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (string.IsNullOrEmpty(this.Condition))
			{
				this.Condition = element.GetAttributeString("value", string.Empty);
				if (string.IsNullOrEmpty(this.Condition))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in scripted event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". CheckDataAction with no condition set (");
					defaultInterpolatedStringHandler.AppendFormatted<ContentXElement>(element);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, (element != null) ? element.ContentPackage : null, false, false);
				}
			}
		}

		// Token: 0x06001E0E RID: 7694 RVA: 0x000D3E8C File Offset: 0x000D208C
		public CheckDataAction(ContentXElement element, string parentDebugString) : base(null, element)
		{
			if (string.IsNullOrEmpty(this.Condition))
			{
				this.Condition = element.GetAttributeString("value", string.Empty);
				if (string.IsNullOrEmpty(this.Condition))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in scripted event \"");
					defaultInterpolatedStringHandler.AppendFormatted(parentDebugString);
					defaultInterpolatedStringHandler.AppendLiteral("\". CheckDataAction with no condition set (");
					defaultInterpolatedStringHandler.AppendFormatted<ContentXElement>(element);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, (element != null) ? element.ContentPackage : null, false, false);
				}
			}
		}

		// Token: 0x06001E0F RID: 7695 RVA: 0x000D3F40 File Offset: 0x000D2140
		public bool GetSuccess()
		{
			return this.DetermineSuccess().GetValueOrDefault();
		}

		// Token: 0x06001E10 RID: 7696 RVA: 0x000D3F5C File Offset: 0x000D215C
		protected override bool? DetermineSuccess()
		{
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaignMode = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			if (campaignMode == null)
			{
				return new bool?(false);
			}
			ValueTuple<PropertyConditional.ComparisonOperatorType, string> valueTuple = PropertyConditional.ExtractComparisonOperatorFromConditionString(this.Condition);
			this.Operator = valueTuple.Item1;
			string value = valueTuple.Item2;
			if (this.Operator == PropertyConditional.ComparisonOperatorType.None)
			{
				string error = this.Condition + " is invalid, it should start with an operator followed by a boolean or a floating point value.";
				Exception e = null;
				ScriptedEvent parentEvent = this.ParentEvent;
				ContentPackage contentPackage;
				if (parentEvent == null)
				{
					contentPackage = null;
				}
				else
				{
					EventPrefab prefab = parentEvent.Prefab;
					contentPackage = ((prefab != null) ? prefab.ContentPackage : null);
				}
				DebugConsole.ThrowError(error, e, contentPackage, false, false);
				return new bool?(false);
			}
			if (this.CheckAgainstMetadata)
			{
				object metadata = campaignMode.CampaignMetadata.GetValue(this.Identifier);
				object metadata2 = campaignMode.CampaignMetadata.GetValue(value.ToIdentifier());
				if (metadata == null || metadata2 == null)
				{
					PropertyConditional.ComparisonOperatorType @operator = this.Operator;
					bool value2;
					if (@operator != PropertyConditional.ComparisonOperatorType.Equals)
					{
						value2 = (@operator == PropertyConditional.ComparisonOperatorType.NotEquals && metadata != metadata2);
					}
					else
					{
						value2 = (metadata == metadata2);
					}
					return new bool?(value2);
				}
				if (!this.ForceString)
				{
					object obj = metadata;
					if (obj is bool)
					{
						bool @bool = (bool)obj;
						if (metadata2 is bool)
						{
							bool bool2 = (bool)metadata2;
							return new bool?(this.CompareBool(@bool, bool2).GetValueOrDefault());
						}
					}
					else if (obj is float)
					{
						float @float = (float)obj;
						if (metadata2 is float)
						{
							float float2 = (float)metadata2;
							return new bool?(PropertyConditional.CompareFloat(@float, float2, this.Operator));
						}
					}
				}
				string @string = metadata as string;
				if (@string != null)
				{
					string string2 = metadata2 as string;
					if (string2 != null)
					{
						return new bool?(this.CompareString(@string, string2).GetValueOrDefault());
					}
				}
				return new bool?(false);
			}
			else
			{
				if (!this.ForceString)
				{
					bool? tryBoolean = this.TryBoolean(campaignMode, value);
					if (tryBoolean != null)
					{
						return tryBoolean;
					}
					bool? tryFloat = this.TryFloat(campaignMode, value);
					if (tryFloat != null)
					{
						return tryFloat;
					}
				}
				bool? tryString = this.TryString(campaignMode, value);
				if (tryString != null)
				{
					return tryString;
				}
				return new bool?(false);
			}
		}

		// Token: 0x06001E11 RID: 7697 RVA: 0x000D416C File Offset: 0x000D236C
		private bool? TryBoolean(CampaignMode campaignMode, string value)
		{
			bool b;
			if (bool.TryParse(value, out b))
			{
				return this.CompareBool(this.GetBool(campaignMode), b);
			}
			DebugConsole.Log(value + " != bool");
			return null;
		}

		// Token: 0x06001E12 RID: 7698 RVA: 0x000D41AC File Offset: 0x000D23AC
		private bool? CompareBool(bool val1, bool val2)
		{
			this.value1 = val1;
			this.value2 = val2;
			PropertyConditional.ComparisonOperatorType @operator = this.Operator;
			if (@operator == PropertyConditional.ComparisonOperatorType.Equals)
			{
				return new bool?(val1 == val2);
			}
			if (@operator != PropertyConditional.ComparisonOperatorType.NotEquals)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Only \"Equals\" and \"Not equals\" operators are allowed for a boolean (was ");
				defaultInterpolatedStringHandler.AppendFormatted<PropertyConditional.ComparisonOperatorType>(this.Operator);
				defaultInterpolatedStringHandler.AppendLiteral(" for ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(val2);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				return new bool?(false);
			}
			return new bool?(val1 != val2);
		}

		// Token: 0x06001E13 RID: 7699 RVA: 0x000D4250 File Offset: 0x000D2450
		private bool? TryFloat(CampaignMode campaignMode, string value)
		{
			float f;
			if (float.TryParse(value, out f))
			{
				return new bool?(PropertyConditional.CompareFloat(this.GetFloat(campaignMode), f, this.Operator));
			}
			DebugConsole.Log(value + " != float");
			return null;
		}

		// Token: 0x06001E14 RID: 7700 RVA: 0x000D4299 File Offset: 0x000D2499
		private bool? TryString(CampaignMode campaignMode, string value)
		{
			return this.CompareString(this.GetString(campaignMode), value);
		}

		// Token: 0x06001E15 RID: 7701 RVA: 0x000D42AC File Offset: 0x000D24AC
		private bool? CompareString(string val1, string val2)
		{
			this.value1 = val1;
			this.value2 = val2;
			bool equals = string.Equals(val1, val2, StringComparison.OrdinalIgnoreCase);
			PropertyConditional.ComparisonOperatorType @operator = this.Operator;
			if (@operator == PropertyConditional.ComparisonOperatorType.Equals)
			{
				return new bool?(equals);
			}
			if (@operator != PropertyConditional.ComparisonOperatorType.NotEquals)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(78, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Only \"Equals\" and \"Not equals\" operators are allowed for a string (was ");
				defaultInterpolatedStringHandler.AppendFormatted<PropertyConditional.ComparisonOperatorType>(this.Operator);
				defaultInterpolatedStringHandler.AppendLiteral(" for ");
				defaultInterpolatedStringHandler.AppendFormatted(val2);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				return null;
			}
			return new bool?(!equals);
		}

		// Token: 0x06001E16 RID: 7702 RVA: 0x000D434C File Offset: 0x000D254C
		protected virtual bool GetBool(CampaignMode campaignMode)
		{
			return campaignMode.CampaignMetadata.GetBoolean(this.Identifier, null);
		}

		// Token: 0x06001E17 RID: 7703 RVA: 0x000D4374 File Offset: 0x000D2574
		protected virtual float GetFloat(CampaignMode campaignMode)
		{
			return campaignMode.CampaignMetadata.GetFloat(this.Identifier, null);
		}

		// Token: 0x06001E18 RID: 7704 RVA: 0x000D439B File Offset: 0x000D259B
		private string GetString(CampaignMode campaignMode)
		{
			return campaignMode.CampaignMetadata.GetString(this.Identifier, null);
		}

		// Token: 0x06001E19 RID: 7705 RVA: 0x000D43B0 File Offset: 0x000D25B0
		public override string ToDebugString()
		{
			string condition = "?";
			if (this.value2 != null && this.value1 != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler.AppendFormatted(this.value1.ColorizeObject());
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(this.Operator.ColorizeObject());
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(this.value2.ColorizeObject());
				condition = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else if (!this.Identifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral(" ");
				defaultInterpolatedStringHandler2.AppendFormatted(this.Condition);
				condition = defaultInterpolatedStringHandler2.ToStringAndClear().ColorizeObject();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(38, 5);
			defaultInterpolatedStringHandler3.AppendFormatted(ToolBox.GetDebugSymbol(this.succeeded != null, false));
			defaultInterpolatedStringHandler3.AppendLiteral(" ");
			defaultInterpolatedStringHandler3.AppendFormatted("CheckDataAction");
			defaultInterpolatedStringHandler3.AppendLiteral(" -> (Data: ");
			defaultInterpolatedStringHandler3.AppendFormatted(this.Identifier.ColorizeObject());
			defaultInterpolatedStringHandler3.AppendLiteral(", Success: ");
			defaultInterpolatedStringHandler3.AppendFormatted(this.succeeded.ColorizeObject());
			defaultInterpolatedStringHandler3.AppendLiteral(", Expression: ");
			defaultInterpolatedStringHandler3.AppendFormatted(condition);
			defaultInterpolatedStringHandler3.AppendLiteral(")");
			return defaultInterpolatedStringHandler3.ToStringAndClear();
		}

		// Token: 0x04000E79 RID: 3705
		[Nullable(2)]
		protected object value2;

		// Token: 0x04000E7A RID: 3706
		[Nullable(2)]
		protected object value1;
	}
}
