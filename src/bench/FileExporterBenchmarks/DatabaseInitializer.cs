using Npgsql;
using Testcontainers.PostgreSql;

namespace FileExporterBenchmarks;


public static class DatabaseInitializer
{
    private static readonly PostgreSqlContainer PostgresContainer = new PostgreSqlBuilder()
                                                                                 .WithImage("postgres:15-alpine")
                                                                                 .WithDatabase("benchmarks")
                                                                                 .WithUsername("postgres")
                                                                                 .WithPassword("postgres")
                                                                                 .Build();
    
    
    private const int TotalRows = 10_000;

    public static NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(PostgresContainer.GetConnectionString());
    }

    public static async Task InitializeDatabaseAsync()
    {
        await PostgresContainer.StartAsync();
        Console.WriteLine("🚀 Starting PostgreSQL container...");
        
        await using var connection = new NpgsqlConnection(PostgresContainer.GetConnectionString());
        await connection.OpenAsync();
        
        // Create benchmark table
        await CreateTableAsync(connection);
        
        // Insert data
        await InsertDataAsync(connection);
        
        Console.WriteLine($"✅ Database initialized with {TotalRows:N0} rows");
    }
    
    private static async Task CreateTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            DROP TABLE IF EXISTS benchmark_sales;
            
            CREATE TABLE benchmark_sales (
                id BIGSERIAL PRIMARY KEY,
                transaction_id UUID NOT NULL DEFAULT gen_random_uuid(),
                customer_id INTEGER NOT NULL,
                customer_name VARCHAR(100) NOT NULL,
                customer_email VARCHAR(255) NOT NULL,
                product_id INTEGER NOT NULL,
                product_name VARCHAR(200) NOT NULL,
                category VARCHAR(50) NOT NULL,
                quantity INTEGER NOT NULL CHECK (quantity BETWEEN 1 AND 1000),
                unit_price DECIMAL(10,2) NOT NULL CHECK (unit_price >= 0),
                discount DECIMAL(5,2) NOT NULL DEFAULT 0.00 CHECK (discount BETWEEN 0 AND 100),
                tax_rate DECIMAL(5,2) NOT NULL DEFAULT 20.00,
                total_amount DECIMAL(12,2) GENERATED ALWAYS AS (
                    quantity * unit_price * (1 - discount/100) * (1 + tax_rate/100)
                ) STORED,
                transaction_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                region VARCHAR(50) NOT NULL,
                country VARCHAR(50) NOT NULL,
                payment_method VARCHAR(30) NOT NULL,
                status VARCHAR(20) NOT NULL CHECK (status IN (
                    'PENDING', 'PROCESSING', 'COMPLETED', 'CANCELLED', 'REFUNDED', 'FAILED'
                )),
                notes TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            
            -- Create indexes for realistic queries
            CREATE INDEX idx_sales_date ON benchmark_sales(transaction_date DESC);
            CREATE INDEX idx_sales_region ON benchmark_sales(region);
            CREATE INDEX idx_sales_status ON benchmark_sales(status);
            CREATE INDEX idx_sales_customer ON benchmark_sales(customer_id);
            CREATE INDEX idx_sales_product ON benchmark_sales(product_id);
            
            -- Add some constraints
            ALTER TABLE benchmark_sales ADD CONSTRAINT chk_quantity_positive CHECK (quantity > 0);
            ALTER TABLE benchmark_sales ADD CONSTRAINT chk_price_positive CHECK (unit_price >= 0);
        ";
        
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
    
    private static async Task InsertDataAsync(NpgsqlConnection connection)
    {
        Console.WriteLine($"📊 Generating {TotalRows:N0} rows of benchmark data...");
        
        const string insertSql = @"
            INSERT INTO benchmark_sales 
            (
                customer_id, customer_name, customer_email,
                product_id, product_name, category,
                quantity, unit_price, discount, tax_rate,
                transaction_date, region, country, payment_method, status, notes
            )
            VALUES (
                @customerId, @customerName, @customerEmail,
                @productId, @productName, @category,
                @quantity, @unitPrice, @discount, @taxRate,
                @transactionDate, @region, @country, @paymentMethod, @status, @notes
            );
        ";
        
        var regions = new[] { "North America", "Europe", "Asia", "South America", "Africa", "Oceania" };
        var countries = new[] { "US", "UK", "DE", "FR", "JP", "CA", "AU", "BR", "IN", "CN" };
        var categories = new[] { "Electronics", "Clothing", "Books", "Home & Garden", "Sports", "Toys", "Food", "Health" };
        var paymentMethods = new[] { "Credit Card", "PayPal", "Bank Transfer", "Cash", "Apple Pay", "Google Pay" };
        var statuses = new[] { "PENDING", "PROCESSING", "COMPLETED", "CANCELLED", "REFUNDED" };
        
        var batchSize = 10_000;
        var totalBatches = (TotalRows + batchSize - 1) / batchSize;
        
        for (int batch = 0; batch < totalBatches; batch++)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            await using var command = new NpgsqlCommand(insertSql, connection, transaction);
            
            // Add parameters
            command.Parameters.AddRange(new[]
            {
                new NpgsqlParameter("customerId", NpgsqlTypes.NpgsqlDbType.Integer),
                new NpgsqlParameter("customerName", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("customerEmail", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("productId", NpgsqlTypes.NpgsqlDbType.Integer),
                new NpgsqlParameter("productName", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("category", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("quantity", NpgsqlTypes.NpgsqlDbType.Integer),
                new NpgsqlParameter("unitPrice", NpgsqlTypes.NpgsqlDbType.Numeric),
                new NpgsqlParameter("discount", NpgsqlTypes.NpgsqlDbType.Numeric),
                new NpgsqlParameter("taxRate", NpgsqlTypes.NpgsqlDbType.Numeric),
                new NpgsqlParameter("transactionDate", NpgsqlTypes.NpgsqlDbType.TimestampTz),
                new NpgsqlParameter("region", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("country", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("paymentMethod", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("status", NpgsqlTypes.NpgsqlDbType.Varchar),
                new NpgsqlParameter("notes", NpgsqlTypes.NpgsqlDbType.Text)
            });
            
            var rowsInBatch = Math.Min(batchSize, TotalRows - batch * batchSize);
            
            for (int i = 0; i < rowsInBatch; i++)
            {
                var rowNum = batch * batchSize + i;
                var baseDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(365));
                
                command.Parameters[0].Value = rowNum % 5000 + 1; // customerId
                command.Parameters[1].Value = $"Customer {(rowNum % 5000) + 1}";
                command.Parameters[2].Value = $"customer{(rowNum % 5000) + 1}@example.com";
                command.Parameters[3].Value = rowNum % 1000 + 1; // productId
                command.Parameters[4].Value = $"Product {rowNum % 1000 + 1}";
                command.Parameters[5].Value = categories[rowNum % categories.Length];
                command.Parameters[6].Value = Random.Shared.Next(1, 101); // quantity
                command.Parameters[7].Value = Math.Round(Random.Shared.NextDouble() * 1000, 2); // unitPrice
                command.Parameters[8].Value = Random.Shared.Next(0, 31); // discount
                command.Parameters[9].Value = Random.Shared.Next(5, 26); // taxRate
                command.Parameters[10].Value = baseDate.AddHours(Random.Shared.Next(24));
                command.Parameters[11].Value = regions[rowNum % regions.Length];
                command.Parameters[12].Value = countries[rowNum % countries.Length];
                command.Parameters[13].Value = paymentMethods[rowNum % paymentMethods.Length];
                command.Parameters[14].Value = statuses[rowNum % statuses.Length];
                command.Parameters[15].Value = rowNum % 20 == 0 ? $"Special order #{rowNum}" : DBNull.Value;
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
            
            if ((batch + 1) % 10 == 0 || batch + 1 == totalBatches)
            {
                Console.WriteLine($"  Inserted {(batch + 1) * batchSize:N0} rows...");
            }
        }
        
        // Verify
        await using var countCommand = new NpgsqlCommand("SELECT COUNT(*) FROM benchmark_sales", connection);
        var count = await countCommand.ExecuteScalarAsync();
        Console.WriteLine($"📈 Total rows in table: {count:N0}");
    }
    
}