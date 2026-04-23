namespace AudioKey
{
    public static class Key
    {
        private static string GetPath(string folder, string name) => $"{ROOT}{folder}{name}";
        #region 0. 폴더 경로
        private const string ROOT = "event:/";
        private const string PATH_BGM = "BGM/";
        private const string PATH_SFX = "SFX/";
        private const string PATH_SFXDialogue  = "SFX/DialogueTextSFX/";
        private const string PATH_SFXCombat = "SFX/CombatSFX/";
        private const string PATH_UI = "UI/";
        #endregion

        #region 1. BGM
        public static string BGM_Test => GetPath(PATH_BGM, "Test_BGM01");
        public static string BGM_Lobby_Haderonia => GetPath(PATH_BGM, "Haderonia_LobbyBGM");

        public static string BGM_Boss_JunkKing => GetPath(PATH_BGM, "Haderonia_JunkKingBGM");

        #endregion

        #region 2. SFX


        #region 2-1. Dialogue Text SFX
        public static string SFX_Dialogue_TextSFX_01 => GetPath(PATH_SFXDialogue, "DialogueTextSFX1");
        #endregion

        #region 2-2. Combat SFX
        public static string SFX_Combat_HealPotion => GetPath(PATH_SFXCombat, "Explosion/retro_explosion_life2");

        public static string SFX_Roll = "event:/SFX/Rolling/RollingSound";


        #endregion

        #endregion
    }
}