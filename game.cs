// Compiler commands

//& RunInOwnWindow
//$ GameUtils.cs


/*
Todo:
Fix movement
Items - a/an/the
Load synonyms for commands from file
Wear/Equip item
Last door used (for backing out of dark cells)
Box inventories (chests, items not simply on floor, etc)
Store/Put item (in box)
Dynamic descriptions
Door/Key/Lock system
Mechanisms (levers for doors, etc)
Time system
Weather system
Abilities/unlockable commands
Files/Books
Achievements/Statistics
Re-join inputs after 1st command found (2, 3, 4 etc become index 2) to allow multi word descriptions (blue book, etc)

///Refactoring
Split into cs files
Words from game into lang file
Custom Exceptions
Catch failed add item to inventory ids
*/

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using GameUtils;


namespace TextAdventureGame
{

    public class MainGame
    {
        
        public static Game G = new Game();
        private static World W = new World("game.xml");
        private static Display D = new Display();                
        private static Player P = new Player();
        private static bool roomdesc = false;
        private static string lastinput = "";

        public enum Inputs { Exit, DoExit, Yes, No, Null, Help, Invalid,
                             North, South, East, West, Up, Down, NorthW, NorthE, SouthW, SouthE,
                             Examine, InvToggle, Time, Take, Drop};
        public static Inputs[] directions = new Inputs[10] {Inputs.North, Inputs.East, Inputs.West, Inputs.South,
                        Inputs.NorthW, Inputs.NorthE, Inputs.SouthE, Inputs.SouthW, Inputs.Up, Inputs.Down};
        
        private static Inputs lastcmd = Inputs.Null;
        
        private static void Main(string[] args)
        {
            G.SetVersion("0.0.0.6a");
            G.XMLSetup("game.xml");
            D.Configure(G.GetTitle());

            DisplayWelcome();    
            WaitForKey(ConsoleKey.Enter);

            P.MoveTo(G.GetStart());
            P.SetName("Dave");
            P.UpdateScore(0);
            try {P.clothing.Add((Item)W.Get(typeof(Item), "watch")); }catch (ArgumentNullException) {} catch (ArgumentException) { D.feedback = "Cannot wear that"; }
            
            
            Inputs inp = Inputs.Null;
            do {
                inp = GameLoop();
            } while (inp != Inputs.DoExit) ;
            
            D.Wipe();
        }
        
        private static Inputs GameLoop(){
            string Feedback = "";
            
            //if(Array.BinarySearch(directions, lastcmd) >= 0){
            //    try{P.MoveTo(W.GetDoorFrom(P.Locate(), lastcmd).toID); roomdesc = false;
            //    } catch (Exception) { Feedback = G.String("$noentry"); }                
            //}
            foreach(Inputs inp in directions){
                if(lastcmd == inp) try {P.MoveTo(W.GetDoorFrom(P.Locate(), lastcmd).toID); roomdesc = false;
                } catch (Exception) { Feedback = G.String("$noentry"); } 
            }
            
            if(lastcmd == Inputs.Take) Feedback = PickupItem(lastinput);
            if(lastcmd == Inputs.Drop) Feedback = DropItem(lastinput);
            if(lastcmd == Inputs.Time) Feedback = P.CheckTime();
            if(lastcmd == Inputs.InvToggle) D.showinv = !D.showinv;
            if(lastcmd == Inputs.Examine) Feedback = Examine(lastinput);
            if(lastcmd == Inputs.Help) D.WriteHelp();
            if(lastcmd == Inputs.Invalid) Feedback = G.String("$invalid") + lastinput;
            if(lastcmd == Inputs.Exit) Feedback = G.String("$exitq");
            
            D.Wipe();
            DrawGUI();
            D.WriteLeft(((Cell)W.Get(typeof(Cell),P.Locate())).name);
            D.Inc();
            if(!roomdesc) Feedback = Examine("examine room");
            if(Feedback != ""){
                if(lastinput != ""){ D.WriteLeft("-> "+ lastinput); D.Inc(); }
                D.WriteLeft(Feedback);
                D.Inc();
            }
                                   
            lastinput = D.UserPrompt();
            if((lastcmd == Inputs.Exit)&&(RequestCommand(lastinput) == Inputs.Yes)) return Inputs.DoExit;
            else lastcmd = RequestCommand(lastinput);
            
            return Inputs.Null; //end loop
        }
        
