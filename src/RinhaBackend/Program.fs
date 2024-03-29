module RinhaBackend.App

open System
open System.Text.Json
open System.Text.Json.Serialization
open DbUp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http.Timeouts
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.EndpointRouting

let runMigrations (config: IConfiguration) =
    let connString = config.GetConnectionString("DefaultConnection")

    let upgrader =
        DeployChanges.To
            .SQLiteDatabase(connString)
            .WithScriptsFromFileSystem("Migrations")
            .LogToConsole()
            .Build()

    upgrader.PerformUpgrade()

let genericExceptionHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(genericExceptionHandler).UseHttpsRedirection())
        .UseRouting()
        .UseEndpoints(fun routeBuilder -> routeBuilder.MapGiraffeEndpoints(HttpServer.endpoints))
    |> ignore

let configureServices (services: IServiceCollection) =
    services.AddRequestTimeouts(fun options ->
        options.DefaultPolicy <- RequestTimeoutPolicy(Timeout = TimeSpan.FromSeconds(60.0)))
    |> ignore

    let serviceProvider = services.BuildServiceProvider()

    let config = serviceProvider.GetService<IConfiguration>()
    let shouldRunMigrations = config.GetValue<bool>("RunMigrations")

    if shouldRunMigrations then
        runMigrations config |> ignore

    services.AddRouting() |> ignore
    services.AddGiraffe() |> ignore
    let serializationOptions = SystemTextJson.Serializer.DefaultOptions
    serializationOptions.Converters.Add(JsonFSharpConverter(JsonUnionEncoding.FSharpLuLike))
    serializationOptions.PropertyNamingPolicy <- JsonNamingPolicy.SnakeCaseLower

    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(serializationOptions))
    |> ignore

[<EntryPoint>]
let main args =
    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
