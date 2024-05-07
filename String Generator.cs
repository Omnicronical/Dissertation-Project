using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class StringGenerator : MonoBehaviour
{
   
    public Rule[] rules;
    public string rootSentence;
    [Range(0,10)] public int iterationLimit = 1;

    public bool randomIgnoreRuleModifier = true;
    [Range(0, 1)] public float chanceToIgnoreRule = 0.3f;

    private void Start()
    {
        Debug.Log(GenerateSentence(iterationLimit));
    }

    public string GenerateSentence(int iterationAdjustment, string word = null)
    {
        iterationLimit = iterationAdjustment;
        if(word == null)
        {
            word = rootSentence;
        }
        return RecursiveGrow(word);
    }

    private string RecursiveGrow(string word, int currentIteration = 0)
    {
        if(currentIteration >= iterationLimit)
        {
            return word;
        }

        StringBuilder newWord = new StringBuilder();

        foreach (var c in word)
        {
            newWord.Append(c);
            ProcessRulesRecursivelly(newWord, c, currentIteration);
        }

        return newWord.ToString();


    }

    private void ProcessRulesRecursivelly(StringBuilder newWord, char c, int currentIteration)
    {
        foreach(var rule in rules)
        {
            if(rule.letter == c.ToString())
            {
                if (randomIgnoreRuleModifier && currentIteration > 1)
                {
                    if(Random.value < chanceToIgnoreRule)
                    {
                        return;
                    }
                }
                newWord.Append(RecursiveGrow(rule.GetResults(), currentIteration + 1));
            }
        }
    }
}
