using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Security
{
    public interface ISecureGateway
    {
        /// <summary>
        /// Returns an SecureConnect token.
        /// </summary>
        /// <returns>Returns null if couldn't get a token</returns>
        public string GetToken();
        /// <summary>
        /// Validate a secure connect token
        /// </summary>
        /// <param name="token">Secure connect token</param>
        /// <returns>True or false</returns>
        public bool ValidateToke(string token);
    }
}
