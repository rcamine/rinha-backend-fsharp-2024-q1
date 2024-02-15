namespace RinhaBackend

[<CLIMutable>]
type TransactionRequest =
    { Valor: int
      Tipo: string
      Descricao: string }

[<CLIMutable>]
[<Struct>]
type TransactionResponse = { Limite: int; Saldo: int }
