using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Services;

public interface ICatalogoCursosCacheService
{
    Task<IReadOnlyCollection<Curso>> ObtenerCursosActivosAsync(CancellationToken cancellationToken = default);
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}

public class CatalogoCursosCacheService : ICatalogoCursosCacheService
{
    private const string CacheKey = "catalogo:cursos:activos";
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
    };

    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CatalogoCursosCacheService> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public CatalogoCursosCacheService(
        ApplicationDbContext context,
        IDistributedCache cache,
        ILogger<CatalogoCursosCacheService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Curso>> ObtenerCursosActivosAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            try
            {
                var cacheItems = JsonSerializer.Deserialize<List<CursoCacheItem>>(cached, _serializerOptions);
                if (cacheItems is not null)
                {
                    return cacheItems.Select(ToEntity).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No fue posible deserializar el cache del catalogo de cursos; se reconstruira.");
            }
        }

        var cursos = await _context.Cursos
            .AsNoTracking()
            .Where(c => c.Activo)
            .ToListAsync(cancellationToken);

        var serialized = JsonSerializer.Serialize(cursos.Select(FromEntity), _serializerOptions);
        await _cache.SetStringAsync(CacheKey, serialized, CacheOptions, cancellationToken);

        return cursos;
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(CacheKey, cancellationToken);

    private static CursoCacheItem FromEntity(Curso curso) => new()
    {
        Id = curso.Id,
        Codigo = curso.Codigo,
        Nombre = curso.Nombre,
        Creditos = curso.Creditos,
        CupoMaximo = curso.CupoMaximo,
        HorarioInicio = curso.HorarioInicio.ToString("HH:mm"),
        HorarioFin = curso.HorarioFin.ToString("HH:mm"),
        Activo = curso.Activo
    };

    private static Curso ToEntity(CursoCacheItem item) => new()
    {
        Id = item.Id,
        Codigo = item.Codigo,
        Nombre = item.Nombre,
        Creditos = item.Creditos,
        CupoMaximo = item.CupoMaximo,
        HorarioInicio = TimeOnly.Parse(item.HorarioInicio),
        HorarioFin = TimeOnly.Parse(item.HorarioFin),
        Activo = item.Activo
    };

    private sealed class CursoCacheItem
    {
        public int Id { get; init; }
        public string Codigo { get; init; } = string.Empty;
        public string Nombre { get; init; } = string.Empty;
        public int Creditos { get; init; }
        public int CupoMaximo { get; init; }
        public string HorarioInicio { get; init; } = string.Empty;
        public string HorarioFin { get; init; } = string.Empty;
        public bool Activo { get; init; }
    }
}
