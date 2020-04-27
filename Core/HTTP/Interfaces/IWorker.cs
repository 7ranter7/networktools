

public interface IWorker<O, I>
{
    I Request { get; set; }
    void Execute(O result);
    void ErrorProcessing(long code, string error);

    void Progress(float progress);
}