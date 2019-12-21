using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class AuthDto
{
    public string email;
    public string password;
}

[Serializable]
public class RegistrationDto
{
    public string email;
}