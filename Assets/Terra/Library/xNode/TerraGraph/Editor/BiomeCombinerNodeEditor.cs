﻿using Terra.Graph;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(BiomeCombinerNode))]
	public class BiomeCombinerNodeEditor: NodeEditor {
		private BiomeCombinerNode Bcn {
			get {
				return (BiomeCombinerNode)target;
			}
		}

		public override void OnBodyGUI() {
			//Output
			NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Output"));

		//	if (Event.current.type != EventType.MouseUp) {
				//Draw mix method enum
				SerializedProperty mixType = serializedObject.FindProperty("Mix");
				NodeEditorGUILayout.PropertyField(mixType, new GUIContent("Mix Type"));

				//Draw Instance Ports with colors
				NodePort[] ports = Bcn.GetInstanceInputs();

				foreach (NodePort p in ports) {
					EditorGUILayout.BeginHorizontal();
					NodeEditorGUILayout.PortField(p, GUILayout.ExpandWidth(false));

					BiomeNode node = p.GetInputValue<BiomeNode>();
					if (node != null) {
						EditorGUILayout.ColorField(GUIContent.none, node.PreviewColor, false, false, false, null, GUILayout.MaxWidth(32f));
					}

					EditorGUILayout.EndHorizontal();
				}
		//	} 

			//Show Preview
			PreviewField.Show(Bcn);
		}

		public override Color GetTint() {
			return Constants.TintBiome;
		}

		public override string GetTitle() {
			return "Biome Combiner";
		}
	}
}