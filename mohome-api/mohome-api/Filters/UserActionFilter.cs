using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using mohome_api.API_Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static mohome_api.Options;

namespace mohome_api.Filters
{
    public class UserActionFilter : ActionFilterAttribute, IAsyncActionFilter
    {
        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var currentUser = context.HttpContext.User;                 
            if (!currentUser.HasClaim(c => c.Type == claimTypes.Role.ToString()))
            {
                var resultBody = new
                {
                    error = new
                    {
                        errorId = ErrorList.UndefinedUser.Id,
                        erroeMessage = ErrorList.UndefinedUser.Description
                    }
                };

                context.Result = new ObjectResult(resultBody) { StatusCode = (int?) HttpStatusCode.Unauthorized };    
            }
            else
            {
                await next();
            }
        }
    }
}
