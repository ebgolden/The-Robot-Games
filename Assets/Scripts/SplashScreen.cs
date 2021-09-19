using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SplashScreen : MonoBehaviour
{
    private long splashMessageHomeStartTime, splashScreenStartTime;
    private GameObject loadingSplashScreen, loadingIcon, splashMessage, musicSound, battleButtonSound;
    private Vector3 splashMessageHomePosition;
    private readonly float LOADING_ICON_ROTATE_DEGREES_PER_FRAME = 1, SPLASH_MESSAGE_ANIMATION_COLOR_SPEED = .01f;
    private float splashMessageStartYPosition;
    private readonly int SPLASH_MESSAGE_REST_TIME = 3000;
    private readonly string LOADING_SPLASH_SCREEN_NAME = "LoadingSplashScreen", LOADING_ICON_NAME = "LoadingIcon", SPLASH_MESSAGE_NAME = "SplashMessage", MUSIC_SOUND_NAME = "SplashScreenMusicSound", BATTLE_BUTTON_SOUND = "BattleButtonSound";
    private bool splashMessageColorDirection;
    private SplashMessageGenerator splashMessageGenerator;
    private Type screenType;
    private AsyncOperation asyncLoad;
    private bool enableSplashScreenAnimation = false;
    private Color colorScheme;
    private float musicVolume, battleButtionVolume;

    void Start()
    {
        loadingSplashScreen = GameObject.Find(LOADING_SPLASH_SCREEN_NAME);
        loadingIcon = GameObject.Find(LOADING_ICON_NAME);
        splashMessage = GameObject.Find(SPLASH_MESSAGE_NAME);
        splashMessageHomePosition = splashMessage.transform.localPosition;
        splashMessageStartYPosition = splashMessageHomePosition.y - (Math.Abs(splashMessageHomePosition.y) - (loadingIcon.GetComponent<RectTransform>().sizeDelta.y / 2 - loadingIcon.transform.localPosition.y) - splashMessage.GetComponent<RectTransform>().sizeDelta.y / 2);
        splashMessage.GetComponent<TextMeshProUGUI>().color = new Color(splashMessage.GetComponent<TextMeshProUGUI>().color.r, splashMessage.GetComponent<TextMeshProUGUI>().color.g, splashMessage.GetComponent<TextMeshProUGUI>().color.b, 0);
        musicSound = GameObject.Find(MUSIC_SOUND_NAME);
        battleButtonSound = GameObject.Find(BATTLE_BUTTON_SOUND);
        splashMessageHomeStartTime = 0;
        splashMessageColorDirection = true;
        splashMessageGenerator = new SplashMessageGenerator();
        screenType = typeof(Field);
        splashScreenStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (GetType().ToString() != scene.name)
            {
                foreach (GameObject gameObject in scene.GetRootGameObjects())
                {
                    RobotGame robotGame = gameObject.GetComponent<RobotGame>();
                    if (robotGame != null)
                    {
                        screenType = robotGame.getScreenTypeToChangeTo();
                        if (screenType == default)
                            screenType = GameObject.Find("BuildHubCanvas") != null ? typeof(BuildHub) : typeof(Field);
                        SettingsManager settingsManager = new SettingsManager();
                        SettingPairs settingPairs = settingsManager.getSettingPairs(robotGame.getCurrentSettings());
                        enableSplashScreenAnimation = settingPairs.splash_screen_animation;
                        splashMessage.SetActive(enableSplashScreenAnimation);
                        colorScheme = ImageTools.getColorFromString(settingPairs.color_scheme);
                        musicVolume = (float)(settingPairs.music_volume / 100 * settingPairs.master_volume / 100);
                        musicSound.GetComponent<AudioSource>().volume = musicVolume;
                        battleButtionVolume = (float)(settingPairs.action_performed_volume / 100 * settingPairs.master_volume / 100);
                        battleButtonSound.GetComponent<AudioSource>().volume = battleButtionVolume;
                        if (screenType.ToString() == "Field")
                            battleButtonSound.GetComponent<AudioSource>().Play();
                        loadingIcon.GetComponent<UnityEngine.UI.Image>().color = colorScheme;
                        splashMessage.GetComponent<TextMeshProUGUI>().color = colorScheme;
                        splashMessage.GetComponent<TextMeshProUGUI>().color = new Color(splashMessage.GetComponent<TextMeshProUGUI>().color.r, splashMessage.GetComponent<TextMeshProUGUI>().color.g, splashMessage.GetComponent<TextMeshProUGUI>().color.b, 0);
                    }
                    if (SceneManager.GetActiveScene().name == "SplashScreen")
                        gameObject.SetActive(false);
                }
            }
        }
        if (GetComponent<RobotGame>() == null)
            switchScene();
    }

    private void FixedUpdate()
    {
        if ((loadingSplashScreen == null) || (loadingIcon == null) || (splashMessage == null))
        {
            loadingSplashScreen = GameObject.Find(LOADING_SPLASH_SCREEN_NAME);
            loadingIcon = GameObject.Find(LOADING_ICON_NAME);
            splashMessage = GameObject.Find(SPLASH_MESSAGE_NAME);
        }
        if ((loadingSplashScreen != null) && (!loadingSplashScreen.activeSelf))
            loadingSplashScreen.SetActive(true);
        if (enableSplashScreenAnimation)
            animateSplashScreen();
        else splashMessage.SetActive(false);
        if (GetComponent<RobotGame>() == null)
            checkSceneLoadingAndActivate();
    }

    public void animateSplashScreen()
    {
        if (enableSplashScreenAnimation)
        {
            animateLoadingIcon();
            animateSplashMessage();
        }
    }

    private void animateLoadingIcon()
    {
        if (loadingIcon != null)
            loadingIcon.transform.Rotate(new Vector3(0, 0, -LOADING_ICON_ROTATE_DEGREES_PER_FRAME));
    }

    private void animateSplashMessage()
    {
        if (splashMessage != null)
        {
            if (splashMessage.GetComponent<TextMeshProUGUI>().color.a <= 0)
                splashMessage.GetComponent<TextMeshProUGUI>().text = splashMessageGenerator.generateSplashMessage(screenType);
            Color splashMessageColor = splashMessage.GetComponent<TextMeshProUGUI>().color;
            if (splashMessageHomeStartTime != 0 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - splashMessageHomeStartTime >= SPLASH_MESSAGE_REST_TIME)
                splashMessageHomeStartTime = 0;
            if (splashMessageHomeStartTime == 0)
            {
                splashMessage.GetComponent<TextMeshProUGUI>().color = new Color(splashMessageColor.r, splashMessageColor.g, splashMessageColor.b, splashMessageColor.a + (splashMessageColorDirection ? SPLASH_MESSAGE_ANIMATION_COLOR_SPEED : -SPLASH_MESSAGE_ANIMATION_COLOR_SPEED));
                if (splashMessage.GetComponent<TextMeshProUGUI>().color.a >= 1 || splashMessage.GetComponent<TextMeshProUGUI>().color.a <= 0)
                {
                    splashMessage.GetComponent<TextMeshProUGUI>().color = new Color(splashMessage.GetComponent<TextMeshProUGUI>().color.r, splashMessage.GetComponent<TextMeshProUGUI>().color.g, splashMessage.GetComponent<TextMeshProUGUI>().color.b, (splashMessage.GetComponent<TextMeshProUGUI>().color.a > 1 ? 1 : (splashMessage.GetComponent<TextMeshProUGUI>().color.a < 0 ? 0 : splashMessage.GetComponent<TextMeshProUGUI>().color.a)));
                    splashMessageHomeStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    splashMessageColorDirection = !splashMessageColorDirection;
                }
            }
        }
    }

    private void switchScene()
    {
        asyncLoad = SceneManager.LoadSceneAsync(screenType.Name, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;
    }

    private void checkSceneLoadingAndActivate()
    {
        if (asyncLoad.progress >= 0.9f && System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - splashScreenStartTime > SPLASH_MESSAGE_REST_TIME)
            asyncLoad.allowSceneActivation = true;
    }
}