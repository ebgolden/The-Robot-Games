using System.Collections.Generic;
using UnityEngine;

public abstract class Influencer
{
    public readonly GameObject GAME_OBJECT;
    protected Point position;
    protected Dimension size;
    protected Shape.SHAPES shape;
    private readonly double DEFAULT_HEIGHT = Robot.SCALE / 1.5;

    public Influencer(Dimension size, Shape.SHAPES shape)
    {
        this.shape = shape;
        if (this.shape == Shape.SHAPES.RECTANGULAR_PRISM || this.shape == Shape.SHAPES.PYRAMID)
            GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Cube);
        else if (this.shape == Shape.SHAPES.HEMISPHERE)
            GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        else if (this.shape == Shape.SHAPES.CYLINDER)
            GAME_OBJECT = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        this.position = new Point();
        GAME_OBJECT.AddComponent<Rigidbody>();
        GAME_OBJECT.transform.position = this.position.toVector3();
        GAME_OBJECT.GetComponent<Collider>().isTrigger = true;
        if (this.shape != Shape.SHAPES.PYRAMID)
        {
            if (size.height == 0)
                size.height = DEFAULT_HEIGHT;
            else if (size.height == -1)
                size.height = Field.FIELD_HEIGHT;
            this.size = size;
            Vector3 obstacleSize = GAME_OBJECT.GetComponent<Collider>().bounds.size;
            Vector3 rescale = GAME_OBJECT.transform.localScale;
            rescale.x = (float)this.size.width / GAME_OBJECT.transform.lossyScale.x;
            rescale.y = (float)this.size.height / GAME_OBJECT.transform.lossyScale.y;
            rescale.z = (float)this.size.depth / GAME_OBJECT.transform.lossyScale.z;
            GAME_OBJECT.transform.localScale = rescale;
        }
    }

    public void setSize(Dimension size)
    {
        this.size = size;
    }

    public Dimension getSize()
    {
        return size;
    }

    public void setPosition(Point position)
    {
        this.position = position;
        GAME_OBJECT.transform.position = position.toVector3();
    }

    public Point getPosition()
    {
        return position;
    }

    public abstract List<Collider> getCollidersTouching();

    public abstract Robot triggerEffect(Robot robot);
}