using System;
using System.IO;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RoR2;
using UnityEngine;


namespace AlliedMonsterColors;

class AlliedMonsterColorsConfig
{
    private readonly ConfigEntry<bool> _enableAlliedMonsterColors;
    private readonly ConfigEntry<Color> _recolorTint;
    private readonly ConfigEntry<float> _recolorBrightness;
    private readonly ConfigEntry<bool> _recolorAlliedBeetles;
    private readonly ConfigEntry<bool> _recolorAlliedMonsters;
    private readonly ConfigEntry<bool> _recolorAlliedNonmonsters;
    private readonly ConfigEntry<bool> _recolorAlliedPlayers;
    private readonly ConfigEntry<bool> _recolorSelf;

    public static int configUpdateCount = 0; //this is probably a terrible way to handle the configUpdateCount
    
    public bool EnableAlliedMonsterColors { get => _enableAlliedMonsterColors.Value; }
    public Color RecolorTint { get => _recolorTint.Value; }
    public float RecolorBrightness { get => _recolorBrightness.Value; }
    public bool RecolorAlliedBeetles { get => _recolorAlliedBeetles.Value; }
    public bool RecolorAlliedMonsters { get => _recolorAlliedMonsters.Value; }
    public bool RecolorAlliedNonmonsters { get => _recolorAlliedNonmonsters.Value; }
    public bool RecolorAlliedPlayers { get => _recolorAlliedPlayers.Value; }
    public bool RecolorSelf { get => _recolorSelf.Value; }

    public AlliedMonsterColorsConfig(ConfigFile config)
    {
        _enableAlliedMonsterColors = config.Bind("General", "EnableAlliedMonsterColors", true, "Enables/Disables the entire mod.");
        _recolorTint = config.Bind("General", "RecolorTint", Color.green, "Sets the new color for the selected entities.");
        _recolorBrightness = config.Bind("General", "RecolorBrightness", 0.5f, "Sets the recolor brightness for the selected entities.");
        _recolorAlliedBeetles = config.Bind("General", "RecolorAlliedBeetles", true, "Maybe you think most allied monsters look fine, but just the Queen's Gland Beetle Guards look too similar? This option only colors the Queen's Gland spawns.");
        _recolorAlliedMonsters = config.Bind("General", "RecolorAlliedMonsters", true, "Recolors allied monsters (e.g. Happiest Mask ghosts).");
        _recolorAlliedNonmonsters = config.Bind("General", "RecolorAlliedNonmonsters", false,
            "Recolors allied non-monsters (e.g. turrets/drones).");
        _recolorAlliedPlayers = config.Bind("General", "RecolorAlliedPlayers", false,
            "Recolors allied players (if playing multiplayer).");
        _recolorSelf = config.Bind("General", "RecolorSelf", false,
            "Recolors self (since you are your own ally).");

        _enableAlliedMonsterColors.SettingChanged += AlliedMonsterColors.config_SettingChanged;
        _recolorTint.SettingChanged += AlliedMonsterColors.config_SettingChanged;
        _recolorBrightness.SettingChanged += AlliedMonsterColors.config_SettingChanged;
        _recolorAlliedMonsters.SettingChanged += AlliedMonsterColors.config_SettingChanged;
        _recolorAlliedNonmonsters.SettingChanged += AlliedMonsterColors.config_SettingChanged;
        _recolorAlliedPlayers.SettingChanged += AlliedMonsterColors.config_SettingChanged;
        _recolorSelf.SettingChanged += AlliedMonsterColors.config_SettingChanged;
    }

   public void BuildRiskOfOptionsMenu()
   {
       byte[] iconData = File.ReadAllBytes(System.Reflection.Assembly.GetExecutingAssembly().Location
           .Replace("AlliedMonsterColors.dll", "") + "icon.png");
        Texture2D modIconSprite = new Texture2D(256, 256);
        ImageConversion.LoadImage(modIconSprite, iconData);
        Sprite modIcon = Sprite.Create(modIconSprite,
            new Rect(0, 0, modIconSprite.width, modIconSprite.height), Vector2.zero);
        
        ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(_enableAlliedMonsterColors, new CheckBoxConfig() { description = "Enables/Disables the entire mod." }));
        ModSettingsManager.AddOption(new RiskOfOptions.Options.ColorOption(_recolorTint, new ColorOptionConfig() { description="Sets the new color for the selected entities." }));
        ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(_recolorBrightness, new SliderConfig() { min = 0.0f, max = 20f , FormatString = "{0:0.0}", description = "Sets the recolor brightness for the selected entities."}));
        ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(_recolorAlliedBeetles, new CheckBoxConfig() { description = "Maybe you think most allied monsters look fine, but just the Queen's Gland Beetle Guards look too similar? This option only colors the Queen's Gland spawns.\"" }));
        ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(_recolorAlliedMonsters, new CheckBoxConfig() { description = "Recolors allied monsters (e.g. Happiest Mask ghosts)." }));
        ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(_recolorAlliedNonmonsters, new CheckBoxConfig() { description = "Recolors allied non-monsters (i.e. turrets/drones)." }));
        ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(_recolorAlliedPlayers, new CheckBoxConfig() { description = "Recolors allied players (if playing multiplayer)." }));
        ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(_recolorSelf, new CheckBoxConfig() { description = "Recolors self (since you are your own ally, I suppose)." }));
        
        ModSettingsManager.SetModDescription("A Risk of Rain 2 mod to recolor allied units so they're easier to see, especially monsters so you know they're allied.");
        ModSettingsManager.SetModIcon(modIcon);
    }
}