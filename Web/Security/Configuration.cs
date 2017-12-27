using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Web.Security
{
    public class Configuration
    {
        public static string SecretKey = "Secret".Sha256();

        public static IEnumerable<IdentityResource> IdentityResources
        {
            get
            {
                return new List<IdentityResource>
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile()
                };
            }
        }

        public static IEnumerable<ApiResource> ApiResources()
        {
            return new[]
            {
                new ApiResource("AuthApi", "api")
            };
        }

        public static List<TestUser> Users
        {
            get
            {
                return new List<TestUser>
                   {
                        new TestUser
                        {
                            SubjectId = "1",
                            Username = "mail@qq.com",
                            Password = "password",
                        },
                         new TestUser
                        {
                            SubjectId = "2",
                            Username = "admin",
                            Password = "admin",
                        },
                          new TestUser
                        {
                            SubjectId = "3",
                            Username = "test",
                            Password = "123456",
                        }
                    };

            }

        }

        public static IEnumerable<Client> Clients
        {
            get {
                return new Client[] {
                     new Client
                    {
                        ClientId = "AuthApi",
                        ClientSecrets = new [] { new Secret(SecretKey) },
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                        AllowedScopes = new [] {
                            "AuthApi",
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                        }
                    },
                     new Client
                    {
                        ClientId = "WebPlatform",
                        ClientName = "MVC Code Client",
                        AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                        ClientSecrets = new [] { new Secret(SecretKey) },
                        AllowedScopes = new [] {
                            "WebPlatform",
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                        }
                    },
                };
            }
        }
    }
}
