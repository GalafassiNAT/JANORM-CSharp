# JANORM.Core

JANORM.Core é um mini-ORM (Object-Relational Mapper) bem inspirado no JPA, desenvolvido em C# para facilitar a integração entre aplicações .NET e bancos de dados relacionais, especificamente o SQLITE. Ele abstrai operações comuns de CRUD (Create, Read, Update, Delete), permitindo que desenvolvedores manipulem dados de forma orientada a objetos.

## Funcionalidades

- **Mapeamento de entidades:** Converte classes C# em tabelas do banco de dados.
- **Consultas simplificadas:** Permite executar queries SQL e mapear resultados para objetos.
- **Operações CRUD:** Métodos para inserir, atualizar, remover e buscar entidades.
- **Configuração flexível:** Suporte a atributos para customizar o mapeamento de propriedades.

## Como funciona

1. **Defina suas entidades:** Crie classes C# representando suas tabelas.
2. **Aplique atributos:** Utilize atributos para definir chaves primárias, nomes de colunas e restrições.
3. **Utilize o contexto:** Instancie o contexto JANORM para executar operações no banco de dados.
4. **Execute operações:** Use métodos como `Insert`, `Update`, `Delete`, `FindById`, `FindAll` e `FindOne`(query especificada pelo usuário) para manipular dados.
5. **Atributos `NULL`**: Por padrão, todos os atributos são `NOT NULL`, caso deseje que um atributo seja `NULL`, basta defini-lo como opcional, exemplo:

```csharp
    public int? Idade { get; set; }
```

## Exemplo de uso

```csharp
[Entity(TABLE_NAME)]
public class Usuario
{   
    public const string  TABLE_NAME = "Usuários";

    [Id(GenerationMethod.UUID)]
    public int Id { get; set; }
    public string Nome { get; set; }
}

var contexto = new JanormContext("sua-string-de-conexao");
contexto.Insert(new Usuario { Nome = "Maria" });
var usuarios = contexto.Query<Usuario>("SELECT * FROM usuarios");
```
### Uso de Repositories

Os repositories são uma abstração adicional que permite encapsular a lógica de acesso às operações de banco de dados. Nativamente, o JANORM já possui métodos para realizar operações CRUD, mas os repositories podem ser úteis para organizar melhor o código e separar a lógica de acesso a dados da lógica de negócios.

Exemplo de uso de um repository:

```csharp
public class UserRepository : JanRepository<User, Guid>
{
    public UserRepository(IDBService dbService) : base(dbService){}

}
```

O repository `UserRepository` herda de `JanRepository`, que é uma classe base fornecida pelo JANORM para facilitar a implementação de repositórios. O construtor do repositório recebe um serviço de banco de dados (`IDBService`) que é usado para executar operações no banco. A classe `User` representa a entidade que será manipulada pelo repositório, e `Guid` é o tipo da chave primária da entidade.

### O que é necessário na `MAIN` do projeto:
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        Env.Load(); // ler variáveis de ambiente
        var services = new ServiceCollection(); // Configuração do DI

        // 0) Configuração do SQLite
        string connString = Utils.GetConnectionString();

        // 1) Fábrica e serviço de DB
        services.AddSingleton<IDBFactory>(provider => new SqliteConnectionFactory(connString));
        services.AddTransient<IDBService, SqliteDBService>();

        // 2) Repositório genérico
        services.AddTransient(
            typeof(IRepository<,>),
            typeof(JanRepository<,>)
        );

        // 3) Repositórios específicos
        services.AddTransient<TestRepository>();
        services.AddTransient<CarRepository>();

        var provider = services.BuildServiceProvider();

        Test t1 = new("Name02", 20, "Description02", 1.78f, 65.0f);

        var testRepository = provider.GetRequiredService<TestRepository>();

        // 4) Operações CRUD
        testRepository.Insert(t1);
        var test = testRepository.FindById(t1.Id);
        test.Name = "Name03";
        testRepository.Update(test);
        testRepository.Delete(test.Id);
        var allTests = testRepository.FindAll();
        foreach (var test in allTests)
        {
            Console.WriteLine($"Id: {test.Id}, Name: {test.Name}, Description: {test.Description}");
        }
        
    }
}
```


## Requisitos

- .NET 7.0
- Banco de dados SQLITE 
- Pacotes Nuget (necessário estar instalado tanto no projeto principal quanto no projeto do JANORM):
    - Microsoft.Extensions.DependencyInjection
    - Microsoft.Data.Sqlite
    - DotNetEnv
    - System.Text.Json
    - System.Text.Json.Serialization
    - System.Collections.Generic
    - System.Linq
    - System.Threading.Tasks
    - System.Threading
    - System.IO
    - System.Reflection
    - System.Data.Common

## Instalação

Adicione o projeto JANORM.Core à sua solução e referencie-o em seu projeto principal.
Garanta que você rodou os comandos `dotnet build` e `dotnet pack` em JANORM.Core para gerar o pacote NuGet.
Depois, adicione o pacote NuGet gerado ao seu projeto principal.

```bash
dotnet add package <nome-do-pacote> <versão-mais-recente> --source <caminho-do-pacote>
```

### Exemplos
Veja exemplos completos explorando este [repositório](https://github.com/GalafassiNAT/testProgramForJANORM).

