using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System.Text;
public class Lsystem {
    public Lsystem(ref string s, Dictionary<char, string> rule, char axiom, int appendmentChance) {
    StringBuilder sb = new StringBuilder();
        if (appendmentChance != 0 & s != axiom.ToString()) {
            foreach (char c in s) {
                if (rule.ContainsKey(c)) {
                    if (UnityEngine.Random.Range(0, 100) > appendmentChance) {
                        sb.Append(rule[c]);
                    }
                } else {
                    sb.Append(c.ToString());
                }
            }
        } else {
            foreach (char c in s) {
                sb.Append(rule.ContainsKey(c) ? rule[c] : c.ToString());
            }
        }
        s = sb.ToString();
        Debug.Log("Single String ammendment called: " + s);
    }

    public Lsystem(ref string s, Dictionary<char, string>[] rule, char[] axiom, int appendmentChance) {
        StringBuilder sb = new StringBuilder();
        if (appendmentChance != 0 & s != axiom.ToString()) {
            foreach (char c in s) {
                if (rule[0].ContainsKey(c)) {
                    if (UnityEngine.Random.Range(0, 100) > appendmentChance) {
                        sb.Append(rule[0][c]);
                    }
                } else if (rule[1].ContainsKey(c)) {
                    if (UnityEngine.Random.Range(0, 100) > appendmentChance) {
                        sb.Append(rule[1][c]);
                    }
                }else {
                    sb.Append(c.ToString());
                }
            }
        } else {
            foreach (char c in s) {
                if (rule[0].ContainsKey(c)) {
                    sb.Append(rule[0][c]);
                } else if (rule[1].ContainsKey(c)) {
                    sb.Append(rule[1][c]);
                } else {
                    sb.Append(c.ToString());
                } 
            }
        }
        s = sb.ToString();
        Debug.Log("Double String ammendment called: " + s);
    }
}
