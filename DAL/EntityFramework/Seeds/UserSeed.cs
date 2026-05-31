using ENTITIES.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.EntityFramework.Seeds;

/// <summary>
///     Seeds a default test <see cref="User" /> (email: <c>test@test.tst</c>, password:
///     <c>testtest</c>) via EF Core's <c>HasData</c>. Intended for dev/CI bootstrapping only —
///     swap credentials or remove before going to production.
///     <para>
///         <b>Everything here is a literal constant.</b> Calling <c>Guid.NewGuid()</c> or
///         <c>Pbkdf2PasswordHasher.GenerateSalt()</c> from a seed makes <c>HasData</c>
///         non-deterministic, and EF then scaffolds a destructive
///         <c>DeleteData</c>+<c>InsertData</c> swap into every new migration. If you change the
///         password text or rotate <see cref="CORE.Concrete.Pbkdf2PasswordHasher" />'s
///         algorithm / iteration count, regenerate <see cref="SeedSalt" /> +
///         <see cref="SeedPasswordHash" /> together — they're a matched pair. The hash below
///         was produced by <c>Pbkdf2PasswordHasher(100_000).Hash("testtest", SeedSalt)</c>.
///     </para>
/// </summary>
public class UserSeed
{
    // 16-byte salt: bytes 0x00..0x0F.
    private const string SeedSalt = "AAECAwQFBgcICQoLDA0ODw==";

    // PBKDF2-SHA512, 100_000 iterations, password "testtest", salt above, 64-byte output.
    private const string SeedPasswordHash =
        "bPw2I/Xjtt3/WfM8z1uVbzZG+fR3B1w+xEECKMwOYxbSwUrugZkfUjlLCgc117pQAO4p5lprl0wPD2dVIgirkg==";

    public static readonly Guid SeedUserId = new("00000000-0000-0000-0001-000000000001");

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = SeedUserId,
                Username = "Test",
                Email = "test@test.tst",
                Password = SeedPasswordHash,
                ContactNumber = "",
                RoleId = null,
                Salt = SeedSalt
            }
        );
    }
}