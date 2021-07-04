using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using Dummiesman;
using System;

namespace UnitySUNCG
{

    public class HouseLoader : MonoBehaviour
    {
        public House house;
        public static bool flip = false;

        /// load meshes in a house, left hand coordinate system by default
        public void Load(bool flip2 = false)
        {
            flip = flip2;
            foreach (Transform levelTransform in transform)
                levelTransform.GetComponent<LevelLoader>().Load();
            // convert Unity coordinate system to right hand coordinate system
            if (flip2)
                transform.localScale = new Vector3(1, 1, -1);
        }
    }


    public class LevelLoader : MonoBehaviour
    {
        public Level level;

        /// load meshes in a level
        public void Load()
        {
            foreach (Transform nodeTransform in transform)
            {
                NodeLoader nodeLoader = nodeTransform.GetComponent<NodeLoader>();
                // Nodes in a room will be loaded in the room
                if (nodeLoader.node.roomId != null)
                    continue;
                nodeTransform.GetComponent<NodeLoader>().Load();
            }

        }
    }

    public class NodeLoader : MonoBehaviour
    {
        public Node node;

        /// 
        // void FixedUpdate()
        // {
        //     Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        //     if (rb != null)
        //         rb.velocity = Vector3.zero;
        // }

        /// Add a collider based on the type and id of a gameobject
        /// 'Room' Architecture -> BoxCollider
        /// 'Object' pose -> BoxCollider/NonConvexMeshCollider
        /// 'Object' non-pose -> MeshCollider
        private static void AddCollider(GameObject obj, string type, string modelId)
        {
            if (type == "Object")
            {
                if (Array.IndexOf(Config.POSE_IDS, modelId) > -1)
                {
                    // Use MeshCollider when we have part pose models
                    var collider = obj.AddComponent<MeshCollider>();
                    collider.convex = true;

                    // var collider = obj.AddComponent<BoxCollider>();

                    // use NonConvexMeshCollider (can access in the asset store) 
                    // to get more accurate collider for the whole pose

                    // NonConvexMeshCollider collider = obj.AddComponent<NonConvexMeshCollider>();
                    // collider.avoidExceedingMesh = true;
                    // collider.Calculate();
                }
                else
                {
                    var collider = obj.AddComponent<MeshCollider>();
                    
                    // Make the surface slipery ... the pose maybe slip down
                    PhysicMaterial material = new PhysicMaterial();
                    material.dynamicFriction = 0;
                    // material.staticFriction = 0;
                    // material.bounciness = 0.2f;
                    collider.material = material;
                }
            }
            else
            {
                obj.AddComponent<BoxCollider>();
            }

        }

        /// Load external defined materials
        private static UnityEngine.Material LoadMaterial(Material material)
        {
            UnityEngine.Material mat = new UnityEngine.Material(Shader.Find("Standard")) { name = material.name };
            //UnityEngine.Material mat = new UnityEngine.Material(Shader.Find("Unlit/NewUnlitShader")) { name = material.name };
            if (material.diffuse != null)
            {
                Color c;
                ColorUtility.TryParseHtmlString(material.diffuse, out c);
                mat.color = c;
            }
            if (material.texture != null)
            {
                string texturePath = Config.SUNCG_HOME + "texture/" + material.texture + ".jpg";
                Texture tex = ImageLoader.LoadTexture(texturePath);
                mat.mainTexture = tex;
            }
            return mat;
        }

