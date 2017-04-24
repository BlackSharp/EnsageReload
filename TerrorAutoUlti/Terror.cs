using System;
using System.Linq;
using System.Windows.Input;
using Ensage;
using Ensage.Common;
using SharpDX;

namespace TerrorAutoUlti
{
    internal class TerrorAutoUlti
    {
        private const Key ToggleKey = Key.G;
        private static bool _active = true;
        private static Hero _me;
        private static System.Collections.Generic.List<Hero> _enemies;
        private static bool _loaded;
        private static string _toggleText;

       
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectManager.LocalHero;

                if (!Game.IsInGame || Game.IsWatchingGame || _me == null || Game.IsChatOpen)
                {
                    return;
                }

                _loaded = true;
                _toggleText = "(" + ToggleKey + ") AutoSunder: On";
            }

            if (_me == null || !_me.IsValid)
            {
                _loaded = false;
                _me = ObjectManager.LocalHero;
                _active = false;
            }

            if (Game.IsPaused) return;

            if (!Game.IsKeyDown(ToggleKey) || !Utils.SleepCheck("toggle") || Game.IsChatOpen || Game.IsPaused ||
                Game.IsWatchingGame) return;
            if (!_active)
            {
                _active = true;
                _toggleText = "(" + ToggleKey + ") AutoSunder: On";
            }
            else
            {
                _active = false;
                _toggleText = "(" + ToggleKey + ") AutoSunder: Off";
            }

            Utils.Sleep(200, "toggle");
        }

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Tick;
            PrintSuccess("> TerrorAutoUlti");
        }
        public static void Tick(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            _me = ObjectManager.LocalHero;
            if (_me == null)
                return;
            _enemies = ObjectManager.GetEntities<Hero>().Where(x => _me.Team != x.Team && !x.IsIllusion && x.IsAlive).ToList();
            if (_enemies == null)
                return;

            foreach (var v in _enemies)
            {
                if (!_active) continue;
                if (_me.Player.Hero.Level < 11  && Utils.SleepCheck("1") && _me.Player.Hero.IsAlive && _me.ClassId == ClassId.CDOTA_Unit_Hero_Terrorblade)
                {
                    if (_me.Player.Hero.Health < 200)
                    {
                        if (v.Player.Hero.Health > _me.Player.Hero.Health)
                        {
                            _me.Spellbook.SpellR.UseAbility(v.Player.Hero);
                            Utils.Sleep(1000, "1");
                        }
                    }

                }
                if (_me.Player.Hero.Level <= 11 || !Utils.SleepCheck("2") || !_me.Player.Hero.IsAlive ||
                    _me.ClassId != ClassId.CDOTA_Unit_Hero_Terrorblade) continue;
                if (_me.Player.Hero.Health >= 400) continue;
                if (v.Player.Hero.Health <= _me.Player.Hero.Health) continue;
                _me.Spellbook.SpellR.UseAbility(v.Player.Hero);
                Utils.Sleep(1000, "2");
            }
        }




        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_loaded) return;
            Drawing.DrawText("Terror!", new Vector2(Drawing.Width * 5 / 100, Drawing.Height * 19 / 100), Color.LightGreen, FontFlags.DropShadow);
            Drawing.DrawText(_toggleText,
                new Vector2(Drawing.Width * 5 / 100, Drawing.Height * 20 / 100), Color.LightGreen, FontFlags.DropShadow);
        }   


        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }
        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
    }
}