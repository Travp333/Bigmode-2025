using UnityEngine;
using TMPro;
public class WiggleText : MonoBehaviour
{
    public TMP_Text textComponent;
	public float frequency = 2f; public float scale =10f; public float xWobbleScale = .01f;
	private void Start()
	{
		textComponent = this.gameObject.GetComponent<TMP_Text>();
	}
	// Update is called once per frame
	private void FixedUpdate()
	{
		WobbleUpdate();
	}
	void WobbleUpdate()
    {

		textComponent.ForceMeshUpdate();
		var textInfo = textComponent.textInfo;

		for (int i = 0; i < textInfo.characterCount; i++) {
			var charInfo = textInfo.characterInfo[i];

			if (!charInfo.isVisible) {
				continue;
			}

			var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
			for (int j = 0; j < 4; ++j) {
				Vector3 orig = verts[charInfo.vertexIndex + j];
				verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * frequency + orig.x * xWobbleScale) * scale, 0);

			}
		}
		for (int i = 0; i < textInfo.meshInfo.Length; ++i) {
			var meshInfo = textInfo.meshInfo[i];
			meshInfo.mesh.vertices = meshInfo.vertices;
			textComponent.UpdateGeometry(meshInfo.mesh, i);
		}
    }
}
