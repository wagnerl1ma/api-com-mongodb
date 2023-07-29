using FluentValidation;
using FluentValidation.Results;
using MongoDb.API.Data.ValueObjects;
using MongoDb.API.Domain.Enums;

namespace MongoDb.API.Domain.Models
{
    public class Restaurante : AbstractValidator<Restaurante>
    {
        public string Id { get; private set; }
        public string Nome { get; private set; }
        public CozinhaEnum Cozinha { get; private set; }
        public Endereco Endereco { get; private set; }
        public List<Avaliacao> Avaliacoes { get; private set; }
        public ValidationResult ValidationResult { get; set; }

        #region Construtores
        public Restaurante(string nome, CozinhaEnum cozinha)
        {
            Nome = nome;
            Cozinha = cozinha;
            Avaliacoes = new List<Avaliacao>();
        }

        public Restaurante(string id, string nome, CozinhaEnum cozinha)
        {
            Id = id;
            Nome = nome;
            Cozinha = cozinha;
            Avaliacoes = new List<Avaliacao>();
        }
        #endregion

        public void AtribuirEndereco(Endereco endereco)
        {
            Endereco = endereco;
        }

        public void InserirAvaliacao(Avaliacao avaliacao)
        {
            Avaliacoes.Add(avaliacao);
        }

        /// <summary>
        /// Valida se todas as regras estão ok, sem sim retorna true, se nao false.
        /// </summary>
        public virtual bool Validar()
        {
            ValidarNome();
            ValidationResult = Validate(this); //this = essa classe, faz as validacoes das regras dessa classe

            ValidarEndereco(); //Validacao na Classe de Endereco

            return ValidationResult.IsValid;
        }

        private void ValidarNome()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("Nome não pode ser vazio.")
                .MaximumLength(30).WithMessage("Nome pode ter no maximo 30 caracteres.");
        }

        private void ValidarEndereco()
        {
            if (Endereco.Validar()) //método Validar() da Classe de Endereco
                return;

            foreach (var erro in Endereco.ValidationResult.Errors)
                ValidationResult.Errors.Add(erro);
        }
    }
}
