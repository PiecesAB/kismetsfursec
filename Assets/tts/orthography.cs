
using UnityEngine; // This is meant for horrible-sounding text-to-speech purposes. //
using System.Text;
using System;
using System.Collections;
//using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public static class orthography
{

    public enum ComboRules
    {
        All, Middle, Starting, Ending, VowelNext, NoVowelNext, WholeWord, NotEnding, NotStarting, VowelBefore, NoVowelBefore, EIYNext, NotEIYNext, VoicedBefore, NotVoicedBefore, NoVowelsRemain, NotVoicedBeforeAndEnding, VoicedBeforeAndEnding
    };

    // i thought removing the struct would have improved loading performance. well it didn't.
    /*public struct PhoneticValues
    {
        public string[] list;
        public int backtrace;
        public ComboRules comboBehavior;
        public int minLength;

        public PhoneticValues(string[] l)
        {
            list = l;
            backtrace = 0;
            comboBehavior = ComboRules.All;
            minLength = 0;
        }
        public PhoneticValues(string[] l, int b)
        {
            list = l;
            backtrace = b;
            comboBehavior = ComboRules.All;
            minLength = 0;
        }

        public PhoneticValues(string[] l, int b, ComboRules cb)
        {
            list = l;
            backtrace = b;
            comboBehavior = cb;
            minLength = 0;
        }

        public PhoneticValues(string[] l, int b, ComboRules cb, int min)
        {
            list = l;
            backtrace = b;
            comboBehavior = cb;
            minLength = min;
        }
    };*/

    public static List<string> sounds = new List<string>(){
   "uh","ah","aa","eh","schwa","er","ih","ee","au","oo","uu","ai","ou","ei","oh","oi",

   "b","d","dzh","f","g","h","j","k","l","-l","m","n","-n","ng",
   "p","r","-r","s","sh","t","stop","tsh","thorn","theta","v","w","z","zh",

   "_",
        "y-wild", //stress = "ai" //unstress = "ih"
        "final-y-wild", //stress = "ai" //unstress = "ee"
        "final-i-wild", //stress = "ai" //unstress = "ee"
        "i-wild", //stress = "ai" //unstress = "ih"
        "u-wild", //stress = "uh" //unstress = "schwa"
        "o-wild", //stress = "au" //unstress = "schwa"
        "er-wild", //stress = "ei","er" //unstress = "er"
        "e-wild", //stress = "eh" //unstress = "schwa"
        "a-wild", //stress = "aa" //unstress = "schwa"
        "ei-wild", //stress = "ei" //unstress = "eh"
        "final-a-wild", //stress = "ah" //unstress = "schwa"
    };

    public static int nothing = 0;

    public static List<string> addPhonemes(string[] phonemes, int pos, int posFromEnd, string word, ComboRules rule, int minLength)
    {
        List<string> lol = null; //follow the rules//note:make this less repetitive
        if ((minLength >= 0 && minLength <= word.Length) || (minLength < 0 && -minLength >= word.Length))
        {
            switch(rule)
            {
                case ComboRules.All:
                    lol = new List<string>(phonemes);
                    break;
                case ComboRules.Middle:
                    if (pos != 0 && posFromEnd != 0)
                    {
                        lol = new List<string>(phonemes);
                    }
                    break;
                case ComboRules.Starting:
                    if (pos == 0)
                    {
                        lol = new List<string>(phonemes);
                    }
                    break;
                case ComboRules.NotStarting:
                    if (pos != 0)
                    {
                        lol = new List<string>(phonemes);
                    }
                    break;
                case ComboRules.Ending:
                case ComboRules.NotVoicedBeforeAndEnding:
                    if (posFromEnd == 0 || (posFromEnd == 1 && word.Substring(word.Length - 1) == "s" || (posFromEnd == 1 && word.Substring(word.Length - 1) == "d"))
                    || (posFromEnd == 2 && word.Substring(word.Length - 2) == "ed") || (posFromEnd == 2 && word.Substring(word.Length - 2) == "er")
                    || (posFromEnd == 2 && word.Substring(word.Length - 2) == "ly") || (posFromEnd == 3 && word.Substring(word.Length - 3) == "est")
                    || (posFromEnd == 1 && word.Substring(word.Length - 1) == "r") || (posFromEnd == 4 && word.Substring(word.Length - 4) == "ness")
                    || (posFromEnd == 2 && word.Substring(word.Length - 2) == "es") || (posFromEnd == 4 && word.Substring(word.Length - 4) == "ment")
                    || (posFromEnd == 4 && word.Substring(word.Length - 4) == "able") || (posFromEnd == 4 && word.Substring(word.Length - 4) == "ship"))
                    {
                        if (rule == ComboRules.NotVoicedBeforeAndEnding)
                        {
                            if (pos >= 1)
                            {
                                string succ = word.Substring(pos - 1, 1);
                                if (!isVoiced(succ))
                                {
                                    lol = new List<string>(phonemes);
                                }
                            }
                        }
                        else if (rule == ComboRules.VoicedBeforeAndEnding)
                        {
                            if (pos >= 1)
                            {
                                string succ = word.Substring(pos - 1, 1);
                                if (isVoiced(succ))
                                {
                                    lol = new List<string>(phonemes);
                                }
                            }
                        }
                        else
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    break;
                case ComboRules.NotEnding:
                    if (posFromEnd != 0 || (posFromEnd != 1 || word.Substring(word.Length - 1) != "s" || (posFromEnd != 1 || word.Substring(word.Length - 1) != "d"))
                    || (posFromEnd != 2 || word.Substring(word.Length - 2) != "ed") || (posFromEnd != 2 || word.Substring(word.Length - 2) != "er")
                    || (posFromEnd != 2 || word.Substring(word.Length - 2) != "ly") || (posFromEnd != 3 || word.Substring(word.Length - 3) != "est")
                    || (posFromEnd != 1 || word.Substring(word.Length - 1) != "r") || (posFromEnd != 4 || word.Substring(word.Length - 4) == "ness")
                    || (posFromEnd != 2 || word.Substring(word.Length - 2) != "es"))
                    {
                        lol = new List<string>(phonemes);
                    }
                    break;
                case ComboRules.VowelNext:
                    if (posFromEnd != 0)
                    {
                        if (!isConsonant(word.Substring(pos + 1, 1), out nothing))
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    break;
                case ComboRules.NoVowelsRemain:
                    if (posFromEnd != 0)
                    {
                        int nothing1 = 0;
                        string nothing2 = "";
                        if (nextVowelSound(pos + 1, word, out nothing1, out nothing2).Count > 0)
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    else
                    {
                        lol = new List<string>(phonemes);
                    }
                    break;
                case ComboRules.NoVowelNext:
                    if (posFromEnd != 0)
                    {
                        if (isConsonant(word.Substring(pos + 1, 1), out nothing))
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    break;
                case ComboRules.WholeWord:
                    if (posFromEnd == 0 && pos == 0)
                    {
                        lol = new List<string>(phonemes);
                    }
                    break;
                case ComboRules.EIYNext:
                    if (posFromEnd > 0)
                    {
                        string succ = word.Substring(pos + 1, 1);
                        if (succ == "e" || succ == "i" || succ == "y")
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    break;
                case ComboRules.NotEIYNext:
                    if (posFromEnd != 0)
                    {
                        string succ = word.Substring(pos + 1, 1);
                        if (succ != "e" && succ != "i" && succ != "y")
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    break;
                case ComboRules.VoicedBefore:
                    if (pos >= 1)
                    {
                        string succ = word.Substring(pos - 1, 1);
                        if (isVoiced(succ))
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    break;
                case ComboRules.NotVoicedBefore:
                    if (pos >= 1)
                    {
                        string succ = word.Substring(pos - 1, 1);
                        if (isVoiced(succ))
                        {
                            lol = new List<string>(phonemes);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        if (lol == null)
        {
            return new List<string>();
        }
        return lol;
    }

    public static char[] vowelComps = { 'a', 'e', 'i', 'o', 'u', 'y' };

    public static bool isConsonant(string ltr, out int findPos, int start = 0, bool fromLast = false)
    {

        if (fromLast)
        {
            for (int i = ltr.Length - 1; i >= start; --i)
            {
                for (int j = 0; j < vowelComps.Length; ++j)
                {
                    if (Convert.ToChar(ltr.Substring(i,1)) == vowelComps[j])
                    {
                        //we need to find the first vowel of this sequence. assume vowels only in clusters of 2?
                        if (i-1 >= start && !isConsonant(ltr.Substring(i-1,1), out nothing))
                        {
                            findPos = i - 1;
                        }
                        else
                        {
                            findPos = i;
                        }
                        return false;
                    }
                }
            }
        }
        else
        {
            for (int i = start; i < ltr.Length; ++i)
            {
                for (int j = 0; j < vowelComps.Length; ++j)
                {
                    if (Convert.ToChar(ltr.Substring(i, 1)) == vowelComps[j])
                    {
                        findPos = i;
                        return false;
                    }
                }
            }
        }
        
        findPos = -1;
        return true;
    }

    public static bool hasVowel(string ltr, out int findPos, int start = 0, bool fromLast = false)
    {
        bool res = !isConsonant(ltr, out findPos, start, fromLast);
        return res;
    }

    public static char[] voicelessComps = { 'c', 'f', 'i', 'k', 'o', 'p', 'q', 's', 't', 'u', 'x' };

    public static bool isVoiced(string ltr)
    {
        char[] ltrc = ltr.ToCharArray();

        for (int i = 0; i < ltr.Length; ++i)
        {
            for (int j = 0; j < voicelessComps.Length; ++j)
            {
                if (ltrc[i] == voicelessComps[j])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static List<string> nextVowelSound(int pos, string word, out int howFar, out string inBetween) //finds out how to pronounce the next vowels or null if can't find them
    {
        List<string> phonemes = null;
        int vpos = -1;
        if (hasVowel(word, out vpos, pos + 1))
        {
            //string vowelFound = word.Substring(vpos,1);
            howFar = vpos - pos;
            inBetween = word.Substring(pos, howFar - 1);
            int? skip; // don't use this sorry
            phonemes = letter(word, vpos, out skip);
            if (phonemes.Count == 0)
            {
                phonemes.Add("UNDEFINED!");
            }
            return phonemes;
        }
        else
        {
            howFar = -1;
            inBetween = "";
            return new List<string>();
        }
    }
    public static List<string> prevVowelSound(int pos, string word, out int howFarBack, out string inBetween)
    {
        if (pos > 0)
        {
            List<string> phonemes = null;
            int vpos = -1;
            if (hasVowel(word.Substring(0,pos), out vpos, 0, true))
            {
                //string vowelFound = m.Captures[m.Captures.Count - 1].ToString();
                howFarBack = pos - vpos;
                inBetween = word.Substring(0, howFarBack - 1);
                int? skip; // u know, the usual
                phonemes = letter(word, vpos, out skip);
                if (phonemes.Count == 0)
                {
                    phonemes.Add("UNDEFINED!");
                }
                return phonemes;
            }
            else
            {
                howFarBack = -1;
                inBetween = "";
                return new List<string>();
            }
        }
        else
        {
            howFarBack = -1;
            inBetween = "";
            return new List<string>();
        }
    }

    public static void comboStuff(ref SortedDictionary<string, object[]> combos, out bool found, ref int pos, ref string word, out int newSkip, ref List<string> newPhon)
    {
        int longest = 0; /*int tempskip = 0;*/
        //bool found = false;
        List<string> add = null;
        found = false;
        newSkip = 0;
        foreach (KeyValuePair<string, object[]> pair in combos)
        {
            string key = pair.Key.Split(' ')[0];
            string[] phons = pair.Value[0] as string[];
            int backtrace = 0;
            if (pair.Value.Length > 1)
            {
                backtrace = (int)pair.Value[1];
            }
            int len = key.Length;
            string test = "";
            if (pos + len < word.Length)
            {
                test = word.Substring(pos, len);
            }
            else
            {
                test = word.Substring(pos);
            }
            if (test == key && longest <= len)
            {
                ComboRules cr = ComboRules.All;
                if (pair.Value.Length > 2)
                {
                    cr = (ComboRules)pair.Value[2];
                }
                int minlen = 0;
                if (pair.Value.Length > 3)
                {
                    minlen = (int)pair.Value[3];
                }
                List<string> newadd = addPhonemes(phons, pos, word.Length - len - pos, word, cr, minlen);
                if (newadd.Count != 0)
                {
                    add = newadd;
                    longest = len;
                    found = true;
                    newSkip = backtrace + len - 1;
                }
            }
        }
        
        if (found && add != null)
        {
            newPhon = add;
        }
    }

    public static SortedDictionary<string, object[]>[] comboMaker =
    {
        new SortedDictionary<string, object[]>()
        {
            {"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},

            {"aa", new object[]{new string[]{"ah"}}},
            {"ak", new object[]{new string[]{"aa","k"}}},
            {"act", new object[]{new string[]{"aa","k","t"}}},
            {"ae", new object[]{new string[]{"ei"},0,ComboRules.NotStarting}},
            {"ae 2", new object[]{new string[]{"eh"},0,ComboRules.Starting}},
            {"ah", new object[]{new string[]{"ah"}}},

            {"able", new object[]{new string[]{"uh","b","schwa","l"},0,ComboRules.Ending,8}},
            {"ach", new object[]{new string[]{"ei","k"},0,ComboRules.Starting}},

            {"ai", new object[]{new string[]{"ai"},0,ComboRules.Starting}},
            {"ais", new object[]{new string[]{"ai"},0,ComboRules.Starting}},
            {"ai 2", new object[]{new string[]{"ei"},0,ComboRules.Middle}},
            {"ai 3", new object[]{new string[]{"ai"},0,ComboRules.Ending}},

            {"allow", new object[]{new string[]{"schwa","l","ou"}}},
            {"al", new object[]{new string[]{"schwa","l"},0,ComboRules.Ending}},
            {"alf", new object[]{new string[]{"aa","f"},0,ComboRules.Ending}},
            {"alv", new object[]{new string[]{"aa","v"},0,ComboRules.Ending}},
            {"alf 2", new object[]{new string[]{"aa","-l","f"},0,ComboRules.NotEnding}},
            {"alv 2", new object[]{new string[]{"aa","-l","v"},0,ComboRules.NotEnding}},
            {"alph", new object[]{new string[]{"aa","-l","f"}}},
            {"age", new object[]{new string[]{"eh","dzh"},0,ComboRules.Ending}},
            {"ager", new object[]{new string[]{"eh","dzh","er"},0,ComboRules.Ending}},

            {"all", new object[]{new string[]{"au","-l"},0,ComboRules.WholeWord}},
            {"all 2", new object[]{new string[]{"au","-l"},0,ComboRules.NotStarting}},
            {"alk", new object[]{new string[]{"ah","k"},0,ComboRules.Ending}},
            {"ake", new object[]{new string[]{"ei","k"},0,ComboRules.Ending}},
            {"aker", new object[]{new string[]{"ei","k","er"},0,ComboRules.Ending}},
            {"aked", new object[]{new string[]{"ei","k","d"},0,ComboRules.Ending}},
            {"aking", new object[]{new string[]{"ei","k","ee","ng"},0,ComboRules.Ending}},
            {"ali", new object[]{new string[]{"ei","l","ee"},0,ComboRules.Starting}},
            {"alm", new object[]{new string[]{"au","-l","m"}}},//"-l" is dark-l sound
            {"alt", new object[]{new string[]{"au","-l","t"}}},
            {"and", new object[]{new string[]{"aa","n","d"}}},
            {"an 3", new object[]{new string[]{"aa","n"},0,ComboRules.WholeWord}},
            {"an 2", new object[]{new string[]{"eh","n"},0,ComboRules.Ending,5}},//minimum word length is 5 letters!
            {"analog", new object[]{new string[]{"schwa","n","aa","l","schwa","dzh"},0,ComboRules.NotEnding}},
            {"ana", new object[]{new string[]{"aa","n","schwa"},0,ComboRules.Starting}},
            {"an 1", new object[]{new string[]{"ei","n"},0,ComboRules.Starting}},
            {"ank", new object[]{new string[]{"ei","n","k"},0,ComboRules.Ending}},
            {"ant", new object[]{new string[]{"aa","n","t"},0,ComboRules.Starting}},
            {"any", new object[]{new string[]{"eh","n","ee"},0,ComboRules.Starting}},
            {"anyone", new object[]{new string[]{"eh","n","ee","w","uh","n"},0,ComboRules.Starting}},
            {"ange", new object[]{new string[]{"ei","n","dzh"},-1}},

            {"ao", new object[]{new string[]{"ah","oh"}}},
            {"aos", new object[]{new string[]{"ei","ah","s"},0,ComboRules.Ending}},
            {"asis", new object[]{new string[]{"ei","s","ih","s"},0,ComboRules.Ending}},

            {"ar", new object[]{new string[]{"ah","-r"},0,ComboRules.NoVowelNext}},
            {"ar 2", new object[]{new string[]{"ei","-r"},0,ComboRules.VowelNext}},

            {"aste", new object[]{new string[]{"ei","s","t"},0,ComboRules.Ending}},
            {"asted", new object[]{new string[]{"aa","s","t","eh","d"},0,ComboRules.Ending}},
            {"aster", new object[]{new string[]{"aa","s","t","er"}}},
            {"ation", new object[]{new string[]{"ei","sh","schwa","n"},0,ComboRules.Ending}},

            {"ath", new object[]{new string[]{"ah","thorn"},0,ComboRules.NotEnding}},
            {"ather", new object[]{new string[]{"aa","thorn","er"}}},
            {"ath 2", new object[]{new string[]{"aa","theta"},0,ComboRules.Ending}},
            {"atr", new object[]{new string[]{"ei","tch","r"}}},

            {"au", new object[]{new string[]{"au"}}},
            {"augh", new object[]{new string[]{"au"}}}, // exception words like "laugh" start
            {"aw", new object[]{new string[]{"au"}}},
            {"away", new object[]{new string[]{"schwa","w","ei"}}},
            {"awe", new object[]{new string[]{"au"}}},

            {"ay", new object[]{new string[]{"ei"}}}, //says who?
            {"ayer", new object[]{new string[]{"ei","er"}}},
            {"ayor", new object[]{new string[]{"ei","er"}}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},

            {"bought", new object[]{new string[]{"b","au","t"}}},
            {"borough", new object[]{new string[]{"b","oh","r","oh"}}},
            {"brought", new object[]{new string[]{"b","r","au","t"}}},
            {"bough", new object[]{new string[]{"b","ou"}}},
            {"bowl", new object[]{new string[]{"b","oh","-l"}}},

            {"bb", new object[]{new string[]{"b"}}}, //make sure not to be dumb
            {"broth", new object[]{new string[]{"b","r","au","theta"},0,ComboRules.Ending}},

            {"bear", new object[]{new string[]{"b","ei","-r"},0,ComboRules.WholeWord}},
            {"beau", new object[]{new string[]{"b","j","uu"},0,ComboRules.Starting}},
            {"been", new object[]{new string[]{"b","ih","n"},0,ComboRules.WholeWord}},

            {"begin", new object[]{new string[]{"b","ee","g","eh","n"}}},

            {"bio", new object[]{new string[]{"b","ai","oh"},0,ComboRules.Starting}},

            {"blood", new object[]{new string[]{"b","l","uh","d"}}},
            {"broad", new object[]{new string[]{"b","r","au","d"}}},

            {"break", new object[]{new string[]{"b","r","ei","k"}}},
            {"bt", new object[]{new string[]{"b"},0,ComboRules.Ending}},

            {"bureau", new object[]{new string[]{"b","j","er","ee","au"},0,ComboRules.Starting}},
            {"bureau 2", new object[]{new string[]{"b","j","er","oh"},0,ComboRules.WholeWord}},

            {"buffet", new object[]{new string[]{"b","schwa","f","ei"},0,ComboRules.WholeWord}},

        },
        new SortedDictionary<string, object[]>()
        {
            {"cough", new object[]{new string[]{"k","au","f"}}},

            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"canoe", new object[]{new string[]{"k","schwa","n","uu"},0,ComboRules.Ending}},

            {"chamois", new object[]{new string[]{"sh","aa","m","ee"},0,ComboRules.WholeWord}},
            {"cae", new object[]{new string[]{"s","ee"},0,ComboRules.Starting}},
            {"cial", new object[]{new string[]{"sh","schwa","l"},0,ComboRules.Ending}},
            {"cian", new object[]{new string[]{"sh","schwa","n"},0,ComboRules.Ending}},
            {"ciate", new object[]{new string[]{"sh","ee","ei","stop"},0,ComboRules.Ending}},
            {"cn", new object[]{new string[]{"n"},0,ComboRules.Starting}},
            {"ct", new object[]{new string[]{"t"},0,ComboRules.Starting}},

            {"cc", new object[]{new string[]{"k","s"},0,ComboRules.EIYNext}},
            {"cc 2", new object[]{new string[]{"k"},0,ComboRules.NotEIYNext}},

            {"cei", new object[]{new string[]{"s","ee"}}},
            {"cello", new object[]{new string[]{"tsh","eh","l","oh"},0,ComboRules.Ending}},
            {"celli", new object[]{new string[]{"tsh","eh","l","ee"},0,ComboRules.Ending}},

            {"ch", new object[]{new string[]{"tsh"}}},
            {"clow", new object[]{new string[]{"k","l","ou"}}},
            {"ch 2", new object[]{new string[]{"tsh"}}},

            {"chao", new object[]{new string[]{"k","ei","au"}}},
            {"comp", new object[]{new string[]{"k","au","m"},-1}},
            {"come", new object[]{new string[]{"k","uh","m"},0,ComboRules.Ending}},
            {"com", new object[]{new string[]{"k","uh","m"},0,ComboRules.Ending}},
            {"comb", new object[]{new string[]{"k","oh","m"},0,ComboRules.Ending}},
            {"com 2", new object[]{new string[]{"k","final-a-wild","m"}}},
            {"chasm", new object[]{new string[]{"k","aa","s","m"}}},
            {"chor", new object[]{new string[]{"k","oh","-r"}}},
            {"choir", new object[]{new string[]{"k","oh","ai","er"}}},
            {"chiro", new object[]{new string[]{"k","ai","r","oh"}}},
            {"cious", new object[]{new string[]{"sh","schwa","s"},0,ComboRules.Ending}},

            {"cleans", new object[]{new string[]{"k","l","eh","n","s"},0,ComboRules.Starting}},
            {"cleans 2", new object[]{new string[]{"k","l","ee","n","s"},0,ComboRules.WholeWord}},

            {"ck", new object[]{new string[]{"k"}}},

            {"coopera", new object[]{new string[]{"k","oh","au","p","er","ei"}}},
            {"coordin", new object[]{new string[]{"k","oh","oh","er","d","ih","n"}}},
            {"courage", new object[]{new string[]{"k","schwa","r","ei-wild","dzh"},-1}},
            {"coo", new object[]{new string[]{"k","uu"}}},
            {"con", new object[]{new string[]{"k","o-wild","n"}}},
            {"countr", new object[]{new string[]{"k","uh","n"},-2}},
            {"coupon", new object[]{new string[]{"k","j","uu","p","au","n"}}},
            {"crouton", new object[]{new string[]{"k","r","uu","t","au","n"}}},
            {"coyote", new object[]{new string[]{"k","ai","oh","t","ee"}}},

            {"cqu", new object[]{new string[]{"k","w"}}},
            {"cy", new object[]{new string[]{"s","ai"},0,ComboRules.Starting}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"dought", new object[]{new string[]{"d","au","t"}}},
            {"dough", new object[]{new string[]{"d","oh"}}},
            {"drough", new object[]{new string[]{"dzh","r","ou"}}},

            {"doable", new object[]{new string[]{"d","uu","uh","b","schwa","l"},0,ComboRules.Ending}},
            {"do", new object[]{new string[]{"d","uu"},0,ComboRules.WholeWord}},
            {"done", new object[]{new string[]{"d","uh","n"},0,ComboRules.WholeWord}},
            {"dow", new object[]{new string[]{"d","ou"}}},
            {"does", new object[]{new string[]{"d","uh","z"},0,ComboRules.Ending}},
            {"doer", new object[]{new string[]{"d","uu","er"},0,ComboRules.Ending}},
            {"doeth", new object[]{new string[]{"d","uu","eh","theta"},0,ComboRules.Ending}},
            {"doing", new object[]{new string[]{"d","uu","ee","ng"},0,ComboRules.Ending}},
            {"doe", new object[]{new string[]{"d","oh"},0,ComboRules.Starting}},

            {"dd", new object[]{new string[]{"d"}}},
            {"disown", new object[]{new string[]{"d","ih","s","oh","n"}}},
            {"ded", new object[]{new string[]{"d","eh","d"},0,ComboRules.Ending}},
            {"dh", new object[]{new string[]{"d"}}},
            {"death", new object[]{new string[]{"d","eh","theta"}}},
            {"dead", new object[]{new string[]{"d","eh","d"}}},
            {"die", new object[]{new string[]{"d","ai"}}},
            {"dis", new object[]{new string[]{"d","ih","s"}}},
            {"dys", new object[]{new string[]{"d","ih","s"}}},
            {"dia", new object[]{new string[]{"d","ai"},-1,ComboRules.Starting}},
            {"dg", new object[]{new string[]{"dzh"},0,ComboRules.EIYNext}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"egoist", new object[]{new string[]{"ee","g","oh","ih","s","t"},0,ComboRules.WholeWord}},

            {"eation", new object[]{new string[]{"ee","ei","sh","schwa","n"}}},
            {"enough", new object[]{new string[]{"eh","n","uh","f"}}},
            {"ead", new object[]{new string[]{"ee","d"},0,ComboRules.Ending}},
            {"eat", new object[]{new string[]{"ee","t"},0,ComboRules.Ending}},
            {"eated", new object[]{new string[]{"ee","t","eh","d"},0,ComboRules.Ending}},
            {"each", new object[]{new string[]{"ee","tsh"}}},
            {"ean", new object[]{new string[]{"ee","n"}}},
            {"ear", new object[]{new string[]{"ee","-r"}}},
            {"engin", new object[]{new string[]{"eh","n","dzh","ih","n"}}},
            {"earl 2", new object[]{new string[]{"er","l"},0,ComboRules.WholeWord}},
            {"earl", new object[]{new string[]{"er","l"}}},
            {"eak", new object[]{new string[]{"ee","k"}}},
            {"eas", new object[]{new string[]{"ee","s"}}},
            {"eas 2", new object[]{new string[]{"ee","z"},0,ComboRules.Starting}},
            {"eav", new object[]{new string[]{"ee","v"}}},
            {"eal", new object[]{new string[]{"ee","l"},0,ComboRules.Ending}},
            {"eal 2", new object[]{new string[]{"eh","l"},0,ComboRules.NotEnding}},
            {"ea", new object[]{new string[]{"ee"},0,ComboRules.VoicedBefore}},
            {"ea 2", new object[]{new string[]{"eh"},0,ComboRules.NotVoicedBefore}},
            {"ea 3", new object[]{new string[]{"ee","schwa"},0,ComboRules.Ending}},

            {"eaux", new object[]{new string[]{"oh"},0,ComboRules.Ending}}, //WHY \(^o^)/
            {"eau", new object[]{new string[]{"oh"},0,ComboRules.Ending}},

            {"east", new object[]{new string[]{"ee","s","t"}}},

            {"ed", new object[]{new string[]{"d"},0,ComboRules.VoicedBeforeAndEnding}},
            {"ed 2", new object[]{new string[]{"t"},0,ComboRules.NotVoicedBeforeAndEnding}},

            {"eer", new object[]{new string[]{"ee","-r"}}},
            {"eel", new object[]{new string[]{"ee","-l"}}},
            {"ee", new object[]{new string[]{"ee"}}},

            {"eigh", new object[]{new string[]{"ei"}}},

            {"eign", new object[]{new string[]{"ei","n"},0,ComboRules.Ending,-6}},
            {"eign 2", new object[]{new string[]{"ih","n"},0,ComboRules.Ending,7}},

            {"ei", new object[]{new string[]{"ai"},0,ComboRules.Starting}},
            {"ei 2", new object[]{new string[]{"ei"},0,ComboRules.NotStarting}},
            {"eit", new object[]{new string[]{"ih","stop"},0,ComboRules.Ending}},
            {"eight", new object[]{new string[]{"ai","t"},0,ComboRules.Ending}},
            {"eight 2", new object[]{new string[]{"ei","t"},0,ComboRules.WholeWord}},
            {"ei 3", new object[]{new string[]{"ei"},0,ComboRules.NotStarting}},

            {"eo", new object[]{new string[]{"ee","oh"}}},
            {"eo 2", new object[]{new string[]{"ee","au"},0,ComboRules.Starting}},
            {"eow", new object[]{new string[]{"ee","ou"}}}, //I DOUBT IT

            {"err", new object[]{new string[]{"eh","r"},0,ComboRules.Starting}},
            {"er", new object[]{new string[]{"ei","r"},0,ComboRules.Starting}},
            {"er 2", new object[]{new string[]{"er"}}},

            {"ese", new object[]{new string[]{"ee","z"},0,ComboRules.Ending}},

            {"eu", new object[]{new string[]{"uu"}}},
            {"eum", new object[]{new string[]{"ee","schwa","m"},0,ComboRules.Ending}},
            {"eus", new object[]{new string[]{"ee","schwa","s"},0,ComboRules.Ending}},
            {"ever", new object[]{new string[]{"eh","v","er"},0}},
            {"eve", new object[]{new string[]{"ee","v"},-1}},
            {"evil", new object[]{new string[]{"ee","v","ih","-l"},-1,ComboRules.Starting}},
            {"ew", new object[]{new string[]{"uu"}}},

            {"ey", new object[]{new string[]{"ai"},0,ComboRules.NotEnding}},
            {"ey 2", new object[]{new string[]{"final-ee-wild"},0,ComboRules.Ending}},

            {"e", new object[]{new string[]{"silent-e"},0,ComboRules.Ending,3}}, //Rule -1
            {"e 2", new object[]{new string[]{"ee"},0,ComboRules.Ending,-2}}, //Rule -1
        },
        new SortedDictionary<string, object[]>()
        {
            {"far", new object[]{new string[]{"f","ah","-r"}}},
            {"fak", new object[]{new string[]{"f","ei","k"}}},
            {"fough", new object[]{new string[]{"f","au"}}},
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"father", new object[]{new string[]{"f","ah","thorn","er"}}},

            {"feath", new object[]{new string[]{"f","eh","thorn"},0,ComboRules.Starting}},
            {"feat", new object[]{new string[]{"f","ee","t"},0,ComboRules.Starting}},
            {"fear", new object[]{new string[]{"f","ee","-r"}}},
            {"feud", new object[]{new string[]{"f","j","uu","d"}}},
            {"few", new object[]{new string[]{"f","j","uu"},0,ComboRules.WholeWord}},
            {"fiery", new object[]{new string[]{"f","ai","er","ee"},0,ComboRules.WholeWord}},

            {"ff", new object[]{new string[]{"f"}}},
            {"fj", new object[]{new string[]{"f","j"}}},
            {"flour", new object[]{new string[]{"f","l","ou","er"},0,ComboRules.Ending}},
            {"foyer", new object[]{new string[]{"f","oh","ee","j","ei"}}},

            {"flood", new object[]{new string[]{"f","l","uh","d"}}},
            {"flourish", new object[]{new string[]{"f","l","er","ih","sh"}}},

            {"foot", new object[]{new string[]{"f","oo","t"}}},
            {"for", new object[]{new string[]{"f","oh","-r"},0,ComboRules.Starting}},
            {"fore", new object[]{new string[]{"f","oh","-r"},-1,ComboRules.Starting}},
            {"foul", new object[]{new string[]{"f","ou","-l"}}},

            {"fruits", new object[]{new string[]{"f","r","uu","stop","s"},0,ComboRules.Ending}},
            {"fruity", new object[]{new string[]{"f","r","uu","stop","ee"},0,ComboRules.Ending}},
            {"fruit", new object[]{new string[]{"f","r","uu","t"},0,ComboRules.NotStarting}},

            {"fajita", new object[]{new string[]{"f","schwa","h","ee","t","schwa"},0,ComboRules.WholeWord}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"ghettoise", new object[]{new string[]{"g","eh","t","oh","ai","z"},0,ComboRules.WholeWord}},
            {"good", new object[]{new string[]{"g","oo","d"}}},
            {"golf", new object[]{new string[]{"g","au","l","f"}}},

            {"ger", new object[]{new string[]{"dzh","er"},0,ComboRules.Ending}},
            {"ge", new object[]{new string[]{"dzh"},0,ComboRules.Ending}},
            {"get", new object[]{new string[]{"g","eh","t"}}},
            {"gett", new object[]{new string[]{"g","eh","t"}}},
            {"ge 2", new object[]{new string[]{"dzh"},-1,ComboRules.NotEnding}},
            {"geo", new object[]{new string[]{"dzh","ee","au"},0,ComboRules.Starting}},
            {"gey", new object[]{new string[]{"g","ai"},0,ComboRules.Starting}},
            {"ggi", new object[]{new string[]{"dzh","ee"},0,ComboRules.VowelNext}},
            {"gg", new object[]{new string[]{"g"}}},
            {"gh", new object[]{new string[]{"g"}}},

            {"giv", new object[]{new string[]{"g","ih","v"}}},

            {"gm", new object[]{new string[]{"m"},0,ComboRules.Ending}},
            {"gn", new object[]{new string[]{"n"},0,ComboRules.Starting}},

            {"gradu", new object[]{new string[]{"g","r","aa","dzh","j","uu"}}},
            {"gratu", new object[]{new string[]{"g","r","aa","t","j","uu"}}},
            {"gration", new object[]{new string[]{"g","r","ei","sh","schwa","n"}}},
            {"great", new object[]{new string[]{"g","r","ei","stop"}}},

            {"gu", new object[]{new string[]{"g","w"},0,ComboRules.VowelNext}},
            {"gu 2", new object[]{new string[]{"g","uh"},0,ComboRules.NoVowelNext}},
            {"gu 3", new object[]{new string[]{"g"},0,ComboRules.Starting}},

            {"gui", new object[]{new string[]{"g","ai"}}},
            {"gui 2", new object[]{new string[]{"g","w","ih"},0,ComboRules.NoVowelsRemain}},


            {"gy", new object[]{new string[]{"dzh","ih"},0,ComboRules.NotEnding}},
            {"gy 2", new object[]{new string[]{"dzh","ee"},0,ComboRules.Ending}},

        },
        new SortedDictionary<string, object[]>()
        {
            {"have", new object[]{new string[]{"h","aa","v"},0,ComboRules.WholeWord}},
            {"hy", new object[]{new string[]{"h","ai"},0,ComboRules.Starting}},

            {"heard", new object[]{new string[]{"h","er","d"},0,ComboRules.WholeWord}},
            {"head", new object[]{new string[]{"h","eh","d"}}},
            {"hear", new object[]{new string[]{"h","au","-r"},0,ComboRules.Starting}},
            {"hear 2", new object[]{new string[]{"h","ee","-r"},0,ComboRules.WholeWord}},
            {"here", new object[]{new string[]{"h","ee","-r"},0,ComboRules.WholeWord}},
            {"here's", new object[]{new string[]{"h","ee","-r","z"},0,ComboRules.WholeWord}},
            {"hous", new object[]{new string[]{"h","ou","s"},0,ComboRules.Starting}},

            {"hero", new object[]{new string[]{"h","ee","-r","oh"},0,ComboRules.Starting}},
            {"her", new object[]{new string[]{"h","ei","-r"},0,ComboRules.Starting}},
            {"her 2", new object[]{new string[]{"h","er"},0,ComboRules.WholeWord}},

            {"honor", new object[]{new string[]{"au","n","er"}}},
            {"honest", new object[]{new string[]{"au","n","eh","s","t"}}},

            {"hier", new object[]{new string[]{"h","ai","er"},0,ComboRules.Starting}},

            {"hour", new object[]{new string[]{"ou","er"},0,ComboRules.Starting}},
        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"ia", new object[]{new string[]{"ee"},-1}},
            {"iation", new object[]{new string[]{"ee","ei","sh","schwa","n"}}},
            {"ict", new object[]{new string[]{"ih","k","t"}}},

            {"ied", new object[]{new string[]{"ai","d"},0,ComboRules.Ending}},
            {"ien", new object[]{new string[]{"eh","n"}}},
            {"iel", new object[]{new string[]{"ee","schwa","l"}}},
            {"ier", new object[]{new string[]{"ee","-r"}}},
            {"ies", new object[]{new string[]{"ee","z"},0,ComboRules.Ending}},
            {"ieu", new object[]{new string[]{"ee","uu"},0,ComboRules.Ending}},
            {"ieve", new object[]{new string[]{"ee","v"},0,ComboRules.Ending}},
            {"iew", new object[]{new string[]{"ee","uu"},0,ComboRules.Ending}},

            {"igh", new object[]{new string[]{"ai"}}},
            {"ign", new object[]{new string[]{"ai","n"},0,ComboRules.Ending}},
            {"ild", new object[]{new string[]{"ai","schwa","l","d"},0,ComboRules.Ending}},
            {"ind", new object[]{new string[]{"ai","n","d"},0,ComboRules.Ending}},
            {"ing", new object[]{new string[]{"ee","ng"},0,ComboRules.Ending}},
            {"ing 2", new object[]{new string[]{"ee","ng","g"},0,ComboRules.NotEnding}},
            {"iness", new object[]{new string[]{"ee","n","eh","s"},0,ComboRules.Ending}},

            {"ii", new object[]{new string[]{"ee","ai"}}},
            {"ire", new object[]{new string[]{"ai","er"},-1}},
            {"ir", new object[]{new string[]{"er"},0,ComboRules.NoVowelNext}},
            {"ir 2", new object[]{new string[]{"ai","r"},0,ComboRules.VowelNext}},
            {"into", new object[]{new string[]{"ih","n","t","uu"},0,ComboRules.WholeWord}},
            {"io", new object[]{new string[]{"ee","oh"},-1}},
            {"io 2", new object[]{new string[]{"ee","oh"},0,ComboRules.Ending}},
            {"ion", new object[]{new string[]{"ih","n"},0,ComboRules.Ending}},
            {"isl", new object[]{new string[]{"ai","-l"}}},
             {"ism", new object[]{new string[]{"ih","s","schwa","m"},0,ComboRules.Ending}},
             {"ity", new object[]{new string[]{"ih","stop","ee"},0,ComboRules.Ending}},
            {"iu", new object[]{new string[]{"ee","schwa"}}},
            {"izz", new object[]{new string[]{"ih","z"}}},
            {"iz", new object[]{new string[]{"ai","z"}}},

            {"ive", new object[]{new string[]{"ai","v"},0,ComboRules.Ending}},

            {"idiot", new object[]{new string[]{"ih","d","ee","schwa","t"},0,ComboRules.Ending}},
        },
        new SortedDictionary<string, object[]>()
        {
            //NOTHING
            {"jalapeno", new object[]{new string[]{"h","aa","l","schwa","p","ei","n","j","oh"},0,ComboRules.WholeWord}},
            {"jeopar", new object[]{new string[]{"dzh","eh","p","er"}}},

            {"juan", new object[]{new string[]{"h","w","ah","n"}}},
            {"jui", new object[]{new string[]{"j","uu"},0,ComboRules.Starting}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"knowledg", new object[]{new string[]{"n","au","l","eh","dzh"}}},
            {"koala", new object[]{new string[]{"k","oh","au","-l","final-a-wild"}}},
            {"kk", new object[]{new string[]{"k"}}},
            {"khal", new object[]{new string[]{"k","aa","l"}}},
            {"kook", new object[]{new string[]{"k","uu","k"}}},
            {"kn", new object[]{new string[]{"n"}/*,0,ComboRules.Starting*/}},
            {"kh", new object[]{new string[]{"k"},0,ComboRules.NotEnding}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"logy", new object[]{new string[]{"l","schwa","dzh","ee"}}},
            {"lough", new object[]{new string[]{"l","oh"}}},

            {"live", new object[]{new string[]{"l","ih","v"},-1}},
            {"lives", new object[]{new string[]{"l","ai","v","z"},0,ComboRules.WholeWord}},

            {"laugh", new object[]{new string[]{"l","aa","f"}}},//while you can
             {"learn", new object[]{new string[]{"l","er","n"}}},//while you can
             {"leath", new object[]{new string[]{"l","eh","thorn"}}},
            {"leopar", new object[]{new string[]{"l","eh","p","er"}}},
            {"leth", new object[]{new string[]{"l","ee","theta"}}},
            {"let", new object[]{new string[]{"l","ei"},0,ComboRules.Ending}},
            {"lie", new object[]{new string[]{"l","ai"}}},
            {"ll", new object[]{new string[]{"l"}}},
            {"lve", new object[]{new string[]{"-l","v"},0,ComboRules.Ending}}, //wolves, halves
            {"le", new object[]{new string[]{"schwa","l"},0,ComboRules.Ending}},

            {"llow", new object[]{new string[]{"l","oh"}}},
            {"low", new object[]{new string[]{"l","oh"}}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},

            {"machin", new object[]{new string[]{"m","schwa","sh","ee","n"}}},
            {"maybe", new object[]{new string[]{"m","ei","b","ee"}}},

            {"mm", new object[]{new string[]{"m"}}},
            {"mb", new object[]{new string[]{"m"},0,ComboRules.Ending}},
            {"men", new object[]{new string[]{"m","eh","n"}}},
            {"monk", new object[]{new string[]{"m","uh","n","k"}}},

            {"minute", new object[]{new string[]{"m","ih","n","ih","t"},0,ComboRules.WholeWord}},
            {"mis", new object[]{new string[]{"m","ih","s"},0,ComboRules.NoVowelNext}},

            {"mn", new object[]{new string[]{"m"},0,ComboRules.Ending}},
            {"mn 2", new object[]{new string[]{"n"},0,ComboRules.Starting}},
            {"micro", new object[]{new string[]{"m","ai","k","r","oh"},0,ComboRules.Starting}},
            {"mega", new object[]{new string[]{"m","eh","g","schwa"},0,ComboRules.Starting}},

            {"mov", new object[]{new string[]{"m","uu","v"},0,ComboRules.NotEnding}},

            {"many", new object[]{new string[]{"m","eh","n","ee"},0,ComboRules.WholeWord}},
            //bt
        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"nough", new object[]{new string[]{"n","au"}}},

            {"noone", new object[]{new string[]{"n","oh","w","uh","n"}}},
            {"nobod", new object[]{new string[]{"n","oh","b","uh","d"}}},
            {"number", new object[]{new string[]{"n","uh","m","b","er"}}},
            {"nk", new object[]{new string[]{"ng","k"}}},
            {"nn", new object[]{new string[]{"n"}}},
            {"nine", new object[]{new string[]{"n","ai","n"}}},
            {"ninth", new object[]{new string[]{"n","ai","n","theta"}}},
            {"nsion", new object[]{new string[]{"n","sh","schwa","n"},0,ComboRules.Ending}},
            {"ng", new object[]{new string[]{"ng"},0,ComboRules.Ending}},
            {"ng 2", new object[]{new string[]{"ng","g"},0,ComboRules.NotEnding}},
            {"nucle", new object[]{new string[]{"n","uu","k","l","ee"}}},
            {"national", new object[]{new string[]{"n","aa","sh","schwa","n"},-2}},

        },
        new SortedDictionary<string, object[]>()
        {
            {"oroide", new object[]{new string[]{"oh","r","oh","ai","d"},0,ComboRules.WholeWord}},

            {"oa", new object[]{new string[]{"oh","ei"}}},
            {"oa 2", new object[]{new string[]{"oh"},0,ComboRules.NoVowelsRemain}},
            {"oa 3", new object[]{new string[]{"oh","a-wild"},0,ComboRules.Ending}},

            {"oel", new object[]{new string[]{"oh","eh","-l"}}},
            {"oet", new object[]{new string[]{"oh","eh"},-1}},
            {"oer", new object[]{new string[]{"oh","eh"},-1}},
            {"oe", new object[]{new string[]{"ee"}}},
            {"oe 2", new object[]{new string[]{"oh"},0,ComboRules.Ending}},

            {"of", new object[]{new string[]{"uh","v"},0,ComboRules.WholeWord}},

            {"oh", new object[]{new string[]{"oh"}}},

            {"oic", new object[]{new string[]{"oh","ih","k"},0,ComboRules.Ending}},
            {"oif", new object[]{new string[]{"w","ah","f"}}},
            {"ois", new object[]{new string[]{"ih","s"}}},
            {"oiss", new object[]{new string[]{"ih","s"}}},
            {"oise", new object[]{new string[]{"ih","s"},-1,ComboRules.Ending}},
            {"oir", new object[]{new string[]{"w","ah"}}},
            {"ost", new object[]{new string[]{"oh","s","t"},0,ComboRules.Ending}},
            {"oi", new object[]{new string[]{"oi"}}},

            {"old", new object[]{new string[]{"oh","-l","d"}}},
            {"olf", new object[]{new string[]{"oh","-l","f"}}},
            {"olves", new object[]{new string[]{"oh","-l","v","z"}}},
            {"omb", new object[]{new string[]{"au","m"},0,ComboRules.Ending}},
            {"om", new object[]{new string[]{"uh","m"},0,ComboRules.Ending}},
            {"once", new object[]{new string[]{"w","uh","n","s"},0,ComboRules.WholeWord}},
            {"onge", new object[]{new string[]{"uh","n","dzh"},0,ComboRules.Ending}},
            {"one", new object[]{new string[]{"w","uh","n"},0,ComboRules.Starting}},
            {"on", new object[]{new string[]{"o-wild","n"},0,ComboRules.Ending}},
            {"on 2", new object[]{new string[]{"oh","n"},0,ComboRules.Starting}},
            // o at beginning of word is usually "au"

            {"ood", new object[]{new string[]{"uu","d"}}},
            {"ook", new object[]{new string[]{"oo","k"}}},
            {"oom", new object[]{new string[]{"uu","m"}}},
            {"oon", new object[]{new string[]{"uu","n"}}},
            {"oot", new object[]{new string[]{"uu"},-1}},
            {"oor", new object[]{new string[]{"oh","-r"}}},
            {"oo", new object[]{new string[]{"uu"}}},

            {"oper", new object[]{new string[]{"au","p","er"},0,ComboRules.Starting}},
            {"op", new object[]{new string[]{"oh","p"},0,ComboRules.Starting}},
            {"op 2", new object[]{new string[]{"schwa","p"},0,ComboRules.Ending}},
            {"or", new object[]{new string[]{"er"},0,ComboRules.Ending}},
            {"or 2", new object[]{new string[]{"oh","-r"},0,ComboRules.NotEnding}},
            {"other", new object[]{new string[]{"uh","thorn","er"}}},
            {"oth", new object[]{new string[]{"oh","theta"},0,ComboRules.Ending}},

            {"ouch", new object[]{new string[]{"ou","tsh"}}},
            {"ough", new object[]{new string[]{"au"}}}, //it must be rare to get here
            {"oubt", new object[]{new string[]{"ou"},-1}},
            {"oub", new object[]{new string[]{"uh","b"}}},
            {"ould", new object[]{new string[]{"oo","d"},0,ComboRules.Ending}},
            {"oul", new object[]{new string[]{"oh","-l"}}},
            {"oup", new object[]{new string[]{"uu","p"}}},
            {"our", new object[]{new string[]{"oh","-r"}}},
            {"our 3", new object[]{new string[]{"ou","er"},0,ComboRules.WholeWord}},
            {"ous", new object[]{new string[]{"schwa","s"},0,ComboRules.Ending}},
            {"ou", new object[]{new string[]{"ou"}}},
            {"ov", new object[]{new string[]{"uh","v"}}},
            {"over", new object[]{new string[]{"oh","v","er"},0,ComboRules.Starting}},

            {"owl", new object[]{new string[]{"ou","-l"}}},

            {"own", new object[]{new string[]{"oh","n"},0,ComboRules.All,6}},
            {"own 2", new object[]{new string[]{"ou","n"},0,ComboRules.All,-5}},
            {"own 3", new object[]{new string[]{"oh","n"},0,ComboRules.Starting}},

            {"owr", new object[]{new string[]{"ou","r"}}},
            {"ow", new object[]{new string[]{"ou"}}},

            {"oy", new object[]{new string[]{"oi"}}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"peo", new object[]{new string[]{"p","ee"}}},
            {"per", new object[]{new string[]{"p","er-wild"}}},
            {"prove", new object[]{new string[]{"p","r","uu","v"}}},
            {"pp", new object[]{new string[]{"p"}}},
            {"ph", new object[]{new string[]{"f"}}},
            {"pleas", new object[]{new string[]{"p","l","ee","z"}}},
            {"plea", new object[]{new string[]{"p","l","ee"}}},
            {"pie", new object[]{new string[]{"p","ai"}}},
            {"pious", new object[]{new string[]{"p","ai","schwa","s"},0,ComboRules.WholeWord}},
            {"pious 2", new object[]{new string[]{"p","ai","schwa","s"},0,ComboRules.NotEnding}},
            {"poo", new object[]{new string[]{"p","uu"}}},
            {"pph", new object[]{new string[]{"f"}}},

            {"pn", new object[]{new string[]{"n"},0,ComboRules.Starting}},
            {"ps", new object[]{new string[]{"s"},0,ComboRules.Starting}},
            {"psy", new object[]{new string[]{"s","ai"},0,ComboRules.Starting}},
            {"pt", new object[]{new string[]{"t"},0,ComboRules.Starting}},

            {"pun", new object[]{new string[]{"p","uh","n"},0,ComboRules.Starting}},

            {"pizza", new object[]{new string[]{"p","ee","s","schwa"},0,ComboRules.WholeWord}},
            {"pizzas", new object[]{new string[]{"p","ee","s","uh","z"},0,ComboRules.WholeWord}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"qu", new object[]{new string[]{"k","w"}}},
            {"que", new object[]{new string[]{"k"},0,ComboRules.Ending}},

            {"quer", new object[]{new string[]{"k","er"},0,ComboRules.Ending}},
            {"quet", new object[]{new string[]{"k","ei"},0,ComboRules.Ending}},

            {"quy", new object[]{new string[]{"k","w","ee"},0,ComboRules.Ending}},

        },
        new SortedDictionary<string, object[]>()
        {
            {"rough", new object[]{new string[]{"r","uh","f"}}},

            {"ration", new object[]{new string[]{"r","aa","sh","schwa","n"},0,ComboRules.WholeWord}},
            {"ration 2", new object[]{new string[]{"r","aa","sh","schwa","n"},0,ComboRules.NotEnding}},

            { "rhythms", new object[]{new string[]{"r","ih","thorn","schwa","m","z"},0,ComboRules.Ending}},
            {"rhythm", new object[]{new string[]{"r","ih","thorn","schwa","m"},0,ComboRules.Ending}},

            {"real", new object[]{new string[]{"r","ee","l"}}},
            {"reak", new object[]{new string[]{"r","ei","k"}}},
            {"reality", new object[]{new string[]{"r","ee","aa","l","ih","t","ee"}}},
            {"roen", new object[]{new string[]{"r","eh","n"},0,ComboRules.Starting}},
            {"reak 2", new object[]{new string[]{"r","ee","k"},0,ComboRules.Starting}},

            {"refl", new object[]{new string[]{"r","ee","f","l"}}},

            {"rr", new object[]{new string[]{"r"}}},
            {"rar", new object[]{new string[]{"r","ei","er"}}},
            {"rh", new object[]{new string[]{"r"}}},
            {"rrh", new object[]{new string[]{"r"}}},

            {"re", new object[]{new string[]{"er"},0,ComboRules.Ending}},
            {"rue", new object[]{new string[]{"r","uu"},-1}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},

            {"sought", new object[]{new string[]{"s","au","t"}}},
            {"sough", new object[]{new string[]{"s","oh"}}},

            {"soot", new object[]{new string[]{"s","oo","t"},0,ComboRules.WholeWord}},
            {"she", new object[]{new string[]{"sh","ee"},0,ComboRules.Ending}},
            {"scour", new object[]{new string[]{"s","k","ou","er"},0,ComboRules.Ending}},
            {"shoe", new object[]{new string[]{"sh","uu","z"},0,ComboRules.Ending}},

            {"slaugh", new object[]{new string[]{"s","l","au"}}},

            {"sc", new object[]{new string[]{"s"},0,ComboRules.Starting}},
            {"scism", new object[]{new string[]{"s","k","ih","z","schwa","m"},0,ComboRules.Ending}},
            {"sch", new object[]{new string[]{"s","k"},0,ComboRules.Starting}},
            {"scind", new object[]{new string[]{"s","ih","n","d"}}},
            {"scle", new object[]{new string[]{"s","schwa","l"},0,ComboRules.Ending}},
            {"se 2", new object[]{new string[]{"s"},0,ComboRules.VowelBefore}},
            {"se", new object[]{new string[]{"z","silent-e"},0,ComboRules.Ending}},
            {"seance", new object[]{new string[]{"s","eh","ah","n","s"},0,ComboRules.Starting}},
            {"sei", new object[]{new string[]{"s","ee"},0,ComboRules.Starting}},
            {"ses", new object[]{new string[]{"s","eh","z"},0,ComboRules.Ending}},
            {"sew", new object[]{new string[]{"s","oh"},0,ComboRules.WholeWord}}, // sew what?
            {"sh", new object[]{new string[]{"sh"}}},
            {"sens", new object[]{new string[]{"s","eh","n","s"}}},
            {"show", new object[]{new string[]{"sh","oh"}}},
            {"shoulder", new object[]{new string[]{"sh","oh","-l","d","er"}}},

            {"signage", new object[]{new string[]{"s","ih","g","n"},-3,ComboRules.WholeWord}},
            {"signa", new object[]{new string[]{"s","ih","g","n"},-1}},
            {"sign", new object[]{new string[]{"s","ai","n"},-1}},
            {"sion", new object[]{new string[]{"zh","schwa","n"},0,ComboRules.Ending}},

            {"soccer", new object[]{new string[]{"s","au","k","er"}}},
            {"some", new object[]{new string[]{"s","uh","m"},0,ComboRules.Starting}},
            {"some 2", new object[]{new string[]{"s","uh","m"},0,ComboRules.Ending}},
            {"sour", new object[]{new string[]{"s","ou","er"}}},

            {"sten", new object[]{new string[]{"s","schwa","n"},0,ComboRules.Ending}},
            {"stle", new object[]{new string[]{"s","schwa","l"},0,ComboRules.Ending}},
            {"sure", new object[]{new string[]{"zh","uu","er"},0,ComboRules.Ending}},
            {"survey", new object[]{new string[]{"s","er","v","ei"}}},
            {"supply", new object[]{new string[]{"s","uu","p","l","ai"}}},
            {"supplie", new object[]{new string[]{"s","uu","p","l","ai"}}},
            {"swer", new object[]{new string[]{"s","er"},0,ComboRules.Ending}},
            {"swear", new object[]{new string[]{"s","w","ei","r"}}},
            {"sweat", new object[]{new string[]{"s","w","eh","t"}}},
            {"swor", new object[]{new string[]{"s","oh","r"}}},

            {"ss", new object[]{new string[]{"s"}}},
            {"ssue", new object[]{new string[]{"sh","ee","uu"}}},
            {"ssion", new object[]{new string[]{"sh","schwa","n"}}},
            {"ssian", new object[]{new string[]{"sh","schwa","n"}}},

            {"suit", new object[]{new string[]{"s","uu","stop"}}},
            {"suit 2", new object[]{new string[]{"s","uu","t"},0,ComboRules.WholeWord}},

            {"syn", new object[]{new string[]{"s","ih","n"}}},
            {"siz", new object[]{new string[]{"s"},-2}},

        },
        new SortedDictionary<string, object[]>()
        {
            {"tough", new object[]{new string[]{"t","uh","f"}}},

            {"thought", new object[]{new string[]{"theta","au","t"}}},
            {"though", new object[]{new string[]{"thorn","oh"}}},
            {"thorough", new object[]{new string[]{"theta","er","oh"}}},
            {"through", new object[]{new string[]{"theta","r","uu"}}},


            {"tood", new object[]{new string[]{"t","oo","d"},0,ComboRules.Ending}},
            {"toward", new object[]{new string[]{"t","oh","w","er","d"}}},

            {"th", new object[]{new string[]{"theta"},0,ComboRules.Middle}}, //default th

            {"that", new object[]{new string[]{"thorn","aa","stop"}}},
            {"touch", new object[]{new string[]{"t","uh","tsh"}}},

            {"ted", new object[]{new string[]{"t","eh","d"},0,ComboRules.Ending}},

            //outhouse.
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"tion", new object[]{new string[]{"sh","schwa","n"}}},
            {"to", new object[]{new string[]{"t","uu"},0,ComboRules.WholeWord}},
            {"thereto", new object[]{new string[]{"thorn","ei","-r","t","uu"},0,ComboRules.WholeWord}},
            {"ts", new object[]{new string[]{"stop","s"}}},
            {"tt", new object[]{new string[]{"t"}}},
            {"tw", new object[]{new string[]{"t","w"}}},
            {"tie", new object[]{new string[]{"t","ai"}}},

            {"th 2", new object[]{new string[]{"thorn"},0,ComboRules.Starting,-5}},
            {"th 3", new object[]{new string[]{"theta"},0,ComboRules.Starting,6}},
            {"th 4", new object[]{new string[]{"theta"},0,ComboRules.Ending,5}},
            {"th 5", new object[]{new string[]{"thorn"},0,ComboRules.Ending,-4}},
            {"the", new object[]{new string[]{"thorn"},0,ComboRules.Ending,5}},
            {"the 2", new object[]{new string[]{"thorn","schwa"},0,ComboRules.WholeWord}},
            {"theist", new object[]{new string[]{"theta","ee","ih","s","t"},0,ComboRules.Ending,5}},

            {"thm", new object[]{new string[]{"thorn","m"}}},
            {"thy", new object[]{new string[]{"theta","ee"},0,ComboRules.Ending,5}},

            {"thy 2", new object[]{new string[]{"thorn","ai"},0,ComboRules.Starting}},
            {"them", new object[]{new string[]{"thorn","eh","m"},0,ComboRules.Starting}},
            {"there", new object[]{new string[]{"thorn","ei","-r"},0,ComboRules.Starting}},
            {"thing", new object[]{new string[]{"theta","ee","ng"}}},


            {"ther", new object[]{new string[]{"theta","er"}}},
            {"tch", new object[]{new string[]{"tsh"}}},
            {"tial", new object[]{new string[]{"tsh","ee","schwa","l"}}},
            {"tr", new object[]{new string[]{"tsh","r"}}},
            {"ture", new object[]{new string[]{"tsh","er"},0,ComboRules.Ending}},
            {"tungsten", new object[]{new string[]{"t","uh","ng","s","t","schwa","n"}}},

            {"tun", new object[]{new string[]{"t","uu","n"},0,ComboRules.Starting}},
            {"two", new object[]{new string[]{"t","uu"}}},

            {"tz", new object[]{new string[]{"stop","s"}}},
        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},

            {"ue", new object[]{new string[]{"uu"},0,ComboRules.Ending}},

            {"uild", new object[]{new string[]{"ih","-l","d"},0,ComboRules.Ending}},
            {"uing", new object[]{new string[]{"uu","ee","ng"},0,ComboRules.Ending}},
            {"un", new object[]{new string[]{"uh","n"},0,ComboRules.Starting}},
            {"uni", new object[]{new string[]{"j","uu","n"},-1}},
            {"ul", new object[]{new string[]{"schwa","-l"},0,ComboRules.Ending}},
            {"ule", new object[]{new string[]{"uu","-l"},-1,ComboRules.Ending}},
            {"ur", new object[]{new string[]{"er"},0,ComboRules.NoVowelNext}},
            {"ur 2", new object[]{new string[]{"j","er"},0,ComboRules.VowelNext}},
            {"urr", new object[]{new string[]{"er"}}},
            {"us", new object[]{new string[]{"j","uu","s"},0,ComboRules.Starting}},
            {"use", new object[]{new string[]{"j","uu","z"},0,ComboRules.Starting}},

            {"usual", new object[]{new string[]{"j","uu","zh","uu","schwa","l"}}},

            {"uu", new object[]{new string[]{"uu","schwa"}}},
            {"uy", new object[]{new string[]{"ai"}}},
        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
             {"violen", new object[]{new string[]{"v","ai","l","eh","n"}}},
            {"vv", new object[]{new string[]{"v"}}},

            {"very", new object[]{new string[]{"v","ei","r","ee"},0,ComboRules.WholeWord}},
        },
        new SortedDictionary<string, object[]>()
        {
            {"wrough", new object[]{new string[]{"r","au"}}},
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"wood", new object[]{new string[]{"w","oo","d"}}},

            {"weird", new object[]{new string[]{"w","ee","-r","d"}}},
            {"were", new object[]{new string[]{"w","er"},0,ComboRules.WholeWord}},

            {"what", new object[]{new string[]{"w","uh","t"}}},
            {"where", new object[]{new string[]{"w","ei","-r"}}},
            {"wh", new object[]{new string[]{"w"}}},
            {"who", new object[]{new string[]{"h"},-1,ComboRules.NotEnding}},
            {"who 2", new object[]{new string[]{"h","uu"},0,ComboRules.Ending}},

            {"wind", new object[]{new string[]{"w","ih","n","d"}}},

            {"worn", new object[]{new string[]{"w","oh","-r","n"},0,ComboRules.WholeWord}},
            {"wor", new object[]{new string[]{"w","er"}}},

            {"woe", new object[]{new string[]{"w","oh"},0,ComboRules.Starting}},
            {"woman", new object[]{new string[]{"w","uh","m","eh","n"}}},
            {"world", new object[]{new string[]{"w","er","schwa","l","d"}}},
            {"women", new object[]{new string[]{"w","eh","m","eh","n"}}},

            {"wr", new object[]{new string[]{"r"}}},
            {"ww", new object[]{new string[]{"w"}}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"xious", new object[]{new string[]{"k","sh","schwa","s"}}},
            {"xual", new object[]{new string[]{"k","sh","uu","au","l"}}},
            {"xur", new object[]{new string[]{"k","zh","er"}}},
            {"xc", new object[]{new string[]{"k","s"},0,ComboRules.EIYNext}},
            {"xx", new object[]{new string[]{"k","s"}}},
            {"xy", new object[]{new string[]{"k","s","ee"},0,ComboRules.Ending}},
            {"xy 2", new object[]{new string[]{"z","ai"},0,ComboRules.Starting}},
            {"xiness", new object[]{new string[]{"k","s","ee","n","eh","s"},0,ComboRules.Ending}},
            {"xier", new object[]{new string[]{"k","s","ee","er"},0,ComboRules.Ending}},
            {"xing", new object[]{new string[]{"k","s","ee","ng"},0,ComboRules.Ending}},
            {"xiest", new object[]{new string[]{"k","s","ee","eh","s","t"},0,ComboRules.Ending}},

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"yearn", new object[]{new string[]{"y","er","n"},0,ComboRules.WholeWord}},
            {"you", new object[]{new string[]{"j","uu"},0,ComboRules.WholeWord}},
            {"your", new object[]{new string[]{"j","oh","-r"}}},

            {"ysm", new object[]{new string[]{"ih","s","schwa","m"},0,ComboRules.Ending}},
            {"yy", new object[]{new string[]{"j"}}},
            {"yr", new object[]{new string[]{"er"},0,ComboRules.NoVowelNext}},
            {"yr 2",new object[]{new string[]{"ai","r"},0,ComboRules.VowelNext}}

        },
        new SortedDictionary<string, object[]>()
        {
            //{"a", new object[]{new string[]{"ei"},0,ComboRules.WholeWord}},
            {"zoos", new object[]{new string[]{"z","uu","z"},0,ComboRules.WholeWord}},
            {"zoo", new object[]{new string[]{"z","uu"},0,ComboRules.WholeWord}},
            {"zoo 2", new object[]{new string[]{"z","uu"},-1,ComboRules.Starting}},

            {"zz", new object[]{new string[]{"z"}}},
            {"zure", new object[]{new string[]{"zh","j","uu","-r"},0,ComboRules.Ending}},

        },
    };

    public static void a(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[0], out found, ref pos, ref word, out newSkip, ref newPhon);

        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of A.
            //rule 0: higher rules take precedence


            //rule 1: after w it is pronounced as "ah"
            if (!found && pos > 0)
            {
                if (word.Substring(pos - 1, 1) == "w")
                {
                    newPhon = new List<string>() { "ah" };
                    found = true;
                }
            }

            //rule 2: first letter in the word makes it "aa" at stressed
            if (!found)
            {
                if (pos == 0)
                {
                    newPhon = new List<string>() { "a-wild" };
                    found = true;
                }
            }

            //rule 3: last letter in the word makes it "schwa"
            if (!found)
            {
                if (pos == word.Length - 1)
                {
                    newPhon = new List<string>() { "final-a-wild" };
                    found = true;
                }
            }

            //rule 4: two consonants after makes "aa"
            if (!found && pos + 2 < word.Length)
            {
                if (isConsonant(word.Substring(pos + 1, 1), out nothing) && isConsonant(word.Substring(pos + 2, 1), out nothing))
                {
                    newPhon = new List<string>() { "aa" };
                    found = true;
                }
            }

            //rule 5: last vowel in the word, or next "ih" or "schwa" sound, or makes it "aa"
            if (!found)
            {
                if (nxtVowelPhonemes.Count == 0 || nxtVowelPhonemes[0] == "ih" || nxtVowelPhonemes[0] == "schwa") // on what basis?
                {
                    newPhon = new List<string>() { "a-wild" };
                    found = true;
                }
            }

            //rule 6: two letters later is some vowels, pronounced "ei", but "ah" if that is word final
            if (!found && pos + 2 < word.Length)
            {
                if (howFarToNextVowel == 2)
                {
                    /*if (pos + howFarToNextVowel == word.Length - 1)
                    {
                        newPhon = new List<string>() { "ah" };
                    }
                    else*/
                    if (word.Substring(pos+2,1) == "a")
                    {
                        newPhon = new List<string>() { "aa" };
                    }
                   else
                    {
                        if (pos == 0)
                        {
                            newPhon = new List<string>() { "aa" };
                        }
                        else
                        {
                            newPhon = new List<string>() { "ei" };
                        }
                        found = true;
                    }
                }
            }

            //rule infinity: english you suck, here's a default sound
            if (!found)
            {
                newPhon = new List<string>() { "a-wild" };
            }

            // newPhon = new List<string>() { "ah" }; //TEST
        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void b(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[1], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of B.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "b" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void c(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[2], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of C.

            //rule 1: c before i, we hate the sky
            if (!found && pos + 1 < word.Length)
            {
                string ltr = word.Substring(pos + 1, 1);
                if (ltr == "e" || ltr == "i" || ltr == "y")
                {
                    newPhon = new List<string>() { "s" };
                    found = true;
                }
            }

            //rule 2: k
            if (!found)
            {
                newPhon = new List<string>() { "k" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void d(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[3], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of D.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "d" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void e(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[4], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of E.
            //rule 0: higher rules take precedence

            //rule 1: is the first letter
            if (!found && pos == 0)
            {
                newPhon = new List<string>() { "eh" };
                found = true;
            }

            //rule 2: is the only vowel and last letter
            if (!found && howFarToNextVowel == -1 && pos == word.Length - 1)
            {
                if (pos != 0)
                {
                    if (isConsonant(word.Substring(0, pos), out nothing))
                    {
                        newPhon = new List<string>() { "ee" };
                        found = true;
                    }
                }
            }

            //rule 3: is the only vowel and whatever
            if (!found && howFarToNextVowel == -1 && pos != 0)
            {
                if (pos != 0)
                {
                    if (isConsonant(word.Substring(0, pos), out nothing))
                    {
                        newPhon = new List<string>() { "eh" };
                        found = true;
                    }
                }
            }

            //rule 4: ih
            if (!found && nxtVowelPhonemes.Count > 0)
            {
                if (nxtVowelPhonemes[0] == "ih")
                {
                    newPhon = new List<string>() { "eh" };
                    found = true;
                }
            }

            //rule 5: two consonants next make it "eh"
            if (!found && howFarToNextVowel > 2)
            {
                newPhon = new List<string>() { "eh" };
                found = true;
            }

            //rule infinity: english you suck, here's a default sounds?
            if (!found)
            {
                newPhon = new List<string>() { "e-wild" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void f(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[5], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of G.

            //rule 1: i am   of   save -princes
            if (!found)
            {
                newPhon = new List<string>() { "f" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void g(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[6], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of G.

            //rule 1: jesu
            if (!found && nxtVowelPhonemes.Count > 0)
            {
                if ((nxtVowelPhonemes[0] == "ai" || nxtVowelPhonemes[0] == "ih") && word.Length >= 5)
                {
                    newPhon = new List<string>() { "dzh" };
                    found = true;
                }
            }
            //rule infinity: desu
            if (!found)
            {
                newPhon = new List<string>() { "g" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void h(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[7], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            ////////THIS LETTER SUCKS!////////

            //if (!found)
            //{
            newPhon = new List<string>() { "h" };
            //}

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void i(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[8], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of E.
            //rule 0: higher rules take precedence
            //rule 1: two consonants next make it "ih"
            if (!found && howFarToNextVowel > 2)
            {
                newPhon = new List<string>() { "ih" };
                found = true;
            }

            //rule 2 and 3
            if (!found && howFarToNextVowel == -1)
            {
                //rule 2:        
                if (pos == word.Length - 1)
                {
                    newPhon = new List<string>() { "final-i-wild" };
                }
                //rule 3:        
                else
                {
                    newPhon = new List<string>() { "ih" };
                }
                found = true;
            }

            //rule 4: if "ih" is next then "ih" too
            if (!found && nxtVowelPhonemes.Count > 0)
            {
                if (nxtVowelPhonemes[0] == "ih" || nxtVowelPhonemes[0] == "ee" || nxtVowelPhonemes[0] == "final-y-wild")
                {
                    newPhon = new List<string>() { "ih" };
                    found = true;
                }
                //rule 5: before silent e
                else if (nxtVowelPhonemes[0] == "silent-e")
                {
                    newPhon = new List<string>() { "ai" };
                    found = true;
                }

            }

            //rule 5: 
            if (!found && howFarToNextVowel == 2)
            {
                newPhon = new List<string>() { "ih" };
                found = true;
            }

            //more rules coming soon

            //rule infinity: this has gotten boring
            if (!found)
            {
                newPhon = new List<string>() { "i-wild" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void j(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[9], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            ////////THIS LETTER STILL SUCKS, BUT NOT AS MUCH AS H!////////

            //if (!found) then don't make the j sound: english logic!
            //{
            newPhon = new List<string>() { "dzh" };
            //}

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void k(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[10], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of K.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "k" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void l(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[11], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of L.

            //rule 1: LOL!
            if (!found)
            {
                newPhon = new List<string>() { "l" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void m(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        //combo stuff
        bool found = false;
        comboStuff(ref comboMaker[12], out found, ref pos, ref word, out newSkip, ref newPhon);

        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of M.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "m" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void n(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[13], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of N.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "n" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void o(int pos, string word, out List<string> phonemes, out int? skip) //soon to be finished
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[14], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of O.
            //rule 0: oh dear

            //rule 1: first letter in the word makes it "au" at stressed
            if (!found)
            {
                if (pos == 0)
                {
                    newPhon = new List<string>() { "au" };
                    found = true;
                }
            }

            //rule 2: last letter in the word makes it "oh"
            if (!found)
            {
                if (pos == word.Length - 1)
                {
                    newPhon = new List<string>() { "oh" };
                    found = true;
                }
            }

            //rule 3: two consonants after makes "au"
            if (!found && pos + 2 < word.Length)
            {
                if (isConsonant(word.Substring(pos + 1, 1), out nothing) && isConsonant(word.Substring(pos + 2, 1), out nothing))
                {
                    newPhon = new List<string>() { "au" };
                    found = true;
                }
            }

            //rule 4: last vowel in the word, or next "ih" or "schwa" sound, or makes it "aa"
            if (!found)
            {
                if (nxtVowelPhonemes.Count == 0 || nxtVowelPhonemes[0] == "ih" || nxtVowelPhonemes[0] == "schwa")
                {
                    newPhon = new List<string>() { "o-wild" };
                    found = true;
                }
            }

            //rule 5: damn
            if (!found && pos + 2 < word.Length)
            {
                if (howFarToNextVowel == 2)
                {
                    /*if (pos + howFarToNextVowel == word.Length - 1)
                    {
                        newPhon = new List<string>() { "ah" };
                    }
                    else*/
                    if (word.Substring(pos + 2, 1) == "o")
                    {
                        newPhon = new List<string>() { "au" };
                    }
                    else
                    {
                        if (pos == 0)
                        {
                            newPhon = new List<string>() { "au" };
                        }
                        else
                        {
                            newPhon = new List<string>() { "oh" };
                        }
                        found = true;
                    }
                }
            }

            //rule infinity: english you suck, here's a default sound(en)
            if (!found)
            {
                newPhon = new List<string>() { "o-wild" };
            }


            // newPhon = new List<string>() { "ah" }; //TEST
        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void p(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[15], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of P.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "p" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void q(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[16], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of X.

            //rule 1: they're hour know rules.
            if (!found)
            {
                newPhon = new List<string>() { "k" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void r(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[17], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of R.

            //rule 1: It's hard to get here.
            if (!found)
            {
                newPhon = new List<string>() { "r" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void s(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[18], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of S.

            //rule 1: unvoiced end s is s, voiced end s is Z
            if (!found && pos == word.Length - 1 && word.Length != 1)
            {
                if (isVoiced(word.Substring(pos - 1, 1)))
                {
                    newPhon = new List<string>() { "z" };
                    found = true;
                }
                else
                {
                    newPhon = new List<string>() { "s" };
                    found = true;
                }
            }

            //rule 2: a vowel before & after make it Z
            if (!found && pos > 0 && pos < word.Length - 1)
            {
                if (!isConsonant(word.Substring(pos - 1, 1), out nothing) && !isConsonant(word.Substring(pos + 1, 1), out nothing))
                {
                    newPhon = new List<string>() { "z" };
                    found = true;
                }
            }

            //rule infinity: at least it wasn't the letter o
            if (!found)
            {
                newPhon = new List<string>() { "s" };
            }
        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void t(int pos, string word, out List<string> phonemes, out int? skip) //this thing needs thought though.
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[19], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of T.

            //rule 1: stop thinking about it
            if (!found && howFarToNextVowel > 1 && pos != 0 && pos != word.Length - 1)
            {
                newPhon = new List<string>() { "stop" };
                found = true;
            }

            //rule infinity: annoyed grunt
            if (!found)
            {
                newPhon = new List<string>() { "t" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void u(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[20], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of U.

            //rule 1: the right to rEmain silent
            if (!found && howFarToNextVowel == 2)
            {
                string ltr = word.Substring(pos + 2, 1);
                if (ltr == "e" || ltr == "i" || ltr == "y")
                {
                    newPhon = new List<string>() { "uu" }; /*               uuhhh.wav                 */
                    found = true;
                }
            }

            //rule NaN: byebye now
            if (!found)
            {
                newPhon = new List<string>() { "u-wild" }; //"uh" for stressed and "schwa" for unstressed
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void v(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[21], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of V.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "v" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void w(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[22], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of W.

            //rule 1: their our know rules.
            if (!found)
            {
                newPhon = new List<string>() { "w" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void x(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[23], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of X.

            //rule 1: x at the beginning is Z.
            if (!found && pos == 0)
            {
                newPhon = new List<string>() { "z" };
                found = true;
            }

            //rule infinity: never mind, that was quick.
            if (!found)
            {
                newPhon = new List<string>() { "k", "s" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void y(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[24], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            int howFarToNextVowel = 0;
            string inBetweenThisAndNxtVowel = "";
            List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of Y.

            //rule 0: first sound
            if (!found)
            {
                if (pos == 0)
                {
                    newPhon = new List<string>() { "j" };
                    found = true;
                }
            }

            //rule 1: two consonants next make it "ih"
            if (!found)
            {
                if (howFarToNextVowel > 2)
                {
                    newPhon = new List<string>() { "ih" };
                    found = true;
                }
            }

            //rule 2: the end?
            if (!found)
            {
                if (pos == word.Length - 1)
                {
                    newPhon = new List<string>() { "final-y-wild" }; //stress rules later
                    found = true;
                }
            }

            //rule infinity: y, u do this??????
            if (!found)
            {
                newPhon = new List<string>() { "y-wild" }; //don't be stressed out
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }
    public static void z(int pos, string word, out List<string> phonemes, out int? skip)
    {
        List<string> newPhon = new List<string>() { };
        int newSkip = 0;

        /* combo stuff*/
        bool found = false;
        comboStuff(ref comboMaker[25], out found, ref pos, ref word, out newSkip, ref newPhon);



        if (!found)
        {
            //int howFarToNextVowel = 0;
            //string inBetweenThisAndNxtVowel = "";
            //List<string> nxtVowelPhonemes = nextVowelSound(pos, word, out howFarToNextVowel, out inBetweenThisAndNxtVowel);

            //time for the "rules" of Z.

            //rule -0: z is z.
            if (!found)
            {
                newPhon = new List<string>() { "z" };
            }

        }

        skip = newSkip;
        phonemes = newPhon;
    }

    public delegate void LetterFunc(int pos, string word, out List<string> phonemes, out int? skip);

    private static LetterFunc[] letterFuncs =
    {
        a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z
    };

    public static List<string> letter(string word, int pos, out int? skip)
    {
        skip = 0;
        List<string> list = null;
        int ch = word.Substring(pos, 1).ToLower().ToCharArray()[0] - 'a';

        if (ch >= 0 && ch < 26)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            letterFuncs[ch](pos, word, out list, out skip);
            //watch.Stop();
            //Debug.Log(watch.ElapsedTicks);
        }

        if (list == null)
        {
            list = new List<string>();
        }
        return list;
    }

    public static List<string> main(string message, bool log, out List<int> stresses)
    {
        string newMsg = message;
        newMsg = newMsg.Trim(' ').ToLower();
        List<string> phonemes = new List<string>();
        stresses = new List<int>();

        char[] seperators = { ' ', ',', '.', ':', ';', '?', '!', '\n' };

        string[] tempwords = newMsg.Split(seperators);
        tempwords = tempwords.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        List<string> words = new List<string>(tempwords);

        /*foreach (var s in words) // This is test to print the seperated words //
        {
            Debug.Log(s);
        }*/

        foreach (var word in words)
        {
            for (int pos = 0; pos < word.Length; pos++)
            {
                int? skip = 0;
                List<string> newPhonemes = letter(word, pos, out skip);
                foreach (string phon in newPhonemes)
                {
                    phonemes.Add(phon);
                }
                if (skip != null)
                {
                    pos += (int)skip;
                }
            }
            phonemes.Add("_");
        }

        int pos2 = 0;
        int pos3 = 0;
        int times = 0;
        var phonemes2 = phonemes;
        for (int i = 0; i < phonemes2.Count; i++)
        {
            string phon = phonemes2[i];
            if (phon == "_" || pos2 == phonemes2.Count-1)
            {
                List<string> sublist = new List<string>();
                if (pos2 == phonemes2.Count - 1)
                {
                    sublist = phonemes2.GetRange(pos3, pos2 - pos3 + 1);
                }
                else
                {
                    sublist = phonemes2.GetRange(pos3, pos2 - pos3);
                }
                List<int> stressestemp = new List<int>();
                sublist = stressrules.main(sublist, words[times],out stressestemp);
                stresses.AddRange(stressestemp);
                int nothing = 0;
                for (int something = pos3; something < pos2; something++)
                {
                    phonemes2[something] = sublist[nothing];
                    nothing++;
                }
                pos3 = pos2+1;
                times++;
            }
            pos2++;
        }
        phonemes = phonemes2;
        
        /*if (log)
        {
            foreach (var item in stresses)
            {
                Debug.Log(item); //testingstuff only ~ delet this
            }
            foreach (var item in phonemes)
            {
                Debug.Log(item); //testingstuff only ~ delet this
            }
        }*/

        return phonemes;
    }

}