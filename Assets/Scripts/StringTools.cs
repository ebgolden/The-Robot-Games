public static class StringTools
{
    public static string formatString(double doubleToFormat)
    {
        return (doubleToFormat.ToString().Contains(".") ? doubleToFormat.ToString("#.00") : doubleToFormat.ToString());
    }
}