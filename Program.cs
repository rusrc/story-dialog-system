using DialogueSystem2;
using System.Text;

namespace DialogSystemConsoleApp;

public class Program
{
    public static void Main()
    {
        // Укажите путь к вашему файлу
        string filePath = "scene-wood_npc-wizard-dialog1.csv";

        // Проверяем существование файла
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден!");
            return;
        }

        // Читаем весь текст из файла
        string csvText = File.ReadAllText(filePath, Encoding.UTF8);

        // Где-нибудь при инициализации сцены:
        var database = new CsvDialogueDatabase();
        database.LoadFromCsvText("wood", "wizard", csvText);

        var checkpointStore = new InMemoryCheckpointStore(); // или свой Unity-Store
        var dialogueManager = new DialogueManager(database, checkpointStore);

        // Когда игрок кликает по NPC:
        var session = dialogueManager.StartNextDialogue("wood", "wizard");
        if (session != null)
        {
            session.OnDialogStart += s => { /* показать окно диалога */ Console.WriteLine("Диалог запустился"); };
            session.OnDialogChange += (s, line) => Console.WriteLine("Console text: " + line.GetText("en"));
            session.OnDialogEnd += s => { /* скрыть окно диалога / триггер чего-то */ Console.WriteLine("Диалог закончился"); };

            session.Start(); // запустить первую строку

            while (!session.IsCompleted)
            {
                Console.ReadKey();
                session.MoveNextLine();
            }

            // По нажатию "Далее" в UI:
            // if (!session.MoveNextLine()) { диалог закончился }
        }
    }
}
