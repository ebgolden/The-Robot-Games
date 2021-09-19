using UnityEngine;

public abstract class PeripheralElement
{
    protected bool enabled;
    protected readonly Texture2D TEXTURE2D;
    private readonly int TEXTURE_WIDTH = 1, TEXTURE_HEIGHT = 1;
    protected readonly GUIStyle LINE_STYLE, CENTERED_LINE_STYLE, LEFT_LINE_STYLE, STAT_LINE_STYLE, BAD_STAT_LINE_STYLE, CENTERED_SHORT_BUTTON_LINE_STYLE;
    public static readonly Color TEXT_COLOR = Color.white, BAD_COLOR = Color.red;

    public PeripheralElement()
    {
        enabled = false;
        TEXTURE2D = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT);
        LINE_STYLE = new GUIStyle();
        LINE_STYLE.normal.background = TEXTURE2D;
        LINE_STYLE.normal.textColor = TEXT_COLOR;
        LINE_STYLE.alignment = TextAnchor.UpperCenter;
        LINE_STYLE.fontSize = 25;
        CENTERED_LINE_STYLE = new GUIStyle();
        CENTERED_LINE_STYLE.normal.background = TEXTURE2D;
        CENTERED_LINE_STYLE.normal.textColor = TEXT_COLOR;
        CENTERED_LINE_STYLE.alignment = TextAnchor.MiddleCenter;
        CENTERED_LINE_STYLE.fontSize = 25;
        LEFT_LINE_STYLE = new GUIStyle();
        LEFT_LINE_STYLE.normal.background = TEXTURE2D;
        LEFT_LINE_STYLE.normal.textColor = TEXT_COLOR;
        LEFT_LINE_STYLE.alignment = TextAnchor.MiddleLeft;
        LEFT_LINE_STYLE.fontSize = 25;
        STAT_LINE_STYLE = new GUIStyle();
        STAT_LINE_STYLE.normal.background = TEXTURE2D;
        STAT_LINE_STYLE.normal.textColor = TEXT_COLOR;
        STAT_LINE_STYLE.alignment = TextAnchor.MiddleLeft;
        STAT_LINE_STYLE.fontSize = 16;
        BAD_STAT_LINE_STYLE = new GUIStyle();
        BAD_STAT_LINE_STYLE.normal.background = TEXTURE2D;
        BAD_STAT_LINE_STYLE.normal.textColor = BAD_COLOR;
        BAD_STAT_LINE_STYLE.alignment = TextAnchor.MiddleLeft;
        BAD_STAT_LINE_STYLE.fontSize = 16;
        CENTERED_SHORT_BUTTON_LINE_STYLE = new GUIStyle();
        CENTERED_SHORT_BUTTON_LINE_STYLE.normal.background = TEXTURE2D;
        CENTERED_SHORT_BUTTON_LINE_STYLE.normal.textColor = TEXT_COLOR;
        CENTERED_SHORT_BUTTON_LINE_STYLE.alignment = TextAnchor.MiddleCenter;
        CENTERED_SHORT_BUTTON_LINE_STYLE.fontSize = 20;
    }

    protected void setColor(Color color)
    {
        for (int y = 0; y < TEXTURE2D.height; ++y)
            for (int x = 0; x < TEXTURE2D.width; ++x)
                TEXTURE2D.SetPixel(x, y, color);
        TEXTURE2D.Apply();
    }

    public virtual void enable()
    {
        enabled = true;
    }

    public virtual void disable()
    {
        enabled = false;
    }

    protected abstract void calculatePoints();

    public abstract void update();
}