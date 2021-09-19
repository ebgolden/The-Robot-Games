using System.Collections.Generic;
using UnityEngine;

public class Body : Part
{
    private readonly int MAX_ATTACHMENTS;
    private readonly string[] STAT_PREFIXES = { "Max attachments: " }, STAT_SUFFIXES = { "" };

    public Body(string id, Image image, double weight, Shape.SHAPES shape, double durability, double remainingDurability, int maxAttachments) : base(id, image, weight, shape, durability, remainingDurability)
    {
        MAX_ATTACHMENTS = maxAttachments;
    }

    public Body(bool isPreview, string id, Image image, Texture2D icon, double weight, Shape.SHAPES shape, double durability, double remainingDurability, int maxAttachments) : base(isPreview, id, image, icon, weight, shape, durability, remainingDurability)
    {
        MAX_ATTACHMENTS = maxAttachments;
    }

    public int getMaxAttachments()
    {
        return MAX_ATTACHMENTS;
    }

    public override string[] getStatStrings()
    {
        List<string> partStatStrings = new List<string>();
        partStatStrings.AddRange(base.getStatStrings());
        partStatStrings.Add(StringTools.formatString(MAX_ATTACHMENTS));
        for (int partStatStringIndex = 0; partStatStringIndex < STAT_PREFIXES.Length; ++partStatStringIndex)
            partStatStrings[partStatStringIndex + base.getStatStrings().Length] = STAT_PREFIXES[partStatStringIndex] + partStatStrings[partStatStringIndex + base.getStatStrings().Length] + STAT_SUFFIXES[partStatStringIndex];
        return partStatStrings.ToArray();
    }

    public override double[] compareTo(Part otherPart)
    {
        if (otherPart.GetType() == this.GetType())
        {
            List<double> differenceInStats = new List<double>();
            Body comparablePart = (Body)otherPart;
            differenceInStats.AddRange(base.compareTo(otherPart));
            differenceInStats.Add(comparablePart.getMaxAttachments() - MAX_ATTACHMENTS);
            return differenceInStats.ToArray();
        }
        else return base.compareTo(otherPart);
    }

    public override double[] getStats()
    {
        List<double> stats = new List<double>();
        stats.AddRange(base.getStats());
        stats.Add(MAX_ATTACHMENTS);
        return stats.ToArray();
    }

    public override Part clone(bool isPreview)
    {
        return new Body(isPreview, base.ID, base.getImage(), base.getIcon(), base.getWeight(), base.getShape(), base.getDurability(), base.getRemainingDurability(), MAX_ATTACHMENTS);
    }

    public override bool equals(Part part)
    {
        Body body = (Body)part;
        return base.equals(part) && MAX_ATTACHMENTS == body.getMaxAttachments();
    }
}