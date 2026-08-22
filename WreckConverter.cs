using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000337 RID: 823
	[NullableContext(1)]
	[Nullable(0)]
	internal static class WreckConverter
	{
		// Token: 0x0600414D RID: 16717 RVA: 0x00244720 File Offset: 0x00242920
		public static XElement ConvertToWreck(XElement submarineElement)
		{
			ImmutableHashSet<Identifier> availableWreckContainerTags = (from t in ItemPrefab.Prefabs.SelectMany((ItemPrefab ip) => ip.PreferredContainers.SelectMany((PreferredContainer pc) => pc.Primary.Union(pc.Secondary)))
			where !ItemPrefab.Prefabs.ContainsKey(t) && t.StartsWith("wreck")
			select t).ToImmutableHashSet<Identifier>();
			bool monsterSpawnPointCreated = false;
			List<string> warnings = new List<string>();
			XElement wreckElement = new XElement(submarineElement);
			foreach (XElement element in wreckElement.Elements().ToList<XElement>())
			{
				Identifier identifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
				if (identifier.IsEmpty)
				{
					Identifier identifier2 = element.NameAsIdentifier();
					if (identifier2 == "waypoint" && element.GetAttributeEnum("spawn", SpawnType.Path) == SpawnType.Human)
					{
						identifier2 = element.GetAttributeIdentifier("job", Identifier.Empty);
						if (identifier2 == Identifier.Empty)
						{
							element.SetAttributeValue("spawn", SpawnType.Enemy);
							DebugConsole.NewMessage("Converted a non-job-specific spawnpoint to an enemy spawnpoint.", null, false);
							monsterSpawnPointCreated = true;
						}
						else
						{
							element.SetAttributeValue("spawn", SpawnType.Corpse);
						}
					}
				}
				else
				{
					Identifier[] tags = element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
					if (WreckConverter.itemsToRemove.Any((string it) => tags.Contains(it.ToIdentifier())))
					{
						element.Remove();
					}
					else
					{
						bool tagsModified = false;
						for (int i = 0; i < tags.Length; i++)
						{
							Identifier wreckTag = ("wreck" + tags[i].ToString()).ToIdentifier();
							if (availableWreckContainerTags.Contains(wreckTag))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 3);
								defaultInterpolatedStringHandler.AppendLiteral("Replaced tag ");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(tags[i]);
								defaultInterpolatedStringHandler.AppendLiteral(" with ");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(wreckTag);
								defaultInterpolatedStringHandler.AppendLiteral(" in item \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
								defaultInterpolatedStringHandler.AppendLiteral("\".");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
								tags[i] = wreckTag;
								tagsModified = true;
							}
						}
						if (tagsModified)
						{
							element.SetAttributeValue("tags", string.Join(",", from t in tags
							select t.ToString()));
						}
						Identifier[] wreckedIdentifiers = new Identifier[]
						{
							(identifier.ToString() + "wrecked").ToIdentifier(),
							(identifier.ToString() + "_wrecked").ToIdentifier()
						};
						Identifier[] array = wreckedIdentifiers;
						int j = 0;
						while (j < array.Length)
						{
							Identifier wreckedIdentifier = array[j];
							MapEntityPrefab wreckedPrefab = MapEntityPrefab.FindByIdentifier(wreckedIdentifier);
							if (wreckedPrefab != null)
							{
								MapEntityPrefab oldPrefab = MapEntityPrefab.FindByIdentifier(identifier);
								element.SetAttributeValue("identifier", wreckedIdentifier);
								float currentScale = element.GetAttributeFloat("scale", oldPrefab.Scale);
								element.SetAttributeValue("scale", currentScale * (wreckedPrefab.Scale / oldPrefab.Scale));
								ItemPrefab wreckedItemPrefab = wreckedPrefab as ItemPrefab;
								if (wreckedItemPrefab != null)
								{
									XElement originalConnectionPanelElement = element.GetChildElement("ConnectionPanel", StringComparison.OrdinalIgnoreCase);
									ContentXElement wreckedConnectionPanelElement = wreckedItemPrefab.ConfigElement.GetChildElement("ConnectionPanel");
									if (originalConnectionPanelElement == null)
									{
										break;
									}
									ContentXElement contentXElement = null;
									if (!(wreckedConnectionPanelElement != contentXElement))
									{
										break;
									}
									using (List<XElement>.Enumerator enumerator2 = originalConnectionPanelElement.Elements().ToList<XElement>().GetEnumerator())
									{
										while (enumerator2.MoveNext())
										{
											XElement connectionElement = enumerator2.Current;
											Identifier elementName = connectionElement.NameAsIdentifier();
											if (!(elementName != "input") || !(elementName != "output"))
											{
												string connectionName = connectionElement.GetAttributeString("name", string.Empty);
												if (wreckedConnectionPanelElement.GetChildElements(connectionElement.Name.LocalName).None((ContentXElement c) => c.GetAttributeString("name", string.Empty) == connectionName))
												{
													connectionElement.Remove();
												}
											}
										}
										break;
									}
								}
								StructurePrefab wreckedStructurePrefab = wreckedPrefab as StructurePrefab;
								if (wreckedStructurePrefab != null)
								{
									Rectangle rect = element.GetAttributeRect("rect", Rectangle.Empty);
									if (!wreckedStructurePrefab.ResizeHorizontal)
									{
										if (Math.Abs(wreckedStructurePrefab.ScaledSize.X - (float)rect.Width) > 5f)
										{
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(89, 3);
											defaultInterpolatedStringHandler2.AppendLiteral("The prefab ");
											defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(wreckedStructurePrefab.Name);
											defaultInterpolatedStringHandler2.AppendLiteral(" has different dimensions than the original one. Changing the width from ");
											defaultInterpolatedStringHandler2.AppendFormatted<int>(rect.Width);
											defaultInterpolatedStringHandler2.AppendLiteral(" to ");
											defaultInterpolatedStringHandler2.AppendFormatted<int>((int)wreckedStructurePrefab.ScaledSize.X);
											defaultInterpolatedStringHandler2.AppendLiteral(".");
											DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Yellow), false);
										}
										rect.Width = (int)wreckedStructurePrefab.ScaledSize.X;
									}
									if (!wreckedStructurePrefab.ResizeVertical)
									{
										if (Math.Abs(wreckedStructurePrefab.ScaledSize.Y - (float)rect.Height) > 5f)
										{
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(90, 3);
											defaultInterpolatedStringHandler3.AppendLiteral("The prefab ");
											defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(wreckedStructurePrefab.Name);
											defaultInterpolatedStringHandler3.AppendLiteral(" has different dimensions than the original one. Changing the height from ");
											defaultInterpolatedStringHandler3.AppendFormatted<int>(rect.Height);
											defaultInterpolatedStringHandler3.AppendLiteral(" to ");
											defaultInterpolatedStringHandler3.AppendFormatted<int>((int)wreckedStructurePrefab.ScaledSize.Y);
											defaultInterpolatedStringHandler3.AppendLiteral(".");
											DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), new Color?(Color.Yellow), false);
										}
										rect.Height = (int)wreckedStructurePrefab.ScaledSize.Y;
									}
									element.SetAttributeValue("rect", XMLExtensions.RectToString(rect));
									break;
								}
								break;
							}
							else
							{
								j++;
							}
						}
						XElement itemContainerElement = element.GetChildElement("ItemContainer", StringComparison.OrdinalIgnoreCase);
						if (itemContainerElement != null)
						{
							string containedString = itemContainerElement.GetAttributeString("contained", "");
							string[] itemIdStrings = containedString.Split(',', StringSplitOptions.None);
							HashSet<ushort> itemIds = new HashSet<ushort>();
							foreach (string idListStr in itemIdStrings)
							{
								foreach (string idStr in idListStr.Split(';', StringSplitOptions.None))
								{
									int id;
									if (int.TryParse(idStr, out id))
									{
										itemIds.Add((ushort)id);
									}
								}
							}
							if (itemIds.Any<ushort>())
							{
								List<string> containedItemNames = new List<string>();
								foreach (XElement itemElement in wreckElement.Elements())
								{
									ushort id2 = itemElement.GetAttributeUInt16("id", 0);
									if (itemIds.Contains(id2))
									{
										containedItemNames.Add(itemElement.GetAttributeString("identifier", string.Empty));
									}
								}
								List<string> list = warnings;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(125, 1);
								defaultInterpolatedStringHandler4.AppendLiteral("Potential issue in container \"");
								defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(identifier);
								defaultInterpolatedStringHandler4.AppendLiteral("\". The following items are pre-placed, and may interfere with the loot generated in the wreck: ");
								list.Add(defaultInterpolatedStringHandler4.ToStringAndClear() + string.Join(", ", containedItemNames));
							}
						}
						if (element.GetChildElement("Repairable", StringComparison.OrdinalIgnoreCase) != null && element.GetChildElement("Door", StringComparison.OrdinalIgnoreCase) == null)
						{
							element.SetAttributeValue("conditionpercentage", 0f);
						}
					}
				}
			}
			foreach (string warning in warnings)
			{
				DebugConsole.AddWarning(warning, null);
			}
			if (!monsterSpawnPointCreated)
			{
				DebugConsole.ThrowError("There are no monster spawnpoints in the wreck. Remember to add some for monsters to spawn properly!", null, null, false, false);
			}
			return wreckElement;
		}

		// Token: 0x04002214 RID: 8724
		private static readonly string[] itemsToRemove = new string[]
		{
			"circuitboxcomponent",
			"wire"
		};
	}
}
