﻿namespace DataAccessLogic.HelperServices
{
    public static class ItemStatusProvider
    {
        public static string GetOnModerationStatus()
        {
            return "На модерации";
        }

        public static string GetRejectedStatus()
        {
            return "Отклонен";
        }

        public static string GetAcceptedStatus()
        {
            return "Размещён";
        }

        public static string GetSoldStatus()
        {
            return "Продан";
        }
    }
}