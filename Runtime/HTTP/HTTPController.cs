using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Linq;
using RanterTools.Base;
//using Newtonsoft.Json;

namespace RanterTools.Networking
{

    /// <summary>
    /// HTTP controller for async requests.
    /// </summary>
    public partial class HTTPController : SingletonBehaviour<HTTPController>
    {
        #region Global State
        public static Dictionary<string, string> mocks;

        static LocalToken token;
        /// <summary>
        /// Current access token.
        /// </summary>
        /// <value>Return current access token if it exists.</value>
        public static LocalToken Token
        {
            get
            {
                if (!TokenRead)
                {
                    if (PlayerPrefs.HasKey("Token")) token = JsonUtility.FromJson<LocalToken>(PlayerPrefs.GetString("Token", null));
                    TokenRead = true;
                }
                return token;
            }
            set
            {
                token = value;
                if (token == null) PlayerPrefs.DeleteKey("Token");
                else
                    PlayerPrefs.SetString("Token", JsonUtility.ToJson(token));
            }
        }

        /// <summary>
        /// If token read from PlayerPrefs is true. Set false for one more trying.
        /// </summary>
        /// <value>If token read from PlayerPrefs is true.</value>
        public static bool TokenRead { get; set; } = false;


        static MocksResource mocksResource = MocksResource.NONE;
        /// <summary>
        /// Resource for morks.
        /// </summary>
        /// <value>None if mocks isn't used.</value>
        public static MocksResource MocksResource { get { return mocksResource; } set { mocksResource = value; UpdateMocks(); } }

        static string url = "http://localhost/";
        static string TokenPrefix = "JWT";
        #endregion Global State


        #region Global Methods
        static bool GetRequestInit<O, W>(string endpoint, out UnityWebRequest uwr, out IWorker<O, Dictionary<string, string>> worker,
                                        Dictionary<string, string> query = null, IWorker<O, Dictionary<string, string>> workerDefault = null,
                                        string token = null)
        where W : IWorker<O, Dictionary<string, string>>, new()
        where O : class
        {
            bool useMock = false;
            url = Instance.urlParam;
            TokenPrefix = Instance.tokenPrefix;
            string requestUrl = $"{url}/{ endpoint}";
            byte[] queryUrlString = null;
            if (query != null)
            {
                queryUrlString = UnityWebRequest.SerializeSimpleForm(query);
                requestUrl = $"{requestUrl}?{Encoding.UTF8.GetString(queryUrlString)}";
            }

            if (typeof(O) == typeof(Texture2D))
            {
                if (MocksResource == MocksResource.NONE)
                    uwr = UnityWebRequestTexture.GetTexture($"{requestUrl}");
                else
                {
                    var key = typeof(W).ToString();
                    if (!mocks.ContainsKey(key))
                    {
                        key = endpoint;
                    }
                    if (mocks.ContainsKey(key))
                    {
                        uwr = UnityWebRequestTexture.GetTexture($"{mocks[key]}");
                        ToolsDebug.Log($"Use mock for texture. Key:{key} Value:{mocks[key]}");
                        useMock = true;
                    }
                    else
                    {
                        ToolsDebug.Log($"Mocks for key {key} or {key} not found. Try real request.");
                        uwr = UnityWebRequestTexture.GetTexture($"{requestUrl}");
                    }

                }
            }
            else
            {
                uwr = new UnityWebRequest($"{requestUrl}", UnityWebRequest.kHttpVerbGET);
                uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                if (MocksResource != MocksResource.NONE)
                {
                    var key = typeof(W).ToString();
                    if (!mocks.ContainsKey(key))
                    {
                        key = endpoint;
                    }
                    if (mocks.ContainsKey(key))
                    {
                        ToolsDebug.Log($"Use mock for Key:{key} Value:{mocks[key]}");
                        useMock = true;
                    }
                    else
                    {
                        ToolsDebug.Log($"Mocks for key {key} or {key} not found. Try real request.");
                    }
                }

            }
            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", $"{TokenPrefix} {token}");


            ToolsDebug.Log($"{UnityWebRequest.kHttpVerbGET}: {requestUrl} {uwr.GetRequestHeader("Authorization")}");

            if (workerDefault == null)
            {
                worker = new W();
            }
            else
            {
                worker = workerDefault;
            }
            worker.Request = query;
            worker.Start();
            return useMock;
        }

