using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Mps.Devops.Tests.Mocks;

internal class FakerBuilder
{
    public IConfiguration Configuration { get; set; }

    public DomainFakerBuilder _DomainFakerBuilder { get; set; }

    public VersaoFeatureFakerBuilder _VersaoFeature { get; set; }
    public ExampleFeatureFakerBuilder _ExampleFeature { get; set; }

    public FakerBuilder()
    {
        Configuration = GetConfiguration();

        DomainFakerBuilder = null;
    }

    private static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection
            (
                new Dictionary<string, string>
                {
                    { "authentication:secret", "6oAdbESqDEqN6mQfTxWxYw8hTtxZuP/UCHRhTD9ILA7QEXGIlKAf70207fYNCOyn0QkU8ff9Hz70SdVpiFMAiUoA" },
                    { "authentication:timeout", "15" },
                    { "authentication:oidc:issuer", "iss" },
                    { "authentication:oidc:standard-claims:sub", "sub" },
                    { "authentication:oidc:standard-claims:username", "preferred_username" },
                    { "authentication:oidc:standard-claims:name", "name" },
                    { "authentication:oidc:standard-claims:email", "email" },
                    { "authentication:oidc:custom-claims:matricula", "matricula" },
                    { "authentication:oidc:custom-claims:emissor", "emissor" },
                    { "authentication:oidc:custom-claims:ativo", "ativo" },
                    { "swagger:version", "Version 1.1.0" },
                    { "swagger:title", "release" },
                    { "services:holos:baseAddress", "https://dev.holos.mps.com.br" },
                }
            );
        return builder.Build();
    }


    public virtual DomainFakerBuilder DomainFakerBuilder
    {
        get
        {
            _DomainFakerBuilder ??= new DomainFakerBuilder();

            return _DomainFakerBuilder;
        }
        set { _DomainFakerBuilder = value; }
    }

    public virtual VersaoFeatureFakerBuilder VersaoFeature
    {
        get
        {
            _VersaoFeature ??= new VersaoFeatureFakerBuilder();

            return _VersaoFeature;
        }

        set { _VersaoFeature = value; }
    }

    public virtual ExampleFeatureFakerBuilder ExampleFeature
    {
        get
        {
            _ExampleFeature ??= new ExampleFeatureFakerBuilder();

            return _ExampleFeature;
        }

        set { _ExampleFeature = value; }
    }
}