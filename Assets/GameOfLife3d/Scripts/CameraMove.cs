using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
	GameOfLifeMap GameOfLifeMapObject_;
	GameOfLifeMap GameOfLifeMap_ {
		get {
			if (GameOfLifeMapObject_ == null)
				GameOfLifeMapObject_ = GameObject.FindObjectOfType(typeof(GameOfLifeMap)) as GameOfLifeMap;
			return GameOfLifeMapObject_;
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.A))
			this.transform.RotateAround(Vector3.up, Time.deltaTime);
		if (Input.GetKeyDown(KeyCode.D))
			this.transform.RotateAround(Vector3.up, -Time.deltaTime);
		if (Input.GetKeyDown(KeyCode.W))
			this.transform.RotateAround(this.transform.right, Time.deltaTime);
		if (Input.GetKeyDown(KeyCode.S))
			this.transform.RotateAround(this.transform.right, -Time.deltaTime);
	}

	public void SetCenter(Vector3 Position) {
		this.transform.position = Position;
	}
}
