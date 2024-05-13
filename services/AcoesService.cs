using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using pi5.database;
using pi5.entities;
using pi5.Interfaces.Services;
using PI5.entities;

namespace pi5.services;

public class AcoesService:IAcoesService{
        //Criando uma variável para a classe PI5Context
    private readonly PI5Context _context;
    private readonly IIntegracaoService _integracaoService; 

    //Construtor da classe de services
    public AcoesService(PI5Context context, IIntegracaoService integracaoService){
        _context = context;
        _integracaoService = integracaoService;
    }


    //Task: void do async
    public async Task AtualizaDados(){
        //Validações para adicionar as ações no banco. 
        var todasAcoes = await _integracaoService.TodasAcoes();
        List<string> vinteAcoes = new();

        int contador = 0;

        for(int i = 0; i<todasAcoes.Count; i++){
            if (contador == 20){
                await AtualizaAcoesBanco(vinteAcoes);
                contador = 0;
                vinteAcoes.Clear();
                
            }
            vinteAcoes.Add(todasAcoes[i]);
            contador++;
        }
    
        if(contador > 0){
            await AtualizaAcoesBanco(vinteAcoes);

        }
        
    }

    private async Task AtualizaAcoesBanco(List<string> acoesNome){

        RetornoAPI retornoDados = await _integracaoService.GetDados("quote", acoesNome);
        
        //Verificando se o dado encontrado na API já está inserido na coluna no banco. 
        foreach (var acao in retornoDados.Results)
        {

            //verifica se existe uma ação no banco com o nome da que está sendo inserida
            var acaoBanco = _context.Acoes.FirstOrDefault(x=>x.Nome==acao.LongName);
            if (acaoBanco == null){
                //Se o nome da ação na API ainda nao existir no banco(for nulo), irá instanciar a ação e adicionar no banco. 
                acaoBanco = new Acoes (){
                    Nome = acao.LongName,
                    Logo = acao.Logourl,
                    Sigla = acao.Symbol,

                };
                _context.Acoes.Add(acaoBanco);
                await _context.SaveChangesAsync();

                acaoBanco = _context.Acoes.FirstOrDefault(x=>x.Nome==acao.LongName);
            }

            //pegando todo o conteúdo da determinada ação existente no banco. 
            List<Valores>? valoresBanco = await _context.Valores.Where(x => x.Acao_id == acaoBanco.Id).ToListAsync();
            foreach (var valoresAPI in acao.HistoricalDataPrice){
                DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(valoresAPI.Data);
                if (valoresBanco.Count == 0){
                    Valores valor = new Valores(){
                        Acao_id = acaoBanco.Id,
                        Valor_Fechamento = valoresAPI.Close,
                        Valor_Abertura = valoresAPI.Open,
                        Valor_Alta = valoresAPI.High,
                        Valor_Baixa = valoresAPI.Low,
                        Data = dateTime.LocalDateTime

                    };

                    _context.Valores.Add(valor);

                    await _context.SaveChangesAsync();


                }
                else{
                    if (valoresBanco.Where(x=>x.Data==dateTime.LocalDateTime).FirstOrDefault() == null){
                        Valores valor = new Valores(){
                            Acao_id = acaoBanco.Id,
                            Valor_Fechamento = valoresAPI.Close,
                            Valor_Abertura = valoresAPI.Open,
                            Valor_Alta = valoresAPI.High,
                            Valor_Baixa = valoresAPI.Low,
                            Data = dateTime.LocalDateTime

                        };
                    _context.Valores.Add(valor);
                    await _context.SaveChangesAsync();

                    }
                }

            }
        }
    }


