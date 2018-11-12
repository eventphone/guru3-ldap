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
            var listen = configuration.GetValue<string>("address", null);
            var port = configuration.GetValue<ushort>("port");
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
            var sslPort = configuration.GetValue<ushort?>("sslPort");
            var cert = configuration.GetValue<string>("certificate");
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
