namespace DialogueSystem2
{
    /// <summary>
    /// Активная сессия диалога (проигрывание строк).
    /// </summary>
    public class DialogueSession
    {
        private readonly DialogueDefinition _definition;
        private readonly ICheckpointStore _checkpointStore;
        private readonly string _sceneId;
        private readonly string _npcId;

        private int _currentIndex;
        private bool _isCompleted;
        private bool _started;

        public DialogueDefinition Definition => _definition;
        public bool IsCompleted => _isCompleted;
        public bool IsStarted => _started;

        /// <summary>
        /// Вызывается один раз при начале диалога (после вызова Start()).
        /// </summary>
        public event Action<DialogueSession> OnDialogStart;

        /// <summary>
        /// Вызывается каждый раз, когда активная строка меняется
        /// (включая первую строку в Start()).
        /// </summary>
        public event Action<DialogueSession, DialogueLine> OnDialogChange;

        /// <summary>
        /// Вызывается при завершении диалога (когда строк больше нет).
        /// </summary>
        public event Action<DialogueSession> OnDialogEnd;

        public DialogueSession(DialogueDefinition definition,
                               int startLineIndex,
                               string sceneId,
                               string npcId,
                               ICheckpointStore checkpointStore)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
            _sceneId = sceneId;
            _npcId = npcId;

            if (_definition.Lines.Count == 0)
            {
                _currentIndex = -1;
                _isCompleted = true;
            }
            else
            {
                _currentIndex = Math.Max(0, Math.Min(startLineIndex, _definition.Lines.Count - 1));
            }
        }

        /// <summary>
        /// Начать проигрывание диалога.
        /// Применяет эффекты первой строки и вызывает OnDialogStart + OnDialogChange.
        /// </summary>
        public void Start()
        {
            if (_started) return;
            _started = true;

            OnDialogStart?.Invoke(this);

            if (_definition.Lines.Count == 0)
            {
                // Пустой диалог — сразу конец.
                MarkDialogueCompleted();
                OnDialogEnd?.Invoke(this);
                return;
            }

            ApplyCurrentLineSideEffects(markCompletedIfEnd: false);
            OnDialogChange?.Invoke(this, _definition.Lines[_currentIndex]);
        }

        /// <summary>
        /// Текущая строка (после Start()). Если диалог завершён или не начат — бросает исключение.
        /// </summary>
        public DialogueLine GetCurrentLine()
        {
            if (!_started)
                throw new InvalidOperationException("Dialogue not started. Call Start() first.");

            if (_isCompleted || _currentIndex < 0 || _currentIndex >= _definition.Lines.Count)
                throw new InvalidOperationException("Dialogue has no current line (it is completed or empty).");

            return _definition.Lines[_currentIndex];
        }

        /// <summary>
        /// Перейти к следующей строке.
        /// Возвращает true, если новая текущая строка существует,
        /// false — если диалог завершён (OnDialogEnd будет вызван).
        /// </summary>
        public bool MoveNextLine()
        {
            if (!_started)
                throw new InvalidOperationException("Dialogue not started. Call Start() first.");

            if (_isCompleted)
                return false;

            _currentIndex++;

            if (_currentIndex >= _definition.Lines.Count)
            {
                // Диалог закончился.
                MarkDialogueCompleted();
                OnDialogEnd?.Invoke(this);
                return false;
            }

            ApplyCurrentLineSideEffects(markCompletedIfEnd: false);
            OnDialogChange?.Invoke(this, _definition.Lines[_currentIndex]);
            return true;
        }

        private void ApplyCurrentLineSideEffects(bool markCompletedIfEnd)
        {
            if (_currentIndex < 0 || _currentIndex >= _definition.Lines.Count)
                return;

            var line = _definition.Lines[_currentIndex];

            // Применяем чекпоинты (глобальный и локальный), если они указаны.
            if (!string.IsNullOrEmpty(line.GlobalCheckpointToSet))
            {
                _checkpointStore.SetGlobalCheckpoint(_sceneId, line.GlobalCheckpointToSet);
            }

            if (!string.IsNullOrEmpty(line.LocalCheckpointToSet))
            {
                _checkpointStore.SetLocalCheckpoint(_sceneId, _npcId, line.LocalCheckpointToSet);
            }

            // Обновляем прогресс диалога.
            var progress = _checkpointStore.GetDialogueProgress(_sceneId, _npcId, _definition.DialogueId);

            if (!string.IsNullOrEmpty(line.ResumeCheckpointId))
            {
                progress.LastResumeCheckpointId = line.ResumeCheckpointId;
            }

            if (markCompletedIfEnd && _currentIndex == _definition.Lines.Count - 1)
            {
                progress.IsCompleted = true;
            }

            _checkpointStore.SaveDialogueProgress(progress);
        }

        private void MarkDialogueCompleted()
        {
            if (_isCompleted) return;

            _isCompleted = true;

            var progress = _checkpointStore.GetDialogueProgress(_sceneId, _npcId, _definition.DialogueId);
            progress.IsCompleted = true;
            _checkpointStore.SaveDialogueProgress(progress);
        }
    }
}
