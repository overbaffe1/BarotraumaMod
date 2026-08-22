using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002D5 RID: 725
	internal static class ToolBox
	{
		// Token: 0x17000DF1 RID: 3569
		// (get) Token: 0x060030C3 RID: 12483 RVA: 0x0014E655 File Offset: 0x0014C855
		public static Assembly BarotraumaAssembly
		{
			get
			{
				return Assembly.GetAssembly(typeof(GameMain));
			}
		}

		// Token: 0x060030C4 RID: 12484 RVA: 0x0014E668 File Offset: 0x0014C868
		public static bool IsProperFilenameCase(string filename)
		{
			return true;
		}

		// Token: 0x060030C5 RID: 12485 RVA: 0x0014E678 File Offset: 0x0014C878
		public static string CorrectFilenameCase(string filename, out bool corrected, string directory = "")
		{
			char[] delimiters = new char[]
			{
				'/',
				'\\'
			};
			string[] subDirs = filename.Split(delimiters);
			string originalFilename = filename;
			filename = "";
			corrected = false;
			string existingName;
			if (ToolBox.cachedFileNames.TryGetValue(originalFilename, out existingName))
			{
				return existingName;
			}
			string startPath = directory ?? "";
			string saveFolder = SaveUtil.DefaultSaveFolder.Replace('\\', '/');
			if (originalFilename.Replace('\\', '/').StartsWith(saveFolder))
			{
				startPath = (saveFolder.EndsWith('/') ? saveFolder : (saveFolder + "/"));
				filename = startPath;
				subDirs = subDirs.Skip(saveFolder.Split('/', StringSplitOptions.None).Length).ToArray<string>();
			}
			else if (Path.IsPathRooted(originalFilename))
			{
				return originalFilename;
			}
			for (int i = 0; i < subDirs.Length; i++)
			{
				if (i == subDirs.Length - 1 && string.IsNullOrEmpty(subDirs[i]))
				{
					break;
				}
				string subDir = subDirs[i].TrimEnd();
				string enumPath = Path.Combine(new string[]
				{
					startPath,
					filename
				});
				if (string.IsNullOrWhiteSpace(filename))
				{
					enumPath = (string.IsNullOrWhiteSpace(startPath) ? "./" : startPath);
				}
				IEnumerable<string> fileSystemEntries = Directory.GetFileSystemEntries(enumPath);
				Func<string, string> selector;
				if ((selector = ToolBox.<>O.<0>__GetFileName) == null)
				{
					selector = (ToolBox.<>O.<0>__GetFileName = new Func<string, string>(Path.GetFileName));
				}
				string[] filePaths = fileSystemEntries.Select(selector).ToArray<string>();
				if (filePaths.Any((string s) => s.Equals(subDir, StringComparison.Ordinal)))
				{
					filename += subDir;
				}
				else
				{
					string[] correctedPaths = (from s in filePaths
					where s.Equals(subDir, StringComparison.OrdinalIgnoreCase)
					select s).ToArray<string>();
					if (!correctedPaths.Any<string>())
					{
						corrected = false;
						return originalFilename;
					}
					corrected = true;
					filename += correctedPaths.First<string>();
				}
				if (i < subDirs.Length - 1)
				{
					filename += "/";
				}
			}
			ToolBox.cachedFileNames.TryAdd(originalFilename, filename);
			return filename;
		}

		// Token: 0x060030C6 RID: 12486 RVA: 0x0014E860 File Offset: 0x0014CA60
		public static string RemoveInvalidFileNameChars(string fileName)
		{
			IEnumerable<char> invalidChars = Path.GetInvalidFileNameCharsCrossPlatform().Concat(new char[]
			{
				';'
			});
			foreach (char invalidChar in invalidChars)
			{
				fileName = fileName.Replace(invalidChar.ToString(), "");
			}
			return fileName;
		}

		// Token: 0x060030C7 RID: 12487 RVA: 0x0014E8CC File Offset: 0x0014CACC
		public static string RemoveBBCodeTags(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			return ToolBox.removeBBCodeRegex.Replace(str, "");
		}

		// Token: 0x060030C8 RID: 12488 RVA: 0x0014E8E8 File Offset: 0x0014CAE8
		public static string RandomSeed(int length)
		{
			string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			return new string((from s in Enumerable.Repeat<string>(chars, length)
			select s[Rand.Int(s.Length, Rand.RandSync.Unsynced)]).ToArray<char>());
		}

		// Token: 0x060030C9 RID: 12489 RVA: 0x0014E930 File Offset: 0x0014CB30
		public static int IdentifierToInt(Identifier id)
		{
			return ToolBox.StringToInt(id.Value.ToLowerInvariant());
		}

		// Token: 0x060030CA RID: 12490 RVA: 0x0014E944 File Offset: 0x0014CB44
		public static int StringToInt(string str)
		{
			int hash = 352654597;
			int hash2 = hash;
			for (int i = 0; i < str.Length; i += 2)
			{
				hash = ((hash << 5) + hash ^ (int)str[i]);
				if (i == str.Length - 1)
				{
					break;
				}
				hash2 = ((hash2 << 5) + hash2 ^ (int)str[i + 1]);
			}
			return hash + hash2 * 1566083941;
		}

		// Token: 0x060030CB RID: 12491 RVA: 0x0014E99C File Offset: 0x0014CB9C
		public static string ConvertInputType(string inputType)
		{
			if (inputType == "ActionHit" || inputType == "Action")
			{
				return "Use";
			}
			if (inputType == "SecondaryHit" || inputType == "Secondary")
			{
				return "Aim";
			}
			return inputType;
		}

		// Token: 0x060030CC RID: 12492 RVA: 0x0014E9EA File Offset: 0x0014CBEA
		public static string GetDebugSymbol(bool isFinished, bool isRunning = false)
		{
			if (!isRunning)
			{
				return "[‖color:" + (isFinished ? "0,255,0‖x" : "255,0,0‖o") + "‖color:end‖]";
			}
			return "[‖color:243,162,50‖x‖color:end‖]";
		}

		// Token: 0x060030CD RID: 12493 RVA: 0x0014EA14 File Offset: 0x0014CC14
		public static string ColorizeObject(this object obj)
		{
			string text;
			if (obj is bool)
			{
				bool b = (bool)obj;
				text = (b ? "80,250,123" : "255,85,85");
			}
			else if (!(obj is string))
			{
				if (!(obj is Identifier))
				{
					if (!(obj is int))
					{
						if (!(obj is float))
						{
							if (!(obj is double))
							{
								if (obj != null)
								{
									text = "139,233,253";
								}
								else
								{
									text = "255,85,85";
								}
							}
							else
							{
								text = "189,147,249";
							}
						}
						else
						{
							text = "189,147,249";
						}
					}
					else
					{
						text = "189,147,249";
					}
				}
				else
				{
					text = "241,250,140";
				}
			}
			else
			{
				text = "241,250,140";
			}
			string color = text;
			if (!(obj is string) && !(obj is Identifier))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 2);
				defaultInterpolatedStringHandler.AppendLiteral("‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(color);
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted<object>(obj ?? "null");
				defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(21, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("‖color:");
			defaultInterpolatedStringHandler2.AppendFormatted(color);
			defaultInterpolatedStringHandler2.AppendLiteral("‖\"");
			defaultInterpolatedStringHandler2.AppendFormatted<object>(obj);
			defaultInterpolatedStringHandler2.AppendLiteral("\"‖color:end‖");
			return defaultInterpolatedStringHandler2.ToStringAndClear();
		}

		// Token: 0x060030CE RID: 12494 RVA: 0x0014EB50 File Offset: 0x0014CD50
		public static Vector3 RgbToHLS(Vector3 color)
		{
			double double_r = (double)color.X;
			double double_g = (double)color.Y;
			double double_b = (double)color.Z;
			double max = double_r;
			if (max < double_g)
			{
				max = double_g;
			}
			if (max < double_b)
			{
				max = double_b;
			}
			double min = double_r;
			if (min > double_g)
			{
				min = double_g;
			}
			if (min > double_b)
			{
				min = double_b;
			}
			double diff = max - min;
			double i = (max + min) / 2.0;
			double s;
			double h;
			if (Math.Abs(diff) < 1E-05)
			{
				s = 0.0;
				h = 0.0;
			}
			else
			{
				if (i <= 0.5)
				{
					s = diff / (max + min);
				}
				else
				{
					s = diff / (2.0 - max - min);
				}
				double r_dist = (max - double_r) / diff;
				double g_dist = (max - double_g) / diff;
				double b_dist = (max - double_b) / diff;
				if (double_r == max)
				{
					h = b_dist - g_dist;
				}
				else if (double_g == max)
				{
					h = 2.0 + r_dist - b_dist;
				}
				else
				{
					h = 4.0 + g_dist - r_dist;
				}
				h *= 60.0;
				if (h < 0.0)
				{
					h += 360.0;
				}
			}
			return new Vector3((float)h, (float)i, (float)s);
		}

		// Token: 0x060030CF RID: 12495 RVA: 0x0014EC94 File Offset: 0x0014CE94
		public static int LevenshteinDistance(string s, string t)
		{
			int i = s.Length;
			int j = t.Length;
			int[,] d = new int[i + 1, j + 1];
			if (i == 0 || j == 0)
			{
				return 0;
			}
			int k = 0;
			while (k <= i)
			{
				d[k, 0] = k++;
			}
			int l = 0;
			while (l <= j)
			{
				d[0, l] = l++;
			}
			for (int m = 1; m <= i; m++)
			{
				for (int n = 1; n <= j; n++)
				{
					int cost = (t[n - 1] != s[m - 1]) ? 1 : 0;
					d[m, n] = Math.Min(Math.Min(d[m - 1, n] + 1, d[m, n - 1] + 1), d[m - 1, n - 1] + cost);
				}
			}
			return d[i, j];
		}

		// Token: 0x060030D0 RID: 12496 RVA: 0x0014ED78 File Offset: 0x0014CF78
		public static LocalizedString SecondsToReadableTime(float seconds)
		{
			int s = (int)(seconds % 60f);
			if (seconds < 60f)
			{
				return TextManager.GetWithVariable("timeformatseconds", "[seconds]", s.ToString(), FormatCapitals.No);
			}
			int h = (int)(seconds / 3600f);
			int i = (int)(seconds / 60f % 60f);
			LocalizedString text = "";
			if (h != 0)
			{
				text = TextManager.GetWithVariable("timeformathours", "[hours]", h.ToString(), FormatCapitals.No);
			}
			if (i != 0)
			{
				LocalizedString minutesText = TextManager.GetWithVariable("timeformatminutes", "[minutes]", i.ToString(), FormatCapitals.No);
				text = (text.IsNullOrEmpty() ? minutesText : LocalizedString.Join(" ", new LocalizedString[]
				{
					text,
					minutesText
				}));
			}
			if (s != 0)
			{
				LocalizedString secondsText = TextManager.GetWithVariable("timeformatseconds", "[seconds]", s.ToString(), FormatCapitals.No);
				text = (text.IsNullOrEmpty() ? secondsText : LocalizedString.Join(" ", new LocalizedString[]
				{
					text,
					secondsText
				}));
			}
			return text;
		}

		// Token: 0x060030D1 RID: 12497 RVA: 0x0014EE88 File Offset: 0x0014D088
		public static string GetRandomLine(string filePath, Rand.RandSync randSync = Rand.RandSync.ServerAndClient)
		{
			List<string> lines;
			if (ToolBox.cachedLines.ContainsKey(filePath))
			{
				lines = ToolBox.cachedLines[filePath];
			}
			else
			{
				try
				{
					lines = File.ReadAllLines(filePath, null, false).ToList<string>();
					ToolBox.cachedLines.Add(filePath, lines);
					if (lines.Count == 0)
					{
						DebugConsole.ThrowError("File \"" + filePath + "\" is empty!", null, null, false, false);
						return "";
					}
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Couldn't open file \"" + filePath + "\"!", e, null, false, false);
					return "";
				}
			}
			if (lines.Count == 0)
			{
				return "";
			}
			return lines[Rand.Range(0, lines.Count, randSync)];
		}

		// Token: 0x060030D2 RID: 12498 RVA: 0x0014EF4C File Offset: 0x0014D14C
		public static IReadMessage ExtractBits(this IReadMessage originalBuffer, int numberOfBits)
		{
			ReadWriteMessage buffer = new ReadWriteMessage();
			for (int i = 0; i < numberOfBits; i++)
			{
				bool bit = originalBuffer.ReadBoolean();
				buffer.WriteBoolean(bit);
			}
			buffer.BitPosition = 0;
			return buffer;
		}

		// Token: 0x060030D3 RID: 12499 RVA: 0x0014EF81 File Offset: 0x0014D181
		public static T SelectWeightedRandom<T>(IEnumerable<T> objects, Func<T, float> weightMethod, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<T>(objects, weightMethod, Rand.GetRNG(randSync));
		}

		// Token: 0x060030D4 RID: 12500 RVA: 0x0014EF90 File Offset: 0x0014D190
		public static T SelectWeightedRandom<T>(IEnumerable<T> objects, Func<T, float> weightMethod, Random random)
		{
			if (typeof(PrefabWithUintIdentifier).IsAssignableFrom(typeof(T)))
			{
				objects = objects.OrderBy(delegate(T p)
				{
					PrefabWithUintIdentifier prefabWithUintIdentifier = p as PrefabWithUintIdentifier;
					if (prefabWithUintIdentifier == null)
					{
						return 0U;
					}
					return prefabWithUintIdentifier.UintIdentifier;
				});
			}
			List<T> objectList = objects.ToList<T>();
			List<float> weights = objectList.Select(weightMethod).ToList<float>();
			return ToolBox.SelectWeightedRandom<T>(objectList, weights, random);
		}

		// Token: 0x060030D5 RID: 12501 RVA: 0x0014EFFB File Offset: 0x0014D1FB
		public static T SelectWeightedRandom<T>(IList<T> objects, IList<float> weights, Rand.RandSync randSync)
		{
			return ToolBox.SelectWeightedRandom<T>(objects, weights, Rand.GetRNG(randSync));
		}

		// Token: 0x060030D6 RID: 12502 RVA: 0x0014F00C File Offset: 0x0014D20C
		public static T SelectWeightedRandom<T>(IList<T> objects, IList<float> weights, Random random)
		{
			if (objects.Count == 0)
			{
				return default(T);
			}
			if (objects.Count != weights.Count)
			{
				DebugConsole.ThrowError("Error in SelectWeightedRandom, number of objects does not match the number of weights.\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return objects[0];
			}
			float totalWeight = weights.Sum();
			float randomNum = (float)(random.NextDouble() * (double)totalWeight);
			T objectWithNonZeroWeight = default(T);
			for (int i = 0; i < objects.Count; i++)
			{
				if (weights[i] > 0f)
				{
					objectWithNonZeroWeight = objects[i];
				}
				if (randomNum <= weights[i])
				{
					return objects[i];
				}
				randomNum -= weights[i];
			}
			return objectWithNonZeroWeight;
		}

		// Token: 0x060030D7 RID: 12503 RVA: 0x0014F0C5 File Offset: 0x0014D2C5
		public static T CreateCopy<T>(this T source, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) where T : new()
		{
			return ToolBox.CopyValues<T>(source, Activator.CreateInstance<T>(), flags);
		}

		// Token: 0x060030D8 RID: 12504 RVA: 0x0014F0D3 File Offset: 0x0014D2D3
		public static T CopyValuesTo<T>(this T source, T target, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
		{
			return ToolBox.CopyValues<T>(source, target, flags);
		}

		// Token: 0x060030D9 RID: 12505 RVA: 0x0014F0E0 File Offset: 0x0014D2E0
		public static T CopyValues<T>(T source, T destination, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
		{
			if (source == null)
			{
				throw new Exception("Failed to copy object. Source is null.");
			}
			if (destination == null)
			{
				throw new Exception("Failed to copy object. Destination is null.");
			}
			Type type = source.GetType();
			PropertyInfo[] properties = type.GetProperties(flags);
			foreach (PropertyInfo property in properties)
			{
				if (property.CanWrite)
				{
					property.SetValue(destination, property.GetValue(source, null), null);
				}
			}
			FieldInfo[] fields = type.GetFields(flags);
			foreach (FieldInfo field in fields)
			{
				field.SetValue(destination, field.GetValue(source));
			}
			return destination;
		}

		// Token: 0x060030DA RID: 12506 RVA: 0x0014F1A8 File Offset: 0x0014D3A8
		public static void SiftElement<T>(this List<T> list, int from, int to)
		{
			if (from < 0 || from >= list.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 2);
				defaultInterpolatedStringHandler.AppendLiteral("from parameter out of range (from=");
				defaultInterpolatedStringHandler.AppendFormatted<int>(from);
				defaultInterpolatedStringHandler.AppendLiteral(", range=[0..");
				defaultInterpolatedStringHandler.AppendFormatted<int>(list.Count - 1);
				defaultInterpolatedStringHandler.AppendLiteral("])");
				throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (to < 0 || to >= list.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("to parameter out of range (to=");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(to);
				defaultInterpolatedStringHandler2.AppendLiteral(", range=[0..");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(list.Count - 1);
				defaultInterpolatedStringHandler2.AppendLiteral("])");
				throw new ArgumentException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			T elem = list[from];
			if (from > to)
			{
				for (int i = from; i > to; i--)
				{
					list[i] = list[i - 1];
				}
				list[to] = elem;
				return;
			}
			if (from < to)
			{
				for (int j = from; j < to; j++)
				{
					list[j] = list[j + 1];
				}
				list[to] = elem;
			}
		}

		// Token: 0x060030DB RID: 12507 RVA: 0x0014F2D2 File Offset: 0x0014D4D2
		public static string EscapeCharacters(string str)
		{
			return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}

		// Token: 0x060030DC RID: 12508 RVA: 0x0014F2F4 File Offset: 0x0014D4F4
		public static string UnescapeCharacters(string str)
		{
			string retVal = "";
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != '\\')
				{
					ReadOnlySpan<char> str2 = retVal;
					char c = str[i];
					retVal = str2 + new ReadOnlySpan<char>(ref c);
				}
				else if (i + 1 < str.Length)
				{
					if (str[i + 1] == '\\')
					{
						retVal += "\\";
					}
					else if (str[i + 1] == '"')
					{
						retVal += "\"";
					}
					i++;
				}
			}
			return retVal;
		}

		// Token: 0x060030DD RID: 12509 RVA: 0x0014F384 File Offset: 0x0014D584
		public static string[] SplitCommand(string command)
		{
			command = command.Trim();
			List<string> commands = new List<string>();
			int escape = 0;
			bool inQuotes = false;
			string piece = "";
			for (int i = 0; i < command.Length; i++)
			{
				if (command[i] == '\\')
				{
					if (escape == 0)
					{
						escape = 2;
					}
					else
					{
						piece += "\\";
					}
				}
				else if (command[i] == '"')
				{
					if (escape == 0)
					{
						inQuotes = !inQuotes;
					}
					else
					{
						piece += "\"";
					}
				}
				else if (command[i] == ' ' && !inQuotes)
				{
					if (!string.IsNullOrWhiteSpace(piece))
					{
						commands.Add(piece);
					}
					piece = "";
				}
				else if (escape == 0)
				{
					ReadOnlySpan<char> str = piece;
					char c = command[i];
					piece = str + new ReadOnlySpan<char>(ref c);
				}
				if (escape > 0)
				{
					escape--;
				}
			}
			if (!string.IsNullOrWhiteSpace(piece))
			{
				commands.Add(piece);
			}
			return commands.ToArray();
		}

		// Token: 0x060030DE RID: 12510 RVA: 0x0014F46C File Offset: 0x0014D66C
		public static string CleanUpPathCrossPlatform(this string path, bool correctFilenameCase = true, string directory = "")
		{
			if (string.IsNullOrEmpty(path))
			{
				return "";
			}
			path = path.Replace('\\', '/');
			if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
			{
				path = path.Substring("file:".Length);
			}
			while (path.IndexOf("//") >= 0)
			{
				path = path.Replace("//", "/");
			}
			if (correctFilenameCase)
			{
				bool flag;
				string correctedPath = ToolBox.CorrectFilenameCase(path, out flag, directory);
				if (!string.IsNullOrEmpty(correctedPath))
				{
					path = correctedPath;
				}
			}
			return path;
		}

		// Token: 0x060030DF RID: 12511 RVA: 0x0014F4EE File Offset: 0x0014D6EE
		public static string CleanUpPath(this string path)
		{
			return path.CleanUpPathCrossPlatform(false, "");
		}

		// Token: 0x060030E0 RID: 12512 RVA: 0x0014F4FC File Offset: 0x0014D6FC
		public static float GetEasing(TransitionMode easing, float t)
		{
			float result;
			switch (easing)
			{
			case TransitionMode.Linear:
				result = t;
				break;
			case TransitionMode.Smooth:
				result = MathUtils.SmoothStep(t);
				break;
			case TransitionMode.Smoother:
				result = MathUtils.SmootherStep(t);
				break;
			case TransitionMode.EaseIn:
				result = MathUtils.EaseIn(t);
				break;
			case TransitionMode.EaseOut:
				result = MathUtils.EaseOut(t);
				break;
			case TransitionMode.Exponential:
				result = t * t;
				break;
			default:
				result = t;
				break;
			}
			return result;
		}

		// Token: 0x060030E1 RID: 12513 RVA: 0x0014F55C File Offset: 0x0014D75C
		public static Rectangle GetWorldBounds(Point center, Point size)
		{
			Point halfSize = size.Divide(2);
			Point topLeft = new Point(center.X - halfSize.X, center.Y + halfSize.Y);
			return new Rectangle(topLeft, size);
		}

		// Token: 0x060030E2 RID: 12514 RVA: 0x0014F599 File Offset: 0x0014D799
		public static void ThrowIfNull<T>([NotNull] T o)
		{
			if (o == null)
			{
				throw new ArgumentNullException();
			}
		}

		// Token: 0x060030E3 RID: 12515 RVA: 0x0014F5AC File Offset: 0x0014D7AC
		public static string GetFormattedPercentage(float v)
		{
			return TextManager.GetWithVariable("percentageformat", "[value]", ((int)MathF.Round(v * 100f)).ToString(), FormatCapitals.No).Value;
		}

		// Token: 0x060030E4 RID: 12516 RVA: 0x0014F5E8 File Offset: 0x0014D7E8
		public static string ExtendColorToPercentageSigns(string original)
		{
			char[] chars = original.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				char currentChar;
				if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|38_2(i, chars, out currentChar) && ToolBox.affectedCharacters.Contains(currentChar))
				{
					char c;
					if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|38_2(i - 1, chars, out c) && c == '‖')
					{
						int offset = "‖color:end‖".Length;
						if (ToolBox.<ExtendColorToPercentageSigns>g__MatchesSequence|38_1(i - offset, "‖color:end‖", chars))
						{
							char prev = currentChar;
							for (int j = i - offset; j <= i; j++)
							{
								if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|38_2(j, chars, out c))
								{
									chars[j] = prev;
									prev = c;
								}
							}
							goto IL_FA;
						}
					}
					if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|38_2(i + 1, chars, out c) && c == '‖' && ToolBox.<ExtendColorToPercentageSigns>g__MatchesSequence|38_1(i + 1, "‖color:", chars))
					{
						int offset2 = ToolBox.<ExtendColorToPercentageSigns>g__FindNextDefinitionOffset|38_0(i, "‖color:".Length, chars);
						if (offset2 <= chars.Length)
						{
							char prev2 = currentChar;
							for (int k = i + offset2; k >= i; k--)
							{
								if (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|38_2(k, chars, out c))
								{
									chars[k] = prev2;
									prev2 = c;
								}
							}
							i += offset2;
						}
					}
				}
				IL_FA:;
			}
			return new string(chars);
		}

		// Token: 0x060030E5 RID: 12517 RVA: 0x0014F702 File Offset: 0x0014D902
		public static bool StatIdentifierMatches(Identifier original, Identifier match)
		{
			return original == match || ToolBox.<StatIdentifierMatches>g__Matches|39_0(original, match) || ToolBox.<StatIdentifierMatches>g__Matches|39_0(match, original);
		}

		// Token: 0x060030E6 RID: 12518 RVA: 0x0014F723 File Offset: 0x0014D923
		public static bool EquivalentTo(this IPEndPoint self, IPEndPoint other)
		{
			return self.Address.EquivalentTo(other.Address) && self.Port == other.Port;
		}

		// Token: 0x060030E7 RID: 12519 RVA: 0x0014F748 File Offset: 0x0014D948
		public static bool EquivalentTo(this IPAddress self, IPAddress other)
		{
			if (self.IsIPv4MappedToIPv6)
			{
				self = self.MapToIPv4();
			}
			if (other.IsIPv4MappedToIPv6)
			{
				other = other.MapToIPv4();
			}
			return self.Equals(other);
		}

		// Token: 0x060030E8 RID: 12520 RVA: 0x0014F771 File Offset: 0x0014D971
		public static float ShortAudioSampleToFloat(short value)
		{
			return (float)value / 32767f;
		}

		// Token: 0x060030E9 RID: 12521 RVA: 0x0014F77C File Offset: 0x0014D97C
		public static short FloatToShortAudioSample(float value)
		{
			int temp = (int)(32767f * value);
			if (temp > 32767)
			{
				temp = 32767;
			}
			else if (temp < -32768)
			{
				temp = -32768;
			}
			return (short)temp;
		}

		// Token: 0x060030EA RID: 12522 RVA: 0x0014F7B4 File Offset: 0x0014D9B4
		public static SquareLine GetSquareLineBetweenPoints(Vector2 start, Vector2 end, float knobLength = 24f)
		{
			Vector2[] points = new Vector2[6];
			Vector2[] array = points;
			int num = 0;
			Vector2[] array2 = points;
			int num2 = 1;
			points[2] = start;
			array[num] = (array2[num2] = start);
			Vector2[] array3 = points;
			int num3 = 5;
			Vector2[] array4 = points;
			int num4 = 4;
			points[3] = end;
			array3[num3] = (array4[num4] = end);
			Vector2[] array5 = points;
			int num5 = 2;
			array5[num5].X = array5[num5].X + (points[3].X - points[2].X) / 2f;
			points[2].X = Math.Max(points[2].X, points[0].X + knobLength);
			points[3].X = points[2].X;
			bool isBehind = false;
			if (points[2].X <= points[0].X + knobLength)
			{
				isBehind = true;
				Vector2[] array6 = points;
				int num6 = 1;
				array6[num6].X = array6[num6].X + knobLength;
				points[2].X = points[2].X;
				Vector2[] array7 = points;
				int num7 = 2;
				array7[num7].Y = array7[num7].Y + (points[4].Y - points[1].Y) / 2f;
			}
			if (points[3].X >= points[5].X - knobLength)
			{
				isBehind = true;
				Vector2[] array8 = points;
				int num8 = 4;
				array8[num8].X = array8[num8].X - knobLength;
				points[3].X = points[4].X;
				Vector2[] array9 = points;
				int num9 = 3;
				array9[num9].Y = array9[num9].Y - (points[3].Y - points[2].Y);
			}
			SquareLine.LineType type = isBehind ? SquareLine.LineType.SixPointBackwardsLine : SquareLine.LineType.FourPointForwardsLine;
			return new SquareLine(points, type);
		}

		// Token: 0x060030EB RID: 12523 RVA: 0x0014F96C File Offset: 0x0014DB6C
		public static string BytesToHexString(byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in bytes)
			{
				sb.Append(b.ToString("X2"));
			}
			return sb.ToString();
		}

		// Token: 0x060030EC RID: 12524 RVA: 0x0014F9AC File Offset: 0x0014DBAC
		public static Vector2 GetClosestPointOnRectangle(RectangleF rect, Vector2 point)
		{
			Vector2 closest = new Vector2(MathHelper.Clamp(point.X, rect.Left, rect.Right), MathHelper.Clamp(point.Y, rect.Top, rect.Bottom));
			if (point.X < rect.Left)
			{
				closest.X = rect.Left;
			}
			else if (point.X > rect.Right)
			{
				closest.X = rect.Right;
			}
			if (point.Y < rect.Top)
			{
				closest.Y = rect.Top;
			}
			else if (point.Y > rect.Bottom)
			{
				closest.Y = rect.Bottom;
			}
			return closest;
		}

		// Token: 0x060030ED RID: 12525 RVA: 0x0014FA6B File Offset: 0x0014DC6B
		public static ImmutableArray<uint> PrefabCollectionToUintIdentifierArray(IEnumerable<PrefabWithUintIdentifier> prefabs)
		{
			return (from p in prefabs
			select p.UintIdentifier).ToImmutableArray<uint>();
		}

		// Token: 0x060030EE RID: 12526 RVA: 0x0014FA98 File Offset: 0x0014DC98
		public static ImmutableArray<T> UintIdentifierArrayToPrefabCollection<T>(PrefabCollection<T> Prefabs, IEnumerable<uint> uintIdentifiers) where T : PrefabWithUintIdentifier
		{
			ImmutableArray<T>.Builder builder = ImmutableArray.CreateBuilder<T>();
			using (IEnumerator<uint> enumerator = uintIdentifiers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					uint uintIdentifier = enumerator.Current;
					T matchingPrefab = Prefabs.Find((T p) => p.UintIdentifier == uintIdentifier);
					if (matchingPrefab == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unable to find prefab with uint identifier ");
						defaultInterpolatedStringHandler.AppendFormatted<uint>(uintIdentifier);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						builder.Add(matchingPrefab);
					}
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x060030F0 RID: 12528 RVA: 0x0014FB88 File Offset: 0x0014DD88
		[CompilerGenerated]
		internal static int <ExtendColorToPercentageSigns>g__FindNextDefinitionOffset|38_0(int index, int initialOffset, char[] chars)
		{
			int offset = initialOffset;
			char c;
			while (ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|38_2(index + offset, chars, out c) && c != '‖')
			{
				offset++;
			}
			return offset;
		}

		// Token: 0x060030F1 RID: 12529 RVA: 0x0014FBB4 File Offset: 0x0014DDB4
		[CompilerGenerated]
		internal static bool <ExtendColorToPercentageSigns>g__MatchesSequence|38_1(int index, string sequence, char[] chars)
		{
			for (int i = 0; i < sequence.Length; i++)
			{
				char c;
				if (!ToolBox.<ExtendColorToPercentageSigns>g__TryGetAt|38_2(index + i, chars, out c) || c != sequence[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060030F2 RID: 12530 RVA: 0x0014FBEC File Offset: 0x0014DDEC
		[CompilerGenerated]
		internal static bool <ExtendColorToPercentageSigns>g__TryGetAt|38_2(int i, char[] chars, out char c)
		{
			if (i >= 0 && i < chars.Length)
			{
				c = chars[i];
				return true;
			}
			c = '\0';
			return false;
		}

		// Token: 0x060030F3 RID: 12531 RVA: 0x0014FC04 File Offset: 0x0014DE04
		[CompilerGenerated]
		internal static bool <StatIdentifierMatches>g__Matches|39_0(Identifier a, Identifier b)
		{
			for (int i = 0; i < b.Value.Length; i++)
			{
				if (i >= a.Value.Length)
				{
					return b[i] == '~';
				}
				if (!ToolBox.<StatIdentifierMatches>g__CharEquals|39_1(a[i], b[i]))
				{
					return false;
				}
			}
			return false;
		}

		// Token: 0x060030F4 RID: 12532 RVA: 0x0014FC5E File Offset: 0x0014DE5E
		[CompilerGenerated]
		internal static bool <StatIdentifierMatches>g__CharEquals|39_1(char a, char b)
		{
			return char.ToLowerInvariant(a) == char.ToLowerInvariant(b);
		}

		// Token: 0x0400184C RID: 6220
		private static readonly ConcurrentDictionary<string, string> cachedFileNames = new ConcurrentDictionary<string, string>();

		// Token: 0x0400184D RID: 6221
		private static readonly Regex removeBBCodeRegex = new Regex("\\[\\/?(?:b|i|u|url|quote|code|img|color|size)*?.*?\\]");

		// Token: 0x0400184E RID: 6222
		private static Dictionary<string, List<string>> cachedLines = new Dictionary<string, List<string>>();

		// Token: 0x0400184F RID: 6223
		private static readonly ImmutableHashSet<char> affectedCharacters = ImmutableHashSet.Create<char>(new char[]
		{
			'%',
			'+',
			'％'
		});

		// Token: 0x02000B75 RID: 2933
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003984 RID: 14724
			public static Func<string, string> <0>__GetFileName;
		}
	}
}
