using Microsoft.AspNetCore.Mvc;
using MongoDb.API.Data.ValueObjects;
using MongoDb.API.Domain.Enums;
using MongoDb.API.Domain.Models;
using MongoDb.API.Domain.ViewModels;

namespace MongoDb.API.Controllers
{
    [ApiController]
    public class RestauranteController : ControllerBase
    {
        [HttpPost("restaurante")]
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

            //_restauranteRepository.Inserir(restaurante);

            return Ok( new { data = "Restaurante inserido com sucesso" });
        }
    }
}
