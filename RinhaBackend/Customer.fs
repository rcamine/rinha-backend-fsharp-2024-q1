namespace RinhaBackend

type CustomerType =
    { CustomerId: int
      Limit: int
      Balance: int }

module Customer =
    let create customerId limit balance =
        { CustomerId = customerId
          Limit = limit
          Balance = balance }
