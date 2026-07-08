using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService_SB.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserProfileAndLockoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "account_locked_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_end_time",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "user_profiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "dpi",
                table: "user_profiles",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "job_name",
                table: "user_profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_income",
                table: "user_profiles",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_dpi",
                table: "user_profiles",
                column: "dpi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_profiles_dpi",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "account_locked_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_end_time",
                table: "users");

            migrationBuilder.DropColumn(
                name: "address",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "dpi",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "job_name",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "monthly_income",
                table: "user_profiles");
        }
    }
}