        static void GetResponseWorker<O, I>(UnityWebRequest unityWebRequest, IWorker<O, I> worker, Func<string, O> serializer = null)
        where O : class
        where I : class
        {
            if (unityWebRequest.isNetworkError || !string.IsNullOrEmpty(unityWebRequest.error))
            {
                if (unityWebRequest.responseCode != 400)
                    worker.ErrorProcessing(unityWebRequest.responseCode, unityWebRequest.error);
                else
                {
                    string error = "";
                    if (unityWebRequest?.downloadHandler?.text != null) error += unityWebRequest.downloadHandler.text;
                    worker.ErrorProcessing(unityWebRequest.responseCode, error);
                }
            }
            else
            {
                try
                {
                    O response;
                    if (typeof(O) == typeof(Texture2D))
                    {
                        Texture2D image = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                        image = DownloadHandlerTexture.GetContent(unityWebRequest);
                        if (image == null)
                            response = default(O);
                        else
                            response = image as O;
                    }
                    else
                    {
                        string downloadedText;
                        if (MocksResource == MocksResource.NONE) downloadedText = unityWebRequest.downloadHandler.text;
                        else
                        {
                            var key = worker.GetType().ToString();
                            if (!mocks.ContainsKey(key))
                            {
                                key = unityWebRequest.url.Replace($"{url}/", "");
                            }
                            if (mocks.ContainsKey(key))
                            {
                                downloadedText = mocks[key];
                                ToolsDebug.Log($"Use mock with key: {key}");
                            }
                            else
                            {
                                ToolsDebug.Log($"Mocks for key {unityWebRequest.url.Replace($"{url}/", "")} or {worker.GetType().ToString()} not found. Try real request.");
                                downloadedText = unityWebRequest.downloadHandler.text;
                            }
                        }
                        ToolsDebug.Log($"Response: {downloadedText}");
                        if (serializer == null)
                            response = JsonUtility.FromJson<O>(downloadedText);
                        else response = serializer(downloadedText);
                    }

                    if (response != null)
                    {
                        worker.Execute(response);
                    }
                    else
                    {
                        worker.ErrorProcessing(unityWebRequest.responseCode, "Unknown Error");
                    }
                }
                catch (ArgumentException)
                {
                    ToolsDebug.Log(unityWebRequest.downloadHandler.text);
                }
            }
        }

        static bool PostRequestInit<O, I, W>(string endpoint, out UnityWebRequest uwr, out IWorker<O, I> worker, I param,
                                            IWorker<O, I> workerDefault = null,
                                            string token = null, Func<I, string> serializer = null)
        where W : IWorker<O, I>, new()
        where O : class
        where I : class
        {
            bool useMock = false;
            url = Instance.urlParam;
            TokenPrefix = Instance.tokenPrefix;
            string requestUrl = $"{url}/{endpoint}";

            string json = null;
            byte[] jsonToSend = new byte[1];

            uwr = new UnityWebRequest($"{requestUrl}", UnityWebRequest.kHttpVerbPOST);
            if (MocksResource != MocksResource.NONE)
            {
                var key = typeof(W).ToString();
                if (!mocks.ContainsKey(key))
                {
                    key = endpoint;
                }
                if (mocks.ContainsKey(key))
                {
                    ToolsDebug.Log($"Use mock for Key:{key} Value:{mocks[key]}");
                    useMock = true;
                }
                else
                {
                    ToolsDebug.Log($"Mocks for key {key} or {key} not found. Try real request.");
                }
            }

            if (typeof(I) == typeof(Texture2D))
            {
                Texture2D sendTexture = param as Texture2D;
                json = "Texture2D";
                jsonToSend = ImageConversion.EncodeToPNG(sendTexture);
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                uwr.SetRequestHeader("Content-Type", "image/png");
            }
            else
            {
                if (param != null)
                {
                    if (serializer == null)
                        json = JsonUtility.ToJson(param);
                    else json = serializer(param);
                    jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

                }
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                uwr.uploadHandler.contentType = "application/json";
            }
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", $"{TokenPrefix} {token}");


            ToolsDebug.Log($"{UnityWebRequest.kHttpVerbPOST}: {requestUrl} {uwr.GetRequestHeader("Authorization")} JSONBody:{json}");

            if (workerDefault == null)
            {
                worker = new W();
            }
            else
            {
                worker = workerDefault;
            }
            worker.Request = param;
            worker.Start();
            return useMock;
        }

