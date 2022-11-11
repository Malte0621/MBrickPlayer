using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickHill
{
    // contains map informatio, helper functions, and brick list
    public class Map {
        public Color AmbientColor; // self explanatory
        public Color BaseplateColor; // self explanatory
        public Color SkyColor; // self explanatory
        public int BaseplateSize; // size of baseplate in studs (baseplate is square)
        public int SunIntensity; // unused

        public List<Brick> Bricks = new List<Brick>(); // brick list

        public Brick GetBrick (int id) {
            for (int i = 0; i < Bricks.Count; i++) {
                if (Bricks[i].ID == id) return Bricks[i];
            }
            return null;
        }

        // brick object
        public class Brick {
            public int ID; // brick id
            public GameObject _internal_render_object; // In-Game Unity GameObject, when loaded.
            public Vector3 Position = Vector3.one; // position in bh vector3 format
            public Vector3 Scale = Vector3.one; // scale in bh vector3 format
            //public Vector3 Velocity = Vector3.zero; // position in bh vector3 format
            public int Rotation; // rotation along z (unity y) axis
            public int RotationX; // rotation along x (unity x) axis
            public int RotationY; // rotation along y (unity z) axis
            public Color BrickColor = Color.white; // brick color... duh
            public Color BrickLightColor = Color.white; // brick light color... duh
            public ShapeType Shape; // brick shape
            public bool Collision; // if brick has collision
            public string Model; // brick asset id if applicable
            public string ModelTex; // brick asset id if applicable
            public bool Clickable; // if brick is clickable
            public float BrickLightRange; // how far away the light can reach
            public float ClickDistance; // how far away the brick can be clicked from
            public bool Physics; // If the brick has physics or not
            public string Material; // The material of the brick

            public bool ScuffedScale; // not even sure if i use this anymore

            public bool SeparateChunk = false; // if this brick should be in its own chunk
            public int VisibleMeshID = 0; // what brick mesh this brick is in
            public int CollisionMeshID = -1;

            // all supported shapes
            public enum ShapeType {
                cube, slope, wedge, spawnpoint, arch, dome, bars, flag, pole, cylinder, round_slope, vent
            }
        }
    }

    // information primarily about the player's figure (model), with various player information
    public class Figure {
        public Vector3 Position = new Vector3();
        public Vector3 Rotation = new Vector3(); // only 1 vector is used
        public Vector3 Scale = new Vector3();
        public int ToolSlotID;
        public BodyColors Colors = new BodyColors();
        public string FaceID;
        public string Shirt;
        public string Pants;
        public string TShirt;
        public string[] HatID = new string[3];
        public string[] HatIDTex = new string[3];
        public int Score;
        public int TeamNetID;
        public int Speed = 4;
        public int JumpPower = 5;
        public int CameraFOV; // unused?
        public int CameraDistance; // unused?
        public Vector3 CameraPosition = new Vector3(); // unused?
        public Vector3 CameraRotation = new Vector3(); // unused?
        public string CameraType;
        public int CameraNetID; // what?
        public float Health = 100f;
        public float MaxHealth = 100f;
        public string Speech = "";
        public int EquippedToolSlotID;
        public string EquippedToolModel;
        public string EquippedToolModelTex;
        public bool Unequip = false;

        public bool[] _updateValues = new bool[43];

        /// <summary>
        /// Replace values of this Figure with the modified values of the passed Figure.
        /// </summary>
        /// <param name="newData"></param>
        public void Combine (Figure newData) {
            if (newData._updateValues[0]) Position.x = newData.Position.x;
            if (newData._updateValues[1]) Position.y = newData.Position.y;
            if (newData._updateValues[2]) Position.z = newData.Position.z;
            if (newData._updateValues[3]) Rotation.x = newData.Rotation.x;
            if (newData._updateValues[4]) Rotation.y = newData.Rotation.y;
            if (newData._updateValues[5]) Rotation.z = newData.Rotation.z;
            if (newData._updateValues[6]) Scale.x = newData.Scale.x;
            if (newData._updateValues[7]) Scale.y = newData.Scale.y;
            if (newData._updateValues[8]) Scale.z = newData.Scale.z;
            if (newData._updateValues[9]) ToolSlotID = newData.ToolSlotID;
            if (newData._updateValues[10]) Colors.Head = newData.Colors.Head;
            if (newData._updateValues[11]) Colors.Torso = newData.Colors.Torso;
            if (newData._updateValues[12]) Colors.LeftArm = newData.Colors.LeftArm;
            if (newData._updateValues[13]) Colors.RightArm = newData.Colors.RightArm;
            if (newData._updateValues[14]) Colors.LeftLeg = newData.Colors.LeftLeg;
            if (newData._updateValues[15]) Colors.RightLeg = newData.Colors.RightLeg;
            if (newData._updateValues[16]) FaceID = newData.FaceID;
            if (newData._updateValues[17]) Shirt = newData.Shirt;
            if (newData._updateValues[18]) Pants = newData.Pants;
            if (newData._updateValues[19]) TShirt = newData.TShirt;
            if (newData._updateValues[20]) HatID[0] = newData.HatID[0];
            if (newData._updateValues[21]) HatID[1] = newData.HatID[1];
            if (newData._updateValues[22]) HatID[2] = newData.HatID[2];
            if (newData._updateValues[20]) HatIDTex[0] = newData.HatIDTex[0];
            if (newData._updateValues[21]) HatIDTex[1] = newData.HatIDTex[1];
            if (newData._updateValues[22]) HatIDTex[2] = newData.HatIDTex[2];
            if (newData._updateValues[23]) Score = newData.Score;
            if (newData._updateValues[24]) TeamNetID = newData.TeamNetID;
            if (newData._updateValues[25]) Speed = newData.Speed;
            if (newData._updateValues[26]) JumpPower = newData.JumpPower;
            if (newData._updateValues[27]) CameraFOV = newData.CameraFOV;
            if (newData._updateValues[28]) CameraDistance = newData.CameraDistance;
            if (newData._updateValues[29]) CameraPosition.x = newData.CameraPosition.x;
            if (newData._updateValues[30]) CameraPosition.y = newData.CameraPosition.y;
            if (newData._updateValues[31]) CameraPosition.z = newData.CameraPosition.z;
            if (newData._updateValues[32]) CameraRotation.x = newData.CameraRotation.x;
            if (newData._updateValues[33]) CameraRotation.y = newData.CameraRotation.y;
            if (newData._updateValues[34]) CameraRotation.z = newData.CameraRotation.z;
            if (newData._updateValues[35]) CameraType = newData.CameraType;
            if (newData._updateValues[36]) CameraNetID = newData.CameraNetID;
            if (newData._updateValues[37]) Health = newData.Health;
            if (newData._updateValues[38]) MaxHealth = newData.MaxHealth;
            if (newData._updateValues[39]) Speech = newData.Speech;
            if (newData._updateValues[40]) EquippedToolSlotID = newData.EquippedToolSlotID;
            if (newData._updateValues[41]) EquippedToolModel = newData.EquippedToolModel;
            if (newData._updateValues[41]) EquippedToolModelTex = newData.EquippedToolModelTex;
            if (newData._updateValues[42]) Unequip = newData.Unequip;

            _updateValues = (bool[]) newData._updateValues.Clone(); // if we dont copy the array, _updateValues will be set to a REFERENCE of newdata updatevalues and thats big nono
        }

        public struct BodyColors {
            public Color Head;
            public Color Torso;
            public Color LeftArm;
            public Color RightArm;
            public Color LeftLeg;
            public Color RightLeg;
        }
    }

    // player object themself
    public class Player {
        public int NetID; // connection id
        public string Name; // player name
        public int ID; // player id (not netid)
        public int Membership; // 0: none, 1: mint (retired), 2: ace, 3: royal
        public bool Admin; // whether user is a site admin

        public Figure PlayerFigure = new Figure(); // the player's figure
    }

    // information sent to the client first thing when it connects - contains the local player and map size
    public class LocalInformation {
        public Player LocalPlayer; // self explanatory
        public int MapSize; // amount of bricks in initial map
    }

    // teams
    public class Team {
        public int NetID;
        public string Name;
        public Color TeamColor;
    }

    // BOT
    public class Bot {
        public int ID;
        public string Name;
        public Vector3 Position = new Vector3();
        public Vector3 Rotation = new Vector3();
        public Vector3 Scale = new Vector3();
        public Figure.BodyColors Colors = new Figure.BodyColors();
        public string Face;
        public string[] HatID = new string[3];
        public string[] HatIDTex = new string[3];
        public string Speech;
    }

    // Tool
    public class Tool {
        public int Action;
        public int SlotID;
        public string Name;
        public string Model;
        public string ModelTex;
    }
}