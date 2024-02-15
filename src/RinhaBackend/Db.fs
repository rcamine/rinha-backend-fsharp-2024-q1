module RinhaBackend.Db

open Donald
open Microsoft.Data.Sqlite

type Db(connString: string) =

    //TODO: need to finish this method
    member _.AddTransaction transaction =
        use conn = new SqliteConnection(connString)

        try
            conn
            |> Db.newCommand "UPDATE cliente SET saldo = saldo + @valor WHERE id = @customerId"
            |> Db.setParams
                [ "@valor", SqlType.Int transaction.Amount
                  "@customerId", SqlType.Int transaction.Customer.CustomerId ]
            |> Db.exec

            conn
            |> Db.newCommand "SELECT saldo, limite FROM cliente WHERE id = @customerId"
            |> Db.setParams [ "@customerId", SqlType.Int transaction.Customer.CustomerId ]
            |> Db.querySingle (fun rd ->
                { transaction with
                    Customer =
                        Customer.create transaction.Customer.CustomerId (rd.ReadInt32 "saldo") (rd.ReadInt32 "limite") })
            |> fun txOption ->
                match txOption with
                | Some tx -> Ok tx
                | None -> Error NotFound
        with ex ->
            Error(DbError ex.Message)

    member _.GetCustomer customerId =
        use conn = new SqliteConnection(connString)

        try
            conn
            |> Db.newCommand "SELECT id, saldo, limite FROM cliente WHERE id = @customerId"
            |> Db.setParams [ "@customerId", SqlType.Int customerId ]
            |> Db.querySingle (fun rd -> Customer.create customerId (rd.ReadInt32 "saldo") (rd.ReadInt32 "limite"))
            |> Ok
        with ex ->
            Error(DbError ex.Message)
