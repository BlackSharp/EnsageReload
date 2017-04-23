using System;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using SharpDX;

namespace SkyWrathReload
{
    internal class SkyWrathReload : Variables
    {
        public static void Init()
        {
            Options.MenuInit();

            Events.OnLoad += OnLoad;
            Events.OnClose += OnClose;
        }

        private static void OnClose(object sender, EventArgs e)
        {
            Game.OnUpdate -= ComboUsage;
            Drawing.OnDraw -= TargetIndicator;
            Loaded = false;
            Me = null;
            Target = null;
        }

        private static void OnLoad(object sender, EventArgs e)
        {
            if (!Loaded)
            {
                Me = ObjectManager.LocalHero;
                if (!Game.IsInGame || Me == null || Me.Name != HeroName)
                {
                    return;
                }
                Loaded = true;
                GetAbilities();
                Game.OnUpdate += ComboUsage;
                Drawing.OnDraw += TargetIndicator;
            }

            if (Me == null || !Me.IsValid)
                Loaded = false;
        }

        private static void ComboUsage(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame || Game.IsChatOpen)
                return;

            Target = Me.ClosestToMouseTarget(ClosestToMouseRange.GetValue<Slider>().Value);
            if (Game.IsKeyDown(ComboKey.GetValue<KeyBind>().Key))
            {
                GetAbilities();

                if (Target == null || !Target.IsValid || !Target.IsVisible || Target.IsIllusion || !Target.IsAlive ||
                    Me.IsChanneling() || Target.IsInvul() || HasModifiers()) return;

                if (Target.IsLinkensProtected())
                {
                    PopLinkens(Cyclone);
                    PopLinkens(ForceStaff);
                    PopLinkens(Atos);
                    PopLinkens(Sheep);
                    PopLinkens(Orchid);
                    PopLinkens(Dagon);
                    PopLinkens(Silence);
                }
                else
                {
                    if (!Utils.SleepCheck("combosleep")) return;

                    Orbwalk();

                    if (Target.Distance2D(Me.Position) < 700)
                    {
                        if (Utils.SleepCheck("ezkill"))
                        {
                            EzKill = IsEzKillable();
                            Utils.Sleep(5000, "ezkill");
                        }
                    }

                    if (Soulring != null && Soulring.CanBeCasted() && SoulRing.GetValue<bool>())
                        Soulring.UseAbility();

                    if (!Target.UnitState.HasFlag(UnitState.Hexed) && !Target.UnitState.HasFlag(UnitState.Stunned))
                        UseItem(Sheep, Sheep.GetCastRange());

                    UseBlink();
                    CastAbility(Silence, Silence.GetCastRange());
                    CastAbility(Bolt, Bolt.GetCastRange());
                    CastAbility(Slow, Slow.GetCastRange());

                    UseItem(Atos, Atos.GetCastRange(), 140);
                    UseItem(Medal, Medal.GetCastRange());
                    UseItem(Orchid, Orchid.GetCastRange());
                    UseItem(Bloodthorn, Bloodthorn.GetCastRange());
                    UseItem(Veil, Veil.GetCastRange());
                    UseItem(Ethereal, Ethereal.GetCastRange());

                    UseDagon();

                    CastUltimate();

                    UseItem(Shivas, Shivas.GetCastRange());

                    Utils.Sleep(150, "combosleep");
                }
            }

            if (Game.IsKeyDown(HarassKey.GetValue<KeyBind>().Key))
            {
                GetAbilities();
                if (Target == null || !Target.IsValid || !Target.IsVisible || Target.IsIllusion || !Target.IsAlive ||
                    Me.IsChanneling() || Target.IsInvul() || HasModifiers()) return;
                if (!Utils.SleepCheck("harasssleep")) return;
                Orbwalk();
                CastAbility(Bolt, Bolt.GetCastRange());
                Utils.Sleep(150, "harasssleep");
            }
        }

        private static void GetAbilities()
        {
            if (!Utils.SleepCheck("GetAbilities")) return;
            Blink = Me.FindItem("item_blink");
            Soulring = Me.FindItem("item_soul_ring");
            Medal = Me.FindItem("item_medallion_of_courage");
            Bloodthorn = Me.FindItem("item_bloodthorn");
            ForceStaff = Me.FindItem("item_force_staff");
            Cyclone = Me.FindItem("item_cyclone");
            Orchid = Me.FindItem("item_orchid");
            Sheep = Me.FindItem("item_sheepstick");
            Veil = Me.FindItem("item_veil_of_discord");
            Shivas = Me.FindItem("item_shivas_guard");
            Dagon = Me.GetDagon();
            Atos = Me.FindItem("item_rod_of_atos");
            Ethereal = Me.FindItem("item_ethereal_blade");
            Bolt = Me.FindSpell("skywrath_mage_arcane_bolt");
            Slow = Me.FindSpell("skywrath_mage_concussive_shot");
            Silence = Me.FindSpell("skywrath_mage_ancient_seal");
            Mysticflare = Me.FindSpell("skywrath_mage_mystic_flare");
            Utils.Sleep(1000, "GetAbilities");
        }

