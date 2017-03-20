using System;
using SaidOut.Common.Extensions;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Attribute is used to specify the desired action that should be done for GET request using http scheme.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class HttpGetModeAttribute : Attribute
    {

        /// <summary>Constructor set the <see cref="Mode"/> property.</summary>
        /// <param name="mode">The mode determines the desired action for GET request made using http scheme.</param>
        public HttpGetModeAttribute(HttpGetMode mode)
        {
            mode.ThrowIfEnumValueIsNotDefined(nameof(mode));

            Mode = mode;
        }


        /// <summary>The mode determines the desired action for GET request made using http scheme.</summary>
        public HttpGetMode Mode { get; private set; }
    }
}