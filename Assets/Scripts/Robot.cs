using System.Collections.Generic;
using System;
using UnityEngine;

public class Robot
{
    private readonly float POSITION_MULTIPLIER = 2.5f;
    public static readonly double SCALE = .5;
    private readonly string[] ROBOT_STAT_PREFIXES = { "Weight: ", "Max attachments: ", "Climb angle: ", "Max speed: ", "Max capacity: " }, ROBOT_STAT_SUFFIXES = { " N", "", " deg", " m/s", " N" };
    public static readonly string DEFAULT_NAME = "Robot\n";
    private string name;
    private readonly bool HUMAN;
    private readonly double WEIGHT;
    private readonly double CLIMB_ANGLE;
    private readonly int MAX_ATTACHMENTS;
    private readonly double MAX_SPEED;
    private readonly double MAX_FORCE;
    public readonly GameObject GAME_OBJECT;
    private readonly Head HEAD;
    private readonly Body BODY;
    private readonly Mobility MOBILITY;
    private readonly Carousel<Attachment> ATTACHMENTS;
    private readonly Dimension SIZE;
    private Point force, cachedPosition, cachedAngle;
    private double damageReceived, damageDealt;
    private bool active, positionInitialized;

    public Robot(string name, bool human, Head head, Body body, Mobility mobility, Attachment[] attachments)
    {
        Point position = new Point();
        cachedPosition = position;
        cachedAngle = position;
        force = new Point();
        damageReceived = 0;
        damageDealt = 0;
        active = true;
        positionInitialized = false;
        HUMAN = human;
        if (name != default && name != "")
            this.name = name;
        else this.name = DEFAULT_NAME;
        HEAD = head;
        BODY = body;
        MOBILITY = mobility;
        ATTACHMENTS = new Carousel<Attachment>();
        ATTACHMENTS.AddRange(attachments);
        if (HEAD == null || BODY == null || MOBILITY == null || !ATTACHMENTS.Exists(attachment => attachment.isWeapon()))
            throw new Exception("Some parts of a robot are missing. Robots with missing parts will deleted.");
        else
        {
            GAME_OBJECT = new GameObject();
            GAME_OBJECT.AddComponent<Rigidbody>();
            Vector3 robotFootprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
            GAME_OBJECT.GetComponent<Collider>().isTrigger = true;
            GAME_OBJECT.GetComponent<Rigidbody>().useGravity = true;
            setConstraints();
            Vector3 rescale = GAME_OBJECT.transform.localScale;
            rescale.x = (float)SCALE / robotFootprint.x;
            rescale.y = (float)SCALE / robotFootprint.z;
            rescale.z = (float)SCALE / robotFootprint.z;
            GAME_OBJECT.transform.localScale = rescale;
            HEAD.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
            BODY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
            MOBILITY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
            Vector3 robotPosition = GAME_OBJECT.transform.position = position.toVector3();
            MOBILITY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, 0, robotPosition.z);
            BODY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER, robotPosition.z);
            if (HEAD.getShape() == Shape.SHAPES.HEMISPHERE)
                HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y - HEAD.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y / 2, robotPosition.z);
            else HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y, robotPosition.z);
            double attachmentWeight = 0;
            if (ATTACHMENTS != null && ATTACHMENTS.Count > 0)
                foreach (Attachment attachment in ATTACHMENTS)
                    attachmentWeight += attachment.getWeight();
            WEIGHT = HEAD.getWeight() + BODY.getWeight() + MOBILITY.getWeight() + attachmentWeight;
            MAX_ATTACHMENTS = BODY.getMaxAttachments();
            CLIMB_ANGLE = MOBILITY.getClimbAngle();
            MAX_SPEED = MOBILITY.getMaxSpeed();
            MAX_FORCE = MOBILITY.getMaxForce();
            Vector3 footprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
            SIZE = new Dimension(footprint.x, footprint.y, footprint.z);
        }
    }

    public Robot(string name, bool human, Part[] parts)
    {
        Point position = new Point();
        cachedPosition = position;
        cachedAngle = position;
        force = new Point();
        damageReceived = 0;
        damageDealt = 0;
        active = true;
        positionInitialized = false;
        HUMAN = human;
        if (name != default && name != "")
            this.name = name;
        else this.name = DEFAULT_NAME;
        ATTACHMENTS = new Carousel<Attachment>();
        foreach (Part part in parts)
        {
            if (part is Head)
                HEAD = (Head)part;
            else if (part is Body)
                BODY = (Body)part;
            else if (part is Mobility)
                MOBILITY = (Mobility)part;
            else ATTACHMENTS.Add((Attachment)part);
        }
        if (HEAD == null || BODY == null || MOBILITY == null || !ATTACHMENTS.Exists(attachment => attachment.isWeapon()))
            throw new Exception();
        else
        {
            if (HEAD.GAME_OBJECT == default)
                HEAD = new Head(HEAD.getID(), HEAD.getImage(), HEAD.getWeight(), HEAD.getShape(), HEAD.getDurability(), HEAD.getRemainingDurability());
            if (BODY.GAME_OBJECT == default)
                BODY = new Body(BODY.getID(), BODY.getImage(), BODY.getWeight(), BODY.getShape(), BODY.getDurability(), BODY.getRemainingDurability(), BODY.getMaxAttachments());
            if (MOBILITY.GAME_OBJECT == default)
                MOBILITY = new Mobility(MOBILITY.getID(), MOBILITY.getImage(), MOBILITY.getWeight(), MOBILITY.getShape(), MOBILITY.getDurability(), MOBILITY.getRemainingDurability(), MOBILITY.getClimbAngle(), MOBILITY.getMaxSpeed(), MOBILITY.getMaxForce());
            GAME_OBJECT = new GameObject();
            GAME_OBJECT.AddComponent<Rigidbody>();
            Vector3 robotFootprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
            GAME_OBJECT.GetComponent<Collider>().isTrigger = true;
            GAME_OBJECT.GetComponent<Rigidbody>().useGravity = true;
            setConstraints();
            Vector3 rescale = GAME_OBJECT.transform.localScale;
            rescale.x = (float)SCALE / robotFootprint.x;
            rescale.y = (float)SCALE / robotFootprint.z;
            rescale.z = (float)SCALE / robotFootprint.z;
            GAME_OBJECT.transform.localScale = rescale;
            HEAD.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
            BODY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
            MOBILITY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
            Vector3 robotPosition = GAME_OBJECT.transform.position = position.toVector3();
            MOBILITY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, 0, robotPosition.z);
            BODY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER, robotPosition.z);
            if (HEAD.getShape() == Shape.SHAPES.HEMISPHERE)
                HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y - HEAD.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y / 2, robotPosition.z);
            else HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y, robotPosition.z);
            double attachmentWeight = 0;
            if (ATTACHMENTS != null && ATTACHMENTS.Count > 0)
                foreach (Attachment attachment in ATTACHMENTS)
                    attachmentWeight += attachment.getWeight();
            WEIGHT = HEAD.getWeight() + BODY.getWeight() + MOBILITY.getWeight() + attachmentWeight;
            MAX_ATTACHMENTS = BODY.getMaxAttachments();
            CLIMB_ANGLE = MOBILITY.getClimbAngle();
            MAX_SPEED = MOBILITY.getMaxSpeed();
            MAX_FORCE = MOBILITY.getMaxForce();
            Vector3 footprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
            SIZE = new Dimension(footprint.x, footprint.y, footprint.z);
        }
    }

    public Robot(string name, bool isPreview, bool human, Head head, Body body, Mobility mobility, Attachment[] attachments)
    {
        Point position = new Point();
        cachedPosition = position;
        cachedAngle = position;
        force = new Point();
        damageReceived = 0;
        damageDealt = 0;
        active = true;
        positionInitialized = false;
        HUMAN = human;
        if (name != default && name != "")
            this.name = name;
        else this.name = DEFAULT_NAME;
        HEAD = head;
        BODY = body;
        MOBILITY = mobility;
        ATTACHMENTS = new Carousel<Attachment>();
        ATTACHMENTS.AddRange(attachments);
        SIZE = new Dimension();
        if (HEAD == null || BODY == null || MOBILITY == null)
            throw new Exception();
        else
        {
            if (!isPreview)
            {
                GAME_OBJECT = new GameObject();
                GAME_OBJECT.AddComponent<Rigidbody>();
                Vector3 robotFootprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
                GAME_OBJECT.GetComponent<Collider>().isTrigger = true;
                GAME_OBJECT.GetComponent<Rigidbody>().useGravity = true;
                setConstraints();
                Vector3 rescale = GAME_OBJECT.transform.localScale;
                rescale.x = (float)SCALE / robotFootprint.x;
                rescale.y = (float)SCALE / robotFootprint.z;
                rescale.z = (float)SCALE / robotFootprint.z;
                GAME_OBJECT.transform.localScale = rescale;
                HEAD.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
                BODY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
                MOBILITY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
                Vector3 robotPosition = GAME_OBJECT.transform.position = position.toVector3();
                MOBILITY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, 0, robotPosition.z);
                BODY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER, robotPosition.z);
                if (HEAD.getShape() == Shape.SHAPES.HEMISPHERE)
                    HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y - HEAD.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y / 2, robotPosition.z);
                else HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y, robotPosition.z);
                Vector3 footprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
                SIZE = new Dimension(footprint.x, footprint.y, footprint.z);
            }
            double attachmentWeight = 0;
            if (ATTACHMENTS != null && ATTACHMENTS.Count > 0)
                foreach (Attachment attachment in ATTACHMENTS)
                    attachmentWeight += attachment.getWeight();
            WEIGHT = HEAD.getWeight() + BODY.getWeight() + MOBILITY.getWeight() + attachmentWeight;
            MAX_ATTACHMENTS = BODY.getMaxAttachments();
            CLIMB_ANGLE = MOBILITY.getClimbAngle();
            MAX_SPEED = MOBILITY.getMaxSpeed();
            MAX_FORCE = MOBILITY.getMaxForce();
        }
    }

    public Robot(string name, bool isPreview, bool human, Part[] parts)
    {
        Point position = new Point();
        cachedPosition = position;
        cachedAngle = position;
        force = new Point();
        damageReceived = 0;
        damageDealt = 0;
        active = true;
        positionInitialized = false;
        HUMAN = human;
        if (name != default && name != "")
            this.name = name;
        else this.name = DEFAULT_NAME;
        ATTACHMENTS = new Carousel<Attachment>();
        SIZE = new Dimension();
        foreach (Part part in parts)
        {
            if (part is Head)
                HEAD = (Head)part;
            else if (part is Body)
                BODY = (Body)part;
            else if (part is Mobility)
                MOBILITY = (Mobility)part;
            else ATTACHMENTS.Add((Attachment)part);
        }
        if (HEAD == null || BODY == null || MOBILITY == null)
            throw new Exception();
        else
        {
            if (!isPreview)
            {
                GAME_OBJECT = new GameObject();
                GAME_OBJECT.AddComponent<Rigidbody>();
                Vector3 robotFootprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
                GAME_OBJECT.GetComponent<Collider>().isTrigger = true;
                GAME_OBJECT.GetComponent<Rigidbody>().useGravity = true;
                setConstraints();
                Vector3 rescale = GAME_OBJECT.transform.localScale;
                rescale.x = (float)SCALE / robotFootprint.x;
                rescale.y = (float)SCALE / robotFootprint.z;
                rescale.z = (float)SCALE / robotFootprint.z;
                GAME_OBJECT.transform.localScale = rescale;
                if (HEAD.GAME_OBJECT == default)
                    HEAD = new Head(HEAD.getID(), HEAD.getImage(), HEAD.getWeight(), HEAD.getShape(), HEAD.getDurability(), HEAD.getRemainingDurability());
                HEAD.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
                if (BODY.GAME_OBJECT == default)
                    BODY = new Body(BODY.getID(), BODY.getImage(), BODY.getWeight(), BODY.getShape(), BODY.getDurability(), BODY.getRemainingDurability(), BODY.getMaxAttachments());
                BODY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
                if (MOBILITY.GAME_OBJECT == default)
                    MOBILITY = new Mobility(MOBILITY.getID(), MOBILITY.getImage(), MOBILITY.getWeight(), MOBILITY.getShape(), MOBILITY.getDurability(), MOBILITY.getRemainingDurability(), MOBILITY.getClimbAngle(), MOBILITY.getMaxSpeed(), MOBILITY.getMaxForce());
                MOBILITY.GAME_OBJECT.transform.SetParent(GAME_OBJECT.transform, false);
                Vector3 robotPosition = GAME_OBJECT.transform.position = position.toVector3();
                MOBILITY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, 0, robotPosition.z);
                BODY.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER, robotPosition.z);
                if (HEAD.getShape() == Shape.SHAPES.HEMISPHERE)
                    HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y - HEAD.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y / 2, robotPosition.z);
                else HEAD.GAME_OBJECT.transform.position = new Vector3(robotPosition.x, MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y * POSITION_MULTIPLIER + BODY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y, robotPosition.z);
                Vector3 footprint = GAME_OBJECT.AddComponent<BoxCollider>().bounds.size;
                SIZE = new Dimension(footprint.x, footprint.y, footprint.z);
            }
            double attachmentWeight = 0;
            if (ATTACHMENTS != null && ATTACHMENTS.Count > 0)
                foreach (Attachment attachment in ATTACHMENTS)
                    attachmentWeight += attachment.getWeight();
            WEIGHT = HEAD.getWeight() + BODY.getWeight() + MOBILITY.getWeight() + attachmentWeight;
            MAX_ATTACHMENTS = BODY.getMaxAttachments();
            CLIMB_ANGLE = MOBILITY.getClimbAngle();
            MAX_SPEED = MOBILITY.getMaxSpeed();
            MAX_FORCE = MOBILITY.getMaxForce();
        }
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public string getName()
    {
        return name;
    }

    public bool isHuman()
    {
        return HUMAN;
    }

    public Head getHead()
    {
        return HEAD;
    }

    public Body getBody()
    {
        return BODY;
    }

    public Mobility getMobility()
    {
        return MOBILITY;
    }

    public Carousel<Attachment> getAttachments()
    {
        return ATTACHMENTS;
    }

    public Part[] getParts()
    {
        Part[] parts = new Part[3 + ATTACHMENTS.Count];
        int partIndex = 0;
        parts[partIndex++] = HEAD;
        parts[partIndex++] = BODY;
        parts[partIndex++] = MOBILITY;
        foreach (Attachment attachment in ATTACHMENTS)
            parts[partIndex++] = attachment;
        return parts;
    }

    public Dimension getSize()
    {
        return SIZE;
    }

    public void setPosition(Point position)
    {
        GAME_OBJECT.transform.position = new Vector3((float)position.x, (float)position.y + (float)HEAD.getPosition().y + HEAD.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y / 2 - (float)MOBILITY.getPosition().y - MOBILITY.GAME_OBJECT.GetComponent<Renderer>().bounds.size.y / 2, (float)position.z);
    }

    public Point getPosition()
    {
        Vector3 position = GAME_OBJECT.transform.position;
        cachedPosition = new Point(position);
        cachedAngle = new Point(GAME_OBJECT.transform.eulerAngles);
        return cachedPosition;
    }

    public Point getCachedPosition()
    {
        return cachedPosition;
    }

    public Point getCachedAngle()
    {
        return cachedAngle;
    }

    public void setVelocity(Point velocity)
    {
        GAME_OBJECT.GetComponent<Rigidbody>().velocity = velocity.toVector3();
    }

    public Point getVelocity()
    {
        Vector3 velocity = GAME_OBJECT.GetComponent<Rigidbody>().velocity;
        return new Point(velocity);
    }

    public Point getAcceleration()
    {
        double mass = (double)GAME_OBJECT.GetComponent<Rigidbody>().mass;
        return new Point(force.x / mass, force.y / mass, force.z / mass);
    }

    public void setForce(Point force, bool directional_movement)
    {
        if (!positionInitialized && (Math.Abs(GAME_OBJECT.transform.position.x) != 0 && Math.Abs(GAME_OBJECT.transform.position.z) != 0))
            positionInitialized = true;
        if (positionInitialized)
        {
            this.force = force;
            if (directional_movement)
            {
                GAME_OBJECT.transform.localPosition += GAME_OBJECT.transform.TransformDirection(Vector3.forward) * (float)this.force.z * Time.deltaTime;
                GAME_OBJECT.transform.localPosition += GAME_OBJECT.transform.TransformDirection(Vector3.right) * (float)this.force.x * Time.deltaTime;
            }
            else GAME_OBJECT.transform.position += this.force.toVector3() * Time.deltaTime;
        }
        getPosition();
    }

    public Point getForce()
    {
        return force;
    }

    public double getClimbAngle()
    {
        return CLIMB_ANGLE;
    }

    public double getWeight()
    {
        return WEIGHT;
    }

    public int getMaxAttachments()
    {
        return MAX_ATTACHMENTS;
    }

    public double getMaxSpeed()
    {
        return MAX_SPEED;
    }

    public double getMaxForce()
    {
        return MAX_FORCE;
    }

    public void setConstraints()
    {
        GAME_OBJECT.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public void removeConstraints()
    {
        GAME_OBJECT.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }

    public void damage(double damage)
    {
        damageReceived += damage;
        Part[] parts = getParts();
        for (int partIndex = 0; partIndex < parts.Length; ++partIndex)
            parts[partIndex].damage(damage * parts[partIndex].getDurability() / getDurability());
        if (getRemainingDurability() <= 0)
            active = false;
    }

    public void didDamage(double damage)
    {
        damageDealt += damage;
    }

    public double getDamageDealt()
    {
        return damageDealt;
    }

    public double getRemainingDurability()
    {
        double remainingDurability = 0;
        Part[] parts = getParts();
        foreach (Part part in parts)
            remainingDurability += part.getRemainingDurability();
        return remainingDurability;
    }

    public double getDurability()
    {
        double durability = 0;
        Part[] parts = getParts();
        foreach (Part part in parts)
            durability += part.getDurability();
        return durability;
    }

    public bool isActive()
    {
        return active;
    }

    public void nextAttachment()
    {
        ATTACHMENTS.Next();
        if (ATTACHMENTS.Selected().isPassive())
            nextAttachment();
    }

    public void previousAttachment()
    {
        ATTACHMENTS.Previous();
        if (ATTACHMENTS.Selected().isPassive())
            previousAttachment();
    }

    public bool isVisible()
    {
        if (ATTACHMENTS.Selected().isInvisible())
            return false;
        foreach (Attachment attachment in ATTACHMENTS)
            if (attachment.isPassive() && attachment.isInvisible())
                return false;
        return true;
    }

    public string[] getRobotStatStrings()
    {
        List<string> robotStatStrings = new List<string>();
        robotStatStrings.Add(StringTools.formatString(WEIGHT));
        robotStatStrings.Add(StringTools.formatString(MAX_ATTACHMENTS));
        robotStatStrings.Add(StringTools.formatString(CLIMB_ANGLE));
        robotStatStrings.Add(StringTools.formatString(MAX_SPEED));
        robotStatStrings.Add(StringTools.formatString(MAX_FORCE));
        for (int robotStatStringIndex = 0; robotStatStringIndex < ROBOT_STAT_PREFIXES.Length; ++robotStatStringIndex)
            robotStatStrings[robotStatStringIndex] = ROBOT_STAT_PREFIXES[robotStatStringIndex] + robotStatStrings[robotStatStringIndex] + ROBOT_STAT_SUFFIXES[robotStatStringIndex];
        return robotStatStrings.ToArray();
    }

    public double[] compareTo(Robot otherRobot)
    {
        List<double> differenceInStats = new List<double>();
        differenceInStats.Add(otherRobot.getRemainingDurability() - getRemainingDurability());
        differenceInStats.Add(otherRobot.getDurability() - getDurability());
        differenceInStats.Add(otherRobot.getWeight() - WEIGHT);
        differenceInStats.Add(otherRobot.getMaxAttachments() - MAX_ATTACHMENTS);
        differenceInStats.Add(otherRobot.getClimbAngle() - CLIMB_ANGLE);
        differenceInStats.Add(otherRobot.getMaxSpeed() - MAX_SPEED);
        differenceInStats.Add(otherRobot.getMaxForce() - MAX_FORCE);
        return differenceInStats.ToArray();
    }

    public Robot clone(bool isPreview)
    {
        Part[] parts = getParts();
        Part[] cloneParts = new Part[parts.Length];
        for (int partIndex = 0; partIndex < cloneParts.Length; ++partIndex)
            cloneParts[partIndex] = parts[partIndex].clone(isPreview);
        return new Robot(name, isPreview, HUMAN, cloneParts);
    }

    public bool equals(Robot robot)
    {
        Part[] parts = getParts(), otherParts = robot.getParts();
        if (name != robot.getName() || HUMAN != robot.isHuman() || parts.Length != otherParts.Length)
            return false;
        for (int partIndex = 0; partIndex < parts.Length; ++partIndex)
            if (parts[partIndex].getID() != otherParts[partIndex].getID())
                return false;
        return true;
    }
}