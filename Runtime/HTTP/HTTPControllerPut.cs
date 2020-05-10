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
    /// HTTP controller for async requests. Put part.
    /// </summary>
    public partial class HTTPController : SingletonBehaviour<HTTPController>
    {
        #region Global Methods

        /// <summary>
        /// Put request with JSON data to send and receive with session token. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        /// <returns></returns>
        public static void JSONPutAuth<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null)
           where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            Instance.StartCoroutine(JSONPutRequest<O, I, W>(endpoint, param, worker, Token.Token));
        }
        /// <summary>
        /// Put request with JSON data to send and receive. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void JSONPut<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null)
            where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            Instance.StartCoroutine(JSONPutRequest<O, I, W>(endpoint, param, worker, null));
        }
        /// <summary>
        /// Put request with JSON data to send and receive with session token. Async.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void JSONPutAuthAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null)
            where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            JSONPutRequestAsync<O, I, W>(endpoint, param, worker, Token.Token);
        }
        /// <summary>
        /// Put request with JSON data to send and receive.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void JSONPutAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null)
             where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            JSONPutRequestAsync<O, I, W>(endpoint, param, worker, null);
        }

        static IEnumerator JSONPutRequest<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, string token = null)
        where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, I> workerTmp;
            if (!PutRequestInit<O, I, W>(endpoint, out uwr, out workerTmp, param, worker, token))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress + uwr.downloadProgress) / 2);
                    yield return null;
                }
            }
            workerTmp.Progress((uwr.downloadProgress));
            PostResponseWorker<O, I, W>(uwr, workerTmp);
        }

        static async void JSONPutRequestAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, string token = null)
         where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, I> workerTmp;
            if (!PutRequestInit<O, I, W>(endpoint, out uwr, out workerTmp, param, worker, token))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress));
                    await Task.Yield();
                }
            }
            workerTmp.Progress((uwr.downloadProgress));
            PostResponseWorker<O, I, W>(uwr, workerTmp);
        }



        static bool PutRequestInit<O, I, W>(string endpoint, out UnityWebRequest uwr, out IWorker<O, I> worker, I param,
                                            IWorker<O, I> workerDefault = null,
                                            string token = null)
        where W : IWorker<O, I>, new()
        where O : class
        where I : class
        {
            bool useMock = false;
            url = Instance.urlParam;
            TokenPrefix = Instance.tokenPrefix;
            string requestUrl = $"{url}/{endpoint}";
            if (endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase)) requestUrl = endpoint;

            string json = null;
            byte[] jsonToSend = new byte[1];
            if (workerDefault == null)
            {
                worker = new W();
            }
            else
            {
                worker = workerDefault;
            }
            uwr = new UnityWebRequest($"{requestUrl}", UnityWebRequest.kHttpVerbPUT);
            if (MocksResource != MocksResource.NONE)
            {
                var key = typeof(W).ToString();
                if (!mocks.ContainsKey(key))
                {
                    key = $"PUT:{endpoint}";
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
                    json = worker.Serialize(param);
                    jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

                }
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                uwr.uploadHandler.contentType = "application/json";
            }
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", $"{TokenPrefix} {token}");


            ToolsDebug.Log($"{UnityWebRequest.kHttpVerbPUT}: {requestUrl} {uwr.GetRequestHeader("Authorization")} JSONBody:{json?.Substring(0, Mathf.Min(json.Length, 256))}");


            worker.Request = param;
            worker.Start();
            return useMock;
        }



        #endregion Global Methods
    }
}
