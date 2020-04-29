

public interface IWorker<O, I>
{
    I Request { get; set; }
    void Start();
    void Execute(O result);
    void ErrorProcessing(long code, string error);
    void Progress(float progress);
    string Serialize(I obj);
    O Deserialize(string obj);
}