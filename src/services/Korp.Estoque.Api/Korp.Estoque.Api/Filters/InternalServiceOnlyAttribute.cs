using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Korp.Estoque.Api.Filters;

public class InternalServiceOnlyAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "X-Internal-Secret";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedSecret = configuration["InternalSettings:CommunicationSecret"];

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var extractedSecret) ||
            extractedSecret != expectedSecret)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Acesso restrito a serviços internos." });
            return;
        }

        await next();
    }
}