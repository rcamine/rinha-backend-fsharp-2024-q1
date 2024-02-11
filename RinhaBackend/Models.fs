﻿module Models

type AddTransactionError =
    | Unprocessable
    | NotFound
    | InvalidRequest

type TransactionType =
    | C
    | D
    | Invalid

[<CLIMutable>]
type TransactionRequest =
    { Valor: int
      Tipo: string
      Descricao: string }

[<CLIMutable>]
type TransactionResponse = { Limite: int; Saldo: int }

type Customer =
    { CustomerId: int
      Limite: int
      Saldo: int }

type Transaction =
    { Valor: int
      Tipo: TransactionType
      Descricao: string
      Customer: Customer }

module Transaction =
    let validate (transaction: Transaction) =
        match transaction with
        | transaction when transaction.Tipo = Invalid -> Error InvalidRequest
        | _ -> Ok transaction

    let ofRequest (request: TransactionRequest) customerOption =
        match customerOption with
        | Some customer ->
            Ok
                { Valor = request.Valor
                  Tipo =
                    match request.Tipo with
                    | "c" -> C
                    | "d" -> D
                    | _ -> Invalid
                  Descricao = request.Descricao
                  Customer = customer }
        | None -> Error NotFound
