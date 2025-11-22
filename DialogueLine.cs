namespace DialogueSystem2
{
    /// <summary>
    /// Одна строка диалога.
    /// </summary>
    public class DialogueLine
    {
        /// <summary>
        /// Порядковый индекс строки внутри диалога (0..N-1).
        /// </summary>
        public int LineIndex { get; set; }

        /// <summary> 
        /// Имя говорящего (NPC / игрок / система).
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// Локализованные тексты: ключ = код языка ("ru", "en", "fr", "es", ...).
        /// </summary>
        public Dictionary<string, string> TextByLanguage { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Имя локального checkpoint’а (локальная точка возобновления),
        /// если непустая — прогресс будет сохранён сюда при показе этой строки.
        /// </summary>
        public string ResumeCheckpointId { get; set; }

        /// <summary>
        /// Глобальный story-checkpoint, который выставляется при показе строки.
        /// </summary>
        public string GlobalCheckpointToSet { get; set; }

        /// <summary>
        /// Локальный story-checkpoint, который выставляется при показе строки.
        /// </summary>
        public string LocalCheckpointToSet { get; set; }

        public DialogueLine()
        {
            Speaker = string.Empty;
            ResumeCheckpointId = string.Empty;
            GlobalCheckpointToSet = string.Empty;
            LocalCheckpointToSet = string.Empty;
        }

        /// <summary>
        ///  Получить текст на нужном языке, с fallback (например, на "en" или "ru").
        /// </summary>
        /// <param name="languageCode">Код языка</param>
        /// <param name="fallbackLanguageCode">
        ///  это запасной код языка, который используется когда:
        /// 1. Не нашли язык системы - если не удалось определить язык ОС
        /// 2. Нет перевода - если для языка системы нет локализации в приложении
        /// 3. Ошибка загрузки - если файлы перевода для основного языка недоступны
        /// </param>
        /// <returns></returns>
        public string GetText(string languageCode, string fallbackLanguageCode = "en")
        {
            if (!string.IsNullOrEmpty(languageCode) &&
                TextByLanguage.TryGetValue(languageCode, out var localized) &&
                !string.IsNullOrEmpty(localized))
            {
                return localized;
            }

            if (!string.IsNullOrEmpty(fallbackLanguageCode) &&
                TextByLanguage.TryGetValue(fallbackLanguageCode, out var fallback) &&
                !string.IsNullOrEmpty(fallback))
            {
                return fallback;
            }

            if (TextByLanguage.Count > 0)
                return TextByLanguage.Values.First();

            return string.Empty;
        }
    }
}
