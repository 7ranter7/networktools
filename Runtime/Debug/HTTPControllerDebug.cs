using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace RanterTools.Networking
{


    public class HTTPControllerDebug : MonoBehaviour
    {
        #region Parameters
        [SerializeField]
        GameObject debugScreen;
        [SerializeField]
        MockElement mockElementPrefab;
        [SerializeField]
        RectTransform responses;
        [SerializeField]
        TMP_InputField keyField;
        [SerializeField]
        TMP_InputField valueField;
        [SerializeField]
        Button add;
        [SerializeField]
        TMP_Dropdown mocksResource;
        [SerializeField]
        TMP_InputField filePath;
        #endregion Parameters

        #region State
        List<MockElement> elements = new List<MockElement>();
        float timer = 0;
        #endregion State

        #region Methods
        public void DeleteElement(MockElement element)
        {
            elements.Remove(element);
            HTTPController.mocks.Remove(element.mockKey);
            DestroyImmediate(element.gameObject);
        }

        void Init()
        {
            Clear();
            mocksResource.SetValueWithoutNotify((int)HTTPController.MocksResource);
            filePath.text = HTTPController.Instance.mocksFilePath;
            if (HTTPController.MocksResource != MocksResource.NONE)
            {
                if (HTTPController.mocks != null)
                    foreach (var i in HTTPController.mocks)
                    {
                        var element = Instantiate(mockElementPrefab, responses);
                        element.parent = this;
                        elements.Add(element);
                        element.SetData(i.Key, i.Value);
                    }
            }
        }

        void Clear()
        {
            foreach (var e in elements)
            {
                DestroyImmediate(e.gameObject);
            }
            elements.Clear();
        }

        void ChangeValue(int mockValue)
        {
            HTTPController.Instance.mocksFilePath = filePath.text;
            HTTPController.MocksResource = (MocksResource)mockValue;
            Init();
        }

        void AddMock()
        {
            if (HTTPController.MocksResource == MocksResource.MEMORY)
            {
                HTTPController.mocks[keyField.text] = valueField.text;
                Init();
            }
        }
        #endregion Methods

        #region Unity
        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        void OnEnable()
        {
            mocksResource.onValueChanged.AddListener(ChangeValue);
            add.onClick.AddListener(AddMock);

        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        void OnDisable()
        {

        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
#if RANTER_TOOLS_DEBUG_NETWORKING
            timer += Time.deltaTime;
            if (timer >= 0.5f && (Input.touchCount == 3 || Input.GetKey(KeyCode.Insert)))
            {
                debugScreen.SetActive(!debugScreen.activeSelf);
                if (debugScreen.gameObject.activeSelf) Init();
                timer = 0;
            }
#endif 
        }
        #endregion Unity
    }


}