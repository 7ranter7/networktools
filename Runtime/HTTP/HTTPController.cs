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

        public static Dictionary<string, string> ParseQueryString(string query, Encoding encoding = null)
        {
            Dictionary<string, string> result = null;
            if (query.Length == 0)
                return result;
            result = new Dictionary<string, string>();
            if (encoding == null) encoding = Encoding.UTF8;
            var decodedLength = query.Length;
            var namePos = 0;
            var first = true;

            while (namePos <= decodedLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (var q = namePos; q < decodedLength; q++)
                {
                    if ((valuePos == -1) && (query[q] == '='))
                    {
                        valuePos = q + 1;
                    }
                    else if (query[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                if (first)
                {
                    first = false;
                    if (query[namePos] == '?')
                        namePos++;
                }

                string name;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = UnityWebRequest.UnEscapeURL(query.Substring(namePos, valuePos - namePos - 1));
                }
                if (valueEnd < 0)
                {
                    namePos = -1;
                    valueEnd = query.Length;
                }
                else
                {
                    namePos = valueEnd + 1;
                }
                var value = UnityWebRequest.UnEscapeURL(query.Substring(valuePos, valueEnd - valuePos));

                result.Add(name, value);
                if (namePos == -1)
                    break;
            }
            return result;
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

        [SerializeField]
        int logLimit = 128;
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