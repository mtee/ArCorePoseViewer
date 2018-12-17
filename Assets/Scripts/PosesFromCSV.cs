using System.Collections;
using System.Collections.Generic;
using System;

using System.IO;
using UnityEngine;

public class PosesFromCSV : MonoBehaviour {

	public string dataFolder;
	public string csvFile;
	static private int curNodeId = 0;
	private List<Node> nodes;
	public Transform globalTransform;
	public Transform m_cameraTransform;
	public MeshRenderer imgPlane;
	public TextMesh frameText;

	private Vector3 hCompensation = new Vector3(0, 0, 0);
	private Vector2 size = new Vector2(1280, 720); 
	private Texture2D tex;
	string path = "Assets/Resources/text.txt";
	Texture2D texture;
	private bool arcoreCS = true;
	// Use this for initialization
	void Start () {

		nodes = new List<Node> ();

		// read in poses and frame ids

		String fileContents = System.IO.File.ReadAllText (dataFolder + '/' + csvFile);
		String [] lines = fileContents.Split('\n');
		texture = new Texture2D(740, 360, TextureFormat.RGB24, false);
		texture.filterMode = FilterMode.Trilinear;
		int idCounter = 0;
		int transformOffset = 2;
	//	StreamWriter writer = new StreamWriter(path, true);
		Debug.Log("found "  + lines.Length + " poses. Parsing...");
		foreach (string line in lines) {
			String[] lineData = (line.Trim()).Split(' ');
			if (lineData.Length == 18) {
				arcoreCS = false;
				Node n = new Node ();
				n.id = idCounter;
				float m11, m12, m13, m14;
				float m21, m22, m23, m24;
				float m31, m32, m33, m34;
				float m41, m42, m43, m44;
				int frameId = 0;
				int.TryParse (lineData [0], out frameId);
				int inlierCount = 0;
				int.TryParse (lineData [1], out inlierCount);


				float.TryParse (lineData [transformOffset + 0], out m11);
				float.TryParse (lineData [transformOffset + 1], out m12);
				float.TryParse (lineData [transformOffset + 2], out m13);
				float.TryParse (lineData [transformOffset + 3], out m14);



				float.TryParse (lineData [transformOffset + 4], out m21);
				float.TryParse (lineData [transformOffset + 5], out m22);
				float.TryParse (lineData [transformOffset + 6], out m23);
				float.TryParse (lineData [transformOffset + 7], out m24);

				float.TryParse (lineData [transformOffset + 8], out m31);
				float.TryParse (lineData [transformOffset + 9], out m32);
				float.TryParse (lineData [transformOffset + 10], out m33);
				float.TryParse (lineData [transformOffset + 11], out m34);

				m41 = 0;
				m42 = 0;
				m43 = 0;
				m44 = 1;

				Vector4 col1 = new Vector4 (m11, m21, m31, m41);
				Vector4 col2 = new Vector4 (m12, m22, m32, m42);
				Vector4 col3 = new Vector4 (m13, m23, m33, m43);
				Vector4 col4 = new Vector4 (m14, m24, m34, m44);


				Matrix4x4 transform = new Matrix4x4 (col1, col2, col3, col4);

				transform = transform.inverse;
				globalTransform.rotation = Quaternion.Euler(-90, 0, 0);
				globalTransform.localScale = new Vector3 (-1, 1, 1);

				n.position.Set (transform [0, 3], transform [1, 3], transform [2, 3]);  // !switching Z and Y 
				n.quat = new Quaternion (transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w); // flipping z

//				n.position.Set( x, y, z);
				n.frameId = frameId;
				n.imgPath = dataFolder + '/' + "frames" + '/' + frameId + ".png";
				nodes.Add (n);
			} else if (lineData.Length == 8) {
				arcoreCS = true;
				Node n = new Node ();
				int frameId = 0;
				int.TryParse (lineData [0], out frameId);
				float x = 0;
				float.TryParse (lineData [1], out x);
				float y = 0;
				float.TryParse (lineData [2], out y);
				float z = 0;
				float.TryParse (lineData [3], out z);
				float qx = 0;
				float.TryParse (lineData [4], out qx);
				float qy = 0;
				float.TryParse (lineData [5], out qy);
				float qz = 0;
				float.TryParse (lineData [6], out qz);
				float qw = 0;
				float.TryParse (lineData [7], out qw);
				n.position.Set (x, y, z);
				n.quat = new Quaternion (qx, qy, qz, qw);
				frameId = frameId + 1;
				n.frameId = frameId;
				n.imgPath = dataFolder + '/' + "frames" + '/' + n.frameId + ".png";
				nodes.Add (n);
			} else {
				Debug.LogWarning("Unknown poses file format!");
			}
		}
	//	writer.Close ();
	//	float.TryParse(lineData[0], x);
	}
	
	// Update is called once per frame
	void Update () {
		if (nodes.Count > 0) {
			//Vector3 curPos = form.position;
			frameText.text = "frame #" + nodes [curNodeId].frameId;

			// converting coordinate systems from ArCore to Unity
			Vector3 e = nodes [curNodeId].quat.eulerAngles;
			Vector3 p = nodes [curNodeId].position;
			if (arcoreCS) { 
				//			var tmp = e.z;
				//			e.z = e.y;
				//			e.y = tmp;
				//	e.z = -e.z;
				e.x = -e.x;
				e.y = -e.y;


				//		p.x = -p.x;
				//		tmp = p.z;
				p.z = -p.z;
				//		p.y = tmp;
				globalTransform.rotation = Quaternion.Euler(-90, 0, 90) * Quaternion.Euler (e);
				globalTransform.position = p;  
			} else {
				m_cameraTransform.localRotation = Quaternion.Euler (e);
				m_cameraTransform.Rotate (0, 0, 180, Space.Self);
				m_cameraTransform.localPosition = p;
			}



			//m_cameraTransform.rotation = Quaternion.Euler(90, 0, 0) * Quaternion.Euler (e);
			//m_cameraTransform.rotation = ConvertToUnity(nodes [curNodeId].quat);


			//m_cameraTransform.rotation = nodes [curNodeId].quat;
			//m_cameraTransform.position = p;  

			//m_cameraTransform.localRotation = nodes [curNodeId].quat;



//			if (!arcoreCS) {
//				
//			} else {
//				m_cameraTransform.rotation = Quaternion.Euler(0, 180, 0) * Quaternion.Euler (e);
//			}

//			
		//	m_cameraTransform.localScale = new Vector3(1, 1, 1);  


			loadImage(size, Path.GetFullPath(nodes [curNodeId].imgPath));
			imgPlane.material.mainTexture = texture;


			if (curNodeId < nodes.Count - 1) {
				curNodeId += 1;
			}
			else {
				Debug.Log ("Resetting index");
				curNodeId = 0;
			}

		}
	}

	private void loadImage(Vector2 size, string filePath) {

		byte[] bytes = File.ReadAllBytes(filePath);

		texture.LoadImage(bytes);

		return;
	}


Quaternion ConvertToUnity(Quaternion input) {
	return new Quaternion(
		input.y,   // -(  right = -left  )
		-input.z,   // -(     up =  up     )
		-input.x,   // -(forward =  forward)
		input.w
	);
}

}
public class Node  {

	public int id { get; set; }
	public int frameId { get; set; }
	public Quaternion quat;
	public Vector3 position;
	public string imgPath;

	public override string ToString ()
	{
		//	return string.Format ("[Node: id={0},  pose1={1},", id, transform[0, 0]);
		return string.Format ("[Node: frameId={0}, pose1={1} {2} {3}]", frameId, position.x, position.y, position.z);
	}




}