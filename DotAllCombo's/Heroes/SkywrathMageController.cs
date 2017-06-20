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


    internal class SkywrathMageController : Variables, IHeroController
    {
        private readonly Menu skills = new Menu("Skills", "Skills");
        private readonly Menu items = new Menu("Items", "Items");
        private readonly Menu ult = new Menu("Auto Ability", "Auto Ability");
        private readonly Menu healh = new Menu("Healh", "Max Enemy Healh % to Ult");


        private Ability Q, W, E, R;

        private Item orchid, sheep, vail, soul, arcane, blink, shiva, dagon, atos, ethereal, cheese, ghost, force, cyclone;
        public void OnLoadEvent()
        {

            AssemblyExtensions.InitAssembly("VickTheRock", "1.0");

            Print.LogMessage.Success("I am sworn to turn the tide where ere I can.");

            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
            Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("keyQ", "Spam Q key").SetValue(new KeyBind('Q', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("comboSpeed", "Ability cast speed.The less - the faster.").SetValue(new Slider(250, 100, 350)));

            skills.AddItem(new MenuItem("Skills", "Skills:").SetValue(new AbilityToggler(new Dictionary<string, bool>
            {
                {"skywrath_mage_arcane_bolt", true},
                {"skywrath_mage_concussive_shot", true},
                {"skywrath_mage_ancient_seal", true},
                {"skywrath_mage_mystic_flare", true}
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
                {"item_soul_ring", true},
                {"item_ghost", true},
                {"item_cheese", true}
            })));
            ult.AddItem(new MenuItem("autoUlt", "AutoAbility ").SetValue(true));
            ult.AddItem(new MenuItem("AutoAbility", "AutoAbility:").SetValue(new AbilityToggler(new Dictionary<string, bool>
            {
                {"skywrath_mage_concussive_shot", true},
                {"skywrath_mage_ancient_seal", true},
                {"skywrath_mage_mystic_flare", true},
                {"item_rod_of_atos", true},
                {"item_cyclone", true},
                {"item_ethereal_blade", true},
                {"item_veil_of_discord", true},

            })));
            items.AddItem(new MenuItem("onLink", "Auto triggre Linken").SetValue(true));
            items.AddItem(new MenuItem("Link", "Auto triggre Linken").SetValue(new AbilityToggler(new Dictionary<string, bool>
            {
                {"item_force_staff", true},
                {"item_cyclone", true},
                {"item_orchid", true},
                {"item_bloodthorn", true},
                {"item_rod_of_atos", true},
                {"item_dagon", true}
            })));
            items.AddItem(new MenuItem("atosRange", "Min Cast Range Atos").SetValue(new Slider(750, 700, 1400)));
            skills.AddItem(new MenuItem("shotRange", "Min Cast Range Concussive Shot").SetValue(new Slider(750, 700, 1500)));
            healh.AddItem(new MenuItem("Healh", "Max healh % to ult").SetValue(new Slider(35, 10, 70))); // x/ 10%
            Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(false));
            Menu.AddSubMenu(skills);
            Menu.AddSubMenu(items);
            Menu.AddSubMenu(ult);
            Menu.AddSubMenu(healh);
        } // OnLoadEvent

        public void OnCloseEvent()
        {
            e = null;
        }

        /* Доп. функции скрипта
		-----------------------------------------------------------------------------*/


        public void Combo()
        {
            e = Toolset.ClosestToMouse(me);
            if (e.HasModifier("modifier_abaddon_borrowed_time")
                || e.HasModifier("modifier_item_blade_mail_reflect")
                || e.IsMagicImmune())
            {
                var enemies = ObjectManager.GetEntities<Hero>()
                    .Where(x => x.IsAlive && x.Team != me.Team && !x.IsIllusion && !x.IsMagicImmune()
                                && (!x.HasModifier("modifier_abaddon_borrowed_time")
                                    || !x.HasModifier("modifier_item_blade_mail_reflect"))
                                && x.Distance2D(e) > 200)
                    .ToList();
                e = GetClosestToTarget(enemies, e) ?? null;
                if (Utils.SleepCheck("spam"))
                {
                    Utils.Sleep(5000, "spam");
                }
            }
            Orbwalking.Load();
            if (e == null) return;

            //spell
            Q = me.Spellbook.SpellQ;

            W = me.Spellbook.SpellW;

            E = me.Spellbook.SpellE;

            R = me.Spellbook.SpellR;
            // Item
            ethereal = me.FindItem("item_ethereal_blade");

            sheep = e.ClassId == ClassId.CDOTA_Unit_Hero_Tidehunter ? null : me.FindItem("item_sheepstick");

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


            Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);


            Push = Game.IsKeyDown(Menu.Item("keyQ").GetValue<KeyBind>().Key);

            var stoneModif = e.HasModifier("modifier_medusa_stone_gaze_stone");
            if (Menu.Item("autoUlt").GetValue<bool>() && !Active && me.IsAlive )
            {
                A();
            }
            if (Push)
            {
                if (
                    Q != null
                    && Q.CanBeCasted()
                    && (e.IsLinkensProtected()
                        || !e.IsLinkensProtected())
                    && me.CanCast()
                    && me.Distance2D(e) < Q.GetCastRange() + me.HullRadius + 24
                    && Utils.SleepCheck("combosleep")
                )
                {
                    Q.UseAbility(e);
                    Utils.Sleep(500, "combosleep");
                }
            }
            if (Active && me.IsAlive && e.IsAlive && me.CanCast() && Utils.SleepCheck("combosleep"))
            {
                Utils.Sleep(Menu.Item("comboSpeed").GetValue<Slider>().Value, "combosleep");
                if (stoneModif) return;
                //var noBlade = e.HasModifier("modifier_item_blade_mail_reflect");
                if (e.IsVisible && me.Distance2D(e) <= 2300)
                {
                    var distance = me.IsVisibleToEnemies ? 1400 : E.GetCastRange() + me.HullRadius;
                    
                    float angle = me.FindAngleBetween(e.Position, true);
                    Vector3 pos = new Vector3((float)(e.Position.X - 300 * Math.Cos(angle)),
                        (float)(e.Position.Y - 300 * Math.Sin(angle)), 0);

                    var ally = ObjectManager.GetEntities<Hero>().Where(x => x.IsAlive && x.Team == me.Team && !x.IsIllusion).ToList();

                    var v = GetClosestToTarget(ally, e);
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
                        && me.Mana <= R.ManaCost
                        )
                        arcane.UseAbility();
                    if ( //Ghost
                        ghost != null
                        && ghost.CanBeCasted()
                        && me.CanCast()
                        && (me.Position.Distance2D(e) < e.AttackRange 
                        && me.Health <= me.MaximumHealth * 0.5
                        || me.Health <= me.MaximumHealth * 0.3)
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ghost.Name)
                        )
                        ghost.UseAbility();
                    if (blink != null
                        && Q.CanBeCasted()
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
                    if (elsecount == 2
                        && vail != null
                        && vail.CanBeCasted()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(vail.Name)
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && me.Distance2D(e) <= distance
                        )
                        vail.UseAbility(e.Position);
                    else elsecount += 1;
                    if (elsecount == 3
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
                    if (elsecount == 4
                        && E != null
                        && E.CanBeCasted()
                        && me.CanCast()
                        && !e.IsLinkensProtected()
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(E.Name)
                        && me.Position.Distance2D(e) < E.GetCastRange() + me.HullRadius + 500
                        )   
                        E.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 5
                        && ethereal != null
                        && ethereal.CanBeCasted()
                        && me.CanCast()
                        && !e.IsLinkensProtected()
                        && !e.IsMagicImmune()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)
                        )
                        ethereal.UseAbility(e);
                    else elsecount += 1;
                    if (elsecount == 6
                        && Q != null
                        && Q.CanBeCasted()
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && me.Distance2D(e) < distance
                        )
                        Q.UseAbility(e);
                    elsecount += 1;
                    if (elsecount == 7
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
                    if (elsecount == 8
                        && W != null
                        && e.IsVisible
                        && W.CanBeCasted()
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && (me.Distance2D(e) < distance
                        ||v.Distance2D(e)<=300 && me.Distance2D(e)<= 1500
                        ||me.Distance2D(e)<= Menu.Item("shotRange").GetValue<Slider>().Value)
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)
                        )
                        W.UseAbility();
                    else elsecount += 1;
                    if (elsecount == 9
                        && shiva != null
                        && shiva.CanBeCasted()
                        && me.CanCast()
                        && !e.IsMagicImmune()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(shiva.Name)
                        && me.Distance2D(e) <= 600
                        )
                        shiva.UseAbility();
                    else elsecount += 1;
                    if (elsecount == 10
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
                    if (elsecount == 11
                        && R != null
                        && R.CanBeCasted() 
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name)
                        && me.CanCast()
                        && e.MovementSpeed <= 230
                        && me.Position.Distance2D(e) < R.CastRange
                        && e.Health >= e.MaximumHealth / 100 * Menu.Item("Healh").GetValue<Slider>().Value
                        && !me.HasModifier("modifier_pugna_nether_ward_aura")
                        && !e.HasModifier("modifier_item_blade_mail_reflect")
                        && !e.HasModifier("modifier_skywrath_mystic_flare_aura_effect")
                        && !e.HasModifier("modifier_obsidian_destroyer_astral_imprisonment_prison")
                        && !e.HasModifier("modifier_puck_phase_shift")
                        && !e.HasModifier("modifier_eul_cyclone")
                        && !e.HasModifier("modifier_dazzle_shallow_grave")
                        && !e.HasModifier("modifier_brewmaster_storm_cyclone")
                        && !e.HasModifier("modifier_spirit_breaker_charge_of_darkness")
                        && !e.HasModifier("modifier_shadow_demon_disruption")
                        && !e.HasModifier("modifier_tusk_snowball_movement")
                        && !e.IsMagicImmune()
                        && (e.FindSpell("abaddon_borrowed_time")?.Cooldown > 0
                        || e.FindSpell("abaddon_borrowed_time")==null)
                        && !e.HasModifier("modifier_abaddon_borrowed_time_damage_redirect")
                        && (e.FindItem("item_cyclone")?.Cooldown > 0
                        || e.FindItem("item_cyclone") == null || e.IsStunned() || e.IsHexed() || e.IsRooted() 
                        || e.FindItem("item_force_staff")?.Cooldown > 0 || e.FindItem("item_force_staff") == null )   
                    )
                    {
                        R.UseAbility(Prediction.InFront(e, 100));
                    }
                    else
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
            
            
                Q = me.Spellbook.SpellQ;

                W = me.Spellbook.SpellW;

                E = me.Spellbook.SpellE;

                R = me.Spellbook.SpellR;
                // Item
                ethereal = me.FindItem("item_ethereal_blade");
                
                vail = me.FindItem("item_veil_of_discord");
                
                ghost = me.FindItem("item_ghost");

                orchid = me.FindItem("item_orchid") ?? me.FindItem("item_bloodthorn");

                atos = me.FindItem("item_rod_of_atos");
                

                dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
                var v =
                    ObjectManager.GetEntities<Hero>()
                        .Where(x => x.IsVisible && x.IsAlive && x.IsValid && x.Team != me.Team && !x.IsIllusion).ToList();

                var ecount = v.Count;
                if (ecount == 0) return;

                force = me.FindItem("item_force_staff");
                cyclone = me.FindItem("item_cyclone");
                E = me.Spellbook.SpellE;
                for (int i = 0; i < ecount; ++i)
                {
                    var reflect = v[i].HasModifier("modifier_item_blade_mail_reflect");

                    sheep = v[i].ClassId == ClassId.CDOTA_Unit_Hero_Tidehunter ? null : me.FindItem("item_sheepstick");
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
                                float angle = me.FindAngleBetween(v[i].Position, true);
                                Vector3 pos = new Vector3((float)(me.Position.X + 100 * Math.Cos(angle)),
                                    (float)(me.Position.Y + 100 * Math.Sin(angle)), 0);

                                if (W != null && W.CanBeCasted() && me.Distance2D(v[i]) <= 900 + Game.Ping
                                    && !v[i].IsMagicImmune()
                                    && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(W.Name)
                                    && Utils.SleepCheck("combosleep")
                                )
                                {
                                    W.UseAbility();
                                    Utils.Sleep(250, "combosleep");
                                }

                                if (R != null && R.CanBeCasted() && me.Distance2D(v[i]) <= 700 + Game.Ping
                                    && !v[i].HasModifiers(new[]
                                    {
                                        "modifier_item_blade_mail_reflect",
                                        "modifier_skywrath_mystic_flare_aura_effect",
                                        "modifier_dazzle_shallow_grave"
                                    }, false)
                                    && !v[i].IsMagicImmune()
                                    && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(R.Name)
                                    && Utils.SleepCheck("combosleep")
                                )
                                {
                                    R.UseAbility(pos);
                                    Utils.Sleep(250, "combosleep");
                                }
                                if (cyclone != null && !R.CanBeCasted()
                                    && cyclone.CanBeCasted()
                                    && me.Distance2D(v[i]) <= 500 + Game.Ping
                                    && Utils.SleepCheck("combosleep")
                                )
                                {
                                    cyclone.UseAbility(me);
                                    Utils.Sleep(250, "combosleep");
                                }
                            }
                        }
                    }

                    if (cyclone != null && reflect && cyclone.CanBeCasted() &&
                        v[i].HasModifier("modifier_skywrath_mystic_flare_aura_effect") &&
                        me.Distance2D(v[i]) < cyclone.GetCastRange()
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(cyclone.Name)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        cyclone.UseAbility(me);
                        Utils.Sleep(250, "combosleep");
                    }

                    if (W != null && W.CanBeCasted()
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(W.Name)
                        && me.Distance2D(v[i]) <= 1200
                        && !v[i].IsMagicImmune()
                        && (v[i].MovementSpeed <= 255
                        && !v[i].HasModifier("modifier_phantom_assassin_stiflingdagger")
                        || v[i].Distance2D(me) <= me.HullRadius + 24
                        && v[i].NetworkActivity == NetworkActivity.Attack
                        || v[i].MagicDamageResist <= 0.07)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        W.UseAbility();
                        Utils.Sleep(250, "combosleep");
                    }

                    if (vail != null && v[i].HasModifier("modifier_skywrath_mystic_flare_aura_effect")
                        && vail.CanBeCasted()
                        && !v[i].IsMagicImmune()
                        && me.Distance2D(v[i]) <= 1200
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(vail.Name)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        vail.UseAbility(v[i].Position);
                        Utils.Sleep(250, "combosleep");
                    }

                    if (ethereal != null
                        && ethereal.CanBeCasted()
                        && v[i].HasModifier("modifier_skywrath_mystic_flare_aura_effect")
                        && !v[i].HasModifier("modifier_legion_commander_duel")
                        && E.CanBeCasted()
                        && me.Distance2D(v[i]) <= ethereal.GetCastRange() + me.HullRadius
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        ethereal.UseAbility(v[i]);
                        Utils.Sleep(250, "combosleep");
                    }

                    if (E != null && !E.CanBeCasted() && !v[i].IsStunned() && !v[i].IsHexed() && !v[i].IsRooted()
                        && (orchid != null && orchid.CanBeCasted() || sheep != null && sheep.CanBeCasted()))
                        E = orchid ?? sheep;
                    if (E != null && v[i].HasModifier("modifier_skywrath_mystic_flare_aura_effect")
                        && E.CanBeCasted()
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(E.Name)
                        && (v[i].FindItem("item_manta")?.Cooldown > 0
                            || v[i].FindItem("item_manta") == null || v[i].IsStunned() || v[i].IsHexed() || v[i].IsRooted())
                        && me.Distance2D(v[i]) <= 900
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        E.UseAbility(v[i]);
                        Utils.Sleep(250, "combosleep");
                    }

                    if (atos != null && R != null && R.CanBeCasted() && atos.CanBeCasted()
                        && !v[i].IsLinkensProtected()
                        && me.Distance2D(v[i]) <= 1200
                        && v[i].MagicDamageResist <= 0.1
                        && !v[i].IsMagicImmune()
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(atos.Name)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        atos.UseAbility(v[i]);
                        Utils.Sleep(250, "combosleep");
                    }

                    if (E != null && E.CanBeCasted()
                        && me.Distance2D(v[i]) <= E.GetCastRange()
                        && !v[i].IsMagicImmune()
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(E.Name)
                        && !v[i].IsLinkensProtected()
                        && (v[i].HasModifiers(new[]
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
                            || v[i].FindItem("item_blink")?.Cooldown >= 11
                            || v[i].FindSpell("magnataur_reverse_polarity").IsInAbilityPhase
                            || v[i].FindSpell("queenofpain_blink").IsInAbilityPhase
                            || v[i].FindSpell("antimage_blink").IsInAbilityPhase
                            || v[i].FindSpell("antimage_mana_void").IsInAbilityPhase
                            || v[i].FindSpell("legion_commander_duel")?.Cooldown <= 0
                            || v[i].FindSpell("doom_bringer_doom").IsInAbilityPhase
                            || v[i].HasModifier("modifier_faceless_void_chronosphere_freeze") && v[i].ClassId != ClassId.CDOTA_Unit_Hero_FacelessVoid
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
                            || v[i].IsStunned()
                            || v[i].IsHexed()
                            || v[i].IsRooted()
                        )
                        && (v[i].FindItem("item_manta")?.Cooldown > 0
                            || v[i].FindItem("item_manta") == null)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        E.UseAbility(v[i]);
                        Utils.Sleep(250, "combosleep");
                    }

                    if (R != null && R.CanBeCasted() && me.Distance2D(v[i]) <= R.GetCastRange() + 100
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
                        || v[i].FindSpell("rattletrap_power_cogs").IsInAbilityPhase
                            || v[i].ClassId != ClassId.CDOTA_Unit_Hero_FacelessVoid
                            && !v[i].HasModifier("modifier_faceless_void_chronosphere_freeze")
                            || v[i].FindSpell("witch_doctor_death_ward").IsInAbilityPhase
                            || v[i].FindSpell("crystal_maiden_crystal_nova").IsInAbilityPhase
                            || v[i].IsStunned())
                        && (v[i].FindItem("item_cyclone")?.Cooldown > 0
                            || v[i].FindItem("item_cyclone") == null || v[i].IsStunned() || v[i].IsHexed() || v[i].IsRooted())
                        && (v[i].FindItem("item_force_staff")?.Cooldown > 0
                            || v[i].FindItem("item_force_staff") == null || v[i].IsStunned() || v[i].IsHexed() || v[i].IsRooted())
                        && !v[i].HasModifiers(new[]
                        {
                            "modifier_medusa_stone_gaze_stone",
                            "modifier_faceless_void_time_walk",
                            "modifier_item_monkey_king_bar",
                            "modifier_rattletrap_battery_assault",
                            "modifier_item_blade_mail_reflect",
                            "modifier_skywrath_mystic_flare_aura_effect",
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
                        R.UseAbility(Prediction.InFront(v[i], 70));
                        Utils.Sleep(250, "combosleep");
                    }
                    /*
                    if (E != null && E.CanBeCasted() && me.Distance2D(v[i]) <= E.GetCastRange()

                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(E.Name)
                        && !v[i].IsLinkensProtected()
                        &&
                        (
                            v[i].HasModifier("modifier_meepo_earthbind")
                         || v[i].HasModifier("modifier_pudge_dismember")
                         || v[i].HasModifier("modifier_naga_siren_ensnare")
                         || v[i].HasModifier("modifier_lone_druid_spirit_bear_entangle_effect")
                         || v[i].HasModifier("modifier_legion_commander_duel")
                         || v[i].HasModifier("modifier_kunkka_torrent")
                         || v[i].HasModifier("modifier_ice_blast")
                         || v[i].HasModifier("modifier_enigma_black_hole_pull")
                         || v[i].HasModifier("modifier_ember_spirit_searing_chains")
                         || v[i].HasModifier("modifier_dark_troll_warlord_ensnare")
                         || v[i].HasModifier("modifier_crystal_maiden_crystal_nova")
                         || v[i].HasModifier("modifier_axe_berserkers_call")
                         || v[i].HasModifier("modifier_bane_fiends_grip")
                         || v[i].HasModifier("modifier_rubick_telekinesis")
                         || v[i].HasModifier("modifier_storm_spirit_electric_vortex_pull")
                         || v[i].HasModifier("modifier_winter_wyvern_cold_embrace")
                         || v[i].HasModifier("modifier_shadow_shaman_shackles")
                         || v[i].FindSpell("magnataur_reverse_polarity").IsInAbilityPhase
                         || v[i].FindItem("item_blink")?.Cooldown >= 11
                         || v[i].FindSpell("queenofpain_blink").IsInAbilityPhase
                         || v[i].FindSpell("antimage_blink").IsInAbilityPhase
                         || v[i].FindSpell("antimage_mana_void").IsInAbilityPhase
                         || v[i].FindSpell("legion_commander_duel")?.Cooldown <= 0
                         || v[i].FindSpell("doom_bringer_doom").IsInAbilityPhase
                         || v[i].HasModifier("modifier_faceless_void_chronosphere_freeze")
                             && v[i].ClassId != ClassId.CDOTA_Unit_Hero_FacelessVoid
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
                         ||  v[i].FindSpell("life_stealer_rage")?.Cooldown <= 0
                         && me.Level >= 7
                         || v[i].IsStunned() 
                         || v[i].IsHexed() 
                         || v[i].IsRooted()
                        )
                        && (v[i].FindItem("item_manta")?.Cooldown > 0
                            || v[i].FindItem("item_manta") == null)
                        && !v[i].IsMagicImmune()
                        && !v[i].HasModifier("modifier_medusa_stone_gaze_stone")
                        && Utils.SleepCheck(v[i].Handle.ToString())
                    )
                    {
                        E.UseAbility(v[i]);
                        Utils.Sleep(250, v[i].Handle.ToString());
                    }*/
                    
                    if (R != null && R.CanBeCasted() && me.Distance2D(v[i]) <= R.GetCastRange() + 100
                        && !me.HasModifier("modifier_pugna_nether_ward_aura")
                        && v[i].MovementSpeed <= 240
                        && v[i].MagicDamageResist <= 0.1
                        && !v[i].HasModifiers(new[]
                        {
                            "modifier_zuus_lightningbolt_vision_thinker",
                            "modifier_item_blade_mail_reflect",
                            "modifier_sniper_headshot",
                            "modifier_leshrac_lightning_storm_slow",
                            "modifier_razor_unstablecurrent_slow",
                            "modifier_pudge_meat_hook",
                            "modifier_tusk_snowball_movement",
                            "modifier_faceless_void_time_walk",
                            "modifier_obsidian_destroyer_astral_imprisonment_prison",
                            "modifier_puck_phase_shift",
                            "modifier_abaddon_borrowed_time",
                            "modifier_winter_wyvern_winters_curse",
                            "modifier_eul_cyclone",
                            "modifier_dazzle_shallow_grave",
                            "modifier_brewmaster_storm_cyclone",
                            "modifier_mirana_leap",
                            "modifier_earth_spirit_rolling_boulder_caster",
                            "modifier_spirit_breaker_charge_of_darkness",
                            "modifier_shadow_demon_disruption"
                        }, false)
                        && (v[i].FindSpell("abaddon_borrowed_time")?.Cooldown > 0
                            || v[i].FindSpell("abaddon_borrowed_time") == null)
                        && (v[i].FindItem("item_cyclone")?.Cooldown > 0
                            || v[i].FindItem("item_cyclone") == null)
                        && (v[i].FindItem("item_force_staff")?.Cooldown > 0
                            || v[i].FindItem("item_force_staff") == null )
                        && v[i].Health >= v[i].MaximumHealth / 100 * Menu.Item("Healh").GetValue<Slider>().Value
                        && !v[i].IsMagicImmune()
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(R.Name)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        R.UseAbility(Prediction.InFront(v[i], 90));
                        Utils.Sleep(250, "combosleep");
                    }
                    
                    if (R != null && R.CanBeCasted() && me.Distance2D(v[i]) <= R.GetCastRange() + 100
                        && !me.HasModifier("modifier_pugna_nether_ward_aura")
                        && v[i].MovementSpeed <= 240
                        && !E.CanBeCasted()
                        && v[i].Health >= v[i].MaximumHealth / 100 * Menu.Item("Healh").GetValue<Slider>().Value
                        && !v[i].HasModifiers(new[]
                        {
                            "modifier_item_blade_mail_reflect",
                            "modifier_zuus_lightningbolt_vision_thinker",
                            "modifier_obsidian_destroyer_astral_imprisonment_prison",
                            "modifier_puck_phase_shift",
                            "modifier_eul_cyclone",
                            "modifier_invoker_tornado",
                            "modifier_dazzle_shallow_grave",
                            "modifier_brewmaster_storm_cyclone",
                            "modifier_spirit_breaker_charge_of_darkness",
                            "modifier_shadow_demon_disruption",
                            "modifier_faceless_void_time_walk",
                            "modifier_winter_wyvern_winters_curse",
                            "modifier_huskar_life_break_charge",
                            "modifier_mirana_leap",
                            "modifier_earth_spirit_rolling_boulder_caster",
                            "modifier_tusk_snowball_movement",
                            "modifier_abaddon_borrowed_time"
                        }, false)
                        && (v[i].FindSpell("abaddon_borrowed_time")?.Cooldown > 0
                            || v[i].FindSpell("abaddon_borrowed_time") == null)
                        && (v[i].FindItem("item_cyclone")?.Cooldown > 0
                            || v[i].FindItem("item_cyclone") == null)
                        && (v[i].FindItem("item_force_staff")?.Cooldown > 0
                            || v[i].FindItem("item_force_staff") == null)
                        && Menu.Item("AutoAbility").GetValue<AbilityToggler>().IsEnabled(R.Name)
                        && Utils.SleepCheck("combosleep")
                    )
                    {
                        R.UseAbility(Prediction.InFront(v[i], 90));
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
                        else if (orchid != null && orchid.CanBeCasted() && me.Distance2D(v[i]) < orchid.GetCastRange() &&
                                 Menu.Item("Link").GetValue<AbilityToggler>().IsEnabled(orchid.Name))
                            orchid.UseAbility(v[i]);
                        Utils.Sleep(250, "combosleep");
                    }
                    
            }
        } // SkywrathMage class
    }
}