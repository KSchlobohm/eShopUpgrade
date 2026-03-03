# Entity Framework 6 to EF Core Migration

## Current State

### CatalogDBContext.cs
- Extends `System.Data.Entity.DbContext`
- Uses `DbModelBuilder` in `OnModelCreating`
- Uses `EntityTypeConfiguration<T>` for fluent configuration
- Has `HasRequired`, `WithMany`, `HasForeignKey` relationship configuration
- Uses `HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)`
- Connection string: `"name=CatalogDBContext"` from web.config

### CatalogDBInitializer.cs
- Extends `CreateDatabaseIfNotExists<CatalogDBContext>`
- Overrides `Seed()` to run SQL scripts and seed data
- Uses `HostingEnvironment.ApplicationPhysicalPath` for file paths
- Uses `context.Database.SqlQuery<Int64>()` for raw SQL
- Uses `context.Database.ExecuteSqlCommand()` for DDL scripts

### CatalogItemHiLoGenerator.cs
- Uses `db.Database.SqlQuery<Int64>("SELECT NEXT VALUE FOR catalog_hilo;")`

### CatalogService.cs
- Uses `db.CatalogItems.Include(c => c.CatalogBrand)` (EF6 eager loading)
- Uses `db.Entry(catalogItem).State = EntityState.Modified` for updates

### IdentityModels.cs
- `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>` from EF6-based Identity

## Challenge

### API Differences
| EF6 | EF Core |
|-----|---------|
| `System.Data.Entity.DbContext` | `Microsoft.EntityFrameworkCore.DbContext` |
| `DbModelBuilder` | `ModelBuilder` |
| `EntityTypeConfiguration<T>` | `IEntityTypeConfiguration<T>` or inline |
| `builder.HasKey(x => x.Id)` | `builder.HasKey(x => x.Id)` (same) |
| `builder.Property(x).IsRequired()` | `builder.Property(x).IsRequired()` (same) |
| `builder.HasRequired<T>().WithMany().HasForeignKey()` | `builder.HasOne<T>().WithMany().HasForeignKey()` |
| `HasDatabaseGeneratedOption(None)` | `ValueGeneratedNever()` |
| `builder.Ignore(x => x.Prop)` | `builder.Ignore(x => x.Prop)` (same) |
| `Database.SqlQuery<T>(sql)` | `FromSqlRaw(sql).ToList()` or `context.Database.SqlQueryRaw<T>()` |
| `Database.ExecuteSqlCommand(sql)` | `Database.ExecuteSqlRaw(sql)` |
| `CreateDatabaseIfNotExists<T>` | `Database.EnsureCreated()` or Migrations |
| `Database.SetInitializer<T>()` | No equivalent — use migrations or manual seed |

### Seed Data
EF Core uses `HasData()` in model configuration or custom `IDbContextFactory` seeder. The existing `CatalogDBInitializer` with CSV file parsing needs manual adaptation.

## Migration Plan

### Step 1: Update CatalogDBContext
```csharp
using Microsoft.EntityFrameworkCore;

public class CatalogDBContext : DbContext
{
    public CatalogDBContext(DbContextOptions<CatalogDBContext> options) : base(options) { }

    public DbSet<CatalogItem> CatalogItems { get; set; }
    public DbSet<CatalogBrand> CatalogBrands { get; set; }
    public DbSet<CatalogType> CatalogTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CatalogItem>(entity =>
        {
            entity.ToTable("Catalog");
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Id).ValueGeneratedNever().IsRequired();
            entity.Property(ci => ci.Name).IsRequired().HasMaxLength(50);
            entity.Property(ci => ci.Price).IsRequired();
            entity.Property(ci => ci.PictureFileName).IsRequired();
            entity.Ignore(ci => ci.PictureUri);
            entity.HasOne(ci => ci.CatalogBrand).WithMany().HasForeignKey(ci => ci.CatalogBrandId);
            entity.HasOne(ci => ci.CatalogType).WithMany().HasForeignKey(ci => ci.CatalogTypeId);
        });

        builder.Entity<CatalogBrand>(entity =>
        {
            entity.ToTable("CatalogBrand");
            entity.HasKey(ci => ci.Id);
            entity.Property(cb => cb.Brand).IsRequired().HasMaxLength(100);
        });

        builder.Entity<CatalogType>(entity =>
        {
            entity.ToTable("CatalogType");
            entity.HasKey(ci => ci.Id);
            entity.Property(ct => ct.Type).IsRequired().HasMaxLength(100);
        });

        base.OnModelCreating(builder);
    }
}
```

### Step 2: Register in DI
```csharp
builder.Services.AddDbContext<CatalogDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
```

### Step 3: Migrate HiLo Generator
```csharp
public int GetNextSequenceValue(CatalogDBContext db)
{
    lock (sequenceLock)
    {
        if (remainningLoIds == 0)
        {
            sequenceId = db.Database.SqlQueryRaw<int>("SELECT NEXT VALUE FOR catalog_hilo").Single();
            // ... rest stays the same
        }
    }
}
```

### Step 4: Migrate CatalogDBInitializer
Convert from `CreateDatabaseIfNotExists` override to a custom initializer service that:
1. Calls `context.Database.EnsureCreated()` or applies migrations
2. Uses `context.Database.ExecuteSqlRaw()` for sequence creation
3. Replaces `HostingEnvironment.ApplicationPhysicalPath` with `IWebHostEnvironment.ContentRootPath`

## Actions
- [ ] Replace System.Data.Entity with Microsoft.EntityFrameworkCore in CatalogDBContext
- [ ] Port fluent API: HasRequired→HasOne, HasDatabaseGeneratedOption→ValueGeneratedNever
- [ ] Add DbContextOptions constructor
- [ ] Register DbContext in DI with UseSqlServer
- [ ] Port CatalogDBInitializer to EF Core patterns
- [ ] Update CatalogItemHiLoGenerator: SqlQuery→SqlQueryRaw
- [ ] Update CatalogService: Include() namespace, Entry().State
- [ ] Build and verify

## Verification
- No `System.Data.Entity` references remain
- CatalogDBContext uses EF Core
- Database operations compile (CRUD, sequence, seed)
- `dotnet build` succeeds

## References
- [Port from EF6 to EF Core](https://learn.microsoft.com/ef/core/porting/)
- [EF Core model configuration](https://learn.microsoft.com/ef/core/modeling/)
