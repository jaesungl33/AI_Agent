public class RandomNameGenerator
{
    private static readonly string[] FirstNames =
    {
        "James","John","Robert","Michael","William","David","Richard","Joseph","Thomas","Charles",
        "Christopher","Daniel","Matthew","Anthony","Mark","Donald","Steven","Paul","Andrew","Joshua",
        "Kenneth","Kevin","Brian","George","Edward","Ronald","Timothy","Jason","Jeffrey","Ryan",
        "Jacob","Gary","Nicholas","Eric","Jonathan","Stephen","Larry","Justin","Scott","Brandon",
        "Benjamin","Samuel","Gregory","Alexander","Frank","Patrick","Raymond","Jack","Dennis","Jerry",
        "Emma","Olivia","Ava","Sophia","Isabella","Mia","Charlotte","Amelia","Evelyn","Abigail",
        "Harper","Emily","Elizabeth","Sofia","Madison","Avery","Ella","Scarlett","Grace","Chloe",
        "Victoria","Riley","Aria","Lily","Aurora","Zoey","Penelope","Nora","Hannah","Lillian",
        "Addison","Eleanor","Stella","Natalie","Zoe","Leah","Hazel","Violet","Aurora","Savannah",
        "Audrey","Brooklyn","Bella","Claire","Skylar","Lucy","Paisley","Everly","Anna","Caroline"
    };

    private static readonly string[] LastNames =
    {
        "Smith","Johnson","Williams","Brown","Jones","Miller","Davis","Garcia","Rodriguez","Wilson",
        "Martinez","Anderson","Taylor","Thomas","Hernandez","Moore","Martin","Jackson","Thompson","White"
    };

    private static readonly System.Random rnd = new System.Random();

    public static string GetRandomEnglishName()
    {
        string first = FirstNames[rnd.Next(FirstNames.Length)];
        // string last = LastNames[rnd.Next(LastNames.Length)];
        return $"{first}";
    }
}
