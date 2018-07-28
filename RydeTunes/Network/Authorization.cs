using System;
using System.Collections.Generic;
using System.Text;

namespace RydeTunes.Network
{
    class Authorization
    {
        private string AuthorizationToken = "";
        public Authorization(string authorization)
        {
            if(string.IsNullOrEmpty(authorization))
            {
                throw new Exception("This Value can't be empty");
            }
            AuthorizationToken = authorization;
        }

        private string GetAuthorizationToken()
        {
            if (string.IsNullOrEmpty(AuthorizationToken))
            {
                throw new Exception("This AuthorizationToken can't be empty");
            }

            return AuthorizationToken;
        }
    }
}
