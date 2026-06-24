[Back to README](../README.md)

## Running the Application

This guide shows how to get the API up and running and how to validate it with a real sale payload.

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/)
- (Optional, for running locally without containers) [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Running with Docker Compose

The `docker-compose.yml` file lives in `template/backend` and provisions the Web API together with its dependencies (PostgreSQL, MongoDB and Redis).

```bash
cd template/backend
docker compose up -d --build
```

This builds the API image and starts every service. The Web API is exposed on:

- HTTP: `http://localhost:8080`
- HTTPS: `https://localhost:8081`

Useful commands:

```bash
# Check the status of the containers
docker compose ps

# Follow the API logs (domain events are logged here)
docker compose logs -f ambev.developerevaluation.webapi

# Stop and remove the containers
docker compose down

# Stop and also remove the database volume (clean slate)
docker compose down -v
```

Once the containers are healthy, verify the API is up:

```bash
curl http://localhost:8080/health
# {"status":"Healthy","healthChecks":[]}
```

Swagger UI is available at `http://localhost:8080/swagger`.

### Running locally with .NET (optional)

If you prefer to run the API directly while keeping the databases in containers:

```bash
cd template/backend

# Start only the infrastructure dependencies
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.nosql ambev.developerevaluation.cache

# Run the API
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

### Creating a sale (sample payload)

The payload below was taken from a record persisted in the database and adjusted to reflect real-world data, so it is guaranteed to be valid against the business rules. It mixes the three discount scenarios in a single sale:

| Item | Quantity | Tier | Discount |
| --- | --- | --- | --- |
| Brahma Duplo Malte 350ml | 12 | 10–20 items | 20% |
| Skol Pilsen Lata 269ml | 6 | 4+ items | 10% |
| Guaraná Antarctica 2L | 2 | below 4 items | none |

```bash
curl -X POST http://localhost:8080/api/Sales \
  -H "Content-Type: application/json" \
  -d '{
    "saleNumber": "SALE-2026-000789",
    "saleDate": "2026-06-24T14:30:00Z",
    "customerId": "8d02d43d-51aa-4956-84a5-38ac33f38103",
    "customerName": "Mariana Oliveira",
    "branchId": "8aff67e1-b2bd-43c1-9809-d42e1b3fa53b",
    "branchName": "Filial Recife - Boa Viagem",
    "items": [
      {
        "productId": "de66dc8c-3c2f-440f-a4f3-aba82b942a2b",
        "productName": "Brahma Duplo Malte 350ml",
        "quantity": 12,
        "unitPrice": 4.50
      },
      {
        "productId": "01e39da8-918c-4b01-a60e-f563a4926c2d",
        "productName": "Skol Pilsen Lata 269ml",
        "quantity": 6,
        "unitPrice": 3.20
      },
      {
        "productId": "ff379ebf-f06a-42b8-8395-7929b1a0a2b2",
        "productName": "Guaraná Antarctica 2L",
        "quantity": 2,
        "unitPrice": 8.90
      }
    ]
  }'
```

The API responds with the created sale identifier and the calculated total:

```json
{
  "data": {
    "id": "5bce77da-5815-4bf4-845e-dd3be190519d",
    "saleNumber": "SALE-2026-000789",
    "totalAmount": 78.28,
    "createdAt": "2026-06-24T14:30:05.118273Z"
  },
  "success": true,
  "message": "Sale created successfully",
  "errors": []
}
```

### Retrieving the sale

Using the `id` returned above, fetch the full sale to inspect the computed discounts and item totals:

```bash
curl http://localhost:8080/api/Sales/5bce77da-5815-4bf4-845e-dd3be190519d
```

```json
{
  "data": {
    "id": "5bce77da-5815-4bf4-845e-dd3be190519d",
    "saleNumber": "SALE-2026-000789",
    "saleDate": "2026-06-24T14:30:00Z",
    "customerId": "8d02d43d-51aa-4956-84a5-38ac33f38103",
    "customerName": "Mariana Oliveira",
    "branchId": "8aff67e1-b2bd-43c1-9809-d42e1b3fa53b",
    "branchName": "Filial Recife - Boa Viagem",
    "isCancelled": false,
    "totalAmount": 78.28,
    "createdAt": "2026-06-24T14:30:05.118273Z",
    "updatedAt": null,
    "items": [
      {
        "id": "829b630c-bdb6-4a06-b578-243a73cdb3d3",
        "productId": "de66dc8c-3c2f-440f-a4f3-aba82b942a2b",
        "productName": "Brahma Duplo Malte 350ml",
        "quantity": 12,
        "unitPrice": 4.50,
        "discount": 0.2000,
        "totalAmount": 43.20,
        "isCancelled": false
      },
      {
        "id": "02a7e0d9-2151-44f9-b1a9-0969aa022881",
        "productId": "01e39da8-918c-4b01-a60e-f563a4926c2d",
        "productName": "Skol Pilsen Lata 269ml",
        "quantity": 6,
        "unitPrice": 3.20,
        "discount": 0.1000,
        "totalAmount": 17.28,
        "isCancelled": false
      },
      {
        "id": "38ea8577-6105-47f4-9f69-b2709eff585c",
        "productId": "ff379ebf-f06a-42b8-8395-7929b1a0a2b2",
        "productName": "Guaraná Antarctica 2L",
        "quantity": 2,
        "unitPrice": 8.90,
        "discount": 0.0000,
        "totalAmount": 17.80,
        "isCancelled": false
      }
    ]
  },
  "success": true,
  "message": "",
  "errors": []
}
```

For the full set of Sales endpoints, see the [Sales API](./sales-api.md) documentation.

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./sales-api.md">Previous: Sales API</a>
  <a href="../README.md">Back to README</a>
</div>
