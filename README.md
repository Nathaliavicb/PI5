# Pastas criadas (RF01):


+ Controllers:
  + Pasta **AcoesController**: Cria as rotas http que serão utilizadas. 
+ Database:
  + **PI5Context**: Pasta em que é feito a conexão no banco, nela está informando que as variáveis "Acoes" e "Valores" correspondem às tabelas no banco.
        Toda vez que chamar o Context.Acoes/Valores em alguma classe, irá chamar a tabela no banco. 
+ Entities:
  + **Acoes**: Cria a classe com os atributos da variável Acoes que corresponde a tabela de acoes no banco, conforme descrito na classe PI5Context.
  + **Valores**: Cria a classe com os atributos da variável Valores que corresponde a tabela de valores no banco, conforme descrito na classe PI5Context.
  + **RetornoAPI**: Classe que diz quais as informações do json iremos pegar e cria uma variável para cada informação. 
+ Interfaces:
  + **IAcoesService**: Interface que cria a classe AtualizaDados responsável por fazer a operação de atualização e inserção das açoes no banco. 
  + **IIntegracaoService**: Interface que cria a classe GetDados responsável por pegar o retorno da API que está no link parametrizado. CLasse permite inserir o caminho('quote') e o nome da ação que o usuário deseja inserir no banco. (parametrização ocorre direto na clase 'AcoesService').
+ Services:
  + **AcoesService**: Classe que tem o método 'AtualizaDados' que insere dados nas tabelas de Acoes e VAlores no banco. 
  + **IntegracaoService**: Classe de integração que pega as informações do Appsettings(token, url, conexão com o banco) através do IConfiguration. Cria o link que será acessado por meio do método GET
 
# RF02.

+ Para a entrega da RF02, será necessário escolher as melhores ações de um grupo de ações baseado nos dados históricos. 
+ No nosso projeto, escolhemos 5 ações e analisamos os dados delas no período do dia 08-04 ao dia 12-04-2024 (5 dias), tiramos a média móvel do Valor_Fechamento de cada uma das 5 ações no período referido (5 dias da semana x 5 ações).
+ Como o objetivo era começar o investimento no dia 15-04, pegamos o valor_Abertura do dia 15-04 e calculamos o desvio padrão do valor_Abertura do dia 15-04 em relação a média móvel que cada ação teve na semana anterior. 
+ Das 5 ações, escolhemos as 3 que tiveram o maior desvio padrão em relação a ao valor de Abertura do dia 15-04, visto que as ações com maior desvio tem uma tendencia maior a crescer em curto prazo. Portanto, a primeira parte da análise está finalizada, pegamos as 3 melhores ações mais tendenciosas de um grupos de 5.
+ Finalizamos nosso investimento no dia 19-04, com os dados de perda e ganho que tivemos em cada dia da semana na nossa carteira.

**Calculo Média Móvel:**
**Calulo Desvio Padrão:**
