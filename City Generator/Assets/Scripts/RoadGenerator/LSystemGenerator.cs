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
    public bool randomIgnoreRuleModifier = true;
    [Range(0,1)]
    public float changeToIgnoreRule = 0.3f;
    public int seed = 0;
    private void Start()
    {
        Debug.Log(GenerateSentence());
    }

    public string GenerateSentence(string word = null)
    {
        System.Random prng = new System.Random(seed);
        if (word == null)
        {
            word = rootSentence;
        }
        return GrowRecursive(word, prng);
    }

    private string GrowRecursive(string word, System.Random prng, int currentIteration = 0)
    {
        if (iterationLimit <= currentIteration)
        {
            return word;
        }
        StringBuilder newWord = new StringBuilder();
        foreach (var c in word)
        {
            newWord.Append(c); // Append each character and then recursive part
            int nextRandom = prng.Next();
            ProcessRulesRecursively(newWord, c, currentIteration, prng);
        }

        return newWord.ToString();
    }

    private void ProcessRulesRecursively(StringBuilder newWord, char c, int currentIteration, System.Random prng)
    {
        foreach (var rule in rules)
        {
            if (rule.letter == c.ToString())
            {   
                // Always generate the first branch, otherwise we could end up without city
                if (randomIgnoreRuleModifier && currentIteration > 1) {
                    System.Random subprng = new System.Random(rule.seed+prng.Next());
                    float randomValue = (float)subprng.Next(0, 10)/10f;
                    if (randomValue < changeToIgnoreRule) {
                        // If random value is minor than the change to skip the rule we avoid generating this branch alltogether
                        return;
                    }
                }
                newWord.Append(GrowRecursive(rule.GetResult(rule.seed), prng, currentIteration + 1));
            }
        }
    }
}
