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
        public static void JSONPostAuth<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null,
                                                Func<I, string> serializer = null, Func<string, O> deserializer = null)
           where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            Instance.StartCoroutine(JSONPostRequest<O, I, W>(endpoint, param, worker, Token.Token, serializer, deserializer));
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
        public static void JSONPost<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null,
                                            Func<I, string> serializer = null, Func<string, O> deserializer = null)
            where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            Instance.StartCoroutine(JSONPostRequest<O, I, W>(endpoint, param, worker, null, serializer, deserializer));
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
        public static void JSONPostAuthAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null,
                                                    Func<I, string> serializer = null, Func<string, O> deserializer = null)
            where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            JSONPostRequestAsync<O, I, W>(endpoint, param, worker, Token.Token, serializer, deserializer);
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
        public static void JSONPostAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null,
                                    Func<I, string> serializer = null, Func<string, O> deserializer = null)
             where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            JSONPostRequestAsync<O, I, W>(endpoint, param, worker, null, serializer, deserializer);
        }

        static IEnumerator JSONPostRequest<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, string token = null,
                                                     Func<I, string> serializer = null, Func<string, O> deserializer = null)
        where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, I> workerTmp;
            if (!PostRequestInit<O, I, W>(endpoint, out uwr, out workerTmp, param, worker, token, serializer))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress + uwr.downloadProgress) / 2);
                    yield return null;
                }
            }
            workerTmp.Progress((uwr.downloadProgress));
            PostResponseWorker<O, I, W>(uwr, workerTmp, deserializer);
        }

        static async void JSONPostRequestAsync<O, I, W>(string endpoint, I param, IWorker<O, I> worker = null, string token = null,
                                                         Func<I, string> serializer = null, Func<string, O> deserializer = null)
         where W : IWorker<O, I>, new()
        where I : class
        where O : class
        {
            UnityWebRequest uwr;
            IWorker<O, I> workerTmp;
            if (!PostRequestInit<O, I, W>(endpoint, out uwr, out workerTmp, param, worker, token, serializer))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    workerTmp.Progress((uwr.uploadProgress));
                    await Task.Yield();
                }
            }
            workerTmp.Progress((uwr.downloadProgress));
            PostResponseWorker<O, I, W>(uwr, workerTmp, deserializer);
        }

        #endregion Global Methods
    }

}
