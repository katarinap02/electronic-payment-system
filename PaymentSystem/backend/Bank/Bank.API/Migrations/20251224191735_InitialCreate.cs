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
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountNumber = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    SwiftCode = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsMerchantAccount = table.Column<bool>(type: "boolean", nullable: false),
                    MerchantId = table.Column<string>(type: "text", nullable: true),
                    ExternalUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<string>(type: "text", nullable: false),
                    MerchantId = table.Column<string>(type: "text", nullable: false),
                    MerchantTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Stan = table.Column<string>(type: "text", nullable: false),
                    GlobalTransactionId = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PspTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcquirerTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuthorizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MerchantAccountId = table.Column<long>(type: "bigint", nullable: false),
                    CardTokenId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerAccountId = table.Column<long>(type: "bigint", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "CardTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CardHash = table.Column<string>(type: "text", nullable: false),
                    MaskedPan = table.Column<string>(type: "text", nullable: false),
                    CardholderName = table.Column<string>(type: "text", nullable: false),
                    ExpiryMonth = table.Column<string>(type: "text", nullable: false),
                    ExpiryYear = table.Column<string>(type: "text", nullable: false),
                    CvvHash = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransactionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardTokens_PaymentTransactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_MerchantId",
                table: "BankAccounts",
                column: "MerchantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardTokens_CardHash",
                table: "CardTokens",
                column: "CardHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardTokens_Token",
                table: "CardTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardTokens_TransactionId",
                table: "CardTokens",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CustomerAccountId",
                table: "PaymentTransactions",
                column: "CustomerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_MerchantAccountId",
                table: "PaymentTransactions",
                column: "MerchantAccountId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardTokens");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "BankAccounts");
        }
    }
}
