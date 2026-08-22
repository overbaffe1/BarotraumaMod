using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000120 RID: 288
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class TestScreen : EditorScreen
	{
		// Token: 0x17000A4B RID: 2635
		// (get) Token: 0x06002795 RID: 10133 RVA: 0x001B64A4 File Offset: 0x001B46A4
		public override Camera Cam { get; }

		// Token: 0x06002796 RID: 10134 RVA: 0x001B64AC File Offset: 0x001B46AC
		public TestScreen()
		{
			this.Cam = new Camera();
			TestScreen.BlueprintEffect = GameMain.GameScreen.BlueprintEffect;
			new GUIButton(new RectTransform(new Point(256, 256), this.Frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "Reload shader", Alignment.Center, "", null).OnClicked = delegate(GUIButton button, object o)
			{
				TestScreen.BlueprintEffect.Dispose();
				GameMain.Instance.Content.Unload();
				TestScreen.BlueprintEffect = EffectLoader.Load("Effects/blueprintshader");
				GameMain.GameScreen.BlueprintEffect = TestScreen.BlueprintEffect;
				return true;
			};
		}

		// Token: 0x06002797 RID: 10135 RVA: 0x001B6548 File Offset: 0x001B4748
		public override void Select()
		{
			base.Select();
			Character character = TestScreen.dummyCharacter;
			if (character != null && !character.Removed)
			{
				TestScreen.dummyCharacter.Remove();
			}
			TestScreen.dummyCharacter = Character.Create(CharacterPrefab.HumanSpeciesName, Vector2.Zero, "", null, 65533, false, false, true, null, true, true);
			TestScreen.dummyCharacter.Info.Job = new Job(JobPrefab.Prefabs.FirstOrDefault((JobPrefab jp) => jp.Identifier == "captain"), false);
			TestScreen.dummyCharacter.Info.Name = "Galldren";
			TestScreen.dummyCharacter.Inventory.CreateSlots();
			TestScreen.dummyCharacter.Info.GiveExperience(999999);
			this.miniMapItem = new Item(ItemPrefab.Find(null, "circuitbox".ToIdentifier()), Vector2.Zero, null, 1337, false);
			this.miniMapItem.GetComponent<Holdable>().AttachToWall();
			Character.Controlled = TestScreen.dummyCharacter;
			GameMain.World.ProcessChanges();
			TestScreen.dummyCharacter.Info.TalentRefundPoints = 2;
			this.TabMenu = new TabMenu();
		}

		// Token: 0x06002798 RID: 10136 RVA: 0x001B6678 File Offset: 0x001B4878
		public override void AddToGUIUpdateList()
		{
			this.Frame.AddToGUIUpdateList(false, 0);
			CharacterHUD.AddToGUIUpdateList(TestScreen.dummyCharacter);
			Character character = TestScreen.dummyCharacter;
			if (character != null)
			{
				Item selectedItem = character.SelectedItem;
				if (selectedItem != null)
				{
					selectedItem.AddToGUIUpdateList(0);
				}
			}
			TabMenu tabMenu = this.TabMenu;
			if (tabMenu == null)
			{
				return;
			}
			tabMenu.AddToGUIUpdateList();
		}

		// Token: 0x06002799 RID: 10137 RVA: 0x001B66C8 File Offset: 0x001B48C8
		public override void Update(double deltaTime)
		{
			base.Update(deltaTime);
			TabMenu tabMenu = this.TabMenu;
			if (tabMenu != null)
			{
				tabMenu.Update((float)deltaTime);
			}
			Character dummy = TestScreen.dummyCharacter;
			if (dummy != null)
			{
				Item item = this.miniMapItem;
				if (item != null)
				{
					if (dummy.SelectedItem != item)
					{
						dummy.SelectedItem = item;
					}
					Item selectedItem = dummy.SelectedItem;
					if (selectedItem != null)
					{
						selectedItem.UpdateHUD(this.Cam, dummy, (float)deltaTime);
					}
					item.SendSignal("1", "signal_in1");
					Vector2 pos = ConvertUnits.ToSimUnits(item.Position);
					foreach (Limb limb in dummy.AnimController.Limbs)
					{
						limb.body.SetTransform(pos, 0f, true);
					}
					AnimController animController = dummy.AnimController;
					PhysicsBody collider = (animController != null) ? animController.Collider : null;
					if (collider != null)
					{
						collider.SetTransform(pos, 0f, true);
					}
					dummy.ControlLocalPlayer((float)deltaTime, this.Cam, false);
					dummy.Control((float)deltaTime, this.Cam);
				}
			}
		}

		// Token: 0x0600279A RID: 10138 RVA: 0x001B67CC File Offset: 0x001B49CC
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			base.Draw(deltaTime, graphics, spriteBatch);
			graphics.Clear(EditorScreen.BackgroundColor);
			spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, new Matrix?(this.Cam.Transform));
			Item item = this.miniMapItem;
			if (item != null)
			{
				item.Draw(spriteBatch, false, true, null, null);
			}
			Character dummy = TestScreen.dummyCharacter;
			if (dummy != null)
			{
				TestScreen.dummyCharacter.DrawFront(spriteBatch, this.Cam);
				TestScreen.dummyCharacter.Draw(spriteBatch, this.Cam);
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, null, null, null);
			GUI.Draw(this.Cam, spriteBatch);
			Character character = TestScreen.dummyCharacter;
			if (character != null)
			{
				character.DrawHUD(spriteBatch, this.Cam, false);
			}
			spriteBatch.End();
		}

		// Token: 0x0400142E RID: 5166
		[Nullable(2)]
		private Item miniMapItem;

		// Token: 0x0400142F RID: 5167
		[Nullable(2)]
		public static Character dummyCharacter;

		// Token: 0x04001430 RID: 5168
		[Nullable(2)]
		public static Effect BlueprintEffect;

		// Token: 0x04001431 RID: 5169
		[Nullable(2)]
		public TabMenu TabMenu;
	}
}
