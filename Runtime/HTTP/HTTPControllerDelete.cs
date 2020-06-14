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
        public static void JSONDeleteAuth<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
            where W : IWorker<O, Dictionary<string, string>>, new()
            where O : class
        {
            Instance.StartCoroutine(JSONDeleteRequest<W, O>(endpoint, query, worker, Token.Token));
        }
        /// <summary>
        /// Get request with query input data to send and JSON data receive. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <typeparam name="O">Output type.</typeparam>
        public static void JSONDelete<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
            where W : IWorker<O, Dictionary<string, string>>, new()
            where O : class
        {
            Instance.StartCoroutine(JSONDeleteRequest<W, O>(endpoint, query, worker));
        }

        /// <summary>
        /// Get request with query input data to send and JSON data receive with session token. Async.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <typeparam name="O">Output type.</typeparam>
        public static void JSONDeleteAuthAsync<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
           where W : IWorker<O, Dictionary<string, string>>, new()
           where O : class
        {
            JSONDeleteRequestAsync<W, O>(endpoint, query, worker, Token.Token);
        }
        /// <summary>
        /// Get request with query input data to send and JSON data receive  with session token. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="query">Query parameters.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <typeparam name="O">Output type.</typeparam>
        public static void JSONDeleteAsync<W, O>(string endpoint, Dictionary<string, string> query, IWorker<O, Dictionary<string, string>> worker = null)
            where W : IWorker<O, Dictionary<string, string>>, new()
            where O : class
        {
            JSONDeleteRequestAsync<W, O>(endpoint, query, worker);
        }



        static IEnumerator JSONDeleteRequest<W, O>(string endpoint, Dictionary<string, string> query = null, IWorker<O, Dictionary<string, string>> worker = null, string token = null)
        where W : IWorker<O, Dictionary<string, string>>, new()
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, Dictionary<string, string>> workerTmp;
            if (!DeleteRequestInit<O, W>(endpoint, out uwr, out workerTmp, query, worker, token))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress + uwr.downloadProgress) / 2);
                    yield return null;
                }
            }
            DeleteResponseWorker<O, Dictionary<string, string>>(uwr, workerTmp);
        }

        static async void JSONDeleteRequestAsync<W, O>(string endpoint, Dictionary<string, string> query = null, IWorker<O, Dictionary<string, string>> worker = null, string token = null)
        where W : IWorker<O, Dictionary<string, string>>, new()
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, Dictionary<string, string>> workerTmp;
            if (!DeleteRequestInit<O, W>(endpoint, out uwr, out workerTmp, query, worker, token))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress));
                    await Task.Yield();
                }
            }
            workerTmp.Progress((uwr.downloadProgress));
            DeleteResponseWorker<O, Dictionary<string, string>>(uwr, workerTmp);
        }

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
            if (endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase)) requestUrl = endpoint;
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
                    key = $"DELETE:{endpoint}";
                }
                if (mocks.ContainsKey(key))
                {
                    ToolsDebug.Log($"Use mock for Key:{key} Value:{mocks[key]?.Substring(0, Mathf.Min(mocks[key].Length, Instance.logLimit))}");
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
                            key = $"DELETE:{unityWebRequest.url.Replace($"{url}/", "")}";
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
                    ToolsDebug.Log($"Response: {downloadedText?.Substring(0, Mathf.Min(downloadedText.Length, Instance.logLimit))}");

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

        #endregion Global Methods
    }
}
