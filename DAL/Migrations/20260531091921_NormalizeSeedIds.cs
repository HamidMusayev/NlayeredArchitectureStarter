using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeSeedIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("82ced5c0-a496-419b-9697-8b3bf3306515"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b30bc843-1d40-416e-9107-32e88988e68e"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d0630de8-59a0-40cd-801b-e2f889a010ed"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("eee9f4dc-f5dc-492a-b741-ad246f630c29"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8e8a8492-7284-433c-8332-4fe02b3e2c5c"));

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockedUntil",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedById", "DeletedAt", "DeletedBy", "IsDeleted", "Key", "ModifiedAt", "ModifiedBy", "Name", "TenantId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "SuperAdmin", null, null, "Baş inzibatçı", null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "Admin", null, null, "İnzibatçı", null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "User", null, null, "İstifadəçi", null },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "Guest", null, null, "Qonaq", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ContactNumber", "CreatedAt", "CreatedById", "DeletedAt", "DeletedBy", "Email", "FailedLoginAttempts", "IsDeleted", "LastVerificationCode", "LockedUntil", "ModifiedAt", "ModifiedBy", "Password", "ProfileFileId", "RoleId", "Salt", "TenantId", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0001-000000000001"), "", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "test@test.tst", 0, false, null, null, null, null, "bPw2I/Xjtt3/WfM8z1uVbzZG+fR3B1w+xEECKMwOYxbSwUrugZkfUjlLCgc117pQAO4p5lprl0wPD2dVIgirkg==", null, null, "AAECAwQFBgcICQoLDA0ODw==", null, "Test" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000001"));

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "Users");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedById", "DeletedAt", "DeletedBy", "IsDeleted", "Key", "ModifiedAt", "ModifiedBy", "Name", "TenantId" },
                values: new object[,]
                {
                    { new Guid("82ced5c0-a496-419b-9697-8b3bf3306515"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "User", null, null, "İstifadəçi", null },
                    { new Guid("b30bc843-1d40-416e-9107-32e88988e68e"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "SuperAdmin", null, null, "Baş inzibatçı", null },
                    { new Guid("d0630de8-59a0-40cd-801b-e2f889a010ed"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "Admin", null, null, "İnzibatçı", null },
                    { new Guid("eee9f4dc-f5dc-492a-b741-ad246f630c29"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, false, "Guest", null, null, "Qonaq", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ContactNumber", "CreatedAt", "CreatedById", "DeletedAt", "DeletedBy", "Email", "IsDeleted", "LastVerificationCode", "ModifiedAt", "ModifiedBy", "Password", "ProfileFileId", "RoleId", "Salt", "TenantId", "Username" },
                values: new object[] { new Guid("8e8a8492-7284-433c-8332-4fe02b3e2c5c"), "", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "test@test.tst", false, null, null, null, "Fag1UtnjsK8nWbXw0TYcL1cXyvLajju5c7/RcBbT+X+s4s+GiPyA7PsXtVH3vIUnlXOVHtss2wrOPyH0J2ycxg==", null, null, "5DHTEo4x6ybRXTXYcqiIjA==", null, "Test" });
        }
    }
}
