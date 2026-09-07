using SubastaYa.Domain.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);   // ← acá "adentro" pasa TODO: routing, controller, handler...
        }
        catch (DomainException ex)              // agarra la excepción de negocio
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)                     // agarra cualquier otra cosa no prevista
        {
            _logger.LogError(ex, "Error no controlado");
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new { error = "Error interno" });
        }
    }
}