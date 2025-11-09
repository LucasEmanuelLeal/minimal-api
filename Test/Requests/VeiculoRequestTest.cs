using System.Net;
using System.Text;
using System.Text.Json;
using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public class VeiculoRequestTest
{
    private static string? tokenAdm = null;
    private static string? tokenEditor = null;
    private static readonly object lockObject = new object();
    private static bool inicializado = false;

    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Setup.ClassCleanup();
    }

    private Task InicializarTokensAsync()
    {
        if (inicializado) return Task.CompletedTask;

        lock (lockObject)
        {
            if (inicializado) return Task.CompletedTask;
            
            var taskAdm = TokenHelper.ObterTokenAsync(Setup.client, "adm@teste.com", "123456");
            var taskEditor = TokenHelper.ObterTokenAsync(Setup.client, "editor@teste.com", "123456");
            
            Task.WaitAll(taskAdm, taskEditor);
            
            tokenAdm = taskAdm.Result;
            tokenEditor = taskEditor.Result;
            inicializado = true;
        }
        
        return Task.CompletedTask;
    }

    private HttpRequestMessage CriarRequestComToken(HttpMethod method, string url, string? token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Add("Authorization", $"Bearer {token}");
        }

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    [TestMethod]
    public async Task TestarCriarVeiculoComTokenAdm()
    {
        // Arrange
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Fiesta 2.0",
            Marca = "Ford",
            Ano = 2013
        };

        var request = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenAdm, veiculoDTO);

        // Act
        var response = await Setup.client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var veiculo = JsonSerializer.Deserialize<Veiculo>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(veiculo);
        Assert.AreEqual("Fiesta 2.0", veiculo?.Nome);
        Assert.AreEqual("Ford", veiculo?.Marca);
        Assert.AreEqual(2013, veiculo?.Ano);
    }

    [TestMethod]
    public async Task TestarCriarVeiculoComTokenEditor()
    {
        // Arrange
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Civic",
            Marca = "Honda",
            Ano = 2020
        };

        var request = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenEditor, veiculoDTO);

        // Act
        var response = await Setup.client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task TestarCriarVeiculoSemToken()
    {
        // Arrange
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Corolla",
            Marca = "Toyota",
            Ano = 2018
        };

        var request = CriarRequestComToken(HttpMethod.Post, "/veiculos", null, veiculoDTO);

        // Act
        var response = await Setup.client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task TestarListarVeiculos()
    {
        // Arrange
        await InicializarTokensAsync();
        var request = CriarRequestComToken(HttpMethod.Get, "/veiculos", tokenAdm);

        // Act
        var response = await Setup.client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var veiculos = JsonSerializer.Deserialize<List<Veiculo>>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(veiculos);
    }

    [TestMethod]
    public async Task TestarBuscarVeiculoPorId()
    {
        // Arrange - Primeiro criar um veículo
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Gol",
            Marca = "Volkswagen",
            Ano = 2015
        };

        var requestCriar = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenAdm, veiculoDTO);
        var responseCriar = await Setup.client.SendAsync(requestCriar);
        var resultadoCriar = await responseCriar.Content.ReadAsStringAsync();
        var veiculoCriado = JsonSerializer.Deserialize<Veiculo>(resultadoCriar, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Buscar o veículo criado
        var requestBuscar = CriarRequestComToken(HttpMethod.Get, $"/veiculos/{veiculoCriado?.Id}", tokenAdm);
        var responseBuscar = await Setup.client.SendAsync(requestBuscar);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, responseBuscar.StatusCode);

        var result = await responseBuscar.Content.ReadAsStringAsync();
        var veiculo = JsonSerializer.Deserialize<Veiculo>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(veiculo);
        Assert.AreEqual(veiculoCriado?.Id, veiculo?.Id);
        Assert.AreEqual("Gol", veiculo?.Nome);
    }

    [TestMethod]
    public async Task TestarAtualizarVeiculo()
    {
        // Arrange - Primeiro criar um veículo
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Palio",
            Marca = "Fiat",
            Ano = 2016
        };

        var requestCriar = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenAdm, veiculoDTO);
        var responseCriar = await Setup.client.SendAsync(requestCriar);
        var resultadoCriar = await responseCriar.Content.ReadAsStringAsync();
        var veiculoCriado = JsonSerializer.Deserialize<Veiculo>(resultadoCriar, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Atualizar o veículo
        var veiculoDTOAtualizado = new VeiculoDTO
        {
            Nome = "Palio Fire",
            Marca = "Fiat",
            Ano = 2017
        };

        var requestAtualizar = CriarRequestComToken(HttpMethod.Put, $"/veiculos/{veiculoCriado?.Id}", tokenAdm, veiculoDTOAtualizado);
        var responseAtualizar = await Setup.client.SendAsync(requestAtualizar);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, responseAtualizar.StatusCode);

        var result = await responseAtualizar.Content.ReadAsStringAsync();
        var veiculoAtualizado = JsonSerializer.Deserialize<Veiculo>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(veiculoAtualizado);
        Assert.AreEqual("Palio Fire", veiculoAtualizado?.Nome);
        Assert.AreEqual(2017, veiculoAtualizado?.Ano);
    }

    [TestMethod]
    public async Task TestarAtualizarVeiculoComEditor()
    {
        // Arrange - Primeiro criar um veículo
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Uno",
            Marca = "Fiat",
            Ano = 2015
        };

        var requestCriar = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenAdm, veiculoDTO);
        var responseCriar = await Setup.client.SendAsync(requestCriar);
        var resultadoCriar = await responseCriar.Content.ReadAsStringAsync();
        var veiculoCriado = JsonSerializer.Deserialize<Veiculo>(resultadoCriar, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Tentar atualizar com token de Editor (não deve ter permissão)
        var veiculoDTOAtualizado = new VeiculoDTO
        {
            Nome = "Uno Way",
            Marca = "Fiat",
            Ano = 2016
        };

        var requestAtualizar = CriarRequestComToken(HttpMethod.Put, $"/veiculos/{veiculoCriado?.Id}", tokenEditor, veiculoDTOAtualizado);
        var responseAtualizar = await Setup.client.SendAsync(requestAtualizar);

        // Assert - Editor não pode atualizar
        Assert.AreEqual(HttpStatusCode.Forbidden, responseAtualizar.StatusCode);
    }

    [TestMethod]
    public async Task TestarDeletarVeiculo()
    {
        // Arrange - Primeiro criar um veículo
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Tempra",
            Marca = "Fiat",
            Ano = 1995
        };

        var requestCriar = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenAdm, veiculoDTO);
        var responseCriar = await Setup.client.SendAsync(requestCriar);
        var resultadoCriar = await responseCriar.Content.ReadAsStringAsync();
        var veiculoCriado = JsonSerializer.Deserialize<Veiculo>(resultadoCriar, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Deletar o veículo
        var requestDeletar = CriarRequestComToken(HttpMethod.Delete, $"/veiculos/{veiculoCriado?.Id}", tokenAdm);
        var responseDeletar = await Setup.client.SendAsync(requestDeletar);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, responseDeletar.StatusCode);

        // Verificar se o veículo foi deletado
        var requestBuscar = CriarRequestComToken(HttpMethod.Get, $"/veiculos/{veiculoCriado?.Id}", tokenAdm);
        var responseBuscar = await Setup.client.SendAsync(requestBuscar);
        Assert.AreEqual(HttpStatusCode.NotFound, responseBuscar.StatusCode);
    }

    [TestMethod]
    public async Task TestarValidacaoAoCriarVeiculo()
    {
        // Arrange - Veículo com nome vazio
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "",
            Marca = "Ford",
            Ano = 2013
        };

        var request = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenAdm, veiculoDTO);

        // Act
        var response = await Setup.client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TestarValidacaoAnoMinimo()
    {
        // Arrange - Veículo com ano anterior a 1950
        await InicializarTokensAsync();
        var veiculoDTO = new VeiculoDTO
        {
            Nome = "Model T",
            Marca = "Ford",
            Ano = 1900
        };

        var request = CriarRequestComToken(HttpMethod.Post, "/veiculos", tokenAdm, veiculoDTO);

        // Act
        var response = await Setup.client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task TestarBuscarVeiculoInexistente()
    {
        // Arrange
        await InicializarTokensAsync();
        var request = CriarRequestComToken(HttpMethod.Get, "/veiculos/99999", tokenAdm);

        // Act
        var response = await Setup.client.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}

