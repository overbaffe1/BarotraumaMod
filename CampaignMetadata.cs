using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000065 RID: 101
	[NullableContext(1)]
	[Nullable(0)]
	internal class CampaignMetadata
	{
		// Token: 0x06000E82 RID: 3714 RVA: 0x00089D0C File Offset: 0x00087F0C
		[NullableContext(0)]
		public void DebugDraw(SpriteBatch spriteBatch, Vector2 pos, CampaignMode campaign, GUI.DebugDrawMetaData debugDrawMetaData)
		{
			CampaignMetadata.<>c__DisplayClass1_0 CS$<>8__locals1;
			CS$<>8__locals1.campaignData = this.data;
			if (!debugDrawMetaData.FactionMetadata)
			{
				CampaignMetadata.<DebugDraw>g__removeData|1_0("reputation.faction", ref CS$<>8__locals1);
			}
			if (!debugDrawMetaData.UpgradeLevels)
			{
				CampaignMetadata.<DebugDraw>g__removeData|1_0("upgrade.", ref CS$<>8__locals1);
			}
			if (!debugDrawMetaData.UpgradePrices)
			{
				CampaignMetadata.<DebugDraw>g__removeData|1_0("upgradeprice.", ref CS$<>8__locals1);
			}
			int offset = 0;
			if (CS$<>8__locals1.campaignData.Count > 0)
			{
				offset = debugDrawMetaData.Offset % CS$<>8__locals1.campaignData.Count;
				if (offset < 0)
				{
					offset += CS$<>8__locals1.campaignData.Count;
				}
			}
			string text = "Campaign metadata:\n";
			int max = 0;
			for (int i = offset; i < CS$<>8__locals1.campaignData.Count + offset; i++)
			{
				int index = i;
				if (index >= CS$<>8__locals1.campaignData.Count)
				{
					index -= CS$<>8__locals1.campaignData.Count;
				}
				Identifier identifier;
				object obj;
				CS$<>8__locals1.campaignData.ElementAt(index).Deconstruct(out identifier, out obj);
				Identifier key = identifier;
				object value = obj;
				if (max >= 12)
				{
					text += "Use arrow keys to scroll";
					break;
				}
				text = string.Concat(new string[]
				{
					text,
					key.ColorizeObject(),
					": ",
					value.ColorizeObject(),
					"\n"
				});
				max++;
			}
			text = text.TrimEnd('\n');
			ImmutableArray<RichTextData>? richTextDatas = RichTextData.GetRichTextData(text, out text);
			Vector2 size = GUIStyle.SmallFont.MeasureString(text, false);
			Vector2 infoPos = new Vector2((float)GameMain.GraphicsWidth - size.X - 16f, pos.Y + 8f);
			Rectangle infoRect = new Rectangle(infoPos.ToPoint(), size.ToPoint());
			infoRect.Inflate(8, 8);
			GUI.DrawRectangle(spriteBatch, infoRect, Color.Black * 0.8f, true, 0f, 1f);
			GUI.DrawRectangle(spriteBatch, infoRect, Color.White * 0.8f, false, 0f, 1f);
			ImmutableArray<RichTextData>? left = richTextDatas;
			ImmutableArray<RichTextData>? right = null;
			if (left != right && richTextDatas.Value.Any<RichTextData>())
			{
				Vector2 pos2 = infoPos;
				string text2 = text;
				Color white = Color.White;
				right = new ImmutableArray<RichTextData>?(richTextDatas.Value);
				GUIFont font = GUIStyle.SmallFont;
				GUI.DrawStringWithColors(spriteBatch, pos2, text2, white, right, null, 0, font, 0f);
			}
			else
			{
				Vector2 pos3 = infoPos;
				string text3 = text;
				Color white2 = Color.White;
				GUIFont font = GUIStyle.SmallFont;
				GUI.DrawString(spriteBatch, pos3, text3, white2, null, 0, font, ForceUpperCase.Inherit);
			}
			float y = (float)(infoRect.Bottom + 16);
			if (campaign.Factions != null)
			{
				Vector2 factionHeaderSize = GUIStyle.SubHeadingFont.MeasureString("Reputations", false);
				Vector2 factionPos = new Vector2((float)(GameMain.GraphicsWidth - 132) - factionHeaderSize.X / 2f, y);
				Vector2 pos4 = factionPos;
				string text4 = "Reputations";
				Color white3 = Color.White;
				GUIFont font = GUIStyle.SubHeadingFont;
				GUI.DrawString(spriteBatch, pos4, text4, white3, null, 0, font, ForceUpperCase.Inherit);
				y += factionHeaderSize.Y + 8f;
				foreach (Faction faction in campaign.Factions)
				{
					LocalizedString name = faction.Prefab.Name;
					Vector2 nameSize = GUIStyle.SmallFont.MeasureString(name, false);
					Vector2 pos5 = new Vector2((float)(GameMain.GraphicsWidth - 264), y);
					LocalizedString text5 = name;
					Color white4 = Color.White;
					font = GUIStyle.SmallFont;
					GUI.DrawString(spriteBatch, pos5, text5, white4, null, 0, font, ForceUpperCase.Inherit);
					y += nameSize.Y + 5f;
					Color color = ToolBox.GradientLerp(faction.Reputation.NormalizedValue, new Color[]
					{
						Color.Red,
						Color.Yellow,
						Color.LightGreen
					});
					GUI.DrawRectangle(spriteBatch, new Rectangle(GameMain.GraphicsWidth - 264, (int)y, (int)(faction.Reputation.NormalizedValue * 255f), 10), color, true, 0f, 1f);
					GUI.DrawRectangle(spriteBatch, new Rectangle(GameMain.GraphicsWidth - 264, (int)y, 256, 10), Color.White, false, 0f, 1f);
					y += 15f;
				}
			}
		}

		// Token: 0x06000E84 RID: 3716 RVA: 0x0008A190 File Offset: 0x00088390
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

		// Token: 0x06000E85 RID: 3717 RVA: 0x0008A3EC File Offset: 0x000885EC
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

		// Token: 0x06000E86 RID: 3718 RVA: 0x0008A468 File Offset: 0x00088668
		public float GetFloat(Identifier identifier, float? defaultValue = null)
		{
			return (float)this.GetTypeOrDefault(identifier, typeof(float), defaultValue.GetValueOrDefault());
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x0008A48C File Offset: 0x0008868C
		public int GetInt(Identifier identifier, int? defaultValue = null)
		{
			return (int)this.GetTypeOrDefault(identifier, typeof(int), defaultValue.GetValueOrDefault());
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x0008A4B0 File Offset: 0x000886B0
		public bool GetBoolean(Identifier identifier, bool? defaultValue = null)
		{
			return (bool)this.GetTypeOrDefault(identifier, typeof(bool), defaultValue.GetValueOrDefault());
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x0008A4D4 File Offset: 0x000886D4
		public string GetString(Identifier identifier, [Nullable(2)] string defaultValue = null)
		{
			return (string)this.GetTypeOrDefault(identifier, typeof(string), defaultValue ?? string.Empty);
		}

		// Token: 0x06000E8A RID: 3722 RVA: 0x0008A4F6 File Offset: 0x000886F6
		public bool HasKey(Identifier identifier)
		{
			return this.data.ContainsKey(identifier);
		}

		// Token: 0x06000E8B RID: 3723 RVA: 0x0008A504 File Offset: 0x00088704
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

		// Token: 0x06000E8C RID: 3724 RVA: 0x0008A594 File Offset: 0x00088794
		[NullableContext(2)]
		public object GetValue(Identifier identifier)
		{
			if (!this.data.ContainsKey(identifier))
			{
				return null;
			}
			return this.data[identifier];
		}

		// Token: 0x06000E8D RID: 3725 RVA: 0x0008A5B4 File Offset: 0x000887B4
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

		// Token: 0x06000E8E RID: 3726 RVA: 0x0008A6D4 File Offset: 0x000888D4
		[NullableContext(0)]
		[CompilerGenerated]
		internal static void <DebugDraw>g__removeData|1_0(string keyStartsWith, ref CampaignMetadata.<>c__DisplayClass1_0 A_1)
		{
			A_1.campaignData = (from pair in A_1.campaignData
			where !pair.Key.StartsWith(keyStartsWith)
			select pair).ToDictionary((KeyValuePair<Identifier, object> i) => i.Key, (KeyValuePair<Identifier, object> i) => i.Value);
		}

		// Token: 0x04000770 RID: 1904
		private const int MaxDrawnElements = 12;

		// Token: 0x04000771 RID: 1905
		private readonly Dictionary<Identifier, object> data = new Dictionary<Identifier, object>();
	}
}
