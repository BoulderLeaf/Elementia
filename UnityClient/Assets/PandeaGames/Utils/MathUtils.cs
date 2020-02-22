public static class MathUtils
{
    public static float Round(float value, float unit)
    {
        float remainder = value % unit;
        int units = (int) (value / unit);
        return remainder < unit ? units * unit : units * unit + unit;
    }
}