        private static string Examine(string input)
        {
            input = input.ToLower();
            input = input.Trim();
            string[] inpArr = input.Split(' ');
            if(inpArr.Length != 2){ return  G.String("$noexamine"); }
            if((inpArr[1] == "room") || (inpArr[1] == "cell") || (inpArr[1] == "area")){
                roomdesc = true;
                return ((Cell)W.Get(typeof(Cell), P.Locate())).Describe();
            }            
            if(!((Cell)W.Get(typeof(Cell), P.Locate())).lit){ return G.String("$exdark"); }

            if(P.backpack.ItemFromKeyword(inpArr[1]) != null) return P.backpack.ItemFromKeyword(inpArr[1]).fulldescription;
            if(P.clothing.ItemFromKeyword(inpArr[1]) != null) return P.clothing.ItemFromKeyword(inpArr[1]).fulldescription;

            return G.String("$noexamine");
            //check room inventory
            
        }
        
        private static string PickupItem(string input)
        {
            input = input.ToLower();
            input = input.Trim();
            string[] inpArr = input.Split(' ');
            if(inpArr.Length != 2){ return  G.String("$nopickup"); }
            Cell c = (Cell)W.Get(typeof(Cell), P.Locate());
            if(c.lit){
                Item it = c.TakeItem(inpArr[1]);
                if(it == null) return G.String("$nopickup");
                P.backpack.Add(it);
                return G.String("$pickedup") + it.shortdescription;
            }            
            return G.String("$nopickup");            
        }
        
        private static string DropItem(string input)
        {
            input = input.ToLower();
            input = input.Trim();
            string[] inpArr = input.Split(' ');
            if(inpArr.Length != 2){ return  G.String("$nodrop"); }
            Cell c = (Cell)W.Get(typeof(Cell), P.Locate());
            Item it = P.DropItem(inpArr[1]);
            if(it == null) return G.String("$nodrop");
            c.PutItem(it);
            return G.String("$dropped") + it.shortdescription;           
        }
        
        private static Inputs RequestCommand(string input){
            input = input.ToLower();
            input = input.Trim();
            
            string[] inpArr = input.Split(' ');
            
            switch(inpArr[0]){
                case "north": return Inputs.North;
                case "south": return Inputs.South;
                //case "nw": return Inputs.NorthW;
                //case "sw": return Inputs.SouthW;
                //case "ne": return Inputs.NorthE;
                //case "se": return Inputs.SouthE;
                //case "up": return Inputs.Up;
                //case "down": return Inputs.Down;
                case "east": return Inputs.East;
                case "west": return Inputs.West;
                case "exit": return Inputs.Exit;
                case "quit": return Inputs.Exit;
                case "inv": return Inputs.InvToggle;
                case "help": return Inputs.Help;
                case "examine": return Inputs.Examine;
                case "ex": return Inputs.Examine;
                case "take": return Inputs.Take;
                case "drop": return Inputs.Drop;
                //case "time": return Inputs.Time;
                //TO DO - case "wear" / "equip"
                case "": return Inputs.Null;                
                case "y": return Inputs.Yes; 
                case "yes": return Inputs.Yes; 
                case "n": return Inputs.No; 
                case "no": return Inputs.No;
                default: return Inputs.Invalid;
            }
        }
        
        private static void WaitForKey(ConsoleKey cki)
        {
            ConsoleKeyInfo pressed;
            pressed = Console.ReadKey(true);
            while(pressed.Key != cki){
                pressed = Console.ReadKey(true);
            }            
        }
        
        private static void WaitForInput(bool show)
        {
           Console.ReadKey(show);      
        }
        
