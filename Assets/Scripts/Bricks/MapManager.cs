using BrickHill;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Utils;

public class MapManager : MonoBehaviour
{
    public MapBuilder builder;

    public Map LoadedMap;
    public bool mapIsLoaded = false;

    public PostProcessProfile postProcessProfile;

    Dictionary<Map.Brick, GameObject> BrickGOs = new Dictionary<Map.Brick, GameObject>();
    //public List<Map.Brick> BrickQueue = new List<Map.Brick>();

    public Shader BrickShader;
    public GameObject[] ShapePrefabs;
    public Mesh BaseplateMesh;
    public Camera MainCam;

    public bool useSkybox;
    public Skybox skyboxComponent;
    public Material skybox;

    public float NoCollisionThreshold = 0.05f;

    private GameObject baseplate;
    private Material baseplateMaterial;

    private void Start()
    {
        postProcessProfile.GetSetting<ColorGrading>().enabled.value = true;
    }

    public void LoadBricks(PacketReader mapData)
    {
        checkMapLoaded();
        int num = (int)mapData.ReadUInt();
        List<Map.Brick> list = new List<Map.Brick>();
        for (int i = 0; i < num; i++)
        {
            Map.Brick brick = new Map.Brick();
            brick.ID = (int)mapData.ReadUInt();
            brick.Position = new Vector3(mapData.ReadFloat(), mapData.ReadFloat(), mapData.ReadFloat());
            brick.Scale = new Vector3(mapData.ReadFloat(), mapData.ReadFloat(), mapData.ReadFloat());
            brick.BrickColor = Helper.DecToColor((int)mapData.ReadUInt(), true);
            brick.BrickColor.a = mapData.ReadFloat();
            brick.Collision = true;
            string text = mapData.ReadString();
            for (int j = 0; j < text.Length; j++)
            {
                switch (text[j])
                {
                    case 'A':
                        int testRot = mapData.ReadInt();
                        if (testRot == 1621)
                        {
                            brick.RotationX = mapData.ReadInt();
                            brick.RotationY = mapData.ReadInt();
                            brick.Rotation = mapData.ReadInt();

                            brick.RotationX = brick.RotationX.Mod(360);
                            brick.RotationY = brick.RotationY.Mod(360);

                            if (brick.RotationX != 0 && brick.RotationX != 180)
                            {
                                brick.ScuffedScale = true;
                            }
                            else if (brick.RotationY != 0 && brick.RotationY != 180)
                            {
                                brick.ScuffedScale = true;
                            }
                        }
                        else
                        {
                            brick.Rotation = testRot;
                        }

                        brick.Rotation = brick.Rotation.Mod(360);

                        if (!brick.ScuffedScale && brick.Rotation != 0 && brick.Rotation != 180)
                        {
                            brick.ScuffedScale = true;
                        }
                        break;
                    case 'B':
                        brick.Shape = Helper.GetShapeFromName(mapData.ReadString());
                        break;
                    case 'C':
                        brick.Model = mapData.ReadString();
                        brick.ModelTex = mapData.ReadString();
                        break;
                    case 'D':
                        GameObject lobj = GameObject.Find("Light" + brick.ID.ToString());
                        if (lobj)
                        {
                            Destroy(lobj);
                        }
                        uint lightColorRaw = mapData.ReadUInt();
                        uint lightRange = mapData.ReadUInt();
                        if (lightRange > 0)
                        {
                            GameObject lightGameObject = new GameObject("Light" + brick.ID.ToString());
                            Light lightComp = lightGameObject.AddComponent<Light>();
                            lightComp.color = Helper.DecToColor((int)lightColorRaw);
                            lightComp.range = lightRange;

                            Vector3 adjustedPosition = new Vector3(brick.Position.x * -1, brick.Position.z, brick.Position.y);
                            Vector3 adjustedScale = new Vector3(brick.Scale.x, brick.Scale.z, brick.Scale.y);
                            if (brick.ScuffedScale)
                            {
                                adjustedScale = new Vector3(adjustedScale.z, adjustedScale.y, adjustedScale.x);
                                float adjustment = (adjustedScale.x - adjustedScale.z) / 2;
                                adjustedPosition.x += adjustment;
                                adjustedPosition.z += adjustment;
                            }
                            Vector3 center = adjustedPosition + adjustedScale / 2f;

                            lightGameObject.transform.position = center;
                        }
                        break;
                    case 'G':
                        brick.Clickable = mapData.ReadBool();
                        brick.ClickDistance = (int)mapData.ReadUInt();
                        break;
                    case 'F':
                        brick.Collision = false;
                        break;
                    case 'M':
                        string material = mapData.ReadString();
                        brick.Material = material;
                        break;
                }
            }
            PlayerMain.instance.CreatedBricks.Add(brick.ID);
            LoadedMap.Bricks.Add(brick);
            list.Add(brick);
        }
        builder.AddBricks(list);
    }