        static void PostResponseWorker<O, I, W>(UnityWebRequest unityWebRequest, IWorker<O, I> worker, Func<string, O> serializer = null)
        where O : class
        {
            if (unityWebRequest.isNetworkError || !string.IsNullOrEmpty(unityWebRequest.error))
            {
                if (unityWebRequest.responseCode != 400)
                    worker.ErrorProcessing(unityWebRequest.responseCode, unityWebRequest.error);
                else
                {
                    string error = "";
                    if (unityWebRequest?.downloadHandler?.text != null) error += unityWebRequest.downloadHandler.text;
                    worker.ErrorProcessing(unityWebRequest.responseCode, error);
                }
            }
            else
            {
                try
                {
                    O response;
                    string downloadedText;
                    if (MocksResource == MocksResource.NONE) downloadedText = unityWebRequest.downloadHandler.text;
                    else
                    {
                        var key = worker.GetType().ToString();
                        if (!mocks.ContainsKey(key))
                        {
                            key = unityWebRequest.url.Replace($"{url}/", "");
                        }
                        if (mocks.ContainsKey(key))
                        {
                            downloadedText = mocks[key];
                            ToolsDebug.Log($"Use mock with key: {key}");
                        }
                        else
                        {
                            ToolsDebug.Log($"Mocks for key {unityWebRequest.url.Replace($"{url}/", "")} or {worker.GetType().ToString()} not found. Try real request.");
                            downloadedText = unityWebRequest.downloadHandler.text;
                        }
                    }
                    ToolsDebug.Log($"Response: {downloadedText}");
                    if (serializer == null)
                        response = JsonUtility.FromJson<O>(downloadedText);
                    else response = serializer(downloadedText);
                    if (response != null)
                    {
                        worker.Execute(response);
                    }
                    else
                    {
                        worker.ErrorProcessing(unityWebRequest.responseCode, "Unknown Error");
                    }
                }
                catch (ArgumentException)
                {
                    ToolsDebug.Log(unityWebRequest.downloadHandler.text);
                }
            }
        }

        static bool PutRequestInit<O, I, W>(string endpoint, out UnityWebRequest uwr, out IWorker<O, I> worker, I param,
                                            IWorker<O, I> workerDefault = null,
                                            string token = null, Func<I, string> serializer = null)
        where W : IWorker<O, I>, new()
        where O : class
        where I : class
        {
            bool useMock = false;
            url = Instance.urlParam;
            TokenPrefix = Instance.tokenPrefix;
            string requestUrl = $"{url}/{endpoint}";

            string json = null;
            byte[] jsonToSend = new byte[1];

            uwr = new UnityWebRequest($"{requestUrl}", UnityWebRequest.kHttpVerbPUT);
            if (MocksResource != MocksResource.NONE)
            {
                var key = typeof(W).ToString();
                if (!mocks.ContainsKey(key))
                {
                    key = endpoint;
                }
                if (mocks.ContainsKey(key))
                {
                    ToolsDebug.Log($"Use mock for Key:{key} Value:{mocks[key]}");
                    useMock = true;
                }
                else
                {
                    ToolsDebug.Log($"Mocks for key {key} or {key} not found. Try real request.");
                }
            }

            if (typeof(I) == typeof(Texture2D))
            {
                Texture2D sendTexture = param as Texture2D;
                json = "Texture2D";
                jsonToSend = ImageConversion.EncodeToPNG(sendTexture);
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                uwr.SetRequestHeader("Content-Type", "image/png");
            }
            else
            {
                if (param != null)
                {
                    if (serializer == null)
                        json = JsonUtility.ToJson(param);
                    else json = serializer(param);
                    jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

                }
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                uwr.uploadHandler.contentType = "application/json";
            }
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", $"{TokenPrefix} {token}");


            ToolsDebug.Log($"{UnityWebRequest.kHttpVerbPUT}: {requestUrl} {uwr.GetRequestHeader("Authorization")} JSONBody:{json}");

            if (workerDefault == null)
            {
                worker = new W();
            }
            else
            {
                worker = workerDefault;
            }
            worker.Request = param;
            worker.Start();
            return useMock;
        }


