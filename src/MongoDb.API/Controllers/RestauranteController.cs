using Microsoft.AspNetCore.Mvc;
using MongoDb.API.Data.Repositories;
using MongoDb.API.Data.ValueObjects;
using MongoDb.API.Domain.Enums;
using MongoDb.API.Domain.Models;
using MongoDb.API.Domain.ViewModels;
using MongoDb.API.Results;

namespace MongoDb.API.Controllers
{
    [ApiController]
    [Route("api/restaurantes")]
    public class RestauranteController : ControllerBase
    {
        private readonly RestauranteRepository _restauranteRepository;

        public RestauranteController(RestauranteRepository restauranteRepository)
        {
            _restauranteRepository = restauranteRepository;
        }

        [HttpPost]
        public ActionResult IncluirRestaurante([FromBody] RestauranteInclusaoViewModel restauranteInclusao)
        {
            var cozinha = ECozinhaHelper.ConverterDeInteiro(restauranteInclusao.Cozinha);

            var restaurante = new Restaurante(restauranteInclusao.Nome, cozinha);

            var endereco = new Endereco(
                restauranteInclusao.Logradouro,
                restauranteInclusao.Numero,
                restauranteInclusao.Cidade,
                restauranteInclusao.UF,
                restauranteInclusao.Cep);

            restaurante.AtribuirEndereco(endereco);

            if (!restaurante.Validar())
            {
                return BadRequest( new { errors = restaurante.ValidationResult.Errors.Select(x => x.ErrorMessage) });
            }

            _restauranteRepository.Inserir(restaurante);

            return Ok( new { data = "Restaurante inserido com sucesso" });
        }

        [HttpGet]
        public async Task<ActionResult> ObterTodosRestaurantes()
        {
            var restaurantes = await _restauranteRepository.ObterTodos();

            var listagem = restaurantes.Select(x => new RestauranteListagemResult
            {
                Id = x.Id,
                Nome = x.Nome,
                Cozinha = (int)x.Cozinha,
                Cidade = x.Endereco.Cidade
            });

            return Ok( new { data = listagem });
        }

        [HttpGet("{id}")]
        public ActionResult ObterRestaurantePorId(string id)
        {
            var restaurante = _restauranteRepository.ObterPorId(id);

            if (restaurante == null)
                return NotFound();

            var exibicao = new RestauranteExibicaoResult
            {
                Id = restaurante.Id,
                Nome = restaurante.Nome,
                Cozinha = (int)restaurante.Cozinha,
                Endereco = new EnderecoExibicaoResult
                {
                    Logradouro = restaurante.Endereco.Logradouro,
                    Numero = restaurante.Endereco.Numero,
                    Cidade = restaurante.Endereco.Cidade,
                    Cep = restaurante.Endereco.Cep,
                    UF = restaurante.Endereco.UF
                }
            };

            return Ok( new { data = exibicao });
        }
    }
}
