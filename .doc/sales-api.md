[Back to README](../README.md)

### Sales

The Sales resource handles sales records following the `DDD` and `External Identities` patterns: customers, branches and products are referenced by their identifiers and have their descriptions denormalized into the sale.

Item discounts are calculated automatically by the API based on the quantity of identical items, according to the business rules:

- 4 or more identical items: 10% discount
- 10 to 20 identical items: 20% discount
- Below 4 identical items: no discount
- It is not possible to sell above 20 identical items

> Because discounts and totals are computed by the domain, the fields `discount`, `totalAmount`, `isCancelled` and timestamps are **read-only** and are not accepted in request bodies.

#### POST /api/Sales
- Description: Create a new sale
- Request Body:
  ```json
  {
    "saleNumber": "string",
    "saleDate": "string (date-time)",
    "customerId": "string (uuid)",
    "customerName": "string",
    "branchId": "string (uuid)",
    "branchName": "string",
    "items": [
      {
        "productId": "string (uuid)",
        "productName": "string",
        "quantity": "integer",
        "unitPrice": "number"
      }
    ]
  }
  ```
- Response: `201 Created`
  ```json
  {
    "data": {
      "id": "string (uuid)",
      "saleNumber": "string",
      "totalAmount": "number",
      "createdAt": "string (date-time)"
    },
    "success": true,
    "message": "Sale created successfully",
    "errors": []
  }
  ```

#### GET /api/Sales
- Description: Retrieve a paginated list of sales
- Query Parameters:
  - `_page` (optional): Page number for pagination (default: 1)
  - `_size` (optional): Number of items per page (default: 10)
  - `_order` (optional): Ordering of results (e.g., "saleDate desc, saleNumber asc")
- Response: `200 OK`
  ```json
  {
    "data": {
      "currentPage": "integer",
      "totalPages": "integer",
      "totalCount": "integer",
      "data": [
        {
          "id": "string (uuid)",
          "saleNumber": "string",
          "saleDate": "string (date-time)",
          "customerId": "string (uuid)",
          "customerName": "string",
          "branchId": "string (uuid)",
          "branchName": "string",
          "isCancelled": "boolean",
          "totalAmount": "number",
          "createdAt": "string (date-time)",
          "updatedAt": "string (date-time) | null",
          "items": [
            {
              "id": "string (uuid)",
              "productId": "string (uuid)",
              "productName": "string",
              "quantity": "integer",
              "unitPrice": "number",
              "discount": "number",
              "totalAmount": "number",
              "isCancelled": "boolean"
            }
          ]
        }
      ]
    },
    "success": true,
    "message": "",
    "errors": []
  }
  ```

#### GET /api/Sales/{id}
- Description: Retrieve a specific sale by ID
- Path Parameters:
  - `id`: Sale ID (uuid)
- Response: `200 OK`
  ```json
  {
    "data": {
      "id": "string (uuid)",
      "saleNumber": "string",
      "saleDate": "string (date-time)",
      "customerId": "string (uuid)",
      "customerName": "string",
      "branchId": "string (uuid)",
      "branchName": "string",
      "isCancelled": "boolean",
      "totalAmount": "number",
      "createdAt": "string (date-time)",
      "updatedAt": "string (date-time) | null",
      "items": [
        {
          "id": "string (uuid)",
          "productId": "string (uuid)",
          "productName": "string",
          "quantity": "integer",
          "unitPrice": "number",
          "discount": "number",
          "totalAmount": "number",
          "isCancelled": "boolean"
        }
      ]
    },
    "success": true,
    "message": "",
    "errors": []
  }
  ```

#### PUT /api/Sales/{id}
- Description: Update a specific sale. The sale is rebuilt from the supplied items, and discounts/totals are recalculated.
- Path Parameters:
  - `id`: Sale ID (uuid)
- Request Body:
  ```json
  {
    "saleNumber": "string",
    "saleDate": "string (date-time)",
    "customerId": "string (uuid)",
    "customerName": "string",
    "branchId": "string (uuid)",
    "branchName": "string",
    "items": [
      {
        "productId": "string (uuid)",
        "productName": "string",
        "quantity": "integer",
        "unitPrice": "number"
      }
    ]
  }
  ```
- Response: `200 OK`
  ```json
  {
    "data": {
      "id": "string (uuid)",
      "saleNumber": "string",
      "totalAmount": "number",
      "updatedAt": "string (date-time) | null"
    },
    "success": true,
    "message": "",
    "errors": []
  }
  ```

#### PATCH /api/Sales/{id}/cancel
- Description: Cancel an entire sale. Publishes a `SaleCancelled` event.
- Path Parameters:
  - `id`: Sale ID (uuid)
- Response: `200 OK`
  ```json
  {
    "data": {
      "id": "string (uuid)",
      "saleNumber": "string",
      "isCancelled": true,
      "totalAmount": "number",
      "updatedAt": "string (date-time) | null"
    },
    "success": true,
    "message": "",
    "errors": []
  }
  ```

#### PATCH /api/Sales/{saleId}/items/{itemId}/cancel
- Description: Cancel a single item of a sale. The sale total is recalculated and an `ItemCancelled` event is published.
- Path Parameters:
  - `saleId`: Sale ID (uuid)
  - `itemId`: Sale item ID (uuid)
- Response: `200 OK`
  ```json
  {
    "data": {
      "saleId": "string (uuid)",
      "itemId": "string (uuid)",
      "isCancelled": true,
      "totalAmount": "number",
      "updatedAt": "string (date-time) | null"
    },
    "success": true,
    "message": "",
    "errors": []
  }
  ```

### Domain Events

The following events are published (logged) by the domain as side effects of the operations above:

- `SaleCreated` — on `POST /api/Sales`
- `SaleModified` — on `PUT /api/Sales/{id}`
- `SaleCancelled` — on `PATCH /api/Sales/{id}/cancel`
- `ItemCancelled` — on `PATCH /api/Sales/{saleId}/items/{itemId}/cancel`

> For a ready-to-use sample payload validated against the database, see [Running the Application](./running-the-application.md).

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./auth-api.md">Previous: Auth API</a>
  <a href="./running-the-application.md">Next: Running the Application</a>
</div>
