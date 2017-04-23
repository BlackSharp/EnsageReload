using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace DagonReload
{
    internal class Program
    {
        private static Hero _me, _target;

        private static Item _dagon;

        private static bool _loaded;

        private static readonly Menu Menu = new Menu("DagonReload", "DagonReload", true);

        private static readonly string[] IgnoreModifiers = {
            "modifier_templar_assassin_refraction_absorb",
            "modifier_item_blade_mail_reflect",
            "modifier_item_lotus_orb_active",
            "modifier_nyx_assassin_spiked_carapace",
            "modifier_medusa_stone_gaze_stone",
            "modifier_winter_wyvern_winters_curse"
        };

        private static void Main()
        {
            Menu.AddItem(new MenuItem("toggle", "Toggle button").SetValue(new KeyBind('F', KeyBindType.Toggle, true)));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Events.OnLoad += OnLoad;
            Events.OnClose += OnClose;
        }

        private static void OnClose(object sender, EventArgs e)
        {
            _loaded = false;
            _me = null;
            _target = null;
        }

        private static void OnLoad(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                _me = ObjectManager.LocalHero;

                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
        }

            if (_me == null || !_me.IsValid)
                _loaded = false;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsChatOpen || Game.IsWatchingGame || !Utils.SleepCheck("updaterate"))
                return;

            if (_me.IsChanneling() || _me.IsInvisible() || !Menu.Item("toggle").GetValue<KeyBind>().Active) return;
            _dagon = _me.GetDagon();
            _target = ObjectManager.GetEntitiesParallel<Hero>().FirstOrDefault(CheckTarget);

            if (_dagon == null || _target == null || !_me.CanUseItems() || !_dagon.CanBeCasted()) return;
            _dagon.UseAbility(_target);
            Utils.Sleep(100, "updaterate");
        }

        private static bool CheckTarget(Unit enemy)
        {
            if (enemy == null || enemy.IsIllusion || !enemy.IsValidTarget(_dagon.GetCastRange(), true, _me.NetworkPosition) || enemy.IsLinkensProtected() || enemy.IsMagicImmune() || !enemy.CanDie() || enemy.Modifiers.Any(x => IgnoreModifiers.Any(x.Name.Equals)))
                return false;

           return enemy.Health <
                   enemy.SpellDamageTaken(_dagon.GetAbilityData("damage"), DamageType.Magical, _me, _dagon.Name);
        }
    }
}