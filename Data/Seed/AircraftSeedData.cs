namespace FlightOps.Data.Seed;

public sealed record AircraftSeedRecord(
    string Registration,
    string Name,
    string Model,
    string HomeIata,
    string HangarBay,
    int TakeOffEffort,
    double FuelConsumptionPerKm,
    double CruiseSpeedKmh);

public static class AircraftSeedData
{
    public static IReadOnlyList<AircraftSeedRecord> All { get; } =
    [
        new("CS-TUI", "Porto Atlantic", "Airbus A320neo", "LIS", "T1-H12", 1100, 2.5, 828),
        new("CS-TUK", "Tagus Star", "Airbus A320neo", "LIS", "T1-H14", 1100, 2.5, 828),
        new("CS-TUA", "Algarve Sun", "Airbus A321neo", "FAO", "South-H3", 1250, 2.7, 833),
        new("CS-TVB", "Douro Express", "Boeing 737-800", "OPO", "N-H07", 1200, 2.8, 850),
        new("CS-TVC", "Minho Flyer", "Boeing 737-800", "OPO", "N-H09", 1200, 2.8, 850),
        new("EC-MXY", "Iberia Crest", "Airbus A320ceo", "MAD", "T4-B21", 1150, 2.6, 828),
        new("EC-NXY", "Castilla Air", "Airbus A320neo", "MAD", "T4-B28", 1100, 2.5, 828),
        new("EC-MXZ", "Catalunya Jet", "Airbus A321neo", "BCN", "T1-M05", 1250, 2.7, 833),
        new("G-EZAB", "Thames Hopper", "Airbus A320ceo", "LHR", "T5-C18", 1150, 2.6, 828),
        new("G-EUUP", "Heathrow Link", "Airbus A320neo", "LHR", "T5-C22", 1100, 2.5, 828),
        new("G-EUUQ", "Gatwick Swift", "Boeing 737-800", "LGW", "North-H11", 1200, 2.8, 850),
        new("F-GKXY", "Seine Voyager", "Airbus A320neo", "CDG", "2F-H06", 1100, 2.5, 828),
        new("F-HEPJ", "Orly Connect", "Airbus A320ceo", "ORY", "Sud-H02", 1150, 2.6, 828),
        new("D-AIZA", "Rhein Main", "Airbus A320neo", "FRA", "A-H15", 1100, 2.5, 828),
        new("D-AISB", "Bavaria Sky", "Airbus A321neo", "MUC", "T2-H08", 1250, 2.7, 833),
        new("PH-BXA", "KLM Horizon", "Boeing 737-800", "AMS", "Pier-D12", 1200, 2.8, 850),
        new("OO-SNA", "Brussels Union", "Airbus A320neo", "BRU", "Pier-B04", 1100, 2.5, 828),
        new("HB-JCA", "Alpine Spirit", "Airbus A220-300", "ZRH", "A-H03", 950, 2.2, 829),
        new("OE-LBS", "Danube Wing", "Boeing 737-800", "VIE", "T3-H07", 1200, 2.8, 850),
        new("EI-DVM", "Shamrock One", "Airbus A320ceo", "DUB", "T2-H05", 1150, 2.6, 828),
        new("N12345", "Liberty Star", "Boeing 737-800", "JFK", "T8-H31", 1200, 2.8, 850),
        new("N837DN", "Pacific Runner", "Boeing 737-900ER", "LAX", "T7-H09", 1250, 2.9, 850),
        new("N388UA", "Windy City", "Boeing 737-800", "ORD", "C-H18", 1200, 2.8, 850),
        new("N915NN", "Sunshine Clipper", "Boeing 737-800", "MIA", "North-H06", 1200, 2.8, 850),
        new("C-FRSR", "Maple Leaf", "Boeing 737-800", "YYZ", "T1-H14", 1200, 2.8, 850),
        new("PR-GUO", "Sampa Gold", "Boeing 737-800", "GRU", "T2-H21", 1200, 2.8, 850),
        new("PR-GUP", "Carioca Blue", "Airbus A320neo", "GIG", "T2-H08", 1100, 2.5, 828),
        new("LV-FUB", "Pampa Sur", "Boeing 737-800", "EZE", "T1-H04", 1200, 2.8, 850),
        new("A6-EOA", "Desert Falcon", "Airbus A380-800", "DXB", "Concourse-A12", 1800, 4.2, 900),
        new("A7-BEB", "Pearl Qatar", "Boeing 787-9", "DOH", "South-H06", 1400, 2.4, 913),
        new("9V-SMA", "Lion City", "Airbus A350-900", "SIN", "T3-H10", 1350, 2.3, 903),
        new("B-HNL", "Victoria Harbour", "Airbus A330-300", "HKG", "T1-H16", 1300, 2.8, 871),
        new("JA801A", "Rising Sun", "Boeing 787-9", "HND", "T3-H05", 1400, 2.4, 913),
        new("HL8084", "Han River", "Boeing 737-800", "ICN", "T2-H11", 1200, 2.8, 850),
        new("B-5976", "Capital Express", "Airbus A320neo", "PEK", "T3-H19", 1100, 2.5, 828),
        new("B-6086", "Pudong Pearl", "Airbus A330-300", "PVG", "T2-H07", 1300, 2.8, 871),
        new("VT-ATV", "Delhi Dawn", "Airbus A320neo", "DEL", "T3-H13", 1100, 2.5, 828),
        new("VH-OQA", "Southern Cross", "Airbus A380-800", "SYD", "T1-H02", 1800, 4.2, 900),
        new("VH-VUZ", "Melbourne Mist", "Boeing 737-800", "MEL", "T1-H08", 1200, 2.8, 850),
        new("ZS-SNA", "Joburg Jet", "Airbus A320ceo", "JNB", "T2-H06", 1150, 2.6, 828),
        new("SU-GDN", "Nile Wing", "Boeing 737-800", "CAI", "T3-H04", 1200, 2.8, 850)
    ];
}
