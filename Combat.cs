//&RunInOwnWindow

using System;
using System.Threading;

namespace QuickSharp
{
    public class Program
    {
        private static int con_enemy = 2, con_player = 6, con_hist = 12, con_selection = 10;
        private static Entity enemy = new Entity(new AI(), new Attributes(10,10,10));
        private static Random randomSeed = new Random();
        private static ConsoleHistory history = new ConsoleHistory();
        private static Attack[] attacks = new Attack[4];
        private static void Main()
        {
            attacks[0] = new Attack("Hit", 2);
            attacks[1] = new Attack("Stab", 10);
            attacks[2] = new Attack("Slice", 12);
            attacks[3] = new Attack("Combo", 20);
            
            Console.WriteLine("Go!");
            while(enemy.hp > 0){
                
                string st;
                int a = -1;
                Console.CursorTop = con_enemy;
                st = enemy.name;
                Console.CursorLeft = (Console.WindowWidth/2)-(st.Length/2);
                Console.WriteLine(st);
                Console.CursorTop++;
                if(enemy.hp > 0) st = "HP: "+enemy.hp+" / "+enemy.maxhp;
                else st = "Dead";
                Console.CursorLeft = (Console.WindowWidth/2)-(st.Length/2);
                Console.WriteLine(st); 
                Console.CursorLeft = (Console.WindowWidth/2)-(("Player").Length/2);
                Console.CursorTop = con_player;
                Console.WriteLine("Player");
                Console.CursorLeft = (Console.WindowWidth/2)-(("100 / 100").Length/2);
                Console.CursorTop = con_player+1;
                Console.WriteLine("100 / 100");
                while((a < 0) && (a >= attacks.Length)) a = AttackChoice();
                Attack att;
                if(a != -1) att = attacks[AttackChoice()];
                else att = new Attack("", 0);
                int dmgSeed = att.dmg_base;
                
                int dmg = randomSeed.Next(dmgSeed, (dmgSeed*2));//-(100/(enemy.stats.str/2));
                enemy.hp += (dmg*-1);
                history.Add("Enemy " + enemy.name + " took " + dmg + " damage.");              
                
                Console.CursorTop = con_hist;
                Console.CursorLeft = 0;
                foreach(string s in history.Get())
                {
                    if(s != "") Console.WriteLine(s);
                }
                Thread.Sleep(1000);
                
            }
            Console.ReadKey(true);
        
        }
        
        private static int AttackChoice()
        {
            Console.CursorTop = con_selection;
            string s = " 1. Hit [0]  2. Stab [1]  3. Slice [1]  4. Combo [2] ";
            Console.CursorLeft = (Console.WindowWidth/2)-(s.Length/2);
            Console.Write(s);
            ConsoleKeyInfo UserInput = Console.ReadKey(true);
            int inp = -1;
            if (char.IsDigit(UserInput.KeyChar))
                 inp = (int.Parse(UserInput.KeyChar.ToString())-1); // use Parse if it's a Digit
            else inp = -1;  // Else we assign a default value
            return inp;
        }
    }
    
    public class Entity
    {      
        public string id, name, description;
        public Attributes stats;
        public int hp = 100, maxhp = 100;
        public AI ai;
        
        public Entity(AI a, Attributes stts){ name="spider"; stats = stts; maxhp = 100+(100/stats.end); hp=maxhp;}
        
    }   
    
    
    public class AI {
        public enum States { Sleep, Aware, Hostile, Calm};
        public States state;
        public AI(){}
    }
    
    public struct Attributes
    {
        public int str, dex, end;
        public Attributes(int s, int d, int e) { str = s; dex = d; end = e; }
        
    }
    
    public struct Attack
    {
        public string name;
        public int dmg_base;
        
        public Attack(string n, int dmg){ name = n; dmg_base = dmg; }
    }
    
    public class ConsoleHistory
    {
        private string[] history = new string[5];
        
        public ConsoleHistory(){;}
        
        public void Add(string h)
        {
            for(int i = history.Length-1; i >= 1; i--)
            {
                history[i] = history[i-1];
            }
            history[0] = h;
        }
        
        public string[] Get(){
            return history;
        }
        
        
    }
}
