namespace DialogueSystem2
{
    /// <summary>
    /// Хранилище всех загруженных диалогов из CSV.
    /// </summary>
    public class DialogueCollection
    {
        private readonly List<Dialogue> _dialogues = new List<Dialogue>();

        public IEnumerable<Dialogue> GetDialogues(string sceneId, string npcId)
        {
            return _dialogues
                .Where(d => d.SceneId == sceneId && d.NpcId == npcId)
                .OrderBy(d => d.DialogueOrder);
        }

        public void AddDialogue(Dialogue definition)
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
            using var reader = new StringReader(csvText);

            // Пропускаем заголовок
            if (reader.ReadLine() == null)
                return;

            var dialogues = CreateDialoguesFromCsv(reader, sceneId, npcId, separator);

            foreach (var dialogue in dialogues)
            {
                AddDialogue(dialogue);
            }
        }

        private IEnumerable<Dialogue> CreateDialoguesFromCsv(
            TextReader reader,
            string sceneId,
            string npcId,
            char separator)
        {
            var csvRows = ReadLines(reader)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Where(line => !line.All(c => c == separator))
                .Select(line => CsvRow.Parse(sceneId, npcId, line, separator))
                .ToList();

            var rowsDialogs = csvRows.GroupBy(r =>  r.DialogueId);

            foreach (var rowsDialog in rowsDialogs)
            {
                yield return CreateDialogueDefinition(rowsDialog, sceneId, npcId);
            }
        }

        private Dialogue CreateDialogueDefinition(
            IGrouping<string, CsvRow> rowsDialog,
            string sceneId,
            string npcId)
        {
            // Получим любой, т.е. они повторяются
            var d = rowsDialog.First();

            var dialogueDefinition = new Dialogue
            {
                SceneId = sceneId,
                NpcId = npcId,
                DialogueId = d.DialogueId,
                DialogueOrder = d.DialogueOrder,
                Kind = d.Kind,
                RequiredGlobalCheckpoint = d.RequiredGlobal,
                RequiredLocalCheckpoint = d.RequiredLocal,
                Lines = CreateDialogueLines(rowsDialog)
            };

            return dialogueDefinition;
        }

        private List<DialogueLine> CreateDialogueLines(IGrouping<string, CsvRow> dialogGroup)
        {
            var dialogLines = new List<DialogueLine>();

            foreach (var row in dialogGroup.OrderBy(r => r.LineIndex))
            {
                var dialogueLine = CreateDialogueLine(row);

                dialogLines.Add(dialogueLine);
            }

            return dialogLines;
        }

        private DialogueLine CreateDialogueLine(CsvRow row)
        {
            var dialogueLine = new DialogueLine
            {
                LineIndex = row.LineIndex,
                Speaker = row.Speaker,
                ResumeCheckpointId = row.ResumeCheckpoint,
                GlobalCheckpointToSet = row.SetGlobal,
                LocalCheckpointToSet = row.SetLocal
            };

            foreach (var (language, text) in row.TextByLanguage)
            {
                dialogueLine.TextByLanguage[language] = text;
            }

            return dialogueLine;
        }

        private static IEnumerable<string> ReadLines(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
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
