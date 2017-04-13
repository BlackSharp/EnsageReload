﻿using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;

namespace SkyWrathReload
{
    internal class Variables
    {
        public const string AssemblyName = "SkyWrathReload";

        public static string heroName;

        public static string[] modifiersNames =
        {
            "modifier_medusa_stone_gaze_stone",
            "modifier_winter_wyvern_winters_curse",
            "modifier_item_lotus_orb_active"
        };

        public static Dictionary<string, Ability> Abilities;

        public static Dictionary<string, bool> abilitiesDictionary = new Dictionary<string, bool>
        {
            {"skywrath_mage_arcane_bolt", true},
            {"skywrath_mage_concussive_shot", true},
            {"skywrath_mage_ancient_seal", true},
            {"skywrath_mage_mystic_flare", true}
        };

        public static Dictionary<string, bool> popLinkensDictionary = new Dictionary<string, bool>
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

        public static Dictionary<string, bool> magicItemsDictionary = new Dictionary<string, bool>
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

        public static Menu magicItems;

        public static Menu popLinkensItems;

        public static Menu abilities;

        public static Menu noCastUlti;

        public static Menu ezkillmenu;

        public static MenuItem comboKey;

        public static MenuItem harassKey;

        public static MenuItem drawTarget;

        public static MenuItem moveMode;

        public static MenuItem ezKillCheck;

        public static MenuItem ezKillStyle;

        public static MenuItem straightTimeCheck;

        public static MenuItem ClosestToMouseRange;

        public static MenuItem predictionType;

        public static MenuItem soulRing;

        public static MenuItem bladeMail;

        public static MenuItem useBlink;

        public static MenuItem nocastulti;

        public static bool loaded, ezKill;

        public static Ability bolt, slow, silence, mysticflare;

        public static Item soulring, force_staff, cyclone, orchid, sheep, veil, shivas, dagon, atos, ethereal, medal, bloodthorn, blink;

        public static Hero me, target;

        public static Vector2 iconSize, screenPosition;

        public static Vector3 predictXYZ;

        public static DotaTexture heroIcon, ezkillIcon;

        public static ParticleEffect circle;
    }
}