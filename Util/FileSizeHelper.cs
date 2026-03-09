public class FileSizeHelper
{
    private static readonly string[] Units = {"B", "KB", "MB", "GB", "TB", "PB"};
    public static string GetReadableFileSize(long size)
    {
        int unitIndex = 0;
        double readableSize = (double) size;

        while (readableSize >= 1024 && unitIndex < Units.Length - 1)
        {
            readableSize /= 1024;
            unitIndex++;
        }

        return $"{readableSize:0.#} {Units[unitIndex]}";
    }
}