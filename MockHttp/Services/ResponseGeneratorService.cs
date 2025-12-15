using System.Xml.Serialization;

namespace MockHttp.Services;

public class XmlSample
{
    public string Title { get; set; } = "Sample";
    public int Id { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<XmlItem> Items { get; set; } = new();
}

public class XmlItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ResponseGeneratorService : IResponseGeneratorService
{
    private static readonly string[] _firstNames = { "John", "Jane", "Alice", "Bob", "Emma", "Michael" };
    private static readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Davis", "Miller" };
    private static readonly string[] _products = { "Laptop", "Mouse", "Keyboard", "Monitor", "Headphones", "Webcam" };
    private static readonly string[] _categories = { "Electronics", "Office", "Gaming", "Accessories" };
    private static readonly string[] _orderStatuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

    public object GetSimpleJson() => new
    {
        success = true,
        message = "Request processed successfully",
        timestamp = DateTime.UtcNow,
        value = 123
    };

    public object GetComplexJson() => new
    {
        meta = new
        {
            version = "1.0.0",
            apiName = "MockHttp API",
            timestamp = DateTime.UtcNow,
            environment = "development"
        },
        data = new
        {
            users = Enumerable.Range(1, 3).Select(i => new
            {
                id = i,
                username = $"user{i}",
                email = $"user{i}@example.com",
                profile = new
                {
                    firstName = _firstNames[i % _firstNames.Length],
                    lastName = _lastNames[i % _lastNames.Length],
                    age = 20 + i * 5,
                    active = i % 2 == 0
                }
            }).ToArray(),
            pagination = new
            {
                currentPage = 1,
                totalPages = 5,
                pageSize = 10,
                totalRecords = 50
            },
            nested = new
            {
                level1 = new
                {
                    level2 = new
                    {
                        level3 = new
                        {
                            deepValue = "Successfully reached level 3",
                            coordinates = new { lat = 37.7749, lng = -122.4194 }
                        }
                    }
                }
            }
        },
        links = new
        {
            self = "/formats/json/complex",
            simple = "/formats/json/simple"
        }
    };

    public string GetHtmlString() => @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>MockHttp API Response</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 40px; background: #f5f5f5; }
        .container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #333; border-bottom: 3px solid #007acc; padding-bottom: 10px; }
        .info { background: #e7f3ff; padding: 15px; border-left: 4px solid #007acc; margin: 20px 0; }
        code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; font-family: 'Courier New', monospace; }
    </style>
</head>
<body>
    <div class=""container"">
        <h1>MockHttp API - HTML Response</h1>
        <p>This is a realistic HTML response from the MockHttp testing suite.</p>
        <div class=""info"">
            <strong>Timestamp:</strong> " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @" UTC<br>
            <strong>Endpoint:</strong> <code>GET /formats/html</code><br>
            <strong>Content-Type:</strong> <code>text/html</code>
        </div>
        <h2>Use Cases</h2>
        <ul>
            <li>Testing HTML content parsing</li>
            <li>Validating web scraping tools</li>
            <li>Simulating server-side rendered responses</li>
            <li>Testing content negotiation</li>
        </ul>
    </div>
</body>
</html>";

    public XmlSample GetXmlObject() => new XmlSample
    {
        Title = "Product Catalog Export",
        Id = 42,
        CreatedAt = DateTime.UtcNow,
        Items = new List<XmlItem>
        {
            new XmlItem { Name = "Laptop Pro", Price = 1299.99m },
            new XmlItem { Name = "Wireless Mouse", Price = 29.99m },
            new XmlItem { Name = "Mechanical Keyboard", Price = 149.99m }
        }
    };

    public string GetPlainText() => $@"MockHttp API - Plain Text Response
=====================================

Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
Endpoint: GET /formats/txt
Content-Type: text/plain

This is a plain text response suitable for:
- Log file simulation
- CSV data (without actual CSV formatting)
- Raw text content testing
- Legacy system integration testing

Status: OK
Server: MockHttp/1.0";

