public class FileSizeHelper {
    private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB" };

    /// <summary>
    /// Formats a byte number by it's relative suffix.
    /// </summary>
    /// <param name="size">File size in bytes.</param>
    /// <returns>A string that is the amount + suffix.</returns>
    public static string GetReadableFileSize(long size) {
        int unitIndex = 0;
        double readableSize = (double)size;

        while (readableSize >= 1024 && unitIndex < Units.Length - 1) {
            readableSize /= 1024;
            unitIndex++;
        }

        return $"{readableSize:0.#} {Units[unitIndex]}";
    }
}