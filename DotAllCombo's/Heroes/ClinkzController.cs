﻿namespace DotaAllCombo.Heroes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ensage;
	using Ensage.Common;
	using Ensage.Common.Extensions;
	using Ensage.Common.Menu;
	using SharpDX;
	using Service;
	using Service.Debug;


	internal class ClinkzController : Variables, IHeroController
	{
		private Ability Q, W, R;

		private Item urn, orchid, sheep, ethereal, dagon, halberd, mjollnir,
					 abyssal, mom, Shiva, mail, bkb, blink, satanic, medall;
		public void Combo()
		{
			Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);

			Q = me.Spellbook.SpellQ;
			W = me.Spellbook.SpellW;
			R = me.Spellbook.SpellR;


			mom = me.FindItem("item_mask_of_madness");
			urn = me.FindItem("item_urn_of_shadows");
			dagon = me.Inventory.Items.FirstOrDefault(x => x.Name.Contains("item_dagon"));
			ethereal = me.FindItem("item_ethereal_blade");
			halberd = me.FindItem("item_heavens_halberd");
			mjollnir = me.FindItem("item_mjollnir");
			orchid = me.FindItem("item_orchid") ?? me.FindItem("item_bloodthorn");
			abyssal = me.FindItem("item_abyssal_blade");
			mail = me.FindItem("item_blade_mail");
			bkb = me.FindItem("item_black_king_bar");
			satanic = me.FindItem("item_satanic");
			blink = me.FindItem("item_blink");
			medall = me.FindItem("item_medallion_of_courage") ?? me.FindItem("item_solar_crest");
			Shiva = me.FindItem("item_shivas_guard");
			var v =
				ObjectManager.GetEntities<Hero>()
					.Where(x => x.Team != me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune())
					.ToList();


			var modifInv = me.IsInvisible();

			if (Active)
            {
                e = Toolset.ClosestToMouse(me);
                if (e == null)
					return;
				
				if ((W!=null && W.CanBeCasted() && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)))
				{
					if (Menu.Item("orbwalk").GetValue<bool>() && me.Distance2D(e) <= 1900)
					{
						Orbwalking.Orbwalk(e, 0, 1600, true, true);
					}
				}
				sheep = e.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter ? null : me.FindItem("item_sheepstick");

				Console.WriteLine("3");
				if (me.Distance2D(e) <= 1400 && e != null && e.IsAlive && !modifInv)
				{
					if ((!W.CanBeCasted() || !Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)))
					{
						if (Menu.Item("orbwalk").GetValue<bool>() && me.Distance2D(e) <= 1900)
						{
							Orbwalking.Orbwalk(e, 0, 1600, false, true);
						}
					}
					Console.WriteLine("4");
					var stoneModif = e.HasModifier("modifier_medusa_stone_gaze_stone");
					if (stoneModif) return;
					float angle = me.FindAngleBetween(e.Position, true);
					Vector3 pos = new Vector3((float)(e.Position.X - 500 * Math.Cos(angle)), (float)(e.Position.Y - 500 * Math.Sin(angle)), 0);
					if (
						blink != null
						&& Q.CanBeCasted()
						&& me.CanCast()
						&& blink.CanBeCasted()
						&& me.Distance2D(e) >= 490
						&& me.Distance2D(pos) <= 1180
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(blink.Name)
						&& Utils.SleepCheck("blink")
						)
					{
						blink.UseAbility(pos);
						Utils.Sleep(250, "blink");
					}
					if ( // sheep
						sheep != null
						&& sheep.CanBeCasted()
						&& me.CanCast()
						&& !e.IsLinkensProtected()
						&& !e.IsMagicImmune()
						&& me.Distance2D(e) <= 1400
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(sheep.Name)
						&& Utils.SleepCheck("sheep")
						)
					{
						sheep.UseAbility(e);
						Utils.Sleep(250, "sheep");
					} // sheep Item end
					if (
						Q != null && Q.CanBeCasted() && me.Distance2D(e) <= me.AttackRange + 50
						&& me.CanAttack()
						&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
						&& Utils.SleepCheck("Q")
						)
					{
						Q.UseAbility();
						Utils.Sleep(150, "Q");
					}
					if (W != null && W.CanBeCasted()
					    && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name))
					{
						if (Menu.Item("orbwalk").GetValue<bool>() && me.Distance2D(e) <= 1900)
						{
							Orbwalking.Orbwalk(e, 0, 1600, true, true);
						}
					}
					if ( // MOM
						mom != null
						&& mom.CanBeCasted()
						&& me.CanCast()
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mom.Name)
						&& Utils.SleepCheck("mom")
						&& me.Distance2D(e) <= 700
						)
					{
						mom.UseAbility();
						Utils.Sleep(250, "mom");
					}
					if ( // Mjollnir
						mjollnir != null
						&& mjollnir.CanBeCasted()
						&& me.CanCast()
						&& !e.IsMagicImmune()
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mjollnir.Name)
						&& Utils.SleepCheck("mjollnir")
						&& me.Distance2D(e) <= 900
						)
					{
						mjollnir.UseAbility(me);
						Utils.Sleep(250, "mjollnir");
					} // Mjollnir Item end
					if ( // Medall
						medall != null
						&& medall.CanBeCasted()
						&& Utils.SleepCheck("Medall")
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(medall.Name)
						&& me.Distance2D(e) <= 700
						)
					{
						medall.UseAbility(e);
						Utils.Sleep(250, "Medall");
					} // Medall Item end
					if ( // orchid
						orchid != null
						&& orchid.CanBeCasted()
						&& me.CanCast()
						&& !e.IsLinkensProtected()
						&& !e.IsMagicImmune()
						&& me.Distance2D(e) <= me.AttackRange + 40
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(orchid.Name)
						&& Utils.SleepCheck("orchid")
						)
					{
						orchid.UseAbility(e);
						Utils.Sleep(250, "orchid");
					} // orchid Item end

					if (Shiva != null && Shiva.CanBeCasted() && me.Distance2D(e) <= 600
					    && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(Shiva.Name)
					    && !e.IsMagicImmune() && Utils.SleepCheck("Shiva"))
					{
						Shiva.UseAbility();
						Utils.Sleep(100, "Shiva");
					}

					if (ethereal != null && ethereal.CanBeCasted()
					    && me.Distance2D(e) <= 700 && me.Distance2D(e) <= 400
					    && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ethereal.Name) &&
					    Utils.SleepCheck("ethereal"))
					{
						ethereal.UseAbility(e);
						Utils.Sleep(100, "ethereal");
					}

					if (dagon != null
					    && dagon.CanBeCasted()
					    && me.Distance2D(e) <= 500
					    && Utils.SleepCheck("dagon"))
					{
						dagon.UseAbility(e);
						Utils.Sleep(100, "dagon");
					}
					if ( // Abyssal Blade
						abyssal != null
						&& abyssal.CanBeCasted()
						&& me.CanCast()
						&& !e.IsStunned()
						&& !e.IsHexed()
						&& Utils.SleepCheck("abyssal")
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(abyssal.Name)
						&& me.Distance2D(e) <= 400
						)
					{
						abyssal.UseAbility(e);
						Utils.Sleep(250, "abyssal");
					} // Abyssal Item end
					if (urn != null && urn.CanBeCasted() && urn.CurrentCharges > 0 && me.Distance2D(e) <= 400
					    && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(urn.Name) && Utils.SleepCheck("urn"))
					{
						urn.UseAbility(e);
						Utils.Sleep(240, "urn");
					}
					if ( // Hellbard
						halberd != null
						&& halberd.CanBeCasted()
						&& me.CanCast()
						&& !e.IsMagicImmune()
						&& (e.NetworkActivity == NetworkActivity.Attack
						    || e.NetworkActivity == NetworkActivity.Crit
						    || e.NetworkActivity == NetworkActivity.Attack2)
						&& Utils.SleepCheck("halberd")
						&& me.Distance2D(e) <= 700
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(halberd.Name)
						)
					{
						halberd.UseAbility(e);
						Utils.Sleep(250, "halberd");
					}
					if ( // Satanic 
						satanic != null &&
						me.Health <= (me.MaximumHealth*0.3) &&
						satanic.CanBeCasted() &&
						me.Distance2D(e) <= me.AttackRange + 50
						&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(satanic.Name)
						&& Utils.SleepCheck("satanic")
						)
					{
						satanic.UseAbility();
						Utils.Sleep(240, "satanic");
					} // Satanic Item end
					if (mail != null && mail.CanBeCasted() && (v.Count(x => x.Distance2D(me) <= 650) >=
					                                           (Menu.Item("Heelm").GetValue<Slider>().Value)) &&
					    Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mail.Name) && Utils.SleepCheck("mail"))
					{
						mail.UseAbility();
						Utils.Sleep(100, "mail");
					}
					if (bkb != null && bkb.CanBeCasted() && (v.Count(x => x.Distance2D(me) <= 650) >=
					                                         (Menu.Item("Heel").GetValue<Slider>().Value)) &&
					    Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(bkb.Name) && Utils.SleepCheck("bkb"))
					{
						bkb.UseAbility();
						Utils.Sleep(100, "bkb");
					}
				}
			}
			if (Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name))
			{
				var Units = ObjectManager.GetEntities<Unit>().Where(creep =>
				(creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Neutral
				|| creep.ClassID == ClassID.CDOTA_BaseNPC_Invoker_Forged_Spirit
				|| creep.ClassID == ClassID.CDOTA_BaseNPC_Creep
				|| creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane
				|| creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege
				|| creep.ClassID == ClassID.CDOTA_Unit_Hero_Beastmaster_Boar
				)
				&& creep.Health >= (creep.MaximumHealth * 0.7)
				&& creep.IsAlive
				&& creep.Distance2D(me) <= R.GetCastRange() + 300
				&& creep.IsSpawned
				&& creep.Team != me.Team).ToList();

				if (R != null && R.CanBeCasted()
					&& Utils.SleepCheck("R")
					&& !me.HasModifier("modifier_clinkz_death_pact")
					&& !me.IsInvisible())
				{
					if (Units.Count > 0)
					{
						R.UseAbility(Units.OrderBy(x => x.Health).LastOrDefault());
					}
					Utils.Sleep(1000, "R");
				}
			}
		}

		public void OnLoadEvent()
		{
			AssemblyExtensions.InitAssembly("VickTheRock", "0.1b");

			Print.LogMessage.Success("Go Rampage!");

			Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
			Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
			Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

		    Menu.AddItem(
				new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"clinkz_death_pact", true},
				    {"clinkz_searing_arrows", true},
				    {"clinkz_strafe", true}
				})));
			Menu.AddItem(
				new MenuItem("Items", ":").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"item_mask_of_madness", true},
				    {"item_heavens_halberd", true},
					{"item_sheepstick", true},
					{"item_orchid", true},
                    {"item_bloodthorn", true},
				    {"item_mjollnir", true},
				    {"item_urn_of_shadows", true},
				    {"item_ethereal_blade", true},
				    {"item_abyssal_blade", true},
				    {"item_shivas_guard", true},
				    {"item_blade_mail", true},
					{"item_blink", true},
					{"item_black_king_bar", true},
				    {"item_satanic", true},
				    {"item_medallion_of_courage", true},
				    {"item_solar_crest", true}
				})));
			Menu.AddItem(new MenuItem("Heel", "Min targets to BKB").SetValue(new Slider(2, 1, 5)));
			Menu.AddItem(new MenuItem("Heelm", "Min targets to BladeMail").SetValue(new Slider(2, 1, 5)));
		}

		public void OnCloseEvent()
		{
			
		}
	}
}