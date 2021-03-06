﻿using Terra;
using Terra.Structure;
using UnityEngine;

[ExecuteInEditMode]
public class LandTex : MonoBehaviour {
	public float TexScale = 1f;

	public Texture2D Control;

	public Texture2D Tex0;
	public Texture2D Norm0;
	public float Smooth0;

	public Texture2D Tex1;
	public Texture2D Norm1;

	Material m;

	void Start() {
		TerraConfig t = FindObjectOfType<TerraConfig>();
		//m = t.CustomMaterial;
	}

	// Update is called once per frame
	void Update () {
		if (Control != null) {
			m.SetTexture("_Control", Control);
			
			SetTex("_Splat0", Tex0);
			SetTex("_Normal0", Norm0);

			SetTex("_Splat1", Tex1);
			SetTex("_Normal1", Norm1);
		}
	}

	void SetTex(string name, Texture2D tex) {
		m.SetTexture(name, tex);
		m.SetTextureScale(name, new Vector2(TexScale, TexScale));
	}
}
