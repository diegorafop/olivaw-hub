using FluentValidation;
using Mps.Devops.Services;

namespace Mps.Devops.Api.Features.ImplantacaoFeature.Internals;

public sealed class ProcessExecutionResultValidator : AbstractValidator<ProcessExecutionResult>
{
    public ProcessExecutionResultValidator()
    {
        RuleFor(item => item.Success).Equal(true)
            .WithMessage("Não foi possível concluir a implantação. Confira o repositório, o branch e o serviço Git/Docker no servidor.")
            .OverridePropertyName("Implantacao");
    }
}

