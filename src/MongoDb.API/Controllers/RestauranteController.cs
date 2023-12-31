﻿using Microsoft.AspNetCore.Mvc;
using MongoDb.API.Data.Repositories;
using MongoDb.API.Data.ValueObjects;
using MongoDb.API.Domain.Enums;
using MongoDb.API.Domain.Models;
using MongoDb.API.Domain.ViewModels;
using MongoDb.API.Results;
using MongoDb.API.ViewModels;

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
                return BadRequest(new { errors = restaurante.ValidationResult.Errors.Select(x => x.ErrorMessage) });
            }

            _restauranteRepository.Inserir(restaurante);

            return Ok(new { data = "Restaurante inserido com sucesso" });
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

            return Ok(new { data = listagem });
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

            return Ok(new { data = exibicao });
        }

        [HttpGet("obter-por-nome")]
        public ActionResult ObterRestaurantePorNome([FromQuery] string nome)
        {
            var restaurantes = _restauranteRepository.ObterPorNome(nome);

            var listagem = restaurantes.Select(x => new RestauranteListagemResult
            {
                Id = x.Id,
                Nome = x.Nome,
                Cozinha = (int)x.Cozinha,
                Cidade = x.Endereco.Cidade
            });

            return Ok(new { data = listagem });
        }

        [HttpPut]
        public ActionResult AlterarRestaurante([FromBody] RestauranteAlteracaoCompletaViewModel restauranteAlteracaoCompleta)
        {
            var restaurante = _restauranteRepository.ObterPorId(restauranteAlteracaoCompleta.Id);

            if (restaurante == null)
                return NotFound();

            var cozinha = ECozinhaHelper.ConverterDeInteiro(restauranteAlteracaoCompleta.Cozinha);
            restaurante = new Restaurante(restauranteAlteracaoCompleta.Id, restauranteAlteracaoCompleta.Nome, cozinha);
            var endereco = new Endereco(
                restauranteAlteracaoCompleta.Logradouro,
                restauranteAlteracaoCompleta.Numero,
                restauranteAlteracaoCompleta.Cidade,
                restauranteAlteracaoCompleta.UF,
                restauranteAlteracaoCompleta.Cep);

            restaurante.AtribuirEndereco(endereco);

            if (!restaurante.Validar())
            {
                return BadRequest(new { errors = restaurante.ValidationResult.Errors.Select(_ => _.ErrorMessage) });
            }

            if (!_restauranteRepository.AlterarCompleto(restaurante))
            {
                return BadRequest(new { errors = "Nenhum documento foi alterado" });
            }

            return Ok(new { data = "Restaurante alterado com sucesso" });
        }

        [HttpPatch("{id}")] //Alteracao Parcial
        public ActionResult AlterarCozinha(string id, [FromBody] RestauranteAlteracaoParcialViewModel restauranteAlteracaoParcial)
        {
            var restaurante = _restauranteRepository.ObterPorId(id);

            if (restaurante == null)
                return NotFound();

            var cozinha = ECozinhaHelper.ConverterDeInteiro(restauranteAlteracaoParcial.Cozinha);

            if (!_restauranteRepository.AlterarCozinha(id, cozinha))
            {
                return BadRequest(new { errors = "Nenhum documento foi alterado" });
            }

            return Ok(new { data = "Restaurante alterado com sucesso" });
        }

        [HttpPatch("{id}/avaliar")] //Alteracao Parcial
        public ActionResult AvaliarRestaurante(string id, [FromBody] AvaliacaoInclusaoViewModel avaliacaoInclusao)
        {
            var restaurante = _restauranteRepository.ObterPorId(id);

            if (restaurante == null)
                return NotFound();

            var avaliacao = new Avaliacao(avaliacaoInclusao.Estrelas, avaliacaoInclusao.Comentario);

            if (!avaliacao.Validar())
            {
                return BadRequest(new { errors = avaliacao.ValidationResult.Errors.Select(_ => _.ErrorMessage)} );
            }

            _restauranteRepository.Avaliar(id, avaliacao);

            return Ok(new { data = "Restaurante avaliado com sucesso" });
        }

        [HttpGet("top3")]
        public async Task<ActionResult> ObterTop3Restaurantes()
        {
            var top3 = await _restauranteRepository.ObterTop3();

            var listagem = top3.Select(x => new RestauranteTop3Result
            {
                Id = x.Key.Id,
                Nome = x.Key.Nome,
                Cozinha = (int)x.Key.Cozinha,
                Cidade = x.Key.Endereco.Cidade,
                Estrelas = x.Value
            });

            return Ok(new { data = listagem });
        }

        [HttpGet("top3-lookup")]
        public ActionResult ObterTop3RestaurantesComLookup()
        {
            var top3 = _restauranteRepository.ObterTop3ComLookup();

            var listagem = top3.Select(x => new RestauranteTop3Result
            {
                Id = x.Key.Id,
                Nome = x.Key.Nome,
                Cozinha = (int)x.Key.Cozinha,
                Cidade = x.Key.Endereco.Cidade,
                Estrelas = x.Value
            });

            return Ok(new { data = listagem });
        }

        [HttpDelete("{id}")]
        public ActionResult Remover(string id)
        {
            var restaurante = _restauranteRepository.ObterPorId(id);

            if (restaurante == null)
                return NotFound();

            (var totalRestauranteRemovido, var totalAvaliacoesRemovidas) = _restauranteRepository.Remover(id);

            return Ok(new { data = $"Total de exclusões: {totalRestauranteRemovido} restaurante com {totalAvaliacoesRemovidas} avaliações" });
        }

        [HttpGet("textual")]
        public async Task<ActionResult> ObterRestaurantePorBuscaTextual([FromQuery] string texto)
        {
            var restaurantes = await _restauranteRepository.ObterPorBuscaTextual(texto);

            var listagem = restaurantes.ToList().Select(x => new RestauranteListagemResult
            {
                Id = x.Id,
                Nome = x.Nome,
                Cozinha = (int)x.Cozinha,
                Cidade = x.Endereco.Cidade
            });

            return Ok(new { data = listagem });
        }
    }
}
