using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001D3 RID: 467
	[NullableContext(1)]
	[Nullable(0)]
	internal class CampaignMetadata
	{
		// Token: 0x06002278 RID: 8824 RVA: 0x000E7E74 File Offset: 0x000E6074
		public void Load(XElement element)
		{
			this.data.Clear();
			foreach (XElement subElement in element.Elements())
			{
				if (string.Equals(subElement.Name.ToString(), "data", StringComparison.InvariantCultureIgnoreCase))
				{
					Identifier identifier = subElement.GetAttributeIdentifier("key", Identifier.Empty);
					string value = subElement.GetAttributeString("value", string.Empty);
					string valueType = subElement.GetAttributeString("type", string.Empty);
					if (identifier.IsEmpty || string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(valueType))
					{
						string str = "Unable to load value because one or more of the required attributes are empty.\n";
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 3);
						defaultInterpolatedStringHandler.AppendLiteral("key: \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\", value: \"");
						defaultInterpolatedStringHandler.AppendFormatted(value);
						defaultInterpolatedStringHandler.AppendLiteral("\", type: \"");
						defaultInterpolatedStringHandler.AppendFormatted(valueType);
						defaultInterpolatedStringHandler.AppendLiteral("\"");
						DebugConsole.ThrowError(str + defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						Type type = ReflectionUtils.GetType(valueType);
						if (type == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(23, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Type for ");
							defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(identifier);
							defaultInterpolatedStringHandler2.AppendLiteral(" not found (");
							defaultInterpolatedStringHandler2.AppendFormatted(valueType);
							defaultInterpolatedStringHandler2.AppendLiteral(").");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
						}
						else if (type == typeof(Identifier))
						{
							this.data.Add(identifier, value.ToIdentifier());
						}
						else
						{
							try
							{
								this.data.Add(identifier, Convert.ChangeType(value, type, NumberFormatInfo.InvariantInfo));
							}
							catch (Exception e)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(46, 2);
								defaultInterpolatedStringHandler3.AppendLiteral("Failed to change the type of the value \"");
								defaultInterpolatedStringHandler3.AppendFormatted(value);
								defaultInterpolatedStringHandler3.AppendLiteral("\" to ");
								defaultInterpolatedStringHandler3.AppendFormatted<Type>(type);
								defaultInterpolatedStringHandler3.AppendLiteral(".");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), e, null, false, false);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002279 RID: 8825 RVA: 0x000E80D0 File Offset: 0x000E62D0
		public void SetValue(Identifier identifier, object value)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Set the value \"");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
			defaultInterpolatedStringHandler.AppendLiteral("\" to ");
			defaultInterpolatedStringHandler.AppendFormatted<object>(value);
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			AchievementManager.OnCampaignMetadataSet(identifier, value, true);
			if (!this.data.ContainsKey(identifier))
			{
				this.data.Add(identifier, value);
				return;
			}
			this.data[identifier] = value;
		}

		// Token: 0x0600227A RID: 8826 RVA: 0x000E814C File Offset: 0x000E634C
		public float GetFloat(Identifier identifier, float? defaultValue = null)
		{
			return (float)this.GetTypeOrDefault(identifier, typeof(float), defaultValue.GetValueOrDefault());
		}

		// Token: 0x0600227B RID: 8827 RVA: 0x000E8170 File Offset: 0x000E6370
		public int GetInt(Identifier identifier, int? defaultValue = null)
		{
			return (int)this.GetTypeOrDefault(identifier, typeof(int), defaultValue.GetValueOrDefault());
		}

		// Token: 0x0600227C RID: 8828 RVA: 0x000E8194 File Offset: 0x000E6394
		public bool GetBoolean(Identifier identifier, bool? defaultValue = null)
		{
			return (bool)this.GetTypeOrDefault(identifier, typeof(bool), defaultValue.GetValueOrDefault());
		}

		// Token: 0x0600227D RID: 8829 RVA: 0x000E81B8 File Offset: 0x000E63B8
		public string GetString(Identifier identifier, [Nullable(2)] string defaultValue = null)
		{
			return (string)this.GetTypeOrDefault(identifier, typeof(string), defaultValue ?? string.Empty);
		}

		// Token: 0x0600227E RID: 8830 RVA: 0x000E81DA File Offset: 0x000E63DA
		public bool HasKey(Identifier identifier)
		{
			return this.data.ContainsKey(identifier);
		}

		// Token: 0x0600227F RID: 8831 RVA: 0x000E81E8 File Offset: 0x000E63E8
		private object GetTypeOrDefault(Identifier identifier, Type type, object defaultValue)
		{
			object value = this.GetValue(identifier);
			if (value != null)
			{
				if (value.GetType() == type)
				{
					return value;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Attempted to get value \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" as a ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(type);
				defaultInterpolatedStringHandler.AppendLiteral(" but the value is ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(value.GetType());
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			return defaultValue;
		}

		// Token: 0x06002280 RID: 8832 RVA: 0x000E8278 File Offset: 0x000E6478
		[NullableContext(2)]
		public object GetValue(Identifier identifier)
		{
			if (!this.data.ContainsKey(identifier))
			{
				return null;
			}
			return this.data[identifier];
		}

		// Token: 0x06002281 RID: 8833 RVA: 0x000E8298 File Offset: 0x000E6498
		public void Save(XElement modeElement)
		{
			XElement element = new XElement("Metadata");
			foreach (KeyValuePair<Identifier, object> keyValuePair in this.data)
			{
				Identifier identifier;
				object obj;
				keyValuePair.Deconstruct(out identifier, out obj);
				Identifier key = identifier;
				object value = obj;
				string text = value.ToString();
				if (text == null)
				{
					throw new NullReferenceException();
				}
				string valueStr = text;
				if (value is float)
				{
					valueStr = ((float)value).ToString("G", CultureInfo.InvariantCulture);
				}
				element.Add(new XElement("Data", new object[]
				{
					new XAttribute("key", key),
					new XAttribute("value", valueStr),
					new XAttribute("type", value.GetType().FullName ?? "")
				}));
			}
			modeElement.Add(element);
		}

		// Token: 0x0400107C RID: 4220
		private readonly Dictionary<Identifier, object> data = new Dictionary<Identifier, object>();
	}
}
