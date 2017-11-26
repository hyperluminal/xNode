﻿using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    /// <summary> UNEC-specific version of <see cref="EditorGUILayout"/> </summary>
    public static class NodeEditorGUILayout {

        /// <summary> Make a field for a serialized property. Automatically displays relevant node port. </summary>
        public static void PropertyField(SerializedProperty property, bool includeChildren = true, params GUILayoutOption[] options) {
            PropertyField(property, (GUIContent) null, includeChildren, options);
        }

        /// <summary> Make a field for a serialized property. Automatically displays relevant node port. </summary>
        public static void PropertyField(SerializedProperty property, GUIContent label, bool includeChildren = true, params GUILayoutOption[] options) {
            if (property == null) throw new NullReferenceException();
            Node node = property.serializedObject.targetObject as Node;
            NodePort port = node.GetPort(property.name);
            PropertyField(property, label, port, includeChildren);
        }

        /// <summary> Make a field for a serialized property. Manual node port override. </summary>
        public static void PropertyField(SerializedProperty property, NodePort port, bool includeChildren = true, params GUILayoutOption[] options) {
            PropertyField(property, null, port, includeChildren, options);
        }

        /// <summary> Make a field for a serialized property. Manual node port override. </summary>
        public static void PropertyField(SerializedProperty property, GUIContent label, NodePort port, bool includeChildren = true, params GUILayoutOption[] options) {
            if (property == null) throw new NullReferenceException();

            // If property is not a port, display a regular property field
            if (port == null) EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
            else {
                Rect rect = new Rect();

                // If property is an input, display a regular property field and put a port handle on the left side
                if (port.direction == NodePort.IO.Input) {
                    // Get data from [Input] attribute
                    Node.ShowBackingValue showBacking = Node.ShowBackingValue.Unconnected;
                    Node.InputAttribute inputAttribute;
                    if (NodeEditorUtilities.GetAttrib(port.node.GetType(), property.name, out inputAttribute)) showBacking = inputAttribute.backingValue;

                    switch (showBacking) {
                        case Node.ShowBackingValue.Unconnected:
                            // Display a label if port is connected
                            if (port.IsConnected) EditorGUILayout.LabelField(label != null ? label : new GUIContent(property.displayName));
                            // Display an editable property field if port is not connected
                            else EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                            break;
                        case Node.ShowBackingValue.Never:
                            // Display a label
                            EditorGUILayout.LabelField(label != null ? label : new GUIContent(property.displayName));
                            break;
                        case Node.ShowBackingValue.Always:
                            // Display an editable property field
                            EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                            break;
                    }

                    rect = GUILayoutUtility.GetLastRect();
                    rect.position = rect.position - new Vector2(16, 0);
                    // If property is an output, display a text label and put a port handle on the right side
                } else if (port.direction == NodePort.IO.Output) {
                    // Get data from [Output] attribute
                    Node.ShowBackingValue showBacking = Node.ShowBackingValue.Unconnected;
                    Node.OutputAttribute outputAttribute;
                    if (NodeEditorUtilities.GetAttrib(port.node.GetType(), property.name, out outputAttribute)) showBacking = outputAttribute.backingValue;

                    switch (showBacking) {
                        case Node.ShowBackingValue.Unconnected:
                            // Display a label if port is connected
                            if (port.IsConnected) EditorGUILayout.LabelField(label != null ? label : new GUIContent(property.displayName), NodeEditorResources.styles.outputPort, GUILayout.MinWidth(30));
                            // Display an editable property field if port is not connected
                            else EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                            break;
                        case Node.ShowBackingValue.Never:
                            // Display a label
                            EditorGUILayout.LabelField(label != null ? label : new GUIContent(property.displayName), NodeEditorResources.styles.outputPort, GUILayout.MinWidth(30));
                            break;
                        case Node.ShowBackingValue.Always:
                            // Display an editable property field
                            EditorGUILayout.PropertyField(property, label, includeChildren, GUILayout.MinWidth(30));
                            break;
                    }

                    rect = GUILayoutUtility.GetLastRect();
                    rect.position = rect.position + new Vector2(rect.width, 0);
                }

                rect.size = new Vector2(16, 16);

                Color backgroundColor = new Color32(90, 97, 105, 255);
                if (NodeEditorWindow.nodeTint.ContainsKey(port.node.GetType())) backgroundColor *= NodeEditorWindow.nodeTint[port.node.GetType()];
                DrawPortHandle(rect, port.ValueType, backgroundColor);

                // Register the handle position
                Vector2 portPos = rect.center;
                if (NodeEditor.portPositions.ContainsKey(port)) NodeEditor.portPositions[port] = portPos;
                else NodeEditor.portPositions.Add(port, portPos);
            }
        }

        /// <summary> Make a simple port field. </summary>
        public static void PortField(NodePort port, params GUILayoutOption[] options) {
            PortField(null, port, options);
        }

        /// <summary> Make a simple port field. </summary>
        public static void PortField(GUIContent label, NodePort port, params GUILayoutOption[] options) {
            if (port == null) return;
            if (label == null) EditorGUILayout.LabelField(port.fieldName.PrettifyCamelCase(), options);
            else EditorGUILayout.LabelField(label, options);
            Rect rect = GUILayoutUtility.GetLastRect();
            if (port.direction == NodePort.IO.Input) rect.position = rect.position - new Vector2(16, 0);
            else if (port.direction == NodePort.IO.Output) rect.position = rect.position + new Vector2(rect.width, 0);
            rect.size = new Vector2(16, 16);

            Color backgroundColor = new Color32(90, 97, 105, 255);
            if (NodeEditorWindow.nodeTint.ContainsKey(port.node.GetType())) backgroundColor *= NodeEditorWindow.nodeTint[port.node.GetType()];
            DrawPortHandle(rect, port.ValueType, backgroundColor);

            // Register the handle position
            Vector2 portPos = rect.center;
            if (NodeEditor.portPositions.ContainsKey(port)) NodeEditor.portPositions[port] = portPos;
            else NodeEditor.portPositions.Add(port, portPos);
        }

        private static void DrawPortHandle(Rect rect, Type type, Color backgroundColor) {
            Color col = GUI.color;
            GUI.color = backgroundColor;
            GUI.DrawTexture(rect, NodeEditorResources.dotOuter);
            GUI.color = NodeEditorPreferences.GetTypeColor(type);
            GUI.DrawTexture(rect, NodeEditorResources.dot);
            GUI.color = col;
        }
    }
}