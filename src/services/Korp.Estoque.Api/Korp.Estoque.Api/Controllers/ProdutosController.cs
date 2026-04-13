using Korp.Estoque.Api.Filters;
using Korp.Estoque.Application.DTOs;
using Korp.Estoque.Application.UseCases.Produtos;
using Microsoft.AspNetCore.Mvc;

namespace Korp.Estoque.Api.Controllers
{
    [ApiController]
    [Route("api/v1/produtos")]
    public class ProdutosController : ControllerBase
    {
        private readonly CreateProdutoUseCase _createProdutoUseCase;
        private readonly ReadProdutoUseCase _readProdutoUseCase;
        private readonly UpdateProdutoUseCase _updateProdutoUseCase;
        private readonly DeleteProdutoUseCase _deleteProdutoUseCase;
        private readonly BaixaEstoqueUseCase _baixaEstoqueUseCase;
        private readonly ValidarProdutosUseCase _validarProdutosUseCase;

        public ProdutosController(
            CreateProdutoUseCase createProdutoUseCase,
            ReadProdutoUseCase readProdutoUseCase,
            UpdateProdutoUseCase updateProdutoUseCase,
            DeleteProdutoUseCase deleteProdutoUseCase,
            BaixaEstoqueUseCase baixaEstoqueUseCase,
            ValidarProdutosUseCase validarProdutosUseCase)
        {
            _createProdutoUseCase = createProdutoUseCase;
            _readProdutoUseCase = readProdutoUseCase;
            _updateProdutoUseCase = updateProdutoUseCase;
            _deleteProdutoUseCase = deleteProdutoUseCase;
            _baixaEstoqueUseCase = baixaEstoqueUseCase;
            _validarProdutosUseCase = validarProdutosUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProdutoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _readProdutoUseCase.ExecuteGetAll();
            return Ok(result);
        }

        [HttpGet("{codigo}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCodigo([FromRoute] string codigo)
        {
            var result = await _readProdutoUseCase.ExecuteGetByCodigo(codigo);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProdutoRequest request)
        {
            var result = await _createProdutoUseCase.Execute(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateDescricaoRequest request)
        {
            var result = await _updateProdutoUseCase.Execute(id, request);
            return Ok(result);
        }

        [HttpPost("baixa")]
        [InternalServiceOnly]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BaixarEstoque([FromBody] List<BaixaEstoqueRequest> requests)
        {
            await _baixaEstoqueUseCase.Execute(requests);
            return NoContent();
        }

        [HttpPost("validar")]
        [InternalServiceOnly]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Validar([FromBody] ValidarProdutosRequest request)
        {
            await _validarProdutosUseCase.Execute(request);
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _deleteProdutoUseCase.Execute(id);
            return NoContent();
        }
    }
}