using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002B2 RID: 690
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class PropertyReference
	{
		// Token: 0x17000DC0 RID: 3520
		// (get) Token: 0x06002F63 RID: 12131 RVA: 0x0013B162 File Offset: 0x00139362
		// (set) Token: 0x06002F64 RID: 12132 RVA: 0x0013B16A File Offset: 0x0013936A
		[Nullable(2)]
		public object OriginalValue { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x06002F65 RID: 12133 RVA: 0x0013B173 File Offset: 0x00139373
		private PropertyReference(Identifier name, string multiplier, Upgrade upgrade)
		{
			this.Name = name;
			this.Multiplier = multiplier;
			this.upgrade = upgrade;
		}

		// Token: 0x06002F66 RID: 12134 RVA: 0x0013B190 File Offset: 0x00139390
		public void SetOriginalValue(object value)
		{
			if (this.OriginalValue == null)
			{
				this.OriginalValue = value;
			}
		}

		// Token: 0x06002F67 RID: 12135 RVA: 0x0013B1B0 File Offset: 0x001393B0
		public object CalculateUpgrade(int level)
		{
			object originalValue = this.OriginalValue;
			if (!(originalValue is float) && !(originalValue is int) && !(originalValue is double))
			{
				if (originalValue is bool)
				{
					bool result;
					if (bool.TryParse(this.Multiplier, out result))
					{
						return result;
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(108, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Original value of \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" in the upgrade \"");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.upgrade.Prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" is not a integer, float, double or boolean but ");
				object originalValue2 = this.OriginalValue;
				defaultInterpolatedStringHandler.AppendFormatted<Type>((originalValue2 != null) ? originalValue2.GetType() : null);
				defaultInterpolatedStringHandler.AppendLiteral(" with a value of (");
				defaultInterpolatedStringHandler.AppendFormatted<object>(this.OriginalValue);
				defaultInterpolatedStringHandler.AppendLiteral("). \n");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "The value has been assumed to be '0', did you forget a Convert.ChangeType()?", null);
				return 0;
			}
			float value = Convert.ToSingle(this.OriginalValue);
			return (level == 0) ? value : PropertyReference.CalculateUpgrade(value, level, this.Multiplier);
		}

		// Token: 0x06002F68 RID: 12136 RVA: 0x0013B2CE File Offset: 0x001394CE
		public static float CalculateUpgrade(float value, int level, string multiplier)
		{
			if (multiplier[multiplier.Length - 1] != '%')
			{
				return PropertyReference.CalculateUpgradeFloat(multiplier, value, level);
			}
			return PropertyReference.ApplyPercentage(value, (float)UpgradePrefab.ParsePercentage(multiplier, Identifier.Empty, null, true), level);
		}

		// Token: 0x06002F69 RID: 12137 RVA: 0x0013B300 File Offset: 0x00139500
		private static float CalculateUpgradeFloat(string multiplier, float value, int level)
		{
			float multiplierFloat = PropertyReference.ParseValue(multiplier, value);
			char c = multiplier[0];
			switch (c)
			{
			case '*':
				break;
			case '+':
				return value + multiplierFloat * (float)level;
			case ',':
			case '.':
				goto IL_5E;
			case '-':
				return value - multiplierFloat * (float)level;
			case '/':
				return value / (multiplierFloat * (float)level);
			default:
				if (c == '=')
				{
					return multiplierFloat;
				}
				if (c != 'x')
				{
					goto IL_5E;
				}
				break;
			}
			return value * (multiplierFloat * (float)level);
			IL_5E:
			return 0f;
		}

		// Token: 0x06002F6A RID: 12138 RVA: 0x0013B370 File Offset: 0x00139570
		[NullableContext(2)]
		public void ApplySavedValue(XElement savedElement)
		{
			if (savedElement == null)
			{
				return;
			}
			foreach (XElement savedValue in savedElement.Elements())
			{
				Identifier identifier = savedValue.NameAsIdentifier();
				if (identifier == this.Name)
				{
					string value = savedValue.GetAttributeString("value", string.Empty);
					float floatValue;
					bool boolValue;
					if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out floatValue))
					{
						this.OriginalValue = floatValue;
					}
					else if (bool.TryParse(value, out boolValue))
					{
						this.OriginalValue = boolValue;
					}
					else
					{
						this.OriginalValue = value;
					}
				}
			}
		}

		// Token: 0x06002F6B RID: 12139 RVA: 0x0013B424 File Offset: 0x00139624
		private static float ApplyPercentage(float value, float amount, int times)
		{
			return (1f + amount / 100f * (float)times) * value;
		}

		// Token: 0x06002F6C RID: 12140 RVA: 0x0013B438 File Offset: 0x00139638
		public static PropertyReference[] ParseAttributes(IEnumerable<XAttribute> attributes, Upgrade upgrade)
		{
			return (from attribute in attributes
			select new PropertyReference(attribute.NameAsIdentifier(), attribute.Value, upgrade)).ToArray<PropertyReference>();
		}

		// Token: 0x06002F6D RID: 12141 RVA: 0x0013B46C File Offset: 0x0013966C
		private static float ParseValue(string multiplier, [Nullable(2)] object originalValue)
		{
			if (multiplier.Length > 1 && PropertyReference.prefixCharacters.Contains(multiplier[0]))
			{
				float value;
				if (float.TryParse(multiplier.Substring(1).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
				{
					return value;
				}
				if (originalValue is float || originalValue is int || originalValue is double)
				{
					return (float)originalValue;
				}
			}
			return 1f;
		}

		// Token: 0x040017C6 RID: 6086
		public readonly Identifier Name;

		// Token: 0x040017C7 RID: 6087
		private readonly string Multiplier;

		// Token: 0x040017C8 RID: 6088
		private static readonly char[] prefixCharacters = new char[]
		{
			'=',
			'/',
			'*',
			'x',
			'-',
			'+'
		};

		// Token: 0x040017C9 RID: 6089
		private readonly Upgrade upgrade;
	}
}
