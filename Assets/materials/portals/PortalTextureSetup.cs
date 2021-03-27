using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour {

	public Camera cameraA;
	public Camera cameraB;

	public Material portalMatB;
	public Material portalMatA;

	private Vector2 prevScreenSize;

	// Use this for initialization
	void Start () {
		updateTextures();
		prevScreenSize = new Vector2(Screen.width, Screen.height);
	}
	
	void Update() {
		Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		if (screenSize == prevScreenSize) return;
		prevScreenSize = new Vector2(Screen.width, Screen.height);

		updateTextures();
	}

	void updateTextures() {
		print("valus");
		if (cameraA.targetTexture != null) cameraA.targetTexture.Release();
		if (cameraB.targetTexture != null) cameraB.targetTexture.Release();
		
		cameraA.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
		cameraB.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);

		portalMatB.mainTexture = cameraA.targetTexture;
		portalMatA.mainTexture = cameraB.targetTexture;
	}

}