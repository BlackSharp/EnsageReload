using System;
using System.Linq;
using System.Security.Permissions;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct3D9;

[PermissionSet(SecurityAction.Assert, Unrestricted = true)]
internal class Program
{
    private static bool activated;
    private static Font txt;
    private static Font not;

    private static readonly Vector3[] mid =
    {
        new Vector3(-5565, -5039, 384),
        new Vector3(-4436, -3942, 384),
        new Vector3(-3342, -2809, 256),
        new Vector3(-2265, -1869, 255),
        new Vector3(-1083, -887, 256),
        new Vector3(94, 136, 256),
        new Vector3(1398, 1045, 256),
        new Vector3(2522, 2054, 256),
        new Vector3(3509, 3139, 256),
        new Vector3(4663, 4079, 384),
        new Vector3(5233, 4711, 384)
    };

    private static readonly Vector3[] bot =
    {
        new Vector3(-4732, -6082, 384),
        new Vector3(-3248, -6018, 256),
        new Vector3(-1656, -6057, 256),
        new Vector3(-157, -6114, 384),
        new Vector3(1360, -6270, 384),
        new Vector3(2901, -6142, 384),
        new Vector3(4398, -5912, 384),
        new Vector3(5626, -4999, 384),
        new Vector3(5968, -3497, 384),
        new Vector3(6183, -2020, 384),
        new Vector3(6329, -486, 384),
        new Vector3(6232, 1032, 379),
        new Vector3(6295, 2602, 384),
        new Vector3(6368, 3812, 384)
    };

    private static readonly Vector3[] top =
    {
        new Vector3(-6641, -4192, 384),
        new Vector3(-6612, -2662, 256),
        new Vector3(-6443, -1187, 384),
        new Vector3(-6353, 288, 384),
        new Vector3(-6254, 1789, 384),
        new Vector3(-6247, 3348, 384),
        new Vector3(-6203, 4851, 384),
        new Vector3(-4948, 5695, 384),
        new Vector3(-3455, 5748, 384),
        new Vector3(-1975, 6056, 384),
        new Vector3(-466, 6005, 383),
        new Vector3(1039, 5867, 384),
        new Vector3(2555, 5812, 255),
        new Vector3(4084, 5802, 384)
    };

    private static readonly Menu Menu = new Menu("All Unit's Push", "AllUnit's Push", true);

    private static void Main()
    {
        Menu.AddItem(new MenuItem("Push key", "Togle key Push").SetValue(new KeyBind('K', KeyBindType.Toggle)));
        Menu.AddToMainMenu();


        Game.OnUpdate += Game_OnUpdate;
        Console.WriteLine("> Unit by Vick.Rework!");

        txt = new Font(
            Drawing.Direct3DDevice9,
            new FontDescription
            {
                FaceName = "Tahoma",
                Height = 11,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Default
            });

        not = new Font(
            Drawing.Direct3DDevice9,
            new FontDescription
            {
                FaceName = "Tahoma",
                Height = 12,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Default
            });

        Drawing.OnPreReset += Drawing_OnPreReset;
        Drawing.OnPostReset += Drawing_OnPostReset;
        Drawing.OnEndScene += Drawing_OnEndScene;
        AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
    }


