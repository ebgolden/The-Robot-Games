using UnityEngine;

public class Head : Part
{
    public Head(string id, Image image, double weight, Shape.SHAPES shape, double durability, double remainingDurability) : base(id, image, weight, shape, durability, remainingDurability)
    {
        
    }

    public Head(bool isPreview, string id, Image image, Texture2D icon, double weight, Shape.SHAPES shape, double durability, double remainingDurability) : base(isPreview, id, image, icon, weight, shape, durability, remainingDurability)
    {

    }

    public override string[] getStatStrings()
    {
        return base.getStatStrings();
    }

    public override double[] compareTo(Part otherPart)
    {
        return base.compareTo(otherPart);
    }

    public override double[] getStats()
    {
        return base.getStats();
    }

    public override Part clone(bool isPreview)
    {
        return new Head(isPreview, base.ID, base.getImage(), base.getIcon(), base.getWeight(), base.getShape(), base.getDurability(), base.getRemainingDurability());
    }
}