using UnityEngine.Networking;


public class HTTPBuilder
{
    #region State
    UnityWebRequest unityWebRequest;
    #endregion State

    #region Methods
    //TODO:
    //Pack upload data (serializer,multipart and etc)
    //Fill upload data
    //Added callbacks(on complete, on error)
    //Unpack download data
    //


    //public async Task<>




    public HTTPBuilder()
    {
        unityWebRequest = new UnityWebRequest();
    }
    #endregion Methods
}