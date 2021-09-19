using UnityEngine;

public class ButtonListener : MonoBehaviour
{
    private readonly KeyCode EQUIPT_PART_LEFT = KeyCode.LeftControl, EQUIPT_PART_RIGHT = KeyCode.RightControl;
    private bool mouseOver, mouseClick, mouseControlClick;

    void Start()
    {
        mouseOver = false;
        mouseClick = false;
        mouseControlClick = false;
    }

    public void OnMouseEnter()
    {
        mouseOver = true;
    }

    public void OnMouseExit()
    {
        mouseOver = false;
    }

    public void OnMouseClick()
    {
        if (Input.GetKey(EQUIPT_PART_LEFT) || Input.GetKey(EQUIPT_PART_RIGHT))
            mouseControlClick = true;
        else mouseClick = true;
        if (name != "BattleButton")
        {
            GameObject buttonSound = null;
            GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
            for (int gameObjectIndex = 0; gameObjectIndex < allGameObjects.Length; ++gameObjectIndex)
            {
                if (allGameObjects[gameObjectIndex].name.Contains("ButtonSound"))
                {
                    buttonSound = allGameObjects[gameObjectIndex];
                    break;
                }
            }
            if (buttonSound != null)
                buttonSound.GetComponent<AudioSource>().Play();
        }
    }

    public bool isMouseOver()
    {
        return mouseOver;
    }

    public bool isClicked()
    {
        return mouseClick;
    }

    public bool isControlClicked()
    {
        return mouseControlClick;
    }

    public void resetClick()
    {
        mouseClick = false;
        mouseControlClick = false;
    }

    public void click()
    {
        mouseClick = true;
    }
}