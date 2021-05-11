using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

public class HighScoreTable : MonoBehaviour
{
    // Text files and paths
    string highScoreFile, minigameHighScoreFile;

    // UI Elements
    [SerializeField]
    GameObject highScoreText;
    GameObject[] scoreObjects;

    struct HighScoreEntry 
    {
        public int score;
        public string name;
    }

    List<HighScoreEntry> allScores = new List<HighScoreEntry>();

    public void LoadHighScoreTable(string path) 
    {
        using (TextReader file = File.OpenText(path))
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
        scoreObjects = new GameObject[allScores.Count];
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
            t.fontSize = 40;

            o.transform.localPosition = new Vector3(5, (-(i) * 6) - 6, 0);

            o.transform.localRotation = Quaternion.identity;
            o.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            o.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);

            scoreObjects.SetValue(o, i);
        }
    }

    public void SortHighScoreEntries()
    {
        allScores.Sort((x, y) => y.score.CompareTo(x.score));
    }

    public void SaveScore(string name, int score, string mode)
    {
        string path = "";
        if (mode == "game")
        {
            path = highScoreFile;
        }
        else if (mode == "minigame")
        {
            path = minigameHighScoreFile;
        }

        using StreamWriter file = new StreamWriter(path, true);

        file.WriteLine(name + " " + score.ToString());
        file.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        // As assets are combined on build, this makes a new file will all the scores from the
        // files within resources.
        highScoreFile = "scores.txt";
        minigameHighScoreFile = "minigame_scores.txt";

        if (!File.Exists(highScoreFile) && !File.Exists(minigameHighScoreFile))
        {
            File.Create(highScoreFile).Close();
            File.Create(minigameHighScoreFile).Close();

            File.WriteAllText(highScoreFile, Resources.Load<TextAsset>("scores").text);
            File.WriteAllText(minigameHighScoreFile, Resources.Load<TextAsset>("minigame_scores").text);
        }

        LoadHighScoreTable(highScoreFile);
        SortHighScoreEntries();
        CreateHighScoreText();
    }

    public void SetHighScore()
    {
        LoadHighScoreTable(highScoreFile);
        SortHighScoreEntries();
        highScoreText.GetComponent<Text>().text = allScores[0].score.ToString();
    }

    public void SetHighScoreTable(string mode)
    {
        allScores.Clear();
        foreach (GameObject scoreObject in scoreObjects)
        {
            Destroy(scoreObject);
        }
        Array.Clear(scoreObjects, 0, scoreObjects.Length);

        string path = "";
        if (mode == "game")
        {
            path = highScoreFile;
        }
        else if (mode == "minigame")
        {
            path = minigameHighScoreFile;
        }
        LoadHighScoreTable(path);
        SortHighScoreEntries();
        CreateHighScoreText();
    }
}
