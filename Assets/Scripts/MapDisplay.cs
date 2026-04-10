using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public MeshFilter chunkMeshFilter;
	public MeshRenderer chunkMeshRenderer;

	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
	}

	public void DrawMesh(MeshData meshData, Texture2D texture) {
		meshFilter.sharedMesh = meshData.CreateMesh();
		meshRenderer.sharedMaterial.mainTexture = texture;
	}

	public void DrawChunkMesh(MeshData meshData, Texture2D texture) {
		chunkMeshFilter.sharedMesh = meshData.CreateMesh();
		chunkMeshRenderer.sharedMaterial.mainTexture = texture;
	}
}