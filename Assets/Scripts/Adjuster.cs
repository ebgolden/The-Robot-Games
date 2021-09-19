using UnityEngine;

public abstract class Adjuster
{
    protected readonly Color INACTIVE_COLOR = new Color(0, 0, 0, .4f), ACTIVE_COLOR = new Color(1f, 0.45f, 0f, .8f);
    protected readonly string LABEL_TEXT, DESCRIPTION;
    public readonly GameObject GAME_OBJECT;
    protected Color colorScheme;

    public Adjuster(Color colorScheme, string labelText, string description)
    {
        this.colorScheme = colorScheme;
        LABEL_TEXT = labelText != null ? labelText : "";
        DESCRIPTION = description != null ? description : "";
        GAME_OBJECT = GameObject.Instantiate(Resources.Load("Prefabs/" + GetType().ToString()) as GameObject);
    }

    public string getName()
    {
        return LABEL_TEXT;
    }

    public string getDescription()
    {
        return DESCRIPTION;
    }

    public GameObject GetGameObject()
    {
        return GAME_OBJECT;
    }

    public abstract string getValue();

    public abstract void update(Color colorScheme);
}