using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace mohome_api.Filters
{
    public class ModelActionFilter : ActionFilterAttribute, IAsyncActionFilter
    {
        async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errorList = (from item in context.ModelState
                                 where item.Value.Errors.Any()
                                 select item.Value.Errors[0].ErrorMessage).ToList();
                context.Result = new BadRequestObjectResult(new { error = errorList });
            }
            else
            {
               await next();
            }       
        }
    }
}
