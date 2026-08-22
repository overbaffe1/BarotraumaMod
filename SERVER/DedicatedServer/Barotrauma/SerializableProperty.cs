using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000282 RID: 642
	public sealed class SerializableProperty
	{
		// Token: 0x06002D36 RID: 11574 RVA: 0x00129AD8 File Offset: 0x00127CD8
		public SerializableProperty(PropertyDescriptor property)
		{
			Dictionary<Identifier, Func<object, object>> dictionary = new Dictionary<Identifier, Func<object, object>>();
			dictionary.Add("Voltage".ToIdentifier(), delegate(object obj)
			{
				Powered p = obj as Powered;
				if (p == null)
				{
					return null;
				}
				return p.Voltage;
			});
			dictionary.Add("Charge".ToIdentifier(), delegate(object obj)
			{
				PowerContainer p = obj as PowerContainer;
				if (p == null)
				{
					return null;
				}
				return p.Charge;
			});
			dictionary.Add("Overload".ToIdentifier(), delegate(object obj)
			{
				PowerTransfer p = obj as PowerTransfer;
				if (p == null)
				{
					return null;
				}
				return p.Overload;
			});
			dictionary.Add("AvailableFuel".ToIdentifier(), delegate(object obj)
			{
				Reactor r = obj as Reactor;
				if (r == null)
				{
					return null;
				}
				return r.AvailableFuel;
			});
			dictionary.Add("FissionRate".ToIdentifier(), delegate(object obj)
			{
				Reactor r = obj as Reactor;
				if (r == null)
				{
					return null;
				}
				return r.FissionRate;
			});
			dictionary.Add("OxygenFlow".ToIdentifier(), delegate(object obj)
			{
				Vent v = obj as Vent;
				if (v == null)
				{
					return null;
				}
				return v.OxygenFlow;
			});
			dictionary.Add("CurrFlow".ToIdentifier(), delegate(object obj)
			{
				Pump p = obj as Pump;
				if (p != null)
				{
					return p.CurrFlow;
				}
				OxygenGenerator o = obj as OxygenGenerator;
				if (o == null)
				{
					return null;
				}
				return o.CurrFlow;
			});
			dictionary.Add("CurrentVolume".ToIdentifier(), delegate(object obj)
			{
				Engine e = obj as Engine;
				if (e == null)
				{
					return null;
				}
				return e.CurrentVolume;
			});
			dictionary.Add("MotionDetected".ToIdentifier(), delegate(object obj)
			{
				MotionSensor i = obj as MotionSensor;
				if (i == null)
				{
					return null;
				}
				return i.MotionDetected;
			});
			dictionary.Add("Oxygen".ToIdentifier(), delegate(object obj)
			{
				Character c = obj as Character;
				if (c == null)
				{
					return null;
				}
				return c.Oxygen;
			});
			dictionary.Add("Health".ToIdentifier(), delegate(object obj)
			{
				Character c = obj as Character;
				if (c == null)
				{
					return null;
				}
				return c.Health;
			});
			dictionary.Add("OxygenAvailable".ToIdentifier(), delegate(object obj)
			{
				Character c = obj as Character;
				if (c == null)
				{
					return null;
				}
				return c.OxygenAvailable;
			});
			dictionary.Add("PressureProtection".ToIdentifier(), delegate(object obj)
			{
				Character c = obj as Character;
				if (c == null)
				{
					return null;
				}
				return c.PressureProtection;
			});
			dictionary.Add("IsDead".ToIdentifier(), delegate(object obj)
			{
				Character c = obj as Character;
				if (c == null)
				{
					return null;
				}
				return c.IsDead;
			});
			dictionary.Add("IsHuman".ToIdentifier(), delegate(object obj)
			{
				Character c = obj as Character;
				if (c == null)
				{
					return null;
				}
				return c.IsHuman;
			});
			dictionary.Add("IsOn".ToIdentifier(), delegate(object obj)
			{
				LightComponent i = obj as LightComponent;
				if (i == null)
				{
					return null;
				}
				return i.IsOn;
			});
			dictionary.Add("Condition".ToIdentifier(), delegate(object obj)
			{
				Item i = obj as Item;
				if (i == null)
				{
					return null;
				}
				return i.Condition;
			});
			dictionary.Add("ContainerIdentifier".ToIdentifier(), delegate(object obj)
			{
				Item i = obj as Item;
				if (i == null)
				{
					return null;
				}
				return i.ContainerIdentifier;
			});
			dictionary.Add("PhysicsBodyActive".ToIdentifier(), delegate(object obj)
			{
				Item i = obj as Item;
				if (i == null)
				{
					return null;
				}
				return i.PhysicsBodyActive;
			});
			this.valueGetters = dictionary.ToImmutableDictionary<Identifier, Func<object, object>>();
			base..ctor();
			this.Name = property.Name;
			this.PropertyInfo = property.ComponentType.GetProperty(property.Name);
			this.PropertyType = property.PropertyType;
			this.Attributes = property.Attributes;
			Serialize attribute = this.GetAttribute<Serialize>();
			this.OverridePrefabValues = (attribute != null && attribute.AlwaysUseInstanceValues);
		}

		// Token: 0x06002D37 RID: 11575 RVA: 0x00129ECC File Offset: 0x001280CC
		public T GetAttribute<T>() where T : Attribute
		{
			foreach (object obj in this.Attributes)
			{
				Attribute a = (Attribute)obj;
				if (a is T)
				{
					return (T)((object)a);
				}
			}
			return default(T);
		}

		// Token: 0x06002D38 RID: 11576 RVA: 0x00129F3C File Offset: 0x0012813C
		public void SetValue(object parentObject, object val)
		{
			this.PropertyInfo.SetValue(parentObject, val);
		}

		// Token: 0x06002D39 RID: 11577 RVA: 0x00129F4C File Offset: 0x0012814C
		public bool TrySetValue(object parentObject, string value)
		{
			if (value == null)
			{
				return false;
			}
			string typeName;
			if (!SerializableProperty.supportedTypes.TryGetValue(this.PropertyType, out typeName))
			{
				if (this.PropertyType.IsEnum)
				{
					object enumVal;
					try
					{
						enumVal = Enum.Parse(this.PropertyInfo.PropertyType, value, true);
					}
					catch (Exception e)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 4);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to set the value of the property \"");
						defaultInterpolatedStringHandler.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" of \"");
						defaultInterpolatedStringHandler.AppendFormatted<object>(parentObject);
						defaultInterpolatedStringHandler.AppendLiteral("\" to ");
						defaultInterpolatedStringHandler.AppendFormatted(value);
						defaultInterpolatedStringHandler.AppendLiteral(" (not a valid ");
						defaultInterpolatedStringHandler.AppendFormatted<Type>(this.PropertyInfo.PropertyType);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
						return false;
					}
					try
					{
						this.PropertyInfo.SetValue(parentObject, enumVal);
						goto IL_1D2;
					}
					catch (Exception e2)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(52, 3);
						defaultInterpolatedStringHandler2.AppendLiteral("Failed to set the value of the property \"");
						defaultInterpolatedStringHandler2.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\" of \"");
						defaultInterpolatedStringHandler2.AppendFormatted<object>(parentObject);
						defaultInterpolatedStringHandler2.AppendLiteral("\" to ");
						defaultInterpolatedStringHandler2.AppendFormatted(value);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), e2, null, false, false);
						return false;
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(76, 4);
				defaultInterpolatedStringHandler3.AppendLiteral("Failed to set the value of the property \"");
				defaultInterpolatedStringHandler3.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("\" of \"");
				defaultInterpolatedStringHandler3.AppendFormatted<object>(parentObject);
				defaultInterpolatedStringHandler3.AppendLiteral("\" to ");
				defaultInterpolatedStringHandler3.AppendFormatted(value);
				defaultInterpolatedStringHandler3.AppendLiteral(" (Type \"");
				defaultInterpolatedStringHandler3.AppendFormatted(this.PropertyType.Name);
				defaultInterpolatedStringHandler3.AppendLiteral("\" not supported)");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
				return false;
			}
			IL_1D2:
			try
			{
				if (typeName != null)
				{
					switch (typeName.Length)
					{
					case 3:
						if (typeName == "int")
						{
							int intVal;
							if (!int.TryParse(value, out intVal))
							{
								return false;
							}
							if (this.TrySetFloatValueWithoutReflection(parentObject, (float)intVal))
							{
								return true;
							}
							this.PropertyInfo.SetValue(parentObject, intVal, null);
						}
						break;
					case 4:
						if (typeName == "bool")
						{
							Identifier identifier = value.ToIdentifier();
							bool boolValue = identifier == "true";
							if (this.TrySetBoolValueWithoutReflection(parentObject, boolValue))
							{
								return true;
							}
							this.PropertyInfo.SetValue(parentObject, boolValue, null);
						}
						break;
					case 5:
					{
						char c = typeName[0];
						if (c != 'c')
						{
							if (c != 'f')
							{
								if (c == 'p')
								{
									if (typeName == "point")
									{
										this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParsePoint(value, true));
									}
								}
							}
							else if (typeName == "float")
							{
								float floatVal;
								if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out floatVal))
								{
									return false;
								}
								if (this.TrySetFloatValueWithoutReflection(parentObject, floatVal))
								{
									return true;
								}
								this.PropertyInfo.SetValue(parentObject, floatVal, null);
							}
						}
						else if (typeName == "color")
						{
							this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseColor(value, true));
						}
						break;
					}
					case 6:
						if (typeName == "string")
						{
							this.PropertyInfo.SetValue(parentObject, value, null);
						}
						break;
					case 7:
						switch (typeName[6])
						{
						case '2':
							if (typeName == "vector2")
							{
								this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseVector2(value, true));
							}
							break;
						case '3':
							if (typeName == "vector3")
							{
								this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseVector3(value, true));
							}
							break;
						case '4':
							if (typeName == "vector4")
							{
								this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseVector4(value, true));
							}
							break;
						}
						break;
					case 9:
						if (typeName == "rectangle")
						{
							this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseRect(value, true, true));
						}
						break;
					case 10:
						if (typeName == "identifier")
						{
							this.PropertyInfo.SetValue(parentObject, value.ToIdentifier());
						}
						break;
					case 11:
						if (typeName == "stringarray")
						{
							this.PropertyInfo.SetValue(parentObject, SerializableProperty.ParseStringArray(value));
						}
						break;
					case 15:
					{
						char c = typeName[0];
						if (c != 'i')
						{
							if (c == 'l')
							{
								if (typeName == "localizedstring")
								{
									this.PropertyInfo.SetValue(parentObject, new RawLString(value));
								}
							}
						}
						else if (typeName == "identifierarray")
						{
							this.PropertyInfo.SetValue(parentObject, SerializableProperty.ParseIdentifierArray(value));
						}
						break;
					}
					case 18:
						if (typeName == "languageidentifier")
						{
							this.PropertyInfo.SetValue(parentObject, value.ToLanguageIdentifier());
						}
						break;
					}
				}
			}
			catch (Exception e3)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(52, 3);
				defaultInterpolatedStringHandler4.AppendLiteral("Failed to set the value of the property \"");
				defaultInterpolatedStringHandler4.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler4.AppendLiteral("\" of \"");
				defaultInterpolatedStringHandler4.AppendFormatted<object>(parentObject);
				defaultInterpolatedStringHandler4.AppendLiteral("\" to ");
				defaultInterpolatedStringHandler4.AppendFormatted(value);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), e3, null, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x06002D3A RID: 11578 RVA: 0x0012A5D8 File Offset: 0x001287D8
		private static string[] ParseStringArray(string stringArrayValues)
		{
			if (!string.IsNullOrEmpty(stringArrayValues))
			{
				return stringArrayValues.Split(';', StringSplitOptions.None);
			}
			return Array.Empty<string>();
		}

		// Token: 0x06002D3B RID: 11579 RVA: 0x0012A5F1 File Offset: 0x001287F1
		private static Identifier[] ParseIdentifierArray(string stringArrayValues)
		{
			return SerializableProperty.ParseStringArray(stringArrayValues).ToIdentifiers();
		}

		// Token: 0x06002D3C RID: 11580 RVA: 0x0012A600 File Offset: 0x00128800
		public bool TrySetValue(object parentObject, object value)
		{
			if (value == null || parentObject == null || this.PropertyInfo == null)
			{
				return false;
			}
			bool result;
			try
			{
				string typeName;
				if (!SerializableProperty.supportedTypes.TryGetValue(this.PropertyType, out typeName))
				{
					if (this.PropertyType.IsEnum)
					{
						object enumVal;
						try
						{
							enumVal = Enum.Parse(this.PropertyInfo.PropertyType, value.ToString(), true);
						}
						catch (Exception e)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(67, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to set the value of the property \"");
							defaultInterpolatedStringHandler.AppendFormatted(this.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\" of \"");
							defaultInterpolatedStringHandler.AppendFormatted<object>(parentObject);
							defaultInterpolatedStringHandler.AppendLiteral("\" to ");
							defaultInterpolatedStringHandler.AppendFormatted<object>(value);
							defaultInterpolatedStringHandler.AppendLiteral(" (not a valid ");
							defaultInterpolatedStringHandler.AppendFormatted<Type>(this.PropertyInfo.PropertyType);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
							return false;
						}
						this.PropertyInfo.SetValue(parentObject, enumVal);
						result = true;
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(76, 4);
						defaultInterpolatedStringHandler2.AppendLiteral("Failed to set the value of the property \"");
						defaultInterpolatedStringHandler2.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\" of \"");
						defaultInterpolatedStringHandler2.AppendFormatted<object>(parentObject);
						defaultInterpolatedStringHandler2.AppendLiteral("\" to ");
						defaultInterpolatedStringHandler2.AppendFormatted<object>(value);
						defaultInterpolatedStringHandler2.AppendLiteral(" (Type \"");
						defaultInterpolatedStringHandler2.AppendFormatted(this.PropertyType.Name);
						defaultInterpolatedStringHandler2.AppendLiteral("\" not supported)");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
						result = false;
					}
				}
				else
				{
					try
					{
						if (value.GetType() == typeof(string))
						{
							if (typeName != null)
							{
								switch (typeName.Length)
								{
								case 5:
								{
									char c = typeName[0];
									if (c != 'c')
									{
										if (c == 'p')
										{
											if (typeName == "point")
											{
												this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParsePoint((string)value, true));
												return true;
											}
										}
									}
									else if (typeName == "color")
									{
										this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseColor((string)value, true));
										return true;
									}
									break;
								}
								case 6:
									if (typeName == "string")
									{
										this.PropertyInfo.SetValue(parentObject, value, null);
										return true;
									}
									break;
								case 7:
									switch (typeName[6])
									{
									case '2':
										if (typeName == "vector2")
										{
											this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseVector2((string)value, true));
											return true;
										}
										break;
									case '3':
										if (typeName == "vector3")
										{
											this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseVector3((string)value, true));
											return true;
										}
										break;
									case '4':
										if (typeName == "vector4")
										{
											this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseVector4((string)value, true));
											return true;
										}
										break;
									}
									break;
								case 9:
									if (typeName == "rectangle")
									{
										this.PropertyInfo.SetValue(parentObject, XMLExtensions.ParseRect((string)value, false, true));
										return true;
									}
									break;
								case 10:
									if (typeName == "identifier")
									{
										this.PropertyInfo.SetValue(parentObject, new Identifier((string)value));
										return true;
									}
									break;
								case 11:
									if (typeName == "stringarray")
									{
										this.PropertyInfo.SetValue(parentObject, SerializableProperty.ParseStringArray((string)value));
										return true;
									}
									break;
								case 15:
								{
									char c = typeName[0];
									if (c != 'i')
									{
										if (c == 'l')
										{
											if (typeName == "localizedstring")
											{
												this.PropertyInfo.SetValue(parentObject, new RawLString((string)value));
												return true;
											}
										}
									}
									else if (typeName == "identifierarray")
									{
										this.PropertyInfo.SetValue(parentObject, SerializableProperty.ParseIdentifierArray((string)value));
										return true;
									}
									break;
								}
								case 18:
									if (typeName == "languageidentifier")
									{
										this.PropertyInfo.SetValue(parentObject, ((string)value).ToLanguageIdentifier());
										return true;
									}
									break;
								}
							}
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(52, 3);
							defaultInterpolatedStringHandler3.AppendLiteral("Failed to set the value of the property \"");
							defaultInterpolatedStringHandler3.AppendFormatted(this.Name);
							defaultInterpolatedStringHandler3.AppendLiteral("\" of \"");
							defaultInterpolatedStringHandler3.AppendFormatted<object>(parentObject);
							defaultInterpolatedStringHandler3.AppendLiteral("\" to ");
							defaultInterpolatedStringHandler3.AppendFormatted<object>(value);
							DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(31, 1);
							defaultInterpolatedStringHandler4.AppendLiteral("(Cannot convert a string to a ");
							defaultInterpolatedStringHandler4.AppendFormatted<Type>(this.PropertyType);
							defaultInterpolatedStringHandler4.AppendLiteral(")");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
							return false;
						}
						if (this.PropertyType != value.GetType())
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(52, 3);
							defaultInterpolatedStringHandler5.AppendLiteral("Failed to set the value of the property \"");
							defaultInterpolatedStringHandler5.AppendFormatted(this.Name);
							defaultInterpolatedStringHandler5.AppendLiteral("\" of \"");
							defaultInterpolatedStringHandler5.AppendFormatted<object>(parentObject);
							defaultInterpolatedStringHandler5.AppendLiteral("\" to ");
							defaultInterpolatedStringHandler5.AppendFormatted<object>(value);
							DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), null, null, false, false);
							string[] array = new string[5];
							array[0] = "(Non-matching type, should be ";
							int num = 1;
							Type propertyType = this.PropertyType;
							array[num] = ((propertyType != null) ? propertyType.ToString() : null);
							array[2] = " instead of ";
							int num2 = 3;
							Type type = value.GetType();
							array[num2] = ((type != null) ? type.ToString() : null);
							array[4] = ")";
							DebugConsole.ThrowError(string.Concat(array), null, null, false, false);
							return false;
						}
						this.PropertyInfo.SetValue(parentObject, value, null);
					}
					catch (Exception e2)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(52, 3);
						defaultInterpolatedStringHandler6.AppendLiteral("Failed to set the value of the property \"");
						defaultInterpolatedStringHandler6.AppendFormatted(this.Name);
						defaultInterpolatedStringHandler6.AppendLiteral("\" of \"");
						defaultInterpolatedStringHandler6.AppendFormatted<object>(parentObject);
						defaultInterpolatedStringHandler6.AppendLiteral("\" to ");
						defaultInterpolatedStringHandler6.AppendFormatted<object>(value);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler6.ToStringAndClear(), e2, null, false, false);
						return false;
					}
					result = true;
				}
			}
			catch (Exception e3)
			{
				DebugConsole.ThrowError("Error in SerializableProperty.TrySetValue (Property: " + this.PropertyInfo.Name + ")", e3, null, false, false);
				result = false;
			}
			return result;
		}

		// Token: 0x06002D3D RID: 11581 RVA: 0x0012AD5C File Offset: 0x00128F5C
		public bool TrySetValue(object parentObject, float value)
		{
			try
			{
				if (this.TrySetFloatValueWithoutReflection(parentObject, value))
				{
					return true;
				}
				this.PropertyInfo.SetValue(parentObject, value, null);
			}
			catch (TargetInvocationException e)
			{
				DebugConsole.ThrowError("Exception thrown by the target of SerializableProperty.TrySetValue", e.InnerException, null, false, false);
				return false;
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error in SerializableProperty.TrySetValue (Property: " + this.PropertyInfo.Name + ")", e2, null, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x06002D3E RID: 11582 RVA: 0x0012ADEC File Offset: 0x00128FEC
		public bool TrySetValue(object parentObject, bool value)
		{
			try
			{
				if (this.TrySetBoolValueWithoutReflection(parentObject, value))
				{
					return true;
				}
				this.PropertyInfo.SetValue(parentObject, value, null);
			}
			catch (TargetInvocationException e)
			{
				DebugConsole.ThrowError("Exception thrown by the target of SerializableProperty.TrySetValue", e.InnerException, null, false, false);
				return false;
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error in SerializableProperty.TrySetValue (Property: " + this.PropertyInfo.Name + ")", e2, null, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x06002D3F RID: 11583 RVA: 0x0012AE7C File Offset: 0x0012907C
		public bool TrySetValue(object parentObject, int value)
		{
			try
			{
				if (this.TrySetFloatValueWithoutReflection(parentObject, (float)value))
				{
					return true;
				}
				this.PropertyInfo.SetValue(parentObject, value, null);
			}
			catch (TargetInvocationException e)
			{
				DebugConsole.ThrowError("Exception thrown by the target of SerializableProperty.TrySetValue", e.InnerException, null, false, false);
				return false;
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error in SerializableProperty.TrySetValue (Property: " + this.PropertyInfo.Name + ")", e2, null, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x06002D40 RID: 11584 RVA: 0x0012AF0C File Offset: 0x0012910C
		public object GetValue(object parentObject)
		{
			if (parentObject == null || this.PropertyInfo == null)
			{
				return false;
			}
			object value = this.TryGetValueWithoutReflection(parentObject);
			if (value != null)
			{
				return value;
			}
			object result;
			try
			{
				result = this.PropertyInfo.GetValue(parentObject, null);
			}
			catch (TargetInvocationException e)
			{
				DebugConsole.ThrowError("Exception thrown by the target of SerializableProperty.GetValue", e.InnerException, null, false, false);
				result = false;
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error in SerializableProperty.GetValue", e2, null, false, false);
				result = false;
			}
			return result;
		}

		// Token: 0x06002D41 RID: 11585 RVA: 0x0012AFA4 File Offset: 0x001291A4
		public float GetFloatValue(object parentObject)
		{
			if (parentObject == null || this.PropertyInfo == null)
			{
				return 0f;
			}
			float value;
			if (this.TryGetFloatValueWithoutReflection(parentObject, out value))
			{
				return value;
			}
			float result;
			try
			{
				if (this.PropertyType == typeof(int))
				{
					result = (float)((int)this.PropertyInfo.GetValue(parentObject, null));
				}
				else
				{
					result = (float)this.PropertyInfo.GetValue(parentObject, null);
				}
			}
			catch (TargetInvocationException e)
			{
				DebugConsole.ThrowError("Exception thrown by the target of SerializableProperty.GetValue", e.InnerException, null, false, false);
				result = 0f;
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error in SerializableProperty.GetValue", e2, null, false, false);
				result = 0f;
			}
			return result;
		}

		// Token: 0x06002D42 RID: 11586 RVA: 0x0012B068 File Offset: 0x00129268
		public bool GetBoolValue(object parentObject)
		{
			if (parentObject == null || this.PropertyInfo == null)
			{
				return false;
			}
			bool value;
			if (this.TryGetBoolValueWithoutReflection(parentObject, out value))
			{
				return value;
			}
			bool result;
			try
			{
				result = (bool)this.PropertyInfo.GetValue(parentObject, null);
			}
			catch (TargetInvocationException e)
			{
				DebugConsole.ThrowError("Exception thrown by the target of SerializableProperty.GetValue", e.InnerException, null, false, false);
				result = false;
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error in SerializableProperty.GetValue", e2, null, false, false);
				result = false;
			}
			return result;
		}

		// Token: 0x06002D43 RID: 11587 RVA: 0x0012B0F4 File Offset: 0x001292F4
		public static string GetSupportedTypeName(Type type)
		{
			if (type.IsEnum)
			{
				return "Enum";
			}
			string typeName;
			if (!SerializableProperty.supportedTypes.TryGetValue(type, out typeName))
			{
				return null;
			}
			return typeName;
		}

		// Token: 0x06002D44 RID: 11588 RVA: 0x0012B124 File Offset: 0x00129324
		private object TryGetValueWithoutReflection(object parentObject)
		{
			string value3;
			if (this.PropertyType == typeof(float))
			{
				float value;
				if (this.TryGetFloatValueWithoutReflection(parentObject, out value))
				{
					return value;
				}
			}
			else if (this.PropertyType == typeof(bool))
			{
				bool value2;
				if (this.TryGetBoolValueWithoutReflection(parentObject, out value2))
				{
					return value2;
				}
			}
			else if (this.PropertyType == typeof(string) && this.TryGetStringValueWithoutReflection(parentObject, out value3))
			{
				return value3;
			}
			return null;
		}

		// Token: 0x06002D45 RID: 11589 RVA: 0x0012B1A8 File Offset: 0x001293A8
		private bool TryGetFloatValueWithoutReflection(object parentObject, out float value)
		{
			value = 0f;
			string name = this.Name;
			if (name != null)
			{
				switch (name.Length)
				{
				case 5:
					if (name == "Stuck")
					{
						Door door = parentObject as Door;
						if (door != null)
						{
							value = door.Stuck;
							return true;
						}
					}
					break;
				case 6:
				{
					char c = name[0];
					if (c != 'C')
					{
						if (c != 'H')
						{
							if (c == 'O')
							{
								if (name == "Oxygen")
								{
									Character character = parentObject as Character;
									if (character != null)
									{
										value = character.Oxygen;
										return true;
									}
									Hull hull = parentObject as Hull;
									if (hull != null)
									{
										value = hull.Oxygen;
										return true;
									}
								}
							}
						}
						else if (name == "Health")
						{
							Character character2 = parentObject as Character;
							if (character2 != null)
							{
								value = character2.Health;
								return true;
							}
						}
					}
					else if (name == "Charge")
					{
						PowerContainer powerContainer = parentObject as PowerContainer;
						if (powerContainer != null)
						{
							value = powerContainer.Charge;
							return true;
						}
					}
					break;
				}
				case 7:
					if (name == "Voltage")
					{
						Powered powered = parentObject as Powered;
						if (powered != null)
						{
							value = powered.Voltage;
							return true;
						}
					}
					break;
				case 8:
					if (name == "CurrFlow")
					{
						Pump pump = parentObject as Pump;
						if (pump != null)
						{
							value = pump.CurrFlow;
							return true;
						}
						OxygenGenerator oxygenGenerator = parentObject as OxygenGenerator;
						if (oxygenGenerator != null)
						{
							value = oxygenGenerator.CurrFlow;
							return true;
						}
					}
					break;
				case 9:
					if (name == "Condition")
					{
						Item item = parentObject as Item;
						if (item != null)
						{
							value = item.Condition;
							return true;
						}
					}
					break;
				case 10:
				{
					char c = name[1];
					if (c != 'i')
					{
						if (c != 'o')
						{
							if (c == 'x')
							{
								if (name == "OxygenFlow")
								{
									Vent vent = parentObject as Vent;
									if (vent != null)
									{
										value = vent.OxygenFlow;
										return true;
									}
								}
							}
						}
						else if (name == "SoundRange")
						{
							Item item2 = parentObject as Item;
							if (item2 != null)
							{
								value = item2.SoundRange;
								return true;
							}
						}
					}
					else if (name == "SightRange")
					{
						Item item3 = parentObject as Item;
						if (item3 != null)
						{
							value = item3.SightRange;
							return true;
						}
					}
					break;
				}
				case 11:
				{
					char c = name[0];
					if (c != 'F')
					{
						if (c == 'T')
						{
							if (name == "Temperature")
							{
								Reactor reactor = parentObject as Reactor;
								if (reactor != null)
								{
									value = reactor.Temperature;
									return true;
								}
							}
						}
					}
					else if (name == "FissionRate")
					{
						Reactor reactor2 = parentObject as Reactor;
						if (reactor2 != null)
						{
							value = reactor2.FissionRate;
							return true;
						}
					}
					break;
				}
				case 13:
				{
					char c = name[0];
					if (c != 'A')
					{
						if (c != 'C')
						{
							if (c == 'R')
							{
								if (name == "RechargeRatio")
								{
									PowerContainer powerContainer2 = parentObject as PowerContainer;
									if (powerContainer2 != null)
									{
										value = powerContainer2.RechargeRatio;
										return true;
									}
								}
							}
						}
						else if (name == "CurrentVolume")
						{
							Engine engine = parentObject as Engine;
							if (engine != null)
							{
								value = engine.CurrentVolume;
								return true;
							}
						}
					}
					else if (name == "AvailableFuel")
					{
						Reactor reactor3 = parentObject as Reactor;
						if (reactor3 != null)
						{
							value = reactor3.AvailableFuel;
							return true;
						}
					}
					break;
				}
				case 15:
					switch (name[0])
					{
					case 'O':
						if (name == "OxygenAvailable")
						{
							Character character3 = parentObject as Character;
							if (character3 != null)
							{
								value = character3.OxygenAvailable;
								return true;
							}
						}
						break;
					case 'R':
						if (name == "RelativeVoltage")
						{
							Powered powered2 = parentObject as Powered;
							if (powered2 != null)
							{
								value = powered2.RelativeVoltage;
								return true;
							}
						}
						break;
					case 'S':
						if (name == "SpeedMultiplier")
						{
							Character character4 = parentObject as Character;
							if (character4 != null)
							{
								value = character4.SpeedMultiplier;
								return true;
							}
						}
						break;
					}
					break;
				case 16:
					if (name == "ChargePercentage")
					{
						PowerContainer powerContainer3 = parentObject as PowerContainer;
						if (powerContainer3 != null)
						{
							value = powerContainer3.ChargePercentage;
							return true;
						}
					}
					break;
				case 17:
					if (name == "LowPassMultiplier")
					{
						Character character5 = parentObject as Character;
						if (character5 != null)
						{
							value = character5.LowPassMultiplier;
							return true;
						}
					}
					break;
				case 18:
					if (name == "PressureProtection")
					{
						Character character6 = parentObject as Character;
						if (character6 != null)
						{
							value = character6.PressureProtection;
							return true;
						}
					}
					break;
				case 19:
				{
					char c = name[1];
					if (c != 'o')
					{
						if (c == 'u')
						{
							if (name == "CurrentBrokenVolume")
							{
								Engine engine2 = parentObject as Engine;
								if (engine2 != null)
								{
									value = engine2.CurrentBrokenVolume;
									return true;
								}
								Pump pump2 = parentObject as Pump;
								if (pump2 != null)
								{
									value = pump2.CurrentBrokenVolume;
									return true;
								}
							}
						}
					}
					else if (name == "ConditionPercentage")
					{
						Item item4 = parentObject as Item;
						if (item4 != null)
						{
							value = item4.ConditionPercentage;
							return true;
						}
					}
					break;
				}
				case 20:
				{
					char c = name[0];
					if (c != 'C')
					{
						if (c != 'H')
						{
							if (c == 'O')
							{
								if (name == "ObstructVisionAmount")
								{
									Character character7 = parentObject as Character;
									if (character7 != null)
									{
										value = character7.ObstructVisionAmount;
										return true;
									}
								}
							}
						}
						else if (name == "HullOxygenPercentage")
						{
							Character character8 = parentObject as Character;
							if (character8 != null)
							{
								value = character8.HullOxygenPercentage;
								return true;
							}
							Item item5 = parentObject as Item;
							if (item5 != null)
							{
								value = item5.HullOxygenPercentage;
								return true;
							}
						}
					}
					else if (name == "CurrPowerConsumption")
					{
						Powered powered3 = parentObject as Powered;
						if (powered3 != null)
						{
							value = powered3.CurrPowerConsumption;
							return true;
						}
					}
					break;
				}
				case 25:
					if (name == "PropulsionSpeedMultiplier")
					{
						Character character9 = parentObject as Character;
						if (character9 != null)
						{
							value = character9.PropulsionSpeedMultiplier;
							return true;
						}
					}
					break;
				case 27:
					if (name == "ContainedNonBrokenItemCount")
					{
						ItemContainer itemContainer = parentObject as ItemContainer;
						if (itemContainer != null)
						{
							value = (float)itemContainer.ContainedNonBrokenItemCount;
							return true;
						}
					}
					break;
				case 29:
					if (name == "StressDeteriorationMultiplier")
					{
						Repairable repairable = parentObject as Repairable;
						if (repairable != null)
						{
							value = repairable.StressDeteriorationMultiplier;
							return true;
						}
					}
					break;
				}
			}
			return false;
		}

		// Token: 0x06002D46 RID: 11590 RVA: 0x0012B8F0 File Offset: 0x00129AF0
		private bool TryGetBoolValueWithoutReflection(object parentObject, out bool value)
		{
			value = false;
			string name = this.Name;
			if (name != null)
			{
				switch (name.Length)
				{
				case 4:
					if (name == "IsOn")
					{
						LightComponent lightComponent = parentObject as LightComponent;
						if (lightComponent != null)
						{
							value = lightComponent.IsOn;
							return true;
						}
					}
					break;
				case 5:
					if (name == "State")
					{
						Controller controller = parentObject as Controller;
						if (controller != null)
						{
							value = controller.State;
							return true;
						}
					}
					break;
				case 6:
				{
					char c = name[0];
					if (c != 'D')
					{
						if (c == 'I')
						{
							if (name == "IsDead")
							{
								Character character = parentObject as Character;
								if (character != null)
								{
									value = character.IsDead;
									return true;
								}
							}
						}
					}
					else if (name == "Docked")
					{
						DockingPort dockingPort = parentObject as DockingPort;
						if (dockingPort != null)
						{
							value = dockingPort.Docked;
							return true;
						}
					}
					break;
				}
				case 7:
				{
					char c = name[2];
					if (c != 'H')
					{
						if (c != 'W')
						{
							if (c == 'a')
							{
								if (name == "Snapped")
								{
									Rope rope = parentObject as Rope;
									if (rope != null)
									{
										value = rope.Snapped;
										return true;
									}
								}
							}
						}
						else if (name == "InWater")
						{
							Character character2 = parentObject as Character;
							if (character2 != null)
							{
								value = character2.InWater;
								return true;
							}
							Item item = parentObject as Item;
							if (item != null)
							{
								value = item.InWater;
								return true;
							}
						}
					}
					else if (name == "IsHuman")
					{
						Character character3 = parentObject as Character;
						if (character3 != null)
						{
							value = character3.IsHuman;
							return true;
						}
					}
					break;
				}
				case 8:
				{
					char c = name[0];
					if (c <= 'I')
					{
						if (c != 'A')
						{
							if (c == 'I')
							{
								if (name == "IsActive")
								{
									ItemComponent ic = parentObject as ItemComponent;
									if (ic != null)
									{
										value = ic.IsActive;
										return true;
									}
								}
							}
						}
						else if (name == "Attached")
						{
							Holdable holdable = parentObject as Holdable;
							if (holdable != null)
							{
								value = holdable.Attached;
								return true;
							}
						}
					}
					else if (c != 'N')
					{
						if (c == 'O')
						{
							if (name == "Overload")
							{
								PowerTransfer powerTransfer = parentObject as PowerTransfer;
								if (powerTransfer != null)
								{
									value = powerTransfer.Overload;
									return true;
								}
							}
						}
					}
					else if (name == "NeedsAir")
					{
						Character character4 = parentObject as Character;
						if (character4 != null)
						{
							value = character4.NeedsAir;
							return true;
						}
					}
					break;
				}
				case 11:
					if (name == "NeedsOxygen")
					{
						Character character5 = parentObject as Character;
						if (character5 != null)
						{
							value = character5.NeedsOxygen;
							return true;
						}
					}
					break;
				case 13:
					if (name == "TriggerActive")
					{
						TriggerComponent trigger = parentObject as TriggerComponent;
						if (trigger != null)
						{
							value = trigger.TriggerActive;
							return true;
						}
					}
					break;
				case 14:
				{
					char c = name[0];
					if (c != 'M')
					{
						if (c == 'O')
						{
							if (name == "OutputDisabled")
							{
								PowerContainer powerContainer = parentObject as PowerContainer;
								if (powerContainer != null)
								{
									value = powerContainer.OutputDisabled;
									return true;
								}
							}
						}
					}
					else if (name == "MotionDetected")
					{
						MotionSensor motionSensor = parentObject as MotionSensor;
						if (motionSensor != null)
						{
							value = motionSensor.MotionDetected;
							return true;
						}
					}
					break;
				}
				case 17:
					if (name == "PhysicsBodyActive")
					{
						Item item2 = parentObject as Item;
						if (item2 != null)
						{
							value = item2.PhysicsBodyActive;
							return true;
						}
					}
					break;
				case 19:
					if (name == "TemperatureCritical")
					{
						Reactor reactor = parentObject as Reactor;
						if (reactor != null)
						{
							value = reactor.TemperatureCritical;
							return true;
						}
					}
					break;
				}
			}
			return false;
		}

		// Token: 0x06002D47 RID: 11591 RVA: 0x0012BD40 File Offset: 0x00129F40
		private bool TryGetStringValueWithoutReflection(object parentObject, out string value)
		{
			value = null;
			string name = this.Name;
			if (name == "ContainerIdentifier")
			{
				Item item = parentObject as Item;
				if (item != null)
				{
					value = item.ContainerIdentifier.Value;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002D48 RID: 11592 RVA: 0x0012BD84 File Offset: 0x00129F84
		private bool TrySetFloatValueWithoutReflection(object parentObject, float value)
		{
			string name = this.Name;
			if (name != null)
			{
				switch (name.Length)
				{
				case 5:
					if (name == "Scale")
					{
						Item item = parentObject as Item;
						if (item != null)
						{
							item.Scale = value;
							return true;
						}
					}
					break;
				case 6:
				{
					char c = name[0];
					if (c != 'C')
					{
						if (c == 'O')
						{
							if (name == "Oxygen")
							{
								Character character = parentObject as Character;
								if (character != null)
								{
									character.Oxygen = value;
									return true;
								}
							}
						}
					}
					else if (name == "Charge")
					{
						PowerContainer powerContainer = parentObject as PowerContainer;
						if (powerContainer != null)
						{
							powerContainer.Charge = value;
							return true;
						}
					}
					break;
				}
				case 7:
					if (name == "Voltage")
					{
						Powered powered = parentObject as Powered;
						if (powered != null)
						{
							powered.Voltage = value;
							return true;
						}
					}
					break;
				case 9:
					if (name == "Condition")
					{
						Item item2 = parentObject as Item;
						if (item2 != null)
						{
							item2.Condition = value;
							return true;
						}
					}
					break;
				case 10:
				{
					char c = name[1];
					if (c != 'i')
					{
						if (c == 'o')
						{
							if (name == "SoundRange")
							{
								Item item3 = parentObject as Item;
								if (item3 != null)
								{
									item3.SoundRange = value;
									return true;
								}
							}
						}
					}
					else if (name == "SightRange")
					{
						Item item4 = parentObject as Item;
						if (item4 != null)
						{
							item4.SightRange = value;
							return true;
						}
					}
					break;
				}
				case 13:
					if (name == "AvailableFuel")
					{
						Reactor reactor = parentObject as Reactor;
						if (reactor != null)
						{
							reactor.AvailableFuel = value;
							return true;
						}
					}
					break;
				case 15:
				{
					char c = name[0];
					if (c != 'O')
					{
						if (c == 'S')
						{
							if (name == "SpeedMultiplier")
							{
								Character character2 = parentObject as Character;
								if (character2 != null)
								{
									character2.StackSpeedMultiplier(value);
									return true;
								}
							}
						}
					}
					else if (name == "OxygenAvailable")
					{
						Character character3 = parentObject as Character;
						if (character3 != null)
						{
							character3.OxygenAvailable = value;
							return true;
						}
					}
					break;
				}
				case 16:
					if (name == "HealthMultiplier")
					{
						Character character4 = parentObject as Character;
						if (character4 != null)
						{
							character4.StackHealthMultiplier(value);
							return true;
						}
					}
					break;
				case 17:
					if (name == "LowPassMultiplier")
					{
						Character character5 = parentObject as Character;
						if (character5 != null)
						{
							character5.LowPassMultiplier = value;
							return true;
						}
					}
					break;
				case 18:
					if (name == "PressureProtection")
					{
						Character character6 = parentObject as Character;
						if (character6 != null)
						{
							character6.PressureProtection = value;
							return true;
						}
					}
					break;
				case 20:
					if (name == "ObstructVisionAmount")
					{
						Character character7 = parentObject as Character;
						if (character7 != null)
						{
							character7.ObstructVisionAmount = value;
							return true;
						}
					}
					break;
				case 25:
					if (name == "PropulsionSpeedMultiplier")
					{
						Character character8 = parentObject as Character;
						if (character8 != null)
						{
							character8.PropulsionSpeedMultiplier = value;
							return true;
						}
					}
					break;
				}
			}
			return false;
		}

		// Token: 0x06002D49 RID: 11593 RVA: 0x0012C100 File Offset: 0x0012A300
		private bool TrySetBoolValueWithoutReflection(object parentObject, bool value)
		{
			string name = this.Name;
			if (!(name == "ObstructVision"))
			{
				if (!(name == "HideFace"))
				{
					if (!(name == "UseHullOxygen"))
					{
						if (!(name == "IsOn"))
						{
							if (name == "IsActive")
							{
								ItemComponent ic = parentObject as ItemComponent;
								if (ic != null)
								{
									ic.IsActive = value;
									return true;
								}
							}
						}
						else
						{
							LightComponent lightComponent = parentObject as LightComponent;
							if (lightComponent != null)
							{
								lightComponent.IsOn = value;
								return true;
							}
						}
					}
					else
					{
						Character character = parentObject as Character;
						if (character != null)
						{
							character.UseHullOxygen = value;
							return true;
						}
					}
				}
				else
				{
					Character character2 = parentObject as Character;
					if (character2 != null)
					{
						character2.HideFace = value;
						return true;
					}
				}
			}
			else
			{
				Character character3 = parentObject as Character;
				if (character3 != null)
				{
					character3.ObstructVision = value;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002D4A RID: 11594 RVA: 0x0012C1C0 File Offset: 0x0012A3C0
		public static List<SerializableProperty> GetProperties<T>(ISerializableEntity obj)
		{
			List<SerializableProperty> editableProperties = new List<SerializableProperty>();
			foreach (SerializableProperty property in obj.SerializableProperties.Values)
			{
				if (property.Attributes.OfType<T>().Any<T>())
				{
					editableProperties.Add(property);
				}
			}
			return editableProperties;
		}

		// Token: 0x06002D4B RID: 11595 RVA: 0x0012C234 File Offset: 0x0012A434
		public static Dictionary<Identifier, SerializableProperty> GetProperties(object obj)
		{
			Type objType = obj.GetType();
			if (SerializableProperty.cachedProperties.ContainsKey(objType))
			{
				return SerializableProperty.cachedProperties[objType];
			}
			IEnumerable<PropertyDescriptor> properties = TypeDescriptor.GetProperties(obj.GetType()).Cast<PropertyDescriptor>();
			Dictionary<Identifier, SerializableProperty> dictionary = new Dictionary<Identifier, SerializableProperty>();
			foreach (PropertyDescriptor property in properties)
			{
				SerializableProperty serializableProperty = null;
				try
				{
					serializableProperty = new SerializableProperty(property);
				}
				catch (AmbiguousMatchException)
				{
					continue;
				}
				dictionary.Add(serializableProperty.Name.ToIdentifier(), serializableProperty);
			}
			SerializableProperty.cachedProperties[objType] = dictionary;
			return dictionary;
		}

		// Token: 0x06002D4C RID: 11596 RVA: 0x0012C2EC File Offset: 0x0012A4EC
		public static Dictionary<Identifier, SerializableProperty> DeserializeProperties(object obj, XElement element = null)
		{
			Dictionary<Identifier, SerializableProperty> dictionary = SerializableProperty.GetProperties(obj);
			foreach (SerializableProperty property in dictionary.Values)
			{
				using (IEnumerator<Serialize> enumerator2 = property.Attributes.OfType<Serialize>().GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Serialize ini = enumerator2.Current;
						property.TrySetValue(obj, ini.DefaultValue);
					}
				}
			}
			if (element != null)
			{
				foreach (XAttribute attribute in element.Attributes())
				{
					SerializableProperty property2;
					if (dictionary.TryGetValue(attribute.NameAsIdentifier(), out property2) && property2.Attributes.OfType<Serialize>().Any<Serialize>())
					{
						property2.TrySetValue(obj, attribute.Value);
					}
				}
			}
			return dictionary;
		}

		// Token: 0x06002D4D RID: 11597 RVA: 0x0012C400 File Offset: 0x0012A600
		public static void SerializeProperties(ISerializableEntity obj, XElement element, bool saveIfDefault = false, bool ignoreEditable = false)
		{
			List<SerializableProperty> saveProperties = SerializableProperty.GetProperties<Serialize>(obj);
			foreach (SerializableProperty property in saveProperties)
			{
				object value = property.GetValue(obj);
				if (value != null)
				{
					if (!saveIfDefault)
					{
						bool save = false;
						foreach (Serialize attribute in property.Attributes.OfType<Serialize>())
						{
							if ((attribute.IsSaveable == IsPropertySaveable.Yes && !attribute.DefaultValue.Equals(value)) || (!ignoreEditable && property.Attributes.OfType<Editable>().Any<Editable>()))
							{
								save = true;
								break;
							}
						}
						if (!save)
						{
							continue;
						}
					}
					string typeName;
					string stringValue;
					if (!SerializableProperty.supportedTypes.TryGetValue(value.GetType(), out typeName))
					{
						if (!property.PropertyType.IsEnum)
						{
							string[] array = new string[7];
							array[0] = "Failed to serialize the property \"";
							array[1] = property.Name;
							array[2] = "\" of \"";
							array[3] = ((obj != null) ? obj.ToString() : null);
							array[4] = "\" (type ";
							int num = 5;
							Type propertyType = property.PropertyType;
							array[num] = ((propertyType != null) ? propertyType.ToString() : null);
							array[6] = " not supported)";
							DebugConsole.ThrowError(string.Concat(array), null, null, false, false);
							continue;
						}
						stringValue = value.ToString();
					}
					else
					{
						if (typeName != null)
						{
							int length = typeName.Length;
							switch (length)
							{
							case 5:
							{
								char c = typeName[0];
								if (c != 'c')
								{
									if (c != 'f')
									{
										if (c == 'p')
										{
											if (typeName == "point")
											{
												stringValue = XMLExtensions.PointToString((Point)value);
												goto IL_355;
											}
										}
									}
									else if (typeName == "float")
									{
										stringValue = ((float)value).ToString("G", CultureInfo.InvariantCulture);
										goto IL_355;
									}
								}
								else if (typeName == "color")
								{
									stringValue = XMLExtensions.ColorToString((Color)value);
									goto IL_355;
								}
								break;
							}
							case 6:
							case 8:
							case 10:
								break;
							case 7:
								switch (typeName[6])
								{
								case '2':
									if (typeName == "vector2")
									{
										stringValue = XMLExtensions.Vector2ToString((Vector2)value);
										goto IL_355;
									}
									break;
								case '3':
									if (typeName == "vector3")
									{
										stringValue = XMLExtensions.Vector3ToString((Vector3)value, "G");
										goto IL_355;
									}
									break;
								case '4':
									if (typeName == "vector4")
									{
										stringValue = XMLExtensions.Vector4ToString((Vector4)value, "G");
										goto IL_355;
									}
									break;
								}
								break;
							case 9:
								if (typeName == "rectangle")
								{
									stringValue = XMLExtensions.RectToString((Rectangle)value);
									goto IL_355;
								}
								break;
							case 11:
								if (typeName == "stringarray")
								{
									string[] stringArray = (string[])value;
									stringValue = ((stringArray != null) ? string.Join(';', stringArray) : "");
									goto IL_355;
								}
								break;
							default:
								if (length == 15)
								{
									if (typeName == "identifierarray")
									{
										Identifier[] identifierArray = (Identifier[])value;
										stringValue = ((identifierArray != null) ? string.Join<Identifier>(';', identifierArray) : "");
										goto IL_355;
									}
								}
								break;
							}
						}
						stringValue = value.ToString();
					}
					IL_355:
					XAttribute attribute2 = element.GetAttribute(property.Name, StringComparison.OrdinalIgnoreCase);
					if (attribute2 != null)
					{
						attribute2.Remove();
					}
					element.SetAttributeValue(property.Name, stringValue);
				}
			}
		}

		// Token: 0x06002D4E RID: 11598 RVA: 0x0012C7E0 File Offset: 0x0012A9E0
		public static void UpgradeGameVersion(ISerializableEntity entity, ContentXElement configElement, Version savedVersion)
		{
			foreach (ContentXElement subElement in configElement.Elements())
			{
				if (subElement.Name.ToString().Equals("upgrade", StringComparison.OrdinalIgnoreCase))
				{
					Version upgradeVersion = new Version(subElement.GetAttributeString("gameversion", "0.0.0.0"));
					if (subElement.GetAttributeBool("campaignsaveonly", false))
					{
						GameSession gameSession = GameMain.GameSession;
						if ((((gameSession != null) ? gameSession.LastSaveVersion : null) ?? GameMain.Version) >= upgradeVersion)
						{
							continue;
						}
					}
					else if (savedVersion >= upgradeVersion)
					{
						continue;
					}
					foreach (XAttribute attribute in subElement.Attributes())
					{
						Identifier attributeName = attribute.NameAsIdentifier();
						if (!(attributeName == "gameversion") && !(attributeName == "campaignsaveonly"))
						{
							if (attributeName == "refreshrect")
							{
								Structure structure = entity as Structure;
								if (structure != null)
								{
									if (!structure.ResizeHorizontal)
									{
										structure.Rect = (structure.DefaultRect = new Rectangle(structure.Rect.X, structure.Rect.Y, (int)structure.Prefab.ScaledSize.X, structure.Rect.Height));
									}
									if (!structure.ResizeVertical)
									{
										structure.Rect = (structure.DefaultRect = new Rectangle(structure.Rect.X, structure.Rect.Y, structure.Rect.Width, (int)structure.Prefab.ScaledSize.Y));
									}
								}
								else
								{
									Item item = entity as Item;
									if (item != null)
									{
										if (!item.ResizeHorizontal)
										{
											item.Rect = (item.DefaultRect = new Rectangle(item.Rect.X, item.Rect.Y, (int)(item.Prefab.Size.X * item.Prefab.Scale), item.Rect.Height));
										}
										if (!item.ResizeVertical)
										{
											item.Rect = (item.DefaultRect = new Rectangle(item.Rect.X, item.Rect.Y, item.Rect.Width, (int)(item.Prefab.Size.Y * item.Prefab.Scale)));
										}
									}
								}
							}
							else if (attributeName == "unlockrecipe" || attributeName == "unlockrecipes")
							{
								ImmutableHashSet<Identifier> recipes = subElement.GetAttributeIdentifierImmutableHashSet("unlockrecipes", subElement.GetAttributeIdentifierImmutableHashSet("unlockrecipe", ImmutableHashSet<Identifier>.Empty, true), true);
								foreach (Identifier recipe in recipes)
								{
									GameSession gameSession2 = GameMain.GameSession;
									if (gameSession2 != null)
									{
										gameSession2.UnlockRecipe(CharacterTeamType.Team1, recipe, false);
									}
								}
							}
							SerializableProperty property;
							if (entity.SerializableProperties.TryGetValue(attributeName, out property))
							{
								SerializableProperty.<UpgradeGameVersion>g__FixValue|32_0(property, entity, attribute);
								if (property.Name == "Msg")
								{
									ItemComponent component = entity as ItemComponent;
									if (component != null)
									{
										component.ParseMsg();
									}
								}
							}
							else
							{
								Item item2 = entity as Item;
								if (item2 != null)
								{
									foreach (ISerializableEntity component2 in item2.AllPropertyObjects)
									{
										SerializableProperty componentProperty;
										if (component2.SerializableProperties.TryGetValue(attributeName, out componentProperty))
										{
											SerializableProperty.<UpgradeGameVersion>g__FixValue|32_0(componentProperty, component2, attribute);
											if (componentProperty.Name == "Msg")
											{
												((ItemComponent)component2).ParseMsg();
											}
										}
									}
								}
							}
						}
					}
					Item item3 = entity as Item;
					if (item3 != null)
					{
						SerializableProperty.<>c__DisplayClass32_0 CS$<>8__locals1 = new SerializableProperty.<>c__DisplayClass32_0();
						CS$<>8__locals1.componentElement = subElement.FirstElement();
						SerializableProperty.<>c__DisplayClass32_0 CS$<>8__locals2 = CS$<>8__locals1;
						ContentXElement contentXElement = null;
						if (!(CS$<>8__locals2.componentElement == contentXElement))
						{
							ItemComponent itemComponent = item3.Components.FirstOrDefault((ItemComponent c) => c.Name == CS$<>8__locals1.componentElement.Name.ToString());
							if (itemComponent != null)
							{
								foreach (XAttribute attribute2 in CS$<>8__locals1.componentElement.Attributes())
								{
									Identifier attributeName2 = attribute2.NameAsIdentifier();
									SerializableProperty property2;
									if (itemComponent.SerializableProperties.TryGetValue(attributeName2, out property2))
									{
										SerializableProperty.<UpgradeGameVersion>g__FixValue|32_0(property2, itemComponent, attribute2);
									}
								}
								foreach (ContentXElement element in CS$<>8__locals1.componentElement.Elements())
								{
									string a = element.Name.ToString().ToLowerInvariant();
									if (a == "requireditem" || a == "requireditems")
									{
										itemComponent.RequiredItems.Clear();
										itemComponent.DisabledRequiredItems.Clear();
										itemComponent.SetRequiredItems(element, true);
									}
								}
								ItemContainer itemContainer = itemComponent as ItemContainer;
								if (itemContainer != null)
								{
									contentXElement = CS$<>8__locals1.componentElement.GetChildElement("containable");
									ContentXElement contentXElement2 = null;
									if (!(contentXElement != contentXElement2))
									{
										ContentXElement childElement = CS$<>8__locals1.componentElement.GetChildElement("subcontainer");
										ContentXElement contentXElement3 = null;
										if (!(childElement != contentXElement3))
										{
											continue;
										}
									}
									itemContainer.ReloadContainableRestrictions(CS$<>8__locals1.componentElement);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06002D50 RID: 11600 RVA: 0x0012CF6C File Offset: 0x0012B16C
		[CompilerGenerated]
		internal static void <UpgradeGameVersion>g__FixValue|32_0(SerializableProperty property, object parentObject, XAttribute attribute)
		{
			if (attribute.Value.Length > 0 && attribute.Value[0] == '*')
			{
				float multiplier;
				float.TryParse(attribute.Value.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out multiplier);
				if (property.PropertyType == typeof(int))
				{
					property.TrySetValue(parentObject, (int)((float)((int)property.GetValue(parentObject)) * multiplier));
					return;
				}
				if (property.PropertyType == typeof(float))
				{
					property.TrySetValue(parentObject, (float)property.GetValue(parentObject) * multiplier);
					return;
				}
				if (property.PropertyType == typeof(Vector2))
				{
					property.TrySetValue(parentObject, (Vector2)property.GetValue(parentObject) * multiplier);
					return;
				}
				if (property.PropertyType == typeof(Point))
				{
					property.TrySetValue(parentObject, ((Point)property.GetValue(parentObject)).Multiply(multiplier));
					return;
				}
			}
			else if (attribute.Value.Length > 0 && attribute.Value[0] == '+')
			{
				if (property.PropertyType == typeof(int))
				{
					float addition;
					float.TryParse(attribute.Value.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out addition);
					property.TrySetValue(parentObject, (int)((float)((int)property.GetValue(parentObject)) + addition));
					return;
				}
				if (property.PropertyType == typeof(float))
				{
					float addition2;
					float.TryParse(attribute.Value.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out addition2);
					property.TrySetValue(parentObject, (float)property.GetValue(parentObject) + addition2);
					return;
				}
				if (property.PropertyType == typeof(Vector2))
				{
					Vector2 addition3 = XMLExtensions.ParseVector2(attribute.Value.Substring(1), true);
					property.TrySetValue(parentObject, (Vector2)property.GetValue(parentObject) + addition3);
					return;
				}
				if (property.PropertyType == typeof(Point))
				{
					Point addition4 = XMLExtensions.ParsePoint(attribute.Value.Substring(1), true);
					property.TrySetValue(parentObject, (Point)property.GetValue(parentObject) + addition4);
					return;
				}
			}
			else
			{
				property.TrySetValue(parentObject, attribute.Value);
			}
		}

		// Token: 0x04001643 RID: 5699
		private static readonly ImmutableDictionary<Type, string> supportedTypes = new Dictionary<Type, string>
		{
			{
				typeof(bool),
				"bool"
			},
			{
				typeof(int),
				"int"
			},
			{
				typeof(float),
				"float"
			},
			{
				typeof(string),
				"string"
			},
			{
				typeof(Identifier),
				"identifier"
			},
			{
				typeof(LanguageIdentifier),
				"languageidentifier"
			},
			{
				typeof(LocalizedString),
				"localizedstring"
			},
			{
				typeof(Point),
				"point"
			},
			{
				typeof(Vector2),
				"vector2"
			},
			{
				typeof(Vector3),
				"vector3"
			},
			{
				typeof(Vector4),
				"vector4"
			},
			{
				typeof(Rectangle),
				"rectangle"
			},
			{
				typeof(Color),
				"color"
			},
			{
				typeof(string[]),
				"stringarray"
			},
			{
				typeof(Identifier[]),
				"identifierarray"
			}
		}.ToImmutableDictionary<Type, string>();

		// Token: 0x04001644 RID: 5700
		private static readonly Dictionary<Type, Dictionary<Identifier, SerializableProperty>> cachedProperties = new Dictionary<Type, Dictionary<Identifier, SerializableProperty>>();

		// Token: 0x04001645 RID: 5701
		public readonly string Name;

		// Token: 0x04001646 RID: 5702
		public readonly AttributeCollection Attributes;

		// Token: 0x04001647 RID: 5703
		public readonly Type PropertyType;

		// Token: 0x04001648 RID: 5704
		public readonly bool OverridePrefabValues;

		// Token: 0x04001649 RID: 5705
		public readonly PropertyInfo PropertyInfo;

		// Token: 0x0400164A RID: 5706
		private readonly ImmutableDictionary<Identifier, Func<object, object>> valueGetters;
	}
}
