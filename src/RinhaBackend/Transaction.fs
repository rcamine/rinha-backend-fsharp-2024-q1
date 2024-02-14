namespace RinhaBackend

type TransactionType =
    | Credit
    | Debit
    | Invalid

type Error =
    | Unprocessable
    | NotFound
    | InvalidRequest
    | DbError of string

type Transaction =
    { Amount: int
      Type: TransactionType
      Description: string
      Customer: Customer }

[<RequireQualifiedAccess>]
module Transaction =
    let validate transaction =
        match transaction with
        //TODO: check for negative inputs, etc.
        | transaction when transaction.Type = Invalid -> Error InvalidRequest
        //TODO: need to finish this validation, this is just a sample, btw should consider credit/debit as well
        | transaction when transaction.Amount > transaction.Customer.Limit -> Error Unprocessable
        | _ -> Ok transaction

    let create request customerOption =
        match customerOption with
        | None -> Error NotFound
        | Some customer ->
            Ok
                { Amount = request.Valor
                  Type =
                    match request.Tipo with
                    | "c" -> Credit
                    | "d" -> Debit
                    | _ -> Invalid
                  Description = request.Descricao
                  Customer = customer }
