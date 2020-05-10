using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Linq;
using RanterTools.Base;


namespace RanterTools.Networking
{
    /// <summary>
    /// HTTP controller for async requests. Get part.
    /// </summary>
    public partial class HTTPController : SingletonBehaviour<HTTPController>
    {
        #region Global Methods
        /// <summary>
        /// Get request with query input data to send and JSON data receive with session token. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <typeparam name="O">Output type.</typeparam>
        public static void JSONGetAuth<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
            where W : IWorker<O, Dictionary<string, string>>, new()
            where O : class
        {
            Instance.StartCoroutine(JSONGetRequest<W, O>(endpoint, query, worker, Token.Token));
        }
        /// <summary>
        /// Get request with query input data to send and JSON data receive. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <typeparam name="O">Output type.</typeparam>
        public static void JSONGet<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
            where W : IWorker<O, Dictionary<string, string>>, new()
            where O : class
        {
            Instance.StartCoroutine(JSONGetRequest<W, O>(endpoint, query, worker));
        }

        /// <summary>
        /// Get request with query input data to send and JSON data receive with session token. Async.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <typeparam name="O">Output type.</typeparam>
        public static void JSONGetAuthAsync<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
           where W : IWorker<O, Dictionary<string, string>>, new()
           where O : class
        {
            JSONGetRequestAsync<W, O>(endpoint, query, worker, Token.Token);
        }
        /// <summary>
        /// Get request with query input data to send and JSON data receive  with session token. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <typeparam name="O">Output type.</typeparam>
        public static void JSONGetAsync<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
            where W : IWorker<O, Dictionary<string, string>>, new()
            where O : class
        {
            JSONGetRequestAsync<W, O>(endpoint, query, worker);
        }

        /// <summary>
        /// Get request with query input data to send and Texture2D data receive. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void TextureGetAuth<W>(string endpoint, Dictionary<string, string> query, IWorker<Texture2D, Dictionary<string, string>> worker = null)
            where W : IWorker<Texture2D, Dictionary<string, string>>, new()
        {
            Instance.StartCoroutine(TextureGetRequest<W>(endpoint, query, worker, Token.Token));
        }
        /// <summary>
        /// Get request with query input data to send and Texture2D data receive with session token. Async.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void TextureGet<W>(string endpoint, Dictionary<string, string> query, IWorker<Texture2D, Dictionary<string, string>> worker = null)
            where W : IWorker<Texture2D, Dictionary<string, string>>, new()
        {
            Instance.StartCoroutine(TextureGetRequest<W>(endpoint, query, worker));
        }

        public static void TextureGetAuthAsync<W>(string endpoint, Dictionary<string, string> query, IWorker<Texture2D, Dictionary<string, string>> worker = null)
           where W : IWorker<Texture2D, Dictionary<string, string>>, new()
        {
            TextureGetRequestAsync<W>(endpoint, query, worker, Token.Token);
        }
        /// <summary>
        /// Get request with query input data to send and Texture2D data receive. Async.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void TextureGetAsync<W>(string endpoint, Dictionary<string, string> query, IWorker<Texture2D, Dictionary<string, string>> worker = null)
            where W : IWorker<Texture2D, Dictionary<string, string>>, new()
        {
            TextureGetRequestAsync<W>(endpoint, query, worker);
        }

        static IEnumerator JSONGetRequest<W, O>(string endpoint, Dictionary<string, string> query = null, IWorker<O, Dictionary<string, string>> worker = null, string token = null)
        where W : IWorker<O, Dictionary<string, string>>, new()
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, Dictionary<string, string>> workerTmp;
            if (!GetRequestInit<O, W>(endpoint, out uwr, out workerTmp, query, worker, token))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress + uwr.downloadProgress) / 2);
                    yield return null;
                }
            }
            GetResponseWorker<O, Dictionary<string, string>>(uwr, workerTmp);
        }

        static async void JSONGetRequestAsync<W, O>(string endpoint, Dictionary<string, string> query = null, IWorker<O, Dictionary<string, string>> worker = null, string token = null)
        where W : IWorker<O, Dictionary<string, string>>, new()
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, Dictionary<string, string>> workerTmp;
            if (!GetRequestInit<O, W>(endpoint, out uwr, out workerTmp, query, worker, token))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress));
                    await Task.Yield();
                }
            }
            workerTmp.Progress((uwr.downloadProgress));
            GetResponseWorker<O, Dictionary<string, string>>(uwr, workerTmp);
        }

        static IEnumerator TextureGetRequest<W>(string endpoint, Dictionary<string, string> query = null, IWorker<Texture2D, Dictionary<string, string>> worker = null, string token = null)
       where W : IWorker<Texture2D, Dictionary<string, string>>, new()
        {
            UnityWebRequest uwr;
            IWorker<Texture2D, Dictionary<string, string>> workerTmp;
            GetRequestInit<Texture2D, W>(endpoint, out uwr, out workerTmp, query, worker, token);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                workerTmp.Progress((uwr.uploadProgress + uwr.downloadProgress) / 2);
                yield return null;
            }

            GetResponseWorker<Texture2D, Dictionary<string, string>>(uwr, workerTmp);
        }

        static async void TextureGetRequestAsync<W>(string endpoint, Dictionary<string, string> query = null, IWorker<Texture2D, Dictionary<string, string>> worker = null, string token = null)
        where W : IWorker<Texture2D, Dictionary<string, string>>, new()
        {
            UnityWebRequest uwr;
            IWorker<Texture2D, Dictionary<string, string>> workerTmp;
            GetRequestInit<Texture2D, W>(endpoint, out uwr, out workerTmp, query, worker, token);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                workerTmp.Progress((uwr.uploadProgress));
                await Task.Yield();
            }

            workerTmp.Progress((uwr.downloadProgress));
            GetResponseWorker<Texture2D, Dictionary<string, string>>(uwr, workerTmp);
        }


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
            if (endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase)) requestUrl = endpoint;
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
                        key = $"GET:{endpoint}";
                    }
                    if (mocks.ContainsKey(key))
                    {
                        uwr = UnityWebRequestTexture.GetTexture($"{mocks[key]}");
                        ToolsDebug.Log($"Use mock for texture. Key:{key} Value:{mocks[key]?.Substring(0, Mathf.Min(mocks[key].Length, 256))}");
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
                        key = $"GET:{endpoint}";
                    }
                    if (mocks.ContainsKey(key))
                    {
                        ToolsDebug.Log($"Use mock for Key:{key} Value:{mocks[key]?.Substring(0, Mathf.Min(mocks[key].Length, 256))}");
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

        static void GetResponseWorker<O, I>(UnityWebRequest unityWebRequest, IWorker<O, I> worker)
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
                                key = $"GET:{unityWebRequest.url.Replace($"{url}/", "")}";
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
        #endregion Global Methods
    }
}