namespace Backend.Ingestion;

public interface IChunker
{
    IReadOnlyList<Chunk> Split(string text);
}