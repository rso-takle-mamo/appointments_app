using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Runtime.CompilerServices;

namespace UserService.Api.Filters;

public class ModelValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            throw new ArgumentException(string.Join(" ", errors));
        }

        base.OnActionExecuting(context);
    }
}