using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class Part
{
    protected readonly string ID;
    private readonly Image IMAGE;
    private Texture2D icon;
    private readonly double WEIGHT;
    public readonly GameObject GAME_OBJECT;
    private Point position;
    private readonly Shape.SHAPES SHAPE;
    private readonly double DURABILITY;
    private double remainingDurability;
    private readonly string[] STAT_PREFIXES = { "Weight: " }, STAT_SUFFIXES = { " N" };

    public Part(string id, Image image, double weight, Shape.SHAPES shape, double durability, double remainingDurability)
    {
        if (id == default || id == null || id == "" || ((int)id[0]) == 8203)
            ID = Guid.NewGuid().ToString();
        else ID = id;
        IMAGE = image;
        if (IMAGE != null)
            IMAGE.setTexture(ImageTools.fixBadColors(IMAGE.getTexture()));
        WEIGHT = weight;
        SHAPE = shape;
        DURABILITY = durability;
        this.remainingDurability = remainingDurability;
        if (IMAGE != null)
        {
            if (shape == Shape.SHAPES.RECTANGULAR_PRISM || shape == Shape.SHAPES.PYRAMID)
            {
                GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GAME_OBJECT.GetComponent<Renderer>().material.mainTexture = IMAGE.getTexture();
                Vector2[] uvs = GAME_OBJECT.GetComponent<MeshFilter>().mesh.uv;
                //front
                uvs[0] = new Vector2(0.0f, 0.24975f);
                uvs[1] = new Vector2(0.333f, 0.24975f);
                uvs[2] = new Vector2(0.0f, 0.333f);
                uvs[3] = new Vector2(0.333f, 0.333f);
                //top
                uvs[4] = new Vector2(0.334f, 0.333f);
                uvs[5] = new Vector2(0.666f, 0.333f);
                uvs[8] = new Vector2(0.334f, 0.0f);
                uvs[9] = new Vector2(0.666f, 0.0f);
                //back
                uvs[6] = new Vector2(1.0f, 0.24975f);
                uvs[7] = new Vector2(0.667f, 0.24975f);
                uvs[10] = new Vector2(1.0f, 0.333f);
                uvs[11] = new Vector2(0.667f, 0.333f);
                //bottom
                uvs[12] = new Vector2(0.0f, 0.334f);
                uvs[13] = new Vector2(0.0f, 0.666f);
                uvs[14] = new Vector2(0.333f, 0.666f);
                uvs[15] = new Vector2(0.333f, 0.334f);
                //left
                uvs[16] = new Vector2(0.334f, 0.334f);
                uvs[17] = new Vector2(0.334f, 0.41625f);
                uvs[18] = new Vector2(0.666f, 0.41625f);
                uvs[19] = new Vector2(0.666f, 0.334f);
                //right
                uvs[20] = new Vector2(0.667f, 0.334f);
                uvs[21] = new Vector2(0.667f, 0.41625f);
                uvs[22] = new Vector2(1.0f, 0.41625f);
                uvs[23] = new Vector2(1.0f, 0.334f);
                GAME_OBJECT.GetComponent<MeshFilter>().mesh.uv = uvs;
            }
            else if (shape == Shape.SHAPES.HEMISPHERE)
            {
                GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GAME_OBJECT.GetComponent<Renderer>().material.mainTexture = IMAGE.getTexture();
            }
            else if (shape == Shape.SHAPES.CYLINDER)
            {
                GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                GAME_OBJECT.GetComponent<Renderer>().material.mainTexture = IMAGE.getTexture();
            }
            position = new Point();
            GAME_OBJECT.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
            GAME_OBJECT.GetComponent<Collider>().isTrigger = false;
            if (!(this is Attachment))
                icon = null;
            else icon = IMAGE.getTexture();
        }
    }

    public Part(bool isPreview, string id, Image image, Texture2D icon, double weight, Shape.SHAPES shape, double durability, double remainingDurability)
    {
        if (id == default || id == null || id == "" || ((int)id[0]) == 8203)
            ID = Guid.NewGuid().ToString();
        else ID = id;
        IMAGE = image;
        if (IMAGE != null && !isPreview)
            IMAGE.setTexture(ImageTools.fixBadColors(IMAGE.getTexture()));
        WEIGHT = weight;
        SHAPE = shape;
        DURABILITY = durability;
        this.remainingDurability = remainingDurability;
        if (IMAGE != null && !isPreview)
        {
            if (shape == Shape.SHAPES.RECTANGULAR_PRISM || shape == Shape.SHAPES.PYRAMID)
            {
                GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GAME_OBJECT.GetComponent<Renderer>().material.mainTexture = IMAGE.getTexture();
                Vector2[] uvs = GAME_OBJECT.GetComponent<MeshFilter>().mesh.uv;
                //front
                uvs[0] = new Vector2(0.0f, 0.24975f);
                uvs[1] = new Vector2(0.333f, 0.24975f);
                uvs[2] = new Vector2(0.0f, 0.333f);
                uvs[3] = new Vector2(0.333f, 0.333f);
                //top
                uvs[4] = new Vector2(0.334f, 0.333f);
                uvs[5] = new Vector2(0.666f, 0.333f);
                uvs[8] = new Vector2(0.334f, 0.0f);
                uvs[9] = new Vector2(0.666f, 0.0f);
                //back
                uvs[6] = new Vector2(1.0f, 0.24975f);
                uvs[7] = new Vector2(0.667f, 0.24975f);
                uvs[10] = new Vector2(1.0f, 0.333f);
                uvs[11] = new Vector2(0.667f, 0.333f);
                //bottom
                uvs[12] = new Vector2(0.0f, 0.334f);
                uvs[13] = new Vector2(0.0f, 0.666f);
                uvs[14] = new Vector2(0.333f, 0.666f);
                uvs[15] = new Vector2(0.333f, 0.334f);
                //left
                uvs[16] = new Vector2(0.334f, 0.334f);
                uvs[17] = new Vector2(0.334f, 0.41625f);
                uvs[18] = new Vector2(0.666f, 0.41625f);
                uvs[19] = new Vector2(0.666f, 0.334f);
                //right
                uvs[20] = new Vector2(0.667f, 0.334f);
                uvs[21] = new Vector2(0.667f, 0.41625f);
                uvs[22] = new Vector2(1.0f, 0.41625f);
                uvs[23] = new Vector2(1.0f, 0.334f);
                GAME_OBJECT.GetComponent<MeshFilter>().mesh.uv = uvs;
            }
            else if (shape == Shape.SHAPES.HEMISPHERE)
            {
                GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GAME_OBJECT.GetComponent<Renderer>().material.mainTexture = IMAGE.getTexture();
            }
            else if (shape == Shape.SHAPES.CYLINDER)
            {
                GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                GAME_OBJECT.GetComponent<Renderer>().material.mainTexture = IMAGE.getTexture();
            }
            position = new Point();
            GAME_OBJECT.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
            GAME_OBJECT.GetComponent<Collider>().isTrigger = false;
            if (!(this is Attachment))
                this.icon = null;
            else this.icon = IMAGE.getTexture();
        }
        else
        {
            GAME_OBJECT = default;
            if (!(this is Attachment))
                this.icon = icon;
            else this.icon = IMAGE.getTexture();
        }
    }

    public void changeTextureAndShape(Texture texture, Mesh mesh, Shape.SHAPES shape)
    {
        GAME_OBJECT.GetComponent<Renderer>().material.mainTexture = texture;
        GAME_OBJECT.GetComponent<MeshFilter>().mesh = mesh;
        if (shape == Shape.SHAPES.RECTANGULAR_PRISM || shape == Shape.SHAPES.PYRAMID)
        {
            Vector2[] uvs = GAME_OBJECT.GetComponent<MeshFilter>().mesh.uv;
            //front
            uvs[0] = new Vector2(0.0f, 0.24975f);
            uvs[1] = new Vector2(0.333f, 0.24975f);
            uvs[2] = new Vector2(0.0f, 0.333f);
            uvs[3] = new Vector2(0.333f, 0.333f);
            //top
            uvs[4] = new Vector2(0.334f, 0.333f);
            uvs[5] = new Vector2(0.666f, 0.333f);
            uvs[8] = new Vector2(0.334f, 0.0f);
            uvs[9] = new Vector2(0.666f, 0.0f);
            //back
            uvs[6] = new Vector2(1.0f, 0.24975f);
            uvs[7] = new Vector2(0.667f, 0.24975f);
            uvs[10] = new Vector2(1.0f, 0.333f);
            uvs[11] = new Vector2(0.667f, 0.333f);
            //bottom
            uvs[12] = new Vector2(0.0f, 0.334f);
            uvs[13] = new Vector2(0.0f, 0.666f);
            uvs[14] = new Vector2(0.333f, 0.666f);
            uvs[15] = new Vector2(0.333f, 0.334f);
            //left
            uvs[16] = new Vector2(0.334f, 0.334f);
            uvs[17] = new Vector2(0.334f, 0.41625f);
            uvs[18] = new Vector2(0.666f, 0.41625f);
            uvs[19] = new Vector2(0.666f, 0.334f);
            //right
            uvs[20] = new Vector2(0.667f, 0.334f);
            uvs[21] = new Vector2(0.667f, 0.41625f);
            uvs[22] = new Vector2(1.0f, 0.41625f);
            uvs[23] = new Vector2(1.0f, 0.334f);
            GAME_OBJECT.GetComponent<MeshFilter>().mesh.uv = uvs;
        }
    }

    public string getID()
    {
        return ID;
    }

    public Image getImage()
    {
        return IMAGE;
    }

    public void setIcon(Texture2D icon)
    {
        this.icon = icon;
    }

    public Texture2D getIcon()
    {
        return icon;
    }

    public double getWeight()
    {
        return WEIGHT;
    }

    public GameObject getGameObject()
    {
        return GAME_OBJECT;
    }

    public Dimension getSize()
    {
        return new Dimension(IMAGE.getWidth(), IMAGE.getHeight(), IMAGE.getDepth());
    }

    public void setPosition(Point position)
    {
        this.position = position;
    }

    public Point getPosition()
    {
        return position;
    }

    public Shape.SHAPES getShape()
    {
        return SHAPE;
    }

    public double getDurability()
    {
        return DURABILITY;
    }

    public void damage(double damage)
    {
        remainingDurability -= damage;
        if (remainingDurability < 0)
            remainingDurability = 0;
    }

    public double getRemainingDurability()
    {
        return remainingDurability;
    }

    public void repair()
    {
        remainingDurability = DURABILITY;
    }

    public virtual string[] getStatStrings()
    {
        List<string> partStatStrings = new List<string>();
        partStatStrings.Add(StringTools.formatString(WEIGHT));
        for (int partStatStringIndex = 0; partStatStringIndex < STAT_PREFIXES.Length; ++partStatStringIndex)
            partStatStrings[partStatStringIndex] = STAT_PREFIXES[partStatStringIndex] + partStatStrings[partStatStringIndex] + STAT_SUFFIXES[partStatStringIndex];
        return partStatStrings.ToArray();
    }

    public virtual double[] compareTo(Part otherPart)
    {
        List<double> differenceInStats = new List<double>();
        differenceInStats.Add(otherPart.getRemainingDurability() - remainingDurability);
        differenceInStats.Add(otherPart.getDurability() - DURABILITY);
        differenceInStats.Add(otherPart.getWeight() - WEIGHT);
        return differenceInStats.ToArray();
    }

    public virtual double[] getStats()
    {
        List<double> stats = new List<double>();
        stats.Add(remainingDurability);
        stats.Add(DURABILITY);
        stats.Add(WEIGHT);
        return stats.ToArray();
    }

    public abstract Part clone(bool isPreview);

    public virtual bool equals(Part part)
    {
        return ID == part.ID && IMAGE == part.getImage() && WEIGHT == part.getWeight() && SHAPE == part.getShape() && DURABILITY == part.getDurability() && remainingDurability == part.getRemainingDurability();
    }

    public void destroyGameObject()
    {
        if (GAME_OBJECT != null)
            GameObject.Destroy(GAME_OBJECT);
    }

    public void toggleGameObject(bool active)
    {
        GAME_OBJECT.SetActive(active);
    }
}