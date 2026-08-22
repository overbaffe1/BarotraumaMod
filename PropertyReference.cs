using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200037E RID: 894
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class PropertyReference
	{
		// Token: 0x170011C3 RID: 4547
		// (get) Token: 0x060043FB RID: 17403 RVA: 0x002556A6 File Offset: 0x002538A6
		// (set) Token: 0x060043FC RID: 17404 RVA: 0x002556AE File Offset: 0x002538AE
		[Nullable(2)]
		public object OriginalValue { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x060043FD RID: 17405 RVA: 0x002556B7 File Offset: 0x002538B7
		private PropertyReference(Identifier name, string multiplier, Upgrade upgrade)
		{
			this.Name = name;
			this.Multiplier = multiplier;
			this.upgrade = upgrade;
		}

		// Token: 0x060043FE RID: 17406 RVA: 0x002556D4 File Offset: 0x002538D4
		public void SetOriginalValue(object value)
		{
			if (this.OriginalValue == null)
			{
				this.OriginalValue = value;
			}
		}

		// Token: 0x060043FF RID: 17407 RVA: 0x002556F4 File Offset: 0x002538F4
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

		// Token: 0x06004400 RID: 17408 RVA: 0x00255812 File Offset: 0x00253A12
		public static float CalculateUpgrade(float value, int level, string multiplier)
		{
			if (multiplier[multiplier.Length - 1] != '%')
			{
				return PropertyReference.CalculateUpgradeFloat(multiplier, value, level);
			}
			return PropertyReference.ApplyPercentage(value, (float)UpgradePrefab.ParsePercentage(multiplier, Identifier.Empty, null, true), level);
		}

		// Token: 0x06004401 RID: 17409 RVA: 0x00255844 File Offset: 0x00253A44
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

		// Token: 0x06004402 RID: 17410 RVA: 0x002558B4 File Offset: 0x00253AB4
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

		// Token: 0x06004403 RID: 17411 RVA: 0x00255968 File Offset: 0x00253B68
		private static float ApplyPercentage(float value, float amount, int times)
		{
			return (1f + amount / 100f * (float)times) * value;
		}

		// Token: 0x06004404 RID: 17412 RVA: 0x0025597C File Offset: 0x00253B7C
		public static PropertyReference[] ParseAttributes(IEnumerable<XAttribute> attributes, Upgrade upgrade)
		{
			return (from attribute in attributes
			select new PropertyReference(attribute.NameAsIdentifier(), attribute.Value, upgrade)).ToArray<PropertyReference>();
		}

		// Token: 0x06004405 RID: 17413 RVA: 0x002559B0 File Offset: 0x00253BB0
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

		// Token: 0x040023A9 RID: 9129
		public readonly Identifier Name;

		// Token: 0x040023AA RID: 9130
		private readonly string Multiplier;

		// Token: 0x040023AB RID: 9131
		private static readonly char[] prefixCharacters = new char[]
		{
			'=',
			'/',
			'*',
			'x',
			'-',
			'+'
		};

		// Token: 0x040023AC RID: 9132
		private readonly Upgrade upgrade;
	}
}
