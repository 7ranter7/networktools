public interface IWorker<T, W>
{
    W Request { get; set; }
    void Execute(T result);
    void ErrorProcessing(string error);
}