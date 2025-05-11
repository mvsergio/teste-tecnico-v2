using Rebus.Handlers;
using Thunders.TechTest.ApiService.Models;
using Thunders.TechTest.ApiService.Services;

namespace Thunders.TechTest.ApiService.Handlers

{
    public class UtilizacaoMessageHandler(PedagioService pedagioService) : IHandleMessages<Utilizacao>
    {
        private readonly PedagioService _pedagioService = pedagioService;

        public async Task Handle(Utilizacao message)
        {
            await _pedagioService.InserirUtilizacaoAsync(message);
        }
    }
}