module RinhaBackend.HttpServer

open RinhaDb
open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

let handleGetStatement (clientId: int) : HttpHandler = (text $"{clientId}")

//TODO: logs
let handleAddTransaction (customerId: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let db = ctx.GetService<IConfiguration>().GetConnectionString("DefaultConnection") |> RinhaDb
        let request = ctx.BindJsonAsync<TransactionRequest>().Result
        
        let result =
            db.GetCustomer customerId
            |> Result.bind (Transaction.create request) 
            |> Result.bind Transaction.validate
            |> Result.bind db.AddTransaction
        
        match result with
        | Ok tx -> Successful.OK { Limite = tx.Customer.Limit ; Saldo = tx.Customer.Balance } next ctx
        | Error NotFound -> RequestErrors.NOT_FOUND "" next ctx
        | Error InvalidRequest -> RequestErrors.BAD_REQUEST "" next ctx
        | Error Unprocessable -> RequestErrors.UNPROCESSABLE_ENTITY "" next ctx
        | Error (DbError _) -> ServerErrors.INTERNAL_ERROR "" next ctx

let endpoints =
    [ GET [ routef "/clientes/%i/extrato/" handleGetStatement ]
      POST [ routef "/clientes/%i/transacoes" handleAddTransaction ] ]
