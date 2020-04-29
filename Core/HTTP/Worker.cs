using UnityEngine;
namespace RanterTools.Networking.Examples
{

    public class Worker<O, I> : IWorker<O, I>
    {
        public I Request { get; set; }
        public virtual void Start() { }
        public virtual void Execute(O result) { }
        public virtual void ErrorProcessing(long code, string message) { }
        public virtual void Progress(float progress) { }
        public virtual string Serialize(I obj) { return JsonUtility.ToJson(obj); }
        public virtual O Deserialize(string obj) { return JsonUtility.FromJson<O>(obj); }
    }
}