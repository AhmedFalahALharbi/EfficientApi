using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace YourApp.Services
{
    public class ProductSorterHostedService : IHostedService
    {
        private readonly ILogger<ProductSorterHostedService> _logger;
        private readonly IWebHostEnvironment _env;
        private const int BatchSize = 1000;

        public ProductSorterHostedService(
            ILogger<ProductSorterHostedService> logger,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Build paths
            var dataDir = Path.Combine(_env.ContentRootPath, "Data");
            var sourcePath = Path.Combine(dataDir, "products.txt");
            var idPath    = Path.Combine(dataDir, "products_sorted_by_id.txt");
            var namePath  = Path.Combine(dataDir, "products_sorted_by_name.txt");
            var pricePath = Path.Combine(dataDir, "products_sorted_by_price.txt");

            // Validate source
            if (!File.Exists(sourcePath))
            {
                _logger.LogWarning("Source products.txt not found at {Path}", sourcePath);
                return Task.CompletedTask;
            }

            Directory.CreateDirectory(dataDir);

            // Prepare writers
            using var idWriter    = new StreamWriter(idPath);
            using var nameWriter  = new StreamWriter(namePath);
            using var priceWriter = new StreamWriter(pricePath);

            var batch = new List<Product>();
            foreach (var line in File.ReadLines(sourcePath))
            {
                var parts = line.Split(',');
                if (parts.Length != 3) continue;  // skip bad lines

                if (int.TryParse(parts[0], out var id) &&
                    decimal.TryParse(parts[2], out var price))
                {
                    batch.Add(new Product { Id = id, Name = parts[1], Price = price });
                }

                if (batch.Count >= BatchSize)
                {
                    WriteBatch(batch, idWriter, nameWriter, priceWriter);
                    batch.Clear();
                }
            }

            // Final batch
            if (batch.Count > 0)
                WriteBatch(batch, idWriter, nameWriter, priceWriter);

            _logger.LogInformation("Finished sorting products into files.");
            return Task.CompletedTask;
        }

        private void WriteBatch(
            List<Product> batch,
            StreamWriter idWriter,
            StreamWriter nameWriter,
            StreamWriter priceWriter)
        {
            foreach (var p in batch.OrderBy(x => x.Id))
                idWriter.WriteLine($"{p.Id},{p.Name},{p.Price}");
            foreach (var p in batch.OrderBy(x => x.Name))
                nameWriter.WriteLine($"{p.Id},{p.Name},{p.Price}");
            foreach (var p in batch.OrderBy(x => x.Price))
                priceWriter.WriteLine($"{p.Id},{p.Name},{p.Price}");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
