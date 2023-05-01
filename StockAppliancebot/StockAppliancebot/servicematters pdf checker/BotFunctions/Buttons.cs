using Telegram.Bot.Types.ReplyMarkups;

namespace StockAppliance.BotFunctions
{
    class Buttons
    {
        /// <summary>
        /// Make ReplyKayboardMarkup when using /start
        /// </summary>
        /// <returns>ReplyKayboardMarkup type</returns>
        public static ReplyKeyboardMarkup StartRKM()
        {

            var r = new ReplyKeyboardMarkup(

                        new KeyboardButton[][]
                        {
                            new KeyboardButton[] // First row
                            {
                                new KeyboardButton( // First Column
                                    "👤 Мои данные"// Button Name
                                ),
                                new KeyboardButton( //Second column
                                    "🆘 Помощь" // Button Name
                                )
                            },
                             new KeyboardButton[] // First row
                            {
                                new KeyboardButton( //Second column
                                    "📋 Настройки таблиц" // Button Name
                                ),
                                new KeyboardButton( //Second column
                                    "📤 Настройки выдачи" // Button Name
                                )
                            }
                        }
                    )
            {
                ResizeKeyboard = true
            };
            return r;
        }

        /// <summary>
        /// Make InlineKeyboardMarkup for TableInfo
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup TableIKM()
        {
            return new InlineKeyboardMarkup(

                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "📜 Изменить таблицу",
                                "ChangeSpreadsheetQuery"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "📋 Изменить название листа",
                                "ChangeSheetName"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "❌ Закрыть меню настроек",
                                "CloseSettingsMenu"
                            )
                        }

                    }
                );

        }

        /// <summary>
        /// Make InlineKeyboardMarkup for OutputInfo
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup OutputIKM()
        {
            return new InlineKeyboardMarkup(

                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк в сокращенной выдаче",
                                "ChangeCountOfFirstOutputQuery"
                            )
                        },
                        /* new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "1️⃣ Изменить паузу перед первым сообщением",
                                "ChangeFirstTimeoutQuery"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "2️⃣ Изменить паузу перед вторым сообщением",
                                "ChangeSecondTimeoutQuery"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "⌛ Изменить паузу перед финальным сообщением",
                                "ChangeTotalTimeoutQuery"
                            )
                        },*/
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "❌ Закрыть меню настроек",
                                "CloseSettingsMenu"
                            )
                        }
                    }
                );

        }

        /// <summary>
        /// Make InlineKeyboardMarkup for RowsCountOutputInfo
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup RowsCountOutputIKM()
        {
            return new InlineKeyboardMarkup(

                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк 🌐 Diagram WEB",
                                "ChangeCountDiagramWeb"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк 📙Partlist PDF",
                                "ChangeCountPartlistPDF"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк 📙Tech Sheet PDF",
                                "ChangeCountTechSheetPDF"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк 📙Service Manual PDF",
                                "ChangeCountServiceManualPDF"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк 🌐Service Manual WEB",
                                "ChangeCountServiceManualWEB"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк 📙Wiring Sheet PDF",
                                "ChangeCountWiringSheetPDF"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "📤 Изменить количество строк 📙Service pointer PDF",
                                "ChangeCountServicePointerPDF"
                            )
                        },
                         new InlineKeyboardButton[]
                        {

                            InlineKeyboardButton.WithCallbackData(
                                "❌ Закрыть меню настроек",
                                "CloseSettingsMenu"
                            )
                        }
                    }
                );

        }

        /// <summary>
        /// Make ForceReplyMarkup for Spreadsheet
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup SpreadsheetFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "https://docs.google.com/spreadsheets/d/1Om3ou1S3un1KyiG35DTAwTjZLMi8u3hV7Us5xViCToM/edit#gid=0";
            return a;

        }

        /// <summary>
        /// Make ForceReplyMarkup for Spreadsheet Sheet Name
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup SpreadsheetSheetNameFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "Лист1";
            return a;

        }

        /// <summary>
        /// Make ForceReplyMarkup for Integer data
        /// </summary>
        /// <returns></returns>
        public static ForceReplyMarkup IntegerFRM()
        {
            var a = new ForceReplyMarkup();
            a.InputFieldPlaceholder = "Целочисленное значение: 5,8,15 и т.д.";
            return a;

        }

        /// <summary>
        /// Make InlineKeyboardMarkup for Show More action
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup ShowMoreIKM()
        {
            return new InlineKeyboardMarkup(

                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "⇣ Show other results",
                                "ShowMoreQuery"
                            )
                        }

                    }
                );

        }

        /// <summary>
        /// Make InlineKeyboardMarkup for Show More action
        /// </summary>
        /// <returns></returns>
        public static InlineKeyboardMarkup ShowLessIKM()
        {
            return new InlineKeyboardMarkup(

                    new InlineKeyboardButton[][]
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                "⇡ Hide other results",
                                "ShowLessQuery"
                            )
                        }

                    }
                );

        }

    }
}
