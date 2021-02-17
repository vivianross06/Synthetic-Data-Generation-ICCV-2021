using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OmniLoaderUnity;

namespace UnitySUNCG
{
    public class Config
    {
        //public static string SUNCG_HOME = Application.dataPath + "/../../SUNCG/suncg/"; // Todo: set the path to SUNCG
        public static string SUNCG_HOME = OmniLoaderUnity.Config.SUNCG_HOME;
        public static string PART_POSE_HOME = Application.dataPath + "/Scripts/Loaders/UnitySUNCG/pose/";
        public static bool SHOW_WALL = true, SHOW_FLOOR = true, SHOW_CEILING = false, USE_PART_POSE = true;
        public static string[] POSE_IDS = {"325","346","324","323","333"};
    }

}