    //Decidir as 5 melhores ações investir o dinheiro com base na análise da ultima semana
    public async Task HistoricoFechamento() {
        
       
        //Pegando os ultimos 30 dias para realizar a média móvel
        DateTime dataInicio = DateTime.Today.AddDays(-30);
        DateTime dataFim = DateTime.Today.AddDays(-1); 
        
        // Camando os métodos e passando os parametros
        var mediaMovel = await GetMediaMovel(dataInicio, dataFim);
        var melhoresAcoes = await MelhorAcao(mediaMovel, 5);
        await AtualizaCarteira(melhoresAcoes);

        // dataInicial = "2024-04-22 00:00:00";
        // dataFinal = "2024-04-26 23:59:59";
        // DateTime.TryParseExact(dataInicial, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dataInicio);
        // DateTime.TryParseExact(dataFinal, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dataFim);

        // //Chamando o metodod
        // await InsercaoHistorico(carteiraId, dataInicio, dataFim);

    }
    public async Task<List<MediaAcoes>> GetMediaMovel(DateTime dataInicio, DateTime dataFim){
        List<Valores> historicoAcoes = await _context.Valores.Where(x=> x.Data >= dataInicio && x.Data <= dataFim).ToListAsync();
        List<Acoes> acoes = await _context.Acoes.ToListAsync();
        List<MediaAcoes> mediaAcoes = new ();


        //Foreach para calcular a soma dos valores de fechamento de cada uma das 5 ações
        foreach(var acao in acoes){
            //Se o id da ação da tabela acoes for igual ao id da tabela Valores
            var historicoLista = historicoAcoes.Where(x=> x.Acao_id == acao.Id).ToList();
            //contador da quantidade de dias da análise
            int contador = 0;
            decimal somaFechamento = 0;

            foreach(var historico in historicoLista){
                somaFechamento+= historico.Valor_Fechamento;
                contador ++;

            }

            if(contador > 0){
                //adicionando o id da ação e a média movel no periodo selecionado na lista de mediaAcoes

                mediaAcoes.Add(new MediaAcoes{
                AcaoId = acao.Id, 
                MediaMovel = somaFechamento / contador,
                Desvio = 0
            });
            }
           

        }
        return mediaAcoes;
        
    } 
//Escolher as 5 melhores ações de todas
    public async Task<List<MediaAcoes>> MelhorAcao(List<MediaAcoes> mediaAcoes, int qtdAcoesEscolhidas){
        DateTime dataInicio = DateTime.Today;
        List<Valores> historicoAcoes = await _context.Valores.Where(x=> x.Data == dataInicio).ToListAsync();

        //Calculando o desvio padrão referente a cada ação da minha lista de mediaAcoes com a data atual. 
        for(int i = 0; i < mediaAcoes.Count; i++){

            mediaAcoes[i].Desvio = (float) Math.Round(((100*historicoAcoes.FirstOrDefault(x=>x.Acao_id == mediaAcoes[i].AcaoId).Valor_Abertura) / mediaAcoes[i].MediaMovel)-100, 2);
            Console.WriteLine($"acaoID: {mediaAcoes[i].AcaoId} | Media Movel: {mediaAcoes[i].MediaMovel} | Valor Abertura: R${historicoAcoes.FirstOrDefault(x=>x.Acao_id == mediaAcoes[i].AcaoId).Valor_Abertura} | Desvio: {mediaAcoes[i].Desvio}%");
        }

        return mediaAcoes.OrderByDescending(x=>x.Desvio).Take(qtdAcoesEscolhidas).ToList();
    }

    //Tabela carteira_acao
    //Método para atualizar a carteira todos os dias, irá manter as ações resultado da MediaAcao que ja possui hoje na carteira e irá trocar aquelas que não saíram no resultado da MediaAcao de hoje
    public async Task AtualizaCarteira(List<MediaAcoes> melhoresAcoes){

        //INDO NO BANCO, VERIFICANDO SE TEM CARTEIRA CADASTRADA E TRAZENDO ELA
        var carteira = await _context.Carteira.FirstOrDefaultAsync();
        //Se a carteira estiver com dados, cria uma lista para armazenar os Id's das ações existentes
        if (carteira != null){
           List<int> acoesId = new();

            //Adicionando o ID das melhores ações na lista acoesId
            foreach(var melhorAcao in melhoresAcoes){
                acoesId.Add(melhorAcao.AcaoId);
            }
            //Comparando com as ações que ja tenho na carteira para nao inserir duplicado
            var acoesCompra = melhoresAcoes.ExceptBy(_context.CarteiraAcao.Where(x=> x.Qtd_cotas > 0).Select(x=> x.Acao_id).ToList(), x=> x.AcaoId);
            //Guardando numa variável as ações resultado de MelhorAcao que são iguais as ações que ja tenho na minha carteira atual
            var acoesIguais = await _context.CarteiraAcao.Where(x=> acoesId.Contains(x.Acao_id)).ToListAsync();
            //Apagando as ações existentes no banco, exceto as ações da interseção(acoesIguais). 
            var acoesVender =  await _context.CarteiraAcao.Where(x=> x.Qtd_cotas > 0).ExceptBy(acoesIguais.Select(x => x.Acao_id), x=> x.Acao_id).ToListAsync();

            //Venda das ações 
            foreach (var acaoVender in acoesVender){
                acaoVender.Qtd_cotas = 0;
                _context.CarteiraAcao.Update(acaoVender);
                await _context.SaveChangesAsync();
                //PEGANDO TODO MUNDO QUE TEM O VALOR VENDA NULO. 
                var historicoCarteira = await _context.HistoricoCarteira.Where(x => x.Acao_id == acaoVender.Acao_id && x.Valor_Venda == null).FirstOrDefaultAsync();
                var valorVenda = (await _context.Valores.Where(x=> x.Data.Date == DateTime.Today.Date).FirstOrDefaultAsync()).Valor_Abertura;
                //vendendo todas as ações cujo o valor de venda anterior era Null
                historicoCarteira.Valor_Venda = valorVenda;
                _context.HistoricoCarteira.Update(historicoCarteira);
                await _context.SaveChangesAsync();


            }
            //Compra das ações (Atribuição  na tabela de Carteira Acao)
            foreach(var acaoCompra in acoesCompra){

                var valorCompra = (await _context.Valores.Where(x=> x.Data.Date == DateTime.Today.Date).FirstOrDefaultAsync()).Valor_Abertura;
               
                var carteiraAcao = await _context.CarteiraAcao.Where(x=> x.Acao_id == acaoCompra.AcaoId).FirstOrDefaultAsync();
                
                if(carteiraAcao != null){
                    carteiraAcao.Qtd_cotas = 3;
                    _context.CarteiraAcao.Update(carteiraAcao);
                    await _context.SaveChangesAsync();

                }
                else{

                    carteiraAcao = new CarteiraAcao(){
                        Carteira_id = carteira.Id,
                        Acao_id = acaoCompra.AcaoId,
                        Qtd_cotas = 3

                    };
                    _context.CarteiraAcao.Add(carteiraAcao);
                    await _context.SaveChangesAsync();

                }

                HistoricoCarteira historicoCarteira = new HistoricoCarteira(){
                    Carteira_id = carteira.Id,
                    Acao_id = acaoCompra.AcaoId,
                    Data_historico = DateTime.Now,
                    Valor_Compra = valorCompra,

                };
                await _context.SaveChangesAsync();
            }
        }

        else{
            //Criando uma nova carteira
            carteira = new Carteira();
            _context.Carteira.Add(carteira);
            await _context.SaveChangesAsync();

            foreach(var acao in melhoresAcoes)
            {
                CarteiraAcao carteiraAcao = new(){
                    Carteira_id = carteira.Id,
                    Acao_id = acao.AcaoId,
                    Qtd_cotas = 3
                };

                _context.CarteiraAcao.Add(carteiraAcao);
                await _context.SaveChangesAsync();
            }

        }

    }
    // //Tabela historico_carteira
    // public async Task InsercaoHistorico(int carteiraId, DateTime dataInicio, DateTime dataFim){
        
