To create a dynamic LINQ query that groups and sums `Freight` by the specified fields — while allowing for flexible `AND`/`OR` filters — you'll want to combine dynamic predicate building with grouping and aggregation. Here's how you can approach it:

---

### 🧠 Step 1: Define Your Filters
You'll likely want to create a flexible filter system using `PredicateBuilder` from the [LINQKit](https://github.com/scottksmith95/LINQKit) library, or manually construct expressions.

```csharp
// Example filters (modify based on user inputs)
string? cityFilter = "Seattle";
string? countryFilter = null;
string? shipperFilter = "Speedy Express";
```

---

### 🔧 Step 2: Build the Dynamic Where Clause

```csharp
var queryableData = dbContext.OrderDetailsDynamic.AsQueryable();

if (!string.IsNullOrEmpty(cityFilter))
    queryableData = queryableData.Where(o => o.ShipCity == cityFilter);

if (!string.IsNullOrEmpty(countryFilter))
    queryableData = queryableData.Where(o => o.ShipCountry == countryFilter);

if (!string.IsNullOrEmpty(shipperFilter))
    queryableData = queryableData.Where(o => o.ShipperName == shipperFilter);
```

If you'd like to support combining filters with `OR`, you can use LINQKit like this:

```csharp
var predicate = PredicateBuilder.New<OrderDetailsDynamic>(true);

if (!string.IsNullOrEmpty(cityFilter))
    predicate = predicate.Or(o => o.ShipCity == cityFilter);

if (!string.IsNullOrEmpty(shipperFilter))
    predicate = predicate.Or(o => o.ShipperName == shipperFilter);

queryableData = queryableData.AsExpandable().Where(predicate);
```

---

### 📊 Step 3: Group and Sum Freight

Now group by the specified fields and calculate the total freight:

```csharp
var result = queryableData
    .GroupBy(o => new 
    { 
        o.ShipCity, 
        o.ShipCountry, 
        o.ShipperName, 
        o.EmployeeName, 
        o.CustomerName 
    })
    .Select(g => new 
    {
        g.Key.ShipCity,
        g.Key.ShipCountry,
        g.Key.ShipperName,
        g.Key.EmployeeName,
        g.Key.CustomerName,
        TotalFreight = g.Sum(x => x.Freight ?? 0)
    })
    .ToList();
```

---

### ✅ Output Structure

| ShipCity | ShipCountry | ShipperName | EmployeeName | CustomerName | TotalFreight |
|----------|-------------|-------------|---------------|---------------|----------------|
| Seattle  | USA         | Speedy Express | Nancy Davolio | Alfreds Futterkiste | 350.45 |

---

Let me know if you'd like this turned into a reusable method with parameterized filters or used in a UI-based filter scenario 👨‍💻