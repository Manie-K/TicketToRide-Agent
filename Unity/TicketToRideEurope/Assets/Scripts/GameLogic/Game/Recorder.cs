using System;

namespace CoreEngine.Game
{
    public enum RecorderMode
    {
        Record,
        Replay
    }

    public class Recorder
    {
        private string path;
        private int choiceIndex;
        
        public RecorderMode Mode { get; private set; }


        public Recorder(string filePath, GameMode mode) 
        {
            this.path = filePath;
            choiceIndex = 0;

            this.Mode = (mode == GameMode.Record) ? RecorderMode.Record : (mode == GameMode.Replay) ? RecorderMode.Replay : throw new ArgumentException();
        }

        internal void RecordPlayerChoice(int index)
        {
            //File(path).append(currentPlayer.toString() + index) or something
        }

        internal int GetNextPlayerChoice()
        {
            int res = -1;
            //res = File(path).readAt(choiceIndex) 

            choiceIndex++;

            return res;
        }
    }
}
