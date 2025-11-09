using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Servicos;

[TestClass]
public class VeiculoServicoTest
{
    private DbContexto CriarContextoDeTeste()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContexto(configuration);
    }

    [TestMethod]
    public void TestandoSalvarVeiculo()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("DELETE FROM Veiculos");

        var veiculo = new Veiculo
        {
            Nome = "Fiesta 2.0",
            Marca = "Ford",
            Ano = 2013
        };

        var veiculoServico = new VeiculoServico(context);

        // Act
        veiculoServico.Incluir(veiculo);

        // Assert
        var veiculos = veiculoServico.Todos(1);
        Assert.IsTrue(veiculos.Count > 0);
        Assert.AreEqual("Fiesta 2.0", veiculo.Nome);
    }

    [TestMethod]
    public void TestandoBuscaPorId()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("DELETE FROM Veiculos");

        var veiculo = new Veiculo
        {
            Nome = "Civic",
            Marca = "Honda",
            Ano = 2020
        };

        var veiculoServico = new VeiculoServico(context);

        // Act
        veiculoServico.Incluir(veiculo);
        var veiculoDoBanco = veiculoServico.BuscaPorId(veiculo.Id);

        // Assert
        Assert.IsNotNull(veiculoDoBanco);
        Assert.AreEqual(veiculo.Id, veiculoDoBanco.Id);
        Assert.AreEqual("Civic", veiculoDoBanco.Nome);
        Assert.AreEqual("Honda", veiculoDoBanco.Marca);
        Assert.AreEqual(2020, veiculoDoBanco.Ano);
    }

    [TestMethod]
    public void TestandoAtualizarVeiculo()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("DELETE FROM Veiculos");

        var veiculo = new Veiculo
        {
            Nome = "Corolla",
            Marca = "Toyota",
            Ano = 2018
        };

        var veiculoServico = new VeiculoServico(context);
        veiculoServico.Incluir(veiculo);

        // Act
        veiculo.Nome = "Corolla Cross";
        veiculo.Ano = 2022;
        veiculoServico.Atualizar(veiculo);

        // Assert
        var veiculoAtualizado = veiculoServico.BuscaPorId(veiculo.Id);
        Assert.IsNotNull(veiculoAtualizado);
        Assert.AreEqual("Corolla Cross", veiculoAtualizado.Nome);
        Assert.AreEqual(2022, veiculoAtualizado.Ano);
    }

    [TestMethod]
    public void TestandoApagarVeiculo()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("DELETE FROM Veiculos");

        var veiculo = new Veiculo
        {
            Nome = "Gol",
            Marca = "Volkswagen",
            Ano = 2015
        };

        var veiculoServico = new VeiculoServico(context);
        veiculoServico.Incluir(veiculo);
        var id = veiculo.Id;

        // Act
        veiculoServico.Apagar(veiculo);

        // Assert
        var veiculoDeletado = veiculoServico.BuscaPorId(id);
        Assert.IsNull(veiculoDeletado);
    }

    [TestMethod]
    public void TestandoListarVeiculos()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("DELETE FROM Veiculos");

        var veiculoServico = new VeiculoServico(context);

        var veiculo1 = new Veiculo { Nome = "Fiesta", Marca = "Ford", Ano = 2013 };
        var veiculo2 = new Veiculo { Nome = "Civic", Marca = "Honda", Ano = 2020 };
        var veiculo3 = new Veiculo { Nome = "Corolla", Marca = "Toyota", Ano = 2018 };

        veiculoServico.Incluir(veiculo1);
        veiculoServico.Incluir(veiculo2);
        veiculoServico.Incluir(veiculo3);

        // Act
        var veiculos = veiculoServico.Todos(1);

        // Assert
        Assert.IsTrue(veiculos.Count >= 3);
    }

    [TestMethod]
    public void TestandoFiltroPorNome()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("DELETE FROM Veiculos");

        var veiculoServico = new VeiculoServico(context);

        var veiculo1 = new Veiculo { Nome = "Fiesta", Marca = "Ford", Ano = 2013 };
        var veiculo2 = new Veiculo { Nome = "Civic", Marca = "Honda", Ano = 2020 };

        veiculoServico.Incluir(veiculo1);
        veiculoServico.Incluir(veiculo2);

        // Act
        var veiculos = veiculoServico.Todos(1, nome: "Fiesta");

        // Assert
        Assert.IsTrue(veiculos.Count > 0);
        Assert.IsTrue(veiculos.All(v => v.Nome.Contains("Fiesta", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void TestandoFiltroPorMarca()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("DELETE FROM Veiculos");

        var veiculoServico = new VeiculoServico(context);

        var veiculo1 = new Veiculo { Nome = "Fiesta", Marca = "Ford", Ano = 2013 };
        var veiculo2 = new Veiculo { Nome = "Civic", Marca = "Honda", Ano = 2020 };

        veiculoServico.Incluir(veiculo1);
        veiculoServico.Incluir(veiculo2);

        // Act
        var veiculos = veiculoServico.Todos(1, marca: "Ford");

        // Assert
        Assert.IsTrue(veiculos.Count > 0);
        Assert.IsTrue(veiculos.All(v => v.Marca.Contains("Ford", StringComparison.OrdinalIgnoreCase)));
    }
}