    public object GetUserProfile(int userId)
    {
        var random = new Random(userId);
        return new
        {
            id = userId,
            username = $"user_{userId}",
            email = $"user{userId}@example.com",
            profile = new
            {
                firstName = _firstNames[userId % _firstNames.Length],
                lastName = _lastNames[userId % _lastNames.Length],
                age = 20 + (userId % 40),
                bio = $"I'm a mock user #{userId} for testing purposes.",
                avatar = $"/images/png?seed={userId}",
                verified = userId % 3 == 0,
                memberSince = DateTime.UtcNow.AddDays(-userId * 30).ToString("yyyy-MM-dd")
            },
            preferences = new
            {
                theme = userId % 2 == 0 ? "dark" : "light",
                language = "en-US",
                notifications = userId % 2 == 0
            },
            stats = new
            {
                posts = random.Next(0, 100),
                followers = random.Next(0, 1000),
                following = random.Next(0, 500)
            }
        };
    }

    public object GetProductCatalog(int page, int pageSize)
    {
        var random = new Random(page);
        var totalItems = 100;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = Enumerable.Range((page - 1) * pageSize + 1, Math.Min(pageSize, totalItems - (page - 1) * pageSize))
            .Select(i => new
            {
                id = i,
                sku = $"PROD-{i:D6}",
                name = $"{_products[i % _products.Length]} Model {i}",
                category = _categories[i % _categories.Length],
                price = Math.Round(random.NextDouble() * 1000 + 50, 2),
                inStock = i % 4 != 0,
                rating = Math.Round(random.NextDouble() * 2 + 3, 1),
                reviews = random.Next(0, 500),
                imageUrl = $"/images/jpeg?id={i}",
                createdAt = DateTime.UtcNow.AddDays(-random.Next(1, 365)).ToString("yyyy-MM-dd")
            }).ToArray();

        return new
        {
            success = true,
            data = items,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalPages,
                totalItems,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            },
            links = new
            {
                self = $"/api/products?page={page}&pageSize={pageSize}",
                first = $"/api/products?page=1&pageSize={pageSize}",
                last = $"/api/products?page={totalPages}&pageSize={pageSize}",
                next = page < totalPages ? $"/api/products?page={page + 1}&pageSize={pageSize}" : null,
                prev = page > 1 ? $"/api/products?page={page - 1}&pageSize={pageSize}" : null
            }
        };
    }

    public object GetOrderDetails(Guid orderId)
    {
        var random = new Random(orderId.GetHashCode());
        var itemCount = random.Next(1, 6);

        return new
        {
            orderId,
            orderNumber = $"ORD-{Math.Abs(orderId.GetHashCode()):D10}",
            status = _orderStatuses[Math.Abs(orderId.GetHashCode()) % _orderStatuses.Length],
            customer = new
            {
                id = random.Next(1, 10000),
                name = $"{_firstNames[random.Next(_firstNames.Length)]} {_lastNames[random.Next(_lastNames.Length)]}",
                email = $"customer{random.Next(1000)}@example.com"
            },
            items = Enumerable.Range(1, itemCount).Select(i => new
            {
                id = random.Next(1, 100),
                name = $"{_products[random.Next(_products.Length)]} Model {random.Next(100)}",
                quantity = random.Next(1, 5),
                unitPrice = Math.Round(random.NextDouble() * 500 + 50, 2),
                total = Math.Round((random.NextDouble() * 500 + 50) * random.Next(1, 5), 2)
            }).ToArray(),
            shipping = new
            {
                address = new
                {
                    street = $"{random.Next(100, 9999)} Main Street",
                    city = "San Francisco",
                    state = "CA",
                    zipCode = $"{random.Next(90000, 99999)}",
                    country = "USA"
                },
                method = random.Next(2) == 0 ? "Standard" : "Express",
                trackingNumber = $"TRK{random.Next(100000, 999999)}"
            },
            payment = new
            {
                method = "Credit Card",
                last4 = $"{random.Next(1000, 9999)}",
                status = "Completed"
            },
            dates = new
            {
                ordered = DateTime.UtcNow.AddDays(-random.Next(1, 30)).ToString("yyyy-MM-dd HH:mm:ss"),
                shipped = DateTime.UtcNow.AddDays(-random.Next(0, 25)).ToString("yyyy-MM-dd HH:mm:ss"),
                estimatedDelivery = DateTime.UtcNow.AddDays(random.Next(1, 7)).ToString("yyyy-MM-dd")
            },
            totals = new
            {
                subtotal = Math.Round(random.NextDouble() * 500 + 100, 2),
                tax = Math.Round(random.NextDouble() * 50 + 10, 2),
                shipping = Math.Round(random.NextDouble() * 20 + 5, 2),
                total = Math.Round(random.NextDouble() * 570 + 115, 2)
            }
        };
    }
}
