using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Contain method for getting attributes set on a MVC action on a <see cref="Microsoft.AspNetCore.Mvc.Controller"/>.</summary>
    public interface IControllerAttributeSupport
    {

        /// <summary>Get the first attribute found on a controller action by first looking for attributes on the action method and then on the controller class.</summary>
        /// <typeparam name="TAttribute">Attribute to look for.</typeparam>
        /// <param name="context">HTTP context used to determine which controller action that should be invoked for the HTTP request.</param>
        /// <returns>The first attribute found of type <typeparamref name="TAttribute"/> or <c>null</c>.</returns>
        Task<TAttribute> GetActionAttributeAsync<TAttribute>(HttpContext context) where TAttribute : Attribute;
    }
}