module RinhaBackend.Tests.UnitTests

open RinhaBackend
open Xunit
open FsUnit.Xunit

//TODO: FsCheck / Expecto?
[<Fact>]
let ``When debit customer balance shouldn't go beyond his limit and should return Unprocessable`` () =
    let transaction =
        { Amount = 1001
          Type = Debit
          Description = "Some debit"
          Customer =
            { CustomerId = 1
              Limit = 1000
              Balance = 0 } }

    let result = Transaction.validate transaction

    result
    |> should equal (Result<Transaction, TransactionError>.Error Unprocessable)
