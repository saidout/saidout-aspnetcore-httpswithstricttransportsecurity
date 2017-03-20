using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaidOut.Common.Extensions;
using System.Reflection;

namespace SaidOut.AspNetCore.HttpsWithStrictTransportSecurity
{

    /// <summary>Use <see cref="IRouter"/> and <see cref="HttpContext"/> in order to get attribute associated with the controller action that should be used when processing a HTTP request.</summary>
    public class RouterControllerAttributeSupport : IControllerAttributeSupport
    {
        private readonly IRouter _router;


        /// <summary></summary>
        /// <param name="router">Route needed in order to determine which controller action should be invoked for a HTTP request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="router"/> is <c>null</c>.</exception>
        public RouterControllerAttributeSupport(IRouter router)
        {
            router.ThrowIfNull(nameof(router));
            _router = router;
        }


        /// <summary>Get the first attribute found on a controller action by first looking for attributes on the action method and then on the controller class.</summary>
        /// <typeparam name="TAttribute">Attribute to look for.</typeparam>
        /// <param name="context">HTTP context used to determine which controller action that should be invoked for the HTTP request.</param>
        /// <returns>The first attribute found of type <typeparamref name="TAttribute"/> or <c>null</c>.</returns>
        public async Task<TAttribute> GetActionAttributeAsync<TAttribute>(HttpContext context)
            where TAttribute : Attribute
        {
            var routeContext = new RouteContext(context);
            await _router.RouteAsync(routeContext);
            var target = routeContext.Handler?.Target;
            var actionDescritpion = target?.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(it => it.FieldType == typeof(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor))
                ?.GetValue(target) as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;

            if (actionDescritpion == null)
                return null;

            var actionMethodAttribute = actionDescritpion.MethodInfo.GetCustomAttribute<TAttribute>();
            if (actionMethodAttribute != null)
                return actionMethodAttribute;

            return actionDescritpion.ControllerTypeInfo.GetCustomAttribute<TAttribute>();
        }
    }
}