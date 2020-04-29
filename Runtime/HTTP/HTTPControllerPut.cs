using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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

        #endregion Global Methods
    }
}