        static void UpdateMocks()
        {
            switch (MocksResource)
            {
                case MocksResource.MEMORY:
                    if (mocks == null)
                        mocks = new Dictionary<string, string>();
                    break;
                case MocksResource.FILE:

                    if (File.Exists(Instance.mocksFilePath))
                    {
                        ListKeyValueMocks mocksList = JsonUtility.FromJson<ListKeyValueMocks>(File.ReadAllText(Instance.mocksFilePath));
                        mocks = mocksList.mocks.ToDictionary((pair) => pair.Key, (pair) => pair.Value);
                        foreach (var p in mocks)
                        {
                            if (!p.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                mocks[p.Key] = File.ReadAllText(p.Value);
                            }
                        }
                    }
                    else
                    {
                        ToolsDebug.Log($"Can't read mock data file from: {Instance.mocksFilePath}");
                        MocksResource = MocksResource.NONE;
                    }
                    break;
                case MocksResource.REMOTE_FILE:
                    UnityWebRequest unityWebRequest = new UnityWebRequest(Instance.mocksFilePath, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
                    unityWebRequest.SendWebRequest().completed += (dh) =>
                      {
                          if (!unityWebRequest.isNetworkError && !unityWebRequest.isHttpError && !string.IsNullOrEmpty(unityWebRequest.downloadHandler?.text))
                          {
                              ListKeyValueMocks mocksList = JsonUtility.FromJson<ListKeyValueMocks>(unityWebRequest.downloadHandler?.text);
                              mocks = mocksList.mocks.ToDictionary((pair) => pair.Key, (pair) => pair.Value);
                              foreach (var p in mocks)
                              {
                                  if (p.Value.EndsWith("json", StringComparison.OrdinalIgnoreCase))
                                  {

                                      UnityWebRequest uwr = new UnityWebRequest(p.Value, UnityWebRequest.kHttpVerbGET, new DownloadHandlerBuffer(), null);
                                      uwr.SendWebRequest().completed += (dhh) =>
                                       {
                                           if (!uwr.isNetworkError && !uwr.isHttpError && !string.IsNullOrEmpty(uwr.downloadHandler?.text))
                                           {
                                               Debug.Log($"WTF {uwr.downloadHandler?.text}");
                                               mocks[p.Key] = uwr.downloadHandler?.text;
                                           }
                                       };
                                  }
                              }
                          }
                          else
                          {
                              ToolsDebug.Log($"Can't read mock data remote file from: {Instance.mocksFilePath}");
                              MocksResource = MocksResource.NONE;
                          }
                      };
                    break;
                default:
                    mocks = null;
                    break;
            }
        }

        #endregion Global Methods







        #region Parameters
        [Tooltip("Base URL")]
        [SerializeField]
        string urlParam = "http://localhost/";
        [SerializeField]
        string tokenPrefix = "JWT";
        [SerializeField]
        MocksResource mocksResourceParam;
        [SerializeField]
        [Tooltip("Used for file mock or remote file mock.")]
        public string mocksFilePath;
        #endregion Parameters

        #region Unity
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            MocksResource = mocksResourceParam;
        }
        #endregion Unity
    }

    public enum MocksResource { MEMORY = 1, FILE = 2, REMOTE_FILE = 3, NONE = 0 }

    [Serializable]
    public class LocalToken
    {
        public string Token;
        public DateTime ReceivedDate;
    }
    [Serializable]
    public class ListKeyValueMocks
    {
        public List<KeyValuePair> mocks;
    }
    [Serializable]
    public class KeyValuePair
    {
        public string Key;
        public string Value;
    }

}