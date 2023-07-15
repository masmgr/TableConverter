namespace TableConverter;

public static class LinqExtend
{
    public static IEnumerable<IEnumerable<string>> Chunk(this IEnumerable<string> source, int chunkSize)
    {
        while (source.Any())
        {
            yield return source.Take(chunkSize);
            source = source.Skip(chunkSize);
        }
    }
}