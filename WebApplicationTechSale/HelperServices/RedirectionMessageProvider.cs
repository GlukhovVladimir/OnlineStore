using System.Collections.Generic;

namespace WebApplicationTechSale.HelperServices
{
    public static class RedirectionMessageProvider
    {
        public static List<string> ItemAcceptedMessages()
        {
            return new List<string>()
            {
                "Модерация прошла успешно",
                "Товар опубликован и теперь виден всем пользователям.",
                "Сейчас вы будете перенаправлены на страницу с товарами"
            };
        }

        public static List<string> ItemRejectedMessages()
        {
            return new List<string>()
            {
                "Вы отклонили публикацию товара",
                "Пользователь получит уведомление и сможет отредактировать данные для повторной публикации.",
                "Сейчас вы будете перенаправлены на страницу c товарами",
            };
        }

        public static List<string> ItemCreatedMessages()
        {
            return new List<string>()
            {
                "Товар успешно создан",
                "Сейчас товар будет отправлен на модерацию. Модератор примет решение о публикации. Проверяйте статус товара в личном кабинете",
                "Сейчас вы будете перенаправлены на главную страницу"
            };
        }

        public static List<string> ItemUpdatedMessages()
        {
            return new List<string>()
            {
                "Данные обновлены",
                "Товар повторно отправлен на модерацию. Проверяйте статус товара в личном кабинете",
                "Сейчас вы будете перенаправлены на главную страницу"
            };
        }

        public static List<string> AccountCreatedMessages()
        {
            return new List<string>()
            {
                "Регистрация прошла успешно",
                "Сейчас вы будете перенаправлены на главную страницу нашего сайта"
            };
        }

        public static List<string> OrderCreateMessage()
        {
            return new List<string>()
            {
                "Поздравляем с покупкой!!!",
                "Сейчас вы будете перенаправлены на страницу со списком Ваших покупок"
            };
        }

        public static List<string> AccountUpdatedMessages()
        {
            return new List<string>()
            {
                "Изменения сохранены",
                "Сейчас вы будете перенаправлены в личный кабинет"
            };
        }
    }
}