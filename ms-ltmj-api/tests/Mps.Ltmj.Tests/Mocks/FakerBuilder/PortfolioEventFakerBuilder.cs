using System;
using Bogus;
using Mps.Ltmj.Domain;

namespace Mps.Ltmj.Tests.Mocks;

internal static class PortfolioEventFakerBuilder
{
    internal static PortfolioEvent Build() => new Faker<PortfolioEvent>()
        .CustomInstantiator(faker => PortfolioEvent.Create(faker.Lorem.Sentence(3), faker.Lorem.Paragraph(), "Confraternizações", Guid.NewGuid()))
        .Generate();
}
