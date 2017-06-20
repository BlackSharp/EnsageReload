namespace DotaAllCombo.Heroes
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

    internal class CrystalMaidenController : Variables, IHeroController
    {
        private readonly Menu skills = new Menu("Skills", "Skills");
        private readonly Menu items = new Menu("Items", "Items");
        private readonly Menu ult = new Menu("AutoAbility", "AutoAbility");
        private readonly Menu healh = new Menu("Healh", "Max Enemy Healh % to Ult");


        private Ability Q, W, R;

        private Item orchid, sheep, vail, soul, arcane, blink
        ,urn, glimmer, mail,  bkb, lotus, shiva, dagon, atos, ethereal, cheese, ghost, force, cyclone;

        
        public void OnLoadEvent()
        {

            AssemblyExtensions.InitAssembly("VickTheRock", "1.0");

            Print.LogMessage.Success("I am sworn to turn the tide where ere I can.");

            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
            Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D')));


            skills.AddItem(new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
            {
                {"crystal_maiden_crystal_nova", true},
                {"crystal_maiden_frostbite", true},
                {"crystal_maiden_freezing_field", true}
            })));
            items.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
            {
                {"item_dagon",true},
                {"item_orchid", true},
                {"item_bloodthorn", true},
                {"item_ethereal_blade", true},
                {"item_veil_of_discord", true},
                {"item_rod_of_atos", true},
                {"item_sheepstick", true},
                {"item_arcane_boots", true},
                {"item_shivas_guard",true},
                {"item_blink", true},
                {"item_lotus_orb", true},
                {"item_urn_of_shadows",true},
                {"item_soul_ring", true},
                {"item_ghost", true},
                {"item_cheese", true},
                {"item_blade_mail",true},
                {"item_black_king_bar",true}
            })));
            items.AddItem(new MenuItem("Heel", "Min targets to BKB|BladeMail").SetValue(new Slider(2, 1, 5)));
            ult.AddItem(new MenuItem("autoUlt", "AutoAbility").SetValue(true));
            ult.AddItem(new MenuItem("AutoAbility", "AutoAbility").SetValue(new AbilityToggler(new Dictionary<string, bool>
            {
                {"crystal_maiden_frostbite", true},
                {"item_ghost", true},
                {"item_rod_of_atos", true},
                {"item_black_king_bar",true}
            })));

            items.AddItem(new MenuItem("onLink", "Auto triggre Linken").SetValue(true));
            items.AddItem(new MenuItem("Link", "Auto triggre Linken").SetValue(new AbilityToggler(new Dictionary<string, bool>
            {
                {"item_force_staff", true},
                {"item_cyclone", true},
                {"item_rod_of_atos", true},
                {"item_dagon", true}
            })));
            items.AddItem(new MenuItem("atosRange", "Min Cast Range Atos").SetValue(new Slider(700, 750, 1400)));
            //skills.AddItem(new MenuItem("shotRange", "Min Cast Range Concussive Shot").SetValue(new Slider(700, 750, 1500)));
            healh.AddItem(new MenuItem("Healh", "Min healh % to ult").SetValue(new Slider(35, 10, 70))); // x/ 10%
            Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(false));
            Menu.AddSubMenu(skills);
            Menu.AddSubMenu(items);
            Menu.AddSubMenu(ult);
            Menu.AddSubMenu(healh);
            Q = me.Spellbook.SpellQ;
            W = me.Spellbook.SpellW;
            R = me.Spellbook.SpellR;
            // Item
            ethereal = me.FindItem("item_ethereal_blade");

            urn = me.FindItem("item_urn_of_shadows");
            glimmer = me.FindItem("item_glimmer_cape");
            bkb = me.FindItem("item_black_king_bar");
            mail = me.FindItem("item_blade_mail");
            lotus = me.FindItem("item_lotus_orb");
            vail = me.FindItem("item_veil_of_discord");

            cheese = me.FindItem("item_cheese");

            ghost = me.FindItem("item_ghost");

            orchid = me.FindItem("item_orchid") ?? me.FindItem("item_bloodthorn");

            atos = me.FindItem("item_rod_of_atos");

            soul = me.FindItem("item_soul_ring");

            arcane = me.FindItem("item_arcane_boots");

            blink = me.FindItem("item_blink");

            shiva = me.FindItem("item_shivas_guard");

            dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));

        } // OnLoadEvent

        public void OnCloseEvent()
        {
            e = null;
        }

        /* Доп. функции скрипта
		-----------------------------------------------------------------------------*/


        public void Combo()
        {// Ability

            
            Orbwalking.Load();
            e = Toolset.ClosestToMouse(me);
            var enemies = ObjectManager.GetEntities<Hero>()
                .Where(x => x.IsAlive && x.Team != me.Team && !x.IsIllusion && !x.IsMagicImmune()
                            && (!x.HasModifier("modifier_abaddon_borrowed_time")
                                || !x.HasModifier("modifier_item_blade_mail_reflect")))
                .ToList();
            if (e.HasModifier("modifier_abaddon_borrowed_time")
                || e.HasModifier("modifier_item_blade_mail_reflect")
                || e.IsMagicImmune())
            {
                
                e = GetClosestToTarget(enemies, e) ?? null;
                if (Utils.SleepCheck("spam"))
                {
                    Utils.Sleep(5000, "spam");
                }
            }
            if (e == null) return;
            //spell

            Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);

            sheep = e.ClassId == ClassId.CDOTA_Unit_Hero_Tidehunter ? null : me.FindItem("item_sheepstick");
            
            var stoneModif = e.HasModifier("modifier_medusa_stone_gaze_stone");
            
            
            var ally = ObjectManager.GetEntities<Hero>().Where(x => x.IsAlive && x.Team == me.Team && !x.IsIllusion).ToList();

            var v = GetClosestToTarget(ally, e);
            if (Active && me.IsAlive && e.IsAlive && me.CanCast() && Utils.SleepCheck("combosleep"))
            {
                Utils.Sleep(250, "combosleep");
                if (R.IsInAbilityPhase || R.IsChanneling || me.IsChanneling())
                    return;
                if (stoneModif) return;
                //var noBlade = e.HasModifier("modifier_item_blade_mail_reflect");
                if (e.IsVisible && me.Distance2D(e) <= 2300)
                {
                    var distance = me.IsVisibleToEnemies ? 1400 : W.GetCastRange() + me.HullRadius;

                    float angle = me.FindAngleBetween(e.Position, true);
                    Vector3 pos = new Vector3((float)(e.Position.X - 250 * Math.Cos(angle)),
                        (float)(e.Position.Y - 250 * Math.Sin(angle)), 0);
                    
                    
                    if (
                        // cheese
                        cheese != null
                        && cheese.CanBeCasted()
                        && me.Health <= me.MaximumHealth * 0.3
                        && me.Distance2D(e) <= 700
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(cheese.Name)
                        )
                        cheese.UseAbility();
                    if ( // SoulRing Item 
                        soul != null
                        && soul.CanBeCasted()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(soul.Name)
                        && me.CanCast()
                        && me.Health >= me.MaximumHealth * 0.5
                        && me.Mana <= R.ManaCost
                        )
                        soul.UseAbility();
                    if ( // Arcane Boots Item
                        arcane != null
                        && arcane.CanBeCasted()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(arcane.Name)
                        && me.CanCast()
                        && me.Mana <= me.MaximumMana * 0.5
                        )
                        arcane.UseAbility();
                    if ( //Ghost
                        ghost != null
                        && ghost.CanBeCasted()
                        && me.CanCast()
                        && me.Distance2D(e) < me.AttackRange 
                        && (me.Health <= me.MaximumHealth * 0.5
                        || R.CanBeCasted() && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name)
                        || bkb == null
                        || !bkb.CanBeCasted()
                        || enemies.Count(x => x.Distance2D(me) <= 650) <=
                           Menu.Item("Heel").GetValue<Slider>().Value
                        || !Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(bkb.Name)
                        )
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ghost.Name)
                        && !me.IsMagicImmune()
                        )
                        ghost.UseAbility();
                    if (blink != null
                        && W.CanBeCasted()
                        && me.CanCast()
                        && blink.CanBeCasted()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(blink.Name)
                        && me.Distance2D(e) >= 490
                        && me.Distance2D(pos) <= 1180
                        )
                        blink.UseAbility(pos);
                    uint elsecount = 0;
                    elsecount += 1;
                    if (elsecount == 1
                        && urn != null && urn.CanBeCasted() && urn.CurrentCharges > 0 
                        && me.Distance2D(e) <= urn.CastRange && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(urn.Name) 
                        )
                    urn.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 2
                        && glimmer != null 
                        && glimmer.CanBeCasted() 
                        && me.Distance2D(e) <= 700 
                        )
                    glimmer.UseAbility(me);
                    else elsecount += 1;
                    if (elsecount == 3
                        && mail != null 
                        && mail.CanBeCasted() 
                        && enemies.Count(x => x.Distance2D(me) <= 650) >=
                        Menu.Item("Heel").GetValue<Slider>().Value
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mail.Name)
                        )
                     mail.UseAbility();
                    else elsecount += 1;
                    if (elsecount == 4
                        && bkb != null 
                        && bkb.CanBeCasted() 
                        && enemies.Count(x => x.Distance2D(me) <= 650) >=
                        Menu.Item("Heel").GetValue<Slider>().Value
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(bkb.Name)
                        )
                    bkb.UseAbility();
                    else elsecount += 1;
                    if (elsecount == 5
                        && lotus != null 
                        && lotus.CanBeCasted() 
                        && enemies.Count(x => x.Distance2D(me) <= 650) >= 1
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(lotus.Name)
                        )
                    lotus.UseAbility(me);
                    else elsecount += 1;
                    if (elsecount == 6
                        && sheep != null
                        && sheep.CanBeCasted()
                        && me.CanCast()
                        && !e.IsLinkensProtected()
                        && !e.IsMagicImmune()
                        && me.Distance2D(e) <= 1400
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(sheep.Name)

                        )
                        sheep.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 7
                        && vail != null
                        && vail.CanBeCasted()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(vail.Name)
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && (me.Distance2D(e) < distance
                        || v.Distance2D(e) <= 300 && me.Distance2D(e) <= 1200)

                        )
                        vail.UseAbility(e.Position);
                    else elsecount += 1;
                    if (elsecount == 8
                        && orchid != null
                        && orchid.CanBeCasted()
                        && me.CanCast()
                        && !e.IsLinkensProtected()
                        && !e.IsMagicImmune()
                        && me.Distance2D(e) <= distance
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(orchid.Name)

                        )
                        orchid.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 9
                        && ethereal != null
                        && ethereal.CanBeCasted()
                        && me.CanCast()
                        && !e.IsLinkensProtected()
                        && !e.IsMagicImmune()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)
                        )
                        ethereal.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 10
                        && Q != null
                        && Q.CanBeCasted()
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && me.Distance2D(e) < Q.CastRange
                        )
                        Q.UseAbility(e.Position);
                    elsecount += 1;
                    if (elsecount == 11
                        && atos != null
                        && atos.CanBeCasted()
                        && me.CanCast()
                        && !e.IsLinkensProtected()
                        && !e.IsMagicImmune()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(atos.Name)
                        && (me.Distance2D(e) < distance
                        || v.Distance2D(e) <= 300 && me.Distance2D(e) <= 1200
                        || me.Distance2D(e) <= Menu.Item("atosRange").GetValue<Slider>().Value)
                        )
                        atos.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 12
                        && W != null
                        && W.CanBeCasted()
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && me.Distance2D(e) < W.CastRange
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)
                        )
                        W.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 13
                        && shiva != null
                        && shiva.CanBeCasted()
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(shiva.Name)
                        && me.Distance2D(e) <= 600
                        )
                        shiva.UseAbility();
                    else elsecount += 1;
                    if (elsecount == 14
                        && dagon != null
                        && dagon.CanBeCasted()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled("item_dagon")
                        && me.CanCast()
                        && (ethereal == null
                        || e.HasModifier("modifier_item_ethereal_blade_slow")
                        || ethereal.Cooldown < 17)
                        && !e.IsLinkensProtected()
                        && !e.IsMagicImmune()
                        )
                        dagon.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 15
                        && R != null
                        && R.CanBeCasted()
                        && me.Distance2D(e) <= R.GetCastRange()-100
                        && !me.HasModifier("modifier_pugna_nether_ward_aura")
                        && (e.HasModifiers(new[]
                        {
                            "modifier_rod_of_atos_debuff",
                            "modifier_meepo_earthbind",
                            "modifier_pudge_dismember",
                            "modifier_lone_druid_spirit_bear_entangle_effect",
                            "modifier_legion_commander_duel",
                            "modifier_kunkka_torrent",
                            "modifier_ice_blast",
                            "modifier_crystal_maiden_crystal_nova",
                            "modifier_enigma_black_hole_pull",
                            "modifier_ember_spirit_searing_chains",
                            "modifier_dark_troll_warlord_ensnare",
                            "modifier_crystal_maiden_frostbite",
                            "modifier_axe_berserkers_call",
                            "modifier_bane_fiends_grip",
                            "modifier_faceless_void_chronosphere_freeze",
                            "modifier_storm_spirit_electric_vortex_pull",
                            "modifier_axe_berserkers_call",
                            "modifier_naga_siren_ensnare"
                        }, false)
                        || e.ClassId != ClassId.CDOTA_Unit_Hero_FacelessVoid
                            && !e.HasModifier("modifier_faceless_void_chronosphere_freeze")
                            || e.IsStunned()
                            || e.IsHexed() 
                            || e.IsRooted())
                        && !e.HasModifiers(new[]
                        {
                            "modifier_medusa_stone_gaze_stone",
                            "modifier_faceless_void_time_walk",
                            "modifier_item_monkey_king_bar",
                            "modifier_rattletrap_battery_assault",
                            "modifier_item_blade_mail_reflect",
                            "crystal_maiden_frostbite",
                            "modifier_pudge_meat_hook",
                            "modifier_zuus_lightningbolt_vision_thinker",
                            "modifier_puck_phase_shift",
                            "modifier_eul_cyclone",
                            "modifier_dazzle_shallow_grave",
                            "modifier_mirana_leap",
                            "modifier_abaddon_borrowed_time",
                            "modifier_winter_wyvern_winters_curse",
                            "modifier_earth_spirit_rolling_boulder_caster",
                            "modifier_brewmaster_storm_cyclone",
                            "modifier_spirit_breaker_charge_of_darkness",
                            "modifier_shadow_demon_disruption",
                            "modifier_tusk_snowball_movement",
                            "modifier_invoker_tornado",
                            "modifier_obsidian_destroyer_astral_imprisonment_prison"
                        }, false)
                        && (e.FindSpell("abaddon_borrowed_time")?.Cooldown > 0
                            || e.FindSpell("abaddon_borrowed_time") == null)

                        && !e.IsMagicImmune()
                        && e.Health >= e.MaximumHealth / 100 * Menu.Item("Healh").GetValue<Slider>().Value
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name)
                    )
                    {
                        R.UseAbility();
                        Utils.Sleep(250, "combosleep");
                    }
                    if (Menu.Item("orbwalk").GetValue<bool>() && me.Distance2D(e) > 350 && !e.IsAttackImmune())
                    {
                        Orbwalking.Orbwalk(e, 0, 1600, true, true);
                    }
                    else if (me.Distance2D(e) < 350 && !e.IsAttackImmune() && Utils.SleepCheck("attack"))
                    {
                        me.Attack(e);
                        Utils.Sleep(190, "attack");
                    }
                }
            }
            if (Menu.Item("autoUlt").GetValue<bool>() && me.IsAlive && !Active)
            {
                A();
            }

        } // Combo


        private Hero GetClosestToTarget(List<Hero> units, Hero z)
        {
            Hero closestHero = null;
            foreach (var v in units.Where(v => closestHero == null || closestHero.Distance2D(z) > v.Distance2D(z)))
            {
                closestHero = v;
            }
            return closestHero;
        }

        public void A()
        {
                var v =
                    ObjectManager.GetEntities<Hero>()
                        .Where(x => x.IsVisible && x.IsAlive && x.IsValid && x.Team != me.Team && !x.IsIllusion).ToList();

                var ecount = v.Count;
                if (ecount == 0) return;
            if (Game.Ping <= 200 && R != null && R.CanBeCasted()
                && R.IsInAbilityPhase
                && me.Distance2D(e) <= R.CastRange - 100)
            {
                if (bkb != null
                    && bkb.CanBeCasted()
                    && v.Count(x => x.Distance2D(me) <= 650) >=
                    Menu.Item("Heel").GetValue<Slider>().Value
                    && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(bkb.Name)
                    && Utils.SleepCheck("combosleep")
                )
                {
                    Utils.Sleep(200, "combosleep");
                    me.Stop();
                    bkb.UseAbility();
                }
                if (ghost != null 
                    && ghost.CanBeCasted()
                    && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(ghost.Name)
                    && (bkb == null
                    || !bkb.CanBeCasted()
                    || v.Count(x => x.Distance2D(me) <= 650) <= Menu.Item("Heel").GetValue<Slider>().Value
                    || !Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(bkb.Name))
                    && Utils.SleepCheck("combosleep")
                    )
                {
                    Utils.Sleep(200, "combosleep");
                    me.Stop();
                    ghost.UseAbility();
                    R.UseAbility();
                }
            }
                
            if (R.IsInAbilityPhase || R.IsChanneling || me.IsChanneling())
                return;
            for (int i = 0; i < ecount; ++i)
                {
                    var reflect = v[i].HasModifier("modifier_item_blade_mail_reflect");

                    if (me.HasModifier("modifier_spirit_breaker_charge_of_darkness_vision"))
                    {
                        if (v[i].ClassId == ClassId.CDOTA_Unit_Hero_SpiritBreaker)
                        {
                            if (atos != null && atos.CanBeCasted())
                            {
                                if (me.Distance2D(v[i]) <= 700
                                    && !v[i].IsMagicImmune()
                                    && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(atos.Name)
                                    && Utils.SleepCheck("combosleep")
                                    )
                                {
                                    atos.UseAbility(v[i]);
                                    Utils.Sleep(250, "combosleep");
                                }
                            }
                            else
                            {
                                /*float angle = me.FindAngleBetween(v[i].Position, true);
                                Vector3 pos = new Vector3((float)(me.Position.X + 100 * Math.Cos(angle)),
                                    (float)(me.Position.Y + 100 * Math.Sin(angle)), 0);*/

                                if (W != null && W.CanBeCasted() && me.Distance2D(v[i]) <= 900 + Game.Ping
                                    && !v[i].IsMagicImmune()
                                    && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(W.Name)
                                    && Utils.SleepCheck("combosleep")
                                )
                                {
                                    W.UseAbility(e);
                                    Utils.Sleep(250, "combosleep");
                                }
                            }
                        }
                    }
                /*
                if (R != null && R.CanBeCasted() && me.Distance2D(v[i]) <= R.GetCastRange() -100
                    && !me.HasModifier("modifier_pugna_nether_ward_aura")
                    && (v[i].HasModifiers(new[]
                    {
                        "modifier_rod_of_atos_debuff",
                        "modifier_meepo_earthbind",
                        "modifier_pudge_dismember",
                        "modifier_lone_druid_spirit_bear_entangle_effect",
                        "modifier_legion_commander_duel",
                        "modifier_kunkka_torrent",
                        "modifier_ice_blast",
                        "modifier_crystal_maiden_crystal_nova",
                        "modifier_enigma_black_hole_pull",
                        "modifier_ember_spirit_searing_chains",
                        "modifier_dark_troll_warlord_ensnare",
                        "modifier_crystal_maiden_frostbite",
                        "modifier_axe_berserkers_call",
                        "modifier_bane_fiends_grip",
                        "modifier_faceless_void_chronosphere_freeze",
                        "modifier_storm_spirit_electric_vortex_pull",
                        "modifier_naga_siren_ensnare"
                    }, false)
                        || v[i].IsStunned()
                        || v[i].IsHexed() 
                        || v[i].IsRooted())
                    && (v[i].FindItem("item_cyclone")?.Cooldown > 0
                        || v[i].FindItem("item_cyclone") == null)
                    && (v[i].FindItem("item_force_staff")?.Cooldown > 0
                        || v[i].FindItem("item_force_staff") == null)
                    && !v[i].HasModifiers(new[]
                    {
                        "modifier_medusa_stone_gaze_stone",
                        "modifier_faceless_void_time_walk",
                        "modifier_item_monkey_king_bar",
                        "modifier_rattletrap_battery_assault",
                        "modifier_item_blade_mail_reflect",
                        "modifier_pudge_meat_hook",
                        "modifier_zuus_lightningbolt_vision_thinker",
                        "modifier_puck_phase_shift",
                        "modifier_eul_cyclone",
                        "modifier_invoker_tornado",
                        "modifier_dazzle_shallow_grave",
                        "modifier_mirana_leap",
                        "modifier_abaddon_borrowed_time",
                        "modifier_winter_wyvern_winters_curse",
                        "modifier_earth_spirit_rolling_boulder_caster",
                        "modifier_brewmaster_storm_cyclone",
                        "modifier_spirit_breaker_charge_of_darkness",
                        "modifier_shadow_demon_disruption",
                        "modifier_tusk_snowball_movement",
                        "modifier_invoker_tornado",
                        "modifier_obsidian_destroyer_astral_imprisonment_prison"
                    }, false)
                    && (v[i].FindSpell("abaddon_borrowed_time")?.Cooldown > 0
                        || v[i].FindSpell("abaddon_borrowed_time") == null)
                    && !v[i].IsMagicImmune()
                    && v[i].Health >= v[i].MaximumHealth / 100 * Menu.Item("Healh").GetValue<Slider>().Value
                    && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(R.Name)
                    && Utils.SleepCheck("combosleep")
                )
                {
                    R.UseAbility();
                    Utils.Sleep(250, "combosleep");
                }
                */
                if (glimmer != null
                    && glimmer.CanBeCasted()
                    && me.Distance2D(v[i]) <= 700
                    && me.HasModifier("modifier_crystal_maiden_freezing_field")
                    && Utils.SleepCheck("combosleep"))
                {
                    glimmer.UseAbility(me);
                    Utils.Sleep(250, "combosleep");
                }
                if (W != null && W.CanBeCasted() && me.Distance2D(v[i]) <= 500
                        && v[i].Distance2D(me) <= me.HullRadius + 50
                        && v[i].NetworkActivity == NetworkActivity.Attack
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(W.Name)
                        && Utils.SleepCheck("combosleep")
                        && !v[i].IsMagicImmune()
                    )
                    {
                        W.UseAbility(e);
                        Utils.Sleep(250, "combosleep");
                    }
                    if (W != null && W.CanBeCasted() && me.Distance2D(v[i]) <= W.GetCastRange()+me.HullRadius+24
                        && !v[i].IsLinkensProtected()
                        &&
                        !v[i].HasModifiers(new[]
                        {
                            "modifier_rod_of_atos_debuff",
                            "modifier_shadow_shaman_shackles",
                            "modifier_winter_wyvern_cold_embrace",
                            "modifier_storm_spirit_electric_vortex_pull",
                            "modifier_rubick_telekinesis",
                            "modifier_bane_fiends_grip",
                            "modifier_axe_berserkers_call",
                            "modifier_crystal_maiden_crystal_nova",
                            "modifier_dark_troll_warlord_ensnare",
                            "modifier_ember_spirit_searing_chains",
                            "modifier_enigma_black_hole_pull",
                            "modifier_ice_blast",
                            "modifier_kunkka_torrent",
                            "modifier_legion_commander_duel",
                            "modifier_lone_druid_spirit_bear_entangle_effect",
                            "modifier_naga_siren_ensnare",
                            "modifier_pudge_dismember",
                            "modifier_meepo_earthbind"
                        }, false)
                        && (v[i].FindSpell("magnataur_reverse_polarity").IsInAbilityPhase
                            || v[i].FindItem("item_blink")?.Cooldown > 11
                            || v[i].FindSpell("queenofpain_blink").IsInAbilityPhase
                            || v[i].FindSpell("antimage_blink").IsInAbilityPhase
                            || v[i].FindSpell("antimage_mana_void").IsInAbilityPhase
                            || v[i].FindSpell("legion_commander_duel")?.Cooldown <= 0
                            || v[i].FindSpell("doom_bringer_doom").IsInAbilityPhase
                            || v[i].HasModifier("modifier_faceless_void_chronosphere_freeze")
                            || v[i].FindSpell("witch_doctor_death_ward").IsInAbilityPhase
                            || v[i].FindSpell("rattletrap_power_cogs").IsInAbilityPhase
                            || v[i].FindSpell("tidehunter_ravage").IsInAbilityPhase
                            || v[i].FindSpell("axe_berserkers_call").IsInAbilityPhase
                            || v[i].FindSpell("brewmaster_primal_split").IsInAbilityPhase
                            || v[i].FindSpell("omniknight_guardian_angel").IsInAbilityPhase
                            || v[i].FindSpell("queenofpain_sonic_wave").IsInAbilityPhase
                            || v[i].FindSpell("sandking_epicenter").IsInAbilityPhase
                            || v[i].FindSpell("slardar_slithereen_crush").IsInAbilityPhase
                            || v[i].FindSpell("tiny_toss").IsInAbilityPhase
                            || v[i].FindSpell("tusk_walrus_punch").IsInAbilityPhase
                            || v[i].FindSpell("faceless_void_time_walk").IsInAbilityPhase
                            || v[i].FindSpell("faceless_void_chronosphere").IsInAbilityPhase
                            || v[i].FindSpell("disruptor_static_storm")?.Cooldown <= 0
                            || v[i].FindSpell("lion_finger_of_death")?.Cooldown <= 0
                            || v[i].FindSpell("luna_eclipse")?.Cooldown <= 0
                            || v[i].FindSpell("lina_laguna_blade")?.Cooldown <= 0
                            || v[i].FindSpell("doom_bringer_doom")?.Cooldown <= 0
                            || v[i].FindSpell("life_stealer_rage")?.Cooldown <= 0
                            && me.Level >= 7
                        )
                        && (v[i].FindItem("item_manta")?.Cooldown > 0
                            || v[i].FindItem("item_manta") == null || v[i].IsStunned() || v[i].IsHexed() || v[i].IsRooted())
                        && !v[i].IsMagicImmune()
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(W.Name)
                        && !v[i].HasModifier("modifier_medusa_stone_gaze_stone")
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        W.UseAbility(v[i]);
                        Utils.Sleep(250, "combosleep");
                    }
                    if (Menu.Item("onLink").GetValue<bool>() && v[i].IsLinkensProtected() && (me.IsVisibleToEnemies || Active) && Utils.SleepCheck("combosleep"))
                    {
                        if (force != null && force.CanBeCasted() && me.Distance2D(v[i]) < force.GetCastRange() &&
                            Menu.Item("Link").GetValue<AbilityToggler>().IsEnabled(force.Name))
                            force.UseAbility(v[i]);
                        else if (cyclone != null && cyclone.CanBeCasted() && me.Distance2D(v[i]) < cyclone.GetCastRange() &&
                                 Menu.Item("Link").GetValue<AbilityToggler>().IsEnabled(cyclone.Name))
                            cyclone.UseAbility(v[i]);
                        else if (atos != null && atos.CanBeCasted() && me.Distance2D(v[i]) < atos.GetCastRange() - 400 &&
                                 Menu.Item("Link").GetValue<AbilityToggler>().IsEnabled(atos.Name))
                            atos.UseAbility(v[i]);
                        else if (dagon != null && dagon.CanBeCasted() && me.Distance2D(v[i]) < dagon.GetCastRange() &&
                                 Menu.Item("Link").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
                            dagon.UseAbility(v[i]);
                        Utils.Sleep(250, "combosleep");
                    }
            }
        } 
    }
}