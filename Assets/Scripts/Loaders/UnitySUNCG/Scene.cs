using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace UnitySUNCG
{
    public class Scene
    {

        /// load house json by path
        public static House GetHouse(string pathToJson)
        {
            // parse the house json
            House house = JsonUtility.FromJson<House>(File.ReadAllText(pathToJson));
            foreach (Level level in house.levels)
            {
                // add room references to level
                foreach (Node node in level.nodes)
                {
                    if (node.type != "Room")
                        continue;
                    level.rooms.Add(node);
                }
            }
            return house;
        }

        /// load house json by id
        public static House GetHouseById(string houseId)
        {
            return GetHouse(Config.SUNCG_HOME + "house/" + houseId + "/house.json");
        }

        /// create house gameobject by house json
        public static GameObject GetHouseObject(House house)
        {
            
            GameObject houseObject = new GameObject("House#" + house.id);
            HouseLoader houseLoader = houseObject.AddComponent<HouseLoader>();
            houseLoader.house = house;
            
            foreach (Level level in house.levels)
            {
                GameObject levelObject = GetLevelObject(house, level);
                levelObject.transform.parent = houseObject.transform;
                //levelObject.AddComponent<BoxCollider>();
            }
            //houseObject.AddComponent<BoxCollider>();
            return houseObject;
        }

        /// create house gameobject by house id
        public static GameObject GetHouseObjectById(string houseId)
        {
            return GetHouseObject(GetHouse(Config.SUNCG_HOME + "house/" + houseId + "/house.json"));
        }

        /// create level gameobject by level json and house json
        public static GameObject GetLevelObject(House house, Level level)
        {
            GameObject levelObject = new GameObject("Level#" + level.id);
            LevelLoader LevelLoader = levelObject.AddComponent<LevelLoader>();
            LevelLoader.level = level;
            // create room gameobjects
            foreach (Node room in level.rooms)
            {
                GameObject roomObject = GetRoomObject(level, room);
                roomObject.transform.parent = levelObject.transform;

            }
            // create other object gameobjects
            foreach (Node node in level.nodes)
            {
                if (node.roomId != null || !node.valid)
                    continue;
                GameObject nodeObject = GetNodeObject(node);
                nodeObject.transform.parent = levelObject.transform;
            }
            return levelObject;
        }

        // create room gameobject by room json and level json
        public static GameObject GetRoomObject(Level level, Node room)
        {
            GameObject roomObject = new GameObject("Room#" + room.id);
            NodeLoader roomLoader = roomObject.AddComponent<NodeLoader>();
            roomLoader.node = room;
            // create room object gameobjects
            try
            {
                foreach (int i in room.nodeIndices)
                {
                    if (level.nodes[i].valid)
                    {
                        GameObject nodeObject = GetNodeObject(level.nodes[i]);
                        nodeObject.transform.parent = roomObject.transform;
                        // set the roomId of an object to avoid loading the mesh twice
                        level.nodes[i].roomId = room.id;
                    }
                }
            }
            catch (Exception e)
			{
                e.ToString();
                Debug.Log("Tried to load invalid room!");
			}
            return roomObject;
        }

        // create house gameobject for a specific room by house json and id
        public static GameObject GetRoomHouseObject(House house, int levelId, int roomId)
        {
            GameObject houseObject = new GameObject("House#" + house.id);
            HouseLoader houseLoader = houseObject.AddComponent<HouseLoader>();
            houseLoader.house = house;

            Level level = house.levels[levelId];
            GameObject levelObject = new GameObject("Level#" + level.id);
            levelObject.transform.parent = houseObject.transform;

            LevelLoader levelLoader = levelObject.AddComponent<LevelLoader>();
            levelLoader.level = level;

            Node room = level.nodes[roomId];
            GameObject roomObject = GetRoomObject(level, room);
            roomObject.transform.parent = levelObject.transform;

            return houseObject;
        }

        // create house gameobject for a specific room by full id
        public static GameObject GetRoomHouseObjectById(string fullId)
        {
            string[] splits = fullId.Split('_');
            string houseId = splits[0];
            int levelId = Int32.Parse(splits[1]);
            int roomId = Int32.Parse(splits[2]);
            House house = GetHouseById(houseId);
            return GetRoomHouseObject(house, levelId, roomId);
        }

        // create object gameobject by node json
        public static GameObject GetNodeObject(Node node)
        {
            GameObject nodeObject = new GameObject(node.type + "#" + node.modelId);
            NodeLoader nodeLoader = nodeObject.AddComponent<NodeLoader>();
            nodeLoader.node = node;
            return nodeObject;
        }
    }
}
