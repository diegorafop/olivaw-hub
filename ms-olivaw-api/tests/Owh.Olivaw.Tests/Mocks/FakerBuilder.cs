using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Owh.Olivaw.Tests.Mocks;

internal class FakerBuilder
{
    public IConfiguration Configuration { get; set; }


    public VersionFeatureFakerBuilder _VersionFeature { get; set; }

    public FakerBuilder()
    {
        Configuration = GetConfiguration();

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
                    { "authentication:oidc:custom-claims:isActive", "isActive" },
                    { "swagger:version", "Version 1.1.0" },
                    { "swagger:title", "release" },
                    { "services:holos:baseAddress", "https://dev.holos.mps.com.br" },
                }
            );
        return builder.Build();
    }



    public virtual VersionFeatureFakerBuilder VersionFeature
    {
        get
        {
            _VersionFeature ??= new VersionFeatureFakerBuilder();

            return _VersionFeature;
        }

        set { _VersionFeature = value; }
    }

}