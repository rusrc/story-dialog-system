namespace DialogueSystem2
{
    /// <summary>
    /// Интерфейс к системе сохранений (Unity реализует по-своему).
    /// </summary>
    public interface ICheckpointStore
    {
        // Глобальные story-checkpoints (общие для сцены, одинаковые у всех NPC на сцене).
        bool HasGlobalCheckpoint(string sceneId, string checkpointId);
        void SetGlobalCheckpoint(string sceneId, string checkpointId);

        // Локальные story-checkpoints (обычно привязаны к NPC/квесту).
        bool HasLocalCheckpoint(string sceneId, string npcId, string checkpointId);
        void SetLocalCheckpoint(string sceneId, string npcId, string checkpointId);

        // Прогресс по конкретному диалогу.
        DialogueProgress GetDialogueProgress(string sceneId, string npcId, string dialogueId);
        void SaveDialogueProgress(DialogueProgress progress);
    }
}
