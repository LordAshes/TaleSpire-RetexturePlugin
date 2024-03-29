﻿using BepInEx;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LordAshes
{
    public static class Utility
    {
        public static void Initialize(System.Reflection.MemberInfo plugin)
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                try
                {
                    if (scene.name == "UI")
                    {
                        TextMeshProUGUI betaText = GetUITextByName("BETA");
                        if (betaText)
                        {
                            betaText.text = "INJECTED BUILD - unstable mods";
                        }
                    }
                    else
                    {
                        TextMeshProUGUI modListText = GetUITextByName("TextMeshPro Text");
                        if (modListText)
                        {
                            BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(plugin, typeof(BepInPlugin));
                            if (modListText.text.EndsWith("</size>"))
                            {
                                modListText.text += "\n\nMods Currently Installed:\n";
                            }
                            modListText.text += "\nLord Ashes' " + bepInPlugin.Name + " - " + bepInPlugin.Version;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            };
        }

        public static TextMeshProUGUI GetUITextByName(string name)
        {
            TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name == name)
                {
                    return texts[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Method to obtain the Base Loader Game Object based on a CreatureGuid
        /// </summary>
        /// <param name="cid">Creature Guid</param>
        /// <returns>BaseLoader Game Object</returns>
        public static GameObject GetBaseLoader(CreatureGuid cid)
        {
            CreatureBoardAsset asset = null;
            CreaturePresenter.TryGetAsset(cid, out asset);
            if (asset != null)
            {
                Transform _base = null;
                StartWith(asset, "_base", ref _base);
                Transform baseLoader = null;
                Traverse(_base, "BaseLoader", ref baseLoader);
                if (baseLoader != null)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension:  Base Loader '" + baseLoader.GetChild(0).name + "' Found");
                    return baseLoader.GetChild(0).gameObject;
                }
                else
                {
                    Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Could Not Find Base Loader");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Method to obtain the Asset Loader Game Object based on a CreatureGuid
        /// </summary>
        /// <param name="cid">Creature Guid</param>
        /// <returns>AssetLoader Game Object</returns>
        public static GameObject GetAssetLoader(CreatureGuid cid)
        {
            CreatureBoardAsset asset = null;
            CreaturePresenter.TryGetAsset(cid, out asset);
            if (asset != null)
            {
                Transform _creatureRoot = null;
                StartWith(asset, "_creatureRoot", ref _creatureRoot);
                Transform assetLoader = null;
                Traverse(_creatureRoot, "AssetLoader", ref assetLoader);
                if (assetLoader != null)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension:  Asset Loader '" + assetLoader.GetChild(0).name + "' Found");
                    return assetLoader.GetChild(0).gameObject;
                }
                else
                {
                    Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Could Not Find Asset Loader");
                    return null;
                }
            }
            return null;
        }

        public static void StartWith(CreatureBoardAsset asset, string seek, ref Transform match)
        {
            Type type = typeof(CreatureBoardAsset);
            match = null;
            foreach (FieldInfo fi in type.GetRuntimeFields())
            {
                if (fi.Name == seek) { match = (Transform)fi.GetValue(asset); break; }
            }
        }

        public static void Traverse(Transform root, string seek, ref Transform match)
        {
            // Debug.Log("Seeking Child Named '" + seek + "'. Found '" + root.name + "'");
            if (match != null) { return; }
            if (root.name == seek) { match = root; return; }
            foreach (Transform child in root.Children())
            {
                Traverse(child, seek, ref match);
            }
        }
    }
}
