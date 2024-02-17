module RinhaBackend.Tests.IntegrationTests

open TestHelpers
open System.Net
open System.Net.Http
open FsUnit.Xunit
open Xunit

//todo: seeding should be done in each case, this can lead to db sharing between tests
[<Fact>]
let ``When transaction is successful should return 200 OK`` () =
    let request = new HttpRequestMessage(HttpMethod.Post, "/clientes/1/transacoes")
    request.Content <- new StringContent("""{"valor": 100, "tipo": "c", "descricao": "some debit"}""")
    let response = testRequest request
    response.StatusCode |> should equal HttpStatusCode.OK
    Assert.Equal((httpResponseContent response), """{"limite":100,"saldo":100000}""")

[<Fact>]
let ``When transaction is unprocessable should return 422 UnprocessableEntity`` () =
    let request = new HttpRequestMessage(HttpMethod.Post, "/clientes/1/transacoes")
    request.Content <- new StringContent("""{"valor": 100001, "tipo": "d", "descricao": "some debit"}""")
    let response = testRequest request
    response.StatusCode |> should equal HttpStatusCode.UnprocessableEntity

[<Fact>]
let ``When request id doesn't exist should return 404 NotFound`` () =
    let request = new HttpRequestMessage(HttpMethod.Post, "/clientes/99/transacoes")
    request.Content <- new StringContent("""{"valor": 100, "tipo": "d", "descricao": "some debit"}""")
    let response = testRequest request
    response.StatusCode |> should equal HttpStatusCode.NotFound
