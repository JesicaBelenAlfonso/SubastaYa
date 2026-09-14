using Microsoft.EntityFrameworkCore;
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
            await _next(ctx);
        }
        catch (NotFoundException ex)                    // recurso inexistente → 404
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (DomainConflictException ex)              // conflicto de estado → 409
        {
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)          // conflicto de concurrencia (optimistic lock) → 409
        {
            _logger.LogWarning(ex, "Conflicto de concurrencia al guardar cambios");
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            await ctx.Response.WriteAsJsonAsync(new { error = "El recurso fue modificado por otro proceso. Reintentá la operación." });
        }
        catch (InvalidCredentialsException ex)          // credenciales inválidas → 401
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (DomainException ex)                      // regla de negocio → 400
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)                            // resto → 500
        {
            _logger.LogError(ex, "Error no controlado");
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new { error = "Error interno" });
        }
    }
}