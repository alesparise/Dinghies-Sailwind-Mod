using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dinghies
{   /// <summary>
    /// Controls the nameplates objects and handles writing into it 
    /// </summary>
    public class Nameplate : GoPointerButton
    {
        private string plateName = "CUTTER";    //THIS MUST BE ALL CAPS. I'M MAD!!! GRRRR!
        private const string key = DinghiesMain.shortName + ".plateName";

        private int letterCount;
        
        private float unpausedTimescale;
        private float textAnchorX;
        private float textAnchorY;
        private float textAnchorZ;
        private float currentOffset;
        private readonly float[] spacing = { 
            0.045f,  //A
            0.065f,  //B
            0.06f,   //C
            0.07f,   //D
            0.06f,   //E

            0.06f,   //F
            0.06f,   //G
            0.07f,   //H
            0.035f,  //I
            0.045f,  //J

            0.065f,  //K
            0.055f,  //L
            0.095f,  //M
            0.075f,  //N
            0.065f,  //O

            0.055f,  //P
            0.065f,  //Q
            0.06f,   //R
            0.05f,   //S
            0.06f,   //T

            0.07f,   //U
            0.065f,  //V
            0.09f,   //W
            0.06f,   //X
            0.06f,   //Y

            0.05f,   //Z
            0.06f,  //' ' (space)
        };

        private bool typing;
        private bool initialised;

        private Nameplate otherPlate;

        private Transform boat;
        private Transform textAnchor;

        private Vector3 direction = new Vector3(-1f, 0f, 0f);

        private NameSaver nameSaver;
        private static List<NameSaver> list;

        //UNITY METHODS
        private void Awake()
        {   //initialises the nameplate
            Transform parent = transform.parent;
            boat = parent.parent;
            if (name == "nameplate_left")
            {   //the other plate is the right one
                otherPlate = parent.Find("nameplate_right").GetComponent<Nameplate>();
            }
            else
            {   //the other plate is the left one
                otherPlate = parent.Find("nameplate_left").GetComponent<Nameplate>();
            }

            //setup letters stuff
            textAnchor = transform.GetChild(0);
            textAnchorX = textAnchor.localPosition.x;
            textAnchorY = textAnchor.localPosition.y;
            textAnchorZ = textAnchor.localPosition.z;

            WriteLoadedName(plateName);

            if (list == null) list = new List<NameSaver>();
        }
        public override void OnActivate()
        {   //gets called when clicking on the nameplate
            if (!typing)
            {
                unpausedTimescale = Time.timeScale;
                Time.timeScale = 0f;
                typing = true;
                MouseLook.ToggleMouseLook(false);
                Refs.SetPlayerControl(false);
            }
        }
        public override void ExtraLateUpdate()
        {   //detect typing letters
            if (!initialised && GameState.playing)
            {   //initialise the plate
                LoadName();
                initialised = true;
            }
            if (!typing) return;
            if (Input.GetKeyDown(KeyCode.A))
            {
                AddLetter('A', 0);
                otherPlate.AddLetter('A', 0);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                AddLetter('B', 1);
                otherPlate.AddLetter('B', 1);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                AddLetter('C', 2);
                otherPlate.AddLetter('C', 2);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                AddLetter('D', 3);
                otherPlate.AddLetter('D', 3);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                AddLetter('E', 4);
                otherPlate.AddLetter('E', 4);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                AddLetter('F', 5);
                otherPlate.AddLetter('F', 5);
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                AddLetter('G', 6);
                otherPlate.AddLetter('G', 6);
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                AddLetter('H', 7);
                otherPlate.AddLetter('H', 7);
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                AddLetter('I', 8);
                otherPlate.AddLetter('I', 8);
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                AddLetter('J', 9);
                otherPlate.AddLetter('J', 9);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                AddLetter('K', 10);
                otherPlate.AddLetter('K', 10);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                AddLetter('L', 11);
                otherPlate.AddLetter('L', 11);
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                AddLetter('M', 12);
                otherPlate.AddLetter('M', 12);
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                AddLetter('N', 13);
                otherPlate.AddLetter('N', 13);
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                AddLetter('O', 14);
                otherPlate.AddLetter('O', 14);
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                AddLetter('P', 15);
                otherPlate.AddLetter('P', 15);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                AddLetter('Q', 16);
                otherPlate.AddLetter('Q', 16);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {   
                AddLetter('R', 17);
                otherPlate.AddLetter('R', 17);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                AddLetter('S', 18);
                otherPlate.AddLetter('S', 18);
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                AddLetter('T', 19);
                otherPlate.AddLetter('T', 19);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                AddLetter('U', 20);
                otherPlate.AddLetter('U', 20);
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                AddLetter('V', 21);
                otherPlate.AddLetter('V', 21);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                AddLetter('W', 22);
                otherPlate.AddLetter('W', 22);
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                AddLetter('X', 23);
                otherPlate.AddLetter('X', 23);
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                AddLetter('Y', 24);
                otherPlate.AddLetter('Y', 24);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                AddLetter('Z', 25);
                otherPlate.AddLetter('Z', 25);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                AddLetter(' ', 26);
                otherPlate.AddLetter(' ', 26);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                RemoveLetter();
                otherPlate.RemoveLetter();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {   //close the nameplate input by pressing enter
                Time.timeScale = unpausedTimescale;
                SaveName();
                otherPlate.SaveName();
                typing = false;
                Refs.SetPlayerControl(true);
                MouseLook.ToggleMouseLook(true);
            }
        }
        
        //NAMEPLATE METHODS
        private void AddLetter(char letter, int index) 
        {   //adds a letter to the name and instantiate the correct letter object in the correct position
            if (currentOffset > 1.25f) return; //don't add any more letter if the name is too long
            plateName += letter;
            if (index < 26)
            {
                GameObject go = Instantiate(DinghiesPatches.letters[index], textAnchor);
                Transform t = go.transform;
                currentOffset += spacing[index];
                t.localPosition += direction * currentOffset;
            }
            else
            {
                Instantiate(new GameObject(), textAnchor);  //create an empty go so that the child number of textAnchor is coherent with letterCount
                currentOffset += spacing[index];
            }

            if (currentOffset > 0.95f)
            {
                ScaleAnchor(0.4f);
            }
            else if (currentOffset > 0.72f)
            {
                ScaleAnchor(0.6f);
            }
            else if (currentOffset > 0.55f)
            {
                ScaleAnchor(0.8f);
            }
            
            letterCount++;
        }
        private void RemoveLetter()
        {   //removes the last letter in the name, destroys the object
            if (letterCount == 0) return;
            letterCount--;
            char l = plateName[letterCount];
            plateName = plateName.Remove(letterCount);
            currentOffset -= spacing[IndexFromLetter(l)];
            Destroy(textAnchor.GetChild(letterCount).gameObject);
            //scale the name up if it goes back to be shorter
            if (currentOffset < 0.55f)
            {
                ScaleAnchor(1f);
            }
            else if (currentOffset < 0.72f)
            {
                ScaleAnchor(0.8f);
            }
            else if (currentOffset < 0.95f)
            {
                ScaleAnchor(0.6f);
            }
        }
        private void ScaleAnchor(float scale)
        {   //scale the text anchor up or down depending on how long the name is
            textAnchor.localScale = new Vector3(-scale, scale, scale);
            textAnchor.localPosition = new Vector3(textAnchorX, textAnchorY * scale, textAnchorZ);
        }
        private int IndexFromLetter(char l)
        {   //returns the index value for the given character
            if (l == 'A')
            {
                return 0;
            }
            else if (l == 'B')
            {
                return 1;
            }
            else if (l == 'C')
            {
                return 2;
            }
            else if (l == 'D')
            {
                return 3;
            }
            else if (l == 'E')
            {
                return 4;
            }
            else if (l == 'F')
            {
                return 5;
            }
            else if (l == 'G')
            {
                return 6;
            }
            else if (l == 'H')
            {
                return 7;
            }
            else if (l == 'I')
            {
                return 8;
            }
            else if (l == 'J')
            {
                return 9;
            }
            else if (l == 'K')
            {
                return 10;
            }
            else if (l == 'L')
            {
                return 11;
            }
            else if (l == 'M')
            {
                return 12;
            }
            else if (l == 'N')
            {
                return 13;
            }
            else if (l == 'O')
            {
                return 14;
            }
            else if (l == 'P')
            {
                return 15;
            }
            else if (l == 'Q')
            {
                return 16;
            }
            else if (l == 'R')
            {
                return 17;
            }
            else if (l == 'S')
            {
                return 18;
            }
            else if (l == 'T')
            {
                return 19;
            }
            else if (l == 'U')
            {
                return 20;
            }
            else if (l == 'V')
            {
                return 21;
            }
            else if (l == 'W')
            {
                return 22;
            }
            else if (l == 'X')
            {
                return 23;
            }
            else if (l == 'Y')
            {
                return 24;
            }
            else if (l == 'Z')
            {
                return 25;
            }
            return 26;  //space ' '
        }
        private void WriteLoadedName(string str)
        {   //writes the loaded name to the boat
            foreach (char c in str)
            {
                AddLetter(c, IndexFromLetter(c));
            }
        }
        private void SaveName()
        {   //save the name to modData so we can load it upon awakening

            nameSaver = new NameSaver
            {
                boatName = boat.name,
                plateName = plateName,
            };
            if (!list.Contains(nameSaver))
            {
                list.Add(nameSaver);
            }

            GameState.modData[key] = NameSaver.Serialize(list);
            //GameState.modData[key] = boat.name + ":" + plateName + ";";
        }
        private void LoadName()
        {   //loads the correct name for the current nameplate
            if (GameState.modData.ContainsKey(key))
            {
                List<NameSaver> nameSavers = NameSaver.Unserialize(GameState.modData[key]);
                foreach (NameSaver ns in nameSavers)
                {
                    if (ns.boatName == boat.name)
                    {
                        CleanName();
                        WriteLoadedName(ns.plateName);
                        break;
                    }
                }
            }
        }
        private void CleanName()
        {   //removes the name entirely
            int l = plateName.Length;
            for (int i = 0; i < l; i++)
            {
                RemoveLetter();
            }
        }
        internal class NameSaver
        {
            public string boatName;
            public string plateName;

            public static string Serialize(List<NameSaver> nameSavers)
            {
                StringBuilder sb = new StringBuilder();
                foreach (NameSaver ns in nameSavers)
                {
                    sb.Append(ns.boatName); //cutterModel
                    sb.Append(":");         //cutterModel:
                    sb.Append(ns.plateName);//cutterModel:Jonny
                    sb.Append(";");         //cutterModel:Jonny;
                }

                return sb.ToString(); //cutterModel:Jonny;outriggerModel:Tommy;
            }
            public static List<NameSaver> Unserialize(string s)
            {
                List<NameSaver> nameSavers = new List<NameSaver>();
                string[] pairs = s.Split(';');
                foreach (string pair in pairs)
                {
                    if (pair == "") continue;

                    string[] split = pair.Split(':');
                    NameSaver ns = new NameSaver
                    {
                        boatName = split[0],
                        plateName = split[1]
                    };
                    nameSavers.Add(ns);
                }

                return nameSavers;
            }
        }
    }
}
