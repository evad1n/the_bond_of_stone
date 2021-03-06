﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Bond_of_Stone
{
    public class ScoreManager
    {
        public List<int> Score = new List<int>();
        int numHighScores = 10;

		public int mostRecentScore;

        const bool RESET_SCORES = false;

        public ScoreManager() {
            //Read from the hs.dat file and populate the highscore list
            using(BinaryReader reader = new BinaryReader(File.Open("hs.dat", FileMode.OpenOrCreate))) {
                int pos = 0;
                int length = (int)reader.BaseStream.Length;

                while (pos < length)
                {
                    int s = reader.ReadInt32();
                    Score.Add(s);

                    pos += sizeof(int);
                }
            }

            if (RESET_SCORES) {
                Score[0] = 1000000;
                Score[1] = 500000;
                Score[2] = 250000;
                Score[3] = 100000;
                Score[4] = 75000;
                Score[5] = 50000;
                Score[6] = 25000;
                Score[7] = 20000;
                Score[8] = 15000;
                Score[9] = 10000;
            }
        }
        
        public void AddScore(int score) {
            if (Score.Count < numHighScores || score > Score[Score.Count - 1]) {
                Score.Add(score);
                OrderScores();

                while(Score.Count > numHighScores)
                    Score.Remove(Score[Score.Count - 1]);
            }
        }

        public void OrderScores()
        {
            //Sort the scores ascending and then reverse the list so it's descending
            Score.Sort();
            Score.Reverse();
        }

        public void ClearFile()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open("hs.dat", FileMode.Truncate)))
            {
                foreach (int s in Score)
                {
                    writer.Write("");
                }
            }
        }

        public void RewriteFile()
        {
            //write the contents of the Score list to the file
            using (BinaryWriter writer = new BinaryWriter(File.Open("hs.dat", FileMode.Truncate)))
            {
                foreach(int s in Score)
                {
                    writer.Write(s);
                }
            }
        }
    }
}
