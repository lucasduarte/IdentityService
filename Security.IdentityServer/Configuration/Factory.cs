﻿using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Security.IdentityServer.Configuration
{
    class Factory
    {
        public static IdentityServerServiceFactory Configure(string connString)
        {
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = connString,
                //Schema = "foo"
            };

            var cleanup = new TokenCleanup(efConfig, 10);
            cleanup.Start();

            // these two calls just pre-populate the test DB from the in-memory config
            ConfigureClients(Clients.Get(), efConfig);
            ConfigureScopes(Scopes.Get(), efConfig);

            var factory = new IdentityServerServiceFactory();

            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);

            factory.CorsPolicyService = new Registration<ICorsPolicyService>(new DefaultCorsPolicyService { AllowAll = true });

            factory.UseInMemoryUsers(Users.Get());

            return factory;
        }

        public static void ConfigureClients(IEnumerable<Client> clients, EntityFrameworkServiceOptions options)
        {
            using (var db = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Clients.Any())
                {
                    foreach (var c in clients)
                    {
                        var e = c.ToEntity();
                        db.Clients.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void ConfigureScopes(IEnumerable<Scope> scopes, EntityFrameworkServiceOptions options)
        {
            using (var db = new ScopeConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Scopes.Any())
                {
                    foreach (var s in scopes)
                    {
                        var e = s.ToEntity();
                        db.Scopes.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}