namespace DotaAllCombo.Heroes
{
    using System.Collections.Generic;
    using System.Linq;
    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Service;
    using Service.Debug;

    internal class ShredderController : Variables, IHeroController
    {
        private Ability Q, W, R, D, F;
        public void Combo()
        {
            me = ObjectManager.LocalHero;

            if (!Menu.Item("enabled").IsActive()) return;
            Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);
            e = Toolset.ClosestToMouse(me);
            if (e == null) return;

            Q = me.Spellbook.SpellQ;
            W = me.Spellbook.SpellW; 
            R = me.FindSpell("shredder_chakram");
            D = me.FindSpell("shredder_chakram_2");
            F = me.FindSpell("shredder_return_chakram");
            /*Shiva = me.FindItem("item_shivas_guard");
            mom = me.FindItem("item_mask_of_madness");
            diff = me.FindItem("item_diffusal_blade") ?? me.FindItem("item_diffusal_blade_2");
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

            var stoneModif = e.HasModifier("modifier_medusa_stone_gaze_stone");

            var v =
                ObjectManager.GetEntities<Hero>()
                    .Where(x => x.Team != me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune())
                    .ToList();
                    */

            e = Toolset.ClosestToMouse(me);
            if (e == null) return;
            if (
                Q != null
                && Q.CanBeCasted()
                && me.Distance2D(e) <= Q.GetCastRange() + me.HullRadius
                && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
                && Utils.SleepCheck("Q")
                )
            {
                Q.UseAbility();
                Utils.Sleep(200, "Q");
            }
            var thinker =
            ObjectManager.GetEntities<Unit>().Where(x => x.Name == "npc_dota_thinker" && x.HasModifier("modifier_shredder_chakram_thinker")  && x.Team == me.Team).ToList();
            if (thinker.Count > 0)
            {
                var chakram = Toolset.GetClosestToUnit(thinker, e);
                if (chakram.Distance2D(e) >= 180)
                {
                    F.UseAbility();
                }
            }
            if (Active && e.IsAlive)
            {
                if (Menu.Item("orbwalk").GetValue<bool>() && me.Distance2D(e) <= 1900)
                {
                    Orbwalking.Orbwalk(e, 0, 1600, true, true);
                }
                if (R!=null && R.CanBeCasted() &&  me.Distance2D(e) <= R.GetCastRange())
                {
                    R.UseAbility(Game.MousePosition);
                }
                if (D != null && D.CanBeCasted() && me.Distance2D(e) <= D.GetCastRange())
                {
                    D.UseAbility(Game.MousePosition);
                }

            }


        } // Combo

        public void OnLoadEvent()
        {
            AssemblyExtensions.InitAssembly("TODO", "0");
            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
            Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

            Menu.AddItem(
                new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
                {
                    {"shredder_whirling_death", true},
                    {"shredder_timber_chain", true},
                    {"shredder_chakram", true},
                    {"shredder_chakram_2", true}
                })));
            Menu.AddItem(
                new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
                {
                   /* {"item_blink", true},
                    {"item_diffusal_blade", true},
                    {"item_diffusal_blade_2", true},
                    {"item_orchid", true},
                    {"item_bloodthorn", true},
                    {"item_urn_of_shadows", true},
                    {"item_abyssal_blade", true},
                    {"item_shivas_guard", true},
                    {"item_blade_mail", true},
                    {"item_black_king_bar", true},
                    {"item_satanic", true},
                    {"item_medallion_of_courage", true},
                    {"item_solar_crest", true}*/
                })));
            Menu.AddItem(new MenuItem("Heel", "Min targets to BKB").SetValue(new Slider(2, 1, 5)));
            Print.LogMessage.Error("This hero not Supported!");
        }
       
        public void OnCloseEvent()
        {

        }
    }
}