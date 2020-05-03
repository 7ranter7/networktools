using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RanterTools.UI.Debug;
namespace RanterTools.Networking
{


    public class HTTPControllerDebug : DebugTab
    {
        #region Parameters
        [Header("HTTP")]
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

        public override void Init(Transform tab, Transform frame)
        {
            base.Init(tab, frame);
            InitMocks();
        }

        void InitMocks()
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
            InitMocks();
        }

        void AddMock()
        {
            if (HTTPController.MocksResource == MocksResource.MEMORY)
            {
                HTTPController.mocks[keyField.text] = valueField.text;
                InitMocks();
            }
        }
        #endregion Methods

        #region Unity
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (keyField == null)
                keyField = GameObject.Find("ResponseKey").GetComponent<TMP_InputField>();
            if (valueField == null)
                valueField = GameObject.Find("ResponseValue").GetComponent<TMP_InputField>();
            if (add == null)
                add = GameObject.Find("AddResponse").GetComponent<Button>();
            if (mocksResource == null)
                mocksResource = GameObject.Find("MockType").GetComponent<TMP_Dropdown>();
            if (filePath == null)
                filePath = GameObject.Find("MocksFilePath").GetComponent<TMP_InputField>();
            if (responses == null)
                responses = GameObject.Find("Responses").GetComponent<RectTransform>();
        }
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
        #endregion Unity
    }


}