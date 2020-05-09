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


        #endregion Global Methods
    }
}
