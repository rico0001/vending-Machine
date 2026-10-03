using System.Text.Json;
using System.Text.Json.Serialization;

namespace CS2S564VendingMachine.Core;

public sealed class TransactionHistory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _filePath;
    private readonly List<Transaction> _transactions = new();

    public TransactionHistory(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    public string FilePath => _filePath;
    public IReadOnlyList<Transaction> Transactions => _transactions;

    public void RecordCompletedSale(Transaction transaction)
    {
        if (transaction is null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (!string.Equals(transaction.PaymentStatus, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only completed sales may be written to history.");
        }

        _transactions.Add(transaction);
        Save();
    }

    public IReadOnlyList<Transaction> Reload()
    {
        Load();
        return _transactions;
    }

    private void Load()
    {
        _transactions.Clear();
        if (!File.Exists(_filePath))
        {
            return;
        }

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        var loaded = JsonSerializer.Deserialize<List<TransactionRecord>>(json, JsonOptions);
        if (loaded is null)
        {
            return;
        }

        foreach (var record in loaded)
        {
            _transactions.Add(record.ToTransaction());
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var records = _transactions.Select(TransactionRecord.FromTransaction).ToList();
        File.WriteAllText(_filePath, JsonSerializer.Serialize(records, JsonOptions));
    }

    private sealed class TransactionRecord
    {
        public string TransactionId { get; set; } = "";
        public DateTimeOffset CompletedAt { get; set; }
        public int OrderNumber { get; set; }
        public List<LineRecord> Products { get; set; } = new();
        public int OrderTotalPence { get; set; }
        public int AmountInsertedPence { get; set; }
        public int ChangeReturnedPence { get; set; }
        public string PaymentStatus { get; set; } = "";

        public static TransactionRecord FromTransaction(Transaction transaction) => new()
        {
            TransactionId = transaction.TransactionId,
            CompletedAt = transaction.CompletedAt,
            OrderNumber = transaction.OrderNumber,
            Products = transaction.Products.Select(line => new LineRecord
            {
                ProductId = line.ProductId,
                ProductName = line.ProductName,
                Quantity = line.Quantity,
                UnitPricePence = line.UnitPricePence,
                SubtotalPence = line.SubtotalPence
            }).ToList(),
            OrderTotalPence = transaction.OrderTotalPence,
            AmountInsertedPence = transaction.AmountInsertedPence,
            ChangeReturnedPence = transaction.ChangeReturnedPence,
            PaymentStatus = transaction.PaymentStatus
        };

        public Transaction ToTransaction()
        {
            var lines = Products.Select(line =>
                new TransactionLine(line.ProductId, line.ProductName, line.Quantity, line.UnitPricePence, line.SubtotalPence)
            ).ToList();

            return new Transaction(
                TransactionId,
                CompletedAt,
                OrderNumber,
                lines,
                OrderTotalPence,
                AmountInsertedPence,
                ChangeReturnedPence,
                PaymentStatus);
        }
    }

    private sealed class LineRecord
    {
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public int UnitPricePence { get; set; }
        public int SubtotalPence { get; set; }
    }
}
