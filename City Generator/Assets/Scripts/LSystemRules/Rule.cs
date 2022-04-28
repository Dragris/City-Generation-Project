using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ProceduralCity/Rule")]
public class Rule : ScriptableObject
{
    public string letter; // Letter to trigger this rule
    [SerializeField] // Will allow to edit in Unity
    private string[] rewritingRules = null;
    
    [SerializeField]
    private bool randomResult = false;
    public int seed = 0;

    /**
     * Returns a letter of the rule
     */
    public string GetResult(int seed = 0)
    {
        // Set default to 0 as if we don't have randomResult activated
        int randomIndex = 0;
        if (randomResult) {
            System.Random prng = new System.Random(seed);
            // Get the random index 
            randomIndex = prng.Next(0, rewritingRules.Length);
        }
        
        return rewritingRules[randomIndex];
    }
}
