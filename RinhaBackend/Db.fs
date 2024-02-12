module Db

open Donald
open Microsoft.Data.Sqlite
open Models

//TODO: is this the best approach to have a connString? I think other projects use it as a method
type Db(connString: string) =

    member _.Initialize() = ()

    member _.AddNewTransaction transaction =
        use conn = new SqliteConnection(connString)

        try
            conn
            |> Db.newCommand "UPDATE clientes SET saldo = saldo + @valor WHERE id = @customerId"
            |> Db.setParams
                [ "@valor", SqlType.Int transaction.Valor
                  "@customerId", SqlType.Int transaction.Customer.CustomerId ]
            |> Db.exec

            conn
            |> Db.newCommand "SELECT id, saldo, limite FROM clientes WHERE id = @customerId"
            |> Db.setParams [ "@customerId", SqlType.Int transaction.Customer.CustomerId ]
            |> Db.querySingle (fun rd ->
                { transaction with
                    Customer =
                        { CustomerId = rd.ReadInt32 "id"
                          Saldo = rd.ReadInt32 "saldo"
                          Limite = rd.ReadInt32 "limite" } })
            |> fun x ->
                match x with
                | Some x -> Ok x
                | None -> Error NotFound
        with ex ->
            Error(DbError ex.Message)

    //TODO: try catch
    member _.GetCustomer customerId =
        use conn = new SqliteConnection(connString)

        conn
        |> Db.newCommand "SELECT id, saldo, limite FROM clientes WHERE id = @customerId"
        |> Db.setParams [ "@customerId", SqlType.Int customerId ]
        |> Db.querySingle (fun rd ->
            { CustomerId = customerId
              Saldo = rd.ReadInt32 "saldo"
              Limite = rd.ReadInt32 "limite" })