    public void LoadBricksOld (string mapData) {
        checkMapLoaded();

        List<Map.Brick> newBricks = new List<Map.Brick>();
        string[] lines = mapData.Split(new string[] {"\r\n", "\r", "\n"}, System.StringSplitOptions.RemoveEmptyEntries); // split map data into lines
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            try {
                if (line[0] != '+') {
                    if (line[0] == '>') {
                        if (line.StartsWith(">TEAM")) {
                            // team
                            Debug.Log("TEAM");
                        } else if (line.StartsWith(">TOOL")) {
                            // tool
                            Debug.Log("TOOL");
                        }
                    } else {
                        // probably defining a brick
                        Map.Brick brick = new Map.Brick();
                        float[] brickInfo = stringToFloatArray(line); // brick initial info
                        //Debug.Log(brickInfo);
                        brick.Position = new Vector3(brickInfo[0], brickInfo[1], brickInfo[2]);
                        brick.Scale = new Vector3(brickInfo[3], brickInfo[4], brickInfo[5]);
                        brick.BrickColor = new Color(brickInfo[6], brickInfo[7], brickInfo[8], brickInfo[9]);
                        brick.Collision = true;
                        LoadedMap.Bricks.Add(brick);
                        newBricks.Add(brick);
                        //BrickQueue.Add(brick);
                    }
                } else {
                    Map.Brick lastBrick = LoadedMap.Bricks[LoadedMap.Bricks.Count-1]; // get last brick
                    if (line.StartsWith("+NAME")) {
                        if (line.Length == 5) {
                            //lastBrick.ID = "";
                            lastBrick.ID = -1;
                        } else {
                            //lastBrick.name = line.Substring(6);
                            lastBrick.ID = Int32.Parse(line.Substring(6), CultureInfo.InvariantCulture);
                        }
                        PlayerMain.instance.CreatedBricks.Add(lastBrick.ID);
                    } else if (line.StartsWith("+ROTX")) {
                        lastBrick.RotationX = int.Parse(line.Substring(6), CultureInfo.InvariantCulture);
                        lastBrick.RotationX = lastBrick.RotationX.Mod(360);
                    } else if (line.StartsWith("+ROTY")) {
                        lastBrick.RotationY = int.Parse(line.Substring(6), CultureInfo.InvariantCulture);
                        lastBrick.RotationY = lastBrick.RotationY.Mod(360);
                    } else if (line.StartsWith("+ROT")) {
                        lastBrick.Rotation = int.Parse(line.Substring(5), CultureInfo.InvariantCulture);
                        lastBrick.Rotation = lastBrick.Rotation.Mod(360);
                        if (lastBrick.Rotation != 0 && lastBrick.Rotation != 180) {
                            lastBrick.ScuffedScale = true;
                        }
                    } else if (line.StartsWith("+SHAPE")) {
                        lastBrick.Shape = Helper.GetShapeFromName(line.Substring(7));
                    } else if (line.StartsWith("+NOCOLLISION")) {
                        lastBrick.Collision = false;
                    } else if (line.StartsWith("+MODEL")) {
                        //lastBrick.Model = int.Parse(line.Substring(7), CultureInfo.InvariantCulture);
                    } else if (line.StartsWith("+CLICKABLE")) {
                        lastBrick.Clickable = true;
                        if (line.Length > 11) {
                            string dist = line.Substring(11);
                            lastBrick.ClickDistance = int.Parse(dist, CultureInfo.InvariantCulture);
                        }
                    }
                }
            } catch (FormatException e) {
                continue;
            }
        }
        // build new bricks
        builder.AddBricks(newBricks);
    }

    public void BuildEntireMap () {
        if (!mapIsLoaded) return;

        if (baseplate == null)
            BuildBaseplate();

        builder.RebuildEntireMap();
    }

    public void BuildBaseplate () {
        if (baseplate != null) return;

        baseplate = new GameObject("Baseplate");
        baseplate.transform.position = LoadedMap.BaseplateSize % 2 == 0 ? Vector3.zero : new Vector3(0.5f, 0f, -0.5f);
        baseplate.transform.localScale = new Vector3(LoadedMap.BaseplateSize, 1f,LoadedMap.BaseplateSize);
        baseplate.AddComponent<MeshFilter>().mesh = BaseplateMesh;
        //baseplate.AddComponent<MeshRenderer>().material = MaterialCache.instance.GetMaterial((LoadedMap.BaseplateColor, MaterialCache.FaceType.Stud, new Vector2(LoadedMap.BaseplateSize, LoadedMap.BaseplateSize), BrickShader));

        baseplateMaterial = new Material(builder.StudMat);
        baseplateMaterial.mainTextureScale = new Vector2(LoadedMap.BaseplateSize / builder.StudTile, LoadedMap.BaseplateSize / builder.StudTile);
        baseplateMaterial.SetTextureScale("_NormalTex", new Vector2(LoadedMap.BaseplateSize / builder.StudTile, LoadedMap.BaseplateSize / builder.StudTile));
        baseplateMaterial.color = LoadedMap.BaseplateColor;
        baseplate.AddComponent<MeshRenderer>().material = baseplateMaterial;

        BoxCollider bc = baseplate.AddComponent<BoxCollider>();
        bc.size = Vector3.one;
        bc.center = new Vector3(0,-0.5f,0);
    }

    /*
    public void BuildBrick (Map.Brick brick) {
        if (BrickGOs.ContainsKey(brick)) {
            Debug.Log("Tried to build an already existing brick!");
            DestroyBrick(brick);
        }
        GameObject b = Instantiate(ShapePrefabs[(int)brick.Shape]);
        b.name = brick.ID.ToString();
        if (brick.Clickable) b.tag = "Clickable";

        Vector3 pos = brick.Position.SwapYZ() + brick.Scale.SwapYZ() / 2f; // swap y and z and offset position to brick corner
        pos.x *= -1; // flip x
        b.transform.position = pos;

        brick.Rotation = brick.Rotation % 360;
        b.transform.eulerAngles = new Vector3(0, brick.Rotation * -1, 0);

        if (!brick.Collision || brick.Scale.x <= NoCollisionThreshold || brick.Scale.y <= NoCollisionThreshold || brick.Scale.z <= NoCollisionThreshold) {
            b.GetComponent<BoxCollider>().enabled = false;
            //b.layer = 10; // no collide
        }

        BrickShape bs = b.GetComponent<BrickShape>();

        for (int i = 0; i < bs.elements.Length; i++) {
            MeshFilter mf = bs.elements[i].GetComponent<MeshFilter>();
            if (mf == null) continue;
            int smCount = mf.mesh.subMeshCount; // amount of materials model uses

            MeshRenderer mr = bs.elements[i].GetComponent<MeshRenderer>();
            Material[] brickMats = new Material[smCount]; // prepare material array
            brickMats[0] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Smooth, Vector2.one, BrickShader));
            if (smCount > 1) { // studs
                if (brick.Shape == Map.Brick.ShapeType.spawnpoint) {
                    brickMats[1] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Spawnpoint, Vector2.one, BrickShader));
                } else {
                    brickMats[1] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Stud, Helper.CorrectBHScale(brick.Scale, brick.Rotation), BrickShader));
                }
            }
            if (smCount > 2) { // inlets
                brickMats[2] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Inlet, Helper.CorrectBHScale(brick.Scale, brick.Rotation), BrickShader));
            }
            mr.materials = brickMats;
        }
        
        bs.brick = brick;
        bs.UpdateShape();

        // set model ...

        BrickGOs.Add(brick, b);
    }

    public void DestroyBrick (Map.Brick brick) {
        if (BrickGOs.TryGetValue(brick, out GameObject b)) {
            Destroy(b);
            BrickGOs.Remove(brick);
            PlayerMain.instance.DeletedBricks.Add(brick.ID);
        }
    }

    public void UpdateBrick (Map.Brick brick) {
        if (BrickGOs.TryGetValue(brick, out GameObject brickGO)) {
            if (brick.Clickable) {
                brickGO.tag = "Clickable";
            } else {
                brickGO.tag = "Untagged"; // untag
            }

            Vector3 pos = brick.Position.SwapYZ() + brick.Scale.SwapYZ() / 2f; // swap y and z and offset position to brick corner
            pos.x *= -1; // flip x
            brickGO.transform.position = pos;

            brick.Rotation = brick.Rotation % 360;
            brickGO.transform.eulerAngles = new Vector3(0, brick.Rotation * -1, 0);

            if (!brick.Collision || brick.Scale.x <= NoCollisionThreshold || brick.Scale.y <= NoCollisionThreshold || brick.Scale.z <= NoCollisionThreshold) {
                brickGO.GetComponent<BoxCollider>().enabled = false;
                //brickGO.layer = 10; // no collide
            } else {
                //brickGO.layer = 0; // default
            }

            BrickShape bs = brickGO.GetComponent<BrickShape>();

            for (int i = 0; i < bs.elements.Length; i++) {
                MeshFilter mf = bs.elements[i].GetComponent<MeshFilter>();
                if (mf == null) continue;
                int smCount = mf.mesh.subMeshCount; // amount of materials model uses

                MeshRenderer mr = bs.elements[i].GetComponent<MeshRenderer>();
                Material[] brickMats = new Material[smCount]; // prepare material array
                brickMats[0] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Smooth, Vector2.one, BrickShader));
                if (smCount > 1) { // studs
                    if (brick.Shape == Map.Brick.ShapeType.spawnpoint) {
                        brickMats[1] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Spawnpoint, Vector2.one, BrickShader));
                    } else {
                        brickMats[1] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Stud, Helper.CorrectBHScale(brick.Scale, brick.Rotation), BrickShader));
                    }
                }
                if (smCount > 2) { // inlets
                    brickMats[2] = MaterialCache.instance.GetMaterial((brick.BrickColor, MaterialCache.FaceType.Inlet, Helper.CorrectBHScale(brick.Scale, brick.Rotation), BrickShader));
                }
                mr.materials = brickMats;
            }

            bs.UpdateShape();

            // take care of model
        } else {
            Debug.Log("tried to update nonexistent brick!");
        }
    }
    */

    public void DestroyBrick (Map.Brick brick) {
        for (int i = 0; i < LoadedMap.Bricks.Count; i++) {
            if (LoadedMap.Bricks[i].ID == brick.ID) {
                // get mesh ids
                int meshIndex = LoadedMap.Bricks[i].VisibleMeshID;
                int collisionIndex = LoadedMap.Bricks[i].CollisionMeshID;

                // delete brick
                LoadedMap.Bricks.RemoveAt(i);
                builder.GetChunk(meshIndex)?.RemoveBrick(brick);
                builder.GetChunk(collisionIndex)?.RemoveBrick(brick);

                // regenerate meshes
                builder.UpdateChunk(meshIndex);
                builder.UpdateChunk(collisionIndex);
                break;
            }
        }
    }
    
    public void DestroyBrickContainer(Map.Brick brick) {
        for (int i = 0; i < LoadedMap.Bricks.Count; i++) {
            if (LoadedMap.Bricks[i].ID == brick.ID) {
                // get mesh ids
                int meshIndex = LoadedMap.Bricks[i].VisibleMeshID;
                int collisionIndex = LoadedMap.Bricks[i].CollisionMeshID;

                // delete brick
                LoadedMap.Bricks.RemoveAt(i);
                builder.GetChunk(meshIndex)?.RemoveBrick(brick);
                builder.GetChunk(collisionIndex)?.RemoveBrick(brick);

                // regenerate meshes
                builder.RemoveChunk(meshIndex);
                break;
            }
        }
    }

    public void UpdateBrick (Map.Brick brick) {
        builder.UpdateChunk(brick.VisibleMeshID);
        builder.UpdateChunk(brick.CollisionMeshID);
    }

    public void SetMapAmbient(Color ambient)
    {
        ColorGrading setting = postProcessProfile.GetSetting<ColorGrading>();
        setting.mixerRedOutRedIn.value = ambient.r * 200f + 100f;
        setting.mixerRedOutGreenIn.value = ambient.r * 200f;
        setting.mixerRedOutBlueIn.value = ambient.r * 200f;
        setting.mixerGreenOutRedIn.value = ambient.g * 200f;
        setting.mixerGreenOutGreenIn.value = ambient.g * 200f + 100f;
        setting.mixerGreenOutBlueIn.value = ambient.g * 200f;
        setting.mixerBlueOutRedIn.value = ambient.b * 200f;
        setting.mixerBlueOutGreenIn.value = ambient.b * 200f;
        setting.mixerBlueOutBlueIn.value = ambient.b * 200f + 100f;
        setting.lift.value = new Vector4(ambient.r, ambient.g, ambient.b, 0f);
    }

    public void SetEnvironmentProperty (string property, object value) { // TODO: weather
        checkMapLoaded();
        switch (property) {
            case "Ambient":
                // ambient color
                LoadedMap.AmbientColor = (Color)value;
                LoadedMap.AmbientColor.a = 1f;
                break;
            case "BaseCol":
                // baseplate color
                LoadedMap.BaseplateColor = (Color)value;
                LoadedMap.BaseplateColor.a = 1f;
                break;
            case "Sky":
                // sky color
                LoadedMap.SkyColor = (Color)value;
                LoadedMap.SkyColor.a = 1f;
                break;
            case "BaseSize":
                // baseplate size
                LoadedMap.BaseplateSize = (int)value;
                break;
            case "Sun":
                // sun intensity
                LoadedMap.SunIntensity = (int)value;
                break;
            case "WeatherSun":
                // clear weather
                break;
        }

        SetMapAmbient(LoadedMap.AmbientColor);
        GameObject.Find("Directional Light").GetComponent<Light>().intensity = (LoadedMap.SunIntensity / (Math.Max(25, LoadedMap.BaseplateSize)))/10;
        MainCam.backgroundColor = LoadedMap.SkyColor;

        if (baseplate != null) {
            baseplate.transform.position = LoadedMap.BaseplateSize % 2 == 0 ? Vector3.zero : new Vector3(0.5f, 0f, -0.5f);
            baseplate.transform.localScale = new Vector3(LoadedMap.BaseplateSize, 1.0f, LoadedMap.BaseplateSize);

            //baseplate.GetComponent<MeshRenderer>().material = MaterialCache.instance.GetMaterial((LoadedMap.BaseplateColor, MaterialCache.FaceType.Stud, new Vector2(LoadedMap.BaseplateSize, LoadedMap.BaseplateSize), BrickShader));
            baseplateMaterial.mainTextureScale = new Vector2(LoadedMap.BaseplateSize / builder.StudTile, LoadedMap.BaseplateSize / builder.StudTile);
            baseplateMaterial.SetTextureScale("_NormalTex", new Vector2(LoadedMap.BaseplateSize / builder.StudTile, LoadedMap.BaseplateSize / builder.StudTile));
            baseplateMaterial.color = LoadedMap.BaseplateColor;

            BoxCollider bc = baseplate.GetComponent<BoxCollider>();
            bc.size = Vector3.one;
            bc.center = new Vector3(0,-0.5f,0);
        } else {
            BuildBaseplate();
        }
    }

    public void SetSkybox (Texture skytex) {
        skybox.mainTexture = skytex;
        skyboxComponent.material = skybox;
        MainCam.clearFlags = CameraClearFlags.Skybox;
        useSkybox = true;
    }

    void checkMapLoaded () {
        if (!mapIsLoaded) {
            LoadedMap = new Map();
            mapIsLoaded = true;
        }
    }

    // helper
    float[] stringToFloatArray (string input) {
        string[] words = input.Split(' ');
        float[] output = new float[words.Length];
        for (int i = 0; i < words.Length; i++) {
            if (float.TryParse(words[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) {
                output[i] = f;
            }
        }
        return output;
    }
}