        /// Load the object mesh for this gameobject
        private void LoadMesh()
        {
            string pathToObj = Config.SUNCG_HOME;
            if (Config.USE_PART_POSE && Array.IndexOf(Config.POSE_IDS, node.modelId) > -1)
                pathToObj = Config.PART_POSE_HOME + "object/" + node.modelId + "/" + node.modelId + ".parts.obj"; // Pose object with part groups
            else
                pathToObj += "object/" + node.modelId + "/" + node.modelId + ".obj";

            if (File.Exists(pathToObj))
            {
                // Load the mesh with an empty gameobject as parent
                GameObject loadedObject = new OBJLoader().Load(pathToObj, Shader.Find("Standard"));
                List<Transform> children = TransformUtils.GetChildrenList(loadedObject.transform);
                LevelLoader ll = transform.parent.parent.GetComponent<LevelLoader>();
                int levelId = -1;
                if (ll != null) {
                    levelId = Int32.Parse(ll.level.id);
                }
                foreach (Transform child in children)
                {
                    // Don't Add colliders for every part of the mesh
                    //AddCollider(child.gameObject, "Object", node.modelId);
                    if (levelId != -1)
                    {
                        child.gameObject.layer = 8;
                    }

                        // Load external materials
                    if (node.materials.Length > 0)
                    {
                        MeshRenderer mr = child.GetComponent<MeshRenderer>();
                        UnityEngine.Material[] materials = new UnityEngine.Material[Math.Max(node.materials.Length, mr.materials.Length)];
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (i < node.materials.Length &&
                            (node.materials[i].diffuse != null || node.materials[i].texture != null))
                                materials[i] = LoadMaterial(node.materials[i]);
                            else
                                materials[i] = mr.materials[i];
                        }
                        mr.materials = materials;
                    }

                    // Change the parent of the mesh to this gameobject
                    child.parent = transform;
                }
                Destroy(loadedObject);

                // Apply the transform of the node
                if (node.transform != null)
                {
                    Matrix4x4 matrix = TransformUtils.Array2Matrix4x4(node.transform);
                    TransformUtils.SetTransformFromMatrix(transform, ref matrix);
                }

                /* Don't Add rigid body to this gameobject and fix the rotation
                Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
                rigid.isKinematic = true;
                rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;*/
            }
            else
            {
                Debug.Log("Can't find object " + pathToObj);
            }
        }

        /// Load the room mesh for this room gameobject
        private void LoadRoomMesh(string type, string suffix)
        {
            string houseId = transform.root.GetComponent<HouseLoader>().house.id;
            string pathToObj = Config.SUNCG_HOME + "room/" + houseId + "/" + node.modelId + suffix + ".obj";
            if (File.Exists(pathToObj))
            {
                GameObject roomObject = new OBJLoader().Load(pathToObj, Shader.Find("Standard"));
                roomObject.name = type + "#" + node.id;
                roomObject.transform.parent = transform;
                List<Transform> children = TransformUtils.GetChildrenList(roomObject.transform);
                // Ground: the gameobject has created by GetNodeObject()
                // W/F/C: add the empty parent gameobject to this room
                int levelId = Int32.Parse(transform.parent.GetComponent<LevelLoader>().level.id);
                foreach (Transform child in children)
                {
                    //AddCollider(child.gameObject, type, node.modelId);

                    /*if (type == "Ground" || type == "Floor")
                    {
                        AddCollider(child.gameObject, type, node.modelId);
                    }*/
                    if(type != "Ground")
                        child.gameObject.layer = 8;
                    child.gameObject.transform.localScale = new Vector3(-1, 1, 1);
                    //if (type == "Wall") //This code makes the tops of the walls unwalkable, at the expense of the agent not being able to not pass through them.
                    //{
                    //    NavMeshModifier navMeshModifier = child.gameObject.AddComponent<NavMeshModifier>();
                    //    navMeshModifier.overrideArea = true;
                    //    navMeshModifier.area = 1;
                    //}
                    child.parent = (type == "Ground" ? transform : roomObject.transform);
                }

                // Apply the transform of the node
                if (node.transform != null)
                {
                    Matrix4x4 matrix = TransformUtils.Array2Matrix4x4(node.transform);
                    TransformUtils.SetTransformFromMatrix(transform, ref matrix);
                }
            }
        }


        public void Load()
        {
            if (node.type == "Room")
            {
                List<Transform> children = TransformUtils.GetChildrenList(transform);
                foreach (Transform child in children)
                {
                    child.GetComponent<NodeLoader>().Load();
                }

                if (Config.SHOW_WALL)
                    LoadRoomMesh("Wall", "w");
                if (Config.SHOW_FLOOR)
                    LoadRoomMesh("Floor", "f");
                if (Config.SHOW_CEILING)
                    LoadRoomMesh("Ceiling", "c");
            }
            else if (node.type == "Ground")
                LoadRoomMesh("Ground", "f");
            else if (node.type == "Object")
                LoadMesh();
        }


    }





}
