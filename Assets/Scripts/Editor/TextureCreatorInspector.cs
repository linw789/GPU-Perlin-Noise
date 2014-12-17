using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(TextureCreator))]
public class TextureCreatorInspector : Editor 
{
	TextureCreator m_creator;

	void OnEnable()
	{
		m_creator = (TextureCreator)target;
		Undo.undoRedoPerformed += Refresh;
	}

	void OnDisable()
	{
		Undo.undoRedoPerformed -= Refresh;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if (EditorGUI.EndChangeCheck())
		{
			Refresh();
		}
	}

	void Refresh()
	{
		if (Application.isPlaying)
		{
			m_creator.FillTexture();
		}
	}
}
