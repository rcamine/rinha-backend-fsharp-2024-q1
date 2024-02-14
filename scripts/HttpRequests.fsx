#r "nuget: FsHttp"

open FsHttp
open FsHttp.Operators

type TransacaoResponse = { Limite: int; Saldo: int }

%http {
    POST "http://localhost:5000/clientes/1/transacoes"
    body

    jsonSerialize
        {| valor = -100
           tipo = "c"
           descricao = "Teste" |}
}
|> Response.deserializeJson<TransacaoResponse>