        private static void DisplayWelcome()
        {
            for(int i = 0; i < G.welcome.Length; i++){
                string s = G.welcome[i];
                if(s == "$title") s = G.GetTitle();
                if(s == "$version") s = "v"+G.GetVersion();
                D.WriteCentral(s);
            }
        }
        public static void DrawGUI()
        {
            int width = Console.WindowWidth;
            D.ResetCursor();
            D.WriteCentral("** " + D.title + " **");
            D.Inc();
            D.FullLine('-');
            D.Inc();
            D.WriteLeftRightSplit("Player: " + P.GetName(), "Score: " + P.GetScore());
            if(D.showinv) {
                D.FullLine('-');
                D.Inc();
                if(P.backpack.Size() == 0){ D.WriteLeft(MainGame.G.String("$packempty")); }
                else{
                    D.WriteLeft(MainGame.G.String("$packcontents"));
                    string[] items = P.backpack.List();
                    for(int i = 0; i < items.Length; i++) D.WriteLeft(" - " + items[i]);
                }
                D.Inc();
                if(P.clothing.Size() > 0){
                    D.WriteLeft(MainGame.G.String("$wearing"));
                    string[] items = P.clothing.List();
                    for(int i = 0; i < items.Length; i++) D.WriteLeft(" - " + items[i]);
                } 
            };
            D.FullLine('-');
            D.Inc(); 
        }
    }
    
public class World
{
    private Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
    private Dictionary<string, Door> doors = new Dictionary<string, Door>();
    private Dictionary<string, Item> items = new Dictionary<string, Item>();

    public World(){ }
    public World(string file){ GenerateWorld(file);}
    
    
    public void GenerateWorld(string file){
        XMLLoad(file);        
    }
    
    public void Add(Object obj)
    {
        if(obj.GetType() == typeof(Cell)) cells.Add(obj.ToString(), (Cell)obj);
        if(obj.GetType() == typeof(Door)) doors.Add(obj.ToString(), (Door)obj);
        if(obj.GetType() == typeof(Item)) items.Add(obj.ToString(), (Item)obj);
    }
    
    public Object Get(Type type, string id)
    {
        if(type == typeof(Cell)) { if (cells.ContainsKey(id)) return cells[id]; else return null; }
        if(type == typeof(Door)) { if (doors.ContainsKey(id)) return doors[id]; else return null; }
        if(type == typeof(Item)) { if (items.ContainsKey(id)) return items[id]; else return null; }
        return null;
    }
    
    
    public Door GetDoorFrom(string f, MainGame.Inputs d){
        foreach (var val in doors.Values)
        {
            Door dr = val;
            if((dr.fromID == f) && (dr.direction == d)) return dr;
        }
        throw new Exception("Door not found");
    }
    
    public string[] GetAllDoorsFrom(string f){
        string toSplit = "";
        foreach (var key in doors.Keys)
        {
            toSplit += key.ToString() + "|";
        }
        
        return toSplit.Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);
    }
    
