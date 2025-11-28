public interface IStorage
{
    void Write();
    void Read();
    void Delete();
    bool Exists();
}
