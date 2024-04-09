using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomDiscordRPC
{
    [BepInPlugin("com.ev.CustomDiscordRPC", "CustomDiscordRPC", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        static string applicationID;
        static string Details;
        static string State;
        static string LargeImageKey;
        static string SmallImageKey;
        static string LargeImageText;
        static string SmallImageText;

        static GameObject go;

        public void Awake()
        {
            try
            {
                ConfigFile cfg = new ConfigFile(Path.Combine(BepInEx.Paths.ConfigPath, "CustomDiscordRPC.cfg"), true);

                applicationID = cfg.Bind("Customization", "Application_ID", "1227076853399814214", "The ID of your discord application!").Value;

                Details = cfg.Bind("Customization", "Details", "Custom Discord RPC", "The large text above state!").Value;

                State = cfg.Bind("Customization", "State", "Using Custom Discord RPC!", "The smaller text below details!").Value;

                LargeImageKey = cfg.Bind("Customization", "large_image_key", "Large_Image_Key", "The name of a image in your Rich Presence Art Assets!").Value;

                SmallImageKey = cfg.Bind("Customization", "small_image_key", "Small_Image_Key", "The name of a image in your Rich Presence Art Assets!").Value;

                LargeImageText = cfg.Bind("Customization", "LargeImageText", "Large Image Text", "The text above your large image (large key) when you hover over it!").Value;

                SmallImageText = cfg.Bind("Customization", "SmallImageText", "Small Image Text", "The text above your small image (small key) when you hover over it!").Value;

                if (gameObject.scene != null && !string.IsNullOrEmpty(gameObject.scene.name))
                {
                    if (go == null && !gameObject.scene.name.Contains("Hide", StringComparison.OrdinalIgnoreCase) && !gameObject.scene.name.Contains("Dont", StringComparison.OrdinalIgnoreCase))
                    {
                        go = new GameObject("CustomDiscordRPC");
                        go.AddComponent<Plugin>();
                        DontDestroyOnLoad(go);
                    }
                }

                byte[] dllBytes = LoadResource("CustomDiscordRPC.Resources.DiscordRPC.dll");
                if (dllBytes != null)
                {
                    var rpcAssembly = Assembly.Load(dllBytes);

                    var rpcClientType = rpcAssembly.GetType("DiscordRPC.DiscordRpcClient");
                    var rpcClient = Activator.CreateInstance(rpcClientType, string.IsNullOrEmpty(applicationID) ? "1227076853399814214" : applicationID);
                    var initializeMethod = rpcClientType.GetMethod("Initialize");
                    initializeMethod.Invoke(rpcClient, null);

                    var richPresenceType = rpcAssembly.GetType("DiscordRPC.RichPresence");
                    var presence = Activator.CreateInstance(richPresenceType);

                    richPresenceType.GetProperty("Details").SetValue(presence, string.IsNullOrEmpty(Details) ? "Custom Discord RPC" : Details);
                    richPresenceType.GetProperty("State").SetValue(presence, string.IsNullOrEmpty(State) ? "Using Custom Discord RPC!" : State);

                    var assetsType = rpcAssembly.GetType("DiscordRPC.Assets");
                    var assets = Activator.CreateInstance(assetsType);

                    assetsType.GetProperty("LargeImageKey").SetValue(assets, string.IsNullOrEmpty(LargeImageKey) ? "large_image_key" : LargeImageKey.ToLower());
                    assetsType.GetProperty("LargeImageText").SetValue(assets, string.IsNullOrEmpty(LargeImageText) ? "Large Image Text" : LargeImageText);

                    assetsType.GetProperty("SmallImageKey").SetValue(assets, string.IsNullOrEmpty(SmallImageKey) ? "small_image_key" : SmallImageKey.ToLower());
                    assetsType.GetProperty("SmallImageText").SetValue(assets, string.IsNullOrEmpty(SmallImageText) ? "Small Image Text" : SmallImageText);

                    richPresenceType.GetProperty("Assets").SetValue(presence, assets);

                    var setPresenceMethod = rpcClientType.GetMethod("SetPresence");
                    setPresenceMethod.Invoke(rpcClient, new object[] { presence });
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private byte[] LoadResource(string resourceName)
        {
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception e) { Debug.LogException(e); throw; }
        }
    }
}
