using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public static class stressrules{

    public const int one = 1; // i don't know what this is for, but maybe don't mess with it?

    public struct VowelInfo
    {
        public bool longVowel;
        public bool wildcard;
        public string stressedSound;
        public string unstressedSound;

        public VowelInfo(bool is_this_long_vowel)
        {
            longVowel = is_this_long_vowel;
            wildcard = false;
            stressedSound = "none";
            unstressedSound = "none";
        }

        public VowelInfo(bool is_this_long_vowel, bool wild_type_sound)
        {
            longVowel = is_this_long_vowel;
            wildcard = wild_type_sound;
            stressedSound = "none";
            unstressedSound = "none";
        }

        public VowelInfo(bool is_this_long_vowel, bool wild_type_sound, string stressed_Sound, string unstressed_Sound)
        {
            longVowel = is_this_long_vowel;
            wildcard = wild_type_sound;
            stressedSound = stressed_Sound;
            unstressedSound = unstressed_Sound;
        }

    };

    public struct VowelInfo2
    {
        public string sound_;
        public int consonantsBefore_;
        public int phonemesIndex_;

        public VowelInfo2(string sound,int consonantsBefore,int phonemesIndex)
        {
            sound_ = sound;
            consonantsBefore_ = consonantsBefore;
            phonemesIndex_ = phonemesIndex;
        }

    };

    public static int HowManyVowels(string substr)
    {
        List<int> whatever = new List<int>();
       List<string> lol = orthography.main(substr,false,out whatever); //oh deer
        int num = 0;
        foreach (var phon in lol)
        {
            if (vowelLookup.ContainsKey(phon))
            {
                num++;
            }
        }
        return num;
    }

    public static SortedDictionary<string, VowelInfo> vowelLookup = new SortedDictionary<string, VowelInfo>()
        {

            //"uh","ah","aa","eh","schwa","er","ih","ee","au","oo","uu","ai","ui","ou","ei","oh","oi"

        //\\\\//   arg 1 is whether the vowel is long //arg 2 is whether there are different values about stress

        //\\\\//   args 3 and 4 are the stressed then unstressed sounds   //\\\\//

            {"uh", new VowelInfo(false,true,"uh","schwa")},
            {"ah", new VowelInfo(false,true,"ah","schwa")},
            {"aa", new VowelInfo(true,true,"aa","schwa")},
            {"eh", new VowelInfo(false,false)},
            {"schwa", new VowelInfo(false,false)},
            {"er", new VowelInfo(false,false)},
            {"ih", new VowelInfo(false,false)},
            {"ee", new VowelInfo(true,false)},
            {"au", new VowelInfo(false,false)},
            {"oo", new VowelInfo(false,false)},
            {"uu", new VowelInfo(true,false)},
            {"ai", new VowelInfo(true,false)},
            {"ui", new VowelInfo(true,true,"ui","ih")},
            {"ou", new VowelInfo(true,false)},
            {"ei", new VowelInfo(true,false)},
            {"oh", new VowelInfo(true,false)},
            {"oi", new VowelInfo(true,false)},
            //// // / / /  / /  /  /  /   /  /    /     /     /      /        /          /              /                       /
            {"y-wild", new VowelInfo(true,true,"ai","ih")},
            {"final-y-wild", new VowelInfo(true,true,"ai","ee")},
            {"final-i-wild", new VowelInfo(true,true,"ai","ee")},
            {"i-wild", new VowelInfo(false,true,"ai","ih")},
            {"u-wild", new VowelInfo(false,true,"uh","schwa")},
            {"o-wild", new VowelInfo(false,true,"au","schwa")},
            {"er-wild", new VowelInfo(false,true,"er","er")},
            {"final-ee-wild", new VowelInfo(false,true,"ei","ee")},
            {"e-wild", new VowelInfo(false,true,"eh","schwa")},
            {"a-wild", new VowelInfo(false,true,"aa","schwa")},
            {"ei-wild", new VowelInfo(false,true,"ei","eh")},
            {"final-a-wild", new VowelInfo(false,true,"ah","schwa")},

        };

    public static SortedDictionary<string, int> suffixTests = new SortedDictionary<string, int>() //string is name, int is stressed syllable's vowel distance from end
                                                                            //add 10000 if only at the ending position
        {
            {"sion",1},
            {"tion",1},
            {"ly",10002},
            {"ated",10003},
            {"able",2},
            {"cial",1},
            {"ial",2},
            {"cian",1},
            {"ery",10002},
            {"ory",10003},
            {"ian",1},
            {"ible",10002},
            {"ary",10003},
            {"ia",10002},
            {"ic",10001},
            {"ics",10001},
            {"ient",10001},
            {"ive",10001},
            {"ious",10001},
            {"ment",10002},
            {"ness",10002},
            {"ous",10002},
            {"ish",10001},
            {"sis",10001},

            {"cy",10002},
            {"ty",10002},
            {"phy",10002},
            {"gy",10002},
            {"val",10001},
            {"al",10002},

            {"ade",10000},
            {"ee",10000},
            {"or",1},
            {"eer",10000},
            {"ese",10000},
            {"ette",10000},
            {"self",10000},
            {"selves",10000},
            {"que",10001},
            {"oon",10000},
        };



    // It takes the result from "orthography" and modifies the stress placement
    public static List<string> main(List<string> orthoresult,string word,out List<int> stresses_)
    {
        List<string> moddedList = orthoresult;
        List<VowelInfo2> vowelsOnlyPart = new List<VowelInfo2>();
        List<int> stresses = new List<int>();
        int consonantsBefore = 0;
        int j2 = 0;
        foreach (string lol in moddedList) {
            if (vowelLookup.ContainsKey(lol))
            {
            vowelsOnlyPart.Add(new VowelInfo2(lol, consonantsBefore, j2));
            stresses.Add(0);
            consonantsBefore = -1;
            }
            consonantsBefore++;
            j2++;
        }

        int tests = 0;
        for (int i = vowelsOnlyPart.Count - 3; i >= -2; i-=3) //take three syllables at a time
        {
            if (i == -2)
            {
              stresses[0] -= tests;
            }
            if (i == -1)
            {
                    VowelInfo info0 = vowelLookup[vowelsOnlyPart[0].sound_];
                    VowelInfo info1 = vowelLookup[vowelsOnlyPart[1].sound_];
                    int cons1 = vowelsOnlyPart[1].consonantsBefore_;

                    if (!info1.longVowel)
                    {

                      stresses[0]++; 

                    }
                    else
                    {
                    if (info0.longVowel)
                    {
                        stresses[0]++;
                    }
                    else
                    {
                      stresses[1]++;
                    }
                    }
                //the two beginning syllables
                stresses[0] -= tests;
                stresses[1] -= tests;
            }

            if (i >= 0)
            {
                //three at a time
                int backtrace = 0;
                VowelInfo infoA = vowelLookup[vowelsOnlyPart[i].sound_];
                VowelInfo infoB = vowelLookup[vowelsOnlyPart[i + 1].sound_];
                VowelInfo infoC = vowelLookup[vowelsOnlyPart[i + 2].sound_];
                int consC = vowelsOnlyPart[i + 2].consonantsBefore_;

                if (!infoC.longVowel)
                {
                    if (infoB.longVowel || consC >= 2)
                    {
                        if (infoA.longVowel)
                        {
                            stresses[i] += 2;
                            stresses[i + 1]++;
                        }
                        else
                        {
                            stresses[i + 1] += 2;
                            stresses[i + 2]++;
                            backtrace++; //back
                        }
                    }
                    else
                    {
                        if (infoA.longVowel)
                        {
                            stresses[i] += 2;
                            stresses[i + 1]++;
                        }
                        else
                        {
                            stresses[i] += 2;
                            stresses[i + 1]++;
                        }
                    }
                }
                else
                {
                    if (!infoB.longVowel)
                    {
                        stresses[i] += 2;
                        stresses[i + 1]++;
                    }
                    else
                    {
                        if (infoA.longVowel)
                        {
                            stresses[i + 1]++;
                            stresses[i + 2] += 2; //idk
                        }
                        else
                        {
                            stresses[i] += 2;
                            stresses[i + 2]++; //idk
                        }
                    }
                }
                stresses[i] -= tests;
                stresses[i+1] -= tests;
                stresses[i+2] -= tests;
                i += backtrace;
            }
            tests++;
        }

        List<string> blacklist = new List<string>() { };

        foreach (KeyValuePair<string, int> stuf in suffixTests)
        {

            if (word.Contains(stuf.Key))
            {

                if (stuf.Value < 9000) //IT'S UNDAR 9000!!!1
                {
                    Regex r = new Regex(stuf.Key);
                    Match m = r.Match(word);
                    int pos2 = m.Captures[0].Index;
                    int num2 = HowManyVowels(word.Substring(0, pos2 + 1));
                    if (num2 - stuf.Value < 0)
                    {
                        stresses[0] = 100;
                        for (int lol = 0; lol < suffixTests.Count; lol++) // This delets the smol keys
                        {
                            string keystuf = suffixTests.ElementAt(lol).Key;
                            if (stuf.Key.Contains(keystuf))
                            {
                                blacklist.Add(keystuf);
                            }
                        }
                    }
                    else
                    {
                        stresses[num2 - stuf.Value] = 100 + (num2 - stuf.Value); // oh deer
                        for (int lol = 0; lol < suffixTests.Count; lol++) // This delets the smol keys
                        {
                            string keystuf = suffixTests.ElementAt(lol).Key;
                            if (stuf.Key.Contains(keystuf))
                            {
                                blacklist.Add(keystuf);
                            }
                        }
                    }
                }
                else
                {
                    int len2 = stuf.Key.Length;
                    if (word.Substring(word.Length - len2) == stuf.Key || (word.Substring(Mathf.Max(word.Length - len2 - 1,0)) == stuf.Key+"s"))
                    {
                        int num2 = stresses.Count - 1;
                        int val2 = stuf.Value - 10000;
                        if (num2 - val2 < 0)
                        {
                            if (stresses.Count > 0) { stresses[0] = 100; }
                            for (int lol = 0; lol < suffixTests.Count; lol++) // This delets the smol keys
                            {
                                string keystuf = suffixTests.ElementAt(lol).Key;
                                if (stuf.Key.Contains(keystuf))
                                {
                                    blacklist.Add(keystuf);
                                }
                            }
                        }
                        else
                        {
                            stresses[num2 - val2] = 100 + (num2 - val2); // oh deer
                            for (int lol = 0; lol < suffixTests.Count; lol++) // This delets the smol keys
                            {
                                string keystuf = suffixTests.ElementAt(lol).Key;
                                if (stuf.Key.Contains(keystuf))
                                {
                                    blacklist.Add(keystuf);
                                }
                            }
                        }
                    }
                }

        }


        }

        int primaryStressIndex = 0;
        if (stresses.Count > 1)
        {
            primaryStressIndex = stresses.IndexOf(stresses.Max()); //please do not harm the performance
            VowelInfo primStressInfo = vowelLookup[vowelsOnlyPart[primaryStressIndex].sound_];
            if (primStressInfo.wildcard)
            {
                stresses[primaryStressIndex] = int.MinValue;
                vowelsOnlyPart[primaryStressIndex] = new VowelInfo2(primStressInfo.stressedSound, 0, 0); //0 because it doesn't matter
            }
        }
        else
        {
            try
            {
                VowelInfo primStressInfo = vowelLookup[vowelsOnlyPart[0].sound_];
                if (primStressInfo.wildcard)
                {
                    stresses[0] = int.MinValue;
                    vowelsOnlyPart[0] = new VowelInfo2(primStressInfo.stressedSound, 0, 0); //0 because it doesn't matter
                }
            }
            catch
            {
                //:<
            }
        }

        int j = 0;
        for (int i = 0; i < vowelsOnlyPart.Count; i++)
        {
            VowelInfo2 item = vowelsOnlyPart[i];
            VowelInfo succ = vowelLookup[item.sound_];
            if (succ.wildcard == true && stresses[j] != int.MinValue)
            {
                if (stresses[j] >= 0)
                {
                    if (item.sound_.Length > 5)
                    {
                        if (item.sound_.Substring(0, 5) == "final")
                        {
                            vowelsOnlyPart[j] = new VowelInfo2(succ.unstressedSound, 0,0);
                        }
                    }

                    else
                    {
                        vowelsOnlyPart[j] = new VowelInfo2(succ.stressedSound, 0,0); //but it will be quieter
                    }
                }
                else
                {
                    vowelsOnlyPart[j] = new VowelInfo2(succ.unstressedSound, 0,0);
                }
            }
            j++;
        }

        j = 0;
        for (int i = 0; i < moddedList.Count; i++)
        {
            string phoneme = moddedList[i];
            if (vowelLookup.ContainsKey(phoneme))
            {
                moddedList[i] = vowelsOnlyPart[j].sound_;
                j++;
            }
        }

        stresses_ = stresses;

        return moddedList;
	}
}
