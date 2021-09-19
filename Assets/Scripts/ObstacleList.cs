using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ObstacleList : PeripheralElement
{
    private readonly List<ObstacleData> OBSTACLES_DATA;
    private readonly GameObject GAME_OBJECT, CANVAS, BACK_BUTTON, SCROLLBAR;
    private readonly RectTransform OBSTACLES_INFO_CONTAINER, OBSTACLES_INFO_PANEL;
    private readonly ScrollRect SCROLL_RECT;
    private readonly List<GameObject> OBSTACLES_INFO;

    public ObstacleList(List<ObstacleData> obstaclesData)
    {
        OBSTACLES_DATA = obstaclesData;
        GAME_OBJECT = GameObject.Instantiate(Resources.Load("Prefabs/ObstaclesInfo") as GameObject);
        OBSTACLES_INFO_CONTAINER = GAME_OBJECT.transform.Find("ObstaclesInfoCard").Find("ObstaclesInfoContainer").gameObject.GetComponent<RectTransform>();
        OBSTACLES_INFO_PANEL = GAME_OBJECT.transform.Find("ObstaclesInfoCard").Find("ObstaclesInfoContainer").Find("ObstaclesInfoPanel").gameObject.GetComponent<RectTransform>();
        SCROLLBAR = GAME_OBJECT.transform.Find("ObstaclesInfoCard").Find("ObstaclesInfoScrollbar").gameObject;
        GameObject SCROLL_VIEW = GAME_OBJECT.transform.Find("ObstaclesInfoCard").gameObject;
        SCROLL_RECT = SCROLL_VIEW.GetComponent<ScrollRect>();
        BACK_BUTTON = GAME_OBJECT.transform.Find("BackButton").gameObject;
        OBSTACLES_INFO = new List<GameObject>();
        createGameObjectsForObstaclesInfo(OBSTACLES_DATA);
        GAME_OBJECT.SetActive(false);
        GameObject[] sceneGameObject = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject gameObject in sceneGameObject)
        {
            if (gameObject.name == "BuildHubCanvas" || gameObject.name == "FieldDynamicCanvas")
            {
                CANVAS = gameObject;
                break;
            }
        }
        update();
    }

    private void createGameObjectsForObstaclesInfo(List<ObstacleData> obstaclesData)
    {
        List<GameObject> obstaclesInfo = new List<GameObject>();
        FieldInfo[] fieldInfoList = obstaclesData[0].GetType().GetFields();
        foreach (ObstacleData obstacleData in obstaclesData)
        {
            GameObject obstacleDataGameObject = GameObject.Instantiate(Resources.Load("Prefabs/ObstacleInfo") as GameObject);
            GameObject fieldValueObject = obstacleDataGameObject.transform.Find("FieldValue").gameObject;
            obstacleDataGameObject.GetComponent<TextMeshProUGUI>().text = "";
            fieldValueObject.GetComponent<TextMeshProUGUI>().text = "";
            foreach (FieldInfo fieldInfo in fieldInfoList)
            {
                string fieldName = fieldInfo.Name;
                for (int characterIndex = fieldName.Length - 1; characterIndex >= 0; --characterIndex)
                    if (char.IsUpper(fieldName[characterIndex]))
                        fieldName = fieldName.Substring(0, characterIndex) + " " + fieldName.Substring(characterIndex);
                fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
                string fieldValue = null;
                double tempFieldValue = 0;
                if (fieldInfo.GetValue(obstacleData) != null)
                {
                    fieldValue = fieldInfo.GetValue(obstacleData).ToString().ToLower();
                    fieldValue = char.ToUpper(fieldValue[0]) + fieldValue.Substring(1);
                }
                obstacleDataGameObject.GetComponent<TextMeshProUGUI>().text += "\n" + fieldName + ":";
                fieldValueObject.GetComponent<TextMeshProUGUI>().text += "\n" + (fieldValue != null ? (!double.TryParse(fieldValue, out tempFieldValue) ? fieldValue : StringTools.formatString(double.Parse(fieldValue))) : "None");
            }
            obstacleDataGameObject.transform.SetParent(OBSTACLES_INFO_PANEL);
            obstacleDataGameObject.transform.localPosition = new Vector3(obstacleDataGameObject.transform.localPosition.x, obstacleDataGameObject.transform.localPosition.y, 0);
            obstacleDataGameObject.transform.localScale = Vector3.one;
            obstaclesInfo.Add(obstacleDataGameObject);
        }
        OBSTACLES_INFO.AddRange(obstaclesInfo);
    }

    protected override void calculatePoints()
    {

    }

    public override void update()
    {
        if (base.enabled)
        {
            if (!GAME_OBJECT.activeInHierarchy)
            {
                GAME_OBJECT.SetActive(true);
                GAME_OBJECT.transform.SetParent(default);
                GAME_OBJECT.transform.SetParent(CANVAS.transform);
                GAME_OBJECT.transform.SetAsLastSibling();
                GAME_OBJECT.transform.localPosition = new Vector3(GAME_OBJECT.transform.localPosition.x, GAME_OBJECT.transform.localPosition.y, 0);
                GAME_OBJECT.GetComponent<RectTransform>().offsetMin = Vector2.zero;
                GAME_OBJECT.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                GAME_OBJECT.GetComponent<RectTransform>().sizeDelta = Vector3.one;
                GAME_OBJECT.transform.localScale = Vector3.one;
            }
            if ((OBSTACLES_INFO_PANEL.offsetMax.y - OBSTACLES_INFO_PANEL.offsetMin.y) > (OBSTACLES_INFO_CONTAINER.offsetMax.y - OBSTACLES_INFO_CONTAINER.offsetMin.y))
            {
                SCROLLBAR.SetActive(true);
                SCROLL_RECT.vertical = true;
            }
            else
            {
                SCROLLBAR.SetActive(false);
                SCROLL_RECT.vertical = false;
            }
            if (!CANVAS.name.Contains("BuildHub") || BACK_BUTTON.GetComponent<ButtonListener>().isClicked())
            {
                BACK_BUTTON.GetComponent<ButtonListener>().resetClick();
                GAME_OBJECT.transform.SetParent(null);
                GAME_OBJECT.SetActive(false);
                base.disable();
            }
        }
        else if (GAME_OBJECT.activeInHierarchy)
        {
            GAME_OBJECT.SetActive(false);
            GAME_OBJECT.transform.SetParent(default);
        }
    }

    public void onGUI()
    {

    }

    public bool isEnabled()
    {
        return base.enabled;
    }
}