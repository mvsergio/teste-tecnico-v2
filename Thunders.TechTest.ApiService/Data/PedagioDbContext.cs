using Microsoft.EntityFrameworkCore;
using Thunders.TechTest.ApiService.Models;

namespace Thunders.TechTest.ApiService.Data;

public class PedagioDbContext(DbContextOptions<PedagioDbContext> options) : DbContext(options)
{
    public DbSet<Utilizacao> Utilizacoes { get; set; }
}