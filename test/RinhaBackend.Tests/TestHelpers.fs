module RinhaBackend.Tests.TestHelpers


open System
open System.IO
open System.Net.Http
open DbUp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

let testConfig =
    dict [ "ConnectionStrings:DefaultConnection", "Data Source=InMemorySample;Mode=Memory;Cache=Shared" ]

let configureTestAppConfiguration =
    fun (context: WebHostBuilderContext) (builder: IConfigurationBuilder) ->
        builder.AddInMemoryCollection testConfig |> ignore

let seed (connString: string) =
    let seedingPath = Path.Combine(Directory.GetCurrentDirectory(), "Seeding")

    let upgrader =
        DeployChanges.To
            .SQLiteDatabase(connString)
            .WithScriptsFromFileSystem(seedingPath)
            .LogToConsole()
            .Build()

    upgrader.PerformUpgrade() |> ignore

let getTestHost () =
    WebHostBuilder()
        .UseTestServer()
        .ConfigureAppConfiguration(configureTestAppConfiguration)
        .Configure(Action<IApplicationBuilder> RinhaBackend.App.configureApp)
        .ConfigureServices(RinhaBackend.App.configureServices)
        .ConfigureLogging(RinhaBackend.App.configureLogging)
        .UseUrls([| "http://localhost:5000"; "http://localhost:5001" |])

let testRequest (request: HttpRequestMessage) =
    let resp =
        task {
            use server = new TestServer(getTestHost ())
            use client = server.CreateClient()

            let connString =
                server.Host.Services
                    .GetService<IConfiguration>()
                    .GetConnectionString("DefaultConnection")

            use conn = new SqliteConnection(connString)
            do! conn.OpenAsync()
            seed connString

            let! response = request |> client.SendAsync
            return response
        }

    resp.Result

let httpRequest (httpMethod: HttpMethod) (endpoint: string) =
    new HttpRequestMessage(httpMethod, endpoint)

let httpResponseContent (httpResponseMessage: HttpResponseMessage) =
    httpResponseMessage.Content.ReadAsStringAsync().Result
