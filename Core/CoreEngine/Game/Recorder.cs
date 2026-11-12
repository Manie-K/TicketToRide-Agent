
using System.Diagnostics;

namespace CoreEngine.Game
{
    public enum RecorderMode
    {
        Record,
        Replay
    }

    public class Recorder
    {
        private List<int> choices;
        private int index;
        private string fullPath;
        public RecorderMode Mode { get; private set; }


        public Recorder(string fullPath, GameMode mode) 
        {
            this.fullPath = fullPath;
            if (mode == GameMode.Record)
            {
                Mode = RecorderMode.Record;
                choices = new List<int>();
            }
            else
            {
                Mode = RecorderMode.Replay;
            }
        }

        internal void RecordPlayerChoice(int index)
        {
            Debug.Assert(Mode == RecorderMode.Record);
            choices.Add(index);
        }

        internal void SaveRecording()
        {
            Debug.Assert(Mode == RecorderMode.Record);
            string choicesString = "";
            foreach (var choice in choices)
            {
                choicesString += choice.ToString() + " ";
            }

            File.WriteAllText(fullPath, choicesString);
        }

        internal int ReadNextChoice()
        {
            Debug.Assert(Mode == RecorderMode.Replay);

            if (choices == null)
            {
                string[] savedChoices;
                index = 0;
                try
                {
                    savedChoices = File.ReadAllText(fullPath).Split(" ");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error when opening file: {e.Message}");
                    Console.WriteLine($"Stack trace: {e.StackTrace}");
                    return -1;
                }

                choices = new List<int>(savedChoices.Length);

                for (int i = 0; i < savedChoices.Length; i++)
                {
                    string? choice = savedChoices[i];
                    choices[i] = int.Parse(savedChoices[i]);    
                }
            }

            return choices[index++];
        }
    }
}
