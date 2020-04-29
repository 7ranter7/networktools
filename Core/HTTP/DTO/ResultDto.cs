using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RanterTools.Networking.Examples
{
    [Serializable]
    public class ResultDto<T>
    {
        public bool IsSuccess;
        public string Error;
        public T Result;
    }
}