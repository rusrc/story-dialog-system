namespace DialogueSystem2
{
    /// <summary>
    /// Тип диалога: сюжетный (Story) или постоянный (Persistent).
    /// </summary>
    public enum DialogueKind
    {
        Story,
        Idle
    }

    /// <summary>
    /// Высокоуровневый менеджер: выбирает диалог и создаёт сессию.
    /// </summary>
    public class DialogueManager
    {
        private readonly CsvDialogueDatabase _database;
        private readonly ICheckpointStore _checkpointStore;

        public DialogueManager(CsvDialogueDatabase database, ICheckpointStore checkpointStore)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
        }

        /// <summary>
        /// Начать следующий подходящий диалог для данного NPC на сцене.
        /// Возвращает null, если подходящих диалогов нет.
        /// ВАЖНО: после получения session — подпишись на события и вызови session.Start().
        /// </summary>
        public DialogueSession StartNextDialogue(string sceneId, string npcId)
        {
            var dialogues = _database.GetDialogues(sceneId, npcId);

            foreach (var def in dialogues)
            {
                if (!IsDialogueAvailable(def, sceneId, npcId))
                    continue;

                var progress = _checkpointStore.GetDialogueProgress(sceneId, npcId, def.DialogueId);

                if (progress.IsCompleted)
                {
                    // Диалог уже прошёл, идём к следующему.
                    continue;
                }

                int startIndex = 0;

                if (!string.IsNullOrEmpty(progress.LastResumeCheckpointId))
                {
                    var idx = def.Lines.FindIndex(l => l.ResumeCheckpointId == progress.LastResumeCheckpointId);
                    if (idx >= 0)
                        startIndex = idx;
                }

                var session = new DialogueSession(def, startIndex, sceneId, npcId, _checkpointStore);
                return session;
            }

            return null;
        }

        /// <summary>
        /// Начать конкретный диалог по его ID (например, для скриптовых вызовов).
        /// После получения session обязательно вызови session.Start().
        /// </summary>
        public DialogueSession StartDialogueById(string sceneId, string npcId, string dialogueId)
        {
            var def = _database
                .GetDialogues(sceneId, npcId)
                .FirstOrDefault(d => d.DialogueId == dialogueId);

            if (def == null) return null;
            if (!IsDialogueAvailable(def, sceneId, npcId)) return null;

            var progress = _checkpointStore.GetDialogueProgress(sceneId, npcId, def.DialogueId);

            int startIndex = 0;
            if (!string.IsNullOrEmpty(progress.LastResumeCheckpointId))
            {
                var idx = def.Lines.FindIndex(l => l.ResumeCheckpointId == progress.LastResumeCheckpointId);
                if (idx >= 0)
                    startIndex = idx;
            }

            var session = new DialogueSession(def, startIndex, sceneId, npcId, _checkpointStore);
            return session;
        }

        /// <summary>
        /// Проверяем, можно ли запустить диалог по его требованиям.
        /// </summary>
        private bool IsDialogueAvailable(DialogueDefinition def, string sceneId, string npcId)
        {
            // Проверяем глобальный checkpoint, если указан.
            if (!string.IsNullOrEmpty(def.RequiredGlobalCheckpoint))
            {
                if (!_checkpointStore.HasGlobalCheckpoint(sceneId, def.RequiredGlobalCheckpoint))
                    return false;
            }

            // Проверяем локальный checkpoint, если указан.
            if (!string.IsNullOrEmpty(def.RequiredLocalCheckpoint))
            {
                if (!_checkpointStore.HasLocalCheckpoint(sceneId, npcId, def.RequiredLocalCheckpoint))
                    return false;
            }

            // Дополнительную логику для Story/Persistent можно добавить здесь.
            return true;
        }
    }
}
