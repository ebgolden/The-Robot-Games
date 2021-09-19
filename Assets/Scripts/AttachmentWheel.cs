using UnityEngine;

public class AttachmentWheel
{
    private readonly Carousel<Attachment> ATTACHMENTS;
    private readonly WheelMenu MAIN_MENU;
    public Wheel wheel;
    private readonly GameObject CANVAS;
    private readonly string CANVAS_NAME = "FieldDynamicCanvas", MAIN_MENU_NAME = "Wheel";

    public AttachmentWheel(Carousel<Attachment> attachments)
    {
        ATTACHMENTS = attachments;
        CANVAS = GameObject.Find(CANVAS_NAME);
        wheel = new Wheel();
        int numberOfNonPassiveAttachments = 0;
        foreach (Attachment attachment in ATTACHMENTS)
            if (!attachment.isPassive())
                ++numberOfNonPassiveAttachments;
        wheel.wheelElements = new WheelElement[numberOfNonPassiveAttachments];
        int attachmentIndex = 0;
        foreach (Attachment attachment in ATTACHMENTS)
        {
            if (!attachment.isPassive())
            {
                wheel.wheelElements[attachmentIndex] = new WheelElement();
                wheel.wheelElements[attachmentIndex++].Icon = Sprite.Create(attachment.getImage().getTexture(), new Rect(0, 0, attachment.getImage().getTexture().width, attachment.getImage().getTexture().height), new Vector2(0.5f, 0.5f), 100);
            }
        }
        if (GameObject.FindObjectOfType<Canvas>() != null)
        {
            MAIN_MENU = GameObject.Find(MAIN_MENU_NAME).GetComponent<WheelMenu>();
            MAIN_MENU.transform.SetParent(CANVAS.transform);
            MAIN_MENU.transform.localPosition = new Vector3(CANVAS.GetComponent<RectTransform>().sizeDelta.x / 2 + MAIN_MENU.GetComponent<RectTransform>().sizeDelta.x / 6, 0, 0);
            MAIN_MENU.initialize(wheel);
        }
    }

    public void nextAttachment()
    {
        if (MAIN_MENU != null && wheel.wheelElements.Length > 1)
        {
            MAIN_MENU.next();
            if (ATTACHMENTS.Selected().isPassive())
                nextAttachment();
        }
    }

    public void previousAttachment()
    {
        if (MAIN_MENU != null && wheel.wheelElements.Length > 1)
        {
            MAIN_MENU.previous();
            if (ATTACHMENTS.Selected().isPassive())
                previousAttachment();
        }
    }

    public void disableAttachment(int attachmentIndex)
    {
        if (MAIN_MENU != null)
            MAIN_MENU.disableSlot(attachmentIndex);
    }

    public void enableAttachment(int attachmentIndex)
    {
        if (MAIN_MENU != null)
            MAIN_MENU.enableSlot(attachmentIndex);
    }

    public WheelMenu getWheelMenu()
    {
        return MAIN_MENU;
    }
}