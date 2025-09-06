using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WMITF
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "SpecialAPI.WMITF";
        public const string NAME = "WMITF";
        public const string VERSION = "1.2.0";

        public void Start()
        {
            PluginFinder.Init();
            ModConfig.Init(Config);
            ConsoleCommands.Init();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }
    }
}
