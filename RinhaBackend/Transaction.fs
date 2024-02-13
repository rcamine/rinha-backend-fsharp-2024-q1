namespace RinhaBackend

type Mode =
    | Credit
    | Debit
    | Invalid

type TransactionType =
    { Amount: int
      Mode: Mode
      Description: string
      Customer: CustomerType }

type Error =
    | Unprocessable
    | NotFound
    | InvalidRequest
    | DbError of string

module Transaction =
    let validate transaction =
        match transaction with
        | transaction when transaction.Mode = Invalid -> Error InvalidRequest
        //TODO: finish this validation, should consider credit/debit as well
        | transaction when transaction.Amount > transaction.Customer.Limit -> Error Unprocessable
        | _ -> Ok transaction

    let create request customerOption =
        match customerOption with
        | None -> Error NotFound
        | Some customer ->
            Ok
                { Amount = request.Valor
                  Mode =
                    match request.Tipo with
                    | "c" -> Credit
                    | "d" -> Debit
                    | _ -> Invalid
                  Description = request.Descricao
                  Customer = customer }
