using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class Screen : IDisposable
{
    private readonly List<Setting> CURRENT_SETTINGS;
    public readonly SettingsMenu SETTINGS_MENU;
    public readonly GameObject GAME_OBJECT;
    private GameObject musicSound, buttonSound, attachmentSound, weaponSound, robotSound;
    public readonly Canvas CANVAS;
    public readonly List<GameObject> GAME_OBJECTS;
    private bool disposed;
    private readonly SettingsManager SETTINGS_MANAGER;
    protected SettingPairs settingPairs;
    protected Color colorScheme;
    private KeyCode menuToggle;
    private bool resetSettings, triedFindingSound;
    private double currentMasterVolume, currentMusicVolume, currentButtonVolume, currentAttachmentVolume, currentWeaponVolume, currentRobotVolume;

    public Screen(List<Setting> settingList)
    {
        CURRENT_SETTINGS = settingList;
        SETTINGS_MANAGER = new SettingsManager();
        settingPairs = SETTINGS_MANAGER.getSettingPairs(settingList);
        colorScheme = ImageTools.getColorFromString(settingPairs.color_scheme);
        getMenuToggleKeyCode();
        resetSettings = settingPairs.reset_settings;
        SETTINGS_MENU = new SettingsMenu(settingList, colorScheme, menuToggle, resetSettings);
        GAME_OBJECT = GameObject.Find(getName().ToLower());
        CANVAS = UnityEngine.Object.FindObjectOfType<Canvas>();
        findSound();
        currentMasterVolume = settingPairs.master_volume;
        currentMusicVolume = settingPairs.music_volume;
        currentButtonVolume = settingPairs.action_performed_volume;
        currentAttachmentVolume = settingPairs.attachment_sound_volume;
        currentWeaponVolume = settingPairs.weapon_used_volume;
        currentRobotVolume = settingPairs.robot_sound_volume;
        musicSound.GetComponent<AudioSource>().volume = (float)(currentMasterVolume / 100 * currentMusicVolume / 100);
        buttonSound.GetComponent<AudioSource>().volume = (float)(currentMasterVolume / 100 * currentButtonVolume / 100);
        GAME_OBJECTS = new List<GameObject>();
        disposed = false;
        triedFindingSound = false;
        GAME_OBJECTS.Add(GAME_OBJECT);
    }

    private void getMenuToggleKeyCode()
    {
        if (settingPairs.menu_toggle != default && menuToggle.ToString() != settingPairs.menu_toggle)
            this.menuToggle = (KeyCode)Enum.Parse(typeof(KeyCode), settingPairs.menu_toggle, true);
    }

    private void findSound()
    {
        GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        for (int gameObjectIndex = 0; gameObjectIndex < allGameObjects.Length; ++gameObjectIndex)
        {
            if (!allGameObjects[gameObjectIndex].name.Contains("SplashScreen"))
            {
                if (allGameObjects[gameObjectIndex].name.Contains("MusicSound"))
                    musicSound = allGameObjects[gameObjectIndex];
                else if (allGameObjects[gameObjectIndex].name.Contains("ButtonSound"))
                    buttonSound = allGameObjects[gameObjectIndex];
                else if (allGameObjects[gameObjectIndex].name.Contains("AttachmentSound"))
                    attachmentSound = allGameObjects[gameObjectIndex];
                else if (allGameObjects[gameObjectIndex].name.Contains("WeaponSound"))
                    weaponSound = allGameObjects[gameObjectIndex];
                else if (allGameObjects[gameObjectIndex].name.Contains("RobotSound"))
                    robotSound = allGameObjects[gameObjectIndex];
            }
        }
    }

    public virtual void Dispose()
    {
        disposed = true;
    }

    public string getName()
    {
        return this.GetType().ToString();
    }

    public virtual void show()
    {
        if (areSettingsOpen())
        {
            settingPairs = SETTINGS_MANAGER.getSettingPairs(getSettings());
            colorScheme = ImageTools.getColorFromString(settingPairs.color_scheme);
            if (!triedFindingSound && (musicSound == null || buttonSound == null || attachmentSound == null || weaponSound == null || robotSound == null))
            {
                triedFindingSound = true;
                findSound();
            }
            if (musicSound != null && (currentMasterVolume != settingPairs.master_volume || currentMusicVolume != settingPairs.music_volume))
                musicSound.GetComponent<AudioSource>().volume = (float)(settingPairs.master_volume / 100 * settingPairs.music_volume / 100);
            if (buttonSound != null && (currentMasterVolume != settingPairs.master_volume || currentButtonVolume != settingPairs.action_performed_volume))
                buttonSound.GetComponent<AudioSource>().volume = (float)(settingPairs.master_volume / 100 * settingPairs.action_performed_volume / 100);
            if (attachmentSound != null && (currentMasterVolume != settingPairs.master_volume || currentAttachmentVolume != settingPairs.attachment_sound_volume))
                attachmentSound.GetComponent<AudioSource>().volume = (float)(settingPairs.master_volume / 100 * settingPairs.attachment_sound_volume / 100);
            if (weaponSound != null && (currentMasterVolume != settingPairs.master_volume || currentWeaponVolume != settingPairs.weapon_used_volume))
                weaponSound.GetComponent<AudioSource>().volume = (float)(settingPairs.master_volume / 100 * settingPairs.weapon_used_volume / 100);
            if (robotSound != null && (currentMasterVolume != settingPairs.master_volume || currentRobotVolume != settingPairs.robot_sound_volume))
                robotSound.GetComponent<AudioSource>().volume = (float)(settingPairs.master_volume / 100 * settingPairs.robot_sound_volume / 100);
            currentMasterVolume = settingPairs.master_volume;
            currentMusicVolume = settingPairs.music_volume;
            currentButtonVolume = settingPairs.action_performed_volume;
            currentAttachmentVolume = settingPairs.attachment_sound_volume;
            currentWeaponVolume = settingPairs.weapon_used_volume;
            currentRobotVolume = settingPairs.robot_sound_volume;
            getMenuToggleKeyCode();
            resetSettings = settingPairs.reset_settings;
            SETTINGS_MENU.updateSettings(colorScheme, menuToggle, resetSettings);
            SETTINGS_MENU.update();
        }
    }

    public virtual void onGUI()
    {
        
    }

    protected bool isDisposed()
    {
        return disposed;
    }

    protected void openSettings()
    {
        SETTINGS_MENU.enable();
        triedFindingSound = false;
    }

    public List<Setting> getSettings()
    {
        return SETTINGS_MENU.getSettings();
    }

    public List<Setting> getSettingObjectList()
    {
        return SETTINGS_MENU.getSettingObjectList();
    }

    public bool areSettingsOpen()
    {
        return SETTINGS_MENU.isEnabled();
    }
}