        private static bool HasModifiers()
        {
            if (Target.HasModifiers(ModifiersNames, false) ||
                (BladeMail.GetValue<bool>() && Target.HasModifier("modifier_item_blade_mail_reflect")) ||
                !Utils.SleepCheck("HasModifiers"))
                return true;
            Utils.Sleep(100, "HasModifiers");
            return false;
        }

        private static void TargetIndicator(EventArgs args)
        {
            if (!drawTarget.GetValue<bool>())
            {
                if (Circle == null) return;
                Circle.Dispose();
                Circle = null;
                return;
            }
            if (Target != null && Target.IsValid && !Target.IsIllusion && Target.IsAlive && Target.IsVisible &&
                Me.IsAlive)
            {
                DrawTarget();
                DrawEzKill();
            }
            else if (Circle != null)
            {
                Circle.Dispose();
                Circle = null;
            }
        }

        private static void DrawTarget()
        {
            HeroIcon = Drawing.GetTexture("materials/ensage_ui/miniheroes/skywrath_mage");
            IconSize = new Vector2(HUDInfo.GetHpBarSizeY()*2);

            if (
                !Drawing.WorldToScreen(Target.Position + new Vector3(0, 0, Target.HealthBarOffset/3f), out ScreenPosition))
                return;

            ScreenPosition += new Vector2(-IconSize.X, 0);
            Drawing.DrawRect(ScreenPosition, IconSize, HeroIcon);

            if (Circle == null)
            {
                Circle = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", Target);
                Circle.SetControlPoint(2, Me.Position);
                Circle.SetControlPoint(6, new Vector3(1, 0, 0));
                Circle.SetControlPoint(7, Target.Position);
            }
            else
            {
                Circle.SetControlPoint(2, Me.Position);
                Circle.SetControlPoint(6, new Vector3(1, 0, 0));
                Circle.SetControlPoint(7, Target.Position);
            }
        }

        private static void DrawEzKill()
        {
            if (!Menu.Item("ezKillCheck").GetValue<bool>() || Game.IsWatchingGame || Game.IsChatOpen) return;
            switch (EzKillStyle.GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    var icoSize = new Vector2((HUDInfo.GetHpBarSizeY()*2));
                    var icoPos = HUDInfo.GetHPbarPosition(Target) - new Vector2(21, 5);
                    switch (IsEzKillable())
                    {
                        case true:
                            EzkillIcon = Drawing.GetTexture("materials/ensage_ui/emoticons/bc_emoticon_fire");
                            break;
                        case false:
                            EzkillIcon = null;
                            break;
                    }
                    Drawing.DrawRect(icoPos, icoSize, EzkillIcon);
                    break;

                case 1:
                    var pos = HUDInfo.GetHPbarPosition(Target);
                    var size = HUDInfo.GetHpBarSizeY() + 3;
                    var text = string.Empty;
                    var color = new Color();
                    var fontFlags = FontFlags.AntiAlias | FontFlags.Additive;
                    if (Game.IsKeyDown(ComboKey.GetValue<KeyBind>().Key))
                    {
                        pos = pos - new Vector2(58, 0);
                        text = "CASTING...";
                        color = Color.Crimson;
                    }
                    else
                    {
                        switch (IsEzKillable())
                        {
                            case true:
                                pos = pos - new Vector2(40, 0);
                                text = "EZKILL";
                                color = Color.Chartreuse;
                                break;
                            case false:
                                pos = pos - new Vector2(63, 0);
                                text = "NON-EZKILL";
                                color = Color.White;
                                break;
                        }
                    }
                    Drawing.DrawText(text, pos, new Vector2(size), color, fontFlags);
                    break;
            }
        }

