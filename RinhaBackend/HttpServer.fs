module HttpServer

open Db
open Wire
open Transaction
open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

let dbFromCtx (ctx: HttpContext) =
    ctx.GetService<IConfiguration>().GetConnectionString("DefaultConnection") |> Db

// Handlers
let handleGetStatement (clientId: int) : HttpHandler = (text $"{clientId}")

let handleAddTransaction (customerId: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let db = ctx |> dbFromCtx
        let request = ctx.BindJsonAsync<TransactionRequest>().Result
        let customerOrError = db.GetCustomer customerId

        let transactionResult =
            ofRequest request customerOrError
            |> Result.bind validate
            |> Result.bind db.AddNewTransaction

        match transactionResult with
        | Ok t -> Successful.OK { Limite = t.Customer.Limite ; Saldo = t.Customer.Saldo } next ctx
        | Error e when e = NotFound -> RequestErrors.NOT_FOUND "" next ctx
        | Error e when e = InvalidRequest -> RequestErrors.BAD_REQUEST "" next ctx
        | Error e when e = Unprocessable -> RequestErrors.UNPROCESSABLE_ENTITY "" next ctx
        | Error _ -> ServerErrors.INTERNAL_ERROR "" next ctx

// Endpoints
let endpoints =
    [ GET [ routef "/clientes/%i/extrato/" handleGetStatement ]
      POST [ routef "/clientes/%i/transacoes" handleAddTransaction ] ]
