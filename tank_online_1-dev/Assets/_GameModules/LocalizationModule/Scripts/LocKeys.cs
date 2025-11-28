public static class LocKeys
{
    public static class UI_Popup
    {
        public const string UI_Popup_NotEnoughCurrency = "UI_Popup_NotEnoughCurrency";
        public const string UI_Popup_ComingSoon = "UI_Popup_ComingSoon";
    }
    public static class UI_GamePlay
    {
        public static string GetUpgradeNameType(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Damage:
                    return UI_GamePlay_DMG;
                case UpgradeType.FireRate:
                    return UI_GamePlay_FireRate;
                case UpgradeType.MovementSpeed:
                    return UI_GamePlay_MovementSpeed;
                case UpgradeType.MaxHP:
                    return UI_GamePlay_MaxHP;
                default:
                    return "";
            }
        }
        public const string UI_GamePlay_DMG = "UI_GamePlay_DMG";
        public const string UI_GamePlay_FireRate = "UI_GamePlay_FireRate";
        public const string UI_GamePlay_MovementSpeed = "UI_GamePlay_MovementSpeed";
        public const string UI_GamePlay_MaxHP = "UI_GamePlay_MaxHP";
    }
    public static class UI_Garage
    {

    }
    
    public static class UI_GarageDecor
    {
        
    }

    public static class Tank
    {
    }

    public static class UI_Common
    {
    }

    public static class UI_Home
    {
    }

    public static class UI_ChooseTank
    {
        public const string UI_ChooseTank_ProtectOutpost = "UI_ChooseTank_ProtectOutpost";
        public const string UI_ChooseTank_DestroyOutpost = "UI_ChooseTank_DestroyOupost";
        public const string UI_ChooseTank_Defender = "UI_ChooseTank_Defender";
        public const string UI_ChooseTank_Attacker = "UI_ChooseTank_Attacker";
        public const string UI_ChooseTank_KillEnemies = "UI_ChooseTank_KillEnemies";
        public const string UI_ChooseTank_KillOne = "UI_ChooseTank_KillOne";
    }
}