using UnityEngine;
using TMPro;

public class FieldPeripheral
{
    private readonly Robot HUMAN;
    private readonly Robot[] ROBOTS;
    private readonly float DURABILITY_BAR_SPACING = 40f, DURABILITY_BAR_LABEL_OFFSET = 10f, CHARGE_BAR_OFFSET = -60f;
    private readonly int DURABILITY_BAR_TO_START_FROM;
    private readonly Point HUMAN_DURABILITY_BAR_POSITION = new Point();
    private readonly Point ROBOT_DURABILITY_BAR_POSITION = new Point();
    private readonly Dimension CROSSHAIR_SIZE = new Dimension(3, 35), DURABILITY_BAR_SIZE = new Dimension(400, 30);
    private readonly GameObject STATIC_CANVAS, DYNAMIC_CANVAS, HUMAN_DURABILITY_BAR, HUMAN_DURABILITY_BAR_LABEL, CHARGE_BAR, ELAPSED_TIME_BAR, MIN_CHARGE_TIME_TICK, CROSSHAIR;
    private readonly GameObject[] ROBOT_DURABILITY_BARS, ROBOT_DURABILITY_BAR_LABELS;
    private readonly AttachmentWheel ATTACHMENT_WHEEL;
    private readonly GameObject WHEEL;
    private readonly string STATIC_CANVAS_NAME = "FieldStaticCanvas", DYNAMIC_CANVAS_NAME = "FieldDynamicCanvas";
    private SettingPairs settingPairs;
    private string chargeBarColorString, crosshairGraphicString, attachmentWheelGraphicString;

