using System;
using BepInEx;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AlliedMonsterColors
{
    // This attribute is required, and lists metadata for the plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class AlliedMonsterColors : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Alexio_Xela";
        public const string PluginName = "AlliedMonsterColors";
        public const string PluginVersion = "0.1.1";

        public static Material colorMaterial;
        private static AlliedMonsterColorsConfig _config;
        
        internal static ManualLogSource log { get; set; }
        
        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            log = base.Logger;
            
            //Init our config
            _config = new AlliedMonsterColorsConfig(Config);
            
            SetupHooks();
        }
        public void Start()
        {
            //Get some Risk of Options compatibility goin'.
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue("com.rune580.riskofoptions", out var riskOfOptionsPlugin)) _config.BuildRiskOfOptionsMenu();
        }
        private void SetupHooks()
        {
            On.RoR2.CharacterModel.InitMaterials += CharacterModel_InitMaterials; //Using this to make our material
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays; //Using this to apply our material
        }
        public static void config_SettingChanged(object obj, EventArgs eventArgs)
        {
            colorMaterial = setMatColorEffect(colorMaterial); //update colorMaterial to reflect the new config
            
            //This commented out chunk was an attempt to make configuration changes mid-run update immediately, but...it doesn't. No idea why not.
            /*foreach (CharacterBody body in CharacterBody.instancesList)
            {
                log.LogDebug("Updating " + body.name + " and its model " + body.modelLocator.modelTransform.GetComponent<CharacterModel>().name);
                ((Component) body.modelLocator.modelTransform).GetComponent<CharacterModel>().UpdateOverlays();
                ((Component) body.modelLocator.modelTransform).GetComponent<CharacterModel>().UpdateOverlayStates();
            }
            log.LogDebug("New config has been applied.");*/
        }
        private void CharacterModel_InitMaterials(On.RoR2.CharacterModel.orig_InitMaterials orig)
        { 
            orig();
            
            AsyncOperationHandle<Material> asyncOperationHandle = LegacyResourcesAPI.LoadAsync<Material>("Materials/matGhostEffect"); //"it has good bones," we'll just clean it up a little
            asyncOperationHandle.Completed += x =>
            {
                colorMaterial = setMatColorEffect(new Material(x.Result));
            };
        }
        private static Material setMatColorEffect(Material material) //modify matGhostEffect to look OK for this mod.
        {
            material.SetColor("_TintColor", _config.RecolorTint);
            material.SetTexture("_RemapTex", null);
            material.SetFloat("_Boost", _config.RecolorBrightness * (_config.EnableAlliedMonsterColors ? 1f : 0f));
            material.name = "matColorEffect";
            return material;
        }
        private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel characterModel)
        { //I believe this hook runs somewhat frequently on every CharacterModel
            orig(characterModel);
            
            //Next we will describe which scenarios we do NOT want to update the selected CharacterModel.
            if (!_config.EnableAlliedMonsterColors) return; //If the config is disabled, do nothing.
            if (!(characterModel.body != null && characterModel.body.master != null && //To update, we want characterModel.body.master not null
                  characterModel.body.master.teamIndex == TeamIndex.Player && //and we want them on the player team
                  !(characterModel.activeOverlayCount >= CharacterModel.maxOverlays))) //and we don't want to exceed max overlays
            {
                return;
            }

            if (characterModel.body.name.ToLower().Contains("gummy") && !_config.RecolorAlliedNonmonsters) return; //Don't recolor if they're goobo if the config says not to

            if (characterModel.body.isPlayerControlled && !_config.RecolorAlliedPlayers && //Don't recolor if they're player controlled if the config says not to...
                characterModel.body.master != PlayerCharacterMasterController.instances[0].master) return; //...unless it's the user (as that is handled by a different config)
            
            string characterName = characterModel.body.name.ToLower();
            
            if ((characterName.Contains("drone") || characterName.Contains("turret")) && !_config.RecolorAlliedNonmonsters) return; //don't color drones/turrets if the config says not to
            
            if (!(characterName.Contains("drone") || characterName.Contains("turret")) && !characterModel.body.isPlayerControlled && !_config.RecolorAlliedMonsters //don't color monsters if the config says not to
                && (characterName != "beetleguardallybody(clone)" || !_config.RecolorAlliedBeetles)) return; //Unless it's a beetle guard and the config says to recolor
            
            if (characterModel.body.master == PlayerCharacterMasterController.instances[0].master && //don't recolor if the character is the user...
                !_config.RecolorSelf) return; //...if the config says not to
            
            AddOverlay(colorMaterial); 
            
            void AddOverlay(Material overlayMaterial)
            {
                characterModel.currentOverlays[characterModel.activeOverlayCount++] = overlayMaterial;
            }
        }
    }
}
