namespace VietnamTrafficPolice.WebApi.Configurations;

public class TesseractEngineOptions
{
    public const string Section = "TesseractOptions";

    public required string DirectoryPath { get; init; }
    public required string StemPath { get; init; }

    public void Deconstruct(out string directoryPath, out string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(DirectoryPath);
        ArgumentException.ThrowIfNullOrEmpty(StemPath);
        directoryPath = DirectoryPath;
        fileName = StemPath;
    }
}