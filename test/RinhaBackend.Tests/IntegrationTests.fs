module IntegrationTests

open System
open Xunit

[<Fact>]
let ``My test`` () =
    Assert.True(true)

//Obrigatoriamente, o http status code de requisições para transações bem sucedidas deve ser 200!

//Regras Uma transação de débito nunca pode deixar o saldo do cliente menor que seu limite disponível.
//Por exemplo, um cliente com limite de 1000 (R$ 10) nunca deverá ter o saldo menor que -1000 (R$ -10).
//Nesse caso, um saldo de -1001 ou menor significa inconsistência na Rinha de Backend!

//Se uma requisição para débito for deixar o saldo inconsistente, a API deve retornar HTTP Status Code 422 sem completar a transação!
//O corpo da resposta nesse caso não será testado e você pode escolher como o representar.

//Se o atributo [id] da URL for de uma identificação não existente de cliente, a API deve retornar HTTP Status Code 404.
//O corpo da resposta nesse caso não será testado e você pode escolher como o representar.
//Se a API retornar algo como HTTP 200 informando que o cliente não foi encontrado no corpo da resposta ou HTTP 204 sem corpo,
//ficarei extremamente deprimido e a Rinha será cancelada para sempre.
