using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystemGenerator : MonoBehaviour
{
    public Rule[] rules; // Array of rules that we want to use
    public string rootSentence; // Sentence from we will generate

    [Range(0, 20)] // Range of iterationLimint
    public int iterationLimit = 1;
    
    private void Start()
    {
        Debug.Log(GenerateSentence());
    }

    public string GenerateSentence(string word = null)
    {
        if (word == null)
        {
            word = rootSentence;
        }
        return GrowRecursive(word);
    }

    private string GrowRecursive(string word, int currentIteration = 0)
    {
        if (iterationLimit <= currentIteration)
        {
            return word;
        }
        StringBuilder newWord = new StringBuilder();

        foreach (var c in word)
        {
            newWord.Append(c); // Append each character and then recursive part
            ProcessRulesRecursively(newWord, c, currentIteration);
        }

        return newWord.ToString();
    }

    private void ProcessRulesRecursively(StringBuilder newWord, char c, int currentIteration)
    {
        foreach (var rule in rules)
        {
            if (rule.letter == c.ToString())
            {
                newWord.Append(GrowRecursive(rule.GetResult(), currentIteration + 1));
            }
        }
    }
}
