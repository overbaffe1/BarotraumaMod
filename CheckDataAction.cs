using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200027C RID: 636
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckDataAction : BinaryOptionAction
	{
		// Token: 0x17000EE8 RID: 3816
		// (get) Token: 0x060038CF RID: 14543 RVA: 0x00219326 File Offset: 0x00217526
		// (set) Token: 0x060038D0 RID: 14544 RVA: 0x0021932E File Offset: 0x0021752E
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the data to check.", "", false)]
		public Identifier Identifier { get; set; } = Identifier.Empty;

		// Token: 0x17000EE9 RID: 3817
		// (get) Token: 0x060038D1 RID: 14545 RVA: 0x00219337 File Offset: 0x00217537
		// (set) Token: 0x060038D2 RID: 14546 RVA: 0x0021933F File Offset: 0x0021753F
		[Serialize("", IsPropertySaveable.Yes, "The condition that must be met for the check to succeed. Uses the same formatting as conditionals (for example, \"gt 5.2\", \"true\", \"lt 10\".)", "", false)]
		public string Condition { get; set; } = "";

		// Token: 0x17000EEA RID: 3818
		// (get) Token: 0x060038D3 RID: 14547 RVA: 0x00219348 File Offset: 0x00217548
		// (set) Token: 0x060038D4 RID: 14548 RVA: 0x00219350 File Offset: 0x00217550
		[Serialize(false, IsPropertySaveable.Yes, "Forces the comparison to use string instead of attempting to parse it as a boolean or a float first. Use this if you know the value is a string.", "", false)]
		public bool ForceString { get; set; }

		// Token: 0x17000EEB RID: 3819
		// (get) Token: 0x060038D5 RID: 14549 RVA: 0x00219359 File Offset: 0x00217559
		// (set) Token: 0x060038D6 RID: 14550 RVA: 0x00219361 File Offset: 0x00217561
		[Serialize(false, IsPropertySaveable.Yes, "Performs the comparison against a metadata by identifier instead of a constant value. Meaning that you could for example check whether the value of \"progress_of_some_event\" is larger than \"progress_of_some_other_event\".", "", false)]
		public bool CheckAgainstMetadata { get; set; }

		// Token: 0x17000EEC RID: 3820
		// (get) Token: 0x060038D7 RID: 14551 RVA: 0x0021936A File Offset: 0x0021756A
		// (set) Token: 0x060038D8 RID: 14552 RVA: 0x00219372 File Offset: 0x00217572
		protected PropertyConditional.ComparisonOperatorType Operator { get; set; }

		// Token: 0x060038D9 RID: 14553 RVA: 0x0021937C File Offset: 0x0021757C
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

		// Token: 0x060038DA RID: 14554 RVA: 0x00219440 File Offset: 0x00217640
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

		// Token: 0x060038DB RID: 14555 RVA: 0x002194F4 File Offset: 0x002176F4
		public bool GetSuccess()
		{
			return this.DetermineSuccess().GetValueOrDefault();
		}

		// Token: 0x060038DC RID: 14556 RVA: 0x00219510 File Offset: 0x00217710
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

		// Token: 0x060038DD RID: 14557 RVA: 0x00219720 File Offset: 0x00217920
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

		// Token: 0x060038DE RID: 14558 RVA: 0x00219760 File Offset: 0x00217960
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

		// Token: 0x060038DF RID: 14559 RVA: 0x00219804 File Offset: 0x00217A04
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

		// Token: 0x060038E0 RID: 14560 RVA: 0x0021984D File Offset: 0x00217A4D
		private bool? TryString(CampaignMode campaignMode, string value)
		{
			return this.CompareString(this.GetString(campaignMode), value);
		}

		// Token: 0x060038E1 RID: 14561 RVA: 0x00219860 File Offset: 0x00217A60
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

		// Token: 0x060038E2 RID: 14562 RVA: 0x00219900 File Offset: 0x00217B00
		protected virtual bool GetBool(CampaignMode campaignMode)
		{
			return campaignMode.CampaignMetadata.GetBoolean(this.Identifier, null);
		}

		// Token: 0x060038E3 RID: 14563 RVA: 0x00219928 File Offset: 0x00217B28
		protected virtual float GetFloat(CampaignMode campaignMode)
		{
			return campaignMode.CampaignMetadata.GetFloat(this.Identifier, null);
		}

		// Token: 0x060038E4 RID: 14564 RVA: 0x0021994F File Offset: 0x00217B4F
		private string GetString(CampaignMode campaignMode)
		{
			return campaignMode.CampaignMetadata.GetString(this.Identifier, null);
		}

		// Token: 0x060038E5 RID: 14565 RVA: 0x00219964 File Offset: 0x00217B64
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

		// Token: 0x04001D70 RID: 7536
		[Nullable(2)]
		protected object value2;

		// Token: 0x04001D71 RID: 7537
		[Nullable(2)]
		protected object value1;
	}
}
