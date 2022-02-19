using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="ProceduralCity/Rule")]
public class Rule : ScriptableObject
{
    public string letter; // Letter to trigger this rule
    [SerializeField] // Will allow to edit in Unity
    private string[] results = null;

    /**
     * Returns a letter of the rule
     */
    public string GetResult()
    {
        return results[0];
    }
}
