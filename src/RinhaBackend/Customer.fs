namespace RinhaBackend

type Customer =
    { CustomerId: int
      Limit: int
      Balance: int }

[<RequireQualifiedAccess>]
module Customer =
    let create customerId limit balance =
        { CustomerId = customerId
          Limit = limit
          Balance = balance }
