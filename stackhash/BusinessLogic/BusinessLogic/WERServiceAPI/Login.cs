//---------------------------------------------------------------------
// <summary>
//      Class for logging into the WER service using the API. This class
//      also holds the login credentials.
// </summary>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using StackHash.WindowsErrorReporting.Services.Data.API;

namespace StackHash.WindowsErrorReporting.Services.Data.API
{
    /// <summary>
    /// Class for logging into the WER service using the API.This class
    /// also holds the login credentials.
    /// </summary>
    public class Login : Base
    {
        #region Fields
        /// <summary>
        /// Private field to hold the user name.
        /// </summary>
        private string username;

        /// <summary>
        /// Private field to hold the password.
        /// </summary>
        private string password;

        /// <summary>
        /// private field to hold the encrypted ticket.
        /// </summary>
        private string encryptedTicket;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Method to log into Winqual.
        /// </summary>
        /// <param name="username">User name.</param>
        /// <param name="password">Password.</param>
        public Login(string username, string password)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }

            if (username.Trim() == string.Empty)
            {
                throw new ArgumentOutOfRangeException("username", "username parameter cannot be empty.");
            }

            this.username = username;

            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            if (password.Trim() == string.Empty)
            {
                throw new ArgumentOutOfRangeException("password", "password parameter cannot be empty.");
            }

            this.password = password;
        }
        #endregion Constructors

        #region Properties
        #region Internal Properties
        /// <summary>
        /// Gets or sets the encrypted ticket for the login. Internal so that only the 
        /// classes in this assembly can access the ticket.
        /// </summary>
        internal string EncryptedTicket
        {
            get { return this.encryptedTicket; }
            set { this.encryptedTicket = value; }
        }
        #endregion Internal Properties
        #endregion Properties

        #region Methods
        #region Public Methods
        /// <summary>
        /// Method to validate the credentials with the WER service.
        /// </summary>
        public bool Validate()
        {
            //
            // Use the Winqual authentication web service to log into WER services.
            //
            string ticket = null;
            try
            {
                AuthenticationService authenticationService = new AuthenticationService();
                ticket = authenticationService.GetBasicTicket(this.username, this.password);
            }
            catch (Exception)
            {
                //
                // throw authentication exception.
                //
                throw new FeedAuthenticationException();
            }

            if (string.IsNullOrEmpty(ticket) == true)
            {
                //
                // throw authentication exception.
                // The ticket will be null in the following cases:
                // 1. username and/or password is incorrect.
                // 2. password has expired.
                // 3. password locked out.
                // 4. company disabled.
                //
                throw new FeedAuthenticationException();
            }

            //
            // assign the ticket to the EncryptedTicket internal property
            // where it will be used with every subsequent call.
            //
            this.EncryptedTicket = ticket;
            return true;
        }
        #endregion Public Methods
        #endregion Methods
    }
}
