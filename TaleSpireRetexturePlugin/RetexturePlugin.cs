using UnityEngine;
using BepInEx;
using System.Linq;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using SRF;
using UnityEngine.Video;

namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]

    public class RetexturePlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Retexture Plug-In";
        public const string Guid = "org.lordashes.plugins.retexture";
        public const string Version = "2.2.0.0";

        // Configuration
        private ConfigEntry<KeyboardShortcut> triggerKey { get; set; }
        private ConfigEntry<KeyboardShortcut> transformKey { get; set; }

        private Dictionary<CreatureGuid, Texture> originalMaterials = new Dictionary<CreatureGuid, Texture>();

        // Content directory
        private string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Retexture Plugin: " + this.GetType().AssemblyQualifiedName + " Active.");

            triggerKey = Config.Bind("Hotkeys", "Repaint Asset Activation", new KeyboardShortcut(KeyCode.X, KeyCode.RightControl));
            transformKey = Config.Bind("Hotkeys", "Transform (Recall) Repainted", new KeyboardShortcut(KeyCode.Y, KeyCode.RightControl));

            // Subscrive to Stat Messaging requests (needed in case this is an initial board and not a board reload)
            StatMessaging.Subscribe(RetexturePlugin.Guid, RequestHandler);

            // Post plugin on the TaleSpire main page
            Utility.Initialize(this.GetType());
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (transformKey.Value.IsUp())
            {
                Debug.Log("Retexture Plugin: Resetting '" + RetexturePlugin.Guid + "' Stat Messages...");
                StatMessaging.Reset(RetexturePlugin.Guid);
                Debug.Log("Retexture Plugin: Subscribing To '" + RetexturePlugin.Guid + "' Stat Messages...");
                StatMessaging.Subscribe(RetexturePlugin.Guid, RequestHandler);
            }

            if (triggerKey.Value.IsUp())
            {
                CreatureBoardAsset asset;
                CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                if (asset != null)
                {
                    SystemMessage.AskForTextInput("Replacement Texture...", "\r\nSource:", "Texture",
                    (source) =>
                    {
                        StatMessaging.SetInfo(asset.CreatureId, RetexturePlugin.Guid, source);
                    }, null, "Original",
                    () =>
                    {
                        StatMessaging.ClearInfo(asset.CreatureId, RetexturePlugin.Guid);
                    }, "");
                }
                else
                {
                    SystemMessage.DisplayInfoText("Retexture Plugin Requires A Selected Mini");
                }
            }
        }

        /// <summary>
        /// Method to properly evaluate shortcut keys. 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool StrictKeyCheck(KeyboardShortcut check)
        {
            if (!check.IsUp()) { return false; }
            foreach (KeyCode modifier in new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.RightControl, KeyCode.RightShift })
            {
                if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Handle Stat Messaging Requests
        /// </summary>
        /// <param name="changes"></param>
        private void RequestHandler(StatMessaging.Change[] changes)
        {
            foreach(StatMessaging.Change change in changes)
            {
                Debug.Log("Retexture Plugin: Request To Retexture " + change.cid + " To " + change.value);
                CreatureBoardAsset asset = null;
                CreaturePresenter.TryGetAsset(change.cid, out asset);
                GameObject go = GameObject.Find("Effect:" + asset.CreatureId);
                Material mat = null;
                if (go != null)
                {
                    Debug.Log("Retexture Plugin: Retexturing Effect");
                    mat = FindMaterial(GameObject.Find("Effect:" + asset.CreatureId), "RETEXTURE_MAT");
                }
                else
                {
                    Debug.Log("Retexture Plugin: Retexturing Mini");
                    mat = FindMaterial(Utility.GetAssetLoader(asset.CreatureId), "RETEXTURE_MAT");
                }
                if (change.action!=StatMessaging.ChangeType.removed)
                {
                    ApplyTexture(asset, mat, change.value);
                }
                else // change.action==StatMessaging.ChangeType.removed
                {
                    RestoreTexture(asset, mat);
                }
            }
        }

        /// <summary>
        /// Apply Alternate Texture
        /// </summary>
        /// <param name="asset">Asset whose material is to be changed</param>
        /// <param name="mat">Material to be changed</param>
        /// <param name="source">Source of the alternate texture</param>
        private void ApplyTexture(CreatureBoardAsset asset, Material mat, String source)
        {
            if (!originalMaterials.ContainsKey(asset.CreatureId))
            {
                Debug.Log("Retexture Plugin: Storing orignal " + asset.Name + " material");
                originalMaterials.Add(asset.CreatureId, mat.mainTexture);
            }

            var go = GameObject.Find("Effect:" + asset.CreatureId);
            if (go == null)
            {
                go = Utility.GetAssetLoader(asset.CreatureId);
            }

            if (source.EndsWith(".mp4") || source.EndsWith(".mov;") || source.EndsWith(".webm;") || source.EndsWith(".wmv;"))
            {
                ApplyVideo(go, "RETEXTURE_MAT", source); 
                return;
            }

            go.RemoveComponentIfExists<VideoPlayer>();
            var children = go.GetComponentsInChildren<VideoPlayer>().Select(v => v.gameObject);
            foreach (var child in children)
            {
                child.RemoveComponentIfExists<VideoPlayer>();
            }

            Debug.Log("Retexture Plugin: Retexturing " + asset.Name + " main texture (" + mat.name + ":" + mat.mainTexture.name + ") with  " + source);
            mat.mainTexture = LordAshes.FileAccessPlugin.Image.LoadTexture(source);
        }

        /// <summary>
        /// Restore original texure
        /// </summary>
        /// <param name="asset">Asset whose material is to be changed</param>
        /// <param name="mat">Material to be changed</param>
        private void RestoreTexture(CreatureBoardAsset asset, Material mat)
        {
            var go = asset.gameObject;
            go.RemoveComponentIfExists<VideoPlayer>();
            var children = go.GetComponentsInChildren<VideoPlayer>().Select(v => v.gameObject);
            foreach (var child in children)
            {
                child.RemoveComponentIfExists<VideoPlayer>();
            }

            Debug.Log("Retexture Plugin: Restoring " + asset.Name + " original material");
            mat.mainTexture = originalMaterials[asset.CreatureId];
            Debug.Log("Retexture Plugin: Removing stored material for " + asset.Name);
            originalMaterials.Remove(asset.CreatureId);
        }

        /// <summary>
        /// Finds material by name
        /// </summary>
        /// <param name="asset">Asset whose children are searched</param>
        /// <param name="name">Name of the material</param>
        /// <returns>Material</returns>
        private Material FindMaterial(GameObject asset, string name, bool useDefault = true)
        {
            List<Renderer> renderers = new List<Renderer>();
            foreach (MeshRenderer mr in asset.GetComponentsInChildren<MeshRenderer>())
            {
                Debug.Log("Retexture Plugin: Adding MeshRenderer " + mr.name);
                renderers.Add(mr);
            }
            foreach (SkinnedMeshRenderer mr in asset.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Debug.Log("Retexture Plugin: Adding SkinnedMeshRenderer " + mr.name);
                renderers.Add(mr);
            }
            foreach (Renderer rend in renderers)
            {
                foreach (Material mat in rend.materials)
                {
                    Debug.Log("Retexture Plugin: Looking At Material " + rend.name + "." + mat.name);
                    if (mat.name.Contains(name)) 
                    {
                        Debug.Log("Retexture Plugin: Found " + rend.name + "." + mat.name);
                        return mat; 
                    }
                }
            }
            Debug.Log("Retexture Plugin: Using Default/Null");
            return (useDefault) ? renderers[0].material : null;
        }

        /// <summary>
        /// Finds GameObject and by renderer's material name then applies video
        /// </summary>
        /// <param name="asset">Asset whose children are searched</param>
        /// <param name="name">Name of the material</param>
        /// <param name="source">The source of the video</param>
        private void ApplyVideo(GameObject asset, string name, string source)
        {
            List<Renderer> renderers = new List<Renderer>();
            foreach (MeshRenderer mr in asset.GetComponentsInChildren<MeshRenderer>())
            {
                Debug.Log("Retexture Plugin: Adding MeshRenderer " + mr.name);
                renderers.Add(mr);
            }
            foreach (SkinnedMeshRenderer mr in asset.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Debug.Log("Retexture Plugin: Adding SkinnedMeshRenderer " + mr.name);
                renderers.Add(mr);
            }
            foreach (Renderer rend in renderers)
            {
                foreach (Material mat in rend.materials)
                {
                    Debug.Log("Retexture Plugin: Looking At Material " + rend.name + "." + mat.name);
                    if (mat.name.Contains(name))
                    {
                        Debug.Log("Retexture Plugin: Found " + rend.name + "." + mat.name);

                        var go = rend.gameObject;

                        VideoPlayer player;
                        if (!go.TryGetComponent(out player))
                            player = go.AddComponent<VideoPlayer>();
                        player.playOnAwake = true;
                        player.source = VideoSource.Url;
                        if (!source.StartsWith("http"))
                            source = "file://" + source;

                        player.url = source;
                        player.isLooping = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Finds material by name
        /// </summary>
        /// <param name="asset">Asset whose children are searched</param>
        /// <param name="name">Name of the material</param>
        /// <returns>Material</returns>
        private Material FindMaterial(AssetLoader asset, string name, bool useDefault = true)
        {
            return FindMaterial(asset.gameObject, name, useDefault);
        }
    }
}
