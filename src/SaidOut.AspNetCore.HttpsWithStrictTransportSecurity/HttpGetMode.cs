namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Specify the desired action that should be done for GET request using http scheme.</summary>
    public enum HttpGetMode
    {

        /// <summary>HTTP Forbidden (403) should be returned on GET request using http scheme.</summary>
        ReturnForbidden,

        /// <summary>Redirection to https scheme should be done for GET request using http scheme.</summary>
        RedirectToHttps
    }
}