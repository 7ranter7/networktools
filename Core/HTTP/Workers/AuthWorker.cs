using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace RanterTools.Networking.Examples
{
    public class AuthWorker : IWorker<TokenDto, AuthDto>
    {
        public AuthDto Request { get; set; }

        public virtual void Start()
        {

        }
        public virtual void Execute(TokenDto tokenDto)
        {
            Debug.Log("Token: " + tokenDto.Token);
        }

        public virtual void ErrorProcessing(long code, string error)
        {
            Debug.Log(error);
        }

        public virtual void Progress(float progress)
        {

        }

        public virtual string Serialize(AuthDto obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public virtual TokenDto Deserialize(string obj)
        {
            return JsonUtility.FromJson<TokenDto>(obj);
        }
    }

    public class RegistrationWorker : IWorker<RegistrationDto, AuthDto>
    {
        public AuthDto Request { get; set; }

        public virtual void Start()
        {

        }
        public virtual void Execute(RegistrationDto registrationDto)
        {
            Debug.Log("Token: " + registrationDto.email);
        }

        public virtual void ErrorProcessing(long code, string error)
        {
            Debug.Log(error);
        }

        public virtual void Progress(float progress)
        {

        }

        public virtual string Serialize(AuthDto obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public virtual RegistrationDto Deserialize(string obj)
        {
            return JsonUtility.FromJson<RegistrationDto>(obj);
        }
    }

}