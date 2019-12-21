using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using RanterTools.Base;
//using Newtonsoft.Json;

namespace RanterTools.Networking
{

    /// <summary>
    /// HTTP controller for async requests.
    /// </summary>
    public class HTTPController : SingletonBehaviour<HTTPController>
    {
        #region Global State
        static string token;
        /// <summary>
        /// Current access token.
        /// </summary>
        /// <value>Return current access token if it exists.</value>
        public static string Token
        {
            get
            {
                if (token == null)
                {
                    if (PlayerPrefs.HasKey("Token")) token = PlayerPrefs.GetString("Token", null);
                }
                return token;
            }
            set
            {
                token = value;
                PlayerPrefs.SetString("Token", token);
                lastTokenDateTime = DateTime.Now;
            }
        }
        static DateTime lastTokenDateTime;
        /// <summary>
        /// Last token received time.
        /// </summary>
        public static DateTime LastTokenDateTime;
        static string url = "http://localhost/";
        #endregion Global State


        #region Global Methods
        /// <summary>
        /// Send json post request with auth token.
        /// </summary>
        /// <param name="controller">Controller of end point.</param>
        /// <param name="method">Methods of end point.</param>
        /// <param name="param">Serializable parameter.</param>
        /// <typeparam name="T">IWorker<K,O>.</typeparam>
        /// <typeparam name="K">Result serializable parameter of request.</typeparam>
        /// <typeparam name="O">Request serializable parameter.</typeparam>
        public static void JSONPostAuth<T, K, O>(string controller, string method, O param)
            where T : IWorker<K, O>, new()
            where O : class
        {
            Instance.StartCoroutine(JSONPostRequest<T, K, O>(controller, method, param, null, token));
        }
        /// <summary>
        /// Send json post auth request without parameters.
        /// </summary>
        /// <param name="controller">Controller of end point.</param>
        /// <param name="method">Methods of end point.</param>
        /// <typeparam name="T">IWorker<K,O>.</typeparam>
        /// <typeparam name="K">Result serializable parameter of request.</typeparam>
        public static void JSONPostAuth<T, K>(string controller, string method)
            where T : IWorker<K, object>, new()
        {
            Instance.StartCoroutine(JSONPostRequest<T, K, object>(controller, method, null, null, token));
        }
        /// <summary>
        ///  Send json post request without auth token.
        /// </summary>
        /// <param name="controller">Controller of end point.</param>
        /// <param name="method">Methods of end point.</param>
        /// <param name="param">Serializable parameter.</param>
        /// <param name="worker">Concret worker for some data.</param>
        /// <typeparam name="T">IWorker<K,O>.</typeparam>
        /// <typeparam name="K">Result serializable parameter of request.</typeparam>
        /// <typeparam name="O">Request serializable parameter.</typeparam>
        public static void JSONPost<T, K, O>(string controller, string method, O param, IWorker<K, O> worker = null)
            where T : IWorker<K, O>, new()
            where O : class
        {
            Instance.StartCoroutine(JSONPostRequest<T, K, O>(controller, method, param, worker, null));
        }

        static IEnumerator JSONPostRequest<T, K, O>(string controller, string method, O param, IWorker<K, O> worker = null, string token = null)
            where T : IWorker<K, O>, new()
            where O : class
        {
            url = Instance.urlParam;
            UnityWebRequest uwr = new UnityWebRequest(url + "/" + controller + "/" + method, "POST");

            string json = null;
            byte[] jsonToSend = new byte[1];

            if (param != null)
            {
                json = JsonUtility.ToJson(param);//JsonConvert.SerializeObject(param);
                jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            }

            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", "Bearer " + token);

            ToolsDebug.Log("Sending : " + json);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");


            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            IWorker<K, O> workerTmp;
            if (worker == null)
            {
                workerTmp = new T();
            }
            else
            {
                workerTmp = worker;
            }

            if (param != null)
                workerTmp.Request = JsonUtility.FromJson<O>(new System.Text.UTF8Encoding().GetString(uwr.uploadHandler.data));//JsonConvert.DeserializeObject<O>(new System.Text.UTF8Encoding().GetString(uwr.uploadHandler.data));
            if (uwr.isNetworkError || !string.IsNullOrEmpty(uwr.error))
            {
                workerTmp.ErrorProcessing(uwr.error);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<K>(uwr.downloadHandler.text);// JsonConvert.DeserializeObject<K>(uwr.downloadHandler.text);
                    ToolsDebug.Log(uwr.downloadHandler.text);
                    if (response != null)
                    {
                        workerTmp.Execute(response);

                    }
                    else
                    {
                        workerTmp.ErrorProcessing("Unknown Error");
                    }
                }
                catch (ArgumentException)
                {
                    ToolsDebug.Log(uwr.downloadHandler.text);
                }
            }

        }




