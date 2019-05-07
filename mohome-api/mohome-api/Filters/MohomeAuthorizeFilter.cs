using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using mohome_api.API_Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace mohome_api.Filters
{
    public class MohomeAuthorizeFilter : IAsyncAuthorizationFilter
    {
        public AuthorizationPolicy Policy { get; }

        public MohomeAuthorizeFilter(AuthorizationPolicy policy)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Allow Anonymous skips all authorization
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                return;
            }

            var policyEvaluator = context.HttpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
            var authenticateResult = await policyEvaluator.AuthenticateAsync(Policy, context.HttpContext);
            var authorizeResult = await policyEvaluator.AuthorizeAsync(Policy, authenticateResult, context.HttpContext, context);

            if (authorizeResult.Challenged)
            {
                // Return custom 401 result
                context.Result = new UnauthorizedErrorResult(new { error = new { errorCode = ErrorList.Unauthorized.Id,
                                                                    errorMessage = ErrorList.Unauthorized.Description} });
            }
            else if (authorizeResult.Forbidden)
            {
                // Return default 403 result
                context.Result = new ForbidResult(Policy.AuthenticationSchemes.ToArray());
            }
        }

      

        public class UnauthorizedErrorResult : JsonResult
        {
            private readonly HttpStatusCode _httpStatus;

            public UnauthorizedErrorResult(object data) : base(data)
            {
                _httpStatus = HttpStatusCode.Unauthorized;
            }


            public async override Task ExecuteResultAsync(ActionContext context)
            {
                context.HttpContext.Response.StatusCode = (int)_httpStatus;
                await base.ExecuteResultAsync(context);
            }
     
        }
    }
}
