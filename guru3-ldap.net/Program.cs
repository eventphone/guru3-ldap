using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace eventphone.guru3.ldap
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .Build();
            var listen = configuration["address"];
            var port = UInt16.Parse(configuration["port"]);
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            LdapDBServer server;
            if (String.IsNullOrEmpty(listen))
            {
                server = new LdapDBServer(port, connectionString);
            }
            else
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(listen), port);
                server = new LdapDBServer(endpoint, connectionString);
            }
            ushort? sslPort = null;
            if (UInt16.TryParse(configuration["sslPort"], out var value))
            {
                sslPort = value;
            };
            var cert = configuration["certificate"];
            if (!String.IsNullOrEmpty(cert))
            {
                //SSL
                var options = new SslServerAuthenticationOptions
                {
                    AllowRenegotiation = true,
                    ClientCertificateRequired = false,
                    EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                    ServerCertificate = new X509Certificate2(cert),
                };
                if (sslPort != null)
                {
                    server.UseSsl(sslPort.Value, options);
                }
                else
                {
                    server.UseSsl(options);
                }
            }
            server.AdminToken = configuration["token"];
            using (server)
            using (var cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    cts.Cancel();
                    e.Cancel = true;
                };
                await server.RunAsync(cts.Token);
            }
        }
    }
}