    public void XMLLoad(string filename)
    {
        XmlReader xml = XmlReader.Create(filename);
        while(xml.Read())
            if((xml.NodeType == XmlNodeType.Element) && (xml.HasAttributes))
            {
                if(xml.Name == "Cell"){
                    string i, n, ld, l;
                    bool lit = true;
                    i = xml.GetAttribute("id");
                    n = xml.GetAttribute("n");
                    ld = xml.GetAttribute("ld");
                    l = xml.GetAttribute("lit");
                    if(l == "no") lit = false;
                    Cell c = new Cell(i, n, ld, lit);
                    c.floor.Add((Item)Get(typeof(Item), "watch"));
                    c.floor.Add((Item)Get(typeof(Item), "keyring"));
                    c.floor.Add((Item)Get(typeof(Item), "watch"));
                    if((i != "") && (ld != "") && (n != "")) Add(c);
                }
                if(xml.Name == "Door"){
                    string i, f, to, k, d, dir;
                    i = xml.GetAttribute("id");
                    f = xml.GetAttribute("from");
                    to = xml.GetAttribute("to");
                    k = xml.GetAttribute("key");
                    dir = xml.GetAttribute("dir");
                    d = xml.GetAttribute("d");
                    MainGame.Inputs inp = MainGame.Inputs.Null;
                    switch(dir){
                        case "N" : inp = MainGame.Inputs.North; break;
                        case "S" : inp = MainGame.Inputs.South; break;
                        case "E" : inp = MainGame.Inputs.East; break; 
                        case "W" : inp = MainGame.Inputs.West; break;
                        default : inp = MainGame.Inputs.Null; break;
                    }
                    if((i != "") && (d != "") && (f != "") && (to != "") && (inp != MainGame.Inputs.Null))
                        Add(new Door(i, f, to, inp, k, d));
                }
                if(xml.Name == "Item"){
                    Item i = new Item(xml.GetAttribute("name"), xml.GetAttribute("type"), xml.GetAttribute("keywords"), xml.GetAttribute("short"), xml.GetAttribute("long"));
                    Add(i);
                }
            }       
    }
    
}
    
public class Cell
{
    public string id, name, description;
    public Inventory floor = new Inventory(99);
    public bool lit;
   
    public Cell(string i, string n, string desc, bool l){
        id = i; name = n; description = desc; lit = l;
    }
    public string Name(){ return name; }
    
    public string Describe(){
        if(!lit) return MainGame.G.String("$roomdark");
        else {
            string d = MainGame.G.String("$roomintro") + description + " ";
            if(floor.Size() > 0){
                string e = "";
                string[] items = floor.List();
                for(int i = 0; i < items.Length; i++){
                    items[i] = items[i] + ", ";
                    if(items.Length == 1){ e += items[i].Remove(items[i].Length-2); break;}
                    if(i == (items.Length-1)) e = e.Remove(e.Length-2)+" and " + items[i].Remove(items[i].Length-2);
                    else e += items[i];
                    
                }
                d += MainGame.G.String("$roomcontentsS") + e + MainGame.G.String("$roomcontentsF") ;
            } 
            return(d);
        }
    }
    public void PutItem(Item it)
    {
        floor.Add(it);
        
    }
    public Item TakeItem(string keyw)
    {
        string id = floor.IDFromKeyword(keyw);
        if(id != "") return floor.Take(id);        
        return null;
    }
    
    public override string ToString() { return id; }
}

public class Door
{
    public string id, fromID, toID, keyID, description;
    public MainGame.Inputs direction;
    public Door(string n, string f, string to, MainGame.Inputs d, string key, string desc){
        id = n; fromID = f; toID = to; direction = d; keyID = key; description = desc;
    }
    
    public override string ToString() { return id; }
}

public class Container
{
    public string id, description;
    public Inventory inv;
    public bool open;
    
    public Container(string i, string d){
        id = i;
        description = d;
        open = false;
    }
    
    public void Add(Item it, bool write)
    {
        try { inv.Add(it); } catch (ArgumentNullException) {} catch (ArgumentException) { if(write) Console.WriteLine("That cannot go there"); }
    }
    
    public override string ToString() { return id; }
    
}

public class Player
{
    private string name;
    private int score;
    private string location;
    public Inventory backpack = new Inventory(10);
    public Inventory clothing = new Inventory(5, "wear");
    
    public Player()
    {
        name = "Player";
        score = 0;
        location = "1";
    }
    
    public void UpdateScore(int points){ score += points;}
    public int GetScore() { return score; }
    public string GetName() { return name; }
    public string Locate() { return location; }
    public void SetName(string n) { name = n; }
    public void MoveTo(string l) { location = l; }
    public string CheckTime(){
        if(clothing.Contains("watch")) return "Your watch shows a time of " + MainGame.G.time; 
        else return "You are not wearing a watch";
    }
    
    public Item DropItem(string keyw)
    {
        string id = backpack.IDFromKeyword(keyw);
        if(id != "") return backpack.Take(id);        
        return null;
    }
                    

}

