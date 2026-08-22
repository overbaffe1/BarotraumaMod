using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000121 RID: 289
	internal sealed class SerializableEntityEditor : GUIComponent
	{
		// Token: 0x17000A4C RID: 2636
		// (get) Token: 0x0600279B RID: 10139 RVA: 0x001B68A3 File Offset: 0x001B4AA3
		// (set) Token: 0x0600279C RID: 10140 RVA: 0x001B68AC File Offset: 0x001B4AAC
		public bool Readonly
		{
			get
			{
				return this.isReadonly;
			}
			set
			{
				foreach (GUIComponent component in this.Fields.SelectMany((KeyValuePair<Identifier, GUIComponent[]> f) => f.Value))
				{
					GUINumberInput numInput = component as GUINumberInput;
					if (numInput == null)
					{
						GUITextBox textBox = component as GUITextBox;
						if (textBox == null)
						{
							component.Enabled = !value;
						}
						else
						{
							textBox.Readonly = value;
						}
					}
					else
					{
						numInput.Readonly = value;
					}
				}
				this.isReadonly = value;
			}
		}

		// Token: 0x17000A4D RID: 2637
		// (get) Token: 0x0600279D RID: 10141 RVA: 0x001B6950 File Offset: 0x001B4B50
		public int ContentHeight
		{
			get
			{
				if (this.layoutGroup.NeedsToRecalculate)
				{
					this.layoutGroup.Recalculate();
				}
				int spacing = (this.layoutGroup.CountChildren == 0) ? 0 : ((this.layoutGroup.CountChildren - 1) * this.layoutGroup.AbsoluteSpacing);
				return spacing + this.layoutGroup.Children.Sum((GUIComponent c) => c.RectTransform.NonScaledSize.Y);
			}
		}

		// Token: 0x17000A4E RID: 2638
		// (get) Token: 0x0600279E RID: 10142 RVA: 0x001B69D0 File Offset: 0x001B4BD0
		public int ContentCount
		{
			get
			{
				return this.layoutGroup.CountChildren;
			}
		}

		// Token: 0x17000A4F RID: 2639
		// (get) Token: 0x0600279F RID: 10143 RVA: 0x001B69DD File Offset: 0x001B4BDD
		// (set) Token: 0x060027A0 RID: 10144 RVA: 0x001B69E5 File Offset: 0x001B4BE5
		public Dictionary<Identifier, GUIComponent[]> Fields { get; private set; } = new Dictionary<Identifier, GUIComponent[]>();

		// Token: 0x060027A1 RID: 10145 RVA: 0x001B69F0 File Offset: 0x001B4BF0
		public void UpdateValue(SerializableProperty property, object newValue, bool flash = true)
		{
			GUIComponent[] fields;
			if (!this.Fields.TryGetValue(property.Name.ToIdentifier(), out fields))
			{
				DebugConsole.ThrowError("No field for " + property.Name + " found!", null, null, false, false);
				return;
			}
			if (newValue is float)
			{
				float f = (float)newValue;
				foreach (GUIComponent field in fields)
				{
					GUINumberInput numInput = field as GUINumberInput;
					if (numInput != null && numInput.InputType == NumberType.Float)
					{
						numInput.FloatValue = f;
						if (flash)
						{
							numInput.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
						}
					}
				}
				return;
			}
			if (newValue is int)
			{
				int integer = (int)newValue;
				foreach (GUIComponent field2 in fields)
				{
					GUINumberInput numInput2 = field2 as GUINumberInput;
					if (numInput2 != null && numInput2.InputType == NumberType.Int)
					{
						numInput2.IntValue = integer;
						if (flash)
						{
							numInput2.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
						}
					}
				}
				return;
			}
			if (newValue is bool)
			{
				bool b = (bool)newValue;
				GUITickBox tickBox = fields[0] as GUITickBox;
				if (tickBox != null)
				{
					tickBox.Selected = b;
					if (flash)
					{
						tickBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
						return;
					}
				}
			}
			else
			{
				string s = newValue as string;
				if (s != null)
				{
					GUITextBox textBox = fields[0] as GUITextBox;
					if (textBox != null)
					{
						textBox.Text = s;
						if (flash)
						{
							textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
							return;
						}
					}
				}
				else if (newValue.GetType().IsEnum)
				{
					GUIDropDown dropDown = fields[0] as GUIDropDown;
					if (dropDown != null)
					{
						dropDown.Select((int)newValue);
						if (flash)
						{
							dropDown.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
							return;
						}
					}
				}
				else
				{
					if (newValue is Vector2)
					{
						Vector2 v2 = (Vector2)newValue;
						for (int i = 0; i < fields.Length; i++)
						{
							GUIComponent field3 = fields[i];
							GUINumberInput numInput3 = field3 as GUINumberInput;
							if (numInput3 != null && numInput3.InputType == NumberType.Float)
							{
								numInput3.FloatValue = ((i == 0) ? v2.X : v2.Y);
								if (flash)
								{
									numInput3.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
								}
							}
						}
						return;
					}
					if (newValue is Vector3)
					{
						Vector3 v3 = (Vector3)newValue;
						for (int j = 0; j < fields.Length; j++)
						{
							GUIComponent field4 = fields[j];
							GUINumberInput numInput4 = field4 as GUINumberInput;
							if (numInput4 != null && numInput4.InputType == NumberType.Float)
							{
								switch (j)
								{
								case 0:
									numInput4.FloatValue = v3.X;
									break;
								case 1:
									numInput4.FloatValue = v3.Y;
									break;
								case 2:
									numInput4.FloatValue = v3.Z;
									break;
								}
								if (flash)
								{
									numInput4.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
								}
							}
						}
						return;
					}
					if (newValue is Vector4)
					{
						Vector4 v4 = (Vector4)newValue;
						for (int k = 0; k < fields.Length; k++)
						{
							GUIComponent field5 = fields[k];
							GUINumberInput numInput5 = field5 as GUINumberInput;
							if (numInput5 != null && numInput5.InputType == NumberType.Float)
							{
								switch (k)
								{
								case 0:
									numInput5.FloatValue = v4.X;
									break;
								case 1:
									numInput5.FloatValue = v4.Y;
									break;
								case 2:
									numInput5.FloatValue = v4.Z;
									break;
								case 3:
									numInput5.FloatValue = v4.W;
									break;
								}
								if (flash)
								{
									numInput5.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
								}
							}
						}
						return;
					}
					if (newValue is Color)
					{
						Color c = (Color)newValue;
						for (int l = 0; l < fields.Length; l++)
						{
							GUIComponent field6 = fields[l];
							GUINumberInput numInput6 = field6 as GUINumberInput;
							if (numInput6 != null && numInput6.InputType == NumberType.Int)
							{
								switch (l)
								{
								case 0:
									numInput6.IntValue = (int)c.R;
									break;
								case 1:
									numInput6.IntValue = (int)c.G;
									break;
								case 2:
									numInput6.IntValue = (int)c.B;
									break;
								case 3:
									numInput6.IntValue = (int)c.A;
									break;
								}
								if (flash)
								{
									numInput6.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
								}
							}
						}
						GUIComponent comp = fields.FirstOrDefault<GUIComponent>();
						if (comp != null)
						{
							GUIComponent parent2 = comp.Parent;
							GUIComponent guicomponent;
							if (parent2 == null)
							{
								guicomponent = null;
							}
							else
							{
								GUIComponent parent3 = parent2.Parent;
								guicomponent = ((parent3 != null) ? parent3.Parent : null);
							}
							GUIComponent parent = guicomponent;
							if (parent != null)
							{
								GUIButton preview = parent.FindChild("colorpreview", true) as GUIButton;
								if (preview != null)
								{
									preview.Color = (preview.HoverColor = (preview.PressedColor = (preview.SelectedTextColor = c)));
									return;
								}
							}
						}
					}
					else
					{
						if (newValue is Rectangle)
						{
							Rectangle r = (Rectangle)newValue;
							for (int m = 0; m < fields.Length; m++)
							{
								GUIComponent field7 = fields[m];
								GUINumberInput numInput7 = field7 as GUINumberInput;
								if (numInput7 != null && numInput7.InputType == NumberType.Int)
								{
									switch (m)
									{
									case 0:
										numInput7.IntValue = r.X;
										break;
									case 1:
										numInput7.IntValue = r.Y;
										break;
									case 2:
										numInput7.IntValue = r.Width;
										break;
									case 3:
										numInput7.IntValue = r.Height;
										break;
									}
									if (flash)
									{
										numInput7.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
									}
								}
							}
							return;
						}
						string[] a = newValue as string[];
						if (a != null)
						{
							int n = 0;
							while (n < fields.Length && n < a.Length)
							{
								GUITextBox textBox2 = fields[n] as GUITextBox;
								if (textBox2 != null)
								{
									textBox2.Text = a[n];
									if (flash)
									{
										textBox2.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
									}
								}
								n++;
							}
						}
					}
				}
			}
		}

		// Token: 0x060027A2 RID: 10146 RVA: 0x001B70B4 File Offset: 0x001B52B4
		public SerializableEntityEditor(RectTransform parent, ISerializableEntity entity, bool inGame, bool showName, string style = "", int elementHeight = 24, GUIFont titleFont = null, bool dimOutDefaultValues = true) : this(parent, entity, inGame ? SerializableProperty.GetProperties<InGameEditable>(entity).Union(SerializableProperty.GetProperties<ConditionallyEditable>(entity).Where(delegate(SerializableProperty p)
		{
			ConditionallyEditable attribute = p.GetAttribute<ConditionallyEditable>();
			return attribute != null && attribute.IsEditable(entity);
		})) : SerializableProperty.GetProperties<Editable>(entity).Where(delegate(SerializableProperty p)
		{
			ConditionallyEditable attribute = p.GetAttribute<ConditionallyEditable>();
			return attribute == null || attribute.IsEditable(entity);
		}), showName, style, elementHeight, titleFont, dimOutDefaultValues)
		{
		}

		// Token: 0x060027A3 RID: 10147 RVA: 0x001B7134 File Offset: 0x001B5334
		public SerializableEntityEditor(RectTransform parent, ISerializableEntity entity, IEnumerable<SerializableProperty> properties, bool showName, string style = "", int elementHeight = 24, GUIFont titleFont = null, bool dimOutDefaultValues = true) : base(style, new RectTransform(Vector2.One, parent, Anchor.TopLeft, null, null, null, ScaleBasis.Normal))
		{
			this.dimOutDefaultValues = dimOutDefaultValues;
			elementHeight = (int)((float)elementHeight * GUI.Scale);
			GUIComponentStyle tickBoxStyle = GUIStyle.GetComponentStyle("GUITickBox");
			GUIComponentStyle textBoxStyle = GUIStyle.GetComponentStyle("GUITextBox");
			GUIComponentStyle numberInputStyle = GUIStyle.GetComponentStyle("GUINumberInput");
			if (tickBoxStyle.Height != null)
			{
				this.elementHeight = Math.Max(tickBoxStyle.Height.Value, this.elementHeight);
			}
			if (textBoxStyle.Height != null)
			{
				this.elementHeight = Math.Max(textBoxStyle.Height.Value, this.elementHeight);
			}
			if (numberInputStyle.Height != null)
			{
				this.elementHeight = Math.Max(numberInputStyle.Height.Value, this.elementHeight);
			}
			this.layoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, base.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = (int)(5f * GUI.Scale)
			};
			if (showName)
			{
				RectTransform rectT = new RectTransform(new Point(this.layoutGroup.Rect.Width, this.elementHeight), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true);
				RichString text = entity.Name;
				GUIFont font = titleFont ?? GUIStyle.Font;
				GUITextBlock guitextBlock = new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
				guitextBlock.TextColor = Color.White;
				guitextBlock.Color = Color.Black;
			}
			List<Header> headers = new List<Header>
			{
				null
			};
			Dictionary<SerializableProperty, Header> propertyHeaders = new Dictionary<SerializableProperty, Header>();
			Header prevHeader = null;
			foreach (SerializableProperty property in properties)
			{
				Header header = property.GetAttribute<Header>();
				if (header != null)
				{
					prevHeader = header;
					if (!headers.Contains(header))
					{
						headers.Add(header);
					}
				}
				propertyHeaders[property] = prevHeader;
			}
			prevHeader = null;
			foreach (Header header2 in headers)
			{
				foreach (SerializableProperty property2 in properties)
				{
					if (object.Equals(propertyHeaders[property2], header2))
					{
						if (header2 != null && !object.Equals(header2, prevHeader))
						{
							new GUITextBlock(new RectTransform(new Point(this.Rect.Width, Math.Max(elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), header2.Text, new Color?(GUIStyle.TextColorBright), GUIStyle.SubHeadingFont, Alignment.Left, false, "", null);
							prevHeader = header2;
						}
						this.CreateNewField(property2, entity);
					}
				}
			}
			this.Recalculate();
		}

		// Token: 0x060027A4 RID: 10148 RVA: 0x001B74DC File Offset: 0x001B56DC
		public void AddCustomContent(GUIComponent component, int childIndex)
		{
			component.RectTransform.Parent = this.layoutGroup.RectTransform;
			component.RectTransform.RepositionChildInHierarchy(Math.Min(childIndex, this.layoutGroup.CountChildren - 1));
			this.layoutGroup.Recalculate();
			this.Recalculate();
		}

		// Token: 0x060027A5 RID: 10149 RVA: 0x001B752F File Offset: 0x001B572F
		public void RefreshValues()
		{
			Action action = this.refresh;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x060027A6 RID: 10150 RVA: 0x001B7541 File Offset: 0x001B5741
		public void Recalculate()
		{
			base.RectTransform.Resize(new Point(base.RectTransform.NonScaledSize.X, this.ContentHeight), true);
		}

		// Token: 0x060027A7 RID: 10151 RVA: 0x001B756C File Offset: 0x001B576C
		public GUIComponent CreateNewField(SerializableProperty property, ISerializableEntity entity)
		{
			object value = property.GetValue(entity);
			if (property.PropertyType == typeof(string) && value == null)
			{
				value = "";
			}
			Identifier propertyTag = (property.PropertyInfo.DeclaringType.Name + "." + property.PropertyInfo.Name).ToIdentifier();
			Identifier fallbackTag = property.PropertyInfo.Name.ToIdentifier();
			Identifier[] array = new Identifier[2];
			array[0] = propertyTag;
			int num = 1;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
			defaultInterpolatedStringHandler.AppendLiteral("sp.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(propertyTag);
			defaultInterpolatedStringHandler.AppendLiteral(".name");
			array[num] = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
			LocalizedString displayName = TextManager.Get(array);
			if (displayName.IsNullOrEmpty())
			{
				Editable editable = property.GetAttribute<Editable>();
				if (editable != null && !string.IsNullOrEmpty(editable.FallBackTextTag))
				{
					displayName = TextManager.Get(editable.FallBackTextTag);
				}
				else
				{
					Identifier[] array2 = new Identifier[2];
					array2[0] = fallbackTag;
					int num2 = 1;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(8, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("sp.");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(fallbackTag);
					defaultInterpolatedStringHandler2.AppendLiteral(".name");
					array2[num2] = defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier();
					displayName = TextManager.Get(array2);
				}
			}
			if (displayName.IsNullOrEmpty())
			{
				displayName = property.Name.FormatCamelCaseWithSpaces();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("sp.");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(propertyTag);
			defaultInterpolatedStringHandler3.AppendLiteral(".description");
			LocalizedString toolTip = TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
			if (entity.GetType() != property.PropertyInfo.DeclaringType)
			{
				Identifier propertyTagForDerivedClass = (entity.GetType().Name + "." + property.PropertyInfo.Name).ToIdentifier();
				string[] array3 = new string[2];
				int num3 = 0;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(propertyTagForDerivedClass);
				defaultInterpolatedStringHandler4.AppendLiteral(".description");
				array3[num3] = defaultInterpolatedStringHandler4.ToStringAndClear();
				int num4 = 1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("sp.");
				defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(propertyTagForDerivedClass);
				defaultInterpolatedStringHandler5.AppendLiteral(".description");
				array3[num4] = defaultInterpolatedStringHandler5.ToStringAndClear();
				LocalizedString toolTipForDerivedClass = TextManager.Get(array3);
				if (!toolTipForDerivedClass.IsNullOrEmpty())
				{
					toolTip = toolTipForDerivedClass;
				}
			}
			if (toolTip.IsNullOrEmpty())
			{
				string[] array4 = new string[3];
				int num5 = 0;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(propertyTag);
				defaultInterpolatedStringHandler6.AppendLiteral(".description");
				array4[num5] = defaultInterpolatedStringHandler6.ToStringAndClear();
				int num6 = 1;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler7.AppendFormatted<Identifier>(fallbackTag);
				defaultInterpolatedStringHandler7.AppendLiteral(".description");
				array4[num6] = defaultInterpolatedStringHandler7.ToStringAndClear();
				int num7 = 2;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler8.AppendLiteral("sp.");
				defaultInterpolatedStringHandler8.AppendFormatted<Identifier>(fallbackTag);
				defaultInterpolatedStringHandler8.AppendLiteral(".description");
				array4[num7] = defaultInterpolatedStringHandler8.ToStringAndClear();
				toolTip = TextManager.Get(array4);
			}
			if (toolTip.IsNullOrEmpty())
			{
				Serialize attribute = property.GetAttribute<Serialize>();
				toolTip = ((attribute != null) ? attribute.Description : null);
			}
			GUIComponent propertyField = null;
			if (value is bool)
			{
				bool boolVal = (bool)value;
				propertyField = this.CreateBoolField(entity, property, boolVal, displayName, toolTip);
			}
			else if (value.GetType().IsEnum)
			{
				if (value.GetType().IsDefined(typeof(FlagsAttribute), false))
				{
					propertyField = this.CreateEnumFlagField(entity, property, value, displayName, toolTip);
				}
				else
				{
					propertyField = this.CreateEnumField(entity, property, value, displayName, toolTip);
				}
			}
			else if (value is int)
			{
				int i = (int)value;
				propertyField = this.CreateIntField(entity, property, i, displayName, toolTip);
			}
			else if (value is float)
			{
				float f = (float)value;
				propertyField = this.CreateFloatField(entity, property, f, displayName, toolTip);
			}
			else if (value is Point)
			{
				Point p = (Point)value;
				propertyField = this.CreatePointField(entity, property, p, displayName, toolTip);
			}
			else if (value is Vector2)
			{
				Vector2 v2 = (Vector2)value;
				propertyField = this.CreateVector2Field(entity, property, v2, displayName, toolTip);
			}
			else if (value is Vector3)
			{
				Vector3 v3 = (Vector3)value;
				propertyField = this.CreateVector3Field(entity, property, v3, displayName, toolTip);
			}
			else if (value is Vector4)
			{
				Vector4 v4 = (Vector4)value;
				propertyField = this.CreateVector4Field(entity, property, v4, displayName, toolTip);
			}
			else if (value is Color)
			{
				Color c = (Color)value;
				propertyField = this.CreateColorField(entity, property, c, displayName, toolTip);
			}
			else if (value is Rectangle)
			{
				Rectangle r = (Rectangle)value;
				propertyField = this.CreateRectangleField(entity, property, r, displayName, toolTip);
			}
			else
			{
				string[] a = value as string[];
				if (a != null)
				{
					propertyField = this.CreateStringArrayField(entity, property, a, displayName, toolTip);
				}
				else
				{
					bool flag = value is string || value is Identifier;
					if (flag)
					{
						propertyField = this.CreateStringField(entity, property, value.ToString(), displayName, toolTip);
					}
				}
			}
			if (propertyField != null && this.dimOutDefaultValues)
			{
				this.UpdateTextColors(property, entity, propertyField);
			}
			return propertyField;
		}

		// Token: 0x060027A8 RID: 10152 RVA: 0x001B7A68 File Offset: 0x001B5C68
		private void UpdateTextColors(SerializableProperty property, object parentObject, GUIComponent parentElement)
		{
			if (!this.dimOutDefaultValues)
			{
				return;
			}
			bool isSetToDefaultValue = false;
			object currentValue = property.GetValue(parentObject);
			foreach (Serialize attribute in property.Attributes.OfType<Serialize>())
			{
				if (!XMLExtensions.DefaultValueEquals(attribute.DefaultValue, currentValue))
				{
					if (currentValue != null)
					{
						continue;
					}
					string defaultValueStr = attribute.DefaultValue as string;
					if (defaultValueStr == null || !defaultValueStr.IsNullOrEmpty())
					{
						continue;
					}
				}
				isSetToDefaultValue = true;
				break;
			}
			foreach (GUIComponent component in parentElement.GetAllChildren())
			{
				this.UpdateTextColors(component, isSetToDefaultValue);
			}
		}

		// Token: 0x060027A9 RID: 10153 RVA: 0x001B7B3C File Offset: 0x001B5D3C
		private void UpdateTextColors(GUIComponent component, bool isSetToDefaultValue)
		{
			SerializableEntityEditor.<>c__DisplayClass30_0 CS$<>8__locals1;
			CS$<>8__locals1.isSetToDefaultValue = isSetToDefaultValue;
			if (!this.dimOutDefaultValues)
			{
				return;
			}
			GUINumberInput numberInput = component as GUINumberInput;
			if (numberInput != null)
			{
				SerializableEntityEditor.<UpdateTextColors>g__SetTextColor|30_0(numberInput.TextBox.TextBlock, ref CS$<>8__locals1);
				return;
			}
			GUIDropDown dropDown = component as GUIDropDown;
			if (dropDown != null)
			{
				SerializableEntityEditor.<UpdateTextColors>g__SetTextColor|30_0(dropDown.Button.TextBlock, ref CS$<>8__locals1);
				return;
			}
			GUITextBox textBox = component as GUITextBox;
			if (textBox != null)
			{
				SerializableEntityEditor.<UpdateTextColors>g__SetTextColor|30_0(textBox.TextBlock, ref CS$<>8__locals1);
				return;
			}
			GUITextBlock textBlock = component as GUITextBlock;
			if (textBlock != null)
			{
				SerializableEntityEditor.<UpdateTextColors>g__SetTextColor|30_0(textBlock, ref CS$<>8__locals1);
				return;
			}
			GUITickBox tickBox = component as GUITickBox;
			if (tickBox != null)
			{
				SerializableEntityEditor.<UpdateTextColors>g__SetTextColor|30_0(tickBox.TextBlock, ref CS$<>8__locals1);
			}
		}

		// Token: 0x060027AA RID: 10154 RVA: 0x001B7BDC File Offset: 0x001B5DDC
		public GUIComponent CreateBoolField(ISerializableEntity entity, SerializableProperty property, bool value, LocalizedString displayName, LocalizedString toolTip)
		{
			SerializableEntityEditor.<>c__DisplayClass31_0 CS$<>8__locals1 = new SerializableEntityEditor.<>c__DisplayClass31_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.property = property;
			CS$<>8__locals1.entity = entity;
			Editable editableAttribute = CS$<>8__locals1.property.GetAttribute<Editable>();
			if (editableAttribute.ReadOnly)
			{
				GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
				RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = displayName;
				GUIFont smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
				return new GUITextBlock(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), value.ToString(), null, null, Alignment.Left, false, "", null)
				{
					ToolTip = toolTip,
					Font = GUIStyle.SmallFont
				};
			}
			GUITickBox propertyTickBox = new GUITickBox(new RectTransform(new Point(this.Rect.Width, this.elementHeight), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), displayName, null, "")
			{
				Font = GUIStyle.SmallFont,
				Enabled = !this.Readonly,
				Selected = value,
				ToolTip = toolTip,
				OnSelected = delegate(GUITickBox tickBox)
				{
					if (CS$<>8__locals1.<>4__this.SetPropertyValue(CS$<>8__locals1.property, CS$<>8__locals1.entity, tickBox.Selected))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(CS$<>8__locals1.entity, CS$<>8__locals1.property);
					}
					bool propertyValue = (bool)CS$<>8__locals1.property.GetValue(CS$<>8__locals1.entity);
					if (tickBox.Selected != propertyValue)
					{
						tickBox.Selected = propertyValue;
						tickBox.Flash(new Color?(Color.Red), 1.5f, false, false, null);
					}
					CS$<>8__locals1.<>4__this.UpdateTextColors(CS$<>8__locals1.property, CS$<>8__locals1.entity, tickBox);
					return true;
				}
			};
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				propertyTickBox.Selected = (bool)CS$<>8__locals1.property.GetValue(CS$<>8__locals1.entity);
			}));
			if (!this.Fields.ContainsKey(CS$<>8__locals1.property.Name))
			{
				this.Fields.Add(CS$<>8__locals1.property.Name.ToIdentifier(), new GUIComponent[]
				{
					propertyTickBox
				});
			}
			return propertyTickBox;
		}

		// Token: 0x060027AB RID: 10155 RVA: 0x001B7E88 File Offset: 0x001B6088
		public GUIComponent CreateIntField(ISerializableEntity entity, SerializableProperty property, int value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			Editable editableAttribute = property.GetAttribute<Editable>();
			GUIComponent field;
			if (editableAttribute.ReadOnly)
			{
				GUITextBlock numberInput3 = new GUITextBlock(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), value.ToString(), null, null, Alignment.Left, false, "", null)
				{
					ToolTip = toolTip,
					Font = GUIStyle.SmallFont
				};
				field = numberInput3;
			}
			else
			{
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					ToolTip = toolTip,
					Font = GUIStyle.SmallFont,
					Readonly = this.Readonly
				};
				numberInput.MinValueInt = new int?(editableAttribute.MinValueInt);
				numberInput.MaxValueInt = new int?(editableAttribute.MaxValueInt);
				numberInput.IntValue = value;
				GUINumberInput numberInput2 = numberInput;
				numberInput2.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(numberInput2.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
				{
					if (this.SetPropertyValue(property, entity, numInput.IntValue))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
					}
					this.UpdateTextColors(property, entity, frame);
				}));
				this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
				{
					if (!numberInput.TextBox.Selected)
					{
						numberInput.IntValue = (int)property.GetValue(entity);
					}
				}));
				field = numberInput;
			}
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), new GUIComponent[]
				{
					field
				});
			}
			return frame;
		}

		// Token: 0x060027AC RID: 10156 RVA: 0x001B81AC File Offset: 0x001B63AC
		public GUIComponent CreateFloatField(ISerializableEntity entity, SerializableProperty property, float value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent))
			{
				CanBeFocused = false
			};
			RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				ToolTip = toolTip,
				Font = GUIStyle.SmallFont
			};
			Editable editableAttribute = property.GetAttribute<Editable>();
			numberInput.MinValueFloat = new float?(editableAttribute.MinValueFloat);
			numberInput.MaxValueFloat = new float?(editableAttribute.MaxValueFloat);
			numberInput.DecimalsToDisplay = editableAttribute.DecimalCount;
			numberInput.ValueStep = editableAttribute.ValueStep;
			numberInput.PlusMinusButtonVisibility = (editableAttribute.ForceShowPlusMinusButtons ? GUINumberInput.ButtonVisibility.ForceVisible : GUINumberInput.ButtonVisibility.Automatic);
			numberInput.FloatValue = value;
			GUINumberInput numberInput2 = numberInput;
			numberInput2.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(numberInput2.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
			{
				if (this.SetPropertyValue(property, entity, numInput.FloatValue))
				{
					SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
				}
				this.UpdateTextColors(property, entity, frame);
			}));
			SerializableEntityEditor.HandleSetterValueTampering(numberInput, () => property.GetFloatValue(entity));
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!numberInput.TextBox.Selected)
				{
					numberInput.FloatValue = (float)property.GetValue(entity);
				}
			}));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), new GUIComponent[]
				{
					numberInput
				});
			}
			return frame;
		}

		// Token: 0x060027AD RID: 10157 RVA: 0x001B845C File Offset: 0x001B665C
		private static void HandleSetterValueTampering(GUINumberInput numberInput, Func<float> getter)
		{
			SerializableEntityEditor.<>c__DisplayClass34_0 CS$<>8__locals1 = new SerializableEntityEditor.<>c__DisplayClass34_0();
			CS$<>8__locals1.getter = getter;
			CS$<>8__locals1.numberInput = numberInput;
			GUINumberInput numberInput2 = CS$<>8__locals1.numberInput;
			numberInput2.OnValueEntered = (GUINumberInput.OnValueEnteredHandler)Delegate.Combine(numberInput2.OnValueEntered, new GUINumberInput.OnValueEnteredHandler(CS$<>8__locals1.<HandleSetterValueTampering>g__HandleSetterModifyingInput|0));
			GUIButton plusButton = CS$<>8__locals1.numberInput.PlusButton;
			plusButton.OnPressed = (GUIButton.OnPressedHandler)Delegate.Combine(plusButton.OnPressed, new GUIButton.OnPressedHandler(CS$<>8__locals1.<HandleSetterValueTampering>g__HandleSetterModifyingInputOnButtonPressed|1));
			GUIButton plusButton2 = CS$<>8__locals1.numberInput.PlusButton;
			plusButton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(plusButton2.OnClicked, new GUIButton.OnClickedHandler(CS$<>8__locals1.<HandleSetterValueTampering>g__HandleSetterModifyingInputOnButtonClicked|2));
			GUIButton minusButton = CS$<>8__locals1.numberInput.MinusButton;
			minusButton.OnPressed = (GUIButton.OnPressedHandler)Delegate.Combine(minusButton.OnPressed, new GUIButton.OnPressedHandler(CS$<>8__locals1.<HandleSetterValueTampering>g__HandleSetterModifyingInputOnButtonPressed|1));
			GUIButton minusButton2 = CS$<>8__locals1.numberInput.MinusButton;
			minusButton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(minusButton2.OnClicked, new GUIButton.OnClickedHandler(CS$<>8__locals1.<HandleSetterValueTampering>g__HandleSetterModifyingInputOnButtonClicked|2));
		}

		// Token: 0x060027AE RID: 10158 RVA: 0x001B8554 File Offset: 0x001B6754
		public GUIComponent CreateEnumField(ISerializableEntity entity, SerializableProperty property, object value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, this.elementHeight), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUIDropDown enumDropDown = new GUIDropDown(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), null, Enum.GetValues(value.GetType()).Length, "", false, false, Alignment.CenterLeft, 1f)
			{
				ToolTip = toolTip
			};
			foreach (object enumValue in Enum.GetValues(value.GetType()))
			{
				enumDropDown.AddItem(enumValue.ToString(), enumValue, null, null, null);
			}
			enumDropDown.SelectItem(value);
			GUIDropDown enumDropDown2 = enumDropDown;
			enumDropDown2.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(enumDropDown2.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent selected, object val)
			{
				if (this.SetPropertyValue(property, entity, val))
				{
					SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
				}
				this.UpdateTextColors(property, entity, frame);
				return true;
			}));
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!enumDropDown.Dropped)
				{
					enumDropDown.SelectItem(property.GetValue(entity));
				}
			}));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), new GUIComponent[]
				{
					enumDropDown
				});
			}
			return frame;
		}

		// Token: 0x060027AF RID: 10159 RVA: 0x001B87E0 File Offset: 0x001B69E0
		public GUIComponent CreateEnumFlagField(ISerializableEntity entity, SerializableProperty property, object value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, this.elementHeight), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUIDropDown enumDropDown = new GUIDropDown(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), null, Enum.GetValues(value.GetType()).Length, "", true, false, Alignment.CenterLeft, 1f)
			{
				ToolTip = toolTip
			};
			bool isFlagsAttribute = value.GetType().IsDefined(typeof(FlagsAttribute), false);
			bool hasNoneOption = false;
			foreach (object enumValue in Enum.GetValues(value.GetType()))
			{
				if (!isFlagsAttribute || MathHelper.IsPowerOfTwo((int)enumValue))
				{
					hasNoneOption |= ((int)enumValue == 0);
					enumDropDown.AddItem(enumValue.ToString(), enumValue, null, null, null);
					if (((int)enumValue != 0 || (int)value == 0) && ((Enum)value).HasFlag((Enum)enumValue))
					{
						enumDropDown.SelectItem(enumValue);
					}
				}
			}
			enumDropDown.MustSelectAtLeastOne = !hasNoneOption;
			GUIDropDown enumDropDown2 = enumDropDown;
			enumDropDown2.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(enumDropDown2.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent selected, object val)
			{
				if (this.SetPropertyValue(property, entity, string.Join(", ", from d in enumDropDown.SelectedDataMultiple
				select d.ToString())))
				{
					SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
				}
				return true;
			}));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), new GUIComponent[]
				{
					enumDropDown
				});
			}
			return frame;
		}

		// Token: 0x060027B0 RID: 10160 RVA: 0x001B8AB0 File Offset: 0x001B6CB0
		public GUIComponent CreateStringField(ISerializableEntity entity, SerializableProperty property, string value, LocalizedString displayName, LocalizedString toolTip)
		{
			SerializableEntityEditor.<>c__DisplayClass37_0 CS$<>8__locals1 = new SerializableEntityEditor.<>c__DisplayClass37_0();
			CS$<>8__locals1.property = property;
			CS$<>8__locals1.entity = entity;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.isItemTagBox = SerializableEntityEditor.<CreateStringField>g__IsItemTagBox|37_6(CS$<>8__locals1.entity, CS$<>8__locals1.property.Name, out CS$<>8__locals1.it);
			GUILayoutGroup mainFrame = new GUILayoutGroup(new RectTransform(new Point(this.Rect.Width, CS$<>8__locals1.isItemTagBox ? (this.elementHeight * 2) : this.elementHeight), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), false, Anchor.TopLeft);
			CS$<>8__locals1.frame = new GUILayoutGroup(new RectTransform(CS$<>8__locals1.isItemTagBox ? new Vector2(1f, 0.5f) : Vector2.One, mainFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), CS$<>8__locals1.frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text3, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			SerializableEntityEditor.<>c__DisplayClass37_0 CS$<>8__locals2 = CS$<>8__locals1;
			Serialize attribute = CS$<>8__locals1.property.GetAttribute<Serialize>();
			CS$<>8__locals2.translationTextTag = ((attribute != null) ? attribute.TranslationTextTag : Identifier.Empty);
			Editable editableAttribute = CS$<>8__locals1.property.GetAttribute<Editable>();
			float textBoxWidth = this.inputFieldWidth;
			if (!CS$<>8__locals1.translationTextTag.IsEmpty | CS$<>8__locals1.isItemTagBox)
			{
				textBoxWidth -= 0.1f;
			}
			CS$<>8__locals1.propertyBox = new GUITextBox(new RectTransform(new Vector2(textBoxWidth, 1f), CS$<>8__locals1.frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				Enabled = (editableAttribute != null && !editableAttribute.ReadOnly),
				Readonly = this.Readonly,
				ToolTip = toolTip,
				Font = GUIStyle.SmallFont,
				Text = CS$<>8__locals1.<CreateStringField>g__StripPrefabTags|7(value),
				OverflowClip = true
			};
			if (editableAttribute != null && editableAttribute.MaxLength > 0)
			{
				CS$<>8__locals1.propertyBox.MaxTextLength = new int?(editableAttribute.MaxLength);
			}
			CS$<>8__locals1.editedEntities = new HashSet<MapEntity>();
			CS$<>8__locals1.propertyBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				foreach (MapEntity entity2 in MapEntity.SelectedList)
				{
					CS$<>8__locals1.editedEntities.Add(entity2);
				}
				return true;
			};
			CS$<>8__locals1.propertyBox.OnDeselected += delegate(GUITextBox textBox, Keys keys)
			{
				base.<CreateStringField>g__OnApply|3(textBox);
			};
			GUITextBox propertyBox = CS$<>8__locals1.propertyBox;
			propertyBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(propertyBox.OnEnterPressed, new GUITextBox.OnEnterHandler((GUITextBox box, string text) => base.<CreateStringField>g__OnApply|3(box)));
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (CS$<>8__locals1.propertyBox.Selected)
				{
					return;
				}
				GUITextBox propertyBox2 = CS$<>8__locals1.propertyBox;
				object value2 = CS$<>8__locals1.property.GetValue(CS$<>8__locals1.entity);
				propertyBox2.Text = base.<CreateStringField>g__StripPrefabTags|7((value2 != null) ? value2.ToString() : null);
			}));
			if (!CS$<>8__locals1.translationTextTag.IsEmpty)
			{
				new GUIButton(new RectTransform(new Vector2(0.1f, 1f), CS$<>8__locals1.frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), "...", Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton bt, object userData)
				{
					CS$<>8__locals1.<>4__this.CreateTextPicker(CS$<>8__locals1.translationTextTag.Value, CS$<>8__locals1.entity, CS$<>8__locals1.property, CS$<>8__locals1.propertyBox);
					return true;
				};
				CS$<>8__locals1.propertyBox.OnTextChanged += delegate(GUITextBox tb, string text)
				{
					LocalizedString translatedText = TextManager.Get(text);
					if (translatedText.IsNullOrEmpty())
					{
						CS$<>8__locals1.propertyBox.TextColor = Color.Gray;
						CS$<>8__locals1.propertyBox.ToolTip = TextManager.GetWithVariable("StringPropertyCannotTranslate", "[tag]", text ?? string.Empty, FormatCapitals.No);
					}
					else
					{
						CS$<>8__locals1.propertyBox.TextColor = GUIStyle.Green;
						CS$<>8__locals1.propertyBox.ToolTip = TextManager.GetWithVariable("StringPropertyTranslate", "[translation]", translatedText, FormatCapitals.No);
					}
					return true;
				};
				CS$<>8__locals1.propertyBox.Text = value;
			}
			if (CS$<>8__locals1.isItemTagBox)
			{
				GUILayoutGroup prefabFrame = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), mainFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true
				};
				RectTransform rectT2 = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), prefabFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = TextManager.Get("predefinedtags.name");
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Left, false, "", null).ToolTip = TextManager.Get("predefinedtags.description");
				GUITextBox guitextBox = new GUITextBox(new RectTransform(new Vector2(this.inputFieldWidth, 1f), prefabFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, false);
				guitextBox.Readonly = true;
				guitextBox.Font = GUIStyle.SmallFont;
				guitextBox.Text = SerializableEntityEditor.<CreateStringField>g__GetPrefabTags|37_8(CS$<>8__locals1.it);
				guitextBox.OverflowClip = true;
				guitextBox.ToolTip = TextManager.Get("predefinedtags.description");
				new GUIButton(new RectTransform(new Vector2(0.1f, 1f), CS$<>8__locals1.frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), "...", Alignment.Center, "", null).OnClicked = delegate(GUIButton _, object _)
				{
					CS$<>8__locals1.it.CreateContainerTagPicker(CS$<>8__locals1.propertyBox);
					return true;
				};
			}
			CS$<>8__locals1.frame.RectTransform.MinSize = new Point(0, CS$<>8__locals1.frame.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			if (!this.Fields.ContainsKey(CS$<>8__locals1.property.Name))
			{
				this.Fields.Add(CS$<>8__locals1.property.Name.ToIdentifier(), new GUIComponent[]
				{
					CS$<>8__locals1.propertyBox
				});
			}
			return CS$<>8__locals1.frame;
		}

		// Token: 0x060027B1 RID: 10161 RVA: 0x001B9108 File Offset: 0x001B7308
		public GUIComponent CreatePointField(ISerializableEntity entity, SerializableProperty property, Point value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			Editable editableAttribute = property.GetAttribute<Editable>();
			GUIComponent[] fields = new GUIComponent[2];
			for (int i = 1; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.45f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				LocalizedString componentLabel = GUI.VectorComponentLabels[i];
				if (editableAttribute.VectorComponentLabels != null && i < editableAttribute.VectorComponentLabels.Length)
				{
					componentLabel = TextManager.Get(editableAttribute.VectorComponentLabels[i]);
				}
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = componentLabel;
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Center, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				if (i == 0)
				{
					numberInput.IntValue = value.X;
				}
				else
				{
					numberInput.IntValue = value.Y;
				}
				numberInput.MinValueInt = new int?(editableAttribute.MinValueInt);
				numberInput.MaxValueInt = new int?(editableAttribute.MaxValueInt);
				int comp = i;
				GUINumberInput guinumberInput = numberInput;
				guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
				{
					Point newVal = (Point)property.GetValue(entity);
					if (comp == 0)
					{
						newVal.X = numInput.IntValue;
					}
					else
					{
						newVal.Y = numInput.IntValue;
					}
					if (this.SetPropertyValue(property, entity, newVal))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
					}
					this.UpdateTextColors(property, entity, frame);
				}));
				fields[i] = numberInput;
			}
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!fields.Any((GUIComponent f) => ((GUINumberInput)f).TextBox.Selected))
				{
					Point value2 = (Point)property.GetValue(entity);
					((GUINumberInput)fields[0]).IntValue = value2.X;
					((GUINumberInput)fields[1]).IntValue = value2.Y;
				}
			}));
			frame.RectTransform.MinSize = new Point(0, frame.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), fields);
			}
			return frame;
		}

		// Token: 0x060027B2 RID: 10162 RVA: 0x001B9528 File Offset: 0x001B7728
		public GUIComponent CreateVector2Field(ISerializableEntity entity, SerializableProperty property, Vector2 value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f - this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(this.inputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			Editable editableAttribute = property.GetAttribute<Editable>();
			GUIComponent[] fields = new GUIComponent[2];
			for (int i = 1; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.45f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				LocalizedString componentLabel = GUI.VectorComponentLabels[i];
				if (editableAttribute.VectorComponentLabels != null && i < editableAttribute.VectorComponentLabels.Length)
				{
					componentLabel = TextManager.Get(editableAttribute.VectorComponentLabels[i]);
				}
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = componentLabel;
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Center, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				numberInput.MinValueFloat = new float?(editableAttribute.MinValueFloat);
				numberInput.MaxValueFloat = new float?(editableAttribute.MaxValueFloat);
				numberInput.DecimalsToDisplay = editableAttribute.DecimalCount;
				numberInput.ValueStep = editableAttribute.ValueStep;
				numberInput.PlusMinusButtonVisibility = (editableAttribute.ForceShowPlusMinusButtons ? GUINumberInput.ButtonVisibility.ForceVisible : GUINumberInput.ButtonVisibility.Automatic);
				numberInput.FloatValue = ((i == 0) ? value.X : value.Y);
				int comp = i;
				GUINumberInput guinumberInput = numberInput;
				guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
				{
					Vector2 newVal = (Vector2)property.GetValue(entity);
					if (comp == 0)
					{
						newVal.X = numInput.FloatValue;
					}
					else
					{
						newVal.Y = numInput.FloatValue;
					}
					if (this.SetPropertyValue(property, entity, newVal))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
					}
					this.UpdateTextColors(property, entity, frame);
				}));
				SerializableEntityEditor.HandleSetterValueTampering(numberInput, delegate
				{
					Vector2 currVal = (Vector2)property.GetValue(entity);
					if (comp != 0)
					{
						return currVal.Y;
					}
					return currVal.X;
				});
				fields[i] = numberInput;
			}
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!fields.Any((GUIComponent f) => ((GUINumberInput)f).TextBox.Selected))
				{
					Vector2 value2 = (Vector2)property.GetValue(entity);
					((GUINumberInput)fields[0]).FloatValue = value2.X;
					((GUINumberInput)fields[1]).FloatValue = value2.Y;
				}
			}));
			frame.RectTransform.MinSize = new Point(0, frame.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), fields);
			}
			return frame;
		}

		// Token: 0x060027B3 RID: 10163 RVA: 0x001B9984 File Offset: 0x001B7B84
		public GUIComponent CreateVector3Field(ISerializableEntity entity, SerializableProperty property, Vector3 value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f - this.largeInputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(this.largeInputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.03f
			};
			Editable editableAttribute = property.GetAttribute<Editable>();
			GUIComponent[] fields = new GUIComponent[3];
			for (int i = 2; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.33f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				LocalizedString componentLabel = GUI.VectorComponentLabels[i];
				if (editableAttribute.VectorComponentLabels != null && i < editableAttribute.VectorComponentLabels.Length)
				{
					componentLabel = TextManager.Get(editableAttribute.VectorComponentLabels[i]);
				}
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = componentLabel;
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Center, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				numberInput.MinValueFloat = new float?(editableAttribute.MinValueFloat);
				numberInput.MaxValueFloat = new float?(editableAttribute.MaxValueFloat);
				numberInput.DecimalsToDisplay = editableAttribute.DecimalCount;
				numberInput.ValueStep = editableAttribute.ValueStep;
				if (i == 0)
				{
					numberInput.FloatValue = value.X;
				}
				else if (i == 1)
				{
					numberInput.FloatValue = value.Y;
				}
				else if (i == 2)
				{
					numberInput.FloatValue = value.Z;
				}
				int comp = i;
				GUINumberInput guinumberInput = numberInput;
				guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
				{
					Vector3 newVal = (Vector3)property.GetValue(entity);
					if (comp == 0)
					{
						newVal.X = numInput.FloatValue;
					}
					else if (comp == 1)
					{
						newVal.Y = numInput.FloatValue;
					}
					else
					{
						newVal.Z = numInput.FloatValue;
					}
					if (this.SetPropertyValue(property, entity, newVal))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
					}
					this.UpdateTextColors(property, entity, frame);
				}));
				fields[i] = numberInput;
			}
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!fields.Any((GUIComponent f) => ((GUINumberInput)f).TextBox.Selected))
				{
					Vector3 value2 = (Vector3)property.GetValue(entity);
					((GUINumberInput)fields[0]).FloatValue = value2.X;
					((GUINumberInput)fields[1]).FloatValue = value2.Y;
					((GUINumberInput)fields[2]).FloatValue = value2.Z;
				}
			}));
			frame.RectTransform.MinSize = new Point(0, frame.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), fields);
			}
			return frame;
		}

		// Token: 0x060027B4 RID: 10164 RVA: 0x001B9DD8 File Offset: 0x001B7FD8
		public GUIComponent CreateVector4Field(ISerializableEntity entity, SerializableProperty property, Vector4 value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f - this.largeInputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			Editable editableAttribute = property.GetAttribute<Editable>();
			GUIComponent[] fields = new GUIComponent[4];
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(this.largeInputFieldWidth, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			for (int i = 3; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.22f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(50, 0),
					MaxSize = new Point(150, 50)
				}, null, null);
				LocalizedString componentLabel = GUI.VectorComponentLabels[i];
				if (editableAttribute.VectorComponentLabels != null && i < editableAttribute.VectorComponentLabels.Length)
				{
					componentLabel = TextManager.Get(editableAttribute.VectorComponentLabels[i]);
				}
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = componentLabel;
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Center, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				numberInput.MinValueFloat = new float?(editableAttribute.MinValueFloat);
				numberInput.MaxValueFloat = new float?(editableAttribute.MaxValueFloat);
				numberInput.DecimalsToDisplay = editableAttribute.DecimalCount;
				numberInput.ValueStep = editableAttribute.ValueStep;
				if (i == 0)
				{
					numberInput.FloatValue = value.X;
				}
				else if (i == 1)
				{
					numberInput.FloatValue = value.Y;
				}
				else if (i == 2)
				{
					numberInput.FloatValue = value.Z;
				}
				else
				{
					numberInput.FloatValue = value.W;
				}
				int comp = i;
				GUINumberInput guinumberInput = numberInput;
				guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
				{
					Vector4 newVal = (Vector4)property.GetValue(entity);
					if (comp == 0)
					{
						newVal.X = numInput.FloatValue;
					}
					else if (comp == 1)
					{
						newVal.Y = numInput.FloatValue;
					}
					else if (comp == 2)
					{
						newVal.Z = numInput.FloatValue;
					}
					else
					{
						newVal.W = numInput.FloatValue;
					}
					if (this.SetPropertyValue(property, entity, newVal))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
					}
					this.UpdateTextColors(property, entity, frame);
				}));
				fields[i] = numberInput;
			}
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!fields.Any((GUIComponent f) => ((GUINumberInput)f).TextBox.Selected))
				{
					Vector4 value2 = (Vector4)property.GetValue(entity);
					((GUINumberInput)fields[0]).FloatValue = value2.X;
					((GUINumberInput)fields[1]).FloatValue = value2.Y;
					((GUINumberInput)fields[2]).FloatValue = value2.Z;
					((GUINumberInput)fields[3]).FloatValue = value2.W;
				}
			}));
			frame.RectTransform.MinSize = new Point(0, frame.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), fields);
			}
			return frame;
		}

		// Token: 0x060027B5 RID: 10165 RVA: 0x001BA25C File Offset: 0x001B845C
		public GUIComponent CreateColorField(ISerializableEntity entity, SerializableProperty property, Color value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectTransform = new RectTransform(new Vector2(1f - this.largeInputFieldWidth, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(80, 26);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock label = new GUITextBlock(rectTransform, text, null, smallFont, Alignment.Left, false, "", null)
			{
				ToolTip = displayName + '\n' + toolTip
			};
			label.Text = ToolBox.LimitString(label.Text, label.Font, label.Rect.Width);
			GUIFrame colorBoxBack = new GUIFrame(new RectTransform(new Vector2(0.04f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(label.Rect.Width, 0)
			}, null, new Color?(Color.Black));
			GUIButton colorBox = new GUIButton(new RectTransform(new Vector2(this.largeInputFieldWidth, 0.9f), colorBoxBack.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
			{
				UserData = "colorpreview",
				OnClicked = delegate(GUIButton component, object data)
				{
					if (!SubEditorScreen.IsSubEditor())
					{
						return false;
					}
					if (GUIMessageBox.MessageBoxes.Any(delegate(GUIComponent msgBox)
					{
						GUIMessageBox guimessageBox = msgBox as GUIMessageBox;
						if (guimessageBox != null && !guimessageBox.Closed)
						{
							string text3 = msgBox.UserData as string;
							if (text3 != null)
							{
								return text3 == "colorpicker";
							}
						}
						return false;
					}))
					{
						return false;
					}
					GUIMessageBox msgBox2 = SubEditorScreen.CreatePropertyColorPicker((Color)property.GetValue(entity), property, entity);
					return true;
				}
			};
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(Math.Max((float)(frame.Rect.Width - label.Rect.Width - colorBoxBack.Rect.Width) / (float)frame.Rect.Width, 0.5f), 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.001f
			};
			GUIComponent[] fields = new GUIComponent[4];
			for (int i = 3; i >= 0; i--)
			{
				GUILayoutGroup element = new GUILayoutGroup(new RectTransform(new Vector2(0.18f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true
				};
				RectTransform rectTransform2 = new RectTransform(new Vector2(0.2f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				rectTransform2.MinSize = new Point(15, 0);
				RichString text2 = GUI.ColorComponentLabels[i];
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectTransform2, text2, null, smallFont, Alignment.Center, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				numberInput.MinValueInt = new int?(0);
				numberInput.MaxValueInt = new int?(255);
				if (i == 0)
				{
					numberInput.IntValue = (int)value.R;
				}
				else if (i == 1)
				{
					numberInput.IntValue = (int)value.G;
				}
				else if (i == 2)
				{
					numberInput.IntValue = (int)value.B;
				}
				else
				{
					numberInput.IntValue = (int)value.A;
				}
				numberInput.Font = GUIStyle.SmallFont;
				int comp = i;
				GUINumberInput guinumberInput = numberInput;
				guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
				{
					Color newVal = (Color)property.GetValue(entity);
					if (comp == 0)
					{
						newVal.R = (byte)numInput.IntValue;
					}
					else if (comp == 1)
					{
						newVal.G = (byte)numInput.IntValue;
					}
					else if (comp == 2)
					{
						newVal.B = (byte)numInput.IntValue;
					}
					else
					{
						newVal.A = (byte)numInput.IntValue;
					}
					if (this.SetPropertyValue(property, entity, newVal))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
						colorBox.Color = (colorBox.HoverColor = (colorBox.PressedColor = (colorBox.SelectedTextColor = newVal)));
					}
					this.UpdateTextColors(property, entity, frame);
				}));
				colorBox.Color = (colorBox.HoverColor = (colorBox.PressedColor = (colorBox.SelectedTextColor = (Color)property.GetValue(entity))));
				fields[i] = numberInput;
			}
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!fields.Any((GUIComponent f) => ((GUINumberInput)f).TextBox.Selected))
				{
					Color value2 = (Color)property.GetValue(entity);
					((GUINumberInput)fields[0]).IntValue = (int)value2.R;
					((GUINumberInput)fields[1]).IntValue = (int)value2.G;
					((GUINumberInput)fields[2]).IntValue = (int)value2.B;
					((GUINumberInput)fields[3]).IntValue = (int)value2.A;
				}
			}));
			frame.RectTransform.MinSize = new Point(0, frame.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), fields);
			}
			return frame;
		}

		// Token: 0x060027B6 RID: 10166 RVA: 0x001BA85C File Offset: 0x001B8A5C
		public GUIComponent CreateRectangleField(ISerializableEntity entity, SerializableProperty property, Rectangle value, LocalizedString displayName, LocalizedString toolTip)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, Math.Max(this.elementHeight, 26)), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(0.25f, 1f), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock label = new GUITextBlock(rectT, text, null, smallFont, Alignment.Left, false, "", null)
			{
				ToolTip = displayName + '\n' + toolTip
			};
			label.Text = ToolBox.LimitString(label.Text, label.Font, label.Rect.Width);
			GUIComponent[] fields = new GUIComponent[4];
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 1f), frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), true, Anchor.CenterRight)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			for (int i = 3; i >= 0; i--)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.22f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(50, 0),
					MaxSize = new Point(150, 50)
				}, null, null);
				RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = GUI.RectComponentLabels[i];
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectT2, text2, null, smallFont, Alignment.Center, false, "", null);
				GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					Font = GUIStyle.SmallFont
				};
				numberInput.MinValueInt = new int?(0);
				numberInput.MaxValueInt = new int?(9999);
				if (i == 0)
				{
					numberInput.IntValue = value.X;
				}
				else if (i == 1)
				{
					numberInput.IntValue = value.Y;
				}
				else if (i == 2)
				{
					numberInput.IntValue = value.Width;
				}
				else
				{
					numberInput.IntValue = value.Height;
				}
				int comp = i;
				GUINumberInput guinumberInput = numberInput;
				guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
				{
					Rectangle newVal = (Rectangle)property.GetValue(entity);
					if (comp == 0)
					{
						newVal.X = numInput.IntValue;
					}
					else if (comp == 1)
					{
						newVal.Y = numInput.IntValue;
					}
					else if (comp == 2)
					{
						newVal.Width = numInput.IntValue;
					}
					else
					{
						newVal.Height = numInput.IntValue;
					}
					if (this.SetPropertyValue(property, entity, newVal))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
					}
					this.UpdateTextColors(property, entity, frame);
				}));
				fields[i] = numberInput;
			}
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (!fields.Any((GUIComponent f) => ((GUINumberInput)f).TextBox.Selected))
				{
					Rectangle value2 = (Rectangle)property.GetValue(entity);
					((GUINumberInput)fields[0]).IntValue = value2.X;
					((GUINumberInput)fields[1]).IntValue = value2.Y;
					((GUINumberInput)fields[2]).IntValue = value2.Width;
					((GUINumberInput)fields[3]).IntValue = value2.Height;
				}
			}));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), fields);
			}
			return frame;
		}

		// Token: 0x060027B7 RID: 10167 RVA: 0x001BAC6C File Offset: 0x001B8E6C
		public GUIComponent CreateStringArrayField(ISerializableEntity entity, SerializableProperty property, string[] value, LocalizedString displayName, LocalizedString toolTip)
		{
			int elementCount = value.Length + 1;
			GUIFrame frame = new GUIFrame(new RectTransform(new Point(this.Rect.Width, elementCount * this.elementHeight), this.layoutGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), "", new Color?(Color.Transparent));
			RectTransform rectT = new RectTransform(new Vector2(1f, 1f / (float)elementCount), frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = displayName;
			GUIFont smallFont = GUIStyle.SmallFont;
			new GUITextBlock(rectT, text3, null, smallFont, Alignment.Left, false, "", null).ToolTip = toolTip;
			Editable editableAttribute = property.GetAttribute<Editable>();
			GUIComponent[] fields = new GUIComponent[value.Length];
			GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(1f, (float)(elementCount - 1) / (float)elementCount), frame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f
			};
			elementCount--;
			for (int i = 0; i < value.Length; i++)
			{
				GUIFrame element = new GUIFrame(new RectTransform(new Vector2(1f, 1f / (float)elementCount), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(50, 0),
					MaxSize = new Point((int)(0.9f * (float)inputArea.Rect.Width), 50)
				}, null, null);
				GUILayoutGroup elementLayoutGroup = new GUILayoutGroup(new RectTransform(Vector2.One, element.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
				string componentLabel = (i + 1).ToString();
				RectTransform rectTransform = new RectTransform(new Vector2(0.3f, 1f), elementLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				rectTransform.MaxSize = new Point(25, elementLayoutGroup.Rect.Height);
				RichString text2 = componentLabel;
				smallFont = GUIStyle.SmallFont;
				new GUITextBlock(rectTransform, text2, null, smallFont, Alignment.Center, false, "", null);
				GUITextBox textBox2 = new GUITextBox(new RectTransform(new Vector2(0.7f, 1f), elementLayoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), value[i], null, null, Alignment.Left, false, "", null, false, true)
				{
					Font = GUIStyle.SmallFont,
					Readonly = this.Readonly
				};
				int comp = i;
				GUITextBox guitextBox = textBox2;
				guitextBox.OnEnterPressed = (GUITextBox.OnEnterHandler)Delegate.Combine(guitextBox.OnEnterPressed, new GUITextBox.OnEnterHandler((GUITextBox textBox, string text) => base.<CreateStringArrayField>g__OnApply|3(textBox)));
				textBox2.OnDeselected += delegate(GUITextBox textBox, Keys keys)
				{
					base.<CreateStringArrayField>g__OnApply|3(textBox);
				};
				fields[i] = textBox2;
			}
			this.refresh = (Action)Delegate.Combine(this.refresh, new Action(delegate()
			{
				if (fields.None((GUIComponent f) => ((GUITextBox)f).Selected))
				{
					string[] value2 = (string[])property.GetValue(entity);
					for (int j = 0; j < fields.Length; j++)
					{
						((GUITextBox)fields[j]).Text = value2[j];
					}
				}
			}));
			frame.RectTransform.MinSize = new Point(0, frame.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y));
			if (!this.Fields.ContainsKey(property.Name))
			{
				this.Fields.Add(property.Name.ToIdentifier(), fields);
			}
			return frame;
		}

		// Token: 0x060027B8 RID: 10168 RVA: 0x001BB0D0 File Offset: 0x001B92D0
		public void CreateTextPicker(string textTag, ISerializableEntity entity, SerializableProperty property, GUITextBox textBox)
		{
			GUIMessageBox msgBox = new GUIMessageBox("", "", new LocalizedString[]
			{
				TextManager.Get("Ok")
			}, new Vector2?(new Vector2(0.2f, 0.5f)), new Point?(new Point(300, 400)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
			GUIListBox textList = new GUIListBox(new RectTransform(new Vector2(1f, 0.8f), msgBox.Content.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = delegate(GUIComponent component, object userData)
				{
					string text = (userData as string) ?? "";
					if (this.SetPropertyValue(property, entity, text))
					{
						SerializableEntityEditor.TrySendNetworkUpdate(entity, property);
						textBox.Text = (string)property.GetValue(entity);
						textBox.Deselect();
					}
					return true;
				}
			};
			List<KeyValuePair<Identifier, string>> tagTextPairs = TextManager.GetAllTagTextPairs().ToList<KeyValuePair<Identifier, string>>();
			tagTextPairs.Sort((KeyValuePair<Identifier, string> t1, KeyValuePair<Identifier, string> t2) => t1.Value.CompareTo(t2.Value));
			foreach (KeyValuePair<Identifier, string> tagTextPair in tagTextPairs)
			{
				if (tagTextPair.Key.StartsWith(textTag))
				{
					new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), textList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 20)
					}, ToolBox.LimitString(tagTextPair.Value, GUIStyle.Font, textList.Content.Rect.Width), null, null, Alignment.Left, false, "", null).UserData = tagTextPair.Key.ToString();
				}
			}
			IHasExtraTextPickerEntries hasExtraTextPickerEntries = entity as IHasExtraTextPickerEntries;
			if (hasExtraTextPickerEntries != null)
			{
				foreach (string extraEntry in hasExtraTextPickerEntries.GetExtraTextPickerEntries())
				{
					new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), textList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 20)
					}, ToolBox.LimitString(extraEntry, GUIStyle.Font, textList.Content.Rect.Width), new Color?(GUIStyle.Green), null, Alignment.Left, false, "", null).UserData = extraEntry;
				}
			}
		}

		// Token: 0x060027B9 RID: 10169 RVA: 0x001BB41C File Offset: 0x001B961C
		private static void TrySendNetworkUpdate(ISerializableEntity entity, SerializableProperty property)
		{
			if (SerializableEntityEditor.IsEntityRemoved(entity))
			{
				return;
			}
			if (GameMain.Client != null)
			{
				Item item = entity as Item;
				if (item != null)
				{
					GameMain.Client.CreateEntityEvent(item, new Item.ChangePropertyEventData(property, item));
					return;
				}
				ItemComponent ic = entity as ItemComponent;
				if (ic != null)
				{
					GameMain.Client.CreateEntityEvent(ic.Item, new Item.ChangePropertyEventData(property, ic));
				}
			}
		}

		// Token: 0x060027BA RID: 10170 RVA: 0x001BB484 File Offset: 0x001B9684
		private bool SetPropertyValue(SerializableProperty property, object entity, object value)
		{
			if (SerializableEntityEditor.LockEditing || SerializableEntityEditor.IsEntityRemoved(entity) || this.Readonly)
			{
				return false;
			}
			object oldData = property.GetValue(entity);
			if (oldData == null && value is string)
			{
				oldData = "";
			}
			ISerializableEntity sEntity = entity as ISerializableEntity;
			if (sEntity != null && Screen.Selected is SubEditorScreen && !object.Equals(oldData, value))
			{
				List<ISerializableEntity> entities = new List<ISerializableEntity>
				{
					sEntity
				};
				Dictionary<ISerializableEntity, object> affected = this.MultiSetProperties(property, entity, value);
				Dictionary<object, List<ISerializableEntity>> oldValues = new Dictionary<object, List<ISerializableEntity>>
				{
					{
						oldData,
						new List<ISerializableEntity>
						{
							sEntity
						}
					}
				};
				affected.ForEach(delegate(KeyValuePair<ISerializableEntity, object> aEntity)
				{
					KeyValuePair<ISerializableEntity, object> keyValuePair = aEntity;
					ISerializableEntity serializableEntity;
					object obj;
					keyValuePair.Deconstruct(out serializableEntity, out obj);
					ISerializableEntity item = serializableEntity;
					object oldVal = obj;
					entities.Add(item);
					if (!oldValues.ContainsKey(oldVal))
					{
						oldValues.Add(oldVal, new List<ISerializableEntity>
						{
							item
						});
						return;
					}
					oldValues[oldVal].Add(item);
				});
				PropertyCommand cmd = new PropertyCommand(entities, property.Name.ToIdentifier(), value, oldValues);
				if (SerializableEntityEditor.CommandBuffer != null)
				{
					if (SerializableEntityEditor.CommandBuffer.Item1 == property && SerializableEntityEditor.CommandBuffer.Item2.PropertyCount == cmd.PropertyCount)
					{
						if (!SerializableEntityEditor.CommandBuffer.Item2.MergeInto(cmd))
						{
							SerializableEntityEditor.CommitCommandBuffer();
						}
					}
					else
					{
						SerializableEntityEditor.CommitCommandBuffer();
					}
				}
				SerializableEntityEditor.NextCommandPush = DateTime.Now.AddSeconds(1.0);
				SerializableEntityEditor.CommandBuffer = Tuple.Create<SerializableProperty, PropertyCommand>(property, cmd);
				SerializableEntityEditor.PropertyChangesActive = true;
			}
			return property.TrySetValue(entity, value);
		}

		// Token: 0x060027BB RID: 10171 RVA: 0x001BB5E0 File Offset: 0x001B97E0
		public static bool IsEntityRemoved(object entity)
		{
			Entity entity2 = entity as Entity;
			if (entity2 != null)
			{
				if (!entity2.Removed)
				{
					goto IL_3B;
				}
			}
			else
			{
				ItemComponent itemComponent = entity as ItemComponent;
				if (itemComponent == null)
				{
					goto IL_3B;
				}
				Item item = itemComponent.Item;
				if (item == null)
				{
					goto IL_3B;
				}
				bool removed = item.Removed;
				if (!removed)
				{
					goto IL_3B;
				}
			}
			return true;
			IL_3B:
			return false;
		}

		// Token: 0x060027BC RID: 10172 RVA: 0x001BB62D File Offset: 0x001B982D
		public static void CommitCommandBuffer()
		{
			if (SerializableEntityEditor.CommandBuffer != null)
			{
				SubEditorScreen.StoreCommand(SerializableEntityEditor.CommandBuffer.Item2);
			}
			SerializableEntityEditor.CommandBuffer = null;
			SerializableEntityEditor.PropertyChangesActive = false;
		}

		// Token: 0x060027BD RID: 10173 RVA: 0x001BB654 File Offset: 0x001B9854
		private Dictionary<ISerializableEntity, object> MultiSetProperties(SerializableProperty property, object parentObject, object value)
		{
			SerializableEntityEditor.<>c__DisplayClass50_0 CS$<>8__locals1 = new SerializableEntityEditor.<>c__DisplayClass50_0();
			CS$<>8__locals1.parentObject = parentObject;
			CS$<>8__locals1.affected = new Dictionary<ISerializableEntity, object>();
			if (!(Screen.Selected is SubEditorScreen) || MapEntity.SelectedList.Count <= 1)
			{
				return CS$<>8__locals1.affected;
			}
			if (!(CS$<>8__locals1.parentObject is ItemComponent) && !(CS$<>8__locals1.parentObject is Item) && !(CS$<>8__locals1.parentObject is Structure) && !(CS$<>8__locals1.parentObject is Hull))
			{
				return CS$<>8__locals1.affected;
			}
			IEnumerable<MapEntity> selectedList = MapEntity.SelectedList;
			Func<MapEntity, bool> predicate;
			if ((predicate = CS$<>8__locals1.<>9__1) == null)
			{
				predicate = (CS$<>8__locals1.<>9__1 = ((MapEntity entity) => entity != CS$<>8__locals1.parentObject));
			}
			foreach (MapEntity entity2 in selectedList.Where(predicate))
			{
				object parentObject2 = CS$<>8__locals1.parentObject;
				if (!(parentObject2 is Hull) && !(parentObject2 is Structure) && !(parentObject2 is Item))
				{
					ItemComponent <parentComponent>5__2 = parentObject2 as ItemComponent;
					if (<parentComponent>5__2 != null)
					{
						Item otherItem = entity2 as Item;
						if (otherItem != null && otherItem != <parentComponent>5__2.Item)
						{
							int componentIndex = <parentComponent>5__2.Item.Components.FindAll((ItemComponent c) => c.GetType() == <parentComponent>5__2.GetType()).IndexOf(<parentComponent>5__2);
							List<ItemComponent> otherComponents = otherItem.Components.FindAll((ItemComponent c) => c.GetType() == <parentComponent>5__2.GetType());
							if (componentIndex >= 0 && componentIndex < otherComponents.Count)
							{
								ItemComponent component = otherComponents[componentIndex];
								CS$<>8__locals1.<MultiSetProperties>g__SafeAdd|0(component, property);
								string stringValue = value as string;
								object enumValue;
								if (stringValue != null && property.PropertyType.IsEnum && Enum.TryParse(property.PropertyType, stringValue, out enumValue))
								{
									property.PropertyInfo.SetValue(component, enumValue);
								}
								else
								{
									try
									{
										property.PropertyInfo.SetValue(component, value);
									}
									catch (ArgumentException e)
									{
										DebugConsole.ThrowError("Failed to set the value of the property \"" + property.Name + "\" to " + (((value != null) ? value.ToString() : null) ?? "null"), e, null, false, false);
									}
								}
							}
						}
					}
				}
				else if (entity2.GetType() == CS$<>8__locals1.parentObject.GetType())
				{
					CS$<>8__locals1.<MultiSetProperties>g__SafeAdd|0((ISerializableEntity)entity2, property);
					property.PropertyInfo.SetValue(entity2, value);
				}
				else
				{
					ISerializableEntity sEntity = entity2 as ISerializableEntity;
					if (sEntity != null && sEntity.SerializableProperties != null)
					{
						Dictionary<Identifier, SerializableProperty> props = sEntity.SerializableProperties;
						SerializableProperty foundProp;
						if (props.TryGetValue(property.Name.ToIdentifier(), out foundProp) && foundProp.Attributes.OfType<Editable>().Any<Editable>())
						{
							CS$<>8__locals1.<MultiSetProperties>g__SafeAdd|0(sEntity, foundProp);
							foundProp.PropertyInfo.SetValue(entity2, value);
						}
					}
				}
			}
			return CS$<>8__locals1.affected;
		}

		// Token: 0x060027BE RID: 10174 RVA: 0x001BB96C File Offset: 0x001B9B6C
		[CompilerGenerated]
		internal static void <UpdateTextColors>g__SetTextColor|30_0(GUITextBlock textBlock, ref SerializableEntityEditor.<>c__DisplayClass30_0 A_1)
		{
			textBlock.TextColor = new Color(textBlock.TextColor, A_1.isSetToDefaultValue ? 0.5f : 1f);
		}

		// Token: 0x060027BF RID: 10175 RVA: 0x001BB994 File Offset: 0x001B9B94
		[CompilerGenerated]
		internal static bool <CreateStringField>g__IsItemTagBox|37_6(ISerializableEntity entity, string propertyName, [NotNullWhen(true)] out Item it)
		{
			Item item = entity as Item;
			if (item != null && propertyName.Equals("Tags", StringComparison.OrdinalIgnoreCase))
			{
				it = item;
				return true;
			}
			it = null;
			return false;
		}

		// Token: 0x060027C0 RID: 10176 RVA: 0x001BB9C2 File Offset: 0x001B9BC2
		[CompilerGenerated]
		internal static string <CreateStringField>g__GetPrefabTags|37_8(Item it)
		{
			return string.Join<Identifier>(',', it.Prefab.Tags);
		}

		// Token: 0x04001432 RID: 5170
		private readonly int elementHeight;

		// Token: 0x04001433 RID: 5171
		private readonly GUILayoutGroup layoutGroup;

		// Token: 0x04001434 RID: 5172
		private readonly float inputFieldWidth = 0.5f;

		// Token: 0x04001435 RID: 5173
		private readonly float largeInputFieldWidth = 0.8f;

		// Token: 0x04001436 RID: 5174
		public static bool LockEditing;

		// Token: 0x04001437 RID: 5175
		public static bool PropertyChangesActive;

		// Token: 0x04001438 RID: 5176
		public static DateTime NextCommandPush;

		// Token: 0x04001439 RID: 5177
		public static Tuple<SerializableProperty, PropertyCommand> CommandBuffer;

		// Token: 0x0400143A RID: 5178
		private bool dimOutDefaultValues;

		// Token: 0x0400143B RID: 5179
		private bool isReadonly;

		// Token: 0x0400143C RID: 5180
		private Action refresh;
	}
}
