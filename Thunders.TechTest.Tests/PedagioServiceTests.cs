using Microsoft.EntityFrameworkCore;
using Moq;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Models;
using Thunders.TechTest.ApiService.Services;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thunders.TechTest.ApiService.DTO;
using Thunders.TechTest.ApiService.Handlers;

namespace Thunders.TechTest.Tests.Services;

public class PedagioServiceTests
{
    private PedagioService CriarServiceComDbEmMemoria(out PedagioDbContext dbContext)
    {
        var options = new DbContextOptionsBuilder<PedagioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        dbContext = new PedagioDbContext(options);
        return new PedagioService(dbContext);
    }

    [Fact]
    public async Task InserirUtilizacaoAsync_DeveInserirComSucesso()
    {
        var service = CriarServiceComDbEmMemoria(out var context);
        var utilizacao = new Utilizacao
        {
            DataHora = DateTime.UtcNow,
            Praca = "Ponte Rio-Niterói",
            Cidade = "Rio de Janeiro",
            Estado = "RJ",
            ValorPago = 10.00m,
            TipoVeiculo = "Carro"
        };

        await service.InserirUtilizacaoAsync(utilizacao);

        var inserido = await context.Utilizacoes.FirstOrDefaultAsync();
        Assert.NotNull(inserido);
        Assert.Equal("Ponte Rio-Niterói", inserido!.Praca);
    }

    [Fact]
    public async Task InserirUtilizacaoAsync_DeveLancarExcecaoSePracaForNula()
    {
        var service = CriarServiceComDbEmMemoria(out _);
        var utilizacao = new Utilizacao
        {
            DataHora = DateTime.UtcNow,
            Praca = null!,
            Cidade = "SP",
            Estado = "SP",
            ValorPago = 10,
            TipoVeiculo = "Carro"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.InserirUtilizacaoAsync(utilizacao));
    }

    [Fact]
    public async Task GerarRelatorioValorTotalPorHoraAsync_DeveAgruparPorHora()
    {
        var service = CriarServiceComDbEmMemoria(out var context);

        context.Utilizacoes.AddRange(
            new Utilizacao
            {
                DataHora = new DateTime(2024, 1, 1, 10, 0, 0),
                Cidade = "São Paulo",
                Praca = "A",
                Estado = "SP",
                TipoVeiculo = "Moto",
                ValorPago = 5.0m
            },
            new Utilizacao
            {
                DataHora = new DateTime(2024, 1, 1, 10, 30, 0),
                Cidade = "São Paulo",
                Praca = "B",
                Estado = "SP",
                TipoVeiculo = "Carro",
                ValorPago = 7.0m
            }
        );

        await context.SaveChangesAsync();

        var relatorio = await service.GerarRelatorioValorTotalPorHoraAsync("São Paulo");

        Assert.Single(relatorio);
        Assert.Equal(10, relatorio[0].Hora);
        Assert.Equal(12.0m, relatorio[0].ValorTotal);
    }

    [Fact]
    public async Task GerarRelatorioValorTotalPorHoraAsync_DeveRetornarListaVaziaQuandoNaoHouverDados()
    {
        var service = CriarServiceComDbEmMemoria(out _);
        var resultado = await service.GerarRelatorioValorTotalPorHoraAsync("CidadeInexistente");
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GerarRelatorioPracasMaisFaturaramAsync_DeveRetornarPracasOrdenadas()
    {
        var service = CriarServiceComDbEmMemoria(out var context);

        context.Utilizacoes.AddRange(
            new Utilizacao { DataHora = new DateTime(2024, 5, 1), Praca = "A", Cidade = "X", Estado = "X", TipoVeiculo = "Carro", ValorPago = 20 },
            new Utilizacao { DataHora = new DateTime(2024, 5, 2), Praca = "B", Cidade = "X", Estado = "X", TipoVeiculo = "Carro", ValorPago = 30 },
            new Utilizacao { DataHora = new DateTime(2024, 5, 3), Praca = "A", Cidade = "X", Estado = "X", TipoVeiculo = "Carro", ValorPago = 40 }
        );

        await context.SaveChangesAsync();

        var resultado = await service.GerarRelatorioPracasMaisFaturaramAsync(2, new DateTime(2024, 5, 1));

        Assert.Equal(2, resultado.Count);
        Assert.Equal("A", resultado[0].Praca);
        Assert.Equal(60m, resultado[0].FaturamentoTotal);
    }

    [Fact]
    public async Task GerarRelatorioPracasMaisFaturaramAsync_DeveRetornarListaVaziaQuandoNaoHouverDados()
    {
        var service = CriarServiceComDbEmMemoria(out _);
        var resultado = await service.GerarRelatorioPracasMaisFaturaramAsync(5, new DateTime(2024, 1, 1));
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task GerarRelatorioTiposVeiculosPorPracaAsync_DeveAgruparPorTipoVeiculo()
    {
        var service = CriarServiceComDbEmMemoria(out var context);

        context.Utilizacoes.AddRange(
            new Utilizacao { Praca = "P1", Cidade = "X", Estado = "SP", TipoVeiculo = "Moto", ValorPago = 5, DataHora = DateTime.Now },
            new Utilizacao { Praca = "P1", Cidade = "X", Estado = "SP", TipoVeiculo = "Carro", ValorPago = 10, DataHora = DateTime.Now },
            new Utilizacao { Praca = "P1", Cidade = "X", Estado = "SP", TipoVeiculo = "Carro", ValorPago = 10, DataHora = DateTime.Now }
        );

        await context.SaveChangesAsync();

        var resultado = await service.GerarRelatorioTiposVeiculosPorPracaAsync("P1");

        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, r => r.TipoVeiculo == "Carro" && r.Quantidade == 2);
        Assert.Contains(resultado, r => r.TipoVeiculo == "Moto" && r.Quantidade == 1);
    }

    [Fact]
    public async Task GerarRelatorioTiposVeiculosPorPracaAsync_DeveRetornarListaVaziaQuandoNaoHouverDados()
    {
        var service = CriarServiceComDbEmMemoria(out _);
        var resultado = await service.GerarRelatorioTiposVeiculosPorPracaAsync("PracaInexistente");
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task UtilizacaoMessageHandler_DeveChamarInserirUtilizacao()
    {
        var mockService = new Mock<IPedagioService>();
        var handler = new UtilizacaoMessageHandler(mockService.Object);

        var utilizacao = new Utilizacao
        {
            DataHora = DateTime.UtcNow,
            Praca = "Imigrantes",
            Cidade = "São Paulo",
            Estado = "SP",
            TipoVeiculo = "Carro",
            ValorPago = 8.50m
        };

        await handler.Handle(utilizacao);

        mockService.Verify(s => s.InserirUtilizacaoAsync(It.Is<Utilizacao>(u => u.Praca == "Imigrantes")), Times.Once);
    }

    [Fact]
    public async Task UtilizacaoMessageHandler_DeveLancarExcecaoQuandoInsercaoFalhar()
    {
        var mockService = new Mock<IPedagioService>();
        var handler = new UtilizacaoMessageHandler(mockService.Object);

        var utilizacao = new Utilizacao
        {
            DataHora = DateTime.UtcNow,
            Praca = "Anchieta",
            Cidade = "Santos",
            Estado = "SP",
            TipoVeiculo = "Carro",
            ValorPago = 9.99m
        };

        mockService.Setup(s => s.InserirUtilizacaoAsync(It.IsAny<Utilizacao>())).ThrowsAsync(new InvalidOperationException("Erro simulado"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(utilizacao));
        Assert.Equal("Erro simulado", ex.Message);
    }
}
