using Thunders.TechTest.ApiService.DTO;
using Thunders.TechTest.ApiService.Models;

namespace Thunders.TechTest.ApiService.Services
{
    public interface IPedagioService
    {
        Task<List<RelatorioPracasMaisFaturaramDTO>> GerarRelatorioPracasMaisFaturaramAsync(int quantidade, DateTime mes);
        Task<List<RelatorioTiposVeiculosDTO>> GerarRelatorioTiposVeiculosPorPracaAsync(string praca);
        Task<List<RelatorioValorPorHoraDTO>> GerarRelatorioValorTotalPorHoraAsync(string cidade);
        Task InserirUtilizacaoAsync(Utilizacao utilizacao);
    }
}