using System;
using Microsoft.AspNetCore.Mvc;
using pi5.Interfaces.Services;

namespace pi5.Controllers;

[ApiController]
[Route("api/[Controller]")]

public class AcoesController:ControllerBase{

    private readonly IAcoesService _acoesService;
    public AcoesController (IAcoesService acoesService){
        _acoesService = acoesService;
    }

    //criando rota de teste
    [HttpGet("teste")]
    public string Teste() {
        return "Ei";

    }
    //Rota para atualizar os dados das ações
    [HttpPost("atualizar-dados")]
    public void AtualizarDados() {
        _acoesService.AtualizaDados();

    }

}