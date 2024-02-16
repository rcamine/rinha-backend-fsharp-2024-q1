namespace RinhaBackend

type TransactionType =
    | Credit
    | Debit
    | Invalid

type TransactionError =
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
        | tx when
            tx.Description.Length > 10
            || tx.Description.Length < 1
            || tx.Amount <= 0
            || tx.Type = Invalid
            ->
            Error InvalidRequest
        | tx when tx.Type = Debit && tx.Customer.Limit < (tx.Customer.Balance + tx.Amount) -> Error Unprocessable
        | _ -> Ok transaction

    let createFrom request customerOption =
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

    let create request customer =
        { Amount = request.Valor
          Type =
            match request.Tipo with
            | "c" -> Credit
            | "d" -> Debit
            | _ -> Invalid
          Description = request.Descricao
          Customer = customer }
