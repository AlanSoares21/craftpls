﻿// <auto-generated />
using System;
using AlternativeMkt.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlternativeMkt.Db.Migrations
{
    [DbContext(typeof(MktDbContext))]
    [Migration("20240319211856_CreateTableToStoreNamesByCulture")]
    partial class CreateTableToStoreNamesByCulture
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AlternativeMkt.Db.Asset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasMaxLength(69)
                        .HasColumnType("character varying(69)")
                        .HasColumnName("endpoint");

                    b.HasKey("Id")
                        .HasName("asset_pkey");

                    b.HasIndex(new[] { "Endpoint" }, "assets_endpoint_key")
                        .IsUnique();

                    b.ToTable("assets", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.Attribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("attributes_key");

                    b.ToTable("attributes", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("craft_categories_pkey");

                    b.ToTable("craft_categories", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("AssetId")
                        .HasColumnType("integer")
                        .HasColumnName("assetid");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("integer")
                        .HasColumnName("categoryid");

                    b.Property<int?>("Level")
                        .HasColumnType("integer")
                        .HasColumnName("level");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("craft_items_pkey");

                    b.HasIndex("AssetId");

                    b.HasIndex("CategoryId");

                    b.ToTable("craft_items", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItemAttribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AttributeId")
                        .HasColumnType("integer")
                        .HasColumnName("attributeid");

                    b.Property<int>("ItemId")
                        .HasColumnType("integer")
                        .HasColumnName("itemid");

                    b.Property<float>("Value")
                        .HasColumnType("real")
                        .HasColumnName("value");

                    b.HasKey("Id")
                        .HasName("craft_items_attributes_key");

                    b.HasIndex("AttributeId");

                    b.HasIndex("ItemId");

                    b.ToTable("craft_items_attributes", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItemDataByCulture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Culture")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ItemId")
                        .HasColumnType("integer")
                        .HasColumnName("itemid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("craft_items_data_by_culture_pkey");

                    b.HasIndex("ItemId");

                    b.ToTable("craft_items_data_by_culture", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItemsPrice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("createdat")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("deletedat");

                    b.Property<int>("ItemId")
                        .HasColumnType("integer")
                        .HasColumnName("itemid");

                    b.Property<Guid>("ManufacturerId")
                        .HasColumnType("uuid")
                        .HasColumnName("manufacturerid");

                    b.Property<int>("Price")
                        .HasColumnType("integer")
                        .HasColumnName("price");

                    b.Property<bool>("ResourcesChanged")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("resourceschanged");

                    b.Property<int>("TotalPrice")
                        .HasColumnType("integer")
                        .HasColumnName("totalprice");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updatedat");

                    b.HasKey("Id")
                        .HasName("craft_items_prices_pkey");

                    b.HasIndex("ItemId");

                    b.HasIndex("ManufacturerId");

                    b.ToTable("craft_items_prices", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftResource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<short?>("Amount")
                        .HasColumnType("smallint")
                        .HasColumnName("ammount");

                    b.Property<int>("ItemId")
                        .HasColumnType("integer")
                        .HasColumnName("itemid");

                    b.Property<int>("ResourceId")
                        .HasColumnType("integer")
                        .HasColumnName("resourceid");

                    b.HasKey("Id")
                        .HasName("craft_resource_pkey");

                    b.HasIndex("ItemId");

                    b.HasIndex("ResourceId");

                    b.ToTable("craft_resource", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CreateUserAccountRequest", b =>
                {
                    b.Property<string>("Email")
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<DateTime?>("AcceptedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("acceptedat");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("createdat")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Email")
                        .HasName("users_create_account_request_key");

                    b.ToTable("create_user_account_requests", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.GameAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("createdat")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("deletedat");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<byte>("ServerId")
                        .HasColumnType("smallint")
                        .HasColumnName("serverid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updatedat");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("userid");

                    b.HasKey("Id")
                        .HasName("game_accounts_pkey");

                    b.HasIndex("ServerId");

                    b.HasIndex("UserId");

                    b.ToTable("game_accounts", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.Manufacturer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("createdat")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("deletedat");

                    b.Property<bool>("Hide")
                        .HasColumnType("boolean")
                        .HasColumnName("hide");

                    b.Property<short>("MaxRequestsAccepted")
                        .HasColumnType("smallint")
                        .HasColumnName("maxrequestsaccepted");

                    b.Property<short>("MaxRequestsOpen")
                        .HasColumnType("smallint")
                        .HasColumnName("maxrequestsopen");

                    b.Property<byte>("ServerId")
                        .HasColumnType("smallint")
                        .HasColumnName("serverid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updatedat");

                    b.Property<Guid>("Userid")
                        .HasColumnType("uuid")
                        .HasColumnName("userid");

                    b.HasKey("Id")
                        .HasName("manufacturers_pkey");

                    b.HasIndex("ServerId");

                    b.HasIndex("Userid");

                    b.ToTable("manufacturers", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.Request", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime?>("Accepted")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("accepted");

                    b.Property<DateTime?>("Cancelled")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("cancelled");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("createdat")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("deletedat");

                    b.Property<DateTime?>("FinishedByManufacturer")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("finishedbymanufacturer");

                    b.Property<DateTime?>("FinishedByRequester")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("finishedbyrequester");

                    b.Property<int>("ItemId")
                        .HasColumnType("integer")
                        .HasColumnName("itemid");

                    b.Property<Guid>("ManufacturerId")
                        .HasColumnType("uuid")
                        .HasColumnName("manufacturerid");

                    b.Property<int>("Price")
                        .HasColumnType("integer")
                        .HasColumnName("price");

                    b.Property<DateTime?>("Refused")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("refused");

                    b.Property<Guid>("RequesterId")
                        .HasColumnType("uuid")
                        .HasColumnName("requesterid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updatedat");

                    b.HasKey("Id")
                        .HasName("requests_pkey");

                    b.HasIndex("ItemId");

                    b.HasIndex("ManufacturerId");

                    b.HasIndex("RequesterId");

                    b.ToTable("requests", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.ResourceProvided", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uuid")
                        .HasColumnName("requestid");

                    b.Property<int>("ResourceId")
                        .HasColumnType("integer")
                        .HasColumnName("resourceid");

                    b.HasKey("Id")
                        .HasName("resource_provided_key");

                    b.HasIndex("ResourceId");

                    b.HasIndex("RequestId", "ResourceId")
                        .IsUnique();

                    b.ToTable("resources_provided", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("roles_pkey");

                    b.HasIndex(new[] { "Name" }, "roles_name_key")
                        .IsUnique();

                    b.ToTable("roles", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.Server", b =>
                {
                    b.Property<byte>("Id")
                        .HasColumnType("smallint")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("server_pkey");

                    b.ToTable("servers", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("createdat")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("deletedat");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("email");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updatedat");

                    b.HasKey("Id")
                        .HasName("users_pkey");

                    b.HasIndex(new[] { "Email" }, "users_email_key")
                        .IsUnique();

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.UserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("createdat")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("deletedat");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer")
                        .HasColumnName("roleid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("userid");

                    b.HasKey("Id")
                        .HasName("users_roles_pkey");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("users_roles", (string)null);
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItem", b =>
                {
                    b.HasOne("AlternativeMkt.Db.Asset", "Asset")
                        .WithMany("CraftItems")
                        .HasForeignKey("AssetId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("craft_items_asset_fkey");

                    b.HasOne("AlternativeMkt.Db.CraftCategory", "Category")
                        .WithMany("CraftItems")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("craft_items_craftcategoryid_fkey");

                    b.Navigation("Asset");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItemAttribute", b =>
                {
                    b.HasOne("AlternativeMkt.Db.Attribute", "Attribute")
                        .WithMany("CraftItems")
                        .HasForeignKey("AttributeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("craft_items_attributes_attributeid_fkey");

                    b.HasOne("AlternativeMkt.Db.CraftItem", "Item")
                        .WithMany("Attributes")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("craft_items_attributes_itemid_fkey");

                    b.Navigation("Attribute");

                    b.Navigation("Item");
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItemDataByCulture", b =>
                {
                    b.HasOne("AlternativeMkt.Db.CraftItem", "Item")
                        .WithMany("DataByCulture")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("craft_items_data_by_culture_fkey");

                    b.Navigation("Item");
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItemsPrice", b =>
                {
                    b.HasOne("AlternativeMkt.Db.CraftItem", "Item")
                        .WithMany("Prices")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("craft_items_prices_craftitemid_fkey");

                    b.HasOne("AlternativeMkt.Db.Manufacturer", "Manufacturer")
                        .WithMany("CraftItemsPrices")
                        .HasForeignKey("ManufacturerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("craft_items_prices_manufacturerid_fkey");

                    b.Navigation("Item");

                    b.Navigation("Manufacturer");
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftResource", b =>
                {
                    b.HasOne("AlternativeMkt.Db.CraftItem", "Item")
                        .WithMany("Resources")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("craftresource_itemid_fkey");

                    b.HasOne("AlternativeMkt.Db.CraftItem", "Resource")
                        .WithMany("ResourceFor")
                        .HasForeignKey("ResourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("craftresource_resourceid_fkey");

                    b.Navigation("Item");

                    b.Navigation("Resource");
                });

            modelBuilder.Entity("AlternativeMkt.Db.GameAccount", b =>
                {
                    b.HasOne("AlternativeMkt.Db.Server", "Server")
                        .WithMany("GameAccounts")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("game_accounts_serverid_fkey");

                    b.HasOne("AlternativeMkt.Db.User", "User")
                        .WithMany("GameAccounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("game_accounts_userid_fkey");

                    b.Navigation("Server");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Manufacturer", b =>
                {
                    b.HasOne("AlternativeMkt.Db.Server", "Server")
                        .WithMany("Manufacturers")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("manufacturers_serverid_fkey");

                    b.HasOne("AlternativeMkt.Db.User", "User")
                        .WithMany("Manufacturers")
                        .HasForeignKey("Userid")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("manufacturers_userid_fkey");

                    b.Navigation("Server");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Request", b =>
                {
                    b.HasOne("AlternativeMkt.Db.CraftItem", "Item")
                        .WithMany("Requests")
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("requests_itemid_fkey");

                    b.HasOne("AlternativeMkt.Db.Manufacturer", "Manufacturer")
                        .WithMany("Requests")
                        .HasForeignKey("ManufacturerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("requests_manufacturerid_fkey");

                    b.HasOne("AlternativeMkt.Db.User", "Requester")
                        .WithMany("Requests")
                        .HasForeignKey("RequesterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("requests_requesterid_fkey");

                    b.Navigation("Item");

                    b.Navigation("Manufacturer");

                    b.Navigation("Requester");
                });

            modelBuilder.Entity("AlternativeMkt.Db.ResourceProvided", b =>
                {
                    b.HasOne("AlternativeMkt.Db.Request", "Request")
                        .WithMany("ResourcesProvided")
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("resources_provided_requestid_fkey");

                    b.HasOne("AlternativeMkt.Db.CraftResource", "Resource")
                        .WithMany("ProvidedIn")
                        .HasForeignKey("ResourceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("resources_provided_resourceid_fkey");

                    b.Navigation("Request");

                    b.Navigation("Resource");
                });

            modelBuilder.Entity("AlternativeMkt.Db.UserRole", b =>
                {
                    b.HasOne("AlternativeMkt.Db.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("users_roles_rolesid_fkey");

                    b.HasOne("AlternativeMkt.Db.User", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("users_roles_userid_fkey");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Asset", b =>
                {
                    b.Navigation("CraftItems");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Attribute", b =>
                {
                    b.Navigation("CraftItems");
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftCategory", b =>
                {
                    b.Navigation("CraftItems");
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftItem", b =>
                {
                    b.Navigation("Attributes");

                    b.Navigation("DataByCulture");

                    b.Navigation("Prices");

                    b.Navigation("Requests");

                    b.Navigation("ResourceFor");

                    b.Navigation("Resources");
                });

            modelBuilder.Entity("AlternativeMkt.Db.CraftResource", b =>
                {
                    b.Navigation("ProvidedIn");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Manufacturer", b =>
                {
                    b.Navigation("CraftItemsPrices");

                    b.Navigation("Requests");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Request", b =>
                {
                    b.Navigation("ResourcesProvided");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("AlternativeMkt.Db.Server", b =>
                {
                    b.Navigation("GameAccounts");

                    b.Navigation("Manufacturers");
                });

            modelBuilder.Entity("AlternativeMkt.Db.User", b =>
                {
                    b.Navigation("GameAccounts");

                    b.Navigation("Manufacturers");

                    b.Navigation("Requests");

                    b.Navigation("Roles");
                });
#pragma warning restore 612, 618
        }
    }
}
