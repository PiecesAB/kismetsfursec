using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNamer
{
    public static readonly Dictionary<string, string[]> all = new Dictionary<string, string[]>()
    {
        {"bat",new string[]{"Joann","Vir","Elise","Twilight","Succ","Ma","Luis","Pierre","Rudy","Sugus","Dingbat","Bart"}},
        // enjoying weird but ethnic names for the skull enemy might be seen as offensive.
        {"skull", new string[]{"Muerto"}},
        {"tank", new string[]{"Sparker","Thomas","Heavy","Fuze","Snafu","Bertha","Armata","Pulver","Rocky"}},
        {"crawler", new string[]{"Rainbow","Barcode","Googol","Ping","Pong","Ipconfig","Spidulus","Walker","Lazer","Pokey"}},
        {"gnat", new string[]{"Pesto","Flux","Blitter","Flitter","Gnancy","Gnorman","Wingman","Malaria"}},
        {"bee", new string[]{"Vivaldi","Stingy","Polly","Hivemind","Triangle","Worker","Drone"}},
        {"thumpr", new string[]{"Thumpr","Snowball",">:3","Qubo","Hops"}},
        {"bread", new string[]{"Rye-an","BLT","Grainy","Breadman","Doughson"}},
        {"minister", new string[]{"John","Luke","Augustine","Bishop","Matthew","Joshua","Ruth"}},
        {"dolphin", new string[]{ "-=|+_=+", "|||'|-=+", "-|=-:=.", "+_+_\"\"", "-|=:::'.'", "__=-=+-:", "+++::+|+|"  } },
        {"blackhole", new string[]{"Messier","Hercules","NGC","Markarian","Holmberg","Hawking","Lorentz","Solaris" } },
        {"butterfly", new string[]{"Monarch","Vlinder","Paloma","Papillon","Mariposa","Msn-ger","Yuyuko","Tabuu"} }
    };
}
