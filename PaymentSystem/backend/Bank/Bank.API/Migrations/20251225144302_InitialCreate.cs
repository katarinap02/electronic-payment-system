using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bank.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PhoneHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WebShopUserIdHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountNumber = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    SwiftCode = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false, defaultValue: "BACXRSBG"),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AvailableBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ReservedBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PendingCaptureBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    IsMerchantAccount = table.Column<bool>(type: "boolean", nullable: false),
                    MerchantId = table.Column<string>(type: "text", nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CardHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MaskedPan = table.Column<string>(type: "character varying(19)", maxLength: 19, nullable: false),
                    LastFourDigits = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    CardholderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpiryMonth = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ExpiryYear = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(50)", nullable: false),
                    CvvSalt = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PinAttempts = table.Column<int>(type: "integer", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CardTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardId = table.Column<long>(type: "bigint", nullable: false),
                    CvvHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CvvValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardTokens_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MerchantId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MerchantTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Stan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GlobalTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PspTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcquirerTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AuthorizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MerchantAccountId = table.Column<long>(type: "bigint", nullable: false),
                    CardTokenId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerAccountId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(50)", nullable: true),
                    CardId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_BankAccounts_CustomerAccountId",
                        column: x => x.CustomerAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_BankAccounts_MerchantAccountId",
                        column: x => x.MerchantAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_CardTokens_CardTokenId",
                        column: x => x.CardTokenId,
                        principalTable: "CardTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CustomerId",
                table: "BankAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_MerchantId",
                table: "BankAccounts",
                column: "MerchantId",
                unique: true,
                filter: "\"MerchantId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardHash",
                table: "Cards",
                column: "CardHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CustomerId",
                table: "Cards",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CardTokens_CardId",
                table: "CardTokens",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardTokens_ExpiresAt",
                table: "CardTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CardTokens_IsUsed",
                table: "CardTokens",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_CardTokens_Token",
                table: "CardTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CardId",
                table: "PaymentTransactions",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CardTokenId",
                table: "PaymentTransactions",
                column: "CardTokenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CustomerAccountId",
                table: "PaymentTransactions",
                column: "CustomerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CustomerId",
                table: "PaymentTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ExpiresAt",
                table: "PaymentTransactions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_GlobalTransactionId",
                table: "PaymentTransactions",
                column: "GlobalTransactionId",
                unique: true,
                filter: "\"GlobalTransactionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_MerchantAccountId",
                table: "PaymentTransactions",
                column: "MerchantAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_MerchantId_Status",
                table: "PaymentTransactions",
                columns: new[] { "MerchantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_MerchantTimestamp",
                table: "PaymentTransactions",
                column: "MerchantTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Stan",
                table: "PaymentTransactions",
                column: "Stan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Status",
                table: "PaymentTransactions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "CardTokens");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
