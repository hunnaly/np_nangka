using UnityEngine;

public class mp_particle : MonoBehaviour {

    private float angleZ;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        angleZ += 0.1f;

		this.transform.rotation =
            Quaternion.AngleAxis(angleZ, new Vector3(0.0f, 0.0f, 1.0f));
    }
}
