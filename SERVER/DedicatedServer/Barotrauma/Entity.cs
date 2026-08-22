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
	// Token: 0x0200022B RID: 555
	internal abstract class Entity : ISpatialEntity
	{
		// Token: 0x060025F0 RID: 9712 RVA: 0x000F649F File Offset: 0x000F469F
		public static IReadOnlyCollection<Entity> GetEntities()
		{
			return Entity.dictionary.Values;
		}

		// Token: 0x17000AE0 RID: 2784
		// (get) Token: 0x060025F1 RID: 9713 RVA: 0x000F64AB File Offset: 0x000F46AB
		public static int EntityCount
		{
			get
			{
				return Entity.dictionary.Count;
			}
		}

		// Token: 0x17000AE1 RID: 2785
		// (get) Token: 0x060025F2 RID: 9714 RVA: 0x000F64B7 File Offset: 0x000F46B7
		// (set) Token: 0x060025F3 RID: 9715 RVA: 0x000F64BF File Offset: 0x000F46BF
		public bool Removed { get; private set; }

		// Token: 0x17000AE2 RID: 2786
		// (get) Token: 0x060025F4 RID: 9716 RVA: 0x000F64C8 File Offset: 0x000F46C8
		// (set) Token: 0x060025F5 RID: 9717 RVA: 0x000F64D0 File Offset: 0x000F46D0
		public bool IdFreed { get; private set; }

		// Token: 0x17000AE3 RID: 2787
		// (get) Token: 0x060025F6 RID: 9718 RVA: 0x000F64D9 File Offset: 0x000F46D9
		public virtual Vector2 SimPosition
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x17000AE4 RID: 2788
		// (get) Token: 0x060025F7 RID: 9719 RVA: 0x000F64E0 File Offset: 0x000F46E0
		public virtual Vector2 Position
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x17000AE5 RID: 2789
		// (get) Token: 0x060025F8 RID: 9720 RVA: 0x000F64E7 File Offset: 0x000F46E7
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

		// Token: 0x17000AE6 RID: 2790
		// (get) Token: 0x060025F9 RID: 9721 RVA: 0x000F650E File Offset: 0x000F470E
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

		// Token: 0x17000AE7 RID: 2791
		// (get) Token: 0x060025FA RID: 9722 RVA: 0x000F6535 File Offset: 0x000F4735
		// (set) Token: 0x060025FB RID: 9723 RVA: 0x000F653D File Offset: 0x000F473D
		public Submarine Submarine { get; set; }

		// Token: 0x17000AE8 RID: 2792
		// (get) Token: 0x060025FC RID: 9724 RVA: 0x000F6546 File Offset: 0x000F4746
		public AITarget AiTarget
		{
			get
			{
				return this.aiTarget;
			}
		}

		// Token: 0x17000AE9 RID: 2793
		// (get) Token: 0x060025FD RID: 9725 RVA: 0x000F654E File Offset: 0x000F474E
		// (set) Token: 0x060025FE RID: 9726 RVA: 0x000F6565 File Offset: 0x000F4765
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

		// Token: 0x17000AEA RID: 2794
		// (get) Token: 0x060025FF RID: 9727 RVA: 0x000F657B File Offset: 0x000F477B
		public double SpawnTime
		{
			get
			{
				return this.spawnTime;
			}
		}

		// Token: 0x17000AEB RID: 2795
		// (get) Token: 0x06002600 RID: 9728 RVA: 0x000F6584 File Offset: 0x000F4784
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

		// Token: 0x17000AEC RID: 2796
		// (get) Token: 0x06002601 RID: 9729 RVA: 0x000F6647 File Offset: 0x000F4847
		public virtual ContentPackage ContentPackage
		{
			get
			{
				return GameMain.VanillaContent;
			}
		}

		// Token: 0x06002602 RID: 9730 RVA: 0x000F6650 File Offset: 0x000F4850
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

		// Token: 0x06002603 RID: 9731 RVA: 0x000F689C File Offset: 0x000F4A9C
		protected virtual ushort DetermineID(ushort id, Submarine submarine)
		{
			if (id == 0)
			{
				return Entity.FindFreeId((submarine == null) ? 1 : submarine.IdOffset);
			}
			return id;
		}

		// Token: 0x06002604 RID: 9732 RVA: 0x000F68B4 File Offset: 0x000F4AB4
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

		// Token: 0x06002605 RID: 9733 RVA: 0x000F692C File Offset: 0x000F4B2C
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

		// Token: 0x06002606 RID: 9734 RVA: 0x000F696C File Offset: 0x000F4B6C
		public static Entity FindEntityByID(ushort ID)
		{
			Entity matchingEntity;
			Entity.dictionary.TryGetValue(ID, out matchingEntity);
			return matchingEntity;
		}

		// Token: 0x06002607 RID: 9735 RVA: 0x000F6988 File Offset: 0x000F4B88
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

		// Token: 0x06002608 RID: 9736 RVA: 0x000F6E64 File Offset: 0x000F5064
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

		// Token: 0x06002609 RID: 9737 RVA: 0x000F70DD File Offset: 0x000F52DD
		public virtual void Remove()
		{
			this.FreeID();
			this.Removed = true;
		}

		// Token: 0x0600260A RID: 9738 RVA: 0x000F70EC File Offset: 0x000F52EC
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

		// Token: 0x04001273 RID: 4723
		public const ushort NullEntityID = 0;

		// Token: 0x04001274 RID: 4724
		public const ushort EntitySpawnerID = 65535;

		// Token: 0x04001275 RID: 4725
		public const ushort RespawnManagerID = 65534;

		// Token: 0x04001276 RID: 4726
		public const ushort DummyID = 65533;

		// Token: 0x04001277 RID: 4727
		public const ushort ReservedIDStart = 65532;

		// Token: 0x04001278 RID: 4728
		public const ushort MaxEntityCount = 65531;

		// Token: 0x04001279 RID: 4729
		private static readonly Dictionary<ushort, Entity> dictionary = new Dictionary<ushort, Entity>();

		// Token: 0x0400127A RID: 4730
		public static EntitySpawner Spawner;

		// Token: 0x0400127B RID: 4731
		protected AITarget aiTarget;

		// Token: 0x0400127E RID: 4734
		public readonly ushort ID;

		// Token: 0x04001280 RID: 4736
		private readonly double spawnTime;

		// Token: 0x04001281 RID: 4737
		private static ulong creationCounter = 0UL;

		// Token: 0x04001282 RID: 4738
		private static readonly object creationCounterMutex = new object();

		// Token: 0x04001283 RID: 4739
		public readonly string CreationStackTrace;

		// Token: 0x04001284 RID: 4740
		public readonly ulong CreationIndex;
	}
}
