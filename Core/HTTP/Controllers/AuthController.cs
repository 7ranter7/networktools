using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RanterTools.Networking.Examples
{


    /// <summary>
    /// Simple auth controller for signIn, signUp and signOut methods.
    /// </summary>
    public static class AuthController
    {
        #region Events

        #endregion Events

        #region Global State

        #endregion Global State

        #region Global Methods

        /// <summary>
        /// Sample of sign in method.
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        public static void SignIn(string login, string password)
        {
            var param = new AuthDto { email = login, password = password };
            HTTPController.JSONPost<TokenDto, AuthDto, AuthWorker>("auth/login", param);
        }
        /// <summary>
        /// Sample of sign up method.
        /// </summary>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        public static void SignUp(string login, string password)
        {
            var param = new AuthDto { email = login, password = password };
            HTTPController.JSONPost<RegistrationDto, AuthDto, RegistrationWorker>("auth/register", param);
        }
        /// <summary>
        /// Sample of sign out method.
        /// </summary>
        public static void SignOut()
        {
            HTTPController.Token = null;
        }
        #endregion Global Methods

    }
}