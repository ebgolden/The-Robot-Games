using UnityEngine;

public class Dimension
{
    private double _width, _height, _depth;
    public double width {
        get { return _width; }
        set {
            _width = value;
            vector2.x = (float)value;
            vector3.x = (float)value;
        }
    }
    public double height
    {
        get { return _height; }
        set
        {
            _height = value;
            vector2.y = (float)value;
            vector3.y = (float)value;
        }
    }
    public double depth
    {
        get { return _depth; }
        set
        {
            _depth = value;
            vector3.z = (float)value;
        }
    }
    private Vector2 vector2;
    private Vector3 vector3;

    public Dimension()
    {
        width = 0;
        height = 0;
        depth = 0;
        vector2 = new Vector2((float)width, (float)height);
        vector3 = new Vector3((float)width, (float)height, (float)depth);
    }

    public Dimension(double width, double height)
    {
        this.width = width;
        this.height = height;
        depth = 0;
        vector2 = new Vector2((float)this.width, (float)this.height);
        vector3 = new Vector3((float)this.width, (float)this.height, (float)depth);
    }

    public Dimension(float width, float height)
    {
        this.width = width;
        this.height = height;
        depth = 0;
        vector2 = new Vector2((float)this.width, (float)this.height);
        vector3 = new Vector3((float)this.width, (float)this.height, (float)depth);
    }

    public Dimension(int width, int height)
    {
        this.width = width;
        this.height = height;
        depth = 0;
        vector2 = new Vector2((float)this.width, (float)this.height);
        vector3 = new Vector3((float)this.width, (float)this.height, (float)depth);
    }

    public Dimension(double width, double height, double depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
        vector2 = new Vector2((float)this.width, (float)this.height);
        vector3 = new Vector3((float)this.width, (float)this.height, (float)this.depth);
    }

    public Dimension(float width, float height, float depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
        vector2 = new Vector2((float)this.width, (float)this.height);
        vector3 = new Vector3((float)this.width, (float)this.height, (float)this.depth);
    }

    public Dimension(int width, int height, int depth)
    {
        this.width = width;
        this.height = height;
        this.depth = depth;
        vector2 = new Vector2((float)this.width, (float)this.height);
        vector3 = new Vector3((float)this.width, (float)this.height, (float)this.depth);
    }

    public Dimension(Vector2 dimension)
    {
        width = dimension.x;
        height = dimension.y;
        depth = 0;
        vector2 = new Vector2((float)width, (float)height);
        vector3 = new Vector3((float)width, (float)height, (float)depth);
    }

    public Dimension(Vector3 dimension)
    {
        width = dimension.x;
        height = dimension.y;
        depth = dimension.z;
        vector2 = new Vector2((float)width, (float)height);
        vector3 = new Vector3((float)width, (float)height, (float)depth);
    }

    public double getWidth()
    {
        return width;
    }

    public double getHeight()
    {
        return height;
    }

    public double getDepth()
    {
        return depth;
    }

    public Vector2 toVector2()
    {
        return vector2;
    }

    public Vector3 toVector3()
    {
        return vector3;
    }

    public string toString()
    {
        return toVector3().ToString();
    }
}