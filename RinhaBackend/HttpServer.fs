module HttpServer

open Db
open Models
open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

// Helpers
let okResultOr errorType aOption =
    match aOption with
    | Some x -> Ok x
    | None -> Error errorType

let dbFromCtx (ctx: HttpContext) =
    ctx.GetService<IConfiguration>().GetConnectionString("DefaultConnection") |> Db

// Handlers
let handleGetStatement (clientId: int) : HttpHandler = (text $"{clientId}")

let handleAddTransaction (customerId: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let db = ctx |> dbFromCtx
        let request = ctx.BindJsonAsync<TransactionRequest>().Result

        let transactionResult =
            db.GetCustomer customerId
            |> Transaction.ofRequest request
            |> Result.bind Transaction.validate
            //|> TODO: se saldo vai ficar negativo, retorno erro unprocessable
            |> Result.bind (fun t -> db.AddNewTransaction t |> okResultOr NotFound)

        match transactionResult with
        | Ok t ->
            Successful.OK
                { Limite = t.Customer.Limite
                  Saldo = t.Customer.Saldo }
                next
                ctx
        | Error e when e = NotFound -> RequestErrors.NOT_FOUND "" next ctx
        | Error e when e = InvalidRequest -> RequestErrors.BAD_REQUEST "" next ctx
        | Error e when e = Unprocessable -> RequestErrors.UNPROCESSABLE_ENTITY "" next ctx
        | Error _ -> ServerErrors.INTERNAL_ERROR "" next ctx

// Endpoints
let endpoints =
    [ GET [ routef "/clientes/%i/extrato/" handleGetStatement ]
      POST [ routef "/clientes/%i/transacoes" handleAddTransaction ] ]
