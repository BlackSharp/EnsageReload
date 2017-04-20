﻿namespace DotaAllCombo.Heroes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;

	using Service;
	using Service.Debug;

    internal class UrsaController : Variables, IHeroController
    {
        private static Ability Q, W, R;

        private static Item urn, dagon, mjollnir, orchid, abyssal, mom, Shiva, mail, bkb, satanic, medall, blink, sheep;
		
		public void Combo()
		{
			if (!Menu.Item("enabled").IsActive())
				return;
			Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key) && !Game.IsChatOpen;

			Q = me.Spellbook.SpellQ;
			W = me.Spellbook.SpellW;
			R = me.FindSpell("ursa_enrage");
            Shiva = me.FindItem("item_shivas_guard");
			mom = me.FindItem("item_mask_of_madness");
			urn = me.FindItem("item_urn_of_shadows");
			dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
			mjollnir = me.FindItem("item_mjollnir");
			orchid = me.FindItem("item_orchid") ?? me.FindItem("item_bloodthorn");
			abyssal = me.FindItem("item_abyssal_blade");
			mail = me.FindItem("item_blade_mail");
			bkb = me.FindItem("item_black_king_bar");
			satanic = me.FindItem("item_satanic");
			blink = me.FindItem("item_blink");
			medall = me.FindItem("item_medallion_of_courage") ?? me.FindItem("item_solar_crest");
            e = Toolset.ClosestToMouse(me);
            if (e == null) return;

			sheep = e.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter ? null : me.FindItem("item_sheepstick");
			var stoneModif = e.HasModifier("modifier_medusa_stone_gaze_stone");


			var v =
				ObjectManager.GetEntities<Hero>()
					.Where(x => x.Team != me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune())
					.ToList();
			var modifInv =me.IsInvisible();

			if (Active && me.Distance2D(e) <= 1700 && e != null && e.IsAlive)
            {
				if (Menu.Item("orbwalk").GetValue<bool>() && me.Distance2D(e) <= 1900)
				{
					Orbwalking.Orbwalk(e, 0, 1600, true, true);
				}
				//var Wmod = me.Modifiers.FirstOrDefault(y => y.Name == "modifier_ursa_overpower");
				//if (Wmod.StackCount == 0)

			}
			if (Active && me.Distance2D(e) <= 1400 && e != null && e.IsAlive && !modifInv)
			{

				if (
					Q != null && Q.CanBeCasted() && me.Distance2D(e) <= 200
					&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
					&& Utils.SleepCheck("Q")
					)
				{
					Q.UseAbility();
					Utils.Sleep(200, "Q");
				}
				if (
					W != null && W.CanBeCasted() && me.Distance2D(e) <= 1700
					&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)
					&& !me.HasModifier("modifier_ursa_overpower")
					&& Utils.SleepCheck("W")
					)
				{
					W.UseAbility();
					Utils.Sleep(200, "W");
				}
				if (
					R != null && R.CanBeCasted() && me.Distance2D(e) <= 200
                    && me.HasModifier("modifier_ursa_overpower")
                    && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name)
					&& Utils.SleepCheck("R")
					)
				{
					R.UseAbility();
					Utils.Sleep(200, "R");
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
					   blink != null
					   && me.CanCast()
					   && blink.CanBeCasted()
					   && me.Distance2D(e) < 1180
					   && me.Distance2D(e) > 300
					   && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(blink.Name)
					   && Utils.SleepCheck("blink")
					   )
				{
					blink.UseAbility(e.Position);
					Utils.Sleep(250, "blink");
				}

				if ( // Abyssal Blade
					abyssal != null
					&& abyssal.CanBeCasted()
					&& me.CanCast()
					&& !e.IsStunned()
					&& !e.IsHexed()
					&& Utils.SleepCheck("abyssal")
					&& me.Distance2D(e) <= 400
					)
				{
					abyssal.UseAbility(e);
					Utils.Sleep(250, "abyssal");
				} // Abyssal Item end
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
				if (orchid != null && orchid.CanBeCasted() && me.Distance2D(e) <= 900 &&
					Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(orchid.Name) && Utils.SleepCheck("orchid"))
				{
					orchid.UseAbility(e);
					Utils.Sleep(100, "orchid");
				}

				if (Shiva != null && Shiva.CanBeCasted() && me.Distance2D(e) <= 600
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(Shiva.Name)
					&& !e.IsMagicImmune() && Utils.SleepCheck("Shiva"))
				{
					Shiva.UseAbility();
					Utils.Sleep(100, "Shiva");
				}

				if ( // Dagon
					me.CanCast()
					&& dagon != null
					&& !e.IsLinkensProtected()
					&& dagon.CanBeCasted()
					&& !e.IsMagicImmune()
					&& !stoneModif
					&& Utils.SleepCheck("dagon")
					)
				{
					dagon.UseAbility(e);
					Utils.Sleep(200, "dagon");
				} // Dagon Item end


				if (urn != null && urn.CanBeCasted() && urn.CurrentCharges > 0 && me.Distance2D(e) <= 400
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(urn.Name) && Utils.SleepCheck("urn"))
				{
					urn.UseAbility(e);
					Utils.Sleep(240, "urn");
				}
				if ( // Satanic 
					satanic != null &&
					me.Health <= (me.MaximumHealth * 0.3) &&
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
				if (Menu.Item("logic").IsActive())
				{
					if (mail != null && mail.CanBeCasted() && Toolset.HasStun(e) && !e.IsStunned() &&
						Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mail.Name) && Utils.SleepCheck("mail"))
					{
						mail.UseAbility();
						Utils.Sleep(100, "mail");
					}
					if (bkb != null && bkb.CanBeCasted() && Toolset.HasStun(e) && !e.IsStunned() &&
						Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(bkb.Name) && Utils.SleepCheck("bkb"))
					{
						bkb.UseAbility();
						Utils.Sleep(100, "bkb");
					}
				}
			}
		}

		public void OnLoadEvent()
		{
			AssemblyExtensions.InitAssembly("VickTheRock", "0.1");

			Print.LogMessage.Success("Fuzzy Wuzzy!");

			Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
			Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
			Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

		    Menu.AddItem(
				new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"ursa_overpower", true},
				    {"ursa_earthshock", true},
				    {"ursa_enrage", true}
				})));
			Menu.AddItem(
				new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
                    {"item_mask_of_madness", true},
                    {"item_blink", true},
				    {"item_heavens_halberd", true},
				    {"item_orchid", true},
					{"item_sheepstick", true},
					{ "item_bloodthorn", true},
				    {"item_urn_of_shadows", true},
				    {"item_abyssal_blade", true},
				    {"item_shivas_guard", true},
				    {"item_blade_mail", true},
				    {"item_black_king_bar", true},
				    {"item_satanic", true},
				    {"item_medallion_of_courage", true},
				    {"item_solar_crest", true}
				})));
			Menu.AddItem(new MenuItem("Heel", "Min targets to BKB").SetValue(new Slider(2, 1, 5)));
			Menu.AddItem(new MenuItem("logic", "UseBKB if e have stun").SetValue(true));
			Menu.AddItem(new MenuItem("Heelm", "Min targets to BladeMail").SetValue(new Slider(2, 1, 5)));
		}

		public void OnCloseEvent()
		{
			
		}
	}
}