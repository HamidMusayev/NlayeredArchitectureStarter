using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <summary>
    ///     P1.1b finalization. Drops the plaintext <c>RefreshToken</c> column + its index and
    ///     makes <c>RefreshTokenHash</c> NOT NULL. Apply only after the
    ///     <c>RefreshTokenHashBackfillService</c> from PR #1 has run to completion so every
    ///     existing row has a non-null hash; un-backfilled rows will receive the default empty
    ///     string and become unauthenticatable (callers will have to re-login), which is the
    ///     intended behavior — no plaintext should survive on disk.
    ///     <para>
    ///         The auto-scaffolded version of this migration also re-seeded <c>Roles</c> + the
    ///         test <c>User</c> because <c>RoleSeed</c> uses <c>Guid.NewGuid()</c> and
    ///         <c>UserSeed</c> re-salts on every model build — a pre-existing seed-determinism
    ///         bug. Those operations are deliberately stripped here so this migration only does
    ///         the column drop. A separate follow-up should make the seeds deterministic.
    ///     </para>
    /// </summary>
    public partial class RemoveRefreshTokenPlaintext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_RefreshToken",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Tokens");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenHash",
                table: "Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Tokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_RefreshToken",
                table: "Tokens",
                column: "RefreshToken");
        }
    }
}
