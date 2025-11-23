namespace DialogueSystem2
{
    /// <summary>
    /// Простейшая in-memory реализация для тестов / прототипов.
    /// В Unity замените на сохранение в PlayerPrefs/файл.
    /// </summary>
    public class InMemoryCheckpointStore : ICheckpointStore
    {
        private readonly HashSet<string> _global = new HashSet<string>();
        private readonly HashSet<string> _local = new HashSet<string>();
        private readonly Dictionary<string, DialogueProgress> _progress = new Dictionary<string, DialogueProgress>();

        public bool HasGlobalCheckpoint(string sceneId, string checkpointId)  
            => _global.Contains(GlobalKey(sceneId, checkpointId));

        public void SetGlobalCheckpoint(string sceneId, string checkpointId)
        {
            if (string.IsNullOrEmpty(checkpointId)) return;
            _global.Add(GlobalKey(sceneId, checkpointId));
        }

        public bool HasLocalCheckpoint(string sceneId, string npcId, string checkpointId) 
            => _local.Contains(LocalKey(sceneId, npcId, checkpointId));

        public void SetLocalCheckpoint(string sceneId, string npcId, string checkpointId)
        {
            if (string.IsNullOrEmpty(checkpointId)) return;
            _local.Add(LocalKey(sceneId, npcId, checkpointId));
        }

        public DialogueProgress GetDialogueProgress(string sceneId, string npcId, string dialogueId)
        {
            var key = ProgressKey(sceneId, npcId, dialogueId);
            if (_progress.TryGetValue(key, out var progress))
                return progress;

            return new DialogueProgress
            {
                SceneId = sceneId,
                NpcId = npcId,
                DialogueId = dialogueId
            };
        }

        public void SaveDialogueProgress(DialogueProgress progress)
        {
            var key = ProgressKey(progress.SceneId, progress.NpcId, progress.DialogueId);
            _progress[key] = progress;
        }

        private static string GlobalKey(string sceneId, string checkpointId) => $"{sceneId}::{checkpointId}";
        private static string LocalKey(string sceneId, string npcId, string checkpointId) => $"{sceneId}::{npcId}::{checkpointId}";
        private static string ProgressKey(string sceneId, string npcId, string dialogueId) => $"{sceneId}::{npcId}::{dialogueId}";
    }
}
