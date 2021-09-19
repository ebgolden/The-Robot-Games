using System.Collections.Generic;

public class Carousel<T> : List<T>
{
    private int selected;

    public Carousel()
    {
        selected = 0;
    }

    public T Selected()
    {
        if (base.Count == 0)
            return default;
        return base[selected];
    }

    public T Next()
    {
        if (base.Count == 0)
            return default;
        if (++selected >= base.Count)
            selected = 0;
        return Selected();
    }

    public T Previous()
    {
        if (base.Count == 0)
            return default;
        if (--selected < 0)
            selected = base.Count - 1;
        return Selected();
    }

    public T Remove()
    {
        if (base.Count == 0)
            return default;
        T removed = Selected();
        base.RemoveAt(selected);
        if (selected >= base.Count)
            --selected;
        return removed;
    }
}