    private static void Game_OnUpdate(EventArgs args)
    {
        var me = ObjectManager.LocalHero;
        if (!Game.IsInGame || me == null || Game.IsWatchingGame)
            return;
        activated = Menu.Item("Push key").GetValue<KeyBind>().Active;

        if (!activated || Game.IsPaused) return;
        var unit = ObjectManager.GetEntities<Unit>()
            .Where(creep =>
                (creep.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral
                 || creep.ClassId == ClassId.CDOTA_BaseNPC_Additive
                 || creep.ClassId == ClassId.CDOTA_BaseNPC_Invoker_Forged_Spirit
                 || creep.ClassId == ClassId.CDOTA_BaseNPC_Warlock_Golem
                 || creep.ClassId == ClassId.CDOTA_BaseNPC_Creep
                 || creep.ClassId == ClassId.CDOTA_Unit_Hero_Beastmaster_Boar
                 || creep.ClassId == ClassId.CDOTA_Unit_SpiritBear
                 || creep.ClassId == ClassId.CDOTA_Unit_Broodmother_Spiderling
                )
                && creep.IsAlive
                && creep.NetworkActivity != NetworkActivity.Move
                && creep.Team == me.Team
                && creep.IsControllable
                && creep.IsValid)
            .ToList();
        if (unit.Count == 0) return;

        var fount = ObjectManager.GetEntities<Unit>()
            .FirstOrDefault(x => x.Team == me.Team && x.ClassId == ClassId.CDOTA_Unit_Fountain);
        var tower = ObjectManager.GetEntities<Unit>()
            .Where(x => x.Team != me.Team && x.ClassId == ClassId.CDOTA_BaseNPC_Tower)
            .ToList();

        for (var i = 0; i < unit.Count(); ++i)
        {
            var Mid = GetClosestToVector(mid, unit[i]);
            var Top = GetClosestToVector(top, unit[i]);
            var Bot = GetClosestToVector(bot, unit[i]);
            if (me.Distance2D(unit[i]) <= 600) return;
            var v =
                ObjectManager.GetEntities<Hero>()
                    .Where(x => x.Team != me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion)
                    .ToList();

            if (Mid.Distance2D(unit[i]) <= 1700)
                for (var x = 0; x < mid.Count(); ++x)
                {
                    var b = mid[x];
                    if (!(unit[i].Distance2D(fount) + 170 < b.Distance2D(fount)) || !(unit[i].Distance2D(b) <= 1700) ||
                        !Utils.SleepCheck(unit[i].Handle.ToString())) continue;
                    unit[i].Attack(b);
                    Utils.Sleep(400, unit[i].Handle.ToString());
                }
            if (Bot.Distance2D(unit[i]) <= 1700)
                for (var x = 0; x < bot.Count(); ++x)
                {
                    var b = bot[x];
                    if (!(unit[i].Distance2D(fount) + 170 < b.Distance2D(fount)) || !(unit[i].Distance2D(b) <= 1700) ||
                        !Utils.SleepCheck(unit[i].Handle.ToString())) continue;
                    unit[i].Attack(b);
                    Utils.Sleep(400, unit[i].Handle.ToString());
                }
            if (!(Top.Distance2D(unit[i]) <= 1700)) continue;
            {
                for (var x = 0; x < top.Count(); ++x)
                {
                    var b = top[x];
                    if (!(unit[i].Distance2D(fount) + 170 < b.Distance2D(fount)) || !(unit[i].Distance2D(b) <= 1700) ||
                        !Utils.SleepCheck(unit[i].Handle.ToString())) continue;
                    unit[i].Attack(b);
                    Utils.Sleep(400, unit[i].Handle.ToString());
                }
            }
        }
    }


    private static Vector3 GetClosestToVector(Vector3[] coords, Unit z)
    {
        var closestVector = coords.First();
        foreach (var v in coords.Where(v => closestVector.Distance2D(z) > v.Distance2D(z)))
            closestVector = v;
        return closestVector;
    }

    private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
    {
        txt.Dispose();
        not.Dispose();
    }

    private static void Drawing_OnEndScene(EventArgs args)
    {
        if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
            return;

        var me = ObjectManager.LocalHero;
        if (me == null)
            return;

        if (activated)
            txt.DrawText(null, "Unit Push Active", 1200, 37, Color.Coral);

        if (!activated)
            txt.DrawText(null, "Unit Push UnActive", 1200, 37, Color.DarkRed);
    }


    private static void Drawing_OnPostReset(EventArgs args)
    {
        txt.OnResetDevice();
        not.OnResetDevice();
    }

    private static void Drawing_OnPreReset(EventArgs args)
    {
        txt.OnLostDevice();
        not.OnLostDevice();
    }
}