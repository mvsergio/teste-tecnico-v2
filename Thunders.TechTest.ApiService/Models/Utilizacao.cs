namespace Thunders.TechTest.ApiService.Models;

public class Utilizacao
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; }
    public required string Praca { get; set; }
    public required string Cidade { get; set; }
    public required string Estado { get; set; }
    public decimal ValorPago { get; set; }
    public required string TipoVeiculo { get; set; } 
}