public class Item
{

    public string type, id, keywords, fulldescription, shortdescription;
    public int amount = 1;
    
    public Item(string n)
    {
        id = n;
        keywords = n;
        shortdescription = n;
        fulldescription = n;
    }
    
    public Item(string n, string t, string kw, string sd, string fd)
    {
        id = n;
        type = t;
        keywords = kw;
        shortdescription = sd;
        fulldescription = fd;
    }
    
    public override string ToString() { return id; }
}

public class Game
{
    private string title;
    private string version;
    public string[] welcome;
    private string start;    
    public int time;
    private Dictionary<string, string> words = new Dictionary<string, string>();
    
    public Game()
    {
        title = "";
        version = "";
        time = 0;
        start = "1";
    }
    
    public void SetTitle(string t) { title = t; }
    public void SetVersion(string v) { version = v; }
    public string GetTitle(){ return title; }
    public string GetVersion(){ return version; }
    public string GetStart(){ return start; }
    
    public void XMLSetup(string filename)
    {
        XmlReader xml = XmlReader.Create(filename);
        while(xml.Read())
            if((xml.NodeType == XmlNodeType.Element) && (xml.HasAttributes)){
                if(xml.Name == "Game"){
                    SetTitle(xml.GetAttribute("title"));
                    welcome = xml.GetAttribute("welcome").Split('|');
                }
                if(xml.Name == "Defaults"){
                    start = xml.GetAttribute("start");
                }
                if(xml.Name == "String"){
                    AddWord(xml.GetAttribute("id"), xml.GetAttribute("s"));
                }                
            }
    }
    private void AddWord(string id, string text)
    {
        words.Add(id, text);
    }
    
    public string String(string id)
    {        
        if(words.ContainsKey(id)) return words[id];
        else return id;
    }
    
}

public class Inventory
{
    private int maxSize;
    private Dictionary<string, Item> contents = new Dictionary<string, Item>();
    private string type;
    
    public Inventory(int size){
        maxSize = size;
        type = "";
    }
    
    public Inventory(int size, string t){
        maxSize = size;
        type = t;
    }    
    
    public void Add(Item it)
    {
        if(it.id=="?") throw new Exception("Invalid item");
        if((type != "")&&(type != it.type)) throw new ArgumentException();
        if(contents.Count == maxSize) throw new Exception("No room for that item there");
        if(!contents.ContainsKey(it.id)) contents.Add(it.id, it);
        else {
            Item ii = contents[it.id];
            ii.amount += it.amount;
            contents.Remove(ii.id);
            contents.Add(ii.id, ii);
        }
    }
    
    public int Size()
    {
        return contents.Count;
    }
    
    public bool Contains(string id)
    {
        return contents.ContainsKey(id);        
    }
    
    public bool ContainsKeyword(string keyw)
    {
        if(ItemFromKeyword(keyw) == null) return false;
        else return true;
    }
    
    public string IDFromKeyword(string keyw)
    {
        Item it = ItemFromKeyword(keyw);
        if(it != null) return it.id;
        else return "";
        
    }
    
    public Item ItemFromKeyword(string keyw)
    {
        foreach (var val in contents.Values)
        {
            Item it = val;
            if(it.keywords.Contains(keyw)) return it;
        }
        return null;        
    }
    
    public string[] List()
    {
        string toSplit = "";
        foreach (var val in contents.Values)
        {
            toSplit += val.shortdescription + "|";
        }
        
        return toSplit.Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);
    }
    
    public void RemoveItem(string id)
    {
        if(contents.ContainsKey(id)) contents.Remove(id);
    }
    
    public Item GetItem(string id)
    {
        if(contents.ContainsKey(id)) return contents[id];
        else return null;
    }
    
    public Item Take(string id)
    {
        if(contents.ContainsKey(id)){
            Item it = contents[id];
            contents.Remove(id);
            return it;
        } else return null;
        
    }
}
   
}
