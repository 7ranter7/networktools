﻿using UnityEngine;
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
    /// HTTP controller for async requests. Post part.
    /// </summary>
    public partial class HTTPController : SingletonBehaviour<HTTPController>
    {
        #region Global Methods
        /// <summary>
        /// Post request with JSON data to send and receive with session token. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void JSONPostAuth<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, params IMultipartFormSection[] parts)
           where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            Instance.StartCoroutine(JSONPostRequest<O, I, W>(endpoint, param, worker, Token.Token, parts));
        }
        /// <summary>
        /// Post request with JSON data to send and receive. Coroutine.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void JSONPost<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, params IMultipartFormSection[] parts)
            where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            Instance.StartCoroutine(JSONPostRequest<O, I, W>(endpoint, param, worker, null, parts));
        }
        /// <summary>
        /// Post request with JSON data to send and receive with session token. Async.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void JSONPostAuthAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, params IMultipartFormSection[] parts)
            where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            JSONPostRequestAsync<O, I, W>(endpoint, param, worker, Token.Token, parts);
        }
        /// <summary>
        /// Post request with JSON data to send and receive. Async.
        /// </summary>
        /// <param name="endpoint">Endpoint.</param>
        /// <param name="param">Data for send.</param>
        /// <param name="worker">Custom request worker.</param>
        /// <param name="serializer">Custom data serializer.</param>
        /// <param name="deserializer">Custom data deserializer.</param>
        /// <typeparam name="O">Output data type.</typeparam>
        /// <typeparam name="I">Input data type.</typeparam>
        /// <typeparam name="W">Worker type.</typeparam>
        public static void JSONPostAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, params IMultipartFormSection[] parts)
             where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            JSONPostRequestAsync<O, I, W>(endpoint, param, worker, null, parts);
        }

        static IEnumerator JSONPostRequest<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, string token = null, params IMultipartFormSection[] parts)
        where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, I> workerTmp;
            if (!PostRequestInit<O, I, W>(endpoint, out uwr, out workerTmp, param, worker, token))
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

        static async void JSONPostRequestAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, string token = null, params IMultipartFormSection[] parts)
         where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, I> workerTmp;
            if (!PostRequestInit<O, I, W>(endpoint, out uwr, out workerTmp, param, worker, token, parts))
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



        static bool PostRequestInit<O, I, W>(string endpoint, out UnityWebRequest uwr, out IWorker<O, I> worker, I param,
                                            IWorker<O, I> workerDefault = null,
                                            string token = null, params IMultipartFormSection[] parts)
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
            uwr = new UnityWebRequest($"{requestUrl}", UnityWebRequest.kHttpVerbPOST);
            if (MocksResource != MocksResource.NONE)
            {
                var key = typeof(W).ToString();
                if (!mocks.ContainsKey(key))
                {
                    key = $"POST:{endpoint}";
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
                    json = worker.Serialize<I>(param);
                    jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
                }
                if (parts != null && parts.Length != 0)
                {
                    //TODO:
                    //Finish it
                    //Debug.Log("WTF");
                    List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                    formData.Add(new MultipartFormDataSection("JSON Body", json, "application/json"));
                    formData.AddRange(parts);
                    var boundary = UnityWebRequest.GenerateBoundary();
                    byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
                    uwr = UnityWebRequest.Put($"{requestUrl}", formSections);
                    uwr.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + System.Text.Encoding.UTF8.GetString(boundary));

                    uwr.uploadHandler.contentType = "multipart/form-data; boundary=" + System.Text.Encoding.UTF8.GetString(boundary);

                }
                else
                {
                    uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                    uwr.uploadHandler.contentType = "application/json";
                }
            }
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", $"{TokenPrefix} {token}");


            ToolsDebug.Log($"{UnityWebRequest.kHttpVerbPOST}: {requestUrl} {uwr.GetRequestHeader("Authorization")} JSONBody:{json?.Substring(0, Mathf.Min(json.Length, Instance.logLimit))}");


            worker.Request = param;
            worker.Start();
            return useMock;
        }

        static void PostResponseWorker<O, I, W>(UnityWebRequest unityWebRequest, IWorker<O, I> worker)
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
                            if (unityWebRequest.method == "POST")
                                key = $"POST:{unityWebRequest.url.Replace($"{url}/", "")}";
                            else if (unityWebRequest.method == "PUT") key = $"PUT:{unityWebRequest.url.Replace($"{url}/", "")}";
                            else key = $"POST:{unityWebRequest.url.Replace($"{url}/", "")}";
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
                    try
                    {
                        response = worker.Deserialize<O>(downloadedText);
                    }
                    catch (Exception e)
                    {
                        worker.ErrorProcessing(400,"Can't deserialize answer.");
                        return;
                    }
                    if (response != null || typeof(O) == typeof(string))
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
