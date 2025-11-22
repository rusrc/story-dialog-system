namespace DialogueSystem2
{
    /// <summary>
    /// Хранилище всех загруженных диалогов из CSV.
    /// </summary>
    public class CsvDialogueDatabase
    {
        private readonly List<DialogueDefinition> _dialogues = new List<DialogueDefinition>();

        public IEnumerable<DialogueDefinition> GetDialogues(string sceneId, string npcId)
        {
            return _dialogues
                .Where(d => d.SceneId == sceneId && d.NpcId == npcId)
                .OrderBy(d => d.DialogueOrder);
        }

        public void AddDialogue(DialogueDefinition definition)
        {
            _dialogues.Add(definition);
        }

        /// <summary>
        /// Загрузить один CSV текст для конкретной сцены и NPC.
        /// В Unity можно передать TextAsset.text.
        /// Ожидаемый заголовок:
        /// dialogueId;dialogueOrder;lineIndex;kind;requiredGlobal;requiredLocal;resumeCheckpoint;setGlobal;setLocal;speaker;ru;en;fr;es
        /// </summary>
        public void LoadFromCsvText(string sceneId, string npcId, string csvText, char separator = ';')
        {
            using (var reader = new StringReader(csvText))
            {
                var header = reader.ReadLine(); // можно проанализировать, здесь просто пропускаем.
                if (header == null) return;

                var rows = new List<CsvRow>();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var row = CsvRow.Parse(sceneId, npcId, line, separator);
                    rows.Add(row);
                }

                var groups = rows.GroupBy(r => r.DialogueId);
                foreach (var g in groups)
                {
                    var sample = g.First();
                    var def = new DialogueDefinition
                    {
                        SceneId = sceneId,
                        NpcId = npcId,
                        DialogueId = sample.DialogueId,
                        DialogueOrder = sample.DialogueOrder,
                        Kind = sample.Kind,
                        RequiredGlobalCheckpoint = sample.RequiredGlobal,
                        RequiredLocalCheckpoint = sample.RequiredLocal
                    };

                    foreach (var r in g.OrderBy(r => r.LineIndex))
                    {
                        var lineDef = new DialogueLine
                        {
                            LineIndex = r.LineIndex,
                            Speaker = r.Speaker,
                            ResumeCheckpointId = r.ResumeCheckpoint,
                            GlobalCheckpointToSet = r.SetGlobal,
                            LocalCheckpointToSet = r.SetLocal
                        };

                        foreach (var kv in r.TextByLanguage)
                        {
                            lineDef.TextByLanguage[kv.Key] = kv.Value;
                        }

                        def.Lines.Add(lineDef);
                    }

                    AddDialogue(def);
                }
            }
        }

        private class CsvRow
        {
            public string SceneId { get; set; }
            public string NpcId { get; set; }
            public string DialogueId { get; set; }
            public int DialogueOrder { get; set; }
            public int LineIndex { get; set; }
            public DialogueKind Kind { get; set; }
            public string RequiredGlobal { get; set; }
            public string RequiredLocal { get; set; }
            public string ResumeCheckpoint { get; set; }
            public string SetGlobal { get; set; }
            public string SetLocal { get; set; }
            public string Speaker { get; set; }

            public Dictionary<string, string> TextByLanguage { get; } = new Dictionary<string, string>();

            public CsvRow()
            {
                SceneId = string.Empty;
                NpcId = string.Empty;
                DialogueId = string.Empty;
                RequiredGlobal = string.Empty;
                RequiredLocal = string.Empty;
                ResumeCheckpoint = string.Empty;
                SetGlobal = string.Empty;
                SetLocal = string.Empty;
                Speaker = string.Empty;
            }

            public static CsvRow Parse(string sceneId, string npcId, string line, char separator)
            {
                var parts = line.Split(separator);

                string Get(int index) => index < parts.Length ? parts[index].Trim() : string.Empty;

                // Ожидаемый порядок столбцов:
                // 0: dialogueId
                // 1: dialogueOrder
                // 2: lineIndex
                // 3: kind
                // 4: requiredGlobal
                // 5: requiredLocal
                // 6: resumeCheckpoint
                // 7: setGlobal
                // 8: setLocal
                // 9: speaker
                // 10: ru
                // 11: en
                // 12: fr
                // 13: es

                var kindStr = Get(3);
                DialogueKind kind;
                if (!Enum.TryParse(kindStr, true, out kind))
                {
                    kind = DialogueKind.Idle;
                }

                int TryParseInt(string s) => int.TryParse(s, out var v) ? v : 0;

                var row = new CsvRow
                {
                    SceneId = sceneId,
                    NpcId = npcId,
                    DialogueId = Get(0),
                    DialogueOrder = TryParseInt(Get(1)),
                    LineIndex = TryParseInt(Get(2)),
                    Kind = kind,
                    RequiredGlobal = Get(4),
                    RequiredLocal = Get(5),
                    ResumeCheckpoint = Get(6),
                    SetGlobal = Get(7),
                    SetLocal = Get(8),
                    Speaker = Get(9)
                };

                var ru = Get(10);
                var en = Get(11);
                var fr = Get(12);
                var es = Get(13);

                if (!string.IsNullOrEmpty(ru)) row.TextByLanguage["ru"] = ru;
                if (!string.IsNullOrEmpty(en)) row.TextByLanguage["en"] = en;
                if (!string.IsNullOrEmpty(fr)) row.TextByLanguage["fr"] = fr;
                if (!string.IsNullOrEmpty(es)) row.TextByLanguage["es"] = es;

                return row;
            }
        }
    }
}
