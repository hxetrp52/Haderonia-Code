using UnityEngine;

namespace UIStringKey
{
    public static class UIKey
    {
        private static string GetPath(string folder, string name) => $"{ROOT}{folder}{name}";
        #region 0. 폴더 경로
        private const string ROOT = "UI/";
        private const string LOBBY = "Lobby/";
        private const string BOSS = "Boss/";
        private const string TITLE = "Title/";
        private const string Guide = "Guide/";

        #endregion

        #region Lobby
        public static string DialogueUI => GetPath(LOBBY, "DialogueUI");
        public static string LobbySettingUI => GetPath(LOBBY, "LobbySettingUI");
        public static string OwnOutfitShowUI => GetPath(LOBBY, "OwnOutfitShowUI");
        public static string QuestSelectUI  => GetPath(LOBBY, "QuestSelectUI");
        public static string QuestAcceptPopup => GetPath(LOBBY, "QuestAcceptPopup");
        public static string QuestListPopup  => GetPath(LOBBY, "QuestListPopup");

        #endregion
    }
}
