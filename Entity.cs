using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000313 RID: 787
	internal abstract class Entity : ISpatialEntity
	{
		// Token: 0x06003ED7 RID: 16087 RVA: 0x00233E57 File Offset: 0x00232057
		public static IReadOnlyCollection<Entity> GetEntities()
		{
			return Entity.dictionary.Values;
		}

		// Token: 0x17001078 RID: 4216
		// (get) Token: 0x06003ED8 RID: 16088 RVA: 0x00233E63 File Offset: 0x00232063
		public static int EntityCount
		{
			get
			{
				return Entity.dictionary.Count;
			}
		}

		// Token: 0x17001079 RID: 4217
		// (get) Token: 0x06003ED9 RID: 16089 RVA: 0x00233E6F File Offset: 0x0023206F
		// (set) Token: 0x06003EDA RID: 16090 RVA: 0x00233E77 File Offset: 0x00232077
		public bool Removed { get; private set; }

		// Token: 0x1700107A RID: 4218
		// (get) Token: 0x06003EDB RID: 16091 RVA: 0x00233E80 File Offset: 0x00232080
		// (set) Token: 0x06003EDC RID: 16092 RVA: 0x00233E88 File Offset: 0x00232088
		public bool IdFreed { get; private set; }

		// Token: 0x1700107B RID: 4219
		// (get) Token: 0x06003EDD RID: 16093 RVA: 0x00233E91 File Offset: 0x00232091
		public virtual Vector2 SimPosition
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x1700107C RID: 4220
		// (get) Token: 0x06003EDE RID: 16094 RVA: 0x00233E98 File Offset: 0x00232098
		public virtual Vector2 Position
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x1700107D RID: 4221
		// (get) Token: 0x06003EDF RID: 16095 RVA: 0x00233E9F File Offset: 0x0023209F
		public virtual Vector2 WorldPosition
		{
			get
			{
				if (this.Submarine != null)
				{
					return this.Submarine.Position + this.Position;
				}
				return this.Position;
			}
		}

		// Token: 0x1700107E RID: 4222
		// (get) Token: 0x06003EE0 RID: 16096 RVA: 0x00233EC6 File Offset: 0x002320C6
		public virtual Vector2 DrawPosition
		{
			get
			{
				if (this.Submarine != null)
				{
					return this.Submarine.DrawPosition + this.Position;
				}
				return this.Position;
			}
		}

		// Token: 0x1700107F RID: 4223
		// (get) Token: 0x06003EE1 RID: 16097 RVA: 0x00233EED File Offset: 0x002320ED
		// (set) Token: 0x06003EE2 RID: 16098 RVA: 0x00233EF5 File Offset: 0x002320F5
		public Submarine Submarine { get; set; }

		// Token: 0x17001080 RID: 4224
		// (get) Token: 0x06003EE3 RID: 16099 RVA: 0x00233EFE File Offset: 0x002320FE
		public AITarget AiTarget
		{
			get
			{
				return this.aiTarget;
			}
		}

		// Token: 0x17001081 RID: 4225
		// (get) Token: 0x06003EE4 RID: 16100 RVA: 0x00233F06 File Offset: 0x00232106
		// (set) Token: 0x06003EE5 RID: 16101 RVA: 0x00233F1D File Offset: 0x0023211D
		public bool InDetectable
		{
			get
			{
				return this.aiTarget != null && this.aiTarget.InDetectable;
			}
			set
			{
				if (this.aiTarget != null)
				{
					this.aiTarget.InDetectable = value;
				}
			}
		}

		// Token: 0x17001082 RID: 4226
		// (get) Token: 0x06003EE6 RID: 16102 RVA: 0x00233F33 File Offset: 0x00232133
		public double SpawnTime
		{
			get
			{
				return this.spawnTime;
			}
		}

		// Token: 0x17001083 RID: 4227
		// (get) Token: 0x06003EE7 RID: 16103 RVA: 0x00233F3C File Offset: 0x0023213C
		public string ErrorLine
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 5);
				defaultInterpolatedStringHandler.AppendLiteral("-   ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<Entity>(this);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				Submarine submarine = this.Submarine;
				string text;
				if (submarine == null)
				{
					text = null;
				}
				else
				{
					SubmarineInfo info = submarine.Info;
					text = ((info != null) ? info.Name : null);
				}
				defaultInterpolatedStringHandler.AppendFormatted(text ?? "[null]");
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				Submarine submarine2 = this.Submarine;
				defaultInterpolatedStringHandler.AppendFormatted<ushort>((submarine2 != null) ? submarine2.ID : 0);
				defaultInterpolatedStringHandler.AppendLiteral(") ");
				defaultInterpolatedStringHandler.AppendFormatted(this.CreationStackTrace);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x17001084 RID: 4228
		// (get) Token: 0x06003EE8 RID: 16104 RVA: 0x00233FFF File Offset: 0x002321FF
		public virtual ContentPackage ContentPackage
		{
			get
			{
				return GameMain.VanillaContent;
			}
		}

		// Token: 0x06003EE9 RID: 16105 RVA: 0x00234008 File Offset: 0x00232208
		public Entity(Submarine submarine, ushort id)
		{
			this.Submarine = submarine;
			this.spawnTime = Timing.TotalTime;
			if (Entity.dictionary.Count >= 65531)
			{
				Dictionary<Identifier, int> entityCounts = new Dictionary<Identifier, int>();
				foreach (KeyValuePair<ushort, Entity> entity in Entity.dictionary)
				{
					MapEntity me = entity.Value as MapEntity;
					if (me != null)
					{
						if (entityCounts.ContainsKey(me.Prefab.Identifier))
						{
							Dictionary<Identifier, int> dictionary = entityCounts;
							Identifier identifier = me.Prefab.Identifier;
							int num = dictionary[identifier];
							dictionary[identifier] = num + 1;
						}
						else
						{
							entityCounts[me.Prefab.Identifier] = 1;
						}
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Maximum amount of entities (");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(65531);
				defaultInterpolatedStringHandler.AppendLiteral(") exceeded! Largest numbers of entities: ");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear() + string.Join(", ", (from kvp in entityCounts
				orderby kvp.Value descending
				select kvp).Take(10).Select(delegate(KeyValuePair<Identifier, int> kvp)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(2, 2);
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(kvp.Key);
					defaultInterpolatedStringHandler3.AppendLiteral(": ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(kvp.Value);
					return defaultInterpolatedStringHandler3.ToStringAndClear();
				}));
				throw new Exception(errorMsg);
			}
			this.ID = this.DetermineID(id, submarine);
			if (Entity.dictionary.ContainsKey(this.ID))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("ID ");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler2.AppendLiteral(" is taken by ");
				defaultInterpolatedStringHandler2.AppendFormatted<Entity>(Entity.dictionary[this.ID]);
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			Entity.dictionary.Add(this.ID, this);
			this.CreationStackTrace = "";
			object obj = Entity.creationCounterMutex;
			lock (obj)
			{
				this.CreationIndex = Entity.creationCounter;
				Entity.creationCounter += 1UL;
			}
		}

		// Token: 0x06003EEA RID: 16106 RVA: 0x00234254 File Offset: 0x00232454
		protected virtual ushort DetermineID(ushort id, Submarine submarine)
		{
			if (id == 0)
			{
				return Entity.FindFreeId((submarine == null) ? 1 : submarine.IdOffset);
			}
			return id;
		}

		// Token: 0x06003EEB RID: 16107 RVA: 0x0023426C File Offset: 0x0023246C
		private static ushort FindFreeId(ushort idOffset)
		{
			if (Entity.dictionary.Count >= 65531)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Maximum amount of entities (");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(65531);
				defaultInterpolatedStringHandler.AppendLiteral(") reached!");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			ushort id = idOffset;
			while (id < 65532 && Entity.dictionary.ContainsKey(id))
			{
				id += 1;
			}
			return id;
		}

		// Token: 0x06003EEC RID: 16108 RVA: 0x002342E4 File Offset: 0x002324E4
		public static int FindFreeIdBlock(int minBlockSize)
		{
			int currentBlockSize = 0;
			for (int i = 1; i < 65532; i++)
			{
				if (Entity.dictionary.ContainsKey((ushort)i))
				{
					currentBlockSize = 0;
				}
				else
				{
					currentBlockSize++;
					if (currentBlockSize >= minBlockSize)
					{
						return i - (currentBlockSize - 1);
					}
				}
			}
			return 0;
		}

		// Token: 0x06003EED RID: 16109 RVA: 0x00234324 File Offset: 0x00232524
		public static Entity FindEntityByID(ushort ID)
		{
			Entity matchingEntity;
			Entity.dictionary.TryGetValue(ID, out matchingEntity);
			return matchingEntity;
		}

		// Token: 0x06003EEE RID: 16110 RVA: 0x00234340 File Offset: 0x00232540
		public static void RemoveAll()
		{
			List<Entity> list = new List<Entity>(Entity.dictionary.Values);
			foreach (Entity e in list)
			{
				try
				{
					e.Remove();
				}
				catch (Exception exception)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Error while removing entity \"");
					defaultInterpolatedStringHandler.AppendFormatted<Entity>(e);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), exception, null, false, false);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(26, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Entity.RemoveAll:Exception");
					defaultInterpolatedStringHandler2.AppendFormatted<Entity>(e);
					string identifier = defaultInterpolatedStringHandler2.ToStringAndClear();
					GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(33, 3);
					defaultInterpolatedStringHandler3.AppendLiteral("Error while removing entity \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Entity>(e);
					defaultInterpolatedStringHandler3.AppendLiteral(" (");
					defaultInterpolatedStringHandler3.AppendFormatted(exception.Message);
					defaultInterpolatedStringHandler3.AppendLiteral(")\n");
					defaultInterpolatedStringHandler3.AppendFormatted(exception.StackTrace.CleanupStackTrace());
					GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, defaultInterpolatedStringHandler3.ToStringAndClear());
				}
			}
			StringBuilder errorMsg = new StringBuilder();
			if (Entity.dictionary.Count > 0)
			{
				errorMsg.AppendLine("Some entities were not removed in Entity.RemoveAll:");
				foreach (Entity e2 in Entity.dictionary.Values)
				{
					errorMsg.AppendLine(string.Concat(new string[]
					{
						" - ",
						e2.ToString(),
						"(ID ",
						e2.ID.ToString(),
						")"
					}));
				}
			}
			if (Item.ItemList.Count > 0)
			{
				errorMsg.AppendLine("Some items were not removed in Entity.RemoveAll:");
				foreach (Item item in Item.ItemList)
				{
					errorMsg.AppendLine(string.Concat(new string[]
					{
						" - ",
						item.Name,
						"(ID ",
						item.ID.ToString(),
						")"
					}));
				}
				List<Item> items = new List<Item>(Item.ItemList);
				foreach (Item item2 in items)
				{
					try
					{
						item2.Remove();
					}
					catch (Exception exception2)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(28, 1);
						defaultInterpolatedStringHandler4.AppendLiteral("Error while removing item \"");
						defaultInterpolatedStringHandler4.AppendFormatted<Item>(item2);
						defaultInterpolatedStringHandler4.AppendLiteral("\"");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), exception2, null, false, false);
					}
				}
				Item.ItemList.Clear();
			}
			if (Character.CharacterList.Count > 0)
			{
				errorMsg.AppendLine("Some characters were not removed in Entity.RemoveAll:");
				foreach (Character character in Character.CharacterList)
				{
					errorMsg.AppendLine(string.Concat(new string[]
					{
						" - ",
						character.Name,
						"(ID ",
						character.ID.ToString(),
						")"
					}));
				}
				List<Character> characters = new List<Character>(Character.CharacterList);
				foreach (Character character2 in characters)
				{
					try
					{
						character2.Remove();
					}
					catch (Exception exception3)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(33, 1);
						defaultInterpolatedStringHandler5.AppendLiteral("Error while removing character \"");
						defaultInterpolatedStringHandler5.AppendFormatted<Character>(character2);
						defaultInterpolatedStringHandler5.AppendLiteral("\"");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), exception3, null, false, false);
					}
				}
				Character.CharacterList.Clear();
			}
			if (!string.IsNullOrEmpty(errorMsg.ToString()))
			{
				foreach (string errorLine in errorMsg.ToString().Split('\n', StringSplitOptions.None))
				{
					DebugConsole.ThrowError(errorLine, null, null, false, false);
				}
				GameAnalyticsManager.AddErrorEventOnce("Entity.RemoveAll", GameAnalyticsManager.ErrorSeverity.Error, errorMsg.ToString());
			}
			Entity.dictionary.Clear();
			Hull.EntityGrids.Clear();
			EntitySpawner spawner = Entity.Spawner;
			if (spawner != null)
			{
				spawner.Reset();
			}
			Projectile.ResetSpreadCounter();
		}

		// Token: 0x06003EEF RID: 16111 RVA: 0x0023481C File Offset: 0x00232A1C
		public void FreeID()
		{
			if (this.IdFreed)
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Removing entity ");
			defaultInterpolatedStringHandler.AppendFormatted(this.ToString());
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
			defaultInterpolatedStringHandler.AppendLiteral(") from entity dictionary.");
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			Entity existingEntity;
			if (!Entity.dictionary.TryGetValue(this.ID, out existingEntity))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(44, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Entity ");
				defaultInterpolatedStringHandler2.AppendFormatted(this.ToString());
				defaultInterpolatedStringHandler2.AppendLiteral(" (");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler2.AppendLiteral(") not present in entity dictionary.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(28, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Entity.FreeID:EntityNotFound");
				defaultInterpolatedStringHandler3.AppendFormatted<ushort>(this.ID);
				string identifier = defaultInterpolatedStringHandler3.ToStringAndClear();
				GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(45, 3);
				defaultInterpolatedStringHandler4.AppendLiteral("Entity ");
				defaultInterpolatedStringHandler4.AppendFormatted(this.ToString());
				defaultInterpolatedStringHandler4.AppendLiteral(" (");
				defaultInterpolatedStringHandler4.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler4.AppendLiteral(") not present in entity dictionary.\n");
				defaultInterpolatedStringHandler4.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, defaultInterpolatedStringHandler4.ToStringAndClear());
			}
			else if (existingEntity != this)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(73, 3);
				defaultInterpolatedStringHandler5.AppendLiteral("Entity ID mismatch in entity dictionary. Entity ");
				defaultInterpolatedStringHandler5.AppendFormatted<Entity>(existingEntity);
				defaultInterpolatedStringHandler5.AppendLiteral(" had the ID ");
				defaultInterpolatedStringHandler5.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler5.AppendLiteral(" (expecting ");
				defaultInterpolatedStringHandler5.AppendFormatted(this.ToString());
				defaultInterpolatedStringHandler5.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), null, null, false, false);
				string identifier2 = "Entity.FreeID:EntityMismatch" + this.ID.ToString();
				GameAnalyticsManager.ErrorSeverity errorSeverity2 = GameAnalyticsManager.ErrorSeverity.Error;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(73, 3);
				defaultInterpolatedStringHandler6.AppendLiteral("Entity ID mismatch in entity dictionary. Entity ");
				defaultInterpolatedStringHandler6.AppendFormatted<Entity>(existingEntity);
				defaultInterpolatedStringHandler6.AppendLiteral(" had the ID ");
				defaultInterpolatedStringHandler6.AppendFormatted<ushort>(this.ID);
				defaultInterpolatedStringHandler6.AppendLiteral(" (expecting ");
				defaultInterpolatedStringHandler6.AppendFormatted(this.ToString());
				defaultInterpolatedStringHandler6.AppendLiteral(")");
				GameAnalyticsManager.AddErrorEventOnce(identifier2, errorSeverity2, defaultInterpolatedStringHandler6.ToStringAndClear());
			}
			else
			{
				Entity.dictionary.Remove(this.ID);
			}
			this.IdFreed = true;
		}

		// Token: 0x06003EF0 RID: 16112 RVA: 0x00234A95 File Offset: 0x00232C95
		public virtual void Remove()
		{
			this.FreeID();
			this.Removed = true;
		}

		// Token: 0x06003EF1 RID: 16113 RVA: 0x00234AA4 File Offset: 0x00232CA4
		public static void DumpIds(int count, string filename)
		{
			List<Entity> entities = (from e in Entity.dictionary.Values
			orderby e.ID descending
			select e).ToList<Entity>();
			count = Math.Min(entities.Count, count);
			List<string> lines = new List<string>();
			for (int i = 0; i < count; i++)
			{
				List<string> list = lines;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(entities[i].ID);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<Entity>(entities[i]);
				list.Add(defaultInterpolatedStringHandler.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(entities[i].ID);
				defaultInterpolatedStringHandler2.AppendLiteral(": ");
				defaultInterpolatedStringHandler2.AppendFormatted<Entity>(entities[i]);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
			}
			if (!string.IsNullOrWhiteSpace(filename))
			{
				File.WriteAllLines(filename, lines, null, true);
			}
		}

		// Token: 0x0400209F RID: 8351
		public const ushort NullEntityID = 0;

		// Token: 0x040020A0 RID: 8352
		public const ushort EntitySpawnerID = 65535;

		// Token: 0x040020A1 RID: 8353
		public const ushort RespawnManagerID = 65534;

		// Token: 0x040020A2 RID: 8354
		public const ushort DummyID = 65533;

		// Token: 0x040020A3 RID: 8355
		public const ushort ReservedIDStart = 65532;

		// Token: 0x040020A4 RID: 8356
		public const ushort MaxEntityCount = 65531;

		// Token: 0x040020A5 RID: 8357
		private static readonly Dictionary<ushort, Entity> dictionary = new Dictionary<ushort, Entity>();

		// Token: 0x040020A6 RID: 8358
		public static EntitySpawner Spawner;

		// Token: 0x040020A7 RID: 8359
		protected AITarget aiTarget;

		// Token: 0x040020AA RID: 8362
		public readonly ushort ID;

		// Token: 0x040020AC RID: 8364
		private readonly double spawnTime;

		// Token: 0x040020AD RID: 8365
		private static ulong creationCounter = 0UL;

		// Token: 0x040020AE RID: 8366
		private static readonly object creationCounterMutex = new object();

		// Token: 0x040020AF RID: 8367
		public readonly string CreationStackTrace;

		// Token: 0x040020B0 RID: 8368
		public readonly ulong CreationIndex;
	}
}
