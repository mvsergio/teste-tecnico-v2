using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.DTO;
using Thunders.TechTest.ApiService.Models;

namespace Thunders.TechTest.ApiService.Services;

public class PedagioService(PedagioDbContext dbContext) : IPedagioService
{
    private readonly PedagioDbContext _dbContext = dbContext;
    private static readonly ActivitySource ActivitySource = new("PedagioService");

    public async Task InserirUtilizacaoAsync(Utilizacao utilizacao)
    {
        if (utilizacao == null)
            throw new ArgumentNullException(nameof(utilizacao));

        if (string.IsNullOrEmpty(utilizacao.Praca))
            throw new ArgumentException("Praça é obrigatória.", nameof(utilizacao.Praca));

        if (string.IsNullOrEmpty(utilizacao.Cidade))
            throw new ArgumentException("Cidade é obrigatória.", nameof(utilizacao.Cidade));

        if (string.IsNullOrEmpty(utilizacao.TipoVeiculo))
            throw new ArgumentException("Tipo de Veículo é obrigatório.", nameof(utilizacao.TipoVeiculo));

        if (utilizacao.ValorPago < 0)
            throw new ArgumentException("Valor pago não pode zer menor que zero.", nameof(utilizacao.ValorPago));

        using var activity = ActivitySource.StartActivity("InserirUtilizacao", ActivityKind.Internal);
        activity?.SetTag("utilizacao.data", utilizacao.DataHora.ToString("o"));
        activity?.SetTag("utilizacao.praca", utilizacao.Praca);
        activity?.SetTag("utilizacao.cidade", utilizacao.Cidade);

        try
        {
            await _dbContext.Utilizacoes.AddAsync(utilizacao);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<List<RelatorioValorPorHoraDTO>> GerarRelatorioValorTotalPorHoraAsync(string cidade)
    {
        using var activity = ActivitySource.StartActivity("GerarRelatorioValorTotalPorHora", ActivityKind.Internal);
        activity?.SetTag("cidade", cidade);

        try
        {
            var resultado = await _dbContext.Utilizacoes
                .Where(u => u.Cidade == cidade)
                .GroupBy(u => u.DataHora.Hour)
                .Select(g => new RelatorioValorPorHoraDTO
                {
                    Hora = g.Key,
                    ValorTotal = g.Sum(u => u.ValorPago)
                })
                .ToListAsync();

            return resultado;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<List<RelatorioPracasMaisFaturaramDTO>> GerarRelatorioPracasMaisFaturaramAsync(int quantidade, DateTime mes)
    {
        using var activity = ActivitySource.StartActivity("GerarRelatorioPracasMaisFaturaram", ActivityKind.Internal);
        activity?.SetTag("quantidade", quantidade);
        activity?.SetTag("mes", mes.ToString("yyyy-MM"));

        try
        {
            var inicioMes = new DateTime(mes.Year, mes.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddTicks(-1);

            var resultado = await _dbContext.Utilizacoes
                .Where(u => u.DataHora >= inicioMes && u.DataHora <= fimMes)
                .GroupBy(u => u.Praca)
                .Select(g => new RelatorioPracasMaisFaturaramDTO
                {
                    Praca = g.Key,
                    FaturamentoTotal = g.Sum(u => u.ValorPago)
                })
                .OrderByDescending(r => r.FaturamentoTotal)
                .Take(quantidade)
                .ToListAsync();

            return resultado;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<List<RelatorioTiposVeiculosDTO>> GerarRelatorioTiposVeiculosPorPracaAsync(string praca)
    {
        using var activity = ActivitySource.StartActivity("GerarRelatorioTiposVeiculosPorPraca", ActivityKind.Internal);
        activity?.SetTag("praca", praca);

        try
        {
            var resultado = await _dbContext.Utilizacoes
                .Where(u => u.Praca == praca)
                .GroupBy(u => u.TipoVeiculo)
                .Select(g => new RelatorioTiposVeiculosDTO
                {
                    TipoVeiculo = g.Key,
                    Quantidade = g.Count()
                })
                .ToListAsync();

            return resultado;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
