module RinhaBackend.Db

open Donald
open Microsoft.Data.Sqlite

//TODO: is this the best approach to have a connString? I think other projects use it as a method
type Db(connString: string) =

    //TODO: insert values and call it from Program.fs
    member _.Initialize() =
        use conn = new SqliteConnection(connString)

        conn
        |> Db.newCommand
            "CREATE TABLE cliente (
                                id integer PRIMARY KEY NOT NULL,
                                saldo integer NOT NULL,
                               limite integer NOT NULL);

                             CREATE TABLE transacao (
                                id SERIAL PRIMARY KEY,
                                valor integer NOT NULL,
                                descricao varchar(10) NOT NULL,
                                realizadaem timestamp NOT NULL,
                                idcliente integer NOT NULL);

                             CREATE INDEX ix_transacao_idcliente ON transacao (idcliente ASC);"
        |> Db.exec

    member _.ResetDb() =
        use conn = new SqliteConnection(connString)

        conn
        |> Db.newCommand
            "delete from transacao;
                            delete from cliente;
                            INSERT INTO cliente (id, saldo, limite) VALUES (1, 0, -100000);
                            INSERT INTO cliente (id, saldo, limite) VALUES (2, 0, -80000);
                            INSERT INTO cliente (id, saldo, limite) VALUES (3, 0, -1000000);
                            INSERT INTO cliente (id, saldo, limite) VALUES (4, 0, -10000000);
                            INSERT INTO cliente (id, saldo, limite) VALUES (5, 0, -500000);"
        |> Db.exec

    //TODO: should consider credit/debit aswell
    member _.AddTransaction transaction =
        use conn = new SqliteConnection(connString)

        try
            conn
            |> Db.newCommand "UPDATE clientes SET saldo = saldo + @valor WHERE id = @customerId"
            |> Db.setParams
                [ "@valor", SqlType.Int transaction.Amount
                  "@customerId", SqlType.Int transaction.Customer.CustomerId ]
            |> Db.exec

            conn
            |> Db.newCommand "SELECT saldo, limite FROM clientes WHERE id = @customerId"
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
            |> Db.newCommand "SELECT id, saldo, limite FROM clientes WHERE id = @customerId"
            |> Db.setParams [ "@customerId", SqlType.Int customerId ]
            |> Db.querySingle (fun rd -> Customer.create customerId (rd.ReadInt32 "saldo") (rd.ReadInt32 "limite"))
            |> Ok
        with ex ->
            Error(DbError ex.Message)
