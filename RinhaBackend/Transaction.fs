﻿module Transaction

open Wire

type Customer =
    { CustomerId: int
      Limite: int
      Saldo: int }

type TransactionType =
    | Credit
    | Debit
    | Invalid

type Transaction =
    { Valor: int
      Tipo: TransactionType
      Descricao: string
      Customer: Customer }

type TransactionError =
    | Unprocessable
    | NotFound
    | InvalidRequest
    | DbError of string

let validate transaction =
    match transaction with
    | transaction when transaction.Tipo = Invalid -> Error InvalidRequest
    //TODO: finish this validation, should consider credit/debit as well
    | transaction when transaction.Valor > transaction.Customer.Limite -> Error Unprocessable
    | _ -> Ok transaction

let ofRequest (request: TransactionRequest) customerOption =
    match customerOption with
    | Some customer ->
        Ok
            { Valor = request.Valor
              Tipo =
                match request.Tipo with
                | "c" -> Credit
                | "d" -> Debit
                | _ -> Invalid
              Descricao = request.Descricao
              Customer = customer }
    | None -> Error NotFound