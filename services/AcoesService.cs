using System;
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
        RetornoAPI retornoDados = await _integracaoService.GetDados("quote", "BBAS3");
        //Verificando se o dado encontrado na API já está inserido na coluna NOME no banco. 
        var acao = _context.Acoes.FirstOrDefault(x=>x.Nome==retornoDados.Results[0].LongName);
        if (acao == null){
            //Se o nome da ação na API ainda nao existir no banco(for nulo), irá instanciar a ação e adicionar no banco. 
            acao = new Acoes (){
                Nome = retornoDados.Results[0].LongName,
                Logo = retornoDados.Results[0].Logourl,
                Sigla = retornoDados.Results[0].Symbol,

            };
            _context.Acoes.Add(acao);

    }
}
}