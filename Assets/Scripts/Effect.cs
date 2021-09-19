public abstract class Effect<T>
{
    private T entity;

    public Effect()
    {
        entity = default;
    }

    public void bindTo(T entity)
    {
        this.entity = entity;
    }

    public abstract Robot applyTo(Robot robot, bool effects);
}