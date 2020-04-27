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
            GetRequestInit<O, W>(endpoint, out uwr, out workerTmp, query, worker, token);
            if (MocksResource == MocksResource.NONE)
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
            GetRequestInit<O, W>(endpoint, out uwr, out workerTmp, query, worker, token);
            if (MocksResource == MocksResource.NONE)
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
        #endregion Global Methods
    }
}