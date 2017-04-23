using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;
namespace SkyWrathReload
{
    internal class Variables
    {
        public const string AssemblyName = "SkyWrathReload";

        public static string HeroName;

        public static string[] ModifiersNames =
        {
            "modifier_medusa_stone_gaze_stone",
            "modifier_winter_wyvern_winters_curse",
            "modifier_item_lotus_orb_active"
        };

        public static Dictionary<string, Ability> Abilities;

        public static Dictionary<string, bool> AbilitiesDictionary = new Dictionary<string, bool>
        {
            {"skywrath_mage_arcane_bolt", true},
            {"skywrath_mage_concussive_shot", true},
            {"skywrath_mage_ancient_seal", true},
            {"skywrath_mage_mystic_flare", true}
        };

        public static Dictionary<string, bool> PopLinkensDictionary = new Dictionary<string, bool>
        {
            {"item_medallion_of_courage", true},
            {"item_rod_of_atos", true},
            {"item_sheepstick", true},
            {"item_force_staff", true},
            {"item_cyclone", true},
            {"item_orchid", true},
            {"item_bloodthorn", true},
            {"skywrath_mage_ancient_seal", true}
        };

        public static Dictionary<string, bool> MagicItemsDictionary = new Dictionary<string, bool>
        {
            {"item_medallion_of_courage", true},
            {"item_rod_of_atos", true},
            {"item_dagon", true},
            {"item_sheepstick", true},
            {"item_orchid", true},
            {"item_bloodthorn", true},
            {"item_veil_of_discord", true},
            {"item_ethereal_blade", true},
            {"item_shivas_guard", true}
        };

        public static Menu Menu;

        public static Menu MagicItems;

        public static Menu PopLinkensItems;

        public static Menu abilities;

        public static Menu NoCastUlti;

        public static Menu Ezkillmenu;

        public static MenuItem ComboKey;

        public static MenuItem HarassKey;

        public static MenuItem drawTarget;

        public static MenuItem MoveMode;

        public static MenuItem EzKillCheck;

        public static MenuItem EzKillStyle;

        public static MenuItem StraightTimeCheck;

        public static MenuItem ClosestToMouseRange;

        public static MenuItem PredictionType;

        public static MenuItem SoulRing;

        public static MenuItem BladeMail;

        public static MenuItem useBlink;

        public static MenuItem Nocastulti;

        public static bool Loaded, EzKill;

        public static Ability Bolt, Slow, Silence, Mysticflare;

        public static Item Soulring, ForceStaff, Cyclone, Orchid, Sheep, Veil, Shivas, Dagon, Atos, Ethereal, Medal, Bloodthorn, Blink;

        public static Hero Me, Target;

        public static Vector2 IconSize, ScreenPosition;

        public static Vector3 PredictXyz;

        public static DotaTexture HeroIcon, EzkillIcon;

        public static ParticleEffect Circle;
    }
}