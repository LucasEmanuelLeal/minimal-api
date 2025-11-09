using System.Net;
using System.Text;
using System.Text.Json;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.DTOs;

namespace Test.Helpers;

public static class TokenHelper
{
    public static async Task<string?> ObterTokenAsync(HttpClient client, string email = "adm@teste.com", string senha = "123456")
    {
        var loginDTO = new LoginDTO
        {
            Email = email,
            Senha = senha
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginDTO),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("/administradores/login", content);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return admLogado?.Token;
        }

        return null;
    }
}

