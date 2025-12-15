# Instructions for GitHub Copilot Agent: .NET 8 Mock HTTP Suite

## Role & Objective
Act as a Senior .NET Architect and Developer. Your task is to build a robust, scalable, and clean ASP.NET Core 8 Web API. This API will serve as a "Mock Suite" (similar to httpbin) for testing various HTTP scenarios.

## Global Technical Constraints
1.  **Framework:** ASP.NET Core 8.0 (C# 12).
2.  **Architecture:** Clean Architecture principles.
    * **Controller per Feature:** Group controllers by logical functionality.
    * **Services:** Logic must reside in Services, not Controllers.
    * **Dependency Injection:** All services must be injected via constructor injection.
3.  **Persistence:** **NO DATABASE**. Use in-memory data structures (e.g., `ConcurrentDictionary`, `List`) within Singleton services to simulate persistence.
4.  **SOLID Principles:** Strictly adhere to Single Responsibility, Open/Closed, and Interface Segregation.
5.  **Code Style:** Use File-scoped namespaces, Primary Constructors, and Async/Await patterns.
6.  **Serialization:** Ensure JSON default settings are used, but XML support is enabled.

---

## Phase 1: Project Setup & Configuration
1.  Initialize a new Web API project.
2.  **Program.cs Configuration:**
    * Enable `AddXmlSerializerFormatters()` to support XML responses.
    * Enable Swagger/OpenAPI for testing.
    * Register all Services created in subsequent steps.

## Phase 2: Implementation Steps

### Step 1: Content Negotiation & Formats
**Goal:** Return different content types based on specific endpoints.
1.  **Service:** Create `IResponseGeneratorService` with methods to generate sample simple JSON, complex nested JSON, HTML strings, and XML objects.
2.  **Controller:** Create `FormatsController`.
    * `GET /formats/json/simple`: Return a flat JSON object.
    * `GET /formats/json/complex`: Return a deeply nested JSON object.
    * `GET /formats/html`: Return a string with `Content-Type: text/html` containing valid HTML tags.
    * `GET /formats/xml`: Return an object formatted as XML.
    * `GET /formats/txt`: Return a plain text string.

### Step 2: Image Mocking
**Goal:** Return binary data representing images.
1.  **Service:** Create `IImageService` to generate byte arrays.
    * *Simulate* an image by returning a minimal valid byte array for a 1x1 pixel PNG and JPEG (hardcode the bytes if necessary to avoid external libraries like System.Drawing).
2.  **Controller:** Create `ImagesController`.
    * `GET /images/png`: Return `FileContentResult` with mime type `image/png`.
    * `GET /images/jpeg`: Return `FileContentResult` with mime type `image/jpeg`.

### Step 3: Request Inspection (The "Anything" API)
**Goal:** Echo back whatever the client sends.
1.  **DTO:** Create `RequestDetailsDto` containing properties for: Method, Url, Headers, QueryParams, BodyContent.
2.  **Controller:** Create `EchoController`.
    * `ANY /anything`: Accept any HTTP Verb.
    * Logic: Inspect `HttpContext.Request`. Read headers, query strings, and the body stream.
    * Return: The `RequestDetailsDto` as JSON.

### Step 4: Data Simulation (CRUD)
**Goal:** Simulate adding and updating data without a real DB.
1.  **Service:** Create `IMockDataStore` (Singleton).
    * Use a `ConcurrentDictionary<Guid, string>` to store mock "Items".
2.  **Controller:** Create `DataController`.
    * `GET /data`: Return all items.
    * `POST /data`: Add an item (Input: string value). Return 201 Created + Location header.
    * `PUT /data/{id}`: Update item.
    * `DELETE /data/{id}`: Remove item.

### Step 5: Delays & Status Codes
**Goal:** Test timeouts and error handling.
1.  **Controller:** Create `SimulationController`.
    * `GET /status/{code}`: Implementation should take the `{code}` integer (e.g., 404, 500, 418) and return that specific HTTP Status Result immediately.
    * `GET /delay/{seconds}`: Use `await Task.Delay(...)` for the specified seconds, then return 200 OK.

### Step 6: Auth Simulation
**Goal:** Mock auth logic manually (do not use ASP.NET Core Identity middleware, as we want to simulate the responses manually).
1.  **Controller:** Create `AuthController`.
    * `GET /auth/basic/{user}/{pass}`: Inspect `Authorization` header. Decode Base64. If it matches the route params, return 200, else 401.
    * `GET /auth/bearer`: Inspect `Authorization` header for "Bearer <token>". If present, return 200, else 401.
    * `GET /auth/digest`: (Simplified) Check for Digest header presence.

### Step 7: Cookie Management
**Goal:** Set and Read cookies.
1.  **Controller:** Create `CookiesController`.
    * `GET /cookies`: Return JSON of `Request.Cookies`.
    * `GET /cookies/set`: Read query params (key=value) and append them to `Response.Cookies`.
    * `GET /cookies/delete`: Read query param (key) and expire that cookie.

---

## Final Review Instructions
After generating the code:
1.  Ensure `Program.cs` has all `builder.Services.AddSingleton<...>` lines.
2.  Ensure no unnecessary `using` statements are present.
3.  Verify that Exception Handling is graceful (try-catch blocks in services where appropriate).