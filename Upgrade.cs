using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200037F RID: 895
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class Upgrade : IDisposable
	{
		// Token: 0x170011C4 RID: 4548
		// (get) Token: 0x06004407 RID: 17415 RVA: 0x00255A38 File Offset: 0x00253C38
		private ISerializableEntity TargetEntity { get; }

		// Token: 0x170011C5 RID: 4549
		// (get) Token: 0x06004408 RID: 17416 RVA: 0x00255A40 File Offset: 0x00253C40
		public Dictionary<ISerializableEntity, PropertyReference[]> TargetComponents { get; }

		// Token: 0x170011C6 RID: 4550
		// (get) Token: 0x06004409 RID: 17417 RVA: 0x00255A48 File Offset: 0x00253C48
		public UpgradePrefab Prefab { get; }

		// Token: 0x170011C7 RID: 4551
		// (get) Token: 0x0600440A RID: 17418 RVA: 0x00255A50 File Offset: 0x00253C50
		public Identifier Identifier
		{
			get
			{
				return this.Prefab.Identifier;
			}
		}

		// Token: 0x170011C8 RID: 4552
		// (get) Token: 0x0600440B RID: 17419 RVA: 0x00255A5D File Offset: 0x00253C5D
		// (set) Token: 0x0600440C RID: 17420 RVA: 0x00255A65 File Offset: 0x00253C65
		public int Level { get; set; }

		// Token: 0x170011C9 RID: 4553
		// (get) Token: 0x0600440D RID: 17421 RVA: 0x00255A6E File Offset: 0x00253C6E
		// (set) Token: 0x0600440E RID: 17422 RVA: 0x00255A76 File Offset: 0x00253C76
		public bool Disposed { get; private set; }

		// Token: 0x0600440F RID: 17423 RVA: 0x00255A80 File Offset: 0x00253C80
		public Upgrade(ISerializableEntity targetEntity, UpgradePrefab prefab, int level, [Nullable(2)] XContainer saveElement = null)
		{
			this.TargetEntity = targetEntity;
			this.sourceElement = prefab.SourceElement;
			this.Prefab = prefab;
			this.Level = level;
			Dictionary<ISerializableEntity, PropertyReference[]> targetProperties = new Dictionary<ISerializableEntity, PropertyReference[]>();
			List<XElement> saveElements = (saveElement != null) ? saveElement.Elements().ToList<XElement>() : null;
			foreach (ContentXElement subElement in prefab.SourceElement.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					switch (length)
					{
					case 4:
					{
						char c = text[0];
						if (c <= 'i')
						{
							if (c != 'b')
							{
								if (c != 'i')
								{
									goto IL_1EA;
								}
								if (!(text == "item"))
								{
									goto IL_1EA;
								}
							}
							else if (!(text == "base"))
							{
								goto IL_1EA;
							}
						}
						else if (c != 'r')
						{
							if (c != 't')
							{
								goto IL_1EA;
							}
							if (!(text == "this"))
							{
								goto IL_1EA;
							}
						}
						else if (!(text == "root"))
						{
							goto IL_1EA;
						}
						break;
					}
					case 5:
						if (!(text == "price"))
						{
							goto IL_1EA;
						}
						continue;
					case 6:
						if (!(text == "sprite"))
						{
							goto IL_1EA;
						}
						continue;
					case 7:
					case 8:
						goto IL_1EA;
					case 9:
						if (!(text == "structure"))
						{
							goto IL_1EA;
						}
						break;
					default:
						if (length != 16)
						{
							goto IL_1EA;
						}
						if (!(text == "decorativesprite"))
						{
							goto IL_1EA;
						}
						continue;
					}
					XElement xelement;
					if (saveElements == null)
					{
						xelement = null;
					}
					else
					{
						xelement = saveElements.Find((XElement e) => string.Equals(e.Name.ToString(), "This", StringComparison.OrdinalIgnoreCase));
					}
					XElement savedRootElement = xelement;
					PropertyReference[] rootProperties = PropertyReference.ParseAttributes(subElement.Attributes(), this);
					targetProperties.Add(targetEntity, rootProperties);
					foreach (PropertyReference propertyRef in rootProperties)
					{
						propertyRef.ApplySavedValue(savedRootElement);
					}
					continue;
				}
				IL_1EA:
				Item item = targetEntity as Item;
				if (item != null)
				{
					ISerializableEntity[] itemComponents = Upgrade.FindItemComponent(item, subElement.Name.ToString());
					if (itemComponents != null && itemComponents.Any<ISerializableEntity>())
					{
						ISerializableEntity[] array2 = itemComponents;
						for (int j = 0; j < array2.Length; j++)
						{
							ISerializableEntity sEntity = array2[j];
							XElement savedElement = (saveElements != null) ? saveElements.Find((XElement e) => string.Equals(e.Name.ToString(), sEntity.Name, StringComparison.OrdinalIgnoreCase)) : null;
							PropertyReference[] properties = PropertyReference.ParseAttributes(subElement.Attributes(), this);
							foreach (PropertyReference propertyRef2 in properties)
							{
								propertyRef2.ApplySavedValue(savedElement);
							}
							targetProperties.Add(sEntity, properties);
						}
					}
				}
			}
			this.TargetComponents = targetProperties;
			if (saveElement != null)
			{
				this.ResetNonAffectedProperties(saveElement);
			}
		}

		// Token: 0x06004410 RID: 17424 RVA: 0x00255D7C File Offset: 0x00253F7C
		private void ResetNonAffectedProperties(XContainer saveElement)
		{
			using (IEnumerator<XElement> enumerator = saveElement.Elements().Elements<XElement>().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					XElement element = enumerator.Current;
					if (!(from @ref in this.TargetComponents.SelectMany((KeyValuePair<ISerializableEntity, PropertyReference[]> pair) => pair.Value)
					select @ref.Name).Any(delegate(Identifier identifier)
					{
						Identifier identifier2 = element.NameAsIdentifier();
						return identifier == identifier2;
					}))
					{
						string value = element.GetAttributeString("value", string.Empty);
						Identifier name = element.NameAsIdentifier();
						XElement parent = element.Parent;
						if (parent == null)
						{
							throw new NullReferenceException("Unable to reset properties: Parent element is null.");
						}
						XElement parentElement = parent;
						string componentName = parentElement.Name.ToString();
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(208, 4);
						defaultInterpolatedStringHandler.AppendLiteral("Upgrade \"");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Prefab.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" in ");
						defaultInterpolatedStringHandler.AppendFormatted(this.TargetEntity.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" does not affect the property \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(name);
						defaultInterpolatedStringHandler.AppendLiteral("\" but the save file suggest it has done so before (has it been overriden?). \n");
						defaultInterpolatedStringHandler.AppendLiteral("The property has been reset to the original value of ");
						defaultInterpolatedStringHandler.AppendFormatted(value);
						defaultInterpolatedStringHandler.AppendLiteral(" and will be ignored from now on.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						if (string.Equals(componentName, "This", StringComparison.OrdinalIgnoreCase))
						{
							SerializableProperty property;
							if (this.TargetEntity.SerializableProperties.TryGetValue(name, out property) && property != null)
							{
								property.SetValue(this.TargetEntity, Convert.ChangeType(value, property.GetValue(this.TargetEntity).GetType(), NumberFormatInfo.InvariantInfo));
							}
						}
						else
						{
							Item item = this.TargetEntity as Item;
							if (item != null)
							{
								ISerializableEntity[] foundComponents = Upgrade.FindItemComponent(item, componentName);
								if (foundComponents != null)
								{
									foreach (ISerializableEntity serializableEntity in foundComponents)
									{
										SerializableProperty property2;
										if (serializableEntity.SerializableProperties.TryGetValue(name, out property2) && property2 != null)
										{
											property2.SetValue(serializableEntity, Convert.ChangeType(value, property2.GetValue(serializableEntity).GetType(), NumberFormatInfo.InvariantInfo));
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06004411 RID: 17425 RVA: 0x00255FFC File Offset: 0x002541FC
		[return: Nullable(new byte[]
		{
			2,
			1
		})]
		private static ISerializableEntity[] FindItemComponent(Item item, string name)
		{
			Type type = Type.GetType("Barotrauma.Items.Components." + name.ToLowerInvariant(), false, true);
			if (!(type != null))
			{
				return null;
			}
			if (item.Components.Count((ItemComponent ic) => ic.GetType() == type) == 0)
			{
				return null;
			}
			IEnumerable<ItemComponent> itemComponents = from ic in item.Components
			where ic.GetType() == type
			select ic;
			return itemComponents.Cast<ISerializableEntity>().ToArray<ISerializableEntity>();
		}

		// Token: 0x06004412 RID: 17426 RVA: 0x0025607C File Offset: 0x0025427C
		public void Save(XElement element)
		{
			XElement upgrade = new XElement("Upgrade", new object[]
			{
				new XAttribute("identifier", this.Identifier),
				new XAttribute("level", this.Level)
			});
			foreach (KeyValuePair<ISerializableEntity, PropertyReference[]> targetComponent in this.TargetComponents)
			{
				KeyValuePair<ISerializableEntity, PropertyReference[]> keyValuePair = targetComponent;
				ISerializableEntity serializableEntity;
				PropertyReference[] array;
				keyValuePair.Deconstruct(out serializableEntity, out array);
				ISerializableEntity key = serializableEntity;
				PropertyReference[] value = array;
				string name = (key is ItemComponent) ? key.Name : "This";
				XElement subElement = new XElement(name);
				foreach (PropertyReference propertyRef in value)
				{
					if (propertyRef.OriginalValue != null)
					{
						subElement.Add(new XElement(propertyRef.Name.Value, new XAttribute("value", propertyRef.OriginalValue)));
					}
					else if (!this.Prefab.SuppressWarnings)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to save upgrade \"");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Prefab.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" on ");
						defaultInterpolatedStringHandler.AppendFormatted(this.TargetEntity.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" because property reference \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(propertyRef.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" is missing original values. \n");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "Upgrades should always call Upgrade.ApplyUpgrade() or manually set the original value in a property reference after they have been added. \nIf you are not a developer submit a bug report at https://github.com/Regalis11/Barotrauma/issues/.", this.Prefab.ContentPackage);
					}
				}
				upgrade.Add(subElement);
			}
			element.Add(upgrade);
		}

		// Token: 0x06004413 RID: 17427 RVA: 0x00256278 File Offset: 0x00254478
		public void ApplyUpgrade()
		{
			foreach (KeyValuePair<ISerializableEntity, PropertyReference[]> keyValuePair in this.TargetComponents)
			{
				KeyValuePair<ISerializableEntity, PropertyReference[]> keyValuePair2 = keyValuePair;
				ISerializableEntity serializableEntity;
				PropertyReference[] array;
				keyValuePair2.Deconstruct(out serializableEntity, out array);
				ISerializableEntity entity = serializableEntity;
				PropertyReference[] properties = array;
				foreach (PropertyReference propertyReference in properties)
				{
					SerializableProperty property;
					if (entity.SerializableProperties.TryGetValue(propertyReference.Name, out property) && property != null)
					{
						object originalValue = property.GetValue(entity);
						propertyReference.SetOriginalValue(originalValue);
						object newValue = Convert.ChangeType(propertyReference.CalculateUpgrade(this.Level), originalValue.GetType(), NumberFormatInfo.InvariantInfo);
						property.SetValue(entity, newValue);
					}
					else
					{
						string matchingString = string.Empty;
						int closestMatch = int.MaxValue;
						foreach (KeyValuePair<Identifier, SerializableProperty> keyValuePair3 in entity.SerializableProperties)
						{
							Identifier identifier;
							SerializableProperty serializableProperty;
							keyValuePair3.Deconstruct(out identifier, out serializableProperty);
							Identifier propertyName = identifier;
							int match = ToolBox.LevenshteinDistance(propertyName.Value, propertyReference.Name.Value);
							if (match < closestMatch)
							{
								matchingString = (propertyName.Value ?? "");
								closestMatch = match;
							}
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(120, 4);
						defaultInterpolatedStringHandler.AppendLiteral("The upgrade \"");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Prefab.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" cannot be applied to ");
						defaultInterpolatedStringHandler.AppendFormatted(entity.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" because it does not contain the property \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(propertyReference.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" and has been ignored. \n");
						defaultInterpolatedStringHandler.AppendLiteral("Did you mean \"");
						defaultInterpolatedStringHandler.AppendFormatted(matchingString);
						defaultInterpolatedStringHandler.AppendLiteral("\"?");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
				}
			}
		}

		// Token: 0x06004414 RID: 17428 RVA: 0x002564A0 File Offset: 0x002546A0
		public void Dispose()
		{
			if (!this.Disposed)
			{
				this.TargetComponents.Clear();
			}
			this.Disposed = true;
		}

		// Token: 0x040023B2 RID: 9138
		private readonly ContentXElement sourceElement;
	}
}
