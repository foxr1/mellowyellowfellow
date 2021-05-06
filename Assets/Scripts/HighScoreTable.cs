using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreTable : MonoBehaviour
{
    [SerializeField]
    string highScoreFile = "scores.txt";

    struct HighScoreEntry 
    {
        public int score;
        public string name;
    }

    List<HighScoreEntry> allScores = new List<HighScoreEntry>();

    public void LoadHighScoreTable() 
    {
        using (TextReader file = File.OpenText(highScoreFile))
        {
            string text = null;
            while ((text = file.ReadLine()) != null)
            {
                Debug.Log(text);
                string[] splits = text.Split(' ');
                HighScoreEntry entry;
                entry.name = splits[0];
                entry.score = int.Parse(splits[1]);
                allScores.Add(entry);
            }
        }
    }

    [SerializeField]
    Font scoreFont;

    void CreateHighScoreText()
    {
        
        for (int i = 0; i < allScores.Count; ++i) 
        {
            // Changed to end loop at 10 to remove overflowing out of UI when loading high scores
            // Used a break statement instead of changing end parameter to factor for if there are 
            // less than 10 entries in the .txt and throwing an error.
            if (i == 10)
            {
                break;
            }

            GameObject o = new GameObject();
            o.transform.parent = transform;

            Text t = o.AddComponent<Text>();
            t.text = allScores[i].name + "\t\t" + allScores[i].score;
            t.font = scoreFont;
            t.fontSize = 50;

            o.transform.localPosition = new Vector3(0, -(i) * 6, 0);

            o.transform.localRotation = Quaternion.identity;
            o.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            o.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);
        }
    }

    public void SortHighScoreEntries()
    {
        allScores.Sort((x, y) => y.score.CompareTo(x.score));
    }

    public void SaveScore(string name, int score)
    {
        using StreamWriter file = new StreamWriter(highScoreFile, true);
        file.WriteLine("\n" + name + " " + score.ToString());
        file.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadHighScoreTable();
        SortHighScoreEntries();
        CreateHighScoreText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