        /// <summary>
        /// Get JSON data from get request with auth token.
        /// </summary>
        /// <param name="controller">Controller of end point.</param>
        /// <param name="method">Methods of end point.</param>
        /// <param name="worker">Concret worker for some data.</param>
        /// <typeparam name="T">IWorker<K,O>.</typeparam>
        /// <typeparam name="K">Result serializable parameter of request.</typeparam>
        public static void JSONGetAuth<T, K>(string controller, string method, IWorker<K, string> worker = null)
        where T : IWorker<K, string>, new()
        {
            Instance.StartCoroutine(JSONGetRequest<T, K>(controller, method, worker, Token));
        }

        /// <summary>
        /// Get JSON data from get request without auth token.
        /// </summary>
        /// <param name="controller">Controller of end point.</param>
        /// <param name="method">Methods of end point.</param>
        /// <param name="worker">Concret worker for some data.</param>
        /// <typeparam name="T">IWorker<K,O>.</typeparam>
        /// <typeparam name="K">Result serializable parameter of request.</typeparam>
        public static void JSONGet<T, K>(string controller, string method, IWorker<K, string> worker = null)
            where T : IWorker<K, string>, new()
        {
            Instance.StartCoroutine(JSONGetRequest<T, K>(controller, method, worker));
        }

        static IEnumerator JSONGetRequest<T, K>(string controller, string method, IWorker<K, string> worker, string token = null)
        where T : IWorker<K, string>, new()
        {
            url = Instance.urlParam;
            UnityWebRequest uwr = new UnityWebRequest(url + "/" + controller + "/" + method + "/", UnityWebRequest.kHttpVerbGET);

            string json = null;
            byte[] jsonToSend = new byte[1];
            if (!string.IsNullOrEmpty(token))
                uwr.SetRequestHeader("Authorization", "JWT " + token);

            ToolsDebug.Log(url + "/" + controller + "/" + method + " Sending : " + json + " " + uwr.GetRequestHeader("Authorization"));
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");


            yield return uwr.SendWebRequest();

            IWorker<K, string> workerTmp;
            if (worker == null)
            {
                workerTmp = new T();
            }
            else
            {
                workerTmp = worker;
            }
            if (uwr.isNetworkError || !string.IsNullOrEmpty(uwr.error))
            {
                if (uwr.responseCode != 400)
                    workerTmp.ErrorProcessing(uwr.error);
                else
                {
                    string error = "";
                    if (uwr?.downloadHandler?.text != null) error += uwr.downloadHandler.text;
                    workerTmp.ErrorProcessing(error);
                }
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<K>(uwr.downloadHandler.text);
                    Debug.Log(uwr.downloadHandler.text);
                    if (response != null)
                    {
                        workerTmp.Execute(response);
                    }
                    else
                    {
                        workerTmp.ErrorProcessing("Unknown Error");
                    }
                }
                catch (ArgumentException)
                {
                    ToolsDebug.Log(uwr.downloadHandler.text);
                }
            }

        }


        #endregion Global Methods




        #region Parameters
        [Tooltip("Base URL")]
        [SerializeField]
        string urlParam = "http://localhost/";
        #endregion Parameters
    }

}