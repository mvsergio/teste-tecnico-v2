using Rebus.Handlers;
using Thunders.TechTest.ApiService.Models;
using Thunders.TechTest.ApiService.Services;

namespace Thunders.TechTest.ApiService.Handlers

{
    public class UtilizacaoMessageHandler(IPedagioService pedagioService) : IHandleMessages<Utilizacao>
    {
        private readonly IPedagioService _pedagioService = pedagioService;

        public async Task Handle(Utilizacao message)
        {
            await _pedagioService.InserirUtilizacaoAsync(message);
        }
    }
}