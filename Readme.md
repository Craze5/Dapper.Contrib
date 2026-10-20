Dapper.Contrib - a simple object mapper for .Net
========================================
[![Build status](https://ci.appveyor.com/api/projects/status/1w448i6nfxd14w75?svg=true)](https://ci.appveyor.com/project/StackExchange/dapper-contrib)

## Changes in this fork

This fork adds support for **DM (Dameng) Database 8** while preserving all original functionality.

### DM (Dameng) Adapter

- **Package**: `DM.DmProvider` 8.3.1.47463 (net7.0)
- **Connection class**: `DmConnection` (namespace `Dm`)
- **Parameter prefix**: `:` (Oracle-style, not `@`)
- **Identifier quoting**: Double-quoted identifiers (`"column"`)
- **Identity columns**: Compatible with SQL Server `IDENTITY(1,1)` and `SCOPE_IDENTITY()`
- **Test suite**: `DmTestSuite` in `Dapper.Tests.Contrib` (auto-skips if no DM server available)

### API changes

- `ISqlAdapter` interface: added `string GetParameterPrefix()` with C# 8 default implementation (`"@"`)
- `ISqlAdapter.Insert` / `InsertAsync` return type: changed from `int` to `long` to support 64-bit identity values
- `SqlMapperExtensions.Get<T>` / `GetAsync<T>`: cache key now includes connection type name to support per-adapter parameter prefixes
- `SqlMapperExtensions.Insert<T>` (list): parameter prefix now uses `adapter.GetParameterPrefix()` instead of hardcoding `@`
- `SqlMapperExtensions.GetAll<T>` / `GetAllAsync<T>`: fixed cache key to use `type.TypeHandle.Value.ToString()` (previously all types collided via `type.TypeHandle.ToString()`)
- **Postgres `Get<T>`**: now uses `AppendColumnName` (double-quoted identifiers) in the WHERE clause instead of raw property name; fixes case mismatch when tables were created with quoted identifiers
- **Postgres `Insert` / `InsertAsync`**: now uses `AppendColumnName` in the RETURNING clause and accesses result dictionary by property name directly (no `.ToLower()`); fixes lowercase key issue when columns are double-quoted

### Original project

All original features, attributes, and documentation below are preserved and remain unchanged from the upstream [DapperLib/Dapper.Contrib](https://github.com/DapperLib/Dapper.Contrib/) repository.

---

Release Notes
-------------
Located at [contrib.dapperlib.dev](https://contrib.dapperlib.dev/)

Packages
--------

MyGet Pre-release feed: https://www.myget.org/gallery/dapper

| Package | NuGet Stable | NuGet Pre-release | Downloads | MyGet |
| ------- | ------------ | ----------------- | --------- | ----- |
| [Dapper.Contrib](https://www.nuget.org/packages/Dapper.Contrib/) | [![Dapper.Contrib](https://img.shields.io/nuget/v/Dapper.Contrib.svg)](https://www.nuget.org/packages/Dapper.Contrib/) | [![Dapper.Contrib](https://img.shields.io/nuget/vpre/Dapper.Contrib.svg)](https://www.nuget.org/packages/Dapper.Contrib/) | [![Dapper.Contrib](https://img.shields.io/nuget/dt/Dapper.Contrib.svg)](https://www.nuget.org/packages/Dapper.Contrib/) | [![Dapper.Contrib MyGet](https://img.shields.io/myget/dapper/vpre/Dapper.Contrib.svg)](https://www.myget.org/feed/dapper/package/nuget/Dapper.Contrib) |

Features
--------

Dapper.Contrib contains a number of helper methods for inserting, getting,
updating and deleting records.

The full list of extension methods in Dapper.Contrib right now are:

```csharp
T Get<T>(id);
IEnumerable<T> GetAll<T>();
int Insert<T>(T obj);
int Insert<T>(Enumerable<T> list);
bool Update<T>(T obj);
bool Update<T>(Enumerable<T> list);
bool Delete<T>(T obj);
bool Delete<T>(Enumerable<T> list);
bool DeleteAll<T>();
```

For these extensions to work, the entity in question _MUST_ have a
key property. Dapper will automatically use a property named "`id`" 
(case-insensitive) as the key property, if one is present.

```csharp
public class Car
{
    public int Id { get; set; } // Works by convention
    public string Name { get; set; }
}
```

If the entity doesn't follow this convention, decorate 
a specific property with a `[Key]` or `[ExplicitKey]` attribute.

```csharp
public class User
{
    [Key]
    int TheId { get; set; }
    string Name { get; set; }
    int Age { get; set; }
}
```

`[Key]` should be used for database-generated keys (e.g. autoincrement columns), 
while `[ExplicitKey]` should be used for explicit keys generated in code.

`Get` methods
-------

Get one specific entity based on id

```csharp
var car = connection.Get<Car>(1);
```

or a list of all entities in the table.

```csharp
var cars = connection.GetAll<Car>();
```

`Insert` methods
-------

Insert one entity

```csharp
connection.Insert(new Car { Name = "Volvo" });
```

or a list of entities.

```csharp
connection.Insert(cars);
```



`Update` methods
-------
Update one specific entity

```csharp
connection.Update(new Car() { Id = 1, Name = "Saab" });
```

or update a list of entities.

```csharp
connection.Update(cars);
```

`Delete` methods
-------
Delete an entity by the specified `[Key]` property

```csharp
connection.Delete(new Car() { Id = 1 });
```

a list of entities

```csharp
connection.Delete(cars);
```

or _ALL_ entities in the table.

```csharp
connection.DeleteAll<Car>();
```

Special Attributes
----------
Dapper.Contrib makes use of some optional attributes:

* `[Table("Tablename")]` - use another table name instead of the (by default pluralized) name of the class

    ```csharp
    [Table ("emps")]
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    ```
* `[Key]` - this property represents a database-generated identity/key
    
    ```csharp
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        public string Name { get; set; }
    }
    ```
* `[ExplicitKey]` - this property represents an explicit identity/key which is 
  *not* automatically generated by the database 

    ```csharp
    public class Employee
    {
        [ExplicitKey]
        public Guid EmployeeId { get; set; }
        public string Name { get; set; }
    }
    ```
* `[Write(true/false)]` -  this property is (not) writeable
* `[Computed]` - this property is computed and should not be part of updates

Limitations and caveats
-------

### SQLite

`SQLiteConnection` exposes an `Update` event that clashes with the `Update`
extension provided by Dapper.Contrib. There are 2 ways to deal with this.

1. Call the `Update` method explicitly from `SqlMapperExtensions`

    ```Csharp
    SqlMapperExtensions.Update(_conn, new Employee { Id = 1, Name = "Mercedes" });
    ```
2. Make the method signature unique by passing a type parameter to `Update`

    ```Csharp
    connection.Update<Car>(new Car() { Id = 1, Name = "Maruti" });
    ```