        private static void CastAbility(Ability ability, float range)
        {
            if (ability == null || !ability.CanBeCasted() || ability.IsInAbilityPhase || Target.IsMagicImmune() ||
                !Target.IsValidTarget(range, true, Me.NetworkPosition) ||
                !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(ability.Name)) return;

            if (ability.IsAbilityBehavior(AbilityBehavior.UnitTarget))
            {
                ability.UseAbility(Target);
                return;
            }
            if (ability.IsAbilityBehavior(AbilityBehavior.NoTarget))
            {
                if (Equals(ability, Slow))
                {
                    ability.UseAbility();
                    Utils.Sleep(Me.NetworkPosition.Distance2D(Target.NetworkPosition)/800*1000, "slowsleep");
                    return;
                }
                ability.UseAbility();
            }
        }

        private static void CastUltimate()
        {
			if (Mysticflare == null
				|| !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(Mysticflare.Name)
				|| !Mysticflare.CanBeCasted()
				|| Target.IsMagicImmune()
				|| !IsFullDebuffed()
				|| EzKill
				|| Target.HasModifier("modifier_rune_haste")
				|| Target.Health * 100 / Target.MaximumHealth < Menu.Item("noCastUlti").GetValue<Slider>().Value
				|| Prediction.StraightTime(Target) / 1000 < StraightTimeCheck.GetValue<Slider>().Value)
                return;

            if (!Target.CanMove() ||
                Target.UnitState.HasFlag(UnitState.Rooted) ||
                Target.UnitState.HasFlag(UnitState.Stunned))
                Mysticflare.UseAbility(Target.NetworkPosition);
            else
                GetPrediction();
        }

        private static void GetPrediction()
        {
            switch (PredictionType.GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    if (Target.UnitState.HasFlag(UnitState.Hexed))
                    {
                        Mysticflare.UseAbility(Prediction.InFront(Target, 87));
                        break;
                    }
                    Mysticflare.UseAbility(Prediction.InFront(Target, 100));
                    break;
                case 1:
                    if (Target.UnitState.HasFlag(UnitState.Hexed))
                    {
                        Mysticflare.UseAbility(Prediction.PredictedXYZ(Target, 210/Target.MovementSpeed*1000));
                        break;
                    }
                    Mysticflare.UseAbility(Prediction.PredictedXYZ(Target, 230/Target.MovementSpeed*1000));
                    break;
            }
        }

        private static void UseDagon()
        {
            if (Dagon == null
                || !Dagon.CanBeCasted()
                || Target.IsMagicImmune()
                || !(Target.NetworkPosition.Distance2D(Me) - Target.RingRadius <= Dagon.CastRange)
                || !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled("item_dagon")
                || !IsFullDebuffed()
                || !Utils.SleepCheck("ebsleep")) return;
            Dagon.UseAbility(Target);
        }

