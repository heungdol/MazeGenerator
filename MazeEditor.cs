using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MazeGenerator))]
public class MazeEditor : Editor {
	public override void OnInspectorGUI () {
		base.OnInspectorGUI ();
		MazeGenerator maze = target as MazeGenerator;

		if (GUILayout.Button ("Generate Maze")) {
			if (maze) {
				maze.StartGenerateMaze ();
			}
		}
	}
}
