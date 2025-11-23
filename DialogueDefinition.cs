namespace DialogueSystem2
{
    /// <summary>
    /// Описание одного линейного диалога (последовательность реплик).
    /// </summary>
    public class Dialogue
    {
        public string SceneId { get; set; }
        public string NpcId { get; set; }

        /// <summary>Идентификатор диалога внутри файла (dialog1, questIntro и т.п.).</summary>
        public string DialogueId { get; set; }

        /// <summary>Тип диалога: сюжетный или постоянный.</summary>
        public DialogueKind Kind { get; set; }

        /// <summary>
        /// Порядок диалогов внутри файла (0,1,2...), чтобы понимать "следующий диалог".
        /// </summary>
        public int DialogueOrder { get; set; }

        /// <summary>
        /// Какой глобальный story-checkpoint должен быть установлен,
        /// чтобы этот диалог был доступен. Пустая строка — нет требования.
        /// </summary>
        public string RequiredGlobalCheckpoint { get; set; }

        /// <summary>
        /// Какой локальный checkpoint должен быть установлен,
        /// чтобы диалог был доступен. Пустая строка — нет требования.
        /// </summary>
        public string RequiredLocalCheckpoint { get; set; }

        /// <summary>Строки диалога.</summary>
        public List<DialogueLine> Lines { get; set; } = new List<DialogueLine>();

        public Dialogue()
        {
            SceneId = string.Empty;
            NpcId = string.Empty;
            DialogueId = string.Empty;
            RequiredGlobalCheckpoint = string.Empty;
            RequiredLocalCheckpoint = string.Empty;
        }
    }
}
