using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AuthWorker : IWorker<TokenDto, AuthDto>
{
    public AuthDto Request { get; set; }
    public void Execute(TokenDto tokenDto)
    {
        Debug.Log("Token: " + tokenDto.Token);
    }

    public void ErrorProcessing(long code, string error)
    {
        Debug.Log(error);
    }

    public void Progress(float progress)
    {

    }
}

public class RegistrationWorker : IWorker<RegistrationDto, AuthDto>
{
    public AuthDto Request { get; set; }
    public void Execute(RegistrationDto registrationDto)
    {
        Debug.Log("Token: " + registrationDto.email);
    }

    public void ErrorProcessing(long code, string error)
    {
        Debug.Log(error);
    }

    public void Progress(float progress)
    {

    }
}

