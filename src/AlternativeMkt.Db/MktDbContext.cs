using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AlternativeMkt.Db;

public partial class MktDbContext : DbContext
{
    IConfiguration _configuration;

    public MktDbContext(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public MktDbContext(
        DbContextOptions<MktDbContext> options,
        IConfiguration configuration): base(options) 
    {
        _configuration = configuration;
    }

    public virtual DbSet<CraftCategory> CraftCategories { get; set; }

    public virtual DbSet<CraftItem> CraftItems { get; set; }

    public virtual DbSet<CraftItemsPrice> CraftItemsPrices { get; set; }

    public virtual DbSet<CraftResource> CraftResources { get; set; }

    public virtual DbSet<GameAccount> GameAccounts { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        string? connstring = _configuration.GetConnectionString("MainDb");
        if (connstring is null)
            throw new Exception($"Connection string MainDb is null \n {JsonSerializer.Serialize(_configuration)}");
        optionsBuilder.UseNpgsql(connstring);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CraftCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("craft_categories_pkey");

            entity.ToTable("craft_categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CraftItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("craft_items_pkey");

            entity.ToTable("craft_items");

            entity.HasIndex(e => e.Name, "craft_items_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("categoryid");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Category).WithMany(p => p.CraftItems)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("craft_items_craftcategoryid_fkey");
        });

        modelBuilder.Entity<CraftItemsPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("craft_items_prices_pkey");

            entity.ToTable("craft_items_prices");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ItemId).HasColumnName("itemid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deletedat");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturerid");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Item).WithMany(p => p.Prices)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("craft_items_prices_craftitemid_fkey");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.CraftItemsPrices)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("craft_items_prices_manufacturerid_fkey");
        });

        modelBuilder.Entity<CraftResource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("craft_resource_pkey");

            entity.ToTable("craft_resource");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("ammount");
            entity.Property(e => e.ItemId).HasColumnName("itemid");
            entity.Property(e => e.ResourceId).HasColumnName("resourceid");

            entity.HasOne(d => d.Item).WithMany(p => p.Resources)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("craftresource_itemid_fkey");

            entity.HasOne(d => d.Resource).WithMany(p => p.ResourceFor)
                .HasForeignKey(d => d.ResourceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("craftresource_resourceid_fkey");
        });

        modelBuilder.Entity<GameAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_accounts_pkey");

            entity.ToTable("game_accounts");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deletedat");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.UserId).HasColumnName("userid");
            entity.Property(e => e.ServerId).HasColumnName("serverid");

            entity.HasOne(d => d.User).WithMany(p => p.GameAccounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("game_accounts_userid_fkey");
            
            entity.HasOne(d => d.Server).WithMany(e => e.GameAccounts)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("game_accounts_serverid_fkey");
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("manufacturers_pkey");

            entity.ToTable("manufacturers");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deletedat");
            entity.Property(e => e.Hide).HasColumnName("hide");
            entity.Property(e => e.MaxRequestsAccepted).HasColumnName("maxrequestsaccepted");
            entity.Property(e => e.MaxRequestsOpen).HasColumnName("maxrequestsopen");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.ServerId).HasColumnName("serverid");

            entity.HasOne(d => d.User).WithMany(p => p.Manufacturers)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("manufacturers_userid_fkey");

            entity.HasOne(d => d.Server).WithMany(e => e.Manufacturers)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("manufacturers_serverid_fkey");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("requests_pkey");

            entity.ToTable("requests");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Accepted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("accepted");
            entity.Property(e => e.Cancelled)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("cancelled");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deletedat");
            entity.Property(e => e.ItemId).HasColumnName("itemid");
            entity.Property(e => e.FinishedByManufacturer)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("finishedbymanufacturer");
            entity.Property(e => e.FinishedByRequester)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("finishedbyrequester");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturerid");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Refused)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refused");
            entity.Property(e => e.RequesterId).HasColumnName("requesterid");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Item).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("requests_itemid_fkey");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Requests)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("requests_manufacturerid_fkey");

            entity.HasOne(d => d.Requester).WithMany(p => p.Requests)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("requests_requesterid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deletedat");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
        });

        modelBuilder.Entity<Server>(entity => {
            entity.HasKey(e => e.Id).HasName("server_pkey");

            entity.ToTable("servers");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100);
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
