module UnitTests

open RinhaBackend
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``When debit customer balance shouldn't go beyond his limit and should return Unprocessable`` () =
    let customer = Customer.create 1 1000 0

    let transaction =
        Transaction.create
            { Valor = 1001
              Tipo = "d"
              Descricao = "Some debit" }
            customer

    let result = Transaction.validate transaction

    result
    |> should equal (Result<Transaction, TransactionError>.Error Unprocessable)
