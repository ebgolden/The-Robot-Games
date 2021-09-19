using UnityEngine;

public class WheelMenu : MonoBehaviour
{
    public Wheel data;
    public WheelSlot WheelSlotPrefab;
    public readonly float GAP_WIDTH_DEGREE = 0f;
    private readonly float DISABLED_ALPHA_VALUE = .2f;
    protected WheelSlot[] slots;

    public void initialize(Wheel wheel)
    {
        data = wheel;
        float stepLength = 360f / data.wheelElements.Length;
        float iconDist = Vector3.Distance(WheelSlotPrefab.Icon.transform.position, WheelSlotPrefab.SlotIcon.transform.position);
        slots = new WheelSlot[data.wheelElements.Length];
        for (int slotIndex = 0; slotIndex < data.wheelElements.Length; ++slotIndex)
        {
            slots[slotIndex] = Instantiate(WheelSlotPrefab, transform);
            slots[slotIndex].transform.localPosition = Vector3.zero;
            slots[slotIndex].transform.localRotation = Quaternion.identity;
            slots[slotIndex].SlotIcon.fillAmount = 1f / data.wheelElements.Length - GAP_WIDTH_DEGREE / 360f;
            slots[slotIndex].SlotIcon.transform.localPosition = Vector3.zero;
            slots[slotIndex].SlotIcon.transform.localRotation = Quaternion.Euler(0, 0, -stepLength / 2f + GAP_WIDTH_DEGREE / 2f + slotIndex * stepLength);
            slots[slotIndex].SlotIcon.color = new Color(1f, 1f, 1f, 0);
            slots[slotIndex].Icon.transform.localPosition = slots[slotIndex].SlotIcon.transform.localPosition + Quaternion.AngleAxis(slotIndex * stepLength, Vector3.forward) * Vector3.up * iconDist;
            slots[slotIndex].Icon.sprite = data.wheelElements[slotIndex].Icon;
        }
        transform.eulerAngles = Vector3.zero;
        transform.localEulerAngles = new Vector3(0, 0, 90);
    }

    public void next()
    {
        if (data.wheelElements.Length > 1)
            transform.Rotate(new Vector3(0, 0, 360f / data.wheelElements.Length));
    }

    public void previous()
    {
        if (data.wheelElements.Length > 1)
            transform.Rotate(new Vector3(0, 0, -360f / data.wheelElements.Length));
    }

    public void disableSlot(int slotIndex)
    {
        slots[slotIndex].Icon.material.SetFloat("_GrayscaleAmount", 1);
        Color materialColor = slots[slotIndex].Icon.material.color;
        materialColor.a = DISABLED_ALPHA_VALUE;
        slots[slotIndex].Icon.material.color = materialColor;
    }

    public void enableSlot(int slotIndex)
    {
        slots[slotIndex].Icon.material.SetFloat("_GrayscaleAmount", 0);
        Color materialColor = slots[slotIndex].Icon.material.color;
        materialColor.a = 1;
        slots[slotIndex].Icon.material.color = materialColor;
    }
}