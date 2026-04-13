using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Korp.Faturamento.Application.Services;
using Korp.Faturamento.Domain.Exceptions;
using Korp.Faturamento.Application.DTOs.Request;
using Microsoft.Extensions.Configuration;

namespace Korp.Faturamento.Infrastructure.Clients;

public class EstoqueServiceClient : IEstoqueService
{
    private readonly HttpClient _httpClient;
    private readonly string _internalSecret;

    public EstoqueServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _internalSecret = configuration["InternalSettings:CommunicationSecret"]!;
    }

    public async Task ValidarProdutosAsync(List<string> codigos)
    {
        try
        {
            var response = await SendInternalAsync(
                HttpMethod.Post,
                "/api/v1/produtos/validar",
                new ValidarProdutosRequest(codigos));

            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                var erro = await response.Content.ReadAsStringAsync();
                throw new BusinessException(erro);
            }

            response.EnsureSuccessStatusCode();
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceUnavailableException(
                "Serviço de estoque indisponível. Tente novamente.", ex);
        }
        catch (TaskCanceledException)
        {
            throw new ServiceUnavailableException(
                "Serviço de estoque não respondeu a tempo.");
        }
    }

    public async Task BaixarEstoqueAsync(List<BaixaEstoqueRequest> requests)
    {
        try
        {
            var response = await SendInternalAsync(
                HttpMethod.Post,
                "/api/v1/produtos/baixa",
                requests);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                throw new BusinessException(erro);
            }
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new ServiceUnavailableException(
                "Serviço de estoque indisponível. Tente novamente.", ex);
        }
        catch (TaskCanceledException)
        {
            throw new ServiceUnavailableException(
                "Serviço de estoque não respondeu a tempo.");
        }
    }

    private async Task<HttpResponseMessage> SendInternalAsync<T>(
        HttpMethod method,
        string url,
        T body)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Add("X-Internal-Secret", _internalSecret);

        return await _httpClient.SendAsync(request);
    }
}