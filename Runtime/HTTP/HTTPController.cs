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
        static bool DeleteRequestInit<O, W>(string endpoint, out UnityWebRequest uwr, out IWorker<O, Dictionary<string, string>> worker,
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


            uwr = new UnityWebRequest($"{requestUrl}", UnityWebRequest.kHttpVerbDELETE);
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
                    ToolsDebug.Log($"Use mock for Key:{key} Value:{mocks[key]?.Substring(0, Mathf.Min(mocks[key].Length, 256))}");
                    useMock = true;
                }
                else
                {
                    ToolsDebug.Log($"Mocks for key {endpoint} or {typeof(W).ToString()} not found. Try real request.");
                }
            }


            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", $"{TokenPrefix} {token}");


            ToolsDebug.Log($"{UnityWebRequest.kHttpVerbDELETE}: {requestUrl} {uwr.GetRequestHeader("Authorization")}");

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


        static void DeleteResponseWorker<O, I>(UnityWebRequest unityWebRequest, IWorker<O, I> worker)
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
                    ToolsDebug.Log($"Response: {downloadedText?.Substring(0, Mathf.Min(downloadedText.Length, 256))}");

                    response = worker.Deserialize(downloadedText);


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



        static void UpdateMocks()
        {
            switch (MocksResource)
            {
                case MocksResource.MEMORY:
                    if (mocks == null)
                        mocks = new Dictionary<string, string>();
                    break;
                case MocksResource.FILE:
                    string filePath = Instance.mocksFilePath;
                    if (!File.Exists(filePath))
                    {
                        filePath = Path.Combine(Application.persistentDataPath, Instance.mocksFilePath);
                        if (!File.Exists(filePath))
                        {
                            filePath = Path.Combine(Application.streamingAssetsPath, Instance.mocksFilePath);
                            if (!File.Exists(filePath))
                            {
                                UnityWebRequest copyFile = new UnityWebRequest(filePath, "GET");
                                copyFile.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                                copyFile.SendWebRequest();
                                while (!copyFile.isDone) { }
                                if (!copyFile.isHttpError && !copyFile.isNetworkError)
                                {
                                    if (!string.IsNullOrEmpty(copyFile.downloadHandler.text))
                                    {
                                        File.WriteAllText(Path.Combine(Application.persistentDataPath, Instance.mocksFilePath), copyFile.downloadHandler.text);
                                        filePath = Path.Combine(Application.persistentDataPath, Instance.mocksFilePath);
                                    }

                                }
                            }
                        }
                    }


                    if (File.Exists(filePath))
                    {
                        ListKeyValueMocks mocksList = JsonUtility.FromJson<ListKeyValueMocks>(File.ReadAllText(filePath));
                        if (mocks == null) mocks = new Dictionary<string, string>();
                        foreach (var p in mocksList.mocks.ToDictionary((pair) => pair.Key, (pair) => pair.Value))
                        {
                            if (!p.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                string filePathRequest = p.Value;

                                if (!File.Exists(filePathRequest))
                                {
                                    filePathRequest = Path.Combine(Application.persistentDataPath, p.Value);
                                    if (!File.Exists(filePathRequest))
                                    {
                                        filePathRequest = Path.Combine(Application.streamingAssetsPath, p.Value);
                                        if (!File.Exists(filePathRequest))
                                        {
                                            UnityWebRequest copyFile = new UnityWebRequest(filePathRequest, "GET");
                                            copyFile.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                                            copyFile.SendWebRequest();
                                            while (!copyFile.isDone) { }
                                            if (!copyFile.isHttpError && !copyFile.isNetworkError)
                                            {
                                                File.WriteAllText(Path.Combine(Application.persistentDataPath, p.Value), copyFile.downloadHandler.text);
                                                filePathRequest = Path.Combine(Application.persistentDataPath, p.Value);
                                            }
                                        }
                                    }
                                }
                                mocks[p.Key] = File.ReadAllText(filePathRequest);
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