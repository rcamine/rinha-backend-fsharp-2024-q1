namespace RinhaBackend

[<CLIMutable>]
type TransactionRequest =
    { Valor: int
      Tipo: string
      Descricao: string }

[<CLIMutable>]
type TransactionResponse = { Limite: int; Saldo: int }
