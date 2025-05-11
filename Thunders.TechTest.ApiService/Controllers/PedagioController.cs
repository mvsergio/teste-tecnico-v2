using Microsoft.AspNetCore.Mvc;
using Thunders.TechTest.ApiService.Models;
using Thunders.TechTest.ApiService.Services;
using Thunders.TechTest.OutOfBox.Queues;

namespace Thunders.TechTest.ApiService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PedagioController(PedagioService pedagioService, IMessageSender messageSender) : ControllerBase
{
    [HttpPost("utilizacao")]
    public async Task<IActionResult> EnviarUtilizacao([FromBody] Utilizacao utilizacao)
    {
        await messageSender.SendLocal(utilizacao);
        return Accepted();
    }

    [HttpGet("relatorio/valor-por-hora")]
    public async Task<IActionResult> GerarRelatorioValorPorHora(string cidade)
    {
        var relatorio = await pedagioService.GerarRelatorioValorTotalPorHoraAsync(cidade);
        return Ok(relatorio);
    }

    [HttpGet("relatorio/pracas-mais-faturaram")]
    public async Task<IActionResult> GerarRelatorioPracasMaisFaturaram(int quantidade, DateTime mes)
    {
        var relatorio = await pedagioService.GerarRelatorioPracasMaisFaturaramAsync(quantidade, mes);
        return Ok(relatorio);
    }

    [HttpGet("relatorio/tipos-veiculos")]
    public async Task<IActionResult> GerarRelatorioTiposVeiculos(string praca)
    {
        var relatorio = await pedagioService.GerarRelatorioTiposVeiculosPorPracaAsync(praca);
        return Ok(relatorio);
    }
}