    public FieldPeripheral(Robot human, Robot[] robots, SettingPairs settingPairs)
    {
        HUMAN = human;
        ROBOTS = robots;
        this.settingPairs = settingPairs;
        chargeBarColorString = this.settingPairs.charge_bar_color;
        crosshairGraphicString = this.settingPairs.crosshair_graphic;
        attachmentWheelGraphicString = this.settingPairs.attachment_wheel_graphic;
        STATIC_CANVAS = GameObject.Find(STATIC_CANVAS_NAME);
        DYNAMIC_CANVAS = GameObject.Find(DYNAMIC_CANVAS_NAME);
        DURABILITY_BAR_TO_START_FROM = 1;
        if (HUMAN != null)
        {
            DURABILITY_BAR_TO_START_FROM = 2;
            HUMAN_DURABILITY_BAR = GameObject.Instantiate(Resources.Load("Prefabs/DurabilityBar") as GameObject);
            HUMAN_DURABILITY_BAR_LABEL = GameObject.Instantiate(Resources.Load("Prefabs/DurabilityBarLabel") as GameObject);
            HUMAN_DURABILITY_BAR.transform.SetParent(STATIC_CANVAS.transform);
            HUMAN_DURABILITY_BAR_LABEL.transform.SetParent(STATIC_CANVAS.transform);
            HUMAN_DURABILITY_BAR.transform.localPosition = new Vector3(STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.x / 2 - DURABILITY_BAR_LABEL_OFFSET, STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.y / 2 - DURABILITY_BAR_LABEL_OFFSET, 0);
            HUMAN_DURABILITY_BAR_LABEL.transform.localPosition = new Vector3(STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.x / 2 - DURABILITY_BAR_LABEL_OFFSET, STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.y / 2 - DURABILITY_BAR_LABEL_OFFSET, 0);
            HUMAN_DURABILITY_BAR_LABEL.GetComponent<TextMeshProUGUI>().text = HUMAN.getName();
            HUMAN_DURABILITY_BAR.GetComponent<RectTransform>().localScale = Vector3.one;
            HUMAN_DURABILITY_BAR_LABEL.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        ROBOT_DURABILITY_BARS = new GameObject[ROBOTS.Length];
        ROBOT_DURABILITY_BAR_LABELS = new GameObject[ROBOTS.Length];
        int robotIndex = 0;
        foreach (Robot otherRobot in ROBOTS)
        {
            ROBOT_DURABILITY_BARS[robotIndex] = GameObject.Instantiate(Resources.Load("Prefabs/DurabilityBar") as GameObject);
            ROBOT_DURABILITY_BAR_LABELS[robotIndex] = GameObject.Instantiate(Resources.Load("Prefabs/DurabilityBarLabel") as GameObject);
            ROBOT_DURABILITY_BARS[robotIndex].transform.SetParent(STATIC_CANVAS.transform);
            ROBOT_DURABILITY_BAR_LABELS[robotIndex].transform.SetParent(STATIC_CANVAS.transform);
            ROBOT_DURABILITY_BARS[robotIndex].transform.localPosition = new Vector3(STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.x / 2 - DURABILITY_BAR_LABEL_OFFSET, STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.y / 2 - (robotIndex + DURABILITY_BAR_TO_START_FROM) * DURABILITY_BAR_SPACING, 0);
            ROBOT_DURABILITY_BAR_LABELS[robotIndex].transform.localPosition = new Vector3(STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.x / 2 - DURABILITY_BAR_LABEL_OFFSET, STATIC_CANVAS.GetComponent<RectTransform>().sizeDelta.y / 2 - (robotIndex + DURABILITY_BAR_TO_START_FROM) * DURABILITY_BAR_SPACING, 0);
            ROBOT_DURABILITY_BARS[robotIndex].GetComponent<RectTransform>().localScale = Vector3.one;
            ROBOT_DURABILITY_BAR_LABELS[robotIndex].GetComponent<RectTransform>().localScale = Vector3.one;
            ROBOT_DURABILITY_BAR_LABELS[robotIndex++].GetComponent<TextMeshProUGUI>().text = otherRobot.getName();
        }
        CHARGE_BAR = GameObject.Instantiate(Resources.Load("Prefabs/ChargeBar") as GameObject);
        ELAPSED_TIME_BAR = CHARGE_BAR.transform.Find("ElapsedTimeBar").gameObject;
        MIN_CHARGE_TIME_TICK = CHARGE_BAR.transform.Find("MinChargeTimeTick").gameObject;
        ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale = new Vector3(0, ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale.y, 0);
        ELAPSED_TIME_BAR.GetComponent<UnityEngine.UI.Image>().color = ImageTools.getColorFromString(chargeBarColorString); ;
        CHARGE_BAR.SetActive(false);
        CHARGE_BAR.transform.SetParent(DYNAMIC_CANVAS.transform);
        CHARGE_BAR.transform.localPosition = new Vector3(0, CHARGE_BAR_OFFSET, 0);
        CHARGE_BAR.transform.localScale = Vector3.one;
        changeWeapon();
        CROSSHAIR = GameObject.Instantiate(Resources.Load("Prefabs/Crosshair") as GameObject);
        CROSSHAIR.GetComponent<UnityEngine.UI.Image>().sprite = ImageTools.getSpriteFromString(crosshairGraphicString);
        CROSSHAIR.SetActive(false);
        CROSSHAIR.transform.SetParent(DYNAMIC_CANVAS.transform);
        CROSSHAIR.transform.localPosition = Vector3.zero;
        CROSSHAIR.transform.localScale = Vector3.one;
        Robot robot = (HUMAN != null) ? HUMAN : ROBOTS[0];
        MIN_CHARGE_TIME_TICK.transform.localPosition = new Vector3(-CHARGE_BAR.GetComponent<RectTransform>().sizeDelta.x / 2 + (float)robot.getAttachments().Selected().getMinChargeTime() / (float)robot.getAttachments().Selected().getMaxChargeTime() * CHARGE_BAR.GetComponent<RectTransform>().sizeDelta.x, MIN_CHARGE_TIME_TICK.transform.localPosition.y, 0);
        ATTACHMENT_WHEEL = new AttachmentWheel(robot.getAttachments());
        WHEEL = ATTACHMENT_WHEEL.getWheelMenu().gameObject;
        WHEEL.GetComponent<UnityEngine.UI.Image>().sprite = ImageTools.getSpriteFromString(attachmentWheelGraphicString);
    }

    public void chargeWeapon()
    {
        Attachment attachment = (HUMAN != null) ? HUMAN.getAttachments().Selected() : ROBOTS[0].getAttachments().Selected();
        if (!attachment.isPassive() && attachment.canUse())
        {
            if (!CHARGE_BAR.activeInHierarchy)
                attachment.resetCharging();
            CROSSHAIR.SetActive(true);
            CHARGE_BAR.SetActive(true);
        }
    }

    public void fireWeapon()
    {
        CROSSHAIR.SetActive(false);
        ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale = new Vector3(0, ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale.y, 0);
        CHARGE_BAR.SetActive(false);
    }

    public void nextAttachment()
    {
        ATTACHMENT_WHEEL.nextAttachment();
        changeWeapon();
    }

    public void previousAttachment()
    {
        ATTACHMENT_WHEEL.previousAttachment();
        changeWeapon();
    }

    private void changeWeapon()
    {
        Robot robot = (HUMAN != null) ? HUMAN : ROBOTS[0];
        if (robot.getAttachments().Selected().isWeapon())
            MIN_CHARGE_TIME_TICK.transform.localPosition = new Vector3(-CHARGE_BAR.GetComponent<RectTransform>().sizeDelta.x / 2 + (float)robot.getAttachments().Selected().getMinChargeTime() / (float)robot.getAttachments().Selected().getMaxChargeTime() * CHARGE_BAR.GetComponent<RectTransform>().sizeDelta.x, MIN_CHARGE_TIME_TICK.transform.localPosition.y, 0);
        ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale = new Vector3(0, ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale.y, 0);
    }

    public void update(SettingPairs settingPairs)
    {
        this.settingPairs = settingPairs;
        if (chargeBarColorString != this.settingPairs.charge_bar_color)
        {
            chargeBarColorString = this.settingPairs.charge_bar_color;
            ELAPSED_TIME_BAR.GetComponent<UnityEngine.UI.Image>().color = ImageTools.getColorFromString(chargeBarColorString);
        }
        if (crosshairGraphicString != this.settingPairs.crosshair_graphic)
        {
            crosshairGraphicString = this.settingPairs.crosshair_graphic;
            CROSSHAIR.GetComponent<UnityEngine.UI.Image>().sprite = ImageTools.getSpriteFromString(crosshairGraphicString);
        }
        if (attachmentWheelGraphicString != this.settingPairs.attachment_wheel_graphic)
        {
            attachmentWheelGraphicString = this.settingPairs.attachment_wheel_graphic;
            WHEEL.GetComponent<UnityEngine.UI.Image>().sprite = ImageTools.getSpriteFromString(attachmentWheelGraphicString);
        }
        if (HUMAN != null && HUMAN_DURABILITY_BAR != null)
            HUMAN_DURABILITY_BAR.GetComponent<RectTransform>().localScale = new Vector3((float)(HUMAN.getRemainingDurability() / HUMAN.getDurability()), HUMAN_DURABILITY_BAR.GetComponent<RectTransform>().localScale.y, 0);
        int robotIndex = 0;
        foreach (GameObject durabilityBar in ROBOT_DURABILITY_BARS)
            durabilityBar.GetComponent<RectTransform>().localScale = new Vector3((float)(ROBOTS[robotIndex].getRemainingDurability() / ROBOTS[robotIndex++].getDurability()), durabilityBar.GetComponent<RectTransform>().localScale.y, 0);
        Robot robot = (HUMAN != null) ? HUMAN : ROBOTS[0];
        if (robot.getAttachments().Selected().isWeapon() && robot.getAttachments().Selected().isCooled())
            ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale = new Vector3((float)robot.getAttachments().Selected().getElapsedChargeTime() / (float)robot.getAttachments().Selected().getMaxChargeTime(), ELAPSED_TIME_BAR.GetComponent<RectTransform>().localScale.y, 0);
        int attachmentIndex = 0;
        Attachment[] attachments = robot.getAttachments().ToArray();
        foreach (Attachment attachment in attachments)
        {
            if (!attachment.isPassive())
            {
                if (attachment.canUse())
                    ATTACHMENT_WHEEL.enableAttachment(attachmentIndex++);
                else ATTACHMENT_WHEEL.disableAttachment(attachmentIndex++);
            }
        }
        CHARGE_BAR.transform.localScale = Vector3.one;
        CROSSHAIR.transform.localScale = Vector3.one;
    }
}