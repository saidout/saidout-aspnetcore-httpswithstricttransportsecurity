namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Specify the operational mode of the <see cref="HttpsWithStrictTransportSecurityMiddleware"/>.</summary>
    public enum HttpsMode
    {

        /// <summary>HTTP Forbidden (403) should be returned for all scheme that is not https.</summary>
        Strict,

        /// <summary>
        /// GET request using http scheme should be redirected if <see cref="HttpGetModeAttribute"/> with <see cref="HttpGetModeAttribute.Mode"/> set to <see cref="HttpGetMode.RedirectToHttps"/> is specified on the controller action associated with the GET request.
        /// For all other request that is not using https scheme HTTP Forbidden (403) should be returned.
        /// </summary>
        AllowedRedirectForGet
    }
}