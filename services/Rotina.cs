
using pi5.Interfaces.Services;
namespace pi5.services;

public class Rotina : BackgroundService{

    private readonly IAcoesService _acoesService;

    public Rotina (IAcoesService acoesService) {
        _acoesService = acoesService;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken){

        while (!stoppingToken.IsCancellationRequested){

            TimeSpan rodarRotina = new TimeSpan(10, 1, 0); //Hora que quero que minha rotina rode
            TimeSpan agora = DateTime.Now.TimeOfDay;

            if(rodarRotina == agora){
                _acoesService.HistoricoFechamento();
            }

            Task.Delay(60000);
        }
        return Task.CompletedTask;
    }
}