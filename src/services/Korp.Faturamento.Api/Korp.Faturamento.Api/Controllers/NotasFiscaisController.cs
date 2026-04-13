using Korp.Faturamento.Application.UseCases.NotasFiscais;
using Korp.Faturamento.Application.DTOs.Request;
using Korp.Faturamento.Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Faturamento.Api.Controllers
{
    [ApiController]
    [Route("api/v1/notas")]
    public class NotasFiscaisController : ControllerBase
    {
        private readonly CreateNotaUseCase _createNota;
        private readonly ReadNotaUseCase _readNota;
        private readonly ImprimirNotaUseCase _imprimirNota;

        public NotasFiscaisController(
            CreateNotaUseCase createNota,
            ReadNotaUseCase readNota,
            ImprimirNotaUseCase imprimirNota)
        {
            _createNota = createNota;
            _readNota = readNota;
            _imprimirNota = imprimirNota;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<NotaFiscalResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _readNota.ExecuteGetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var result = await _readNota.ExecuteGetById(id);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateNotaRequest request)
        {
            var result = await _createNota.Execute(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("{id}/imprimir")]
        [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Imprimir(
            [FromRoute] Guid id,
            [FromBody] ImprimirNotaRequest request)
        {
            var result = await _imprimirNota.Execute(id, request.ImpressaoId);
            return Ok(result);
        }
    }
}
