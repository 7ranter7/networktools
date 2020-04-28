using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace RanterTools.Networking
{


    public class MockElement : MonoBehaviour
    {
        #region Parameters
        [SerializeField]
        TextMeshProUGUI text;
        [SerializeField]
        Button delete;
        #endregion Parameters
        #region State
        [System.NonSerialized]
        public string mockKey;
        [System.NonSerialized]
        public HTTPControllerDebug parent;
        #endregion State
        #region Methods
        public void SetData(string key, string value)
        {
            text.text = value;
            mockKey = key;
        }
        public void Delete()
        {
            if (parent != null)
                parent.DeleteElement(this);
            else DestroyImmediate(gameObject);
        }
        #endregion Methods

        #region Unity
        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        void OnEnable()
        {
            delete.onClick.AddListener(Delete);
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        void OnDisable()
        {
            delete.onClick.RemoveListener(Delete);
        }
        #endregion Unity
    }
}
