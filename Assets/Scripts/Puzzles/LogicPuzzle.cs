using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LogicPuzzle : MonoBehaviour, IChoiceUIResponse
{
    #region generation stuff

    public abstract class Proposition
    {
        public bool truthValue;
        public uint size;
        public abstract override string ToString();
        public abstract bool Equals(Proposition other);
        public abstract Theorem[] theorems { get; }

        public Proposition Clone()
        {
            if (this is Atomic) { return new Atomic((Atomic)this); }
            else if (this is AtomicPlaceholder) { return new AtomicPlaceholder((AtomicPlaceholder)this); }
            else if (this is Negation) { return new Negation((Negation)this); }
            else if (this is Conditional) { return new Conditional((Conditional)this); }
            else if (this is Disjunction) { return new Disjunction((Disjunction)this); }
            else if (this is Conjunction) { return new Conjunction((Conjunction)this); }
            else if (this is Biconditional) { return new Biconditional((Biconditional)this); }
            return null;
        }
    }
    //P
    public class Atomic : LogicPuzzle.Proposition
    {
        public int negationInsertPoint;
        public string statement;

        public static string[] sampleStrings = { "pick ", "choose ", "select ", "answer with "};

        public override string ToString()
        {
            string rand1 = sampleStrings[Fakerand.Int(0, sampleStrings.Length)];
            return rand1 + statement;
        }

        public override bool Equals(Proposition other)
        {
            if (other == this) { return true; }
            return (other is Atomic) && (((Atomic)other).statement == statement);
        }

        public Atomic(string statement, int negationInsertPoint = -1, bool truthValue = true)
        {
            size = 1;
            this.truthValue = truthValue;
            this.statement = statement;
            this.negationInsertPoint = negationInsertPoint;
        }

        public Atomic(Atomic other)
        {
            size = 1;
            truthValue = other.truthValue;
            statement = other.statement;
            negationInsertPoint = other.negationInsertPoint;
        }

        
        public override Theorem[] theorems
        { get { return LogicPuzzle.theorems[1]; } }
    }

    public class AtomicPlaceholder : LogicPuzzle.Proposition
    {
        public string letter;

        public override string ToString()
        {
            return letter;
        }

        public override bool Equals(Proposition other)
        {
            if (other == this) { return true; }
            return (other is AtomicPlaceholder) && (((AtomicPlaceholder)other).letter == letter);
        }

        public AtomicPlaceholder(string letter)
        {
            size = 1;
            truthValue = true;
            this.letter = letter;
        }

        public AtomicPlaceholder(AtomicPlaceholder other)
        {
            size = 1;
            truthValue = other.truthValue;
            letter = other.letter;
        }

        public override Theorem[] theorems
        { get { return new Theorem[0]; } }

    }

    // ~P
    public class Negation : LogicPuzzle.Proposition
    {
        public Proposition p;

        public static string[] sampleStrings = { "it's incorrect that ", "it's false that ", "it's not true that ", "it's wrong that " };

        public override string ToString()
        {
            if (p is Atomic)
            {
                if (((Atomic)p).negationInsertPoint != -1)
                {
                    int n = ((Atomic)p).negationInsertPoint;
                    string temp = p.ToString();
                    return temp.Substring(0, n) + "not " + temp.Substring(n);
                }
                else
                {
                    return "don't " + p.ToString();
                }
            }
            else
            {
                string rand1 = sampleStrings[Fakerand.Int(0, sampleStrings.Length)];
                return rand1 + "\"" + p.ToString() + "\"";
            }
        }

        public override bool Equals(Proposition other)
        {
            if (other == this) { return true; }
            return (other is Negation) && p.Equals(((Negation)other).p);
        }

        public Negation(ref Proposition _p)
        {
            p = _p;
            size = _p.size + 1;
            truthValue = !_p.truthValue;
        }

        public Negation(Proposition _p)
        {
            p = _p;
            size = _p.size + 1;
            truthValue = !_p.truthValue;
        }

        public Negation(string _p)
        {
            p = new AtomicPlaceholder(_p);
            size = 2;
            truthValue = false;
        }

        public Negation(Negation other)
        {
            if (other.p is Atomic) { p = new Atomic((Atomic)other.p); }
            else if (other.p is AtomicPlaceholder) { p = new AtomicPlaceholder((AtomicPlaceholder)other.p); }
            else if (other.p is Negation) { p = new Negation((Negation)other.p); }
            else if (other.p is Conditional) { p = new Conditional((Conditional)other.p); }
            else if (other.p is Disjunction) { p = new Disjunction((Disjunction)other.p); }
            else if (other.p is Conjunction) { p = new Conjunction((Conjunction)other.p); }
            else if (other.p is Biconditional) { p = new Biconditional((Biconditional)other.p); }
            size = other.size;
            truthValue = other.truthValue;
        }

        public override Theorem[] theorems
        { get { return LogicPuzzle.theorems[2]; } }
    }

    public abstract class BinaryConnector : LogicPuzzle.Proposition
    {
        public Proposition p;
        public Proposition q;

        public BinaryConnector(ref Proposition _p, ref Proposition _q)
        {
            p = _p;
            q = _q;
            size = _p.size + _q.size + 1;
        }

        public BinaryConnector(Proposition _p, Proposition _q)
        {
            p = _p;
            q = _q;
            size = _p.size + _q.size + 1;
        }

        public BinaryConnector(string _p, Proposition _q)
        {
            p = new AtomicPlaceholder(_p);
            q = _q;
            size = _q.size + 2;
        }

        public BinaryConnector(Proposition _p, string _q)
        {
            p = _p;
            q = new AtomicPlaceholder(_q);
            size = _p.size + 2;
        }

        public BinaryConnector(string _p, string _q)
        {
            p = new AtomicPlaceholder(_p);
            q = new AtomicPlaceholder(_q);
            size = 3;
        }

        public BinaryConnector(BinaryConnector other)
        {
            if (other.p is Atomic) { p = new Atomic((Atomic)other.p); }
            else if (other.p is AtomicPlaceholder) { p = new AtomicPlaceholder((AtomicPlaceholder)other.p); }
            else if (other.p is Negation) { p = new Negation((Negation)other.p); }
            else if (other.p is Conditional) { p = new Conditional((Conditional)other.p); }
            else if (other.p is Disjunction) { p = new Disjunction((Disjunction)other.p); }
            else if (other.p is Conjunction) { p = new Conjunction((Conjunction)other.p); }
            else if (other.p is Biconditional) { p = new Biconditional((Biconditional)other.p); }

            if (other.q is Atomic) { q = new Atomic((Atomic)other.q); }
            else if (other.q is AtomicPlaceholder) { q = new AtomicPlaceholder((AtomicPlaceholder)other.q); }
            else if (other.q is Negation) { q = new Negation((Negation)other.q); }
            else if (other.q is Conditional) { q = new Conditional((Conditional)other.q); }
            else if (other.q is Disjunction) { q = new Disjunction((Disjunction)other.q); }
            else if (other.q is Conjunction) { q = new Conjunction((Conjunction)other.q); }
            else if (other.q is Biconditional) { q = new Biconditional((Biconditional)other.q); }

            size = other.size;

            truthValue = other.truthValue;
        }
    }

    // P --> Q
    public class Conditional : LogicPuzzle.BinaryConnector
    {
        public static KeyValuePair<string, string>[] sampleStringsPairs =
        {
            new KeyValuePair<string, string>("if ", ", then "),
            new KeyValuePair<string, string>("when ", ", then "),
            new KeyValuePair<string, string>("if ", ", it's necessary that "),
        };

        public override string ToString()
        {
            KeyValuePair<string,string> rand1 = sampleStringsPairs[Fakerand.Int(0, sampleStringsPairs.Length)];
            return rand1.Key + p.ToString() + rand1.Value + q.ToString();
        }

        public override bool Equals(Proposition other)
        {
            if (other == this) { return true; }
            return (other is Conditional) && p.Equals(((Conditional)other).p) && q.Equals(((Conditional)other).q);
        }

        public Conditional(ref Proposition _p, ref Proposition _q) : base(_p, _q)
        {
            truthValue = (!p.truthValue) || q.truthValue;
        }

        public Conditional(Proposition _p, Proposition _q) : base(_p, _q)
        {
            truthValue = (!p.truthValue) || q.truthValue;
        }

        public Conditional(string _p, Proposition _q) : base(_p, _q)
        {
            truthValue = (!p.truthValue) || q.truthValue;
        }

        public Conditional(Proposition _p, string _q) : base(_p, _q)
        {
            truthValue = (!p.truthValue) || q.truthValue;
        }

        public Conditional(string _p, string _q) : base(_p, _q)
        {
            truthValue = (!p.truthValue) || q.truthValue; //...
        }

        public Conditional(Conditional other): base(other) { }

        public override Theorem[] theorems
        { get { return LogicPuzzle.theorems[3]; } }
    }

    // P v Q
    public class Disjunction : LogicPuzzle.BinaryConnector
    {
        public static KeyValuePair<string, string>[] sampleStringsPairs =
        {
            new KeyValuePair<string, string>("either ", ", or "),
            new KeyValuePair<string, string>("unless ", ", then "),
        };

        public override string ToString()
        {
            KeyValuePair<string, string> rand1 = sampleStringsPairs[Fakerand.Int(0, sampleStringsPairs.Length)];
            return rand1.Key + p.ToString() + rand1.Value + q.ToString();
        }

        public override bool Equals(Proposition other)
        {
            if (other == this) { return true; }
            return (other is Disjunction) && p.Equals(((Disjunction)other).p) && q.Equals(((Disjunction)other).q);
        }

        public Disjunction(ref Proposition _p, ref Proposition _q) : base(_p, _q)
        {
            truthValue = p.truthValue || q.truthValue;
        }

        public Disjunction(Proposition _p, Proposition _q) : base(_p, _q)
        {
            truthValue = p.truthValue || q.truthValue;
        }

        public Disjunction(string _p, Proposition _q) : base(_p, _q)
        {
            truthValue = p.truthValue || q.truthValue; //...
        }

        public Disjunction(Proposition _p, string _q) : base(_p, _q)
        {
            truthValue = p.truthValue || q.truthValue; //...
        }

        public Disjunction(string _p, string _q) : base(_p, _q)
        {
            truthValue = p.truthValue || q.truthValue; //...
        }

        public Disjunction(Disjunction other) : base(other) { }

        public override Theorem[] theorems
        { get { return LogicPuzzle.theorems[4]; } }
    }

    // P ^ Q
    public class Conjunction : LogicPuzzle.BinaryConnector
    {
        public static KeyValuePair<string, string>[] sampleStringsPairs =
        {
            new KeyValuePair<string, string>("", ", also, "),
            new KeyValuePair<string, string>("", ", and "),
            new KeyValuePair<string, string>("both ", ", and "),
        };

        public override string ToString()
        {
            KeyValuePair<string, string> rand1 = sampleStringsPairs[Fakerand.Int(0, sampleStringsPairs.Length)];
            return rand1.Key + p.ToString() + rand1.Value + q.ToString();
        }

        public override bool Equals(Proposition other)
        {
            if (other == this) { return true; }
            return (other is Conjunction) && p.Equals(((Conjunction)other).p) && q.Equals(((Conjunction)other).q);
        }

        public Conjunction(ref Proposition _p, ref Proposition _q) : base(_p, _q)
        {
            truthValue = p.truthValue && q.truthValue;
        }

        public Conjunction(Proposition _p, Proposition _q) : base(_p, _q)
        {
            truthValue = p.truthValue && q.truthValue;
        }

        public Conjunction(string _p, Proposition _q) : base(_p, _q)
        {
            truthValue = p.truthValue && q.truthValue; //...
        }

        public Conjunction(Proposition _p, string _q) : base(_p, _q)
        {
            truthValue = p.truthValue && q.truthValue; //...
        }

        public Conjunction(string _p, string _q) : base(_p, _q)
        {
            truthValue = p.truthValue && q.truthValue; //...
        }

        public Conjunction(Conjunction other) : base(other) { }

        public override Theorem[] theorems
        { get { return LogicPuzzle.theorems[5]; } }
    }

    // P <-> Q
    public class Biconditional : LogicPuzzle.BinaryConnector
    {
        public static KeyValuePair<string, string>[] sampleStringsPairs =
        {
            new KeyValuePair<string, string>("", ", exactly when "),
            new KeyValuePair<string, string>("", ", if and only if "),
        };

        public override string ToString()
        {
            KeyValuePair<string, string> rand1 = sampleStringsPairs[Fakerand.Int(0, sampleStringsPairs.Length)];
            return rand1.Key + p.ToString() + rand1.Value + q.ToString();
        }

        public override bool Equals(Proposition other)
        {
            if (other == this) { return true; }
            return (other is Biconditional) && p.Equals(((Biconditional)other).p) && q.Equals(((Biconditional)other).q);
        }

        public Biconditional(ref Proposition _p, ref Proposition _q) : base(_p, _q)
        {
            truthValue = (p.truthValue == q.truthValue);
        }

        public Biconditional(Proposition _p, Proposition _q) : base(_p, _q)
        {
            truthValue = (p.truthValue == q.truthValue);
        }

        public Biconditional(string _p, Proposition _q) : base(_p, _q)
        {
            truthValue = (p.truthValue == q.truthValue);
        }

        public Biconditional(Proposition _p, string _q) : base(_p, _q)
        {
            truthValue = (p.truthValue == q.truthValue);
        }

        public Biconditional(string _p, string _q) : base(_p, _q)
        {
            truthValue = (p.truthValue == q.truthValue);
        }

        public Biconditional(Biconditional other) : base(other) { }

        public override Theorem[] theorems
        { get { return LogicPuzzle.theorems[6]; } }
    }

    public class Theorem
    {
        public Proposition prop;
        public string propString;
        public Theorem(string str)
        {
            propString = str;
            prop = ParseTheoremNotation(str);
        }
    }

    public static Proposition ParseTheoremNotation(string notation) // postfix notation. ~ (neg.), > (cond.), v, ^, = (bicond.).  all other symbols are propositions.
    {
        Stack<Proposition> tokens = new Stack<Proposition>();
        for (int i = 0; i < notation.Length; ++i)
        {
            char c = notation[i];
            if (c == '~')
            {
                if (tokens.Count == 0) { throw new Exception("Too many operations!"); }
                Proposition p = tokens.Pop();
                tokens.Push(new Negation(ref p));
            }
            else if (c == '>')
            {
                if (tokens.Count < 2) { throw new Exception("Too many operations!"); }
                Proposition q = tokens.Pop();
                Proposition p = tokens.Pop();
                tokens.Push(new Conditional(ref p, ref q));
            }
            else if (c == 'v')
            {
                if (tokens.Count < 2) { throw new Exception("Too many operations!"); }
                Proposition q = tokens.Pop();
                Proposition p = tokens.Pop();
                tokens.Push(new Disjunction(ref p, ref q));
            }
            else if (c == '^')
            {
                if (tokens.Count < 2) { throw new Exception("Too many operations!"); }
                Proposition q = tokens.Pop();
                Proposition p = tokens.Pop();
                tokens.Push(new Conjunction(ref p, ref q));
            }
            else if (c == '=')
            {
                if (tokens.Count < 2) { throw new Exception("Too many operations!"); }
                Proposition q = tokens.Pop();
                Proposition p = tokens.Pop();
                tokens.Push(new Biconditional(ref p, ref q));
            }
            else
            {
                tokens.Push(new AtomicPlaceholder(c + ""));
            }

        }
        if (tokens.Count == 1) { return tokens.Pop(); }
        throw new Exception("Empty string, or too many letters!");
    }

    public static Theorem[][] theorems = new Theorem[][]
    {
        //misc. 
        new Theorem[]
        {
        new Theorem("QPQ>>"),
        new Theorem("PP~~="), //double negation
        new Theorem("PPQ>^Q>"), //modus ponens
        new Theorem("Q~PQ>^P~>"), //modus tollens
        new Theorem("PQ>Q~P~>="), //contrapositive
        new Theorem("P~Q>Q~P>="), //contrapositive2
        new Theorem("PPQ>>PQ>>"), //absorption?

        new Theorem("PQvQPv="), //commutivities
        new Theorem("PQ^QP^="),
        new Theorem("PQ=QP=="),

        new Theorem("PQvRvPQRvv="), //associativities
        new Theorem("PQ^R^PQR^^="),
        new Theorem("PQ=R=PQR==="),

        new Theorem("PQ>QP>^PQ=="), // two cond. to bicond.
        new Theorem("PQ>P~Qv="), // cond. to disj.
        new Theorem("PQ>~PQ~^="), // cond. to negative conj.

        new Theorem("PPPv="), // idempotencies
        new Theorem("PPP^="),

        new Theorem("PPQ^vP="), //absorptions
        new Theorem("PPQv^P="),

        new Theorem("P~P>P="),
        new Theorem("PP~>P~="),

        new Theorem("PQ^~P~Q~v="), //de morgans
        new Theorem("PQv~P~Q~^="),
        new Theorem("PQ^P~Q~v~="),
        new Theorem("PQvP~Q~^~="),

        new Theorem("PQ>P~Q>^Q>"), //separation of cases
        
        new Theorem("PQ^P>"), //simplify
        new Theorem("QP^P>"), //simplify

        new Theorem("PQ=~PQ~=="), //negating bicond.
        new Theorem("PQ=PQ~=~="),

        new Theorem("PR>PQ>QR>^="),

        //more later if needed
        },
        
        //atomic. t must be true. f must be false.
        new Theorem[]
        {
            new Theorem("Pt^P>"),
            new Theorem("Pf^P>"),
            new Theorem("tP>P>"),
            new Theorem("tP=P>"),
            new Theorem("fP~=P>"),
            new Theorem("PPvP>"),
            new Theorem("P~P>P>"),
            new Theorem("P~~P>"),
            new Theorem("PfvP>")
        },

        // negation. t must be true. f must be false.
        new Theorem[]
        {
            new Theorem("Pf>P~>"),
            new Theorem("PP~>P~>"),
            new Theorem("P~~~P~>"),
            new Theorem("Pt^~P~>"),
        },

        //conditional.
        new Theorem[]
        {
            new Theorem("PQ~^~PQ>>"),
            new Theorem("PPQ>>PQ>>"),
            new Theorem("Q~P~>PQ>>"),
            new Theorem("P~QvPQ>>"),
            new Theorem("QP=PQ>>"),
            new Theorem("PQ=PQ>>"),
            new Theorem("P~PQ>>"),
            new Theorem("QPQ>>"),
        },

        //disjunction.
        new Theorem[]
        {
            new Theorem("QPvPQv>"),
            new Theorem("P~Q>PQv>"),
            new Theorem("PQ^PQv>"),
            new Theorem("PPQv>"),
            new Theorem("QPQv>"),
            new Theorem("P~Q~^~PQv>"),
        },

        //conjunction.
        new Theorem[]
        {
            new Theorem("QP^PQ^>"),
            new Theorem("PPQ>^PQ^>"),
            new Theorem("QQP>^PQ^>"),
            new Theorem("P~Q~v~PQ^>"),
            new Theorem("PQ~>~PQ^>"),
            new Theorem("QP~>~PQ^>"),
        },

        //biconditional.
        new Theorem[]
        {
            new Theorem("QP=PQ=>"),
            new Theorem("PQ^PQ=>"),
            new Theorem("P~Q~^PQ=>"),
            new Theorem("PQv~PQ=>"),
            new Theorem("PQ>QP>^PQ=>"),
            new Theorem("PQ~=~PQ=>"),
            new Theorem("P~Q=~PQ=>"),
        },
    };

    public bool TheoremPartFits(ref Proposition theoremPart, ref Proposition main, ref Dictionary<string, Proposition> replacements)
    {
        if (theoremPart is AtomicPlaceholder)
        {
            string atomicLetter = ((AtomicPlaceholder)theoremPart).letter;
            if (replacements.ContainsKey(atomicLetter)) { return replacements[atomicLetter].Equals(main); }
            else { replacements.Add(atomicLetter, main); return true; }
        }

        if (theoremPart is Negation)
        {
            if (main is Negation) { return TheoremPartFits(ref ((Negation)theoremPart).p, ref ((Negation)main).p, ref replacements); }
            return false;
        }

        if (theoremPart is Conditional)
        {
            if (main is Conditional)
            {
                return TheoremPartFits(ref ((Conditional)theoremPart).p, ref ((Conditional)main).p, ref replacements)
                    && TheoremPartFits(ref ((Conditional)theoremPart).q, ref ((Conditional)main).q, ref replacements);
            }
            return false;
        }

        if (theoremPart is Disjunction)
        {
            if (main is Disjunction)
            {
                return TheoremPartFits(ref ((Disjunction)theoremPart).p, ref ((Disjunction)main).p, ref replacements)
                    && TheoremPartFits(ref ((Disjunction)theoremPart).q, ref ((Disjunction)main).q, ref replacements);
            }
            return false;
        }

        if (theoremPart is Conjunction)
        {
            if (main is Conjunction)
            {
                return TheoremPartFits(ref ((Conjunction)theoremPart).p, ref ((Conjunction)main).p, ref replacements)
                    && TheoremPartFits(ref ((Conjunction)theoremPart).q, ref ((Conjunction)main).q, ref replacements);
            }
            return false;
        }

        if (theoremPart is Biconditional)
        {
            if (main is Biconditional)
            {
                return TheoremPartFits(ref ((Biconditional)theoremPart).p, ref ((Biconditional)main).p, ref replacements)
                    && TheoremPartFits(ref ((Biconditional)theoremPart).q, ref ((Biconditional)main).q, ref replacements);
            }
            return false;
        }

        return false;
    }

    private int testKey = 1;

    private string AddGrammar(string original)
    {
        string result = original;
        foreach (string sub in Atomic.sampleStrings)
        {
            result = result.Replace("if " + sub, "if one should " + sub);
            result = result.Replace("if don't " + sub, "if one shouldn't " + sub);
            result = result.Replace("when " + sub, "when one should " + sub);
            result = result.Replace("when don't " + sub, "when one shouldn't " + sub);
            result = result.Replace("unless " + sub, "unless one should " + sub);
            result = result.Replace("unless don't " + sub, "unless one shouldn't " + sub);
            result = result.Replace("that " + sub, "to " + sub);
            result = result.Replace("that don't " + sub, "to not " + sub);
            result = result.Replace("that \"don't " + sub, "to \"not " + sub);
        }
        result = result[0].ToString().ToUpper() + result.Substring(1) + ".";
        return result;
    }

    // Here we're assuming that theoremPart and main "fit" (above). If no replacement, the placeholder will remain.
    public Proposition ReplaceTheoremPart(Proposition theoremPart, ref Dictionary<string, Proposition> replacements, ref List<Proposition> givens)
    {
        if (theoremPart is AtomicPlaceholder) {
            string letter = ((AtomicPlaceholder)theoremPart).letter;
            if (replacements.ContainsKey(letter)) { return replacements[letter]; }
            else if (letter == "t") { return givens[Fakerand.Int(0, givens.Count)]; }
            else if (letter == "f") {
                Proposition temp = givens[Fakerand.Int(0, givens.Count)];
                if (temp is Negation) { return ((Negation)temp).p; }
                return new Negation(temp);
            }
            else { replacements.Add(letter, new Atomic("[" + testKey + "]")); ++testKey; return replacements[letter]; } // for testing
            //else { return theoremPart; }
        }

        if (theoremPart is Negation) {
            //we don't like repeat negatives too much. If there are three or more negatives, delete two of them.
            if (((Negation)theoremPart).p is Negation && ((Negation)((Negation)theoremPart).p).p is Negation) // that's a hard three
            {
                return new Negation(ReplaceTheoremPart(((Negation)((Negation)((Negation)theoremPart).p).p).p, ref replacements, ref givens));
            }
            return new Negation( ReplaceTheoremPart(((Negation)theoremPart).p, ref replacements, ref givens) );
        }

        if (theoremPart is Conditional)
        {
            return new Conditional(
                ReplaceTheoremPart(((Conditional)theoremPart).p, ref replacements, ref givens),
                ReplaceTheoremPart(((Conditional)theoremPart).q, ref replacements, ref givens));
        }

        if (theoremPart is Disjunction)
        {
            return new Disjunction(
                ReplaceTheoremPart(((Disjunction)theoremPart).p, ref replacements, ref givens),
                ReplaceTheoremPart(((Disjunction)theoremPart).q, ref replacements, ref givens));
        }

        if (theoremPart is Conjunction)
        {
            return new Conjunction(
                ReplaceTheoremPart(((Conjunction)theoremPart).p, ref replacements, ref givens),
                ReplaceTheoremPart(((Conjunction)theoremPart).q, ref replacements, ref givens));
        }

        if (theoremPart is Biconditional)
        {
            return new Biconditional(
                ReplaceTheoremPart(((Biconditional)theoremPart).p, ref replacements, ref givens),
                ReplaceTheoremPart(((Biconditional)theoremPart).q, ref replacements, ref givens));
        }

        return null; //???
    }

    public Proposition TheoremUnapply(ref Theorem theorem, ref Proposition main, ref List<Proposition> givens) //returns null if theorem does not apply
    {
        Dictionary<string, Proposition> replacements = new Dictionary<string, Proposition>();

        if (theorem.prop is Conditional) // only one possibility, to match q
        {
            bool fits = TheoremPartFits(ref ((Conditional)theorem.prop).q, ref main, ref replacements);
            if (!fits) { return null; }
            return ReplaceTheoremPart(((Conditional)theorem.prop).p, ref replacements, ref givens);
        }

        if (theorem.prop is Biconditional) // it could match either p or q. Choose randomly!
        {
            Biconditional thisBicon = (Biconditional)theorem.prop;
            if (Fakerand.Int(0, 2) == 1) { thisBicon = new Biconditional(thisBicon.q, thisBicon.p); }

            bool fits = TheoremPartFits(ref thisBicon.q, ref main, ref replacements);
            if (!fits) //try the other way
            {
                fits = TheoremPartFits(ref thisBicon.p, ref main, ref replacements);
                if (!fits) { return null; }
                return ReplaceTheoremPart(thisBicon.q, ref replacements, ref givens);
            }
            return ReplaceTheoremPart(thisBicon.p, ref replacements, ref givens);
        }

        

        return null;
    }

    private uint MaxDifficulty(ref List<Proposition> statements)
    {
        if (statements.Count == 0) { return 0; }
        uint res = 0;
        for (int i = 0; i < statements.Count; ++i) { if (res < statements[i].size) { res = statements[i].size; } }
        return res;
    }

    private void ListSubprops(ref List<Proposition> list, ref Proposition main)
    {
        list.Add(main);
        if (main is Negation)
        {
            ListSubprops(ref list, ref ((Negation)main).p);
        }
        else if (main is BinaryConnector)
        {
            ListSubprops(ref list, ref ((BinaryConnector)main).p);
            ListSubprops(ref list, ref ((BinaryConnector)main).q);
        }
    }

    private void ReplaceSubprop(ref Proposition replacement, ref Proposition matcher, ref Proposition main)
    {
        if (main.Equals(matcher))
        {
            main = replacement;
            return;
        }
        else if (main is Negation)
        {
            ReplaceSubprop(ref replacement, ref matcher, ref ((Negation)main).p);
        }
        else if (main is BinaryConnector)
        {
            if (Fakerand.Int(0,2) == 1)
            {
                ReplaceSubprop(ref replacement, ref matcher, ref ((BinaryConnector)main).p);
                ReplaceSubprop(ref replacement, ref matcher, ref ((BinaryConnector)main).q);
            }
            else
            {
                ReplaceSubprop(ref replacement, ref matcher, ref ((BinaryConnector)main).q);
                ReplaceSubprop(ref replacement, ref matcher, ref ((BinaryConnector)main).p);
            }
        }
    }

    private void CorrectSize(ref Proposition main) // ???
    {
        if (main is Atomic || main is AtomicPlaceholder) { main.size = 1; }
        else if (main is Negation)
        {
            CorrectSize(ref ((Negation)main).p);
            main.size = ((Negation)main).p.size + 1;
        }
        else if (main is BinaryConnector) {
            CorrectSize(ref ((BinaryConnector)main).p);
            CorrectSize(ref ((BinaryConnector)main).q);
            main.size = ((BinaryConnector)main).p.size + ((BinaryConnector)main).q.size + 1;
        }
    }

    public List<Proposition> GeneratePuzzle(uint length, float difficulty, out string correctLetter)
    {
        if (length < 1) { length = 1; }
        if (length > 3) { length = 3; }
        List<string> letters = new List<string>() { "L", "R", "△" };
        int correctIdx = Fakerand.Int(0, letters.Count);
        correctLetter = letters[correctIdx];
        letters.RemoveAt(correctIdx);
        List<Proposition> statements = new List<Proposition>(){ new Atomic(correctLetter) };
        while (statements.Count < length)
        {
            int supNum = Fakerand.Int(0, letters.Count);
            statements.Add(new Negation(new Atomic(letters[supNum], -1, false)));
            letters.RemoveAt(supNum);
        }

        //make the puzzle harder
        int i = 0;
        while (i < difficulty && MaxDifficulty(ref statements) < difficulty)
        {
            int newIdx = 0; //Fakerand.Int(0, statements.Count);
            Proposition newProp = statements[newIdx].Clone();
            List<Proposition> subProps = new List<Proposition>();
            ListSubprops(ref subProps, ref newProp);
            Proposition subProp = subProps[Fakerand.Int(0, subProps.Count)];
            Proposition newSubProp;
            bool treatAsAtomic = (newProp.size < 4 && Fakerand.Int(0, (int)newProp.size) == newProp.size - 1);

            Theorem thm = theorems[0][0]; //default
            
            if (treatAsAtomic) { Atomic temp = new Atomic(""); thm = temp.theorems[Fakerand.Int(0, temp.theorems.Length)]; }
            else { thm = subProp.theorems[Fakerand.Int(0, subProp.theorems.Length)]; }

            List<Proposition> statements2 = new List<Proposition>(statements);
            statements2.RemoveAt(newIdx);

            newSubProp = TheoremUnapply(ref thm, ref subProp, ref statements2);
            if (!newSubProp.truthValue) { continue; }

            ReplaceSubprop(ref newSubProp, ref subProp, ref newProp);
            CorrectSize(ref newProp);
            statements[newIdx] = newProp;

            ++i;
            statements = statements.OrderBy(a => a.size + Fakerand.Int(0, Mathf.CeilToInt(a.size/2f))).ToList();
        }

        return statements.OrderBy(a => Guid.NewGuid()).ToList();
    }

    #endregion

    [SerializeField]
    private int puzzlesCompleted = 0;
    private const int puzzlesNeeded = 8;
    [SerializeField]
    private SpriteRenderer[] lamps = new SpriteRenderer[8];
    [SerializeField]
    private Sprite lampOff;
    [SerializeField]
    private Sprite lampOn;
    [SerializeField]
    private PrimExaminableItem submitCollider;
    [SerializeField]
    private MainTextsStuff beforeChoiceTextStuff;
    [SerializeField]
    private MainTextsStuff wrongTextStuff;
    [SerializeField]
    private MainTextsStuff rightTextStuff;
    [SerializeField]
    private GameObject wrongTextBox;
    [SerializeField]
    private GameObject rightTextBox;
    [SerializeField]
    private Text mainTextDisplay;
    [SerializeField]
    private GameObject pausedBox;
    [SerializeField]
    private primDecorationMoving chainLifter;
    [SerializeField]
    private Camera lightsOutCamera;
    [SerializeField]
    private Transform mainTextBoxHolder;

    public SpriteRenderer passSprite;
    public SpriteRenderer failSprite;

    [SerializeField]
    private AmbushController ambushController;

    [SerializeField]
    private GameObject crawlerPrefab;
    [SerializeField]
    private GameObject missilePrefab;


    private Vector3 chainLiftFirstPosition;

    private string choiceResponse;
    private float punishTime;
    private float lowerTime;

    private string[] beforeTextChoices =
    {
        "Ten seconds to submit the choice.",
        "No sweat.",
        "No time to second-guess.",
        "It has to be one of them.",
        "Choose the perfect answer.",
        "Choose the Mathematical answer.",
        "The test is almost over.",
        "Don't choke.",
    };

    private string[] wrongTextChoices =
    {
        "<shake>FAIL!",
        "<shake>WRONG!",
        "<shake>FALSE!"
    };

    private string[] rightTextChoices =
    {
        "Correct.",
        "Well done.",
        "True."
    };

    enum WackyBehavior
    {
        None, Crawlers, LightsOut, WaveText, RevealFacts, UpsideDownText, Missiles//, RotateLevel
    }

    private GameObject bareChoiceBox;

    private void UpdateLamps()
    {
        for (int i = 0; i < puzzlesNeeded; ++i)
        {
            lamps[i].sprite = (puzzlesCompleted > i) ? lampOn : lampOff;
        }
    }

    public GameObject ChoiceResponse(string text)
    {
        choiceResponse = text;
        return null;
    }

    private IEnumerator LightsOut()
    {
        int currPuzzlesCompleted = puzzlesCompleted;
        while (currPuzzlesCompleted == puzzlesCompleted)
        {
            yield return new WaitForSeconds(Fakerand.Single(1f, 3f - 0.5f * (puzzlesCompleted - 4)));
            mainTextDisplay.transform.localPosition += new Vector3(0, 800, 0);
            lightsOutCamera.enabled = true;
            yield return new WaitForSeconds(Fakerand.Single(0.25f + 0.1f * (puzzlesCompleted - 4), 2f + 0.5f * (puzzlesCompleted - 4)));
            mainTextDisplay.transform.localPosition -= new Vector3(0, 800, 0);
            lightsOutCamera.enabled = false;
        }
        yield return null;
    }

    private IEnumerator RevealFacts(List<Proposition> puzzle)
    {
        int currPuzzlesCompleted = puzzlesCompleted;
        string[] bubbles = { "① ", "② ", "③ " };
        string internalString = "";
        for (int i = 0; i < 3; ++i)
        {
            if (currPuzzlesCompleted != puzzlesCompleted || internalString != mainTextDisplay.text) { yield break; }
            mainTextDisplay.text += bubbles[i] + AddGrammar(puzzle[i].ToString()) + "\n";
            internalString = mainTextDisplay.text;
            yield return new WaitForSeconds(3.6f + 0.3f * (puzzlesCompleted - 4));
        }
        yield return null;
    }

    private IEnumerator RotateLevel()
    {
        int currPuzzlesCompleted = puzzlesCompleted;
        int sign = (Fakerand.Int(0, 2) == 0) ? 1 : -1;
        Transform at = LevelInfoContainer.allPlayersInLevel[0].transform;
        while (currPuzzlesCompleted == puzzlesCompleted && mainTextDisplay.text != "")
        {
            if (Time.timeScale == 0) { yield return new WaitForEndOfFrame(); continue; }
            at.localEulerAngles += new Vector3(0, 0, sign * 0.08f * (-3 + puzzlesCompleted));
            mainTextBoxHolder.localEulerAngles = -at.localEulerAngles;
            yield return new WaitForEndOfFrame();
        }
        int ti = 0;
        while (ti++ < 20)
        {
            at.localEulerAngles = new Vector3(at.localEulerAngles.x, at.localEulerAngles.y, Mathf.LerpAngle(at.localEulerAngles.z, 0f, 0.17f));
            mainTextBoxHolder.localEulerAngles = -at.localEulerAngles;
            yield return new WaitForEndOfFrame();
        }
        at.localEulerAngles = new Vector3(at.localEulerAngles.x, at.localEulerAngles.y, 0f);
        mainTextBoxHolder.localEulerAngles = -at.localEulerAngles;
        yield return null;
    }

    /*private IEnumerator RotateTextObject()
    {
        int currPuzzlesCompleted = puzzlesCompleted;
        while (currPuzzlesCompleted == puzzlesCompleted)
        {
            mainTextDisplay.transform.localScale = new Vector3(mainTextDisplay.transform.localScale.x, 
                Mathf.Sqrt(Mathf.Abs((float)Math.Sin(DoubleTime.ScaledTimeSinceLoad*3.14f))), 
                1f);
            yield return new WaitForEndOfFrame();
            if (mainTextDisplay.text == "") { break; }
        }
        yield return null;
    }*/

    /*private IEnumerator SpawnCrawlers()
    {
        int currPuzzlesCompleted = puzzlesCompleted;
        while (currPuzzlesCompleted == puzzlesCompleted)
        {
            StartCoroutine(ambushController.SpawnIntoAmbush(crawlerPrefab));
            yield return new WaitForSeconds(10f - 0.5f * (puzzlesCompleted - 4));
        }
        yield return null;
    }*/

    private IEnumerator SpawnMissiles()
    {
        int currPuzzlesCompleted = puzzlesCompleted;
        int it = 0;
        while (currPuzzlesCompleted == puzzlesCompleted && it++ < -1 + puzzlesCompleted)
        {
            StartCoroutine(ambushController.SpawnIntoAmbush(missilePrefab));
            yield return new WaitForSeconds(5f - 0.05f*(puzzlesCompleted - 4));
        }
        yield return null;
    }

    private IEnumerator GivePuzzle()
    {
        choiceResponse = "";

        string correctLetter;
        mainTextDisplay.text = "";

        int difficulty = Mathf.Min((int)((puzzlesCompleted + 2.5f)/1.2f),4);

        List<Proposition> puzzle = GeneratePuzzle(3, difficulty, out correctLetter);

        string[] bubbles = { "① ", "② ", "③ " };
        for (int i = 0; i < 3; ++i)
        {
            mainTextDisplay.text += bubbles[i] + AddGrammar(puzzle[i].ToString()) + "\n";
        }

        WackyBehavior wack = WackyBehavior.None;
        if (puzzlesCompleted >= 4) wack = (WackyBehavior)Fakerand.Int(1, 8);

        switch(wack)
        {
            case WackyBehavior.WaveText:
                mainTextDisplay.GetComponent<ModifyOtherText>().wave = 2f + 2f * (puzzlesCompleted - 4);
                break;
            case WackyBehavior.LightsOut:
                StartCoroutine(LightsOut());
                break;
            case WackyBehavior.UpsideDownText:
                if (Fakerand.Int(0, 2) == 0) { mainTextDisplay.transform.localEulerAngles = new Vector3(0, 0, 180); }
                else { mainTextDisplay.transform.localScale = new Vector3(-1, 1, 1); }
                //if (puzzlesCompleted >= 6) { StartCoroutine(RotateTextObject()); }
                break;
            case WackyBehavior.RevealFacts:
                mainTextDisplay.text = "";
                StartCoroutine(RevealFacts(puzzle));
                break;
            /*case WackyBehavior.RotateLevel:
                StartCoroutine(RotateLevel());
                break;*/
            case WackyBehavior.Crawlers:
                StartCoroutine(ambushController.SpawnIntoAmbush(crawlerPrefab));
                break;
            case WackyBehavior.Missiles:
                StartCoroutine(SpawnMissiles());
                break;
            default:
            case WackyBehavior.None:
                break;
        }
        
        

        beforeChoiceTextStuff.messages[0] = new MainTextsStuff.MessageData(beforeTextChoices[puzzlesCompleted]);

        beforeChoiceTextStuff.choiceOutcomeObjects = new MainTextsStuff.ObjectsWorkaround[3];
        beforeChoiceTextStuff.choiceOutcomeObjects[0].objects = new GameObject[2] { gameObject, (correctLetter == "L") ? rightTextBox : wrongTextBox };
        beforeChoiceTextStuff.choiceOutcomeObjects[1].objects = new GameObject[2] { gameObject, (correctLetter == "R") ? rightTextBox : wrongTextBox };
        beforeChoiceTextStuff.choiceOutcomeObjects[2].objects = new GameObject[2] { gameObject, (correctLetter == "△") ? rightTextBox : wrongTextBox };

        wrongTextStuff.messages[0] = new MainTextsStuff.MessageData(wrongTextChoices[Fakerand.Int(0, wrongTextChoices.Length)],1f,new Color(1f,0.2f,0.2f));
        if (DoubleTime.ScaledTimeSinceLoad < 3f + 1f*puzzlesCompleted && Fakerand.Int(0,2) == 0)
        {
            rightTextStuff.messages[0] = new MainTextsStuff.MessageData(rightTextChoices[Fakerand.Int(0, rightTextChoices.Length)] + ".. looks like we have a TAS.", 1f, new Color(0f, 0.7f, 1f));
        }
        else
        {
            rightTextStuff.messages[0] = new MainTextsStuff.MessageData(rightTextChoices[Fakerand.Int(0, rightTextChoices.Length)], 1f, new Color(0f, 0.7f, 1f));
        }
        submitCollider.gameObject.SetActive(true);

        bool success = false;

        yield return new WaitUntil(() => {
            return (choiceResponse != "") || KHealth.someoneDied || PauseMenuMain.gameIsPausedThroughMenu;
        });
        if (KHealth.someoneDied) { yield break; }

        mainTextDisplay.text = "";
        success = (choiceResponse == correctLetter);

        if (PauseMenuMain.gameIsPausedThroughMenu)
        {
            success = false;

            yield return new WaitUntil(() => { return Time.timeScale > 0; });
            yield return new WaitForEndOfFrame();

            GameObject made = Instantiate(pausedBox);
            made.SetActive(true);
            made.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
        }

        if (success) {
            ++puzzlesCompleted;
            lowerTime += 3f;
        } else {
            failSprite.enabled = true;
            yield return new WaitForSeconds(punishTime);
            punishTime += 1f;
        }
        UpdateLamps();
        mainTextDisplay.transform.localEulerAngles = Vector3.zero;
        mainTextDisplay.transform.localScale = Vector3.one;
        mainTextDisplay.GetComponent<ModifyOtherText>().wave = 0;
        yield return new WaitUntil(() => { return Time.timeScale > 0; });
        submitCollider.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.25f);
        passSprite.enabled = failSprite.enabled = false;
        if (puzzlesCompleted < puzzlesNeeded) { StartCoroutine(GivePuzzle()); }
        else {
            submitCollider.gameObject.SetActive(false);
            passSprite.enabled = true;
            while ((chainLifter.transform.position.y - chainLiftFirstPosition.y) > 1f
                || lightsOutCamera.enabled)
            {
                lowerTime = 100f;
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        passSprite.enabled = failSprite.enabled = false;
        puzzlesCompleted = 0;
        punishTime = 0.75f;
        chainLiftFirstPosition = chainLifter.transform.position;
        StartCoroutine(GivePuzzle());
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }

        if (lowerTime >= 0f)
        {
            chainLifter.v = new Vector2(0, -160);
            lowerTime -= 0.01666666f * Time.timeScale;
        }
        else
        {
            chainLifter.v = new Vector2(0, 32f);
        }
    }
}