        private static void UseItem(Item item, float range, int speed = 0)
        {
            if (item == null || !item.CanBeCasted() || Target.IsMagicImmune() || Target.MovementSpeed < speed ||
                Target.HasModifier(item.Name) || !Target.IsValidTarget(range, true, Me.NetworkPosition) ||
                !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(item.Name))
                return;

            if (item.Equals(Ethereal) && IsFullDebuffed())
            {
                item.UseAbility(Target);
                Utils.Sleep(Me.NetworkPosition.Distance2D(Target.NetworkPosition) / 1200 * 1000, "ebsleep");
                return;
            }

			if (item.Equals(Atos))
			{
				item.UseAbility(Target);
				Utils.Sleep(Me.NetworkPosition.Distance2D(Target.NetworkPosition) / 1500 * 1000, "atossleep");
				return;
			}

			if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget) && !item.Name.Contains("item_dagon"))
            {
                item.UseAbility(Target);
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.Point))
            {
                item.UseAbility(Target.NetworkPosition);
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.Immediate))
            {
                item.UseAbility();
            }
        }

        private static void PopLinkens(Ability item)
        {
            if (item == null || !item.CanBeCasted() ||
                !Menu.Item("popLinkensItems").GetValue<AbilityToggler>().IsEnabled(item.Name) ||
                !Utils.SleepCheck("PopLinkens")) return;
            item.UseAbility(Target);
            Utils.Sleep(100, "PopLinkens");
        }

        private static bool IsFullDebuffed()
        {
            if ((Atos != null && Atos.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(Atos.Name) &&
                 !Target.HasModifier("modifier_item_rod_of_atos") && !Utils.SleepCheck("atossleep"))
                ||
                (Veil != null && Veil.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(Veil.Name) &&
                 !Target.HasModifier("modifier_item_veil_of_discord"))
                ||
                (Silence != null && Silence.CanBeCasted() &&
                 Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(Silence.Name) &&
                 !Target.HasModifier("modifier_skywrath_mage_ancient_seal"))
                ||
                (Orchid != null && Orchid.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(Orchid.Name) &&
                 !Target.HasModifier("modifier_item_orchid_malevolence"))
                ||
                (Ethereal != null && Ethereal.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(Ethereal.Name) &&
                 !Target.HasModifier("modifier_item_ethereal_blade_slow") && !Utils.SleepCheck("slowsleep"))
				||
                (Bloodthorn != null && Bloodthorn.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(Bloodthorn.Name) &&
                 !Target.HasModifier("modifier_item_bloodthorn"))
                ||
                (Slow != null && Slow.CanBeCasted() &&
                 Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(Slow.Name) &&
                 !Target.HasModifier("modifier_skywrath_mage_concussive_shot_slow")) && !Utils.SleepCheck("slowsleep"))
                return false;
            return true;
        }

        private static bool IsEzKillable()
        {
            if (!Menu.Item("ezKillCheck").GetValue<bool>()) return false;
            int totalDamage = 0;
            int plusPerc = 0;
            uint reqMana = 0;

            if (Ethereal != null && Ethereal.CanBeCasted() &&
                Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(Ethereal.Name))
            {
                totalDamage +=
                    (int)
                        Target.SpellDamageTaken((int) (Me.TotalIntelligence*2) + 75, DamageType.Magical, Me,
                            Ethereal.Name);
                plusPerc += 40;
                reqMana += Ethereal.ManaCost;
            }

            if (Veil != null && Veil.CanBeCasted() &&
                Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(Veil.Name))
            {
                plusPerc += 25;
                reqMana += Veil.ManaCost;
            }


            if (Silence != null && Silence.CanBeCasted() &&
                Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(Silence.Name))
            {
                plusPerc += (int)((Silence.Level - 1) * 5 + 30);
                reqMana += Silence.ManaCost;
            }


            if (Dagon != null && Dagon.CanBeCasted()
                /*Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled("item_dagon")*/)
            {
                totalDamage +=
                    (int)
                        Target.SpellDamageTaken(Dagon.GetAbilityData("damage"), DamageType.Magical, Me, Dagon.Name,
                            minusMagicResistancePerc: plusPerc);
                reqMana += Dagon.ManaCost;
            }
                

            if (Bolt != null && Bolt.CanBeCasted() &&
                Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(Bolt.Name))
            {
                if (Bolt.Level < 4)
                {
                    totalDamage +=
                        (int)
                            Target.SpellDamageTaken((Bolt.GetAbilityData("bolt_damage") + Me.TotalIntelligence * 1.6f) * 1,
                                DamageType.Magical, Me, Bolt.Name, minusMagicResistancePerc: plusPerc);
                    reqMana += Bolt.ManaCost;
                }
                    
                else
                {
                    totalDamage +=
                        (int)
                            Target.SpellDamageTaken((Bolt.GetAbilityData("bolt_damage") + Me.TotalIntelligence * 1.6f) * 2,
                                DamageType.Magical, Me, Bolt.Name, minusMagicResistancePerc: plusPerc);
                    reqMana += Bolt.ManaCost*2;
                }
            }

            if (Slow != null && Slow.CanBeCasted() &&
                Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(Slow.Name))
            {
                totalDamage +=
                    (int)
                        Target.SpellDamageTaken(Slow.GetAbilityData("damage"), DamageType.Magical, Me, Slow.Name,
                            minusMagicResistancePerc: plusPerc);
                reqMana += Slow.ManaCost;
            }
                

            if (Me.CanAttack())
                totalDamage += (int) Target.DamageTaken(Me.DamageAverage*2, DamageType.Physical, Me);

            return reqMana < Me.Mana && Target.Health < totalDamage;
        }

        private static void Orbwalk()
        {
            switch (MoveMode.GetValue<bool>())
            {
                case true:
                    Orbwalking.Orbwalk(Target);
                    break;
                case false:
                    break;
            }
        }

        private static void UseBlink()
        {
            if (!useBlink.GetValue<bool>() || Blink == null || !Blink.CanBeCasted() ||
                Target.Distance2D(Me.Position) < 600 || !Utils.SleepCheck("blink")) return;
            PredictXyz = Target.NetworkActivity == NetworkActivity.Move
                ? Prediction.InFront(Target,
                    (float) (Target.MovementSpeed*(Game.Ping/1000 + 0.3 + Target.GetTurnTime(Target))))
                : Target.Position;

            if (Me.Position.Distance2D(PredictXyz) > 1200)
            {
                PredictXyz = (PredictXyz - Me.Position)*1200/PredictXyz.Distance2D(Me.Position) + Me.Position;
            }

            Blink.UseAbility(PredictXyz);
            Utils.Sleep(500, "blink");
        }
    }
}