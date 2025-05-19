# JANORM.CLIENT

O JANORM.Client é o Client do JANORM, uma biblioteca de ORM (Object-Relational Mapping) para .NET. O intuito do JANORM.Client é fornecer recursos para inicialização do JANORM no projeto, como a criação do `schema.jan`, a leitura e inserção das entidades neste schema, e realizar o `push` do schema para o banco de dados.

## Instalação
Para instalar o JANORM.Client crie uma nova tool no seu projeto e adicione a referência ao `JANORM.Client`:
```bash
dotnet new tool-manifest
dotnet tool install JANORM.Client --add-source <diretorio-do-pacote> --version <versao-mais-recente>
```
Vale ressaltar que é necessário usar os comandos `dotnet build` e `dotnet pack` no projeto `JANORM.Client` para gerar o pacote NuGet. Depois, adicione o pacote NuGet gerado ao seu projeto principal.

## Dependências
- .NET 7.0
- Banco de dados SQLITE
- Pacotes Nuget (necessário estar instalado tanto no projeto principal quanto no projeto do JANORM):
    - JANORM.Core 
    - Microsoft.Extensions.DependencyInjection
    - Microsoft.Data.Sqlite
    - System.CommandLine
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

## Exemplo de uso

A primeira coisa a se fazer é criar um arquivo `schema.jan` na raiz do seu projeto. Esse arquivo será utilizado para armazenar as entidades que você deseja mapear para o banco de dados. O arquivo deve conter as seguintes informações:

```json
{
    "Source": {
    "DatabaseUrl": "env(DATA_SOURCE)",
    "Host": "sqlite"
    },
    "entities": [],
}
```
Para criar o arquivo `schema.jan` você pode usar o comando `dotnet janx init` no terminal. Esse comando irá criar o arquivo `schema.jan` em seu projeto (importante executar o comando na raíz do projeto).

Feito isso, é possível criar a definição das entidades que você deseja mapear para o banco de dados. Para isso, garanta que as classes tenham os *Attributes* `Entity` e `Id` em uma propriedade do tipo `int` ou `Guid` e execute o comando a seguir:

```bash
dotnet janx generate --project-assembly .\bin\Debug\net7.0\testProject.dll (o caminho até o assembly do projeto)
```
Esse comando irá gerar as entidades no arquivo `schema.jan`.

Finalmente, basta executar o comando `dotnet janx push` para inserir as entidades no banco de dados. O comando irá criar as tabelas no banco de dados.