using Korp.Estoque.Domain.Exceptions;

namespace Korp.Estoque.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Erro de validação",
                    status = 400,
                    detail = string.Join(" | ", ex.Errors)
                });
            }

            catch (UnauthorizedException ex)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Não autorizado",
                    status = 401,
                    detail = ex.Message
                });
            }

            catch (NotFoundException ex)
            {
                context.Response.StatusCode = 404;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    title = "Não encontrado",
                    status = 404,
                    detail = ex.Message
                });
            }

            catch (ConflictException ex)
            {
                context.Response.StatusCode = 409;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    title = "Conflito",
                    status = 409,
                    detail = ex.Message
                });
            }

            catch (BusinessException ex)
            {
                context.Response.StatusCode = 422;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Regra de negócio",
                    status = 422,
                    detail = ex.Message
                });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "Erro interno",
                    status = 500,
                    detail = "Ocorreu um erro inesperado no servidor."
                });
            }
        }
    }
}
