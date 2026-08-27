public class MathUtil
{
    /// <summary>
    /// A quick Clamp method that I got from Trap (userid: 7839) on StackOverflow.
    /// </summary>
    /// <param name="value">Value to be clamped.</param>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum Value.</param>
    /// <returns>Value within range.</returns>
    public static int Clamp(int value, int min, int max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }
}