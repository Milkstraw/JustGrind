using GrindAtlas.API.Models;

namespace GrindAtlas.API.Data;

public static class SeedData
{
    public static void Seed(AppDbContext ctx)
    {
        if (ctx.Coffees.Any()) return;

        // ── GRINDERS ──────────────────────────────────────────────────────────
        var grinders = new List<Grinder>
        {
            new() { Id=1, Brand="Comandante", Model="C40 MK4",            GrindType=GrindType.Stepped,  BurrType=BurrType.Conical, BurrSizeMm=39, ScaleMin=0,   ScaleMax=40 },
            new() { Id=2, Brand="Baratza",    Model="Encore ESP",          GrindType=GrindType.Stepped,  BurrType=BurrType.Conical, BurrSizeMm=40, ScaleMin=1,   ScaleMax=40 },
            new() { Id=3, Brand="Fellow",     Model="Ode Gen 2",           GrindType=GrindType.Stepped,  BurrType=BurrType.Flat,    BurrSizeMm=64, ScaleMin=1,   ScaleMax=11 },
            new() { Id=4, Brand="1Zpresso",   Model="JX-Pro",              GrindType=GrindType.Stepped,  BurrType=BurrType.Conical, BurrSizeMm=48, ScaleMin=0,   ScaleMax=90 },
            new() { Id=5, Brand="Niche",      Model="Zero",                GrindType=GrindType.Stepless, BurrType=BurrType.Conical, BurrSizeMm=63, ScaleMin=0,   ScaleMax=50 },
            new() { Id=6, Brand="Eureka",     Model="Mignon Specialita",   GrindType=GrindType.Stepless, BurrType=BurrType.Flat,    BurrSizeMm=55, ScaleMin=0,   ScaleMax=6  },
            new() { Id=7, Brand="Timemore",   Model="C3 Pro",              GrindType=GrindType.Stepped,  BurrType=BurrType.Conical, BurrSizeMm=38, ScaleMin=0,   ScaleMax=30 },
            new() { Id=8, Brand="Hario",      Model="Skerton Pro",         GrindType=GrindType.Stepless, BurrType=BurrType.Conical, BurrSizeMm=38, ScaleMin=0,   ScaleMax=5  },
            new() { Id=9, Brand="Weber",      Model="EG-1 Mini",           GrindType=GrindType.Stepless, BurrType=BurrType.Flat,    BurrSizeMm=68, ScaleMin=0,   ScaleMax=10 },
            new() { Id=10,Brand="Baratza",    Model="Vario-W",             GrindType=GrindType.Stepped,  BurrType=BurrType.Flat,    BurrSizeMm=54, ScaleMin=1,   ScaleMax=10 },
        };
        ctx.Grinders.AddRange(grinders);

        // ── CALIBRATIONS (NGI 0-100 anchor points) ───────────────────────────
        // NGI reference: 10=espresso, 50=pour-over, 82=french-press, 90=cold-brew
        var cals = new List<GrinderCalibration>
        {
            // Comandante C40 MK4 (ID 1, scale 0-40)
            new() { Id=1,  GrinderId=1, BrewMethod=BrewMethod.Espresso,   NativeSetting=8m,   NgiValue=12m, AnchorLabel="Espresso baseline" },
            new() { Id=2,  GrinderId=1, BrewMethod=BrewMethod.PourOver,   NativeSetting=25m,  NgiValue=50m, AnchorLabel="V60 standard" },
            new() { Id=3,  GrinderId=1, BrewMethod=BrewMethod.FrenchPress,NativeSetting=35m,  NgiValue=82m, AnchorLabel="French press" },
            new() { Id=4,  GrinderId=1, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=38m,  NgiValue=90m, AnchorLabel="Cold brew" },
            // Baratza Encore ESP (ID 2, scale 1-40)
            new() { Id=5,  GrinderId=2, BrewMethod=BrewMethod.Espresso,   NativeSetting=5m,   NgiValue=10m, AnchorLabel="Espresso baseline" },
            new() { Id=6,  GrinderId=2, BrewMethod=BrewMethod.PourOver,   NativeSetting=20m,  NgiValue=50m, AnchorLabel="V60 standard" },
            new() { Id=7,  GrinderId=2, BrewMethod=BrewMethod.FrenchPress,NativeSetting=32m,  NgiValue=82m, AnchorLabel="French press" },
            new() { Id=8,  GrinderId=2, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=38m,  NgiValue=90m, AnchorLabel="Cold brew" },
            // Fellow Ode Gen 2 (ID 3, scale 1-11) – not ideal for espresso
            new() { Id=9,  GrinderId=3, BrewMethod=BrewMethod.AeropressFine,NativeSetting=3.5m,NgiValue=35m,AnchorLabel="AeroPress fine" },
            new() { Id=10, GrinderId=3, BrewMethod=BrewMethod.PourOver,   NativeSetting=5.0m, NgiValue=48m, AnchorLabel="V60 standard" },
            new() { Id=11, GrinderId=3, BrewMethod=BrewMethod.Chemex,     NativeSetting=6.0m, NgiValue=55m, AnchorLabel="Chemex" },
            new() { Id=12, GrinderId=3, BrewMethod=BrewMethod.FrenchPress,NativeSetting=8.0m, NgiValue=78m, AnchorLabel="French press" },
            new() { Id=13, GrinderId=3, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=10.0m,NgiValue=90m, AnchorLabel="Cold brew" },
            // 1Zpresso JX-Pro (ID 4, scale 0-90)
            new() { Id=14, GrinderId=4, BrewMethod=BrewMethod.Espresso,   NativeSetting=15m,  NgiValue=12m, AnchorLabel="Espresso baseline" },
            new() { Id=15, GrinderId=4, BrewMethod=BrewMethod.PourOver,   NativeSetting=50m,  NgiValue=50m, AnchorLabel="V60 standard" },
            new() { Id=16, GrinderId=4, BrewMethod=BrewMethod.FrenchPress,NativeSetting=75m,  NgiValue=82m, AnchorLabel="French press" },
            new() { Id=17, GrinderId=4, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=85m,  NgiValue=90m, AnchorLabel="Cold brew" },
            // Niche Zero (ID 5, scale 0-50)
            new() { Id=18, GrinderId=5, BrewMethod=BrewMethod.Espresso,   NativeSetting=10m,  NgiValue=10m, AnchorLabel="Espresso baseline" },
            new() { Id=19, GrinderId=5, BrewMethod=BrewMethod.PourOver,   NativeSetting=28m,  NgiValue=48m, AnchorLabel="V60 standard" },
            new() { Id=20, GrinderId=5, BrewMethod=BrewMethod.FrenchPress,NativeSetting=42m,  NgiValue=82m, AnchorLabel="French press" },
            new() { Id=21, GrinderId=5, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=48m,  NgiValue=90m, AnchorLabel="Cold brew" },
            // Eureka Mignon Specialita (ID 6, scale 0-6)
            new() { Id=22, GrinderId=6, BrewMethod=BrewMethod.Espresso,   NativeSetting=1.5m, NgiValue=10m, AnchorLabel="Espresso baseline" },
            new() { Id=23, GrinderId=6, BrewMethod=BrewMethod.PourOver,   NativeSetting=3.8m, NgiValue=50m, AnchorLabel="V60 standard" },
            new() { Id=24, GrinderId=6, BrewMethod=BrewMethod.FrenchPress,NativeSetting=5.2m, NgiValue=82m, AnchorLabel="French press" },
            new() { Id=25, GrinderId=6, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=5.8m, NgiValue=90m, AnchorLabel="Cold brew" },
            // Timemore C3 Pro (ID 7, scale 0-30)
            new() { Id=26, GrinderId=7, BrewMethod=BrewMethod.Espresso,   NativeSetting=6m,   NgiValue=12m, AnchorLabel="Espresso baseline" },
            new() { Id=27, GrinderId=7, BrewMethod=BrewMethod.PourOver,   NativeSetting=18m,  NgiValue=50m, AnchorLabel="V60 standard" },
            new() { Id=28, GrinderId=7, BrewMethod=BrewMethod.FrenchPress,NativeSetting=26m,  NgiValue=82m, AnchorLabel="French press" },
            new() { Id=29, GrinderId=7, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=29m,  NgiValue=90m, AnchorLabel="Cold brew" },
            // Hario Skerton Pro (ID 8, scale 0-5 turns)
            new() { Id=30, GrinderId=8, BrewMethod=BrewMethod.AeropressFine,NativeSetting=0.8m,NgiValue=32m,AnchorLabel="AeroPress fine" },
            new() { Id=31, GrinderId=8, BrewMethod=BrewMethod.PourOver,   NativeSetting=2.0m, NgiValue=45m, AnchorLabel="V60 standard" },
            new() { Id=32, GrinderId=8, BrewMethod=BrewMethod.FrenchPress,NativeSetting=3.5m, NgiValue=80m, AnchorLabel="French press" },
            new() { Id=33, GrinderId=8, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=4.5m, NgiValue=90m, AnchorLabel="Cold brew" },
            // Weber EG-1 Mini (ID 9, scale 0-10)
            new() { Id=34, GrinderId=9, BrewMethod=BrewMethod.Espresso,   NativeSetting=2.0m, NgiValue=10m, AnchorLabel="Espresso baseline" },
            new() { Id=35, GrinderId=9, BrewMethod=BrewMethod.PourOver,   NativeSetting=5.5m, NgiValue=50m, AnchorLabel="V60 standard" },
            new() { Id=36, GrinderId=9, BrewMethod=BrewMethod.FrenchPress,NativeSetting=8.0m, NgiValue=80m, AnchorLabel="French press" },
            new() { Id=37, GrinderId=9, BrewMethod=BrewMethod.ColdBrew,   NativeSetting=9.5m, NgiValue=90m, AnchorLabel="Cold brew" },
            // Baratza Vario-W (ID 10, scale 1-10 macro)
            new() { Id=38, GrinderId=10,BrewMethod=BrewMethod.Espresso,   NativeSetting=2.0m, NgiValue=12m, AnchorLabel="Espresso baseline" },
            new() { Id=39, GrinderId=10,BrewMethod=BrewMethod.PourOver,   NativeSetting=6.0m, NgiValue=50m, AnchorLabel="V60 standard" },
            new() { Id=40, GrinderId=10,BrewMethod=BrewMethod.FrenchPress,NativeSetting=8.5m, NgiValue=80m, AnchorLabel="French press" },
            new() { Id=41, GrinderId=10,BrewMethod=BrewMethod.ColdBrew,   NativeSetting=9.8m, NgiValue=90m, AnchorLabel="Cold brew" },
        };
        ctx.GrinderCalibrations.AddRange(cals);

        // ── COFFEES (60) ─────────────────────────────────────────────────────
        var coffees = new List<Coffee>
        {
            // ─── Ethiopian (1–6) ───
            new() { Id=1,  Name="Yirgacheffe Kochere",        Roaster="Onyx Coffee Lab",     OriginCountry="Ethiopia", OriginRegion="Yirgacheffe",  ElevationMasl=1900, ProcessingMethod=ProcessingMethod.Washed,   Variety="Heirloom", Species=Species.Arabica, RoastLevel=1.5m, TastingNotes="blueberry,jasmine,lemon,bergamot" },
            new() { Id=2,  Name="Yirgacheffe Gedeo Natural",  Roaster="Counter Culture",     OriginCountry="Ethiopia", OriginRegion="Yirgacheffe",  ElevationMasl=2100, ProcessingMethod=ProcessingMethod.Natural,  Variety="Heirloom", Species=Species.Arabica, RoastLevel=1.5m, TastingNotes="strawberry,blueberry,wine,floral" },
            new() { Id=3,  Name="Sidama Bensa Washed",        Roaster="Intelligentsia",      OriginCountry="Ethiopia", OriginRegion="Sidama",       ElevationMasl=2000, ProcessingMethod=ProcessingMethod.Washed,   Variety="Heirloom", Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="peach,black tea,orange,clean" },
            new() { Id=4,  Name="Guji Zone Uraga Natural",    Roaster="George Howell",       OriginCountry="Ethiopia", OriginRegion="Guji",         ElevationMasl=2200, ProcessingMethod=ProcessingMethod.Natural,  Variety="Heirloom", Species=Species.Arabica, RoastLevel=1.5m, TastingNotes="blackberry,rose,dark chocolate,complex" },
            new() { Id=5,  Name="Harrar Longberry",           Roaster="Blue Bottle",         OriginCountry="Ethiopia", OriginRegion="Harrar",       ElevationMasl=1900, ProcessingMethod=ProcessingMethod.Natural,  Variety="Longberry",Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="blueberry muffin,cardamom,winey" },
            new() { Id=6,  Name="Bench Maji Washed",          Roaster="Stumptown",           OriginCountry="Ethiopia", OriginRegion="Bench Maji",   ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Washed,   Variety="Heirloom", Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="citrus,stone fruit,floral,clean" },
            // ─── Kenyan (7–10) ───
            new() { Id=7,  Name="Nyeri AA",                   Roaster="Verve",               OriginCountry="Kenya",    OriginRegion="Nyeri",        ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Washed,   Variety="SL28",     Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="blackcurrant,tomato,brown sugar,juicy" },
            new() { Id=8,  Name="Kirinyaga AB",               Roaster="Ritual",              OriginCountry="Kenya",    OriginRegion="Kirinyaga",    ElevationMasl=1750, ProcessingMethod=ProcessingMethod.Washed,   Variety="SL34",     Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="raspberry,hibiscus,grapefruit,bright" },
            new() { Id=9,  Name="Kiambu PB",                  Roaster="Chromatic",           OriginCountry="Kenya",    OriginRegion="Kiambu",       ElevationMasl=1650, ProcessingMethod=ProcessingMethod.Washed,   Variety="SL28/34",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="red apple,toffee,citrus peel" },
            new() { Id=10, Name="Thika AA Washed",            Roaster="Sightglass",          OriginCountry="Kenya",    OriginRegion="Thika",        ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Ruiru 11", Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="black tea,lemon curd,subtle berry" },
            // ─── Colombian (11–15) ───
            new() { Id=11, Name="Huila El Paraiso Washed",    Roaster="La Cabra",            OriginCountry="Colombia", OriginRegion="Huila",        ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Washed,   Variety="Castillo", Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="red grape,caramel,milk chocolate,smooth" },
            new() { Id=12, Name="Nariño La Palma Washed",     Roaster="Heart",               OriginCountry="Colombia", OriginRegion="Nariño",       ElevationMasl=2000, ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="floral,lime,nectarine,elegant" },
            new() { Id=13, Name="Cauca Popayan Honey",        Roaster="Parlor Coffee",       OriginCountry="Colombia", OriginRegion="Cauca",        ElevationMasl=1750, ProcessingMethod=ProcessingMethod.Honey,    Variety="Caturra",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="apricot,honey,brown sugar,balanced" },
            new() { Id=14, Name="Antioquia Los Andes",        Roaster="Dragonfly",           OriginCountry="Colombia", OriginRegion="Antioquia",    ElevationMasl=1650, ProcessingMethod=ProcessingMethod.Washed,   Variety="Colombia", Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="hazelnut,caramel,mild citrus,easy drinking" },
            new() { Id=15, Name="Tolima Planadas Honey",      Roaster="Madcap",              OriginCountry="Colombia", OriginRegion="Tolima",       ElevationMasl=1900, ProcessingMethod=ProcessingMethod.Honey,    Variety="Bourbon",  Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="peach,guava,light caramel,tropical" },
            // ─── Guatemalan (16–18) ───
            new() { Id=16, Name="Antigua Santa Rosa Washed",  Roaster="Equator",             OriginCountry="Guatemala",OriginRegion="Antigua",      ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Bourbon",  Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="dark chocolate,almond,red apple,classic" },
            new() { Id=17, Name="Huehuetenango Todos Santos", Roaster="PT's",                OriginCountry="Guatemala",OriginRegion="Huehuetenango",ElevationMasl=1900, ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="stone fruit,caramel,walnut,balanced" },
            new() { Id=18, Name="Atitlan Panabaj Washed",     Roaster="Passenger",           OriginCountry="Guatemala",OriginRegion="Atitlan",      ElevationMasl=1600, ProcessingMethod=ProcessingMethod.Washed,   Variety="Caturra",  Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="milk chocolate,vanilla,dried cherry" },
            // ─── Costa Rican (19–21) ───
            new() { Id=19, Name="Tarrazu La Minita",          Roaster="Toby's Estate",       OriginCountry="Costa Rica",OriginRegion="Tarrazu",     ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Caturra",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="bright citrus,brown sugar,clean,classic" },
            new() { Id=20, Name="Tres Rios Dota Honey",       Roaster="Sprudge Select",      OriginCountry="Costa Rica",OriginRegion="Tres Rios",   ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Honey,    Variety="Typica",   Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="mango,honey,peach tea,sweet" },
            new() { Id=21, Name="Brunca San Marcos Natural",  Roaster="Trade",               OriginCountry="Costa Rica",OriginRegion="Brunca",      ElevationMasl=1600, ProcessingMethod=ProcessingMethod.Natural,  Variety="Catuai",   Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="dried fruit,chocolate,nutty" },
            // ─── Panamanian (22–24) ───
            new() { Id=22, Name="Boquete Geisha Washed",      Roaster="Proud Mary",          OriginCountry="Panama",   OriginRegion="Boquete",      ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Geisha",   Species=Species.Arabica, RoastLevel=1.5m, TastingNotes="jasmine,bergamot,white peach,extraordinary" },
            new() { Id=23, Name="Volcan Geisha Natural",      Roaster="Cat & Cloud",         OriginCountry="Panama",   OriginRegion="Volcan",       ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Natural,  Variety="Geisha",   Species=Species.Arabica, RoastLevel=1.5m, TastingNotes="tropical fruit,floral,complex,wine-like" },
            new() { Id=24, Name="Chiriqui Caturra Washed",    Roaster="Coava",               OriginCountry="Panama",   OriginRegion="Chiriqui",     ElevationMasl=1600, ProcessingMethod=ProcessingMethod.Washed,   Variety="Caturra",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="melon,green grape,light body,refreshing" },
            // ─── Brazilian (25–28) ───
            new() { Id=25, Name="Sul de Minas Natural",       Roaster="Nossa Familia",       OriginCountry="Brazil",   OriginRegion="Sul de Minas", ElevationMasl=1200, ProcessingMethod=ProcessingMethod.Natural,  Variety="Mundo Novo",Species=Species.Arabica, RoastLevel=3.5m, TastingNotes="dark chocolate,hazelnut,caramel,low acid" },
            new() { Id=26, Name="Cerrado Mineiro Natural",    Roaster="Kuma",                OriginCountry="Brazil",   OriginRegion="Cerrado",      ElevationMasl=1100, ProcessingMethod=ProcessingMethod.Natural,  Variety="Catuai",   Species=Species.Arabica, RoastLevel=4.0m, TastingNotes="bittersweet chocolate,almond,roasted,full body" },
            new() { Id=27, Name="Serra da Mantiqueira Bourbon",Roaster="Café Grumpy",        OriginCountry="Brazil",   OriginRegion="Mantiqueira",  ElevationMasl=1300, ProcessingMethod=ProcessingMethod.Honey,    Variety="Yellow Bourbon",Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="butterscotch,peach,light chocolate,smooth" },
            new() { Id=28, Name="Mogiana Natural Dark",       Roaster="Black & White",       OriginCountry="Brazil",   OriginRegion="Mogiana",      ElevationMasl=1050, ProcessingMethod=ProcessingMethod.Natural,  Variety="Icatu",    Species=Species.Arabica, RoastLevel=4.5m, TastingNotes="dark chocolate,tobacco,syrupy,bold" },
            // ─── Peruvian (29–30) ───
            new() { Id=29, Name="Chanchamayo Junin Washed",   Roaster="Dogwood",             OriginCountry="Peru",     OriginRegion="Chanchamayo",  ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="citrus,brown sugar,mild,clean" },
            new() { Id=30, Name="Cajamarca Jaen Washed",      Roaster="Equator",             OriginCountry="Peru",     OriginRegion="Cajamarca",    ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Bourbon",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="orange blossom,honey,dried apricot" },
            // ─── Honduran (31–32) ───
            new() { Id=31, Name="Copan Marcala Washed",       Roaster="Olympia",             OriginCountry="Honduras", OriginRegion="Copan",        ElevationMasl=1600, ProcessingMethod=ProcessingMethod.Washed,   Variety="Lempira",  Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="brown sugar,apple,chocolate,balanced" },
            new() { Id=32, Name="Comayagua Santa Barbara Honey",Roaster="Sip of Hope",       OriginCountry="Honduras", OriginRegion="Comayagua",    ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Honey,    Variety="IHCAFE-90",Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="honey,peach,chocolate,sweet" },
            // ─── Salvadoran (33–34) ───
            new() { Id=33, Name="Santa Ana Pacamara Natural", Roaster="Temple",              OriginCountry="El Salvador",OriginRegion="Santa Ana",   ElevationMasl=1600, ProcessingMethod=ProcessingMethod.Natural,  Variety="Pacamara", Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="cantaloupe,tropical,wine,unique" },
            new() { Id=34, Name="Ahuachapan Bourbon Honey",   Roaster="Drip Bar",            OriginCountry="El Salvador",OriginRegion="Ahuachapan",  ElevationMasl=1500, ProcessingMethod=ProcessingMethod.Honey,    Variety="Bourbon",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="caramel,orange peel,milk chocolate" },
            // ─── Indonesian (35–38) ───
            new() { Id=35, Name="Sumatra Mandheling",         Roaster="Peet's",              OriginCountry="Indonesia",OriginRegion="North Sumatra", ElevationMasl=1300, ProcessingMethod=ProcessingMethod.WetHulled,Variety="Typica",   Species=Species.Arabica, RoastLevel=4.0m, TastingNotes="earthy,cedar,dark chocolate,full body,low acid" },
            new() { Id=36, Name="Sumatra Lintong Blue Batak", Roaster="Volcanica",           OriginCountry="Indonesia",OriginRegion="Lintong",       ElevationMasl=1400, ProcessingMethod=ProcessingMethod.WetHulled,Variety="Lintong",  Species=Species.Arabica, RoastLevel=3.5m, TastingNotes="earthy,dark cherry,cardamom,complex" },
            new() { Id=37, Name="Java Blawan Estate",         Roaster="Black Dog",           OriginCountry="Indonesia",OriginRegion="East Java",     ElevationMasl=1400, ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="tobacco,dark chocolate,herbal,smooth" },
            new() { Id=38, Name="Flores Bajawa Washed",       Roaster="Ceremony",            OriginCountry="Indonesia",OriginRegion="Flores",        ElevationMasl=1200, ProcessingMethod=ProcessingMethod.WetHulled,Variety="Typica",   Species=Species.Arabica, RoastLevel=3.5m, TastingNotes="brown sugar,earthy,dried fruit,medium body" },
            // ─── Rwandan (39–40) ───
            new() { Id=39, Name="Nyungwe Red Bourbon Washed", Roaster="Koppi",               OriginCountry="Rwanda",   OriginRegion="Nyungwe",      ElevationMasl=1900, ProcessingMethod=ProcessingMethod.Washed,   Variety="Red Bourbon",Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="pomegranate,hibiscus,stone fruit,elegant" },
            new() { Id=40, Name="Kigali Muraho Natural",      Roaster="Onyx Coffee Lab",     OriginCountry="Rwanda",   OriginRegion="Kigali",       ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Natural,  Variety="Heirloom", Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="strawberry jam,vanilla,brown sugar,lush" },
            // ─── Burundian (41–42) ───
            new() { Id=41, Name="Kibira Natural",             Roaster="Matchbook",           OriginCountry="Burundi",  OriginRegion="Kibira",       ElevationMasl=1900, ProcessingMethod=ProcessingMethod.Natural,  Variety="Red Bourbon",Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="raspberry,cranberry,creamy,bright" },
            new() { Id=42, Name="Ngozi Washed",               Roaster="Greater Goods",       OriginCountry="Burundi",  OriginRegion="Ngozi",        ElevationMasl=1850, ProcessingMethod=ProcessingMethod.Washed,   Variety="Jackson",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="black tea,apricot,dark cherry,clean" },
            // ─── Mexican (43–44) ───
            new() { Id=43, Name="Oaxaca Pluma Washed",        Roaster="Verve",               OriginCountry="Mexico",   OriginRegion="Oaxaca",       ElevationMasl=1400, ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="cocoa,mild citrus,smooth,approachable" },
            new() { Id=44, Name="Chiapas San Cristobal Washed",Roaster="Onyx Coffee Lab",    OriginCountry="Mexico",   OriginRegion="Chiapas",      ElevationMasl=1500, ProcessingMethod=ProcessingMethod.Washed,   Variety="Bourbon",  Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="hazelnut,caramel,orange,soft" },
            // ─── Yemeni (45) ───
            new() { Id=45, Name="Yemen Mocha Matari Natural", Roaster="Klatch",              OriginCountry="Yemen",    OriginRegion="Bani Matar",   ElevationMasl=1800, ProcessingMethod=ProcessingMethod.Natural,  Variety="Dawairi",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="wild berry,cardamom,earthy,ancient character" },
            // ─── Bolivian (46) ───
            new() { Id=46, Name="Caranavi Typica Washed",     Roaster="Ninety Plus",         OriginCountry="Bolivia",  OriginRegion="Caranavi",     ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="stone fruit,floral,crisp,clean" },
            // ─── Papua New Guinean (47) ───
            new() { Id=47, Name="Wahgi Valley Washed",        Roaster="Blue Bottle",         OriginCountry="Papua New Guinea",OriginRegion="Wahgi Valley",ElevationMasl=1600,ProcessingMethod=ProcessingMethod.Washed,  Variety="Typica/Arusha",Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="brown sugar,dried mango,earthy,medium body" },
            // ─── Nicaraguan (48–49) ───
            new() { Id=48, Name="Matagalpa Selva Negra Washed",Roaster="Intelligentsia",     OriginCountry="Nicaragua",OriginRegion="Matagalpa",    ElevationMasl=1400, ProcessingMethod=ProcessingMethod.Washed,   Variety="Caturra",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="chocolate,nuts,mild citrus,approachable" },
            new() { Id=49, Name="Jinotega San Ramon Natural", Roaster="Merit",               OriginCountry="Nicaragua",OriginRegion="Jinotega",     ElevationMasl=1500, ProcessingMethod=ProcessingMethod.Natural,  Variety="Catimor",  Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="dried cherry,cocoa,mild,sweet" },
            // ─── Vietnamese (50) ───
            new() { Id=50, Name="Da Lat Arabica Washed",      Roaster="K'Ho Coffee",         OriginCountry="Vietnam",  OriginRegion="Da Lat",       ElevationMasl=1500, ProcessingMethod=ProcessingMethod.Washed,   Variety="Catimor",  Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="mild,caramel,light chocolate,smooth" },
            // ─── Jamaican (51) ───
            new() { Id=51, Name="Blue Mountain No. 1",        Roaster="Wallenford Estate",   OriginCountry="Jamaica",  OriginRegion="Blue Mountains",ElevationMasl=1500, ProcessingMethod=ProcessingMethod.Washed,  Variety="Typica",   Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="mild,sweet,nutty,clean,balanced" },
            // ─── Hawaiian (52) ───
            new() { Id=52, Name="Kona Extra Fancy",           Roaster="Greenwell Farms",     OriginCountry="USA",      OriginRegion="Kona",         ElevationMasl=700,  ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="macadamia,light caramel,mild,smooth" },
            // ─── Zimbabwean (53) ───
            new() { Id=53, Name="Chipinge AA Washed",         Roaster="Torch & Crown",       OriginCountry="Zimbabwe", OriginRegion="Chipinge",     ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Bourbon",  Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="citrus,mild berry,clean,medium body" },
            // ─── Tanzanian (54) ───
            new() { Id=54, Name="Kilimanjaro Peaberry",       Roaster="Barrington",          OriginCountry="Tanzania", OriginRegion="Kilimanjaro",  ElevationMasl=1700, ProcessingMethod=ProcessingMethod.Washed,   Variety="Kent",     Species=Species.Arabica, RoastLevel=2.5m, TastingNotes="bright,citrus,mild chocolate,clean" },
            // ─── Ecuadorian (55) ───
            new() { Id=55, Name="Loja Vilcabamba Washed",     Roaster="Devoción",            OriginCountry="Ecuador",  OriginRegion="Loja",         ElevationMasl=1900, ProcessingMethod=ProcessingMethod.Washed,   Variety="Typica",   Species=Species.Arabica, RoastLevel=2.0m, TastingNotes="floral,citrus,stone fruit,elegant" },
            // ─── Dominican (56) ───
            new() { Id=56, Name="Barahona Cibao Washed",      Roaster="Irving Farm",         OriginCountry="Dominican Republic",OriginRegion="Barahona",ElevationMasl=1200,ProcessingMethod=ProcessingMethod.Washed,Variety="Typica",   Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="chocolate,mild,nutty,balanced" },
            // ─── Chinese (57) ───
            new() { Id=57, Name="Yunnan Bourbon Washed",      Roaster="Sightglass",          OriginCountry="China",    OriginRegion="Yunnan",       ElevationMasl=1600, ProcessingMethod=ProcessingMethod.Washed,   Variety="Bourbon",  Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="brown sugar,stone fruit,mild,smooth" },
            // ─── Indian (58–59) ───
            new() { Id=58, Name="Bababudangiri Arabica Natural",Roaster="Counter Culture",   OriginCountry="India",    OriginRegion="Chikmagalur",  ElevationMasl=1600, ProcessingMethod=ProcessingMethod.Natural,  Variety="Kent",     Species=Species.Arabica, RoastLevel=3.0m, TastingNotes="spice,chocolate,earthy,medium body" },
            new() { Id=59, Name="Mysore Nuggets Washed",      Roaster="Equator",             OriginCountry="India",    OriginRegion="Mysore",       ElevationMasl=1200, ProcessingMethod=ProcessingMethod.Washed,   Variety="Selections 5b",Species=Species.Arabica, RoastLevel=3.5m, TastingNotes="dark chocolate,earthy,heavy body,low acid" },
            // ─── Blend (60) ───
            new() { Id=60, Name="House Espresso Blend",       Roaster="Onyx Coffee Lab",     OriginCountry="Blend",    OriginRegion="Blend",        ElevationMasl=null, ProcessingMethod=ProcessingMethod.Other,    Variety="Mixed",    Species=Species.Arabica, RoastLevel=3.5m, IsBlend=true, TastingNotes="dark chocolate,caramel,walnut,full body,classic espresso" },
        };
        ctx.Coffees.AddRange(coffees);

        // ── GRIND LOGS (~100) ────────────────────────────────────────────────
        // NGI values are pre-computed via linear interpolation from calibrations above
        // Format: CoffeeId, GrinderId, BrewMethod, NativeSetting, NgiNormalized, Rating
        var logs = new List<GrindLog>
        {
            // ── Comandante C40 + PourOver (calibration: 8→12, 25→50, 35→82) ──
            new() { Id=1,  CoffeeId=1,  GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=22m,  NgiNormalized=45.7m, Rating=5, Notes="Clean and floral, perfect extraction" },
            new() { Id=2,  CoffeeId=2,  GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=24m,  NgiNormalized=48.6m, Rating=4, Notes="Great fruity sweetness" },
            new() { Id=3,  CoffeeId=3,  GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=23m,  NgiNormalized=47.1m, Rating=4, Notes="Peach notes pronounced" },
            new() { Id=4,  CoffeeId=4,  GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=21m,  NgiNormalized=44.3m, Rating=5, Notes="Incredible complexity" },
            new() { Id=5,  CoffeeId=7,  GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=22m,  NgiNormalized=45.7m, Rating=5, Notes="Blackcurrant really pops" },
            new() { Id=6,  CoffeeId=11, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=26m,  NgiNormalized=51.4m, Rating=4, Notes="Smooth and balanced" },
            new() { Id=7,  CoffeeId=12, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=24m,  NgiNormalized=48.6m, Rating=5, Notes="Floral and lime, stunning" },
            new() { Id=8,  CoffeeId=16, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=27m,  NgiNormalized=52.9m, Rating=4, Notes="Chocolate and fruit balance" },
            new() { Id=9,  CoffeeId=17, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=24m,  NgiNormalized=48.6m, Rating=5, Notes="Stone fruit shines" },
            new() { Id=10, CoffeeId=19, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=26m,  NgiNormalized=51.4m, Rating=5, Notes="Classic bright Tarrazu" },
            new() { Id=11, CoffeeId=20, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=25m,  NgiNormalized=50.0m, Rating=4, Notes="Mango and honey" },
            new() { Id=12, CoffeeId=22, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=21m,  NgiNormalized=44.3m, Rating=5, Notes="Jasmine and white peach – wow" },
            new() { Id=13, CoffeeId=23, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=22m,  NgiNormalized=45.7m, Rating=5, Notes="Tropical and wine-like" },
            new() { Id=14, CoffeeId=25, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=28m,  NgiNormalized=54.3m, Rating=3, Notes="Low acid, not my fave for V60" },
            new() { Id=15, CoffeeId=35, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=30m,  NgiNormalized=57.1m, Rating=4, Notes="Earthy, interesting" },
            new() { Id=16, CoffeeId=39, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=22m,  NgiNormalized=45.7m, Rating=5, Notes="Pomegranate acidity is beautiful" },
            new() { Id=17, CoffeeId=41, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=23m,  NgiNormalized=47.1m, Rating=5, Notes="Raspberry and cream" },
            new() { Id=18, CoffeeId=13, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=25m,  NgiNormalized=50.0m, Rating=4, Notes="Honey process sweetness" },
            new() { Id=19, CoffeeId=15, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=24m,  NgiNormalized=48.6m, Rating=4, Notes="Tropical guava notes" },
            new() { Id=20, CoffeeId=29, GrinderId=1, BrewMethod=BrewMethod.PourOver, NativeSetting=26m,  NgiNormalized=51.4m, Rating=4, Notes="Clean and citrusy" },
            // ── Comandante C40 + Espresso ──
            new() { Id=21, CoffeeId=60, GrinderId=1, BrewMethod=BrewMethod.Espresso, NativeSetting=8m,   NgiNormalized=12.0m, Rating=3, Notes="Works but not ideal" },
            new() { Id=22, CoffeeId=25, GrinderId=1, BrewMethod=BrewMethod.Espresso, NativeSetting=9m,   NgiNormalized=13.8m, Rating=3, Notes="Surprisingly decent for espresso" },
            // ── Comandante C40 + FrenchPress ──
            new() { Id=23, CoffeeId=25, GrinderId=1, BrewMethod=BrewMethod.FrenchPress, NativeSetting=34m, NgiNormalized=83.4m, Rating=4, Notes="Chocolatey and full" },
            new() { Id=24, CoffeeId=35, GrinderId=1, BrewMethod=BrewMethod.FrenchPress, NativeSetting=35m, NgiNormalized=82.0m, Rating=4, Notes="Earthy and great" },
            new() { Id=25, CoffeeId=26, GrinderId=1, BrewMethod=BrewMethod.FrenchPress, NativeSetting=33m, NgiNormalized=80.6m, Rating=3, Notes="A bit muddy" },

            // ── Baratza Encore + PourOver (calibration: 5→10, 20→50, 32→82) ──
            new() { Id=26, CoffeeId=1,  GrinderId=2, BrewMethod=BrewMethod.PourOver, NativeSetting=17m,  NgiNormalized=46.7m, Rating=5, Notes="Bright floral cup" },
            new() { Id=27, CoffeeId=7,  GrinderId=2, BrewMethod=BrewMethod.PourOver, NativeSetting=17m,  NgiNormalized=46.7m, Rating=4, Notes="Juicy Kenyan profile" },
            new() { Id=28, CoffeeId=11, GrinderId=2, BrewMethod=BrewMethod.PourOver, NativeSetting=20m,  NgiNormalized=50.0m, Rating=4, Notes="Balanced and smooth" },
            new() { Id=29, CoffeeId=16, GrinderId=2, BrewMethod=BrewMethod.PourOver, NativeSetting=21m,  NgiNormalized=51.7m, Rating=4, Notes="Classic Antigua" },
            new() { Id=30, CoffeeId=25, GrinderId=2, BrewMethod=BrewMethod.PourOver, NativeSetting=23m,  NgiNormalized=55.0m, Rating=3, Notes="Too heavy for V60" },
            new() { Id=31, CoffeeId=35, GrinderId=2, BrewMethod=BrewMethod.PourOver, NativeSetting=25m,  NgiNormalized=58.3m, Rating=4, Notes="Earthy and bold" },
            new() { Id=32, CoffeeId=22, GrinderId=2, BrewMethod=BrewMethod.PourOver, NativeSetting=16m,  NgiNormalized=45.0m, Rating=5, Notes="Geisha brilliance" },
            // ── Baratza Encore + Espresso ──
            new() { Id=33, CoffeeId=60, GrinderId=2, BrewMethod=BrewMethod.Espresso, NativeSetting=5m,   NgiNormalized=10.0m, Rating=4, Notes="Good all-day espresso" },
            new() { Id=34, CoffeeId=26, GrinderId=2, BrewMethod=BrewMethod.Espresso, NativeSetting=4m,   NgiNormalized=8.3m,  Rating=4, Notes="Dark roast shoots well" },
            new() { Id=35, CoffeeId=28, GrinderId=2, BrewMethod=BrewMethod.Espresso, NativeSetting=4m,   NgiNormalized=8.3m,  Rating=4, Notes="Bold and syrupy" },
            // ── Baratza Encore + FrenchPress ──
            new() { Id=36, CoffeeId=25, GrinderId=2, BrewMethod=BrewMethod.FrenchPress, NativeSetting=30m, NgiNormalized=82.0m, Rating=4, Notes="Chocolatey and full" },
            new() { Id=37, CoffeeId=35, GrinderId=2, BrewMethod=BrewMethod.FrenchPress, NativeSetting=32m, NgiNormalized=82.0m, Rating=5, Notes="Classic Indonesian press" },
            new() { Id=38, CoffeeId=35, GrinderId=2, BrewMethod=BrewMethod.ColdBrew,    NativeSetting=38m, NgiNormalized=90.0m, Rating=5, Notes="Incredible cold brew" },

            // ── Fellow Ode Gen 2 + PourOver (calibration: 3.5→35, 5.0→48, 6.0→55) ──
            new() { Id=39, CoffeeId=1,  GrinderId=3, BrewMethod=BrewMethod.PourOver, NativeSetting=4.5m, NgiNormalized=44.6m, Rating=5, Notes="Flat burr clarity on Ethiopian" },
            new() { Id=40, CoffeeId=22, GrinderId=3, BrewMethod=BrewMethod.PourOver, NativeSetting=4.5m, NgiNormalized=44.6m, Rating=5, Notes="Geisha sparkles on flat burrs" },
            new() { Id=41, CoffeeId=7,  GrinderId=3, BrewMethod=BrewMethod.PourOver, NativeSetting=4.8m, NgiNormalized=47.5m, Rating=5, Notes="Kenyan on flat = outstanding" },
            new() { Id=42, CoffeeId=11, GrinderId=3, BrewMethod=BrewMethod.PourOver, NativeSetting=5.2m, NgiNormalized=50.8m, Rating=4, Notes="Colombian works well" },
            new() { Id=43, CoffeeId=25, GrinderId=3, BrewMethod=BrewMethod.PourOver, NativeSetting=5.8m, NgiNormalized=56.3m, Rating=3, Notes="Brazilian a bit flat for this" },
            new() { Id=44, CoffeeId=39, GrinderId=3, BrewMethod=BrewMethod.PourOver, NativeSetting=4.6m, NgiNormalized=45.5m, Rating=5, Notes="Rwandan brightness" },
            // ── Fellow Ode + Chemex ──
            new() { Id=45, CoffeeId=1,  GrinderId=3, BrewMethod=BrewMethod.Chemex,   NativeSetting=5.5m, NgiNormalized=52.5m, Rating=5, Notes="Perfect Chemex" },
            new() { Id=46, CoffeeId=7,  GrinderId=3, BrewMethod=BrewMethod.Chemex,   NativeSetting=5.5m, NgiNormalized=52.5m, Rating=4, Notes="Good Kenyan Chemex" },

            // ── 1Zpresso JX-Pro + PourOver (calibration: 15→12, 50→50, 75→82) ──
            new() { Id=47, CoffeeId=1,  GrinderId=4, BrewMethod=BrewMethod.PourOver, NativeSetting=46m,  NgiNormalized=45.8m, Rating=5, Notes="Even extraction, floral" },
            new() { Id=48, CoffeeId=4,  GrinderId=4, BrewMethod=BrewMethod.PourOver, NativeSetting=45m,  NgiNormalized=44.6m, Rating=5, Notes="Dense Ethiopian shines" },
            new() { Id=49, CoffeeId=8,  GrinderId=4, BrewMethod=BrewMethod.PourOver, NativeSetting=47m,  NgiNormalized=46.9m, Rating=4, Notes="Kirinyaga brightness" },
            new() { Id=50, CoffeeId=22, GrinderId=4, BrewMethod=BrewMethod.PourOver, NativeSetting=44m,  NgiNormalized=43.4m, Rating=5, Notes="Best Geisha I've pulled" },
            new() { Id=51, CoffeeId=12, GrinderId=4, BrewMethod=BrewMethod.PourOver, NativeSetting=48m,  NgiNormalized=48.1m, Rating=4, Notes="Floral Colombian" },
            new() { Id=52, CoffeeId=20, GrinderId=4, BrewMethod=BrewMethod.PourOver, NativeSetting=47m,  NgiNormalized=46.9m, Rating=4, Notes="Honey sweetness" },
            // ── 1Zpresso JX-Pro + FrenchPress ──
            new() { Id=53, CoffeeId=25, GrinderId=4, BrewMethod=BrewMethod.FrenchPress, NativeSetting=72m, NgiNormalized=79.4m, Rating=4, Notes="Full body" },
            new() { Id=54, CoffeeId=35, GrinderId=4, BrewMethod=BrewMethod.FrenchPress, NativeSetting=75m, NgiNormalized=82.0m, Rating=4, Notes="Earthy Indonesian" },

            // ── Niche Zero + PourOver (calibration: 10→10, 28→48, 42→82) ──
            new() { Id=55, CoffeeId=1,  GrinderId=5, BrewMethod=BrewMethod.PourOver, NativeSetting=27m,  NgiNormalized=46.7m, Rating=5, Notes="Incredible clarity" },
            new() { Id=56, CoffeeId=7,  GrinderId=5, BrewMethod=BrewMethod.PourOver, NativeSetting=26m,  NgiNormalized=45.1m, Rating=5, Notes="Kenyan blackcurrant" },
            new() { Id=57, CoffeeId=22, GrinderId=5, BrewMethod=BrewMethod.PourOver, NativeSetting=25m,  NgiNormalized=43.5m, Rating=5, Notes="Geisha at its best" },
            new() { Id=58, CoffeeId=39, GrinderId=5, BrewMethod=BrewMethod.PourOver, NativeSetting=26m,  NgiNormalized=45.1m, Rating=5, Notes="Rwanda hibiscus clarity" },
            // ── Niche Zero + Espresso ──
            new() { Id=59, CoffeeId=60, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=12m,  NgiNormalized=12.2m, Rating=5, Notes="Sweet all-day shot" },
            new() { Id=60, CoffeeId=11, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=16m,  NgiNormalized=17.2m, Rating=4, Notes="Light roast espresso needs coarser" },
            new() { Id=61, CoffeeId=16, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=13m,  NgiNormalized=13.8m, Rating=4, Notes="Chocolate shot" },
            new() { Id=62, CoffeeId=25, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=10m,  NgiNormalized=10.0m, Rating=5, Notes="Classic Brazilian espresso" },
            new() { Id=63, CoffeeId=26, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=9m,   NgiNormalized=9.0m,  Rating=5, Notes="Bold and intense" },
            new() { Id=64, CoffeeId=35, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=11m,  NgiNormalized=11.1m, Rating=4, Notes="Earthy Sumatran shot" },
            new() { Id=65, CoffeeId=28, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=8m,   NgiNormalized=8.0m,  Rating=4, Notes="Syrupy Mogiana" },
            new() { Id=66, CoffeeId=27, GrinderId=5, BrewMethod=BrewMethod.Espresso, NativeSetting=11m,  NgiNormalized=11.1m, Rating=4, Notes="Butterscotch shot" },

            // ── Eureka Mignon Specialita + Espresso (calibration: 1.5→10, 3.8→50) ──
            new() { Id=67, CoffeeId=60, GrinderId=6, BrewMethod=BrewMethod.Espresso, NativeSetting=1.6m, NgiNormalized=10.9m, Rating=5, Notes="Dialed in perfectly" },
            new() { Id=68, CoffeeId=25, GrinderId=6, BrewMethod=BrewMethod.Espresso, NativeSetting=1.5m, NgiNormalized=10.0m, Rating=5, Notes="Brazilian sweetness" },
            new() { Id=69, CoffeeId=26, GrinderId=6, BrewMethod=BrewMethod.Espresso, NativeSetting=1.3m, NgiNormalized=8.3m,  Rating=5, Notes="Full and bold" },
            new() { Id=70, CoffeeId=11, GrinderId=6, BrewMethod=BrewMethod.Espresso, NativeSetting=2.2m, NgiNormalized=17.4m, Rating=4, Notes="Light Huila for espresso" },
            new() { Id=71, CoffeeId=16, GrinderId=6, BrewMethod=BrewMethod.Espresso, NativeSetting=1.8m, NgiNormalized=13.0m, Rating=4, Notes="Chocolate and almond" },
            new() { Id=72, CoffeeId=28, GrinderId=6, BrewMethod=BrewMethod.Espresso, NativeSetting=1.2m, NgiNormalized=7.4m,  Rating=4, Notes="Very bold and syrupy" },
            // ── Eureka Mignon Specialita + PourOver ──
            new() { Id=73, CoffeeId=1,  GrinderId=6, BrewMethod=BrewMethod.PourOver, NativeSetting=3.8m, NgiNormalized=50.0m, Rating=5, Notes="Flat burrs on Ethiopian" },
            new() { Id=74, CoffeeId=7,  GrinderId=6, BrewMethod=BrewMethod.PourOver, NativeSetting=3.7m, NgiNormalized=49.1m, Rating=4, Notes="Kenyan on Specialita" },

            // ── Timemore C3 Pro + PourOver (calibration: 6→12, 18→50, 26→82) ──
            new() { Id=75, CoffeeId=1,  GrinderId=7, BrewMethod=BrewMethod.PourOver, NativeSetting=17m,  NgiNormalized=47.0m, Rating=4, Notes="Good budget grind" },
            new() { Id=76, CoffeeId=4,  GrinderId=7, BrewMethod=BrewMethod.PourOver, NativeSetting=16m,  NgiNormalized=44.3m, Rating=5, Notes="Guji pops even here" },
            new() { Id=77, CoffeeId=11, GrinderId=7, BrewMethod=BrewMethod.PourOver, NativeSetting=19m,  NgiNormalized=52.7m, Rating=4, Notes="Solid Colombian" },
            new() { Id=78, CoffeeId=16, GrinderId=7, BrewMethod=BrewMethod.PourOver, NativeSetting=20m,  NgiNormalized=55.5m, Rating=3, Notes="A bit coarse for this" },
            new() { Id=79, CoffeeId=35, GrinderId=7, BrewMethod=BrewMethod.PourOver, NativeSetting=22m,  NgiNormalized=61.1m, Rating=3, Notes="Earthy and muddy" },
            new() { Id=80, CoffeeId=39, GrinderId=7, BrewMethod=BrewMethod.PourOver, NativeSetting=17m,  NgiNormalized=47.0m, Rating=4, Notes="Rwanda on budget grinder" },
            // ── Timemore C3 Pro + MokaPot ──
            new() { Id=81, CoffeeId=25, GrinderId=7, BrewMethod=BrewMethod.MokaPot, NativeSetting=8m,   NgiNormalized=24.1m, Rating=4, Notes="Brazilian moka" },
            new() { Id=82, CoffeeId=26, GrinderId=7, BrewMethod=BrewMethod.MokaPot, NativeSetting=7m,   NgiNormalized=20.9m, Rating=4, Notes="Dark roast moka" },
            new() { Id=83, CoffeeId=60, GrinderId=7, BrewMethod=BrewMethod.MokaPot, NativeSetting=8m,   NgiNormalized=24.1m, Rating=4, Notes="House blend in moka" },

            // ── Hario Skerton Pro + PourOver (calibration: 0.8→32, 2.0→45, 3.5→80) ──
            new() { Id=84, CoffeeId=1,  GrinderId=8, BrewMethod=BrewMethod.PourOver, NativeSetting=1.8m, NgiNormalized=43.5m, Rating=4, Notes="Not bad for hand grinder" },
            new() { Id=85, CoffeeId=11, GrinderId=8, BrewMethod=BrewMethod.PourOver, NativeSetting=2.2m, NgiNormalized=47.1m, Rating=3, Notes="Inconsistent grind" },
            new() { Id=86, CoffeeId=25, GrinderId=8, BrewMethod=BrewMethod.PourOver, NativeSetting=2.5m, NgiNormalized=49.8m, Rating=3, Notes="Low acid helps Hario" },
            // ── Hario Skerton + AeroPress ──
            new() { Id=87, CoffeeId=1,  GrinderId=8, BrewMethod=BrewMethod.AeropressCoarse, NativeSetting=1.5m, NgiNormalized=40.6m, Rating=4, Notes="Nice inverted AeroPress" },
            new() { Id=88, CoffeeId=11, GrinderId=8, BrewMethod=BrewMethod.AeropressCoarse, NativeSetting=1.8m, NgiNormalized=43.5m, Rating=3, Notes="Decent" },
            // ── Hario Skerton + FrenchPress ──
            new() { Id=89, CoffeeId=35, GrinderId=8, BrewMethod=BrewMethod.FrenchPress, NativeSetting=3.5m, NgiNormalized=80.0m, Rating=5, Notes="Perfect Sumatran press" },

            // ── Weber EG-1 Mini + Espresso (calibration: 2.0→10, 5.5→50) ──
            new() { Id=90, CoffeeId=60, GrinderId=9, BrewMethod=BrewMethod.Espresso, NativeSetting=2.8m, NgiNormalized=12.9m, Rating=5, Notes="Weber precision espresso" },
            new() { Id=91, CoffeeId=25, GrinderId=9, BrewMethod=BrewMethod.Espresso, NativeSetting=3.0m, NgiNormalized=14.3m, Rating=5, Notes="Brazilian on Weber" },
            new() { Id=92, CoffeeId=26, GrinderId=9, BrewMethod=BrewMethod.Espresso, NativeSetting=2.5m, NgiNormalized=12.1m, Rating=5, Notes="Dark roast espresso" },
            new() { Id=93, CoffeeId=11, GrinderId=9, BrewMethod=BrewMethod.Espresso, NativeSetting=3.5m, NgiNormalized=17.1m, Rating=4, Notes="Colombian light on Weber" },
            new() { Id=94, CoffeeId=28, GrinderId=9, BrewMethod=BrewMethod.Espresso, NativeSetting=2.3m, NgiNormalized=11.4m, Rating=4, Notes="Syrupy Mogiana on Weber" },
            // ── Weber EG-1 Mini + PourOver ──
            new() { Id=95, CoffeeId=1,  GrinderId=9, BrewMethod=BrewMethod.PourOver, NativeSetting=5.3m, NgiNormalized=47.7m, Rating=5, Notes="Exceptional Ethiopian" },
            new() { Id=96, CoffeeId=7,  GrinderId=9, BrewMethod=BrewMethod.PourOver, NativeSetting=5.2m, NgiNormalized=46.9m, Rating=5, Notes="Kenyan brilliance on Weber" },

            // ── Baratza Vario-W + Espresso (calibration: 2.0→12, 6.0→50) ──
            new() { Id=97,  CoffeeId=25, GrinderId=10,BrewMethod=BrewMethod.Espresso, NativeSetting=2.5m, NgiNormalized=14.7m, Rating=4, Notes="Brazilian on Vario" },
            new() { Id=98,  CoffeeId=60, GrinderId=10,BrewMethod=BrewMethod.Espresso, NativeSetting=2.2m, NgiNormalized=13.2m, Rating=5, Notes="House blend dialed" },
            new() { Id=99,  CoffeeId=26, GrinderId=10,BrewMethod=BrewMethod.Espresso, NativeSetting=2.0m, NgiNormalized=12.0m, Rating=4, Notes="Cerrado on Vario" },
            new() { Id=100, CoffeeId=16, GrinderId=10,BrewMethod=BrewMethod.Espresso, NativeSetting=2.8m, NgiNormalized=16.3m, Rating=3, Notes="Too light for espresso" },
            // ── Baratza Vario-W + PourOver ──
            new() { Id=101, CoffeeId=1,  GrinderId=10,BrewMethod=BrewMethod.PourOver, NativeSetting=5.8m, NgiNormalized=47.6m, Rating=4, Notes="Flat burr Ethiopian" },
            new() { Id=102, CoffeeId=39, GrinderId=10,BrewMethod=BrewMethod.PourOver, NativeSetting=5.6m, NgiNormalized=46.2m, Rating=5, Notes="Rwanda clarity" },
        };
        ctx.GrindLogs.AddRange(logs);
        ctx.SaveChanges();
    }
}
