namespace Shared.DataMigrator;

/// <summary>
/// Shared GUIDs for data migration across different bounded contexts
/// Ensures referential integrity between User and Address aggregates
/// </summary>
public static class SharedUserIds
{
    // Predefined user IDs that will be used consistently across User and Address migrations
    public static readonly Guid MarioRossi = new Guid("11111111-1111-1111-1111-111111111111");
    public static readonly Guid GiuliaBianchi = new Guid("22222222-2222-2222-2222-222222222222");
    public static readonly Guid LucaVerdi = new Guid("33333333-3333-3333-3333-333333333333");
    public static readonly Guid AnnaRomano = new Guid("44444444-4444-4444-4444-444444444444");
    public static readonly Guid MarcoFerrari = new Guid("55555555-5555-5555-5555-555555555555");
    public static readonly Guid SofiaEsposito = new Guid("66666666-6666-6666-6666-666666666666");
    public static readonly Guid AlessandroRicci = new Guid("77777777-7777-7777-7777-777777777777");
    public static readonly Guid ElenaMoretti = new Guid("88888888-8888-8888-8888-888888888888");
    public static readonly Guid FrancescoBarbieri = new Guid("99999999-9999-9999-9999-999999999999");
    public static readonly Guid ChiaraFontana = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>
    /// Returns all predefined user IDs in order
    /// </summary>
    public static IReadOnlyList<Guid> All => new[]
    {
        MarioRossi,
        GiuliaBianchi,
        LucaVerdi,
        AnnaRomano,
        MarcoFerrari,
        SofiaEsposito,
        AlessandroRicci,
        ElenaMoretti,
        FrancescoBarbieri,
        ChiaraFontana
    };
}

