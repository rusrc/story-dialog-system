namespace DialogueSystem2
{
    /// <summary>
    /// Прогресс по конкретному диалогу (для продолжения с checkpoint’а).
    /// </summary>
    public class DialogueProgress
    {
        public string SceneId { get; set; }
        // TODO rename to speaker
        public string NpcId { get; set; }
        public string DialogueId { get; set; }

        /// <summary>
        /// Имя последнего достигнутого ResumeCheckpointId внутри диалога
        /// (или пустая строка, если не начинали / не было чекпоинта).
        /// </summary>
        public string LastResumeCheckpointId { get; set; }

        /// <summary>Флаг, что диалог полностью пройден.</summary>
        public bool IsCompleted { get; set; }

        public DialogueProgress()
        {
            SceneId = string.Empty;
            NpcId = string.Empty;
            DialogueId = string.Empty;
            LastResumeCheckpointId = string.Empty;
        }
    }
}