    //     List<CarteiraAcao> carteirasAcoes = await _context.CarteiraAcao.Where(x=> x.Carteira_id == carteiraId).ToListAsync();
        
    //     //rodando foreach nas melhores ações a serem investidas
    //     foreach(var acao in carteirasAcoes){
    //         List<Valores> historicoAcoes = await _context.Valores.Where(x=> x.Data >= dataInicio && x.Data <= dataFim && x.Acao_id==acao.Acao_id).ToListAsync();
    //         historicoAcoes = historicoAcoes.OrderBy(x => x.Data).ToList();
    //         for(int i = 0; i < historicoAcoes.Count; i++){
    //             if(i == 0){
    //                 HistoricoCarteira historicoCarteira = new(){
    //                     Carteira_id = carteiraId,
    //                     Acao_id = acao.Acao_id,
    //                     Data_historico = historicoAcoes[i].Data,
    //                     Valor_Fechamento_Acao = historicoAcoes[i].Valor_Abertura*acao.Qtd_cotas
    

    //                 };

    //                 _context.HistoricoCarteira.Add(historicoCarteira);
    //                 await _context.SaveChangesAsync();
    //             }
    //             else{
    //                 HistoricoCarteira historicoCarteira = new(){
    //                     Carteira_id = carteiraId,
    //                     Acao_id = acao.Acao_id,
    //                     Data_historico = historicoAcoes[i].Data,
    //                     Valor_Fechamento_Acao = historicoAcoes[i].Valor_Fechamento*acao.Qtd_cotas
    

    //                 };

    //                 _context.HistoricoCarteira.Add(historicoCarteira);
    //                 await _context.SaveChangesAsync();
    //             }
    //         }
    //     }
    // }
    // // public Task<RetornoDashboardAPI> RetornoDadosDashboard(int carteiraId){
    // //     List<HistoricoCarteira> historicoCarteira = await _context.HistoricoCarteira.Where(x=>x.Carteira_id == carteiraId).ToListAsync();
    // //     List<DateTime> Datas = await _context.
    
    // // }
   public async Task<RetornoDashboardAPI> RetornoDadosDashboard(int carteiraId)
    {
        List<HistoricoCarteira> historicoCarteira = await _context.HistoricoCarteira.Where(x => x.Carteira_id == carteiraId).ToListAsync();
        var datasHistorico = from historico in historicoCarteira
                                    group historico by historico.Data_historico into novoHistorico
                                    orderby novoHistorico.Key
                                    select novoHistorico;

        foreach (var item in datasHistorico)
        {
            Console.WriteLine(item.Key.ToString());
        }

        return new RetornoDashboardAPI();
    }
}

//Rotina

