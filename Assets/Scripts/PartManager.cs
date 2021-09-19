using System.Reflection;
using System;
using TinyJson;

public class PartManager
{
    public Part partDataToPart(PartData partData)
    {
        Part part = null;
        Image image = new Image(partData.image);
        switch (partData.partType)
        {
            case "Head":
                part = new Head(partData.id, image, partData.weight, (Shape.SHAPES)Enum.Parse(typeof(Shape.SHAPES), partData.shape), partData.durability, partData.durability);
                break;
            case "Body":
                part = new Body(partData.id, image, partData.weight, (Shape.SHAPES)Enum.Parse(typeof(Shape.SHAPES), partData.shape), partData.durability, partData.durability, partData.maxAttachments);
                break;
            case "Mobility":
                part = new Mobility(partData.id, image, partData.weight, (Shape.SHAPES)Enum.Parse(typeof(Shape.SHAPES), partData.shape), partData.durability, partData.durability, partData.climbAngle, partData.maxSpeed, partData.maxForce);
                break;
            default:
                Type partType = Type.GetType(partData.partType);
                ConstructorInfo[] constructorInfo = partType.GetConstructors();
                ParameterInfo[] parameters = constructorInfo[0].GetParameters();
                Effect<Robot> effect;
                if (partData.effect == null)
                    effect = null;
                else
                {
                    Type effectType = Type.GetType(partData.effect);
                    ConstructorInfo[] effectConstructorInfo = effectType.GetConstructors();
                    ParameterInfo[] effectParameters = effectConstructorInfo[0].GetParameters();
                    object[] parameterValues = new object[effectParameters.Length];
                    for (int parameterIndex = 0; parameterIndex < effectParameters.Length; ++parameterIndex)
                        parameterValues[parameterIndex] = 0;
                    effect = (Effect<Robot>)effectConstructorInfo[0].Invoke(parameterValues);
                }
                part = (Part)constructorInfo[0].Invoke(new object[] { partData.id, image, partData.weight, effect, (Shape.SHAPES)Enum.Parse(typeof(Shape.SHAPES), partData.shape), partData.durability, partData.durability, partData.minChargeTime, partData.maxChargeTime, partData.minCoolingTime, partData.maxDamage });
                break;
        }
        return part;
    }

    public PartData partToPartData(Part part)
    {
        PartData partData = new PartData();
        partData.partType = part.GetType().ToString();
        partData.id = part.getID();
        partData.image = part.getImage().ToString();
        partData.weight = part.getWeight();
        partData.shape = part.getShape().ToString();
        partData.durability = part.getDurability();
        if (part is Attachment)
        {
            if (((Attachment)part).getEffect() != null)
                partData.effect = ((Attachment)part).getEffect().GetType().ToString();
            partData.minChargeTime = ((Attachment)part).getMinChargeTime();
            partData.maxChargeTime = ((Attachment)part).getMaxChargeTime();
            partData.maxUsageTime = ((Attachment)part).getMaxUsageTime();
            partData.minCoolingTime = ((Attachment)part).getMinCoolingTime();
            partData.maxDamage = ((Attachment)part).getMaxDamage();
            partData.weapon = ((Attachment)part).isWeapon();
            partData.passive = ((Attachment)part).isPassive();
            partData.invisible = ((Attachment)part).isInvisible();
        }
        else if (part is Body)
            partData.maxAttachments = ((Body)part).getMaxAttachments();
        else if (part is Mobility)
        {
            partData.climbAngle = ((Mobility)part).getClimbAngle();
            partData.maxSpeed = ((Mobility)part).getMaxSpeed();
            partData.maxForce = ((Mobility)part).getMaxForce();
        }
        return partData;
    }

    public PartData getPartDataFromJSON(string partJSON)
    {
        string partString = partJSON.Substring(1, partJSON.Length - 2);
        partString = partString.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        if (partString[0] == '[')
            partString = partString.Substring(1);
        if (partString[partString.Length - 1] == ']')
            partString = partString.Substring(0, partString.Length - 1);
        partString = "{" + partString;
        return partString.FromJson<PartData>();
    }
}