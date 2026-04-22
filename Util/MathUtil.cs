public class MathUtil
{
    // Credits to Trap (7839) on StackOverflow. 
    public static int Clamp(int value, int min, int max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }
}