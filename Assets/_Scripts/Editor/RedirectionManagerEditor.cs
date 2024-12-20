﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using HR_Toolkit;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RedirectionManager))]
[CanEditMultipleObjects]
public class RedirectionManagerEditor : Editor
{
    
    private SerializedProperty _movement;
    private RedirectionManager _redirectionManager;

    private SerializedProperty _virtualWorld;
    private SerializedProperty _realHand;
    private SerializedProperty _virtualHand;
    private SerializedProperty _warpOrigin;
    private SerializedProperty _body;
    private SerializedProperty _allRedirectedPrefabs;

    private SerializedProperty _redirectionTechnique;

    private SerializedProperty _isRandomTarget;
    private SerializedProperty _isRandomVector;
    private SerializedProperty _isShiftKeypad;
    private SerializedProperty _isHighlightActive;
    private SerializedProperty _isStudySetup;
    private SerializedProperty _isScaleKeypad;
    private SerializedProperty _randomVector;
    private SerializedProperty _maxRedirectionAngle;
    private SerializedProperty _keypad;
    private SerializedProperty _questionnaire;
    private SerializedProperty _showQuestionnaire;
    private SerializedProperty _threshold;
    private SerializedProperty _webcam;
    
    private SerializedProperty _target;
    private SerializedProperty _lastTarget;

    private SerializedProperty _testController;
    private SerializedProperty _pathGenerator;
    private SerializedProperty _logFile;
    

    private void OnEnable()
    {
        _movement= serializedObject.FindProperty("movement");
        _virtualWorld = serializedObject.FindProperty("virtualWorld");
        _realHand = serializedObject.FindProperty("realHand");
        _virtualHand = serializedObject.FindProperty("virtualHand");
        _warpOrigin = serializedObject.FindProperty("warpOrigin");
        _body = serializedObject.FindProperty("body");
        _allRedirectedPrefabs = serializedObject.FindProperty("allRedirectedPrefabs");

        _redirectionTechnique = serializedObject.FindProperty("redirectionTechnique");
        
        _isRandomTarget = serializedObject.FindProperty("isRandomTarget");
        _isRandomVector = serializedObject.FindProperty("isRandomVector");
        _isShiftKeypad = serializedObject.FindProperty("isShiftKeypad");
        _isHighlightActive = serializedObject.FindProperty("isHighlightActive");
        _isStudySetup = serializedObject.FindProperty("isStudySetup");
        _isScaleKeypad = serializedObject.FindProperty("isScaleKeypad");
        _randomVector = serializedObject.FindProperty("randomVector");
        _maxRedirectionAngle = serializedObject.FindProperty("maxRedirectionAngle");
        _keypad = serializedObject.FindProperty("keypad");
        _questionnaire = serializedObject.FindProperty("questionnaire");
        _showQuestionnaire = serializedObject.FindProperty("showQuestionnaire");
        _threshold = serializedObject.FindProperty("threshold");
        _webcam = serializedObject.FindProperty("webcam");
        
        _target = serializedObject.FindProperty("target");
        _lastTarget = serializedObject.FindProperty("lastTarget");

        _testController = serializedObject.FindProperty("testControllers");
        _pathGenerator = serializedObject.FindProperty("pathGenerator");
        _logFile = serializedObject.FindProperty("logFile");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        _redirectionManager = (RedirectionManager) target;

        EditorGUILayout.PropertyField(_virtualWorld, new GUIContent("Virtual World"));
        EditorGUILayout.PropertyField(_realHand, new GUIContent("Real Hand"));
        EditorGUILayout.PropertyField(_virtualHand, new GUIContent("Virtual Hand"));
        EditorGUILayout.PropertyField(_warpOrigin, new GUIContent("Warp Origin"));
        EditorGUILayout.PropertyField(_body, new GUIContent("Body"));
        EditorGUILayout.PropertyField(_keypad, new GUIContent("Keypad"));
        EditorGUILayout.PropertyField(_questionnaire, new GUIContent("Questionnaire"));
        EditorGUILayout.PropertyField(_webcam, new GUIContent("Webcam"));
        EditorGUILayout.PropertyField(_showQuestionnaire, new GUIContent("Show Questionnaire"));
        EditorGUILayout.PropertyField(_isRandomTarget, new GUIContent("Random Targets"));
        EditorGUILayout.PropertyField(_isRandomVector, new GUIContent("Random Vector"));
        EditorGUILayout.PropertyField(_isShiftKeypad, new GUIContent("Shift Keypad"));
        EditorGUILayout.PropertyField(_isHighlightActive, new GUIContent("Highlight Targets"));
        EditorGUILayout.PropertyField(_isStudySetup, new GUIContent("Study Setup"));
        EditorGUILayout.PropertyField(_isScaleKeypad, new GUIContent("Scale Keypad"));
        EditorGUILayout.PropertyField(_randomVector, new GUIContent("Random Vector"));
        EditorGUILayout.Slider(_maxRedirectionAngle, 0f, 90f, "Max Redirection Angle");
        EditorGUILayout.Slider(_threshold, 0f, 1f, "Threshold");
        EditorGUILayout.LabelField("Movement Options", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField(_movement, new GUIContent("Movement"));

        DefineRedirectedPrefabSection();
        DefineRedirectionSection();
        //DefineThresholdControllerSection();

        //EditorGUILayout.PropertyField(_target, new GUIContent("Current Target"));
        //EditorGUILayout.PropertyField(_lastTarget, new GUIContent("Last Target"));
        
        
        //DefineAnalysisSection();
        
        /*if (GUILayout.Button("Test"))
        {
            Debug.Log("We pressed a button!");
        }*/

        serializedObject.ApplyModifiedProperties();
        DefineMovement();
    }
    
    private void DefineMovement()
    {
        _redirectionManager.movementController = _redirectionManager.GetComponent<MovementController>();
        if (_redirectionManager.movementController == null)
        {
            _redirectionManager.gameObject.AddComponent<MovementController>();
            _redirectionManager.movementController = _redirectionManager.GetComponent<MovementController>();
        }

        // Mouse Movement
        if (_redirectionManager.movement == MovementController.Movement.Mouse)
        {
            _redirectionManager.speed = EditorGUILayout.Slider("Speed", _redirectionManager.speed, 1f, 20f);
            _redirectionManager.mouseWheelSpeed = EditorGUILayout.Slider("Mouse Wheel Speed", _redirectionManager.mouseWheelSpeed, 1f, 20f);
        }
    }

    private void DefineRedirectedPrefabSection()
    {
        EditorGUILayout.LabelField("Redirected Prefab Options", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField(_allRedirectedPrefabs, new GUIContent("All Redirected Prefabs"));
    }

    private void DefineRedirectionSection()
    {
        EditorGUILayout.LabelField("Redirection Technique Options", EditorStyles.whiteLargeLabel);
        EditorGUILayout.PropertyField(_redirectionTechnique, new GUIContent("Default Redirection Technique"));
        
    }

    /*private void DefineThresholdControllerSection()
    {
        EditorGUILayout.LabelField("Threshold Controller Options (WIP)", EditorStyles.boldLabel);
    }

    private void DefineAnalysisSection()
    {
        EditorGUILayout.LabelField("Test & Analysis Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_testController, new GUIContent("Test Controllers (WIP)"));
        EditorGUILayout.PropertyField(_pathGenerator, new GUIContent("Path Generator (WIP)"));
        EditorGUILayout.PropertyField(_logFile, new GUIContent("Log File"));
    }*/
}
