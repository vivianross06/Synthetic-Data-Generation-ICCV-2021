﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OmniLoader))]
public class OLEditor : Editor
{
	private Type agentType;
	private Type loaderType;
	private Type screenshotType;

    int ListSize;
	bool showList = true;

    public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		OmniLoader ol = (OmniLoader)target;
		SerializedObject GetTarget = new SerializedObject(target);

		if (ol.LoaderScript == null)
			loaderType = null;
		else
			loaderType = ol.LoaderScript.GetClass();
		EditorGUI.BeginChangeCheck();
		ol.LoaderScript = (MonoScript)EditorGUILayout.ObjectField("Loader", ol.LoaderScript, typeof(MonoScript), false);
		if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
		{
			if (ol.LoaderScript == null || loaderType != ol.LoaderScript.GetClass())
			{
				if (ol.LoaderScript != null)
				{
					ol.gameObject.AddComponent(ol.LoaderScript.GetClass());
				}
				else
				{
					if (loaderType != null)
						DestroyImmediate(ol.gameObject.GetComponent(loaderType));
				}
				if (loaderType != null && ol.LoaderScript != null)
					DestroyImmediate(ol.gameObject.GetComponent(loaderType));
			}
		}
		EditorGUILayout.PropertyField(GetTarget.FindProperty("loadAll"));

		if (ol.AgentScript == null)
			agentType = null;
		else
			agentType = ol.AgentScript.GetClass();
		//EditorGUI.BeginChangeCheck();
		ol.AgentScript = (MonoScript)EditorGUILayout.ObjectField("Agent", ol.AgentScript, typeof(MonoScript), false);
		/*if (EditorGUI.EndChangeCheck())
		{
			//Code that runs when a script is selected.
			if (ol.AgentScript == null || agentType != ol.AgentScript.GetClass())
			{
				if (ol.AgentScript != null)
				{
					ol.gameObject.AddComponent(ol.AgentScript.GetClass());
				}
				else
				{
					if(agentType != null)
						DestroyImmediate(ol.gameObject.GetComponent(agentType));
				}
				if (agentType != null && ol.AgentScript != null)
					DestroyImmediate(ol.gameObject.GetComponent(agentType));
			}
		}*/
		EditorGUILayout.PropertyField(GetTarget.FindProperty("flythroughMode"));
		EditorGUILayout.PropertyField(GetTarget.FindProperty("agentWaypoints"));
		EditorGUILayout.PropertyField(GetTarget.FindProperty("stepDistance"));
		
		if (ol.ScreenshotScript == null)
			screenshotType = null;
		else
			screenshotType = ol.ScreenshotScript.GetClass();
		EditorGUI.BeginChangeCheck();
		ol.ScreenshotScript = (MonoScript)EditorGUILayout.ObjectField("Screenshot Script", ol.ScreenshotScript, typeof(MonoScript), false);
		if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
		{
			if (ol.ScreenshotScript == null || screenshotType != ol.ScreenshotScript.GetClass())
			{
				if (ol.ScreenshotScript != null)
				{
					ol.gameObject.AddComponent(ol.ScreenshotScript.GetClass());
				}
				else
				{
					if (screenshotType != null)
						DestroyImmediate(ol.gameObject.GetComponent(screenshotType));
				}
				if (screenshotType != null && ol.ScreenshotScript != null)
					DestroyImmediate(ol.gameObject.GetComponent(screenshotType));
			}
		}
		if (GUILayout.Button("Take Screenshot"))
		{
			Screenshoter[] components = GameObject.FindObjectsOfType<Screenshoter> ();
			if (components.Length == 0)
			{
				Screenshoter sc = (Screenshoter)Camera.main.gameObject.AddComponent(ol.ScreenshotScript.GetClass());
				sc.CaptureScreenshot(Camera.main, 1710, 840);
				DestroyImmediate(sc);

			}
			else
			{
				Screenshoter sc = components[0];
				sc.CaptureScreenshot(Camera.main, 1710, 840);
			}
		}

		//List code starts after here.
		//Code for custom class list in inspector originially from user ForceX of Unity Fourm.
		showList = EditorGUILayout.Foldout(showList, "Screenshot Properties");
		if(showList)
		{
		SerializedProperty ThisList;
        //SerializedObject GetTarget = new SerializedObject(target); I put this line earlier.
        ThisList = GetTarget.FindProperty("scs");
		//List<ScreenShotType> ThisList = ol.scs;
		//EditorGUILayout.Space();
		//EditorGUILayout.Space();
		//EditorGUILayout.Space();

		//Resize our list
		EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Define the list size with a number");
        ListSize = ThisList.arraySize;
        ListSize = EditorGUILayout.IntField("List Size", ListSize);

        if (ListSize != ThisList.arraySize)
        {
            while (ListSize > ThisList.arraySize)
            {
                ThisList.InsertArrayElementAtIndex(ThisList.arraySize);
            }
            while (ListSize < ThisList.arraySize)
            {
                ThisList.DeleteArrayElementAtIndex(ThisList.arraySize - 1);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Or");
        //EditorGUILayout.Space();
        EditorGUILayout.Space();

        //Or add a new item to the List<> with a button
        EditorGUILayout.LabelField("Add a new item with a button");

        if (GUILayout.Button("Add New"))
        {
            ScreenShotType sct = new ScreenShotType();
            sct.shader = null;
            sct.directoryName = "";
            sct.formatType = FormatEnum.RGB;
            ol.scs.Add(sct);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //Display our list to the inspector window

        for (int i = 0; i < ThisList.arraySize; i++)
        {
            SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
            SerializedProperty MyInt = MyListRef.FindPropertyRelative("shader");
            SerializedProperty MyFloat = MyListRef.FindPropertyRelative("directoryName");
            SerializedProperty MyVect3 = MyListRef.FindPropertyRelative("formatType");
			//SerializedProperty MyGO = MyListRef.FindPropertyRelative("AnGO");


			// Display the property fields in two ways.

			// Choose to display automatic or custom field types. This is only for example to help display automatic and custom fields.
			//1. Automatic, No customization <-- Choose me I'm automatic and easy to setup
			//EditorGUILayout.LabelField("Automatic Field By Property Type");
			//EditorGUILayout.PropertyField(MyGO);
			EditorGUILayout.LabelField("Screenshot #"+i+" Properties");
			EditorGUILayout.PropertyField(MyInt);
            EditorGUILayout.PropertyField(MyFloat);
            EditorGUILayout.PropertyField(MyVect3);
            EditorGUILayout.Space();

            //Remove this index from the List
            EditorGUILayout.LabelField("Remove an index from the List<> with a button");
            if (GUILayout.Button("Remove This Index (" + i.ToString() + ")"))
            {
                ThisList.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

		//Apply the changes to our list
		GetTarget.ApplyModifiedProperties();
		OL_GLOBAL_INFO.SCREENSHOT_PROPERTIES = ol.scs;
		}
	}
}