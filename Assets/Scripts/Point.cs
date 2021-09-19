using System;
using UnityEngine;

public class Point
{
    private double _x, _y, _z;
    public double x {
        get { return _x; }
        set {
            _x = value;
            vector2.x = (float)value;
            vector3.x = (float)value;
            update();
        }
    }
    public double y
    {
        get { return _y; }
        set
        {
            _y = value;
            vector2.y = (float)value;
            vector3.y = (float)value;
            update();
        }
    }
    public double z
    {
        get { return _z; }
        set
        {
            _z = value;
            vector3.z = (float)value;
            update();
        }
    }
    private Vector2 vector2;
    private Vector3 vector3;
    private float magnitude;
    private Point roundedPoint;
    private readonly bool ROUNDED;

    public Point()
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        x = 0;
        y = 0;
        z = 0;
        vector2 = Vector2.zero;
        vector3 = Vector3.zero;
        magnitude = 0;
    }

    public Point(bool rounded)
    {
        ROUNDED = rounded;
        x = 0;
        y = 0;
        z = 0;
        vector2 = Vector2.zero;
        vector3 = Vector3.zero;
        magnitude = 0;
    }

    public Point(double x, double y)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        this.x = x;
        this.y = y;
        z = 0;
        vector2 = new Vector2((float)this.x, (float)this.y);
        vector3 = new Vector3((float)this.x, (float)this.y, (float)z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(float x, float y)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        this.x = x;
        this.y = y;
        z = 0;
        vector2 = new Vector2((float)this.x, (float)this.y);
        vector3 = new Vector3((float)this.x, (float)this.y, (float)z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(int x, int y)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        this.x = x;
        this.y = y;
        z = 0;
        vector2 = new Vector2((float)this.x, (float)this.y);
        vector3 = new Vector3((float)this.x, (float)this.y, (float)z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(double x, double y, double z)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        this.x = x;
        this.y = y;
        this.z = z;
        vector2 = new Vector2((float)this.x, (float)this.y);
        vector3 = new Vector3((float)this.x, (float)this.y, (float)this.z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(double x, double y, double z, bool rounded)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        this.x = x;
        this.y = y;
        this.z = z;
        vector2 = new Vector2((float)this.x, (float)this.y);
        vector3 = new Vector3((float)this.x, (float)this.y, (float)this.z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(float x, float y, float z)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        this.x = x;
        this.y = y;
        this.z = z;
        vector2 = new Vector2((float)this.x, (float)this.y);
        vector3 = new Vector3((float)this.x, (float)this.y, (float)this.z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(int x, int y, int z)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        this.x = x;
        this.y = y;
        this.z = z;
        vector2 = new Vector2((float)this.x, (float)this.y);
        vector3 = new Vector3((float)this.x, (float)this.y, (float)this.z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(Vector2 point)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        x = point.x;
        y = point.y;
        z = 0;
        vector2 = new Vector2((float)x, (float)y);
        vector3 = new Vector3((float)x, (float)y, (float)z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    public Point(Vector3 point)
    {
        ROUNDED = false;
        roundedPoint = new Point(true);
        x = point.x;
        y = point.y;
        z = point.z;
        vector2 = new Vector2((float)x, (float)y);
        vector3 = new Vector3((float)x, (float)y, (float)z);
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    private void update()
    {
        magnitude = vector3.magnitude;
        updateRoundedPoint();
    }

    private void updateRoundedPoint()
    {
        if (!ROUNDED)
        {
            double xRounded = Math.Round(x, 1);
            double yRounded = Math.Round(y, 1);
            double zRounded = Math.Round(z, 1);
            if (x < 0)
                xRounded *= -1;
            if (y < 0)
                yRounded *= -1;
            if (z < 0)
                zRounded *= -1;
            if ((xRounded + .5) > ((int)xRounded + 1))
                xRounded = (int)xRounded + .5;
            else xRounded = (int)xRounded;
            if ((yRounded + .5) > ((int)yRounded + 1))
                yRounded = (int)yRounded + .5;
            else yRounded = (int)yRounded;
            if ((zRounded + .5) > ((int)zRounded + 1))
                zRounded = (int)zRounded + .5;
            else zRounded = (int)zRounded;
            if (x < 0)
                xRounded *= -1;
            if (y < 0)
                yRounded *= -1;
            if (z < 0)
                zRounded *= -1;
            roundedPoint.x = xRounded;
            roundedPoint.y = yRounded;
            roundedPoint.z = zRounded;
        }
    }

    public double getX()
    {
        return x;
    }

    public double getY()
    {
        return y;
    }

    public double getZ()
    {
        return z;
    }

    public Vector2 toVector2()
    {
        return vector2;
    }

    public Vector3 toVector3()
    {
        return vector3;
    }

    public float getMagnitude()
    {
        return magnitude;
    }

    public Point round()
    {
        return roundedPoint;
    }

    public string toString()
    {
        return toVector3().ToString();
    }
}